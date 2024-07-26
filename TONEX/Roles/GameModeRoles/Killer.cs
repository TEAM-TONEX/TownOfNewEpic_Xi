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
public sealed class Killer : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Killer),
            player => new Killer(player),
            CustomRoles.Killer,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            94_1_3_0400,
            null,
            "Ki",
            "#FF3333",
            true,
           introSound: () => DestroyableSingleton<HnSImpostorScreamSfx>.Instance.HnSOtherImpostorTransformSfx,
    ctop: true
        );
    public Killer(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
    }
    public bool IsNK { get; private set; } = true;
    public bool CanUseKillButton() => true;
    public bool CanUseSabotageButton() => false;
    public float CalculateKillCooldown() => FFAManager.FFA_KCD.GetFloat();
}