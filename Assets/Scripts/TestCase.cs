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

partial class TestCase : ISerializeTo<TestCase.Serial> {
    [Serializable]
    public struct Serial : IDeserializeTo<TestCase> {
        private ReplayLog.Serial initialization;
        private string expectedResult;

        public Serial(ReplayLog.Serial initialization, IEnumerable<TapeValue> expectedResult) {
            this.initialization = initialization;
            this.expectedResult = expectedResult.Select(r => r.Serialize()).AsString();
        }

        public TestCase Deserialize(Workspace workspace) {
            return new TestCase(initialization.Deserialize(workspace), expectedResult.AsTapeValueSerials().Select(r => r.Deserialize(workspace)));
        }
    }

    public Serial Serialize() {
        return new Serial(initialization, expectedResult);
    }

    public static implicit operator Serial(TestCase self) => self.Serialize();
}

public sealed partial class TestCase {
    private ReplayLog initialization;
    private ImmutableArray<TapeValue> expectedResult;

    public TestCase(ReplayLog initialization, params TapeValue[] expectedResult) : this(initialization, (IEnumerable<TapeValue>)expectedResult) { }

    public TestCase(ReplayLog initialization, IEnumerable<TapeValue> expectedResult) {
        this.initialization = initialization;
        this.expectedResult = expectedResult.ToImmutableArray();
    }

    public ReplayLog Initialization => initialization;

    public IImmutableList<TapeValue> ExpectedResult => expectedResult;
}
