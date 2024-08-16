using AmongUs.GameOptions;
using TONEX.Roles.Core;
using System;
using static TONEX.Translator;
using UnityEngine;
using Hazel;
using System.Collections.Generic;
using TONEX.Modules.SoundInterface;

namespace TONEX.Roles.Crewmate;
public sealed class TimeMaster : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(TimeMaster),
            player => new TimeMaster(player),
            CustomRoles.TimeMaster,
            () => Options.UsePets.GetBool() ? RoleTypes.Crewmate : RoleTypes.Engineer,
            CustomRoleTypes.Crewmate,
            226312,
            SetupOptionItem,
            "zhu|时主",
            "#44baff"
        );
    public TimeMaster(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Marked = false;
        TimeMasterbacktrack = new();
    }
    private bool Marked;
    static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillDuration;
    static OptionItem ReduceCooldown;
    static OptionItem MaxCooldown;
    enum OptionName
    {
        TimeMasterSkillCooldown,
        TimeMasterSkillDuration,
        ReduceCooldown,
        MaxCooldown,
    }
    public static Dictionary<byte, Vector2> TimeMasterbacktrack = new();
    private float Cooldown;
    public override long UsePetCooldown { get; set; } = (long)OptionSkillCooldown.GetFloat();
    public override bool EnablePetSkill() => true;
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 14, OptionName.TimeMasterSkillCooldown, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        ReduceCooldown = FloatOptionItem.Create(RoleInfo, 11, OptionName.ReduceCooldown, new(2.5f, 180f, 2.5f), 10f, false)
    .SetValueFormat(OptionFormat.Seconds);
        MaxCooldown = FloatOptionItem.Create(RoleInfo, 12, OptionName.MaxCooldown, new(2.5f, 250f, 2.5f), 60f, false)
  .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 13, OptionName.TimeMasterSkillDuration, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {
        Marked = false;
        Cooldown = OptionSkillCooldown.GetFloat();
        TimeMasterbacktrack = new();
        CreateCountdown(OptionSkillDuration.GetFloat());

    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown = Cooldown;
        AURoleOptions.EngineerInVentMaxTime = 1f;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("TimeMasterVetnButtonText");
        return true;
    }
    public override bool GetPetButtonText(out string text)
    {
        text = GetString("TimeMasterVetnButtonText");
        return PetUnSet();
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(Marked);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        Marked = reader.ReadBoolean();
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
        ResetCountdown(0);
        if (!Player.IsModClient()) Player.RpcProtectedMurderPlayer(Player);
        Player.Notify(GetString("TimeMasterOnGuard"));
        foreach (var player in Main.AllPlayerControls)
        {
            Player.DisableAction(player);
            if (TimeMasterbacktrack.ContainsKey(player.PlayerId))
            {
                player.RPCPlayCustomSound("Teleport");
                var position = TimeMasterbacktrack[player.PlayerId];
                player.RpcTeleport(position);
                TimeMasterbacktrack.Remove(player.PlayerId);
            }
            else
            {
                TimeMasterbacktrack.Add(player.PlayerId, player.GetTruePosition());
                SendRPC();
                Marked = true;
            }
        }
        return true;
    }
    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
    {
        Player.RpcResetAbilityCooldown();
    }
    public override bool OnCheckMurderAsTargetAfter(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        if (CheckForOnGuard(0) && Marked)
        {
            var (killer, target) = info.AttemptTuple;
            foreach (var player in Main.AllPlayerControls)
            {
                Player.DisableAction(player);
                if (TimeMasterbacktrack.ContainsKey(player.PlayerId))
                {
                    var position = TimeMasterbacktrack[player.PlayerId];
                    player.RpcTeleport(position);
                }
            }
            killer.SetKillCooldownV2(target: target, forceAnime: true);
            return false;
        }
        return true;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = "KingOfTime";
        return true;
    }
    public override bool GetPetButtonSprite(out string buttonName)
    {
        buttonName = "KingOfTime";
        return PetUnSet();
    }
}