using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using static TONEX.Translator;
using static Il2CppSystem.Uri;
using System.Linq;

namespace TONEX.OptionUI;
public class CanvasManager : UIBase
{
    public static CanvasManager Instance;
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
    public void CreateCanvas()
    {
        GetInstance().canvas = new GameObject("DynamicCanvas");

        Canvas _canvas = GetInstance().canvas.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = GetInstance().canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GetInstance().canvas.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = GetInstance().canvas.GetComponent<RectTransform>();
        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.anchoredPosition = Vector2.zero;

        canvasRect.sizeDelta = new Vector2(1920, 1080);

        if (GetInstance().canvas == null)
        {
            Debug.LogError("Canvas object not found!");
            return;
        }

    }

}
