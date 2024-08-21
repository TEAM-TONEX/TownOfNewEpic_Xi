using System.Linq;
using AmongUs.GameOptions;
using TONEX.Roles.Core;

namespace TONEX.Roles.Crewmate;
public sealed class Awake : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Awake),
            player => new Awake(player),
            CustomRoles.Awake,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            94_1_4_0600,
            SetupOptionItem,
            "aw|觉醒者|睡醒者|睡醒了",
            "#33FFCC"
        );
    public Awake(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
    }

    static OptionItem OptionAwake;
    enum OptionName
    {
        CanGetAllTargetSubRole
    }
    private static void SetupOptionItem()
    {
        OptionAwake = BooleanOptionItem.Create(RoleInfo, 11, OptionName.CanGetAllTargetSubRole, true, false);
    }
    public override bool OnCompleteTask(out bool cancel)
    {
      if (Player.IsAlive()){
            var pcList = Main.AllAlivePlayerControls.Where(x => x.PlayerId != Player.PlayerId && x.GetCustomRole().IsCrewmate()).ToList();
            var target = pcList[IRandom.Instance.Next(0, pcList.Count)];
            var pc = target.GetCustomSubRoles();
            if (OptionAwake.GetBool())
            {
                if (pc != null && pc.Any())
                {
                    foreach (var role in pc)
                    {
                        if (!(role is CustomRoles.Lovers or CustomRoles.Neptune or CustomRoles.AdmirerLovers or CustomRoles.AkujoLovers or CustomRoles.CupidLovers or CustomRoles.AkujoFakeLovers or CustomRoles.Workhorse or CustomRoles.Reach))
                            Player.RpcSetCustomRole(role);
                    }
                }
            }
            else {
                if (pc != null && pc.Any()){
                    var role = pc.First();
                    if (!(role is CustomRoles.Lovers or CustomRoles.Neptune or CustomRoles.AdmirerLovers or CustomRoles.AkujoLovers or CustomRoles.CupidLovers or CustomRoles.AkujoFakeLovers or CustomRoles.Workhorse or CustomRoles.Reach))
                        Player.RpcSetCustomRole(role);
                }
            }
        }
        cancel = false;
        return false;
    }

}