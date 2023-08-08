using Functional.Option;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;


public static class Utility {
    private sealed class NoopIDisposable : IDisposable {
        public NoopIDisposable() { }

        public void Dispose() { }
    }
    private static IDisposable noopDisposable = new NoopIDisposable();
    public static IDisposable NoopDisposable => noopDisposable;

    public const string resourcesDirectoryName = "Resources";
    private const string dataDirectoryName = "Data";
    public const string languagesDirectoryName = "Languages";

    // Hack to avoid initializing these (must be done main thread) when accessing any unrelated function
    private class UtilityCctor {
        public static readonly DirectoryInfo installPath = Directory.GetParent(Application.dataPath);
        public static readonly DirectoryInfo userPath = new DirectoryInfo(Application.persistentDataPath);

        // Defer these to cctor to ensure directories are created first if they do no exist
        public static readonly DirectoryInfo installResourcesPath;
        public static readonly DirectoryInfo installDataPath;
        public static readonly DirectoryInfo installLanguagesPath;

        static UtilityCctor() {
            installPath.CreateSubdirectory(resourcesDirectoryName);
            installPath.CreateSubdirectory(dataDirectoryName);
            installPath.CreateSubdirectory(languagesDirectoryName);

            installResourcesPath = installPath.GetDirectories(resourcesDirectoryName, SearchOption.TopDirectoryOnly).Single();
            installDataPath = installPath.GetDirectories(dataDirectoryName, SearchOption.TopDirectoryOnly).Single();
            installLanguagesPath = installPath.GetDirectories(languagesDirectoryName, SearchOption.TopDirectoryOnly).Single();
        }
    }

