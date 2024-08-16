using AmongUs.Data;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TONEX.Roles.Core;
using TONEX.Roles.Neutral;

namespace TONEX;

class ExileControllerWrapUpPatch
{
    public static List<Action> ActionsOnWrapUp = new();
    public static NetworkedPlayerInfo AntiBlackout_LastExiled;
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            if (Main.AssistivePluginMode.Value) return;
            try
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
            finally
            {
                WrapUpFinalizer(__instance.initData.networkedPlayer);
            }
        }
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    class AirshipExileControllerPatch
    {
        public static void Postfix(AirshipExileController __instance)
        {
            if (Main.AssistivePluginMode.Value) return;
            try
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
            finally
            {
                WrapUpFinalizer(__instance.initData.networkedPlayer);
            }
        }
    }
    static void WrapUpPostfix(NetworkedPlayerInfo exiled)
    {
        if (Main.AssistivePluginMode.Value) return;
        if (AntiBlackout.OverrideExiledPlayer)
        {
            exiled = AntiBlackout_LastExiled;
        }

        var mapId = Main.NormalOptions.MapId;
        // エアシップではまだ湧かない
        if ((MapNames)mapId != MapNames.Airship)
        {
            foreach (var state in PlayerState.AllPlayerStates.Values)
            {
                state.HasSpawned = true;
            }
        }

        bool DecidedWinner = false;
        if (!AmongUsClient.Instance.AmHost) return; //ホスト以外はこれ以降の処理を実行しません
        AntiBlackout.RestoreIsDead(doSend: false);
        if (exiled != null)
        {
            var role = exiled.GetCustomRole();
            var info = role.GetRoleInfo();
            //霊界用暗転バグ対処
            if (!AntiBlackout.OverrideExiledPlayer && info?.IsDesyncImpostor == true)
                exiled.Object?.ResetPlayerCam(1f);

            exiled.IsDead = true;
            PlayerState.GetByPlayerId(exiled.PlayerId).DeathReason = CustomDeathReason.Vote;

            ActionsOnWrapUp.Do(f => f.Invoke());
            ActionsOnWrapUp = new();

            foreach (var roleClass in CustomRoleManager.AllActiveRoles.Values)
            {
                roleClass.OnExileWrapUp(exiled, ref DecidedWinner);
                var now = Utils.GetTimeStamp();

                if (roleClass.Player.IsAlive())
                {
                    for (int i = 0; i < roleClass.CountdownList.Count; i++)
                    {
                        roleClass.CountdownList[i] = now;
                    }
                    roleClass.UsePetCooldown_Timer = now;
                }

            }
            foreach (var roleClass in CustomRoleManager.AllActiveAddons.Values)
            {
                roleClass.Do_Addons(x=>x.OnExileWrapUp(exiled, ref DecidedWinner));
                var now = Utils.GetTimeStamp();

                roleClass.Do_Addons(x =>
                {
                    for (int i = 0; i < x.CountdownList.Count; i++)
                    {
                        x.CountdownList[i] = now;
                    }
                    x.UsePetCooldown_Timer = now;
                });

            }
            if (CustomWinnerHolder.WinnerTeam != CustomWinner.Terrorist) PlayerState.GetByPlayerId(exiled.PlayerId).SetDead();
        }

        foreach (var pc in Main.AllPlayerControls)
        {
            pc.ResetKillCooldown();
        }
        if (RandomSpawn.IsRandomSpawn())
        {
            RandomSpawn.SpawnMap map;
                switch (mapId)
            {
                case 0:
                    map = new RandomSpawn.SkeldSpawnMap();
                    Main.AllPlayerControls.Do(map.RandomTeleport);
                    break;
                case 1:
                    map = new RandomSpawn.MiraHQSpawnMap();
                    Main.AllPlayerControls.Do(map.RandomTeleport);
                    break;
                case 2:
                    map = new RandomSpawn.PolusSpawnMap();
                    Main.AllPlayerControls.Do(map.RandomTeleport);
                    break;
                case 5:
                    map = new RandomSpawn.FungleSpawnMap();
                    Main.AllPlayerControls.Do(map.RandomTeleport);
                    break;
            }
        }
        FallFromLadder.Reset();
        Utils.CountAlivePlayers(true);
        Utils.AfterMeetingTasks();
        Utils.SyncAllSettings();
        Utils.NotifyRoles();
    }

    static void WrapUpFinalizer(NetworkedPlayerInfo exiled)
    {
        //WrapUpPostfixで例外が発生しても、この部分だけは確実に実行されます。
        if (AmongUsClient.Instance.AmHost)
        {
            _ = new LateTask(() =>
            {
                exiled = AntiBlackout_LastExiled;
                AntiBlackout.SendGameData();
                if (AntiBlackout.OverrideExiledPlayer && // 追放対象が上書きされる状態 (上書きされない状態なら実行不要)
                    exiled != null && //exiledがnullでない
                    exiled.Object != null) //exiled.Objectがnullでない
                {
                    exiled.Object.RpcExileV2();
                }
            }, 0.5f, "Restore IsDead Task");
            _ = new LateTask(() =>
            {
                Main.AfterMeetingDeathPlayers.Do(x =>
                {
                    var player = Utils.GetPlayerById(x.Key);
                    var roleClass = CustomRoleManager.GetRoleBaseByPlayerId(x.Key);
                    var requireResetCam = player?.GetCustomRole().GetRoleInfo()?.IsDesyncImpostor == true;
                    var state = PlayerState.GetByPlayerId(x.Key);
                    Logger.Info($"{player.GetNameWithRole()}を{x.Value}で死亡させました", "AfterMeetingDeath");
                    state.DeathReason = x.Value;
                    state.SetDead();
                    player?.RpcExileV2();
                    if (x.Value == CustomDeathReason.Suicide)
                        player?.SetRealKiller(player, true);
                    if (requireResetCam)
                        player?.ResetPlayerCam(1f);
                });
                Main.AfterMeetingDeathPlayers.Clear();
            }, 0.5f, "AfterMeetingDeathPlayers Task");
        }

        GameStates.AlreadyDied |= !Utils.IsAllAlive;
        RemoveDisableDevicesPatch.UpdateDisableDevices();
        SoundManager.Instance.ChangeAmbienceVolume(DataManager.Settings.Audio.AmbienceVolume);
        Logger.Info("タスクフェイズ開始", "Phase");
    }
}

[HarmonyPatch(typeof(PbExileController), nameof(PbExileController.PlayerSpin))]
class PolusExileHatFixPatch
{
    public static void Prefix(PbExileController __instance)
    {
        __instance.Player.cosmetics.hat.transform.localPosition = new(-0.2f, 0.6f, 1.1f);
    }
}