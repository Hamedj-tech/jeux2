using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace JeuxRPG
{
    // ==============================================================
    // 1. COMPOSANTS GRAPHIQUES ANTI-BUG (ZÉRO SUPERPOSITION)
    // ==============================================================

    public class DarkProgressBar : Control
    {
        private int _valeur = 0;
        private int _maximum = 100;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CouleurDebut { get; set; } = Color.FromArgb(231, 76, 60);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CouleurFin { get; set; } = Color.FromArgb(192, 57, 43);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TexteCentral { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Valeur
        {
            get => _valeur;
            set
            {
                _valeur = Math.Clamp(value, 0, Math.Max(1, _maximum));
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = Math.Max(1, value);
                _valeur = Math.Clamp(_valeur, 0, _maximum);
                Invalidate();
            }
        }

        public DarkProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 24;
            Margin = new Padding(0, 2, 0, 6);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Fond sombre
            using (var bFond = new SolidBrush(Color.FromArgb(22, 24, 30)))
            {
                g.FillRectangle(bFond, ClientRectangle);
            }

            // Barre de progression en dégradé
            float ratio = (float)_valeur / _maximum;
            int largeurRemplie = (int)(Width * ratio);

            if (largeurRemplie > 0)
            {
                Rectangle rectRempli = new Rectangle(0, 0, largeurRemplie, Height);
                using (var brushGradient = new LinearGradientBrush(rectRempli, CouleurDebut, CouleurFin, LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brushGradient, rectRempli);
                }

                // Reflet glossy
                using (var brushReflet = new SolidBrush(Color.FromArgb(45, 255, 255, 255)))
                {
                    g.FillRectangle(brushReflet, 0, 0, largeurRemplie, Height / 2);
                }
            }

            // Bordure nette
            using (var penBordure = new Pen(Color.FromArgb(60, 65, 80), 1f))
            {
                g.DrawRectangle(penBordure, 0, 0, Width - 1, Height - 1);
            }

            // Texte centré parfaitement
            string texte = string.IsNullOrEmpty(TexteCentral) ? $"{_valeur} / {_maximum}" : TexteCentral;
            using (Font f = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Ombre
                using (var bOmbre = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
                {
                    Rectangle rectOmbre = new Rectangle(1, 1, Width, Height);
                    g.DrawString(texte, f, bOmbre, rectOmbre, sf);
                }
                // Texte blanc
                using (var bTexte = new SolidBrush(Color.White))
                {
                    g.DrawString(texte, f, bTexte, ClientRectangle, sf);
                }
            }
        }
    }

    public class DarkCardPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CouleurBordure { get; set; } = Color.FromArgb(50, 56, 72);

        public DarkCardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(24, 27, 35);
            Padding = new Padding(12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(CouleurBordure, 1f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    public class DarkButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CouleurSurvol { get; set; } = Color.FromArgb(52, 152, 219);
        private Color _couleurBase;

        public DarkButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 1;
            FlatAppearance.BorderColor = Color.FromArgb(65, 72, 90);
            Cursor = Cursors.Hand;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            Height = 52;
            Width = 185;
            Margin = new Padding(5);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            _couleurBase = BackColor;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            BackColor = CouleurSurvol;
            FlatAppearance.BorderColor = Color.White;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            BackColor = _couleurBase;
            FlatAppearance.BorderColor = Color.FromArgb(65, 72, 90);
        }
    }

    // ==============================================================
    // 2. FENÊTRE PRINCIPALE DU JEU AVEC ARCHITECTURE TABLEAU ÉTANCHE
    // ==============================================================
    // 2. DONJONS & RAIDS MULTI-SALLES
    // ==============================================================
    public class EtapeDonjon
    {
        public int SalleNumero { get; set; }
        public string NomSalle { get; set; } = "";
        public string DescriptionLore { get; set; } = "";
        public bool EstCombat { get; set; }
        public Func<Monstre>? CreerEnnemi { get; set; }
        public string Choix1Texte { get; set; } = "";
        public Action<FormJeu>? Choix1Action { get; set; }
        public string Choix2Texte { get; set; } = "";
        public Action<FormJeu>? Choix2Action { get; set; }
    }

    public class ModeleDonjon
    {
        public string Nom { get; set; } = "";
        public string SousTitre { get; set; } = "";
        public int NiveauRecommande { get; set; }
        public Color Couleur { get; set; }
        public List<EtapeDonjon> Etapes { get; set; } = new List<EtapeDonjon>();
        public int PrimeOr { get; set; }
        public int PrimeXP { get; set; }
        public int PrimePierres { get; set; }
        public int PrimeReputation { get; set; }
        public int PrimeSceaux { get; set; }
        public Equipement? PrimeEquipement { get; set; }
    }

    // ==============================================================
    // 3. FENÊTRE PRINCIPALE DU JEU AVEC ARCHITECTURE TABLEAU ÉTANCHE
    // ==============================================================

    public class FormJeu : Form
    {
        // Données du jeu
        private Joueur hero = null!;
        private Monstre? ennemiActuel = null;
        private bool enCombat = false;
        private Random rng = Random.Shared;
        private List<Quete> quetesDisponibles = new List<Quete>();
        private const string FichierSauvegarde = "sauvegarde_aethelgard.json";

        // Donjons et Raids
        private ModeleDonjon? donjonActuel = null;
        private int indexSalleDonjon = 0;

        // Effets visuels & Statuts de combat
        private DarkCardPanel pnlAreneCard = null!;
        private Label lblImpactVisuel = null!;
        private System.Windows.Forms.Timer timerPulsation = null!;
        private bool pulsationPhase = false;
        private int jetonImpact = 0;

        // Statuts du joueur et boss
        private int joueurBruleTours = 0;
        private int joueurPoisonTours = 0;
        private int joueurStunTours = 0;
        private bool bossChargeAttaque = false;

        // Combat temporaire
        private int buffAttaqueTours = 0;
        private int buffDefenseTours = 0;
        private bool postureDefense = false;

        // Contrôles UI - Haut
        private Label lblBandeauTitre = null!;
        private Label lblBandeauLieu = null!;

        // Contrôles UI - Fiche Héros (Gauche)
        private Label lblHeroNom = null!;
        private Label lblHeroClasse = null!;
        private DarkProgressBar pbHeroPV = null!;
        private DarkProgressBar pbHeroMana = null!;
        private DarkProgressBar pbHeroXP = null!;
        private DarkProgressBar pbHeroUltime = null!;

        private Label lblRessources = null!;
        private Label lblStatsCombat = null!;
        private Label lblPointsAttributs = null!;
        private FlowLayoutPanel pnlBoutonsStats = null!;
        private Label lblEquipementResume = null!;

        // Contrôles UI - Arène / Ennemi (Centre Haut)
        private Label lblAreneTitre = null!;
        private Label lblEnnemiNom = null!;
        private Label lblEnnemiBadge = null!;
        private DarkProgressBar pbEnnemiPV = null!;
        private Label lblEnnemiFaiblesseLore = null!;

        // Journal de combat (Centre Milieu)
        private RichTextBox rtbJournal = null!;

        // Hub d'actions séparé (Centre Bas)
        private enum CategorieCombat { Donjons, Zones, BossRegionaux, BossMythiques, BossCosmiques, Tout }
        private CategorieCombat categorieCombatActive = CategorieCombat.Donjons;

        private DarkCardPanel pnlServicesCard = null!;
        private Label lblServicesTitre = null!;
        private FlowLayoutPanel pnlServicesButtons = null!;

        private DarkCardPanel pnlCombatsCard = null!;
        private Label lblCombatsTitre = null!;
        private FlowLayoutPanel pnlCombatOnglets = null!;
        private FlowLayoutPanel pnlActionButtons = null!;

        // Boutons onglets combat
        private Button btnOngletDonjons = null!;
        private Button btnOngletZones = null!;
        private Button btnOngletBossRegionaux = null!;
        private Button btnOngletBossMythiques = null!;
        private Button btnOngletBossCosmiques = null!;
        private Button btnOngletTout = null!;

        // Boutons services dédiés
        private DarkButton btnReclamerPrimesDirect = null!;
        private DarkButton btnServiceSac = null!;
        private DarkButton btnServiceGuilde = null!;
        private DarkButton btnServiceForge = null!;
        private DarkButton btnServiceAuberge = null!;

        // Boutons combat dédiés
        private DarkButton btnCombatAttaque = null!;
        private DarkButton btnCombatLourde = null!;
        private DarkButton btnCombatSort = null!;
        private DarkButton btnCombatSortForge = null!;
        private DarkButton btnCombatUltime = null!;
        private DarkButton btnCombatSoin = null!;
        private DarkButton btnCombatBouclier = null!;
        private DarkButton btnCombatFuite = null!;

        // Tour Astrale Infinie
        private bool enTourAstrale = false;

        public FormJeu()
        {
            InitialiserFenetreGraphique();
            InitialiserTimerPulsation();
            InitialiserQuetes();

            if (File.Exists(FichierSauvegarde))
            {
                var rep = MessageBox.Show(
                    "Une sauvegarde de vos exploits a été trouvée !\nVoulez-vous charger votre ancienne partie ?",
                    "Partie Existante Détectée",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (rep == DialogResult.Yes && ChargerPartie())
                {
                    BasculerModeVillage();
                    return;
                }
            }

            AfficherDialogueCreationPersonnage();
            BasculerModeVillage();
        }

        private void InitialiserFenetreGraphique()
        {
            this.Text = "⚔️ CHRONIQUES D'AETHELGARD — RPG HD";
            this.Size = new Size(1320, 900);
            this.MinimumSize = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(14, 15, 19);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // ==============================================================
            // STRUCTURE MAÎTRESSE : TableLayoutPanel (ZÉRO RECOUVREMENT)
            // ==============================================================
            TableLayoutPanel mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.FromArgb(14, 15, 19),
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // Colonne 0 = Fiche Héros (360 px fixe), Colonne 1 = Zone de jeu (100%)
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360f));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // Ligne 0 = Bandeau Top (65 px fixe), Ligne 1 = Corps principal (100%)
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 65f));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            this.Controls.Add(mainTable);

            // ==============================================================
            // 1. BANDEAU DE NAVIGATION SUPÉRIEUR (Ligne 0, Col 0 avec ColSpan=2)
            // ==============================================================
            TableLayoutPanel topTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(20, 23, 30),
                Margin = new Padding(0),
                Padding = new Padding(15, 6, 15, 6)
            };
            topTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f)); // Titres à gauche
            topTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f)); // Boutons à droite
            topTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Titres Gauche
            Panel pnlTopLeft = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            lblBandeauTitre = new Label
            {
                Text = "⚔️ AETHELGARD : L'ÈRE DES LÉGENDES",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15),
                AutoSize = true,
                Location = new Point(0, 4)
            };
            lblBandeauLieu = new Label
            {
                Text = "📍 Emplacement : 🏰 Village de Val-Serein (Zone Sûre)",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(46, 204, 113),
                AutoSize = true,
                Location = new Point(2, 30)
            };
            pnlTopLeft.Controls.AddRange(new Control[] { lblBandeauTitre, lblBandeauLieu });
            topTable.Controls.Add(pnlTopLeft, 0, 0);

            // Boutons Droite (Alignés proprement sans jamais déborder)
            FlowLayoutPanel pnlTopRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Color.Transparent,
                WrapContents = false,
                Padding = new Padding(0, 4, 0, 0)
            };
            pnlTopRight.Controls.Add(CreerTopPillButton("❓ Guide", Color.FromArgb(45, 52, 70), (s, e) => AfficherAide()));
            pnlTopRight.Controls.Add(CreerTopPillButton("📂 Charger", Color.FromArgb(52, 73, 94), (s, e) => { if (ChargerPartie()) { BasculerModeVillage(); MettreAJourInterface(); } }));
            pnlTopRight.Controls.Add(CreerTopPillButton("💾 Sauvegarder", Color.FromArgb(39, 174, 96), (s, e) => SauvegarderPartie()));
            pnlTopRight.Controls.Add(CreerTopPillButton("🏰 DONJONS & RAIDS", Color.FromArgb(192, 57, 43), (s, e) => OuvrirMenuDonjons()));
            pnlTopRight.Controls.Add(CreerTopPillButton("🔨 FORGE", Color.FromArgb(180, 100, 30), (s, e) => OuvrirForge()));
            pnlTopRight.Controls.Add(CreerTopPillButton("📜 GUILDE", Color.FromArgb(212, 160, 23), (s, e) => OuvrirQuetes()));
            pnlTopRight.Controls.Add(CreerTopPillButton("🎒 INVENTAIRE", Color.FromArgb(142, 68, 173), (s, e) => OuvrirSac()));
            topTable.Controls.Add(pnlTopRight, 1, 0);

            mainTable.Controls.Add(topTable, 0, 0);
            mainTable.SetColumnSpan(topTable, 2);

            // ==============================================================
            // 2. FICHE DU HÉROS (Ligne 1, Col 0) - FLUIDE SANS CHEVAUCHEMENT
            // ==============================================================
            Panel pnlLeftPad = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 10, 6, 10),
                BackColor = Color.Transparent
            };

            DarkCardPanel pnlHeroCard = new DarkCardPanel
            {
                Dock = DockStyle.Fill,
                CouleurBordure = Color.FromArgb(50, 56, 72),
                Padding = new Padding(0)
            };

            FlowLayoutPanel pnlHeroStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(14, 12, 14, 12)
            };

            lblHeroNom = new Label
            {
                Text = "⭐ Héros",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 156, 18),
                Width = 300,
                Height = 26,
                Margin = new Padding(0, 0, 0, 0)
            };
            pnlHeroStack.Controls.Add(lblHeroNom);

            lblHeroClasse = new Label
            {
                Text = "NIVEAU 1 • CLASSE",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(170, 185, 205),
                Width = 300,
                AutoSize = true,
                MaximumSize = new Size(300, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            pnlHeroStack.Controls.Add(lblHeroClasse);

            // Barres personnalisées
            pbHeroPV = AjouterJaugeSection(pnlHeroStack, "❤️ SANTÉ (PV)", Color.FromArgb(231, 76, 60), Color.FromArgb(192, 57, 43));
            pbHeroMana = AjouterJaugeSection(pnlHeroStack, "💧 MANA MAGIQUE", Color.FromArgb(52, 152, 219), Color.FromArgb(41, 128, 185));
            pbHeroXP = AjouterJaugeSection(pnlHeroStack, "⭐ EXPÉRIENCE", Color.FromArgb(155, 89, 182), Color.FromArgb(142, 68, 173));
            pbHeroUltime = AjouterJaugeSection(pnlHeroStack, "🌟 JAUGE D'ULTIME", Color.FromArgb(241, 196, 15), Color.FromArgb(230, 126, 34));

            // Ressources
            lblRessources = new Label
            {
                Width = 300,
                AutoSize = true,
                MaximumSize = new Size(300, 0),
                ForeColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.FromArgb(18, 20, 26),
                Padding = new Padding(8),
                Margin = new Padding(0, 4, 0, 8)
            };
            pnlHeroStack.Controls.Add(lblRessources);

            // Stats de combat
            lblStatsCombat = new Label
            {
                Width = 300,
                AutoSize = true,
                MaximumSize = new Size(300, 0),
                ForeColor = Color.FromArgb(200, 205, 220),
                Font = new Font("Segoe UI", 9f),
                BackColor = Color.FromArgb(18, 20, 26),
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 8)
            };
            pnlHeroStack.Controls.Add(lblStatsCombat);

            // Points d'attributs
            lblPointsAttributs = new Label
            {
                Width = 300,
                AutoSize = true,
                ForeColor = Color.FromArgb(241, 196, 15),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 4)
            };
            pnlHeroStack.Controls.Add(lblPointsAttributs);

            pnlBoutonsStats = new FlowLayoutPanel
            {
                Width = 300,
                Height = 36,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            pnlHeroStack.Controls.Add(pnlBoutonsStats);

            // Équipement équipé
            lblEquipementResume = new Label
            {
                Width = 300,
                AutoSize = true,
                MaximumSize = new Size(300, 0),
                ForeColor = Color.FromArgb(195, 200, 215),
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.FromArgb(18, 20, 26),
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 8)
            };
            pnlHeroStack.Controls.Add(lblEquipementResume);

            // Séparation nette des raccourcis dans la barre latérale du héros
            Label lblSideSectionServices = new Label
            {
                Text = "🏛️ SERVICES DU VILLAGE",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15),
                Width = 300,
                Margin = new Padding(0, 4, 0, 4)
            };
            pnlHeroStack.Controls.Add(lblSideSectionServices);

            Button btnOuvrirSacSide = new Button
            {
                Text = "🎒 OUVRIR L'INVENTAIRE",
                Width = 300,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 4)
            };
            btnOuvrirSacSide.FlatAppearance.BorderSize = 0;
            btnOuvrirSacSide.Click += (s, e) => OuvrirSac();
            pnlHeroStack.Controls.Add(btnOuvrirSacSide);

            Button btnOuvrirQuetesSide = new Button
            {
                Text = "📜 GUILDE DES QUÊTES",
                Width = 300,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(212, 160, 23),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 4)
            };
            btnOuvrirQuetesSide.FlatAppearance.BorderSize = 0;
            btnOuvrirQuetesSide.Click += (s, e) => OuvrirQuetes();
            pnlHeroStack.Controls.Add(btnOuvrirQuetesSide);

            Button btnOuvrirForgeSide = new Button
            {
                Text = "🔨 FORGE DE BROM",
                Width = 300,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 100, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 8)
            };
            btnOuvrirForgeSide.FlatAppearance.BorderSize = 0;
            btnOuvrirForgeSide.Click += (s, e) => OuvrirForge();
            pnlHeroStack.Controls.Add(btnOuvrirForgeSide);

            Label lblSideSectionCombats = new Label
            {
                Text = "⚔️ EXPÉDITIONS & RAIDS",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60),
                Width = 300,
                Margin = new Padding(0, 4, 0, 4)
            };
            pnlHeroStack.Controls.Add(lblSideSectionCombats);

            Button btnOuvrirDonjonsSide = new Button
            {
                Text = "🏰 DONJONS DE FOU & RAIDS",
                Width = 300,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 10)
            };
            btnOuvrirDonjonsSide.FlatAppearance.BorderSize = 0;
            btnOuvrirDonjonsSide.Click += (s, e) => OuvrirMenuDonjons();
            pnlHeroStack.Controls.Add(btnOuvrirDonjonsSide);

            pnlHeroCard.Controls.Add(pnlHeroStack);
            pnlLeftPad.Controls.Add(pnlHeroCard);
            mainTable.Controls.Add(pnlLeftPad, 0, 1);

            // ==============================================================
            // 3. ZONE DE JEU CENTRALE (Ligne 1, Col 1) - TableLayoutPanel
            // ==============================================================
            TableLayoutPanel centerTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.FromArgb(14, 15, 19),
                Margin = new Padding(0),
                Padding = new Padding(6, 10, 12, 10)
            };

            // Ligne 0 = Arène (135 px), Ligne 1 = Journal (Fill), Ligne 2 = Actions (235 px)
            centerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 135f));
            centerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            centerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 235f));

            // 3.1 Carte Arène (Ligne 0)
            pnlAreneCard = new DarkCardPanel
            {
                Dock = DockStyle.Fill,
                CouleurBordure = Color.FromArgb(70, 35, 45),
                BackColor = Color.FromArgb(26, 18, 24),
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 8)
            };

            TableLayoutPanel tblAreneContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tblAreneContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
            tblAreneContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            tblAreneContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            tblAreneContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblAreneTitre = new Label
            {
                Text = "🏰 VILLAGE DE VAL-SEREIN",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Dock = DockStyle.Fill
            };
            tblAreneContent.Controls.Add(lblAreneTitre, 0, 0);

            FlowLayoutPanel pnlEnnemiLigneNom = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };
            lblEnnemiNom = new Label
            {
                Text = "Havre de paix et de repos",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60),
                AutoSize = true
            };
            lblEnnemiBadge = new Label
            {
                Text = "[ZONE SÛRE]",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                AutoSize = true,
                Margin = new Padding(10, 3, 0, 0)
            };
            lblImpactVisuel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.Gold,
                AutoSize = true,
                Visible = false,
                Margin = new Padding(15, 1, 0, 0)
            };
            pnlEnnemiLigneNom.Controls.AddRange(new Control[] { lblEnnemiNom, lblEnnemiBadge, lblImpactVisuel });
            tblAreneContent.Controls.Add(pnlEnnemiLigneNom, 0, 1);

            pbEnnemiPV = new DarkProgressBar
            {
                Dock = DockStyle.Fill,
                Height = 22,
                CouleurDebut = Color.FromArgb(231, 76, 60),
                CouleurFin = Color.FromArgb(150, 25, 25),
                Margin = new Padding(0, 2, 0, 4)
            };
            tblAreneContent.Controls.Add(pbEnnemiPV, 0, 2);

            lblEnnemiFaiblesseLore = new Label
            {
                Text = "Vous êtes en sécurité au village. Choisissez une destination ci-dessous pour partir en quête !",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 205, 215),
                Dock = DockStyle.Fill
            };
            tblAreneContent.Controls.Add(lblEnnemiFaiblesseLore, 0, 3);

            pnlAreneCard.Controls.Add(tblAreneContent);
            centerTable.Controls.Add(pnlAreneCard, 0, 0);

            // 3.2 Journal de Combat (Ligne 1 - Confiné STRICTEMENT dans sa cellule !)
            rtbJournal = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(11, 12, 15),
                ForeColor = Color.FromArgb(230, 235, 245),
                Font = new Font("Consolas", 10f, FontStyle.Regular),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 0, 8)
            };
            centerTable.Controls.Add(rtbJournal, 0, 1);

            // 3.3 Hub des Actions Séparé (Ligne 2) : 2 Cartes Distinctes (Services à Gauche, Combats à Droite)
            TableLayoutPanel tblHubActions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tblHubActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290f));
            tblHubActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tblHubActions.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // --- CARTE DE GAUCHE : SERVICES DU VILLAGE & GESTION ---
            pnlServicesCard = new DarkCardPanel
            {
                Dock = DockStyle.Fill,
                CouleurBordure = Color.FromArgb(90, 75, 40),
                BackColor = Color.FromArgb(20, 21, 27),
                Padding = new Padding(8, 6, 8, 6),
                Margin = new Padding(0, 0, 8, 0)
            };

            TableLayoutPanel tblServicesContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tblServicesContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
            tblServicesContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblServicesTitre = new Label
            {
                Text = "🏛️ SERVICES DU VILLAGE",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            tblServicesContent.Controls.Add(lblServicesTitre, 0, 0);

            pnlServicesButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0)
            };
            tblServicesContent.Controls.Add(pnlServicesButtons, 0, 1);
            pnlServicesCard.Controls.Add(tblServicesContent);
            tblHubActions.Controls.Add(pnlServicesCard, 0, 0);

            // --- CARTE DE DROITE : EXPÉDITIONS, DONJONS & COMBATS ---
            pnlCombatsCard = new DarkCardPanel
            {
                Dock = DockStyle.Fill,
                CouleurBordure = Color.FromArgb(85, 35, 45),
                BackColor = Color.FromArgb(23, 19, 23),
                Padding = new Padding(8, 6, 8, 6),
                Margin = new Padding(0)
            };

            TableLayoutPanel tblCombatsContent = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tblCombatsContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
            tblCombatsContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Ligne d'en-tête de combat avec Onglets Filtres
            TableLayoutPanel tblCombatsHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            tblCombatsHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
            tblCombatsHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tblCombatsHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblCombatsTitre = new Label
            {
                Text = "⚔️ EXPÉDITIONS & COMBATS",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            tblCombatsHeader.Controls.Add(lblCombatsTitre, 0, 0);

            pnlCombatOnglets = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };

            btnOngletDonjons = CreerOngletCombatButton("🏰 Donjons (3)", CategorieCombat.Donjons);
            btnOngletZones = CreerOngletCombatButton("🌲 Zones (5)", CategorieCombat.Zones);
            btnOngletBossRegionaux = CreerOngletCombatButton("👑 Régionaux (10)", CategorieCombat.BossRegionaux);
            btnOngletBossMythiques = CreerOngletCombatButton("🔥 Mythiques (10)", CategorieCombat.BossMythiques);
            btnOngletBossCosmiques = CreerOngletCombatButton("🌌 Cosmiques (10)", CategorieCombat.BossCosmiques);
            btnOngletTout = CreerOngletCombatButton("🌐 Tout (38)", CategorieCombat.Tout);

            pnlCombatOnglets.Controls.AddRange(new Control[] {
                btnOngletDonjons, btnOngletZones, btnOngletBossRegionaux, btnOngletBossMythiques, btnOngletBossCosmiques, btnOngletTout
            });
            tblCombatsHeader.Controls.Add(pnlCombatOnglets, 1, 0);
            tblCombatsContent.Controls.Add(tblCombatsHeader, 0, 0);

            pnlActionButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Margin = new Padding(0)
            };
            tblCombatsContent.Controls.Add(pnlActionButtons, 0, 1);
            pnlCombatsCard.Controls.Add(tblCombatsContent);
            tblHubActions.Controls.Add(pnlCombatsCard, 1, 0);

            centerTable.Controls.Add(tblHubActions, 0, 2);
            mainTable.Controls.Add(centerTable, 1, 1);

            // Boutons Services Dédiés
            btnReclamerPrimesDirect = CreerActionButton("🎁 RENDRE QUÊTES", "Toucher primes royales", Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113), (s, e) => ReclamerToutesQuetesTerminees(), 270, 42);
            btnServiceSac = CreerActionButton("🎒 Sac & Inventaire", "Gérer stuff, gemmes & potions", Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182), (s, e) => OuvrirSac(), 270, 42);
            btnServiceGuilde = CreerActionButton("📜 Guilde des Quêtes", "Rangs, Primes & Boutique", Color.FromArgb(212, 160, 23), Color.FromArgb(241, 196, 15), (s, e) => OuvrirQuetes(), 270, 42);
            btnServiceForge = CreerActionButton("🔨 Forge de Brom", "Amélioration d'armes", Color.FromArgb(180, 100, 30), Color.FromArgb(210, 130, 50), (s, e) => OuvrirForge(), 270, 42);
            btnServiceAuberge = CreerActionButton("🍻 Auberge du Repos", "Soin 100% & +25% XP", Color.FromArgb(22, 160, 133), Color.FromArgb(26, 188, 156), (s, e) => Auberge(), 270, 42);

            // Boutons de combat préparés
            btnCombatAttaque = CreerActionButton("⚔️ Attaque Normale", "Coup standard", Color.FromArgb(192, 57, 43), Color.FromArgb(231, 76, 60), (s, e) => ActionAttaqueNormale(), 155, 52);
            btnCombatLourde = CreerActionButton("🔨 Frappe Lourde", "160% Dégâts", Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182), (s, e) => ActionAttaqueLourde(), 155, 52);
            btnCombatSort = CreerActionButton("🔮 Sort Arcanique", "25 Mana requis", Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), (s, e) => ActionCompetence(), 155, 52);
            btnCombatUltime = CreerActionButton("🌟 ATTAQUE ULTIME", "Dégâts Colossaux", Color.FromArgb(211, 84, 0), Color.FromArgb(243, 156, 18), (s, e) => ActionUltime(), 155, 52);
            btnCombatSoin = CreerActionButton("🧪 Potion Soin", "+60 PV", Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113), (s, e) => ActionPotionSoin(), 270, 42);
            btnCombatBouclier = CreerActionButton("🛡️ Lever Bouclier", "-50% Dégâts, +Mana", Color.FromArgb(52, 73, 94), Color.FromArgb(70, 95, 120), (s, e) => ActionDefense(), 270, 42);
            btnCombatFuite = CreerActionButton("🏃 Tenter Fuite", "Quitter combat", Color.FromArgb(127, 140, 141), Color.FromArgb(149, 165, 166), (s, e) => ActionFuite(), 270, 42);
        }

        private DarkProgressBar AjouterJaugeSection(FlowLayoutPanel parent, string titre, Color deb, Color fin)
        {
            Label lbl = new Label
            {
                Text = titre,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(170, 175, 190),
                Width = 300,
                Height = 16,
                Margin = new Padding(0, 0, 0, 1)
            };
            parent.Controls.Add(lbl);

            DarkProgressBar pb = new DarkProgressBar
            {
                Width = 300,
                Height = 22,
                CouleurDebut = deb,
                CouleurFin = fin,
                Margin = new Padding(0, 0, 0, 6)
            };
            parent.Controls.Add(pb);
            return pb;
        }

        private DarkButton CreerActionButton(string titre, string sousTitre, Color baseCol, Color survolCol, EventHandler onClick, int largeur = 180, int hauteur = 50)
        {
            DarkButton btn = new DarkButton
            {
                Text = string.IsNullOrEmpty(sousTitre) ? titre : $"{titre}\n{sousTitre}",
                Size = new Size(largeur, hauteur),
                BackColor = baseCol,
                CouleurSurvol = survolCol,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btn.Click += onClick;
            return btn;
        }

        private Button CreerOngletCombatButton(string texte, CategorieCombat cat)
        {
            Button btn = new Button
            {
                Text = texte,
                AutoSize = true,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = (cat == categorieCombatActive) ? Color.FromArgb(192, 57, 43) : Color.FromArgb(35, 40, 52),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(2, 0, 2, 0),
                Padding = new Padding(8, 0, 8, 0)
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = (cat == categorieCombatActive) ? Color.FromArgb(231, 76, 60) : Color.FromArgb(60, 68, 85);
            btn.Click += (s, e) =>
            {
                categorieCombatActive = cat;
                ActualiserOngletsCombat();
                AfficherOngletCombat(cat);
            };
            return btn;
        }

        private void ActualiserOngletsCombat()
        {
            Button[] onglets = { btnOngletDonjons, btnOngletZones, btnOngletBossRegionaux, btnOngletBossMythiques, btnOngletBossCosmiques, btnOngletTout };
            CategorieCombat[] cats = { CategorieCombat.Donjons, CategorieCombat.Zones, CategorieCombat.BossRegionaux, CategorieCombat.BossMythiques, CategorieCombat.BossCosmiques, CategorieCombat.Tout };

            for (int i = 0; i < onglets.Length; i++)
            {
                if (onglets[i] == null) continue;
                bool active = (cats[i] == categorieCombatActive);
                onglets[i].BackColor = active ? Color.FromArgb(192, 57, 43) : Color.FromArgb(35, 40, 52);
                onglets[i].FlatAppearance.BorderColor = active ? Color.FromArgb(231, 76, 60) : Color.FromArgb(60, 68, 85);
            }
        }

        private void AfficherOngletCombat(CategorieCombat cat)
        {
            if (enCombat) return;

            pnlActionButtons.Controls.Clear();

            switch (cat)
            {
                case CategorieCombat.Donjons:
                    pnlActionButtons.Controls.AddRange(new Control[] {
                        CreerActionButton("💀 Labyrinthe Damnés", "4 Salles • Niv. 3+", Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182), (s, e) => LancerDonjon(CreerDonjonLabyrinthe())),
                        CreerActionButton("🌋 Faille Abyssale", "5 Salles • Niv. 6+", Color.FromArgb(211, 84, 0), Color.FromArgb(230, 126, 34), (s, e) => LancerDonjon(CreerDonjonFailleVolcan())),
                        CreerActionButton("🌌 Citadelle Céleste", "6 Salles • Raid Niv. 10+", Color.FromArgb(192, 57, 43), Color.FromArgb(231, 76, 60), (s, e) => LancerDonjon(CreerDonjonCitadelleChaos())),
                        CreerActionButton("📋 Infos & Raids", "Règles & Butins", Color.FromArgb(52, 73, 94), Color.FromArgb(70, 95, 120), (s, e) => OuvrirMenuDonjons())
                    });
                    break;

                case CategorieCombat.Zones:
                    pnlActionButtons.Controls.AddRange(new Control[] {
                        CreerActionButton("🌲 Forêt des Brumes", "Niv. 1-3 | Gobelins", Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113), (s, e) => ExplorerZone(TypeZone.Foret)),
                        CreerActionButton("💀 Catacombes Hantées", "Niv. 3-5 | Squelettes", Color.FromArgb(85, 95, 110), Color.FromArgb(110, 120, 140), (s, e) => ExplorerZone(TypeZone.Catacombes)),
                        CreerActionButton("❄️ Cime Glaciale", "Niv. 5-7 | Golems", Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), (s, e) => ExplorerZone(TypeZone.ForteresseGivre)),
                        CreerActionButton("🌋 Abîmes de Feu", "Niv. 7-9 | Drakes", Color.FromArgb(211, 84, 0), Color.FromArgb(230, 126, 34), (s, e) => ExplorerZone(TypeZone.Volcan)),
                        CreerActionButton("🌌 Tour Astrale", "Donjon Infini & Record", Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182), (s, e) => DefierTourAstrale())
                    });
                    break;

                case CategorieCombat.BossRegionaux:
                    pnlActionButtons.Controls.AddRange(new Control[] {
                        CreerActionButton("👑 Boss : Grok", "Seigneur Gobelin (Niv. 1+)", Color.FromArgb(180, 40, 40), Color.FromArgb(220, 60, 60), (s, e) => LancerCombat(new BossGrok())),
                        CreerActionButton("🐗 Boss : Gorrok", "Broyeur Sauvage (Niv. 2+)", Color.FromArgb(160, 50, 30), Color.FromArgb(200, 70, 40), (s, e) => LancerCombat(new BossGorrok())),
                        CreerActionButton("🗡️ Boss : Kragh", "Écorcheur Horde (Niv. 3+)", Color.FromArgb(140, 60, 20), Color.FromArgb(180, 80, 30), (s, e) => LancerCombat(new BossKragh())),
                        CreerActionButton("💀 Boss : Malakor", "Nécromancien (Niv. 3+)", Color.FromArgb(130, 25, 50), Color.FromArgb(170, 40, 70), (s, e) => LancerCombat(new BossMalakor())),
                        CreerActionButton("❄️ Boss : Skuldir", "Roi Cryomancien (Niv. 4+)", Color.FromArgb(30, 80, 130), Color.FromArgb(50, 110, 170), (s, e) => LancerCombat(new BossSkuldir())),
                        CreerActionButton("🧪 Boss : Zulgar", "Chaman Putride (Niv. 5+)", Color.FromArgb(40, 110, 60), Color.FromArgb(60, 140, 80), (s, e) => LancerCombat(new BossZulgar())),
                        CreerActionButton("🕷️ Boss : Vespera", "Reine Araignée (Niv. 5+)", Color.FromArgb(110, 30, 110), Color.FromArgb(150, 40, 150), (s, e) => LancerCombat(new BossVespera())),
                        CreerActionButton("❄️ Boss : Kaelas", "Archifée Blizzard (Niv. 6+)", Color.FromArgb(20, 90, 140), Color.FromArgb(40, 120, 180), (s, e) => LancerCombat(new BossKaelas())),
                        CreerActionButton("🧛 Boss : Balthazar", "Seigneur Vampire (Niv. 6+)", Color.FromArgb(120, 15, 40), Color.FromArgb(160, 25, 60), (s, e) => LancerCombat(new BossBalthazar())),
                        CreerActionButton("🦂 Boss : Kryll", "Seigneur Chitinique (Niv. 7+)", Color.FromArgb(100, 70, 20), Color.FromArgb(140, 95, 30), (s, e) => LancerCombat(new BossKryll()))
                    });
                    break;

                case CategorieCombat.BossMythiques:
                    pnlActionButtons.Controls.AddRange(new Control[] {
                        CreerActionButton("🌋 Boss : Magmarion", "Titan Magma (Niv. 7+)", Color.FromArgb(190, 50, 10), Color.FromArgb(230, 70, 20), (s, e) => LancerCombat(new BossMagmarion())),
                        CreerActionButton("🌲 Boss : Sylvana", "Matriarche Sylvestre (Niv. 8+)", Color.FromArgb(35, 100, 50), Color.FromArgb(50, 135, 70), (s, e) => LancerCombat(new BossSylvana())),
                        CreerActionButton("🪨 Boss : Obsidius", "Colosse Obsidienne (Niv. 8+)", Color.FromArgb(60, 50, 60), Color.FromArgb(90, 75, 90), (s, e) => LancerCombat(new BossObsidius())),
                        CreerActionButton("🐉 Boss : Dragon Ignis", "Titan Volcanique (Niv. 8+)", Color.FromArgb(180, 60, 10), Color.FromArgb(230, 80, 20), (s, e) => LancerCombat(new BossDragonIgnis())),
                        CreerActionButton("🌑 Boss : Nox", "Seigneur Ombres (Niv. 9+)", Color.FromArgb(45, 30, 65), Color.FromArgb(70, 50, 95), (s, e) => LancerCombat(new BossNox())),
                        CreerActionButton("🔥 Boss : Belial", "Démon Infernal (Niv. 9+)", Color.FromArgb(160, 30, 10), Color.FromArgb(210, 50, 20), (s, e) => LancerCombat(new BossBelial())),
                        CreerActionButton("⚔️ Boss : Thalor", "Guerrier Immortel (Niv. 10+)", Color.FromArgb(140, 90, 15), Color.FromArgb(185, 120, 25), (s, e) => LancerCombat(new BossThalor())),
                        CreerActionButton("💀 Boss : Mor'Gath", "Liche Démoniaque (Niv. 10+)", Color.FromArgb(90, 20, 90), Color.FromArgb(130, 40, 130), (s, e) => LancerCombat(new BossMorGath())),
                        CreerActionButton("🐍 Boss : Azkalith", "Reine Vipère (Niv. 10+)", Color.FromArgb(25, 110, 70), Color.FromArgb(40, 145, 90), (s, e) => LancerCombat(new BossAzkalith())),
                        CreerActionButton("🪨 Boss : Valdorak", "Colosse Granite (Niv. 11+)", Color.FromArgb(70, 80, 90), Color.FromArgb(100, 110, 125), (s, e) => LancerCombat(new BossValdorak()))
                    });
                    break;

                case CategorieCombat.BossCosmiques:
                    pnlActionButtons.Controls.AddRange(new Control[] {
                        CreerActionButton("⚡ Boss : Zephyros", "Archi-Tempête (Niv. 11+)", Color.FromArgb(30, 110, 140), Color.FromArgb(45, 145, 185), (s, e) => LancerCombat(new BossZephyros())),
                        CreerActionButton("🐺 Boss : Fenrir", "Lune de Sang (Niv. 12+)", Color.FromArgb(150, 20, 25), Color.FromArgb(195, 30, 40), (s, e) => LancerCombat(new BossFenrirLuneSang())),
                        CreerActionButton("🌌 ULTIME : Ch. Néant", "Néant Primordial (Niv. 12+)", Color.FromArgb(90, 15, 45), Color.FromArgb(145, 30, 70), (s, e) => LancerCombat(new BossChevalierDuNeant())),
                        CreerActionButton("☀️ Boss : Archonte", "Flamme Céleste (Niv. 13+)", Color.FromArgb(180, 120, 15), Color.FromArgb(225, 155, 25), (s, e) => LancerCombat(new BossArchonteSolaire())),
                        CreerActionButton("🐙 Boss : Kraken", "Terreur Abyssale (Niv. 14+)", Color.FromArgb(15, 60, 90), Color.FromArgb(25, 90, 130), (s, e) => LancerCombat(new BossKrakenAbyssal())),
                        CreerActionButton("🌌 Boss : Xanthos", "Gardien Cosmique (Niv. 15+)", Color.FromArgb(110, 45, 130), Color.FromArgb(150, 65, 170), (s, e) => LancerCombat(new BossXanthos())),
                        CreerActionButton("⏳ Boss : Chronos", "Seigneur du Temps (Niv. 15+)", Color.FromArgb(135, 105, 30), Color.FromArgb(175, 140, 45), (s, e) => LancerCombat(new BossChronos())),
                        CreerActionButton("☄️ Boss : Léviathan", "Galaxies Oubliées (Niv. 16+)", Color.FromArgb(70, 25, 110), Color.FromArgb(105, 40, 155), (s, e) => LancerCombat(new BossLeviathanStellaire())),
                        CreerActionButton("💀 Boss : Abaddon", "Pourfendeur Mondes (Niv. 18+)", Color.FromArgb(130, 15, 15), Color.FromArgb(175, 25, 25), (s, e) => LancerCombat(new BossAbaddon())),
                        CreerActionButton("👑 DEUS EX NIHILO", "Architecte Chaos (Niv. 20+)", Color.FromArgb(110, 10, 60), Color.FromArgb(170, 20, 90), (s, e) => LancerCombat(new BossDeusExNihilo()))
                    });
                    break;

                case CategorieCombat.Tout:
                    pnlActionButtons.Controls.AddRange(new Control[] {
                        // Donjons (3)
                        CreerActionButton("💀 Labyrinthe Damnés", "Niv. 3+ • 4 Salles", Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182), (s, e) => LancerDonjon(CreerDonjonLabyrinthe())),
                        CreerActionButton("🌋 Faille Abyssale", "Niv. 6+ • 5 Salles", Color.FromArgb(211, 84, 0), Color.FromArgb(230, 126, 34), (s, e) => LancerDonjon(CreerDonjonFailleVolcan())),
                        CreerActionButton("🌌 Citadelle Céleste", "Raid 10+ • 6 Salles", Color.FromArgb(192, 57, 43), Color.FromArgb(231, 76, 60), (s, e) => LancerDonjon(CreerDonjonCitadelleChaos())),

                        // Zones (5)
                        CreerActionButton("🌲 Forêt des Brumes", "Niv. 1-3 | Gobelins", Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113), (s, e) => ExplorerZone(TypeZone.Foret)),
                        CreerActionButton("💀 Catacombes Hantées", "Niv. 3-5 | Squelettes", Color.FromArgb(85, 95, 110), Color.FromArgb(110, 120, 140), (s, e) => ExplorerZone(TypeZone.Catacombes)),
                        CreerActionButton("❄️ Cime Glaciale", "Niv. 5-7 | Golems", Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), (s, e) => ExplorerZone(TypeZone.ForteresseGivre)),
                        CreerActionButton("🌋 Abîmes de Feu", "Niv. 7-9 | Drakes", Color.FromArgb(211, 84, 0), Color.FromArgb(230, 126, 34), (s, e) => ExplorerZone(TypeZone.Volcan)),
                        CreerActionButton("🌌 Tour Astrale", "Donjon Infini & Record", Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182), (s, e) => DefierTourAstrale()),

                        // Boss Régionaux (10)
                        CreerActionButton("👑 Boss : Grok", "Seigneur Gobelin", Color.FromArgb(180, 40, 40), Color.FromArgb(220, 60, 60), (s, e) => LancerCombat(new BossGrok())),
                        CreerActionButton("🐗 Boss : Gorrok", "Broyeur Sauvage", Color.FromArgb(160, 50, 30), Color.FromArgb(200, 70, 40), (s, e) => LancerCombat(new BossGorrok())),
                        CreerActionButton("🗡️ Boss : Kragh", "Écorcheur Horde", Color.FromArgb(140, 60, 20), Color.FromArgb(180, 80, 30), (s, e) => LancerCombat(new BossKragh())),
                        CreerActionButton("💀 Boss : Malakor", "Nécromancien", Color.FromArgb(130, 25, 50), Color.FromArgb(170, 40, 70), (s, e) => LancerCombat(new BossMalakor())),
                        CreerActionButton("❄️ Boss : Skuldir", "Roi Cryomancien", Color.FromArgb(30, 80, 130), Color.FromArgb(50, 110, 170), (s, e) => LancerCombat(new BossSkuldir())),
                        CreerActionButton("🧪 Boss : Zulgar", "Chaman Putride", Color.FromArgb(40, 110, 60), Color.FromArgb(60, 140, 80), (s, e) => LancerCombat(new BossZulgar())),
                        CreerActionButton("🕷️ Boss : Vespera", "Reine Araignée", Color.FromArgb(110, 30, 110), Color.FromArgb(150, 40, 150), (s, e) => LancerCombat(new BossVespera())),
                        CreerActionButton("❄️ Boss : Kaelas", "Archifée Blizzard", Color.FromArgb(20, 90, 140), Color.FromArgb(40, 120, 180), (s, e) => LancerCombat(new BossKaelas())),
                        CreerActionButton("🧛 Boss : Balthazar", "Seigneur Vampire", Color.FromArgb(120, 15, 40), Color.FromArgb(160, 25, 60), (s, e) => LancerCombat(new BossBalthazar())),
                        CreerActionButton("🦂 Boss : Kryll", "Seigneur Vermine", Color.FromArgb(100, 70, 20), Color.FromArgb(140, 95, 30), (s, e) => LancerCombat(new BossKryll())),

                        // Boss Mythiques (10)
                        CreerActionButton("🌋 Boss : Magmarion", "Titan Magma", Color.FromArgb(190, 50, 10), Color.FromArgb(230, 70, 20), (s, e) => LancerCombat(new BossMagmarion())),
                        CreerActionButton("🌲 Boss : Sylvana", "Matriarche Sylvestre", Color.FromArgb(35, 100, 50), Color.FromArgb(50, 135, 70), (s, e) => LancerCombat(new BossSylvana())),
                        CreerActionButton("🪨 Boss : Obsidius", "Colosse Obsidienne", Color.FromArgb(60, 50, 60), Color.FromArgb(90, 75, 90), (s, e) => LancerCombat(new BossObsidius())),
                        CreerActionButton("🐉 Boss : Dragon Ignis", "Titan Volcanique", Color.FromArgb(180, 60, 10), Color.FromArgb(230, 80, 20), (s, e) => LancerCombat(new BossDragonIgnis())),
                        CreerActionButton("🌑 Boss : Nox", "Seigneur Ombres", Color.FromArgb(45, 30, 65), Color.FromArgb(70, 50, 95), (s, e) => LancerCombat(new BossNox())),
                        CreerActionButton("🔥 Boss : Belial", "Démon Infernal", Color.FromArgb(160, 30, 10), Color.FromArgb(210, 50, 20), (s, e) => LancerCombat(new BossBelial())),
                        CreerActionButton("⚔️ Boss : Thalor", "Guerrier Immortel", Color.FromArgb(140, 90, 15), Color.FromArgb(185, 120, 25), (s, e) => LancerCombat(new BossThalor())),
                        CreerActionButton("💀 Boss : Mor'Gath", "Liche Démoniaque", Color.FromArgb(90, 20, 90), Color.FromArgb(130, 40, 130), (s, e) => LancerCombat(new BossMorGath())),
                        CreerActionButton("🐍 Boss : Azkalith", "Reine Vipère", Color.FromArgb(25, 110, 70), Color.FromArgb(40, 145, 90), (s, e) => LancerCombat(new BossAzkalith())),
                        CreerActionButton("🪨 Boss : Valdorak", "Colosse Granite", Color.FromArgb(70, 80, 90), Color.FromArgb(100, 110, 125), (s, e) => LancerCombat(new BossValdorak())),

                        // Boss Cosmiques (10)
                        CreerActionButton("⚡ Boss : Zephyros", "Archi-Tempête", Color.FromArgb(30, 110, 140), Color.FromArgb(45, 145, 185), (s, e) => LancerCombat(new BossZephyros())),
                        CreerActionButton("🐺 Boss : Fenrir", "Lune de Sang", Color.FromArgb(150, 20, 25), Color.FromArgb(195, 30, 40), (s, e) => LancerCombat(new BossFenrirLuneSang())),
                        CreerActionButton("🌌 ULTIME : Ch. Néant", "Néant Primordial", Color.FromArgb(90, 15, 45), Color.FromArgb(145, 30, 70), (s, e) => LancerCombat(new BossChevalierDuNeant())),
                        CreerActionButton("☀️ Boss : Archonte", "Flamme Céleste", Color.FromArgb(180, 120, 15), Color.FromArgb(225, 155, 25), (s, e) => LancerCombat(new BossArchonteSolaire())),
                        CreerActionButton("🐙 Boss : Kraken", "Terreur Abyssale", Color.FromArgb(15, 60, 90), Color.FromArgb(25, 90, 130), (s, e) => LancerCombat(new BossKrakenAbyssal())),
                        CreerActionButton("🌌 Boss : Xanthos", "Gardien Cosmique", Color.FromArgb(110, 45, 130), Color.FromArgb(150, 65, 170), (s, e) => LancerCombat(new BossXanthos())),
                        CreerActionButton("⏳ Boss : Chronos", "Seigneur du Temps", Color.FromArgb(135, 105, 30), Color.FromArgb(175, 140, 45), (s, e) => LancerCombat(new BossChronos())),
                        CreerActionButton("☄️ Boss : Léviathan", "Galaxies Oubliées", Color.FromArgb(70, 25, 110), Color.FromArgb(105, 40, 155), (s, e) => LancerCombat(new BossLeviathanStellaire())),
                        CreerActionButton("💀 Boss : Abaddon", "Pourfendeur Mondes", Color.FromArgb(130, 15, 15), Color.FromArgb(175, 25, 25), (s, e) => LancerCombat(new BossAbaddon())),
                        CreerActionButton("👑 DEUS EX NIHILO", "Architecte Chaos", Color.FromArgb(110, 10, 60), Color.FromArgb(170, 20, 90), (s, e) => LancerCombat(new BossDeusExNihilo()))
                    });
                    break;
            }
        }

        private Button CreerTopPillButton(string texte, Color bg, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = texte,
                Size = new Size(110, 36),
                Margin = new Padding(4, 2, 4, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 90, 110);
            btn.Click += onClick;
            return btn;
        }

        // ==============================================================
        // EFFETS VISUELS & ANIMATIONS HD
        // ==============================================================
        private async void SecouerEcran(int intensite = 7, int iterations = 6)
        {
            try
            {
                Point pos = this.Location;
                for (int i = 0; i < iterations; i++)
                {
                    int dx = (i % 2 == 0) ? intensite : -intensite;
                    int dy = (i % 3 == 0) ? -intensite / 2 : intensite / 2;
                    this.Location = new Point(pos.X + dx, pos.Y + dy);
                    await System.Threading.Tasks.Task.Delay(20);
                }
                this.Location = pos;
            }
            catch (Exception ex) { JournalErreurs.Enregistrer(ex, "SecouerEcran"); }
        }

        private async void DeclencherFlash(Color couleur, int dureeMs = 160)
        {
            try
            {
                if (pnlAreneCard == null) return;
                Color bgOrig = pnlAreneCard.BackColor;
                pnlAreneCard.BackColor = couleur;
                await System.Threading.Tasks.Task.Delay(dureeMs);
                pnlAreneCard.BackColor = bgOrig;
            }
            catch (Exception ex) { JournalErreurs.Enregistrer(ex, "DeclencherFlash"); }
        }

        private async void AfficherImpactVisuel(string texte, Color couleur, int dureeMs = 1200)
        {
            try
            {
                if (lblImpactVisuel == null) return;
                int jeton = ++jetonImpact;
                lblImpactVisuel.Text = texte;
                lblImpactVisuel.ForeColor = couleur;
                lblImpactVisuel.Visible = true;
                await System.Threading.Tasks.Task.Delay(dureeMs);
                if (jeton == jetonImpact)
                {
                    lblImpactVisuel.Visible = false;
                }
            }
            catch (Exception ex) { JournalErreurs.Enregistrer(ex, "AfficherImpactVisuel"); }
        }

        private void InitialiserTimerPulsation()
        {
            timerPulsation = new System.Windows.Forms.Timer { Interval = 380 };
            timerPulsation.Tick += (s, e) =>
            {
                pulsationPhase = !pulsationPhase;

                // 1. Bouton Ultime prêt (100%)
                if (hero != null && hero.JaugeUltime >= 100 && btnCombatUltime != null)
                {
                    btnCombatUltime.BackColor = pulsationPhase ? Color.FromArgb(241, 196, 15) : Color.FromArgb(230, 126, 34);
                    btnCombatUltime.ForeColor = pulsationPhase ? Color.Black : Color.White;
                }

                // 2. Boss enragé (< 35% PV)
                if (enCombat && ennemiActuel != null && ennemiActuel.EstBoss && ennemiActuel.PVActuels <= ennemiActuel.PVMax * 0.35 && lblEnnemiBadge != null)
                {
                    lblEnnemiBadge.Text = pulsationPhase ? "🔥 BOSS ENRAGÉ ! 🔥" : "💀 DANGER MORTEL 💀";
                    lblEnnemiBadge.ForeColor = pulsationPhase ? Color.FromArgb(255, 75, 75) : Color.FromArgb(255, 160, 20);
                }

                // 3. Avertissement attaque chargée de Boss
                if (enCombat && bossChargeAttaque && lblBandeauLieu != null)
                {
                    lblBandeauLieu.ForeColor = pulsationPhase ? Color.Red : Color.Gold;
                }
            };
            timerPulsation.Start();
        }

        // ==============================================================
        // JOURNAL ET HISTORIQUE
        // ==============================================================
        public void AjouterLog(string texte, Color couleur)
        {
            rtbJournal.SelectionStart = rtbJournal.TextLength;
            rtbJournal.SelectionLength = 0;
            rtbJournal.SelectionColor = couleur;
            rtbJournal.AppendText(texte + "\n");
            rtbJournal.SelectionColor = rtbJournal.ForeColor;
            rtbJournal.ScrollToCaret();
        }

        // ==============================================================
        // MISE À JOUR VISUELLE
        // ==============================================================
        private void MettreAJourInterface()
        {
            if (hero == null) return;

            lblHeroNom.Text = $"⭐ {hero.Nom}";
            lblHeroClasse.Text = $"NIV. {hero.Niveau}  •  {hero.Classe.ToString().ToUpper()}\n{hero.ObtenirRangGuilde()}";

            // Barres
            pbHeroPV.Maximum = hero.PVMaxTotal;
            pbHeroPV.Valeur = hero.PVActuels;
            pbHeroPV.TexteCentral = $"{hero.PVActuels} / {hero.PVMaxTotal} PV";

            pbHeroMana.Maximum = hero.ManaMaxTotal;
            pbHeroMana.Valeur = hero.ManaActuel;
            pbHeroMana.TexteCentral = $"{hero.ManaActuel} / {hero.ManaMaxTotal} Mana";

            pbHeroXP.Maximum = hero.XPRequisPourNiveau;
            pbHeroXP.Valeur = hero.XP;
            pbHeroXP.TexteCentral = $"{hero.XP} / {hero.XPRequisPourNiveau} XP";

            pbHeroUltime.Maximum = 100;
            pbHeroUltime.Valeur = hero.JaugeUltime;
            pbHeroUltime.TexteCentral = hero.JaugeUltime >= 100 ? "🌟 ULTIME PRÊT (100%) !" : $"Ultime : {hero.JaugeUltime}%";

            if (hero.JaugeUltime >= 100)
            {
                btnCombatUltime.BackColor = Color.FromArgb(241, 196, 15);
                btnCombatUltime.ForeColor = Color.Black;
            }
            else
            {
                btnCombatUltime.BackColor = Color.FromArgb(120, 60, 10);
                btnCombatUltime.ForeColor = Color.White;
            }

            // Ressources
            lblRessources.Text = $"💰 Trésor : {hero.Or} Or   💎 Pierres : {hero.PierresDeForge}   🏺 Sceaux : {hero.SceauxDeGuilde}   🌌 Éclats : {hero.ObtenirMateriau("Éclat Astral")}\n" +
                                $"⭐ Réputation Guilde : {hero.ReputationGuilde} pts\n" +
                                $"🧪 Soin : {hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure)}   " +
                                $"💧 Mana : {hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMineure)}   " +
                                $"💣 Bombes : {hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire)}";

            // Stats
            lblStatsCombat.Text = $"⚔️ Attaque : {hero.AttaqueTotale}   🛡️ Défense : {hero.DefenseTotale}\n" +
                                 $"💥 Critique : {hero.ChanceCritiqueTotale}%\n" +
                                 $"🤸 Esquive : {hero.ChanceEsquiveTotale}%   |   🧛 Vol de Vie : {hero.VampirismeTotal}%";

            // Points d'attributs
            lblPointsAttributs.Text = hero.PointsCaracteristiques > 0
                ? $"⭐ {hero.PointsCaracteristiques} POINTS NON ALLOUÉS !"
                : "Caractéristiques :";

            pnlBoutonsStats.Controls.Clear();
            if (hero.PointsCaracteristiques > 0)
            {
                pnlBoutonsStats.Controls.Add(CreerStatPill("FOR +1", () => { hero.Force++; hero.PointsCaracteristiques--; }));
                pnlBoutonsStats.Controls.Add(CreerStatPill("AGI +1", () => { hero.Agilite++; hero.PointsCaracteristiques--; }));
                pnlBoutonsStats.Controls.Add(CreerStatPill("END +1", () => { hero.Endurance++; hero.PointsCaracteristiques--; }));
                pnlBoutonsStats.Controls.Add(CreerStatPill("INT +1", () => { hero.Intelligence++; hero.PointsCaracteristiques--; }));
            }
            else
            {
                Label lblAttrInfo = new Label
                {
                    Text = $"FOR: {hero.Force}  AGI: {hero.Agilite}  END: {hero.Endurance}  INT: {hero.Intelligence}",
                    ForeColor = Color.FromArgb(170, 180, 195),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 0, 0)
                };
                pnlBoutonsStats.Controls.Add(lblAttrInfo);
            }

            // Équipement & Sort équipé
            string sortEquipeInfo = hero.CompetenceEquipee != null
                ? $"\n🔮 Sort     : {hero.CompetenceEquipee.Icone} {hero.CompetenceEquipee.Nom} ({hero.CompetenceEquipee.CoutMana} Mana)"
                : "\n🔮 Sort     : Aucun sort forgé équipé";

            lblEquipementResume.Text =
                $"🗡️ Arme     : {FormatItem(hero.ArmeEquipee)}\n" +
                $"🛡️ Armure   : {FormatItem(hero.ArmureEquipee)}\n" +
                $"🪖 Heaume   : {FormatItem(hero.CasqueEquipe)}\n" +
                $"💍 Anneau   : {FormatItem(hero.AnneauEquipe)}\n" +
                $"📿 Amulette : {FormatItem(hero.AmuletteEquipee)}" +
                sortEquipeInfo;

            // Synchronisation du bouton de rendu direct des quêtes
            if (!enCombat && donjonActuel == null && !enTourAstrale && pnlServicesButtons != null)
            {
                int nbQuetesFinies = hero.QuetesActives.Count(q => q.EstTerminee);
                if (nbQuetesFinies > 0)
                {
                    btnReclamerPrimesDirect.Text = $"🎁 RENDRE QUÊTES ({nbQuetesFinies} FINIES)\nToucher primes royales";
                    if (!pnlServicesButtons.Controls.Contains(btnReclamerPrimesDirect))
                    {
                        pnlServicesButtons.Controls.Add(btnReclamerPrimesDirect);
                        pnlServicesButtons.Controls.SetChildIndex(btnReclamerPrimesDirect, 0);
                    }
                }
                else
                {
                    if (pnlServicesButtons.Controls.Contains(btnReclamerPrimesDirect))
                    {
                        pnlServicesButtons.Controls.Remove(btnReclamerPrimesDirect);
                    }
                }
            }

            // Ennemi
            if (enCombat && ennemiActuel != null)
            {
                lblAreneTitre.Text = "⚔️ ARÈNE DE COMBAT";
                lblAreneTitre.ForeColor = Color.FromArgb(231, 76, 60);

                lblEnnemiNom.Text = ennemiActuel.Nom;
                lblEnnemiBadge.Text = ennemiActuel.EstBoss ? "👑 BOSS LÉGENDAIRE" : "👹 MONSTRE";
                lblEnnemiBadge.ForeColor = ennemiActuel.EstBoss ? Color.FromArgb(241, 196, 15) : Color.FromArgb(230, 126, 34);

                pbEnnemiPV.Maximum = ennemiActuel.PVMax;
                pbEnnemiPV.Valeur = ennemiActuel.PVActuels;
                pbEnnemiPV.TexteCentral = $"{ennemiActuel.PVActuels} / {ennemiActuel.PVMax} PV";

                List<string> statuts = new List<string>();
                if (postureDefense) statuts.Add("🛡️ Garde (-50% dégâts)");
                if (buffDefenseTours > 0) statuts.Add("🪨 Peau de Pierre");
                if (joueurBruleTours > 0) statuts.Add($"🔥 Brûlure ({joueurBruleTours}t)");
                if (joueurPoisonTours > 0) statuts.Add($"🧪 Poison ({joueurPoisonTours}t)");
                if (joueurStunTours > 0) statuts.Add($"💫 Étourdi ({joueurStunTours}t)");
                if (hero.BuffReposCombatsRestants > 0) statuts.Add("✨ Repos (+25% XP)");

                string statutsStr = statuts.Count > 0 ? $" | Statuts : [{string.Join(", ", statuts)}]" : "";
                lblEnnemiFaiblesseLore.Text = $"Faiblesse : {ennemiActuel.Faiblesse}  •  {ennemiActuel.Lore}{statutsStr}";
            }
            else
            {
                lblAreneTitre.Text = "🏰 VILLAGE DE VAL-SEREIN";
                lblAreneTitre.ForeColor = Color.FromArgb(46, 204, 113);

                lblEnnemiNom.Text = "Havre de paix et de repos";
                lblEnnemiBadge.Text = "[ZONE SÛRE]";
                lblEnnemiBadge.ForeColor = Color.FromArgb(46, 204, 113);

                pbEnnemiPV.Maximum = 100;
                pbEnnemiPV.Valeur = 0;
                pbEnnemiPV.TexteCentral = "Aucun combat actif";

                lblEnnemiFaiblesseLore.Text = "Vous êtes en sécurité au village. Choisissez une destination ci-dessous pour partir en quête !";
            }
        }

        private string FormatItem(Equipement? eq)
        {
            if (eq == null) return "Aucun";
            return $"{eq.ObtenirNomComplet()} [{eq.RareteItem}]";
        }

        private Button CreerStatPill(string text, Action onAdd)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(68, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(2, 0, 2, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                onAdd();
                MettreAJourInterface();
            };
            return btn;
        }

        // ==============================================================
        // BASCULE DE MODE PAR CHANGEMENT DYNAMIQUE PROPRE
        // ==============================================================
        private void BasculerModeCombat(bool actif)
        {
            enCombat = actif;

            if (actif)
            {
                // Mode Combat : Séparation claire entre utilitaires/soins à gauche et attaques/compétences à droite
                lblServicesTitre.Text = "🛡️ TACTIQUE & SOINS";
                lblServicesTitre.ForeColor = Color.FromArgb(52, 152, 219);
                pnlServicesButtons.Controls.Clear();
                pnlServicesButtons.Controls.AddRange(new Control[] {
                    btnCombatSoin, btnCombatBouclier, btnCombatFuite
                });

                lblCombatsTitre.Text = "⚔️ ACTIONS DE COMBAT";
                lblCombatsTitre.ForeColor = Color.FromArgb(231, 76, 60);
                pnlCombatOnglets.Visible = false;

                pnlActionButtons.Controls.Clear();
                pnlActionButtons.Controls.AddRange(new Control[] {
                    btnCombatAttaque, btnCombatLourde, btnCombatSort
                });

                if (hero.CompetenceEquipee != null)
                {
                    var sort = hero.CompetenceEquipee;
                    btnCombatSortForge = CreerActionButton($"{sort.Icone} {sort.Nom}", $"{sort.CoutMana} Mana • {sort.Effet}", Color.FromArgb(108, 52, 131), Color.FromArgb(142, 68, 173), (s, e) => ActionCompetenceCraft(), 155, 52);
                    pnlActionButtons.Controls.Add(btnCombatSortForge);
                }

                pnlActionButtons.Controls.Add(btnCombatUltime);
            }
            else
            {
                // Mode Village : Services et Gestion à gauche, Expéditions & Donjons à droite avec onglets
                lblServicesTitre.Text = "🏛️ SERVICES DU VILLAGE";
                lblServicesTitre.ForeColor = Color.FromArgb(241, 196, 15);
                pnlServicesButtons.Controls.Clear();

                int nbQuetesFinies = hero.QuetesActives.Count(q => q.EstTerminee);
                if (nbQuetesFinies > 0)
                {
                    btnReclamerPrimesDirect.Text = $"🎁 RENDRE QUÊTES ({nbQuetesFinies} FINIES)\nToucher primes royales";
                    pnlServicesButtons.Controls.Add(btnReclamerPrimesDirect);
                }

                pnlServicesButtons.Controls.AddRange(new Control[] {
                    btnServiceSac, btnServiceGuilde, btnServiceForge, btnServiceAuberge
                });

                lblCombatsTitre.Text = "⚔️ EXPÉDITIONS & COMBATS";
                lblCombatsTitre.ForeColor = Color.FromArgb(231, 76, 60);
                pnlCombatOnglets.Visible = true;

                ActualiserOngletsCombat();
                AfficherOngletCombat(categorieCombatActive);
            }

            MettreAJourInterface();
        }

        private void BasculerModeVillage()
        {
            donjonActuel = null;
            indexSalleDonjon = 0;
            BasculerModeCombat(false);
            lblBandeauLieu.Text = "📍 Emplacement : 🏰 Village de Val-Serein (Zone Sûre)";
            lblBandeauLieu.ForeColor = Color.FromArgb(46, 204, 113);
        }

        // ==============================================================
        // COMBAT AVEC EFFETS VISUELS & MÉCANIQUES AVANCÉES
        // ==============================================================
        public void LancerCombat(Monstre ennemi)
        {
            ennemiActuel = ennemi;
            buffAttaqueTours = 0;
            buffDefenseTours = 0;
            postureDefense = false;
            joueurBruleTours = 0;
            joueurPoisonTours = 0;
            joueurStunTours = 0;
            bossChargeAttaque = false;

            lblBandeauLieu.Text = $"📍 En combat contre : {ennemi.Nom} !";
            lblBandeauLieu.ForeColor = Color.FromArgb(231, 76, 60);

            AjouterLog("\n=======================================================", Color.FromArgb(70, 75, 90));
            AjouterLog($"⚔️ UN COMBAT S'ENGAGE CONTRE {ennemi.Nom.ToUpper()} !", Color.FromArgb(241, 196, 15));
            if (!string.IsNullOrEmpty(ennemi.CriDeGuerre))
            {
                AjouterLog($"📢 \"{ennemi.CriDeGuerre}\"", Color.FromArgb(235, 87, 87));
            }
            AjouterLog("=======================================================", Color.FromArgb(70, 75, 90));

            SecouerEcran(6, 4);
            DeclencherFlash(Color.FromArgb(60, 20, 25), 140);
            AfficherImpactVisuel("⚔️ DÉBUT DU COMBAT !", Color.Orange);

            BasculerModeCombat(true);
        }

        private bool VerifierStunJoueur()
        {
            if (joueurStunTours > 0)
            {
                joueurStunTours--;
                AjouterLog($"💫 Vous êtes étourdi et incapable d'attaquer ce tour ! ({joueurStunTours} tour restant)", Color.FromArgb(241, 196, 15));
                AfficherImpactVisuel("💫 ÉTOURDI !", Color.Gold);
                TourEnnemi();
                return true;
            }
            return false;
        }

        private void ActionAttaqueNormale()
        {
            if (!enCombat || ennemiActuel == null) return;
            if (VerifierStunJoueur()) return;

            int atkEff = hero.AttaqueTotale + (buffAttaqueTours > 0 ? 10 : 0);
            int variance = rng.Next(-2, 3);
            int degats = Math.Max(4, (atkEff + variance) - ennemiActuel.Defense);

            bool crit = rng.Next(100) < hero.ChanceCritiqueTotale;
            if (crit)
            {
                degats = (int)(degats * 1.75);
                AjouterLog($"💥 COUP CRITIQUE ! Vous tranchez violemment pour {degats} dégâts !", Color.FromArgb(241, 196, 15));
                SecouerEcran(8, 6);
                DeclencherFlash(Color.FromArgb(90, 80, 20), 140);
                AfficherImpactVisuel($"💥 CRITIQUE ! -{degats}", Color.Gold);
            }
            else
            {
                AjouterLog($"⚔️ Vous attaquez {ennemiActuel.Nom} et infligez {degats} dégâts.", Color.White);
                AfficherImpactVisuel($"⚔️ ATTAQUE -{degats}", Color.White);
            }

            if (hero.VampirismeTotal > 0)
            {
                int soin = Math.Max(1, (degats * hero.VampirismeTotal) / 100);
                hero.Soigner(soin);
                AjouterLog($"🧛 Vol de Vie : vous siphonnez {soin} PV !", Color.FromArgb(187, 134, 252));
            }

            ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
            hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 15);

            FinTourJoueur();
        }

        private void ActionAttaqueLourde()
        {
            if (!enCombat || ennemiActuel == null) return;
            if (VerifierStunJoueur()) return;

            if (rng.Next(100) < 20)
            {
                AjouterLog($"❌ Votre attaque lourde est trop lente ! {ennemiActuel.Nom} esquive !", Color.FromArgb(235, 87, 87));
                AfficherImpactVisuel("❌ ESQUIVÉ !", Color.FromArgb(235, 87, 87));
            }
            else
            {
                int degats = (int)(hero.AttaqueTotale * 1.6) - ennemiActuel.Defense;
                degats = Math.Max(8, degats);
                ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                AjouterLog($"🔨 FRACAS TITANESQUE ! Vous ébranlez {ennemiActuel.Nom} pour {degats} dégâts !", Color.FromArgb(255, 121, 198));
                SecouerEcran(11, 7);
                DeclencherFlash(Color.FromArgb(85, 35, 95), 160);
                AfficherImpactVisuel($"🔨 FRACAS ! -{degats}", Color.FromArgb(255, 121, 198));
                hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 20);
            }

            FinTourJoueur();
        }

        private void ActionCompetence()
        {
            if (!enCombat || ennemiActuel == null) return;
            if (VerifierStunJoueur()) return;

            int cout = 25;
            if (hero.ManaActuel < cout)
            {
                MessageBox.Show("Mana insuffisant (25 Mana requis) !", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            hero.ManaActuel -= cout;
            int degats = 0;

            switch (hero.Classe)
            {
                case ClasseType.Guerrier:
                    degats = (int)(hero.AttaqueTotale * 1.8) + 12;
                    AjouterLog($"💥 BRISE-CRÂNE SISMIQUE ! Frappe brutale de {degats} dégâts !", Color.FromArgb(230, 126, 34));
                    SecouerEcran(12, 7);
                    DeclencherFlash(Color.FromArgb(90, 50, 15), 160);
                    AfficherImpactVisuel($"💥 BRISE-CRÂNE -{degats}", Color.Orange);
                    break;
                case ClasseType.Mage:
                    degats = (int)(hero.AttaqueTotale * 2.2) + 16;
                    AjouterLog($"🔥 CATACLYSME PYROTECHNIQUE ! Une onde de flammes consume l'ennemi pour {degats} dégâts !", Color.FromArgb(231, 76, 60));
                    SecouerEcran(10, 6);
                    DeclencherFlash(Color.FromArgb(95, 30, 20), 160);
                    AfficherImpactVisuel($"🔥 PYROTECHNIE -{degats}", Color.OrangeRed);
                    break;
                case ClasseType.Rodeur:
                    degats = (int)(hero.AttaqueTotale * 1.9) + 10;
                    AjouterLog($"🏹 FLÈCHE DU FAUCON PERFORANTE ! Tir meurtrier pour {degats} dégâts !", Color.FromArgb(46, 204, 113));
                    DeclencherFlash(Color.FromArgb(20, 80, 40), 150);
                    AfficherImpactVisuel($"🏹 FLÈCHE FAUCON -{degats}", Color.LightGreen);
                    break;
                case ClasseType.Paladin:
                    degats = (int)(hero.AttaqueTotale * 1.5) + 12;
                    hero.Soigner(35);
                    AjouterLog($"✨ ÉGIDE SACRÉE ! {degats} dégâts divins et +35 PV restaurés !", Color.FromArgb(241, 196, 15));
                    DeclencherFlash(Color.FromArgb(95, 85, 20), 160);
                    AfficherImpactVisuel($"✨ ÉGIDE DIVINE -{degats}", Color.Gold);
                    break;
                case ClasseType.Necromancien:
                    degats = (int)(hero.AttaqueTotale * 1.6) + 14;
                    hero.Soigner(degats / 2);
                    AjouterLog($"💀 SUPPLICATION DES OMBRES ! {degats} dégâts occultes et +{degats / 2} PV volés !", Color.FromArgb(155, 89, 182));
                    DeclencherFlash(Color.FromArgb(70, 20, 90), 160);
                    AfficherImpactVisuel($"💀 SIPHON OMBRE -{degats}", Color.MediumPurple);
                    break;
            }

            ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
            hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 20);

            FinTourJoueur();
        }

        private void ActionCompetenceCraft()
        {
            if (!enCombat || ennemiActuel == null || hero.CompetenceEquipee == null) return;
            if (VerifierStunJoueur()) return;

            var sort = hero.CompetenceEquipee;
            if (hero.ManaActuel < sort.CoutMana)
            {
                MessageBox.Show($"Mana insuffisant ({sort.CoutMana} Mana requis) !", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            hero.ManaActuel -= sort.CoutMana;
            int baseDmg = (int)(hero.AttaqueTotale * sort.MultiplicateurDegats);
            int degats = Math.Max(12, baseDmg - ennemiActuel.Defense);

            string effetTxt = "";
            switch (sort.Effet)
            {
                case TypeEffetCompetence.Saignement:
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    effetTxt = $"🩸 HÉMORRAGIE PROFONDE ! {ennemiActuel.Nom} est lacéré et perd son sang !";
                    break;
                case TypeEffetCompetence.Brulure:
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    effetTxt = $"🔥 BRAISES ARDENTES ! L'onde incendiaire calcine l'ennemi !";
                    break;
                case TypeEffetCompetence.Poison:
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    effetTxt = $"🧪 VENIN NOIR ! Les toxines rongent la chair de l'adversaire !";
                    break;
                case TypeEffetCompetence.Etourdissement:
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    effetTxt = $"⚡ CHOC SISMIQUE ! L'ennemi est sonné par la violence de l'onde de choc !";
                    break;
                case TypeEffetCompetence.SoinEtDegats:
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    int soin = Math.Min(sort.ValeurEffet, hero.PVMaxTotal - hero.PVActuels);
                    hero.Soigner(soin);
                    effetTxt = $"✨ ÉCLIPSE SACRÉE ! Vous régénérez +{soin} PV grâce à la lumière céleste !";
                    break;
                case TypeEffetCompetence.Bouclier:
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    buffDefenseTours = 3;
                    effetTxt = $"🛡️ CHAMP DE FORCE MYSTIQUE ACTIVÉ (+50% Défense pour 3 tours) !";
                    break;
                case TypeEffetCompetence.PerforantArmure:
                    degats = Math.Max(25, baseDmg); // Ignore la défense ennemie
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    effetTxt = $"🗡️ TRANCHE-RÉALITÉ DU NÉANT ! L'armure ennemie a été totalement ignorée !";
                    break;
                case TypeEffetCompetence.DrainMana:
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    int recupMana = Math.Min(35, hero.ManaMaxTotal - hero.ManaActuel);
                    hero.RestaurerMana(recupMana);
                    effetTxt = $"🌀 SIPHON D'ÉTHER ! Vous aspirez +{recupMana} Mana à la cible !";
                    break;
                default:
                    ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);
                    break;
            }

            AjouterLog($"\n{sort.Icone} SORT FORGÉ LANCÉ : {sort.Nom.ToUpper()} !", Color.FromArgb(175, 122, 198));
            AjouterLog($"💥 Vous déchaînez le grimoire forgé pour {degats} dégâts arcaniques !", Color.FromArgb(241, 196, 15));
            if (!string.IsNullOrEmpty(effetTxt)) AjouterLog(effetTxt, Color.Cyan);

            SecouerEcran(10, 6);
            DeclencherFlash(Color.FromArgb(85, 30, 95), 180);
            AfficherImpactVisuel($"{sort.Icone} {degats} DÉGÂTS RUNES !", Color.Magenta);

            hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 20);
            FinTourJoueur();
        }

        private void ActionUltime()
        {
            if (!enCombat || ennemiActuel == null) return;
            if (VerifierStunJoueur()) return;

            if (hero.JaugeUltime < 100)
            {
                MessageBox.Show("Votre jauge d'Ultime n'est pas encore chargée à 100% !", "Ultime non prêt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            hero.JaugeUltime = 0;
            int degats = (hero.AttaqueTotale * 3) + 70;
            ennemiActuel.PVActuels = Math.Max(0, ennemiActuel.PVActuels - degats);

            AjouterLog("\n🌟🌟🌟 FUREUR ULTIME DÉCHAÎNÉE ! 🌟🌟🌟", Color.FromArgb(241, 196, 15));
            AjouterLog($"Vous déchaînez l'art secret du {hero.Classe} ! {ennemiActuel.Nom} encaisse {degats} DÉGÂTS COLOSSAUX !", Color.Gold);

            SecouerEcran(16, 10);
            DeclencherFlash(Color.FromArgb(110, 95, 20), 280);
            AfficherImpactVisuel($"🌟🌟 ULTIME : -{degats} 🌟🌟", Color.Gold);

            FinTourJoueur();
        }

        private void ActionPotionSoin()
        {
            if (enCombat && VerifierStunJoueur()) return;

            if (hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure) <= 0)
            {
                MessageBox.Show("Vous n'avez plus de Potions de Soin en stock !", "Inventaire vide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            hero.UtiliserConsommable(TypeConsommable.PotionSoinMineure);
            hero.Soigner(60);
            AjouterLog("🧪 Vous buvez une Potion de Soin : +60 PV régénérés !", Color.FromArgb(46, 204, 113));
            DeclencherFlash(Color.FromArgb(20, 80, 35), 160);
            AfficherImpactVisuel("🧪 +60 PV RESTAURÉS", Color.LimeGreen);

            if (enCombat)
            {
                FinTourJoueur();
            }
        }

        private void ActionDefense()
        {
            if (!enCombat) return;
            if (VerifierStunJoueur()) return;

            postureDefense = true;
            hero.RestaurerMana(20);
            hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 10);
            AjouterLog("🛡️ Posture de Garde : Dégâts réduits de 50%, +20 Mana et +10% Ultime générés !", Color.FromArgb(52, 152, 219));
            DeclencherFlash(Color.FromArgb(25, 55, 85), 160);
            AfficherImpactVisuel("🛡️ GARDE BOUCLIER", Color.Cyan);

            FinTourJoueur();
        }

        private void ActionFuite()
        {
            if (!enCombat || ennemiActuel == null) return;

            if (ennemiActuel.EstBoss)
            {
                MessageBox.Show("Un sceau draconique interdit la fuite face à un Boss !", "Fuite Impossible", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (rng.Next(100) < 55 + hero.ChanceEsquiveTotale)
            {
                AjouterLog("💨 Vous profitez d'une diversion et fuyez le combat avec succès !", Color.FromArgb(241, 196, 15));
                BasculerModeVillage();
            }
            else
            {
                AjouterLog("❌ Fuite manquée ! L'adversaire vous barre le passage !", Color.FromArgb(231, 76, 60));
                FinTourJoueur();
            }
        }

        private void FinTourJoueur()
        {
            MettreAJourInterface();

            if (ennemiActuel != null && ennemiActuel.PVActuels <= 0)
            {
                VictoireCombat();
                return;
            }

            TourEnnemi();
        }

        private void TourEnnemi()
        {
            if (ennemiActuel == null) return;

            if (rng.Next(100) < hero.ChanceEsquiveTotale)
            {
                AjouterLog($"🤸 ESQUIVE PARFAITE ! Vous esquivez avec agilité l'attaque de {ennemiActuel.Nom} !", Color.FromArgb(52, 152, 219));
                AfficherImpactVisuel("🤸 ESQUIVE PARFAITE !", Color.Cyan);
            }
            else
            {
                int degats = ennemiActuel.Attaquer(hero, out string msgAction);

                if (postureDefense)
                {
                    int degatsReduits = degats / 2;
                    hero.PVActuels += (degats - degatsReduits);
                    AjouterLog(msgAction, Color.FromArgb(231, 76, 60));
                    AjouterLog($"🛡️ Votre bouclier absorbe l'impact : vous n'encaissez que {degatsReduits} dégâts !", Color.FromArgb(46, 204, 113));
                    SecouerEcran(5, 4);
                    DeclencherFlash(Color.FromArgb(60, 40, 20), 120);
                    AfficherImpactVisuel($"🛡️ ABSORBÉ : -{degatsReduits} PV", Color.Cyan);
                }
                else if (buffDefenseTours > 0)
                {
                    int degatsReduits = (int)(degats * 0.5);
                    hero.PVActuels += (degats - degatsReduits);
                    AjouterLog(msgAction, Color.FromArgb(231, 76, 60));
                    AjouterLog($"🪨 Peau de Pierre : vous n'encaissez que {degatsReduits} dégâts !", Color.FromArgb(243, 156, 18));
                    SecouerEcran(6, 4);
                    buffDefenseTours--;
                    AfficherImpactVisuel($"🪨 PEAU DE PIERRE : -{degatsReduits} PV", Color.Orange);
                }
                else
                {
                    AjouterLog(msgAction, Color.FromArgb(231, 76, 60));
                    if (degats > 0)
                    {
                        SecouerEcran(7, 5);
                        DeclencherFlash(Color.FromArgb(85, 20, 20), 140);
                        AfficherImpactVisuel($"🩸 -{degats} PV", Color.Crimson);
                    }
                }

                // Détection de mécaniques spéciales
                if (ennemiActuel is BossBelial bBelial && bBelial.ChargeMeteore)
                {
                    bossChargeAttaque = true;
                    AfficherImpactVisuel("⚠️ MÉTÉORE EN PRÉPARATION !", Color.Yellow);
                }
                else if (ennemiActuel is BossValdorak && msgAction.Contains("FRACAS SISMIQUE"))
                {
                    SecouerEcran(14, 10);
                    DeclencherFlash(Color.FromArgb(80, 50, 20), 200);
                    AfficherImpactVisuel("🌋 SÉISME TITANESQUE !", Color.OrangeRed);
                    if (rng.Next(100) < 40)
                    {
                        joueurStunTours = 1;
                        AfficherImpactVisuel("💫 ÉTOURDI PAR LE SÉISME !", Color.Gold);
                        AjouterLog("💫 L'onde sismique vous déséquilibre : vous êtes étourdi pour 1 tour !", Color.FromArgb(241, 196, 15));
                    }
                }
                else if (ennemiActuel is BossChevalierDuNeant && msgAction.Contains("ÉCLIPSE"))
                {
                    SecouerEcran(16, 12);
                    DeclencherFlash(Color.FromArgb(100, 10, 80), 250);
                    AfficherImpactVisuel("🌑 ÉCLIPSE DU VIDE !", Color.MediumPurple);
                    if (rng.Next(100) < 35)
                    {
                        joueurStunTours = 1;
                        AfficherImpactVisuel("💫 ÉTOURDI DANS LE VIDE !", Color.Gold);
                        AjouterLog("💫 L'éclipse aspire votre conscience : vous êtes étourdi pour 1 tour !", Color.FromArgb(241, 196, 15));
                    }
                }
                else if (ennemiActuel is BossAzkalith && msgAction.Contains("VENIMEUSE"))
                {
                    joueurPoisonTours = Math.Min(3, joueurPoisonTours + 2);
                    AfficherImpactVisuel("🐍 ENVENIMÉ (2 TOURS) !", Color.LimeGreen);
                }
                else if (ennemiActuel is BossBelial && msgAction.Contains("brûle"))
                {
                    joueurBruleTours = Math.Min(3, joueurBruleTours + 2);
                    AfficherImpactVisuel("🔥 BRÛLÉ (2 TOURS) !", Color.OrangeRed);
                }

                hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 10);
            }

            postureDefense = false;

            // Début du tour joueur : DoT
            if (joueurBruleTours > 0 && hero.PVActuels > 0)
            {
                hero.PVActuels = Math.Max(0, hero.PVActuels - 12);
                joueurBruleTours--;
                AjouterLog($"🔥 Brûlure infernale : les braises vous consument (-12 PV, {joueurBruleTours}t restants) !", Color.OrangeRed);
                AfficherImpactVisuel("🔥 BRÛLURE -12 PV", Color.OrangeRed);
            }
            if (joueurPoisonTours > 0 && hero.PVActuels > 0)
            {
                hero.PVActuels = Math.Max(0, hero.PVActuels - 10);
                joueurPoisonTours--;
                AjouterLog($"🧪 Venin corrosif : le poison ronge vos organes (-10 PV, {joueurPoisonTours}t restants) !", Color.LimeGreen);
                AfficherImpactVisuel("🧪 POISON -10 PV", Color.LimeGreen);
            }

            MettreAJourInterface();

            if (hero.PVActuels <= 0)
            {
                DefaiteCombat();
            }
        }

        private void VictoireCombat()
        {
            if (ennemiActuel == null) return;

            AjouterLog("\n🏆 VICTOIRE ÉCLATANTE !", Color.FromArgb(46, 204, 113));
            AjouterLog($"Vous avez triomphé de {ennemiActuel.Nom} !", Color.White);

            hero.Or += ennemiActuel.GainOr;
            AjouterLog($"💰 Butin ramassé : +{ennemiActuel.GainOr} Or !", Color.FromArgb(241, 196, 15));

            if (!hero.BestiaireMonstresTues.ContainsKey(ennemiActuel.Nom))
                hero.BestiaireMonstresTues[ennemiActuel.Nom] = 0;
            hero.BestiaireMonstresTues[ennemiActuel.Nom]++;

            if (ennemiActuel.EstBoss) hero.BossVaincusTotal++;

            foreach (var q in hero.QuetesActives.Where(x => !x.EstTerminee && (x.CibleNom == ennemiActuel.Nom || ennemiActuel.Nom.Contains(x.CibleNom))))
            {
                q.Progression++;
                AjouterLog($"📜 Progression de Quête : '{q.Titre}' ({q.Progression}/{q.Objectif}) !", Color.Gold);
                if (q.EstTerminee)
                {
                    AjouterLog($"🎉 OBJECTIF DE QUÊTE ACCOMPLI : '{q.Titre}' ! Réclamez votre prime à la Guilde !", Color.FromArgb(46, 204, 113));
                    AfficherImpactVisuel("🎉 QUÊTE COMPLÉTÉE !", Color.Gold);
                }
            }

            hero.GagnerXP(ennemiActuel.GainXP);

            // Matériaux d'artisanat
            var matsLoot = ennemiActuel.ObtenirLootMateriaux();
            if (matsLoot != null && matsLoot.Count > 0)
            {
                foreach (var kvp in matsLoot)
                {
                    hero.AjouterMateriau(kvp.Key, kvp.Value);
                    AjouterLog($"📦 Matériau ramassé : +{kvp.Value} {kvp.Key} !", Color.FromArgb(52, 152, 219));
                }
            }

            if (rng.Next(100) < 45)
            {
                hero.PierresDeForge++;
                AjouterLog("💎 Relique précieuse : Vous trouvez 1 Pierre de Forge !", Color.FromArgb(52, 152, 219));
            }

            // Gestion de l'avancée de Donjon
            if (donjonActuel != null)
            {
                AjouterLog($"\n✅ SALLE {indexSalleDonjon + 1} DU DONJON SÉCURISÉE !", Color.FromArgb(46, 204, 113));
                indexSalleDonjon++;
                if (indexSalleDonjon >= donjonActuel.Etapes.Count)
                {
                    TerminerDonjonSucces();
                    return;
                }

                // Afficher écran d'intermède de donjon
                lblServicesTitre.Text = "🛡️ REPOS & POTIONS";
                lblServicesTitre.ForeColor = Color.FromArgb(46, 204, 113);
                pnlServicesButtons.Controls.Clear();
                pnlServicesButtons.Controls.Add(CreerActionButton("🧪 Boire Potion", "+60 PV", Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), (s, e) => { ActionPotionSoin(); MettreAJourInterface(); }, 270, 42));
                pnlServicesButtons.Controls.Add(CreerActionButton("🏃 Quitter le Donjon", "Fuir au village", Color.FromArgb(127, 140, 141), Color.FromArgb(149, 165, 166), (s, e) =>
                {
                    var rep = MessageBox.Show("Voulez-vous abandonner l'expédition et rentrer au village ?", "Quitter Donjon", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (rep == DialogResult.Yes)
                    {
                        donjonActuel = null;
                        BasculerModeVillage();
                    }
                }, 270, 42));

                lblCombatsTitre.Text = "🏰 PROGRESSION DU DONJON";
                pnlCombatOnglets.Visible = false;
                pnlActionButtons.Controls.Clear();
                pnlActionButtons.Controls.Add(CreerActionButton("➡️ SALLE SUIVANTE", $"Entrer dans la salle {indexSalleDonjon + 1}", Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113), (s, e) => TraiterSalleDonjonActuelle(), 220, 52));

                lblAreneTitre.Text = $"🏰 {donjonActuel.Nom.ToUpper()} — SALLE TERMINÉE";
                lblAreneTitre.ForeColor = Color.FromArgb(46, 204, 113);
                lblEnnemiNom.Text = "Zone temporairement sécurisée";
                lblEnnemiBadge.Text = "[RÉPIT]";
                lblEnnemiBadge.ForeColor = Color.FromArgb(46, 204, 113);
                lblEnnemiFaiblesseLore.Text = "Vous pouvez vous soigner avec vos potions avant d'avancer vers la salle suivante !";
                pbEnnemiPV.Valeur = 0;
                pbEnnemiPV.TexteCentral = "Aucun monstre";
                MettreAJourInterface();
                return;
            }

            // Gestion de l'avancée de la Tour Astrale Infinie
            if (enTourAstrale)
            {
                hero.EtageTourRecord = Math.Max(hero.EtageTourRecord, hero.EtageTourActuel);
                int gainOrTour = 60 + (hero.EtageTourActuel * 40);
                int gainXPTour = 90 + (hero.EtageTourActuel * 50);
                int gainEclats = 1 + (hero.EtageTourActuel / 5);

                hero.OrAccumuleTour += gainOrTour;
                hero.XPAccumuleTour += gainXPTour;
                hero.EclatsAccumulesTour += gainEclats;
                hero.AjouterMateriau("Éclat Astral", gainEclats);

                AjouterLog($"\n🌌 ÉTAGE {hero.EtageTourActuel} DE LA TOUR ASTRALE PURIFIÉ !", Color.FromArgb(175, 122, 198));
                AjouterLog($"✨ Butin temporaire accumulé : {hero.OrAccumuleTour} Or, {hero.XPAccumuleTour} XP, {hero.EclatsAccumulesTour} Éclats Astraux !", Color.Gold);

                lblServicesTitre.Text = "🛡️ SALLE DE REPOS ASTRALE";
                lblServicesTitre.ForeColor = Color.FromArgb(155, 89, 182);
                pnlServicesButtons.Controls.Clear();
                pnlServicesButtons.Controls.Add(CreerActionButton("🧪 Boire Potion", "+60 PV", Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), (s, e) => { ActionPotionSoin(); MettreAJourInterface(); }, 270, 42));
                pnlServicesButtons.Controls.Add(CreerActionButton("🚪 ENCAISSER LE BUTIN", "Sécuriser et quitter", Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113), (s, e) => EncaisserEtQuitterTour(), 270, 44));

                lblCombatsTitre.Text = "🌌 ASCENSION DE LA TOUR INFINIE";
                pnlCombatOnglets.Visible = false;
                pnlActionButtons.Controls.Clear();
                pnlActionButtons.Controls.Add(CreerActionButton($"➡️ ÉTAGE SUIVANT ({hero.EtageTourActuel + 1})", "Défi plus redoutable", Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182), (s, e) =>
                {
                    hero.EtageTourActuel++;
                    LancerCombatTourAstrale(hero.EtageTourActuel);
                }, 220, 52));

                lblAreneTitre.Text = $"🌌 TOUR ASTRALE — PALIER {hero.EtageTourActuel} SÉCURISÉ";
                lblAreneTitre.ForeColor = Color.FromArgb(155, 89, 182);
                lblEnnemiNom.Text = "Nexus Céleste Apaisé";
                lblEnnemiBadge.Text = $"[RECORD: {hero.EtageTourRecord}]";
                lblEnnemiBadge.ForeColor = Color.FromArgb(241, 196, 15);
                lblEnnemiFaiblesseLore.Text = $"Butin accumulé à encaisser : {hero.OrAccumuleTour} Or, {hero.XPAccumuleTour} XP, {hero.EclatsAccumulesTour} Éclats Astraux. Encaisserez-vous ou tenterez-vous l'étage suivant ?";
                pbEnnemiPV.Valeur = 0;
                pbEnnemiPV.TexteCentral = "Étage purifié";
                MettreAJourInterface();
                return;
            }

            BasculerModeVillage();
        }

        private void DefaiteCombat()
        {
            donjonActuel = null;
            indexSalleDonjon = 0;
            joueurBruleTours = 0;
            joueurPoisonTours = 0;
            joueurStunTours = 0;
            bossChargeAttaque = false;

            AjouterLog("\n💀 DÉFAITE AU COMBAT...", Color.FromArgb(231, 76, 60));

            if (enTourAstrale)
            {
                int orPerdu = hero.OrAccumuleTour / 2;
                int orSauve = hero.OrAccumuleTour - orPerdu;
                hero.Or += orSauve;
                hero.GagnerXP(hero.XPAccumuleTour / 2);

                AjouterLog($"🌌 Vous avez été terrassé à l'Étage {hero.EtageTourActuel} de la Tour Astrale !", Color.FromArgb(231, 76, 60));
                AjouterLog($"💸 50% du butin de cette ascension a été perdu ({orPerdu} Or disparus dans le vide spatial) !", Color.IndianRed);
                AjouterLog($"📦 Vous sauvez tout de même {orSauve} Or et la moitié de l'XP.", Color.Gold);

                hero.OrAccumuleTour = 0;
                hero.XPAccumuleTour = 0;
                hero.EclatsAccumulesTour = 0;
                enTourAstrale = false;
                hero.EtageTourActuel = Math.Max(1, ((hero.EtageTourActuel - 1) / 5) * 5 + 1);
            }
            else
            {
                AjouterLog("Des gardes vous rapatrient d'urgence au village de Val-Serein.", Color.FromArgb(230, 126, 34));
                int perteOr = (int)(hero.Or * 0.12);
                hero.Or -= perteOr;
                if (perteOr > 0)
                {
                    AjouterLog($"💸 Les soins d'urgence vous ont coûté {perteOr} Or.", Color.IndianRed);
                }
            }

            hero.PVActuels = hero.PVMaxTotal / 2;
            hero.ManaActuel = hero.ManaMaxTotal / 2;

            BasculerModeVillage();
        }

        // ==============================================================
        // EXPLORATION & SERVICES DU MONDE
        // ==============================================================
        private void ExplorerZone(TypeZone zone)
        {
            string nomZone = zone switch
            {
                TypeZone.Foret => "La Forêt des Brumes",
                TypeZone.Catacombes => "Les Catacombes Hantées",
                TypeZone.ForteresseGivre => "La Cime Glaciale",
                TypeZone.Volcan => "Les Abîmes de Feu",
                _ => "Terres Inconnues"
            };

            AjouterLog($"\n🌲 Vous vous enfoncez dans : {nomZone}...", Color.FromArgb(52, 152, 219));

            int jet = rng.Next(100);
            if (jet < 18)
            {
                AjouterLog("✨ Vous découvrez un autel ancien ! Une lumière bienveillante restaure 100% de vos PV et Mana !", Color.FromArgb(46, 204, 113));
                hero.PVActuels = hero.PVMaxTotal;
                hero.ManaActuel = hero.ManaMaxTotal;
                hero.JaugeUltime = Math.Min(100, hero.JaugeUltime + 30);
                MettreAJourInterface();
            }
            else if (jet < 32)
            {
                int gainOr = rng.Next(40, 100);
                hero.Or += gainOr;
                hero.CoffresTresorOuverts++;
                AjouterLog($"🎁 Trésor trouvé ! Vous ouvrez un coffre scellé et récupérez {gainOr} Or !", Color.FromArgb(241, 196, 15));
                MettreAJourInterface();
            }
            else
            {
                Monstre m = GenererMonstrePourZone(zone);
                LancerCombat(m);
            }
        }

        private Monstre GenererMonstrePourZone(TypeZone zone)
        {
            switch (zone)
            {
                case TypeZone.Foret:
                    int f = rng.Next(3);
                    return f switch
                    {
                        0 => new Monstre("Sanglier Enragé", 60, 15, 6, 50, 25, false, "GRRRH !", "Tranchant", "Bête sauvage corrompue."),
                        1 => new Monstre("Gobelin Maraudeur", 75, 17, 7, 65, 35, false, "DONNE L'OR !", "Feu", "Pillard sournois armé d'une pique."),
                        _ => new Monstre("Loup Alpha des Brumes", 85, 20, 8, 80, 45, false, "AOUUUH !", "Glace", "Chasseur agile et impitoyable.")
                    };
                case TypeZone.Catacombes:
                    int c = rng.Next(2);
                    return c switch
                    {
                        0 => new Monstre("Squelette Gardien", 110, 24, 10, 120, 60, false, "CLAC-CLAC !", "Sacré", "Sentinelle spectrale éternelle."),
                        _ => new Monstre("Spectre Tourmenté", 130, 28, 8, 160, 85, false, "VOTRE ÂME EST À MOI...", "Lumière", "Apparition sinistre flottant dans l'ombre.")
                    };
                case TypeZone.ForteresseGivre:
                    return new Monstre("Golem de Glace Ancestral", 210, 36, 16, 280, 140, false, "KRAAAK !", "Feu", "Colosse forgé dans le blizzard éternel.");
                case TypeZone.Volcan:
                    return new Monstre("Drake de Magma", 290, 45, 18, 420, 210, false, "ROAAAR ARDENT !", "Glace", "Monstre reptilien crachant du feu liquide.");
                default:
                    return new Monstre("Bandit de Grand Chemin", 50, 12, 5, 40, 20);
            }
        }

        private void DefierTourAstrale()
        {
            if (hero.EtageTourActuel < 1) hero.EtageTourActuel = 1;

            using (Form dlg = new Form())
            {
                dlg.Text = "🌌 Tour Astrale Infinie — Sanctuaire Cosmique";
                dlg.Size = new Size(620, 480);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(18, 20, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };

                Label lblTitre = new Label
                {
                    Text = "🌌 LA TOUR ASTRALE INFINIE",
                    Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(175, 122, 198),
                    AutoSize = true,
                    Location = new Point(24, 20)
                };

                Label lblStats = new Label
                {
                    Text = $"🏆 Record d'ascension : Étage {hero.EtageTourRecord}\n" +
                           $"📍 Prochain Étage à gravir : Étage {hero.EtageTourActuel}\n" +
                           $"💎 Éclats Astraux possédés : {hero.ObtenirMateriau("Éclat Astral")}\n" +
                           $"💰 Butin accumulé en cours : {hero.OrAccumuleTour} Or, {hero.XPAccumuleTour} XP",
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(24, 60),
                    Size = new Size(550, 78)
                };

                Label lblRegles = new Label
                {
                    Text = "Bienvenue dans l'épreuve suprême d'Aethelgard :\n\n" +
                           "• Chaque étage augmente drastiquement la puissance des gardiens cosmiques.\n" +
                           "• Tous les 5 étages : Gardien Suprême (Boss cosmique aux reliques mythiques) !\n" +
                           "• Après chaque victoire d'étage, vous pouvez choisir de QUITTER et de SÉCURISER votre butin.\n" +
                           "• En cas de mort, 50% du butin non sécurisé est englouti dans le vide !",
                    Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                    ForeColor = Color.FromArgb(190, 200, 215),
                    Location = new Point(24, 145),
                    Size = new Size(550, 110)
                };

                Button btnLancer = new Button
                {
                    Text = $"🚀 GRAVIR L'ÉTAGE {hero.EtageTourActuel}",
                    Size = new Size(260, 48),
                    Location = new Point(24, 275),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnLancer.FlatAppearance.BorderSize = 0;
                btnLancer.Click += (s, e) =>
                {
                    dlg.Close();
                    LancerCombatTourAstrale(hero.EtageTourActuel);
                };

                Button btnReset = new Button
                {
                    Text = "🔄 Recommencer à l'Étage 1",
                    Size = new Size(250, 48),
                    Location = new Point(300, 275),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnReset.FlatAppearance.BorderSize = 0;
                btnReset.Click += (s, e) =>
                {
                    if (MessageBox.Show("Voulez-vous réinitialiser votre ascension à l'Étage 1 ?\n(Votre record historique sera préservé)", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        hero.EtageTourActuel = 1;
                        hero.OrAccumuleTour = 0;
                        hero.XPAccumuleTour = 0;
                        hero.EclatsAccumulesTour = 0;
                        lblStats.Text = $"🏆 Record d'ascension : Étage {hero.EtageTourRecord}\n" +
                                       $"📍 Prochain Étage à gravir : Étage {hero.EtageTourActuel}\n" +
                                       $"💎 Éclats Astraux possédés : {hero.ObtenirMateriau("Éclat Astral")}\n" +
                                       $"💰 Butin accumulé en cours : 0 Or, 0 XP";
                        btnLancer.Text = "🚀 GRAVIR L'ÉTAGE 1";
                    }
                };

                Button btnFermer = new Button
                {
                    Text = "Retour au Village de Val-Serein",
                    Size = new Size(526, 38),
                    Location = new Point(24, 340),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(35, 40, 52),
                    ForeColor = Color.FromArgb(170, 180, 200),
                    Font = new Font("Segoe UI", 9f),
                    Cursor = Cursors.Hand
                };
                btnFermer.FlatAppearance.BorderSize = 0;
                btnFermer.Click += (s, e) => dlg.Close();

                pnl.Controls.AddRange(new Control[] { lblTitre, lblStats, lblRegles, btnLancer, btnReset, btnFermer });
                dlg.Controls.Add(pnl);
                dlg.ShowDialog(this);
            }
        }

        private void LancerCombatTourAstrale(int etage)
        {
            enTourAstrale = true;
            Monstre m = GenererMonstreTour(etage);
            LancerCombat(m);
            lblAreneTitre.Text = $"🌌 TOUR ASTRALE — ÉTAGE {etage} / ∞";
            lblBandeauLieu.Text = $"📍 Tour Astrale : Étage {etage} (Ascension Cosmique)";
            lblBandeauLieu.ForeColor = Color.FromArgb(175, 122, 198);
        }

        private Monstre GenererMonstreTour(int etage)
        {
            if (etage % 5 == 0)
            {
                int tier = (etage / 5) % 8;
                return tier switch
                {
                    1 => new BossZephyros(),
                    2 => new BossArchonteSolaire(),
                    3 => new BossKrakenAbyssal(),
                    4 => new BossChronos(),
                    5 => new BossLeviathanStellaire(),
                    6 => new BossAbaddon(),
                    7 => new BossDeusExNihilo(),
                    _ => new BossXanthos()
                };
            }

            int pv = 130 + (etage * 38);
            int atk = 22 + (etage * 6);
            int def = 8 + (etage * 3);
            int xp = 160 + (etage * 90);
            int orGain = 90 + (etage * 50);

            string[] noms = {
                $"Sentinelle Éthérée (Étage {etage})",
                $"Guerrier Stellaire (Étage {etage})",
                $"Anomalie Dimensionnelle (Étage {etage})",
                $"Spectre Astral du Chaos (Étage {etage})",
                $"Goliath Cosmique (Étage {etage})"
            };
            string nom = noms[(etage - 1) % noms.Length];
            return new Monstre(nom, pv, atk, def, xp, orGain, false, "L'ÉNERGIE COSMIQUE VOUS PURGERA !", "Cosmique / Sacré", "Entité dimensionnelle gardant les échelons de la Tour.");
        }

        private void EncaisserEtQuitterTour()
        {
            int or = hero.OrAccumuleTour;
            int xp = hero.XPAccumuleTour;
            int eclats = hero.EclatsAccumulesTour;

            hero.Or += or;
            hero.GagnerXP(xp);
            hero.EclatsAstraux += eclats;

            hero.OrAccumuleTour = 0;
            hero.XPAccumuleTour = 0;
            hero.EclatsAccumulesTour = 0;
            enTourAstrale = false;
            hero.EtageTourActuel++;

            AjouterLog($"\n🏆 ASCENSION SÉCURISÉE ! Vous quittez la Tour avec votre butin : +{or} Or, +{xp} XP et +{eclats} Éclats Astraux !", Color.FromArgb(241, 196, 15));
            DeclencherFlash(Color.FromArgb(90, 80, 20), 250);
            AfficherImpactVisuel("🏆 BUTIN SÉCURISÉ !", Color.Gold);

            MessageBox.Show(
                $"Félicitations pour votre ascension héroïque !\n\n" +
                $"Vous sécurisez vos trésors jusqu'à l'Étage {hero.EtageTourActuel - 1} :\n" +
                $"💰 +{or} Or\n" +
                $"⭐ +{xp} Points d'Expérience\n" +
                $"💎 +{eclats} Éclats Astraux\n\n" +
                $"Votre palier de départ est sauvegardé à l'Étage {hero.EtageTourActuel} (Record : Étage {hero.EtageTourRecord}) !",
                "Victoire Astrale Sécurisée", MessageBoxButtons.OK, MessageBoxIcon.Information);

            BasculerModeVillage();
        }

        private void Auberge()
        {
            if (hero.Or < 25)
            {
                MessageBox.Show("Vous n'avez pas assez d'Or (25 Or requis) !", "Or Insuffisant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            hero.Or -= 25;
            hero.PVActuels = hero.PVMaxTotal;
            hero.ManaActuel = hero.ManaMaxTotal;
            hero.BuffReposCombatsRestants = 3;

            AjouterLog("\n🍺 Vous savourez un repas chaud et passez la nuit à l'Auberge du Sanglier Doré.", Color.FromArgb(46, 204, 113));
            AjouterLog("Vos PV et votre Mana sont totalement restaurés (100 %) !", Color.White);
            AjouterLog("✨ Bénédiction du Repos : +25% XP pour vos 3 prochains combats !", Color.FromArgb(241, 196, 15));
            MettreAJourInterface();
        }

        private Color ObtenirCouleurRarete(Rarete r) => r switch
        {
            Rarete.Rare => Color.FromArgb(52, 152, 219),
            Rarete.Epique => Color.FromArgb(155, 89, 182),
            Rarete.Legendaire => Color.FromArgb(241, 196, 15),
            Rarete.Mythique => Color.FromArgb(231, 76, 60),
            _ => Color.FromArgb(220, 220, 220)
        };

        private void OuvrirForge()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "🔨 Forge Royale & Atelier d'Artisanat de Brom";
                dlg.Size = new Size(980, 680);
                dlg.MinimumSize = new Size(900, 600);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(16, 18, 24);
                dlg.ForeColor = Color.White;
                dlg.Font = new Font("Segoe UI", 9.5f);

                // --- 1. En-tête de la Forge ---
                Panel pnlHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 90,
                    BackColor = Color.FromArgb(26, 22, 20),
                    Padding = new Padding(16, 10, 16, 10)
                };

                Label lblTitre = new Label
                {
                    Text = "🔨 FORGE & ATELIER D'ARTISANAT DU MAÎTRE BROM",
                    Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(230, 126, 34),
                    AutoSize = true,
                    Location = new Point(16, 8)
                };

                Label lblSousTitre = new Label
                {
                    Text = "Forge d'armes et armures, fabrication de grimoires de sorts, potions et amélioration d'équipement",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(180, 185, 195),
                    AutoSize = true,
                    Location = new Point(18, 34)
                };

                FlowLayoutPanel pnlHeaderStats = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    Height = 28,
                    FlowDirection = FlowDirection.LeftToRight,
                    BackColor = Color.Transparent
                };

                Label lblStockPierres = new Label
                {
                    Text = $"💎 {hero.PierresDeForge} Pierres de Forge",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 152, 219),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 25, 0)
                };

                Label lblStockOr = new Label
                {
                    Text = $"💰 {hero.Or} Or",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    AutoSize = true,
                    Margin = new Padding(0, 4, 25, 0)
                };

                Label lblStockSorts = new Label
                {
                    Text = $"🔮 {hero.CompetencesDebloquees.Count} Sort(s) Maîtrisé(s)",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(175, 122, 198),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 25, 0)
                };

                int nbMatsTotal = hero.MateriauxCraft.Values.Sum();
                Label lblStockMats = new Label
                {
                    Text = $"📦 {nbMatsTotal} Matériaux en Réserve",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 204, 113),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 0, 0)
                };

                pnlHeaderStats.Controls.AddRange(new Control[] { lblStockPierres, lblStockOr, lblStockSorts, lblStockMats });
                pnlHeader.Controls.AddRange(new Control[] { lblTitre, lblSousTitre, pnlHeaderStats });
                dlg.Controls.Add(pnlHeader);

                // --- 2. TabControl ---
                TabControl tabs = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Padding = new Point(14, 6)
                };

                TabPage tabCraft = new TabPage("⚒️ Atelier de Forge (Recettes)") { BackColor = Color.FromArgb(16, 18, 24) };
                TabPage tabAmelioration = new TabPage("🔨 Amélioration de Stuff (+1, +2...)") { BackColor = Color.FromArgb(16, 18, 24) };
                TabPage tabMateriaux = new TabPage($"📦 Sac de Matériaux ({hero.MateriauxCraft.Count})") { BackColor = Color.FromArgb(16, 18, 24) };

                Action rafraichirHeader = () =>
                {
                    lblStockPierres.Text = $"💎 {hero.PierresDeForge} Pierres de Forge";
                    lblStockOr.Text = $"💰 {hero.Or} Or";
                    lblStockSorts.Text = $"🔮 {hero.CompetencesDebloquees.Count} Sort(s) Maîtrisé(s)";
                    lblStockMats.Text = $"📦 {hero.MateriauxCraft.Values.Sum()} Matériaux en Réserve";
                    tabMateriaux.Text = $"📦 Sac de Matériaux ({hero.MateriauxCraft.Count})";
                };

                // === TAB 1 : ATELIER DE CRAFT ===
                TableLayoutPanel tblCraft = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    Padding = new Padding(10)
                };
                tblCraft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));
                tblCraft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));
                tblCraft.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
                tblCraft.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

                FlowLayoutPanel pnlFiltres = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0)
                };
                tblCraft.Controls.Add(pnlFiltres, 0, 0);
                tblCraft.SetColumnSpan(pnlFiltres, 2);

                CategorieCraft? categorieFiltre = null;

                ListBox lbRecettes = new ListBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(24, 27, 36),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    ItemHeight = 26
                };
                tblCraft.Controls.Add(lbRecettes, 0, 1);

                Panel pnlDetailCraft = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(22, 25, 34),
                    Padding = new Padding(16)
                };
                tblCraft.Controls.Add(pnlDetailCraft, 1, 1);

                Label lblRecNom = new Label
                {
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    AutoSize = true,
                    Location = new Point(16, 12)
                };

                Label lblRecBadges = new Label
                {
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 204, 113),
                    AutoSize = true,
                    Location = new Point(16, 42)
                };

                Label lblRecDesc = new Label
                {
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(220, 225, 235),
                    Size = new Size(430, 75),
                    Location = new Point(16, 72)
                };

                Label lblRecCout = new Label
                {
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    AutoSize = true,
                    Location = new Point(16, 155)
                };

                RichTextBox rtbRecMateriaux = new RichTextBox
                {
                    Location = new Point(16, 185),
                    Size = new Size(430, 160),
                    BackColor = Color.FromArgb(18, 20, 26),
                    ForeColor = Color.White,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Segoe UI", 9.5f)
                };

                Button btnForger = new Button
                {
                    Text = "⚒️ FORGER L'OBJET",
                    Size = new Size(260, 48),
                    Location = new Point(16, 360),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnForger.FlatAppearance.BorderSize = 0;

                pnlDetailCraft.Controls.AddRange(new Control[] {
                    lblRecNom, lblRecBadges, lblRecDesc, lblRecCout, rtbRecMateriaux, btnForger
                });

                List<RecetteCraft> recettesFiltrees = new List<RecetteCraft>();

                Action rafraichirListeRecettes = () =>
                {
                    lbRecettes.Items.Clear();
                    recettesFiltrees = CatalogueCraft.Recettes
                        .Where(r => categorieFiltre == null || r.Categorie == categorieFiltre)
                        .ToList();

                    foreach (var r in recettesFiltrees)
                    {
                        bool pret = r.PeutFabriquer(hero);
                        string statut = pret ? "✓ [PRÊT]" : "✗ [MANQUE]";
                        lbRecettes.Items.Add($"{statut} {r.Nom} (Niv. {r.NiveauRequis}+)");
                    }

                    if (recettesFiltrees.Count > 0) lbRecettes.SelectedIndex = 0;
                    else
                    {
                        lblRecNom.Text = "Aucune recette dans cette catégorie.";
                        lblRecBadges.Text = "";
                        lblRecDesc.Text = "";
                        lblRecCout.Text = "";
                        rtbRecMateriaux.Clear();
                        btnForger.Enabled = false;
                    }
                };

                lbRecettes.SelectedIndexChanged += (s, e) =>
                {
                    if (lbRecettes.SelectedIndex >= 0 && lbRecettes.SelectedIndex < recettesFiltrees.Count)
                    {
                        var r = recettesFiltrees[lbRecettes.SelectedIndex];
                        lblRecNom.Text = r.Nom;
                        lblRecNom.ForeColor = ObtenirCouleurRarete(r.RareteItem);
                        lblRecBadges.Text = $"[CATÉGORIE : {r.Categorie.ToString().ToUpper()}]  •  [RARETÉ : {r.RareteItem}]  •  [NIVEAU REQUIS : {r.NiveauRequis}+]";
                        lblRecDesc.Text = r.Description;
                        lblRecCout.Text = $"💰 Coût en Or : {r.CoutOr} Or  (Vous possédez : {hero.Or} Or)";
                        lblRecCout.ForeColor = hero.Or >= r.CoutOr ? Color.Gold : Color.IndianRed;

                        rtbRecMateriaux.Clear();
                        rtbRecMateriaux.SelectionFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        rtbRecMateriaux.SelectionColor = Color.FromArgb(241, 196, 15);
                        rtbRecMateriaux.AppendText("COMPOSANTS & MATÉRIAUX REQUIS :\n\n");

                        foreach (var mat in r.MateriauxRequis)
                        {
                            int possede = hero.ObtenirMateriau(mat.Key);
                            bool ok = possede >= mat.Value;
                            rtbRecMateriaux.SelectionFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
                            rtbRecMateriaux.SelectionColor = ok ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
                            rtbRecMateriaux.AppendText($"  {(ok ? "✓" : "✗")} {mat.Key} : {possede} / {mat.Value} requis\n");
                        }

                        bool peutFabriquer = r.PeutFabriquer(hero);
                        btnForger.Enabled = peutFabriquer;
                        btnForger.BackColor = peutFabriquer ? Color.FromArgb(39, 174, 96) : Color.FromArgb(60, 65, 75);
                    }
                };

                btnForger.Click += (s, e) =>
                {
                    if (lbRecettes.SelectedIndex >= 0 && lbRecettes.SelectedIndex < recettesFiltrees.Count)
                    {
                        var r = recettesFiltrees[lbRecettes.SelectedIndex];
                        if (!r.PeutFabriquer(hero))
                        {
                            MessageBox.Show("Vous ne remplissez pas les conditions nécessaires (Or, niveau ou matériaux manquants) !", "Artisanat Impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        hero.Or -= r.CoutOr;
                        foreach (var mat in r.MateriauxRequis)
                        {
                            hero.ConsommerMateriau(mat.Key, mat.Value);
                        }

                        if (r.GenererEquipement != null)
                        {
                            var eq = r.GenererEquipement();
                            hero.SacEquipements.Add(eq);
                            AjouterLog($"\n⚒️ FORGE ROYALE : Vous avez forgé {eq.ObtenirNomComplet()} !", Color.FromArgb(241, 196, 15));
                            DeclencherFlash(Color.FromArgb(90, 80, 20), 220);
                            AfficherImpactVisuel("⚒️ OBJET FORGÉ !", Color.Gold);

                            var equiperRep = MessageBox.Show($"Félicitations !\nVous avez forgé avec succès :\n\n{eq.ObtenirNomComplet()}\n{eq.ObtenirDescription()}\n\nSouhaitez-vous l'équiper immédiatement ?", "Forge Réussie !", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (equiperRep == DialogResult.Yes)
                            {
                                hero.EquiperObjet(eq);
                                AjouterLog($"🛡️ Vous équipez immédiatement {eq.ObtenirNomComplet()} !", Color.FromArgb(46, 204, 113));
                            }
                        }
                        else if (r.CompetenceResultat != null)
                        {
                            var sort = r.CompetenceResultat;
                            if (!hero.CompetencesDebloquees.Any(c => c.Id == sort.Id))
                                hero.CompetencesDebloquees.Add(sort);
                            if (hero.CompetenceEquipee == null)
                                hero.CompetenceEquipee = sort;

                            AjouterLog($"\n🔮 GRIMOIRE MAÎTRISÉ : Vous débloquez le sort '{sort.Nom}' ({sort.Icone}) !", Color.FromArgb(155, 89, 182));
                            DeclencherFlash(Color.FromArgb(80, 40, 90), 220);
                            AfficherImpactVisuel("🔮 SORT DÉBLOQUÉ !", Color.MediumPurple);

                            MessageBox.Show($"Félicitations !\nVous avez étudié le grimoire et maîtrisez désormais :\n\n{sort.Icone} {sort.Nom}\n{sort.Description}\nCoût : {sort.CoutMana} Mana\n\nCe sort est prêt à être déchaîné lors de vos prochains combats !", "Sort Magique Appris !", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (r.ConsommableResultat != null)
                        {
                            hero.AjouterConsommable(r.ConsommableResultat.Value, r.QuantiteConsommable);
                            string nomCons = ConsommableInfo.ObtenirNom(r.ConsommableResultat.Value);
                            AjouterLog($"\n🧪 ALCHIMIE : +{r.QuantiteConsommable}x {nomCons} ajoutées à votre réserve !", Color.FromArgb(46, 204, 113));
                            DeclencherFlash(Color.FromArgb(20, 80, 40), 180);
                            AfficherImpactVisuel("🧪 ALCHIMIE RÉUSSIE !", Color.LimeGreen);
                            MessageBox.Show($"+{r.QuantiteConsommable}x {nomCons} ajoutées à vos potions !", "Alchimie Réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        rafraichirHeader();
                        MettreAJourInterface();
                        int precIndex = lbRecettes.SelectedIndex;
                        rafraichirListeRecettes();
                        if (precIndex >= 0 && precIndex < lbRecettes.Items.Count)
                            lbRecettes.SelectedIndex = precIndex;
                    }
                };

                Action<string, CategorieCraft?> ajouterBoutonFiltre = (texte, cat) =>
                {
                    Button btn = new Button
                    {
                        Text = texte,
                        AutoSize = true,
                        Height = 30,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = (categorieFiltre == cat) ? Color.FromArgb(180, 100, 30) : Color.FromArgb(35, 40, 52),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(2, 2, 4, 2)
                    };
                    btn.FlatAppearance.BorderSize = 1;
                    btn.FlatAppearance.BorderColor = (categorieFiltre == cat) ? Color.FromArgb(230, 126, 34) : Color.FromArgb(60, 68, 85);
                    btn.Click += (s, e) =>
                    {
                        categorieFiltre = cat;
                        foreach (Control c in pnlFiltres.Controls)
                        {
                            if (c is Button b)
                            {
                                bool active = (b == btn);
                                b.BackColor = active ? Color.FromArgb(180, 100, 30) : Color.FromArgb(35, 40, 52);
                                b.FlatAppearance.BorderColor = active ? Color.FromArgb(230, 126, 34) : Color.FromArgb(60, 68, 85);
                            }
                        }
                        rafraichirListeRecettes();
                    };
                    pnlFiltres.Controls.Add(btn);
                };

                ajouterBoutonFiltre("🌐 Toutes", null);
                ajouterBoutonFiltre("🗡️ Armes", CategorieCraft.Arme);
                ajouterBoutonFiltre("🛡️ Armures & Bijoux", CategorieCraft.Armure);
                ajouterBoutonFiltre("🔮 Grimoires & Sorts", CategorieCraft.Competence);
                ajouterBoutonFiltre("🧪 Consommables", CategorieCraft.Consommable);

                rafraichirListeRecettes();
                tabCraft.Controls.Add(tblCraft);

                // === TAB 2 : AMÉLIORATION DE STUFF ===
                FlowLayoutPanel pnlAmelioration = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Padding = new Padding(20)
                };

                Action<string, Equipement?, Action> ajouterLigneAmelioration = (typeLabel, item, onUp) =>
                {
                    Panel card = new Panel
                    {
                        Width = 880,
                        Height = 85,
                        BackColor = Color.FromArgb(24, 28, 38),
                        Margin = new Padding(0, 6, 0, 10),
                        Padding = new Padding(12)
                    };

                    Label lblItemTitre = new Label
                    {
                        Text = $"{typeLabel} : {(item != null ? item.ObtenirNomComplet() : "Aucun équipé")}",
                        Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                        ForeColor = item != null ? ObtenirCouleurRarete(item.RareteItem) : Color.Gray,
                        Location = new Point(12, 10),
                        AutoSize = true
                    };

                    string bonusTxt = item != null
                        ? $"Niveau actuel : +{item.NiveauAmelioration}  •  Prochain niveau (+{item.NiveauAmelioration + 1}) : +3 Attaque, +2 Défense, +8 PV"
                        : "Équipez un objet de cette catégorie pour le perfectionner.";

                    Label lblItemDesc = new Label
                    {
                        Text = bonusTxt,
                        Font = new Font("Segoe UI", 9f),
                        ForeColor = Color.FromArgb(180, 190, 205),
                        Location = new Point(14, 38),
                        AutoSize = true
                    };

                    Button btnUp = new Button
                    {
                        Text = "🔨 Renforcer (+1)\nCoût : 2 💎",
                        Size = new Size(160, 56),
                        Location = new Point(700, 12),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = (item != null && hero.PierresDeForge >= 2) ? Color.FromArgb(180, 100, 30) : Color.FromArgb(60, 65, 75),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Enabled = item != null && hero.PierresDeForge >= 2
                    };
                    btnUp.FlatAppearance.BorderSize = 0;
                    btnUp.Click += (s, e) =>
                    {
                        if (hero.PierresDeForge < 2)
                        {
                            MessageBox.Show("Vous n'avez pas assez de Pierres de Forge (2 Pierres requises) !", "Pierres Insuffisantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        hero.PierresDeForge -= 2;
                        onUp();
                        AjouterLog($"🔨 PERFECTIONNEMENT : {item!.ObtenirNomComplet()} renforcé au niveau +{item.NiveauAmelioration} !", Color.FromArgb(241, 196, 15));
                        DeclencherFlash(Color.FromArgb(90, 60, 20), 180);
                        AfficherImpactVisuel("🔨 RENFORCÉ +1 !", Color.Gold);
                        MettreAJourInterface();
                        rafraichirHeader();
                        dlg.Close();
                        OuvrirForge();
                    };

                    card.Controls.AddRange(new Control[] { lblItemTitre, lblItemDesc, btnUp });
                    pnlAmelioration.Controls.Add(card);
                };

                ajouterLigneAmelioration("🗡️ ARME ÉQUIPÉE", hero.ArmeEquipee, () => { if (hero.ArmeEquipee != null) hero.ArmeEquipee.NiveauAmelioration++; });
                ajouterLigneAmelioration("🛡️ ARMURE ÉQUIPÉE", hero.ArmureEquipee, () => { if (hero.ArmureEquipee != null) hero.ArmureEquipee.NiveauAmelioration++; });
                ajouterLigneAmelioration("🪖 HEAUME ÉQUIPÉ", hero.CasqueEquipe, () => { if (hero.CasqueEquipe != null) hero.CasqueEquipe.NiveauAmelioration++; });
                tabAmelioration.Controls.Add(pnlAmelioration);

                // === TAB 3 : SAC DE MATÉRIAUX & SORTS ===
                TableLayoutPanel tblMats = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Padding = new Padding(12)
                };
                tblMats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                tblMats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

                GroupBox gbStockMats = new GroupBox
                {
                    Dock = DockStyle.Fill,
                    Text = "Matériaux d'Artisanat Récoltés",
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Padding = new Padding(10)
                };

                ListBox lbStockMateriaux = new ListBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(24, 27, 36),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    ItemHeight = 24
                };
                foreach (var kvp in hero.MateriauxCraft.OrderByDescending(x => x.Value))
                {
                    lbStockMateriaux.Items.Add($"📦 {kvp.Key} : {kvp.Value} en stock");
                }
                if (hero.MateriauxCraft.Count == 0)
                {
                    lbStockMateriaux.Items.Add("Aucun matériau pour l'instant.");
                    lbStockMateriaux.Items.Add("Terrassez des monstres et des boss pour en récolter !");
                }
                gbStockMats.Controls.Add(lbStockMateriaux);
                tblMats.Controls.Add(gbStockMats, 0, 0);

                GroupBox gbSorts = new GroupBox
                {
                    Dock = DockStyle.Fill,
                    Text = "Grimoires de Sorts Débloqués",
                    ForeColor = Color.FromArgb(175, 122, 198),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Padding = new Padding(10)
                };

                ListBox lbSortsActifs = new ListBox
                {
                    Location = new Point(10, 25),
                    Size = new Size(420, 200),
                    BackColor = Color.FromArgb(24, 27, 36),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    ItemHeight = 24
                };

                foreach (var srt in hero.CompetencesDebloquees)
                {
                    string actif = (hero.CompetenceEquipee?.Id == srt.Id) ? "⭐ [ÉQUIPÉ] " : "";
                    lbSortsActifs.Items.Add($"{actif}{srt.Icone} {srt.Nom} ({srt.CoutMana} Mana)");
                }
                if (hero.CompetencesDebloquees.Count == 0)
                {
                    lbSortsActifs.Items.Add("Aucun sort forgé pour le moment.");
                    lbSortsActifs.Items.Add("Fabriquez des tomes dans l'Atelier pour apprendre des sorts !");
                }

                Label lblSortDetail = new Label
                {
                    Location = new Point(10, 235),
                    Size = new Size(420, 100),
                    ForeColor = Color.FromArgb(200, 210, 225),
                    Font = new Font("Segoe UI", 9f)
                };

                Button btnEquiperSort = new Button
                {
                    Text = "🔮 ÉQUIPER CE SORT EN COMBAT",
                    Location = new Point(10, 345),
                    Size = new Size(270, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnEquiperSort.FlatAppearance.BorderSize = 0;
                btnEquiperSort.Click += (s, e) =>
                {
                    if (lbSortsActifs.SelectedIndex >= 0 && lbSortsActifs.SelectedIndex < hero.CompetencesDebloquees.Count)
                    {
                        var sortChoisi = hero.CompetencesDebloquees[lbSortsActifs.SelectedIndex];
                        hero.CompetenceEquipee = sortChoisi;
                        AjouterLog($"🔮 Nouveau sort équipé en combat : {sortChoisi.Icone} {sortChoisi.Nom} !", Color.FromArgb(155, 89, 182));
                        MessageBox.Show($"Vous avez équipé le sort : {sortChoisi.Nom} !\nIl sera accessible via le bouton d'action violet en combat.", "Sort Équipé", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        MettreAJourInterface();
                        dlg.Close();
                        OuvrirForge();
                    }
                };

                lbSortsActifs.SelectedIndexChanged += (s, e) =>
                {
                    if (lbSortsActifs.SelectedIndex >= 0 && lbSortsActifs.SelectedIndex < hero.CompetencesDebloquees.Count)
                    {
                        var srt = hero.CompetencesDebloquees[lbSortsActifs.SelectedIndex];
                        lblSortDetail.Text = $"{srt.Icone} {srt.Nom}\n{srt.Description}\nEffet Spécial : {srt.Effet}  •  Coût : {srt.CoutMana} Mana  •  Dégâts : x{srt.MultiplicateurDegats}";
                    }
                };

                gbSorts.Controls.AddRange(new Control[] { lbSortsActifs, lblSortDetail, btnEquiperSort });
                tblMats.Controls.Add(gbSorts, 1, 0);

                tabMateriaux.Controls.Add(tblMats);

                tabs.TabPages.AddRange(new TabPage[] { tabCraft, tabAmelioration, tabMateriaux });
                dlg.Controls.Add(tabs);

                Button btnFermer = new Button
                {
                    Text = "Fermer la Forge",
                    Dock = DockStyle.Bottom,
                    Height = 40,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 45, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnFermer.FlatAppearance.BorderSize = 0;
                btnFermer.Click += (s, e) => dlg.Close();
                dlg.Controls.Add(btnFermer);

                dlg.ShowDialog(this);
                MettreAJourInterface();
            }
        }

        private void ReclamerToutesQuetesTerminees()
        {
            var quetesFinies = hero.QuetesActives.Where(q => q.EstTerminee).ToList();
            if (quetesFinies.Count == 0)
            {
                MessageBox.Show("Vous n'avez aucune prime de quête terminée à encaisser pour le moment.\nConsultez la Guilde pour accepter de nouveaux contrats !", "Aucune Quête Finie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int totalOr = 0;
            int totalXP = 0;
            int totalPierres = 0;
            int totalRep = 0;
            int totalSceaux = 0;
            List<string> itemsObtenus = new List<string>();

            foreach (var q in quetesFinies)
            {
                q.RecompenseReclamee = true;
                totalOr += q.RecompenseOr;
                totalXP += q.RecompenseXP;
                totalPierres += q.RecompensePierresForge;
                totalRep += q.RecompenseReputation;
                totalSceaux += q.RecompenseSceauxGuilde;
                if (q.RecompenseItem != null)
                {
                    hero.SacEquipements.Add(q.RecompenseItem);
                    itemsObtenus.Add(q.RecompenseItem.ObtenirNomComplet());
                }
                hero.QuetesCompleteesIds.Add(q.Id);
                hero.QuetesActives.Remove(q);
            }

            hero.Or += totalOr;
            hero.PierresDeForge += totalPierres;
            hero.ReputationGuilde += totalRep;
            hero.SceauxDeGuilde += totalSceaux;
            hero.GagnerXP(totalXP);

            AjouterLog($"\n=======================================================", Color.Gold);
            AjouterLog($"🎁 TOUTES LES PRIMES ({quetesFinies.Count}) ONT ÉTÉ ENCAISSÉES !", Color.FromArgb(241, 196, 15));
            AjouterLog($"💰 +{totalOr} Or  •  ⭐ +{totalXP} XP  •  💎 +{totalPierres} Pierres  •  🎖️ +{totalRep} Rep  •  🏺 +{totalSceaux} Sceaux", Color.White);
            if (itemsObtenus.Count > 0)
            {
                AjouterLog($"🎁 Équipements reçus : {string.Join(", ", itemsObtenus)}", Color.Cyan);
            }
            AjouterLog("=======================================================", Color.Gold);

            DeclencherFlash(Color.FromArgb(90, 80, 20), 300);
            AfficherImpactVisuel($"🎉 {quetesFinies.Count} PRIMES RENDUES !", Color.Gold);

            string resume = $"👑 FÉLICITATIONS CHAMPION !\n\nVous venez de rendre {quetesFinies.Count} contrat(s) de quête terminé(s) auprès de la Guilde d'Aethelgard.\n\n" +
                            $"TOTAL DES RÉCOMPENSES ACCORDÉES :\n" +
                            $"💰 +{totalOr} Or\n" +
                            $"⭐ +{totalXP} Points d'Expérience\n" +
                            $"💎 +{totalPierres} Pierres de Forge\n" +
                            $"🎖️ +{totalRep} Points de Réputation\n" +
                            $"🏺 +{totalSceaux} Sceaux de Guilde" +
                            (itemsObtenus.Count > 0 ? $"\n\n🎁 Équipements ajoutés au sac :\n• {string.Join("\n• ", itemsObtenus)}" : "");

            MessageBox.Show(resume, "Primes Royales Encaissées !", MessageBoxButtons.OK, MessageBoxIcon.Information);

            BasculerModeVillage();
        }

        private void OuvrirQuetes()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "📜 Grande Guilde des Aventuriers d'Aethelgard";
                dlg.Size = new Size(950, 660);
                dlg.MinimumSize = new Size(880, 600);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(16, 18, 24);
                dlg.ForeColor = Color.White;
                dlg.Font = new Font("Segoe UI", 9.5f);

                // --- 1. Header supérieur ---
                Panel pnlHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 90,
                    BackColor = Color.FromArgb(22, 26, 36),
                    Padding = new Padding(15, 10, 15, 10)
                };

                Label lblTitre = new Label
                {
                    Text = "📜 GRANDE GUILDE D'AETHELGARD",
                    Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    AutoSize = true,
                    Location = new Point(15, 10)
                };

                Label lblSousTitre = new Label
                {
                    Text = "Tableau des Primes Officielles, Rangs Héroïques & Boutique Secrète",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(160, 175, 195),
                    AutoSize = true,
                    Location = new Point(17, 36)
                };

                FlowLayoutPanel pnlStatuts = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    Height = 30,
                    FlowDirection = FlowDirection.LeftToRight,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0)
                };

                Label lblRang = new Label
                {
                    Text = $"🎖️ {hero.ObtenirRangGuilde()}",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 204, 113),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 20, 0)
                };

                Label lblRep = new Label
                {
                    Text = $"⭐ Réputation : {hero.ReputationGuilde} pts",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 152, 219),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 20, 0)
                };

                Label lblSceaux = new Label
                {
                    Text = $"🏺 Sceaux : {hero.SceauxDeGuilde}",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(230, 126, 34),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 20, 0)
                };

                Label lblBourse = new Label
                {
                    Text = $"💰 Bourse : {hero.Or} Or",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    AutoSize = true,
                    Margin = new Padding(0, 4, 0, 0)
                };

                pnlStatuts.Controls.AddRange(new Control[] { lblRang, lblRep, lblSceaux, lblBourse });
                pnlHeader.Controls.AddRange(new Control[] { lblTitre, lblSousTitre, pnlStatuts });
                dlg.Controls.Add(pnlHeader);

                // --- 2. TabControl ---
                TabControl tabs = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Padding = new Point(12, 6)
                };

                TabPage tabDispo = new TabPage("📜 Tableau des Primes (Disponibles)") { BackColor = Color.FromArgb(16, 18, 24) };
                TabPage tabActives = new TabPage($"⚔️ Mes Contrats Actifs ({hero.QuetesActives.Count}/6)") { BackColor = Color.FromArgb(16, 18, 24) };
                TabPage tabBoutique = new TabPage("🏺 Boutique de la Guilde") { BackColor = Color.FromArgb(16, 18, 24) };

                // Rafraîchisseurs
                Action rafraichirHeader = () =>
                {
                    lblRang.Text = $"🎖️ {hero.ObtenirRangGuilde()}";
                    lblRep.Text = $"⭐ Réputation : {hero.ReputationGuilde} pts";
                    lblSceaux.Text = $"🏺 Sceaux : {hero.SceauxDeGuilde}";
                    lblBourse.Text = $"💰 Bourse : {hero.Or} Or";
                    tabActives.Text = $"⚔️ Mes Contrats Actifs ({hero.QuetesActives.Count}/6)";
                };

                // === TAB 1 : QUÊTES DISPONIBLES ===
                TableLayoutPanel tblDispo = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Padding = new Padding(10)
                };
                tblDispo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));
                tblDispo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));

                ListBox lbDispo = new ListBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(24, 27, 36),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    ItemHeight = 26
                };

                Panel pnlDetailDispo = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(22, 25, 34),
                    Padding = new Padding(15)
                };

                Label lblDetTitre = new Label
                {
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    AutoSize = true,
                    Location = new Point(15, 15)
                };
                Label lblDetDifficulte = new Label
                {
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 204, 113),
                    AutoSize = true,
                    Location = new Point(15, 45)
                };
                Label lblDetLore = new Label
                {
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                    ForeColor = Color.FromArgb(200, 205, 215),
                    Size = new Size(420, 75),
                    Location = new Point(15, 75)
                };
                Label lblDetCible = new Label
                {
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(15, 160)
                };
                Label lblDetRecompenses = new Label
                {
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(240, 240, 240),
                    Size = new Size(420, 130),
                    Location = new Point(15, 190)
                };

                Button btnAccepter = new Button
                {
                    Text = "⭐ ACCEPTER CE CONTRAT",
                    Size = new Size(240, 44),
                    Location = new Point(15, 335),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnAccepter.FlatAppearance.BorderSize = 0;

                pnlDetailDispo.Controls.AddRange(new Control[] {
                    lblDetTitre, lblDetDifficulte, lblDetLore, lblDetCible, lblDetRecompenses, btnAccepter
                });

                List<Quete> listeAffichable = new List<Quete>();
                Action rechargerListeDispo = () =>
                {
                    lbDispo.Items.Clear();
                    listeAffichable = quetesDisponibles
                        .Where(q => !hero.QuetesActives.Any(a => a.Id == q.Id) && !hero.QuetesCompleteesIds.Contains(q.Id))
                        .ToList();

                    foreach (var q in listeAffichable)
                    {
                        lbDispo.Items.Add($"[{q.Difficulte}] {q.Titre} (Niv. {q.NiveauRequis}+)");
                    }
                    if (listeAffichable.Count > 0) lbDispo.SelectedIndex = 0;
                    else
                    {
                        lblDetTitre.Text = "Toutes les primes disponibles ont été acceptées !";
                        lblDetDifficulte.Text = "";
                        lblDetLore.Text = "Consultez l'onglet 'Mes Contrats Actifs' pour suivre votre progression.";
                        lblDetCible.Text = "";
                        lblDetRecompenses.Text = "";
                        btnAccepter.Enabled = false;
                    }
                };

                lbDispo.SelectedIndexChanged += (s, e) =>
                {
                    if (lbDispo.SelectedIndex >= 0 && lbDispo.SelectedIndex < listeAffichable.Count)
                    {
                        var q = listeAffichable[lbDispo.SelectedIndex];
                        lblDetTitre.Text = q.Titre;
                        lblDetDifficulte.Text = $"Difficulté : {q.Difficulte}  •  Niveau Conseillé : {q.NiveauRequis}+";
                        lblDetLore.Text = q.Description;
                        lblDetCible.Text = $"🎯 Cible : {q.Objectif}x {q.CibleNom}";
                        lblDetRecompenses.Text =
                            $"RÉCOMPENSES ROYALES :\n" +
                            $"💰 +{q.RecompenseOr} Or\n" +
                            $"⭐ +{q.RecompenseXP} Points d'Expérience\n" +
                            $"💎 +{q.RecompensePierresForge} Pierres de Forge\n" +
                            $"🎖️ +{q.RecompenseReputation} Points de Réputation\n" +
                            $"🏺 +{q.RecompenseSceauxGuilde} Sceau(x) de Guilde\n" +
                            $"🎁 Équipement : {(q.RecompenseItem != null ? q.RecompenseItem.ObtenirDescription() : "Aucun")}";

                        btnAccepter.Enabled = true;
                    }
                };

                btnAccepter.Click += (s, e) =>
                {
                    if (hero.QuetesActives.Count >= 6)
                    {
                        MessageBox.Show("Votre journal de quêtes est plein (maximum 6 contrats actifs) !\nTerminez ou abandonnez un contrat pour en accepter un nouveau.", "Journal Plein", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (lbDispo.SelectedIndex >= 0 && lbDispo.SelectedIndex < listeAffichable.Count)
                    {
                        var q = listeAffichable[lbDispo.SelectedIndex];
                        hero.QuetesActives.Add(q);
                        AjouterLog($"📜 CONTRAT ACCEPTÉ : '{q.Titre}' ! Objectif : {q.Objectif}x {q.CibleNom}.", Color.Gold);
                        MettreAJourInterface();
                        rafraichirHeader();
                        rechargerListeDispo();
                        MessageBox.Show($"Contrat '{q.Titre}' accepté avec succès !\nBonne chasse aventurier !", "Contrat Signé", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                rechargerListeDispo();
                tblDispo.Controls.Add(lbDispo, 0, 0);
                tblDispo.Controls.Add(pnlDetailDispo, 1, 0);
                tabDispo.Controls.Add(tblDispo);

                // === TAB 2 : QUÊTES ACTIVES ===
                TableLayoutPanel tblActives = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Padding = new Padding(10)
                };
                tblActives.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));
                tblActives.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));

                ListBox lbActives = new ListBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(24, 27, 36),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    ItemHeight = 26
                };

                Panel pnlDetailActives = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(22, 25, 34),
                    Padding = new Padding(15)
                };

                Label lblActTitre = new Label
                {
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    AutoSize = true,
                    Location = new Point(15, 15)
                };
                Label lblActProgression = new Label
                {
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 204, 113),
                    AutoSize = true,
                    Location = new Point(15, 50)
                };
                Label lblActLore = new Label
                {
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                    ForeColor = Color.FromArgb(200, 205, 215),
                    Size = new Size(420, 65),
                    Location = new Point(15, 80)
                };
                Label lblActRecompenses = new Label
                {
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(240, 240, 240),
                    Size = new Size(420, 130),
                    Location = new Point(15, 155)
                };

                Button btnReclamer = new Button
                {
                    Text = "🎁 RÉCLAMER",
                    Size = new Size(130, 44),
                    Location = new Point(15, 300),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnReclamer.FlatAppearance.BorderSize = 0;

                Button btnReclamerTout = new Button
                {
                    Text = "🎁 TOUT RÉCLAMER",
                    Size = new Size(160, 44),
                    Location = new Point(155, 300),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(212, 160, 23),
                    ForeColor = Color.Black,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnReclamerTout.FlatAppearance.BorderSize = 0;
                btnReclamerTout.Click += (s, e) =>
                {
                    dlg.Close();
                    ReclamerToutesQuetesTerminees();
                };

                Button btnAbandonner = new Button
                {
                    Text = "❌ Abandonner ce contrat",
                    Size = new Size(180, 36),
                    Location = new Point(15, 355),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(192, 57, 43),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnAbandonner.FlatAppearance.BorderSize = 0;

                pnlDetailActives.Controls.AddRange(new Control[] {
                    lblActTitre, lblActProgression, lblActLore, lblActRecompenses, btnReclamer, btnReclamerTout, btnAbandonner
                });

                Action rechargerListeActives = () =>
                {
                    lbActives.Items.Clear();
                    foreach (var q in hero.QuetesActives)
                    {
                        string statut = q.EstTerminee ? "⭐ [PRÊT À RÉCLAMER]" : $"({q.Progression}/{q.Objectif})";
                        lbActives.Items.Add($"{statut} {q.Titre}");
                    }

                    int nbFinies = hero.QuetesActives.Count(q => q.EstTerminee);
                    btnReclamerTout.Visible = nbFinies > 0;
                    btnReclamerTout.Text = $"🎁 TOUT ({nbFinies})";

                    if (hero.QuetesActives.Count > 0) lbActives.SelectedIndex = 0;
                    else
                    {
                        lblActTitre.Text = "Aucun contrat actif pour le moment.";
                        lblActProgression.Text = "";
                        lblActLore.Text = "Allez dans l'onglet 'Tableau des Primes' pour accepter de nouvelles quêtes !";
                        lblActRecompenses.Text = "";
                        btnReclamer.Enabled = false;
                        btnAbandonner.Enabled = false;
                    }
                };

                lbActives.SelectedIndexChanged += (s, e) =>
                {
                    if (lbActives.SelectedIndex >= 0 && lbActives.SelectedIndex < hero.QuetesActives.Count)
                    {
                        var q = hero.QuetesActives[lbActives.SelectedIndex];
                        lblActTitre.Text = q.Titre;
                        lblActProgression.Text = q.EstTerminee
                            ? $"🎉 OBJECTIF ATTEINT : {q.Progression} / {q.Objectif} terrassés !"
                            : $"🎯 Progression : {q.Progression} / {q.Objectif} terrassés";
                        lblActProgression.ForeColor = q.EstTerminee ? Color.FromArgb(46, 204, 113) : Color.FromArgb(241, 196, 15);
                        lblActLore.Text = q.Description;
                        lblActRecompenses.Text =
                            $"RÉCOMPENSES À TOUCHER :\n" +
                            $"💰 +{q.RecompenseOr} Or\n" +
                            $"⭐ +{q.RecompenseXP} Points d'Expérience\n" +
                            $"💎 +{q.RecompensePierresForge} Pierres de Forge\n" +
                            $"🎖️ +{q.RecompenseReputation} Points de Réputation\n" +
                            $"🏺 +{q.RecompenseSceauxGuilde} Sceau(x) de Guilde\n" +
                            $"🎁 Équipement : {(q.RecompenseItem != null ? q.RecompenseItem.ObtenirDescription() : "Aucun")}";

                        btnReclamer.Enabled = q.EstTerminee;
                        btnReclamer.BackColor = q.EstTerminee ? Color.FromArgb(39, 174, 96) : Color.FromArgb(60, 65, 75);
                        btnAbandonner.Enabled = true;
                    }
                };

                btnReclamer.Click += (s, e) =>
                {
                    if (lbActives.SelectedIndex >= 0 && lbActives.SelectedIndex < hero.QuetesActives.Count)
                    {
                        var q = hero.QuetesActives[lbActives.SelectedIndex];
                        if (q.EstTerminee)
                        {
                            q.RecompenseReclamee = true;
                            hero.Or += q.RecompenseOr;
                            hero.PierresDeForge += q.RecompensePierresForge;
                            hero.ReputationGuilde += q.RecompenseReputation;
                            hero.SceauxDeGuilde += q.RecompenseSceauxGuilde;
                            hero.GagnerXP(q.RecompenseXP);
                            if (q.RecompenseItem != null) hero.SacEquipements.Add(q.RecompenseItem);
                            hero.QuetesCompleteesIds.Add(q.Id);
                            hero.QuetesActives.Remove(q);

                            AjouterLog($"🎉 PRIME DE GUILDE RÉCUPÉRÉE : '{q.Titre}' (+{q.RecompenseOr} Or, +{q.RecompenseXP} XP, +{q.RecompenseReputation} Rep, +{q.RecompenseSceauxGuilde} Sceaux) !", Color.FromArgb(241, 196, 15));
                            DeclencherFlash(Color.FromArgb(90, 80, 20), 200);
                            AfficherImpactVisuel("🎉 PRIME TOUCHÉE !", Color.Gold);

                            MessageBox.Show(
                                $"Félicitations Champion !\nVous avez touché votre prime :\n\n" +
                                $"💰 +{q.RecompenseOr} Or\n" +
                                $"⭐ +{q.RecompenseXP} XP\n" +
                                $"💎 +{q.RecompensePierresForge} Pierres de Forge\n" +
                                $"🎖️ +{q.RecompenseReputation} Réputation de Guilde\n" +
                                $"🏺 +{q.RecompenseSceauxGuilde} Sceaux de Guilde" +
                                (q.RecompenseItem != null ? $"\n🎁 Équipement ajouté au sac : {q.RecompenseItem.ObtenirNomComplet()}" : ""),
                                "Prime Touchée !", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            MettreAJourInterface();
                            rafraichirHeader();
                            rechargerListeActives();
                            rechargerListeDispo();
                        }
                    }
                };

                btnAbandonner.Click += (s, e) =>
                {
                    if (lbActives.SelectedIndex >= 0 && lbActives.SelectedIndex < hero.QuetesActives.Count)
                    {
                        var q = hero.QuetesActives[lbActives.SelectedIndex];
                        var conf = MessageBox.Show($"Êtes-vous sûr de vouloir abandonner le contrat '{q.Titre}' ?", "Abandonner", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (conf == DialogResult.Yes)
                        {
                            hero.QuetesActives.Remove(q);
                            q.Progression = 0;
                            AjouterLog($"❌ Contrat '{q.Titre}' abandonné.", Color.IndianRed);
                            MettreAJourInterface();
                            rafraichirHeader();
                            rechargerListeActives();
                            rechargerListeDispo();
                        }
                    }
                };

                rechargerListeActives();
                tblActives.Controls.Add(lbActives, 0, 0);
                tblActives.Controls.Add(pnlDetailActives, 1, 0);
                tabActives.Controls.Add(tblActives);

                // === TAB 3 : BOUTIQUE DE LA GUILDE ===
                FlowLayoutPanel pnlBoutiqueFlow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    Padding = new Padding(15)
                };

                Label lblBoutiqueTitre = new Label
                {
                    Text = "🏺 ÉCHOPE SECRÈTE DE L'INTENDANT DE GUILDE",
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(230, 126, 34),
                    Width = 850,
                    Height = 24
                };
                Label lblBoutiqueDesc = new Label
                {
                    Text = "Les Sceaux de Guilde s'obtiennent en complétant les contrats royaux. Échangez-les contre ces reliques uniques :",
                    Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                    ForeColor = Color.FromArgb(170, 180, 195),
                    Width = 850,
                    Height = 22
                };
                pnlBoutiqueFlow.Controls.AddRange(new Control[] { lblBoutiqueTitre, lblBoutiqueDesc });

                // Helper pour créer un article de boutique
                Action<string, string, int, int, Action> ajouterArticle = (nom, description, coutSceaux, coutOr, onAcheter) =>
                {
                    Panel pnlItem = new Panel
                    {
                        Width = 870,
                        Height = 62,
                        BackColor = Color.FromArgb(25, 29, 40),
                        Margin = new Padding(0, 4, 0, 6),
                        Padding = new Padding(10)
                    };

                    Label lblItemNom = new Label
                    {
                        Text = nom,
                        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(241, 196, 15),
                        Location = new Point(10, 8),
                        AutoSize = true
                    };
                    Label lblItemDesc = new Label
                    {
                        Text = description,
                        Font = new Font("Segoe UI", 8.5f),
                        ForeColor = Color.FromArgb(180, 190, 205),
                        Location = new Point(10, 32),
                        AutoSize = true
                    };

                    Button btnBuy = new Button
                    {
                        Text = $"Acheter\n{coutSceaux} 🏺 + {coutOr} 💰",
                        Size = new Size(160, 44),
                        Location = new Point(690, 8),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(212, 160, 23),
                        ForeColor = Color.Black,
                        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                        Cursor = Cursors.Hand
                    };
                    btnBuy.FlatAppearance.BorderSize = 0;
                    btnBuy.Click += (s, e) =>
                    {
                        if (hero.SceauxDeGuilde < coutSceaux || hero.Or < coutOr)
                        {
                            MessageBox.Show($"Ressources insuffisantes !\nRequis : {coutSceaux} Sceaux de Guilde et {coutOr} Or.", "Achat Impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        hero.SceauxDeGuilde -= coutSceaux;
                        hero.Or -= coutOr;
                        onAcheter();
                        rafraichirHeader();
                        MettreAJourInterface();
                    };

                    pnlItem.Controls.AddRange(new Control[] { lblItemNom, lblItemDesc, btnBuy });
                    pnlBoutiqueFlow.Controls.Add(pnlItem);
                };

                ajouterArticle("🧪 Élixir de Sang de Dragon Primordial", "+5 Points d'Attributs à répartir librement !", 3, 200, () =>
                {
                    hero.PointsCaracteristiques += 5;
                    AjouterLog("✨ Vous buvez l'Élixir de Sang de Dragon : +5 Points de caractéristiques obtenus !", Color.FromArgb(241, 196, 15));
                    MessageBox.Show("+5 Points de caractéristiques ajoutés à votre Héros !", "Bénédiction du Dragon", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

                ajouterArticle("🪨 Cœur de Titan Fossilisé", "+40 PV Max de manière permanente !", 4, 300, () =>
                {
                    hero.PVMaxBase += 40;
                    hero.PVActuels += 40;
                    AjouterLog("🪨 Le Cœur de Titan renforce votre vitalité : +40 PV Max permanents !", Color.FromArgb(46, 204, 113));
                    MessageBox.Show("+40 PV Max permanents accordés !", "Vitalité Titanesque", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

                ajouterArticle("💧 Essence Stellaire Condensée", "+30 Mana Max de manière permanente !", 3, 250, () =>
                {
                    hero.ManaMaxBase += 30;
                    hero.ManaActuel += 30;
                    AjouterLog("💧 L'Essence Stellaire dilate votre réserve magique : +30 Mana Max permanents !", Color.FromArgb(52, 152, 219));
                    MessageBox.Show("+30 Mana Max permanents accordés !", "Pouvoir Stellaire", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

                ajouterArticle("🗡️ Lame Runique du Champion de Guilde", "Arme Épique exclusive : +26 Attaque, +15% Critique, +5% Vampirisme", 5, 450, () =>
                {
                    var arme = new Equipement("Lame Runique du Champion", TypeEquipement.Arme, Rarete.Epique, 26, 0, 0, 0, 15, 0, 5, 400);
                    hero.SacEquipements.Add(arme);
                    AjouterLog("🗡️ Lame Runique du Champion de Guilde ajoutée à votre sac !", Color.FromArgb(241, 196, 15));
                    MessageBox.Show("Lame Runique ajoutée dans votre Inventaire !", "Arme Acquise", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

                ajouterArticle("🛡️ Égide Protectrice d'Aethelgard", "Armure Épique : +20 Défense, +60 PV Max, +5% Esquive", 5, 450, () =>
                {
                    var armure = new Equipement("Égide Protectrice d'Aethelgard", TypeEquipement.Armure, Rarete.Epique, 0, 20, 60, 0, 0, 5, 0, 400);
                    hero.SacEquipements.Add(armure);
                    AjouterLog("🛡️ Égide Protectrice d'Aethelgard ajoutée à votre sac !", Color.FromArgb(46, 204, 113));
                    MessageBox.Show("Égide Protectrice ajoutée dans votre Inventaire !", "Armure Acquise", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

                ajouterArticle("💎 Lot de 3 Pierres de Forge Pures", "Matériaux précieux pour améliorer vos armes et armures jusqu'à +10", 2, 120, () =>
                {
                    hero.PierresDeForge += 3;
                    AjouterLog("💎 +3 Pierres de Forge pures acquises à la Guilde !", Color.FromArgb(52, 152, 219));
                    MessageBox.Show("+3 Pierres de Forge ajoutées à vos réserves !", "Matériaux Acquis", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

                ajouterArticle("🌟 Potion de Transcendance Sacrée", "Restaure 100% PV, 100% Mana et remplit immédiatement l'Ultime à 100%", 2, 150, () =>
                {
                    hero.PVActuels = hero.PVMaxTotal;
                    hero.ManaActuel = hero.ManaMaxTotal;
                    hero.JaugeUltime = 100;
                    AjouterLog("🌟 Potion de Transcendance bue : 100% PV, 100% Mana et Ultime CHARGÉ !", Color.Gold);
                    MessageBox.Show("Vous êtes restauré au maximum et votre Ultime est prêt !", "Transcendance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

                tabBoutique.Controls.Add(pnlBoutiqueFlow);

                tabs.TabPages.AddRange(new TabPage[] { tabDispo, tabActives, tabBoutique });
                dlg.Controls.Add(tabs);

                Button btnFermer = new Button
                {
                    Text = "Fermer le Panneau de la Guilde",
                    Dock = DockStyle.Bottom,
                    Height = 40,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 45, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnFermer.FlatAppearance.BorderSize = 0;
                btnFermer.Click += (s, e) => dlg.Close();
                dlg.Controls.Add(btnFermer);

                dlg.ShowDialog(this);
                MettreAJourInterface();
            }
        }

        private void OuvrirSac()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "🎒 Sac de Voyage & Équipements du Champion";
                dlg.Size = new Size(680, 580);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(20, 22, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                Label lblTitre = new Label
                {
                    Text = "🎒 INVENTAIRE & GESTION DE L'ÉQUIPEMENT",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(155, 89, 182),
                    Location = new Point(20, 12),
                    AutoSize = true
                };

                Label lblStatsHero = new Label
                {
                    Text = $"⚔️ Attaque : {hero.AttaqueTotale}   •   🎯 Critique : {hero.ChanceCritiqueTotale}%   •   🩸 Vol de Vie : {hero.VampirismeTotal}%   •   🛡️ Défense : {hero.DefenseTotale}",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(20, 38),
                    AutoSize = true
                };

                // Section Potions
                GroupBox gbPotions = new GroupBox
                {
                    Text = "Potions & Objets Rapides",
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(20, 65),
                    Size = new Size(624, 82),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };

                Button btnBoireSoin = new Button
                {
                    Text = $"🧪 Potion Soin ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure)})\n[Boire +60 PV]",
                    Location = new Point(15, 20),
                    Size = new Size(180, 48),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnBoireSoin.FlatAppearance.BorderSize = 0;

                Button btnBoireMana = new Button
                {
                    Text = $"💧 Potion Mana ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMineure)})\n[Boire +50 Mana]",
                    Location = new Point(205, 20),
                    Size = new Size(180, 48),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnBoireMana.FlatAppearance.BorderSize = 0;

                Label lblBombes = new Label
                {
                    Text = $"💣 Bombes Incendiaires :\n{hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire)} en réserve",
                    Location = new Point(400, 26),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(200, 205, 215)
                };

                gbPotions.Controls.AddRange(new Control[] { btnBoireSoin, btnBoireMana, lblBombes });

                // Section Équipements de réserve
                GroupBox gbEquip = new GroupBox
                {
                    Text = $"Équipements en Réserve ({hero.SacEquipements.Count})",
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(20, 152),
                    Size = new Size(624, 335),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };

                ListBox lbEquip = new ListBox
                {
                    Location = new Point(15, 25),
                    Size = new Size(420, 292),
                    BackColor = Color.FromArgb(28, 31, 40),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 8.8f)
                };

                Button btnEquiper = new Button
                {
                    Text = "🗡️ Équiper Sélection\n(Remplacer pièce)",
                    Location = new Point(445, 25),
                    Size = new Size(165, 46),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnEquiper.FlatAppearance.BorderSize = 0;

                Button btnEquiperMeilleur = new Button
                {
                    Text = "⚡ ÉQUIPER BEST\n(Dégâts Max 🔥)",
                    Location = new Point(445, 78),
                    Size = new Size(165, 68),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(230, 126, 34),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnEquiperMeilleur.FlatAppearance.BorderSize = 0;

                Panel pnlInfoDps = new Panel
                {
                    Location = new Point(445, 154),
                    Size = new Size(165, 163),
                    BackColor = Color.FromArgb(26, 29, 38),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label lblInfoDps = new Label
                {
                    Location = new Point(8, 8),
                    Size = new Size(147, 145),
                    Text = "⚡ AUTO-ÉQUIP DPS\n\nScanne tous les slots (Armes, Armures, Heaumes, Anneaux, Amulettes) et équipe automatiquement le stuff qui maximise votre Attaque, Critique et Dégâts totaux !",
                    ForeColor = Color.FromArgb(189, 195, 199),
                    Font = new Font("Segoe UI", 7.8f)
                };
                pnlInfoDps.Controls.Add(lblInfoDps);

                Action rafraichirInventaire = () =>
                {
                    int indexPrecedent = lbEquip.SelectedIndex;
                    lbEquip.BeginUpdate();
                    lbEquip.Items.Clear();
                    if (hero.SacEquipements.Count == 0)
                    {
                        lbEquip.Items.Add("(Votre sac est vide — Aucun équipement en réserve)");
                    }
                    else
                    {
                        foreach (var item in hero.SacEquipements)
                        {
                            lbEquip.Items.Add(item.ObtenirDescription());
                        }
                    }
                    if (indexPrecedent >= 0 && indexPrecedent < lbEquip.Items.Count)
                        lbEquip.SelectedIndex = indexPrecedent;
                    lbEquip.EndUpdate();

                    gbEquip.Text = $"Équipements en Réserve ({hero.SacEquipements.Count})";
                    lblStatsHero.Text = $"⚔️ Attaque : {hero.AttaqueTotale}   •   🎯 Critique : {hero.ChanceCritiqueTotale}%   •   🩸 Vol de Vie : {hero.VampirismeTotal}%   •   🛡️ Défense : {hero.DefenseTotale}";
                    btnBoireSoin.Text = $"🧪 Potion Soin ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure)})\n[Boire +60 PV]";
                    btnBoireMana.Text = $"💧 Potion Mana ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMineure)})\n[Boire +50 Mana]";
                    lblBombes.Text = $"💣 Bombes Incendiaires :\n{hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire)} en réserve";
                };

                btnBoireSoin.Click += (s, e) =>
                {
                    if (hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure) > 0)
                    {
                        hero.UtiliserConsommable(TypeConsommable.PotionSoinMineure);
                        hero.Soigner(60);
                        AjouterLog("🧪 Vous buvez une Potion de Soin depuis l'inventaire (+60 PV) !", Color.FromArgb(46, 204, 113));
                        MettreAJourInterface();
                        rafraichirInventaire();
                    }
                    else MessageBox.Show(dlg, "Plus de potions de soin en stock !", "Vide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };

                btnBoireMana.Click += (s, e) =>
                {
                    if (hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMineure) > 0)
                    {
                        hero.UtiliserConsommable(TypeConsommable.PotionManaMineure);
                        hero.RestaurerMana(50);
                        AjouterLog("💧 Vous buvez une Potion de Mana depuis l'inventaire (+50 Mana) !", Color.FromArgb(52, 152, 219));
                        MettreAJourInterface();
                        rafraichirInventaire();
                    }
                    else MessageBox.Show(dlg, "Plus de potions de mana en stock !", "Vide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };

                btnEquiper.Click += (s, e) =>
                {
                    if (lbEquip.SelectedIndex >= 0 && lbEquip.SelectedIndex < hero.SacEquipements.Count)
                    {
                        var selection = hero.SacEquipements[lbEquip.SelectedIndex];
                        hero.EquiperObjet(selection);
                        AjouterLog($"🛡️ Vous équipez désormais : {selection.ObtenirNomComplet()} !", Color.FromArgb(241, 196, 15));
                        MettreAJourInterface();
                        rafraichirInventaire();
                    }
                    else MessageBox.Show(dlg, "Veuillez sélectionner un équipement valide dans la liste !", "Sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                btnEquiperMeilleur.Click += (s, e) =>
                {
                    if (hero.SacEquipements.Count == 0)
                    {
                        MessageBox.Show(dlg, "Votre sac est vide ! Obtenez des équipements en donjon, sur les boss ou à la forge.", "Sac vide", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var res = hero.AutoEquiperMeilleurStuffDegats();
                    if (res.ChangementsCount > 0)
                    {
                        rafraichirInventaire();
                        MettreAJourInterface();
                        DeclencherFlash(Color.FromArgb(120, 80, 0), 220);
                        AfficherImpactVisuel("⚡ MEILLEUR STUFF ÉQUIPÉ !", Color.Gold);

                        string msgLog = $"⚡ AUTO-ÉQUIP : {res.ChangementsCount} pièce(s) optimisée(s) ! Attaque : {res.AncienneAtk} ➜ {hero.AttaqueTotale} ({(res.GainAttaque >= 0 ? "+" : "")}{res.GainAttaque}), Critique : {res.AncienCrit}% ➜ {hero.ChanceCritiqueTotale}%.";
                        AjouterLog(msgLog, Color.FromArgb(241, 196, 15));

                        string resume = "⚡ OPTIMISATION DÉGÂTS MAX RÉUSSIE !\n\n" +
                                        "Nouvelles pièces équipées automatiquement :\n" +
                                        string.Join("\n", res.Details) + "\n\n" +
                                        "📊 Bilan de vos Statistiques :\n" +
                                        $"• ⚔️ Attaque Totale  : {res.AncienneAtk} ➜ {hero.AttaqueTotale} ({(res.GainAttaque >= 0 ? "+" : "")}{res.GainAttaque})\n" +
                                        $"• 🎯 Chance Critique : {res.AncienCrit}% ➜ {hero.ChanceCritiqueTotale}% ({(res.GainCritique >= 0 ? "+" : "")}{res.GainCritique}%)\n" +
                                        $"• 🩸 Vol de Vie      : {hero.VampirismeTotal}%\n" +
                                        $"• 🛡️ Défense Totale : {hero.DefenseTotale}\n\n" +
                                        "Votre champion est équipé avec le stuff le plus puissant pour frapper fort !";

                        MessageBox.Show(dlg, resume, "⚡ Équipement Optimal Dégâts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(dlg,
                            $"⚡ Votre équipement actuel est déjà optimal pour les dégâts !\n\nAucune pièce dans votre réserve ne permet d'augmenter davantage votre Attaque Totale ou votre potentiel offensif.\n\nStats actuelles :\n• ⚔️ Attaque Totale : {hero.AttaqueTotale}\n• 🎯 Chance Critique : {hero.ChanceCritiqueTotale}%\n• 🩸 Vol de Vie : {hero.VampirismeTotal}%",
                            "Équipement Déjà Optimal",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                };

                gbEquip.Controls.AddRange(new Control[] { lbEquip, btnEquiper, btnEquiperMeilleur, pnlInfoDps });

                Button btnFermer = new Button
                {
                    Text = "Fermer l'Inventaire",
                    Location = new Point(245, 496),
                    Size = new Size(190, 38),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnFermer.FlatAppearance.BorderSize = 0;
                btnFermer.Click += (s, e) => dlg.Close();

                dlg.Controls.AddRange(new Control[] { lblTitre, lblStatsHero, gbPotions, gbEquip, btnFermer });

                // Initialiser le sac
                rafraichirInventaire();

                dlg.ShowDialog(this);
            }
        }

        private void AfficherAide()
        {
            string guide = "📜 CHRONIQUES D'AETHELGARD — GUIDE OFFICIEL\n\n" +
                           "• Combat : Alternez entre l'Attaque Normale (gain d'Ultime), l'Attaque Lourde et vos Sorts de classe.\n" +
                           "• Jauge Ultime : Remplissez-la à 100% en combattant pour déclencher une frappe dévastatrice !\n" +
                           "• Progression : Chaque niveau vous octroie 4 points à répartir dans vos attributs (+FOR, +AGI, +END, +INT).\n" +
                           "• Équipement & Forge : Ramassez des Pierres de Forge sur les monstres pour upgrader votre équipement jusqu'à +10 !\n" +
                           "• Sauvegarde : Utilisez le bouton 'Sauvegarder' en haut pour conserver vos données.";

            MessageBox.Show(guide, "Guide & Astuces", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ==============================================================
        // CRÉATION DE PERSONNAGE
        // ==============================================================
        private void AfficherDialogueCreationPersonnage()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Création de votre Champion";
                dlg.Size = new Size(520, 420);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(20, 22, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                Label lblTitre = new Label
                {
                    Text = "FORGEZ VOTRE LÉGENDE",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(30, 18),
                    AutoSize = true
                };

                Label lblNom = new Label { Text = "Nom de votre Héros :", Location = new Point(30, 55), AutoSize = true };
                TextBox txtNom = new TextBox
                {
                    Text = "Eldrin",
                    Location = new Point(30, 80),
                    Width = 440,
                    BackColor = Color.FromArgb(30, 33, 42),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 10f)
                };

                Label lblClasse = new Label { Text = "Choisissez votre Classe Héroïque :", Location = new Point(30, 120), AutoSize = true };
                ComboBox cbClasse = new ComboBox
                {
                    Location = new Point(30, 145),
                    Width = 440,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(30, 33, 42),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f)
                };
                cbClasse.Items.AddRange(new object[] {
                    "🛡️ GUERRIER (Colosse en armure lourde, gros PV, frappes sismiques)",
                    "🔮 MAGE (Archimage arcanique, déflagration de feu, grand réservoir de Mana)",
                    "🏹 RÔDEUR (Traqueur véloce, coups critiques mortels, haute esquive)",
                    "⚔️ PALADIN (Chevalier divin, châtiment sacré, régénération et défense)",
                    "💀 NÉCROMANCIEN (Maître des ombres, drain vampirique et décrépitude)"
                });
                cbClasse.SelectedIndex = 0;

                Label lblInfos = new Label
                {
                    Location = new Point(30, 190),
                    Size = new Size(440, 90),
                    ForeColor = Color.FromArgb(170, 180, 195),
                    Text = "Chaque classe possède son propre équipement légendaire de départ, ses aptitudes magiques uniques, et ses bonus de caractéristiques personnalisés !"
                };

                Button btnStart = new Button
                {
                    Text = "⚔️ COMMENCER L'AVENTURE",
                    Location = new Point(110, 300),
                    Size = new Size(280, 50),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnStart.FlatAppearance.BorderSize = 0;
                btnStart.Click += (s, e) => dlg.DialogResult = DialogResult.OK;

                dlg.Controls.AddRange(new Control[] { lblTitre, lblNom, txtNom, lblClasse, cbClasse, lblInfos, btnStart });

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string nom = string.IsNullOrWhiteSpace(txtNom.Text) ? "Héros" : txtNom.Text.Trim();
                    ClasseType classe = (ClasseType)cbClasse.SelectedIndex;
                    hero = new Joueur(nom, classe);
                    AjouterLog($"✨ Bienvenue en Aethelgard, {hero.Nom} le {hero.Classe} ! Que votre légende commence !", Color.FromArgb(241, 196, 15));
                }
                else
                {
                    hero = new Joueur("Héros", ClasseType.Guerrier);
                }
            }
        }

        // ==============================================================
        // SYSTÈME DE DONJONS & RAIDS DE FOU
        // ==============================================================
        private void OuvrirMenuDonjons()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "🏰 EXPÉDITIONS & RAIDS DE FOU — AETHELGARD";
                dlg.Size = new Size(760, 560);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(16, 18, 24);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                Label lblTitre = new Label
                {
                    Text = "🏰 EXPÉDITIONS & RAIDS DE FOU",
                    Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(231, 76, 60),
                    Location = new Point(20, 15),
                    AutoSize = true
                };

                Label lblDesc = new Label
                {
                    Text = "Attention : Dans les Donjons, vos PV et votre Mana se conservent entre les salles !\nPréparez vos potions et affrontez des monstres élites, des énigmes et des Boss colossaux !",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(180, 190, 205),
                    Location = new Point(22, 44),
                    Size = new Size(700, 36)
                };

                FlowLayoutPanel pnlDonjons = new FlowLayoutPanel
                {
                    Location = new Point(20, 90),
                    Size = new Size(705, 380),
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true
                };

                // Helper pour créer une carte de Donjon
                Action<string, string, string, string, int, Color, Func<ModeleDonjon>> ajouterCarteDonjon =
                    (nom, sousTitre, difficulte, recompenses, nivMin, couleur, createur) =>
                {
                    Panel pnl = new Panel
                    {
                        Width = 680,
                        Height = 110,
                        BackColor = Color.FromArgb(24, 27, 36),
                        Margin = new Padding(0, 0, 0, 12),
                        Padding = new Padding(12)
                    };

                    Label lblNom = new Label
                    {
                        Text = nom,
                        Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                        ForeColor = couleur,
                        Location = new Point(10, 8),
                        AutoSize = true
                    };

                    Label lblSous = new Label
                    {
                        Text = sousTitre,
                        Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                        ForeColor = Color.FromArgb(200, 205, 215),
                        Location = new Point(12, 32),
                        AutoSize = true
                    };

                    Label lblInfo = new Label
                    {
                        Text = $"Difficulté : {difficulte}  •  Niveau Recommandé : {nivMin}+\nRécompenses : {recompenses}",
                        Font = new Font("Segoe UI", 8.5f),
                        ForeColor = Color.FromArgb(170, 180, 195),
                        Location = new Point(12, 56),
                        AutoSize = true
                    };

                    Button btnEntrer = new Button
                    {
                        Text = "⚔️ ENTRER DANS\nLE DONJON",
                        Size = new Size(150, 52),
                        Location = new Point(515, 28),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = couleur,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Cursor = Cursors.Hand
                    };
                    btnEntrer.FlatAppearance.BorderSize = 0;
                    btnEntrer.Click += (s, e) =>
                    {
                        dlg.Close();
                        LancerDonjon(createur());
                    };

                    pnl.Controls.AddRange(new Control[] { lblNom, lblSous, lblInfo, btnEntrer });
                    pnlDonjons.Controls.Add(pnl);
                };

                ajouterCarteDonjon(
                    "💀 LE LABYRINTHE DES DAMNÉS",
                    "4 Salles de catacombes hantées, autels de sang et la Liche Mor'Gath",
                    "⚔️ Dangereux (Niv. 3+)",
                    "+600 Or, +900 XP, +5 Pierres, +120 Rep, +3 Sceaux, Heaume Ancien",
                    3,
                    Color.FromArgb(155, 89, 182),
                    () => CreerDonjonLabyrinthe()
                );

                ajouterCarteDonjon(
                    "🌋 LA FAILLE ABYSSALE DU VOLCAN",
                    "5 Salles de magma incandescent, enclume des Titans et Belial le Seigneur Démoniaque",
                    "🔥 Cauchemar (Niv. 6+)",
                    "+1500 Or, +2000 XP, +8 Pierres, +200 Rep, +5 Sceaux, Plastron Cœur-de-Braise",
                    6,
                    Color.FromArgb(230, 126, 34),
                    () => CreerDonjonFailleVolcan()
                );

                ajouterCarteDonjon(
                    "🌌 LA CITADELLE CÉLESTE DU CHAOS",
                    "6 Salles de Raid Hardcore, Doppelgänger, Titan Valdorak & Chevalier du Néant",
                    "👑 Hardcore Extrême (Niv. 10+)",
                    "+5000 Or, +8000 XP, +20 Pierres, +600 Rep, +15 Sceaux, Lame Suprême du Néant",
                    10,
                    Color.FromArgb(231, 76, 60),
                    () => CreerDonjonCitadelleChaos()
                );

                Button btnFermer = new Button
                {
                    Text = "Retour au Village",
                    Size = new Size(160, 36),
                    Location = new Point(290, 480),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnFermer.FlatAppearance.BorderSize = 0;
                btnFermer.Click += (s, e) => dlg.Close();

                dlg.Controls.AddRange(new Control[] { lblTitre, lblDesc, pnlDonjons, btnFermer });
                dlg.ShowDialog(this);
            }
        }

        private ModeleDonjon CreerDonjonLabyrinthe()
        {
            return new ModeleDonjon
            {
                Nom = "Le Labyrinthe des Damnés",
                SousTitre = "Crypte Nécromantique",
                NiveauRecommande = 3,
                Couleur = Color.FromArgb(155, 89, 182),
                PrimeOr = 600,
                PrimeXP = 900,
                PrimePierres = 5,
                PrimeReputation = 120,
                PrimeSceaux = 3,
                PrimeEquipement = new Equipement("Heaume Ancien du Templier Maudit", TypeEquipement.Casque, Rarete.Epique, 0, 15, 45, 20, 0, 0, 0, 300),
                Etapes = new List<EtapeDonjon>
                {
                    new EtapeDonjon
                    {
                        SalleNumero = 1,
                        NomSalle = "L'Antichambre des Cryptes",
                        DescriptionLore = "Des relents d'ossements et de poussière millénaire vous accueillent.",
                        EstCombat = true,
                        CreerEnnemi = () => new Monstre("Squelette Gardien d'Élite", 140, 28, 12, 180, 90, false, "GARDE À VOUS !", "Sacré", "Garde d'honneur momifié des cryptes.")
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 2,
                        NomSalle = "Le Puits Sacrificiel",
                        DescriptionLore = "Une vasque de sang sombre et une fontaine d'eau bénite reposent devant vous.",
                        EstCombat = false,
                        Choix1Texte = "🩸 Boire au Puits de Sang\n(-25 PV, +5 Attaque permanente)",
                        Choix1Action = (f) =>
                        {
                            f.hero.PVActuels = Math.Max(1, f.hero.PVActuels - 25);
                            f.hero.AttaqueBase += 5;
                            f.AjouterLog("🩸 Vous buvez le sang sacrificiel : vos veines brûlent d'une rage obscure (+5 Attaque permanente, -25 PV) !", Color.FromArgb(231, 76, 60));
                            f.DeclencherFlash(Color.FromArgb(90, 20, 20), 160);
                            f.SecouerEcran(8, 6);
                        },
                        Choix2Texte = "✨ Boire à la Fontaine Bénie\n(+60 PV & +40 Mana restaurés)",
                        Choix2Action = (f) =>
                        {
                            f.hero.Soigner(60);
                            f.hero.RestaurerMana(40);
                            f.AjouterLog("✨ L'eau sacrée apaise vos plaies (+60 PV & +40 Mana restaurés) !", Color.FromArgb(46, 204, 113));
                            f.DeclencherFlash(Color.FromArgb(20, 80, 40), 160);
                        }
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 3,
                        NomSalle = "Le Couloir des Supplices",
                        DescriptionLore = "Un monstre gigantesque armé d'un hachoir maculé de sang vous bloque le chemin !",
                        EstCombat = true,
                        CreerEnnemi = () => new Monstre("Le Bourreau des Cryptes", 290, 42, 16, 380, 190, false, "DE LA CHAIR FRAÎCHE !", "Feu", "Tortionnaire colossal rôdant dans les ombres.")
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 4,
                        NomSalle = "Le Trône Sépulcral",
                        DescriptionLore = "Le Seigneur de la Mort en personne vous attend sur son trône d'ossements !",
                        EstCombat = true,
                        CreerEnnemi = () => new BossMorGath()
                    }
                }
            };
        }

        private ModeleDonjon CreerDonjonFailleVolcan()
        {
            return new ModeleDonjon
            {
                Nom = "La Faille Abyssale du Volcan",
                SousTitre = "Gouffre Incandescent",
                NiveauRecommande = 6,
                Couleur = Color.FromArgb(230, 126, 34),
                PrimeOr = 1500,
                PrimeXP = 2000,
                PrimePierres = 8,
                PrimeReputation = 200,
                PrimeSceaux = 5,
                PrimeEquipement = new Equipement("Plastron Cœur-de-Braise Ignifugé", TypeEquipement.Armure, Rarete.Legendaire, 0, 24, 75, 0, 0, 0, 0, 700),
                Etapes = new List<EtapeDonjon>
                {
                    new EtapeDonjon
                    {
                        SalleNumero = 1,
                        NomSalle = "La Brèche Incandescente",
                        DescriptionLore = "La chaleur suffocante fait bouillonner la roche à vos pieds.",
                        EstCombat = true,
                        CreerEnnemi = () => new Monstre("Salamandre de Braise", 260, 45, 18, 350, 180, false, "SHHHH !", "Glace", "Reptile enflammé crachant des flammèches.")
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 2,
                        NomSalle = "L'Enclume des Titans Nains",
                        DescriptionLore = "Une forge runique abandonnée fonctionne encore toute seule avec le magma.",
                        EstCombat = false,
                        Choix1Texte = "🔨 Forger l'Arme Équipée\n(+1 Niveau d'Amélioration gratuit !)",
                        Choix1Action = (f) =>
                        {
                            if (f.hero.ArmeEquipee != null)
                            {
                                f.hero.ArmeEquipee.NiveauAmelioration++;
                                f.AjouterLog($"🔨 FORGE DE DONJON : Votre arme est trempée dans le magma (+1 Amélioration : {f.hero.ArmeEquipee.ObtenirNomComplet()}) !", Color.Gold);
                                f.DeclencherFlash(Color.FromArgb(90, 80, 20), 180);
                            }
                        },
                        Choix2Texte = "🔥 Inhaler les Cendres Arcaniques\n(Jauge Ultime chargée à 100%)",
                        Choix2Action = (f) =>
                        {
                            f.hero.JaugeUltime = 100;
                            f.AjouterLog("🔥 L'ardeur du volcan emplit votre cœur : JAUGE D'ULTIME À 100% !", Color.Gold);
                            f.DeclencherFlash(Color.FromArgb(90, 40, 10), 160);
                        }
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 3,
                        NomSalle = "Le Gouffre de Lave Liquide",
                        DescriptionLore = "Un colosse de roche en fusion émerge des profondeurs bouillonnantes !",
                        EstCombat = true,
                        CreerEnnemi = () => new Monstre("Colosse de Lave Primordiale", 420, 52, 22, 550, 260, false, "KRAKATOAAA !", "Glace", "Monolithe de magma liquide et obsidienne.")
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 4,
                        NomSalle = "Le Pacte du Trésor Maudit",
                        DescriptionLore = "Un coffre d'obsidienne orné de rubis géants et un cristal de régénération.",
                        EstCombat = false,
                        Choix1Texte = "💰 Piller le Trésor d'Obsidienne\n(+350 Or & +2 Pierres de Forge)",
                        Choix1Action = (f) =>
                        {
                            f.hero.Or += 350;
                            f.hero.PierresDeForge += 2;
                            f.AjouterLog("💰 Vous pillez le coffre : +350 Or et +2 Pierres de Forge !", Color.Gold);
                            f.DeclencherFlash(Color.FromArgb(90, 80, 20), 160);
                        },
                        Choix2Texte = "🛡️ Absorber le Cristal Tellurique\n(+5 Défense permanente)",
                        Choix2Action = (f) =>
                        {
                            f.hero.DefenseBase += 5;
                            f.AjouterLog("🛡️ Le cristal fusionne avec votre peau : +5 Défense permanente !", Color.FromArgb(46, 204, 113));
                            f.DeclencherFlash(Color.FromArgb(40, 70, 90), 160);
                        }
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 5,
                        NomSalle = "La Chambre du Brasier Éternel",
                        DescriptionLore = "Belial, le Souverain Démoniaque en personne se dresse au centre de la caldera !",
                        EstCombat = true,
                        CreerEnnemi = () => new BossBelial()
                    }
                }
            };
        }

        private ModeleDonjon CreerDonjonCitadelleChaos()
        {
            return new ModeleDonjon
            {
                Nom = "La Citadelle Céleste du Chaos",
                SousTitre = "Raid Suprême Endgame",
                NiveauRecommande = 10,
                Couleur = Color.FromArgb(231, 76, 60),
                PrimeOr = 5000,
                PrimeXP = 8000,
                PrimePierres = 20,
                PrimeReputation = 600,
                PrimeSceaux = 15,
                PrimeEquipement = new Equipement("Lame Suprême du Néant Primordial", TypeEquipement.Arme, Rarete.Mythique, 46, 12, 50, 40, 22, 10, 10, 2500),
                Etapes = new List<EtapeDonjon>
                {
                    new EtapeDonjon
                    {
                        SalleNumero = 1,
                        NomSalle = "Le Pont Astral des Étoiles Mortes",
                        DescriptionLore = "Des fragments de météorites flottent dans le vide infini.",
                        EstCombat = true,
                        CreerEnnemi = () => new Monstre("Sentinelle Stellaire Déchue", 480, 60, 26, 750, 350, false, "LE NÉANT VOUS ATTEND...", "Cosmique", "Sentinelle cosmique protégeant les portes de la citadelle.")
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 2,
                        NomSalle = "La Galerie des Échos Miroirs",
                        DescriptionLore = "Votre propre reflet s'échappe d'un miroir brisé pour vous détruire !",
                        EstCombat = true,
                        CreerEnnemi = () => new Monstre($"Ombre Miroir de {hero.Nom}", (int)(hero.PVMaxTotal * 0.9), (int)(hero.AttaqueTotale * 0.85), (int)(hero.DefenseTotale * 0.75), 1200, 600, true, "JE SUIS CE QUE TU CRAINS LE PLUS !", "Lumière", "Double ténébreux né de vos doutes.")
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 3,
                        NomSalle = "L'Observatoire Cosmique",
                        DescriptionLore = "Une fontaine de lumière céleste pulse d'une énergie divine pure.",
                        EstCombat = false,
                        Choix1Texte = "🌌 Bénédiction des Astres\n(+50 PV Max & +30 Mana Max permanents)",
                        Choix1Action = (f) =>
                        {
                            f.hero.PVMaxBase += 50;
                            f.hero.PVActuels += 50;
                            f.hero.ManaMaxBase += 30;
                            f.hero.ManaActuel += 30;
                            f.AjouterLog("🌌 Bénédiction des Astres reçue : +50 PV Max et +30 Mana Max permanents !", Color.FromArgb(155, 89, 182));
                            f.DeclencherFlash(Color.FromArgb(80, 40, 90), 180);
                        },
                        Choix2Texte = "⚡ Résonance Temporelle\n(+4 Pierres de Forge & Ultime 100%)",
                        Choix2Action = (f) =>
                        {
                            f.hero.PierresDeForge += 4;
                            f.hero.JaugeUltime = 100;
                            f.AjouterLog("⚡ Résonance Temporelle : +4 Pierres de Forge et JAUGE D'ULTIME CHARGÉE !", Color.Gold);
                            f.DeclencherFlash(Color.FromArgb(90, 80, 20), 180);
                        }
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 4,
                        NomSalle = "Le Gouffre du Titan Inébranlable",
                        DescriptionLore = "Valdorak, le Colosse de Granite Primordial garde le chemin des cieux !",
                        EstCombat = true,
                        CreerEnnemi = () => new BossValdorak()
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 5,
                        NomSalle = "Le Sanctuaire Venimeux des Abysses",
                        DescriptionLore = "Azkalith, la Reine Vipère glisse sur les dalles de marbre céleste !",
                        EstCombat = true,
                        CreerEnnemi = () => new BossAzkalith()
                    },
                    new EtapeDonjon
                    {
                        SalleNumero = 6,
                        NomSalle = "Le Trône du Vide Primordial",
                        DescriptionLore = "L'Entité Suprême d'Aethelgard : Le Chevalier du Néant Primordial vous défie !",
                        EstCombat = true,
                        CreerEnnemi = () => new BossChevalierDuNeant()
                    }
                }
            };
        }

        private void LancerDonjon(ModeleDonjon donjon)
        {
            donjonActuel = donjon;
            indexSalleDonjon = 0;

            AjouterLog("\n=======================================================", donjon.Couleur);
            AjouterLog($"🏰 EXPÉDITION ENGAGÉE : {donjon.Nom.ToUpper()} !", Color.Gold);
            AjouterLog($"Niveau Recommandé : {donjon.NiveauRecommande}+ | {donjon.Etapes.Count} Salles à conquérir !", Color.White);
            AjouterLog("=======================================================", donjon.Couleur);

            SecouerEcran(8, 6);
            DeclencherFlash(donjon.Couleur, 200);

            TraiterSalleDonjonActuelle();
        }

        private void TraiterSalleDonjonActuelle()
        {
            if (donjonActuel == null) return;
            if (indexSalleDonjon >= donjonActuel.Etapes.Count)
            {
                TerminerDonjonSucces();
                return;
            }

            var etape = donjonActuel.Etapes[indexSalleDonjon];
            lblBandeauLieu.Text = $"🏰 {donjonActuel.Nom} — Salle {indexSalleDonjon + 1}/{donjonActuel.Etapes.Count} : {etape.NomSalle}";
            lblBandeauLieu.ForeColor = donjonActuel.Couleur;

            AjouterLog($"\n📍 Entrée dans la Salle {indexSalleDonjon + 1}/{donjonActuel.Etapes.Count} : {etape.NomSalle}", donjonActuel.Couleur);
            AjouterLog(etape.DescriptionLore, Color.FromArgb(200, 205, 215));

            if (etape.EstCombat)
            {
                Monstre monstre = etape.CreerEnnemi!();
                LancerCombat(monstre);
            }
            else
            {
                // Salle d'événement interactif
                lblCombatsTitre.Text = "🔮 ÉVÉNEMENT & CHOIX";
                pnlCombatOnglets.Visible = false;

                lblAreneTitre.Text = $"🏰 {donjonActuel.Nom.ToUpper()} — ÉVÉNEMENT";
                lblAreneTitre.ForeColor = Color.Gold;
                lblEnnemiNom.Text = etape.NomSalle;
                lblEnnemiBadge.Text = "[ÉVÉNEMENT DE SALLE]";
                lblEnnemiBadge.ForeColor = Color.Gold;
                lblEnnemiFaiblesseLore.Text = etape.DescriptionLore;
                pbEnnemiPV.Maximum = 100;
                pbEnnemiPV.Valeur = 0;
                pbEnnemiPV.TexteCentral = "Faites votre choix tactique ci-dessous !";

                pnlActionButtons.Controls.Clear();

                DarkButton btnChoix1 = CreerActionButton("Option 1", etape.Choix1Texte, Color.FromArgb(142, 68, 173), Color.FromArgb(155, 89, 182), (s, e) =>
                {
                    etape.Choix1Action?.Invoke(this);
                    MettreAJourInterface();
                    AfficherBoutonSalleSuivante();
                }, 240, 52);

                DarkButton btnChoix2 = CreerActionButton("Option 2", etape.Choix2Texte, Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113), (s, e) =>
                {
                    etape.Choix2Action?.Invoke(this);
                    MettreAJourInterface();
                    AfficherBoutonSalleSuivante();
                }, 240, 52);

                pnlActionButtons.Controls.AddRange(new Control[] { btnChoix1, btnChoix2 });
            }
        }

        private void AfficherBoutonSalleSuivante()
        {
            lblCombatsTitre.Text = "➡️ VOIE DÉGAGÉE";
            pnlCombatOnglets.Visible = false;
            pnlActionButtons.Controls.Clear();
            DarkButton btnSuivant = CreerActionButton("➡️ SALLE SUIVANTE", $"Pénétrer dans la Salle {indexSalleDonjon + 2}", Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), (s, e) =>
            {
                indexSalleDonjon++;
                TraiterSalleDonjonActuelle();
            }, 240, 52);
            pnlActionButtons.Controls.Add(btnSuivant);
        }

        private void TerminerDonjonSucces()
        {
            if (donjonActuel == null) return;

            SecouerEcran(16, 12);
            DeclencherFlash(Color.FromArgb(100, 85, 20), 300);
            AfficherImpactVisuel("🏆 DONJON CONQUIS !", Color.Gold);

            AjouterLog("\n=======================================================", Color.Gold);
            AjouterLog($"🏆 VICTOIRE TOTALE DU DONJON : {donjonActuel.Nom.ToUpper()} CONQUIS !", Color.Gold);
            AjouterLog($"💰 Butin Royal : +{donjonActuel.PrimeOr} Or  •  ⭐ +{donjonActuel.PrimeXP} XP  •  💎 +{donjonActuel.PrimePierres} Pierres !", Color.White);
            AjouterLog($"🎖️ Réputation de Guilde : +{donjonActuel.PrimeReputation} pts  •  🏺 +{donjonActuel.PrimeSceaux} Sceaux de Guilde !", Color.Gold);

            hero.Or += donjonActuel.PrimeOr;
            hero.GagnerXP(donjonActuel.PrimeXP);
            hero.PierresDeForge += donjonActuel.PrimePierres;
            hero.ReputationGuilde += donjonActuel.PrimeReputation;
            hero.SceauxDeGuilde += donjonActuel.PrimeSceaux;

            if (donjonActuel.PrimeEquipement != null)
            {
                hero.SacEquipements.Add(donjonActuel.PrimeEquipement);
                AjouterLog($"🎁 TRÉSOR MYTHIQUE OBTENU : {donjonActuel.PrimeEquipement.ObtenirDescription()} !", Color.FromArgb(241, 196, 15));
            }
            AjouterLog("=======================================================", Color.Gold);

            string msgVictoire =
                $"🎉 FÉLICITATIONS CHAMPION D'AETHELGARD !\n\n" +
                $"Vous avez conquis '{donjonActuel.Nom}' jusqu'à la dernière salle !\n\n" +
                $"Gains du Donjon :\n" +
                $"💰 +{donjonActuel.PrimeOr} Or\n" +
                $"⭐ +{donjonActuel.PrimeXP} XP\n" +
                $"💎 +{donjonActuel.PrimePierres} Pierres de Forge\n" +
                $"🎖️ +{donjonActuel.PrimeReputation} Réputation de Guilde\n" +
                $"🏺 +{donjonActuel.PrimeSceaux} Sceaux de Guilde\n" +
                (donjonActuel.PrimeEquipement != null ? $"🎁 Équipement Légendaire : {donjonActuel.PrimeEquipement.ObtenirNomComplet()}" : "");

            MessageBox.Show(msgVictoire, "Donjon Triomphal !", MessageBoxButtons.OK, MessageBoxIcon.Information);

            donjonActuel = null;
            indexSalleDonjon = 0;
            BasculerModeVillage();
        }

        private void InitialiserQuetes()
        {
            quetesDisponibles = new List<Quete>
            {
                // RANG F & E : Débutant & Chasseur
                new Quete(1, "Nettoyage des Maraudeurs", "Éliminez 3 Gobelins Maraudeurs dans la forêt des brumes.", "Gobelin Maraudeur", 3, 110, 160, 2,
                    new Equipement("Dague Dentelée de l'Ombre", TypeEquipement.Arme, Rarete.Rare, 12, 0, 0, 0, 8, 4, 0, 80), 35, 1, "🌱 Facile", 1),

                new Quete(2, "Menace des Bêtes Sauvages", "Chassez 3 Sangliers Enragés aux abords du village.", "Sanglier Enragé", 3, 120, 180, 2,
                    new Equipement("Ceinturon Renforcé en Cuir", TypeEquipement.Armure, Rarete.Commun, 0, 7, 20, 0, 0, 0, 0, 75), 40, 1, "🌱 Facile", 1),

                new Quete(3, "La Meute de l'Ombre", "Traquez 2 Loups Alpha des Brumes dans les bois sombres.", "Loup Alpha des Brumes", 2, 140, 210, 2,
                    new Equipement("Bottes de Traqueur Furtif", TypeEquipement.Armure, Rarete.Rare, 0, 6, 15, 0, 0, 6, 0, 95), 45, 1, "🌱 Facile", 1),

                // RANG D & C : Vétéran & Élite
                new Quete(4, "Purification de la Crypte", "Purifiez les catacombes de 2 Squelettes Gardiens.", "Squelette Gardien", 2, 180, 280, 3,
                    new Equipement("Heaume Renforcé du Templier", TypeEquipement.Casque, Rarete.Rare, 0, 9, 28, 10, 0, 0, 0, 130), 55, 2, "⚔️ Moyenne", 2),

                new Quete(5, "Repos des Âmes Tourmentées", "Exorcisez 2 Spectres Tourmentés dans les profondeurs.", "Spectre Tourmenté", 2, 240, 360, 3,
                    new Equipement("Amulette de Protection Spectrale", TypeEquipement.Amulette, Rarete.Rare, 0, 6, 20, 25, 0, 4, 0, 160), 65, 2, "⚔️ Moyenne", 3),

                new Quete(6, "Le Colosse de Givre", "Terrassez le titanesque Golem de Glace Ancestral.", "Golem de Glace Ancestral", 1, 380, 520, 4,
                    new Equipement("Glaive Frigorigène d'Élite", TypeEquipement.Arme, Rarete.Epique, 20, 3, 20, 0, 10, 0, 0, 240), 85, 3, "🔥 Élite", 4),

                new Quete(7, "Chasse au Drake Ardent", "Abattez un féroce Drake de Magma dans la caldera.", "Drake de Magma", 1, 500, 750, 5,
                    new Equipement("Plastron Ignifugé en Écailles", TypeEquipement.Armure, Rarete.Epique, 0, 16, 45, 0, 0, 0, 0, 320), 100, 3, "🔥 Élite", 6),

                // RANG B & A : Boss Légendaires
                new Quete(8, "Contrat Royal : Grok le Sanguinaire", "Éradiquez Grok, Seigneur de la Horde Gobeline.", "Grok, Seigneur de la Horde Gobeline", 1, 450, 650, 4,
                    new Equipement("Hache de Guerre Fracassante", TypeEquipement.Arme, Rarete.Epique, 24, 4, 0, 0, 12, 0, 0, 280), 110, 3, "💀 Boss", 2),

                new Quete(9, "Contrat Sacré : Malakor la Terreur", "Annihilez Malakor, Seigneur de la Mort Éternelle.", "Malakor, Seigneur de la Mort Éternelle", 1, 750, 1100, 6,
                    new Equipement("Robe Funeste de l'Archi-Nécro", TypeEquipement.Armure, Rarete.Epique, 0, 14, 30, 60, 0, 0, 8, 380), 150, 4, "💀 Boss", 4),

                new Quete(10, "Châtiment Boréal : Kaelas la Reine", "Vainquez Kaelas, Archifée du Blizzard Éternel.", "Kaelas, Archifée du Blizzard Éternel", 1, 1300, 1800, 7,
                    new Equipement("Sceptre Boréal de Cristal Pur", TypeEquipement.Arme, Rarete.Legendaire, 30, 6, 0, 80, 14, 0, 0, 550), 200, 5, "💀 Boss", 6),

                new Quete(11, "Fléau Primordial : Dragon Ignis", "Triomphez d'Ignis, Dragon Millénaire Suprême.", "Ignis, Dragon Millénaire Suprême", 1, 2200, 3000, 9,
                    new Equipement("Armure du Pourfendeur de Dragon", TypeEquipement.Armure, Rarete.Legendaire, 0, 26, 80, 0, 0, 0, 0, 750), 260, 6, "💀 Boss", 8),

                // RANG S : Boss Mythiques & Hardcore Endgame
                new Quete(12, "L'Abîme Brûlant : Belial le Démon", "Terrassez Belial, Seigneur Démoniaque des Flammes Noires.", "Belial, Seigneur Démoniaque des Flammes Noires", 1, 3000, 4000, 11,
                    new Equipement("Trident Infernal des Abîmes", TypeEquipement.Arme, Rarete.Legendaire, 36, 8, 30, 0, 16, 0, 5, 900), 320, 8, "🌌 Mythique", 9),

                new Quete(13, "La Tombe Impie : Mor'Gath la Liche", "Annihilez Mor'Gath, Liche Nécromancienne des Profondeurs.", "Mor'Gath, Liche Nécromancienne des Profondeurs", 1, 4000, 5200, 13,
                    new Equipement("Couronne Maudite des Rois Morts", TypeEquipement.Casque, Rarete.Legendaire, 0, 22, 60, 70, 0, 6, 8, 1100), 400, 10, "🌌 Mythique", 10),

                new Quete(14, "Le Colosse Tellurique : Valdorak", "Brisez Valdorak, Colosse de Granite Primordial.", "Valdorak, Colosse de Granite Primordial", 1, 5200, 6500, 16,
                    new Equipement("Plastron Inébranlable du Titan", TypeEquipement.Armure, Rarete.Legendaire, 0, 34, 120, 0, 0, 0, 0, 1400), 500, 12, "🌌 Mythique", 11),

                new Quete(15, "Le Venin Obscur : Azkalith", "Exterminez Azkalith, Reine Vipère des Abysses Oubliées.", "Azkalith, Reine Vipère des Abysses Oubliées", 1, 4800, 6000, 15,
                    new Equipement("Fouet d'Écailles Émeraude", TypeEquipement.Arme, Rarete.Legendaire, 34, 6, 0, 30, 18, 10, 6, 1250), 460, 11, "🌌 Mythique", 10),

                new Quete(16, "Fléau Absolu : Chevalier du Néant", "Terrassez l'Entité Suprême : Le Chevalier du Néant Primordial.", "Le Chevalier du Néant Primordial", 1, 12000, 15000, 25,
                    new Equipement("Lame Suprême du Néant Primordial", TypeEquipement.Arme, Rarete.Mythique, 50, 15, 60, 50, 25, 12, 12, 3000), 1000, 25, "👑 Hardcore Ultime", 12)
            };
        }

        // ==============================================================
        // SAUVEGARDE & CHARGEMENT
        // ==============================================================
        private void SauvegarderPartie()
        {
            try
            {
                string json = JsonSerializer.Serialize(hero, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FichierSauvegarde, json);
                AjouterLog($"\n💾 Partie sauvegardée avec succès dans '{FichierSauvegarde}' !", Color.FromArgb(46, 204, 113));
                MessageBox.Show("Votre aventure a été sauvegardée avec succès !", "Sauvegarde Réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ChargerPartie()
        {
            try
            {
                if (File.Exists(FichierSauvegarde))
                {
                    string json = File.ReadAllText(FichierSauvegarde);
                    hero = JsonSerializer.Deserialize<Joueur>(json)!;
                    InitialiserQuetes();
                    AjouterLog($"\n✅ Sauvegarde chargée : Gloire à vous, {hero.Nom} le {hero.Classe} (Niv. {hero.Niveau}) !", Color.FromArgb(46, 204, 113));
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
    }
}

