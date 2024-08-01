using AmongUs.GameOptions;
using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Options;

namespace TONEX.Roles.AddOns.Common;
public sealed class Lighter : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Lighter),
    player => new Lighter(player),
    CustomRoles.Lighter,
    82100,
    SetupOptionItem,
    "li|執燈人|执灯|灯人|小灯人",
    "#eee5be"
    );
    public Lighter(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }




    public static OptionItem OptionVistion;
    enum OptionName
    {
        LighterVision
    }

    static void SetupOptionItem()
    {
        OptionVistion = FloatOptionItem.Create(RoleInfo, 20, OptionName.LighterVision, new(0.5f, 5f, 0.25f), 1.5f,  false)
       .SetValueFormat(OptionFormat.Multiplier);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
            opt.SetVision(true);
            opt.SetFloat(FloatOptionNames.CrewLightMod, (Main.DefaultCrewmateVision +OptionVistion.GetFloat()) > 5f ? 5f : Main.DefaultCrewmateVision + OptionVistion.GetFloat());
            opt.SetFloat(FloatOptionNames.ImpostorLightMod, (Main.DefaultImpostorVision + OptionVistion.GetFloat()) > 5f ? 5f : Main.DefaultImpostorVision + OptionVistion.GetFloat());
        }
}