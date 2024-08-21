using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;

namespace TONEX.Roles.AddOns.Common;
public sealed class Spiders : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Spiders),
    player => new Spiders(player),
    CustomRoles.Spiders,
    94_1_1_0100,
   SetupCustomOption,
    "sd|蜘蛛",
    "#ff1919",
    2
    );
    public Spiders(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    public static OptionItem OptionSpeed;
    enum OptioName
    {
        SpidersSpeed
    }
    public static void SetupCustomOption()
    {
        OptionSpeed = FloatOptionItem.Create(RoleInfo, 20, OptioName.SpidersSpeed, new(0.25f, 5f, 0.25f), 0.5f,  false)
              .SetValueFormat(OptionFormat.Multiplier);
    }
}
