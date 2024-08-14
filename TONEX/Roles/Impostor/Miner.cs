using AmongUs.GameOptions;
using Hazel;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Translator;
using System.Text;

namespace TONEX.Roles.Impostor;
public sealed class Miner : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Miner),
            player => new Miner(player),
            CustomRoles.Miner,
            () => RoleTypes.Phantom,
            CustomRoleTypes.Impostor,
            1900,
            SetupOptionItem,
            "mn|礦工"
        );
    public Miner(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        InvisTime = -1;
        LastTime = -1;
        VentedId = -1;
    }

    static OptionItem MinerCooldown;
    static OptionItem MinerDuration;
    enum OptionName
    {
        MinerCooldown,
        MinerDuration,
    }

    private long InvisTime;
    private long LastTime;
    private int VentedId;
    private static void SetupOptionItem()
    {
        MinerCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.MinerCooldown, new(2.5f, 180f, 2.5f), 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        MinerDuration = FloatOptionItem.Create(RoleInfo, 11, OptionName.MinerDuration, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(InvisTime.ToString());
        sender.Writer.Write(LastTime.ToString());
    }
    public override void ReceiveRPC(MessageReader reader)
    {

        InvisTime = long.Parse(reader.ReadString());
        LastTime = long.Parse(reader.ReadString());
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = Translator.GetString("MinerTeleButtonText");
        return Main.LastEnteredVent.ContainsKey(Player.PlayerId);
    }
    public override bool OnCheckVanish()
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        Player.RpcResetAbilityCooldown();
        if (Main.LastEnteredVent.ContainsKey(Player.PlayerId))
        {
            Player.RpcTeleport(Main.LastEnteredVentLocation[Player.PlayerId]);
            Logger.Msg($"矿工传送：{Player.GetNameWithRole()}", "Miner.OnShapeshift");
        }
        return false;
    }
    public bool CanGoInvis() => GameStates.IsInTask && InvisTime == -1 && LastTime == -1;
    public bool IsInvis() => InvisTime != -1;
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost || LastTime == -1) return;
        var now = Utils.GetTimeStamp();

        if (LastTime + (long)MinerCooldown.GetFloat() < now)
        {
            LastTime = -1;
            if (!player.IsModClient()) player.Notify(GetString("MinerCanVent"));
            SendRPC();
        }
    }
    public override void OnSecondsUpdate(PlayerControl player, long now)
    {
        if (!AmongUsClient.Instance.AmHost || !IsInvis()) return;
        var remainTime = InvisTime + (long)MinerDuration.GetFloat() - now;
        if (remainTime < 0)
        {
            LastTime = now;
            InvisTime = -1;
            SendRPC();
            player?.MyPhysics?.RpcBootFromVent(VentedId != -1 ? VentedId : Main.LastEnteredVent[player.PlayerId].Id);
            NameNotifyManager.Notify(player, GetString("MinerInvisStateOut"));
            Player.EnableAction(Player, ExtendedPlayerControl.PlayerActionType.Kill | ExtendedPlayerControl.PlayerActionType.Shapeshift | ExtendedPlayerControl.PlayerActionType.Sabotage | ExtendedPlayerControl.PlayerActionType.Report | ExtendedPlayerControl.PlayerActionType.Meeting | ExtendedPlayerControl.PlayerActionType.Pet, true);
            return;
        }
        else if (remainTime <= 10)
        {
            if (!player.IsModClient()) player.Notify(string.Format(GetString("MinerInvisStateCountdown"), remainTime));
        }
    }
    public override bool OnExitVent(PlayerPhysics physics, int ventId)
    {
        var now = Utils.GetTimeStamp();

        new LateTask(() =>
        {
            if (CanGoInvis())
            {
                VentedId = ventId;
                physics.RpcEnterVent(ventId);
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(physics.NetId, 34, SendOption.Reliable, Player.GetClientId());
                writer.WritePacked(ventId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                Player.DisableAction(Player, ExtendedPlayerControl.PlayerActionType.Kill | ExtendedPlayerControl.PlayerActionType.Shapeshift | ExtendedPlayerControl.PlayerActionType.Sabotage | ExtendedPlayerControl.PlayerActionType.Report | ExtendedPlayerControl.PlayerActionType.Meeting | ExtendedPlayerControl.PlayerActionType.Pet, ExtendedPlayerControl.PlayerActionInUse.All, true);
                InvisTime = now;
                SendRPC();

                NameNotifyManager.Notify(Player, GetString("MinerInvisState"), MinerDuration.GetFloat());
            }
            else if (IsInvis())
            {
                physics.RpcEnterVent(ventId);
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(physics.NetId, 34, SendOption.Reliable, Player.GetClientId());
                writer.WritePacked(ventId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            else
            {
                NameNotifyManager.Notify(Player, GetString("MinerInvisInCooldown"));
            }
        }, 0.5f, "Miner Vent");
        return true;

    }
    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        if (isForMeeting) return "";

        var str = new StringBuilder();
        if (IsInvis())
        {
            var remainTime = InvisTime + (long)MinerDuration.GetFloat() - Utils.GetTimeStamp();
            return string.Format(GetString("MinerInvisStateCountdown"), remainTime);
        }
        else if (LastTime != -1)
        {
            var cooldown = LastTime + (long)MinerCooldown.GetFloat() - Utils.GetTimeStamp();

            return string.Format(GetString("MinerInvisCooldownRemain"), cooldown);
        }
        else
        {
            str.Append(GetString("MinerCanVent"));
        }
        return str.ToString();
    }
    public void BeforeMurderPlayerAsKiller(MurderInfo info)
    {
        if (IsInvis()) return;
    }
    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
    {
        LastTime = -1;
        InvisTime = -1;
        SendRPC();
    }
    public override void OnGameStart()
    {
        LastTime = Utils.GetTimeStamp();
        SendRPC();
    }
}
