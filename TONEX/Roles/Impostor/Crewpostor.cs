﻿using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;

using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;

namespace TONEX.Roles.Impostor;
public sealed class CrewPostor : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(CrewPostor),
            player => new CrewPostor(player),
            CustomRoles.CrewPostor,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Impostor,
            4800,
            SetupOptionItem,
            "ca|舰长",
            experimental: true
        );
    public CrewPostor(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.ForRecompute
    )
    { }

    static OptionItem OptionCanKillAllies;

    public bool CanKill { get; private set; } = false;
    private static void SetupOptionItem()
    {
        OptionCanKillAllies = BooleanOptionItem.Create(RoleInfo, 10, GeneralOption.CanKillAllies, false, false);
        Options.OverrideTasksData.Create(RoleInfo, 20);
    }
    public override bool OnCompleteTask(out bool cancel)
    {
        List<PlayerControl> list = Main.AllAlivePlayerControls.Where(x => !Is(x) && (OptionCanKillAllies.GetBool() || !x.Is(CustomRoleTypes.Impostor))).ToList();

        if (list.Count < 1)
        {
            Logger.Info($"船鬼没有可击杀目标", "CrewPostor");
        }
        else
        {
            list = list.OrderBy(x => Vector2.Distance(Player.GetTruePosition(), x.GetTruePosition())).ToList();
            var target = list[0];

            CustomRoleManager.OnCheckMurder(
                Player, target,
                target, target
                );

            if (Player.IsModClient()) RPC.PlaySoundRPC(Player.PlayerId, Sounds.KillSound);
            else Player.RpcProtectedMurderPlayer();

            Logger.Info($"船鬼完成任务击杀：{Player.GetNameWithRole()} => {target.GetNameWithRole()}", "CrewPostor.OnCompleteTask");
        }

        cancel = false;
        return false;
    }
}