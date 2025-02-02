﻿using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using Hazel;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX.Roles.Crewmate;
public sealed class NiceGrenadier : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(NiceGrenadier),
            player => new NiceGrenadier(player),
            CustomRoles.NiceGrenadier,
         () => Options.UsePets.GetBool() ? RoleTypes.Crewmate : RoleTypes.Engineer,
            CustomRoleTypes.Crewmate,
            22000,
            SetupOptionItem,
            "gr|擲雷兵|掷雷|闪光弹",
            "#3c4a16"
        );
    public NiceGrenadier(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        //CustomRoleManager.SuffixOthers.Add(GetSuffixOthers);
        CustomRoleManager.MarkOthers.Add(GetSuffixOthers);
    }

    static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillDuration;
    static OptionItem OptionCanAffectNeutral;
    static OptionItem OptionSkillRange;
    static List<byte> Blinds;
    private static void SendRPC_SyncList()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetNiceGraList, SendOption.Reliable, -1);
        writer.Write(Blinds.Count);
        for (int i = 0; i < Blinds.Count; i++)
            writer.Write(Blinds[i]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC_SyncList(MessageReader reader)
    {
        int count = reader.ReadInt32();
        Blinds = new();
        for (int i = 0; i < count; i++)
            Blinds.Add(reader.ReadByte());
    }
    enum OptionName
    {
        NiceGrenadierSkillCooldown,
        NiceGrenadierSkillDuration,
        NiceGrenadierCanAffectNeutral,
        NiceGrenadierSkillRange,
    }
    public static bool IsBlinding(PlayerControl target)
    {
        if (Blinds.Contains(target.PlayerId) && target.IsAlive())
            return true;
        return false;
    }

    public override void OnGameStart()
    {
        Blinds = new();
    }
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.NiceGrenadierSkillCooldown, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 11, OptionName.NiceGrenadierSkillDuration, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionCanAffectNeutral = BooleanOptionItem.Create(RoleInfo, 13, OptionName.NiceGrenadierCanAffectNeutral, false, false);
        OptionSkillRange = FloatOptionItem.Create(RoleInfo, 14, OptionName.NiceGrenadierSkillRange, new(0f, 50f, 2.5f), 10f, false)
            .SetValueFormat(OptionFormat.Multiplier);
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
        text = GetString("GrenadierVentButtonText");
        return true;
    }
    public override bool GetPetButtonText(out string text)
    {
        text = GetString("GrenadierVentButtonText");
        return PetUnSet();
    }
    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {
        ResetCountdown(0);
        if (Player.Is(CustomRoles.Madmate))
        {
            foreach (var pc in Main.AllAlivePlayerControls.Where(x => !x.IsImpTeam()))
            {

                OnBlinding(pc);
                Player.DisableAction(pc);
            }
        }
        else
        {
            foreach (var pc in Main.AllAlivePlayerControls.Where(x => x.IsImpTeam() || (x.IsNeutral() && OptionCanAffectNeutral.GetBool())))
            {
                OnBlinding(pc);
                Player.DisableAction(pc);
            }
        }
        SendRPC_SyncList();
        if (!Player.IsModClient()) Player.RpcProtectedMurderPlayer();
        Player.RPCPlayCustomSound("FlashBang");
        Player.Notify(GetString("GrenadierSkillInUse"), OptionSkillDuration.GetFloat());
        return true;
    }
    void OnBlinding(PlayerControl pc)
    {
        var posi = Player.transform.position;
        var diss = Vector2.Distance(posi, pc.transform.position);
        if (pc != Player && diss <= OptionSkillRange.GetFloat())
        {
            if (pc.IsModClient())
            {
                pc.RPCPlayCustomSound("FlashBang");

            }
            if (!Blinds.Contains(pc.PlayerId))
            Blinds.Add(pc.PlayerId);
            
        }
    }
    public static void ChangeColorBlindText()
    {
        if (CustomRoles.NiceGrenadier.IsExist() && IsBlinding(PlayerControl.LocalPlayer))
            foreach (var pc in Main.AllAlivePlayerControls)
                pc.cosmetics.colorBlindText.text = $"<size=1000><color=#ffffff>●</color></size>";
    }
    public static string GetSuffixOthers(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        if (IsBlinding(seer))
            return "<size=1000><color=#ffffff>●</color></size>";
        return "";
    }

    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = "Gangstar";
        return true;
    }
    public override bool GetPetButtonSprite(out string buttonName)
    {
        buttonName = "Gangstar";
        return PetUnSet();
    }
}