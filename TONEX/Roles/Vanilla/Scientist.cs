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
           0040,
           SetupOptionItem,
           "sci",
           "#8cffff"

       );
    public Scientist(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    static OptionItem ScientistCooldown;
    static OptionItem ScientistBatteryCharge;
    private static void SetupOptionItem()
    {
        ScientistCooldown = IntegerOptionItem.Create(RoleInfo, 5, StringNames.ScientistCooldown, new(0, 60, 5), 10, false)
            .SetValueFormat(OptionFormat.Seconds);
        ScientistBatteryCharge = IntegerOptionItem.Create(RoleInfo, 6, StringNames.ScientistBatteryCharge, new(0, 30, 5), 5, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.ScientistCooldown = ScientistCooldown.GetInt();
        AURoleOptions.ScientistBatteryCharge = ScientistBatteryCharge.GetInt();
    }
}