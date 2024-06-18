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
           0030,
           SetupOptionItem,
           "engin",
           "#8C90FF"

       );
    public Engineer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    static OptionItem EngineerCooldown;
    static OptionItem EngineerInVentCooldown;
    private static void SetupOptionItem()
    {
        EngineerCooldown = IntegerOptionItem.Create(RoleInfo, 5, StringNames.EngineerCooldown, new(0, 180, 5), 20, false)
            .SetValueFormat(OptionFormat.Seconds);
        EngineerInVentCooldown = IntegerOptionItem.Create(RoleInfo, 6, StringNames.EngineerInVentCooldown, new(0, 60, 5), 20, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown = EngineerCooldown.GetInt();
        AURoleOptions.EngineerInVentMaxTime = EngineerInVentCooldown.GetInt();
    }
}