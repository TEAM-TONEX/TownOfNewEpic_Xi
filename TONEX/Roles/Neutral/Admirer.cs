using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;
using static TONEX.Translator;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using TONEX.Roles.AddOns.Common;

namespace TONEX.Roles.Neutral;
public sealed class Admirer : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Admirer),
            player => new Admirer(player),
            CustomRoles.Admirer,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            75_1_2_1700,
            SetupOptionItem,
            "admir",
            "#FFC8EE",
            true,
            assignCountRule: new(1, 1, 1)
        );
    public Admirer(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
    }
    enum OptionName
    {
        AkujoModeSwitchAction,
        LoverKnowRoles,

        LoverSuicide,
        AkujoFakeLimited
    }
    public static void SetupOptionItem()
    {

        AdmirerLovers.AdmirerLoverKnowRoles = BooleanOptionItem.Create(RoleInfo, 12, OptionName.LoverKnowRoles, true, false);
        AdmirerLovers.AdmirerLoverSuicide = BooleanOptionItem.Create(RoleInfo, 13, OptionName.LoverSuicide, true, false);
    }
    public bool IsKiller { get; private set; } = false;

    private int AdmirerLimit;
    public override void Add()
    {
        AdmirerLimit = 1;
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(AdmirerLimit);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        AdmirerLimit = reader.ReadInt32();
    }
    public float CalculateKillCooldown() => CanUseKillButton() ? 1f : 255f;
    public bool CanUseKillButton() => Player.IsAlive() && AdmirerLimit >= 1;
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(false);
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        if (AdmirerLimit >= 1 && CanBeLover (target))
        {
            AdmirerLimit--;
            AdmirerLovers.AdmirerLoversPlayers.Clear();
            AdmirerLovers.isAdmirerLoversDead = false;
            AdmirerLovers.AdmirerLoversPlayers.Add(killer);
            PlayerState.GetByPlayerId(killer.PlayerId).SetSubRole(CustomRoles.AdmirerLovers);
            AdmirerLovers.AdmirerLoversPlayers.Add(target);
            PlayerState.GetByPlayerId(target.PlayerId).SetSubRole(CustomRoles.AdmirerLovers);
            AdmirerLovers.SyncAdmirerLoversPlayers();
            SendRPC();
            NameColorManager.Add(Player.PlayerId, target.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
            NameColorManager.Add(target.PlayerId, Player.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
            killer.RpcProtectedMurderPlayer(target);
            target.RpcProtectedMurderPlayer(killer);
            killer.SetKillCooldownV2();
            Utils.NotifyRoles(killer);
            Utils.NotifyRoles(target);
        }
        else if (CanBeLover(target))
        {
            if (AdmirerLovers.AdmirerLoversPlayers.Contains(target))
            Player.Notify(GetString("AkujoSixSix"));
            else
                Player.Notify(GetString("AkujoSixSix"));
        }
        else if (AdmirerLimit <= 0)
        {
            if (AdmirerLovers.AdmirerLoversPlayers.Contains(target))
                Player.Notify(GetString("AdmirerCant"));
        }
        return false;
    }
    public static bool CanBeLover(PlayerControl pc) => pc != null && (
        !(pc.Is(CustomRoles.LazyGuy) 
        || pc.Is(CustomRoles.Neptune) 
        || pc.Is(CustomRoles.God) 
        || pc.Is(CustomRoles.Hater) 
        || pc.Is(CustomRoles.Believer )
        || pc.Is(CustomRoles.Nihility)
        || pc.Is(CustomRoles.Lovers) 
        || pc.Is(CustomRoles.AkujoLovers) 
        || pc.Is(CustomRoles.CupidLovers) 
        || pc.Is(CustomRoles.Akujo) 
        || pc.Is(CustomRoles.Cupid) 
        || pc.Is(CustomRoles.Yandere)
        || pc.Is(CustomRoles.Admirer)
        || pc.Is(CustomRoles.AdmirerLovers)
      //  || Yandere.Targets.Contains(pc)
        ));

    public override string GetProgressText(bool comms = false) => Utils.ColorString(CanUseKillButton() ? RoleInfo.RoleColor : Color.gray, $"({AdmirerLimit})");
    public bool OverrideKillButtonSprite(out string buttonName)
    {
        buttonName = "Subbus";
        return true;
    }
    public bool OverrideKillButtonText(out string text)
    {
        text = Translator.GetString("AdmirerButtonText");
        return true;
    }
}
