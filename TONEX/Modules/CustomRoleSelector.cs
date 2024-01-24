﻿using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using System.Linq;

using TONEX.Roles.Core;

namespace TONEX.Modules;

internal static class CustomRoleSelector
{
    public static Dictionary<PlayerControl, CustomRoles> RoleResult;
    public static IReadOnlyList<CustomRoles> AllRoles => RoleResult.Values.ToList();

    public static void SelectCustomRoles()
    {
        // 开始职业抽取
        RoleResult = new();
        var rd = IRandom.Instance;
        int playerCount = Main.AllAlivePlayerControls.Count();
        int optImpNum = Main.RealOptionsData.GetInt(Int32OptionNames.NumImpostors);
        int optHPNum = HotPotatoManager.HotQuan.GetInt();
        int optNeutralNum = 0;
        if (Options.NeutralRolesMaxPlayer.GetInt() > 0 && Options.NeutralRolesMaxPlayer.GetInt() >= Options.NeutralRolesMinPlayer.GetInt())
            optNeutralNum = rd.Next(Options.NeutralRolesMinPlayer.GetInt(), Options.NeutralRolesMaxPlayer.GetInt() + 1);
        int optNKNum = 0;
        if (Options.NeutralKillingRolesMaxPlayer.GetInt() > 0 && Options.NeutralKillingRolesMaxPlayer.GetInt() >= Options.NeutralKillingRolesMinPlayer.GetInt())
            optNKNum = rd.Next(Options.NeutralKillingRolesMinPlayer.GetInt(), Options.NeutralKillingRolesMaxPlayer.GetInt() + 1);

        int readyRoleNum = 0;
        int readyNeutralNum = 0;
        int readyNKNum = 0;

        List<CustomRoles> rolesToAssign = new();

        List<CustomRoles> roleList = new();
        List<CustomRoles> roleOnList = new();
        List<CustomRoles> ImpOnList = new();
        List<CustomRoles> NeutralOnList = new();
        List<CustomRoles> NKOnList = new();

        List<CustomRoles> roleRateList = new();
        List<CustomRoles> ImpRateList = new();
        List<CustomRoles> NeutralRateList = new();
        List<CustomRoles> NKRateList = new();
        if (Options.CurrentGameMode == CustomGameMode.HotPotato)
        {
            foreach (var pc in Main.AllAlivePlayerControls)
            {
                RoleResult.Add(pc, CustomRoles.ColdPotato);
                HotPotatoManager.IsAliveCold++;
            }
            return;
        }
        foreach (var cr in Enum.GetValues(typeof(CustomRoles)))
        {
            CustomRoles role = (CustomRoles)Enum.Parse(typeof(CustomRoles), cr.ToString());
            if (role.IsVanilla() || role.IsAddon() || !Options.CustomRoleSpawnChances.TryGetValue(role, out var option) || option.Selections.Length != 3) continue;
            if (role is CustomRoles.GM or CustomRoles.NotAssigned) continue;
            if (role is CustomRoles.Mare or CustomRoles.Concealer && Main.NormalOptions.MapId == 5) continue;
            for (int i = 0; i < role.GetAssignCount(); i++)
                roleList.Add(role);
        }

        // 职业设置为：优先
        foreach (var role in roleList.Where(x => Options.GetRoleChance(x) == 2))
        {
            if (role.IsImpostor()) ImpOnList.Add(role);
            else if (role.IsNotNeutralKilling()) NeutralOnList.Add(role);
            else if (role.IsNeutralKilling()) NKOnList.Add(role);
            else roleOnList.Add(role);
        }
        // 职业设置为：启用
        foreach (var role in roleList.Where(x => Options.GetRoleChance(x) == 1))
        {
            if (role.IsImpostor()) ImpRateList.Add(role);
            else if (role.IsNotNeutralKilling()) NeutralRateList.Add(role);
            else if (role.IsNeutralKilling()) NKRateList.Add(role);
            else roleRateList.Add(role);
        }

        #region 抽取隐藏职业
#if RELEASE
        if (!Options.DisableHiddenRoles.GetBool())
        {
            if (readyRoleNum >= playerCount) goto EndOfAssign;

            var sp = UnityEngine.Random.Range(0, 100);
            if (sp < 5 && !rolesToAssign.Contains(CustomRoles.Bard))
            {
                var shouldExecute = true;
                if (ImpRateList.Count > 0)
                {
                    var remove = ImpRateList[rd.Next(0, ImpRateList.Count)];
                    ImpRateList.Remove(remove);
                }
                else if (ImpOnList.Count > 0)
                {
                    var remove = ImpOnList[rd.Next(0, ImpOnList.Count)];
                    ImpOnList.Remove(remove);
                }
                else
                {
                    shouldExecute = false;
                }
                if (shouldExecute)
                {
                    rolesToAssign.Add(CustomRoles.Bard);

                    readyRoleNum++;
                }
                sp = UnityEngine.Random.Range(0, 100);
            }
            if (readyRoleNum >= playerCount) goto EndOfAssign;

            if (sp < 50 && !rolesToAssign.Contains(CustomRoles.Non_Villain) && readyNeutralNum < optNeutralNum)
            {
                var shouldExecute = true;
                if (NeutralRateList.Count > 0)
                {
                    var remove = NeutralRateList[rd.Next(0, NeutralOnList.Count)];
                    NeutralRateList.Remove(remove);
                }
                else if (NeutralOnList.Count > 0)
                {
                    var remove = NeutralOnList[rd.Next(0, NeutralOnList.Count)];
                    NeutralOnList.Remove(remove);
                }
                else
                {
                    shouldExecute = false;
                }
                if (shouldExecute)
                {
                    rolesToAssign.Add(CustomRoles.Non_Villain);
                    readyRoleNum++;
                    readyNeutralNum++;
                }
                sp = UnityEngine.Random.Range(0, 100);
            }
            if (readyRoleNum >= playerCount) goto EndOfAssign;

            /*if (sp < 10 && !rolesToAssign.Contains(CustomRoles.Vagor_FAFL) && readyNKNum < optNKNum && Options.UsePet.GetBool())
            {
                var shouldExecute = true;
                if (NKRateList.Count > 0)
                {
                    var remove = NeKRateList[rd.Next(0, NKRateList.Count)];
                    NKRateList.Remove(remove);
                }
                else if (NKOnList.Count > 0)
                {
                    var remove = NKOnList[rd.Next(0, NKOnList.Count)];
                    NKOnList.Remove(remove);
                }
                else
                {
                    shouldExecute = false;
                }
                if (shouldExecute)
                {
                    rolesToAssign.Add(CustomRoles.Vagor_FAFL);
                    readyRoleNum++;
                    readyNKNum++;
                }
                sp = UnityEngine.Random.Range(0, 100);
            }
            if (readyRoleNum >= playerCount) goto EndOfAssign;*/

            /*if (sp < 3 && !rolesToAssign.Contains(CustomRoles.Sunnyboy) && readyNeutralNum < optNeutralNum)
            {
                var shouldExecute = true;
                if (NeutralRateList.Count > 0)
                {
                    var remove = NeutralRateList[rd.Next(0, NeutralRateList.Count)];
                    NeutralRateList.Remove(remove);
                }
                else if (NeutralOnList.Count > 0)
                {
                    var remove = NeutralOnList[rd.Next(0, NeutralOnList.Count)];
                    NeutralOnList.Remove(remove);
                }
                else
                {
                    shouldExecute = false;
                }
                if (shouldExecute)
                {
                    rolesToAssign.Add(CustomRoles.Sunnyboy);
                    readyRoleNum++;
                    readyNeutralNum++;
                }
                sp = UnityEngine.Random.Range(0, 100);
            }
            if (readyRoleNum >= playerCount) goto EndOfAssign;*/
        }
#endif
#endregion
        // 抽取优先职业（内鬼）
        while (ImpOnList.Count > 0)
        {
            var select = ImpOnList[rd.Next(0, ImpOnList.Count)];
            ImpOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleNum++;
            Logger.Info(select.ToString() + " 加入内鬼职业待选列表（优先）", "CustomRoleSelector");
            if (readyRoleNum >= playerCount) goto EndOfAssign;
            if (readyRoleNum >= optImpNum) break;
        }
        // 优先职业不足以分配，开始分配启用的职业（内鬼）
        if (readyRoleNum < playerCount && readyRoleNum < optImpNum)
        {
            while (ImpRateList.Count > 0)
            {
                var select = ImpRateList[rd.Next(0, ImpRateList.Count)];
                ImpRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleNum++;
                Logger.Info(select.ToString() + " 加入内鬼职业待选列表", "CustomRoleSelector");
                if (readyRoleNum >= playerCount) goto EndOfAssign;
                if (readyRoleNum >= optImpNum) break;
            }
        }
        // 抽取优先职业（中立杀手）
        while (NKOnList.Count > 0 && optNKNum > 0)
        {
            var select = NKOnList[rd.Next(0, NKOnList.Count)];
            NKOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleNum++;
            readyNKNum += select.GetAssignCount();
            Logger.Info(select.ToString() + " 加入中立职业待选列表（优先）", "CustomRoleSelector");
            if (readyRoleNum >= playerCount) goto EndOfAssign;
            if (readyNKNum >= optNKNum) break;
        }
        // 优先职业不足以分配，开始分配启用的职业（中立杀手）
        if (readyRoleNum < playerCount && readyNKNum < optNKNum)
        {
            while (NKRateList.Count > 0 && optNKNum > 0)
            {
                var select = NKRateList[rd.Next(0, NKRateList.Count)];
                NKRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleNum++;
                readyNKNum += select.GetAssignCount();
                Logger.Info(select.ToString() + " 加入中立职业待选列表", "CustomRoleSelector");
                if (readyRoleNum >= playerCount) goto EndOfAssign;
                if (readyNKNum >= optNKNum) break;
            }
        }
        // 抽取优先职业（中立）
        while (NeutralOnList.Count > 0 && optNeutralNum > 0)
        {
            
            
            
            var select = NeutralOnList[rd.Next(0, NeutralOnList.Count)];
            NeutralOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleNum++;
            readyNeutralNum += select.GetAssignCount();
            Logger.Info(select.ToString() + " 加入中立职业待选列表（优先）", "CustomRoleSelector");
            if (readyRoleNum >= playerCount) goto EndOfAssign;
            if (readyNeutralNum >= optNeutralNum) break;
        }
        // 优先职业不足以分配，开始分配启用的职业（中立）
        if (readyRoleNum < playerCount && readyNeutralNum < optNeutralNum)
        {
            while (NeutralRateList.Count > 0 && optNeutralNum > 0)
            {
                var select = NeutralRateList[rd.Next(0, NeutralRateList.Count)];
                NeutralRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleNum++;
                readyNeutralNum += select.GetAssignCount();
                Logger.Info(select.ToString() + " 加入中立职业待选列表", "CustomRoleSelector");
                if (readyRoleNum >= playerCount) goto EndOfAssign;
                if (readyNeutralNum >= optNeutralNum) break;
            }
        }

        // 抽取优先职业
        while (roleOnList.Count > 0)
        {
            var select = roleOnList[rd.Next(0, roleOnList.Count)];
            roleOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleNum++;
            Logger.Info(select.ToString() + " 加入船员职业待选列表（优先）", "CustomRoleSelector");
            if (readyRoleNum >= playerCount) goto EndOfAssign;
        }
        // 优先职业不足以分配，开始分配启用的职业
        if (readyRoleNum < playerCount)
        {
            while (roleRateList.Count > 0)
            {
                var select = roleRateList[rd.Next(0, roleRateList.Count)];
                roleRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleNum++;
                Logger.Info(select.ToString() + " 加入船员职业待选列表", "CustomRoleSelector");
                if (readyRoleNum >= playerCount) goto EndOfAssign;
            }
        }

    // 职业抽取结束
    EndOfAssign:
       

        // Dev Roles List Edit
        foreach (var dr in Main.DevRole)
        {
            if (dr.Key == PlayerControl.LocalPlayer.PlayerId && Options.EnableGM.GetBool()) continue;
            if (rolesToAssign.Contains(dr.Value))
            {
                rolesToAssign.Remove(dr.Value);
                rolesToAssign.Insert(0, dr.Value);
                Logger.Info("职业列表提高优先：" + dr.Value, "Dev Role");
                continue;
            }
            for (int i = 0; i < rolesToAssign.Count; i++)
            {
                var role = rolesToAssign[i];
                if (Options.GetRoleChance(dr.Value) != Options.GetRoleChance(role)) continue;
                if (
                    (dr.Value.IsImpostor() && role.IsImpostor()) ||
                    (dr.Value.IsNeutral() && role.IsNeutral()) ||
                    (dr.Value.IsCrewmate() & role.IsCrewmate())
                    )
                {
                    rolesToAssign.RemoveAt(i);
                    rolesToAssign.Insert(0, dr.Value);
                    Logger.Info("覆盖职业列表：" + i + " " + role.ToString() + " => " + dr.Value, "Dev Role");
                    break;
                }
            }
        }

        var AllPlayer = Main.AllAlivePlayerControls.ToList();

        while (AllPlayer.Count > 0 && rolesToAssign.Count > 0)
        {
            PlayerControl delPc = null;
            foreach (var pc in AllPlayer)
                foreach (var dr in Main.DevRole.Where(x => pc.PlayerId == x.Key))
                {
                    if (dr.Key == PlayerControl.LocalPlayer.PlayerId && Options.EnableGM.GetBool()) continue;
                    var id = rolesToAssign.IndexOf(dr.Value);
                    if (id == -1) continue;
                    RoleResult.Add(pc, rolesToAssign[id]);
                    Logger.Info($"职业优先分配：{AllPlayer[0].GetRealName()} => {rolesToAssign[id]}", "CustomRoleSelector");
                    delPc = pc;
                    rolesToAssign.RemoveAt(id);
                    goto EndOfWhile;
                }

            var roleId = rd.Next(0, rolesToAssign.Count);
            RoleResult.Add(AllPlayer[0], rolesToAssign[roleId]);
            Logger.Info($"职业分配：{AllPlayer[0].GetRealName()} => {rolesToAssign[roleId]}", "CustomRoleSelector");
            AllPlayer.RemoveAt(0);
            rolesToAssign.RemoveAt(roleId);

        EndOfWhile:;
            if (delPc != null)
            {
                AllPlayer.Remove(delPc);
                Main.DevRole.Remove(delPc.PlayerId);
            }
        }

        if (AllPlayer.Count > 0)
            Logger.Error("职业分配错误：存在未被分配职业的玩家", "CustomRoleSelector");
        if (rolesToAssign.Count > 0)
            Logger.Error("职业分配错误：存在未被分配的职业", "CustomRoleSelector");

    }

