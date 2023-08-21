using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Functional.Option;

public class AssertionException : Exception {
    public AssertionException(string message = "") : base(message) { }

    public AssertionException(string message, AssertionException innerException) : base(message, innerException) { }
}

/// <summary>
/// Debug Assert methods
/// </summary>
public static class Assert {
    [System.Diagnostics.Conditional("DEBUG")]
    public static void Null<T>(T obj, string msg = null) where T : class {
        RtlAssert.Null(obj, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void RefNull<T>(T obj, string msg = null) where T : class {
        RtlAssert.RefNull(obj, msg); 
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotNull<T>(T obj, string msg = null) where T : class {
        RtlAssert.NotNull(obj, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotRefNull<T>(T obj, string msg = null) where T : class {
        RtlAssert.NotRefNull(obj, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void HasValue<T>(Option<T> obj, string msg = null) where T : class {
        RtlAssert.HasValue(obj, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotHasValue<T>(Option<T> obj, string msg = null) where T : class {
        RtlAssert.NotHasValue(obj, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Default<T>(T obj, string msg = null) {
        RtlAssert.Default(obj, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void ContainsNoDefault<T>(IEnumerable<T> obj, string msg = null) {
        RtlAssert.ContainsNoDefault(obj, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void ContainsNoNull<T>(IEnumerable<T> obj, string msg = null) where T : class {
        RtlAssert.ContainsNoNull(obj, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void True(bool expression, string msg = null) {
        RtlAssert.True(expression, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void False(bool expression, string msg = null) {
        RtlAssert.False(expression, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotNegative(int expression, string msg = null) {
        RtlAssert.NotNegative(expression, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Negative(int expression, string msg = null) {
        RtlAssert.Negative(expression, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotPositive(int expression, string msg = null) {
        RtlAssert.NotPositive(expression, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Positive(int expression, string msg = null) {
        RtlAssert.Positive(expression, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotZero(int expression, string msg = null) {
        RtlAssert.NotZero(expression, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Zero(int expression, string msg = null) {
        RtlAssert.Zero(expression, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Within(int expression, int lowerBound, int upperBound, string msg = null) {
        RtlAssert.Within(expression, lowerBound, upperBound, msg);
    }

    // There seems to be a bug in Mono when doing overload resolution with default parameters.
    // Since the float version needs to be named differently than the int version, cover all inclusive/exclusive bases
    [System.Diagnostics.Conditional("DEBUG")]
    public static void WithinIE(float expression, float lowerBound, float upperBound, string msg = null) {
        RtlAssert.WithinIE(expression, lowerBound, upperBound, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void WithinII(float expression, float lowerBound, float upperBound, string msg = null) {
        RtlAssert.WithinII(expression, lowerBound, upperBound, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void WithinEE(float expression, float lowerBound, float upperBound, string msg = null) {
        RtlAssert.WithinEE(expression, lowerBound, upperBound, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void WithinEI(float expression, float lowerBound, float upperBound, string msg = null) {
        RtlAssert.WithinEI(expression, lowerBound, upperBound, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Empty<T>(IEnumerable<T> enumerable, string msg = null) {
        RtlAssert.Empty(enumerable, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotEmpty<T>(IEnumerable<T> enumerable, string msg = null) {
        RtlAssert.NotEmpty(enumerable, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Equal(object a, object b, string msg = null) {
        RtlAssert.Equal(a, b, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotEqual(object a, object b, string msg = null) {
        RtlAssert.NotEqual(a, b, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Greater(int expression, int target, string msg = null) {
        RtlAssert.Greater(expression, target, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void GreaterOrEqual(int expression, int target, string msg = null) {
        RtlAssert.GreaterOrEqual(expression, target, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Less(int expression, int target, string msg = null) {
        RtlAssert.Less(expression, target, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void LessOrEqual(int expression, int target, string msg = null) {
        RtlAssert.LessOrEqual(expression, target, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void OfType(object obj, System.Type type, string msg = null) {
        RtlAssert.OfType(obj, type, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void Implies(bool expression1, bool expression2, string msg = null) {
        RtlAssert.Implies(expression1, expression2, msg);
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void NotReached(string msg = null) {
        throw RtlAssert.NotReached(msg);
    }
}

/// <summary>
/// Retail Assert methods
/// </summary>
public static class RtlAssert {
    public static void Null<T>(T obj, string msg = null) where T : class {
        msg = msg ?? "Assertion failed";

        if (!EqualityComparer<T>.Default.Equals(obj, null)) {
            throw new AssertionException(msg);
        }
    }

    public static void RefNull<T>(T obj, string msg = null) where T : class {
        msg = msg ?? "Assertion failed";

        if (obj is object) {
            throw new AssertionException(msg);
        }
    }

    public static void NotNull<T>(T obj, string msg = null) where T : class {
        msg = msg ?? "Assertion failed";

        if (EqualityComparer<T>.Default.Equals(obj, null)) {
            throw new AssertionException(msg);
        }
    }

    public static void NotRefNull<T>(T obj, string msg = null) where T : class {
        msg = msg ?? "Assertion failed";

        if (obj is null) {
            throw new AssertionException(msg);
        }
    }

    public static void HasValue<T>(Option<T> obj, string msg = null) where T : class {
        msg = msg ?? "Assertion failed";

        if (!obj.HasValue) {
            throw new AssertionException(msg);
        }
    }

    public static void NotHasValue<T>(Option<T> obj, string msg = null) where T : class {
        msg = msg ?? "Assertion failed";

        if (obj.HasValue) {
            throw new AssertionException(msg);
        }
    }

    public static void Default<T>(T obj, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (!EqualityComparer<T>.Default.Equals(obj, default)) {
            throw new AssertionException(msg);
        }
    }

    public static void ContainsNoDefault<T>(IEnumerable<T> obj, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (obj.Contains(default)) {
            throw new AssertionException(msg);
        }
    }

    public static void ContainsNoNull<T>(IEnumerable<T> obj, string msg = null) where T : class {
        msg = msg ?? "Assertion failed";

        if (obj.Contains(null)) {
            throw new AssertionException(msg);
        }
    }

    public static void True(bool expression, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (!expression) {
            throw new AssertionException(msg);
        }
    }

    public static void False(bool expression, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression) {
            throw new AssertionException(msg);
        }
    }

    public static void NotNegative(int expression, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression < 0) {
            throw new AssertionException(msg);
        }
    }

    public static void Negative(int expression, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression >= 0) {
            throw new AssertionException(msg);
        }
    }

    public static void NotPositive(int expression, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression > 0) {
            throw new AssertionException(msg);
        }
    }

    public static void Positive(int expression, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression <= 0) {
            throw new AssertionException(msg);
        }
    }

    public static void NotZero(int expression, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression == 0) {
            throw new AssertionException(msg);
        }
    }

    public static void Zero(int expression, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression != 0) {
            throw new AssertionException(msg);
        }
    }

    public static void Within(int expression, int lowerBound, int upperBound, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression < lowerBound || expression >= upperBound) {
            throw new AssertionException(msg);
        }
    }

    // There seems to be a bug in Mono when doing overload resolution with default parameters.
    // Since the float version needs to be named differently than the int version, cover all inclusive/exclusive bases
    public static void WithinIE(float expression, float lowerBound, float upperBound, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression < lowerBound || expression >= upperBound) {
            throw new AssertionException(msg);
        }
    }

    public static void WithinEE(float expression, float lowerBound, float upperBound, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression <= lowerBound || expression >= upperBound) {
            throw new AssertionException(msg);
        }
    }

    public static void WithinII(float expression, float lowerBound, float upperBound, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression < lowerBound || expression > upperBound) {
            throw new AssertionException(msg);
        }
    }

    public static void WithinEI(float expression, float lowerBound, float upperBound, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression <= lowerBound || expression > upperBound) {
            throw new AssertionException(msg);
        }
    }

    public static void Empty<T>(IEnumerable<T> enumerable, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (enumerable.Count() != 0) {
            throw new AssertionException(msg);
        }
    }

    public static void NotEmpty<T>(IEnumerable<T> enumerable, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (enumerable.Count() == 0) {
            throw new AssertionException(msg);
        }
    }

    public static void Equal(object a, object b, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (!a.Equals(b)) {
            throw new AssertionException(msg);
        }
    }

    public static void NotEqual(object a, object b, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (a.Equals(b)) {
            throw new AssertionException(msg);
        }
    }

    public static void Greater(int expression, int target, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression <= target) {
            throw new AssertionException(msg);
        }
    }

    public static void GreaterOrEqual(int expression, int target, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression < target) {
            throw new AssertionException(msg);
        }
    }

    public static void Less(int expression, int target, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression >= target) {
            throw new AssertionException(msg);
        }
    }

    public static void LessOrEqual(int expression, int target, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression > target) {
            throw new AssertionException(msg);
        }
    }

    public static void OfType(object obj, System.Type type, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (!type.IsSubclassOf(obj.GetType())) {
            throw new AssertionException(msg);
        }
    }

    public static void Implies(bool expression1, bool expression2, string msg = null) {
        msg = msg ?? "Assertion failed";

        if (expression1 && !expression2) {
            throw new AssertionException(msg);
        }
    }

    public static T ValueOrAssert<T>(this Option<T> self, string msg = null) {
        msg = msg ?? "Assertion failed";

        foreach (var value in self) {
            return value;
        }
        throw new AssertionException(msg);
    }

    public static AssertionException NotReached(string msg = null) {
        msg = msg ?? "Assertion failed";

        // Offer to return an exception so calling code can attempt to throw to avoid the compiler from detecting no return value
        throw new AssertionException(msg);
    }
}