    public static string InstallResourcesPath {
        get {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "Export");
#else
            return UtilityCctor.installResourcesPath.FullName;
#endif
        }
    }

    public static DirectoryInfo InstallLanguagesPath => UtilityCctor.installLanguagesPath;

    public static IEnumerable<T> Yield<T>(this T item) {
        yield return item;
    }

    public static void AssignOnce<T>(ref T field, T value) where T : class {
        Assert.Null(field);
        Assert.NotNull(value);
        field = value;
    }

    public static int GetCompositeHashCode(params object[] objs) {
        return GetCompositeHashCode((IEnumerable<object>)objs);
    }

    public static int GetCompositeHashCode(IEnumerable<object> objs) {
        if (objs is null) {
            return 0;
        }
        unchecked {
            int initialPrime = 17;
            int iterativePrime = 23;

            int hash = initialPrime;

            foreach (object obj in objs) {
                hash *= iterativePrime;
                hash += obj == null ? 0 : obj.GetHashCode();
            }

            return hash;
        }
    }

    public static void Swap<T>(ref T a, ref T b) {
        T temp = a;
        a = b;
        b = temp;
    }

    public static void MatchParent(this RectTransform self) {
        self.anchorMin = Vector2.zero;
        self.anchorMax = Vector2.one;
        self.sizeDelta = Vector2.zero;
    }

    public static TransformProxy CreateProxy(this Transform parent, string objectName, bool forwardActive = false, bool forwardDestruction = false, bool forwardTransformation = false) {
        var proxyGo = new GameObject(objectName);
        var proxy = proxyGo.AddComponent<TransformProxy>();
        proxy.ForwardActive = forwardActive;
        proxy.ForwardDestruction = forwardDestruction;
        proxy.ForwardTransformation = forwardTransformation;
        proxyGo.transform.SetParent(parent, false);
        var proxyRt = proxyGo.AddComponent<RectTransform>();
        proxyRt.MatchParent();

        return proxy;
    }

    public static void DestroyGameObject(GameObject go) {
        if (go != null) {
            UnityEngine.Object.Destroy(go);
        }
    }

    public static void DestroyGameObject(this Component c) {
        if (c != null) {
            UnityEngine.Object.Destroy(c.gameObject);
        }
    }

    public static Screen GetScreen(this Component c) => Screen.Get(c);

    public static int Mod(int a, int b) {
        var value = a % b;
        if (value < 0) {
            value += b;
        }
        return value;
    }

    public static void ExecuteNextFrame(this MonoBehaviour mb, Action action) {
        mb.StartCoroutine(ExecuteNextFrameHelper(action));
    }

    public static void ExecuteNextFrame(Action action) {
        Overseer.Instance.StartCoroutine(ExecuteNextFrameHelper(action));
    }

    private static System.Collections.IEnumerator ExecuteNextFrameHelper(Action action) {
        yield return new WaitForEndOfFrame();
        action();
    }

    public static IEnumerable<int> CountTo(int start, int end) {
        int i = start;
        int dir = start > end ? -1 : 1;
        yield return i;
        while (i != end) {
            i += dir;
            yield return i;
        }
    }

    /// <summary>
    /// A wrapper to remove the stupid "(clone)" from the new object name
    /// </summary>
    public static T Instantiate<T>(T original, Transform parent) where T : UnityEngine.Object {
        var t = UnityEngine.Object.Instantiate(original, parent);
        t.name = original.name;
        return t;
    }

    public static IEnumerable<T> RepeatConstruct<T>(int count) where T : new() {
        for (int i = 0; i < count; ++i) {
            yield return new T();
        }
    }

    public static T NewGameObject<T>(string name = null) where T : Component {
        name = name ?? typeof(T).Name;
        var go = new GameObject(name, typeof(T));
        return go.GetComponent<T>();
    }

    public static Option<TValue> GetOrNone<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key) {
        if (self.TryGetValue(key, out TValue value)) {
            return value;
        } else {
            return Option.None;
        }
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> self) {
        var list = self.ToList();
        var random = new System.Random();
        for (int i = 0; i < list.Count; ++i) {
            var j = random.Next(list.Count);
            var swap = list[i];
            list[i] = list[j];
            list[j] = swap;
        }
        return list;
    }

    public static KeyValuePair<TKey, TValue> MakeKVP<TKey, TValue>(TKey key, TValue value) => new KeyValuePair<TKey, TValue>(key, value);

    public static Option<TT> Cast<T, TT>(this Option<T> self) where T : TT => Option.Create<TT>(self.ValueOrDefault);
    public static Option<TT> ExplicitCast<T, TT>(this Option<T> self) => Option.Create((TT)(object)self.ValueOrDefault);

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> self) => self.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public static T Pop<T>(this IList<T> self) {
        Assert.NotEmpty(self);
        int lastIndex = self.Count - 1;
        var value = self[lastIndex];
        self.RemoveAt(lastIndex);
        return value;
    }

    public static int IndexOf<T>(this IReadOnlyList<T> self, T element) {
        for (int i = 0; i < self.Count; ++i) {
            if (Equals(element, self[i])) {
                return i;
            }
        }
        throw new ArgumentOutOfRangeException();
    }

    public static Vector2 WithX(this Vector2 self, float x) => new Vector2(x, self.y);
    public static Vector2 WithY(this Vector2 self, float y) => new Vector2(self.x, y);
    public static Vector3 WithZ(this Vector2 self, float z) => new Vector3(self.x, self.y, z);
    public static Vector3 WithX(this Vector3 self, float x) => new Vector3(x, self.y, self.z);
    public static Vector3 WithY(this Vector3 self, float y) => new Vector3(self.x, y, self.z);
    public static Vector3 WithZ(this Vector3 self, float z) => new Vector3(self.x, self.y, z);
    public static Color WithR(this Color self, float r) => new Color(r, self.g, self.b, self.a);
    public static Color WithG(this Color self, float g) => new Color(self.r, g, self.b, self.a);
    public static Color WithB(this Color self, float b) => new Color(self.r, self.g, b, self.a);
    public static Color WithA(this Color self, float a) => new Color(self.r, self.g, self.b, a);

    public static Vector2 Abs(this Vector2 self) => new Vector2(Mathf.Abs(self.x), Mathf.Abs(self.y));
    public static Vector3 Abs(this Vector3 self) => new Vector3(Mathf.Abs(self.x), Mathf.Abs(self.y), Mathf.Abs(self.z));

    public static GameObject CreateQuadObject(string name, Material material, bool withCollider = false, bool shareMaterial = true) {
        return CreateMeshObject(name, material, Overseer.GlobalAssets.QuadMesh, withCollider, shareMaterial);
    }

    public static GameObject CreateRectObject(string name, Material material, bool withCollider = false, bool shareMaterial = true) {
        var go = CreateQuadObject(name, material, withCollider, shareMaterial);
        go.AddComponent<RectTransform>();
        return go;
    }

    public static GameObject CreateCubeObject(string name, Material material, bool withCollider = false, bool shareMaterial = true) {
        return CreateMeshObject(name, material, Overseer.GlobalAssets.CubeMesh, withCollider, shareMaterial);
    }

    private static GameObject CreateMeshObject(string name, Material material, Mesh mesh, bool withCollider, bool shareMaterial) {
        var go = new GameObject(name);
        var filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        if (shareMaterial) {
            renderer.sharedMaterial = material;
        } else {
            renderer.material = material;
        }
        if (withCollider) {
            // OPT this can be BoxCollider[2D]
            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;
        }

        return go;
    }
}

