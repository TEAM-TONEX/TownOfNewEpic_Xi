using AmongUs.GameOptions;

using TONEX.Roles.Core;

namespace TONEX.Roles.Vanilla;

public sealed class GuardianAngel : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
       SimpleRoleInfo.Create(
           typeof(GuardianAngel),
           player => new GuardianAngel(player),
           CustomRoles.GuardianAngel,
           () => RoleTypes.GuardianAngel,
           CustomRoleTypes.Crewmate,
           0005,
           null,
           "ga|guard",
           "#8cffff"

       );
    public GuardianAngel(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
}