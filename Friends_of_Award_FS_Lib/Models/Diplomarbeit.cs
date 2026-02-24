using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friends_of_Award_FS_Lib.Models
{
    public class Diplomarbeit
    {
        public int Nr { get; set; }
        public string AbteilungsKuerzel { get; set; }
        public string Titel { get; set; }
        public string Autoren { get; set; }

        public Diplomarbeit(int nr, string abteilungsKuerzel, string titel, string autoren)
        {
            Nr = nr;
            AbteilungsKuerzel = abteilungsKuerzel;
            Titel = titel;
            Autoren = autoren;
        }
    }
}
