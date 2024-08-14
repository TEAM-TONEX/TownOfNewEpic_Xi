using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using MS.Internal.Xml.XPath;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Translator;
using UnityEngine;
//职业设计来自鹅鸭杀
namespace TONEX.Roles.Impostor;
public sealed class SuperPower : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(SuperPower),
            player => new SuperPower(player),
            CustomRoles.SuperPower,
            () => RoleTypes.Phantom,
            CustomRoleTypes.Impostor,
            94_1_4_0700,
            SetupOptionItem,
            "sp"
        );
    public SuperPower(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillDuration;
    public PlayerControl target=null;
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.SkillCooldown, new(2.5f, 180f, 2.5f), 12f, false)
      .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 11, GeneralOption.SkillDuration, new(2.5f, 180f, 2.5f), 10f, false)
      .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add() => target = null;
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.PhantomCooldown=OptionSkillCooldown.GetFloat();
        AURoleOptions.PhantomDuration = OptionSkillDuration.GetFloat()+1f;
    }
    public Vector2 Position;
    public override bool OnCheckVanish()
    {
        var pcList = Main.AllAlivePlayerControls.Where(x => x.PlayerId != Player.PlayerId && !x.GetCustomRole().IsImpostor() && x.IsAlive()).ToList();
        Position = Player.GetTruePosition();
        new LateTask(() =>
        {
            target = pcList[IRandom.Instance.Next(0, pcList.Count)];
        }, 2f, "SuperPower");
        new LateTask(() =>
        {
            if (target != null) {
              target.RpcMurderPlayerV2(target);
            target.SetRealKiller(Player);
            target = null;
            Player.RpcTeleport(Position);
            Position = new();
            }
        }, OptionSkillDuration.GetFloat(), "SuperPower");
        return true ;
    }
    public override bool OnAppear( bool animate)
    {
        if (target != null){
            target.RpcMurderPlayerV2(target);
            target.SetRealKiller(Player);
            target = null;
            Player.RpcTeleport(Position);
            Position = new();
        }
        return true;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (target != null) {
            Player.RpcTeleport(target.GetTruePosition());
        }
    }
    public override bool GetAbilityButtonText(out string text)
    {
        if(target==null)
           text = GetString("SuperPowerAbilityButtonText");
       else if(target!=null) text = GetString("KillButtonText");
        else text = GetString(" ");
        return true;
    }
}

