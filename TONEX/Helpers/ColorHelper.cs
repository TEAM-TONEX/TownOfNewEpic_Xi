using UnityEngine;

namespace TONEX;

public static class ColorHelper
{
    /// <summary>蛍光マーカーのような色合いの透過色に変換する</summary>
    /// <param name="bright">最大明度にするかどうか．黒っぽい色を黒っぽいままにしたい場合はfalse</param>
    public static Color ToMarkingColor(this Color color, bool bright = true)
    {
        Color.RGBToHSV(color, out var h, out _, out var v);
        var markingColor = Color.HSVToRGB(h, MarkerSat, bright ? MarkerVal : v).SetAlpha(MarkerAlpha);
        return markingColor;
    }
    /// <summary>白背景での可読性を保てる色に変換する</summary>
    /// <summary>白背景での可読性を保てる色に変換する</summary>
    public static Color ToReadableColor(this Color color)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        // 適切な彩度でない場合は彩度を変更
        if (s < ReadableSat)
        {
            s = ReadableSat;
        }
        // 適切な明度でない場合は明度を変更
        if (v > ReadableVal)
        {
            v = ReadableVal;
        }
        return Color.HSVToRGB(h, s, v);
    }

    /// <summary>マーカー色のS値 = 彩度</summary>
    private const float MarkerSat = 1f;
    /// <summary>マーカー色のV値 = 明度</summary>
    private const float MarkerVal = 1f;
    /// <summary>マーカー色のアルファ = 不透明度</summary>
    private const float MarkerAlpha = 0.2f;
    /// <summary>白背景テキスト色の最大S = 彩度</summary>
    private const float ReadableSat = 0.8f;
    /// <summary>白背景テキスト色の最大V = 明度</summary>
    private const float ReadableVal = 0.8f;
    // 来源：https://github.com/dabao40/TheOtherRolesGMIA/blob/main/TheOtherRoles/Helpers.cs

    public static string GradientColorText(string startColorHex, string endColorHex, string text)
    {


        Color startColor = HexToColor(startColorHex);
        Color endColor = HexToColor(endColorHex);

        int textLength = text.Length;
        float stepR = (endColor.r - startColor.r) / (float)textLength;
        float stepG = (endColor.g - startColor.g) / (float)textLength;
        float stepB = (endColor.b - startColor.b) / (float)textLength;
        float stepA = (endColor.a - startColor.a) / (float)textLength;

        string gradientText = "";

        for (int i = 0; i < textLength; i++)
        {
            float r = startColor.r + (stepR * i);
            float g = startColor.g + (stepG * i);
            float b = startColor.b + (stepB * i);
            float a = startColor.a + (stepA * i);


            string colorhex = ColorToHex(new Color(r, g, b, a));
            gradientText += $"<color=#{colorhex}>{text[i]}</color>";

        }

        return gradientText;

    }
    private static Color HexToColor(string hex)
    {
        Color color = new();
        ColorUtility.TryParseHtmlString("#" + hex, out color);
        return color;
    }
    public static string ColorToHex(Color color)
    {
        Color32 color32 = (Color32)color;
        return $"{color32.r:X2}{color32.g:X2}{color32.b:X2}{color32.a:X2}";
    }
}

public class CameraPosition : MonoBehaviour
{
    public static void Update()
    {
        // 获取主摄像机对象
        Camera mainCamera = Camera.main;

        // 检查摄像机是否存在
        if (mainCamera != null)
        {
            // 获取摄像机的位置
            Vector3 cameraPosition = mainCamera.transform.position;

            // 输出摄像机位置
            Logger.Info("Camera Position: " + cameraPosition, "Camera");
        }
        else
        {
            Debug.LogError("Main camera not found!");
        }
    }
}