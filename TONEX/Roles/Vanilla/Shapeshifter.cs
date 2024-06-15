using AmongUs.GameOptions;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Vanilla;

public sealed class Shapeshifter : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
           typeof(Shapeshifter),
           player => new Shapeshifter(player),
           CustomRoles.Shapeshifter,
           () => RoleTypes.Shapeshifter,
           CustomRoleTypes.Impostor,
           0004,
           null,
           "shape",
           "#ff1919"

       );
    public Shapeshifter(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
}