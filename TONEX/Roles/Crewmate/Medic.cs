﻿using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace TONEX.Roles.Crewmate;
public sealed class Medic : RoleBase, IKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Medic),
            player => new Medic(player),
            CustomRoles.Medic,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Crewmate,
            22100,
            SetupOptionItem,
            "me|醫生",
            "#80ffdd",
            true
        );
    public Medic(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        ProtectList = new();
        ProtectLimit = OptionProtectNums.GetInt();
        CustomRoleManager.MarkOthers.Add(GetMarkOthers);
        CustomRoleManager.OnCheckMurderPlayerOthers_Before.Add(OnCheckMurderPlayerOthers_Before);
    }

    static OptionItem OptionProtectNums;
    static OptionItem OptionProtectCooldown;
    static OptionItem OptionSeenShieldMode;
    static OptionItem OptionKnowTargetShieldBroken;
    static OptionItem OptionMedicShieldMode;
    enum OptionName
    {
        MedicCooldown,
        MedicSkillLimit,
        MedicSeenShieldMode,
        MedicKnowTargetShieldBroken,
        MedicShieldMode,
    }
    public enum SwitchShieldMode
    {
        ForOnce,
        ForMedicAlive,
        Forever,
    };
    public enum SwitchSeenShieldMode
    {
        ForMedic,
        ForTargetAndMedic,
        ForEveryOne,
    };
    private int ProtectLimit;
    private static List<byte> ProtectList;
    public bool IsKiller { get; private set; } = false;
    private static void SetupOptionItem()
    {
        OptionProtectCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.MedicCooldown, new(2.5f, 180f, 2.5f), 5f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionProtectNums = IntegerOptionItem.Create(RoleInfo, 11, OptionName.MedicSkillLimit, new(1, 99, 1), 3, false)
            .SetValueFormat(OptionFormat.Times);
        OptionSeenShieldMode = StringOptionItem.Create(RoleInfo, 12, OptionName.MedicSeenShieldMode, EnumHelper.GetAllNames<SwitchSeenShieldMode>(), 2, false);
        OptionKnowTargetShieldBroken = BooleanOptionItem.Create(RoleInfo, 13, OptionName.MedicKnowTargetShieldBroken, true, false);
        OptionMedicShieldMode = StringOptionItem.Create(RoleInfo, 14, OptionName.MedicShieldMode, EnumHelper.GetAllNames<SwitchShieldMode>(), 1, false);
    }
    private void SendRPC_SyncLimit()
    {
        using var sender = CreateSender();
        sender.Writer.Write(ProtectLimit);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        ProtectLimit = reader.ReadInt32();
    }
    private static void SendRPC_SyncList()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetMedicProtectList, SendOption.Reliable, -1);
        writer.Write(ProtectList.Count);
        for (int i = 0; i < ProtectList.Count; i++)
            writer.Write(ProtectList[i]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC_SyncList(MessageReader reader)
    {
        int count = reader.ReadInt32();
        ProtectList = new();
        for (int i = 0; i < count; i++)
            ProtectList.Add(reader.ReadByte());
    }
    public bool OverrideKillButtonText(out string text)
    {
        text = Translator.GetString("MedicButtonText");
        return true;
    }
    public bool OverrideKillButtonSprite(out string buttonName)
    {
        buttonName = "Shield";
        return true;
    }
    public float CalculateKillCooldown() => CanUseKillButton() ? OptionProtectCooldown.GetFloat() : 255f;
    public bool CanUseKillButton()
       => Player.IsAlive()
       && ProtectLimit > 0;
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(false);
    public override string GetProgressText(bool comms = false) => Utils.ColorString(CanUseKillButton() ? RoleInfo.RoleColor : Color.gray, $"({ProtectLimit})");
    public static bool InProtect(byte id) => ProtectList.Contains(id) && !(PlayerState.GetByPlayerId(id)?.IsDead ?? true);
    public override bool GetGameStartSound(out string sound)
    {
        sound = "Shield";
        return true;
    }
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        var (killer, target) = info.AttemptTuple;
        if (ProtectList.Contains(target.PlayerId)) return false;

        ProtectLimit--;
        SendRPC_SyncLimit();
        ProtectList.Add(target.PlayerId);
        SendRPC_SyncList();

        killer.SetKillCooldownV2(target: target);
        killer.RPCPlayCustomSound("Shield");
        target.RPCPlayCustomSound("Shield");

        Utils.NotifyRoles(killer);
        Utils.NotifyRoles(target);

        Logger.Info($"{killer.GetNameWithRole()} : 将护盾发送给 {target.GetNameWithRole()}", "Medic.OnCheckMurderAsKiller");
        Logger.Info($"{killer.GetNameWithRole()} : 剩余{ProtectLimit}个护盾", "Medic.OnCheckMurderAsKiller");
        return false;
    }
    private static bool OnCheckMurderPlayerOthers_Before(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        var (killer, target) = info.AttemptTuple;
        if (!ProtectList.Contains(target.PlayerId)) return true;
        if (OptionMedicShieldMode.GetInt() == 0)
        {
            ProtectList.Remove(target.PlayerId);
            SendRPC_SyncList();
            Logger.Info($"{target.GetNameWithRole()} : 来自医生的盾破碎", "Medic.OnCheckMurderPlayerOthers_Before");
        }
        killer.SetKillCooldownV2(target: target, forceAnime: true);
        killer.RpcProtectedMurderPlayer(target);
        info.CanKill = false;
        if (OptionSeenShieldMode.GetInt() == 1 || OptionSeenShieldMode.GetInt() == 2)
            target.KillFlash();
        if (OptionKnowTargetShieldBroken.GetBool())
            Main.AllPlayerControls.Where(x => x.Is(CustomRoles.Medic) && x.PlayerId != target.PlayerId).Do(x => 
            { 
                x.KillFlash();
                x.Notify(Translator.GetString("MedicTargetShieldBroken"));
            });
        else
            Main.AllPlayerControls.Where(x => x.Is(CustomRoles.Medic)).Do(x => Utils.NotifyRoles(x));
        Utils.NotifyRoles(target);
        return false;
    }
    public static string GetMarkOthers(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
    {
        seen ??= seer;
        if (!InProtect(seen.PlayerId)) return "";
        return (seer.Is(CustomRoles.Medic)
            || (seer == seen && OptionSeenShieldMode.GetInt() == 1) || OptionSeenShieldMode.GetInt() == 2
            ) ? Utils.ColorString(RoleInfo.RoleColor, "●") : "";
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!Player.IsAlive() && OptionMedicShieldMode.GetInt() == 1)
        {
            List<PlayerControl> playersToRemove = new();

            foreach (var pc in ProtectList)
            {
                playersToRemove.Add(Utils.GetPlayerById(pc));

                if (OptionSeenShieldMode.GetInt() == 1 || OptionSeenShieldMode.GetInt() == 2)
                    Utils.GetPlayerById(pc).RpcProtectedMurderPlayer(Utils.GetPlayerById(pc));

                Logger.Info($"{Utils.GetPlayerById(pc).GetNameWithRole()} : 来自医生的盾破碎", "Medic.OnCheckMurderPlayerOthers_Before");
            }

            foreach (var pc in playersToRemove)
            {
                ProtectList.Remove(pc.PlayerId);
            }

            SendRPC_SyncList();
        }
    }

}