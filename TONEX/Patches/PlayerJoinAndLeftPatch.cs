using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TONEX.Modules;
using TONEX.Roles.Core;
using static TONEX.Translator;

namespace TONEX;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
class OnGameJoinedPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        while (!Options.IsLoaded) System.Threading.Tasks.Task.Delay(1);
        Logger.Info($"{__instance.GameId} 加入房间", "OnGameJoined");
        Main.playerVersion = new Dictionary<byte, PlayerVersion>();
        if (!Main.VersionCheat.Value) RPC.RpcVersionCheck();
        SoundManager.Instance.ChangeAmbienceVolume(DataManager.Settings.Audio.AmbienceVolume);

        Main.AllPlayerNames = new();
        ShowDisconnectPopupPatch.ReasonByHost = string.Empty;
        ChatUpdatePatch.DoBlockChat = false;
        GameStates.InGame = false;
        ErrorText.Instance.Clear();
        ServerAddManager.SetServerName();

        if (AmongUsClient.Instance.AmHost) //以下、ホストのみ実行
        {
            GameStartManagerPatch.GameStartManagerUpdatePatch.exitTimer = -1;
            Main.DoBlockNameChange = false;
            Main.NewLobby = true;
            Main.DevRole = new();
            EAC.DeNum = new();

            if (Main.NormalOptions.KillCooldown == 0f)
                Main.NormalOptions.KillCooldown = Main.LastKillCooldown.Value;

            AURoleOptions.SetOpt(Main.NormalOptions.Cast<IGameOptions>());
            if (AURoleOptions.ShapeshifterCooldown == 0f)
                AURoleOptions.ShapeshifterCooldown = Main.LastShapeshifterCooldown.Value;
        }
    }
}
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.OnBecomeHost))]
class OnBecomeHostPatch
{
    public static void Postfix()
    {
        if (GameStates.InGame)
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
    }
}
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.DisconnectInternal))]
class DisconnectInternalPatch
{
    public static void Prefix(InnerNetClient __instance, DisconnectReasons reason, string stringReason)
    {
        ShowDisconnectPopupPatch.Reason = reason;
        ShowDisconnectPopupPatch.StringReason = stringReason;

        Logger.Info($"断开连接(理由:{reason}:{stringReason}，Ping:{__instance.Ping})", "Session");

        ErrorText.Instance.CheatDetected = false;
        ErrorText.Instance.SBDetected = false;
        ErrorText.Instance.Clear();
        Cloud.StopConnect();

        if (AmongUsClient.Instance.AmHost && GameStates.InGame)
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);

        CustomRoleManager.Dispose();
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
class OnPlayerJoinedPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        Logger.Info($"{client.PlayerName}(ClientID:{client.Id}/FriendCode:{client.FriendCode}) 加入房间", "Session");
        if (AmongUsClient.Instance.AmHost && client.FriendCode == "" && Options.KickPlayerFriendCodeNotExist.GetBool())
        {
            Utils.KickPlayer(client.Id, false, "NotLogin");
            RPC.NotificationPop(string.Format(GetString("Message.KickedByNoFriendCode"), client.PlayerName));
            Logger.Info($"フレンドコードがないプレイヤーを{client?.PlayerName}をキックしました。", "Kick");
        }
        if (AmongUsClient.Instance.AmHost && client.PlatformData.Platform == Platforms.Android && Options.KickAndroidPlayer.GetBool())
        {
            Utils.KickPlayer(client.Id, false, "Andriod");
            string msg = string.Format(GetString("KickAndriodPlayer"), client?.PlayerName);
            RPC.NotificationPop(msg);
            Logger.Info(msg, "Android Kick");
        }
        if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
        {
            Utils.KickPlayer(client.Id, true, "BanList");
            Logger.Info($"ブロック済みのプレイヤー{client?.PlayerName}({client.FriendCode})をBANしました。", "BAN");
        }
        BanManager.CheckBanPlayer(client);
        BanManager.CheckDenyNamePlayer(client);
        RPC.RpcVersionCheck();

