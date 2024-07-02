/*using AmongUs.GameOptions;
using UnityEngine;
using Hazel;
using TONEX.Roles.Core;
using static TONEX.Translator;
using System.Text;
using TONEX.MoreGameModes;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using TONEX.Roles.Neutral;

namespace TONEX.Roles.GameModeRoles;
public sealed class Human : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Human),
            player => new Human(player),
            CustomRoles.Human,
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
    public ColdPotato(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.True
        )
    { }
    public override void Add()
    {
        Player.SetOutFitStatic(2);
    }
    public override bool OnCompleteTask(out bool cancel)
    {
        if (MyTaskState.IsTaskFinished && Player.IsAlive() && !ZombieManager.HumanCompleteTasks.Contains(Player))  {
            ZombieManager.HumanCompleteTasks.Add(Player);
        }
        cancel = false;
        return false;
    }


    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        //seenが省略の場合seer
        seen ??= seer;
        //seeおよびseenが自分である場合以外は関係なし
        if (!Is(seer) || !Is(seen)) return "";

        return string.Format(GetString("HotPotatoTimeRemain"), ZombieManager.RemainRoundTime);
    }
}
*/