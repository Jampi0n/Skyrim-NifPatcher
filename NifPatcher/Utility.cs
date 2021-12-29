using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NifPatcher {
    public static class Utility {
        private static object myLock = new();
        private static readonly Dictionary<Type, Dictionary<string, object>> lookup = new();

        private static HashSet<string> GetAllVariations(string s) {
            s = s.ToLower();
            HashSet<string> variations = new();
            variations.Add(s);
            variations.Add(s.Replace("_", ""));
            variations.Add(s.Replace("_", " "));
            return variations;
        }

        public static object ParseEnumSmart<TEnum>(string s) {
            return lookup[typeof(TEnum)][s.ToLower()];
        }
        private static void InitEnumDict<TEnum>() {
            if(!lookup.ContainsKey(typeof(TEnum))) {
                lock(myLock) {
                    var newLookup = new Dictionary<string, object>();
                    var names = Enum.GetNames(typeof(TEnum));
                    var values = Enum.GetValues(typeof(TEnum));
                    for(int i = 0; i < names.Length; ++i) {
                        foreach(var variation in GetAllVariations(names[i])) {
                            newLookup.Add(variation, values.GetValue(i)!);
                        }
                    }
                    lookup.Add(typeof(TEnum), newLookup);
                }
            }
        }

        static Utility() {
            InitEnumDict<ShaderType>();
            InitEnumDict<ShaderFlags1>();
            InitEnumDict<ShaderFlags2>();
        }
    }
}
