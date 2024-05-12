using AmongUs.GameOptions;
using static TONEX.Translator;
using TONEX.Roles.Core;
using UnityEngine;
using MS.Internal.Xml.XPath;
using static UnityEngine.GraphicsBuffer;
using TONEX.Roles.Neutral;
using System.Collections.Generic;
using Hazel;
using static Il2CppSystem.Net.Http.Headers.Parser;
using TONEX.Modules;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using TONEX.Roles.Core.Interfaces;
using System.Linq;

namespace TONEX.Roles.Neutral;

public sealed class MeteorMurderer : RoleBase, INeutralKiller
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(MeteorMurderer),
            player => new MeteorMurderer(player),
            CustomRoles.MeteorMurderer,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Neutral,
            75_1_2_0200,
            null,
            "Frisk|MeteorMurderer|USF!Frisk",
             "#ff0000",
            true,
            true,
            countType: CountTypes.MeteorMurderer,
            assignCountRule: new(1, 1, 1)
        );
    public MeteorMurderer(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    {
    }
    public bool IsNK { get; private set; } = true;

    #region 全局变量
    public int LOVE;
    public int LVOverFlow;
    public int Shield;
    #endregion
    #region RPC相关
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(LOVE);
        sender.Writer.Write(LVOverFlow);
        sender.Writer.Write(Shield);

    }
    public override void ReceiveRPC(MessageReader reader)
    {

            LOVE = reader.ReadInt32();
            LVOverFlow = reader.ReadInt32();
        Shield = reader.ReadInt32();

    }
    #endregion
    public bool CanUseKillButton() => true;
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public float CalculateKillCooldown() => 25f;
    public override bool OnCheckMurderAsTargetAfter(MurderInfo info)
    {
        if (info.IsSuicide) return true;

        if (Shield >0)
        {
            Shield--;
            SendRPC();
            return false;
        }
        return true;
    }
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        if (info.IsSuicide) return true;
        var (killer, target) = info.AttemptTuple;
        if (LOVE<=16)
        {
            LOVE = 20;
            LVOverFlow += LVOverFlow - 16;
        }
        else
        {
            LOVE += 4;
        }
        Shield = LOVE +LVOverFlow;
        SendRPC();
        return true;
    }

    public override string GetSuffix(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        if (!GameStates.IsInTask || isForMeeting || !Is(seer) || !Is(seen)) return "";
        Color color = Utils.GetRoleColor(CustomRoles.MeteorMurderer);
        if (LOVE >= 10 && LOVE<20)
            color = Color.red;
        if (LOVE == 20)
            color = Palette.Purple;
        var hp = Player.IsAlive() ? Shield + 1 : 0;
        return Utils.ColorString(color, $"(LV{LOVE})"  +$"HP{hp}");
    }
  
    public override bool OnCheckReportDeadBody(PlayerControl reporter, GameData.PlayerInfo target)
    {
        if (Is(reporter) && target != null)
        {
            if (LOVE <= 16)
            {
                LOVE = 20;
                LVOverFlow += LVOverFlow - 16;
            }
            else
            {
                LOVE += 4;
            }
            Shield = LOVE + LVOverFlow;
            SendRPC();
        }
            return true;
    }
}
