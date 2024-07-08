using HarmonyLib;
using Il2CppInterop.Runtime;
using InnerNet;
using System.Drawing;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;

namespace TONEX;

public static class CustomButton
{
    public static Sprite GetSprite(string name) => Utils.LoadSprite($"TONEX.Resources.Images.Skills.{name}.png", 115f);
}

#nullable enable
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update)), HarmonyPriority(Priority.LowerThanNormal)]
class HudSpritePatch
{
    private static Sprite? Defalt_Kill => DestroyableSingleton<HudManager>.Instance?.KillButton?.graphic?.sprite;
    private static Sprite? Defalt_Ability => DestroyableSingleton<HudManager>.Instance?.AbilityButton?.graphic?.sprite;
    private static Sprite? Defalt_Vent => DestroyableSingleton<HudManager>.Instance?.ImpostorVentButton?.graphic?.sprite;
    private static Sprite? Defalt_Report => DestroyableSingleton<HudManager>.Instance?.ReportButton?.graphic?.sprite;
    private static Sprite? Defalt_Pet => DestroyableSingleton<HudManager>.Instance?.PetButton?.graphic?.sprite;
    private static Sprite? Defalt_Use => DestroyableSingleton<HudManager>.Instance?.UseButton?.graphic?.sprite;
    private static Sprite? Defalt_Admin => DestroyableSingleton<HudManager>.Instance?.AdminButton?.graphic?.sprite;
    private static Sprite? Defalt_Remain => DestroyableSingleton<HudManager>.Instance?.AbilityButton?.usesRemainingSprite?.sprite;
    private static Sprite? Defalt_Set => DestroyableSingleton<HudManager>.Instance?.SettingsButton?.GetComponent<Sprite>();
    private static Sprite? Defalt_Map => DestroyableSingleton<HudManager>.Instance?.MapButton?.HeldButtonSprite?.sprite;
    private static Sprite? Defalt_Chat => DestroyableSingleton<HudManager>.Instance?.Chat?.chatButton?.HeldButtonSprite?.sprite;
    public static void Postfix(HudManager __instance)
    {
        var player = PlayerControl.LocalPlayer;
        if (__instance == null || player == null || !GameStates.IsModHost) return;
        if (!SetHudActivePatch.IsActive || !player.IsAlive()) return;


        Sprite newKillButton = Defalt_Kill ?? __instance.KillButton.graphic.sprite;
        Sprite newAbilityButton = Defalt_Ability ?? __instance.AbilityButton.graphic.sprite;
        Sprite newVentButton = Defalt_Vent ?? __instance.ImpostorVentButton.graphic.sprite;
        Sprite newReportButton = Defalt_Report ?? __instance.ReportButton.graphic.sprite;
        Sprite newPetButton = Defalt_Pet ?? __instance.PetButton.graphic.sprite;
        Sprite newRemain = Defalt_Remain ?? __instance.AbilityButton.usesRemainingSprite.sprite;
        Sprite newUseButton = Defalt_Use ?? __instance.UseButton.graphic.sprite;
        Sprite newAdminButton = Defalt_Admin ?? __instance.AdminButton.graphic.sprite;
        //Image newSetting = Defalt_Set ?? __instance.SettingsButton;
        //Sprite newMap = Defalt_Map ?? __instance.MapButton.HeldButtonSprite.sprite;
        //Sprite newChat = Defalt_Chat ?? __instance.Chat.chatButton.HeldButtonSprite.sprite;

        if (Main.EnableCustomButton.Value)
        {
            #region 内鬼基础职业专用
            if (player.GetRoleClass() is IKiller killer)
            {
                if (killer.OverrideKillButtonSprite(out var newKillButtonName))
                    newKillButton = CustomButton.GetSprite(newKillButtonName);
                
                


                if (killer.OverrideVentButtonSprite(out var newVentButtonName))
                    newVentButton = CustomButton.GetSprite(newVentButtonName);
                
            }
            if (__instance.KillButton.graphic.sprite != newKillButton && newKillButton != null)
            {
                __instance.KillButton.graphic.sprite = newKillButton;
                __instance.KillButton.graphic.material = __instance.ReportButton.graphic.material;
            }
            if (__instance.ImpostorVentButton.graphic.sprite != newVentButton && newVentButton != null)
            {
                __instance.ImpostorVentButton.graphic.sprite = newVentButton;
            }
            __instance.KillButton?.graphic?.material?.SetFloat("_Desat", __instance?.KillButton?.isCoolingDown ?? true ? 1f : 0f);
            #endregion


            #region 技能
            if (player.GetRoleClass()?.GetAbilityButtonSprite(out var newAbilityButtonName) ?? false)
            {
                newAbilityButton = CustomButton.GetSprite(newAbilityButtonName);
                newRemain = CustomButton.GetSprite("UseNum");
            }
            if (__instance.AbilityButton.graphic.sprite != newAbilityButton && newAbilityButton != null)
            {
                __instance.AbilityButton.graphic.sprite = newAbilityButton;
                __instance.AbilityButton.graphic.material = __instance.ReportButton.graphic.material;
            }
            if (__instance.AbilityButton.usesRemainingSprite.sprite != newRemain && newRemain != null)
            {
                __instance.AbilityButton.usesRemainingSprite.sprite = newRemain;
            }
            __instance.AbilityButton?.graphic?.material?.SetFloat("_Desat", __instance?.AbilityButton?.isCoolingDown ?? true ? 1f : 0f);
            #endregion


            #region 报告
            if (player.GetRoleClass()?.GetReportButtonSprite(out var newReportButtonName) ?? false)
            {
                newReportButton = CustomButton.GetSprite(newReportButtonName);
                
            }
            if (__instance.ReportButton.graphic.sprite != newReportButton && newReportButton != null)
            {
                __instance.ReportButton.graphic.sprite = newReportButton;
            }
            #endregion


            #region 宠物
            if (player.GetRoleClass()?.GetPetButtonSprite(out var newPetButtonName) ?? false && Options.UsePets.GetBool())
            {
                newPetButton = CustomButton.GetSprite(newPetButtonName);
                
            }
            if (__instance.PetButton.graphic.sprite != newPetButton && newPetButton != null)
            {
                __instance.PetButton.graphic.sprite = newPetButton;
            }
            __instance.PetButton?.graphic?.material?.SetFloat("_Desat", player.GetRoleClass()?.PetUnSet() ?? false ? 0f: 1f);
            #endregion


            #region 使用
            if (player.GetRoleClass()?.GetUseButtonSprite(out var newUseButtonName) ?? false)
            {
                newUseButton = CustomButton.GetSprite(newUseButtonName);
                if (__instance.UseButton.graphic.sprite != newUseButton && newUseButton != null)
                {
                    __instance.UseButton.graphic.sprite = newUseButton;
                }
            }
            #endregion


            #region 管理
            if (player.GetRoleClass()?.GetAdminButtonSprite(out var newAdminButtonName) ?? false)
            {
                newAdminButton = CustomButton.GetSprite(newAdminButtonName);
                if (__instance.AdminButton.graphic.sprite != newAdminButton && newAdminButton != null)
                {
                    __instance.AdminButton.graphic.sprite = newAdminButton;
                }
            }
            #endregion


            #region 设置
            //try
            //{
            //    Sprite newSettingButton = CustomButton.GetSprite("SettingButton");
            //    newSetting = newSettingButton;
            //}
            //catch { }
            //try
            //{
            //    if (__instance.SettingsButton != newSetting && newSetting != null)
            //    {
            //        var ns = __instance.SettingsButton.GetComponent<Sprite>();
            //        ns = newSetting;
            //    }
            //}
            //catch { }
            #endregion


            #region 聊天
            //try
            //{
            //    newChat = CustomButton.GetSprite("ChatLobby");
            //    if (GameStates.IsInGame)
            //    {
            //        switch (player.GetCustomRole().GetCustomRoleTypes())
            //        {
            //            case CustomRoleTypes.Crewmate:
            //                newChat = CustomButton.GetSprite("ChatCrew");
            //                break;
            //            case CustomRoleTypes.Impostor:
            //                newChat = CustomButton.GetSprite("ChatImp");
            //                break;
            //            case CustomRoleTypes.Neutral:
            //                if (!player.IsNeutralEvil())
            //                    newChat = CustomButton.GetSprite("ChatN");
            //                else
            //                    newChat = CustomButton.GetSprite("ChatEvilN");
            //                break;
            //        }
            //    }
            //}
            //catch { }
            //try
            //{
            //    if (__instance.Chat.chatButton != newChat && newChat != null)
            //    {
            //        __instance.Chat.chatButton.HeldButtonSprite.sprite = newChat;
            //    }
            //}
            //catch { }
            #endregion


            #region 地图
            //try
            //{
            //    newMap = CustomButton.GetSprite("mapJourne");
            //    switch (Main.NormalOptions.MapId)
            //    {
            //        case 0:
            //            newMap = CustomButton.GetSprite("mapJourney_icon");
            //            break;
            //        case 1:
            //            newMap = CustomButton.GetSprite("mapMIRA_icon");
            //            break;
            //        case 2:
            //            newMap = CustomButton.GetSprite("mapPolus_icon");
            //            break;
            //        case 4:
            //            newMap = CustomButton.GetSprite("mapAirship_icon");
            //            break;
            //        case 5:
            //            newMap = CustomButton.GetSprite("theFungle_circleIcon");
            //            break;

            //    }
            //}
            //catch { }
            //try
            //{
            //    if (__instance.MapButton != newMap && newMap != null)
            //    {
            //        __instance.MapButton.HeldButtonSprite.sprite = newMap;
            //    }
            //}
            //catch { }
            #endregion
        }
    }

    
}
#nullable disable