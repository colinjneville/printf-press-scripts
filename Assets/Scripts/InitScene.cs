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

public sealed class InitScene : MonoBehaviour {
    private void Start() {
        RegisterDefaultCampaign();
        RegisterCustomCampaigns();
        Overseer.UserDataManager.Load();

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GlobalAssets.MenuScene);
    }

    private void RegisterDefaultCampaign() {
        var json = Overseer.GlobalAssets.DefaultCampaignJson;
        var campaign = json.text.AsSerial<Campaign>();
        Campaign.RegisterCampaign(campaign);
    }

    private void RegisterCustomCampaigns() {
        // TODO
    }
}
