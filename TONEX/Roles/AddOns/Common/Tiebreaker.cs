using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;
using System.Drawing;

namespace TONEX.Roles.AddOns.Common;
public sealed class Tiebreaker : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Tiebreaker),
    player => new Tiebreaker(player),
    CustomRoles.Tiebreaker,
   81000,
    null,
    "br|ÆÆÆ½",
    "#1447af"
    );
    public Tiebreaker(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    private static Dictionary<byte, byte> TiebreakerVotes = new();
    public static void OnVote(byte voter, byte target)
    {
        if (Utils.GetPlayerById(voter).Is(CustomRoles.Tiebreaker))
        {
            TiebreakerVotes.TryAdd(voter, target);
            TiebreakerVotes[voter] = target;
        }
    }
    public static bool ChooseExileTarget(byte[] mostVotedPlayers, out byte target)
    {
        target = byte.MaxValue;
        if (mostVotedPlayers.Count(TiebreakerVotes.ContainsValue) == 1)
        {
            target = mostVotedPlayers.Where(TiebreakerVotes.ContainsValue).FirstOrDefault();
            Logger.Info($"Tiebreaker Override Tie => {Utils.GetPlayerById(target)?.GetNameWithRole()}", "Tiebreaker");
            return true;
        }
        return false;
    }
    public override void OnStartMeeting()
    {
        TiebreakerVotes = new();
    }
}
