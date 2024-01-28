using AmongUs.GameOptions;
using System.Linq;

using TONEX.Roles.Core;

namespace TONEX;

static class CustomRolesHelper
{
    /// <summary>���٤Ƥ���(���ԤϺ��ޤʤ�)</summary>
    public static readonly CustomRoles[] AllRoles = EnumHelper.GetAllValues<CustomRoles>().Where(role => role < CustomRoles.NotAssigned).ToArray();
    /// <summary>���٤Ƥ�����</summary>
    public static readonly CustomRoles[] AllAddOns = EnumHelper.GetAllValues<CustomRoles>().Where(role => role > CustomRoles.NotAssigned).ToArray();
    /// <summary>��������`�ɥ�`�ɤǳ��F�Ǥ��뤹�٤Ƥ���</summary>
    public static readonly CustomRoles[] AllStandardRoles = AllRoles.ToArray();
    public static readonly CustomRoleTypes[] AllRoleTypes = EnumHelper.GetAllValues<CustomRoleTypes>();

    public static bool IsImpostor(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.CustomRoleType == CustomRoleTypes.Impostor;
        return false;
    }
    public static bool IsImpostorTeam(this CustomRoles role) => role.IsImpostor() || role is CustomRoles.Madmate;
    public static bool IsNeutral(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.CustomRoleType == CustomRoleTypes.Neutral;
        return false;
    }
    public static bool IsNotNeutralKilling(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.CustomRoleType == CustomRoleTypes.Neutral && !role.IsNeutralKilling();
        return false;
    }
    public static bool IsNeutralKilling(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.CustomRoleType == CustomRoleTypes.Neutral && roleInfo.IsNK;
        return false;
    }
    public static bool IsHidden(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.IsHidden;
        return false;
    }
    public static bool IsCanNotOpen(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.CantOpen;
        return false;
    }
    public static bool IsCrewmate(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.CustomRoleType == CustomRoleTypes.Crewmate;
        return
            role is CustomRoles.Crewmate or
            CustomRoles.Engineer or
            CustomRoles.Scientist;
    }
    public static bool IsAddon(this CustomRoles role) => (int)role > 500;
    public static bool IsValid(this CustomRoles role) => role is not CustomRoles.GM and not CustomRoles.NotAssigned;
    public static bool IsExist(this CustomRoles role, bool CountDeath = false) => Main.AllPlayerControls.Any(x => x.Is(role) && (x.IsAlive() || CountDeath));
    public static bool IsVanilla(this CustomRoles role)
    {
        return
            role is CustomRoles.Crewmate or
            CustomRoles.Engineer or
            CustomRoles.Scientist or
            CustomRoles.GuardianAngel or
            CustomRoles.Impostor or
            CustomRoles.Shapeshifter;
    }

    public static CustomRoleTypes GetCustomRoleTypes(this CustomRoles role)
    {
        if (role is CustomRoles.NotAssigned) return CustomRoleTypes.Crewmate;
        CustomRoleTypes type = CustomRoleTypes.Crewmate;

        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.CustomRoleType;

        if (role.IsImpostor()) type = CustomRoleTypes.Impostor;
        else if (role.IsCrewmate()) type = CustomRoleTypes.Crewmate;
        else if (role.IsNeutral()) type = CustomRoleTypes.Neutral;
        else if (role.IsAddon()) type = CustomRoleTypes.Addon;

        return type;
    }
    public static int GetCount(this CustomRoles role)
    {
        if (role.IsVanilla())
        {
            var roleOpt = Main.NormalOptions.RoleOptions;
            return role switch
            {
                CustomRoles.Engineer => roleOpt.GetNumPerGame(RoleTypes.Engineer),
                CustomRoles.Scientist => roleOpt.GetNumPerGame(RoleTypes.Scientist),
                CustomRoles.Shapeshifter => roleOpt.GetNumPerGame(RoleTypes.Shapeshifter),
                CustomRoles.GuardianAngel => roleOpt.GetNumPerGame(RoleTypes.GuardianAngel),
                CustomRoles.Crewmate => roleOpt.GetNumPerGame(RoleTypes.Crewmate),
                _ => 0
            };
        }
        else
        {
            return Options.GetRoleCount(role);
        }
    }
    public static int GetChance(this CustomRoles role)
    {
        if (role.IsVanilla())
        {
            var roleOpt = Main.NormalOptions.RoleOptions;
            return role switch
            {
                CustomRoles.Engineer => roleOpt.GetChancePerGame(RoleTypes.Engineer),
                CustomRoles.Scientist => roleOpt.GetChancePerGame(RoleTypes.Scientist),
                CustomRoles.Shapeshifter => roleOpt.GetChancePerGame(RoleTypes.Shapeshifter),
                CustomRoles.GuardianAngel => roleOpt.GetChancePerGame(RoleTypes.GuardianAngel),
                CustomRoles.Crewmate => roleOpt.GetChancePerGame(RoleTypes.Crewmate),
                _ => 0
            };
        }
        else
        {
            return Options.GetRoleChance(role);
        }
    }
    public static bool IsEnable(this CustomRoles role) => role.GetCount() > 0;
    public static CustomRoles GetCustomRoleTypes(this RoleTypes role)
    {
        return role switch
        {
            RoleTypes.Crewmate => CustomRoles.Crewmate,
            RoleTypes.Scientist => CustomRoles.Scientist,
            RoleTypes.Engineer => CustomRoles.Engineer,
            RoleTypes.GuardianAngel => CustomRoles.GuardianAngel,
            RoleTypes.Shapeshifter => CustomRoles.Shapeshifter,
            RoleTypes.Impostor => CustomRoles.Impostor,
            _ => CustomRoles.NotAssigned
        };
    }
    public static RoleTypes GetRoleTypes(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.BaseRoleType.Invoke();
        return role switch
        {
            CustomRoles.GM => RoleTypes.GuardianAngel,

            _ => role.IsImpostor() ? RoleTypes.Impostor : RoleTypes.Crewmate,
        };
    }
}
public enum CountTypes
{
    OutOfGame,
    None,
    Crew,
    Impostor,
    Jackal,
    Pelican,
    Demon,
    BloodKnight,
    Succubus,
    FAFL,
}