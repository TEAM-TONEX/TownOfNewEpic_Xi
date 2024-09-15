using AmongUs.GameOptions;
using System.Collections.Generic;
using System.Linq;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;
using static TONEX.Translator;
using TONEX.Modules;
using Hazel;

namespace TONEX.Roles.Impostor;
public sealed class Fireworker : RoleBase, IImpostor
{
    public enum FireworkerState
    {
        Initial = 1,
        SettingFireworker = 2,
        WaitTime = 4,
        ReadyFire = 8,
        FireEnd = 16,
        CanUseKill = Initial | FireEnd
    }

    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Fireworker),
            player => new Fireworker(player),
            CustomRoles.Fireworker,
            () => RoleTypes.Phantom,
            CustomRoleTypes.Impostor,
            2300,
            SetupCustomOption,
            "fw|煙花商人|烟火商人|烟花|烟火"
        );
    public Fireworker(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        FireworkerCount = OptionFireworkerCount.GetInt();
        FireworkerRadius = OptionFireworkerRadius.GetFloat();
    }

    static OptionItem OptionFireworkerCount;
    static OptionItem OptionFireworkerRadius;
    static OptionItem OptionFireworkerCanKill;
    enum OptionName
    {
        FireworkerMaxCount,
        FireworkerRadius,
        FireworkerCanKill,
    }

    int FireworkerCount;
    float FireworkerRadius;
    int NowFireworkerCount;
    List<Vector3> FireworkerPosition = new();
    FireworkerState State = FireworkerState.Initial;
    private readonly List<FireWorkerData> fireworks = [];
    public static void SetupCustomOption()
    {
        OptionFireworkerCount = IntegerOptionItem.Create(RoleInfo, 10, OptionName.FireworkerMaxCount, new(1, 99, 1), 3, false)
            .SetValueFormat(OptionFormat.Pieces);
        OptionFireworkerRadius = FloatOptionItem.Create(RoleInfo, 11, OptionName.FireworkerRadius, new(0.5f, 5f, 0.5f), 2f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionFireworkerCanKill = BooleanOptionItem.Create(RoleInfo, 13, OptionName.FireworkerCanKill, false, false);
    }

    public override void Add()
    {
        NowFireworkerCount = FireworkerCount;
        FireworkerPosition.Clear();
        State = FireworkerState.Initial;
    }

    public bool CanUseKillButton()
    {
        if (!Player.IsAlive()) return false;
        return (State & FireworkerState.CanUseKill) != 0 || OptionFireworkerCanKill.GetBool();
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.PhantomDuration = State != FireworkerState.FireEnd ? 1f : 30f;
    }
    public override bool GetGameStartSound(out string sound)
    {
        sound = "Boom";
        return true;
    }
    public override bool OnCheckVanish()
    {
        Logger.Info($"Fireworker ShapeShift", "Fireworker");
        switch (State)
        {
            case FireworkerState.Initial:
            case FireworkerState.SettingFireworker:
                Logger.Info("花火を一個設置", "Fireworker");
                FireworkerPosition.Add(Player.transform.position);
                NowFireworkerCount--;
                var pos1 = Player.GetTruePosition();
                fireworks.Add(new(new(pos1, Player.PlayerId) ,pos1));
                Utils.SendRPC(CustomRPC.CustomRoleSync, Player, 1, pos1);
                if (NowFireworkerCount == 0)
                    State = Main.AliveImpostorCount <= 1 ? FireworkerState.ReadyFire : FireworkerState.WaitTime;
                else
                    State = FireworkerState.SettingFireworker;
                break;
            case FireworkerState.ReadyFire:
                CustomSoundsManager.RPCPlayCustomSoundAll("Boom");
                Logger.Info("花火を爆破", "Fireworker");
                if (AmongUsClient.Instance.AmHost)
                {
                    //爆破処理はホストのみ
                    bool suicide = false;
                    foreach (var fireTarget in Main.AllAlivePlayerControls)
                    {
                        foreach (var pos in FireworkerPosition)
                        {
                            var dis = Vector2.Distance(pos, fireTarget.transform.position);
                            if (dis > FireworkerRadius) continue;

                            if (fireTarget == Player)
                            {
                                //自分は後回し
                                suicide = true;
                            }
                            else
                            {
                                PlayerState.GetByPlayerId(fireTarget.PlayerId).DeathReason = CustomDeathReason.Bombed;
                                fireTarget.SetRealKiller(Player);
                                fireTarget.RpcMurderPlayer(fireTarget);
                            }
                        }
                    }
                    if (suicide)
                    {
                        var totalAlive = Main.AllAlivePlayerControls.Count();
                        //自分が最後の生き残りの場合は勝利のために死なない
                        if (totalAlive != 1)
                        {
                            MyState.DeathReason = CustomDeathReason.Misfire;
                            Player.RpcMurderPlayer(Player);
                        }
                    }
                    Player.MarkDirtySettings();
                }
                State = FireworkerState.FireEnd;
                for(int i=0;i<fireworks.Count;i++) {
                    fireworks.RemoveAt(i);
                    fireworks[i].NetObject.Despawn();
                    Utils.SendRPC(CustomRPC.CustomRoleSync, Player,2, i);
                }
                break;
            default:
                break;
        }
        Utils.NotifyRoles();
        return false;
    }

    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        string retText = "";
        
        if (State == FireworkerState.WaitTime && Main.AliveImpostorCount <= 1)
        {
            Logger.Info("爆破準備OK", "Fireworker");
            State = FireworkerState.ReadyFire;
            Utils.NotifyRoles();
        }
        switch (State)
        {
            case FireworkerState.Initial:
            case FireworkerState.SettingFireworker:
                retText = string.Format(GetString("FireworksPutPhase"), NowFireworkerCount);
                break;
            case FireworkerState.WaitTime:
                retText = GetString("FireworksWaitPhase");
                break;
            case FireworkerState.ReadyFire:
                retText = GetString("FireworksReadyFirePhase");
                break;
            case FireworkerState.FireEnd:
                break;
        }
        return retText;
    }
    public override bool CanUseAbilityButton() => State != FireworkerState.FireEnd;
    public override bool GetAbilityButtonText(out string text)
    {
        text = State == FireworkerState.ReadyFire
            ? GetString("FireworkerExplosionButtonText")
            : GetString("FireworkerInstallAtionButtonText");
        return true;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = State == FireworkerState.ReadyFire ? "FireworkD" : "FireworkP";
        return true;
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        switch (reader.ReadPackedInt32())
        {
            case 1:
                var pos = reader.ReadVector2();
                var roomName = reader.ReadString();
                fireworks.Add(new(new(pos, Player.PlayerId), pos));
                break;
            case 2:
                fireworks.RemoveAt(reader.ReadPackedInt32());
                break;
        }
    }

    class FireWorkerData(Fireworks NetObject, Vector2 Position)
    {
        public Fireworks NetObject { get; } = NetObject;
        public Vector2 Position { get; set; } = Position;
    }
}