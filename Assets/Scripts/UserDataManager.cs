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

public sealed class UserDataManager : MonoBehaviour {
    [HideInInspector]
    [SerializeField]
    private UserData active;

    private Coroutine autoSaveCo;

    private void OnEnable() {
        autoSaveCo = StartCoroutine(AutoSaveCo());
    }

    private void OnDisable() {
        StopCoroutine(autoSaveCo);
    }

    public Option<UserData> Active => active.ToOption();

    public void ClearActive() {
        active = null;
    }

    public void Load() {
        var path = System.IO.Path.Combine(Application.persistentDataPath, userDataFile);
        var tempPath = System.IO.Path.Combine(Application.persistentDataPath, userDataTempFile);
        try {
            string json;
            try {
                json = System.IO.File.ReadAllText(path);
            } catch {
                json = System.IO.File.ReadAllText(tempPath);
            }

            active = json.AsSerial<UserData>();
        } catch (Exception ex) {
            var userData = new UserData();
            var json = userData.AsJson();
            try {
                using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.CreateNew))
                using (var streamWriter = new System.IO.StreamWriter(fileStream)) {
                    streamWriter.Write(json);
                    active = userData;
                }
            } catch (Exception ex2) {
                Debug.LogError($"Unable to read user data: '{ex}'. Creating new user data file failed: {ex2}");
            }
        }
    }

    public void Save() {
        // Give subscribers a chance to write any unflushed changes to UserData before we check Dirty
        OnSave?.Invoke();

        foreach (var userData in Active) {
            if (userData.Dirty) {
                userData.ClearDirty();

                var json = userData.AsJson();
                var path = System.IO.Path.Combine(Application.persistentDataPath, userDataFile);
                var tempPath = System.IO.Path.Combine(Application.persistentDataPath, userDataTempFile);

                if (System.IO.File.Exists(path)) {
                    if (System.IO.File.Exists(tempPath)) {
                        System.IO.File.Delete(tempPath);
                    }
                    System.IO.File.Move(path, tempPath);
                }
                System.IO.File.WriteAllText(path, json);
                System.IO.File.Delete(tempPath);

                Debug.Log($"Saved user data to {path}");
            }
        }
    }

    public event Action OnSave;

    private System.Collections.IEnumerator AutoSaveCo() {
        while (true) {
            if (Active.TryGetValue(out var userData)) {
                yield return new WaitForSecondsRealtime(userData.Settings.AutoSaveInterval);

                Save();
            } else {
                break;
            }
        }
    }

    private const string userDataFile = "userdata.json";
    private const string userDataTempFile = "userdata-temp.json";
}
