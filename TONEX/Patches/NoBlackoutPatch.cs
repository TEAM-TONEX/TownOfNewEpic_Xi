using HarmonyLib;

namespace TONEX;

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
class DontBlackoutPatch
{
    public static void Postfix(ref bool __result)
    {
        if (Main.AssistivePluginMode.Value) return;
        __result = false;
    }
}