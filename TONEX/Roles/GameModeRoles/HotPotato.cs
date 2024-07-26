using AmongUs.GameOptions;
using UnityEngine;
using Hazel;
using TONEX.Roles.Core;
using static TONEX.Translator;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using TONEX.MoreGameModes;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using RewiredConsts;

namespace TONEX.Roles.GameModeRoles;
public sealed class HotPotato : RoleBase, IKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(HotPotato),
            player => new HotPotato(player),
            CustomRoles.HotPotato,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            94_1_0_0200,
            null,
            "ho",
            "#ff9900",
            true,
           introSound: () => GetIntroSound(RoleTypes.Impostor),
           ctop: true
        );
    public HotPotato(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        CustomRoleManager.MarkOthers.Add(MarkOthers);
    }
    public bool IsKiller { get; private set; } = false;
    public static bool KnowTargetRoleColor(PlayerControl target, bool isMeeting)
    => target.Is(CustomRoles.HotPotato);
    public static string MarkOthers(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        return (seen.Is(CustomRoles.HotPotato)) ? Utils.ColorString(RoleInfo.RoleColor, "●") : "";
    }
    public override void OverrideDisplayRoleNameAsSeen(PlayerControl seer, ref bool enabled, ref Color roleColor, ref string roleText)
        => enabled |= true;
    public float CalculateKillCooldown() => 0f;
    public bool CanUseKillButton() => true;
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        info.CanKill = false;
        var (killer, target) = info.AttemptTuple;
        if(Options.CurrentGameMode != CustomGameMode.HotPotato || !killer.Is(CustomRoles.HotPotato) || target.Is(CustomRoles.HotPotato)) return false;
        target.RpcSetCustomRole(CustomRoles.HotPotato);
        killer.RpcSetCustomRole(CustomRoles.ColdPotato);
        RPC.PlaySoundRPC(killer.PlayerId, Sounds.KillSound);
        RPC.PlaySoundRPC(target.PlayerId, Sounds.KillSound);
        new LateTask(() =>
        {
             target.SetKillCooldownV2(0f);
        }, 0.1f, "Clam");
        Utils.NotifyRoles(killer);
        Utils.NotifyRoles(target);
        info.CanKill = false;
        return false;
    }
    public override void OnSecondsUpdate(PlayerControl player, long now)
    {
        Player.Notify(string.Format(GetString("HotPotatoTimeRemain"), HotPotatoManager.RemainExplosionTime));
    }
}
