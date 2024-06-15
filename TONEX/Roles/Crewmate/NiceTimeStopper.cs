using AmongUs.GameOptions;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Translator;
using Hazel;
using System.Collections.Generic;
using TONEX.Roles.Neutral;
using TONEX.Modules.SoundInterface;

namespace TONEX.Roles.Crewmate;
public sealed class NiceTimeStopper : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(NiceTimeStopper),
            player => new NiceTimeStopper(player),
            CustomRoles.NiceTimeStopper,
            () => Options.UsePets.GetBool() ? RoleTypes.Crewmate : RoleTypes.Engineer,
            CustomRoleTypes.Crewmate,
            15546396,
            SetupOptionItem,
            "shi|时停",
            "#f6f657"
        );
    public NiceTimeStopper(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        NiceTimeStopperstop = new();
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
    private List<byte> NiceTimeStopperstop;
    private long ProtectStartTime;
    private float Cooldown;
    public override long UsePetCoolDown_Totally { get; set; } = (long)OptionSkillCooldown.GetFloat();
    public override bool EnablePetSkill() => true;
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.NiceTimeStopperSkillCooldown, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        ReduceCooldown = FloatOptionItem.Create(RoleInfo, 11, OptionName.ReduceCooldown, new(2.5f, 180f, 2.5f), 10f, false)
    .SetValueFormat(OptionFormat.Seconds);
        MaxCooldown = FloatOptionItem.Create(RoleInfo, 12, OptionName.MaxCooldown, new(2.5f, 250f, 2.5f), 60f, false)
  .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 13, OptionName.NiceTimeStopperSkillDuration, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        
    }
    public override void Add()
    {
        ProtectStartTime = -1;
        Cooldown = OptionSkillCooldown.GetFloat();
        
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown = Cooldown;
        AURoleOptions.EngineerInVentMaxTime = 1f;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("NiceTimeStopperVetnButtonText");
        return true;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = "TheWorld";
        return true;
    }
    public override bool GetPetButtonSprite(out string buttonName)
    {
        buttonName = "TheWorld";
        return PetUnSet();
    }
    public override bool GetPetButtonText(out string text)
    {
        text = GetString("NiceTimeStopperVetnButtonText");
        return PetUnSet();
    }
    public override bool GetGameStartSound(out string sound)
    {
        sound = "TheWorld";
        return true;
    }

    public void ReduceNowCooldown()
    {
        Cooldown = Cooldown + ReduceCooldown.GetFloat();
        if (Cooldown > MaxCooldown.GetFloat())Cooldown -= ReduceCooldown.GetFloat();    
    }
    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {
        ReduceNowCooldown();
        Player.SyncSettings();
        Player.RpcResetAbilityCooldown();
        ProtectStartTime = Utils.GetTimeStamp();
        if (!Player.IsModClient()) Player.RpcProtectedMurderPlayer(Player);
        Player.Notify(GetString("NiceTimeStopperOnGuard"));
        Player.ColorFlash(Utils.GetRoleColor(CustomRoles.SchrodingerCat));
        CustomSoundsManager.RPCPlayCustomSoundAll("TheWorld");
        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (Player == player) continue;
            if (!player.IsAlive() || Pelican.IsEaten(player.PlayerId)) continue;
            NameNotifyManager.Notify(player, Utils.ColorString(Utils.GetRoleColor(CustomRoles.NiceTimeStopper), GetString("ForNiceTimeStopper")));
            NiceTimeStopperstop.Add(player.PlayerId);
            Player.DisableAction(player, ExtendedPlayerControl.PlayerActionType.All);


            new LateTask(() =>
            {
                Player.EnableAction(player, ExtendedPlayerControl.PlayerActionType.All);
                NiceTimeStopperstop.Remove(player.PlayerId);
                RPC.PlaySoundRPC(player.PlayerId, Sounds.TaskComplete);
            }, OptionSkillDuration.GetFloat(), "Time Stopper");
        }
        return true;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var now = Utils.GetTimeStamp();
        if (Player.IsAlive() && ProtectStartTime + (long)OptionSkillDuration.GetFloat() < now && ProtectStartTime != -1)
        {
            ProtectStartTime = -1;
            player.RpcProtectedMurderPlayer();
            player.Notify(string.Format(GetString("NiceTimeStopperOffGuard")));
        }
    }
    public override bool OnCheckReportDeadBody(PlayerControl reporter, GameData.PlayerInfo target)
    {
        if (NiceTimeStopperstop.Contains(reporter.PlayerId))    
            return false;
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
