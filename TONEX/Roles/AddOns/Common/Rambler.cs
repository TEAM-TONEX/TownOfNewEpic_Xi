using System.Collections.Generic;
using System.Drawing;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;
using AmongUs.GameOptions;
using System.Linq;

namespace TONEX.Roles.AddOns.Common;
public sealed class Rambler : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Rambler),
    player => new Rambler(player),
    CustomRoles.Rambler,
   154564874,
    SetupOptionItem,
    "ra|漫步",
    "#ccffff"
    );
    public Rambler(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }



    public static OptionItem OptionSpeed;
    enum OptionName
    {
        RamblerSpeed
    }

    static void SetupOptionItem()
    {
        OptionSpeed = FloatOptionItem.Create(RoleInfo,20,OptionName.RamblerSpeed, new(0.25f, 5f, 0.25f), 0.5f,false)
         .SetValueFormat(OptionFormat.Multiplier);
    }

    public override void ApplyGameOptions(IGameOptions opt)
    {
        if (Main.AllPlayerControls.Any(x => x.Is(CustomRoles.Rambler) && !x.IsAlive() && x.GetRealKiller()?.PlayerId == player.PlayerId && !x.Is(CustomRoles.Rambler)))
        {
            Main.AllPlayerSpeed[player.PlayerId] = Rambler.OptionSpeed.GetFloat();
            player.RpcSetCustomRole(CustomRoles.Rambler);
            Utils.NotifyRoles(player);
        }

    }
}
