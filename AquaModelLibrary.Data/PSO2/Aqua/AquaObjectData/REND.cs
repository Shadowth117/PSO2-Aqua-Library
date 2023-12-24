using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    public struct REND
    {
        public int tag { get; set; }      //0x40, type 0x9 //Always 0x1FF
        public int unk0 { get; set; }     //0x41, type 0x9 //3 usually
        public int twosided { get; set; }  //0x42, type 0x9 //0 for backface cull, 1 for twosided, 2 used in persona live dance models for unknown purposes (backface only?)
        public int int_0C { get; set; }  //0x43, type 0x9 //Maybe related to blend sort order? I'm not sure...

        //Next 12 values appear related, maybe to some texture setting? There are 3 sets that start with 5, first two go to 6, all go to 1, thhen 4th is typically different.
        public int sourceAlpha { get; set; }  //0x44, type 0x9 //5 usually
        public int destinationAlpha { get; set; }  //0x45, type 0x9 //6 usually
        public int unk3 { get; set; }  //0x46, type 0x9 //1 usually
        public int unk4 { get; set; }  //0x47, type 0x9 //0 usually

        public int unk5 { get; set; }  //0x48, type 0x9 //5 usually
        public int unk6 { get; set; }  //0x49, type 0x9 //6 usually
        public int unk7 { get; set; }  //0x4A, type 0x9 //1 usually. Another alpha setting, perhaps for multi/_s map?
        public int unk8 { get; set; }  //0x4B, type 0x9 //1 usually.

        public int unk9 { get; set; }   //0x4C, type 0x9 //5 usually
        public int alphaCutoff { get; set; }  //0x4D, type 0x9 //0-256, (Assumedly value of alpha at which a pixel is rendered invisible vs fully visible)
        public int unk11 { get; set; }  //0x4E, type 0x9 //1 usually
        public int unk12 { get; set; }  //0x4F, type 0x9 //4 usually

        public int unk13 { get; set; }  //0x50, type 0x9 //1 usually

        public REND(Dictionary<int, object> rendRaw)
        {
            tag = (int)rendRaw[0x40];
            unk0 = (int)rendRaw[0x41];
            twosided = (int)rendRaw[0x42];
            int_0C = (int)rendRaw[0x43];

            sourceAlpha = (int)rendRaw[0x44];
            destinationAlpha = (int)rendRaw[0x45];
            unk3 = (int)rendRaw[0x46];
            unk4 = (int)rendRaw[0x47];

            unk5 = (int)rendRaw[0x48];
            unk6 = (int)rendRaw[0x49];
            unk7 = (int)rendRaw[0x4A];
            unk8 = (int)rendRaw[0x4B];

            unk9 = (int)rendRaw[0x4C];
            alphaCutoff = (int)rendRaw[0x4D];
            unk11 = (int)rendRaw[0x4E];
            unk12 = (int)rendRaw[0x4F];

            unk13 = (int)rendRaw[0x50];
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals((REND)obj);
        }

        public bool Equals(REND c)
        {

            // Optimization for a common success case.
            if (ReferenceEquals(this, c))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (GetType() != c.GetType())
            {
                return false;
            }

            return tag == c.tag && unk0 == c.unk0 && twosided == c.twosided && int_0C == c.int_0C && sourceAlpha == c.sourceAlpha && destinationAlpha == c.destinationAlpha && unk3 == c.unk3
                && unk4 == c.unk4 && unk5 == c.unk5 && unk6 == c.unk6 && unk7 == c.unk7 && unk8 == c.unk8 && unk9 == c.unk9 && alphaCutoff == c.alphaCutoff && unk11 == c.unk11
                && unk12 == c.unk12 && unk13 == c.unk13;
        }

        public static bool operator ==(REND lhs, REND rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(REND lhs, REND rhs) => !(lhs == rhs);
    }
}
