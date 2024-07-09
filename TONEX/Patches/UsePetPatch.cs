using HarmonyLib;
using Hazel;
using TONEX.Roles.Core;
using static TONEX.Translator;

namespace TONEX;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
class TryPetPatch
{
    public static void Prefix(PlayerControl __instance)
    {
        if (!/* Main.AssistivePluginMode.Value */ false)
        {
            if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.AmClient && !GameStates.IsLobby && (Options.IsStandard || Options.CurrentGameMode == CustomGameMode.AllCrewModMode))
            {
                __instance.petting = true;
                ExternalRpcPetPatch.Prefix(__instance.MyPhysics, 51, new MessageReader());
            }
        }
    }

    public static void Postfix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost || GameStates.IsLobby || !Options.IsStandard || !Options.UsePets.GetBool() ||/* Main.AssistivePluginMode.Value */ false) return;
        var cancel = Options.IsStandard;

            __instance.petting = false;
            if (__instance.AmOwner)
                __instance.MyPhysics.RpcCancelPet();
        
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
class ExternalRpcPetPatch
{
    public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (!AmongUsClient.Instance.AmHost || !GameStates.IsLobby || !Options.IsStandard || !Options.UsePets.GetBool() || !/* Main.AssistivePluginMode.Value */ false) return true;
        {
            var rpcType = callId == 51 ? RpcCalls.Pet : (RpcCalls)callId;
            if (rpcType != RpcCalls.Pet) return false;

            PlayerControl pc = __instance.myPlayer;

            if (callId == 51)
                __instance.CancelPet();
            else
            {
                __instance.CancelPet();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    AmongUsClient.Instance.FinishRpcImmediately(AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 50, SendOption.None, player.GetClientId()));
            }
            if (pc.IsDisabledAction(ExtendedPlayerControl.PlayerActionType.Pet, ExtendedPlayerControl.PlayerActionInUse.Skill) 
                || pc.IsDisabledAction(ExtendedPlayerControl.PlayerActionType.Pet)) return false;

            var roleClass = pc.GetRoleClass();

            if (!roleClass.EnablePetSkill()) return false;
            var petCooldown = roleClass.UsePetCooldown_Timer;
            var totalCooldown = roleClass.UsePetCooldown;
            
            if (petCooldown != -1)
            {
                var cooldown = petCooldown + totalCooldown - Utils.GetTimeStamp();
                pc.Notify(string.Format(GetString("ShowUsePetCooldown"), cooldown, 1f));
                Logger.Info($"使用宠物冷却时间：{cooldown}", "Pet");
                return false;
            }
            roleClass?.OnUsePet();
            if (!roleClass.OnEnterVentWithUsePet())
            {
                Logger.Info($"使用宠物被阻塞", "Pet");
            }
            else
            {
                roleClass.UsePetCooldown_Timer = Utils.GetTimeStamp();
            }
            roleClass.OnShapeshiftWithUsePet();
        }
        return true;
    }
    
}

