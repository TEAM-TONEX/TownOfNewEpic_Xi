using Hazel;
using System.Collections.Generic;
using TONEX.Attributes;
using TONEX.Roles.Core;
using UnityEngine;
using UnityEngine.UIElements.UIR;
using static TONEX.Options;

namespace TONEX.Roles.AddOns.Common;
public sealed class Diseased : AddonBase
{
    public static readonly SimpleRoleInfo RoleInfo =
    SimpleRoleInfo.Create(
    typeof(Diseased),
    player => new Diseased(player),
    CustomRoles.Diseased,
    75_1_1_0500,
   SetupOptionItem,
    "dis|ªº’ﬂ|≤°»À",
    "#c0c0c0"
    );
    public Diseased(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    public static List<byte> DisList = new();
    public static OptionItem OptionVistion;
    enum OptionName
    {
        DiseasedVision
    }

    static void SetupOptionItem()
    {
        OptionVistion = FloatOptionItem.Create(RoleInfo, 20, OptionName.DiseasedVision, new(0.5f, 5f, 0.25f), 1.5f, false)
.SetValueFormat(OptionFormat.Multiplier);
    }
    public override void Add() => DisList = new();
    public void SendRPC()
    {
        var sender = CreateSender(CustomRPC.SetDiseasedList);
        sender.Writer.Write(DisList.Count);
        foreach (var pc in DisList)
        {
            sender.Writer.Write(pc);
        }
    }
    public override void ReceiveRPC(MessageReader reader, CustomRPC rpcTypes)
    {
        if (rpcTypes != CustomRPC.SetDiseasedList) return;
        var dis = reader.ReadInt32();
        for (int i = 0; i < dis; i++)
        {
            var pc = reader.ReadByte();
            if (!DisList.Contains(pc))
                DisList.Add(pc);
        }
    }
}