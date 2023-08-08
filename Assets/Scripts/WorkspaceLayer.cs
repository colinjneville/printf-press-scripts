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


public sealed partial class WorkspaceLayer {
    private Dictionary<Guid, Cryptex> cryptexes;
    [JsonIgnore]
    private Option<ExecutionContext> executionContext;

    public WorkspaceLayer(params Cryptex[] cryptexes) : this((IEnumerable<Cryptex>)cryptexes) { }
    public WorkspaceLayer(IEnumerable<Cryptex> cryptexes) {
        this.cryptexes = cryptexes.ToDictionary(c => c.Id, c => c);
    }

    public void SetContext(ExecutionContext ec) {
        Assert.NotHasValue(executionContext);
        executionContext = ec;
    }

    public IReadOnlyCollection<Cryptex> Cryptexes => cryptexes.Values;
    public Option<ExecutionContext> ExecutionContext => executionContext;

    public Option<Cryptex> GetCryptex(Guid cryptexId) {
        return cryptexes.GetOrNone(cryptexId);
    }

    public void AddCryptex(Cryptex cryptex) {
        cryptexes.Add(cryptex.Id, cryptex);
        foreach (var view in View) {
            view.OnAddCryptex(cryptex);
        }
    }

    public void RemoveCryptex(Guid id) {
        foreach (var cryptex in GetCryptex(id)) {
            cryptexes.Remove(id);
            foreach (var view in View) {
                view.OnRemoveCryptex(cryptex);
            }
        }
    }

}
