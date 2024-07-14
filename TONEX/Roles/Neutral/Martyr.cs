using AmongUs.GameOptions;
using Hazel;
using System.Linq;
using TONEX.Roles.Core;
using UnityEngine;
using System.Collections.Generic;
using TONEX.Roles.Core.Interfaces;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Translator;
using TONEX.Roles.Crewmate;
using MS.Internal.Xml.XPath;
using static UnityEngine.GraphicsBuffer;

namespace TONEX.Roles.Neutral;
public sealed class Martyr : RoleBase, IAdditionalWinner, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Martyr),
            player => new Martyr(player),
            CustomRoles.Martyr,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            94_1_1_0200,
            SetupOptionItem,
            "mar",
            "#A94F34",
            true,
            CanKill,
            countType: CountTypes.Martyr,
            introSound: () => GetIntroSound(RoleTypes.Crewmate),
            assignCountRule: new(1, 1, 1)
//#if RELEASE
//            ,ctop: true
//#endif
        );
    public Martyr(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {

        CustomRoleManager.OnCheckMurderPlayerOthers_After.Add(OnCheckMurderPlayerOthers_After);
    }
    public static byte WinnerID;

    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionCanGetKillButton;
    private static OptionItem OptionHasImpostorVision;
    enum OptionName
    {
        MartryCanUseKillButtonOnGameStart,
    }

    public static byte TargetId;
    public static bool CanKill = false;
    public bool HasProtect;
    public bool IsNK { get; private set; } = CanKill;
    public bool IsNE { get; private set; } = CanKill;
    public static List<PlayerControl> player;
    private static void SetupOptionItem()
    {
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionCanGetKillButton = BooleanOptionItem.Create(RoleInfo, 11, OptionName.MartryCanUseKillButtonOnGameStart, true, false);
        OptionHasImpostorVision = BooleanOptionItem.Create(RoleInfo, 13, GeneralOption.ImpostorVision, true, false);
    }
    public override void Add()
    {
        //ターゲット割り当て
        if (!AmongUsClient.Instance.AmHost) return;
        player = new();
        player.Add(Player);
        CanKill = OptionCanGetKillButton.GetBool();
        HasProtect = false;
        var playerId = Player.PlayerId;
        List<PlayerControl> targetList = new();
        var rand = IRandom.Instance;
        foreach (var target in Main.AllPlayerControls)
        {
            if (playerId == target.PlayerId) continue;
            if (target.Is(CustomRoles.GM)) continue;
            targetList.Add(target);
        }
        var SelectedTarget = targetList[rand.Next(targetList.Count)];
        TargetId = SelectedTarget.PlayerId;
        SendRPC();
    }
    public float CalculateKillCooldown() => OptionKillCooldown.GetFloat();
    public bool CanUseSabotageButton() => false;
    public bool CanUseKillButton() => CanKill && Player.IsAlive();
    public bool CanUseImpostorVentButton() => !HasProtect;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(OptionHasImpostorVision.GetBool());
    public void SendRPC()
    {
        if (!AmongUsClient.Instance.AmHost) return;

        using var sender = CreateSender();
        sender.Writer.Write(TargetId);
        sender.Writer.Write(HasProtect);
        sender.Writer.Write(CanKill);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        byte targetId = reader.ReadByte();
        TargetId = targetId;
        HasProtect = reader.ReadBoolean();
        CanKill = reader.ReadBoolean();
    }
    public override void OnPlayerDeath(PlayerControl player, CustomDeathReason deathReason, bool isOnMeeting = false)
    {
        if (player.PlayerId == TargetId)
        {
            HasProtect = false;
            SendRPC();
        }
    }

    private static bool OnCheckMurderPlayerOthers_After(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        if (info.IsSuicide) return true;
        if (target.PlayerId == TargetId)
        {
            foreach (var pc in Main.AllPlayerControls.Where(x => x.PlayerId != target.PlayerId && player.Contains(x) && x.IsAlive()))
            {
                    if ((pc.GetRoleClass() as Martyr).HasProtect)
                {
                    pc.RpcTeleport(target.transform.position);
                    killer.RpcTeleport(pc.transform.position);
                    killer.RpcMurderPlayerV2(pc);
                    killer.ResetKillCooldown();
                    killer.SetKillCooldownV2();
                    return false;
                }
                else
                {
                    CanKill = true;
                    pc.ResetKillCooldown();
                    pc.SetKillCooldownV2();
                    (pc.GetRoleClass() as Martyr).SendRPC();
                }


            }
        }
        return true;
    }

    public override string GetMark(PlayerControl seer, PlayerControl seen, bool _ = false)
    {
        //seenが省略の場合seer
        seen ??= seer;

        return TargetId == seen.PlayerId ? Utils.ColorString(RoleInfo.RoleColor, "♦") : "";
    }
    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {
        if (HasProtect) Player.Notify(GetString("HasProtect"));
        HasProtect = true;
        return false;
    }
    public bool CheckWin(ref CustomRoles winnerRole, ref CountTypes winnerCountType)
    {
        if (CustomWinnerHolder.WinnerIds.Contains(TargetId))
        {
            return true;
        }
        return false;
    }
}