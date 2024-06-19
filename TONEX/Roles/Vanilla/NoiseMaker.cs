using AmongUs.GameOptions;

using TONEX.Roles.Core;

namespace TONEX.Roles.Vanilla;

public sealed class Noisemaker : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
       SimpleRoleInfo.Create(
           typeof(Noisemaker),
           player => new Noisemaker(player),
           CustomRoles.Noisemaker,
           () => RoleTypes.Noisemaker,
           CustomRoleTypes.Crewmate,
           0070,
           SetupOptionItem,
           "engin",
           "#93FF8C"

       );
    public Noisemaker(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    static OptionItem NoisemakerImpostorAlert;
    static OptionItem NoisemakerAlertDuration;
    private static void SetupOptionItem()
    {
        NoisemakerImpostorAlert = BooleanOptionItem.Create(RoleInfo, 5, StringNames.NoisemakerImpostorAlert, true, false);
        NoisemakerAlertDuration = IntegerOptionItem.Create(RoleInfo, 6, StringNames.NoisemakerAlertDuration, new(0, 180, 5), 20, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.NoisemakerAlertDuration = NoisemakerAlertDuration.GetInt();
        AURoleOptions.NoisemakerImpostorAlert = NoisemakerImpostorAlert.GetBool();
    }
}