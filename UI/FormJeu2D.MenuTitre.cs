using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace JeuxRPG
{
    public enum EtatEcranJeu { MenuTitre, EnJeu, MenuOptions }

    public partial class FormJeu2D
    {
        private EtatEcranJeu etatEcran = EtatEcranJeu.MenuTitre;
        private int boutonTitreSelectionne = 0;
        private float tempsAnimationTitre = 0f;
        private Image? imageEcranTitre = null;
        private bool tentativeChargementImageTitre = false;

        // Coordonnées dans l'espace natif de l'illustration (1024 x 542)
        private static readonly RectangleF RectParchmentHero = new RectangleF(312f, 160f, 400f, 35f);
        private static readonly RectangleF[] RectanglesBoutonsTitre = new RectangleF[]
        {
            new RectangleF(362f, 221f, 300f, 43f), // 0: Continuer l'Aventure
            new RectangleF(362f, 269f, 300f, 43f), // 1: Nouvelle Partie
            new RectangleF(362f, 317f, 300f, 43f), // 2: Options & Commandes
            new RectangleF(362f, 365f, 300f, 43f)  // 3: Quitter
        };
        private static readonly PointF PositionTorcheGauche = new PointF(33f, 176f);
        private static readonly PointF PositionTorcheDroite = new PointF(999f, 177f);

        private Image? ObtenirImageMenuTitre()
        {
            if (tentativeChargementImageTitre) return imageEcranTitre;
            tentativeChargementImageTitre = true;

            string[] cheminsPotentiels = {
                Path.Combine(AppContext.BaseDirectory, "Assets", "ecran_titre.png"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "ecran_titre.png"),
                Path.Combine(Environment.CurrentDirectory, "Assets", "ecran_titre.png"),
                Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Assets", "ecran_titre.png"),
                Path.Combine(RepertoireSauvegardes, "Assets", "ecran_titre.png")
            };

            foreach (string chemin in cheminsPotentiels)
            {
                try
                {
                    if (File.Exists(chemin))
                    {
                        using var stream = new FileStream(chemin, FileMode.Open, FileAccess.Read, FileShare.Read);
                        using var imgTmp = Image.FromStream(stream);
                        imageEcranTitre = new Bitmap(imgTmp);
                        break;
                    }
                }
                catch { }
            }

            return imageEcranTitre;
        }

        private void LibererRessourcesMenuTitre()
        {
            imageEcranTitre?.Dispose();
            imageEcranTitre = null;
        }

        private void InitialiserMenuTitre()
        {
            etatEcran = EtatEcranJeu.MenuTitre;
            Cursor = Cursors.Default;
        }

        private void LancerPartieDepuisMenu()
        {
            etatEcran = EtatEcranJeu.EnJeu;
            jeuEnPause = false;
            Cursor = Cursors.Default;
            AudioSynthetiseur.SonSelectionMenu();
            Invalidate();
        }

        private Rectangle CalculerRectangleDestImageTitre()
        {
            const float ratioImg = 1024f / 542f;
            float ratioEcran = (float)ClientSize.Width / Math.Max(1, ClientSize.Height);

            int destW, destH, destX, destY;
            if (ratioEcran > ratioImg)
            {
                destH = ClientSize.Height;
                destW = (int)(destH * ratioImg);
                destX = (ClientSize.Width - destW) / 2;
                destY = 0;
            }
            else
            {
                destW = ClientSize.Width;
                destH = (int)(destW / ratioImg);
                destX = 0;
                destY = (ClientSize.Height - destH) / 2;
            }

            return new Rectangle(destX, destY, destW, destH);
        }

        private Rectangle ObtenirRectangleEcran(RectangleF rectImage, Rectangle destRect)
        {
            float scaleX = (float)destRect.Width / 1024f;
            float scaleY = (float)destRect.Height / 542f;
            return new Rectangle(
                (int)(destRect.X + rectImage.X * scaleX),
                (int)(destRect.Y + rectImage.Y * scaleY),
                (int)(rectImage.Width * scaleX),
                (int)(rectImage.Height * scaleY)
            );
        }

        private void DessinerMenuTitre(Graphics g)
        {
            // Fond pierre gothique sombre pour les barres d'adaptation
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(12, 13, 17)), ClientRectangle);

            Rectangle destRect = CalculerRectangleDestImageTitre();
            Image? img = ObtenirImageMenuTitre();

            if (img != null)
            {
                var prevInterpolation = g.InterpolationMode;
                var prevSmoothing = g.SmoothingMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Affichage de l'illustration principale
                g.DrawImage(img, destRect);

                float scaleX = (float)destRect.Width / 1024f;
                float scaleY = (float)destRect.Height / 542f;

                // Torches animées à gauche et à droite
                PointF torchL = new PointF(destRect.X + PositionTorcheGauche.X * scaleX, destRect.Y + PositionTorcheGauche.Y * scaleY);
                PointF torchR = new PointF(destRect.X + PositionTorcheDroite.X * scaleX, destRect.Y + PositionTorcheDroite.Y * scaleY);
                DessinerFlammeTorche(g, torchL, scaleX, 1);
                DessinerFlammeTorche(g, torchR, scaleX, 2);

                // Poussières d'or en suspension
                DessinerPoussieresMagiques(g, destRect);

                // Bannière héroïque dynamique
                DessinerBanniereHero(g, destRect, scaleX);

                // Mise en valeur dorée du bouton sélectionné
                DessinerSurbrillanceBouton(g, destRect, boutonTitreSelectionne, scaleX, scaleY);

                g.InterpolationMode = prevInterpolation;
                g.SmoothingMode = prevSmoothing;
            }
            else
            {
                DessinerMenuTitreRepli(g);
            }
        }

        private void DessinerFlammeTorche(Graphics g, PointF centre, float scale, int seed)
        {
            float t = tempsAnimationTitre * 11f + seed * 3.14159f;
            float flicker = 1.0f + 0.14f * MathF.Sin(t) + 0.08f * MathF.Cos(t * 1.63f);
            float baseR = 26f * scale * flicker;

            // Halo externe ambré
            float rOuter = baseR * 1.55f;
            using (SolidBrush bOuter = new SolidBrush(Color.FromArgb(35, 255, 120, 20)))
                g.FillEllipse(bOuter, centre.X - rOuter, centre.Y - rOuter, rOuter * 2, rOuter * 2);

            // Cœur chaud doré
            float rMid = baseR * 0.95f;
            using (SolidBrush bMid = new SolidBrush(Color.FromArgb(85, 255, 175, 40)))
                g.FillEllipse(bMid, centre.X - rMid, centre.Y - rMid, rMid * 2, rMid * 2);

            // Flamme incandescente
            float rCore = baseR * 0.45f;
            using (SolidBrush bCore = new SolidBrush(Color.FromArgb(170, 255, 240, 180)))
                g.FillEllipse(bCore, centre.X - rCore, centre.Y - rCore, rCore * 2, rCore * 2);

            // Étincelles incandescentes ascendantes
            for (int i = 0; i < 7; i++)
            {
                float phase = ((tempsAnimationTitre * 0.75f + i * 0.14f + seed * 0.28f) % 1.0f);
                float emberY = centre.Y - phase * 50f * scale;
                float emberX = centre.X + MathF.Sin(tempsAnimationTitre * 3.8f + i * 1.8f + seed) * (7f * scale);
                int alpha = (int)((1.0f - phase) * 210f);
                if (alpha <= 0) continue;
                float emberSize = (1.5f + (i % 3) * 0.7f) * scale;
                Color cEmber = (i % 2 == 0) ? Color.FromArgb(alpha, 255, 200, 50) : Color.FromArgb(alpha, 255, 130, 30);
                using (SolidBrush bEmber = new SolidBrush(cEmber))
                    g.FillEllipse(bEmber, emberX - emberSize / 2f, emberY - emberSize / 2f, emberSize, emberSize);
            }
        }

        private void DessinerPoussieresMagiques(Graphics g, Rectangle destRect)
        {
            for (int i = 0; i < 16; i++)
            {
                float x = destRect.X + ((i * 67 + tempsAnimationTitre * 9f * (1 + (i % 3))) % destRect.Width);
                float y = destRect.Y + ((i * 41 + MathF.Sin(tempsAnimationTitre * 0.7f + i) * 20f + destRect.Height) % destRect.Height);
                int alpha = (int)(22 + 18 * MathF.Sin(tempsAnimationTitre * 2f + i));
                if (alpha <= 0) continue;
                using (SolidBrush bPoussiere = new SolidBrush(Color.FromArgb(alpha, 255, 220, 150)))
                    g.FillEllipse(bPoussiere, x, y, 2f, 2f);
            }
        }

        private void DessinerBanniereHero(Graphics g, Rectangle destRect, float scaleX)
        {
            Rectangle rectHero = ObtenirRectangleEcran(RectParchmentHero, destRect);
            bool correspondImage = hero != null && hero.Nom == "larabe" && hero.Niveau == 71 && hero.Or == 50000;

            if (!correspondImage)
            {
                string texteHero = hero != null
                    ? $"Héros Actuel : {hero.Nom} • {hero.Classe} • Niveau {hero.Niveau} • {hero.Or} 🪙"
                    : "Aucune Sauvegarde Active — Créez Votre Héros !";

                // Fond parchemin assorti à la palette de l'illustration
                using (LinearGradientBrush brushParchment = new(
                    rectHero,
                    Color.FromArgb(224, 192, 144),
                    Color.FromArgb(178, 143, 98),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brushParchment, rectHero);
                }

                // Encadrement médiéval sombre et liseré supérieur lumineux
                using (Pen penBordure = new Pen(Color.FromArgb(105, 70, 36), 1.5f * scaleX))
                {
                    g.DrawRectangle(penBordure, rectHero);
                }
                using (Pen penReflet = new Pen(Color.FromArgb(180, 255, 235, 170), 1f))
                {
                    g.DrawLine(penReflet, rectHero.Left + 2, rectHero.Top + 1, rectHero.Right - 2, rectHero.Top + 1);
                }

                // Texte de synthèse du héros en calligraphie sombre
                using (Font fontHero = new Font("Segoe UI", Math.Max(8f, 10.5f * scaleX), FontStyle.Bold))
                {
                    SizeF sz = g.MeasureString(texteHero, fontHero);
                    float tx = rectHero.Left + (rectHero.Width - sz.Width) / 2f;
                    float ty = rectHero.Top + (rectHero.Height - sz.Height) / 2f;

                    // Ombre claire portée
                    using (SolidBrush bOmbre = new SolidBrush(Color.FromArgb(130, 255, 245, 220)))
                        g.DrawString(texteHero, fontHero, bOmbre, tx + 1, ty + 1);

                    // Encre brune
                    using (SolidBrush bEncre = new SolidBrush(Color.FromArgb(45, 25, 12)))
                        g.DrawString(texteHero, fontHero, bEncre, tx, ty);
                }
            }

            // Reflet doré périodique balayant doucement le ruban
            float cycle = (tempsAnimationTitre * 0.35f) % 4.5f;
            if (cycle < 1.0f)
            {
                float sheenX = rectHero.Left + cycle * rectHero.Width;
                float sheenW = 45f * scaleX;
                RectangleF rectSheen = new RectangleF(sheenX - sheenW / 2f, rectHero.Top, sheenW, rectHero.Height);
                using (LinearGradientBrush sheenBrush = new(
                    rectSheen,
                    Color.Transparent,
                    Color.FromArgb(50, 255, 235, 160),
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(sheenBrush, rectSheen);
                }
            }
        }

        private void DessinerSurbrillanceBouton(Graphics g, Rectangle destRect, int index, float scaleX, float scaleY)
        {
            if (index < 0 || index >= RectanglesBoutonsTitre.Length) return;

            Rectangle rectBtn = ObtenirRectangleEcran(RectanglesBoutonsTitre[index], destRect);
            float pulsation = (MathF.Sin(tempsAnimationTitre * 5f) * 0.5f + 0.5f);

            // Halo externe doré doux
            int alphaGlow = (int)(55 + 40 * pulsation);
            Rectangle rectGlow = Rectangle.Inflate(rectBtn, (int)(4 * scaleX), (int)(3 * scaleY));
            using (Pen penGlow = new Pen(Color.FromArgb(alphaGlow, 255, 215, 0), 3f * scaleX))
            {
                g.DrawRectangle(penGlow, rectGlow);
            }

            // Cadre doré illuminé
            int alphaBordure = (int)(210 + 45 * pulsation);
            using (Pen penBorder = new Pen(Color.FromArgb(alphaBordure, 255, 225, 90), 2f * scaleX))
            {
                g.DrawRectangle(penBorder, rectBtn);
            }

            // Remplissage shimmer doré subtil
            int alphaShimmer = (int)(25 + 20 * pulsation);
            using (SolidBrush bShimmer = new SolidBrush(Color.FromArgb(alphaShimmer, 255, 220, 60)))
            {
                g.FillRectangle(bShimmer, rectBtn);
            }

            // Coins ornementaux médiévaux
            int coinTaille = (int)(6 * scaleX);
            using (SolidBrush bCoin = new SolidBrush(Color.FromArgb(240, 255, 230, 120)))
            {
                g.FillRectangle(bCoin, rectBtn.Left, rectBtn.Top, coinTaille, 2);
                g.FillRectangle(bCoin, rectBtn.Left, rectBtn.Top, 2, coinTaille);
                g.FillRectangle(bCoin, rectBtn.Right - coinTaille, rectBtn.Top, coinTaille, 2);
                g.FillRectangle(bCoin, rectBtn.Right - 2, rectBtn.Top, 2, coinTaille);
                g.FillRectangle(bCoin, rectBtn.Left, rectBtn.Bottom - 2, coinTaille, 2);
                g.FillRectangle(bCoin, rectBtn.Left, rectBtn.Bottom - coinTaille, 2, coinTaille);
                g.FillRectangle(bCoin, rectBtn.Right - coinTaille, rectBtn.Bottom - 2, coinTaille, 2);
                g.FillRectangle(bCoin, rectBtn.Right - 2, rectBtn.Bottom - coinTaille, 2, coinTaille);
            }

            // Marqueurs latéraux lumineux
            using (Font fontMarqueur = new Font("Segoe UI", Math.Max(9f, 13f * scaleX), FontStyle.Bold))
            {
                int alphaMarqueur = (int)(180 + 75 * pulsation);
                using (SolidBrush bMarqueur = new SolidBrush(Color.FromArgb(alphaMarqueur, 255, 225, 90)))
                {
                    float leftX = rectBtn.Left - 24f * scaleX;
                    float rightX = rectBtn.Right + 8f * scaleX;
                    float midY = rectBtn.Top + (rectBtn.Height - fontMarqueur.Height) / 2f;
                    g.DrawString("▶", fontMarqueur, bMarqueur, leftX, midY);
                    g.DrawString("◀", fontMarqueur, bMarqueur, rightX, midY);
                }
            }
        }

        private void DessinerMenuTitreRepli(Graphics g)
        {
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(14, 16, 22)), ClientRectangle);
            int centreX = ClientSize.Width / 2;
            int yTitre = Math.Max(40, ClientSize.Height / 6);
            float oscillation = MathF.Sin(tempsAnimationTitre * 1.8f) * 6f;

            using (Font fontGrandTitre = new Font("Segoe UI", 32f, FontStyle.Bold))
            {
                string titre = "CHRONIQUES D'AETHELGARD";
                SizeF szTitre = g.MeasureString(titre, fontGrandTitre);
                g.DrawString(titre, fontGrandTitre, Brushes.Black, centreX - szTitre.Width / 2f + 2, yTitre + oscillation + 3);
                g.DrawString(titre, fontGrandTitre, Brushes.Gold, centreX - szTitre.Width / 2f, yTitre + oscillation);
            }

            using (Font fontSousTitre = new Font("Segoe UI", 13f, FontStyle.Italic))
            {
                string sousTitre = "— Action RPG 2D • L'Éveil des Arcanes —";
                SizeF szSous = g.MeasureString(sousTitre, fontSousTitre);
                g.DrawString(sousTitre, fontSousTitre, Brushes.LightSkyBlue, centreX - szSous.Width / 2f, yTitre + 65 + oscillation * 0.5f);
            }

            int yHeroInfo = yTitre + 115;
            if (hero != null)
            {
                string infoHero = $"Héros Actuel : {hero.Nom}  •  {hero.Classe}  •  Niveau {hero.Niveau}  •  {hero.Or} 🪙";
                SizeF szInfo = g.MeasureString(infoHero, fontGras);
                int wPill = (int)szInfo.Width + 30;
                Rectangle rectPill = new Rectangle(centreX - wPill / 2, yHeroInfo, wPill, 32);
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(35, 40, 55)), rectPill);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(142, 68, 173), 1.5f), rectPill);
                g.DrawString(infoHero, fontGras, Brushes.White, centreX - szInfo.Width / 2f, yHeroInfo + 6);
            }

            int btnW = 340;
            int btnH = 46;
            int yBtns = yHeroInfo + 60;
            int espacement = 16;

            DessinerBoutonTitreRepli(g, centreX - btnW / 2, yBtns, btnW, btnH, "▶  CONTINUER L'AVENTURE", boutonTitreSelectionne == 0);
            yBtns += btnH + espacement;

            DessinerBoutonTitreRepli(g, centreX - btnW / 2, yBtns, btnW, btnH, "✨  NOUVELLE PARTIE", boutonTitreSelectionne == 1);
            yBtns += btnH + espacement;

            DessinerBoutonTitreRepli(g, centreX - btnW / 2, yBtns, btnW, btnH, "⚙️  OPTIONS & COMMANDES", boutonTitreSelectionne == 2);
            yBtns += btnH + espacement;

            DessinerBoutonTitreRepli(g, centreX - btnW / 2, yBtns, btnW, btnH, "🚪  QUITTER", boutonTitreSelectionne == 3);

            string footer = "Utilisez la souris ou [ENTRÉE] pour valider  •  F11 pour Plein Écran";
            SizeF szF = g.MeasureString(footer, fontPetit);
            g.DrawString(footer, fontPetit, Brushes.Gray, centreX - szF.Width / 2f, ClientSize.Height - 35);
        }

        private void DessinerBoutonTitreRepli(Graphics g, int x, int y, int w, int h, string texte, bool survole)
        {
            Rectangle rect = new Rectangle(x, y, w, h);
            Color fond = survole ? Color.FromArgb(50, 60, 85) : Color.FromArgb(28, 30, 42);
            Color bordure = survole ? Color.Gold : Color.FromArgb(80, 90, 110);

            g.FillRectangle(CacheRenduGDI.ObtenirBrush(fond), rect);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(bordure, survole ? 2f : 1f), rect);

            if (survole)
            {
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.Gold), x + 4, y + 4, 4, h - 8);
            }

            SizeF sz = g.MeasureString(texte, fontGras);
            Brush brushTexte = survole ? Brushes.Gold : Brushes.White;
            g.DrawString(texte, fontGras, brushTexte, x + (w - sz.Width) / 2f, y + (h - sz.Height) / 2f);
        }

        private void GererSurvolMenuTitre(Point pt)
        {
            Rectangle destRect = CalculerRectangleDestImageTitre();
            int survole = -1;

            if (ObtenirImageMenuTitre() != null)
            {
                for (int i = 0; i < RectanglesBoutonsTitre.Length; i++)
                {
                    Rectangle rect = ObtenirRectangleEcran(RectanglesBoutonsTitre[i], destRect);
                    if (rect.Contains(pt))
                    {
                        survole = i;
                        break;
                    }
                }
            }
            else
            {
                int centreX = ClientSize.Width / 2;
                int yTitre = Math.Max(40, ClientSize.Height / 6);
                int yHeroInfo = yTitre + 115;
                int btnW = 340;
                int btnH = 46;
                int yBtns = yHeroInfo + 60;
                int espacement = 16;
                for (int i = 0; i < 4; i++)
                {
                    Rectangle rect = new Rectangle(centreX - btnW / 2, yBtns, btnW, btnH);
                    if (rect.Contains(pt)) { survole = i; break; }
                    yBtns += btnH + espacement;
                }
            }

            if (survole != -1)
            {
                Cursor = Cursors.Hand;
                if (boutonTitreSelectionne != survole)
                {
                    boutonTitreSelectionne = survole;
                    AudioSynthetiseur.SonClic();
                    Invalidate();
                }
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        private void GererClicMenuTitre(Point pt)
        {
            Rectangle destRect = CalculerRectangleDestImageTitre();

            if (ObtenirImageMenuTitre() != null)
            {
                for (int i = 0; i < RectanglesBoutonsTitre.Length; i++)
                {
                    Rectangle rect = ObtenirRectangleEcran(RectanglesBoutonsTitre[i], destRect);
                    if (rect.Contains(pt))
                    {
                        boutonTitreSelectionne = i;
                        AudioSynthetiseur.SonImpact();
                        ExecuterActionMenuTitre(i);
                        return;
                    }
                }
            }
            else
            {
                int centreX = ClientSize.Width / 2;
                int yTitre = Math.Max(40, ClientSize.Height / 6);
                int yHeroInfo = yTitre + 115;
                int btnW = 340;
                int btnH = 46;
                int yBtns = yHeroInfo + 60;
                int espacement = 16;

                for (int i = 0; i < 4; i++)
                {
                    Rectangle rect = new Rectangle(centreX - btnW / 2, yBtns, btnW, btnH);
                    if (rect.Contains(pt))
                    {
                        boutonTitreSelectionne = i;
                        AudioSynthetiseur.SonImpact();
                        ExecuterActionMenuTitre(i);
                        return;
                    }
                    yBtns += btnH + espacement;
                }
            }
        }

        private void ExecuterActionMenuTitre(int index)
        {
            switch (index)
            {
                case 0: // Continuer l'Aventure
                    if (hero == null)
                    {
                        if (!AfficherCreationPersonnageRapide()) return;
                    }
                    LancerPartieDepuisMenu();
                    break;

                case 1: // Nouvelle Partie
                    if (AfficherCreationPersonnageRapide())
                    {
                        InitialiserMondeEtCamera();
                        SauvegarderPartie(false);
                        LancerPartieDepuisMenu();
                    }
                    break;

                case 2: // Options & Commandes
                    OuvrirMenuOptionsComplet();
                    break;

                case 3: // Quitter
                    Close();
                    break;
            }
        }

        private void OuvrirMenuOptionsComplet()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Options & Paramètres de Jeu";
                dlg.Size = new Size(500, 420);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(24, 26, 34);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                Label lblTitre = new Label
                {
                    Text = "⚙️ PARAMÈTRES DU JEU",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    Location = new Point(25, 20),
                    AutoSize = true
                };

                // Contrôle du Volume
                Label lblVol = new Label
                {
                    Text = $"🔊 Volume des effets sonores : {(int)(AudioSynthetiseur.Volume * 100)}%",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Location = new Point(25, 70),
                    AutoSize = true
                };

                TrackBar tbVol = new TrackBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = (int)(AudioSynthetiseur.Volume * 100),
                    TickFrequency = 10,
                    Location = new Point(25, 100),
                    Width = 430
                };
                tbVol.ValueChanged += (s, e) =>
                {
                    AudioSynthetiseur.Volume = tbVol.Value / 100f;
                    lblVol.Text = $"🔊 Volume des effets sonores : {tbVol.Value}%";
                };

                // Bouton Configuration des Touches
                Button btnTouches = new Button
                {
                    Text = "⌨️ Configurer les Touches du Clavier (F7)",
                    Location = new Point(25, 175),
                    Size = new Size(430, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnTouches.FlatAppearance.BorderSize = 0;
                btnTouches.Click += (s, e) =>
                {
                    OuvrirConfigurationTouches();
                };

                // Bouton Plein Écran
                Button btnPleinEcran = new Button
                {
                    Text = FormBorderStyle == FormBorderStyle.None ? "🖥️ Passer en Mode Fenêtré (F11)" : "🖥️ Passer en Plein Écran (F11)",
                    Location = new Point(25, 230),
                    Size = new Size(430, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnPleinEcran.FlatAppearance.BorderSize = 0;
                btnPleinEcran.Click += (s, e) =>
                {
                    BasculerPleinEcran();
                    btnPleinEcran.Text = FormBorderStyle == FormBorderStyle.None ? "🖥️ Passer en Mode Fenêtré (F11)" : "🖥️ Passer en Plein Écran (F11)";
                };

                // Bouton Fermer
                Button btnFermer = new Button
                {
                    Text = "✔ Valider & Retour",
                    Location = new Point(150, 310),
                    Size = new Size(180, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnFermer.FlatAppearance.BorderSize = 0;
                btnFermer.Click += (s, e) => { dlg.Close(); };

                dlg.Controls.AddRange(new Control[] { lblTitre, lblVol, tbVol, btnTouches, btnPleinEcran, btnFermer });
                dlg.ShowDialog(this);
            }
        }

        private void BasculerPleinEcran()
        {
            if (FormBorderStyle == FormBorderStyle.None)
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
                Size = new Size(1366, 768);
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
        }
    }
}
