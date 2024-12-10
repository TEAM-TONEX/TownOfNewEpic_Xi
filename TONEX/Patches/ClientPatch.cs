using HarmonyLib;
using Hazel;
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

        //#if RELEASE

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
//#endif
        return true;
    }
}
[HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
class MMOnlineManagerStartPatch
{
    public static void Postfix(MMOnlineManager __instance)
    {
        //#if RELEASE

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
            else if (!Main.AllowPublicRoom)
            {
                message = GetString("PublicNotAvailableOnThisVersion");
            }
            textObj.text = $"<size=2>{Utils.ColorString(Color.red, message)}</size>";
        }

        //#endif
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
        if (Main.AssistivePluginMode.Value) return true;
        
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

// 来自：TownOfHost https://github.com/tukasa0001/TownOfHost
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendAllStreamedObjects))]
class InnerNetObjectSerializePatch
{
        public static bool Prefix(InnerNetClient __instance, ref bool __result)
    {
        if (AmongUsClient.Instance.AmHost)
            GameOptionsSender.SendAllGameOptions();



        var sended = false;
        __result = false;
        var obj = __instance.allObjects;
        lock (obj)
        {
            for (int i = 0; i < __instance.allObjects.Count; i++)
            {
                InnerNetObject innerNetObject = __instance.allObjects[i];
                if (innerNetObject && innerNetObject.IsDirty && (innerNetObject.AmOwner ||
                    (innerNetObject.OwnerId == -2 && __instance.AmHost)))
                {
                    var messageWriter = __instance.Streams[(byte)innerNetObject.sendMode];
                    if (messageWriter.Length > 500)
                    {
                        if (!sended)
                        {
                            if (DebugModeManager.IsDebugMode)
                            {
                                Logger.Info($"SendAllStreamedObjects: Start", "InnerNetClient");
                            }
                            sended = true;
                        }
                        messageWriter.EndMessage();
                        __instance.SendOrDisconnect(messageWriter);
                        messageWriter.Clear(innerNetObject.sendMode);
                        messageWriter.StartMessage(5);
                        messageWriter.Write(__instance.GameId);
                    }
                    messageWriter.StartMessage(1);
                    messageWriter.WritePacked(innerNetObject.NetId);
                    try
                    {
                        if (innerNetObject.Serialize(messageWriter, false))
                        {
                            messageWriter.EndMessage();
                        }
                        else
                        {
                            messageWriter.CancelMessage();
                        }
                        if (innerNetObject.Chunked && innerNetObject.IsDirty)
                        {
                            Logger.Info($"SendAllStreamedObjects: Chunked", "InnerNetClient");
                            __result = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Logger.Info($"Exception:{ex.Message}", "InnerNetClient");
                        messageWriter.CancelMessage();
                    }
                }
            }
        }
        for (int j = 0; j < __instance.Streams.Length; j++)
        {
            MessageWriter messageWriter2 = __instance.Streams[j];
            if (messageWriter2.HasBytes(7))
            {
                if (!sended)
                {
                    if (DebugModeManager.IsDebugMode)
                    {
                        Logger.Info($"SendAllStreamedObjects: Start", "InnerNetClient");
                    }
                    sended = true;
                }
                messageWriter2.EndMessage();
                __instance.SendOrDisconnect(messageWriter2);
                messageWriter2.Clear((SendOption)j);
                messageWriter2.StartMessage(5);
                messageWriter2.Write(__instance.GameId);
            }
        }
        if (DebugModeManager.IsDebugMode && sended) Logger.Info($"SendAllStreamedObjects: End", "InnerNetClient");
        return false;
    }
}
[HarmonyPatch]
class InnerNetClientPatch
{
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage)), HarmonyPrefix]
    public static bool HandleMessagePatch(InnerNetClient __instance, MessageReader reader, SendOption sendOption)
    {
        if (DebugModeManager.IsDebugMode)
        {
            Logger.Info($"HandleMessagePatch:Packet({reader.Length}) ,SendOption:{sendOption}", "InnerNetClient");
        }
        else if (reader.Length > 1000)
        {
            Logger.Info($"HandleMessagePatch:Large Packet({reader.Length})", "InnerNetClient");
        }
        return true;
    }
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendOrDisconnect)), HarmonyPrefix]
    public static void SendOrDisconnectPatch(InnerNetClient __instance, MessageWriter msg)
    {
        if (DebugModeManager.IsDebugMode)
        {
            Logger.Info($"SendOrDisconnectPatch:Packet({msg.Length}) ,SendOption:{msg.SendOption}", "InnerNetClient");
        }
        else if (msg.Length > 1000)
        {
            Logger.Info($"SendOrDisconnectPatch:Large Packet({msg.Length})", "InnerNetClient");
        }
    }
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendInitialData)), HarmonyPrefix]
    public static bool SendInitialDataPatch(InnerNetClient __instance, int clientId)
    {
        if (DebugModeManager.IsDebugMode)
        {
            Logger.Info($"SendInitialData: Start", "InnerNetClient");
        }
        MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
        messageWriter.StartMessage(6);
        messageWriter.Write(__instance.GameId);
        messageWriter.WritePacked(clientId);

        var obj = __instance.allObjects;
        lock (obj)
        {
            var hashSet = new System.Collections.Generic.HashSet<GameObject>();
            //まずはGameManagerを送信
            GameManager gameManager = GameManager.Instance;
            __instance.SendGameManager(clientId, gameManager);
            hashSet.Add(gameManager.gameObject);

            for (int i = 0; i < __instance.allObjects.Count; i++)
            {
                InnerNetObject innerNetObject = __instance.allObjects[i];
                if (innerNetObject && (innerNetObject.OwnerId != -4 || __instance.AmModdedHost) && hashSet.Add(innerNetObject.gameObject))
                {
                    if (messageWriter.Length > 500)
                    {
                        messageWriter.EndMessage();
                        __instance.SendOrDisconnect(messageWriter);
                        messageWriter.Clear(SendOption.Reliable);
                        messageWriter.StartMessage(6);
                        messageWriter.Write(__instance.GameId);
                        messageWriter.WritePacked(clientId);

                    }
                    __instance.WriteSpawnMessage(innerNetObject, innerNetObject.OwnerId, innerNetObject.SpawnFlags, messageWriter);
                }
            }
        }
        messageWriter.EndMessage();
        __instance.SendOrDisconnect(messageWriter);
        messageWriter.Recycle();
        if (DebugModeManager.IsDebugMode)
        {
            Logger.Info($"SendInitialData: End", "InnerNetClient");
        }
        return false;
    }
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Spawn)), HarmonyPostfix]
    public static void SpawnPatch(InnerNetClient __instance, InnerNetObject netObjParent, int ownerId, SpawnFlags flags)
    {
        if (DebugModeManager.IsDebugMode)
        {
            Logger.Info($"SpawnPatch", "InnerNetClient");
        }
        var messageWriter = __instance.Streams[(byte)SendOption.Reliable];
        if (messageWriter.Length > 500)
        {
            messageWriter.EndMessage();
            __instance.SendOrDisconnect(messageWriter);
            messageWriter.Clear(SendOption.Reliable);
            messageWriter.StartMessage(5);
            messageWriter.Write(__instance.GameId);
        }
    }
}