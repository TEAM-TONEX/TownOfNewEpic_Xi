using HarmonyLib;
using UnityEngine;

namespace TONEX;

[HarmonyPatch]
public class MapRealTimeLocationPatch
{
    private static bool ShouldShowRealTime => !PlayerControl.LocalPlayer.IsAlive() || PlayerControl.LocalPlayer.Is(Roles.Core.CustomRoles.GM) || Main.GodMode.Value;
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap)), HarmonyPrefix]
    public static bool ShowNormalMap(MapBehaviour __instance)
    {
        var color = PlayerControl.LocalPlayer.GetRoleColor() == Color.white ? PlayerControl.LocalPlayer.GetRoleColor() : Palette.DisabledGrey;
        if (Main.EnableMapBackGround.Value)
            __instance.ColorControl.SetColor(color);
        if (!ShouldShowRealTime) return true;
       
        __instance.ShowCountOverlay(true, true, true);
       
        return false;
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap)), HarmonyPostfix]
    public static void ShowNormalMapAfter(MapBehaviour __instance)
    {
        var color = PlayerControl.LocalPlayer.GetRoleColor() == Color.white ? PlayerControl.LocalPlayer.GetRoleColor() : Palette.DisabledGrey;
        if (Main.EnableMapBackGround.Value)
            __instance.ColorControl.SetColor(color);
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap)), HarmonyPrefix]
    public static bool ShowSabotageMap(MapBehaviour __instance)
    {
        var color = PlayerControl.LocalPlayer.GetRoleColor() == Color.white ? PlayerControl.LocalPlayer.GetRoleColor() : Palette.DisabledGrey;
        if (Main.EnableMapBackGround.Value)
            __instance.ColorControl.SetColor(color);
        if (!ShouldShowRealTime || PlayerControl.LocalPlayer.Is(Roles.Core.CustomRoleTypes.Impostor)) return true;
        
        __instance.ShowCountOverlay(true, true, true);
        return false;
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap)), HarmonyPostfix]
    public static void ShowSabotageMapAfter(MapBehaviour __instance)
    {
        var color = PlayerControl.LocalPlayer.GetRoleColor() == Color.white ? PlayerControl.LocalPlayer.GetRoleColor() : Palette.DisabledGrey;
        if (Main.EnableMapBackGround.Value)
            __instance.ColorControl.SetColor(color);
    }
}