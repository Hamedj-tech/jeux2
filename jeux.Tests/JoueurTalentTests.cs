using JeuxRPG;
using System.Drawing;
using System.Windows.Forms;
using Xunit;

public class JoueurTalentTests
{
    [Fact]
    public void NouveauJoueurObtientDesPointsDeTalentEtPeutLesAmeliorer()
    {
        var joueur = new Joueur("Testeur", ClasseType.Guerrier);

        Assert.True(joueur.PointsTalents >= 1);
        int bonusAvant = joueur.ObtenirBonusTalent("Force du Héros");

        joueur.AmeliorerTalent("Force du Héros");

        Assert.Equal(bonusAvant + 1, joueur.ObtenirBonusTalent("Force du Héros"));
        Assert.True(joueur.PointsTalents >= 0);
    }

    [Fact]
    public void GuerrierObtientUnBonusDeClassePassifVisible()
    {
        var joueur = new Joueur("Champion", ClasseType.Guerrier);

        Assert.True(joueur.ObtenirBonusPassifClasse() > 0);
        Assert.Contains("Guerrier", joueur.ObtenirDescriptionPassiveClasse());
        Assert.Contains("guerre", joueur.ObtenirDescriptionPassiveClasse().ToLowerInvariant());
    }

    [Fact]
    public void BossChangeDePhaseSelonLePourcentageDeVie()
    {
        var monstre = new Monstre("Boss Test", 1000, 30, 10, 50, 20, true);
        var boss = new Monstre2D(monstre, new Vector2(0, 0));

        Assert.Equal(0, boss.ObtenirPhaseBoss(0.80f));
        Assert.Equal(1, boss.ObtenirPhaseBoss(0.60f));
        Assert.Equal(2, boss.ObtenirPhaseBoss(0.35f));
        Assert.Equal(3, boss.ObtenirPhaseBoss(0.10f));
    }

    [Fact]
    public void BonusDeSetSActiventSelonLesPiecesEquipees()
    {
        var joueur = new Joueur("Collectionneur", ClasseType.Guerrier);
        var pieces = new[]
        {
            TypeEquipement.Arme,
            TypeEquipement.Armure,
            TypeEquipement.Casque,
            TypeEquipement.Anneau
        };

        foreach (TypeEquipement type in pieces)
        {
            joueur.EquiperObjet(new Equipement($"Pièce {type}", type, Rarete.Legendaire, 0, 0, 0)
            {
                IdentifiantSet = "set_dragon"
            });
        }

        Assert.Equal(4, joueur.ObtenirPiecesSetEquipees("set_dragon"));
        Assert.Equal(20, joueur.BonusAttaqueSet);
        Assert.Equal(12, joueur.BonusDefenseSet);
        Assert.Equal(120, joueur.BonusPVSet);
        Assert.Contains("Embrasement du Dragon 4/5", joueur.ObtenirResumeSets());
    }

    [Fact]
    public void ContratsSecondairesRespectentLeNiveauEtLeStatut()
    {
        var joueur = new Joueur("Recrue", ClasseType.Rodeur);

        List<Quete> disponibles = ContratsSecondaires2D.CreerDisponibles(joueur);

        Assert.Single(disponibles);
        Assert.Equal(5001, disponibles[0].Id);

        joueur.QuetesActives.Add(disponibles[0]);
        Assert.Empty(ContratsSecondaires2D.CreerDisponibles(joueur));

        joueur.QuetesActives.Clear();
        joueur.QuetesCompleteesIds.Add(5001);
        Assert.Empty(ContratsSecondaires2D.CreerDisponibles(joueur));
    }

    [Fact]
    public void EchelleDeNiveauAdapteStatsEtRecompenses()
    {
        var monstre = new Monstre("Test", 1000, 100, 50, 200, 100);

        Monde2D.AppliquerEchelleNiveau(monstre, 1, 5);

        Assert.Equal(720, monstre.PVMax);
        Assert.Equal(monstre.PVMax, monstre.PVActuels);
        Assert.Equal(80, monstre.Attaque);
        Assert.Equal(42, monstre.Defense);
        Assert.Equal(180, monstre.GainXP);
        Assert.Equal(90, monstre.GainOr);
    }

    [Fact]
    public void ButinDeBossEstUnePieceDeSetLegendaire()
    {
        Equipement piece = ButinBoss2D.GenererPieceSet("Ignis Dragon Suprême", 8, new Random(4));

        Assert.Equal(Rarete.Legendaire, piece.RareteItem);
        Assert.Equal("set_dragon", piece.IdentifiantSet);
        Assert.Contains("Dragon", piece.Nom);
    }

