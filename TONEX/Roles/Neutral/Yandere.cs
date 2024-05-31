using AmongUs.GameOptions;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;
using HarmonyLib;
using InnerNet;
using System;
using static UnityEngine.GraphicsBuffer;
using MS.Internal.Xml.XPath;

namespace TONEX.Roles.Neutral;
public sealed class Yandere : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Yandere),
            player => new Yandere(player),
            CustomRoles.Yandere,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            94_1_2_0400,
            SetupOptionItem,
            "ya",
            "#ff25e4",
            true,
            true,
            countType: CountTypes.Yandere
        );
    public Yandere(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionContactTime;
    private static OptionItem OptionContactRange;
    public bool IsNK { get; private set; } = true;
    enum OptionName
    {
        OptionContactTime,
        OptionContactRange
    }
    public PlayerControl TargetId;
    public static List<PlayerControl> Targets = new();
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionContactTime = FloatOptionItem.Create(RoleInfo, 11, OptionName.OptionContactTime, new(0, 20, 1), 3, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionContactRange = FloatOptionItem.Create(RoleInfo, 12, OptionName.OptionContactRange, new(0.5f, 5f, 0.25f), 2.5f, false)
           .SetValueFormat(OptionFormat.Multiplier);

    }
      List<byte> NeedKill;
    private static Dictionary<byte, float> Infos;
    public override void Add()
    {
        //ターゲット割り当て
        if (!AmongUsClient.Instance.AmHost) return;
        NeedKill = new();
        Targets = new();
        Infos = new();
        foreach (var pc in Main.AllPlayerControls)
            Infos.TryAdd(pc.PlayerId, 0);
        var playerId = Player.PlayerId;
        var pcList = Main.AllAlivePlayerControls.Where(x => x.PlayerId != Player.PlayerId && x.IsAlive() && CanBeLover(x)).ToList();
        var SelectedTarget = pcList[IRandom.Instance.Next(0, pcList.Count)];
        TargetId = SelectedTarget;
        Targets.Add(SelectedTarget);
        NameColorManager.Add(Player.PlayerId, SelectedTarget.PlayerId, "#ff25e4");
        SendRPC();
    }
    private void SendRPC()
    {
        var sender = CreateSender();
        sender.Writer.Write(TargetId.PlayerId);
        sender.Writer.Write(NeedKill.Count);
        foreach (var pc in NeedKill)
            sender.Writer.Write(pc);
        sender.Writer.Write(Infos.Count);
        foreach (var pc in Infos)
        {
            sender.Writer.Write(pc.Key);
            sender.Writer.Write(pc.Value);
        }
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        TargetId = Utils.GetPlayerById(reader.ReadByte());
        NeedKill = new();
        
        var nkc = reader.ReadInt32();
        for (int i = 0; i <nkc; i++)
            NeedKill.Add(reader.ReadByte());

        Infos = new();
        var ic = reader.ReadInt32();
        for (int i = 0; i < ic; i++)
        {
            var id = reader.ReadByte();
            var rate = reader.ReadSingle();
            Infos.TryAdd(id, rate);
        }
    }
    public static bool CanBeLover(PlayerControl pc) => pc != null && (
    !(pc.Is(CustomRoles.LazyGuy)
    || pc.Is(CustomRoles.Neptune)
    || pc.Is(CustomRoles.God)
    || pc.Is(CustomRoles.Hater)
    || pc.Is(CustomRoles.Believer)
    || pc.Is(CustomRoles.Nihility)
//    || pc.Is(CustomRoles.Lovers)
//    || pc.Is(CustomRoles.CupidLovers)
//    || pc.Is(CustomRoles.AkujoLovers)
//    || pc.Is(CustomRoles.Akujo)
//    || pc.Is(CustomRoles.Cupid)
    || pc.Is(CustomRoles.Yandere)
//    || pc.Is(CustomRoles.Admirer)
//    || pc.Is(CustomRoles.AdmirerLovers)
        ));
    public override string GetMark(PlayerControl seer, PlayerControl seen, bool _ = false)
    {
        //seenが省略の場合seer
        seen ??= seer;

        return TargetId.PlayerId == seen.PlayerId ? Utils.ColorString(RoleInfo.RoleColor, "♡") : "";
    }
    public override string GetSuffix(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        if (isForMeeting) return "";
        seen ??= seer;
        if (!Is(seer) && !Is(seen)) return "";

        var arrows = "";
        foreach (var targetId in NeedKill) arrows += TargetArrow.GetArrows(seer, targetId);

        return arrows;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var pc in Main.AllAlivePlayerControls)
        {
            var posi = pc.GetTruePosition();

            var dis = Vector2.Distance(TargetId.GetTruePosition(), posi);
            if (dis > OptionContactRange.GetFloat() || NeedKill.Contains(pc.PlayerId)) continue;
            Infos.TryGetValue(pc.PlayerId, out var oldRate);
            var newRate = oldRate + Time.fixedDeltaTime / OptionContactTime.GetFloat() * 100;
            newRate = Math.Clamp(newRate, 0, 100);
            Infos[pc.PlayerId] = newRate;
            if ((oldRate < 25 && newRate >= 25) || (oldRate < 50 && newRate >= 50))
            {
                SendRPC();
            }
            if (newRate >= 100)
            {
                NeedKill.Add(pc.PlayerId);
                Infos.Remove(pc.PlayerId);
                SendRPC();
            }
            
            
        }
        
        
    }
    public override void OnPlayerDeath(PlayerControl player, CustomDeathReason deathReason, bool isOnMeeting)
    {
        var pc = player;
        if (TargetId==pc)
        {
           Player.RpcMurderPlayerV2(Player);
            NeedKill.Clear();
            SendRPC();
            foreach (var targetId in NeedKill)   TargetArrow.Remove(Player.PlayerId, targetId);
            Utils.NotifyRoles(Player);
        }
    }
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
           if(!NeedKill.Contains(target.PlayerId)) return false;
        return true;
    }
    public float CalculateKillCooldown() => OptionKillCooldown.GetFloat();
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(false);
    public void ApplySchrodingerCatOptions(IGameOptions option) => ApplyGameOptions(option);
}