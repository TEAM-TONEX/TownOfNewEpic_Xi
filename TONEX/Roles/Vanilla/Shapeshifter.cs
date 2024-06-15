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
           0050,
           SetupOptionItem,
           "shape",
           "#ff1919"

       );
    public Shapeshifter(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    static OptionItem ShapeshifterCooldown;
    static OptionItem ShapeshifterDuration;
    static OptionItem ShapeshifterLeaveSkin;
    private static void SetupOptionItem()
    {
        ShapeshifterCooldown = IntegerOptionItem.Create(RoleInfo, 5, StringNames.ShapeshifterCooldown, new(0, 180, 5), 30, false)
            .SetValueFormat(OptionFormat.Seconds);
        ShapeshifterDuration = IntegerOptionItem.Create(RoleInfo, 6, StringNames.ShapeshifterDuration, new(0, 60, 5), 20, false)
            .SetValueFormat(OptionFormat.Seconds);
        ShapeshifterLeaveSkin = BooleanOptionItem.Create(RoleInfo, 7, StringNames.ShapeshifterLeaveSkin, true, false);
    
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.ShapeshifterCooldown = ShapeshifterCooldown.GetInt();
        AURoleOptions.ShapeshifterDuration = ShapeshifterDuration.GetInt();
        AURoleOptions.ShapeshifterLeaveSkin = ShapeshifterLeaveSkin.GetBool();
    }
}