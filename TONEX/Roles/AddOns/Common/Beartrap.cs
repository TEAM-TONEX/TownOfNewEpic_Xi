using TONEX.Roles.Core;

namespace TONEX.Roles.AddOns.Common;
public sealed class Beartrap : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Beartrap),
    player => new Beartrap(player),
    CustomRoles.Beartrap,
    81800,
    SetupOptionItem,
    "tra|ÏÝÚåŽŸ|ÏÝÚå|Ð¡½±",
    "#5a8fd0"
    );
    public Beartrap(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }


    public static OptionItem OptionBlockMoveTime;

    enum OptionName
    {
        BeartrapBlockMoveTime
    }

    static void SetupOptionItem()
    {
        OptionBlockMoveTime = FloatOptionItem.Create(RoleInfo, 20, OptionName.BeartrapBlockMoveTime, new(1f, 180f, 1f), 5f, false)
         .SetValueFormat(OptionFormat.Seconds);
    }
    public override bool OnCheckMurderAsTargetAfter(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        if (info.IsSuicide) return true;

        var tmpSpeed = Main.AllPlayerSpeed[killer.PlayerId];
        Main.AllPlayerSpeed[killer.PlayerId] = Main.MinSpeed;    //tmpSpeed¤Çáá¤Û¤É‚Ž¤ò‘ø¤¹¤Î¤Ç´úÈë¤·¤Æ¤¤¤Þ¤¹¡£
        ReportDeadBodyPatch.CanReport[killer.PlayerId] = false;
        killer.MarkDirtySettings();
        _ = new LateTask(() =>
        {
            Main.AllPlayerSpeed[killer.PlayerId] = tmpSpeed;
            ReportDeadBodyPatch.CanReport[killer.PlayerId] = true;
            killer.MarkDirtySettings();
            RPC.PlaySoundRPC(killer.PlayerId, Sounds.TaskComplete);
        }, OptionBlockMoveTime.GetFloat(), "Beartrap BlockMove");
        return true;
    }
}
