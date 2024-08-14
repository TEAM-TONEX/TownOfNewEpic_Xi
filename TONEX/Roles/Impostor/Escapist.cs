using AmongUs.GameOptions;
using Hazel;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using TONEX.Roles.Core.Interfaces.GroupAndRole;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX.Roles.Impostor;
public sealed class Escapist : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Escapist),
            player => new Escapist(player),
            CustomRoles.Escapist,
       () => Options.UsePets.GetBool() ? RoleTypes.Impostor : RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            2000,
            null,
            "ec|逃逸"
        );
    public Escapist(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        Marked = false;
    }

    private bool Shapeshifting;
    private bool Marked;
    private Vector2 MarkedPosition;
    
    public override void Add()
    {
        Marked = false;
        Shapeshifting = false;
        
    }
    public override long UsePetCooldown { get; set; } = (long)AURoleOptions.PhantomCooldown;
    public override bool EnablePetSkill() => true;
    private void SendRPC()
    {
        using var sender = CreateSender();
        sender.Writer.Write(Marked);
    }
    public override void ReceiveRPC(MessageReader reader)
    {
        
        Marked = reader.ReadBoolean();
    }
    public override bool GetAbilityButtonText(out string text)
    {
        text = Marked ? Translator.GetString("EscapistTeleportButtonText") : Translator.GetString("EscapistMarkButtonText");
        return !Shapeshifting;
    }
    public override bool GetPetButtonText(out string text)
    {
        text = Marked ? Translator.GetString("EscapistTeleportButtonText") : Translator.GetString("EscapistMarkButtonText");
        return PetUnSet();
    }
    public override bool GetPetButtonSprite(out string buttonName)
    {
        buttonName = "Telport";
        return PetUnSet();
    }
    public override void OnUsePet()
    {
        if (!Options.UsePets.GetBool()) return;
                if (Marked)
        {
            Marked = false;
            Player.RPCPlayCustomSound("Teleport");
            Player.RpcTeleport(MarkedPosition);
            Logger.Msg($"{Player.GetNameWithRole()}：{MarkedPosition}", "Escapist.OnShapeshift");
        }
        else
        {
            MarkedPosition = Player.GetTruePosition();
            Marked = true;
            SendRPC();
        }
        return;
    }
    public override bool OnVanish()
    {

        if (!AmongUsClient.Instance.AmHost) return false;

        if (Marked)
        {
            Marked = false;
            Player.RPCPlayCustomSound("Teleport");
            Player.RpcTeleport(MarkedPosition);
            Logger.Msg($"{Player.GetNameWithRole()}：{MarkedPosition}", "Escapist.OnShapeshift");
        }
        else
        {
            MarkedPosition = Player.GetTruePosition();
            Marked = true;
            SendRPC();
        }
        return false;
    }
}