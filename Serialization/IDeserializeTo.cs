using Functional.Option;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = LocalizationFormatted;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public interface IDeserializeTo {

}

public interface IDeserializeTo<out T> : IDeserializeTo {
    T Deserialize(Workspace workspace);
}

public static class DeserializeToExtensions {
    public static string AsJson(this IDeserializeTo self) {
        return JsonConvert.SerializeObject(self, SerializationUtility.Settings);
    }

    public static T AsSerial<T>(this string self) where T : IDeserializeTo {
        return JsonConvert.DeserializeObject<T>(self, SerializationUtility.Settings);
    }
}

public sealed class SingleFieldConverter<TObj, TField> : JsonConverter {
    private FieldInfo fieldInfo;

    public SingleFieldConverter() {
        var fields = typeof(TObj).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(fi => fi.FieldType == typeof(TField)).ToArray();
        switch (fields.Length) {
            case 0:
                throw RtlAssert.NotReached($"Type '{typeof(TObj)}' has no fields of type '{typeof(TField)}'");
            case 1:
                fieldInfo = fields[0];
                break;
            default:
                throw RtlAssert.NotReached($"Type '{typeof(TObj)}' has multiple fields of type '{typeof(TField)}'");
        }
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(TObj);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) {
            return null;
        }
        //var token = JToken.Load(reader);
        var value = serializer.Deserialize(reader, typeof(TField));
        //var convertedValue = Convert.ChangeType(value, typeof(TField));

        var obj = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TObj));
        //fieldInfo.SetValue(obj, Convert.ChangeType(token, typeof(TField)));
        fieldInfo.SetValue(obj, value);
        return obj;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        var primitive = fieldInfo.GetValue(value);
        serializer.Serialize(writer, primitive);
        //writer.WriteValue(primitive);
    }
}
