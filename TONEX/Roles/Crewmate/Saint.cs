using AmongUs.GameOptions;
using UnityEngine;
using TONEX.Roles.Core;
using static TONEX.Translator;
using Hazel;
using static UnityEngine.GraphicsBuffer;
using TONEX.Modules.SoundInterface;
using System.Linq;

namespace TONEX.Roles.Crewmate;
public sealed class Saint : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Saint),
            player => new Saint(player),
            CustomRoles.Saint,
         () => RoleTypes.Engineer,
            CustomRoleTypes.Crewmate,
            94_1_4_0300,
            SetupOptionItem,
            "sa",
            "#CCFFFF"
        );
    public Saint(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        CustomRoleManager.MarkOthers.Add(MarkOthers);
    }

    static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillNums;
    enum OptionName
    {

    }
    private static Options.OverrideTasksData Tasks;
    private int SkillLimit;
    public bool CanRedemption = false;
    public bool CanNotRedemptionButAlice = false;
    public static PlayerControl Id;
    public bool CanNotBeKill = false;
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.VentCooldown, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSkillNums = IntegerOptionItem.Create(RoleInfo, 12, GeneralOption.SkillLimit, new(1, 99, 1), 5, false)
            .SetValueFormat(OptionFormat.Times);
        Tasks = Options.OverrideTasksData.Create(RoleInfo, 20);
    }
    public override void Add()
    {
        SkillLimit = OptionSkillNums.GetInt();
        CanRedemption = false;
        CanNotRedemptionButAlice = false;
        CanNotBeKill = false;
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown =
            SkillLimit <= 0
            ? 255f
            : OptionSkillCooldown.GetFloat();
        AURoleOptions.EngineerInVentMaxTime = 1f;
    }
    public override int OverrideAbilityButtonUsesRemaining() => SkillLimit;
    public static string MarkOthers(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        return (seen==Id) ? Utils.ColorString(RoleInfo.RoleColor, Id.GetTrueRoleName()) : "";
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(SkillLimit);
    }
    public override void ReceiveRPC(MessageReader reader)
    {

        SkillLimit = reader.ReadInt32();
    }
    public override bool OnEnterVent(PlayerPhysics physics, int ventId)
    {
        if (!CanNotRedemptionButAlice && !CanRedemption)
        {
         SkillLimit -= 1;
        SendRPC();
        Utils.NotifyRoles(Player);
            Player.RpcProtectedMurderPlayer();
            if (SkillLimit<=0)
                CanRedemption = true;
            return true;
        }
        else if (CanRedemption && !CanNotRedemptionButAlice) {
            var pcList = Main.AllAlivePlayerControls.Where(x => x.PlayerId != Player.PlayerId && x.IsAlive()).ToList();
            var SelectedTarget = pcList[IRandom.Instance.Next(0, pcList.Count)];
            Id = SelectedTarget;
            var user = physics.myPlayer;
            physics.RpcBootFromVent(ventId);
            new LateTask(() =>
            {
                Player.RpcMurderPlayerV2(Player);

                var state = PlayerState.GetByPlayerId(Player.PlayerId);
                state.DeathReason = CustomDeathReason.Redemption;
            }, 0.9f, "Clam");
            Utils.NotifyRoles();
            return false;
        }
        else 
            return false;

    }
    public override bool OnCompleteTask(out bool cancel)
    {
        if (MyTaskState.IsTaskFinished && Player.IsAlive())
        {
            CanNotRedemptionButAlice = true;
            CanNotBeKill = true;
            CanRedemption = false;
        }
        cancel = false;
        return false;
    }
    public override bool OnCheckMurderAsTargetAfter(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        if (CanNotBeKill)
        {
            var (killer, target) = info.AttemptTuple;
            CanNotBeKill = false;
            killer.SetKillCooldownV2(target: target, forceAnime: true);
            info.CanKill = false ;
            return false;
        }
        return true;
    }
    public override void OnExileWrapUp(GameData.PlayerInfo exiled, ref bool DecidedWinner)
    {
        Player.RpcResetAbilityCooldown();
    }
    public override string GetProgressText(bool comms = false) => Utils.ColorString(Utils.GetRoleColor(CustomRoles.Saint), SkillLimit >= 1 ? $"({SkillLimit})" : "");
}