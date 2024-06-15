using AmongUs.GameOptions;

using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Vanilla;

public sealed class Impostor : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
   SimpleRoleInfo.Create(
       typeof(Impostor),
       player => new Impostor(player),
       CustomRoles.Impostor,
       () => RoleTypes.Impostor,
       CustomRoleTypes.Impostor,
       0020,
       null,
       "imp",
       "#ff1919"

    );
    public Impostor(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    
}