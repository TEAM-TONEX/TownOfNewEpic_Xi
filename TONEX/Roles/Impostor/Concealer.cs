using AmongUs.GameOptions;
using System.Linq;
using static TONEX.Translator;
using TONEX.Roles.Core;
using YamlDotNet.Core;

using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Impostor;
public sealed class Concealer : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Concealer),
            player => new Concealer(player),
            CustomRoles.Concealer,
            () => RoleTypes.Phantom,
            CustomRoleTypes.Impostor,
            4500,
            SetupOptionItem,
            "co|隱蔽者|隐蔽|小黑人",
            experimental: true
        );
    public Concealer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }

    static OptionItem OptionShapeshiftCooldown;
    static OptionItem OptionShapeshiftDuration;

    private static void SetupOptionItem()
    {
        OptionShapeshiftCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.ShapeshiftCooldown, new(2.5f, 180f, 2.5f), 25f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionShapeshiftDuration = FloatOptionItem.Create(RoleInfo, 11, GeneralOption.ShapeshiftDuration, new(2.5f, 180f, 2.5f), 10f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {
        CreateCountdown(OptionShapeshiftDuration.GetFloat());
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.PhantomCooldown = OptionShapeshiftCooldown.GetFloat();
        AURoleOptions.PhantomDuration = OptionShapeshiftDuration.GetFloat();
    }
    public bool vanish;
    public override bool OnCheckVanish()
    {
        vanish = true;
        ResetCountdown(0);
        Player.RpcResetAbilityCooldown();
        if (!AmongUsClient.Instance.AmHost) return false;

        Camouflage.CheckCamouflage();
        
        return false;
    }
    public override void AfterOffGuard() {
        vanish = false;
        Camouflage.CheckCamouflage();
    }
    public override long UsePetCooldown { get; set; } = (long)OptionShapeshiftCooldown.GetFloat();
    public override bool EnablePetSkill() => true;
    public static bool IsHidding
        => Main.AllAlivePlayerControls.Any(x => (x.GetRoleClass() is Concealer roleClass) && roleClass.vanish) && GameStates.IsInTask;
}