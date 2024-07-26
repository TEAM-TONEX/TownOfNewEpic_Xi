using AmongUs.GameOptions;
using UnityEngine;
using Hazel;
using TONEX.Roles.Core;
using static TONEX.Translator;
using System.Text;
using TONEX.MoreGameModes;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using TONEX.Roles.Neutral;

namespace TONEX.Roles.GameModeRoles;
public sealed class Survivor : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Survivor),
            player => new Survivor(player),
            CustomRoles.Survivor,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Neutral,
            94_1_3_0200,
            null,
            "hm",
            "#66ffff",
            true,
           introSound: () => GetIntroSound(RoleTypes.Crewmate),
           ctop: true

        );
    public Survivor(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.True
        )
    { }
    public override void Add()
    {
        Player.SetOutFit(1);
    }
    public override bool OnCompleteTask(out bool cancel)
    {
        if (MyTaskState.IsTaskFinished && Player.IsAlive() && !InfectorManager.HumanCompleteTasks.Contains(Player.PlayerId))
        {
            InfectorManager.HumanCompleteTasks.Add(Player.PlayerId);
        }
        cancel = false;
        return false;
    }
    public override void OnSecondsUpdate(PlayerControl player, long now)
    {
        Player.Notify(string.Format(GetString("ZombieTimeRemain"), InfectorManager.RemainRoundTime));
    }
}
