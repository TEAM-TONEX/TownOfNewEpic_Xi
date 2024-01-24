﻿using AmongUs.GameOptions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TONEX.Roles.Core;
using UnityEngine;
using TONEX.Modules;
using static TONEX.Translator;
using Hazel;

namespace TONEX.Roles.Crewmate;
public sealed class Paranoia : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Paranoia),
            player => new Paranoia(player),
            CustomRoles.Paranoia,
         () => Options.UsePets.GetBool() ? RoleTypes.Crewmate : RoleTypes.Engineer,
            CustomRoleTypes.Crewmate,
            20800,
            SetupOptionItem,
            "pa|被害妄想|被迫害妄想症|被害|妄想|妄想症",
            "#c993f5"
        );
    public Paranoia(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    static OptionItem OptionSkillNums;
    static OptionItem OptionSkillCooldown;
    enum OptionName
    {
        ParanoiaNumOfUseButton,
        ParanoiaVentCooldown,
    }

    private int SkillLimit;
    private long UsePetCooldown;
    private static void SetupOptionItem()
    {
        OptionSkillNums = IntegerOptionItem.Create(RoleInfo, 10, OptionName.ParanoiaNumOfUseButton, new(1, 99, 1), 3, false)
            .SetValueFormat(OptionFormat.Times);
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 11, OptionName.ParanoiaVentCooldown, new(2.5f, 180f, 2.5f), 10f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
        public override void Add()
    {
        SkillLimit = OptionSkillNums.GetInt();
        if (Options.UsePets.GetBool()) UsePetCooldown = Utils.GetTimeStamp();
    }
    public override void OnGameStart()
    {
        if (Options.UsePets.GetBool()) UsePetCooldown = Utils.GetTimeStamp();
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown =
            SkillLimit < 1
            ? 255f
            : OptionSkillCooldown.GetFloat();
        AURoleOptions.EngineerInVentMaxTime = 1;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = Translator.GetString("ParanoiaVetnButtonText");
        return true;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = "Paranoid";
        return true;
    }
    public override bool GetPetButtonText(out string text)
    {
        text = Translator.GetString("ParanoiaVetnButtonText");
        return !(UsePetCooldown != -1);
    }
    public override bool GetPetButtonSprite(out string buttonName)
    {
        buttonName = "Paranoid";
        return !(UsePetCooldown != -1);
    }
    public override bool OnEnterVent(PlayerPhysics physics, int ventId)
    {
        if (SkillLimit >= 1)
        {
            var user = physics.myPlayer;
            physics.RpcBootFromVent(ventId);
            user?.NoCheckStartMeeting(user?.Data);
            SkillLimit--;
        }
        else
        {
            Player.Notify(Translator.GetString("SkillMaxUsage"));
        }
        return false;
    }
    public override void OnUsePet()
    {
        if (!Options.UsePets.GetBool()) return;
        if (UsePetCooldown != -1)
        {
            var cooldown = UsePetCooldown + (long)OptionSkillCooldown.GetFloat() - Utils.GetTimeStamp();
            Player.Notify(string.Format(GetString("ShowUsePetCooldown"), cooldown, 1f));
            return;
        }
        if (SkillLimit >= 1)
        {
            Player?.NoCheckStartMeeting(Player?.Data);
            SkillLimit--;
        }
        else
        {
            Player.Notify(Translator.GetString("SkillMaxUsage"));
            return;
        }
        return;
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var now = Utils.GetTimeStamp();
        if (UsePetCooldown + (long)OptionSkillCooldown.GetFloat() < now && UsePetCooldown != -1 && Options.UsePets.GetBool())
        {
            UsePetCooldown = -1;
            player.RpcProtectedMurderPlayer();
            player.Notify(string.Format(GetString("PetSkillCanUse")));
        }
    }
    public override bool CanUseAbilityButton() => SkillLimit > 0;
    public override void NotifyOnMeetingStart(ref List<(string, byte, string)> msgToSend)
    {
        msgToSend.Add((Translator.GetString("SkillUsedLeft") + SkillLimit.ToString(), Player.PlayerId, null));
    }
    public override void AfterMeetingTasks()
    {
        UsePetCooldown = Utils.GetTimeStamp();
        Player.RpcResetAbilityCooldown();
    }
    public override void OnExileWrapUp(GameData.PlayerInfo exiled, ref bool DecidedWinner) => Player.RpcResetAbilityCooldown();
}