using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;

namespace TONEX.Roles.AddOns.Common;
public sealed class TicketsStealer : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(TicketsStealer),
    player => new TicketsStealer(player),
    CustomRoles.TicketsStealer,
    81900,
    SetupCustomOption,
    "ts|¸`Æ±Õß|ÍµÆ±|ÍµÆ±Õß|ÇÔÆ±Ê¦|ÇÔÆ±",
    "#ff1919",
    2
    );
    public TicketsStealer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    public static OptionItem OptionTicketsPerKill;
    enum OptionName
    {
        TicketsPerKill
    }
    public static void SetupCustomOption()
    {
        OptionTicketsPerKill = FloatOptionItem.Create(RoleInfo, 20, OptionName.TicketsPerKill, new(0.1f, 10f, 0.1f), 0.5f, false)
            .SetValueFormat(OptionFormat.Votes);
    }
    public static void ModifyVote(ref byte voterId, ref byte voteFor, ref bool isIntentional, ref int numVotes, ref bool doVote)
    {
        if (Utils.GetPlayerById(voterId).Is(CustomRoles.TicketsStealer))
        {
            numVotes += (int)((PlayerState.GetByPlayerId(voterId)?.GetKillCount(true) ?? 0) * OptionTicketsPerKill.GetFloat());
            Logger.Info($"TicketsStealer Additional Votes: {numVotes}", "TicketsStealer.OnVote");
        }
    }
    public override string GetProgressText(bool comms = false)
    {
        var votes = (int)((PlayerState.GetByPlayerId(Player.PlayerId)?.GetKillCount(true) ?? 0) * OptionTicketsPerKill.GetFloat());
        return votes > 0 ? Utils.ColorString(RoleInfo.RoleColor.ShadeColor(0.5f), $"+{votes}") : ""; 
    }
}