using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TONEX.Modules;
using TONEX.Roles.AddOns.Common;
using TONEX.Roles.AddOns.Crewmate;
using TONEX.Roles.Core;
using TONEX.Roles.Crewmate;
using TONEX.Roles.Neutral;
using UnityEngine;
using YamlDotNet.Core;
using static TONEX.Translator;
using static UnityEngine.GraphicsBuffer;

namespace TONEX;

[HarmonyPatch]
public static class MeetingHudPatch
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    class CheckForEndVotingPatch
    {
        public static bool Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Main.AssistivePluginMode.Value) return true;
            MeetingVoteManager.Instance?.CheckAndEndMeeting();
            return false;
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
    public static class CastVotePatch
    {
        public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId /* 投票者 */ , [HarmonyArgument(1)] byte suspectPlayerId /* 被票者 */ )
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (Main.AssistivePluginMode.Value) return true;
            var voter = Utils.GetPlayerById(srcPlayerId);
            var voted = Utils.GetPlayerById(suspectPlayerId);

            if (voter != null)
            {
                if (!Madmate.CheckVoteAsVoter(srcPlayerId, suspectPlayerId, voter, ref __instance)) return false;
                if (voter.GetRoleClass()?.CheckVoteAsVoter(voted) == false || voter.Any_Addons(x=>x.CheckVoteAsVoter(voted) == false))
                {
                    __instance.RpcClearVote(voter.GetClientId());
                    Logger.Info($"{voter.GetNameWithRole()} 的投票被清除", nameof(CastVotePatch));
                    return false;
                }
            }

            MeetingVoteManager.Instance?.SetVote(srcPlayerId, suspectPlayerId);

           return true;
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPriority(Priority.First)]
    class StartPatch
    {
        public static void Prefix()
        {
            Logger.Info("------------会议开始------------", "Phase");
            if (!Main.AssistivePluginMode.Value)
            {
                
                ChatUpdatePatch.DoBlockChat = true;
                GameStates.AlreadyDied |= !Utils.IsAllAlive;
                Main.AllPlayerControls.Do(x => ReportDeadBodyPatch.WaitReport[x.PlayerId].Clear());
                MeetingStates.MeetingCalled = true;
            }
        }
        public static void Postfix(MeetingHud __instance)
        {
            if (Main.AssistivePluginMode.Value)
            {
                foreach (var pva in __instance.playerStates)
                {
                    var pc = Utils.GetPlayerById(pva.TargetPlayerId);
                    if (pc == null) continue;
                    var roleTextMeeting = UnityEngine.Object.Instantiate(pva.NameText);
                    roleTextMeeting.text = "";
                    roleTextMeeting.enabled = false;
                    roleTextMeeting.transform.SetParent(pva.NameText.transform);
                    roleTextMeeting.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                    roleTextMeeting.fontSize = 1.5f;
                    roleTextMeeting.gameObject.name = "RoleTextMeeting";
                    roleTextMeeting.enableWordWrapping = false;

                    // 役職とサフィックスを同時に表示する必要が出たら要改修
                    var suffixBuilder = new StringBuilder(32);
                    var roleType = pc.Data.Role.Role;
                    var cr = roleType.GetCustomRoleTypes();
                    var color = Utils.GetRoleColorCode(cr);
                    if (pc == PlayerControl.LocalPlayer)
                    {
                        pva.NameText.text =
        $"<color={color}>{pva.NameText.text}</color>";
                        suffixBuilder.Append
                            (
                            $"<color={color}><size=80%>{Translator.GetRoleString(cr.ToString())}</size></color>"
                            );
                    }
                    else
                    {

                        if (PlayerControl.LocalPlayer.Data.IsDead && pc.Data.IsDead)
                        {
                            pva.NameText.text =
                                $"<color={color}>{pva.NameText.text}</color>";
                            suffixBuilder.Append
                            (
                                $"<color={color}><size=80%>{Translator.GetRoleString(cr.ToString())}</size></color>");
                        }
                        else if (PlayerControl.LocalPlayer.Data.Role.Role.GetCustomRoleTypes().IsImpostor() && cr.IsImpostor())
                        {
                            if (PlayerControl.LocalPlayer.Data.IsDead)
                            {
                                pva.NameText.text =
        $"<color=#ff1919>{pva.NameText.text}</color>";
                                suffixBuilder.Append
                            (
                                       $"<color={color}><size=80%>{Translator.GetRoleString(cr.ToString())}</size></color>");
                            }
                            else
                            {

                                pva.NameText.text =
                                $"<color=#FF1919>{pc?.name}</color>";

                            }
                        }
                        else
                        {

                            pva.NameText.text =
                            $"<color=#FFFFFF>{pc?.name}</color>";



                        }

                    }
                    if (suffixBuilder.Length > 0)
                    {
                        roleTextMeeting.text = suffixBuilder.ToString();
                        roleTextMeeting.enabled = true;
                    }
                }
                return;
            }
            MeetingVoteManager.Start();

            SoundManager.Instance.ChangeAmbienceVolume(0f);
            if (!GameStates.IsModHost) return;
            var myRole = PlayerControl.LocalPlayer.GetRoleClass();
            var myAddons = PlayerControl.LocalPlayer.GetAddonClasses();
            foreach (var pva in __instance.playerStates)
            {
                var pc = Utils.GetPlayerById(pva.TargetPlayerId);
                if (pc == null) continue;
                var roleTextMeeting = UnityEngine.Object.Instantiate(pva.NameText);
                roleTextMeeting.transform.SetParent(pva.NameText.transform);
                roleTextMeeting.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                roleTextMeeting.fontSize = 1.5f;
                (roleTextMeeting.enabled, roleTextMeeting.text)
                    = Utils.GetRoleNameAndProgressTextData(PlayerControl.LocalPlayer, pc);
                roleTextMeeting.gameObject.name = "RoleTextMeeting";
                roleTextMeeting.enableWordWrapping = false;

                // 役職とサフィックスを同時に表示する必要が出たら要改修
                var suffixBuilder = new StringBuilder(32);
                if (myRole != null)
                {
                    suffixBuilder.Append(myRole.GetSuffix(PlayerControl.LocalPlayer, pc, isForMeeting: true));
                }
                if (myAddons != null)
                    myAddons.Do_Addons(x => suffixBuilder.Append(x.GetSuffix(PlayerControl.LocalPlayer, pc, isForMeeting: true)));

                suffixBuilder.Append(CustomRoleManager.GetSuffixOthers(PlayerControl.LocalPlayer, pc, isForMeeting: true));


                if (suffixBuilder.Length > 0)
                {
                    roleTextMeeting.text = suffixBuilder.ToString();
                    roleTextMeeting.enabled = true;
                }
            }

            if (Options.SyncButtonMode.GetBool())
            {
                Utils.SendMessage(string.Format(GetString("Message.SyncButtonLeft"), Options.SyncedButtonCount.GetFloat() - Options.UsedButtonCount));
                Logger.Info("紧急会议剩余 " + (Options.SyncedButtonCount.GetFloat() - Options.UsedButtonCount) + " 次使用次数", "SyncButtonMode");
            }
            if (AntiBlackout.OverrideExiledPlayer && !Options.NoGameEnd.GetBool())
            {
                _ = new LateTask(() =>
                {
                    Utils.SendMessage(GetString("Warning.OverrideExiledPlayer"), 255, Utils.ColorString(Color.red, GetString("DefaultSystemMessageTitle")));
                }, 5f, "Warning OverrideExiledPlayer");
            }
            if (MeetingStates.FirstMeeting) TemplateManager.SendTemplate("OnFirstMeeting", noErr: true);
            TemplateManager.SendTemplate("OnMeeting", noErr: true);

            if (AmongUsClient.Instance.AmHost)
            {
                _ = new LateTask(() =>
                {
                    foreach (var seen in Main.AllPlayerControls)
                    {
                        var seenName = seen.GetRealName(isMeeting: true);
                        var coloredName = Utils.ColorString(seen.GetRoleColor(), seenName);
                        foreach (var seer in Main.AllPlayerControls)
                        {
                            seen.RpcSetNamePrivate(
                                seer == seen ? coloredName : seenName,
                                seer);
                        }
                    }
                    ChatUpdatePatch.DoBlockChat = false;
                }, 3f, "SetName To Chat");
            }

            if (AmongUsClient.Instance.AmHost)
            {
                CustomRoleManager.AllActiveRoles.Values.Do(role => 
                { 
                    role.OnStartMeeting();
                    if (role.Player.IsAlive())
                    {
                        for (int i = 0; i < role.CountdownList.Count; i++)
                        {
                            role.ZeroingCountdown(i);
                        }
                        role.UsePetCooldown_Timer = -1;
                    }
                });
                CustomRoleManager.AllActiveAddons.Values.Do(x => x.Do_Addons(role =>
                {
                    role.OnStartMeeting();
                    if (role.Player.IsAlive())
                    {
                        for (int i = 0; i < role.CountdownList.Count; i++)
                        {
                            role.CountdownList[i] = -1;
                        }
                        role.UsePetCooldown_Timer = -1;
                    }
                }));
                MeetingStartNotify.OnMeetingStart();

            }

            foreach (var pva in __instance.playerStates)
            {
                if (pva == null) continue;
                var seer = PlayerControl.LocalPlayer;
                var seerRole = seer.GetRoleClass();
                var seerAddon = seer.GetAddonClasses();
                var target = Utils.GetPlayerById(pva.TargetPlayerId);
                if (target == null) continue;

                var sb = new StringBuilder();

                //会議画面での名前変更
                //自分自身の名前の色を変更
                //NameColorManager準拠の処理
                pva.NameText.text = pva.NameText.text.ApplyNameColorData(seer, target, true);

                var overrideName = pva.NameText.text;
                //调用职业类通过 seer 重写 name
                seer.GetRoleClass()?.OverrideNameAsSeer(target, ref overrideName, true);
                seer.Do_Addons(x => x?.OverrideNameAsSeer(target, ref overrideName, true));
                //调用职业类通过 seen 重写 name
                target.GetRoleClass()?.OverrideNameAsSeen(seer, ref overrideName, true);
                target.Do_Addons(x => x?.OverrideNameAsSeen(seer, ref overrideName, true));

                pva.NameText.text = overrideName;
                //とりあえずSnitchは会議中にもインポスターを確認することができる仕様にしていますが、変更する可能性があります。

                if (seer.KnowDeathReason(target))
                    sb.Append($"({Utils.ColorString(Utils.GetRoleColor(CustomRoles.MedicalExaminer), Utils.GetVitalText(target.PlayerId))})");

                sb.Append(seerRole?.GetMark(seer, target, true));
                seerAddon?.Do_Addons(x => sb.Append(x?.GetMark(seer, target, true)));

                sb.Append(CustomRoleManager.GetMarkOthers(seer, target, true));

                bool isLover = false;
                foreach (var subRole in target.GetCustomSubRoles())
                {
                    switch (subRole)
                    {
                        case CustomRoles.Lovers:
                            if (seer.Is(CustomRoles.Lovers) || seer.Data.IsDead)
                            {
                                sb.Append(Utils.ColorString(Utils.GetRoleColor(CustomRoles.Lovers), "♡"));
                                isLover = true;
                            }
                            break;
                        case CustomRoles.AdmirerLovers:
                            if (seer.Is(CustomRoles.AdmirerLovers) || seer.Data.IsDead)
                            {
                                sb.Append(Utils.ColorString(Utils.GetRoleColor(CustomRoles.AdmirerLovers), "♡"));
                                isLover = true;
                            }
                            break;
                        case CustomRoles.AkujoLovers:
                            if (seer.Is(CustomRoles.Akujo) || seer.Data.IsDead || seer == target)
                            {
                                sb.Append(Utils.ColorString(Utils.GetRoleColor(CustomRoles.AkujoLovers), "❤"));
                                isLover = true;
                            }
                            break;
                        case CustomRoles.CupidLovers:
                            if (seer.Is(CustomRoles.CupidLovers) || seer.Is(CustomRoles.Cupid) || seer.Data.IsDead)
                            {
                                sb.Append(Utils.ColorString(Utils.GetRoleColor(CustomRoles.CupidLovers), "♡"));
                                isLover = true;
                            }
                            break;
                    }
                }

                AkujoFakeLovers.MeetingHud(isLover, seer, target, ref sb);
                //海王相关显示
                Neptune.MeetingHud(isLover, seer, target, ref sb);


                //会議画面ではインポスター自身の名前にSnitchマークはつけません。

                pva.NameText.text += sb.ToString();
                pva.ColorBlindName.transform.localPosition -= new Vector3(1.35f, 0f, 0f);

            }
        }

    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class UpdatePatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (Main.AssistivePluginMode.Value) return;
                if (!AmongUsClient.Instance.AmHost || !GameStates.IsInGame || __instance == null || __instance.IsDestroyedOrNull()) return;
            if (Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.LeftControl))
            {
                __instance.playerStates.DoIf(x => x.HighlightedFX.enabled, x =>
                {
                    var player = Utils.GetPlayerById(x.TargetPlayerId);
                    player.RpcExileV2();
                    var state = PlayerState.GetByPlayerId(player.PlayerId);
                    state.DeathReason = CustomDeathReason.Execution;
                    state.SetDead();
                    Utils.SendMessage(string.Format(GetString("Message.Executed"), player.Data.PlayerName));
                    Logger.Info($"{player.GetNameWithRole()}を処刑しました", "Execution");
                    __instance.CheckForEndVoting();
                });
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
    class OnDestroyPatch
    {
        public static void Postfix()
        {

            MeetingStates.FirstMeeting = false;
            Logger.Info("------------会议结束------------", "Phase");
            if (Main.AssistivePluginMode.Value) return;
            if (AmongUsClient.Instance.AmHost)
            {
                AntiBlackout.SetIsDead();
                Main.AllPlayerControls.Do(pc => RandomSpawn.CustomNetworkTransformPatch.FirstTP[pc.PlayerId] = false);
                EAC.MeetingTimes = 0;
            }

            // MeetingVoteManagerを通さずに会議が終了した場合の後処理
            MeetingVoteManager.Instance?.Destroy();
        }
    }

    public static void TryAddAfterMeetingDeathPlayers(CustomDeathReason deathReason, params byte[] playerIds)
    {
        var AddedIdList = new List<byte>();
        foreach (var playerId in playerIds)
            if (Main.AfterMeetingDeathPlayers.TryAdd(playerId, deathReason))
                AddedIdList.Add(playerId);
        CheckForDeathOnExile(deathReason, AddedIdList.ToArray());
    }
    public static void CheckForDeathOnExile(CustomDeathReason deathReason, params byte[] playerIds)
    {
        Lovers.CheckForDeathOnExile(deathReason, playerIds);
        AdmirerLovers.CheckForDeathOnExile(deathReason, playerIds);
        AkujoLovers.CheckForDeathOnExile(deathReason, playerIds);
        CupidLovers.CheckForDeathOnExile(deathReason, playerIds);
    }
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetHighlighted))]
class SetHighlightedPatch
{
    public static bool Prefix(PlayerVoteArea __instance, bool value)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (!__instance.HighlightedFX) return false;
        __instance.HighlightedFX.enabled = value;
        return false;
    }
}