using AmongUs.GameOptions;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TONEX.Attributes;
using TONEX.Modules;
using TONEX.Roles.Core;
using TONEX.Roles.Neutral;
namespace TONEX;

public static class AntiBlackout
{
    ///<summary>
    ///是否覆盖放逐处理
    ///</summary>
    public static bool OverrideExiledPlayer => IsRequired && (IsSingleImpostor || Diff_CrewImp == 1);
    ///<summary>
    ///是否只有一个内鬼
    ///</summary>
    public static bool IsSingleImpostor 
        => (Main.RealOptionsData?.GetInt(Int32OptionNames.NumImpostors) ?? Main.NormalOptions.NumImpostors) 
        - Main.AllPlayerControls.Count(x => GameStates.IsInGame && x.Is(CustomRoles.CrewPostor)) <= 1;
    ///<summary>
    ///AntiBlackout内的处理是否必要
    ///</summary>
    public static bool IsRequired
            => Options.NoGameEnd.GetBool()
                || Enum.GetValues(typeof(CustomRoles))
                .Cast<CustomRoles>()
                .Any(role => role.GetRoleInfo()?.IsNK ?? false && role.IsExist(true));
    ///<summary>
    ///非内鬼玩家人数与内鬼人数之差
    ///</summary>
    public static int Diff_CrewImp
    {
        get
        {
            int numImpostors = 0;
            int numCrewmates = 0;
            foreach (var pc in Main.AllPlayerControls)
            {
                if (pc.Data.Role.IsImpostor)
                {
                    numImpostors++;
                }
                else
                {
                    numCrewmates++;
                }
            }
            return numCrewmates - numImpostors;
        }
    }


    public static bool IsCached { get; private set; } = false;
    private static Dictionary<byte, (bool isDead, bool Disconnected)> isDeadCache = new();
    private readonly static LogHandler logger = Logger.Handler("AntiBlackout");

    public static void SetIsDead(bool doSend = true, [CallerMemberName] string callerMethodName = "")
    {
        logger.Info($"SetIsDead is called from {callerMethodName}");
        if (IsCached)
        {
            logger.Info("再度SetIsDeadを実行する前に、RestoreIsDeadを実行してください。");
            return;
        }
        isDeadCache.Clear();
        foreach (var info in GameData.Instance.AllPlayers)
        {
            if (info == null) continue;
            isDeadCache[info.PlayerId] = (info.IsDead, info.Disconnected);
            info.IsDead = false;
            info.Disconnected = false;
        }
        IsCached = true;
        if (doSend) SendGameData();
    }
    public static void RestoreIsDead(bool doSend = true, [CallerMemberName] string callerMethodName = "")
    {
        logger.Info($"RestoreIsDead is called from {callerMethodName}");
        foreach (var info in GameData.Instance.AllPlayers)
        {
            if (info == null) continue;
            if (isDeadCache.TryGetValue(info.PlayerId, out var val))
            {
                info.IsDead = val.isDead;
                info.Disconnected = val.Disconnected;
            }
        }
        isDeadCache.Clear();
        IsCached = false;
        if (doSend) SendGameData();
    }

    public static void SendGameData([CallerMemberName] string callerMethodName = "")
    {
        if (/* Main.AssistivePluginMode.Value */ false) return;
        logger.Info($"SendGameData is called from {callerMethodName}");
        foreach (var innerNetObject in GameData.Instance.AllPlayers)
        {
            innerNetObject.SetDirtyBit(uint.MaxValue);
        }
        AmongUsClient.Instance.SendAllStreamedObjects();
    }
    public static void OnDisconnect(NetworkedPlayerInfo player)
    {
        // 実行条件: クライアントがホストである, IsDeadが上書きされている, playerが切断済み
        if (!AmongUsClient.Instance.AmHost || !IsCached || !player.Disconnected) return;
        isDeadCache[player.PlayerId] = (true, true);
        player.IsDead = player.Disconnected = false;
        SendGameData();
    }

    ///<summary>
    ///一時的にIsDeadを本来のものに戻した状態でコードを実行します
    ///<param name="action">実行内容</param>
    ///</summary>
    public static void TempRestore(Action action)
    {
        logger.Info("==Temp Restore==");
        //IsDeadが上書きされた状態でTempRestoreが実行されたかどうか
        bool before_IsCached = IsCached;
        try
        {
            if (before_IsCached) RestoreIsDead(doSend: false);
            action();
        }
        catch (Exception ex)
        {
            logger.Warn("AntiBlackout.TempRestore内で例外が発生しました");
            logger.Exception(ex);
        }
        finally
        {
            if (before_IsCached) SetIsDead(doSend: false);
            logger.Info("==/Temp Restore==");
        }
    }

    [GameModuleInitializer]
    public static void Reset()
    {
        logger.Info("==Reset==");
        if (isDeadCache == null) isDeadCache = new();
        isDeadCache.Clear();
        IsCached = false;
    }
}