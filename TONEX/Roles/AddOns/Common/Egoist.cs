using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Options;
using Hazel;


namespace TONEX.Roles.AddOns.Common;
public sealed class Egoist : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Egoist),
    player => new Egoist(player),
    CustomRoles.Egoist,
   80800,
   SetupOptionItem,
    "ego|利己主義者|利己|野心|利己主义",
    "#5600ff"
    );
    public Egoist(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    enum OptionName
    {
        ImpEgoistVisibalToAllies,
    }
    public static OptionItem OptionImpEgoVisibalToAllies;
    static void SetupOptionItem()
    {
        OptionImpEgoVisibalToAllies = BooleanOptionItem.Create(RoleInfo, 20, OptionName.ImpEgoistVisibalToAllies, true, false);
    }

}