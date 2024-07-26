using AmongUs.GameOptions;
using Hazel;
using UnityEngine;
using TONEX.Roles.Core;
using System.Text;
using static TONEX.Translator;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Roles.AddOns.CanNotOpened.CupidLovers;
using UnityEngine.ProBuilder;
using System.Linq;

namespace TONEX.Roles.Neutral;
public sealed class Cupid : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Cupid),
            player => new Cupid(player),
            CustomRoles.Cupid,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            75_1_2_2300,
            SetupOptionItem,
            "cup",
            "#F69896",
            true,
            assignCountRule: new(1, 1, 1)
        );
    public Cupid(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
        CustomRoleManager.OnCheckMurderPlayerOthers_After.Add(OnCheckMurderPlayerOthers_After);
    }
    enum OptionName
    {
        LoverKnowRoles,

        LoverSuicide, 
        CupidCanShield,
        CupidLoverKnowCupid
    }
    static OptionItem CupidCanShield;
    public static OptionItem CupidLoverKnowCupid;

    public static void SetupOptionItem()
    {
        CupidCanShield = BooleanOptionItem.Create(RoleInfo, 11, OptionName.CupidCanShield, true, false);
        CupidLoverKnowRoles = BooleanOptionItem.Create(RoleInfo, 12, OptionName.LoverKnowRoles, true, false);
        CupidLoverKnowCupid = BooleanOptionItem.Create(RoleInfo, 14, OptionName.CupidLoverKnowCupid, true, false);
        CupidLoverSuicide = BooleanOptionItem.Create(RoleInfo, 13, OptionName.LoverSuicide, true, false);
    }
    public bool IsKiller { get; private set; } = false;

    private int CupidLimit;
    List<byte> ReadyPlayers=new();
    bool Shield;
    public override void Add()
    {
        CupidLimit = 2;
        Shield = false;
        ReadyPlayers = new();
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(CupidLimit);
        sender.Writer.Write(Shield);
        sender.Writer.Write(ReadyPlayers.Count);
        foreach (var id in ReadyPlayers)
            sender.Writer.Write(id);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        CupidLimit = reader.ReadInt32();
        Shield = reader.ReadBoolean();
        ReadyPlayers = new();
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
            ReadyPlayers.Add(reader.ReadByte());

    }
    public float CalculateKillCooldown() => CanUseKillButton() ? 1f : 255f;
    public bool CanUseKillButton() => CupidLimit>=0 && Player.IsAlive();
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => !Shield && CupidCanShield.GetBool();
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(false);
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;

        if (CanBeLover(target))
        {
            if (CupidLimit >= 1)
            {
                CupidLimit--;
                ReadyPlayers.Add(target.PlayerId);
                NameColorManager.Add(Player.PlayerId, target.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
                SendRPC();
                target.RpcProtectedMurderPlayer(killer);
                killer.SetKillCooldownV2();
                Utils.NotifyRoles(killer);
            }
            if (ReadyPlayers.Count == 2 && CupidLimit == 0)
            {
                foreach (var id in ReadyPlayers)
                {
                    CupidLoversPlayers.Clear();
                    isCupidLoversDead = false;
                    CupidLoversPlayers.Add(Utils.GetPlayerById(id));
                    PlayerState.GetByPlayerId(id).SetSubRole(CustomRoles.CupidLovers);
                    NameColorManager.Add(id, Player.PlayerId, $"{ColorHelper.ColorToHex(RoleInfo.RoleColor)}");
                    target.RpcProtectedMurderPlayer(killer);
                    Utils.NotifyRoles(target);
                }
                ReadyPlayers.Clear();
                SendRPC();
            }


        }
        else if (target.Is(CustomRoles.CupidLovers) || ReadyPlayers.Contains(target.PlayerId))
        {

            Player.Notify(GetString("CupidSix"));
        }
        else
        {

            Player.Notify(GetString("CupidCant"));

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
        || pc.Is(CustomRoles.CupidLovers)
        || pc.Is(CustomRoles.CupidLovers)
        || pc.Is(CustomRoles.Cupid)
        || pc.Is(CustomRoles.Cupid)
        || pc.Is(CustomRoles.Yandere)
        || pc.Is(CustomRoles.Admirer)
        || pc.Is(CustomRoles.AdmirerLovers)
    //    || Yandere.Targets.Contains(pc)
        ));

    public override string GetProgressText(bool comms = false) => Utils.ColorString(CanUseKillButton() ? RoleInfo.RoleColor : Color.gray, $"({CupidLimit})");
    public bool OverrideKillButtonSprite(out string buttonName)
    {
        buttonName = "CupidButton";
        return true;
    }
    public bool OverrideKillButtonText(out string text)
    {
        text = GetString("CupidKillButtonText");
        return true;
    }
    public override bool EnablePetSkill() => true;

    public override bool OnEnterVentWithUsePet(PlayerPhysics physics, int ventId)
    {
        if (!CupidCanShield.GetBool()) return false;
        if (!Shield)
        {
            Shield = true;
            Player.Notify(GetString("CupidShielded"));
        }
        else
            Player.Notify(GetString("CupidIsShielded"));

        return false;
    }
    public static bool OnCheckMurderPlayerOthers_After(MurderInfo info)
    {

        var (killer, target) = info.AttemptTuple;
        if (target.Is(CustomRoles.CupidLovers))
        {
            foreach(var pc in Main.AllPlayerControls.Where(p=>p.Is(CustomRoles.Cupid)))
            {
                var roleclass = pc.GetRoleClass() as Cupid;
                if (roleclass.Shield)
                {
                    pc.RpcTeleport(target.transform.position);
                    killer.RpcTeleport(pc.transform.position);
                    killer.RpcMurderPlayerV2(pc);
                    killer.ResetKillCooldown();
                    killer.SetKillCooldownV2();
                    foreach (var p in CupidLoversPlayers)
                        p.Notify(GetString("CupidIsDead"));
                    return false;
                }
            }
        }
        return true;
    }
}
