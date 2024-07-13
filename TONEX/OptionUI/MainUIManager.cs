using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static TONEX.Translator;
using static Il2CppSystem.Uri;
using System.Linq;

namespace TONEX.OptionUI;
public class MainUIManager : SidebarManager
{
    public static MainUIManager Instance;
    public new void Awake()

    {
        Init();
        DontDestroyOnLoad(gameObject);
    }
    public override void Init()
    {
        Debug.Log("Init Started");
        if (Instance != null && Instance != this)
            Destroy(Instance);
        if (Instance == null)
        {
            Instance = this;

        }

    }
    public void CreateMainUI()
    {
        GetInstance().mainUI = new GameObject("MainUI");

        GetInstance().mainUI.AddComponent<RectTransform>();
        GetInstance().mainUI.AddComponent<CanvasRenderer>();
        Image image = GetInstance().mainUI.AddComponent<Image>();

        GetInstance().mainUI.GetComponent<RectTransform>().SetParent(GetInstance().canvas.transform);
        GetInstance().mainUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        GetInstance().mainUI.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * 0.8f, Screen.height * 0.8f);

        image.sprite = Utils.LoadSprite($"TONEX.Resources.Images.UI.BackGround.png", 100f);
        GetInstance().mainUI.SetActive(false); // 初始状态下隐藏

        CreateUIComponent();
    }
    void CreateUIComponent()
    {
        CreateCloseButton();
        CreateSidebar();
    }
    void CreateCloseButton()
    {
        GetInstance().closeButton = new GameObject("CloseButton");

        RectTransform rectTransform = GetInstance().closeButton.AddComponent<RectTransform>();
        GetInstance().closeButton.AddComponent<CanvasRenderer>();
        var image = GetInstance().closeButton.AddComponent<Image>();
        image.sprite = Utils.LoadSprite("TONEX.Resources.Images.UI.Escape.png", 100f);
        Button buttonComponent = GetInstance().closeButton.AddComponent<Button>();

        // 设置父对象为 GetInstance().mainUI
        rectTransform.SetParent(GetInstance().mainUI.transform);

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
        GetInstance().mainUI.SetActive(false);
        GetInstance().openButton.SetActive(true);

    }

}
