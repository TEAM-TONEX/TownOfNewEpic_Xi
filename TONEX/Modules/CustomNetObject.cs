﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using InnerNet;
using TMPro;

using TONEX;
using UnityEngine;


// Credit: https://github.com/Rabek009/MoreGamemodes/blob/e054eb498094dfca0a365fc6b6fea8d17f9974d7/Modules/CustomObjects, https://github.com/Gurge44/EndlessHostRoles/blob/main/Modules/CustomNetObject.cs
// Huge thanks to Rabek009 for this code! And thanks to Gurge for some modifications





// 8x8 + Animator : https://ultradragon005.github.io/AmongUs-Utilities/animator.html
// 10x10 : https://ultradragon005.github.io/AmongUs-Utilities/10xeditor.html
// For special grid such as "8x6" jsut copy 10x10 code and ask gpt to make u that specific grid.

//Sidenote: 8x8 on 100% size is a pretty golden standard and trying to make something smaller than that is very ugly (as the grean bean is very visible) so I wouldn't recommend it. 

namespace TONEX
{
    internal class CustomNetObject
    {
        public static readonly List<CustomNetObject> AllObjects = [];
        private static int MaxId = -1;
        protected int Id;
        protected byte OwnerId;
        public PlayerControl playerControl;
        private float PlayerControlTimer;
        public Vector2 Position;
        public HashSet<byte> HiddenList = [];

        private string Sprite;

        protected void RpcChangeSprite(string sprite)
        {
            Sprite = sprite;
            _ = new LateTask(() =>
            {
                playerControl.RawSetName(sprite);
                var name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                var colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
                var hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
                var skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
                var petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
                var visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
                CustomRpcSender sender = CustomRpcSender.Create("SetFakeData");
                MessageWriter writer = sender.stream;
                sender.StartMessage();
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14><br></size>" + sprite;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = 255;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                    PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                    .WriteNetObject(PlayerControl.LocalPlayer)
                    .Write(false)
                    .EndRpc();
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                    PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.EndMessage();
                sender.SendMessage();
            }, 0f);
        }

        public void TP(Vector2 position)
        {
            playerControl.NetTransform.RpcSnapTo(position);
            Position = position;
        }

        public void Despawn()
        {
            Logger.Info($" Despawn Custom Net Object {this.GetType().Name} (ID {Id})", "CNO.Despawn");
            playerControl.Despawn();
            AllObjects.Remove(this);
        }

        protected void Hide(PlayerControl player)
        {
            Logger.Info($" Hide Custom Net Object {this.GetType().Name} (ID {Id}) from {player.GetNameWithRole()}", "CNO.Hide");

            HiddenList.Add(player.PlayerId);
            if (player.AmOwner)
            {
                _ = new LateTask(() =>
                {
                    playerControl.transform.FindChild("Names").FindChild("NameText_TMP").gameObject.SetActive(false);
                }, 0.1f);
                playerControl.Visible = false;
                return;
            }

            _ = new LateTask(() =>
            {
                CustomRpcSender sender = CustomRpcSender.Create("FixModdedClientCNOText", sendOption: SendOption.Reliable);
                sender.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FixModdedClientCNO, player.GetClientId())
                    .WriteNetObject(playerControl)
                    .Write(false)
                    .EndRpc();
                sender.SendMessage();
            }, 0.4f);

            MessageWriter writer = MessageWriter.Get();
            writer.StartMessage(6);
            writer.Write(AmongUsClient.Instance.GameId);
            writer.WritePacked(player.GetClientId());
            writer.StartMessage(5);
            writer.WritePacked(playerControl.NetId);
            writer.EndMessage();
            writer.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(writer);
            writer.Recycle();


        }

