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
using static TONEX.GuesserHelper;

namespace TONEX.Roles.AddOns.Common;
public static class Guesser
{
    private static readonly int Id = 75_1_3_0200;
    private static List<byte> playerIdList = new();
    public static Dictionary<byte, int> GuessLimit = new();
    public static OptionItem OptionGuessNums;
    public static OptionItem OptionCanGuessImp;
    public static OptionItem OptionCanGuessAddons;
    public static OptionItem OptionCanGuessVanilla;
    public static OptionItem OptionCanGuessTaskDoneSnitch;
    public static OptionItem OptionJustShowExist;
    public static OptionItem OptionIgnoreMedicShield;
    public static void SetupCustomOption()
    {
        SetupAddonOptions(Id, TabGroup.Addons, CustomRoles.Guesser);
        AddOnsAssignData.Create(Id + 10, CustomRoles.Guesser, true, true, true);
        OptionGuessNums = IntegerOptionItem.Create(Id + 21, "GuesserCanGuessTimes", new(1, 15, 1), 15, TabGroup.Addons, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Guesser])
            .SetValueFormat(OptionFormat.Times);
        OptionCanGuessAddons = BooleanOptionItem.Create(Id + 23, "EGCanGuessAdt", false, TabGroup.Addons, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Guesser]);
        OptionCanGuessVanilla = BooleanOptionItem.Create(Id + 24, "EGCanGuessVanilla", true, TabGroup.Addons, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Guesser]);
        OptionCanGuessTaskDoneSnitch = BooleanOptionItem.Create(Id + 25, "EGCanGuessTaskDoneSnitch", true, TabGroup.Addons, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Guesser]);
        OptionJustShowExist = BooleanOptionItem.Create(Id + 26, "JustShowExist", false, TabGroup.Addons, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Guesser]);
        OptionIgnoreMedicShield = BooleanOptionItem.Create(Id + 27, "IgnoreMedicShield", true, TabGroup.Addons, false)
            .SetParent(CustomRoleSpawnChances[CustomRoles.Guesser]);
    }
    [GameModuleInitializer]
    public static void Init()
    {
        playerIdList = new();
        GuessLimit = new();
    }

    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        GuessLimit.TryAdd(playerId, OptionGuessNums.GetInt());
    }
    public static void OverrideNameAsSeer(PlayerControl seen, ref string nameText, bool isForMeeting = false)
    {
        foreach (var id in playerIdList)
        {
            if (Utils.GetPlayerById(id).IsAlive() && seen.IsAlive() && isForMeeting)
            {
                nameText = Utils.ColorString(Utils.GetRoleColor(CustomRoles.EvilGuesser), seen.PlayerId.ToString()) + " " + nameText;
            }
        }
    }
    public static bool IsEnable => playerIdList.Count > 0;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);
    public static bool ShouldShowButton() => PlayerControl.LocalPlayer.IsAlive() && playerIdList.Contains(PlayerControl.LocalPlayer.PlayerId);
    public static bool ShouldShowButtonFor(PlayerControl target) => target.IsAlive() && playerIdList.Contains(PlayerControl.LocalPlayer.PlayerId);
    public static bool OnClickButtonLocal(PlayerControl target)
    {
        ShowGuessPanel(target.PlayerId, MeetingHud.Instance);
        return false;
    }
}
