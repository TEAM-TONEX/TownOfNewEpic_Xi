using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using UnityEngine;
using TONEX.Roles.Core;
using static TONEX.Translator;
using static TONEX.Utils;
using System.Text;
using InnerNet;
using static UnityEngine.GraphicsBuffer;


namespace TONEX.Roles.AddOns.Common;
public sealed class AkujoFakeLovers : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(AkujoFakeLovers),
    player => new AkujoFakeLovers(player),
    CustomRoles.AkujoFakeLovers,
     75_1_2_2000,
    null,
    "akflo|魅魔假情人|魅魔假愛人|魅魔假链子",
    "#9C709F",
    ctop: false
    );
    public AkujoFakeLovers(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    public static void GetSubRolesText(bool intro, bool disableColor, List<CustomRoles> SubRoles, ref StringBuilder sb)
    {
        if (intro && SubRoles.Contains(CustomRoles.AkujoFakeLovers))
        {
            var RoleText = disableColor ? GetRoleName(CustomRoles.AkujoLovers) : ColorString(GetRoleColor(CustomRoles.AkujoLovers), GetRoleName(CustomRoles.AkujoLovers));
            sb.Append($"{ColorString(Color.white, " + ")}{RoleText}");
        }
    }
    public static void TargetMarks(PlayerControl seer, PlayerControl target, ref StringBuilder targetMark)
    {
        if (seer.IsAlive())
        {
            if (seer.Is(CustomRoles.AkujoFakeLovers) && (target.Is(CustomRoles.Akujo)))
            {
                targetMark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoLovers)}>❤</color>");
            }
        }
        if (!seer.IsAlive() || !CustomRoles.Akujo.IsExist())
        {
            if (seer.Is(CustomRoles.AkujoFakeLovers) && (target.Is(CustomRoles.Akujo)))
            {
                targetMark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoFakeLovers)}>_♡_</color>");
            }
        }
        if (seer.Is(CustomRoles.Akujo) && target.Is(CustomRoles.AkujoFakeLovers))
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoFakeLovers)}>_♡_</color>");
        }
    }
    public static bool CanKnowOthers(PlayerControl seer, PlayerControl seen)
    {
        if (seer.Is(CustomRoles.AkujoFakeLovers) && seen.Is(CustomRoles.Akujo) || seer.Is(CustomRoles.Akujo) && seen.Is(CustomRoles.AkujoFakeLovers) && AkujoLovers.AkujoLoverKnowRoles.GetBool())
            return true;
        return false;
    }
    public static void MeetingngStartNotifyOthers(ref StringBuilder sb, CustomRoles role)
    {
        if (role == (CustomRoles.AkujoFakeLovers))
            sb.Append($"\n\n" + GetString($"AkujoLovers") + Utils.GetRoleDisplaySpawnMode(CustomRoles.AkujoLovers) + GetString($"AkujoLoversInfoLong"));

    }
    public static void MeetingHud(bool isLover, PlayerControl seer, PlayerControl target, ref StringBuilder sb)
    {
        if (seer.IsAlive())
        {
            if (seer.Is(CustomRoles.AkujoFakeLovers) && (target.Is(CustomRoles.Akujo)) && !seer.Data.IsDead)
                sb.Append(Utils.ColorString(Utils.GetRoleColor(CustomRoles.AkujoLovers), "❤"));
        }
        if (!seer.IsAlive() || !CustomRoles.Akujo.IsExist())
        {
            if (seer.Is(CustomRoles.AkujoFakeLovers) && (target.Is(CustomRoles.Akujo)))
                sb.Append(Utils.ColorString(Utils.GetRoleColor(CustomRoles.AkujoFakeLovers), "_♡_"));
        }
        if (seer.Is(CustomRoles.Akujo) && target.Is(CustomRoles.AkujoFakeLovers))
        {
            sb.Append($"<color={GetRoleColorCode(CustomRoles.AkujoFakeLovers)}>_♡_</color>");
        }

    }
    public static void Marks(PlayerControl __instance, ref StringBuilder Mark)
    {
        if (PlayerControl.LocalPlayer.IsAlive())
        {
            if (PlayerControl.LocalPlayer.Is(CustomRoles.AkujoFakeLovers) && (__instance.Is(CustomRoles.Akujo)))
            {
                Mark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoLovers)}>❤</color>");
            }
        }
        if (!PlayerControl.LocalPlayer.IsAlive() || !CustomRoles.Akujo.IsExist())
        {
            if (PlayerControl.LocalPlayer.Is(CustomRoles.AkujoFakeLovers) && (__instance.Is(CustomRoles.Akujo)))
            {
                Mark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoFakeLovers)}>_♡_</color>");
            }
        }
        if (PlayerControl.LocalPlayer.Is(CustomRoles.Akujo) && __instance.Is(CustomRoles.AkujoFakeLovers))
        {
            Mark.Append($"<color={GetRoleColorCode(CustomRoles.AkujoFakeLovers)}>_♡_</color>");
        }
    }


}