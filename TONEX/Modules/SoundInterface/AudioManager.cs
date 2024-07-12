using AmongUs.Data;
using HarmonyLib;
using Hazel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TONEX.Attributes;
using TONEX.Modules.SoundInterface;
using UnityEngine;
using static TONEX.Translator;

namespace TONEX;

#nullable enable
public static class AudioManager
{
    public static readonly string TAGS_DIRECTORY_PATH = @"./TONEX_Data/SoundNames/";

    public static IReadOnlyDictionary<string, bool> TONEXMusic => TONEXOfficialMusic;

    public static IReadOnlyDictionary<string, bool> AllMusics => CustomMusic.Concat(TONEXOfficialMusic)
        .ToDictionary(x => x.Key.ToString(), x => x.Value, StringComparer.OrdinalIgnoreCase);
    public static IReadOnlyDictionary<string, bool> AllSounds => TONEXSounds;

    public static IReadOnlyDictionary<string, bool> AllFiles => AllSounds.Concat(AllMusics)
        .ToDictionary(x => x.Key.ToString(), x => x.Value, StringComparer.OrdinalIgnoreCase);
    public static IReadOnlyDictionary<string, bool> AllTONEX => AllSounds.Concat(TONEXMusic)
        .ToDictionary(x => x.Key.ToString(), x => x.Value, StringComparer.OrdinalIgnoreCase);


    private static Dictionary<string, bool> CustomMusic = new();
    public static Dictionary<string, bool> TONEXOfficialMusic = new();
    public static Dictionary<string, bool> TONEXSounds = new();


    public static List<string> TONEXOfficialMusicList = new()
    {
        "GongXiFaCaiLiuDeHua",
        "NeverGonnaGiveYouUp",
        "RejoiceThisSEASONRespectThisWORLD",
        "SpringRejoicesinParallelUniverses",
        "AFamiliarPromise",
        "GuardianandDream",
        "HeartGuidedbyLight",
        "HopeStillExists",
        "Mendax",
        "MendaxsTimeForExperiment",
        "StarfallIntoDarkness",
        "StarsFallWithDomeCrumbles",
        "TheDomeofTruth",
        "TheTruthFadesAway",
        "unavoidable",



    };

    public static List<string> NotUp = new()
    {


    };

    public static List<string> TONEXSoundList = new()
    {
        "Birthday",
        "AWP",
        "Bet",
        "Bite",
        "Boom",
        "Clothe",
        "Congrats",
        "Curse",
        "Dove",
        "Eat",
        "ElementSkill1",
        "ElementSkill2",
        "ElementSkill3",
        "ElementMaxi1",
        "ElementMaxi2",
        "ElementMaxi3",
        "FlashBang",
        "GongXiFaCai",
        "Gunfire",
        "Gunload",
        "Join1",
        "Join2",
        "Join3",
        "Line",
        "MarioCoin",
        "MarioJump",
        "Onichian",
        "Shapeshifter",
        "Shield",
        "Teleport",
        "TheWorld",
    };
    public static void ReloadTag(string? sound)
    {
        if (sound == null)
        {
            Init();
            return;
        }

        CustomMusic.Remove(sound);

        string path = $"{TAGS_DIRECTORY_PATH}{sound}.json";
        if (File.Exists(path))
        {
            try { ReadTagsFromFile(path); }
            catch (Exception ex)
            {
                Logger.Error($"Load Sounds From: {path} Failed\n" + ex.ToString(), "AudioManager", false);
            }
        }
    }

