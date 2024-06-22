using UnityEngine;

namespace TONEX;

public static class ColorHelper
{
    /// <summary>将颜色转换为荧光笔颜色</summary>
    /// <param name="bright">是否将颜色调整为最大亮度。如果希望较暗的颜色保持不变，请传入 false</param>
    public static Color ToMarkingColor(this Color color, bool bright = true)
    {
        Color.RGBToHSV(color, out var h, out _, out var v);
        var markingColor = Color.HSVToRGB(h, MarkerSat, bright ? MarkerVal : v).SetAlpha(MarkerAlpha);
        return markingColor;
    }

    /// <summary>将颜色转换为适合在白色背景下保持可读性的颜色</summary>
    public static Color ToReadableColor(this Color color)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        // 如果饱和度不合适，则调整饱和度
        if (s < ReadableSat)
        {
            s = ReadableSat;
        }
        // 如果明度不合适，则调整明度
        if (v > ReadableVal)
        {
            v = ReadableVal;
        }
        return Color.HSVToRGB(h, s, v);
    }

    /// <summary>标记颜色的 S 值 = 饱和度</summary>
    private const float MarkerSat = 1f;
    /// <summary>标记颜色的 V 值 = 明度</summary>
    private const float MarkerVal = 1f;
    /// <summary>标记颜色的 Alpha 值 = 不透明度</summary>
    private const float MarkerAlpha = 0.2f;
    /// <summary>白色背景文本颜色的最大 S = 饱和度</summary>
    private const float ReadableSat = 0.8f;
    /// <summary>白色背景文本颜色的最大 V = 明度</summary>

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
    public static Color HexToColor(string hex)
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