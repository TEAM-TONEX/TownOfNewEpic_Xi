using AmongUs.GameOptions;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX.Roles.Impostor;
public sealed class Sniper : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Sniper),
            player => new Sniper(player),
            CustomRoles.Sniper,
            () => RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            2400,
            SetupOptionItem,
            "snp|狙擊手|狙击"
        );
    public Sniper(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        MaxBulletCount = SniperBulletCount.GetInt();
        PrecisionShooting = SniperPrecisionShooting.GetBool();
        AimAssist = SniperAimAssist.GetBool();
        AimAssistOneshot = SniperAimAssistOnshot.GetBool();

        CustomRoleManager.MarkOthers.Add(GetMarkOthers);
    }

    public override void OnDestroy()
    {
        Snipers.Clear();
    }
    static OptionItem SniperBulletCount;
    static OptionItem SniperPrecisionShooting;
    static OptionItem SniperAimAssist;
    static OptionItem SniperAimAssistOnshot;
    static OptionItem SniperCanKill;
    enum OptionName
    {
        SniperBulletCount,
        SniperPrecisionShooting,
        SniperAimAssist,
        SniperAimAssistOneshot,
        SniperCanKill
    }
    Vector3 SnipeBasePosition;
    Vector3 LastPosition;
    int BulletCount;
    List<byte> ShotNotify = new();
    bool IsAim;
    float AimTime;

    static HashSet<Sniper> Snipers = new();

    int MaxBulletCount;
    bool PrecisionShooting;
    bool AimAssist;
    bool AimAssistOneshot;

    bool MeetingReset;
    public static void SetupOptionItem()
    {
        SniperBulletCount = IntegerOptionItem.Create(RoleInfo, 10, OptionName.SniperBulletCount, new(1, 5, 1), 2, false)
            .SetValueFormat(OptionFormat.Pieces);
        SniperPrecisionShooting = BooleanOptionItem.Create(RoleInfo, 11, OptionName.SniperPrecisionShooting, false, false);
        SniperAimAssist = BooleanOptionItem.Create(RoleInfo, 12, OptionName.SniperAimAssist, true, false);
        SniperAimAssistOnshot = BooleanOptionItem.Create(RoleInfo, 13, OptionName.SniperAimAssistOneshot, false, false, SniperAimAssist);
        SniperCanKill = BooleanOptionItem.Create(RoleInfo, 14, OptionName.SniperCanKill, false, false);
    }
    public override void Add()
    {
        Logger.Disable("Sniper");

        SnipeBasePosition = new();
        LastPosition = new();
        BulletCount = MaxBulletCount;
        ShotNotify.Clear();
        IsAim = false;
        AimTime = 0f;
        MeetingReset = false;

        Snipers.Add(this);
    }
    private void SendRPC()
    {
        Logger.Info($"{Player.GetNameWithRole()}:SendRPC", "Sniper");
        using var sender = CreateSender();

        var snList = ShotNotify;
        sender.Writer.Write(snList.Count);
        foreach (var sn in snList)
        {
            sender.Writer.Write(sn);
        }
    }

    public override void ReceiveRPC(MessageReader reader)
    {
        
        ShotNotify.Clear();
        var count = reader.ReadInt32();
        while (count > 0)
        {
            ShotNotify.Add(reader.ReadByte());
            count--;
        }
        Logger.Info($"{Player.GetNameWithRole()}:ReceiveRPC", "Sniper");
    }
    public bool CanUseKillButton()
    {
        return Player.IsAlive() && (BulletCount <= 0 || SniperCanKill.GetBool());
    }
    /// <summary>
    /// 狙撃の場合死因設定
    /// </summary>
    /// <param name="info"></param>
    public void OnMurderPlayerAsKiller(MurderInfo info)
    {
        //AttemptKillerは自分確定
        //スナイパーがAppearanceKillerだった場合は狙撃じゃない
        //ターゲットが自殺扱いなら狙撃
        if (!Is(info.AppearanceKiller) && info.IsFakeSuicide)
        {
            PlayerState.GetByPlayerId(info.AttemptTarget.PlayerId).DeathReason = CustomDeathReason.Sniped;
        }
    }

    Dictionary<PlayerControl, float> GetSnipeTargets()
    {
        // 创建一个空字典来存储狙击目标和它们的误差值
        var targets = new Dictionary<PlayerControl, float>();

        // 狙击起始位置
        var snipeBasePos = SnipeBasePosition;
        // 玩家当前位置
        var snipePos = Player.transform.position;
        // 狙击方向
        var dir = (snipePos - snipeBasePos).normalized;

        // 从略微后方开始检测，以防止在极近距离出现未命中的情况
        snipePos -= dir;

        // 遍历所有存活玩家的控制器
        foreach (var target in Main.AllAlivePlayerControls)
        {
            // 跳过自己
            if (target.PlayerId == Player.PlayerId) continue;

            // 计算目标相对于狙击位置的向量
            var target_pos = target.transform.position - snipePos;

            // 如果目标在狙击者的后方，跳过
            if (target_pos.magnitude < 1) continue;

            // 计算目标位置的单位方向向量
            var target_dir = target_pos.normalized;

            // 计算狙击方向和目标方向的点积
            var target_dot = Vector3.Dot(dir, target_dir);

            // 输出日志信息，显示目标玩家名称、位置向量和方向向量
            Logger.Info($"{target?.Data?.PlayerName}:pos={target_pos} dir={target_dir}", "Sniper");
            Logger.Info($"  Dot={target_dot}", "Sniper");

            // 如果点积足够接近于1，则说明目标在狙击方向上
            if (target_dot < 0.995) continue;

            // 如果是精准射击模式
            if (PrecisionShooting)
            {
                // 计算射线和目标位置之间的误差
                var err = Vector3.Cross(dir, target_pos).magnitude;
                Logger.Info($"  err={err}", "Sniper");

                // 如果误差小于0.5，则将目标添加到字典中
                if (err < 0.5)
                {
                    targets.Add(target, err);
                }
            }
            else
            {
                // 如果是普通射击模式，使用距离作为判定依据
                var err = target_pos.magnitude;
                Logger.Info($"  err={err}", "Sniper");
                targets.Add(target, err);
            }
        }

        // 返回狙击目标字典
        return targets;
    }
    public override void OnShapeshift(PlayerControl target)
    {
        var shapeshifting = !Is(target);

        if (BulletCount <= 0) return;

        //弾が残ってたら
        if (shapeshifting)
        {
            //Aim開始
            MeetingReset = false;

            //スナイプ地点の登録
            SnipeBasePosition = Player.transform.position;

            LastPosition = Player.transform.position;
            IsAim = true;
            AimTime = 0f;

            return;
        }

        //エイム終了
        IsAim = false;
        AimTime = 0f;

        //ミーティングによる変身解除なら射撃しない
        if (MeetingReset)
        {
            MeetingReset = false;
            return;
        }

        //一発消費して
        BulletCount--;

        //命中判定はホストのみ行う
        if (!AmongUsClient.Instance.AmHost) return;

        var targets = GetSnipeTargets();

        if (targets.Count != 0)
        {
            //一番正確な対象がターゲット
            var snipedTarget = targets.OrderBy(c => c.Value).First().Key;
            var killed = false;
            CustomRoleManager.OnCheckMurder(
                Player, snipedTarget,       // sniperがsnipedTargetを打ち抜く
                snipedTarget, snipedTarget,  // 表示上はsnipedTargetの自爆
                () => killed = true
            );

            //あたった通知
            Player.RPCPlayCustomSound("AWP");
            if (!Player.IsModClient() && killed) Player.RpcProtectedMurderPlayer();
            if (killed) RPC.PlaySoundRPC(Player.PlayerId, Sounds.KillSound);
            else Player.Notify(GetString("SniperKillFaild"));

            //スナイプが起きたことを聞こえそうな対象に通知したい
            targets.Remove(snipedTarget);
            var snList = ShotNotify;
            snList.Clear();
            foreach (var otherPc in targets.Keys)
            {
                snList.Add(otherPc.PlayerId);
                Utils.NotifyRoles(SpecifySeer: otherPc);
            }

            SendRPC();
            _ = new LateTask(
                () =>
                {
                    snList.Clear();
                    if (targets.Count != 0)
                    {
                        foreach (var otherPc in targets.Keys)
                        {
                            Utils.NotifyRoles(SpecifySeer: otherPc);
                        }
                        SendRPC();
                    }
                },
                0.5f, "Sniper shot Notify"
                );
        }
    }
    public override bool GetGameStartSound(out string sound)
    {
        sound = "AWP";
        return true;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!Player.IsAlive()) return;

        if (!AimAssist) return;

        if (!IsAim) return;

        if (!GameStates.IsInTask)
        {
            //エイム終了
            IsAim = false;
            AimTime = 0f;
            return;
        }

        var pos = Player.transform.position;
        if (pos != LastPosition)
        {
            AimTime = 0f;
            LastPosition = pos;
        }
        else
        {
            AimTime += Time.fixedDeltaTime;
            Utils.NotifyRoles(SpecifySeer: Player);
        }
    }
    public override void OnReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        MeetingReset = true;
    }
    public override string GetProgressText(bool comms = false)
    {
        return Utils.ColorString(Color.yellow, $"({BulletCount})");
    }
    public override string GetMark(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        if (!Is(seer) || !Is(seen)) return "";

        if (AimAssist)
        {
            //エイムアシスト中のスナイパー
            if (0.5f < AimTime && (!AimAssistOneshot || AimTime < 1.0f))
            {
                if (GetSnipeTargets().Count > 0)
                {
                    return $"<size=200%>{Utils.ColorString(Palette.ImpostorRed, "◎")}</size>";
                }
            }
        }
        return "";
    }
    public static string GetMarkOthers(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        //各スナイパーから
        foreach (var sniper in Snipers)
        {
            //射撃音が聞こえるプレイヤー
            var snList = sniper.ShotNotify;
            if (snList.Count > 0 && snList.Contains(seer.PlayerId))
            {
                return $"<size=200%>{Utils.ColorString(Palette.ImpostorRed, "!")}</size>";
            }
        }
        return "";
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString(BulletCount <= 0 ? "DefaultShapeshiftText" : "SniperSnipeButtonText");
        return true;
    }
}