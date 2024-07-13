using HarmonyLib;
using Hazel;
using TONEX.Attributes;
using TONEX.Modules;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Patches.ISystemType;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
public static class SabotageSystemTypeUpdateSystemPatch
{
    private static bool isCooldownModificationEnabled;
    private static float modifiedCooldownSec;
    private static readonly LogHandler logger = Logger.Handler(nameof(SabotageSystemType));

    [GameModuleInitializer]
    public static void Initialize()
    {
        isCooldownModificationEnabled = Options.ModifySabotageCooldown.GetBool();
        modifiedCooldownSec = Options.SabotageCooldown.GetFloat();
    }

    public static bool Prefix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
    {
        if (Main.AssistivePluginMode.Value) return true;
        byte amount;
        {
            var newReader = MessageReader.Get(msgReader);
            amount = newReader.ReadByte();
            newReader.Recycle();
        }

        var nextSabotage = (SystemTypes)amount;

        if (player.IsDisabledAction(ExtendedPlayerControl.PlayerActionType.Sabotage, ExtendedPlayerControl.PlayerActionInUse.All)) return false;
        logger.Info($"PlayerName: {player.GetNameWithRole()}, SabotageType: {nextSabotage}");

        if (!player.IsDisabledAction(ExtendedPlayerControl.PlayerActionType.Sabotage, ExtendedPlayerControl.PlayerActionInUse.Skill)) 
        if (!CustomRoleManager.OnSabotage(player, nextSabotage))
        {
            return false;
        }
        var roleClass = player.GetRoleClass();
        if (roleClass is IKiller killer)
        {
            //そもそもサボタージュボタン使用不可ならサボタージュ不可
            if (!killer.CanUseSabotageButton()) return false;
            //その他処理が必要であれば処理
            return roleClass.OnInvokeSabotage(nextSabotage);
        }
        else
        {
            return CanSabotage(player);
        }
    }
    private static bool CanSabotage(PlayerControl player)
    {
        //サボタージュ出来ないキラー役職はサボタージュ自体をキャンセル
        if (!player.Is(CustomRoleTypes.Impostor))
        {
            return false;
        }
        return true;
    }
    public static void Postfix(SabotageSystemType __instance, bool __runOriginal /* Prefixの結果，本体処理が実行されたかどうか */ )
    {
        if (Main.AssistivePluginMode.Value) return;
        if (!__runOriginal || !isCooldownModificationEnabled || !AmongUsClient.Instance.AmHost)
        {
            return;
        }
        // サボタージュクールダウンを変更
        __instance.Timer = modifiedCooldownSec;
        __instance.IsDirty = true;
    }
}

[HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.Initialize))]
public static class ElectricTaskInitializePatch
{
    public static void Postfix()
    {
        if (Main.AssistivePluginMode.Value) return;
        Utils.MarkEveryoneDirtySettings();
        if (!GameStates.IsMeeting)
            Utils.NotifyRoles(ForceLoop: true);
    }
}
[HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.Complete))]
public static class ElectricTaskCompletePatch
{
    public static void Postfix()
    {
        if (Main.AssistivePluginMode.Value) return;
        Utils.MarkEveryoneDirtySettings();
        if (!GameStates.IsMeeting)
            Utils.NotifyRoles(ForceLoop: true);
    }
}