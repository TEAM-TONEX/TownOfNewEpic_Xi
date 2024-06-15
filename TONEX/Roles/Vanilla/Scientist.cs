using AmongUs.GameOptions;
using TONEX.Roles.Core;

namespace TONEX.Roles.Vanilla;

public sealed class Scientist : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
           typeof(Scientist),
           player => new Scientist(player),
           CustomRoles.Scientist,
           () => RoleTypes.Scientist,
           CustomRoleTypes.Crewmate,
           0003,
           null,
           "sci",
           "#8cffff"

       );
    public Scientist(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
}