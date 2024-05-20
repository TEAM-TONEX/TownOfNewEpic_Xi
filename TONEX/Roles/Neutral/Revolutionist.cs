/*using AmongUs.GameOptions;
using Hazel;
using MS.Internal.Xml.XPath;
using System.Collections.Generic;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;
using static TONEX.Translator;
using static UnityEngine.GraphicsBuffer;

namespace TONEX.Roles.Neutral;
public sealed class Revolutionist : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Revolutionist),
            player => new Revolutionist(player),
            CustomRoles.Revolutionist,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            75_1_2_1300,
            SetupOptionItem,
            "re|革命|改个",
            "#ff6633",
            isDesyncImpostor: true,
            introSound: () => GetIntroSound(RoleTypes.Crewmate)
        );
    public Revolutionist(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        DouseTime = RevolutionistDrawTime.GetFloat();
        DouseCooldown = RevolutionistCooldown.GetFloat();

        TargetInfo = null;
        RevolutionistTimer = new(GameData.Instance.PlayerCount);
    }
    private static OptionItem RevolutionistDrawTime;
    private static OptionItem RevolutionistCooldown;
    private static OptionItem RevolutionistDrawCount;
    private static OptionItem RevolutionistKillProbability;
    private static OptionItem RevolutionistVentCountDown;


    private static float DouseTime;
    private static float DouseCooldown;
    private TimerInfo TargetInfo;
    public static Dictionary<byte, bool> isDraw = new();
    public static Dictionary<byte, (PlayerControl, float)> RevolutionistTimer = new();
    public static Dictionary<byte, long> RevolutionistStart = new();
    public static Dictionary<byte, long> RevolutionistLastTime = new();
    public static Dictionary<byte, int> RevolutionistCountdown = new();

    public class TimerInfo
    {
        public byte TargetId;
        public float Timer;
        public TimerInfo(byte targetId, float timer)
        {
            TargetId = targetId;
            Timer = timer;
        }
    }


    public bool IsKiller { get; private set; } = false;
    public bool CanKill { get; private set; } = false;

    enum OptionName
    {
        RevolutionistDrawTime,
        RevolutionistCooldown,
        RevolutionistDrawCount,
        RevolutionistKillProbability,
        RevolutionistVentCountDown,
    }
    private static void SetupOptionItem()
    {
        RevolutionistDrawTime = FloatOptionItem.Create(RoleInfo, 10, OptionName.RevolutionistDrawTime, new(0f, 10f, 1f), 3f, false)
           .SetValueFormat(OptionFormat.Seconds);
        RevolutionistCooldown = FloatOptionItem.Create(RoleInfo, 11, OptionName.RevolutionistCooldown, new(5f, 100f, 1f), 10f, false)
            .SetValueFormat(OptionFormat.Seconds);
        RevolutionistDrawCount = IntegerOptionItem.Create(RoleInfo, 12, OptionName.RevolutionistDrawCount, new(1, 14, 1), 6, false)
            .SetValueFormat(OptionFormat.Players);
        RevolutionistKillProbability = IntegerOptionItem.Create(RoleInfo, 13, OptionName.RevolutionistKillProbability, new(0, 100, 5), 15, false)
            .SetValueFormat(OptionFormat.Percent);
        RevolutionistVentCountDown = FloatOptionItem.Create(RoleInfo, 14, OptionName.RevolutionistVentCountDown, new(1f, 180f, 1f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {

    }
    public bool CanUseKillButton() => !IsDrawDone(Player);
    public bool CanUseImpostorVentButton() => IsDrawDone(Player) && !Player.inVent;
    public float CalculateKillCooldown() => DouseCooldown;
    public bool CanUseSabotageButton() => false;
    public override string GetProgressText(bool comms = false)
    {
        var doused = GetDousedPlayerCount();
        return Utils.ColorString(RoleInfo.RoleColor.ShadeColor(0.25f), $"({doused.Item1}/{doused.Item2})");
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        opt.SetVision(false);
    }
    enum RPC_type
    {
SetDousedPlayer,
        SetCurrentDousingTarget
    }
    private void SendRPC(RPC_type rpcType, byte targetId = byte.MaxValue, bool isDoused = false)
    {
        using var sender = CreateSender();
        sender.Writer.Write(targetId);

        sender.Writer.Write((byte)rpcType);
        if (rpcType == RPC_type.SetDousedPlayer)
            sender.Writer.Write(isDoused);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        var targetId = reader.ReadByte();
        var rpcType = (RPC_type)reader.ReadByte();
        switch (rpcType)
        {
            case RPC_type.SetDousedPlayer:
                bool doused = reader.ReadBoolean();
                IsDoused[targetId] = doused;
                break;
            case RPC_type.SetCurrentDousingTarget:
                TargetInfo = new(targetId, 0f);
                break;
        }
    }
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;

        Logger.Info("Revolutionist start douse", "OnCheckMurderAsKiller");
        killer.SetKillCooldown(DouseTime);
        if (!IsDoused[target.PlayerId] && TargetInfo == null)
        {
            TargetInfo = new(target.PlayerId, 0f);
            Utils.NotifyRoles(SpecifySeer: killer);
            SendRPC(RPC_type.SetCurrentDousingTarget, target.PlayerId);
        }
        return false;
    }
    public override void OnReportDeadBody(PlayerControl reporter, GameData.PlayerInfo target)
    {
        TargetInfo = null;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (GameStates.IsInTask && RevolutionistTimer.ContainsKey(player.PlayerId))//当革命家拉拢一个玩家时
        {
            if (!player.IsAlive() || Pelican.IsEaten(player.PlayerId))
            {
                RevolutionistTimer.Remove(player.PlayerId);
                Utils.NotifyRoles(player);
                RPC.ResetCurrentDrawTarget(player.PlayerId);
            }
            else
            {
                var rv_target = RevolutionistTimer[player.PlayerId].Item1;//拉拢的人
                var rv_time = RevolutionistTimer[player.PlayerId].Item2;//拉拢时间
                if (!rv_target.IsAlive())
                {
                    RevolutionistTimer.Remove(player.PlayerId);
                }
                else if (rv_time >= Options.RevolutionistDrawTime.GetFloat())//在一起时间超过多久
                {
                    player.SetKillCooldown();
                    RevolutionistTimer.Remove(player.PlayerId);//拉拢完成从字典中删除
                    isDraw[(player.PlayerId, rv_target.PlayerId)] = true;//完成拉拢
                    player.RpcSetDrawPlayer(rv_target, true);
                    Utils.NotifyRoles(player);
                    RPC.ResetCurrentDrawTarget(player.PlayerId);
                    if (IRandom.Instance.Next(1, 100) <= Options.RevolutionistKillProbability.GetInt())
                    {
                        rv_target.SetRealKiller(player);
                        PlayerStates[rv_target.PlayerId].deathReason = PlayerState.DeathReason.Sacrifice;
                        player.RpcMurderPlayerV3(rv_target);
                        PlayerStates[rv_target.PlayerId].SetDead();
                        Logger.Info($"Revolutionist: {player.GetNameWithRole()} killed {rv_target.GetNameWithRole()}", "Revolutionist");
                    }
                }
                else
                {
                    float range = NormalGameOptionsV07.KillDistances[Mathf.Clamp(player.Is(CustomRoles.Reach) ? 2 : NormalOptions.KillDistance, 0, 2)] + 0.5f;
                    float dis = Vector2.Distance(player.transform.position, rv_target.transform.position);//超出距离
                    if (dis <= range)//在一定距离内则计算时间
                    {
                        RevolutionistTimer[player.PlayerId] = (rv_target, rv_time + Time.fixedDeltaTime);
                    }
                    else//否则删除
                    {
                        RevolutionistTimer.Remove(player.PlayerId);
                        Utils.NotifyRoles(__instance);
                        RPC.ResetCurrentDrawTarget(player.PlayerId);

                        Logger.Info($"Canceled: {__instance.GetNameWithRole()}", "Revolutionist");
                    }
                }
            }
        }
        if (GameStates.IsInTask && player.IsDrawDone() && player.IsAlive())
        {
            if (RevolutionistStart.ContainsKey(player.PlayerId)) //如果存在字典
            {
                if (RevolutionistLastTime.ContainsKey(player.PlayerId))
                {
                    long nowtime = Utils.GetTimeStamp();
                    if (RevolutionistLastTime[player.PlayerId] != nowtime) RevolutionistLastTime[player.PlayerId] = nowtime;
                    int time = (int)(RevolutionistLastTime[player.PlayerId] - RevolutionistStart[player.PlayerId]);
                    int countdown = Options.RevolutionistVentCountDown.GetInt() - time;
                    RevolutionistCountdown.Clear();
                    if (countdown <= 0)//倒计时结束
                    {
                        Utils.GetDrawPlayerCount(player.PlayerId, out var y);
                        foreach (var pc in y.Where(x => x != null && x.IsAlive()))
                        {
                            pc.Data.IsDead = true;
                            PlayerStates[pc.PlayerId].deathReason = PlayerState.DeathReason.Sacrifice;
                            pc.RpcMurderPlayerV3(pc);
                            PlayerStates[pc.PlayerId].SetDead();
                            Utils.NotifyRoles(pc);
                        }
                        player.Data.IsDead = true;
                        PlayerStates[player.PlayerId].deathReason = PlayerState.DeathReason.Sacrifice;
                        player.RpcMurderPlayerV3(player);
                        PlayerStates[player.PlayerId].SetDead();
                    }
                    else
                    {
                        RevolutionistCountdown.Add(player.PlayerId, countdown);
                    }
                }
                else
                {
                    RevolutionistLastTime.TryAdd(player.PlayerId, RevolutionistStart[player.PlayerId]);
                }
            }
            else //如果不存在字典
            {
                RevolutionistStart.TryAdd(player.PlayerId, Utils.GetTimeStamp());
            }
        }
    }
    public override bool OnEnterVent(PlayerPhysics physics, int ventId)
    {
        if (GameStates.IsInGame && IsDrawDone(Player))
        {
            foreach (var pc in Main.AllAlivePlayerControls)
            {
                if (pc.PlayerId != Player.PlayerId)
                {
                    //生存者は焼殺
                    pc.SetRealKiller(Player);
                    pc.RpcMurderPlayer(pc);
                    var state = PlayerState.GetByPlayerId(pc.PlayerId);
                    state.DeathReason = CustomDeathReason.Torched;
                    state.SetDead();
                }
                else
                    RPC.PlaySoundRPC(pc.PlayerId, Sounds.KillSound);
            }
            CustomWinnerHolder.ShiftWinnerAndSetWinner(CustomWinner.Revolutionist); //焼殺で勝利した人も勝利させる
            CustomWinnerHolder.WinnerIds.Add(Player.PlayerId);
            return true;
        }
        return false;
    }
    public override void OnUsePet()
    {
                if (GameStates.IsInGame && IsDrawDone(Player))
        {
            foreach (var pc in Main.AllAlivePlayerControls)
            {
                if (pc.PlayerId != Player.PlayerId)
                {
                    //生存者は焼殺
                    pc.SetRealKiller(Player);
                    pc.RpcMurderPlayer(pc);
                    var state = PlayerState.GetByPlayerId(pc.PlayerId);
                    state.DeathReason = CustomDeathReason.Torched;
                    state.SetDead();
                }
                else
                    RPC.PlaySoundRPC(pc.PlayerId, Sounds.KillSound);
            }
            CustomWinnerHolder.ShiftWinnerAndSetWinner(CustomWinner.Revolutionist); //焼殺で勝利した人も勝利させる
            CustomWinnerHolder.WinnerIds.Add(Player.PlayerId);
            return;
        }
        return;
    }
    public bool OverrideKillButtonText(out string text)
    {
        text = GetString("RevolutionistDouseButtonText");
        return true;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("RevolutionistVetnButtonText");
        return true;
    }
    public bool OverrideKillButtonSprite(out string buttonName)
    {
        buttonName = "Douse";
        return true;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = "Ignite";
        return true;
    }

    public override string GetMark(PlayerControl seer, PlayerControl seen, bool isForMeeting = false)
    {
        //seenが省略の場合seer
        seen ??= seer;

        if (IsDrawPlayer(target))
            TargetMark.Append($"<color={GetRoleColorCode(CustomRoles.Revolutionist)}>●</color>");

        if (Main.RevolutionistTimer.TryGetValue(seer.PlayerId, out var re_kvp) && re_kvp.Item1 == target)
            TargetMark.Append($"<color={GetRoleColorCode(CustomRoles.Revolutionist)}>○</color>");

        return "";
    }
    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        if (isForMeeting) return "";
        //seenが省略の場合seer
        seen ??= seer;
        //seeおよびseenが自分である場合以外は関係なし
        if (!Is(seer) || !Is(seen)) return "";

        return IsDrawDone(Player) ? Utils.ColorString(RoleInfo.RoleColor, GetString("EnterVentToWin")) : "";
    }
    public bool IsDrawPlayer(byte targetId) => isDraw.TryGetValue(targetId, out bool isDoused) && isDoused;
    public static bool IsDrawDone( this PlayerControl player)
    {
        if (player.GetRoleClass() is not Revolutionist Revolutionist) return false;
        var count = Revolutionist.GetDousedPlayerCount();
        return count.Item1 == count.Item2;
    }
    public (int, int) GetDousedPlayerCount()
    {
        int doused = 0, all = 0;
        //多分この方がMain.isDousedでforeachするより他のアーソニストの分ループ数少なくて済む
        foreach (var pc in Main.AllAlivePlayerControls)
        {
            if (pc.PlayerId == Player.PlayerId) continue; //アーソニストは除外

            all++;
            if (isDraw.TryGetValue(pc.PlayerId, out var isDoused) && isDoused)
                //塗れている場合
                doused++;
        }

        return (doused, all);
    }
}
//*/