    [Fact]
    public void RenduGraphiqueFonctionneDansChaqueTypeDeZone()
    {
        var monde = new Monde2D();
        var hero = new Joueur("Test graphique", ClasseType.Mage);
        var joueur = new Joueur2D(hero, new Vector2(260, 300));
        var camera = new Camera2D(1024, 768) { Position = joueur.Position, Cible = joueur.Position };
        using var bitmap = new Bitmap(1024, 768);
        using var graphics = Graphics.FromImage(bitmap);
        using var font = new Font("Segoe UI", 9f);
        using var fontCritique = new Font("Segoe UI", 12f, FontStyle.Bold);

        monde.Dessiner(graphics, camera, joueur, font, fontCritique);

        monde.ChargerDonjon(
            "Catacombes Oubliées",
            new List<Monstre> { new Monstre("Squelette Gardien", 500, 20, 8, 100, 40) },
            new Monstre("Malakor", 3000, 40, 12, 500, 300, true),
            hero.Niveau,
            3);
        camera.Position = new Vector2(900, 750);
        monde.Dessiner(graphics, camera, joueur, font, fontCritique);

        monde.ChargerEtageTour(2, new Monstre("Écho Stellaire", 800, 30, 10, 150, 80), niveauJoueur: hero.Niveau);
        monde.Dessiner(graphics, camera, joueur, font, fontCritique);
    }

    [Fact]
    public void CapitaleAgrandieConserveUnPointDeDepartDansLaCarte()
    {
        var monde = new Monde2D();

        Assert.True(monde.LargeurMonde > 2000f);
        Assert.True(monde.HauteurMonde > 1600f);
        Assert.InRange(monde.PositionDepartVillage.X, 0f, monde.LargeurMonde);
        Assert.InRange(monde.PositionDepartVillage.Y, 0f, monde.HauteurMonde);
        Assert.Contains(monde.ObjetsInteractifs, obj => obj.Type == TypeInteractif.NPC_Elenora);
    }

    [Fact]
    public void EnnemisNormauxOntDesArchetypesDeCombatDistincts()
    {
        var tireur = new Monstre2D(new Monstre("Archer Squelette", 300, 18, 4, 35, 12), new Vector2(0, 0));
        var rapide = new Monstre2D(new Monstre("Loup Affamé", 300, 18, 4, 35, 12), new Vector2(0, 0));
        var tank = new Monstre2D(new Monstre("Golem de Pierre", 600, 18, 4, 35, 12), new Vector2(0, 0));

        Assert.True(tireur.EstTireur);
        Assert.True(rapide.EstRapide);
        Assert.True(tank.EstLourd);
        Assert.True(tireur.Vitesse < rapide.Vitesse);
        Assert.True(tank.Rayon > rapide.Rayon);
    }

    [Fact]
    public void RaccourcisPeuventEtreRemappesEtRechargesSansConflit()
    {
        var entrees = new GestionnaireEntrees();
        Assert.False(entrees.DefinirTouche("Sort1", Keys.F));

        Assert.True(entrees.DefinirTouche("Sort2", Keys.V));
        Assert.True(entrees.DefinirTouche("Sort1", Keys.F));
        Assert.False(entrees.DefinirTouche("Ultime", Keys.F));

        var rechargees = new GestionnaireEntrees();
        rechargees.ChargerRaccourcis(entrees.CopierRaccourcis());

        Assert.Equal(Keys.F, rechargees.Touche("Sort1"));
        Assert.Equal(Keys.V, rechargees.Touche("Sort2"));
    }

    [Fact]
    public void SubirDegatsAppliqueDefenseEtDonneUneInvulnerabiliteTemporaire()
    {
        var hero = new Joueur("Défenseur", ClasseType.Guerrier);
        var joueur = new Joueur2D(hero, new Vector2(100, 100));
        var camera = new Camera2D(800, 600);
        var particules = new GestionnaireParticules();
        var textes = new List<string>();
        int pvAvant = hero.PVActuels;
        int degatsAttendus = Math.Max(1, 40 - hero.DefenseTotale / 2);

        int degats = joueur.SubirDegats(40, "TEST", particules, camera, (texte, _, _) => textes.Add(texte));
        int deuxiemeCoup = joueur.SubirDegats(40, "TEST", particules, camera, (texte, _, _) => textes.Add(texte));

        Assert.Equal(degatsAttendus, degats);
        Assert.Equal(pvAvant - degatsAttendus, hero.PVActuels);
        Assert.Equal(0, deuxiemeCoup);
        Assert.Equal(0.5f, joueur.TempsInvulnerabilite);
        Assert.Single(textes);
        Assert.Contains("TEST", textes[0]);
    }