        if (AmongUsClient.Instance.AmHost)
        {
            if (Main.SayStartTimes.ContainsKey(client.Id)) Main.SayStartTimes.Remove(client.Id);
            if (Main.SayBanwordsTimes.ContainsKey(client.Id)) Main.SayBanwordsTimes.Remove(client.Id);
            if (Main.NewLobby && Options.ShareLobby.GetBool()) Cloud.ShareLobby();
        }
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
class OnPlayerLeftPatch
{
    static void Prefix([HarmonyArgument(0)] ClientData data)
    {
        if (!GameStates.IsInGame || !AmongUsClient.Instance.AmHost) return;
        CustomRoleManager.AllActiveRoles.Values.Do(role => role.OnPlayerDeath(data.Character, PlayerState.GetByPlayerId(data.Character.PlayerId).DeathReason, GameStates.IsMeeting));
    }
    public static List<int> ClientsProcessed = new();
    public static void Add(int id)
    {
        ClientsProcessed.Remove(id);
        ClientsProcessed.Add(id);
    }
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
    {
        //            Logger.info($"RealNames[{data.Character.PlayerId}]を削除");
        //            main.RealNames.Remove(data.Character.PlayerId);
        if (GameStates.IsInGame)
        {
            if (data.Character.Is(CustomRoles.Lovers) && !data.Character.Data.IsDead)
                foreach (var lovers in Main.LoversPlayers.ToArray())
                {
                    Main.isLoversDead = true;
                    Main.LoversPlayers.Remove(lovers);
                    PlayerState.GetByPlayerId(lovers.PlayerId).RemoveSubRole(CustomRoles.Lovers);
                }
            var state = PlayerState.GetByPlayerId(data.Character.PlayerId);
            if (state.DeathReason == CustomDeathReason.etc) //死因が設定されていなかったら
            {
                state.DeathReason = CustomDeathReason.Disconnected;
                state.SetDead();
            }
            AntiBlackout.OnDisconnect(data.Character.Data);
            PlayerGameOptionsSender.RemoveSender(data.Character);
        }

        Main.playerVersion.Remove(data.Character.PlayerId);
        Logger.Info($"{data?.PlayerName}(ClientID:{data?.Id}/FriendCode:{data?.FriendCode})断开连接(理由:{reason}，Ping:{AmongUsClient.Instance.Ping})", "Session");

        if (AmongUsClient.Instance.AmHost)
        {
            Main.SayStartTimes.Remove(__instance.ClientId);
            Main.SayBanwordsTimes.Remove(__instance.ClientId);

            // 附加描述掉线原因
            switch (reason)
            {
                case DisconnectReasons.Hacking:
                    RPC.NotificationPop(string.Format(GetString("PlayerLeftByAU-Anticheat"), data?.PlayerName));
                    break;
                case DisconnectReasons.Error:
                    RPC.NotificationPop(string.Format(GetString("PlayerLeftCuzError"), data?.PlayerName));
                    break;
                case DisconnectReasons.Kicked:
                case DisconnectReasons.Banned:
                    break;
                default:
                    if (!ClientsProcessed.Contains(data?.Id ?? 0))
                        RPC.NotificationPop(string.Format(GetString("PlayerLeft"), data?.PlayerName));
                    break;
            }
            ClientsProcessed.Remove(data?.Id ?? 0);
        }
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
class CreatePlayerPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        Logger.Msg($"创建玩家数据：ID{client.Character.PlayerId}: {client.PlayerName}", "CreatePlayer");

        //规范昵称
        var name = client.PlayerName;
        if (Options.FormatNameMode.GetInt() == 2 && client.Id != AmongUsClient.Instance.ClientId)
            name = Main.Get_TName_Snacks;
        else
        {
            // 删除非法字符
            name = name.RemoveHtmlTags().Replace(@"\", string.Empty).Replace("/", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\0", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty);
            // 删除超出10位的字符
            if (name.Length > 10) name = name[..10];
            // 删除Emoji
            if (Options.DisableEmojiName.GetBool()) name = Regex.Replace(name, @"\p{Cs}", string.Empty);
            // 若无有效字符则随机取名
            if (Regex.Replace(Regex.Replace(name, @"\s", string.Empty), @"[\x01-\x1F,\x7F]", string.Empty).Length < 1) name = Main.Get_TName_Snacks;
            // 替换重名
            string fixedName = name;
            int suffixNumber = 0;
            while (Main.AllPlayerNames.ContainsValue(fixedName))
            {
                suffixNumber++;
                fixedName = $"{name} {suffixNumber}";
            }
            if (!fixedName.Equals(name)) name = fixedName;
        }
        Main.AllPlayerNames.Remove(client.Character.PlayerId);
        Main.AllPlayerNames.TryAdd(client.Character.PlayerId, name);
        if (!name.Equals(client.PlayerName))
        {
            _ = new LateTask(() =>
            {
                if (client.Character == null) return;
                Logger.Warn($"规范昵称：{client.PlayerName} => {name}", "Name Format");
                //client.Character.RpcSetName(name);
                
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(client.Character.NetId, (byte)RpcCalls.SetName, SendOption.None, -1);
                    writer.Write(name);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                
            }, 1f, "Name Format");
        }

        _ = new LateTask(() => { if (client.Character == null || !GameStates.IsLobby) return; OptionItem.SyncAllOptions(client.Id); }, 3f, "Sync All Options For New Player");

        _ = new LateTask(() =>
        {
            if (client.Character == null) return;
            if (Main.OverrideWelcomeMsg != "") Utils.SendMessage(Main.OverrideWelcomeMsg, client.Character.PlayerId);
            else TemplateManager.SendTemplate("welcome", client.Character.PlayerId, true);
        }, 3f, "Welcome Message");
        if (Main.OverrideWelcomeMsg == "" && PlayerState.AllPlayerStates.Count != 0 && Main.clientIdList.Contains(client.Id))
        {
            if (Options.AutoDisplayKillLog.GetBool() && PlayerState.AllPlayerStates.Count != 0 && Main.clientIdList.Contains(client.Id))
            {
                _ = new LateTask(() =>
                {
                    if (!AmongUsClient.Instance.IsGameStarted && client.Character != null)
                    {
                        Utils.ShowKillLog(client.Character.PlayerId);
                    }
                }, 3f, "DisplayKillLog");
            }
            if (Options.AutoDisplayLastResult.GetBool())
            {
                _ = new LateTask(() =>
                {
                    if (!AmongUsClient.Instance.IsGameStarted && client.Character != null)
                    {
                        Utils.ShowLastResult(client.Character.PlayerId);
                    }
                }, 3.1f, "DisplayLastResult");
            }
            if (Options.EnableDirectorMode.GetBool())
            {
                _ = new LateTask(() =>
                {
                    if (!AmongUsClient.Instance.IsGameStarted && client.Character != null)
                    {
                        Utils.SendMessage($"{GetString("Message.DirectorModeNotice")}", client.Character.PlayerId);
                    }
                }, 3.2f, "DisplayDirectorModeWarnning");
            }
         if (Options.UsePets.GetBool())
            {
                _ = new LateTask(() =>
                {
                    if (!AmongUsClient.Instance.IsGameStarted && client.Character != null)
                    {
                        Utils.SendMessage($"{GetString("Message.PetModeNotice")}", client.Character.PlayerId);
                    }
                }, 3.2f, "DisplayDirectorModeWarnning");
            }
        }
    }
}