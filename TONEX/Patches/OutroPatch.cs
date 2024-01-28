using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using TONEX.Modules;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Templates;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
class EndGamePatch
{
    public static Dictionary<byte, string> SummaryText = new();
    public static string KillLog = "";
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        GameStates.InGame = false;

        Logger.Info("-----------游戏结束-----------", "Phase");
        if (!GameStates.IsModHost) return;
        SummaryText = new();
        foreach (var id in PlayerState.AllPlayerStates.Keys)
            SummaryText[id] = Utils.SummaryTexts(id, false);

        var sb = new StringBuilder(GetString("KillLog"));
        sb.Append("<size=70%>");
        foreach (var kvp in PlayerState.AllPlayerStates.OrderBy(x => x.Value.RealKiller.Item1.Ticks))
        {
            var date = kvp.Value.RealKiller.Item1;
            if (date == DateTime.MinValue) continue;
            var killerId = kvp.Value.GetRealKiller();
            var targetId = kvp.Key;
            sb.Append($"\n{date:T} {Main.AllPlayerNames[targetId]}({Utils.GetTrueRoleName(targetId, false)}{Utils.GetSubRolesText(targetId, summary: true)}) [{Utils.GetVitalText(kvp.Key)}]".RemoveHtmlTags());
            if (killerId != byte.MaxValue && killerId != targetId)
                sb.Append($"\n\t⇐ {Main.AllPlayerNames[killerId]}({Utils.GetTrueRoleName(targetId, false)}{Utils.GetSubRolesText(killerId, summary: true)})".RemoveHtmlTags());
        }
        KillLog = sb.ToString();
        if (!KillLog.Contains('\n')) KillLog = string.Empty;

        Main.NormalOptions.KillCooldown = Options.DefaultKillCooldown;
        //winnerListリセット
        TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
        var winner = new List<PlayerControl>();
        foreach (var pc in Main.AllPlayerControls)
        {
            if (CustomWinnerHolder.WinnerIds.Contains(pc.PlayerId)) winner.Add(pc);
        }
        foreach (var team in CustomWinnerHolder.WinnerRoles)
        {
            winner.AddRange(Main.AllPlayerControls.Where(p => p.Is(team) && !winner.Contains(p)));
        }

        Main.winnerNameList = new();
        Main.winnerList = new();
        foreach (var pc in winner)
        {
            if (CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw && pc.Is(CustomRoles.GM)) continue;

            TempData.winners.Add(new WinningPlayerData(pc.Data));
            Main.winnerList.Add(pc.PlayerId);
            Main.winnerNameList.Add(pc.GetRealName());
        }

