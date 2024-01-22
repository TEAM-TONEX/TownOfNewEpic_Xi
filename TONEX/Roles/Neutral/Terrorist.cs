using AmongUs.GameOptions;
using System.Linq;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;

namespace TONEX.Roles.Neutral;

public sealed class Terrorist : RoleBase, IIndependent
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Terrorist),
            player => new Terrorist(player),
            CustomRoles.Terrorist,
            () => RoleTypes.Engineer,
            CustomRoleTypes.Neutral,
            50200,
            SetupOptionItem,
            "te|恐怖",
            "#00ff00",
            introSound: () => ShipStatus.Instance.CommonTasks.Where(task => task.TaskType == TaskTypes.FixWiring).FirstOrDefault().MinigamePrefab.OpenSound
        );
    public Terrorist(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.ForRecompute
    )
    {
        canSuicideWin = OptionCanSuicideWin.GetBool();
    }
    private static OptionItem OptionCanSuicideWin;
    private static Options.OverrideTasksData Tasks;
    private enum OptionName
    {
        CanTerroristSuicideWin
    }
    private static bool canSuicideWin;

    private static void SetupOptionItem()
    {
        OptionCanSuicideWin = BooleanOptionItem.Create(RoleInfo, 10, OptionName.CanTerroristSuicideWin, false, false);
        // 20-23を使用
        Tasks = Options.OverrideTasksData.Create(RoleInfo, 20);
    }

    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown = 0;
        AURoleOptions.EngineerInVentMaxTime = 0;
    }
    public override void OnMurderPlayerAsTarget(MurderInfo info)
    {
        Logger.Info($"{Player.GetRealName()}はTerroristだった", nameof(Terrorist));
        if (CanWin())
        {
            MyState.DeathReason = CustomDeathReason.Suicide;
            Win();
        }
    }
    public override bool GetGameStartSound(out string sound)
    {
        sound = "Boom";
        return true;
    }
    public override void OnExileWrapUp(GameData.PlayerInfo exiled, ref bool DecidedWinner)
    {
        if (exiled.PlayerId != Player.PlayerId)
        {
            return;
        }

        if (CanWin())
        {
            Win();
            DecidedWinner = true;
        }
    }
    public bool CanWin()
    {
        if (!canSuicideWin && MyState.IsSuicide())
        {
            return false;
        }
        return IsTaskFinished;
    }
    public void Win()
    {
        foreach (var otherPlayer in Main.AllAlivePlayerControls.Where(p => !Is(p)))
        {
            otherPlayer.SetRealKiller(Player);
            otherPlayer.RpcMurderPlayer(otherPlayer);
            var playerState = PlayerState.GetByPlayerId(otherPlayer.PlayerId);
            playerState.DeathReason = CustomDeathReason.Bombed;
            playerState.SetDead();
        }
        CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Terrorist);
        CustomWinnerHolder.WinnerIds.Add(Player.PlayerId);
    }
}
