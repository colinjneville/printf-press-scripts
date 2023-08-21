using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

[Serializable]
public sealed class UnlockRequirement {
    private Guid[] levels;
    private int stars;

    public UnlockRequirement(IEnumerable<Guid> levels, int stars = 0) {
        this.levels = levels.ToArray();
        this.stars = stars;
    }

    public IReadOnlyCollection<Guid> Levels => levels;
    public int Stars => stars;

    private static UnlockRequirement none = new UnlockRequirement(Array.Empty<Guid>());
    public static UnlockRequirement None => none;
}

[Serializable]
public sealed class CampaignLevel {
    private Level level;
    private UnlockRequirement unlockRequirement;

    public CampaignLevel(Level level, UnlockRequirement unlockRequirement) {
        this.level = level;
        this.unlockRequirement = unlockRequirement;
    }

    public Level Level => level;
    public UnlockRequirement UnlockRequirement => unlockRequirement;

    public CampaignChapter Chapter => CampaignChapter.GetFromLevelId(level.Id);

    [System.Runtime.Serialization.OnDeserialized()]
    private void AddToCache(System.Runtime.Serialization.StreamingContext context) {
        cache.Add(level.Id, this);
    }

    private static Dictionary<Guid, CampaignLevel> cache = new Dictionary<Guid, CampaignLevel>();

    public static Option<CampaignLevel> Get(Guid levelId) => cache.GetOrNone(levelId);
}

[Serializable]
public sealed class CampaignChapter {
    private LE name;
    private CampaignLevel[] levels;
    private UnlockRequirement unlockRequirement;

    public CampaignChapter(LE name, IEnumerable<CampaignLevel> levels, UnlockRequirement unlockRequirement) {
        this.name = name;
        this.levels = levels.ToArray();
        this.unlockRequirement = unlockRequirement;
    }

    public LE Name => name;
    public IReadOnlyCollection<CampaignLevel> Levels => levels;
    public UnlockRequirement UnlockRequirement => unlockRequirement;

    public Campaign Campaign => Campaign.GetCampaignFromChapter(this);

    public Option<CampaignLevel> GetLevel(Guid levelId) {
        foreach (var level in levels) {
            if (level.Level.Id == levelId) {
                return level;
            }
        }
        return Option.None;
    }

    [System.Runtime.Serialization.OnDeserialized()]
    private void AddToCache(System.Runtime.Serialization.StreamingContext context) {
        foreach (var level in levels) {
            levelIdChapterCache.Add(level.Level.Id, this);
        }
    }

    private static Dictionary<Guid, CampaignChapter> levelIdChapterCache = new Dictionary<Guid, CampaignChapter>();

    public static CampaignChapter GetFromLevelId(Guid levelId) => levelIdChapterCache[levelId];

}

[Serializable]
public sealed class Campaign : IDeserializeTo {
    private Guid id;
    private LE name;
    private CampaignChapter[] chapters;

    public Campaign(Guid id, LE name, IEnumerable<CampaignChapter> chapters) {
        this.id = id;
        this.name = name;
        this.chapters = chapters.ToArray();
    }

    public Guid Id => id;
    public LE Name => name;
    public IReadOnlyList<CampaignChapter> Chapters => chapters;

    public Option<CampaignLevel> GetLevel(Guid levelId) {
        foreach (var chapter in Chapters) {
            foreach (var level in chapter.GetLevel(levelId)) {
                return level;
            }
        }
        return Option.None;
    }

    [System.Runtime.Serialization.OnDeserialized()]
    private void AddToCache(System.Runtime.Serialization.StreamingContext context) {
        foreach (var chapter in chapters) {
            chapterCache.Add(chapter, this);
        }
    }

    private static Dictionary<Guid, Campaign> registry = new Dictionary<Guid, Campaign>();

    public static void RegisterCampaign(Campaign campaign) {
        if (!registry.ContainsKey(campaign.Id)) {
            registry.Add(campaign.Id, campaign);
        }
    }

    public static Option<Campaign> Get(Guid id) => registry.GetOrNone(id);

    public static IReadOnlyCollection<Campaign> All => registry.Values;

    private static Dictionary<CampaignChapter, Campaign> chapterCache = new Dictionary<CampaignChapter, Campaign>();

    public static Campaign GetCampaignFromChapter(CampaignChapter chapter) => chapterCache[chapter];
}
