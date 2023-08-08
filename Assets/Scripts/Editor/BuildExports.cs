using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

using LE = ILocalizationExpression;
using LD = LocalizationDefault;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public static class ExportsEditor {
    [MenuItem("Exports/Build en-us")]
    //[DidReloadScripts]
    private static void BuildEnUs() {
        if (!EditorApplication.isCompiling) {
            var dictionary = new Dictionary<string, string>();
            var ls = typeof(LD).GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.FieldType == typeof(LD));

            foreach (var field in ls) {
                var l = (LD)field.GetValue(null);
                dictionary.Add(field.Name, l.DefaultValue);
            }

            var vocab = new Vocabulary(dictionary);

            var language = new Language("English (US)", "en-us", 1, null, vocab);

            var path = Path.Combine(Utility.InstallLanguagesPath.FullName, string.Format(@"{0}.json", language.Code));

            var json = JsonConvert.SerializeObject(language, typeof(Language), SerializationUtility.Settings);

            File.WriteAllText(path, json);
            Debug.Log($"Wrote en-us to '{path}'");
        }
    }

    [PostProcessBuild(0)]
    private static void BundleLanguages(BuildTarget target, string pathToBuiltProject) {
        string buildDirectory = Directory.GetParent(pathToBuiltProject).FullName;
        Copy(Utility.InstallLanguagesPath.FullName, Path.Combine(buildDirectory, Utility.languagesDirectoryName));
    }

    [PostProcessBuild(0)]
    private static void BundleExports(BuildTarget target, string pathToBuiltProject) {
        string buildDirectory = Directory.GetParent(pathToBuiltProject).FullName;
        Copy(Utility.InstallResourcesPath, Path.Combine(buildDirectory, Utility.resourcesDirectoryName));
    }

    // From https://stackoverflow.com/a/690980
    private static void Copy(string sourceDirectory, string targetDirectory) {
        Debug.Log(string.Format("Copying '{0}' to '{1}'", sourceDirectory, targetDirectory));

        var diSource = new DirectoryInfo(sourceDirectory);
        var diTarget = new DirectoryInfo(targetDirectory);

        CopyAll(diSource, diTarget);
    }

    public static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
        Directory.CreateDirectory(target.FullName);

        // Copy each file into the new directory.
        foreach (FileInfo fi in source.GetFiles()) {
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }
}
