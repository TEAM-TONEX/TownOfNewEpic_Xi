using AmongUs.GameOptions;
using System.Linq;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Neutral;

public sealed class Hater : RoleBase, IKiller, IAdditionalWinner, ISchrodingerCatOwner
{
    public static readonly SimpleRoleInfo RoleInfo =
       SimpleRoleInfo.Create(
            typeof(Hater),
            player => new Hater(player),
            CustomRoles.Hater,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            51700,
            null,
            "ht|fff�F|fff|fff��",
            "#414b66",
            true
        );
    public Hater(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    { }

    public float CalculateKillCooldown() => 0f;
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(true);

    public SchrodingerCat.TeamType SchrodingerCatChangeTo => SchrodingerCat.TeamType.Hater;

    public bool OverrideKillButtonText(out string text)
    {
        text = Translator.GetString("HaterButtonText");
        return true;
    }
    public void OnMurderPlayerAsKiller(MurderInfo info)
    {
        (var killer, var target) = info.AttemptTuple;
        if (Is(killer) && !info.IsSuicide && !target.Is(CustomRoles.Lovers) && !target.Is(CustomRoles.Neptune))
        {
            killer.RpcMurderPlayer(killer);
            PlayerState.GetByPlayerId(killer.PlayerId).DeathReason = CustomDeathReason.Sacrifice;
            Logger.Info($"{killer.GetRealName()} ��ɱ�˷�Ŀ����ң�׳�������ˣ�bushi��", "FFF");
            return;
        }
    }
    public bool CheckWin(ref CustomRoles winnerRole , ref CountTypes winnerCountType)
    {
        return CustomWinnerHolder.WinnerTeam != CustomWinner.Lovers
            && !CustomWinnerHolder.AdditionalWinnerRoles.Contains(CustomRoles.Lovers)
            && !CustomRoles.Lovers.IsExist()
            && !CustomRoles.Neptune.IsExist()
            && Main.AllPlayerControls.Any(p => (p.Is(CustomRoles.Lovers) || p.Is(CustomRoles.Neptune)) && Is(p.GetRealKiller()));
    }
}
