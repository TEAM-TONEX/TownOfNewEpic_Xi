using HarmonyLib;
using Hazel;
using System.Linq;
using TONEX.Modules;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX;

[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
internal class ControllerManagerUpdatePatch
{
    private static readonly (int, int)[] resolutions = { (480, 270), (640, 360), (800, 450), (1280, 720), (1600, 900), (1920, 1080) };
    private static int resolutionIndex = 0;

    public static void Postfix(ControllerManager __instance)
    {
        //切换自定义设置的页面
        if (GameStates.IsLobby && !ChatUpdatePatch.Active)
        {
            //カスタム設定切り替え
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Input.GetKey(KeyCode.LeftControl)) OptionShower.Previous();
                else OptionShower.Next();
                OptionShowerPatch.Scroller.ScrollToTop();
            }
            for (var i = 0; i < 9; i++)
            {
                if (ORGetKeysDown(KeyCode.Alpha1 + i, KeyCode.Keypad1 + i) && OptionShower.pages.Count >= i + 1)
                {
                    OptionShower.currentPage = i;
                    OptionShowerPatch.Scroller.ScrollToTop();
                }
            }
            // 現在の設定を文字列形式のデータに変換してコピー
            if (GetKeysDown(KeyCode.O, KeyCode.LeftAlt))
            {
                OptionSerializer.SaveToClipboard();
            }
            // 現在の設定を文字列形式のデータに変換してファイルに出力
            if (GetKeysDown(KeyCode.L, KeyCode.LeftAlt))
            {
                OptionSerializer.SaveToFile();
            }
            // クリップボードから文字列形式の設定データを読み込む
            if (GetKeysDown(KeyCode.P, KeyCode.LeftAlt))
            {
                OptionSerializer.LoadFromClipboard();
            }
        }
        //职业介绍
        if (GameStates.IsInGame && (GameStates.IsCanMove || GameStates.IsMeeting))
        {
            if (Input.GetKey(KeyCode.F1))
            {
                if (!InGameRoleInfoMenu.Showing)
                    InGameRoleInfoMenu.SetRoleInfoRef(PlayerControl.LocalPlayer);
                InGameRoleInfoMenu.Show();
            }
            else InGameRoleInfoMenu.Hide();
        }
        else InGameRoleInfoMenu.Hide();
        //更改分辨率
        if (Input.GetKeyDown(KeyCode.F11))
        {
            resolutionIndex++;
            if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
            ResolutionManager.SetResolution(resolutions[resolutionIndex].Item1, resolutions[resolutionIndex].Item2, false);
        }
        //重新加载自定义翻译
        if (GetKeysDown(KeyCode.F5, KeyCode.T))
        {
            Logger.Info("加载自定义翻译文件", "KeyCommand");
            Translator.LoadLangs();
            Logger.SendInGame("Reloaded Custom Translation File");
        }
        if (GetKeysDown(KeyCode.F5, KeyCode.X))
        {
            Logger.Info("导出自定义翻译文件", "KeyCommand");
            Translator.ExportCustomTranslation();
            Logger.SendInGame("Exported Custom Translation File");
        }
        //日志文件转储
        if (GetKeysDown(KeyCode.F1, KeyCode.LeftControl))
        {
            Logger.Info("输出日志", "KeyCommand");
            Utils.DumpLog();
        }
        //将当前设置复制为文本
        if (GetKeysDown(KeyCode.LeftAlt, KeyCode.C) && !Input.GetKey(KeyCode.LeftShift) && !GameStates.IsNotJoined)
        {
            Utils.CopyCurrentSettings();
        }
        //打开游戏目录
        if (GetKeysDown(KeyCode.F10))
        {
            Utils.OpenDirectory(System.Environment.CurrentDirectory);
        }

        //-- 下面是主机专用的命令--//
        if (!AmongUsClient.Instance.AmHost) return;
        // 强制显示聊天框
        if (GetKeysDown(KeyCode.Return, KeyCode.C, KeyCode.LeftShift))
        {
            HudManager.Instance.Chat.SetVisible(true);
        }
        //强制结束游戏
        if (GetKeysDown(KeyCode.Return, KeyCode.L, KeyCode.LeftShift) && GameStates.IsInGame)
        {
            CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Draw);
            GameManager.Instance.LogicFlow.CheckEndCriteria();
        }
        //强制结束会议或召开会议
        if (GetKeysDown(KeyCode.Return, KeyCode.M, KeyCode.LeftShift) && GameStates.IsInGame)
        {
            if (GameStates.IsMeeting) MeetingHud.Instance.RpcClose();
            else PlayerControl.LocalPlayer.NoCheckStartMeeting(null, true);
        }
        //立即开始
        if (Input.GetKeyDown(KeyCode.LeftShift) && GameStates.IsCountDown)
        {
            Logger.Info("倒计时修改为0", "KeyCommand");
            GameStartManager.Instance.countDownTimer = 0;
        }
        //倒计时取消
        if (Input.GetKeyDown(KeyCode.C) && GameStates.IsCountDown)
        {
            Logger.Info("重置倒计时", "KeyCommand");
            GameStartManager.Instance.ResetStartState();
        }
        //显示当前有效设置的说明
        if (GetKeysDown(KeyCode.N, KeyCode.LeftShift, KeyCode.LeftControl))
        {
            Utils.ShowActiveSettingsHelp();
        }
        //显示当前有效设置
        if (GetKeysDown(KeyCode.N, KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
        {
            Utils.ShowActiveSettings();
        }
        //将 TONEX 选项设置为默认值
        if (GetKeysDown(KeyCode.Delete, KeyCode.LeftControl))
        {
            OptionItem.AllOptions.ToArray().Where(x => x.Id > 0).Do(x => x.SetValue(x.DefaultValue, false));
            Logger.SendInGame(GetString("RestTONEXSetting"));
            if (!(!AmongUsClient.Instance.AmHost || PlayerControl.AllPlayerControls.Count <= 1 || (AmongUsClient.Instance.AmHost == false && PlayerControl.LocalPlayer == null)))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RestTONEXSetting, SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            OptionShower.BuildText();
        }
        //放逐自己
        if (GetKeysDown(KeyCode.Return, KeyCode.E, KeyCode.LeftShift) && GameStates.IsInGame)
        {
            var state = PlayerState.GetByPlayerId(PlayerControl.LocalPlayer.PlayerId);
            PlayerControl.LocalPlayer.Data.IsDead = true;
            state.DeathReason = CustomDeathReason.etc;
            PlayerControl.LocalPlayer.RpcExileV2();
            state.SetDead();
            Utils.SendMessage(GetString("HostKillSelfByCommand"), title: $"<color=#ff0000>{GetString("DefaultSystemMessageTitle")}</color>");
        }
        //切换日志是否也在游戏中输出
        if (GetKeysDown(KeyCode.F2, KeyCode.LeftControl))
        {
            Logger.isAlsoInGame = !Logger.isAlsoInGame;
            Logger.SendInGame($"游戏中输出日志：{Logger.isAlsoInGame}");
        }

        //--下面是调试模式的命令--//
        if (!DebugModeManager.IsDebugMode) return;

        //杀戮闪烁
        if (GetKeysDown(KeyCode.Return, KeyCode.F, KeyCode.LeftShift))
        {
            Utils.FlashColor(new(1f, 0f, 0f, 0.3f));
            if (Constants.ShouldPlaySfx()) RPC.PlaySound(PlayerControl.LocalPlayer.PlayerId, Sounds.KillSound);
        }

        //实名投票
        if (GetKeysDown(KeyCode.Return, KeyCode.V, KeyCode.LeftShift) && GameStates.IsMeeting && !GameStates.IsOnlineGame)
        {
            MeetingHud.Instance.RpcClearVote(AmongUsClient.Instance.ClientId);
        }

        //打开飞艇所有的门
        if (GetKeysDown(KeyCode.Return, KeyCode.D, KeyCode.LeftShift) && GameStates.IsInGame)
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, 79);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, 80);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, 81);
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, 82);
        }

        //将击杀冷却设定为0秒
        if (GetKeysDown(KeyCode.Return, KeyCode.K, KeyCode.LeftShift) && GameStates.IsInGame)
        {
            PlayerControl.LocalPlayer.Data.Object.SetKillTimer(0f);
        }

        //同步全部选项
        if (Input.GetKeyDown(KeyCode.Y))
        {
            RPC.SyncCustomSettingsRPC();
            Logger.SendInGame(GetString("SyncCustomSettingsRPC"));
        }

        //开场动画测试
        if (Input.GetKeyDown(KeyCode.G))
        {
            HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black));
            HudManager.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro());
        }
        //用任务面板显示设施信息
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Main.VisibleTasksCount = !Main.VisibleTasksCount;
            DestroyableSingleton<HudManager>.Instance.Notifier.AddItem("VisibleTaskCountが" + Main.VisibleTasksCount.ToString() + "に変更されました。");
        }

        //获取现在的坐标
        if (Input.GetKeyDown(KeyCode.I))
            Logger.Info(PlayerControl.LocalPlayer.GetTruePosition().ToString(), "GetLocalPlayerPos");

        //マスゲーム用コード
        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.AmOwner) pc.MyPhysics.RpcEnterVent(2);
            }
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            Vector2 pos = PlayerControl.LocalPlayer.NetTransform.transform.position;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.AmOwner)
                {
                    pc.RpcTeleport(pos);
                    pos.x += 0.5f;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc.AmOwner) pc.MyPhysics.RpcExitVent(2);
            }
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            VentilationSystem.Update(VentilationSystem.Operation.StartCleaning, 0);
        }
        //マスゲーム用コード終わり
    }

    private static bool GetKeysDown(params KeyCode[] keys)
    {
        if (keys.Any(Input.GetKeyDown) && keys.All(Input.GetKey))
        {
            Logger.Info($"快捷键：{keys.Where(Input.GetKeyDown).First()} in [{string.Join(",", keys)}]", "GetKeysDown");
            return true;
        }
        return false;
    }

    private static bool ORGetKeysDown(params KeyCode[] keys) => keys.Any(k => Input.GetKeyDown(k));
}

