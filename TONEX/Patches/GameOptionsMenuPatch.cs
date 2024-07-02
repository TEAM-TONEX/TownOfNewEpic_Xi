using System.Linq;
using UnityEngine;
using static TONEX.Translator;
using Object = UnityEngine.Object;
using System;
using Il2CppSystem.Collections.Generic;
using HarmonyLib;
using static UnityEngine.RemoteConfigSettingsHelper;
using AmongUs.GameOptions;
using InnerNet;
using TMPro;

namespace TONEX;


public static class ModGameOptionsMenu
{
    public static int TabIndex = 0;
    public static Dictionary<OptionBehaviour, int> OptionList = new();
    public static Dictionary<int, OptionBehaviour> BehaviourList = new();
    public static Dictionary<int, CategoryHeaderMasked> CategoryHeaderList = new();
}
[HarmonyPatch(typeof(GameOptionsMenu))]
public static class GameOptionsMenuPatch
{
    [HarmonyPatch(nameof(GameOptionsMenu.Initialize)), HarmonyPrefix]
    private static bool InitializePrefix(GameOptionsMenu __instance)
    {
        if (ModGameOptionsMenu.TabIndex < 3) return true;

        if (__instance.Children == null || __instance.Children.Count == 0)
        {
            __instance.MapPicker.gameObject.SetActive(false);
            //__instance.MapPicker.Initialize(20);
            //BaseGameSetting mapNameSetting = GameManager.Instance.GameSettingsList.MapNameSetting;
            //__instance.MapPicker.SetUpFromData(mapNameSetting, 20);
            __instance.Children = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
            //__instance.Children.Add(__instance.MapPicker);
            __instance.CreateSettings();
            __instance.cachedData = GameOptionsManager.Instance.CurrentGameOptions;
            for (int i = 0; i < __instance.Children.Count; i++)
            {
                OptionBehaviour optionBehaviour = __instance.Children[i];
                optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
                //if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
                //{
                //    optionBehaviour.SetAsPlayer();
                //}
            }
            __instance.InitializeControllerNavigation();
        }

        return false;
    }
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings)), HarmonyPrefix]
    private static bool CreateSettingsPrefix(GameOptionsMenu __instance)
    {
        if (ModGameOptionsMenu.TabIndex < 3) return true;
        var modTab = (TabGroup)(ModGameOptionsMenu.TabIndex - 3);

        //float num = 0.713f;
        float num = 2.0f;
        const float pos_x = 0.952f;
        const float pos_z = -2.0f;
        for (int index = 0; index < OptionItem.AllOptions.Count; index++)
        {
            var option = OptionItem.AllOptions[index];
            if (option.Tab != modTab) continue;

            var enabled = !option.IsHiddenOn(Options.CurrentGameMode)
                         && (option.Parent == null || (!option.Parent.IsHiddenOn(Options.CurrentGameMode) && option.Parent.GetBool()));

            if (option.IsHeader || option is TextOptionItem)
            {
                CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(__instance.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                categoryHeaderMasked.SetHeader(StringNames.RolesCategory, 20);
                categoryHeaderMasked.Title.text = option.GetName();
                categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, pos_z);
                categoryHeaderMasked.transform.FindChild("HeaderText").GetComponent<TMPro.TextMeshPro>().fontStyle = TMPro.FontStyles.Bold;
                categoryHeaderMasked.transform.FindChild("HeaderText").GetComponent<TMPro.TextMeshPro>().outlineWidth = 0.17f;
                categoryHeaderMasked.gameObject.SetActive(enabled);
                ModGameOptionsMenu.CategoryHeaderList.TryAdd(index, categoryHeaderMasked);

                if (enabled) num -= 0.63f;
            }
            if (option is TextOptionItem) continue;

            var baseGameSetting = GetSetting(option);
            if (baseGameSetting == null) continue;


            OptionBehaviour optionBehaviour;

            switch (baseGameSetting.Type)
            {
                case OptionTypes.Checkbox:
                    {
                        optionBehaviour = UnityEngine.Object.Instantiate<ToggleOption>(__instance.checkboxOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                        optionBehaviour.transform.localPosition = new Vector3(pos_x, num, pos_z);

                        OptionBehaviourSetSizeAndPosition(optionBehaviour, option, baseGameSetting.Type);

                        optionBehaviour.SetClickMask(__instance.ButtonClickMask);
                        optionBehaviour.SetUpFromData(baseGameSetting, 20);
                        ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
                        //Logger.Info($"{option.Name}, {index}", "OptionList.TryAdd");
                        break;
                    }
                case OptionTypes.String:
                    {
                        optionBehaviour = UnityEngine.Object.Instantiate<StringOption>(__instance.stringOptionOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                        optionBehaviour.transform.localPosition = new Vector3(pos_x, num, pos_z);

                        OptionBehaviourSetSizeAndPosition(optionBehaviour, option, baseGameSetting.Type);

                        optionBehaviour.SetClickMask(__instance.ButtonClickMask);
                        optionBehaviour.SetUpFromData(baseGameSetting, 20);
                        ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
                        //Logger.Info($"{option.Name}, {index}", "OptionList.TryAdd");
                        break;
                    }
                case OptionTypes.Float:
                case OptionTypes.Int:
                    {
                        optionBehaviour = UnityEngine.Object.Instantiate<NumberOption>(__instance.numberOptionOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                        optionBehaviour.transform.localPosition = new Vector3(pos_x, num, pos_z);

                        OptionBehaviourSetSizeAndPosition(optionBehaviour, option, baseGameSetting.Type);

                        optionBehaviour.SetClickMask(__instance.ButtonClickMask);
                        optionBehaviour.SetUpFromData(baseGameSetting, 20);
                        ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
                        //Logger.Info($"{option.Name}, {index}", "OptionList.TryAdd");
                        break;
                    }

                //case OptionTypes.Player:
                //    {
                //        OptionBehaviour optionBehaviour = UnityEngine.Object.Instantiate<PlayerOption>(__instance.playerOptionOrigin, Vector3.zero, Quaternion.identity, __instance.settingsContainer);
                //        break;
                //    }
                default:
                    continue;

            }
            optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
            optionBehaviour.SetClickMask(__instance.ButtonClickMask);
            optionBehaviour.SetUpFromData(baseGameSetting, 20);
            ModGameOptionsMenu.OptionList.TryAdd(optionBehaviour, index);
            ModGameOptionsMenu.BehaviourList.TryAdd(index, optionBehaviour);
            optionBehaviour.gameObject.SetActive(enabled);
            __instance.Children.Add(optionBehaviour);

            if (enabled) num -= 0.45f;
        }

        __instance.ControllerSelectable.Clear();
        foreach (var x in __instance.scrollBar.GetComponentsInChildren<UiElement>())
            __instance.ControllerSelectable.Add(x);
        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);

        return false;
    }
    private static void OptionBehaviourSetSizeAndPosition(OptionBehaviour optionBehaviour, OptionItem option, OptionTypes type)
    {
        optionBehaviour.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite($"TownOfHost_Y.Resources.SettingMenu_LabelBackground.png", 100f);

        Vector3 positionOffset = new(0f, 0f, 0f);
        Vector3 scaleOffset = new(0f, 0f, 0f);
        Color color = new(0.7f, 0.7f, 0.7f);
        float sizeDelta_x = 5.7f;

        if (option.Parent?.Parent?.Parent != null)
        {
            scaleOffset = new(-0.18f, 0, 0);
            positionOffset = new(0.3f, 0f, 0f);
            color = new(0.7f, 0.5f, 0.5f);
            sizeDelta_x = 5.1f;
        }
        else if (option.Parent?.Parent != null)
        {
            scaleOffset = new(-0.12f, 0, 0);
            positionOffset = new(0.2f, 0f, 0f);
            color = new(0.5f, 0.5f, 0.7f);
            sizeDelta_x = 5.3f;
        }
        else if (option.Parent != null)
        {
            scaleOffset = new(-0.05f, 0, 0);
            positionOffset = new(0.1f, 0f, 0f);
            color = new(0.5f, 0.7f, 0.5f);
            sizeDelta_x = 5.5f;
        }

        optionBehaviour.transform.FindChild("LabelBackground").GetComponent<SpriteRenderer>().color = color;
        optionBehaviour.transform.FindChild("LabelBackground").localScale += new Vector3(0.9f, -0.2f, 0f) + scaleOffset;
        optionBehaviour.transform.FindChild("LabelBackground").localPosition += new Vector3(-0.4f, 0f, 0f) + positionOffset;

        optionBehaviour.transform.FindChild("Title Text").localPosition += new Vector3(-0.4f, 0f, 0f) + positionOffset; ;
        optionBehaviour.transform.FindChild("Title Text").GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta_x, 0.37f);
        optionBehaviour.transform.FindChild("Title Text").GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        optionBehaviour.transform.FindChild("Title Text").GetComponent<TMPro.TextMeshPro>().fontStyle = TMPro.FontStyles.Bold;
        optionBehaviour.transform.FindChild("Title Text").GetComponent<TMPro.TextMeshPro>().outlineWidth = 0.17f;

        switch (type)
        {
            case OptionTypes.Checkbox:
                optionBehaviour.transform.FindChild("Toggle").localPosition = new Vector3(1.46f, -0.042f);
                break;

            case OptionTypes.String:
                optionBehaviour.transform.FindChild("PlusButton (1)").localPosition += new Vector3(option.IsSingleValue ? 100f : 1.7f, option.IsSingleValue ? 100f : 0f, option.IsSingleValue ? 100f : 0f);
                optionBehaviour.transform.FindChild("MinusButton (1)").localPosition += new Vector3(option.IsSingleValue ? 100f : 0.9f, option.IsSingleValue ? 100f : 0f, option.IsSingleValue ? 100f : 0f);
                optionBehaviour.transform.FindChild("Value_TMP (1)").localPosition += new Vector3(1.3f, 0f, 0f);
                optionBehaviour.transform.FindChild("Value_TMP (1)").GetComponent<RectTransform>().sizeDelta = new Vector2(2.3f, 0.4f);
                goto default;

            case OptionTypes.Float:
            case OptionTypes.Int:
                optionBehaviour.transform.FindChild("PlusButton").localPosition += new Vector3(option.IsSingleValue ? 100f : 1.7f, option.IsSingleValue ? 100f : 0f, option.IsSingleValue ? 100f : 0f);
                optionBehaviour.transform.FindChild("MinusButton").localPosition += new Vector3(option.IsSingleValue ? 100f : 0.9f, option.IsSingleValue ? 100f : 0f, option.IsSingleValue ? 100f : 0f);
                optionBehaviour.transform.FindChild("Value_TMP").localPosition += new Vector3(1.3f, 0f, 0f);
                goto default;

            default:// Number & String 共通
                optionBehaviour.transform.FindChild("ValueBox").localScale += new Vector3(0.2f, 0f, 0f);
                optionBehaviour.transform.FindChild("ValueBox").localPosition += new Vector3(1.3f, 0f, 0f);
                break;
        }
    }

    [HarmonyPatch(nameof(GameOptionsMenu.ValueChanged)), HarmonyPrefix]
    private static bool ValueChangedPrefix(GameOptionsMenu __instance, OptionBehaviour option)
    {
        if (ModGameOptionsMenu.TabIndex < 3) return true;

        if (ModGameOptionsMenu.OptionList.TryGetValue(option, out var index))
        {
            var item = OptionItem.AllOptions[index];
            if (item != null && item.Children.Count > 0) ReCreateSettings(__instance);
        }
        return false;
    }
    private static void ReCreateSettings(GameOptionsMenu __instance)
    {
        if (ModGameOptionsMenu.TabIndex < 3) return;
        var modTab = (TabGroup)(ModGameOptionsMenu.TabIndex - 3);

        //float num = 0.713f;
        float num = 2.0f;
        for (int index = 0; index < OptionItem.AllOptions.Count; index++)
        {
            var option = OptionItem.AllOptions[index];
            if (option.Tab != modTab) continue;

            var enabled = !option.IsHiddenOn(Options.CurrentGameMode)
                         && (option.Parent == null || (!option.Parent.IsHiddenOn(Options.CurrentGameMode) && option.Parent.GetBool()));

            if (ModGameOptionsMenu.CategoryHeaderList.TryGetValue(index, out var categoryHeaderMasked))
            {
                categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
                categoryHeaderMasked.gameObject.SetActive(enabled);
                if (enabled) num -= 0.63f;
            }
            if (ModGameOptionsMenu.BehaviourList.TryGetValue(index, out var optionBehaviour))
            {
                optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                optionBehaviour.gameObject.SetActive(enabled);
                if (enabled) num -= 0.45f;
            }
        }

        __instance.ControllerSelectable.Clear();
        foreach (var x in __instance.scrollBar.GetComponentsInChildren<UiElement>())
            __instance.ControllerSelectable.Add(x);
        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);
    }

    private static BaseGameSetting GetSetting(OptionItem item)
    {
        BaseGameSetting baseGameSetting = null;

        if (item is BooleanOptionItem)
        {
            baseGameSetting = new CheckboxGameSetting
            {
                Type = OptionTypes.Checkbox,
            };
        }
        else if (item is IntegerOptionItem)
        {
            IntegerOptionItem intItem = item as IntegerOptionItem;
            baseGameSetting = new IntGameSetting
            {
                Type = OptionTypes.Int,
                Value = intItem.GetInt(),
                Increment = intItem.Rule.Step,
                ValidRange = new IntRange(intItem.Rule.MinValue, intItem.Rule.MaxValue),
                ZeroIsInfinity = false,
                SuffixType = NumberSuffixes.Multiplier,
                FormatString = string.Empty,
            };
        }
        else if (item is FloatOptionItem)
        {
            FloatOptionItem floatItem = item as FloatOptionItem;
            baseGameSetting = new FloatGameSetting
            {
                Type = OptionTypes.Float,
                Value = floatItem.GetFloat(),
                Increment = floatItem.Rule.Step,
                ValidRange = new FloatRange(floatItem.Rule.MinValue, floatItem.Rule.MaxValue),
                ZeroIsInfinity = false,
                SuffixType = NumberSuffixes.Multiplier,
                FormatString = string.Empty,
            };
        }
        else if (item is StringOptionItem)
        {
            StringOptionItem stringItem = item as StringOptionItem;
            baseGameSetting = new StringGameSetting
            {
                Type = OptionTypes.String,
                Values = new StringNames[stringItem.Selections.Length], //ダミー
                Index = stringItem.GetInt(),
            };
        }

        if (baseGameSetting != null)
        {
            baseGameSetting.Title = StringNames.Accept; //ダミー
        }

        return baseGameSetting;
    }
}

