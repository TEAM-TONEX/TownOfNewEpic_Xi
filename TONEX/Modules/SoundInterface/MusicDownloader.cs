using HarmonyLib;
using Newtonsoft.Json.Linq;
using Sentry.Unity.NativeUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using static TONEX.Translator;
using TONEX.Modules;

namespace TONEX;

[HarmonyPatch]
public class MusicDownloader
{
    public static string DownloadFileTempPath = "TONEX_Data/Sounds/{{sound}}.wav";

    public static string Url_github = "";
    public static string Url_gitee = ""; 
    public static string Url_website = "";
    public static string Url_website2 = "";
    public static string downloadUrl_github = "";
    public static string downloadUrl_gitee = "";
    public static string downloadUrl_website = "";
    public static string downloadUrl_website2 = "";
    public static bool succeed = false;

    public static async Task StartDownload(string sound, string url = "waitToSelect")
    {
        succeed = false;
        if (!Directory.Exists(@"./TONEX_Data/Sounds"))
        {
            Directory.CreateDirectory(@"./TONEX_Data/Sounds");
        }
        DownloadFileTempPath = "TONEX_Data/Sounds/{{sound}}.wav";
        if (url == "waitToSelect")
        {
            downloadUrl_github = Url_github.Replace("{{sound}}", $"{sound}");
            downloadUrl_gitee = Url_gitee.Replace("{{sound}}", $"{sound}");
            downloadUrl_website = Url_website.Replace("{{sound}}", $"{sound}");
            downloadUrl_website2 = Url_website2.Replace("{{sound}}", $"{sound}");
            url = IsChineseLanguageUser ? downloadUrl_website : downloadUrl_github;
        }

        if (!IsValidUrl(url))
        {
            Logger.Error($"Invalid URL: {url}", "DownloadSound", false);
            return;
        }
        retry:
        DownloadFileTempPath = DownloadFileTempPath.Replace("{{sound}}", $"{sound}");
        string filePath = DownloadFileTempPath + ".xwmus";
        File.Create(filePath).Close();
       

        Logger.Msg("Start Downloading from: " + url, "DownloadSound");
        Logger.Msg("Saving file to: " + DownloadFileTempPath, "DownloadSound");

        try
        {
           
            
            using var client = new HttpClientDownloadWithProgress(url, filePath);
            client.ProgressChanged += OnDownloadProgressChanged;
            await client.StartDownload();
            Thread.Sleep(100);
            
            if (md5ForFiles.ContainsKey(sound))
            {
                if (GetMD5HashFromFile(filePath).ToLower() != md5ForFiles[sound].ToLower())
                {
                    Logger.Error($"Md5 Wrong in {url}", "DownloadSound");
                    File.Delete(filePath);
                    if (url == downloadUrl_website && IsChineseLanguageUser || url == downloadUrl_github && !IsChineseLanguageUser)
                    {

                        url = downloadUrl_gitee;
                        goto retry;
                    }
                    else if (url == downloadUrl_gitee && IsChineseLanguageUser)
                    {
                        url = downloadUrl_github;
                        goto retry;
                    }
                    return;
                }
                File.Move(filePath, DownloadFileTempPath);
                Logger.Info($"Md5 Currect in {url}", "DownloadSound");
            }
            else
            {
                Logger.Error($"Md5 No Found in {url}", "DownloadSound");
                File.Delete(filePath);
                if (url == downloadUrl_website && IsChineseLanguageUser || url == downloadUrl_github && !IsChineseLanguageUser)
                {

                    url = downloadUrl_gitee;
                    goto retry;
                }
                else if (url == downloadUrl_gitee && IsChineseLanguageUser)
                {
                    url = downloadUrl_github;
                    goto retry;
                }
                return;
            }
            
            var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./TONEX_Data/Sounds/{sound}.wav";
            if (File.Exists(path))
            {
                AudioManager.StartLoadAC(path, sound);
                Logger.Info($"Succeed in {url}", "DownloadSound");
                succeed = true;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to download\n{ex.Message}", "DownloadSound", false);
            if (!string.IsNullOrEmpty(filePath))
            {
                File.Delete(filePath);
            }
        }
        if (!succeed)
        {
            if (url == downloadUrl_website && IsChineseLanguageUser || url == downloadUrl_github && !IsChineseLanguageUser)
            {
                
                url = downloadUrl_gitee;
                goto retry;
            }
            else if (url == downloadUrl_gitee && IsChineseLanguageUser)
            {
                url = downloadUrl_github;
                goto retry;
            }

        }
    }
    private static bool IsValidUrl(string url)
    {
        string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
        return Regex.IsMatch(url, pattern);
    }
    private static void OnDownloadProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
    {
        string msg = $"\n{totalFileSize / 1000}KB / {totalBytesDownloaded / 1000}KB  -  {(int)progressPercentage}%";
        Logger.Info(msg, "DownloadSounds");
    }
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(fileName);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "GetMD5HashFromFile");
            return "";
        }
    }
    private static Dictionary<string, string> md5ForFiles = new()
    {
        {"Birthday","e288b805c09d35a4545401539b00d3d1" },
        {"AWP","A191F48B6689290ECD4568149C22A381" },
        {"Bet","8B9B734E97998BE8872ADAE6B5D4343C"},
        {"Bite","9AEFF327DE582FF926EF2187AE4DC338"},
        {"Boom","4DF61F364E7EE087A983172021CEE00C"},
        {"Clothe","394F4EC5823A7F8AD4ECEA6897D2928C"},
        {"Congrats","65F53C4C1731B112CF5C38E6E1C74988"},
        {"Curse","3548B2872E3630789FB664BE5137E3D3"},
        {"Dove","C4FE25AF79505A866C8ECAB38761809F"},
        {"Eat","4BBF93B90712722AC0DC3DD976634E78"},
        {"ElementMaxi1","D99694C79BF36615939ED02FF05F339C"},
        {"ElementMaxi2","F64D5C34ADE6637258DBC289BB47B59A"},
        {"ElementMaxi3","D698A12A1801328739EE0B87777243AF"},
        {"ElementSkill1","45204B00A499C52ACE852BFAE913076C"},
        {"ElementSkill2","34892FB0B82C1A5A827AC955ED3147BF"},
        {"ElementSkill3","BDF00B0AC80B4E6510619F2C9A5E2062"},
        {"FlashBang","E4C9912E139F1DFFDFD95F0081472EBA"},
        {"GongXiFaCai","7DED159AD441CA72DB98A442B037A3D7"},
        
        {"Gunfire","4A44EAF6B45B96B63BBC12A946DB517B"},
        {"Gunload","27441FBFC8CC5A5F2945A8CE344A52B9"},
        {"Join1","0DBC4FEDCD5C8D10A57EBB8E5C31189D"},
        {"Join2","646B104360FD8DC2E20339291FC25BDE"},
        {"Join3","E613F02735A761E720367AAED8F93AF9"},
        {"Line","4DA0B66BD3E2C8D2D5984CB15F518378"},
        {"MarioCoin","2698FB768F1E1045C1231B63C2639766"},
        {"MarioJump","A485BCFEE7311EF3A7651F4B20E381CB"},
        {"Onichian","13B71F389E21C2AF8E35996201843642"},
        
        {"Shapeshifter","B7119CC4E0E5B108B8735D734769AA5C"},
        {"Shield","9EA3B450C5B53A4B952CB8418DF84539"},
        
        
        {"Teleport","8D3DA143C59CD7C4060129C46BEB7A39"},
        {"TheWorld","395010A373FAE0EC704BB4FE8FC5A57A"},
        //“Ù¿÷
        {"GongXiFaCaiLiuDeHua","DB200D93E613020D62645F4841DD55BD"},
        {"RejoiceThisSEASONRespectThisWORLD","7AB4778744242E4CFA0468568308EA9B"},
        {"SpringRejoicesinParallelUniverses","D92528104A82DBBFADB4FF251929BA5E"},
{"AFamiliarPromise", "a3672341f586b4d81efba6d4278cfeae"},
{"GuardianandDream", "cd8fb04bad5755937496eed60c4892f3"},
{"HeartGuidedbyLight", "f1ded08a59936b8e1db95067a69b006e"},
{"HopeStillExists", "8d5ba9ac283e156ab2c930f7b63a4a36"},
{"Mendax", "1054c90edfa66e31655bc7a58f553231"},
{"MendaxsTimeForExperiment", "1b82e1ea81aeb9a968a94bec7f4f62fd"},
{"StarfallIntoDarkness", "46f09e0384eb8a087c3ba8cc22e4ac11"},
{"StarsFallWithDomeCrumbles", "b5ccabeaf3324cedb107c83a2dc0ce1e"},
{"TheDomeofTruth", "183804914e3310b9f92b47392f503a9f"},
{"TheTruthFadesAway", "75fbed53db391ed73085074ad0709d82"},
{"unavoidable", "da520f4613103826b4df7647e368d4b4"},
        {"NeverGonnaGiveYouUp","354cab3103b7e033c6e31d12766eb59c" }


    };
}