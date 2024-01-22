using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces;

namespace TONEX.Roles.Neutral;
public sealed class Jester : RoleBase, IIndependent
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Jester),
            player => new Jester(player),
            CustomRoles.Jester,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Neutral,
            50000,
            SetupOptionItem,
            "je|С��|���",
            "#ec62a5"
        );
    public Jester(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    static OptionItem OptionCanUseButton;
    enum OptionName
    {
        JesterCanUseButton
    }
    private static void SetupOptionItem()
    {
        OptionCanUseButton = BooleanOptionItem.Create(RoleInfo, 10, OptionName.JesterCanUseButton, false, false);
    }
    public override Action CheckExile(GameData.PlayerInfo exiled, ref bool DecidedWinner, ref List<string> WinDescriptionText)
    {
        if (!AmongUsClient.Instance.AmHost || Player.PlayerId != exiled.PlayerId) return null;

        DecidedWinner = true;
        WinDescriptionText.Add(Translator.GetString("ExiledJester"));
        return () =>
        {
            CustomWinnerHolder.SetWinnerOrAdditonalWinner(CustomWinner.Jester);
            CustomWinnerHolder.WinnerIds.Add(Player.PlayerId);
        };
    }
    public override bool OnCheckReportDeadBody(PlayerControl reporter, GameData.PlayerInfo target)
    {
        if (Is(reporter) && target == null && !OptionCanUseButton.GetBool())
        {
            Logger.Info("���ֹС���ĵ�ȡ������", "Jester.OnCheckReportDeadBody");
            return false;
        }
        return true;
    }
}