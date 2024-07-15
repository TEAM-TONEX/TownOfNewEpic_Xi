using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.MoreGameModes;
using TONEX.Roles.AddOns.CanNotOpened;
using TONEX.Roles.AddOns.Common;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Crewmate;
using TONEX.Roles.Neutral;
using System;
using static RoleEffectAnimation;
using static TONEX.Translator;
using Newtonsoft.Json.Linq;

namespace TONEX;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckEndGameViaTasks))]
class CheckEndGameViaTasksForNormalPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (Main.AssistivePluginMode.Value) return true;
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class GameEndChecker
{
    private static GameEndPredicate predicate;
    public static bool Prefix()
    {
        if (Main.AssistivePluginMode.Value || !AmongUsClient.Instance.AmHost) return true;

        // 如果游戏已经判定结束，则中止执行
        if (predicate == null) return false;

        // 如果设置为不结束游戏且获胜者不是平局或错误情况，则中止执行
        if (Options.NoGameEnd.GetBool() && CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw and not CustomWinner.Error) return false;

        // 设定为内鬼杀人结束游戏的初始理由
        var reason = GameOverReason.ImpostorByKill;

        // 进行游戏结束判定
        predicate.CheckForEndGame(out reason);


        var playerList = Main.AllAlivePlayerControls.ToList();
        // 烫手山芋模式
        if (Options.CurrentGameMode == CustomGameMode.HotPotato && playerList.Count == 1)
        {
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.ColdPotato);
            ShipStatus.Instance.enabled = false;
            foreach (var cp in playerList)
                CustomWinnerHolder.WinnerIds.Add(cp.PlayerId);
            StartEndGame(reason);
            predicate = null;
            return false;

        }
        // 感染模式
        if (Options.CurrentGameMode == CustomGameMode.InfectorMode)
        {
            if (playerList.Count == InfectorManager.ZombiePlayers.Count || (InfectorManager.RemainRoundTime <= 0 && InfectorManager.HumanCompleteTasks.Count != InfectorManager.HumanNum.Count))
            {
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Infector);
                foreach (var zb in playerList)
                    CustomWinnerHolder.WinnerIds.Add(zb.PlayerId);
                ShipStatus.Instance.enabled = false;
                StartEndGame(reason);
                predicate = null;
                return false;
            }
            else if (InfectorManager.HumanCompleteTasks.Count == InfectorManager.HumanNum.Count)
            {
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Survivor);
                foreach (var hm in InfectorManager.HumanCompleteTasks)
                    CustomWinnerHolder.WinnerIds.Add(hm);
                ShipStatus.Instance.enabled = false;
                StartEndGame(reason);
                predicate = null;
                return false;
            }

        }

        // 游戏结束时
        if (CustomWinnerHolder.WinnerTeam != CustomWinner.Default)
        {
            // 强制解除伪装
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
                        .Where(pc => pc.IsCrewTeam())
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Impostor:
                    Main.AllPlayerControls
                        .Where(pc => pc.IsImpTeam())
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Jackal:
                    Main.AllPlayerControls
                     .Where(pc => pc.IsJackalTeam())
                     .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Pelican:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.Pelican))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Demon:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.Demon))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.BloodKnight:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.BloodKnight))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;

                case CustomWinner.Succubus:
                    Main.AllPlayerControls
                        .Where(pc =>pc.IsSuccubusTeam())
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;

                case CustomWinner.Vagator:
                    Main.AllPlayerControls
                     .Where(pc => pc.Is(CustomRoles.Vagator) )
                     .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;

                case CustomWinner.Martyr:
                    foreach (var m in Main.AllPlayerControls.Where(P => P.Is(CustomRoles.Martyr)))
                    {
                        var rc = m.GetRoleClass() as Martyr;
                        CustomWinnerHolder.WinnerIds.Add(m.PlayerId);
                        CustomWinnerHolder.WinnerIds.Add(rc.TargetId);
                    }
                    break;
                case CustomWinner.NightWolf:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.NightWolf))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.GodOfPlagues:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.GodOfPlagues))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.MeteorArbiter:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.MeteorArbiter))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.MeteorMurderer:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.MeteorMurderer))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.SharpShooter:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.SharpShooter))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.Yandere:
                    foreach (var y in Main.AllPlayerControls.Where(p => p.Is(CustomRoles.Yandere)))
                    {
                        var rc = y.GetRoleClass() as Yandere;
                        CustomWinnerHolder.WinnerIds.Add(y.PlayerId);
                        CustomWinnerHolder.WinnerIds.Add(rc.TargetId.PlayerId);
                    }
                    break;
                case CustomWinner.Lovers:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.Lovers))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.AdmirerLovers:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.AdmirerLovers))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.AkujoLovers:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.AkujoLovers) || pc.Is(CustomRoles.Akujo))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
                case CustomWinner.CupidLovers:
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.CupidLovers) || pc.Is(CustomRoles.Cupid))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;
            }

            if (CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw and not CustomWinner.None and not CustomWinner.Error)
            { 
                foreach (var pc in Main.AllPlayerControls)
                {
                    var roleClass = pc.GetRoleClass();

                    //抢夺胜利
                    if (roleClass is IOverrideWinner overrideWinner)
                    {
                        overrideWinner.CheckWin(ref CustomWinnerHolder.WinnerTeam, ref CustomWinnerHolder.WinnerIds);
                    }

                    //追加胜利
                    if (roleClass is IAdditionalWinner additionalWinner)
                    {
                        var winnerRole = pc.GetCustomRole();
                        var ct = pc.GetCountTypes();
                        if (additionalWinner.CheckWin(ref winnerRole, ref ct))
                        {
                            CustomWinnerHolder.WinnerIds.Add(pc.PlayerId);
                            if (pc.GetCustomRole() is not CustomRoles.SchrodingerCat)
                            CustomWinnerHolder.AdditionalWinnerRoles.Add(winnerRole);
                        }
                    }

                    //Instigator 胜利时移除玩家ID了
                    (roleClass as Instigator).CheckWin();
                }
                //if (CustomRoles.Non_Villain.IsExist() && Non_Villain.DigitalLifeList.Count <= 0)
                //    foreach (var pc in Non_Villain.DigitalLifeList)
                //    {
                //        CustomWinnerHolder.WinnerIds.Add(pc);
                //    }

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

                //Lovers.CheckWin();
                //AdmirerLovers.CheckWin();
                //AkujoLovers.CheckWin();
                //CupidLovers.CheckWin();
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
       

        //变灵魂
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
                    Logger.Info($"{pc.GetNameWithRole()}: 更改为ImpostorGhost", "ResetRoleAndEndGame");
                    sender.StartRpc(pc.NetId, RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.ImpostorGhost)
                        .Write(false)
                        .EndRpc();
                    pc.SetRole(RoleTypes.ImpostorGhost, false);
                }
                else
                {
                    Logger.Info($"{pc.GetNameWithRole()}: 更改为CrewmateGhost", "ResetRoleAndEndGame");
                    sender.StartRpc(pc.NetId, RpcCalls.SetRole)
                        .Write((ushort)RoleTypes.CrewmateGhost)
                        .Write(false)
                        .EndRpc();
                    pc.SetRole(RoleTypes.Crewmate, false);
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
            writer.WritePacked(PlayerControl.LocalPlayer.NetId); // NetId
            foreach (var info in GameData.Instance.AllPlayers)// undecided
            {
                if (ReviveRequiredPlayerIds.Contains(info.PlayerId))
                {
                    // 蘇生&メッセージ書き込み
                    info.IsDead = false;
                    writer.StartMessage(info.PlayerId);
                    info.Serialize(writer, false);
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
    public static void SetPredicateToZombie() => predicate = new ZombieGameEndPredicate();

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

            // 计数阵营记录字典
            Dictionary<CountTypes, int> playerTypeCounts = 
                Enum.GetValues(typeof(CountTypes))
                .Cast<CountTypes>()
                .ToDictionary(key => key, _ => 0); ;

            // 有效计数阵营记录字典
            var validPlayerTypeCounts = playerTypeCounts
                .Where(p => p.Key is not CountTypes.OutOfGame and not CountTypes.None)
                .ToDictionary(p => p.Key, v => v.Value);

            // 判断阵营玩家数量
            foreach (var Player in Main.AllAlivePlayerControls)
            {
                if (((Player.GetRoleClass() as MeteorArbiter)?.CanWin ?? false)
                    || ((Player.GetRoleClass() as Martyr)?.CanKill ?? false)) continue;// 先烈、陨星判官独立判断

                var playerType = Player.GetCountTypes();
                if (validPlayerTypeCounts.ContainsKey(playerType))
                {
                    validPlayerTypeCounts[playerType]++;

                    if (Player.Is(CustomRoles.Yandere))// 病娇独立判断
                    {
                        validPlayerTypeCounts[playerType]++;
                        var targetType = (Player.GetRoleClass() as Yandere).TargetId.GetCountTypes();
                        if (validPlayerTypeCounts.ContainsKey(targetType))
                            validPlayerTypeCounts[targetType]--;
                    }

                    if (Player.Is(CustomRoles.Schizophrenic))// 双重人格独立判断
                        validPlayerTypeCounts[playerType]++;
                }
                if (Player.Is(CustomRoles.SchrodingerCat))// 猫独立判断
                {
                    playerType = SchrodingerCat.GetCatTeam((Player.GetRoleClass() as SchrodingerCat).Team);
                    validPlayerTypeCounts[playerType]++;
                }
            }

            var crewCount = validPlayerTypeCounts[CountTypes.Crew];// 获取船员数量
            bool winnerFound = false;// 判断是否结束游戏的bool
            CustomWinner winningType = CustomWinner.None;// 赢家

            // 船员外所有潜在胜利者
            var potentialWinners = validPlayerTypeCounts 
                .Where(p => p.Key is not CountTypes.Crew)
                .ToDictionary(p => p.Key, v => v.Value);

            if (validPlayerTypeCounts.All(pair => pair.Value == 0)) //无人生还
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.None);
                Logger.Info($"无人生还", "CheckGameEnd");
                return true;
            }

            if (potentialWinners.All(kv => kv.Value == 0)) //船员胜利
            {
                reason = GameOverReason.HumansByVote;
                winnerFound = true;
                winningType = CustomWinner.Crewmate;
                Logger.Info($"胜利阵营暂定为{winningType}", "CheckGameEnd");
            }

            var winningCandidate = potentialWinners.FirstOrDefault(kv => kv.Value >= crewCount && kv.Value != 0);// 找到第一个可能的胜利者
            if (!potentialWinners.Any(p => p.Key != winningCandidate.Key && p.Value != 0))// 判断是否有其他有机会的胜利者
            {
                winnerFound = true;
                winningType = (CustomWinner)winningCandidate.Key; 
                Logger.Info($"胜利阵营暂定为{winningType}", "CheckGameEnd");
            }

            // 所有链子的数组
            CustomRoles[] loverRoles = { CustomRoles.Lovers, CustomRoles.AdmirerLovers, CustomRoles.AkujoLovers, CustomRoles.CupidLovers };
            var validLovers = loverRoles.Where(x => x.IsExist()).ToList();// 有效链子的列表

            //判断有没有非恋人的阵营计数
            var hasTypeSansLover = Main.AllAlivePlayerControls.Any(p =>
               potentialWinners.ContainsKey(p.GetCountTypes())
               && validLovers.All(role => !p.Is(role)));
               
            if (hasTypeSansLover || validLovers.Count() >1)// 如果有、或者不止一对链子，那就滚犊子
            {
                winnerFound = false;
            }
            // 否则，如果只有一对链子或者这对链子人数大于等于总人数一半
            else if (validLovers.Count() == 1 && Main.AllAlivePlayerControls.Count(p => p.Is(validLovers[0])) >= Main.AllAlivePlayerControls.Count()/2)
            {
                var loverRole = validLovers[0];
                winningType = (CustomWinner)loverRole;
                winnerFound = true;
                Logger.Info($"胜利阵营暂定为{winningType}", "CheckGameEnd");
            }

            if (winnerFound) // 胜利条件达成
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(winningType);
                Logger.Info($"胜利阵营判断决定为{winningType}", "CheckGameEnd");
                return true;
            }
            // 胜利条件未达成
            return false;
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
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.ColdPotato);
                foreach (var cp in playerList)
                    CustomWinnerHolder.WinnerIds.Add(cp.PlayerId);  
                return true;
            }
            return false; 
        }
    }
    class ZombieGameEndPredicate : GameEndPredicate
    {
        public override bool CheckForEndGame(out GameOverReason reason)
        {
            reason = GameOverReason.ImpostorByKill;
            var playerList = Main.AllAlivePlayerControls.ToList();
            if (playerList.Count == InfectorManager.ZombiePlayers.Count || (InfectorManager.RemainRoundTime <= 0 && InfectorManager.HumanCompleteTasks.Count != InfectorManager.HumanNum.Count))
            {
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Infector);
                foreach (var zb in playerList)
                    CustomWinnerHolder.WinnerIds.Add(zb.PlayerId);
                return true;
            }
            else if (InfectorManager.HumanCompleteTasks.Count == InfectorManager.HumanNum.Count)
            {
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Survivor);
                foreach (var hm in InfectorManager.HumanCompleteTasks)
                    CustomWinnerHolder.WinnerIds.Add(hm);
                return true;
            }
            return false; 
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