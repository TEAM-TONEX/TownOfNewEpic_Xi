using AmongUs.GameOptions;
using TONEX.Roles.Neutral;

namespace TONEX.Roles.Core.Interfaces.GroupAndRole;

/// <summary>
/// 可以杀死薛定谔的猫并将其引入自己阵营的角色接口。
/// </summary>
public interface ISchrodingerCatOwner
{
    /// <summary>
    /// 杀死薛定谔的猫后的结果角色。
    /// </summary>
    public SchrodingerCat.TeamType SchrodingerCatChangeTo { get; }

    /// <summary>
    /// 对被此角色切割的薛定谔的猫进行可选修改。<br/>
    /// 默认情况下，不进行任何操作。
    /// </summary>
    public void ApplySchrodingerCatOptions(IGameOptions option) { }

    /// <summary>
    /// 在杀死薛定谔的猫时执行的额外动作。
    /// </summary>
    public void OnSchrodingerCatKill(SchrodingerCat schrodingerCat) { }
}
