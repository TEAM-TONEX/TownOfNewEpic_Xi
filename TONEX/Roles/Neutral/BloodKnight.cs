using AmongUs.GameOptions;
using Hazel;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Translator;

namespace TONEX.Roles.Neutral;
public sealed class BloodKnight : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
       SimpleRoleInfo.Create(
           typeof(BloodKnight),
           player => new BloodKnight(player),
           CustomRoles.BloodKnight,
           () => RoleTypes.Impostor,
           CustomRoleTypes.Neutral,
           509230,
           SetupOptionItem,
           "bn|ÊÈÑªòTÊ¿|ÑªÆï|ÆïÊ¿",
           "#630000",
           true,
           true,
           countType: CountTypes.BloodKnight,
            assignCountRule: new(1, 1, 1)
       );
    public BloodKnight(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        CanVent = OptionCanVent.GetBool();
    }

    static OptionItem OptionKillCooldown;
    static OptionItem OptionCanVent;
    static OptionItem OptionHasImpostorVision;
    static OptionItem OptionProtectDuration;
    enum OptionName
    {
        BKProtectDuration
    }
    public static bool CanVent;

    private long ProtectStartTime;
    public bool IsNK { get; private set; } = true;
    public SchrodingerCat.TeamType SchrodingerCatChangeTo => SchrodingerCat.TeamType.BloodKnight;

    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 25f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionCanVent = BooleanOptionItem.Create(RoleInfo, 11, GeneralOption.CanVent, true, false);
        OptionHasImpostorVision = BooleanOptionItem.Create(RoleInfo, 13, GeneralOption.ImpostorVision, true, false);
        OptionProtectDuration = FloatOptionItem.Create(RoleInfo, 14, OptionName.BKProtectDuration, new(1f, 999f, 1f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add() 
    {
        ProtectStartTime = 0;
        CooldownList.Add((long)OptionProtectDuration.GetFloat());
        CountdownList.Add(ProtectStartTime);
    }
    public override void CD_Update()
    {
        ProtectStartTime = CountdownList[0];
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(ProtectStartTime.ToString());
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        ProtectStartTime = long.Parse(reader.ReadString());
    }
    public float CalculateKillCooldown() => OptionKillCooldown.GetFloat();
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(OptionHasImpostorVision.GetBool());
    public static void SetHudActive(HudManager __instance, bool _) => __instance.SabotageButton.ToggleVisible(false);
    public bool CanUseSabotageButton() => false;
    public void OnMurderPlayerAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return;
        ResetCountdown(0);
        SendRPC();
        Utils.NotifyRoles(Player);
    }
    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        if (!Is(seer) || seen != null || isForMeeting) return "";
        if (!CheckForOnGuard(0))
        {
            return isForHud ? GetString("BKSkillNotice") : "";
        }
        else return isForHud
            ? string.Format(GetString("BKSkillTimeRemain"), RemainTime(0))
            : GetString("BKInProtectForUnModed");
    }
}