using System;
using System.Windows.Forms;

namespace JeuxRPG
{
    // ==============================================================
    // POINT D'ENTRÉE DE L'APPLICATION
    // ==============================================================
    // Les modèles sont répartis dans Modeles/, Boss/ et Craft/.
    // L'ancien mode console a été archivé dans _archive/avant_nettoyage_20261004/Program.cs.
    class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormJeu2D());
        }
    }
}
