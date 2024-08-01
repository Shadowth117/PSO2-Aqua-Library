using static AquaModelLibrary.Data.PSO2.Constants.CharacterMakingDynamic;

namespace AquaModelLibrary.Data.PSO2.Constants
{
    public static class CharacterAnimationNames
    {
        public static string loadDollAnims = characterStart + "apc_loaddoll_citizen.ice";
        public static string npcAnims = characterStart + "np_npc_object.ice";
        public static string supportPartnerAnims = characterStart + "np_support_partner.ice";
        public static string npcDelicious = characterStart + "npc_delicious.ice";
        public static string tpdAnims = characterStart + "pl_bodel.ice";
        public static string plLightLooks = characterStart + "pl_light_looks_basnet.ice";
        public static string laconiumAnims = characterStart + "pl_object_rgrs.ice";
        public static string playerRideRoidAnims = characterStart + "pl_object_rideroid.ice";
        public static string dashPanelAnims = characterStart + "pl_object_dashpanel.ice";
        public static string monHunAnim = characterStart + "pl_volcano.ice";
        public static string monHunCarve = characterStart + "pl_volcano_pickup.ice";
        public static string npCommonHumanReboot = characterStart + "np_common_human_reboot.ice";
        public static string rebootNpcCommonHuman = characterStart + "reboot_npc_common_human.ice";
        public static string npRebootMiniRobo = characterStart + "np_reboot_minirobo.ice";
        public static string npRegion111 = characterStart + "np_reboot_region_111.ice";
        public static string npRegion112 = characterStart + "np_reboot_region_112.ice";
        public static string npRegion113 = characterStart + "np_reboot_region_113.ice";
        public static string npRegion114 = characterStart + "np_reboot_region_114.ice";
        public static string plExtraMagAccess = characterStart + "pl_extra_mag_access.ice";
        public static string plRebootMyspace = characterStart + "pl_reboot_myspace_";
        public static string plRebootObject = characterStart + "pl_reboot_object_";

        public static List<string> chrMySpaceList = new List<string>() { "bathtub", "bed01", "deckchair", "saboten", "shower", "sit_point" };
        public static List<string> chrobjectList = new List<string>() { "dashpanel", "gatlinggun", "halloween", "jumppad", "multimissile", "snowboard", "throw", "throw_movement", "sit_point" };

        public static List<string> wepTypeList = new List<string>() { "compoundbow", "doublesaber", "dualblade", "gunslash", "jetboots", "katana", "knuckle", "launcher", "master_doublesaber",
            "master_dualblade", "master_wand", "partisan", "poka_compoundbow", "rifle", "rod", "slayer_gunslash", "sword", "takt", "talis", "twindagger", "twinsubmachinegun",
            "unarmed", "villain_katana", "villain_rifle", "villain_rod", "wand", "wiredlance", "wpnman_sword", "wpnman_talis", "wpnman_twinsubmachinegun"};

        public static List<string> pvpWepList = new List<string>() { "compoundbow", "doublesaber", "dualblade", "gunslash", "jetboots", "katana", "knuckle", "launcher", "partisan", "rifle", "rod",
                "unarmed", "wand", "wiredlance"};

        public static List<string> wepTypeListNGS = new List<string>() { "compoundbow", "doublesaber", "dualblade", "gunslash", "jetboots", "katana", "knuckle", "launcher",
                "partisan", "rifle", "rod", "sword", "takt", "talis", "twindagger", "twinsubmachinegun",
            "unarmed", "wand", "wiredlance", "reboot_sp_arsenalmodule"};
    }
}