[HarmonyPatch(typeof(ToggleOption))]
public static class ToggleOptionPatch
{
    [HarmonyPatch(nameof(ToggleOption.Initialize)), HarmonyPrefix]
    private static bool InitializePrefix(ToggleOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "ToggleOption.Initialize.TryGetValue");
            __instance.TitleText.text = item.GetName();
            __instance.CheckMark.enabled = item.GetBool();
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(ToggleOption.UpdateValue)), HarmonyPrefix]
    private static bool UpdateValuePrefix(ToggleOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "ToggleOption.UpdateValue.TryGetValue");
            item.SetValue(__instance.GetBool() ? 1 : 0);
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(NumberOption))]
public static class NumberOptionPatch
{
    [HarmonyPatch(nameof(NumberOption.Initialize)), HarmonyPrefix]
    private static bool InitializePrefix(NumberOption __instance)
    {
        // バニラゲーム設定の拡張
        switch (__instance.Title)
        {
            case StringNames.GameShortTasks:
            case StringNames.GameLongTasks:
            case StringNames.GameCommonTasks:
                __instance.ValidRange = new FloatRange(0, 99);
                break;
            case StringNames.GameKillCooldown:
                __instance.ValidRange = new FloatRange(0, 180);
                break;
            case StringNames.GameNumImpostors:
                if (DebugModeManager.IsDebugMode)
                {
                    __instance.ValidRange.min = 0;
                }
                break;
            default:
                break;
        }

        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "NumberOption.Initialize.TryGetValue");
            __instance.TitleText.text = item.GetName();
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(NumberOption.UpdateValue)), HarmonyPrefix]
    private static bool UpdateValuePrefix(NumberOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "NumberOption.UpdateValue.TryGetValue");

            if (item is IntegerOptionItem integerOptionItem)
            {
                integerOptionItem.SetValue(integerOptionItem.Rule.GetNearestIndex(__instance.GetInt()));
            }
            else if (item is FloatOptionItem floatOptionItem)
            {
                floatOptionItem.SetValue(floatOptionItem.Rule.GetNearestIndex(__instance.GetFloat()));
            }

            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(NumberOption.FixedUpdate)), HarmonyPrefix]
    private static bool FixedUpdatePrefix(NumberOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "NumberOption.FixedUpdate.TryGetValue");

            if (__instance.oldValue != __instance.Value)
            {
                __instance.oldValue = __instance.Value;
                __instance.ValueText.text = GetValueString(__instance, __instance.Value, item);
            }
            return false;
        }
        return true;
    }
    public static string GetValueString(NumberOption __instance, float value, OptionItem item)
    {
        if (__instance.ZeroIsInfinity && Mathf.Abs(value) < 0.0001f) return "<b>∞</b>";
        if (item == null) return value.ToString(__instance.FormatString);
        return item.ApplyFormat(value.ToString());
    }
    [HarmonyPatch(nameof(NumberOption.Increase)), HarmonyPrefix]
    public static bool IncreasePrefix(NumberOption __instance)
    {
        if (__instance.Value == __instance.ValidRange.max)
        {
            __instance.Value = __instance.ValidRange.min;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(NumberOption.Decrease)), HarmonyPrefix]
    public static bool DecreasePrefix(NumberOption __instance)
    {
        if (__instance.Value == __instance.ValidRange.min)
        {
            __instance.Value = __instance.ValidRange.max;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(StringOption))]
