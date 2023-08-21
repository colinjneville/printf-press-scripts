using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

partial class TapeValueType : ISerializeTo<TapeValueType.Serial> {
    [Serializable]
    [TypeConverter(typeof(Converter))]
    [JsonConverter(typeof(SingleFieldConverter<Serial, Guid>))]
    public struct Serial : IDeserializeTo<TapeValueType> {
        private Guid id;

        public Serial(Guid id) {
            this.id = id;
        }

        public TapeValueType Deserialize(Workspace workspace) => Lookup(id);

        public sealed class Converter : TypeConverter {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type source) => source == typeof(string);

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => new Serial(new Guid(value.ToString()));

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) => ((Serial)value).id.ToString();
        }
    }

    public Serial Serialize() => new Serial(id);

    public static implicit operator Serial(TapeValueType self) => self.Serialize();
}
