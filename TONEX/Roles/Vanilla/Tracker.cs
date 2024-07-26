using AmongUs.GameOptions;

using TONEX.Roles.Core;

namespace TONEX.Roles.Vanilla;

public sealed class Tracker : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
       SimpleRoleInfo.Create(
           typeof(Tracker),
           player => new Tracker(player),
           CustomRoles.Tracker,
           () => RoleTypes.Tracker,
           CustomRoleTypes.Crewmate,
           0080,
           SetupOptionItem,
           "engin",
           "#93FF8C"

       );
    public Tracker(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    static OptionItem TrackerCooldown;
    static OptionItem TrackerDelay;
    static OptionItem TrackerDuration;
    private static void SetupOptionItem()
    {
        TrackerCooldown = IntegerOptionItem.Create(RoleInfo, 5, StringNames.TrackerCooldown, new(0, 180, 5), 20, false)
            .SetValueFormat(OptionFormat.Seconds);
        TrackerDelay = IntegerOptionItem.Create(RoleInfo, 6, StringNames.TrackerDelay, new(0, 60, 5), 20, false)
            .SetValueFormat(OptionFormat.Seconds);
        TrackerDuration = IntegerOptionItem.Create(RoleInfo, 7, StringNames.TrackerDuration, new(0, 60, 5), 20, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.TrackerCooldown = TrackerCooldown.GetInt();
        AURoleOptions.TrackerDelay = TrackerDelay.GetInt();
        AURoleOptions.TrackerDuration = TrackerDuration.GetInt();
    }
}