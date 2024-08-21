using System;
using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;
using static TONEX.Options;

namespace TONEX.Roles.AddOns.Common;
public sealed class Workhorse : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Workhorse),
    player => new Workhorse(player),
    CustomRoles.Workhorse,
   80400,
    SetupCustomOption,
    "wh|加班",
    "#00ffff",
    1
    );
    public Workhorse(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    enum OptionName
    {
        AssignOnlyToCrewmate,
        SnitchCanBeWorkhorse,
        WorkhorseNumLongTasks,
        WorkhorseNumShortTasks
    }
    private static OptionItem OptionAssignOnlyToCrewmate;
    private static OptionItem OptionSnitchCanBeWorkhorse;
    private static OptionItem OptionNumLongTasks;
    private static OptionItem OptionNumShortTasks;
    public static bool AssignOnlyToCrewmate;
    public static bool SnitchCanBeWorkhorse;
    public static int NumLongTasks;
    public static int NumShortTasks;
    public static int WorkhorseNum = 0;
    public static void SetupCustomOption()
    {
        OptionAssignOnlyToCrewmate = BooleanOptionItem.Create(RoleInfo, 20, OptionName.AssignOnlyToCrewmate, true, false);
        OptionSnitchCanBeWorkhorse = BooleanOptionItem.Create(RoleInfo, 21, OptionName.SnitchCanBeWorkhorse, false, false);
        OptionNumLongTasks = IntegerOptionItem.Create(RoleInfo, 22, OptionName.WorkhorseNumLongTasks, new(0, 5, 1), 1, false)
            .SetValueFormat(OptionFormat.Pieces);
        OptionNumShortTasks = IntegerOptionItem.Create(RoleInfo, 23, OptionName.WorkhorseNumShortTasks, new(0, 5, 1), 1, false)
            .SetValueFormat(OptionFormat.Pieces);
    }
    public override void Add()
    {
        AssignOnlyToCrewmate = OptionAssignOnlyToCrewmate.GetBool();
        SnitchCanBeWorkhorse = OptionSnitchCanBeWorkhorse.GetBool();
        NumLongTasks = OptionNumLongTasks.GetInt();
        NumShortTasks = OptionNumShortTasks.GetInt();
        WorkhorseNum = 0;
    }
    public static (bool, int, int) TaskData => (false, NumLongTasks, NumShortTasks);
    private static bool IsAssignTarget(PlayerControl pc)
    {
        if (!pc.IsAlive()) return false;
        var taskState = pc.GetPlayerTaskState();
        if (taskState.CompletedTasksCount < taskState.AllTasksCount) return false;
        if (!Utils.HasTasks(pc.Data)) return false;
        if (pc.Is(CustomRoles.Snitch) && !SnitchCanBeWorkhorse) return false;
        if (AssignOnlyToCrewmate)
            return pc.Is(CustomRoleTypes.Crewmate);
        return !OverrideTasksData.AllData.ContainsKey(pc.GetCustomRole()); //タスク上書きオプションが無い
    }
    public static bool OnCompleteTask(PlayerControl pc)
    {
        if (WorkhorseNum >= CustomRoles.Workhorse.GetCount()) return true;
        if (!IsAssignTarget(pc)) return true;
        WorkhorseNum++;
        pc.RpcSetCustomRole(CustomRoles.Workhorse);
        var taskState = pc.GetPlayerTaskState();
        taskState.AllTasksCount += NumLongTasks + NumShortTasks;

        if (AmongUsClient.Instance.AmHost)
        {
            pc.Data.RpcSetTasks(Array.Empty<byte>()); //タスクを再配布
            pc.SyncSettings();
            Utils.NotifyRoles();
        }

        return false;
    }
}
