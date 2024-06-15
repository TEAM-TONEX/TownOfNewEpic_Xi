using AmongUs.GameOptions;

using TONEX.Roles.Core;

namespace TONEX.Roles.Vanilla;

public sealed class GuardianAngel : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
       SimpleRoleInfo.Create(
           typeof(GuardianAngel),
           player => new GuardianAngel(player),
           CustomRoles.GuardianAngel,
           () => RoleTypes.GuardianAngel,
           CustomRoleTypes.Crewmate,
           0060,
           null,
           "ga|guard",
           "#8cffff",
           ctop: true

       );
    public GuardianAngel(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    public static int SatPlayerCount;
    public override void OnGameStart()
    {
        SatPlayerCount = 0;
    }
    public static OptionItem EnableGuardianAngel;
    public static OptionItem PlayerCount;
    static OptionItem GuardianAngelCooldown;
    public static void SetupCustomOptionItem()
    {
        EnableGuardianAngel = BooleanOptionItem.Create(0065, "EnableGuardianAngel", false, TabGroup.CrewmateRoles, false)
            .SetHeader(true)
            .SetGameMode(CustomGameMode.Standard);
        PlayerCount = IntegerOptionItem.Create(0066, "Maximum", new(1, 15, 1), 1, TabGroup.CrewmateRoles, false)
            .SetValueFormat(OptionFormat.Players)
            .SetParent(EnableGuardianAngel);
        
        GuardianAngelCooldown = FloatOptionItem.Create(0067, StringNames.GuardianAngelCooldown, new(0f, 180f, 2.5f), 60f,TabGroup.CrewmateRoles, false)
            .SetValueFormat(OptionFormat.Seconds)
             .SetParent(EnableGuardianAngel);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.GuardianAngelCooldown = GuardianAngelCooldown.GetInt();
    }

}