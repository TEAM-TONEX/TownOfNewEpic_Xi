using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TONEX.Modules.OptionItems;
using TONEX.Modules.OptionItems.Interfaces;
using static TONEX.Translator;
using Object = UnityEngine.Object;

namespace TONEX;
[HarmonyPatch(typeof(GameSettingMenu))]
public static class GameSettingMenuPatch
{
    public static GameOptionsMenu tonexSettingsTab;
    public static PassiveButton tonexSettingsButton;
    public static CategoryHeaderMasked SystemCategoryHeader { get; private set; }
    public static CategoryHeaderMasked ModCategoryHeader { get; private set; }
    public static CategoryHeaderMasked ImpostorRoleCategoryHeader { get; private set; }
    public static CategoryHeaderMasked CrewmateRoleCategoryHeader { get; private set; }
    public static CategoryHeaderMasked NeutralRoleCategoryHeader { get; private set; }
    public static CategoryHeaderMasked AddOnCategoryHeader { get; private set; }
    public static CategoryHeaderMasked OtherRoleCategoryHeader { get; private set; }

    [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPostfix]
    public static void StartPostfix(GameSettingMenu __instance)
    {
        tonexSettingsTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        tonexSettingsTab.name = TONEXMenuName;
        var vanillaOptions = tonexSettingsTab.GetComponentsInChildren<OptionBehaviour>();
        foreach (var vanillaOption in vanillaOptions)
        {
            Object.Destroy(vanillaOption.gameObject);
        }

        // TONEX設定ボタンのスペースを作るため，左側の要素を上に詰める
        var gameSettingsLabel = __instance.transform.Find("GameSettingsLabel");
        if (gameSettingsLabel)
        {
            gameSettingsLabel.localPosition += Vector3.up * 0.2f;
        }
        __instance.MenuDescriptionText.transform.parent.localPosition += Vector3.up * 0.4f;
        __instance.GamePresetsButton.transform.parent.localPosition += Vector3.up * 0.5f;

        // TONEX設定ボタン
        tonexSettingsButton = Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);
        tonexSettingsButton.name = "TONEXSettingsButton";
        tonexSettingsButton.transform.localPosition = __instance.RoleSettingsButton.transform.localPosition + (__instance.RoleSettingsButton.transform.localPosition - __instance.GameSettingsButton.transform.localPosition);
        tonexSettingsButton.buttonText.DestroyTranslator();
        tonexSettingsButton.buttonText.text = GetString("TONEXSettingsButtonLabel");
        var activeSprite = tonexSettingsButton.activeSprites.GetComponent<SpriteRenderer>();
        var selectedSprite = tonexSettingsButton.selectedSprites.GetComponent<SpriteRenderer>();
        activeSprite.color = selectedSprite.color = Main.ModColor32;
        tonexSettingsButton.OnClick.AddListener((Action)(() =>
        {
            __instance.ChangeTab(-1, false);  // バニラタブを閉じる
            tonexSettingsTab.gameObject.SetActive(true);
            __instance.MenuDescriptionText.text = GetString("TONEXSettingsDescription");
            tonexSettingsButton.SelectButton(true);
        }));

        // 各カテゴリの見出しを作成
        SystemCategoryHeader = CreateCategoryHeader(__instance, tonexSettingsTab, TabGroup.SystemSettings);
        ModCategoryHeader = CreateCategoryHeader(__instance, tonexSettingsTab, TabGroup.ModSettings);
        ImpostorRoleCategoryHeader = CreateCategoryHeader(__instance, tonexSettingsTab, TabGroup.ImpostorRoles);
        CrewmateRoleCategoryHeader = CreateCategoryHeader(__instance, tonexSettingsTab, TabGroup.CrewmateRoles);
        NeutralRoleCategoryHeader = CreateCategoryHeader(__instance, tonexSettingsTab, TabGroup.NeutralRoles);
        AddOnCategoryHeader = CreateCategoryHeader(__instance, tonexSettingsTab, TabGroup.Addons);
        OtherRoleCategoryHeader = CreateCategoryHeader(__instance, tonexSettingsTab, TabGroup.OtherRoles);

