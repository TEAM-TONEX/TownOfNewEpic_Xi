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

public sealed class MeteorMurderer : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(MeteorMurderer),
            player => new MeteorMurderer(player),
            CustomRoles.MeteorMurderer,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            75_1_2_0200,
            SetupOptionItem,
            "Frisk|MeteorMurderer|USF!Frisk",
             "#ff0000",
            true,
            true,
            countType: CountTypes.MeteorMurderer,
            assignCountRule: new(1, 1, 1)
#if RELEASE
,
ctop:true
#endif
        );
    public MeteorMurderer(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        LOVE = 1;
        LVOverFlow = 0;
        HealthPoint = 0;
    }
    public bool IsNK { get; private set; } = true;

    static OptionItem GetLoveCountByKillImp;
    static OptionItem GetLoveCountByKillCrew;
    static OptionItem GetLoveCountByKillNK;
    static OptionItem GetLoveCountByKillNeu;
    static OptionItem OptionCanGetLoveByReport;
    static OptionItem GetLoveCountByReportImp;
    static OptionItem GetLoveCountByReportCrew;
    static OptionItem GetLoveCountByReportNK;
    static OptionItem GetLoveCountByReportNeu;
    static OptionItem GetHPCountByLV;
    static OptionItem OptionHasImpostorVision;
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

        OptionHasImpostorVision,
        GetHPCountByLV
    }
    private static void SetupOptionItem()
    {
        GetLoveCountByKillImp = IntegerOptionItem.Create(RoleInfo, 10, OptionName.GetLoveCountByKillImp, new (0, 20, 1), 4, false)
     .SetValueFormat(OptionFormat.Level);

        GetLoveCountByKillCrew = IntegerOptionItem.Create(RoleInfo, 11, OptionName.GetLoveCountByKillCrew, new (0, 20, 1), 4, false)
            .SetValueFormat(OptionFormat.Level);
        GetLoveCountByKillNK = IntegerOptionItem.Create(RoleInfo, 12, OptionName.GetLoveCountByKillNK, new(0, 20, 1), 4, false)
            .SetValueFormat(OptionFormat.Level);
        GetLoveCountByKillNeu = IntegerOptionItem.Create(RoleInfo, 13, OptionName.GetLoveCountByKillNeu, new (0, 20, 1), 4, false)
            .SetValueFormat(OptionFormat.Level);

        OptionCanGetLoveByReport = BooleanOptionItem.Create(RoleInfo, 14, OptionName.OptionCanGetLoveByReport, true, false);

        GetLoveCountByReportImp = IntegerOptionItem.Create(RoleInfo, 15, OptionName.GetLoveCountByReportImp, new (0, 20, 1), 4, false, OptionCanGetLoveByReport)
            .SetValueFormat(OptionFormat.Level);

        GetLoveCountByReportCrew = IntegerOptionItem.Create(RoleInfo, 16, OptionName.GetLoveCountByReportCrew, new (0, 20, 1), 4, false, OptionCanGetLoveByReport)
            .SetValueFormat(OptionFormat.Level);
        GetLoveCountByReportNK = IntegerOptionItem.Create(RoleInfo, 17, OptionName.GetLoveCountByReportNK, new(0, 20, 1), 4, false, OptionCanGetLoveByReport)
           .SetValueFormat(OptionFormat.Level);
        GetLoveCountByReportNeu = IntegerOptionItem.Create(RoleInfo, 18, OptionName.GetLoveCountByReportNeu, new (0, 20, 1), 4, false, OptionCanGetLoveByReport)
            .SetValueFormat(OptionFormat.Level);
        GetHPCountByLV = FloatOptionItem.Create(RoleInfo, 19, OptionName.GetHPCountByLV, new(0f, 3f, 0.1f), 0.2f, false)
            .SetValueFormat(OptionFormat.Health);
        OptionHasImpostorVision = BooleanOptionItem.Create(RoleInfo, 20, GeneralOption.ImpostorVision, true, false);
        
    }

    #region 全局变量
    public int LOVE = 1;
    public int LVOverFlow;
    public float HealthPoint;
    #endregion
    #region RPC相关
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(LOVE);
        sender.Writer.Write(LVOverFlow);
        sender.Writer.Write(HealthPoint);

    }
    public override void ReceiveRPC(MessageReader reader)
    {

            LOVE = reader.ReadInt32();
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

        if (HealthPoint >0)
        {
            HealthPoint--;
            if (HealthPoint < 0)
                HealthPoint = 0;
            SendRPC();
            return false;
        }
        return true;
    }
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        var (killer, target) = info.AttemptTuple;
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
        HealthPoint = lv * GetHPCountByLV.GetFloat();
        SendRPC();
        return true;
    }
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(OptionHasImpostorVision.GetBool());
    public override string GetSuffix(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        if (!GameStates.IsInTask || isForMeeting || !Is(seer) || !Is(seen)) return "";
        Color color = Color.white;
        if (LOVE >= 10 && LOVE<20)
            color = Color.red;
        if (LOVE == 20)
            color = Palette.Purple;
        var hp = Player.IsAlive() ? HealthPoint + 1 : 0;
        return Utils.ColorString(color, $"(LV{LOVE})"  +$"HP{hp}");
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
        HealthPoint = lv * GetHPCountByLV.GetFloat();
        SendRPC();
        return true;
    }
}
