using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Roles.AddOns.CanNotOpened;
using TONEX.Roles.AddOns.Common;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Crewmate;
using TONEX.Roles.Neutral;
using UnityEngine.Bindings;
using static TONEX.Translator;

namespace TONEX;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
class GameEndChecker
{
    private static GameEndPredicate predicate;
    public static bool Prefix()
    {
        if (Main.AssistivePluginMode.Value) return true;
        
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
                case CustomWinner.Jackal:
                    Main.AllPlayerControls
                     .Where(pc => pc.Is(CustomRoles.Jackal) || pc.Is(CustomRoles.Wolfmate) || pc.Is(CustomRoles.Sidekick) || pc.Is(CustomRoles.Whoops))
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
                        .Where(pc => pc.Is(CustomRoles.Succubus) || pc.Is(CustomRoles.Charmed))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;

                case CustomWinner.Vagator:
                    Main.AllPlayerControls
                     .Where(pc => pc.Is(CustomRoles.Vagator) )
                     .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
                    break;

                case CustomWinner.Martyr:
                    Main.AllPlayerControls
                     .Where(pc => pc.Is(CustomRoles.Martyr) || pc.PlayerId == Martyr.TargetId)
                     .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
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
                    Main.AllPlayerControls
                        .Where(pc => pc.Is(CustomRoles.Yandere))
                        .Do(pc => CustomWinnerHolder.WinnerIds.Add(pc.PlayerId));
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
                //抢夺胜利
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.GetRoleClass() is IOverrideWinner overrideWinner)
                    {
                        overrideWinner.CheckWin(ref CustomWinnerHolder.WinnerTeam, ref CustomWinnerHolder.WinnerIds);
                    }
                }
                
                //Instigator 胜利时移除玩家ID了
                if (CustomRoles.Instigator.IsExist() && Instigator.ForInstigator.Count != 0)
                {
                    foreach (var pc in Main.AllPlayerControls)
                    {
                        if (Instigator.ForInstigator.Contains(pc.PlayerId))
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
                            if (pc.GetCustomRole() is not CustomRoles.SchrodingerCat)
                            CustomWinnerHolder.AdditionalWinnerRoles.Add(winnerRole);
                        }
                    }
                }
                //if (CustomRoles.Non_Villain.IsExist() && Non_Villain.DigitalLifeList.Count <=0) 
                 //   foreach (var pc in Non_Villain.DigitalLifeList)
                   // {
                     //       CustomWinnerHolder.WinnerIds.Add(pc);
                   // }
                

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

            // 计数阵营记录字典
            Dictionary<CountTypes, int> playerTypeCounts = new();
            playerTypeCounts.Clear();
            foreach (var ct in System.Enum.GetValues(typeof(CountTypes)))
            {
                if (ct is CountTypes.OutOfGame or CountTypes.None) continue;
                playerTypeCounts.TryAdd((CountTypes)ct, 0);
            }

            foreach (var Player in Main.AllAlivePlayerControls)// 判断阵营玩家数量
            {
                if ((Player.GetRoleClass() as MeteorArbiter)?.CanWin ?? false || Player.Is(CustomRoles.Martyr) && !Martyr.CanKill) continue;// 先烈、陨星判官独立判断
                var playerType = Player.GetCountTypes();
                if (playerTypeCounts.ContainsKey(playerType))
                {
                    playerTypeCounts[playerType]++;
                    if (Player.Is(CustomRoles.Yandere))// 病娇独立判断
                    {
                        playerTypeCounts[playerType]++;
                        var targetType = (Player.GetRoleClass() as Yandere).TargetId.GetCountTypes();
                        if (playerTypeCounts.ContainsKey(targetType))
                            playerTypeCounts[targetType]--;
                    }
                    if (Player.Is(CustomRoles.Schizophrenic))// 双重人格独立判断
                        playerTypeCounts[playerType]++;
                    if (Player.Is(CustomRoles.SchrodingerCat))// 猫独立判断
                    {

                        playerType = SchrodingerCat.GetCatTeam((Player.GetRoleClass() as SchrodingerCat).Team);
                        playerTypeCounts[playerType]++;
                    }
                }
            }

            var win = false;// 是否进入判断阵营胜利的bool
            KeyValuePair<CountTypes, int> maywinner = new();// 定义存储胜利者的键值对
            var crewValue = playerTypeCounts.ElementAt(0).Value;// 船员阵营数量
            var nonCrewPlayerTypes = playerTypeCounts.Skip(1).Where(kv => kv.Value >= crewValue && kv.Value != 0).ToList(); // 没有船员阵营的可能的胜利者键值对列表
            foreach (var maywin in nonCrewPlayerTypes)// 寻找剩余玩家数量大于等于船员的阵营键值对
            { 
                win = true; // 假设为胜利者
                foreach (var kv in playerTypeCounts.Skip(1))
                {
                    if (kv.Key == maywin.Key || kv.Value == 0) continue;
                    win = false; // 如果有其他阵营的值不为 0，则不是胜利者
                    
                    break;
                }
                if (win)
                {
                    maywinner = maywin; // 确定胜利者
                    Logger.Info($"胜利阵营已决定", "CheckGameEnd");
                    break;
                }
            }
            var playerCount = Main.AllAlivePlayerControls.ToList().Count;
            var skip = false;
            if (playerTypeCounts.All(pair => pair.Value == 0)) //全灭
            {
                reason = GameOverReason.ImpostorByKill;
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.None);
            }

            CustomRoles[] loverRoles = { CustomRoles.Lovers, CustomRoles.AdmirerLovers, CustomRoles.AkujoLovers, CustomRoles.CupidLovers };

            foreach (var loverRole in loverRoles)// 多种恋人判断胜利
            {
                if (!loverRole.IsExist()) continue;
                if (Main.AllAlivePlayerControls.Count(p => p.Is(loverRole)) >= (playerCount / 2) 
                    && !Main.AllAlivePlayerControls
                    .Where(p => !p.Is(loverRole))// 不是恋人
                    .Any(p => playerTypeCounts.ContainsKey(p.GetCountTypes()) // 被计数包含
                    && !p.IsCrew() // 不是船员
                    || ForLover(p, loverRole))) //或者是恋人
                {
                    skip = true;
                    reason = GameOverReason.ImpostorByKill;

                    CustomWinner customWinner;
                    switch (loverRole)
                    {
                        case CustomRoles.Lovers:
                            customWinner = CustomWinner.Lovers;
                            break;
                        case CustomRoles.AdmirerLovers:
                            customWinner = CustomWinner.AdmirerLovers;
                            break;
                        case CustomRoles.AkujoLovers:
                            customWinner = CustomWinner.AkujoLovers;
                            break;
                        case CustomRoles.CupidLovers:
                            customWinner = CustomWinner.CupidLovers;
                            break;
                        default:
                            customWinner = CustomWinner.None;
                            break;
                    }
                    Logger.Info($"胜利阵营已决定", "CheckGameEnd");
                    CustomWinnerHolder.ResetAndSetWinner(customWinner);

                    break; // 结束循环
                }
            }
            if (!skip)
            {
                if (win)// 确定有胜利阵营，开始根据不同阵营判断
                {
                    reason = GameOverReason.ImpostorByKill;
                    CustomWinnerHolder.ResetAndSetWinner((CustomWinner)maywinner.Key);// 将胜利阵营键值对的键写入并且转化为CustomWinner
                }
                else if (playerTypeCounts.Skip(1).All(kv => kv.Value == 0)) //船员胜利
                {
                    reason = GameOverReason.HumansByVote;
                    CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Crewmate);
                }
                else
                {
                    return false; //胜利条件未达成
                }
            }
            return true;
        }

    }
    static bool ForLover(PlayerControl p,CustomRoles role)
    {
        List<CustomRoles> LoverRoles = new()
        {
            CustomRoles.Lovers,
            CustomRoles.AdmirerLovers,
            CustomRoles.CupidLovers,
            CustomRoles.AkujoLovers
            
        };
        LoverRoles.Remove(role);
        bool hasCommonItems = LoverRoles.Intersect(p.GetCustomSubRoles()).Any();
        return hasCommonItems;
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