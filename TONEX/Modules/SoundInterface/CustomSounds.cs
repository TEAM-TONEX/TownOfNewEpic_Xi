using AmongUs.HTTP;
using Hazel;
using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Media;
using static TONEX.AudioManager;
using System.Linq;
using static Il2CppSystem.Xml.XmlWellFormedWriter.AttributeValueCache;
//using Il2CppSystem.IO;
using UnityEngine;


namespace TONEX.Modules.SoundInterface;

public static class CustomSoundsManager
{
    public static void RPCPlayCustomSound(this PlayerControl pc, string sound, int playmode = 0, bool force = false)
    {
        if (pc == null || pc.AmOwner)
        {
            Play(sound, playmode);
            return;
        }
        if (!force) if (!AmongUsClient.Instance.AmHost || !pc.IsModClient()) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlayCustomSound, SendOption.Reliable, pc.GetClientId());
        writer.Write(sound);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void RPCPlayCustomSoundAll(string sound)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlayCustomSound, SendOption.Reliable, -1);
        writer.Write(sound);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        Play(sound, 0);
    }
    public static void ReceiveRPC(MessageReader reader) => Play(reader.ReadString(), 0);


    public static readonly string SOUNDS_PATH = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./TONEX_Data/Sounds/";
    public static readonly string PLAY_PATH = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}/TONEX_Data/Sounds/";
    public static void Play(string sound, int playmode = 0, bool forcePlay = false)
    {
        StopPlay();
        if (!Constants.ShouldPlaySfx() || !Main.EnableCustomSoundEffect.Value && !forcePlay) return;
        var path = SOUNDS_PATH + sound + ".wav";

        if (!Directory.Exists(SOUNDS_PATH)) Directory.CreateDirectory(SOUNDS_PATH);
        DirectoryInfo folder = new(SOUNDS_PATH);
        if ((folder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            folder.Attributes = FileAttributes.Hidden;
        //getExtension(path);
        if (File.Exists(path))
        {
            path = path.Replace(SOUNDS_PATH, PLAY_PATH);
            Logger.Warn($"{path} Finded", "CustomSoundsManager.Play");
        }
        switch (playmode)
        {
            case 0:
                StartPlayOnce(path);
                break;
            case 1:
                StartPlayInAmongUs(path,sound);
                break;
            case 2:
            case 3:
                AutoPlay(path, sound);
                break;

        }

        Logger.Msg($"播放声音：{sound}", "CustomSounds");
    }

    [DllImport("winmm.dll")]
    public static extern bool PlaySound(string Filename, int Mod, int Flags);
    public static void AutoPlay(string sound, string soundname)
    {
        Play(sound);
        MusicNow = soundname;
        MusicPlaybackCompletedHandler();
    }

    public static string MusicNow = "";
    // 播放音乐

    // 播放完成事件处理程序
    private static void MusicPlaybackCompletedHandler()
    {
        var rd = IRandom.Instance;
        List<string> mus = new();
        foreach (var music in AllMusics.Keys)
        {

            mus.Add(music);


        }
        if (SoundPanel.PlayMode == 2)
        {
            for (int i = 0; i < 10; i++)
            {
                var select = mus[rd.Next(0, mus.Count)];
                var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./TONEX_Data/Sounds/{select}.wav";
                if (File.Exists(path))
                    StartPlayWait(path);
                else
                    i--;
            }

        }
        else if (SoundPanel.PlayMode == 3)
        {
            var musicn = mus.IndexOf(MusicNow);
            for (int i = 0; i < 10; i++)
            {
                int index = musicn;
                if (index > mus.Count - 2)
                    index = -1;
                var select = mus[index + 1];
                var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./TONEX_Data/Sounds/{select}.wav";
                if (File.Exists(path))
                {
                    StartPlayWait(path);
                    musicn++;

                }
                else
                    i--;
            }

        }
        new LateTask(() =>
        {
            MusicPlaybackCompletedHandler();
        }, 40f, "AddMusic");
    }
    public static void StartPlayOnce(string path) => PlaySound(@$"{path}", 0, 1); //第3个形参，换为9，连续播放
    public static void StartPlayLoop(string path) => PlaySound(@$"{path}", 0, 9);
    public static void StartPlayWait(string path) => PlaySound(@$"{path}", 0, 17);
    public static bool isinternal = false;
    public static void StopPlay()
    {
        isinternal = true;
        PlaySound(null, 0, 0);
        SoundManager.Instance.StopAllSound();
        isinternal = false;
    }
    public static void StartPlayInAmongUs(string path="", string mus ="")
    {
        AllSoundClips.TryGetValue(mus, out var audioClip);
        if (audioClip != null)
        {
            SoundManager.Instance.StopAllSound();
            SoundManager.Instance.PlaySound(audioClip, true, 1, null);
        }
        else
        {
            var task = LoadAudioClipAsync(path);
            task.ContinueWith(t =>
            {
                if (t.Result != null)
                    AllSoundClips.TryAdd(mus, t.Result);
                SoundManager.Instance.StopAllSound();
                SoundManager.Instance.PlaySound(t.Result, true, 1, null);
            });

            Debug.LogWarning($"Failed to load AudioClip from path: {path}");
        }
    }



}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
public class SoundManagerPlaySoundPatch
{
    public static void Prefix(SoundManager __instance, [HarmonyArgument(0)] AudioClip clip, [HarmonyArgument(1)] bool loop)
    {
        if ((AllSoundClips?.Values?.Any(SoundManager.Instance.SoundIsPlaying) ?? false) 
            && !AllSoundClips.ContainsValue(clip) && loop) 
            return;
    }
    
}
[HarmonyPatch(typeof(SoundManager), nameof(SoundManager.StopAllSound))]
public class SoundManagerStopAllSoundPatch
{
    public static void Prefix(SoundManager __instance)
    {
        if ((AllSoundClips?.Values?.Any(SoundManager.Instance.SoundIsPlaying) ?? false) && !CustomSoundsManager.isinternal)
            return;
    }
}
