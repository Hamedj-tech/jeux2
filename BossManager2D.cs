using System;
using System.Collections.Generic;
using System.Drawing;

namespace JeuxRPG
{
    public class DifficulteBoss2D
    {
        public string Nom { get; set; } = "Normale";
        public string Titre { get; set; } = "🟢 Normale";
        public float MultiplicateurPV { get; set; } = 1.0f;
        public float MultiplicateurAttaque { get; set; } = 1.0f;
        public float MultiplicateurRecompenses { get; set; } = 1.0f;
        public int PierresBonus { get; set; } = 0;
        public bool EstInfernal { get; set; } = false;
        public Color CouleurBadge { get; set; } = Color.FromArgb(46, 204, 113);

        public static readonly DifficulteBoss2D Normale = new DifficulteBoss2D
        {
            Nom = "Normale",
            Titre = "🟢 Normale",
            MultiplicateurPV = 1.0f,
            MultiplicateurAttaque = 1.0f,
            MultiplicateurRecompenses = 1.0f,
            PierresBonus = 0,
            CouleurBadge = Color.FromArgb(46, 204, 113)
        };

        public static readonly DifficulteBoss2D Heroique = new DifficulteBoss2D
        {
            Nom = "Héroïque",
            Titre = "🟡 Héroïque",
            MultiplicateurPV = 2.0f,
            MultiplicateurAttaque = 1.45f,
            MultiplicateurRecompenses = 1.85f,
            PierresBonus = 2,
            CouleurBadge = Color.FromArgb(241, 196, 15)
        };

        public static readonly DifficulteBoss2D Mythique = new DifficulteBoss2D
        {
            Nom = "Mythique",
            Titre = "🔴 Mythique",
            MultiplicateurPV = 3.6f,
            MultiplicateurAttaque = 2.1f,
            MultiplicateurRecompenses = 3.2f,
            PierresBonus = 4,
            CouleurBadge = Color.FromArgb(231, 76, 60)
        };

        public static readonly DifficulteBoss2D Infernal = new DifficulteBoss2D
        {
            Nom = "Infernal",
            Titre = "💀 Infernal",
            MultiplicateurPV = 6.0f,
            MultiplicateurAttaque = 3.0f,
            MultiplicateurRecompenses = 5.5f,
            PierresBonus = 8,
            EstInfernal = true,
            CouleurBadge = Color.FromArgb(155, 89, 182)
        };

        public static readonly List<DifficulteBoss2D> Toutes = new List<DifficulteBoss2D>
        {
            Normale, Heroique, Mythique, Infernal
        };
    }

    public class EntreeBoss2D
    {
        public string Nom { get; set; } = "";
        public string SousTitre { get; set; } = "";
        public string Categorie { get; set; } = "Régional";
        public int NiveauConseille { get; set; } = 1;
        public string Icone { get; set; } = "👑";
        public Color CouleurTheme { get; set; } = Color.FromArgb(180, 40, 40);
        public Func<Monstre> Fabrique { get; set; } = null!;

        public EntreeBoss2D(string nom, string sousTitre, string cat, int niv, string icone, Color coul, Func<Monstre> fabrique)
        {
            Nom = nom;
            SousTitre = sousTitre;
            Categorie = cat;
            NiveauConseille = niv;
            Icone = icone;
            CouleurTheme = coul;
            Fabrique = fabrique;
        }
    }

