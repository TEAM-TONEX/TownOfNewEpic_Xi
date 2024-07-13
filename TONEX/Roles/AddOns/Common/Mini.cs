using Hazel;
using System.Collections.Generic;
using System.Linq;
using TONEX.Attributes;
using TONEX.Modules.SoundInterface;
using TONEX.Roles.Core;
using UnityEngine;
using static TONEX.Options;
using System.Text;
using static TONEX.Utils;
using static UnityEngine.GraphicsBuffer;

namespace TONEX.Roles.AddOns.Common;
public static class Mini
{
    private static readonly int Id = 94_1_0_0700;
    private static List<byte> playerIdList = new();
    public static OptionItem OptionAgeTime;
    public static OptionItem OptionNotGrowInMeeting;
    public static OptionItem OptionKidKillCoolDown;
    public static OptionItem OptionAdultKillCoolDown;
    public static Dictionary<byte,int> Age;
    public static int UpTime;
    public static List<byte> MKL;
    public static List<byte> MAL;
    public static void SetupCustomOption()
    {
        SetupAddonOptions(Id, TabGroup.Addons, CustomRoles.Mini);
        AddOnsAssignData.Create(Id + 10, CustomRoles.Mini, true, true, true);
        OptionAgeTime = FloatOptionItem.Create(Id + 20, "MiniUpTime", new(60f, 360f, 2.5f), 180f, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Mini])
.SetValueFormat(OptionFormat.Seconds);
        OptionNotGrowInMeeting = BooleanOptionItem.Create(Id + 21, "NotGrowInMeeting", false, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Mini]);
        OptionKidKillCoolDown = FloatOptionItem.Create(Id + 22, "OptionKidKillCoolDown", new(2.5f, 180f, 2.5f), 45f, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Mini])
            .SetValueFormat(OptionFormat.Seconds);
        OptionAdultKillCoolDown = FloatOptionItem.Create(Id + 23, "OptionAdultKillCoolDown", new(2.5f, 180f, 2.5f), 15f, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Mini])
            .SetValueFormat(OptionFormat.Seconds);
    }
    [GameModuleInitializer]
    public static void Init()
    {
        playerIdList = new();
        Age = new();
        UpTime = -8;
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        Age.TryAdd(playerId, 0);
        CustomRoleManager.OnCheckMurderPlayerOthers_After.Add(OnCheckMurderPlayerOthers_After);
    }
   public static void SendRPC()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MiniAge, SendOption.Reliable, -1);
        writer.Write(Age.Count);
        foreach (var pc in playerIdList)
        {
            writer.Write(pc);
            writer.Write(Age[pc]);
        }
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void ReceiveRPC(MessageReader reader, CustomRPC rpcType)
    {
        if (rpcType != CustomRPC.MiniAge) return;
        var ages = reader.ReadInt32();
        for (int i = 0; i < ages; i++)
        {
            var pc = reader.ReadByte();
            var age = reader.ReadInt32();
            Age.TryAdd(pc, age);
            Age[pc] = age;
        }
    }
    public static bool IsEnable => playerIdList.Count > 0;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);
    public static void OnSecondsUpdate(PlayerControl player,long now)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (player.Is(CustomRoles.Mini))
        {
            if (!player.IsAlive() && Age[player.PlayerId] < 18 && player.IsCrew())
            {
                
                CustomSoundsManager.RPCPlayCustomSoundAll("Congrats");
                CustomWinnerHolder.ResetAndSetWinner(CustomWinner.Mini); 
                foreach (var pid in playerIdList.Where(p=>Utils.GetPlayerById(p).IsCrew()))
                CustomWinnerHolder.WinnerIds.Add(pid);
            }
            if (!GameStates.IsInTask && OptionNotGrowInMeeting.GetBool()) return;
            if (Age[player.PlayerId] < 18 && player.IsAlive())
            {
                UpTime++;
                if (UpTime >= OptionAgeTime.GetInt() / 18)
                {
                    Age[player.PlayerId] += 1;
                    UpTime = 0;
                    SendRPC();
                    foreach (var pc in Main.AllPlayerControls)
                    {
                        Utils.NotifyRoles(pc);
                    }
                }
            }
        }
    }
    public static bool OnCheckMurderPlayerOthers_After(MurderInfo info)
    {

        var (killer, target) = info.AttemptTuple;
        if ( target.Is(CustomRoles.Mini))
        {
            if (Age[target.PlayerId] < 18)
            {
                killer.Notify(Translator.GetString("CantKillKid"));
                return false;
            }
        }
        return true;
    }
    public static string GetProgressText(byte playerId, bool comms = false)
    {
        if (!playerIdList.Contains(playerId)) return "";
        return Age[playerId] < 18 ? Utils.ColorString(Color.yellow, $"({Age[playerId]})") : "";
    }
    public static void TargetSuffixs(PlayerControl seer, PlayerControl target, ref StringBuilder targetMark)
    {
        if (target.Is(CustomRoles.Mini) && seer != target)
        {
            targetMark.Append($"<color={GetRoleColorCode(CustomRoles.Judge)}>({Age[target.PlayerId]})</color>");
        }
    }
    public static void MeetingHud(bool isLover, PlayerControl seer, PlayerControl target, ref StringBuilder sb)
    {
        if (target.Is(CustomRoles.Mini) && seer != target)
            sb.Append(ColorString(Color.yellow, $"({Age[target.PlayerId]})"));
    }
    public static void Marks(PlayerControl __instance, ref StringBuilder Mark)
    {
        if (__instance.Is(CustomRoles.Mini) && __instance != PlayerControl.LocalPlayer)
        {
            Mark.Append($"<color={Utils.GetRoleColorCode(CustomRoles.Judge)}>({Age[__instance.PlayerId]})</color>");
        }
    
    }
}
