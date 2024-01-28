﻿using HarmonyLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace TONEX;

# pragma warning disable CA1416
public static class RegistryManager
{
    public static RegistryKey SoftwareKeys => Registry.CurrentUser.OpenSubKey("Software", true);
    public static RegistryKey Keys = SoftwareKeys.OpenSubKey("AU-TONEX", true);
    public static Version LastVersion;

    public static void Init()
    {
        if (Keys == null)
        {
            Logger.Info("Create TONEX Registry Key", "Registry Manager");
            Keys = SoftwareKeys.CreateSubKey("AU-TONEX", true);
        }
        if (Keys == null)
        {
            Logger.Error("Create Registry Failed", "Registry Manager");
            return;
        }

        if (Keys.GetValue("Last launched version") is not string regLastVersion)
            LastVersion = new Version(0, 0, 0);
        else LastVersion = Version.Parse(regLastVersion);

        Keys.SetValue("Last launched version", Main.version.ToString());
        Keys.SetValue("Path", Path.GetFullPath("./"));

        List<string> FoldersNFileToDel =
            [
                @"./TOH_DATA",
                @"./TOHE_DATA",
            ];

        Logger.Warn("上次启动的TONEX版本：" + LastVersion, "Registry Manager");

        if (LastVersion < new Version(1, 0, 0))
        {
            Logger.Warn("v1.0 New Version Operation Needed", "Registry Manager");
            FoldersNFileToDel.Add(@"./BepInEx/config");
        }

        FoldersNFileToDel.DoIf(Directory.Exists, p =>
        {
            Logger.Warn("Delete Useless Directory:" + p, "Registry Manager");
            Directory.Delete(p, true);
        });
        FoldersNFileToDel.DoIf(File.Exists, p =>
        {
            Logger.Warn("Delete Useless File:" + p, "Registry Manager");
            File.Delete(p);
        });

    }
}
