using AmongUs.GameOptions;

using TONEX.Roles.Core;

namespace TONEX.Roles.Vanilla;

public sealed class Crewmate : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Crewmate),
            player => new Crewmate(player),
            CustomRoles.Crewmate,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            0010,
            null,
            "crew",
            "#8cffff"

        );
    public Crewmate(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
}