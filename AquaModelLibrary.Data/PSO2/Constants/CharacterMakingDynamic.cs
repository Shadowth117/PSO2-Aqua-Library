namespace AquaModelLibrary.Data.PSO2.Constants
{
    public static class CharacterMakingDynamic
    {
        public static string classicStart = $"character/making/pl_";
        public static string rebootStart = $"character/making_reboot/pl_";
        public static string rebootExStart = $"character/making_reboot_ex/pl_";
        public static string playerVoiceStart = $"player_voice/";
        public static string icon = $"large_icon/ui_making_";
        public static string lobbyActionStart = $"actor/lobby_action/";
        public static string characterStart = $"character/";
        public static string pvpStart = $"character_poka/";
        public static string lobbyActionStartReboot = $"character/motion/lobby_action/";
        public static string substituteMotion = $"character/motion/substitute_motion/pl_sb_";
        public static string magItem = $"item/mag/it_mg_";

        public static string basewearIcon = $"basewear01_";
        public static string bodyPaintIcon = $"bodypaint01_";
        public static string stickerIcon = $"bodypaint02_";
        public static string costumeIcon = $"costume01_";
        public static string castPartIcon = $"costume02_"; //No extra string for body. Best to check if both costume01 and costume02 exist for costumes since they're not
                                                           //always differentiated well (cast only costumes etc.)
        public static string castArmIcon = $"arm_";
        public static string castLegIcon = $"leg_";
        public static string accessoryIcon = $"decoy01_";
        public static string earIcon = $"ears01_";
        public static string eyeIcon = $"eye01_";
        public static string eyeBrowsIcon = $"eyebrows01_";
        public static string eyelashesIcon = $"eyelashes01_";
        public static string faceIcon = $"face01_";             //aka facepaint01
        public static string facePaintIcon = $"facepaint02_";
        public static string hairIcon = $"hair01_";
        public static string hornIcon = $"horn01_";
        public static string innerwearIcon = $"innerwear01_";
        public static string outerwearIcon = $"outerwear01_";
        public static string teethIcon = $"dental01_";
        public static string skinIcon = $"skin01_";

        public static string iconCast = "cast_"; //Only used for hair? Idk
        public static string iconMale = "man_";
        public static string iconFemale = "woman_";

        public static string voiceCman = "11_voice_cman";
        public static string voiceCwoman = "11_voice_cwoman";
        public static string voiceMan = "11_voice_man";
        public static string voiceWoman = "11_voice_woman";

        public static string rebootHornDataStr = "pl_rhn_";
        public static string rebootTeethDataStr = "pl_rdt_";
        public static string rebootEarDataStr = "pl_rea_";

        public static string rebootLAHuman = "_std";
        public static string rebootLACastMale = "_cam";
        public static string rebootLACastFemale = "_caf";
        public static string rebootFig = "_base";

        public static string subSwim = "swim_";
        public static string subGlide = "glide_";
        public static string subJump = "jump_";
        public static string subLanding = "landing_";
        public static string subMove = "mov_";
        public static string subSprint = "sprint_";
        public static string subIdle = "idle_";
    }
}
