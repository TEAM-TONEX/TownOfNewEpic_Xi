using AmongUs.GameOptions;
using System.Collections.Generic;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using static TONEX.Translator;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using TONEX.Modules.SoundInterface;

namespace TONEX.Roles.Neutral;
public sealed class Rebels : RoleBase, IOverrideWinner, INeutral
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Rebels),
            player => new Rebels(player),
            CustomRoles.Rebels,
         () => Options.UsePets.GetBool() ? RoleTypes.Crewmate : RoleTypes.Engineer,
            CustomRoleTypes.Neutral,
            94_1_1_0300,
            SetupOptionItem,
            "re",
            "#339900"
        );
    public Rebels(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { 
    }

    static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillDuration;
    enum OptionName
    {
        RebelsSkillCooldown,
        RebelsSkillDuration
    }
    
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.RebelsSkillCooldown, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 13, OptionName.RebelsSkillDuration, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {
        CreateCountdown(OptionSkillDuration.GetFloat());
    }
    public override long UsePetCooldown { get; set; } = (long)OptionSkillCooldown.GetFloat();
    public override bool EnablePetSkill() => true;
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown = OptionSkillCooldown.GetFloat();
        AURoleOptions.EngineerInVentMaxTime = 1f;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("RebelsVetnButtonText");
        return true;
    }
    public void CheckWin(ref CustomWinner WinnerTeam, ref HashSet<byte> WinnerIds)
    {
        if (Player.IsAlive() && WinnerTeam != CustomWinner.Rebels && CheckForOnGuard(0))
        {
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Rebels);
            CustomWinnerHolder.WinnerRoles.Add(CustomRoles.Rebels);
        }
    }
    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {

        ResetCountdown(0);
            if (!Player.IsModClient()) Player.RpcProtectedMurderPlayer(Player);
        Player.RPCPlayCustomSound("Gunload");
        Player.Notify(string.Format(GetString("RebelsOnGuard"),2f));
            return true;
    }
    public override bool GetPetButtonText(out string text)
    {
        text = GetString("RebelsVetnButtonText");
        return PetUnSet();
    }
    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
    {
        Player.RpcResetAbilityCooldown();
    }
}