public static class SerializationUtility {
    private static JsonSerializerSettings serializeSettings = new JsonSerializerSettings {
        TypeNameHandling = TypeNameHandling.Auto,
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        ReferenceLoopHandling = ReferenceLoopHandling.Error,
        Formatting = Formatting.Indented,
        //PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
        ContractResolver = new NoConstructorPrivateContractResolver(),
    };

    public static JsonSerializerSettings Settings => serializeSettings;

    static SerializationUtility() {
        serializeSettings.Converters.Add(Record.SerializationConverter);
        serializeSettings.Converters.Add(new Language.LocalizationConverter());
        serializeSettings.Converters.Add(new Language.VocabularyConverter());
        serializeSettings.Converters.Add(new GuidConverter());
        serializeSettings.Converters.Add(new Color32Converter());
    }

    private sealed class Color32Converter : JsonConverter {
        public override bool CanConvert(Type objectType) => objectType == typeof(Color32);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var array = serializer.Deserialize<int[]>(reader);
            if (array.Length < 3) {
                Debug.LogWarning($"Incorrect Color32 format '{array}'");
                return Color.black;
            } else {
                byte a;
                if (array.Length < 4) {
                    a = 255;
                } else {
                    a = (byte)array[3];
                }
                return new Color32((byte)array[0], (byte)array[1], (byte)array[2], a);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var rgba = (Color32)value;
            int[] array;
            if (rgba.a == 255) {
                array = new int[3] { rgba[0], rgba[1], rgba[2] };
            } else {
                array = new int[4] { rgba[0], rgba[1], rgba[2], rgba[3] };
            }
            
            serializer.Serialize(writer, array);
        }
    }

    private sealed class GuidConverter : JsonConverter {
        public override bool CanConvert(Type objectType) => objectType == typeof(Guid);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var base64 = (string)serializer.Deserialize(reader, typeof(string));
            // TODO for now, allow reading base16 until I can make sure all files have been converted to base64
            if (base64.Length > 24) {
                return new Guid(base64);
            }

            return Base64ToGuid(base64);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            serializer.Serialize(writer, GuidToBase64((Guid)value));
        }

        private static string GuidToBase64(Guid guid) => Convert.ToBase64String(guid.ToByteArray()).Substring(0, 22);

        private static Guid Base64ToGuid(string base64) => new Guid(Convert.FromBase64String(base64 + "=="));
    }

    public struct ReaderWriter {
        public ReaderWriter(Func<JsonReader, Type, object, JsonSerializer, object> reader, Action<JsonWriter, object, JsonSerializer> writer) {
            this.reader = reader;
            this.writer = writer;
        }

        private Func<JsonReader, Type, object, JsonSerializer, object> reader;

        public Func<JsonReader, Type, object, JsonSerializer, object> Reader => reader;

        private Action<JsonWriter, object, JsonSerializer> writer;

        public Action<JsonWriter, object, JsonSerializer> Writer => writer;
    }

    // https://stackoverflow.com/questions/24106986/json-net-force-serialization-of-all-private-fields-and-all-fields-in-sub-classe
    private class PrivateContractResolver : DefaultContractResolver {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
            // For readability, the base-most class members will be added first
            var fieldInfos = Enumerable.Empty<IEnumerable<FieldInfo>>();
            while (type != null) {
                fieldInfos = fieldInfos.Append(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                type = type.BaseType;
            }

            var props = fieldInfos.Reverse().SelectMany(e => e).Select(f => base.CreateProperty(f, memberSerialization)).ToList();
            props.ForEach(p => { p.Writable = true; p.Readable = true; });
            return props;
        }

        public override JsonContract ResolveContract(Type type) {
            var contract = base.ResolveContract(type);
            if (IsCustomType(type)) {
                contract.IsReference = false;
            }
            return contract;
        }

        protected bool IsCustomType(Type type) {
            // TODO
            return string.IsNullOrWhiteSpace(type.Namespace);
        }
    }

    private class NoConstructorPrivateContractResolver : PrivateContractResolver {
        protected override JsonObjectContract CreateObjectContract(Type type) {
            var c = base.CreateObjectContract(type);

            if (c.DefaultCreator == null && IsCustomType(type)) {
                c.OverrideCreator = args => UninitializedConstructor(type, args);
                c.CreatorParameters.Clear();
            }

            return c;
        }

        private static object UninitializedConstructor(Type type, object[] args) {
            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
            return instance;
        }


        /*
        public override bool CanConvert(Type objectType) {
            var constructorInfo = objectType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, new ParameterModifier[] { });
            return constructorInfo == null;
        }
        */
    }

    
}
