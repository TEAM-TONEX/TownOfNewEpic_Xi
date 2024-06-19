using AmongUs.GameOptions;
using Hazel;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Impostor;
public sealed class Skinwalker : RoleBase, IImpostor
{

    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Skinwalker),
            player => new Skinwalker(player),
            CustomRoles.Skinwalker,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            199874,
            SetupOptionItem,
            "sh|化形",
           experimental: true
        );
    public Skinwalker(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        TargetSkins = new();
        KillerSkins = new();
        KillerSpeed = new();
        KillerName = "";
        TargetSpeed = new();
        TargetName = "";
    }
    public NetworkedPlayerInfo.PlayerOutfit TargetSkins = new();
    public NetworkedPlayerInfo.PlayerOutfit KillerSkins = new();
    public float KillerSpeed = new();
    public string KillerName = "";
    public float TargetSpeed = new();
    public string TargetName = "";
    static OptionItem KillCooldown;
    private static void SetupOptionItem()
    {
        KillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 35f, false)
             .SetValueFormat(OptionFormat.Seconds);
    }
    public override void Add()
    {
        TargetSkins = new();
        KillerSkins = new();
        KillerSpeed = new();
        KillerName = "";
        TargetSpeed = new();
        TargetName = "";
    }
    public float CalculateKillCooldown() => KillCooldown.GetFloat();
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        KillerSkins = new NetworkedPlayerInfo.PlayerOutfit().Set(killer.GetRealName(), killer.Data.DefaultOutfit.ColorId, killer.Data.DefaultOutfit.HatId, killer.Data.DefaultOutfit.SkinId, killer.Data.DefaultOutfit.VisorId, killer.Data.DefaultOutfit.PetId);

        TargetSkins = new NetworkedPlayerInfo.PlayerOutfit().Set(target.GetRealName(), target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.PetId);
        TargetSpeed = Main.AllPlayerSpeed[target.PlayerId];
        TargetName = Main.AllPlayerNames[target.PlayerId];
        KillerSpeed = Main.AllPlayerSpeed[killer.PlayerId];
        KillerName = Main.AllPlayerNames[killer.PlayerId];
        target.SetOutFitStatic(killer.Data.DefaultOutfit.ColorId);
        var sender = CustomRpcSender.Create(name: $"RpcSetSkin({target.Data.PlayerName})");

        Logger.Info($"Pet={killer.Data.DefaultOutfit.PetId}", "RpcSetSkin");
        new LateTask(() =>
        {
            Main.AllPlayerSpeed[killer.PlayerId] = TargetSpeed;
            var outfit = TargetSkins;
            var outfit2 = KillerSkins;
            //凶手变样子
            killer.SetOutFitStatic(outfit.ColorId, outfit.HatId, outfit.SkinId, outfit.VisorId, outfit.PetId);
            Main.AllPlayerNames[killer.PlayerId] = TargetName;
            Main.AllPlayerNames[target.PlayerId] = KillerName; 
            killer.RpcSetName(TargetName);
            target.RpcSetName(KillerName);
            Main.AllPlayerSpeed[target.PlayerId] = KillerSpeed;
            target.SetOutFitStatic(outfit2.ColorId, outfit2.HatId, outfit2.SkinId, outfit2.VisorId, outfit2.PetId);
        }, 0.2f, "Clam");
            killer.RpcMurderPlayerV2(target);
        target.SetRealKiller(killer);
        new LateTask(() =>
        {
            Utils.NotifyRoles(target);
            Utils.NotifyRoles(killer);
            Utils.NotifyRoles();
        /*    target.RpcSetName(KillerName);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, -1);
            writer.Write(KillerName);
            AmongUsClient.Instance.FinishRpcImmediately(writer);*/
        }, 0.5f, "Clam");

        return false;
    }
}