public static class StringOptionPatch
{
    [HarmonyPatch(nameof(StringOption.Initialize)), HarmonyPrefix]
    private static bool InitializePrefix(StringOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            //Logger.Info($"{item.Name}, {index}", "StringOption.Initialize.TryAdd");
            __instance.TitleText.text = item.GetName();
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(StringOption.UpdateValue)), HarmonyPrefix]
    private static bool UpdateValuePrefix(StringOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];
            Logger.Info($"{item.Name}, {index}", "StringOption.UpdateValue.TryAdd");

            item.SetValue(__instance.GetInt());
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(StringOption.FixedUpdate)), HarmonyPrefix]
    private static bool FixedUpdatePrefix(StringOption __instance)
    {
        if (ModGameOptionsMenu.OptionList.TryGetValue(__instance, out var index))
        {
            var item = OptionItem.AllOptions[index];

            if (item is StringOptionItem stringOptionItem)
            {
                if (__instance.oldValue != __instance.Value)
                {
                    __instance.oldValue = __instance.Value;
                    __instance.ValueText.text = Translator.GetString(stringOptionItem.Selections[stringOptionItem.Rule.GetValueByIndex(__instance.Value)]);
                }
            }
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(StringOption.Increase)), HarmonyPrefix]
    public static bool IncreasePrefix(StringOption __instance)
    {
        if (__instance.Value == __instance.Values.Length - 1)
        {
            __instance.Value = 0;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            return false;
        }
        return true;
    }
    [HarmonyPatch(nameof(StringOption.Decrease)), HarmonyPrefix]
    public static bool DecreasePrefix(StringOption __instance)
    {
        if (__instance.Value == 0)
        {
            __instance.Value = __instance.Values.Length - 1;
            __instance.UpdateValue();
            __instance.OnValueChanged.Invoke(__instance);
            return false;
        }
        return true;
    }
}

