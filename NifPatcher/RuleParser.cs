using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Drawing;

namespace NifPatcher {

    public static class RuleParser {

        public class RuleParseException : Exception {
            public readonly int line;
            public RuleParseException(int line, string msg) : base(msg) {
                this.line = line;
            }
        }

        private static readonly Dictionary<string, Action<string, RuleSet>> parseRules = new();
        private static string currentKey = "";

        private class RuleSet {
            public static readonly Dictionary<string, List<RuleSet>> diffuseRules = new();
            public readonly Dictionary<string, Tuple<string, Func<NifFileWrapper.NiShapeWrapper, bool>>> rules = new();
            public readonly List<string> rulesOrder = new();
            public string pathFilter = "";
            public void SetRule(string s, Func<NifFileWrapper.NiShapeWrapper, bool> func) {
                if(!rules.ContainsKey(currentKey)) {
                    rulesOrder.Add(currentKey);
                }
                rules[currentKey] = new Tuple<string, Func<NifFileWrapper.NiShapeWrapper, bool>>(s, func);
            }

            public RuleSet Clone() {
                var clone = new RuleSet();
                foreach(var key in rulesOrder) {
                    clone.rulesOrder.Add(key);
                    clone.rules[key] = new Tuple<string, Func<NifFileWrapper.NiShapeWrapper, bool>>(rules[key].Item1, rules[key].Item2);
                }
                clone.pathFilter = pathFilter;
                return clone;
            }

            public bool Apply(NifFileWrapper.NiShapeWrapper shape) {
                var modified = false;
                foreach(var ruleKey in rulesOrder) {
                    var rule = rules[ruleKey];
                    var tmp = rule.Item2(shape);
                    modified = tmp || modified;
                }
                return modified;
            }
        }

        private static string TranslateTexture(NifFileWrapper.NiShapeWrapper shape, string s) {
            var diffuse = shape.DiffuseMap;
            s = s.ToLower();
            var regex = new Regex(@"(.*)#diffuse(\d*)\+(.*)");
            var match = regex.Match(s);
            if(!match.Success) {
                return s;
            }

            var result = string.Concat(match.Groups[1].Value, diffuse.AsSpan(0, diffuse.Length - int.Parse(match.Groups[2].Value)), match.Groups[3].Value);
            return result;
        }

        private static ShaderFlags1 FromString_ShaderFlags1(string s) {
            var split = s.Split(",");
            var flags = ShaderFlags1.None;
            foreach(var f in split) {
                var flagName = f.Trim();
                if(Utility.ParseEnumSmart<ShaderFlags1>(flagName) is ShaderFlags1 flag) {
                    flags |= flag;
                }
            }
            return flags;
        }

        private static ShaderFlags2 FromString_ShaderFlags2(string s) {
            var split = s.Split(",");
            var flags = ShaderFlags2.None;
            foreach(var f in split) {
                var flagName = f.Trim();
                if(Utility.ParseEnumSmart<ShaderFlags2>(flagName) is ShaderFlags2 flag) {
                    flags |= flag;
                }
            }
            return flags;
        }

        private static ShaderType FromString_ShaderType(string s) {
            return (ShaderType)Utility.ParseEnumSmart<ShaderType>(s);
        }

