using System;
using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Options;
using static TONEX.Translator;

namespace TONEX.Roles.AddOns.Common;
public sealed class Believer : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Believer),
    player => new Believer(player),
    CustomRoles.Believer,
    75_1_2_0600,
    null,
    "bel|信徒",
    "#007169"
    );
    public Believer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
}



