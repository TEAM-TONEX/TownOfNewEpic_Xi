using AmongUs.GameOptions;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Translator;
using Hazel;
using System.Collections.Generic;
using TONEX.Roles.Neutral;
using TONEX.Modules.SoundInterface;
using System.Linq;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Impostor;
public sealed class EvilTimeStopper : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(EvilTimeStopper),
            player => new EvilTimeStopper(player),
            CustomRoles.EvilTimeStopper,
            () => Options.UsePets.GetBool() ? RoleTypes.Impostor : RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            75_1_2_0300,
            SetupOptionItem,
            "shi|时停"
        );
    public EvilTimeStopper(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        EvilTimeStopperstop = new();
    }

    static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillDuration;
    enum OptionName
    {
        NiceTimeStopperSkillCooldown,
        NiceTimeStopperSkillDuration,
    }
    private List<byte> EvilTimeStopperstop;
    private long ProtectStartTime;
    private float Cooldown;
    
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.NiceTimeStopperSkillCooldown, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 13, OptionName.NiceTimeStopperSkillDuration, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        
    }
    public override void Add()
    {
        ProtectStartTime = -1;
        Cooldown = OptionSkillCooldown.GetFloat();
        
    }
    public override long UsePetCooldown { get; set; } = (long)OptionSkillCooldown.GetFloat();
    public override bool EnablePetSkill() => true;
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

    public override bool OnCheckShapeshift(PlayerControl target, ref bool animate)
    {
        Player.SyncSettings();
        Player.RpcResetAbilityCooldown();
        ProtectStartTime = Utils.GetTimeStamp();
        
        foreach (var pc in Main.AllPlayerControls.Where(p => p.IsImpTeam()))
            pc.Notify(GetString("NiceTimeStopperOnGuard"));
        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (Player == player) continue;
            if (!player.IsAlive() || Pelican.IsEaten(player.PlayerId)) continue;
            NameNotifyManager.Notify(player, Utils.ColorString(Utils.GetRoleColor(CustomRoles.NiceTimeStopper), GetString("ForNiceTimeStopper")));
            EvilTimeStopperstop.Add(player.PlayerId);
            Player.DisableAction(player, ExtendedPlayerControl.PlayerActionType.All);


            new LateTask(() =>
            {
                Player.EnableAction(player, ExtendedPlayerControl.PlayerActionType.All);
                EvilTimeStopperstop.Remove(player.PlayerId);
                RPC.PlaySoundRPC(player.PlayerId, Sounds.TaskComplete);
            }, OptionSkillDuration.GetFloat(), "Time Stopper");
        }
        return false;
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
    public override void OnUsePet()
    {
        Player.SyncSettings();
        Player.RpcResetAbilityCooldown();
        ProtectStartTime = Utils.GetTimeStamp();
        
        foreach (var pc in Main.AllPlayerControls.Where(p => p.IsImpTeam()))
            pc.Notify(GetString("NiceTimeStopperOnGuard"));
        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (Player == player) continue;
            if (!player.IsAlive() || Pelican.IsEaten(player.PlayerId)) continue;
            NameNotifyManager.Notify(player, Utils.ColorString(Utils.GetRoleColor(CustomRoles.NiceTimeStopper), GetString("ForNiceTimeStopper")));
            EvilTimeStopperstop.Add(player.PlayerId);
            Player.DisableAction(player, ExtendedPlayerControl.PlayerActionType.All);


            new LateTask(() =>
            {
                Player.EnableAction(player, ExtendedPlayerControl.PlayerActionType.All);
                EvilTimeStopperstop.Remove(player.PlayerId);
                RPC.PlaySoundRPC(player.PlayerId, Sounds.TaskComplete);
            }, OptionSkillDuration.GetFloat(), "Time Stopper");
        }
    }
    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        if (EvilTimeStopperstop.Contains(reporter.PlayerId))    return false;
        return true;
    }
    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
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
