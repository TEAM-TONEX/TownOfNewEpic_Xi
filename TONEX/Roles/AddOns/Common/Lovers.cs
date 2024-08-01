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

namespace TONEX.Roles.AddOns.Common;
public sealed class Lovers : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Lovers),
    player => new Lovers(player),
    CustomRoles.Lovers,
    75_1_2_1400,
    SetupOptionItem,
    "lo|情人|愛人|链子",
    "#ff9ace"
    );
    public Lovers(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }



    public static OptionItem LoverKnowRoles;
    public static OptionItem LoverSuicide;

    public static List<PlayerControl> LoversPlayers = new();
    public static bool isLoversDead = true;

    enum OptionName
    {
        LoverKnowRoles,
             LoverSuicide,
    }

    static void SetupOptionItem()
    {
        LoverKnowRoles = BooleanOptionItem.Create(RoleInfo, 20, OptionName.LoverKnowRoles, true, false)
      .SetGameMode(CustomGameMode.Standard);
        LoverSuicide = BooleanOptionItem.Create(RoleInfo, 21, OptionName.LoverSuicide, true,false)
            .SetGameMode(CustomGameMode.Standard);
    }
    public static void ReceiveRPC(MessageReader reader)
    {
        LoversPlayers.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
            LoversPlayers.Add(Utils.GetPlayerById(reader.ReadByte()));
    }
    public static void SyncLoversPlayers()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLoversPlayers, SendOption.Reliable, -1);
        writer.Write(LoversPlayers.Count);
        foreach (var lp in LoversPlayers)
        {
            writer.Write(lp.PlayerId);
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void CheckWin()
    {
        // 恋人胜利
        if (Main.AllPlayerControls.Any(p => CustomWinnerHolder.WinnerIds.Contains(p.PlayerId) && p.Is(CustomRoles.Lovers)))
        {
            CustomWinnerHolder.AdditionalWinnerRoles.Add(CustomRoles.Lovers);
            Main.AllPlayerControls.Where(p => p.Is(CustomRoles.Lovers))
                .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
        }
    }
    public static void CheckForDeathOnExile(CustomDeathReason deathReason, params byte[] playerIds)
    {
        foreach (var playerId in playerIds)
        {
            //Loversの後追い
            if (CustomRoles.Lovers.IsExist(true) && !isLoversDead && LoversPlayers.Find(lp => lp.PlayerId == playerId) != null)
                LoversSuicide(playerId, true);
        }
    } 
    public static void AssignLoversRoles(int RawCount = -1)
    {
        if (!CustomRoles.Lovers.IsEnable()) return;
        if (Main.AllPlayerControls.Count() < 2) return;
        //Loversを初期化
        LoversPlayers.Clear();
        isLoversDead = false;
        var allPlayers = new List<PlayerControl>();
        foreach (var pc in Main.AllPlayerControls)
        {
            if (pc.Is(CustomRoles.GM) || (PlayerState.GetByPlayerId(pc.PlayerId).SubRoles.Count >= Options.AddonsNumLimit.GetInt())
                || pc.Is(CustomRoles.LazyGuy) || pc.Is(CustomRoles.Neptune) || pc.Is(CustomRoles.God) || pc.Is(CustomRoles.Hater) || pc.Is(CustomRoles.Believer) || pc.Is(CustomRoles.Nihility)
                || pc.Is(CustomRoles.Admirer) || pc.Is(CustomRoles.Akujo) || pc.Is(CustomRoles.Cupid) || pc.Is(CustomRoles.Yandere)) continue;
            allPlayers.Add(pc);
        }
        var loversRole = CustomRoles.Lovers;
        var rd = IRandom.Instance;
        var count = Math.Clamp(RawCount, 0, allPlayers.Count);
        if (RawCount == -1) count = Math.Clamp(loversRole.GetCount(), 0, allPlayers.Count);
        if (count <= 0) return;
        for (var i = 0; i < count; i++)
        {
            var player = allPlayers[rd.Next(0, allPlayers.Count)];
            LoversPlayers.Add(player);
            allPlayers.Remove(player);
            PlayerState.GetByPlayerId(player.PlayerId).SetSubRole(loversRole);
            Logger.Info($"注册附加职业：{player?.Data?.PlayerName}（{player.GetCustomRole()}）=> {loversRole}", "AssignCustomSubRoles");
        }
        SyncLoversPlayers();
    }
    public static void LoversSuicide(byte deathId = 0x7f, bool isExiled = false, bool now = false)
    {
        if (LoverSuicide.GetBool() && CustomRoles.Lovers.IsExist(true) && !isLoversDead)
        {
            foreach (var loversPlayer in LoversPlayers)
            {
                //生きていて死ぬ予定でなければスキップ
                if (!loversPlayer.Data.IsDead && loversPlayer.PlayerId != deathId) continue;

                isLoversDead = true;
                foreach (var partnerPlayer in LoversPlayers)
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

        if (data.Character.Is(CustomRoles.Lovers) && !data.Character.Data.IsDead)
            foreach (var lovers in LoversPlayers.ToArray())
            {
                isLoversDead = true;
                LoversPlayers.Remove(lovers);
                PlayerState.GetByPlayerId(lovers.PlayerId).RemoveSubRole(CustomRoles.Lovers);
            }
    }
    public static bool CanKnowOthers(PlayerControl seer, PlayerControl seen)
    {
        if (seer.Is(CustomRoles.Lovers) && seen.Is(CustomRoles.Lovers) && LoverKnowRoles.GetBool())
            return true;
        return false;
    }
    public static void TargetMarks(PlayerControl seer, PlayerControl target, ref StringBuilder targetMark)
    {
        //ハートマークを付ける(相手に)
        if (seer.Is(CustomRoles.Lovers) && target.Is(CustomRoles.Lovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.Lovers)}>♡</color>");
        }
        //霊界からラバーズ視認
        else if (seer.Data.IsDead && !seer.Is(CustomRoles.Lovers) && target.Is(CustomRoles.Lovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.Lovers)}>♡</color>");
        }
    }
    public static void Marks(PlayerControl __instance, ref StringBuilder Mark)
    {
        if (__instance.Is(CustomRoles.Lovers) && PlayerControl.LocalPlayer.Is(CustomRoles.Lovers))
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.Lovers)}>♡</color>");
        }
        else if (__instance.Is(CustomRoles.Lovers) && PlayerControl.LocalPlayer.Data.IsDead)
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.Lovers)}>♡</color>");
        }
    }
}
