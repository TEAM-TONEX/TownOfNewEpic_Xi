using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TONEX.Roles.Core;
using TONEX.Roles.Impostor;
using UnityEngine;

namespace TONEX;

public enum SpawnPoint
{
    Cafeteria,
    Weapons,
    LifeSupp,
    Nav,
    Shields,
    Comms,
    Storage,
    Admin,
    Electrical,
    LowerEngine,
    UpperEngine,
    Security,
    Reactor,
    MedBay,
    Balcony,
    Junction,//StringNamesにない文言 string.csvに追加
    LockerRoom,
    Decontamination,
    Laboratory,
    Launchpad,
    Office,
    OfficeLeft,//StringNamesにない文言 string.csvに追加
    OfficeRight,//StringNamesにない文言 string.csvに追加
    Greenhouse,
    BoilerRoom,
    Dropship,
    Rocket,//StringNamesにない文言 string.csvに追加
    Toilet,//StringNamesにない文言 string.csvに追加
    Specimens,
    Brig,
    Engine,
    Kitchen,
    CargoBay,
    Records,
    MainHall,
    NapRoom,//StringNamesにない文言 string.csvに追加 AirShipメインホール左上の仮眠室
    MeetingRoom,
    GapRoom,
    VaultRoom,
    Cockpit,
    Armory,
    ViewingDeck,
    Medical,
    Showers,
    Beach,
    RecRoom,//SplashZoneのこと
    Bonfire,//StringNamesにない文言 string.csvに追加 Fungleの焚き火
    SleepingQuarters,//TheDorm 宿舎のこと
    JungleTop,//StringNamesにない文言 string.csvに追加
    JungleBottom,//StringNamesにない文言 string.csvに追加
    Lookout,
    MiningPit,
    Highlands,//Fungleの高地
    Precipice,//StringNamesにない文言 string.csvに追加
}
class RandomSpawn
{
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.SnapTo), typeof(Vector2), typeof(ushort))]
    public class CustomNetworkTransformPatch
    {
        public static Dictionary<byte, bool> FirstTP = new();
        public static void Postfix(CustomNetworkTransform __instance, Vector2 position, ushort minSid)
        {
            var player = Main.AllPlayerControls.Where(p => p.NetTransform == __instance).FirstOrDefault();
            if (player == null)
            {
                Logger.Warn("プレイヤーがnullです", "RandomSpawn");
                return;
            }
            //Logger.Info($"{player.name} pos:{position} minSid={minSid}", "SnapTo");
            if (!AmongUsClient.Instance.AmHost) return;

            if (GameStates.IsInTask)
            {
                if (player.Is(CustomRoles.GM)) return; //GMは対象外に

                if (Main.NormalOptions.MapId != 4) return;//AirShip以外無効

                if (position == new Vector2(-25f, 40f))
                {
                    //最初の湧き地点なら次回スポーン
                    FirstTP[player.PlayerId] = true;
                    return;
                }

                if (FirstTP[player.PlayerId])
                {
                    FirstTP[player.PlayerId] = false;
                    //ランダムスポーンをvanillaの初期スポーンより後の判定とする
                    __instance.lastSequenceId++;
                    AirshipSpawn(player);
                    __instance.lastSequenceId--;
                }
            }
        }
    }
    public static void AirshipSpawn(PlayerControl player)
    {
        if (player.Is(CustomRoles.Penguin))
        {
            var penguin = player.GetRoleClass() as Penguin;
            penguin?.OnSpawnAirship();
        }
        player.RpcResetAbilityCooldown();
        if (Options.FixFirstKillCooldown.GetBool() && !MeetingStates.MeetingCalled) player.SetKillCooldown(Main.AllPlayerKillCooldown[player.PlayerId]);
        if (!IsRandomSpawn()) return; //ランダムスポーンが無効ならreturn
        new AirshipSpawnMap().RandomTeleport(player);
    }
    public static bool IsRandomSpawn()
    {
        if (!Options.EnableRandomSpawn.GetBool()) return false;
        switch (Main.NormalOptions.MapId)
        {
            case 0:
                return Options.RandomSpawnSkeld.GetBool();
            case 1:
                return Options.RandomSpawnMira.GetBool();
            case 2:
                return Options.RandomSpawnPolus.GetBool();
            case 4:
                return Options.RandomSpawnAirship.GetBool();
            case 5:
                return Options.RandomSpawnFungle.GetBool();
            default:
                Logger.Error($"MapIdFailed ID:{Main.NormalOptions.MapId}", "IsRandomSpawn");
                return false;
        }
    }
    public static void TP(CustomNetworkTransform nt, Vector2 location)
    {
       // nt.(location);
    }

    public static void SetupCustomOption(int startId)
    {
        // Skeld
        Options.RandomSpawnSkeld = BooleanOptionItem.Create(startId + 1, StringNames.MapNameSkeld, false, TabGroup.GameSettings, false).SetParent(Options.EnableRandomSpawn).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldCafeteria = BooleanOptionItem.Create(startId + 2, StringNames.Cafeteria, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldWeapons = BooleanOptionItem.Create(startId + 3, StringNames.Weapons, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldShields = BooleanOptionItem.Create(startId + 4, StringNames.Shields, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldStorage = BooleanOptionItem.Create(startId + 5, StringNames.Storage, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldLowerEngine = BooleanOptionItem.Create(startId + 6, StringNames.LowerEngine, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldUpperEngine = BooleanOptionItem.Create(startId + 7, StringNames.UpperEngine, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldLifeSupp = BooleanOptionItem.Create(startId + 8, StringNames.LifeSupp, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldNav = BooleanOptionItem.Create(startId + 9, StringNames.Nav, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldComms = BooleanOptionItem.Create(startId + 10, StringNames.Comms, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldAdmin = BooleanOptionItem.Create(startId + 11, StringNames.Admin, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldElectrical = BooleanOptionItem.Create(startId + 12, StringNames.Electrical, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldSecurity = BooleanOptionItem.Create(startId + 13, StringNames.Security, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldReactor = BooleanOptionItem.Create(startId + 14, StringNames.Reactor, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnSkeldMedBay = BooleanOptionItem.Create(startId + 15, StringNames.MedBay, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnSkeld).SetGameMode(CustomGameMode.All);
        // Mira
        startId += 100;
        Options.RandomSpawnMira = BooleanOptionItem.Create(startId + 1, StringNames.MapNameMira, false, TabGroup.GameSettings, false).SetParent(Options.EnableRandomSpawn).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraCafeteria = BooleanOptionItem.Create(startId + 2, StringNames.Cafeteria, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraComms = BooleanOptionItem.Create(startId + 3, StringNames.Comms, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraDecontamination = BooleanOptionItem.Create(startId + 4, StringNames.Decontamination, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraReactor = BooleanOptionItem.Create(startId + 5, StringNames.Reactor, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraLaunchpad = BooleanOptionItem.Create(startId + 6, StringNames.Launchpad, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraAdmin = BooleanOptionItem.Create(startId + 7, StringNames.Admin, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraBalcony = BooleanOptionItem.Create(startId + 8, StringNames.Balcony, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraStorage = BooleanOptionItem.Create(startId + 9, StringNames.Storage, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraJunction = BooleanOptionItem.Create(startId + 10, SpawnPoint.Junction, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraMedBay = BooleanOptionItem.Create(startId + 11, StringNames.MedBay, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraLockerRoom = BooleanOptionItem.Create(startId + 12, StringNames.LockerRoom, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraLaboratory = BooleanOptionItem.Create(startId + 13, StringNames.Laboratory, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraOffice = BooleanOptionItem.Create(startId + 14, StringNames.Office, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnMiraGreenhouse = BooleanOptionItem.Create(startId + 15, StringNames.Greenhouse, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnMira).SetGameMode(CustomGameMode.All);
        // Polus
        startId += 100;
        Options.RandomSpawnPolus = BooleanOptionItem.Create(startId + 1, StringNames.MapNamePolus, false, TabGroup.GameSettings, false).SetParent(Options.EnableRandomSpawn).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusOfficeLeft = BooleanOptionItem.Create(startId + 2, SpawnPoint.OfficeLeft, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusBoilerRoom = BooleanOptionItem.Create(startId + 3, StringNames.BoilerRoom, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusSecurity = BooleanOptionItem.Create(startId + 4, StringNames.Security, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusDropship = BooleanOptionItem.Create(startId + 5, StringNames.Dropship, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusLaboratory = BooleanOptionItem.Create(startId + 6, StringNames.Laboratory, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusSpecimens = BooleanOptionItem.Create(startId + 7, StringNames.Specimens, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusOfficeRight = BooleanOptionItem.Create(startId + 8, SpawnPoint.OfficeRight, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusAdmin = BooleanOptionItem.Create(startId + 9, StringNames.Admin, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusComms = BooleanOptionItem.Create(startId + 10, StringNames.Comms, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusWeapons = BooleanOptionItem.Create(startId + 11, StringNames.Weapons, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusLifeSupp = BooleanOptionItem.Create(startId + 12, StringNames.LifeSupp, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusElectrical = BooleanOptionItem.Create(startId + 13, StringNames.Electrical, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusStorage = BooleanOptionItem.Create(startId + 14, StringNames.Storage, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusRocket = BooleanOptionItem.Create(startId + 15, SpawnPoint.Rocket, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnPolusToilet = BooleanOptionItem.Create(startId + 16, SpawnPoint.Toilet, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnPolus).SetGameMode(CustomGameMode.All);
        // Airship
        startId += 100;
        Options.RandomSpawnAirship = BooleanOptionItem.Create(startId + 1, StringNames.MapNameAirship, false, TabGroup.GameSettings, false).SetParent(Options.EnableRandomSpawn).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipBrig = BooleanOptionItem.Create(startId + 2, StringNames.Brig, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipEngine = BooleanOptionItem.Create(startId + 3, StringNames.Engine, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipKitchen = BooleanOptionItem.Create(startId + 4, StringNames.Kitchen, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipCargoBay = BooleanOptionItem.Create(startId + 5, StringNames.CargoBay, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipRecords = BooleanOptionItem.Create(startId + 6, StringNames.Records, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipMainHall = BooleanOptionItem.Create(startId + 7, StringNames.MainHall, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipNapRoom = BooleanOptionItem.Create(startId + 8, SpawnPoint.NapRoom, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipMeetingRoom = BooleanOptionItem.Create(startId + 9, StringNames.MeetingRoom, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipGapRoom = BooleanOptionItem.Create(startId + 10, StringNames.GapRoom, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipVaultRoom = BooleanOptionItem.Create(startId + 11, StringNames.VaultRoom, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipComms = BooleanOptionItem.Create(startId + 12, StringNames.Comms, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipCockpit = BooleanOptionItem.Create(startId + 13, StringNames.Cockpit, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipArmory = BooleanOptionItem.Create(startId + 14, StringNames.Armory, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipViewingDeck = BooleanOptionItem.Create(startId + 15, StringNames.ViewingDeck, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipSecurity = BooleanOptionItem.Create(startId + 16, StringNames.Security, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipElectrical = BooleanOptionItem.Create(startId + 17, StringNames.Electrical, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipMedical = BooleanOptionItem.Create(startId + 18, StringNames.Medical, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipToilet = BooleanOptionItem.Create(startId + 19, SpawnPoint.Toilet, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnAirshipShowers = BooleanOptionItem.Create(startId + 20, StringNames.Showers, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnAirship).SetGameMode(CustomGameMode.All);
        // Fungle
        startId += 100;
        Options.RandomSpawnFungle = BooleanOptionItem.Create(startId + 1, StringNames.MapNameFungle, false, TabGroup.GameSettings, false).SetParent(Options.EnableRandomSpawn).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleKitchen = BooleanOptionItem.Create(startId + 2, StringNames.Kitchen, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleBeach = BooleanOptionItem.Create(startId + 3, StringNames.Beach, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleBonfire = BooleanOptionItem.Create(startId + 4, SpawnPoint.Bonfire, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleGreenhouse = BooleanOptionItem.Create(startId + 5, StringNames.Greenhouse, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleComms = BooleanOptionItem.Create(startId + 6, StringNames.Comms, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleHighlands = BooleanOptionItem.Create(startId + 7, StringNames.Highlands, true, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleCafeteria = BooleanOptionItem.Create(startId + 8, StringNames.Cafeteria, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleRecRoom = BooleanOptionItem.Create(startId + 9, StringNames.RecRoom, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleDropship = BooleanOptionItem.Create(startId + 10, StringNames.Dropship, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleStorage = BooleanOptionItem.Create(startId + 11, StringNames.Storage, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleMeetingRoom = BooleanOptionItem.Create(startId + 12, StringNames.MeetingRoom, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleSleepingQuarters = BooleanOptionItem.Create(startId + 13, StringNames.SleepingQuarters, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleLaboratory = BooleanOptionItem.Create(startId + 14, StringNames.Laboratory, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleReactor = BooleanOptionItem.Create(startId + 15, StringNames.Reactor, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleJungleTop = BooleanOptionItem.Create(startId + 16, SpawnPoint.JungleTop, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleJungleBottom = BooleanOptionItem.Create(startId + 17, SpawnPoint.JungleBottom, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleLookout = BooleanOptionItem.Create(startId + 18, StringNames.Lookout, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleMiningPit = BooleanOptionItem.Create(startId + 19, StringNames.MiningPit, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFungleUpperEngine = BooleanOptionItem.Create(startId + 20, StringNames.UpperEngine, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
        Options.RandomSpawnFunglePrecipice = BooleanOptionItem.Create(startId + 21, SpawnPoint.Precipice, false, TabGroup.GameSettings, false).SetParent(Options.RandomSpawnFungle).SetGameMode(CustomGameMode.All);
    }

    public abstract class SpawnMap
    {
        public abstract Dictionary<OptionItem, Vector2> Positions { get; }
        public virtual void RandomTeleport(PlayerControl player)
        {
            var location = GetLocation();
            Logger.Info($"{player.Data.PlayerName}:{location}", "RandomSpawn");
            player.RpcTeleport( location);
        }
        public Vector2 GetLocation()
        {
            var locations =
                Positions.ToArray().Where(o => o.Key.GetBool()).Any()
                ? Positions.ToArray().Where(o => o.Key.GetBool())
                : Positions.ToArray();
            var location = locations.OrderBy(_ => Guid.NewGuid()).Take(1).FirstOrDefault();
            return location.Value;
        }
    }

    public class SkeldSpawnMap : SpawnMap
    {
        public override Dictionary<OptionItem, Vector2> Positions { get; } = new()
        {
            [Options.RandomSpawnSkeldCafeteria] = new(-1.0f, 3.0f),
            [Options.RandomSpawnSkeldWeapons] = new(9.3f, 1.0f),
            [Options.RandomSpawnSkeldLifeSupp] = new(6.5f, -3.8f),
            [Options.RandomSpawnSkeldNav] = new(16.5f, -4.8f),
            [Options.RandomSpawnSkeldShields] = new(9.3f, -12.3f),
            [Options.RandomSpawnSkeldComms] = new(4.0f, -15.5f),
            [Options.RandomSpawnSkeldStorage] = new(-1.5f, -15.5f),
            [Options.RandomSpawnSkeldAdmin] = new(4.5f, -7.9f),
            [Options.RandomSpawnSkeldElectrical] = new(-7.5f, -8.8f),
            [Options.RandomSpawnSkeldLowerEngine] = new(-17.0f, -13.5f),
            [Options.RandomSpawnSkeldUpperEngine] = new(-17.0f, -1.3f),
            [Options.RandomSpawnSkeldSecurity] = new(-13.5f, -5.5f),
            [Options.RandomSpawnSkeldReactor] = new(-20.5f, -5.5f),
            [Options.RandomSpawnSkeldMedBay] = new(-9.0f, -4.0f)
        };
    }
    public class MiraHQSpawnMap : SpawnMap
    {
        public override Dictionary<OptionItem, Vector2> Positions { get; } = new()
        {
            [Options.RandomSpawnMiraCafeteria] = new(25.5f, 2.0f),
            [Options.RandomSpawnMiraBalcony] = new(24.0f, -2.0f),
            [Options.RandomSpawnMiraStorage] = new(19.5f, 4.0f),
            [Options.RandomSpawnMiraJunction] = new(17.8f, 11.5f),
            [Options.RandomSpawnMiraComms] = new(15.3f, 3.8f),
            [Options.RandomSpawnMiraMedBay] = new(15.5f, -0.5f),
            [Options.RandomSpawnMiraLockerRoom] = new(9.0f, 1.0f),
            [Options.RandomSpawnMiraDecontamination] = new(6.1f, 6.0f),
            [Options.RandomSpawnMiraLaboratory] = new(9.5f, 12.0f),
            [Options.RandomSpawnMiraReactor] = new(2.5f, 10.5f),
            [Options.RandomSpawnMiraLaunchpad] = new(-4.5f, 2.0f),
            [Options.RandomSpawnMiraAdmin] = new(21.0f, 17.5f),
            [Options.RandomSpawnMiraOffice] = new(15.0f, 19.0f),
            [Options.RandomSpawnMiraGreenhouse] = new(17.8f, 23.0f)
        };
    }
    public class PolusSpawnMap : SpawnMap
    {
        public override Dictionary<OptionItem, Vector2> Positions { get; } = new()
        {

            [Options.RandomSpawnPolusOfficeLeft] = new(19.5f, -18.0f),
            [Options.RandomSpawnPolusOfficeRight] = new(26.0f, -17.0f),
            [Options.RandomSpawnPolusAdmin] = new(24.0f, -22.5f),
            [Options.RandomSpawnPolusComms] = new(12.5f, -16.0f),
            [Options.RandomSpawnPolusWeapons] = new(12.0f, -23.5f),
            [Options.RandomSpawnPolusBoilerRoom] = new(2.3f, -24.0f),
            [Options.RandomSpawnPolusLifeSupp] = new(2.0f, -17.5f),
            [Options.RandomSpawnPolusElectrical] = new(9.5f, -12.5f),
            [Options.RandomSpawnPolusSecurity] = new(3.0f, -12.0f),
            [Options.RandomSpawnPolusDropship] = new(16.7f, -3.0f),
            [Options.RandomSpawnPolusStorage] = new(20.5f, -12.0f),
            [Options.RandomSpawnPolusRocket] = new(26.7f, -8.5f),
            [Options.RandomSpawnPolusLaboratory] = new(36.5f, -7.5f),
            [Options.RandomSpawnPolusToilet] = new(34.0f, -10.0f),
            [Options.RandomSpawnPolusSpecimens] = new(36.5f, -22.0f)
        };
    }
    public class AirshipSpawnMap : SpawnMap
    {
        public override Dictionary<OptionItem, Vector2> Positions { get; } = new()
        {
            [Options.RandomSpawnAirshipBrig] = new(-0.7f, 8.5f),
            [Options.RandomSpawnAirshipEngine] = new(-0.7f, -1.0f),
            [Options.RandomSpawnAirshipKitchen] = new(-7.0f, -11.5f),
            [Options.RandomSpawnAirshipCargoBay] = new(33.5f, -1.5f),
            [Options.RandomSpawnAirshipRecords] = new(20.0f, 10.5f),
            [Options.RandomSpawnAirshipMainHall] = new(15.5f, 0.0f),
            [Options.RandomSpawnAirshipNapRoom] = new(6.3f, 2.5f),
            [Options.RandomSpawnAirshipMeetingRoom] = new(17.1f, 14.9f),
            [Options.RandomSpawnAirshipGapRoom] = new(12.0f, 8.5f),
            [Options.RandomSpawnAirshipVaultRoom] = new(-8.9f, 12.2f),
            [Options.RandomSpawnAirshipComms] = new(-13.3f, 1.3f),
            [Options.RandomSpawnAirshipCockpit] = new(-23.5f, -1.6f),
            [Options.RandomSpawnAirshipArmory] = new(-10.3f, -5.9f),
            [Options.RandomSpawnAirshipViewingDeck] = new(-13.7f, -12.6f),
            [Options.RandomSpawnAirshipSecurity] = new(5.8f, -10.8f),
            [Options.RandomSpawnAirshipElectrical] = new(16.3f, -8.8f),
            [Options.RandomSpawnAirshipMedical] = new(29.0f, -6.2f),
            [Options.RandomSpawnAirshipToilet] = new(30.9f, 6.8f),
            [Options.RandomSpawnAirshipShowers] = new(21.2f, -0.8f)
        };
    }
    public class FungleSpawnMap : SpawnMap
    {
        public override Dictionary<OptionItem, Vector2> Positions { get; } = new()
        {
            [Options.RandomSpawnFungleKitchen] = new(-17.8f, -7.3f),
            [Options.RandomSpawnFungleBeach] = new(-21.3f, 3.0f),   //海岸
            [Options.RandomSpawnFungleCafeteria] = new(-16.9f, 5.5f),
            [Options.RandomSpawnFungleRecRoom] = new(-17.7f, 0.0f),
            [Options.RandomSpawnFungleBonfire] = new(-9.7f, 2.7f),  //焚き火
            [Options.RandomSpawnFungleDropship] = new(-7.6f, 10.4f),
            [Options.RandomSpawnFungleStorage] = new(2.3f, 4.3f),
            [Options.RandomSpawnFungleMeetingRoom] = new(-4.2f, -2.2f),
            [Options.RandomSpawnFungleSleepingQuarters] = new(1.7f, -1.4f),  //宿舎
            [Options.RandomSpawnFungleLaboratory] = new(-4.2f, -7.9f),
            [Options.RandomSpawnFungleGreenhouse] = new(9.2f, -11.8f),
            [Options.RandomSpawnFungleReactor] = new(21.8f, -7.2f),
            [Options.RandomSpawnFungleJungleTop] = new(4.2f, -5.3f),
            [Options.RandomSpawnFungleJungleBottom] = new(15.9f, -14.8f),
            [Options.RandomSpawnFungleLookout] = new(6.4f, 3.1f),
            [Options.RandomSpawnFungleMiningPit] = new(12.5f, 9.6f),
            [Options.RandomSpawnFungleHighlands] = new(15.5f, 3.9f),    //展望台右の高地
            [Options.RandomSpawnFungleUpperEngine] = new(21.9f, 3.2f),
            [Options.RandomSpawnFunglePrecipice] = new(19.8f, 7.3f),   //通信室下の崖
            [Options.RandomSpawnFungleComms] = new(20.9f, 13.4f),
        };
    }
}