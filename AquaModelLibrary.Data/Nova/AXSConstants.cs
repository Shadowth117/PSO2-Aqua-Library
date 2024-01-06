namespace AquaModelLibrary.Data.Nova
{
    public static class AXSConstants
    {
        public const int FSA = 0x41534620; //Contains bones, materials, and maybe defines vertex data layout
        public const int FAA = 0x41414620; //Contains Animation data
        public const int FIA = 0x41494620; //Contains images
        public const int FRa = 0x61524620; //Unknown struct. Contains a large block of encrypted or hashed data.
        public const int daeh = 0x68656164; //Head. Appears before major structs.
        public const int __lm = 0x6D6C5F5F; //Material List Contains stam substructs
        public const int stam = 0x6D617473; //Materials

        public const int __oa = 0x616F5F5F; //Contains mesh and image data
        public const int __bm = 0x6D625F5F; //Contains mesh data
        public const int ydbm = 0x6D626479; //mesh in a __bm container
        public const int lxdi = 0x6964786C; //Contains face md5 references
        public const int salv = 0x766C6173; //Contains vertex definitions and md5 references
        public const int ssem = 0x6D657373; //Precedes material data. Per mesh material colors?
        public const int xenr = 0x726E6578; //Contains texture/material references. Also contains texture setting data?
        public const int Xgmi = 0x696D6758; //Image data header
        public const int ipnb = 0x626E7069; //Local bone list per mesh. Indices in list are references to the bone at that index in the global bone list
        public const int lpnb = 0x626E706C; //Global bone list

        public const int eert = 0x74726565; //Contains model nodes
        public const int rtta = 0x61747472; //Node struct
        public const int ltxe = 0x6578746C; //At end of eert
        public const int _foe = 0x656F665F; //At end of eert

        public const int FMA = 0x414D4620; //Geometry data struct
        public const int ffub = 0x62756666; //Buffer. Stores bounds of vertex or face data
        public const int rdda = 0x61646472; //Address. Stores bounds of vertex or face data per mesh

        public const int animMagic = 0x0002002D;
    }
}
