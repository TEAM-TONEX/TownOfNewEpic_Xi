﻿using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Runtime.Remoting.Messaging;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Descriptions;
using UnityEngine;
using static TONEX.Translator;
using static UnityEngine.GraphicsBuffer;

namespace TONEX.Modules;

public class ChatCommand(List<string> keywords, CommandAccess access, Func<MessageControl, (MsgRecallMode, string)> command)
{
    public List<string> KeyWords { get; set; } = keywords;

    public CommandAccess Access { get; set; } = access;

    public Func<MessageControl, (MsgRecallMode, string)> Command { get; set; } = command;

    public static List<ChatCommand> AllCommands;
    public static List<ChatCommand> SpamCommands;

    public static void Init()
    {
        SpamInitOnly();
        AllCommands = new()
        {
            new(["srm"], CommandAccess.Debugger, mc =>
            {
                ChatCommand.GetRoleByInputName(mc.Args, out var role);
                PlayerState.GetByPlayerId(mc.Player.PlayerId).SetMainRole(role);
                return (MsgRecallMode.Block, null);
            }),
            new(["srs"], CommandAccess.Debugger, mc =>
            {

                ChatCommand.GetRoleByInputName(mc.Args, out var role);
                PlayerState.GetByPlayerId(mc.Player.PlayerId).SetSubRole(role);
                return (MsgRecallMode.Block, null);
            }),
            new(["sr"], CommandAccess.Debugger, mc =>
            {
                
                SetRoles(mc.Args, mc.Player.PlayerId);
                return (MsgRecallMode.Block, null);
            }),
            new(["rev"], CommandAccess.Debugger, mc =>
            {
                var id = Convert.ToByte(mc.Args);
                var player = Utils.GetPlayerById(id);
                player.RpcSetRole(RoleTypes.Crewmate, true);
                
                return (MsgRecallMode.Block, null);
            }),
            new(["mw", "messagewait"], CommandAccess.Host, mc =>
            {
                string text = $"{GetString("Message.MessageWaitHelp")}\n{GetString("ForExample")}:\nmw 3";
                if (int.TryParse(mc.Args, out int sec))
                {
                    Main.MessageWait.Value = sec;
                    text = string.Format(GetString("Message.SetToSeconds"), sec);
                }
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, text);
            }),
            new(["dump"], CommandAccess.LocalMod, mc =>
            {
                Utils.DumpLog();
                return (MsgRecallMode.Block, null);
            }),
            new(["v", "ver", "version"], CommandAccess.LocalMod, mc =>
            {
                StringBuilder sb = new();
                foreach (var kvp in Main.playerVersion.OrderBy(pair => pair.Key))
                    sb.Append($"{kvp.Key}:{Main.AllPlayerNames[kvp.Key]}:{kvp.Value.forkId}/{kvp.Value.version}({kvp.Value.tag})\n");
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, sb.ToString());
            }),
            new(["win", "winner"], CommandAccess.All, mc =>
            {
                string text = GetString("NoInfoExists");
                if (Main.winnerNameList.Any())
                    text = "Winner: " + string.Join(",", Main.winnerNameList);
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, text);
            }),
            new(["level"], CommandAccess.Host, mc =>
            {
                string text = GetString("Message.AllowLevelRange");
                if (int.TryParse(mc.Args, out int level) && level is >= 1 and <= 999)
                {
                    text = string.Format(GetString("Message.SetLevel"), level);
                    mc.SendToList.Add(mc.Player.PlayerId);
                    mc.Player.RpcSetLevel(Convert.ToUInt32(level) - 1);
                }
                return (MsgRecallMode.Block, text);
            }),
            new(["l", "lastresult"], CommandAccess.All, mc =>
            {
                Utils.ShowKillLog(mc.Player.PlayerId);
                Utils.ShowLastResult(mc.Player.PlayerId);
                return (MsgRecallMode.Block, null);
            }),
            new(["rn", "rename"], CommandAccess.Host, mc =>
            {
                string text = mc.Args.Length is > 10 or < 1 ? GetString("Message.AllowNameLength") : null;
                if (text == null) Main.HostNickName = mc.Args;
                mc.SendToList.Add(mc.Player.PlayerId);
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, text);
            }),
            new(["hn", "hidename"], CommandAccess.Host, mc =>
            {
                Main.HideName.Value = mc.HasValidArgs ? mc.Args : Main.HideName.DefaultValue.ToString();
                GameStartManagerPatch.HideName.text = Main.HideName.Value;
                return (MsgRecallMode.Block, null);
            }),
            new(["hy", "mt", "meeting"], CommandAccess.Host, mc =>
            {
                if (GameStates.IsMeeting) MeetingHud.Instance.RpcClose();
                else mc.Player.NoCheckStartMeeting(null, true);
                return (MsgRecallMode.Block, null);
            }),
            new(["now","n"], CommandAccess.All, mc =>
            {
                switch (mc.Args)
                {
                    case "r":
                    case "roles":
                        Utils.ShowActiveRoles(mc.Player.PlayerId);
                        break;
                    default:
                        Utils.ShowActiveSettings(mc.Player.PlayerId);
                        break;
                }
                return (MsgRecallMode.Block, null);
            }),
            new(["disconnect","dis" ], CommandAccess.Host, mc =>
            {
                switch (mc.Args)
                {
                    case "crew":
                        GameManager.Instance.enabled = false;
                        GameManager.Instance.RpcEndGame(GameOverReason.HumansDisconnect, false);
                        break;
                    case "imp":
                        GameManager.Instance.enabled = false;
                        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                        break;
                    default:
                        Utils.AddChatMessage("crew | imp");
                        break;
                }
                return (MsgRecallMode.Block, null);
            }),
            new(["role","r"], CommandAccess.All, mc =>
            {
                SendRolesInfo(mc.Args, mc.Player.PlayerId);
                return (MsgRecallMode.Block, null);
            }),
            new(["up", "specify"], CommandAccess.Host, mc =>
            {
                SpecifyRole(mc.Args, mc.Player.PlayerId);
                return (MsgRecallMode.Block, null);
            }),
            new(["qq", "share"], CommandAccess.Host, mc =>
            {
                Cloud.ShareLobby(true);
                return (MsgRecallMode.Block, null);
            }),
            new(["h", "help"], CommandAccess.All, mc =>
            {
                Utils.ShowHelp(mc.Player.PlayerId);
                return (MsgRecallMode.Block, null);
            }),
            new(["ss", "SetScanner"], CommandAccess.All, mc =>
            {
                if (!GameStates.IsLobby)
                {
                    string cantuse = GetString("Message.ReadySetScannerReturn");
                    return (MsgRecallMode.Block, cantuse);
                }
                string text = GetString("Message.ReadySetScanner");
                var player = mc.Player;
                player.RpcSetScanner(true);
             /*   MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetScanner, SendOption.Reliable, -1);
                writer.Write(true);
                AmongUsClient.Instance.FinishRpcImmediately(writer);*/
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, text);
            }),
            new(["m", "myrole"], CommandAccess.All, mc =>
            {
                string text = GetString("Message.CanNotUseInLobby");
                if (GameStates.IsInGame)
                {
                    var role = mc.Player.GetCustomRole();
                    text = role.GetRoleInfo()?.Description?.GetFullFormatHelpWithAddonsByPlayer(mc.Player) ??
                        // roleInfoがない役職
                        GetString(role.ToString()) + mc.Player.GetRoleInfo(true);
                }
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, text);
            }),
            new(["eje", "ejection","ejections"], CommandAccess.All, mc =>
            {
                string text = GetString("Message.CanNotUseInLobby");
                if (GameStates.IsInGame)
                {
                    if(ConfirmEjections.LatestEjec==null)
                        text = GetString("Message.NonLatestEje");
                    else
                    text = ConfirmEjections.LatestEjec;
                }
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, text);
            }),
            new(["ank", "activenk","activeneutralkiller"], CommandAccess.All, mc =>
            {
                Utils.ShowActiveNKs(mc.Player.PlayerId);
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, null);
            }),
            new(["t", "template"], CommandAccess.LocalMod, mc =>
            {
                if (mc.HasValidArgs) TemplateManager.SendTemplate(mc.Args);
                else Utils.AddChatMessage($"{GetString("ForExample")}:\nt test");
                return (MsgRecallMode.Block, null);
            }),
            new(["exe", "execute"], CommandAccess.Host, mc =>
            {
                string text = GetString("Message.CanNotUseInLobby");
                if (GameStates.IsInGame)
                {
                    if (!mc.HasValidArgs || !int.TryParse(mc.Args, out int id)) return (MsgRecallMode.Block, null);
                    var target = Utils.GetPlayerById(id);
                    if (target != null)
                    {
                        target.Data.IsDead = true;
                        var state = PlayerState.GetByPlayerId(target.PlayerId);
                        state.DeathReason = CustomDeathReason.etc;
                        target.RpcExileV2();
                        state.SetDead();
                        text = target.AmOwner
                            ? Utils.ColorString(Color.red, GetString("HostKillSelfByCommand"))
                            : string.Format(GetString("Message.Executed"), target.Data.PlayerName);
                    }
                    foreach (var pc in Main.AllPlayerControls)
                    {
                        mc.SendToList.Add(pc.PlayerId);
                    }
                }
                return (MsgRecallMode.Block, text);
            }),
            new(["kill"], CommandAccess.Host, mc =>
            {
                string text = GetString("Message.CanNotUseInLobby");
                if (GameStates.IsInGame)
                {
                    if (!mc.HasValidArgs || !int.TryParse(mc.Args, out int id)) return (MsgRecallMode.Block, null);
                    var target = Utils.GetPlayerById(id);
                    if (target != null)
                    {
                        var state = PlayerState.GetByPlayerId(target.PlayerId);
                        state.DeathReason = CustomDeathReason.etc;
                        target.RpcMurderPlayer(target);
                        text = target.AmOwner
                            ? Utils.ColorString(Color.red, GetString("HostKillSelfByCommand"))
                            : string.Format(GetString("Message.Executed"), target.Data.PlayerName);
                    }
                }
                 foreach (var pc in Main.AllPlayerControls)
                    {
                        mc.SendToList.Add(pc.PlayerId);
                    }
                return (MsgRecallMode.Block, text);
            }),
            new(["color", "colour"], Options.PlayerCanSetColor.GetBool()?CommandAccess.All:CommandAccess.Host, mc =>
            {
                string text = GetString("Message.OnlyCanUseInLobby");
                if (GameStates.IsLobby)
                {
                    text = GetString("IllegalColor");
                    var color = Utils.MsgToColor(mc.Args, mc.IsFromSelf);
                    if (color != byte.MaxValue)
                    {
                        mc.Player.SetOutFit(color);
                        text = string.Format(GetString("Message.SetColor"), mc.Args);
                    }
                }
                mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Block, text);
            }),
            new(["qt", "quit"], CommandAccess.All, mc =>
            {
                string text = GetString("Message.CanNotUseByHost");
                mc.SendToList.Add(mc.Player.PlayerId);
                if (!mc.IsFromSelf)
                {
                    var cid = mc.Player.PlayerId.ToString();
                    cid = cid.Length != 1 ? cid.Substring(1, 1) : cid;
                    if (mc.Args.Equals(cid))
                    {
                        string name = mc.Player.GetRealName();
                        text = string.Format(GetString("Message.PlayerQuitForever"), name);
                        Utils.KickPlayer(mc.Player.GetClientId(), true, "VoluntarilyQuit");
                        foreach (var pc in Main.AllPlayerControls)
                    {
                        mc.SendToList.Add(pc.PlayerId);
                    }
                    }
                    else
                    {
                        text = string.Format(GetString("SureUse.quit"), cid);
                        mc.SendToList.Add(mc.Player.PlayerId);
                    }
                }
                return (MsgRecallMode.Block, text);
            }),
            
            new(["end", "endgame"], CommandAccess.Host, mc =>
            {
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Draw);
                GameManager.Instance.LogicFlow.CheckEndCriteria();
                return (MsgRecallMode.Block, null);
            }),

            new(["cosid"], CommandAccess.Host, mc =>
            {
                var of =mc.Player.Data.DefaultOutfit;
                Logger.Warn($"ColorId: {of.ColorId}", "Get Cos Id");
                Logger.Warn($"PetId: {of.PetId}", "Get Cos Id");
                Logger.Warn($"HatId: {of.HatId}", "Get Cos Id");
                Logger.Warn($"SkinId: {of.SkinId}", "Get Cos Id");
                Logger.Warn($"VisorId: {of.VisorId}", "Get Cos Id");
                Logger.Warn($"NamePlateId: {of.NamePlateId}", "Get Cos Id");
                return (MsgRecallMode.Block, null);
            }),
        };
        
    }
    public static void SpamInitOnly()
    {
        InitRoleCommands();
        SpamCommands = new()
        {
            new(["id"], CommandAccess.All, mc =>
            {
                string text = GetString("PlayerIdList");
                foreach (var pc in Main.AllPlayerControls)
                    text += "\n" + pc.PlayerId.ToString() + " → " + Main.AllPlayerNames[pc.PlayerId];
                 mc.SendToList.Add(mc.Player.PlayerId);
                return (MsgRecallMode.Spam, text);
            }),
            new(["id","guesslist","gl编号","玩家编号","玩家id","id列表","玩家列表","列表","所有id","全部id",
            "shoot","guess","bet","st","gs","bt","猜","赌"], CommandAccess.All, mc =>
            {
                bool isCommand = GuesserHelper.GuesserMsg(mc.Player, mc.Message, out bool spam);
                var recallMode = spam ? MsgRecallMode.Spam : MsgRecallMode.None;
                return (recallMode, null);
            }),
        };

    }

    private static Dictionary<CustomRoles, List<string>> RoleCommands;
    public static void InitRoleCommands()
    {
        // 初回のみ処理
        RoleCommands = new();

        // GM
        RoleCommands.Add(CustomRoles.GM, new() { "gm", "管理" });

        // RoleClass
        ConcatCommands(CustomRoleTypes.Impostor);
        ConcatCommands(CustomRoleTypes.Crewmate);
        ConcatCommands(CustomRoleTypes.Neutral);

    }
    public static void SendRolesInfo(string input, byte playerId, bool onlycountexists = false)
    {
        if (Options.CurrentGameMode == CustomGameMode.HotPotato)
        {
            Utils.SendMessage(GetString("ModeDescribe.HotPotato"), playerId);
            return;
        }
        else if (Options.CurrentGameMode == CustomGameMode.InfectorMode)
        {
            Utils.SendMessage(GetString("ModeDescribe.InfectorMode"), playerId);
            return;
        }
        else if (Options.CurrentGameMode == CustomGameMode.FFA)
        {
            Utils.SendMessage(GetString("ModeDescribe.FFA"), playerId);
            return;
        }



        if (string.IsNullOrWhiteSpace(input))
        {
            Utils.ShowActiveRoles(playerId);
            return;
        }
        else if (!GetRoleByInputName(input, out var role) || role is CustomRoles.Non_Villain or CustomRoles.HotPotato or CustomRoles.ColdPotato)
        {
            Utils.SendMessage(GetString("Message.CanNotFindRoleThePlayerEnter"), playerId);
            return;
        }
        else
        {

            if (!role.IsAddon() && !role.IsVanilla())
                Utils.SendMessage(role.GetRoleInfo().Description.FullFormatHelp, playerId);
            else if (role.IsVanilla())
                Utils.SendMessage(role.GetRoleInfo().Description.FullFormatHelp, playerId);
            else if (role.IsAddon())
            {
                Utils.SendMessage(AddonDescription.FullFormatHelpByRole(role) 
                    ??
                            // roleInfoがない役職
                            $"<size=130%><color={Utils.GetRoleColor(role)}>{GetString(role.ToString())}</color></size>:\n\n{role.GetRoleInfoWithRole()}", playerId);
            }
        }
    }
    public static void SetRoles(string input, byte playerId)
    {
        if (Options.CurrentGameMode == CustomGameMode.HotPotato)
        {
            Utils.SendMessage(GetString("ModeDescribe.HotPotato"), playerId);
            return;
        }
        if (Options.CurrentGameMode == CustomGameMode.InfectorMode)
        {
            Utils.SendMessage(GetString("ModeDescribe.InfectorMode"), playerId);
            return;
        }
        if (Options.CurrentGameMode == CustomGameMode.FFA)
        {
            Utils.SendMessage(GetString("ModeDescribe.FFA"), playerId);
            return;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            Utils.ShowActiveRoles(playerId);
            return;
        }
        else if (!GetRoleByInputName(input, out var role))
        {
            Utils.SendMessage(GetString("Message.CanNotFindRoleThePlayerEnter"), playerId);
            return;
        }
        else
        {
            Utils.GetPlayerById(playerId).RpcSetCustomRole(role);
        }
    }
    public static void SpecifyRole(string input, byte playerId)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Utils.ShowActiveRoles(playerId);
            return;
        }
        else if (!GetRoleByInputName(input, out var role))
        {
            Utils.SendMessage(GetString("Message.DirectorModeCanNotFindRoleThePlayerEnter"), playerId);
            return;
        }
        else if (!Options.EnableDirectorMode.GetBool())
        {
            Utils.SendMessage(string.Format(GetString("Message.DirectorModeDisabled"), GetString("EnableDirectorMode")));
        }
        else if (!GameStates.IsLobby)
        {
            Utils.SendMessage(GetString("Message.OnlyCanUseInLobby"), playerId);
        }
        else
        {
            string roleName = GetString(Enum.GetName(typeof(CustomRoles), role));
            if (
                !role.IsEnable()
                || role.IsAddon()
                || role.IsVanilla() && Options.DisableVanillaRoles.GetBool()
                || role is CustomRoles.GM or CustomRoles.NotAssigned
                || role.IsHidden()
                || role.IsCanNotOpen()
                || !Options.CustomRoleSpawnChances.ContainsKey(role))
            {
                Utils.SendMessage(string.Format(GetString("Message.DirectorModeSelectFailed"), roleName), playerId);
            }
            else
            {
                byte pid = playerId == byte.MaxValue ? byte.MinValue : playerId;
                Main.DevRole.Remove(pid);
                Main.DevRole.Add(pid, role);

                Utils.SendMessage(string.Format(GetString("Message.DirectorModeSelected"), roleName), playerId);
            }
        }
    }
    private static void ConcatCommands(CustomRoleTypes roleType)
    {
        var roles = CustomRoleManager.AllRolesInfo.Values.Where(role => role.CustomRoleType == roleType);
        foreach (var role in roles)
        {
            if (role.ChatCommand is null) continue;
            var coms = role.ChatCommand.Split('|');
            RoleCommands[role.RoleName] = new();
            coms.DoIf(c => c.Trim() != "", RoleCommands[role.RoleName].Add);
        }
    }
    public static bool GetRoleByInputName(string input, out CustomRoles output, bool includeVanilla = false)
    {
        output = new();
        input = Regex.Replace(input, @"[0-9]+", string.Empty); //清除数字
        input = Regex.Replace(input, @"\s", string.Empty); //清除空字符
        input = Regex.Replace(input, @"[\x01-\x1F,\x7F]", string.Empty); //清除无效字符
        input = input.ToLower().Trim().Replace("是", string.Empty).Replace("着", "者");
        if (string.IsNullOrEmpty(input)) return false;
        foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
        {
            if (!includeVanilla && Options.DisableVanillaRoles.GetBool() && role.IsVanilla()) continue;
            if (input == GetString(Enum.GetName(typeof(CustomRoles), role)).TrimStart('*').ToLower().Trim().Replace(" ", string.Empty).RemoveHtmlTags() //匹配到翻译文件中的职业原名
                || (RoleCommands.TryGetValue(role, out var com) && com.Any(c => input == c.Trim().ToLower())) //匹配到职业缩写
                )
            {
                output = role;
                return true;
            }
        }
        return false;
    }
}

public enum CommandAccess
{
    All, // Everyone Can use this command
    LocalMod, // Command won't received by host
    Host, // Only host can use this comand
    Debugger,
}