﻿using AmongUs.GameOptions;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Impostor;
public sealed class Miner : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Miner),
            player => new Miner(player),
            CustomRoles.Miner,
            () => RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            1900,
            null,
            "mn|礦工",
            ctop: true
        );
    public Miner(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    public override bool GetAbilityButtonText(out string text)
    {
        text = Translator.GetString("MinerTeleButtonText");
        return Main.LastEnteredVent.ContainsKey(Player.PlayerId);
    }
    public override void OnShapeshift(PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        if (Main.LastEnteredVent.ContainsKey(Player.PlayerId))
        {
            Player.RpcTeleport(Main.LastEnteredVentLocation[Player.PlayerId]);
            Logger.Msg($"矿工传送：{Player.GetNameWithRole()}", "Miner.OnShapeshift");
        }
    }
}