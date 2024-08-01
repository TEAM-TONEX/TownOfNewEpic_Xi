using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;
using System.Drawing;

namespace TONEX.Roles.AddOns.Common;
public sealed class Schizophrenic : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Schizophrenic),
    player => new Schizophrenic(player),
    CustomRoles.Schizophrenic,
    81500,
    null,
    "sp|雙重人格|双重|双人格|人格",
    "#3a648f"
    );
    public Schizophrenic(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

}