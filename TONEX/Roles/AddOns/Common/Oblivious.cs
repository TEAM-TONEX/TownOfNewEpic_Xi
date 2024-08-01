using TONEX.Roles.Core;
public sealed class Oblivious : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Oblivious),
    player => new Oblivious(player),
    CustomRoles.Oblivious,
     81100,
    null,
    "pb|Ä‘Ð¡¹í|µ¨Ð¡",
    "#424242"
    );
    public Oblivious(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }




}