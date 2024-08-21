using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;


namespace TONEX.Roles.AddOns.Common;
public sealed class Nihility : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Nihility),
    player => new Nihility(player),
    CustomRoles.Nihility,
     75_1_2_0400,
    null,
    "nihi|虚无",
    "#444444"
    );
    public Nihility(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    public static bool OnCheckMurderPlayerOthers_Nihility(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        if (!killer.IsNeutralEvil() && target.Is(CustomRoles.Nihility)){
            killer.Notify(Translator.GetString("Nihility"));
            return false;
        }
        return true;
    }

}
