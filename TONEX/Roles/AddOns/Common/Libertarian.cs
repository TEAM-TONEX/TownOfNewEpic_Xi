using System.Collections.Generic;
using System.Drawing;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Options;

namespace TONEX.Roles.AddOns.Common;
public sealed class Libertarian : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Libertarian),
    player => new Libertarian(player),
    CustomRoles.Libertarian,
    156674,
    SetupOptionItem,
    "li|广播|自主主义者",
    "#33CC99"
    );
    public Libertarian(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }




    public static OptionItem OptionRadius;
    enum OptionName
    {
        LibertarianRadius
    }

    static void SetupOptionItem()
    {
        OptionRadius = FloatOptionItem.Create(RoleInfo, 20, OptionName.LibertarianRadius, new(0.5f, 10f, 0.5f), 1f, false)
            .SetValueFormat(OptionFormat.Multiplier);
    }
    public override void OnPlayerDeath(PlayerControl player, CustomDeathReason deathReason, bool isOnMeeting = false)
    {
        var target = player;
        if (target != null && Player!=null&& Vector2.Distance(Player.transform.position, target.transform.position) <= OptionRadius.GetFloat())
            Player.NoCheckStartMeeting(target?.Data);
    }
}


