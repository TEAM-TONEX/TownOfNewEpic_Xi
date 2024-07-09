using AmongUs.GameOptions;
using HarmonyLib;
using System;
using System.Linq;
using System.Threading.Tasks;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Translator;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using Hazel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using TONEX.Roles.Core.Interfaces;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.AddOns.Common;

namespace TONEX;

[HarmonyPatch(typeof(IntroCutscene))]
class IntroCutscenePatch
{
    [HarmonyPatch(nameof(IntroCutscene.ShowRole)), HarmonyPostfix]
    public static void ShowRole_Postfix(IntroCutscene __instance)
    {

        if (Main.AssistivePluginMode.Value)
        {
            _ = new LateTask(() =>
            {
                var roleType = PlayerControl.LocalPlayer.Data.Role.Role;
            var cr = roleType.GetCustomRoleTypes();
            __instance.YouAreText.color = Utils.GetRoleColor(cr);
            __instance.RoleText.text = Utils.GetRoleName(cr);
            __instance.RoleText.color = Utils.GetRoleColor(cr);
            __instance.RoleText.fontWeight = TMPro.FontWeight.Thin;
            __instance.RoleText.SetOutlineColor(Utils.ShadeColor(Utils.GetRoleColor(cr), 0.1f).SetAlpha(0.38f));
            __instance.RoleText.SetOutlineThickness(0.17f);
            __instance.RoleBlurbText.color = Utils.GetRoleColor(cr);
            __instance.RoleBlurbText.text = cr.GetRoleInfoForVanilla();
            
            }, 0.0001f, "Override Role Text");
            return;
        }
        if (!GameStates.IsModHost) return;
        _ = new LateTask(() =>
        {
            if (Options.CurrentGameMode == CustomGameMode.HotPotato)
            {
                var color = ColorUtility.TryParseHtmlString("#FF9900", out var c) ? c : new(255, 255, 255, 255);
                CustomRoles roles = PlayerControl.LocalPlayer.GetCustomRole();
                __instance.YouAreText.color = color;
                __instance.RoleText.text = Utils.GetRoleName(roles);
                __instance.RoleText.color = Utils.GetRoleColor(roles);
                __instance.RoleBlurbText.color = color;
                __instance.RoleBlurbText.text = PlayerControl.LocalPlayer.GetRoleInfo();
            }
           else if (Options.CurrentGameMode == CustomGameMode.InfectorMode){
                var color = ColorUtility.TryParseHtmlString("#FF9900", out var c) ? c : new(255, 255, 255, 255);
                CustomRoles roles = PlayerControl.LocalPlayer.GetCustomRole();
                __instance.YouAreText.color = color;
                __instance.RoleText.text = Utils.GetRoleName(roles);
                __instance.RoleText.color = Utils.GetRoleColor(roles);
                __instance.RoleBlurbText.color = color;
                __instance.RoleBlurbText.text = PlayerControl.LocalPlayer.GetRoleInfo();
            }
           else
            {
                CustomRoles role = PlayerControl.LocalPlayer.GetCustomRole();
                if (!(role.IsVanilla() && Options.DisableVanillaRoles.GetBool()))
                {
                    __instance.YouAreText.color = Utils.GetRoleColor(role);
                    __instance.RoleText.text = Utils.GetRoleName(role);
                    __instance.RoleText.color = Utils.GetRoleColor(role);
                    __instance.RoleText.fontWeight = TMPro.FontWeight.Thin;
                    __instance.RoleText.SetOutlineColor(Utils.ShadeColor(Utils.GetRoleColor(role), 0.1f).SetAlpha(0.38f));
                    __instance.RoleText.SetOutlineThickness(0.17f);
                    __instance.RoleBlurbText.color = Utils.GetRoleColor(role);
                    __instance.RoleBlurbText.text = PlayerControl.LocalPlayer.GetRoleInfo();
                }
                foreach (var subRole in PlayerState.GetByPlayerId(PlayerControl.LocalPlayer.PlayerId).SubRoles)
                    __instance.RoleBlurbText.text += "\n" + Utils.ColorString(Utils.GetRoleColor(subRole), GetString($"{subRole}Info"));
                Neptune.Intro(ref __instance);
                __instance.RoleText.text += Utils.GetSubRolesText(PlayerControl.LocalPlayer.PlayerId, false, true);
            }

        }, 0.0001f, "Override Role Text");
    }
    [HarmonyPatch(nameof(IntroCutscene.CoBegin)), HarmonyPrefix]
    public static void CoBegin_Prefix()
    {
        if (!Main.AssistivePluginMode.Value)
        {
            var logger = Logger.Handler("Info");
            Logger.Info("------------显示名称------------", "CoBegin");
            foreach (var pc in Main.AllPlayerControls)
            {
                Logger.Info($"{(pc.AmOwner ? "[*]" : ""),-3}{pc.PlayerId,-2}:{pc.name.PadRightV2(20)}:{pc.cosmetics.nameText.text}({Palette.ColorNames[pc.Data.DefaultOutfit.ColorId].ToString().Replace("Color", "")})", "CoBegin");
                pc.cosmetics.nameText.text = pc.name;
            }
            Logger.Info("------------职业分配------------", "CoBegin");
            foreach (var pc in Main.AllPlayerControls)
            {
                Logger.Info($"{(pc.AmOwner ? "[*]" : ""),-3}{pc.PlayerId,-2}:{pc?.Data?.PlayerName?.PadRightV2(20)}:{pc.GetAllRoleName().RemoveHtmlTags()}", "CoBegin");
            }
            Logger.Info("------------运行环境------------", "CoBegin");
            foreach (var pc in Main.AllPlayerControls)
            {
                try
                {
                    var text = pc.AmOwner ? "[*]" : "   ";
                    text += $"{pc.PlayerId,-2}:{pc.Data?.PlayerName?.PadRightV2(20)}:{pc.GetClient()?.PlatformData?.Platform.ToString()?.Replace("Standalone", ""),-11}";
                    if (Main.playerVersion.TryGetValue(pc.PlayerId, out PlayerVersion pv))
                        text += $":Mod({pv.forkId}/{pv.version}:{pv.tag})";
                    else text += ":Vanilla";
                    Logger.Info(text, "CoBegin");
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Platform");
                }
            }
            Logger.Info("------------基本设置------------", "CoBegin");
            var tmp = GameOptionsManager.Instance.CurrentGameOptions.ToHudString(GameData.Instance ? GameData.Instance.PlayerCount : 10).Split("\r\n").Skip(1);
            foreach (var t in tmp) Logger.Info(t, "CoBegin");
            Logger.Info("------------详细设置------------", "CoBegin");
            foreach (var o in OptionItem.AllOptions)
                if (!o.IsHiddenOn(Options.CurrentGameMode) && (o.Parent == null ? !o.GetString().Equals("0%") : o.Parent.GetBool()))
                    Logger.Info(
                        $"{(o.Parent == null ? o.GetName(true, true).RemoveHtmlTags().PadRightV2(40) : $"┗ {o.GetName(true, true).RemoveHtmlTags()}".PadRightV2(41))}:{o.GetString().RemoveHtmlTags()}"
                        , "CoBegin");
            Logger.Info("-------------其它信息-------------", "CoBegin");
            Logger.Info($"玩家人数: {Main.AllPlayerControls.Count()}", "CoBegin");
            Main.AllPlayerControls.Do(x => PlayerState.GetByPlayerId(x.PlayerId).InitTask(x));
            GameData.Instance.RecomputeTaskCounts();
            TaskState.InitialTotalTasks = GameData.Instance.TotalTasks;

            Utils.NotifyRoles();

            
        }
        GameStates.InGame = true;
    }
    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate)), HarmonyPrefix]
    public static bool BeginCrewmate_Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        if (Main.AssistivePluginMode.Value) return true;
        
            if (PlayerControl.LocalPlayer.Is(CustomRoles.CrewPostor))
            {
                teamToDisplay = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                teamToDisplay.Add(PlayerControl.LocalPlayer);
                foreach (var pc in Main.AllPlayerControls.Where(x => !x.AmOwner && x.GetCustomRole().IsImpostor())) teamToDisplay.Add(pc);
                __instance.BeginImpostor(teamToDisplay);
                __instance.overlayHandle.color = Palette.ImpostorRed;
                return false;
            }
            else if (PlayerControl.LocalPlayer.Is(CustomRoleTypes.Neutral))
            {
                teamToDisplay = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                teamToDisplay.Add(PlayerControl.LocalPlayer);
            }
            else if (PlayerControl.LocalPlayer.Is(CustomRoles.Madmate))
            {
                teamToDisplay = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                teamToDisplay.Add(PlayerControl.LocalPlayer);
                __instance.BeginImpostor(teamToDisplay);
                __instance.overlayHandle.color = Palette.ImpostorRed;
                return false;
            }
        
        return true;
    }
    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate)), HarmonyPostfix]
    public static void BeginCrewmate_Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
    {
        if (Main.AssistivePluginMode.Value)
        {

            __instance.TeamTitle.text = $"{GetString("TeamCrewmate")}";

            __instance.ImpostorText.text = $"{string.Format(GetString("ImpostorNumCrew"), GameOptionsManager.Instance.currentNormalGameOptions.NumImpostors)}";
            __instance.ImpostorText.text += "\n" + GetString("CrewmateIntroText");
            __instance.TeamTitle.color = new Color32(140, 255, 255, byte.MaxValue);


            return;
        }
            //チーム表示変更
            CustomRoles role = PlayerControl.LocalPlayer.GetCustomRole();
        __instance.ImpostorText.gameObject.SetActive(true);
        PlayerControl.LocalPlayer.Data.Role.IntroSound = null;
        PlayerControl.LocalPlayer.Data.Role.UseSound = GetIntroSound(RoleTypes.Impostor);
        if (Main.EnableRoleBackGround.Value)
        {
            switch (role.GetCustomRoleTypes())
            {
                case CustomRoleTypes.Impostor:
                    __instance.TeamTitle.text = GetString("TeamImpostor");
                    __instance.ImpostorText.text = GetString("ImpostorIntroText");
                    __instance.TeamTitle.color = __instance.BackgroundBar.material.color =  new Color32(255, 25, 25, byte.MaxValue);
                    break;
                case CustomRoleTypes.Crewmate:
                    if (PlayerControl.LocalPlayer.Is(CustomRoles.Madmate))
                    {
                        __instance.TeamTitle.text = GetString("TeamImpostor");
                        __instance.ImpostorText.text = GetString("ImpostorIntroText");
                        __instance.TeamTitle.color = __instance.BackgroundBar.material.color = new Color32(255, 25, 25, byte.MaxValue);
                        break;
                    }
                    __instance.TeamTitle.text = $"{GetString("TeamCrewmate")}";
                    __instance.ImpostorText.text = $"{string.Format(GetString("ImpostorNumCrew"), Options.SetImpNum.GetBool() ? Options.ImpNum.GetInt() : Main.RealOptionsData.GetInt(Int32OptionNames.NumImpostors))}";
                    __instance.ImpostorText.text += "\n" + GetString("CrewmateIntroText");
                    __instance.TeamTitle.color = new Color32(140, 255, 255, byte.MaxValue);
                    break;
                case CustomRoleTypes.Neutral:
                    
                        
                    if (!PlayerControl.LocalPlayer.IsNeutralEvil())
                    {
                        __instance.TeamTitle.text = GetString("TeamNNeutral");
                        __instance.ImpostorText.text = $"{string.Format(GetString("ImpostorNumNN"), Options.SetImpNum.GetBool() ? Options.ImpNum.GetInt() : Main.RealOptionsData.GetInt(Int32OptionNames.NumImpostors))}";
                        __instance.ImpostorText.text += "\n" + GetString("NNeutralIntroText");
                        __instance.TeamTitle.color =  new Color32(255, 254, 226, byte.MaxValue);
                       
                    }
                    else
                    {
                        __instance.TeamTitle.text = GetString("TeamIndependent");
                        __instance.ImpostorText.text = $"{string.Format(GetString("ImpostorNumEN"), Options.SetImpNum.GetBool() ? Options.ImpNum.GetInt() : Main.RealOptionsData.GetInt(Int32OptionNames.NumImpostors))}";
                        __instance.ImpostorText.text += "\n" + GetString("IndependentIntroText");
                        __instance.TeamTitle.color = new Color32(187, 186, 161, byte.MaxValue);
                    }
                    break;
            }
            __instance.BackgroundBar.material.color = Utils.GetRoleColor(PlayerControl.LocalPlayer.GetCustomRole());
        }
        if (PlayerControl.LocalPlayer.GetRoleClass()?.GetGameStartSound(out var newsound) ?? false)
        {
            if (Options.SubGameMode.GetInt() == 1)
                newsound = "GongXiFaCai";
            new LateTask(() =>
            {
                PlayerControl.LocalPlayer.RPCPlayCustomSound(newsound);
            }, 4f, "Sound");
        }
        else
        {
        switch (role.GetCustomRoleTypes())
        {
            case CustomRoleTypes.Impostor:
                if (PlayerControl.LocalPlayer.Is(RoleTypes.Shapeshifter))
                {
                    new LateTask(() =>
                    {
                        PlayerControl.LocalPlayer.RPCPlayCustomSound("Shapeshifter");
                    }, 3.8f, "Sound");
                    break;
                }
                else if(PlayerControl.LocalPlayer.Is(CustomRoles.Bard))
                {
                        PlayerControl.LocalPlayer.Data.Role.IntroSound = PlayerControl.LocalPlayer.KillSfx;
                        break;
                }
                else
                {
                        PlayerControl.LocalPlayer.Data.Role.IntroSound = PlayerControl.LocalPlayer.KillSfx;
                        break;
                }
            case CustomRoleTypes.Crewmate:
                PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Crewmate);
                break;
            case CustomRoleTypes.Neutral:
                if (PlayerControl.LocalPlayer.IsNeutralKiller())
                {
                    new LateTask(() =>
                    {
                        PlayerControl.LocalPlayer.Data.Role.IntroSound = PlayerControl.LocalPlayer.KillSfx;
                    }, 4f, "Sound");
                    break;
                }
                else if(PlayerControl.LocalPlayer.Is(CustomRoles.Sunnyboy))
                {
                    new LateTask(() =>
                    {
                        PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                    }, 4f, "Sound");
                    break;
                }
                else
                {

                   new LateTask(() =>
                    {
                        PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                    }, 4f, "Sound");  
                    break;
                } 
        }
        }
          

        switch (role)
        {
            case CustomRoles.GM:
                __instance.TeamTitle.text = Utils.GetRoleName(role);
                __instance.TeamTitle.color = Utils.GetRoleColor(role);
                __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                PlayerControl.LocalPlayer.Data.Role.IntroSound = DestroyableSingleton<HudManager>.Instance.TaskCompleteSound;
                break;
        }

        if (role.GetRoleInfo()?.IntroSound is AudioClip introSound)
        {
            PlayerControl.LocalPlayer.Data.Role.IntroSound = introSound;
        }

        if (PlayerControl.LocalPlayer.Is(CustomRoles.Madmate))
        {
            __instance.TeamTitle.text = GetString("TeamImpostor");
            __instance.TeamTitle.color = __instance.BackgroundBar.material.color = new Color32(255, 25, 25, byte.MaxValue);
            PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
        }
        if (Options.CurrentGameMode == CustomGameMode.HotPotato)
        {
            var color = ColorUtility.TryParseHtmlString("#ffa300", out var c) ? c : new(255, 255, 255, 255);
            __instance.TeamTitle.text = Utils.GetRoleName(role);
            __instance.TeamTitle.color = Utils.GetRoleColor(role);
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = GetString("ModeHotPotato");
            __instance.BackgroundBar.material.color = color;
            PlayerControl.LocalPlayer.Data.Role.IntroSound = PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
        }
        if (Options.CurrentGameMode == CustomGameMode.InfectorMode)
        {
            var color = ColorUtility.TryParseHtmlString("#ffa300", out var c) ? c : new(255, 255, 255, 255);
            __instance.TeamTitle.text = Utils.GetRoleName(role);
            __instance.TeamTitle.color = Utils.GetRoleColor(role);
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = GetString("ModeZombieMode");
            __instance.BackgroundBar.material.color = color;
            PlayerControl.LocalPlayer.Data.Role.IntroSound = PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
        }
        

        if (Input.GetKey(KeyCode.RightShift))
        {
            __instance.TeamTitle.text = "明天就跑路啦";
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = "嘿嘿嘿嘿嘿嘿";
            __instance.TeamTitle.color = Color.cyan;
            StartFadeIntro(__instance, Color.cyan, Color.yellow);
        }
        if (Input.GetKey(KeyCode.RightControl))
        {
            __instance.TeamTitle.text = "警告";
            __instance.ImpostorText.gameObject.SetActive(true);
            __instance.ImpostorText.text = "请远离无知的玩家";
            __instance.TeamTitle.color = Color.magenta;
            StartFadeIntro(__instance, Color.magenta, Color.magenta);
        }
    }
    public static AudioClip GetIntroSound(RoleTypes roleType)
    {
        return RoleManager.Instance.AllRoles.Where((role) => role.Role == roleType).FirstOrDefault().IntroSound;
    }
    private static async void StartFadeIntro(IntroCutscene __instance, Color start, Color end)
    {
        await Task.Delay(1000);
        int milliseconds = 0;
        while (true)
        {
            await Task.Delay(20);
            milliseconds += 20;
            float time = milliseconds / (float)500;
            Color LerpingColor = Color.Lerp(start, end, time);
            if (__instance == null || milliseconds > 500)
            {
                Logger.Info("ループを終了します", "StartFadeIntro");
                break;
            }
            __instance.BackgroundBar.material.color = LerpingColor;
        }
    }
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor)), HarmonyPrefix]
    public static bool BeginImpostor_Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        if (Main.AssistivePluginMode.Value) return true;

        var role = PlayerControl.LocalPlayer.GetCustomRole();
            if (role is CustomRoles.CrewPostor)
            {
                yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                yourTeam.Add(PlayerControl.LocalPlayer);
                foreach (var pc in Main.AllPlayerControls.Where(x => !x.AmOwner && x.GetCustomRole().IsImpostor())) yourTeam.Add(pc);
                __instance.overlayHandle.color = Palette.ImpostorRed;
                return true;
            }
            else if (PlayerControl.LocalPlayer.Is(CustomRoles.Madmate))
            {
                yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                yourTeam.Add(PlayerControl.LocalPlayer);
                __instance.overlayHandle.color = Palette.ImpostorRed;
                return true;
            }
            else if (role.IsCrewmate() && role.GetRoleInfo().IsDesyncImpostor)
            {
                yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                yourTeam.Add(PlayerControl.LocalPlayer);
                foreach (var pc in Main.AllPlayerControls.Where(x => !x.AmOwner)) yourTeam.Add(pc);
                __instance.BeginCrewmate(yourTeam);
                __instance.overlayHandle.color = Palette.CrewmateBlue;
                return false;
            }
            BeginCrewmate_Prefix(__instance, ref yourTeam);
        
            return true;
    }
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor)), HarmonyPostfix]
    public static void BeginImpostor_Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        if (Main.AssistivePluginMode.Value)
        {
            __instance.ImpostorText.gameObject.SetActive(true);

            __instance.TeamTitle.text = GetString("TeamImpostor");
            __instance.ImpostorText.text = GetString("ImpostorIntroText");
            __instance.TeamTitle.color = __instance.BackgroundBar.material.color = new Color32(255, 25, 25, byte.MaxValue);

            return;
        }

        BeginCrewmate_Postfix(__instance, ref yourTeam);
    }
    [HarmonyPatch(nameof(IntroCutscene.OnDestroy)), HarmonyPostfix]
    public static void OnDestroy_Postfix(IntroCutscene __instance)
    {
        if (Main.AssistivePluginMode.Value) return;
        if (!GameStates.IsInGame) return;

        Main.introDestroyed = true;

        var mapId = Main.NormalOptions.MapId;
        // エアシップではまだ湧かない
        if ((MapNames)mapId != MapNames.Airship)
        {
            foreach (var state in PlayerState.AllPlayerStates.Values)
            {
                state.HasSpawned = true;
            }
        }
        Main.introDestroyed = true;
        if (AmongUsClient.Instance.AmHost)
        {
            if (mapId != 4)
            {
                Main.AllPlayerControls.Do(pc => pc.RpcResetAbilityCooldown());
                if (Options.FixFirstKillCooldown.GetBool() && Options.CurrentGameMode != CustomGameMode.HotPotato && Options.CurrentGameMode != CustomGameMode.InfectorMode)
                    _ = new LateTask(() =>
                    {
                        if (GameStates.IsInTask)
                        {
                            Main.AllPlayerControls.Do(x => x.ResetKillCooldown());
                            Main.AllPlayerControls.Where(x => (Main.AllPlayerKillCooldown[x.PlayerId] - 2f) > 0f).Do(pc => pc.SetKillCooldownV2(Main.AllPlayerKillCooldown[pc.PlayerId] - 2f));
                        }
                    }, 2f, "FixKillCooldownTask");
                _ = new LateTask(() =>
                {
                    CustomRoleManager.AllActiveRoles.Values.Do(x => x?.OnGameStart());
                }, 0.1f, "RoleClassOnGameStartTask");
            }
            _ = new LateTask(() => Main.AllPlayerControls.Do(pc => pc.RpcSetRoleDesync(RoleTypes.Shapeshifter, false, -3)), 2f, "SetImpostorForServer");
            if (PlayerControl.LocalPlayer.Is(CustomRoles.GM))
            {
                PlayerControl.LocalPlayer.RpcExile();
                PlayerState.GetByPlayerId(PlayerControl.LocalPlayer.PlayerId).SetDead();
            }
            if (RandomSpawn.IsRandomSpawn())
            {
                RandomSpawn.SpawnMap map;
                switch (mapId)
                {
                    case 0:
                        map = new RandomSpawn.SkeldSpawnMap();
                        Main.AllPlayerControls.Do(map.RandomTeleport);
                        break;
                    case 1:
                        map = new RandomSpawn.MiraHQSpawnMap();
                        Main.AllPlayerControls.Do(map.RandomTeleport);
                        break;
                }
            }
            if (RandomSpawn.IsRandomSpawn() || Options.CurrentGameMode == CustomGameMode.HotPotato || Options.CurrentGameMode == CustomGameMode.InfectorMode)
            {
                RandomSpawn.SpawnMap map;
                switch (Main.NormalOptions.MapId)
                {
                    case 0:
                        map = new RandomSpawn.SkeldSpawnMap();
                        Main.AllPlayerControls.Do(map.RandomTeleport);
                        break;
                    case 1:
                        map = new RandomSpawn.MiraHQSpawnMap();
                        Main.AllPlayerControls.Do(map.RandomTeleport);
                        break;
                }
            }

            // そのままだとホストのみDesyncImpostorの暗室内での視界がクルー仕様になってしまう
            var roleInfo = PlayerControl.LocalPlayer.GetCustomRole().GetRoleInfo();
            var amDesyncImpostor = roleInfo?.IsDesyncImpostor == true;
            if (amDesyncImpostor)
            {
                PlayerControl.LocalPlayer.Data.Role.AffectedByLightAffectors = false;
            }
        }
        Logger.Info("OnDestroy", "IntroCutscene");
    }
}