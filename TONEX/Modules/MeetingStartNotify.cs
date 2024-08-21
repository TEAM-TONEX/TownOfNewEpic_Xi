using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TONEX.Roles.AddOns.Common;
using TONEX.Roles.Core;
using static TONEX.Translator;

namespace TONEX;

public static class MeetingStartNotify
{
    public static void OnMeetingStart()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        List<(string, byte, string)> msgToSend = new();

        void AddMsg(string text, byte sendTo = 255, string title = "")
            => msgToSend.Add((text, sendTo, title));

        //首次会议技能提示
        if (Options.SendRoleDescriptionFirstMeeting.GetBool() && MeetingStates.FirstMeeting)
            foreach (var pc in Main.AllAlivePlayerControls.Where(x => !x.IsModClient()))
            {
                var role = pc.GetCustomRole();
                var text = role.GetRoleInfo()?.Description?.GetFullFormatHelpWithAddonsByPlayer(pc) ??
                    // roleInfoがない役職
                    GetString(role.ToString()) + pc.GetRoleInfo(true);
               
                AddMsg(text, pc.PlayerId);
            }
        if (msgToSend.Count >= 1)
        {
            var msgTemp = msgToSend.ToList();
            new LateTask(() => { msgTemp.Do(x => Utils.SendMessage(x.Item1, x.Item2, x.Item3 ?? "")); }, 3f, "NotifyOnMeetingStart");
        }

        msgToSend = new();
        CustomRoleManager.AllActiveRoles.Values.Do(x => x.NotifyOnMeetingStart(ref msgToSend));

        CustomRoleManager.AllActiveAddons.Values.Do(x => x.Do_Addons(x => x.NotifyOnMeetingStart(ref msgToSend)));

        msgToSend.Do(x => Logger.Info($"To:{x.Item2} {x.Item3 ?? ""} => {x.Item1}", "NotifyOnMeetingStart"));
        new LateTask(() => { msgToSend.DoIf(x => x.Item1 != null, x => Utils.SendMessage(x.Item1, x.Item2, x.Item3 ?? "")); }, 3f, "NotifyOnMeetingStart");
    }
}
