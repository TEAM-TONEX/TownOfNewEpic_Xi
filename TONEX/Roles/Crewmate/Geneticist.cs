using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using UnityEngine;
using Hazel;
using TONEX.Roles.Core;
using static TONEX.Translator;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static Il2CppSystem.Diagnostics.Tracing.TraceLoggingMetadataCollector;
using Sentry.Internal.Http;
using TONEX.Roles.GameModeRoles;
//职业设计来自星云舰
namespace TONEX.Roles.Crewmate;
public sealed class Geneticist : RoleBase, IKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Geneticist),
            player => new Geneticist(player),
            CustomRoles.Geneticist,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Crewmate,
            94_1_4_0500,
            SetupOptionItem,
            "ge|只因学家",
            "#009999",
            true
        );
    public Geneticist(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
  
    }

    static OptionItem Cooldown;
    public string DNA1;
    public static string DNA2;
    public PlayerControl target1;
    public string Result;
    public bool IsKiller { get; private set; } = false;
    private static void SetupOptionItem()
    {
        Cooldown = FloatOptionItem.Create(RoleInfo, 10,GeneralOption.SkillCooldown, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {
        DNA1 = "";
        DNA2 = "";
        Result = "";
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(DNA1);
    }
    public override void ReceiveRPC(MessageReader reader) => DNA1 = reader.ReadString();
    private static void SendRPC_DNA2()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetGeneticistDNA2, SendOption.Reliable, -1);
        writer.Write(DNA2);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC_DNA2(MessageReader reader) => DNA2 = reader.ReadString();
    public bool OverrideKillButtonText(out string text)
    {
        text = GetString("GeneticistButtonText");
        return true;
    }
    public float CalculateKillCooldown() => CanUseKillButton() ? Cooldown.GetFloat() : 255f;
    public bool CanUseKillButton() => Player.IsAlive();
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(false);
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        killer.SetKillCooldownV2(target: target);
        if (DNA1 == "")
        {
            DNA1 = target.GetRealName();
            target1 = target;
            SendRPC();
            Utils.NotifyRoles(killer);
        }
        else if (DNA1 != "" && target == target1)
            Player.Notify(GetString("DoNotExtractDNAAnymore"));
        else if (DNA1!="" && target!=target1 && DNA2=="")
        {
            DNA2 = target.GetRealName();
            SendRPC_DNA2();
            Utils.NotifyRoles(killer);
            if (target.GetCountTypes() == target1.GetCountTypes())
                Result = GetString("Match");
            else
                Result = GetString("MisMatch");
        }

        info.CanKill = false;
        return false;
    }
    public override void NotifyOnMeetingStart(ref List<(string, byte, string)> msgToSend)
    {
        if(Result!="")
            msgToSend.Add((GetString("DNAMatching") + Result, Player.PlayerId, null));
    }
    public override void AfterMeetingTasks()
    {
        DNA1 = "";
        DNA2 = "";
        Result = "";
        SendRPC();
        SendRPC_DNA2();
        Utils.NotifyRoles(Player);
    }
    public override string GetProgressText(bool comms = false) => Utils.ColorString(CanUseKillButton() ? RoleInfo.RoleColor : Color.gray, string.Format(GetString("Sample"), DNA1, DNA2));
}