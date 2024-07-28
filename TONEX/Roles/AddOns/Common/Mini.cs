using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Options;
using System.Text;
using static TONEX.Utils;
using static UnityEngine.GraphicsBuffer;

namespace TONEX.Roles.AddOns.Common;
public sealed class Mini : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Mini),
    player => new Mini(player),
    CustomRoles.Mini,
    75_1_2_1800,
    SetupOptionItem,
    "mini|迷你",
    "#ffebd7"
    );
    public Mini(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    public static OptionItem OptionAgeTime;
    public static OptionItem OptionNotGrowInMeeting;
    public static OptionItem OptionKidKillCoolDown;
    public static OptionItem OptionAdultKillCoolDown;
    public int Age;
    public int UpTime;

    enum OptionName
    {
        MiniUpTime,
        NotGrowInMeeting,

        OptionKidKillCoolDown,
        OptionAdultKillCoolDown
    }

    static void SetupOptionItem()
    {
        OptionAgeTime = FloatOptionItem.Create(RoleInfo, 20, OptionName.MiniUpTime, new(60f, 360f, 2.5f), 180f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionNotGrowInMeeting = BooleanOptionItem.Create(RoleInfo, 21, OptionName.NotGrowInMeeting, false, false);
        OptionKidKillCoolDown = FloatOptionItem.Create(RoleInfo, 22, OptionName.OptionKidKillCoolDown, new(2.5f, 180f, 2.5f), 45f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionAdultKillCoolDown = FloatOptionItem.Create(RoleInfo, 23, OptionName.OptionAdultKillCoolDown, new(2.5f, 180f, 2.5f), 15f,  false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {
        UpTime = -8;
        Age = 0;
    }
    public  void SendRPC()
    {
        var sender = CreateSender(CustomRPC.MiniAge);
        sender.Writer.Write(Age);
    }
    public override void ReceiveRPC(MessageReader reader, CustomRPC rpcTypes)
    {
        if (rpcTypes != CustomRPC.MiniAge) return;
        Age = reader.ReadInt32();

    }
    public override void OnSecondsUpdate(PlayerControl player,long now)
    {
        if (Age >= 18) return;
        if (!player.IsAlive() && player.IsCrew())
        {

            CustomSoundsManager.RPCPlayCustomSoundAll("Congrats");
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Mini);
                CustomWinnerHolder.WinnerIds.Add(Player.PlayerId);
        }
        if (!GameStates.IsInTask && OptionNotGrowInMeeting.GetBool()) return;
        if (player.IsAlive())
        {
            UpTime++;
            if (UpTime >= OptionAgeTime.GetInt() / 18)
            {
                Age ++;
                UpTime = 0;
                SendRPC();
                Utils.NotifyRoles(Player);
            }
        }

    }
    public override bool OnCheckMurderAsTargetAfter(MurderInfo info)
    {

        var (killer, target) = info.AttemptTuple;

        if (Age < 18)
        {
            killer.Notify(Translator.GetString("CantKillKid"));
            return false;
        }

        return true;
    }
    public override string GetMark(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        return Age < 18 ? Utils.ColorString(Color.yellow, $"({Age})") : "";
    }
}
