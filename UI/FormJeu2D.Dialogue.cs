using System;
using System.Drawing;
using System.Windows.Forms;

namespace JeuxRPG
{
    public partial class FormJeu2D
    {
        private sealed class EtatDialogue
        {
            public string Titre { get; set; } = "";
            public string Texte { get; set; } = "";
            public string Icone { get; set; } = "💬";
            public Action? AuFermer { get; set; }
            public float TempsAffichage { get; set; }
        }

        private EtatDialogue? dialogueActif;

        public void AfficherDialogueInGame(string titre, string texte, string icone = "💬", Action? auFermer = null)
        {
            dialogueActif = new EtatDialogue
            {
                Titre = titre,
                Texte = texte,
                Icone = icone,
                AuFermer = auFermer,
                TempsAffichage = 0f
            };
            jeuEnPause = true;
            Invalidate();
        }

        public void FermerDialogueInGame()
        {
            if (dialogueActif == null) return;
            var action = dialogueActif.AuFermer;
            dialogueActif = null;
            jeuEnPause = false;
            Invalidate();
            action?.Invoke();
        }

        private void DessinerDialogueInGame(Graphics g)
        {
            if (dialogueActif == null) return;

            // Voile sombre semi-transparent
            g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(150, Color.Black), ClientRectangle);

            int margeHorizontale = Math.Max(40, ClientSize.Width / 8);
            int largeur = ClientSize.Width - (margeHorizontale * 2);
            int hauteur = Math.Clamp(ClientSize.Height / 4, 150, 240);
            int x = margeHorizontale;
            int y = ClientSize.Height - hauteur - 30;

            Rectangle rect = new Rectangle(x, y, largeur, hauteur);

            // Fond sombre panneau
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(240, 20, 22, 30)), rect);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(241, 196, 15), 2.5f), rect);

            // Icône / Avatar à gauche
            int tailleAvatar = 64;
            int xAvatar = x + 20;
            int yAvatar = y + 20;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(50, 40, 70)), xAvatar, yAvatar, tailleAvatar, tailleAvatar);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.Gold, 2f), xAvatar, yAvatar, tailleAvatar, tailleAvatar);

            using (Font fontAvatar = new Font("Segoe UI", 24f))
            {
                SizeF szAvatar = g.MeasureString(dialogueActif.Icone, fontAvatar);
                g.DrawString(dialogueActif.Icone, fontAvatar, Brushes.White, xAvatar + (tailleAvatar - szAvatar.Width) / 2f, yAvatar + (tailleAvatar - szAvatar.Height) / 2f);
            }

            // Titre du locuteur
            int xTexte = xAvatar + tailleAvatar + 20;
            g.DrawString(dialogueActif.Titre, fontTitre, Brushes.Gold, xTexte, y + 20);

            // Ligne de séparation
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(80, 255, 215, 0), 1f), xTexte, y + 48, x + largeur - 25, y + 48);

            // Corps du texte
            RectangleF rectTexte = new RectangleF(xTexte, y + 56, largeur - (xTexte - x) - 25, hauteur - 95);
            using (StringFormat sf = new StringFormat { Trimming = StringTrimming.Word })
            {
                g.DrawString(dialogueActif.Texte, fontGras, Brushes.White, rectTexte, sf);
            }

            // Bouton / Invite de touche clignotante
            string invite = "Appuyez sur [ESPACE] ou [E] pour continuer ▶";
            SizeF szInvite = g.MeasureString(invite, fontPetit);
            int xInvite = x + largeur - (int)szInvite.Width - 25;
            int yInvite = y + hauteur - 32;

            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(180, 39, 174, 96)), xInvite - 8, yInvite - 3, szInvite.Width + 16, szInvite.Height + 6);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Gold, 1.5f), xInvite - 8, yInvite - 3, szInvite.Width + 16, szInvite.Height + 6);
            g.DrawString(invite, fontPetit, Brushes.White, xInvite, yInvite);
        }
    }
}
