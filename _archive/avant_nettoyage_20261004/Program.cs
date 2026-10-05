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
    // 1. ÉNUMÉRATIONS & CONSTANTES
    // ==============================================================
    public enum ClasseType { Guerrier, Mage, Rodeur, Paladin, Necromancien }
    public enum Rarete { Commun, Rare, Epique, Legendaire, Mythique }
    public enum TypeEquipement { Arme, Armure, Casque, Anneau, Amulette }
    public enum TypeZone { Foret, Catacombes, Volcan, ForteresseGivre, TourAstrale }
    public enum TypeConsommable { PotionSoinMineure, PotionSoinMajeure, PotionManaMineure, PotionManaMajeure, ElixirForce, PeauDePierre, BombeIncendiaire, ParcheminTeleport }

    // ==============================================================
    // 2. ÉQUIPEMENT & OBJETS
    // ==============================================================
    public class Equipement
    {
        public string Nom { get; set; } = "";
        public TypeEquipement Type { get; set; }
        public Rarete RareteItem { get; set; }
        public int BonusAttaque { get; set; }
        public int BonusDefense { get; set; }
        public int BonusPVMax { get; set; }
        public int BonusManaMax { get; set; }
        public int BonusCritique { get; set; } // % chance critique
        public int BonusEsquive { get; set; }  // % chance esquive
        public int BonusVampirisme { get; set; } // % vol de vie
        public string IdentifiantSet { get; set; } = "";
        public int NiveauAmelioration { get; set; } = 0; // +0 à +10
        public int PrixOr { get; set; }
        public int ValeurOr { get => PrixOr; set => PrixOr = value; }

        public Equipement() { }

        public Equipement(string nom, TypeEquipement type, Rarete rarete, int atk, int def, int pv, int mana = 0, int crit = 0, int esq = 0, int vamp = 0, int prix = 50)
        {
            Nom = nom;
            Type = type;
            RareteItem = rarete;
            BonusAttaque = atk;
            BonusDefense = def;
            BonusPVMax = pv;
            BonusManaMax = mana;
            BonusCritique = crit;
            BonusEsquive = esq;
            BonusVampirisme = vamp;
            NiveauAmelioration = 0;
            PrixOr = prix;
        }

        public string ObtenirNomComplet()
        {
            return NiveauAmelioration > 0 ? $"{Nom} +{NiveauAmelioration}" : Nom;
        }

        public string ObtenirDescription()
        {
            List<string> stats = new List<string>();
            int atkEff = BonusAttaque + (NiveauAmelioration * 3);
            int defEff = BonusDefense + (NiveauAmelioration * 2);
            int pvEff = BonusPVMax + (NiveauAmelioration * 8);

            if (atkEff > 0) stats.Add($"+{atkEff} Atk");
            if (defEff > 0) stats.Add($"+{defEff} Def");
            if (pvEff > 0) stats.Add($"+{pvEff} PV");
            if (BonusManaMax > 0) stats.Add($"+{BonusManaMax} Mana");
            if (BonusCritique > 0) stats.Add($"+{BonusCritique}% Crit");
            if (BonusEsquive > 0) stats.Add($"+{BonusEsquive}% Esq");
            if (BonusVampirisme > 0) stats.Add($"+{BonusVampirisme}% Vamp");
            if (!string.IsNullOrWhiteSpace(IdentifiantSet)) stats.Add($"Set : {NomSet2D.ObtenirNom(IdentifiantSet)}");

            return $"{ObtenirNomComplet()} [{RareteItem}] ({string.Join(", ", stats)})";
        }

        public ConsoleColor ObtenirCouleurRarete()
        {
            return RareteItem switch
            {
                Rarete.Commun => ConsoleColor.Gray,
                Rarete.Rare => ConsoleColor.Cyan,
                Rarete.Epique => ConsoleColor.Magenta,
                Rarete.Legendaire => ConsoleColor.Yellow,
                Rarete.Mythique => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
        }

        public double ObtenirScoreDegats()
        {
            double atkEff = BonusAttaque + (NiveauAmelioration * 3);
            double critEff = BonusCritique;
            double vampEff = BonusVampirisme;
            double manaEff = BonusManaMax * 0.05;
            double defEff = (BonusDefense + (NiveauAmelioration * 2)) * 0.04;
            double pvEff = (BonusPVMax + (NiveauAmelioration * 8)) * 0.01;
            double rareteVal = (int)RareteItem * 0.2;

            // Poids offensifs : 10 pts/atk, 3.5 pts/%crit, 1.2 pts/%vamp
            return (atkEff * 10.0) + (critEff * 3.5) + (vampEff * 1.2) + manaEff + defEff + pvEff + rareteVal;
        }
    }

    // ==============================================================
    // 3. CONSOMMABLES & MATÉRIAUX
    // ==============================================================
    public class ConsommableInfo
    {
        public static string ObtenirNom(TypeConsommable type)
        {
            return type switch
            {
                TypeConsommable.PotionSoinMineure => "Potion de Soin Mineure (+60 PV)",
                TypeConsommable.PotionSoinMajeure => "Émulsion Vitale Supérieure (+160 PV)",
                TypeConsommable.PotionManaMineure => "Potion de Mana Mineure (+50 Mana)",
                TypeConsommable.PotionManaMajeure => "Élixir d'Arcane Majeur (+120 Mana)",
                TypeConsommable.ElixirForce => "Élixir de Fureur Berserk (+40% Dégâts pour le combat)",
                TypeConsommable.PeauDePierre => "Flacon de Peau de Pierre (+50% Défense pour le combat)",
                TypeConsommable.BombeIncendiaire => "Bombe Alchimique Incendiaire (80 dégâts explosifs)",
                TypeConsommable.ParcheminTeleport => "Parchemin de Repli Céleste (Fuite garantie)",
                _ => type.ToString()
            };
        }

        public static int ObtenirPrix(TypeConsommable type)
        {
            return type switch
            {
                TypeConsommable.PotionSoinMineure => 25,
                TypeConsommable.PotionSoinMajeure => 70,
                TypeConsommable.PotionManaMineure => 20,
                TypeConsommable.PotionManaMajeure => 60,
                TypeConsommable.ElixirForce => 85,
                TypeConsommable.PeauDePierre => 80,
                TypeConsommable.BombeIncendiaire => 55,
                TypeConsommable.ParcheminTeleport => 90,
                _ => 30
            };
        }
    }

    // ==============================================================
    // 4. QUÊTES & HAUTS FAITS
    // ==============================================================
    public class Quete
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "";
        public string Description { get; set; } = "";
        public string CibleNom { get; set; } = "";
        public int Objectif { get; set; }
        public int Progression { get; set; }
        public int RecompenseOr { get; set; }
        public int RecompenseXP { get; set; }
        public Equipement? RecompenseItem { get; set; }
        public int RecompensePierresForge { get; set; }
        public int RecompenseReputation { get; set; } = 30;
        public int RecompenseSceauxGuilde { get; set; } = 1;
        public string Difficulte { get; set; } = "⚔️ Normale";
        public int NiveauRequis { get; set; } = 1;
        public bool EstTerminee => Progression >= Objectif;
        public bool RecompenseReclamee { get; set; } = false;

        public Quete() { }

        public Quete(int id, string titre, string description, string cible, int objectif, int or, int xp, int pierres = 1, Equipement? item = null, int rep = 30, int sceaux = 1, string difficulte = "⚔️ Normale", int niveauRequis = 1)
        {
            Id = id;
            Titre = titre;
            Description = description;
            CibleNom = cible;
            Objectif = objectif;
            Progression = 0;
            RecompenseOr = or;
            RecompenseXP = xp;
            RecompensePierresForge = pierres;
            RecompenseItem = item;
            RecompenseReputation = rep;
            RecompenseSceauxGuilde = sceaux;
            Difficulte = difficulte;
            NiveauRequis = niveauRequis;
        }
    }

    // ==============================================================
    // 5. JOUEUR & CARACTÉRISTIQUES
    // ==============================================================
    public class Joueur
    {
        public string Nom { get; set; } = "Héros";
        public ClasseType Classe { get; set; }
        public int Niveau { get; set; } = 1;
        public int XP { get; set; } = 0;
        public int XPRequisPourNiveau => (int)Math.Min(int.MaxValue, Math.Max(100d, 100d * Math.Pow(Math.Max(1, Niveau), 1.5d)));

        // Points d'attributs à distribuer
        public int PointsCaracteristiques { get; set; } = 0;
        public int Force { get; set; } = 5;       // Augmente Attaque
        public int Agilite { get; set; } = 5;     // Augmente Critique & Esquive
        public int Endurance { get; set; } = 5;   // Augmente PV Max & Défense
        public int Intelligence { get; set; } = 5;// Augmente Mana Max & Magie

        // Statistiques de base
        public int PVMaxBase { get; set; }
        public int PVActuels { get; set; }
        public int ManaMaxBase { get; set; }
        public int ManaActuel { get; set; }

        public int AttaqueBase { get; set; }
        public int DefenseBase { get; set; }

        // Bourse & Ressources
        public int Or { get; set; } = 100;
        public int PierresDeForge { get; set; } = 3;
        public int HerbesMagiques { get; set; } = 2;
        public int EcaillesDeMonstre { get; set; } = 1;
        public int ReputationGuilde { get; set; } = 0;
        public int SceauxDeGuilde { get; set; } = 0;

        // Arbre de talents du héros (points distribuables, 0 à 3 par talent)
        public int PointsTalents { get; set; } = 3;
        public Dictionary<string, int> TalentsNiveaux { get; set; } = new Dictionary<string, int>();

        public static readonly string[] TalentsDisponibles =
        {
            "Force du Héros",
            "Garde de Fer",
            "Résilience",
            "Arcane Furtive",
            "Vitesse de l'Ombre"
        };

        public int ObtenirBonusTalent(string nom)
        {
            if (TalentsNiveaux == null)
                TalentsNiveaux = new Dictionary<string, int>();

            return TalentsNiveaux.TryGetValue(nom, out int niveau) ? niveau : 0;
        }

        public bool AmeliorerTalent(string nom)
        {
            if (TalentsNiveaux == null)
                TalentsNiveaux = new Dictionary<string, int>();

            if (PointsTalents <= 0)
                return false;

            int niveauActuel = TalentsNiveaux.TryGetValue(nom, out int niveau) ? niveau : 0;
            if (niveauActuel >= 3)
                return false;

            TalentsNiveaux[nom] = niveauActuel + 1;
            PointsTalents--;
            return true;
        }

        public string ObtenirDescriptionTalent(string nom)
        {
            return nom switch
            {
                "Force du Héros" => "+6% d'attaque par niveau",
                "Garde de Fer" => "+5% de défense par niveau",
                "Résilience" => "+10% de PV max par niveau",
                "Arcane Furtive" => "+8% de mana max par niveau",
                "Vitesse de l'Ombre" => "+3% de critique et esquive par niveau",
                _ => "Bonus passif"
            };
        }

        public int ObtenirBonusPassifClasse()
        {
            return Classe switch
            {
                ClasseType.Guerrier => 12,
                ClasseType.Mage => 11,
                ClasseType.Rodeur => 10,
                ClasseType.Paladin => 14,
                ClasseType.Necromancien => 13,
                _ => 0
            };
        }

        public string ObtenirDescriptionPassiveClasse()
        {
            return Classe switch
            {
                ClasseType.Guerrier => "Guerrier : +12% de force de guerre, +8% défense",
                ClasseType.Mage => "Mage : +11% de mana, +10% critique",
                ClasseType.Rodeur => "Rodeur : +10% précision, +8% esquive",
                ClasseType.Paladin => "Paladin : +14% endurance sacrée, +6% soins",
                ClasseType.Necromancien => "Nécromancien : +13% vampirisme, +8% dégâts ombres",
                _ => "Passif : puissance de base"
            };
        }

        public string ObtenirRangGuilde()
        {
            if (ReputationGuilde >= 1200) return "👑 S - Souverain Mythique";
            if (ReputationGuilde >= 750) return "🌟 A - Légende Vivante";
            if (ReputationGuilde >= 450) return "⚔️ B - Champion d'Élite";
            if (ReputationGuilde >= 250) return "🛡️ C - Pourfendeur Vétéran";
            if (ReputationGuilde >= 120) return "🏹 D - Aventurier Éprouvé";
            if (ReputationGuilde >= 40) return "🗡️ E - Chasseur de Primes";
            return "🌱 F - Novice de la Guilde";
        }

        public int ObtenirBonusAttaqueGuilde()
        {
            if (ReputationGuilde >= 1200) return 20;
            if (ReputationGuilde >= 750) return 14;
            if (ReputationGuilde >= 450) return 9;
            if (ReputationGuilde >= 250) return 5;
            if (ReputationGuilde >= 120) return 2;
            return 0;
        }

        public int ObtenirBonusDefenseGuilde()
        {
            if (ReputationGuilde >= 1200) return 15;
            if (ReputationGuilde >= 750) return 10;
            if (ReputationGuilde >= 450) return 7;
            if (ReputationGuilde >= 250) return 4;
            if (ReputationGuilde >= 120) return 2;
            return 0;
        }

        // Jauge d'Ultime en combat (0 à 100)
        [System.Text.Json.Serialization.JsonIgnore]
        public int JaugeUltime { get; set; } = 0;

        // Buffs temporaires
        public int BuffReposCombatsRestants { get; set; } = 0; // +25% XP si repos à l'auberge

        // Consommables
        public Dictionary<TypeConsommable, int> InventaireConsommables { get; set; } = new Dictionary<TypeConsommable, int>();

        // Équipement équipé
        public Equipement? ArmeEquipee { get; set; }
        public Equipement? ArmureEquipee { get; set; }
        public Equipement? CasqueEquipe { get; set; }
        public Equipement? AnneauEquipe { get; set; }
        public Equipement? AmuletteEquipee { get; set; }

        // Sac à dos d'équipement
        public List<Equipement> SacEquipements { get; set; } = new List<Equipement>();

        // Matériaux de Craft & Artisanat
        public Dictionary<string, int> MateriauxCraft { get; set; } = new Dictionary<string, int>();

        public int ObtenirMateriau(string nom)
        {
            return MateriauxCraft.TryGetValue(nom, out int val) ? val : 0;
        }

        public void AjouterMateriau(string nom, int qte)
        {
            if (qte <= 0) return;
            if (MateriauxCraft.ContainsKey(nom))
                MateriauxCraft[nom] += qte;
            else
                MateriauxCraft[nom] = qte;
        }

        public bool ConsommerMateriau(string nom, int qte)
        {
            if (ObtenirMateriau(nom) < qte) return false;
            MateriauxCraft[nom] -= qte;
            if (MateriauxCraft[nom] <= 0) MateriauxCraft.Remove(nom);
            return true;
        }

        // Compétences & Sorts Débloqués (Crafting & Inné)
        public List<CompetenceCraftable> CompetencesDebloquees { get; set; } = new List<CompetenceCraftable>();
        public CompetenceCraftable? CompetenceEquipee { get; set; }

        public void AssurerSortsParDefaut()
        {
            if (CompetencesDebloquees == null) CompetencesDebloquees = new List<CompetenceCraftable>();

            CompetenceCraftable sortInne = Classe switch
            {
                ClasseType.Guerrier => new CompetenceCraftable("sort_guerrier_base", "Lancer de Hache Fracassant", "Projette une lourde hache perforant les armures ennemies.", 15, 1.85, TypeEffetCompetence.DegatsDirects, 0, "🪓"),
                ClasseType.Mage => new CompetenceCraftable("sort_mage_base", "Foudre Arcanique", "Décharge électrique perçante foudroyant les rangs adverses.", 18, 2.25, TypeEffetCompetence.DegatsDirects, 0, "⚡"),
                ClasseType.Rodeur => new CompetenceCraftable("sort_rodeur_base", "Flèche Explosive", "Tire un projectile instable qui détone avec force à l'impact.", 16, 2.10, TypeEffetCompetence.DegatsDirects, 0, "💣"),
                ClasseType.Paladin => new CompetenceCraftable("sort_paladin_base", "Rayon Sacré Purificateur", "Faisceau de lumière céleste calcinant les impies.", 20, 2.15, TypeEffetCompetence.DegatsDirects, 0, "☀️"),
                ClasseType.Necromancien => new CompetenceCraftable("sort_necro_base", "Essaim d'Âmes Voraces", "Libère trois esprits spectraux dévorant l'essence adverse.", 18, 1.95, TypeEffetCompetence.DegatsDirects, 0, "👻"),
                _ => new CompetenceCraftable("sort_guerrier_base", "Lancer de Hache Fracassant", "Projette une lourde hache.", 15, 1.85, TypeEffetCompetence.DegatsDirects, 0, "🪓")
            };

            if (!CompetencesDebloquees.Any(c => c.Id == sortInne.Id))
            {
                CompetencesDebloquees.Insert(0, sortInne);
            }
            if (CompetenceEquipee == null)
            {
                CompetenceEquipee = sortInne;
            }
        }

        // Tour Astrale Infinie
        public int EtageTourActuel { get; set; } = 1;
        public int EtageTourRecord { get; set; } = 0;
        public int EclatsAstraux { get; set; } = 0;
        public int OrAccumuleTour { get; set; } = 0;
        public int XPAccumuleTour { get; set; } = 0;
        public int EclatsAccumulesTour { get; set; } = 0;

        // Quêtes & Statistiques de jeu
        public List<Quete> QuetesActives { get; set; } = new List<Quete>();
        public List<int> QuetesCompleteesIds { get; set; } = new List<int>();
        public Dictionary<string, int> BestiaireMonstresTues { get; set; } = new Dictionary<string, int>();
        public int BossVaincusTotal { get; set; } = 0;
        public int CoffresTresorOuverts { get; set; } = 0;
        public int EtageTourAstraleMax { get; set; } = 0;

        // Calculs dynamiques de statistiques
        public int PVMaxTotal => PVMaxBase + (Endurance * 8)
            + (ArmureEquipee?.BonusPVMax ?? 0) + (ArmureEquipee?.NiveauAmelioration * 8 ?? 0)
            + (CasqueEquipe?.BonusPVMax ?? 0) + (CasqueEquipe?.NiveauAmelioration * 8 ?? 0)
            + (ArmeEquipee?.BonusPVMax ?? 0) + (ArmeEquipee?.NiveauAmelioration * 8 ?? 0)
            + (AnneauEquipe?.BonusPVMax ?? 0)
            + (AmuletteEquipee?.BonusPVMax ?? 0)
            + (int)(ObtenirBonusTalent("Résilience") * 0.10f * (PVMaxBase + (Endurance * 8)))
            + BonusPVSet
            + (Classe == ClasseType.Paladin ? 20 : 0)
            + (Classe == ClasseType.Guerrier ? 14 : 0);

        public int ManaMaxTotal => ManaMaxBase + (Intelligence * 6)
            + (ArmureEquipee?.BonusManaMax ?? 0)
            + (CasqueEquipe?.BonusManaMax ?? 0)
            + (ArmeEquipee?.BonusManaMax ?? 0)
            + (AnneauEquipe?.BonusManaMax ?? 0)
            + (AmuletteEquipee?.BonusManaMax ?? 0)
            + (ObtenirBonusTalent("Arcane Furtive") * 6)
            + (Classe == ClasseType.Mage ? 20 : 0)
            + (Classe == ClasseType.Necromancien ? 12 : 0);

        public int AttaqueTotale => AttaqueBase + (Force * 3) + ObtenirBonusAttaqueGuilde()
            + (ArmeEquipee?.BonusAttaque ?? 0) + (ArmeEquipee?.NiveauAmelioration * 3 ?? 0)
            + (ArmureEquipee?.BonusAttaque ?? 0) + (ArmureEquipee?.NiveauAmelioration * 3 ?? 0)
            + (CasqueEquipe?.BonusAttaque ?? 0) + (CasqueEquipe?.NiveauAmelioration * 3 ?? 0)
            + (AnneauEquipe?.BonusAttaque ?? 0) + (AnneauEquipe?.NiveauAmelioration * 3 ?? 0)
            + (AmuletteEquipee?.BonusAttaque ?? 0) + (AmuletteEquipee?.NiveauAmelioration * 3 ?? 0)
            + (int)(AttaqueBase * (ObtenirBonusTalent("Force du Héros") * 0.06f))
            + (int)(AttaqueBase * (ObtenirBonusPassifClasse() * 0.015f))
            + BonusAttaqueSet;

        public int DefenseTotale => DefenseBase + (Endurance * 2) + ObtenirBonusDefenseGuilde()
            + (ArmureEquipee?.BonusDefense ?? 0) + (ArmureEquipee?.NiveauAmelioration * 2 ?? 0)
            + (CasqueEquipe?.BonusDefense ?? 0) + (CasqueEquipe?.NiveauAmelioration * 2 ?? 0)
            + (ArmeEquipee?.BonusDefense ?? 0) + (ArmeEquipee?.NiveauAmelioration * 2 ?? 0)
            + (AnneauEquipe?.BonusDefense ?? 0) + (AnneauEquipe?.NiveauAmelioration * 2 ?? 0)
            + (AmuletteEquipee?.BonusDefense ?? 0) + (AmuletteEquipee?.NiveauAmelioration * 2 ?? 0)
            + (int)(DefenseBase * (ObtenirBonusTalent("Garde de Fer") * 0.05f))
            + (int)(DefenseBase * ((Classe == ClasseType.Paladin ? 12 : Classe == ClasseType.Guerrier ? 8 : Classe == ClasseType.Rodeur ? 4 : 0) * 0.01f))
            + BonusDefenseSet;

        public int ChanceCritiqueTotale => 5 + (Agilite * 2)
            + (ArmeEquipee?.BonusCritique ?? 0)
            + (ArmureEquipee?.BonusCritique ?? 0)
            + (CasqueEquipe?.BonusCritique ?? 0)
            + (AnneauEquipe?.BonusCritique ?? 0)
            + (AmuletteEquipee?.BonusCritique ?? 0)
            + (ObtenirBonusTalent("Vitesse de l'Ombre") * 2)
            + (Classe == ClasseType.Rodeur ? 8 : 0)
            + (Classe == ClasseType.Mage ? 5 : 0);

        public int ChanceEsquiveTotale => 3 + (Agilite * 1)
            + (ArmureEquipee?.BonusEsquive ?? 0)
            + (CasqueEquipe?.BonusEsquive ?? 0)
            + (ArmeEquipee?.BonusEsquive ?? 0)
            + (AnneauEquipe?.BonusEsquive ?? 0)
            + (AmuletteEquipee?.BonusEsquive ?? 0)
            + (ObtenirBonusTalent("Vitesse de l'Ombre") * 2)
            + (Classe == ClasseType.Rodeur ? 6 : 0)
            + (Classe == ClasseType.Paladin ? 4 : 0);

        public int VampirismeTotal => (ArmeEquipee?.BonusVampirisme ?? 0)
            + (ArmureEquipee?.BonusVampirisme ?? 0)
            + (CasqueEquipe?.BonusVampirisme ?? 0)
            + (AnneauEquipe?.BonusVampirisme ?? 0)
            + (AmuletteEquipee?.BonusVampirisme ?? 0)
            + (Classe == ClasseType.Necromancien ? 8 : 0)
            + (Classe == ClasseType.Paladin ? 3 : 0);

        private IEnumerable<Equipement> EquipementEquipe
        {
            get
            {
                if (ArmeEquipee != null) yield return ArmeEquipee;
                if (ArmureEquipee != null) yield return ArmureEquipee;
                if (CasqueEquipe != null) yield return CasqueEquipe;
                if (AnneauEquipe != null) yield return AnneauEquipe;
                if (AmuletteEquipee != null) yield return AmuletteEquipee;
            }
        }

        public int ObtenirPiecesSetEquipees(string identifiantSet)
        {
            return string.IsNullOrWhiteSpace(identifiantSet)
                ? 0
                : EquipementEquipe.Count(e => string.Equals(e.IdentifiantSet, identifiantSet, StringComparison.Ordinal));
        }

        public int BonusAttaqueSet => EquipementEquipe
            .Where(e => !string.IsNullOrWhiteSpace(e.IdentifiantSet))
            .GroupBy(e => e.IdentifiantSet, StringComparer.Ordinal)
            .Sum(g => g.Count() >= 4 ? 20 : g.Count() >= 2 ? 8 : 0);

        public int BonusDefenseSet => EquipementEquipe
            .Where(e => !string.IsNullOrWhiteSpace(e.IdentifiantSet))
            .GroupBy(e => e.IdentifiantSet, StringComparer.Ordinal)
            .Sum(g => g.Count() >= 3 ? 12 : 0);

        public int BonusPVSet => EquipementEquipe
            .Where(e => !string.IsNullOrWhiteSpace(e.IdentifiantSet))
            .GroupBy(e => e.IdentifiantSet, StringComparer.Ordinal)
            .Sum(g => g.Count() >= 4 ? 120 : 0);

        public string ObtenirResumeSets()
        {
            var ensembles = EquipementEquipe
                .Where(e => !string.IsNullOrWhiteSpace(e.IdentifiantSet))
                .GroupBy(e => e.IdentifiantSet, StringComparer.Ordinal)
                .Select(g => $"{NomSet2D.ObtenirNom(g.Key)} {g.Count()}/5");
            return string.Join(" • ", ensembles);
        }

        public Joueur() {
            TalentsNiveaux ??= new Dictionary<string, int>();
        }

        public Joueur(string nom, ClasseType classe)
        {
            Nom = nom;
            Classe = classe;

            // Initialisation selon la classe choisie
            switch (classe)
            {
                case ClasseType.Guerrier:
                    PVMaxBase = 140;
                    ManaMaxBase = 40;
                    AttaqueBase = 18;
                    DefenseBase = 12;
                    Force = 9; Endurance = 8; Agilite = 4; Intelligence = 2;
                    ArmeEquipee = new Equipement("Glaive d'Initié en Acier", TypeEquipement.Arme, Rarete.Commun, 6, 0, 0, 0, 4, 0, 0, 20);
                    ArmureEquipee = new Equipement("Plastron de Fer Battu", TypeEquipement.Armure, Rarete.Commun, 0, 6, 15, 0, 0, 0, 0, 25);
                    CasqueEquipe = new Equipement("Heaume du Soldat", TypeEquipement.Casque, Rarete.Commun, 0, 3, 10, 0, 0, 0, 0, 15);
                    break;

                case ClasseType.Mage:
                    PVMaxBase = 90;
                    ManaMaxBase = 120;
                    AttaqueBase = 24;
                    DefenseBase = 5;
                    Force = 2; Endurance = 4; Agilite = 5; Intelligence = 12;
                    ArmeEquipee = new Equipement("Bâton d'Ambre Runique", TypeEquipement.Arme, Rarete.Commun, 8, 0, 0, 15, 6, 0, 0, 20);
                    ArmureEquipee = new Equipement("Toge en Lin Tissée d'Éther", TypeEquipement.Armure, Rarete.Commun, 0, 3, 5, 20, 0, 0, 0, 25);
                    CasqueEquipe = new Equipement("Diadème d'Apprenti", TypeEquipement.Casque, Rarete.Commun, 0, 2, 5, 10, 0, 0, 0, 15);
                    break;

                case ClasseType.Rodeur:
                    PVMaxBase = 110;
                    ManaMaxBase = 65;
                    AttaqueBase = 20;
                    DefenseBase = 8;
                    Force = 5; Endurance = 5; Agilite = 10; Intelligence = 3;
                    ArmeEquipee = new Equipement("Arc Long en Bois d'If", TypeEquipement.Arme, Rarete.Commun, 7, 0, 0, 0, 10, 5, 0, 20);
                    ArmureEquipee = new Equipement("Gilet de Cuir Souple", TypeEquipement.Armure, Rarete.Commun, 0, 5, 10, 0, 0, 6, 0, 25);
                    CasqueEquipe = new Equipement("Capuche de Traqueur", TypeEquipement.Casque, Rarete.Commun, 0, 2, 8, 0, 3, 3, 0, 15);
                    break;

                case ClasseType.Paladin:
                    PVMaxBase = 135;
                    ManaMaxBase = 70;
                    AttaqueBase = 17;
                    DefenseBase = 14;
                    Force = 7; Endurance = 8; Agilite = 3; Intelligence = 5;
                    ArmeEquipee = new Equipement("Masse Sacrée de Dévot", TypeEquipement.Arme, Rarete.Commun, 6, 2, 10, 10, 2, 0, 0, 20);
                    ArmureEquipee = new Equipement("Armure Émaillée du Temple", TypeEquipement.Armure, Rarete.Commun, 0, 7, 20, 10, 0, 0, 0, 25);
                    CasqueEquipe = new Equipement("Casque Béni de Gardien", TypeEquipement.Casque, Rarete.Commun, 0, 4, 10, 5, 0, 0, 0, 15);
                    break;

                case ClasseType.Necromancien:
                    PVMaxBase = 100;
                    ManaMaxBase = 110;
                    AttaqueBase = 22;
                    DefenseBase = 6;
                    Force = 3; Endurance = 5; Agilite = 5; Intelligence = 10;
                    ArmeEquipee = new Equipement("Faux d'Os Sculptée", TypeEquipement.Arme, Rarete.Commun, 7, 0, 0, 10, 5, 0, 4, 20);
                    ArmureEquipee = new Equipement("Manteau des Ténèbres", TypeEquipement.Armure, Rarete.Commun, 0, 4, 8, 15, 0, 0, 2, 25);
                    CasqueEquipe = new Equipement("Masque Funéraire Sombre", TypeEquipement.Casque, Rarete.Commun, 0, 2, 6, 8, 2, 0, 0, 15);
                    break;
            }

            AnneauEquipe = new Equipement("Anneau de Cuivre Simple", TypeEquipement.Anneau, Rarete.Commun, 1, 1, 5, 5, 2, 1, 0, 15);
            AmuletteEquipee = new Equipement("Amulette de Bonne Fortune", TypeEquipement.Amulette, Rarete.Commun, 1, 1, 10, 5, 2, 1, 0, 20);

            PVActuels = PVMaxTotal;
            ManaActuel = ManaMaxTotal;

            // Consommables de départ
            InventaireConsommables[TypeConsommable.PotionSoinMineure] = 4;
            InventaireConsommables[TypeConsommable.PotionManaMineure] = 3;
            InventaireConsommables[TypeConsommable.BombeIncendiaire] = 1;

            // Sorts de départ
            AssurerSortsParDefaut();
        }

        public (int OrPerdu, int XPPerdu) AppliquerPenaliteMort()
        {
            int orPerdu = Or > 0 ? Math.Min(Or, (int)Math.Ceiling(Or * 0.10d)) : 0;
            int xpPerdu = XP > 0 ? Math.Min(XP, (int)Math.Ceiling(XP * 0.10d)) : 0;
            Or -= orPerdu;
            XP -= xpPerdu;
            return (orPerdu, xpPerdu);
        }

        public void GagnerXP(int quantite)
        {
            if (BuffReposCombatsRestants > 0)
            {
                int bonusXP = (int)(quantite * 0.25);
                quantite += bonusXP;
                BuffReposCombatsRestants--;
                UI.AfficherTexte($"✨ Bonus Repos de l'Auberge (+25%) : +{bonusXP} XP supplémentaire ! (Reste {BuffReposCombatsRestants} combats)", ConsoleColor.Cyan);
            }

            XP += quantite;
            UI.AfficherTexte($"⭐ Vous gagnez {quantite} points d'expérience !", ConsoleColor.Yellow);

            while (XP >= XPRequisPourNiveau)
            {
                XP -= XPRequisPourNiveau;
                Niveau++;
                PVMaxBase += 22;
                ManaMaxBase += 16;
                AttaqueBase += 3;
                DefenseBase += 2;
                PointsCaracteristiques += 4; // 4 points à répartir librement !
                PointsTalents += 1;

                PVActuels = PVMaxTotal;
                ManaActuel = ManaMaxTotal;

                UI.LigneSeparateur();
                UI.AfficherTexte($"🎉 NIVEAU SUPÉRIEUR ! Vous atteignez le NIVEAU {Niveau} !", ConsoleColor.Green);
                UI.AfficherTexte($"Vos caractéristiques augmentent, votre santé/mana sont régénérés,", ConsoleColor.Cyan);
                UI.AfficherTexte($"et vous obtenez 4 POINTS D'ATTRIBUTS à investir dans votre profil !", ConsoleColor.Magenta);
                UI.LigneSeparateur();
            }
        }

        public void Soigner(int pv)
        {
            PVActuels = Math.Min(PVMaxTotal, PVActuels + pv);
        }

        public void RestaurerMana(int mana)
        {
            ManaActuel = Math.Min(ManaMaxTotal, ManaActuel + mana);
        }

        public void RegenererMana(int mana) => RestaurerMana(mana);

        public void AjouterConsommable(TypeConsommable type, int quantite = 1)
        {
            if (!InventaireConsommables.ContainsKey(type))
                InventaireConsommables[type] = 0;
            InventaireConsommables[type] += quantite;
        }

        public bool UtiliserConsommable(TypeConsommable type)
        {
            if (!InventaireConsommables.ContainsKey(type) || InventaireConsommables[type] <= 0)
                return false;

            InventaireConsommables[type]--;
            return true;
        }

        public bool ConsommerObjet(TypeConsommable type) => UtiliserConsommable(type);

        public int ObtenirQuantiteConsommable(TypeConsommable type)
        {
            return InventaireConsommables.TryGetValue(type, out int q) ? q : 0;
        }

        public void EquiperObjet(Equipement nouvelEquipement)
        {
            Equipement? ancien = null;
            switch (nouvelEquipement.Type)
            {
                case TypeEquipement.Arme: ancien = ArmeEquipee; ArmeEquipee = nouvelEquipement; break;
                case TypeEquipement.Armure: ancien = ArmureEquipee; ArmureEquipee = nouvelEquipement; break;
                case TypeEquipement.Casque: ancien = CasqueEquipe; CasqueEquipe = nouvelEquipement; break;
                case TypeEquipement.Anneau: ancien = AnneauEquipe; AnneauEquipe = nouvelEquipement; break;
                case TypeEquipement.Amulette: ancien = AmuletteEquipee; AmuletteEquipee = nouvelEquipement; break;
            }

            SacEquipements.Remove(nouvelEquipement);
            if (ancien != null)
            {
                SacEquipements.Add(ancien);
            }

            // Réajustement des PV/Mana si dépassement
            if (PVActuels > PVMaxTotal) PVActuels = PVMaxTotal;
            if (ManaActuel > ManaMaxTotal) ManaActuel = ManaMaxTotal;
        }

        public (int ChangementsCount, List<string> Details, int GainAttaque, int GainCritique, int AncienneAtk, int AncienCrit) AutoEquiperMeilleurStuffDegats()
        {
            int atkAvant = AttaqueTotale;
            int critAvant = ChanceCritiqueTotale;

            var slots = new (TypeEquipement Type, string Nom, Func<Equipement?> ObtenirActuel)[]
            {
                (TypeEquipement.Arme, "🗡️ Arme", () => ArmeEquipee),
                (TypeEquipement.Armure, "🛡️ Armure", () => ArmureEquipee),
                (TypeEquipement.Casque, "🪖 Heaume", () => CasqueEquipe),
                (TypeEquipement.Anneau, "💍 Anneau", () => AnneauEquipe),
                (TypeEquipement.Amulette, "📿 Amulette", () => AmuletteEquipee)
            };

            List<string> details = new List<string>();
            int nbChangements = 0;

            foreach (var slot in slots)
            {
                Equipement? actuel = slot.ObtenirActuel();
                double scoreActuel = actuel?.ObtenirScoreDegats() ?? -9999.0;

                var candidats = SacEquipements.Where(e => e.Type == slot.Type).ToList();
                if (candidats.Count == 0) continue;

                Equipement meilleur = candidats.OrderByDescending(e => e.ObtenirScoreDegats()).First();
                double scoreMeilleur = meilleur.ObtenirScoreDegats();

                if (scoreMeilleur > scoreActuel)
                {
                    string nomAncien = actuel != null ? actuel.ObtenirNomComplet() : "Aucun";
                    int atkAncien = actuel != null ? (actuel.BonusAttaque + (actuel.NiveauAmelioration * 3)) : 0;
                    int atkNouveau = meilleur.BonusAttaque + (meilleur.NiveauAmelioration * 3);
                    int diffAtk = atkNouveau - atkAncien;

                    int critAncien = actuel?.BonusCritique ?? 0;
                    int critNouveau = meilleur.BonusCritique;
                    int diffCrit = critNouveau - critAncien;

                    EquiperObjet(meilleur);
                    nbChangements++;

                    List<string> statsModifs = new List<string>();
                    if (diffAtk != 0) statsModifs.Add($"{(diffAtk > 0 ? "+" : "")}{diffAtk} Atk");
                    if (diffCrit != 0) statsModifs.Add($"{(diffCrit > 0 ? "+" : "")}{diffCrit}% Crit");
                    if (statsModifs.Count == 0) statsModifs.Add($"+{(int)(scoreMeilleur - scoreActuel)} pts");

                    details.Add($"• {slot.Nom} : {nomAncien} ➜ {meilleur.ObtenirNomComplet()} ({string.Join(", ", statsModifs)})");
                }
            }

            int gainAtk = AttaqueTotale - atkAvant;
            int gainCrit = ChanceCritiqueTotale - critAvant;

            return (nbChangements, details, gainAtk, gainCrit, atkAvant, critAvant);
        }
    }

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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Minerai de Fer", 5 }, { "Morceau de Cuir", 4 }, { "Croc Sauvage", 2 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Os Renforcé", 5 }, { "Ectoplasme", 4 }, { "Pierre d'Âme", 3 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Noyau de Givre", 4 }, { "Acier Trempé", 4 }, { "Pierre d'Âme", 3 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Écaille Draconique", 6 }, { "Cœur Ardent", 4 }, { "Acier Trempé", 5 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Matière du Néant", 5 }, { "Cristal Cosmique", 4 }, { "Éclat Astral", 8 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Sang de Démon", 6 }, { "Cœur Ardent", 4 }, { "Écaille Draconique", 4 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Ectoplasme", 6 }, { "Pierre d'Âme", 4 }, { "Os Renforcé", 5 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Cœur de Titan", 4 }, { "Acier Trempé", 8 }, { "Éclat Astral", 3 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Venin Obscur", 5 }, { "Cuir Épais", 5 }, { "Écaille Draconique", 3 } };
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

        public override Dictionary<string, int> ObtenirLootMateriaux()
        {
            return new Dictionary<string, int> {
                { "Matière du Néant", 5 },
                { "Cristal Cosmique", 3 },
                { "Éclat Astral", 8 }
            };
        }
    }

    // ==============================================================
    // 20 NOUVEAUX BOSS ÉPIQUES & MYTHIQUES (BOSS 11 À 30)
    // ==============================================================

    // --- BOSS 11 : GORROK LE BROYEUR DES TERRES SAUVAGES (Niv. 2) ---
    public class BossGorrok : Monstre
    {
        public BossGorrok() : base("Gorrok le Broyeur des Terres Sauvages", 250, 24, 10, 420, 210, true,
            "GROOOOAAAR ! MON GROIN VA T'ÉCRASER CONTRE LE ROC !", "Tranchant / Feu", "Sanglier colossal enragé ayant terrassé des dizaines de chasseurs royaux.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(12, (int)(Attaque * 1.55) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🐗 CHARGE BESTIALE FRACASSANTE ! {Nom} vous percute de plein fouet pour {degats} dégâts !";
                return degats;
            }
            int d = Math.Max(6, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🐗 {Nom} vous laboure avec ses défenses aiguisées pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Cuir Épais", 4 }, { "Croc Sauvage", 3 }, { "Minerai de Fer", 3 } };
    }

    // --- BOSS 12 : KRAGH L'ÉCORCHEUR DE LA HORDE (Niv. 3) ---
    public class BossKragh : Monstre
    {
        public BossKragh() : base("Kragh l'Écorcheur de la Horde Noire", 310, 28, 11, 500, 250, true,
            "Kikiki ! Un coup dans l'ombre et tes boyaux se répandront !", "Magie Pure", "Assassin gobelin sournois muni de dagues dentelées trempées dans l'acide.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(14, (int)(Attaque * 1.6) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🗡️ FRAPPE SOURNOISE DANS L'OMBRE ! {Nom} transperce vos points faibles pour {degats} dégâts venimeux !";
                return degats;
            }
            int d = Math.Max(8, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🗡️ {Nom} lacère vos protections de ses doubles dagues pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Minerai de Fer", 6 }, { "Venin Corrosif", 2 }, { "Cuir Épais", 3 } };
    }

    // --- BOSS 13 : SKULDIR LE ROI SQUELETTE CRYOMANCIEN (Niv. 4) ---
    public class BossSkuldir : Monstre
    {
        public BossSkuldir() : base("Skuldir, Roi Squelette Cryomancien", 410, 32, 15, 640, 320, true,
            "LE FROID DE LA TOMBE PÉNÈTRERA TES OS JUSQU'À LA MOELLE !", "Feu Sacré", "Monarque antique réanimé par une magie de glace éternelle.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int degats = Math.Max(16, (int)(Attaque * 1.65) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"❄️ PIEUX DE GLACE NÉCROTIQUE ! {Nom} empale votre chair avec des stalactites pour {degats} dégâts de givre !";
                return degats;
            }
            int d = Math.Max(9, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"❄️ {Nom} assène un coup de sceptre gelé infligeant {d} dégâts de froid.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Os Renforcé", 5 }, { "Noyau de Givre", 3 }, { "Ectoplasme", 3 } };
    }

    // --- BOSS 14 : ZULGAR LE CHAMAN PUTRIDE (Niv. 5) ---
    public class BossZulgar : Monstre
    {
        public BossZulgar() : base("Zulgar le Chaman Putride", 490, 35, 16, 750, 380, true,
            "LES ESPRITS ANCIENS ME RÉCLAMENT TON ÂME ET TES ENTRAILLES !", "Foudre / Sacré", "Maître des arts occultes orcs invoquant des effluves corrompus.")
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
                messageAction = $"🧪 TOTEM DE GUÉRISON PUTRIDE ! {Nom} se soigne de {soin} PV et vous foudroie de miasmes pour {degats} dégâts !";
                return degats;
            }
            int d = Math.Max(10, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🪓 {Nom} abat son bâton d'os totémique pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Os Renforcé", 4 }, { "Ectoplasme", 4 }, { "Herbes Magiques", 4 } };
    }

    // --- BOSS 15 : VESPERA LA MATRIARCHE DES HARPIES (Niv. 5) ---
    public class BossVespera : Monstre
    {
        public BossVespera() : base("Vespera, Matriarche des Harpies", 460, 38, 13, 780, 400, true,
            "PERSONNE N'ÉCHAPPE AUX GRIFFES DES HAUTS VENTS !", "Distance / Perforant", "Reine sanguinaire des falaises embrumées fendant les cieux.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(18, (int)(Attaque * 1.7) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🌪️ OURAGAN DE SERRES ! {Nom} plonge en piqué et déchire vos chairs pour {degats} dégâts aériens !";
                return degats;
            }
            int d = Math.Max(11, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🦅 {Nom} vous cingle de ses ailes aux plumes tranchantes pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Plume Aérienne", 5 }, { "Cuir Épais", 4 }, { "Croc Sauvage", 3 } };
    }

    // --- BOSS 16 : BALTHAZAR LE CHEVALIER NOIR DÉCHU (Niv. 6) ---
    public class BossBalthazar : Monstre
    {
        public BossBalthazar() : base("Balthazar, Chevalier Noir Déchu", 620, 40, 23, 950, 480, true,
            "MON SERMENT EST MORT AVEC MON ROI ! SEUL RESTE LE FER !", "Magie Sombre / Foudre", "Ancien paladin de la garde royale corrompu par la soif de puissance.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int degats = Math.Max(20, (int)(Attaque * 1.75) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚔️ FRAPPE DU SERMENT BRISÉ ! {Nom} abat son espadon d'acier noir pour {degats} dégâts titanesques !";
                return degats;
            }
            int d = Math.Max(12, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🛡️ {Nom} vous percute de son pavois noir et enchaîne pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Acier Trempé", 5 }, { "Minerai de Fer", 7 }, { "Pierre d'Âme", 2 } };
    }

    // --- BOSS 17 : KRYLL L'ARACHNIDE DE NÉCROSE (Niv. 7) ---
    public class BossKryll : Monstre
    {
        public BossKryll() : base("Kryll l'Arachnide de Nécrose", 690, 44, 20, 1100, 540, true,
            "Sssss... Emprisonné dans ma soie, tu serviras de couveuse vivante !", "Feu Purificateur", "Arachnide géante des profondeurs souterraines aux crochets suintants de poison.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(22, (int)(Attaque * 1.5) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🕸️ MORSURE NÉCROTIQUE TOXIQUE ! {Nom} inocule un venin rongeur de chair pour {degats} dégâts venimeux !";
                return degats;
            }
            int d = Math.Max(13, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🕷️ {Nom} vous transperce d'une patte acérée comme une javeline pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Venin Obscur", 4 }, { "Cuir Épais", 5 }, { "Ectoplasme", 3 } };
    }

    // --- BOSS 18 : MAGMARION LE COLOSSE DE BRAISE (Niv. 7) ---
    public class BossMagmarion : Monstre
    {
        public BossMagmarion() : base("Magmarion, Colosse de Braise Éveillé", 770, 46, 24, 1200, 600, true,
            "LA TERRE BRÛLE SOUS MES PAS ! FUSIONNE AVEC LA LAVE !", "Glace / Eau Bénite", "Entité élémentaire née au cœur des cratères volcaniques.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(24, (int)(Attaque * 1.7) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🌋 CATACLYSME PYROCLASTIQUE ! {Nom} expulse une geyser de lave brûlante pour {degats} dégâts de feu !";
                return degats;
            }
            int d = Math.Max(14, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🔥 {Nom} vous écrase de son poing de basalte en fusion pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Cœur Ardent", 4 }, { "Acier Trempé", 5 }, { "Écaille Draconique", 3 } };
    }

    // --- BOSS 19 : SYLVANA LA REINE DES RONCES (Niv. 8) ---
    public class BossSylvana : Monstre
    {
        public BossSylvana() : base("Sylvana la Reine des Ronces Sépulcrales", 820, 48, 22, 1350, 680, true,
            "LES RACINES ANCIENNES BOIRONT VOTRE SANG POUR PURIFIER LA FORÊT !", "Feu Dévorant", "Dryade corrompue par des énergies nécrotiques contrôlant la flore épineuse.")
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
                messageAction = $"🌿 RONCES VAMPIRIQUES MAUDITES ! {Nom} draine {siphon} PV et vous inflige {degats} dégâts végétaux !";
                return degats;
            }
            int d = Math.Max(14, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🌵 {Nom} vous fustige de fouets d'épines acérées pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Herbes Magiques", 8 }, { "Ectoplasme", 5 }, { "Pierre d'Âme", 3 } };
    }

    // --- BOSS 20 : OBSIDIUS LE TITAN DE VERRE NOIR (Niv. 8) ---
    public class BossObsidius : Monstre
    {
        public BossObsidius() : base("Obsidius, Titan de Verre Noir", 920, 50, 30, 1500, 750, true,
            "JE SUIS LE CRISTAL NOIR INDESTRUCTIBLE ! BRISERA CELUI QUI ME TOUCHE !", "Frappe Lourde / Marteau", "Colosse taillé dans les profondeurs de verre volcanique impénétrable.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int degats = Math.Max(26, (int)(Attaque * 1.8) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"💎 FRACAS D'OBSIDIENNE TRANCHANTE ! {Nom} abat son bras de roche coupante pour {degats} dégâts massifs !";
                return degats;
            }
            int d = Math.Max(15, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🪨 {Nom} vous écrase lourdement pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Acier Trempé", 6 }, { "Cœur de Titan", 2 }, { "Éclat Astral", 2 } };
    }

    // --- BOSS 21 : NOX LE SPECTRE DÉVOREUR DE LUMIÈRE (Niv. 9) ---
    public class BossNox : Monstre
    {
        public BossNox() : base("Nox, Spectre Dévoreur de Lumière", 860, 54, 22, 1650, 820, true,
            "LA LUMIÈRE MEURT ICI... TON DERNIER SOUFFLE SERA NOIR ET GLACIAL !", "Sacré Pur", "Entité d'ombre tapie dans les failles stellaires dévorant les âmes.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(28, (int)(Attaque * 1.7) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                joueur.ManaActuel = Math.Max(0, joueur.ManaActuel - 20);
                messageAction = $"🌑 ÉCLIPSE ABSORBANTE D'ÂME ! {Nom} vous submerge de ténèbres (-20 Mana) pour {degats} dégâts spectraux !";
                return degats;
            }
            int d = Math.Max(16, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"👻 {Nom} vous traverse de ses griffes impalpables pour {d} dégâts de froid glacial.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Ectoplasme", 7 }, { "Pierre d'Âme", 4 }, { "Éclat Astral", 3 } };
    }

    // --- BOSS 22 : THALOR LE GÉNÉRAL SANS-TÊTE (Niv. 10) ---
    public class BossThalor : Monstre
    {
        public BossThalor() : base("Général Thalor le Sans-Tête", 1020, 58, 27, 1900, 950, true,
            "MA TÊTE FUT TRANCHÉE, MAIS MA VENGEANCE EST ÉTERNELLE !", "Feu / Foudre", "Légat des légions impériales spectrales chevauchant un cauchemar de feu.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 30)
            {
                int degats = Math.Max(32, (int)(Attaque * 1.85) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🐎 CHARGE DE CAUCHEMAR SPECTRAL ! {Nom} vous piétine sauvagement pour {degats} dégâts dévastateurs !";
                return degats;
            }
            int d = Math.Max(18, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"⚔️ {Nom} abat sa grande hallebarde fantomatique pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Acier Trempé", 7 }, { "Sang de Démon", 3 }, { "Pierre d'Âme", 3 } };
    }

    // --- BOSS 23 : ZEPHYROS LE SEIGNEUR DES OURAGANS (Niv. 11) ---
    public class BossZephyros : Monstre
    {
        public BossZephyros() : base("Zephyros, Archi-Seigneur des Ouragans", 1180, 63, 29, 2200, 1100, true,
            "QUE LA FOUDRE DU CIEL PURIFIE CE BAS MONDE !", "Tellurique / Terre", "Titan céleste régnant au sommet de la Tour Astrale et déchaînant les tempêtes.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(35, (int)(Attaque * 1.8) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚡⚡ FOUDRE ASCENDANTE EN CHAÎNE ! {Nom} électrocute votre système nerveux pour {degats} DÉGÂTS ÉLECTRIQUES !";
                return degats;
            }
            int d = Math.Max(20, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🌪️ {Nom} vous propulse de bourrasques tranchantes pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Éclat Astral", 6 }, { "Acier Trempé", 6 }, { "Noyau de Givre", 4 } };
    }

    // --- BOSS 24 : FENRIR DE LA LUNE DE SANG (Niv. 12) ---
    public class BossFenrirLuneSang : Monstre
    {
        public int ToursCombat { get; set; } = 0;

        public BossFenrirLuneSang() : base("Fenrir, Loup Ancestral de la Lune Rouge", 1320, 69, 29, 2600, 1300, true,
            "AWOOOOOO ! LA LUNE ÉCARLATE RÉCLAME UN FESTIN DE CHAIR ET DE SANG !", "Argent Pur / Sacré", "Prédateur primauté dont la soif de sang augmente à chaque seconde passée au combat.")
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
                messageAction = $"🩸 MORSURE CAROTIDIENNE FRÉNÉTIQUE ! {Nom} vous égorge (+{bonusFrenesie} ATK de frénésie) pour {degats} dégâts sanglants !";
                return degats;
            }
            int d = Math.Max(22, (Attaque + bonusFrenesie) - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🐺 {Nom} bondit de ses crocs acérés et vous arrache de la chair pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Sang de Démon", 6 }, { "Cuir Épais", 8 }, { "Croc Sauvage", 6 } };
    }

    // --- BOSS 25 : L'ARCHONTE SOLAIRE DÉCHU (Niv. 13) ---
    public class BossArchonteSolaire : Monstre
    {
        public BossArchonteSolaire() : base("L'Archonte Solaire Déchu de la Flamme Blanche", 1480, 73, 35, 3000, 1500, true,
            "LE FEU SACRÉ DES CIEUX VOUS CONSUMERA JUSQU'AUX CENDRES !", "Ombre / Ténèbres", "Ange guerrier aux ailes de platine incandescent foudroyant les impurs.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(42, (int)(Attaque * 1.85) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"☀️ NOVA BLANCHE PURIFICATRICE ! {Nom} libère l'éclat du soleil sacré pour {degats} dégâts radiants !";
                return degats;
            }
            int d = Math.Max(24, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🗡️ {Nom} transperce vos défenses de sa lance de lumière pure pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Éclat Astral", 8 }, { "Cristal Cosmique", 2 }, { "Cœur Ardent", 4 } };
    }

    // --- BOSS 26 : LE KRAKEN DES ABÎMES PRIMORDIAUX (Niv. 14) ---
    public class BossKrakenAbyssal : Monstre
    {
        public BossKrakenAbyssal() : base("Le Kraken des Abîmes Primordiaux", 1650, 77, 37, 3500, 1700, true,
            "LES ABÎMES PROFONDS RÉCLAMENT TON CORPS ET TON ÂME !", "Foudre Céleste", "Terreur titanesque des gouffres océaniques souterrains brisant la roche comme du verre.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(45, (int)(Attaque * 1.9) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"🐙 BROYAGE DE TENTACULES TITANESQUES ! {Nom} écrase votre squelette pour {degats} dégâts colossaux !";
                return degats;
            }
            int d = Math.Max(26, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🌊 {Nom} vous fouette d'un tentacule d'écailles sombres pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Venin Obscur", 6 }, { "Écaille Draconique", 6 }, { "Cœur de Titan", 2 } };
    }

    // --- BOSS 27 : CHRONOS LE MAÎTRE DU TEMPS (Niv. 15) ---
    public class BossChronos : Monstre
    {
        public BossChronos() : base("Chronos, Seigneur des Sabliers Brisés", 1790, 81, 39, 4000, 2000, true,
            "LE TEMPS M'OBÉIT. TES COUPS NE SONT QU'UN ÉCHO PASSÉ !", "Chaos / Ultime", "Entité dimensionnelle capable de plier le cours temporel à son bon vouloir.")
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
                messageAction = $"⏳ REMBOBINAGE TEMPOREL ! {Nom} remonte le temps et régénère {soin} PV, puis vous frappe pour {degats} dégâts !";
                return degats;
            }
            else if (rng.Next(100) < 40)
            {
                int degats = Math.Max(48, (int)(Attaque * 1.9) - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚡ ACCÉLÉRATION CHRONO-DIMENSIONNELLE ! {Nom} frappe deux fois dans la même seconde pour {degats} dégâts temporels !";
                return degats;
            }
            int d = Math.Max(28, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"⌛ {Nom} projette des sables temporels corrosifs pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Cristal Cosmique", 4 }, { "Éclat Astral", 9 }, { "Pierre d'Âme", 5 } };
    }

    // --- BOSS 28 : LÉVIATHAN DES GALAXIES (Niv. 16) ---
    public class BossLeviathanStellaire : Monstre
    {
        public BossLeviathanStellaire() : base("Léviathan des Galaxies Oubliées", 1990, 87, 43, 4600, 2300, true,
            "JE VOYAGE ENTRE LES ÉTOILES... TU N'ES QU'UN GRAIN DE POUSSIÈRE !", "Sacré / Feu Divin", "Bête cosmique se nourrissant de nébuleuses et naviguant à travers l'espace profond.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(52, (int)(Attaque * 2.0) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                joueur.ManaActuel = Math.Max(0, joueur.ManaActuel - 30);
                messageAction = $"🌌 RAZ-DE-MARÉE GRAVITATIONNEL ! {Nom} courbe l'espace autour de vous (-30 Mana) pour {degats} DÉGÂTS STELLAIRES !";
                return degats;
            }
            int d = Math.Max(30, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"☄️ {Nom} expulse une onde de matière cométaire pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Cristal Cosmique", 5 }, { "Éclat Astral", 12 }, { "Cœur de Titan", 4 } };
    }

    // --- BOSS 29 : ABADDON LE POURFENDEUR DE MONDES (Niv. 18) ---
    public class BossAbaddon : Monstre
    {
        public BossAbaddon() : base("Abaddon, Pourfendeur de Mondes", 2250, 95, 46, 5800, 2900, true,
            "L'APOCALYPSE EST LÀ ! AUCUN REFUGE, AUCUN SALUT !", "Lumière Primordiale", "Archange de la ruine portant une faux forgée dans le cœur d'une étoile morte.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;
            if (rng.Next(100) < 35)
            {
                int degats = Math.Max(60, (int)(Attaque * 2.1) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"💀 FAUX D'EXTERMINATION APOCALYPTIQUE ! {Nom} fauche votre essence pour {degats} DÉGÂTS DÉVASTATEURS !";
                return degats;
            }
            int d = Math.Max(34, Attaque - joueur.DefenseTotale);
            joueur.PVActuels = Math.Max(0, joueur.PVActuels - d);
            messageAction = $"🔥 {Nom} déchaîne une onde de feu noir de soufre pour {d} dégâts.";
            return d;
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> { { "Matière du Néant", 4 }, { "Sang de Démon", 7 }, { "Cristal Cosmique", 4 } };
    }

    // --- BOSS 30 : DEUS EX NIHILO L'ARCHITECTE DU CHAOS (Niv. 20 - BOSS SUPRÊME ULTIME) ---
    public class BossDeusExNihilo : Monstre
    {
        public bool PhaseChaos { get; set; } = false;

        public BossDeusExNihilo() : base("Deus Ex Nihilo, Architecte du Chaos Primordial", 3600, 115, 58, 15000, 10000, true,
            "JE SUIS CE QUI ÉTAIT AVANT LA CRÉATION. JE SUIS LE RIEN ET LE TOUT. EFFACE-TOI !", "Aucune / Volonté Inébranlable", "L'Entité Divine Cosmique Suprême, créatrice et destructrice de l'univers.")
        { }

        public override int Attaquer(Joueur joueur, out string messageAction)
        {
            Random rng = Random.Shared;

            if (!PhaseChaos && PVActuels <= PVMax * 0.35)
            {
                PhaseChaos = true;
                Attaque += 28;
                messageAction = $"🌌 L'UNIVERS ENTIER HURLE DANS LE SILENCE ABSOLU ! {Nom} LIBÈRE SA FORME DE CHAOS PRIMORDIAL SUPRÊME (+28 ATK) !";
                return 0;
            }

            int action = rng.Next(100);
            if (action < 30) // Trou Noir Primordial
            {
                int degats = Math.Max(70, 130 - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                joueur.ManaActuel = 0;
                joueur.JaugeUltime = Math.Max(0, joueur.JaugeUltime - 50);
                messageAction = $"🕳️ TROU NOIR DE L'ANNIHILATION ! L'espace s'effondre sur vous ! {degats} DÉGÂTS PURS (Mana vidé, -50% Ultime) !";
                return degats;
            }
            else if (action < 65) // Rayon Cosmique de Genèse
            {
                int degats = Math.Max(55, (int)(Attaque * 2.1) - (joueur.DefenseTotale / 2));
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                int siphon = 45;
                PVActuels = Math.Min(PVMax, PVActuels + siphon);
                messageAction = $"🌟 RAYON DE GENÈSE PRIMORDIALE ! Faisceau d'antimatière pure de {degats} dégâts (il régénère {siphon} PV) !";
                return degats;
            }
            else
            {
                int degats = Math.Max(40, Attaque - joueur.DefenseTotale);
                joueur.PVActuels = Math.Max(0, joueur.PVActuels - degats);
                messageAction = $"⚡ {Nom} foudroie votre armure d'une pluie d'ondes gravitationnelles pour {degats} dégâts.";
                return degats;
            }
        }

        public override Dictionary<string, int> ObtenirLootMateriaux() =>
            new Dictionary<string, int> {
                { "Essence du Chaos", 4 },
                { "Matière du Néant", 8 },
                { "Cristal Cosmique", 6 },
                { "Éclat Astral", 16 }
            };
    }

    // ==============================================================
    // SYSTÈME DE CRAFT & ARTISANAT (RECETTES, SORTS & ATELIER)
    // ==============================================================
    public enum TypeEffetCompetence
    {
        DegatsDirects,
        Saignement,
        Brulure,
        Poison,
        Etourdissement,
        SoinEtDegats,
        Bouclier,
        PerforantArmure,
        DrainMana
    }

    public class CompetenceCraftable
    {
        public string Id { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Description { get; set; } = "";
        public int CoutMana { get; set; } = 25;
        public double MultiplicateurDegats { get; set; } = 1.8;
        public TypeEffetCompetence Effet { get; set; } = TypeEffetCompetence.DegatsDirects;
        public int ValeurEffet { get; set; } = 0;
        public string Icone { get; set; } = "🔮";

        public CompetenceCraftable() { }

        public CompetenceCraftable(string id, string nom, string desc, int mana, double multi, TypeEffetCompetence effet, int valEffet, string icone)
        {
            Id = id;
            Nom = nom;
            Description = desc;
            CoutMana = mana;
            MultiplicateurDegats = multi;
            Effet = effet;
            ValeurEffet = valEffet;
            Icone = icone;
        }
    }

    public enum CategorieCraft
    {
        Arme,
        Armure,
        Bijou,
        Competence,
        Consommable
    }

    public class RecetteCraft
    {
        public string Id { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Description { get; set; } = "";
        public CategorieCraft Categorie { get; set; }
        public Rarete RareteItem { get; set; }
        public int NiveauRequis { get; set; } = 1;
        public int CoutOr { get; set; } = 100;
        public Dictionary<string, int> MateriauxRequis { get; set; } = new Dictionary<string, int>();

        public Func<Equipement>? GenererEquipement { get; set; }
        public CompetenceCraftable? CompetenceResultat { get; set; }
        public TypeConsommable? ConsommableResultat { get; set; }
        public int QuantiteConsommable { get; set; } = 1;

        public bool PeutFabriquer(Joueur joueur)
        {
            if (joueur.Or < CoutOr || joueur.Niveau < NiveauRequis) return false;
            foreach (var mat in MateriauxRequis)
            {
                if (joueur.ObtenirMateriau(mat.Key) < mat.Value) return false;
            }
            return true;
        }
    }

    public static class CatalogueCraft
    {
        public static List<RecetteCraft> Recettes { get; } = new List<RecetteCraft>();

        static CatalogueCraft()
        {
            InitialiserCatalogue();
        }

        private static void InitialiserCatalogue()
        {
            // === 1. ARMES FORGEABLES (Drops classiques + Armes légendaires exclusives) ===
            Recettes.Add(new RecetteCraft {
                Id = "w_fer", Nom = "Épée Courte en Fer Forgé",
                Description = "+10 Attaque, +4% Critique. Idéale pour débuter l'aventure.",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Commun, NiveauRequis = 1, CoutOr = 60,
                MateriauxRequis = new Dictionary<string, int> { { "Minerai de Fer", 3 }, { "Morceau de Cuir", 2 } },
                GenererEquipement = () => new Equipement("Épée Courte en Fer Forgé", TypeEquipement.Arme, Rarete.Commun, 10, 0, 0, 0, 4, 0, 0, 50)
            });

            Recettes.Add(new RecetteCraft {
                Id = "w_dague_ombre", Nom = "Dague Dentelée de l'Ombre",
                Description = "+16 Attaque, +8% Critique, +4% Esquive. Frappe sournoise rapide.",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Rare, NiveauRequis = 2, CoutOr = 140,
                MateriauxRequis = new Dictionary<string, int> { { "Minerai de Fer", 5 }, { "Croc Sauvage", 3 }, { "Cuir Épais", 2 } },
                GenererEquipement = () => new Equipement("Dague Dentelée de l'Ombre", TypeEquipement.Arme, Rarete.Rare, 16, 0, 0, 0, 8, 4, 0, 110)
            });

            Recettes.Add(new RecetteCraft {
                Id = "w_glaive_arcanique", Nom = "Glaive Arcanique Renforcé",
                Description = "+24 Attaque, +4 Défense, +30 Mana, +8% Critique. Forgé aux runes spectrales.",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Epique, NiveauRequis = 4, CoutOr = 320,
                MateriauxRequis = new Dictionary<string, int> { { "Acier Trempé", 4 }, { "Ectoplasme", 3 }, { "Os Renforcé", 3 } },
                GenererEquipement = () => new Equipement("Glaive Arcanique Renforcé", TypeEquipement.Arme, Rarete.Epique, 24, 4, 0, 30, 8, 0, 0, 240)
            });

            Recettes.Add(new RecetteCraft {
                Id = "w_masse_givre", Nom = "Masse Frigorigène d'Élite",
                Description = "+30 Attaque, +6 Défense, +12% Critique, +15 Mana. Gèle la chair des adversaires.",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Epique, NiveauRequis = 5, CoutOr = 480,
                MateriauxRequis = new Dictionary<string, int> { { "Noyau de Givre", 4 }, { "Acier Trempé", 5 }, { "Pierre d'Âme", 2 } },
                GenererEquipement = () => new Equipement("Masse Frigorigène d'Élite", TypeEquipement.Arme, Rarete.Epique, 30, 6, 0, 15, 12, 0, 0, 350)
            });

            Recettes.Add(new RecetteCraft {
                Id = "w_lame_draconique", Nom = "Lame Ardent du Pourfendeur",
                Description = "+40 Attaque, +10 Défense, +15% Critique, +5% Vampirisme. Trempée dans le sang d'Ignis.",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Legendaire, NiveauRequis = 8, CoutOr = 900,
                MateriauxRequis = new Dictionary<string, int> { { "Écaille Draconique", 6 }, { "Cœur Ardent", 4 }, { "Acier Trempé", 6 } },
                GenererEquipement = () => new Equipement("Lame Ardent du Pourfendeur", TypeEquipement.Arme, Rarete.Legendaire, 40, 10, 0, 0, 15, 0, 5, 750)
            });

            Recettes.Add(new RecetteCraft {
                Id = "w_trident_belial", Nom = "Trident Infernal du Purgatoire",
                Description = "+48 Attaque, +12 Défense, +40 PV, +18% Critique, +6% Vampirisme.",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Legendaire, NiveauRequis = 10, CoutOr = 1400,
                MateriauxRequis = new Dictionary<string, int> { { "Sang de Démon", 6 }, { "Cœur Ardent", 4 }, { "Écaille Draconique", 5 } },
                GenererEquipement = () => new Equipement("Trident Infernal du Purgatoire", TypeEquipement.Arme, Rarete.Legendaire, 48, 12, 40, 0, 18, 0, 6, 1100)
            });

            Recettes.Add(new RecetteCraft {
                Id = "w_marteau_titan", Nom = "Fracas Tellurique de Valdorak",
                Description = "+56 Attaque, +20 Défense, +80 PV. Arme colossale brisant les montagnes.",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Legendaire, NiveauRequis = 12, CoutOr = 2000,
                MateriauxRequis = new Dictionary<string, int> { { "Cœur de Titan", 5 }, { "Acier Trempé", 10 }, { "Éclat Astral", 4 } },
                GenererEquipement = () => new Equipement("Fracas Tellurique de Valdorak", TypeEquipement.Arme, Rarete.Legendaire, 56, 20, 80, 0, 10, 0, 0, 1500)
            });

            Recettes.Add(new RecetteCraft {
                Id = "w_espadon_neant", Nom = "Lame Tranche-Matière du Néant",
                Description = "+68 Attaque, +18 Défense, +70 PV, +60 Mana, +25% Critique, +10% Vampirisme.",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Mythique, NiveauRequis = 15, CoutOr = 4500,
                MateriauxRequis = new Dictionary<string, int> { { "Matière du Néant", 6 }, { "Cristal Cosmique", 4 }, { "Éclat Astral", 12 } },
                GenererEquipement = () => new Equipement("Lame Tranche-Matière du Néant", TypeEquipement.Arme, Rarete.Mythique, 68, 18, 70, 60, 25, 8, 10, 3200)
            });

            Recettes.Add(new RecetteCraft {
                Id = "w_deus_nihilo", Nom = "Lame Divine : Deus Ex Nihilo",
                Description = "+85 Attaque, +25 Défense, +120 PV, +100 Mana, +35% Critique, +15% Esquive, +15% Vampirisme. L'arme des Dieux !",
                Categorie = CategorieCraft.Arme, RareteItem = Rarete.Mythique, NiveauRequis = 20, CoutOr = 10000,
                MateriauxRequis = new Dictionary<string, int> { { "Essence du Chaos", 4 }, { "Matière du Néant", 10 }, { "Cristal Cosmique", 8 }, { "Éclat Astral", 20 } },
                GenererEquipement = () => new Equipement("Lame Divine : Deus Ex Nihilo", TypeEquipement.Arme, Rarete.Mythique, 85, 25, 120, 100, 35, 15, 15, 8000)
            });

            // === 2. ARMURES & CASQUES ===
            Recettes.Add(new RecetteCraft {
                Id = "a_cuirasse_fer", Nom = "Cuirasse Renforcée en Fer",
                Description = "+10 Défense, +30 PV Max. Bonne armure de base.",
                Categorie = CategorieCraft.Armure, RareteItem = Rarete.Commun, NiveauRequis = 1, CoutOr = 75,
                MateriauxRequis = new Dictionary<string, int> { { "Minerai de Fer", 4 }, { "Cuir Épais", 3 } },
                GenererEquipement = () => new Equipement("Cuirasse Renforcée en Fer", TypeEquipement.Armure, Rarete.Commun, 0, 10, 30, 0, 0, 0, 0, 60)
            });

            Recettes.Add(new RecetteCraft {
                Id = "a_plastron_ecailles", Nom = "Plastron Épineux en Écailles",
                Description = "+18 Défense, +50 PV Max, +5% Esquive.",
                Categorie = CategorieCraft.Armure, RareteItem = Rarete.Rare, NiveauRequis = 3, CoutOr = 220,
                MateriauxRequis = new Dictionary<string, int> { { "Cuir Épais", 5 }, { "Os Renforcé", 4 }, { "Minerai de Fer", 4 } },
                GenererEquipement = () => new Equipement("Plastron Épineux en Écailles", TypeEquipement.Armure, Rarete.Rare, 0, 18, 50, 0, 0, 5, 0, 160)
            });

            Recettes.Add(new RecetteCraft {
                Id = "a_heaume_givre", Nom = "Heaume Boréal de Cristal",
                Description = "+16 Défense, +45 PV Max, +40 Mana.",
                Categorie = CategorieCraft.Bijou, RareteItem = Rarete.Epique, NiveauRequis = 5, CoutOr = 380,
                MateriauxRequis = new Dictionary<string, int> { { "Noyau de Givre", 4 }, { "Acier Trempé", 4 }, { "Ectoplasme", 3 } },
                GenererEquipement = () => new Equipement("Heaume Boréal de Cristal", TypeEquipement.Casque, Rarete.Epique, 0, 16, 45, 40, 0, 0, 0, 280)
            });

            Recettes.Add(new RecetteCraft {
                Id = "a_armure_dragon", Nom = "Carapace Draconique Suprême",
                Description = "+32 Défense, +110 PV Max, +6% Critique. Écailles d'Ignis impénétrables.",
                Categorie = CategorieCraft.Armure, RareteItem = Rarete.Legendaire, NiveauRequis = 8, CoutOr = 1100,
                MateriauxRequis = new Dictionary<string, int> { { "Écaille Draconique", 8 }, { "Cœur Ardent", 4 }, { "Acier Trempé", 8 } },
                GenererEquipement = () => new Equipement("Carapace Draconique Suprême", TypeEquipement.Armure, Rarete.Legendaire, 0, 32, 110, 0, 6, 0, 0, 850)
            });

            Recettes.Add(new RecetteCraft {
                Id = "a_cuirasse_titan", Nom = "Plastron Inébranlable du Titan",
                Description = "+45 Défense, +160 PV Max, +10 Attaque. Taillé dans le roc de Valdorak.",
                Categorie = CategorieCraft.Armure, RareteItem = Rarete.Legendaire, NiveauRequis = 12, CoutOr = 2200,
                MateriauxRequis = new Dictionary<string, int> { { "Cœur de Titan", 6 }, { "Acier Trempé", 12 }, { "Éclat Astral", 5 } },
                GenererEquipement = () => new Equipement("Plastron Inébranlable du Titan", TypeEquipement.Armure, Rarete.Legendaire, 10, 45, 160, 0, 0, 0, 0, 1600)
            });

            Recettes.Add(new RecetteCraft {
                Id = "a_armure_chaos", Nom = "Armure Divine de l'Architecte Cosmique",
                Description = "+65 Défense, +260 PV Max, +120 Mana, +15% Esquive, +15% Critique. Invulnérabilité divine.",
                Categorie = CategorieCraft.Armure, RareteItem = Rarete.Mythique, NiveauRequis = 20, CoutOr = 9000,
                MateriauxRequis = new Dictionary<string, int> { { "Essence du Chaos", 4 }, { "Matière du Néant", 8 }, { "Cristal Cosmique", 6 }, { "Éclat Astral", 18 } },
                GenererEquipement = () => new Equipement("Armure Divine de l'Architecte Cosmique", TypeEquipement.Armure, Rarete.Mythique, 15, 65, 260, 120, 15, 15, 0, 7500)
            });

            // === 3. BIJOUX & AMULETTES ===
            Recettes.Add(new RecetteCraft {
                Id = "j_anneau_vie", Nom = "Anneau de Rubis Régénérant",
                Description = "+40 PV Max, +5 Défense, +4% Vampirisme.",
                Categorie = CategorieCraft.Bijou, RareteItem = Rarete.Rare, NiveauRequis = 3, CoutOr = 180,
                MateriauxRequis = new Dictionary<string, int> { { "Minerai de Fer", 5 }, { "Pierre d'Âme", 2 } },
                GenererEquipement = () => new Equipement("Anneau de Rubis Régénérant", TypeEquipement.Anneau, Rarete.Rare, 0, 5, 40, 0, 0, 0, 4, 150)
            });

            Recettes.Add(new RecetteCraft {
                Id = "j_amulette_astrale", Nom = "Amulette d'Éther Cosmique",
                Description = "+10 Attaque, +10 Défense, +60 PV, +60 Mana, +8% Critique.",
                Categorie = CategorieCraft.Bijou, RareteItem = Rarete.Legendaire, NiveauRequis = 11, CoutOr = 1600,
                MateriauxRequis = new Dictionary<string, int> { { "Éclat Astral", 8 }, { "Cristal Cosmique", 2 }, { "Pierre d'Âme", 4 } },
                GenererEquipement = () => new Equipement("Amulette d'Éther Cosmique", TypeEquipement.Amulette, Rarete.Legendaire, 10, 10, 60, 60, 8, 5, 0, 1200)
            });

            Recettes.Add(new RecetteCraft {
                Id = "j_anneau_vide", Nom = "Sceau de l'Éclipse Absolue",
                Description = "+20 Attaque, +18 Défense, +90 PV, +80 Mana, +12% Critique, +8% Vampirisme.",
                Categorie = CategorieCraft.Bijou, RareteItem = Rarete.Mythique, NiveauRequis = 16, CoutOr = 4000,
                MateriauxRequis = new Dictionary<string, int> { { "Matière du Néant", 4 }, { "Cristal Cosmique", 4 }, { "Éclat Astral", 10 } },
                GenererEquipement = () => new Equipement("Sceau de l'Éclipse Absolue", TypeEquipement.Anneau, Rarete.Mythique, 20, 18, 90, 80, 12, 6, 8, 3000)
            });

            // === 4. COMPÉTENCES & SORTS FORGEABLES (NOUVEAU SYSTÈME !) ===
            Recettes.Add(new RecetteCraft {
                Id = "c_onde_sismique", Nom = "Tome : Onde Sismique",
                Description = "Sort : 25 Mana | 200% Dégâts | Brise 50% de l'armure ennemie !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Rare, NiveauRequis = 3, CoutOr = 250,
                MateriauxRequis = new Dictionary<string, int> { { "Os Renforcé", 4 }, { "Minerai de Fer", 5 } },
                CompetenceResultat = new CompetenceCraftable("c_onde_sismique", "Onde Sismique", "Fracas tellurique brisant 50% de l'armure de l'ennemi.", 25, 2.0, TypeEffetCompetence.PerforantArmure, 50, "🌋")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_foudre_chaine", Nom = "Tome : Foudre Enchaînée",
                Description = "Sort : 30 Mana | 220% Dégâts | 100% de chance d'Étourdir l'ennemi (Stun) !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Epique, NiveauRequis = 5, CoutOr = 450,
                MateriauxRequis = new Dictionary<string, int> { { "Noyau de Givre", 3 }, { "Ectoplasme", 4 }, { "Acier Trempé", 3 } },
                CompetenceResultat = new CompetenceCraftable("c_foudre_chaine", "Foudre Enchaînée", "Archi-éclair paralysant instantanément l'adversaire.", 30, 2.2, TypeEffetCompetence.Etourdissement, 1, "⚡")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_siphon_sang", Nom = "Tome : Festin de Sang",
                Description = "Sort : 25 Mana | 190% Dégâts | Vole 60 PV à la cible pour vous soigner !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Epique, NiveauRequis = 7, CoutOr = 600,
                MateriauxRequis = new Dictionary<string, int> { { "Sang de Démon", 4 }, { "Ectoplasme", 5 } },
                CompetenceResultat = new CompetenceCraftable("c_siphon_sang", "Festin de Sang", "Siphon démoniaque drainant la vitalité ennemie.", 25, 1.9, TypeEffetCompetence.SoinEtDegats, 60, "🩸")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_souffle_dragon", Nom = "Tome : Souffle d'Ignis",
                Description = "Sort : 35 Mana | 260% Dégâts | Inflige Brûlure infernale (3 tours) !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Legendaire, NiveauRequis = 9, CoutOr = 1000,
                MateriauxRequis = new Dictionary<string, int> { { "Cœur Ardent", 4 }, { "Écaille Draconique", 6 } },
                CompetenceResultat = new CompetenceCraftable("c_souffle_dragon", "Souffle d'Ignis", "Cataclysme de flammes calcinant l'ennemi sur la durée.", 35, 2.6, TypeEffetCompetence.Brulure, 3, "🔥")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_venin_corrosif", Nom = "Tome : Peste de la Vipère",
                Description = "Sort : 25 Mana | 180% Dégâts | Applique un Poison mortel de 30 dégâts/tour !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Legendaire, NiveauRequis = 11, CoutOr = 1200,
                MateriauxRequis = new Dictionary<string, int> { { "Venin Obscur", 5 }, { "Cuir Épais", 6 } },
                CompetenceResultat = new CompetenceCraftable("c_venin_corrosif", "Peste de la Vipère", "Toxines abyssales rongeant les organes de la cible.", 25, 1.8, TypeEffetCompetence.Poison, 30, "🧪")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_rayon_astral", Nom = "Tome : Rayon Cosmique Suprême",
                Description = "Sort : 45 Mana | 330% Dégâts | Perce 40% de la défense adverse !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Legendaire, NiveauRequis = 13, CoutOr = 2000,
                MateriauxRequis = new Dictionary<string, int> { { "Éclat Astral", 10 }, { "Cristal Cosmique", 3 } },
                CompetenceResultat = new CompetenceCraftable("c_rayon_astral", "Rayon Cosmique Suprême", "Faisceau d'étoile pure traversant l'armure.", 45, 3.3, TypeEffetCompetence.PerforantArmure, 40, "🌟")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_trou_noir", Nom = "Tome : Annihilation du Néant",
                Description = "Sort : 55 Mana | 420% Dégâts Colossaux | Perce 100% de la défense !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Mythique, NiveauRequis = 16, CoutOr = 4500,
                MateriauxRequis = new Dictionary<string, int> { { "Matière du Néant", 6 }, { "Cristal Cosmique", 5 }, { "Éclat Astral", 12 } },
                CompetenceResultat = new CompetenceCraftable("c_trou_noir", "Annihilation du Néant", "Onde de matière noire absolue pulvérisant toute résistance.", 55, 4.2, TypeEffetCompetence.PerforantArmure, 100, "🌑")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_deus_chaos", Nom = "Tome Divin : Genèse du Chaos",
                Description = "Sort Suprême : 60 Mana | 550% Dégâts Démesurés | Régénère 120 PV et recharge 40% de l'Ultime !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Mythique, NiveauRequis = 20, CoutOr = 10000,
                MateriauxRequis = new Dictionary<string, int> { { "Essence du Chaos", 4 }, { "Matière du Néant", 10 }, { "Cristal Cosmique", 8 }, { "Éclat Astral", 20 } },
                CompetenceResultat = new CompetenceCraftable("c_deus_chaos", "Genèse du Chaos", "Le pouvoir créateur et destructeur de Deus Ex Nihilo déchaîné !", 60, 5.5, TypeEffetCompetence.SoinEtDegats, 120, "🌌")
            });

            // === 5. ALCHIMIE & CONSOMMABLES ===
            Recettes.Add(new RecetteCraft {
                Id = "alch_soin_maj", Nom = "Lot de 3 Potions de Soin Majeures",
                Description = "Restaure 150 PV chacune. Concocté à base d'herbes rares.",
                Categorie = CategorieCraft.Consommable, RareteItem = Rarete.Rare, NiveauRequis = 2, CoutOr = 90,
                MateriauxRequis = new Dictionary<string, int> { { "Herbes Magiques", 4 }, { "Ectoplasme", 2 } },
                ConsommableResultat = TypeConsommable.PotionSoinMajeure, QuantiteConsommable = 3
            });

            Recettes.Add(new RecetteCraft {
                Id = "alch_mana_maj", Nom = "Lot de 3 Potions de Mana Majeures",
                Description = "Restaure 120 Mana chacune. Essentiel pour les mages.",
                Categorie = CategorieCraft.Consommable, RareteItem = Rarete.Rare, NiveauRequis = 2, CoutOr = 90,
                MateriauxRequis = new Dictionary<string, int> { { "Herbes Magiques", 4 }, { "Noyau de Givre", 2 } },
                ConsommableResultat = TypeConsommable.PotionManaMajeure, QuantiteConsommable = 3
            });

            Recettes.Add(new RecetteCraft {
                Id = "alch_bombe", Nom = "Lot de 2 Bombes Incendiaires Suprêmes",
                Description = "Inflige 150 dégâts de feu purs perçant toute défense en combat.",
                Categorie = CategorieCraft.Consommable, RareteItem = Rarete.Epique, NiveauRequis = 4, CoutOr = 150,
                MateriauxRequis = new Dictionary<string, int> { { "Cœur Ardent", 2 }, { "Minerai de Fer", 4 } },
                ConsommableResultat = TypeConsommable.BombeIncendiaire, QuantiteConsommable = 2
            });
        }
    }

    // ==============================================================
    // ENCYCLOPÉDIE & GUIDE DES MATÉRIAUX (SOURCES DE BUTIN & CRAFT)
    // ==============================================================
    public class SourceObtentionMateriau
    {
        public string TypeSource { get; set; } = ""; // 👑 Boss, ⚔️ Monstres, 🏰 Donjon/Tour, ♻️ Recyclage, 🌿 Récolte, 📜 Quête
        public string NomSource { get; set; } = "";
        public string ZoneOuLieu { get; set; } = "";
        public string DetailsTaux { get; set; } = "";
    }

    public class FicheMateriau
    {
        public string Nom { get; set; } = "";
        public string Icone { get; set; } = "📦";
        public Rarete RareteItem { get; set; } = Rarete.Commun;
        public string Categorie { get; set; } = "Composant";
        public string Description { get; set; } = "";
        public List<SourceObtentionMateriau> Sources { get; set; } = new List<SourceObtentionMateriau>();

        public List<string> ObtenirRecettesAssociees()
        {
            var list = new List<string>();
            foreach (var r in CatalogueCraft.Recettes)
            {
                if (r.MateriauxRequis.TryGetValue(Nom, out int requis))
                {
                    string cat = r.Categorie switch
                    {
                        CategorieCraft.Arme => "⚔️ Arme",
                        CategorieCraft.Armure => "🛡️ Armure",
                        CategorieCraft.Bijou => "💍 Bijou",
                        CategorieCraft.Competence => "🔮 Sort",
                        CategorieCraft.Consommable => "🧪 Potion",
                        _ => "📦 Objet"
                    };
                    list.Add($"{cat} : {r.Nom} (x{requis} requis)");
                }
            }
            if (Nom == "Pierres de Forge")
            {
                list.Add("🔨 Amélioration d'Équipement (+1 à +10 chez Brom le Forgeron)");
            }
            return list;
        }
    }

    public static class GuideDesMateriaux
    {
        public static List<FicheMateriau> Fiches { get; } = new List<FicheMateriau>();

        static GuideDesMateriaux()
        {
            InitialiserGuide();
        }

        private static void InitialiserGuide()
        {
            // 1. Minerai de Fer
            Fiches.Add(new FicheMateriau {
                Nom = "Minerai de Fer", Icone = "⛏️", RareteItem = Rarete.Commun, Categorie = "Minerais & Métaux",
                Description = "Minerai brut extrait des filons des montagnes ou pillé sur les troupes gobelines. Indispensable pour forger les armes et armures de base chez le forgeron.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Gobelins, Pillards des routes", ZoneOuLieu = "Bois des Ombres & Plaines", DetailsTaux = "Taux 60% (1 à 3 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Squelettes & Gardes Morts-Vivants", ZoneOuLieu = "Cryptes & Donjon Oublié", DetailsTaux = "Taux 40% (1 à 2 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kragh l'Écorcheur (Boss Niv. 3)", ZoneOuLieu = "Antre de la Horde Noire", DetailsTaux = "GARANTI (x6 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Gorrok le Broyeur (Boss Niv. 2)", ZoneOuLieu = "Terres Sauvages", DetailsTaux = "GARANTI (x3 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Grok Seigneur Gobelin (Boss Niv. 2)", ZoneOuLieu = "Camp Gobelin", DetailsTaux = "GARANTI (x5 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Balthazar Chevalier Déchu (Boss Niv. 6)", ZoneOuLieu = "Donjon Déchu", DetailsTaux = "GARANTI (x7 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "♻️ Recyclage", NomSource = "Brom le Forgeron", ZoneOuLieu = "Forge du Village", DetailsTaux = "Recycler des armes et armures en fer" }
                }
            });

            // 2. Morceau de Cuir
            Fiches.Add(new FicheMateriau {
                Nom = "Morceau de Cuir", Icone = "🐗", RareteItem = Rarete.Commun, Categorie = "Peaux & Cuirs",
                Description = "Lanières de cuir souple prélevées sur le gibier ou confectionnées par les tanneurs. Sert de doublure aux armures et de poignées pour les armes.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Gobelins éclaireurs & Petites bêtes", ZoneOuLieu = "Forêt de Val-Serein", DetailsTaux = "Taux 60% (x1 morceau)" },
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Sangliers & Loups des plaines", ZoneOuLieu = "Terres Sauvages", DetailsTaux = "Taux 50% (x1 morceau)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Grok le Seigneur Gobelin (Boss Niv. 2)", ZoneOuLieu = "Camp Gobelin", DetailsTaux = "GARANTI (x4 morceaux)" },
                    new SourceObtentionMateriau { TypeSource = "♻️ Recyclage", NomSource = "Brom le Forgeron", ZoneOuLieu = "Forge du Village", DetailsTaux = "Recycler des armures légères" }
                }
            });

            // 3. Cuir Épais
            Fiches.Add(new FicheMateriau {
                Nom = "Cuir Épais", Icone = "🐺", RareteItem = Rarete.Rare, Categorie = "Peaux & Cuirs",
                Description = "Peau robuste et dense issue des bêtes féroces et des prédateurs dominants. Offre une formidable résistance aux coupures et aux morsures.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Loups sauvages, Sangliers enragés", ZoneOuLieu = "Forêt Maudite & Cavernes", DetailsTaux = "Taux 80% (1 à 3 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Gorrok le Broyeur (Boss Niv. 2)", ZoneOuLieu = "Terres Sauvages", DetailsTaux = "GARANTI (x4 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kragh l'Écorcheur (Boss Niv. 3)", ZoneOuLieu = "Antre de la Horde", DetailsTaux = "GARANTI (x3 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Vespera la Matriarche (Boss Niv. 5)", ZoneOuLieu = "Falaises Embrumées", DetailsTaux = "GARANTI (x4 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kryll l'Arachnide (Boss Niv. 7)", ZoneOuLieu = "Gouffre de Nécrose", DetailsTaux = "GARANTI (x5 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Fenrir de la Lune Rouge (Boss Niv. 12)", ZoneOuLieu = "Arène de Lune de Sang", DetailsTaux = "GARANTI (x8 cuirs)" }
                }
            });

            // 4. Croc Sauvage
            Fiches.Add(new FicheMateriau {
                Nom = "Croc Sauvage", Icone = "🦷", RareteItem = Rarete.Rare, Categorie = "Trophées & Faune",
                Description = "Croc effilé prélevé sur les bêtes sanguinaires. Utilisé pour forger des lames tranchantes et des dagues infligeant des saignements.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Loups alpha & Bêtes féroces", ZoneOuLieu = "Terres Sauvages & Bois", DetailsTaux = "Taux 50% (x1 croc)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Gorrok le Broyeur (Boss Niv. 2)", ZoneOuLieu = "Terres Sauvages", DetailsTaux = "GARANTI (x3 crocs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Vespera la Matriarche (Boss Niv. 5)", ZoneOuLieu = "Falaises Embrumées", DetailsTaux = "GARANTI (x3 crocs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Fenrir de la Lune Rouge (Boss Niv. 12)", ZoneOuLieu = "Sanctuaire Sanguin", DetailsTaux = "GARANTI (x6 crocs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Grok le Seigneur Gobelin (Boss Niv. 2)", ZoneOuLieu = "Camp Gobelin", DetailsTaux = "GARANTI (x2 crocs)" }
                }
            });

            // 5. Os Renforcé
            Fiches.Add(new FicheMateriau {
                Nom = "Os Renforcé", Icone = "💀", RareteItem = Rarete.Rare, Categorie = "Ossements & Reliques",
                Description = "Ossement imprégné de magie sépulcrale dense. Utilisé pour forger des masses contondantes, plastrons d'écailles et tomes telluriques.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Squelettes & Guerriers d'ossuaire", ZoneOuLieu = "Cryptes Sépulcrales & Donjon", DetailsTaux = "Taux 85% (1 à 3 os)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Skuldir le Roi Squelette (Boss Niv. 4)", ZoneOuLieu = "Donjon de Glace Antique", DetailsTaux = "GARANTI (x5 os)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zulgar le Chaman Putride (Boss Niv. 5)", ZoneOuLieu = "Marais Corrompus", DetailsTaux = "GARANTI (x4 os)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Malakor le Nécromancien (Boss Niv. 4)", ZoneOuLieu = "Donjon Oublié", DetailsTaux = "GARANTI (x5 os)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Mor'Gath la Liche (Boss Niv. 7)", ZoneOuLieu = "Catacombes Impies", DetailsTaux = "GARANTI (x5 os)" }
                }
            });

            // 6. Ectoplasme
            Fiches.Add(new FicheMateriau {
                Nom = "Ectoplasme", Icone = "👻", RareteItem = Rarete.Epique, Categorie = "Essences Spectrales",
                Description = "Matière vaporeuse phosphorescente laissée par les apparitions spectrales. Catalyseur majeur pour la magie des runes et les potions supérieures.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Spectres errants & Ombres nocturnes", ZoneOuLieu = "Catacombes & Nécropoles", DetailsTaux = "Taux 70% (1 à 3 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Malakor le Nécromancien (Boss Niv. 4)", ZoneOuLieu = "Donjon Oublié", DetailsTaux = "GARANTI (x4 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Skuldir le Roi Squelette (Boss Niv. 4)", ZoneOuLieu = "Donjon de Glace Antique", DetailsTaux = "GARANTI (x3 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zulgar le Chaman Putride (Boss Niv. 5)", ZoneOuLieu = "Marais Corrompus", DetailsTaux = "GARANTI (x4 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kryll l'Arachnide (Boss Niv. 7)", ZoneOuLieu = "Gouffre de Nécrose", DetailsTaux = "GARANTI (x3 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Sylvana Reine des Ronces (Boss Niv. 8)", ZoneOuLieu = "Forêt Épineuse", DetailsTaux = "GARANTI (x5 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Mor'Gath la Liche (Boss Niv. 7)", ZoneOuLieu = "Catacombes Impies", DetailsTaux = "GARANTI (x6 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Nox le Dévoreur de Lumière (Boss Niv. 9)", ZoneOuLieu = "Failles Obscures", DetailsTaux = "GARANTI (x7 ectoplasmes)" }
                }
            });

            // 7. Pierre d'Âme
            Fiches.Add(new FicheMateriau {
                Nom = "Pierre d'Âme", Icone = "🔮", RareteItem = Rarete.Epique, Categorie = "Joyaux Magiques",
                Description = "Joyau ésotérique scintillant abritant l'écho d'une âme transcendée. Indispensable pour sertir les anneaux régénérants et armes arcaniques.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Ombres des profondeurs & Spectres majeurs", ZoneOuLieu = "Sanctuaires Nocturnes", DetailsTaux = "Taux 40% (x1 pierre)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Malakor le Nécromancien (Boss Niv. 4)", ZoneOuLieu = "Donjon Oublié", DetailsTaux = "GARANTI (x3 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kaelas Reine du Blizzard (Boss Niv. 6)", ZoneOuLieu = "Pics Boréaux", DetailsTaux = "GARANTI (x3 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Balthazar Chevalier Déchu (Boss Niv. 6)", ZoneOuLieu = "Donjon Déchu", DetailsTaux = "GARANTI (x2 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Mor'Gath la Liche (Boss Niv. 7)", ZoneOuLieu = "Catacombes Impies", DetailsTaux = "GARANTI (x4 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Sylvana Reine des Ronces (Boss Niv. 8)", ZoneOuLieu = "Forêt Épineuse", DetailsTaux = "GARANTI (x3 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Nox le Dévoreur de Lumière (Boss Niv. 9)", ZoneOuLieu = "Failles Obscures", DetailsTaux = "GARANTI (x4 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Thalor le Sans-Tête (Boss Niv. 10)", ZoneOuLieu = "Champs de Bataille Maudits", DetailsTaux = "GARANTI (x3 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chronos Maître du Temps (Boss Niv. 15)", ZoneOuLieu = "Sabliers Brisés", DetailsTaux = "GARANTI (x5 pierres)" }
                }
            });

            // 8. Acier Trempé
            Fiches.Add(new FicheMateriau {
                Nom = "Acier Trempé", Icone = "🛡️", RareteItem = Rarete.Epique, Categorie = "Minerais & Métaux",
                Description = "Alliage noble forgé dans les creusets élémentaires. Sa dureté extrême permet de façonner des lames de guerre et armures de plates de haut rang.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Golems de pierre & Gardiens de fer", ZoneOuLieu = "Cités Enfouies & Donjons", DetailsTaux = "Taux 75% (1 à 2 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Balthazar Chevalier Déchu (Boss Niv. 6)", ZoneOuLieu = "Donjon Déchu", DetailsTaux = "GARANTI (x5 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Magmarion Colosse de Braise (Boss Niv. 7)", ZoneOuLieu = "Cratère Volcanique", DetailsTaux = "GARANTI (x5 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Obsidius Titan de Verre (Boss Niv. 8)", ZoneOuLieu = "Mont d'Obsidienne", DetailsTaux = "GARANTI (x6 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Valdorak Colosse de Granite (Boss Niv. 8)", ZoneOuLieu = "Abîmes Telluriques", DetailsTaux = "GARANTI (x8 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Dragon Ignis (Boss Niv. 8)", ZoneOuLieu = "Volcan d'Aethelgard", DetailsTaux = "GARANTI (x5 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Thalor le Sans-Tête (Boss Niv. 10)", ZoneOuLieu = "Légion Impériale", DetailsTaux = "GARANTI (x7 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zephyros Seigneur des Ouragans (Boss Niv. 11)", ZoneOuLieu = "Sommets Éthérés", DetailsTaux = "GARANTI (x6 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "♻️ Recyclage", NomSource = "Brom le Forgeron", ZoneOuLieu = "Forge du Village", DetailsTaux = "Démantèlement d'équipements Épiques & Légendaires" }
                }
            });

            // 9. Noyau de Givre
            Fiches.Add(new FicheMateriau {
                Nom = "Noyau de Givre", Icone = "❄️", RareteItem = Rarete.Epique, Categorie = "Élémentaire Boréal",
                Description = "Cristal de froid absolu insensible à la chaleur ambiante. Nécessaire pour forger des marteaux givrants, heaumes boréaux et tomes de foudre paralysante.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Golems polaires & Élémentaires de givre", ZoneOuLieu = "Cavernes Boréales", DetailsTaux = "Taux 80% (x1 noyau)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kaelas Archifée du Blizzard (Boss Niv. 6)", ZoneOuLieu = "Pics Boréaux", DetailsTaux = "GARANTI (x4 noyaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Skuldir le Cryomancien (Boss Niv. 4)", ZoneOuLieu = "Donjon de Glace Antique", DetailsTaux = "GARANTI (x3 noyaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zephyros Seigneur des Ouragans (Boss Niv. 11)", ZoneOuLieu = "Pic Tempétueux", DetailsTaux = "GARANTI (x4 noyaux)" }
                }
            });

            // 10. Plume Aérienne
            Fiches.Add(new FicheMateriau {
                Nom = "Plume Aérienne", Icone = "🦅", RareteItem = Rarete.Epique, Categorie = "Bêtes Célestes",
                Description = "Plume aux barbes tranchantes comme l'acier prélevée sur les souveraines des cieux. Apporte une vélocité sans égale.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Harpies des falaises & Rocs tempestaires", ZoneOuLieu = "Falaises Embrumées", DetailsTaux = "Taux 60% (1 à 2 plumes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Vespera Matriarche des Harpies (Boss Niv. 5)", ZoneOuLieu = "Falaises Embrumées", DetailsTaux = "GARANTI (x5 plumes)" }
                }
            });

            // 11. Venin Obscur
            Fiches.Add(new FicheMateriau {
                Nom = "Venin Obscur", Icone = "🧪", RareteItem = Rarete.Legendaire, Categorie = "Alchimie & Toxines",
                Description = "Toxine abyssale corrosive dissolvant le métal et brûlant les chairs en quelques fractions de seconde. Requis pour le tome Peste de la Vipère.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Arachnides toxiques des profondeurs", ZoneOuLieu = "Cavernes Abyssales", DetailsTaux = "Taux 70% (1 à 2 venins)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kryll l'Arachnide de Nécrose (Boss Niv. 7)", ZoneOuLieu = "Gouffre Souterrain", DetailsTaux = "GARANTI (x4 venins)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Azkalith Reine Vipère des Abysses (Boss Niv. 9)", ZoneOuLieu = "Abîmes Oubliées", DetailsTaux = "GARANTI (x5 venins)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kragh l'Écorcheur (Boss Niv. 3)", ZoneOuLieu = "Antre de la Horde", DetailsTaux = "GARANTI (x2 venins)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kraken des Abîmes Primordiaux (Boss Niv. 14)", ZoneOuLieu = "Fosses Océaniques", DetailsTaux = "GARANTI (x6 venins)" }
                }
            });

            // 12. Écaille Draconique
            Fiches.Add(new FicheMateriau {
                Nom = "Écaille Draconique", Icone = "🐉", RareteItem = Rarete.Legendaire, Categorie = "Draconique & Igné",
                Description = "Écaille quasi-indestructible issue des drakes et dragons millénaires. Nécessaire pour forger la Lame du Pourfendeur et la Carapace Suprême.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Drakes de feu & Vouivres des pics", ZoneOuLieu = "Pics Volcaniques", DetailsTaux = "Taux 75% (1 à 3 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Ignis Dragon Millénaire Suprême (Boss Niv. 8)", ZoneOuLieu = "Cœur du Volcan", DetailsTaux = "GARANTI (x6 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Magmarion Colosse de Braise (Boss Niv. 7)", ZoneOuLieu = "Cratère Ardent", DetailsTaux = "GARANTI (x3 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Belial Seigneur Démoniaque (Boss Niv. 6)", ZoneOuLieu = "Abîme Infernal", DetailsTaux = "GARANTI (x4 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Azkalith la Reine Vipère (Boss Niv. 9)", ZoneOuLieu = "Fosses Abyssales", DetailsTaux = "GARANTI (x3 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kraken des Abîmes (Boss Niv. 14)", ZoneOuLieu = "Abîmes Primordiaux", DetailsTaux = "GARANTI (x6 écailles)" }
                }
            });

            // 13. Cœur Ardent
            Fiches.Add(new FicheMateriau {
                Nom = "Cœur Ardent", Icone = "🔥", RareteItem = Rarete.Legendaire, Categorie = "Élémentaire de Feu",
                Description = "Noyau de magma solidifié rayonnant d'une chaleur volcanique infinie. Alimente les forges mythiques et le sortilège Souffle d'Ignis.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Élémentaires de magma & Drakes anciens", ZoneOuLieu = "Cheminée du Volcan", DetailsTaux = "Taux 70% (x1 cœur)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Ignis Dragon Millénaire Suprême (Boss Niv. 8)", ZoneOuLieu = "Cœur du Volcan", DetailsTaux = "GARANTI (x4 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Magmarion Colosse de Braise (Boss Niv. 7)", ZoneOuLieu = "Cratère Ardent", DetailsTaux = "GARANTI (x4 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Belial Seigneur Démoniaque (Boss Niv. 6)", ZoneOuLieu = "Abîme Infernal", DetailsTaux = "GARANTI (x4 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Archonte Solaire Déchu (Boss Niv. 13)", ZoneOuLieu = "Sanctuaire Solaire", DetailsTaux = "GARANTI (x4 cœurs)" }
                }
            });

            // 14. Sang de Démon
            Fiches.Add(new FicheMateriau {
                Nom = "Sang de Démon", Icone = "🩸", RareteItem = Rarete.Legendaire, Categorie = "Occulte & Abyssal",
                Description = "Sang impie bouillonnant recueilli sur les seigneurs démoniaques. Ouvre la voie au Trident du Purgatoire et au tome Festin de Sang.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Démons majeurs des failles", ZoneOuLieu = "Abîmes Souterrains", DetailsTaux = "Taux 60% (1 à 2 sangs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Belial Seigneur Démoniaque (Boss Niv. 6)", ZoneOuLieu = "Abîme Infernal", DetailsTaux = "GARANTI (x6 sangs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Fenrir de la Lune Rouge (Boss Niv. 12)", ZoneOuLieu = "Sanctuaire Écarlate", DetailsTaux = "GARANTI (x6 sangs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Thalor le Général Sans-Tête (Boss Niv. 10)", ZoneOuLieu = "Légion Impériale", DetailsTaux = "GARANTI (x3 sangs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Abaddon Pourfendeur de Mondes (Boss Niv. 18)", ZoneOuLieu = "Faille Apocalyptique", DetailsTaux = "GARANTI (x7 sangs)" }
                }
            });

            // 15. Cœur de Titan
            Fiches.Add(new FicheMateriau {
                Nom = "Cœur de Titan", Icone = "🗿", RareteItem = Rarete.Legendaire, Categorie = "Tellurique Ancien",
                Description = "Fragment de pierre millénaire doté d'une pulsation tellurique continue. Composant primordial du Fracas de Valdorak et du Plastron Inébranlable.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Colosses telluriques & Golems millénaires", ZoneOuLieu = "Gouffres Souterrains", DetailsTaux = "Taux 50% (x1 cœur)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Valdorak Colosse de Granite (Boss Niv. 8)", ZoneOuLieu = "Gouffres Telluriques", DetailsTaux = "GARANTI (x4 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Obsidius Titan de Verre Noir (Boss Niv. 8)", ZoneOuLieu = "Mont d'Obsidienne", DetailsTaux = "GARANTI (x2 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kraken des Abîmes (Boss Niv. 14)", ZoneOuLieu = "Fosses Océaniques", DetailsTaux = "GARANTI (x2 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Léviathan des Galaxies (Boss Niv. 16)", ZoneOuLieu = "Éther Stellaire", DetailsTaux = "GARANTI (x4 cœurs)" }
                }
            });

            // 16. Éclat Astral
            Fiches.Add(new FicheMateriau {
                Nom = "Éclat Astral", Icone = "🌌", RareteItem = Rarete.Legendaire, Categorie = "Stellaire & Cosmique",
                Description = "Lumière condensée issue du firmament céleste. Composant central de tous les équipements et sorts mythiques de haute volée.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "🏰 Tour Astrale", NomSource = "Passage de chaque Étage", ZoneOuLieu = "Tour Astrale Infinie", DetailsTaux = "GARANTI (Récompense automatique de montée)" },
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Créatures stellaires & cosmiques", ZoneOuLieu = "Tour Astrale Infinie", DetailsTaux = "Taux 80% (1 à 3 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Xanthos Dévoreur Stellaire (Boss Niv. 10)", ZoneOuLieu = "Failles Dimensionnelles", DetailsTaux = "GARANTI (x8 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chevalier du Néant Primordial (Boss Niv. 10)", ZoneOuLieu = "Abîme Originel", DetailsTaux = "GARANTI (x8 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zephyros Seigneur des Ouragans (Boss Niv. 11)", ZoneOuLieu = "Sommet des Ouragans", DetailsTaux = "GARANTI (x6 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Archonte Solaire Déchu (Boss Niv. 13)", ZoneOuLieu = "Flamme Blanche", DetailsTaux = "GARANTI (x8 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chronos Maître du Temps (Boss Niv. 15)", ZoneOuLieu = "Sabliers Brisés", DetailsTaux = "GARANTI (x9 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Léviathan des Galaxies (Boss Niv. 16)", ZoneOuLieu = "Voie Lactée Perdue", DetailsTaux = "GARANTI (x12 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Deus Ex Nihilo (Boss Niv. 20)", ZoneOuLieu = "Trône Cosmique", DetailsTaux = "GARANTI (x16 éclats)" }
                }
            });

            // 17. Cristal Cosmique
            Fiches.Add(new FicheMateriau {
                Nom = "Cristal Cosmique", Icone = "🌠", RareteItem = Rarete.Mythique, Categorie = "Cosmique Suprême",
                Description = "Prisme irisé né dans le vide intersidéral, vibrant à la fréquence des galaxies. Permet de forger les reliques et tomes de rang Mythique.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Xanthos Dévoreur Stellaire (Boss Niv. 10)", ZoneOuLieu = "Failles Dimensionnelles", DetailsTaux = "GARANTI (x4 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chevalier du Néant Primordial (Boss Niv. 10)", ZoneOuLieu = "Abîme Originel", DetailsTaux = "GARANTI (x3 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Archonte Solaire Déchu (Boss Niv. 13)", ZoneOuLieu = "Sanctuaire Solaire", DetailsTaux = "GARANTI (x2 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chronos Maître du Temps (Boss Niv. 15)", ZoneOuLieu = "Sabliers Brisés", DetailsTaux = "GARANTI (x4 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Léviathan des Galaxies (Boss Niv. 16)", ZoneOuLieu = "Voie Lactée Perdue", DetailsTaux = "GARANTI (x5 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Abaddon Pourfendeur de Mondes (Boss Niv. 18)", ZoneOuLieu = "Faille Apocalyptique", DetailsTaux = "GARANTI (x4 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Deus Ex Nihilo (Boss Niv. 20)", ZoneOuLieu = "Trône Cosmique", DetailsTaux = "GARANTI (x6 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "🏰 Tour Astrale", NomSource = "Paliers majeurs d'étages (10+)", ZoneOuLieu = "Tour Astrale Infinie", DetailsTaux = "Drop précieux sur boss d'étage" }
                }
            });

            // 18. Matière du Néant
            Fiches.Add(new FicheMateriau {
                Nom = "Matière du Néant", Icone = "🕳️", RareteItem = Rarete.Mythique, Categorie = "Néant Primordial",
                Description = "Substance gravitationnelle instable issue de l'antimatière pure. Base de la Lame Tranche-Matière et du sort Annihilation du Néant.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Xanthos Dévoreur Stellaire (Boss Niv. 10)", ZoneOuLieu = "Failles Dimensionnelles", DetailsTaux = "GARANTI (x5 matières)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chevalier du Néant Primordial (Boss Niv. 10)", ZoneOuLieu = "Abîme Originel", DetailsTaux = "GARANTI (x5 matières)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Abaddon Pourfendeur de Mondes (Boss Niv. 18)", ZoneOuLieu = "Faille Apocalyptique", DetailsTaux = "GARANTI (x4 matières)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Deus Ex Nihilo (Boss Niv. 20)", ZoneOuLieu = "Trône Cosmique", DetailsTaux = "GARANTI (x8 matières)" },
                    new SourceObtentionMateriau { TypeSource = "🏰 Tour Astrale", NomSource = "Sommets ultimes de la Tour (Étages 15+)", ZoneOuLieu = "Tour Astrale", DetailsTaux = "Créatures d'antimatière pure" }
                }
            });

            // 19. Essence du Chaos
            Fiches.Add(new FicheMateriau {
                Nom = "Essence du Chaos", Icone = "👁️", RareteItem = Rarete.Mythique, Categorie = "Divin & Primordial",
                Description = "L'étincelle d'énergie divine primordiale antérieure au temps. Nécessaire pour éveiller l'arme Deus Ex Nihilo et l'Armure Divine de l'Architecte.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "👑 Boss Suprême", NomSource = "Deus Ex Nihilo, Architecte du Chaos Primordial (Boss Niv. 20)", ZoneOuLieu = "Trône Cosmique Suprême", DetailsTaux = "GARANTI (x4 essences divines)" },
                    new SourceObtentionMateriau { TypeSource = "🏆 Tour Astrale", NomSource = "Sommets légendaires (Étages 20+)", ZoneOuLieu = "Tour Astrale Infinie", DetailsTaux = "Récompense de triomphe divin" }
                }
            });

            // 20. Herbes Magiques
            Fiches.Add(new FicheMateriau {
                Nom = "Herbes Magiques", Icone = "🌿", RareteItem = Rarete.Rare, Categorie = "Flore & Alchimie",
                Description = "Plantes médicinales luminescentes gorgées de sève vitale. Ingrédient principal pour concocter les Lots de Potions de Soin et de Mana Majeures.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "🌿 Récolte", NomSource = "Herboristerie après chaque combat", ZoneOuLieu = "Toutes zones & donjons", DetailsTaux = "Taux 50% sur chaque combat victorieux" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zulgar le Chaman Putride (Boss Niv. 5)", ZoneOuLieu = "Marais Corrompus", DetailsTaux = "GARANTI (x4 herbes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Sylvana Reine des Ronces (Boss Niv. 8)", ZoneOuLieu = "Forêt Épineuse Sépulcrale", DetailsTaux = "GARANTI (x8 herbes)" },
                    new SourceObtentionMateriau { TypeSource = "📜 Quêtes", NomSource = "Missions botaniques d'Élénora", ZoneOuLieu = "Village de Val-Serein", DetailsTaux = "Récompense de quête" }
                }
            });

            // 21. Pierres de Forge
            Fiches.Add(new FicheMateriau {
                Nom = "Pierres de Forge", Icone = "💎", RareteItem = Rarete.Epique, Categorie = "Forge & Amélioration",
                Description = "Minéral résonnant indispensable à Brom le Forgeron pour sublimer vos armes et armures du niveau +1 jusqu'au niveau légendaire +10.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "TOUS les monstres réguliers du jeu", ZoneOuLieu = "Toutes les zones et donjons", DetailsTaux = "Taux 25% à 45% (1 pierre)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "TOUS LES BOSS du jeu (Niv. 1 à 20)", ZoneOuLieu = "Toutes les arènes de boss", DetailsTaux = "GARANTI (2 à 5 pierres par boss vaincu !)" },
                    new SourceObtentionMateriau { TypeSource = "♻️ Recyclage", NomSource = "Brom le Forgeron", ZoneOuLieu = "Village de Val-Serein", DetailsTaux = "Démanteler des équipements du sac" },
                    new SourceObtentionMateriau { TypeSource = "📜 Quêtes", NomSource = "Contrats de guilde d'Élénora", ZoneOuLieu = "Village de Val-Serein", DetailsTaux = "Récompense de contrat" }
                }
            });
        }
    }

    // ==============================================================
    // 7. INTERFACE ET OUTILS VISUELS (UI)
    // ==============================================================
    public static class UI
    {
        public static void EffacerConsole()
        {
            try { Console.Clear(); }
            catch { Console.WriteLine("\n" + new string('=', 68) + "\n"); }
        }

        public static void AfficherTitre(string titre)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔" + new string('═', 66) + "╗");
            Console.WriteLine($"║   {titre.PadRight(63)}║");
            Console.WriteLine("╚" + new string('═', 66) + "╝");
            Console.ResetColor();
        }

        public static void LigneSeparateur(char c = '-')
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(c, 68));
            Console.ResetColor();
        }

        public static void AfficherTexte(string texte, ConsoleColor couleur = ConsoleColor.White)
        {
            Console.ForegroundColor = couleur;
            Console.WriteLine(texte);
            Console.ResetColor();
        }

        public static void Pause(string message = "Appuyez sur Entrée pour continuer...")
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"\n[ {message} ] ");
            Console.ResetColor();
            Console.ReadLine();
        }

        public static void BarreDeStatut(string label, int actuel, int max, ConsoleColor couleur, int tailleMax = 20)
        {
            double ratio = Math.Clamp((double)actuel / Math.Max(1, max), 0.0, 1.0);
            int barresPleines = (int)Math.Round(ratio * tailleMax);
            int barresVides = Math.Max(0, tailleMax - barresPleines);

            Console.Write($"{label,-9} [");
            Console.ForegroundColor = couleur;
            Console.Write(new string('█', barresPleines));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('░', barresVides));
            Console.ResetColor();
            Console.WriteLine($"] {actuel,4}/{max,-4}");
        }

        public static void BarreUltime(int actuel, int max = 100)
        {
            int tailleMax = 15;
            double ratio = Math.Clamp((double)actuel / max, 0.0, 1.0);
            int barresPleines = (int)Math.Round(ratio * tailleMax);
            int barresVides = Math.Max(0, tailleMax - barresPleines);

            Console.Write($"{"ULTIME",-9} [");
            Console.ForegroundColor = actuel >= 100 ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            Console.Write(new string('★', barresPleines));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('·', barresVides));
            Console.ResetColor();
            if (actuel >= 100)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("] [PRÊT ! 100%]");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"] {actuel}%");
            }
        }
    }

    // ==============================================================
    // 8. MOTEUR DE JEU PRINCIPAL & SYSTÈMES
    // ==============================================================
    class Program
    {
        private static Joueur hero = null!;
        private static List<Quete> quetesDisponibles = new List<Quete>();
        private static Random rng = Random.Shared;
        private const string FichierSauvegarde = "sauvegarde_aethelgard.json";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormJeu2D());
        }

        // ==============================================================
        // MENU DE DÉPART & SAUVEGARDE
        // ==============================================================
        static void MenuLancement()
        {
            while (true)
            {
                UI.EffacerConsole();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(@"
    ██████╗██╗  ██╗██████╗  ██████╗ ███╗   ██╗██╗ ██████╗██╗   ██╗███████╗███████╗
   ██╔════╝██║  ██║██╔══██╗██╔═══██╗████╗  ██║██║██╔════╝██║   ██║██╔════╝██╔════╝
   ██║     ███████║██████╔╝██║   ██║██╔██╗ ██║██║██║     ██║   ██║█████╗  ███████╗
   ██║     ██╔══██║██╔══██╗██║   ██║██║╚██╗██║██║██║     ██║   ██║██╔══╝  ╚════██║
   ╚██████╗██║  ██║██║  ██║╚██████╔╝██║ ╚████║██║╚██████╗╚██████╔╝███████╗███████║
    ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝ ╚═════╝ ╚═════╝ ╚══════╝╚══════╝
                     ⚔️  D' A E T H E L G A R D  -  R P G  ⚔️
                ");
                Console.ResetColor();
                UI.AfficherTexte("     Version Ultime : 5 Classes, Multiples Donjons, Forgeron, Alchimie, Boss Mythiques & Loot Infini !", ConsoleColor.Cyan);
                UI.LigneSeparateur('=');

                bool sauvegardeExiste = File.Exists(FichierSauvegarde);
                Console.WriteLine("1. ⚔️  Créer une Nouvelle Partie");
                if (sauvegardeExiste)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("2. 💾  Continuer la partie sauvegardée");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("2. 💾  Continuer (Aucune sauvegarde détectée)");
                    Console.ResetColor();
                }
                Console.WriteLine("3. 📜  À propos & Règles du RPG");
                Console.WriteLine("4. 🚪  Quitter le jeu");

                Console.Write("\nVotre choix > ");
                string choix = Console.ReadLine()?.Trim() ?? "";

                if (choix == "1")
                {
                    AfficherCreationPersonnage();
                    InitialiserQuetes();
                    break;
                }
                else if (choix == "2" && sauvegardeExiste)
                {
                    if (ChargerPartie())
                    {
                        break;
                    }
                }
                else if (choix == "3")
                {
                    AfficherGuideJeu();
                }
                else if (choix == "4")
                {
                    Environment.Exit(0);
                }
            }
        }

        static void AfficherGuideJeu()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("GUIDE ET MÉCANIQUES D'AETHELGARD");
            Console.WriteLine("• CLASSES : Guerrier (tank/dégâts bruts), Mage (sorts violents/soin),");
            Console.WriteLine("  Rôdeur (critique/esquive/poison), Paladin (défense divine), Nécromancien (drain/vampirisme).");
            Console.WriteLine("• ULTIME : Frappez et subissez des coups pour remplir votre jauge d'Ultime à 100%.");
            Console.WriteLine("• CARACTÉRISTIQUES : À chaque niveau, investissez vos 4 points de stats !");
            Console.WriteLine("• FORGE : Améliorez vos armes et armures jusqu'à +10 avec des Pierres de Forge !");
            Console.WriteLine("• ALCHIMIE : Concoctez des potions et élixirs chez Dame Elowen.");
            Console.WriteLine("• BUTIN & RARETÉ : Commun (Gris) -> Rare (Bleu) -> Épique (Violet) -> Légendaire (Doré) -> Mythique (Rouge) !");
            Console.WriteLine("• TOUR ASTRALE : Donjon infini sans pitié avec boss divins et loots légendaires !");
            UI.Pause();
        }

        // ==============================================================
        // CRÉATION DE PERSONNAGE
        // ==============================================================
        static void AfficherCreationPersonnage()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("CRÉATION DE VOTRE CHAMPION");

            string nom = "";
            while (string.IsNullOrWhiteSpace(nom))
            {
                Console.Write("Entrez le noble nom de votre héros > ");
                nom = Console.ReadLine()?.Trim() ?? "";
            }

            UI.AfficherTitre("CHOISISSEZ VOTRE CLASSE");
            Console.WriteLine("1. 🛡️  GUERRIER     : Armure lourde, gros PV, frappes sismiques brutales.");
            Console.WriteLine("2. 🔮  MAGE         : Dévastation arcanique, boule de feu, soin puissant.");
            Console.WriteLine("3. 🏹  RÔDEUR       : Vitesse fulgurante, coups critiques, flèches empoisonnées.");
            Console.WriteLine("4. ⚔️  PALADIN      : Chevalier divin, haute résistance, châtiment sacré.");
            Console.WriteLine("5. 💀  NÉCROMANCIEN : Maître des ombres, drain de vie vampirique, décrépitude.");

            ClasseType classeChoisie = ClasseType.Guerrier;
            bool choixValide = false;
            while (!choixValide)
            {
                Console.Write("\nVotre choix (1 à 5) > ");
                string choix = Console.ReadLine()?.Trim() ?? "";
                switch (choix)
                {
                    case "1": classeChoisie = ClasseType.Guerrier; choixValide = true; break;
                    case "2": classeChoisie = ClasseType.Mage; choixValide = true; break;
                    case "3": classeChoisie = ClasseType.Rodeur; choixValide = true; break;
                    case "4": classeChoisie = ClasseType.Paladin; choixValide = true; break;
                    case "5": classeChoisie = ClasseType.Necromancien; choixValide = true; break;
                    default: UI.AfficherTexte("Choix invalide. Tapez un chiffre entre 1 et 5.", ConsoleColor.Red); break;
                }
            }

            hero = new Joueur(nom, classeChoisie);
            UI.AfficherTexte($"\nGloire à {hero.Nom} le {hero.Classe} ! Puisse votre lame chasser les ténèbres !", ConsoleColor.Green);
            UI.Pause();
        }

        static void InitialiserQuetes()
        {
            quetesDisponibles = new List<Quete>
            {
                new Quete(1, "Nettoyage des Maraudeurs", "Éliminez 3 Gobelins Maraudeurs dans la forêt.", "Gobelin Maraudeur", 3, 90, 150, 2,
                    new Equipement("Dague Dentelée de l'Ombre", TypeEquipement.Arme, Rarete.Rare, 12, 0, 0, 0, 8, 4, 0, 80)),

                new Quete(2, "Purification de la Crypte", "Purifiez les catacombes de 3 Squelettes Gardiens.", "Squelette Gardien", 3, 160, 260, 3,
                    new Equipement("Heaume Renforcé du Templier", TypeEquipement.Casque, Rarete.Rare, 0, 8, 25, 10, 0, 0, 0, 120)),

                new Quete(3, "La Menace de Grok", "Traquez et terrassez Grok, Seigneur de la Horde Gobeline.", "Grok, Seigneur de la Horde Gobeline", 1, 300, 450, 4,
                    new Equipement("Hache de Guerre Fracassante", TypeEquipement.Arme, Rarete.Epique, 22, 4, 0, 0, 12, 0, 0, 250)),

                new Quete(4, "Le Rituel de Malakor", "Mettez fin aux rituels morbides de Malakor le Nécromancien.", "Malakor, Seigneur de la Mort Éternelle", 1, 500, 750, 6,
                    new Equipement("Robe Astrale de l'Archimage", TypeEquipement.Armure, Rarete.Epique, 4, 16, 45, 50, 6, 4, 0, 400)),

                new Quete(5, "Le Blizzard Assassin", "Défiez Kaelas, l'Archifée du Blizzard Éternel.", "Kaelas, Archifée du Blizzard Éternel", 1, 900, 1300, 8,
                    new Equipement("Amulette du Cœur Gelé", TypeEquipement.Amulette, Rarete.Legendaire, 10, 10, 60, 40, 10, 8, 5, 800)),

                new Quete(6, "Le Réveil du Dragon", "Terrassez Ignis, Dragon Millénaire Suprême au mont volcanique.", "Ignis, Dragon Millénaire Suprême", 1, 1800, 2400, 15,
                    new Equipement("Épée Solaire Flamboyante d'Aethelgard", TypeEquipement.Arme, Rarete.Legendaire, 42, 10, 80, 40, 18, 5, 8, 1500)),

                new Quete(7, "L'Ombre du Néant", "Terrassez le dévoreur stellaire Xanthos au sommet de la Tour Astrale.", "Xanthos, Dévoreur Stellaire du Néant", 1, 4000, 5000, 25,
                    new Equipement("Couronne Divine d'Éternité", TypeEquipement.Casque, Rarete.Mythique, 25, 30, 200, 150, 25, 15, 10, 3500))
            };
        }

        // ==============================================================
        // HUB PRINCIPAL DU VILLAGE DE VAL-SEREIN
        // ==============================================================
        static void BouclePrincipale()
        {
            bool quitter = false;

            while (!quitter)
            {
                UI.EffacerConsole();
                UI.AfficherTitre($"VILLAGE DE VAL-SEREIN | {hero.Nom} (Niv. {hero.Niveau} {hero.Classe})");
                Console.WriteLine($"💰 Or : {hero.Or}  |  💎 Pierres de Forge : {hero.PierresDeForge}  |  🌿 Herbes : {hero.HerbesMagiques}  |  🐉 Écailles : {hero.EcaillesDeMonstre}");
                UI.BarreDeStatut("Santé", hero.PVActuels, hero.PVMaxTotal, ConsoleColor.Green);
                UI.BarreDeStatut("Mana ", hero.ManaActuel, hero.ManaMaxTotal, ConsoleColor.Blue);

                if (hero.PointsCaracteristiques > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"⭐ VOUS AVEZ {hero.PointsCaracteristiques} POINTS D'ATTRIBUTS NON ALLOUÉS ! (Voir Fiche Héros)");
                    Console.ResetColor();
                }

                UI.LigneSeparateur();
                Console.WriteLine("Où souhaitez-vous aller ?");
                Console.WriteLine("1. ⚔️  Partir en Expédition (Zones de monstres & Boss)");
                Console.WriteLine("2. 🌌  La Tour Astrale du Néant (Donjon infini à étages)");
                Console.WriteLine("3. 📜  Guilde des Aventuriers & Primes");
                Console.WriteLine("4. ⚒️  La Grande Forge de Brom (Acheter, Améliorer +1 à +10, Recycler)");
                Console.WriteLine("5. 🧪  Laboratoire Alchimique de Dame Elowen (Potions & Craft)");
                Console.WriteLine("6. 🍻  Auberge du Sanglier Doré (Repos & Mini-Jeu de Tripot)");
                Console.WriteLine("7. 👤  Fiche du Héros, Sac & Répartition des Attributs");
                Console.WriteLine("8. 📚  Sanctuaire des Sages (Bestiaire & Salle des Hauts Faits)");
                Console.WriteLine("9. 💾  Sauvegarder la partie");
                Console.WriteLine("0. 🚪  Quitter le jeu");

                Console.Write("\nVotre choix > ");
                string choix = Console.ReadLine()?.Trim() ?? "";

                switch (choix)
                {
                    case "1": MenuExpeditions(); break;
                    case "2": MenuTourAstrale(); break;
                    case "3": MenuGuilde(); break;
                    case "4": MenuForge(); break;
                    case "5": MenuAlchimie(); break;
                    case "6": MenuAuberge(); break;
                    case "7": MenuFicheEtInventaire(); break;
                    case "8": MenuBestiaireEtHautsFaits(); break;
                    case "9": SauvegarderPartie(); break;
                    case "0":
                        Console.Write("Voulez-vous sauvegarder avant de quitter ? (o/n) > ");
                        string r = Console.ReadLine()?.Trim().ToLower() ?? "";
                        if (r == "o" || r == "oui") SauvegarderPartie();
                        quitter = true;
                        break;
                    default:
                        UI.AfficherTexte("Choix invalide.", ConsoleColor.Red);
                        Thread.Sleep(700);
                        break;
                }
            }

            UI.AfficherTexte("\nQue les étoiles veillent sur vous, noble héros. À bientôt sur Aethelgard !", ConsoleColor.Yellow);
        }

        // ==============================================================
        // EXPÉDITIONS & ZONES DE COMBAT
        // ==============================================================
        static void MenuExpeditions()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("EXPÉDITIONS DANS LES CONTRÉES D'AETHELGARD");
            Console.WriteLine("Choisissez votre zone de destination :");
            Console.WriteLine("1. 🌲 La Forêt des Brumes (Niv. 1-3) - Loups, Gobelins, Araignées venimeuses");
            Console.WriteLine("2. 💀 Les Catacombes Hantées (Niv. 3-5) - Squelettes, Goules, Spectres");
            Console.WriteLine("3. ❄️ La Cime Glaciale (Niv. 5-7) - Wendigos, Golems de Glace, Loups arctiques");
            Console.WriteLine("4. 🌋 Les Abîmes de Feu (Niv. 7-9) - Diablotins, Drakes, Golems de Magma");
            Console.WriteLine("--- AFFRONTER DIRECTEMENT LES BOSS LÉGENDAIRES ---");
            Console.WriteLine("5. 👑 Boss : Grok, Seigneur Gobelin (Niv. 2+)");
            Console.WriteLine("6. 💀 Boss : Malakor le Nécromancien (Niv. 4+)");
            Console.WriteLine("7. ❄️ Boss : Kaelas, Reine du Blizzard (Niv. 6+)");
            Console.WriteLine("8. 🐉 Boss Ultime : Ignis, Dragon Millénaire Suprême (Niv. 8+)");
            Console.WriteLine("9. ↩️  Retourner au village");

            Console.Write("\nVotre destination > ");
            string choix = Console.ReadLine()?.Trim() ?? "";

            switch (choix)
            {
                case "1": ExplorerZone(TypeZone.Foret); break;
                case "2": ExplorerZone(TypeZone.Catacombes); break;
                case "3": ExplorerZone(TypeZone.ForteresseGivre); break;
                case "4": ExplorerZone(TypeZone.Volcan); break;
                case "5": LancerCombat(new BossGrok()); break;
                case "6": LancerCombat(new BossMalakor()); break;
                case "7": LancerCombat(new BossKaelas()); break;
                case "8": LancerCombat(new BossDragonIgnis()); break;
                case "9": return;
            }
        }

        static void ExplorerZone(TypeZone zone)
        {
            UI.EffacerConsole();
            string nomZone = zone switch
            {
                TypeZone.Foret => "La Forêt des Brumes",
                TypeZone.Catacombes => "Les Catacombes Hantées",
                TypeZone.ForteresseGivre => "La Cime Glaciale",
                TypeZone.Volcan => "Les Abîmes de Feu",
                _ => "Terres Inconnues"
            };

            UI.AfficherTitre($"EXPLORATION : {nomZone.ToUpper()}");
            UI.AfficherTexte("Vous vous avancez prudemment...", ConsoleColor.DarkYellow);
            Thread.Sleep(800);

            // Événement aléatoire : Rencontre ennemie (70%), Coffre au trésor (15%), Source de guérison (15%)
            int jet = rng.Next(100);

            if (jet < 15)
            {
                // Source de bénédiction
                UI.AfficherTexte("\n✨ Vous découvrez un sanctuaire mystique oublié !", ConsoleColor.Cyan);
                UI.AfficherTexte("Une énergie bienfaisante imprègne votre être. Vos PV et votre Mana sont totalement restaurés !", ConsoleColor.Green);
                hero.PVActuels = hero.PVMaxTotal;
                hero.ManaActuel = hero.ManaMaxTotal;
                hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 30);
                UI.Pause();
            }
            else if (jet < 25)
            {
                // Coffre au trésor abandonné
                UI.AfficherTexte("\n🎁 Vous dénichez un antique Coffre au Trésor caché dans un recoin !", ConsoleColor.Yellow);
                OuvrirCoffreAuTresor(false);
                UI.Pause();
            }
            else
            {
                // Combat de monstre régulier
                Monstre monstre = GenererMonstrePourZone(zone);
                LancerCombat(monstre);
            }
        }

        static Monstre GenererMonstrePourZone(TypeZone zone)
        {
            switch (zone)
            {
                case TypeZone.Foret:
                    int f = rng.Next(4);
                    return f switch
                    {
                        0 => new Monstre("Sanglier Enragé", 60, 15, 6, 50, 25, false, "", "Tranchant", "Bête sauvage des sous-bois."),
                        1 => new Monstre("Gobelin Maraudeur", 75, 17, 7, 65, 35, false, "", "Feu", "Petit pillard rusé armé d'une pique."),
                        2 => new Monstre("Loup Alpha des Brumes", 85, 20, 8, 80, 45, false, "", "Glace", "Prédateur agile qui chasse en meute."),
                        _ => new Monstre("Araignée Tisse-Venin", 90, 22, 9, 95, 55, false, "", "Feu", "Arachnide géante sécrétant des acides corrosifs.")
                    };

                case TypeZone.Catacombes:
                    int c = rng.Next(4);
                    return c switch
                    {
                        0 => new Monstre("Squelette Gardien", 120, 24, 12, 120, 60, false, "", "Sacré", "Guerrier d'antan relevé par la nécromancie."),
                        1 => new Monstre("Goule Affamée", 140, 26, 10, 140, 75, false, "", "Feu", "Créature putride vorace errant dans les tombes."),
                        2 => new Monstre("Spectre Hurlant", 130, 29, 14, 170, 90, false, "", "Magie", "Esprit tourmenté traversant la matière."),
                        _ => new Monstre("Chevalier de la Peste", 160, 31, 16, 210, 110, false, "", "Lumière", "Paladin déchu corrompu par le poison.")
                    };

                case TypeZone.ForteresseGivre:
                    int g = rng.Next(3);
                    return g switch
                    {
                        0 => new Monstre("Loup Arctique du Blizzard", 175, 34, 16, 230, 120, false, "", "Feu", "Loup féroce aux poils durs comme le givre."),
                        1 => new Monstre("Guerrier Wendigo", 210, 38, 18, 280, 150, false, "", "Feu", "Bête maudite dévoreuse de chair congelée."),
                        _ => new Monstre("Golem de Permafrost", 260, 36, 24, 340, 180, false, "", "Impact", "Colosse de glace quasi indestructible.")
                    };

                case TypeZone.Volcan:
                default:
                    int v = rng.Next(3);
                    return v switch
                    {
                        0 => new Monstre("Diablotin Pyromane", 220, 42, 17, 360, 190, false, "", "Glace", "Démon farceur crachant des braises sulfureuses."),
                        1 => new Monstre("Drake Rouge Ardent", 280, 48, 22, 450, 240, false, "", "Glace", "Cousin ailé des dragons aux écailles tranchantes."),
                        _ => new Monstre("Golem de Magma Vivant", 340, 46, 28, 520, 290, false, "", "Eau/Glace", "Masse de roche en fusion animée par un noyau ardent.")
                    };
            }
        }

        // ==============================================================
        // LA TOUR ASTRALE DU NÉANT (DONJON INFINI)
        // ==============================================================
        static void MenuTourAstrale()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("LA TOUR ASTRALE DU NÉANT");
            Console.WriteLine("Une tour titanesque suspendue dans le vide cosmique.");
            Console.WriteLine("Chaque étage abrite des ennemis plus redoutables, mais réserve des trésors divins !");
            Console.WriteLine($"Votre record d'ascension actuel : Étage {hero.EtageTourAstraleMax}\n");

            Console.WriteLine("1. 🌌 Gravir un étage supérieur de la Tour");
            Console.WriteLine("2. 👁️ Affronter directement XANTHOS (Boss Secret Niv 10+)");
            Console.WriteLine("3. ↩️ Faire demi-tour vers le village");

            Console.Write("\nVotre choix > ");
            string choix = Console.ReadLine()?.Trim() ?? "";

            if (choix == "1")
            {
                int prochainEtage = hero.EtageTourAstraleMax + 1;
                UI.AfficherTexte($"\nVous franchissez le portail de distorsion vers l'ÉTAGE {prochainEtage}...", ConsoleColor.Magenta);
                Thread.Sleep(800);

                int pvMonstre = 120 + (prochainEtage * 40);
                int atkMonstre = 20 + (prochainEtage * 6);
                int defMonstre = 10 + (prochainEtage * 4);
                int xpMonstre = 150 + (prochainEtage * 80);
                int orMonstre = 80 + (prochainEtage * 50);

                string[] nomsCréatures = { "Sentinelle Astrale", "Ombre du Vide", "Chasseur Cosmique", "Annihilateur de Réalité" };
                string nomEnnemi = $"{nomsCréatures[rng.Next(nomsCréatures.Length)]} (Étage {prochainEtage})";

                Monstre monstreTour = new Monstre(nomEnnemi, pvMonstre, atkMonstre, defMonstre, xpMonstre, orMonstre, prochainEtage % 5 == 0);
                if (prochainEtage % 5 == 0)
                {
                    monstreTour.CriDeGuerre = "AUCUN MORTEL NE PEUT DÉFIER L'ORDRE ASTRAL !";
                }

                bool victoire = LancerCombat(monstreTour);
                if (victoire && prochainEtage > hero.EtageTourAstraleMax)
                {
                    hero.EtageTourAstraleMax = prochainEtage;
                    hero.PierresDeForge += 2;
                    UI.AfficherTexte($"🏆 NOUVEAU RECORD ! Vous avez validé l'Étage {hero.EtageTourAstraleMax} !", ConsoleColor.Yellow);
                    UI.AfficherTexte("Bonus : +2 Pierres de Forge cosmiques !", ConsoleColor.Cyan);
                }
            }
            else if (choix == "2")
            {
                LancerCombat(new BossXanthos());
            }
        }

        // ==============================================================
        // SYSTÈME DE COMBAT TOUR PAR TOUR ULTIME
        // ==============================================================
        static bool LancerCombat(Monstre ennemi)
        {
            UI.EffacerConsole();
            Console.ForegroundColor = ennemi.EstBoss ? ConsoleColor.Magenta : ConsoleColor.Red;
            Console.WriteLine(ennemi.EstBoss ? "★★★ ALERTE : COMBAT DE BOSS DÉCISIF ★★★" : "⚠️ UN MONSTRE EMERGE DES OMBRES !");
            Console.ResetColor();

            if (!string.IsNullOrEmpty(ennemi.CriDeGuerre))
            {
                UI.AfficherTexte($"\n{ennemi.Nom} rugit : \"{ennemi.CriDeGuerre}\"", ConsoleColor.DarkYellow);
            }
            UI.Pause("En garde ! Le combat commence...");

            bool postureDefense = false;
            int toursPoisonEnnemi = 0;
            int toursBrulureEnnemi = 0;
            int buffAttaqueTours = 0;
            int buffDefenseTours = 0;

            while (hero.PVActuels > 0 && ennemi.PVActuels > 0)
            {
                UI.EffacerConsole();
                UI.AfficherTitre($"COMBAT CONTRE : {ennemi.Nom}");

                // Barres de statut du monstre et du joueur
                UI.BarreDeStatut($"Monstre", ennemi.PVActuels, ennemi.PVMax, ConsoleColor.Red);
                Console.WriteLine();
                UI.BarreDeStatut($"Santé  ", hero.PVActuels, hero.PVMaxTotal, ConsoleColor.Green);
                UI.BarreDeStatut($"Mana   ", hero.ManaActuel, hero.ManaMaxTotal, ConsoleColor.Blue);
                UI.BarreUltime(hero.JaugeUltime);
                UI.LigneSeparateur();

                // Dégâts sur la durée (DoTs) sur l'ennemi
                if (toursPoisonEnnemi > 0)
                {
                    int degatsPoison = 16;
                    ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degatsPoison);
                    UI.AfficherTexte($"🧪 Le poison ronge {ennemi.Nom} (-{degatsPoison} PV) ! ({toursPoisonEnnemi} tours)", ConsoleColor.DarkGreen);
                    toursPoisonEnnemi--;
                    if (ennemi.PVActuels <= 0) break;
                }
                if (toursBrulureEnnemi > 0)
                {
                    int degatsFeu = 22;
                    ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degatsFeu);
                    UI.AfficherTexte($"🔥 Les flammes consument {ennemi.Nom} (-{degatsFeu} PV) ! ({toursBrulureEnnemi} tours)", ConsoleColor.DarkYellow);
                    toursBrulureEnnemi--;
                    if (ennemi.PVActuels <= 0) break;
                }

                // MENU ACTIONS
                Console.WriteLine("Que voulez-vous faire ?");
                Console.WriteLine("1. ⚔️  Attaque Basique (Gain Ultime +15%)");
                Console.WriteLine("2. 💥  Attaque Lourde (Forte mais 20% risque de manquer)");
                Console.WriteLine($"3. 🔮  Sorts & Compétences de Classe ({hero.Classe})");
                if (hero.JaugeUltime >= 100)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("4. ⭐  DÉCHAÎNER L'ATTAQUE ULTIME ! [PRÊTE]");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"4. ⭐  Attaque Ultime ({hero.JaugeUltime}/100% requis)");
                    Console.ResetColor();
                }
                Console.WriteLine("5. 🧪  Poche de Consommables (Potions, Élixirs, Bombes)");
                Console.WriteLine("6. 🛡️  Posture Défensive (Dégâts /2, +20 Mana, +10% Ultime)");
                Console.WriteLine("7. 🏃  Tenter la Fuite");

                Console.Write("\nVotre décision > ");
                string action = Console.ReadLine()?.Trim() ?? "";
                bool actionValidee = false;
                postureDefense = false;

                int bonusAtkCombat = buffAttaqueTours > 0 ? (int)(hero.AttaqueTotale * 0.4) : 0;

                switch (action)
                {
                    case "1": // Attaque normale
                        int variance = rng.Next(-3, 4);
                        bool critique = rng.Next(100) < hero.ChanceCritiqueTotale;
                        int degats = Math.Max(2, (hero.AttaqueTotale + bonusAtkCombat + variance) - ennemi.Defense);

                        if (critique)
                        {
                            degats = (int)(degats * 1.75);
                            UI.AfficherTexte($"⚡ COUP CRITIQUE DÉVASTATEUR ! {degats} dégâts infligés !", ConsoleColor.Yellow);
                        }
                        else
                        {
                            UI.AfficherTexte($"🗡️ Vous frappez {ennemi.Nom} pour {degats} dégâts.", ConsoleColor.White);
                        }

                        // Effet de vampirisme éventuel
                        if (hero.VampirismeTotal > 0)
                        {
                            int pvVole = (int)(degats * (hero.VampirismeTotal / 100.0));
                            if (pvVole > 0)
                            {
                                hero.Soigner(pvVole);
                                UI.AfficherTexte($"🩸 Vol de vie : vous récupérez {pvVole} PV !", ConsoleColor.DarkRed);
                            }
                        }

                        ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                        hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 15);
                        actionValidee = true;
                        break;

                    case "2": // Attaque lourde
                        if (rng.Next(100) < 20)
                        {
                            UI.AfficherTexte($"❌ Votre attaque lourde est trop lente ! {ennemi.Nom} l'esquive habilement !", ConsoleColor.DarkRed);
                        }
                        else
                        {
                            int degatsLourds = (int)((hero.AttaqueTotale + bonusAtkCombat) * 1.6) - ennemi.Defense;
                            degatsLourds = Math.Max(5, degatsLourds);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degatsLourds);
                            UI.AfficherTexte($"🔨 FRACAS GÉANT ! Vous ébranlez {ennemi.Nom} pour {degatsLourds} dégâts brutaux !", ConsoleColor.Magenta);
                            hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 20);
                        }
                        actionValidee = true;
                        break;

                    case "3": // Compétences de classe
                        actionValidee = ExecuterMenuCompetences(ennemi, ref toursPoisonEnnemi, ref toursBrulureEnnemi);
                        break;

                    case "4": // Ultime
                        if (hero.JaugeUltime >= 100)
                        {
                            hero.JaugeUltime = 0;
                            ExecuterAttaqueUltime(ennemi);
                            actionValidee = true;
                        }
                        else
                        {
                            UI.AfficherTexte("❌ Votre jauge d'Ultime n'est pas encore pleine !", ConsoleColor.Red);
                        }
                        break;

                    case "5": // Consommables
                        actionValidee = UtiliserConsommableEnCombat(ennemi, ref toursBrulureEnnemi, ref buffAttaqueTours, ref buffDefenseTours);
                        break;

                    case "6": // Défense
                        postureDefense = true;
                        hero.RestaurerMana(20);
                        hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 10);
                        UI.AfficherTexte("🛡️ Vous levez votre bouclier ! Dégâts divisés par 2, +20 Mana, +10% Ultime.", ConsoleColor.Cyan);
                        actionValidee = true;
                        break;

                    case "7": // Fuite
                        if (ennemi.EstBoss)
                        {
                            UI.AfficherTexte("⛔ Un champ de force draconique vous empêche de fuir un Boss !", ConsoleColor.Red);
                        }
                        else if (rng.Next(100) < 55 + hero.ChanceEsquiveTotale)
                        {
                            UI.AfficherTexte("💨 Vous profitez d'une brèche pour vous enfuir avec succès !", ConsoleColor.Yellow);
                            UI.Pause();
                            return false;
                        }
                        else
                        {
                            UI.AfficherTexte("❌ Fuite échouée ! L'ennemi vous barre la route !", ConsoleColor.Red);
                            actionValidee = true;
                        }
                        break;

                    default:
                        UI.AfficherTexte("Choix invalide.", ConsoleColor.Red);
                        break;
                }

                if (!actionValidee)
                {
                    Thread.Sleep(800);
                    continue;
                }

                // Vérification si ennemi terrassé
                if (ennemi.PVActuels <= 0) break;

                // TOURS DE BUFFS DÉCOMPTÉS
                if (buffAttaqueTours > 0) buffAttaqueTours--;
                if (buffDefenseTours > 0) buffDefenseTours--;

                // TOUR DE L'ENNEMI
                Thread.Sleep(600);
                Console.WriteLine();

                // Chance d'esquive du joueur
                if (rng.Next(100) < hero.ChanceEsquiveTotale)
                {
                    UI.AfficherTexte($"🤸 ESQUIVE PARFAITE ! Vous esquivez l'assaut de {ennemi.Nom} avec aisance !", ConsoleColor.Cyan);
                }
                else
                {
                    int degatsEnnemi = ennemi.Attaquer(hero, out string msgAction);

                    if (postureDefense)
                    {
                        int degatsReduits = degatsEnnemi / 2;
                        hero.PVActuels += (degatsEnnemi - degatsReduits);
                        UI.AfficherTexte(msgAction, ConsoleColor.Red);
                        UI.AfficherTexte($"🛡️ Votre posture défensive absorbe l'impact : vous n'encaissez que {degatsReduits} dégâts !", ConsoleColor.Cyan);
                    }
                    else if (buffDefenseTours > 0)
                    {
                        int degatsReduits = (int)(degatsEnnemi * 0.5);
                        hero.PVActuels += (degatsEnnemi - degatsReduits);
                        UI.AfficherTexte(msgAction, ConsoleColor.Red);
                        UI.AfficherTexte($"🪨 Peau de Pierre : vous n'encaissez que {degatsReduits} dégâts !", ConsoleColor.DarkYellow);
                    }
                    else
                    {
                        UI.AfficherTexte(msgAction, ConsoleColor.Red);
                    }

                    // Encaisser des dégâts charge l'Ultime
                    hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 10);
                }

                Thread.Sleep(900);
            }

            // CONCLUSION
            if (hero.PVActuels > 0)
            {
                VictoireCombat(ennemi);
                return true;
            }
            else
            {
                DefaiteCombat(ennemi);
                return false;
            }
        }

        // ==============================================================
        // SORTS ET COMPÉTENCES PAR CLASSE
        // ==============================================================
        static bool ExecuterMenuCompetences(Monstre ennemi, ref int toursPoison, ref int toursBrulure)
        {
            Console.WriteLine($"\n--- Sorts & Aptitudes de {hero.Classe} ---");

            switch (hero.Classe)
            {
                case ClasseType.Guerrier:
                    Console.WriteLine("1. 💥 Brise-Crâne (25 Mana) - Gros dégâts physiques");
                    Console.WriteLine("2. 🌀 Tourbillon de Lames (40 Mana) - Dégâts violents + boost Ultime");
                    Console.Write("Votre choix > ");
                    string cg = Console.ReadLine()?.Trim() ?? "";
                    if (cg == "1")
                    {
                        if (hero.ManaActuel >= 25)
                        {
                            hero.ManaActuel -= 25;
                            int degats = Math.Max(18, (int)(hero.AttaqueTotale * 2.1) - ennemi.Defense);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                            hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 15);
                            UI.AfficherTexte($"💥 BRISE-CRÂNE ! Votre coup fracasse l'armure ennemie pour {degats} dégâts !", ConsoleColor.Yellow);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (25 requis) !", ConsoleColor.Red);
                    }
                    else if (cg == "2")
                    {
                        if (hero.ManaActuel >= 40)
                        {
                            hero.ManaActuel -= 40;
                            int degats = Math.Max(25, (int)(hero.AttaqueTotale * 2.7) - ennemi.Defense);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                            hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 30);
                            UI.AfficherTexte($"🌀 TOURBILLON DE FER ! Une tornade d'acier lacère l'ennemi pour {degats} dégâts !", ConsoleColor.Magenta);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (40 requis) !", ConsoleColor.Red);
                    }
                    break;

                case ClasseType.Mage:
                    Console.WriteLine("1. 🔥 Pyrosphère d'Éther (35 Mana) - Dégâts arcaniques + Brûlure");
                    Console.WriteLine("2. ⚡ Chaîne d'Éclairs (45 Mana) - Pulvérise l'armure");
                    Console.WriteLine("3. 💚 Bénédiction de Soin (30 Mana) - Restaure 90 PV");
                    Console.Write("Votre choix > ");
                    string cm = Console.ReadLine()?.Trim() ?? "";
                    if (cm == "1")
                    {
                        if (hero.ManaActuel >= 35)
                        {
                            hero.ManaActuel -= 35;
                            int degats = (int)(hero.AttaqueTotale * 2.4);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                            toursBrulure = 3;
                            UI.AfficherTexte($"🔥 PYROSPHÈRE ! {ennemi.Nom} est carbonisé pour {degats} dégâts et brûle pour 3 tours !", ConsoleColor.DarkYellow);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (35 requis) !", ConsoleColor.Red);
                    }
                    else if (cm == "2")
                    {
                        if (hero.ManaActuel >= 45)
                        {
                            hero.ManaActuel -= 45;
                            int degats = (int)(hero.AttaqueTotale * 3.1);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                            UI.AfficherTexte($"⚡ FOUDRE DIVINE ! L'éclair frappe de plein fouet pour {degats} dégâts d'électricité !", ConsoleColor.Cyan);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (45 requis) !", ConsoleColor.Red);
                    }
                    else if (cm == "3")
                    {
                        if (hero.ManaActuel >= 30)
                        {
                            hero.ManaActuel -= 30;
                            hero.Soigner(90);
                            UI.AfficherTexte("✨ Une pluie de rayons dorés restaure 90 de vos Points de Vie !", ConsoleColor.Green);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (30 requis) !", ConsoleColor.Red);
                    }
                    break;

                case ClasseType.Rodeur:
                    Console.WriteLine("1. 🏹 Flèche Toxique (20 Mana) - Dégâts + Poison 3 tours");
                    Console.WriteLine("2. 🎯 Tir Transperçant (35 Mana) - Ignore 50% de la défense");
                    Console.Write("Votre choix > ");
                    string cr = Console.ReadLine()?.Trim() ?? "";
                    if (cr == "1")
                    {
                        if (hero.ManaActuel >= 20)
                        {
                            hero.ManaActuel -= 20;
                            int degats = Math.Max(12, (int)(hero.AttaqueTotale * 1.6) - ennemi.Defense);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                            toursPoison = 3;
                            UI.AfficherTexte($"🏹 FLÈCHE VENIMEUSE ! {degats} dégâts et {ennemi.Nom} est gravement empoisonné !", ConsoleColor.DarkGreen);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (20 requis) !", ConsoleColor.Red);
                    }
                    else if (cr == "2")
                    {
                        if (hero.ManaActuel >= 35)
                        {
                            hero.ManaActuel -= 35;
                            int degats = Math.Max(20, (int)(hero.AttaqueTotale * 2.5) - (ennemi.Defense / 2));
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                            UI.AfficherTexte($"🎯 FLÈCHE PERFORANTE ! Le projectile transperce la cible pour {degats} dégâts mortels !", ConsoleColor.Yellow);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (35 requis) !", ConsoleColor.Red);
                    }
                    break;

                case ClasseType.Paladin:
                    Console.WriteLine("1. ⚔️ Châtiment Sacré (30 Mana) - Dégâts divins accrus contre les monstres");
                    Console.WriteLine("2. 🛡️ Imposition des Mains (40 Mana) - Soigne 110 PV et restaure 20% Ultime");
                    Console.Write("Votre choix > ");
                    string cp = Console.ReadLine()?.Trim() ?? "";
                    if (cp == "1")
                    {
                        if (hero.ManaActuel >= 30)
                        {
                            hero.ManaActuel -= 30;
                            int degats = (int)(hero.AttaqueTotale * 2.2) - (ennemi.Defense / 2);
                            degats = Math.Max(20, degats);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                            UI.AfficherTexte($"⚔️ JUGEMENT DE LA LUMIÈRE ! Une aura sainte punit {ennemi.Nom} pour {degats} dégâts !", ConsoleColor.Yellow);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (30 requis) !", ConsoleColor.Red);
                    }
                    else if (cp == "2")
                    {
                        if (hero.ManaActuel >= 40)
                        {
                            hero.ManaActuel -= 40;
                            hero.Soigner(110);
                            hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 20);
                            UI.AfficherTexte("🛡️ Grâce divine : +110 PV et +20% d'Ultime récupérés !", ConsoleColor.Green);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (40 requis) !", ConsoleColor.Red);
                    }
                    break;

                case ClasseType.Necromancien:
                    Console.WriteLine("1. 💀 Ponction Mortelle (30 Mana) - Dégâts ténébreux + soigne 60% des dégâts");
                    Console.WriteLine("2. ☠️ Nuage Putride (40 Mana) - Empoisonne férocement pour 4 tours");
                    Console.Write("Votre choix > ");
                    string cn = Console.ReadLine()?.Trim() ?? "";
                    if (cn == "1")
                    {
                        if (hero.ManaActuel >= 30)
                        {
                            hero.ManaActuel -= 30;
                            int degats = (int)(hero.AttaqueTotale * 2.0);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degats);
                            int soin = (int)(degats * 0.6);
                            hero.Soigner(soin);
                            UI.AfficherTexte($"💀 FAUX D'ÂME ! {degats} dégâts infligés et vous absorbez {soin} PV !", ConsoleColor.DarkMagenta);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (30 requis) !", ConsoleColor.Red);
                    }
                    else if (cn == "2")
                    {
                        if (hero.ManaActuel >= 40)
                        {
                            hero.ManaActuel -= 40;
                            int degatsInit = (int)(hero.AttaqueTotale * 1.5);
                            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degatsInit);
                            toursPoison = 4;
                            UI.AfficherTexte($"☠️ MIASMES NÉCROTIQUES ! {degatsInit} dégâts immédiats et poison mortel actif !", ConsoleColor.DarkGreen);
                            return true;
                        }
                        UI.AfficherTexte("❌ Mana insuffisant (40 requis) !", ConsoleColor.Red);
                    }
                    break;
            }

            return false;
        }

        // ==============================================================
        // ATTAQUE ULTIME (JAUGE 100%)
        // ==============================================================
        static void ExecuterAttaqueUltime(Monstre ennemi)
        {
            UI.LigneSeparateur('=');
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★");
            Console.WriteLine("               DÉCHAÎNEMENT DE L'ATTAQUE ULTIME !");
            Console.WriteLine("★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★");
            Console.ResetColor();

            int degatsUltime = 0;
            switch (hero.Classe)
            {
                case ClasseType.Guerrier:
                    degatsUltime = (int)(hero.AttaqueTotale * 4.2) + 40;
                    UI.AfficherTexte($"💥 CATACLYSME SISMIQUE DU COLOSSE !", ConsoleColor.Red);
                    UI.AfficherTexte($"Vous levez votre arme vers les cieux et frappez le sol avec la force d'un astéroïde !", ConsoleColor.Yellow);
                    break;

                case ClasseType.Mage:
                    degatsUltime = (int)(hero.AttaqueTotale * 5.0) + 60;
                    UI.AfficherTexte($"🌌 ANNIHILATION COSMIQUE DE SUPRÉMATIE !", ConsoleColor.Cyan);
                    UI.AfficherTexte($"Le tissu de la réalité se fissure ! Une pluie de supernovas s'abat sur {ennemi.Nom} !", ConsoleColor.Magenta);
                    break;

                case ClasseType.Rodeur:
                    degatsUltime = (int)(hero.AttaqueTotale * 4.5) + 50;
                    UI.AfficherTexte($"🏹 FLÈCHE DU DESTIN CÉLESTE !", ConsoleColor.Green);
                    UI.AfficherTexte($"Une volée de cent flèches d'argent pur transperce tous les points vitaux de la créature !", ConsoleColor.Yellow);
                    break;

                case ClasseType.Paladin:
                    degatsUltime = (int)(hero.AttaqueTotale * 4.0) + 40;
                    hero.Soigner(hero.PVMaxTotal); // Soin complet !
                    UI.AfficherTexte($"☀️ AUBE SACRÉE DU SÉRAPHIN !", ConsoleColor.Yellow);
                    UI.AfficherTexte($"Une déflagration de lumière sacrée frappe l'ennemi et RESTAURE TOUS VOS POINTS DE VIE !", ConsoleColor.Green);
                    break;

                case ClasseType.Necromancien:
                    degatsUltime = (int)(hero.AttaqueTotale * 4.3) + 45;
                    hero.Soigner(150);
                    UI.AfficherTexte($"👁️ L'ÉVEIL DU NÉANT ÉTERNEL !", ConsoleColor.DarkMagenta);
                    UI.AfficherTexte($"Des mains spectrales surgissent des abîmes pour déchiqueter l'ennemi et nourrir votre essence vitale (+150 PV) !", ConsoleColor.Magenta);
                    break;
            }

            ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degatsUltime);
            UI.AfficherTexte($"\n🔥 L'Attaque Ultime anéantit la résistance ennemie et inflige {degatsUltime} DÉGÂTS SPECTACULAIRES !", ConsoleColor.Red);
            UI.LigneSeparateur('=');
            Thread.Sleep(1200);
        }

        // ==============================================================
        // UTILISATION DES CONSOMMABLES EN COMBAT
        // ==============================================================
        static bool UtiliserConsommableEnCombat(Monstre ennemi, ref int toursBrulure, ref int buffAtk, ref int buffDef)
        {
            Console.WriteLine("\n--- Choisissez un consommable de votre sac ---");
            List<TypeConsommable> dispo = new List<TypeConsommable>();
            int index = 1;
            foreach (var kvp in hero.InventaireConsommables)
            {
                if (kvp.Value > 0)
                {
                    Console.WriteLine($"{index}. {ConsommableInfo.ObtenirNom(kvp.Key)} x{kvp.Value}");
                    dispo.Add(kvp.Key);
                    index++;
                }
            }

            if (dispo.Count == 0)
            {
                UI.AfficherTexte("❌ Vous n'avez aucun objet consommable dans votre sac !", ConsoleColor.Red);
                return false;
            }

            Console.WriteLine($"{index}. Annuler");
            Console.Write("Votre choix > ");
            string saisie = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(saisie, out int choixIndex) && choixIndex >= 1 && choixIndex <= dispo.Count)
            {
                TypeConsommable type = dispo[choixIndex - 1];
                hero.UtiliserConsommable(type);

                switch (type)
                {
                    case TypeConsommable.PotionSoinMineure:
                        hero.Soigner(60);
                        UI.AfficherTexte("🧪 Glou... Potion de soin bue : +60 PV !", ConsoleColor.Green);
                        break;
                    case TypeConsommable.PotionSoinMajeure:
                        hero.Soigner(160);
                        UI.AfficherTexte("🧪 Glou... Émulsion supérieure bue : +160 PV !", ConsoleColor.Green);
                        break;
                    case TypeConsommable.PotionManaMineure:
                        hero.RestaurerMana(50);
                        UI.AfficherTexte("✨ Potion de mana bue : +50 Mana !", ConsoleColor.Blue);
                        break;
                    case TypeConsommable.PotionManaMajeure:
                        hero.RestaurerMana(120);
                        UI.AfficherTexte("✨ Élixir arcanique bu : +120 Mana !", ConsoleColor.Blue);
                        break;
                    case TypeConsommable.ElixirForce:
                        buffAtk = 4;
                        UI.AfficherTexte("⚔️ Vos muscles se gorgent de puissance : +40% Attaque pour 4 tours !", ConsoleColor.Magenta);
                        break;
                    case TypeConsommable.PeauDePierre:
                        buffDef = 4;
                        UI.AfficherTexte("🪨 Votre peau se durcit comme du granit : Dégâts reçus réduits de 50% pour 4 tours !", ConsoleColor.DarkYellow);
                        break;
                    case TypeConsommable.BombeIncendiaire:
                        int degatsBombe = 80;
                        ennemi.PVActuels = Math.Max(0, ennemi.PVActuels - degatsBombe);
                        toursBrulure = 3;
                        UI.AfficherTexte($"💣 BOOM ! La bombe incendiaire explose sur {ennemi.Nom} pour {degatsBombe} dégâts et le brûle !", ConsoleColor.Red);
                        break;
                    case TypeConsommable.ParcheminTeleport:
                        if (ennemi.EstBoss)
                        {
                            UI.AfficherTexte("⛔ Les sceaux dimensionnels du boss empêchent la téléportation !", ConsoleColor.Red);
                            hero.AjouterConsommable(TypeConsommable.ParcheminTeleport, 1);
                            return false;
                        }
                        hero.PVActuels = Math.Max(1, hero.PVActuels);
                        UI.AfficherTexte("🌀 Le parchemin s'illumine et vous téléporte instantanément au village sain et sauf !", ConsoleColor.Cyan);
                        UI.Pause();
                        BouclePrincipale();
                        return true;
                }
                return true;
            }
            return false;
        }

        // ==============================================================
        // VICTOIRE & GÉNÉRATION DE LOOT DE FOLIE
        // ==============================================================
        static void VictoireCombat(Monstre ennemi)
        {
            UI.LigneSeparateur();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
    ██╗   ██╗██╗ ██████╗████████╗ ██████╗ ██╗██████╗ ███████╗██╗
    ██║   ██║██║██╔════╝╚══██╔══╝██╔═══██╗██║██╔══██╗██╔════╝██║
    ██║   ██║██║██║        ██║   ██║   ██║██║██████╔╝█████╗  ██║
    ╚██╗ ██╔╝██║██║        ██║   ██║   ██║██║██╔══██╗██╔══╝  ╚═╝
     ╚████╔╝ ██║╚██████╗   ██║   ╚██████╔╝██║██║  ██║███████╗██╗
      ╚═══╝  ╚═╝ ╚═════╝   ╚═╝    ╚═════╝ ╚═╝╚═╝  ╚═╝╚══════╝╚═╝
            ");
            Console.ResetColor();

            UI.AfficherTexte($"Victoire écrasante face à {ennemi.Nom} !", ConsoleColor.Green);
            hero.Or += ennemi.GainOr;
            UI.AfficherTexte($"💰 Butin de guerre : +{ennemi.GainOr} pièces d'or !", ConsoleColor.Yellow);
            hero.GagnerXP(ennemi.GainXP);

            // Enregistrer dans le bestiaire
            if (!hero.BestiaireMonstresTues.ContainsKey(ennemi.Nom))
                hero.BestiaireMonstresTues[ennemi.Nom] = 0;
            hero.BestiaireMonstresTues[ennemi.Nom]++;

            if (ennemi.EstBoss) hero.BossVaincusTotal++;

            // Mise à jour des quêtes
            foreach (var q in hero.QuetesActives)
            {
                if (!q.EstTerminee && (q.CibleNom.Equals(ennemi.Nom, StringComparison.OrdinalIgnoreCase) || ennemi.Nom.Contains(q.CibleNom, StringComparison.OrdinalIgnoreCase)))
                {
                    q.Progression++;
                    UI.AfficherTexte($"📜 Quête [{q.Titre}] : {q.Progression}/{q.Objectif} validé !", ConsoleColor.Cyan);
                }
            }

            // Butin de matériaux d'artisanat
            if (rng.Next(100) < 50)
            {
                hero.HerbesMagiques++;
                UI.AfficherTexte("🌿 Vous récoltez 1x Herbe Magique rare sur les lieux du combat.", ConsoleColor.Green);
            }
            if (rng.Next(100) < (ennemi.EstBoss ? 100 : 35))
            {
                hero.EcaillesDeMonstre++;
                UI.AfficherTexte("🐉 Vous prélevez 1x Écaille de Monstre résistante.", ConsoleColor.DarkYellow);
            }
            if (rng.Next(100) < (ennemi.EstBoss ? 80 : 25))
            {
                hero.PierresDeForge++;
                UI.AfficherTexte("💎 Une Pierre de Forge brille au sol ! (+1 Pierre de Forge)", ConsoleColor.Cyan);
            }

            // Génération de Loot d'Équipement RPG
            GenererLootApresCombat(ennemi);

            UI.Pause();
        }

        static void GenererLootApresCombat(Monstre ennemi)
        {
            int chance = rng.Next(100);
            int seuil = ennemi.EstBoss ? 100 : 40; // 100% sur un boss, 40% sur un monstre régulier

            if (chance < seuil)
            {
                Equipement nouveauLoot = CreerEquipementAleatoire(ennemi.EstBoss, ennemi.Nom.Contains("Xanthos"));
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🎁 ✦✦✦ LE MONSTRE A LAISSÉ ÉCHAPPER UN ÉQUIPEMENT D'EXCEPTION ! ✦✦✦");
                Console.ResetColor();

                Console.Write("Objet trouvé : ");
                Console.ForegroundColor = nouveauLoot.ObtenirCouleurRarete();
                Console.WriteLine(nouveauLoot.ObtenirDescription());
                Console.ResetColor();

                Console.WriteLine("Que voulez-vous faire de cet objet ?");
                Console.WriteLine("1. L'équiper immédiatement");
                Console.WriteLine("2. Le ranger dans votre Sac à Dos");
                Console.WriteLine($"3. Le revendre immédiatement ({nouveauLoot.PrixOr / 2} Or)");

                Console.Write("Votre décision > ");
                string r = Console.ReadLine()?.Trim() ?? "";

                if (r == "1")
                {
                    hero.EquiperObjet(nouveauLoot);
                    UI.AfficherTexte($"✅ {nouveauLoot.Nom} a été équipé avec succès !", ConsoleColor.Green);
                }
                else if (r == "2")
                {
                    hero.SacEquipements.Add(nouveauLoot);
                    UI.AfficherTexte($"🎒 {nouveauLoot.Nom} a été glissé dans votre sac.", ConsoleColor.Cyan);
                }
                else
                {
                    int prixVente = nouveauLoot.PrixOr / 2;
                    hero.Or += prixVente;
                    UI.AfficherTexte($"💰 Vendu pour {prixVente} pièces d'or !", ConsoleColor.Yellow);
                }
            }
        }

        static Equipement CreerEquipementAleatoire(bool estBoss, bool estMythiqueGaranti = false)
        {
            Rarete rarete;
            if (estMythiqueGaranti)
            {
                rarete = Rarete.Mythique;
            }
            else if (estBoss)
            {
                int r = rng.Next(100);
                if (r < 40) rarete = Rarete.Epique;
                else if (r < 85) rarete = Rarete.Legendaire;
                else rarete = Rarete.Mythique;
            }
            else
            {
                int r = rng.Next(100);
                if (r < 55) rarete = Rarete.Commun;
                else if (r < 85) rarete = Rarete.Rare;
                else if (r < 97) rarete = Rarete.Epique;
                else rarete = Rarete.Legendaire;
            }

            TypeEquipement type = (TypeEquipement)rng.Next(5);
            int facteur = ((int)rarete + 1);

            string[] prefixes = { "Antique", "Ardent", "Céleste", "Maudit", "Runique", "Spectral", "Titanesque", "Draconique" };
            string[] suffixes = { "du Héros", "de l'Aube", "du Dragon", "du Néant", "des Anciens", "de Puissance", "de Vérité" };

            string pref = prefixes[rng.Next(prefixes.Length)];
            string suff = suffixes[rng.Next(suffixes.Length)];

            int atk = 0, def = 0, pv = 0, mana = 0, crit = 0, esq = 0, vamp = 0;
            string nomBase = "";

            switch (type)
            {
                case TypeEquipement.Arme:
                    string[] armes = { "Lame", "Hache", "Glaive", "Faux", "Bâton", "Arc Long", "Espadon" };
                    nomBase = $"{armes[rng.Next(armes.Length)]} {pref} {suff}";
                    atk = 8 + (facteur * 9) + rng.Next(1, 6);
                    if (facteur >= 2) crit = 4 + (facteur * 3);
                    if (facteur >= 4) vamp = 4 + (facteur * 2);
                    break;

                case TypeEquipement.Armure:
                    string[] armures = { "Plastron", "Cuirasse", "Robe", "Cotte de mailles", "Armure de plates" };
                    nomBase = $"{armures[rng.Next(armures.Length)]} {pref} {suff}";
                    def = 6 + (facteur * 7) + rng.Next(1, 5);
                    pv = facteur * 25;
                    if (facteur >= 2) mana = facteur * 15;
                    break;

                case TypeEquipement.Casque:
                    string[] casques = { "Heaume", "Diadème", "Couronne", "Masque", "Capuche" };
                    nomBase = $"{casques[rng.Next(casques.Length)]} {pref} {suff}";
                    def = 4 + (facteur * 4);
                    pv = facteur * 18;
                    mana = facteur * 12;
                    if (facteur >= 3) crit = 3 + facteur;
                    break;

                case TypeEquipement.Anneau:
                    nomBase = $"Anneau {pref} {suff}";
                    atk = facteur * 4;
                    def = facteur * 3;
                    crit = 2 + (facteur * 2);
                    esq = 2 + (facteur * 2);
                    if (facteur >= 4) vamp = 5;
                    break;

                case TypeEquipement.Amulette:
                    nomBase = $"Pendentif {pref} {suff}";
                    pv = facteur * 30;
                    mana = facteur * 25;
                    crit = 3 + facteur;
                    esq = 3 + facteur;
                    break;
            }

            int prix = facteur * 60 + rng.Next(10, 50);
            return new Equipement(nomBase, type, rarete, atk, def, pv, mana, crit, esq, vamp, prix);
        }

        static void OuvrirCoffreAuTresor(bool estRare)
        {
            hero.CoffresTresorOuverts++;
            int orGagne = estRare ? rng.Next(150, 400) : rng.Next(50, 150);
            hero.Or += orGagne;
            UI.AfficherTexte($"💰 Le coffre contenait {orGagne} pièces d'or !", ConsoleColor.Yellow);

            if (rng.Next(100) < 60)
            {
                hero.PierresDeForge += estRare ? 2 : 1;
                UI.AfficherTexte("💎 Une Pierre de Forge était dissimulée au fond du coffre !", ConsoleColor.Cyan);
            }

            Equipement loot = CreerEquipementAleatoire(estRare);
            hero.SacEquipements.Add(loot);
            Console.Write("🎁 Vous trouvez également : ");
            Console.ForegroundColor = loot.ObtenirCouleurRarete();
            Console.WriteLine(loot.ObtenirDescription());
            Console.ResetColor();
        }

        static void DefaiteCombat(Monstre ennemi)
        {
            UI.LigneSeparateur();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(@"
    ██████╗  █████╗ ███╗   ███╗███████╗     ██████╗ ██╗   ██╗███████╗██████╗ 
   ██╔════╝ ██╔══██╗████╗ ████║██╔════╝    ██╔═══██╗██║   ██║██╔════╝██╔══██╗
   ██║  ███╗███████║██╔████╔██║█████╗      ██║   ██║██║   ██║█████╗  ██████╔╝
   ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝      ██║   ██║╚██╗ ██╔╝██╔══╝  ██╔══██╗
   ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗    ╚██████╔╝ ╚████╔╝ ███████╗██║  ██║
    ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝     ╚═════╝   ╚═══╝  ╚══════╝╚═╝  ╚═╝
            ");
            Console.ResetColor();

            UI.AfficherTexte($"Vous vous effondrez face à {ennemi.Nom}...", ConsoleColor.Red);
            int orPerdu = hero.Or / 4;
            hero.Or -= orPerdu;
            hero.PVActuels = Math.Max(1, hero.PVMaxTotal / 2);
            hero.ManaActuel = Math.Max(1, hero.ManaMaxTotal / 2);
            hero.JaugeUltime = 0;

            UI.AfficherTexte($"Des marchands bienveillants vous ramènent à l'auberge de Val-Serein.", ConsoleColor.Gray);
            UI.AfficherTexte($"Vous perdez {orPerdu} Or pour les soins, mais gardez tout votre équipement !", ConsoleColor.Yellow);
            UI.Pause();
        }

        // ==============================================================
        // GUILDE DES AVENTURIERS
        // ==============================================================
        static void MenuGuilde()
        {
            while (true)
            {
                UI.EffacerConsole();
                UI.AfficherTitre("GUILDE DES AVENTURIERS DE VAL-SEREIN");
                Console.WriteLine("Sire Galahad, Maître de Guilde, examine les avis de primes sur le tableau.\n");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("--- VOS QUÊTES EN COURS ---");
                Console.ResetColor();

                if (hero.QuetesActives.Count == 0)
                {
                    Console.WriteLine("Aucune quête active pour le moment.");
                }
                else
                {
                    foreach (var q in hero.QuetesActives)
                    {
                        string etat = q.EstTerminee ? "★ [TERMINÉE - PRÊTE À VALIDER !]" : $"[{q.Progression}/{q.Objectif}]";
                        ConsoleColor coul = q.EstTerminee ? ConsoleColor.Green : ConsoleColor.White;
                        UI.AfficherTexte($"• #{q.Id} {q.Titre} {etat}", coul);
                        Console.WriteLine($"   Objectif : {q.Description}");
                        Console.WriteLine($"   Récompenses : {q.RecompenseOr} Or | {q.RecompenseXP} XP | {q.RecompensePierresForge} Pierres" + (q.RecompenseItem != null ? $" | {q.RecompenseItem.Nom}" : ""));
                    }
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("--- CONTRATS DISPONIBLES AU TABLEAU ---");
                Console.ResetColor();

                var quetesDispos = quetesDisponibles.FindAll(q => !hero.QuetesActives.Exists(aq => aq.Id == q.Id) && !hero.QuetesCompleteesIds.Contains(q.Id));

                if (quetesDispos.Count == 0)
                {
                    Console.WriteLine("Toutes les quêtes ont été acceptées ou accomplies !");
                }
                else
                {
                    foreach (var q in quetesDispos)
                    {
                        Console.WriteLine($"[{q.Id}] {q.Titre}");
                        Console.WriteLine($"    {q.Description}");
                        Console.WriteLine($"    Gain : {q.RecompenseOr} Or | {q.RecompenseXP} XP | {q.RecompensePierresForge} Pierres" + (q.RecompenseItem != null ? $" | Objet : {q.RecompenseItem.Nom}" : ""));
                    }
                }

                UI.LigneSeparateur();
                Console.WriteLine("Actions :");
                Console.WriteLine("1. Accepter une quête (Entrez son numéro)");
                Console.WriteLine("2. Valider toutes les quêtes terminées");
                Console.WriteLine("3. Entraînement militaire avec Sire Galahad (+1 point de stat pour 200 Or)");
                Console.WriteLine("4. Retourner au village");

                Console.Write("\nVotre choix > ");
                string choix = Console.ReadLine()?.Trim() ?? "";

                if (choix == "2")
                {
                    ValiderQuetes();
                }
                else if (choix == "3")
                {
                    EntrainerHero();
                }
                else if (choix == "4")
                {
                    break;
                }
                else if (int.TryParse(choix, out int idQ))
                {
                    var queteChoisie = quetesDispos.Find(q => q.Id == idQ);
                    if (queteChoisie != null)
                    {
                        hero.QuetesActives.Add(queteChoisie);
                        UI.AfficherTexte($"✅ Quête acceptée : \"{queteChoisie.Titre}\" !", ConsoleColor.Green);
                        UI.Pause();
                    }
                    else
                    {
                        UI.AfficherTexte("Numéro de quête non valide.", ConsoleColor.Red);
                        Thread.Sleep(700);
                    }
                }
            }
        }

        static void EntrainerHero()
        {
            int prixEntrainement = 200;
            if (hero.Or >= prixEntrainement)
            {
                hero.Or -= prixEntrainement;
                hero.PointsCaracteristiques++;
                UI.AfficherTexte($"\n⚔️ Sire Galahad vous enseigne des techniques de maître d'armes !", ConsoleColor.Green);
                UI.AfficherTexte($"Vous gagnez +1 Point d'Attribut à investir ! (Or restant : {hero.Or})", ConsoleColor.Cyan);
            }
            else
            {
                UI.AfficherTexte($"\n❌ Sire Galahad requiert {prixEntrainement} Or pour son entraînement. Vous n'avez pas assez.", ConsoleColor.Red);
            }
            UI.Pause();
        }

        static void ValiderQuetes()
        {
            var quetesFinies = hero.QuetesActives.FindAll(q => q.EstTerminee);
            if (quetesFinies.Count == 0)
            {
                UI.AfficherTexte("Aucune quête achevée à valider.", ConsoleColor.Yellow);
                UI.Pause();
                return;
            }

            foreach (var q in quetesFinies)
            {
                q.RecompenseReclamee = true;
                hero.QuetesActives.Remove(q);
                hero.QuetesCompleteesIds.Add(q.Id);

                UI.AfficherTexte($"\n🏆 QUÊTE VALIDÉE : {q.Titre} !", ConsoleColor.Green);
                hero.Or += q.RecompenseOr;
                hero.PierresDeForge += q.RecompensePierresForge;
                UI.AfficherTexte($"💰 +{q.RecompenseOr} Or reçu !", ConsoleColor.Yellow);
                UI.AfficherTexte($"💎 +{q.RecompensePierresForge} Pierres de Forge reçues !", ConsoleColor.Cyan);
                hero.GagnerXP(q.RecompenseXP);

                if (q.RecompenseItem != null)
                {
                    hero.SacEquipements.Add(q.RecompenseItem);
                    Console.Write("🎁 Équipement de récompense ajouté à votre sac : ");
                    Console.ForegroundColor = q.RecompenseItem.ObtenirCouleurRarete();
                    Console.WriteLine(q.RecompenseItem.ObtenirDescription());
                    Console.ResetColor();
                }
            }
            UI.Pause();
        }

        // ==============================================================
        // LA FORGE DE BROM (AMÉLIORATION +1 À +10, RECYCLAGE & BOUTIQUE)
        // ==============================================================
        static void MenuForge()
        {
            bool dansForge = true;
            while (dansForge)
            {
                UI.EffacerConsole();
                UI.AfficherTitre("LA GRANDE FORGE DE BROM");
                Console.WriteLine("Brom frappe son enclume au milieu des étincelles fumantes.");
                Console.WriteLine($"Vos richesses : {hero.Or} Or  |  {hero.PierresDeForge} Pierres de Forge\n");

                Console.WriteLine("1. 🔨 Améliorer une pièce d'équipement (+1 jusqu'à +10)");
                Console.WriteLine("2. ♻️  Recycler un équipement inutile (Obtenir Pierres de Forge & Or)");
                Console.WriteLine("3. ⚔️  Acheter des armes et armures forgées par Brom");
                Console.WriteLine("4. 💎 Acheter des Pierres de Forge (75 Or l'unité)");
                Console.WriteLine("5. ↩️  Retourner au village");

                Console.Write("\nVotre choix > ");
                string choix = Console.ReadLine()?.Trim() ?? "";

                switch (choix)
                {
                    case "1": AmeliorerEquipement(); break;
                    case "2": RecyclerEquipement(); break;
                    case "3": BoutiqueEquipementForge(); break;
                    case "4":
                        if (hero.Or >= 75)
                        {
                            hero.Or -= 75;
                            hero.PierresDeForge++;
                            UI.AfficherTexte("✅ 1x Pierre de Forge achetée !", ConsoleColor.Green);
                        }
                        else UI.AfficherTexte("❌ Or insuffisant.", ConsoleColor.Red);
                        Thread.Sleep(700);
                        break;
                    case "5": dansForge = false; break;
                }
            }
        }

        static void AmeliorerEquipement()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("ATELIER D'AMÉLIORATION D'ÉQUIPEMENT");
            Console.WriteLine("Choisissez la pièce que vous souhaitez améliorer (+3 Atk, +2 Def, +8 PV par niveau) :\n");

            List<Equipement> equipementsAmeliorables = new List<Equipement>();
            if (hero.ArmeEquipee != null) equipementsAmeliorables.Add(hero.ArmeEquipee);
            if (hero.ArmureEquipee != null) equipementsAmeliorables.Add(hero.ArmureEquipee);
            if (hero.CasqueEquipe != null) equipementsAmeliorables.Add(hero.CasqueEquipe);

            for (int i = 0; i < equipementsAmeliorables.Count; i++)
            {
                var eq = equipementsAmeliorables[i];
                int coutPierres = (eq.NiveauAmelioration + 1);
                int coutOr = (eq.NiveauAmelioration + 1) * 60;
                Console.WriteLine($"{i + 1}. {eq.ObtenirDescription()} -> Coût : {coutPierres} Pierres & {coutOr} Or");
            }

            Console.WriteLine($"{equipementsAmeliorables.Count + 1}. Annuler");
            Console.Write("\nVotre choix > ");
            string choix = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(choix, out int index) && index >= 1 && index <= equipementsAmeliorables.Count)
            {
                var eqChoisi = equipementsAmeliorables[index - 1];
                if (eqChoisi.NiveauAmelioration >= 10)
                {
                    UI.AfficherTexte("❌ Cette pièce a déjà atteint son niveau d'amélioration maximal (+10) !", ConsoleColor.Red);
                    UI.Pause();
                    return;
                }

                int coutPierres = (eqChoisi.NiveauAmelioration + 1);
                int coutOr = (eqChoisi.NiveauAmelioration + 1) * 60;

                if (hero.PierresDeForge >= coutPierres && hero.Or >= coutOr)
                {
                    hero.PierresDeForge -= coutPierres;
                    hero.Or -= coutOr;
                    eqChoisi.NiveauAmelioration++;
                    UI.AfficherTexte($"\n✨ SUCCÈS ! Brom martèle la pièce et la transcende en : {eqChoisi.ObtenirDescription()} !", ConsoleColor.Green);
                }
                else
                {
                    UI.AfficherTexte($"❌ Ressources insuffisantes ! Nécessite {coutPierres} Pierres et {coutOr} Or.", ConsoleColor.Red);
                }
                UI.Pause();
            }
        }

        static void RecyclerEquipement()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("FOURNEAU DE RECYCLAGE");

            if (hero.SacEquipements.Count == 0)
            {
                UI.AfficherTexte("Votre sac ne contient aucun équipement recyclable.", ConsoleColor.Yellow);
                UI.Pause();
                return;
            }

            Console.WriteLine("Choisissez un équipement à démonter en matériaux bruts :\n");
            for (int i = 0; i < hero.SacEquipements.Count; i++)
            {
                var item = hero.SacEquipements[i];
                Console.WriteLine($"{i + 1}. {item.ObtenirDescription()}");
            }
            Console.WriteLine($"{hero.SacEquipements.Count + 1}. Tout recycler d'un coup !");
            Console.WriteLine($"{hero.SacEquipements.Count + 2}. Annuler");

            Console.Write("\nVotre choix > ");
            string choix = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(choix, out int c))
            {
                if (c >= 1 && c <= hero.SacEquipements.Count)
                {
                    var item = hero.SacEquipements[c - 1];
                    hero.SacEquipements.RemoveAt(c - 1);
                    int gainOr = item.PrixOr / 3;
                    int gainPierres = ((int)item.RareteItem >= 2) ? 2 : 1;
                    hero.Or += gainOr;
                    hero.PierresDeForge += gainPierres;
                    UI.AfficherTexte($"✅ {item.Nom} recyclé : +{gainOr} Or et +{gainPierres} Pierre(s) de Forge !", ConsoleColor.Green);
                    UI.Pause();
                }
                else if (c == hero.SacEquipements.Count + 1)
                {
                    int totalOr = 0;
                    int totalPierres = 0;
                    foreach (var item in hero.SacEquipements)
                    {
                        totalOr += item.PrixOr / 3;
                        totalPierres += ((int)item.RareteItem >= 2) ? 2 : 1;
                    }
                    hero.SacEquipements.Clear();
                    hero.Or += totalOr;
                    hero.PierresDeForge += totalPierres;
                    UI.AfficherTexte($"✅ Tout le sac a été recyclé : +{totalOr} Or et +{totalPierres} Pierres de Forge !", ConsoleColor.Green);
                    UI.Pause();
                }
            }
        }

        static void BoutiqueEquipementForge()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("VITRINE DES CHEFS-D'ŒUVRE DE BROM");
            Console.WriteLine("1. ⚔️  Glaive de Chevalier d'Élite (+20 Atk, +6% Crit)        : 220 Or");
            Console.WriteLine("2. 🛡️  Cuirasse en Mithril des Monts (+18 Def, +40 PV)        : 260 Or");
            Console.WriteLine("3. 👑 Heaume d'Obsidienne Renforcé (+12 Def, +35 PV)          : 190 Or");
            Console.WriteLine("4. 💍 Anneau Solaire du Faucon (+8 Atk, +8% Crit, +6% Esq)    : 340 Or");
            Console.WriteLine("5. 🌟 Lame Runique Flamboyante (+35 Atk, +15% Crit, +8% Vamp) : 650 Or");
            Console.WriteLine("6. ↩️  Retour");

            Console.Write("\nVotre choix > ");
            string c = Console.ReadLine()?.Trim() ?? "";

            switch (c)
            {
                case "1": AcheterEquipementForge(new Equipement("Glaive de Chevalier d'Élite", TypeEquipement.Arme, Rarete.Rare, 20, 0, 0, 0, 6, 0, 0, 220)); break;
                case "2": AcheterEquipementForge(new Equipement("Cuirasse en Mithril des Monts", TypeEquipement.Armure, Rarete.Rare, 0, 18, 40, 0, 0, 0, 0, 260)); break;
                case "3": AcheterEquipementForge(new Equipement("Heaume d'Obsidienne Renforcé", TypeEquipement.Casque, Rarete.Rare, 0, 12, 35, 0, 0, 0, 0, 190)); break;
                case "4": AcheterEquipementForge(new Equipement("Anneau Solaire du Faucon", TypeEquipement.Anneau, Rarete.Epique, 8, 4, 20, 15, 8, 6, 0, 340)); break;
                case "5": AcheterEquipementForge(new Equipement("Lame Runique Flamboyante", TypeEquipement.Arme, Rarete.Legendaire, 35, 6, 0, 0, 15, 0, 8, 650)); break;
            }
        }

        static void AcheterEquipementForge(Equipement eq)
        {
            if (hero.Or >= eq.PrixOr)
            {
                hero.Or -= eq.PrixOr;
                hero.SacEquipements.Add(eq);
                UI.AfficherTexte($"✅ {eq.Nom} acheté et rangé dans votre sac !", ConsoleColor.Green);
            }
            else
            {
                UI.AfficherTexte($"❌ Or insuffisant ({eq.PrixOr} requis) !", ConsoleColor.Red);
            }
            UI.Pause();
        }

        // ==============================================================
        // LABORATOIRE ALCHIMIQUE DE DAME ELOWEN
        // ==============================================================
        static void MenuAlchimie()
        {
            bool dansAlchimie = true;
            while (dansAlchimie)
            {
                UI.EffacerConsole();
                UI.AfficherTitre("LABORATOIRE ALCHIMIQUE DE DAME ELOWEN");
                Console.WriteLine("Des fioles bouillonnantes dégagent d'enivrantes vapeurs violettes.");
                Console.WriteLine($"Vos ingrédients : {hero.HerbesMagiques} Herbes  |  {hero.EcaillesDeMonstre} Écailles  |  {hero.Or} Or\n");

                Console.WriteLine("--- CHAUDRON DE TRANSMUTATION (CRAFT) ---");
                Console.WriteLine("1. 🌿 Concocter Potion de Soin Majeure (2 Herbes + 15 Or)");
                Console.WriteLine("2. 🔮 Concocter Élixir d'Arcane Majeur (2 Herbes + 15 Or)");
                Console.WriteLine("3. 🪨 Concocter Flacon de Peau de Pierre (1 Écaille + 1 Herbe + 25 Or)");
                Console.WriteLine("4. 💣 Fabriquer Bombe Incendiaire (2 Écailles + 25 Or)");
                Console.WriteLine("--- ACHAT DIRECT AU COMPTOIR ---");
                Console.WriteLine("5. 🧪 Acheter Potion de Soin Mineure (25 Or)");
                Console.WriteLine("6. 🧪 Acheter Potion de Mana Mineure (20 Or)");
                Console.WriteLine("7. 📜 Acheter Parchemin de Repli Céleste (90 Or)");
                Console.WriteLine("8. ↩️  Retourner au village");

                Console.Write("\nVotre choix > ");
                string choix = Console.ReadLine()?.Trim() ?? "";

                switch (choix)
                {
                    case "1":
                        if (hero.HerbesMagiques >= 2 && hero.Or >= 15)
                        {
                            hero.HerbesMagiques -= 2; hero.Or -= 15;
                            hero.AjouterConsommable(TypeConsommable.PotionSoinMajeure);
                            UI.AfficherTexte("✅ Potion de Soin Majeure concoctée !", ConsoleColor.Green);
                        }
                        else UI.AfficherTexte("❌ Ingrédients ou or insuffisants !", ConsoleColor.Red);
                        Thread.Sleep(800);
                        break;

                    case "2":
                        if (hero.HerbesMagiques >= 2 && hero.Or >= 15)
                        {
                            hero.HerbesMagiques -= 2; hero.Or -= 15;
                            hero.AjouterConsommable(TypeConsommable.PotionManaMajeure);
                            UI.AfficherTexte("✅ Élixir d'Arcane Majeur concocté !", ConsoleColor.Green);
                        }
                        else UI.AfficherTexte("❌ Ingrédients ou or insuffisants !", ConsoleColor.Red);
                        Thread.Sleep(800);
                        break;

                    case "3":
                        if (hero.EcaillesDeMonstre >= 1 && hero.HerbesMagiques >= 1 && hero.Or >= 25)
                        {
                            hero.EcaillesDeMonstre -= 1; hero.HerbesMagiques -= 1; hero.Or -= 25;
                            hero.AjouterConsommable(TypeConsommable.PeauDePierre);
                            UI.AfficherTexte("✅ Flacon de Peau de Pierre concocté !", ConsoleColor.Green);
                        }
                        else UI.AfficherTexte("❌ Ingrédients ou or insuffisants !", ConsoleColor.Red);
                        Thread.Sleep(800);
                        break;

                    case "4":
                        if (hero.EcaillesDeMonstre >= 2 && hero.Or >= 25)
                        {
                            hero.EcaillesDeMonstre -= 2; hero.Or -= 25;
                            hero.AjouterConsommable(TypeConsommable.BombeIncendiaire);
                            UI.AfficherTexte("✅ Bombe Incendiaire assemblée !", ConsoleColor.Green);
                        }
                        else UI.AfficherTexte("❌ Écailles ou or insuffisants !", ConsoleColor.Red);
                        Thread.Sleep(800);
                        break;

                    case "5":
                        AcheterConsommable(TypeConsommable.PotionSoinMineure, 25);
                        break;
                    case "6":
                        AcheterConsommable(TypeConsommable.PotionManaMineure, 20);
                        break;
                    case "7":
                        AcheterConsommable(TypeConsommable.ParcheminTeleport, 90);
                        break;
                    case "8":
                        dansAlchimie = false;
                        break;
                }
            }
        }

        static void AcheterConsommable(TypeConsommable type, int prix)
        {
            if (hero.Or >= prix)
            {
                hero.Or -= prix;
                hero.AjouterConsommable(type);
                UI.AfficherTexte($"✅ 1x {ConsommableInfo.ObtenirNom(type)} acheté !", ConsoleColor.Green);
            }
            else
            {
                UI.AfficherTexte("❌ Or insuffisant !", ConsoleColor.Red);
            }
            Thread.Sleep(700);
        }

        // ==============================================================
        // AUBERGE DU SANGLIER DORÉ & MINI-JEU DE TRIPOT
        // ==============================================================
        static void MenuAuberge()
        {
            bool dansAuberge = true;
            while (dansAuberge)
            {
                UI.EffacerConsole();
                UI.AfficherTitre("AUBERGE DU SANGLIER DORÉ");
                Console.WriteLine("Une odeur de pain chaud et de ragoût flotte près de la cheminée.");
                Console.WriteLine("Finnick le Barde accorde son luth tandis que des parieurs s'agitent autour d'une table.\n");

                Console.WriteLine("1. 🛏️  Louer une chambre royale (20 Or) -> Restaure PV/Mana + BUFF +25% XP pour 3 combats");
                Console.WriteLine("2. 🎲  Le Tripot : Jouer aux 3 Dés d'Or contre le Parieur");
                Console.WriteLine("3. 🎁  Ezekiel le Mystérieux : Acheter un Coffre au Trésor Inconnu (150 Or)");
                Console.WriteLine("4. ↩️  Retourner sur la place du village");

                Console.Write("\nVotre choix > ");
                string choix = Console.ReadLine()?.Trim() ?? "";

                switch (choix)
                {
                    case "1":
                        if (hero.Or >= 20)
                        {
                            hero.Or -= 20;
                            hero.PVActuels = hero.PVMaxTotal;
                            hero.ManaActuel = hero.ManaMaxTotal;
                            hero.BuffReposCombatsRestants = 3;
                            UI.AfficherTexte("\n💤 Vous passez une nuit splendide dans des draps de velours.", ConsoleColor.Cyan);
                            UI.AfficherTexte("✨ Santé & Mana régénérés au maximum !", ConsoleColor.Green);
                            UI.AfficherTexte("⭐ BUFF ACTIF : +25% d'XP reçue lors de vos 3 prochains combats !", ConsoleColor.Yellow);
                        }
                        else UI.AfficherTexte("\n❌ Or insuffisant pour payer la chambre !", ConsoleColor.Red);
                        UI.Pause();
                        break;

                    case "2":
                        MiniJeuDesTripot();
                        break;

                    case "3":
                        AcheterCoffreMystere();
                        break;

                    case "4":
                        dansAuberge = false;
                        break;
                }
            }
        }

        static void MiniJeuDesTripot()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("LE JEU DES 3 DÉS D'OR");
            Console.WriteLine("RÈGLES DU JEU :");
            Console.WriteLine("Vous et le Parieur lancez chacun 3 dés à 6 faces (résultats entre 3 et 18).");
            Console.WriteLine("Le total le plus élevé remporte la mise doublée ! En cas d'égalité, mise remboursée.\n");

            Console.WriteLine($"Vous possédez {hero.Or} pièces d'or.");
            Console.Write("Combien voulez-vous miser ? (0 pour annuler) > ");
            string saisie = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(saisie, out int mise) && mise > 0)
            {
                if (mise > hero.Or)
                {
                    UI.AfficherTexte("❌ Vous ne pouvez pas miser plus d'or que vous n'en possédez !", ConsoleColor.Red);
                    UI.Pause();
                    return;
                }

                hero.Or -= mise;
                UI.AfficherTexte("\nLes dés roulent sur le tapis de velours...", ConsoleColor.DarkYellow);
                Thread.Sleep(900);

                int deJoueur1 = rng.Next(1, 7), deJoueur2 = rng.Next(1, 7), deJoueur3 = rng.Next(1, 7);
                int totalJoueur = deJoueur1 + deJoueur2 + deJoueur3;

                int deAdv1 = rng.Next(1, 7), deAdv2 = rng.Next(1, 7), deAdv3 = rng.Next(1, 7);
                int totalAdv = deAdv1 + deAdv2 + deAdv3;

                Console.WriteLine($"🎲 Vos dés    : [{deJoueur1}] + [{deJoueur2}] + [{deJoueur3}] = {totalJoueur}");
                Console.WriteLine($"🎲 L'Adversaire: [{deAdv1}] + [{deAdv2}] + [{deAdv3}] = {totalAdv}\n");

                if (totalJoueur > totalAdv)
                {
                    int gain = mise * 2;
                    hero.Or += gain;
                    UI.AfficherTexte($"🎉 VICTOIRE ! Vous remportez {gain} pièces d'or !", ConsoleColor.Green);
                }
                else if (totalJoueur < totalAdv)
                {
                    UI.AfficherTexte($"💀 DÉFAITE ! L'adversaire empoche votre mise de {mise} pièces d'or...", ConsoleColor.Red);
                }
                else
                {
                    hero.Or += mise;
                    UI.AfficherTexte("⚖️ Égalité parfaite ! Vous récupérez votre mise.", ConsoleColor.Cyan);
                }
                UI.Pause();
            }
        }

        static void AcheterCoffreMystere()
        {
            if (hero.Or >= 150)
            {
                hero.Or -= 150;
                UI.AfficherTexte("\n🎁 Ezekiel sort une malle gravée de runes arcaniques...", ConsoleColor.Magenta);
                Thread.Sleep(800);
                OuvrirCoffreAuTresor(true);
            }
            else
            {
                UI.AfficherTexte("\n❌ Ezekiel ricane : \"Reviens quand tu auras 150 pièces d'or, voyageur.\"", ConsoleColor.Red);
            }
            UI.Pause();
        }

        // ==============================================================
        // FICHE DU HÉROS, INVENTAIRE & ATTRIBUTS
        // ==============================================================
        static void MenuFicheEtInventaire()
        {
            bool continuer = true;
            while (continuer)
            {
                UI.EffacerConsole();
                UI.AfficherTitre($"FICHE DE PERSONNAGE : {hero.Nom.ToUpper()}");
                Console.WriteLine($"Classe        : {hero.Classe}  |  Niveau : {hero.Niveau}  |  XP : {hero.XP}/{hero.XPRequisPourNiveau}");
                Console.WriteLine($"Points de Vie : {hero.PVActuels}/{hero.PVMaxTotal}  |  Mana : {hero.ManaActuel}/{hero.ManaMaxTotal}");
                Console.WriteLine($"Attaque       : {hero.AttaqueTotale}  |  Défense : {hero.DefenseTotale}");
                Console.WriteLine($"Taux Critique : {hero.ChanceCritiqueTotale}%  |  Esquive : {hero.ChanceEsquiveTotale}%  |  Vol de vie : {hero.VampirismeTotal}%");
                UI.LigneSeparateur();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"ATTRIBUTS (Points disponibles : {hero.PointsCaracteristiques}) :");
                Console.ResetColor();
                Console.WriteLine($"• [1] Force        : {hero.Force} (Attaque physique)");
                Console.WriteLine($"• [2] Agilité      : {hero.Agilite} (Chance de Coup Critique & Esquive)");
                Console.WriteLine($"• [3] Endurance    : {hero.Endurance} (PV Max & Défense)");
                Console.WriteLine($"• [4] Intelligence : {hero.Intelligence} (Mana Max & Puissance des sorts)");
                UI.LigneSeparateur();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("ÉQUIPEMENT ACTUEL :");
                Console.ResetColor();
                AfficherLigneEquipement("Arme    ", hero.ArmeEquipee);
                AfficherLigneEquipement("Armure  ", hero.ArmureEquipee);
                AfficherLigneEquipement("Casque  ", hero.CasqueEquipe);
                AfficherLigneEquipement("Anneau  ", hero.AnneauEquipe);
                AfficherLigneEquipement("Amulette", hero.AmuletteEquipee);
                UI.LigneSeparateur();

                Console.WriteLine("Options :");
                Console.WriteLine("1. ➕ Répartir des points d'attributs");
                Console.WriteLine("2. 🎒 Ouvrir le Sac à Dos d'équipement");
                Console.WriteLine("3. 🧪 Voir l'inventaire de consommables");
                Console.WriteLine("4. ↩️  Retourner au village");

                Console.Write("\nVotre choix > ");
                string choix = Console.ReadLine()?.Trim() ?? "";

                switch (choix)
                {
                    case "1": RepartirAttributs(); break;
                    case "2": GererSacEquipements(); break;
                    case "3": AfficherConsommables(); break;
                    case "4": continuer = false; break;
                }
            }
        }

        static void AfficherLigneEquipement(string slot, Equipement? eq)
        {
            Console.Write($"• {slot} : ");
            if (eq != null)
            {
                Console.ForegroundColor = eq.ObtenirCouleurRarete();
                Console.WriteLine(eq.ObtenirDescription());
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Aucun");
            }
        }

        static void RepartirAttributs()
        {
            if (hero.PointsCaracteristiques <= 0)
            {
                UI.AfficherTexte("❌ Vous n'avez aucun point d'attribut disponible à investir !", ConsoleColor.Red);
                UI.Pause();
                return;
            }

            Console.WriteLine($"\nQuel attribut voulez-vous augmenter ? (Reste {hero.PointsCaracteristiques} pts)");
            Console.WriteLine("1. Force (+3 Attaque)");
            Console.WriteLine("2. Agilité (+2% Critique, +1% Esquive)");
            Console.WriteLine("3. Endurance (+8 PV Max, +2 Défense)");
            Console.WriteLine("4. Intelligence (+6 Mana Max)");
            Console.Write("Votre choix (1 à 4) > ");
            string choix = Console.ReadLine()?.Trim() ?? "";

            switch (choix)
            {
                case "1": hero.Force++; hero.PointsCaracteristiques--; UI.AfficherTexte("✅ Force augmentée !", ConsoleColor.Green); break;
                case "2": hero.Agilite++; hero.PointsCaracteristiques--; UI.AfficherTexte("✅ Agilité augmentée !", ConsoleColor.Green); break;
                case "3": hero.Endurance++; hero.PointsCaracteristiques--; UI.AfficherTexte("✅ Endurance augmentée !", ConsoleColor.Green); break;
                case "4": hero.Intelligence++; hero.PointsCaracteristiques--; UI.AfficherTexte("✅ Intelligence augmentée !", ConsoleColor.Green); break;
                default: UI.AfficherTexte("Choix invalide.", ConsoleColor.Red); break;
            }
            Thread.Sleep(700);
        }

        static void GererSacEquipements()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("SAC À DOS D'ÉQUIPEMENT");

            if (hero.SacEquipements.Count == 0)
            {
                Console.WriteLine("Votre sac est vide pour le moment.");
                UI.Pause();
                return;
            }

            for (int i = 0; i < hero.SacEquipements.Count; i++)
            {
                var item = hero.SacEquipements[i];
                Console.Write($"{i + 1}. [{item.Type}] ");
                Console.ForegroundColor = item.ObtenirCouleurRarete();
                Console.WriteLine(item.ObtenirDescription());
                Console.ResetColor();
            }

            Console.WriteLine("0. ⚡ Équiper automatiquement le MEILLEUR STUFF (Dégâts Max)");
            Console.WriteLine($"{hero.SacEquipements.Count + 1}. Annuler");
            Console.Write("\nSélectionnez un équipement à équiper > ");
            string choix = Console.ReadLine()?.Trim() ?? "";

            if (choix == "0")
            {
                var res = hero.AutoEquiperMeilleurStuffDegats();
                if (res.ChangementsCount > 0)
                {
                    UI.AfficherTexte($"\n⚡ {res.ChangementsCount} pièce(s) optimisée(s) pour dégâts max !", ConsoleColor.Yellow);
                    foreach (var d in res.Details) UI.AfficherTexte(d, ConsoleColor.Cyan);
                    UI.AfficherTexte($"⚔️ Attaque Totale : {res.AncienneAtk} ➜ {hero.AttaqueTotale} (+{res.GainAttaque}) | 🎯 Critique : {hero.ChanceCritiqueTotale}%", ConsoleColor.Green);
                }
                else
                {
                    UI.AfficherTexte("\n⚡ Votre équipement actuel est déjà optimal pour les dégâts !", ConsoleColor.Green);
                }
                UI.Pause();
            }
            else if (int.TryParse(choix, out int index) && index >= 1 && index <= hero.SacEquipements.Count)
            {
                var selection = hero.SacEquipements[index - 1];
                hero.EquiperObjet(selection);
                UI.AfficherTexte($"✅ {selection.Nom} équipé !", ConsoleColor.Green);
                UI.Pause();
            }
        }

        static void AfficherConsommables()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("POCHE DE CONSOMMABLES & MATÉRIAUX");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- POTIONS & OBJETS DE COMBAT ---");
            Console.ResetColor();

            bool aucun = true;
            foreach (var kvp in hero.InventaireConsommables)
            {
                if (kvp.Value > 0)
                {
                    Console.WriteLine($"• {ConsommableInfo.ObtenirNom(kvp.Key)} x{kvp.Value}");
                    aucun = false;
                }
            }
            if (aucun) Console.WriteLine("Aucun consommable en réserve.");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- MATÉRIAUX D'ARTISANAT ---");
            Console.ResetColor();
            Console.WriteLine($"• Pierres de Forge    : {hero.PierresDeForge}");
            Console.WriteLine($"• Herbes Magiques     : {hero.HerbesMagiques}");
            Console.WriteLine($"• Écailles de Monstre : {hero.EcaillesDeMonstre}");

            UI.Pause();
        }

        // ==============================================================
        // BESTIAIRE & HAUTS FAITS
        // ==============================================================
        static void MenuBestiaireEtHautsFaits()
        {
            UI.EffacerConsole();
            UI.AfficherTitre("SANCTUAIRE DES SAGES : BESTIAIRE & HAUTS FAITS");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- BESTIAIRE DES CRÉATURES COMBATTUES ---");
            Console.ResetColor();

            if (hero.BestiaireMonstresTues.Count == 0)
            {
                Console.WriteLine("Aucune créature enregistrée pour l'instant.");
            }
            else
            {
                foreach (var entry in hero.BestiaireMonstresTues)
                {
                    Console.WriteLine($"• {entry.Key,-38} : {entry.Value,3} terrassés");
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- TABLEAU DES HAUTS FAITS ---");
            Console.ResetColor();
            AfficherHautFait("Chasseur Novice", "Tuer au moins 5 monstres", hero.BestiaireMonstresTues.Values.Sum() >= 5);
            AfficherHautFait("Tueur de Boss", "Vaincre au moins 3 Boss", hero.BossVaincusTotal >= 3);
            AfficherHautFait("Pillard de Trésors", "Ouvrir au moins 5 Coffres", hero.CoffresTresorOuverts >= 5);
            AfficherHautFait("Pionnier Astral", "Atteindre l'Étage 5 de la Tour", hero.EtageTourAstraleMax >= 5);
            AfficherHautFait("Pourfendeur de Dragon", "Vaincre Ignis le Dragon Millénaire", hero.BestiaireMonstresTues.ContainsKey("Ignis, Dragon Millénaire Suprême"));
            AfficherHautFait("Sauveur Cosmique", "Vaincre Xanthos, Dévoreur du Néant", hero.BestiaireMonstresTues.ContainsKey("Xanthos, Dévoreur Stellaire du Néant"));

            UI.Pause();
        }

        static void AfficherHautFait(string titre, string description, bool debloque)
        {
            if (debloque)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[✓] {titre,-24} : {description} [ACCOMPLI]");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[ ] {titre,-24} : {description} [VERROUILLÉ]");
            }
            Console.ResetColor();
        }

        // ==============================================================
        // SAUVEGARDE & CHARGEMENT
        // ==============================================================
        static void SauvegarderPartie()
        {
            try
            {
                string json = JsonSerializer.Serialize(hero, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FichierSauvegarde, json);
                UI.AfficherTexte($"\n💾 Partie sauvegardée avec succès dans '{FichierSauvegarde}' !", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                UI.AfficherTexte($"\n❌ Erreur de sauvegarde : {ex.Message}", ConsoleColor.Red);
            }
            Thread.Sleep(1000);
        }

        static bool ChargerPartie()
        {
            try
            {
                if (File.Exists(FichierSauvegarde))
                {
                    string json = File.ReadAllText(FichierSauvegarde);
                    hero = JsonSerializer.Deserialize<Joueur>(json)!;
                    InitialiserQuetes();
                    UI.AfficherTexte($"\n✅ Bienvenue à nouveau, {hero.Nom} le {hero.Classe} (Niv. {hero.Niveau}) !", ConsoleColor.Green);
                    UI.Pause();
                    return true;
                }
            }
            catch (Exception ex)
            {
                UI.AfficherTexte($"\n❌ Erreur lors du chargement : {ex.Message}", ConsoleColor.Red);
                UI.Pause();
            }
            return false;
        }
    }
}
