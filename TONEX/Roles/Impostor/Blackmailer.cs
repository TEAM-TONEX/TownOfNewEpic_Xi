using AmongUs.GameOptions;
using TONEX.Roles.Core;
using System;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using static TONEX.Translator;
using System.Collections.Generic;
using TONEX.Modules;

namespace TONEX.Roles.Impostor;
public sealed class Blackmailer : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Blackmailer),
            player => new Blackmailer(player),
            CustomRoles.Blackmailer,
            () => RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            75_1_1_0600,
            SetupOptionItem,
            "bl|勒索"
        );
    public Blackmailer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        ForBlackmailer = new();
        CustomRoleManager.MarkOthers.Add(MarkOthers);
        BlackmailerLimit = new();
    }

    static OptionItem OptionShapeshiftCooldown;
    public static List<byte> ForBlackmailer;
    public static Dictionary<byte, int> BlackmailerLimit;

    enum OptionName
    {
        BlackmailerCooldown,
    }
    private static void SetupOptionItem()
    {
        OptionShapeshiftCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.BlackmailerCooldown, new(2.5f, 180f, 2.5f), 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override string GetMark(PlayerControl seer, PlayerControl seen, bool _ = false)
    {
        //seenが省略の場合seer
        seen ??= seer;
        if (ForBlackmailer.Contains(seen.PlayerId)) return Utils.ColorString(RoleInfo.RoleColor, "‼");
        else
            return "";
    }
    public override void Add()
    {
        ForBlackmailer = new();
        //ForBlackmailer.Add(Player.PlayerId);
        BlackmailerLimit = new();
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.ShapeshifterLeaveSkin = false;
        AURoleOptions.ShapeshifterCooldown = OptionShapeshiftCooldown.GetFloat();
        AURoleOptions.ShapeshifterDuration = 1f;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = Translator.GetString("BlackMailerButtonText");
        return !Shapeshifting;
    }
    private bool Shapeshifting;
    public override bool OnCheckShapeshift(PlayerControl target, ref bool animate)
    {

        Shapeshifting = !Is(target);
        Player.RpcResetAbilityCooldown();
        if (!AmongUsClient.Instance.AmHost) return false;

        if (Shapeshifting)
        {
            if (!target.IsAlive())
                Player.Notify(GetString("TargetIsDead"));
            else if (ForBlackmailer.Contains(target.PlayerId))
                Player.Notify(string.Format(GetString("HasBlackmailed"), target.GetRealName()));
            else
            {
                ForBlackmailer.Add(target.PlayerId);
                Player.Notify(string.Format(GetString("BlackmailSucceed"), target.GetRealName()));
            }
        }
        return false;
    }
    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
    {
        ForBlackmailer.Clear();
    }
    string Name = "";
    
    public override void OnStartMeeting()
    {
        foreach (var target in ForBlackmailer)
        {
            var player = Utils.GetPlayerById(target);
            Name = player.GetRealName();
            foreach (var pc in Main.AllPlayerControls)
            {
                Utils.SendMessageV2(string.Format(Translator.GetString("ForBlackmailer"), Name), pc.PlayerId, Utils.ColorString(RoleInfo.RoleColor, Translator.GetString("BlackmailerNewsTitle")));
            }
        }
    }
    public static string MarkOthers(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        seen ??= seer;
        return (ForBlackmailer.Contains(seen.PlayerId) && isForMeeting) ? Utils.ColorString(RoleInfo.RoleColor, "‼") : "";
    }
}