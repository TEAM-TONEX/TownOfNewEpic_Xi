using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using UnityEngine;
using TONEX.Modules;
using TONEX.Roles.Core;
using static TONEX.Translator;
using TONEX.Roles.Core.Interfaces;
using MS.Internal.Xml.XPath;


namespace TONEX.Roles.Crewmate;

public sealed class Instigator : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
        typeof(Instigator),
        player => new Instigator(player),
        CustomRoles.Instigator,
         () => Options.UsePets.GetBool() ? RoleTypes.Crewmate : RoleTypes.Engineer,
        CustomRoleTypes.Crewmate,
        22568,
        SetupOptionItem,
        "ru",
        "#66ff00"
    );
    public Instigator(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        ForInstigator = new();
    }

        static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillDuration;
    static OptionItem ReduceCooldown;
    static OptionItem MaxCooldown;
    enum OptionName
    {
        NiceTimeStopperSkillCooldown,
        NiceTimeStopperSkillDuration,
        ReduceCooldown,
        MaxCooldown,
    }
   public static List<byte> ForInstigator;
    private long ProtectStartTime;
    private float Cooldown;
    
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.SkillCooldown, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        ReduceCooldown = FloatOptionItem.Create(RoleInfo, 11, OptionName.ReduceCooldown, new(2.5f, 180f, 2.5f), 10f, false)
    .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 13, GeneralOption.SkillDuration, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        MaxCooldown = FloatOptionItem.Create(RoleInfo, 12, OptionName.MaxCooldown, new(2.5f, 250f, 2.5f), 60f, false)
          .SetValueFormat(OptionFormat.Seconds);
    }
    public override long UsePetCoolDown_Totally { get; set; } = (long)OptionSkillCooldown.GetFloat();
    public override bool EnablePetSkill() => true;
    public override void Add()
    {
        ProtectStartTime = -1;
        Cooldown = OptionSkillCooldown.GetFloat();
        ForInstigator = new();
        
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown = Cooldown;
        AURoleOptions.EngineerInVentMaxTime = 1f;
    }
    
public override void OnGameStart()
{
    
}
public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("InstigatorVetnButtonText");
        return true;
    }
    public override bool GetPetButtonText(out string text)
    {
        text = GetString("InstigatorVetnButtonText");
        return PetUnSet();
    }
    public void CheckWin(ref CustomWinner WinnerTeam, ref HashSet<byte> WinnerIds)
    {
        if (ForInstigator.Count != 0)
        {
            foreach(var item in ForInstigator)
            {
                if (WinnerIds.Contains(item))   CustomWinnerHolder.WinnerIds.Remove(item);
            }
        }
    }
    public void ReduceNowCooldown()
    {
        Cooldown = Cooldown + ReduceCooldown.GetFloat();
        if (Cooldown > MaxCooldown.GetFloat()) Cooldown -= ReduceCooldown.GetFloat();
    }
    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {
        ReduceNowCooldown();
        Player.SyncSettings();
        ProtectStartTime = Utils.GetTimeStamp();
        if (!Player.IsModClient()) Player.RpcProtectedMurderPlayer(Player);
        Player.Notify(GetString("InstigatorOnGuard"),2f);
        return true;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var now = Utils.GetTimeStamp();
        if (ProtectStartTime + (long)OptionSkillDuration.GetFloat() < now && ProtectStartTime != -1)
        {
            ProtectStartTime = -1;
            player.RpcProtectedMurderPlayer();
            player.Notify(string.Format(GetString("NiceTimeStopperOffGuard")));
        }
    }
    public override bool OnCheckMurderAsTargetAfter(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        if (ProtectStartTime != -1 && ProtectStartTime + (long)OptionSkillDuration.GetFloat() >= Utils.GetTimeStamp())
        {
            var (killer, target) = info.AttemptTuple;
            target.RpcMurderPlayerV2(killer);
            killer.RpcMurderPlayerV2(target);
            ForInstigator.Add(killer.PlayerId);
            return false;
        }
        return true;
    }
    public override void OnExileWrapUp(GameData.PlayerInfo exiled, ref bool DecidedWinner)
    {
        Player.RpcResetAbilityCooldown();
    }
    public override void AfterMeetingTasks()
    {
        
    }
    public override void OnStartMeeting()
    {
        ProtectStartTime = -1;
    }
}