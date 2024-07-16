using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using TONEX.Roles.AddOns.Common;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.MeetingHudPatch;
using static TONEX.Options;
using static TONEX.Translator;
using System;

namespace TONEX.Roles.AddOns.Crewmate;
public static class Madmate
{
    //private static readonly int Id = 75_1_2_1500;
    private static Color RoleColor = Utils.GetRoleColor(CustomRoles.Madmate);
    private static List<byte> playerIdList = new();

    public static OptionItem ImpKnowWhosMadmate;
    public static OptionItem MadmateKnowWhosImp;
    public static OptionItem MadmateKnowWhosMadmate;
    public static OptionItem ImpCanKillMadmate;
    public static OptionItem MadmateCanKillImp;
    public static OptionItem MadmateSpawnMode;
    public static OptionItem MadmateCountMode;
    public static OptionItem SheriffCanBeMadmate;
    public static OptionItem MayorCanBeMadmate;
    public static OptionItem NGuesserCanBeMadmate;
    public static OptionItem SnitchCanBeMadmate;
    public static OptionItem JudgeCanBeMadmate;
    public static OptionItem NSwapperCanBeMadmate;
    public static OptionItem MadSnitchTasks;

    public static readonly string[] madmateSpawnMode =
   {
        "MadmateSpawnMode.Assign",
        "MadmateSpawnMode.FirstKill",
        "MadmateSpawnMode.SelfVote",
    };
    public static readonly string[] madmateCountMode =
    {
        "MadmateCountMode.None",
        "MadmateCountMode.Imp",
        "MadmateCountMode.Crew",
    };
    public static void SetupImpTotalOption()
    {
        ImpKnowWhosMadmate = BooleanOptionItem.Create(1_000_002, "ImpKnowWhosMadmate", false, TabGroup.ImpostorRoles, false)
            .SetGameMode(CustomGameMode.Standard);
        ImpCanKillMadmate = BooleanOptionItem.Create(1_000_003, "ImpCanKillMadmate", true, TabGroup.ImpostorRoles, false)
            .SetGameMode(CustomGameMode.Standard);

        MadmateKnowWhosMadmate = BooleanOptionItem.Create(1_001_001, "MadmateKnowWhosMadmate", false, TabGroup.ImpostorRoles, false)
            .SetHeader(true)
            .SetGameMode(CustomGameMode.Standard);
        MadmateKnowWhosImp = BooleanOptionItem.Create(1_001_002, "MadmateKnowWhosImp", true, TabGroup.ImpostorRoles, false)
            .SetGameMode(CustomGameMode.Standard);
        MadmateCanKillImp = BooleanOptionItem.Create(1_001_003, "MadmateCanKillImp", true, TabGroup.ImpostorRoles, false)
            .SetGameMode(CustomGameMode.Standard);
    }
    public static void SetupMadmateRoleOptionsToggle(int id, CustomGameMode customGameMode = CustomGameMode.Standard)
    {
        var role = CustomRoles.Madmate;
        var spawnOption = StringOptionItem.Create(id, role.ToString(), RoleSpwanToggle, 0, TabGroup.Addons, false).SetColor(Utils.GetRoleColor(role))
                .SetHeader(true)
                .SetGameMode(customGameMode) as StringOptionItem;

        var countOption = IntegerOptionItem.Create(id + 1, "Maximum", new(1, 15, 1), 1, TabGroup.Addons, false).SetParent(spawnOption)
            .SetGameMode(customGameMode);

        MadmateSpawnMode = StringOptionItem.Create(id + 10, "MadmateSpawnMode", madmateSpawnMode, 0, TabGroup.Addons, false).SetParent(spawnOption);
        MadmateCountMode = StringOptionItem.Create(id + 11, "MadmateCountMode", madmateCountMode, 0, TabGroup.Addons, false).SetParent(spawnOption);
        SheriffCanBeMadmate = BooleanOptionItem.Create(id + 12, "SheriffCanBeMadmate", false, TabGroup.Addons, false).SetParent(spawnOption);
        MayorCanBeMadmate = BooleanOptionItem.Create(id + 13, "MayorCanBeMadmate", false, TabGroup.Addons, false).SetParent(spawnOption);
        NGuesserCanBeMadmate = BooleanOptionItem.Create(id + 14, "NGuesserCanBeMadmate", false, TabGroup.Addons, false).SetParent(spawnOption);
        SnitchCanBeMadmate = BooleanOptionItem.Create(id + 15, "SnitchCanBeMadmate", false, TabGroup.Addons, false).SetParent(spawnOption);
        MadSnitchTasks = IntegerOptionItem.Create(id + 16, "MadSnitchTasks", new(1, 99, 1), 3, TabGroup.Addons, false).SetParent(SnitchCanBeMadmate)
            .SetValueFormat(OptionFormat.Pieces);
        JudgeCanBeMadmate = BooleanOptionItem.Create(id + 17, "JudgeCanBeMadmate", false, TabGroup.Addons, false).SetParent(spawnOption);
        NSwapperCanBeMadmate = BooleanOptionItem.Create(id + 18, "NSwapperCanBeMadmate", false, TabGroup.Addons, false).SetParent(spawnOption);
        CustomRoleSpawnChances.Add(role, spawnOption);
        CustomRoleCounts.Add(role, countOption);
    }
    [GameModuleInitializer]
    public static void Init()
    {
        playerIdList = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
    }
    public static bool IsEnable => playerIdList.Count > 0;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);

    public static void KnowTargetRoleColor(PlayerControl seer, PlayerControl target, ref string color)
    {
        // 内鬼叛徒互认
        if (seer.Is(CustomRoleTypes.Impostor) && target.Is(CustomRoleTypes.Impostor)) color = (target.Is(CustomRoles.Egoist) && Egoist.OptionImpEgoVisibalToAllies.GetBool() && seer != target) ? Utils.GetRoleColorCode(CustomRoles.Egoist) : Utils.GetRoleColorCode(CustomRoles.Impostor);
        if (seer.Is(CustomRoles.Madmate) && target.Is(CustomRoleTypes.Impostor) && MadmateKnowWhosImp.GetBool()) color = Main.roleColors[CustomRoles.Impostor];
        if (seer.Is(CustomRoleTypes.Impostor) && target.Is(CustomRoles.Madmate) && ImpKnowWhosMadmate.GetBool()) color = Main.roleColors[CustomRoles.Madmate];
        if (seer.Is(CustomRoles.Madmate) && target.Is(CustomRoles.Madmate) && MadmateKnowWhosMadmate.GetBool()) color = Main.roleColors[CustomRoles.Madmate];
        if (seer.Is(CustomRoles.Gangster) && target.Is(CustomRoles.Madmate)) color = Main.roleColors[CustomRoles.Madmate];
    }
    public static bool CanKnowOthers(PlayerControl seer, PlayerControl seen)
    {
        if ((seer.Is(CustomRoles.Madmate) && seen.Is(CustomRoleTypes.Impostor) && MadmateKnowWhosImp.GetBool())
                    || (seer.Is(CustomRoleTypes.Impostor) && seen.Is(CustomRoles.Madmate) && ImpKnowWhosMadmate.GetBool())
                    || (seer.Is(CustomRoles.Madmate) && seen.Is(CustomRoles.Madmate) && MadmateKnowWhosMadmate.GetBool()))
            return true;
        return false;
    }
    public static bool CanBeMadmate(this PlayerControl pc)
    {
        return pc != null &&pc.IsCrew() && !pc.Is(CustomRoles.Madmate)
        && !(
            (pc.Is(CustomRoles.Sheriff) && !SheriffCanBeMadmate.GetBool()) ||
            (pc.Is(CustomRoles.Mayor) && !MayorCanBeMadmate.GetBool()) ||
            (pc.Is(CustomRoles.NiceGuesser) && !NGuesserCanBeMadmate.GetBool()) ||
            (pc.Is(CustomRoles.Snitch) && !SnitchCanBeMadmate.GetBool()) ||
            (pc.Is(CustomRoles.Judge) && !JudgeCanBeMadmate.GetBool()) ||
            (pc.Is(CustomRoles.NiceSwapper) && !NSwapperCanBeMadmate.GetBool()) ||
            pc.Is(CustomRoles.LazyGuy) ||
            pc.Is(CustomRoles.Egoist)
            );
    }
    public static bool CheckVoteAsVoter(byte srcPlayerId, byte suspectPlayerId, PlayerControl voter,ref MeetingHud __instance)
    {
        //主动叛变模式
        if (MadmateSpawnMode.GetInt() == 2 && srcPlayerId == suspectPlayerId)
        {
            if (Main.AllPlayerControls.Count(p => p.Is(CustomRoles.Madmate)) < CustomRoles.Madmate.GetCount() && voter.CanBeMadmate())
            {
                voter.RpcSetCustomRole(CustomRoles.Madmate);
                if (voter.Is(CustomRoles.Snitch))
                {
                    var taskState = voter.GetPlayerTaskState();
                    taskState.AllTasksCount = Madmate.MadSnitchTasks.GetInt();
                    if (AmongUsClient.Instance.AmHost)
                    {
                        voter.Data.RpcSetTasks(Array.Empty<byte>());
                        voter.SyncSettings();
                        Utils.NotifyRoles();
                    }
                }
                Logger.Info($"注册附加职业：{voter.GetNameWithRole()} => {CustomRoles.Madmate}", "AssignCustomSubRoles");
                voter.ShowPopUp(GetString("MadmateSelfVoteModeSuccessfulMutiny"));
                Utils.SendMessage(GetString("MadmateSelfVoteModeSuccessfulMutiny"), voter.PlayerId);
            }
            else
            {
                voter.ShowPopUp(GetString("MadmateSelfVoteModeMutinyFailed"));
                Utils.SendMessage(GetString("MadmateSelfVoteModeMutinyFailed"), voter.PlayerId);
            }
            __instance.RpcClearVote(voter.GetClientId());
            Logger.Info($"{voter.GetNameWithRole()} 的投票被清除", nameof(CastVotePatch));
            return false;
        }
        else return true;
    }
    public static void AssignMadmateRoles()
    {
        if (!CustomRoles.Madmate.IsEnable() || Madmate.MadmateSpawnMode.GetInt() != 0) return;
        var allPlayers = Main.AllPlayerControls.Where(x => x.CanBeMadmate()).ToList();
        var count = Math.Clamp(CustomRoles.Madmate.GetCount(), 0, allPlayers.Count);
        if (count <= 0) return;
        for (var i = 0; i < count; i++)
        {
            var player = allPlayers[IRandom.Instance.Next(0, allPlayers.Count)];
            allPlayers.Remove(player);
            PlayerState.GetByPlayerId(player.PlayerId).SetSubRole(CustomRoles.Madmate);
            new LateTask(() =>
            {
                if (player.Is(CustomRoles.Snitch))
                {
                    var taskState = player.GetPlayerTaskState();
                    taskState.AllTasksCount = Madmate.MadSnitchTasks.GetInt();
                    if (AmongUsClient.Instance.AmHost)
                    {
                        player.Data.RpcSetTasks(Array.Empty<byte>());
                        player.SyncSettings();
                        Utils.NotifyRoles();
                    }
                }
            }, 2F, "MadMate");
            Logger.Info($"注册附加职业：{player?.Data?.PlayerName}（{player.GetCustomRole()}）=> {CustomRoles.Madmate}", "AssignCustomSubRoles");
        }
    }
    public static bool FirstKill(PlayerControl killer, PlayerControl target)
    {
        if (!killer.IsImpTeam() || !target.IsCrew() || target.IsImpTeam() ||MadmateSpawnMode.GetInt() !=1 || Main.FirstDied != byte.MaxValue) return false;
        target.RpcSetCustomRole(CustomRoles.Madmate);
        if (target.Is(CustomRoles.Snitch))
        {
            var taskState = target.GetPlayerTaskState();
            taskState.AllTasksCount = Madmate.MadSnitchTasks.GetInt();
            if (AmongUsClient.Instance.AmHost)
            {
                target.Data.RpcSetTasks(Array.Empty<byte>());
                target.SyncSettings();
                Utils.NotifyRoles();
            }
        }
        Logger.Info($"注册附加职业：{target.GetNameWithRole()} => {CustomRoles.Madmate}", "AssignCustomSubRoles");
        target.RpcProtectedMurderPlayer();
        return true;
    }
    public static void TaskAssgin(PlayerControl pc, ref bool hasCommonTasks, ref int NumLongTasks, ref int NumShortTasks)
    {
        if (pc.Is(CustomRoles.Snitch) && pc.Is(CustomRoles.Madmate))
        {
            hasCommonTasks = false;
            NumLongTasks = 0;
            NumShortTasks = MadSnitchTasks.GetInt();
        }
    }
}