using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static TONEX.Translator;
using static Il2CppSystem.Uri;
using System.Linq;

namespace TONEX.OptionUI;
public class SidebarManager : TabGroupManager
{

    public void CreateSidebar()
    {
        // 创建 Sidebar GameObject 并添加必要的组件
        GetInstance().sidebar = new GameObject("Sidebar");

        GetInstance().sidebar.AddComponent<RectTransform>();
        GetInstance().sidebar.AddComponent<CanvasRenderer>();
        Image image = GetInstance().sidebar.AddComponent<Image>();
        image.sprite = Utils.LoadSprite($"TONEX.Resources.Images.UI.Empty_transparent.png", 100f);

        // 获取 GetInstance().mainUI 的 RectTransform
        RectTransform mainUIRect = GetInstance().mainUI.GetComponent<RectTransform>();

        // 设置 Sidebar 的父对象为 GetInstance().mainUI
        RectTransform sidebarRect = GetInstance().sidebar.GetComponent<RectTransform>();
        sidebarRect.SetParent(GetInstance().mainUI.transform, false); // 使用 false 参数确保保持本地坐标不变

        // 设置 Sidebar 的位置和大小
        sidebarRect.anchorMin = new Vector2(0, 0);
        sidebarRect.anchorMax = new Vector2(0, 1);
        sidebarRect.pivot = new Vector2(0, 1);

        // 根据 GetInstance().mainUI 的宽度设置 Sidebar 的大小
        float sidebarWidth = mainUIRect.rect.width * 0.075f;
        sidebarRect.sizeDelta = new Vector2(sidebarWidth, 0); // 高度设置为0，这样就会使用主 UI 的高度

        sidebarRect.offsetMin = new Vector2(0, 0); // 左边距离
        sidebarRect.offsetMax = new Vector2(sidebarWidth, 0); // 右边距离
        CreateTabGroupButton();
    }

}
