using System;
using UnityEngine;
using UnityEngine.UI;

namespace TONEX.OptionUI;

public class OpenButtonManager:UIBase
{
    public static OpenButtonManager Instance;
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
    public void CreateOpenButton()
    {
        GetInstance().openButton = new GameObject("OpenButton");
       

        RectTransform rectTransform = GetInstance().openButton.AddComponent<RectTransform>();
        GetInstance().openButton.AddComponent<CanvasRenderer>();
        Image image = GetInstance().openButton.AddComponent<Image>();

        Button buttonComponent = GetInstance().openButton.AddComponent<Button>();
        Debug.Log("6");
        rectTransform.SetParent(GetInstance().canvas.transform, false);
        Debug.Log("666");
        rectTransform.anchoredPosition = new Vector2(0, 0);
        // 计算长宽比为 4:3 的尺寸
        float desiredWidth = 200f;
        float desiredHeight = desiredWidth * (3f / 4f);

        // 设置长宽比为 4:3 的尺寸
        rectTransform.sizeDelta = new Vector2(desiredWidth, desiredHeight);


        var sprite = Utils.LoadSprite("TONEX.Resources.Images.UI.Open.png", 100f);
        image.sprite = sprite;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(GetInstance().openButton.transform);

        buttonComponent.onClick.AddListener(new Action(OnOpenButtonClick));

        GetInstance().openButton.SetActive(true);
    }
    void OnOpenButtonClick()
    {
        Debug.Log("Central Button Clicked");
        GetInstance().mainUI.SetActive(true);
        GetInstance().openButton.SetActive(false);
    }
}
