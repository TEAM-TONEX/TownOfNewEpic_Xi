using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Roles.Core;
using TONEX.Roles.Neutral;
using static TONEX.Utils;
using System.Text;
using InnerNet;


namespace TONEX.Roles.AddOns.Common;
public sealed class CupidLovers : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(CupidLovers),
    player => new CupidLovers(player),
    CustomRoles.CupidLovers,
    75_1_2_2200,
    null,
    "culo|丘比特情人|丘比特愛人|丘比特链子",
    "#F69896",
    ctop: false
    );
    public CupidLovers(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    public static OptionItem CupidLoverKnowRoles;
    public static OptionItem CupidLoverSuicide;

    public static List<PlayerControl> CupidLoversPlayers = new();
    public static bool isCupidLoversDead = true;
    public static void ReceiveRPC(MessageReader reader)
    {
        CupidLoversPlayers.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
            CupidLoversPlayers.Add(Utils.GetPlayerById(reader.ReadByte()));
    }
    public static void SyncCupidLoversPlayers()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCupidLoversPlayers, SendOption.Reliable, -1);
        writer.Write(CupidLoversPlayers.Count);
        foreach (var lp in CupidLoversPlayers)
        {
            writer.Write(lp.PlayerId);
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void CheckWin()
    {
        // 恋人胜利
        if (Main.AllPlayerControls.Any(p => CustomWinnerHolder.WinnerIds.Contains(p.PlayerId) && p.Is(CustomRoles.CupidLovers)))
        {
            CustomWinnerHolder.AdditionalWinnerRoles.Add(CustomRoles.CupidLovers);
            Main.AllPlayerControls.Where(p => p.Is(CustomRoles.CupidLovers) || p.Is(CustomRoles.Cupid))
                .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
        }
    }
    public static void CheckForDeathOnExile(CustomDeathReason deathReason, params byte[] playerIds)
    {
        foreach (var playerId in playerIds)
        {
            //CupidLoversの後追い
            if (CustomRoles.CupidLovers.IsExist(true) && !isCupidLoversDead && CupidLoversPlayers.Find(lp => lp.PlayerId == playerId) != null)
                CupidLoversSuicide(playerId, true);
        }
    }
    public static void CupidLoversSuicide(byte deathId = 0x7f, bool isExiled = false, bool now = false)
    {
        if (CupidLoverSuicide.GetBool() && CustomRoles.CupidLovers.IsExist(true) && !isCupidLoversDead)
        {
            foreach (var loversPlayer in CupidLoversPlayers)
            {
                //生きていて死ぬ予定でなければスキップ
                if (!loversPlayer.Data.IsDead && loversPlayer.PlayerId != deathId) continue;

                isCupidLoversDead = true;
                foreach (var partnerPlayer in CupidLoversPlayers)
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
                foreach (var cupid in Main.AllAlivePlayerControls.Where(P => P.Is(CustomRoles.Cupid)))
                {
                    if (!cupid.Data.IsDead)
                    {
                        PlayerState.GetByPlayerId(cupid.PlayerId).DeathReason = CustomDeathReason.FollowingSuicide;
                        if (isExiled)
                        {
                            if (now) cupid?.RpcExileV2();
                            MeetingHudPatch.TryAddAfterMeetingDeathPlayers(CustomDeathReason.FollowingSuicide, cupid.PlayerId);
                        }
                        else
                        {
                            cupid.RpcMurderPlayer(cupid);
                        }
                        Utils.NotifyRoles(cupid);
                    }
                }
            }
        }

    }
    public static void OnPlayerLeft(ClientData data)
    {

        if (data.Character.Is(CustomRoles.CupidLovers) && !data.Character.Data.IsDead)
            foreach (var lovers in CupidLoversPlayers.ToArray())
            {
                isCupidLoversDead = true;
                CupidLoversPlayers.Remove(lovers);
                PlayerState.GetByPlayerId(lovers.PlayerId).RemoveSubRole(CustomRoles.CupidLovers);
            }
    }
    public static bool CanKnowOthers(PlayerControl seer, PlayerControl seen)
    {
        if ((seer.Is(CustomRoles.CupidLovers) || seer.Is(CustomRoles.Cupid)) && seen.Is(CustomRoles.CupidLovers) && CupidLoverKnowRoles.GetBool()
            || seer.Is(CustomRoles.CupidLovers) && seen.Is(CustomRoles.Cupid) && Cupid.CupidLoverKnowCupid.GetBool())
            return true;
        return false;
    }
    public static void TargetMarks(PlayerControl seer, PlayerControl target, ref StringBuilder targetMark)
    {
        //ハートマークを付ける(相手に)
        if ((seer.Is(CustomRoles.CupidLovers) || seer.Is(CustomRoles.Cupid)) && target.Is(CustomRoles.CupidLovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.CupidLovers)}>♡</color>");
        }
        //霊界からラバーズ視認
        else if (seer.Data.IsDead && !seer.Is(CustomRoles.CupidLovers) && target.Is(CustomRoles.CupidLovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.CupidLovers)}>♡</color>");
        }
    }
    public static void Marks(PlayerControl __instance, ref StringBuilder Mark)
    {
        if (__instance.Is(CustomRoles.CupidLovers) && (PlayerControl.LocalPlayer.Is(CustomRoles.CupidLovers) || PlayerControl.LocalPlayer.Is(CustomRoles.Cupid)))
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.CupidLovers)}>♡</color>");
        }
        else if (__instance.Is(CustomRoles.CupidLovers) && PlayerControl.LocalPlayer.Data.IsDead)
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.CupidLovers)}>♡</color>");
        }
    }
}