using AmongUs.GameOptions;
using System.Linq;

using TONEX.Roles.Core;
using TONEX.Roles.Crewmate;
using TONEX.Roles.Impostor;
using TONEX.Roles.Neutral;
using TONEX.Roles.Vanilla;

namespace TONEX;

static class CustomRolesHelper
{
    /// <summary>所有角色（不包括附加）</summary>
    public static readonly CustomRoles[] AllRoles = EnumHelper.GetAllValues<CustomRoles>().Where(role => role < CustomRoles.NotAssigned).ToArray();
    /// <summary>所有附加</summary>
    public static readonly CustomRoles[] AllAddOns = EnumHelper.GetAllValues<CustomRoles>().Where(role => role > CustomRoles.NotAssigned).ToArray();
    /// <summary>可以在标准模式下出现的所有角色</summary>
    public static readonly CustomRoles[] AllStandardRoles = AllRoles.Concat(AllAddOns).ToList().ToArray();
    /// <summary>所有职业类型</summary>
    public static readonly CustomRoleTypes[] AllRoleTypes = EnumHelper.GetAllValues<CustomRoleTypes>();

    /// <summary>是否内鬼</summary>
    public static bool IsImpostor(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null && (int)role >= 100 && (int)role < 400)
            return roleInfo.CustomRoleType == CustomRoleTypes.Impostor;
        return role is CustomRoles.Impostor or CustomRoles.Shapeshifter or
            CustomRoles.Phantom or CustomRoles.ImpostorGhost;
    }
    public static bool IsImpostorTeam(this CustomRoles role) => role.IsImpostor() || role is CustomRoles.Madmate;
    public static bool IsNeutral(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null && (int)role >= 800 && (int)role < 1500)
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
            return (roleInfo.CustomRoleType == CustomRoleTypes.Neutral && roleInfo.IsNK) || role ==CustomRoles.Opportunist && Opportunist.OptionCanKill.GetBool();
        return false;
    }

    public static bool IsCrewmate(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null && (int)role >= 400 && (int)role < 800)
            return roleInfo.CustomRoleType == CustomRoleTypes.Crewmate;
        return
            role is CustomRoles.Crewmate or
            CustomRoles.Engineer or
            CustomRoles.Tracker or
            CustomRoles.Noisemaker or
            CustomRoles.CrewmateGhost or
            CustomRoles.GuardianAngel or
            CustomRoles.Scientist;
    }
    public static bool IsAddon(this CustomRoles role) => (int)role > 1500;
    public static bool IsValid(this CustomRoles role) => role is not CustomRoles.GM and not CustomRoles.NotAssigned;
    public static bool IsExist(this CustomRoles role, bool CountDeath = false) => Main.AllPlayerControls.Any(x => x.Is(role) && (x.IsAlive() || CountDeath));
    public static bool IsVanilla(this CustomRoles role)
    {
        return
            role is 
            
            CustomRoles.Crewmate or
            CustomRoles.Engineer or
            CustomRoles.Scientist or
            CustomRoles.GuardianAngel or
            CustomRoles.ImpostorGhost or
            CustomRoles.CrewmateGhost or
            CustomRoles.Impostor or
            CustomRoles.Shapeshifter or
            CustomRoles.Tracker or
            CustomRoles.Noisemaker or
            CustomRoles.Phantom;
    }

    public static bool IsHidden(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();

        return roleInfo?.IsHidden ?? false;
    }
    public static bool IsTODO(this CustomRoles role)
    {
        if (role is
    CustomRoles.MirrorSpirit or//TODO 镜妖
    CustomRoles.MimicTeam or//TODO 模仿者团队
    CustomRoles.MimicKiller or//TODO 模仿者（杀手）
    CustomRoles.MimicAssistant or//TODO 模仿者（助手）
    CustomRoles.Disperser or//TODO 分散者
    CustomRoles.EvilPianist or//TODO 邪恶的钢琴家
    CustomRoles.Perfumer or //TODO 香水师
    CustomRoles.Captain or// TODO 舰长
    CustomRoles.VirtueGuider or //TODO 善导者，TONEXEX的舰长
    CustomRoles.NiceTracker or//TODO 正义的追踪者
    CustomRoles.NiceInvisibler or//TODO 影行者（正义隐身）
    CustomRoles.Alien or //TODO 外星人
    CustomRoles.Spy or//TODO 卧底
    CustomRoles.NicePianist or//TODO 正义的钢琴家
    CustomRoles.Sloth or//TODO 树懒
    CustomRoles.Bees or//TODO 蜜蜂
    CustomRoles.CopyCat or//TODO 效颦者
    CustomRoles.Konan or//TODO 柯南
    CustomRoles.PVPboss or//TODO PVP大佬
   CustomRoles.Changger or//TODO 连环交换师
    CustomRoles.Amnesiac or//TODO 失忆者
    CustomRoles.PoliticalStrategists or//TODO 纵横家

    CustomRoles.Challenger or//TODO 挑战者

    CustomRoles.NightWolf or//TORELRASE 月下狼人-1.4
    CustomRoles.Moonshadow or//TODO 月影 1.4限定
     CustomRoles.Professional or//TODO 专业赌怪
    CustomRoles.Luckless or//TODO 倒霉蛋
    CustomRoles.FateFavor or//TODO 命运眷顾者
    CustomRoles.IncorruptibleOfficial or//TODO 清廉之官
    CustomRoles.VIP or//TODO VIP
      CustomRoles.Non_Villain //不演反派

            )
            return true;
        return false;
    }
    public static bool IsCanNotOpen(this CustomRoles role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.CantOpen;
        return false;
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
        if (role.IsVanilla() && Options.DisableVanillaRoles.GetBool())
        {
            var roleOpt = Main.NormalOptions.RoleOptions;
            return role switch
            {
                CustomRoles.Engineer => roleOpt.GetNumPerGame(RoleTypes.Engineer),
                CustomRoles.Scientist => roleOpt.GetNumPerGame(RoleTypes.Scientist),
                CustomRoles.Shapeshifter => roleOpt.GetNumPerGame(RoleTypes.Shapeshifter),
                CustomRoles.GuardianAngel => roleOpt.GetNumPerGame(RoleTypes.GuardianAngel),
                CustomRoles.Noisemaker => roleOpt.GetNumPerGame(RoleTypes.Noisemaker),
                CustomRoles.Tracker => roleOpt.GetNumPerGame(RoleTypes.Tracker),
                CustomRoles.Phantom => roleOpt.GetNumPerGame(RoleTypes.Phantom),
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
        if (role.IsVanilla() && Options.DisableVanillaRoles.GetBool())
        {
            var roleOpt = Main.NormalOptions.RoleOptions;
            return role switch
            {
                CustomRoles.Engineer => roleOpt.GetChancePerGame(RoleTypes.Engineer),
                CustomRoles.Scientist => roleOpt.GetChancePerGame(RoleTypes.Scientist),
                CustomRoles.Shapeshifter => roleOpt.GetChancePerGame(RoleTypes.Shapeshifter),
                CustomRoles.GuardianAngel => roleOpt.GetChancePerGame(RoleTypes.GuardianAngel),
                CustomRoles.Noisemaker => roleOpt.GetChancePerGame(RoleTypes.Noisemaker),
                CustomRoles.Tracker => roleOpt.GetChancePerGame(RoleTypes.Tracker),
                CustomRoles.Phantom => roleOpt.GetChancePerGame(RoleTypes.Phantom),
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
            RoleTypes.Tracker => CustomRoles.Tracker,
            RoleTypes.Noisemaker => CustomRoles.Noisemaker,
            RoleTypes.GuardianAngel => CustomRoles.GuardianAngel,
            RoleTypes.ImpostorGhost =>CustomRoles.ImpostorGhost,
            RoleTypes.CrewmateGhost => CustomRoles.CrewmateGhost,
            RoleTypes.Shapeshifter => CustomRoles.Shapeshifter,
            RoleTypes.Phantom => CustomRoles.Phantom,
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
            CustomRoles.ImpostorGhost => RoleTypes.ImpostorGhost,
            CustomRoles.CrewmateGhost => RoleTypes.CrewmateGhost,
            _ => role.IsImpostor() ? RoleTypes.Impostor : RoleTypes.Crewmate,
        };
    }
   
}
public enum CountTypes
{
    OutOfGame = CustomWinner.None,
    None = CustomWinner.None,
    Crew = CustomWinner.Crewmate,
    Impostor = CustomWinner.Impostor,
    Jackal= CustomWinner.Jackal,
    Pelican= CustomWinner.Pelican,
    Demon= CustomWinner.Demon,
    BloodKnight= CustomWinner.BloodKnight,
    Succubus= CustomWinner.Succubus,
    Vagator= CustomWinner.Vagator,
    Martyr= CustomWinner.Martyr,
    NightWolf= CustomWinner.NightWolf,
    GodOfPlagues= CustomWinner.GodOfPlagues,
    MeteorArbiter= CustomWinner.MeteorArbiter,
    MeteorMurderer= CustomWinner.MeteorMurderer,
    SharpShooter= CustomWinner.SharpShooter,
    Yandere=CustomWinner.Yandere,
}