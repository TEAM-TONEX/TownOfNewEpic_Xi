using AmongUs.GameOptions;
using Hazel;
using System.Linq;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using TONEX.Roles.Neutral;
using UnityEngine;

namespace TONEX.Roles.Crewmate;
public sealed class Vigilante : RoleBase, IKiller, ISchrodingerCatOwner
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Vigilante),
            player => new Vigilante(player),
            CustomRoles.Vigilante,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Crewmate,
            21400,
            SetupOptionItem,
            "vi|俠客",
            "#C90000",
            true,
            introSound: () => GetIntroSound(RoleTypes.Crewmate)
        );
    public Vigilante(PlayerControl player)
    : base(
        RoleInfo,
        player,
        () => HasTask.False
    )
    { CustomRoleManager.OnCheckMurderPlayerOthers_After.Add(OnCheckMurderPlayerOthers_After); }
    public SchrodingerCat.TeamType SchrodingerCatChangeTo => SchrodingerCat.TeamType.Crew;
    static OptionItem OptionRevengeTimes;
    enum OptionName
    {
        BodyguardProtectRadius
    }

    private static void SetupOptionItem()
    {
        OptionRevengeTimes = IntegerOptionItem.Create(RoleInfo, 10, OptionName.BodyguardProtectRadius, new(0, 15, 1), 1, false)
            .SetValueFormat(OptionFormat.Times);
    }
    private bool IsKilled;
    private int revengeTimes;
    public override void Add()
    {
        var playerId = Player.PlayerId;
        IsKilled = false;
        revengeTimes=OptionRevengeTimes.GetInt();
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(IsKilled);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        IsKilled = reader.ReadBoolean();
    }
    public float CalculateKillCooldown() => CanUseKillButton() ? 0f : 255f;
    public bool CanUseKillButton() => Player.IsAlive() && !IsKilled;
    public bool CanUseSabotageButton() => false;
    public bool CanUseImpostorVentButton() => false;
    public override void ApplyGameOptions(IGameOptions opt) => opt.SetVision(false);
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        if (Is(info.AttemptKiller) && !info.IsSuicide)
        {
            if (IsKilled) return false;
            IsKilled = true;
            SendRPC();
            Player.ResetKillCooldown();
        }
        return true;
    }
    private static bool OnCheckMurderPlayerOthers_After(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;
        
        if (info.IsSuicide) return true;
        foreach (var pc in Main.AllAlivePlayerControls.Where(x => x.PlayerId != target.PlayerId))
        {
            var pos = target.transform.position;
            var dis = Vector2.Distance(pos, pc.transform.position);
            if (dis > Main.DefaultCrewmateVision || target == pc) continue;
            if (pc.Is(CustomRoles.Vigilante))
            {
                var rc = pc.GetRoleClass() as Vigilante;
                if (!rc.IsKilled || rc.revengeTimes <=0) continue;
                rc.revengeTimes--;
                if (killer.Is(CustomRoles.SchrodingerCat))
                {
                    if ((killer.GetRoleClass() as SchrodingerCat).Team == SchrodingerCat.TeamType.None)
                    {
                        (killer.GetRoleClass() as SchrodingerCat).ChangeTeamOnKill(pc);
                        continue;
                    }
                }
                pc.RpcTeleport(killer.GetTruePosition());
                pc.RpcMurderPlayerV2(killer);
            }
        }
        return true;
    }

    public override string GetProgressText(bool comms = false) => Utils.ColorString(CanUseKillButton() ? Utils.GetRoleColor(CustomRoles.Vigilante) : Color.gray, $"({(CanUseKillButton() ? 1 : 0)})");
}