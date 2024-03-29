﻿using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DEModLauncher_GUI;

internal static class DOOMEternal
{
    public static string DefaultGameDirectory => @"C:\Program Files (x86)\Steam\steamapps\common\DOOMEternal";

    public static string DefaultModPackImage => @"\DEModLauncher_GUI;component\Resources\Images\header3.jpg";

    public static string GameMainExecutor { get; set; } = "DOOMEternalx64vk.exe";

    public static string ModLoader { get; set; } = "EternalModInjector.bat";

    public static string ModLoaderProfileFile { get; } = "EternalModInjector Settings.txt";

    public static string GameDirectory { get; set; } = "";

    public static bool ModificationSaved { get; set; } = true;

    public static string ModDirectory => $@"{GameDirectory}\Mods";

    public static string ModPacksDirectory => $@"{GameDirectory}\ModPacks";

    public static string ModPackImagesDirectory => $@"{GameDirectory}\ModPacks\Images";

    public static string LauncherProfileFile
    {
        get
        {
            if (string.IsNullOrEmpty(GameDirectory))
            {
                return $@"Mods\ModPacks\DEModProfiles.json";
            }
            return $@"{GameDirectory}\Mods\ModPacks\DEModProfiles.json";
        }
    }

    public static void LaunchGame()
    {
        var p = new Process();
        p.StartInfo.FileName = $@"{GameDirectory}\{GameMainExecutor}";
        if (!File.Exists(p.StartInfo.FileName))
        {
            throw new FileNotFoundException($"无法找到游戏主程序{GameMainExecutor}");
        }
        p.Start();
    }

    public static void LaunchModLoader()
    {
        var p = new Process();
        p.StartInfo.FileName = $@"{GameDirectory}\{ModLoader}";
        p.Start();
        p.WaitForExit();
    }

    public static void OpenGameDirectory()
    {
        var p = new Process();
        p.StartInfo.FileName = "explorer.exe";
        p.StartInfo.Arguments = $@"/e, {GameDirectory}";
        p.Start();
    }

    public static void OpenLauncherProfile()
    {
        var p = new Process();
        p.StartInfo.FileName = "explorer.exe";
        p.StartInfo.Arguments = $@"/select, {LauncherProfileFile}";
        p.Start();
    }

    public static void InitNecessaryDirectory()
    {
        if (!Directory.Exists(ModDirectory))
        {
            Directory.CreateDirectory(ModDirectory);
        }
        if (!Directory.Exists(ModPacksDirectory))
        {
            Directory.CreateDirectory(ModPacksDirectory);
        }
        if (!Directory.Exists(ModPackImagesDirectory))
        {
            Directory.CreateDirectory(ModPackImagesDirectory);
        }
    }

    public static void SetModLoaderProfile(string option, object value)
    {
        if (!File.Exists(ModLoaderProfileFile))
        {
            return;
        }
        var reg = new Regex(@$"(?<=:{option}=)[0-9\.]+");
        var text = new StringBuilder();
        using (var reader = new StreamReader(ModLoaderProfileFile))
        {
            while (!reader.EndOfStream)
            {
                string currentLine = reader.ReadLine() ?? "";
                if (reg.IsMatch(currentLine))
                {
                    currentLine = reg.Replace(currentLine, value.ToString() ?? "");
                }
                text.AppendLine(currentLine);
            }
        }
        using (var writer = new StreamWriter(ModLoaderProfileFile))
        {
            writer.Write(text);
        }
    }
}
