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
public sealed class Infector : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Infector),
            player => new Infector(player),
            CustomRoles.Infector,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Neutral,
            94_1_3_0300,
            null,
            "zb",
            "#009900",
            true,
           introSound: () => GetIntroSound(RoleTypes.Impostor),
           ctop: true

        );
    public Infector(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
        )
    { }
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetFloat(FloatOptionNames.ImpostorLightMod, 0.2f);
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var Sr in InfectorManager.HumanNum)
        {
            var Zb = Utils.GetPlayerById(Player.PlayerId);
            var pc = Utils.GetPlayerById(Sr);
            if (!pc.IsAlive()) continue;
            var pos = Zb.transform.position;
            var dis = Vector2.Distance(pos, pc.transform.position);
            if (dis > 0.3f) continue;
            pc.RpcSetCustomRole(CustomRoles.Infector);
            pc.SetOutFit(2);
            Utils.NotifyRoles();
        }
    }
    public override void OnSecondsUpdate(PlayerControl player, long now)
    {
        Player.Notify(string.Format(GetString("ZombieTimeRemain"), InfectorManager.RemainRoundTime));
    }
}

