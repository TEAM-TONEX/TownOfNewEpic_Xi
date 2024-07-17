using UnityEngine;
using TONEX.Roles.Core;

namespace TONEX.Modules.OptionItems.Interfaces;

public interface IRoleOptionItem
{
    public CustomRoles RoleId { get; }
    public Color RoleColor { get; }
}
