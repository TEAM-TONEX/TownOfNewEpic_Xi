using Hazel;
using System.Collections.Generic;
using TONEX.Roles.Core;

namespace TONEX.Roles.AddOns.Common;
public sealed class Diseased : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Diseased),
    player => new Diseased(player),
    CustomRoles.Diseased,
    75_1_1_0500,
   SetupOptionItem,
    "dis|ªº’ﬂ|≤°»À",
    "#c0c0c0"
    );
    public Diseased(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    public static OptionItem OptionVistion;
    enum OptionName
    {
        DiseasedVision
    }

    static void SetupOptionItem()
    {
        OptionVistion = FloatOptionItem.Create(RoleInfo, 20, OptionName.DiseasedVision, new(0.5f, 5f, 0.25f), 1.5f, false)
.SetValueFormat(OptionFormat.Multiplier);
    }


}