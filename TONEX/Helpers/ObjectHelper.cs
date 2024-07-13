using HarmonyLib;
using UnityEngine;

namespace TONEX;

public static class ObjectHelper
{
    /// <summary>
    /// 销毁对象的<see cref="TextTranslatorTMP"/>组件
    /// </summary>

    public static void DestroyTranslator(this GameObject obj)
    {
        if (obj == null) return;
        obj.ForEachChild((Il2CppSystem.Action<GameObject>)DestroyTranslator);
        TextTranslatorTMP[] translator = obj.GetComponentsInChildren<TextTranslatorTMP>(true);
        translator?.Do(Object.Destroy);
    }
    /// <summary>
    /// 销毁对象的 <see cref="TextTranslatorTMP"/> 组件
    /// </summary>
    public static void DestroyTranslator(this MonoBehaviour obj) => obj?.gameObject?.DestroyTranslator();
}
