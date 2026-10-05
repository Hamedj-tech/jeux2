using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JeuxRPG
{
    public partial class FormJeu2D
    {
        private static string FormaterDelta(int delta)
        {
            if (delta > 0) return $"+{delta} 🟢 (GAIN)";
            if (delta < 0) return $"{delta} 🔴 (PERTE)";
            return "= ⚪ (ÉGAL)";
        }

        private static string ConstruireComparaisonEquipementTexte(Joueur hero, Equipement nouvelEquipement)
        {
            Equipement? actuel = nouvelEquipement.Type switch
            {
                TypeEquipement.Arme => hero.ArmeEquipee,
                TypeEquipement.Armure => hero.ArmureEquipee,
                TypeEquipement.Casque => hero.CasqueEquipe,
                TypeEquipement.Anneau => hero.AnneauEquipe,
                TypeEquipement.Amulette => hero.AmuletteEquipee,
                _ => null
            };

            var sb = new StringBuilder();
            sb.AppendLine("📊 COMPARAISON AVEC LA PIÈCE ÉQUIPÉE :");

            if (actuel == null)
            {
                sb.AppendLine("• Emplacement actuellement VIDE");
                sb.AppendLine($"• Attaque : +{nouvelEquipement.BonusAttaque} 🟢");
                sb.AppendLine($"• Défense : +{nouvelEquipement.BonusDefense} 🟢");
                if (nouvelEquipement.BonusCritique > 0)
                    sb.AppendLine($"• Critique : +{nouvelEquipement.BonusCritique}% 🟢");
                if (nouvelEquipement.BonusManaMax > 0)
                    sb.AppendLine($"• Mana : +{nouvelEquipement.BonusManaMax} 🟢");
                return sb.ToString();
            }

            sb.AppendLine($"• Équipé : {actuel.Nom} ({actuel.RareteItem})");
            int deltaAtk = (nouvelEquipement.BonusAttaque + nouvelEquipement.NiveauAmelioration * 3)
                        - (actuel.BonusAttaque + actuel.NiveauAmelioration * 3);
            int deltaDef = (nouvelEquipement.BonusDefense + nouvelEquipement.NiveauAmelioration * 2)
                        - (actuel.BonusDefense + actuel.NiveauAmelioration * 2);
            int deltaCrit = nouvelEquipement.BonusCritique - actuel.BonusCritique;
            int deltaMana = nouvelEquipement.BonusManaMax - actuel.BonusManaMax;
            int deltaVamp = nouvelEquipement.BonusVampirisme - actuel.BonusVampirisme;

            sb.AppendLine($"• Attaque  : {FormaterDelta(deltaAtk)}");
            sb.AppendLine($"• Défense  : {FormaterDelta(deltaDef)}");
            if (nouvelEquipement.BonusCritique > 0 || actuel.BonusCritique > 0)
                sb.AppendLine($"• Critique : {FormaterDelta(deltaCrit)}");
            if (nouvelEquipement.BonusManaMax > 0 || actuel.BonusManaMax > 0)
                sb.AppendLine($"• Mana     : {FormaterDelta(deltaMana)}");
            if (nouvelEquipement.BonusVampirisme > 0 || actuel.BonusVampirisme > 0)
                sb.AppendLine($"• Vampir.  : {FormaterDelta(deltaVamp)}");

            return sb.ToString();
        }
    }
}
