using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace JeuxRPG
{
    // ==============================================================
    // ENCYCLOPÉDIE & GUIDE DES MATÉRIAUX (SOURCES DE BUTIN & CRAFT)
    // ==============================================================
    public class SourceObtentionMateriau
    {
        public string TypeSource { get; set; } = ""; // 👑 Boss, ⚔️ Monstres, 🏰 Donjon/Tour, ♻️ Recyclage, 🌿 Récolte, 📜 Quête
        public string NomSource { get; set; } = "";
        public string ZoneOuLieu { get; set; } = "";
        public string DetailsTaux { get; set; } = "";
    }

    public class FicheMateriau
    {
        public string Nom { get; set; } = "";
        public string Icone { get; set; } = "📦";
        public Rarete RareteItem { get; set; } = Rarete.Commun;
        public string Categorie { get; set; } = "Composant";
        public string Description { get; set; } = "";
        public List<SourceObtentionMateriau> Sources { get; set; } = new List<SourceObtentionMateriau>();

        public List<string> ObtenirRecettesAssociees()
        {
            var list = new List<string>();
            foreach (var r in CatalogueCraft.Recettes)
            {
                if (r.MateriauxRequis.TryGetValue(Nom, out int requis))
                {
                    string cat = r.Categorie switch
                    {
                        CategorieCraft.Arme => "⚔️ Arme",
                        CategorieCraft.Armure => "🛡️ Armure",
                        CategorieCraft.Bijou => "💍 Bijou",
                        CategorieCraft.Competence => "🔮 Sort",
                        CategorieCraft.Consommable => "🧪 Potion",
                        _ => "📦 Objet"
                    };
                    list.Add($"{cat} : {r.Nom} (x{requis} requis)");
                }
            }
            if (Nom == "Pierres de Forge")
            {
                list.Add("🔨 Amélioration d'Équipement (+1 à +10 chez Brom le Forgeron)");
            }
            return list;
        }
    }

    public static class GuideDesMateriaux
    {
        public static List<FicheMateriau> Fiches { get; } = new List<FicheMateriau>();

        static GuideDesMateriaux()
        {
            InitialiserGuide();
        }

        private static void InitialiserGuide()
        {
            // 1. Minerai de Fer
            Fiches.Add(new FicheMateriau {
                Nom = "Minerai de Fer", Icone = "⛏️", RareteItem = Rarete.Commun, Categorie = "Minerais & Métaux",
                Description = "Minerai brut extrait des filons des montagnes ou pillé sur les troupes gobelines. Indispensable pour forger les armes et armures de base chez le forgeron.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Gobelins, Pillards des routes", ZoneOuLieu = "Bois des Ombres & Plaines", DetailsTaux = "Taux 60% (1 à 3 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Squelettes & Gardes Morts-Vivants", ZoneOuLieu = "Cryptes & Donjon Oublié", DetailsTaux = "Taux 40% (1 à 2 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kragh l'Écorcheur (Boss Niv. 3)", ZoneOuLieu = "Antre de la Horde Noire", DetailsTaux = "GARANTI (x6 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Gorrok le Broyeur (Boss Niv. 2)", ZoneOuLieu = "Terres Sauvages", DetailsTaux = "GARANTI (x3 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Grok Seigneur Gobelin (Boss Niv. 2)", ZoneOuLieu = "Camp Gobelin", DetailsTaux = "GARANTI (x5 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Balthazar Chevalier Déchu (Boss Niv. 6)", ZoneOuLieu = "Donjon Déchu", DetailsTaux = "GARANTI (x7 minerais)" },
                    new SourceObtentionMateriau { TypeSource = "♻️ Recyclage", NomSource = "Brom le Forgeron", ZoneOuLieu = "Forge du Village", DetailsTaux = "Recycler des armes et armures en fer" }
                }
            });

            // 2. Morceau de Cuir
            Fiches.Add(new FicheMateriau {
                Nom = "Morceau de Cuir", Icone = "🐗", RareteItem = Rarete.Commun, Categorie = "Peaux & Cuirs",
                Description = "Lanières de cuir souple prélevées sur le gibier ou confectionnées par les tanneurs. Sert de doublure aux armures et de poignées pour les armes.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Gobelins éclaireurs & Petites bêtes", ZoneOuLieu = "Forêt de Val-Serein", DetailsTaux = "Taux 60% (x1 morceau)" },
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Sangliers & Loups des plaines", ZoneOuLieu = "Terres Sauvages", DetailsTaux = "Taux 50% (x1 morceau)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Grok le Seigneur Gobelin (Boss Niv. 2)", ZoneOuLieu = "Camp Gobelin", DetailsTaux = "GARANTI (x4 morceaux)" },
                    new SourceObtentionMateriau { TypeSource = "♻️ Recyclage", NomSource = "Brom le Forgeron", ZoneOuLieu = "Forge du Village", DetailsTaux = "Recycler des armures légères" }
                }
            });

            // 3. Cuir Épais
            Fiches.Add(new FicheMateriau {
                Nom = "Cuir Épais", Icone = "🐺", RareteItem = Rarete.Rare, Categorie = "Peaux & Cuirs",
                Description = "Peau robuste et dense issue des bêtes féroces et des prédateurs dominants. Offre une formidable résistance aux coupures et aux morsures.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Loups sauvages, Sangliers enragés", ZoneOuLieu = "Forêt Maudite & Cavernes", DetailsTaux = "Taux 80% (1 à 3 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Gorrok le Broyeur (Boss Niv. 2)", ZoneOuLieu = "Terres Sauvages", DetailsTaux = "GARANTI (x4 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kragh l'Écorcheur (Boss Niv. 3)", ZoneOuLieu = "Antre de la Horde", DetailsTaux = "GARANTI (x3 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Vespera la Matriarche (Boss Niv. 5)", ZoneOuLieu = "Falaises Embrumées", DetailsTaux = "GARANTI (x4 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kryll l'Arachnide (Boss Niv. 7)", ZoneOuLieu = "Gouffre de Nécrose", DetailsTaux = "GARANTI (x5 cuirs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Fenrir de la Lune Rouge (Boss Niv. 12)", ZoneOuLieu = "Arène de Lune de Sang", DetailsTaux = "GARANTI (x8 cuirs)" }
                }
            });

            // 4. Croc Sauvage
            Fiches.Add(new FicheMateriau {
                Nom = "Croc Sauvage", Icone = "🦷", RareteItem = Rarete.Rare, Categorie = "Trophées & Faune",
                Description = "Croc effilé prélevé sur les bêtes sanguinaires. Utilisé pour forger des lames tranchantes et des dagues infligeant des saignements.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Loups alpha & Bêtes féroces", ZoneOuLieu = "Terres Sauvages & Bois", DetailsTaux = "Taux 50% (x1 croc)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Gorrok le Broyeur (Boss Niv. 2)", ZoneOuLieu = "Terres Sauvages", DetailsTaux = "GARANTI (x3 crocs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Vespera la Matriarche (Boss Niv. 5)", ZoneOuLieu = "Falaises Embrumées", DetailsTaux = "GARANTI (x3 crocs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Fenrir de la Lune Rouge (Boss Niv. 12)", ZoneOuLieu = "Sanctuaire Sanguin", DetailsTaux = "GARANTI (x6 crocs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Grok le Seigneur Gobelin (Boss Niv. 2)", ZoneOuLieu = "Camp Gobelin", DetailsTaux = "GARANTI (x2 crocs)" }
                }
            });

            // 5. Os Renforcé
            Fiches.Add(new FicheMateriau {
                Nom = "Os Renforcé", Icone = "💀", RareteItem = Rarete.Rare, Categorie = "Ossements & Reliques",
                Description = "Ossement imprégné de magie sépulcrale dense. Utilisé pour forger des masses contondantes, plastrons d'écailles et tomes telluriques.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Squelettes & Guerriers d'ossuaire", ZoneOuLieu = "Cryptes Sépulcrales & Donjon", DetailsTaux = "Taux 85% (1 à 3 os)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Skuldir le Roi Squelette (Boss Niv. 4)", ZoneOuLieu = "Donjon de Glace Antique", DetailsTaux = "GARANTI (x5 os)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zulgar le Chaman Putride (Boss Niv. 5)", ZoneOuLieu = "Marais Corrompus", DetailsTaux = "GARANTI (x4 os)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Malakor le Nécromancien (Boss Niv. 4)", ZoneOuLieu = "Donjon Oublié", DetailsTaux = "GARANTI (x5 os)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Mor'Gath la Liche (Boss Niv. 7)", ZoneOuLieu = "Catacombes Impies", DetailsTaux = "GARANTI (x5 os)" }
                }
            });

            // 6. Ectoplasme
            Fiches.Add(new FicheMateriau {
                Nom = "Ectoplasme", Icone = "👻", RareteItem = Rarete.Epique, Categorie = "Essences Spectrales",
                Description = "Matière vaporeuse phosphorescente laissée par les apparitions spectrales. Catalyseur majeur pour la magie des runes et les potions supérieures.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Spectres errants & Ombres nocturnes", ZoneOuLieu = "Catacombes & Nécropoles", DetailsTaux = "Taux 70% (1 à 3 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Malakor le Nécromancien (Boss Niv. 4)", ZoneOuLieu = "Donjon Oublié", DetailsTaux = "GARANTI (x4 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Skuldir le Roi Squelette (Boss Niv. 4)", ZoneOuLieu = "Donjon de Glace Antique", DetailsTaux = "GARANTI (x3 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zulgar le Chaman Putride (Boss Niv. 5)", ZoneOuLieu = "Marais Corrompus", DetailsTaux = "GARANTI (x4 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kryll l'Arachnide (Boss Niv. 7)", ZoneOuLieu = "Gouffre de Nécrose", DetailsTaux = "GARANTI (x3 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Sylvana Reine des Ronces (Boss Niv. 8)", ZoneOuLieu = "Forêt Épineuse", DetailsTaux = "GARANTI (x5 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Mor'Gath la Liche (Boss Niv. 7)", ZoneOuLieu = "Catacombes Impies", DetailsTaux = "GARANTI (x6 ectoplasmes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Nox le Dévoreur de Lumière (Boss Niv. 9)", ZoneOuLieu = "Failles Obscures", DetailsTaux = "GARANTI (x7 ectoplasmes)" }
                }
            });

            // 7. Pierre d'Âme
            Fiches.Add(new FicheMateriau {
                Nom = "Pierre d'Âme", Icone = "🔮", RareteItem = Rarete.Epique, Categorie = "Joyaux Magiques",
                Description = "Joyau ésotérique scintillant abritant l'écho d'une âme transcendée. Indispensable pour sertir les anneaux régénérants et armes arcaniques.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Ombres des profondeurs & Spectres majeurs", ZoneOuLieu = "Sanctuaires Nocturnes", DetailsTaux = "Taux 40% (x1 pierre)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Malakor le Nécromancien (Boss Niv. 4)", ZoneOuLieu = "Donjon Oublié", DetailsTaux = "GARANTI (x3 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kaelas Reine du Blizzard (Boss Niv. 6)", ZoneOuLieu = "Pics Boréaux", DetailsTaux = "GARANTI (x3 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Balthazar Chevalier Déchu (Boss Niv. 6)", ZoneOuLieu = "Donjon Déchu", DetailsTaux = "GARANTI (x2 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Mor'Gath la Liche (Boss Niv. 7)", ZoneOuLieu = "Catacombes Impies", DetailsTaux = "GARANTI (x4 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Sylvana Reine des Ronces (Boss Niv. 8)", ZoneOuLieu = "Forêt Épineuse", DetailsTaux = "GARANTI (x3 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Nox le Dévoreur de Lumière (Boss Niv. 9)", ZoneOuLieu = "Failles Obscures", DetailsTaux = "GARANTI (x4 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Thalor le Sans-Tête (Boss Niv. 10)", ZoneOuLieu = "Champs de Bataille Maudits", DetailsTaux = "GARANTI (x3 pierres)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chronos Maître du Temps (Boss Niv. 15)", ZoneOuLieu = "Sabliers Brisés", DetailsTaux = "GARANTI (x5 pierres)" }
                }
            });

            // 8. Acier Trempé
            Fiches.Add(new FicheMateriau {
                Nom = "Acier Trempé", Icone = "🛡️", RareteItem = Rarete.Epique, Categorie = "Minerais & Métaux",
                Description = "Alliage noble forgé dans les creusets élémentaires. Sa dureté extrême permet de façonner des lames de guerre et armures de plates de haut rang.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Golems de pierre & Gardiens de fer", ZoneOuLieu = "Cités Enfouies & Donjons", DetailsTaux = "Taux 75% (1 à 2 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Balthazar Chevalier Déchu (Boss Niv. 6)", ZoneOuLieu = "Donjon Déchu", DetailsTaux = "GARANTI (x5 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Magmarion Colosse de Braise (Boss Niv. 7)", ZoneOuLieu = "Cratère Volcanique", DetailsTaux = "GARANTI (x5 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Obsidius Titan de Verre (Boss Niv. 8)", ZoneOuLieu = "Mont d'Obsidienne", DetailsTaux = "GARANTI (x6 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Valdorak Colosse de Granite (Boss Niv. 8)", ZoneOuLieu = "Abîmes Telluriques", DetailsTaux = "GARANTI (x8 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Dragon Ignis (Boss Niv. 8)", ZoneOuLieu = "Volcan d'Aethelgard", DetailsTaux = "GARANTI (x5 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Thalor le Sans-Tête (Boss Niv. 10)", ZoneOuLieu = "Légion Impériale", DetailsTaux = "GARANTI (x7 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zephyros Seigneur des Ouragans (Boss Niv. 11)", ZoneOuLieu = "Sommets Éthérés", DetailsTaux = "GARANTI (x6 aciers)" },
                    new SourceObtentionMateriau { TypeSource = "♻️ Recyclage", NomSource = "Brom le Forgeron", ZoneOuLieu = "Forge du Village", DetailsTaux = "Démantèlement d'équipements Épiques & Légendaires" }
                }
            });

            // 9. Noyau de Givre
            Fiches.Add(new FicheMateriau {
                Nom = "Noyau de Givre", Icone = "❄️", RareteItem = Rarete.Epique, Categorie = "Élémentaire Boréal",
                Description = "Cristal de froid absolu insensible à la chaleur ambiante. Nécessaire pour forger des marteaux givrants, heaumes boréaux et tomes de foudre paralysante.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Golems polaires & Élémentaires de givre", ZoneOuLieu = "Cavernes Boréales", DetailsTaux = "Taux 80% (x1 noyau)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kaelas Archifée du Blizzard (Boss Niv. 6)", ZoneOuLieu = "Pics Boréaux", DetailsTaux = "GARANTI (x4 noyaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Skuldir le Cryomancien (Boss Niv. 4)", ZoneOuLieu = "Donjon de Glace Antique", DetailsTaux = "GARANTI (x3 noyaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zephyros Seigneur des Ouragans (Boss Niv. 11)", ZoneOuLieu = "Pic Tempétueux", DetailsTaux = "GARANTI (x4 noyaux)" }
                }
            });

            // 10. Plume Aérienne
            Fiches.Add(new FicheMateriau {
                Nom = "Plume Aérienne", Icone = "🦅", RareteItem = Rarete.Epique, Categorie = "Bêtes Célestes",
                Description = "Plume aux barbes tranchantes comme l'acier prélevée sur les souveraines des cieux. Apporte une vélocité sans égale.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Harpies des falaises & Rocs tempestaires", ZoneOuLieu = "Falaises Embrumées", DetailsTaux = "Taux 60% (1 à 2 plumes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Vespera Matriarche des Harpies (Boss Niv. 5)", ZoneOuLieu = "Falaises Embrumées", DetailsTaux = "GARANTI (x5 plumes)" }
                }
            });

            // 11. Venin Obscur
            Fiches.Add(new FicheMateriau {
                Nom = "Venin Obscur", Icone = "🧪", RareteItem = Rarete.Legendaire, Categorie = "Alchimie & Toxines",
                Description = "Toxine abyssale corrosive dissolvant le métal et brûlant les chairs en quelques fractions de seconde. Requis pour le tome Peste de la Vipère.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Arachnides toxiques des profondeurs", ZoneOuLieu = "Cavernes Abyssales", DetailsTaux = "Taux 70% (1 à 2 venins)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kryll l'Arachnide de Nécrose (Boss Niv. 7)", ZoneOuLieu = "Gouffre Souterrain", DetailsTaux = "GARANTI (x4 venins)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Azkalith Reine Vipère des Abysses (Boss Niv. 9)", ZoneOuLieu = "Abîmes Oubliées", DetailsTaux = "GARANTI (x5 venins)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kragh l'Écorcheur (Boss Niv. 3)", ZoneOuLieu = "Antre de la Horde", DetailsTaux = "GARANTI (x2 venins)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kraken des Abîmes Primordiaux (Boss Niv. 14)", ZoneOuLieu = "Fosses Océaniques", DetailsTaux = "GARANTI (x6 venins)" }
                }
            });

            // 12. Écaille Draconique
            Fiches.Add(new FicheMateriau {
                Nom = "Écaille Draconique", Icone = "🐉", RareteItem = Rarete.Legendaire, Categorie = "Draconique & Igné",
                Description = "Écaille quasi-indestructible issue des drakes et dragons millénaires. Nécessaire pour forger la Lame du Pourfendeur et la Carapace Suprême.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Drakes de feu & Vouivres des pics", ZoneOuLieu = "Pics Volcaniques", DetailsTaux = "Taux 75% (1 à 3 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Ignis Dragon Millénaire Suprême (Boss Niv. 8)", ZoneOuLieu = "Cœur du Volcan", DetailsTaux = "GARANTI (x6 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Magmarion Colosse de Braise (Boss Niv. 7)", ZoneOuLieu = "Cratère Ardent", DetailsTaux = "GARANTI (x3 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Belial Seigneur Démoniaque (Boss Niv. 6)", ZoneOuLieu = "Abîme Infernal", DetailsTaux = "GARANTI (x4 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Azkalith la Reine Vipère (Boss Niv. 9)", ZoneOuLieu = "Fosses Abyssales", DetailsTaux = "GARANTI (x3 écailles)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kraken des Abîmes (Boss Niv. 14)", ZoneOuLieu = "Abîmes Primordiaux", DetailsTaux = "GARANTI (x6 écailles)" }
                }
            });

            // 13. Cœur Ardent
            Fiches.Add(new FicheMateriau {
                Nom = "Cœur Ardent", Icone = "🔥", RareteItem = Rarete.Legendaire, Categorie = "Élémentaire de Feu",
                Description = "Noyau de magma solidifié rayonnant d'une chaleur volcanique infinie. Alimente les forges mythiques et le sortilège Souffle d'Ignis.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Élémentaires de magma & Drakes anciens", ZoneOuLieu = "Cheminée du Volcan", DetailsTaux = "Taux 70% (x1 cœur)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Ignis Dragon Millénaire Suprême (Boss Niv. 8)", ZoneOuLieu = "Cœur du Volcan", DetailsTaux = "GARANTI (x4 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Magmarion Colosse de Braise (Boss Niv. 7)", ZoneOuLieu = "Cratère Ardent", DetailsTaux = "GARANTI (x4 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Belial Seigneur Démoniaque (Boss Niv. 6)", ZoneOuLieu = "Abîme Infernal", DetailsTaux = "GARANTI (x4 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Archonte Solaire Déchu (Boss Niv. 13)", ZoneOuLieu = "Sanctuaire Solaire", DetailsTaux = "GARANTI (x4 cœurs)" }
                }
            });

            // 14. Sang de Démon
            Fiches.Add(new FicheMateriau {
                Nom = "Sang de Démon", Icone = "🩸", RareteItem = Rarete.Legendaire, Categorie = "Occulte & Abyssal",
                Description = "Sang impie bouillonnant recueilli sur les seigneurs démoniaques. Ouvre la voie au Trident du Purgatoire et au tome Festin de Sang.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Démons majeurs des failles", ZoneOuLieu = "Abîmes Souterrains", DetailsTaux = "Taux 60% (1 à 2 sangs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Belial Seigneur Démoniaque (Boss Niv. 6)", ZoneOuLieu = "Abîme Infernal", DetailsTaux = "GARANTI (x6 sangs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Fenrir de la Lune Rouge (Boss Niv. 12)", ZoneOuLieu = "Sanctuaire Écarlate", DetailsTaux = "GARANTI (x6 sangs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Thalor le Général Sans-Tête (Boss Niv. 10)", ZoneOuLieu = "Légion Impériale", DetailsTaux = "GARANTI (x3 sangs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Abaddon Pourfendeur de Mondes (Boss Niv. 18)", ZoneOuLieu = "Faille Apocalyptique", DetailsTaux = "GARANTI (x7 sangs)" }
                }
            });

            // 15. Cœur de Titan
            Fiches.Add(new FicheMateriau {
                Nom = "Cœur de Titan", Icone = "🗿", RareteItem = Rarete.Legendaire, Categorie = "Tellurique Ancien",
                Description = "Fragment de pierre millénaire doté d'une pulsation tellurique continue. Composant primordial du Fracas de Valdorak et du Plastron Inébranlable.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Colosses telluriques & Golems millénaires", ZoneOuLieu = "Gouffres Souterrains", DetailsTaux = "Taux 50% (x1 cœur)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Valdorak Colosse de Granite (Boss Niv. 8)", ZoneOuLieu = "Gouffres Telluriques", DetailsTaux = "GARANTI (x4 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Obsidius Titan de Verre Noir (Boss Niv. 8)", ZoneOuLieu = "Mont d'Obsidienne", DetailsTaux = "GARANTI (x2 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Kraken des Abîmes (Boss Niv. 14)", ZoneOuLieu = "Fosses Océaniques", DetailsTaux = "GARANTI (x2 cœurs)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Léviathan des Galaxies (Boss Niv. 16)", ZoneOuLieu = "Éther Stellaire", DetailsTaux = "GARANTI (x4 cœurs)" }
                }
            });

            // 16. Éclat Astral
            Fiches.Add(new FicheMateriau {
                Nom = "Éclat Astral", Icone = "🌌", RareteItem = Rarete.Legendaire, Categorie = "Stellaire & Cosmique",
                Description = "Lumière condensée issue du firmament céleste. Composant central de tous les équipements et sorts mythiques de haute volée.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "🏰 Tour Astrale", NomSource = "Passage de chaque Étage", ZoneOuLieu = "Tour Astrale Infinie", DetailsTaux = "GARANTI (Récompense automatique de montée)" },
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "Créatures stellaires & cosmiques", ZoneOuLieu = "Tour Astrale Infinie", DetailsTaux = "Taux 80% (1 à 3 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Xanthos Dévoreur Stellaire (Boss Niv. 10)", ZoneOuLieu = "Failles Dimensionnelles", DetailsTaux = "GARANTI (x8 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chevalier du Néant Primordial (Boss Niv. 10)", ZoneOuLieu = "Abîme Originel", DetailsTaux = "GARANTI (x8 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zephyros Seigneur des Ouragans (Boss Niv. 11)", ZoneOuLieu = "Sommet des Ouragans", DetailsTaux = "GARANTI (x6 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Archonte Solaire Déchu (Boss Niv. 13)", ZoneOuLieu = "Flamme Blanche", DetailsTaux = "GARANTI (x8 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chronos Maître du Temps (Boss Niv. 15)", ZoneOuLieu = "Sabliers Brisés", DetailsTaux = "GARANTI (x9 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Léviathan des Galaxies (Boss Niv. 16)", ZoneOuLieu = "Voie Lactée Perdue", DetailsTaux = "GARANTI (x12 éclats)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Deus Ex Nihilo (Boss Niv. 20)", ZoneOuLieu = "Trône Cosmique", DetailsTaux = "GARANTI (x16 éclats)" }
                }
            });

            // 17. Cristal Cosmique
            Fiches.Add(new FicheMateriau {
                Nom = "Cristal Cosmique", Icone = "🌠", RareteItem = Rarete.Mythique, Categorie = "Cosmique Suprême",
                Description = "Prisme irisé né dans le vide intersidéral, vibrant à la fréquence des galaxies. Permet de forger les reliques et tomes de rang Mythique.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Xanthos Dévoreur Stellaire (Boss Niv. 10)", ZoneOuLieu = "Failles Dimensionnelles", DetailsTaux = "GARANTI (x4 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chevalier du Néant Primordial (Boss Niv. 10)", ZoneOuLieu = "Abîme Originel", DetailsTaux = "GARANTI (x3 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Archonte Solaire Déchu (Boss Niv. 13)", ZoneOuLieu = "Sanctuaire Solaire", DetailsTaux = "GARANTI (x2 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chronos Maître du Temps (Boss Niv. 15)", ZoneOuLieu = "Sabliers Brisés", DetailsTaux = "GARANTI (x4 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Léviathan des Galaxies (Boss Niv. 16)", ZoneOuLieu = "Voie Lactée Perdue", DetailsTaux = "GARANTI (x5 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Abaddon Pourfendeur de Mondes (Boss Niv. 18)", ZoneOuLieu = "Faille Apocalyptique", DetailsTaux = "GARANTI (x4 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Deus Ex Nihilo (Boss Niv. 20)", ZoneOuLieu = "Trône Cosmique", DetailsTaux = "GARANTI (x6 cristaux)" },
                    new SourceObtentionMateriau { TypeSource = "🏰 Tour Astrale", NomSource = "Paliers majeurs d'étages (10+)", ZoneOuLieu = "Tour Astrale Infinie", DetailsTaux = "Drop précieux sur boss d'étage" }
                }
            });

            // 18. Matière du Néant
            Fiches.Add(new FicheMateriau {
                Nom = "Matière du Néant", Icone = "🕳️", RareteItem = Rarete.Mythique, Categorie = "Néant Primordial",
                Description = "Substance gravitationnelle instable issue de l'antimatière pure. Base de la Lame Tranche-Matière et du sort Annihilation du Néant.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Xanthos Dévoreur Stellaire (Boss Niv. 10)", ZoneOuLieu = "Failles Dimensionnelles", DetailsTaux = "GARANTI (x5 matières)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Chevalier du Néant Primordial (Boss Niv. 10)", ZoneOuLieu = "Abîme Originel", DetailsTaux = "GARANTI (x5 matières)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Abaddon Pourfendeur de Mondes (Boss Niv. 18)", ZoneOuLieu = "Faille Apocalyptique", DetailsTaux = "GARANTI (x4 matières)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Deus Ex Nihilo (Boss Niv. 20)", ZoneOuLieu = "Trône Cosmique", DetailsTaux = "GARANTI (x8 matières)" },
                    new SourceObtentionMateriau { TypeSource = "🏰 Tour Astrale", NomSource = "Sommets ultimes de la Tour (Étages 15+)", ZoneOuLieu = "Tour Astrale", DetailsTaux = "Créatures d'antimatière pure" }
                }
            });

            // 19. Essence du Chaos
            Fiches.Add(new FicheMateriau {
                Nom = "Essence du Chaos", Icone = "👁️", RareteItem = Rarete.Mythique, Categorie = "Divin & Primordial",
                Description = "L'étincelle d'énergie divine primordiale antérieure au temps. Nécessaire pour éveiller l'arme Deus Ex Nihilo et l'Armure Divine de l'Architecte.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "👑 Boss Suprême", NomSource = "Deus Ex Nihilo, Architecte du Chaos Primordial (Boss Niv. 20)", ZoneOuLieu = "Trône Cosmique Suprême", DetailsTaux = "GARANTI (x4 essences divines)" },
                    new SourceObtentionMateriau { TypeSource = "🏆 Tour Astrale", NomSource = "Sommets légendaires (Étages 20+)", ZoneOuLieu = "Tour Astrale Infinie", DetailsTaux = "Récompense de triomphe divin" }
                }
            });

            // 20. Herbes Magiques
            Fiches.Add(new FicheMateriau {
                Nom = "Herbes Magiques", Icone = "🌿", RareteItem = Rarete.Rare, Categorie = "Flore & Alchimie",
                Description = "Plantes médicinales luminescentes gorgées de sève vitale. Ingrédient principal pour concocter les Lots de Potions de Soin et de Mana Majeures.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "🌿 Récolte", NomSource = "Herboristerie après chaque combat", ZoneOuLieu = "Toutes zones & donjons", DetailsTaux = "Taux 50% sur chaque combat victorieux" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Zulgar le Chaman Putride (Boss Niv. 5)", ZoneOuLieu = "Marais Corrompus", DetailsTaux = "GARANTI (x4 herbes)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "Sylvana Reine des Ronces (Boss Niv. 8)", ZoneOuLieu = "Forêt Épineuse Sépulcrale", DetailsTaux = "GARANTI (x8 herbes)" },
                    new SourceObtentionMateriau { TypeSource = "📜 Quêtes", NomSource = "Missions botaniques d'Élénora", ZoneOuLieu = "Village de Val-Serein", DetailsTaux = "Récompense de quête" }
                }
            });

            // 21. Pierres de Forge
            Fiches.Add(new FicheMateriau {
                Nom = "Pierres de Forge", Icone = "💎", RareteItem = Rarete.Epique, Categorie = "Forge & Amélioration",
                Description = "Minéral résonnant indispensable à Brom le Forgeron pour sublimer vos armes et armures du niveau +1 jusqu'au niveau légendaire +10.",
                Sources = new List<SourceObtentionMateriau> {
                    new SourceObtentionMateriau { TypeSource = "⚔️ Monstres", NomSource = "TOUS les monstres réguliers du jeu", ZoneOuLieu = "Toutes les zones et donjons", DetailsTaux = "Taux 25% à 45% (1 pierre)" },
                    new SourceObtentionMateriau { TypeSource = "👑 Boss", NomSource = "TOUS LES BOSS du jeu (Niv. 1 à 20)", ZoneOuLieu = "Toutes les arènes de boss", DetailsTaux = "GARANTI (2 à 5 pierres par boss vaincu !)" },
                    new SourceObtentionMateriau { TypeSource = "♻️ Recyclage", NomSource = "Brom le Forgeron", ZoneOuLieu = "Village de Val-Serein", DetailsTaux = "Démanteler des équipements du sac" },
                    new SourceObtentionMateriau { TypeSource = "📜 Quêtes", NomSource = "Contrats de guilde d'Élénora", ZoneOuLieu = "Village de Val-Serein", DetailsTaux = "Récompense de contrat" }
                }
            });
        }
    }
}
