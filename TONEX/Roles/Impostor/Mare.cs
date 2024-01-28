using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Options;

namespace TONEX.Roles.Impostor;
public sealed class Mare : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Mare),
            player => new Mare(player),
            CustomRoles.Mare,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            2600,
            SetupCustomOption,
            "ma|夢魘|夜魇"
        );
    public Mare(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldownInLightsOut = OptionKillCooldownInLightsOut.GetFloat();
        SpeedInLightsOut = OptionSpeedInLightsOut.GetFloat();

        IsActivateKill = false;
        IsAccelerated = false;

    }

    private static OptionItem OptionKillCooldownInLightsOut;
    private static OptionItem OptionSpeedInLightsOut;
    enum OptionName
    {
        MareAddSpeedInLightsOut,
        MareKillCooldownInLightsOut,
    }
    private float KillCooldownInLightsOut;
    private float SpeedInLightsOut;
    private static bool IsActivateKill;
    private bool IsAccelerated;  //加速済みかフラグ

    public static void SetupCustomOption()
    {
        OptionSpeedInLightsOut = FloatOptionItem.Create(RoleInfo, 10, OptionName.MareAddSpeedInLightsOut, new(0.1f, 0.5f, 0.1f), 0.3f, false);
        OptionKillCooldownInLightsOut = FloatOptionItem.Create(RoleInfo, 11, OptionName.MareKillCooldownInLightsOut, new(2.5f, 180f, 2.5f), 12f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public float CalculateKillCooldown() => IsActivateKill ? KillCooldownInLightsOut : DefaultKillCooldown;
    public override void ApplyGameOptions(IGameOptions opt)
    {
        if (IsActivateKill && !IsAccelerated)
        { //停電中で加速済みでない場合
            IsAccelerated = true;
            Main.AllPlayerSpeed[Player.PlayerId] += SpeedInLightsOut;//Mareの速度を加算
        }
        else if (!IsActivateKill && IsAccelerated)
        { //停電中ではなく加速済みになっている場合
            IsAccelerated = false;
            Main.AllPlayerSpeed[Player.PlayerId] -= SpeedInLightsOut;//Mareの速度を減算
        }
    }
    private void ActivateKill(bool activate)
    {
        IsActivateKill = activate;
        if (AmongUsClient.Instance.AmHost)
        {
            SendRPC();
            Player.MarkDirtySettings();
            Utils.NotifyRoles();
        }
    }
    public void SendRPC()
    {
        using var sender = CreateSender(CustomRPC.MareSync);
        sender.Writer.Write(IsActivateKill);
    }
    public override void ReceiveRPC(MessageReader reader, CustomRPC rpcType)
    {
        if (rpcType != CustomRPC.MareSync) return;

        IsActivateKill = reader.ReadBoolean();
    }
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        if (IsActivateKill) return true;
        if (killer == null) return false;
                   killer.ResetKillCooldown();
        killer.SetKillCooldown();
        killer.RpcProtectedMurderPlayer(target);
        MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.UpdateSystem, SendOption.Reliable, target.GetClientId());
        SabotageFixWriter.Write((byte)SystemTypes.Electrical);
        MessageExtensions.WriteNetObject(SabotageFixWriter, target);
        AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
        return false;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (GameStates.IsInTask && IsActivateKill)
        {
            if (!Utils.IsActive(SystemTypes.Electrical))
            {
                //停電解除されたらキルモード解除
                ActivateKill(false);
            }
        }
    }
    public override bool OnSabotage(PlayerControl player, SystemTypes systemType)
    {
        if (systemType == SystemTypes.Electrical)
        {
            _ = new LateTask(() =>
            {
                //まだ停電が直っていなければキル可能モードに
                if (Utils.IsActive(SystemTypes.Electrical))
                {
                    ActivateKill(true);
                }
            }, 4.0f, "Mare Activate Kill");
        }
        return true;
    }
    public static bool KnowTargetRoleColor(PlayerControl target, bool isMeeting)
        => !isMeeting && IsActivateKill && target.Is(CustomRoles.Mare);

}