        protected virtual void OnFixedUpdate()
        {
            //
            // Need to respawn player every 20s because of 30s timeout
            // 
            PlayerControlTimer += Time.fixedDeltaTime;
            if (PlayerControlTimer > 20f)
            {
                Logger.Info($" Recreate Custom Net Object {this.GetType().Name} (ID {Id})", "CNO.OnFixedUpdate");
                PlayerControl oldPlayerControl = playerControl;
                playerControl = GameObject.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
                playerControl.PlayerId = 255;
                playerControl.isNew = false;
                playerControl.notRealPlayer = true;
                AmongUsClient.Instance.NetIdCnt += 1U;
                MessageWriter msg = MessageWriter.Get();
                msg.StartMessage(5);
                msg.Write(AmongUsClient.Instance.GameId);
                AmongUsClient.Instance.WriteSpawnMessage(playerControl, -2, SpawnFlags.None, msg);
                msg.EndMessage();
                msg.StartMessage(6);
                msg.Write(AmongUsClient.Instance.GameId);
                msg.WritePacked(int.MaxValue);
                for (uint i = 1; i <= 3; ++i)
                {
                    msg.StartMessage(4);
                    msg.WritePacked(2U);
                    msg.WritePacked(-2);
                    msg.Write((byte)SpawnFlags.None);
                    msg.WritePacked(1);
                    msg.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
                    msg.StartMessage(1);
                    msg.EndMessage();
                    msg.EndMessage();
                }

                msg.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(msg);
                msg.Recycle();
                if (PlayerControl.AllPlayerControls.Contains(playerControl))
                    PlayerControl.AllPlayerControls.Remove(playerControl);
                _ = new LateTask(() =>
                {
                    playerControl.NetTransform.RpcSnapTo(Position);
                    playerControl.RawSetName(Sprite);
                    var name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                    var colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
                    var hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
                    var skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
                    var petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
                    var visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
                    CustomRpcSender sender = CustomRpcSender.Create("SetFakeData");
                    MessageWriter writer = sender.stream;
                    sender.StartMessage();
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14><br></size>" + Sprite;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = 255;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = "";
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = "";
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                        PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                    }
                    writer.EndMessage();
                    sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                        .WriteNetObject(PlayerControl.LocalPlayer)
                        .Write(false)
                        .EndRpc();
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
                    PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                        PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                    }
                    writer.EndMessage();
                    sender.EndMessage();
                    sender.SendMessage();
                }, 0.2f);
                _ = new LateTask(() => oldPlayerControl.Despawn(), 0.3f);
                foreach (var pc in Main.AllPlayerControls)
                {
                    if (pc.AmOwner) continue;
                    _ = new LateTask(() =>
                    {
                        CustomRpcSender sender = CustomRpcSender.Create("SetFakeData");
                        MessageWriter writer = sender.stream;
                        sender.StartMessage(pc.GetClientId());
                        writer.StartMessage(1);
                        {
                            writer.WritePacked(playerControl.NetId);
                            writer.Write(pc.PlayerId);
                        }
                        writer.EndMessage();
                        sender.StartRpc(playerControl.NetId, (byte)RpcCalls.MurderPlayer)
                            .WriteNetObject(playerControl)
                            .Write((int)MurderResultFlags.FailedError)
                            .EndRpc();
                        writer.StartMessage(1);
                        {
                            writer.WritePacked(playerControl.NetId);
                            writer.Write((byte)255);
                        }
                        writer.EndMessage();
                        sender.EndMessage();
                        sender.SendMessage();
                    }, 0.1f);
                }

