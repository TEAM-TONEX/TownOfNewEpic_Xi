using HarmonyLib;
using System.Linq;
using UnityEngine;
using TONEX.Roles.Core;
using static TONEX.Translator;
using TONEX.Attributes;
using TONEX.Roles.AddOns.Common;
using Hazel;
using System.Collections.Generic;

namespace TONEX.MoreGameModes;

internal static class InfectorManager
{
    public static List<byte> ZombiePlayers;
    public static List<byte> HumanCompleteTasks;
    public static List<byte> HumanNum;
    public static int RemainRoundTime = new();

    public static OptionItem RoundTotalTime;
    public static void SetupCustomOption()
    {
        RoundTotalTime = IntegerOptionItem.Create(62_293_010, "RoundTotalTime", new(100, 300, 25), 150, TabGroup.ModSettings, false)
          .SetGameMode(CustomGameMode.InfectorMode)
          .SetColor(new Color32(245, 82, 82, byte.MaxValue))
          .SetValueFormat(OptionFormat.Seconds);
    }
    [GameModuleInitializer]
    public static void Init()
    {
        if (Options.CurrentGameMode != CustomGameMode.InfectorMode) return;
        RemainRoundTime = RoundTotalTime.GetInt() + 9;
        ZombiePlayers = new();
        HumanCompleteTasks = new();
        HumanNum = new();
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FixedUpdatePatch
    {
        
    private static long LastFixedUpdate = new();
        public static void Postfix(PlayerControl __instance)
        {
            if (Main.AssistivePluginMode.Value) return;
            if (!GameStates.IsInTask || Options.CurrentGameMode != CustomGameMode.InfectorMode || !AmongUsClient.Instance.AmHost || Main.AllAlivePlayerControls.ToList().Count == 0) return;

            foreach (var player in Main.AllAlivePlayerControls)
            {
                if (player.Is(CustomRoles.Infector) && !ZombiePlayers.Contains(player.PlayerId))
                {
                    HumanNum.Remove(player.PlayerId);
                    ZombiePlayers.Add(player.PlayerId);
                }
                if (player.Is(CustomRoles.Survivor) && !HumanNum.Contains(player.PlayerId))
                {
                    HumanNum.Add(player.PlayerId);
                }
            }


            if (LastFixedUpdate == Utils.GetTimeStamp()) return;
            LastFixedUpdate = Utils.GetTimeStamp();

            RemainRoundTime--;
            Utils.NotifyRoles();
        }

    }
}

