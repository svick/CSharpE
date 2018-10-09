using System.Collections.Generic;

namespace CSharpE.Transform.VisualStudio
{
    // copied from Roslyn
    // TODO: proper attribution

    internal class LanguageMetadata
    {
        public string Language { get; }

        public LanguageMetadata(IDictionary<string, object> data)
        {
            this.Language = (string)data.GetValueOrDefault("Language");
        }
    }

    internal class LanguageServiceMetadata : LanguageMetadata
    {
        public string ServiceType { get; }
        public string Layer { get; }

        public IReadOnlyDictionary<string, object> Data { get; }

        public LanguageServiceMetadata(IDictionary<string, object> data)
            : base(data)
        {
            this.ServiceType = (string)data.GetValueOrDefault("ServiceType");
            this.Layer = (string)data.GetValueOrDefault("Layer");
            this.Data = (IReadOnlyDictionary<string, object>)data;
        }
    }

    internal static class IDictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            return default;
        }
    }
}
