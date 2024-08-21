using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using System;
using TONEX.Roles.Core;
using static TONEX.Options;
using static TONEX.Utils;
using System.Text;
using InnerNet;
using TONEX.Roles.Core.Interfaces;

namespace TONEX.Roles.AddOns.Common;
public sealed class Lovers : AddonBase//, IOverrideWinner
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Lovers),
    player => new Lovers(player),
    CustomRoles.Lovers,
    75_1_2_1400,
    SetupOptionItem,
    "lo|情人|愛人|链子",
    "#ff9ace"
    );
    public Lovers(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    public PlayerControl Love;

    public static OptionItem LoverKnowRoles;
    public static OptionItem LoverSuicide;

    public static bool isLoversDead = true;

    enum OptionName
    {
        LoverKnowRoles,
             LoverSuicide,
    }

    static void SetupOptionItem()
    {
        LoverKnowRoles = BooleanOptionItem.Create(RoleInfo, 20, OptionName.LoverKnowRoles, true, false)
      .SetGameMode(CustomGameMode.Standard);
        LoverSuicide = BooleanOptionItem.Create(RoleInfo, 21, OptionName.LoverSuicide, true,false)
            .SetGameMode(CustomGameMode.Standard);
    }
    public override void ReceiveRPC(MessageReader reader, CustomRPC rpcTypes)
    {
        if (rpcTypes != CustomRPC.SetLoversPlayers) return;
    }
    public static void SyncLoversPlayers()
    {
        if (!AmongUsClient.Instance.AmHost) return;
    }
    public void CheckWin()
    {
        // 恋人胜利
        if (Main.AllPlayerControls.Any(p => CustomWinnerHolder.WinnerIds.Contains(p.PlayerId) && p.Is(CustomRoles.Lovers)))
        {
            CustomWinnerHolder.AdditionalWinnerRoles.Add(CustomRoles.Lovers);
            Main.AllPlayerControls.Where(p => p.Is(CustomRoles.Lovers))
                .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
        }
    }
    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
    {
        if (Player.PlayerId == exiled.PlayerId)
            Love.RpcExileV2();
    } 
    public static void AssignLoversRoles(int RawCount = -1)
    {
        if (!CustomRoles.Lovers.IsEnable()) return;
        if (Main.AllPlayerControls.Count() < 2) return;
        //Loversを初期化
        isLoversDead = false;
        var allPlayers = new List<PlayerControl>();
        foreach (var pc in Main.AllPlayerControls)
        {
            if (pc.Is(CustomRoles.GM) || (PlayerState.GetByPlayerId(pc.PlayerId).SubRoles.Count >= Options.AddonsNumLimit.GetInt())
                || pc.Is(CustomRoles.LazyGuy) || pc.Is(CustomRoles.Neptune) || pc.Is(CustomRoles.God) || pc.Is(CustomRoles.Hater) || pc.Is(CustomRoles.Believer) || pc.Is(CustomRoles.Nihility)
                || pc.Is(CustomRoles.Admirer) || pc.Is(CustomRoles.Akujo) || pc.Is(CustomRoles.Cupid) || pc.Is(CustomRoles.Yandere)) continue;
            allPlayers.Add(pc);
        }
        var loversRole = CustomRoles.Lovers;
        var rd = IRandom.Instance;
        var count = Math.Clamp(RawCount, 0, allPlayers.Count);
        if (RawCount == -1) count = Math.Clamp(loversRole.GetCount(), 0, allPlayers.Count);
        if (count <= 0) return;
        for (var i = 0; i < count; i++)
        {
            var player = allPlayers[rd.Next(0, allPlayers.Count)];
            allPlayers.Remove(player);
            PlayerState.GetByPlayerId(player.PlayerId).SetSubRole(loversRole);
            Logger.Info($"注册附加职业：{player?.Data?.PlayerName}（{player.GetCustomRole()}）=> {loversRole}", "AssignCustomSubRoles");
        }
    }
    public static void OnPlayerLeft(ClientData data)
    {
        if (data.Character.Is(CustomRoles.Lovers) && !data.Character.Data.IsDead)
            data.Character.GetAddonClasses().Where(x => x is Lovers).Do(x => x.Dispose());
    }
    public override void OverrideDisplayRoleNameAsSeer(PlayerControl seen, ref bool enabled, ref UnityEngine.Color roleColor, ref string roleText)

    {
        enabled = (seen == Player || seen == Love) && LoverKnowRoles.GetBool();
    }
    public static void TargetMarks(PlayerControl seer, PlayerControl target, ref StringBuilder targetMark)
    {
        //ハートマークを付ける(相手に)
        if (seer.Is(CustomRoles.Lovers) && target.Is(CustomRoles.Lovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.Lovers)}>♡</color>");
        }
        //霊界からラバーズ視認
        else if (seer.Data.IsDead && !seer.Is(CustomRoles.Lovers) && target.Is(CustomRoles.Lovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.Lovers)}>♡</color>");
        }
    }
    public static void Marks(PlayerControl __instance, ref StringBuilder Mark)
    {
        if (__instance.Is(CustomRoles.Lovers) && PlayerControl.LocalPlayer.Is(CustomRoles.Lovers))
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.Lovers)}>♡</color>");
        }
        else if (__instance.Is(CustomRoles.Lovers) && PlayerControl.LocalPlayer.Data.IsDead)
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.Lovers)}>♡</color>");
        }
    }
}