        Main.VisibleTasksCount = false;
        if (AmongUsClient.Instance.AmHost)
        {
            Main.RealOptionsData.Restore(GameOptionsManager.Instance.CurrentGameOptions);
            GameOptionsSender.AllSenders.Clear();
            GameOptionsSender.AllSenders.Add(new NormalGameOptionsSender());
            /* Send SyncSettings RPC */
        }
        //オブジェクト破棄
        CustomRoleManager.Dispose();
    }
}
[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
class SetEverythingUpPatch
{
    public static string LastWinsText = "";
    public static string LastWinsReason = "";
    private static TextMeshPro roleSummary;
    private static SimpleButton showHideButton;

    public static void Postfix(EndGameManager __instance)
    {
        if (!Main.playerVersion.ContainsKey(0)) return;
        //#######################################
        //          ==勝利陣営表示==
        //#######################################

        var WinnerTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
        WinnerTextObject.transform.position = new(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
        WinnerTextObject.transform.localScale = new(0.6f, 0.6f, 0.6f);
        var WinnerText = WinnerTextObject.GetComponent<TMPro.TextMeshPro>(); //WinTextと同じ型のコンポーネントを取得
        WinnerText.fontSizeMin = 3f;
        WinnerText.text = "";
        var InEndWinnerText = "";


        string CustomWinnerText = "";
        string EndWinnerText = "";
        var AdditionalWinnerText = new StringBuilder(32);
        var EndAdditionalWinnerText = new StringBuilder(32);
        string CustomWinnerColor = Utils.GetRoleColorCode(CustomRoles.Crewmate);
        string EndWinnerColor = Utils.GetRoleColorCode(CustomRoles.Crewmate);

        var winnerRole = (CustomRoles)CustomWinnerHolder.WinnerTeam;
        if (winnerRole >= 0)
        {
            CustomWinnerText = GetWinnerRoleName(winnerRole, 0);
            EndWinnerText = GetWinnerRoleName(winnerRole, 1);
            CustomWinnerColor = Utils.GetRoleColorCode(winnerRole);
            EndWinnerColor = Utils.GetRoleColorCode(winnerRole);
            if (winnerRole.IsNeutral())
            {
                __instance.BackgroundBar.material.color = Utils.GetRoleColor(winnerRole);
            }
        }
        if (AmongUsClient.Instance.AmHost && PlayerState.GetByPlayerId(0).MainRole == CustomRoles.GM)
        {
            __instance.WinText.text = GetString("GameOver");
            __instance.WinText.color = Utils.GetRoleColor(CustomRoles.GM);
            __instance.BackgroundBar.material.color = Utils.GetRoleColor(CustomRoles.GM);
        }
        switch (CustomWinnerHolder.WinnerTeam)
        {
            //通常勝利
            case CustomWinner.Crewmate:
                CustomWinnerColor = Utils.GetRoleColorCode(CustomRoles.Engineer);
                EndWinnerColor = Utils.GetRoleColorCode(CustomRoles.Engineer);
                break;
            //特殊勝利
            case CustomWinner.Terrorist:
                __instance.Foreground.material.color = Color.red;
                break;
            case CustomWinner.Lovers:
                __instance.BackgroundBar.material.color = Utils.GetRoleColor(CustomRoles.Lovers);
                break;
            //引き分け処理
            case CustomWinner.Draw:
                __instance.WinText.text = GetString("ForceEnd");
                __instance.WinText.color = Color.white;
                __instance.BackgroundBar.material.color = Color.gray;
                WinnerText.text = GetString("ForceEndText");
                WinnerText.color = Color.gray;
                break;
            //全滅
            case CustomWinner.None:
                __instance.WinText.text = "";
                __instance.WinText.color = Color.black;
                __instance.BackgroundBar.material.color = Color.gray;
                WinnerText.text = GetString("EveryoneDied");
                WinnerText.color = Color.gray;
                break;
            case CustomWinner.Error:
                __instance.WinText.text = GetString("ErrorEndText");
                __instance.WinText.color = Color.red;
                __instance.BackgroundBar.material.color = Color.red;
                WinnerText.text = GetString("ErrorEndTextDescription");
                WinnerText.color = Color.white;
                break;
        }

        foreach (var role in CustomWinnerHolder.AdditionalWinnerRoles)
        {
            var addWinnerRole = (CustomRoles)role;
            AdditionalWinnerText.Append('＆').Append(Utils.ColorString(Utils.GetRoleColor(role), GetWinnerRoleName(addWinnerRole, 0) + GetString("Win")));
            EndAdditionalWinnerText.Append('＆').Append(Utils.ColorString(Utils.GetRoleColor(role), GetWinnerRoleName(addWinnerRole, 1) + GetString("Win")));
        }
        if (CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw and not CustomWinner.None and not CustomWinner.Error)
        {
            if (AdditionalWinnerText.Length < 1) WinnerText.text = $"<color={CustomWinnerColor}>{CustomWinnerText}</color>";
            else WinnerText.text = $"<color={CustomWinnerColor}>{CustomWinnerText}</color>{AdditionalWinnerText}";
            if (EndAdditionalWinnerText.Length < 1) InEndWinnerText = $"<color={EndWinnerColor}>{EndWinnerText}{GetString("Win")}</color>";
            else InEndWinnerText = $"<color={CustomWinnerColor}>{CustomWinnerText}</color>{AdditionalWinnerText}{GetString("Win")}";
        }

        static string GetWinnerRoleName(CustomRoles role, int a)
        {
            if (a == 0
                )
            {
                var name = GetString($"WinnerRoleText.{Enum.GetName(typeof(CustomRoles), role)}");
                if (name == "" || name.StartsWith("*") || name.StartsWith("<INVALID")) name = Utils.GetRoleName(role);
                return name;
            }
            else
            {
                var name = GetString($"WinnerRoleText.InEnd.{Enum.GetName(typeof(CustomRoles), role)}");
                if (name == "" || name.StartsWith("*") || name.StartsWith("<INVALID")) name = Utils.GetRoleName(role);
                return name;
            }

        }

        LastWinsText = InEndWinnerText;



        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //#######################################
        //           ==最終結果表示==
        //#######################################

        var showInitially = Main.ShowResults.Value;
        showHideButton = new SimpleButton(
           __instance.transform,
           "ShowHideResultsButton",
           new(-4.5f, 2.6f, -14f),  // BackgroundLayer(z=-13)より手前
           new(0, 136, 209, byte.MaxValue),
           new(0, 196, byte.MaxValue, byte.MaxValue),
           () =>
           {
               var setToActive = !roleSummary.gameObject.activeSelf;
               roleSummary.gameObject.SetActive(setToActive);
               Main.ShowResults.Value = setToActive;
               showHideButton.Label.text = GetString(setToActive ? "HideResults" : "ShowResults");
           },
           GetString(showInitially ? "HideResults" : "ShowResults"))
        {
            Scale = new(1.5f, 0.5f),
            FontSize = 2f,
        };

        StringBuilder sb = new($"{GetString("RoleSummaryText")}");
        List<byte> cloneRoles = new(PlayerState.AllPlayerStates.Keys);
        foreach (var id in Main.winnerList.Where(i => !EndGamePatch.SummaryText[i].Contains("NotAssigned")))
        {
            sb.Append($"\n<color={CustomWinnerColor}>★</color> ").Append(EndGamePatch.SummaryText[id]);
            cloneRoles.Remove(id);
        }
        foreach (var id in cloneRoles.Where(i => !EndGamePatch.SummaryText[i].Contains("NotAssigned")))
        {
            sb.Append($"\n　 ").Append(EndGamePatch.SummaryText[id]);
        }
        roleSummary = TMPTemplate.Create(
                "RoleSummaryText",
                sb.ToString(),
                Color.white,
                1.25f,
                TextAlignmentOptions.TopLeft,
                setActive: showInitially,
                parent: showHideButton.Button.transform);
        roleSummary.transform.localPosition = new(1.7f, -0.4f, 0f);
        roleSummary.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Utils.ApplySuffix();
    }
}