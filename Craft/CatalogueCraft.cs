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
        DrainMana,
        Gel,
        VortexAspiration,
        DomeProtecteur
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

            // --- NOUVEAUX SORTS DU GRIMOIRE DES ARCANES (IMAGES 2 & 3) ---
            Recettes.Add(new RecetteCraft {
                Id = "c_orbe_foudre", Nom = "Grimoire : Orbe de Foudre Arcanique",
                Description = "Sort : 26 Mana | 230% Dégâts | Décharge des arcs électriques paralysants et inflige Étourdissement !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Rare, NiveauRequis = 3, CoutOr = 300,
                MateriauxRequis = new Dictionary<string, int> { { "Acier Trempé", 3 }, { "Ectoplasme", 3 }, { "Minerai de Fer", 5 } },
                CompetenceResultat = new CompetenceCraftable("c_orbe_foudre", "Orbe de Foudre Arcanique", "Sphère électrique projetant des arcs de foudre en chaîne et étourdissant les cibles.", 26, 2.3, TypeEffetCompetence.Etourdissement, 1, "⚡")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_dragon_flammes", Nom = "Grimoire : Dragon de Flammes Primordial",
                Description = "Sort : 35 Mana | 280% Dégâts | Invoque un wyrm de feu ardent crachant des météores et appliquant Brûlure !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Epique, NiveauRequis = 6, CoutOr = 650,
                MateriauxRequis = new Dictionary<string, int> { { "Cœur Ardent", 3 }, { "Écaille Draconique", 4 }, { "Minerai de Fer", 6 } },
                CompetenceResultat = new CompetenceCraftable("c_dragon_flammes", "Dragon de Flammes Primordial", "Grand wyrm igné serpentant en arc et crachant une averse de météores incendiaires.", 35, 2.8, TypeEffetCompetence.Brulure, 4, "🐉")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_trombe_tempete", Nom = "Grimoire : Trombe des Tempêtes",
                Description = "Sort : 30 Mana | 240% Dégâts | Cyclone aspirant les ennemis vers son vortex et projetant des lames de vent !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Rare, NiveauRequis = 4, CoutOr = 400,
                MateriauxRequis = new Dictionary<string, int> { { "Cuir Épais", 4 }, { "Ectoplasme", 4 } },
                CompetenceResultat = new CompetenceCraftable("c_trombe_tempete", "Trombe des Tempêtes", "Cyclone tourbillonnant aspirant tous les monstres vers son épicentre.", 30, 2.4, TypeEffetCompetence.VortexAspiration, 180, "🌪️")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_dome_sacre", Nom = "Grimoire : Dôme Sacré Céleste",
                Description = "Sort : 32 Mana | 180% Dégâts | Dôme doré protecteur repoussant les ennemis et absorbant les dégâts subis !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Epique, NiveauRequis = 5, CoutOr = 500,
                MateriauxRequis = new Dictionary<string, int> { { "Pierre d'Âme", 3 }, { "Acier Trempé", 4 } },
                CompetenceResultat = new CompetenceCraftable("c_dome_sacre", "Dôme Sacré Céleste", "Barrière céleste hémisphérique protégeant le lanceur et blessant les assaillants.", 32, 1.8, TypeEffetCompetence.DomeProtecteur, 60, "🛡️")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_phenix_givre", Nom = "Grimoire : Phénix de Givre Céleste",
                Description = "Sort : 34 Mana | 270% Dégâts | Rapace de glace transperçant toutes les lignes et gelant les monstres touchés !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Epique, NiveauRequis = 6, CoutOr = 600,
                MateriauxRequis = new Dictionary<string, int> { { "Noyau de Givre", 4 }, { "Ectoplasme", 4 } },
                CompetenceResultat = new CompetenceCraftable("c_phenix_givre", "Phénix de Givre Céleste", "Oiseau céleste de givre fendant l'air et congelant les ennemis sur son passage.", 34, 2.7, TypeEffetCompetence.Gel, 3, "🦅")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_portail_givre", Nom = "Grimoire : Portail Glacial de l'Aurore",
                Description = "Sort : 28 Mana | 210% Dégâts | Arche de glace bloquant les monstres et projetant des stalagmites de rime !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Rare, NiveauRequis = 4, CoutOr = 380,
                MateriauxRequis = new Dictionary<string, int> { { "Noyau de Givre", 3 }, { "Morceau de Cuir", 5 } },
                CompetenceResultat = new CompetenceCraftable("c_portail_givre", "Portail Glacial de l'Aurore", "Portail givré gothique ralentissant et criblant de pointes de glace les ennemis.", 28, 2.1, TypeEffetCompetence.Gel, 2, "🧊")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_vortex_ames", Nom = "Grimoire : Vortex des Âmes Éthérées",
                Description = "Sort : 36 Mana | 290% Dégâts | Sceau runique violet avec 4 spectres orbitaux traquant les cibles et empoisonnant !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Epique, NiveauRequis = 7, CoutOr = 750,
                MateriauxRequis = new Dictionary<string, int> { { "Ectoplasme", 6 }, { "Pierre d'Âme", 3 }, { "Venin Obscur", 2 } },
                CompetenceResultat = new CompetenceCraftable("c_vortex_ames", "Vortex des Âmes Éthérées", "Disque occulte libérant 4 esprits tourbillonnants qui infectent de poison l'adversaire.", 36, 2.9, TypeEffetCompetence.Poison, 25, "👻")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_croissant_lunaire", Nom = "Grimoire : Croissant Lunaire Irisé",
                Description = "Sort : 40 Mana | 320% Dégâts | Balayage en croissant prismatique perçant l'armure avec des étoiles étincelantes !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Legendaire, NiveauRequis = 9, CoutOr = 1100,
                MateriauxRequis = new Dictionary<string, int> { { "Éclat Astral", 5 }, { "Pierre d'Âme", 4 }, { "Acier Trempé", 5 } },
                CompetenceResultat = new CompetenceCraftable("c_croissant_lunaire", "Croissant Lunaire Irisé", "Lame céleste en croissant brillant aux reflets de l'arc-en-ciel et ignorant l'armure.", 40, 3.2, TypeEffetCompetence.PerforantArmure, 45, "🌙")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_dragon_tricephale", Nom = "Grimoire : Fureur du Dragon Tricéphale",
                Description = "Sort : 48 Mana | 360% Dégâts | Déchaîne un dragon à 3 têtes tirant une triple salve de météores incendiaires !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Legendaire, NiveauRequis = 11, CoutOr = 1500,
                MateriauxRequis = new Dictionary<string, int> { { "Cœur Ardent", 6 }, { "Écaille Draconique", 8 }, { "Acier Trempé", 6 } },
                CompetenceResultat = new CompetenceCraftable("c_dragon_tricephale", "Fureur du Dragon Tricéphale", "Trois têtes draconiques vomissant simultanément des orbes de flammes primordiales.", 48, 3.6, TypeEffetCompetence.Brulure, 5, "🐲")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_leviathan_glaces", Nom = "Grimoire : Léviathan des Glaces",
                Description = "Sort : 44 Mana | 340% Dégâts | Serpent polaire monumental glaçant toute la zone et pétrifiant les ennemis !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Legendaire, NiveauRequis = 10, CoutOr = 1350,
                MateriauxRequis = new Dictionary<string, int> { { "Noyau de Givre", 6 }, { "Acier Trempé", 6 }, { "Éclat Astral", 3 } },
                CompetenceResultat = new CompetenceCraftable("c_leviathan_glaces", "Léviathan des Glaces", "Grand serpent des mers de glace figeant instantanément les monstres pris dans son blizzard.", 44, 3.4, TypeEffetCompetence.Gel, 4, "❄️")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_miroir_glace", Nom = "Grimoire : Miroir de Glace Mystique",
                Description = "Sort : 30 Mana | 220% Dégâts | Miroir de cristal givré renvoyant les assauts ennemis sous forme de faisceaux polaires !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Rare, NiveauRequis = 5, CoutOr = 480,
                MateriauxRequis = new Dictionary<string, int> { { "Noyau de Givre", 3 }, { "Pierre d'Âme", 3 } },
                CompetenceResultat = new CompetenceCraftable("c_miroir_glace", "Miroir de Glace Mystique", "Miroir orné de runes renvoyant une barrière glaciale et infligeant des dégâts de gel.", 30, 2.2, TypeEffetCompetence.Bouclier, 50, "🪞")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_trou_noir_tellurique", Nom = "Grimoire : Singularité Tellurique",
                Description = "Sort : 50 Mana | 400% Dégâts | Vortex gravitationnel noir écrasant les ennemis sous des rochers titanesques !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Legendaire, NiveauRequis = 12, CoutOr = 1800,
                MateriauxRequis = new Dictionary<string, int> { { "Cœur de Titan", 4 }, { "Matière du Néant", 3 }, { "Éclat Astral", 6 } },
                CompetenceResultat = new CompetenceCraftable("c_trou_noir_tellurique", "Singularité Tellurique", "Singularité gravitationnelle aspirant les monstres et les broyant sous des météores rocheux.", 50, 4.0, TypeEffetCompetence.VortexAspiration, 220, "🕳️")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_prisme_irise", Nom = "Grimoire : Prisme de Cristal Irisé",
                Description = "Sort : 46 Mana | 370% Dégâts | Cristal géant réfractant 5 rayons lasers arc-en-ciel perçant toutes les cibles !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Legendaire, NiveauRequis = 13, CoutOr = 1950,
                MateriauxRequis = new Dictionary<string, int> { { "Cristal Cosmique", 3 }, { "Éclat Astral", 8 }, { "Pierre d'Âme", 5 } },
                CompetenceResultat = new CompetenceCraftable("c_prisme_irise", "Prisme de Cristal Irisé", "Prisme de lumière pure diffusant un éventail de 5 lasers spectraux dévastateurs.", 46, 3.7, TypeEffetCompetence.PerforantArmure, 60, "💎")
            });

            Recettes.Add(new RecetteCraft {
                Id = "c_abysse_eldritch", Nom = "Grimoire : Portail de l'Abysse Eldritch",
                Description = "Sort : 55 Mana | 450% Dégâts | Faille du Vide aux tentacules d'ombre et yeux mystiques étourdissant tous les monstres !",
                Categorie = CategorieCraft.Competence, RareteItem = Rarete.Mythique, NiveauRequis = 16, CoutOr = 4200,
                MateriauxRequis = new Dictionary<string, int> { { "Matière du Néant", 6 }, { "Essence du Chaos", 2 }, { "Cristal Cosmique", 4 } },
                CompetenceResultat = new CompetenceCraftable("c_abysse_eldritch", "Portail de l'Abysse Eldritch", "Sceau abyssal runique libérant des tentacules voraces qui étourdissent et corrompent.", 55, 4.5, TypeEffetCompetence.Etourdissement, 2, "👁️")
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
}
