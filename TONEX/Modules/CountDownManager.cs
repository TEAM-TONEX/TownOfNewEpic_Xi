using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TONEX.Modules;
public static class TimerManager
{
    private static List<float> countdowns = new List<float>();

    // 添加倒计时
    public static void AddCountdown(float time)
    {
        countdowns.Add(time);
    }

    // 在 FixedUpdate 中更新倒计时并同步至原始变量
    public static void UpdateCountdowns(ref float[] originalCountdowns)
    {
        for (int i = 0; i < countdowns.Count; i++)
        {
            countdowns[i] -= Time.fixedDeltaTime;

            if (countdowns[i] <= 0)
            {
                // 处理倒计时结束的逻辑，例如触发事件或执行特定操作
                countdowns.RemoveAt(i);
                Array.Resize(ref originalCountdowns, originalCountdowns.Length - 1);
                i--;
            }
        }

        // 同步倒计时数值至原始变量
        for (int i = 0; i < countdowns.Count; i++)
        {
            originalCountdowns[i] = countdowns[i];
        }
    }
}

