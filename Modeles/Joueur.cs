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

            var sortsBonusVisuels = new List<CompetenceCraftable>
            {
                new CompetenceCraftable("c_orbe_foudre", "Orbe de Foudre Arcanique", "Sphère électrique projetant des arcs de foudre en chaîne et étourdissant les cibles.", 26, 2.3, TypeEffetCompetence.Etourdissement, 1, "⚡"),
                new CompetenceCraftable("c_dragon_flammes", "Dragon de Flammes Primordial", "Grand wyrm igné serpentant en arc et crachant une averse de météores incendiaires.", 35, 2.8, TypeEffetCompetence.Brulure, 4, "🐉"),
                new CompetenceCraftable("c_phenix_givre", "Phénix de Givre Céleste", "Oiseau céleste de givre fendant l'air et congelant les ennemis sur son passage.", 34, 2.7, TypeEffetCompetence.Gel, 3, "🦅"),
                new CompetenceCraftable("c_trombe_tempete", "Trombe des Tempêtes", "Cyclone tourbillonnant aspirant tous les monstres vers son épicentre.", 30, 2.4, TypeEffetCompetence.VortexAspiration, 180, "🌪️"),
                new CompetenceCraftable("c_dome_sacre", "Dôme Sacré Céleste", "Barrière céleste hémisphérique protégeant le lanceur et blessant les assaillants.", 32, 1.8, TypeEffetCompetence.DomeProtecteur, 60, "🛡️"),
                new CompetenceCraftable("c_vortex_ames", "Vortex des Âmes Éthérées", "Disque occulte libérant 4 esprits tourbillonnants qui infectent de poison l'adversaire.", 36, 2.9, TypeEffetCompetence.Poison, 25, "👻"),
                new CompetenceCraftable("c_croissant_lunaire", "Croissant Lunaire Irisé", "Lame céleste en croissant brillant aux reflets de l'arc-en-ciel et ignorant l'armure.", 40, 3.2, TypeEffetCompetence.PerforantArmure, 45, "🌙")
            };

            foreach (var sb in sortsBonusVisuels)
            {
                if (!CompetencesDebloquees.Any(c => c.Id == sb.Id))
                    CompetencesDebloquees.Add(sb);
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
            }

            XP += quantite;

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
}