    [Fact]
    public void SubirDegatsNeTouchePasUnJoueurEnDash()
    {
        var hero = new Joueur("Esquive", ClasseType.Rodeur);
        var joueur = new Joueur2D(hero, new Vector2(100, 100)) { DashTempsRestant = 0.2f };
        var camera = new Camera2D(800, 600);
        int pvAvant = hero.PVActuels;

        int degats = joueur.SubirDegats(80, "TEST", new GestionnaireParticules(), camera, (_, _, _) => { });

        Assert.Equal(0, degats);
        Assert.Equal(pvAvant, hero.PVActuels);
        Assert.Equal(0f, joueur.TempsInvulnerabilite);
    }

    [Fact]
    public void CourbeExperienceEstDouceEtCompatibleAvecLeNiveauSauvegarde()
    {
        var hero = new Joueur("Progression", ClasseType.Mage);

        Assert.Equal(100, hero.XPRequisPourNiveau);
        hero.Niveau = 4;
        Assert.Equal(800, hero.XPRequisPourNiveau);
    }

    [Fact]
    public void PenaliteDeMortRetireDixPourCentSansFairePerdreDeNiveau()
    {
        var hero = new Joueur("Survivant", ClasseType.Guerrier)
        {
            Niveau = 8,
            Or = 1250,
            XP = 237
        };

        var perte = hero.AppliquerPenaliteMort();

        Assert.Equal(125, perte.OrPerdu);
        Assert.Equal(24, perte.XPPerdu);
        Assert.Equal(1125, hero.Or);
        Assert.Equal(213, hero.XP);
        Assert.Equal(8, hero.Niveau);
    }

    [Fact]
    public void ConsommablesAjoutesEtConsommesCorrectement()
    {
        var hero = new Joueur("Alchimiste", ClasseType.Mage);
        int bombesDepart = hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire); // 1 au départ
        hero.AjouterConsommable(TypeConsommable.ElixirForce, 2);
        hero.AjouterConsommable(TypeConsommable.BombeIncendiaire, 1);

        Assert.Equal(2, hero.ObtenirQuantiteConsommable(TypeConsommable.ElixirForce));
        Assert.Equal(bombesDepart + 1, hero.ObtenirQuantiteConsommable(TypeConsommable.BombeIncendiaire));

        bool utilise = hero.UtiliserConsommable(TypeConsommable.ElixirForce);
        Assert.True(utilise);
        Assert.Equal(1, hero.ObtenirQuantiteConsommable(TypeConsommable.ElixirForce));

