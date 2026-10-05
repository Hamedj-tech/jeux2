using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace JeuxRPG
{
    // ==============================================================
    // MODÈLE DE DONNÉES DU CODEX DES DROPS DE BOSS & RAIDS
    // ==============================================================

    public class DropMateriauDetail
    {
        public string Nom { get; set; } = "";
        public string Icone { get; set; } = "📦";
        public Rarete RareteItem { get; set; } = Rarete.Commun;
        public int QuantiteMin { get; set; } = 1;
        public int QuantiteMax { get; set; } = 3;
        public float TauxDropPourcent { get; set; } = 100f;
        public string Usage { get; set; } = "";
    }

    public class DropEquipementDetail
    {
        public string Nom { get; set; } = "";
        public TypeEquipement Type { get; set; }
        public Rarete RareteItem { get; set; } = Rarete.Legendaire;
        public string IdentifiantSet { get; set; } = "";
        public string NomSet { get; set; } = "";
        public float TauxDropPourcent { get; set; } = 20f;
        public string StatsApercu { get; set; } = "";
        public string EffetSet { get; set; } = "";
    }

    public class FicheDropBoss
    {
        public string Id { get; set; } = "";
        public string NomBoss { get; set; } = "";
        public string SousTitre { get; set; } = "";
        public string Categorie { get; set; } = "Régional"; // Donjon, Régional, Mythique, Cosmique, Tour
        public int NiveauConseille { get; set; } = 1;
        public string Icone { get; set; } = "👑";
        public Color CouleurTheme { get; set; } = Color.FromArgb(231, 76, 60);
        public string Faiblesse { get; set; } = "Aucune";
        public string Lore { get; set; } = "";
        public string CriDeGuerre { get; set; } = "";
        public string ZoneOuLieu { get; set; } = "";

        // Statistiques de base (Difficulté Normale)
        public int PVBase { get; set; }
        public int AttaqueBase { get; set; }
        public int DefenseBase { get; set; }
        public int GainOrBase { get; set; }
        public int GainXPBase { get; set; }
        public int PierresDeForgeBase { get; set; } = 2;

        // Butins & Sets
        public string IdentifiantSet { get; set; } = "set_aethelgard";
        public string NomSet => NomSet2D.ObtenirNom(IdentifiantSet);
        public List<DropMateriauDetail> Materiaux { get; set; } = new List<DropMateriauDetail>();
        public List<DropEquipementDetail> Equipements { get; set; } = new List<DropEquipementDetail>();
        public List<string> SortsAssocies { get; set; } = new List<string>();
        public string ConseilsTactiques { get; set; } = "";

        // Calculs dynamiques selon la difficulté
        public int CalculerPV(DifficulteBoss2D diff) => (int)(PVBase * diff.MultiplicateurPV);
        public int CalculerAttaque(DifficulteBoss2D diff) => (int)(AttaqueBase * diff.MultiplicateurAttaque);
        public int CalculerOr(DifficulteBoss2D diff) => (int)(GainOrBase * diff.MultiplicateurRecompenses);
        public int CalculerXP(DifficulteBoss2D diff) => (int)(GainXPBase * diff.MultiplicateurRecompenses);
        public int CalculerPierres(DifficulteBoss2D diff) => PierresDeForgeBase + diff.PierresBonus;

        public Func<Monstre>? Fabrique { get; set; }

        public HashSet<string> NomsConnus { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public class ResultatLootBoss
    {
        public FicheDropBoss Fiche { get; set; } = null!;
        public Dictionary<string, int> Materiaux { get; set; } = new Dictionary<string, int>();
        public Equipement? PieceEquipement { get; set; }
        public int PierresDeForge { get; set; }
        public int Or { get; set; }
        public int XP { get; set; }
    }

    // ==============================================================
    // CATALOGUE STATIQUE COMPLET DU CODEX DES DROPS (35 FICHES)
    // ==============================================================

    public static class CatalogueCodexDrops
    {
        public static List<FicheDropBoss> ToutesLesFiches { get; } = new List<FicheDropBoss>();

        static CatalogueCodexDrops()
        {
            InitialiserCatalogue();
        }

        private static void InitialiserCatalogue()
        {
            ToutesLesFiches.Clear();

            // -------------------------------------------------------------
            // SECTION 1 : EXPÉDITIONS & RAIDS DE DONJONS (4)
            // -------------------------------------------------------------
            ToutesLesFiches.Add(new FicheDropBoss
            {
                Id = "donjon_foret",
                NomBoss = "🌲 Forêt des Murmures",
                SousTitre = "Expédition Bois Sauvages • Chef Grok le Brise-Crâne",
                Categorie = "Donjon",
                NiveauConseille = 1,
                Icone = "🌲",
                CouleurTheme = Color.FromArgb(39, 174, 96),
                ZoneOuLieu = "Bois des Murmures (Ouest de la Capitale)",
                Faiblesse = "🔥 Feu / Tranchant",
                Lore = "Une forêt dense autrefois paisible, désormais infestée de loups affamés, de sangliers géants et de clans gobelins menés par Grok.",
                CriDeGuerre = "GROK ÉCRASE LES INTRUS !",
                PVBase = 2600,
                AttaqueBase = 32,
                DefenseBase = 16,
                GainOrBase = 250,
                GainXPBase = 450,
                PierresDeForgeBase = 2,
                IdentifiantSet = "set_aethelgard",
                ConseilsTactiques = "Esquivez les charges frontales de Grok. Utilisez le feu pour embraser ses gardes du corps.",
                Materiaux = new List<DropMateriauDetail>
                {
                    new DropMateriauDetail { Nom = "Minerai de Fer", Icone = "⛏️", RareteItem = Rarete.Commun, QuantiteMin = 4, QuantiteMax = 7, TauxDropPourcent = 100f, Usage = "Armes de base & Cuirasses" },
                    new DropMateriauDetail { Nom = "Morceau de Cuir", Icone = "🐗", RareteItem = Rarete.Commun, QuantiteMin = 3, QuantiteMax = 6, TauxDropPourcent = 100f, Usage = "Bottes & protections d'archer" },
                    new DropMateriauDetail { Nom = "Croc Sauvage", Icone = "🦷", RareteItem = Rarete.Rare, QuantiteMin = 2, QuantiteMax = 4, TauxDropPourcent = 85f, Usage = "Dagues dentelées & Anneaux sauvages" },
                    new DropMateriauDetail { Nom = "Cuir Épais", Icone = "🐺", RareteItem = Rarete.Rare, QuantiteMin = 1, QuantiteMax = 3, TauxDropPourcent = 70f, Usage = "Armures renforcées & Fourreaux" }
                },
                Equipements = CreerPiecesSet("set_aethelgard", 1),
                SortsAssocies = new List<string> { "🪓 Lancer de Hache Fracassant", "💣 Bombe Incendiaire Alchimique" },
                Fabrique = () => new BossGrok()
            });

            ToutesLesFiches.Add(new FicheDropBoss
            {
                Id = "donjon_catacombes",
                NomBoss = "💀 Catacombes Oubliées",
                SousTitre = "Cryptes Funèbres • Seigneur Malakor l'Ancien",
                Categorie = "Donjon",
                NiveauConseille = 3,
                Icone = "💀",
                CouleurTheme = Color.FromArgb(142, 68, 173),
                ZoneOuLieu = "Profondeurs de la Crypte Royale",
                Faiblesse = "☀️ Sacré / Foudre",
                Lore = "Les sépultures des rois déchus, réanimées par les sortilèges nécromantiques de Malakor qui draine la vie de ses assaillants.",
                CriDeGuerre = "TON SANG NOURRIRA LES OMBRES !",
                PVBase = 5600,
                AttaqueBase = 44,
                DefenseBase = 20,
                GainOrBase = 480,
                GainXPBase = 850,
                PierresDeForgeBase = 3,
                IdentifiantSet = "set_aethelgard",
                ConseilsTactiques = "Détruisez les orbes vampiriques de Malakor pour l'empêcher de régénérer sa santé. Les sorts sacrés brisent son bouclier.",
                Materiaux = new List<DropMateriauDetail>
                {
                    new DropMateriauDetail { Nom = "Pierre d'Âme", Icone = "🔮", RareteItem = Rarete.Epique, QuantiteMin = 3, QuantiteMax = 5, TauxDropPourcent = 100f, Usage = "Sceptres occultes & Sorts spectraux" },
                    new DropMateriauDetail { Nom = "Ectoplasme", Icone = "👻", RareteItem = Rarete.Epique, QuantiteMin = 4, QuantiteMax = 7, TauxDropPourcent = 100f, Usage = "Bijoux d'affliction & Potions de mana" },
                    new DropMateriauDetail { Nom = "Os Renforcé", Icone = "💀", RareteItem = Rarete.Rare, QuantiteMin = 4, QuantiteMax = 6, TauxDropPourcent = 90f, Usage = "Boucliers osseux & Flèches d'ombre" }
                },
                Equipements = CreerPiecesSet("set_aethelgard", 3),
                SortsAssocies = new List<string> { "👻 Vortex des Âmes Éthérées", "⚡ Orbe de Foudre Arcanique" },
                Fabrique = () => new BossMalakor()
            });

            ToutesLesFiches.Add(new FicheDropBoss
            {
                Id = "donjon_volcan",
                NomBoss = "🌋 Faille du Volcan",
                SousTitre = "Raid Volcanique de Drak'Thar • Ignis Dragon Suprême",
                Categorie = "Donjon",
                NiveauConseille = 7,
                Icone = "🌋",
                CouleurTheme = Color.FromArgb(211, 84, 0),
                ZoneOuLieu = "Cratère Supérieur de Valdorak",
                Faiblesse = "❄️ Givre Absolu / Glace",
                Lore = "L'antre de lave du souverain draconique Ignis. Les fumerolles brûlantes et les cascades de magma calcinent toute créature sans résistance au feu.",
                CriDeGuerre = "BRÛLE DANS LE BRASIER DE VALDORAK !",
                PVBase = 13500,
                AttaqueBase = 70,
                DefenseBase = 32,
                GainOrBase = 950,
                GainXPBase = 1800,
                PierresDeForgeBase = 5,
                IdentifiantSet = "set_dragon",
                ConseilsTactiques = "Équipez-vous du Phénix ou du Portail Glacial pour contrer ses souffles. Roulez hors des zones d'impact de météores.",
                Materiaux = new List<DropMateriauDetail>
                {
                    new DropMateriauDetail { Nom = "Cœur Ardent", Icone = "🔥", RareteItem = Rarete.Legendaire, QuantiteMin = 4, QuantiteMax = 7, TauxDropPourcent = 100f, Usage = "Armes de feu légendaires & Sorts draconiques" },
                    new DropMateriauDetail { Nom = "Écaille Draconique", Icone = "🐉", RareteItem = Rarete.Legendaire, QuantiteMin = 6, QuantiteMax = 10, TauxDropPourcent = 100f, Usage = "Set complet d'Embrasement du Dragon" },
                    new DropMateriauDetail { Nom = "Acier Trempé", Icone = "🛡️", RareteItem = Rarete.Epique, QuantiteMin = 5, QuantiteMax = 8, TauxDropPourcent = 95f, Usage = "Armures d'assaut lourdes (+4 à +7)" }
                },
                Equipements = CreerPiecesSet("set_dragon", 7),
                SortsAssocies = new List<string> { "🐉 Dragon de Flammes Primordial", "🐲 Fureur du Dragon Tricéphale" },
                Fabrique = () => new BossDragonIgnis()
            });

            ToutesLesFiches.Add(new FicheDropBoss
            {
                Id = "donjon_sanctuaire",
                NomBoss = "⭐ Sanctuaire Cosmique",
                SousTitre = "Raid Stellaire • Xanthos du Néant",
                Categorie = "Donjon",
                NiveauConseille = 11,
                Icone = "⭐",
                CouleurTheme = Color.FromArgb(41, 128, 185),
                ZoneOuLieu = "Chambre Astrale au-delà du Voile",
                Faiblesse = "🌌 Éther Pur / Cosmique",
                Lore = "Une dimension dérobée suspendue au cœur des étoiles où Xanthos absorbe la matière pour reforger l'univers dans le vide absolu.",
                CriDeGuerre = "LE NÉANT ABSORBERA VOTRE LUMIÈRE !",
                PVBase = 26000,
                AttaqueBase = 95,
                DefenseBase = 45,
                GainOrBase = 1800,
                GainXPBase = 3500,
                PierresDeForgeBase = 8,
                IdentifiantSet = "set_astral",
                ConseilsTactiques = "Fuyez le centre lorsqu'il déclenche la singularité gravitationnelle. Le Dôme Sacré Céleste vous sauvera des salves lasers.",
                Materiaux = new List<DropMateriauDetail>
                {
                    new DropMateriauDetail { Nom = "Éclat Astral", Icone = "🌌", RareteItem = Rarete.Legendaire, QuantiteMin = 6, QuantiteMax = 10, TauxDropPourcent = 100f, Usage = "Set de la Sentinelle Astrale & Potions astrales" },
                    new DropMateriauDetail { Nom = "Cristal Cosmique", Icone = "🌠", RareteItem = Rarete.Mythique, QuantiteMin = 3, QuantiteMax = 5, TauxDropPourcent = 85f, Usage = "Joyaux de téléportation & Sorts de singularité" },
                    new DropMateriauDetail { Nom = "Matière du Néant", Icone = "🕳️", RareteItem = Rarete.Mythique, QuantiteMin = 3, QuantiteMax = 6, TauxDropPourcent = 80f, Usage = "Sorts occultes & Armes de vide" }
                },
                Equipements = CreerPiecesSet("set_astral", 11),
                SortsAssocies = new List<string> { "🕳️ Singularité Tellurique", "💎 Prisme de Cristal Irisé", "🌙 Croissant Lunaire Irisé" },
                Fabrique = () => new BossXanthos()
            });

            // -------------------------------------------------------------
            // SECTION 2 : LES 10 BOSS RÉGIONAUX (AETHELGARD)
            // -------------------------------------------------------------
            AjouterBossRegional("Grok le Brise-Crâne", "Seigneur Gobelin Brutal", 1, "👑", Color.FromArgb(180, 40, 40),
                "🔥 Feu", "Chef cruel ayant fédéré les clans gobelins du sud.", "GROK VA BRISER TES OS !", 240, 26, 12, 180, 350,
                new (string, int, int, float)[] { ("Minerai de Fer", 5, 8, 100f), ("Morceau de Cuir", 4, 6, 100f), ("Croc Sauvage", 2, 4, 80f) },
                new string[] { "🪓 Lancer de Hache Fracassant" }, () => new BossGrok());

            AjouterBossRegional("Gorrok le Boucher", "Broyeur des Terres Sauvages", 2, "🐗", Color.FromArgb(160, 50, 30),
                "🔥 Tranchant / Feu", "Sanglier colossal enragé ayant éventré des compagnies entières de gardes.", "MON GROIN VA T'ÉCRASER CONTRE LE ROC !", 250, 24, 10, 210, 420,
                new (string, int, int, float)[] { ("Cuir Épais", 4, 7, 100f), ("Croc Sauvage", 3, 5, 90f), ("Minerai de Fer", 3, 5, 75f) },
                new string[] { "🌪️ Trombe des Tempêtes" }, () => new BossGorrok());

            AjouterBossRegional("Kragh l'Écorcheur", "Chef de la Horde Noire", 3, "🗡️", Color.FromArgb(140, 60, 20),
                "✨ Magie Pure", "Assassin gobelin aux lames trempées dans l'acide noir.", "Un coup dans l'ombre et tes boyaux se répandront !", 310, 28, 11, 250, 500,
                new (string, int, int, float)[] { ("Minerai de Fer", 6, 9, 100f), ("Venin Obscur", 2, 4, 85f), ("Cuir Épais", 3, 5, 80f) },
                new string[] { "⚡ Orbe de Foudre Arcanique" }, () => new BossKragh());

            AjouterBossRegional("Malakor le Seigneur", "Nécromancien des Catacombes", 3, "💀", Color.FromArgb(130, 25, 50),
                "☀️ Lumière / Sacré", "Maître des arts occultes réveillant les dépouilles des rois d'antan.", "Vos chairs pourriront dans mes cryptes !", 380, 34, 15, 320, 650,
                new (string, int, int, float)[] { ("Pierre d'Âme", 4, 6, 100f), ("Ectoplasme", 5, 8, 100f), ("Os Renforcé", 4, 6, 85f) },
                new string[] { "👻 Vortex des Âmes Éthérées" }, () => new BossMalakor());

            AjouterBossRegional("Skuldir le Roi Cryo", "Souverain Squelette de Glace", 4, "❄️", Color.FromArgb(30, 80, 130),
                "🔥 Feu Sacré", "Monarque mort-vivant régnant sur les sommets gelés de la chaîne d'argent.", "LE FROID DE LA TOMBE PÉNÈTRERA TES OS !", 410, 32, 15, 320, 640,
                new (string, int, int, float)[] { ("Noyau de Givre", 4, 6, 100f), ("Os Renforcé", 5, 7, 100f), ("Ectoplasme", 3, 5, 80f) },
                new string[] { "🧊 Portail Glacial de l'Aurore", "🪞 Miroir de Glace Mystique" }, () => new BossSkuldir());

            AjouterBossRegional("Zulgar le Chaman", "Maître Putride des Marais", 5, "🧪", Color.FromArgb(46, 125, 50),
                "⚡ Foudre / Sacré", "Chaman orc manipulant les poisons les plus fétides des marécages de Valdor.", "LES ESPRITS ANCIENS ME RÉCLAMENT TON ÂME !", 490, 35, 16, 380, 750,
                new (string, int, int, float)[] { ("Venin Obscur", 4, 6, 100f), ("Ectoplasme", 4, 6, 90f), ("Cuir Épais", 4, 6, 80f) },
                new string[] { "👻 Vortex des Âmes Éthérées" }, () => new BossZulgar());

            AjouterBossRegional("Vespera la Tisseuse", "Matriarche des Harpies", 5, "🦅", Color.FromArgb(142, 68, 173),
                "🔥 Feu / Foudre", "Reine des créatures ailées tissant des toiles toxiques dans les falaises venteuses.", "Mes serres vont t'arracher les yeux, ver de terre !", 520, 38, 16, 420, 820,
                new (string, int, int, float)[] { ("Plume Aérienne", 5, 8, 100f), ("Venin Obscur", 3, 5, 85f), ("Cuir Épais", 4, 6, 80f) },
                new string[] { "🌪️ Trombe des Tempêtes" }, () => new BossVespera());

            AjouterBossRegional("Kaelas l'Archifée", "Gardienne du Blizzard Éternel", 6, "✨", Color.FromArgb(41, 128, 185),
                "🔥 Feu Draconique", "Entité féerique corrompue gelant quiconque foule les prairies boréales.", "Endors-toi dans le linceul de glace éternelle...", 580, 42, 18, 480, 920,
                new (string, int, int, float)[] { ("Noyau de Givre", 5, 8, 100f), ("Pierre d'Âme", 4, 6, 100f), ("Éclat Astral", 2, 4, 70f) },
                new string[] { "🦅 Phénix de Givre Céleste", "❄️ Léviathan des Glaces" }, () => new BossKaelas());

            AjouterBossRegional("Balthazar le Déchu", "Chevalier Noir Vampire", 6, "⚔️", Color.FromArgb(52, 73, 94),
                "☀️ Lumière Purificatrice", "Paladin réprouvé assoiffé du sang des champions royaux.", "Ta lame tremble face au maître de la nuit !", 640, 45, 20, 520, 1050,
                new (string, int, int, float)[] { ("Acier Trempé", 5, 8, 100f), ("Pierre d'Âme", 5, 7, 100f), ("Minerai de Fer", 7, 10, 90f) },
                new string[] { "🛡️ Dôme Sacré Céleste" }, () => new BossBalthazar());

            AjouterBossRegional("Kryll de Nécrose", "Arachnide Chitinique Titanesque", 7, "🦂", Color.FromArgb(121, 85, 72),
                "🔥 Feu / Foudre", "Scorpion titanesque fouillant les sables noirs avec un dard dégoulinant de poison.", "SKRRR ! TA CHAIR SERA DISSOUSTE !", 720, 48, 22, 580, 1200,
                new (string, int, int, float)[] { ("Venin Obscur", 5, 8, 100f), ("Os Renforcé", 6, 9, 100f), ("Pierre d'Âme", 4, 6, 85f) },
                new string[] { "👻 Vortex des Âmes Éthérées" }, () => new BossKryll());

            // -------------------------------------------------------------
            // SECTION 3 : LES 10 BOSS MYTHIQUES & VOLCANIQUES (DRAGON)
            // -------------------------------------------------------------
            AjouterBossMythique("Magmarion le Colosse", "Titan de Lave Éveillé", 7, "🌋", Color.FromArgb(190, 50, 10),
                "❄️ Givre Pur", "Géant né du cœur du volcan Valdorak marchant sur la roche en fusion.", "LA TERRE BRÛLE SOUS MES PAS !", 850, 52, 24, 650, 1400,
                new (string, int, int, float)[] { ("Cœur Ardent", 4, 6, 100f), ("Écaille Draconique", 5, 8, 100f), ("Acier Trempé", 5, 7, 90f) },
                new string[] { "🐉 Dragon de Flammes Primordial" }, () => new BossMagmarion());

            AjouterBossMythique("Sylvana des Ronces", "Reine Sépulcrale Primordiale", 8, "🌲", Color.FromArgb(35, 100, 50),
                "🔥 Feu Purificateur", "Esprit millénaire des grands arbres corrompu par la nécromancie.", "Les racines étoufferont ton dernier souffle !", 920, 54, 25, 720, 1550,
                new (string, int, int, float)[] { ("Cuir Épais", 6, 9, 100f), ("Pierre d'Âme", 5, 8, 100f), ("Ectoplasme", 5, 8, 85f) },
                new string[] { "🌪️ Trombe des Tempêtes" }, () => new BossSylvana());

            AjouterBossMythique("Obsidius le Titan", "Colosse de Verre Noir Volcanique", 8, "🪨", Color.FromArgb(60, 50, 60),
                "⚡ Foudre / Choc", "Golem forgé dans le verre noir impénétrable des entrailles de Drak'Thar.", "BRISÉ ! TU SERAS BROYÉ COMME LE VERRE !", 1050, 58, 28, 800, 1700,
                new (string, int, int, float)[] { ("Cœur de Titan", 3, 5, 100f), ("Cœur Ardent", 4, 6, 100f), ("Acier Trempé", 6, 9, 90f) },
                new string[] { "🕳️ Singularité Tellurique" }, () => new BossObsidius());

            AjouterBossMythique("Ignis Dragon Suprême", "Monarque Millénaire de Valdorak", 8, "🐉", Color.FromArgb(180, 60, 10),
                "❄️ Glace Polaire", "Le souverain suprême des dragons de feu régnant sur la caldeira.", "VALDORAK NE S'ÉTEINDRA JAMAIS !", 1200, 64, 30, 950, 2000,
                new (string, int, int, float)[] { ("Cœur Ardent", 6, 9, 100f), ("Écaille Draconique", 7, 11, 100f), ("Acier Trempé", 6, 8, 95f) },
                new string[] { "🐉 Dragon de Flammes Primordial", "🐲 Fureur du Dragon Tricéphale" }, () => new BossDragonIgnis());

            AjouterBossMythique("Nox le Spectre", "Dévoreur de Toute Lumière", 9, "🌑", Color.FromArgb(45, 30, 65),
                "☀️ Lumière Solaire", "Entité fantomatique consumant les étoiles dans les abysses de la nuit.", "Dans l'obscurité, nul ne t'entendra crier...", 1100, 62, 26, 880, 1900,
                new (string, int, int, float)[] { ("Matière du Néant", 3, 5, 100f), ("Ectoplasme", 7, 10, 100f), ("Pierre d'Âme", 6, 8, 90f) },
                new string[] { "🌙 Croissant Lunaire Irisé", "👁️ Portail de l'Abysse Eldritch" }, () => new BossNox());

            AjouterBossMythique("Bélial le Démon", "Seigneur des Flammes Noires", 9, "🔥", Color.FromArgb(160, 30, 10),
                "❄️ Glace Sacrée", "Archi-démon invoqué par les cultistes du néant pour ravager la capitale.", "VOS ÂMES BRÛLERONT EN ENFER !", 1250, 68, 30, 1050, 2200,
                new (string, int, int, float)[] { ("Sang de Démon", 4, 7, 100f), ("Cœur Ardent", 5, 8, 100f), ("Écaille Draconique", 6, 9, 90f) },
                new string[] { "🐉 Dragon de Flammes Primordial" }, () => new BossBelial());

            AjouterBossMythique("Général Thalor", "Guerrier Immortel Sans-Tête", 10, "⚔️", Color.FromArgb(140, 90, 15),
                "✨ Sacré / Éther", "Ancien général suprême maudit condamné à errer avec sa lame sanglante.", "MA LAME N'A JAMAIS CONNU LA DÉFAITE !", 1350, 72, 32, 1150, 2400,
                new (string, int, int, float)[] { ("Acier Trempé", 8, 12, 100f), ("Os Renforcé", 7, 10, 100f), ("Pierre d'Âme", 5, 7, 85f) },
                new string[] { "🛡️ Dôme Sacré Céleste" }, () => new BossThalor());

            AjouterBossMythique("Mor'Gath la Liche", "Nécromancienne Démoniaque", 10, "💀", Color.FromArgb(90, 20, 90),
                "☀️ Lumière Astrale", "Sorcier antique ayant troqué son humanité contre l'immortalité squelettique.", "La mort n'est qu'un commencement pour moi !", 1400, 74, 32, 1250, 2600,
                new (string, int, int, float)[] { ("Sang de Démon", 4, 6, 100f), ("Pierre d'Âme", 7, 10, 100f), ("Ectoplasme", 7, 10, 100f) },
                new string[] { "👻 Vortex des Âmes Éthérées", "👁️ Portail de l'Abysse Eldritch" }, () => new BossMorGath());

            AjouterBossMythique("Azkalith la Vipère", "Reine des Abysses Oubliées", 10, "🐍", Color.FromArgb(25, 110, 70),
                "⚡ Foudre / Sacré", "Serpent géant reptilien crachant des torrents de venin corrosif.", "Ssshh... Goûte au baiser de la mort !", 1450, 76, 34, 1300, 2750,
                new (string, int, int, float)[] { ("Venin Obscur", 6, 9, 100f), ("Sang de Démon", 4, 6, 90f), ("Pierre d'Âme", 5, 8, 85f) },
                new string[] { "👻 Vortex des Âmes Éthérées" }, () => new BossAzkalith());

            AjouterBossMythique("Valdorak le Colosse", "Titan de Granite Primordial", 11, "🪨", Color.FromArgb(70, 80, 90),
                "🌌 Énergie Cosmique", "Montagne vivante dont chaque pas déclenche des séismes apocalyptiques.", "RIEN NE PEUT BRISER LE ROC PRIMORDIAL !", 1600, 80, 38, 1450, 3000,
                new (string, int, int, float)[] { ("Cœur de Titan", 5, 8, 100f), ("Acier Trempé", 8, 12, 100f), ("Écaille Draconique", 6, 9, 90f) },
                new string[] { "🕳️ Singularité Tellurique" }, () => new BossValdorak());

            // -------------------------------------------------------------
            // SECTION 4 : LES 10 BOSS COSMIQUES & RAIDS STELLAIRES (ASTRAL)
            // -------------------------------------------------------------
            AjouterBossCosmique("Zephyros Ouragan", "Archi-Seigneur des Tempêtes", 11, "⚡", Color.FromArgb(30, 110, 140),
                "🪨 Tellurique", "Maître des vents célestes déchaînant la foudre en chaîne sur les imprudents.", "QUE LES CIEUX S'EFFONDRENT !", 1750, 84, 38, 1600, 3300,
                new (string, int, int, float)[] { ("Éclat Astral", 6, 9, 100f), ("Plume Aérienne", 6, 9, 100f), ("Acier Trempé", 6, 9, 90f) },
                new string[] { "🌪️ Trombe des Tempêtes", "⚡ Orbe de Foudre Arcanique" }, () => new BossZephyros());

            AjouterBossCosmique("Fenrir Lune de Sang", "Loup Ancestral Primordial", 12, "🐺", Color.FromArgb(150, 20, 25),
                "🔥 Argent / Feu", "Bête mythologique géante dont la morsure fissure la trame de l'espace.", "LA LUNE DE SANG EXIGE TON SANG !", 1900, 88, 40, 1750, 3600,
                new (string, int, int, float)[] { ("Croc Sauvage", 8, 12, 100f), ("Cuir Épais", 8, 12, 100f), ("Éclat Astral", 5, 8, 90f) },
                new string[] { "🌙 Croissant Lunaire Irisé" }, () => new BossFenrirLuneSang());

            AjouterBossCosmique("Chevalier du Néant", "Entité Primordiale de l'Oubli", 12, "🌌", Color.FromArgb(90, 15, 45),
                "☀️ Lumière Divine", "Guerrier spectral né dans le grand rien avant la création des étoiles.", "L'Oubli est votre seule délivrance...", 2050, 92, 42, 1900, 3900,
                new (string, int, int, float)[] { ("Matière du Néant", 5, 8, 100f), ("Éclat Astral", 6, 9, 100f), ("Acier Trempé", 7, 10, 90f) },
                new string[] { "🕳️ Singularité Tellurique" }, () => new BossChevalierDuNeant());

            AjouterBossCosmique("L'Archonte Solaire", "Flamme Céleste Purificatrice", 13, "☀️", Color.FromArgb(180, 120, 15),
                "❄️ Glace / Néant", "Guerrier angélique irradiant d'une lumière solaire aveuglante et sacrée.", "QUE LA LUMIÈRE PURIFIE VOTRE ÂME !", 2250, 96, 44, 2100, 4300,
                new (string, int, int, float)[] { ("Cristal Cosmique", 4, 6, 100f), ("Cœur Ardent", 6, 9, 100f), ("Éclat Astral", 7, 10, 95f) },
                new string[] { "🛡️ Dôme Sacré Céleste", "💎 Prisme de Cristal Irisé" }, () => new BossArchonteSolaire());

            AjouterBossCosmique("Kraken des Abîmes", "Terreur Céphalopode Ancestrale", 14, "🐙", Color.FromArgb(15, 60, 90),
                "⚡ Foudre Divine", "Monstre marin colossal aux tentacules broyant les navires et les réalités.", "GLOIRE AUX PROFONDEURS ÉTERNELLES !", 2500, 100, 46, 2300, 4800,
                new (string, int, int, float)[] { ("Matière du Néant", 5, 8, 100f), ("Venin Obscur", 6, 9, 100f), ("Éclat Astral", 7, 10, 90f) },
                new string[] { "👁️ Portail de l'Abysse Eldritch" }, () => new BossKrakenAbyssal());

            AjouterBossCosmique("Xanthos le Gardien", "Dévoreur Stellaire du Néant", 15, "🌌", Color.FromArgb(110, 45, 130),
                "🌌 Magie Primordiale", "Seigneur du Sanctuaire Cosmique absorbant des systèmes stellaires entiers.", "NUL N'ÉCHAPPE AU NÉANT ABSOLU !", 2800, 105, 50, 2600, 5400,
                new (string, int, int, float)[] { ("Cristal Cosmique", 6, 9, 100f), ("Matière du Néant", 5, 8, 100f), ("Éclat Astral", 8, 12, 100f) },
                new string[] { "🕳️ Singularité Tellurique", "💎 Prisme de Cristal Irisé", "🌙 Croissant Lunaire Irisé" }, () => new BossXanthos());

            AjouterBossCosmique("Chronos du Temps", "Maître des Sabliers Brisés", 15, "⏳", Color.FromArgb(135, 105, 30),
                "🌀 Chaos Pur", "Archi-mage du temps manipulant les flux temporels et annulant vos attaques.", "LE TEMPS S'ARRÊTE À MON COMMANDEMENT !", 3100, 110, 52, 2900, 6000,
                new (string, int, int, float)[] { ("Cristal Cosmique", 6, 9, 100f), ("Éclat Astral", 8, 12, 100f), ("Acier Trempé", 8, 12, 90f) },
                new string[] { "🌙 Croissant Lunaire Irisé", "💎 Prisme de Cristal Irisé" }, () => new BossChronos());

            AjouterBossCosmique("Léviathan Stellaire", "Fléau des Galaxies Oubliées", 16, "☄️", Color.FromArgb(70, 25, 110),
                "⚡ Foudre Cosmique", "Serpent d'éther boréal fendant les galaxies et gelant la réalité.", "LE SILENCE DES ÉTOILES T'ENGHIUTIRA !", 3500, 118, 55, 3300, 6800,
                new (string, int, int, float)[] { ("Cristal Cosmique", 7, 10, 100f), ("Noyau de Givre", 8, 12, 100f), ("Éclat Astral", 9, 13, 100f) },
                new string[] { "❄️ Léviathan des Glaces", "🦅 Phénix de Givre Céleste" }, () => new BossLeviathanStellaire());

            AjouterBossCosmique("Abaddon Pourfendeur", "Destructeur de Réalités", 18, "💀", Color.FromArgb(130, 15, 15),
                "✨ Harmonie Céleste", "Entité démoniaque du chaos suprême brisant les dimensions d'un seul coup.", "TOUTE EXISTENCE SERA ANÉANTIE !", 4200, 128, 60, 4000, 8200,
                new (string, int, int, float)[] { ("Essence du Chaos", 4, 7, 100f), ("Matière du Néant", 7, 11, 100f), ("Éclat Astral", 10, 15, 100f) },
                new string[] { "👁️ Portail de l'Abysse Eldritch", "🐲 Fureur du Dragon Tricéphale" }, () => new BossAbaddon());

            AjouterBossCosmique("Deus Ex Nihilo", "Architecte Suprême du Chaos", 20, "👑", Color.FromArgb(110, 10, 60),
                "⚔️ Volonté Pure du Héros", "Le créateur et destructeur primordial de l'univers d'Aethelgard. L'ultime défi.", "JE SUIS L'ALPHA ET L'OMÉGA DE CE MONDE !", 5500, 145, 68, 5500, 12000,
                new (string, int, int, float)[] { ("Essence du Chaos", 7, 11, 100f), ("Cristal Cosmique", 8, 12, 100f), ("Éclat Astral", 12, 18, 100f) },
                new string[] { "Toutes les 14 compétences débloquées & optimisées" }, () => new BossDeusExNihilo());

            // -------------------------------------------------------------
            // SECTION 5 : LA TOUR ASTRALE INFINIE
            // -------------------------------------------------------------
            ToutesLesFiches.Add(new FicheDropBoss
            {
                Id = "tour_astrale",
                NomBoss = "🗼 Tour Astrale Infinie",
                SousTitre = "Paliers de Boss tous les 5 étages • Récompenses Évolutives",
                Categorie = "Tour",
                NiveauConseille = 1,
                Icone = "🗼",
                CouleurTheme = Color.FromArgb(155, 89, 182),
                ZoneOuLieu = "Flèche Céleste d'Aethelgard",
                Faiblesse = "Adaptez vos éléments selon le boss du palier",
                Lore = "Une tour sans fin s'élevant vers les confins du cosmos. Tous les 5 étages, un Boss majeur garde le palier avec des trésors garantis.",
                CriDeGuerre = "NUL NE MONTERA PLUS HAUT !",
                PVBase = 1200,
                AttaqueBase = 35,
                DefenseBase = 15,
                GainOrBase = 300,
                GainXPBase = 500,
                PierresDeForgeBase = 3,
                IdentifiantSet = "set_astral",
                ConseilsTactiques = "Sécurisez votre butin via le portail de repli si votre mana ou vos potions faiblissent.",
                Materiaux = new List<DropMateriauDetail>
                {
                    new DropMateriauDetail { Nom = "Éclat Astral", Icone = "🌌", RareteItem = Rarete.Legendaire, QuantiteMin = 3, QuantiteMax = 8, TauxDropPourcent = 100f, Usage = "Pièces du Set Astral & Sorts de haut niveau" },
                    new DropMateriauDetail { Nom = "Cristal Cosmique", Icone = "🌠", RareteItem = Rarete.Mythique, QuantiteMin = 2, QuantiteMax = 5, TauxDropPourcent = 75f, Usage = "Améliorations supérieures & crafts ultimes" },
                    new DropMateriauDetail { Nom = "Pierres de Forge", Icone = "💎", RareteItem = Rarete.Epique, QuantiteMin = 3, QuantiteMax = 8, TauxDropPourcent = 100f, Usage = "Amélioration de pièces (+1 à +10)" }
                },
                Equipements = CreerPiecesSet("set_astral", 5),
                SortsAssocies = new List<string> { "🌙 Croissant Lunaire Irisé", "💎 Prisme de Cristal Irisé" }
            });

            // Enregistrer tous les alias et noms connus pour chaque fiche
            var ficheTourAstrale = ToutesLesFiches.FirstOrDefault(f => f.Id == "tour_astrale");
            if (ficheTourAstrale != null)
            {
                string[] gardiensTour = new[]
                {
                    "Sentinelle Stellaire", "Ombre du Vide", "Traqueur de Nova",
                    "Colosse Céleste", "Annihilateur de Réalité", "Gardien de l'Éther",
                    "Guerrier d'Anti-Matière", "Spectre des Galaxies", "Écho Stellaire",
                    "Tour Astrale", "Tour Astrale Infinie"
                };
                foreach (var g in gardiensTour)
                {
                    ficheTourAstrale.NomsConnus.Add(g);
                    ficheTourAstrale.NomsConnus.Add(NettoyerChaine(g));
                }
            }

            foreach (var f in ToutesLesFiches)
            {
                f.NomsConnus.Add(f.Id);
                f.NomsConnus.Add(f.NomBoss);
                f.NomsConnus.Add(NettoyerChaine(f.NomBoss));
                if (!string.IsNullOrWhiteSpace(f.SousTitre))
                {
                    f.NomsConnus.Add(f.SousTitre);
                    f.NomsConnus.Add(NettoyerChaine(f.SousTitre));
                }

                if (f.Fabrique != null)
                {
                    try
                    {
                        var m = f.Fabrique();
                        if (m != null && !string.IsNullOrWhiteSpace(m.Nom))
                        {
                            f.NomsConnus.Add(m.Nom);
                            f.NomsConnus.Add(NettoyerChaine(m.Nom));
                            int virgule = m.Nom.IndexOf(',');
                            if (virgule > 0)
                            {
                                string racine = m.Nom.Substring(0, virgule).Trim();
                                f.NomsConnus.Add(racine);
                                f.NomsConnus.Add(NettoyerChaine(racine));
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private static void AjouterBossRegional(string nom, string titre, int niv, string icone, Color coul,
            string faiblesse, string lore, string cri, int pv, int atk, int def, int or, int xp,
            (string nom, int min, int max, float taux)[] mats, string[] sorts, Func<Monstre> fabrique)
        {
            var f = new FicheDropBoss
            {
                Id = "reg_" + nom.ToLowerInvariant().Replace(" ", "_").Replace("'", ""),
                NomBoss = nom,
                SousTitre = $"{titre} (Niveau {niv}+)",
                Categorie = "Régional",
                NiveauConseille = niv,
                Icone = icone,
                CouleurTheme = coul,
                Faiblesse = faiblesse,
                Lore = lore,
                CriDeGuerre = cri,
                ZoneOuLieu = "Région d'Aethelgard",
                PVBase = pv,
                AttaqueBase = atk,
                DefenseBase = def,
                GainOrBase = or,
                GainXPBase = xp,
                PierresDeForgeBase = 2,
                IdentifiantSet = "set_aethelgard",
                ConseilsTactiques = $"Exploitez sa faiblesse ({faiblesse}). Roulez en dash pour esquiver ses attaques.",
                Equipements = CreerPiecesSet("set_aethelgard", niv),
                SortsAssocies = sorts.ToList(),
                Fabrique = fabrique
            };
            foreach (var m in mats)
            {
                f.Materiaux.Add(new DropMateriauDetail
                {
                    Nom = m.nom,
                    Icone = ObtenirIconeMateriau(m.nom),
                    RareteItem = m.taux >= 90f ? Rarete.Rare : Rarete.Epique,
                    QuantiteMin = m.min,
                    QuantiteMax = m.max,
                    TauxDropPourcent = m.taux,
                    Usage = ObtenirUsageMateriau(m.nom)
                });
            }
            ToutesLesFiches.Add(f);
        }

        private static void AjouterBossMythique(string nom, string titre, int niv, string icone, Color coul,
            string faiblesse, string lore, string cri, int pv, int atk, int def, int or, int xp,
            (string nom, int min, int max, float taux)[] mats, string[] sorts, Func<Monstre> fabrique)
        {
            var f = new FicheDropBoss
            {
                Id = "myth_" + nom.ToLowerInvariant().Replace(" ", "_").Replace("'", ""),
                NomBoss = nom,
                SousTitre = $"{titre} (Niveau {niv}+)",
                Categorie = "Mythique",
                NiveauConseille = niv,
                Icone = icone,
                CouleurTheme = coul,
                Faiblesse = faiblesse,
                Lore = lore,
                CriDeGuerre = cri,
                ZoneOuLieu = "Domaine Volcanique & Sanctuaires Mythiques",
                PVBase = pv,
                AttaqueBase = atk,
                DefenseBase = def,
                GainOrBase = or,
                GainXPBase = xp,
                PierresDeForgeBase = 4,
                IdentifiantSet = "set_dragon",
                ConseilsTactiques = $"Frappez avec des attaques d'élément ({faiblesse}). Ne restez jamais statique.",
                Equipements = CreerPiecesSet("set_dragon", niv),
                SortsAssocies = sorts.ToList(),
                Fabrique = fabrique
            };
            foreach (var m in mats)
            {
                f.Materiaux.Add(new DropMateriauDetail
                {
                    Nom = m.nom,
                    Icone = ObtenirIconeMateriau(m.nom),
                    RareteItem = Rarete.Legendaire,
                    QuantiteMin = m.min,
                    QuantiteMax = m.max,
                    TauxDropPourcent = m.taux,
                    Usage = ObtenirUsageMateriau(m.nom)
                });
            }
            ToutesLesFiches.Add(f);
        }

        private static void AjouterBossCosmique(string nom, string titre, int niv, string icone, Color coul,
            string faiblesse, string lore, string cri, int pv, int atk, int def, int or, int xp,
            (string nom, int min, int max, float taux)[] mats, string[] sorts, Func<Monstre> fabrique)
        {
            var f = new FicheDropBoss
            {
                Id = "cosm_" + nom.ToLowerInvariant().Replace(" ", "_").Replace("'", ""),
                NomBoss = nom,
                SousTitre = $"{titre} (Niveau {niv}+)",
                Categorie = "Cosmique",
                NiveauConseille = niv,
                Icone = icone,
                CouleurTheme = coul,
                Faiblesse = faiblesse,
                Lore = lore,
                CriDeGuerre = cri,
                ZoneOuLieu = "Plan Céleste & Galaxies Lointaines",
                PVBase = pv,
                AttaqueBase = atk,
                DefenseBase = def,
                GainOrBase = or,
                GainXPBase = xp,
                PierresDeForgeBase = 6,
                IdentifiantSet = "set_astral",
                ConseilsTactiques = $"Le timing d'esquive est primordial. Gardez votre Ultime pour ses phases d'enrage.",
                Equipements = CreerPiecesSet("set_astral", niv),
                SortsAssocies = sorts.ToList(),
                Fabrique = fabrique
            };
            foreach (var m in mats)
            {
                f.Materiaux.Add(new DropMateriauDetail
                {
                    Nom = m.nom,
                    Icone = ObtenirIconeMateriau(m.nom),
                    RareteItem = Rarete.Mythique,
                    QuantiteMin = m.min,
                    QuantiteMax = m.max,
                    TauxDropPourcent = m.taux,
                    Usage = ObtenirUsageMateriau(m.nom)
                });
            }
            ToutesLesFiches.Add(f);
        }

        private static List<DropEquipementDetail> CreerPiecesSet(string identifiantSet, int niveau)
        {
            int facteurNiveau = 1 + (Math.Clamp(niveau, 1, 20) - 1) / 4;
            string nomSet = NomSet2D.ObtenirNom(identifiantSet);
            string theme = identifiantSet switch
            {
                "set_dragon" => "du Dragon",
                "set_astral" => "Astral",
                _ => "d'Aethelgard"
            };

            string bonusSet = identifiantSet switch
            {
                "set_dragon" => "2P: +8 ATK • 4P: +20 ATK, +120 PV • 5P: +30 ATK, Aura de Flammes",
                "set_astral" => "2P: +12 DEF • 4P: +22 DEF, +15 ATK, +150 PV • 5P: -25% Dégâts Subis",
                _ => "2P: +8 ATK/DEF • 4P: +20 ATK, +12 DEF, +120 PV • 5P: Champion Protecteur"
            };

            return new List<DropEquipementDetail>
            {
                new DropEquipementDetail
                {
                    Nom = $"Lame {theme}",
                    Type = TypeEquipement.Arme,
                    RareteItem = Rarete.Legendaire,
                    IdentifiantSet = identifiantSet,
                    NomSet = nomSet,
                    TauxDropPourcent = 20f,
                    StatsApercu = $"⚔️ Attaque +{14 * facteurNiveau}  •  🎯 Critique +5%",
                    EffetSet = bonusSet
                },
                new DropEquipementDetail
                {
                    Nom = $"Cuirasse {theme}",
                    Type = TypeEquipement.Armure,
                    RareteItem = Rarete.Legendaire,
                    IdentifiantSet = identifiantSet,
                    NomSet = nomSet,
                    TauxDropPourcent = 20f,
                    StatsApercu = $"🛡️ Défense +{12 * facteurNiveau}  •  ❤️ PV +{24 * facteurNiveau}",
                    EffetSet = bonusSet
                },
                new DropEquipementDetail
                {
                    Nom = $"Heaume {theme}",
                    Type = TypeEquipement.Casque,
                    RareteItem = Rarete.Legendaire,
                    IdentifiantSet = identifiantSet,
                    NomSet = nomSet,
                    TauxDropPourcent = 20f,
                    StatsApercu = $"🛡️ Défense +{7 * facteurNiveau}  •  💨 Esquive +3%",
                    EffetSet = bonusSet
                },
                new DropEquipementDetail
                {
                    Nom = $"Anneau {theme}",
                    Type = TypeEquipement.Anneau,
                    RareteItem = Rarete.Legendaire,
                    IdentifiantSet = identifiantSet,
                    NomSet = nomSet,
                    TauxDropPourcent = 20f,
                    StatsApercu = $"⚔️ Attaque +{5 * facteurNiveau}  •  🩸 Vampirisme +3%",
                    EffetSet = bonusSet
                },
                new DropEquipementDetail
                {
                    Nom = $"Amulette {theme}",
                    Type = TypeEquipement.Amulette,
                    RareteItem = Rarete.Legendaire,
                    IdentifiantSet = identifiantSet,
                    NomSet = nomSet,
                    TauxDropPourcent = 20f,
                    StatsApercu = $"🛡️ Défense +{2 * facteurNiveau}  •  💧 Mana +{12 * facteurNiveau}",
                    EffetSet = bonusSet
                }
            };
        }

        public static string ObtenirIconeMateriau(string nom) => nom switch
        {
            "Minerai de Fer" => "⛏️",
            "Morceau de Cuir" => "🐗",
            "Cuir Épais" => "🐺",
            "Croc Sauvage" => "🦷",
            "Os Renforcé" => "💀",
            "Ectoplasme" => "👻",
            "Pierre d'Âme" => "🔮",
            "Acier Trempé" => "🛡️",
            "Noyau de Givre" => "❄️",
            "Plume Aérienne" => "🦅",
            "Venin Obscur" => "🧪",
            "Écaille Draconique" => "🐉",
            "Cœur Ardent" => "🔥",
            "Sang de Démon" => "🩸",
            "Cœur de Titan" => "🗿",
            "Éclat Astral" => "🌌",
            "Cristal Cosmique" => "🌠",
            "Matière du Néant" => "🕳️",
            "Essence du Chaos" => "👁️",
            _ => "📦"
        };

        public static string ObtenirUsageMateriau(string nom) => nom switch
        {
            "Minerai de Fer" => "Armes en fer, armures forgées chez Brom",
            "Morceau de Cuir" => "Bottes, poignées et renforts d'armures",
            "Cuir Épais" => "Armures légères et capes d'assaut",
            "Croc Sauvage" => "Dagues perçantes et pendentifs féroces",
            "Os Renforcé" => "Boucliers d'ossements et arcs renforcés",
            "Ectoplasme" => "Bâtons occultes et potions de mana",
            "Pierre d'Âme" => "Anneaux d'ensorcellement et sorts d'âmes",
            "Acier Trempé" => "Armes royales (+3 à +6) et cuirasses lourdes",
            "Noyau de Givre" => "Sorts et armes de givre polaire",
            "Plume Aérienne" => "Flèches perçantes et sort Trombe des Tempêtes",
            "Venin Obscur" => "Potions d'empoisonnement et lames corrompues",
            "Écaille Draconique" => "Set complet de l'Embrasement du Dragon",
            "Cœur Ardent" => "Sorts de wyrm de feu et lames incandescentes",
            "Sang de Démon" => "Armes vampiriques et rituels d'assaut",
            "Cœur de Titan" => "Marteaux sismiques et cuirasses de granite",
            "Éclat Astral" => "Set complet de la Sentinelle Astrale",
            "Cristal Cosmique" => "Lasers prismatiques et orbes stellaires",
            "Matière du Néant" => "Sorts de trou noir et lames de l'oubli",
            "Essence du Chaos" => "Recettes mythiques ultimes de Maître Kaëlith",
            _ => "Composant d'artisanat & de forge"
        };

        public static string NettoyerChaine(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            string sansCrochets = System.Text.RegularExpressions.Regex.Replace(s, @"\[.*?\]|\(.*?\)", " ");
            var sb = new System.Text.StringBuilder();
            foreach (char c in sansCrochets)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                {
                    char norm = c switch
                    {
                        'é' or 'è' or 'ê' or 'ë' => 'e',
                        'É' or 'È' or 'Ê' or 'Ë' => 'e',
                        'à' or 'â' or 'ä' => 'a',
                        'À' or 'Â' or 'Ä' => 'a',
                        'î' or 'ï' => 'i',
                        'Î' or 'Ï' => 'i',
                        'ô' or 'ö' => 'o',
                        'Ô' or 'Ö' => 'o',
                        'ù' or 'û' or 'ü' => 'u',
                        'Ù' or 'Û' or 'Ü' => 'u',
                        'ç' or 'Ç' => 'c',
                        _ => char.ToLowerInvariant(c)
                    };
                    sb.Append(norm);
                }
                else if (c == '\'' || c == '-')
                {
                    sb.Append(' ');
                }
            }
            return System.Text.RegularExpressions.Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }

        public static FicheDropBoss? TrouverFiche(string? nom, string? nomZone = null)
        {
            if (string.IsNullOrWhiteSpace(nom)) return null;

            if (!string.IsNullOrWhiteSpace(nomZone))
            {
                string zPropre = NettoyerChaine(nomZone);
                if (zPropre.Contains("foret") && (nom.Contains("Grok") || nom.Contains("foret", StringComparison.OrdinalIgnoreCase)))
                    return ToutesLesFiches.FirstOrDefault(f => f.Id == "donjon_foret");
                if (zPropre.Contains("catacombe") && (nom.Contains("Malakor") || nom.Contains("catacombe", StringComparison.OrdinalIgnoreCase)))
                    return ToutesLesFiches.FirstOrDefault(f => f.Id == "donjon_catacombes");
                if (zPropre.Contains("volcan") && (nom.Contains("Ignis") || nom.Contains("volcan", StringComparison.OrdinalIgnoreCase)))
                    return ToutesLesFiches.FirstOrDefault(f => f.Id == "donjon_volcan");
                if (zPropre.Contains("cosmique") && (nom.Contains("Xanthos") || nom.Contains("cosmique", StringComparison.OrdinalIgnoreCase)))
                    return ToutesLesFiches.FirstOrDefault(f => f.Id == "donjon_cosmique");
                if (zPropre.Contains("tour"))
                    return ToutesLesFiches.FirstOrDefault(f => f.Id == "tour_astrale");
            }

            string brut = nom.Trim();
            string propre = NettoyerChaine(nom);

            // 1. Recherche exacte dans NomsConnus
            foreach (var f in ToutesLesFiches)
            {
                if (f.NomsConnus.Contains(brut) || f.NomsConnus.Contains(propre))
                    return f;
            }

            // 2. Recherche par sous-chaîne dans NomsConnus
            foreach (var f in ToutesLesFiches)
            {
                foreach (var alias in f.NomsConnus)
                {
                    if (alias.Length >= 4 && (propre.Contains(alias) || alias.Contains(propre)))
                        return f;
                }
            }

            // 3. Mots-clés discriminants des boss
            string[] motsCles = new[]
            {
                "deus ex nihilo", "abaddon", "leviathan", "chronos", "xanthos", "kraken",
                "archonte", "chevalier du neant", "fenrir", "zephyros", "valdorak",
                "azkalith", "morgath", "mor gath", "thalor", "belial", "nox", "ignis",
                "obsidius", "sylvana", "magmarion", "kryll", "balthazar", "kaelas",
                "vespera", "zulgar", "skuldir", "malakor", "kragh", "gorrok", "grok"
            };

            foreach (var mot in motsCles)
            {
                if (propre.Contains(mot))
                {
                    var trouve = ToutesLesFiches.FirstOrDefault(f => f.NomsConnus.Any(a => a.Contains(mot)));
                    if (trouve != null) return trouve;
                }
            }

            // 4. Donjons & Tour
            if (propre.Contains("foret")) return ToutesLesFiches.FirstOrDefault(f => f.Id == "donjon_foret");
            if (propre.Contains("catacombe")) return ToutesLesFiches.FirstOrDefault(f => f.Id == "donjon_catacombes");
            if (propre.Contains("volcan")) return ToutesLesFiches.FirstOrDefault(f => f.Id == "donjon_volcan");
            if (propre.Contains("sanctuaire") || propre.Contains("cosmique")) return ToutesLesFiches.FirstOrDefault(f => f.Id == "donjon_cosmique");
            if (propre.Contains("tour")) return ToutesLesFiches.FirstOrDefault(f => f.Id == "tour_astrale");

            return null;
        }

        public static int ObtenirVictoiresBoss(Joueur? hero, FicheDropBoss fiche)
        {
            if (hero?.BestiaireMonstresTues == null || fiche == null) return 0;
            int total = 0;
            string nomNettoye = NettoyerChaine(fiche.NomBoss);
            foreach (var kvp in hero.BestiaireMonstresTues)
            {
                string cleNettoyee = NettoyerChaine(kvp.Key);
                if (cleNettoyee == nomNettoye ||
                    (nomNettoye.Length >= 4 && cleNettoyee.Contains(nomNettoye)) ||
                    (cleNettoyee.Length >= 4 && nomNettoye.Contains(cleNettoyee)) ||
                    fiche.NomsConnus.Any(alias => alias.Length >= 4 && cleNettoyee.Contains(NettoyerChaine(alias))))
                {
                    total += kvp.Value;
                }
            }
            return total;
        }

        public static FicheDropBoss? TrouverParNom(string nom)
        {
            return TrouverFiche(nom);
        }

        public static Dictionary<string, int> TirerMateriaux(FicheDropBoss fiche, Random rng)
        {
            var resultat = new Dictionary<string, int>();
            foreach (var mat in fiche.Materiaux)
            {
                double roll = rng.NextDouble() * 100.0;
                if (roll <= mat.TauxDropPourcent)
                {
                    int qte = mat.QuantiteMin == mat.QuantiteMax
                        ? mat.QuantiteMin
                        : rng.Next(mat.QuantiteMin, mat.QuantiteMax + 1);
                    if (qte > 0)
                    {
                        resultat[mat.Nom] = qte;
                    }
                }
            }
            return resultat;
        }

        public static Equipement GenererPieceSet(FicheDropBoss fiche, int niveauJoueur, Random rng)
        {
            TypeEquipement type = (TypeEquipement)rng.Next(5);
            return GenererPieceSet(fiche.IdentifiantSet, type, niveauJoueur);
        }

        public static Equipement GenererPieceSet(string identifiantSet, TypeEquipement type, int niveauJoueur)
        {
            int niveau = Math.Clamp(niveauJoueur, 1, 20);
            int facteurNiveau = 1 + (niveau - 1) / 4;
            string theme = identifiantSet switch
            {
                "set_dragon" => "du Dragon",
                "set_astral" => "Astral",
                _ => "d'Aethelgard"
            };

            string nomPiece = type switch
            {
                TypeEquipement.Arme => $"Lame {theme}",
                TypeEquipement.Armure => $"Cuirasse {theme}",
                TypeEquipement.Casque => $"Heaume {theme}",
                TypeEquipement.Anneau => $"Anneau {theme}",
                _ => $"Amulette {theme}"
            };

            Rarete rarete = identifiantSet == "set_astral" ? Rarete.Mythique : Rarete.Legendaire;

            var piece = new Equipement(
                nomPiece,
                type,
                rarete,
                type == TypeEquipement.Arme ? 14 * facteurNiveau : type == TypeEquipement.Anneau ? 5 * facteurNiveau : 0,
                type == TypeEquipement.Armure ? 12 * facteurNiveau : type == TypeEquipement.Casque ? 7 * facteurNiveau : type == TypeEquipement.Amulette ? 2 * facteurNiveau : 0,
                type == TypeEquipement.Armure ? 24 * facteurNiveau : 0,
                type == TypeEquipement.Amulette ? 12 * facteurNiveau : 0,
                type == TypeEquipement.Arme ? 5 : 0,
                type == TypeEquipement.Casque ? 3 : 0,
                type == TypeEquipement.Anneau ? 3 : 0,
                450 * facteurNiveau);

            piece.IdentifiantSet = identifiantSet;
            return piece;
        }

        public static ResultatLootBoss GenererLootBossComplet(FicheDropBoss fiche, DifficulteBoss2D difficulte, int niveauJoueur, Random rng)
        {
            return new ResultatLootBoss
            {
                Fiche = fiche,
                Materiaux = TirerMateriaux(fiche, rng),
                PieceEquipement = GenererPieceSet(fiche, niveauJoueur, rng),
                PierresDeForge = fiche.CalculerPierres(difficulte),
                Or = fiche.CalculerOr(difficulte),
                XP = fiche.CalculerXP(difficulte)
            };
        }

        public static ResultatLootBoss? GenererLootBossComplet(string nomBoss, DifficulteBoss2D difficulte, int niveauJoueur, Random rng, string? nomZone = null)
        {
            var fiche = TrouverFiche(nomBoss, nomZone);
            if (fiche == null) return null;
            return GenererLootBossComplet(fiche, difficulte, niveauJoueur, rng);
        }
    }

    // ==============================================================
    // FENÊTRE GRAPHIQUE INTERACTIVE DU CODEX DES DROPS DE BOSS / RAIDS
    // ==============================================================

    public class FormCodexDropsBoss : Form
    {
        private readonly Joueur hero;
        private readonly Action<Monstre, DifficulteBoss2D>? onLancerCombat;
        private readonly Action<FicheDropBoss, DifficulteBoss2D>? onLancerDefi;

        private DifficulteBoss2D difficulteActive = DifficulteBoss2D.Normale;
        private string categorieActive = "Tout";
        private string termeRecherche = "";

        private List<FicheDropBoss> fichesAffichees = new List<FicheDropBoss>();
        private FicheDropBoss? ficheSelectionnee;

        private ListBox lbBoss = null!;
        private TextBox txtRecherche = null!;
        private Panel pnlDetails = null!;
        private FlowLayoutPanel pnlDiffButtons = null!;

        public FormCodexDropsBoss(Joueur joueur, Action<FicheDropBoss, DifficulteBoss2D>? actionLancerDefi, string? bossPrefere = null)
        {
            hero = joueur;
            onLancerDefi = actionLancerDefi;
            onLancerCombat = null;

            Text = "📖 Codex des Drops & Butins de Boss, Raids & Donjons";
            Size = new Size(1020, 720);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(18, 20, 26);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            ConstruireInterface();

            if (!string.IsNullOrEmpty(bossPrefere))
            {
                var trouve = CatalogueCodexDrops.TrouverParNom(bossPrefere);
                if (trouve != null)
                {
                    SelectionnerFiche(trouve);
                }
            }
        }

        public FormCodexDropsBoss(Joueur joueur, Action<Monstre, DifficulteBoss2D>? actionLancer = null, string? bossPrefere = null)
            : this(joueur, actionLancer != null ? (fiche, diff) =>
            {
                if (fiche.Fabrique != null) actionLancer(fiche.Fabrique(), diff);
            } : (Action<FicheDropBoss, DifficulteBoss2D>?)null, bossPrefere)
        {
            onLancerCombat = actionLancer;
        }

        private void ConstruireInterface()
        {
            // 1. En-tête
            Panel pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1020, 72),
                BackColor = Color.FromArgb(24, 27, 36)
            };

            Label lblTitre = new Label
            {
                Text = "📖 CODEX DES DROPS & BUTINS — BOSS, RAIDS & EXPÉDITIONS",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(20, 10),
                AutoSize = true
            };

            Label lblSousTitre = new Label
            {
                Text = "Consultez les tables de butin garanties, pièces de sets légendaires, composants de forge et taux de drop de chaque défi !",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(170, 180, 200),
                Location = new Point(20, 36),
                AutoSize = true
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitre, lblSousTitre });

            // 2. Barre d'outils (Difficulté + Recherche + Catégories)
            Panel pnlBarreOutils = new Panel
            {
                Location = new Point(20, 78),
                Size = new Size(965, 48),
                BackColor = Color.Transparent
            };

            Label lblDiffTitre = new Label
            {
                Text = "Difficulté :",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.LightGray,
                Location = new Point(0, 14),
                AutoSize = true
            };

            pnlDiffButtons = new FlowLayoutPanel
            {
                Location = new Point(75, 8),
                Size = new Size(460, 36),
                BackColor = Color.Transparent
            };

            foreach (var diff in DifficulteBoss2D.Toutes)
            {
                Button btnD = new Button
                {
                    Text = diff.Titre,
                    Height = 30,
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = (diff == difficulteActive) ? diff.CouleurBadge : Color.FromArgb(32, 35, 46),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Tag = diff,
                    Margin = new Padding(0, 0, 6, 0)
                };
                btnD.FlatAppearance.BorderSize = 0;
                btnD.Click += (s, e) =>
                {
                    difficulteActive = diff;
                    ActualiserBoutonsDifficulte();
                    RafraichirDetailsFiche();
                };
                pnlDiffButtons.Controls.Add(btnD);
            }

            // Champ de recherche
            Label lblRecherche = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI", 11f),
                Location = new Point(540, 12),
                AutoSize = true
            };

            txtRecherche = new TextBox
            {
                Location = new Point(568, 11),
                Size = new Size(390, 26),
                BackColor = Color.FromArgb(28, 31, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                PlaceholderText = "Rechercher par nom, set (Dragon/Astral), élément (Feu/Givre)..."
            };
            txtRecherche.TextChanged += (s, e) =>
            {
                termeRecherche = txtRecherche.Text.Trim().ToLowerInvariant();
                FiltrerListe();
            };

            pnlBarreOutils.Controls.AddRange(new Control[] { lblDiffTitre, pnlDiffButtons, lblRecherche, txtRecherche });

            // 3. Barre de catégories
            FlowLayoutPanel pnlCategories = new FlowLayoutPanel
            {
                Location = new Point(20, 130),
                Size = new Size(965, 34),
                BackColor = Color.Transparent
            };

            string[] categories = { "✨ Tout (35)", "🌲 Donjons (4)", "👑 Régionaux (10)", "🔥 Mythiques (10)", "🌌 Cosmiques (10)", "🗼 Tour Astrale" };
            string[] tagsCat = { "Tout", "Donjon", "Régional", "Mythique", "Cosmique", "Tour" };

            for (int i = 0; i < categories.Length; i++)
            {
                string tag = tagsCat[i];
                Button btnC = new Button
                {
                    Text = categories[i],
                    Height = 28,
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = (tag == categorieActive) ? Color.FromArgb(41, 128, 185) : Color.FromArgb(32, 35, 46),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Tag = tag,
                    Margin = new Padding(0, 0, 6, 0)
                };
                btnC.FlatAppearance.BorderSize = 0;
                btnC.Click += (s, e) =>
                {
                    categorieActive = tag;
                    foreach (Control c in pnlCategories.Controls)
                    {
                        if (c is Button b)
                        {
                            bool actif = (b.Tag as string) == categorieActive;
                            b.BackColor = actif ? Color.FromArgb(41, 128, 185) : Color.FromArgb(32, 35, 46);
                        }
                    }
                    FiltrerListe();
                };
                pnlCategories.Controls.Add(btnC);
            }

            // 4. Liste de gauche (Boss & Donjons)
            lbBoss = new ListBox
            {
                Location = new Point(20, 172),
                Size = new Size(330, 485),
                BackColor = Color.FromArgb(24, 27, 36),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f),
                ItemHeight = 26
            };
            lbBoss.SelectedIndexChanged += (s, e) =>
            {
                if (lbBoss.SelectedIndex >= 0 && lbBoss.SelectedIndex < fichesAffichees.Count)
                {
                    ficheSelectionnee = fichesAffichees[lbBoss.SelectedIndex];
                    RafraichirDetailsFiche();
                }
            };

            // 5. Panneau de droite (Fiche détaillée du Boss)
            pnlDetails = new Panel
            {
                Location = new Point(365, 172),
                Size = new Size(620, 485),
                BackColor = Color.FromArgb(22, 25, 34),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            Controls.AddRange(new Control[] { pnlHeader, pnlBarreOutils, pnlCategories, lbBoss, pnlDetails });

            FiltrerListe();
        }

        private void ActualiserBoutonsDifficulte()
        {
            foreach (Control c in pnlDiffButtons.Controls)
            {
                if (c is Button b && b.Tag is DifficulteBoss2D d)
                {
                    b.BackColor = (d == difficulteActive) ? d.CouleurBadge : Color.FromArgb(32, 35, 46);
                }
            }
        }

        private void FiltrerListe()
        {
            fichesAffichees = CatalogueCodexDrops.ToutesLesFiches.Where(f =>
            {
                bool matchCat = (categorieActive == "Tout") || (f.Categorie == categorieActive);
                if (!matchCat) return false;

                if (string.IsNullOrEmpty(termeRecherche)) return true;

                bool matchNom = f.NomBoss.ToLowerInvariant().Contains(termeRecherche);
                bool matchSous = f.SousTitre.ToLowerInvariant().Contains(termeRecherche);
                bool matchSet = f.NomSet.ToLowerInvariant().Contains(termeRecherche);
                bool matchFaible = f.Faiblesse.ToLowerInvariant().Contains(termeRecherche);
                bool matchMat = f.Materiaux.Any(m => m.Nom.ToLowerInvariant().Contains(termeRecherche));

                return matchNom || matchSous || matchSet || matchFaible || matchMat;
            }).ToList();

            lbBoss.BeginUpdate();
            lbBoss.Items.Clear();
            foreach (var f in fichesAffichees)
            {
                int victoires = hero.BestiaireMonstresTues.TryGetValue(f.NomBoss, out int v) ? v : 0;
                string badgeVaincu = victoires > 0 ? $"🏆x{victoires}" : "○";
                lbBoss.Items.Add($"{f.Icone} {f.NomBoss} (Niv. {f.NiveauConseille}) {badgeVaincu}");
            }
            lbBoss.EndUpdate();

            if (fichesAffichees.Count > 0)
            {
                lbBoss.SelectedIndex = 0;
            }
            else
            {
                ficheSelectionnee = null;
                pnlDetails.Controls.Clear();
                Label lblVide = new Label
                {
                    Text = "Aucun Boss ou Donjon ne correspond à votre recherche.",
                    ForeColor = Color.Gray,
                    Location = new Point(20, 20),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 10f, FontStyle.Italic)
                };
                pnlDetails.Controls.Add(lblVide);
            }
        }

        private void SelectionnerFiche(FicheDropBoss cible)
        {
            int idx = fichesAffichees.IndexOf(cible);
            if (idx >= 0)
            {
                lbBoss.SelectedIndex = idx;
            }
            else
            {
                categorieActive = "Tout";
                termeRecherche = "";
                txtRecherche.Text = "";
                FiltrerListe();
                idx = fichesAffichees.IndexOf(cible);
                if (idx >= 0) lbBoss.SelectedIndex = idx;
            }
        }

        private void RafraichirDetailsFiche()
        {
            if (ficheSelectionnee == null) return;
            var f = ficheSelectionnee;

            pnlDetails.SuspendLayout();
            pnlDetails.Controls.Clear();

            int curY = 12;

            // --- CARTE D'IDENTITÉ DU BOSS ---
            Panel pnlCarteIdentite = new Panel
            {
                Location = new Point(14, curY),
                Size = new Size(585, 125),
                BackColor = Color.FromArgb(28, 31, 42),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblNom = new Label
            {
                Text = $"{f.Icone} {f.NomBoss}",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = f.CouleurTheme,
                Location = new Point(12, 8),
                AutoSize = true
            };

            int victoires = CatalogueCodexDrops.ObtenirVictoiresBoss(hero, f);
            Label lblBadgeVictoires = new Label
            {
                Text = victoires > 0 ? $"🏆 VAINCU {victoires} FOIS" : "⚠️ JAMAIS VAINCU",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = victoires > 0 ? Color.Gold : Color.OrangeRed,
                Location = new Point(440, 11),
                AutoSize = true
            };

            Label lblSous = new Label
            {
                Text = $"{f.SousTitre}  •  {f.ZoneOuLieu}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(180, 190, 210),
                Location = new Point(12, 33),
                Size = new Size(550, 18)
            };

            // Stats selon difficulté
            int pv = f.CalculerPV(difficulteActive);
            int atk = f.CalculerAttaque(difficulteActive);
            int def = f.DefenseBase;
            int or = f.CalculerOr(difficulteActive);
            int xp = f.CalculerXP(difficulteActive);
            int pierres = f.CalculerPierres(difficulteActive);

            Label lblStatsLigne = new Label
            {
                Text = $"❤️ PV: {pv}   ⚔️ ATK: {atk}   🛡️ DEF: {def}   |   [{difficulteActive.Titre}]",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = difficulteActive.CouleurBadge,
                Location = new Point(12, 56),
                AutoSize = true
            };

            Label lblRecompensesLigne = new Label
            {
                Text = $"💰 Butin Fixe : +{or} 🪙 Or   •   +{xp} ✨ XP   •   +{pierres} 💎 Pierres de Forge",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(12, 78),
                AutoSize = true
            };

            Label lblFaiblesse = new Label
            {
                Text = $"⚡ Faiblesse : {f.Faiblesse}",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(235, 150, 50),
                Location = new Point(12, 100),
                AutoSize = true
            };

            pnlCarteIdentite.Controls.AddRange(new Control[] { lblNom, lblBadgeVictoires, lblSous, lblStatsLigne, lblRecompensesLigne, lblFaiblesse });
            pnlDetails.Controls.Add(pnlCarteIdentite);
            curY += 135;

            // --- SECTION 1 : PIÈCES DE SETS LÉGENDAIRES ---
            Label lblTitreSet = new Label
            {
                Text = $"⚔️ BUTIN D'ÉQUIPEMENT — SET « {f.NomSet.ToUpperInvariant()} » (100% GARANTI : 1 PIÈCE PAR VICTOIRE)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 156, 18),
                Location = new Point(14, curY),
                AutoSize = true
            };
            pnlDetails.Controls.Add(lblTitreSet);
            curY += 22;

            Panel pnlSet = new Panel
            {
                Location = new Point(14, curY),
                Size = new Size(585, 115),
                BackColor = Color.FromArgb(26, 28, 38),
                BorderStyle = BorderStyle.FixedSingle
            };

            int py = 6;
            foreach (var eq in f.Equipements)
            {
                string iconePiece = eq.Type switch
                {
                    TypeEquipement.Arme => "🗡️",
                    TypeEquipement.Armure => "🛡️",
                    TypeEquipement.Casque => "🪖",
                    TypeEquipement.Anneau => "💍",
                    _ => "📿"
                };

                Label lblPiece = new Label
                {
                    Text = $"{iconePiece} {eq.Nom} ({eq.RareteItem}) : {eq.StatsApercu}  [Taux {eq.TauxDropPourcent:0}%]",
                    Font = new Font("Segoe UI", 8.3f),
                    ForeColor = Color.FromArgb(250, 215, 160),
                    Location = new Point(10, py),
                    AutoSize = true
                };
                pnlSet.Controls.Add(lblPiece);
                py += 17;
            }

            if (f.Equipements.Count > 0)
            {
                Label lblEffetSet = new Label
                {
                    Text = $"✨ Bonus d'Ensemble : {f.Equipements[0].EffetSet}",
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    ForeColor = Color.Gold,
                    Location = new Point(10, py + 2),
                    AutoSize = true
                };
                pnlSet.Controls.Add(lblEffetSet);
            }

            pnlDetails.Controls.Add(pnlSet);
            curY += 125;

            // --- SECTION 2 : MATÉRIAUX D'ARTISANAT ---
            Label lblTitreMats = new Label
            {
                Text = "📦 MATÉRIAUX D'ARTISANAT & INGRÉDIENTS DE FORGE",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                Location = new Point(14, curY),
                AutoSize = true
            };
            pnlDetails.Controls.Add(lblTitreMats);
            curY += 22;

            Panel pnlMats = new Panel
            {
                Location = new Point(14, curY),
                Size = new Size(585, Math.Max(70, f.Materiaux.Count * 22 + 10)),
                BackColor = Color.FromArgb(26, 28, 38),
                BorderStyle = BorderStyle.FixedSingle
            };

            int my = 6;
            foreach (var m in f.Materiaux)
            {
                Label lblMat = new Label
                {
                    Text = $"{m.Icone} {m.Nom} x{m.QuantiteMin}-{m.QuantiteMax}  •  Taux : {m.TauxDropPourcent:0}%  •  Usage : {m.Usage}",
                    Font = new Font("Segoe UI", 8.3f),
                    ForeColor = Color.FromArgb(190, 220, 245),
                    Location = new Point(10, my),
                    AutoSize = true
                };
                pnlMats.Controls.Add(lblMat);
                my += 20;
            }

            pnlDetails.Controls.Add(pnlMats);
            curY += pnlMats.Height + 10;

            // --- SECTION 3 : SORTS & GRIMOIRE ASSOCIÉS ---
            if (f.SortsAssocies.Count > 0)
            {
                Label lblTitreSorts = new Label
                {
                    Text = "🔮 SORTS MAGIQUES & COMPOSANTS ASSOCIÉS",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(155, 89, 182),
                    Location = new Point(14, curY),
                    AutoSize = true
                };
                pnlDetails.Controls.Add(lblTitreSorts);
                curY += 22;

                Panel pnlSorts = new Panel
                {
                    Location = new Point(14, curY),
                    Size = new Size(585, Math.Max(40, f.SortsAssocies.Count * 20 + 8)),
                    BackColor = Color.FromArgb(26, 28, 38),
                    BorderStyle = BorderStyle.FixedSingle
                };

                int sy = 6;
                foreach (var s in f.SortsAssocies)
                {
                    Label lblSort = new Label
                    {
                        Text = $"✨ {s} (Fabricable chez Maître Kaëlith grâce aux matériaux de ce Boss)",
                        Font = new Font("Segoe UI", 8.3f),
                        ForeColor = Color.FromArgb(220, 190, 250),
                        Location = new Point(10, sy),
                        AutoSize = true
                    };
                    pnlSorts.Controls.Add(lblSort);
                    sy += 18;
                }

                pnlDetails.Controls.Add(pnlSorts);
                curY += pnlSorts.Height + 10;
            }

            // --- SECTION 4 : LORE & CONSEIL TACTIQUE ---
            Label lblTitreLore = new Label
            {
                Text = "📜 LORE & CONSEILS TACTIQUES",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(149, 165, 166),
                Location = new Point(14, curY),
                AutoSize = true
            };
            pnlDetails.Controls.Add(lblTitreLore);
            curY += 22;

            RichTextBox rtbLore = new RichTextBox
            {
                Location = new Point(14, curY),
                Size = new Size(585, 75),
                BackColor = Color.FromArgb(24, 26, 36),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Font = new Font("Segoe UI", 8.5f),
                Text = $"\"{f.Lore}\"\n\n📢 Cri de guerre : \"{f.CriDeGuerre}\"\n💡 Tactique : {f.ConseilsTactiques}"
            };
            pnlDetails.Controls.Add(rtbLore);
            curY += 85;

            // --- BOUTON D'ACTION DIRECTE (COMBAT / DONJON / TOUR) ---
            bool peutLancer = (onLancerDefi != null || onLancerCombat != null) && (f.Fabrique != null || f.Id == "tour_astrale" || f.Categorie == "Donjon");
            if (peutLancer)
            {
                string texteBouton;
                if (f.Id == "tour_astrale")
                {
                    texteBouton = $"🗼 ENTRER DANS LA TOUR ASTRALE [Palier 1+]";
                }
                else if (f.Categorie == "Donjon")
                {
                    texteBouton = $"⚔️ LANCER L'EXPÉDITION : {f.NomBoss.ToUpperInvariant()} [{difficulteActive.Titre}]";
                }
                else
                {
                    texteBouton = $"⚔️ DÉFIER {f.NomBoss.ToUpperInvariant()} MAINTENANT [{difficulteActive.Titre}]";
                }

                Button btnDefier = new Button
                {
                    Text = texteBouton,
                    Location = new Point(14, curY),
                    Size = new Size(585, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = f.CouleurTheme,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnDefier.FlatAppearance.BorderSize = 0;
                btnDefier.Click += (s, e) =>
                {
                    Close();
                    if (onLancerDefi != null)
                    {
                        onLancerDefi(f, difficulteActive);
                    }
                    else if (onLancerCombat != null && f.Fabrique != null)
                    {
                        onLancerCombat(f.Fabrique(), difficulteActive);
                    }
                };
                pnlDetails.Controls.Add(btnDefier);
                curY += 50;
            }

            pnlDetails.ResumeLayout();
        }
    }
}