//[HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
//public class StringOptionIncreasePatch
//{
//    public static bool Prefix(StringOption __instance)
//    {
//        var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
//        if (option == null) return true;

//        option.SetValue(option.CurrentValue + (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5 : 1));
//        return false;
//    }
//}

//[HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
//public class StringOptionDecreasePatch
//{
//    public static bool Prefix(StringOption __instance)
//    {
//        var option = OptionItem.AllOptions.FirstOrDefault(opt => opt.OptionBehaviour == __instance);
//        if (option == null) return true;

//        option.SetValue(option.CurrentValue - (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 5 : 1));
//        return false;
//    }
//}

// TEST

[HarmonyPatch(typeof(GameSettingMenu))]
public class GameSettingMenuPatch
{
    // ゲーム設定メニュータブ
    public enum GameSettingMenuTab
    {
        GamePresets = 0,
        GameSettings,
        RoleSettings,
        Mod_MainSettings,
        Mod_ImpostorRoles,
        Mod_MadmateRoles,
        Mod_CrewmateRoles,
        Mod_NeutralRoles,
        Mod_UnitRoles,
        Mod_AddOns,

        MaxCount,
    }

    // ボタンに表示する名前
    public static string[] buttonName = new string[]{
        "Game Settings",
        "TOH_Y Settings",
        "Impostor Roles",
        "Madmate Roles",
        "Crewmate Roles",
        "Neutral Roles",
        "Unit Roles",
        "Add-Ons"
    };

