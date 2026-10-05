using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace JeuxRPG
{
    // ==============================================================
    // SYSTÈME DE PARTICULES 2D HAUTE PERFORMANCE & EFFETS VISUELS
    // ==============================================================
    public enum TypeParticule { Disque, OndeChoc, LigneVitesse, AuraMote, Etoile, Flamme, Cristal, Rune, Petale, Brume, Goutte }
    public enum TypeAnimationSort2D 
    { 
        Tourbillon, NovaFeu, Volee, Chatiment, FauxOmbre, FracasTellurique, Foudre, TirExplosif, RayonSacre, EssaimAmes, SortForge, Ultime,
        // Nouveaux sorts issus des visuels (Images 2 & 3)
        DragonFlammes, PhenixGivre, TrombeTempete, DomeSacre, VortexAmes, OrbeFoudre,
        CroissantLunaire, DragonTricephale, LeviathanGlaces, MiroirGlace, SingulariteTellurique,
        PrismeIrise, AbysseEldritch, PortailGlacial
    }

    public class EffetSortAnime
    {
        public Vector2 Position;
        public Vector2 Direction;
        public Color Couleur;
        public TypeAnimationSort2D Type;
        public float Duree;
        public float TempsRestant;
        public float TempsEcoule;

        public bool EstTermine => TempsRestant <= 0f;

        public void MettreAJour(float dt)
        {
            TempsRestant -= dt;
            TempsEcoule += dt;
        }

        public void Dessiner(Graphics g, Camera2D cam)
        {
            if (EstTermine || !cam.EstVisible(Position, 240f)) return;

            Point p = cam.MondeVersEcran(Position);
            float progression = Math.Clamp(1f - TempsRestant / Math.Max(0.01f, Duree), 0f, 1f);
            float ouverture = 1f - MathF.Pow(1f - progression, 4f);
            float apparition = Math.Clamp(progression / 0.12f, 0f, 1f);
            float disparition = Math.Clamp((1f - progression) / 0.3f, 0f, 1f);
            int alpha = (int)(255f * apparition * disparition);
            float pulse = 0.9f + MathF.Sin(TempsEcoule * 23f) * 0.1f;
            float rayon = (22f + ouverture * 104f) * pulse * cam.Zoom;
            float angle = MathF.Atan2(Direction.Y, Direction.X) * 180f / MathF.PI;
            GraphicsState etat = g.Save();
            g.TranslateTransform(p.X, p.Y);

            if (Type is TypeAnimationSort2D.Volee or TypeAnimationSort2D.FracasTellurique or TypeAnimationSort2D.Foudre or TypeAnimationSort2D.TirExplosif or TypeAnimationSort2D.DragonFlammes or TypeAnimationSort2D.PhenixGivre or TypeAnimationSort2D.DragonTricephale or TypeAnimationSort2D.CroissantLunaire or TypeAnimationSort2D.LeviathanGlaces)
                g.RotateTransform(angle);

            switch (Type)
            {
                case TypeAnimationSort2D.Tourbillon:
                    for (int i = 0; i < 4; i++)
                    {
                        float r = rayon * (0.34f + i * 0.19f);
                        float rotation = TempsEcoule * (420f + i * 65f) + i * 91f;
                        float aplatissement = 0.46f + i * 0.07f;
                        g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Couleur), (8f - i) * cam.Zoom), -r, -r * aplatissement, r * 2f, r * aplatissement * 2f, rotation, 138f + i * 13f);
                        g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), Math.Max(1f, 2.5f * cam.Zoom)), -r, -r * aplatissement, r * 2f, r * aplatissement * 2f, rotation + 4f, 48f);
                    }
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Couleur), 2f * cam.Zoom), -rayon * 0.16f, -rayon * 0.16f, rayon * 0.32f, rayon * 0.32f);
                    break;

                case TypeAnimationSort2D.NovaFeu:
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * MathF.Tau / 12f + TempsEcoule * (i % 2 == 0 ? 0.55f : -0.35f);
                        float interieur = rayon * (0.56f + (i % 3) * 0.08f);
                        float exterieur = rayon * (1.02f + (i % 2) * 0.2f);
                        float x1 = MathF.Cos(a) * interieur;
                        float y1 = MathF.Sin(a) * interieur;
                        float x2 = MathF.Cos(a) * exterieur;
                        float y2 = MathF.Sin(a) * exterieur;
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(255, 255, 115, 28)), 7f * cam.Zoom), x1, y1, x2, y2);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(255, 255, 226, 154)), 2.4f * cam.Zoom), x1, y1, x2, y2);
                    }
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(255, 255, 238, 190)), 4f * cam.Zoom), -rayon * ouverture * 0.7f, -rayon * ouverture * 0.7f, rayon * ouverture * 1.4f, rayon * ouverture * 1.4f);
                    break;

                case TypeAnimationSort2D.Volee:
                    for (int i = -2; i <= 2; i++)
                    {
                        float y = i * rayon * 0.2f;
                        float x = (progression * 1.8f - 0.7f) * rayon + Math.Abs(i) * rayon * 0.16f;
                        float longueur = rayon * (0.5f + (2 - Math.Abs(i)) * 0.13f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Couleur), 6f * cam.Zoom), x - longueur, y, x + longueur, y - i * rayon * 0.08f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(245, 235, 215)), 2f * cam.Zoom), x - longueur, y, x + longueur, y - i * rayon * 0.08f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Couleur), 2.5f * cam.Zoom), x + longueur, y - i * rayon * 0.08f, x + longueur * 0.62f, y - i * rayon * 0.08f - rayon * 0.12f);
                    }
                    break;

                case TypeAnimationSort2D.Chatiment:
                    float rayonSceau = rayon * (0.45f + ouverture * 0.55f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Color.Gold), 5f * cam.Zoom), -rayonSceau, -rayonSceau * 0.32f, rayonSceau * 2f, rayonSceau * 0.64f);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(alpha / 4, Color.Gold), -rayon * 0.48f, -rayon * (1.9f + ouverture), rayon * 0.96f, rayon * (1.9f + ouverture));
                    g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(alpha, Color.White), -rayon * 0.12f, -rayon * (1.8f + ouverture), rayon * 0.24f, rayon * (1.8f + ouverture));
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * MathF.Tau / 8f + TempsEcoule * 0.6f;
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Gold), 2.5f * cam.Zoom), MathF.Cos(a) * rayonSceau * 0.65f, MathF.Sin(a) * rayonSceau * 0.65f, MathF.Cos(a) * rayonSceau, MathF.Sin(a) * rayonSceau);
                    }
                    break;

                case TypeAnimationSort2D.FauxOmbre:
                    float balayage = -125f + ouverture * 250f;
                    g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 3, Couleur), 18f * cam.Zoom), -rayon, -rayon, rayon * 2f, rayon * 2f, balayage, 180f);
                    g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Couleur), 8f * cam.Zoom), -rayon, -rayon, rayon * 2f, rayon * 2f, balayage, 180f);
                    g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(238, 221, 255)), 2.5f * cam.Zoom), -rayon * 0.94f, -rayon * 0.94f, rayon * 1.88f, rayon * 1.88f, balayage + 4f, 155f);
                    g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 4f * cam.Zoom), MathF.Cos((balayage + 180f) * MathF.PI / 180f) * rayon, MathF.Sin((balayage + 180f) * MathF.PI / 180f) * rayon, MathF.Cos(balayage * MathF.PI / 180f) * rayon, MathF.Sin(balayage * MathF.PI / 180f) * rayon);
                    break;

                case TypeAnimationSort2D.FracasTellurique:
                    for (int i = 0; i < 4; i++)
                    {
                        float depart = (i - 1.5f) * rayon * 0.48f;
                        float x = depart + (progression * 1.4f - 0.35f) * rayon;
                        float hauteur = rayon * (0.38f + (i % 2) * 0.3f) * (0.35f + ouverture * 0.65f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Couleur), 12f * cam.Zoom), x - rayon * 0.16f, rayon * 0.16f, x, -hauteur);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(255, 255, 118, 54)), 6f * cam.Zoom), x - rayon * 0.16f, rayon * 0.16f, x, -hauteur);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(255, 255, 223, 157)), 2f * cam.Zoom), x, -hauteur, x + rayon * 0.23f, rayon * 0.12f);
                    }
                    g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Color.FromArgb(220, 100, 55)), 3f * cam.Zoom), -rayon, rayon * 0.28f, rayon, rayon * 0.28f);
                    break;

                case TypeAnimationSort2D.Foudre:
                    float avance = (progression - 0.5f) * rayon * 0.9f;
                    Pen penHaloFoudre = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 3, Couleur), 15f * cam.Zoom);
                    Pen penFoudre = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 3.5f * cam.Zoom);
                    g.DrawLine(penHaloFoudre, -rayon * 1.2f, 0, rayon * 1.2f, 0);
                    g.DrawLine(penFoudre, -rayon, 0, -rayon * 0.62f + avance, -rayon * 0.3f);
                    g.DrawLine(penFoudre, -rayon * 0.62f + avance, -rayon * 0.3f, -rayon * 0.24f, rayon * 0.2f);
                    g.DrawLine(penFoudre, -rayon * 0.24f, rayon * 0.2f, rayon * 0.12f + avance, -rayon * 0.42f);
                    g.DrawLine(penFoudre, rayon * 0.12f + avance, -rayon * 0.42f, rayon * 0.42f, rayon * 0.15f);
                    g.DrawLine(penFoudre, rayon * 0.42f, rayon * 0.15f, rayon, 0);
                    g.DrawLine(penFoudre, -rayon * 0.24f, rayon * 0.2f, -rayon * 0.12f + avance, rayon * 0.78f);
                    g.DrawLine(penFoudre, rayon * 0.42f, rayon * 0.15f, rayon * 0.65f, rayon * 0.62f);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), rayon * 0.75f, -rayon * 0.12f, rayon * 0.24f, rayon * 0.24f);
                    break;

                case TypeAnimationSort2D.TirExplosif:
                    float explosion = rayon * ouverture;
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha / 4, Couleur), -explosion * 0.42f, -explosion * 0.42f, explosion * 0.84f, explosion * 0.84f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(255, 255, 207, 120)), 4f * cam.Zoom), -explosion, -explosion, explosion * 2f, explosion * 2f);
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i * MathF.Tau / 10f;
                        float interieur = explosion * (0.45f + (i % 2) * 0.12f);
                        float exterieur = explosion * (1.08f + (i % 3) * 0.12f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.FromArgb(255, 255, 137, 35)), 4f * cam.Zoom), MathF.Cos(a) * interieur, MathF.Sin(a) * interieur, MathF.Cos(a) * exterieur, MathF.Sin(a) * exterieur);
                    }
                    break;

                case TypeAnimationSort2D.RayonSacre:
                    float largeurRayon = rayon * (0.18f + ouverture * 0.28f);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(alpha / 5, Color.Gold), -largeurRayon, -rayon * 3f, largeurRayon * 2f, rayon * 3.6f);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(alpha, Color.White), -largeurRayon * 0.18f, -rayon * 2.8f, largeurRayon * 0.36f, rayon * 3.4f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Gold), 4f * cam.Zoom), -rayon * ouverture, -rayon * 0.3f, rayon * ouverture * 2f, rayon * 0.6f);
                    for (int i = 0; i < 4; i++)
                    {
                        float x = (i - 1.5f) * rayon * 0.55f;
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Color.White), 1.5f * cam.Zoom), x, -rayon * 2.4f, x * 0.5f, rayon * 0.25f);
                    }
                    break;

                case TypeAnimationSort2D.EssaimAmes:
                    for (int i = 0; i < 6; i++)
                    {
                        float a = TempsEcoule * (3f + i % 3) + i * MathF.Tau / 6f;
                        float orbit = rayon * (0.34f + (i % 3) * 0.15f);
                        float x = MathF.Cos(a) * orbit;
                        float y = MathF.Sin(a * 1.17f) * orbit * 0.62f;
                        float rAme = rayon * (0.07f + (i % 2) * 0.025f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Couleur), 2f * cam.Zoom), x - rAme, y + rAme * 2f, x - rAme * 1.4f, y + rAme * 4f);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha / 3, Couleur), x - rAme * 1.8f, y - rAme * 1.8f, rAme * 3.6f, rAme * 3.6f);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(235, 220, 255)), x - rAme, y - rAme, rAme * 2f, rAme * 2f);
                    }
                    break;

                case TypeAnimationSort2D.SortForge:
                    float rotationForge = TempsEcoule * 110f;
                    g.RotateTransform(rotationForge);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Couleur), 12f * cam.Zoom), -rayon * 0.44f, -rayon * 0.44f, rayon * 0.88f, rayon * 0.88f);
                    g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Gold), 3f * cam.Zoom), -rayon * 0.72f, -rayon * 0.72f, rayon * 1.44f, rayon * 1.44f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2f * cam.Zoom), -rayon * 0.52f, -rayon * 0.52f, rayon * 1.04f, rayon * 1.04f);
                    g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2f * cam.Zoom), -rayon, 0, rayon, 0);
                    g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2f * cam.Zoom), 0, -rayon, 0, rayon);
                    break;

                case TypeAnimationSort2D.Ultime:
                    float rayonUltime = rayon * (0.35f + ouverture * 0.8f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 3, Color.Gold), 18f * cam.Zoom), -rayonUltime, -rayonUltime, rayonUltime * 2f, rayonUltime * 2f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Gold), 6f * cam.Zoom), -rayonUltime, -rayonUltime, rayonUltime * 2f, rayonUltime * 2f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2f * cam.Zoom), -rayonUltime * 0.62f, -rayonUltime * 0.62f, rayonUltime * 1.24f, rayonUltime * 1.24f);
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * MathF.Tau / 12f + TempsEcoule * (i % 2 == 0 ? 0.7f : -0.45f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, i % 2 == 0 ? Couleur : Color.White), 4f * cam.Zoom), MathF.Cos(a) * rayonUltime * 0.55f, MathF.Sin(a) * rayonUltime * 0.55f, MathF.Cos(a) * rayonUltime * 1.28f, MathF.Sin(a) * rayonUltime * 1.28f);
                    }
                    break;

                case TypeAnimationSort2D.DragonFlammes:
                    // Wyrm de feu ardent serpentant en arc et crachant des orbes de feu (Image 2)
                    float rD = rayon * 0.95f;
                    Pen penCorpsFeuExt = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 231, 76, 60), 14f * cam.Zoom);
                    Pen penCorpsFeuInt = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 241, 196, 15), 7f * cam.Zoom);
                    Pen penCorpsFeuCoeur = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2.5f * cam.Zoom);
                    g.DrawArc(penCorpsFeuExt, -rD, -rD * 0.7f, rD * 1.8f, rD * 1.4f, -40f, 210f);
                    g.DrawArc(penCorpsFeuInt, -rD, -rD * 0.7f, rD * 1.8f, rD * 1.4f, -35f, 200f);
                    g.DrawArc(penCorpsFeuCoeur, -rD, -rD * 0.7f, rD * 1.8f, rD * 1.4f, -30f, 190f);
                    float têteX = rD * 0.75f;
                    float têteY = -rD * 0.2f;
                    PointF[] polyTeteFeu = {
                        new PointF(têteX - 10f * cam.Zoom, têteY - 14f * cam.Zoom),
                        new PointF(têteX + 22f * cam.Zoom, têteY - 2f * cam.Zoom),
                        new PointF(têteX + 8f * cam.Zoom, têteY + 12f * cam.Zoom),
                        new PointF(têteX - 8f * cam.Zoom, têteY + 6f * cam.Zoom)
                    };
                    g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 230, 80, 30)), polyTeteFeu);
                    g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Gold), 2f * cam.Zoom), polyTeteFeu);
                    g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Gold), 3f * cam.Zoom), têteX - 5f * cam.Zoom, têteY - 12f * cam.Zoom, têteX - 18f * cam.Zoom, têteY - 22f * cam.Zoom);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), têteX + 6f * cam.Zoom, têteY - 4f * cam.Zoom, 4f * cam.Zoom, 4f * cam.Zoom);
                    for (int o = 1; o <= 3; o++)
                    {
                        float oDist = (o * 28f + TempsEcoule * 45f) * cam.Zoom;
                        float oTaille = (8f - o * 1.5f) * cam.Zoom;
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 255, 100, 20)), têteX + oDist, têteY + (o * 8f - 10f) * cam.Zoom, oTaille * 2, oTaille * 2);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, Color.White)), têteX + oDist + oTaille * 0.4f, têteY + (o * 8f - 10f) * cam.Zoom + oTaille * 0.4f, oTaille * 1.2f, oTaille * 1.2f);
                    }
                    break;

                case TypeAnimationSort2D.PhenixGivre:
                    // Phénix de glace cristalline fendant l'air avec ailes déployées (Image 2)
                    float rPh = rayon * 0.9f;
                    Pen pGivreExt = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 120, 220, 255), 3f * cam.Zoom);
                    Pen pGivreInt = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 1.5f * cam.Zoom);
                    PointF[] aileG = {
                        new PointF(0, -6f * cam.Zoom),
                        new PointF(-rPh * 0.6f, -rPh * 0.85f),
                        new PointF(-rPh * 0.3f, -rPh * 0.4f),
                        new PointF(-rPh * 0.9f, -rPh * 0.7f),
                        new PointF(0, 0)
                    };
                    PointF[] aileD = {
                        new PointF(0, 6f * cam.Zoom),
                        new PointF(-rPh * 0.6f, rPh * 0.85f),
                        new PointF(-rPh * 0.3f, rPh * 0.4f),
                        new PointF(-rPh * 0.9f, rPh * 0.7f),
                        new PointF(0, 0)
                    };
                    g.FillPolygon(CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.65f), Color.FromArgb(70, 180, 245)), aileG);
                    g.DrawPolygon(pGivreExt, aileG);
                    g.DrawPolygon(pGivreInt, aileG);
                    g.FillPolygon(CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.65f), Color.FromArgb(70, 180, 245)), aileD);
                    g.DrawPolygon(pGivreExt, aileD);
                    g.DrawPolygon(pGivreInt, aileD);
                    PointF[] corpsOiseau = {
                        new PointF(rPh * 0.75f, 0),
                        new PointF(rPh * 0.2f, -8f * cam.Zoom),
                        new PointF(-rPh * 0.5f, 0),
                        new PointF(rPh * 0.2f, 8f * cam.Zoom)
                    };
                    g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 200, 245, 255)), corpsOiseau);
                    g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2f * cam.Zoom), corpsOiseau);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.Cyan), rPh * 0.35f, -3f * cam.Zoom, 4f * cam.Zoom, 4f * cam.Zoom);
                    for (int s = 0; s < 4; s++)
                    {
                        float angS = s * MathF.PI / 2f + TempsEcoule * 4f;
                        float distS = (rPh * 0.5f + s * 14f) * cam.Zoom;
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 1.5f), -distS, MathF.Sin(angS) * 16f, -distS, MathF.Sin(angS) * 16f + 8f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 1.5f), -distS - 4f, MathF.Sin(angS) * 16f + 4f, -distS + 4f, MathF.Sin(angS) * 16f + 4f);
                    }
                    break;

                case TypeAnimationSort2D.TrombeTempete:
                    // Cyclone en entonnoir aspirant les ennemis avec débris et feuilles (Image 2)
                    float hTrombe = rayon * 1.6f;
                    int nbAnneaux = 7;
                    for (int i = 0; i < nbAnneaux; i++)
                    {
                        float t = (float)i / nbAnneaux;
                        float yAnneau = (t - 0.5f) * hTrombe;
                        float wAnneau = (0.25f + t * 0.95f) * rayon * 1.4f;
                        float hAnneau = wAnneau * 0.32f;
                        float decalageX = MathF.Sin(TempsEcoule * 8f + t * 4f) * (14f * cam.Zoom);
                        Pen pVent = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 210, 235, 245), (4f - t * 1.5f) * cam.Zoom);
                        Pen pVentExt = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(alpha * 0.45f), 150, 200, 220), (7f - t * 2f) * cam.Zoom);
                        g.DrawEllipse(pVentExt, -wAnneau / 2f + decalageX, yAnneau - hAnneau / 2f, wAnneau, hAnneau);
                        g.DrawEllipse(pVent, -wAnneau / 2f + decalageX, yAnneau - hAnneau / 2f, wAnneau, hAnneau);
                    }
                    for (int f = 0; f < 6; f++)
                    {
                        float aF = f * MathF.Tau / 6f + TempsEcoule * 11f;
                        float rF = (0.35f + (f % 3) * 0.28f) * rayon;
                        float yF = (MathF.Sin(f * 1.7f) * 0.4f) * hTrombe;
                        float xF = MathF.Cos(aF) * rF;
                        Color cDebris = (f % 2 == 0) ? Color.FromArgb(alpha, 46, 204, 113) : Color.FromArgb(alpha, 140, 150, 160);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(cDebris), xF - 3.5f * cam.Zoom, yF - 2.5f * cam.Zoom, 7f * cam.Zoom, 5f * cam.Zoom);
                    }
                    break;

                case TypeAnimationSort2D.DomeSacre:
                    // Dôme sacré protecteur hémisphérique d'or solaire (Image 2)
                    float rDome = rayon * 1.15f;
                    Brush bDomeFond = CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.28f), Color.FromArgb(255, 230, 120));
                    Pen pDomeExt = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 255, 215, 0), 4f * cam.Zoom);
                    Pen pDomeInt = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2f * cam.Zoom);
                    g.FillPie(bDomeFond, -rDome, -rDome, rDome * 2f, rDome * 2f, 180f, 180f);
                    g.DrawArc(pDomeExt, -rDome, -rDome, rDome * 2f, rDome * 2f, 180f, 180f);
                    g.DrawArc(pDomeInt, -rDome * 0.94f, -rDome * 0.94f, rDome * 1.88f, rDome * 1.88f, 180f, 180f);
                    g.DrawLine(pDomeExt, -rDome, 0, rDome, 0);
                    for (int i = -3; i <= 3; i++)
                    {
                        float xFlamme = (i * rDome / 3.8f);
                        float hFlamme = (8f + MathF.Sin(TempsEcoule * 12f + i) * 6f) * cam.Zoom;
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Gold), 2.5f * cam.Zoom), xFlamme, 0, xFlamme, -hFlamme);
                    }
                    for (int s = 0; s < 3; s++)
                    {
                        float angS = (s - 1) * 0.65f - MathF.PI / 2f;
                        float sx = MathF.Cos(angS) * (rDome * 0.65f);
                        float sy = MathF.Sin(angS) * (rDome * 0.65f);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), sx - 3f * cam.Zoom, sy - 3f * cam.Zoom, 6f * cam.Zoom, 6f * cam.Zoom);
                    }
                    break;

                case TypeAnimationSort2D.VortexAmes:
                    // Sceau runique violet avec âmes spectrales en orbite (Image 2)
                    float rVortex = rayon * 0.9f;
                    Pen pRune = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 165, 80, 255), 3f * cam.Zoom);
                    g.DrawEllipse(pRune, -rVortex, -rVortex, rVortex * 2f, rVortex * 2f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Color.White), 1.5f * cam.Zoom), -rVortex * 0.85f, -rVortex * 0.85f, rVortex * 1.7f, rVortex * 1.7f);
                    float rotVortex = TempsEcoule * 180f;
                    for (int i = 0; i < 4; i++)
                    {
                        float arcAng = rotVortex + i * 90f;
                        g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 190, 110, 255), 4f * cam.Zoom), -rVortex * 0.55f, -rVortex * 0.55f, rVortex * 1.1f, rVortex * 1.1f, arcAng, 60f);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        float angAme = -TempsEcoule * 4.5f + i * MathF.Tau / 4f;
                        float ax = MathF.Cos(angAme) * (rVortex * 1.18f);
                        float ay = MathF.Sin(angAme) * (rVortex * 1.18f);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 220, 240, 255)), ax - 6f * cam.Zoom, ay - 6f * cam.Zoom, 12f * cam.Zoom, 12f * cam.Zoom);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 40, 20, 80)), ax - 2f * cam.Zoom, ay - 2f * cam.Zoom, 4f * cam.Zoom, 4f * cam.Zoom);
                    }
                    break;

                case TypeAnimationSort2D.OrbeFoudre:
                    // Plasma électrique sphérique avec éclairs jaillissants (Image 2)
                    float rOrbe = rayon * 0.45f;
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.7f), Color.FromArgb(70, 190, 255)), -rOrbe * 1.4f, -rOrbe * 1.4f, rOrbe * 2.8f, rOrbe * 2.8f);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, Color.White)), -rOrbe, -rOrbe, rOrbe * 2f, rOrbe * 2f);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Cyan), 3f * cam.Zoom), -rOrbe, -rOrbe, rOrbe * 2f, rOrbe * 2f);
                    for (int i = 0; i < 6; i++)
                    {
                        float angF = i * MathF.Tau / 6f + TempsEcoule * 14f;
                        float dist1 = rOrbe * 1.5f;
                        float dist2 = rayon * 1.15f;
                        float midX = MathF.Cos(angF + 0.3f) * dist1;
                        float midY = MathF.Sin(angF + 0.3f) * dist1;
                        float endX = MathF.Cos(angF) * dist2;
                        float endY = MathF.Sin(angF) * dist2;
                        Pen pEclair = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, i % 2 == 0 ? Color.Cyan : Color.FromArgb(200, 140, 255)), 2f * cam.Zoom);
                        g.DrawLine(pEclair, 0, 0, midX, midY);
                        g.DrawLine(pEclair, midX, midY, endX, endY);
                    }
                    float orb1X = MathF.Cos(TempsEcoule * 6f) * (rayon * 0.82f);
                    float orb1Y = MathF.Sin(TempsEcoule * 6f) * (rayon * 0.82f);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 180, 80, 255)), orb1X - 5f * cam.Zoom, orb1Y - 5f * cam.Zoom, 10f * cam.Zoom, 10f * cam.Zoom);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), orb1X - 2f * cam.Zoom, orb1Y - 2f * cam.Zoom, 4f * cam.Zoom, 4f * cam.Zoom);
                    break;

                case TypeAnimationSort2D.CroissantLunaire:
                    // Lame lunaire en croissant avec halo prismatique arc-en-ciel et orbes solaires (Image 3)
                    float rLune = rayon * 1.1f;
                    Pen pLuneRose = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 255, 160, 200), 8f * cam.Zoom);
                    Pen pLuneBleu = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 140, 220, 255), 5f * cam.Zoom);
                    Pen pLuneOr = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 255, 230, 100), 3f * cam.Zoom);
                    Pen pLuneBlanc = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 1.5f * cam.Zoom);
                    g.DrawArc(pLuneRose, -rLune * 0.7f, -rLune, rLune * 1.4f, rLune * 2f, -80f, 160f);
                    g.DrawArc(pLuneBleu, -rLune * 0.65f, -rLune * 0.95f, rLune * 1.3f, rLune * 1.9f, -75f, 150f);
                    g.DrawArc(pLuneOr, -rLune * 0.6f, -rLune * 0.9f, rLune * 1.2f, rLune * 1.8f, -70f, 140f);
                    g.DrawArc(pLuneBlanc, -rLune * 0.55f, -rLune * 0.85f, rLune * 1.1f, rLune * 1.7f, -65f, 130f);
                    for (int s = 0; s < 3; s++)
                    {
                        float oX = (s * 24f - 18f) * cam.Zoom;
                        float oY = (s * 14f - 10f) * cam.Zoom;
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 255, 215, 0)), oX - 7f * cam.Zoom, oY - 7f * cam.Zoom, 14f * cam.Zoom, 14f * cam.Zoom);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), oX - 3f * cam.Zoom, oY - 3f * cam.Zoom, 6f * cam.Zoom, 6f * cam.Zoom);
                    }
                    break;

                case TypeAnimationSort2D.DragonTricephale:
                    // Dragon tricéphale déchaînant 3 jets de flammes (Image 3)
                    float rTri = rayon * 0.85f;
                    float[] anglesTetes = { -32f, 0f, 32f };
                    for (int h = 0; h < 3; h++)
                    {
                        float aRad = anglesTetes[h] * MathF.PI / 180f;
                        float hx = MathF.Cos(aRad) * rTri;
                        float hy = MathF.Sin(aRad) * rTri;
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 231, 76, 60), 8f * cam.Zoom), 0, 0, hx, hy);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Gold), 4f * cam.Zoom), 0, 0, hx, hy);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 230, 80, 20)), hx - 8f * cam.Zoom, hy - 8f * cam.Zoom, 16f * cam.Zoom, 16f * cam.Zoom);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), hx + 1f * cam.Zoom, hy - 3f * cam.Zoom, 4f * cam.Zoom, 4f * cam.Zoom);
                        float projX = hx + MathF.Cos(aRad) * (26f * cam.Zoom);
                        float projY = hy + MathF.Sin(aRad) * (26f * cam.Zoom);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 255, 120, 20)), projX - 6f * cam.Zoom, projY - 6f * cam.Zoom, 12f * cam.Zoom, 12f * cam.Zoom);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), projX - 2f * cam.Zoom, projY - 2f * cam.Zoom, 4f * cam.Zoom, 4f * cam.Zoom);
                    }
                    break;

                case TypeAnimationSort2D.LeviathanGlaces:
                    // Serpent des mers polaires ondulant avec crête de pointes de glace (Image 3)
                    float rLev = rayon * 1.1f;
                    int segments = 10;
                    PointF[] pointsLev = new PointF[segments];
                    for (int i = 0; i < segments; i++)
                    {
                        float progressionS = (float)i / segments;
                        float ondul = MathF.Sin(TempsEcoule * 12f + i * 0.8f) * (20f * cam.Zoom);
                        pointsLev[i] = new PointF(progressionS * rLev * 1.5f - rLev * 0.5f, ondul);
                    }
                    Pen pLevExt = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 85, 195, 255), 11f * cam.Zoom);
                    Pen pLevInt = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 4f * cam.Zoom);
                    g.DrawCurve(pLevExt, pointsLev);
                    g.DrawCurve(pLevInt, pointsLev);
                    for (int i = 1; i < segments - 1; i++)
                    {
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.Cyan), 2.5f * cam.Zoom), pointsLev[i].X, pointsLev[i].Y, pointsLev[i].X - 4f * cam.Zoom, pointsLev[i].Y - 14f * cam.Zoom);
                    }
                    break;

                case TypeAnimationSort2D.MiroirGlace:
                    // Miroir de glace runique sculpté (Image 3)
                    float rMirW = rayon * 0.75f;
                    float rMirH = rayon * 1.2f;
                    Pen pCadre = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 120, 200, 255), 4f * cam.Zoom);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.45f), Color.FromArgb(180, 240, 255)), -rMirW / 2f, -rMirH / 2f, rMirW, rMirH);
                    g.DrawEllipse(pCadre, -rMirW / 2f, -rMirH / 2f, rMirW, rMirH);
                    g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 180, 100, 255), 2f * cam.Zoom), -rMirW * 0.45f, -rMirH * 0.45f, rMirW * 0.9f, rMirH * 0.9f);
                    g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2f * cam.Zoom), -rMirW * 0.25f, -rMirH * 0.35f, rMirW * 0.25f, rMirH * 0.35f);
                    break;

                case TypeAnimationSort2D.SingulariteTellurique:
                    // Trou noir tellurique avec rochers orbitaux en tourbillon (Image 3)
                    float rSing = rayon * 0.85f;
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 12, 10, 24)), -rSing * 0.65f, -rSing * 0.65f, rSing * 1.3f, rSing * 1.3f);
                    float rotSing = TempsEcoule * 220f;
                    for (int a = 0; a < 6; a++)
                    {
                        float angA = rotSing + a * 60f;
                        g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 70, 90, 160), 5f * cam.Zoom), -rSing, -rSing * 0.65f, rSing * 2f, rSing * 1.3f, angA, 45f);
                    }
                    for (int r = 0; r < 4; r++)
                    {
                        float angRocher = -TempsEcoule * 4f + r * MathF.Tau / 4f;
                        float rx = MathF.Cos(angRocher) * (rSing * 0.95f);
                        float ry = MathF.Sin(angRocher) * (rSing * 0.65f);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 75, 70, 80)), rx - 7f * cam.Zoom, ry - 6f * cam.Zoom, 14f * cam.Zoom, 12f * cam.Zoom);
                        g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 120, 115, 130), 1.5f * cam.Zoom), rx - 7f * cam.Zoom, ry - 6f * cam.Zoom, 14f * cam.Zoom, 12f * cam.Zoom);
                    }
                    break;

                case TypeAnimationSort2D.PrismeIrise:
                    // Prisme géométrique réfractant 5 lasers arc-en-ciel (Image 3)
                    float rPrisme = rayon * 0.65f;
                    PointF[] polyPrisme = {
                        new PointF(0, -rPrisme * 1.2f),
                        new PointF(rPrisme * 0.8f, rPrisme * 0.6f),
                        new PointF(0, rPrisme * 1.1f),
                        new PointF(-rPrisme * 0.8f, rPrisme * 0.6f)
                    };
                    g.FillPolygon(CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.6f), Color.FromArgb(220, 245, 255)), polyPrisme);
                    g.DrawPolygon(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 2.5f * cam.Zoom), polyPrisme);
                    Color[] couleursSpectre = { Color.FromArgb(231, 76, 60), Color.FromArgb(241, 196, 15), Color.FromArgb(46, 204, 113), Color.FromArgb(52, 152, 219), Color.FromArgb(155, 89, 182) };
                    for (int l = 0; l < 5; l++)
                    {
                        float angLaser = (l - 2) * 0.38f;
                        float endLx = MathF.Cos(angLaser) * (rayon * 1.6f);
                        float endLy = MathF.Sin(angLaser) * (rayon * 1.6f);
                        Pen pLaser = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, couleursSpectre[l]), 4f * cam.Zoom);
                        g.DrawLine(pLaser, 0, 0, endLx, endLy);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), 1.5f * cam.Zoom), 0, 0, endLx, endLy);
                    }
                    break;

                case TypeAnimationSort2D.AbysseEldritch:
                    // Portail de l'abysse avec tentacules et yeux mystiques (Image 3)
                    float rAby = rayon * 0.9f;
                    Pen pRuneAby = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 120, 50, 180), 3f * cam.Zoom);
                    g.DrawEllipse(pRuneAby, -rAby, -rAby, rAby * 2f, rAby * 2f);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 18, 12, 32)), -rAby * 0.8f, -rAby * 0.8f, rAby * 1.6f, rAby * 1.6f);
                    for (int t = 0; t < 6; t++)
                    {
                        float angT = t * MathF.Tau / 6f;
                        float ondulT = MathF.Sin(TempsEcoule * 8f + t * 2f) * (12f * cam.Zoom);
                        float tx1 = MathF.Cos(angT) * (rAby * 0.6f);
                        float ty1 = MathF.Sin(angT) * (rAby * 0.6f);
                        float tx2 = MathF.Cos(angT + 0.3f) * (rAby * 1.35f) + ondulT;
                        float ty2 = MathF.Sin(angT + 0.3f) * (rAby * 1.35f) + ondulT;
                        Pen pTentacule = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 85, 30, 120), 4f * cam.Zoom);
                        g.DrawLine(pTentacule, tx1, ty1, tx2, ty2);
                    }
                    for (int e = 0; e < 3; e++)
                    {
                        float eyX = (e - 1) * (18f * cam.Zoom);
                        float eyY = (MathF.Sin(e * 1.5f) * 8f) * cam.Zoom;
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 220, 200, 255)), eyX - 6f * cam.Zoom, eyY - 3.5f * cam.Zoom, 12f * cam.Zoom, 7f * cam.Zoom);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 50, 0, 90)), eyX - 2f * cam.Zoom, eyY - 2.5f * cam.Zoom, 4f * cam.Zoom, 5f * cam.Zoom);
                    }
                    break;

                case TypeAnimationSort2D.PortailGlacial:
                    // Portail de glace gothique avec stalagmites de givre (Image 2)
                    float rPortW = rayon * 0.8f;
                    float rPortH = rayon * 1.25f;
                    Pen pGlace = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, 130, 225, 255), 4f * cam.Zoom);
                    g.DrawLine(pGlace, -rPortW / 2f, rPortH / 2f, -rPortW / 2f, -rPortH * 0.2f);
                    g.DrawLine(pGlace, rPortW / 2f, rPortH / 2f, rPortW / 2f, -rPortH * 0.2f);
                    g.DrawArc(pGlace, -rPortW / 2f, -rPortH * 0.6f, rPortW, rPortH * 0.8f, 180f, 180f);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.35f), Color.FromArgb(170, 235, 255)), -rPortW * 0.4f, -rPortH * 0.4f, rPortW * 0.8f, rPortH * 0.8f);
                    for (int st = -2; st <= 2; st++)
                    {
                        float stX = st * (rPortW * 0.25f);
                        float stH = (12f + (st % 2) * 8f) * cam.Zoom;
                        PointF[] picGlace = {
                            new PointF(stX - 4f * cam.Zoom, rPortH / 2f),
                            new PointF(stX, rPortH / 2f - stH),
                            new PointF(stX + 4f * cam.Zoom, rPortH / 2f)
                        };
                        g.FillPolygon(CacheRenduGDI.ObtenirBrush(Color.FromArgb(alpha, 200, 245, 255)), picGlace);
                    }
                    break;
            }

            g.Restore(etat);
        }
    }

    public class Particule
    {
        public Vector2 Position;
        public Vector2 Velocite;
        public Color Couleur;
        public float Taille;
        public float VieRestante;
        public float VieMax;
        public float Gravite;
        public float Frottement;
        public float Rotation;
        public float VitesseRotation;
        public TypeParticule Type = TypeParticule.Disque;

        public bool EstMorte => VieRestante <= 0f;

        public void MettreAJour(float dt)
        {
            Position += Velocite * dt;
            Velocite.Y += Gravite * dt;
            Velocite *= MathF.Max(0f, 1f - Frottement * dt);
            Rotation += VitesseRotation * dt;
            VieRestante -= dt;
        }

        public void Dessiner(Graphics g, Camera2D cam)
        {
            if (EstMorte) return;
            Point p = cam.MondeVersEcran(Position);

            // Culling de frustum écran (gain de performance majeur)
            if (p.X < -50 || p.X > cam.LargeurEcran + 50 || p.Y < -50 || p.Y > cam.HauteurEcran + 50)
                return;

            float ratio = Math.Clamp(VieRestante / VieMax, 0f, 1f);
            int alpha = (int)(ratio * Couleur.A);
            if (alpha <= 4) return;

            float r = Math.Max(1f, Taille * ratio * cam.Zoom);

            switch (Type)
            {
                case TypeParticule.OndeChoc:
                    // Onde circulaire de choc ultra-détaillée (plusieurs couches d'énergie)
                    float rayonOnde = Taille * (1.2f - ratio * 0.9f) * 2.5f * cam.Zoom;
                    if (rayonOnde > 1f)
                    {
                        // Couche externe floue
                        Pen penOndeGlow = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(alpha * 0.4f), Couleur), Math.Max(2f, 10f * ratio));
                        g.DrawEllipse(penOndeGlow, p.X - rayonOnde, p.Y - rayonOnde, rayonOnde * 2f, rayonOnde * 2f);
                        // Couche interne brillante
                        Pen penOndeCore = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), Math.Max(1f, 3f * ratio));
                        g.DrawEllipse(penOndeCore, p.X - rayonOnde, p.Y - rayonOnde, rayonOnde * 2f, rayonOnde * 2f);
                        
                        // Onde de remplissage très légère au centre
                        SolidBrush brushOndeFade = CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.15f), Couleur);
                        g.FillEllipse(brushOndeFade, p.X - rayonOnde * 0.8f, p.Y - rayonOnde * 0.8f, rayonOnde * 1.6f, rayonOnde * 1.6f);
                    }
                    break;

                case TypeParticule.LigneVitesse:
                    // Traînée lumineuse (laser / balle) avec noyau blanc
                    float dx = Velocite.X * 0.08f;
                    float dy = Velocite.Y * 0.08f;
                    Pen penLigneGlow = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(alpha * 0.6f), Couleur), Math.Max(2f, r * 1.5f));
                    g.DrawLine(penLigneGlow, p.X - dx, p.Y - dy, p.X + dx, p.Y + dy);
                    
                    Pen penLigneCore = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), Math.Max(1f, r * 0.5f));
                    g.DrawLine(penLigneCore, p.X - dx * 0.6f, p.Y - dy * 0.6f, p.X + dx * 0.6f, p.Y + dy * 0.6f);
                    break;

                case TypeParticule.Etoile:
                    // Étincelle à 4 pointes
                    SolidBrush brEtoile = CacheRenduGDI.ObtenirBrushAlpha(alpha, Couleur);
                    g.FillEllipse(brEtoile, p.X - r * 1.6f, p.Y - r * 0.35f, r * 3.2f, r * 0.7f);
                    g.FillEllipse(brEtoile, p.X - r * 0.35f, p.Y - r * 1.6f, r * 0.7f, r * 3.2f);
                    break;

                case TypeParticule.AuraMote:
                    // Mote vaporeux doux avec double couche
                    SolidBrush brHalo = CacheRenduGDI.ObtenirBrushAlpha((int)(alpha * 0.4f), Couleur);
                    g.FillEllipse(brHalo, p.X - r * 1.6f, p.Y - r * 1.6f, r * 3.2f, r * 3.2f);
                    SolidBrush brCoeur = CacheRenduGDI.ObtenirBrushAlpha(alpha, Color.White);
                    g.FillEllipse(brCoeur, p.X - r * 0.5f, p.Y - r * 0.5f, r, r);
                    break;

                case TypeParticule.Flamme:
                case TypeParticule.Cristal:
                case TypeParticule.Rune:
                case TypeParticule.Petale:
                case TypeParticule.Brume:
                case TypeParticule.Goutte:
                    DessinerFormeAnimee(g, p, r, alpha, cam.Zoom);
                    break;

                default: // Disque classique ultra rapide
                    SolidBrush b = CacheRenduGDI.ObtenirBrushAlpha(alpha, Couleur);
                    g.FillEllipse(b, p.X - r, p.Y - r, r * 2f, r * 2f);
                    break;
            }
        }

        private void DessinerFormeAnimee(Graphics g, Point p, float rayon, int alpha, float zoom)
        {
            float ratioVie = Math.Clamp(VieRestante / VieMax, 0f, 1f);
            float taille = Math.Max(1f, Taille * (0.45f + 0.55f * ratioVie) * zoom);
            GraphicsState etat = g.Save();
            g.TranslateTransform(p.X, p.Y);
            g.RotateTransform(Rotation * 180f / MathF.PI);

            switch (Type)
            {
                case TypeParticule.Flamme:
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha / 2, Couleur), -taille * 0.62f, -taille * 1.15f, taille * 1.24f, taille * 2.3f);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha, Color.FromArgb(255, 255, 220, 145)), -taille * 0.24f, -taille * 0.65f, taille * 0.48f, taille * 1.25f);
                    break;

                case TypeParticule.Cristal:
                    Pen penCristal = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Couleur), Math.Max(1f, taille * 0.18f));
                    Pen penCoeurCristal = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Color.White), Math.Max(1f, taille * 0.1f));
                    g.DrawLine(penCristal, 0, -taille * 1.4f, taille * 0.8f, 0);
                    g.DrawLine(penCristal, taille * 0.8f, 0, 0, taille * 1.4f);
                    g.DrawLine(penCristal, 0, taille * 1.4f, -taille * 0.8f, 0);
                    g.DrawLine(penCristal, -taille * 0.8f, 0, 0, -taille * 1.4f);
                    g.DrawLine(penCoeurCristal, 0, -taille * 0.7f, 0, taille * 0.7f);
                    break;

                case TypeParticule.Rune:
                    Pen penRune = CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha, Couleur), Math.Max(1f, taille * 0.16f));
                    g.DrawRectangle(penRune, -taille, -taille, taille * 2f, taille * 2f);
                    g.DrawLine(penRune, -taille * 0.65f, 0, taille * 0.65f, 0);
                    g.DrawLine(penRune, 0, -taille * 0.65f, 0, taille * 0.65f);
                    break;

                case TypeParticule.Petale:
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha, Couleur), -taille * 0.48f, -taille, taille * 0.96f, taille * 2f);
                    g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alpha / 2, Color.White), Math.Max(1f, taille * 0.08f)), 0, -taille * 0.65f, 0, taille * 0.65f);
                    break;

                case TypeParticule.Brume:
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha / 3, Couleur), -taille * 1.3f, -taille * 0.7f, taille * 2.6f, taille * 1.4f);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha / 2, Couleur), -taille * 0.85f, -taille * 0.95f, taille * 1.7f, taille * 1.9f);
                    break;

                case TypeParticule.Goutte:
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha, Couleur), -taille * 0.42f, -taille * 0.95f, taille * 0.84f, taille * 1.9f);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alpha, Color.White), -taille * 0.18f, -taille * 0.55f, taille * 0.2f, taille * 0.35f);
                    break;
            }

            g.Restore(etat);
        }
    }

    public class GestionnaireParticules
    {
        private const int NombreMaxParticules = 1200;
        private const int NombreMaxEffetsSort = 32;
        private readonly List<Particule> particules = new List<Particule>(NombreMaxParticules);
        private readonly List<EffetSortAnime> effetsSorts = new List<EffetSortAnime>(NombreMaxEffetsSort);
        private readonly Random rng = Random.Shared;

        public int NombreParticules => particules.Count;

        private void Ajouter(Particule particule)
        {
            if (particules.Count < NombreMaxParticules)
                particules.Add(particule);
        }

        public void EmettreAnimationSort(Vector2 position, Vector2 direction, Color couleur, TypeAnimationSort2D type, float duree = 0.72f)
        {
            if (effetsSorts.Count >= NombreMaxEffetsSort) return;
            effetsSorts.Add(new EffetSortAnime
            {
                Position = position,
                Direction = direction,
                Couleur = couleur,
                Type = type,
                Duree = duree,
                TempsRestant = duree
            });
        }

        public void EmettreEclats(Vector2 pos, Color couleur, int quantite, float vitesse = 180f, float taille = 4f, float duree = 0.45f)
        {
            for (int i = 0; i < quantite && particules.Count < NombreMaxParticules; i++)
            {
                float angle = rng.NextSingle() * MathF.PI * 2f;
                float spd = (rng.NextSingle() * 0.7f + 0.3f) * vitesse;
                Ajouter(new Particule
                {
                    Position = pos,
                    Velocite = new Vector2(MathF.Cos(angle) * spd, MathF.Sin(angle) * spd),
                    Couleur = couleur,
                    Taille = taille * (rng.NextSingle() * 0.6f + 0.7f),
                    VieMax = duree,
                    VieRestante = duree,
                    Gravite = 35f,
                    Frottement = 3.2f,
                    Rotation = angle,
                    VitesseRotation = (rng.NextSingle() - 0.5f) * 10f,
                    Type = TypeParticule.Etoile
                });
            }
        }

        public void EmettreAnneauExplosion(Vector2 pos, Color couleur, int quantite = 24, float vitesse = 220f)
        {
            for (int i = 0; i < quantite && particules.Count < NombreMaxParticules; i++)
            {
                float angle = (i / (float)quantite) * MathF.PI * 2f;
                Ajouter(new Particule
                {
                    Position = pos,
                    Velocite = new Vector2(MathF.Cos(angle) * vitesse, MathF.Sin(angle) * vitesse),
                    Couleur = couleur,
                    Taille = 5.5f,
                    VieMax = 0.55f,
                    VieRestante = 0.55f,
                    Gravite = 0f,
                    Frottement = 2.0f,
                    Type = TypeParticule.LigneVitesse
                });
            }
            // Ajouter une onde de choc au centre
            EmettreOndeDeChoc(pos, couleur, vitesse * 0.45f, 0.4f);
        }

        public void EmettreOndeDeChoc(Vector2 pos, Color couleur, float rayonMax = 80f, float duree = 0.35f)
        {
            Ajouter(new Particule
            {
                Position = pos,
                Velocite = Vector2.Zero,
                Couleur = couleur,
                Taille = rayonMax,
                VieMax = duree,
                VieRestante = duree,
                Gravite = 0f,
                Frottement = 0f,
                Type = TypeParticule.OndeChoc
            });
        }

        public void EmettrePoussiereCourse(Vector2 pos)
        {
            Ajouter(new Particule
            {
                Position = pos + new Vector2(rng.NextSingle() * 10f - 5f, rng.NextSingle() * 4f - 2f),
                Velocite = new Vector2(rng.NextSingle() * 20f - 10f, -15f - rng.NextSingle() * 15f),
                Couleur = Color.FromArgb(120, 160, 150, 140),
                Taille = 3.5f + rng.NextSingle() * 2f,
                VieMax = 0.3f,
                VieRestante = 0.3f,
                Gravite = -8f,
                Frottement = 2.5f,
                Type = TypeParticule.Disque
            });
        }

        public void EmettreTrailProjectile(Vector2 pos, Color couleur, float rayon = 4f)
        {
            Ajouter(new Particule
            {
                Position = pos + new Vector2(rng.NextSingle() * 4f - 2f, rng.NextSingle() * 4f - 2f),
                Velocite = new Vector2(rng.NextSingle() * 16f - 8f, rng.NextSingle() * 16f - 8f),
                Couleur = couleur,
                Taille = rayon,
                VieMax = 0.22f,
                VieRestante = 0.22f,
                Gravite = 0f,
                Frottement = 2.0f,
                Type = TypeParticule.AuraMote
            });
        }

        public void EmettreAuraClasse(Vector2 pos, ClasseType classe)
        {
            Color c = classe switch
            {
                ClasseType.Guerrier => Color.OrangeRed,
                ClasseType.Mage => Color.Cyan,
                ClasseType.Rodeur => Color.LimeGreen,
                ClasseType.Paladin => Color.Gold,
                ClasseType.Necromancien => Color.DarkOrchid,
                _ => Color.White
            };
            TypeParticule forme = classe switch
            {
                ClasseType.Guerrier => TypeParticule.Flamme,
                ClasseType.Mage => TypeParticule.Rune,
                ClasseType.Rodeur => TypeParticule.Petale,
                ClasseType.Paladin => TypeParticule.Etoile,
                ClasseType.Necromancien => TypeParticule.Brume,
                _ => TypeParticule.AuraMote
            };
            Ajouter(new Particule
            {
                Position = pos + new Vector2(rng.NextSingle() * 26f - 13f, rng.NextSingle() * 10f),
                Velocite = new Vector2(rng.NextSingle() * 14f - 7f, -35f - rng.NextSingle() * 20f),
                Couleur = c,
                Taille = 3f + rng.NextSingle() * 2f,
                VieMax = 0.45f,
                VieRestante = 0.45f,
                Gravite = -15f,
                Frottement = 1.5f,
                Rotation = rng.NextSingle() * MathF.Tau,
                VitesseRotation = (rng.NextSingle() - 0.5f) * 4f,
                Type = forme
            });
        }

        public void EmettreMeteoZone(Camera2D cam, ZoneType2D zone)
        {
            // Fait flotter 2 ou 3 particules d'ambiance sur l'écran
            Point posEcran = new Point(rng.Next(cam.LargeurEcran), rng.Next(-10, cam.HauteurEcran / 3));
            Vector2 posMonde = cam.EcranVersMonde(posEcran);

            switch (zone)
            {
                case ZoneType2D.Village:
                    // Pétales de fleurs de cerisier / feuilles dorées
                    Ajouter(new Particule
                    {
                        Position = posMonde,
                        Velocite = new Vector2(40f + rng.NextSingle() * 30f, 25f + rng.NextSingle() * 20f),
                        Couleur = rng.Next(2) == 0 ? Color.FromArgb(180, 255, 182, 193) : Color.FromArgb(160, 241, 196, 15),
                        Taille = 3.5f,
                        VieMax = 4.0f,
                        VieRestante = 4.0f,
                        Gravite = 2f,
                        Frottement = 0.2f,
                        Rotation = rng.NextSingle() * MathF.Tau,
                        VitesseRotation = (rng.NextSingle() - 0.5f) * 1.8f,
                        Type = TypeParticule.Petale
                    });
                    break;

                case ZoneType2D.Donjon:
                    // Brume spectrale et étincelles de braises
                    Ajouter(new Particule
                    {
                        Position = posMonde,
                        Velocite = new Vector2(rng.NextSingle() * 20f - 10f, 15f + rng.NextSingle() * 25f),
                        Couleur = Color.FromArgb(140, 230, 126, 34),
                        Taille = 3f,
                        VieMax = 3.0f,
                        VieRestante = 3.0f,
                        Gravite = -10f,
                        Frottement = 0.5f,
                        Rotation = rng.NextSingle() * MathF.Tau,
                        VitesseRotation = (rng.NextSingle() - 0.5f) * 4f,
                        Type = TypeParticule.Flamme
                    });
                    break;

                case ZoneType2D.TourAstrale:
                    // Poussière d'étoiles scintillantes
                    Ajouter(new Particule
                    {
                        Position = posMonde,
                        Velocite = new Vector2(rng.NextSingle() * 15f - 7f, 10f + rng.NextSingle() * 15f),
                        Couleur = Color.FromArgb(200, 140, 220, 255),
                        Taille = 4f,
                        VieMax = 3.5f,
                        VieRestante = 3.5f,
                        Gravite = 1f,
                        Frottement = 0.3f,
                        Rotation = rng.NextSingle() * MathF.Tau,
                        VitesseRotation = (rng.NextSingle() - 0.5f) * 2.5f,
                        Type = TypeParticule.Cristal
                    });
                    break;
            }
        }

        public void EmettrePuffFumee(Vector2 posCheminee)
        {
            Ajouter(new Particule
            {
                Position = posCheminee + new Vector2(rng.NextSingle() * 8f - 4f, 0f),
                Velocite = new Vector2(12f + rng.NextSingle() * 14f, -32f - rng.NextSingle() * 18f),
                Couleur = Color.FromArgb(140, 190, 195, 205),
                Taille = 4f + rng.NextSingle() * 3f,
                VieMax = 1.3f,
                VieRestante = 1.3f,
                Gravite = -2f,
                Frottement = 0.5f,
                Type = TypeParticule.Brume
            });
        }

        public void EmettreGoutteletteFontaine(Vector2 posFontaine)
        {
            float angle = rng.NextSingle() * MathF.PI * 2f;
            float dist = rng.NextSingle() * 16f;
            Ajouter(new Particule
            {
                Position = posFontaine + new Vector2(MathF.Cos(angle) * dist, MathF.Sin(angle) * dist),
                Velocite = new Vector2(MathF.Cos(angle) * 18f, -45f - rng.NextSingle() * 25f),
                Couleur = Color.FromArgb(160, 180, 240, 255),
                Taille = 2.4f + rng.NextSingle() * 1.6f,
                VieMax = 0.55f,
                VieRestante = 0.55f,
                Gravite = 130f,
                Frottement = 0.2f,
                Type = TypeParticule.Goutte
            });
        }

        public void MettreAJour(float dt)
        {
            for (int i = effetsSorts.Count - 1; i >= 0; i--)
            {
                effetsSorts[i].MettreAJour(dt);
                if (effetsSorts[i].EstTermine)
                    effetsSorts.RemoveAt(i);
            }

            for (int i = particules.Count - 1; i >= 0; i--)
            {
                particules[i].MettreAJour(dt);
                if (particules[i].EstMorte)
                {
                    particules.RemoveAt(i);
                }
            }
        }

        public void Dessiner(Graphics g, Camera2D cam)
        {
            for (int i = 0; i < particules.Count; i++)
            {
                particules[i].Dessiner(g, cam);
            }

            for (int i = 0; i < effetsSorts.Count; i++)
            {
                effetsSorts[i].Dessiner(g, cam);
            }
        }
    }

    // ==============================================================
    // TEXTES COMBAT FLOTTANTS (DEGATS, CRITS, SOINS)
    // ==============================================================
    public class TexteFlottant
    {
        public Vector2 Position;
        public Vector2 Velocite;
        public string Texte = "";
        public Color Couleur;
        public float VieRestante;
        public float VieMax;
        public bool EstCritique;

        public bool EstMort => VieRestante <= 0f;

        public void MettreAJour(float dt)
        {
            Position += Velocite * dt;
            Velocite.Y -= 18f * dt; // Flotte doucement vers le haut
            Velocite.X *= MathF.Max(0f, 1f - 2f * dt);
            VieRestante -= dt;
        }

        public void Dessiner(Graphics g, Camera2D cam, Font fontNormale, Font fontCritique)
        {
            if (EstMort) return;
            Point p = cam.MondeVersEcran(Position);

            // Culling de vue
            if (p.X < -100 || p.X > cam.LargeurEcran + 100 || p.Y < -50 || p.Y > cam.HauteurEcran + 50) return;

            float ratio = Math.Clamp(VieRestante / VieMax, 0f, 1f);
            int alpha = (int)(ratio * 255);
            if (alpha <= 5) return;

            Font font = EstCritique ? fontCritique : fontNormale;
            SolidBrush bOmbre = CacheRenduGDI.ObtenirBrushAlpha(Math.Min(220, alpha), Color.Black);
            SolidBrush bTexte = CacheRenduGDI.ObtenirBrushAlpha(alpha, Couleur);

            // Double contour pour lisibilité absolue sur n'importe quel fond
            g.DrawString(Texte, font, bOmbre, p.X - 1, p.Y);
            g.DrawString(Texte, font, bOmbre, p.X + 1, p.Y);
            g.DrawString(Texte, font, bOmbre, p.X, p.Y - 1);
            g.DrawString(Texte, font, bOmbre, p.X, p.Y + 1);

            g.DrawString(Texte, font, bTexte, p.X, p.Y);
        }
    }

    // ==============================================================
    // PROJECTILES (FLECHES, BOULES DE FEU, ATTAQUES DE BOSS)
    // ==============================================================
    public class Projectile2D
    {
        public Vector2 Position;
        public Vector2 Velocite;
        public float Rayon = 6f;
        public int Degats;
        public bool EstDuJoueur;
        public bool EstCritique;
        public Color Couleur;
        public float TempsVie = 2.5f;
        public bool Transpercant = false;
        public bool EstMort = false;
        public string NomSort = "";
        public float ChronoTrail = 0f;
        public float TempsEcoule = 0f;

        public void MettreAJour(float dt, GestionnaireParticules? particules = null)
        {
            Position += Velocite * dt;
            TempsEcoule += dt;
            TempsVie -= dt;
            if (TempsVie <= 0f) EstMort = true;

            // Traînée de particules lumineuses
            if (particules != null)
            {
                ChronoTrail -= dt;
                if (ChronoTrail <= 0f)
                {
                    ChronoTrail = 0.035f;
                    particules.EmettreTrailProjectile(Position, Couleur, Rayon * 0.55f);
                }
            }
        }

        public void Dessiner(Graphics g, Camera2D cam)
        {
            if (!cam.EstVisible(Position, Rayon * 4f)) return;
            Point p = cam.MondeVersEcran(Position);
            float r = Rayon * cam.Zoom;
            float angleVitesse = MathF.Atan2(Velocite.Y, Velocite.X);
            GraphicsState etat = g.Save();
            g.TranslateTransform(p.X, p.Y);
            g.RotateTransform(angleVitesse * 180f / MathF.PI);

            if (NomSort.Contains("Flèche", StringComparison.OrdinalIgnoreCase) ||
                NomSort.Contains("Volée", StringComparison.OrdinalIgnoreCase) ||
                NomSort.Contains("Tir d'Élite", StringComparison.OrdinalIgnoreCase))
            {
                DessinerProjectileFleche(g, r);
            }
            else if (NomSort.Contains("Foudre", StringComparison.OrdinalIgnoreCase))
            {
                DessinerProjectileFoudre(g, r);
            }
            else if (NomSort.Contains("Âme", StringComparison.OrdinalIgnoreCase))
            {
                DessinerProjectileAme(g, r);
            }
            else
            {
                DessinerProjectileArcanique(g, r);
            }

            g.Restore(etat);
        }

        private void DessinerProjectileFleche(Graphics g, float rayon)
        {
            float longueur = rayon * 4.2f;
            float plume = MathF.Sin(TempsEcoule * 32f) * rayon * 0.12f;
            bool explosive = NomSort.Contains("Explosive", StringComparison.OrdinalIgnoreCase);
            if (explosive)
            {
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(115, Color.OrangeRed), rayon * 3.2f), -longueur * 1.8f, 0, -rayon * 0.35f, 0);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(210, Color.Orange), rayon * 1.25f), -longueur * 1.5f, 0, -rayon * 0.25f, 0);
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(255, 255, 215, 130)), -longueur * 1.65f, -rayon * 0.3f, rayon * 0.55f, rayon * 0.6f);
            }
            else
            {
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(65, Couleur), rayon * 1.35f), -longueur * 1.45f, 0, -rayon * 0.25f, 0);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(185, Couleur), Math.Max(1.5f, rayon * 0.55f)), -longueur * 1.3f, 0, -rayon * 0.15f, 0);
            }

            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(235, 143, 103, 67), Math.Max(2f, rayon * 0.55f)), -longueur, 0, longueur * 0.82f, 0);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(255, 238, 220, 180), Math.Max(1f, rayon * 0.22f)), -longueur * 0.86f, -rayon * 0.12f, longueur * 0.76f, -rayon * 0.12f);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(190, 233, 239, 219), Math.Max(1f, rayon * 0.42f)), -longueur, 0, -longueur * 0.52f, -rayon * (0.85f + plume));
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(190, 233, 239, 219), Math.Max(1f, rayon * 0.42f)), -longueur, 0, -longueur * 0.52f, rayon * (0.85f + plume));
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(235, Couleur), Math.Max(2f, rayon * 0.72f)), longueur * 0.45f, 0, longueur, 0);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(255, 248, 246, 229), Math.Max(2f, rayon * 0.25f)), longueur * 0.55f, 0, longueur * 0.96f, 0);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.White, Math.Max(1.5f, rayon * 0.48f)), longueur, 0, longueur * 0.6f, -rayon * 0.58f);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.White, Math.Max(1.5f, rayon * 0.48f)), longueur, 0, longueur * 0.6f, rayon * 0.58f);
        }

        private void DessinerProjectileFoudre(Graphics g, float rayon)
        {
            float flicker = 0.72f + MathF.Abs(MathF.Sin(TempsEcoule * 41f)) * 0.28f;
            float wobble = MathF.Sin(TempsEcoule * 29f) * rayon * 0.3f;
            float avance = MathF.Sin(TempsEcoule * 17f) * rayon * 0.18f;
            Pen halo = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(85 * flicker), Couleur), rayon * 2.4f);
            Pen arc = CacheRenduGDI.ObtenirPen(Color.FromArgb(245, Color.White), Math.Max(2f, rayon * 0.52f));
            Pen branche = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(205 * flicker), Color.FromArgb(135, 238, 255)), Math.Max(1f, rayon * 0.3f));
            g.DrawLine(halo, -rayon * 3.5f, 0, rayon * 2.9f, 0);
            g.DrawLine(arc, -rayon * 3.5f, 0, -rayon * 2.2f, -rayon * 0.78f + wobble);
            g.DrawLine(arc, -rayon * 2.2f, -rayon * 0.78f + wobble, -rayon * 1.05f + avance, rayon * 0.62f);
            g.DrawLine(arc, -rayon * 1.05f + avance, rayon * 0.62f, rayon * 0.15f, -rayon * 0.55f + wobble);
            g.DrawLine(arc, rayon * 0.15f, -rayon * 0.55f + wobble, rayon * 1.25f, rayon * 0.38f);
            g.DrawLine(arc, rayon * 1.25f, rayon * 0.38f, rayon * 2.9f, 0);
            g.DrawLine(branche, -rayon * 1.05f + avance, rayon * 0.62f, -rayon * 0.72f + avance, rayon * 1.55f);
            g.DrawLine(branche, rayon * 0.15f, -rayon * 0.55f + wobble, rayon * 0.42f, -rayon * 1.45f);
            g.DrawLine(branche, rayon * 1.25f, rayon * 0.38f, rayon * 1.72f, rayon * 1.08f);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), rayon * 2.1f, -rayon * 0.28f, rayon * 0.56f, rayon * 0.56f);
        }

        private void DessinerProjectileAme(Graphics g, float rayon)
        {
            float pulsation = 0.88f + MathF.Sin(TempsEcoule * 15f) * 0.1f;
            float r = rayon * pulsation;
            float rotation = TempsEcoule * 75f;
            g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(50, Couleur), -r * 2.25f, -r * 2.25f, r * 4.5f, r * 4.5f);
            GraphicsState orbite = g.Save();
            g.RotateTransform(rotation);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(185, Couleur), Math.Max(1f, r * 0.16f)), -r * 1.7f, -r * 0.62f, r * 3.4f, r * 1.24f);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(135, Color.White), Math.Max(1f, r * 0.1f)), -r * 1.25f, -r * 1.1f, r * 2.5f, r * 2.2f);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), r * 1.35f, -r * 0.14f, r * 0.28f, r * 0.28f);
            g.Restore(orbite);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Couleur), -r, -r, r * 2f, r * 2f);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(230, 220, 255)), -r * 0.42f, -r * 0.52f, r * 0.84f, r * 1.04f);
            g.DrawArc(CacheRenduGDI.ObtenirPen(Color.FromArgb(210, Color.White), Math.Max(1f, r * 0.16f)), -r * 0.8f, -r * 0.9f, r * 1.6f, r * 1.8f, 205f, 135f);
        }

        private void DessinerProjectileArcanique(Graphics g, float rayon)
        {
            float pulsation = 0.84f + MathF.Sin(TempsEcoule * 18f) * 0.13f;
            float r = rayon * pulsation;
            float rotation = TempsEcoule * 92f;
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(55, Couleur), r * 2.3f), -r * 3f, 0, -r * 0.7f, 0);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(115, Color.White), Math.Max(1f, r * 0.34f)), -r * 2.7f, -r * 0.12f, -r * 0.95f, -r * 0.12f);
            g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(50, Couleur), -r * 1.8f, -r * 1.8f, r * 3.6f, r * 3.6f);
            GraphicsState sceau = g.Save();
            g.RotateTransform(rotation);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(200, Couleur), Math.Max(1f, r * 0.16f)), -r * 1.35f, -r * 0.65f, r * 2.7f, r * 1.3f);
            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(135, Color.White), Math.Max(1f, r * 0.12f)), -r * 0.65f, -r * 1.35f, r * 1.3f, r * 2.7f);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(180, Color.White), Math.Max(1f, r * 0.12f)), -r * 1.05f, -r * 1.05f, r * 1.05f, r * 1.05f);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(180, Color.White), Math.Max(1f, r * 0.12f)), -r * 1.05f, r * 1.05f, r * 1.05f, -r * 1.05f);
            g.Restore(sceau);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Couleur), -r, -r, r * 2f, r * 2f);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), -r * 0.3f, -r * 0.3f, r * 0.6f, r * 0.6f);
        }
    }

    // ==============================================================
    // BUTINS AU SOL (LOOTS, OR, PIERRES, EQUIPEMENT)
    // ==============================================================
    public enum TypeLootAuSol { Or, PierreForge, Materiau, Potion, Equipement }

    public class LootAuSol2D
    {
        private static readonly Random RngAnimation = new();

        public Vector2 Position;
        public TypeLootAuSol Type;
        public int Quantite;
        public string NomItem = "";
        public Rarete RareteItem = Rarete.Commun;
        public Equipement? ObjetEquipement;
        public float AnimationBob;
        public bool EstRecupere = false;

        public LootAuSol2D(Vector2 pos, TypeLootAuSol type, int qte, string nom = "", Rarete rarete = Rarete.Commun, Equipement? eq = null)
        {
            Position = pos;
            Type = type;
            Quantite = qte;
            NomItem = nom;
            RareteItem = rarete;
            ObjetEquipement = eq;
            AnimationBob = RngAnimation.NextSingle() * MathF.Tau;
        }

        public Color ObtenirCouleurLoot()
        {
            return Type switch
            {
                TypeLootAuSol.Or => Color.FromArgb(241, 196, 15),
                TypeLootAuSol.PierreForge => Color.FromArgb(230, 126, 34),
                TypeLootAuSol.Potion => Color.FromArgb(46, 204, 113),
                TypeLootAuSol.Materiau => Color.FromArgb(52, 152, 219),
                TypeLootAuSol.Equipement => RareteItem switch
                {
                    Rarete.Mythique => Color.FromArgb(231, 76, 60),
                    Rarete.Legendaire => Color.FromArgb(241, 196, 15),
                    Rarete.Epique => Color.FromArgb(155, 89, 182),
                    Rarete.Rare => Color.FromArgb(52, 152, 219),
                    _ => Color.FromArgb(189, 195, 199)
                },
                _ => Color.White
            };
        }

        public void MettreAJour(float dt, Vector2 posJoueur)
        {
            AnimationBob += dt * 4f;

            // Aimant à butin (magnétisme vers le joueur)
            float distCarree = Vector2.DistanceCarree(Position, posJoueur);
            if (distCarree < 110f * 110f)
            {
                Vector2 dir = (posJoueur - Position).Normaliser();
                float vitesseAimant = 280f;
                Position += dir * vitesseAimant * dt;
            }
        }

        public void Dessiner(Graphics g, Camera2D cam)
        {
            if (!cam.EstVisible(Position, 70f)) return;

            float decalageY = MathF.Sin(AnimationBob) * 5f;
            Point p = cam.MondeVersEcran(new Vector2(Position.X, Position.Y + decalageY));
            Point pOmbre = cam.MondeVersEcran(Position);
            Color col = ObtenirCouleurLoot();

            // Ombre au sol dynamique
            SolidBrush bOmbre = CacheRenduGDI.ObtenirBrushAlpha(80, Color.Black);
            g.FillEllipse(bOmbre, pOmbre.X - 10, pOmbre.Y + 2, 20, 7);

            if (Type == TypeLootAuSol.Equipement)
            {
                float pulsationRarete = 0.8f + 0.2f * MathF.Sin(AnimationBob * 1.7f);
                int alphaRarete = 35 + (int)(30f * pulsationRarete);
                float rayonHalo = 15f + pulsationRarete * 4f;
                g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(alphaRarete, col), p.X - rayonHalo, p.Y - rayonHalo, rayonHalo * 2f, rayonHalo * 2f);

                if (RareteItem >= Rarete.Epique)
                {
                    int largeurRayon = 8 + (int)(4f * pulsationRarete);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(35, col), p.X - largeurRayon, p.Y - 72, largeurRayon * 2, 72);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(125, col), p.X - 2, p.Y - 72, 4, 72);
                }

                if (RareteItem >= Rarete.Legendaire)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = AnimationBob * 0.45f + i * MathF.PI / 2f;
                        float inner = rayonHalo * 0.75f;
                        float outer = rayonHalo * (1.25f + pulsationRarete * 0.2f);
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(190, Color.White), 1.5f),
                            p.X + MathF.Cos(angle) * inner, p.Y + MathF.Sin(angle) * inner,
                            p.X + MathF.Cos(angle) * outer, p.Y + MathF.Sin(angle) * outer);
                    }
                }
            }

            // Orbe ou icône
            SolidBrush halo = CacheRenduGDI.ObtenirBrushAlpha(65, col);
            g.FillEllipse(halo, p.X - 12, p.Y - 12, 24, 24);

            SolidBrush coeur = CacheRenduGDI.ObtenirBrush(col);
            g.FillEllipse(coeur, p.X - 7, p.Y - 7, 14, 14);

            SolidBrush centreBlanc = CacheRenduGDI.ObtenirBrush(Color.White);
            g.FillEllipse(centreBlanc, p.X - 3, p.Y - 4, 6, 6);

            // Écriture du nom avec fond sombre pour lisibilité
            using (Font f = new Font("Segoe UI", 7.8f, FontStyle.Bold))
            {
                string txt = Type switch
                {
                    TypeLootAuSol.Or => $"+{Quantite} 🪙",
                    TypeLootAuSol.PierreForge => $"💎 {Quantite} Pierre(s)",
                    TypeLootAuSol.Potion => $"🧪 Potion",
                    TypeLootAuSol.Materiau => $"📦 {NomItem} x{Quantite}",
                    TypeLootAuSol.Equipement => ObjetEquipement != null ? $"⚔️ {ObjetEquipement.ObtenirNomComplet()}" : "Équipement",
                    _ => NomItem
                };
                SizeF sz = g.MeasureString(txt, f);
                SolidBrush bFondTexte = CacheRenduGDI.ObtenirBrushAlpha(160, Color.FromArgb(15, 15, 20));
                g.FillRectangle(bFondTexte, p.X - sz.Width / 2f - 3, p.Y + 9, sz.Width + 6, sz.Height + 1);
                g.DrawString(txt, f, coeur, p.X - sz.Width / 2f, p.Y + 9);
            }
        }
    }

    // ==============================================================
    // JOUEUR 2D (DÉPLACEMENTS WASD/ZQSD, DASH, ATTAQUES, JAUGE)
    // ==============================================================
    public class Joueur2D
    {
        private float tempsPoussierePas;

        public Joueur ModeleHero { get; }
        public Vector2 Position;
        public Vector2 Velocite;
        public float AngleVise = 0f;
        public float Rayon = 18f;

        // Dash / Esquive
        public float DashCooldown = 0f;
        public float DashCooldownMax = 1.1f;
        public float DashTempsRestant = 0f;
        public Vector2 DashDirection;
        public bool EstEnDash => DashTempsRestant > 0f;
        public List<(Vector2 Pos, float Angle, float Alpha)> FantomesDash = new List<(Vector2, float, float)>();

        // Attaque & Coups
        public float CooldownAttaque = 0f;
        public float AnimationAttaque = 0f;
        public float AnimationDuree = 0.22f;
        public int ComboIndex = 0;
        public float ComboTempsReset = 0f;

        // Compétences 1 et 2
        public float CooldownCompetence1 = 0f;
        public float CooldownCompetence2 = 0f;
        public float CooldownAttaqueSecondaire => CooldownCompetence1;

        // Animations & Effets Visuels
        public float AnimationMarche = 0f;
        public float TempsAnimation = 0f;
        public float TempsFlashDegats = 0f;
        public float ChronoAura = 0f;
        public float ChronoGlyphe = 0f;
        public Color CouleurDernierSort = Color.Cyan;

        // Ultime
        public float JaugeUltime { get; set; } = 0f; // 0 à 100%
        public float TempsInvulnerabilite { get; private set; }

        // Statuts 2D (Effets d'état persistants)
        public float TempsPoison = 0f;
        public float TempsBrulure = 0f;
        public float TempsGel = 0f;
        public float TempsEtourdi = 0f;
        public int DegatsPoisonTick = 0;
        public int DegatsBrulureTick = 0;
        private float tickChronoPoison = 0f;
        private float tickChronoBrulure = 0f;

        public bool EstSousEffetStatut => TempsPoison > 0f || TempsBrulure > 0f || TempsGel > 0f || TempsEtourdi > 0f;

        public void AppliquerStatut(string type, float duree, int degatsTick = 0)
        {
            if (ModeleHero.PVActuels <= 0 || EstEnDash) return;
            switch (type.ToLowerInvariant())
            {
                case "poison":
                    TempsPoison = Math.Max(TempsPoison, duree);
                    DegatsPoisonTick = Math.Max(DegatsPoisonTick, degatsTick);
                    break;
                case "brulure":
                case "feu":
                    TempsBrulure = Math.Max(TempsBrulure, duree);
                    DegatsBrulureTick = Math.Max(DegatsBrulureTick, degatsTick);
                    break;
                case "gel":
                case "givre":
                    TempsGel = Math.Max(TempsGel, duree);
                    break;
                case "etourdi":
                case "vertige":
                    TempsEtourdi = Math.Max(TempsEtourdi, duree);
                    break;
            }
        }

        public int SubirDegats(
            int degats,
            string source,
            GestionnaireParticules particules,
            Camera2D cam,
            Action<string, Color, bool> ajouterTexteFlottant,
            float intensiteSecousse = 7f,
            int degatsMinimum = 1)
        {
            if (degats <= 0 || EstEnDash || TempsInvulnerabilite > 0f || ModeleHero.PVActuels <= 0)
                return 0;

            int degatsInfliges = Math.Max(degatsMinimum, degats - ModeleHero.DefenseTotale / 2);
            ModeleHero.PVActuels = Math.Max(0, ModeleHero.PVActuels - degatsInfliges);
            TempsFlashDegats = 0.22f;
            TempsInvulnerabilite = 0.5f;
            cam.DeclencherSecousse(intensiteSecousse, 0.24f);
            AudioSynthetiseur.SonImpact();
            particules.EmettreEclats(Position, Color.Red, 12, 150f, 3.5f, 0.28f);
            string etiquette = string.IsNullOrWhiteSpace(source) ? "" : $" {source}";
            ajouterTexteFlottant($"-{degatsInfliges}{etiquette}", Color.FromArgb(231, 76, 60), intensiteSecousse >= 14f);
            return degatsInfliges;
        }

        public Joueur2D(Joueur hero, Vector2 positionInitiale)
        {
            ModeleHero = hero;
            Position = positionInitiale;
        }

        public void MettreAJour(float dt, GestionnaireEntrees entrees, Monde2D monde, GestionnaireParticules particules, Action<string, Color, bool> ajouterTexteFlottant)
        {
            TempsAnimation += dt;

            // Mise à jour des statuts négatifs (DoT, Ralentissement, Étourdissement)
            if (TempsPoison > 0f)
            {
                TempsPoison -= dt;
                tickChronoPoison += dt;
                if (tickChronoPoison >= 0.5f)
                {
                    tickChronoPoison = 0f;
                    int deg = Math.Max(1, DegatsPoisonTick);
                    ModeleHero.PVActuels = Math.Max(0, ModeleHero.PVActuels - deg);
                    TempsFlashDegats = 0.12f;
                    particules.EmettreEclats(Position, Color.LimeGreen, 3, 30f, 2f, 0.2f);
                    ajouterTexteFlottant($"🧪 -{deg}", Color.FromArgb(46, 204, 113), false);
                }
            }

            if (TempsBrulure > 0f)
            {
                TempsBrulure -= dt;
                tickChronoBrulure += dt;
                if (tickChronoBrulure >= 0.5f)
                {
                    tickChronoBrulure = 0f;
                    int deg = Math.Max(1, DegatsBrulureTick);
                    ModeleHero.PVActuels = Math.Max(0, ModeleHero.PVActuels - deg);
                    TempsFlashDegats = 0.12f;
                    particules.EmettreEclats(Position, Color.OrangeRed, 4, 35f, 2f, 0.2f);
                    ajouterTexteFlottant($"🔥 -{deg}", Color.FromArgb(230, 126, 34), false);
                }
            }

            if (TempsGel > 0f)
            {
                TempsGel -= dt;
                if (TempsAnimation % 0.25f < dt)
                {
                    particules.EmettreEclats(Position, Color.Cyan, 1, 15f, 2f, 0.2f);
                }
            }

            if (TempsEtourdi > 0f)
            {
                TempsEtourdi -= dt;
                Velocite = Vector2.Zero;
                return; // Le héros étourdi ne peut ni bouger ni attaquer
            }

            // Cooldowns
            if (DashCooldown > 0f) DashCooldown -= dt;
            if (CooldownAttaque > 0f) CooldownAttaque -= dt;
            if (CooldownCompetence1 > 0f) CooldownCompetence1 -= dt;
            if (CooldownCompetence2 > 0f) CooldownCompetence2 -= dt;
            if (AnimationAttaque > 0f) AnimationAttaque -= dt;
            if (TempsFlashDegats > 0f) TempsFlashDegats -= dt;
            if (TempsInvulnerabilite > 0f)
                TempsInvulnerabilite = MathF.Max(0f, TempsInvulnerabilite - dt);
            if (ChronoGlyphe > 0f) ChronoGlyphe -= dt;

            // Émission périodique d'aura de classe
            ChronoAura -= dt;
            if (ChronoAura <= 0f)
            {
                ChronoAura = 0.28f;
                particules.EmettreAuraClasse(Position, ModeleHero.Classe);
            }

            if (ComboTempsReset > 0f)
            {
                ComboTempsReset -= dt;
                if (ComboTempsReset <= 0f) ComboIndex = 0;
            }

            // Régénération naturelle de Mana
            if (ModeleHero.ManaActuel < ModeleHero.ManaMaxTotal)
            {
                ModeleHero.ManaActuel = Math.Min(ModeleHero.ManaMaxTotal, ModeleHero.ManaActuel + (int)(dt * 4));
            }

            // Angle vers la souris
            Vector2 deltaSouris = entrees.PositionSourisMonde - Position;
            AngleVise = MathF.Atan2(deltaSouris.Y, deltaSouris.X);

            // Gestion du Dash
            if (EstEnDash)
            {
                DashTempsRestant -= dt;
                float vitesseDash = 580f;
                Vector2 deplacement = DashDirection * vitesseDash * dt;
                Position = monde.ResoudreCollisions(Position, deplacement, Rayon);

                // Fantômes d'après-image
                FantomesDash.Add((Position, AngleVise, 1.0f));
                particules.EmettreEclats(Position, Color.FromArgb(120, 200, 255), 2, 40f, 3f, 0.25f);

                if (DashTempsRestant <= 0f)
                {
                    DashCooldown = DashCooldownMax;
                }
            }
            else
            {
                // Déplacements WASD / ZQSD
                Vector2 dirMouv = Vector2.Zero;
                if (entrees.AllerHaut) dirMouv.Y -= 1f;
                if (entrees.AllerBas) dirMouv.Y += 1f;
                if (entrees.AllerGauche) dirMouv.X -= 1f;
                if (entrees.AllerDroite) dirMouv.X += 1f;

                if (dirMouv.LongueurCarree() > 0.001f)
                {
                    AnimationMarche += dt * 14f;
                    dirMouv = dirMouv.Normaliser();
                    float vitesseDeBase = 210f + (ModeleHero.Agilite * 2.5f);
                    if (TempsGel > 0f) vitesseDeBase *= 0.60f; // Ralentissement de 40%
                    Velocite = Vector2.Lerp(Velocite, dirMouv * vitesseDeBase, MathF.Min(1f, dt * 14f));

                    // Particules de poussière de pas au sol
                    tempsPoussierePas -= dt;
                    if (tempsPoussierePas <= 0f)
                    {
                        tempsPoussierePas = 0.085f;
                        particules.EmettrePoussiereCourse(Position + new Vector2(0, Rayon - 2f));
                    }
                }
                else
                {
                    AnimationMarche = 0f;
                    Velocite = Vector2.Lerp(Velocite, Vector2.Zero, MathF.Min(1f, dt * 12f));
                }

                Position = monde.ResoudreCollisions(Position, Velocite * dt, Rayon);

                // Déclenchement du Dash (Espace)
                if (entrees.EsquiveDash && DashCooldown <= 0f)
                {
                    DashDirection = dirMouv.LongueurCarree() > 0.001f ? dirMouv : deltaSouris.Normaliser();
                    DashTempsRestant = 0.22f;
                    AudioSynthetiseur.SonDash();
                    particules.EmettreAnneauExplosion(Position, Color.Cyan, 16, 180f);
                }
            }

            // Mise à jour des fantômes de Dash
            for (int i = FantomesDash.Count - 1; i >= 0; i--)
            {
                var f = FantomesDash[i];
                float nouvAlpha = f.Alpha - dt * 4f;
                if (nouvAlpha <= 0f)
                {
                    FantomesDash.RemoveAt(i);
                }
                else
                {
                    FantomesDash[i] = (f.Pos, f.Angle, nouvAlpha);
                }
            }
        }

        public void AttaquePrimaire(Monde2D monde, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            if (CooldownAttaque > 0f || EstEnDash) return;

            CooldownAttaque = 0.32f;
            AnimationAttaque = AnimationDuree;
            ComboIndex = ComboTempsReset > 0f ? Math.Min(3, ComboIndex + 1) : 1;
            ComboTempsReset = 1.4f;

            // Dépend de la classe
            if (ModeleHero.Classe == ClasseType.Rodeur)
            {
                // Tir d'arc
                Vector2 dir = new Vector2(MathF.Cos(AngleVise), MathF.Sin(AngleVise));
                monde.AjouterProjectile(new Projectile2D
                {
                    Position = Position + dir * 20f,
                    Velocite = dir * 650f,
                    Rayon = 5f,
                    Degats = ModeleHero.AttaqueTotale,
                    EstDuJoueur = true,
                    Couleur = Color.FromArgb(46, 204, 113),
                    TempsVie = 1.5f,
                    NomSort = "Tir d'Élite"
                });
                AudioSynthetiseur.SonCoupEpee();
            }
            else if (ModeleHero.Classe == ClasseType.Mage)
            {
                // Éclair / Orbe Arcanique
                Vector2 dir = new Vector2(MathF.Cos(AngleVise), MathF.Sin(AngleVise));
                monde.AjouterProjectile(new Projectile2D
                {
                    Position = Position + dir * 20f,
                    Velocite = dir * 550f,
                    Rayon = 8f,
                    Degats = (int)(ModeleHero.AttaqueTotale * 1.15f),
                    EstDuJoueur = true,
                    Couleur = Color.FromArgb(155, 89, 182),
                    TempsVie = 1.6f,
                    NomSort = "Orbe d'Éther"
                });
                AudioSynthetiseur.SonTirMagique();
            }
            else
            {
                // Attaque de mêlée (Guerrier, Paladin, Nécromancien)
                AudioSynthetiseur.SonCoupEpee();
                particules.EmettreEclats(Position + new Vector2(MathF.Cos(AngleVise), MathF.Sin(AngleVise)) * 35f, Color.White, 8, 120f, 3f, 0.2f);
                monde.InfligerDegatsZone(Position, 75f, AngleVise, MathF.PI * 0.75f, ModeleHero.AttaqueTotale, this, particules, cam, ajouterTexteFlottant);
            }

            JaugeUltime = Math.Min(100f, JaugeUltime + 6f);
        }

        public void AttaqueCompetence1(Monde2D monde, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            if (CooldownCompetence1 > 0f || EstEnDash) return;

            int coutMana = 8;
            if (ModeleHero.ManaActuel < coutMana)
            {
                ajouterTexteFlottant("Pas assez de Mana !", Color.FromArgb(52, 152, 219), false);
                return;
            }

            ModeleHero.ManaActuel -= coutMana;
            CooldownCompetence1 = 0.5f;

            if (ModeleHero.Classe == ClasseType.Guerrier)
            {
                // Tourbillon dévastateur 360°
                AudioSynthetiseur.SonExplosion();
                cam.DeclencherEffetSort(13f, 0.34f, 0.065f, Color.FromArgb(231, 76, 60), 42);
                particules.EmettreAnimationSort(Position, new Vector2(MathF.Cos(AngleVise), MathF.Sin(AngleVise)), Color.FromArgb(231, 76, 60), TypeAnimationSort2D.Tourbillon, 0.62f);
                monde.InfligerDegatsRayon(Position, 115f, (int)(ModeleHero.AttaqueTotale * 1.85f), this, particules, cam, ajouterTexteFlottant);
                ajouterTexteFlottant("🌪️ Tourbillon !", Color.FromArgb(231, 76, 60), true);
            }
            else if (ModeleHero.Classe == ClasseType.Mage)
            {
                // Nova de feu à la souris
                Vector2 cible = monde.PositionSourisActuelle;
                AudioSynthetiseur.SonExplosion();
                cam.DeclencherEffetSort(10f, 0.28f, 0.045f, Color.FromArgb(255, 112, 38), 38);
                particules.EmettreAnimationSort(cible, Vector2.Zero, Color.FromArgb(230, 126, 34), TypeAnimationSort2D.NovaFeu, 0.68f);
                monde.InfligerDegatsRayon(cible, 105f, (int)(ModeleHero.AttaqueTotale * 2.1f), this, particules, cam, ajouterTexteFlottant, "Brulure", 4.0f, (int)(ModeleHero.AttaqueTotale * 0.25f));
                ajouterTexteFlottant("🔥 Nova de Feu !", Color.FromArgb(230, 126, 34), true);
            }
            else if (ModeleHero.Classe == ClasseType.Rodeur)
            {
                // Volée conique de 5 flèches
                AudioSynthetiseur.SonCoupEpee();
                cam.DeclencherEffetSort(5f, 0.18f, 0.025f, Color.FromArgb(80, 210, 130), 22);
                particules.EmettreAnimationSort(Position, new Vector2(MathF.Cos(AngleVise), MathF.Sin(AngleVise)), Color.FromArgb(46, 204, 113), TypeAnimationSort2D.Volee, 0.48f);
                for (int i = -2; i <= 2; i++)
                {
                    float angle = AngleVise + (i * 0.14f);
                    Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                    monde.AjouterProjectile(new Projectile2D
                    {
                        Position = Position + dir * 20f,
                        Velocite = dir * 720f,
                        Rayon = 5f,
                        Degats = (int)(ModeleHero.AttaqueTotale * 0.95f),
                        EstDuJoueur = true,
                        Couleur = Color.FromArgb(46, 204, 113),
                        TempsVie = 1.3f,
                        NomSort = "Volée"
                    });
                }
                ajouterTexteFlottant("🏹 Volée de Flèches !", Color.FromArgb(46, 204, 113), true);
            }
            else if (ModeleHero.Classe == ClasseType.Paladin)
            {
                // Châtiment Divin
                AudioSynthetiseur.SonExplosion();
                cam.DeclencherEffetSort(13f, 0.38f, 0.07f, Color.Gold, 52);
                particules.EmettreAnimationSort(Position, Vector2.Zero, Color.Gold, TypeAnimationSort2D.Chatiment, 0.7f);
                monde.InfligerDegatsRayon(Position, 110f, (int)(ModeleHero.AttaqueTotale * 1.95f), this, particules, cam, ajouterTexteFlottant);
                int soin = (int)(ModeleHero.PVMaxTotal * 0.12f);
                ModeleHero.Soigner(soin);
                ajouterTexteFlottant($"✨ Châtiment (+{soin} PV) !", Color.FromArgb(241, 196, 15), true);
            }
            else // Nécromancien
            {
                // Faux des Ombres vampirique
                AudioSynthetiseur.SonTirMagique();
                cam.DeclencherEffetSort(11f, 0.32f, 0.06f, Color.FromArgb(142, 68, 173), 45);
                particules.EmettreAnimationSort(Position, new Vector2(MathF.Cos(AngleVise), MathF.Sin(AngleVise)), Color.FromArgb(142, 68, 173), TypeAnimationSort2D.FauxOmbre, 0.64f);
                monde.InfligerDegatsRayon(Position, 100f, (int)(ModeleHero.AttaqueTotale * 1.8f), this, particules, cam, ajouterTexteFlottant, "Poison", 4.5f, (int)(ModeleHero.AttaqueTotale * 0.20f));
                int drain = (int)(ModeleHero.PVMaxTotal * 0.09f);
                ModeleHero.Soigner(drain);
                ajouterTexteFlottant($"💀 Faux Drain (+{drain} PV) !", Color.FromArgb(155, 89, 182), true);
            }

            JaugeUltime = Math.Min(100f, JaugeUltime + 14f);
        }

        public void AttaqueSecondaire(Monde2D monde, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            AttaqueCompetence1(monde, particules, cam, ajouterTexteFlottant);
        }

        public void AttaqueCompetence2(Monde2D monde, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            if (CooldownCompetence2 > 0f || EstEnDash) return;

            int coutMana = 15;
            if (ModeleHero.ManaActuel < coutMana)
            {
                ajouterTexteFlottant("Pas assez de Mana !", Color.FromArgb(52, 152, 219), false);
                return;
            }

            ModeleHero.ManaActuel -= coutMana;
            CooldownCompetence2 = 0.75f;

            // Si un sort forgé ou grimoire est équipé
            if (ModeleHero.CompetenceEquipee != null)
            {
                var sort = ModeleHero.CompetenceEquipee;
                int degatsSort = (int)(ModeleHero.AttaqueTotale * sort.MultiplicateurDegats);
                Vector2 dir = new Vector2(MathF.Cos(AngleVise), MathF.Sin(AngleVise));
                Vector2 cible = monde.PositionSourisActuelle;

                switch (sort.Id)
                {
                    case "c_dragon_flammes":
                        AudioSynthetiseur.SonExplosion();
                        cam.DeclencherEffetSort(12f, 0.3f, 0.04f, Color.FromArgb(231, 76, 60), 45);
                        particules.EmettreAnimationSort(Position, dir, Color.FromArgb(231, 76, 60), TypeAnimationSort2D.DragonFlammes, 0.85f);
                        for (int i = 0; i < 3; i++)
                        {
                            float decAngle = (i - 1) * 0.18f;
                            Vector2 dirFlamme = new Vector2(MathF.Cos(AngleVise + decAngle), MathF.Sin(AngleVise + decAngle));
                            monde.AjouterProjectile(new Projectile2D
                            {
                                Position = Position + dirFlamme * 25f,
                                Velocite = dirFlamme * 720f,
                                Rayon = 10f,
                                Degats = (int)(degatsSort * 0.45f),
                                EstDuJoueur = true,
                                Couleur = Color.OrangeRed,
                                TempsVie = 1.6f,
                                Transpercant = false,
                                NomSort = "Wyrm de Feu"
                            });
                        }
                        break;

                    case "c_phenix_givre":
                        AudioSynthetiseur.SonTirMagique();
                        cam.DeclencherEffetSort(10f, 0.28f, 0.035f, Color.Cyan, 40);
                        particules.EmettreAnimationSort(Position, dir, Color.Cyan, TypeAnimationSort2D.PhenixGivre, 0.85f);
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + dir * 30f,
                            Velocite = dir * 850f,
                            Rayon = 16f,
                            Degats = degatsSort,
                            EstDuJoueur = true,
                            Couleur = Color.FromArgb(120, 220, 255),
                            TempsVie = 2.0f,
                            Transpercant = true,
                            NomSort = "Phénix de Givre"
                        });
                        break;

                    case "c_trombe_tempete":
                        AudioSynthetiseur.SonDash();
                        cam.DeclencherEffetSort(14f, 0.35f, 0.05f, Color.FromArgb(200, 230, 245), 50);
                        particules.EmettreAnimationSort(cible, Vector2.Zero, Color.FromArgb(200, 230, 245), TypeAnimationSort2D.TrombeTempete, 1.3f);
                        foreach (var m in monde.Monstres)
                        {
                            if (!m.EstMort && Vector2.Distance(m.Position, cible) <= 220f)
                            {
                                m.Position = Vector2.Lerp(m.Position, cible, 0.65f);
                            }
                        }
                        monde.InfligerDegatsRayon(cible, 220f, (int)(degatsSort * 0.75f), this, particules, cam, ajouterTexteFlottant);
                        break;

                    case "c_dome_sacre":
                        AudioSynthetiseur.SonCritique();
                        cam.DeclencherEffetSort(15f, 0.4f, 0.06f, Color.Gold, 55);
                        particules.EmettreAnimationSort(Position, Vector2.Zero, Color.Gold, TypeAnimationSort2D.DomeSacre, 1.4f);
                        TempsInvulnerabilite = 1.2f;
                        foreach (var m in monde.Monstres)
                        {
                            float d = Vector2.Distance(m.Position, Position);
                            if (!m.EstMort && d <= 170f)
                            {
                                Vector2 repousse = (m.Position - Position).Normaliser();
                                m.Position += repousse * 110f;
                            }
                        }
                        monde.InfligerDegatsRayon(Position, 170f, degatsSort, this, particules, cam, ajouterTexteFlottant, "Etourdi", 1.2f);
                        break;

                    case "c_vortex_ames":
                        AudioSynthetiseur.SonTirMagique();
                        cam.DeclencherEffetSort(12f, 0.32f, 0.045f, Color.FromArgb(155, 89, 182), 48);
                        particules.EmettreAnimationSort(cible, Vector2.Zero, Color.FromArgb(155, 89, 182), TypeAnimationSort2D.VortexAmes, 1.4f);
                        monde.InfligerDegatsRayon(cible, 140f, degatsSort, this, particules, cam, ajouterTexteFlottant, "Poison", 3.5f);
                        break;

                    case "c_orbe_foudre":
                        AudioSynthetiseur.SonTirMagique();
                        cam.DeclencherEffetSort(12f, 0.25f, 0.03f, Color.Cyan, 45);
                        particules.EmettreAnimationSort(Position, dir, Color.Cyan, TypeAnimationSort2D.OrbeFoudre, 0.75f);
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + dir * 25f,
                            Velocite = dir * 750f,
                            Rayon = 14f,
                            Degats = degatsSort,
                            EstDuJoueur = true,
                            Couleur = Color.Cyan,
                            TempsVie = 1.5f,
                            Transpercant = true,
                            NomSort = "Foudre Électrique"
                        });
                        break;

                    case "c_croissant_lunaire":
                        AudioSynthetiseur.SonTirMagique();
                        cam.DeclencherEffetSort(14f, 0.35f, 0.05f, Color.FromArgb(255, 180, 220), 52);
                        particules.EmettreAnimationSort(Position, dir, Color.FromArgb(255, 180, 220), TypeAnimationSort2D.CroissantLunaire, 0.9f);
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + dir * 25f,
                            Velocite = dir * 820f,
                            Rayon = 22f,
                            Degats = (int)(degatsSort * 1.2f),
                            EstDuJoueur = true,
                            Couleur = Color.FromArgb(255, 215, 0),
                            TempsVie = 1.8f,
                            Transpercant = true,
                            NomSort = "Croissant Lunaire"
                        });
                        break;

                    case "c_dragon_tricephale":
                        AudioSynthetiseur.SonExplosion();
                        cam.DeclencherEffetSort(16f, 0.38f, 0.06f, Color.OrangeRed, 60);
                        particules.EmettreAnimationSort(Position, dir, Color.OrangeRed, TypeAnimationSort2D.DragonTricephale, 1.0f);
                        for (int i = -1; i <= 1; i++)
                        {
                            float a = AngleVise + i * 0.3f;
                            Vector2 d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                            monde.AjouterProjectile(new Projectile2D
                            {
                                Position = Position + d * 30f,
                                Velocite = d * 700f,
                                Rayon = 12f,
                                Degats = (int)(degatsSort * 0.55f),
                                EstDuJoueur = true,
                                Couleur = Color.FromArgb(255, 100, 20),
                                TempsVie = 1.6f,
                                NomSort = "Feu Tricéphale"
                            });
                        }
                        break;

                    case "c_leviathan_glaces":
                        AudioSynthetiseur.SonTirMagique();
                        cam.DeclencherEffetSort(14f, 0.36f, 0.05f, Color.Cyan, 55);
                        particules.EmettreAnimationSort(Position, dir, Color.FromArgb(85, 195, 255), TypeAnimationSort2D.LeviathanGlaces, 1.1f);
                        monde.InfligerDegatsRayon(Position + dir * 180f, 130f, degatsSort, this, particules, cam, ajouterTexteFlottant, "Gel", 4.0f);
                        break;

                    case "c_miroir_glace":
                        AudioSynthetiseur.SonCritique();
                        cam.DeclencherEffetSort(10f, 0.28f, 0.04f, Color.FromArgb(180, 230, 255), 45);
                        particules.EmettreAnimationSort(Position + dir * 50f, dir, Color.FromArgb(180, 230, 255), TypeAnimationSort2D.MiroirGlace, 1.2f);
                        monde.InfligerDegatsRayon(Position + dir * 60f, 90f, degatsSort, this, particules, cam, ajouterTexteFlottant, "Gel", 2.5f);
                        break;

                    case "c_trou_noir_tellurique":
                        AudioSynthetiseur.SonExplosion();
                        cam.DeclencherEffetSort(18f, 0.45f, 0.07f, Color.FromArgb(70, 90, 160), 65);
                        particules.EmettreAnimationSort(cible, Vector2.Zero, Color.FromArgb(70, 90, 160), TypeAnimationSort2D.SingulariteTellurique, 1.5f);
                        foreach (var m in monde.Monstres)
                        {
                            if (!m.EstMort && Vector2.Distance(m.Position, cible) <= 250f)
                            {
                                m.Position = Vector2.Lerp(m.Position, cible, 0.75f);
                            }
                        }
                        monde.InfligerDegatsRayon(cible, 250f, degatsSort, this, particules, cam, ajouterTexteFlottant, "Etourdi", 1.5f);
                        break;

                    case "c_prisme_irise":
                        AudioSynthetiseur.SonTirMagique();
                        cam.DeclencherEffetSort(16f, 0.38f, 0.06f, Color.White, 58);
                        particules.EmettreAnimationSort(Position, dir, Color.White, TypeAnimationSort2D.PrismeIrise, 1.0f);
                        for (int l = -2; l <= 2; l++)
                        {
                            float a = AngleVise + l * 0.22f;
                            Vector2 d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                            monde.AjouterProjectile(new Projectile2D
                            {
                                Position = Position + d * 25f,
                                Velocite = d * 920f,
                                Rayon = 8f,
                                Degats = (int)(degatsSort * 0.38f),
                                EstDuJoueur = true,
                                Couleur = Color.White,
                                TempsVie = 1.5f,
                                Transpercant = true,
                                NomSort = "Laser Prismatique"
                            });
                        }
                        break;

                    case "c_abysse_eldritch":
                        AudioSynthetiseur.SonExplosion();
                        cam.DeclencherEffetSort(20f, 0.5f, 0.08f, Color.FromArgb(120, 50, 180), 70);
                        particules.EmettreAnimationSort(cible, Vector2.Zero, Color.FromArgb(120, 50, 180), TypeAnimationSort2D.AbysseEldritch, 1.6f);
                        monde.InfligerDegatsRayon(cible, 160f, (int)(degatsSort * 1.3f), this, particules, cam, ajouterTexteFlottant, "Etourdi", 2.0f);
                        break;

                    case "c_portail_givre":
                        AudioSynthetiseur.SonTirMagique();
                        cam.DeclencherEffetSort(11f, 0.3f, 0.04f, Color.Cyan, 45);
                        particules.EmettreAnimationSort(Position + dir * 65f, dir, Color.Cyan, TypeAnimationSort2D.PortailGlacial, 1.2f);
                        monde.InfligerDegatsRayon(Position + dir * 65f, 100f, degatsSort, this, particules, cam, ajouterTexteFlottant, "Gel", 3.0f);
                        break;

                    default:
                        AudioSynthetiseur.SonExplosion();
                        cam.DeclencherEffetSort(9f, 0.25f, 0.04f, Color.Cyan, 35);
                        particules.EmettreAnimationSort(Position, dir, Color.Cyan, TypeAnimationSort2D.SortForge, 0.52f);
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + dir * 25f,
                            Velocite = dir * 680f,
                            Rayon = 12f,
                            Degats = degatsSort,
                            EstDuJoueur = true,
                            Couleur = Color.FromArgb(241, 196, 15),
                            TempsVie = 1.8f,
                            Transpercant = true,
                            NomSort = sort.Nom
                        });
                        break;
                }

                ajouterTexteFlottant($"{sort.Icone} {sort.Nom} !", Color.Gold, true);
            }
            else
            {
                // Sort de classe secondaire de base
                Vector2 dir = new Vector2(MathF.Cos(AngleVise), MathF.Sin(AngleVise));

                if (ModeleHero.Classe == ClasseType.Guerrier)
                {
                    // Onde tellurique linéaire
                    AudioSynthetiseur.SonExplosion();
                    cam.DeclencherEffetSort(14f, 0.34f, 0.055f, Color.FromArgb(255, 95, 45), 48);
                    particules.EmettreAnimationSort(Position, dir, Color.OrangeRed, TypeAnimationSort2D.FracasTellurique, 0.62f);
                    for (int i = 1; i <= 4; i++)
                    {
                        Vector2 stepPos = Position + dir * (i * 60f);
                        monde.InfligerDegatsRayon(stepPos, 55f, (int)(ModeleHero.AttaqueTotale * 1.55f), this, particules, cam, ajouterTexteFlottant, "Etourdi", 1.2f);
                    }
                    ajouterTexteFlottant("🌋 Fracas Tellurique !", Color.OrangeRed, true);
                }
                else if (ModeleHero.Classe == ClasseType.Mage)
                {
                    // Foudre arcanique perçante
                    AudioSynthetiseur.SonTirMagique();
                    cam.DeclencherEffetSort(12f, 0.22f, 0.025f, Color.Cyan, 50);
                    particules.EmettreAnimationSort(Position, dir, Color.Cyan, TypeAnimationSort2D.Foudre, 0.42f);
                    monde.AjouterProjectile(new Projectile2D
                    {
                        Position = Position + dir * 20f,
                        Velocite = dir * 880f,
                        Rayon = 9f,
                        Degats = (int)(ModeleHero.AttaqueTotale * 2.25f),
                        EstDuJoueur = true,
                        Couleur = Color.Cyan,
                        TempsVie = 1.2f,
                        Transpercant = true,
                        NomSort = "Foudre Céleste"
                    });
                    ajouterTexteFlottant("⚡ Foudre Arcanique !", Color.Cyan, true);
                }
                else if (ModeleHero.Classe == ClasseType.Rodeur)
                {
                    // Flèche explosive
                    AudioSynthetiseur.SonTirMagique();
                    cam.DeclencherEffetSort(8f, 0.27f, 0.05f, Color.Orange, 40);
                    monde.AjouterProjectile(new Projectile2D
                    {
                        Position = Position + dir * 20f,
                        Velocite = dir * 780f,
                        Rayon = 7f,
                        Degats = (int)(ModeleHero.AttaqueTotale * 2.1f),
                        EstDuJoueur = true,
                        Couleur = Color.Orange,
                        TempsVie = 1.4f,
                        NomSort = "Flèche Explosive"
                    });
                    ajouterTexteFlottant("💣 Tir Explosif !", Color.Orange, true);
                }
                else if (ModeleHero.Classe == ClasseType.Paladin)
                {
                    // Rayon Sacré du Ciel
                    Vector2 cible = monde.PositionSourisActuelle;
                    AudioSynthetiseur.SonCritique();
                    cam.DeclencherEffetSort(15f, 0.4f, 0.065f, Color.Gold, 58);
                    particules.EmettreAnimationSort(cible, Vector2.Zero, Color.Gold, TypeAnimationSort2D.RayonSacre, 0.72f);
                    monde.InfligerDegatsRayon(cible, 90f, (int)(ModeleHero.AttaqueTotale * 2.15f), this, particules, cam, ajouterTexteFlottant, "Etourdi", 1.0f);
                    ajouterTexteFlottant("☀️ Rayon Sacré !", Color.Gold, true);
                }
                else // Nécromancien
                {
                    // 3 Orbes d'âmes voraces
                    AudioSynthetiseur.SonTirMagique();
                    cam.DeclencherEffetSort(10f, 0.3f, 0.04f, Color.FromArgb(142, 68, 173), 42);
                    particules.EmettreAnimationSort(Position, dir, Color.FromArgb(142, 68, 173), TypeAnimationSort2D.EssaimAmes, 0.75f);
                    for (int i = -1; i <= 1; i++)
                    {
                        float angle = AngleVise + (i * 0.28f);
                        Vector2 d = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + d * 20f,
                            Velocite = d * 500f,
                            Rayon = 7f,
                            Degats = (int)(ModeleHero.AttaqueTotale * 1.15f),
                            EstDuJoueur = true,
                            Couleur = Color.FromArgb(142, 68, 173),
                            TempsVie = 2.0f,
                            NomSort = "Âme Vorace"
                        });
                    }
                    ajouterTexteFlottant("👻 Essaim d'Âmes !", Color.Plum, true);
                }
            }

            JaugeUltime = Math.Min(100f, JaugeUltime + 15f);
        }

        public void DeclencherUltime(Monde2D monde, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            if (JaugeUltime < 100f)
            {
                ajouterTexteFlottant($"⚡ Ultime : {JaugeUltime:F0}% (Combattez pour charger !)", Color.Orange, false);
                return;
            }

            JaugeUltime = 0f;
            cam.DeclencherEffetSort(25f, 0.85f, 0.12f, Color.Gold, 96);
            AudioSynthetiseur.SonCriBoss();

            particules.EmettreAnimationSort(Position, Vector2.Zero, Color.Gold, TypeAnimationSort2D.Ultime, 0.9f);

            int degatsUltime = (int)(ModeleHero.AttaqueTotale * 4.8f) + 80;
            monde.InfligerDegatsRayon(Position, 280f, degatsUltime, this, particules, cam, ajouterTexteFlottant);
            ajouterTexteFlottant("⚡ ULTIME CATACLYSMIQUE !", Color.Gold, true);
        }

        public void Dessiner(Graphics g, Camera2D cam)
        {
            // Dessin des fantômes de dash spectraux (Mirage dynamique)
            for (int i = 0; i < FantomesDash.Count; i++)
            {
                var f = FantomesDash[i];
                Point pf = cam.MondeVersEcran(f.Pos);
                int a = (int)(f.Alpha * 120);
                if (a <= 0) continue;
                Color colGhost = i % 2 == 0 ? Color.Cyan : Color.MediumPurple;
                SolidBrush bf = CacheRenduGDI.ObtenirBrushAlpha(a, colGhost);
                
                // Silhouette fantôme étirée
                GraphicsState etatDash = g.Save();
                g.TranslateTransform(pf.X, pf.Y);
                g.RotateTransform(f.Angle * 180f / MathF.PI);
                
                g.FillEllipse(bf, -Rayon * 1.5f, -Rayon * 0.8f, Rayon * 3f, Rayon * 1.6f); // Torse étiré
                g.FillRectangle(bf, -Rayon * 2f, -Rayon * 0.4f, Rayon * 1.5f, Rayon * 0.8f); // Traînée résiduelle
                
                g.Restore(etatDash);
            }

            // Animation procédurale de pas (bobbing vertical & squash)
            bool enMouvement = Velocite.LongueurCarree() > 10f;
            float phaseIdle = TempsAnimation * 2.2f;
            float bobY = enMouvement ? MathF.Abs(MathF.Sin(AnimationMarche)) * 3.5f : MathF.Sin(phaseIdle) * 1.2f;
            float squashX = enMouvement ? (1f + MathF.Sin(AnimationMarche * 2f) * 0.06f) : (1f + MathF.Sin(phaseIdle * 0.5f) * 0.018f);
            float squashY = 2f - squashX;

            Point p = cam.MondeVersEcran(new Vector2(Position.X, Position.Y - bobY));
            Point pSol = cam.MondeVersEcran(Position);

            // Ombre portée au sol dynamique
            SolidBrush bOmbre = CacheRenduGDI.ObtenirBrushAlpha(85, Color.Black);
            g.FillEllipse(bOmbre, pSol.X - Rayon * 1.1f, pSol.Y + Rayon * 0.35f, Rayon * 2.2f, Rayon * 0.8f);

            // Glyphe magique au sol sous les pieds lors d'un sort
            if (ChronoGlyphe > 0f)
            {
                float ratioG = ChronoGlyphe / 0.5f;
                float rG = Rayon * (1.2f + (1f - ratioG) * 0.9f);
                Pen pG = CacheRenduGDI.ObtenirPen(Color.FromArgb((int)(ratioG * 210), CouleurDernierSort), 1.8f);
                g.DrawEllipse(pG, pSol.X - rG, pSol.Y - rG * 0.45f + (Rayon * 0.5f), rG * 2f, rG * 0.9f);
            }

            // Halo d'invulnérabilité pendant le Dash
            if (EstEnDash)
            {
                SolidBrush bHalo = CacheRenduGDI.ObtenirBrushAlpha(130, Color.Cyan);
                g.FillEllipse(bHalo, p.X - Rayon * 1.45f, p.Y - Rayon * 1.45f, Rayon * 2.9f, Rayon * 2.9f);
            }

            // Couleur selon la classe
            Color couleurClasse = ModeleHero.Classe switch
            {
                ClasseType.Guerrier => Color.FromArgb(231, 76, 60),
                ClasseType.Mage => Color.FromArgb(52, 152, 219),
                ClasseType.Rodeur => Color.FromArgb(46, 204, 113),
                ClasseType.Paladin => Color.FromArgb(241, 196, 15),
                ClasseType.Necromancien => Color.FromArgb(155, 89, 182),
                _ => Color.White
            };

            // Corps Humanoïde Détaillé avec Cape Dynamique et Articulations
            float rx = Rayon * squashX;
            float ry = Rayon * squashY;

            bool clignoteInvulnerable = TempsInvulnerabilite > 0f && ((int)(TempsInvulnerabilite * 20f) % 2 == 0);
            Color couleurCorps = TempsFlashDegats > 0f || clignoteInvulnerable ? Color.White : couleurClasse;
            SolidBrush bCorps = CacheRenduGDI.ObtenirBrush(couleurCorps);
            SolidBrush bCape = CacheRenduGDI.ObtenirBrush(Color.FromArgb(190, Math.Max(0, couleurClasse.R - 50), Math.Max(0, couleurClasse.G - 50), Math.Max(0, couleurClasse.B - 50)));
            Pen penContour = CacheRenduGDI.ObtenirPen(Color.FromArgb(25, 25, 30), 1.8f);
            
            // Animation Cape Fluide
            float vitesse = MathF.Sqrt(Velocite.X * Velocite.X + Velocite.Y * Velocite.Y);
            if (vitesse > 5f)
            {
                float angleMouv = MathF.Atan2(Velocite.Y, Velocite.X);
                float reculCape = 14f + MathF.Sin(AnimationMarche * 2f) * 3f;
                Point[] cape = {
                    new Point(p.X - 7, p.Y - 5),
                    new Point(p.X + 7, p.Y - 5),
                    new Point((int)(p.X - MathF.Cos(angleMouv - 0.2f) * reculCape), (int)(p.Y - MathF.Sin(angleMouv - 0.2f) * reculCape) + 8),
                    new Point((int)(p.X - MathF.Cos(angleMouv + 0.2f) * reculCape), (int)(p.Y - MathF.Sin(angleMouv + 0.2f) * reculCape) + 8)
                };
                g.FillPolygon(bCape, cape);
                g.DrawPolygon(penContour, cape);
            }
            else
            {
                float idleCape = MathF.Sin(phaseIdle) * 2f;
                g.FillRectangle(bCape, p.X - 8, p.Y - 5, 16, 18 + idleCape);
                g.DrawRectangle(penContour, p.X - 8, p.Y - 5, 16, 18 + idleCape);
            }

            // Calcul des cinématiques inverses (IK) et animations vectorielles fluides
            float phaseMembres = enMouvement ? AnimationMarche : phaseIdle * 0.22f;
            float amplitudeRepos = enMouvement ? 1f : 0.13f;
            float angleMarcheJambeG = MathF.Cos(phaseMembres) * 0.6f * amplitudeRepos;
            float angleMarcheJambeD = -MathF.Cos(phaseMembres) * 0.6f * amplitudeRepos;
            float angleBrasG = MathF.Sin(phaseMembres) * 0.6f * amplitudeRepos;

            // Inclinaison du corps lors d'une attaque
            float progressionAttaque = AnimationAttaque > 0f ? 1f - AnimationAttaque / AnimationDuree : 0f;
            float intensiteAttaque = AnimationAttaque > 0f ? MathF.Sin(progressionAttaque * MathF.PI) : 0f;
            float inclinaisonAttaque = intensiteAttaque * 0.35f;
            float yDecalageAttaque = intensiteAttaque * 3f;

            Pen penJambe = CacheRenduGDI.ObtenirPenArrondi(Color.FromArgb(45, 45, 55), 6f);
            Pen penBotte = CacheRenduGDI.ObtenirPenArrondi(Color.FromArgb(30, 25, 25), 6.5f);
            Pen penBras = CacheRenduGDI.ObtenirPenArrondi(Color.FromArgb(45, 45, 55), 5.5f);

            // --- JAMBES (Articulées via DrawLine pour une fluidité parfaite) ---
            float hancheY = p.Y + 7 + yDecalageAttaque;
            
            // Jambe Gauche
            float hancheGx = p.X - 4;
            float genouGx = hancheGx + MathF.Sin(angleMarcheJambeG) * 6f; float genouGy = hancheY + MathF.Cos(angleMarcheJambeG) * 6f;
            float piedGx = genouGx + MathF.Sin(angleMarcheJambeG + 0.2f) * 6f; float piedGy = genouGy + MathF.Cos(angleMarcheJambeG + 0.2f) * 6f;
            g.DrawLine(penJambe, hancheGx, hancheY, genouGx, genouGy);
            g.DrawLine(penBotte, genouGx, genouGy, piedGx, piedGy);

            // Jambe Droite
            float hancheDx = p.X + 4;
            float genouDx = hancheDx + MathF.Sin(angleMarcheJambeD) * 6f; float genouDy = hancheY + MathF.Cos(angleMarcheJambeD) * 6f;
            float piedDx = genouDx + MathF.Sin(angleMarcheJambeD + 0.2f) * 6f; float piedDy = genouDy + MathF.Cos(angleMarcheJambeD + 0.2f) * 6f;
            g.DrawLine(penJambe, hancheDx, hancheY, genouDx, genouDy);
            g.DrawLine(penBotte, genouDx, genouDy, piedDx, piedDy);

            // --- TORSE (Avec rotation d'attaque et respiration) ---
            GraphicsState etatTorse = g.Save();
            g.TranslateTransform(p.X, p.Y + yDecalageAttaque);
            g.RotateTransform(inclinaisonAttaque * 180f / MathF.PI);

            g.FillPie(bCorps, -10, -8, 20, 20, 180, 180); // Épaules rondes profilées
            g.FillRectangle(bCorps, -10, 2, 20, 7); // Buste
            g.DrawArc(penContour, -10, -8, 20, 20, 180, 180);
            g.DrawLine(penContour, -10, 2, -10, 9);
            g.DrawLine(penContour, 10, 2, 10, 9);

            // Ceinture détaillée
            SolidBrush bCeinture = CacheRenduGDI.ObtenirBrush(Color.FromArgb(60, 40, 20));
            g.FillRectangle(bCeinture, -10, 6, 20, 4);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.Gold), -3, 5, 6, 6);
            
            g.Restore(etatTorse);

            // Épaulières dynamiques
            g.FillEllipse(bCorps, p.X - 13, p.Y - 7 + yDecalageAttaque, 8, 8);
            g.FillEllipse(bCorps, p.X + 5, p.Y - 7 + yDecalageAttaque, 8, 8);
            g.DrawEllipse(penContour, p.X - 13, p.Y - 7 + yDecalageAttaque, 8, 8);
            g.DrawEllipse(penContour, p.X + 5, p.Y - 7 + yDecalageAttaque, 8, 8);

            // --- BRAS (Fluides, balancier naturel) ---
            // Bras Gauche
            float epauleGx = p.X - 10; float epauleGy = p.Y - 2 + yDecalageAttaque;
            float coudeGx = epauleGx + MathF.Sin(angleBrasG) * 5f; float coudeGy = epauleGy + MathF.Cos(angleBrasG) * 5f;
            float mainGx = coudeGx + MathF.Sin(angleBrasG) * 5f; float mainGy = coudeGy + MathF.Cos(angleBrasG) * 5f;
            g.DrawLine(penBras, epauleGx, epauleGy, coudeGx, coudeGy);
            g.DrawLine(penBras, coudeGx, coudeGy, mainGx, mainGy);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(255, 215, 175)), mainGx - 3, mainGy - 3, 6, 6);

            // Bras Droit (tenant l'arme, tendu vers la visée)
            float epauleDx = p.X + 10; float epauleDy = p.Y - 2 + yDecalageAttaque;
            float angleBrasAttaque = AngleVise - intensiteAttaque * 0.85f;
            float mainDx = epauleDx + MathF.Cos(angleBrasAttaque) * 11f; float mainDy = epauleDy + MathF.Sin(angleBrasAttaque) * 11f;
            g.DrawLine(penBras, epauleDx, epauleDy, mainDx, mainDy);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(255, 215, 175)), mainDx - 3, mainDy - 3, 6, 6);

            // Tête & Visage
            SolidBrush bPeau = CacheRenduGDI.ObtenirBrush(TempsFlashDegats > 0f || clignoteInvulnerable ? Color.White : Color.FromArgb(255, 215, 175));
            g.FillEllipse(bPeau, p.X - 8, p.Y - 20, 16, 16);
            g.DrawEllipse(penContour, p.X - 8, p.Y - 20, 16, 16);

            // Cheveux / Heaume stylisé
            SolidBrush bCheveux = CacheRenduGDI.ObtenirBrush(couleurClasse);
            g.FillPie(bCheveux, p.X - 10, p.Y - 23, 20, 18, 160, 220); // Coupe en arc
            g.DrawArc(penContour, p.X - 10, p.Y - 23, 20, 18, 160, 220);

            // Yeux expressifs
            float eyeDist = 4.5f;
            float eyeX = p.X + MathF.Cos(AngleVise) * eyeDist;
            float eyeY = p.Y - 14 + MathF.Sin(AngleVise) * eyeDist;
            SolidBrush bEyes = CacheRenduGDI.ObtenirBrush(Color.Black);
            g.FillEllipse(bEyes, eyeX - 3.5f, eyeY - 1.5f, 3f, 3f);
            g.FillEllipse(bEyes, eyeX + 0.5f, eyeY - 1.5f, 3f, 3f);
            
            // Reflet oeil (éclat de vie)
            SolidBrush bPupille = CacheRenduGDI.ObtenirBrush(Color.White);
            g.FillEllipse(bPupille, eyeX - 2.5f, eyeY - 1f, 1f, 1f);
            g.FillEllipse(bPupille, eyeX + 1.5f, eyeY - 1f, 1f, 1f);

            // Arc d'attaque de mêlée dynamique hyper stylisé
            if (AnimationAttaque > 0f)
            {
                float progressionBrute = Math.Clamp(1f - (AnimationAttaque / AnimationDuree), 0f, 1f);
                float slashProg = progressionBrute * progressionBrute * (3f - 2f * progressionBrute);
                float angleActuelArme = AngleVise - 1.2f + (slashProg * 2.4f);

                // Wedge (croissant) de balayage pour l'arme
                GraphicsState etatSlash = g.Save();
                g.TranslateTransform(p.X, p.Y);

                float startAngleDeg = (AngleVise - 1.2f) * 180f / MathF.PI;
                float sweepDeg = Math.Max(5f, slashProg * 140f);

                int alphaSlash = (int)(MathF.Sin(progressionBrute * MathF.PI) * 220f);
                if (alphaSlash > 0)
                {
                    SolidBrush bLameExt = CacheRenduGDI.ObtenirBrushAlpha(alphaSlash, couleurClasse);
                    SolidBrush bLameInt = CacheRenduGDI.ObtenirBrushAlpha((int)(alphaSlash * 1.2f > 255 ? 255 : alphaSlash * 1.2f), Color.White);

                    // Tracé du balayage (camembert)
                    g.FillPie(bLameExt, -65, -65, 130, 130, startAngleDeg, sweepDeg);
                    g.FillPie(bLameInt, -50, -50, 100, 100, startAngleDeg, sweepDeg);

                    // Ligne de bord de coupe tranchante
                    Pen penCoupe = CacheRenduGDI.ObtenirPen(Color.FromArgb(alphaSlash, Color.White), 2f);
                    g.DrawArc(penCoupe, -65, -65, 130, 130, startAngleDeg, sweepDeg);
                }

                // Arme elle-même
                float armeLx = MathF.Cos(angleActuelArme) * 45f;
                float armeLy = MathF.Sin(angleActuelArme) * 45f;
                float gardeLx = MathF.Cos(angleActuelArme) * 15f;
                float gardeLy = MathF.Sin(angleActuelArme) * 15f;

                Pen pArme = CacheRenduGDI.ObtenirPen(Color.White, 4f);
                g.DrawLine(pArme, gardeLx, gardeLy, armeLx, armeLy);

                // Garde croisée et effet lumineux sur l'arme
                Pen penGarde = CacheRenduGDI.ObtenirPen(Color.Gold, 3f);
                Vector2 dirEpee = new Vector2(armeLx - gardeLx, armeLy - gardeLy).Normaliser();
                Vector2 orthoEpee = new Vector2(-dirEpee.Y, dirEpee.X);
                g.DrawLine(penGarde, gardeLx - orthoEpee.X * 12f, gardeLy - orthoEpee.Y * 12f, gardeLx + orthoEpee.X * 12f, gardeLy + orthoEpee.Y * 12f);
                
                g.Restore(etatSlash);
            }
            else
            {
                // Arme au repos détaillée
                float armeLx = MathF.Cos(AngleVise) * 35f;
                float armeLy = MathF.Sin(AngleVise) * 35f;
                float gardeLx = MathF.Cos(AngleVise) * 12f;
                float gardeLy = MathF.Sin(AngleVise) * 12f;

                Pen pArme = CacheRenduGDI.ObtenirPen(Color.LightGray, 3f);
                g.DrawLine(pArme, p.X + gardeLx, p.Y + gardeLy, p.X + armeLx, p.Y + armeLy);

                Pen penGarde = CacheRenduGDI.ObtenirPen(Color.Gold, 2.5f);
                Vector2 dirEpee = new Vector2(armeLx - gardeLx, armeLy - gardeLy).Normaliser();
                Vector2 orthoEpee = new Vector2(-dirEpee.Y, dirEpee.X);
                g.DrawLine(penGarde, p.X + gardeLx - orthoEpee.X * 10f, p.Y + gardeLy - orthoEpee.Y * 10f, p.X + gardeLx + orthoEpee.X * 10f, p.Y + gardeLy + orthoEpee.Y * 10f);
            }

            // Mini barre de vie au-dessus de la tête
            int barreLarg = 44;
            int barreHaut = 5;
            int bx = p.X - barreLarg / 2;
            int by = p.Y - (int)Rayon - 14;

            float ratioPV = Math.Clamp((float)ModeleHero.PVActuels / ModeleHero.PVMaxTotal, 0f, 1f);
            SolidBrush bFond = CacheRenduGDI.ObtenirBrushAlpha(180, Color.FromArgb(20, 20, 26));
            SolidBrush bVieBack = CacheRenduGDI.ObtenirBrush(Color.FromArgb(160, 40, 40));
            SolidBrush bVie = CacheRenduGDI.ObtenirBrush(Color.FromArgb(46, 204, 113));

            g.FillRectangle(bFond, bx - 1, by - 1, barreLarg + 2, barreHaut + 2);
            g.FillRectangle(bVieBack, bx, by, barreLarg, barreHaut);
            g.FillRectangle(bVie, bx, by, barreLarg * ratioPV, barreHaut);
            g.DrawRectangle(Pens.Black, bx - 1, by - 1, barreLarg + 2, barreHaut + 2);

            // Effets visuels et badges de statuts actifs
            if (TempsEtourdi > 0f)
            {
                float starAngle = TempsAnimation * 7f;
                for (int s = 0; s < 3; s++)
                {
                    float sa = starAngle + s * MathF.Tau / 3f;
                    float sx = p.X + MathF.Cos(sa) * 14f;
                    float sy = by - 8f + MathF.Sin(sa) * 4f;
                    g.FillEllipse(Brushes.Gold, sx - 2.5f, sy - 2.5f, 5f, 5f);
                }
            }

            if (EstSousEffetStatut)
            {
                string badges = (TempsEtourdi > 0f ? "💫 " : "") +
                                (TempsBrulure > 0f ? "🔥 " : "") +
                                (TempsPoison > 0f ? "🧪 " : "") +
                                (TempsGel > 0f ? "❄️ " : "");
                g.DrawString(badges.Trim(), CacheRenduGDI.FontMini, Brushes.White, bx - 2, by - 14);
            }
        }
    }

    // ==============================================================
    // ENNEMI 2D (MONSTRE NORMAL OU BOSS ÉPIQUE)
    // ==============================================================
    public enum ArchetypeBoss
    {
        FeuDraconique,
        GlaceBlizzard,
        OmbreNecrotique,
        TitanSismique,
        CosmiqueVide
    }

    public class Monstre2D
    {
        private static readonly Random RngBoss = new();

        public Monstre ModeleMonstre { get; }
        public Vector2 Position;
        public Vector2 Velocite;
        public float Rayon;
        public float Vitesse;
        public float VitesseBase { get; private set; }
        private int attaqueBase;
        public bool EstBoss;
        public ArchetypeBoss Archetype;
        public Color CouleurElement;
        private bool estRapide;
        private bool estLourd;
        private float cooldownTir;
        public bool EstTireur { get; private set; }
        public bool EstLourd => estLourd;
        public bool EstRapide => estRapide;
        public float TempsTirTelegraph { get; private set; }
        public Vector2 DirectionTir { get; private set; }

        // IA & Attaque
        public float CooldownAttaque = 0f;
        public float TempsWindup = 0f; // Télégraphe rouge avant frappe
        public float TempsWindupMax = 0.28f;
        public bool EstEnWindup => TempsWindup > 0f;
        public float CooldownSortBoss = 1.0f; // Les boss attaquent dès le début !
        public int IndexRotationSort = 0;
        public bool EstEnrage = false;
        public int PhaseBossActuelle { get; private set; } = 0;

        // Ruée / Charge télégraphiée
        public bool EstEnPreparationCharge = false;
        public float TempsPreparationCharge = 0f;
        public bool EstEnCharge = false;
        public float TempsCharge = 0f;
        public Vector2 DirectionCharge;

        // Statuts 2D (Effets d'état persistants)
        public float TempsPoison = 0f;
        public float TempsBrulure = 0f;
        public float TempsGel = 0f;
        public float TempsEtourdi = 0f;
        public int DegatsPoisonTick = 0;
        public int DegatsBrulureTick = 0;
        private float tickChronoPoison = 0f;
        private float tickChronoBrulure = 0f;

        public bool EstSousEffetStatut => TempsPoison > 0f || TempsBrulure > 0f || TempsGel > 0f || TempsEtourdi > 0f;

        public void AppliquerStatut(string type, float duree, int degatsTick = 0)
        {
            if (EstMort) return;
            switch (type.ToLowerInvariant())
            {
                case "poison":
                    TempsPoison = Math.Max(TempsPoison, duree);
                    DegatsPoisonTick = Math.Max(DegatsPoisonTick, degatsTick);
                    break;
                case "brulure":
                case "feu":
                    TempsBrulure = Math.Max(TempsBrulure, duree);
                    DegatsBrulureTick = Math.Max(DegatsBrulureTick, degatsTick);
                    break;
                case "gel":
                case "givre":
                    TempsGel = Math.Max(TempsGel, duree);
                    break;
                case "etourdi":
                case "vertige":
                    TempsEtourdi = Math.Max(TempsEtourdi, EstBoss ? duree * 0.5f : duree);
                    break;
            }
        }

        // Animations & Effets
        public float TempsVie = 0f;
        public float TempsFlashDegats = 0f;
        public float AnimationMarche = 0f;
        private static readonly Font FontNomBoss = new Font("Segoe UI", 8.2f, FontStyle.Bold);

        public bool EstMort => ModeleMonstre.PVActuels <= 0;

        public Monstre2D(Monstre monstre, Vector2 positionInitiale)
        {
            ModeleMonstre = monstre;
            Position = positionInitiale;
            EstBoss = monstre.EstBoss;
            string nomNormalise = monstre.Nom.ToLowerInvariant();
            EstTireur = !EstBoss && (nomNormalise.Contains("archer") || nomNormalise.Contains("tireur") ||
                nomNormalise.Contains("chaman") || nomNormalise.Contains("sorcier") || nomNormalise.Contains("mage") ||
                nomNormalise.Contains("arbalétrier") || nomNormalise.Contains("arbaletrier"));
            estRapide = !EstBoss && (nomNormalise.Contains("loup") || nomNormalise.Contains("sanglier") || nomNormalise.Contains("assassin") || nomNormalise.Contains("gobelin"));
            estLourd = !EstBoss && (nomNormalise.Contains("golem") || nomNormalise.Contains("titan") || nomNormalise.Contains("ogre") || nomNormalise.Contains("géant") || nomNormalise.Contains("geant"));
            Rayon = EstBoss ? 35f : estLourd ? 23f : 16f;
            Vitesse = EstBoss ? 170f : EstTireur ? 112f : estRapide ? 195f : estLourd ? 92f : 135f;
            TempsWindupMax = EstBoss ? 0.26f : estLourd ? 0.52f : estRapide ? 0.22f : 0.35f;
            cooldownTir = 0.8f + (RngBoss.NextSingle() * 0.8f);
            TempsVie = RngBoss.NextSingle() * 10f;

            // Détection de l'archétype élémentaire et de la couleur du boss
            Archetype = ObtenirArchetype(monstre.Nom);
            CouleurElement = ObtenirCouleurElement(Archetype);

            // Ajustement automatique des PV pour le temps réel 2D (les boss ne doivent pas mourir en 2 clics)
            if (EstBoss)
            {
                if (ModeleMonstre.PVMax < 2200)
                {
                    int nouvPV = Math.Max(2600, (int)(ModeleMonstre.PVMax * 4.5f));
                    ModeleMonstre.PVMax = nouvPV;
                    ModeleMonstre.PVActuels = nouvPV;
                }
                if (ModeleMonstre.Attaque < 25)
                {
                    ModeleMonstre.Attaque = Math.Max(28, (int)(ModeleMonstre.Attaque * 1.35f));
                }
            }

            VitesseBase = Vitesse;
            attaqueBase = ModeleMonstre.Attaque;
        }

        private static ArchetypeBoss ObtenirArchetype(string nom)
        {
            string n = nom.ToLowerInvariant();
            if (n.Contains("dragon") || n.Contains("ignis") || n.Contains("magmarion") || n.Contains("belial") || n.Contains("flamme") || n.Contains("lave") || n.Contains("drake") || n.Contains("volcan") || n.Contains("braise"))
                return ArchetypeBoss.FeuDraconique;
            if (n.Contains("skuldir") || n.Contains("kaelas") || n.Contains("givre") || n.Contains("blizzard") || n.Contains("cryo") || n.Contains("glace") || n.Contains("permafrost") || n.Contains("arctique"))
                return ArchetypeBoss.GlaceBlizzard;
            if (n.Contains("malakor") || n.Contains("mor'gath") || n.Contains("morgath") || n.Contains("zulgar") || n.Contains("azkalith") || n.Contains("kryll") || n.Contains("vespera") || n.Contains("balthazar") || n.Contains("nécrose") || n.Contains("nécro") || n.Contains("poison") || n.Contains("peste") || n.Contains("sang") || n.Contains("spectre") || n.Contains("sépulcrale") || n.Contains("mort"))
                return ArchetypeBoss.OmbreNecrotique;
            if (n.Contains("xanthos") || n.Contains("néant") || n.Contains("neant") || n.Contains("cosmique") || n.Contains("chaos") || n.Contains("deus") || n.Contains("chronos") || n.Contains("zephyros") || n.Contains("abaddon") || n.Contains("stellaire") || n.Contains("lune") || n.Contains("archonte") || n.Contains("kraken") || n.Contains("astral") || n.Contains("écho") || n.Contains("galaxie") || n.Contains("sentinelle") || n.Contains("ombre du vide"))
                return ArchetypeBoss.CosmiqueVide;
            return ArchetypeBoss.TitanSismique;
        }

        private static Color ObtenirCouleurElement(ArchetypeBoss arch) => arch switch
        {
            ArchetypeBoss.FeuDraconique => Color.FromArgb(231, 76, 60),      // Rouge-flamme
            ArchetypeBoss.GlaceBlizzard => Color.FromArgb(52, 152, 219),     // Cyan azur
            ArchetypeBoss.OmbreNecrotique => Color.FromArgb(142, 68, 173),   // Violet sombre
            ArchetypeBoss.TitanSismique => Color.FromArgb(211, 84, 0),       // Ambre tellurique
            ArchetypeBoss.CosmiqueVide => Color.FromArgb(155, 89, 182),      // Pourpre cosmique
            _ => Color.FromArgb(231, 76, 60)
        };

        public int ObtenirPhaseBoss(float ratioPv)
        {
            if (!EstBoss) return 0;
            if (ratioPv <= 0.15f) return 3;
            if (ratioPv <= 0.40f) return 2;
            if (ratioPv <= 0.70f) return 1;
            return 0;
        }

        private void ActualiserStatistiquesPhaseBoss()
        {
            if (!EstBoss) return;

            float multiplicateurVitesse = PhaseBossActuelle switch
            {
                1 => 1.15f,
                2 => 1.32f,
                3 => 1.5f,
                _ => 1f
            };
            float multiplicateurAttaque = PhaseBossActuelle switch
            {
                1 => 1.12f,
                2 => 1.25f,
                3 => 1.4f,
                _ => 1f
            };

            if (EstEnrage)
            {
                multiplicateurVitesse *= 1.15f;
                multiplicateurAttaque *= 1.12f;
            }

            Vitesse = VitesseBase * multiplicateurVitesse;
            ModeleMonstre.Attaque = Math.Max(1, (int)MathF.Round(attaqueBase * multiplicateurAttaque));
        }

        public string ObtenirNomPhaseBoss()
        {
            return PhaseBossActuelle switch
            {
                0 => "Phase 1 : Martiale",
                1 => "Phase 2 : Furie",
                2 => "Phase 3 : Désolation",
                3 => "Phase 4 : Annihilation",
                _ => "Phase 1 : Martiale"
            };
        }

        public void MettreAJour(float dt, Joueur2D joueur, Monde2D monde, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            if (EstMort) return;

            TempsVie += dt;
            if (TempsFlashDegats > 0f) TempsFlashDegats -= dt;

            // Statuts DoT et Afflictions
            if (TempsPoison > 0f)
            {
                TempsPoison -= dt;
                tickChronoPoison += dt;
                if (tickChronoPoison >= 0.5f)
                {
                    tickChronoPoison = 0f;
                    int deg = Math.Max(1, DegatsPoisonTick);
                    ModeleMonstre.PVActuels = Math.Max(0, ModeleMonstre.PVActuels - deg);
                    TempsFlashDegats = 0.12f;
                    particules.EmettreEclats(Position, Color.LimeGreen, 3, 25f, 2f, 0.2f);
                    ajouterTexteFlottant($"🧪 -{deg}", Color.FromArgb(46, 204, 113), false);
                }
            }

            if (TempsBrulure > 0f)
            {
                TempsBrulure -= dt;
                tickChronoBrulure += dt;
                if (tickChronoBrulure >= 0.5f)
                {
                    tickChronoBrulure = 0f;
                    int deg = Math.Max(1, DegatsBrulureTick);
                    ModeleMonstre.PVActuels = Math.Max(0, ModeleMonstre.PVActuels - deg);
                    TempsFlashDegats = 0.12f;
                    particules.EmettreEclats(Position, Color.OrangeRed, 4, 30f, 2f, 0.2f);
                    ajouterTexteFlottant($"🔥 -{deg}", Color.FromArgb(230, 126, 34), false);
                }
            }

            if (TempsGel > 0f)
            {
                TempsGel -= dt;
            }

            if (TempsEtourdi > 0f)
            {
                TempsEtourdi -= dt;
                Velocite = Vector2.Zero;
                return; // Ennemi étourdi, incapable d'agir
            }

            if (EstBoss)
            {
                float ratioPv = (float)ModeleMonstre.PVActuels / Math.Max(1, ModeleMonstre.PVMax);
                int nouvellePhase = ObtenirPhaseBoss(ratioPv);
                if (nouvellePhase != PhaseBossActuelle)
                {
                    PhaseBossActuelle = nouvellePhase;
                    ActualiserStatistiquesPhaseBoss();
                    cam.DeclencherSecousse(14f + PhaseBossActuelle * 4f, 0.45f);
                    particules.EmettreAnneauExplosion(Position, Color.Red, 35 + PhaseBossActuelle * 10, 260f + PhaseBossActuelle * 30f);
                    ajouterTexteFlottant($"⚠️ {ModeleMonstre.Nom} : {ObtenirNomPhaseBoss()} !", Color.Gold, true);
                    AudioSynthetiseur.SonCriBoss();
                }

                if (!EstEnrage && ratioPv < 0.40f)
                {
                    EstEnrage = true;
                    ActualiserStatistiquesPhaseBoss();
                    cam.DeclencherSecousse(16f, 0.55f);
                    particules.EmettreAnneauExplosion(Position, Color.Red, 45, 320f);
                    particules.EmettreOndeDeChoc(Position, Color.Red, 90f, 0.4f);
                    ajouterTexteFlottant($"🔥 {ModeleMonstre.Nom} ENRAGÉ !", Color.Red, true);
                    AudioSynthetiseur.SonCriBoss();
                }
            }

            if (CooldownAttaque > 0f) CooldownAttaque -= dt;
            if (CooldownSortBoss > 0f) CooldownSortBoss -= dt;
            if (cooldownTir > 0f) cooldownTir -= dt;

            float dist = Vector2.Distance(Position, joueur.Position);
            Vector2 versJoueur = (joueur.Position - Position).Normaliser();

            // 1. GESTION DE LA CHARGE / RUÉE BRUTALE DU BOSS
            if (EstEnPreparationCharge)
            {
                TempsPreparationCharge -= dt;
                Velocite = Vector2.Zero;
                particules.EmettreTrailProjectile(Position, CouleurElement, Rayon * 0.75f);
                if (TempsPreparationCharge <= 0f)
                {
                    EstEnPreparationCharge = false;
                    EstEnCharge = true;
                    TempsCharge = 0.48f;
                    AudioSynthetiseur.SonCriBoss();
                    cam.DeclencherSecousse(10f, 0.25f);
                }
                return;
            }

            if (EstEnCharge)
            {
                TempsCharge -= dt;
                float vitesseCharge = EstEnrage ? 680f : 580f;
                Velocite = DirectionCharge * vitesseCharge;
                Position = monde.ResoudreCollisions(Position, Velocite * dt, Rayon);
                particules.EmettreTrailProjectile(Position, CouleurElement, Rayon * 0.75f);

                // Collision pendant la ruée
                if (dist < Rayon + joueur.Rayon + 16f)
                {
                    int degatsCharge = (int)(ModeleMonstre.Attaque * 1.4f);
                    int degatsInfliges = joueur.SubirDegats(
                        degatsCharge,
                        "CHARGE",
                        particules,
                        cam,
                        ajouterTexteFlottant,
                        20f,
                        16);
                    if (degatsInfliges > 0)
                    {
                        particules.EmettreAnneauExplosion(joueur.Position, Color.Red, 25, 200f);
                        joueur.AppliquerStatut("Etourdi", 0.6f);
                    }
                    EstEnCharge = false;
                }

                if (TempsCharge <= 0f)
                {
                    EstEnCharge = false;
                    particules.EmettreOndeDeChoc(Position, CouleurElement, 60f, 0.25f);
                }
                return;
            }

            // 2. GESTION DU WINDUP (AVEC AVANCÉE VERS LE JOUEUR AU LIEU D'ÊTRE FIGÉ)
            if (EstEnWindup)
            {
                TempsWindup -= dt;
                float vitesseEff = TempsGel > 0f ? Vitesse * 0.60f : Vitesse;
                Velocite = versJoueur * (vitesseEff * 0.85f);
                Position = monde.ResoudreCollisions(Position, Velocite * dt, Rayon);

                if (TempsWindup <= 0f)
                {
                    // Zone de frappe de mêlée généreuse (balayage d'attaque)
                    float porteeFrappe = Rayon + joueur.Rayon + 32f;
                    if (dist < porteeFrappe)
                    {
                        int degatsInfliges = joueur.SubirDegats(
                            ModeleMonstre.Attaque,
                            EstBoss ? "FRAPPE DU BOSS" : "COUP ENNEMI",
                            particules,
                            cam,
                            ajouterTexteFlottant,
                            EstBoss ? 15f : 6f,
                            8);
                        if (degatsInfliges > 0)
                            particules.EmettreOndeDeChoc(Position, CouleurElement, 45f, 0.2f);
                    }
                    CooldownAttaque = EstBoss ? (EstEnrage ? 0.45f : 0.65f) : 1.1f;
                }
                return;
            }

            if (EstTireur && !EstBoss)
            {
                if (TempsTirTelegraph > 0f)
                {
                    TempsTirTelegraph -= dt;
                    Velocite = Vector2.Zero;
                    if (TempsTirTelegraph <= 0f)
                    {
                        TempsTirTelegraph = 0f;
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + DirectionTir * (Rayon + 8f),
                            Velocite = DirectionTir * 390f,
                            Rayon = 6f,
                            Degats = Math.Max(4, ModeleMonstre.Attaque / 2 - joueur.ModeleHero.DefenseTotale / 3),
                            EstDuJoueur = false,
                            Couleur = CouleurElement,
                            TempsVie = 2.4f,
                            NomSort = "Tir de " + ModeleMonstre.Nom
                        });
                        particules.EmettreEclats(Position + DirectionTir * Rayon, CouleurElement, 7, 85f, 2.5f, 0.2f);
                        AudioSynthetiseur.SonTirMagique();
                        cooldownTir = 1.9f + RngBoss.NextSingle() * 0.7f;
                    }
                    return;
                }

                float distanceMin = 245f;
                float distanceConfort = 390f;
                if (dist > distanceConfort)
                {
                    Velocite = versJoueur * Vitesse;
                    Position = monde.ResoudreCollisions(Position, Velocite * dt, Rayon);
                    AnimationMarche += dt * 8f;
                }
                else if (dist < distanceMin)
                {
                    Velocite = versJoueur * -Vitesse;
                    Position = monde.ResoudreCollisions(Position, Velocite * dt, Rayon);
                    AnimationMarche += dt * 6f;
                }
                else
                {
                    Velocite = Vector2.Zero;
                }

                if (cooldownTir <= 0f && dist < 850f)
                {
                    DirectionTir = versJoueur;
                    TempsTirTelegraph = 0.62f;
                }
                return;
            }

            // 3. DÉPLACEMENT & IA
            float porteeContact = Rayon + joueur.Rayon + 14f;
            if (dist > porteeContact)
            {
                AnimationMarche += dt * 10f;
                Vector2 directionApproche = versJoueur;
                if (estRapide)
                {
                    Vector2 perpendiculaire = new Vector2(-versJoueur.Y, versJoueur.X);
                    directionApproche = (versJoueur + perpendiculaire * MathF.Sin(TempsVie * 4.5f) * 0.28f).Normaliser();
                }
                Velocite = directionApproche * Vitesse;
                Position = monde.ResoudreCollisions(Position, Velocite * dt, Rayon);
            }
            else
            {
                Velocite = versJoueur * (Vitesse * 0.25f);
                if (CooldownAttaque <= 0f)
                {
                    TempsWindupMax = EstBoss ? 0.26f : 0.35f;
                    TempsWindup = TempsWindupMax;
                }
            }

            // 4. LANCEMENT FRÉQUENT DE SORTS PUISSANTS POUR LES BOSS
            if (EstBoss && CooldownSortBoss <= 0f && dist < 1200f)
            {
                ExecuterSortBoss(versJoueur, dist, joueur, monde, particules, cam, ajouterTexteFlottant);
            }
        }

        private void ExecuterSortBoss(Vector2 versJoueur, float dist, Joueur2D joueur, Monde2D monde, GestionnaireParticules particules, Camera2D cam, Action<string, Color, bool> ajouterTexteFlottant)
        {
            // Réinitialisation du cooldown : cadence d'attaque dynamique et agressive !
            Random rng = RngBoss;
            CooldownSortBoss = (EstEnrage || PhaseBossActuelle >= 1) ? (0.8f + rng.NextSingle() * 0.35f) : (1.6f + rng.NextSingle() * 0.7f);

            bool phaseFurie = EstEnrage || PhaseBossActuelle >= 1;
            int maxSorts = phaseFurie ? 7 : 5;
            int sortChoisi = IndexRotationSort % maxSorts;
            IndexRotationSort = (IndexRotationSort + 1) % maxSorts;

            float angleVersJoueur = MathF.Atan2(versJoueur.Y, versJoueur.X);

            switch (sortChoisi)
            {
                case 0: // SALVE DE PROJECTILES CIBLÉS
                    AudioSynthetiseur.SonTirMagique();
                    int nbTirs = (EstEnrage || PhaseBossActuelle >= 1) ? (PhaseBossActuelle >= 2 ? 8 : 6) : 4;
                    float ecart = (EstEnrage || PhaseBossActuelle >= 1) ? (PhaseBossActuelle >= 2 ? 0.52f : 0.36f) : 0.26f;
                    for (int i = 0; i < nbTirs; i++)
                    {
                        float ratio = nbTirs == 1 ? 0f : ((float)i / (nbTirs - 1) - 0.5f);
                        float angle = angleVersJoueur + (ratio * ecart);
                        Vector2 d = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + d * (Rayon + 8f),
                            Velocite = d * 520f,
                            Rayon = 8f,
                            Degats = Math.Max(12, (int)(ModeleMonstre.Attaque * 0.85f)),
                            EstDuJoueur = false,
                            Couleur = CouleurElement,
                            TempsVie = 2.4f
                        });
                    }
                    particules.EmettreEclats(Position, CouleurElement, 14, 150f, 3.5f, 0.25f);
                    string nomTir = Archetype switch
                    {
                        ArchetypeBoss.FeuDraconique => "🔥 Souffle Ardent !",
                        ArchetypeBoss.GlaceBlizzard => "❄️ Rafale de Givre !",
                        ArchetypeBoss.OmbreNecrotique => "💀 Essaim Maudit !",
                        ArchetypeBoss.CosmiqueVide => "🌌 Salve Stellaire !",
                        _ => "🪨 Mégalithes Fracassants !"
                    };
                    ajouterTexteFlottant(nomTir, CouleurElement, true);
                    break;

                case 1: // RUÉE / CHARGE D'ASSAUT BRUTALE AVEC COULOIR D'ALERTE TÉLÉGRAPHIÉ
                    EstEnPreparationCharge = true;
                    TempsPreparationCharge = EstEnrage ? 0.45f : 0.60f;
                    DirectionCharge = versJoueur;
                    AudioSynthetiseur.SonCriBoss();
                    cam.DeclencherSecousse(7f, 0.2f);
                    particules.EmettreOndeDeChoc(Position, CouleurElement, 50f, 0.25f);
                    string nomCharge = Archetype switch
                    {
                        ArchetypeBoss.FeuDraconique => "⚡ Ruée Infernale !",
                        ArchetypeBoss.GlaceBlizzard => "⚡ Glissade Boréale !",
                        ArchetypeBoss.OmbreNecrotique => "⚡ Saut de l'Ombre !",
                        ArchetypeBoss.CosmiqueVide => "⚡ Warp Stellaire !",
                        _ => "🐗 Charge Titanesque !"
                    };
                    monde.AjouterZoneDanger(new ZoneDanger2D
                    {
                        Position = Position,
                        Direction = DirectionCharge,
                        Forme = FormeZoneDanger.LigneCharge,
                        LongueurLigne = 480f,
                        LargeurLigne = Rayon * 2.2f,
                        TempsTotal = TempsPreparationCharge,
                        TempsRestant = TempsPreparationCharge,
                        Degats = (int)(ModeleMonstre.Attaque * 1.4f),
                        Couleur = CouleurElement,
                        NomSort = nomCharge
                    });
                    ajouterTexteFlottant(nomCharge, Color.Gold, true);
                    break;

                case 2: // ZONES D'IMPACT TÉLÉGRAPHIÉES AU SOL (AOE)
                    AudioSynthetiseur.SonCritique();
                    cam.DeclencherSecousse(8f, 0.2f);
                    int nbZones = (EstEnrage || PhaseBossActuelle >= 1) ? (PhaseBossActuelle >= 2 ? 5 : 4) : 3;
                    string nomImpact = Archetype switch
                    {
                        ArchetypeBoss.FeuDraconique => "Pluie de Météores",
                        ArchetypeBoss.GlaceBlizzard => "Piliers de Givre",
                        ArchetypeBoss.OmbreNecrotique => "Geysers Nécrotiques",
                        ArchetypeBoss.CosmiqueVide => "Failles du Néant",
                        _ => "Fracas Sismique"
                    };

                    // 1ère zone directement sur le joueur
                    monde.AjouterZoneDanger(new ZoneDanger2D
                    {
                        Position = joueur.Position,
                        Rayon = 75f,
                        TempsTotal = EstEnrage ? 0.65f : 0.85f,
                        TempsRestant = EstEnrage ? 0.65f : 0.85f,
                        Degats = Math.Max(18, (int)(ModeleMonstre.Attaque * 1.35f)),
                        Couleur = CouleurElement,
                        NomSort = nomImpact
                    });

                    // Autres zones disposées autour
                    for (int z = 1; z < nbZones; z++)
                    {
                        float offsetX = rng.Next(-170, 170);
                        float offsetY = rng.Next(-170, 170);
                        monde.AjouterZoneDanger(new ZoneDanger2D
                        {
                            Position = joueur.Position + new Vector2(offsetX, offsetY),
                            Rayon = 70f,
                            TempsTotal = EstEnrage ? 0.70f : 0.90f,
                            TempsRestant = EstEnrage ? 0.70f : 0.90f,
                            Degats = Math.Max(16, (int)(ModeleMonstre.Attaque * 1.25f)),
                            Couleur = CouleurElement,
                            NomSort = nomImpact
                        });
                    }

                    particules.EmettreAnneauExplosion(Position, CouleurElement, 22, 180f);
                    ajouterTexteFlottant($"⚠️ {nomImpact} !", CouleurElement, true);
                    break;

                case 3: // NOVA CIRCULAIRE / BULLET HELL DÉFERLANT
                    AudioSynthetiseur.SonCritique();
                    cam.DeclencherSecousse(10f, 0.3f);
                    int nbBalles = (EstEnrage || PhaseBossActuelle >= 1) ? (PhaseBossActuelle >= 2 ? 24 : 18) : 14;
                    for (int i = 0; i < nbBalles; i++)
                    {
                        float angle = (i / (float)nbBalles) * MathF.PI * 2f;
                        Vector2 d = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + d * (Rayon + 6f),
                            Velocite = d * 390f,
                            Rayon = 7.5f,
                            Degats = Math.Max(10, (int)(ModeleMonstre.Attaque * 0.75f)),
                            EstDuJoueur = false,
                            Couleur = CouleurElement,
                            TempsVie = 2.5f
                        });
                    }
                    particules.EmettreAnneauExplosion(Position, CouleurElement, 32, 240f);
                    string nomNova = Archetype switch
                    {
                        ArchetypeBoss.FeuDraconique => "🌋 Brasier de l'Apocalypse !",
                        ArchetypeBoss.GlaceBlizzard => "❄️ Tempête Zéro Absolu !",
                        ArchetypeBoss.OmbreNecrotique => "☠️ Moisson d'Âmes !",
                        ArchetypeBoss.CosmiqueVide => "👑 SUPERNOVA DU CHAOS !",
                        _ => "🔨 Séisme Primordial !"
                    };
                    ajouterTexteFlottant(nomNova, CouleurElement, true);
                    break;

                case 4: // MUR DE PROJECTILES (RIDEAU)
                    AudioSynthetiseur.SonTirMagique();
                    int largeurRideau = EstEnrage ? 8 : 5;
                    Vector2 dirOrthogonale = new Vector2(-versJoueur.Y, versJoueur.X);
                    for (int i = -largeurRideau; i <= largeurRideau; i++)
                    {
                        Vector2 posDepart = Position + versJoueur * (Rayon + 10f) + dirOrthogonale * (i * 24f);
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = posDepart,
                            Velocite = versJoueur * 450f,
                            Rayon = 7.5f,
                            Degats = Math.Max(14, (int)(ModeleMonstre.Attaque * 0.90f)),
                            EstDuJoueur = false,
                            Couleur = CouleurElement,
                            TempsVie = 3.0f,
                            NomSort = "Rideau"
                        });
                    }
                    ajouterTexteFlottant("🌊 Mur Élémentaire !", CouleurElement, true);
                    break;

                case 5: // SPIRALE MAGIQUE DEFERLANTE
                    AudioSynthetiseur.SonCritique();
                    int nbSpirale = EstEnrage ? 30 : 20;
                    for (int i = 0; i < nbSpirale; i++)
                    {
                        float angle = angleVersJoueur + (i * 0.35f);
                        Vector2 d = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                        monde.AjouterProjectile(new Projectile2D
                        {
                            Position = Position + d * (Rayon + 5f),
                            Velocite = d * (200f + i * 12f), // Vitesses croissantes
                            Rayon = 7f,
                            Degats = Math.Max(10, (int)(ModeleMonstre.Attaque * 0.7f)),
                            EstDuJoueur = false,
                            Couleur = CouleurElement,
                            TempsVie = 4.0f,
                            NomSort = "Spirale"
                        });
                    }
                    particules.EmettreAnneauExplosion(Position, CouleurElement, 30, 250f);
                    ajouterTexteFlottant("🌀 Spirale du Désespoir !", CouleurElement, true);
                    break;

                case 6: // SOUFFLE ÉLÉMENTAIRE EN CÔNE (AOE TÉLÉGRAPHIÉE EN ÉVENTAIL)
                    AudioSynthetiseur.SonCritique();
                    cam.DeclencherSecousse(9f, 0.25f);
                    float dureeSouffle = EstEnrage ? 0.65f : 0.85f;
                    string nomSouffle = Archetype switch
                    {
                        ArchetypeBoss.FeuDraconique => "🔥 Souffle Ardent Primordial",
                        ArchetypeBoss.GlaceBlizzard => "❄️ Souffle Boréal Zéro Absolu",
                        ArchetypeBoss.OmbreNecrotique => "☠️ Miasmes Pestilentiels",
                        ArchetypeBoss.CosmiqueVide => "🌌 Déferlement Stellaire",
                        _ => "🪨 Souffle Tellurique Fracassant"
                    };
                    monde.AjouterZoneDanger(new ZoneDanger2D
                    {
                        Position = Position,
                        Direction = versJoueur,
                        Forme = FormeZoneDanger.ConeSouffle,
                        Rayon = 340f,
                        AngleOuverture = 70f,
                        TempsTotal = dureeSouffle,
                        TempsRestant = dureeSouffle,
                        Degats = Math.Max(20, (int)(ModeleMonstre.Attaque * 1.55f)),
                        Couleur = CouleurElement,
                        NomSort = nomSouffle
                    });
                    particules.EmettreAnneauExplosion(Position, CouleurElement, 24, 150f);
                    ajouterTexteFlottant($"⚠️ {nomSouffle} !", CouleurElement, true);
                    break;
            }
        }

        public void Dessiner(Graphics g, Camera2D cam)
        {
            if (EstMort) return;
            if (!cam.EstVisible(Position, Rayon * 3f)) return;

            Point p = cam.MondeVersEcran(Position);

            // Télégraphe de frappe de mêlée (cercle rouge d'avertissement)
            if (EstEnWindup)
            {
                float ratioWind = Math.Clamp(TempsWindup / TempsWindupMax, 0f, 1f);
                float rDanger = (Rayon + 32f) * cam.Zoom;

                SolidBrush bDanger = CacheRenduGDI.ObtenirBrushAlpha((int)((1f - ratioWind) * 120 + 40), Color.Red);
                g.FillEllipse(bDanger, p.X - rDanger, p.Y - rDanger, rDanger * 2f, rDanger * 2f);

                Pen pDanger = CacheRenduGDI.ObtenirPen(Color.Red, 2.5f);
                g.DrawEllipse(pDanger, p.X - rDanger, p.Y - rDanger, rDanger * 2f, rDanger * 2f);

                float rTiming = rDanger * ratioWind;
                Pen pTiming = CacheRenduGDI.ObtenirPen(Color.Gold, 2f);
                g.DrawEllipse(pTiming, p.X - rTiming, p.Y - rTiming, rTiming * 2f, rTiming * 2f);
            }

            // Halo dynamique de préparation et de ruée de charge
            if (EstEnPreparationCharge)
            {
                float pulsationCharge = 0.7f + 0.3f * MathF.Sin(TempsVie * 25f);
                SolidBrush bPrep = CacheRenduGDI.ObtenirBrushAlpha((int)(130 * pulsationCharge), Color.OrangeRed);
                g.FillEllipse(bPrep, p.X - Rayon * 1.6f, p.Y - Rayon * 1.6f, Rayon * 3.2f, Rayon * 3.2f);
                Pen pPrep = CacheRenduGDI.ObtenirPen(Color.Gold, 2.5f);
                g.DrawEllipse(pPrep, p.X - Rayon * 1.6f, p.Y - Rayon * 1.6f, Rayon * 3.2f, Rayon * 3.2f);
            }
            else if (EstEnCharge)
            {
                SolidBrush bCharge = CacheRenduGDI.ObtenirBrushAlpha(130, CouleurElement);
                g.FillEllipse(bCharge, p.X - Rayon * 1.5f, p.Y - Rayon * 1.5f, Rayon * 3f, Rayon * 3f);
            }

            if (EstTireur && TempsTirTelegraph > 0f)
            {
                float porteeVisuelle = 210f * cam.Zoom;
                float finX = p.X + DirectionTir.X * porteeVisuelle;
                float finY = p.Y + DirectionTir.Y * porteeVisuelle;
                int alphaTir = Math.Clamp((int)(155f + 90f * (1f - TempsTirTelegraph / 0.62f)), 0, 245);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alphaTir / 3, CouleurElement), 12f * cam.Zoom), p.X, p.Y, finX, finY);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(alphaTir, Color.FromArgb(255, 255, 235, 165)), 2f * cam.Zoom), p.X, p.Y, finX, finY);
                float marqueur = (5f + (1f - TempsTirTelegraph / 0.62f) * 5f) * cam.Zoom;
                g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(alphaTir, Color.Gold), 2f * cam.Zoom), finX - marqueur, finY - marqueur, marqueur * 2f, marqueur * 2f);
            }

            // Ombre au sol
            SolidBrush bOmbre = CacheRenduGDI.ObtenirBrushAlpha(85, Color.Black);
            g.FillEllipse(bOmbre, p.X - Rayon, p.Y + Rayon * 0.4f, Rayon * 2f, Rayon * 0.75f);
            if (estRapide && Velocite.LongueurCarree() > 100f)
            {
                Vector2 directionFuite = Velocite.Normaliser();
                Vector2 perpendiculaire = new Vector2(-directionFuite.Y, directionFuite.X);
                for (int i = 0; i < 3; i++)
                {
                    float decalage = (i - 1) * Rayon * 0.42f;
                    g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(90 - i * 18, baseColor: Color.White), 1.5f * cam.Zoom),
                        p.X - directionFuite.X * Rayon * (1.2f + i * 0.3f) + perpendiculaire.X * decalage,
                        p.Y - directionFuite.Y * Rayon * (1.2f + i * 0.3f) + perpendiculaire.Y * decalage,
                        p.X - directionFuite.X * Rayon * 0.45f + perpendiculaire.X * decalage,
                        p.Y - directionFuite.Y * Rayon * 0.45f + perpendiculaire.Y * decalage);
                }
            }
            if (EstBoss)
            {
                float pulsation = 0.5f + 0.5f * MathF.Sin(TempsVie * (EstEnrage ? 6f : 3f));
                Color couleurAura = EstEnrage ? Color.OrangeRed : CouleurElement;
                float rayonAura = Rayon * (1.6f + pulsation * 0.12f);
                g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(65 + (int)(55 * pulsation), couleurAura), 2f), p.X - rayonAura, p.Y + Rayon * 0.2f, rayonAura * 2f, rayonAura * 0.72f);
                g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(35 + (int)(40 * pulsation), Color.Gold), 1f), p.X - rayonAura * 0.78f, p.Y + Rayon * 0.3f, rayonAura * 1.56f, rayonAura * 0.52f);

                for (int i = 0; i < 3; i++)
                {
                    float angleOrbite = TempsVie * (EstEnrage ? 2.2f : 1.1f) + i * MathF.Tau / 3f;
                    float ox = p.X + MathF.Cos(angleOrbite) * Rayon * 1.55f;
                    float oy = p.Y - Rayon * 0.35f + MathF.Sin(angleOrbite) * Rayon * 0.42f;
                    float tailleOrbe = 2.2f + pulsation;
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha((int)(150 + pulsation * 90), couleurAura), ox - tailleOrbe, oy - tailleOrbe, tailleOrbe * 2f, tailleOrbe * 2f);
                }
            }

            // Respiration procédurale & squash
            float breathing = 1f + MathF.Sin(TempsVie * 3.5f) * 0.05f;
            float squashX = Velocite.LongueurCarree() > 10f ? (1f + MathF.Sin(AnimationMarche) * 0.07f) : 1f;
            float squashY = 2f - squashX;

            float rx = Rayon * breathing * squashX;
            float ry = Rayon * breathing * squashY;

            // Couleur selon le type de monstre ou élément du boss
            Color baseColor = EstBoss
                ? (EstEnrage ? Color.FromArgb(192, 57, 43) : CouleurElement)
                : Color.FromArgb(127, 140, 141);

            if (!EstBoss)
            {
                if (ModeleMonstre.Nom.Contains("Loup") || ModeleMonstre.Nom.Contains("Sanglier")) baseColor = Color.FromArgb(160, 100, 60);
                if (ModeleMonstre.Nom.Contains("Givre") || ModeleMonstre.Nom.Contains("Permafrost")) baseColor = Color.FromArgb(52, 152, 219);
                if (ModeleMonstre.Nom.Contains("Lave") || ModeleMonstre.Nom.Contains("Dragon") || ModeleMonstre.Nom.Contains("Drake")) baseColor = Color.FromArgb(230, 126, 34);
                if (ModeleMonstre.Nom.Contains("Squelette") || ModeleMonstre.Nom.Contains("Spectre")) baseColor = Color.FromArgb(189, 195, 199);
            }

            // Flash de dégâts blanc
            Color finalCorpsColor = TempsFlashDegats > 0f ? Color.White : baseColor;

            // --- Dessin Humanoïde Monstre / Boss (Cinématique Inverse Vectorielle) ---
            SolidBrush bCorps = CacheRenduGDI.ObtenirBrush(finalCorpsColor);
            Pen penContour = CacheRenduGDI.ObtenirPen(EstBoss ? (EstEnrage ? Color.OrangeRed : Color.Gold) : Color.FromArgb(30,30,30), EstBoss ? 2.5f : 1.5f);
            
            float angleJambeG = MathF.Cos(AnimationMarche) * 0.7f;
            float angleJambeD = -MathF.Cos(AnimationMarche) * 0.7f;
            float angleBrasG = MathF.Sin(AnimationMarche) * 0.7f;
            float angleBrasD = -MathF.Sin(AnimationMarche) * 0.7f;

            float largeurCorps = rx * 0.85f;
            float hauteurCorps = ry * 0.95f;
            float rayonTete = rx * 0.65f;
            float echelle = EstBoss ? 1.4f : 1f;

            Pen penJambe = CacheRenduGDI.ObtenirPenArrondi(Color.FromArgb(30, 30, 35), 8f * echelle);
            Pen penBotte = CacheRenduGDI.ObtenirPenArrondi(Color.FromArgb(15, 15, 20), 9f * echelle);
            Pen penBras = CacheRenduGDI.ObtenirPenArrondi(finalCorpsColor, 7f * echelle);

            float hancheY = p.Y + (hauteurCorps * 0.5f);
            float longueurMembre = hauteurCorps * 0.6f;

            // Jambes Gauche et Droite
            float hancheGx = p.X - (largeurCorps * 0.5f);
            float genouGx = hancheGx + MathF.Sin(angleJambeG) * longueurMembre; float genouGy = hancheY + MathF.Cos(angleJambeG) * longueurMembre;
            float piedGx = genouGx + MathF.Sin(angleJambeG + 0.3f) * longueurMembre * 0.8f; float piedGy = genouGy + MathF.Cos(angleJambeG + 0.3f) * longueurMembre * 0.8f;
            g.DrawLine(penJambe, hancheGx, hancheY, genouGx, genouGy); g.DrawLine(penBotte, genouGx, genouGy, piedGx, piedGy);

            float hancheDx = p.X + (largeurCorps * 0.5f);
            float genouDx = hancheDx + MathF.Sin(angleJambeD) * longueurMembre; float genouDy = hancheY + MathF.Cos(angleJambeD) * longueurMembre;
            float piedDx = genouDx + MathF.Sin(angleJambeD + 0.3f) * longueurMembre * 0.8f; float piedDy = genouDy + MathF.Cos(angleJambeD + 0.3f) * longueurMembre * 0.8f;
            g.DrawLine(penJambe, hancheDx, hancheY, genouDx, genouDy); g.DrawLine(penBotte, genouDx, genouDy, piedDx, piedDy);

            // Torse profilé et Plastron massif
            float wTorse = largeurCorps * 2.2f;
            float hTorse = hauteurCorps * 1.5f;
            g.FillPie(bCorps, p.X - wTorse/2, p.Y - hTorse/2, wTorse, wTorse, 180, 180);
            g.FillRectangle(bCorps, p.X - wTorse/2, p.Y, wTorse, hTorse/2);
            g.DrawArc(penContour, p.X - wTorse/2, p.Y - hTorse/2, wTorse, wTorse, 180, 180);
            g.DrawLine(penContour, p.X - wTorse/2, p.Y, p.X - wTorse/2, p.Y + hTorse/2);
            g.DrawLine(penContour, p.X + wTorse/2, p.Y, p.X + wTorse/2, p.Y + hTorse/2);
            
            SolidBrush bPlastron = CacheRenduGDI.ObtenirBrush(Color.FromArgb(80, 0, 0, 0));
            PointF[] plastron = {
                new PointF(p.X - wTorse * 0.4f, p.Y - hTorse * 0.4f),
                new PointF(p.X + wTorse * 0.4f, p.Y - hTorse * 0.4f),
                new PointF(p.X, p.Y + hTorse * 0.3f)
            };
            g.FillPolygon(bPlastron, plastron);

            if (estLourd)
            {
                float plaqueX = p.X - wTorse * 0.34f;
                float plaqueY = p.Y - hTorse * 0.14f;
                float plaqueW = wTorse * 0.68f;
                float plaqueH = hTorse * 0.34f;
                Color couleurPlaque = TempsFlashDegats > 0f ? Color.White : Color.FromArgb(100, 104, 112);
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(couleurPlaque), plaqueX, plaqueY, plaqueW, plaqueH);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(35, 38, 45), 2f), plaqueX, plaqueY, plaqueW, plaqueH);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(185, 196, 205), 1.5f), plaqueX + 4f, plaqueY + 3f, plaqueX + plaqueW - 4f, plaqueY + 3f);
                g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(218, 139, 59)), p.X - 3f, plaqueY + plaqueH * 0.45f, 6f, 6f);
            }

            // Épaulières
            if (EstBoss)
            {
                SolidBrush bEpauliere = CacheRenduGDI.ObtenirBrush(Color.FromArgb(50, 50, 55));
                float epR = 15f * echelle;
                g.FillPie(bEpauliere, p.X - wTorse/2 - epR, p.Y - hTorse/2 - epR*0.5f, epR*2, epR*2, 180, 180);
                g.FillPie(bEpauliere, p.X + wTorse/2 - epR, p.Y - hTorse/2 - epR*0.5f, epR*2, epR*2, 180, 180);
            }

            // Bras Gauche et Droit
            float epauleY = p.Y - hTorse * 0.2f;
            float epauleGx = p.X - wTorse/2;
            float coudeGx = epauleGx + MathF.Sin(angleBrasG) * longueurMembre; float coudeGy = epauleY + MathF.Cos(angleBrasG) * longueurMembre;
            float mainGx = coudeGx + MathF.Sin(angleBrasG) * longueurMembre; float mainGy = coudeGy + MathF.Cos(angleBrasG) * longueurMembre;
            g.DrawLine(penBras, epauleGx, epauleY, coudeGx, coudeGy); g.DrawLine(penBras, coudeGx, coudeGy, mainGx, mainGy);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(30, 30, 35)), mainGx - 5f*echelle, mainGy - 5f*echelle, 10f*echelle, 10f*echelle); // Poing

            float epauleDx = p.X + wTorse/2;
            float coudeDx = epauleDx + MathF.Sin(angleBrasD) * longueurMembre; float coudeDy = epauleY + MathF.Cos(angleBrasD) * longueurMembre;
            float mainDx = coudeDx + MathF.Sin(angleBrasD) * longueurMembre; float mainDy = coudeDy + MathF.Cos(angleBrasD) * longueurMembre;
            g.DrawLine(penBras, epauleDx, epauleY, coudeDx, coudeDy); g.DrawLine(penBras, coudeDx, coudeDy, mainDx, mainDy);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(30, 30, 35)), mainDx - 5f*echelle, mainDy - 5f*echelle, 10f*echelle, 10f*echelle); // Poing

            // Tête
            g.FillEllipse(bCorps, p.X - rayonTete, p.Y - hauteurCorps - rayonTete * 1.2f, rayonTete*2, rayonTete*2);
            g.DrawEllipse(penContour, p.X - rayonTete, p.Y - hauteurCorps - rayonTete * 1.2f, rayonTete*2, rayonTete*2);

            // Mâchoire / Masque sombre
            g.FillPie(CacheRenduGDI.ObtenirBrush(Color.FromArgb(30, 30, 35)), p.X - rayonTete, p.Y - hauteurCorps - rayonTete * 1.2f, rayonTete*2, rayonTete*2, 0, 180);

            // Cornes pour Boss
            if (EstBoss)
            {
                SolidBrush bCorne = CacheRenduGDI.ObtenirBrush(Color.FromArgb(40, 40, 40));
                // Corne gauche incurvée
                Point[] corneGauche = { 
                    new Point((int)(p.X - rayonTete + 5), (int)(p.Y - hauteurCorps - rayonTete * 0.5f)), 
                    new Point((int)(p.X - rayonTete - 15), (int)(p.Y - hauteurCorps - rayonTete * 1.8f)), 
                    new Point((int)(p.X - rayonTete - 5), (int)(p.Y - hauteurCorps - rayonTete * 2.5f)),
                    new Point((int)(p.X - 5), (int)(p.Y - hauteurCorps - rayonTete - 5)) 
                };
                g.FillPolygon(bCorne, corneGauche);
                g.DrawPolygon(penContour, corneGauche);
                
                // Corne droite incurvée
                Point[] corneDroite = { 
                    new Point((int)(p.X + rayonTete - 5), (int)(p.Y - hauteurCorps - rayonTete * 0.5f)), 
                    new Point((int)(p.X + rayonTete + 15), (int)(p.Y - hauteurCorps - rayonTete * 1.8f)), 
                    new Point((int)(p.X + rayonTete + 5), (int)(p.Y - hauteurCorps - rayonTete * 2.5f)),
                    new Point((int)(p.X + 5), (int)(p.Y - hauteurCorps - rayonTete - 5)) 
                };
                g.FillPolygon(bCorne, corneDroite);
                g.DrawPolygon(penContour, corneDroite);
            }

            // Orbes élémentaires orbitant autour du Boss
            if (EstBoss)
            {
                int nbOrbes = EstEnrage ? 5 : 3;
                for (int i = 0; i < nbOrbes; i++)
                {
                    float sens = Archetype == ArchetypeBoss.OmbreNecrotique ? -1f : 1f;
                    float vitesseOrbite = EstEnrage ? 2.8f : 1.65f;
                    float angleOrbe = TempsVie * vitesseOrbite * sens + i * (MathF.Tau / nbOrbes);
                    float pulsationOrbe = MathF.Sin(TempsVie * 4f + i * 1.7f);
                    float rayonOrbite = Rayon + 22f;
                    float facteurX = Archetype switch
                    {
                        ArchetypeBoss.FeuDraconique => 1.35f,
                        ArchetypeBoss.TitanSismique => 0.9f,
                        _ => 1f
                    };
                    float facteurY = Archetype switch
                    {
                        ArchetypeBoss.FeuDraconique => 0.48f,
                        ArchetypeBoss.GlaceBlizzard => 0.82f,
                        ArchetypeBoss.OmbreNecrotique => 1.18f,
                        ArchetypeBoss.TitanSismique => 0.62f,
                        _ => 1f + pulsationOrbe * 0.18f
                    };
                    float variationRayon = Archetype == ArchetypeBoss.CosmiqueVide ? 1f + pulsationOrbe * 0.16f : 1f;
                    float ox = p.X + MathF.Cos(angleOrbe) * rayonOrbite * facteurX * variationRayon;
                    float oy = p.Y + MathF.Sin(angleOrbe) * rayonOrbite * facteurY * variationRayon;
                    float tailleOrbe = 4.5f + (pulsationOrbe + 1f) * 1.5f;
                    Color couleurOrbe = EstEnrage ? Color.OrangeRed : CouleurElement;

                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(85, couleurOrbe), ox - tailleOrbe * 1.7f, oy - tailleOrbe * 1.7f, tailleOrbe * 3.4f, tailleOrbe * 3.4f);
                    switch (Archetype)
                    {
                        case ArchetypeBoss.GlaceBlizzard:
                            Pen penGivre = CacheRenduGDI.ObtenirPen(Color.FromArgb(220, Color.White), 2f);
                            g.DrawLine(penGivre, ox, oy - tailleOrbe, ox + tailleOrbe * 0.65f, oy);
                            g.DrawLine(penGivre, ox + tailleOrbe * 0.65f, oy, ox, oy + tailleOrbe);
                            g.DrawLine(penGivre, ox, oy + tailleOrbe, ox - tailleOrbe * 0.65f, oy);
                            g.DrawLine(penGivre, ox - tailleOrbe * 0.65f, oy, ox, oy - tailleOrbe);
                            break;
                        case ArchetypeBoss.OmbreNecrotique:
                            g.DrawEllipse(CacheRenduGDI.ObtenirPen(Color.FromArgb(210, couleurOrbe), 2f), ox - tailleOrbe, oy - tailleOrbe, tailleOrbe * 2f, tailleOrbe * 2f);
                            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(220, 24, 10, 35)), ox - tailleOrbe * 0.35f, oy - tailleOrbe * 0.35f, tailleOrbe * 0.7f, tailleOrbe * 0.7f);
                            break;
                        case ArchetypeBoss.TitanSismique:
                            g.FillRectangle(CacheRenduGDI.ObtenirBrush(couleurOrbe), ox - tailleOrbe * 0.65f, oy - tailleOrbe * 0.65f, tailleOrbe * 1.3f, tailleOrbe * 1.3f);
                            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(210, Color.White), 1.5f), ox - tailleOrbe * 0.4f, oy, ox + tailleOrbe * 0.4f, oy);
                            break;
                        case ArchetypeBoss.CosmiqueVide:
                            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(230, Color.White), 2f), ox - tailleOrbe, oy, ox + tailleOrbe, oy);
                            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(230, Color.White), 2f), ox, oy - tailleOrbe, ox, oy + tailleOrbe);
                            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), ox - 2f, oy - 2f, 4f, 4f);
                            break;
                        default:
                            g.FillEllipse(CacheRenduGDI.ObtenirBrush(couleurOrbe), ox - tailleOrbe, oy - tailleOrbe, tailleOrbe * 2f, tailleOrbe * 2f);
                            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(255, 255, 225, 170)), ox - tailleOrbe * 0.35f, oy - tailleOrbe * 0.35f, tailleOrbe * 0.7f, tailleOrbe * 0.7f);
                            break;
                    }
                }
            }

            // Yeux hostiles et brillants
            SolidBrush bYeux = CacheRenduGDI.ObtenirBrush(EstEnrage ? Color.Gold : (EstBoss ? Color.FromArgb(255, 230, 100) : Color.Red));
            float espacementYeux = rayonTete * 0.45f;
            float tailleOeil = EstBoss ? 7f : 5f;
            g.FillEllipse(bYeux, p.X - espacementYeux - tailleOeil/2, p.Y - hauteurCorps - rayonTete * 0.5f, tailleOeil, tailleOeil);
            g.FillEllipse(bYeux, p.X + espacementYeux - tailleOeil/2, p.Y - hauteurCorps - rayonTete * 0.5f, tailleOeil, tailleOeil);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), p.X - espacementYeux - 1, p.Y - hauteurCorps - rayonTete * 0.5f - 1, 2, 2);
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.White), p.X + espacementYeux - 1, p.Y - hauteurCorps - rayonTete * 0.5f - 1, 2, 2);

            // Barre de vie au-dessus
            int barreLarg = EstBoss ? 80 : 42;
            int barreHaut = EstBoss ? 7 : 4;
            int bx = p.X - barreLarg / 2;
            int by = p.Y - (int)Rayon - (EstBoss ? 20 : 12);

            float ratioPV = Math.Clamp((float)ModeleMonstre.PVActuels / ModeleMonstre.PVMax, 0f, 1f);
            SolidBrush bFondBarre = CacheRenduGDI.ObtenirBrushAlpha(180, Color.FromArgb(20, 20, 26));
            SolidBrush bVieBack = CacheRenduGDI.ObtenirBrush(Color.FromArgb(160, 30, 30));
            SolidBrush bVie = CacheRenduGDI.ObtenirBrush(EstBoss ? Color.FromArgb(231, 76, 60) : Color.FromArgb(241, 196, 15));

            g.FillRectangle(bFondBarre, bx - 1, by - 1, barreLarg + 2, barreHaut + 2);
            g.FillRectangle(bVieBack, bx, by, barreLarg, barreHaut);
            g.FillRectangle(bVie, bx, by, barreLarg * ratioPV, barreHaut);
            g.DrawRectangle(Pens.Black, bx - 1, by - 1, barreLarg + 2, barreHaut + 2);

            // Effets de statuts actifs sur le monstre
            if (TempsEtourdi > 0f)
            {
                float starAngle = TempsVie * 7f;
                for (int s = 0; s < 3; s++)
                {
                    float sa = starAngle + s * MathF.Tau / 3f;
                    float sx = p.X + MathF.Cos(sa) * (Rayon * 0.75f);
                    float sy = by - 8f + MathF.Sin(sa) * 4f;
                    g.FillEllipse(Brushes.Gold, sx - 2.5f, sy - 2.5f, 5f, 5f);
                }
            }

            if (EstSousEffetStatut)
            {
                string badges = (TempsEtourdi > 0f ? "💫 " : "") +
                                (TempsBrulure > 0f ? "🔥 " : "") +
                                (TempsPoison > 0f ? "🧪 " : "") +
                                (TempsGel > 0f ? "❄️ " : "");
                g.DrawString(badges.Trim(), CacheRenduGDI.FontMini, Brushes.White, bx - 2, by - (EstBoss ? 26 : 14));
            }

            // Nom du Boss
            if (EstBoss)
            {
                string infoBoss = $"{ModeleMonstre.Nom} ({ModeleMonstre.PVActuels}/{ModeleMonstre.PVMax})";
                SizeF sz = g.MeasureString(infoBoss, FontNomBoss);
                g.DrawString(infoBoss, FontNomBoss, Brushes.Black, p.X - sz.Width / 2f + 1, by - 16);
                g.DrawString(infoBoss, FontNomBoss, Brushes.Gold, p.X - sz.Width / 2f, by - 17);
            }
        }
    }
}