                foreach (var pc in Main.AllPlayerControls.Where(x => HiddenList.Contains(x.PlayerId)))
                {
                    Hide(pc);
                }
                _ = new LateTask(() =>
                { // Fix for host
                    if (!HiddenList.Contains(PlayerControl.LocalPlayer.PlayerId))
                        playerControl.transform.FindChild("Names").FindChild("NameText_TMP").gameObject.SetActive(true);
                }, 0.1f);
                _ = new LateTask(() =>
                { // Fix for Modded
                    foreach (var visiblePC in Main.AllPlayerControls.ExceptBy(HiddenList, x => x.PlayerId))
                    {
                        CustomRpcSender sender = CustomRpcSender.Create("FixModdedClientCNOText", sendOption: SendOption.Reliable);
                        sender.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FixModdedClientCNO, visiblePC.GetClientId())
                            .WriteNetObject(playerControl)
                            .Write(true)
                            .EndRpc();
                        sender.SendMessage();
                    }
                }, 0.4f);
                PlayerControlTimer = 0f;
            }
        }

        public void CreateNetObject(string sprite, Vector2 position)
        {
            Logger.Info($" Create Custom Net Object {this.GetType().Name} (ID {Id}) at {position}", "CNO.CreateNetObject");
            playerControl = GameObject.Instantiate(AmongUsClient.Instance.PlayerPrefab, Vector2.zero, Quaternion.identity);
            playerControl.PlayerId = 255;
            playerControl.isNew = false;
            playerControl.notRealPlayer = true;
            AmongUsClient.Instance.NetIdCnt += 1U;
            MessageWriter msg = MessageWriter.Get();
            msg.StartMessage(5);
            msg.Write(AmongUsClient.Instance.GameId);
            AmongUsClient.Instance.WriteSpawnMessage(playerControl, -2, SpawnFlags.None, msg);
            msg.EndMessage();
            msg.StartMessage(6);
            msg.Write(AmongUsClient.Instance.GameId);
            msg.WritePacked(int.MaxValue);
            for (uint i = 1; i <= 3; ++i)
            {
                msg.StartMessage(4);
                msg.WritePacked(2U);
                msg.WritePacked(-2);
                msg.Write((byte)SpawnFlags.None);
                msg.WritePacked(1);
                msg.WritePacked(AmongUsClient.Instance.NetIdCnt - i);
                msg.StartMessage(1);
                msg.EndMessage();
                msg.EndMessage();
            }

            msg.EndMessage();
            AmongUsClient.Instance.SendOrDisconnect(msg);
            msg.Recycle();
            if (PlayerControl.AllPlayerControls.Contains(playerControl))
                PlayerControl.AllPlayerControls.Remove(playerControl);
            _ = new LateTask(() =>
            {
                playerControl.NetTransform.RpcSnapTo(position);
                playerControl.RawSetName(sprite);
                var name = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName;
                var colorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId;
                var hatId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId;
                var skinId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId;
                var petId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId;
                var visorId = PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId;
                CustomRpcSender sender = CustomRpcSender.Create("SetFakeData");
                MessageWriter writer = sender.stream;
                sender.StartMessage();
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = "<size=14><br></size>" + sprite;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = 255;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = "";
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = "";
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                    PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.StartRpc(playerControl.NetId, (byte)RpcCalls.Shapeshift)
                    .WriteNetObject(PlayerControl.LocalPlayer)
                    .Write(false)
                    .EndRpc();
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PlayerName = name;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].ColorId = colorId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].HatId = hatId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].SkinId = skinId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].PetId = petId;
                PlayerControl.LocalPlayer.Data.Outfits[PlayerOutfitType.Default].VisorId = visorId;
                writer.StartMessage(1);
                {
                    writer.WritePacked(PlayerControl.LocalPlayer.Data.NetId);
                    PlayerControl.LocalPlayer.Data.Serialize(writer, false);
                }
                writer.EndMessage();
                sender.EndMessage();
                sender.SendMessage();
            }, 0.2f);
            Position = position;
            PlayerControlTimer = 0f;
            Sprite = sprite;
            ++MaxId;
            Id = MaxId;
            if (MaxId == int.MaxValue) MaxId = int.MinValue;
            AllObjects.Add(this);
            foreach (var pc in Main.AllPlayerControls)
            {
                if (pc.AmOwner) continue;
                _ = new LateTask(() =>
                {
                    CustomRpcSender sender = CustomRpcSender.Create("SetFakeData");
                    MessageWriter writer = sender.stream;
                    sender.StartMessage(pc.GetClientId());
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(playerControl.NetId);
                        writer.Write(pc.PlayerId);
                    }
                    writer.EndMessage();
                    sender.StartRpc(playerControl.NetId, (byte)RpcCalls.MurderPlayer)
                        .WriteNetObject(playerControl)
                        .Write((int)MurderResultFlags.FailedError)
                        .EndRpc();
                    writer.StartMessage(1);
                    {
                        writer.WritePacked(playerControl.NetId);
                        writer.Write((byte)255);
                    }
                    writer.EndMessage();
                    sender.EndMessage();
                    sender.SendMessage();
                }, 0.1f);
            }
            _ = new LateTask(() =>
            { // Fix for host
                playerControl.transform.FindChild("Names").FindChild("NameText_TMP").gameObject.SetActive(true);
            }, 0.1f);
            _ = new LateTask(() =>
            { // Fix for Modded
                CustomRpcSender sender = CustomRpcSender.Create("FixModdedClientCNOText", sendOption: SendOption.Reliable);
                sender.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.FixModdedClientCNO)
                    .WriteNetObject(playerControl)
                    .Write(true)
                    .EndRpc();
                sender.SendMessage();
            }, 0.4f);
        }
        public static void FixedUpdate() => AllObjects.ToArray().Do(x => x.OnFixedUpdate());
        public static CustomNetObject Get(int id) => AllObjects.FirstOrDefault(x => x.Id == id);
        public static void DespawnOnQuit(byte Playerid) => AllObjects.Where(x => x.OwnerId == Playerid).ToArray().Do(x => x.Despawn());

        public static void Reset()
        {
            try
            {
                AllObjects.ToArray().Do(x => x.Despawn());
                AllObjects.Clear();
            }
            catch (Exception e)
            {
                Utils.ThrowException(e);
            }
        }


    }
    internal sealed class Explosion : CustomNetObject
    {
        private readonly float Duration;

        private readonly float Size;
        private int Frame;
        private float Timer;

        public Explosion(float size, float duration, Vector2 position)
        {
            Size = size;
            Duration = duration;
            Timer = -0.1f;
            Frame = 0;
            CreateNetObject($"<size={Size}><line-height=72%><font=\"VCR SDF\"><br><#0000>███<#ff0000>█<#0000>███<br><#ff0000>█<#0000>█<#ff0000>███<#0000>█<#ff0000>█<br>█<#ff8000>██<#ffff00>█<#ff8000>██<#ffff00>█<br>██<#ff8000>█<#ffff00>█<#ff8000>█<#ffff00>██<br><#ff8000>█<#ffff80>██<#ffff00>█<#ffff80>██<#ff8000>█<br><#0000>█<#ff8000>█<#ffff80>███<#ff8000>█<#0000>█<br>██<#ff8000>███<#0000>██", position);
        }

        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            Timer += Time.deltaTime;
            if (Timer >= Duration / 5f && Frame == 0)
            {
                RpcChangeSprite($"<size={Size}><line-height=72%><font=\"VCR SDF\"><br><#0000>█<#ff0000>█<#0000>█<#ff0000>█<#0000>█<#ff0000>█<#0000>█<br><#ff0000>█<#ff8000>█<#ff0000>█<#ff8000>█<#ff0000>█<#ff8000>█<#ff0000>█<br><#ff8000>██<#ffff00>█<#ff8000>█<#ffff00>█<#ff8000>██<br><#ffff00>███████<br><#ff8000>█<#ffff00>█████<#ff8000>█<br>██<#ffff00>█<#ff8000>█<#ffff00>█<#ff8000>██<br><#ff0000>█<#0000>█<#ff8000>█<#ff0000>█<#ff8000>█<#0000>█<#ff0000>█");
                Frame = 1;
            }

            if (Timer >= Duration / 5f * 2f && Frame == 1)
            {
                RpcChangeSprite($"<size={Size}><line-height=72%><font=\"VCR SDF\"><br><#0000>█<#c0c0c0>█<#ff0000>█<#000000>█<#ff0000>█<#c0c0c0>█<#0000>█<br><#c0c0c0>█<#808080>█<#ff0000>█<#ff8000>█<#ff0000>█<#c0c0c0>██<br><#ff0000>██<#ff8000>█<#ffff00>█<#ff8000>█<#ff0000>██<br><#c0c0c0>█<#ff8000>█<#ffff00>█<#ffff80>█<#ffff00>█<#ff8000>█<#808080>█<br><#ff0000>██<#ff8000>█<#ffff00>█<#ff8000>█<#ff0000>██<br><#c0c0c0>█<#808080>█<#ff0000>█<#ff8000>█<#ff0000>█<#000000>█<#c0c0c0>█<br><#0000>█<#c0c0c0>█<#ff0000>█<#c0c0c0>█<#ff0000>█<#c0c0c0>█<#0000>█");
                Frame = 2;
            }

            if (Timer >= Duration / 5f * 3f && Frame == 2)
            {
                RpcChangeSprite($"<size={Size}><line-height=72%><font=\"VCR SDF\"><br><#ff0000>█<#ff8000>█<#0000>█<#808080>█<#0000>█<#ff8000>█<#ff0000>█<br><#ff8000>█<#0000>█<#ffff00>█<#c0c0c0>█<#ffff00>█<#0000>█<#ff8000>█<br><#0000>█<#ffff00>█<#c0c0c0>███<#ffff00>█<#0000>█<br><#808080>█<#c0c0c0>█████<#808080>█<br><#0000>█<#ffff00>█<#c0c0c0>███<#ffff00>█<#0000>█<br><#ff8000>█<#0000>█<#ffff00>█<#c0c0c0>█<#ffff00>█<#0000>█<#ff8000>█<br><#ff0000>█<#ff8000>█<#0000>█<#808080>█<#0000>█<#ff8000>█<#ff0000>█");
                Frame = 3;
            }

            if (Timer >= Duration / 5f * 4f && Frame == 3)
            {
                RpcChangeSprite($"<size={Size}><line-height=72%><font=\"VCR SDF\"><br><#0000>█<#808080>█<#0000>██<#c0c0c0>█<#0000>█<#808080>█<br><#ffff00>█<#0000>██<#c0c0c0>█<#0000>█<#808080>█<#0000>█<br>█<#808080>█<#c0c0c0>████<#0000>█<br>█<#c0c0c0>██████<br>█<#0000>█<#c0c0c0>███<#808080>█<#0000>█<br>█<#c0c0c0>█<#0000>█<#c0c0c0>█<#0000>█<#c0c0c0>██<br><#808080>█<#0000>█<#c0c0c0>█<#0000>█<#808080>█<#0000>█<#ffff00>█");
                Frame = 4;
            }

            if (Timer >= Duration && Frame == 4)
            {
                Despawn();
            }
        }
    }
    //internal sealed class BlackHole : CustomNetObject
    //{
    //    internal BlackHole(Vector2 position, byte OwnerId)
    //    {
    //        if (!AmongUsClient.Instance.AmHost) return; // Spawning gets ignored for abyssbringer RPC, because it already does an rpc as Host
    //        CreateNetObject("<size=100%><font=\"VCR SDF\"><line-height=67%><alpha=#00>█<alpha=#00>█<#000000>█<#19131c>█<#000000>█<#000000>█<alpha=#00>█<alpha=#00>█<br><alpha=#00>█<#412847>█<#000000>█<#19131c>█<#000000>█<#412847>█<#260f26>█<alpha=#00>█<br><#000000>█<#412847>█<#412847>█<#000000>█<#260f26>█<#1c0d1c>█<#19131c>█<#000000>█<br><#19131c>█<#000000>█<#412847>█<#1c0d1c>█<#1c0d1c>█<#000000>█<#19131c>█<#000000>█<br><#000000>█<#000000>█<#260f26>█<#1c0d1c>█<#1c0d1c>█<#000000>█<#000000>█<#260f26>█<br><#000000>█<#260f26>█<#1c0d1c>█<#1c0d1c>█<#19131c>█<#412847>█<#412847>█<#19131c>█<br><alpha=#00>█<#260f26>█<#412847>█<#412847>█<#19131c>█<#260f26>█<#19131c>█<alpha=#00>█<br><alpha=#00>█<alpha=#00>█<#412847>█<#260f26>█<#260f26>█<#000000>█<alpha=#00>█<alpha=#00>█<br></line-height></size>", position);
    //        this.OwnerId = OwnerId;
    //    }
    //}
    //internal sealed class Fireworks : CustomNetObject
    //{
    //    internal Fireworks(Vector2 position, byte OwnerId)
    //    {
    //        if (!AmongUsClient.Instance.AmHost) return; // Spawning gets ignored for abyssbringer RPC, because it already does an rpc as Host
    //        CreateNetObject("<size=100%>□□□□□</size>", position);
    //        this.OwnerId = OwnerId;
    //    }
    //}
}