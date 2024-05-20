using AmongUs.GameOptions;
using static TONEX.Translator;
using TONEX.Roles.Core;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TONEX.Roles.Ghost.Crewmate;
public sealed class InjusticeSpirit : RoleBase
{

    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(InjusticeSpirit),
            player => new InjusticeSpirit(player),
            CustomRoles.InjusticeSpirit,
            () => RoleTypes.GuardianAngel,
            CustomRoleTypes.Crewmate,
            75_1_5_0200,
            null,
            "ijs|冤枉",
            "#B6B87B",
            ctop: true
        );
    public InjusticeSpirit(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        
        EnableTargetArrow = OptionEnableTargetArrow.GetBool();
        CanGetColoredArrow = OptionCanGetColoredArrow.GetBool();
        CanFindNeutralEvil = OptionCanFindNeutralEvil.GetBool();
        CanFindNeutralKiller = OptionCanFindNeutralKiller.GetBool();
        CanFindMadmate = OptionCanFindMadmate.GetBool();
        CanFindCharmed = OptionCanFindCharmed.GetBool();
        CanFindWolfmate = OptionCanFindWolfmate.GetBool();
    }
    private static bool EnableTargetArrow;
    private static bool CanGetColoredArrow;
    private static bool CanFindNeutralKiller;
    private static bool CanFindNeutralEvil;
    private static bool CanFindMadmate;
    private static bool CanFindCharmed;
    private static bool CanFindWolfmate;

    //private bool IsExposed = false;
    private bool IsComplete = false;
    public override void OnGameStart()
    {
        SetYet = false;
    }
    //複数Snitchで共有するためstatic
    private static HashSet<byte> TargetList = new();
    private static Dictionary<byte, Color> TargetColorlist = new();
    private static HashSet<byte> ExposedList = new();

    public static OptionItem EnableInjusticeSpirit;
    public static OptionItem OptionTaskCount;
    private static OptionItem OptionEnableTargetArrow;
    private static OptionItem OptionCanGetColoredArrow;
    private static OptionItem OptionCanFindNeutralKiller;
    private static OptionItem OptionCanFindNeutralEvil;
    private static OptionItem OptionCanFindMadmate;
    private static OptionItem OptionCanFindCharmed;
    private static OptionItem OptionCanFindWolfmate;

    public static bool SetYet;
    public static PlayerControl SetPlayer;
    public static void SetupOptionItem()
    {
        EnableInjusticeSpirit = BooleanOptionItem.Create(75_1_5_0210, "EnableInjusticeSpirit", false, TabGroup.CrewmateRoles, false)
            .SetHeader(true)
            .SetGameMode(CustomGameMode.Standard);
        OptionTaskCount = IntegerOptionItem.Create(75_1_5_0211, "OptionTaskCount", new(0, 100, 1), 10, TabGroup.CrewmateRoles, false)
            .SetValueFormat(OptionFormat.Pieces)
            .SetParent(EnableInjusticeSpirit);
        OptionEnableTargetArrow = BooleanOptionItem.Create(75_1_5_0212, "SnitchEnableTargetArrow", true, TabGroup.CrewmateRoles, false).SetParent(EnableInjusticeSpirit);
        OptionCanGetColoredArrow = BooleanOptionItem.Create(75_1_5_0213, "SnitchCanGetArrowColor", true, TabGroup.CrewmateRoles, false).SetParent(EnableInjusticeSpirit);
        OptionCanFindNeutralKiller = BooleanOptionItem.Create(75_1_5_0214, "SnitchCanFindNeutralKiller", true, TabGroup.CrewmateRoles, false).SetParent(EnableInjusticeSpirit);
        OptionCanFindNeutralEvil = BooleanOptionItem.Create(75_1_5_0215, "SnitchCanFindNeutralEvil", true, TabGroup.CrewmateRoles, false).SetParent(EnableInjusticeSpirit);
        OptionCanFindMadmate = BooleanOptionItem.Create(75_1_5_0216, "SnitchCanFindMadmate", false, TabGroup.CrewmateRoles, false).SetParent(EnableInjusticeSpirit);
        OptionCanFindCharmed = BooleanOptionItem.Create(75_1_5_0217, "SnitchCanFindCharmed", false, TabGroup.CrewmateRoles, false).SetParent(EnableInjusticeSpirit);
        OptionCanFindWolfmate = BooleanOptionItem.Create(75_1_5_0218, "SnitchCanFindWolfmate", false, TabGroup.CrewmateRoles, false).SetParent(EnableInjusticeSpirit);
    }
    public override void OverrideDisplayRoleNameAsSeen(PlayerControl seer, ref bool enabled, ref UnityEngine.Color roleColor, ref string roleText)
        => enabled |= true;
    public override bool OnCompleteTask(out bool cancel)
    {
        var update = false;
        if(TargetList.Count == 0)
        {
            //TargetListが未作成ならここで作る
            foreach (var target in Main.AllAlivePlayerControls)
            {
                if (!IsSnitchTarget(target)) continue;

                var targetId = target.PlayerId;
                TargetList.Add(targetId);
                TargetColorlist.Add(targetId, target.GetRoleColor());
            }
        }
        
        if (!IsComplete && IsTaskFinished)
        {
            IsComplete = true;
            foreach (var targetId in TargetList)
            {
                foreach (var pc in Main.AllPlayerControls.Where(p=>!p.IsAlive()||p.IsCrew()))
                NameColorManager.Add(pc.PlayerId, targetId);

                if (EnableTargetArrow)
                    foreach (var pc in Main.AllPlayerControls.Where(p => !p.IsAlive() || p.IsCrew()))
                        TargetArrow.Add(pc.PlayerId, targetId);
            }
            update = true;
        }
        if (update) Utils.NotifyRoles();
        cancel = false;
        return false;
    }
    public override bool CanUseAbilityButton() => false;
    public override bool OnProtectPlayer(PlayerControl target)
    {
        return false;
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.GuardianAngelCooldown =255f;
    }
    private static bool IsSnitchTarget(PlayerControl target)
    {
        return target.Is(CustomRoleTypes.Impostor)
            || (CanFindNeutralKiller && target.IsNeutralKiller())
            || (CanFindNeutralEvil && target.IsNeutralEvil())
            || (CanFindMadmate && target.Is(CustomRoles.Madmate))
            || (CanFindCharmed && target.Is(CustomRoles.Charmed))
            || (CanFindWolfmate && (target.Is(CustomRoles.Wolfmate) || target.Is(CustomRoles.Sidekick) || target.Is(CustomRoles.Whoops)));
    }

    public override string GetSuffix(PlayerControl seer, PlayerControl seen = null, bool isForMeeting = false)
    {
        //矢印表示する必要がなければ無し
        if (!EnableTargetArrow || isForMeeting) return "";

        //seenが省略の場合seer
        seen ??= seer;

        //ともにスニッチでなければ無し
        if (!Is(seer) && !Is(seen)) return "";
        //タスク終わってなければ無し
        if (!IsComplete) return "";

        var arrows = "";
        foreach (var targetId in TargetList)
        {
            var arrow = TargetArrow.GetArrows(seer, targetId);
            arrows += CanGetColoredArrow ? Utils.ColorString(TargetColorlist[targetId], arrow) : arrow;
        }
        return arrows;
    }
}
