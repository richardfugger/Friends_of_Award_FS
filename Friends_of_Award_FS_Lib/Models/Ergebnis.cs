using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friends_of_Award_FS_Lib.Models
{
    public class Ergebnis
    {
        public int ErgebnisID { get; set; }

        public int DiplomarbeitNr { get; set; }
        public Diplomarbeit Diplomarbeit { get; set; }

        public int Punkte { get; set; }

        public Ergebnis(int ergebnisID, int diplomarbeitNr, Diplomarbeit diplomarbeit, int punkte)
        {
            ErgebnisID = ergebnisID;
            DiplomarbeitNr = diplomarbeitNr;
            Diplomarbeit = diplomarbeit;
            Punkte = punkte;
        }
    }
}
