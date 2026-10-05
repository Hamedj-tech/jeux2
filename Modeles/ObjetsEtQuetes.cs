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
}
