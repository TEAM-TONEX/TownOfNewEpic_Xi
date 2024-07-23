using AmongUs.GameOptions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using TONEX.Roles.AddOns.Crewmate;
using TONEX.Roles.Core;
using TONEX.Roles.Neutral;

namespace TONEX;

public class PlayerState
{
    byte PlayerId;
    public CustomRoles MainRole;
    public List<CustomRoles> SubRoles;
    public CountTypes CountType { get; private set; }
    public bool IsDead { get; set; }
    public CustomDeathReason DeathReason { get; set; }
    public TaskState taskState;
    public bool IsBlackOut { get; set; }
    private bool _canUseMovingPlatform = true;
    public bool CanUseMovingPlatform
    {
        get => _canUseMovingPlatform;
        set
        {
            Logger.Info($"ID: {PlayerId} の昇降機可用性を {value} に設定", nameof(PlayerState));
            _canUseMovingPlatform = value;
        }
    }
    public (DateTime, byte) RealKiller;
    public PlainShipRoom LastRoom;
    /// <summary>会議等の後に湧いた後かどうか</summary>
    public bool HasSpawned { get; set; } = false;
    public Dictionary<byte, string> TargetColorData;
    public PlayerState(byte playerId)
    {
        MainRole = CustomRoles.NotAssigned;
        SubRoles = new();
        CountType = CountTypes.OutOfGame;
        PlayerId = playerId;
        IsDead = false;
        DeathReason = CustomDeathReason.etc;
        taskState = new();
        IsBlackOut = false;
        RealKiller = (DateTime.MinValue, byte.MaxValue);
        LastRoom = null;
        TargetColorData = new();
    }
    public CustomRoles GetCustomRole()
    {
        var RoleInfo = Utils.GetPlayerInfoById(PlayerId);
        return RoleInfo.Role == null
            ? MainRole
            : RoleInfo.Role.Role switch
            {
                RoleTypes.Crewmate => CustomRoles.Crewmate,
                RoleTypes.Engineer => CustomRoles.Engineer,
                RoleTypes.Scientist => CustomRoles.Scientist,
                RoleTypes.Tracker => CustomRoles.Tracker,
                RoleTypes.Noisemaker => CustomRoles.Noisemaker,
                RoleTypes.GuardianAngel => CustomRoles.GuardianAngel,
                RoleTypes.Impostor => CustomRoles.Impostor,
                RoleTypes.Shapeshifter => CustomRoles.Shapeshifter,
                RoleTypes.Phantom => CustomRoles.Phantom,
                _ => CustomRoles.Crewmate,
            };
    }
    public void SetMainRole(CustomRoles role)
    {
        MainRole = role;

        CountType = CustomRoleManager.GetRoleInfo(role) is SimpleRoleInfo roleInfo ?
               roleInfo.CountType :
               role switch
               {
                   CustomRoles.GM => CountTypes.OutOfGame,
                   _ => role.IsImpostor() ? CountTypes.Impostor : CountTypes.Crew,
               };
        if (role is CustomRoles.Sidekick or CustomRoles.Whoops)
        {
            CountType = CountTypes.Jackal;
            SubRoles.Remove(CustomRoles.Madmate);
            SubRoles.Remove(CustomRoles.Charmed);
            SubRoles.Remove(CustomRoles.LastImpostor);
        }
        if (role is CustomRoles.EvilAngel)
        {
            SubRoles.Remove(CustomRoles.Madmate);
            SubRoles.Remove(CustomRoles.LastImpostor);
        }
    }
    public void SetSubRole(CustomRoles role, bool AllReplace = false)
    {
        if (AllReplace)
            SubRoles.ToArray().Do(role => SubRoles.Remove(role));

        if (!SubRoles.Contains(role))
            SubRoles.Add(role);

        if (role == CustomRoles.Madmate)
        {
            CountType = Madmate.MadmateCountMode.GetInt() switch
            {
                0 => CountTypes.OutOfGame,
                1 => CountTypes.Impostor,
                2 => CountTypes.Crew,
                _ => throw new NotImplementedException()
            };
            SubRoles.Remove(CustomRoles.Charmed);
            SubRoles.Remove(CustomRoles.Wolfmate);
        }
        if (role == CustomRoles.Charmed)
        {
            CountType = Succubus.OptionCharmedCountMode.GetInt() switch
            {
                0 => CountTypes.OutOfGame,
                1 => CountTypes.Succubus,
                2 => CountType,
                _ => throw new NotImplementedException()
            };
            SubRoles.Remove(CustomRoles.Madmate);
            SubRoles.Remove(CustomRoles.Wolfmate);
            SubRoles.Remove(CustomRoles.LastImpostor);
        }
        if (role == CustomRoles.Wolfmate)
        {
            CountType = CountTypes.Jackal;
            SubRoles.Remove(CustomRoles.Madmate);
            SubRoles.Remove(CustomRoles.Charmed);
            SubRoles.Remove(CustomRoles.LastImpostor);
        }
    }
    public void RemoveSubRole(CustomRoles role)
    {
        if (SubRoles.Contains(role))
            SubRoles.Remove(role);
    }

    public void SetDead()
    {
        IsDead = true;
        if (AmongUsClient.Instance.AmHost)
        {
            RPC.SendDeathReason(PlayerId, DeathReason);
        }
    }
    public bool IsSuicide() { return DeathReason == CustomDeathReason.Suicide; }
    public TaskState GetTaskState() { return taskState; }
    public void InitTask(PlayerControl player)
    {
        taskState.Init(player);
    }
    public void UpdateTask(PlayerControl player)
    {
        taskState.Update(player);
    }

