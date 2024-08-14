using System.Collections.Generic;
using System.Linq;
using System.Text;
using TONEX.Roles.Core;
using static TONEX.Translator;

namespace TONEX.Roles.AddOns.Common;
public sealed class Mimic : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Mimic),
    player => new Mimic(player),
    CustomRoles.Mimic,
    82000,
    null,
    "mi|åöœ‰π÷|±¶œ‰",
    "#ff1919",
    2
    );
    public Mimic(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    public override void NotifyOnMeetingStart(ref List<(string, byte, string)> msgToSend)
    {
        var mimicSb = new StringBuilder();
        foreach (var vic in Main.AllPlayerControls.Where(p => !p.IsAlive()))
        {
            if (vic.GetRealKiller()== Player && !Player.IsAlive())
                mimicSb.Append($"\n{vic.GetNameWithRole(true)}");
        }
        if (mimicSb.Length > 1)
        {
            string mimicMsg = GetString("MimicDeadMsg") + "\n" + mimicSb.ToString();
            foreach (var ipc in Main.AllPlayerControls.Where(x => x.Is(CustomRoleTypes.Impostor)))
                msgToSend.Add((mimicMsg, ipc.PlayerId, Utils.ColorString(Utils.GetRoleColor(CustomRoles.Mimic), GetString("MimicMsgTitle"))));
        }
    }
}