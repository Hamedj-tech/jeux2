using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NAudio.Wave;

namespace JeuxRPG
{
    public static class JournalErreurs
    {
        private static readonly object Verrou = new object();

        public static void Enregistrer(Exception erreur, string contexte)
        {
            try
            {
                string repertoire = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Aethelgard", "Sauvegardes2D");
                Directory.CreateDirectory(repertoire);
                string entree = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {contexte}{Environment.NewLine}{erreur}{Environment.NewLine}{Environment.NewLine}";
                lock (Verrou)
                    File.AppendAllText(Path.Combine(repertoire, "log.txt"), entree);
            }
            catch (Exception erreurJournal)
            {
                Debug.WriteLine($"Journalisation impossible ({contexte}) : {erreurJournal}\nErreur initiale : {erreur}");
            }
        }
    }

    // ==============================================================
    // VECTEUR 2D LÉGER ET ROBUSTE
    // ==============================================================
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0, 0);

        public float Longueur() => MathF.Sqrt(X * X + Y * Y);
        public float LongueurCarree() => X * X + Y * Y;

        public Vector2 Normaliser()
        {
            float len = Longueur();
            return len > 0.0001f ? new Vector2(X / len, Y / len) : Zero;
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public static float DistanceCarree(Vector2 a, Vector2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float scalar) => new Vector2(a.X * scalar, a.Y * scalar);
        public static Vector2 operator /(Vector2 a, float scalar) => scalar != 0 ? new Vector2(a.X / scalar, a.Y / scalar) : Zero;
    }

    // ==============================================================
    // CACHE GDI+ ULTRA RAPIDE (ZERO ALLOCATION, ZERO GC STUTTER)
    // ==============================================================
    public static class CacheRenduGDI
    {
        private sealed class EntreeBrush
        {
            public SolidBrush Brush { get; }
            public int DerniereImage { get; set; }
            public EntreeBrush(SolidBrush brush, int derniereImage) { Brush = brush; DerniereImage = derniereImage; }
        }

        private sealed class EntreePen
        {
            public Pen Pen { get; }
            public int DerniereImage { get; set; }
            public EntreePen(Pen pen, int derniereImage) { Pen = pen; DerniereImage = derniereImage; }
        }

        private const int LimiteBrushes = 1024;
        private const int LimitePens = 1536;
        private static readonly Dictionary<int, EntreeBrush> cacheBrushes = new Dictionary<int, EntreeBrush>(LimiteBrushes);
        private static readonly Dictionary<(int Couleur, int Epaisseur), EntreePen> cachePens = new Dictionary<(int, int), EntreePen>(LimitePens);
        private static int numeroImage;

        public static readonly Font FontMini = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        public static readonly Font FontStandard = new Font("Segoe UI", 9f);
        public static readonly Font FontGras = new Font("Segoe UI", 9f, FontStyle.Bold);

        public static int NombreBrushesEnCache => cacheBrushes.Count;
        public static int NombrePensEnCache => cachePens.Count;

        public static void CommencerImage()
        {
            numeroImage++;
        }

        public static void TerminerImage()
        {
            if (cacheBrushes.Count > LimiteBrushes)
                NettoyerBrushes();
            if (cachePens.Count > LimitePens)
                NettoyerPens();
        }

        public static SolidBrush ObtenirBrush(Color couleur)
        {
            int argb = couleur.ToArgb();
            if (cacheBrushes.TryGetValue(argb, out var entree))
            {
                entree.DerniereImage = numeroImage;
                return entree.Brush;
            }

            var brush = new SolidBrush(couleur);
            cacheBrushes[argb] = new EntreeBrush(brush, numeroImage);
            return brush;
        }

        public static SolidBrush ObtenirBrushAlpha(int alpha, Color couleur)
        {
            alpha = Math.Clamp(alpha, 0, 255);
            alpha = Math.Clamp(((alpha + 16) / 32) * 32, 0, 255);
            int argb = Color.FromArgb(alpha, couleur.R, couleur.G, couleur.B).ToArgb();
            return ObtenirBrush(Color.FromArgb(argb));
        }

        public static Pen ObtenirPen(Color couleur, float epaisseur = 1f)
        {
            return ObtenirPenInterne(couleur, epaisseur, false);
        }

        /// <summary>
        /// Pen à extrémités arrondies (membres articulés, traînées). Mis en cache séparément
        /// pour ne jamais modifier les pens partagés retournés par <see cref="ObtenirPen"/>.
        /// </summary>
        public static Pen ObtenirPenArrondi(Color couleur, float epaisseur = 1f)
        {
            return ObtenirPenInterne(couleur, epaisseur, true);
        }

        private static Pen ObtenirPenInterne(Color couleur, float epaisseur, bool arrondi)
        {
            int alpha = Math.Clamp(((couleur.A + 16) / 32) * 32, 0, 255);
            Color couleurCachee = Color.FromArgb(alpha, couleur.R, couleur.G, couleur.B);
            int epaisseurCachee = Math.Max(1, (int)MathF.Round(Math.Max(0.5f, epaisseur) * 2f));
            // Le bit de poids fort de l'épaisseur distingue les pens arrondis des pens normaux
            var key = (couleurCachee.ToArgb(), arrondi ? epaisseurCachee | 0x40000000 : epaisseurCachee);
            if (cachePens.TryGetValue(key, out var entree))
            {
                entree.DerniereImage = numeroImage;
                return entree.Pen;
            }

            float largeur = epaisseurCachee * 0.5f;
            var pen = new Pen(couleurCachee, largeur);
            if (arrondi)
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            }
            cachePens[key] = new EntreePen(pen, numeroImage);
            return pen;
        }

        private static void NettoyerBrushes()
        {
            var aSupprimer = new List<int>(cacheBrushes.Count - LimiteBrushes);
            foreach (var entree in cacheBrushes)
            {
                if (entree.Value.DerniereImage != numeroImage)
                    aSupprimer.Add(entree.Key);
                if (cacheBrushes.Count - aSupprimer.Count <= LimiteBrushes)
                    break;
            }

            foreach (int cle in aSupprimer)
            {
                if (cacheBrushes.Remove(cle, out EntreeBrush? entree))
                    entree.Brush.Dispose();
            }
        }

        private static void NettoyerPens()
        {
            var aSupprimer = new List<(int Couleur, int Epaisseur)>(cachePens.Count - LimitePens);
            foreach (var entree in cachePens)
            {
                if (entree.Value.DerniereImage != numeroImage)
                    aSupprimer.Add(entree.Key);
                if (cachePens.Count - aSupprimer.Count <= LimitePens)
                    break;
            }

            foreach (var cle in aSupprimer)
            {
                if (cachePens.Remove(cle, out EntreePen? entree))
                    entree.Pen.Dispose();
            }
        }
    }

    // ==============================================================
    // GESTIONNAIRE D'ENTRÉES (CLAVIER WASD/ZQSD + SOURIS)
    // ==============================================================
    public class GestionnaireEntrees
    {
        private readonly HashSet<Keys> touchesEnfoncees = new HashSet<Keys>();
        private readonly HashSet<Keys> touchesFraichementPressees = new HashSet<Keys>();
        private readonly Dictionary<string, Keys> raccourcis = new Dictionary<string, Keys>(StringComparer.OrdinalIgnoreCase)
        {
            ["Haut"] = Keys.W,
            ["Bas"] = Keys.S,
            ["Gauche"] = Keys.A,
            ["Droite"] = Keys.D,
            ["Esquive"] = Keys.Space,
            ["Interaction"] = Keys.E,
            ["Sac"] = Keys.I,
            ["Quetes"] = Keys.G,
            ["Forge"] = Keys.C,
            ["Artisanat"] = Keys.K,
            ["Pause"] = Keys.Escape,
            ["Sauvegarde"] = Keys.F5,
            ["Sort1"] = Keys.Q,
            ["Sort2"] = Keys.F,
            ["Ultime"] = Keys.R,
            ["PotionSoin"] = Keys.D1,
            ["PotionMana"] = Keys.D2
        };

        public Point PositionSourisEcran { get; private set; }
        public Vector2 PositionSourisMonde { get; private set; }

        public bool ClicGauche { get; private set; }
        public bool ClicDroit { get; private set; }
        public bool ClicGaucheDeclenche { get; private set; }
        public bool ClicDroitDeclenche { get; private set; }

        public Keys Touche(string action) => raccourcis.TryGetValue(action, out Keys touche) ? touche : Keys.None;

        public Dictionary<string, Keys> CopierRaccourcis() => new Dictionary<string, Keys>(raccourcis, StringComparer.OrdinalIgnoreCase);

        public bool EstToucheAffectee(string action, Keys touche)
        {
            foreach (var raccourci in raccourcis)
            {
                if (!string.Equals(raccourci.Key, action, StringComparison.OrdinalIgnoreCase) && raccourci.Value == touche)
                    return true;
            }
            return false;
        }

        public bool DefinirTouche(string action, Keys touche)
        {
            if (!raccourcis.ContainsKey(action) || touche == Keys.None || EstToucheAffectee(action, touche))
                return false;
            raccourcis[action] = touche;
            return true;
        }

        public void ChargerRaccourcis(IEnumerable<KeyValuePair<string, Keys>>? sauvegardes)
        {
            if (sauvegardes == null) return;

            Dictionary<string, Keys> configuration = new Dictionary<string, Keys>(raccourcis, StringComparer.OrdinalIgnoreCase);
            foreach (var raccourci in sauvegardes)
            {
                if (configuration.ContainsKey(raccourci.Key) && raccourci.Value != Keys.None)
                    configuration[raccourci.Key] = raccourci.Value;
            }

            HashSet<Keys> touchesUtilisees = new HashSet<Keys>();
            foreach (Keys touche in configuration.Values)
            {
                if (!touchesUtilisees.Add(touche)) return;
            }

            raccourcis.Clear();
            foreach (var raccourci in configuration)
                raccourcis.Add(raccourci.Key, raccourci.Value);
        }

        public void TraiterKeyDown(Keys key)
        {
            if (!touchesEnfoncees.Contains(key))
            {
                touchesFraichementPressees.Add(key);
            }
            touchesEnfoncees.Add(key);
        }

        public void TraiterKeyUp(Keys key)
        {
            touchesEnfoncees.Remove(key);
        }

        public void TraiterMouseMove(Point posEcran, Camera2D camera)
        {
            PositionSourisEcran = posEcran;
            PositionSourisMonde = camera.EcranVersMonde(posEcran);
        }

        public void TraiterMouseDown(MouseButtons bouton)
        {
            if (bouton == MouseButtons.Left)
            {
                ClicGauche = true;
                ClicGaucheDeclenche = true;
            }
            else if (bouton == MouseButtons.Right)
            {
                ClicDroit = true;
                ClicDroitDeclenche = true;
            }
        }

        public void TraiterMouseUp(MouseButtons bouton)
        {
            if (bouton == MouseButtons.Left) ClicGauche = false;
            if (bouton == MouseButtons.Right) ClicDroit = false;
        }

        public void ReinitialiserTriggersFrame(Camera2D camera)
        {
            touchesFraichementPressees.Clear();
            ClicGaucheDeclenche = false;
            ClicDroitDeclenche = false;
            PositionSourisMonde = camera.EcranVersMonde(PositionSourisEcran);
        }

        public void ReinitialiserToutesTouches()
        {
            touchesEnfoncees.Clear();
            touchesFraichementPressees.Clear();
            ClicGauche = false;
            ClicDroit = false;
            ClicGaucheDeclenche = false;
            ClicDroitDeclenche = false;
        }

        public bool EstEnfoncee(Keys key) => touchesEnfoncees.Contains(key);
        public bool VientDEtrePressee(Keys key) => touchesFraichementPressees.Contains(key);

        // Déplacements : STRICTEMENT WASD (W = Haut, A = Gauche, S = Bas, D = Droite)
        public bool AllerHaut => EstEnfoncee(Touche("Haut"));
        public bool AllerBas => EstEnfoncee(Touche("Bas"));
        public bool AllerGauche => EstEnfoncee(Touche("Gauche"));
        public bool AllerDroite => EstEnfoncee(Touche("Droite"));

        public bool EsquiveDash => VientDEtrePressee(Touche("Esquive")) || EstEnfoncee(Touche("Esquive"));
        public bool ToucheInteragir => VientDEtrePressee(Touche("Interaction"));
        public bool ToucheInventaire => VientDEtrePressee(Touche("Sac")) || VientDEtrePressee(Keys.Tab);
        public bool ToucheGuilde => VientDEtrePressee(Touche("Quetes"));
        public bool ToucheForge => VientDEtrePressee(Touche("Forge"));
        public bool ToucheMenuPause => VientDEtrePressee(Touche("Pause"));

        public bool TouchePotionSoin => VientDEtrePressee(Touche("PotionSoin")) || VientDEtrePressee(Keys.NumPad1);
        public bool TouchePotionMana => VientDEtrePressee(Touche("PotionMana")) || VientDEtrePressee(Keys.NumPad2);
        
        // Touches de compétences ultra-réactives
        public bool ToucheCompetence1 => VientDEtrePressee(Touche("Sort1")) || EstEnfoncee(Touche("Sort1")) || VientDEtrePressee(Keys.D3) || VientDEtrePressee(Keys.NumPad3) || ClicDroitDeclenche || ClicDroit;
        public bool ToucheCompetence2 => VientDEtrePressee(Touche("Sort2")) || EstEnfoncee(Touche("Sort2")) || VientDEtrePressee(Keys.D4) || VientDEtrePressee(Keys.NumPad4);
        public bool ToucheUltime => VientDEtrePressee(Touche("Ultime")) || EstEnfoncee(Touche("Ultime"));
    }

    // ==============================================================
    // CAMÉRA 2D DYNAMIQUE AVEC SECOUSSES & LERP FLUIDE
    // ==============================================================
    public class Camera2D
    {
        public Vector2 Position;
        public Vector2 Cible;
        public float Zoom = 1.0f;
        public int LargeurEcran = 1280;
        public int HauteurEcran = 720;

        private float secousseIntensite = 0f;
        private float secousseTempsRestant = 0f;
        private float phaseSecousse = 0f;
        private Vector2 offsetSecousse;
        private float zoomRepos = 1f;
        private float impulsionZoom = 0f;
        private float tempsZoomRestant = 0f;
        private float dureeZoom = 0f;
        private float flashAlpha = 0f;
        private float flashRestant = 0f;
        private float flashDuree = 0f;
        public Color CouleurFlash { get; private set; } = Color.White;
        public int AlphaFlash => flashDuree > 0f
            ? Math.Clamp((int)(flashAlpha * MathF.Pow(Math.Clamp(flashRestant / flashDuree, 0f, 1f), 1.7f)), 0, 255)
            : 0;

        public Camera2D(int largeur, int hauteur)
        {
            LargeurEcran = largeur;
            HauteurEcran = hauteur;
            Position = Vector2.Zero;
            Cible = Vector2.Zero;
        }

        public void MettreAJour(float dt)
        {
            // Poursuite fluide (Lerp) de la cible
            Position = Vector2.Lerp(Position, Cible, MathF.Min(1f, dt * 8f));

            if (tempsZoomRestant > 0f)
            {
                tempsZoomRestant = MathF.Max(0f, tempsZoomRestant - dt);
                float progressionZoom = 1f - tempsZoomRestant / dureeZoom;
                Zoom = zoomRepos + impulsionZoom * MathF.Sin(progressionZoom * MathF.PI);
            }
            else
            {
                zoomRepos = Zoom;
                impulsionZoom = 0f;
            }

            if (flashRestant > 0f)
            {
                flashRestant = MathF.Max(0f, flashRestant - dt);
                if (flashRestant <= 0f)
                    flashAlpha = 0f;
            }

            if (secousseTempsRestant > 0f)
            {
                secousseTempsRestant -= dt;
                phaseSecousse += dt;
                if (secousseTempsRestant <= 0f)
                {
                    secousseIntensite = 0f;
                    offsetSecousse = Vector2.Zero;
                }
                else
                {
                    float facteur = Math.Clamp(secousseTempsRestant * 4f, 0f, 1f);
                    float dx = (MathF.Sin(phaseSecousse * 31f) + MathF.Sin(phaseSecousse * 47f) * 0.32f) * secousseIntensite * facteur * 0.68f;
                    float dy = (MathF.Cos(phaseSecousse * 37f) + MathF.Sin(phaseSecousse * 53f) * 0.28f) * secousseIntensite * facteur * 0.68f;
                    offsetSecousse = new Vector2(dx, dy);
                }
            }
            else
            {
                offsetSecousse = Vector2.Zero;
            }
        }

        public void DeclencherSecousse(float intensite, float dureeSecondes = 0.25f)
        {
            secousseIntensite = MathF.Max(secousseIntensite, intensite);
            secousseTempsRestant = MathF.Max(secousseTempsRestant, dureeSecondes);
        }

        public void DeclencherEffetSort(float secousse, float duree, float zoom, Color couleurFlash, int alphaFlash)
        {
            DeclencherSecousse(secousse, duree);

            if (zoom > impulsionZoom)
            {
                if (tempsZoomRestant <= 0f)
                    zoomRepos = Zoom;
                impulsionZoom = zoom;
                dureeZoom = MathF.Max(dureeZoom, duree);
                tempsZoomRestant = MathF.Max(tempsZoomRestant, duree);
            }

            if (flashRestant <= 0f || alphaFlash >= flashAlpha)
            {
                CouleurFlash = couleurFlash;
                flashAlpha = alphaFlash;
                flashDuree = MathF.Max(flashDuree, MathF.Min(duree, 0.28f));
                flashRestant = MathF.Max(flashRestant, flashDuree);
            }
        }

        public Vector2 ObtenirOffsetSecousse()
        {
            return offsetSecousse;
        }

        public Point MondeVersEcran(Vector2 posMonde)
        {
            Vector2 shake = ObtenirOffsetSecousse();
            float cx = (posMonde.X - Position.X + shake.X) * Zoom + (LargeurEcran / 2f);
            float cy = (posMonde.Y - Position.Y + shake.Y) * Zoom + (HauteurEcran / 2f);
            return new Point((int)cx, (int)cy);
        }

        public Vector2 EcranVersMonde(Point posEcran)
        {
            Vector2 shake = ObtenirOffsetSecousse();
            float wx = (posEcran.X - (LargeurEcran / 2f)) / Zoom + Position.X - shake.X;
            float wy = (posEcran.Y - (HauteurEcran / 2f)) / Zoom + Position.Y - shake.Y;
            return new Vector2(wx, wy);
        }

        public RectangleF ObtenirZoneVisible(float padding = 90f)
        {
            Vector2 shake = ObtenirOffsetSecousse();
            float demiLarg = (LargeurEcran / (2f * Zoom)) + padding;
            float demiHaut = (HauteurEcran / (2f * Zoom)) + padding;
            return new RectangleF(
                Position.X - shake.X - demiLarg,
                Position.Y - shake.Y - demiHaut,
                demiLarg * 2f,
                demiHaut * 2f
            );
        }

        public bool EstVisible(Vector2 posMonde, float rayon = 30f)
        {
            Point p = MondeVersEcran(posMonde);
            return p.X >= -rayon - 50 && p.X <= LargeurEcran + rayon + 50 &&
                   p.Y >= -rayon - 50 && p.Y <= HauteurEcran + rayon + 50;
        }
    }

    // ==============================================================
    // SYNTHÉTISEUR AUDIO TEMPS RÉEL (NAudio : mixage, enveloppe, volume)
    // ==============================================================
    public static class AudioSynthetiseur
    {
        private const int FrequenceEchantillonnage = 44100;
        private const int SonsSimultanesMax = 12;

        private static readonly object Verrou = new object();
        private static WaveOut? sortie;
        private static NAudio.Wave.SampleProviders.MixingSampleProvider? mixeur;
        private static bool initialisationEchouee;
        private static DateTime dernierSon = DateTime.MinValue;
        private static float volume = 0.6f;

        public static bool AudioActif { get; set; } = true;

        /// <summary>Volume général des effets (0 à 1).</summary>
        public static float Volume
        {
            get => volume;
            set => volume = Math.Clamp(value, 0f, 1f);
        }

        private static bool AssurerInitialisation()
        {
            if (mixeur != null) return true;
            if (initialisationEchouee) return false;

            lock (Verrou)
            {
                if (mixeur != null) return true;
                try
                {
                    var format = WaveFormat.CreateIeeeFloatWaveFormat(FrequenceEchantillonnage, 1);
                    mixeur = new NAudio.Wave.SampleProviders.MixingSampleProvider(format) { ReadFully = true };
                    sortie = new WaveOut();
                    sortie.Init(mixeur.ToWaveProvider());
                    sortie.Play();
                    return true;
                }
                catch (Exception ex)
                {
                    initialisationEchouee = true;
                    mixeur = null;
                    sortie?.Dispose();
                    sortie = null;
                    JournalErreurs.Enregistrer(ex, "AudioSynthetiseur.AssurerInitialisation");
                    return false;
                }
            }
        }

        /// <summary>Joue une suite de notes (fréquence Hz, durée ms) sans bloquer le jeu.</summary>
        private static void JouerNotes(float intensite, params (int Frequence, int DureeMs)[] notes)
        {
            if (!AudioActif || volume <= 0f || notes.Length == 0) return;
            if (!AssurerInitialisation() || mixeur == null) return;

            try
            {
                if (mixeur.MixerInputs.Count() >= SonsSimultanesMax) return;
                mixeur.AddMixerInput(new NoteSynthetisee(notes, intensite * volume, FrequenceEchantillonnage));
            }
            catch (Exception ex) { JournalErreurs.Enregistrer(ex, "AudioSynthetiseur.JouerNotes"); }
        }

        public static void Liberer()
        {
            lock (Verrou)
            {
                try { sortie?.Stop(); } catch { /* fermeture : rien à faire */ }
                sortie?.Dispose();
                sortie = null;
                mixeur = null;
            }
        }

        public static void SonCoupEpee()
        {
            if ((DateTime.Now - dernierSon).TotalMilliseconds < 80) return;
            dernierSon = DateTime.Now;
            JouerNotes(0.35f, (520, 45));
        }

        public static void SonImpact() => JouerNotes(0.5f, (240, 60));
        public static void SonCritique() => JouerNotes(0.45f, (880, 50), (1320, 90));
        public static void SonDash() => JouerNotes(0.3f, (400, 35));
        public static void SonTirMagique() => JouerNotes(0.35f, (740, 50));
        public static void SonExplosion() => JouerNotes(0.6f, (140, 110));
        public static void SonLoot() => JouerNotes(0.4f, (987, 40), (1318, 70));
        public static void SonLevelUp() => JouerNotes(0.45f, (523, 70), (659, 70), (784, 80), (1046, 140));
        public static void SonCriBoss() => JouerNotes(0.65f, (120, 120), (90, 200));
        public static void SonClic() => JouerNotes(0.25f, (640, 30));
        public static void SonSelectionMenu() => JouerNotes(0.35f, (520, 35), (780, 50));

        /// <summary>
        /// Source audio : onde carrée adoucie (style rétro) avec attaque/relâche courtes
        /// pour éviter les clics. Retourne 0 échantillon une fois terminée → retirée du mixeur.
        /// </summary>
        private sealed class NoteSynthetisee : NAudio.Wave.ISampleProvider
        {
            private readonly (int Frequence, int DureeMs)[] notes;
            private readonly float amplitude;
            private readonly int frequenceEch;
            private int indexNote;
            private int echantillonDansNote;
            private int echantillonsNote;
            private double phase;

            public NAudio.Wave.WaveFormat WaveFormat { get; }

            public NoteSynthetisee((int, int)[] notes, float amplitude, int frequenceEch)
            {
                this.notes = notes;
                this.amplitude = Math.Clamp(amplitude, 0f, 1f) * 0.5f;
                this.frequenceEch = frequenceEch;
                WaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(frequenceEch, 1);
                echantillonsNote = DureeEnEchantillons(0);
            }

            private int DureeEnEchantillons(int index) =>
                Math.Max(1, notes[index].DureeMs * frequenceEch / 1000);

            public int Read(Span<float> buffer)
            {
                int ecrits = 0;
                while (ecrits < buffer.Length && indexNote < notes.Length)
                {
                    float freq = Math.Clamp(notes[indexNote].Frequence, 37, 12000);
                    int fondu = Math.Min(echantillonsNote / 4, frequenceEch / 200); // ≤ 5 ms
                    int restant = echantillonsNote - echantillonDansNote;
                    float enveloppe = 1f;
                    if (fondu > 0)
                    {
                        if (echantillonDansNote < fondu) enveloppe = (float)echantillonDansNote / fondu;
                        else if (restant < fondu) enveloppe = (float)restant / fondu;
                    }

                    // Carrée adoucie : somme des 3 premières harmoniques impaires
                    double s = Math.Sin(phase) + Math.Sin(3 * phase) / 3.0 + Math.Sin(5 * phase) / 5.0;
                    buffer[ecrits] = (float)(s * 0.8) * amplitude * enveloppe;

                    phase += 2 * Math.PI * freq / frequenceEch;
                    if (phase > 2 * Math.PI) phase -= 2 * Math.PI;

                    ecrits++;
                    echantillonDansNote++;
                    if (echantillonDansNote >= echantillonsNote)
                    {
                        indexNote++;
                        echantillonDansNote = 0;
                        if (indexNote < notes.Length) echantillonsNote = DureeEnEchantillons(indexNote);
                    }
                }
                return ecrits;
            }

            public int Read(float[] buffer, int offset, int count)
            {
                return Read(buffer.AsSpan(offset, count));
            }
        }
    }

    // ==============================================================
    // BOUCLE DE JEU PRÉCISE (Application.Idle + attente sur messages)
    // ==============================================================
    /// <summary>
    /// Remplace System.Windows.Forms.Timer (résolution ~15,6 ms, saccades) par une boucle
    /// cadencée au Stopwatch. Même API utile : Interval, Tick, Start, Stop, Enabled.
    /// La boucle rend la main dès qu'un message Windows arrive (clavier, souris, peinture),
    /// et ne tourne pas pendant les boîtes de dialogue modales natives.
    /// </summary>
    public sealed class BoucleJeuPrecise : IDisposable
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public Point pt; }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool PeekMessage(out MSG msg, IntPtr hWnd, uint filterMin, uint filterMax, uint flags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint MsgWaitForMultipleObjects(uint nCount, IntPtr[]? handles, bool waitAll, uint millisecondes, uint masqueReveil);

        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint periode);

        [System.Runtime.InteropServices.DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint periode);

        private const uint QS_ALLINPUT = 0x04FF;
        private readonly Stopwatch chrono = Stopwatch.StartNew();
        private double prochainTickMs;
        private bool abonne;
        private bool periodeActive;
        private bool enTick;

        public int Interval { get; set; } = 16;
        public bool Enabled { get; private set; }
        public event EventHandler? Tick;

        public void Start()
        {
            if (Enabled) return;
            Enabled = true;
            prochainTickMs = chrono.Elapsed.TotalMilliseconds;
            if (!periodeActive) { timeBeginPeriod(1); periodeActive = true; }
            if (!abonne) { Application.Idle += SurIdle; abonne = true; }
        }

        public void Stop()
        {
            Enabled = false;
        }

        private static bool MessageEnAttente() => PeekMessage(out _, IntPtr.Zero, 0, 0, 0);

        private void SurIdle(object? sender, EventArgs e)
        {
            while (Enabled && !MessageEnAttente())
            {
                double maintenant = chrono.Elapsed.TotalMilliseconds;
                double attente = prochainTickMs - maintenant;
                if (attente <= 0)
                {
                    // Rattrapage limité : si on a pris trop de retard, on repart de maintenant
                    prochainTickMs = Math.Max(prochainTickMs + Interval, maintenant);
                    if (enTick) return;
                    enTick = true;
                    try { Tick?.Invoke(this, EventArgs.Empty); }
                    finally { enTick = false; }
                }
                else if (attente > 1.5)
                {
                    // Dort jusqu'au prochain tick, mais se réveille immédiatement sur une entrée
                    MsgWaitForMultipleObjects(0, null, false, (uint)(attente - 1), QS_ALLINPUT);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        public void Dispose()
        {
            Enabled = false;
            if (abonne) { Application.Idle -= SurIdle; abonne = false; }
            if (periodeActive) { timeEndPeriod(1); periodeActive = false; }
        }
    }
}
