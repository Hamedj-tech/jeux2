using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace JeuxRPG
{
    // ==============================================================
    // 6. ENNEMIS ET BOSS ÉPIQUES
    // ==============================================================
    public class Monstre
    {
        public string Nom { get; set; } = "Monstre";
        public int PVMax { get; set; }
        public int PVActuels { get; set; }
        public int Attaque { get; set; }
        public int Defense { get; set; }
        public int GainXP { get; set; }
        public int GainOr { get; set; }
        public bool EstBoss { get; set; }
        public string CriDeGuerre { get; set; } = "";
        public string Faiblesse { get; set; } = "Aucune";
        public string Lore { get; set; } = "";
        public int NiveauRecommande { get; set; } = 1;

        public Monstre() { }

        public Monstre(string nom, int pv, int atk, int def, int xp, int or, bool estBoss = false, string cri = "", string faiblesse = "Aucune", string lore = "")
        {
            Nom = nom;
            PVMax = pv;
            PVActuels = pv;
            Attaque = atk;
            Defense = def;
            GainXP = xp;
            GainOr = or;
            EstBoss = estBoss;
            CriDeGuerre = cri;
            Faiblesse = faiblesse;
            Lore = lore;
        }

        public virtual int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            int variance = rng.Next(-3, 4);
            int degatsBruts = Math.Max(2, (Attaque + variance) - joueur.DefenseTotale);

            // Chance de coup critique de l'ennemi (15%)
            if (rng.Next(100) < 15)
            {
                degatsBruts = (int)(degatsBruts * 1.5);
                messageAction = $"💥 {Nom} porte un COUP CRITIQUE dévastateur et inflige {degatsBruts} dégâts !";
            }
            else
            {
                messageAction = $"⚔️ {Nom} vous assène un coup et inflige {degatsBruts} dégâts.";
            }

            joueur.PVActuels = Math.Max(0, joueur.PVActuels - degatsBruts);
            return degatsBruts;
        }

        public virtual Dictionary<string, int> ObtenirLootMateriaux()
        {
            var fiche = CatalogueCodexDrops.TrouverFiche(Nom);
            if (fiche != null && fiche.Materiaux.Count > 0)
            {
                return CatalogueCodexDrops.TirerMateriaux(fiche, Random.Shared);
            }

            Dictionary<string, int> loot = new Dictionary<string, int>();
            Random rng = Random.Shared;

            if (Nom.Contains("Gobelin"))
            {
                loot["Minerai de Fer"] = rng.Next(1, 3);
                if (rng.Next(100) < 60) loot["Morceau de Cuir"] = 1;
            }
            else if (Nom.Contains("Loup") || Nom.Contains("Sanglier") || Nom.Contains("Bête"))
            {
                loot["Cuir Épais"] = rng.Next(1, 3);
                if (rng.Next(100) < 50) loot["Croc Sauvage"] = 1;
            }
            else if (Nom.Contains("Squelette"))
            {
                loot["Os Renforcé"] = rng.Next(1, 3);
                if (rng.Next(100) < 40) loot["Minerai de Fer"] = 1;
            }
            else if (Nom.Contains("Spectre") || Nom.Contains("Ombre"))
            {
                loot["Ectoplasme"] = rng.Next(1, 3);
                if (rng.Next(100) < 40) loot["Pierre d'Âme"] = 1;
            }
            else if (Nom.Contains("Golem"))
            {
                loot["Acier Trempé"] = rng.Next(1, 2);
                loot["Noyau de Givre"] = 1;
            }
            else if (Nom.Contains("Drake") || Nom.Contains("Dragon"))
            {
                loot["Écaille Draconique"] = rng.Next(1, 3);
                loot["Cœur Ardent"] = 1;
            }
            else if (Nom.Contains("Stellaire") || Nom.Contains("Astral") || Nom.Contains("Cosmique"))
            {
                loot["Éclat Astral"] = rng.Next(1, 3);
            }
            else
            {
                loot["Minerai de Fer"] = 1;
            }

            return loot;
        }
    }

    // --- BOSS 1 : GROK LE SEIGNEUR GOBELIN ---
    public class BossGrok : Monstre
    {
        public bool Enrage { get; set; } = false;

        public BossGrok() : base("Grok, Seigneur de la Horde Gobeline", 240, 26, 12, 350, 180, true,
            "GROK VA RÉDUIRE TES OS EN BOUILLIE ET TE JETER AUX LOUPS !", "Feu", "Chef brutal des clans gobelins ayant pillé les routes royales.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (!Enrage && PVActuels <= PVMax * 0.45)
            {
                Enrage = true;
                Attaque += 8;
                messageAction = $"😡 {Nom} entre dans une FRÉNÉSIE ENRAGÉE ! Ses yeux s'injectent de sang et son attaque augmente grandement !";
                return 0;
            }

            int action = rng.Next(100);
            if (action < 30)
            {
                int degats = Math.Max(12, (Attaque + 8) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🔨 {Nom} abat sa lourde massue cloutée dans un impact fracassant ! {degats} dégâts !";
                return degats;
            }
            else if (action < 50)
            {
                int degats = Math.Max(8, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🐗 {Nom} appelle un gobelin kamikaze qui explose à vos pieds ! {degats} dégâts !";
                return degats;
            }
            else
            {
                int degats = Math.Max(6, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚔️ {Nom} vous taillade sauvagement avec sa lame rouillée pour {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 2 : MALAKOR LE NÉCROMANCIEN ---
    public class BossMalakor : Monstre
    {
        public bool BouclierOmbreActif { get; set; } = false;

        public BossMalakor() : base("Malakor, Seigneur de la Mort Éternelle", 380, 34, 15, 650, 320, true,
            "Vos chairs pourriront, et votre âme rejoindra ma légion de spectres !", "Lumière / Sacré", "Ancien mage de la cour ayant succombé aux arts nécromantiques interdits.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            int action = rng.Next(100);

            if (action < 30) // Drain d'âme vampirique
            {
                int degats = Math.Max(18, 38 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                int soin = degats;
                PVActuels = Math.Min(PVMax, PVActuels + soin);
                messageAction = $"💀 {Nom} canalise un VOILE MORTEL qui vous siphonne {degats} PV et le régénère de {soin} PV !";
                return degats;
            }
            else if (action < 60) // Éclair d'ombre noire
            {
                int degats = Math.Max(14, (Attaque + 6) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚡ {Nom} foudroie votre esprit avec un Éclair Nécrotique ! {degats} dégâts arcaniques sombres !";
                return degats;
            }
            else
            {
                int degats = Math.Max(8, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🗡️ {Nom} projette une volée d'éclats d'os tranchants ! {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 3 : KAELAS, REINE DU GIVRE ÉTERNEL ---
    public class BossKaelas : Monstre
    {
        public BossKaelas() : base("Kaelas, Archifée du Blizzard Éternel", 520, 42, 20, 1100, 500, true,
            "Sentez le froid glacial engourdir vos veines... Vous dormirez à jamais dans la glace !", "Feu", "Entité glaciale millénaire régnant sur les pics boréaux.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            int action = rng.Next(100);

            if (action < 35) // Tempête de blizzard
            {
                int degats = Math.Max(22, 50 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"❄️ {Nom} déchaîne un BLIZZARD MEURTRIER ! Des lances de glace vous transpercent pour {degats} dégâts !";
                return degats;
            }
            else if (action < 65) // Morsure du zéro absolu
            {
                int degats = Math.Max(16, (Attaque + 10) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🧊 {Nom} gèle le sol sous vos pieds et crée une déflagration de givre ! {degats} dégâts !";
                return degats;
            }
            else
            {
                int degats = Math.Max(10, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚔️ {Nom} frappe avec son sceptre de diamant polaire pour {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 4 : IGNIS LE DRAGON MILLÉNAIRE ---
    public class BossDragonIgnis : Monstre
    {
        public bool EnRage { get; set; } = false;

        public BossDragonIgnis() : base("Ignis, Dragon Millénaire Suprême", 750, 50, 24, 1800, 850, true,
            "UN RUGISSEMENT QUI DÉCHIRE LE CIEL ! DES CENDRES ARDENTES TOMBENT EN PLUIE SUR LE MONDE !", "Glace", "Fléau ancestral réveillé des entrailles du volcan d'Aethelgard.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;

            if (!EnRage && PVActuels <= PVMax * 0.40)
            {
                EnRage = true;
                Attaque += 15;
                messageAction = $"🔥 {Nom} DÉPLOIE SES AILES GIGANTESQUES ! La montagne s'embrase et sa puissance devient CATACLYSMIQUE !";
                return 0;
            }

            int action = rng.Next(100);
            if (action < 35)
            {
                int degats = Math.Max(30, 65 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🌪️ {Nom} déverse un SOUFFLE DE FLAMMES INFERNALES ! Tout est consumé ! {degats} dégâts brûlants !";
                return degats;
            }
            else if (action < 65)
            {
                int degats = Math.Max(20, (Attaque + 12) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"💥 {Nom} s'élève et s'abat sur vous dans un séisme titanesque ! {degats} dégâts de choc !";
                return degats;
            }
            else
            {
                int degats = Math.Max(14, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🦷 {Nom} broie vos armures avec ses crocs d'obsidienne ! {degats} dégâts !";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS SECRET & ULTIME : XANTHOS L'AVATAR DU NÉANT ---
    public class BossXanthos : Monstre
    {
        public BossXanthos() : base("Xanthos, Dévoreur Stellaire du Néant", 1100, 62, 30, 3500, 2000, true,
            "L'ESPACE ET LE TEMPS SE TORDENT... VOTRE MONDE S'ÉTEINT DANS LE VIDE ÉTERNEL !", "Cosmique / Pureté", "Le Souverain Cosmique tapi au sommet de la Tour Astrale.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            int action = rng.Next(100);

            if (action < 35) // Trou noir dimensionnel
            {
                int degats = Math.Max(38, 75 - (joueur.DefenseTotale / 3));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🌌 {Nom} ouvre une FAILLE DANS LE NÉANT ! L'énergie gravitationnelle déchire la réalité pour {degats} dégâts colossaux !";
                return degats;
            }
            else if (action < 65) // Distorsion temporelle
            {
                int degats = Math.Max(25, (Attaque + 15) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                joueur.ManaActuel = Math.Max(0, joueur.ManaActuel - 25);
                messageAction = $"⏳ {Nom} déforme le cours du temps : {degats} dégâts et vous perdez 25 Mana !";
                return degats;
            }
            else
            {
                int degats = Math.Max(18, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"👁️ Le regard cosmique de {Nom} vous inflige {degats} dégâts d'agonie psychique !";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 6 : BELIAL LE SEIGNEUR DÉMONIAQUE ---
    public class BossBelial : Monstre
    {
        public bool Enrage { get; set; } = false;
        public bool ChargeMeteore { get; set; } = false;

        public BossBelial() : base("Belial, Seigneur Démoniaque des Flammes Noires", 980, 58, 26, 2600, 1300, true,
            "LES FLAMMES DE L'ABÎME CONSUMERONT VOTRE CHAIR ET VOTRE ESPRIT !", "Glace / Eau", "Démon primordial invoqué des fosses ardentes.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;

            if (ChargeMeteore)
            {
                ChargeMeteore = false;
                int degatsMeteore = Math.Max(45, 95 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degatsMeteore);
                messageAction = $"☄️ MÉTÉORE DE L'APOCALYPSE ! Belial s'écrase dans une explosion infernale ! {degatsMeteore} DÉGÂTS CATACLYSMIQUES !";
                return degatsMeteore;
            }

            if (!Enrage && PVActuels <= PVMax * 0.40)
            {
                Enrage = true;
                Attaque += 16;
                messageAction = $"🔥 {Nom} ENRAGE ! Ses cornes s'embrasent d'un feu noir impie (+16 ATK) !";
                return 0;
            }

            int action = rng.Next(100);
            if (action < 25)
            {
                ChargeMeteore = true;
                messageAction = $"⚠️ {Nom} LÈVE LES BRAS VERS LE CIEL : Il canalise un Météore pour le prochain tour ! Activez votre BOUCLIER !";
                return 0;
            }
            else if (action < 55)
            {
                int degats = Math.Max(24, (Attaque + 10) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🔥 {Nom} crache une gerbe de flammes maudites qui vous inflige {degats} dégâts brûlants !";
                return degats;
            }
            else
            {
                int degats = Math.Max(16, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🗡️ {Nom} transperce vos défenses avec son trident d'obsidienne rouge pour {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 7 : MOR'GATH LA LICHE NÉCROMANCIENNE ---
    public class BossMorGath : Monstre
    {
        public BossMorGath() : base("Mor'Gath, Liche Nécromancienne des Profondeurs", 1250, 64, 30, 3600, 1800, true,
            "LA MORT N'EST QUE LE COMMENCEMENT... ET VOTRE ÂME M'APPARTIENT DÉJÀ !", "Lumière / Sacré", "Maître des cryptes impies, ayant asservi la mort elle-même.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            int action = rng.Next(100);

            if (action < 30) // Siphon d'âme & Mana
            {
                int manaSiphone = Math.Min(35, joueur.ManaActuel);
                joueur.ManaActuel -= manaSiphone;
                int degats = Math.Max(26, 48 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                int soin = degats + 25;
                PVActuels = Math.Min(PVMax, PVActuels + soin);
                messageAction = $"💀 {Nom} siphonne {manaSiphone} Mana et vous inflige {degats} dégâts tout en se régénérant de {soin} PV !";
                return degats;
            }
            else if (action < 60) // Malédiction
            {
                int degats = Math.Max(28, (Attaque + 8) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🕸️ {Nom} libère une MALÉDICTION DE DÉCRÉPITUDE ! Une onde d'ombre foudroie votre corps pour {degats} dégâts sombres !";
                return degats;
            }
            else
            {
                int degats = Math.Max(18, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚡ {Nom} projette une volée de crânes spectraux enflammés pour {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 8 : VALDORAK LE COLOSSE DE GRANITE ---
    public class BossValdorak : Monstre
    {
        public BossValdorak() : base("Valdorak, Colosse de Granite Primordial", 1550, 74, 44, 4800, 2400, true,
            "INÉBRANLABLE COMME LA MONTAGNE... TON CORPS FINIRA EN POUSSIÈRE !", "Foudre / Magie Pure", "Titan millénaire impassible, gardien des abîmes telluriques.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            int action = rng.Next(100);

            if (action < 35) // Fracas sismique
            {
                int degats = Math.Max(38, 72 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🌋 FRACAS SISMIQUE ! {Nom} pilonne le sol rocheux ! La terre s'ouvre : {degats} DÉGÂTS SISMIQUES PURS !";
                return degats;
            }
            else if (action < 65) // Rocher géant
            {
                int degats = Math.Max(28, (Attaque + 12) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🪨 {Nom} vous projette un mégalithe d'une tonne ! Écrasement brutal de {degats} dégâts !";
                return degats;
            }
            else
            {
                int degats = Math.Max(20, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🔨 Le poing de pierre cyclopéen de {Nom} s'abat sur vous pour {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 9 : AZKALITH LA REINE VIPÈRE DES ABYSSES ---
    public class BossAzkalith : Monstre
    {
        public BossAzkalith() : base("Azkalith, Reine Vipère des Abysses Oubliées", 1350, 70, 32, 4200, 2100, true,
            "MON VENIN DISSOUDRA TON SANG... CHANTE TA DERNIÈRE PRIÈRE !", "Feu", "Gorgone abyssale aux anneaux d'écailles émeraude et crochets empoisonnés.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            int action = rng.Next(100);

            if (action < 35) // Morsure venimeuse
            {
                int degats = Math.Max(32, 62 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🐍 MORSURE VENIMEUSE MORTELLE ! {Nom} injecte un venin nécrotique violent pour {degats} dégâts toxiques !";
                return degats;
            }
            else if (action < 65) // Étreinte constrictrice
            {
                int degats = Math.Max(25, (Attaque + 10) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⛓️ {Nom} enroule ses anneaux titanesques autour de vous : {degats} dégâts de constriction !";
                return degats;
            }
            else
            {
                int degats = Math.Max(18, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🗡️ {Nom} vous cingle de sa queue d'écailles tranchantes pour {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 10 (HARDCORE ULTIME) : LE CHEVALIER DU NÉANT PRIMORDIAL ---
    public class BossChevalierDuNeant : Monstre
    {
        public bool FormeChaos { get; set; } = false;

        public BossChevalierDuNeant() : base("Le Chevalier du Néant Primordial", 2200, 90, 48, 8000, 5000, true,
            "TOUTE MATIÈRE N'EST QUE POUSSIÈRE ILLUSOIRE. DISPARAIS DANS L'ABÎME SANS FIN !", "Sacré Pur / Ultime", "L'Entité suprême née du vide originel, exterminatrice d'empires entiers.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;

            if (!FormeChaos && PVActuels <= PVMax * 0.35)
            {
                FormeChaos = true;
                Attaque += 22;
                messageAction = $"🌌 L'ARME DU NÉANT EXPLOSE EN MILLE ÉTOILES NOIRES ! {Nom} prend sa FORME DU CHAOS ULTIME (+22 ATK) !";
                return 0;
            }

            int action = rng.Next(100);
            if (action < 30) // Éclipse totale du vide
            {
                int degats = Math.Max(50, 105 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                joueur.JaugeUltime = Math.Max(0, joueur.JaugeUltime - 35);
                messageAction = $"🌑 ÉCLIPSE DU NÉANT TOTAL ! L'espace-temps implose sous sa lame ! {degats} DÉGÂTS PURS et -35% Ultime !";
                return degats;
            }
            else if (action < 60) // Lame de ténèbres fendant la réalité
            {
                int degats = Math.Max(38, (Attaque + 14) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                int siphon = 30;
                PVActuels = Math.Min(PVMax, PVActuels + siphon);
                messageAction = $"⚔️ TRANCHE-RÉALITÉ DU NÉANT ! Frappe interdimensionnelle de {degats} dégâts (il draine {siphon} PV) !";
                return degats;
            }
            else
            {
                int degats = Math.Max(26, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚡ {Nom} foudroie vos armures avec un rayon d'antimatière pure pour {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }
}
