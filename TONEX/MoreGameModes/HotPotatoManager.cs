using HarmonyLib;
using System.Linq;
using UnityEngine;
using TONEX.Roles.Core;
using static TONEX.Translator;
using TONEX.Attributes;
using TONEX.Roles.AddOns.Common;
using Hazel;

namespace TONEX.MoreGameModes;

internal static class HotPotatoManager
{
    public static int RemainRoundTime = new();
    public static int RemainExplosionTime = new();
    public static int HotPotatoMax = new();

    public static OptionItem HotPotatoMaxNum;
    public static OptionItem ExplosionTotalTime;
    public static OptionItem RoundTotalTime;

    public static void SetupCustomOption()
    {
        HotPotatoMaxNum = IntegerOptionItem.Create(62_293_009, "HotPotatoMaxNum", new(1, 4, 1), 2, TabGroup.ModSettings, false)
           .SetGameMode(CustomGameMode.HotPotato)
           .SetColor(new Color32(245, 82, 82, byte.MaxValue))
           .SetHeader(true)
           .SetValueFormat(OptionFormat.Players);
        ExplosionTotalTime = IntegerOptionItem.Create(62_293_008, "ExplosionTotalTime", new(10, 60, 5), 15, TabGroup.ModSettings, false)
           .SetGameMode(CustomGameMode.HotPotato)
           .SetColor(new Color32(245, 82, 82, byte.MaxValue))
           .SetValueFormat(OptionFormat.Seconds);
        RoundTotalTime = IntegerOptionItem.Create(62_293_010, "RoundTotalTime", new(100, 300, 25), 150, TabGroup.ModSettings, false)
          .SetGameMode(CustomGameMode.HotPotato)
          .SetColor(new Color32(245, 82, 82, byte.MaxValue))
          .SetValueFormat(OptionFormat.Seconds);
    }
    [GameModuleInitializer]
    public static void Init()
    {
        if (Options.CurrentGameMode != CustomGameMode.HotPotato) return;
        RemainExplosionTime = ExplosionTotalTime.GetInt() + 9;
        RemainRoundTime = RoundTotalTime.GetInt() + 9;
        HotPotatoMax = HotPotatoMaxNum.GetInt();
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FixedUpdatePatch
    {
        
    private static long LastFixedUpdate = new();
        public static void Postfix(PlayerControl __instance)
        {
            if (/* Main.AssistivePluginMode.Value */ false) return;
            if (!GameStates.IsInTask || Options.CurrentGameMode != CustomGameMode.HotPotato || !AmongUsClient.Instance.AmHost || Main.AllAlivePlayerControls.ToList().Count == 0) return;
            //一些巴拉巴拉的东西
            var playerList = Main.AllAlivePlayerControls.ToList();
            //土豆数量检测
            if ((playerList.Count >= 9 && playerList.Count <= 11 && HotPotatoMax >= 3) ||(playerList.Count >= 5 && playerList.Count <= 7 && HotPotatoMax >= 2))
            {
                HotPotatoMax -= 1;
            }
            if (playerList.Count <= HotPotatoMax + 1)
            {
                HotPotatoMax = 1;
            }
            //爆炸时间为0时
            if (RemainExplosionTime <= 0)
            {

                foreach (var pc in Main.AllAlivePlayerControls)
                {
                    if (pc.Is(CustomRoles.HotPotato))
                    {
                        pc.RpcMurderPlayerV2(pc);
                        pc.Notify(GetString("HotPotatoExplosion"));
                    }
                    else
                        pc.Notify(GetString("ReadyToSelectHot"));
                    Logger.Info($"炸死一群", "HotPotato");
                }
                for (int i = 0; i < HotPotatoMax; i++)
                {
                    var pcList = Main.AllAlivePlayerControls.Where(x => x.GetCustomRole() != CustomRoles.HotPotato).ToList();
                    var HP = pcList[IRandom.Instance.Next(0, pcList.Count - 1)];
                    HP.RpcSetCustomRole(CustomRoles.HotPotato);
                    HP.Notify(GetString("GetHotPotato"), 1f);
                    Logger.Info($"分配热土豆", "HotPotato");

                }
                RemainExplosionTime = ExplosionTotalTime.GetInt();
            }
            if (LastFixedUpdate == Utils.GetTimeStamp()) return;
            LastFixedUpdate = Utils.GetTimeStamp();
            //减少爆炸冷却
            RemainExplosionTime--;
            RemainRoundTime--;
        }

    }
}

