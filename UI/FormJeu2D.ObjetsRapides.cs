using System;
using System.Drawing;
using System.Windows.Forms;

namespace JeuxRPG
{
    public partial class FormJeu2D
    {
        private float tempsBuffForce = 0f;
        private float tempsBuffPeauDePierre = 0f;

        public bool BuffForceActif => tempsBuffForce > 0f;
        public bool BuffPeauDePierreActif => tempsBuffPeauDePierre > 0f;

        private void MettreAJourBuffs(float dt)
        {
            if (tempsBuffForce > 0f)
            {
                tempsBuffForce -= dt;
                if (tempsBuffForce <= 0f)
                {
                    tempsBuffForce = 0f;
                    monde.AjouterTexteFlottant(joueur2D.Position, "Fin du buff de Force", Color.LightGray, false);
                }
            }

            if (tempsBuffPeauDePierre > 0f)
            {
                tempsBuffPeauDePierre -= dt;
                if (tempsBuffPeauDePierre <= 0f)
                {
                    tempsBuffPeauDePierre = 0f;
                    monde.AjouterTexteFlottant(joueur2D.Position, "Fin du buff Peau de Pierre", Color.LightGray, false);
                }
            }
        }

        private void UtiliserElixirForce()
        {
            if (hero.ObtenirQuantiteConsommable(TypeConsommable.ElixirForce) > 0)
            {
                hero.UtiliserConsommable(TypeConsommable.ElixirForce);
                tempsBuffForce = 15f;
                AudioSynthetiseur.SonCritique();
                camera.DeclencherSecousse(6f, 0.25f);
                monde.Particules.EmettreEclats(joueur2D.Position, Color.OrangeRed, 25, 180f, 4f, 0.5f);
                monde.Particules.EmettreOndeDeChoc(joueur2D.Position, Color.Gold, 80f, 0.35f);
                monde.AjouterTexteFlottant(joueur2D.Position, "⚔️ ÉLIXIR DE FORCE ! (+30% ATQ / 15s)", Color.Gold, true);
            }
            else
            {
                monde.AjouterTexteFlottant(joueur2D.Position, "Plus d'Élixir de Force !", Color.Orange, false);
            }
        }

        private void UtiliserPeauDePierre()
        {
            if (hero.ObtenirQuantiteConsommable(TypeConsommable.PeauDePierre) > 0)
            {
                hero.UtiliserConsommable(TypeConsommable.PeauDePierre);
                tempsBuffPeauDePierre = 15f;
                AudioSynthetiseur.SonImpact();
                camera.DeclencherSecousse(5f, 0.2f);
                monde.Particules.EmettreEclats(joueur2D.Position, Color.SlateGray, 25, 160f, 4.5f, 0.5f);
                monde.Particules.EmettreOndeDeChoc(joueur2D.Position, Color.SlateGray, 75f, 0.3f);
                monde.AjouterTexteFlottant(joueur2D.Position, "🛡️ PEAU DE PIERRE ! (+40% DEF / 15s)", Color.LightSteelBlue, true);
            }
            else
            {
                monde.AjouterTexteFlottant(joueur2D.Position, "Plus de potion Peau de Pierre !", Color.Orange, false);
            }
        }

        private void UtiliserBombeIncendiaire()
        {
            if (hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire) > 0)
            {
                hero.UtiliserConsommable(TypeConsommable.BombeIncendiaire);
                Vector2 cibleMonde = entrees.PositionSourisMonde;
                Vector2 dir = (cibleMonde - joueur2D.Position).Normaliser();
                if (dir.LongueurCarree() < 0.01f) dir = new Vector2(1, 0);

                int degatsBombe = 120 + hero.AttaqueTotale * 2;

                monde.AjouterProjectile(new Projectile2D
                {
                    Position = joueur2D.Position,
                    Velocite = dir * 480f,
                    Rayon = 8f,
                    Degats = degatsBombe,
                    EstDuJoueur = true,
                    Couleur = Color.DarkOrange,
                    TempsVie = 0.9f,
                    NomSort = "Bombe Incendiaire",
                    Transpercant = false
                });

                AudioSynthetiseur.SonTirMagique();
                monde.Particules.EmettreEclats(joueur2D.Position, Color.OrangeRed, 12, 140f, 3.5f, 0.3f);
                monde.AjouterTexteFlottant(joueur2D.Position, "💣 Bombe Incendiaire lancée !", Color.Orange, false);
            }
            else
            {
                monde.AjouterTexteFlottant(joueur2D.Position, "Plus de Bombe Incendiaire !", Color.Orange, false);
            }
        }

        private void UtiliserParcheminTeleport()
        {
            if (hero.ObtenirQuantiteConsommable(TypeConsommable.ParcheminTeleport) > 0)
            {
                hero.UtiliserConsommable(TypeConsommable.ParcheminTeleport);
                AudioSynthetiseur.SonCritique();
                camera.DeclencherSecousse(10f, 0.4f);
                monde.Particules.EmettreAnneauExplosion(joueur2D.Position, Color.Cyan, 35, 250f);
                monde.ChargerVillage();
                joueur2D.Position = monde.PositionDepartVillage;
                camera.Position = joueur2D.Position;
                camera.Cible = joueur2D.Position;
                monde.Particules.EmettreAnneauExplosion(joueur2D.Position, Color.Cyan, 35, 250f);
                monde.AjouterTexteFlottant(joueur2D.Position, "🌀 Téléportation vers la Capitale !", Color.Cyan, true);
            }
            else
            {
                monde.AjouterTexteFlottant(joueur2D.Position, "Plus de Parchemin de Téléportation !", Color.Orange, false);
            }
        }
    }
}
