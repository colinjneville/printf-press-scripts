using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = LocalizationFormatted;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public static class BuildCampaigns {
    private const string CampaignsFolder = @"Data/Campaigns";
    private const string BaseCampaignFolder = @"Base";

    [MenuItem("Data/Build Campaigns")]
    private static void BuildAllCampaigns() {
        var campaignsPath = Path.Combine(Application.dataPath, CampaignsFolder);
        foreach (var campaignFolder in Directory.EnumerateDirectories(campaignsPath)) {
            var chapters = new List<CampaignChapter>();
            foreach (var chapterFolder in Directory.EnumerateDirectories(campaignFolder)) {
                var levels = new List<CampaignLevel>();
                foreach (var levelFile in Directory.EnumerateFiles(chapterFolder, "*.level.json")) {
                    Debug.Log($"Deserializing level '{levelFile}'");
                    var json = File.ReadAllText(levelFile);
                    var level = json.AsSerial<Level>();
                    var campaignLevel = new CampaignLevel(level, UnlockRequirement.None);
                    levels.Add(campaignLevel);
                }
                var chapterName = string.Concat(new DirectoryInfo(chapterFolder).Name.SkipWhile(c => !char.IsLetter(c)));
                chapters.Add(new CampaignChapter(LC.Temp(chapterName), levels, UnlockRequirement.None));
            }
            var campaignDirName = new DirectoryInfo(campaignFolder).Name;
            var campaignName = string.Concat(campaignDirName.SkipWhile(c => !char.IsLetter(c)));
            // TODO get persistent GUID from file
            var campaign = new Campaign(Guid.NewGuid(), LC.Temp(campaignName), chapters);

            var campaignFile = Path.Combine(CampaignsFolder, $"{campaignDirName}.campaign.json");
            File.WriteAllText(Path.Combine(Application.dataPath, campaignFile), campaign.AsJson());
            AssetDatabase.ImportAsset(Path.Combine("Assets", campaignFile));
            Debug.Log($"Wrote campaign to {Path.Combine(Application.dataPath, campaignFile)}");
        }
    }
    /*
    [MenuItem("Data/Replace Base Campaign")]
    private static void ReplaceBaseCampaign() {
        var baseCampaignFile = Path.Combine(Application.dataPath, CampaignsFolder, BaseCampaignFolder, @"{BaseCampaignFolder}.campaign.json");
        File.Copy(baseCampaignFile, )
    }
    */
}
