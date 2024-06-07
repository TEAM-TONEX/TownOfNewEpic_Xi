using AmongUs.GameOptions;
using static TONEX.Translator;
using TONEX.Roles.Core;
using UnityEngine;
using MS.Internal.Xml.XPath;
using static UnityEngine.GraphicsBuffer;
using TONEX.Roles.Neutral;
using System.Collections.Generic;
using Hazel;
using static Il2CppSystem.Net.Http.Headers.Parser;
using TONEX.Modules;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using TONEX.Roles.Core.Interfaces;
using System.Linq;

namespace TONEX.Roles.Neutral;

public sealed class MeteorArbiter : RoleBase, INeutralKiller, IAdditionalWinner
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(MeteorArbiter),
            player => new MeteorArbiter(player),
            CustomRoles.MeteorArbiter,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            75_1_2_0100,
            SetupOptionItem,
            "Sans|MeteorArbiter|USF!Sans",
             "#C0EAFF",
            true,
            true,
            countType: CountTypes.MeteorArbiter,
            assignCountRule: new(1, 1, 1)
#if RELEASE
,
ctop:true
#endif
        );
    public MeteorArbiter(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        Murderer = false;
        Crumble = false;
        LOVE = 1;
        Tired = 0;
        LVOverFlow = 0;
        HealthPoint = 0;
    }
    static OptionItem GetLoveCountByKillImp;
    static OptionItem GetLoveCountByKillCrew;
    static OptionItem GetLoveCountByKillNK;
    static OptionItem GetLoveCountByKillNeu;
    static OptionItem OptionCanGetLoveByReport;
    static OptionItem GetLoveCountByReportImp;
    static OptionItem GetLoveCountByReportCrew;
    static OptionItem GetLoveCountByReportNK;
    static OptionItem GetLoveCountByReportNeu;
    static OptionItem GetHPCountByLVOverFlow;
    static OptionItem OptionHasImpostorVision;
    static OptionItem SuccessKillProbability;
    static OptionItem SuccessMissProbability;
    static OptionItem ReduceMissProbabilityByLV;
    static OptionItem CanFollowWin;
    static OptionItem CanFollowWinWhenMurdererExists;

    enum OptionName
    {
        GetLoveCountByKillImp,
        GetLoveCountByKillCrew,
        GetLoveCountByKillNeu,
        GetLoveCountByKillNK,
        OptionCanGetLoveByReport,
        GetLoveCountByReportImp,
        GetLoveCountByReportCrew,
        GetLoveCountByReportNeu,
        GetLoveCountByReportNK,
        GetHPCountByLVOverFlow,
        SuccessKillProbability,
        SuccessMissProbability,
        ReduceMissProbabilityByLV,
        CanFollowWin,
        CanFollowWinWhenMurdererExists,
    }
    private static void SetupOptionItem()
    {
        GetLoveCountByKillImp = IntegerOptionItem.Create(RoleInfo, 10, OptionName.GetLoveCountByKillImp, new(0, 20, 1), 10, false)
     .SetValueFormat(OptionFormat.Level);

        GetLoveCountByKillCrew = IntegerOptionItem.Create(RoleInfo, 11, OptionName.GetLoveCountByKillCrew, new(0, 20, 1), 4, false)
            .SetValueFormat(OptionFormat.Level);
        GetLoveCountByKillNK = IntegerOptionItem.Create(RoleInfo, 12, OptionName.GetLoveCountByKillNK, new(0, 20, 1), 6, false)
            .SetValueFormat(OptionFormat.Level);
        GetLoveCountByKillNeu = IntegerOptionItem.Create(RoleInfo, 13, OptionName.GetLoveCountByKillNeu, new(0, 20, 1), 10, false)
            .SetValueFormat(OptionFormat.Level);

        OptionCanGetLoveByReport = BooleanOptionItem.Create(RoleInfo, 14, OptionName.OptionCanGetLoveByReport, true, false);

        GetLoveCountByReportImp = IntegerOptionItem.Create(RoleInfo, 15, OptionName.GetLoveCountByReportImp, new(0, 20, 1), 10, false, OptionCanGetLoveByReport)
            .SetValueFormat(OptionFormat.Level);

        GetLoveCountByReportCrew = IntegerOptionItem.Create(RoleInfo, 16, OptionName.GetLoveCountByReportCrew, new(0, 20, 1), 4, false, OptionCanGetLoveByReport)
            .SetValueFormat(OptionFormat.Level);
        GetLoveCountByReportNK = IntegerOptionItem.Create(RoleInfo, 17, OptionName.GetLoveCountByReportNK, new(0, 20, 1), 6, false, OptionCanGetLoveByReport)
           .SetValueFormat(OptionFormat.Level);
        GetLoveCountByReportNeu = IntegerOptionItem.Create(RoleInfo, 18, OptionName.GetLoveCountByReportNeu, new(0, 20, 1), 10, false, OptionCanGetLoveByReport)
            .SetValueFormat(OptionFormat.Level);
        GetHPCountByLVOverFlow = FloatOptionItem.Create(RoleInfo, 19, OptionName.GetHPCountByLVOverFlow, new(0f, 3f, 0.1f), 0.2f, false)
            .SetValueFormat(OptionFormat.Health);
        OptionHasImpostorVision = BooleanOptionItem.Create(RoleInfo, 20, GeneralOption.ImpostorVision, true, false);
        SuccessKillProbability = FloatOptionItem.Create(RoleInfo, 21, OptionName.SuccessKillProbability, new(0f, 100f, 2.5f), 0f, false)
    .SetValueFormat(OptionFormat.Percent);
        SuccessMissProbability = FloatOptionItem.Create(RoleInfo, 22, OptionName.SuccessMissProbability, new(0f, 100f, 2.5f), 100f, false)
            .SetValueFormat(OptionFormat.Percent);
        ReduceMissProbabilityByLV = FloatOptionItem.Create(RoleInfo, 23, OptionName.ReduceMissProbabilityByLV, new(0f, 100f, 2.5f), 7.5f, false)
            .SetValueFormat(OptionFormat.Percent);
        CanFollowWin = BooleanOptionItem.Create(RoleInfo, 24, OptionName.CanFollowWin, true, false);
        CanFollowWinWhenMurdererExists = BooleanOptionItem.Create(RoleInfo, 25, OptionName.CanFollowWinWhenMurdererExists, true, false, CanFollowWin);


    }
    public bool IsNK { get; private set; } = false;
    public bool IsNE { get; private set; } = false;
    #region 全局变量
    public bool Murderer;
    public bool Crumble;
    public int LOVE = 1;
    public int Tired = 0;
    public int LVOverFlow;
    public float HealthPoint;
    public bool CanWin { get; private set; } = false;
    #endregion
    #region RPC相关
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(Murderer);
        sender.Writer.Write(Crumble);
        sender.Writer.Write(LOVE);
        sender.Writer.Write(Tired);
        sender.Writer.Write(LVOverFlow);
        sender.Writer.Write(HealthPoint);

    }
    public override void ReceiveRPC(MessageReader reader)
    {

            Murderer = reader.ReadBoolean();
            Crumble = reader.ReadBoolean();
            LOVE = reader.ReadInt32();
            Tired = reader.ReadInt32();
            LVOverFlow = reader.ReadInt32();
        HealthPoint = reader.ReadInt32();

    }
    #endregion
    public bool CanUseKillButton() => true;
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public float CalculateKillCooldown() => 25f;
    public override bool OnCheckMurderAsTargetAfter(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        var (killer, target) = info.AttemptTuple;
        var misspercent = Random.Range(0, 100);
        float misssucceed = SuccessMissProbability.GetFloat();
        for (int i = 0; i < Tired; i++)
            misssucceed -= 4f;
        misssucceed -= ReduceMissProbabilityByLV.GetFloat() * LOVE;
        Tired++;
        if (misspercent < misssucceed)
        {
            killer.RpcProtectedMurderPlayer(target);
            killer.SetKillCooldown();
            target.Notify($"<color=#666666>MISS</color>");
            return false;

        }
        SendRPC();
        if (HealthPoint >0)
        {
            HealthPoint--;
            killer.RpcProtectedMurderPlayer(target);
            killer.SetKillCooldown();
            SendRPC();
            return false;
        }
        return true;
    }
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(OptionHasImpostorVision.GetBool());

    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        var (killer, target) = info.AttemptTuple;
        var killpercent = Random.Range(0, 100);
        float killsucceed = SuccessKillProbability.GetFloat();

        for (int i = 0; i < Tired;i++)
           killsucceed += 2.5f;

        Tired++;
        SendRPC();

        if (Murderer)
            killsucceed = killsucceed + killsucceed * 0.50f;
        if (Crumble)
            killsucceed = 1000;
        if (killpercent < killsucceed)
        {
            int lv = LOVE;
            var player = target;
            if (player.IsImp())
            {
                lv += GetLoveCountByKillImp.GetInt();
            }
            else if (player.IsNeutralKiller())
            {
                lv += GetLoveCountByKillNK.GetInt();
            }
            else if (player.IsNeutralNonKiller())
            {
                lv += GetLoveCountByKillNeu.GetInt();
            }
            else if (player.IsCrew())
            {
                lv += GetLoveCountByKillCrew.GetInt();
            }
            if (lv > 20)
            {
                LVOverFlow += lv - 20;
                lv = 20;
            }
            LOVE = lv;
            var overflow = LVOverFlow;
            overflow += lv > 20 ? lv - 20 : 0;
            HealthPoint = (int)(Mathf.Floor(overflow / 5f) * GetHPCountByLVOverFlow.GetFloat());
            overflow -= (int)Mathf.Floor(LVOverFlow / 5f);
            LVOverFlow = overflow;
            SendRPC();
            return true;
        }
        return false;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (LOVE > 5 && !Murderer)
        {
            Murderer = true;
            SendRPC();
        }
        if (LOVE == 20 && !Crumble)
        {
            Crumble = true;
            SendRPC();
        }
       
        if (((LOVE == 1 && CanFollowWin.GetBool() || LOVE < 5 && CustomRoles.MeteorMurderer.IsExist(true)) && CanFollowWinWhenMurdererExists.GetBool()) && !CanWin)
        {
            CanWin = true;
            SendRPC();
        }
        else if ((LOVE != 1 && !CustomRoles.MeteorMurderer.IsExist(true) || LOVE >= 5 && CustomRoles.MeteorMurderer.IsExist(true)) && CanWin)
        {
            CanWin = false;
            IsNK = true;
            IsNE = true;
            SendRPC();
        }
        
        
    }
    
    public override void AfterMeetingTasks()
    {
        Tired -= 2;
    }
    public override string GetMark(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        if (!GameStates.IsInTask || isForMeeting || !Is(seer) || !Is(seen)) return "";
        Color color = Utils.GetRoleColor(CustomRoles.MeteorArbiter);
        if (Murderer)
            color = Color.red;
        if (Crumble)
            color = Palette.Purple;
        var hp = Player.IsAlive() ? HealthPoint + 1 : 0;
        return Utils.ColorString(color, $"(LV{LOVE})" + GetString("Tired")+ $" {Tired}，" +$"HP{hp}");
    }
  
    public override bool OnCheckReportDeadBody(PlayerControl reporter, GameData.PlayerInfo target)
    {
        if (!Is(reporter) || target != null || !OptionCanGetLoveByReport.GetBool()) return true;

        int lv = LOVE;
        var player = target.Object;
        if (player.IsImp())
        {
            lv += GetLoveCountByReportImp.GetInt();
        }
        else if (player.IsNeutralKiller())
        {
            lv += GetLoveCountByReportNK.GetInt();
        }
        else if (player.IsNeutralNonKiller())
        {
            lv += GetLoveCountByReportNeu.GetInt();
        }
        else if (player.IsCrew())
        {
            lv += GetLoveCountByReportCrew.GetInt();
        }
        if (lv > 20)
        {
            LVOverFlow += lv - 20;
            lv = 20;
        }
        LOVE = lv;
        HealthPoint = lv * GetHPCountByLVOverFlow.GetFloat();
        SendRPC();
        return true;
    }
    public bool CheckWin(ref CustomRoles winnerRole, ref CountTypes winnerCountType)
    {
        return Player.IsAlive() && CanWin;
    }
}
