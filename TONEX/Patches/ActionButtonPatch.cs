using HarmonyLib;
using TONEX.Roles.Core;
using static TONEX.ExtendedPlayerControl;

namespace TONEX.Patches;

[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
public static class SabotageButtonDoClickPatch
{
    public static bool Prefix()
    {
        if (Main.AssistivePluginMode.Value) return true;
        
            if (!PlayerControl.LocalPlayer.inVent && GameManager.Instance.SabotagesEnabled())
            {
                DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
                {
                    Mode = MapOptions.Modes.Sabotage
                });
            }
        
        return false;
    }
}
[HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
class VentButtonDoClickPatch
{
    public static bool Prefix(VentButton __instance)
    {
        if (Main.AssistivePluginMode.Value) return true;

        var pc = PlayerControl.LocalPlayer;
            if (pc.inVent && (pc.HasDisabledAction(PlayerActionType.ExitVent)))
            {
                pc?.MyPhysics?.RpcExitVent(__instance.currentTarget.Id);
                return false;
            }
            if (pc == null || pc.inVent || __instance.currentTarget == null || !pc.CanMove || !__instance.isActiveAndEnabled) return true;
            if (pc.GetCustomRole() is CustomRoles.EvilInvisibler or CustomRoles.Arsonist or CustomRoles.Veteran or CustomRoles.NiceTimePauser
                or CustomRoles.TimeMaster or CustomRoles.Instigator or CustomRoles.Paranoia or CustomRoles.Mayor or CustomRoles.DoveOfPeace
                or CustomRoles.NiceGrenadier or CustomRoles.Akujo || pc.HasDisabledAction(PlayerActionType.EnterVent))
            {
                pc?.MyPhysics?.RpcEnterVent(__instance.currentTarget.Id);
                return false;
            }
        
        return true;
    }
}
[HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
class KillButtonDoClickPatch
{
    public static bool Prefix(KillButton __instance)
    {
        if (Main.AssistivePluginMode.Value) return true;

        var pc = PlayerControl.LocalPlayer;
            if (pc == null || pc.inVent || __instance.currentTarget == null || !__instance.isActiveAndEnabled) return true;
            if (pc.HasDisabledAction(PlayerActionType.Kill))
            {
            pc.CheckMurder(__instance.currentTarget);
                return false;
            }
        
        return true;
    }
}