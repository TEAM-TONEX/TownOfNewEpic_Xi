using Hazel;
using Il2CppInterop.Generator.Extensions;
using System.Collections.Generic;
using System.Linq;
using TONEX.Roles.Core;
using static TONEX.Translator;

namespace TONEX;

public static class ConfirmEjections
{
    // 参考：https://github.com/music-discussion/TownOfHost-TheOtherRoles
    public static string LatestEjec = "";
    public static void Apply(NetworkedPlayerInfo exiledPlayer, bool decidedWinner, List<string> winDescriptionText)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (exiledPlayer == null) return;
        var exileId = exiledPlayer.PlayerId;
        if (exileId is < 0 or > 254) return;

        var player = exiledPlayer.Object;
        var playerName = player.GetRealName(isMeeting: true);
        var coloredPlayerName = Utils.ColorString(Main.PlayerColors[player.PlayerId], playerName);
        var role = exiledPlayer.GetCustomRole();
        var roleName = GetRoleString(role.ToString());
        var coloredRoleName = Utils.GetTrueRoleName(exileId, false);
        var roleType = player.Is(CustomRoles.Madmate) ? CustomRoleTypes.Impostor
            : player.Is(CustomRoles.Charmed) || player.Is(CustomRoles.Wolfmate) ? CustomRoleTypes.Neutral
            : role.GetCustomRoleTypes();
        var coloredTeamName = GetString($"Team{roleType}").Color(Utils.GetCustomRoleTypeColor(roleType));

        string text = string.Empty;
        int impNum = 0;
        int neutralNum = 0;
        int neutralkillNum = 0;
        if (Main.AllAlivePlayerControls != null && Main.AllAlivePlayerControls.ToList().Count != 0)
        {
            impNum = Main.AllAlivePlayerControls.Count(p => p.Is(CustomRoleTypes.Impostor) || p.Is(CustomRoles.Madmate));
            neutralNum = Main.AllAlivePlayerControls.Count(p => p.IsNeutralEvil());
            neutralkillNum = Main.AllAlivePlayerControls.Count(p => p.IsNeutralKiller() || p.Is(CustomRoles.Charmed) || p.Is(CustomRoles.Wolfmate));
        }
        var text2 = "";
        if (CustomRoles.Bard.IsExist()) // 吟游诗人创作
        {
            try { text = ModUpdater.Get("https://v1.hitokoto.cn/?encode=text"); }
            catch { text = GetString("ByBardGetFailed"); }
            text2 = text;
            text += "\n\t\t——" + GetString("ByBard");
            text2 += "\n——" + GetString("ByBard");
            goto EndOfSession;
        }
        else if (decidedWinner) // 已经决定胜利者
        {
            text = string.Format(GetString("ExiledWrongPerson"), coloredPlayerName, coloredRoleName);
            winDescriptionText.Do(t => text += $"\n{t}");
        }
        else // 没有胜利者，游戏继续
        {
            switch (Options.CEMode.GetInt())
            {
                case 0: // 不确认身份
                    text = string.Format(GetString("PlayerExiled"), coloredPlayerName);
                    break;
                case 1: // 确认阵营
                    text = roleType is CustomRoleTypes.Crewmate
                        ? string.Format(GetString("IsGood"), coloredPlayerName)
                        : string.Format(GetString("BelongTo"), coloredPlayerName, coloredTeamName);
                    break;
                case 2: // 确认职业
                    text = string.Format(GetString("PlayerIsRole"), coloredPlayerName, coloredRoleName);
                    if (Options.ShowTeamNextToRoleNameOnEject.GetBool())
                        text += $" ({coloredTeamName})";
                    break;
            }
        }
        
