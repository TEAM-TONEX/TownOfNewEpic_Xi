using AmongUs.GameOptions;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Vanilla;

public sealed class Phantom : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
           typeof(Phantom),
           player => new Phantom(player),
           CustomRoles.Phantom,
           () => RoleTypes.Phantom,
           CustomRoleTypes.Impostor,
           0090,
           SetupOptionItem,
           "shape",
           "#ff1919"

       );
    public Phantom(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    static OptionItem PhantomCooldown;
    static OptionItem PhantomDuration;
    private static void SetupOptionItem()
    {
        PhantomCooldown = IntegerOptionItem.Create(RoleInfo, 5, StringNames.PhantomCooldown, new(0, 180, 5), 30, false)
            .SetValueFormat(OptionFormat.Seconds);
        PhantomDuration = IntegerOptionItem.Create(RoleInfo, 6, StringNames.PhantomDuration, new(0, 60, 5), 20, false)
            .SetValueFormat(OptionFormat.Seconds);
    
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.PhantomCooldown = PhantomCooldown.GetInt();
        AURoleOptions.PhantomDuration = PhantomDuration.GetInt();
    }
}