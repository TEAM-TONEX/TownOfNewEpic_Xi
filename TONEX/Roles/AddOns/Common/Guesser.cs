using System.Collections.Generic;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using static TONEX.GuesserHelper;


namespace TONEX.Roles.AddOns.Common;
public sealed class Guesser : AddonBase, IMeetingButton
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Guesser),
    player => new Guesser(player),
    CustomRoles.Guesser,
   75_1_3_0200,
    SetupOptionItem,
    "Gu|附加赌|赌怪",
    "#DF9965"
    );
    public Guesser(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    public static Dictionary<byte, int> GuessLimit = new();
    public static OptionItem OptionGuessNums;
    public static OptionItem OptionCanGuessImp;
    public static OptionItem OptionCanGuessAddons;
    public static OptionItem OptionCanGuessVanilla;
    public static OptionItem OptionCanGuessTaskDoneSnitch;
    public static OptionItem OptionJustShowExist;
    public static OptionItem OptionIgnoreMedicShield;

    public static OptionItem OptionRadius;
    public static byte player;
    enum OptionName
    {
        GuesserCanGuessTimes,
        EGCanGuessAdt,
        EGCanGuessVanilla,
        EGCanGuessTaskDoneSnitch,
        JustShowExist,
        IgnoreMedicShield,
    }

    static void SetupOptionItem()
    {
        OptionGuessNums = IntegerOptionItem.Create(RoleInfo,21, OptionName.GuesserCanGuessTimes, new(1, 15, 1), 15, false)
    .SetValueFormat(OptionFormat.Times);
        OptionCanGuessAddons = BooleanOptionItem.Create(RoleInfo, 23, OptionName.EGCanGuessAdt, false, false);
        OptionCanGuessVanilla = BooleanOptionItem.Create(RoleInfo, 24, OptionName.EGCanGuessVanilla, true,  false);
        OptionCanGuessTaskDoneSnitch = BooleanOptionItem.Create(RoleInfo, 25, OptionName.EGCanGuessTaskDoneSnitch, true, false);
        OptionJustShowExist = BooleanOptionItem.Create(RoleInfo, 26, OptionName.JustShowExist, false,  false);
        OptionIgnoreMedicShield = BooleanOptionItem.Create(RoleInfo, 27, OptionName.IgnoreMedicShield, true, false);
    }
    public override void Add()
    {
        player = Player.PlayerId;
        GuessLimit.TryAdd(Player.PlayerId, OptionGuessNums.GetInt());
    }
    public override void OverrideNameAsSeer(PlayerControl seen, ref string nameText, bool isForMeeting = false)
    => nameText = Utils.ColorString(Utils.GetRoleColor(CustomRoles.EvilGuesser), seen.PlayerId.ToString()) + " " + nameText;

    public bool ShouldShowButton() => PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.PlayerId == player;
    public bool ShouldShowButtonFor(PlayerControl target) => target.IsAlive() && PlayerControl.LocalPlayer.PlayerId == player;
    public bool OnClickButtonLocal(PlayerControl target)
    {
        ShowGuessPanel(target.PlayerId, MeetingHud.Instance);
        return false;
    }
}
