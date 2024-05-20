using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using System;
using TONEX.Roles.Core;
using static TONEX.Options;
using static TONEX.Utils;
using System.Text;
using InnerNet;

namespace TONEX.Roles.AddOns.CanNotOpened;
public static class AdmirerLovers
{
    //private static readonly int Id = 75_1_2_1800;
    private static List<byte> playerIdList = new();

    public static OptionItem AdmirerLoverKnowRoles;
    public static OptionItem AdmirerLoverSuicide;

    public static List<PlayerControl> AdmirerLoversPlayers = new();
    public static bool isAdmirerLoversDead = true;
    [GameModuleInitializer]
    public static void Init()
    {
        playerIdList = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
    }
    public static void ReceiveRPC(MessageReader reader)
    {
        AdmirerLoversPlayers.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
            AdmirerLoversPlayers.Add(Utils.GetPlayerById(reader.ReadByte()));
    }
    public static bool IsEnable => playerIdList.Count > 0;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);
    public static void SyncAdmirerLoversPlayers()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetAdmirerLoversPlayers, SendOption.Reliable, -1);
        writer.Write(AdmirerLoversPlayers.Count);
        foreach (var lp in AdmirerLoversPlayers)
        {
            writer.Write(lp.PlayerId);
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void CheckWin()
    {
        // 恋人胜利
        if (Main.AllPlayerControls.Any(p => CustomWinnerHolder.WinnerIds.Contains(p.PlayerId) && p.Is(CustomRoles.AdmirerLovers)))
        {
            CustomWinnerHolder.AdditionalWinnerRoles.Add(CustomRoles.AdmirerLovers);
            Main.AllPlayerControls.Where(p => p.Is(CustomRoles.AdmirerLovers))
                .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
        }
    }
    public static void CheckForDeathOnExile(CustomDeathReason deathReason, params byte[] playerIds)
    {
        foreach (var playerId in playerIds)
        {
            //AdmirerLoversの後追い
            if (CustomRoles.AdmirerLovers.IsExistCountDeath() && !isAdmirerLoversDead && AdmirerLoversPlayers.Find(lp => lp.PlayerId == playerId) != null)
                AdmirerLoversSuicide(playerId, true);
        }
    }
    public static void AdmirerLoversSuicide(byte deathId = 0x7f, bool isExiled = false, bool now = false)
    {
        if (AdmirerLoverSuicide.GetBool() && CustomRoles.AdmirerLovers.IsExistCountDeath() && !isAdmirerLoversDead)
        {
            foreach (var loversPlayer in AdmirerLoversPlayers)
            {
                //生きていて死ぬ予定でなければスキップ
                if (!loversPlayer.Data.IsDead && loversPlayer.PlayerId != deathId) continue;

                isAdmirerLoversDead = true;
                foreach (var partnerPlayer in AdmirerLoversPlayers)
                {
                    //本人ならスキップ
                    if (loversPlayer.PlayerId == partnerPlayer.PlayerId) continue;

                    //残った恋人を全て殺す(2人以上可)
                    //生きていて死ぬ予定もない場合は心中
                    if (partnerPlayer.PlayerId != deathId && !partnerPlayer.Data.IsDead)
                    {
                        PlayerState.GetByPlayerId(partnerPlayer.PlayerId).DeathReason = CustomDeathReason.FollowingSuicide;
                        if (isExiled)
                        {
                            if (now) partnerPlayer?.RpcExileV2();
                            MeetingHudPatch.TryAddAfterMeetingDeathPlayers(CustomDeathReason.FollowingSuicide, partnerPlayer.PlayerId);
                        }
                        else
                        {
                            partnerPlayer.RpcMurderPlayer(partnerPlayer);
                        }
                        Utils.NotifyRoles(partnerPlayer);
                    }
                }
            }
        }
    }
    public static void OnPlayerLeft(ClientData data)
    {

        if (data.Character.Is(CustomRoles.AdmirerLovers) && !data.Character.Data.IsDead)
            foreach (var lovers in AdmirerLoversPlayers.ToArray())
            {
                isAdmirerLoversDead = true;
                AdmirerLoversPlayers.Remove(lovers);
                PlayerState.GetByPlayerId(lovers.PlayerId).RemoveSubRole(CustomRoles.AdmirerLovers);
            }
    }
    public static bool CanKnowOthers(PlayerControl seer, PlayerControl seen)
    {
        if (seer.Is(CustomRoles.AdmirerLovers) && seen.Is(CustomRoles.AdmirerLovers) && AdmirerLoverKnowRoles.GetBool())
            return true;
        return false;
    }
    public static void TargetMarks(PlayerControl seer, PlayerControl target, ref StringBuilder targetMark)
    {
        //ハートマークを付ける(相手に)
        if (seer.Is(CustomRoles.AdmirerLovers) && target.Is(CustomRoles.AdmirerLovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.AdmirerLovers)}>♡</color>");
        }
        //霊界からラバーズ視認
        else if (seer.Data.IsDead && !seer.Is(CustomRoles.AdmirerLovers) && target.Is(CustomRoles.AdmirerLovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.AdmirerLovers)}>♡</color>");
        }
    }
    public static void Marks(PlayerControl __instance,ref StringBuilder Mark)
    {
    if (__instance.Is(CustomRoles.AdmirerLovers) && PlayerControl.LocalPlayer.Is(CustomRoles.AdmirerLovers))
                {
                    Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.AdmirerLovers)}>♡</color>");
                }
                else if (__instance.Is(CustomRoles.AdmirerLovers) && PlayerControl.LocalPlayer.Data.IsDead)
                {
                    Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.AdmirerLovers)}>♡</color>");
                }
    }
}
