using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;

namespace TONEX.Roles.AddOns.Common;
public sealed class PublicOpinionShaper : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(PublicOpinionShaper),
    player => new PublicOpinionShaper(player),
    CustomRoles.PublicOpinionShaper,
    75_1_2_0700,
    null,
    "pos|舆论缔造者",
    "#ff1919",
    2
    );
    public PublicOpinionShaper(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

}