        bool echec = hero.UtiliserConsommable(TypeConsommable.ParcheminTeleport);
        Assert.False(echec);
    }

    [Fact]
    public void EquipementAmelioreAugmenteAttaqueEtDefense()
    {
        var hero = new Joueur("Forgeron", ClasseType.Guerrier);
        var epee = new Equipement("Épée Légendaire", TypeEquipement.Arme, Rarete.Legendaire, atk: 25, def: 5, pv: 0, crit: 4);
        hero.EquiperObjet(epee);

        int atkBase = hero.AttaqueTotale;
        epee.NiveauAmelioration = 3; // +9 ATK (3*3)
        Assert.True(hero.AttaqueTotale > atkBase);
    }

    [Fact]
    public void Statuts2D_PoisonEtBrulureAppliquentDegatsEtDuree()
    {
        var hero = new Joueur("Aventurier", ClasseType.Guerrier);
        var joueur = new Joueur2D(hero, new Vector2(100, 100));
        var monstre = new Monstre2D(new Monstre("Gobelin", 200, 15, 5, 20, 10), new Vector2(200, 200));

        joueur.AppliquerStatut("Poison", 2.5f, 10);
        monstre.AppliquerStatut("Brulure", 3.0f, 14);

        Assert.True(joueur.TempsPoison > 0f);
        Assert.Equal(10, joueur.DegatsPoisonTick);
        Assert.True(monstre.TempsBrulure > 0f);
        Assert.Equal(14, monstre.DegatsBrulureTick);
        Assert.True(joueur.EstSousEffetStatut);
        Assert.True(monstre.EstSousEffetStatut);
    }

    [Fact]
    public void Statuts2D_GelEtEtourdiAffectentEntites()
    {
        var hero = new Joueur("Givreux", ClasseType.Mage);
        var joueur = new Joueur2D(hero, new Vector2(100, 100));
        var monstre = new Monstre2D(new Monstre("Troll", 400, 20, 8, 40, 20), new Vector2(300, 300));

        joueur.AppliquerStatut("Gel", 2.0f);
        monstre.AppliquerStatut("Etourdi", 1.5f);

        Assert.True(joueur.TempsGel > 0f);
        Assert.True(monstre.TempsEtourdi > 0f);
        Assert.True(joueur.EstSousEffetStatut);
        Assert.True(monstre.EstSousEffetStatut);
    }

    [Fact]
    public void DonjonProcedural_GenerePlusieursSallesEtObstacles()
    {
        var monde = new Monde2D();
        var minions = new List<Monstre>
        {
            new Monstre("Squelette 1", 200, 15, 5, 20, 10),
            new Monstre("Squelette 2", 200, 15, 5, 20, 10)
        };
        var boss = new Monstre("Seigneur Nécromancien", 1200, 35, 12, 200, 150, true);

        monde.ChargerDonjon("Crypte Déchue", minions, boss, niveauJoueur: 3, niveauRecommande: 3);

        Assert.Equal(ZoneType2D.Donjon, monde.TypeZoneActuelle);
        Assert.Equal(3000f, monde.LargeurMonde);
        Assert.Equal(2000f, monde.HauteurMonde);
        Assert.True(monde.Obstacles.Count > 15);
        Assert.Contains(monde.ObjetsInteractifs, o => o.Type == TypeInteractif.PortailRetour);
        Assert.Contains(monde.ObjetsInteractifs, o => o.Type == TypeInteractif.Coffre);
        Assert.Contains(monde.Monstres, m => m.EstBoss);
    }

    [Fact]
    public void ZoneDanger2D_FormesConeEtLigneSontInitialisees()
    {
        var zoneLigne = new ZoneDanger2D
        {
            Position = new Vector2(100, 100),
            Forme = FormeZoneDanger.LigneCharge,
            Direction = new Vector2(1, 0),
            LongueurLigne = 450f,
            LargeurLigne = 80f,
            TempsTotal = 0.5f,
            TempsRestant = 0.5f,
            Degats = 50,
            NomSort = "Charge Brutale"
        };

        var zoneCone = new ZoneDanger2D
        {
            Position = new Vector2(200, 200),
            Forme = FormeZoneDanger.ConeSouffle,
            Direction = new Vector2(0, 1),
            Rayon = 320f,
            AngleOuverture = 70f,
            TempsTotal = 0.8f,
            TempsRestant = 0.8f,
            Degats = 70,
            NomSort = "Souffle de Flammes"
        };

        Assert.Equal(FormeZoneDanger.LigneCharge, zoneLigne.Forme);
        Assert.Equal(450f, zoneLigne.LongueurLigne);
        Assert.Equal(FormeZoneDanger.ConeSouffle, zoneCone.Forme);
        Assert.Equal(70f, zoneCone.AngleOuverture);
    }

    [Fact]
    public void VillageCapitale_ContientElementsImage1EtPoules()
    {
        var monde = new Monde2D();
        monde.ChargerVillage();

        Assert.Equal(ZoneType2D.Village, monde.TypeZoneActuelle);
        Assert.Contains(monde.Obstacles, o => o.TypeObstacle == TypeObstacle.TaverneDragonVert);
        Assert.True(monde.Obstacles.Count(o => o.TypeObstacle == TypeObstacle.EtalMarche) >= 2);
        Assert.Contains(monde.Obstacles, o => o.TypeObstacle == TypeObstacle.CharretteAttellee);
        Assert.Contains(monde.Obstacles, o => o.TypeObstacle == TypeObstacle.PontPierre);
        Assert.True(monde.Poules.Count >= 6);

        // Pont en pierre ne doit pas bloquer le passage du joueur
        var pont = monde.Obstacles.First(o => o.TypeObstacle == TypeObstacle.PontPierre);
        Vector2 posSurPont = new Vector2(pont.Boite.X + pont.Boite.Width / 2f, pont.Boite.Y + pont.Boite.Height / 2f);
        Vector2 resolu = monde.ResoudreCollisions(posSurPont, Vector2.Zero, 15f);
        Assert.Equal(posSurPont.X, resolu.X, 1f);
        Assert.Equal(posSurPont.Y, resolu.Y, 1f);
    }

    [Fact]
    public void PouleVillageoise_FuitJoueurEtPicore()
    {
        var hero = new Joueur("Aventurier", ClasseType.Guerrier);
        var joueur2D = new Joueur2D(hero, new Vector2(500, 500));
        var poule = new PouleVillageoise(new Vector2(510, 500), 0);

        poule.MettreAJour(0.1f, joueur2D, 2000f, 1600f);

        // La poule doit fuir vers la droite (X augmente)
        Assert.True(poule.Velocite.X > 0);
        Assert.False(poule.DirectionGauche);
    }

    [Fact]
    public void GrimoireEtCatalogue_ContientLes14SortsMagiques()
    {
        string[] idSortsAttendus = new[]
        {
            "c_orbe_foudre",
            "c_dragon_flammes",
            "c_trombe_tempete",
            "c_dome_sacre",
            "c_phenix_givre",
            "c_portail_givre",
            "c_vortex_ames",
            "c_croissant_lunaire",
            "c_dragon_tricephale",
            "c_leviathan_glaces",
            "c_miroir_glace",
            "c_trou_noir_tellurique",
            "c_prisme_irise",
            "c_abysse_eldritch"
        };

        foreach (string id in idSortsAttendus)
        {
            var recette = CatalogueCraft.Recettes.FirstOrDefault(r => r.CompetenceResultat?.Id == id);
            Assert.NotNull(recette);
            Assert.Equal(CategorieCraft.Competence, recette.Categorie);
            Assert.NotNull(recette.CompetenceResultat);
            Assert.True(recette.CompetenceResultat.CoutMana > 0);
            Assert.True(recette.CompetenceResultat.MultiplicateurDegats > 0);
        }

        var joueur = new Joueur("MageTest", ClasseType.Mage);
        joueur.AssurerSortsParDefaut();

        Assert.True(joueur.CompetencesDebloquees.Count >= 8);
        Assert.NotNull(joueur.CompetenceEquipee);
        Assert.Contains(joueur.CompetencesDebloquees, c => c.Id == "c_orbe_foudre");
        Assert.Contains(joueur.CompetencesDebloquees, c => c.Id == "c_dragon_flammes");
        Assert.Contains(joueur.CompetencesDebloquees, c => c.Id == "c_phenix_givre");
        Assert.Contains(joueur.CompetencesDebloquees, c => c.Id == "c_trombe_tempete");
        Assert.Contains(joueur.CompetencesDebloquees, c => c.Id == "c_dome_sacre");
        Assert.Contains(joueur.CompetencesDebloquees, c => c.Id == "c_vortex_ames");
        Assert.Contains(joueur.CompetencesDebloquees, c => c.Id == "c_croissant_lunaire");
    }

    [Fact]
    public void CatalogueCodexDrops_ContientTousLesBossDonjonsEtTour()
    {
        var fiches = CatalogueCodexDrops.ToutesLesFiches;
        Assert.NotNull(fiches);
        Assert.Equal(35, fiches.Count);

        int countDonjons = fiches.Count(f => f.Categorie == "Donjon");
        int countRegionaux = fiches.Count(f => f.Categorie == "Régional");
        int countMythiques = fiches.Count(f => f.Categorie == "Mythique");
        int countCosmiques = fiches.Count(f => f.Categorie == "Cosmique");
        int countTour = fiches.Count(f => f.Categorie == "Tour");

        Assert.Equal(4, countDonjons);
        Assert.Equal(10, countRegionaux);
        Assert.Equal(10, countMythiques);
        Assert.Equal(10, countCosmiques);
        Assert.Equal(1, countTour);
    }

    [Fact]
    public void CatalogueCodexDrops_RechercheEtFiltrageFonctionnent()
    {
        var fiches = CatalogueCodexDrops.ToutesLesFiches;

        // Recherche par boss
        var ficheGrok = fiches.FirstOrDefault(f => f.NomBoss.Contains("Grok", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(ficheGrok);
        Assert.Equal("Régional", ficheGrok.Categorie);

        var ficheIgnis = fiches.FirstOrDefault(f => f.NomBoss.Contains("Ignis", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(ficheIgnis);
        Assert.Equal("Mythique", ficheIgnis.Categorie);

        var ficheXanthos = fiches.FirstOrDefault(f => f.NomBoss.Contains("Xanthos", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(ficheXanthos);
        Assert.Equal("Cosmique", ficheXanthos.Categorie);

        // Recherche par matériaux
        var bossesAvecEcailles = fiches.Where(f => f.Materiaux.Any(m => m.Nom.Contains("Écaille", StringComparison.OrdinalIgnoreCase))).ToList();
        Assert.NotEmpty(bossesAvecEcailles);

        var bossesAvecEclatAstral = fiches.Where(f => f.Materiaux.Any(m => m.Nom.Contains("Astral", StringComparison.OrdinalIgnoreCase))).ToList();
        Assert.NotEmpty(bossesAvecEclatAstral);
    }

    [Fact]
    public void CatalogueCodexDrops_CalculDifficulteAjusteRecompensesEtStats()
    {
        var ficheIgnis = CatalogueCodexDrops.ToutesLesFiches.First(f => f.NomBoss.Contains("Ignis", StringComparison.OrdinalIgnoreCase));
        var diffNormale = DifficulteBoss2D.Normale;
        var diffInfernal = DifficulteBoss2D.Infernal;

        int pvNormale = ficheIgnis.CalculerPV(diffNormale);
        int pvInfernal = ficheIgnis.CalculerPV(diffInfernal);
        Assert.True(pvInfernal > pvNormale);

        int orNormale = ficheIgnis.CalculerOr(diffNormale);
        int orInfernal = ficheIgnis.CalculerOr(diffInfernal);
        Assert.True(orInfernal > orNormale);

        int pierresNormale = ficheIgnis.CalculerPierres(diffNormale);
        int pierresInfernal = ficheIgnis.CalculerPierres(diffInfernal);
        Assert.True(pierresInfernal > pierresNormale);
    }

    [Fact]
    public void CatalogueCodexDrops_TousLesBossOntLeursDropsSetsEtMateriaux()
    {
        foreach (var fiche in CatalogueCodexDrops.ToutesLesFiches)
        {
            Assert.False(string.IsNullOrWhiteSpace(fiche.NomBoss));
            Assert.False(string.IsNullOrWhiteSpace(fiche.Icone));
            Assert.False(string.IsNullOrWhiteSpace(fiche.Faiblesse));
            Assert.True(fiche.NiveauConseille >= 1);
            Assert.True(fiche.PVBase > 0);
            Assert.True(fiche.AttaqueBase > 0);
            Assert.True(fiche.GainOrBase > 0);
            Assert.True(fiche.GainXPBase > 0);

            // Matériaux avec taux valides
            Assert.NotEmpty(fiche.Materiaux);
            foreach (var mat in fiche.Materiaux)
            {
                Assert.False(string.IsNullOrWhiteSpace(mat.Nom));
                Assert.True(mat.QuantiteMin >= 1);
                Assert.True(mat.QuantiteMax >= mat.QuantiteMin);
                Assert.True(mat.TauxDropPourcent > 0 && mat.TauxDropPourcent <= 100);
            }

            // Sets et pièces d'équipement
            if (fiche.Categorie != "Tour")
            {
                Assert.NotEmpty(fiche.Equipements);
                Assert.Equal(5, fiche.Equipements.Count); // Épée, Plastron, Casque, Anneau, Amulette
                Assert.Equal(100f, fiche.Equipements.Sum(e => e.TauxDropPourcent));
                foreach (var eq in fiche.Equipements)
                {
                    Assert.False(string.IsNullOrWhiteSpace(eq.Nom));
                    Assert.False(string.IsNullOrWhiteSpace(eq.NomSet));
                    Assert.Equal(20f, eq.TauxDropPourcent);
                }
            }
        }
    }

    [Fact]
    public void CatalogueCodexDrops_TrouverFiche_TrouveChaqueBossAvecOuSansBadge()
    {
        // Vérifier que tous les boss du jeu sont résolus par TrouverFiche
        foreach (var fiche in CatalogueCodexDrops.ToutesLesFiches)
        {
            if (fiche.Fabrique != null)
            {
                var monstre = fiche.Fabrique();
                var trouveParNomExact = CatalogueCodexDrops.TrouverFiche(monstre.Nom);
                Assert.NotNull(trouveParNomExact);

                // Avec badge de difficulté d'arène
                var trouveAvecBadge = CatalogueCodexDrops.TrouverFiche($"{monstre.Nom} [🟢 Normale]");
                Assert.NotNull(trouveAvecBadge);

                var trouveAvecBadgeInfernal = CatalogueCodexDrops.TrouverFiche($"{monstre.Nom} [💀 Infernal]");
                Assert.NotNull(trouveAvecBadgeInfernal);
            }
        }
    }

    [Fact]
    public void CatalogueCodexDrops_TirerMateriaux_RespecteTauxEtQuantites()
    {
        var fiche = CatalogueCodexDrops.TrouverFiche("Grok le Brise-Crâne");
        Assert.NotNull(fiche);

        var rng = new Random(42);
        for (int i = 0; i < 50; i++)
        {
            var loots = CatalogueCodexDrops.TirerMateriaux(fiche, rng);
            // Matériau garanti à 100% : Minerai de Fer (4-8)
            Assert.True(loots.ContainsKey("Minerai de Fer"));
            Assert.InRange(loots["Minerai de Fer"], 4, 8);

            // Morceau de Cuir est aussi à 100% (3-6 ou 4-6)
            Assert.True(loots.ContainsKey("Morceau de Cuir"));

            // Chaque matériau tiré doit être dans la fiche
            foreach (var kvp in loots)
            {
                Assert.Contains(fiche.Materiaux, m => m.Nom == kvp.Key);
            }
        }
    }

    [Fact]
    public void Boss_ObtenirLootMateriaux_UtiliseLeCodex()
    {
        var kragh = new BossKragh();
        var lootKragh = kragh.ObtenirLootMateriaux();

        // Dans l'ancien code obsolète, Kragh donnait "Venin Corrosif" (inexistant).
        // Avec le Codex, il donne des matériaux valides tels que "Minerai de Fer", "Venin Obscur", "Cuir Épais".
        Assert.DoesNotContain("Venin Corrosif", lootKragh.Keys);
        Assert.True(lootKragh.ContainsKey("Minerai de Fer"));

        var belial = new BossBelial();
        var lootBelial = belial.ObtenirLootMateriaux();
        // Bélial donne Sang de Démon, Cœur Ardent, Écaille Draconique
        Assert.True(lootBelial.ContainsKey("Sang de Démon") || lootBelial.ContainsKey("Cœur Ardent"));
    }

    [Fact]
    public void CatalogueCodexDrops_GenererPieceSet_DonneLeBonSetSelonLaCategorieDuBoss()
    {
        var rng = new Random(123);

        // Boss Régional -> set_aethelgard
        var ficheRegional = CatalogueCodexDrops.TrouverFiche("Grok le Brise-Crâne");
        Assert.NotNull(ficheRegional);
        var pieceReg = CatalogueCodexDrops.GenererPieceSet(ficheRegional, 5, rng);
        Assert.Equal("set_aethelgard", pieceReg.IdentifiantSet);
        Assert.Contains("d'Aethelgard", pieceReg.Nom);

        // Boss Mythique -> set_dragon
        var ficheMythique = CatalogueCodexDrops.TrouverFiche("Ignis Dragon Suprême");
        Assert.NotNull(ficheMythique);
        var pieceMyth = CatalogueCodexDrops.GenererPieceSet(ficheMythique, 10, rng);
        Assert.Equal("set_dragon", pieceMyth.IdentifiantSet);
        Assert.Contains("du Dragon", pieceMyth.Nom);

        // Boss Cosmique -> set_astral
        var ficheCosmique = CatalogueCodexDrops.TrouverFiche("Xanthos le Gardien");
        Assert.NotNull(ficheCosmique);
        var pieceCosm = CatalogueCodexDrops.GenererPieceSet(ficheCosmique, 15, rng);
        Assert.Equal("set_astral", pieceCosm.IdentifiantSet);
        Assert.Contains("Astral", pieceCosm.Nom);
    }

    [Fact]
    public void ButinBoss2D_GenererPieceSet_UtiliseCodexEtIdentifiantSetCorrect()
    {
        var rng = new Random(99);
        var pieceDeus = ButinBoss2D.GenererPieceSet("Deus Ex Nihilo, Architecte du Chaos Primordial", 20, rng);
        Assert.Equal("set_astral", pieceDeus.IdentifiantSet);

        var pieceChronos = ButinBoss2D.GenererPieceSet("Chronos, Seigneur des Sabliers Brisés", 15, rng);
        Assert.Equal("set_astral", pieceChronos.IdentifiantSet);

        var pieceBelial = ButinBoss2D.GenererPieceSet("Belial, Seigneur Démoniaque des Flammes Noires", 9, rng);
        Assert.Equal("set_dragon", pieceBelial.IdentifiantSet);
    }

    [Fact]
    public void Monde2D_RecompensesBossMort_DistribuePierresSelonDifficulte()
    {
        var hero = new Joueur("HeroTest", ClasseType.Guerrier);
        hero.PierresDeForge = 0;

        var fiche = CatalogueCodexDrops.TrouverFiche("Grok le Brise-Crâne");
        Assert.NotNull(fiche);

        var butinNormale = CatalogueCodexDrops.GenererLootBossComplet(fiche, DifficulteBoss2D.Normale, 1, new Random(1));
        var butinInfernal = CatalogueCodexDrops.GenererLootBossComplet(fiche, DifficulteBoss2D.Infernal, 1, new Random(1));

        // Normale : 2 pierres de base
        Assert.Equal(2, butinNormale.PierresDeForge);
        // Infernal : 2 + 8 = 10 pierres
        Assert.Equal(10, butinInfernal.PierresDeForge);

        Assert.NotNull(butinNormale.PieceEquipement);
        Assert.Equal("set_aethelgard", butinNormale.PieceEquipement.IdentifiantSet);
    }

    [Fact]
    public void CatalogueCodexDrops_TrouverFiche_TourAstraleTrouveGardiensEtSentinelles()
    {
        // 1. Recherche par nom direct d'un gardien de la tour
        var ficheSentinelle = CatalogueCodexDrops.TrouverFiche("Sentinelle Stellaire");
        Assert.NotNull(ficheSentinelle);
        Assert.Equal("tour_astrale", ficheSentinelle.Id);

        var ficheColosse = CatalogueCodexDrops.TrouverFiche("Colosse Céleste (Étage 10)");
        Assert.NotNull(ficheColosse);
        Assert.Equal("tour_astrale", ficheColosse.Id);

        // 2. Recherche avec nom de zone "Tour Astrale — Étage X"
        var ficheZone = CatalogueCodexDrops.TrouverFiche("👑 Grok le Brise-Crâne (Étage 5)", "Tour Astrale — Étage 5");
        Assert.NotNull(ficheZone);
        Assert.Equal("tour_astrale", ficheZone.Id);
    }

    [Fact]
    public void CatalogueCodexDrops_ObtenirVictoiresBoss_CompteLesVictoiresAvecBadgesEtEtages()
    {
        var hero = new Joueur("HeroTest", ClasseType.Guerrier);
        // Boss tué dans différentes difficultés et contextes
        hero.BestiaireMonstresTues["Grok le Brise-Crâne [🟢 Normale]"] = 2;
        hero.BestiaireMonstresTues["Grok le Brise-Crâne [💀 Infernal]"] = 1;
        hero.BestiaireMonstresTues["👑 Grok le Brise-Crâne (Étage 5)"] = 3;

        var ficheGrok = CatalogueCodexDrops.TrouverFiche("Grok le Brise-Crâne");
        Assert.NotNull(ficheGrok);

        int totalVictoires = CatalogueCodexDrops.ObtenirVictoiresBoss(hero, ficheGrok);
        // Doit agréger toutes les victoires sur Grok (2 + 1 + 3 = 6)
        Assert.Equal(6, totalVictoires);
    }

    [Fact]
    public void CodexDropsBoss_DonjonsEtTour_OntDesBoutonsDExpeditionEtLancement()
    {
        var hero = new Joueur("HeroTest", ClasseType.Guerrier);

        // Donjon Forêt
        var ficheForet = CatalogueCodexDrops.TrouverFiche("Forêt des Murmures");
        Assert.NotNull(ficheForet);
        Assert.Equal("Donjon", ficheForet.Categorie);

        // Tour Astrale
        var ficheTour = CatalogueCodexDrops.TrouverFiche("Tour Astrale Infinie");
        Assert.NotNull(ficheTour);
        Assert.Equal("tour_astrale", ficheTour.Id);
        Assert.Equal("Tour", ficheTour.Categorie);

        // Vérifier que FormCodexDropsBoss s'instancie sans erreur avec le nouveau callback
        FicheDropBoss? ficheLancee = null;
        DifficulteBoss2D? diffLancee = null;
        using var codex = new FormCodexDropsBoss(hero, (f, diff) =>
        {
            ficheLancee = f;
            diffLancee = diff;
        });
        Assert.NotNull(codex);
    }
}