        private static Color FromString_Color(string s) {
            var red = int.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            var green = int.Parse(s.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var blue = int.Parse(s.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb(red, green, blue);
        }

        static RuleParser() {

            // numeric values

            AddParseRule("EnvironmentMapScale", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.EnvironmentMapScale != value) {
                        shape.EnvironmentMapScale = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("EmissiveMultiple", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.EmissiveMultiple != value) {
                        shape.EmissiveMultiple = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("Glossiness", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.Glossiness != value) {
                        shape.Glossiness = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("SpecularStrength", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.SpecularStrength != value) {
                        shape.SpecularStrength = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("LightingEffect1", "LightningEffect1", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.LightingEffect1 != value) {
                        shape.LightingEffect1 = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("LightingEffect2", "LightningEffect2", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.LightingEffect2 != value) {
                        shape.LightingEffect2 = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("ParallaxEnvmapStrength", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.ParallaxEnvmapStrength != value) {
                        shape.ParallaxEnvmapStrength = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("ParallaxInnerLayerThickness", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.ParallaxInnerLayerThickness != value) {
                        shape.ParallaxInnerLayerThickness = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("ParallaxRefractionScale", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.ParallaxRefractionScale != value) {
                        shape.ParallaxRefractionScale = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("ParallaxInnerLayerTextureScaleX", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.ParallaxInnerLayerTextureScaleX != value) {
                        shape.ParallaxInnerLayerTextureScaleX = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("ParallaxInnerLayerTextureScaleY", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.ParallaxInnerLayerTextureScaleY != value) {
                        shape.ParallaxInnerLayerTextureScaleY = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("Alpha", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.Alpha != value) {
                        shape.Alpha = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("RefractionStrength", (string s, RuleSet ruleSet) => {
                var value = float.Parse(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.RefractionStrength != value) {
                        shape.RefractionStrength = value;
                        return true;
                    }
                    return false;
                });
            });

            // colors

            AddParseRule("EmissiveColor", (string s, RuleSet ruleSet) => {
                var value = FromString_Color(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.EmissiveColor != value) {
                        shape.EmissiveColor = value;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("SpecularColor", (string s, RuleSet ruleSet) => {
                var value = FromString_Color(s);
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    if(shape.SpecularColor != value) {
                        shape.SpecularColor = value;
                        return true;
                    }
                    return false;
                });
            });

            // textures

            AddParseRule("Specular", "SpecularMap", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var t = TranslateTexture(shape, s);
                    if(shape.SpecularMap != t) {
                        shape.SpecularMap = t;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("Normal", "NormalMap", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var t = TranslateTexture(shape, s);
                    if(shape.NormalMap != t) {
                        shape.NormalMap = t;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("Cube", "CubeMap", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var t = TranslateTexture(shape, s);
                    if(shape.CubeMap != t) {
                        shape.CubeMap = t;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("Backlight", "BacklightMap", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var t = TranslateTexture(shape, s);
                    if(shape.BacklightMap != t) {
                        shape.BacklightMap = t;
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("Subsurface", "SubsurfaceMap", "SubsurfaceTint", "SubsurfaceTintMap", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var t = TranslateTexture(shape, s);
                    if(shape.SubsurfaceMap != t) {
                        shape.SubsurfaceMap = t;
                        return true;
                    }
                    return false;
                });
            });

            // flags

            AddParseRule("FlagAdd1", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var flags = FromString_ShaderFlags1(s);
                    if(!shape.HasAllFlags1(flags)) {
                        shape.AddShaderFlags1(flags);
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("FlagRemove1", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var flags = FromString_ShaderFlags1(s);
                    if(!shape.HasNoFlags1(flags)) {
                        shape.RemoveShaderFlags1(flags);
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("FlagAdd2", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var flags = FromString_ShaderFlags2(s);
                    if(!shape.HasAllFlags2(flags)) {
                        shape.AddShaderFlags2(flags);
                        return true;
                    }
                    return false;
                });
            });

            AddParseRule("FlagRemove2", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var flags = FromString_ShaderFlags2(s);
                    if(!shape.HasNoFlags2(flags)) {
                        shape.RemoveShaderFlags2(flags);
                        return true;
                    }
                    return false;
                });
            });

            // shader type

            AddParseRule("ShaderType", (string s, RuleSet ruleSet) => {
                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var shaderType = FromString_ShaderType(s);
                    if(shape.ShaderType != shaderType) {
                        shape.ShaderType = shaderType;
                        return true;
                    }
                    return false;
                });
            });


            AddParseRule("PathReplace", (string s, RuleSet ruleSet) => {
                var tmp = s.Split(":");
                if(tmp.Length != 2) {
                    throw new Exception("PathReplace must contain one \":\".");
                }
                var search = tmp[0];
                var replace = tmp[1];


                ruleSet.SetRule(s, (NifFileWrapper.NiShapeWrapper shape) => {
                    var modified = false;
                    for(int i = 0; i < 8; ++i) {
                        var t = shape.GetTextureSlot((TextureId)i);
                        var newT = t.Replace(search, replace);
                        if(t != newT) {
                            modified = true;
                            shape.SetTextureSlot((TextureId)i, newT);
                        }
                    }
                    return modified;
                });
            });

            // rule condition

            AddParseRule("Diffuse", (string s, RuleSet ruleSet) => {
                var tmp = ruleSet.Clone();
                if(!RuleSet.diffuseRules.ContainsKey(s)) {
                    RuleSet.diffuseRules[s] = new List<RuleSet>();
                }
                RuleSet.diffuseRules[s].Add(tmp);
            });


            AddParseRule("PathFilter", (string s, RuleSet ruleSet) => {
                ruleSet.pathFilter = s;
            });


        }

        public static void ParseRuleBlock(string[] lines) {
            var ruleSet = new RuleSet();
            for(var i = 0; i < lines.Length; ++i) {
                var line = lines[i];
                if(line.Trim() == "") {
                    continue;
                }
                var split = line.ToLower().Split("=");
                if(split.Length != 2) {
                    throw new RuleParseException(i, "Cannot parse line (Syntax Error - Must contain 1 \"=\" sign): " + line);
                }
                var key = split[0].Trim();
                var value = split[1].Trim();
                if(!parseRules.ContainsKey(key)) {
                    throw new RuleParseException(i, "Cannot parse line (Key Error - Unkown Key): " + key);
                }
                if(value != "#no_change") {
                    currentKey = key;
                    parseRules[key](value, ruleSet);
                } else {
                    ruleSet.rules.Remove(key);
                    ruleSet.rulesOrder.Remove(key);
                }

            }
        }

        public static bool PatchNif(NifFileWrapper nif, string relativePath) {
            var modified = false;
            for(var i = 0; i < nif.GetNumShapes(); ++i) {
                var shape = nif.GetShape(i);
                var diffuse = shape.DiffuseMap.ToLower();
                if(RuleSet.diffuseRules.ContainsKey(diffuse)) {
                    var rules = RuleSet.diffuseRules[diffuse];
                    foreach(var rule in rules) {
                        if(rule.pathFilter == "" || relativePath.Split("\\").Contains(rule.pathFilter)) {
                            var tmp = rule.Apply(shape);
                            modified = tmp || modified;
                        }
                    }
                }
            }
            return modified;
        }

        private static void AddParseRule(string key, Action<string, RuleSet> action) {
            parseRules.Add(key.ToLower(), action);
        }

        private static void AddParseRule(string key1, string key2, Action<string, RuleSet> action) {
            parseRules.Add(key1.ToLower(), action);
            parseRules.Add(key2.ToLower(), action);
        }

        private static void AddParseRule(string key1, string key2, string key3, Action<string, RuleSet> action) {
            parseRules.Add(key1.ToLower(), action);
            parseRules.Add(key2.ToLower(), action);
            parseRules.Add(key3.ToLower(), action);
        }

        private static void AddParseRule(string key1, string key2, string key3, string key4, Action<string, RuleSet> action) {
            parseRules.Add(key1.ToLower(), action);
            parseRules.Add(key2.ToLower(), action);
            parseRules.Add(key3.ToLower(), action);
            parseRules.Add(key4.ToLower(), action);
        }
    }
}
