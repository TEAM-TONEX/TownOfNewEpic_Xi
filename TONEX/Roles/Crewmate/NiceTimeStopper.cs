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
    private float Cooldown;
    public override long UsePetCooldown { get; set; } = (long)OptionSkillCooldown.GetFloat();
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
    public override List<long> CooldownList { get; set; } = new();
    public override List<long> CountdownList { get; set; } = new();

    public override bool SetOffGuardProtect(out string notify, out int format_int, out float format_float)
    {
        notify = GetString("NiceTimeStopperOffGuard");
        format_int = -255;
        format_float = -255;
        return true;
    }
    public override void Add()
    {
        Cooldown = OptionSkillCooldown.GetFloat();
        CooldownList.Add((long)OptionSkillDuration.GetFloat());
        CountdownList.Add(-1);
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
        UsePetCooldown = (long)Cooldown;
    }
    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {
        ReduceNowCooldown();
        Player.SyncSettings();
        Player.RpcResetAbilityCooldown();
        CountdownList[0] = Utils.GetTimeStamp();
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
    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        if (NiceTimeStopperstop.Contains(reporter.PlayerId))    
            return false;
        return true;
    }
    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
    {
        Player.RpcResetAbilityCooldown();
    }
}
