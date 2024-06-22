using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static TONEX.Translator;
using static Il2CppSystem.Uri;
using System.Linq;

namespace TONEX.OptionUI;

public class SettingItemManager : UIBase
{

    public void CreateScrollbarAndSettingsContent(TabGroup tab)
    {
        var scOptions = new List<OptionBehaviour>();
        foreach (var option in OptionItem.AllOptions)
        {
            if (option.Tab != tab) continue;
            continue;
            if (option.OptionBehaviour == null)
            {
                GameObject settingsContent = new GameObject($"SettingsContent{tab}");
                settingsContent.AddComponent<RectTransform>();
                settingsContent.AddComponent<CanvasRenderer>();
                settingsContent.AddComponent<VerticalLayoutGroup>();

                settingsContent.GetComponent<RectTransform>().SetParent(GetInstance().mainUI.transform);
                settingsContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                settingsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * 0.7f, Screen.height * 0.8f);

                GameObject scrollbar = new GameObject($"Scrollbar{tab}");
                scrollbar.AddComponent<RectTransform>();
                scrollbar.AddComponent<CanvasRenderer>();
                scrollbar.AddComponent<Image>();
                scrollbar.AddComponent<UnityEngine.UI.Scrollbar>();

                scrollbar.GetComponent<RectTransform>().SetParent(GetInstance().mainUI.transform);
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
    public void CreateSettingItem(TabGroup tab)
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
            newItemRectTransform.SetParent(GetInstance().mainUI.transform.Find($"SettingsContent{tab}"));
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
