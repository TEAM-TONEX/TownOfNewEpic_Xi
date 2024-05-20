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
public static class AkujoLovers
{
    //private static readonly int Id = 75_1_2_1900;
    private static List<byte> playerIdList = new();

    public static OptionItem AkujoLoverKnowRoles;
    public static OptionItem AkujoLoverSuicide;

    public static List<PlayerControl> AkujoLoversPlayers = new();
    public static bool isAkujoLoversDead = true;
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
        AkujoLoversPlayers.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
            AkujoLoversPlayers.Add(Utils.GetPlayerById(reader.ReadByte()));
    }
    public static bool IsEnable => playerIdList.Count > 0;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);
    public static void SyncAkujoLoversPlayers()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetAkujoLoversPlayers, SendOption.Reliable, -1);
        writer.Write(AkujoLoversPlayers.Count);
        foreach (var lp in AkujoLoversPlayers)
        {
            writer.Write(lp.PlayerId);
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void CheckWin()
    {
        // 恋人胜利
        if (Main.AllPlayerControls.Any(p => CustomWinnerHolder.WinnerIds.Contains(p.PlayerId) && p.Is(CustomRoles.AkujoLovers)))
        {
            CustomWinnerHolder.AdditionalWinnerRoles.Add(CustomRoles.AkujoLovers);
            Main.AllPlayerControls.Where(p => p.Is(CustomRoles.AkujoLovers))
                .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
        }
    }
    public static void CheckForDeathOnExile(CustomDeathReason deathReason, params byte[] playerIds)
    {
        foreach (var playerId in playerIds)
        {
            //AkujoLoversの後追い
            if (CustomRoles.AkujoLovers.IsExistCountDeath() && !isAkujoLoversDead && AkujoLoversPlayers.Find(lp => lp.PlayerId == playerId) != null)
                AkujoLoversSuicide(playerId, true);
        }
    }
    public static void AkujoLoversSuicide(byte deathId = 0x7f, bool isExiled = false, bool now = false)
    {
        if (AkujoLoverSuicide.GetBool() && CustomRoles.AkujoLovers.IsExistCountDeath() && !isAkujoLoversDead)
        {
            foreach (var loversPlayer in AkujoLoversPlayers)
            {
                //生きていて死ぬ予定でなければスキップ
                if (!loversPlayer.Data.IsDead && loversPlayer.PlayerId != deathId) continue;

                isAkujoLoversDead = true;
                foreach (var partnerPlayer in AkujoLoversPlayers)
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

        if (data.Character.Is(CustomRoles.AkujoLovers) && !data.Character.Data.IsDead)
            foreach (var lovers in AkujoLoversPlayers.ToArray())
            {
                isAkujoLoversDead = true;
                AkujoLoversPlayers.Remove(lovers);
                PlayerState.GetByPlayerId(lovers.PlayerId).RemoveSubRole(CustomRoles.AkujoLovers);
            }
    }
    public static bool CanKnowOthers(PlayerControl seer, PlayerControl seen)
    {
        if (seer.Is(CustomRoles.AkujoLovers) && seen.Is(CustomRoles.Akujo) || seer.Is(CustomRoles.Akujo) && seen.Is(CustomRoles.AkujoLovers) && AkujoLoverKnowRoles.GetBool())
            return true;
        return false;
    }
    public static void TargetMarks(PlayerControl seer, PlayerControl target, ref StringBuilder targetMark)
    {
        //ハートマークを付ける(相手に)
        if (seer.Is(CustomRoles.AkujoLovers) && target.Is(CustomRoles.Akujo))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoLovers)}>❤</color>");
        }
        if (seer.Is(CustomRoles.Akujo) && target.Is(CustomRoles.AkujoLovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoLovers)}>❤</color>");
        }
        //霊界からラバーズ視認
        else if (seer.Data.IsDead && !seer.Is(CustomRoles.AkujoLovers) && (target.Is(CustomRoles.AkujoLovers) || target.Is(CustomRoles.Akujo)))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoLovers)}>❤</color>");
        }
    }
    public static void Marks(PlayerControl __instance,ref StringBuilder Mark)
    {
        if (__instance.Is(CustomRoles.AkujoLovers) && PlayerControl.LocalPlayer.Is(CustomRoles.Akujo))
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.AkujoLovers)}>♡</color>");
        }
        else if (__instance.Is(CustomRoles.Akujo) && PlayerControl.LocalPlayer.Is(CustomRoles.AkujoLovers))
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.AkujoLovers)}>♡</color>");
        }
        else if (__instance.Is(CustomRoles.AkujoLovers) && PlayerControl.LocalPlayer.Data.IsDead)
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.AkujoLovers)}>♡</color>");
        }
    }
}
