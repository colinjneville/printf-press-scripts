using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using LK = LocalizationKeycode;

[JsonObject(MemberSerialization.Fields)]
public sealed class Language {

    #region    Serialized Data

    private string name;

    private string code;

    private int version;

    [JsonProperty(PropertyName = "extends-code", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
    private string extendsCode;

    private Vocabulary vocabulary;

    #endregion Serialized Data

    public Language(string name, string code, int version, string extendsCode, Vocabulary vocabulary) {
        this.name = name;
        this.code = code;
        this.version = version;
        this.extendsCode = extendsCode;
        this.vocabulary = vocabulary;
    }

    [JsonIgnore]
    private Language extends;

    public string Name => name;

    public string Code => code;

    public int Version => version;

    public string ExtendsCode => extendsCode;

    public Language Extends {
        get {
            if (extendsCode != null && extends == null) {
                extends = Load(extendsCode);
            }
            return extends;
        }
    }

    public Vocabulary Vocabulary => vocabulary;

    public string this[string index] {
        get {
            string result = Vocabulary[index];
            if (result == null) {
                if (Extends != null) {
                    result = Extends[index];
                }

                if (result == null) {
                    Debug.LogWarning($"Missing translation for key '{index}'");
                    result = "[MISSING TRANSLATION]";
                }
            }
            return result;
        }
    }

    private const string languageFileExtension = "json";

    private static Uri GetLanguagePath(string code) {
        string path = System.IO.Path.Combine(Utility.InstallLanguagesPath.FullName, string.Format("{0}.{1}", code, languageFileExtension));
        return new Uri(path);
    }

    public static Language Load(string code) {
        string fileContents;
        try {
            fileContents = System.IO.File.ReadAllText(GetLanguagePath(code).LocalPath);
        } catch (Exception e) {
            Debug.Log(e);
            return null;
        }

        var language = JsonConvert.DeserializeObject<Language>(fileContents, SerializationUtility.Settings);
        return language;
    }

    private static Language en_us = Load("en-us");

    public static Language Default => en_us;

    public static Language Current {
        get {
            // TODO
            return en_us;
        }
    }

    public class LocalizationConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(LE).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var text = (string)reader.Value;
            return LocalizationExpression.Parse(text);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var le = (LE)value;
            writer.WriteValue(LocalizationExpression.Serialize(le));
        }
    }

    public class VocabularyConverter : JsonConverter {
        public override bool CanConvert(Type objectType) => objectType == typeof(Vocabulary);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            return new Vocabulary(serializer.Deserialize<Dictionary<string, string>>(reader));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var fi = typeof(Vocabulary).GetField("dictionary", BindingFlags.NonPublic | BindingFlags.Instance);
            var dict = fi.GetValue(value);
            serializer.Serialize(writer, dict);
        }
    }

}

public sealed class Vocabulary {
    public Vocabulary(Dictionary<string, string> dictionary) {
        this.dictionary = new Dictionary<string, string>(dictionary);
    }

    private Dictionary<string, string> dictionary;

    public string this[string index] {
        get {
            if (dictionary.ContainsKey(index)) {
                return dictionary[index];
            }
            return null;
        }
    }
}
