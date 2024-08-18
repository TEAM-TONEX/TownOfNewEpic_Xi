using AmongUs.GameOptions;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX.Roles.Impostor;
public sealed class Warlock : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Warlock),
            player => new Warlock(player),
            CustomRoles.Warlock,
       () => Options.UsePets.GetBool() ? RoleTypes.Impostor : RoleTypes.Phantom,
            CustomRoleTypes.Impostor,
            1500,
            SetupOptionItem,
            "wa|術士"
        );
    public Warlock(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    { }
    public override long UsePetCooldown { get; set; } = 0;
    public override bool EnablePetSkill() => true;
    public override void OnDestroy()
    {
        CursedPlayer = null;
    }
    static OptionItem OptionCanKillAllies;
    static OptionItem OptionCanKillSelf;

    PlayerControl CursedPlayer;
    bool IsCursed;
    
    public float Cooldown;
    private static void SetupOptionItem()
    {
        OptionCanKillAllies = BooleanOptionItem.Create(RoleInfo, 10, GeneralOption.CanKillAllies, false, false);
        OptionCanKillSelf = BooleanOptionItem.Create(RoleInfo, 11, GeneralOption.CanKillSelf, false, false);
    }
    public override void Add()
    {
        CursedPlayer = null;
        IsCursed = false;
        Cooldown = Options.DefaultKillCooldown;
        
    }
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(IsCursed);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        IsCursed = reader.ReadBoolean();
    }
    public bool OverrideKillButtonText(out string text)
    {
        text = GetString("WarlockCurseButtonText");
        return true;
    }
    public bool OverrideKillButtonSprite(out string buttonName)
    {
        buttonName = "Curse";
        return true;;
    }
    public override bool GetGameStartSound(out string sound)
    {
        sound = "Line";
        return true;
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = GetString("WarlockShapeshiftButtonText");
        return IsCursed;
    }
    public override bool GetAbilityButtonSprite(out string buttonName)
    {
        buttonName = "CurseKill";
        return IsCursed;
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.PhantomCooldown = IsCursed ? 1f : Options.DefaultKillCooldown;
    }
    public bool OnCheckMurderAsKiller(MurderInfo info)
    {
        //自殺なら関係ない
        if (info.IsSuicide) return true;

        var (killer, target) = info.AttemptTuple;

            if (!IsCursed)
            {//まだ呪っていない
                IsCursed = true;
                SendRPC();
                CursedPlayer = target;  
                UsePetCooldown_Timer = 1;
                //呪える相手は一人だけなのでキルボタン無効化
                killer.SetKillCooldownV2(255f);
                killer.RpcResetAbilityCooldown();
                killer.RPCPlayCustomSound("Line");
              Cooldown = 1f;
            }
            //どちらにしてもキルは無効
            return false;

    }
    public override bool OnCheckVanish()
    {

        if (!AmongUsClient.Instance.AmHost) return false;

        if (CursedPlayer != null && CursedPlayer.IsAlive())
        {//呪っていて対象がまだ生きていたら
            Vector2 cpPos = CursedPlayer.transform.position;
            Dictionary<PlayerControl, float> candidateList = new();
            float distance;
            foreach (PlayerControl candidatePC in Main.AllAlivePlayerControls)
            {
                if (candidatePC.PlayerId == CursedPlayer.PlayerId) continue;
                if (Is(candidatePC) && !OptionCanKillSelf.GetBool()) continue;
                if ((candidatePC.Is(CustomRoleTypes.Impostor) || candidatePC.Is(CustomRoles.Madmate)) && !OptionCanKillAllies.GetBool()) continue;
                distance = Vector2.Distance(cpPos, candidatePC.transform.position);
                candidateList.Add(candidatePC, distance);
                Logger.Info($"{candidatePC?.Data?.PlayerName}の位置{distance}", "Warlock.OnShapeshift");
            }
            if (candidateList.Count >= 1)
            {
                var nearest = candidateList.OrderBy(c => c.Value).FirstOrDefault();
                var killTarget = nearest.Key;

                var killed = false;
                CustomRoleManager.OnCheckMurder(
                    Player, killTarget,
                    CursedPlayer, killTarget,
                    () => killed = true
                    );

                if (killed)
                {
                    killTarget.SetRealKiller(Player);
                    RPC.PlaySoundRPC(Player.PlayerId, Sounds.KillSound);
                }
                else
                {
                    Player.Notify(GetString("WarlcokKillFaild"));
                }

                Logger.Info($"{killTarget.GetNameWithRole()} 被操控击杀", "Warlock.OnShapeshift");

            }
            else
            {
                Player.Notify(GetString("WarlockNoTarget"));
            }
            Player.SetKillCooldownV2();
            CursedPlayer = null;
            IsCursed = false;
        }
        return false;
    }
    public override void AfterMeetingTasks()
    {
                CursedPlayer = null;
        
        IsCursed = false;
    }
}