﻿using AmongUs.GameOptions;
using Hazel;
using MS.Internal.Xml.XPath;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX.Roles.Neutral;
public sealed class Sidekick : RoleBase ,INeutralKilling, IKiller, IIndependent, ISchrodingerCatOwner
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Sidekick),
            player => new Sidekick(player),
            CustomRoles.Sidekick,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            94_5_1_0,
            null,
            "si",
            "#00b4eb",
            true,
            true,
            countType: CountTypes.Jackal,
            ctop: true

        );
    public Sidekick(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    { }
    public bool CanUseSabotageButton() => false;
    public SchrodingerCat.TeamType SchrodingerCatChangeTo => SchrodingerCat.TeamType.Jackal;
    public override void OnPlayerDeath(PlayerControl player, CustomDeathReason deathReason, bool isOnMeeting)
    {
        var target = player;
        
            if (target.Is(CustomRoles.Jackal) && Player.Is(CustomRoles.Sidekick))
            {
                Player.RpcSetCustomRole(CustomRoles.Jackal);
                Player.ResetKillCooldown();
                Player.SetKillCooldown();
                Player.Notify("BeJackal");

            }
       
    }
    public override string GetMark(PlayerControl seer, PlayerControl seen, bool _ = false)
    {
        //seenが省略の場合seer
        seen ??= seer;
        if (seen.Is(CustomRoles.Whoops)) return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), "🔻");
        else if (seen.Is(CustomRoles.Jackal)) return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), "M");
        //else if (seen.Is(CustomRoles.Wolfmate)) return Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), "🔻");
        else
            return "";
    }
}
