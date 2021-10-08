using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaExtras
{
    public static class FilenameConstants
    {
        public static string weaponDir = "item/weapon/";
        public static string baseWeaponString = "it_wp_00_base_";
        public static string weaponString = "it_wp_";
        public static string defaultWeaponString = "it_wp_xxx_";
        //Types for weapon default files (fig, default anims)
        public static List<string> weaponTypes = new List<string>()
        {
            "sword",            //1
            "wiredlance",       //2
            "partisan",         //3
            "twindagger",       //4
            "doublesaber",      //5
            "knuckle",          //6
            "gunslash",         //7
            "assaultrifle",     //8
            "grenadelauncher",  //9
            "twinmachinegun",   //10
            "rod",              //11
            "thalys",           //12
            "wand",             //13
            "ktn",              //14
            "bow",              //15
            "jetboots",         //16
            "dualblade",        //17
            "tact"              //18
        };

        public static List<string> weaponTypesNGS = new List<string>()
        {
            "sword",            //1
            "wiredlance",       //2
            "partisan",         //3
            "twindagger",       //4
            "doublesaber",      //5
            "knuckle",          //6
            "gunslash",         //7
            "rifle",            //8
            "launcher",         //9
            "twinmachinegun",   //10
            "rod",              //11
            "thalys",           //12
            "wand",             //13
            "ktn",              //14
            "bow",              //15
            "jetboots",         //16
            "dualblade",        //17
            "tact"              //18
        };

        //Types for default weapon model files? Dual blade seemingly doesn't have one...
        public static List<string> weaponTypesShort = new List<string>()
        {
            "sw",            //1
            "ln",            //2
            "pa",            //3
            "td",            //4
            "ds",            //5
            "kn",            //6
            "gs",            //7
            "rf",            //8
            "lu",            //9
            "tm",            //10
            "rd",            //11
            "ta",            //12
            "wa",            //13
            "kt",            //14
            "cb",            //15
            "jb",            //16
            null,//"dualblade",     //17 not used?
            "tc"             //18
        };

        public static Dictionary<int, string> swordNames = new Dictionary<int, string>();
        public static Dictionary<int, string> wiredLanceNames = new Dictionary<int, string>();
        public static Dictionary<int, string> partizanNames = new Dictionary<int, string>();
        public static Dictionary<int, string> twinDaggerNames = new Dictionary<int, string>();
        public static Dictionary<int, string> doubleSaberNames = new Dictionary<int, string>();
        public static Dictionary<int, string> knucklesNames = new Dictionary<int, string>();
        public static Dictionary<int, string> gunslashNames = new Dictionary<int, string>();
        public static Dictionary<int, string> rifleNames = new Dictionary<int, string>();
        public static Dictionary<int, string> launcherNames = new Dictionary<int, string>();
        public static Dictionary<int, string> tmgNames = new Dictionary<int, string>();
        public static Dictionary<int, string> rodNames = new Dictionary<int, string>();
        public static Dictionary<int, string> talysNames = new Dictionary<int, string>();
        public static Dictionary<int, string> wandNames = new Dictionary<int, string>();
        public static Dictionary<int, string> katanaNames = new Dictionary<int, string>();
        public static Dictionary<int, string> bowNames = new Dictionary<int, string>();
        public static Dictionary<int, string> jetBootsNames = new Dictionary<int, string>();
        public static Dictionary<int, string> dualBladesNames = new Dictionary<int, string>();
        public static Dictionary<int, string> tactNames = new Dictionary<int, string>();


        public static Dictionary<int, string> swordNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> wiredLanceNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> partizanNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> twinDaggerNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> doubleSaberNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> knucklesNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> gunslashNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> rifleNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> launcherNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> tmgNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> rodNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> talysNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> wandNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> katanaNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> bowNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> jetBootsNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> dualBladesNGSNames = new Dictionary<int, string>();
        public static Dictionary<int, string> tactNGSNames = new Dictionary<int, string>();
        
        public static List<string> csvFilenames = new List<string>() 
        { 
            "SwordNames.csv",
            "WiredLanceNames.csv",
            "PartizanNames.csv",
            "TwinDaggerNames.csv",
            "DoubleSaberNames.csv",
            "KnucklesNames.csv",
            "GunslashNames.csv",
            "RifleNames.csv",
            "LauncherNames.csv",
            "TwinMachineGunNames.csv",
            "RodNames.csv",
            "TalisNames.csv",
            "WandNames.csv",
            "KatanaNames.csv",
            "BowNames.csv",
            "JetBootsNames.csv",
            "DualBladesNames.csv",
            "TactNames.csv",

            "SwordNGSNames.csv",
            "WiredLanceNGSNames.csv",
            "PartizanNGSNames.csv",
            "TwinDaggerNGSNames.csv",
            "DoubleSaberNGSNames.csv",
            "KnucklesNGSNames.csv",
            "GunslashNGSNames.csv",
            "RifleNGSNames.csv",
            "LauncherNGSNames.csv",
            "TwinMachineGunNGSNames.csv",
            "RodNGSNames.csv",
            "TalisNGSNames.csv",
            "WandNGSNames.csv",
            "KatanaNGSNames.csv",
            "BowNGSNames.csv",
            "JetBootsNGSNames.csv",
            "DualBladesNGSNames.csv",
            "TactNGSNames.csv",
        };
    }
}
