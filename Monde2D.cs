using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace JeuxRPG
{
    public enum ZoneType2D { Village, Donjon, TourAstrale }

    public enum TypeObstacle
    {
        MurStandard,
        MurEnceinteNord,
        MurEnceinteSud,
        MurEnceinteOuest,
        MurEnceinteEst,
        BatimentForge,
        BatimentGuilde,
        BatimentAlchimie,
        GrandeFontaine,
        Arbre,
        ArbreGrand,
        PilierPierre,
        PilierAstral,
        Lampadaire,
        Brasero,
        BancPierre,
        CaisseTonneau,
        TaverneDragonVert,
        EtalMarche,
        CharretteAttellee,
        PontPierre
    }

    public class Obstacle2D
    {
        public RectangleF Boite;
        public string Nom = "";
        public Color Couleur = Color.FromArgb(60, 64, 75);
        public TypeObstacle TypeObstacle = TypeObstacle.MurStandard;

        public Obstacle2D(float x, float y, float w, float h, string nom = "", TypeObstacle type = TypeObstacle.MurStandard, Color? couleur = null)
        {
            Boite = new RectangleF(x, y, w, h);
            Nom = nom;
            TypeObstacle = type;
            if (couleur.HasValue) Couleur = couleur.Value;
        }

        private static readonly Font FontEnseigne = new Font("Segoe UI", 8f, FontStyle.Bold);
        private static readonly Font FontPlaque = new Font("Segoe UI", 7.5f, FontStyle.Bold);

        public void DessinerOmbre(Graphics g, Camera2D cam)
        {
            Point p = cam.MondeVersEcran(new Vector2(Boite.X, Boite.Y));
            int w = (int)(Boite.Width * cam.Zoom);
            int h = (int)(Boite.Height * cam.Zoom);
            Brush bOmbre = CacheRenduGDI.ObtenirBrushAlpha(85, Color.FromArgb(12, 14, 18));

            switch (TypeObstacle)
            {
                case TypeObstacle.Arbre:
                case TypeObstacle.ArbreGrand:
                    float rOmbre = (Boite.Width * 0.95f) * cam.Zoom;
                    g.FillEllipse(bOmbre, p.X + w / 2f - rOmbre + 8, p.Y + h / 2f - rOmbre * 0.65f + 16, rOmbre * 2f, rOmbre * 1.3f);
                    break;

                case TypeObstacle.GrandeFontaine:
                    float rFontOmbre = (Boite.Width / 2f + 16f) * cam.Zoom;
                    g.FillEllipse(bOmbre, p.X + w / 2f - rFontOmbre + 8, p.Y + h / 2f - rFontOmbre * 0.85f + 14, rFontOmbre * 2f, rFontOmbre * 1.7f);
                    break;

                case TypeObstacle.PilierPierre:
                case TypeObstacle.PilierAstral:
                case TypeObstacle.Lampadaire:
                case TypeObstacle.Brasero:
                    g.FillEllipse(bOmbre, p.X + 2, p.Y + h - (int)(8 * cam.Zoom), w + 6, (int)(18 * cam.Zoom));
                    break;

                case TypeObstacle.CharretteAttellee:
                case TypeObstacle.EtalMarche:
                    g.FillEllipse(bOmbre, p.X + 4, p.Y + h - (int)(10 * cam.Zoom), w - 8, (int)(16 * cam.Zoom));
                    break;

                case TypeObstacle.PontPierre:
                    break;

                default: // Bâtiments et murs
                    g.FillRectangle(bOmbre, p.X + 10, p.Y + 14, w, h);
                    break;
            }
        }

        public void Dessiner(Graphics g, Camera2D cam, float tempsTotal)
        {
            Point p = cam.MondeVersEcran(new Vector2(Boite.X, Boite.Y));
            int w = (int)(Boite.Width * cam.Zoom);
            int h = (int)(Boite.Height * cam.Zoom);

            switch (TypeObstacle)
            {
                case TypeObstacle.BatimentForge:
                    DessinerForge(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.BatimentGuilde:
                    DessinerGuilde(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.BatimentAlchimie:
                    DessinerAlchimie(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.GrandeFontaine:
                    DessinerFontaine(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.Arbre:
                case TypeObstacle.ArbreGrand:
                    DessinerArbre(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.PilierPierre:
                    DessinerPilierPierre(g, cam, p, w, h);
                    break;
                case TypeObstacle.PilierAstral:
                    DessinerPilierAstral(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.Lampadaire:
                    DessinerLampadaire(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.Brasero:
                    DessinerBrasero(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.BancPierre:
                    DessinerBanc(g, cam, p, w, h);
                    break;
                case TypeObstacle.CaisseTonneau:
                    DessinerCaissesTonneaux(g, cam, p, w, h);
                    break;
                case TypeObstacle.MurEnceinteNord:
                case TypeObstacle.MurEnceinteSud:
                case TypeObstacle.MurEnceinteOuest:
                case TypeObstacle.MurEnceinteEst:
                    DessinerRempart(g, cam, p, w, h);
                    break;
                case TypeObstacle.TaverneDragonVert:
                    DessinerTaverneDragonVert(g, cam, p, w, h, tempsTotal);
                    break;
                case TypeObstacle.EtalMarche:
                    DessinerEtalMarche(g, cam, p, w, h);
                    break;
                case TypeObstacle.CharretteAttellee:
                    DessinerCharretteAttellee(g, cam, p, w, h);
                    break;
                case TypeObstacle.PontPierre:
                    DessinerPontPierre(g, cam, p, w, h, tempsTotal);
                    break;
                default:
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Couleur), p.X, p.Y, w, h);
                    g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(90, 95, 110), 2f), p.X, p.Y, w, h);
                    break;
            }
        }

        private void DessinerForge(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            // Base mur en grès chaud & colombages
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(68, 58, 48)), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(45, 35, 25), 2f), p.X, p.Y, w, h);

            // Toiture en tuiles de terre cuite / ardoise rouge
            int hautToit = (int)(h * 0.48f);
            Point[] polyToit = {
                new Point(p.X - (int)(10 * cam.Zoom), p.Y + hautToit),
                new Point(p.X + w / 2, p.Y - (int)(12 * cam.Zoom)),
                new Point(p.X + w + (int)(10 * cam.Zoom), p.Y + hautToit)
            };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(165, 62, 42)), polyToit);
            g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(115, 38, 24), 2.5f), polyToit);

            // Lignes de tuiles sur la toiture
            for (int i = 1; i <= 3; i++)
            {
                float t = i / 4f;
                int yTuile = (int)(p.Y + hautToit - (hautToit + 12 * cam.Zoom) * t);
                int xG = (int)(p.X + (w / 2f) * (1f - t) - 6 * cam.Zoom);
                int xD = (int)(p.X + w - (w / 2f) * (1f - t) + 6 * cam.Zoom);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(135, 48, 30), 1.5f), xG, yTuile, xD, yTuile);
            }

            // Cheminée en pierre sur l'angle supérieur gauche
            int xChem = p.X + (int)(22 * cam.Zoom);
            int yChem = p.Y - (int)(24 * cam.Zoom);
            int wChem = (int)(32 * cam.Zoom);
            int hChem = (int)(48 * cam.Zoom);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(75, 78, 85)), xChem, yChem, wChem, hChem);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(48, 50, 56), 2f), xChem, yChem, wChem, hChem);
            // Rebord supérieur de cheminée
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(92, 95, 104)), xChem - 3, yChem - 4, wChem + 6, 6);
            // Lueur chaude de braise dans le conduit
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(240, 100, 20)), xChem + 4, yChem, wChem - 8, 4);

            // Porte en chêne sombre ferrée
            int xPorte = p.X + (int)(w * 0.48f);
            int yPorte = p.Y + (int)(h * 0.52f);
            int wPorte = (int)(w * 0.28f);
            int hPorte = (int)(h * 0.48f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(42, 30, 20)), xPorte, yPorte, wPorte, hPorte);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(28, 18, 12), 2f), xPorte, yPorte, wPorte, hPorte);
            // Charnières en fer forgé
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(180, 180, 190), 2f), xPorte + 2, yPorte + 8, xPorte + 14, yPorte + 8);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(180, 180, 190), 2f), xPorte + 2, yPorte + hPorte - 8, xPorte + 14, yPorte + hPorte - 8);

            // Fenêtre de forge avec fourneau incandescent animé
            int xFen = p.X + (int)(w * 0.14f);
            int yFen = p.Y + (int)(h * 0.56f);
            int wFen = (int)(w * 0.24f);
            int hFen = (int)(h * 0.32f);
            float pulseFeu = 0.82f + 0.18f * MathF.Sin(tempsTotal * 6.5f);
            Color colFeu = Color.FromArgb(255, (int)(245 * pulseFeu), (int)(125 * pulseFeu), 20);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(colFeu), xFen, yFen, wFen, hFen);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(25, 20, 15), 2f), xFen, yFen, wFen, hFen);
            // Grille de ferronnerie
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.Black, 1.5f), xFen + wFen / 2, yFen, xFen + wFen / 2, yFen + hFen);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.Black, 1.5f), xFen, yFen + hFen / 2, xFen + wFen, yFen + hFen / 2);

            // Enseigne en bois suspendue
            int xEns = p.X + (int)(w * 0.38f);
            int yEns = p.Y + (int)(h * 0.38f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(46, 32, 22)), xEns, yEns, (int)(76 * cam.Zoom), (int)(16 * cam.Zoom));
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Gold, 1f), xEns, yEns, (int)(76 * cam.Zoom), (int)(16 * cam.Zoom));
            g.DrawString("🔨 Brom", FontEnseigne, Brushes.Gold, xEns + 4, yEns + 1);
        }

        private void DessinerGuilde(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            // Façade noble en pierre de taille
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(108, 104, 96)), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(72, 68, 62), 2f), p.X, p.Y, w, h);

            // Lignes de maçonnerie en pierre de taille
            for (int yM = p.Y + 20; yM < p.Y + h; yM += 24)
            {
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(88, 84, 78), 1f), p.X, yM, p.X + w, yM);
            }

            // Toiture majestueuse en ardoise bleu royal avec crête dorée
            int hautToit = (int)(h * 0.50f);
            Point[] polyToit = {
                new Point(p.X - (int)(12 * cam.Zoom), p.Y + hautToit),
                new Point(p.X + w / 2, p.Y - (int)(18 * cam.Zoom)),
                new Point(p.X + w + (int)(12 * cam.Zoom), p.Y + hautToit)
            };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(38, 58, 92)), polyToit);
            g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(24, 38, 64), 2.5f), polyToit);

            // Faîte doré du toit
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(215, 175, 45), 3f), p.X + w / 2 - 20, p.Y - (int)(17 * cam.Zoom), p.X + w / 2 + 20, p.Y - (int)(17 * cam.Zoom));

            // Double vitrail gothique illuminé d'un éclat saphir et améthyste
            int wVit = (int)(w * 0.16f);
            int hVit = (int)(h * 0.34f);
            int yVit = p.Y + (int)(h * 0.54f);

            // Vitrail Gauche (Saphir)
            int xVitG = p.X + (int)(w * 0.12f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(85, 135, 230)), xVitG, yVit, wVit, hVit);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(220, 200, 120), 1.5f), xVitG, yVit, wVit, hVit);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(30, 40, 70), 1f), xVitG + wVit / 2, yVit, xVitG + wVit / 2, yVit + hVit);

            // Vitrail Droit (Améthyste)
            int xVitD = p.X + (int)(w * 0.72f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(170, 90, 220)), xVitD, yVit, wVit, hVit);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(220, 200, 120), 1.5f), xVitD, yVit, wVit, hVit);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 20, 60), 1f), xVitD + wVit / 2, yVit, xVitD + wVit / 2, yVit + hVit);

            // Grand portail en chêne à double battant
            int wPorte = (int)(w * 0.28f);
            int hPorte = (int)(h * 0.44f);
            int xPorte = p.X + (w - wPorte) / 2;
            int yPorte = p.Y + h - hPorte;
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(52, 38, 28)), xPorte, yPorte, wPorte, hPorte);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(215, 175, 45), 2f), xPorte, yPorte, wPorte, hPorte);

            // Bannière héraldique suspendue au centre
            int xBan = p.X + (int)(w * 0.34f);
            int yBan = p.Y + (int)(h * 0.38f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(110, 40, 140)), xBan, yBan, (int)(88 * cam.Zoom), (int)(18 * cam.Zoom));
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Gold, 1.5f), xBan, yBan, (int)(88 * cam.Zoom), (int)(18 * cam.Zoom));
            g.DrawString("📜 Guilde Royale", FontEnseigne, Brushes.Gold, xBan + 4, yBan + 2);
        }

        private void DessinerAlchimie(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            // Façade rustique en colombages et crépi végétal
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(115, 110, 95)), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(65, 50, 35), 2f), p.X, p.Y, w, h);

            // Toiture en bardeaux de bois moussus vert forêt
            int hautToit = (int)(h * 0.48f);
            Point[] polyToit = {
                new Point(p.X - (int)(10 * cam.Zoom), p.Y + hautToit),
                new Point(p.X + w / 2, p.Y - (int)(14 * cam.Zoom)),
                new Point(p.X + w + (int)(10 * cam.Zoom), p.Y + hautToit)
            };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(45, 78, 50)), polyToit);
            g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(30, 55, 34), 2.5f), polyToit);

            // Feuilles de lierre pendantes le long de la corniche
            for (int i = 0; i < 6; i++)
            {
                int xLierre = p.X + (int)(w * (0.12f + i * 0.15f));
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(60, 115, 65)), xLierre, p.Y + hautToit - 2, (int)(10 * cam.Zoom), (int)(14 * cam.Zoom));
            }

            // Oculi magique / Hublot d'apothicaire avec lueur d'émeraude animée
            int rHublot = (int)(22 * cam.Zoom);
            int xHublot = p.X + (int)(w * 0.22f);
            int yHublot = p.Y + (int)(h * 0.62f);
            float pulseAlc = 0.80f + 0.20f * MathF.Sin(tempsTotal * 4f + 1.2f);
            Color colPotion = Color.FromArgb(255, 25, (int)(220 * pulseAlc), (int)(135 * pulseAlc));
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(colPotion), xHublot - rHublot, yHublot - rHublot, rHublot * 2, rHublot * 2);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(45, 35, 25), 2.5f), xHublot - rHublot, yHublot - rHublot, rHublot * 2, rHublot * 2);
            // Fiole d'alchimie visible par transparence
            g.DrawString("⚗️", FontEnseigne, Brushes.White, xHublot - 7, yHublot - 8);

            // Porte en bois avec poignée cuivrée
            int wPorte = (int)(w * 0.28f);
            int hPorte = (int)(h * 0.45f);
            int xPorte = p.X + (int)(w * 0.58f);
            int yPorte = p.Y + h - hPorte;
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(48, 36, 24)), xPorte, yPorte, wPorte, hPorte);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(32, 22, 14), 2f), xPorte, yPorte, wPorte, hPorte);

            // Enseigne suspendue
            int xEns = p.X + (int)(w * 0.40f);
            int yEns = p.Y + (int)(h * 0.38f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(28, 55, 34)), xEns, yEns, (int)(76 * cam.Zoom), (int)(16 * cam.Zoom));
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.LightGreen, 1f), xEns, yEns, (int)(76 * cam.Zoom), (int)(16 * cam.Zoom));
            g.DrawString("🧪 Sylas", FontEnseigne, Brushes.White, xEns + 4, yEns + 1);
        }

        private void DessinerFontaine(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            Point pc = new Point(p.X + w / 2, p.Y + h / 2);
            float rExt = (Boite.Width / 2f + 8f) * cam.Zoom;

            // 1. Bassin extérieur en marbre sculpté
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(145, 150, 160)), pc.X - rExt, pc.Y - rExt, rExt * 2f, rExt * 2f);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(90, 95, 105), 3f), pc.X - rExt, pc.Y - rExt, rExt * 2f, rExt * 2f);

            // 4 Gargouilles cardinales sur le rebord
            for (int a = 0; a < 4; a++)
            {
                float ang = a * MathF.PI / 2f;
                int gx = (int)(pc.X + MathF.Cos(ang) * (rExt - 2));
                int gy = (int)(pc.Y + MathF.Sin(ang) * (rExt - 2));
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(180, 185, 195)), gx - 5, gy - 5, 10, 10);
            }

            // 2. Eau cristalline turquoise
            float rEau = rExt - (7f * cam.Zoom);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(32, 130, 175)), pc.X - rEau, pc.Y - rEau, rEau * 2f, rEau * 2f);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(20, 90, 125), 1.5f), pc.X - rEau, pc.Y - rEau, rEau * 2f, rEau * 2f);

            // 3. Ondulations concentriques d'eau animées (vagues douces)
            for (int ring = 0; ring < 3; ring++)
            {
                float phase = (tempsTotal * 0.75f + ring * 0.333f) % 1.0f;
                float rippleR = (8f * cam.Zoom + phase * (rEau * 0.75f));
                int alpha = (int)((1f - phase) * 170);
                if (alpha > 8)
                {
                    Pen penR = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 200, 245, 255), 1.8f);
                    g.DrawEllipse(penR, pc.X - rippleR, pc.Y - rippleR, rippleR * 2f, rippleR * 2f);
                }
            }

            // 4. Piédestal central surélevé & jet d'eau
            float rCentre = 16f * cam.Zoom;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(175, 180, 190)), pc.X - rCentre, pc.Y - rCentre, rCentre * 2f, rCentre * 2f);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(115, 120, 130), 1.5f), pc.X - rCentre, pc.Y - rCentre, rCentre * 2f, rCentre * 2f);

            // Écume d'eau jaillissante au centre
            float pulseJet = 0.85f + 0.15f * MathF.Sin(tempsTotal * 8f);
            float rJet = 8f * pulseJet * cam.Zoom;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(230, 250, 255)), pc.X - rJet, pc.Y - rJet, rJet * 2f, rJet * 2f);

            // Plaque commémorative gravée au bas de la fontaine
            g.DrawString("Fontaine des Héros", FontPlaque, Brushes.Gold, pc.X - 44, pc.Y + (int)rExt - 2);
        }

        private void DessinerArbre(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            Point pc = new Point(p.X + w / 2, p.Y + h / 2);
            float rCanopy = (Boite.Width * (TypeObstacle == TypeObstacle.ArbreGrand ? 0.95f : 0.80f)) * cam.Zoom;
            float balancement = MathF.Sin(tempsTotal * 1.25f + Boite.X * 0.03f) * 1.6f * cam.Zoom;
            pc.X += (int)balancement;

            // Tronc en bois de chêne texturé
            int wTronc = (int)(16 * cam.Zoom);
            int hTronc = (int)(26 * cam.Zoom);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(72, 48, 30)), pc.X - wTronc / 2, pc.Y - 2, wTronc, hTronc);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(45, 28, 16), 1.5f), pc.X - wTronc / 2, pc.Y - 2, wTronc, hTronc);

            // Couche 1 : Ombre profonde du feuillage (vert très sombre)
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(24, 60, 26)), pc.X - rCanopy + 4, pc.Y - rCanopy + 8, rCanopy * 2f, rCanopy * 1.9f);

            // Couche 2 : Cœur de feuillage forestier vibrant
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(38, 102, 42)), pc.X - rCanopy, pc.Y - rCanopy, rCanopy * 2f, rCanopy * 2f);

            // Sous-amas de feuilles pour donner du volume organique
            float rCluster = rCanopy * 0.58f;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(48, 120, 52)), pc.X - rCanopy * 0.45f, pc.Y - rCanopy * 0.55f, rCluster * 2f, rCluster * 2f);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(44, 115, 48)), pc.X - rCanopy * 0.15f, pc.Y - rCanopy * 0.25f, rCluster * 1.8f, rCluster * 1.8f);

            // Couche 3 : Rehauts lumineux ensoleillés sur le quadrant supérieur gauche
            float rSun = rCanopy * 0.38f;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(72, 155, 72)), pc.X - rCanopy * 0.60f, pc.Y - rCanopy * 0.60f, rSun * 2f, rSun * 2f);

            // Fleurs ou fruits subtils
            int seed = (int)(Boite.X * 13 + Boite.Y * 7);
            if ((seed & 1) == 0)
            {
                Brush bFruit = (seed & 2) == 0 ? Brushes.Tomato : Brushes.LightPink;
                g.FillEllipse(bFruit, pc.X - rCanopy * 0.35f, pc.Y + rCanopy * 0.20f, 5, 5);
                g.FillEllipse(bFruit, pc.X + rCanopy * 0.30f, pc.Y - rCanopy * 0.15f, 5, 5);
                g.FillEllipse(bFruit, pc.X - rCanopy * 0.10f, pc.Y - rCanopy * 0.40f, 5, 5);
            }
        }

        public void DessinerFeuillagePremierPlan(Graphics g, Camera2D cam, float tempsTotal)
        {
            if (TypeObstacle != TypeObstacle.Arbre && TypeObstacle != TypeObstacle.ArbreGrand)
                return;

            Point p = cam.MondeVersEcran(new Vector2(Boite.X, Boite.Y));
            float r = Boite.Width * (TypeObstacle == TypeObstacle.ArbreGrand ? 0.95f : 0.80f) * cam.Zoom;
            float sway = MathF.Sin(tempsTotal * 1.25f + Boite.X * 0.03f) * 1.6f * cam.Zoom;
            float centreX = p.X + Boite.Width * cam.Zoom * 0.5f + sway;
            float centreY = p.Y + Boite.Height * cam.Zoom * 0.5f;
            GraphicsState state = g.Save();
            g.SetClip(new RectangleF(centreX - r, centreY + r * 0.38f, r * 2f, r * 0.58f));
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(24, 60, 26)), centreX - r + 4, centreY - r + 8, r * 2f, r * 1.9f);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(38, 102, 42)), centreX - r, centreY - r, r * 2f, r * 2f);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(48, 120, 52)), centreX - r * 0.45f, centreY - r * 0.55f, r * 1.16f, r * 1.16f);
            g.Restore(state);
        }

        private void DessinerPilierPierre(Graphics g, Camera2D cam, Point p, int w, int h)
        {
            // Base du pilier
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(90, 92, 100)), p.X, p.Y + h - 14, w, 14);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 52, 58), 1.5f), p.X, p.Y + h - 14, w, 14);

            // Fût cannelé
            int xFut = p.X + 6;
            int wFut = Math.Max(6, w - 12);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(120, 122, 130)), xFut, p.Y + 12, wFut, h - 26);
            // Ombrage sur le côté droit
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(75, 77, 85)), xFut + wFut / 2, p.Y + 12, wFut / 2, h - 26);
            // Rehaut sur le côté gauche
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(150, 152, 160)), xFut, p.Y + 12, 3, h - 26);

            // Chapiteau supérieur
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(105, 108, 116)), p.X - 2, p.Y, w + 4, 14);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 52, 58), 1.5f), p.X - 2, p.Y, w + 4, 14);
        }

        private void DessinerPilierAstral(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            // Monolithe sombre en obsidienne
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(22, 24, 38)), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(60, 140, 220), 2f), p.X, p.Y, w, h);

            // Glyphes stellaires gravés le long du pilier
            Pen penRune = CacheRenduGDI.ObtenirPen(Color.FromArgb(120, 80, 220, 255), 1.5f);
            for (int yR = p.Y + 15; yR < p.Y + h - 15; yR += 20)
            {
                g.DrawLine(penRune, p.X + 8, yR, p.X + w - 8, yR);
                g.DrawEllipse(penRune, p.X + w / 2 - 3, yR - 3, 6, 6);
            }

            // Cristal céleste en lévitation au-dessus du pilier
            float yFlottant = MathF.Sin(tempsTotal * 3f + Boite.X * 0.1f) * 6f;
            Point pCristal = new Point(p.X + w / 2, (int)(p.Y - 18 + yFlottant));
            int rC = (int)(10 * cam.Zoom);
            Point[] polyCristal = {
                new Point(pCristal.X, pCristal.Y - rC),
                new Point(pCristal.X + rC, pCristal.Y),
                new Point(pCristal.X, pCristal.Y + rC),
                new Point(pCristal.X - rC, pCristal.Y)
            };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(140, 240, 255)), polyCristal);
            g.DrawPolygon(Pens.White, polyCristal);
        }

        private void DessinerLampadaire(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            Point pc = new Point(p.X + w / 2, p.Y + h / 2);

            // Socle en fonte noire
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(32, 34, 38)), pc.X - 5, p.Y + h - 6, 10, 6);
            // Poteau en fer forgé
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(24, 26, 30), 2.5f), pc.X, p.Y + h - 6, pc.X, p.Y + 6);

            // Cage de lanterne en verre
            int wL = (int)(14 * cam.Zoom);
            int hL = (int)(18 * cam.Zoom);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(180, 255, 230, 100)), pc.X - wL / 2, p.Y, wL, hL);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(35, 36, 40), 1.5f), pc.X - wL / 2, p.Y, wL, hL);

            // Flamme vacillante au centre
            float flicker = 0.85f + 0.15f * MathF.Sin(tempsTotal * 9f + Boite.X);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(255, 240, (int)(180 * flicker), 30)), pc.X - 3, p.Y + hL / 2 - 4, 6, 8);
        }

        private void DessinerBrasero(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            Point pc = new Point(p.X + w / 2, p.Y + h / 2);

            // Trépied et vasque en fer
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(45, 40, 42)), pc.X - w / 2, pc.Y - 4, w, h / 2);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(25, 20, 22), 2f), pc.X - w / 2, pc.Y - 4, w, h / 2);

            // Braises ardentes
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(180, 50, 20)), pc.X - w / 2 + 3, pc.Y - 2, w - 6, h / 2 - 4);

            // Flammes animées
            float f = 0.80f + 0.20f * MathF.Sin(tempsTotal * 8f + Boite.X);
            int hautFeu = (int)(16 * f * cam.Zoom);
            Point[] polyFeu = {
                new Point(pc.X - (int)(6 * cam.Zoom), pc.Y),
                new Point(pc.X, pc.Y - hautFeu),
                new Point(pc.X + (int)(6 * cam.Zoom), pc.Y)
            };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(255, 210, 40)), polyFeu);
        }

        private void DessinerBanc(Graphics g, Camera2D cam, Point p, int w, int h)
        {
            // Banc en calcaire taillé
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(135, 138, 145)), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(80, 82, 88), 1.5f), p.X, p.Y, w, h);
            // Pieds
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(100, 102, 108)), p.X + 4, p.Y + h, 8, 4);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(100, 102, 108)), p.X + w - 12, p.Y + h, 8, 4);
        }

        private void DessinerCaissesTonneaux(Graphics g, Camera2D cam, Point p, int w, int h)
        {
            // Tonneau en chêne
            int rT = Math.Min(w, h) / 2;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(92, 64, 42)), p.X, p.Y, rT * 2, rT * 2);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(48, 30, 18), 2f), p.X, p.Y, rT * 2, rT * 2);
            // Cerclages métalliques
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(150, 155, 165), 1.5f), p.X + 3, p.Y + 3, rT * 2 - 6, rT * 2 - 6);

            // Caisse à côté
            int xC = p.X + rT;
            int yC = p.Y + 2;
            int sC = Math.Max(12, rT);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(115, 85, 55)), xC, yC, sC, sC);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(60, 42, 26), 1.5f), xC, yC, sC, sC);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(60, 42, 26), 1.5f), xC, yC, xC + sC, yC + sC);
        }

        private void DessinerRempart(Graphics g, Camera2D cam, Point p, int w, int h)
        {
            // Mur de fortification en granit
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(52, 56, 64)), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(35, 38, 44), 2f), p.X, p.Y, w, h);

            // Créneaux défensifs le long du mur
            if (w > h) // Mur horizontal
            {
                int pas = (int)(28 * cam.Zoom);
                for (int xM = p.X + 4; xM < p.X + w - pas; xM += pas * 2)
                {
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(70, 75, 85)), xM, p.Y - 5, pas, 5);
                    g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(35, 38, 44), 1.5f), xM, p.Y - 5, pas, 5);
                }
            }
            else // Mur vertical
            {
                int pas = (int)(28 * cam.Zoom);
                for (int yM = p.Y + 4; yM < p.Y + h - pas; yM += pas * 2)
                {
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(70, 75, 85)), p.X - 5, yM, 5, pas);
                    g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(35, 38, 44), 1.5f), p.X - 5, yM, 5, pas);
                }
            }
        }

        private void DessinerTaverneDragonVert(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            // Façade 1er étage en colombages chaleureux et crépi blanc cassé (Image 1)
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(235, 228, 210)), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(75, 45, 25), 2f), p.X, p.Y, w, h);

            // Colombages en bois foncé
            Pen pBois = CacheRenduGDI.ObtenirPen(Color.FromArgb(70, 42, 22), 3f * cam.Zoom);
            int pasPoteaux = Math.Max(1, (int)(w / 4f));
            for (int i = 1; i < 4; i++)
            {
                g.DrawLine(pBois, p.X + i * pasPoteaux, p.Y, p.X + i * pasPoteaux, p.Y + h);
            }
            g.DrawLine(pBois, p.X, p.Y + (int)(h * 0.52f), p.X + w, p.Y + (int)(h * 0.52f));
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(70, 42, 22), 2f * cam.Zoom), p.X, p.Y + (int)(h * 0.52f), p.X + pasPoteaux, p.Y);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(70, 42, 22), 2f * cam.Zoom), p.X + w - pasPoteaux, p.Y + (int)(h * 0.52f), p.X + w, p.Y);

            // Toiture en tuiles bois brunes / ardoise rustique
            int hautToit = (int)(h * 0.46f);
            Point[] polyToit = {
                new Point(p.X - (int)(12 * cam.Zoom), p.Y + hautToit),
                new Point(p.X + w / 2, p.Y - (int)(16 * cam.Zoom)),
                new Point(p.X + w + (int)(12 * cam.Zoom), p.Y + hautToit)
            };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(142, 68, 45)), polyToit);
            g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(85, 38, 22), 2.5f), polyToit);

            // Fenêtres à croisillons et jardinières fleuries
            int wFen = (int)(28 * cam.Zoom);
            int hFen = (int)(24 * cam.Zoom);
            for (int f = 1; f <= 2; f++)
            {
                int xF = p.X + (int)(w * (f == 1 ? 0.22f : 0.68f)) - wFen / 2;
                int yF = p.Y + (int)(h * 0.18f);
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(255, 230, 150)), xF, yF, wFen, hFen);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(60, 35, 18), 1.5f), xF, yF, wFen, hFen);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(60, 35, 18), 1f), xF + wFen / 2, yF, xF + wFen / 2, yF + hFen);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(60, 35, 18), 1f), xF, yF + hFen / 2, xF + wFen, yF + hFen / 2);
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(92, 54, 30)), xF - 2, yF + hFen, wFen + 4, 5);
                g.FillEllipse(Brushes.Crimson, xF + 2, yF + hFen - 2, 5, 5);
                g.FillEllipse(Brushes.Gold, xF + 10, yF + hFen - 2, 5, 5);
                g.FillEllipse(Brushes.Crimson, xF + 18, yF + hFen - 2, 5, 5);
            }

            // Cheminée en pierre avec braise
            int xChem = p.X + (int)(18 * cam.Zoom);
            int yChem = p.Y - (int)(22 * cam.Zoom);
            int wChem = (int)(28 * cam.Zoom);
            int hChem = (int)(42 * cam.Zoom);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(90, 85, 80)), xChem, yChem, wChem, hChem);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 48, 45), 2f), xChem, yChem, wChem, hChem);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(240, 100, 20)), xChem + 3, yChem, wChem - 6, 4);

            // Grande Porte cintrée d'auberge
            int wPorte = (int)(w * 0.24f);
            int hPorte = (int)(h * 0.42f);
            int xPorte = p.X + (int)(w * 0.38f);
            int yPorte = p.Y + h - hPorte;
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(65, 42, 25)), xPorte, yPorte, wPorte, hPorte);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(38, 24, 14), 2f), xPorte, yPorte, wPorte, hPorte);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.Black, 2f), xPorte + 3, yPorte + 8, xPorte + wPorte - 4, yPorte + 8);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.Black, 2f), xPorte + 3, yPorte + hPorte - 10, xPorte + wPorte - 4, yPorte + hPorte - 10);

            // Enseigne emblématique "LE DRAGON VERT" suspendue
            int xEns = p.X + (int)(w * 0.22f);
            int yEns = p.Y + (int)(h * 0.44f);
            int wEns = (int)(140 * cam.Zoom);
            int hEns = (int)(20 * cam.Zoom);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(34, 110, 52)), xEns, yEns, wEns, hEns);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Gold, 2f), xEns, yEns, wEns, hEns);
            g.DrawString("🐉 LE DRAGON VERT", FontEnseigne, Brushes.Gold, xEns + 4, yEns + 2);

            // Tonneau et banc à l'entrée
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(95, 62, 38)), xPorte - (int)(22 * cam.Zoom), yPorte + hPorte - (int)(18 * cam.Zoom), (int)(18 * cam.Zoom), (int)(18 * cam.Zoom));
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 32, 18), 1.5f), xPorte - (int)(22 * cam.Zoom), yPorte + hPorte - (int)(18 * cam.Zoom), (int)(18 * cam.Zoom), (int)(18 * cam.Zoom));
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(120, 80, 50)), xPorte + wPorte + (int)(6 * cam.Zoom), yPorte + hPorte - (int)(12 * cam.Zoom), (int)(32 * cam.Zoom), (int)(10 * cam.Zoom));
        }

        private void DessinerEtalMarche(Graphics g, Camera2D cam, Point p, int w, int h)
        {
            int yComptoir = p.Y + (int)(h * 0.48f);
            int hComptoir = (int)(h * 0.52f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(115, 80, 50)), p.X + 4, yComptoir, w - 8, hComptoir);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(65, 42, 25), 1.5f), p.X + 4, yComptoir, w - 8, hComptoir);

            Pen pPoteau = CacheRenduGDI.ObtenirPen(Color.FromArgb(80, 50, 30), 2.5f);
            g.DrawLine(pPoteau, p.X + 6, p.Y + 8, p.X + 6, yComptoir);
            g.DrawLine(pPoteau, p.X + w - 6, p.Y + 8, p.X + w - 6, yComptoir);

            bool estBleu = Nom.Contains("Bleu") || Nom.Contains("Épices");
            Color couleurBande = estBleu ? Color.FromArgb(41, 128, 185) : Color.FromArgb(231, 76, 60);
            int hautToile = (int)(h * 0.42f);
            int nbBandes = 6;
            float wBande = (float)w / nbBandes;

            for (int b = 0; b < nbBandes; b++)
            {
                Color col = (b % 2 == 0) ? couleurBande : Color.WhiteSmoke;
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(col), p.X + b * wBande, p.Y, wBande + 1, hautToile);
                g.FillPie(CacheRenduGDI.ObtenirBrush(col), p.X + b * wBande, p.Y + hautToile - 5, wBande, 10, 0f, 180f);
            }
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(40, 40, 40), 1.5f), p.X, p.Y, w, hautToile);

            int nbCaisses = 3;
            float wCaisse = (w - 16f) / nbCaisses;
            for (int c = 0; c < nbCaisses; c++)
            {
                float xC = p.X + 8 + c * wCaisse;
                float yC = yComptoir + 4;
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(135, 95, 60)), xC, yC, wCaisse - 4, (int)(16 * cam.Zoom));
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(70, 45, 25), 1f), xC, yC, wCaisse - 4, (int)(16 * cam.Zoom));

                Color colContenu = (c == 0) ? Color.FromArgb(231, 76, 60) : (c == 1) ? Color.FromArgb(46, 204, 113) : Color.FromArgb(230, 126, 34);
                for (int f = 0; f < 3; f++)
                {
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(colContenu), xC + 2 + f * (wCaisse / 3.8f), yC + 2, (int)(6 * cam.Zoom), (int)(6 * cam.Zoom));
                }
            }
        }

        private void DessinerCharretteAttellee(Graphics g, Camera2D cam, Point p, int w, int h)
        {
            int wCharrette = (int)(w * 0.58f);
            int hBache = (int)(h * 0.55f);
            g.FillPie(CacheRenduGDI.ObtenirBrush(Color.FromArgb(240, 236, 226)), p.X, p.Y, wCharrette, hBache * 2, 180f, 180f);
            g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(140, 130, 120), 2f), p.X, p.Y, wCharrette, hBache * 2, 180f, 180f);

            int yPlancher = p.Y + hBache;
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(105, 72, 44)), p.X, yPlancher, wCharrette, (int)(18 * cam.Zoom));
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(55, 36, 20), 1.5f), p.X, yPlancher, wCharrette, (int)(18 * cam.Zoom));

            int rRoue = (int)(14 * cam.Zoom);
            int yRoue = yPlancher + (int)(8 * cam.Zoom);
            int xRoue1 = p.X + (int)(12 * cam.Zoom);
            int xRoue2 = p.X + wCharrette - (int)(16 * cam.Zoom);
            foreach (int xR in new[] { xRoue1, xRoue2 })
            {
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(130, 90, 55)), xR - rRoue, yRoue - rRoue, rRoue * 2, rRoue * 2);
                g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 32, 18), 2.5f), xR - rRoue, yRoue - rRoue, rRoue * 2, rRoue * 2);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 32, 18), 1.5f), xR - rRoue, yRoue, xR + rRoue, yRoue);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 32, 18), 1.5f), xR, yRoue - rRoue, xR, yRoue + rRoue);
            }

            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(90, 60, 35), 3f), p.X + wCharrette, yPlancher + 4, p.X + wCharrette + (int)(24 * cam.Zoom), yPlancher + 4);

            int xBoeuf = p.X + wCharrette + (int)(14 * cam.Zoom);
            int yBoeuf = yPlancher - (int)(10 * cam.Zoom);
            for (int b = 0; b < 2; b++)
            {
                int ox = xBoeuf + b * (int)(18 * cam.Zoom);
                int oy = yBoeuf + b * (int)(6 * cam.Zoom);
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(135, 75, 40)), ox, oy, (int)(24 * cam.Zoom), (int)(18 * cam.Zoom));
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(115, 60, 30)), ox + (int)(18 * cam.Zoom), oy + (int)(2 * cam.Zoom), (int)(12 * cam.Zoom), (int)(12 * cam.Zoom));
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(235, 210, 190)), ox + (int)(24 * cam.Zoom), oy + (int)(6 * cam.Zoom), (int)(7 * cam.Zoom), (int)(7 * cam.Zoom));
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(230, 230, 220), 1.5f), ox + (int)(22 * cam.Zoom), oy + (int)(2 * cam.Zoom), ox + (int)(26 * cam.Zoom), oy - (int)(4 * cam.Zoom));
            }
        }

        private void DessinerPontPierre(Graphics g, Camera2D cam, Point p, int w, int h, float tempsTotal)
        {
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(42, 125, 175)), p.X, p.Y, w, h);
            float phaseEau = MathF.Sin(tempsTotal * 2.5f) * 4f;
            Pen pOndul = CacheRenduGDI.ObtenirPen(Color.FromArgb(160, 210, 245), 1.5f);
            g.DrawLine(pOndul, p.X, p.Y + (int)(h * 0.35f) + phaseEau, p.X + w, p.Y + (int)(h * 0.35f) + phaseEau);
            g.DrawLine(pOndul, p.X, p.Y + (int)(h * 0.65f) - phaseEau, p.X + w, p.Y + (int)(h * 0.65f) - phaseEau);

            int yTablier = p.Y + (int)(h * 0.22f);
            int hTablier = (int)(h * 0.56f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(110, 115, 125)), p.X, yTablier, w, hTablier);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(65, 70, 78), 2f), p.X, yTablier, w, hTablier);

            for (int xPav = p.X + 8; xPav < p.X + w - 8; xPav += 16)
            {
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(85, 90, 100), 1f), xPav, yTablier + 4, xPav, yTablier + hTablier - 4);
            }

            int hParapet = (int)(8 * cam.Zoom);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(80, 85, 95)), p.X, yTablier, w, hParapet);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(80, 85, 95)), p.X, yTablier + hTablier - hParapet, w, hParapet);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 52, 60), 1.5f), p.X, yTablier, w, hParapet);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(50, 52, 60), 1.5f), p.X, yTablier + hTablier - hParapet, w, hParapet);

            int xGarde = p.X + w / 2;
            int yGarde = yTablier + hTablier / 2;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(231, 76, 60)), xGarde - 6, yGarde - 8, 12, 14);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(170, 175, 185)), xGarde - 4, yGarde - 16, 8, 8);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(80, 50, 20), 2f), xGarde + 6, yGarde - 22, xGarde + 6, yGarde + 10);
            Point[] ferLance = { new Point(xGarde + 6, yGarde - 26), new Point(xGarde + 4, yGarde - 20), new Point(xGarde + 8, yGarde - 20) };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.Silver), ferLance);
        }
    }

    public class PouleVillageoise
    {
        public Vector2 Position;
        public Vector2 Velocite;
        public float TempsPicorage = 0f;
        public float TempsMarche = 0f;
        public bool DirectionGauche = false;
        public int TypeCouleur = 0; // 0 = Blanc, 1 = Roux/Doré, 2 = Tacheté
        private static readonly Random rng = Random.Shared;

        public PouleVillageoise(Vector2 pos, int couleur = 0)
        {
            Position = pos;
            TypeCouleur = couleur;
            TempsPicorage = rng.NextSingle() * 2f;
        }

        public void MettreAJour(float dt, Joueur2D joueur, float largeurMonde, float hauteurMonde)
        {
            float distJoueur = Vector2.Distance(Position, joueur.Position);
            if (distJoueur < 75f)
            {
                Vector2 fuite = (Position - joueur.Position).Normaliser();
                Velocite = fuite * 110f;
                DirectionGauche = fuite.X < 0;
                TempsPicorage = 1.5f;
            }
            else
            {
                TempsPicorage -= dt;
                if (TempsPicorage <= 0f)
                {
                    TempsPicorage = 2.5f + rng.NextSingle() * 3f;
                    if (rng.Next(2) == 0)
                    {
                        float angle = rng.NextSingle() * MathF.Tau;
                        Velocite = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (25f + rng.NextSingle() * 20f);
                        DirectionGauche = Velocite.X < 0;
                        TempsMarche = 1.0f + rng.NextSingle() * 1.5f;
                    }
                    else
                    {
                        Velocite = Vector2.Zero;
                        TempsMarche = 0f;
                    }
                }

                if (TempsMarche > 0f)
                {
                    TempsMarche -= dt;
                    if (TempsMarche <= 0f) Velocite = Vector2.Zero;
                }
            }

            Position += Velocite * dt;
            Position.X = Math.Clamp(Position.X, 100f, largeurMonde - 100f);
            Position.Y = Math.Clamp(Position.Y, 150f, hauteurMonde - 150f);
            Velocite *= MathF.Max(0f, 1f - 4f * dt);
        }

        public void Dessiner(Graphics g, Camera2D cam, float tempsTotal)
        {
            if (!cam.EstVisible(Position, 30f)) return;
            Point p = cam.MondeVersEcran(Position);

            g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(70, Color.Black), p.X - 5 * cam.Zoom, p.Y + 3 * cam.Zoom, 10 * cam.Zoom, 4 * cam.Zoom);

            Color corpsCol = TypeCouleur switch
            {
                1 => Color.FromArgb(215, 130, 45),
                2 => Color.FromArgb(170, 160, 150),
                _ => Color.FromArgb(245, 245, 245)
            };

            float bobY = Velocite.LongueurCarree() > 10f ? MathF.Abs(MathF.Sin(tempsTotal * 14f)) * 3f : 0f;
            float picoreY = (TempsPicorage > 0.5f && TempsPicorage < 1.8f && Velocite.LongueurCarree() < 5f) ? MathF.Sin(tempsTotal * 8f) * 2f : 0f;

            int dirSign = DirectionGauche ? -1 : 1;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(corpsCol), p.X - 5 * cam.Zoom, p.Y - (6 + bobY) * cam.Zoom, 10 * cam.Zoom, 8 * cam.Zoom);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(corpsCol), p.X + (3 * dirSign - 3) * cam.Zoom, p.Y - (10 + bobY - picoreY) * cam.Zoom, 6 * cam.Zoom, 6 * cam.Zoom);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.Red), p.X + (3 * dirSign - 2) * cam.Zoom, p.Y - (12 + bobY - picoreY) * cam.Zoom, 4 * cam.Zoom, 3 * cam.Zoom);
            PointF[] bec = {
                new PointF(p.X + (6 * dirSign) * cam.Zoom, p.Y - (8 + bobY - picoreY) * cam.Zoom),
                new PointF(p.X + (9 * dirSign) * cam.Zoom, p.Y - (7 + bobY - picoreY) * cam.Zoom),
                new PointF(p.X + (6 * dirSign) * cam.Zoom, p.Y - (6 + bobY - picoreY) * cam.Zoom)
            };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.Gold), bec);
        }
    }

    public enum TypeInteractif { NPC_Brom, NPC_Elenora, NPC_Marchand, NPC_Artisan, NPC_Aubergiste, NPC_Capitaine, PortailDonjon, PortailTour, PortailRetour, Coffre, Mannequin, PortailProchainEtage }

    public class ObjetInteractif2D
    {
        private Vector2 positionAncrage;
        private float phasePatrouille;

        public Vector2 Position;
        public float Rayon = 28f;
        public TypeInteractif Type;
        public string Titre = "";
        public string Description = "";
        public bool EstOuvert = false;

        public ObjetInteractif2D(Vector2 pos, TypeInteractif type, string titre, string desc = "")
        {
            Position = pos;
            positionAncrage = pos;
            phasePatrouille = ((int)type * 1.37f) + pos.X * 0.013f + pos.Y * 0.007f;
            Type = type;
            Titre = titre;
            Description = desc;
        }

        public void ReinitialiserPositionAncrage()
        {
            positionAncrage = Position;
        }

        public void MettreAJour(float tempsTotal, bool estDansVillage)
        {
            if (!estDansVillage || !EstPNJ())
            {
                Position = positionAncrage;
                return;
            }

            float phase = tempsTotal * 0.42f + phasePatrouille;
            Position = positionAncrage + new Vector2(MathF.Sin(phase) * 10f, MathF.Sin(phase * 1.8f) * 2.5f);
        }

        private bool EstPNJ() => Type is TypeInteractif.NPC_Brom or TypeInteractif.NPC_Artisan or
            TypeInteractif.NPC_Elenora or TypeInteractif.NPC_Marchand or TypeInteractif.NPC_Aubergiste or
            TypeInteractif.NPC_Capitaine;

        private static readonly Font FontEmojiGrand = new Font("Segoe UI", 13f);
        private static readonly Font FontEmojiMoyen = new Font("Segoe UI", 11f);
        private static readonly Font FontEmojiPetit = new Font("Segoe UI", 10f);
        private static readonly Font FontInfoBulle = new Font("Segoe UI", 8.5f, FontStyle.Bold);

        public void Dessiner(Graphics g, Camera2D cam, bool joueurProche, float tempsTotal = 0f)
        {
            if (!cam.EstVisible(Position, Rayon * 3f)) return;

            Point p = cam.MondeVersEcran(Position);

            // Ombre au sol sous l'objet interactif
            Brush bOmbreSol = CacheRenduGDI.ObtenirBrushAlpha(80, Color.FromArgb(10, 12, 16));
            g.FillEllipse(bOmbreSol, p.X - Rayon * 0.7f, p.Y + Rayon * 0.35f, Rayon * 1.4f, Rayon * 0.65f);

            // Halo d'interaction si le joueur est à portée
            if (joueurProche)
            {
                Brush bHalo = CacheRenduGDI.ObtenirBrush(Color.FromArgb(55, 241, 196, 15));
                g.FillEllipse(bHalo, p.X - Rayon * 1.5f, p.Y - Rayon * 1.5f, Rayon * 3f, Rayon * 3f);
            }

            // Dessin selon le type
            switch (Type)
            {
                case TypeInteractif.NPC_Brom:
                    DessinerPNJHumanoid(g, p, Color.FromArgb(215, 90, 15), Color.FromArgb(160, 80, 50), "🔨", tempsTotal, TypeInteractif.NPC_Brom);
                    break;

                case TypeInteractif.NPC_Artisan:
                    DessinerPNJHumanoid(g, p, Color.FromArgb(20, 145, 215), Color.FromArgb(40, 100, 180), "⚒️", tempsTotal, TypeInteractif.NPC_Artisan);
                    break;

                case TypeInteractif.NPC_Elenora:
                    DessinerPNJHumanoid(g, p, Color.FromArgb(145, 65, 175), Color.FromArgb(100, 40, 120), "📜", tempsTotal, TypeInteractif.NPC_Elenora);
                    break;

                case TypeInteractif.NPC_Marchand:
                    DessinerPNJHumanoid(g, p, Color.FromArgb(40, 175, 100), Color.FromArgb(30, 120, 70), "🧪", tempsTotal, TypeInteractif.NPC_Marchand);
                    break;

                case TypeInteractif.NPC_Aubergiste:
                    DessinerPNJHumanoid(g, p, Color.FromArgb(185, 125, 55), Color.FromArgb(120, 75, 35), "🍲", tempsTotal, TypeInteractif.NPC_Aubergiste);
                    break;

                case TypeInteractif.NPC_Capitaine:
                    DessinerPNJHumanoid(g, p, Color.FromArgb(70, 100, 175), Color.FromArgb(40, 60, 120), "🛡️", tempsTotal, TypeInteractif.NPC_Capitaine);
                    break;

                case TypeInteractif.PortailDonjon:
                    // Vortex Donjon : anneau ardent écarlate et or
                    float rPortailD = 26f * cam.Zoom;
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(210, 45, 30)), p.X - rPortailD, p.Y - rPortailD, rPortailD * 2, rPortailD * 2);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(241, 196, 15), 3.5f), p.X - rPortailD, p.Y - rPortailD, rPortailD * 2, rPortailD * 2);
                    g.DrawString("🌀", FontEmojiGrand, Brushes.White, p.X - 13, p.Y - 14);
                    break;

                case TypeInteractif.PortailTour:
                    // Tour Astrale : vortex céleste cyan stellaire
                    float rPortailT = 26f * cam.Zoom;
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(35, 105, 190)), p.X - rPortailT, p.Y - rPortailT, rPortailT * 2, rPortailT * 2);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.Cyan, 3.5f), p.X - rPortailT, p.Y - rPortailT, rPortailT * 2, rPortailT * 2);
                    g.DrawString("⭐", FontEmojiGrand, Brushes.White, p.X - 13, p.Y - 14);
                    break;

                case TypeInteractif.PortailProchainEtage:
                    // Portail montant vers l'étage suivant de la Tour Infinie
                    float rPortailP = 28f * cam.Zoom;
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(145, 65, 180)), p.X - rPortailP, p.Y - rPortailP, rPortailP * 2, rPortailP * 2);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(241, 196, 15), 4f), p.X - rPortailP, p.Y - rPortailP, rPortailP * 2, rPortailP * 2);
                    g.DrawString("⚡", FontEmojiGrand, Brushes.White, p.X - 13, p.Y - 14);
                    break;

                case TypeInteractif.PortailRetour:
                    // Portail de retour au village
                    float rPortailR = 24f * cam.Zoom;
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(40, 185, 105)), p.X - rPortailR, p.Y - rPortailR, rPortailR * 2, rPortailR * 2);
                    g.DrawEllipse(Pens.White, p.X - rPortailR, p.Y - rPortailR, rPortailR * 2, rPortailR * 2);
                    g.DrawString("🚪", FontEmojiMoyen, Brushes.White, p.X - 11, p.Y - 12);
                    break;

                case TypeInteractif.Coffre:
                    // Coffre à trésor en bois ferré
                    Color colCoffre = EstOuvert ? Color.FromArgb(127, 140, 141) : Color.FromArgb(235, 175, 25);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(colCoffre), p.X - 16, p.Y - 12, 32, 24);
                    g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(40, 30, 20), 2f), p.X - 16, p.Y - 12, 32, 24);
                    // Ferrures d'angles
                    g.FillRectangle(Brushes.Silver, p.X - 16, p.Y - 12, 5, 24);
                    g.FillRectangle(Brushes.Silver, p.X + 11, p.Y - 12, 5, 24);
                    g.DrawString(EstOuvert ? "📦" : "💎", FontEmojiPetit, Brushes.White, p.X - 9, p.Y - 11);
                    break;

                case TypeInteractif.Mannequin:
                    // Mannequin d'entraînement : socle en bois & paille avec cible
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(90, 60, 35)), p.X - 4, p.Y + 8, 8, 12);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(220, 190, 145)), p.X - 15, p.Y - 15, 30, 30);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.SaddleBrown, 2f), p.X - 15, p.Y - 15, 30, 30);
                    // Anneau de cible concentrique
                    g.DrawEllipse(Pens.Red, p.X - 9, p.Y - 9, 18, 18);
                    g.DrawString("🎯", FontEmojiPetit, Brushes.Black, p.X - 9, p.Y - 10);
                    break;
            }

            // Info-bulle au-dessus de la tête
            string info = joueurProche ? $"[E] {Titre}" : Titre;
            SizeF sz = g.MeasureString(info, FontInfoBulle);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(180, 12, 14, 20)), p.X - sz.Width / 2f - 5, p.Y - 38, sz.Width + 10, 18);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(joueurProche ? Color.Gold : Color.FromArgb(80, 85, 100), 1f), p.X - sz.Width / 2f - 5, p.Y - 38, sz.Width + 10, 18);
            Brush bTexte = joueurProche ? Brushes.Gold : Brushes.White;
            g.DrawString(info, FontInfoBulle, bTexte, p.X - sz.Width / 2f, p.Y - 37);
        }

        private void DessinerPNJHumanoid(Graphics g, Point p, Color couleurVetement, Color couleurOmbre, string emoji, float tempsTotal, TypeInteractif type)
        {
            SolidBrush bVetement = CacheRenduGDI.ObtenirBrush(couleurVetement);
            Pen penContour = CacheRenduGDI.ObtenirPen(Color.FromArgb(20, 20, 20), 1.5f);

            float rythmeTravail = type switch
            {
                TypeInteractif.NPC_Brom => 6.8f,
                TypeInteractif.NPC_Artisan => 2.6f,
                TypeInteractif.NPC_Elenora => 1.4f,
                TypeInteractif.NPC_Marchand => 3.8f,
                TypeInteractif.NPC_Aubergiste => 2.1f,
                _ => 0.85f
            };
            float amplitudeTravail = type switch
            {
                TypeInteractif.NPC_Brom => 1.2f,
                TypeInteractif.NPC_Artisan => 0.85f,
                TypeInteractif.NPC_Elenora => 0.22f,
                TypeInteractif.NPC_Marchand => 0.7f,
                TypeInteractif.NPC_Aubergiste => 0.48f,
                _ => 0.12f
            };
            float phaseRole = (int)type * 0.83f;
            float respiration = MathF.Sin(tempsTotal * 2.2f + phaseRole) * (type == TypeInteractif.NPC_Capitaine ? 1f : 1.5f);
            float regardAnime = MathF.Sin(tempsTotal * (type == TypeInteractif.NPC_Capitaine ? 0.9f : 1.2f) + phaseRole) * (type == TypeInteractif.NPC_Capitaine ? 3f : 1.5f);
            float animBrasTravail = MathF.Sin(tempsTotal * rythmeTravail + phaseRole) * amplitudeTravail;

            Pen penJambe = CacheRenduGDI.ObtenirPenArrondi(Color.FromArgb(50, 40, 40), 5.5f);
            Pen penBotte = CacheRenduGDI.ObtenirPenArrondi(Color.FromArgb(30, 25, 20), 6f);
            Pen penBras = CacheRenduGDI.ObtenirPenArrondi(couleurVetement, 5f);

            float hancheY = p.Y + 6;
            
            // Jambes (Statiques avec stance légère)
            g.DrawLine(penJambe, p.X - 4, hancheY, p.X - 5, hancheY + 6);
            g.DrawLine(penBotte, p.X - 5, hancheY + 6, p.X - 6, hancheY + 10);
            g.DrawLine(penJambe, p.X + 4, hancheY, p.X + 5, hancheY + 6);
            g.DrawLine(penBotte, p.X + 5, hancheY + 6, p.X + 6, hancheY + 10);

            // Torse arrondi et respirant
            g.FillPie(CacheRenduGDI.ObtenirBrush(couleurOmbre), p.X - 9, p.Y - 6 - respiration, 18, 18, 180, 180);
            g.FillRectangle(bVetement, p.X - 8, p.Y - 2 - respiration, 16, 11);
            g.DrawArc(penContour, p.X - 9, p.Y - 6 - respiration, 18, 18, 180, 180);
            g.DrawLine(penContour, p.X - 8, p.Y - 2 - respiration, p.X - 8, p.Y + 9 - respiration);
            g.DrawLine(penContour, p.X + 8, p.Y - 2 - respiration, p.X + 8, p.Y + 9 - respiration);
            
            // Ceinture
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(60, 30, 15)), p.X - 8, p.Y + 5 - respiration, 16, 3);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.Silver), p.X - 2, p.Y + 4 - respiration, 4, 5);

            // Bras Gauche (Repos)
            g.DrawLine(penBras, p.X - 9, p.Y - 2 - respiration, p.X - 11, p.Y + 5 - respiration);

            // Bras Droit travaillant (IK simplifiée)
            float epauleDx = p.X + 9; float epauleDy = p.Y - 2 - respiration;
            float coudeDx = epauleDx + 3f; float coudeDy = epauleDy + 5f;
            float mainDx = coudeDx + MathF.Sin(animBrasTravail) * 6f; float mainDy = coudeDy - MathF.Cos(animBrasTravail) * 6f;
            g.DrawLine(penBras, epauleDx, epauleDy, coudeDx, coudeDy);
            
            Pen penAvantBras = CacheRenduGDI.ObtenirPenArrondi(Color.FromArgb(240, 200, 160), 4.5f);
            g.DrawLine(penAvantBras, coudeDx, coudeDy, mainDx, mainDy);

            // Tête
            SolidBrush bPeau = CacheRenduGDI.ObtenirBrush(Color.FromArgb(240, 200, 160));
            g.FillEllipse(bPeau, p.X - 8, p.Y - 20 - respiration, 16, 16);
            g.DrawEllipse(penContour, p.X - 8, p.Y - 20 - respiration, 16, 16);

            // Capuche / Cheveux
            g.FillPie(CacheRenduGDI.ObtenirBrush(couleurOmbre), p.X - 9, p.Y - 22 - respiration, 18, 16, 160, 220);
            g.DrawArc(penContour, p.X - 9, p.Y - 22 - respiration, 18, 16, 160, 220);

            // Yeux
            SolidBrush bYeux = CacheRenduGDI.ObtenirBrush(Color.FromArgb(40, 40, 40));
            g.FillEllipse(bYeux, p.X - 4 + regardAnime, p.Y - 14 - respiration, 2.5f, 2.5f);
            g.FillEllipse(bYeux, p.X + 2 + regardAnime, p.Y - 14 - respiration, 2.5f, 2.5f);

            // Outil
            g.DrawString(emoji, FontEmojiPetit, Brushes.White, mainDx - 8, mainDy - 12);
        }
    }

    // ==============================================================
    // ZONES DE DANGER AU SOL (TÉLÉGRAPHES DE SORTS & CHARGES DE BOSS)
    // ==============================================================
    public enum FormeZoneDanger
    {
        Cercle,
        LigneCharge,
        ConeSouffle
    }

    public class ZoneDanger2D
    {
        public Vector2 Position;
        public float Rayon = 70f;
        public float TempsRestant;
        public float TempsTotal;
        public int Degats;
        public Color Couleur = Color.Red;
        public string NomSort = "";
        public bool EstExecutee = false;

        public FormeZoneDanger Forme = FormeZoneDanger.Cercle;
        public Vector2 Direction = new Vector2(1, 0);
        public float LongueurLigne = 360f;
        public float LargeurLigne = 70f;
        public float AngleOuverture = 60f; // degrés

        public void MettreAJour(float dt, Monde2D monde, Joueur2D joueur, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            TempsRestant -= dt;
            if (TempsRestant <= 0f && !EstExecutee)
            {
                EstExecutee = true;
                cam.DeclencherSecousse(14f, 0.35f);
                AudioSynthetiseur.SonExplosion();

                bool joueurTouche = false;

                if (Forme == FormeZoneDanger.Cercle)
                {
                    particules.EmettreAnneauExplosion(Position, Couleur, 35, Rayon * 2.6f);
                    particules.EmettreOndeDeChoc(Position, Couleur, Rayon, 0.35f);
                    joueurTouche = Vector2.Distance(Position, joueur.Position) <= Rayon + joueur.Rayon;
                }
                else if (Forme == FormeZoneDanger.LigneCharge)
                {
                    // Ligne d'impact
                    Vector2 fin = Position + Direction * LongueurLigne;
                    particules.EmettreOndeDeChoc(Position, Couleur, LargeurLigne, 0.3f);
                    particules.EmettreOndeDeChoc(fin, Couleur, LargeurLigne * 1.2f, 0.35f);
                    for (float t = 0f; t <= 1f; t += 0.2f)
                        particules.EmettreEclats(Vector2.Lerp(Position, fin, t), Couleur, 8, 140f, 3.5f, 0.3f);

                    // Collision segment vs cercle joueur
                    joueurTouche = CollisionSegmentCercle(Position, fin, joueur.Position, (LargeurLigne / 2f) + joueur.Rayon);
                }
                else if (Forme == FormeZoneDanger.ConeSouffle)
                {
                    particules.EmettreAnneauExplosion(Position, Couleur, 25, Rayon * 1.8f);
                    float dist = Vector2.Distance(Position, joueur.Position);
                    if (dist <= Rayon + joueur.Rayon && dist > 0.01f)
                    {
                        Vector2 versJoueur = (joueur.Position - Position).Normaliser();
                        float dot = Vector2.Dot(Direction, versJoueur);
                        float angleMaxRad = (AngleOuverture / 2f) * MathF.PI / 180f;
                        if (dot >= MathF.Cos(angleMaxRad))
                            joueurTouche = true;
                    }
                }

                if (joueurTouche)
                {
                    joueur.SubirDegats(Degats, NomSort, particules, cam, ajouterTexteFlottant, 18f, 12);
                    string nom = NomSort.ToLowerInvariant();
                    if (nom.Contains("feu") || nom.Contains("météore") || nom.Contains("ardent") || nom.Contains("flamme"))
                    {
                        joueur.AppliquerStatut("Brulure", 3.5f, Math.Max(2, (int)(Degats * 0.15f)));
                    }
                    else if (nom.Contains("givre") || nom.Contains("blizzard") || nom.Contains("boréale"))
                    {
                        joueur.AppliquerStatut("Gel", 3.0f);
                    }
                    else if (nom.Contains("nécro") || nom.Contains("pestilence") || nom.Contains("miasm") || nom.Contains("poison") || nom.Contains("âmes"))
                    {
                        joueur.AppliquerStatut("Poison", 4.0f, Math.Max(2, (int)(Degats * 0.12f)));
                    }
                    else if (nom.Contains("charge") || nom.Contains("sismique") || nom.Contains("fracas") || nom.Contains("tellurique"))
                    {
                        joueur.AppliquerStatut("Etourdi", 0.65f);
                    }
                }
            }
        }

        private static bool CollisionSegmentCercle(Vector2 a, Vector2 b, Vector2 p, float rayon)
        {
            Vector2 ab = b - a;
            float lenSq = ab.LongueurCarree();
            if (lenSq < 0.001f) return Vector2.Distance(a, p) <= rayon;
            float t = Math.Clamp(Vector2.Dot(p - a, ab) / lenSq, 0f, 1f);
            Vector2 proj = a + ab * t;
            return Vector2.Distance(p, proj) <= rayon;
        }

        public void Dessiner(Graphics g, Camera2D cam)
        {
            if (EstExecutee) return;
            float ratio = Math.Clamp(1f - (TempsRestant / Math.Max(0.01f, TempsTotal)), 0f, 1f);
            float battement = MathF.Sin(ratio * MathF.PI * 10f * ratio) * 20f;

            if (Forme == FormeZoneDanger.Cercle)
            {
                DessinerCercle(g, cam, ratio, battement);
            }
            else if (Forme == FormeZoneDanger.LigneCharge)
            {
                DessinerLigneCharge(g, cam, ratio);
            }
            else if (Forme == FormeZoneDanger.ConeSouffle)
            {
                DessinerCone(g, cam, ratio);
            }
        }

        private void DessinerCercle(Graphics g, Camera2D cam, float ratio, float battement)
        {
            Point p = cam.MondeVersEcran(Position);
            float r = Rayon * cam.Zoom;

            GraphicsState etat = g.Save();
            g.TranslateTransform(p.X, p.Y);

            SolidBrush bFond = CacheRenduGDI.ObtenirBrushAlpha((int)(40 + ratio * 85 + battement), Couleur);
            g.FillEllipse(bFond, -r, -r, r * 2f, r * 2f);

            Pen pBord = CacheRenduGDI.ObtenirPen(Couleur, 3f + (ratio * 2f));
            g.DrawEllipse(pBord, -r, -r, r * 2f, r * 2f);

            g.RotateTransform(ratio * 180f);
            Pen pRunes = CacheRenduGDI.ObtenirPen(Color.FromArgb(120, Couleur), 2f);
            g.DrawRectangle(pRunes, -r * 0.7f, -r * 0.7f, r * 1.4f, r * 1.4f);
            g.RotateTransform(45f);
            g.DrawRectangle(pRunes, -r * 0.7f, -r * 0.7f, r * 1.4f, r * 1.4f);

            g.Restore(etat);

            float innerR = r * ratio;
            SolidBrush bInner = CacheRenduGDI.ObtenirBrushAlpha((int)(90 + ratio * 120), Couleur);
            g.FillEllipse(bInner, p.X - innerR, p.Y - innerR, innerR * 2f, innerR * 2f);
        }

        private void DessinerLigneCharge(Graphics g, Camera2D cam, float ratio)
        {
            Point pDebut = cam.MondeVersEcran(Position);
            float angleDeg = MathF.Atan2(Direction.Y, Direction.X) * 180f / MathF.PI;
            float zoom = cam.Zoom;
            float longPx = LongueurLigne * zoom;
            float largPx = LargeurLigne * zoom;

            GraphicsState etat = g.Save();
            g.TranslateTransform(pDebut.X, pDebut.Y);
            g.RotateTransform(angleDeg);

            // Fond du couloir d'alerte
            int alphaFond = (int)(45 + ratio * 95);
            g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(alphaFond, Couleur), 0, -largPx / 2f, longPx, largPx);

            // Contour avec tirets vifs
            Pen pBord = CacheRenduGDI.ObtenirPen(Couleur, 2.5f);
            g.DrawRectangle(pBord, 0, -largPx / 2f, longPx, largPx);

            // Barre de charge qui avance vers la cible
            float avancePx = longPx * ratio;
            g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha((int)(110 + ratio * 125), Couleur), 0, -largPx / 2f, avancePx, largPx);

            // Chevrons indicateurs de ruée
            Pen pFleche = CacheRenduGDI.ObtenirPen(Color.Gold, 2.5f);
            for (float x = 40f * zoom; x < longPx; x += 60f * zoom)
            {
                g.DrawLine(pFleche, x, -largPx * 0.35f, x + 20f * zoom, 0);
                g.DrawLine(pFleche, x + 20f * zoom, 0, x, largPx * 0.35f);
            }

            g.Restore(etat);
        }

        private void DessinerCone(Graphics g, Camera2D cam, float ratio)
        {
            Point p = cam.MondeVersEcran(Position);
            float r = Rayon * cam.Zoom;
            float angleDeg = MathF.Atan2(Direction.Y, Direction.X) * 180f / MathF.PI;
            float debutAngle = angleDeg - (AngleOuverture / 2f);

            // Cône de fond
            g.FillPie(CacheRenduGDI.ObtenirBrushAlpha((int)(40 + ratio * 90), Couleur), p.X - r, p.Y - r, r * 2f, r * 2f, debutAngle, AngleOuverture);
            g.DrawPie(CacheRenduGDI.ObtenirPen(Couleur, 2.5f), p.X - r, p.Y - r, r * 2f, r * 2f, debutAngle, AngleOuverture);

            // Arc qui se remplit
            float innerR = r * ratio;
            if (innerR > 4)
            {
                g.FillPie(CacheRenduGDI.ObtenirBrushAlpha((int)(80 + ratio * 130), Couleur), p.X - innerR, p.Y - innerR, innerR * 2f, innerR * 2f, debutAngle, AngleOuverture);
            }
        }
    }

    // ==============================================================
    // CARTE & MONDE 2D
    // ==============================================================
    public class Monde2D
    {
        public const float FacteurEchelleCapitale = 1.35f;
        private const float OrigineEchelleCapitale = 50f;

        public ZoneType2D TypeZoneActuelle { get; private set; } = ZoneType2D.Village;
        public string NomZone { get; private set; } = "Capitale d'Aethelgard";
        public DifficulteBoss2D DifficulteActive { get; set; } = DifficulteBoss2D.Normale;
        public float LargeurMonde = 2400f;
        public float HauteurMonde = 2000f;
        public float TempsTotal = 0f;

        public List<Obstacle2D> Obstacles = new List<Obstacle2D>();
        public List<ObjetInteractif2D> ObjetsInteractifs = new List<ObjetInteractif2D>();
        public List<Monstre2D> Monstres = new List<Monstre2D>();
        public List<Projectile2D> Projectiles = new List<Projectile2D>();
        public List<ZoneDanger2D> ZonesDanger = new List<ZoneDanger2D>();
        public List<LootAuSol2D> Loots = new List<LootAuSol2D>();
        public List<TexteFlottant> Textes = new List<TexteFlottant>();
        public List<PouleVillageoise> Poules = new List<PouleVillageoise>();
        public GestionnaireParticules Particules = new GestionnaireParticules();

        public Vector2 PositionDepartVillage => EchellePositionCapitale(new Vector2(1000, 580));

        public static float EchelleCoordonneeCapitale(float valeur) =>
            OrigineEchelleCapitale + (valeur - OrigineEchelleCapitale) * FacteurEchelleCapitale;

        private static Vector2 EchellePositionCapitale(Vector2 position) => new Vector2(
            EchelleCoordonneeCapitale(position.X),
            EchelleCoordonneeCapitale(position.Y));

        private static RectangleF EchelleZoneCapitale(RectangleF zone) => new RectangleF(
            EchelleCoordonneeCapitale(zone.X),
            EchelleCoordonneeCapitale(zone.Y),
            zone.Width * FacteurEchelleCapitale,
            zone.Height * FacteurEchelleCapitale);

        public void AjouterZoneDanger(ZoneDanger2D zd) => ZonesDanger.Add(zd);

        public Vector2 PositionSourisActuelle;
        private readonly Random rng = Random.Shared;
        private float chronoMeteo;
        private float chronoFumee;
        private float chronoFontaine;

        // DPS Mannequin
        public int DegatsMannequinCumules = 0;
        public float ChronoMannequin = 0f;

        public Monde2D()
        {
            ChargerVillage();
        }

        public void ChargerVillage()
        {
            DifficulteActive = DifficulteBoss2D.Normale;
            TypeZoneActuelle = ZoneType2D.Village;
            NomZone = "Capitale d'Aethelgard";
            LargeurMonde = EchelleCoordonneeCapitale(2000f);
            HauteurMonde = EchelleCoordonneeCapitale(1600f);

            Obstacles.Clear();
            ObjetsInteractifs.Clear();
            Monstres.Clear();
            Projectiles.Clear();
            ZonesDanger.Clear();
            Loots.Clear();

            // Murs d'enceinte de la ville avec créneaux et remparts
            Obstacles.Add(new Obstacle2D(50, 50, LargeurMonde - 100, 28, "Rempart Nord", TypeObstacle.MurEnceinteNord));
            Obstacles.Add(new Obstacle2D(50, HauteurMonde - 78, LargeurMonde - 100, 28, "Rempart Sud", TypeObstacle.MurEnceinteSud));
            Obstacles.Add(new Obstacle2D(50, 50, 28, HauteurMonde - 100, "Rempart Ouest", TypeObstacle.MurEnceinteOuest));
            Obstacles.Add(new Obstacle2D(LargeurMonde - 78, 50, 28, HauteurMonde - 100, "Rempart Est", TypeObstacle.MurEnceinteEst));

            // Fontaine centrale sculptée
            Obstacles.Add(new Obstacle2D(940, 710, 120, 120, "Grande Fontaine", TypeObstacle.GrandeFontaine));

            // Bâtiments & Échoppes
            Obstacles.Add(new Obstacle2D(380, 260, 260, 160, "Forge de Brom", TypeObstacle.BatimentForge));
            Obstacles.Add(new Obstacle2D(1330, 260, 280, 160, "Guilde des Aventuriers", TypeObstacle.BatimentGuilde));
            Obstacles.Add(new Obstacle2D(380, 1080, 240, 150, "Échoppe Alchimie", TypeObstacle.BatimentAlchimie));

            // Taverne "LE DRAGON VERT" (Image 1)
            Obstacles.Add(new Obstacle2D(580, 780, 260, 160, "Taverne Le Dragon Vert", TypeObstacle.TaverneDragonVert));

            // Marché & Étalages aux auvents rayés (Image 1)
            Obstacles.Add(new Obstacle2D(860, 960, 110, 65, "Étalage de Fruits & Légumes (Rouge)", TypeObstacle.EtalMarche));
            Obstacles.Add(new Obstacle2D(1110, 960, 110, 65, "Étalage des Épices & Pains (Bleu)", TypeObstacle.EtalMarche));

            // Charrette marchande attelée aux bœufs (Image 1)
            Obstacles.Add(new Obstacle2D(1330, 980, 140, 75, "Chariot de Marchand & Bœufs", TypeObstacle.CharretteAttellee));

            // Pont en pierre fortifié au-dessus de la rivière ouest avec gardes (Image 1)
            Obstacles.Add(new Obstacle2D(110, 720, 110, 100, "Pont en Pierre des Remparts", TypeObstacle.PontPierre));

            // Arbres majestueux et vergers
            Obstacles.Add(new Obstacle2D(720, 220, 70, 70, "Grand Chêne", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(260, 200, 60, 60, "Chêne", TypeObstacle.Arbre));
            Obstacles.Add(new Obstacle2D(240, 650, 70, 70, "Saule", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(1280, 220, 60, 60, "Chêne", TypeObstacle.Arbre));
            Obstacles.Add(new Obstacle2D(1700, 200, 70, 70, "Grand Chêne", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(250, 1150, 60, 60, "Arbre des Fées", TypeObstacle.Arbre));
            Obstacles.Add(new Obstacle2D(260, 1400, 70, 70, "Grand Chêne", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(800, 1300, 60, 60, "Chêne", TypeObstacle.Arbre));
            Obstacles.Add(new Obstacle2D(1200, 1300, 60, 60, "Chêne", TypeObstacle.Arbre));
            Obstacles.Add(new Obstacle2D(1680, 600, 70, 70, "Grand Saule", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(1680, 1020, 70, 70, "Grand Chêne", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(1450, 1200, 60, 60, "Chêne", TypeObstacle.Arbre));
            Obstacles.Add(new Obstacle2D(180, 850, 78, 78, "Bosquet du rempart", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(1800, 350, 78, 78, "Bosquet du rempart", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(1820, 1320, 78, 78, "Bosquet du rempart", TypeObstacle.ArbreGrand));
            Obstacles.Add(new Obstacle2D(820, 1450, 68, 68, "Jardin sud", TypeObstacle.Arbre));

            // Lampadaires en fer forgé
            Obstacles.Add(new Obstacle2D(860, 630, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(1140, 630, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(860, 910, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(1140, 910, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(350, 440, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(1630, 440, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(350, 1250, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(920, 220, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(1080, 220, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(760, 550, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(1240, 550, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(760, 1020, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(1240, 1020, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(1760, 780, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));
            Obstacles.Add(new Obstacle2D(300, 780, 24, 24, "Lampadaire", TypeObstacle.Lampadaire));

            // Bancs en pierre autour de la place centrale
            Obstacles.Add(new Obstacle2D(900, 640, 55, 20, "Banc en pierre", TypeObstacle.BancPierre));
            Obstacles.Add(new Obstacle2D(1050, 640, 55, 20, "Banc en pierre", TypeObstacle.BancPierre));
            Obstacles.Add(new Obstacle2D(900, 890, 55, 20, "Banc en pierre", TypeObstacle.BancPierre));
            Obstacles.Add(new Obstacle2D(1050, 890, 55, 20, "Banc en pierre", TypeObstacle.BancPierre));

            // Caisses & Tonneaux décoratifs
            Obstacles.Add(new Obstacle2D(650, 390, 36, 36, "Tonneaux & Caisses", TypeObstacle.CaisseTonneau));
            Obstacles.Add(new Obstacle2D(700, 390, 48, 38, "Établi de Craft", TypeObstacle.CaisseTonneau));
            Obstacles.Add(new Obstacle2D(630, 1180, 36, 36, "Tonneaux d'Alchimie", TypeObstacle.CaisseTonneau));

            // PNJ et Objets
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(510, 470), TypeInteractif.NPC_Brom, "Brom le Forgeron", "Améliorations & forge (+1, +2...)"));
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(700, 470), TypeInteractif.NPC_Artisan, "Maître Kaëlith", "Craft d'armes, armures & sorts magiques"));
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(1470, 470), TypeInteractif.NPC_Elenora, "Maîtresse Elenora", "Quêtes royales & réputation"));
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(500, 1270), TypeInteractif.NPC_Marchand, "Apothicaire Sylas", "Potions & consommables"));
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(700, 960), TypeInteractif.NPC_Aubergiste, "Mira l'Aubergiste", "Repos, soins et bonus d'expérience"));
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(1450, 960), TypeInteractif.NPC_Capitaine, "Capitaine Arven", "Contrats secondaires et défense du royaume"));

            // Portails
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(1000, 230), TypeInteractif.PortailDonjon, "Portail des Donjons", "Expéditions & Boss"));
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(1650, 800), TypeInteractif.PortailTour, "Tour Astrale Infinie", "Roguelite sans fin"));

            // Mannequin d'entraînement
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(1000, 1280), TypeInteractif.Mannequin, "Mannequin d'entraînement", "Tester vos combos & DPS"));

            for (int i = 0; i < Obstacles.Count; i++)
            {
                Obstacle2D obstacle = Obstacles[i];
                obstacle.Boite = EchelleZoneCapitale(obstacle.Boite);
            }

            for (int i = 0; i < ObjetsInteractifs.Count; i++)
            {
                ObjetsInteractifs[i].Position = EchellePositionCapitale(ObjetsInteractifs[i].Position);
                ObjetsInteractifs[i].Rayon *= FacteurEchelleCapitale;
                ObjetsInteractifs[i].ReinitialiserPositionAncrage();
            }

            // Poules villageoises en liberté (Image 1)
            Poules.Clear();
            Vector2[] positionsPoules = new[]
            {
                new Vector2(820, 880),
                new Vector2(890, 1040),
                new Vector2(1050, 980),
                new Vector2(1180, 1030),
                new Vector2(1260, 920),
                new Vector2(740, 1020),
                new Vector2(980, 1120),
                new Vector2(1120, 860)
            };
            for (int i = 0; i < positionsPoules.Length; i++)
            {
                Poules.Add(new PouleVillageoise(EchellePositionCapitale(positionsPoules[i]), i % 3));
            }
        }

        public void ChargerDonjon(string nomDonjon, List<Monstre> monstresDonjon, Monstre bossFinal, int niveauJoueur = 1, int niveauRecommande = 1)
        {
            GenererDonjonProcedural(nomDonjon, monstresDonjon, bossFinal, niveauJoueur, niveauRecommande);
        }

        public void GenererDonjonProcedural(string nomDonjon, List<Monstre> monstresDonjon, Monstre bossFinal, int niveauJoueur = 1, int niveauRecommande = 1)
        {
            DifficulteActive = DifficulteBoss2D.Normale;
            TypeZoneActuelle = ZoneType2D.Donjon;
            NomZone = nomDonjon;
            LargeurMonde = 3000f;
            HauteurMonde = 2000f;

            Obstacles.Clear();
            ObjetsInteractifs.Clear();
            Monstres.Clear();
            Projectiles.Clear();
            ZonesDanger.Clear();
            Loots.Clear();
            Poules.Clear();

            string theme = nomDonjon.ToLowerInvariant();
            TypeObstacle typePilier = theme.Contains("cosmique") ? TypeObstacle.PilierAstral :
                                      theme.Contains("forêt") || theme.Contains("foret") ? TypeObstacle.Arbre :
                                      TypeObstacle.PilierPierre;

            // ==============================================================
            // 1. MURS EXTÉRIEURS DU DONJON
            // ==============================================================
            Obstacles.Add(new Obstacle2D(50, 50, LargeurMonde - 100, 28, "Mur Nord", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(50, HauteurMonde - 78, LargeurMonde - 100, 28, "Mur Sud", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(50, 50, 28, HauteurMonde - 100, "Mur Ouest", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(LargeurMonde - 78, 50, 28, HauteurMonde - 100, "Mur Est", TypeObstacle.MurStandard));

            // ==============================================================
            // 2. SALLE 1 : ENTRÉE & VESTIBULE (X: 100..650, Y: 750..1250)
            // ==============================================================
            // Portail de repli au village & torche d'accueil
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(250, 1000), TypeInteractif.PortailRetour, "Sortir du Donjon", "Retourner au village"));
            Obstacles.Add(new Obstacle2D(200, 820, 24, 24, "Brasero Entrée", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(200, 1180, 24, 24, "Brasero Entrée", TypeObstacle.Brasero));

            // Mur de séparation Ouest (avec porte centrale ouverte Y: 920..1080)
            Obstacles.Add(new Obstacle2D(650, 50, 28, 870, "Cloison Ouest Nord", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(650, 1080, 28, HauteurMonde - 1130, "Cloison Ouest Sud", TypeObstacle.MurStandard));

            // Couloir Ouest-Centre : Murs horizontaux guidant vers la salle 2
            Obstacles.Add(new Obstacle2D(650, 910, 300, 28, "Mur Couloir Ouest Haut", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(650, 1080, 300, 28, "Mur Couloir Ouest Bas", TypeObstacle.MurStandard));

            // ==============================================================
            // 3. SALLE 2 : GRAND CARREFOUR CENTRAL (X: 950..1750, Y: 600..1400)
            // ==============================================================
            // 4 Piliers monumentaux et brasiers
            Obstacles.Add(new Obstacle2D(1120, 780, 55, 55, "Pilier Monumental", typePilier));
            Obstacles.Add(new Obstacle2D(1580, 780, 55, 55, "Pilier Monumental", typePilier));
            Obstacles.Add(new Obstacle2D(1120, 1220, 55, 55, "Pilier Monumental", typePilier));
            Obstacles.Add(new Obstacle2D(1580, 1220, 55, 55, "Pilier Monumental", typePilier));

            Obstacles.Add(new Obstacle2D(1135, 730, 24, 24, "Brasero Sanctuaire", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(1595, 730, 24, 24, "Brasero Sanctuaire", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(1135, 1290, 24, 24, "Brasero Sanctuaire", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(1595, 1290, 24, 24, "Brasero Sanctuaire", TypeObstacle.Brasero));

            // Cloisons Nord de la salle 2 avec passage ouvert vers Salle 3 (X: 1270..1430)
            Obstacles.Add(new Obstacle2D(950, 600, 320, 28, "Mur Salle 2 Nord G", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(1430, 600, 320, 28, "Mur Salle 2 Nord D", TypeObstacle.MurStandard));

            // Cloisons Sud de la salle 2 avec passage ouvert vers Salle 4 (X: 1270..1430)
            Obstacles.Add(new Obstacle2D(950, 1400, 320, 28, "Mur Salle 2 Sud G", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(1430, 1400, 320, 28, "Mur Salle 2 Sud D", TypeObstacle.MurStandard));

            // ==============================================================
            // 4. SALLE 3 : AILE NORD / ARMURERIE & TRÉSORS (X: 1000..1700, Y: 100..600)
            // ==============================================================
            Obstacles.Add(new Obstacle2D(980, 100, 28, 528, "Mur Armurerie Ouest", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(1720, 100, 28, 528, "Mur Armurerie Est", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(1180, 300, 45, 45, "Stèle Antique", typePilier));
            Obstacles.Add(new Obstacle2D(1520, 300, 45, 45, "Stèle Antique", typePilier));
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(1350, 220), TypeInteractif.Coffre, "Coffre Antique"));

            // ==============================================================
            // 5. SALLE 4 : AILE SUD / CRYPTE & CACHE SECRÈTE (X: 1000..1700, Y: 1400..1900)
            // ==============================================================
            Obstacles.Add(new Obstacle2D(980, 1400, 28, 528, "Mur Crypte Ouest", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(1720, 1400, 28, 528, "Mur Crypte Est", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(1180, 1650, 45, 45, "Monolithe Funéraire", typePilier));
            Obstacles.Add(new Obstacle2D(1520, 1650, 45, 45, "Monolithe Funéraire", typePilier));
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(1350, 1750), TypeInteractif.Coffre, "Coffre Scellé"));

            // ==============================================================
            // 6. COULOIR CENTRE-EST & SALLE 5 : ANTICHAMBRE DU BOSS (X: 1750..2350)
            // ==============================================================
            // Mur Est de la Salle 2 avec ouverture centrale (Y: 920..1080)
            Obstacles.Add(new Obstacle2D(1750, 600, 28, 320, "Cloison Est Nord", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(1750, 1080, 28, 320, "Cloison Est Sud", TypeObstacle.MurStandard));

            // Couloir vers antichambre
            Obstacles.Add(new Obstacle2D(1750, 910, 300, 28, "Mur Couloir Est Haut", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(1750, 1080, 300, 28, "Mur Couloir Est Bas", TypeObstacle.MurStandard));

            // Brasiers d'avertissement dans l'Antichambre
            Obstacles.Add(new Obstacle2D(2120, 840, 28, 28, "Brasero de Garde", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(2120, 1160, 28, 28, "Brasero de Garde", TypeObstacle.Brasero));

            // Porte majestueuse vers le sanctum (X = 2350, ouverture Y: 890..1110)
            Obstacles.Add(new Obstacle2D(2350, 50, 28, 840, "Arche Sanctum Haut", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(2350, 1110, 28, HauteurMonde - 1160, "Arche Sanctum Bas", TypeObstacle.MurStandard));

            // ==============================================================
            // 7. SALLE 6 : SANCTUM DU BOSS (L'ARÈNE ROYALE X: 2350..2950, Y: 550..1450)
            // ==============================================================
            // 4 Piliers tactiques permettant au joueur d'esquiver les souffles et projectiles
            Obstacles.Add(new Obstacle2D(2520, 750, 60, 60, "Pilier de Sanctuaire", typePilier));
            Obstacles.Add(new Obstacle2D(2800, 750, 60, 60, "Pilier de Sanctuaire", typePilier));
            Obstacles.Add(new Obstacle2D(2520, 1250, 60, 60, "Pilier de Sanctuaire", typePilier));
            Obstacles.Add(new Obstacle2D(2800, 1250, 60, 60, "Pilier de Sanctuaire", typePilier));

            Obstacles.Add(new Obstacle2D(2538, 690, 24, 24, "Brasero Sacré", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(2818, 690, 24, 24, "Brasero Sacré", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(2538, 1320, 24, 24, "Brasero Sacré", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(2818, 1320, 24, 24, "Brasero Sacré", TypeObstacle.Brasero));

            // Coffre secret supplémentaire aléatoire dans l'Antichambre
            if (rng.Next(2) == 0)
            {
                ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(2150, 1000), TypeInteractif.Coffre, "Coffre de l'Antichambre"));
            }

            // ==============================================================
            // 8. RÉPARTITION THÉMATIQUE DES MONSTRES DANS LES SALLES
            // ==============================================================
            Vector2[] pointsApparition = new[]
            {
                new Vector2(1200, 950),  // Salle 2 Centre
                new Vector2(1500, 1050), // Salle 2 Centre
                new Vector2(1350, 420),  // Salle 3 Armurerie
                new Vector2(1480, 360),  // Salle 3 Armurerie
                new Vector2(1350, 1580), // Salle 4 Crypte
                new Vector2(1220, 1620), // Salle 4 Crypte
                new Vector2(2200, 1000)  // Salle 5 Antichambre
            };

            for (int i = 0; i < monstresDonjon.Count; i++)
            {
                var m = monstresDonjon[i];
                Vector2 spawnBase = pointsApparition[i % pointsApparition.Length];
                Vector2 jitter = new Vector2(rng.Next(-40, 40), rng.Next(-40, 40));
                Vector2 posFinale = spawnBase + jitter;

                AppliquerEchelleNiveau(m, niveauJoueur, niveauRecommande);
                Monstres.Add(new Monstre2D(m, posFinale));
            }

            // Boss Final majestueux au cœur de son Sanctum à l'Est
            var boss2D = new Monstre2D(bossFinal, new Vector2(2680, 1000));
            AppliquerEchelleNiveau(boss2D.ModeleMonstre, niveauJoueur, niveauRecommande);
            Monstres.Add(boss2D);
        }

        public static void AppliquerEchelleNiveau(Monstre monstre, int niveauJoueur, int niveauRecommande)
        {
            int ecart = Math.Clamp(niveauJoueur - Math.Max(1, niveauRecommande), -8, 10);
            float facteurVie = Math.Clamp(1f + (ecart * 0.07f), 0.65f, 1.7f);
            float facteurAttaque = Math.Clamp(1f + (ecart * 0.05f), 0.7f, 1.5f);
            float facteurDefense = Math.Clamp(1f + (ecart * 0.04f), 0.7f, 1.4f);
            float facteurRecompense = Math.Clamp(1f + (ecart * 0.025f), 0.8f, 1.3f);

            monstre.PVMax = Math.Max(1, (int)Math.Round(monstre.PVMax * facteurVie));
            monstre.PVActuels = monstre.PVMax;
            monstre.Attaque = Math.Max(1, (int)Math.Round(monstre.Attaque * facteurAttaque));
            monstre.Defense = Math.Max(0, (int)Math.Round(monstre.Defense * facteurDefense));
            monstre.GainXP = Math.Max(1, (int)Math.Round(monstre.GainXP * facteurRecompense));
            monstre.GainOr = Math.Max(0, (int)Math.Round(monstre.GainOr * facteurRecompense));
            monstre.NiveauRecommande = Math.Max(1, niveauRecommande);
        }

        public void ChargerEtageTour(int etage, Monstre bossOuEnnemi, List<Monstre>? adds = null, int niveauJoueur = 1)
        {
            DifficulteActive = DifficulteBoss2D.Normale;
            TypeZoneActuelle = ZoneType2D.TourAstrale;
            NomZone = $"Tour Astrale — Étage {etage}";
            LargeurMonde = 1800f;
            HauteurMonde = 1800f;

            Obstacles.Clear();
            ObjetsInteractifs.Clear();
            Monstres.Clear();
            Projectiles.Clear();
            ZonesDanger.Clear();
            Loots.Clear();
            Poules.Clear();

            // Arène céleste fermée
            Obstacles.Add(new Obstacle2D(100, 100, LargeurMonde - 200, 24, "Bordure Nord", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(100, HauteurMonde - 124, LargeurMonde - 200, 24, "Bordure Sud", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(100, 100, 24, HauteurMonde - 200, "Bordure Ouest", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(LargeurMonde - 124, 100, 24, HauteurMonde - 200, "Bordure Est", TypeObstacle.MurStandard));

            // Piliers astraux mystiques
            Obstacles.Add(new Obstacle2D(480, 480, 60, 60, "Pilier Astral", TypeObstacle.PilierAstral));
            Obstacles.Add(new Obstacle2D(1260, 480, 60, 60, "Pilier Astral", TypeObstacle.PilierAstral));
            Obstacles.Add(new Obstacle2D(480, 1220, 60, 60, "Pilier Astral", TypeObstacle.PilierAstral));
            Obstacles.Add(new Obstacle2D(1260, 1220, 60, 60, "Pilier Astral", TypeObstacle.PilierAstral));

            // Portail de repli pour sécuriser le butin
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(900, 1620), TypeInteractif.PortailRetour, "Encaisser & Quitter", "Sécuriser le butin et rentrer"));

            // Pop du monstre ou boss au centre
            var bossEtage = new Monstre2D(bossOuEnnemi, new Vector2(900, 750));
            AppliquerEchelleNiveau(bossEtage.ModeleMonstre, niveauJoueur, Math.Max(1, etage));
            Monstres.Add(bossEtage);

            // Pop des adds célestes
            if (adds != null)
            {
                float[] dx = { -200f, 200f, -300f, 300f, 0f };
                float[] dy = { -100f, -100f, 100f, 100f, -220f };
                for (int i = 0; i < adds.Count; i++)
                {
                    Vector2 posAdd = new Vector2(900 + dx[i % dx.Length], 750 + dy[i % dy.Length]);
                    var add = new Monstre2D(adds[i], posAdd);
                    AppliquerEchelleNiveau(add.ModeleMonstre, niveauJoueur, Math.Max(1, etage));
                    Monstres.Add(add);
                }
            }
            else if (etage > 3)
            {
                var echoGauche = new Monstre("Écho Stellaire", 120 + etage * 20, 18 + etage * 3, 8 + etage, 100, 40);
                var echoDroite = new Monstre("Écho Stellaire", 120 + etage * 20, 18 + etage * 3, 8 + etage, 100, 40);
                AppliquerEchelleNiveau(echoGauche, niveauJoueur, Math.Max(1, etage));
                AppliquerEchelleNiveau(echoDroite, niveauJoueur, Math.Max(1, etage));
                Monstres.Add(new Monstre2D(echoGauche, new Vector2(700, 700)));
                Monstres.Add(new Monstre2D(echoDroite, new Vector2(1100, 700)));
            }
        }

        public void ChargerAreneBoss(Monstre bossOriginal, DifficulteBoss2D difficulte, int niveauJoueur = 1)
        {
            DifficulteActive = difficulte;
            TypeZoneActuelle = ZoneType2D.Donjon;
            NomZone = $"Arène : {bossOriginal.Nom} [{difficulte.Titre}]";
            LargeurMonde = 2400f;
            HauteurMonde = 1800f;

            Obstacles.Clear();
            ObjetsInteractifs.Clear();
            Monstres.Clear();
            Projectiles.Clear();
            ZonesDanger.Clear();
            Loots.Clear();
            Poules.Clear();

            // Murs extérieurs de l'arène
            Obstacles.Add(new Obstacle2D(60, 60, LargeurMonde - 120, 24, "Mur Nord", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(60, HauteurMonde - 84, LargeurMonde - 120, 24, "Mur Sud", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(60, 60, 24, HauteurMonde - 120, "Mur Ouest", TypeObstacle.MurStandard));
            Obstacles.Add(new Obstacle2D(LargeurMonde - 84, 60, 24, HauteurMonde - 120, "Mur Est", TypeObstacle.MurStandard));

            // Piliers d'arène & braserots
            Obstacles.Add(new Obstacle2D(650, 500, 70, 70, "Pilier", TypeObstacle.PilierPierre));
            Obstacles.Add(new Obstacle2D(650, 1200, 70, 70, "Pilier", TypeObstacle.PilierPierre));
            Obstacles.Add(new Obstacle2D(1650, 500, 70, 70, "Pilier", TypeObstacle.PilierPierre));
            Obstacles.Add(new Obstacle2D(1650, 1200, 70, 70, "Pilier", TypeObstacle.PilierPierre));

            Obstacles.Add(new Obstacle2D(650, 410, 24, 24, "Brasero", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(650, 1290, 24, 24, "Brasero", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(1650, 410, 24, 24, "Brasero", TypeObstacle.Brasero));
            Obstacles.Add(new Obstacle2D(1650, 1290, 24, 24, "Brasero", TypeObstacle.Brasero));

            // Portail de repli
            ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(250, 900), TypeInteractif.PortailRetour, "Sortir de l'Arène", "Repli vers la Capitale"));

            // Ajustement des stats selon la difficulté pour le jeu 2D temps réel
            int basePV2D = Math.Max(2600, (int)(bossOriginal.PVMax * 4.5f));
            int pv = (int)(basePV2D * difficulte.MultiplicateurPV);
            int baseAtk2D = Math.Max(26, (int)(bossOriginal.Attaque * 1.35f));
            int atk = (int)(baseAtk2D * difficulte.MultiplicateurAttaque);
            int def = Math.Max(8, (int)(bossOriginal.Defense * (1f + (difficulte.MultiplicateurAttaque - 1f) * 0.4f)));
            int xp = (int)(bossOriginal.GainXP * difficulte.MultiplicateurRecompenses);
            int or = (int)(bossOriginal.GainOr * difficulte.MultiplicateurRecompenses);

            Monstre bossAjuste = new Monstre(
                $"{bossOriginal.Nom} [{difficulte.Titre}]",
                pv, atk, def, xp, or,
                true,
                bossOriginal.CriDeGuerre,
                bossOriginal.Faiblesse,
                bossOriginal.Lore
            );
            var boss2D = new Monstre2D(bossAjuste, new Vector2(1650, 900));
            AppliquerEchelleNiveau(boss2D.ModeleMonstre, niveauJoueur, bossOriginal.NiveauRecommande);
            if (difficulte.EstInfernal)
            {
                boss2D.EstEnrage = true;
                boss2D.Vitesse *= 1.35f;
            }
            Monstres.Add(boss2D);
        }

        public void AjouterTexteFlottant(Vector2 pos, string texte, Color couleur, bool critique = false)
        {
            Textes.Add(new TexteFlottant
            {
                Position = pos + new Vector2((rng.NextSingle() * 20f - 10f), -15f),
                Velocite = new Vector2(rng.NextSingle() * 30f - 15f, -60f),
                Texte = texte,
                Couleur = couleur,
                VieMax = 0.85f,
                VieRestante = 0.85f,
                EstCritique = critique
            });
        }

        public void AjouterProjectile(Projectile2D p)
        {
            Projectiles.Add(p);
        }

        public void AjouterLoot(LootAuSol2D l)
        {
            Loots.Add(l);
        }

        public bool YATilInteractifProche(Vector2 posJoueur, float rayonJoueur)
        {
            for (int i = 0; i < ObjetsInteractifs.Count; i++)
            {
                var obj = ObjetsInteractifs[i];
                if (Vector2.Distance(posJoueur, obj.Position) <= obj.Rayon + rayonJoueur + 30f)
                {
                    return true;
                }
            }
            return false;
        }

        public Vector2 ResoudreCollisions(Vector2 posActuelle, Vector2 deplacement, float rayon)
        {
            Vector2 nouvPos = posActuelle + deplacement;

            // Limites du monde
            nouvPos.X = Math.Clamp(nouvPos.X, rayon + 55, LargeurMonde - rayon - 55);
            nouvPos.Y = Math.Clamp(nouvPos.Y, rayon + 55, HauteurMonde - rayon - 55);

            // Collisions avec obstacles AABB
            foreach (var obs in Obstacles)
            {
                if (obs.TypeObstacle == TypeObstacle.PontPierre) continue;

                RectangleF b = obs.Boite;
                // Trouver le point le plus proche sur le rectangle
                float plusProcheX = Math.Clamp(nouvPos.X, b.Left, b.Right);
                float plusProcheY = Math.Clamp(nouvPos.Y, b.Top, b.Bottom);

                float dx = nouvPos.X - plusProcheX;
                float dy = nouvPos.Y - plusProcheY;
                float distCarree = dx * dx + dy * dy;

                if (distCarree < rayon * rayon && distCarree > 0.0001f)
                {
                    float dist = MathF.Sqrt(distCarree);
                    float penetration = rayon - dist;
                    Vector2 normale = new Vector2(dx / dist, dy / dist);
                    nouvPos += normale * penetration;
                }
            }

            return nouvPos;
        }

        public void InfligerDegatsRayon(Vector2 centre, float rayon, int degats, Joueur2D joueur, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant, string? statut = null, float dureeStatut = 0f, int tickDegats = 0)
        {
            // Vérifier le mannequin d'entraînement
            foreach (var obj in ObjetsInteractifs)
            {
                if (obj.Type == TypeInteractif.Mannequin && Vector2.Distance(centre, obj.Position) <= rayon + obj.Rayon)
                {
                    ToucherMannequin(degats, joueur, particules, cam, ajouterTexteFlottant);
                }
            }

            for (int i = Monstres.Count - 1; i >= 0; i--)
            {
                var m = Monstres[i];
                if (m.EstMort) continue;

                if (Vector2.Distance(centre, m.Position) <= rayon + m.Rayon)
                {
                    AppliquerDegatsMonstre(m, degats, joueur, particules, cam, ajouterTexteFlottant, statut, dureeStatut, tickDegats);
                }
            }
        }

        public void InfligerDegatsZone(Vector2 origine, float portee, float angleCentral, float angleArc, int degats, Joueur2D joueur, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            // Vérifier mannequin
            foreach (var obj in ObjetsInteractifs)
            {
                if (obj.Type == TypeInteractif.Mannequin)
                {
                    Vector2 versObj = obj.Position - origine;
                    float dist = versObj.Longueur();
                    if (dist <= portee + obj.Rayon)
                    {
                        float angleVersObj = MathF.Atan2(versObj.Y, versObj.X);
                        float diff = MathF.Abs(MathF.Atan2(MathF.Sin(angleVersObj - angleCentral), MathF.Cos(angleVersObj - angleCentral)));
                        if (diff <= angleArc / 2f)
                        {
                            ToucherMannequin(degats, joueur, particules, cam, ajouterTexteFlottant);
                        }
                    }
                }
            }

            for (int i = Monstres.Count - 1; i >= 0; i--)
            {
                var m = Monstres[i];
                if (m.EstMort) continue;

                Vector2 versMonstre = m.Position - origine;
                float dist = versMonstre.Longueur();
                if (dist <= portee + m.Rayon)
                {
                    float angleVersM = MathF.Atan2(versMonstre.Y, versMonstre.X);
                    float diff = MathF.Abs(MathF.Atan2(MathF.Sin(angleVersM - angleCentral), MathF.Cos(angleVersM - angleCentral)));
                    if (diff <= angleArc / 2f)
                    {
                        AppliquerDegatsMonstre(m, degats, joueur, particules, cam, ajouterTexteFlottant);
                    }
                }
            }
        }

        private void ToucherMannequin(int degats, Joueur2D joueur, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            bool crit = rng.Next(100) < joueur.ModeleHero.ChanceCritiqueTotale;
            int dmgFinal = crit ? (int)(degats * 1.8f) : degats;

            DegatsMannequinCumules += dmgFinal;
            ChronoMannequin = 3.0f;

            cam.DeclencherSecousse(crit ? 8f : 3f, 0.15f);
            Vector2 positionMannequin = EchellePositionCapitale(new Vector2(1000, 1250));
            particules.EmettreOndeDeChoc(positionMannequin, crit ? Color.Gold : Color.White, crit ? 60f : 35f, 0.22f);
            particules.EmettreEclats(positionMannequin, crit ? Color.Gold : Color.White, crit ? 14 : 7, 160f, 3f, 0.25f);

            if (crit)
            {
                AudioSynthetiseur.SonCritique();
                ajouterTexteFlottant($"💥 {dmgFinal} CRIT!", Color.Gold, true);
            }
            else
            {
                AudioSynthetiseur.SonImpact();
                ajouterTexteFlottant($"{dmgFinal}", Color.White, false);
            }
        }

        private void AppliquerDegatsMonstre(Monstre2D m, int degats, Joueur2D joueur, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant, string? statut = null, float dureeStatut = 0f, int tickDegats = 0)
        {
            bool crit = rng.Next(100) < joueur.ModeleHero.ChanceCritiqueTotale;
            int reductionDef = m.ModeleMonstre.Defense / 2 + (m.EstLourd ? 8 : 0);
            int dmgFinal = Math.Max(3, degats - reductionDef);
            if (crit) dmgFinal = (int)(dmgFinal * 1.85f);

            m.ModeleMonstre.PVActuels -= dmgFinal;
            m.TempsFlashDegats = 0.14f;

            if (!string.IsNullOrEmpty(statut) && dureeStatut > 0f)
            {
                m.AppliquerStatut(statut, dureeStatut, tickDegats);
            }

            // Recul (knockback)
            Vector2 dirKnock = (m.Position - joueur.Position).Normaliser();
            m.Position = ResoudreCollisions(m.Position, dirKnock * (crit ? 25f : 12f), m.Rayon);

            // Vol de vie (Vampirisme)
            if (joueur.ModeleHero.VampirismeTotal > 0)
            {
                int soinVamp = Math.Max(1, (int)(dmgFinal * (joueur.ModeleHero.VampirismeTotal / 100f)));
                joueur.ModeleHero.Soigner(soinVamp);
                ajouterTexteFlottant($"+{soinVamp} PV", Color.FromArgb(46, 204, 113), false);
            }

            // Effets sonores et visuels
            if (crit)
            {
                cam.DeclencherSecousse(10f, 0.25f);
                AudioSynthetiseur.SonCritique();
                particules.EmettreEclats(m.Position, Color.Gold, 18, 200f, 4f, 0.35f);
                ajouterTexteFlottant($"💥 {dmgFinal} CRIT!", Color.Gold, true);
            }
            else
            {
                cam.DeclencherSecousse(4f, 0.15f);
                AudioSynthetiseur.SonImpact();
                particules.EmettreEclats(m.Position, Color.FromArgb(231, 76, 60), 8, 140f, 3f, 0.25f);
                ajouterTexteFlottant($"{dmgFinal}", Color.White, false);
            }

            // Mort du monstre
            if (m.EstMort)
            {
                TuerMonstre(m, joueur, particules, cam, ajouterTexteFlottant);
            }
        }

        private void TuerMonstre(Monstre2D m, Joueur2D joueur, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            cam.DeclencherSecousse(m.EstBoss ? 16f : 7f, 0.4f);
            particules.EmettreAnneauExplosion(m.Position, m.EstBoss ? Color.Gold : Color.OrangeRed, m.EstBoss ? 45 : 20, 260f);

            // Quêtes
            foreach (var q in joueur.ModeleHero.QuetesActives)
            {
                if (!q.EstTerminee && (q.CibleNom == m.ModeleMonstre.Nom || m.ModeleMonstre.Nom.Contains(q.CibleNom)))
                {
                    q.Progression++;
                    ajouterTexteFlottant($"📜 Quête : {q.Progression}/{q.Objectif}", Color.Cyan, true);
                }
            }

            // Bestiaire
            string nomMonstre = m.ModeleMonstre.Nom;
            if (!joueur.ModeleHero.BestiaireMonstresTues.ContainsKey(nomMonstre))
                joueur.ModeleHero.BestiaireMonstresTues[nomMonstre] = 0;
            joueur.ModeleHero.BestiaireMonstresTues[nomMonstre]++;

            var ficheBoss = m.EstBoss ? CatalogueCodexDrops.TrouverFiche(m.ModeleMonstre.Nom, NomZone) : null;
            if (ficheBoss != null)
            {
                if (!joueur.ModeleHero.BestiaireMonstresTues.ContainsKey(ficheBoss.NomBoss))
                    joueur.ModeleHero.BestiaireMonstresTues[ficheBoss.NomBoss] = 0;
                joueur.ModeleHero.BestiaireMonstresTues[ficheBoss.NomBoss]++;
            }

            if (m.EstBoss)
            {
                joueur.ModeleHero.BossVaincusTotal++;
                AudioSynthetiseur.SonCriBoss();
                ajouterTexteFlottant($"👑 BOSS VAINCU : {m.ModeleMonstre.Nom} !", Color.Gold, true);
            }

            // Gains XP & Or
            int xpGain = m.ModeleMonstre.GainXP > 0 ? m.ModeleMonstre.GainXP : (ficheBoss?.CalculerXP(DifficulteActive) ?? 50);
            int orGain = m.ModeleMonstre.GainOr > 0 ? m.ModeleMonstre.GainOr : (ficheBoss?.CalculerOr(DifficulteActive) ?? 50);
            joueur.ModeleHero.GagnerXP(xpGain);
            joueur.ModeleHero.Or += orGain;
            ajouterTexteFlottant($"+{xpGain} XP", Color.FromArgb(241, 196, 15), false);
            ajouterTexteFlottant($"+{orGain} 🪙", Color.FromArgb(241, 196, 15), false);

            if (m.EstBoss && ficheBoss != null)
            {
                // BUTIN DU BOSS SELON LE CODEX OFFICIEL (TAUX DE CHANCE EXACTS ET SETS)
                var butin = CatalogueCodexDrops.GenererLootBossComplet(ficheBoss, DifficulteActive, joueur.ModeleHero.Niveau, rng);
                var lootsBoss = new List<LootAuSol2D>();

                // 1. Or au sol (visuel représentatif)
                int orSol = Math.Max(orGain / 2, butin.Or / 2);
                if (orSol > 0)
                {
                    lootsBoss.Add(new LootAuSol2D(m.Position, TypeLootAuSol.Or, orSol));
                }

                // 2. Pierres de forge (selon la difficulté active)
                if (butin.PierresDeForge > 0)
                {
                    joueur.ModeleHero.PierresDeForge += butin.PierresDeForge;
                    lootsBoss.Add(new LootAuSol2D(m.Position, TypeLootAuSol.PierreForge, butin.PierresDeForge));
                    ajouterTexteFlottant($"+{butin.PierresDeForge} 💎 Pierres de Forge", Color.MediumPurple, true);
                }

                // 3. Matériaux selon les taux de drop réels du Codex
                foreach (var kvp in butin.Materiaux)
                {
                    joueur.ModeleHero.AjouterMateriau(kvp.Key, kvp.Value);
                    lootsBoss.Add(new LootAuSol2D(m.Position, TypeLootAuSol.Materiau, kvp.Value, kvp.Key));
                    ajouterTexteFlottant($"+{kvp.Value} {CatalogueCodexDrops.ObtenirIconeMateriau(kvp.Key)} {kvp.Key}", Color.LimeGreen, true);
                }

                // 4. Équipement de set garanti (1 pièce parmi les 5 du set, 20% chacune)
                if (butin.PieceEquipement != null)
                {
                    joueur.ModeleHero.SacEquipements.Add(butin.PieceEquipement);
                    lootsBoss.Add(new LootAuSol2D(m.Position, TypeLootAuSol.Equipement, 1, butin.PieceEquipement.Nom, butin.PieceEquipement.RareteItem, butin.PieceEquipement));
                    ajouterTexteFlottant($"★ SET : {butin.PieceEquipement.Nom} !", Color.Orange, true);
                    AudioSynthetiseur.SonLoot();
                }

                // Disperser les loots en anneau circulaire autour du boss
                for (int i = 0; i < lootsBoss.Count; i++)
                {
                    float angle = (float)(i * (Math.PI * 2.0 / Math.Max(1, lootsBoss.Count))) + (rng.NextSingle() * 0.3f - 0.15f);
                    float rayon = 30f + (rng.NextSingle() * 20f);
                    lootsBoss[i].Position = m.Position + new Vector2(MathF.Cos(angle) * rayon, MathF.Sin(angle) * rayon);
                    AjouterLoot(lootsBoss[i]);
                }
            }
            else
            {
                // Monstre normal
                int orQte = m.ModeleMonstre.GainOr / 2;
                if (orQte > 0)
                {
                    AjouterLoot(new LootAuSol2D(m.Position + new Vector2(-15, 0), TypeLootAuSol.Or, orQte));
                }

                // Pierres de forge
                if (rng.Next(100) < 45 || m.EstBoss)
                {
                    int nbPierres = m.EstBoss ? rng.Next(2, 5) : 1;
                    joueur.ModeleHero.PierresDeForge += nbPierres;
                    AjouterLoot(new LootAuSol2D(m.Position + new Vector2(15, 0), TypeLootAuSol.PierreForge, nbPierres));
                }

                // Matériaux ciblés
                var dictMats = m.ModeleMonstre.ObtenirLootMateriaux();
                if (dictMats != null)
                {
                    int idx = 0;
                    foreach (var kvp in dictMats)
                    {
                        joueur.ModeleHero.AjouterMateriau(kvp.Key, kvp.Value);
                        float angleMat = (float)(idx * 0.8f);
                        AjouterLoot(new LootAuSol2D(m.Position + new Vector2(MathF.Cos(angleMat) * 16f, MathF.Sin(angleMat) * 16f), TypeLootAuSol.Materiau, kvp.Value, kvp.Key));
                        idx++;
                    }
                }

                // Drop d'équipement standard
                if (m.EstBoss || rng.Next(100) < 25)
                {
                    Equipement drop = GenererLootEquipement(m.EstBoss, m.ModeleMonstre.Nom, joueur.ModeleHero.Niveau);
                    joueur.ModeleHero.SacEquipements.Add(drop);
                    AjouterLoot(new LootAuSol2D(m.Position + new Vector2(0, 15), TypeLootAuSol.Equipement, 1, drop.Nom, drop.RareteItem, drop));
                    AudioSynthetiseur.SonLoot();
                }
            }

            // Si c'est un Boss et qu'on est en donjon ou arène, faire apparaître le portail de victoire
            if (m.EstBoss && TypeZoneActuelle == ZoneType2D.Donjon)
            {
                ObjetsInteractifs.Add(new ObjetInteractif2D(m.Position, TypeInteractif.PortailRetour, "Portail de Victoire 🏆", "Retour triomphal au village !"));
            }

            // Si on est dans la Tour Astrale
            if (TypeZoneActuelle == ZoneType2D.TourAstrale)
            {
                bool tousMorts = true;
                foreach (var monstre in Monstres)
                {
                    if (monstre != m && !monstre.EstMort)
                    {
                        tousMorts = false;
                        break;
                    }
                }

                if (tousMorts)
                {
                    ObjetsInteractifs.Add(new ObjetInteractif2D(new Vector2(900, 750), TypeInteractif.PortailProchainEtage, "Portail Prochain Étage ⚡", "Monter vers l'étage suivant (Plus difficile !)"));
                    particules.EmettreAnneauExplosion(new Vector2(900, 750), Color.Cyan, 35, 240f);
                    ajouterTexteFlottant("🌟 ÉTAGE PURIFIÉ ! Empruntez le portail ⚡ pour monter !", Color.Cyan, true);
                    AudioSynthetiseur.SonCritique();
                }
            }
        }

        public Equipement GenererLootCoffre(int niveauJoueur, int etageTour = 1)
        {
            int bonusZone = TypeZoneActuelle switch
            {
                ZoneType2D.Donjon => 4,
                ZoneType2D.TourAstrale => Math.Clamp(etageTour * 2, 2, 16),
                _ => 0
            };
            int chanceMythique = TypeZoneActuelle == ZoneType2D.TourAstrale ? 1 + Math.Min(3, etageTour / 10) : 1;
            int chanceLegendaire = 5 + bonusZone;
            int chanceEpique = 18 + bonusZone;
            int tirRarete = rng.Next(100);
            Rarete rarete = tirRarete < chanceMythique ? Rarete.Mythique
                : tirRarete < chanceMythique + chanceLegendaire ? Rarete.Legendaire
                : tirRarete < chanceMythique + chanceLegendaire + chanceEpique ? Rarete.Epique
                : tirRarete < chanceMythique + chanceLegendaire + chanceEpique + 35 ? Rarete.Rare
                : Rarete.Commun;

            return GenererLootEquipement(false, "", niveauJoueur, rarete);
        }

        private Equipement GenererLootEquipement(bool estBoss, string nomBoss, int niveauJoueur, Rarete? rareteForcee = null)
        {
            if (estBoss)
                return ButinBoss2D.GenererPieceSet(nomBoss, niveauJoueur, rng);

            TypeEquipement type = (TypeEquipement)rng.Next(5);
            Rarete rarete = rareteForcee ?? (rng.Next(100) < 15 ? Rarete.Epique : (rng.Next(100) < 45 ? Rarete.Rare : Rarete.Commun));

            int mult = (int)rarete + 1;
            int atk = type == TypeEquipement.Arme ? (6 * mult) + rng.Next(4) : (type == TypeEquipement.Anneau || type == TypeEquipement.Amulette ? (2 * mult) : 0);
            int def = type == TypeEquipement.Armure ? (5 * mult) : (type == TypeEquipement.Casque ? (3 * mult) : 1);
            int pv = (8 * mult) + rng.Next(15);
            int crit = (type == TypeEquipement.Arme || type == TypeEquipement.Anneau) ? (2 * mult) : 0;
            int vamp = (rarete >= Rarete.Epique && rng.Next(100) < 40) ? (2 * mult) : 0;

            string[] prefixes = { "Luminescent", "Ancestral", "Runique", "Solaire", "Astral", "Démoniaque" };
            string prefix = prefixes[rng.Next(prefixes.Length)];
            string nom = $"{prefix} {type}";

            return new Equipement(nom, type, rarete, atk, def, pv, 10 * mult, crit, 2 * mult, vamp, 100 * mult);
        }

        public void MettreAJour(float dt, Joueur2D joueur, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            TempsTotal += dt;

            foreach (ObjetInteractif2D objet in ObjetsInteractifs)
                objet.MettreAJour(TempsTotal, TypeZoneActuelle == ZoneType2D.Village);

            chronoMeteo -= dt;
            if (chronoMeteo <= 0f)
            {
                chronoMeteo = 0.18f;
                Particules.EmettreMeteoZone(cam, TypeZoneActuelle);
            }

            if (TypeZoneActuelle == ZoneType2D.Village)
            {
                for (int i = 0; i < Poules.Count; i++)
                {
                    Poules[i].MettreAJour(dt, joueur, LargeurMonde, HauteurMonde);
                }

                chronoFumee -= dt;
                if (chronoFumee <= 0f)
                {
                    chronoFumee = 0.24f;
                    Particules.EmettrePuffFumee(EchellePositionCapitale(new Vector2(402, 236)));
                }

                chronoFontaine -= dt;
                if (chronoFontaine <= 0f)
                {
                    chronoFontaine = 0.12f;
                    Particules.EmettreGoutteletteFontaine(EchellePositionCapitale(new Vector2(1000, 770)));
                }
            }

            Particules.MettreAJour(dt);

            if (ChronoMannequin > 0f)
            {
                ChronoMannequin -= dt;
                if (ChronoMannequin <= 0f) DegatsMannequinCumules = 0;
            }

            // Textes flottants
            for (int i = Textes.Count - 1; i >= 0; i--)
            {
                Textes[i].MettreAJour(dt);
                if (Textes[i].EstMort) Textes.RemoveAt(i);
            }

            // Monstres
            for (int i = Monstres.Count - 1; i >= 0; i--)
            {
                var m = Monstres[i];
                if (m.EstMort)
                {
                    Monstres.RemoveAt(i);
                    continue;
                }
                m.MettreAJour(dt, joueur, this, Particules, cam, ajouterTexteFlottant);
            }

            // Zones de danger au sol (sorts des boss)
            for (int i = ZonesDanger.Count - 1; i >= 0; i--)
            {
                var zd = ZonesDanger[i];
                zd.MettreAJour(dt, this, joueur, Particules, cam, ajouterTexteFlottant);
                if (zd.EstExecutee && zd.TempsRestant <= -0.15f)
                {
                    ZonesDanger.RemoveAt(i);
                }
            }

            // Projectiles
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                p.MettreAJour(dt, Particules);

                if (p.EstMort)
                {
                    Particules.EmettreEclats(p.Position, p.Couleur, 6, 80f, 2.5f, 0.2f);
                    Projectiles.RemoveAt(i);
                    continue;
                }

                // Collision avec les obstacles
                foreach (var obs in Obstacles)
                {
                    if (obs.TypeObstacle == TypeObstacle.PontPierre) continue;

                    if (obs.Boite.Contains(p.Position.X, p.Position.Y))
                    {
                        p.EstMort = true;
                        AnimerImpactProjectile(p, p.Position);
                        Particules.EmettreEclats(p.Position, p.Couleur, 8, 120f, 3f, 0.2f);
                        break;
                    }
                }

                if (p.EstMort) continue;

                // Si projectile du joueur -> touche les monstres
                if (p.EstDuJoueur)
                {
                    // Mannequin
                    foreach (var obj in ObjetsInteractifs)
                    {
                        if (obj.Type == TypeInteractif.Mannequin && Vector2.Distance(p.Position, obj.Position) <= p.Rayon + obj.Rayon)
                        {
                            ToucherMannequin(p.Degats, joueur, Particules, cam, ajouterTexteFlottant);
                            AnimerImpactProjectile(p, obj.Position);
                            p.EstMort = true;
                            break;
                        }
                    }

                    if (p.EstMort) continue;

                    for (int mIdx = 0; mIdx < Monstres.Count; mIdx++)
                    {
                        var m = Monstres[mIdx];
                        if (m.EstMort) continue;

                        if (Vector2.Distance(p.Position, m.Position) <= p.Rayon + m.Rayon)
                        {
                            string nomSort = p.NomSort ?? "";
                            string? statutProj = null;
                            float dureeProj = 0f;
                            int tickProj = 0;

                            if (nomSort.Contains("Incendiaire") || nomSort.Contains("Feu"))
                            {
                                statutProj = "Brulure"; dureeProj = 4f; tickProj = Math.Max(3, (int)(p.Degats * 0.18f));
                            }
                            else if (nomSort.Contains("Givre") || nomSort.Contains("Glace"))
                            {
                                statutProj = "Gel"; dureeProj = 3.5f;
                            }
                            else if (nomSort.Contains("Poison") || nomSort.Contains("Vorace") || nomSort.Contains("Âme"))
                            {
                                statutProj = "Poison"; dureeProj = 4.5f; tickProj = Math.Max(2, (int)(p.Degats * 0.14f));
                            }
                            else if (nomSort.Contains("Foudre"))
                            {
                                statutProj = "Etourdi"; dureeProj = 0.8f;
                            }

                            AppliquerDegatsMonstre(m, p.Degats, joueur, Particules, cam, ajouterTexteFlottant, statutProj, dureeProj, tickProj);
                            AnimerImpactProjectile(p, m.Position);
                            p.EstMort = true;
                            break;
                        }
                    }
                }
                // Si projectile ennemi -> touche le joueur
                else
                {
                    if (Vector2.Distance(p.Position, joueur.Position) <= p.Rayon + joueur.Rayon)
                    {
                        joueur.SubirDegats(
                            p.Degats,
                            string.IsNullOrWhiteSpace(p.NomSort) ? "PROJECTILE" : p.NomSort,
                            Particules,
                            cam,
                            ajouterTexteFlottant,
                            7f,
                            4);

                        string nom = (p.NomSort ?? "").ToLowerInvariant();
                        if (nom.Contains("feu") || nom.Contains("braise") || nom.Contains("ardent"))
                            joueur.AppliquerStatut("Brulure", 3f, Math.Max(2, (int)(p.Degats * 0.15f)));
                        else if (nom.Contains("givre") || nom.Contains("cryo") || nom.Contains("glace"))
                            joueur.AppliquerStatut("Gel", 2.5f);
                        else if (nom.Contains("poison") || nom.Contains("ombre") || nom.Contains("peste") || nom.Contains("maudit"))
                            joueur.AppliquerStatut("Poison", 3.5f, Math.Max(2, (int)(p.Degats * 0.12f)));

                        p.EstMort = true;
                    }
                }
            }

            // Loots au sol
            for (int i = Loots.Count - 1; i >= 0; i--)
            {
                var l = Loots[i];
                l.MettreAJour(dt, joueur.Position);

                // Ramassage automatique si très proche
                if (Vector2.Distance(l.Position, joueur.Position) <= joueur.Rayon + 14f)
                {
                    AudioSynthetiseur.SonLoot();
                    Particules.EmettreEclats(l.Position, l.ObtenirCouleurLoot(), 10, 120f, 3f, 0.3f);
                    string nom;
                    if (l.ObjetEquipement is Equipement nouvelEquipement)
                    {
                        Equipement? equipe = nouvelEquipement.Type switch
                        {
                            TypeEquipement.Arme => joueur.ModeleHero.ArmeEquipee,
                            TypeEquipement.Armure => joueur.ModeleHero.ArmureEquipee,
                            TypeEquipement.Casque => joueur.ModeleHero.CasqueEquipe,
                            TypeEquipement.Anneau => joueur.ModeleHero.AnneauEquipe,
                            TypeEquipement.Amulette => joueur.ModeleHero.AmuletteEquipee,
                            _ => null
                        };
                        int attaque = nouvelEquipement.BonusAttaque + nouvelEquipement.NiveauAmelioration * 3 - (equipe?.BonusAttaque ?? 0) - (equipe?.NiveauAmelioration * 3 ?? 0);
                        int defense = nouvelEquipement.BonusDefense + nouvelEquipement.NiveauAmelioration * 2 - (equipe?.BonusDefense ?? 0) - (equipe?.NiveauAmelioration * 2 ?? 0);
                        int pv = nouvelEquipement.BonusPVMax + nouvelEquipement.NiveauAmelioration * 8 - (equipe?.BonusPVMax ?? 0) - (equipe?.NiveauAmelioration * 8 ?? 0);
                        int mana = nouvelEquipement.BonusManaMax - (equipe?.BonusManaMax ?? 0);
                        string ancien = equipe == null ? "nouvel emplacement" : $"vs {equipe.ObtenirNomComplet()}";
                        nom = $"{nouvelEquipement.ObtenirNomComplet()} {nouvelEquipement.RareteItem} ({ancien}) ATQ {attaque.ToString("+0;-0;0")} DEF {defense.ToString("+0;-0;0")} PV {pv.ToString("+0;-0;0")} MANA {mana.ToString("+0;-0;0")}";
                    }
                    else
                    {
                        nom = l.Type == TypeLootAuSol.Or ? $"+{l.Quantite} 🪙" : (l.Type == TypeLootAuSol.PierreForge ? $"+{l.Quantite} 💎 Pierres" : l.NomItem);
                    }
                    ajouterTexteFlottant(nom, l.ObtenirCouleurLoot(), true);
                    Loots.RemoveAt(i);
                }
            }
        }

        private void AnimerImpactProjectile(Projectile2D projectile, Vector2 position)
        {
            if (!projectile.EstDuJoueur || string.IsNullOrWhiteSpace(projectile.NomSort)) return;

            TypeAnimationSort2D? type = projectile.NomSort switch
            {
                "Flèche Explosive" => TypeAnimationSort2D.TirExplosif,
                "Foudre Céleste" => TypeAnimationSort2D.Foudre,
                "Âme Vorace" => TypeAnimationSort2D.EssaimAmes,
                "Tir d'Élite" or "Volée" => null,
                _ => TypeAnimationSort2D.SortForge
            };

            if (type.HasValue)
                Particules.EmettreAnimationSort(position, projectile.Velocite, projectile.Couleur, type.Value, 0.48f);
        }

        public void Dessiner(Graphics g, Camera2D cam, Joueur2D joueur, Font fontNormale, Font fontCritique)
        {
            RectangleF vue = cam.ObtenirZoneVisible(90f);

            // 1. Terrain riche multi-couche selon le biome
            DessinerTerrain(g, cam, vue, TempsTotal);

            // 2. Ombres portées au sol sous tous les obstacles pour donner du relief 2.5D
            foreach (var obs in Obstacles)
            {
                if (vue.IntersectsWith(obs.Boite))
                {
                    obs.DessinerOmbre(g, cam);
                }
            }

            // 3. Modèles de bâtiments, toitures, fontaine, arbres, piliers et lampadaires
            foreach (var obs in Obstacles)
            {
                if (vue.IntersectsWith(obs.Boite))
                {
                    obs.Dessiner(g, cam, TempsTotal);
                }
            }

            // 4. Objets interactifs & PNJ avec socles et auras
            foreach (var obj in ObjetsInteractifs)
            {
                bool proche = Vector2.Distance(joueur.Position, obj.Position) <= obj.Rayon + joueur.Rayon + 30f;
                obj.Dessiner(g, cam, proche, TempsTotal);
            }

            // 5. Loots au sol
            foreach (var l in Loots)
            {
                l.Dessiner(g, cam);
            }

            // 5.5 Zones de danger au sol (télégraphes des sorts de boss)
            for (int i = 0; i < ZonesDanger.Count; i++)
            {
                ZonesDanger[i].Dessiner(g, cam);
            }

            // 6. Monstres
            foreach (var m in Monstres)
            {
                m.Dessiner(g, cam);
            }

            // 6.5 Poules villageoises (Image 1)
            if (TypeZoneActuelle == ZoneType2D.Village)
            {
                for (int i = 0; i < Poules.Count; i++)
                {
                    Poules[i].Dessiner(g, cam, TempsTotal);
                }
            }

            // 7. Joueur
            joueur.Dessiner(g, cam);

            // Le bas des couronnes passe devant le héros lorsqu'il marche derrière un arbre.
            foreach (var obs in Obstacles)
            {
                if ((obs.TypeObstacle == TypeObstacle.Arbre || obs.TypeObstacle == TypeObstacle.ArbreGrand) &&
                    Math.Abs(joueur.Position.X - (obs.Boite.X + obs.Boite.Width / 2f)) <= obs.Boite.Width * (obs.TypeObstacle == TypeObstacle.ArbreGrand ? 0.95f : 0.80f) + joueur.Rayon &&
                    joueur.Position.Y >= obs.Boite.Y + obs.Boite.Height / 2f - obs.Boite.Width &&
                    joueur.Position.Y < obs.Boite.Bottom + 20f &&
                    vue.IntersectsWith(obs.Boite))
                {
                    obs.DessinerFeuillagePremierPlan(g, cam, TempsTotal);
                }
            }

            // 8. Projectiles
            foreach (var p in Projectiles)
            {
                p.Dessiner(g, cam);
            }

            // 9. Particules
            Particules.Dessiner(g, cam);

            // 10. Éclairage radial & halos lumineux d'ambiance
            DessinerHalosLumineux(g, cam, vue, TempsTotal);

            // 11. Textes flottants
            foreach (var t in Textes)
            {
                t.Dessiner(g, cam, fontNormale, fontCritique);
            }
        }

        private void DessinerTerrain(Graphics g, Camera2D cam, RectangleF vue, float tempsTotal)
        {
            switch (TypeZoneActuelle)
            {
                case ZoneType2D.Village:
                    DessinerTerrainVillage(g, cam, vue, tempsTotal);
                    break;
                case ZoneType2D.Donjon:
                    DessinerTerrainDonjon(g, cam, vue, tempsTotal);
                    break;
                case ZoneType2D.TourAstrale:
                    DessinerTerrainTourAstrale(g, cam, vue, tempsTotal);
                    break;
            }
        }

        private void DessinerTerrainVillage(Graphics g, Camera2D cam, RectangleF vue, float tempsTotal)
        {
            g.Clear(Color.FromArgb(31, 54, 36));

            // Rivière à l'Ouest (Image 1)
            DessinerRiviereOuest(g, cam, vue, tempsTotal);

            // Citadelle et Château en arrière-plan Nord (Image 1)
            DessinerChateauEtRempartsNord(g, cam, vue, tempsTotal);

            // Détails organiques de la pelouse : brins d'herbe, marguerites, boutons d'or, lavande et trèfles
            DessinerDetailsHerbeVillage(g, cam, vue, tempsTotal);

            // Réseau de voies pavées & esplanades
            // Avenue Nord (vers Donjon)
            DessinerPavesRoute(g, cam, EchelleZoneCapitale(new RectangleF(935, 170, 130, 430)), vue);
            // Avenue Ouest (vers Forge)
            DessinerPavesRoute(g, cam, EchelleZoneCapitale(new RectangleF(380, 420, 560, 100)), vue);
            // Parvis de la Forge
            DessinerPavesRoute(g, cam, EchelleZoneCapitale(new RectangleF(380, 420, 260, 90)), vue);
            // Avenue Est (vers Guilde & Tour Astrale)
            DessinerPavesRoute(g, cam, EchelleZoneCapitale(new RectangleF(1060, 420, 580, 100)), vue);
            DessinerPavesRoute(g, cam, EchelleZoneCapitale(new RectangleF(1060, 720, 620, 110)), vue);
            // Avenue Sud (vers Mannequin d'entraînement)
            DessinerPavesRoute(g, cam, EchelleZoneCapitale(new RectangleF(935, 930, 130, 420)), vue);
            // Allée Sud-Ouest (vers l'Échoppe d'Alchimie)
            DessinerPavesRoute(g, cam, EchelleZoneCapitale(new RectangleF(380, 1220, 560, 100)), vue);
            DessinerPavesRoute(g, cam, EchelleZoneCapitale(new RectangleF(870, 930, 90, 300)), vue);

            // Grande Place Centrale pavée entourant la fontaine
            DessinerPlazaCentrale(g, cam, EchellePositionCapitale(new Vector2(1000, 770)), 220f * FacteurEchelleCapitale);
        }

        private void DessinerDetailsHerbeVillage(Graphics g, Camera2D cam, RectangleF vue, float tempsTotal)
        {
            int pas = (int)(110 * FacteurEchelleCapitale);
            float marge = EchelleCoordonneeCapitale(60f);
            float startX = MathF.Floor(Math.Max(marge, vue.Left) / pas) * pas;
            float endX = MathF.Min(LargeurMonde - marge, vue.Right + pas);
            float startY = MathF.Floor(Math.Max(marge, vue.Top) / pas) * pas;
            float endY = MathF.Min(HauteurMonde - marge, vue.Bottom + pas);
            Vector2 centrePlace = EchellePositionCapitale(new Vector2(1000, 770));

            for (float x = startX; x <= endX; x += pas)
            {
                for (float y = startY; y <= endY; y += pas)
                {
                    // Ne pas dessiner de fleurs au milieu des routes principales ou de la place
                    if (Vector2.Distance(new Vector2(x, y), centrePlace) < 235f * FacteurEchelleCapitale) continue;
                    if (x >= EchelleCoordonneeCapitale(920) && x <= EchelleCoordonneeCapitale(1080) &&
                        y >= EchelleCoordonneeCapitale(160) && y <= EchelleCoordonneeCapitale(1360)) continue;
                    if (y >= EchelleCoordonneeCapitale(400) && y <= EchelleCoordonneeCapitale(540) &&
                        x >= EchelleCoordonneeCapitale(360) && x <= EchelleCoordonneeCapitale(1680)) continue;

                    int h = ((int)x * 73856093 ^ (int)y * 19349663) & 0x7fffffff;
                    float variationX = ((h >> 4) & 15) - 7;
                    float variationY = ((h >> 9) & 15) - 7;
                    Point p = cam.MondeVersEcran(new Vector2(x + variationX, y + variationY));
                    float vent = MathF.Sin(tempsTotal * 1.45f + x * 0.012f + y * 0.018f) * 2.2f * cam.Zoom;

                    int type = h % 16;
                    if (type <= 3) // Touffe de brins d'herbe élancés
                    {
                        Pen pHerbe = CacheRenduGDI.ObtenirPen(Color.FromArgb(68, 112, 56), 1.5f * cam.Zoom);
                        g.DrawLine(pHerbe, p.X, p.Y, p.X - 3 + vent, p.Y - 7 * cam.Zoom);
                        g.DrawLine(pHerbe, p.X + 2, p.Y, p.X + 2 + vent * 0.7f, p.Y - 9 * cam.Zoom);
                        g.DrawLine(pHerbe, p.X + 4, p.Y, p.X + 7 + vent, p.Y - 6 * cam.Zoom);
                    }
                    else if (type == 4) // Pâquerettes blanches des prés
                    {
                        float tailleFleur = Math.Max(2f, 5f * cam.Zoom);
                        float scintillement = 0.88f + 0.12f * MathF.Sin(tempsTotal * 2.4f + h);
                        int alphaFleur = Math.Clamp((int)(230 * scintillement), 0, 255);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alphaFleur, Color.WhiteSmoke), p.X - tailleFleur / 2, p.Y - tailleFleur / 2, tailleFleur, tailleFleur);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(255, 216, 95)), p.X - tailleFleur / 6, p.Y - tailleFleur / 6, tailleFleur / 3, tailleFleur / 3);
                    }
                    else if (type == 5) // Boutons d'or dorés
                    {
                        g.FillEllipse(Brushes.Gold, p.X - 2, p.Y - 2, 5, 5);
                        g.FillEllipse(Brushes.Gold, p.X + 5, p.Y - 4, 4, 4);
                    }
                    else if (type == 6) // Brins de lavande pourpre
                    {
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(170, 130, 215)), p.X, p.Y - 2, 4, 6);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(190, 150, 235)), p.X + 1, p.Y - 6, 3, 4);
                    }
                    else if (type == 7) // Trèfle vert tendre
                    {
                        Brush bTrefle = CacheRenduGDI.ObtenirBrush(Color.FromArgb(50, 125, 45));
                        g.FillEllipse(bTrefle, p.X - 3, p.Y - 2, 4, 4);
                        g.FillEllipse(bTrefle, p.X + 1, p.Y - 2, 4, 4);
                        g.FillEllipse(bTrefle, p.X - 1, p.Y - 5, 4, 4);
                    }
                    else if (type == 8 || type == 9) // Petites pierres et feuilles mortes
                    {
                        Color pierre = type == 8 ? Color.FromArgb(80, 91, 70) : Color.FromArgb(113, 87, 49);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(pierre), p.X - 3, p.Y - 1, 6, 3);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(115, 125, 90), 1f), p.X - 2, p.Y - 1, p.X, p.Y - 3);
                    }
                }
            }
        }

        private void DessinerPavesRoute(Graphics g, Camera2D cam, RectangleF routeMonde, RectangleF vue)
        {
            if (!vue.IntersectsWith(routeMonde)) return;

            Point p = cam.MondeVersEcran(new Vector2(routeMonde.X, routeMonde.Y));
            int w = (int)(routeMonde.Width * cam.Zoom);
            int h = (int)(routeMonde.Height * cam.Zoom);

            // Fond de pavés en pierre grise
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(55, 59, 67)), p.X, p.Y, w, h);
            g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(45, Color.FromArgb(160, 175, 180)), p.X + 2, p.Y + 2, Math.Max(0, w - 4), Math.Max(0, h - 4));
            // Bordures en pierre taillée le long des chaussées
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(125, 132, 137), 2f), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(30, 32, 39), 1f), p.X + 4, p.Y + 4, Math.Max(0, w - 8), Math.Max(0, h - 8));

            // Pavés imbriqués dessinés si la résolution le permet
            int taillePave = (int)(24 * cam.Zoom);
            if (taillePave >= 6)
            {
                Pen pJoint = CacheRenduGDI.ObtenirPen(Color.FromArgb(42, 45, 52), 1f);
                for (int y = p.Y; y < p.Y + h; y += taillePave)
                {
                    int decalage = ((y / taillePave) % 2) * (taillePave / 2);
                    for (int x = p.X + decalage; x < p.X + w; x += taillePave)
                    {
                        g.DrawRectangle(pJoint, x, y, taillePave, taillePave);
                    }
                }
            }
        }

        private void DessinerPlazaCentrale(Graphics g, Camera2D cam, Vector2 centreMonde, float rayonMonde)
        {
            Point pc = cam.MondeVersEcran(centreMonde);
            float r = rayonMonde * cam.Zoom;

            // Disque principal en pierre pavée
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(58, 62, 70)), pc.X - r, pc.Y - r, r * 2f, r * 2f);
            // Bordure extérieure en pierre de taille sculptée
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(85, 90, 102), 3f), pc.X - r, pc.Y - r, r * 2f, r * 2f);

            // Anneaux concentriques décoratifs
            float r1 = r * 0.72f;
            float r2 = r * 0.44f;
            Pen pAnneau = CacheRenduGDI.ObtenirPen(Color.FromArgb(74, 78, 88), 2f);
            g.DrawEllipse(pAnneau, pc.X - r1, pc.Y - r1, r1 * 2f, r1 * 2f);
            g.DrawEllipse(pAnneau, pc.X - r2, pc.Y - r2, r2 * 2f, r2 * 2f);

            // 8 Lignes radiales de dallage
            Pen pRayon = CacheRenduGDI.ObtenirPen(Color.FromArgb(48, 52, 60), 1.5f);
            for (int a = 0; a < 8; a++)
            {
                float angle = a * MathF.PI / 4f;
                int x1 = (int)(pc.X + MathF.Cos(angle) * r2);
                int y1 = (int)(pc.Y + MathF.Sin(angle) * r2);
                int x2 = (int)(pc.X + MathF.Cos(angle) * r);
                int y2 = (int)(pc.Y + MathF.Sin(angle) * r);
                g.DrawLine(pRayon, x1, y1, x2, y2);
            }
        }

        private void DessinerRiviereOuest(Graphics g, Camera2D cam, RectangleF vue, float tempsTotal)
        {
            RectangleF zoneRiviere = EchelleZoneCapitale(new RectangleF(110, 50, 110, HauteurMonde - 100));
            if (!vue.IntersectsWith(zoneRiviere)) return;

            Point p = cam.MondeVersEcran(new Vector2(zoneRiviere.X, zoneRiviere.Y));
            int w = (int)(zoneRiviere.Width * cam.Zoom);
            int h = (int)(zoneRiviere.Height * cam.Zoom);

            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(38, 115, 168)), p.X, p.Y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(25, 80, 120), 2f), p.X, p.Y, w, h);

            for (int yR = p.Y + 20; yR < p.Y + h - 20; yR += 35)
            {
                float ondul = MathF.Sin(tempsTotal * 2.2f + yR * 0.05f) * 6f;
                Pen pOndul = CacheRenduGDI.ObtenirPen(Color.FromArgb(170, 225, 255), 1.5f);
                g.DrawLine(pOndul, p.X + 8, yR + ondul, p.X + w - 8, yR + ondul);

                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(46, 125, 50), 2f), p.X + 4, yR, p.X + 6, yR - 10);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(46, 125, 50), 2f), p.X + w - 4, yR, p.X + w - 6, yR - 10);
            }
        }

        private void DessinerChateauEtRempartsNord(Graphics g, Camera2D cam, RectangleF vue, float tempsTotal)
        {
            RectangleF zoneChateau = EchelleZoneCapitale(new RectangleF(750, 40, 500, 130));
            if (!vue.IntersectsWith(zoneChateau)) return;

            Point p = cam.MondeVersEcran(new Vector2(zoneChateau.X, zoneChateau.Y));
            int w = (int)(zoneChateau.Width * cam.Zoom);
            int h = (int)(zoneChateau.Height * cam.Zoom);

            Point[] colline = {
                new Point(p.X - 30, p.Y + h),
                new Point(p.X + w / 2, p.Y + 15),
                new Point(p.X + w + 30, p.Y + h)
            };
            g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(85, 95, 75)), colline);
            g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(55, 65, 50), 2f), colline);

            int wDonjon = (int)(w * 0.44f);
            int hDonjon = (int)(h * 0.75f);
            int xDonjon = p.X + (w - wDonjon) / 2;
            int yDonjon = p.Y + (int)(h * 0.22f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(135, 138, 148)), xDonjon, yDonjon, wDonjon, hDonjon);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(70, 72, 80), 2f), xDonjon, yDonjon, wDonjon, hDonjon);

            int rTour = (int)(18 * cam.Zoom);
            int[] xTours = { xDonjon - rTour / 2, xDonjon + wDonjon - rTour / 2 };
            foreach (int xt in xTours)
            {
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(120, 125, 135)), xt, yDonjon - (int)(10 * cam.Zoom), rTour, hDonjon + (int)(10 * cam.Zoom));
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(60, 65, 72), 1.5f), xt, yDonjon - (int)(10 * cam.Zoom), rTour, hDonjon + (int)(10 * cam.Zoom));

                Point[] toitConique = {
                    new Point(xt - 4, yDonjon - (int)(10 * cam.Zoom)),
                    new Point(xt + rTour / 2, yDonjon - (int)(32 * cam.Zoom)),
                    new Point(xt + rTour + 4, yDonjon - (int)(10 * cam.Zoom))
                };
                g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(52, 90, 140)), toitConique);
                g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(30, 50, 80), 2f), toitConique);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.Gold, 2f), xt + rTour / 2, yDonjon - (int)(32 * cam.Zoom), xt + rTour / 2, yDonjon - (int)(42 * cam.Zoom));
                float flotterFanion = MathF.Sin(tempsTotal * 5f + xt) * 4f;
                Point[] fanion = {
                    new Point(xt + rTour / 2, yDonjon - (int)(42 * cam.Zoom)),
                    new Point(xt + rTour / 2 + (int)(12 * cam.Zoom), (int)(yDonjon - 39 * cam.Zoom + flotterFanion)),
                    new Point(xt + rTour / 2, yDonjon - (int)(36 * cam.Zoom))
                };
                g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(231, 76, 60)), fanion);
            }

            for (int b = 0; b < 4; b++)
            {
                int xBan = p.X + (int)(w * (0.15f + b * 0.25f));
                int yBan = p.Y + h - (int)(22 * cam.Zoom);
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(230, 126, 34)), xBan, yBan, (int)(14 * cam.Zoom), (int)(24 * cam.Zoom));
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Gold, 1.5f), xBan, yBan, (int)(14 * cam.Zoom), (int)(24 * cam.Zoom));
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.Gold), xBan + (int)(3 * cam.Zoom), yBan + (int)(5 * cam.Zoom), (int)(8 * cam.Zoom), (int)(8 * cam.Zoom));
            }
        }

        private void DessinerTerrainDonjon(Graphics g, Camera2D cam, RectangleF vue, float tempsTotal)
        {
            string zone = NomZone.ToLowerInvariant();
            bool volcan = zone.Contains("volcan") || zone.Contains("drak");
            bool catacombes = zone.Contains("catacomb");
            bool cosmique = zone.Contains("cosmique") || zone.Contains("astral");
            Color fond = volcan ? Color.FromArgb(31, 20, 19) : cosmique ? Color.FromArgb(14, 20, 34) : catacombes ? Color.FromArgb(19, 18, 31) : Color.FromArgb(22, 27, 25);
            g.Clear(fond);

            // Dalles anciennes de crypte en damier discret
            int pas = 90;
            float startX = MathF.Floor(Math.Max(50, vue.Left) / pas) * pas;
            float endX = MathF.Min(LargeurMonde - 50, vue.Right + pas);
            float startY = MathF.Floor(Math.Max(50, vue.Top) / pas) * pas;
            float endY = MathF.Min(HauteurMonde - 50, vue.Bottom + pas);

            for (float x = startX; x <= endX; x += pas)
            {
                for (float y = startY; y <= endY; y += pas)
                {
                    Point p = cam.MondeVersEcran(new Vector2(x, y));
                    int w = (int)(pas * cam.Zoom);
                    int h = (int)(pas * cam.Zoom);

                    bool alt = (((int)(x / pas) + (int)(y / pas)) % 2) == 0;
                    Color cDalle = volcan
                        ? (alt ? Color.FromArgb(39, 28, 25) : Color.FromArgb(28, 22, 23))
                        : cosmique
                            ? (alt ? Color.FromArgb(23, 31, 49) : Color.FromArgb(18, 25, 41))
                            : catacombes
                                ? (alt ? Color.FromArgb(29, 27, 43) : Color.FromArgb(21, 20, 34))
                                : (alt ? Color.FromArgb(29, 36, 34) : Color.FromArgb(23, 30, 29));
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(cDalle), p.X, p.Y, w, h);
                    g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(12, 10, 16), 1.5f), p.X, p.Y, w, h);
                    int inset = Math.Max(2, (int)(5 * cam.Zoom));
                    Color refletDalle = volcan ? Color.FromArgb(26, 231, 105, 36) : cosmique ? Color.FromArgb(32, 80, 190, 255) : catacombes ? Color.FromArgb(26, 150, 100, 220) : Color.FromArgb(24, 115, 150, 125);
                    g.DrawLine(CacheRenduGDI.ObtenirPen(refletDalle, 1f), p.X + inset, p.Y + inset, p.X + w - inset, p.Y + inset);

                    // Fissures de pierre occasionnelles
                    int seed = ((int)x * 37 ^ (int)y * 79) & 0x7fffffff;
                    if (seed % 7 == 0)
                    {
                        Color fissure = volcan ? Color.FromArgb(95, 255, 92, 30) : cosmique ? Color.FromArgb(85, 65, 170, 255) : Color.FromArgb(65, 6, 7, 15);
                        Pen pFissure = CacheRenduGDI.ObtenirPen(fissure, 1f);
                        g.DrawLine(pFissure, p.X + 4, p.Y + 4, p.X + w / 2, p.Y + h / 2);
                        g.DrawLine(pFissure, p.X + w / 2, p.Y + h / 2, p.X + w - 6, p.Y + h / 3);
                    }
                    else if ((seed & 15) == 3 && (catacombes || cosmique))
                    {
                        Color rune = cosmique ? Color.FromArgb(45, 95, 190, 255) : Color.FromArgb(40, 175, 120, 215);
                        float rayonRune = Math.Max(2f, 4f * cam.Zoom);
                        g.DrawEllipse(CacheRenduGDI.ObtenirPen(rune, 1f), p.X + w * 0.5f - rayonRune, p.Y + h * 0.5f - rayonRune, rayonRune * 2, rayonRune * 2);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(rune, 1f), p.X + w * 0.5f - rayonRune * 0.65f, p.Y + h * 0.5f, p.X + w * 0.5f + rayonRune * 0.65f, p.Y + h * 0.5f);
                    }
                }
            }

            if (volcan)
            {
                DessinerFissureMagma(g, cam, tempsTotal, new Vector2[] {
                    new Vector2(400, 650), new Vector2(850, 780), new Vector2(1400, 720), new Vector2(2000, 820), new Vector2(2500, 740)
                });
                DessinerFissureMagma(g, cam, tempsTotal, new Vector2[] {
                    new Vector2(600, 1350), new Vector2(1100, 1280), new Vector2(1700, 1420), new Vector2(2300, 1320)
                });
            }

            // Sceau runique rituel de Boss
            Vector2 centreBoss = (NomZone.StartsWith("Arène")) ? new Vector2(1650, 900) : new Vector2(2300, 1000);
            if (vue.IntersectsWith(new RectangleF(centreBoss.X - 230f, centreBoss.Y - 230f, 460f, 460f)))
                DessinerSceauBoss(g, cam, centreBoss, 210f, tempsTotal);
        }

        private void DessinerFissureMagma(Graphics g, Camera2D cam, float tempsTotal, Vector2[] points)
        {
            float glow = 0.82f + 0.18f * MathF.Sin(tempsTotal * 4.5f);
            Pen penHalo = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(70 * glow), 210, 50, 15), 18f * cam.Zoom);
            Pen penLave = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(190 * glow), 255, 105, 25), 7f * cam.Zoom);
            Pen penCoeur = CacheRenduGDI.ObtenirPen(Color.FromArgb(255, 255, 230, 120), 2.5f * cam.Zoom);

            for (int i = 0; i < points.Length - 1; i++)
            {
                Point p1 = cam.MondeVersEcran(points[i]);
                Point p2 = cam.MondeVersEcran(points[i + 1]);
                g.DrawLine(penHalo, p1, p2);
                g.DrawLine(penLave, p1, p2);
                g.DrawLine(penCoeur, p1, p2);
            }
        }

        private void DessinerSceauBoss(Graphics g, Camera2D cam, Vector2 centre, float rayon, float tempsTotal)
        {
            Point pc = cam.MondeVersEcran(centre);
            float r = rayon * cam.Zoom;
            float glow = 0.85f + 0.15f * MathF.Sin(tempsTotal * 3f);

            Pen penSceau = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(160 * glow), 235, 60, 40), 2f);
            Pen penSceauFin = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(120 * glow), 241, 196, 15), 1.5f);

            // Cercles concentriques
            g.DrawEllipse(penSceau, pc.X - r, pc.Y - r, r * 2f, r * 2f);
            g.DrawEllipse(penSceauFin, pc.X - r * 0.82f, pc.Y - r * 0.82f, r * 1.64f, r * 1.64f);
            g.DrawEllipse(penSceau, pc.X - r * 0.45f, pc.Y - r * 0.45f, r * 0.90f, r * 0.90f);

            // Étoile octogonale / Sceau ésotérique
            for (int i = 0; i < 8; i++)
            {
                float a1 = i * MathF.PI / 4f + tempsTotal * 0.15f;
                float a2 = (i + 3) * MathF.PI / 4f + tempsTotal * 0.15f;
                int x1 = (int)(pc.X + MathF.Cos(a1) * (r * 0.82f));
                int y1 = (int)(pc.Y + MathF.Sin(a1) * (r * 0.82f));
                int x2 = (int)(pc.X + MathF.Cos(a2) * (r * 0.82f));
                int y2 = (int)(pc.Y + MathF.Sin(a2) * (r * 0.82f));
                g.DrawLine(penSceauFin, x1, y1, x2, y2);

                // Glyphes stellaires sur le pourtour
                int gx = (int)(pc.X + MathF.Cos(a1) * (r * 0.91f));
                int gy = (int)(pc.Y + MathF.Sin(a1) * (r * 0.91f));
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb((int)(200 * glow), 241, 196, 15)), gx - 3, gy - 3, 6, 6);
            }
        }

        private void DessinerTerrainTourAstrale(Graphics g, Camera2D cam, RectangleF vue, float tempsTotal)
        {
            g.Clear(Color.FromArgb(8, 7, 20));

            // Nébuleuses galactiques diffuses
            DessinerNebuleuse(g, cam, new Vector2(600, 600), 550f, 380f, Color.FromArgb(24, 120, 50, 210));
            DessinerNebuleuse(g, cam, new Vector2(1300, 1100), 650f, 440f, Color.FromArgb(22, 30, 95, 215));
            DessinerNebuleuse(g, cam, new Vector2(950, 450), 480f, 320f, Color.FromArgb(20, 20, 185, 215));

            // Constellations : Lignes stellaires éthérées
            Vector2[] starsConstel = {
                new Vector2(480, 480), new Vector2(900, 350), new Vector2(1260, 480),
                new Vector2(1420, 750), new Vector2(1260, 1220), new Vector2(900, 1400),
                new Vector2(480, 1220), new Vector2(380, 750)
            };
            Pen pConstel = CacheRenduGDI.ObtenirPen(Color.FromArgb(55, 100, 200, 255), 1.5f);
            for (int i = 0; i < starsConstel.Length; i++)
            {
                Point p1 = cam.MondeVersEcran(starsConstel[i]);
                Point p2 = cam.MondeVersEcran(starsConstel[(i + 1) % starsConstel.Length]);
                g.DrawLine(pConstel, p1, p2);

                Point pC = cam.MondeVersEcran(new Vector2(900, 750));
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(30, 80, 180, 255), 1f), p1, pC);
            }

            // Champ d'étoiles scintillantes sur le sol céleste
            for (int i = 0; i < 180; i++)
            {
                float sx = 120f + ((i * 137.5f) % 1560f);
                float sy = 120f + ((i * 269.3f) % 1560f);

                if (sx < vue.Left - 50 || sx > vue.Right + 50 || sy < vue.Top - 50 || sy > vue.Bottom + 50) continue;

                Point p = cam.MondeVersEcran(new Vector2(sx, sy));
                float phase = MathF.Sin(tempsTotal * 2.8f + i * 0.7f);
                int alpha = Math.Clamp((int)(140 + 100 * phase), 40, 255);

                Color colEtoile = (i % 5) switch
                {
                    0 => Color.FromArgb(alpha, 140, 230, 255), // Cyan
                    1 => Color.FromArgb(alpha, 255, 230, 140), // Or
                    2 => Color.FromArgb(alpha, 240, 150, 255), // Rose astral
                    _ => Color.FromArgb(alpha, 255, 255, 255)  // Blanc diamant
                };

                float rE = ((i % 4 == 0) ? 3.5f : 2.0f) * cam.Zoom;
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(colEtoile), p.X - rE, p.Y - rE, rE * 2f, rE * 2f);

                // Scintillement en croix pour les grandes étoiles
                if (i % 6 == 0 && alpha > 160)
                {
                    Pen pGlint = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(alpha * 0.6f), colEtoile), 1f);
                    g.DrawLine(pGlint, p.X - rE * 2.2f, p.Y, p.X + rE * 2.2f, p.Y);
                    g.DrawLine(pGlint, p.X, p.Y - rE * 2.2f, p.X, p.Y + rE * 2.2f);
                }
            }

            // Cercle astral central avec coordonnées célestes
            Point pcTour = cam.MondeVersEcran(new Vector2(900, 750));
            float rTour = 180f * cam.Zoom;
            Pen pTour = CacheRenduGDI.ObtenirPen(Color.FromArgb(90, 100, 220, 255), 2f);
            g.DrawEllipse(pTour, pcTour.X - rTour, pcTour.Y - rTour, rTour * 2f, rTour * 2f);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(60, 160, 240, 255), 1.5f), pcTour.X - rTour * 0.65f, pcTour.Y - rTour * 0.65f, rTour * 1.30f, rTour * 1.30f);
        }

        private void DessinerNebuleuse(Graphics g, Camera2D cam, Vector2 centre, float w, float h, Color couleur)
        {
            Point p = cam.MondeVersEcran(centre);
            float we = w * cam.Zoom;
            float he = h * cam.Zoom;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(couleur), p.X - we / 2f, p.Y - he / 2f, we, he);
        }

        private void DessinerHalosLumineux(Graphics g, Camera2D cam, RectangleF vue, float tempsTotal)
        {
            switch (TypeZoneActuelle)
            {
                case ZoneType2D.Village:
                    // Lampadaires en fer forgé
                    foreach (var obs in Obstacles)
                    {
                        if (obs.TypeObstacle == TypeObstacle.Lampadaire && vue.IntersectsWith(obs.Boite))
                        {
                            Point p = cam.MondeVersEcran(new Vector2(obs.Boite.X + obs.Boite.Width / 2f, obs.Boite.Y + obs.Boite.Height / 2f));
                            float flicker = 0.90f + 0.10f * MathF.Sin(tempsTotal * 8f + obs.Boite.X);
                            DessinerHaloRadial(g, p, 110f * cam.Zoom * flicker, Color.FromArgb(255, 215, 90), 38);
                        }
                    }

                    // Lueur du fourneau de la Forge de Brom
                    Point pForge = cam.MondeVersEcran(EchellePositionCapitale(new Vector2(510, 390)));
                    float fForge = 0.85f + 0.15f * MathF.Sin(tempsTotal * 6.5f);
                    DessinerHaloRadial(g, pForge, 170f * FacteurEchelleCapitale * cam.Zoom * fForge, Color.FromArgb(255, 120, 25), 45);

                    // Lueur d'émeraude de l'Apothicaire Sylas
                    Point pAlch = cam.MondeVersEcran(EchellePositionCapitale(new Vector2(490, 1190)));
                    float fAlch = 0.80f + 0.20f * MathF.Sin(tempsTotal * 4f);
                    DessinerHaloRadial(g, pAlch, 140f * FacteurEchelleCapitale * cam.Zoom * fAlch, Color.FromArgb(35, 215, 120), 40);

                    // Lueur royale saphir de la Guilde des Aventuriers
                    Point pGuilde = cam.MondeVersEcran(EchellePositionCapitale(new Vector2(1470, 380)));
                    DessinerHaloRadial(g, pGuilde, 160f * FacteurEchelleCapitale * cam.Zoom, Color.FromArgb(100, 150, 255), 40);

                    // Lueur cristalline turquoise de la Grande Fontaine
                    Point pFont = cam.MondeVersEcran(EchellePositionCapitale(new Vector2(1000, 770)));
                    DessinerHaloRadial(g, pFont, 160f * FacteurEchelleCapitale * cam.Zoom, Color.FromArgb(40, 160, 220), 35);

                    // Lueur arcanique de l'atelier de Maître Kaëlith
                    Point pKaelith = cam.MondeVersEcran(EchellePositionCapitale(new Vector2(700, 470)));
                    DessinerHaloRadial(g, pKaelith, 130f * FacteurEchelleCapitale * cam.Zoom, Color.FromArgb(30, 180, 255), 40);
                    break;

                case ZoneType2D.Donjon:
                    // Braserots et torches
                    foreach (var obs in Obstacles)
                    {
                        if (obs.TypeObstacle == TypeObstacle.Brasero && vue.IntersectsWith(obs.Boite))
                        {
                            Point p = cam.MondeVersEcran(new Vector2(obs.Boite.X + obs.Boite.Width / 2f, obs.Boite.Y + obs.Boite.Height / 2f));
                            float flicker = 0.82f + 0.18f * MathF.Sin(tempsTotal * 9f + obs.Boite.X);
                            DessinerHaloRadial(g, p, 130f * cam.Zoom * flicker, Color.FromArgb(255, 110, 30), 45);
                        }
                    }
                    break;

                case ZoneType2D.TourAstrale:
                    // Piliers astraux mystiques
                    foreach (var obs in Obstacles)
                    {
                        if (obs.TypeObstacle == TypeObstacle.PilierAstral && vue.IntersectsWith(obs.Boite))
                        {
                            Point p = cam.MondeVersEcran(new Vector2(obs.Boite.X + obs.Boite.Width / 2f, obs.Boite.Y + obs.Boite.Height / 2f));
                            float fAura = 0.85f + 0.15f * MathF.Sin(tempsTotal * 4f + obs.Boite.X);
                            DessinerHaloRadial(g, p, 120f * cam.Zoom * fAura, Color.FromArgb(60, 200, 255), 42);
                        }
                    }
                    break;
            }

            // Halos lumineux autour des portails
            foreach (var obj in ObjetsInteractifs)
            {
                if (obj.Type == TypeInteractif.PortailDonjon && cam.EstVisible(obj.Position, 160f))
                {
                    Point p = cam.MondeVersEcran(obj.Position);
                    DessinerHaloRadial(g, p, 130f * cam.Zoom, Color.FromArgb(235, 70, 40), 55);
                }
                else if (obj.Type == TypeInteractif.PortailTour && cam.EstVisible(obj.Position, 160f))
                {
                    Point p = cam.MondeVersEcran(obj.Position);
                    DessinerHaloRadial(g, p, 130f * cam.Zoom, Color.FromArgb(40, 150, 240), 55);
                }
                else if (obj.Type == TypeInteractif.PortailProchainEtage && cam.EstVisible(obj.Position, 160f))
                {
                    Point p = cam.MondeVersEcran(obj.Position);
                    DessinerHaloRadial(g, p, 140f * cam.Zoom, Color.FromArgb(160, 70, 210), 60);
                }
            }
        }

        private void DessinerHaloRadial(Graphics g, Point centre, float rayon, Color couleur, int alphaMax = 40)
        {
            int r1 = (int)rayon;
            int r2 = (int)(rayon * 0.65f);
            int r3 = (int)(rayon * 0.35f);

            if (r1 > 2) g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha((int)(alphaMax * 0.25f), couleur), centre.X - r1, centre.Y - r1, r1 * 2, r1 * 2);
            if (r2 > 2) g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha((int)(alphaMax * 0.50f), couleur), centre.X - r2, centre.Y - r2, r2 * 2, r2 * 2);
            if (r3 > 2) g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alphaMax, couleur), centre.X - r3, centre.Y - r3, r3 * 2, r3 * 2);
        }
    }
}
