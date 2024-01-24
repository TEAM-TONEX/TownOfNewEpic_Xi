using HarmonyLib;
using InnerNet;
using System.Linq;
using TONEX.Modules;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.MakePublic))]
internal class MakePublicPatch
{
    public static bool Prefix(GameStartManager __instance)
    {
        // 定数設定による公開ルームブロック
#if RELEASE
        if (!Main.AllowPublicRoom)
        {
            var message = GetString("DisabledByProgram");
            Logger.Info(message, "MakePublicPatch");
            Logger.SendInGame(message);
            return false;
        }
        if (ModUpdater.isBroken || (ModUpdater.hasUpdate && ModUpdater.forceUpdate) || !VersionChecker.IsSupported || !Main.IsPublicAvailableOnThisVersion)
        {
            var message = "";
            message = GetString("PublicNotAvailableOnThisVersion");
            if (ModUpdater.isBroken) message = GetString("ModBrokenMessage");
            if (ModUpdater.hasUpdate) message = GetString("CanNotJoinPublicRoomNoLatest");
            Logger.Info(message, "MakePublicPatch");
            Logger.SendInGame(message);
            return false;
        }
#endif
        return true;
    }
}
[HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
class MMOnlineManagerStartPatch
{
    public static void Postfix(MMOnlineManager __instance)
    {
#if RELEASE
        if (!(ModUpdater.hasUpdate || ModUpdater.isBroken || !VersionChecker.IsSupported || !Main.IsPublicAvailableOnThisVersion)) return;
        var obj = GameObject.Find("FindGameButton");
        if (obj)
        {
            obj?.SetActive(false);
            var parentObj = obj.transform.parent.gameObject;
            var textObj = Object.Instantiate(obj.transform.FindChild("Text_TMP").GetComponent<TMPro.TextMeshPro>());
            textObj.transform.position = new Vector3(0.5f, -0.4f, 0f);
            textObj.name = "CanNotJoinPublic";
            textObj.DestroyTranslator();
            string message = "";
            if (ModUpdater.hasUpdate)
            {
                message = GetString("CanNotJoinPublicRoomNoLatest");
            }
            else if (ModUpdater.isBroken)
            {
                message = GetString("ModBrokenMessage");
            }
            else if (!VersionChecker.IsSupported)
            {
                message = GetString("UnsupportedVersion");
            }
            else if (!Main.IsPublicAvailableOnThisVersion)
            {
                message = GetString("PublicNotAvailableOnThisVersion");
            }
            textObj.text = $"<size=2>{Utils.ColorString(Color.red, message)}</size>";
        }
#endif
    }

}
[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
internal class SplashLogoAnimatorPatch
{
    public static void Prefix(SplashManager __instance)
    {
        if (DebugModeManager.AmDebugger)
        {
            __instance.sceneChanger.AllowFinishLoadingScene();
            __instance.startedSceneLoad = true;
        }
    }
}
[HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
internal class RunLoginPatch
{
    public static void Prefix(ref bool canOnline)
    {
        // Ref: https://github.com/0xDrMoe/TownofHost-Enhanced/blob/main/Patches/ClientPatch.cs
        var friendCode = EOSManager.Instance?.friendCode;
        canOnline = !string.IsNullOrEmpty(friendCode) && !BanManager.CheckEACStatus(friendCode, null);

#if DEBUG
        // 如果您希望在调试版本公开您的房间，请仅用于测试用途
        // 如果您修改了代码，请在房间公告内表明这是修改版本，并给出修改作者
        // If you wish to make your lobby public in a debug build, please use it only for testing purposes
        // If you modify the code, please indicate in the lobby announcement that this is a modified version and provide the author of the modification
        canOnline = true;
#endif
    }
}
[HarmonyPatch(typeof(BanMenu), nameof(BanMenu.SetVisible))]
internal class BanMenuSetVisiblePatch
{
    public static bool Prefix(BanMenu __instance, bool show)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        show &= PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null;
        __instance.BanButton.gameObject.SetActive(AmongUsClient.Instance.CanBan());
        __instance.KickButton.gameObject.SetActive(AmongUsClient.Instance.CanKick());
        __instance.MenuButton.gameObject.SetActive(show);
        return false;
    }
}
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.CanBan))]
internal class InnerNetClientCanBanPatch
{
    public static bool Prefix(InnerNetClient __instance, ref bool __result)
    {
        __result = __instance.AmHost;
        return false;
    }
}
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.KickPlayer))]
internal class KickPlayerPatch
{
    public static bool Prefix(InnerNetClient __instance, int clientId, bool ban)
    {
        if (Main.AllPlayerControls.Where(p => p.IsDev()).Any(p => AmongUsClient.Instance.GetRecentClient(clientId).FriendCode == p.FriendCode))
        {
            Logger.SendInGame(GetString("Warning.CantKickDev"));
            return false;
        }
        if (!AmongUsClient.Instance.AmHost) return true;

        if (!OnPlayerLeftPatch.ClientsProcessed.Contains(clientId))
        {
            OnPlayerLeftPatch.Add(clientId);
            if (ban)
            {
                BanManager.AddBanPlayer(AmongUsClient.Instance.GetRecentClient(clientId));
                RPC.NotificationPop(string.Format(GetString("PlayerBanByHost"), AmongUsClient.Instance.GetRecentClient(clientId).PlayerName));
            }
            else
            {
                RPC.NotificationPop(string.Format(GetString("PlayerKickByHost"), AmongUsClient.Instance.GetRecentClient(clientId).PlayerName));
            }
        }
        return true;
    }
}
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendAllStreamedObjects))]
internal class InnerNetObjectSerializePatch
{
    public static void Prefix()
    {
        if (AmongUsClient.Instance.AmHost)
            GameOptionsSender.SendAllGameOptions();
    }
}