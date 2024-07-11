using AmongUs.GameOptions;
using static TONEX.Translator;
using TONEX.Roles.Core;
using MS.Internal.Xml.XPath;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Ghost.Impostor;
public sealed class EvilAngel : RoleBase, IImpostor
{

    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(EvilAngel),
            player => new EvilAngel(player),
            CustomRoles.EvilAngel,
            () => RoleTypes.GuardianAngel,
            CustomRoleTypes.Impostor,
            94_1_5_0100,
           null,
            "eg|邪恶守护",
            ctop: true
        );
    public EvilAngel(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Maxi = OptionTryMax.GetInt();
    }
    public static bool SetYet;
    public static PlayerControl SetPlayer;
    public static OptionItem EnableEvilAngel;
    static OptionItem OptionProbability;
    static OptionItem OptionKiilCooldown;
    static OptionItem OptionTryMax;
    int Maxi;
    public override void OnGameStart()
    {
        SetYet = false;
    }
    public static bool CheckForSet(PlayerControl pc)
    {
        if (SetYet || !EnableEvilAngel.GetBool()) return false;

        SetYet = true;

        pc.Notify(GetString("Surprise"));
        pc.RpcSetRole(RoleTypes.GuardianAngel);
        pc.RpcSetCustomRole(CustomRoles.EvilAngel);

        return true;
    }
    public override void OverrideDisplayRoleNameAsSeen(PlayerControl seer, ref bool enabled, ref UnityEngine.Color roleColor, ref string roleText)
    => enabled |= true;
    public static void SetupOptionItem()
    {
        EnableEvilAngel = BooleanOptionItem.Create(94_1_5_0110, "EnableEvilAngel", false, TabGroup.ImpostorRoles, false)
            .SetHeader(true)
            .SetGameMode(CustomGameMode.Standard);
        OptionProbability = IntegerOptionItem.Create(94_1_5_0111, "KillProbability", new(0, 100, 5), 40, TabGroup.ImpostorRoles, false)
            .SetValueFormat(OptionFormat.Percent)
            .SetParent(EnableEvilAngel);
        OptionKiilCooldown = FloatOptionItem.Create(94_1_5_0112,"KillCooldown", new(0, 100, 5), 40, TabGroup.ImpostorRoles, false)
            .SetValueFormat(OptionFormat.Seconds)
            .SetParent(EnableEvilAngel);
        OptionTryMax = IntegerOptionItem.Create(94_1_5_0113, "OptionTryMax", new(0, 100, 5), 40, TabGroup.ImpostorRoles, false)
            .SetValueFormat(OptionFormat.Times)
            .SetParent(EnableEvilAngel);
    }
    public override bool CanUseAbilityButton() => true;
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("KillButtonText");
        return true;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName= "KillButton";
        return true;
    }
    public override bool OnProtectPlayer(PlayerControl target)
    {
        if (Player.IsAlive() ||Maxi <=0) return false;
        Maxi --;
        if (IRandom.Instance.Next(0, 100) < OptionProbability.GetInt())
        {
            target.Notify(string.Format(GetString("KillForEvilAngel")));
            Player.RpcTeleport(target.GetTruePosition());
            RPC.PlaySoundRPC(Player.PlayerId, Sounds.KillSound);
            Player.RpcMurderPlayerV2(target);
            target.SetRealKiller(Player);
        }
        return false;
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.GuardianAngelCooldown = OptionKiilCooldown.GetFloat();
    }
}