    public static void Init()
    {
        CustomMusic = new();
        TONEXOfficialMusic = new();
        foreach (var file in TONEXOfficialMusicList)
        {
            TONEXOfficialMusic.TryAdd(file, false);
        }
        TONEXSounds = new();
        foreach (var file in TONEXSoundList)
        {
            TONEXSounds.TryAdd(file, false);
        }
        
        if (!Directory.Exists(TAGS_DIRECTORY_PATH)) Directory.CreateDirectory(TAGS_DIRECTORY_PATH);
        if (!Directory.Exists(CustomSoundsManager.SOUNDS_PATH)) Directory.CreateDirectory(CustomSoundsManager.SOUNDS_PATH);

        var files = Directory.EnumerateFiles(TAGS_DIRECTORY_PATH, "*.json", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            try { ReadTagsFromFile(file); }
            catch (Exception ex)
            {
                Logger.Error($"Load Tag From: {file} Failed\n" + ex.ToString(), "AudioManager", false);
            }
        }

        Logger.Msg($"{CustomMusic.Count} Sounds Loaded", "AudioManager");
    }
    public static void ReadTagsFromFile(string path)
    {
        if (path.ToLower().Contains("template")) return;
        string sound = Path.GetFileNameWithoutExtension(path);
        if (sound != null && !AllSounds.ContainsKey(sound) && !TONEXMusic.ContainsKey(sound))
        {
            CustomMusic.TryAdd(sound,false);
            Logger.Info($"Sound Loaded: {sound}", "AudioManager");
        }
    }
    public static void getExtension(string path)
    {
        int i = 0;
        if (path == null) return;
        while (!File.Exists(path))
        {
            i++;
            string matchingKey = extensionMap.Keys.FirstOrDefault(key => path.Contains(key)) ?? "";
            if (matchingKey != "")
            {
                string newFormat = extensionMap[matchingKey];
                path = path.Replace(matchingKey, newFormat);
                Logger.Warn($"{path} Founded", "AudioManager");
                break;
            }
            if (i == extensionMap.Count)
            {
                Logger.Error($"{path} Cannot Be Found", "AudioManager");
                break;
            }
        }
        if (i != extensionMap.Count)
            Logger.Warn($"{path} Founded", "AudioManager");

    }

    public static Dictionary<string, string> extensionMap = new()
    {
    { ".wav", ".flac" },
    { ".flac", ".aiff" },
    { ".aiff", ".mp3" },
    { ".mp3", ".aac" },
    { ".aac", ".ogg" },
    { ".ogg", ".m4a" }
};
    public static Dictionary<string, AudioClip> AllSoundClips = new();
    [PluginModuleInitializer]
    public static void InitForAudioClip()
    {
        foreach (var mus in AllMusics.Keys)
        {
            var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./TONEX_Data/Sounds/{mus}.wav";
            if (!File.Exists(path)) continue;
            StartLoadAC(path, mus);
        }
    }
    public static void StartLoadAC(string path, string mus)
    {
        var task = LoadAudioClipAsync(path);
        task.ContinueWith(t =>
        {
            if (t.Result != null)
                AllSoundClips.TryAdd(mus, t.Result);
        });
    }
    public static async Task<AudioClip> LoadAudioClipAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File does not exist: " + filePath);
            return null;
        }

        byte[] audioData;

        try
        {
            // 异步读取音频文件的字节数据
            audioData = await ReadAllBytesAsync(filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to read file: " + filePath + "\n" + e.Message);
            return null;
        }

        // 创建一个新的AudioClip
        AudioClip audioClip = AudioClip.Create("LoadedAudioClip", audioData.Length / 2, 2, 44100, false);
        float[] floatData = ConvertBytesToFloats(audioData);

        // 将字节流数据加载到AudioClip中
        audioClip.SetData(floatData, 0);

        return audioClip;
    }

    // 异步读取文件字节数据
    private static async Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        using (FileStream sourceStream = new FileStream(filePath,
            FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true))
        {
            MemoryStream memoryStream = new MemoryStream();
            await sourceStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }

    // 将音频字节数据转换为浮点数数组
    private static float[] ConvertBytesToFloats(byte[] audioBytes)
    {
        float[] floatData = new float[audioBytes.Length / 2]; // 2 bytes to 1 float

        for (int i = 0; i < floatData.Length; i++)
        {
            floatData[i] = (float)BitConverter.ToInt16(audioBytes, i * 2) / 32768.0f; // Convert to float (-1.0 to 1.0)
        }

        return floatData;
    }
}
#nullable disable