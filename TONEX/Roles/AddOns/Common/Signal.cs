using Hazel;
using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;
using UnityEngine.UIElements.UIR;

namespace TONEX.Roles.AddOns.Common;
public sealed class Signal : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Signal),
    player => new Signal(player),
    CustomRoles.Signal,
    75_1_0_0300,
    null,
    "si|通讯",
    "#F39C12"
    );
    public Signal(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    public static Dictionary<byte, Vector2> Signalbacktrack = new();
    public static void AddPosi()
    {
        Signalbacktrack = new();
        foreach (var player in Main.AllPlayerControls)
        {
            if (!AmongUsClient.Instance.AmHost || !player.Is(CustomRoles.Signal)) return;
            Signalbacktrack.Add(player.PlayerId, player.GetTruePosition());
        }
        SendRPC();
    }
    public override void AfterMeetingTasks()
    {
        if (Signalbacktrack.ContainsKey(Player.PlayerId))
            Player.RpcTeleport(Signalbacktrack[Player.PlayerId]);
        Signalbacktrack = new();
    }
    public static void SendRPC()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SignalPosition, SendOption.Reliable, -1);
        foreach (var pc in Main.AllAlivePlayerControls)
        {
            if (Signalbacktrack.ContainsKey(pc.PlayerId))
            {
                writer.Write(pc.PlayerId);
                writer.Write(Signalbacktrack[pc.PlayerId].x);
                writer.Write(Signalbacktrack[pc.PlayerId].y);
            }
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC(MessageReader reader, CustomRPC rpcType)
    {
        if (rpcType != CustomRPC.SignalPosition) return;
        var pc = reader.ReadByte();
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        Signalbacktrack = new();
        Signalbacktrack.Add(pc, new(x, y));
    }
}