    public static class CatalogueBoss2D
    {
        public static readonly List<EntreeBoss2D> BossRegionaux = new List<EntreeBoss2D>
        {
            new EntreeBoss2D("Grok le Brise-Crâne", "Seigneur Gobelin Brutal", "Régional", 1, "👑", Color.FromArgb(180, 40, 40), () => new BossGrok()),
            new EntreeBoss2D("Gorrok le Boucher", "Broyeur des Terres Sauvages", "Régional", 2, "🐗", Color.FromArgb(160, 50, 30), () => new BossGorrok()),
            new EntreeBoss2D("Kragh l'Écorcheur", "Chef de la Horde Noire", "Régional", 3, "🗡️", Color.FromArgb(140, 60, 20), () => new BossKragh()),
            new EntreeBoss2D("Malakor le Seigneur", "Nécromancien des Catacombes", "Régional", 3, "💀", Color.FromArgb(130, 25, 50), () => new BossMalakor()),
            new EntreeBoss2D("Skuldir le Roi Cryo", "Souverain Squelette de Glace", "Régional", 4, "❄️", Color.FromArgb(30, 80, 130), () => new BossSkuldir()),
            new EntreeBoss2D("Zulgar le Chaman", "Maître Putride des Marais", "Régional", 5, "🧪", Color.FromArgb(40, 110, 60), () => new BossZulgar()),
            new EntreeBoss2D("Vespera la Tisseuse", "Matriarche des Harpies", "Régional", 5, "🕷️", Color.FromArgb(110, 30, 110), () => new BossVespera()),
            new EntreeBoss2D("Kaelas l'Archifée", "Gardienne du Blizzard Éternel", "Régional", 6, "❄️", Color.FromArgb(20, 90, 140), () => new BossKaelas()),
            new EntreeBoss2D("Balthazar le Déchu", "Chevalier Noir Vampire", "Régional", 6, "🧛", Color.FromArgb(120, 15, 40), () => new BossBalthazar()),
            new EntreeBoss2D("Kryll de Nécrose", "Arachnide Chitinique Titanesque", "Régional", 7, "🦂", Color.FromArgb(100, 70, 20), () => new BossKryll())
        };

        public static readonly List<EntreeBoss2D> BossMythiques = new List<EntreeBoss2D>
        {
            new EntreeBoss2D("Magmarion le Colosse", "Titan de Lave Éveillé", "Mythique", 7, "🌋", Color.FromArgb(190, 50, 10), () => new BossMagmarion()),
            new EntreeBoss2D("Sylvana des Ronces", "Reine Sépulcrale Primordiale", "Mythique", 8, "🌲", Color.FromArgb(35, 100, 50), () => new BossSylvana()),
            new EntreeBoss2D("Obsidius le Titan", "Colosse de Verre Noir Volcanique", "Mythique", 8, "🪨", Color.FromArgb(60, 50, 60), () => new BossObsidius()),
            new EntreeBoss2D("Ignis Dragon Suprême", "Monarque Millénaire de Valdorak", "Mythique", 8, "🐉", Color.FromArgb(180, 60, 10), () => new BossDragonIgnis()),
            new EntreeBoss2D("Nox le Spectre", "Dévoreur de Toute Lumière", "Mythique", 9, "🌑", Color.FromArgb(45, 30, 65), () => new BossNox()),
            new EntreeBoss2D("Bélial le Démon", "Seigneur des Flammes Noires", "Mythique", 9, "🔥", Color.FromArgb(160, 30, 10), () => new BossBelial()),
            new EntreeBoss2D("Général Thalor", "Guerrier Immortel Sans-Tête", "Mythique", 10, "⚔️", Color.FromArgb(140, 90, 15), () => new BossThalor()),
            new EntreeBoss2D("Mor'Gath la Liche", "Nécromancienne Démoniaque", "Mythique", 10, "💀", Color.FromArgb(90, 20, 90), () => new BossMorGath()),
            new EntreeBoss2D("Azkalith la Vipère", "Reine des Abysses Oubliées", "Mythique", 10, "🐍", Color.FromArgb(25, 110, 70), () => new BossAzkalith()),
            new EntreeBoss2D("Valdorak le Colosse", "Titan de Granite Primordial", "Mythique", 11, "🪨", Color.FromArgb(70, 80, 90), () => new BossValdorak())
        };

