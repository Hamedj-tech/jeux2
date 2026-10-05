using System;
using System.Drawing;
using System.Windows.Forms;

namespace JeuxRPG
{
    public partial class FormJeu2D
    {
        private Action ConstruireVueArbreTalents(Panel pnlVueTalents, Joueur heroJoueur, Action rafraichirInventaireEtStats)
        {
            pnlVueTalents.Controls.Clear();
            pnlVueTalents.BackColor = Color.FromArgb(16, 18, 24);

            // ==============================================================
            // 1. EN-TÊTE DE L'ARBRE DE TALENTS
            // ==============================================================
            Panel pnlHeader = new Panel
            {
                Location = new Point(10, 8),
                Size = new Size(764, 48),
                BackColor = Color.FromArgb(22, 26, 36)
            };

            Label lblTitre = new Label
            {
                Text = "👑 ARBRE DE TALENTS & MAÎTRISE DU HÉROS",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15),
                Location = new Point(10, 6),
                AutoSize = true
            };

            Label lblSousTitre = new Label
            {
                Text = "Spécialisez votre champion dans les arts du combat, du rempart et des arcanes primordiales.",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Regular),
                ForeColor = Color.LightGray,
                Location = new Point(11, 28),
                AutoSize = true
            };

            Label lblPoints = new Label
            {
                Text = $"⭐ Points de maîtrise disponibles : {heroJoueur.PointsTalents}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Location = new Point(480, 12),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitre, lblSousTitre, lblPoints });
            pnlVueTalents.Controls.Add(pnlHeader);

            // ==============================================================
            // 2. CONTENEURS DES 3 BRANCHES MAJEURES
            // ==============================================================
            int yBranches = 62;
            int hautBranches = 336;

            Panel pnlBrancheOffensive = CreerPanneauBranche("⚔️ BRANCHE ASSAUT & PUISSANCE", Color.FromArgb(231, 76, 60), new Point(10, yBranches), 244, hautBranches);
            Panel pnlBrancheDefensive = CreerPanneauBranche("🛡️ BRANCHE REMPART & BASTION", Color.FromArgb(52, 152, 219), new Point(262, yBranches), 244, hautBranches);
            Panel pnlBrancheMagique = CreerPanneauBranche("🔮 BRANCHE ARCANES & ESPRIT", Color.FromArgb(155, 89, 182), new Point(514, yBranches), 260, hautBranches);

            pnlVueTalents.Controls.AddRange(new Control[] { pnlBrancheOffensive, pnlBrancheDefensive, pnlBrancheMagique });

            // ==============================================================
            // 3. BANDEAU DE SYNTHÈSE DES BONUS ACTIFS
            // ==============================================================
            Panel pnlSynthese = new Panel
            {
                Location = new Point(10, 404),
                Size = new Size(764, 52),
                BackColor = Color.FromArgb(20, 24, 32)
            };

            Label lblSyntheseTitre = new Label
            {
                Text = "📊 BONUS CUMULÉS DE L'ARBRE :",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15),
                Location = new Point(8, 6),
                AutoSize = true
            };