        // 各設定スイッチを作成
        var template = __instance.GameSettingsTab.stringOptionOrigin;
        var scOptions = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
        foreach (var option in OptionItem.AllOptions)
        {
            if (option.OptionBehaviour == null)
            {
                var stringOption = Object.Instantiate(template, tonexSettingsTab.settingsContainer);
                scOptions.Add(stringOption);
                stringOption.SetClickMask(__instance.GameSettingsButton.ClickMask);
                stringOption.SetUpFromData(stringOption.data, GameOptionsMenu.MASK_LAYER);
                
                stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
              
                if (option is TextOptionItem)
                {
                    stringOption.ValueText.text = option.Name;
                    stringOption.ValueText.color = option.NameColor;


                    stringOption.buttons.Do(x => x.gameObject.SetActive(false));
                    stringOption.TitleText.gameObject.SetActive(false);
                }
                else
                {
                    stringOption.Value = stringOption.oldValue = option.CurrentValue;
                    stringOption.ValueText.text = option.GetString();
                    stringOption.TitleText.text = option.Name;
                }
                stringOption.name = option.Name;

                // タイトルの枠をデカくする
                var indent = 0f;  // 親オプションがある場合枠の左を削ってインデントに見せる
                var parent = option.Parent;
                while (parent != null)
                {
                    indent += 0.15f;
                    parent = parent.Parent;
                }
                stringOption.LabelBackground.size += new Vector2(2f - indent * 2, 0f);
                stringOption.LabelBackground.transform.localPosition += new Vector3(-1f + indent, 0f, 0f);
                stringOption.TitleText.rectTransform.sizeDelta += new Vector2(2f - indent * 2, 0f);
                stringOption.TitleText.transform.localPosition += new Vector3(-1f + indent, 0f, 0f);

                option.OptionBehaviour = stringOption;
            }
            option.OptionBehaviour.gameObject.SetActive(true);
        }
        tonexSettingsTab.Children = scOptions;
        tonexSettingsTab.gameObject.SetActive(false);

