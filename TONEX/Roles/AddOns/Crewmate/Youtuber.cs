using TONEX.Roles.Core;

namespace TONEX.Roles.AddOns.Common;
public sealed class YouTuber : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(YouTuber),
    player => new YouTuber(player),
    CustomRoles.YouTuber,
   80700,
    null,
    "yt|up",
    "#fb749b",
    1
    );
    public YouTuber(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

}
