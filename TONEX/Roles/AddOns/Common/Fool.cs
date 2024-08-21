using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;

namespace TONEX.Roles.AddOns.Common;
public sealed class Fool : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Fool),
    player => new Fool(player),
    CustomRoles.Fool,
    81300,
    SetupOptionItem,
    "fo|¥¿µ∞|±øµ∞|…µπ∑|…µ±∆|÷«’œ",
    "#e6e7ff"
    );
    public Fool(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }




    public static OptionItem OptionImpFoolCanNotSabotage;
    public static OptionItem OptionImpFoolCanNotOpenDoor;
    enum OptionName
    {
        ImpFoolCanNotSabotage,
            FoolCanNotOpenDoor
    }

    static void SetupOptionItem()
    {
        OptionImpFoolCanNotSabotage = BooleanOptionItem.Create(RoleInfo, 20, OptionName.ImpFoolCanNotSabotage, true, false);
        OptionImpFoolCanNotOpenDoor = BooleanOptionItem.Create(RoleInfo, 21, OptionName.FoolCanNotOpenDoor, true, false);
    }
}
