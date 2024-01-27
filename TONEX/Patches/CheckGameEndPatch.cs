using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Crewmate;
using TONEX.Roles.Neutral;
using static TONEX.Translator;

namespace TONEX;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class GameEndChecker
{
    private static GameEndPredicate predicate;
    public static bool Prefix()
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        //ゲーム終了判定済みなら中断
        if (predicate == null) return false;

        //ゲーム終了しないモードで廃村以外の場合は中断
        if (Options.NoGameEnd.GetBool() && CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw and not CustomWinner.Error) return false;
        //廃村用に初期値を設定
        var reason = GameOverReason.ImpostorByKill;

        //ゲーム終了判定
        predicate.CheckForEndGame(out reason);
        //热土豆用
        if (Options.CurrentGameMode == CustomGameMode.HotPotato)
        {
            var playerList = Main.AllAlivePlayerControls.ToList();
            if (playerList.Count == 1)
            {
                foreach (var cp in playerList)
                {
                    CustomWinnerHolder.ResetAndSetWinner(CustomWinner.ColdPotato);
                    CustomWinnerHolder.WinnerIds.Add(cp.PlayerId);
                    ShipStatus.Instance.enabled = false;
            StartEndGame(reason);
            predicate = null;
            return false;
                }
            }

        }
        //ゲーム終了時
        if (CustomWinnerHolder.WinnerTeam != CustomWinner.Default)
        {
            //カモフラージュ強制解除
            Main.AllPlayerControls.Do(pc => Camouflage.RpcSetSkin(pc, ForceRevert: true, RevertToDefault: true));

            if (reason == GameOverReason.ImpostorBySabotage && CustomRoles.Jackal.IsExist() && Jackal.WinBySabotage && !Main.AllAlivePlayerControls.Any(x => x.GetCustomRole().IsImpostorTeam()))
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Jackal);
            }

            switch (CustomWinnerHolder.WinnerTeam)
            {
                case CustomWinner.Crewmate:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoleTypes.Crewmate) && !pc.Is(CustomRoles.Madmate) && !pc.Is(CustomRoles.Charmed) && !pc.Is(CustomRoles.Wolfmate))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Impostor:
                    Main.AllPlayerControls
                        .Where(pc => (pc.Is(CustomRoleTypes.Impostor) || pc.Is(CustomRoles.Madmate)) && !pc.Is(CustomRoles.Charmed) && !pc.Is(CustomRoles.Wolfmate))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Succubus:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.Succubus) || pc.Is(CustomRoles.Charmed))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Jackal:
                    Main.AllPlayerControls
                     .Where(pc => pc.Is(CustomRoles.Jackal) || pc.Is(CustomRoles.Wolfmate) || pc.Is(CustomRoles.Sidekick) || pc.Is(CustomRoles.Whoops))
                     .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.FAFL:
                    Main.AllPlayerControls
                     .Where(pc => pc.Is(CustomRoles.Vagor_FAFL) )
                     .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
            }
            if (CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw and not CustomWinner.None and not CustomWinner.Error)
            {
                //抢夺胜利
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.GetRoleClass() is IOverrideWinner overrideWinner)
                    {
                        overrideWinner.CheckWin(ref CustomWinnerHolder.WinnerTeam, ref CustomWinnerHolder.WinnerIds);
                    }
                }
                //移除胜利
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.GetRoleClass() is IRemoveWinner removeWinner)
                    {
                        removeWinner.CheckWin(ref CustomWinnerHolder.WinnerTeam, ref CustomWinnerHolder.WinnerIds);
                    }
                }
                //RubePeople 胜利时移除玩家ID了
                if (CustomRoles.RubePeople.IsExist() && RubePeople.ForRubePeople.Count != 0)
                {
                    foreach (var pc in Main.AllPlayerControls)
                    {
                        if (RubePeople.ForRubePeople.Contains(pc.PlayerId))
                        {
                            CustomWinnerHolder.WinnerIds.Remove(pc.PlayerId);
                        }
                    }
                }
                //追加胜利
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.GetRoleClass() is IAdditionalWinner additionalWinner)
                    {
                        var winnerRole = pc.GetCustomRole();
                        var ct = pc.GetCountTypes();
                        if (additionalWinner.CheckWin(ref winnerRole, ref ct))
                        {
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                            CustomWinnerHolder.AdditionalWinnerRoles.Add(winnerRole);
                        }
                    }
                }
                foreach (var nv in Main.AllPlayerControls)
                {
                    if (!nv.Is(CustomRoles.Non_Villain) || Non_Villain.DigitalLifeList.Count <=0) continue;
                    foreach (var pc in Non_Villain.DigitalLifeList)
                    {
                        if (CustomWinnerHolder.WinnerIds.Contains(nv.PlayerId))
                        {
                            CustomWinnerHolder.WinnerIds.Add(pc);
                        }
                    }
                }

                // 第三方共同胜利
                if (Options.NeutralWinTogether.GetBool() && Main.AllPlayerControls.Any(p => CustomWinnerHolder.WinnerIds.Contains(p.PlayerId) && p.IsNeutral()))
                {
                    Main.AllPlayerControls.Where(p => p.IsNeutral())
                        .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
                }
                else if (Options.NeutralRoleWinTogether.GetBool())
                {
                    foreach (var pc in Main.AllPlayerControls.Where(p => CustomWinnerHolder.WinnerIds.Contains(p.PlayerId) && p.IsNeutral()))
                    {
                        Main.AllPlayerControls.Where(p => p.GetCustomRole() == pc.GetCustomRole())
                            .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
                    }
                }

                // 恋人胜利
                if (Main.AllPlayerControls.Any(p => CustomWinnerHolder.WinnerIds.Contains(p.PlayerId) && p.Is(CustomRoles.Lovers)))
                {
                    CustomWinnerHolder.AdditionalWinnerRoles.Add(CustomRoles.Lovers);
                    Main.AllPlayerControls.Where(p => p.Is(CustomRoles.Lovers))
                        .Do(p => CustomWinnerHolder.WinnerIds.Add(p.PlayerId));
                }
            }
            ShipStatus.Instance.enabled = false;
            StartEndGame(reason);
            predicate = null;
        }
        return false;
    }
    public static void StartEndGame(GameOverReason reason)
    {
        var sender = new CustomRpcSender("EndGameSender", SendOption.Reliable, true);
        sender.StartMessage(-1); // 5: GameData
        MessageWriter writer = sender.stream;

        //ゴーストロール化
        List<byte> ReviveRequiredPlayerIds = new();
        var winner = CustomWinnerHolder.WinnerTeam;
        foreach (var pc in Main.AllPlayerControls)
        {
            if (winner == CustomWinner.Draw)
            {
                SetGhostRole(ToGhostImpostor: true);
                continue;
            }
            bool canWin = CustomWinnerHolder.WinnerIds.Contains(pc.PlayerId) ||
                    CustomWinnerHolder.WinnerRoles.Contains(pc.GetCustomRole());
            bool isCrewmateWin = reason.Equals(GameOverReason.HumansByVote) || reason.Equals(GameOverReason.HumansByTask);
            SetGhostRole(ToGhostImpostor: canWin ^ isCrewmateWin);

            void SetGhostRole(bool ToGhostImpostor)
            {
                if (!pc.Data.IsDead) ReviveRequiredPlayerIds.Add(pc.PlayerId);
                if (ToGhostImpostor)
                {
                    Logger.Info($"{pc.GetNameWithRole()}: ImpostorGhostに変更", "ResetRoleAndEndGame");
                    sender.StartRpc(pc.NetId, RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.ImpostorGhost)
                        .EndRpc();
                    pc.SetRole(RoleTypes.ImpostorGhost);
                }
                else
                {
                    Logger.Info($"{pc.GetNameWithRole()}: CrewmateGhostに変更", "ResetRoleAndEndGame");
                    sender.StartRpc(pc.NetId, RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.CrewmateGhost)
                        .EndRpc();
                    pc.SetRole(RoleTypes.Crewmate);
                }
            }
            SetEverythingUpPatch.LastWinsReason = winner is CustomWinner.Crewmate or CustomWinner.Impostor ? GetString($"GameOverReason.{reason}") : "";
        }

        // CustomWinnerHolderの情報の同期
        sender.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame);
        CustomWinnerHolder.WriteTo(sender.stream);
        sender.EndRpc();

        // GameDataによる蘇生処理
        writer.StartMessage(1); // Data
        {
            writer.WritePacked(GameData.Instance.NetId); // NetId
            foreach (var info in GameData.Instance.AllPlayers)
            {
                if (ReviveRequiredPlayerIds.Contains(info.PlayerId))
                {
                    // 蘇生&メッセージ書き込み
                    info.IsDead = false;
                    writer.StartMessage(info.PlayerId);
                    info.Serialize(writer);
                    writer.EndMessage();
                }
            }
            writer.EndMessage();
        }

        sender.EndMessage();

        // バニラ側のゲーム終了RPC
        writer.StartMessage(8); //8: EndGame
        {
            writer.Write(AmongUsClient.Instance.GameId); //GameId
            writer.Write((byte)reason); //GameoverReason
            writer.Write(false); //showAd
        }
        writer.EndMessage();

        sender.SendMessage();
    }

    public static void SetPredicateToNormal() => predicate = new NormalGameEndPredicate();
    public static void SetPredicateToHotPotato() => predicate = new HotPotatoGameEndPredicate();

    // ===== ゲーム終了条件 =====
    // 通常ゲーム用
    class NormalGameEndPredicate : GameEndPredicate
    {
        public override bool CheckForEndGame(out GameOverReason reason)
        {
            reason = GameOverReason.ImpostorByKill;
            if (CustomWinnerHolder.WinnerTeam != CustomWinner.Default) return false;
            if (CheckGameEndByLivingPlayers(out reason)) return true;
            if (CheckGameEndByTask(out reason)) return true;
            if (CheckGameEndBySabotage(out reason)) return true;

            return false;
        }

        public bool CheckGameEndByLivingPlayers(out GameOverReason reason)
        {
            reason = GameOverReason.ImpostorByKill;

            if (CustomRoles.Sunnyboy.IsExist() && Main.AllAlivePlayerControls.Count() > 1) return false;

            int Imp = Utils.AlivePlayersCount(CountTypes.Impostor);
            int Crew = Utils.AlivePlayersCount(CountTypes.Crew);
            int JK = Utils.AlivePlayersCount(CountTypes.Jackal);
            int PL = Utils.AlivePlayersCount(CountTypes.Pelican);
            int DM = Utils.AlivePlayersCount(CountTypes.Demon);
            int BK = Utils.AlivePlayersCount(CountTypes.BloodKnight);
            int SC = Utils.AlivePlayersCount(CountTypes.Succubus);
            int FAFL = Utils.AlivePlayersCount(CountTypes.FAFL);

            foreach (var dualPc in Main.AllAlivePlayerControls.Where(p => p.Is(CustomRoles.Schizophrenic)))
            {
                if (dualPc.Is(CountTypes.Impostor)) Imp++;
                else if (dualPc.Is(CountTypes.Crew)) Crew++;
                else if (dualPc.Is(CountTypes.Succubus)) SC++;
            }

            if (Imp == 0 && Crew == 0 && JK == 0 && PL == 0 && DM == 0 && BK == 0 && SC == 0 && FAFL == 0) //全灭
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.None);
            }
            else if (Main.AllAlivePlayerControls.All(p => p.Is(CustomRoles.Lovers))) //恋人胜利
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Lovers);
            }
            else if (JK == 0 && PL == 0 && DM == 0 && BK == 0 && FAFL == 0 && SC == 0 && Crew <= Imp) //内鬼胜利
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Impostor);
            }
            else if (Imp == 0 && PL == 0 && DM == 0 && BK == 0 && FAFL == 0 && SC == 0 && Crew <= JK) //豺狼胜利
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Jackal);
            }
            else if (Imp == 0 && JK == 0 && DM == 0 && BK == 0 && FAFL == 0 && SC == 0 && Crew <= PL) //鹈鹕胜利
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Pelican);
                CustomWinnerHolder.WinnerRoles.Add(CustomRoles.Pelican);
            }
            else if (Imp == 0 && JK == 0 && PL == 0 && BK == 0 && FAFL == 0 && SC == 0 && Crew <= DM) //玩家胜利
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Demon);
                CustomWinnerHolder.WinnerRoles.Add(CustomRoles.Demon);
            }
            else if (Imp == 0 && JK == 0 && PL == 0 && DM == 0 && FAFL == 0 && SC == 0 && Crew <= BK) //嗜血骑士胜利
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.BloodKnight);
                CustomWinnerHolder.WinnerRoles.Add(CustomRoles.BloodKnight);
            }
            else if (Imp == 0 && JK == 0 && PL == 0 && DM == 0 && FAFL == 0 && BK == 0 && Crew <= SC) //魅魔胜利
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Succubus);
            }
            else if (JK == 0 && PL == 0 && Imp == 0 && BK == 0 && DM == 0 && SC == 0 && Crew <= FAFL) //异世阵营胜利
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.FAFL);
            }
            else if (JK == 0 && PL == 0 && Imp == 0 && BK == 0 && FAFL == 0 && DM == 0 && SC == 0) //船员胜利
            {
                reason = GameOverReason.HumansByVote;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Crewmate);
            }
            else return false; //胜利条件未达成

            return true;
        }
    }
    class HotPotatoGameEndPredicate : GameEndPredicate
    {
        public override bool CheckForEndGame(out GameOverReason reason)
        {   
            reason = GameOverReason.ImpostorByKill;
            var playerList = Main.AllAlivePlayerControls.ToList();
            if (playerList.Count == 1)
            {
                foreach (var cp in playerList)
                {
                    CustomWinnerHolder.ResetAndSetWinner(CustomWinner.ColdPotato);
                    CustomWinnerHolder.WinnerIds.Add(cp.PlayerId);
                    return true;
                }
            }
            else { return false; }
            return true;
        }
    }
}