        // 各カテゴリまでスクロールするボタンを作成
        var jumpButtonY = -0.55f;
        var jumpToSystemButton = CreateJumpToCategoryButton(__instance, tonexSettingsTab, "TONEX.Resources.Images.TabIcon_SystemSettings.png", ref jumpButtonY, SystemCategoryHeader);
        var jumpToModButton = CreateJumpToCategoryButton(__instance, tonexSettingsTab, "TONEX.Resources.Images.TabIcon_ModSettings.png", ref jumpButtonY, ModCategoryHeader);
        var jumpToImpButton = CreateJumpToCategoryButton(__instance, tonexSettingsTab, "TONEX.Resources.Images.TabIcon_ImpostorRoles.png", ref jumpButtonY, ImpostorRoleCategoryHeader);
        var jumpToCrewButton = CreateJumpToCategoryButton(__instance, tonexSettingsTab, "TONEX.Resources.Images.TabIcon_CrewmateRoles.png", ref jumpButtonY, CrewmateRoleCategoryHeader);
        var jumpToNeutralButton = CreateJumpToCategoryButton(__instance, tonexSettingsTab, "TONEX.Resources.Images.TabIcon_NeutralRoles.png", ref jumpButtonY, NeutralRoleCategoryHeader);
        var jumpToAddOnButton = CreateJumpToCategoryButton(__instance, tonexSettingsTab, "TONEX.Resources.Images.TabIcon_Addons.png", ref jumpButtonY, AddOnCategoryHeader);
        var jumpToOtherButton = CreateJumpToCategoryButton(__instance, tonexSettingsTab, "TONEX.Resources.Images.TabIcon_OtherRoles.png", ref jumpButtonY, OtherRoleCategoryHeader);
    }
    private static MapSelectButton CreateJumpToCategoryButton(GameSettingMenu __instance, GameOptionsMenu tonexTab, string resourcePath, ref float localY, CategoryHeaderMasked jumpTo)
    {
        var image = Utils.LoadSprite(resourcePath, 100f);
        var button = Object.Instantiate(__instance.GameSettingsTab.MapPicker.MapButtonOrigin, Vector3.zero, Quaternion.identity, tonexTab.transform);
        button.SetImage(image, GameOptionsMenu.MASK_LAYER);
        button.transform.localPosition = new(7.1f, localY, -10f);
        button.transform.localScale *= 0.9f;
        button.Button.ClickMask = tonexTab.ButtonClickMask;
        button.Button.OnClick.AddListener((Action)(() =>
        {
            tonexTab.scrollBar.velocity = Vector2.zero;  // ドラッグの慣性によるスクロールを止める
            var relativePosition = tonexTab.scrollBar.transform.InverseTransformPoint(jumpTo.transform.position);  // Scrollerのローカル空間における座標に変換
            var scrollAmount = CategoryJumpY - relativePosition.y;
            tonexTab.scrollBar.Inner.localPosition = tonexTab.scrollBar.Inner.localPosition + Vector3.up * scrollAmount;  // 強制スクロール
            tonexTab.scrollBar.ScrollRelative(Vector2.zero);  // スクロール範囲内に収め，スクロールバーを更新する
        }));
        button.Button.activeSprites.transform.GetChild(0).gameObject.SetActive(false);  // チェックボックスを消す
        localY -= JumpButtonSpacing;
        return button;
    }
    private const float JumpButtonSpacing = 0.55f;
    // ジャンプしたカテゴリヘッダのScrollerとの相対Y座標がこの値になる
    private const float CategoryJumpY = 2f;
    private static Color GetTabColor(TabGroup tab)
    {
        return tab switch
        {
            TabGroup.SystemSettings => Main.ModColor32,
            TabGroup.ModSettings => new Color32(89, 239, 131, 255),
            TabGroup.ImpostorRoles => Utils.GetCustomRoleTypeColor(Roles.Core.CustomRoleTypes.Impostor),
            TabGroup.CrewmateRoles => Utils.GetCustomRoleTypeColor(Roles.Core.CustomRoleTypes.Crewmate),
            TabGroup.NeutralRoles => Utils.GetCustomRoleTypeColor(Roles.Core.CustomRoleTypes.Neutral),
            TabGroup.Addons => Utils.GetCustomRoleTypeColor(Roles.Core.CustomRoleTypes.Addon),
            TabGroup.OtherRoles => new Color32(118, 184, 224, 255),
            _ => Color.white,
        };
    }
    private static CategoryHeaderMasked CreateCategoryHeader(GameSettingMenu __instance, GameOptionsMenu tonexTab, TabGroup translationKey)
    {
        var categoryHeader = Object.Instantiate(__instance.GameSettingsTab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, tonexTab.settingsContainer);
        categoryHeader.name = $"TabGroup.{translationKey}";
        
        categoryHeader.Title.text = GetString(categoryHeader.name);
        categoryHeader.Title.color = GetTabColor(translationKey);
        var maskLayer = GameOptionsMenu.MASK_LAYER;
        categoryHeader.Background.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        if (categoryHeader.Divider != null)
        {
            categoryHeader.Divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        }
        categoryHeader.Title.fontMaterial.SetFloat("_StencilComp", 3f);
        categoryHeader.Title.fontMaterial.SetFloat("_Stencil", (float)maskLayer);
        categoryHeader.transform.localScale = Vector3.one * GameOptionsMenu.HEADER_SCALE;
        return categoryHeader;
    }
    private static CategoryHeaderMasked CreateCategoryHeaderForTextOptionItem(GameSettingMenu __instance, GameOptionsMenu tonexTab, OptionItem option)
    {
        var categoryHeader = Object.Instantiate(__instance.GameSettingsTab.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, tonexTab.settingsContainer);
        categoryHeader.name = option.GetName();
        categoryHeader.Title.text = GetString(categoryHeader.name);
        var maskLayer = GameOptionsMenu.MASK_LAYER;
        categoryHeader.Background.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        if (categoryHeader.Divider != null)
        {
            categoryHeader.Divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        }
        categoryHeader.Title.fontMaterial.SetFloat("_StencilComp", 3f);
        categoryHeader.Title.fontMaterial.SetFloat("_Stencil", (float)maskLayer);
        categoryHeader.transform.localScale = Vector3.one * GameOptionsMenu.HEADER_SCALE;
        return categoryHeader;
    }

    // 初めてロール設定を表示したときに発生する例外(バニラバグ)の影響を回避するためPrefix
    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab)), HarmonyPrefix]
    public static void ChangeTabPrefix(bool previewOnly)
    {
        if (!previewOnly)
        {
            if (tonexSettingsTab)
            {
                tonexSettingsTab.gameObject.SetActive(false);
            }
            if (tonexSettingsButton)
            {
                tonexSettingsButton.SelectButton(false);
            }
        }
    }

    public const string TONEXMenuName = "TownOfNewEpicTab";
}


