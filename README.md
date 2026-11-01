# ⚔️ Aethelgard - Jeu de Rôle 2D (RPG)

Un jeu de rôle 2D en C# développé avec .NET Windows Forms. Incarnez un héros dans le monde d'Aethelgard, affrontez des monstres et des boss épiques, récoltez des matériaux et fabriquez de puissants équipements.

## 🎮 Fonctionnalités principales

- **Système de Combat & Boss :** Affrontez des boss uniques avec des mécaniques et des attaques variées (`BossManager2D.cs`, `BossEtendus.cs`).
- **Artisanat & Récolte :** Système complet de craft et de recettes pour améliorer votre équipement (`CatalogueCraft.cs`, `GuideDesMateriaux.cs`).
- **Arbre de Talents & Progression :** Personnalisation des caractéristiques (Force, Agilité, Endurance, Intelligence) et des sorts du joueur.
- **Sauvegarde & Chargement :** Sauvegarde automatique et persistante de la progression du héros.
- **Tests Unitaires :** Suite de tests automatisés pour valider la logique de combat et de progression (`jeux.Tests`).

## 🚀 Lancement rapide

### Prérequis
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) ou version ultérieure (Windows).

### Exécution
Pour lancer le jeu depuis un terminal :
```bash
dotnet run --project jeux.csproj
```
Ou double-cliquez simplement sur `Lancer_Le_Jeu.bat`.

### Lancer les tests
```bash
dotnet test jeux.Tests/jeux.Tests.csproj
```

## 📜 Licence
Distribué sous licence MIT. Consultez le fichier `LICENSE` pour plus d'informations.