public abstract class GameEndPredicate
{
    /// <summary>ゲームの終了条件をチェックし、CustomWinnerHolderに値を格納します。</summary>
    /// <params name="reason">バニラのゲーム終了処理に使用するGameOverReason</params>
    /// <returns>ゲーム終了の条件を満たしているかどうか</returns>
    public abstract bool CheckForEndGame(out GameOverReason reason);

    /// <summary>GameData.TotalTasksとCompletedTasksをもとにタスク勝利が可能かを判定します。</summary>
    public virtual bool CheckGameEndByTask(out GameOverReason reason)
    {
        reason = GameOverReason.ImpostorByKill;
        if (Options.DisableTaskWin.GetBool() || TaskState.InitialTotalTasks == 0) return false;

        if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
        {
            reason = GameOverReason.HumansByTask;
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Crewmate);
            return true;
        }
        return false;
    }
    /// <summary>ShipStatus.Systems内の要素をもとにサボタージュ勝利が可能かを判定します。</summary>
    public virtual bool CheckGameEndBySabotage(out GameOverReason reason)
    {
        reason = GameOverReason.ImpostorByKill;
        if (ShipStatus.Instance.Systems == null) return false;

        // TryGetValueは使用不可
        var systems = ShipStatus.Instance.Systems;
        LifeSuppSystemType LifeSupp;
        if (systems.ContainsKey(SystemTypes.LifeSupp) && // サボタージュ存在確認
            (LifeSupp = systems[SystemTypes.LifeSupp].TryCast<LifeSuppSystemType>()) != null && // キャスト可能確認
            LifeSupp.Countdown < 0f) // タイムアップ確認
        {
            // 酸素サボタージュ
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Impostor);
            reason = GameOverReason.ImpostorBySabotage;
            LifeSupp.Countdown = 10000f;
            return true;
        }

        ISystemType sys = null;
        if (systems.ContainsKey(SystemTypes.Reactor)) sys = systems[SystemTypes.Reactor];
        else if (systems.ContainsKey(SystemTypes.Laboratory)) sys = systems[SystemTypes.Laboratory];
        else if (systems.ContainsKey(SystemTypes.HeliSabotage)) sys = systems[SystemTypes.HeliSabotage];

        ICriticalSabotage critical;
        if (sys != null && // サボタージュ存在確認
            (critical = sys.TryCast<ICriticalSabotage>()) != null && // キャスト可能確認
            critical.Countdown < 0f) // タイムアップ確認
        {
            // リアクターサボタージュ
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Impostor);
            reason = GameOverReason.ImpostorBySabotage;
            critical.ClearSabotage();
            return true;
        }

        return false;
    }
}