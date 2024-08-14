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

    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        if (target.Object.GetRealKiller() != null && target.Object.GetRealKiller() == Player)
        {
            if (Utils.IsActive(SystemTypes.Comms) 
                || Utils.IsActive(SystemTypes.Electrical) 
                || Utils.IsActive(SystemTypes.Reactor) 
                || Utils.IsActive(SystemTypes.LifeSupp) 
                || Utils.IsActive(SystemTypes.MushroomMixupSabotage))
            {
                return true;
                
            }
            reporter.Notify(Translator.GetString("NobodyNoticed"));
            return false;
        }
        return true;
    }
}

