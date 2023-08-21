using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;


public abstract class UserException : Exception {
    private LE text;
    protected UserException(LE text, Exception innerException = null) : base(text.ToString(), innerException) {
        this.text = text;
    }

    public LE Text => text;
}
