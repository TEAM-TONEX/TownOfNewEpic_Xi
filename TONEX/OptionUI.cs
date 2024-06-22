using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static TONEX.Translator;
using static Il2CppSystem.Uri;
using System.Linq;

namespace TONEX.UI;

public class CreateUIElements : MonoBehaviour
{
    public static CreateUIElements Instance;
    public GameObject canvas;
    GameObject mainUI;
    public GameObject openButton;
    GameObject sidebar;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        Headers = new();
        CreateCanvas();
        if (canvas == null)
        {
            Debug.LogError("Canvas object not found!");
            return;
        }
        // 创建Central Button
        CreateOpenButton();

        // 创建Main UI及其子元素
        CreateMainUI();
        CreateSidebar();
        CreateCloseButton();

        // 初始化侧边栏和设置项
        CreateSidebarButton();
        Debug.LogError("Done!");
        if (canvas != null) DontDestroyOnLoad(canvas);
        if (mainUI != null) DontDestroyOnLoad(mainUI);
        if (openButton != null) DontDestroyOnLoad(openButton);
        if (sidebar != null) DontDestroyOnLoad(sidebar);
        Debug.LogError("Make Sure Not Be Dest");

    }

    void CreateCanvas()
    {
        canvas = new GameObject("DynamicCanvas");
        Canvas _canvas = canvas.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvas.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.anchoredPosition = Vector2.zero;

        canvasRect.sizeDelta = new Vector2(1920, 1080);

        Debug.Log("Canvas position: " + canvas.transform.position);
    }

    void CreateOpenButton()
    {
        openButton = new GameObject("OpenButton");

        RectTransform rectTransform = openButton.AddComponent<RectTransform>();
        openButton.AddComponent<CanvasRenderer>();
        Image image = openButton.AddComponent<Image>();

        Button buttonComponent = openButton.AddComponent<Button>();

        rectTransform.SetParent(canvas.transform, false);
        rectTransform.anchoredPosition = new Vector2(0, 0);
        // 计算长宽比为 4:3 的尺寸
        float desiredWidth = 200f;
        float desiredHeight = desiredWidth * (3f / 4f);

        // 设置长宽比为 4:3 的尺寸
        rectTransform.sizeDelta = new Vector2(desiredWidth, desiredHeight);


        var sprite = Utils.LoadSprite("TONEX.Resources.Images.UI.Open.png", 100f);
        image.sprite = sprite;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(openButton.transform);

        buttonComponent.onClick.AddListener(new Action(OnOpenButtonClick));

        openButton.SetActive(true);
        Debug.Log("Canvas position: " + openButton.transform.position);
    }

    void OnOpenButtonClick()
    {
        Debug.Log("Central Button Clicked");
        mainUI.SetActive(true);
        openButton.SetActive(false);

    }


    void CreateMainUI()
    {
        mainUI = new GameObject("MainUI");
        mainUI.AddComponent<RectTransform>();
        mainUI.AddComponent<CanvasRenderer>();
        Image image = mainUI.AddComponent<Image>();

        mainUI.GetComponent<RectTransform>().SetParent(canvas.transform);
        mainUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        mainUI.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * 0.8f, Screen.height * 0.8f);

        image.sprite = Utils.LoadSprite($"TONEX.Resources.Images.UI.BackGround.png", 100f);
        mainUI.SetActive(false); // 初始状态下隐藏
    }
    void CreateCloseButton()
    {
        GameObject closeButton = new GameObject("CloseButton");
        RectTransform rectTransform = closeButton.AddComponent<RectTransform>();
        closeButton.AddComponent<CanvasRenderer>();
        var image = closeButton.AddComponent<Image>();
        image.sprite = Utils.LoadSprite("TONEX.Resources.Images.UI.Escape.png", 100f);
        Button buttonComponent = closeButton.AddComponent<Button>();

        // 设置父对象为 mainUI
        rectTransform.SetParent(mainUI.transform);

        // 设置按钮的锚点和对齐方式为右下角
        rectTransform.anchorMin = new Vector2(1, 0);
        rectTransform.anchorMax = new Vector2(1, 0);
        rectTransform.pivot = new Vector2(1, 0);

        // 设置按钮的位置和大小（正方形）
        float buttonSize = 75f; // 正方形的边长
        rectTransform.offsetMin = new Vector2(-buttonSize, 0); // 设置左下角的偏移量
        rectTransform.offsetMax = new Vector2(0, buttonSize); // 设置右上角的偏移量

        buttonComponent.onClick.AddListener(new Action(OnCloseButtonClick));
    }

    void OnCloseButtonClick()
    {
        Debug.Log("Close Button Clicked");
        // 这里可以调用HideMainUI方法
        mainUI.SetActive(false);
        openButton.SetActive(true);

    }
    void CreateSidebar()
    {
        // 创建 Sidebar GameObject 并添加必要的组件
        sidebar = new GameObject("Sidebar");
        sidebar.AddComponent<RectTransform>();
        sidebar.AddComponent<CanvasRenderer>();
        Image image = sidebar.AddComponent<Image>();
        image.sprite = Utils.LoadSprite($"TONEX.Resources.Images.UI.Empty_transparent.png", 100f);

        // 获取 mainUI 的 RectTransform
        RectTransform mainUIRect = mainUI.GetComponent<RectTransform>();

        // 设置 Sidebar 的父对象为 mainUI
        RectTransform sidebarRect = sidebar.GetComponent<RectTransform>();
        sidebarRect.SetParent(mainUI.transform, false); // 使用 false 参数确保保持本地坐标不变

        // 设置 Sidebar 的位置和大小
        sidebarRect.anchorMin = new Vector2(0, 0);
        sidebarRect.anchorMax = new Vector2(0, 1);
        sidebarRect.pivot = new Vector2(0, 1);

        // 根据 mainUI 的宽度设置 Sidebar 的大小
        float sidebarWidth = mainUIRect.rect.width * 0.075f;
        sidebarRect.sizeDelta = new Vector2(sidebarWidth, 0); // 高度设置为0，这样就会使用主 UI 的高度

        sidebarRect.offsetMin = new Vector2(0, 0); // 左边距离
        sidebarRect.offsetMax = new Vector2(sidebarWidth, 0); // 右边距离

       
    }
    Dictionary<Button, bool> tabButtons = new();
    float size;
    void CreateSidebarButton()
    {
        var numItem = 1;

        // 获取Sidebar的RectTransform
        var sidebarRect = sidebar.GetComponent<RectTransform>();

        // 保存按钮引用
        tabButtons = new();

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

            size = sidebarRect.rect.width - 7f;
            rectTransform.anchoredPosition = new Vector2(0, -(size * numItem) - 5f); // 计算按钮位置
            rectTransform.sizeDelta = new Vector2(size, size); // 设置按钮大小

            buttonImage.sprite = Utils.LoadSprite($"TONEX.Resources.Images.TabIcon_{tab}.png", 100f);

            buttonComponent.onClick.AddListener(new Action(() => OnTabButtonClicked(tab)));
            numItem++;

            // 将光圈引用保存到按钮的Tag中
            newButton.tag = "Halo";

            // 将按钮添加到列表中
            tabButtons.Add(buttonComponent, false);
            RefreshSettings(tab);
            OnTabButtonClicked(tab);
        }
    }

    void OnTabButtonClicked(TabGroup tab)
    {
        // 遍历所有按钮，更新它们的外观以反映当前开启的标签页
        foreach (var pair in tabButtons)
        {
            var button = pair.Key;
            var isOn = pair.Value;

            if (button.name.Equals(tab.ToString()))
            {
                // 选中状态
                var rectTransform = button.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(size * 1.1f, size * 1.1f); // 放大按钮
                tabButtons[button] = true; // 更新按钮状态为选中
            }
            else
            {
                // 非选中状态
                var rectTransform = button.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(size, size); // 恢复原始大小
                tabButtons[button] = false; // 更新按钮状态为非选中
            }
        }
        UpdateHeaderText(tab);
    }
    void RefreshSettings(TabGroup tab)
    {
        
        CreateHeaderText(tab);
        CreateScrollbarAndSettingsContent(tab);
        CreateSettingItem(tab);
    }

    List<GameObject> Headers = new();
    void CreateHeaderText(TabGroup tab)
    {
       
        // 创建 HeaderText 对象
        var headerText = new GameObject($"HeaderTextFor{tab}");
        RectTransform headerRect = headerText.AddComponent<RectTransform>();
        headerText.AddComponent<CanvasRenderer>();

        Text textComponent = headerText.AddComponent<Text>();

        // 将 HeaderText 设置为 mainUI 的子对象
        headerRect.SetParent(mainUI.transform, false);

        // 设置 HeaderText 的尺寸
        headerRect.sizeDelta = new Vector2(800, 80); // 设定更大的宽度和高度

        // 设置 HeaderText 的锚点和位置，使其紧贴在 mainUI 的顶部和 sidebar 的边缘
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(0, 1);
        headerRect.pivot = new Vector2(0.5f, 1);

        // 计算 sidebar 的宽度
        RectTransform sidebarRect = sidebar.GetComponent<RectTransform>();
        float sidebarWidth = sidebarRect.rect.width;

        // 设置 HeaderText 的偏移量，使其贴着 sidebar 的右边缘
        headerRect.offsetMin = new Vector2(sidebarWidth, headerRect.offsetMin.y);
        headerRect.offsetMax = new Vector2(sidebarWidth + 100f, headerRect.offsetMax.y);

        // 根据 TabGroup 设置文本颜色
        string tabcolor;
        tabcolor = tab switch
        {
            TONEX.TabGroup.SystemSettings => Main.ModColor,
            TONEX.TabGroup.GameSettings => "#59ef83",
            TONEX.TabGroup.ImpostorRoles => Utils.GetCustomRoleTypeColorCode(TONEX.Roles.Core.CustomRoleTypes.Impostor),
            TONEX.TabGroup.CrewmateRoles => Utils.GetCustomRoleTypeColorCode(TONEX.Roles.Core.CustomRoleTypes.Crewmate),
            TONEX.TabGroup.NeutralRoles => Utils.GetCustomRoleTypeColorCode(TONEX.Roles.Core.CustomRoleTypes.Neutral),
            TONEX.TabGroup.Addons => Utils.GetCustomRoleTypeColorCode(TONEX.Roles.Core.CustomRoleTypes.Addon),
            TONEX.TabGroup.OtherRoles => "#76b8e0",
            _ => "#ffffff",
        };

        // 设置文本组件的属性
        textComponent.text = $"<color={tabcolor}>{GetString("TabGroup." + tab)}</color>";
        textComponent.alignment = TextAnchor.MiddleCenter; // 居中对齐
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 设置文本组件的字体大小
        textComponent.fontSize = 50;
        Headers.Add(headerText);
    }
    void UpdateHeaderText(TabGroup tab)
    {
        foreach (var head in Headers)
        {
            if (head.name == $"HeaderTextFor{tab}")
                head.SetActive(true);
            else
                head.SetActive(false);
        }
    }

    void CreateScrollbarAndSettingsContent(TONEX.TabGroup tab)
    {
        var scOptions = new List<OptionBehaviour>();
        foreach (var option in OptionItem.AllOptions)
        {
            if (option.Tab != tab) continue;
            if (option.OptionBehaviour == null)
            {
                GameObject settingsContent = new GameObject($"SettingsContent{tab}");
                settingsContent.AddComponent<RectTransform>();
                settingsContent.AddComponent<CanvasRenderer>();
                settingsContent.AddComponent<VerticalLayoutGroup>();

                settingsContent.GetComponent<RectTransform>().SetParent(mainUI.transform);
                settingsContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                settingsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * 0.7f, Screen.height * 0.8f);

                GameObject scrollbar = new GameObject($"Scrollbar{tab}");
                scrollbar.AddComponent<RectTransform>();
                scrollbar.AddComponent<CanvasRenderer>();
                scrollbar.AddComponent<Image>();
                scrollbar.AddComponent<UnityEngine.UI.Scrollbar>();

                scrollbar.GetComponent<RectTransform>().SetParent(mainUI.transform);
                scrollbar.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width * 0.35f - 10, 0);
                scrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(20, Screen.height * 0.8f);

                Image scrollbarImage = scrollbar.GetComponent<Image>();
                scrollbarImage.color = Color.white;

                UnityEngine.UI.Scrollbar scrollbarComponent = scrollbar.GetComponent<UnityEngine.UI.Scrollbar>();
                scrollbarComponent.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;

                // 关联滚动条和设置内容
                ScrollRect scrollRect = settingsContent.AddComponent<ScrollRect>();
                scrollRect.verticalScrollbar = scrollbarComponent;
                scrollRect.content = settingsContent.GetComponent<RectTransform>();
                //option.OptionBehaviour = stringOption;
            }
            option.OptionBehaviour.gameObject.SetActive(true);
        }
    }

    void CreateSettingItem(TabGroup tab)
    {
        foreach (var option in OptionItem.AllOptions)
        {
            if (tab != option.Tab) continue;
            if (option?.OptionBehaviour == null || option.OptionBehaviour.gameObject == null) continue;

            var enabled = true;
            var parent = option.Parent;

            enabled = AmongUsClient.Instance.AmHost &&
                !option.IsHiddenOn(Options.CurrentGameMode);

            var opt = option.OptionBehaviour.transform.Find("Background").GetComponent<SpriteRenderer>();
            opt.size = new(5.0f, 0.45f);
            while (parent != null && enabled)
            {
                enabled = parent.GetBool() && !parent.IsHiddenOn(Options.CurrentGameMode);
                parent = parent.Parent;
                opt.color = new(0f, 1f, 0f);
                opt.size = new(4.8f, 0.45f);
                opt.transform.localPosition = new Vector3(0.11f, 0f);
                option.OptionBehaviour.transform.Find("Title_TMP").transform.localPosition = new Vector3(-1.08f, 0f);
                option.OptionBehaviour.transform.FindChild("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(5.1f, 0.28f);
                if (option.Parent?.Parent != null)
                {
                    opt.color = new(0f, 0f, 1f);
                    opt.size = new(4.6f, 0.45f);
                    opt.transform.localPosition = new Vector3(0.24f, 0f);
                    option.OptionBehaviour.transform.Find("Title_TMP").transform.localPosition = new Vector3(-0.88f, 0f);
                    option.OptionBehaviour.transform.FindChild("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(4.9f, 0.28f);
                    if (option.Parent?.Parent?.Parent != null)
                    {
                        opt.color = new(1f, 0f, 0f);
                        opt.size = new(4.4f, 0.45f);
                        opt.transform.localPosition = new Vector3(0.37f, 0f);
                        option.OptionBehaviour.transform.Find("Title_TMP").transform.localPosition = new Vector3(-0.68f, 0f);
                        option.OptionBehaviour.transform.FindChild("Title_TMP").GetComponent<RectTransform>().sizeDelta = new Vector2(4.7f, 0.28f);
                    }
                }


                if (option.IsText)
                {
                    opt.color = new(0, 0, 0);
                    opt.transform.localPosition = new(100f, 100f, 100f);
                }

            }

                var settingText = "";
            // 创建新的设置项
            GameObject newItem = new GameObject(settingText);
            newItem.AddComponent<RectTransform>();
            newItem.AddComponent<CanvasRenderer>();

            Text textComponent = newItem.AddComponent<Text>();

            // 设置新项的父级和尺寸
            RectTransform newItemRectTransform = newItem.GetComponent<RectTransform>();
            newItemRectTransform.SetParent(mainUI.transform.Find($"SettingsContent{tab}"));
            newItemRectTransform.sizeDelta = new Vector2(Screen.width * 0.68f, 50);

            textComponent.text = settingText;
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.color = Color.white;

            // 创建按钮
            GameObject buttonObject = new GameObject("Button");
            buttonObject.AddComponent<RectTransform>();
            buttonObject.AddComponent<CanvasRenderer>();
            Button buttonComponent = buttonObject.AddComponent<Button>();
            Image buttonImage = buttonObject.AddComponent<Image>(); // 添加Image组件以使按钮可见

            // 设置按钮的父级和尺寸
            RectTransform buttonRectTransform = buttonObject.GetComponent<RectTransform>();
            buttonRectTransform.SetParent(newItem.transform); // 将按钮设置为newItem的子对象
            buttonRectTransform.anchorMin = new Vector2(0, 0);
            buttonRectTransform.anchorMax = new Vector2(0, 1);
            buttonRectTransform.pivot = new Vector2(0, 0.5f);
            buttonRectTransform.sizeDelta = new Vector2(50, 50); // 设置按钮的大小
            buttonRectTransform.anchoredPosition = new Vector2(25, 0); // 确保按钮位置居中对齐

            // 设置按钮的图像和点击事件
            buttonImage.color = Color.gray; // 设置按钮颜色，如果需要也可以加载其他图片
            buttonComponent.onClick.AddListener(new Action(() => OnButtonClick(settingText)));
        }
    }

    void OnButtonClick(string settingText)
    {
        Debug.Log("Button clicked for setting: " + settingText);
        // 在这里处理按钮点击事件
    }


}

