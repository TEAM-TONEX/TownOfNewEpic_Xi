using AmongUs.GameOptions;
using static TONEX.Translator;
using TONEX.Roles.Core;
using MS.Internal.Xml.XPath;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Ghost.Neutral;
public sealed class Specterraid : RoleBase, INeutral
{

    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Specterraid),
            player => new Specterraid(player),
            CustomRoles.Specterraid,
            () => RoleTypes.GuardianAngel,
            CustomRoleTypes.Neutral,
            75_1_5_0300,
            null,
            "ijs|冤枉",
            "#65167d",
            ctop: true
        );
    public Specterraid(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.ForRecompute
    )
    {
    }
    public static PlayerControl SetPlayer;
    public static bool SetYet;
    public override void OnGameStart()
    {
        SetYet = false;
    }
    public static OptionItem EnableSpecterraid;
    public static OptionItem OptionTaskCount;

    public static bool CheckForSet(PlayerControl pc)
    {
        if (SetYet || !EnableSpecterraid.GetBool()) return false;

        SetYet = true;
        pc.Notify(GetString("SurpriseAfterMeet"));
        SetPlayer = pc;
        return true;
    }
    public static void SetupOptionItem()
    {
        EnableSpecterraid = BooleanOptionItem.Create(75_1_5_0310, "EnableSpecterraid", false, TabGroup.NeutralRoles, false)
            .SetHeader(true)
            .SetGameMode(CustomGameMode.Standard);
        OptionTaskCount = IntegerOptionItem.Create(75_1_5_0311, "OptionTaskCount", new(0, 100, 1), 10, TabGroup.NeutralRoles, false)
            .SetValueFormat(OptionFormat.Pieces)
            .SetParent(EnableSpecterraid);
        
    }
    public override bool OnCompleteTask(out bool cancel)
    {
        if (IsTaskFinished)
        Win();
        cancel = false;
        return false;
    }
    public void Win()
    {
        CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Specterraid);
        CustomWinnerHolder.WinnerIds.Add(Player.PlayerId);
    }

    public override void OverrideDisplayRoleNameAsSeen(PlayerControl seer, ref bool enabled, ref UnityEngine.Color roleColor, ref string roleText)
    => enabled |= true;
    public override bool CanUseAbilityButton() => false;
    public override bool OnProtectPlayer(PlayerControl target)
    {
        return false;
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.GuardianAngelCooldown = 255f;
    }

}
