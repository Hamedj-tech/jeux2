using System.Drawing;
using System.IO;
using Xunit;
using JeuxRPG;

namespace JeuxRPG.Tests
{
    public class MenuTitreTests
    {
        [Fact]
        public void FichierAssetEcranTitre_ExisteEtDimensionsConformes()
        {
            string chemin = Path.Combine(AppContext.BaseDirectory, "Assets", "ecran_titre.png");
            if (!File.Exists(chemin))
            {
                chemin = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Assets", "ecran_titre.png");
            }

            Assert.True(File.Exists(chemin), $"L'asset 'ecran_titre.png' doit exister à l'emplacement : {chemin}");

            using var img = Image.FromFile(chemin);
            Assert.Equal(1024, img.Width);
            Assert.Equal(542, img.Height);
        }

        [Fact]
        public void EtatEcranJeu_Enum_ContientValeursAttendues()
        {
            Assert.Equal(0, (int)EtatEcranJeu.MenuTitre);
            Assert.Equal(1, (int)EtatEcranJeu.EnJeu);
            Assert.Equal(2, (int)EtatEcranJeu.MenuOptions);
        }

        [Fact]
        public void AudioSynthetiseur_SonsMenu_NeLeventPasDException()
        {
            // Les méthodes de sons d'interface doivent s'exécuter sans erreur
            AudioSynthetiseur.SonClic();
            AudioSynthetiseur.SonSelectionMenu();
            AudioSynthetiseur.SonImpact();
        }
    }
}
