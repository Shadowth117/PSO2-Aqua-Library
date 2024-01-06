//Borrowed from Aqp2obj (Name is a misnomer, program did partial conversions both ways via use of intermediary files)

using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.LegacyObj
{
    public class MtlFile
    {
        public string FileName { get; private set; }
        public List<MtlMaterial> Materials { get; private set; }
        public MtlMaterial CurrentMaterial { get; private set; }

        public MtlFile()
        {
            Materials = new List<MtlMaterial>();
        }

        public MtlMaterial Get(string name)
        {
            return Materials.FirstOrDefault(i => i.Name == name);
        }

        public static MtlFile FromFile(string file)
        {
            using (var r = new StreamReader(file, Encoding.Default))
                return FromReader(r);
        }

        public static MtlFile FromReader(StreamReader r)
        {
            var mtl = new MtlFile();
            mtl.Load(r);
            return mtl;
        }

        private void Load(StreamReader r)
        {
            if (r.BaseStream is FileStream)
                FileName = ((FileStream)r.BaseStream).Name;

            var delim = " \t".ToArray();

            for (; ; )
            {
                var line = r.ReadLine();

                if (null == line)
                    break;

                line = line.Trim();

                if (line.Length == 0)
                    continue;

                if (line[0] == '#')
                    continue;

                LineReaded(line.Split(delim, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private void LineReaded(string[] t)
        {
            switch (t[0].ToLower())
            {
                case "newmtl": NewMaterial(t[1]); break;
                case "ns": GetCurrentMaterial().Ns = float.Parse(t[1]); break;
                case "ni": GetCurrentMaterial().Ni = float.Parse(t[1]); break;
                case "d": GetCurrentMaterial().D = float.Parse(t[1]); break;
                case "tr": GetCurrentMaterial().Tr = float.Parse(t[1]); break;
                case "tf": GetCurrentMaterial().Tf = ParseVector3(t); break;
                case "illum": GetCurrentMaterial().Illum = float.Parse(t[1]); break;
                case "ka": GetCurrentMaterial().Ka = ParseVector3(t); break;
                case "kd": GetCurrentMaterial().Kd = ParseVector3(t); break;
                case "ks": GetCurrentMaterial().Ks = ParseVector3(t); break;
                case "ke": GetCurrentMaterial().Ke = ParseVector3(t); break;
                case "map_ka": GetCurrentMaterial().MapKa = t[1]; break;
                case "map_kd": GetCurrentMaterial().MapKd = t[1]; break;
                case "map_ks": GetCurrentMaterial().MapKs = t[1]; break;
                case "map_ke": GetCurrentMaterial().MapKe = t[1]; break;
                case "map_bump": GetCurrentMaterial().MapBump = t[1]; break;
                default: throw new FormatException();
            }
        }

        private Vector3 ParseVector3(string[] t)
        {
            try
            {
                return new Vector3(float.Parse(t[1]), float.Parse(t[2]), float.Parse(t[3]));
            }
            catch
            {
                return new Vector3(0, 0, 0);
            }
        }

        private MtlMaterial GetCurrentMaterial()
        {
            return null == CurrentMaterial ? NewMaterial("noname") : CurrentMaterial;
        }

        private MtlMaterial NewMaterial(string name)
        {
            Materials.Add(CurrentMaterial = new MtlMaterial(this, Materials.Count) { Name = name });
            return CurrentMaterial;
        }
    }

    public class MtlMaterial
    {
        public MtlFile Owner { get; private set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public float Ns { get; set; }
        public float Ni { get; set; }
        public float D { get; set; }
        public float Tr { get; set; }
        public Vector3 Tf { get; set; }
        public float Illum { get; set; }
        public Vector3 Ka { get; set; }
        public Vector3 Kd { get; set; }
        public Vector3 Ks { get; set; }
        public Vector3 Ke { get; set; }
        public string MapKa { get; set; }
        public string MapKd { get; set; }
        public string MapKs { get; set; }
        public string MapKe { get; set; }
        public string MapBump { get; set; }

        public MtlMaterial(MtlFile owner, int index)
        {
            Owner = owner;
            Index = index;
        }
    }
}
