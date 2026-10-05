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
}
