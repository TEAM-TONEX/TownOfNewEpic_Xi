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

internal static class ZombieManager
{
    public static List<byte> ZombiePlayers;
    public static List<byte> HumanCompleteTasks;
    public static List<byte> HumanNum;
    public static int RemainRoundTime = new();

    public static OptionItem ShortTasksNum;
    public static OptionItem LongTasksNum;
    public static OptionItem RoundTotalTime;
    public static void SetupCustomOption()
    {
        ShortTasksNum = IntegerOptionItem.Create(62_293_011, "ShortTasksNum", new(1, 1, 10), 2, TabGroup.GameSettings, false)
          .SetGameMode(CustomGameMode.ZombieMode)
          .SetColor(new Color32(245, 82, 82, byte.MaxValue));
        LongTasksNum = IntegerOptionItem.Create(62_293_012, "LongTasksNum", new(1, 1, 10), 1, TabGroup.GameSettings, false)
          .SetGameMode(CustomGameMode.ZombieMode)
          .SetColor(new Color32(245, 82, 82, byte.MaxValue));
        RoundTotalTime = IntegerOptionItem.Create(62_293_010, "RoundTotalTime", new(100, 300, 25), 150, TabGroup.GameSettings, false)
          .SetGameMode(CustomGameMode.ZombieMode)
          .SetColor(new Color32(245, 82, 82, byte.MaxValue))
          .SetValueFormat(OptionFormat.Seconds);
    }
    [GameModuleInitializer]
    public static void Init()
    {
        if (Options.CurrentGameMode != CustomGameMode.ZombieMode) return;
        RemainRoundTime = RoundTotalTime.GetInt() + 9;
        ZombiePlayers = new();
        HumanCompleteTasks = new();
        HumanNum = new();
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FixedUpdatePatch
    {
        
    private static long LastFixedUpdate = new();
        //public static void Postfix(PlayerControl __instance)
        //{
        //    if (Main.AssistivePluginMode.Value) return;
        //    if (!GameStates.IsInTask || Options.CurrentGameMode != CustomGameMode.ZombieMode || !AmongUsClient.Instance.AmHost || Main.AllAlivePlayerControls.ToList().Count == 0) return;
            
        //    var playerList = Main.AllAlivePlayerControls.ToList();
        //    foreach(var player in Main.AllAlivePlayerControls) { 
        //    if(player.Is(CustomRoles.ZomBie) && !ZombiePlayers.Contains(player)) {
        //            HumanNum.Remove(player);
        //            ZombiePlayers.Add(player);
        //        }
        //        if (player.Is(CustomRoles.Human) && !HumanNum.Contains(player)) {
        //            HumanNum.Add(player);
        //        }
        //    }


        //    if (LastFixedUpdate == Utils.GetTimeStamp()) return;
        //    LastFixedUpdate = Utils.GetTimeStamp();
       
        //    RemainRoundTime--;
        //    Utils.NotifyRoles();
        //}

    }
}

