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

        public static Dictionary<int, string> swordNames = new Dictionary<int, string>()
        {
            { 022, "＊ブラッドサイズ,* Blood Scythe"},
            { 044, "＊スペース・ツナ,* Space Tuna"},
            { 098, "＊零織・イザヨイ,* Zero-type Izayoi"},
            { 104, "＊フロウウェンの大剣,* Flowen's Sword"},
            { 106, "＊ジャンクヤード・ドッグ,* Junkyard Dog"},
            { 130, "＊約束された勝利の剣,* The Sword of Promised Victory"},
            { 131, "＊乖離剣エア,* Sword of Rupture, Ea"},
            { 177, "＊スライプナーＭｋ６,* Sleipnir Mk6"},
            { 183, "＊スライプナーＭｋ６Ｄ,* Sleipnir Mk6D"},
            { 184, "＊スライプナーＭｋ６Ｒ,* Sleipnir Mk6R"},
            { 238, "＊ブルー・ハート,* Blue Heart"},
            { 256, "ソードレボルシオ,Sword Revolucio"},
            { 257, "ソードドミナシオ,Sword Dominacio"},
            { 258, "ソードインクルシオ,Sword Inclusio"},
            { 333, "＊レヴァティーン,* Laevateinn"},
            { 362, "＊上条専用スライプナー,* Sleipnir for Kamijo"},
            { 363, "＊スライプナー,* Sleipnir"},
            { 374, "＊ＢＬＡＣＫ　ＭＯＮＯＬＩＴＨ,* Black Monolith"},
            { 375, "＊ＢＬＯＯＤ　ＰＲＡＹＥＲＳ,* Blood Prayers"},
        };
        public static Dictionary<int, string> wiredLanceNames = new Dictionary<int, string>()
        {            
            { 080, "＊ラッピーケージ,* Rappy Cage"},
            { 135, "＊超硬質ブレード,* Ultrahard Blades"},
            { 168, "＊荊,* Thorn"},
            { 197, "ランスレボルシオ,Lance Revolucio"},
            { 198, "ランスドミナシオ,Lance Dominacio"},
            { 199, "ランスインクルシオ,Lance Inclusio"},
            { 247, "ゲノンズィーバ,Genon Ziva"},
            { 259, "＊レヴァティーン,* Laevateinn"},
        };
        public static Dictionary<int, string> partizanNames = new Dictionary<int, string>()
        {
            { 061, "＊零織・イザヨイ,* Zero-type Izayoi"},
            { 201, "スピアレボルシオ,Spear Revolucio"},
            { 202, "スピアドミナシオ,Spear Dominacio"},
            { 203, "スピアインクルシオ,Spear Inclusio"},
            { 274, "＊レヴァティーン,* Laevateinn"},
            { 335, "＊ロンギヌスの槍,* Longinus Lance"},
        };
        public static Dictionary<int, string> twinDaggerNames = new Dictionary<int, string>()
        {
            { 217, "スラストレボルシオ,Thrust Revolucio"},
            { 218, "スラストドミナシオ,Thrust Dominacio"},
            { 219, "スラストインクルシオ,Thrust Inclusio"},
            { 278, "＊レヴァティーン,* Laevateinn"},
        };
        public static Dictionary<int, string> doubleSaberNames = new Dictionary<int, string>()
        {
            { 062, "＊ブルー・ハート,* Blue Heart"},
            { 078, "＊ブラッドサイズ,* Blood Scythe"},
            { 196, "＊ジャンクヤード・ドッグ,* Junkyard Dog"},
            { 210, "ストームレボルシオ,Storm Revolucio"},
            { 211, "ストームドミナシオ,Storm Dominacio"},
            { 212, "ストームインクルシオ,Storm Inclusio"},
            { 308, "＊レヴァティーン,* Laevateinn"},
        };
        public static Dictionary<int, string> knucklesNames = new Dictionary<int, string>()
        {
            { 197, "スパイクレボルシオ,Spike Revolucio"},
            { 198, "スパイクドミナシオ,Spike Dominacio"},
            { 199, "スパイクインクルシオ,Spike Inclusio"},
        };
        public static Dictionary<int, string> gunslashNames = new Dictionary<int, string>()
        {
            { 152, "＊スライプナーＭｋ６,* Sleipnir Mk6"},
            { 156, "＊スライプナーＭｋ６Ｄ,* Sleipnir Mk6D"},
            { 157, "＊スライプナーＭｋ６Ｒ,* Sleipnir Mk6R"},
            { 213, "スラッシュレボルシオ,Slash Revolucio"},
            { 214, "スラッシュドミナシオ,Slash Dominacio"},
            { 215, "スラッシュインクルシオ,Slash Inclusio"},
        };
        public static Dictionary<int, string> rifleNames = new Dictionary<int, string>()
        {
            { 237, "ショットレボルシオ,Shot Revolucio"},
            { 238, "ショットドミナシオ,Shot Dominacio"},
            { 239, "ショットインクルシオ,Shot Inclusio"},
        };
        public static Dictionary<int, string> launcherNames = new Dictionary<int, string>()
        {
            { 027, "フォトンランチャー,Photon Launcher"},
            { 037, "レッドスコルピオ,Red Scorpio"},
            { 038, "ギルティライト,Guilty Light"},
            { 165, "＊メセタンシューター,* Mesetan Shooter"},
            { 193, "＊スクルド,* Skuld"},
            { 237, "キャノンレボルシオ,Cannon Revolucio"},
            { 238, "キャノンドミナシオ,Cannon Dominacio"},
            { 239, "キャノンインクルシオ,Cannon Inclusio"},
            { 250, "フォルニスルーギア,Fornis Lugia"},
            { 251, "アクルドルーギア,Aculd Lugia"},
            { 255, "＊ＲＡトラベラー,RA Traberah"},
            { 269, "＊ジラドボルス,* Jilad Volse"},
            { 311, "＊ＢＬＡＣＫ　ＭＯＮＯＬＩＴＨ,* Black Monolith"},
            { 312, "＊ＢＬＯＯＤ　ＰＲＡＹＥＲＳ,* Blood Prayers"},
            { 316, "＊灼零エルゼガンツェ,* Burning Zero Elze Ganze"},
        };
        public static Dictionary<int, string> tmgNames = new Dictionary<int, string>()
        {
            { 276, "バレットレボルシオ,Bullet Revolucio"},
            { 277, "バレットドミナシオ,Bullet Dominacio"},
            { 278, "バレットインクルシオ,Bullet Inclusio"},
        };
        public static Dictionary<int, string> rodNames = new Dictionary<int, string>()
        {
            { 010, "フォールオンス,Fallounce"},
            { 011, "アルバフォールオンス,Alba Fallounce"},
            { 012, "ヴィタフォールオンス,Vita Fallounce"},
            { 259, "シャフトレボルシオ,Shaft Revolucio"},
            { 260, "シャフトドミナシオ,Shaft Dominacio"},
            { 261, "シャフトインクルシオ,Shaft Inclusio"},
            { 323, "＊ヴィタフォールオンス,＊ Vita Fallounce"},
            { 335, "＊リバレイトアーマリー,* Rivalate Armory"},
            { 352, "＊レヴァティーン,* Laevateinn"},
        };
        public static Dictionary<int, string> talysNames = new Dictionary<int, string>()
        {
            { 234, "カードレボルシオ,Card Revolucio"},
            { 235, "カードドミナシオ,Card Dominacio"},
            { 236, "カードインクルシオ,Card Inclusio"},
            { 254, "＊ジラドベリス,* Jilad Veris"},
        };
        public static Dictionary<int, string> wandNames = new Dictionary<int, string>()
        {
            { 055, "＊マジカルサファイア,* Magical Saphire"},
            { 066, "＊ジャンクヤード・ドッグ,* Junkyard Dog"},
            { 139, "＊マジカルルビー,* Magical Ruby"},
            { 253, "エッジレボルシオ,Saika Revolucio"},
            { 254, "エッジドミナシオ,Saika Dominacio"},
            { 255, "エッジインクルシオ,Saika Inclusio"},
            { 354, "＊レヴァティーン,* Laevateinn"},
        };
        public static Dictionary<int, string> katanaNames = new Dictionary<int, string>()
        {
            { 100, "＊ソウル・オブ・ウォパル,* Soul of Vopar"},
            { 169, "カザミノタチ,Kazami-no-tachi"},
            { 210, "サイカレボルシオ,Saika Revolucio"},
            { 211, "サイカドミナシオ,Saika Dominacio"},
            { 212, "サイカインクルシオ,Saika Inclusio"},
            { 291, "＊レヴァティーン,* Laevateinn"},
        };
        public static Dictionary<int, string> bowNames = new Dictionary<int, string>()
        {
            { 198, "アーチレボルシオ,Arch Revolucio"},
            { 199, "アーチドミナシオ,Arch Dominacio"},
            { 200, "アーチインクルシオ,Arch Inclusio"},
        };
        public static Dictionary<int, string> jetBootsNames = new Dictionary<int, string>()
        {
            { 125, "＊キジカナルカミ,* Kizika Narukami"},
            { 181, "ブレードレボルシオ,Shoes Revolucio"},
            { 182, "ブレードドミナシオ,Shoes Dominacio"},
            { 183, "ブレードインクルシオ,Shoes Inclusio"},
        };
        public static Dictionary<int, string> dualBladesNames = new Dictionary<int, string>()
        {
            { 215, "ブレードレボルシオ,Blade Revolucio"},
            { 216, "ブレードドミナシオ,Blade Dominacio"},
            { 217, "ブレードインクルシオ,Blade Inclusio"},
            { 271, "＊レヴァティーン,* Laevateinn"},
        };
        public static Dictionary<int, string> tactNames = new Dictionary<int, string>()
        {
            { 017, "タクトレボルシオ,Takt Revolucio"},
            { 019, "タクトインクルシオ,Takt Inclusio"}
        };


        public static Dictionary<int, string> swordNGSNames = new Dictionary<int, string>()
        {
            { 022, "＊ブラッドサイズ,* Blood Scythe"},
            { 044, "＊スペース・ツナ,* Space Tuna"},
            { 098, "＊零織・イザヨイ,* Zero-type Izayoi"},
            { 104, "＊フロウウェンの大剣,* Flowen's Sword"},
            { 106, "＊ジャンクヤード・ドッグ,* Junkyard Dog"},
            { 130, "＊約束された勝利の剣,* The Sword of Promised Victory"},
            { 131, "＊乖離剣エア,* Sword of Rupture, Ea"},
            { 177, "＊スライプナーＭｋ６,* Sleipnir Mk6"},
            { 183, "＊スライプナーＭｋ６Ｄ,* Sleipnir Mk6D"},
            { 184, "＊スライプナーＭｋ６Ｒ,* Sleipnir Mk6R"},
            { 238, "＊ブルー・ハート,* Blue Heart"},
            { 256, "ソードレボルシオ,Sword Revolucio"},
            { 257, "ソードドミナシオ,Sword Dominacio"},
            { 258, "ソードインクルシオ,Sword Inclusio"},
            { 333, "＊レヴァティーン,* Laevateinn"},
            { 362, "＊上条専用スライプナー,* Sleipnir for Kamijo"},
            { 363, "＊スライプナー,* Sleipnir"},            
            { 374, "＊ＢＬＡＣＫ　ＭＯＮＯＬＩＴＨ,* Black Monolith"},
            { 375, "＊ＢＬＯＯＤ　ＰＲＡＹＥＲＳ,* Blood Prayers"},
            { 501, "プリムソード,Primm Sword"},
            { 503, "ゴルドプリムソード?,Gold Primm Sword?"},
            { 504, "ツヴィアソード,Tzvia Sword"},
            { 505, "リサージュソード,Resurgir Sword"},
            { 508, "テルセウスソード,Theseus Sword"},
            { 509, "キャトリアソード,Cattleya Sword"},
            { 513, "グリッセンソード,Glissen Sword"},
            { 514, "プリムソード?,Primm Sword?"},
            { 515, "エヴォルコートソード,Evolcoat Sword"},
            { 501, "ゴルドプリムソード,Gold Primm Sword"},
            { 501, "シルヴァプリムソード,Silver Primm Sword"},
            { 501, "ストラーガソード,Straga Sword"},
        };
        public static Dictionary<int, string> wiredLanceNGSNames = new Dictionary<int, string>()
        {
            { 080, "＊ラッピーケージ,* Rappy Cage"},
            { 135, "＊超硬質ブレード,* Ultrahard Blades"},
            { 168, "＊荊,* Thorn"},
            { 197, "ランスレボルシオ,Lance Revolucio"},
            { 198, "ランスドミナシオ,Lance Dominacio"},
            { 199, "ランスインクルシオ,Lance Inclusio"},
            { 247, "ゲノンズィーバ,Genon Ziva"},
            { 259, "＊レヴァティーン,* Laevateinn"},
            { 501, "プリムワイヤー,Primm Wire"},
            { 502, "リサージュワイヤー,Resurgir Wire"},
            { 504, "テルセウスワイヤー,Theseus Wire"},
            { 505, "フォーシスワイヤー,Foursis Wire"},
            { 506, "グリッセンワイヤー,Glissen Wire"},
            { 508, "ツヴィアワイヤー,Tzvia Wire"},
            { 509, "エヴォルコートワイヤー,Evolcoat Wire"},
            { 513, "フロステルワイヤー,Frostel Wire"},
            { 521, "ゴルドプリムワイヤー,Gold Primm Wire"},
            { 522, "シルヴァプリムワイヤー,Silver Primm Wire"},
            { 523, "ストラーガワイヤー,Straga Wire"},
        };
        public static Dictionary<int, string> partizanNGSNames = new Dictionary<int, string>()
        {
            { 061, "＊零織・イザヨイ,* Zero-type Izayoi"},
            { 201, "スピアレボルシオ,Spear Revolucio"},
            { 202, "スピアドミナシオ,Spear Dominacio"},
            { 203, "スピアインクルシオ,Spear Inclusio"},
            { 274, "＊レヴァティーン,* Laevateinn"},
            { 335, "＊ロンギヌスの槍,* Longinus Lance"},
            { 501, "プリムスピア,Primm Spear"},
            { 502, "リサージュスピア,Resurgir Spear"},
            { 503, "テルセウススピア,Theseus Spear"},
            { 504, "＊ガロアスピア,* Garoa Spear"},
            { 506, "ヴィアルトスピア,Vialto Spear"},
            { 508, "ツヴィアスピア,Tzvia Spear"},
            { 509, "＊ブルッジスピア,* Dozerro Spear"},
            { 510, "エヴォルコートスピア,Evolcoat Spear"},
            { 522, "ゴルドプリムスピア,Gold Primm Spear"},
            { 523, "シルヴァプリムスピア,Silver Primm Spear"},
            { 524, "ストラーガスピア,Straga Spear"},
        };
        public static Dictionary<int, string> twinDaggerNGSNames = new Dictionary<int, string>()
        {
            { 217, "スラストレボルシオ,Thrust Revolucio"},
            { 218, "スラストドミナシオ,Thrust Dominacio"},
            { 219, "スラストインクルシオ,Thrust Inclusio"},
            { 278, "＊レヴァティーン,* Laevateinn"},
            { 501, "プリムダガー,Primm Daggers"},
            { 502, "リサージュダガー,Resurgir Daggers"},
            { 503, "フォーシスダガー,Foursis Daggers"},
            { 504, "＊マノフィーマルテ,* Manophi Marte"},
            { 505, "エヴォルコートダガー,Evolcoat Daggers"},
            { 507, "トロワーデダガー,Trois De Daggers"},
            { 508, "ツヴィアダガー,Tzvia Daggers"},
            { 522, "ゴルドプリムダガー,Gold Primm Daggers"},
            { 523, "シルヴァプリムダガー,Silver Primm Daggers"},
            { 524, "ストラーガアダガー,Straga Daggers"},
        };
        public static Dictionary<int, string> doubleSaberNGSNames = new Dictionary<int, string>()
        {
            { 062, "＊ブルー・ハート,* Blue Heart"},
            { 078, "＊ブラッドサイズ,* Blood Scythe"},
            { 196, "＊ジャンクヤード・ドッグ,* Junkyard Dog"},
            { 210, "ストームレボルシオ,Storm Revolucio"},
            { 211, "ストームドミナシオ,Storm Dominacio"},
            { 212, "ストームインクルシオ,Storm Inclusio"},
            { 308, "＊レヴァティーン,* Laevateinn"},
            { 501, "プリムセイバー,Primm Saber"},
            { 502, "リサージュセイバー,Resurgir Saber"},
            { 503, "ヴィアルトセイバー,Vialto Saber"},
            { 505, "トロワーデセイバー,Trois De Saber"},
            { 506, "ツヴィアセイバー,Tzvia Saber"},
            { 507, "グリッセンセイバー,Glissen Saber"},
            { 508, "エヴォルコートセイバー,Evolcoat Saber"},
            { 513, "フロステルセイバー,Frostel Saber"},
            { 523, "ゴルドプリムセイバー,Gold Primm Saber"},
            { 524, "シルヴァプリムセイバー,Silver Primm Saber"},
            { 525, "ストラーガプリムセイバー,Straga Saber"},
        };
        public static Dictionary<int, string> knucklesNGSNames = new Dictionary<int, string>()
        {
            { 197, "スパイクレボルシオ,Spike Revolucio"},
            { 198, "スパイクドミナシオ,Spike Dominacio"},
            { 199, "スパイクインクルシオ,Spike Inclusio"},
            { 501, "プリムナックル,Primm Knuckles"},
            { 502, "リサージュナックル,Resurgir Knuckles"},
            { 504, "キャトリアナックル,Cattleya Knuckles"},
            { 505, "トロワーデナックル,Trois De Knuckles"},
            { 506, "ツヴィアナックル,Tzvia Knuckles"},
            { 507, "グリッセンナックル,Glissen Knuckles"},
            { 508, "エヴォルコートナックル,Evolcoat Knuckles"},
            { 521, "ゴルドプリムナックル,Gold Primm Knuckles"},
            { 522, "シルヴァプリムナックル,Silver Primm Knuckles"},
            { 523, "ストラーガナックル,Straga Knuckles"},
        };
        public static Dictionary<int, string> gunslashNGSNames = new Dictionary<int, string>()
        {
            { 152, "＊スライプナーＭｋ６,* Sleipnir Mk6"},
            { 156, "＊スライプナーＭｋ６Ｄ,* Sleipnir Mk6D"},
            { 157, "＊スライプナーＭｋ６Ｒ,* Sleipnir Mk6R"},
            { 213, "スラッシュレボルシオ,Slash Revolucio"},
            { 214, "スラッシュドミナシオ,Slash Dominacio"},
            { 215, "スラッシュインクルシオ,Slash Inclusio"},
        };
        public static Dictionary<int, string> rifleNGSNames = new Dictionary<int, string>()
        {
            { 237, "ショットレボルシオ,Shot Revolucio"},
            { 238, "ショットドミナシオ,Shot Dominacio"},
            { 239, "ショットインクルシオ,Shot Inclusio"},
            { 501, "プリムライフル,Primm Rifle"},
            { 502, "リサージュライフル,Resurgir Rifle"},
            { 505, "テルセウスライフル,Theseus Rifle"},
            { 507, "フォーシスライフル,Foursis Rifle"},
            { 508, "＊アイネーロライフル,* Ainerro Rifle"},
            { 510, "ツヴィアライフル,Tzvia Rifle"},
            { 511, "エヴォルコートライフル,Evolcoat Rifle"},
            { 523, "ゴルドプリムライフル,Gold Primm Rifle"},
            { 524, "シルヴァプリムライフル,Silver Primm Rifle"},
            { 525, "ストラーガライフル,Straga Rifle"},
        };
        public static Dictionary<int, string> launcherNGSNames = new Dictionary<int, string>()
        {
            { 027, "フォトンランチャー,Photon Launcher"},
            { 037, "レッドスコルピオ,Red Scorpio"},
            { 038, "ギルティライト,Guilty Light"},
            { 165, "＊メセタンシューター,* Mesetan Shooter"},
            { 193, "＊スクルド,* Skuld"},
            { 237, "キャノンレボルシオ,Cannon Revolucio"},
            { 238, "キャノンドミナシオ,Cannon Dominacio"},
            { 239, "キャノンインクルシオ,Cannon Inclusio"},
            { 250, "フォルニスルーギア,Fornis Lugia"},
            { 251, "アクルドルーギア,Aculd Lugia"},
            { 255, "＊ＲＡトラベラー,RA Traberah"},
            { 269, "＊ジラドボルス,* Jilad Volse"},
            { 311, "＊ＢＬＡＣＫ　ＭＯＮＯＬＩＴＨ,* Black Monolith"},
            { 312, "＊ＢＬＯＯＤ　ＰＲＡＹＥＲＳ,* Blood Prayers"},
            { 316, "＊灼零エルゼガンツェ,* Burning Zero Elze Ganze"},
            { 501, "プリムランチャー,Primm Launcher"},
            { 502, "リサージュランチャー,Resurgir Launcher"},
            { 503, "テルセウスランチャー,Theseus Launcher"},
            { 506, "ヴィアルトランチャー,Vialto Launcher"},
            { 507, "ツヴィアランチャー,Tzvia Launcher"},
            { 508, "グリッセンランチャー,Glissen Launcher"},
            { 510, "エヴォルコートランチャー,Evolcoat Launcher"},
            { 514, "フロステルランチャー,Frostel Launcher"},
            { 521, "ゴルドプリムランチャー,Gold Primm Launcher"},
            { 522, "シルヴァプリムランチャー,Silver Primm Launcher"},
            { 524, "ストラーガランチャー,Straga Launcher"},
        };
        public static Dictionary<int, string> tmgNGSNames = new Dictionary<int, string>()
        {
            { 276, "バレットレボルシオ,Bullet Revolucio"},
            { 277, "バレットドミナシオ,Bullet Dominacio"},
            { 278, "バレットインクルシオ,Bullet Inclusio"},
            { 501, "プリムマシンガン,Primm Twin Machine Guns"},
            { 502, "リサージュマシンガン,Resurgir Machine Guns"},
            { 505, "キャトリアマシンガン,Cattleya Machine Guns"},
            { 506, "テルセウスマシンガン,Theseus Machine Guns"},
            { 507, "ツヴィアマシンガン,Tzvia Machine Guns"},
            { 508, "グリッセンマシンガン,Glissen Machine Guns"},
            { 509, "エヴォルコートマシンガン,Evolcoat Machine Guns"},
            { 514, "フロステルマシンガン,Frostel Machine Guns"},
            { 521, "ゴルドプリムマシンガン,Gold Primm Machine Guns"},
            { 522, "シルヴァプリムマシンガン,Silver Machine Guns"},
            { 524, "ストラーガマシンガン,Straga Machine Guns"},
        };
        public static Dictionary<int, string> rodNGSNames = new Dictionary<int, string>()
        {
            { 010, "フォールオンス,Fallounce"},
            { 011, "アルバフォールオンス,Alba Fallounce"},
            { 012, "ヴィタフォールオンス,Vita Fallounce"},
            { 259, "シャフトレボルシオ,Shaft Revolucio"},
            { 260, "シャフトドミナシオ,Shaft Dominacio"},
            { 261, "シャフトインクルシオ,Shaft Inclusio"},
            { 323, "＊ヴィタフォールオンス,＊ Vita Fallounce"},
            { 335, "＊リバレイトアーマリー,* Rivalate Armory"},
            { 352, "＊レヴァティーン,* Laevateinn"},
            { 501, "プリムロッド,Primm Rod"},
            { 502, "リサージュロッド,Resurgir Rod"},
            { 503, "トロワーデロッド,Trois De Rod"},
            { 504, ","},
            { 505, "＊マノフィーマルテ,* Manophi Marte"},
            { 508, "フォーシスロッド,Foursis Rod"},
            { 510, "ツヴィアロッド,Tzvia Rod"},
            { 511, "エヴォルコートロッド,Evolcoat Rod"},
            { 522, "ゴルドプリムロッド,Gold Primm Rod"},
            { 523, "シルヴァプリムロッド,Silver Primm Rod"},
            { 524, "ストラーガロッド,Straga Rod"},
            { 525, "フロステルロッド,Frostel Rod"},
        };
        public static Dictionary<int, string> talysNGSNames = new Dictionary<int, string>()
        {
            { 234, "カードレボルシオ,Card Revolucio"},
            { 235, "カードドミナシオ,Card Dominacio"},
            { 236, "カードインクルシオ,Card Inclusio"},
            { 254, "＊ジラドベリス,* Jilad Veris"},
            { 501, "プリムタリス,Primm Talis"},
            { 502, "リサージュタリス,Resurgir Talis"},
            { 505, "ヴィアルトタリス,Vialto Talis"},
            { 506, "トロワーデタリス,Trois De Talis"},
            { 507, "ツヴィアタリス,Tzvia Talis"},
            { 508, "グリッセンタリス,Glissen Talis"},
            { 509, "エヴォルコートタリス,Evolcoat Talis"},
            { 520, "ゴルドプリムタリス,Gold Primm Talis"},
            { 521, "シルヴァプリムタリス,Silver Primm Talis"},
            { 522, "ストラーガタリス,Straga Talis"},
        };
        public static Dictionary<int, string> wandNGSNames = new Dictionary<int, string>()
        {
            { 055, "＊マジカルサファイア,* Magical Saphire"},
            { 066, "＊ジャンクヤード・ドッグ,* Junkyard Dog"},
            { 139, "＊マジカルルビー,* Magical Ruby"},
            { 253, "エッジレボルシオ,Saika Revolucio"},
            { 254, "エッジドミナシオ,Saika Dominacio"},
            { 255, "エッジインクルシオ,Saika Inclusio"},
            { 354, "＊レヴァティーン,* Laevateinn"},
            { 501, "プリムウォンド,Primm Wand"},
            { 502, "リサージュウォンド,Resurgir Wand"},
            { 503, "グリッセンウォンド,Glissen Wand"},
            { 505, "クロフォードのウォンド?,Crawford Wand?"},
            { 507, "トロワーデウォンド,Trois De Wand"},
            { 508, "キャトリアウォンド,Cattleya Wand"},
            { 509, "ツヴィアウォンド,Tzvia Wand"},
            { 510, "エヴォルコートウォンド,Evolcoat Wand"},
            { 514, "フロステルウォンド,Frostel Wand"},
            { 521, "ゴルドプリムウォンド,Gold Primm  Wand"},
            { 522, "シルヴァプリムウォンド,Silver Primm Wand"},
            { 524, "ストラーガウォンド,Straga Wand"},
        };
        public static Dictionary<int, string> katanaNGSNames = new Dictionary<int, string>()
        {
            { 100, "＊ソウル・オブ・ウォパル,* Soul of Vopar"},
            { 169, "カザミノタチ,Kazami-no-tachi"},
            { 210, "サイカレボルシオ,Saika Revolucio"},
            { 211, "サイカドミナシオ,Saika Dominacio"},
            { 212, "サイカインクルシオ,Saika Inclusio"},
            { 291, "＊レヴァティーン,* Laevateinn"},
            { 501, "プリムカタナ,Primm Katana"},
            { 502, "リサージュカタナ,Resurgir Katana"},
            { 505, "トロワーデカタナ,Trois De Katana"},
            { 506, "フォーシスカタナ,Foursis Katana"},
            { 508, "ツヴィアカタナ,Tzvia Katana"},
            { 509, "エヴォルコートカタナ,Evolcoat Katana"},
            { 514, "フロステルカタナ,Frostel Katana"},
            { 522, "ゴルドプリムカタナ,Gold Primm Katana"},
            { 523, "シルヴァプリムカタナ,Silver Primm Katana"},
            { 526, "ストラーガカタナ,Straga Katana"},
        };
        public static Dictionary<int, string> bowNGSNames = new Dictionary<int, string>()
        {
            { 198, "アーチレボルシオ,Arch Revolucio"},
            { 199, "アーチドミナシオ,Arch Dominacio"},
            { 200, "アーチインクルシオ,Arch Inclusio"},
            { 501, "プリムボウ,Primm Bow"},
            { 502, "リサージュボウ,Resurgir Bow"},
            { 505, "ヴィアルトボウ,Vialto Bow"},
            { 506, "ツヴィアボウ,Tzvia Bow"},
            { 507, "エヴォルコートボウ,Evolcaot Bow"},
            { 508, "テルセウスボウ,Theseus Bow"},
            { 522, "ゴルドプリムボウ,Gold Primm Bow"},
            { 523, "シルヴァプリムボウ,Silver Primm Bow"},
            { 525, "ストラーガボウ,Straga Bow"},
        };
        public static Dictionary<int, string> jetBootsNGSNames = new Dictionary<int, string>()
        {
            { 125, "＊キジカナルカミ,* Kizika Narukami"},
            { 181, "ブレードレボルシオ,Shoes Revolucio"},
            { 182, "ブレードドミナシオ,Shoes Dominacio"},
            { 183, "ブレードインクルシオ,Shoes Inclusio"},
            { 501, "プリムブーツ,Primm Boots"},
            { 503, ","},
        };
        public static Dictionary<int, string> dualBladesNGSNames = new Dictionary<int, string>()
        {
            { 215, "ブレードレボルシオ,Blade Revolucio"},
            { 216, "ブレードドミナシオ,Blade Dominacio"},
            { 217, "ブレードインクルシオ,Blade Inclusio"},
            { 271, "＊レヴァティーン,* Laevateinn"},
            { 501, "プリムブレード,Primm Blade"},
        };
        public static Dictionary<int, string> tactNGSNames = new Dictionary<int, string>()
        {
            { 017, "タクトレボルシオ,Takt Revolucio"},
            { 019, "タクトインクルシオ,Takt Inclusio"}
        };
        
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