        public static readonly List<EntreeBoss2D> BossCosmiques = new List<EntreeBoss2D>
        {
            new EntreeBoss2D("Zephyros Ouragan", "Archi-Seigneur des Tempêtes", "Cosmique", 11, "⚡", Color.FromArgb(30, 110, 140), () => new BossZephyros()),
            new EntreeBoss2D("Fenrir Lune de Sang", "Loup Ancestral Primordial", "Cosmique", 12, "🐺", Color.FromArgb(150, 20, 25), () => new BossFenrirLuneSang()),
            new EntreeBoss2D("Chevalier du Néant", "Entité Primordiale de l'Oubli", "Cosmique", 12, "🌌", Color.FromArgb(90, 15, 45), () => new BossChevalierDuNeant()),
            new EntreeBoss2D("L'Archonte Solaire", "Flamme Céleste Purificatrice", "Cosmique", 13, "☀️", Color.FromArgb(180, 120, 15), () => new BossArchonteSolaire()),
            new EntreeBoss2D("Kraken des Abîmes", "Terreur Céphalopode Ancestrale", "Cosmique", 14, "🐙", Color.FromArgb(15, 60, 90), () => new BossKrakenAbyssal()),
            new EntreeBoss2D("Xanthos le Gardien", "Dévoreur Stellaire du Néant", "Cosmique", 15, "🌌", Color.FromArgb(110, 45, 130), () => new BossXanthos()),
            new EntreeBoss2D("Chronos du Temps", "Maître des Sabliers Brisés", "Cosmique", 15, "⏳", Color.FromArgb(135, 105, 30), () => new BossChronos()),
            new EntreeBoss2D("Léviathan Stellaire", "Fléau des Galaxies Oubliées", "Cosmique", 16, "☄️", Color.FromArgb(70, 25, 110), () => new BossLeviathanStellaire()),
            new EntreeBoss2D("Abaddon Pourfendeur", "Destructeur de Réalités", "Cosmique", 18, "💀", Color.FromArgb(130, 15, 15), () => new BossAbaddon()),
            new EntreeBoss2D("Deus Ex Nihilo", "Architecte Suprême du Chaos", "Cosmique", 20, "👑", Color.FromArgb(110, 10, 60), () => new BossDeusExNihilo())
        };

        public static List<EntreeBoss2D> ObtenirTousLesBoss()
        {
            var tous = new List<EntreeBoss2D>();
            tous.AddRange(BossRegionaux);
            tous.AddRange(BossMythiques);
            tous.AddRange(BossCosmiques);
            return tous;
        }

        public static void GenererEnnemisTour(int etage, out Monstre bossOuGardien, out List<Monstre> adds)
        {
            adds = new List<Monstre>();

            // Scaling infini pour les étages adapté au combat 2D
            int pvBase = 850 + (etage * 240) + (int)(Math.Pow(etage, 1.35) * 55);
            int atkBase = 26 + (etage * 7) + (int)(Math.Pow(etage, 1.15) * 3);
            int defBase = 10 + (etage * 4) + (int)(etage * 1.5);
            int xpBase = 220 + (etage * 110);
            int orBase = 120 + (etage * 70);

            // Tous les 5 étages : Boss majeur issu du catalogue des 30 boss
            if (etage % 5 == 0)
            {
                var tous = ObtenirTousLesBoss();
                int idx = ((etage / 5) - 1) % tous.Count;
                var proto = tous[idx].Fabrique();

                int baseProtoPV = Math.Max(2800, (int)(proto.PVMax * 4.5f));
                int pv = (int)(baseProtoPV * (1f + (etage * 0.20f)));
                int atk = (int)(Math.Max(30, proto.Attaque * 1.35f) * (1f + (etage * 0.10f)));
                int def = (int)(proto.Defense * (1f + (etage * 0.08f)));
                int xp = (int)(proto.GainXP * (1f + (etage * 0.18f)));
                int or = (int)(proto.GainOr * (1f + (etage * 0.15f)));

                bossOuGardien = new Monstre(
                    $"👑 {proto.Nom} (Étage {etage})",
                    pv, atk, def, xp, or,
                    true,
                    proto.CriDeGuerre,
                    proto.Faiblesse,
                    proto.Lore
                );
            }
            else
            {
                string[] titres = {
                    "Sentinelle Stellaire", "Ombre du Vide", "Traqueur de Nova",
                    "Colosse Céleste", "Annihilateur de Réalité", "Gardien de l'Éther",
                    "Guerrier d'Anti-Matière", "Spectre des Galaxies"
                };
                string titre = $"{titres[etage % titres.Length]} (Étage {etage})";
                bossOuGardien = new Monstre(titre, pvBase, atkBase, defBase, xpBase, orBase, true, "NUL NE MONTERA PLUS HAUT !", "Éther");
            }

            // Adds qui augmentent en nombre et puissance
            int nbAdds = Math.Min(4, etage / 3);
            for (int i = 0; i < nbAdds; i++)
            {
                int pvAdd = 320 + (etage * 75);
                int atkAdd = 20 + (etage * 4);
                int defAdd = 8 + (etage * 2);
                adds.Add(new Monstre($"Écho Stellaire Rang {etage}", pvAdd, atkAdd, defAdd, 90 + etage * 22, 45 + etage * 15));
            }
        }
    }
}