            Label lblSyntheseValeurs = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(8, 26),
                AutoSize = true
            };

            pnlSynthese.Controls.AddRange(new Control[] { lblSyntheseTitre, lblSyntheseValeurs });
            pnlVueTalents.Controls.Add(pnlSynthese);

            // Action de rafraîchissement complet
            Action rafraichirVisuel = () =>
            {
                lblPoints.Text = $"⭐ Points disponibles : {heroJoueur.PointsTalents}";
                lblPoints.ForeColor = heroJoueur.PointsTalents > 0 ? Color.FromArgb(46, 204, 113) : Color.DarkGray;

                // Actualiser les cartes de talents
                ActualiserCartesBranche(pnlBrancheOffensive, heroJoueur, rafraichirInventaireEtStats);
                ActualiserCartesBranche(pnlBrancheDefensive, heroJoueur, rafraichirInventaireEtStats);
                ActualiserCartesBranche(pnlBrancheMagique, heroJoueur, rafraichirInventaireEtStats);

                // Synthèse
                int atkPct = heroJoueur.ObtenirBonusTalent("Force du Héros") * 6;
                int defPct = heroJoueur.ObtenirBonusTalent("Garde de Fer") * 5;
                int pvPct = heroJoueur.ObtenirBonusTalent("Résilience") * 10;
                int manaPct = heroJoueur.ObtenirBonusTalent("Arcane Furtive") * 8;
                int critPct = heroJoueur.ObtenirBonusTalent("Vitesse de l'Ombre") * 3;

                lblSyntheseValeurs.Text = $"⚔️ +{atkPct}% ATK   |   🛡️ +{defPct}% DEF   |   💖 +{pvPct}% PV Max   |   🔮 +{manaPct}% MANA   |   ⚡ +{critPct}% CRIT/ESQ";
            };

            // Remplissage initial des cartes
            PeuplerBrancheOffensive(pnlBrancheOffensive, heroJoueur, rafraichirVisuel, rafraichirInventaireEtStats);
            PeuplerBrancheDefensive(pnlBrancheDefensive, heroJoueur, rafraichirVisuel, rafraichirInventaireEtStats);
            PeuplerBrancheMagique(pnlBrancheMagique, heroJoueur, rafraichirVisuel, rafraichirInventaireEtStats);

            rafraichirVisuel();
            return rafraichirVisuel;
        }

        private static Panel CreerPanneauBranche(string titre, Color accent, Point loc, int largeur, int hauteur)
        {
            Panel pnl = new Panel
            {
                Location = loc,
                Size = new Size(largeur, hauteur),
                BackColor = Color.FromArgb(20, 23, 31),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitreBranche = new Label
            {
                Text = titre,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(6, 6),
                AutoSize = true
            };

            Panel separateur = new Panel
            {
                Location = new Point(6, 30),
                Size = new Size(largeur - 14, 2),
                BackColor = Color.FromArgb(40, accent.R, accent.G, accent.B)
            };

            pnl.Controls.AddRange(new Control[] { lblTitreBranche, separateur });
            return pnl;
        }

        private void PeuplerBrancheOffensive(Panel pnl, Joueur hero, Action rafraichirVisuel, Action rafraichirInventaireEtStats)
        {
            pnl.Controls.Add(CreerCarteTalent("Force du Héros", "⚔️ Force du Héros", "+6% d'Attaque par rang", Color.FromArgb(231, 76, 60), new Point(6, 38), hero, rafraichirVisuel, rafraichirInventaireEtStats));
            pnl.Controls.Add(CreerCarteTalent("Vitesse de l'Ombre", "⚡ Vitesse de l'Ombre", "+3% Critique & Esquive par rang", Color.FromArgb(230, 126, 34), new Point(6, 184), hero, rafraichirVisuel, rafraichirInventaireEtStats));
        }

        private void PeuplerBrancheDefensive(Panel pnl, Joueur hero, Action rafraichirVisuel, Action rafraichirInventaireEtStats)
        {
            pnl.Controls.Add(CreerCarteTalent("Garde de Fer", "🛡️ Garde de Fer", "+5% de Défense par rang", Color.FromArgb(52, 152, 219), new Point(6, 38), hero, rafraichirVisuel, rafraichirInventaireEtStats));
            pnl.Controls.Add(CreerCarteTalent("Résilience", "💖 Résilience Vitale", "+10% de PV Max par rang", Color.FromArgb(46, 204, 113), new Point(6, 184), hero, rafraichirVisuel, rafraichirInventaireEtStats));
        }

        private void PeuplerBrancheMagique(Panel pnl, Joueur hero, Action rafraichirVisuel, Action rafraichirInventaireEtStats)
        {
            pnl.Controls.Add(CreerCarteTalent("Arcane Furtive", "🔮 Arcane Furtive", "+8% de Mana Max par rang", Color.FromArgb(155, 89, 182), new Point(6, 38), hero, rafraichirVisuel, rafraichirInventaireEtStats));

            // Carte spéciale : Maîtrise Passive Inorganique de Classe
            Panel cartePassif = new Panel
            {
                Location = new Point(6, 184),
                Size = new Size(244, 138),
                BackColor = Color.FromArgb(26, 30, 42),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblNom = new Label
            {
                Text = $"👑 Maîtrise : {hero.Classe}",
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(8, 8),
                AutoSize = true
            };

            Label lblPassif = new Label
            {
                Text = $"Affinité naturelle :\n{hero.ObtenirDescriptionPassiveClasse()}",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Italic),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(8, 34),
                Size = new Size(226, 60)
            };

            Label lblActif = new Label
            {
                Text = "✓ Maîtrise innée active",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Location = new Point(8, 106),
                AutoSize = true
            };

            cartePassif.Controls.AddRange(new Control[] { lblNom, lblPassif, lblActif });
            pnl.Controls.Add(cartePassif);
        }

        private Panel CreerCarteTalent(string nomCle, string titre, string desc, Color accent, Point loc, Joueur hero, Action rafraichirVisuel, Action rafraichirInventaireEtStats)
        {
            Panel carte = new Panel
            {
                Tag = nomCle,
                Location = loc,
                Size = new Size(loc.X == 6 && loc.Y == 38 && loc.X + 244 <= 260 ? 230 : (loc.X == 6 ? 230 : 244), 138),
                BackColor = Color.FromArgb(26, 30, 42),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTitre = new Label
            {
                Name = "lblTitre",
                Text = titre,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(8, 8),
                AutoSize = true
            };

            Label lblJauge = new Label
            {
                Name = "lblJauge",
                Text = "Rang : [ ☆ ☆ ☆ ] (0/3)",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(8, 30),
                AutoSize = true
            };

            Label lblEffet = new Label
            {
                Name = "lblEffet",
                Text = desc,
                Font = new Font("Segoe UI", 8.2f, FontStyle.Regular),
                ForeColor = Color.FromArgb(189, 195, 199),
                Location = new Point(8, 52),
                Size = new Size(carte.Width - 16, 30)
            };

            Button btnInvestir = new Button
            {
                Name = "btnInvestir",
                Text = "✨ Investir (+1)",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(8, 92),
                Size = new Size(carte.Width - 16, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnInvestir.FlatAppearance.BorderSize = 0;

            btnInvestir.Click += (s, e) =>
            {
                if (hero.AmeliorerTalent(nomCle))
                {
                    AudioSynthetiseur.SonCritique();
                    monde.AjouterTexteFlottant(joueur2D.Position, $"🌟 {titre} +1 !", Color.FromArgb(241, 196, 15), true);
                    rafraichirVisuel();
                    rafraichirInventaireEtStats();
                }
            };

            carte.Controls.AddRange(new Control[] { lblTitre, lblJauge, lblEffet, btnInvestir });
            return carte;
        }

        private static void ActualiserCartesBranche(Panel pnlBranche, Joueur hero, Action rafraichirInventaireEtStats)
        {
            foreach (Control c in pnlBranche.Controls)
            {
                if (c is Panel carte && carte.Tag is string nomCle)
                {
                    int rang = hero.ObtenirBonusTalent(nomCle);
                    Label? lblJauge = carte.Controls["lblJauge"] as Label;
                    Label? lblEffet = carte.Controls["lblEffet"] as Label;
                    Button? btnInvestir = carte.Controls["btnInvestir"] as Button;

                    if (lblJauge != null)
                    {
                        string etoiles = rang switch
                        {
                            1 => "[ ★ ☆ ☆ ]",
                            2 => "[ ★ ★ ☆ ]",
                            3 => "[ ★ ★ ★ ]",
                            _ => "[ ☆ ☆ ☆ ]"
                        };
                        lblJauge.Text = $"Rang : {etoiles} ({rang}/3)";
                        lblJauge.ForeColor = rang >= 3 ? Color.FromArgb(46, 204, 113) : Color.Gold;
                    }

                    if (lblEffet != null)
                    {
                        string descBase = hero.ObtenirDescriptionTalent(nomCle);
                        lblEffet.Text = $"{descBase}\nBonus actuel : {ObtenirTexteBonusActuel(nomCle, rang)}";
                    }

                    if (btnInvestir != null)
                    {
                        if (rang >= 3)
                        {
                            btnInvestir.Text = "✔ RANG MAXIMUM";
                            btnInvestir.Enabled = false;
                            btnInvestir.BackColor = Color.FromArgb(39, 174, 96);
                            btnInvestir.ForeColor = Color.White;
                        }
                        else if (hero.PointsTalents > 0)
                        {
                            btnInvestir.Text = $"✨ Investir (+1 Rang)";
                            btnInvestir.Enabled = true;
                            btnInvestir.BackColor = Color.FromArgb(41, 128, 185);
                            btnInvestir.ForeColor = Color.White;
                        }
                        else
                        {
                            btnInvestir.Text = "🔒 Points requis (0 dispo)";
                            btnInvestir.Enabled = false;
                            btnInvestir.BackColor = Color.FromArgb(50, 55, 65);
                            btnInvestir.ForeColor = Color.DarkGray;
                        }
                    }
                }
            }
        }

        private static string ObtenirTexteBonusActuel(string nomCle, int rang) => nomCle switch
        {
            "Force du Héros" => $"+{rang * 6}% Attaque",
            "Garde de Fer" => $"+{rang * 5}% Défense",
            "Résilience" => $"+{rang * 10}% PV Max",
            "Arcane Furtive" => $"+{rang * 8}% Mana Max",
            "Vitesse de l'Ombre" => $"+{rang * 3}% Critique & Esquive",
            _ => "+0%"
        };
    }
}