        text2 = text;
        if (Options.ShowImpRemainOnEject.GetBool())
        {
            string comma = neutralNum > 0 || neutralkillNum > 0 ? "，\r\n" : "";
            string comma2 = neutralkillNum > 0 ? "，\r\n" : "";
            string comma3 = neutralNum > 0 || neutralkillNum > 0 ? "，\n" : "";
            string comma4 = neutralkillNum > 0 ? "，\n" : "";

            if (impNum == 0) 
            { 
                text += GetString("NoImpRemain") + comma;
                text2 += GetString("NoImpRemain") + comma3; 
            }
            else 
            {
                text += string.Format(GetString("ImpRemain"), impNum) + comma;
                text2 += string.Format(GetString("ImpRemain"), impNum) + comma3;
            }

            if (Options.ShowNERemainOnEject.GetBool() && neutralNum > 0)
            {
                text += string.Format(GetString("NeutralEvilRemain"), neutralNum) + comma2;
                text2 += string.Format(GetString("NeutralEvilRemain"), neutralNum) + comma4;
            }
            else if (Options.ShowNERemainOnEject.GetBool() && neutralNum == 0)
            {
                text += GetString("NoNeutralEvilRemain") + comma2;
                text2 += GetString("NoNeutralEvilRemain") + comma4;
            }

            if (Options.ShowNKRemainOnEject.GetBool() && neutralkillNum > 0)
            {
                text += string.Format(GetString("NeutralKillerRemain"), neutralkillNum);
                text2 += string.Format(GetString("NeutralKillerRemain"), neutralkillNum);
            }
            else if (Options.ShowNKRemainOnEject.GetBool() && neutralkillNum == 0)
            {
                text += GetString("NoNeutralKillerRemain");
                text2 += GetString("NoNeutralKillerRemain");
            }
        }

    EndOfSession:
        LatestEjec = text2;
        text += "<size=0>";
        _ = new LateTask(() =>
        {
            Main.DoBlockNameChange = true;
            if (GameStates.IsInGame)
            {
                player.RpcSetName(text);
            }
        }, 3.0f, "Change Exiled Player Name");
        _ = new LateTask(() =>
        {
            if (GameStates.IsInGame && !player.Data.Disconnected)
            {
                player.RpcSetName(playerName);
                Main.DoBlockNameChange = false;
            }
        }, 11.5f, "Change Exiled Player Name Back");
    }
    public static void GetLatest()
    {
        Logger.Info("-2", "test");
        if (!AmongUsClient.Instance.AmHost) return;
        Logger.Info("-1", "test");
        string text = string.Empty;
        int impNum = 0;
        int neutralNum = 0;
        int neutralkillNum = 0;
        Logger.Info("0", "test");
        if (Main.AllAlivePlayerControls != null &&Main.AllAlivePlayerControls.ToList().Count != 0)
        {
            impNum = Main.AllAlivePlayerControls.Count(p => p.Is(CustomRoleTypes.Impostor) || p.Is(CustomRoles.Madmate));
            neutralNum = Main.AllAlivePlayerControls.Count(p => p.IsNeutralEvil());
            neutralkillNum = Main.AllAlivePlayerControls.Count(p => p.IsNeutralKiller() || p.Is(CustomRoles.Charmed) || p.Is(CustomRoles.Wolfmate));
        }
        Logger.Info("1", "test");
        if (CustomRoles.Bard.IsExist()) // 吟游诗人创作
        {
            try { text = ModUpdater.Get("https://v1.hitokoto.cn/?encode=text"); }
            catch { text = GetString("ByBardGetFailed"); }
            text += "\n\t\t——" + GetString("ByBard");
            goto EndOfSession;
        }

        Logger.Info("2", "test");
        if (Options.ShowImpRemainOnEject.GetBool())
        {
            string comma = neutralNum > 0 || neutralkillNum > 0 ? "，\b" : "";
            string comma2 = neutralkillNum > 0 ? "，\n" : "";
            if (impNum == 0) text += GetString("NoImpRemain") + comma;
            else text += string.Format(GetString("ImpRemain"), impNum) + comma;

            Logger.Info("3", "test");
            if (Options.ShowNERemainOnEject.GetBool() && neutralNum > 0)
                text += string.Format(GetString("NeutralEvilRemain"), neutralNum)  + comma2;
            else if (Options.ShowNERemainOnEject.GetBool() && neutralNum == 0)
                text += GetString("NoNeutralEvilRemain") + comma2;

            Logger.Info("4", "test");
            if (Options.ShowNKRemainOnEject.GetBool() && neutralkillNum > 0)
                text += string.Format(GetString("NeutralKillerRemain"), neutralkillNum);
            else if (Options.ShowNKRemainOnEject.GetBool() && neutralkillNum == 0)
                text += GetString("NoNeutralKillerRemain");
        }
        Logger.Info("5", "test");
    EndOfSession:
        LatestEjec = text;
        Logger.Info("6", "test");
    }
}