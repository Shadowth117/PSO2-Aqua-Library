using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class CharacterMakingIndex : AquaCommon
    {
        public static string dataDir = $"data\\win32\\";
        public static string dataNADir = $"data\\win32_na\\";
        public static string dataReboot = $"data\\win32reboot\\";
        public static string dataRebootNA = $"data\\win32reboot_na\\";
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
        public static string eyebrowsIcon = $"eyebrows01_";
        public static string eyelashesIcon = $"eyelashes01_";
        public static string faceIcon = $"face01_";             //aka facepaint01
        public static string facepainticon = $"facepaint02_";
        public static string hairIcon = $"hair01_";
        public static string hornIcon = $"horn01_";
        public static string innerwearIcon = $"innerwear01_";
        public static string outerwearIcon = $"outerwear01_";
        public static string teethIcon = $"dental01_";

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

        public static string classicCMX = $"character/making/pl_system.ice";
        public static string classicPartText = $"ui_charamake_text.ice";
        public static string classicAcceText = $"ui_fashion_text.ice";
        public static string classicCharCreate = $"ui_character_making.ice";
        public static string classicCommon = $"sy_common_text.ice";
        public static string classicLobbyAction = $"actor/lobby_action/pl_lobby_action_setting.ice";
        public static string rebootLobbyAction = $"f94e8bfb6ee674e39fa6bc1aa697bf82";
        public static string magSetting = "item/mag/it_mg_setting.ice";

        public static string partsTextName = "ui_charamake_parts.text";
        public static string acceTextName = "ui_accessories_text.text";
        public static string commonTextName = "common.text";
        public static string faceVarName = "face_variation.cmp.lua";
        public static string lacName = "lobby_action_setting.lac";
        public static string mgxName = "setting.mgx";

        public static string pbCreatures = "photon_blast/photon_blast_";
        public static string db_vehicle = "vehicle/";

        public static List<string> playerEffects =  new List<string>()
            {
                "actor/shot/effect_2dg.ice",
                "actor/shot/effect_2sm.ice",
                "actor/shot/effect_arks_buff.ice",
                "actor/shot/effect_aslt.ice",
                "actor/shot/effect_boots.ice",
                "actor/shot/effect_cmpdbow.ice",
                "actor/shot/effect_cs.ice",
                "actor/shot/effect_dbld.ice",
                "actor/shot/effect_dsa.ice",
                "actor/shot/effect_en_sprdbm.ice",
                "actor/shot/effect_ene_sld.ice",
                "actor/shot/effect_gslh.ice",
                "actor/shot/effect_hr2sm.ice",
                "actor/shot/effect_hrswd.ice",
                "actor/shot/effect_hrthlys.ice",
                "actor/shot/effect_knu.ice",
                "actor/shot/effect_ktn.ice",
                "actor/shot/effect_lnch.ice",
                "actor/shot/effect_mag.ice",
                "actor/shot/effect_mrg_buff.ice",
                "actor/shot/effect_mstdbld.ice",
                "actor/shot/effect_mstdsa.ice",
                "actor/shot/effect_mstwnd.ice",
                "actor/shot/effect_mti_buff.ice",
                "actor/shot/effect_parti.ice",
                "actor/shot/effect_sc_success_buff.ice",
                "actor/shot/effect_slagsl.ice",
                "actor/shot/effect_sum.ice",
                "actor/shot/effect_swd.ice",
                "actor/shot/effect_tec.ice",
                "actor/shot/effect_thlys.ice",
                "actor/shot/effect_vlnaslt.ice",
                "actor/shot/effect_vlnktn.ice",
                "actor/shot/effect_vlnrod.ice",
                "actor/shot/effect_wlce.ice",
                "actor/shot/effect_wnd.ice",
                "actor/shot/effect_wp_pot_02_242.ice",
                "actor/shot/effect_wp_pot_06_202.ice",
                "actor/shot/effect_wp_pot_07_091.ice",
                "actor/shot/effect_wp_pot_09_bnk.ice",
                "actor/shot/effect_wp_pot_13_290.ice",
                "actor/shot/effect_wp_pot_14_264.ice",
                "actor/shot/effect_wp_pot_15_059.ice",
                "actor/shot/effect_wp_pot_16_128.ice",
                "actor/shot/effect_wp_pot_absh.ice",
                "actor/shot/effect_wp_pot_ace.ice",
                "actor/shot/effect_wp_pot_ang.ice",
                "actor/shot/effect_wp_pot_arles.ice",
                "actor/shot/effect_wp_pot_aut_recv.ice",
                "actor/shot/effect_wp_pot_autos.ice",
                "actor/shot/effect_wp_pot_barrier.ice",
                "actor/shot/effect_wp_pot_berserk.ice",
                "actor/shot/effect_wp_pot_bldr.ice",
                "actor/shot/effect_wp_pot_bone.ice",
                "actor/shot/effect_wp_pot_chskip.ice",
                "actor/shot/effect_wp_pot_com_plybuff.ice",
                "actor/shot/effect_wp_pot_crystal.ice",
                "actor/shot/effect_wp_pot_dest.ice",
                "actor/shot/effect_wp_pot_dmgst.ice",
                "actor/shot/effect_wp_pot_elebo.ice",
                "actor/shot/effect_wp_pot_elebo2.ice",
                "actor/shot/effect_wp_pot_elm_black.ice",
                "actor/shot/effect_wp_pot_elm_blue.ice",
                "actor/shot/effect_wp_pot_elm_green.ice",
                "actor/shot/effect_wp_pot_elm_red.ice",
                "actor/shot/effect_wp_pot_elm_white.ice",
                "actor/shot/effect_wp_pot_elm_yellow.ice",
                "actor/shot/effect_wp_pot_endstrong.ice",
                "actor/shot/effect_wp_pot_expwep.ice",
                "actor/shot/effect_wp_pot_gearguard_c.ice",
                "actor/shot/effect_wp_pot_homilan.ice",
                "actor/shot/effect_wp_pot_lastbl.ice",
                "actor/shot/effect_wp_pot_med.ice",
                "actor/shot/effect_wp_pot_mira_mnb.ice",
                "actor/shot/effect_wp_pot_mrroid.ice",
                "actor/shot/effect_wp_pot_nodb.ice",
                "actor/shot/effect_wp_pot_non_pbld.ice",
                "actor/shot/effect_wp_pot_phobos.ice",
                "actor/shot/effect_wp_pot_ppfever.ice",
                "actor/shot/effect_wp_pot_preh.ice",
                "actor/shot/effect_wp_pot_preh2.ice",
                "actor/shot/effect_wp_pot_prfdtlck.ice",
                "actor/shot/effect_wp_pot_prod.ice",
                "actor/shot/effect_wp_pot_rdbd.ice",
                "actor/shot/effect_wp_pot_reng.ice",
                "actor/shot/effect_wp_pot_revgift.ice",
                "actor/shot/effect_wp_pot_saberstyle.ice",
                "actor/shot/effect_wp_pot_sbeam.ice",
                "actor/shot/effect_wp_pot_shooting_s.ice",
                "actor/shot/effect_wp_pot_shwtim_onetim.ice",
                "actor/shot/effect_wp_pot_spinfe.ice",
                "actor/shot/effect_wp_pot_stedb.ice",
                "actor/shot/effect_wp_pot_thdmas.ice",
                "actor/shot/effect_wp_pot_time_chan.ice",
                "actor/shot/effect_wp_pot_turt.ice",
                "actor/shot/effect_wp_pot_union.ice",
                "actor/shot/effect_wp_pot_vintrange.ice",
                "actor/shot/effect_wp_pot_vrefine.ice",
                "actor/shot/effect_wp_pot_weekhit.ice",
                "actor/shot/effect_wp_pot_whitig.ice",
                "actor/shot/effect_wp_pot_wideshot.ice",
                "actor/shot/effect_wp_pot_wlance_high.ice",
                "actor/shot/effect_wp_pot_zerodstrong.ice"
            };

        public static Dictionary<int, string> magNames = new Dictionary<int, string>()
        {
            { 1, "???,Alternate Mag?" },
            { 2, "マグ,Mag" },
            { 3, "ライラ,Lyra" },
            { 4, "フォルナクス,Fornax" },
            { 5, "レプス,Lepus"},
            { 6, "アントリア,Antlia"},
            { 7, "ライブラ,Libra"},
            { 8, "ドルフィヌス,Delphinus"},
            { 9, "キグナス,Cygnus"},
            { 10, "カエルム,Caelum"},
            { 11, "ケフェウス,Cepheus"},
            { 12, "ツカナ,Tsukana"},
            { 13, "カリーナ,Carina"},
            { 14, "モノケロス,Monoceros"},
            { 15, "オリオン,Orion"},
            { 16, "アプス,Apus"},
            { 17, "コルブス,Corvus"},
            { 18, "レオ,Leo"},
            { 19, "クルックス,Crux"},
            { 20, "リンクス,Lynx"},
            { 21, "ドラコ,Draco"},
            { 22, "シャト,Sato"},
            { 23, "ラッピー,Rappy"},
            { 24, "ジャック,Jack"},
            { 25, "ピート,Pete"},
            { 26, "キャンサー,Cancer"},
            { 27, "アクベンス,Acubens"},
            { 28, "アセルス,Asers"},
            { 29, "アルタルフ,Altarf"},
            { 30, "キトゥン,Kitten"},
            { 31, "カッツェ,Katze"},
            { 32, "ミーツェ,Miishe"},
            { 33, "クラーリタ,Clarita"},
            { 34, "ナナハナ,nanahana"},
            { 35, "アークマ,Arkuma"},
            { 36, "アークマミ,Arkumami"},
            { 37, "アンブラ,Umbra"},
            { 38, "トロ,Toro"},
            { 39, "クロ,Kuro"},
            { 40, "ハンプティ,Humpty"},
            { 41, "チャムライ,Chamurai"},
            { 42, "チャーク,Chark"},
            { 43, "リリーパ,Lillipan"},
            { 44, "ゴストン,Ghoston"},
            { 45, "ソニチ,Sonichi"},
            { 46, "プーシャン,Pushan"},
            { 47, "ミクダヨー,Mikudayo-"},
            { 48, "緑ぷよ,Green Puyo"},
            { 49, "カーバンクル,Carbuncle"},
            { 50, "キュゥべえ,Kyubey"},
            { 51, "ポリタン,Polytan"},
            { 52, "カガミミ,Kagamimi"},
            { 53, "ラブラッピー,Love Rappy"},
            { 54, "ラヴィラッピー,Lovey Rappy"},
            { 55, "キャリブー,Caribou" },

            { 56, ",Unidentified Black Bear"},

            { 57, "イーツター,Eatster (Intended spelling)"},
            { 58, "ミャリッジ,Wedding Bell"},
            { 59, "ベレイ,Berei"},
            { 60, "ベレイMk2,Berei Mk2"},
            { 61, "エトワル,étoile"},
            { 62, "月見ラッピー,Moon-Viewing Rappy"},
            { 63, "ソニック人形, Sonic Doll"},
            { 64, "モロ星人,Moro Alien"},
            { 65, "モロ星人B,Moro Alien B"},
            { 66, "ナイトギア,Nightgear"},
            { 67, "ニャウ,Nyau"},
            { 68, "ジャクラン,Jack Lan"},
            { 69, "テルクゥ,Teruku"},
            { 70, "キリゾー,Girizoo"},
            { 71, "SFC妖精,SFC Fairy"},
            { 72, "赤ぷよ,Red Puyo"},
            { 73, "ダンボー,Danboard"},
            { 74, "カラッピー,Karappy"},
            { 75, "タマヒメ,Tamahime"},
            { 76, "お団子ヘア,Dumpling Hair"},
            { 77, "ラブリーパ,LoveLipa"},
            { 78, "ラヴィリー,LoveLi"},
            { 79, "ドリキャス,Dreamcast"},
            { 80, "セガサターン,Sega Saturn"},
            { 81, "メガドラ,Mega Drive"},
            { 82, "アナライザー,Analyzer"},
            { 83, "ホヌホヌ,Honuhonu" },
            { 84, "小梅,Xiaomei"},
            { 85, "ニャンドリオン（ニャンD）,Nyandorion (NyanD)"},
            { 86, "A.I.S,A.I.S"},

            { 88, "ジーツター,Jiitsuter"},
            { 89, "レイニャン,Rainyan"},
            { 90, "ルクミン,Rukmin"},
            { 91, "ネッキー,Necky"},
            { 92, "リンクスR,Lynx R"},
            { 93, "キトゥンR,Kitten R"},
            { 96, "キャンサーR,Cancer R"},
            { 97, "アクベンスR,Acubens R"},
            { 100, "波乗ニャウ,Wave Riding Nyau"},
            { 101, "リリダイコ,Lilidaigo"},
            { 102, "RRリコ,RR Rico"},
            { 103, "ニドラー,Nidra"},
            { 104, "ピトリ,Pitri"},
            { 105, "プチアンガ,Petit Anga"},
            { 106, "ウサノヤ,Usanoya"},
            { 107, "モチュゴヤ,Mochugoya"},
            { 108, "オパオパ,Opa-Opa"},
            { 109, "紫ぷよ,Purple Puyo"},
            { 110, "テムジン,Temjin"},
            { 111, "青ぷよ,Blue Puyo"},
            { 112, "ルーシャー,Luther"},
            { 113, "バットリー,Batreat"},
            { 114, "シャノルン,Chanolun"},
            { 115, "ハッピー,Happy"},
            { 116, "ナガミミ,Nagamimi"},
            { 117, "ノルルン,Nolulun"},
            { 118, "クツッピー,Kutsuppy"},
            { 119, "ラブクマミ,Lovekumami"},
            { 120, "ラヴィクマ,LoveyKuma"},
            { 121, "チェリシュ,Cherish"},
            { 122, "オーディン,Odin"},
            { 123, "モーグリ,Moogle"},
            { 124, "プリニー,Prinny"},
            { 125, "東京タワーラッピー,Tokyo Tower Rappy"},
            { 126, "金シャチラッピー,Golden Whale Rappy"},
            { 127, "屋台ラッピー,Food Cart Rappy"},
            { 128, "ジンギスカンラッピー,Jingisukan Rappy"},
            { 129, "通天閣(R)ラッピー,Tsutenkaku (R) Rappy"},
            { 130, "ステルス化,Stealth"},
            { 131, "ケツカッチン,Ketsukacchin"},
            { 132, "エスカくん,Eska-kun"},
            { 133, "プリュミー,Plumii"},
            { 134, "光武二式（大神機）,Koubu MK II (OM)"},
            { 135, "光武二式（さくら機）,Koubu MK II (SM)"},
            { 136, "パティエンティア,Patty & Tiea"},
            { 137, "ワンダラー,Wanderer"},
            { 138, "ミニゴジラ,Mini Godzilla"},
            { 139, "ピートR,Pete R"},
            { 140, "ブラナート,Brownart"},
            
            { 143, "ポーラー,Polar"},
            { 144, "ポッド０４２,Pod 042"},
            { 145, "ポッド１５３,Pod 153"},
            { 146, "桜ニャウ,Sakura Nyau"},
            { 147, "フログラッピー,Frog Rappy"},
            { 148, "モア,Moa"},
            { 149, "スティレットＧ,Stylet G"},
            { 150, "エンペ・ラッピー,Empe Rappy"},
            { 151, "ヤミガラス,Yamigarasu"},
            { 152, "プラフタ,Plachta"},
            { 153, "ラタン・エンペ,Rotten Empe"},
            { 154, "アクス・ラッピー,ARKS Rappy"},
            { 155, "フェザーキャロル,Feather Carole"},
            { 156, "フェリチェマクス,Felice Max"},
            { 158, "ホワイトラプラス,White Laplace"},
            { 159, "エグ・ラッピー,Egg Rappy"},
            { 160, "ラヴィ・エンペ,Lovey Empe"},
            { 162, "パック,Puck"},
            { 164, "ラブ・エンペ,Love Empe"},
            { 166, "ダークラッピー,Dark Rappy"},
            { 167, "テルテルラッピー,Tell Tell Rappy"},
            { 168, "モルガナ,Morgana"},
            { 169, "バーゼラルドＧ,Baselard G"},
            { 170, "轟雷Ｇ,Gourai G"},
            { 171, "パートニャー,Partnya"},
            { 172, "プーギー,Poogie"},
            { 173, "レイチュ,Reichu"},
            { 174, "セント・ラッピー,Saint Rappy"},
            { 176, "ジャックフロスト,Jack Frost"},

            { 177, ",Ornate Japanese looking black pig"},
            { 178, "ちびレラ,Sabirera (Quartz Dragon/クォーツドラゴン)"},
            { 179, "サクリャ,Sakurya"},
            { 180, "リトルメデューナ,Little Meduna"},
            { 181, "キツネアメ,Kitsune Ame"},
            { 182, "カグヤ,Kaguya"},
            { 183, "ペルチョナ,Perchona"},
            { 184, "タマヒメ2,Tamahime 2"},
            { 185, "ペギー,Pegi"},
            { 186, "ペギータ,Pegita"},
            { 188, "Vハンター,V Hunter"},
            { 190, "パイ,Pai"},
            { 191, ",Ornate Japanese looking black mouse"},
            { 192, "チョトリ,Chotrea"},
            { 193, "ミルトリ,Miltrea"},
            { 194, "フェズ,Feath"},
            { 195, "カエルン,Kaerun"},
            { 196, "オッチャ,Ocha"},
            
            { 999, "Debug" }
        };

        public Dictionary<int, BODYObject> costumeDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> carmDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> clegDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> outerDict = new Dictionary<int, BODYObject>();

        public Dictionary<int, BODYObject> baseWearDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BBLYObject> innerWearDict = new Dictionary<int, BBLYObject>();
        public Dictionary<int, BBLYObject> bodyPaintDict = new Dictionary<int, BBLYObject>();
        public Dictionary<int, StickerObject> stickerDict = new Dictionary<int, StickerObject>();

        public Dictionary<int, FACEObject> faceDict = new Dictionary<int, FACEObject>();
        public Dictionary<int, FCMNObject> fcmnDict = new Dictionary<int, FCMNObject>();
        public Dictionary<int, NGS_FACEObject> ngsFaceDict = new Dictionary<int, NGS_FACEObject>();
        public Dictionary<int, FCPObject> fcpDict = new Dictionary<int, FCPObject>();

        public Dictionary<int, ACCEObject> accessoryDict = new Dictionary<int, ACCEObject>();
        public Dictionary<int, EYEObject> eyeDict = new Dictionary<int, EYEObject>();
        public Dictionary<int, NGS_EarObject> ngsEarDict = new Dictionary<int, NGS_EarObject>();
        public Dictionary<int, NGS_TeethObject> ngsTeethDict = new Dictionary<int, NGS_TeethObject>();

        public List<NGS_HornObject> ngsHornList = new List<NGS_HornObject>(); //Necessary since there are unindexable structs in these. Unfortunate.
        public Dictionary<int, NGS_HornObject> ngsHornDict = new Dictionary<int, NGS_HornObject>();
        public Dictionary<int, NGS_SKINObject> ngsSkinDict = new Dictionary<int, NGS_SKINObject>();
        public Dictionary<int, EYEBObject> eyebrowDict = new Dictionary<int, EYEBObject>();
        public Dictionary<int, EYEBObject> eyelashDict = new Dictionary<int, EYEBObject>();

        public Dictionary<int, HAIRObject> hairDict = new Dictionary<int, HAIRObject>();
        public Dictionary<int, NIFL_COLObject> colDict = new Dictionary<int, NIFL_COLObject>();
        public List<Unk_IntField> unkList = new List<Unk_IntField>();
        public Dictionary<int, BCLN> costumeIdLink = new Dictionary<int, BCLN>();

        public Dictionary<int, BCLN> castArmIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> clegIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> outerWearIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> baseWearIdLink = new Dictionary<int, BCLN>();

        public Dictionary<int, BCLN> innerWearIdLink = new Dictionary<int, BCLN>();

        public CMXTable cmxTable;

        public class BaseCMXObject
        {
            public int num;
            public long originalOffset;
        }

        public class BODYObject : BaseCMXObject
        {
            public BODY body;
            public string dataString;
            public string texString1;
            public string texString2;
            public string texString3;
            public string texString4;
            public string texString5;
            public string texString6;
        }

        //Used for BODY, CARM, CLEG, Outer Wear (BODY), BCLN
        public struct BODY
        {
            public int id; //0xFF, 0x8
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
            public int texString5Ptr;
            public int texString6Ptr;

            public int int_20;
            public int int_24_0x9_0x9;   //0x9, 0x9
            public int int_28;
            public int int_2C;

            public int costumeSoundId;   //0xA, 0x8
            public int reference_id;  //0xD, 0x8
            public int int_38;         //Usually -1
            public int int_3C;         //Usually -1

            public int int_40;         //Usually -1
            public int int_44;         //Usually -1
            public float legLength;   //0x8, 0xA
            public float float_4C_0xB;   //0xB, 0xA

            public float float_50;
            public float float_54;
            public float float_58;
            public float float_5C;

            public float float_60;
            public int int_64;
        }

        public class BBLYObject : BaseCMXObject
        {
            public BBLY bbly;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
            public string texString5;
        }

        //BBLY, BDP1
        public struct BBLY
        {
            public int id; //0xFF, 0x8
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int texString5Ptr;
            public int unkInt0;       //This value forward may just be junk or new functionality in this version. Old BBLY only had the id and texture strings. Seems to correlate to BODY stuff though.
            public int unkInt1;

            public int unkInt2;
            public int unkInt3;
            public float unkFloat0;
            public float unkFloat1;

            public int unkInt4;
            public int unkInt5;
            public float unkFloat2;
            public float unkFloat3;

            public float unkFloat4;
            public float unkFloat5;
        }

        //BDP2(stickers)
        public class StickerObject : BaseCMXObject
        {
            public Sticker sticker;
            public string texString;
        }

        public struct Sticker
        {
            public int id;
            public int texStringPtr;
            public int reserve0;
        }

        public class FACEObject : BaseCMXObject
        {
            public FACE face;
            public string dataString;
            public string texString1;
            public string texString2;

            public string texString3;
            public string texString4;
            public string texString5;
            public string texString6;
        }

        public struct FACE
        {
            public int id; //0xFF, 0x8
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
            public int texString5Ptr;
            public int texString6Ptr;

            public int unkInt0;
            public int unkInt1;
            public int unkInt2;
            public int unkInt3;

            public int unkInt4;
            public float unkFloat0;
            public float unkFloat1;
            public float unkFloat2;

            public float unkFloat3;
            public int unkInt5;
            public int unkInt6;
            public int unkInt7;

            public int unkInt8;
            public int unkInt9;
            public int unkInt10;
            public int unkInt11;
        }

        public class FCMNObject : BaseCMXObject
        {
            public FCMN fcmn;
            public string proportionAnim;
            public string faceAnim1;
            public string faceAnim2;

            public string faceAnim3;
            public string faceAnim4;
            public string faceAnim5;
            public string faceAnim6;

            public string faceAnim7;
            public string faceAnim8;
            public string faceAnim9;
            public string faceAnim10;
        }

        public struct FCMN
        {
            public int id; //FF, 0x8
            public int proportionAnimPtr;
            public int faceAnim1Ptr;
            public int faceAnim2Ptr;

            public int faceAnim3Ptr;
            public int faceAnim4Ptr;
            public int faceAnim5Ptr;
            public int faceAnim6Ptr;

            public int faceAnim7Ptr;
            public int faceAnim8Ptr;
            public int faceAnim9Ptr;
            public int faceAnim10Ptr;
        }

        public class NGS_FACEObject : BaseCMXObject
        {
            public NGS_FACE ngsFace;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
        }

        public struct NGS_FACE
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
        }

        public class FCPObject : BaseCMXObject
        {
            public FCP fcp;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
        }

        public struct FCP
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int unkInt0;
            public int unkInt1;
            public int unkInt2;

            public int unkInt3;
            public int unkInt4;
            public int unkInt5;
        }

        public class ACCEObject : BaseCMXObject
        {
            public ACCE acce;
            public int int_54;
            public ACCE2 acce2;
            public string dataString;
            public string nodeAttach1;
            public string nodeAttach2;

            public string nodeAttach3;
            public string nodeAttach4;
            public string nodeAttach5;
            public string nodeAttach6;

            public string nodeAttach7;
            public string nodeAttach8;
        }

        public struct ACCE
        {
            public int id;
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int nodeAttach1Ptr; //These pointers are to bone names the accessory uses when attaching by default in PSO2 Classic seemingly
            public int nodeAttach2Ptr;

            public int nodeAttach3Ptr;
            public int nodeAttach4Ptr;
            public int nodeAttach5Ptr;
            public int nodeAttach6Ptr;

            public int nodeAttach7Ptr;
            public int nodeAttach8Ptr;
            public int unkInt0;           //Often 0x1
            public int unkInt1;           //Often 0x64

            public int unkInt2;
            public int unkInt3;
            public int unkInt4;
            public int unkInt5;

            public int unkInt6;
        }

        public struct ACCE2
        {
            public int unkInt7;
            public float unkFloat0;
            public float unkFloat1;

            public float unkFloat2;
            public float unkFloat3;
            public float unkFloat4;
            public float unkFloat5;

            public int unkInt8;
            public int unkInt9;
            public int unkInt10;
            public float unkFloat6;

            public int unkInt11;
            public int unkInt12;
            public int unkInt13;
            public short unkShort0;
            public short unkShort_D7; //0xD7, 0x6

            public short unkShort2;
            public short unkShort3;
            public short unkShort4;
            public short unkshort5;
            public int unktIn14;
            public int unkInt15;

            public int unkInt16;
            public short unkShort6;
            public short unkShort_C7; //0xC7, 0x6
            public short unkShort8;
            public short unkShort9;
            public float unkFloat7;

            public float unkFloat8;
            public int unkInt18;
            public int unkInt19;
            public short unkShort10;
            public short unkShort11;

            public short unkShort12;
            public short unkShort13;
        }

        public class EYEObject : BaseCMXObject
        {
            public EYE eye;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
            public string texString5;
        }

        public struct EYE
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int texString5Ptr;
            public float unkFloat0;
            public float unkFloat1;
        }

        public class NGS_EarObject : BaseCMXObject
        {
            public NGS_Ear ngsEar;
            public NGS_Unk_Substruct subStruct;

            public string dataString;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
            public string texString5;
        }

        public struct NGS_Ear
        {
            public int unkSubStructPtr; //Not sure what this is at the moment.
            public int dataStringPtr;
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
            public int texString5Ptr;
            public int unkInt0;

            public int unkInt1;
            public int unkInt2;
            public int unkInt3;
            public int unkInt4;
        }

        //Weird 0xA long substruct for NGS_Ear and NGS_Horn
        public struct NGS_Unk_Substruct
        {
            public short unkShort0;
            public short unkShort1;
            public short unkShort2;
            public short unkShort3;
            public short unkShort4;
        }

        public class NGS_TeethObject : BaseCMXObject
        {
            public NGS_Teeth ngsTeeth;
            public NGS_Unk_Substruct substruct;

            public string dataString;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
        }

        public struct NGS_Teeth
        {
            public int unkSubStructPtr; //Not sure what this is at the moment.
            public int dataStringPtr;
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
        }

        public class NGS_HornObject : BaseCMXObject
        {
            public NGS_Horn ngsHorn;
            public NGS_Unk_Substruct substruct;

            public string dataString;
        }

        public struct NGS_Horn
        {
            public int unkSubStructPtr; //Not sure what this is at the moment. Shared with NGS_Ear versions? Assumedly not used when 0.
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int reserve0;      //Always 0 so far.
        }

        public class NGS_SKINObject : BaseCMXObject
        {
            public NGS_Skin ngsSkin;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
            public string texString5;
            public string texString6;
            public string texString7;
        }

        public struct NGS_Skin
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int texString5Ptr;
            public int texString6Ptr;
            public int texString7Ptr;

            public int texString8Ptr;
            public int texString9Ptr;
            public int texString10Ptr;
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;
        }

        public class EYEBObject : BaseCMXObject
        {
            public EYEB eyeb;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
        }

        //Also for EYEL
        public struct EYEB
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
        }

        public class HAIRObject : BaseCMXObject
        {
            public HAIR hair;
            public string dataString;
            public string texString1;
            public string texString2;

            public string texString3;
            public string texString4;
            public string texString5;
            public string texString6;

            public string texString7;
        }

        public struct HAIR
        {
            public int id;
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
            public int texString5Ptr;
            public int texString6Ptr;

            public int texString7Ptr;
            public int unkIntB1;      //B0, 0x9
            public int unkInt1;
            public int unkInt2;

            public float unkFloat0;
            public float unkFloat1;
            public float unkFloat2;
            public float unkFloat3;

            public int unkInt3;
            public int unkInt4;
            public int unkInt5;
            public int unkInt6;

            public int unkInt7;
            public int unkInt8;
            public float unkFloat4;
            public float unkFloat5;

            public float unkFloat6;
            public int unkInt9;
            public int unkInt10;
            public int unkInt11;

            public int unkInt12;
            public int unkInt13;
            public int unkInt14;
            public float unkFloat7;

            public float unkFloat8;
            public float unkFloat9;
            public int unkInt15;
            public int unkInt16;

            public int unkInt17;
            public int unkInt18;
            public int unkInt19;
            public int unkInt20;

            public short unkShortB1; //0xB1, 0x6
            public short unkShortB2; //0xB2, 0x6
            public short unkShortB3; //0xB3, 0x6
            public short unkShort0;
        }

        public class NIFL_COLObject : BaseCMXObject
        {
            public NIFL_COL niflCol;
            public string textString;
        }

        //The color data here seems to be totally different than before so data just won't be compatible with the old format, not that we understood it before.
        public unsafe struct NIFL_COL
        {
            public int id;
            public int textStringPtr;
            public fixed int colorData[42];
        }

        //This maybe color related. But I have no idea what it's supposed to do.
        public unsafe struct Unk_IntField
        {
            public fixed int unkIntField[79];
        }

        //Also for LCLN, ACLN, and ICLN. Id remapping info. Recolor outfits have multiple ids, but only one id that will actually correlate to a file.
        public struct BCLN
        {
            public int id;
            public int fileId;
            public int unkInt;
        }

        public struct CMXTable
        {
            public int bodyAddress; //BODY Costumes
            public int carmAddress; //CARM Cast Arms
            public int clegAddress; //CLEG Cast Legs
            public int bodyOuterAddress; //BODY Outer Wear

            public int baseWearAddress; //BCLN Base Wear
            public int innerWearAddress; //BBLY Inner Wear
            public int bodyPaintAddress; //BDP1 Body Paint 
            public int stickerAddress; //BDP2 Stickers

            public int faceAddress; //FACE All heads
            public int faceMotionAddress; //Face motions
            public int rebootFaceAddress; //NGS Faces?
            public int faceTexturesAddress; //Face textures and face paint

            public int accessoryAddress; //ACCE Accessories
            public int eyeTextureAddress; //EYE eye textures
            public int earAddress; //reboot ears
            public int teethAddress; //reboot mouths

            public int hornAddress; //reboot horns
            public int skinAddress; //reboot and maybe classic skin?
            public int eyebrowAddress; //EYEB eyebrows
            public int eyelashAddress; //EYEL eyelashes

            public int hairAddress; //HAIR 
            public int colAddress; //COL, for color chart textures
            public int unkAddress; //Unknown arrays
            public int costumeIdLinkAddress; //BCLN Costume ids for recolors

            public int castArmIdLinkAddress; //BCLN Cast arm ids for recolors
            public int castLegIdLinkAddress; //BCLN Cast leg ids for recolors
            public int outerIdLinkAddress; //BCLN Outer ids for recolors
            public int baseWearIdLinkAddress; //BCLN basewear ids for recolors

            public int innerWearIdLinkAddress; //BCLN innerwear ids for recolors

            public int bodyCount; 
            public int carmCount; 
            public int clegCount;
            public int bodyOuterCount;

            public int baseWearCount;
            public int innerWearCount;
            public int bodyPaintCount;
            public int stickerCount;

            public int faceCount;
            public int faceMotionCount;
            public int rebootFaceCount;
            public int faceTexturesCount;

            public int accessoryCount;
            public int eyeTextureCount;
            public int earCount;
            public int teethCount;

            public int hornCount;
            public int skinCount;
            public int eyebrowCount;
            public int eyelashCount;

            public int hairCount;
            public int colCount;
            public int unkCount;
            public int costumeIdLinkCount;

            public int castArmIdLinkCount;
            public int castLegIdLinkCount;
            public int outerIdLinkCount;
            public int baseWearIdLinkCount;

            public int innerWearIdLinkCount;
        }
    }
}
