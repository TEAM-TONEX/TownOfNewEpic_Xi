using AmongUs.GameOptions;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Translator;
using Hazel;
using System.Collections.Generic;
using TONEX.Roles.Neutral;
using TONEX.Modules.SoundInterface;
using System.Linq;
using TONEX.Roles.Core.Interfaces.GroupAndRole;

namespace TONEX.Roles.Impostor;
public sealed class EvilTimePauser : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(EvilTimePauser),
            player => new EvilTimePauser(player),
            CustomRoles.EvilTimePauser,
            () => Options.UsePets.GetBool() ? RoleTypes.Impostor : RoleTypes.Phantom,
            CustomRoleTypes.Impostor,
            75_1_2_0300,
            SetupOptionItem,
            "shi|时停"
        );
    public EvilTimePauser(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        EvilTimePauserstop = new();
    }

    static OptionItem OptionSkillCooldown;
    static OptionItem OptionSkillDuration;
    enum OptionName
    {
        NiceTimePauserSkillCooldown,
        NiceTimePauserSkillDuration,
    }
    private List<byte> EvilTimePauserstop;
    private float Cooldown;
    
    private static void SetupOptionItem()
    {
        OptionSkillCooldown = FloatOptionItem.Create(RoleInfo, 10, OptionName.NiceTimePauserSkillCooldown, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionSkillDuration = FloatOptionItem.Create(RoleInfo, 13, OptionName.NiceTimePauserSkillDuration, new(2.5f, 180f, 2.5f), 20f, false)
            .SetValueFormat(OptionFormat.Seconds);
        
    }
    public override void Add()
    {
        Cooldown = OptionSkillCooldown.GetFloat();
        CreateCountdown(OptionSkillDuration.GetFloat());
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.PhantomCooldown = Cooldown;
        AURoleOptions.PhantomDuration = 1f;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("NiceTimePauserVetnButtonText");
        return true;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = "TheWorld";
        return true;
    }
    public override bool GetGameStartSound(out string sound)
    {
        sound = "TheWorld";
        return true;
    }

    public override bool OnCheckVanish()
    {
        Player.SyncSettings();
        Player.RpcResetAbilityCooldown();
        ResetCountdown(0);
        
        foreach (var pc in Main.AllPlayerControls.Where(p => p.IsImpTeam()))
            pc.Notify(GetString("NiceTimePauserOnGuard"));
        foreach (var player in Main.AllAlivePlayerControls)
        {
            if (Player == player) continue;
            if (!player.IsAlive() || Pelican.IsEaten(player.PlayerId)) continue;
            NameNotifyManager.Notify(player, Utils.ColorString(Utils.GetRoleColor(CustomRoles.NiceTimePauser), GetString("ForNiceTimePauser")));
            EvilTimePauserstop.Add(player.PlayerId);
            Player.DisableAction(player, ExtendedPlayerControl.PlayerActionType.All);


            new LateTask(() =>
            {
                Player.EnableAction(player, ExtendedPlayerControl.PlayerActionType.All);
                EvilTimePauserstop.Remove(player.PlayerId);
                RPC.PlaySoundRPC(player.PlayerId, Sounds.TaskComplete);
            }, OptionSkillDuration.GetFloat(), "Time Pauser");
        }
        return false;
    }
    public override bool OnCheckReportDeadBody(PlayerControl reporter, NetworkedPlayerInfo target)
    {
        if (EvilTimePauserstop.Contains(reporter.PlayerId))    return false;
        return true;
    }
    public override void OnExileWrapUp(NetworkedPlayerInfo exiled, ref bool DecidedWinner)
    {
        Player.RpcResetAbilityCooldown();
    }
}
