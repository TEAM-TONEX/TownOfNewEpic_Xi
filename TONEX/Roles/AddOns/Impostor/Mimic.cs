using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;

namespace TONEX.Roles.AddOns.Common;
public sealed class Mimic : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Mimic),
    player => new Mimic(player),
    CustomRoles.Mimic,
    82000,
    null,
    "mi|åöœ‰π÷|±¶œ‰",
    "#ff1919",
    2
    );
    public Mimic(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

}