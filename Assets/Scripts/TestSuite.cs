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

partial class TestSuite : ISerializeTo<TestSuite.Serial> {
    [Serializable]
    public struct Serial : IDeserializeTo<TestSuite> {
        #region    GeneratedTestSuite

        private int count;
        // TODO generator function lookup?

        #endregion GeneratedTestSuite

        #region    ExampleTestSuite

        private TestCase.Serial[] testCases;

        #endregion

        public Serial(int count, IEnumerable<TestCase> testCases) {
            this.count = count;
            this.testCases = testCases.Select(tc => tc.Serialize()).ToArray();
        }

        public TestSuite Deserialize(Workspace workspace) {
            var generated = count == 0 ? null : new GeneratedTestSuite(count, null);
            var example = testCases.Length == 0 ? null : new ExampleTestSuite(testCases.Select(tc => tc.Deserialize(workspace)));

            RtlAssert.False(generated == null && example == null);
            if (generated == null) {
                return example;
            } else if (example == null) {
                return generated;
            } else {
                return new HybridTestSuite(example, generated);
            }
        }
    }

    public abstract Serial Serialize();
    public static implicit operator Serial(TestSuite self) => self.Serialize();
}

public abstract partial class TestSuite {
    public abstract int TestCaseCount { get; }
    public abstract IEnumerable<TestCase> TestCases { get; }
}

public sealed class GeneratedTestSuite : TestSuite {
    private int count;
    private Func<int, TestCase> generator;

    public GeneratedTestSuite(int count, Func<int, TestCase> generator) {
        this.count = count;
        this.generator = generator;
    }

    public override Serial Serialize() {
        return new Serial(count, Enumerable.Empty<TestCase>());
    }

    public override int TestCaseCount => count;

    public override IEnumerable<TestCase> TestCases {
        get {
            for (int i = 0; i < count; ++i) {
                yield return generator(i);
            }
        }
    }
}

public sealed class ExampleTestSuite : TestSuite {
    private ImmutableArray<TestCase> testCases;

    public ExampleTestSuite(params TestCase[] testCases) : this((IEnumerable<TestCase>)testCases) { }

    public ExampleTestSuite(IEnumerable<TestCase> testCases) {
        this.testCases = testCases.ToImmutableArray();
    }

    public override Serial Serialize() {
        return new Serial(0, testCases);
    }

    public override int TestCaseCount => testCases.Length;
    public override IEnumerable<TestCase> TestCases => testCases;
}

public sealed class HybridTestSuite : TestSuite {
    private ExampleTestSuite example;
    private GeneratedTestSuite generated;

    public HybridTestSuite(ExampleTestSuite example, GeneratedTestSuite generated) {
        this.example = example;
        this.generated = generated;
    }

    public override Serial Serialize() {
        return new Serial(generated.TestCaseCount, example.TestCases);
    }

    public override int TestCaseCount => example.TestCaseCount + generated.TestCaseCount;

    public override IEnumerable<TestCase> TestCases => example.TestCases.Concat(generated.TestCases);
}
