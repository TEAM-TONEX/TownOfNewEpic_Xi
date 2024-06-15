using AmongUs.GameOptions;

using TONEX.Roles.Core;

namespace TONEX.Roles.Vanilla;

public sealed class Engineer : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
       SimpleRoleInfo.Create(
           typeof(Engineer),
           player => new Engineer(player),
           CustomRoles.Engineer,
           () => RoleTypes.Engineer,
           CustomRoleTypes.Crewmate,
           0002,
           null,
           "engin",
           "#8cffff"

       );
    public Engineer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
}