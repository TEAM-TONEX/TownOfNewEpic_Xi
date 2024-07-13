using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static TONEX.Translator;
using static Il2CppSystem.Uri;
using System.Linq;
using TMPro;

namespace TONEX.OptionUI;

public class TabGroupManager : SettingItemManager
{
    public override void Init()
    {
        GetInstance().tabButtons = new();
    }
    public void CreateTabGroupButton()
    {

        var numItem = 1;

        // 获取Sidebar的RectTransform
        var sidebarRect = GetInstance().sidebar.GetComponent<RectTransform>();

        foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
        {
            GameObject newButton = new GameObject($"{tab}");
            var rectTransform = newButton.AddComponent<RectTransform>();
            newButton.AddComponent<CanvasRenderer>();
            var buttonComponent = newButton.AddComponent<Button>();
            var buttonImage = newButton.AddComponent<Image>();

            rectTransform.SetParent(sidebarRect);
            rectTransform.anchorMin = new Vector2(0.5f, 1); // 锚点设置为顶部中心
            rectTransform.anchorMax = new Vector2(0.5f, 1); // 锚点设置为顶部中心
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // 轴心设置为中心

            GetInstance().size = sidebarRect.rect.width - 11f;
            rectTransform.anchoredPosition = new Vector2(0, -(GetInstance().size * numItem) - 5f); // 计算按钮位置
            rectTransform.sizeDelta = new Vector2(GetInstance().size, GetInstance().size); // 设置按钮大小

            buttonImage.sprite = Utils.LoadSprite($"TONEX.Resources.Images.TabIcon_{tab}.png", 100f);

            buttonComponent.onClick.AddListener(new Action(() => OnTabGroupButtonClicked(tab)));
            numItem++;


            // 将按钮添加到列表中
            GetInstance().tabButtons.Add(buttonComponent, false);


        }
        RefreshAllTabGroups();
        OnTabGroupButtonClicked(TabGroup.SystemSettings);
    }
    void OnTabGroupButtonClicked(TabGroup tab)
    {
        // 遍历所有按钮，更新它们的外观以反映当前开启的标签页
        foreach (var pair in GetInstance().tabButtons)
        {
            var button = pair.Key;

            if (button.name.Equals(tab.ToString()))
            {
                // 选中状态
                var rectTransform = button.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(GetInstance().size * 1.3f, GetInstance().size * 1.3f); // 放大按钮
                GetInstance().tabButtons[button] = true; // 更新按钮状态为选中
            }
            else
            {
                // 非选中状态
                var rectTransform = button.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(GetInstance().size, GetInstance().size); // 恢复原始大小
                GetInstance().tabButtons[button] = false; // 更新按钮状态为非选中
            }
        }
        UpdateHeaderText(tab);
    }

    void CreateHeaderText(TabGroup tab)
    {
        // 创建 HeaderText 对象
        var headerText = new GameObject($"HeaderTextFor{tab}");
        RectTransform headerRect = headerText.AddComponent<RectTransform>();
        headerText.AddComponent<CanvasRenderer>();

        TextMeshProUGUI textComponent = headerText.AddComponent<TextMeshProUGUI>();

        // 将 HeaderText 设置为 GetInstance().mainUI 的子对象
        headerRect.SetParent(GetInstance().mainUI.transform, false);

        // 设置 HeaderText 的尺寸
        headerRect.sizeDelta = new Vector2(800, 100); // 设定更大的宽度和高度

        // 设置 HeaderText 的锚点和位置，使其紧贴在 GetInstance().mainUI 的顶部和 GetInstance().sidebar 的边缘
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(0, 1);
        headerRect.pivot = new Vector2(0.5f, 1);

        // 计算 GetInstance().sidebar 的宽度
        RectTransform sidebarRect = GetInstance().sidebar.GetComponent<RectTransform>();
        float sidebarWidth = sidebarRect.rect.width;

        // 设置 HeaderText 的偏移量，使其贴着 GetInstance().sidebar 的右边缘
        headerRect.offsetMin = new Vector2(sidebarWidth, headerRect.offsetMin.y);
        headerRect.offsetMax = new Vector2(sidebarWidth + 800f, headerRect.offsetMax.y); // 确保宽度足够

        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Arial SDF");

        // 设置TextMeshPro的字体和其他属性
        textComponent.font = defaultFont; // 使用默认的TextMesh Pro字体
        textComponent.text = $"<color={GetTabColor(tab)}>{GetString("TabGroup." + tab)}</color>";
        textComponent.alignment = TextAlignmentOptions.MidlineLeft; // 左对齐
        textComponent.enableWordWrapping = false; // 防止换行
        textComponent.fontSize = 75;

        // 将 HeaderText 对象添加到实例的 Headers 列表中
        GetInstance().Headers.Add(headerText);
    }
    // 获取 TabGroup 的颜色
    private string GetTabColor(TabGroup tab)
    {
        return tab switch
        {
            TabGroup.SystemSettings => Main.ModColor,
            TabGroup.ModSettings => "#59ef83",
            TabGroup.ImpostorRoles => Utils.GetCustomRoleTypeColorCode(Roles.Core.CustomRoleTypes.Impostor),
            TabGroup.CrewmateRoles => Utils.GetCustomRoleTypeColorCode(Roles.Core.CustomRoleTypes.Crewmate),
            TabGroup.NeutralRoles => Utils.GetCustomRoleTypeColorCode(Roles.Core.CustomRoleTypes.Neutral),
            TabGroup.Addons => Utils.GetCustomRoleTypeColorCode(Roles.Core.CustomRoleTypes.Addon),
            TabGroup.OtherRoles => "#76b8e0",
            _ => "#ffffff",
        };
    }
    void UpdateHeaderText(TabGroup tab)
    {
        foreach (var head in GetInstance().Headers)
        {
            if (head.name == $"HeaderTextFor{tab}")
                head.SetActive(true);
            else
                head.SetActive(false);
        }
    }


    void RefreshTabGroup(TabGroup tab)
    {
        CreateHeaderText(tab);
        CreateScrollbarAndSettingsContent(tab);
        CreateSettingItem(tab);
    }
    void RefreshAllTabGroups()
    {
        foreach (var tab in EnumHelper.GetAllValues<TabGroup>())
        {
            CreateHeaderText(tab);
            CreateScrollbarAndSettingsContent(tab);
            CreateSettingItem(tab);
        }
    }
}
