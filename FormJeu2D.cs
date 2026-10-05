using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using MessageBoxWinForms = System.Windows.Forms.MessageBox;

namespace JeuxRPG
{
    public partial class FormJeu2D : Form
    {
        private Joueur hero = null!;
        private Joueur2D joueur2D = null!;
        private Monde2D monde = null!;
        private Camera2D camera = null!;
        private GestionnaireEntrees entrees = new GestionnaireEntrees();

        private BoucleJeuPrecise boucleJeu = null!;
        private Stopwatch chronoFrame = new Stopwatch();

        private bool jeuEnPause = false;
        private bool afficherAideControles = false;
        private bool miseAJourEnCours;

        // Polices graphiques réutilisables
        private Font fontTitre = new Font("Segoe UI", 12f, FontStyle.Bold);
        private Font fontTexte = new Font("Segoe UI", 9f, FontStyle.Regular);
        private Font fontGras = new Font("Segoe UI", 9f, FontStyle.Bold);
        private Font fontPetit = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        private Font fontCrit = new Font("Segoe UI", 12f, FontStyle.Bold);
        private Font fontIconeAction = new Font("Segoe UI", 12f);
        private Font fontToucheAction = new Font("Segoe UI", 6.8f, FontStyle.Bold);
        private Font fontJaugeUltime = new Font("Segoe UI", 9f, FontStyle.Bold);
        private Font fontDpsMannequin = new Font("Segoe UI", 11f, FontStyle.Bold);
        private Font fontBadge = new Font("Segoe UI", 12f, FontStyle.Bold);

        // Animation & Juice HUD
        private float pvAffichage = 0f;
        private float pvGhost = 0f;
        private float tempsGhostPause = 0f;
        private float pulseCoeur = 0f;

        // Difficulté de combat active pour les Donjons et Boss
        private DifficulteBoss2D difficulteActive = DifficulteBoss2D.Normale;

        private const string FichierSauvegarde = "sauvegarde_aethelgard.json";
        private const string FichierConfigurationTouches = "commandes_2d.json";
        private const int NombreEmplacementsSauvegarde = 3;
        private const float IntervalleAutosauvegarde = 90f;
        private int emplacementSauvegardeActif = 1;
        private float tempsAutosauvegarde;

        private static string RepertoireSauvegardes => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Aethelgard", "Sauvegardes2D");

        private static string CheminEmplacementSauvegarde(int emplacement) =>
            Path.Combine(RepertoireSauvegardes, $"emplacement_{Math.Clamp(emplacement, 1, NombreEmplacementsSauvegarde)}.json");

        private static string CheminConfigurationTouches => Path.Combine(RepertoireSauvegardes, FichierConfigurationTouches);

        public FormJeu2D()
        {
            InitialiserFenetre();
            ChargerConfigurationTouches();
            ChargerOuCreerPersonnage();
            InitialiserMondeEtCamera();
            InitialiserBoucleJeu();
            SauvegarderPartie(false);
        }

        private void InitialiserFenetre()
        {
            Text = "⚔️ CHRONIQUES D'AETHELGARD 2D — Action RPG Jouable Clavier & Souris";
            Size = new Size(1366, 768);
            MinimumSize = new Size(1024, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(18, 20, 26);
            ForeColor = Color.White;
            DoubleBuffered = true;
            KeyPreview = true;

            // Événements d'entrées
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F11)
                {
                    BasculerPleinEcran();
                    return;
                }

                if (etatEcran == EtatEcranJeu.MenuTitre)
                {
                    if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W || e.KeyCode == Keys.Z)
                    {
                        boutonTitreSelectionne = (boutonTitreSelectionne + 3) % 4;
                        AudioSynthetiseur.SonClic();
                        Invalidate();
                        return;
                    }
                    if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                    {
                        boutonTitreSelectionne = (boutonTitreSelectionne + 1) % 4;
                        AudioSynthetiseur.SonClic();
                        Invalidate();
                        return;
                    }
                    if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                    {
                        ExecuterActionMenuTitre(boutonTitreSelectionne);
                        return;
                    }
                    return;
                }

                if (dialogueActif != null)
                {
                    if (e.KeyCode == Keys.Space || e.KeyCode == Keys.E || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                    {
                        FermerDialogueInGame();
                    }
                    return;
                }

                if (e.KeyCode == entrees.Touche("Pause"))
                {
                    jeuEnPause = !jeuEnPause;
                    Invalidate();
                    return;
                }
                if (e.KeyCode == Keys.H || e.KeyCode == Keys.F1)
                {
                    afficherAideControles = !afficherAideControles;
                    Invalidate();
                    return;
                }
                if (e.KeyCode == entrees.Touche("Sauvegarde"))
                {
                    if (SauvegarderPartie())
                        monde.AjouterTexteFlottant(joueur2D.Position, $"💾 Emplacement {emplacementSauvegardeActif} sauvegardé !", Color.FromArgb(46, 204, 113), true);
                    return;
                }
                if (e.KeyCode == Keys.F6)
                {
                    OuvrirGestionSauvegardes();
                    return;
                }
                if (e.KeyCode == Keys.F7)
                {
                    OuvrirConfigurationTouches();
                    return;
                }

                entrees.TraiterKeyDown(e.KeyCode);

                // Touches de raccourcis menus directs
                if (e.KeyCode == entrees.Touche("Sac") || e.KeyCode == Keys.Tab) { OuvrirSac(); return; }
                if (e.KeyCode == entrees.Touche("Quetes")) { OuvrirQuetes(); return; }
                if (e.KeyCode == entrees.Touche("Forge")) { OuvrirForge(); return; }
                if (e.KeyCode == entrees.Touche("Artisanat")) { OuvrirAtelierArtisanat(); return; }
                if (e.KeyCode == Keys.B || e.KeyCode == Keys.L) { OuvrirCodexDropsBoss(); return; }

                // Raccourcis directs d'objets consommables rapides (5, 6, 7, 8)
                if (e.KeyCode == Keys.D5 || e.KeyCode == Keys.NumPad5) { UtiliserElixirForce(); return; }
                if (e.KeyCode == Keys.D6 || e.KeyCode == Keys.NumPad6) { UtiliserPeauDePierre(); return; }
                if (e.KeyCode == Keys.D7 || e.KeyCode == Keys.NumPad7) { UtiliserBombeIncendiaire(); return; }
                if (e.KeyCode == Keys.D8 || e.KeyCode == Keys.NumPad8) { UtiliserParcheminTeleport(); return; }
            };

            KeyUp += (s, e) => entrees.TraiterKeyUp(e.KeyCode);

            MouseDown += (s, e) =>
            {
                if (etatEcran == EtatEcranJeu.MenuTitre)
                {
                    GererClicMenuTitre(e.Location);
                    return;
                }

                if (dialogueActif != null)
                {
                    FermerDialogueInGame();
                    return;
                }

                if (jeuEnPause)
                {
                    int w = 380;
                    int h = 430;
                    int x = (ClientSize.Width - w) / 2;
                    int y = (ClientSize.Height - h) / 2;

                    Rectangle btnReprendre = new Rectangle(x + 50, y + 75, 280, 36);
                    Rectangle btnSauvegarder = new Rectangle(x + 50, y + 120, 280, 36);
                    Rectangle btnInventaire = new Rectangle(x + 50, y + 165, 280, 36);
                    Rectangle btnCodex = new Rectangle(x + 50, y + 210, 280, 36);
                    Rectangle btnCraft = new Rectangle(x + 50, y + 255, 280, 36);
                    Rectangle btnCapitale = new Rectangle(x + 50, y + 300, 280, 36);
                    Rectangle btnQuitter = new Rectangle(x + 50, y + 345, 280, 36);

                    if (btnReprendre.Contains(e.Location))
                    {
                        jeuEnPause = false;
                        Invalidate();
                    }
                    else if (btnSauvegarder.Contains(e.Location))
                    {
                        OuvrirGestionSauvegardes();
                    }
                    else if (btnInventaire.Contains(e.Location))
                    {
                        OuvrirSac();
                    }
                    else if (btnCodex.Contains(e.Location))
                    {
                        OuvrirCodexDropsBoss();
                    }
                    else if (btnCraft.Contains(e.Location))
                    {
                        OuvrirAtelierArtisanat();
                    }
                    else if (btnCapitale.Contains(e.Location))
                    {
                        monde.ChargerVillage();
                        joueur2D.Position = monde.PositionDepartVillage;
                        jeuEnPause = false;
                        Invalidate();
                    }
                    else if (btnQuitter.Contains(e.Location))
                    {
                        Close();
                    }
                    return;
                }

                if (afficherAideControles)
                {
                    afficherAideControles = false;
                    Invalidate();
                    return;
                }

                entrees.TraiterMouseDown(e.Button);
            };
            MouseUp += (s, e) => entrees.TraiterMouseUp(e.Button);

            MouseMove += (s, e) =>
            {
                if (etatEcran == EtatEcranJeu.MenuTitre)
                {
                    GererSurvolMenuTitre(e.Location);
                    return;
                }

                if (camera != null)
                {
                    entrees.TraiterMouseMove(e.Location, camera);
                    monde.PositionSourisActuelle = entrees.PositionSourisMonde;
                }
            };

            Resize += (s, e) =>
            {
                if (camera != null)
                {
                    camera.LargeurEcran = ClientSize.Width;
                    camera.HauteurEcran = ClientSize.Height;
                }
            };
        }

        private void ChargerConfigurationTouches()
        {
            foreach (string chemin in new[] { CheminConfigurationTouches, CheminConfigurationTouches + ".bak" })
            {
                if (!File.Exists(chemin)) continue;
                try
                {
                    var sauvegarde = JsonSerializer.Deserialize<Dictionary<string, Keys>>(File.ReadAllText(chemin));
                    entrees.ChargerRaccourcis(sauvegarde);
                    return;
                }
                catch (Exception ex)
                {
                    JournalErreurs.Enregistrer(ex, "Chargement de la configuration des touches");
                    try { File.Copy(chemin, chemin + $".corrompu_{DateTime.Now:yyyyMMdd_HHmmss_fff}", false); }
                    catch (Exception copieErreur) { JournalErreurs.Enregistrer(copieErreur, "Copie de configuration de touches corrompue"); }
                }
            }
        }

