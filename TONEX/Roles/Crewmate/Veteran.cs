using AmongUs.GameOptions;
using UnityEngine;
using TONEX.Roles.Core;
using static TONEX.Translator;
using Hazel;
using static UnityEngine.GraphicsBuffer;
using TONEX.Modules.SoundInterface;
using System.Collections.Generic;

namespace TONEX.Roles.Crewmate;
public sealed class Veteran : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Veteran),
            player => new Veteran(player),
            CustomRoles.Veteran,
         () => Options.UsePets.GetBool() ? RoleTypes.Crewmate : RoleTypes.Engineer,
            CustomRoleTypes.Crewmate,
            21800,
            SetupOptionItem,
            "ve",
            "#a77738"
        );
    public Veteran(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillDuration;
    static OptionItem OptionSkillNums;
    enum OptionName
    {
        VeteranSkillCooldown,
        VeteranSkillDuration,
        VeteranSkillMaxOfUseage,
    }


    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.VeteranSkillCooldown, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 11, OptionName.VeteranSkillDuration, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSkillNums = IntegerOptionItem.Create(RoleInfo, 12, OptionName.VeteranSkillMaxOfUseage, new(1, 99, 1), 5, false)
            .SetValueFormat(OptionFormat.Times);
    }
    public override long UsePetCooldown { get; set; } = (long)OptionSkillCooldown.GetFloat();
    public override bool EnablePetSkill() => true;
    private int SkillLimit;
    private long ProtectStartTime;
    public override List<long> CooldownList { get; set; } = new();
    public override List<long> CountdownList { get; set; } = new();

    public override void CD_Update()
    {
        ProtectStartTime = CountdownList[0];
    }

    public override bool SetOffGuardProtect(out string notify, out int format_int, out float format_float)
    {
        notify = GetString("VeteranOffGuard");
        format_int = SkillLimit;
        format_float = -255;
        return true;
    }

    public override void Add()
    {
        SkillLimit = OptionSkillNums.GetInt();
        ProtectStartTime = -1;
        CooldownList.Add((long)OptionSkillDuration.GetFloat());
        CountdownList.Add(ProtectStartTime);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown =
            SkillLimit <= 0
            ? 255f
            : OptionSkillCooldown.GetFloat();
        AURoleOptions.EngineerInVentMaxTime = 1f;
    }
    public override int OverrideAbilityButtonUsesRemaining() => SkillLimit;
   
    public override bool GetGameStartSound(out string sound)
 {
 sound = "Gunload";
        return true;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("VeteranVetnButtonText");
        return true;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = "Veteran";
        return true;
    }
    public override bool CanUseAbilityButton() => SkillLimit >= 1;
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(SkillLimit);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        SkillLimit = reader.ReadInt32();
    }

    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {
        if (SkillLimit >= 1)
        {
            SkillLimit--;
            SendRPC();
            ResetCountdown(0);
            if (!Player.IsModClient()) Player.RpcProtectedMurderPlayer(Player);
            Player.RPCPlayCustomSound("Gunload");
            Player.Notify(string.Format(GetString("VeteranOnGuard"), SkillLimit, 2f));
            return true;
        }
        else
        {
            Player.Notify(GetString("SkillMaxUsage"));
            return false;
        }
    }
    public override bool GetPetButtonText(out string text)
    {
        text = GetString("VeteranVetnButtonText");
        return PetUnSet();
    }
    public override bool GetPetButtonSprite(out string buttonName)
    {
        buttonName = "Veteran";
        return PetUnSet() || SkillLimit >=1;
    }
    public override bool OnCheckMurderAsTargetAfter(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        if (CheckForOnGuard(0))
        {
            var (killer, target) = info.AttemptTuple;
            target.RpcMurderPlayerV2(killer);
            Logger.Info($"{target.GetRealName()} 老兵反弹击杀：{killer.GetRealName()}", "Veteran.OnCheckMurderAsTargetAfter");
            return false;
        }
        return true;
    }
    public override void OnExileWrapUp(GameData.PlayerInfo exiled, ref bool DecidedWinner)
    {
        Player.RpcResetAbilityCooldown();
    }
    public override string GetProgressText(bool comms = false) => Utils.ColorString(SkillLimit >= 1 ? Utils.GetRoleColor(CustomRoles.Veteran) : Color.gray, $"({SkillLimit})");
}