using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using TONEX.Roles.Core;
using System.Text;
using static TONEX.Translator;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Roles.AddOns.CanNotOpened.AkujoLovers;
using UnityEngine.ProBuilder;

namespace TONEX.Roles.Neutral;
public sealed class Akujo : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Akujo),
            player => new Akujo(player),
            CustomRoles.Akujo,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            75_1_2_2100,
            SetupOptionItem,
            "akuj",
            "#8E4593",
            true,
            assignCountRule: new(1, 1, 1)
        );
    public Akujo(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        NowSwitchTrigger = (SwitchTrigger)OptionModeSwitchAction.GetValue();
        Player.AddDoubleTrigger();
    }
    public SwitchTrigger NowSwitchTrigger;

    public enum SwitchTrigger
    {
        TriggerVent,
        TriggerDouble,
    };
    enum OptionName
    {
        AkujoModeSwitchAction,
        LoverKnowRoles,

        LoverSuicide, 
        AkujoFakeLimited
    }
    static OptionItem OptionModeSwitchAction;
    static OptionItem AkujoFakeLimited;

    public static void SetupOptionItem()
    {
        OptionModeSwitchAction = StringOptionItem.Create(RoleInfo, 10, OptionName.AkujoModeSwitchAction, EnumHelper.GetAllNames<SwitchTrigger>(), 1, false);
        AkujoFakeLimited = IntegerOptionItem.Create(RoleInfo, 11, OptionName.AkujoFakeLimited, new(1,15,1),2, false).SetValueFormat(OptionFormat.Players);
        AkujoLoverKnowRoles = BooleanOptionItem.Create(RoleInfo, 12, OptionName.LoverKnowRoles, true, false);
        AkujoLoverSuicide = BooleanOptionItem.Create(RoleInfo, 13, OptionName.LoverSuicide, true, false);
    }
    public bool IsKiller { get; private set; } = false;

    private int AkujoLimit;
    private int FakeLimit;
    bool ChooseFake;
    public override void Add()
    {
        AkujoLimit = 1;
        FakeLimit = AkujoFakeLimited.GetInt();
        ChooseFake = false;
    }
    private void SendRPC(bool writechooseFake = false)
    {
        using var sender = CreateSender();
        sender.Writer.Write(AkujoLimit);
        sender.Writer.Write(FakeLimit);
        sender.Writer.Write(writechooseFake);
        if (writechooseFake) sender.Writer.Write(ChooseFake);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        AkujoLimit = reader.ReadInt32();
        FakeLimit = reader.ReadInt32();
        var writechooseFake = reader.ReadBoolean();
        if (writechooseFake)ChooseFake = reader.ReadBoolean();
    }
    public float CalculateKillCooldown() => CanUseKillButton() ? 1f : 255f;
    public bool CanUseKillButton() => Player.IsAlive() && AkujoLimit >= 1;
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => !Options.UsePets.GetBool() && (int)NowSwitchTrigger == 0;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(false);
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        if (NowSwitchTrigger == SwitchTrigger.TriggerVent)
        {
            if (CanBeLover(target))
            {
                if (ChooseFake && FakeLimit >= 1)
                {
                    FakeLimit--;
                    PlayerState.GetByPlayerId(target.PlayerId).SetSubRole(CustomRoles.AkujoFakeLovers);
                    NameColorManager.Add(Player.PlayerId, target.PlayerId, $"{Utils.GetRoleColorCode(CustomRoles.AkujoFakeLovers)}");
                    NameColorManager.Add(target.PlayerId, Player.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
                    SendRPC();
                    killer.RpcProtectedMurderPlayer(target);
                    target.RpcProtectedMurderPlayer(killer);
                    killer.SetKillCooldownV2();
                    Utils.NotifyRoles(killer);
                    Utils.NotifyRoles(target);

                }
                else if (!ChooseFake && AkujoLimit >= 1)
                {
                    AkujoLimit--;
                    AkujoLoversPlayers.Clear();
                    isAkujoLoversDead = false;
                    AkujoLoversPlayers.Add(killer);
                    AkujoLoversPlayers.Add(target);
                    PlayerState.GetByPlayerId(target.PlayerId).SetSubRole(CustomRoles.AkujoLovers);
                    NameColorManager.Add(Player.PlayerId, target.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
                    NameColorManager.Add(target.PlayerId, Player.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
                    SyncAkujoLoversPlayers();
                    SendRPC();
                    killer.RpcProtectedMurderPlayer(target);
                    target.RpcProtectedMurderPlayer(killer);
                    killer.SetKillCooldownV2();
                    Utils.NotifyRoles(killer);
                    Utils.NotifyRoles(target);
                }


            }
            else if (target.Is(CustomRoles.AkujoLovers))
            {
                if (ChooseFake)
                {
                    Player.Notify(GetString("AkujoSix"));
                }
                else
                {
                    Player.Notify(GetString("AkujoSixSix"));
                }
            }
            else if (target.Is(CustomRoles.AkujoFakeLovers))
            {
                if (ChooseFake)
                {
                    Player.Notify(GetString("AkujoSixTwo"));
                }
                else
                {
                    Player.Notify(GetString("AkujoSixSixTwo"));
                }
            }
            else 
            {
                
                    Player.Notify(GetString("AkujoCant"));
                
            }
        }
        if (NowSwitchTrigger == SwitchTrigger.TriggerDouble)
        {
            var fake = killer.CheckDoubleTrigger(target, () => {
                if (AkujoLimit >= 1 && CanBeLover(target))
                {
                    AkujoLimit--;
                    AkujoLoversPlayers.Clear();
                    isAkujoLoversDead = false;
                    AkujoLoversPlayers.Add(killer);
                    AkujoLoversPlayers.Add(target);
                    PlayerState.GetByPlayerId(target.PlayerId).SetSubRole(CustomRoles.AkujoLovers);
                    NameColorManager.Add(Player.PlayerId, target.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
                    NameColorManager.Add(target.PlayerId, Player.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
                    SyncAkujoLoversPlayers();
                    SendRPC();
                    killer.RpcProtectedMurderPlayer(target);
                    target.RpcProtectedMurderPlayer(killer);
                    killer.SetKillCooldownV2();
                    Utils.NotifyRoles(killer);
                    Utils.NotifyRoles(target);
                };
            });
            if (fake && CanBeLover(target))
            {
                FakeLimit--;
                PlayerState.GetByPlayerId(target.PlayerId).SetSubRole(CustomRoles.AkujoFakeLovers);
                NameColorManager.Add(Player.PlayerId, target.PlayerId, $"{Utils.GetRoleColorCode(CustomRoles.AkujoFakeLovers)}");
                NameColorManager.Add(target.PlayerId, Player.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
                SendRPC();
                killer.RpcProtectedMurderPlayer(target);
                target.RpcProtectedMurderPlayer(killer);
                killer.SetKillCooldownV2();
                Utils.NotifyRoles(killer);
                Utils.NotifyRoles(target);

            }
            if (!CanBeLover(target))
            {
                if (target.Is(CustomRoles.AkujoLovers))
                {
                    if (ChooseFake)
                    {
                        Player.Notify(GetString("AkujoSix"));
                    }
                    else
                    {
                        Player.Notify(GetString("AkujoSixSix"));
                    }
                }
                else if (target.Is(CustomRoles.AkujoFakeLovers))
                {
                    if (ChooseFake)
                    {
                        Player.Notify(GetString("AkujoSixTwo"));
                    }
                    else
                    {
                        Player.Notify(GetString("AkujoSixSixTwo"));
                    }
                }
                else
                {

                    Player.Notify(GetString("AkujoCant"));

                }
            }
        }

        return false;
    }
    public static bool CanBeLover(PlayerControl pc) => pc != null && (
        !(pc.Is(CustomRoles.LazyGuy)
        || pc.Is(CustomRoles.Neptune)
        || pc.Is(CustomRoles.God)
        || pc.Is(CustomRoles.Hater)
        || pc.Is(CustomRoles.Believer)
        || pc.Is(CustomRoles.Nihility)
        || pc.Is(CustomRoles.Lovers)
        || pc.Is(CustomRoles.AkujoLovers)
        || pc.Is(CustomRoles.CupidLovers)
        || pc.Is(CustomRoles.Akujo)
        || pc.Is(CustomRoles.Cupid)
        || pc.Is(CustomRoles.Yandere)
        || pc.Is(CustomRoles.Admirer)
        || pc.Is(CustomRoles.AdmirerLovers)
      //   || Yandere.Targets.Contains(pc)
         ));

    public override string GetProgressText(bool comms = false) => Utils.ColorString(CanUseKillButton() ? RoleInfo.RoleColor : Color.gray, OptionModeSwitchAction.GetInt()==1?(ChooseFake ? $"({FakeLimit})": $"({AkujoLimit})"):$"({AkujoLimit})({FakeLimit})");
    public bool OverrideKillButtonSprite(out string buttonName)
    {
        buttonName = ChooseFake ? "ChooseFakeLove" : "ChooseRealLove";
        return true;
    }
    public bool OverrideKillButtonText(out string text)
    {
        text = ChooseFake ? GetString("AkujoFakeButtonText"):Translator.GetString("AkujoRealButtonText");
        return true;
    }
    public void SwitchChooseMode()
    {
        ChooseFake = !ChooseFake;
        SendRPC(true);
        Utils.NotifyRoles(SpecifySeer: Player);

    }
    public override string GetLowerText(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false, bool isForHud = false)
    {
        seen ??= seer;
        if (!Is(seen) || isForMeeting) return "";

        var sb = new StringBuilder();
        sb.Append(isForHud ? GetString("WitchCurrentMode") : "Mode:");
        if (NowSwitchTrigger == SwitchTrigger.TriggerDouble)
        {
            sb.Append(GetString("AkujoModeDouble"));
        }
        else
        {
            sb.Append(ChooseFake ? GetString("FakeMode") : GetString("RealMode"));
        }
        return sb.ToString();
    }
    public override bool EnablePetSkill() => true;

    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {
        if (NowSwitchTrigger is SwitchTrigger.TriggerVent)
        {
            SwitchChooseMode();
        }
        return false;
    }
    
}
