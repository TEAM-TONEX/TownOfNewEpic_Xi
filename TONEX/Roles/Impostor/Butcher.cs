﻿using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using System.Collections.Generic;
using System.Linq;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;

namespace TONEX.Roles.Impostor;
public sealed class Butcher : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Butcher),
            player => new Butcher(player),
            CustomRoles.Butcher,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            4700,
            null,
            "bu|肢解",
            experimental: true
        );
    public Butcher(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    private List<byte> ButcherKilledPlayers;
    public override void OnDestroy() => ButcherKilledPlayers.Clear();
    public override void Add() => ButcherKilledPlayers = new();
    public bool OverrideKillButtonText(out string text)
    {
        text = Translator.GetString("ButcherButtonText");
        return true;
    }
    public void BeforeMurderPlayerAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return;
        info.DoKill = false;
        var (killer, target) = info.AttemptTuple;

        if (ButcherKilledPlayers.Contains(target.PlayerId)) return;

        PlayerState.GetByPlayerId(target.PlayerId).DeathReason = CustomDeathReason.Dismembered;
        new LateTask(() =>
        {
            if (!ButcherKilledPlayers.Contains(target.PlayerId)) ButcherKilledPlayers.Add(target.PlayerId);
            var ops = target.GetTruePosition();
            var rd = IRandom.Instance;
            for (int i = 0; i < 20; i++)
            {
                Vector2 location = new(ops.x + ((float)(rd.Next(0, 201) - 100) / 100), ops.y + ((float)(rd.Next(0, 201) - 100) / 100));
                location += new Vector2(0, 0.3636f);

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetTransform.NetId, (byte)RpcCalls.SnapTo, SendOption.None, -1);
                NetHelpers.WriteVector2(location, writer);
                writer.Write(target.NetTransform.lastSequenceId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                target.NetTransform.SnapTo(location);
                killer.MurderPlayer(target);

                if (target.Is(CustomRoles.Avenger))
                {
                    var pcList = Main.AllAlivePlayerControls.Where(x => x.PlayerId != target.PlayerId).ToList();
                    var rp = pcList[IRandom.Instance.Next(0, pcList.Count)];
                    var state = PlayerState.GetByPlayerId(rp.PlayerId).DeathReason = CustomDeathReason.Revenge;
                    rp.SetRealKiller(target);
                    rp.RpcMurderPlayerV2(rp);
                }

                MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.None, -1);
                messageWriter.WriteNetObject(target);
                AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
            }
            killer.RpcTeleport(ops);
        }, 0.05f, "Butcher Murder");

        return;
    }
}