        private void SauvegarderConfigurationTouches()
        {
            string chemin = CheminConfigurationTouches;
            string temporaire = chemin + ".tmp";
            try
            {
                Directory.CreateDirectory(RepertoireSauvegardes);
                File.WriteAllText(temporaire, JsonSerializer.Serialize(entrees.CopierRaccourcis(), new JsonSerializerOptions { WriteIndented = true }));
                if (File.Exists(chemin)) File.Copy(chemin, chemin + ".bak", true);
                File.Move(temporaire, chemin, true);
            }
            catch (Exception ex)
            {
                AfficherMessageAvecPause(this, $"Impossible d'enregistrer les raccourcis : {ex.Message}", "Commandes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                try { if (File.Exists(temporaire)) File.Delete(temporaire); }
                catch (Exception ex) { JournalErreurs.Enregistrer(ex, "Nettoyage du fichier temporaire des touches"); }
            }
        }

        private void ChargerOuCreerPersonnage()
        {
            emplacementSauvegardeActif = 1;
            Joueur? sauvegarde = ChargerDepuisFichierAvecRecuperation(CheminEmplacementSauvegarde(emplacementSauvegardeActif));

            if (sauvegarde == null)
            {
                string[] anciensChemins =
                {
                    Path.Combine(Environment.CurrentDirectory, FichierSauvegarde),
                    Path.Combine(AppContext.BaseDirectory, FichierSauvegarde)
                };

                foreach (string ancienChemin in anciensChemins.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    sauvegarde = ChargerDepuisFichierAvecRecuperation(ancienChemin);
                    if (sauvegarde != null) break;
                }
            }

            if (sauvegarde != null)
            {
                hero = sauvegarde;
                InitialiserQuetesSiNecessaire();
                if (!File.Exists(CheminEmplacementSauvegarde(emplacementSauvegardeActif)))
                    SauvegarderPartie(false);
                return;
            }

            hero = new Joueur("Aventurier", ClasseType.Paladin);
            InitialiserQuetesSiNecessaire();
        }

        private static Joueur? ChargerDepuisFichierAvecRecuperation(string chemin)
        {
            foreach (string candidat in new[] { chemin, chemin + ".bak" })
            {
                if (!File.Exists(candidat)) continue;

                try
                {
                    string json = File.ReadAllText(candidat);
                    if (string.IsNullOrWhiteSpace(json))
                        throw new InvalidOperationException("Sauvegarde vide.");

                    using JsonDocument document = JsonDocument.Parse(json);
                    if (!document.RootElement.TryGetProperty("Nom", out _))
                        throw new InvalidOperationException("Sauvegarde invalide.");

                    Joueur sauvegarde = JsonSerializer.Deserialize<Joueur>(json)
                        ?? throw new InvalidOperationException("Sauvegarde invalide.");
                    sauvegarde.TalentsNiveaux ??= new Dictionary<string, int>();
                    sauvegarde.QuetesActives ??= new List<Quete>();
                    sauvegarde.QuetesCompleteesIds ??= new List<int>();
                    sauvegarde.SacEquipements ??= new List<Equipement>();
                    sauvegarde.InventaireConsommables ??= new Dictionary<TypeConsommable, int>();
                    sauvegarde.AssurerSortsParDefaut();
                    return sauvegarde;
                }
                catch (Exception ex)
                {
                    JournalErreurs.Enregistrer(ex, $"Chargement/récupération de sauvegarde : {candidat}");
                    string copieCorrompue = candidat + $".corrompue_{DateTime.Now:yyyyMMdd_HHmmss_fff}";
                    try { File.Copy(candidat, copieCorrompue, false); }
                    catch (Exception copieErreur) { JournalErreurs.Enregistrer(copieErreur, $"Archivage d'une sauvegarde corrompue : {candidat}"); }
                }
            }

            return null;
        }

        private bool AfficherCreationPersonnageRapide()
        {
            using (Form dlg = new Form())
            {
                dlg.Text = "Création de votre Champion";
                dlg.Size = new Size(520, 360);
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.BackColor = Color.FromArgb(24, 26, 32);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                Label lbl = new Label
                {
                    Text = "🗡️ BIENVENUE DANS AETHELGARD 2D\nChoisissez le nom et la classe de votre héros :",
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(20, 15),
                    AutoSize = true
                };

                Label lblNom = new Label { Text = "Nom du Héros :", Location = new Point(20, 75), AutoSize = true };
                TextBox txtNom = new TextBox { Text = "Aethelgardien", Location = new Point(140, 72), Width = 320, BackColor = Color.FromArgb(35, 38, 48), ForeColor = Color.White };

                Label lblClasse = new Label { Text = "Classe :", Location = new Point(20, 120), AutoSize = true };
                ComboBox cbClasse = new ComboBox { Location = new Point(140, 117), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 38, 48), ForeColor = Color.White };
                cbClasse.Items.AddRange(new object[] {
                    "Guerrier (Mêlée lourde, Tourbillon, Grande résistance)",
                    "Mage (Projectiles magiques arcaniques, Nova de flammes)",
                    "Rodeur (Tir à l'arc ultra rapide, Volée de flèches)",
                    "Paladin (Masse sacrée, Soin divin, Bouclier sacré)",
                    "Necromancien (Faux spectrale, Vol de vie vampirique)"
                });
                cbClasse.SelectedIndex = 0;

                Button btnValider = new Button
                {
                    Text = "⚔️ COMMENCER L'AVENTURE 2D",
                    Location = new Point(140, 200),
                    Size = new Size(250, 48),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnValider.FlatAppearance.BorderSize = 0;
                btnValider.Click += (s, e) =>
                {
                    string nom = string.IsNullOrWhiteSpace(txtNom.Text) ? "Valere" : txtNom.Text.Trim();
                    ClasseType classe = (ClasseType)cbClasse.SelectedIndex;
                    hero = new Joueur(nom, classe);
                    InitialiserQuetesSiNecessaire();
                    dlg.DialogResult = DialogResult.OK;
                    dlg.Close();
                };

                dlg.Controls.AddRange(new Control[] { lbl, lblNom, txtNom, lblClasse, cbClasse, btnValider });
                return dlg.ShowDialog(this) == DialogResult.OK;
            }
        }

        private void InitialiserQuetesSiNecessaire()
        {
            if (hero.QuetesActives.Count == 0)
            {
                hero.QuetesActives.Add(new Quete
                {
                    Id = 1,
                    Titre = "Menace des Sangliers",
                    Description = "Éliminer 3 Sangliers Enragés dans les bois.",
                    CibleNom = "Sanglier Enragé",
                    Objectif = 3,
                    RecompenseOr = 80,
                    RecompenseXP = 120,
                    RecompenseReputation = 40
                });
                hero.QuetesActives.Add(new Quete
                {
                    Id = 2,
                    Titre = "Tueur de Chef Gobelin",
                    Description = "Terrasser le redoutable Boss Grok.",
                    CibleNom = "Grok le Brise-Crâne",
                    Objectif = 1,
                    RecompenseOr = 250,
                    RecompenseXP = 350,
                    RecompensePierresForge = 3,
                    RecompenseReputation = 80
                });
            }
        }

        private void InitialiserMondeEtCamera()
        {
            monde = new Monde2D();
            camera = new Camera2D(ClientSize.Width, ClientSize.Height);
            joueur2D = new Joueur2D(hero, monde.PositionDepartVillage);
            camera.Position = joueur2D.Position;
            camera.Cible = joueur2D.Position;
            pvAffichage = hero.PVActuels;
            pvGhost = hero.PVActuels;
        }

        private void InitialiserBoucleJeu()
        {
            chronoFrame.Start();
            boucleJeu = new BoucleJeuPrecise();
            boucleJeu.Interval = 16; // ~60 FPS
            boucleJeu.Tick += (s, e) =>
            {
                if (miseAJourEnCours || jeuEnPause) return;

                miseAJourEnCours = true;
                try
                {
                    float dt = (float)chronoFrame.Elapsed.TotalSeconds;
                    chronoFrame.Restart();
                    dt = Math.Min(dt, 0.05f);
                    MettreAJourJeu(dt);
                    Invalidate();
                }
                finally
                {
                    miseAJourEnCours = false;
                }
            };
            boucleJeu.Start();
        }

        private void MettreAJourJeu(float dt)
        {
            if (etatEcran == EtatEcranJeu.MenuTitre)
            {
                tempsAnimationTitre += dt;
                return;
            }

            if (jeuEnPause) return;

            MettreAJourBuffs(dt);
            if (tempsBuffForce > 0f && Random.Shared.Next(0, 4) == 0)
                monde.Particules.EmettreTrailProjectile(joueur2D.Position, Color.Gold, 12f);
            if (tempsBuffPeauDePierre > 0f && Random.Shared.Next(0, 4) == 0)
                monde.Particules.EmettreTrailProjectile(joueur2D.Position, Color.LightSteelBlue, 14f);

            tempsAutosauvegarde += dt;
            if (tempsAutosauvegarde >= IntervalleAutosauvegarde)
            {
                tempsAutosauvegarde = 0f;
                SauvegarderPartie();
                monde.AjouterTexteFlottant(joueur2D.Position, "💾 Sauvegarde automatique", Color.FromArgb(46, 204, 113), false);
            }

            // Interpolation fluide barre de vie & rattrapage Ghost
            if (pvAffichage <= 0f && hero.PVActuels > 0)
            {
                pvAffichage = hero.PVActuels;
                pvGhost = hero.PVActuels;
            }
            else
            {
                pvAffichage += (hero.PVActuels - pvAffichage) * MathF.Min(1f, dt * 14f);

                if (pvGhost > hero.PVActuels)
                {
                    if (tempsGhostPause > 0f)
                    {
                        tempsGhostPause -= dt;
                    }
                    else
                    {
                        pvGhost += (hero.PVActuels - pvGhost) * MathF.Min(1f, dt * 3.8f);
                    }
                }
                else
                {
                    pvGhost = hero.PVActuels;
                }
            }

            if (hero.PVActuels < pvGhost - 1f && tempsGhostPause <= 0f)
            {
                tempsGhostPause = 0.38f;
            }

            // Pulsation d'alerte critique si PV < 28%
            if (hero.PVActuels < hero.PVMaxTotal * 0.28f && hero.PVActuels > 0)
            {
                pulseCoeur += dt * 5.5f;
            }
            else
            {
                pulseCoeur = 0f;
            }

            // 1. Mise à jour du joueur (WASD, Visée, Dash)
            joueur2D.MettreAJour(dt, entrees, monde, monde.Particules, (txt, col, crit) => monde.AjouterTexteFlottant(joueur2D.Position, txt, col, crit));

            // 2. Actions de combat (Clic Gauche = Attaque Normale)
            if (entrees.ClicGauche)
            {
                joueur2D.AttaquePrimaire(monde, monde.Particules, camera, (txt, col, crit) => monde.AjouterTexteFlottant(joueur2D.Position, txt, col, crit));
            }

            // Compétence 1 (Sort de classe principal : Q, Clic Droit, 3)
            if (entrees.ToucheCompetence1)
            {
                joueur2D.AttaqueCompetence1(monde, monde.Particules, camera, (txt, col, crit) => monde.AjouterTexteFlottant(joueur2D.Position, txt, col, crit));
            }

            // Compétence 2 ou Interaction :
            // Si la touche E est pressée ET qu'un interactif (PNJ, coffre, portail) est à proximité -> Interagir
            // Sinon (E en combat/exploration, ou F, ou 4) -> Lancer Compétence 2 (Sort forgé ou sort secondaire)
            bool interactifProche = monde.YATilInteractifProche(joueur2D.Position, joueur2D.Rayon);
            if (interactifProche)
            {
                if (entrees.VientDEtrePressee(entrees.Touche("Interaction")))
                {
                    entrees.ReinitialiserTriggersFrame(camera);
                    ExecuterInteractionProche();
                }
                else if (entrees.VientDEtrePressee(entrees.Touche("Sort2")) || entrees.EstEnfoncee(entrees.Touche("Sort2")) || entrees.VientDEtrePressee(Keys.D4) || entrees.VientDEtrePressee(Keys.NumPad4))
                {
                    joueur2D.AttaqueCompetence2(monde, monde.Particules, camera, (txt, col, crit) => monde.AjouterTexteFlottant(joueur2D.Position, txt, col, crit));
                }
            }
            else
            {
                if (entrees.ToucheCompetence2)
                {
                    joueur2D.AttaqueCompetence2(monde, monde.Particules, camera, (txt, col, crit) => monde.AjouterTexteFlottant(joueur2D.Position, txt, col, crit));
                }
            }

            // Ultime (Touche R quand jauge = 100%)
            if (entrees.ToucheUltime)
            {
                joueur2D.DeclencherUltime(monde, monde.Particules, camera, (txt, col, crit) => monde.AjouterTexteFlottant(joueur2D.Position, txt, col, crit));
            }

            // 3. Raccourcis potions en direct (1 = Soin, 2 = Mana)
            if (entrees.TouchePotionSoin) BoirePotionSoin();
            if (entrees.TouchePotionMana) BoirePotionMana();

            // 5. Mise à jour du monde & IA monstres
            monde.MettreAJour(dt, joueur2D, camera, (txt, col, crit) => monde.AjouterTexteFlottant(joueur2D.Position, txt, col, crit));

            // 6. Mise à jour Caméra (suivi fluide)
            camera.Cible = joueur2D.Position;
            camera.MettreAJour(dt);

            // 7. Vérification défaite du joueur
            if (hero.PVActuels <= 0)
            {
                AudioSynthetiseur.SonCriBoss();
                camera.DeclencherSecousse(20f, 0.6f);
                monde.Particules.EmettreAnneauExplosion(joueur2D.Position, Color.Red, 45, 280f);

                (int perteOr, int perteXP) = hero.AppliquerPenaliteMort();

                hero.PVActuels = hero.PVMaxTotal;
                hero.ManaActuel = hero.ManaMaxTotal;
                monde.ChargerVillage();
                joueur2D.Position = monde.PositionDepartVillage;
                string penalite = $"Perte : {perteOr} Or et {perteXP} XP. Aucun niveau perdu.";
                monde.AjouterTexteFlottant(joueur2D.Position, penalite, Color.Orange, true);
                SauvegarderPartie(false);

                AfficherDialogueInGame("💀 DÉFAITE AU COMBAT",
                    $"Vous avez succombé au combat...\n\nVous battez en retraite à la fontaine sacrée de la Capitale d'Aethelgard.\nVos blessures ont été soignées par les prêtresses.\n\n{penalite}\n\nReprenez des forces et préparez votre revanche !",
                    "💀");
                return;
            }

            // Réinitialisation des triggers du frame
            entrees.ReinitialiserTriggersFrame(camera);
        }

        private void BoirePotionSoin()
        {
            int manque = Math.Max(0, hero.PVMaxTotal - hero.PVActuels);
            if (manque == 0)
            {
                monde.AjouterTexteFlottant(joueur2D.Position, "PV déjà au maximum", Color.LightGreen, false);
                return;
            }

            TypeConsommable potion = manque >= 150 && hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMajeure) > 0
                ? TypeConsommable.PotionSoinMajeure
                : hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure) > 0
                    ? TypeConsommable.PotionSoinMineure
                    : TypeConsommable.PotionSoinMajeure;
            int soin = potion == TypeConsommable.PotionSoinMajeure ? 150 : 60;
            if (hero.ObtenirQuantiteConsommable(potion) > 0)
            {
                hero.UtiliserConsommable(potion);
                hero.Soigner(soin);
                AudioSynthetiseur.SonLoot();
                monde.Particules.EmettreEclats(joueur2D.Position, Color.FromArgb(46, 204, 113), 16, 140f, 4f, 0.4f);
                monde.AjouterTexteFlottant(joueur2D.Position, $"+{Math.Min(soin, manque)} PV ({(potion == TypeConsommable.PotionSoinMajeure ? "majeure" : "mineure")})", Color.FromArgb(46, 204, 113), true);
            }
            else monde.AjouterTexteFlottant(joueur2D.Position, "Plus de potion de soin !", Color.Orange, false);
        }

        private void BoirePotionMana()
        {
            int manque = Math.Max(0, hero.ManaMaxTotal - hero.ManaActuel);
            if (manque == 0)
            {
                monde.AjouterTexteFlottant(joueur2D.Position, "Mana déjà au maximum", Color.LightSkyBlue, false);
                return;
            }

            TypeConsommable potion = manque >= 120 && hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMajeure) > 0
                ? TypeConsommable.PotionManaMajeure
                : hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMineure) > 0
                    ? TypeConsommable.PotionManaMineure
                    : TypeConsommable.PotionManaMajeure;
            int restauration = potion == TypeConsommable.PotionManaMajeure ? 120 : 50;
            if (hero.ObtenirQuantiteConsommable(potion) > 0)
            {
                hero.UtiliserConsommable(potion);
                hero.RestaurerMana(restauration);
                AudioSynthetiseur.SonTirMagique();
                monde.Particules.EmettreEclats(joueur2D.Position, Color.FromArgb(52, 152, 219), 16, 140f, 4f, 0.4f);
                monde.AjouterTexteFlottant(joueur2D.Position, $"+{Math.Min(restauration, manque)} Mana ({(potion == TypeConsommable.PotionManaMajeure ? "majeure" : "mineure")})", Color.FromArgb(52, 152, 219), true);
            }
            else monde.AjouterTexteFlottant(joueur2D.Position, "Plus de potion de mana !", Color.Orange, false);
        }

        private DialogResult AfficherMessageAvecPause(IWin32Window proprietaire, string texte, string titre, MessageBoxButtons boutons, MessageBoxIcon icone)
        {
            bool timerActif = boucleJeu != null && boucleJeu.Enabled;
            if (timerActif) boucleJeu!.Stop();
            entrees.ReinitialiserToutesTouches();
            chronoFrame.Restart();

            try
            {
                return MessageBoxWinForms.Show(proprietaire, texte, titre, boutons, icone);
            }
            finally
            {
                entrees.ReinitialiserToutesTouches();
                chronoFrame.Restart();
                if (timerActif && !jeuEnPause && !IsDisposed)
                    boucleJeu!.Start();
            }
        }

        private DialogResult AfficherMessageAvecPause(string texte, string titre, MessageBoxButtons boutons, MessageBoxIcon icone) =>
            AfficherMessageAvecPause(this, texte, titre, boutons, icone);

        private void OuvrirDialogueNPC(TypeInteractif type)
        {
            (string titre, string texte, string icone) = type switch
            {
                TypeInteractif.NPC_Brom => ("Brom le Forgeron", "\"Les armes lourdes font la différence lorsqu’on tient le front. Forge bien ton acier et la guerre te sourira.\"", "⚒️"),
                TypeInteractif.NPC_Artisan => ("Maître Kaëlith", "\"Les sorts sont des outils. Choisis celui qui transforme le chaos en avantage et ta main restera ferme.\"", "🔮"),
                TypeInteractif.NPC_Elenora => ("Maîtresse Elenora", "\"La Guilde archive tous les butins légendaires et secrets des monstres majeurs. Appuie sur [B] pour ouvrir le Codex des Butins & Drops et préparer tes expéditions !\"", "📜"),
                TypeInteractif.NPC_Marchand => ("Apothicaire Sylas", "\"La potion sauve le héros, mais la discipline le fait vivre. Garde la présence d’esprit, pas seulement les fioles.\"", "🧪"),
                TypeInteractif.NPC_Aubergiste => ("Mira l'Aubergiste", "\"Pose ton sac, aventurier. Un repas chaud remet les idées en place et le courage revient plus vite.\"", "🍺"),
                TypeInteractif.NPC_Capitaine => ("Capitaine Arven", $"\"Les routes ne sont sûres que si quelqu'un les défend ({hero.BossVaincusTotal} boss terrassés). Appuie sur [B] pour étudier le Codex des Boss, leurs faiblesses et leurs pièces de set légendaires !\"", "🛡️"),
                _ => ("", "", "💬")
            };

            if (!string.IsNullOrWhiteSpace(texte))
            {
                Action? suite = type switch
                {
                    TypeInteractif.NPC_Brom => OuvrirForge,
                    TypeInteractif.NPC_Artisan => OuvrirAtelierArtisanat,
                    TypeInteractif.NPC_Elenora => OuvrirQuetes,
                    TypeInteractif.NPC_Marchand => OuvrirBoutiquePotions,
                    TypeInteractif.NPC_Aubergiste => ProposerReposAuberge,
                    TypeInteractif.NPC_Capitaine => OuvrirContratsSecondaires,
                    _ => null
                };
                AfficherDialogueInGame(titre, texte, icone, suite);
            }
        }

        private void ExecuterInteractionProche()
        {
            foreach (var obj in monde.ObjetsInteractifs)
            {
                if (Vector2.Distance(joueur2D.Position, obj.Position) <= obj.Rayon + joueur2D.Rayon + 30f)
                {
                    switch (obj.Type)
                    {
                        case TypeInteractif.NPC_Brom:
                        case TypeInteractif.NPC_Artisan:
                        case TypeInteractif.NPC_Elenora:
                        case TypeInteractif.NPC_Marchand:
                        case TypeInteractif.NPC_Aubergiste:
                        case TypeInteractif.NPC_Capitaine:
                            OuvrirDialogueNPC(obj.Type);
                            break;
                        case TypeInteractif.PortailDonjon:
                            OuvrirMenuDonjonsEtBoss();
                            break;
                        case TypeInteractif.PortailTour:
                            OuvrirTourAstrale();
                            break;
                        case TypeInteractif.PortailProchainEtage:
                            PasserEtageSuivantTour();
                            break;
                        case TypeInteractif.PortailRetour:
                            monde.ChargerVillage();
                            joueur2D.Position = monde.PositionDepartVillage;
                            camera.Position = joueur2D.Position;
                            camera.Cible = joueur2D.Position;
                            monde.AjouterTexteFlottant(joueur2D.Position, "🏡 Retour à la Capitale", Color.Gold, true);
                            break;
                        case TypeInteractif.Coffre:
                            if (!obj.EstOuvert)
                            {
                                obj.EstOuvert = true;
                                hero.CoffresTresorOuverts++;
                                AudioSynthetiseur.SonCritique();
                                camera.DeclencherSecousse(8f, 0.3f);
                                int bonusZone = monde.TypeZoneActuelle switch
                                {
                                    ZoneType2D.Donjon => 30,
                                    ZoneType2D.TourAstrale => hero.EtageTourActuel * 8,
                                    _ => 0
                                };
                                int orGagne = Random.Shared.Next(50, 121) + bonusZone;
                                int pierresGagnees = Random.Shared.Next(1, 4);
                                hero.Or += orGagne;
                                hero.PierresDeForge += pierresGagnees;
                                string recompenseEquipement = "";
                                int chanceEquipement = monde.TypeZoneActuelle switch
                                {
                                    ZoneType2D.TourAstrale => 55,
                                    ZoneType2D.Donjon => 42,
                                    _ => 18
                                };
                                if (Random.Shared.Next(100) < chanceEquipement)
                                {
                                    Equipement equipement = monde.GenererLootCoffre(hero.Niveau, hero.EtageTourActuel);
                                    hero.SacEquipements.Add(equipement);
                                    recompenseEquipement = $"\n⚔️ {equipement.ObtenirDescription()}";
                                }
                                monde.Particules.EmettreAnneauExplosion(obj.Position, Color.Gold, 25, 200f);
                                monde.AjouterTexteFlottant(obj.Position, $"💎 Coffre Déverrouillé !\n+{orGagne} Or | +{pierresGagnees} Pierres{recompenseEquipement}", Color.Gold, true);
                            }
                            break;
                    }
                    return;
                }
            }
        }

        // ==============================================================
        // GESTION DU RENDU ÉCRAN (HUD, MINICARTE, OVERLAYS)
        // ==============================================================
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            CacheRenduGDI.CommencerImage();
            try
            {
                if (etatEcran == EtatEcranJeu.MenuTitre)
                {
                    DessinerMenuTitre(g);
                    return;
                }

                monde.Dessiner(g, camera, joueur2D, fontTexte, fontCrit);
                if (camera.AlphaFlash > 0)
                {
                    g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(camera.AlphaFlash, camera.CouleurFlash), ClientRectangle);
                }
                DessinerHUD(g);
                DessinerMiniCarte(g);

                if (hero.PVActuels < hero.PVMaxTotal * 0.28f && hero.PVActuels > 0)
                {
                    float intensite = (MathF.Sin(pulseCoeur) * 0.5f + 0.5f) * 65f + 30f;
                    int alpha = Math.Clamp((int)intensite, 0, 110);
                    using (Pen penVignette = new Pen(Color.FromArgb(alpha, 220, 20, 20), 16f))
                        g.DrawRectangle(penVignette, 8, 8, ClientSize.Width - 16, ClientSize.Height - 16);
                }

                if (afficherAideControles) DessinerOverlayAide(g);
                if (jeuEnPause && dialogueActif == null) DessinerMenuPause(g);
                if (dialogueActif != null) DessinerDialogueInGame(g);
            }
            finally
            {
                CacheRenduGDI.TerminerImage();
            }
        }

        private void DessinerHUD(Graphics g)
        {
            // === CARTE DU HÉROS (HAUT GAUCHE) ===
            int xHero = 20;
            int yHero = 20;
            int wHero = 320;
            int hHero = 138;

            DessinerPanneauVerre(g, new Rectangle(xHero, yHero, wHero, hHero), Color.FromArgb(142, 68, 173), 14);

            // Pastille Avatar / Classe
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(142, 68, 173)), xHero + 12, yHero + 12, 38, 38);
            g.DrawEllipse(Pens.Gold, xHero + 12, yHero + 12, 38, 38);
            g.DrawString("🛡️", fontBadge, Brushes.White, xHero + 16, yHero + 16);

            // Nom, Niveau & Classe
            g.DrawString($"{hero.Nom} (Niv. {hero.Niveau})", fontGras, Brushes.White, xHero + 58, yHero + 12);
            g.DrawString($"Classe : {hero.Classe}  •  Zone : {monde.NomZone}", fontPetit, Brushes.LightGray, xHero + 58, yHero + 30);
            g.DrawString($"Passif : {hero.ObtenirDescriptionPassiveClasse()}", fontPetit, Brushes.Cyan, xHero + 58, yHero + 44);

            // Barre de PV dual-layer avec barre Ghost rouge de rattrapage
            int bx = xHero + 58;
            int bw = 245;
            float maxPV = Math.Max(1f, hero.PVMaxTotal);
            float ratioGhost = Math.Clamp(pvGhost / maxPV, 0f, 1f);
            float ratioActuel = Math.Clamp(pvAffichage / maxPV, 0f, 1f);

            // Fond barre
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(50, 18, 18)), bx, yHero + 48, bw, 13);
            // Barre Ghost (rouge orangé qui rattrape en douceur)
            if (ratioGhost > ratioActuel)
            {
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(231, 76, 60)), bx, yHero + 48, bw * ratioGhost, 13);
            }
            // Barre active (vert éclatant)
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(46, 204, 113)), bx, yHero + 48, bw * ratioActuel, 13);
            // Ligne de brillance supérieure
            if (bw * ratioActuel > 2)
            {
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(90, 255, 255, 255)), bx, yHero + 48, bw * ratioActuel, 3);
            }
            g.DrawRectangle(Pens.Black, bx, yHero + 48, bw, 13);
            g.DrawString($"PV : {hero.PVActuels} / {hero.PVMaxTotal}", fontPetit, Brushes.White, bx + 4, yHero + 48);

            // Barre de Mana
            float ratioMana = Math.Clamp((float)hero.ManaActuel / hero.ManaMaxTotal, 0f, 1f);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(18, 25, 45)), bx, yHero + 65, bw, 11);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(52, 152, 219)), bx, yHero + 65, bw * ratioMana, 11);
            if (bw * ratioMana > 2)
            {
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(80, 255, 255, 255)), bx, yHero + 65, bw * ratioMana, 2);
            }
            g.DrawRectangle(Pens.Black, bx, yHero + 65, bw, 11);
            g.DrawString($"Mana : {hero.ManaActuel} / {hero.ManaMaxTotal}", fontPetit, Brushes.White, bx + 4, yHero + 64);

            // Ressources
            g.DrawString($"🪙 {hero.Or} Or   💎 {hero.PierresDeForge} Pierres   ⚔️ Atk {hero.AttaqueTotale}   🛡️ Def {hero.DefenseTotale}", fontPetit, Brushes.Gold, xHero + 12, yHero + 100);
            string resumeSets = hero.ObtenirResumeSets();
            if (!string.IsNullOrWhiteSpace(resumeSets))
                g.DrawString($"✨ {resumeSets}", fontPetit, Brushes.Cyan, xHero + 12, yHero + 114);

            // === BARRE D'ACTIONS (BAS CENTRE) ===
            int barLarg = 860;
            int barHaut = 64;
            int xBar = (ClientSize.Width - barLarg) / 2;
            int yBar = ClientSize.Height - barHaut - 18;

            DessinerPanneauVerre(g, new Rectangle(xBar, yBar, barLarg, barHaut), Color.FromArgb(52, 152, 219), 12);

            // Badges de buffs actifs au-dessus de la barre
            int xBuff = xBar + 10;
            if (tempsBuffForce > 0f)
            {
                string txtBuff = $"⚔️ FORCE +30% ({tempsBuffForce:F1}s)";
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(200, 230, 126, 34)), xBuff, yBar - 24, 155, 20);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Gold, 1.2f), xBuff, yBar - 24, 155, 20);
                g.DrawString(txtBuff, fontPetit, Brushes.White, xBuff + 6, yBar - 21);
                xBuff += 165;
            }
            if (tempsBuffPeauDePierre > 0f)
            {
                string txtBuff = $"🛡️ PEAU DE PIERRE +40% ({tempsBuffPeauDePierre:F1}s)";
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(200, 52, 73, 94)), xBuff, yBar - 24, 185, 20);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.LightSteelBlue, 1.2f), xBuff, yBar - 24, 185, 20);
                g.DrawString(txtBuff, fontPetit, Brushes.White, xBuff + 6, yBar - 21);
                xBuff += 195;
            }
            if (joueur2D.TempsBrulure > 0f)
            {
                string txtStatut = $"🔥 BRÛLURE ({joueur2D.TempsBrulure:F1}s)";
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(210, 192, 57, 43)), xBuff, yBar - 24, 135, 20);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.OrangeRed, 1.2f), xBuff, yBar - 24, 135, 20);
                g.DrawString(txtStatut, fontPetit, Brushes.White, xBuff + 6, yBar - 21);
                xBuff += 145;
            }
            if (joueur2D.TempsPoison > 0f)
            {
                string txtStatut = $"🧪 POISON ({joueur2D.TempsPoison:F1}s)";
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(210, 39, 174, 96)), xBuff, yBar - 24, 130, 20);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.LimeGreen, 1.2f), xBuff, yBar - 24, 130, 20);
                g.DrawString(txtStatut, fontPetit, Brushes.White, xBuff + 6, yBar - 21);
                xBuff += 140;
            }
            if (joueur2D.TempsGel > 0f)
            {
                string txtStatut = $"❄️ GEL -40% ({joueur2D.TempsGel:F1}s)";
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(210, 41, 128, 185)), xBuff, yBar - 24, 130, 20);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Cyan, 1.2f), xBuff, yBar - 24, 130, 20);
                g.DrawString(txtStatut, fontPetit, Brushes.White, xBuff + 6, yBar - 21);
                xBuff += 140;
            }
            if (joueur2D.TempsEtourdi > 0f)
            {
                string txtStatut = $"💫 ÉTOURDI ({joueur2D.TempsEtourdi:F1}s)";
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(220, 241, 196, 15)), xBuff, yBar - 24, 130, 20);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Gold, 1.2f), xBuff, yBar - 24, 130, 20);
                g.DrawString(txtStatut, fontPetit, Brushes.Black, xBuff + 6, yBar - 21);
                xBuff += 140;
            }

            // Boutons de sorts / actions
            int curX = xBar + 8;
            DessinerCaseAction(g, curX, yBar + 8, "⚔️", "CLIC G", "Attaque", joueur2D.CooldownAttaque <= 0f); curX += 58;
            DessinerCaseAction(g, curX, yBar + 8, "🌪️", entrees.Touche("Sort1").ToString(), "Sort 1", joueur2D.CooldownCompetence1 <= 0f); curX += 58;
            string iconeSort2 = hero.CompetenceEquipee?.Icone ?? "⚡";
            string nomSort2 = hero.CompetenceEquipee != null ? (hero.CompetenceEquipee.Nom.Length > 8 ? hero.CompetenceEquipee.Nom.Substring(0, 8) : hero.CompetenceEquipee.Nom) : "Sort 2";
            DessinerCaseAction(g, curX, yBar + 8, iconeSort2, entrees.Touche("Sort2").ToString(), nomSort2, joueur2D.CooldownCompetence2 <= 0f); curX += 58;
            DessinerCaseAction(g, curX, yBar + 8, "💨", entrees.Touche("Esquive").ToString(), "Dash", joueur2D.DashCooldown <= 0f || joueur2D.EstEnDash); curX += 58;

            int qteSoin = hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure) + hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMajeure);
            int qteMana = hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMineure) + hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMajeure);
            DessinerCaseAction(g, curX, yBar + 8, "🧪", entrees.Touche("PotionSoin").ToString(), $"Soin ({qteSoin})", qteSoin > 0); curX += 58;
            DessinerCaseAction(g, curX, yBar + 8, "💧", entrees.Touche("PotionMana").ToString(), $"Mana ({qteMana})", qteMana > 0); curX += 58;

            // Nouveaux objets consommables rapides
            int qteForce = hero.ObtenirQuantiteConsommable(TypeConsommable.ElixirForce);
            int qtePierre = hero.ObtenirQuantiteConsommable(TypeConsommable.PeauDePierre);
            int qteBombe = hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire);
            int qteTP = hero.ObtenirQuantiteConsommable(TypeConsommable.ParcheminTeleport);
            DessinerCaseAction(g, curX, yBar + 8, "🍷", "5", $"Force ({qteForce})", qteForce > 0); curX += 58;
            DessinerCaseAction(g, curX, yBar + 8, "🪨", "6", $"Pierre ({qtePierre})", qtePierre > 0); curX += 58;
            DessinerCaseAction(g, curX, yBar + 8, "💣", "7", $"Bombe ({qteBombe})", qteBombe > 0); curX += 58;
            DessinerCaseAction(g, curX, yBar + 8, "🌀", "8", $"TP ({qteTP})", qteTP > 0); curX += 58;

            DessinerCaseAction(g, curX, yBar + 8, "🎒", entrees.Touche("Sac").ToString(), "Sac", true); curX += 58;
            DessinerCaseAction(g, curX, yBar + 8, "📜", entrees.Touche("Quetes").ToString(), "Quêtes", true); curX += 58;
            DessinerCaseAction(g, curX, yBar + 8, "⚒️", entrees.Touche("Artisanat").ToString(), "Craft", true); curX += 58;

            if (joueur2D.ComboTempsReset > 0f && joueur2D.ComboIndex > 0)
            {
                string texteCombo = joueur2D.ComboIndex >= 3 ? "COMBO MAX !" : $"COMBO x{joueur2D.ComboIndex}";
                Color couleurCombo = joueur2D.ComboIndex >= 3 ? Color.Gold : Color.White;
                g.DrawString(texteCombo, fontGras, Brushes.Black, xBar + 84, yBar - 20);
                g.DrawString(texteCombo, fontGras, CacheRenduGDI.ObtenirBrush(couleurCombo), xBar + 82, yBar - 22);
            }

            if (joueur2D.EstEnDash)
                g.DrawString("ESQUIVE • INVULNÉRABLE", fontPetit, Brushes.Cyan, xBar + 266, yBar - 18);
            else if (joueur2D.DashCooldown > 0f)
                g.DrawString($"Esquive dans {joueur2D.DashCooldown:0.0}s", fontPetit, Brushes.LightSkyBlue, xBar + 266, yBar - 18);

            // Jauge Ultime circulaire
            int xUlt = xBar + barLarg - 64;
            int yUlt = yBar + 8;
            g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(40, 20, 20)), xUlt, yUlt, 48, 48);
            float angleSweep = (joueur2D.JaugeUltime / 100f) * 360f;
            Brush bUlt = CacheRenduGDI.ObtenirBrush(joueur2D.JaugeUltime >= 100f ? Color.Gold : Color.OrangeRed);
            g.FillPie(bUlt, xUlt, yUlt, 48, 48, -90, angleSweep);
            g.DrawEllipse(Pens.Gold, xUlt, yUlt, 48, 48);

            string txtR = joueur2D.JaugeUltime >= 100f ? "R !" : $"{joueur2D.JaugeUltime:F0}%";
            SizeF sz = g.MeasureString(txtR, fontJaugeUltime);
            g.DrawString(txtR, fontJaugeUltime, Brushes.White, xUlt + 24 - sz.Width / 2f, yUlt + 24 - sz.Height / 2f);

            // === BARRE DE BOSS EN HAUT SI BOSS VIVANT DANS LA ZONE ===
            var bossActuel = monde.Monstres.FirstOrDefault(m => m.EstBoss && !m.EstMort);
            if (bossActuel != null)
            {
                int bLarg = 540;
                int bHaut = 46;
                int bxBoss = (ClientSize.Width - bLarg) / 2;
                int byBoss = 25;

                DessinerPanneauVerre(g, new Rectangle(bxBoss, byBoss, bLarg, bHaut), Color.FromArgb(231, 76, 60), 10);

                // Nom du boss
                string titreBoss = $"👑 {bossActuel.ModeleMonstre.Nom} {(bossActuel.EstEnrage ? "🔥 ENRAGÉ !" : "")}";
                g.DrawString(titreBoss, fontGras, Brushes.Gold, bxBoss + 12, byBoss + 5);

                // Jauge PV Boss
                float ratioBoss = Math.Clamp((float)bossActuel.ModeleMonstre.PVActuels / bossActuel.ModeleMonstre.PVMax, 0f, 1f);
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(80, 20, 20)), bxBoss + 12, byBoss + 24, bLarg - 24, 14);
                g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(231, 76, 60)), bxBoss + 12, byBoss + 24, (bLarg - 24) * ratioBoss, 14);
                if ((bLarg - 24) * ratioBoss > 2)
                {
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(90, 255, 255, 255)), bxBoss + 12, byBoss + 24, (bLarg - 24) * ratioBoss, 3);
                }
                g.DrawRectangle(Pens.Black, bxBoss + 12, byBoss + 24, bLarg - 24, 14);
                g.DrawString($"{bossActuel.ModeleMonstre.PVActuels} / {bossActuel.ModeleMonstre.PVMax}", fontPetit, Brushes.White, bxBoss + 20, byBoss + 24);
            }

            // DPS Mannequin au centre si frappé
            if (monde.ChronoMannequin > 0f)
            {
                string txtDps = $"🎯 MANNEQUIN : {monde.DegatsMannequinCumules} DÉGÂTS INFLIGÉS !";
                SizeF szD = g.MeasureString(txtDps, fontDpsMannequin);
                g.DrawString(txtDps, fontDpsMannequin, Brushes.Black, (ClientSize.Width - szD.Width) / 2f + 1, 95 + 1);
                g.DrawString(txtDps, fontDpsMannequin, Brushes.Gold, (ClientSize.Width - szD.Width) / 2f, 95);
            }
        }

        private void DessinerCaseAction(Graphics g, int x, int y, string icone, string touche, string nom, bool pret)
        {
            int taille = 48;
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(pret ? Color.FromArgb(45, 49, 62) : Color.FromArgb(27, 28, 35)), x, y, taille, taille);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(pret ? Color.FromArgb(115, 130, 150) : Color.FromArgb(60, 62, 72), 1.5f), x, y, taille, taille);
            if (pret)
                g.FillRectangle(CacheRenduGDI.ObtenirBrushAlpha(150, Color.FromArgb(241, 196, 15)), x + 8, y + taille - 4, taille - 16, 2);

            SizeF szI = g.MeasureString(icone, fontIconeAction);
            g.DrawString(icone, fontIconeAction, Brushes.White, x + (taille - szI.Width) / 2f, y + 4);

            SizeF szT = g.MeasureString(touche, fontToucheAction);
            g.DrawString(touche, fontToucheAction, Brushes.Gold, x + (taille - szT.Width) / 2f, y + 31);
        }

        private void DessinerMiniCarte(Graphics g)
        {
            int mapW = 210;
            int mapH = 158;
            int mapX = ClientSize.Width - mapW - 20;
            int mapY = 20;
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(225, 15, 19, 27)), mapX, mapY, mapW, mapH);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(125, 52, 152, 219), 1.5f), mapX, mapY, mapW, mapH);
            g.DrawString(monde.NomZone, fontPetit, Brushes.White, mapX + 10, mapY + 8);

            Rectangle carte = new Rectangle(mapX + 9, mapY + 27, mapW - 18, mapH - 38);
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(25, 39, 36)), carte);
                float rx = carte.Width / monde.LargeurMonde;
                float ry = carte.Height / monde.HauteurMonde;

                if (monde.TypeZoneActuelle == ZoneType2D.Village)
                {
                    float mx0 = Monde2D.EchelleCoordonneeCapitale(935);
                    float mx1 = Monde2D.EchelleCoordonneeCapitale(1065);
                    float my0 = Monde2D.EchelleCoordonneeCapitale(170);
                    float my1 = Monde2D.EchelleCoordonneeCapitale(1350);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(90, 115, 121, 123)), carte.X + mx0 * rx, carte.Y + Monde2D.EchelleCoordonneeCapitale(700) * ry, (mx1 - mx0) * rx, (Monde2D.EchelleCoordonneeCapitale(840) - Monde2D.EchelleCoordonneeCapitale(700)) * ry);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(85, 145, 145, 133)), carte.X + mx0 * rx, carte.Y + my0 * ry, (mx1 - mx0) * rx, (my1 - my0) * ry);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(80, 125, 126, 128)), carte.X + Monde2D.EchelleCoordonneeCapitale(380) * rx, carte.Y + Monde2D.EchelleCoordonneeCapitale(420) * ry, (Monde2D.EchelleCoordonneeCapitale(1680) - Monde2D.EchelleCoordonneeCapitale(380)) * rx, (Monde2D.EchelleCoordonneeCapitale(520) - Monde2D.EchelleCoordonneeCapitale(420)) * ry);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(80, 125, 126, 128)), carte.X + Monde2D.EchelleCoordonneeCapitale(1060) * rx, carte.Y + Monde2D.EchelleCoordonneeCapitale(720) * ry, (Monde2D.EchelleCoordonneeCapitale(1680) - Monde2D.EchelleCoordonneeCapitale(1060)) * rx, (Monde2D.EchelleCoordonneeCapitale(830) - Monde2D.EchelleCoordonneeCapitale(720)) * ry);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(80, 125, 126, 128)), carte.X + Monde2D.EchelleCoordonneeCapitale(380) * rx, carte.Y + Monde2D.EchelleCoordonneeCapitale(1220) * ry, (Monde2D.EchelleCoordonneeCapitale(940) - Monde2D.EchelleCoordonneeCapitale(380)) * rx, (Monde2D.EchelleCoordonneeCapitale(1320) - Monde2D.EchelleCoordonneeCapitale(1220)) * ry);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(95, 115, 87, 61)), carte.X + Monde2D.EchelleCoordonneeCapitale(380) * rx, carte.Y + Monde2D.EchelleCoordonneeCapitale(260) * ry, (Monde2D.EchelleCoordonneeCapitale(640) - Monde2D.EchelleCoordonneeCapitale(380)) * rx, (Monde2D.EchelleCoordonneeCapitale(420) - Monde2D.EchelleCoordonneeCapitale(260)) * ry);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(95, 91, 78, 111)), carte.X + Monde2D.EchelleCoordonneeCapitale(1330) * rx, carte.Y + Monde2D.EchelleCoordonneeCapitale(260) * ry, (Monde2D.EchelleCoordonneeCapitale(1610) - Monde2D.EchelleCoordonneeCapitale(1330)) * rx, (Monde2D.EchelleCoordonneeCapitale(420) - Monde2D.EchelleCoordonneeCapitale(260)) * ry);
                    g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(95, 66, 102, 69)), carte.X + Monde2D.EchelleCoordonneeCapitale(380) * rx, carte.Y + Monde2D.EchelleCoordonneeCapitale(1080) * ry, (Monde2D.EchelleCoordonneeCapitale(620) - Monde2D.EchelleCoordonneeCapitale(380)) * rx, (Monde2D.EchelleCoordonneeCapitale(1230) - Monde2D.EchelleCoordonneeCapitale(1080)) * ry);
                    float fontaineX = Monde2D.EchelleCoordonneeCapitale(960);
                    float fontaineY = Monde2D.EchelleCoordonneeCapitale(730);
                    float fontaineFin = Monde2D.EchelleCoordonneeCapitale(1040);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(Color.FromArgb(125, 75, 175, 205)), carte.X + fontaineX * rx, carte.Y + fontaineY * ry, (fontaineFin - fontaineX) * rx, (fontaineFin - fontaineX) * ry);
                }
                else
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        float x = carte.X + carte.Width * i / 4f;
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(35, 190, 205, 220), 1f), x, carte.Y, x, carte.Bottom);
                    }
                    for (int i = 1; i <= 2; i++)
                    {
                        float y = carte.Y + carte.Height * i / 3f;
                        g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(35, 190, 205, 220), 1f), carte.X, y, carte.Right, y);
                    }
                }

                RectangleF vue = camera.ObtenirZoneVisible(0f);
                RectangleF vueMini = new RectangleF(carte.X + vue.X * rx, carte.Y + vue.Y * ry, vue.Width * rx, vue.Height * ry);
                g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(130, 230, 240, 255), 1f), vueMini.X, vueMini.Y, vueMini.Width, vueMini.Height);

                foreach (var m in monde.Monstres)
                {
                    if (m.EstMort) continue;
                    int mx = (int)(carte.X + m.Position.X * rx);
                    int my = (int)(carte.Y + m.Position.Y * ry);
                    Color couleur = m.EstBoss ? Color.OrangeRed : Color.FromArgb(225, 75, 78);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(100, couleur), mx - 4, my - 4, 8, 8);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(couleur), mx - (m.EstBoss ? 3 : 2), my - (m.EstBoss ? 3 : 2), m.EstBoss ? 6 : 4, m.EstBoss ? 6 : 4);
                }

                foreach (var obj in monde.ObjetsInteractifs)
                {
                    int ox = (int)(carte.X + obj.Position.X * rx);
                    int oy = (int)(carte.Y + obj.Position.Y * ry);
                    Color couleur = obj.Type == TypeInteractif.Coffre ? Color.Gold
                        : obj.Type == TypeInteractif.PortailDonjon || obj.Type == TypeInteractif.PortailTour || obj.Type == TypeInteractif.PortailProchainEtage ? Color.Cyan
                        : Color.FromArgb(255, 207, 100);
                    g.FillEllipse(CacheRenduGDI.ObtenirBrush(couleur), ox - 2, oy - 2, 4, 4);
                }

                Quete? queteSuivie = hero.QuetesActives.FirstOrDefault(q => !q.RecompenseReclamee);
                if (queteSuivie != null)
                {
                    Vector2? positionObjectif = null;
                    if (queteSuivie.EstTerminee)
                    {
                        var pnjQuetes = monde.ObjetsInteractifs.FirstOrDefault(obj => obj.Type == TypeInteractif.NPC_Elenora);
                        if (pnjQuetes != null) positionObjectif = pnjQuetes.Position;
                    }
                    else
                    {
                        var cible = monde.Monstres
                            .Where(m => !m.EstMort && m.ModeleMonstre.Nom.Contains(queteSuivie.CibleNom, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(m => Vector2.DistanceCarree(joueur2D.Position, m.Position))
                            .FirstOrDefault();
                        if (cible != null) positionObjectif = cible.Position;
                    }

                    if (!positionObjectif.HasValue)
                    {
                        TypeInteractif typeSortie = monde.TypeZoneActuelle == ZoneType2D.Village
                            ? TypeInteractif.PortailDonjon
                            : TypeInteractif.PortailRetour;
                        var portail = monde.ObjetsInteractifs.FirstOrDefault(obj => obj.Type == typeSortie);
                        if (portail != null) positionObjectif = portail.Position;
                    }

                    if (positionObjectif.HasValue)
                    {
                        int qx = (int)(carte.X + positionObjectif.Value.X * rx);
                        int qy = (int)(carte.Y + positionObjectif.Value.Y * ry);
                        int tailleRepere = 4 + (int)((MathF.Sin(monde.TempsTotal * 5f) + 1f) * 1.5f);
                        g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(85, Color.Gold), qx - tailleRepere * 2, qy - tailleRepere * 2, tailleRepere * 4, tailleRepere * 4);
                        Pen repereQuete = CacheRenduGDI.ObtenirPen(Color.Gold, 1.8f);
                        g.DrawLine(repereQuete, qx, qy - tailleRepere, qx + tailleRepere, qy);
                        g.DrawLine(repereQuete, qx + tailleRepere, qy, qx, qy + tailleRepere);
                        g.DrawLine(repereQuete, qx, qy + tailleRepere, qx - tailleRepere, qy);
                        g.DrawLine(repereQuete, qx - tailleRepere, qy, qx, qy - tailleRepere);
                    }
                }

                int px = (int)(carte.X + joueur2D.Position.X * rx);
                int py = (int)(carte.Y + joueur2D.Position.Y * ry);
                float dx = MathF.Cos(joueur2D.AngleVise);
                float dy = MathF.Sin(joueur2D.AngleVise);
                g.FillEllipse(CacheRenduGDI.ObtenirBrushAlpha(100, Color.LimeGreen), px - 6, py - 6, 12, 12);
                float baseX = px - dx * 4f;
                float baseY = py - dy * 4f;
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(255, 166, 65), 2f), px + dx * 6f, py + dy * 6f, baseX - dy * 3f, baseY + dx * 3f);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(255, 166, 65), 2f), baseX - dy * 3f, baseY + dx * 3f, baseX + dy * 3f, baseY - dx * 3f);
                g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(255, 166, 65), 2f), baseX + dy * 3f, baseY - dx * 3f, px + dx * 6f, py + dy * 6f);

            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(105, 160, 190, 200), 1f), carte);

            g.DrawString("H • AIDE", fontPetit, Brushes.LightGray, mapX + 10, mapY + mapH - 14);
        }

        private static void DessinerPanneauVerre(Graphics g, Rectangle rectangle, Color accent, int rayon)
        {
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(220, 20, 23, 31)), rectangle);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(125, accent), 1.5f), rectangle);
            g.DrawLine(CacheRenduGDI.ObtenirPen(Color.FromArgb(155, accent), 2f), rectangle.X + rayon, rectangle.Y + 1, rectangle.Right - rayon, rectangle.Y + 1);
        }

        private void DessinerOverlayAide(Graphics g)
        {
            int w = 580;
            int h = 370;
            int x = (ClientSize.Width - w) / 2;
            int y = (ClientSize.Height - h) / 2;

            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(240, 18, 20, 26)), x, y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.Gold, 2.5f), x, y, w, h);

            int ly = y + 20;
            g.DrawString("📜 GUIDE DES COMMANDES & TOUCHES DE JEU", fontTitre, Brushes.Gold, x + 25, ly);
            ly += 35;
            g.DrawString($"• [{entrees.Touche("Haut")} {entrees.Touche("Gauche")} {entrees.Touche("Bas")} {entrees.Touche("Droite")}] : Déplacements", fontGras, Brushes.White, x + 25, ly); ly += 24;
            g.DrawString("• [Souris] : Viser avec votre arme et orienter vos sorts", fontGras, Brushes.White, x + 25, ly); ly += 24;
            g.DrawString("• [Clic Gauche] : Attaque Normale / Combo de base", fontGras, Brushes.White, x + 25, ly); ly += 24;
            g.DrawString($"• [{entrees.Touche("Sort1")} / Clic droit] : Sort 1", fontGras, Brushes.Gold, x + 25, ly); ly += 24;
            g.DrawString($"• [{entrees.Touche("Sort2")} / 4] : Sort 2", fontGras, Brushes.Cyan, x + 25, ly); ly += 24;
            g.DrawString($"• [{entrees.Touche("Interaction")} près d'un PNJ] : Interagir", fontGras, Brushes.Yellow, x + 25, ly); ly += 24;
            g.DrawString($"• [{entrees.Touche("Ultime")}] : Ultime Dévastateur (jauge à 100%)", fontGras, Brushes.Orange, x + 25, ly); ly += 24;
            g.DrawString($"• [{entrees.Touche("Esquive")}] : Esquive / Dash", fontGras, Brushes.DeepSkyBlue, x + 25, ly); ly += 24;
            g.DrawString($"• [{entrees.Touche("PotionSoin")}] / [{entrees.Touche("PotionMana")}] : Potions", fontGras, Brushes.LightGreen, x + 25, ly); ly += 24;
            g.DrawString($"• [{entrees.Touche("Sac")}] Sac  |  [{entrees.Touche("Quetes")}] Quêtes  |  [{entrees.Touche("Forge")}] Forge  |  [B] Drops Boss", fontGras, Brushes.Magenta, x + 25, ly); ly += 24;
            g.DrawString($"• [{entrees.Touche("Sauvegarde")}] Sauvegarde rapide  |  [F6] Emplacements  |  [{entrees.Touche("Pause")}] Pause", fontGras, Brushes.LightGray, x + 25, ly); ly += 28;
            g.DrawString("Appuyez sur [H] ou [F1] pour fermer ce guide.", fontPetit, Brushes.Gray, x + 25, ly);
        }

        private void DessinerMenuPause(Graphics g)
        {
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(160, 0, 0, 0)), 0, 0, ClientSize.Width, ClientSize.Height);

            int w = 380;
            int h = 430;
            int x = (ClientSize.Width - w) / 2;
            int y = (ClientSize.Height - h) / 2;

            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(235, 22, 24, 32)), x, y, w, h);
            g.DrawRectangle(CacheRenduGDI.ObtenirPen(Color.FromArgb(142, 68, 173), 3f), x, y, w, h);

            g.DrawString("⏸️ PARTIE EN PAUSE", fontTitre, Brushes.Gold, x + 90, y + 25);

            int by = y + 75;
            DessinerBoutonPause(g, x + 50, by, $"▶ Reprendre la Partie ({entrees.Touche("Pause")})"); by += 45;
            DessinerBoutonPause(g, x + 50, by, "💾 Gérer les sauvegardes (F6)"); by += 45;
            DessinerBoutonPause(g, x + 50, by, "🎒 Ouvrir l'Inventaire & Stuff (I)"); by += 45;
            DessinerBoutonPause(g, x + 50, by, "📖 Codex des Drops & Boss (B)"); by += 45;
            DessinerBoutonPause(g, x + 50, by, "⚒️ Atelier d'Artisanat & Sorts (K)"); by += 45;
            DessinerBoutonPause(g, x + 50, by, "🏡 Retourner à la Capitale"); by += 45;
            DessinerBoutonPause(g, x + 50, by, "🚪 Quitter le Jeu");
        }

        private void DessinerBoutonPause(Graphics g, int x, int y, string texte)
        {
            g.FillRectangle(CacheRenduGDI.ObtenirBrush(Color.FromArgb(40, 44, 56)), x, y, 280, 36);
            g.DrawRectangle(Pens.Gray, x, y, 280, 36);
            g.DrawString(texte, fontGras, Brushes.White, x + 16, y + 8);
        }

        // ==============================================================
        // MENUS & DIALOGUES INTÉGRÉS (SAC AVEC BEST GEAR, FORGE, GUILDE)
        // ==============================================================
        private void OuvrirSac()
        {
            boucleJeu.Stop();
            hero.AssurerSortsParDefaut();

            using (Form dlg = new Form())
            {
                dlg.Text = "🎒 Sac de Voyage, Équipements & Grimoire du Champion";
                dlg.Size = new Size(840, 660);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(20, 22, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                // Titre
                Label lblTitre = new Label
                {
                    Text = "🎒 INVENTAIRE, ÉQUIPEMENTS & SORTS DU CHAMPION",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(155, 89, 182),
                    Location = new Point(20, 10),
                    AutoSize = true
                };

                // Statistiques globales du héros
                Label lblStatsHero = new Label
                {
                    Text = $"⚔️ Attaque : {hero.AttaqueTotale}   •   🎯 Critique : {hero.ChanceCritiqueTotale}%   •   🩸 Vol de Vie : {hero.VampirismeTotal}%   •   🛡️ Défense : {hero.DefenseTotale}   •   💧 Mana : {hero.ManaActuel}/{hero.ManaMaxTotal}",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(20, 36),
                    AutoSize = true
                };

                // Barre d'onglets de navigation
                Panel pnlOnglets = new Panel
                {
                    Location = new Point(20, 62),
                    Size = new Size(784, 38)
                };

                Button btnOngletEquip = new Button
                {
                    Text = $"🎒 Équipements & Sac ({hero.SacEquipements.Count})",
                    Location = new Point(0, 0),
                    Size = new Size(210, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.1f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOngletEquip.FlatAppearance.BorderSize = 0;

                Button btnOngletSorts = new Button
                {
                    Text = $"🔮 Sorts & Grimoire",
                    Location = new Point(215, 0),
                    Size = new Size(190, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(32, 36, 48),
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9.1f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOngletSorts.FlatAppearance.BorderSize = 0;

                Button btnOngletPotions = new Button
                {
                    Text = "🧪 Potions",
                    Location = new Point(410, 0),
                    Size = new Size(170, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(32, 36, 48),
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9.1f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOngletPotions.FlatAppearance.BorderSize = 0;

                Button btnOngletTalents = new Button
                {
                    Text = $"🌟 Talents ({hero.PointsTalents})",
                    Location = new Point(585, 0),
                    Size = new Size(199, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(32, 36, 48),
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9.1f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOngletTalents.FlatAppearance.BorderSize = 0;

                pnlOnglets.Controls.AddRange(new Control[] { btnOngletEquip, btnOngletSorts, btnOngletPotions, btnOngletTalents });

                // ==============================================================
                // 1. PANNEAU ÉQUIPEMENTS & TRI PAR RARETÉ
                // ==============================================================
                Panel pnlVueEquip = new Panel
                {
                    Location = new Point(20, 106),
                    Size = new Size(784, 465),
                    Visible = true
                };

                // Barre d'outils de tri par rareté
                Panel pnlBarreTri = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(784, 68)
                };

                Label lblTriTitre = new Label
                {
                    Text = "Trier :",
                    ForeColor = Color.FromArgb(200, 205, 215),
                    Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                    Location = new Point(0, 7),
                    AutoSize = true
                };

                Button btnTriRareteDesc = new Button
                {
                    Text = "💎 Rareté ⬇️ (Mythique ➔ Commun)",
                    Location = new Point(48, 2),
                    Size = new Size(215, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnTriRareteDesc.FlatAppearance.BorderSize = 0;

                Button btnTriRareteAsc = new Button
                {
                    Text = "💎 Rareté ⬆️ (Commun ➔ Mythique)",
                    Location = new Point(270, 2),
                    Size = new Size(205, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(44, 48, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnTriRareteAsc.FlatAppearance.BorderSize = 0;

                Button btnTriAtk = new Button
                {
                    Text = "⚔️ Attaque ⬇️",
                    Location = new Point(482, 2),
                    Size = new Size(95, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(44, 48, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnTriAtk.FlatAppearance.BorderSize = 0;

                Button btnTriDef = new Button
                {
                    Text = "🛡️ Défense ⬇️",
                    Location = new Point(584, 2),
                    Size = new Size(95, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(44, 48, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnTriDef.FlatAppearance.BorderSize = 0;

                Button btnTriType = new Button
                {
                    Text = "📦 Type",
                    Location = new Point(686, 2),
                    Size = new Size(98, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(44, 48, 60),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnTriType.FlatAppearance.BorderSize = 0;

                Label lblFiltreEquip = new Label
                {
                    Text = "Filtrer :",
                    ForeColor = Color.FromArgb(200, 205, 215),
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Location = new Point(2, 43),
                    AutoSize = true
                };

                ComboBox cbFiltreType = new ComboBox
                {
                    Location = new Point(55, 39),
                    Size = new Size(210, 27),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(32, 36, 48),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f)
                };
                cbFiltreType.Items.AddRange(new object[] { "Tous les types", "Arme", "Armure", "Casque", "Anneau", "Amulette" });
                cbFiltreType.SelectedIndex = 0;

                ComboBox cbFiltreRarete = new ComboBox
                {
                    Location = new Point(272, 39),
                    Size = new Size(190, 27),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(32, 36, 48),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f)
                };
                cbFiltreRarete.Items.AddRange(new object[] { "Toutes raretés", "Commun", "Rare", "Epique", "Legendaire", "Mythique" });
                cbFiltreRarete.SelectedIndex = 0;

                pnlBarreTri.Controls.AddRange(new Control[] { lblTriTitre, btnTriRareteDesc, btnTriRareteAsc, btnTriAtk, btnTriDef, btnTriType, lblFiltreEquip, cbFiltreType, cbFiltreRarete });

                ListBox lbEquip = new ListBox
                {
                    Location = new Point(0, 72),
                    Size = new Size(530, 386),
                    BackColor = Color.FromArgb(28, 31, 40),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 8.8f)
                };

                Panel pnlActionsEquip = new Panel
                {
                    Location = new Point(540, 38),
                    Size = new Size(244, 420)
                };

                Button btnEquiper = new Button
                {
                    Text = "🗡️ Équiper Sélection\n(Remplacer la pièce active)",
                    Location = new Point(0, 0),
                    Size = new Size(244, 44),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnEquiper.FlatAppearance.BorderSize = 0;

                Button btnEquiperMeilleur = new Button
                {
                    Text = "⚡ ÉQUIPER BEST\n(Maximiser Dégâts 🔥)",
                    Location = new Point(0, 50),
                    Size = new Size(244, 52),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(230, 126, 34),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnEquiperMeilleur.FlatAppearance.BorderSize = 0;

                GroupBox gbStuffPorte = new GroupBox
                {
                    Text = "Équipement Porté Actuel",
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(0, 110),
                    Size = new Size(244, 190),
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold)
                };

                Label lblArmePortee = new Label { Location = new Point(10, 20), Size = new Size(224, 28), ForeColor = Color.LightSkyBlue, Font = new Font("Segoe UI", 7.8f) };
                Label lblArmurePortee = new Label { Location = new Point(10, 52), Size = new Size(224, 28), ForeColor = Color.LightGreen, Font = new Font("Segoe UI", 7.8f) };
                Label lblCasquePorte = new Label { Location = new Point(10, 84), Size = new Size(224, 28), ForeColor = Color.Gold, Font = new Font("Segoe UI", 7.8f) };
                Label lblAnneauPorte = new Label { Location = new Point(10, 116), Size = new Size(224, 28), ForeColor = Color.Plum, Font = new Font("Segoe UI", 7.8f) };
                Label lblAmulettePortee = new Label { Location = new Point(10, 148), Size = new Size(224, 28), ForeColor = Color.Khaki, Font = new Font("Segoe UI", 7.8f) };

                gbStuffPorte.Controls.AddRange(new Control[] { lblArmePortee, lblArmurePortee, lblCasquePorte, lblAnneauPorte, lblAmulettePortee });

                Panel pnlInfoDps = new Panel
                {
                    Location = new Point(0, 308),
                    Size = new Size(244, 112),
                    BackColor = Color.FromArgb(26, 29, 38),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label lblInfoDps = new Label
                {
                    Location = new Point(8, 6),
                    Size = new Size(226, 98),
                    Text = "💡 CONSEIL D'ÉQUIPEMENT\n• Triez par Rareté pour trouver vos pièces Mythiques & Légendaires !\n• Utilisez [ÉQUIPER BEST] pour équiper le combo maximisant vos dégâts bruts et critiques.",
                    ForeColor = Color.FromArgb(189, 195, 199),
                    Font = new Font("Segoe UI", 7.8f)
                };
                pnlInfoDps.Controls.Add(lblInfoDps);

                pnlActionsEquip.Controls.AddRange(new Control[] { btnEquiper, btnEquiperMeilleur, gbStuffPorte, pnlInfoDps });
                pnlVueEquip.Controls.AddRange(new Control[] { pnlBarreTri, lbEquip, pnlActionsEquip });

                // ==============================================================
                // 2. PANNEAU SORTS & GRIMOIRE (CHOIX DES SORTS ÉQUIPÉS)
                // ==============================================================
                Panel pnlVueSorts = new Panel
                {
                    Location = new Point(20, 106),
                    Size = new Size(784, 465),
                    Visible = false
                };

                Panel pnlSortActifBandeau = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(784, 44),
                    BackColor = Color.FromArgb(32, 28, 48),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label lblSortActifTitre = new Label
                {
                    Location = new Point(12, 10),
                    Size = new Size(760, 24),
                    Text = $"⭐ Sort Équipé en Combat [{entrees.Touche("Sort2")}] : Aucun",
                    Font = new Font("Segoe UI", 9.8f, FontStyle.Bold),
                    ForeColor = Color.Gold
                };
                pnlSortActifBandeau.Controls.Add(lblSortActifTitre);

                ListBox lbSorts = new ListBox
                {
                    Location = new Point(0, 52),
                    Size = new Size(460, 405),
                    BackColor = Color.FromArgb(28, 31, 40),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 9f)
                };

                Panel pnlDetailsSort = new Panel
                {
                    Location = new Point(470, 52),
                    Size = new Size(314, 405),
                    BackColor = Color.FromArgb(24, 27, 36),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label lblSpellNom = new Label
                {
                    Location = new Point(14, 14),
                    Size = new Size(284, 26),
                    Text = "Sélectionnez un sort",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.Cyan
                };

                Label lblSpellBadges = new Label
                {
                    Location = new Point(14, 44),
                    Size = new Size(284, 22),
                    Text = "",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.Gold
                };

                RichTextBox rtbSpellDesc = new RichTextBox
                {
                    Location = new Point(14, 72),
                    Size = new Size(284, 195),
                    BackColor = Color.FromArgb(18, 20, 28),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    ReadOnly = true,
                    Font = new Font("Segoe UI", 8.8f)
                };

                Button btnEquiperCeSort = new Button
                {
                    Text = $"🔮 ÉQUIPER CE SORT EN COMBAT\n(Touche [{entrees.Touche("Sort2")}] en jeu ⚡)",
                    Location = new Point(14, 278),
                    Size = new Size(284, 48),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnEquiperCeSort.FlatAppearance.BorderSize = 0;

                Button btnResetSortInne = new Button
                {
                    Text = "✨ Rétablir le Sort Inné de Classe",
                    Location = new Point(14, 332),
                    Size = new Size(284, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(44, 48, 60),
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnResetSortInne.FlatAppearance.BorderSize = 0;

                Button btnAllerKaëlith = new Button
                {
                    Text = "⚒️ Crafter de Nouveaux Sorts (Maître Kaëlith)",
                    Location = new Point(14, 368),
                    Size = new Size(284, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnAllerKaëlith.FlatAppearance.BorderSize = 0;

                pnlDetailsSort.Controls.AddRange(new Control[] { lblSpellNom, lblSpellBadges, rtbSpellDesc, btnEquiperCeSort, btnResetSortInne, btnAllerKaëlith });
                pnlVueSorts.Controls.AddRange(new Control[] { pnlSortActifBandeau, lbSorts, pnlDetailsSort });

                // ==============================================================
                // 3. PANNEAU POTIONS & CONSOMMABLES
                // ==============================================================
                Panel pnlVuePotions = new Panel
                {
                    Location = new Point(20, 106),
                    Size = new Size(784, 465),
                    Visible = false
                };

                Panel pnlVueTalents = new Panel
                {
                    Location = new Point(20, 106),
                    Size = new Size(784, 465),
                    Visible = false
                };

                // L'arbre de talents visuel interactif est instancié dynamiquement via ConstruireVueArbreTalents

                GroupBox gbPotions = new GroupBox
                {
                    Text = "Potions & Objets Rapides",
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(0, 0),
                    Size = new Size(784, 180),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                };

                Button btnBoireSoin = new Button
                {
                    Text = $"🧪 Potion Soin Mineure ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure)})\n[Boire +60 PV]",
                    Location = new Point(25, 30),
                    Size = new Size(225, 54),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnBoireSoin.FlatAppearance.BorderSize = 0;

                Button btnBoireSoinMaj = new Button
                {
                    Text = $"🧪 Potion Soin Majeure ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMajeure)})\n[Boire +150 PV]",
                    Location = new Point(265, 30),
                    Size = new Size(225, 54),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(30, 132, 73),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnBoireSoinMaj.FlatAppearance.BorderSize = 0;

                Button btnBoireMana = new Button
                {
                    Text = $"💧 Potion Mana Mineure ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMineure)})\n[Boire +50 Mana]",
                    Location = new Point(25, 96),
                    Size = new Size(225, 54),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnBoireMana.FlatAppearance.BorderSize = 0;

                Button btnBoireManaMaj = new Button
                {
                    Text = $"💧 Potion Mana Majeure ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMajeure)})\n[Boire +120 Mana]",
                    Location = new Point(265, 96),
                    Size = new Size(225, 54),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(31, 97, 141),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnBoireManaMaj.FlatAppearance.BorderSize = 0;

                Label lblBombes = new Label
                {
                    Text = $"💣 Bombes Incendiaires : {hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire)} en réserve\n⚡ Parchemins de Téléport : {hero.ObtenirQuantiteConsommable(TypeConsommable.ParcheminTeleport)}",
                    Location = new Point(515, 45),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(200, 205, 215),
                    Font = new Font("Segoe UI", 9f)
                };

                gbPotions.Controls.AddRange(new Control[] { btnBoireSoin, btnBoireSoinMaj, btnBoireMana, btnBoireManaMaj, lblBombes });
                pnlVuePotions.Controls.Add(gbPotions);

                // Bouton Fermer
                Button btnFermer = new Button
                {
                    Text = "Fermer l'Inventaire",
                    Location = new Point(325, 580),
                    Size = new Size(190, 36),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnFermer.FlatAppearance.BorderSize = 0;
                btnFermer.Click += (s, e) => dlg.Close();

                // Formatage textuel des équipements avec badge visuel de rareté
                Func<Equipement, string> formaterItemSac = (item) =>
                {
                    string badge = item.RareteItem switch
                    {
                        Rarete.Mythique => "👑 [MYTHIQUE]",
                        Rarete.Legendaire => "🟡 [LÉGENDAIRE]",
                        Rarete.Epique => "🟣 [ÉPIQUE]",
                        Rarete.Rare => "🔵 [RARE]",
                        _ => "⚪ [COMMUN]"
                    };
                    return $"{badge} {item.ObtenirDescription()}";
                };

                List<Equipement> equipementsAffiches = new List<Equipement>();

                Action rafraichirComparaison = () =>
                {
                    if (lbEquip.SelectedIndex < 0 || lbEquip.SelectedIndex >= equipementsAffiches.Count)
                    {
                        lblInfoDps.Text = "Sélectionnez une pièce pour comparer ses statistiques à l'équipement actuellement porté.";
                        return;
                    }

                    Equipement selection = equipementsAffiches[lbEquip.SelectedIndex];
                    lblInfoDps.Text = ConstruireComparaisonEquipementTexte(hero, selection);
                };

                // Action de rafraîchissement global
                Action rafraichirInventaire = () =>
                {
                    // 1. Liste des équipements
                    int idxEquip = lbEquip.SelectedIndex;
                    Equipement? selectionAvant = idxEquip >= 0 && idxEquip < equipementsAffiches.Count ? equipementsAffiches[idxEquip] : null;
                    IEnumerable<Equipement> filtrage = hero.SacEquipements;
                    string filtreType = cbFiltreType.SelectedItem?.ToString() ?? "Tous les types";
                    string filtreRarete = cbFiltreRarete.SelectedItem?.ToString() ?? "Toutes raretés";
                    if (filtreType != "Tous les types")
                        filtrage = filtrage.Where(eq => eq.Type.ToString() == filtreType);
                    if (filtreRarete != "Toutes raretés")
                        filtrage = filtrage.Where(eq => eq.RareteItem.ToString() == filtreRarete);
                    equipementsAffiches = filtrage.ToList();
                    lbEquip.BeginUpdate();
                    lbEquip.Items.Clear();
                    if (equipementsAffiches.Count == 0 && hero.SacEquipements.Count == 0)
                    {
                        lbEquip.Items.Add("(Votre sac est vide — Aucun équipement en réserve)");
                    }
                    else if (equipementsAffiches.Count == 0)
                    {
                        lbEquip.Items.Add("(Aucun objet ne correspond à ces filtres)");
                    }
                    else
                    {
                        foreach (Equipement eq in equipementsAffiches)
                        {
                            lbEquip.Items.Add(formaterItemSac(eq));
                        }
                    }
                    if (selectionAvant != null)
                        lbEquip.SelectedIndex = equipementsAffiches.IndexOf(selectionAvant);
                    else if (equipementsAffiches.Count > 0 && idxEquip >= 0)
                        lbEquip.SelectedIndex = Math.Min(idxEquip, equipementsAffiches.Count - 1);
                    lbEquip.EndUpdate();
                    rafraichirComparaison();

                    // 2. Équipements portés
                    lblArmePortee.Text = $"🗡️ Arme : {(hero.ArmeEquipee != null ? hero.ArmeEquipee.ObtenirDescription() : "Aucune")}";
                    lblArmurePortee.Text = $"🛡️ Armure : {(hero.ArmureEquipee != null ? hero.ArmureEquipee.ObtenirDescription() : "Aucune")}";
                    lblCasquePorte.Text = $"🪖 Casque : {(hero.CasqueEquipe != null ? hero.CasqueEquipe.ObtenirDescription() : "Aucun")}";
                    lblAnneauPorte.Text = $"💍 Anneau : {(hero.AnneauEquipe != null ? hero.AnneauEquipe.ObtenirDescription() : "Aucun")}";
                    lblAmulettePortee.Text = $"📿 Amulette : {(hero.AmuletteEquipee != null ? hero.AmuletteEquipee.ObtenirDescription() : "Aucune")}";

                    // 3. Stats & Boutons
                    lblStatsHero.Text = $"⚔️ Attaque : {hero.AttaqueTotale}   •   🎯 Critique : {hero.ChanceCritiqueTotale}%   •   🩸 Vol de Vie : {hero.VampirismeTotal}%   •   🛡️ Défense : {hero.DefenseTotale}   •   💧 Mana : {hero.ManaActuel}/{hero.ManaMaxTotal}";
                    btnOngletEquip.Text = $"🎒 Équipements & Sac ({hero.SacEquipements.Count})";
                    btnOngletSorts.Text = $"🔮 Sorts & Grimoire [{entrees.Touche("Sort2")}] ({hero.CompetencesDebloquees.Count})";

                    btnBoireSoin.Text = $"🧪 Potion Soin Mineure ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMineure)})\n[Boire +60 PV]";
                    btnBoireSoinMaj.Text = $"🧪 Potion Soin Majeure ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMajeure)})\n[Boire +150 PV]";
                    btnBoireMana.Text = $"💧 Potion Mana Mineure ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMineure)})\n[Boire +50 Mana]";
                    btnBoireManaMaj.Text = $"💧 Potion Mana Majeure ({hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMajeure)})\n[Boire +120 Mana]";
                    lblBombes.Text = $"💣 Bombes Incendiaires : {hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire)} en réserve\n⚡ Parchemins de Téléport : {hero.ObtenirQuantiteConsommable(TypeConsommable.ParcheminTeleport)}";

                    // 4. Sorts & Grimoire
                    string nomSortActif = hero.CompetenceEquipee != null ? $"{hero.CompetenceEquipee.Icone} {hero.CompetenceEquipee.Nom} ({hero.CompetenceEquipee.CoutMana} Mana | x{hero.CompetenceEquipee.MultiplicateurDegats:F1} Dégâts)" : "✨ Sort Inné de Classe (15 Mana)";
                    lblSortActifTitre.Text = $"⭐ Sort Actif Équipé en Combat [{entrees.Touche("Sort2")}] : {nomSortActif}";

                    int idxSort = lbSorts.SelectedIndex;
                    lbSorts.BeginUpdate();
                    lbSorts.Items.Clear();
                    foreach (var srt in hero.CompetencesDebloquees)
                    {
                        bool estEquipe = hero.CompetenceEquipee?.Id == srt.Id;
                        string prefix = estEquipe ? "⭐ [ÉQUIPÉ]" : "○ [DISPONIBLE]";
                        lbSorts.Items.Add($"{prefix} {srt.Icone} {srt.Nom} ({srt.CoutMana} Mana | x{srt.MultiplicateurDegats:F1} Dégâts)");
                    }
                    if (idxSort >= 0 && idxSort < lbSorts.Items.Count) lbSorts.SelectedIndex = idxSort;
                    else if (lbSorts.Items.Count > 0 && lbSorts.SelectedIndex < 0) lbSorts.SelectedIndex = 0;
                    lbSorts.EndUpdate();
                };

                lbEquip.SelectedIndexChanged += (s, e) => rafraichirComparaison();
                cbFiltreType.SelectedIndexChanged += (s, e) => rafraichirInventaire();
                cbFiltreRarete.SelectedIndexChanged += (s, e) => rafraichirInventaire();

                // Gestionnaire d'affichage des détails du sort sélectionné
                Action rafraichirDetailsSort = () =>
                {
                    if (lbSorts.SelectedIndex >= 0 && lbSorts.SelectedIndex < hero.CompetencesDebloquees.Count)
                    {
                        var s = hero.CompetencesDebloquees[lbSorts.SelectedIndex];
                        bool estEquipe = hero.CompetenceEquipee?.Id == s.Id;

                        lblSpellNom.Text = $"{s.Icone} {s.Nom}";
                        lblSpellBadges.Text = estEquipe ? $"⭐ ACTIF SUR [{entrees.Touche("Sort2")}]" : "○ EN RÉSERVE (CLIQUEZ CI-DESSOUS POUR ÉQUIPER)";
                        lblSpellBadges.ForeColor = estEquipe ? Color.Gold : Color.LightSkyBlue;

                        rtbSpellDesc.Clear();
                        rtbSpellDesc.AppendText($"💧 Coût en Mana : {s.CoutMana} Mana\n");
                        rtbSpellDesc.AppendText($"⚔️ Puissance : x{s.MultiplicateurDegats:F1} Dégâts Totaux du Héros\n");
                        rtbSpellDesc.AppendText($"✨ Effet Spécial : {s.Effet} {(s.ValeurEffet > 0 ? $"(Valeur: {s.ValeurEffet})" : "")}\n\n");
                        rtbSpellDesc.AppendText($"{s.Description}\n\n");
                        rtbSpellDesc.AppendText($"💡 Utilisation : En combat 2D, orientez le curseur et appuyez sur [{entrees.Touche("Sort2")}] pour lancer ce sort !");

                        btnEquiperCeSort.Enabled = !estEquipe;
                        btnEquiperCeSort.BackColor = estEquipe ? Color.FromArgb(35, 45, 55) : Color.FromArgb(41, 128, 185);
                        btnEquiperCeSort.Text = estEquipe ? $"✓ DÉJÀ ÉQUIPÉ SUR [{entrees.Touche("Sort2")}]" : $"🔮 ÉQUIPER CE SORT EN COMBAT [{entrees.Touche("Sort2")}]";
                    }
                    else
                    {
                        lblSpellNom.Text = "Aucun sort sélectionné";
                        lblSpellBadges.Text = "";
                        rtbSpellDesc.Clear();
                        btnEquiperCeSort.Enabled = false;
                    }
                };

                lbSorts.SelectedIndexChanged += (s, e) => rafraichirDetailsSort();

                Action rafraichirTalents = ConstruireVueArbreTalents(pnlVueTalents, hero, () => rafraichirInventaire());


                // Navigation par onglets
                Action<int> changerOnglet = (onglet) =>
                {
                    pnlVueEquip.Visible = (onglet == 0);
                    pnlVueSorts.Visible = (onglet == 1);
                    pnlVuePotions.Visible = (onglet == 2);
                    pnlVueTalents.Visible = (onglet == 3);

                    btnOngletEquip.BackColor = (onglet == 0) ? Color.FromArgb(41, 128, 185) : Color.FromArgb(32, 36, 48);
                    btnOngletEquip.ForeColor = (onglet == 0) ? Color.White : Color.LightGray;

                    btnOngletSorts.BackColor = (onglet == 1) ? Color.FromArgb(142, 68, 173) : Color.FromArgb(32, 36, 48);
                    btnOngletSorts.ForeColor = (onglet == 1) ? Color.White : Color.LightGray;

                    btnOngletPotions.BackColor = (onglet == 2) ? Color.FromArgb(39, 174, 96) : Color.FromArgb(32, 36, 48);
                    btnOngletPotions.ForeColor = (onglet == 2) ? Color.White : Color.LightGray;

                    btnOngletTalents.BackColor = (onglet == 3) ? Color.FromArgb(241, 196, 15) : Color.FromArgb(32, 36, 48);
                    btnOngletTalents.ForeColor = (onglet == 3) ? Color.Black : Color.LightGray;

                    rafraichirInventaire();
                    if (onglet == 1) rafraichirDetailsSort();
                    if (onglet == 3) rafraichirTalents();
                };

                btnOngletEquip.Click += (s, e) => changerOnglet(0);
                btnOngletSorts.Click += (s, e) => changerOnglet(1);
                btnOngletPotions.Click += (s, e) => changerOnglet(2);
                btnOngletTalents.Click += (s, e) => changerOnglet(3);

                // --- BOUTONS DE TRI DE L'INVENTAIRE ---
                btnTriRareteDesc.Click += (s, e) =>
                {
                    hero.SacEquipements.Sort((a, b) =>
                    {
                        int cr = ((int)b.RareteItem).CompareTo((int)a.RareteItem);
                        if (cr != 0) return cr;
                        int atkA = a.BonusAttaque + (a.NiveauAmelioration * 3);
                        int atkB = b.BonusAttaque + (b.NiveauAmelioration * 3);
                        int ca = atkB.CompareTo(atkA);
                        if (ca != 0) return ca;
                        return b.ValeurOr.CompareTo(a.ValeurOr);
                    });
                    AudioSynthetiseur.SonCritique();
                    rafraichirInventaire();
                };

                btnTriRareteAsc.Click += (s, e) =>
                {
                    hero.SacEquipements.Sort((a, b) =>
                    {
                        int cr = ((int)a.RareteItem).CompareTo((int)b.RareteItem);
                        if (cr != 0) return cr;
                        return a.ValeurOr.CompareTo(b.ValeurOr);
                    });
                    AudioSynthetiseur.SonCritique();
                    rafraichirInventaire();
                };

                btnTriAtk.Click += (s, e) =>
                {
                    hero.SacEquipements.Sort((a, b) =>
                    {
                        int atkA = a.BonusAttaque + (a.NiveauAmelioration * 3);
                        int atkB = b.BonusAttaque + (b.NiveauAmelioration * 3);
                        int ca = atkB.CompareTo(atkA);
                        if (ca != 0) return ca;
                        return ((int)b.RareteItem).CompareTo((int)a.RareteItem);
                    });
                    AudioSynthetiseur.SonCritique();
                    rafraichirInventaire();
                };

                btnTriDef.Click += (s, e) =>
                {
                    hero.SacEquipements.Sort((a, b) =>
                    {
                        int defA = a.BonusDefense + (a.NiveauAmelioration * 2);
                        int defB = b.BonusDefense + (b.NiveauAmelioration * 2);
                        int cd = defB.CompareTo(defA);
                        if (cd != 0) return cd;
                        return ((int)b.RareteItem).CompareTo((int)a.RareteItem);
                    });
                    AudioSynthetiseur.SonCritique();
                    rafraichirInventaire();
                };

                btnTriType.Click += (s, e) =>
                {
                    hero.SacEquipements.Sort((a, b) =>
                    {
                        int ct = a.Type.CompareTo(b.Type);
                        if (ct != 0) return ct;
                        return ((int)b.RareteItem).CompareTo((int)a.RareteItem);
                    });
                    AudioSynthetiseur.SonCritique();
                    rafraichirInventaire();
                };

                // --- ACTIONS D'ÉQUIPEMENT ---
                btnEquiper.Click += (s, e) =>
                {
                    if (lbEquip.SelectedIndex >= 0 && lbEquip.SelectedIndex < equipementsAffiches.Count)
                    {
                        Equipement selection = equipementsAffiches[lbEquip.SelectedIndex];
                        hero.EquiperObjet(selection);
                        AudioSynthetiseur.SonCoupEpee();
                        rafraichirInventaire();
                    }
                    else AfficherMessageAvecPause(dlg, "Veuillez sélectionner un équipement valide dans la liste !", "Sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                btnEquiperMeilleur.Click += (s, e) =>
                {
                    if (hero.SacEquipements.Count == 0)
                    {
                        AfficherMessageAvecPause(dlg, "Votre sac est vide ! Obtenez des équipements en donjon, sur les boss ou à la forge.", "Sac vide", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var res = hero.AutoEquiperMeilleurStuffDegats();
                    if (res.ChangementsCount > 0)
                    {
                        rafraichirInventaire();
                        camera.DeclencherSecousse(10f, 0.3f);
                        monde.Particules.EmettreAnneauExplosion(joueur2D.Position, Color.Gold, 40, 240f);
                        monde.AjouterTexteFlottant(joueur2D.Position, "⚡ MEILLEUR STUFF ÉQUIPÉ !", Color.Gold, true);
                        AudioSynthetiseur.SonCritique();

                        string resume = "⚡ OPTIMISATION DÉGÂTS MAX RÉUSSIE !\n\n" +
                                        "Nouvelles pièces équipées automatiquement :\n" +
                                        string.Join("\n", res.Details) + "\n\n" +
                                        "📊 Bilan de vos Statistiques :\n" +
                                        $"• ⚔️ Attaque Totale  : {res.AncienneAtk} ➜ {hero.AttaqueTotale} ({(res.GainAttaque >= 0 ? "+" : "")}{res.GainAttaque})\n" +
                                        $"• 🎯 Chance Critique : {res.AncienCrit}% ➜ {hero.ChanceCritiqueTotale}% ({(res.GainCritique >= 0 ? "+" : "")}{res.GainCritique}%)\n" +
                                        $"• 🩸 Vol de Vie      : {hero.VampirismeTotal}%\n" +
                                        $"• 🛡️ Défense Totale : {hero.DefenseTotale}\n\n" +
                                        "Votre champion est équipé avec le stuff le plus puissant pour frapper fort !";

                        AfficherMessageAvecPause(dlg, resume, "⚡ Équipement Optimal Dégâts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        AfficherMessageAvecPause(dlg,
                            $"⚡ Votre équipement actuel est déjà optimal pour les dégâts !\n\nAucune pièce dans votre réserve ne permet d'augmenter davantage votre Attaque Totale ou votre potentiel offensif.\n\nStats actuelles :\n• ⚔️ Attaque Totale : {hero.AttaqueTotale}\n• 🎯 Chance Critique : {hero.ChanceCritiqueTotale}%\n• 🩸 Vol de Vie : {hero.VampirismeTotal}%",
                            "Équipement Déjà Optimal",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                };

                // --- ACTIONS DU GRIMOIRE DE SORTS ---
                btnEquiperCeSort.Click += (s, e) =>
                {
                    if (lbSorts.SelectedIndex >= 0 && lbSorts.SelectedIndex < hero.CompetencesDebloquees.Count)
                    {
                        var sortChoisi = hero.CompetencesDebloquees[lbSorts.SelectedIndex];
                        hero.CompetenceEquipee = sortChoisi;
                        AudioSynthetiseur.SonTirMagique();
                        camera.DeclencherSecousse(8f, 0.2f);
                        monde.Particules.EmettreAnneauExplosion(joueur2D.Position, Color.Cyan, 35, 240f);
                        monde.AjouterTexteFlottant(joueur2D.Position, $"🔮 Sort Équipé : {sortChoisi.Nom} !", Color.Gold, true);
                        rafraichirInventaire();
                        rafraichirDetailsSort();
                    }
                };

                btnResetSortInne.Click += (s, e) =>
                {
                    hero.AssurerSortsParDefaut();
                    var sortInne = hero.CompetencesDebloquees.FirstOrDefault(c => c.Id.StartsWith("sort_"));
                    if (sortInne != null)
                    {
                        hero.CompetenceEquipee = sortInne;
                        AudioSynthetiseur.SonCritique();
                        monde.AjouterTexteFlottant(joueur2D.Position, $"✨ Sort Inné Rétabli !", Color.Cyan, true);
                        rafraichirInventaire();
                        rafraichirDetailsSort();
                    }
                };

                btnAllerKaëlith.Click += (s, e) =>
                {
                    dlg.Close();
                    OuvrirAtelierArtisanat();
                };

                // --- ACTIONS POTIONS ---
                btnBoireSoin.Click += (s, e) =>
                {
                    BoirePotionSoin();
                    rafraichirInventaire();
                };

                btnBoireSoinMaj.Click += (s, e) =>
                {
                    if (hero.ObtenirQuantiteConsommable(TypeConsommable.PotionSoinMajeure) > 0)
                    {
                        hero.ConsommerObjet(TypeConsommable.PotionSoinMajeure);
                        hero.Soigner(150);
                        AudioSynthetiseur.SonLoot();
                        monde.Particules.EmettreAnneauExplosion(joueur2D.Position, Color.LimeGreen, 20, 140f);
                        monde.AjouterTexteFlottant(joueur2D.Position, "+150 PV (Potion Majeure) !", Color.LimeGreen, true);
                        rafraichirInventaire();
                    }
                    else AfficherMessageAvecPause(dlg, "Vous n'avez pas de Potion de Soin Majeure en réserve !", "Potion vide", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                btnBoireMana.Click += (s, e) =>
                {
                    BoirePotionMana();
                    rafraichirInventaire();
                };

                btnBoireManaMaj.Click += (s, e) =>
                {
                    if (hero.ObtenirQuantiteConsommable(TypeConsommable.PotionManaMajeure) > 0)
                    {
                        hero.ConsommerObjet(TypeConsommable.PotionManaMajeure);
                        hero.RegenererMana(120);
                        AudioSynthetiseur.SonTirMagique();
                        monde.Particules.EmettreAnneauExplosion(joueur2D.Position, Color.Cyan, 20, 140f);
                        monde.AjouterTexteFlottant(joueur2D.Position, "+120 Mana (Potion Majeure) !", Color.Cyan, true);
                        rafraichirInventaire();
                    }
                    else AfficherMessageAvecPause(dlg, "Vous n'avez pas de Potion de Mana Majeure en réserve !", "Potion vide", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };


                Button btnOuvrirCodexDrops = new Button
                {
                    Text = "📖 Codex des Drops & Butins de Boss / Raids [B]",
                    Location = new Point(20, 580),
                    Size = new Size(350, 34),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnOuvrirCodexDrops.FlatAppearance.BorderSize = 0;
                btnOuvrirCodexDrops.Click += (s, e) =>
                {
                    OuvrirCodexDropsBoss();
                };

                dlg.Controls.AddRange(new Control[] { lblTitre, lblStatsHero, pnlOnglets, pnlVueEquip, pnlVueSorts, pnlVuePotions, pnlVueTalents, btnOuvrirCodexDrops, btnFermer });
                rafraichirInventaire();
                changerOnglet(0);
                dlg.ShowDialog(this);
            }

            entrees.ReinitialiserToutesTouches();
            boucleJeu.Start();
        }

        private void OuvrirForge()
        {
            boucleJeu.Stop();
            using (Form dlg = new Form())
            {
                dlg.Text = "🔨 Forge Royale de Brom";
                dlg.Size = new Size(720, 520);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(22, 24, 30);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                Label lblTitre = new Label
                {
                    Text = "🔨 ATELIER D'AMÉLIORATION & ARTISANAT DE BROM",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(230, 126, 34),
                    Location = new Point(20, 15),
                    AutoSize = true
                };

                Label lblRessources = new Label
                {
                    Text = $"💎 Pierres de Forge : {hero.PierresDeForge}   •   🪙 Or disponible : {hero.Or}",
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    Location = new Point(20, 45),
                    AutoSize = true
                };

                ListBox lbPieces = new ListBox
                {
                    Location = new Point(20, 80),
                    Size = new Size(460, 360),
                    BackColor = Color.FromArgb(28, 31, 40),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f)
                };

                List<Equipement> pieces = new List<Equipement>();
                Action rafraichirPieces = () =>
                {
                    pieces.Clear();
                    lbPieces.Items.Clear();
                    if (hero.ArmeEquipee != null) pieces.Add(hero.ArmeEquipee);
                    if (hero.ArmureEquipee != null) pieces.Add(hero.ArmureEquipee);
                    if (hero.CasqueEquipe != null) pieces.Add(hero.CasqueEquipe);
                    if (hero.AnneauEquipe != null) pieces.Add(hero.AnneauEquipe);
                    if (hero.AmuletteEquipee != null) pieces.Add(hero.AmuletteEquipee);

                    foreach (var p in pieces)
                    {
                        int coutPierres = p.NiveauAmelioration + 1;
                        int coutOr = (p.NiveauAmelioration + 1) * 60;
                        lbPieces.Items.Add($"[{p.Type}] {p.ObtenirNomComplet()} -> Coût: {coutPierres} Pierres & {coutOr} Or");
                    }
                    lblRessources.Text = $"💎 Pierres de Forge : {hero.PierresDeForge}   •   🪙 Or disponible : {hero.Or}";
                };

                Button btnUp = new Button
                {
                    Text = "🔨 FORGER +1\n(Renforcer la pièce)",
                    Location = new Point(500, 80),
                    Size = new Size(180, 65),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(211, 84, 0),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnUp.FlatAppearance.BorderSize = 0;
                btnUp.Click += (s, e) =>
                {
                    if (lbPieces.SelectedIndex >= 0 && lbPieces.SelectedIndex < pieces.Count)
                    {
                        var eq = pieces[lbPieces.SelectedIndex];
                        if (eq.NiveauAmelioration >= 10)
                        {
                            AfficherMessageAvecPause(dlg, "Cette pièce est déjà au niveau maximum (+10) !", "Forge Max", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        int coutPierres = eq.NiveauAmelioration + 1;
                        int coutOr = (eq.NiveauAmelioration + 1) * 60;
                        if (hero.PierresDeForge >= coutPierres && hero.Or >= coutOr)
                        {
                            hero.PierresDeForge -= coutPierres;
                            hero.Or -= coutOr;
                            eq.NiveauAmelioration++;
                            AudioSynthetiseur.SonCritique();
                            rafraichirPieces();
                            AfficherMessageAvecPause(dlg, $"✨ Succès ! Brom renforce votre pièce au rang +{eq.NiveauAmelioration} !\n(+3 Attaque, +2 Défense, +8 PV)", "Succès Forge", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            AfficherMessageAvecPause(dlg, $"Ressources insuffisantes !\nRequis: {coutPierres} Pierres et {coutOr} Or.", "Manque de ressources", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        AfficherMessageAvecPause(dlg, "Veuillez sélectionner une pièce à améliorer.", "Sélection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                Button btnAllerCraft = new Button
                {
                    Text = "⚒️ ATELIER DE CRAFT\n(Armes, Armures, Sorts)",
                    Location = new Point(500, 160),
                    Size = new Size(180, 58),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnAllerCraft.FlatAppearance.BorderSize = 0;
                btnAllerCraft.Click += (s, e) =>
                {
                    dlg.Close();
                    OuvrirAtelierArtisanat();
                };

                dlg.Controls.AddRange(new Control[] { lblTitre, lblRessources, lbPieces, btnUp, btnAllerCraft });
                rafraichirPieces();
                dlg.ShowDialog(this);
            }
            entrees.ReinitialiserToutesTouches();
            boucleJeu.Start();
        }

        private void OuvrirAtelierArtisanat()
        {
            boucleJeu.Stop();
            using (Form dlg = new Form())
            {
                dlg.Text = "⚒️ Atelier d'Artisanat de Maître Kaëlith (Armes, Armures & Sorts)";
                dlg.Size = new Size(920, 660);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(18, 20, 26);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                // En-tête
                Label lblTitre = new Label
                {
                    Text = "⚒️ ATELIER DE FORGE RUNIQUE & ÉCOLE DE MAGIE",
                    Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(241, 196, 15),
                    Location = new Point(20, 12),
                    AutoSize = true
                };

                Label lblSousTitre = new Label
                {
                    Text = "Maître Kaëlith façonne vos armes et armures légendaires, et éveille vos sorts les plus destructeurs !",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(170, 180, 200),
                    Location = new Point(20, 38),
                    AutoSize = true
                };

                Label lblRessources = new Label
                {
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    Location = new Point(20, 60),
                    AutoSize = true
                };

                // Barre des Filtres
                FlowLayoutPanel pnlFiltres = new FlowLayoutPanel
                {
                    Location = new Point(20, 90),
                    Size = new Size(865, 38),
                    BackColor = Color.Transparent
                };

                CategorieCraft? categorieActive = null;
                bool modeGestionSorts = false;

                ListBox lbItems = new ListBox
                {
                    Location = new Point(20, 134),
                    Size = new Size(410, 465),
                    BackColor = Color.FromArgb(24, 27, 36),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    BorderStyle = BorderStyle.FixedSingle,
                    ItemHeight = 26
                };

                // Panneau d'inspection détaillée
                Panel pnlDetail = new Panel
                {
                    Location = new Point(445, 134),
                    Size = new Size(440, 465),
                    BackColor = Color.FromArgb(22, 25, 34),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Label lblNomObjet = new Label
                {
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    Location = new Point(16, 14),
                    Size = new Size(405, 26)
                };

                Label lblBadges = new Label
                {
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 204, 113),
                    Location = new Point(16, 42),
                    Size = new Size(405, 20)
                };

                Label lblDescription = new Label
                {
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(220, 225, 235),
                    Location = new Point(16, 66),
                    Size = new Size(405, 68)
                };

                Label lblCoutOr = new Label
                {
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    Location = new Point(16, 138),
                    AutoSize = true
                };

                RichTextBox rtbMateriaux = new RichTextBox
                {
                    Location = new Point(16, 168),
                    Size = new Size(405, 225),
                    BackColor = Color.FromArgb(16, 18, 24),
                    ForeColor = Color.White,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Segoe UI", 9f)
                };

                Button btnAction = new Button
                {
                    Location = new Point(16, 404),
                    Size = new Size(405, 48),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnAction.FlatAppearance.BorderSize = 0;

                pnlDetail.Controls.AddRange(new Control[] {
                    lblNomObjet, lblBadges, lblDescription, lblCoutOr, rtbMateriaux, btnAction
                });

                List<RecetteCraft> recettesFiltrees = new List<RecetteCraft>();
                List<CompetenceCraftable> sortsPossedes = new List<CompetenceCraftable>();

                Action rafraichirVue = () =>
                {
                    int totalMats = 0;
                    if (hero.MateriauxCraft != null)
                    {
                        foreach (var v in hero.MateriauxCraft.Values) totalMats += v;
                    }
                    lblRessources.Text = $"💰 Or : {hero.Or}   •   💎 Pierres de Forge : {hero.PierresDeForge}   •   🔮 Sorts Maîtrisés : {hero.CompetencesDebloquees.Count}   •   📦 Matériaux en Réserve : {totalMats}";

                    lbItems.Items.Clear();

                    if (!modeGestionSorts)
                    {
                        recettesFiltrees = CatalogueCraft.Recettes
                            .Where(r => categorieActive == null || r.Categorie == categorieActive)
                            .ToList();

                        foreach (var r in recettesFiltrees)
                        {
                            bool pret = r.PeutFabriquer(hero);
                            bool dejaAppris = r.CompetenceResultat != null && hero.CompetencesDebloquees.Any(c => c.Id == r.CompetenceResultat.Id);
                            string prefix = dejaAppris ? "⭐ [ACQUIS]" : (pret ? "✓ [PRÊT]" : "✗ [MANQUE]");
                            lbItems.Items.Add($"{prefix} {r.Nom} (Niv. {r.NiveauRequis}+)");
                        }

                        if (recettesFiltrees.Count > 0) lbItems.SelectedIndex = 0;
                        else
                        {
                            lblNomObjet.Text = "Aucune recette trouvée.";
                            lblBadges.Text = "";
                            lblDescription.Text = "";
                            lblCoutOr.Text = "";
                            rtbMateriaux.Clear();
                            btnAction.Enabled = false;
                        }
                    }
                    else
                    {
                        // Mode gestion des sorts équipés
                        sortsPossedes = new List<CompetenceCraftable>(hero.CompetencesDebloquees);
                        foreach (var s in sortsPossedes)
                        {
                            bool estEquipe = hero.CompetenceEquipee?.Id == s.Id;
                            string mark = estEquipe ? "⭐ [ACTIF EN COMBAT]" : "○ [DISPONIBLE]";
                            lbItems.Items.Add($"{mark} {s.Icone} {s.Nom} ({s.CoutMana} Mana)");
                        }

                        if (sortsPossedes.Count > 0) lbItems.SelectedIndex = 0;
                        else
                        {
                            lblNomObjet.Text = "Aucun sort débloqué pour le moment.";
                            lblBadges.Text = "";
                            lblDescription.Text = "Débloquez des sorts magiques dans l'onglet '🔮 Sorts Magiques' pour pouvoir les équiper ici.";
                            lblCoutOr.Text = "";
                            rtbMateriaux.Clear();
                            btnAction.Enabled = false;
                        }
                    }
                };

                // Boutons de filtres
                Action<string, CategorieCraft?, bool> ajouterBoutonFiltre = (titre, cat, gestionSorts) =>
                {
                    Button btnFiltre = new Button
                    {
                        Text = titre,
                        AutoSize = true,
                        Height = 32,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(32, 36, 48),
                        ForeColor = Color.LightGray,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 0, 8, 0)
                    };
                    btnFiltre.FlatAppearance.BorderSize = 0;
                    btnFiltre.Click += (s, e) =>
                    {
                        categorieActive = cat;
                        modeGestionSorts = gestionSorts;
                        foreach (Control c in pnlFiltres.Controls)
                        {
                            if (c is Button b)
                            {
                                b.BackColor = (b == btnFiltre) ? Color.FromArgb(41, 128, 185) : Color.FromArgb(32, 36, 48);
                                b.ForeColor = (b == btnFiltre) ? Color.White : Color.LightGray;
                            }
                        }
                        rafraichirVue();
                    };
                    pnlFiltres.Controls.Add(btnFiltre);
                };

                ajouterBoutonFiltre("✨ Tout", null, false);
                ajouterBoutonFiltre("⚔️ Armes", CategorieCraft.Arme, false);
                ajouterBoutonFiltre("🛡️ Armures", CategorieCraft.Armure, false);
                ajouterBoutonFiltre("💍 Bijoux", CategorieCraft.Bijou, false);
                ajouterBoutonFiltre("🔮 Sorts Magiques", CategorieCraft.Competence, false);
                ajouterBoutonFiltre("🧪 Consommables", CategorieCraft.Consommable, false);
                ajouterBoutonFiltre("⭐ Gérer Sorts Équipés", null, true);

                // Sélection dans la liste
                lbItems.SelectedIndexChanged += (s, e) =>
                {
                    if (!modeGestionSorts)
                    {
                        if (lbItems.SelectedIndex >= 0 && lbItems.SelectedIndex < recettesFiltrees.Count)
                        {
                            var r = recettesFiltrees[lbItems.SelectedIndex];
                            lblNomObjet.Text = r.Nom;
                            lblNomObjet.ForeColor = ObtenirCouleurRarete(r.RareteItem);
                            lblBadges.Text = $"[{r.Categorie.ToString().ToUpper()}]  •  [{r.RareteItem}]  •  [NIVEAU REQUIS : {r.NiveauRequis}+]";
                            lblDescription.Text = r.Description;
                            lblCoutOr.Text = $"💰 Coût en Or : {r.CoutOr} Or  (Vous possédez : {hero.Or} Or)";
                            lblCoutOr.ForeColor = hero.Or >= r.CoutOr ? Color.Gold : Color.IndianRed;

                            rtbMateriaux.Clear();
                            rtbMateriaux.SelectionFont = new Font("Segoe UI", 9f, FontStyle.Bold);
                            rtbMateriaux.SelectionColor = Color.FromArgb(241, 196, 15);
                            rtbMateriaux.AppendText("COMPOSANTS & MATÉRIAUX REQUIS :\n\n");

                            foreach (var mat in r.MateriauxRequis)
                            {
                                int possede = hero.ObtenirMateriau(mat.Key);
                                bool ok = possede >= mat.Value;
                                rtbMateriaux.SelectionFont = new Font("Segoe UI", 9f);
                                rtbMateriaux.SelectionColor = ok ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
                                rtbMateriaux.AppendText($"  {(ok ? "✓" : "✗")} {mat.Key} : {possede} / {mat.Value} requis\n");
                            }

                            bool dejaAppris = r.CompetenceResultat != null && hero.CompetencesDebloquees.Any(c => c.Id == r.CompetenceResultat.Id);
                            if (dejaAppris)
                            {
                                btnAction.Text = "⭐ SORT DÉJÀ MAÎTRISÉ";
                                btnAction.Enabled = false;
                                btnAction.BackColor = Color.FromArgb(60, 65, 75);
                            }
                            else
                            {
                                bool pret = r.PeutFabriquer(hero);
                                btnAction.Enabled = pret;
                                btnAction.BackColor = pret ? Color.FromArgb(39, 174, 96) : Color.FromArgb(60, 65, 75);
                                btnAction.Text = r.Categorie switch
                                {
                                    CategorieCraft.Arme => "⚔️ FORGER CETTE ARME",
                                    CategorieCraft.Armure => "🛡️ FORGER CETTE ARMURE",
                                    CategorieCraft.Bijou => "💍 SERTIR CE BIJOU",
                                    CategorieCraft.Competence => "🔮 ÉTUDIER & DÉBLOQUER CE SORT",
                                    CategorieCraft.Consommable => "🧪 FABRIQUER LE CONSOMMABLE",
                                    _ => "⚒️ FABRIQUER L'OBJET"
                                };
                            }
                        }
                    }
                    else
                    {
                        if (lbItems.SelectedIndex >= 0 && lbItems.SelectedIndex < sortsPossedes.Count)
                        {
                            var srt = sortsPossedes[lbItems.SelectedIndex];
                            bool estEquipe = hero.CompetenceEquipee?.Id == srt.Id;

                            lblNomObjet.Text = $"{srt.Icone} {srt.Nom}";
                            lblNomObjet.ForeColor = Color.Cyan;
                            lblBadges.Text = estEquipe ? $"⭐ [SORT ACTIF EN COMBAT 2D ({entrees.Touche("Sort2")})]" : "○ [SORT EN RÉSERVE]";
                            lblBadges.ForeColor = estEquipe ? Color.Gold : Color.LightGray;
                            lblDescription.Text = $"{srt.Description}\n\nCoût en Mana : {srt.CoutMana} PM  •  Multiplicateur Dégâts : x{srt.MultiplicateurDegats}";
                            lblCoutOr.Text = "";
                            rtbMateriaux.Clear();
                            rtbMateriaux.SelectionColor = Color.LightGreen;
                            rtbMateriaux.AppendText($"En combat 2D, appuyez sur la touche 'E' ou 'F' (Sort 2) pour projeter '{srt.Nom}' !\n\nLe sort transperce les lignes ennemies et déchaîne sa puissance arcanique !");

                            btnAction.Enabled = !estEquipe;
                            btnAction.Text = estEquipe ? "⭐ DÉJÀ ÉQUIPÉ EN COMBAT" : $"⭐ ÉQUIPER CE SORT EN COMBAT ({entrees.Touche("Sort2")})";
                            btnAction.BackColor = estEquipe ? Color.FromArgb(60, 65, 75) : Color.FromArgb(142, 68, 173);
                        }
                    }
                };

                // Clic sur le bouton d'action (Craft ou Équipement de sort)
                btnAction.Click += (s, e) =>
                {
                    if (!modeGestionSorts)
                    {
                        if (lbItems.SelectedIndex >= 0 && lbItems.SelectedIndex < recettesFiltrees.Count)
                        {
                            var r = recettesFiltrees[lbItems.SelectedIndex];
                            if (!r.PeutFabriquer(hero))
                            {
                                AfficherMessageAvecPause(dlg, "Vous ne remplissez pas les conditions nécessaires (Or, niveau ou matériaux manquants) !", "Artisanat Impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                                AudioSynthetiseur.SonCritique();
                                monde.AjouterTexteFlottant(joueur2D.Position, $"⚒️ {eq.Nom} Forgé !", Color.Gold, true);

                                var rep = AfficherMessageAvecPause(dlg, $"Félicitations !\nVous avez forgé avec succès :\n\n{eq.ObtenirNomComplet()}\n{eq.ObtenirDescription()}\n\nSouhaitez-vous équiper cet objet immédiatement ?", "Forge Réussie !", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (rep == DialogResult.Yes)
                                {
                                    hero.EquiperObjet(eq);
                                }
                            }
                            else if (r.CompetenceResultat != null)
                            {
                                var sort = r.CompetenceResultat;
                                if (!hero.CompetencesDebloquees.Any(c => c.Id == sort.Id))
                                {
                                    hero.CompetencesDebloquees.Add(sort);
                                }
                                hero.CompetenceEquipee = sort;

                                AudioSynthetiseur.SonCritique();
                                monde.AjouterTexteFlottant(joueur2D.Position, $"🔮 {sort.Nom} Appris !", Color.MediumPurple, true);

                                AfficherMessageAvecPause(dlg, $"Félicitations !\nVous avez étudié le grimoire de Maître Kaëlith et maîtrisez désormais :\n\n{sort.Icone} {sort.Nom}\n{sort.Description}\nCoût : {sort.CoutMana} Mana  •  Dégâts : x{sort.MultiplicateurDegats}\n\nCe sort est maintenant équipé et prêt à être déchaîné avec la touche E ou F en combat 2D !", "Nouveau Sort Magique Appris !", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (r.ConsommableResultat.HasValue)
                            {
                                hero.AjouterConsommable(r.ConsommableResultat.Value, r.QuantiteConsommable);
                                AudioSynthetiseur.SonLoot();
                                monde.AjouterTexteFlottant(joueur2D.Position, $"+{r.QuantiteConsommable} {r.Nom}", Color.FromArgb(46, 204, 113), true);
                            }

                            int precIndex = lbItems.SelectedIndex;
                            rafraichirVue();
                            if (precIndex >= 0 && precIndex < lbItems.Items.Count) lbItems.SelectedIndex = precIndex;
                        }
                    }
                    else
                    {
                        if (lbItems.SelectedIndex >= 0 && lbItems.SelectedIndex < sortsPossedes.Count)
                        {
                            var srt = sortsPossedes[lbItems.SelectedIndex];
                            hero.CompetenceEquipee = srt;
                            AudioSynthetiseur.SonLoot();
                            monde.AjouterTexteFlottant(joueur2D.Position, $"⭐ {srt.Nom} Équipé !", Color.Cyan, true);

                            int precIndex = lbItems.SelectedIndex;
                            rafraichirVue();
                            if (precIndex >= 0 && precIndex < lbItems.Items.Count) lbItems.SelectedIndex = precIndex;
                            AfficherMessageAvecPause(dlg, $"Le sort '{srt.Nom}' ({srt.Icone}) est désormais assigné à votre touche de sort secondaire ({entrees.Touche("Sort2")}) !", "Sort Équipé", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };

                dlg.Controls.AddRange(new Control[] {
                    lblTitre, lblSousTitre, lblRessources, pnlFiltres, lbItems, pnlDetail
                });

                if (pnlFiltres.Controls.Count > 0 && pnlFiltres.Controls[0] is Button b0)
                {
                    b0.BackColor = Color.FromArgb(41, 128, 185);
                    b0.ForeColor = Color.White;
                }

                rafraichirVue();
                dlg.ShowDialog(this);
            }
            entrees.ReinitialiserToutesTouches();
            boucleJeu.Start();
        }

        private static Color ObtenirCouleurRarete(Rarete rarete) => rarete switch
        {
            Rarete.Commun => Color.LightGray,
            Rarete.Rare => Color.FromArgb(52, 152, 219),
            Rarete.Epique => Color.FromArgb(155, 89, 182),
            Rarete.Legendaire => Color.FromArgb(241, 196, 15),
            Rarete.Mythique => Color.FromArgb(231, 76, 60),
            _ => Color.White
        };

        private void OuvrirQuetes()
        {
            boucleJeu.Stop();
            using (Form dlg = new Form())
            {
                dlg.Text = "📜 Guilde des Aventuriers d'Aethelgard";
                dlg.Size = new Size(720, 520);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(20, 22, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                Label lblTitre = new Label
                {
                    Text = "📜 CONTRATS ROYAUX & PRIMES DE GUILDE",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(142, 68, 173),
                    Location = new Point(20, 15),
                    AutoSize = true
                };

                ListBox lbQuetes = new ListBox
                {
                    Location = new Point(20, 55),
                    Size = new Size(660, 360),
                    BackColor = Color.FromArgb(28, 31, 40),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f)
                };

                Action rafraichirQuetes = () =>
                {
                    lbQuetes.Items.Clear();
                    foreach (var q in hero.QuetesActives)
                    {
                        string statut = q.EstTerminee ? "🎁 [TERMINÉE - PRÊTE À RENDRE]" : $"⚔️ [{q.Progression}/{q.Objectif}]";
                        lbQuetes.Items.Add($"{statut} {q.Titre} : {q.Description} (Primes: {q.RecompenseOr} 🪙, {q.RecompenseXP} XP)");
                    }
                };

                Button btnRendre = new Button
                {
                    Text = "🎁 RENDRE TOUTES LES QUÊTES FINIES",
                    Location = new Point(20, 430),
                    Size = new Size(320, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnRendre.FlatAppearance.BorderSize = 0;
                btnRendre.Click += (s, e) =>
                {
                    int totalOr = 0;
                    int totalXP = 0;
                    int quetesRendues = 0;
                    int totalSceaux = 0;
                    List<string> objetsRecompense = new List<string>();

                    for (int i = hero.QuetesActives.Count - 1; i >= 0; i--)
                    {
                        var q = hero.QuetesActives[i];
                        if (q.EstTerminee && !q.RecompenseReclamee)
                        {
                            totalOr += q.RecompenseOr;
                            totalXP += q.RecompenseXP;
                            hero.ReputationGuilde += q.RecompenseReputation;
                            hero.PierresDeForge += q.RecompensePierresForge;
                            totalSceaux += q.RecompenseSceauxGuilde;
                            if (q.RecompenseItem != null)
                            {
                                hero.SacEquipements.Add(q.RecompenseItem);
                                objetsRecompense.Add(q.RecompenseItem.ObtenirNomComplet());
                            }
                            q.RecompenseReclamee = true;
                            if (!hero.QuetesCompleteesIds.Contains(q.Id))
                                hero.QuetesCompleteesIds.Add(q.Id);
                            quetesRendues++;
                            hero.QuetesActives.RemoveAt(i);
                        }
                    }

                    if (quetesRendues > 0)
                    {
                        hero.Or += totalOr;
                        hero.SceauxDeGuilde += totalSceaux;
                        hero.GagnerXP(totalXP);
                        AudioSynthetiseur.SonCritique();
                        rafraichirQuetes();
                        string resumeObjets = objetsRecompense.Count > 0 ? $"\n• Équipement : {string.Join(", ", objetsRecompense)}" : "";
                        AfficherMessageAvecPause(dlg, $"🎉 Félicitations !\nVous avez rendu {quetesRendues} quête(s).\n\nRécompenses touchées :\n• +{totalOr} Or 🪙\n• +{totalXP} XP ⭐\n• +{totalSceaux} sceaux de guilde{resumeObjets}", "Primes Reçues", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        AfficherMessageAvecPause(dlg, "Aucune quête terminée à valider pour le moment.", "Pas de quêtes prêtes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                Button btnContrats = new Button
                {
                    Text = "📜 CONTRATS SECONDAIRES",
                    Location = new Point(360, 430),
                    Size = new Size(320, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnContrats.FlatAppearance.BorderSize = 0;
                btnContrats.Click += (s, e) =>
                {
                    OuvrirContratsSecondaires();
                    rafraichirQuetes();
                };

                dlg.Controls.AddRange(new Control[] { lblTitre, lbQuetes, btnRendre, btnContrats });
                rafraichirQuetes();
                dlg.ShowDialog(this);
            }
            entrees.ReinitialiserToutesTouches();
            boucleJeu.Start();
        }

        private void OuvrirContratsSecondaires()
        {
            bool reprendreBoucle = boucleJeu.Enabled;
            boucleJeu.Stop();
            using (Form dlg = new Form())
            {
                dlg.Text = "📜 Contrats secondaires";
                dlg.Size = new Size(760, 470);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(20, 22, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                Label titre = new Label
                {
                    Text = $"CONTRATS DE LA CAPITAINE • Niveau {hero.Niveau}",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    Location = new Point(20, 18),
                    AutoSize = true
                };
                Label details = new Label
                {
                    Text = "Accepte un contrat, élimine ses cibles dans les donjons, puis réclame la récompense auprès de la guilde.",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.LightGray,
                    Location = new Point(20, 48),
                    Size = new Size(700, 36)
                };
                ListBox liste = new ListBox
                {
                    Location = new Point(20, 90),
                    Size = new Size(700, 270),
                    BackColor = Color.FromArgb(28, 31, 40),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f)
                };
                List<Quete> contrats = new List<Quete>();

                Action rafraichir = () =>
                {
                    contrats = ContratsSecondaires2D.CreerDisponibles(hero);
                    liste.Items.Clear();
                    foreach (Quete contrat in contrats)
                    {
                        liste.Items.Add($"[{contrat.Difficulte}] {contrat.Titre} — Niv. {contrat.NiveauRequis}+ | {contrat.Objectif} × {contrat.CibleNom} | {contrat.RecompenseOr} Or, {contrat.RecompenseXP} XP");
                    }
                    if (contrats.Count == 0)
                        liste.Items.Add("Aucun nouveau contrat : augmente ton niveau ou termine les contrats déjà acceptés.");
                };

                Button accepter = new Button
                {
                    Text = "ACCEPTER LE CONTRAT",
                    Location = new Point(20, 375),
                    Size = new Size(230, 40),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                };
                accepter.FlatAppearance.BorderSize = 0;
                accepter.Click += (s, e) =>
                {
                    int index = liste.SelectedIndex;
                    if (index < 0 || index >= contrats.Count)
                    {
                        AfficherMessageAvecPause(dlg, "Sélectionne d'abord un contrat disponible.", "Contrat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Quete contrat = contrats[index];
                    if (hero.Niveau < contrat.NiveauRequis ||
                        hero.QuetesCompleteesIds.Contains(contrat.Id) ||
                        hero.QuetesActives.Exists(q => q.Id == contrat.Id))
                    {
                        rafraichir();
                        AfficherMessageAvecPause(dlg, "Ce contrat n'est plus disponible pour ton personnage.", "Contrat indisponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    hero.QuetesActives.Add(contrat);
                    monde.AjouterTexteFlottant(joueur2D.Position, $"📜 Contrat accepté : {contrat.Titre}", Color.Cyan, true);
                    AudioSynthetiseur.SonCritique();
                    rafraichir();
                };

                Button fermer = new Button
                {
                    Text = "FERMER",
                    Location = new Point(490, 375),
                    Size = new Size(230, 40),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                };
                fermer.FlatAppearance.BorderSize = 0;
                fermer.Click += (s, e) => dlg.Close();

                dlg.Controls.AddRange(new Control[] { titre, details, liste, accepter, fermer });
                rafraichir();
                dlg.ShowDialog(this);
            }
            entrees.ReinitialiserToutesTouches();
            if (reprendreBoucle)
                boucleJeu.Start();
        }

        private void ProposerReposAuberge()
        {
            const int cout = 60;
            if (hero.Or < cout)
            {
                AfficherMessageAvecPause(this, $"Un repas et un lit coûtent {cout} pièces d'or. Il te manque {cout - hero.Or} pièces.", "Auberge", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult choix = AfficherMessageAvecPause(
                this,
                $"Dépenser {cout} pièces pour récupérer tous tes PV et ton mana, puis gagner +25% d'XP pendant les 3 prochains combats ?",
                "Repos à l'auberge",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (choix != DialogResult.Yes)
                return;

            hero.Or -= cout;
            hero.PVActuels = hero.PVMaxTotal;
            hero.ManaActuel = hero.ManaMaxTotal;
            hero.BuffReposCombatsRestants = Math.Max(hero.BuffReposCombatsRestants, 3);
            pvAffichage = hero.PVActuels;
            pvGhost = hero.PVActuels;
            monde.AjouterTexteFlottant(joueur2D.Position, "🍲 Repos complet • Bonus XP ×3 combats", Color.LightGreen, true);
            AudioSynthetiseur.SonLoot();
        }

        private void OuvrirBoutiquePotions()
        {
            boucleJeu.Stop();
            using (Form dlg = new Form())
            {
                dlg.Text = "🧪 Échoppe d'Alchimie";
                dlg.Size = new Size(460, 320);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(20, 22, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;

                Label lbl = new Label { Text = $"🧪 APOTHICAIRE SYLAS\nVotre Or : {hero.Or} 🪙", Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Gold, Location = new Point(20, 15), AutoSize = true };

                Button btnAcheterSoin = new Button
                {
                    Text = "Acheter Potion de Soin (+60 PV) — 25 🪙",
                    Location = new Point(20, 70),
                    Size = new Size(400, 45),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = fontGras,
                    Cursor = Cursors.Hand
                };
                btnAcheterSoin.Click += (s, e) =>
                {
                    if (hero.Or >= 25)
                    {
                        hero.Or -= 25;
                        hero.AjouterConsommable(TypeConsommable.PotionSoinMineure, 1);
                        lbl.Text = $"🧪 APOTHICAIRE SYLAS\nVotre Or : {hero.Or} 🪙";
                        AudioSynthetiseur.SonLoot();
                    }
                    else AfficherMessageAvecPause(dlg, "Or insuffisant !", "Pas assez d'or", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };

                Button btnAcheterMana = new Button
                {
                    Text = "Acheter Potion de Mana (+50 Mana) — 20 🪙",
                    Location = new Point(20, 130),
                    Size = new Size(400, 45),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = fontGras,
                    Cursor = Cursors.Hand
                };
                btnAcheterMana.Click += (s, e) =>
                {
                    if (hero.Or >= 20)
                    {
                        hero.Or -= 20;
                        hero.AjouterConsommable(TypeConsommable.PotionManaMineure, 1);
                        lbl.Text = $"🧪 APOTHICAIRE SYLAS\nVotre Or : {hero.Or} 🪙";
                        AudioSynthetiseur.SonLoot();
                    }
                    else AfficherMessageAvecPause(dlg, "Or insuffisant !", "Pas assez d'or", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };

                dlg.Controls.AddRange(new Control[] { lbl, btnAcheterSoin, btnAcheterMana });
                dlg.ShowDialog(this);
            }
            entrees.ReinitialiserToutesTouches();
            boucleJeu.Start();
        }

        private void OuvrirMenuDonjonsEtBoss()
        {
            boucleJeu.Stop();
            using (Form dlg = new Form())
            {
                dlg.Text = "🌀 Portail des Donjons & Panthéon des 30 Boss 2D";
                dlg.Size = new Size(880, 680);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(18, 20, 26);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                // Titre
                Label lblTitre = new Label
                {
                    Text = "🌀 EXPÉDITIONS & PANTHÉON DES 30 BOSS 2D",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(231, 76, 60),
                    Location = new Point(20, 15),
                    AutoSize = true
                };

                Button btnCodexDrops = new Button
                {
                    Text = "📖 CODEX DROPS & BUTINS [B]",
                    Location = new Point(620, 12),
                    Size = new Size(225, 36),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = fontGras,
                    Cursor = Cursors.Hand
                };
                btnCodexDrops.FlatAppearance.BorderSize = 0;
                btnCodexDrops.Click += (s, e) => OuvrirCodexDropsBoss();

                // Sous-titre
                Label lblSousTitre = new Label
                {
                    Text = "Sélectionnez la difficulté souhaitée, puis choisissez votre Boss ou votre Expédition :",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.LightGray,
                    Location = new Point(20, 42),
                    AutoSize = true
                };

                // Barre des Difficultés
                Label lblDiff = new Label
                {
                    Text = "⚔️ DIFFICULTÉ :",
                    Font = fontGras,
                    ForeColor = Color.Gold,
                    Location = new Point(20, 72),
                    AutoSize = true
                };

                Panel pnlDiff = new Panel
                {
                    Location = new Point(140, 65),
                    Size = new Size(700, 40),
                    BackColor = Color.Transparent
                };

                List<Button> boutonsDiff = new List<Button>();
                int bx = 0;
                foreach (var diff in DifficulteBoss2D.Toutes)
                {
                    Button btnD = new Button
                    {
                        Text = diff.Titre,
                        Location = new Point(bx, 0),
                        Size = new Size(165, 34),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = difficulteActive.Nom == diff.Nom ? diff.CouleurBadge : Color.FromArgb(35, 38, 48),
                        ForeColor = Color.White,
                        Font = fontGras,
                        Cursor = Cursors.Hand
                    };
                    btnD.FlatAppearance.BorderSize = difficulteActive.Nom == diff.Nom ? 2 : 1;
                    btnD.FlatAppearance.BorderColor = difficulteActive.Nom == diff.Nom ? Color.White : Color.Gray;
                    btnD.Click += (s, e) =>
                    {
                        difficulteActive = diff;
                        foreach (var b in boutonsDiff)
                        {
                            bool estActif = b.Text == diff.Titre;
                            b.BackColor = estActif ? diff.CouleurBadge : Color.FromArgb(35, 38, 48);
                            b.FlatAppearance.BorderSize = estActif ? 2 : 1;
                            b.FlatAppearance.BorderColor = estActif ? Color.White : Color.Gray;
                        }
                    };
                    boutonsDiff.Add(btnD);
                    pnlDiff.Controls.Add(btnD);
                    bx += 172;
                }

                // Onglets de Catégories
                Panel pnlOnglets = new Panel
                {
                    Location = new Point(20, 115),
                    Size = new Size(825, 38),
                    BackColor = Color.Transparent
                };

                // Conteneur de Boss
                FlowLayoutPanel flowBoss = new FlowLayoutPanel
                {
                    Location = new Point(20, 160),
                    Size = new Size(825, 460),
                    AutoScroll = true,
                    BackColor = Color.FromArgb(24, 26, 34),
                    Padding = new Padding(10)
                };

                string categorieActive = "Regionaux";

                Action rafraichirListe = () =>
                {
                    flowBoss.SuspendLayout();
                    flowBoss.Controls.Clear();

                    if (categorieActive == "Donjons")
                    {
                        // 4 Donjons
                        flowBoss.Controls.Add(CreerCarteDonjon("🌲 Forêt des Murmures", "Expédition Bois Sauvages • Sangliers & Loups", 1, Color.FromArgb(39, 174, 96), () =>
                        {
                            dlg.Close();
                            LancerExpeditionDonjon("donjon_foret", difficulteActive);
                        }));

                        flowBoss.Controls.Add(CreerCarteDonjon("💀 Catacombes Oubliées", "Cryptes Funèbres • Squelettes & Goules", 3, Color.FromArgb(142, 68, 173), () =>
                        {
                            dlg.Close();
                            LancerExpeditionDonjon("donjon_catacombes", difficulteActive);
                        }));

                        flowBoss.Controls.Add(CreerCarteDonjon("🌋 Faille du Volcan", "Cratère de Magma • Drakes & Colosses", 7, Color.FromArgb(211, 84, 0), () =>
                        {
                            dlg.Close();
                            LancerExpeditionDonjon("donjon_volcan", difficulteActive);
                        }));

                        flowBoss.Controls.Add(CreerCarteDonjon("⭐ Sanctuaire Cosmique", "Haute Chambre Stellaire • Dévoreur Stellaire", 11, Color.FromArgb(41, 128, 185), () =>
                        {
                            dlg.Close();
                            LancerExpeditionDonjon("donjon_sanctuaire", difficulteActive);
                        }));
                    }
                    else
                    {
                        List<EntreeBoss2D> liste = categorieActive switch
                        {
                            "Regionaux" => CatalogueBoss2D.BossRegionaux,
                            "Mythiques" => CatalogueBoss2D.BossMythiques,
                            "Cosmiques" => CatalogueBoss2D.BossCosmiques,
                            _ => CatalogueBoss2D.BossRegionaux
                        };

                        foreach (var b in liste)
                        {
                            flowBoss.Controls.Add(CreerCarteBoss(b, dlg));
                        }
                    }

                    flowBoss.ResumeLayout();
                };

                // Onglets
                string[] nomsOnglets = { "🌲 Donjons (4)", "👑 Boss Régionaux (10)", "🔥 Boss Mythiques (10)", "🌌 Boss Cosmiques (10)" };
                string[] clefsOnglets = { "Donjons", "Regionaux", "Mythiques", "Cosmiques" };
                List<Button> boutonsOnglets = new List<Button>();

                for (int i = 0; i < nomsOnglets.Length; i++)
                {
                    string clef = clefsOnglets[i];
                    Button btnOnglet = new Button
                    {
                        Text = nomsOnglets[i],
                        Location = new Point(i * 206, 0),
                        Size = new Size(200, 34),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = categorieActive == clef ? Color.FromArgb(52, 73, 94) : Color.FromArgb(28, 30, 38),
                        ForeColor = Color.White,
                        Font = fontGras,
                        Cursor = Cursors.Hand
                    };
                    btnOnglet.FlatAppearance.BorderSize = 0;
                    btnOnglet.Click += (s, e) =>
                    {
                        categorieActive = clef;
                        foreach (var bo in boutonsOnglets)
                            bo.BackColor = bo == btnOnglet ? Color.FromArgb(52, 73, 94) : Color.FromArgb(28, 30, 38);
                        rafraichirListe();
                    };
                    boutonsOnglets.Add(btnOnglet);
                    pnlOnglets.Controls.Add(btnOnglet);
                }

                // Clic sur difficulté rafraîchit la liste avec les stats recalculées
                foreach (var b in boutonsDiff)
                {
                    b.Click += (s, e) => rafraichirListe();
                }

                dlg.Controls.AddRange(new Control[] { lblTitre, btnCodexDrops, lblSousTitre, lblDiff, pnlDiff, pnlOnglets, flowBoss });
                rafraichirListe();
                dlg.ShowDialog(this);
            }
            entrees.ReinitialiserToutesTouches();
            boucleJeu.Start();
        }

        private Control CreerCarteBoss(EntreeBoss2D entree, Form parentDialog)
        {
            Panel card = new Panel
            {
                Size = new Size(392, 105),
                BackColor = Color.FromArgb(32, 35, 46),
                Margin = new Padding(4, 4, 4, 8)
            };

            Monstre proto = entree.Fabrique();
            proto.NiveauRecommande = entree.NiveauConseille;
            int pvAffiche = (int)(proto.PVMax * difficulteActive.MultiplicateurPV);
            int atkAffiche = (int)(proto.Attaque * difficulteActive.MultiplicateurAttaque);
            int orAffiche = (int)(proto.GainOr * difficulteActive.MultiplicateurRecompenses);
            int xpAffiche = (int)(proto.GainXP * difficulteActive.MultiplicateurRecompenses);

            Label lblNom = new Label
            {
                Text = $"{entree.Icone} {entree.Nom}",
                Font = fontGras,
                ForeColor = entree.CouleurTheme,
                Location = new Point(10, 8),
                AutoSize = true
            };

            Label lblDesc = new Label
            {
                Text = $"{entree.SousTitre} (Niv. {entree.NiveauConseille}+)\nPV: {pvAffiche}  •  Atk: {atkAffiche}  •  +{orAffiche} 🪙, +{xpAffiche} XP",
                Font = fontPetit,
                ForeColor = Color.LightGray,
                Location = new Point(10, 30),
                Size = new Size(260, 36)
            };

            Label lblDiffBadge = new Label
            {
                Text = $"[{difficulteActive.Titre}] {(difficulteActive.EstInfernal ? "🔥 ENRAGÉ !" : "")}",
                Font = fontPetit,
                ForeColor = difficulteActive.CouleurBadge,
                Location = new Point(10, 75),
                AutoSize = true
            };

            Button btnFight = new Button
            {
                Text = "⚔️ COMBAT",
                Location = new Point(275, 10),
                Size = new Size(105, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = entree.CouleurTheme,
                ForeColor = Color.White,
                Font = fontGras,
                Cursor = Cursors.Hand
            };
            btnFight.FlatAppearance.BorderSize = 0;
            btnFight.Click += (s, e) =>
            {
                parentDialog.Close();
                LancerCombatBoss2D(proto, difficulteActive);
            };

            Button btnDrops = new Button
            {
                Text = "👁️ Butins",
                Location = new Point(275, 56),
                Size = new Size(105, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.Gold,
                Font = fontPetit,
                Cursor = Cursors.Hand
            };
            btnDrops.FlatAppearance.BorderSize = 1;
            btnDrops.FlatAppearance.BorderColor = Color.FromArgb(70, 80, 110);
            btnDrops.Click += (s, e) => OuvrirCodexDropsBoss(entree.Nom);

            card.Controls.AddRange(new Control[] { lblNom, lblDesc, lblDiffBadge, btnFight, btnDrops });
            return card;
        }

        private Control CreerCarteDonjon(string titre, string desc, int niv, Color coul, Action onLancer)
        {
            Panel card = new Panel
            {
                Size = new Size(795, 80),
                BackColor = Color.FromArgb(32, 35, 46),
                Margin = new Padding(4, 4, 4, 8)
            };

            Label lblTitre = new Label
            {
                Text = titre,
                Font = fontGras,
                ForeColor = coul,
                Location = new Point(14, 10),
                AutoSize = true
            };

            Label lblDesc = new Label
            {
                Text = $"{desc} • Recommandé Niv. {niv}+ • Difficulté : {difficulteActive.Titre}",
                Font = fontPetit,
                ForeColor = Color.LightGray,
                Location = new Point(14, 34),
                AutoSize = true
            };

            Button btnDropsDonjon = new Button
            {
                Text = "👁️ Butins & Boss",
                Location = new Point(420, 16),
                Size = new Size(130, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.Gold,
                Font = fontPetit,
                Cursor = Cursors.Hand
            };
            btnDropsDonjon.FlatAppearance.BorderSize = 1;
            btnDropsDonjon.FlatAppearance.BorderColor = Color.FromArgb(70, 80, 110);
            btnDropsDonjon.Click += (s, e) => OuvrirCodexDropsBoss(titre);

            Button btnGo = new Button
            {
                Text = "⚔️ ENTRER DANS LE DONJON",
                Location = new Point(560, 16),
                Size = new Size(215, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = coul,
                ForeColor = Color.White,
                Font = fontGras,
                Cursor = Cursors.Hand
            };
            btnGo.FlatAppearance.BorderSize = 0;
            btnGo.Click += (s, e) => onLancer();

            card.Controls.AddRange(new Control[] { lblTitre, lblDesc, btnDropsDonjon, btnGo });
            return card;
        }

        public void LancerExpeditionDonjon(string identifiantOuNom, DifficulteBoss2D difficulte)
        {
            string id = identifiantOuNom.ToLowerInvariant();
            List<Monstre> monstres;
            Monstre boss;
            string nomDonjon;
            int etage;

            if (id.Contains("foret"))
            {
                nomDonjon = "Forêt des Murmures";
                etage = 1;
                monstres = new List<Monstre> {
                    new Monstre("Sanglier Enragé", (int)(280 * difficulte.MultiplicateurPV), (int)(16 * difficulte.MultiplicateurAttaque), 6, 65, 30),
                    new Monstre("Gobelin Maraudeur", (int)(340 * difficulte.MultiplicateurPV), (int)(18 * difficulte.MultiplicateurAttaque), 7, 85, 40),
                    new Monstre("Loup Alpha", (int)(420 * difficulte.MultiplicateurPV), (int)(22 * difficulte.MultiplicateurAttaque), 9, 110, 50)
                };
                boss = new Monstre($"Grok le Brise-Crâne [{difficulte.Titre}]", (int)(2600 * difficulte.MultiplicateurPV), (int)(32 * difficulte.MultiplicateurAttaque), 16, (int)(450 * difficulte.MultiplicateurRecompenses), (int)(250 * difficulte.MultiplicateurRecompenses), true, "GROK ÉCRASE LES INTRUS !", "Feu");
            }
            else if (id.Contains("catacombe"))
            {
                nomDonjon = "Catacombes Oubliées";
                etage = 3;
                monstres = new List<Monstre> {
                    new Monstre("Squelette Gardien", (int)(520 * difficulte.MultiplicateurPV), (int)(24 * difficulte.MultiplicateurAttaque), 12, 140, 60),
                    new Monstre("Goule Affamée", (int)(680 * difficulte.MultiplicateurPV), (int)(28 * difficulte.MultiplicateurAttaque), 14, 170, 80),
                    new Monstre("Spectre Hurlant", (int)(820 * difficulte.MultiplicateurPV), (int)(30 * difficulte.MultiplicateurAttaque), 15, 210, 95)
                };
                boss = new Monstre($"Malakor l'Ancien [{difficulte.Titre}]", (int)(5600 * difficulte.MultiplicateurPV), (int)(44 * difficulte.MultiplicateurAttaque), 20, (int)(850 * difficulte.MultiplicateurRecompenses), (int)(480 * difficulte.MultiplicateurRecompenses), true, "TON SANG NOURRIRA LES OMBRES !", "Lumière");
            }
            else if (id.Contains("volcan"))
            {
                nomDonjon = "Volcan de Drak'Thar";
                etage = 7;
                monstres = new List<Monstre> {
                    new Monstre("Drake de Magma", (int)(1200 * difficulte.MultiplicateurPV), (int)(38 * difficulte.MultiplicateurAttaque), 20, 320, 160),
                    new Monstre("Colosse de Lave", (int)(1600 * difficulte.MultiplicateurPV), (int)(42 * difficulte.MultiplicateurAttaque), 25, 420, 220)
                };
                boss = new Monstre($"Ignis Dragon Suprême [{difficulte.Titre}]", (int)(13500 * difficulte.MultiplicateurPV), (int)(70 * difficulte.MultiplicateurAttaque), 32, (int)(1800 * difficulte.MultiplicateurRecompenses), (int)(950 * difficulte.MultiplicateurRecompenses), true, "BRÛLE DANS LE BRASIER DE VALDORAK !", "Givre");
            }
            else
            {
                nomDonjon = "Sanctuaire Cosmique";
                etage = 11;
                monstres = new List<Monstre> {
                    new Monstre("Sentinelle Stellaire", (int)(2200 * difficulte.MultiplicateurPV), (int)(55 * difficulte.MultiplicateurAttaque), 30, 650, 320)
                };
                boss = new Monstre($"Xanthos du Néant [{difficulte.Titre}]", (int)(26000 * difficulte.MultiplicateurPV), (int)(95 * difficulte.MultiplicateurAttaque), 45, (int)(3500 * difficulte.MultiplicateurRecompenses), (int)(1800 * difficulte.MultiplicateurRecompenses), true, "LE NÉANT ABSORBERA VOTRE LUMIÈRE !", "Cosmique");
            }

            monde.DifficulteActive = difficulte;
            monde.ChargerDonjon(nomDonjon, monstres, boss, hero.Niveau, etage);
            joueur2D.Position = new Vector2(300, 1000);
            camera.Position = joueur2D.Position;
            camera.Cible = joueur2D.Position;
            monde.AjouterTexteFlottant(joueur2D.Position, $"⚔️ {nomDonjon} [{difficulte.Titre}] !", Color.Gold, true);
        }

        public void OuvrirCodexDropsBoss(string? bossPrefere = null)
        {
            boucleJeu.Stop();
            using (var dlg = new FormCodexDropsBoss(hero, (fiche, diff) =>
            {
                if (fiche.Id == "tour_astrale")
                {
                    OuvrirTourAstrale();
                }
                else if (fiche.Categorie == "Donjon")
                {
                    LancerExpeditionDonjon(fiche.Id, diff);
                }
                else if (fiche.Fabrique != null)
                {
                    LancerCombatBoss2D(fiche.Fabrique(), diff);
                }
            }, bossPrefere))
            {
                dlg.ShowDialog(this);
            }
            entrees.ReinitialiserToutesTouches();
            boucleJeu.Start();
        }

        private void LancerCombatBoss2D(Monstre bossOriginal, DifficulteBoss2D difficulte)
        {
            monde.ChargerAreneBoss(bossOriginal, difficulte, hero.Niveau);
            joueur2D.Position = new Vector2(300, 900);
            camera.Position = joueur2D.Position;
            camera.Cible = joueur2D.Position;

            AudioSynthetiseur.SonCriBoss();
            camera.DeclencherSecousse(12f, 0.4f);

            monde.AjouterTexteFlottant(joueur2D.Position, $"⚔️ Combat de Boss : {bossOriginal.Nom} [{difficulte.Titre}] !", Color.Gold, true);
            if (!string.IsNullOrEmpty(bossOriginal.CriDeGuerre))
            {
                monde.AjouterTexteFlottant(new Vector2(1650, 850), $"📢 \"{bossOriginal.CriDeGuerre}\"", Color.Red, true);
            }
        }

        private void OuvrirTourAstrale()
        {
            boucleJeu.Stop();
            using (Form dlg = new Form())
            {
                dlg.Text = "⭐ Tour Astrale Suprême — Ascension Infinie 2D";
                dlg.Size = new Size(620, 520);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(16, 18, 26);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;

                hero.EtageTourActuel = Math.Max(1, hero.EtageTourActuel);
                int savedFloor = hero.EtageTourActuel;
                int maxFloor = Math.Max(1, hero.EtageTourAstraleMax);

                Label lblTitre = new Label
                {
                    Text = "⭐ TOUR ASTRALE INFINIE (MODE ROGUELITE 2D)",
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.Cyan,
                    Location = new Point(25, 18),
                    AutoSize = true
                };

                Label lblStats = new Label
                {
                    Text = $"🏆 Record Ultime : Étage {maxFloor}    •    💾 Checkpoint Sauvegardé : Étage {savedFloor}\n💎 Éclats Astraux : {hero.EclatsAstraux}   •   Chaque étage purifié sauvegarde votre avancée !",
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.LightGray,
                    Location = new Point(25, 50),
                    AutoSize = true
                };

                // Bouton 1 : Reprendre au checkpoint sauvegardé
                Button btnContinuer = new Button
                {
                    Text = $"▶️ REPRENDRE À L'ÉTAGE {savedFloor}\n(Continuer votre ascension là où vous vous êtes arrêté)",
                    Location = new Point(25, 105),
                    Size = new Size(550, 65),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = fontGras,
                    Cursor = Cursors.Hand
                };
                btnContinuer.FlatAppearance.BorderSize = 0;
                btnContinuer.Click += (s, e) =>
                {
                    dlg.Close();
                    LancerEtageTour(savedFloor);
                };

                // Bouton 2 : Reprendre au dernier Boss validé (palier de 5)
                int dernierPalierBoss = Math.Max(1, (savedFloor / 5) * 5);
                Button btnPalier = new Button
                {
                    Text = $"🚩 PALIER DE BOSS : ÉTAGE {dernierPalierBoss}\n(Point d'ancrage stratégique avec Boss Majeur)",
                    Location = new Point(25, 180),
                    Size = new Size(550, 60),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(142, 68, 173),
                    ForeColor = Color.White,
                    Font = fontGras,
                    Cursor = Cursors.Hand
                };
                btnPalier.FlatAppearance.BorderSize = 0;
                btnPalier.Click += (s, e) =>
                {
                    dlg.Close();
                    LancerEtageTour(dernierPalierBoss);
                };

                // Bouton 3 : Recommencer Étage 1
                Button btnReset = new Button
                {
                    Text = "🔄 RECOMMENCER DEPUIS L'ÉTAGE 1\n(Pour accumuler or, gemmes et loots supplémentaires dès le départ)",
                    Location = new Point(25, 250),
                    Size = new Size(550, 55),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(50, 55, 68),
                    ForeColor = Color.White,
                    Font = fontGras,
                    Cursor = Cursors.Hand
                };
                btnReset.FlatAppearance.BorderSize = 0;
                btnReset.Click += (s, e) =>
                {
                    dlg.Close();
                    LancerEtageTour(1);
                };

                // Bouton 4 : Consulter les drops et loots de la Tour
                Button btnDropsTour = new Button
                {
                    Text = "📖 BUTINS & DROPS DE LA TOUR ASTRALE [B]",
                    Location = new Point(25, 318),
                    Size = new Size(550, 48),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(108, 92, 231),
                    ForeColor = Color.White,
                    Font = fontGras,
                    Cursor = Cursors.Hand
                };
                btnDropsTour.FlatAppearance.BorderSize = 0;
                btnDropsTour.Click += (s, e) => OuvrirCodexDropsBoss("Tour Astrale");

                // Bouton 5 : Fermer
                Button btnRetour = new Button
                {
                    Text = "↩️ Retourner au Village",
                    Location = new Point(25, 378),
                    Size = new Size(550, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(30, 32, 40),
                    ForeColor = Color.LightGray,
                    Font = fontTexte,
                    Cursor = Cursors.Hand
                };
                btnRetour.Click += (s, e) => dlg.Close();

                dlg.Controls.AddRange(new Control[] { lblTitre, lblStats, btnContinuer, btnPalier, btnReset, btnDropsTour, btnRetour });
                dlg.ShowDialog(this);
            }
            entrees.ReinitialiserToutesTouches();
            boucleJeu.Start();
        }

        private void LancerEtageTour(int etage)
        {
            hero.EtageTourActuel = etage;
            SauvegarderPartie();

            CatalogueBoss2D.GenererEnnemisTour(etage, out Monstre boss, out List<Monstre> adds);
            monde.ChargerEtageTour(etage, boss, adds, hero.Niveau);

            joueur2D.Position = new Vector2(900, 1500);
            camera.Position = joueur2D.Position;
            camera.Cible = joueur2D.Position;

            AudioSynthetiseur.SonCritique();
            monde.AjouterTexteFlottant(joueur2D.Position, $"⭐ Tour Astrale — Étage {etage} !", Color.Cyan, true);
        }

        private void PasserEtageSuivantTour()
        {
            hero.EtageTourActuel++;

            if (hero.EtageTourActuel > hero.EtageTourAstraleMax)
            {
                hero.EtageTourAstraleMax = hero.EtageTourActuel;
                hero.EtageTourRecord = hero.EtageTourActuel;
                hero.PierresDeForge += 2;
                hero.EclatsAstraux += 5;
            }

            // Sauvegarde automatique immédiate de la progression
            SauvegarderPartie();

            // Soin de palier
            int soinPV = (int)(hero.PVMaxTotal * 0.35f);
            int soinMana = (int)(hero.ManaMaxTotal * 0.40f);
            hero.Soigner(soinPV);
            hero.RestaurerMana(soinMana);

            camera.DeclencherSecousse(10f, 0.35f);
            AudioSynthetiseur.SonCritique();

            CatalogueBoss2D.GenererEnnemisTour(hero.EtageTourActuel, out Monstre boss, out List<Monstre> adds);
            monde.ChargerEtageTour(hero.EtageTourActuel, boss, adds, hero.Niveau);

            joueur2D.Position = new Vector2(900, 1500);
            camera.Position = joueur2D.Position;
            camera.Cible = joueur2D.Position;

            monde.AjouterTexteFlottant(joueur2D.Position, $"⭐ ÉTAGE {hero.EtageTourActuel} ATTEINT ! (Progression Sauvegardée)", Color.Gold, true);
            monde.AjouterTexteFlottant(joueur2D.Position + new Vector2(0, -35), $"+{soinPV} PV & +{soinMana} Mana régénérés !", Color.FromArgb(46, 204, 113), true);
        }

        private void OuvrirConfigurationTouches()
        {
            bool etaitEnPause = jeuEnPause;
            boucleJeu.Stop();

            using (Form dlg = new Form())
            {
                dlg.Text = "Configuration des commandes";
                dlg.Size = new Size(790, 590);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(20, 22, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.KeyPreview = true;

                Label titre = new Label
                {
                    Text = "⌨️ RACCOURCIS PERSONNALISÉS",
                    Location = new Point(20, 16),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.Gold
                };
                Label statut = new Label
                {
                    Text = "Clique sur Modifier, puis appuie sur une touche libre.",
                    Location = new Point(20, 48),
                    Size = new Size(730, 24),
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.LightGray
                };
                Panel panneau = new Panel
                {
                    Location = new Point(20, 80),
                    Size = new Size(735, 420),
                    AutoScroll = true,
                    BackColor = Color.FromArgb(25, 28, 36)
                };

                (string Id, string Nom)[] actions =
                {
                    ("Haut", "Déplacement haut"), ("Bas", "Déplacement bas"),
                    ("Gauche", "Déplacement gauche"), ("Droite", "Déplacement droite"),
                    ("Esquive", "Esquive / dash"), ("Interaction", "Interagir"),
                    ("Sort1", "Compétence 1"), ("Sort2", "Compétence 2"),
                    ("Ultime", "Ultime"), ("PotionSoin", "Potion de soin"),
                    ("PotionMana", "Potion de mana"), ("Sac", "Sac / inventaire"),
                    ("Quetes", "Journal de quêtes"), ("Forge", "Forge"),
                    ("Artisanat", "Atelier d'artisanat"), ("Pause", "Pause"),
                    ("Sauvegarde", "Sauvegarde rapide")
                };
                Dictionary<string, Button> boutonsTouches = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);
                string? actionEnAttente = null;

                for (int i = 0; i < actions.Length; i++)
                {
                    string actionId = actions[i].Id;
                    int colonne = i / 9;
                    int ligne = i % 9;
                    int x = 12 + colonne * 355;
                    int y = 10 + ligne * 44;
                    Label libelle = new Label
                    {
                        Text = actions[i].Nom,
                        Location = new Point(x, y + 7),
                        Size = new Size(220, 24),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 9f)
                    };
                    Button bouton = new Button
                    {
                        Text = entrees.Touche(actionId).ToString(),
                        Location = new Point(x + 222, y),
                        Size = new Size(112, 34),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(41, 128, 185),
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                    };
                    bouton.Click += (s, e) =>
                    {
                        actionEnAttente = actionId;
                        statut.Text = $"Nouvelle touche pour « {actions.First(a => a.Id == actionId).Nom} » : appuie maintenant.";
                        bouton.Text = "Appuie...";
                        dlg.Activate();
                    };
                    boutonsTouches[actionId] = bouton;
                    panneau.Controls.AddRange(new Control[] { libelle, bouton });
                }

                dlg.KeyDown += (s, e) =>
                {
                    if (actionEnAttente == null) return;
                    if (e.KeyCode is Keys.H or Keys.F1 or Keys.F6 or Keys.F7 or Keys.Tab)
                    {
                        statut.Text = "Cette touche est réservée à l'aide, aux sauvegardes ou à la navigation.";
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        return;
                    }
                    if (entrees.EstToucheAffectee(actionEnAttente, e.KeyCode))
                    {
                        statut.Text = "Cette touche est déjà attribuée à une autre action.";
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        return;
                    }

                    string actionModifiee = actionEnAttente;
                    if (entrees.DefinirTouche(actionModifiee, e.KeyCode))
                    {
                        if (boutonsTouches.TryGetValue(actionModifiee, out Button? boutonAction))
                            boutonAction.Text = entrees.Touche(actionModifiee).ToString();
                        SauvegarderConfigurationTouches();
                        statut.Text = "Raccourci enregistré.";
                        actionEnAttente = null;
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                };

                Button fermer = new Button
                {
                    Text = "Fermer",
                    Location = new Point(600, 510),
                    Size = new Size(155, 36),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White
                };
                fermer.Click += (s, e) => dlg.Close();
                dlg.Controls.AddRange(new Control[] { titre, statut, panneau, fermer });

                try
                {
                    dlg.ShowDialog(this);
                }
                finally
                {
                    jeuEnPause = etaitEnPause;
                    entrees.ReinitialiserToutesTouches();
                    boucleJeu.Start();
                    Invalidate();
                }
            }
        }

        private void OuvrirGestionSauvegardes()
        {
            bool etaitEnPause = jeuEnPause;
            boucleJeu.Stop();

            using (Form dlg = new Form())
            {
                dlg.Text = "Gestion des sauvegardes";
                dlg.Size = new Size(560, 330);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.BackColor = Color.FromArgb(20, 22, 28);
                dlg.ForeColor = Color.White;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                Label titre = new Label
                {
                    Text = "💾 EMPLACEMENTS DE SAUVEGARDE",
                    Location = new Point(20, 18),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.Gold
                };
                ComboBox choixEmplacement = new ComboBox
                {
                    Location = new Point(20, 55),
                    Size = new Size(500, 30),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(32, 36, 48),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10f)
                };
                for (int i = 1; i <= NombreEmplacementsSauvegarde; i++)
                    choixEmplacement.Items.Add($"Emplacement {i}");
                choixEmplacement.SelectedIndex = emplacementSauvegardeActif - 1;

                Label resume = new Label
                {
                    Location = new Point(20, 96),
                    Size = new Size(500, 50),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.LightGray
                };

                string ResumeEmplacement(int index)
                {
                    string chemin = CheminEmplacementSauvegarde(index);
                    if (!File.Exists(chemin))
                        return File.Exists(chemin + ".bak") ? "Fichier principal absent; une copie de secours est disponible." : "Emplacement vide.";

                    try
                    {
                        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(chemin));
                        string nom = document.RootElement.TryGetProperty("Nom", out JsonElement nomElement) ? nomElement.GetString() ?? "Héros" : "Héros";
                        int niveau = document.RootElement.TryGetProperty("Niveau", out JsonElement niveauElement) ? niveauElement.GetInt32() : 1;
                        return $"{nom} • Niveau {niveau} • Modifiée le {File.GetLastWriteTime(chemin):dd/MM/yyyy HH:mm}";
                    }
                    catch
                    {
                        return File.Exists(chemin + ".bak") ? "Sauvegarde endommagée; une copie de secours sera tentée." : "Sauvegarde illisible, aucune copie de secours.";
                    }
                }

                Action actualiserResume = () =>
                {
                    if (choixEmplacement.SelectedIndex >= 0)
                        resume.Text = ResumeEmplacement(choixEmplacement.SelectedIndex + 1);
                };

                Button btnSauverEmplacement = new Button
                {
                    Text = "Sauvegarder ici",
                    Location = new Point(20, 165),
                    Size = new Size(155, 44),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                Button btnChargerEmplacement = new Button
                {
                    Text = "Charger",
                    Location = new Point(185, 165),
                    Size = new Size(155, 44),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                Button btnFermerSauvegardes = new Button
                {
                    Text = "Fermer",
                    Location = new Point(365, 165),
                    Size = new Size(155, 44),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };

                choixEmplacement.SelectedIndexChanged += (s, e) => actualiserResume();
                btnSauverEmplacement.Click += (s, e) =>
                {
                    int emplacement = choixEmplacement.SelectedIndex + 1;
                    string chemin = CheminEmplacementSauvegarde(emplacement);
                    if (File.Exists(chemin) && AfficherMessageAvecPause(dlg, "Remplacer cette sauvegarde ? Sa copie précédente sera conservée.", "Confirmer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    emplacementSauvegardeActif = emplacement;
                    if (SauvegarderPartie())
                    {
                        actualiserResume();
                        AfficherMessageAvecPause(dlg, $"Partie enregistrée dans l'emplacement {emplacement}.", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };
                btnChargerEmplacement.Click += (s, e) =>
                {
                    int emplacement = choixEmplacement.SelectedIndex + 1;
                    Joueur? sauvegarde = ChargerDepuisFichierAvecRecuperation(CheminEmplacementSauvegarde(emplacement));
                    if (sauvegarde == null)
                    {
                        AfficherMessageAvecPause(dlg, "Cet emplacement ne contient aucune sauvegarde valide.", "Chargement impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        actualiserResume();
                        return;
                    }

                    emplacementSauvegardeActif = emplacement;
                    hero = sauvegarde;
                    InitialiserQuetesSiNecessaire();
                    monde.ChargerVillage();
                    joueur2D = new Joueur2D(hero, monde.PositionDepartVillage);
                    camera.Position = joueur2D.Position;
                    camera.Cible = joueur2D.Position;
                    pvAffichage = hero.PVActuels;
                    pvGhost = hero.PVActuels;
                    dlg.Close();
                };
                btnFermerSauvegardes.Click += (s, e) => dlg.Close();

                dlg.Controls.AddRange(new Control[] { titre, choixEmplacement, resume, btnSauverEmplacement, btnChargerEmplacement, btnFermerSauvegardes });
                actualiserResume();
                try
                {
                    dlg.ShowDialog(this);
                }
                finally
                {
                    jeuEnPause = etaitEnPause;
                    entrees.ReinitialiserToutesTouches();
                    boucleJeu.Start();
                    Invalidate();
                }
            }
        }

        private bool SauvegarderPartie(bool afficherErreur = true)
        {
            string chemin = CheminEmplacementSauvegarde(emplacementSauvegardeActif);
            string temporaire = chemin + ".tmp";
            try
            {
                string backup = chemin + ".bak";
                Directory.CreateDirectory(RepertoireSauvegardes);

                hero.TalentsNiveaux ??= new Dictionary<string, int>();
                string json = JsonSerializer.Serialize(hero, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(temporaire, json);
                if (File.Exists(chemin))
                {
                    bool principaleValide = false;
                    try
                    {
                        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(chemin));
                        principaleValide = document.RootElement.TryGetProperty("Nom", out _);
                    }
                    catch (Exception ex) { JournalErreurs.Enregistrer(ex, "Validation de la sauvegarde précédente avant création du backup"); }

                    if (principaleValide)
                        File.Copy(chemin, backup, true);
                }
                File.Move(temporaire, chemin, true);
                tempsAutosauvegarde = 0f;
                return true;
            }
            catch (Exception ex)
            {
                if (afficherErreur)
                    AfficherMessageAvecPause(this, $"Échec de la sauvegarde : {ex.Message}", "Sauvegarde", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(temporaire)) File.Delete(temporaire);
                }
                catch (Exception ex) { JournalErreurs.Enregistrer(ex, "Nettoyage du fichier temporaire de sauvegarde"); }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            boucleJeu?.Dispose();
            AudioSynthetiseur.Liberer();
            LibererRessourcesMenuTitre();
        }
    }
}
