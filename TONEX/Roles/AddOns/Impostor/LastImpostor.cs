using TONEX.Attributes;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Options;
namespace TONEX.Roles.AddOns.Common;
public sealed class LastImpostor : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(LastImpostor),
    player => new LastImpostor(player),
    CustomRoles.LastImpostor,
   80000,
    null,
    "li",
    "#ff1919",
    2
    );
    public LastImpostor(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    public static byte currentId = byte.MaxValue;
    public static OptionItem KillCooldown;
    public static void SetupCustomOption()
    {
        KillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(0f, 180f, 1f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {
        currentId = byte.MaxValue;
    }
    public static void SetKillCooldown()
    {
        if (currentId == byte.MaxValue) return;
        if (!Main.AllPlayerKillCooldown.TryGetValue(currentId, out var x) || KillCooldown.GetFloat() >= x) return;
        Main.AllPlayerKillCooldown[currentId] = KillCooldown.GetFloat();
    }
    public static bool CanBeLastImpostor(PlayerControl pc)
    {
        if (!pc.IsAlive() || pc.Is(CustomRoles.LastImpostor) || !pc.Is(CustomRoleTypes.Impostor))
        {
            return false;
        }
        if (pc.GetRoleClass() is IImpostor impostor)
        {
            return impostor.CanBeLastImpostor;
        }
        return true;
    }
    public static void SetSubRole()
    {
        //ラストインポスターがすでにいれば処理不要
        if (currentId != byte.MaxValue) return;
        if (!IsStandard
        || !CustomRoles.LastImpostor.IsEnable() || Main.AliveImpostorCount != 1)
            return;
        foreach (var pc in Main.AllAlivePlayerControls)
        {
            if (CanBeLastImpostor(pc))
            {
                pc.RpcSetCustomRole(CustomRoles.LastImpostor);
                SetKillCooldown();
                pc.SyncSettings();
                Utils.NotifyRoles();
                break;
            }
        }
    }
}