[HarmonyPatch(typeof(ConsoleJoystick), nameof(ConsoleJoystick.HandleHUD))]
internal class ConsoleJoystickHandleHUDPatch
{
    public static void Postfix()
    {
        HandleHUDPatch.Postfix(ConsoleJoystick.player);
    }
}
[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.HandleHud))]
internal class KeyboardJoystickHandleHUDPatch
{
    public static void Postfix()
    {
        HandleHUDPatch.Postfix(KeyboardJoystick.player);
    }
}

internal class HandleHUDPatch
{
    public static void Postfix(Rewired.Player player)
    {
        if (player.GetButtonDown(8) && // 8:キルボタンのactionId
        PlayerControl.LocalPlayer.Data?.Role?.IsImpostor == false &&
        PlayerControl.LocalPlayer.CanUseKillButton())
        {
            DestroyableSingleton<HudManager>.Instance.KillButton.DoClick();
        }
        if (player.GetButtonDown(50) && // 50:インポスターのベントボタンのactionId
        PlayerControl.LocalPlayer.Data?.Role?.IsImpostor == false &&
        PlayerControl.LocalPlayer.CanUseImpostorVentButton())
        {
            DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
        }
        if (player.GetButtonDown(49) && // 49:变形按钮id
        PlayerControl.LocalPlayer.Data?.Role?.IsImpostor == false &&
        PlayerControl.LocalPlayer.CanUseShapeShiftButton())
        {
            DestroyableSingleton<HudManager>.Instance.AbilityButton.DoClick();
        }
       

        /*
        用于侦测玩家按下了哪个按钮id
        int minButtonId = 1; // 最小按钮ID
        int maxButtonId = 100; // 最大按钮ID

        for (int buttonId = minButtonId; buttonId <= maxButtonId; buttonId++)
        {
            // 排除"Kill"和"Vent"按钮
            if (buttonId == 8 || buttonId == 50 || !GameStates.IsInTask)
                continue;

            bool isButtonPressed = player.GetButtonDown(buttonId);
            if (isButtonPressed)
            {
                Logger.Info($"Button with ID {buttonId} is pressed", "");
            }
        }
        */
    }
}