    // 左側配置ボタン座標
    private static Vector3 buttonPosition_Left = new(-3.9f, -0.4f, 0f);
    // 右側配置ボタン座標
    private static Vector3 buttonPosition_Right = new(-2.4f, -0.4f, 0f);
    // ボタンサイズ
    private static Vector3 buttonSize = new(0.45f, 0.6f, 1f);

    private static GameOptionsMenu templateGameOptionsMenu;
    private static PassiveButton templateGameSettingsButton;

    // MOD設定用ボタン格納変数
    static Dictionary<TabGroup, PassiveButton> ModSettingsButtons = new();
    // MOD設定メニュー用タブ格納変数
    static Dictionary<TabGroup, GameOptionsMenu> ModSettingsTabs = new();

    // ゲーム設定メニュー 初期関数
    [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void StartPostfix(GameSettingMenu __instance)
    {
        /******** ボタン作成 ********/

        // 各グループ毎にボタンを作成する
        ModSettingsButtons = new();
        foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
        {
            // ゲーム設定ボタンを元にコピー
            var button = Object.Instantiate(templateGameSettingsButton, __instance.GameSettingsButton.transform.parent);
            button.gameObject.SetActive(true);
            // 名前は「button_ + ボタン名」
            button.name = "Button_" + buttonName[(int)tab + 1]; // buttonName[0]はバニラ設定用の名前なので+1
            // ボタンテキスト
            var label = button.GetComponentInChildren<TextMeshPro>();
            // ボタンテキストの翻訳破棄
            label.DestroyTranslator();
            // ボタンテキストの名前変更
            label.text = "";
            // ボタンテキストの色変更
            button.activeTextColor = button.inactiveTextColor = Color.black;
            // ボタンテキストの選択中の色変更
            button.selectedTextColor = Color.blue;

            var activeButton = Utils.LoadSprite($"TownOfHost_Y.Resources.Tab_Active_{tab}.png", 100f);
            // 各種スプライトをオリジナルのものに変更
            button.inactiveSprites.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite($"TownOfHost_Y.Resources.Tab_Small_{tab}.png", 100f);
            button.activeSprites.GetComponent<SpriteRenderer>().sprite = activeButton;
            button.selectedSprites.GetComponent<SpriteRenderer>().sprite = activeButton;

            // Y座標オフセット
            Vector3 offset = new(0.0f, 0.5f * (((int)tab + 1) / 2), 0.0f);
            // ボタンの座標設定
            button.transform.localPosition = ((((int)tab + 1) % 2 == 0) ? buttonPosition_Left : buttonPosition_Right) - offset;
            // ボタンのサイズ設定
            button.transform.localScale = buttonSize;

            // ボタンがクリックされた時の設定
            var buttonComponent = button.GetComponent<PassiveButton>();
            buttonComponent.OnClick = new();
            // ボタンがクリックされるとタブをそのものに変更する
            buttonComponent.OnClick.AddListener(
                (Action)(() => __instance.ChangeTab((int)tab + 3, false)));

            // ボタン登録
            ModSettingsButtons.Add(tab, button);
        }/******** ボタン作成 ここまで ********/

        /******** タブ作成 ********/
        //// ストリングオプションのテンプレート作成
        //var templateStringOption = GameObject.Find("Main Camera/PlayerOptionsMenu(Clone)/MainArea/GAME SETTINGS TAB/Scroller/SliderInner/GameOption_String(Clone)").GetComponent<StringOption>();
        //if (templateStringOption == null) return;

        ModGameOptionsMenu.OptionList = new();
        ModGameOptionsMenu.BehaviourList = new();
        ModGameOptionsMenu.CategoryHeaderList = new();

        // 各グループ毎にタブを作成する/基盤作成
        ModSettingsTabs = new();
        foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
        {
            // ゲーム設定タブからコピー
            var setTab = Object.Instantiate(templateGameOptionsMenu, __instance.GameSettingsTab.transform.parent);
            // 名前はゲーム設定タブEnumから取得
            setTab.name = ((GameSettingMenuTab)tab + 3).ToString();
            //// 中身を削除
            //setTab.GetComponentsInChildren<OptionBehaviour>().Do(x => Object.Destroy(x.gameObject));
            //setTab.GetComponentsInChildren<CategoryHeaderMasked>().Do(x => Object.Destroy(x.gameObject));
            setTab.gameObject.SetActive(false);

            // 設定タブを追加
            ModSettingsTabs.Add(tab, setTab);
        }

        foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
        {
            if (ModSettingsButtons.TryGetValue(tab, out var button))
            {
                __instance.ControllerSelectable.Add(button);
            }
        }

        //⇒GamOptionsMenuPatchで処理
        //// 各グループ毎にタブを作成する/中身追加
        //foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
        //{
        //    // オプションをまとめて格納する
        //    Il2CppSystem.Collections.Generic.List<OptionBehaviour> scOptions = new();

        //    // オプションを全てまわす
        //    foreach (var option in OptionItem.AllOptions)
        //    {
        //        // オプションを出すタブでないなら次
        //        if (option.Tab != tab) continue;

        //        // ビヘイビアがまだ設定されていないなら
        //        if (option.OptionBehaviour == null)
        //        {
        //            // ストリングオプションをコピー
        //            var stringOption = Object.Instantiate(templateStringOption, GameObject.Find($"{ModSettingsTabs[tab].name}/Scroller/SliderInner").transform);
        //            // オプションListに追加
        //            scOptions.Add(stringOption);
        //            stringOption.OnValueChanged = new System.Action<OptionBehaviour>((o) => { });
        //            stringOption.TitleText.text = option.Name;
        //            stringOption.Value = stringOption.oldValue = option.CurrentValue;
        //            stringOption.ValueText.text = option.GetString();
        //            stringOption.name = option.Name;
        //            stringOption.transform.FindChild("LabelBackground").localScale = new Vector3(1.6f, 1f, 1f);
        //            stringOption.transform.FindChild("LabelBackground").SetLocalX(-2.2695f);
        //            stringOption.transform.FindChild("PlusButton (1)").localPosition += new Vector3(option.IsFixValue ? 100f : 1.1434f, option.IsFixValue ? 100f : 0f, option.IsFixValue ? 100f : 0f);
        //            stringOption.transform.FindChild("MinusButton (1)").localPosition += new Vector3(option.IsFixValue ? 100f : 0.3463f, option.IsFixValue ? 100f : 0f, option.IsFixValue ? 100f : 0f);
        //            stringOption.transform.FindChild("Value_TMP (1)").localPosition += new Vector3(0.7322f, 0f, 0f);
        //            stringOption.transform.FindChild("ValueBox").localScale += new Vector3(0.2f, 0f, 0f);
        //            stringOption.transform.FindChild("ValueBox").localPosition += new Vector3(0.7322f, 0f, 0f);
        //            stringOption.transform.FindChild("Title Text").localPosition += new Vector3(-1.096f, 0f, 0f);
        //            stringOption.transform.FindChild("Title Text").GetComponent<RectTransform>().sizeDelta = new Vector2(6.5f, 0.37f);
        //            stringOption.transform.FindChild("Title Text").GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        //            stringOption.SetClickMask(ModSettingsTabs[tab].ButtonClickMask);

        //            // ビヘイビアに作成したストリングオプションを設定
        //            option.OptionBehaviour = stringOption;
        //        }
        //        // ビヘイビアのobjectを表示
        //        option.OptionBehaviour.gameObject.SetActive(true);
        //    }
        //    // タブの子にオプションリストを設定
        //    ModSettingsTabs[tab].Children = scOptions;
        //    // 選択されるときに表示するため、初期値はfalse
        //    ModSettingsTabs[tab].gameObject.SetActive(false);
        //    // 有効にする
        //    ModSettingsTabs[tab].enabled = true;
        //}
    }
    private static void SetDefaultButton(GameSettingMenu __instance)
    {
        /******** デフォルトボタン設定 ********/
        // プリセット設定 非表示
        __instance.GamePresetsButton.gameObject.SetActive(false);

        /**** ゲーム設定ボタンを変更 ****/
        var gameSettingButton = __instance.GameSettingsButton;
        // 座標指定
        gameSettingButton.transform.localPosition = new(-3f, -0.5f, 0f);
        // ボタンテキスト
        var textLabel = gameSettingButton.GetComponentInChildren<TextMeshPro>();
        // 翻訳破棄
        textLabel.DestroyTranslator();
        // バニラ設定ボタンの名前を設定
        textLabel.text = "";
        // ボタンテキストの色変更
        gameSettingButton.activeTextColor = gameSettingButton.inactiveTextColor = Color.black;
        // ボタンテキストの選択中の色変更
        gameSettingButton.selectedTextColor = Color.blue;

        var vanillaActiveButton = Utils.LoadSprite($"TownOfHost_Y.Resources.Tab_Active_VanillaGameSettings.png", 100f);
        // 各種スプライトをオリジナルのものに変更
        gameSettingButton.inactiveSprites.GetComponent<SpriteRenderer>().sprite = Utils.LoadSprite($"TownOfHost_Y.Resources.Tab_Small_VanillaGameSettings.png", 100f);
        gameSettingButton.activeSprites.GetComponent<SpriteRenderer>().sprite = vanillaActiveButton;
        gameSettingButton.selectedSprites.GetComponent<SpriteRenderer>().sprite = vanillaActiveButton;
        // ボタンの座標設定
        gameSettingButton.transform.localPosition = buttonPosition_Left;
        // ボタンのサイズ設定
        gameSettingButton.transform.localScale = buttonSize;
        /**** ゲーム設定ボタンを変更 ここまで ****/

        // バニラ役職設定 非表示
        __instance.RoleSettingsButton.gameObject.SetActive(false);
        /******** デフォルトボタン設定 ここまで ********/

        __instance.DefaultButtonSelected = gameSettingButton;
        __instance.ControllerSelectable = new();
        __instance.ControllerSelectable.Add(gameSettingButton);
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab)), HarmonyPrefix]
    public static bool ChangeTabPrefix(GameSettingMenu __instance, ref int tabNum, [HarmonyArgument(1)] bool previewOnly)
    {
        //// プリセットタブは表示させないため、ゲーム設定タブを設定する
        //if (tabNum == (int)GameSettingMenuTab.GamePresets) {
        //    tabNum = (int)GameSettingMenuTab.GameSettings;

        //    // What Is this?のテキスト文を変更
        //    // __instance.MenuDescriptionText.text = "test";
        //}

        ModGameOptionsMenu.TabIndex = tabNum;

        GameOptionsMenu settingsTab;
        PassiveButton button;

        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
            {
                if (ModSettingsTabs.TryGetValue(tab, out settingsTab) &&
                    settingsTab != null)
                {
                    settingsTab.gameObject.SetActive(false);
                }
            }
            foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
            {
                if (ModSettingsButtons.TryGetValue(tab, out button) &&
                    button != null)
                {
                    button.SelectButton(false);
                }
            }
        }

        if (tabNum < 3) return true;

        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            __instance.PresetsTab.gameObject.SetActive(false);
            __instance.GameSettingsTab.gameObject.SetActive(false);
            __instance.RoleSettingsTab.gameObject.SetActive(false);
            __instance.GamePresetsButton.SelectButton(false);
            __instance.GameSettingsButton.SelectButton(false);
            __instance.RoleSettingsButton.SelectButton(false);

            if (ModSettingsTabs.TryGetValue((TabGroup)(tabNum - 3), out settingsTab) &&
                settingsTab != null)
            {
                settingsTab.gameObject.SetActive(true);
                __instance.MenuDescriptionText.DestroyTranslator();
                switch ((TabGroup)(tabNum - 3))
                {
                    case TabGroup.SystemSettings:
                        __instance.MenuDescriptionText.text = "MOD機能の設定ができる。";
                        break;
                    case TabGroup.GameSettings:
                        __instance.MenuDescriptionText.text = "MOD機能の設定ができる。";
                        break;
                    case TabGroup.ImpostorRoles:
                        __instance.MenuDescriptionText.text = "MODインポスターロールの設定ができる。";
                        break;
                    case TabGroup.CrewmateRoles:
                        __instance.MenuDescriptionText.text = "MODクルーメイトロールの設定ができる。";
                        break;
                    case TabGroup.NeutralRoles:
                        __instance.MenuDescriptionText.text = "MODニュートラルロールの設定ができる。";
                        break;
                    case TabGroup.OtherRoles:
                        __instance.MenuDescriptionText.text = "MODユニットロールの設定ができる。";
                        break;
                    case TabGroup.Addons:
                        __instance.MenuDescriptionText.text = "MODロール属性の設定ができる。";
                        break;
                }
            }
        }
        if (previewOnly)
        {
            __instance.ToggleLeftSideDarkener(false);
            __instance.ToggleRightSideDarkener(true);
            return false;
        }
        __instance.ToggleLeftSideDarkener(true);
        __instance.ToggleRightSideDarkener(false);
        //if (ModSettingsTabs.TryGetValue((TabGroup)(tabNum - 3), out settingsTab) &&
        //    settingsTab != null)
        //{
        //    settingsTab.OpenMenu();
        //}
        if (ModSettingsButtons.TryGetValue((TabGroup)(tabNum - 3), out button) &&
            button != null)
        {
            button.SelectButton(true);
        }

        return false;
    }

    [HarmonyPatch(nameof(GameSettingMenu.OnEnable)), HarmonyPrefix]
    private static bool OnEnablePrefix(GameSettingMenu __instance)
    {
        if (templateGameOptionsMenu == null)
        {
            templateGameOptionsMenu = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
            templateGameOptionsMenu.gameObject.SetActive(false);
        }
        if (templateGameSettingsButton == null)
        {
            templateGameSettingsButton = Object.Instantiate(__instance.GameSettingsButton, __instance.GameSettingsButton.transform.parent);
            templateGameSettingsButton.gameObject.SetActive(false);
        }

        SetDefaultButton(__instance);

        ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, __instance.ControllerSelectable, false);
        DestroyableSingleton<HudManager>.Instance.menuNavigationPrompts.SetActive(false);
        if (Controller.currentTouchType != Controller.TouchType.Joystick)
        {
            __instance.ChangeTab(1, Controller.currentTouchType == Controller.TouchType.Joystick);
        }
        __instance.StartCoroutine(__instance.CoSelectDefault());

        return false;
    }
    [HarmonyPatch(nameof(GameSettingMenu.Close)), HarmonyPostfix]
    private static void ClosePostfix(GameSettingMenu __instance)
    {
        foreach (var button in ModSettingsButtons.Values)
            UnityEngine.Object.Destroy(button);
        foreach (var tab in ModSettingsTabs.Values)
            UnityEngine.Object.Destroy(tab);
        ModSettingsButtons = new();
        ModSettingsTabs = new();
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public class RpcSyncSettingsPatch
{
    public static void Postfix()
    {
        OptionItem.SyncAllOptions();
    }
}

//[HarmonyPatch(typeof(NormalGameOptionsV08), nameof(NormalGameOptionsV08.SetRecommendations))]
//public static class SetRecommendationsPatch
//{
//    public static bool Prefix(NormalGameOptionsV08 __instance, int numPlayers, bool isOnline)
//    {
//        numPlayers = Mathf.Clamp(numPlayers, 4, 15);
//        __instance.PlayerSpeedMod = __instance.MapId == 4 ? 1.25f : 1f; //AirShipなら1.25、それ以外は1
//        __instance.CrewLightMod = 0.5f;
//        __instance.ImpostorLightMod = 1.75f;
//        __instance.KillCooldown = 25f;
//        __instance.NumCommonTasks = 2;
//        __instance.NumLongTasks = 4;
//        __instance.NumShortTasks = 6;
//        __instance.NumEmergencyMeetings = 1;
//        if (!isOnline)
//            __instance.NumImpostors = NormalGameOptionsV08.RecommendedImpostors[numPlayers];
//        __instance.KillDistance = 0;
//        __instance.DiscussionTime = 0;
//        __instance.VotingTime = 150;
//        __instance.IsDefaults = true;
//        __instance.ConfirmImpostor = false;
//        __instance.VisualTasks = false;

//        __instance.roleOptions.SetRoleRate(RoleTypes.Shapeshifter, 0, 0);
//        __instance.roleOptions.SetRoleRate(RoleTypes.Scientist, 0, 0);
//        __instance.roleOptions.SetRoleRate(RoleTypes.GuardianAngel, 0, 0);
//        __instance.roleOptions.SetRoleRate(RoleTypes.Engineer, 0, 0);
//        __instance.roleOptions.SetRoleRecommended(RoleTypes.Shapeshifter);
//        __instance.roleOptions.SetRoleRecommended(RoleTypes.Scientist);
//        __instance.roleOptions.SetRoleRecommended(RoleTypes.GuardianAngel);
//        __instance.roleOptions.SetRoleRecommended(RoleTypes.Engineer);

//        //if (Options.IsONMode)
//        //{
//        //    __instance.NumCommonTasks = 1;
//        //    __instance.NumLongTasks = 0;
//        //    __instance.NumShortTasks = 1;
//        //    __instance.KillCooldown = 20f;
//        //    __instance.NumEmergencyMeetings = 0;
//        //    __instance.KillDistance = 0;
//        //    __instance.DiscussionTime = 0;
//        //    __instance.VotingTime = 300;
//        //}

//        return false;
//    }
//}
