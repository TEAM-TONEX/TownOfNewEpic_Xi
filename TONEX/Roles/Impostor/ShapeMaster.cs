using AmongUs.GameOptions;

using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Impostor;
public sealed class ShapeMaster : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(ShapeMaster),
            player => new ShapeMaster(player),
            CustomRoles.ShapeMaster,
            () => RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            1300,
            SetupOptionItem,
            "sha|ǧ���|ǧ��"
        );
    public ShapeMaster(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        shapeshiftDuration = OptionShapeshiftDuration.GetFloat();
    }
    private static OptionItem OptionShapeshiftDuration;

    private static float shapeshiftDuration;

    public static void SetupOptionItem()
    {
        OptionShapeshiftDuration = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.ShapeshiftDuration, new(1, 1000, 1), 10, false)
            .SetValueFormat(OptionFormat.Seconds);
    }

    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.ShapeshifterCooldown = 0f;
        AURoleOptions.ShapeshifterLeaveSkin = false;
        AURoleOptions.ShapeshifterDuration = shapeshiftDuration;
    }
}
