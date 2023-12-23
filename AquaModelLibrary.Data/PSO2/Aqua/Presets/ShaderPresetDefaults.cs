namespace AquaModelLibrary.Data.PSO2.Aqua.Presets
{
    public class ShaderPresetDefaults
    {
        public static Dictionary<string, string> texNamePresetPatterns = new Dictionary<string, string>()
        {
            { "_d.dds", "d"},
            { "_d_", "d"},
            { "_diffuse", "d"},

            { "_s.dds", "s"},
            { "_s_", "s"},
            { "multi", "s"},
            { "_specular", "s"},

            { "_m.dds", "m"},
            { "_m_", "m"},

            { "_n.dds", "n"},
            { "_n_", "n"},
            { "_normal", "n"},

            { "_t.dds", "t"},
            { "_t_", "t"},

            { "_k.dds", "k"},
            { "_k_", "k"},

            { "_p.dds", "p"},
            { "_p_", "p"},

            { "_a.dds", "a"},
            { "_a_", "a"},

            { "_b.dds", "b"},
            { "_b_", "b"},

            { "_c.dds", "c"},
            { "_c_", "c"},

            { "_e.dds", "e"},
            { "_e_", "e"},
            { "_env", "e"},
            { "sqenv.dds", "e"},
            { "dfenv.dds", "e"},

            { "_f.dds", "f"},
            { "_f_", "f"},

            { "_g.dds", "g"},
            { "_g_", "g"},

            { "_r.dds", "r"},
            { "_r_", "r"},

            { "_decal", "decal"},

            { "_noise", "noise"},

            { "subnormal_01", "subnormal_01"},
            { "subnormal_02", "subnormal_02"},
            { "subnormal_03", "subnormal_03"},
            { "_mask", "mask"},
        };

        public static Dictionary<string, List<string>> texNamePresetPatternsReverse = new Dictionary<string, List<string>>()
        {
            {"d", new List<string>() { "_d.dds", "_d_", "_diffuse" } },
            {"s", new List<string>() { "_s.dds", "_s_", "multi" } },
            {"m", new List<string>() { "_m.dds", "_m_" } },
            {"n", new List<string>() { "_n.dds", "_n_", "normal" } },
            {"t", new List<string>() { "_t.dds", "_t_" } },
            {"k", new List<string>() { "_k.dds", "_k_" } },
            {"p", new List<string>() { "_p.dds", "_p_" } },
            {"a", new List<string>() { "_a.dds", "_a_" } },
            {"b", new List<string>() { "_b.dds", "_b_" } },
            {"c", new List<string>() { "_c.dds", "_c_" } },
            {"e", new List<string>() { "_e.dds", "_e_", "_env", "sqenv.dds", "dfenv.dds" } },
            {"f", new List<string>() { "_f.dds", "_f_" } },
            {"g", new List<string>() { "_g.dds", "_g_" } },
            {"r", new List<string>() { "_r.dds", "_r_" } },
            {"decal", new List<string>() { "_decal" } },
            {"noise", new List<string>() { "_noise" } },
            {"subnormal_01", new List<string>() { "_subnormal_01" } },
            {"subnormal_02", new List<string>() { "_subnormal_02" } },
            {"subnormal_03", new List<string>() { "_subnormal_03" } },
            {"mask", new List<string>() { "_mask" } },
        };

    }
}