    public static int addScientistNum = 0;
    public static int addEngineerNum = 0;
    public static int addShapeshifterNum = 0;
    public static void CalculateVanillaRoleCount()
    {
        // 计算原版特殊职业数量
        addEngineerNum = 0;
        addScientistNum = 0;
        addShapeshifterNum = 0;
        foreach (var role in AllRoles)
        {
            switch (role.GetRoleInfo()?.BaseRoleType.Invoke())
            {
                case RoleTypes.Scientist: addScientistNum++; break;
                case RoleTypes.Engineer: addEngineerNum++; break;
                case RoleTypes.Shapeshifter: addShapeshifterNum++; break;
            }
        }
    }
    public static int GetRoleTypesCount(RoleTypes type)
    {
        return type switch
        {
            RoleTypes.Engineer => addEngineerNum,
            RoleTypes.Scientist => addScientistNum,
            RoleTypes.Shapeshifter => addShapeshifterNum,
            _ => 0
        };
    }
    public static int GetAssignCount(this CustomRoles role)
    {
        int maximumCount = role.GetCount();
        int assignUnitCount = CustomRoleManager.GetRoleInfo(role)?.AssignUnitCount ??
            role switch
            {
                CustomRoles.Lovers => 2,
                _ => 1,
            };
        return maximumCount / assignUnitCount;
    }

    public static List<CustomRoles> AddonRolesList = new();
    public static void SelectAddonRoles()
    {
        if (Options.CurrentGameMode == CustomGameMode.HotPotato) return;
        AddonRolesList = new();
        foreach (var cr in Enum.GetValues(typeof(CustomRoles)))
        {
            CustomRoles role = (CustomRoles)Enum.Parse(typeof(CustomRoles), cr.ToString());
            if (!role.IsAddon()) continue;
            //if (role is CustomRoles.Madmate && Options.MadmateSpawnMode.GetInt() != 0) continue;
            if (role is CustomRoles.Lovers or CustomRoles.LastImpostor or CustomRoles.Workhorse) continue;
            AddonRolesList.Add(role);
        }
    }
}
