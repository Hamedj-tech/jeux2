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
    // 20 NOUVEAUX BOSS Ã‰PIQUES & MYTHIQUES (BOSS 11 Ã€ 30)
    // ==============================================================

    // --- BOSS 11 : GORROK LE BROYEUR DES TERRES SAUVAGES (Niv. 2) ---
    public class BossGorrok : Monstre
    {
        public BossGorrok() : base("Gorrok le Broyeur des Terres Sauvages", 250, 24, 10, 420, 210, true,
            "GROOOOAAAR ! MON GROIN VA T'Ã‰CRASER CONTRE LE ROC !", "Tranchant / Feu", "Sanglier colossal enragÃ© ayant terrassÃ© des dizaines de chasseurs royaux.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(12, (int)(Attaque * 1.55) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸ— CHARGE BESTIALE FRACASSANTE ! {Nom} vous percute de plein fouet pour {degats} dÃ©gÃ¢ts !";
                return degats;
            }
            int d = Math.Max(6, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ— {Nom} vous laboure avec ses dÃ©fenses aiguisÃ©es pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 12 : KRAGH L'Ã‰CORCHEUR DE LA HORDE (Niv. 3) ---
    public class BossKragh : Monstre
    {
        public BossKragh() : base("Kragh l'Ã‰corcheur de la Horde Noire", 310, 28, 11, 500, 250, true,
            "Kikiki ! Un coup dans l'ombre et tes boyaux se rÃ©pandront !", "Magie Pure", "Assassin gobelin sournois muni de dagues dentelÃ©es trempÃ©es dans l'acide.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(14, (int)(Attaque * 1.6) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸ—¡ï¸ FRAPPE SOURNOISE DANS L'OMBRE ! {Nom} transperce vos points faibles pour {degats} dÃ©gÃ¢ts venimeux !";
                return degats;
            }
            int d = Math.Max(8, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ—¡ï¸ {Nom} lacÃ¨re vos protections de ses doubles dagues pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 13 : SKULDIR LE ROI SQUELETTE CRYOMANCIEN (Niv. 4) ---
    public class BossSkuldir : Monstre
    {
        public BossSkuldir() : base("Skuldir, Roi Squelette Cryomancien", 410, 32, 15, 640, 320, true,
            "LE FROID DE LA TOMBE PÃ‰NÃˆTRERA TES OS JUSQU'Ã€ LA MOELLE !", "Feu SacrÃ©", "Monarque antique rÃ©animÃ© par une magie de glace Ã©ternelle.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int degats = Math.Max(16, (int)(Attaque * 1.65) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"â„ï¸ PIEUX DE GLACE NÃ‰CROTIQUE ! {Nom} empale votre chair avec des stalactites pour {degats} dÃ©gÃ¢ts de givre !";
                return degats;
            }
            int d = Math.Max(9, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"â„ï¸ {Nom} assÃ¨ne un coup de sceptre gelÃ© infligeant {d} dÃ©gÃ¢ts de froid.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 14 : ZULGAR LE CHAMAN PUTRIDE (Niv. 5) ---
    public class BossZulgar : Monstre
    {
        public BossZulgar() : base("Zulgar le Chaman Putride", 490, 35, 16, 750, 380, true,
            "LES ESPRITS ANCIENS ME RÃ‰CLAMENT TON Ã‚ME ET TES ENTRAILLES !", "Foudre / SacrÃ©", "MaÃ®tre des arts occultes orcs invoquant des effluves corrompus.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int soin = 45;
                PVActuels = Math.Min(PVMax, PVActuels + soin);
                int degats = Math.Max(12, (Attaque + 5) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸ§ª TOTEM DE GUÃ‰RISON PUTRIDE ! {Nom} se soigne de {soin} PV et vous foudroie de miasmes pour {degats} dÃ©gÃ¢ts !";
                return degats;
            }
            int d = Math.Max(10, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸª“ {Nom} abat son bÃ¢ton d'os totÃ©mique pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 15 : VESPERA LA MATRIARCHE DES HARPIES (Niv. 5) ---
    public class BossVespera : Monstre
    {
        public BossVespera() : base("Vespera, Matriarche des Harpies", 460, 38, 13, 780, 400, true,
            "PERSONNE N'Ã‰CHAPPE AUX GRIFFES DES HAUTS VENTS !", "Distance / Perforant", "Reine sanguinaire des falaises embrumÃ©es fendant les cieux.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(18, (int)(Attaque * 1.7) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸŒªï¸ OURAGAN DE SERRES ! {Nom} plonge en piquÃ© et dÃ©chire vos chairs pour {degats} dÃ©gÃ¢ts aÃ©riens !";
                return degats;
            }
            int d = Math.Max(11, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ¦… {Nom} vous cingle de ses ailes aux plumes tranchantes pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 16 : BALTHAZAR LE CHEVALIER NOIR DÃ‰CHU (Niv. 6) ---
    public class BossBalthazar : Monstre
    {
        public BossBalthazar() : base("Balthazar, Chevalier Noir DÃ©chu", 620, 40, 23, 950, 480, true,
            "MON SERMENT EST MORT AVEC MON ROI ! SEUL RESTE LE FER !", "Magie Sombre / Foudre", "Ancien paladin de la garde royale corrompu par la soif de puissance.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int degats = Math.Max(20, (int)(Attaque * 1.75) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"âš”ï¸ FRAPPE DU SERMENT BRISÃ‰ ! {Nom} abat son espadon d'acier noir pour {degats} dÃ©gÃ¢ts titanesques !";
                return degats;
            }
            int d = Math.Max(12, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ›¡ï¸ {Nom} vous percute de son pavois noir et enchaÃ®ne pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 17 : KRYLL L'ARACHNIDE DE NÃ‰CROSE (Niv. 7) ---
    public class BossKryll : Monstre
    {
        public BossKryll() : base("Kryll l'Arachnide de NÃ©crose", 690, 44, 20, 1100, 540, true,
            "Sssss... EmprisonnÃ© dans ma soie, tu serviras de couveuse vivante !", "Feu Purificateur", "Arachnide gÃ©ante des profondeurs souterraines aux crochets suintants de poison.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(22, (int)(Attaque * 1.5) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸ•¸ï¸ MORSURE NÃ‰CROTIQUE TOXIQUE ! {Nom} inocule un venin rongeur de chair pour {degats} dÃ©gÃ¢ts venimeux !";
                return degats;
            }
            int d = Math.Max(13, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ•·ï¸ {Nom} vous transperce d'une patte acÃ©rÃ©e comme une javeline pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 18 : MAGMARION LE COLOSSE DE BRAISE (Niv. 7) ---
    public class BossMagmarion : Monstre
    {
        public BossMagmarion() : base("Magmarion, Colosse de Braise Ã‰veillÃ©", 770, 46, 24, 1200, 600, true,
            "LA TERRE BRÃ›LE SOUS MES PAS ! FUSIONNE AVEC LA LAVE !", "Glace / Eau BÃ©nite", "EntitÃ© Ã©lÃ©mentaire nÃ©e au cÅ“ur des cratÃ¨res volcaniques.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(24, (int)(Attaque * 1.7) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸŒ‹ CATACLYSME PYROCLASTIQUE ! {Nom} expulse une geyser de lave brÃ»lante pour {degats} dÃ©gÃ¢ts de feu !";
                return degats;
            }
            int d = Math.Max(14, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ”¥ {Nom} vous Ã©crase de son poing de basalte en fusion pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 19 : SYLVANA LA REINE DES RONCES (Niv. 8) ---
    public class BossSylvana : Monstre
    {
        public BossSylvana() : base("Sylvana la Reine des Ronces SÃ©pulcrales", 820, 48, 22, 1350, 680, true,
            "LES RACINES ANCIENNES BOIRONT VOTRE SANG POUR PURIFIER LA FORÃŠT !", "Feu DÃ©vorant", "Dryade corrompue par des Ã©nergies nÃ©crotiques contrÃ´lant la flore Ã©pineuse.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int siphon = 35;
                int degats = Math.Max(20, Attaque - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                PVActuels = Math.Min(PVMax, PVActuels + siphon);
                messageAction = $"ðŸŒ¿ RONCES VAMPIRIQUES MAUDITES ! {Nom} draine {siphon} PV et vous inflige {degats} dÃ©gÃ¢ts vÃ©gÃ©taux !";
                return degats;
            }
            int d = Math.Max(14, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸŒµ {Nom} vous fustige de fouets d'Ã©pines acÃ©rÃ©es pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 20 : OBSIDIUS LE TITAN DE VERRE NOIR (Niv. 8) ---
    public class BossObsidius : Monstre
    {
        public BossObsidius() : base("Obsidius, Titan de Verre Noir", 920, 50, 30, 1500, 750, true,
            "JE SUIS LE CRISTAL NOIR INDESTRUCTIBLE ! BRISERA CELUI QUI ME TOUCHE !", "Frappe Lourde / Marteau", "Colosse taillÃ© dans les profondeurs de verre volcanique impÃ©nÃ©trable.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int degats = Math.Max(26, (int)(Attaque * 1.8) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸ’Ž FRACAS D'OBSIDIENNE TRANCHANTE ! {Nom} abat son bras de roche coupante pour {degats} dÃ©gÃ¢ts massifs !";
                return degats;
            }
            int d = Math.Max(15, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸª¨ {Nom} vous Ã©crase lourdement pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 21 : NOX LE SPECTRE DÃ‰VOREUR DE LUMIÃˆRE (Niv. 9) ---
    public class BossNox : Monstre
    {
        public BossNox() : base("Nox, Spectre DÃ©voreur de LumiÃ¨re", 860, 54, 22, 1650, 820, true,
            "LA LUMIÃˆRE MEURT ICI... TON DERNIER SOUFFLE SERA NOIR ET GLACIAL !", "SacrÃ© Pur", "EntitÃ© d'ombre tapie dans les failles stellaires dÃ©vorant les Ã¢mes.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(28, (int)(Attaque * 1.7) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                joueur.ManaActuel = Math.Max(0, joueur.ManaActuel - 20);
                messageAction = $"ðŸŒ‘ Ã‰CLIPSE ABSORBANTE D'Ã‚ME ! {Nom} vous submerge de tÃ©nÃ¨bres (-20 Mana) pour {degats} dÃ©gÃ¢ts spectraux !";
                return degats;
            }
            int d = Math.Max(16, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ‘» {Nom} vous traverse de ses griffes impalpables pour {d} dÃ©gÃ¢ts de froid glacial.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 22 : THALOR LE GÃ‰NÃ‰RAL SANS-TÃŠTE (Niv. 10) ---
    public class BossThalor : Monstre
    {
        public BossThalor() : base("GÃ©nÃ©ral Thalor le Sans-TÃªte", 1020, 58, 27, 1900, 950, true,
            "MA TÃŠTE FUT TRANCHÃ‰E, MAIS MA VENGEANCE EST Ã‰TERNELLE !", "Feu / Foudre", "LÃ©gat des lÃ©gions impÃ©riales spectrales chevauchant un cauchemar de feu.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int degats = Math.Max(32, (int)(Attaque * 1.85) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸŽ CHARGE DE CAUCHEMAR SPECTRAL ! {Nom} vous piÃ©tine sauvagement pour {degats} dÃ©gÃ¢ts dÃ©vastateurs !";
                return degats;
            }
            int d = Math.Max(18, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"âš”ï¸ {Nom} abat sa grande hallebarde fantomatique pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 23 : ZEPHYROS LE SEIGNEUR DES OURAGANS (Niv. 11) ---
    public class BossZephyros : Monstre
    {
        public BossZephyros() : base("Zephyros, Archi-Seigneur des Ouragans", 1180, 63, 29, 2200, 1100, true,
            "QUE LA FOUDRE DU CIEL PURIFIE CE BAS MONDE !", "Tellurique / Terre", "Titan cÃ©leste rÃ©gnant au sommet de la Tour Astrale et dÃ©chaÃ®nant les tempÃªtes.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(35, (int)(Attaque * 1.8) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"âš¡âš¡ FOUDRE ASCENDANTE EN CHAÃŽNE ! {Nom} Ã©lectrocute votre systÃ¨me nerveux pour {degats} DÃ‰GÃ‚TS Ã‰LECTRIQUES !";
                return degats;
            }
            int d = Math.Max(20, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸŒªï¸ {Nom} vous propulse de bourrasques tranchantes pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 24 : FENRIR DE LA LUNE DE SANG (Niv. 12) ---
    public class BossFenrirLuneSang : Monstre
    {
        public int ToursCombat { get; set; } = 0;

        public BossFenrirLuneSang() : base("Fenrir, Loup Ancestral de la Lune Rouge", 1320, 69, 29, 2600, 1300, true,
            "AWOOOOOO ! LA LUNE Ã‰CARLATE RÃ‰CLAME UN FESTIN DE CHAIR ET DE SANG !", "Argent Pur / SacrÃ©", "PrÃ©dateur primautÃ© dont la soif de sang augmente Ã  chaque seconde passÃ©e au combat.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            ToursCombat++;
            int bonusFrenesie = ToursCombat * 4;
            Random rng = Random.Shared;

            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(38, (int)((Attaque + bonusFrenesie) * 1.6) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸ©¸ MORSURE CAROTIDIENNE FRÃ‰NÃ‰TIQUE ! {Nom} vous Ã©gorge (+{bonusFrenesie} ATK de frÃ©nÃ©sie) pour {degats} dÃ©gÃ¢ts sanglants !";
                return degats;
            }
            int d = Math.Max(22, (Attaque + bonusFrenesie) - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸº {Nom} bondit de ses crocs acÃ©rÃ©s et vous arrache de la chair pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 25 : L'ARCHONTE SOLAIRE DÃ‰CHU (Niv. 13) ---
    public class BossArchonteSolaire : Monstre
    {
        public BossArchonteSolaire() : base("L'Archonte Solaire DÃ©chu de la Flamme Blanche", 1480, 73, 35, 3000, 1500, true,
            "LE FEU SACRÃ‰ DES CIEUX VOUS CONSUMERA JUSQU'AUX CENDRES !", "Ombre / TÃ©nÃ¨bres", "Ange guerrier aux ailes de platine incandescent foudroyant les impurs.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(42, (int)(Attaque * 1.85) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"â˜€ï¸ NOVA BLANCHE PURIFICATRICE ! {Nom} libÃ¨re l'Ã©clat du soleil sacrÃ© pour {degats} dÃ©gÃ¢ts radiants !";
                return degats;
            }
            int d = Math.Max(24, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ—¡ï¸ {Nom} transperce vos dÃ©fenses de sa lance de lumiÃ¨re pure pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 26 : LE KRAKEN DES ABÃŽMES PRIMORDIAUX (Niv. 14) ---
    public class BossKrakenAbyssal : Monstre
    {
        public BossKrakenAbyssal() : base("Le Kraken des AbÃ®mes Primordiaux", 1650, 77, 37, 3500, 1700, true,
            "LES ABÃŽMES PROFONDS RÃ‰CLAMENT TON CORPS ET TON Ã‚ME !", "Foudre CÃ©leste", "Terreur titanesque des gouffres ocÃ©aniques souterrains brisant la roche comme du verre.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(45, (int)(Attaque * 1.9) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸ™ BROYAGE DE TENTACULES TITANESQUES ! {Nom} Ã©crase votre squelette pour {degats} dÃ©gÃ¢ts colossaux !";
                return degats;
            }
            int d = Math.Max(26, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸŒŠ {Nom} vous fouette d'un tentacule d'Ã©cailles sombres pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 27 : CHRONOS LE MAÃŽTRE DU TEMPS (Niv. 15) ---
    public class BossChronos : Monstre
    {
        public BossChronos() : base("Chronos, Seigneur des Sabliers BrisÃ©s", 1790, 81, 39, 4000, 2000, true,
            "LE TEMPS M'OBÃ‰IT. TES COUPS NE SONT QU'UN Ã‰CHO PASSÃ‰ !", "Chaos / Ultime", "EntitÃ© dimensionnelle capable de plier le cours temporel Ã  son bon vouloir.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 25)
            {
                int soin = 90;
                PVActuels = Math.Min(PVMax, PVActuels + soin);
                int degats = Math.Max(35, Attaque - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"â³ REMBOBINAGE TEMPOREL ! {Nom} remonte le temps et rÃ©gÃ©nÃ¨re {soin} PV, puis vous frappe pour {degats} dÃ©gÃ¢ts !";
                return degats;
            }
            else if (rng.Next(100) < 40)
            {
                int degats = Math.Max(48, (int)(Attaque * 1.9) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"âš¡ ACCÃ‰LÃ‰RATION CHRONO-DIMENSIONNELLE ! {Nom} frappe deux fois dans la mÃªme seconde pour {degats} dÃ©gÃ¢ts temporels !";
                return degats;
            }
            int d = Math.Max(28, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"âŒ› {Nom} projette des sables temporels corrosifs pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 28 : LÃ‰VIATHAN DES GALAXIES (Niv. 16) ---
    public class BossLeviathanStellaire : Monstre
    {
        public BossLeviathanStellaire() : base("LÃ©viathan des Galaxies OubliÃ©es", 1990, 87, 43, 4600, 2300, true,
            "JE VOYAGE ENTRE LES Ã‰TOILES... TU N'ES QU'UN GRAIN DE POUSSIÃˆRE !", "SacrÃ© / Feu Divin", "BÃªte cosmique se nourrissant de nÃ©buleuses et naviguant Ã  travers l'espace profond.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(52, (int)(Attaque * 2.0) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                joueur.ManaActuel = Math.Max(0, joueur.ManaActuel - 30);
                messageAction = $"ðŸŒŒ RAZ-DE-MARÃ‰E GRAVITATIONNEL ! {Nom} courbe l'espace autour de vous (-30 Mana) pour {degats} DÃ‰GÃ‚TS STELLAIRES !";
                return degats;
            }
            int d = Math.Max(30, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"â˜„ï¸ {Nom} expulse une onde de matiÃ¨re comÃ©taire pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 29 : ABADDON LE POURFENDEUR DE MONDES (Niv. 18) ---
    public class BossAbaddon : Monstre
    {
        public BossAbaddon() : base("Abaddon, Pourfendeur de Mondes", 2250, 95, 46, 5800, 2900, true,
            "L'APOCALYPSE EST LÃ€ ! AUCUN REFUGE, AUCUN SALUT !", "LumiÃ¨re Primordiale", "Archange de la ruine portant une faux forgÃ©e dans le cÅ“ur d'une Ã©toile morte.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(60, (int)(Attaque * 2.1) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"ðŸ’€ FAUX D'EXTERMINATION APOCALYPTIQUE ! {Nom} fauche votre essence pour {degats} DÃ‰GÃ‚TS DÃ‰VASTATEURS !";
                return degats;
            }
            int d = Math.Max(34, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"ðŸ”¥ {Nom} dÃ©chaÃ®ne une onde de feu noir de soufre pour {d} dÃ©gÃ¢ts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }

    // --- BOSS 30 : DEUS EX NIHILO L'ARCHITECTE DU CHAOS (Niv. 20 - BOSS SUPRÃŠME ULTIME) ---
    public class BossDeusExNihilo : Monstre
    {
        public bool PhaseChaos { get; set; } = false;

        public BossDeusExNihilo() : base("Deus Ex Nihilo, Architecte du Chaos Primordial", 3600, 115, 58, 15000, 10000, true,
            "JE SUIS CE QUI Ã‰TAIT AVANT LA CRÃ‰ATION. JE SUIS LE RIEN ET LE TOUT. EFFACE-TOI !", "Aucune / VolontÃ© InÃ©branlable", "L'EntitÃ© Divine Cosmique SuprÃªme, crÃ©atrice et destructrice de l'univers.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;

            if (!PhaseChaos && PVActuels <= PVMax * 0.35)
            {
                PhaseChaos = true;
                Attaque += 28;
                messageAction = $"ðŸŒŒ L'UNIVERS ENTIER HURLE DANS LE SILENCE ABSOLU ! {Nom} LIBÃˆRE SA FORME DE CHAOS PRIMORDIAL SUPRÃŠME (+28 ATK) !";
                return 0;
            }

            int action = rng.Next(100);
            if (action < 30) // Trou Noir Primordial
            {
                int degats = Math.Max(70, 130 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                joueur.ManaActuel = 0;
                joueur.JaugeUltime = Math.Max(0, joueur.JaugeUltime - 50);
                messageAction = $"ðŸ•³ï¸ TROU NOIR DE L'ANNIHILATION ! L'espace s'effondre sur vous ! {degats} DÃ‰GÃ‚TS PURS (Mana vidÃ©, -50% Ultime) !";
                return degats;
            }
            else if (action < 65) // Rayon Cosmique de GenÃ¨se
            {
                int degats = Math.Max(55, (int)(Attaque * 2.1) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                int siphon = 45;
                PVActuels = Math.Min(PVMax, PVActuels + siphon);
                messageAction = $"ðŸŒŸ RAYON DE GENÃˆSE PRIMORDIALE ! Faisceau d'antimatiÃ¨re pure de {degats} dÃ©gÃ¢ts (il rÃ©gÃ©nÃ¨re {siphon} PV) !";
                return degats;
            }
            else
            {
                int degats = Math.Max(40, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"âš¡ {Nom} foudroie votre armure d'une pluie d'ondes gravitationnelles pour {degats} dÃ©gÃ¢ts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() => base.ObtenirLootMateriaux();
    }
}