[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Initialize))]
public static class GameOptionsMenuInitializePatch
{ 
    public static void Postfix(GameOptionsMenu __instance)
    {
        foreach (var ob in __instance.Children)
        {
            switch (ob.Title)
            {
                case StringNames.GameVotingTime:
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 600);
                    break;
                case StringNames.GameShortTasks:
                case StringNames.GameLongTasks:
                case StringNames.GameCommonTasks:
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 99);
                    break;
                case StringNames.GameKillCooldown:
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 180);
                    break;
                case StringNames.GameNumImpostors:
                    if (DebugModeManager.IsDebugMode)
                    {
                        ob.Cast<NumberOption>().ValidRange.min = 0;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
public class GameOptionsMenuUpdatePatch
{
    private static float _timer = 1f;

    public static void Postfix(GameOptionsMenu __instance)
    {
        if (__instance.name != GameSettingMenuPatch.TONEXMenuName) return;
        _timer += Time.deltaTime;
        if (_timer < 0.1f) return;
        _timer = 0f;
        var offset = 2.7f;
        UpdateCategoryHeader(GameSettingMenuPatch.SystemCategoryHeader, ref offset);
        foreach (var option in OptionItem.SystemOptions)
        {
            UpdateOption(option, ref offset);
        }
        UpdateCategoryHeader(GameSettingMenuPatch.ModCategoryHeader, ref offset);
        foreach (var option in OptionItem.ModOptions)
        {
            UpdateOption( option, ref offset);
        }
        UpdateCategoryHeader(GameSettingMenuPatch.ImpostorRoleCategoryHeader, ref offset);
        foreach (var option in OptionItem.ImpostorRoleOptions)
        {
            UpdateOption( option, ref offset);
        }
        UpdateCategoryHeader(GameSettingMenuPatch.CrewmateRoleCategoryHeader, ref offset);
        foreach (var option in OptionItem.CrewmateRoleOptions)
        {
            UpdateOption(option, ref offset);
        }
        UpdateCategoryHeader(GameSettingMenuPatch.NeutralRoleCategoryHeader, ref offset);
        foreach (var option in OptionItem.NeutralRoleOptions)
        {
            UpdateOption(option, ref offset);
        }
        UpdateCategoryHeader(GameSettingMenuPatch.AddOnCategoryHeader, ref offset);
        foreach (var option in OptionItem.AddOnOptions)
        {
            UpdateOption( option, ref offset);
        }
        UpdateCategoryHeader(GameSettingMenuPatch.OtherRoleCategoryHeader, ref offset);
        foreach (var option in OptionItem.OtherRoles)
        {
            UpdateOption(option, ref offset);
        }
        __instance.scrollBar.ContentYBounds.max = (-offset) - 1.5f;

    }
    private static void UpdateCategoryHeader(CategoryHeaderMasked categoryHeader, ref float offset)
    {
        offset -= GameOptionsMenu.HEADER_HEIGHT;
        categoryHeader.transform.localPosition = new(GameOptionsMenu.HEADER_X, offset, -2f);
    }
    private static void UpdateOption(OptionItem item, ref float offset)
    {
        if (item?.OptionBehaviour == null || item.OptionBehaviour.gameObject == null) return;
        var enabled = true;
        var parent = item.Parent;
        // 親オプションの値を見て表示するか決める
        enabled = AmongUsClient.Instance.AmHost && !item.IsHiddenOn(Options.CurrentGameMode);
        var stringOption = item.OptionBehaviour;
        while (parent != null && enabled)
        {
            enabled = parent.GetBool();
            parent = parent.Parent;
        }
        item.OptionBehaviour.gameObject.SetActive(enabled);
        if (enabled)
        {
            // 見やすさのため交互に色を変える
            stringOption.LabelBackground.color = item is IRoleOptionItem roleOption ? roleOption.RoleColor : new Color32(16, 16, 16, 0);
            offset -= GameOptionsMenu.SPACING_Y;
            if (item.IsHeader)
            {
                // IsHeaderなら隙間を広くする
                offset -= HeaderSpacingY;
            }

            item.OptionBehaviour.transform.localPosition = new Vector3(
                   GameOptionsMenu.START_POS_X,
                   offset,
                   -2f);
        }
    }

    private const float HeaderSpacingY = 0.2f;
}



[HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
public class StringOptionInitializePatch
{
    public static bool Prefix(StringOption __instance)
    {
        var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
        if (option == null) return true;

        __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
        __instance.TitleText.text = option.GetName(option is RoleSpawnChanceOptionItem);
        __instance.Value = __instance.oldValue = option.CurrentValue;
        __instance.ValueText.text = option.GetString();

        return false;
    }
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
public class StringOptionIncreasePatch
{
    public static bool Prefix(StringOption __instance)
    {
        var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
        if (option == null) return true;

        option.SetValue(option.CurrentValue + (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5 : 1));
        return false;
    }
    public static void Postfix(StringOption __instance) => OptionShower.BuildText();
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
public class StringOptionDecreasePatch
{
    public static bool Prefix(StringOption __instance)
    {
        var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
        if (option == null) return true;

        option.SetValue(option.CurrentValue - (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5 : 1));
        return false;
    }
    public static void Postfix(StringOption __instance) => OptionShower.BuildText();
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public class RpcSyncSettingsPatch
{
    public static void Postfix()
    {
        if (Main.AssistivePluginMode.Value) return;
        OptionItem.SyncAllOptions();
    }
}
[HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.Start))]
public static class RolesSettingsMenuPatch
{
    public static void Postfix(RolesSettingsMenu __instance)
    {
        foreach (var ob in __instance.advancedSettingChildren)
        {
            switch (ob.Title)
            {
                case StringNames.EngineerCooldown:
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 180);
                    break;
                case StringNames.ShapeshifterCooldown:
                    ob.Cast<NumberOption>().ValidRange = new FloatRange(0, 180);
                    break;
                default:
                    break;
            }
        }
    }
}
[HarmonyPatch(typeof(NormalGameOptionsV08), nameof(NormalGameOptionsV08.SetRecommendations), [typeof(int), typeof(bool), typeof(RulesPresets)])]
public static class SetRecommendationsPatch
{
    public static bool Prefix(NormalGameOptionsV08 __instance, int numPlayers, bool isOnline, RulesPresets rulesPresets)
    {
        switch (rulesPresets)
        {
            case RulesPresets.Standard: SetStandardRecommendations(__instance, numPlayers, isOnline); return false;
            // スタンダード以外のプリセットは一旦そのままにしておく
            default: return true;
        }
    }
    private static void SetStandardRecommendations(NormalGameOptionsV08 __instance, int numPlayers, bool isOnline)
    {
        numPlayers = Mathf.Clamp(numPlayers, 4, 15);
        __instance.PlayerSpeedMod = __instance.MapId == 4 ? 1.5f : 1.25f;
        __instance.CrewLightMod = 1.0f;
        __instance.ImpostorLightMod = 1.75f;
        __instance.KillCooldown = 27.5f;
        __instance.NumCommonTasks = 2;
        __instance.NumLongTasks = 1;
        __instance.NumShortTasks = 2;
        __instance.NumEmergencyMeetings = 3;
        if (!isOnline)
            __instance.NumImpostors = NormalGameOptionsV08.RecommendedImpostors[numPlayers];
        __instance.KillDistance = 0;
        __instance.DiscussionTime = 0;
        __instance.VotingTime = 120;
        __instance.IsDefaults = true;
        __instance.ConfirmImpostor = false;
        __instance.VisualTasks = false;

        __instance.roleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
        __instance.roleOptions.SetRoleRate(RoleTypes.Phantom, 0, 0);
        __instance.roleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
        __instance.roleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
        __instance.roleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
        __instance.roleOptions.SetRoleRate(RoleTypes.Noisemaker, 0, 0);
        __instance.roleOptions.SetRoleRate(RoleTypes.Tracker, 0, 0);
        __instance.roleOptions.SetRoleRecommended(RoleTypes.Shapeshifter);
        __instance.roleOptions.SetRoleRecommended(RoleTypes.Phantom);
        __instance.roleOptions.SetRoleRecommended(RoleTypes.Scientist);
        __instance.roleOptions.SetRoleRecommended(RoleTypes.GuardianAngel);
        __instance.roleOptions.SetRoleRecommended(RoleTypes.Engineer);
        __instance.roleOptions.SetRoleRecommended(RoleTypes.Noisemaker);
        __instance.roleOptions.SetRoleRecommended(RoleTypes.Tracker);
        if (Options.CurrentGameMode == CustomGameMode.HotPotato) //HotPotato
        {
            __instance.PlayerSpeedMod = 1.75f;
            __instance.CrewLightMod = 1f;
            __instance.ImpostorLightMod = 1f;
            __instance.NumImpostors = 3;
            __instance.NumCommonTasks = 0;
            __instance.NumLongTasks = 0;
            __instance.NumShortTasks = 0;
            __instance.KillCooldown = 10f;
        }
    }
}
