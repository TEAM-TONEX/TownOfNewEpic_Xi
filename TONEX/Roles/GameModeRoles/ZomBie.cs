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
//public sealed class ZomBie : RoleBase
//{
//    public static readonly SimpleRoleInfo RoleInfo =
//        SimpleRoleInfo.Create(
//            typeof(ZomBie),
//            player => new ZomBie(player),
//            CustomRoles.ZomBie,
//            () => RoleTypes.Crewmate,
//            CustomRoleTypes.Neutral,
//            94_1_3_0300,
//            null,
//            "zb",
//            "#66ffff",
//            true,
//           introSound: () => GetIntroSound(RoleTypes.Crewmate),
//           ctop: true

//        );
//    public ColdPotato(PlayerControl player)
//    : base(
//        RoleInfo,
//        player,
//        () => HasTask.False
//        )
//    { }
//    public override void OnFixedUpdate(PlayerControl player)
//    {
//        if (!AmongUsClient.Instance.AmHost) return;
//        foreach (var player in ZombieManager.HumanNum)
//        {
//            var Zb = Utils.GetPlayerById(Player);
//            var pc = Utils.GetPlayerById(player);
//            if (!pc.IsAlive()) continue;
//            var pos = Zb.transform.position;
//            var dis = Vector2.Distance(pos, pc.transform.position);
//            if (dis > 0.3f) continue;
//            player.RpcSetCustomRole(CustomRoles.ZomBie);
//            player.SetOutFitStatic(1);
//            Utils.NotifyRoles();
//        }
//    }
//    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
//    {
//        //seenが省略の場合seer
//        seen ??= seer;
//        //seeおよびseenが自分である場合以外は関係なし
//        if (!Is(seer) || !Is(seen)) return "";

//        return string.Format(GetString("ZombieTimeRemain"), ZombieManager.RemainRoundTime);
//    }
//}