    public byte GetRealKiller()
        => IsDead && RealKiller.Item1 != DateTime.MinValue ? RealKiller.Item2 : byte.MaxValue;
    public int GetKillCount(bool ExcludeSelfKill = false)
    {
        int count = 0;
        foreach (var state in AllPlayerStates.Values)
            if (!(ExcludeSelfKill && state.PlayerId == PlayerId) && state.GetRealKiller() == PlayerId)
                count++;
        return count;
    }
    public void SetCountType(CountTypes countType) => CountType = countType;

    private static Dictionary<byte, PlayerState> allPlayerStates = new(15);
    public static IReadOnlyDictionary<byte, PlayerState> AllPlayerStates => allPlayerStates;

    public static PlayerState GetByPlayerId(byte playerId) => AllPlayerStates.TryGetValue(playerId, out var state) ? state : null;

    [GameModuleInitializer]
    public static void Clear() => allPlayerStates.Clear();
    public static void Create(byte playerId)
    {
        if (allPlayerStates.ContainsKey(playerId))
        {
            Logger.Warn($"重複したIDのPlayerStateが作成されました: {playerId}", nameof(PlayerState));
            return;
        }
        allPlayerStates[playerId] = new(playerId);
    }
}
public class TaskState
{
    public static int InitialTotalTasks;
    public int AllTasksCount;
    public int CompletedTasksCount;
    public bool hasTasks;
    public int RemainingTasksCount => AllTasksCount - CompletedTasksCount;
    public bool DoExpose => RemainingTasksCount <= Options.SnitchExposeTaskLeft && hasTasks;
    public bool IsTaskFinished => RemainingTasksCount <= 0 && hasTasks;
    public TaskState()
    {
        this.AllTasksCount = -1;
        this.CompletedTasksCount = 0;
        this.hasTasks = false;
    }

    public void Init(PlayerControl player)
    {
        Logger.Info($"{player.GetNameWithRole()}: InitTask", "TaskState.Init");
        if (player == null || player.Data == null || player.Data.Tasks == null) return;
        if (!Utils.HasTasks(player.Data, false))
        {
            AllTasksCount = 0;
            return;
        }
        hasTasks = true;
        AllTasksCount = player.Data.Tasks.Count;
        Logger.Info($"{player.GetNameWithRole()}: TaskCounts = {CompletedTasksCount}/{AllTasksCount}", "TaskState.Init");
    }
    public void Update(PlayerControl player)
    {
        Logger.Info($"{player.GetNameWithRole()}: UpdateTask", "TaskState.Update");

        //初期化出来ていなかったら初期化
        if (AllTasksCount == -1) Init(player);

        if (!hasTasks) return;

        //クリアしてたらカウントしない
        if (CompletedTasksCount >= AllTasksCount) return;

        CompletedTasksCount++;

        //調整後のタスク量までしか表示しない
        CompletedTasksCount = Math.Min(AllTasksCount, CompletedTasksCount);
        Logger.Info($"{player.GetNameWithRole()}: TaskCounts = {CompletedTasksCount}/{AllTasksCount}", "TaskState.Update");
    }
    public bool HasCompletedEnoughCountOfTasks(int count) =>
            IsTaskFinished || CompletedTasksCount >= count;
}
public class PlayerVersion
{
    public readonly Version version;
    public readonly string tag;
    public readonly string forkId;
    public PlayerVersion(string ver, string tag_str, string forkId) : this(Version.Parse(ver), tag_str, forkId) { }
    public PlayerVersion(Version ver, string tag_str, string forkId)
    {
        version = ver;
        tag = tag_str;
        this.forkId = forkId;
    }
    public bool IsEqual(PlayerVersion pv)
    {
        return pv.version == version && pv.tag == tag;
    }
}
public static class GameStates
{
    public static bool InGame = false;
    public static bool AlreadyDied = false;
    public static bool IsModHost => Main.AllPlayerControls.ToArray().FirstOrDefault(x => x.OwnerId == AmongUsClient.Instance.HostId && x.IsModClient());
    public static bool IsLobby => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Joined;
    public static bool IsInGame => InGame ||IsFreePlay;
    public static bool IsEnded => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Ended;
    public static bool IsNotJoined => AmongUsClient.Instance.GameState == AmongUsClient.GameStates.NotJoined;
    public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    public static bool IsLocalGame => AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
    public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
    public static bool IsInTask => InGame && !MeetingHud.Instance;
    public static bool IsMeeting => InGame && MeetingHud.Instance;
    public static bool IsVoting => IsMeeting && MeetingHud.Instance.state is MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted;
    public static bool IsCountDown => GameStartManager.InstanceExists && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown;
    public static bool IsShip => ShipStatus.Instance != null;
    public static bool IsCanMove => PlayerControl.LocalPlayer?.CanMove is true;
    public static bool IsDead => PlayerControl.LocalPlayer?.Data?.IsDead is true;
    public static bool AirshipIsActive => (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId == MapNames.Airship;
}
public static class MeetingStates
{
    public static DeadBody[] DeadBodies = null;
    public static NetworkedPlayerInfo ReportTarget = null;
    public static bool IsEmergencyMeeting => ReportTarget == null;
    public static bool IsExistDeadBody => DeadBodies.Length > 0;
    public static bool MeetingCalled = false;
    public static bool FirstMeeting = true;
}