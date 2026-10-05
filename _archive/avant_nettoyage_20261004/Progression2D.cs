using System;
using System.Collections.Generic;

namespace JeuxRPG
{
    public static class NomSet2D
    {
        public static string ObtenirNom(string identifiant) => identifiant switch
        {
            "set_dragon" => "Embrasement du Dragon",
            "set_astral" => "Sentinelle Astrale",
            _ => "Gardiens d'Aethelgard"
        };
    }

    public static class ButinBoss2D
    {
        public static Equipement GenererPieceSet(string nomBoss, int niveauJoueur, Random rng)
        {
            string nomNormalise = nomBoss.ToLowerInvariant();
            string identifiantSet = nomNormalise.Contains("dragon") || nomNormalise.Contains("ignis") || nomNormalise.Contains("magma")
                ? "set_dragon"
                : nomNormalise.Contains("xanthos") || nomNormalise.Contains("astral") || nomNormalise.Contains("stellaire")
                    ? "set_astral"
                    : "set_aethelgard";
            TypeEquipement type = (TypeEquipement)rng.Next(5);
            int niveau = Math.Clamp(niveauJoueur, 1, 20);
            int facteurNiveau = 1 + (niveau - 1) / 4;
            string nomPiece = type switch
            {
                TypeEquipement.Arme => "Lame",
                TypeEquipement.Armure => "Cuirasse",
                TypeEquipement.Casque => "Heaume",
                TypeEquipement.Anneau => "Anneau",
                _ => "Amulette"
            };
            string nomTheme = identifiantSet switch
            {
                "set_dragon" => "du Dragon",
                "set_astral" => "Astral",
                _ => "d'Aethelgard"
            };

            var piece = new Equipement(
                $"{nomPiece} {nomTheme}",
                type,
                Rarete.Legendaire,
                type == TypeEquipement.Arme ? 14 * facteurNiveau : type == TypeEquipement.Anneau ? 5 * facteurNiveau : 0,
                type == TypeEquipement.Armure ? 12 * facteurNiveau : type == TypeEquipement.Casque ? 7 * facteurNiveau : 2 * facteurNiveau,
                24 * facteurNiveau,
                12 * facteurNiveau,
                type == TypeEquipement.Arme || type == TypeEquipement.Anneau ? 5 : 0,
                type == TypeEquipement.Casque ? 3 : 0,
                type == TypeEquipement.Anneau ? 3 : 0,
                450 * facteurNiveau);
            piece.IdentifiantSet = identifiantSet;
            return piece;
        }
    }

    public static class ContratsSecondaires2D
    {
        public static List<Quete> CreerDisponibles(Joueur joueur)
        {
            var modeles = new[]
            {
                new Quete(5001, "Les Crocs de la Horde", "Éliminez cinq gobelins maraudeurs qui pillent les routes.", "Gobelin Maraudeur", 5, 220, 180, 2, new Equipement("Bague du Veilleur", TypeEquipement.Anneau, Rarete.Rare, 4, 3, 18, 10, 2, 0, 1, 180), 45, 2, "⚔️ Normale", 1),
                new Quete(5002, "Silence dans les Catacombes", "Renvoyez quatre squelettes gardiens dans leurs cryptes.", "Squelette Gardien", 4, 380, 320, 3, new Equipement("Cotte du Veilleur", TypeEquipement.Armure, Rarete.Epique, 0, 14, 55, 0, 0, 2, 0, 420), 70, 3, "💀 Difficile", 3),
                new Quete(5003, "Cendres du Volcan", "Abattez trois drakes de magma pour sécuriser la route des caravanes.", "Drake de Magma", 3, 650, 520, 4, new Equipement("Anneau de Cendre Vive", TypeEquipement.Anneau, Rarete.Epique, 8, 4, 35, 20, 5, 0, 2, 650), 100, 4, "🔥 Élite", 7),
                new Quete(5004, "Le Néant a un Nom", "Vainquez Xanthos du Néant dans le Sanctuaire Cosmique.", "Xanthos du Néant", 1, 1200, 1000, 6, new Equipement("Sceau de la Sentinelle Astrale", TypeEquipement.Amulette, Rarete.Legendaire, 12, 12, 75, 65, 8, 4, 4, 1600) { IdentifiantSet = "set_astral" }, 180, 8, "🌌 Légendaire", 11)
            };

            var disponibles = new List<Quete>();
            foreach (Quete modele in modeles)
            {
                if (joueur.Niveau < modele.NiveauRequis ||
                    joueur.QuetesCompleteesIds.Contains(modele.Id) ||
                    joueur.QuetesActives.Exists(q => q.Id == modele.Id))
                    continue;

                disponibles.Add(new Quete(
                    modele.Id, modele.Titre, modele.Description, modele.CibleNom,
                    modele.Objectif, modele.RecompenseOr, modele.RecompenseXP,
                    modele.RecompensePierresForge, modele.RecompenseItem,
                    modele.RecompenseReputation, modele.RecompenseSceauxGuilde,
                    modele.Difficulte, modele.NiveauRequis));
            }
            return disponibles;
        }
    }
}
