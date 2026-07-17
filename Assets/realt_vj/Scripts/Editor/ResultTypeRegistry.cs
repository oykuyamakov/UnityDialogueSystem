using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RealtVJ.Data;

namespace RealtVJ.Editor
{
    public readonly struct ResultTypeInfo
    {
        public Type Type { get; }
        public string DisplayName { get; }
        public string Category { get; }

        public ResultTypeInfo(Type type, string displayName, string category)
        {
            Type = type;
            DisplayName = displayName;
            Category = category;
        }
    }

    public static class ResultTypeRegistry
    {
        private static List<ResultTypeInfo> s_All;
        private static Dictionary<Type, ResultTypeInfo> s_ByType;
        private static Dictionary<string, ResultTypeInfo> s_ByDisplayName;

        public static IReadOnlyList<ResultTypeInfo> All
        {
            get
            {
                EnsureInitialized();
                return s_All;
            }
        }

        private static void EnsureInitialized()
        {
            if (s_All != null) return;

            s_All = new List<ResultTypeInfo>();
            s_ByType = new Dictionary<Type, ResultTypeInfo>();
            s_ByDisplayName = new Dictionary<string, ResultTypeInfo>();

            var baseType = typeof(Result);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

                foreach (var type in types)
                {
                    if (type.IsAbstract || !baseType.IsAssignableFrom(type)) continue;

                    var attr = type.GetCustomAttribute<ResultInfoAttribute>();
                    var displayName = attr?.DisplayName ?? type.Name;
                    var category = attr?.Category ?? "General";

                    var info = new ResultTypeInfo(type, displayName, category);
                    s_All.Add(info);
                    s_ByType[type] = info;
                    s_ByDisplayName[displayName] = info;
                }
            }

            s_All.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal));
        }

        public static Result CreateInstance(Type type)
        {
            return (Result)Activator.CreateInstance(type);
        }

        public static string GetDisplayName(Type type)
        {
            EnsureInitialized();
            return s_ByType.TryGetValue(type, out var info) ? info.DisplayName : type.Name;
        }

        public static Type GetTypeByDisplayName(string displayName)
        {
            EnsureInitialized();
            return s_ByDisplayName.TryGetValue(displayName, out var info) ? info.Type : null;
        }

        public static List<string> GetDisplayNames()
        {
            EnsureInitialized();
            return s_All.Select(i => i.DisplayName).ToList();
        }
    }
}
