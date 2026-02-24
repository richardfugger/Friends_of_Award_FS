using ClosedXML.Excel;
using Friends_of_Award_FS_Lib.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace Friends_of_Award_FS_Lib.Services
{
    public class DiplomarbeitenService
    {
        public byte[] ExportErgebnisseToExcel()
        {
            string sql = @"
        SELECT 
            d.AbteilungKuerzel,
            d.Titel,
            d.Autoren,
            e.Punkte
        FROM foa_diplomarbeiten d
        JOIN foa_ergebnisse e ON d.Nr = e.DiplomarbeitNr;
    ";

            // Daten über Wrapper holen
            DataTable table = DbWrapperMySqlV2.Wrapper.RunQuery(sql);

            // Excel erzeugen
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ergebnisse");

            // Kopfzeile
            for (int col = 0; col < table.Columns.Count; col++)
            {
                worksheet.Cell(1, col + 1).Value = table.Columns[col].ColumnName;
            }

            // Daten
            for (int row = 0; row < table.Rows.Count; row++)
            {
                for (int col = 0; col < table.Columns.Count; col++)
                {
                    worksheet.Cell(row + 2, col + 1).Value = table.Rows[row][col]?.ToString();
                }
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static readonly HashSet<string> AllowedAbteilungen =
            new(StringComparer.OrdinalIgnoreCase)
{ "MB", "ME", "WII", "WIE", "GT" };

        public (int imported, List<string> errors) ImportAndReplace(Stream excelStream)
        {
            Console.WriteLine("[Import] Method entered");

            var errors = new List<string>();
            var rows = new List<Diplomarbeit>();

            // ======================
            // Excel einlesen
            // ======================
            using (var wb = new XLWorkbook(excelStream))
            {
                var ws = wb.Worksheets.First();
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                for (int r = 2; r <= lastRow; r++)
                {
                    string nrText = ws.Cell(r, 1).GetString().Trim();
                    string abt = ws.Cell(r, 2).GetString().Trim();
                    string titel = ws.Cell(r, 3).GetString().Trim();
                    string autor = ws.Cell(r, 4).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(nrText) &&
                        string.IsNullOrWhiteSpace(abt) &&
                        string.IsNullOrWhiteSpace(titel) &&
                        string.IsNullOrWhiteSpace(autor))
                    {
                        Console.WriteLine("[Import] Continue");
                        continue;
                    }

                    if (!int.TryParse(nrText, out int nr))
                        errors.Add($"Zeile {r}: Ungültige Nr.");

                    if (!AllowedAbteilungen.Contains(abt))
                        errors.Add($"Zeile {r}: Ungültiges AbteilungKürzel ({abt}).");

                    if (string.IsNullOrWhiteSpace(titel))
                        errors.Add($"Zeile {r}: Titel fehlt.");

                    if (string.IsNullOrWhiteSpace(autor))
                        errors.Add($"Zeile {r}: Autor:innen fehlt.");

                    if (errors.Any(e => e.StartsWith($"Zeile {r}:")))
                        continue;

                    Console.WriteLine("[Import] Continue");
                    rows.Add(new(nr, abt.ToUpper(), titel, autor));
                }
            }

            // Doppelte Nr prüfen
            var dupes = rows.GroupBy(r => r.Nr).Where(g => g.Count() > 1).Select(g => g.Key);
            if (dupes.Any())
                errors.Add("Doppelte Nr: " + string.Join(", ", dupes));

            if (errors.Count > 0)
                return (0, errors);

            // ======================
            // DB: Replace All
            // ======================
            var wrpper = DbWrapperMySqlV2.Wrapper;

            try
            {
                wrpper.Open();
                wrpper.RunNonQuery("START TRANSACTION;");
                wrpper.RunNonQuery("DELETE FROM foa_diplomarbeiten;");

                foreach (var r in rows)
                {
                    Console.WriteLine($"[Import]\n {r.Nr}\n{r.AbteilungsKuerzel}\n{r.Titel}\n{r.Autoren}");
                    string sql = $@"
                        INSERT INTO foa_diplomarbeiten (Nr, AbteilungKuerzel, Titel, Autoren)
                        VALUES (
                          {r.Nr},
                          '{MySqlHelper.EscapeString(r.AbteilungsKuerzel)}',
                          '{MySqlHelper.EscapeString(r.Titel)}',
                          '{MySqlHelper.EscapeString(r.Autoren)}'
                        );";

                    wrpper.RunNonQuery(sql);
                }

                wrpper.RunNonQuery("COMMIT;");
            }
            catch
            {
                wrpper.RunNonQuery("ROLLBACK;");
                throw;
            }
            finally
            {
                wrpper.Close();
            }

            return (rows.Count, errors);
        }


        public List<Ergebnis> GetDiplomarbeitenPlusErgebnis()
        {
            string sql = @"
        SELECT 
            e.ErgebnisID,
            d.Nr AS DiplomarbeitNr,
            COALESCE(e.Punkte, 0) AS Punkte,
            d.Nr,
            d.AbteilungKuerzel,
            d.Titel,
            d.Autoren
        FROM foa_diplomarbeiten d
        LEFT JOIN foa_ergebnisse e 
            ON e.DiplomarbeitNr = d.Nr
        ORDER BY Punkte DESC;
    ";

            try
            {
                DataTable table = DbWrapperMySqlV2.Wrapper.RunQuery(sql);

                var result = new List<Ergebnis>();

                foreach (DataRow row in table.Rows)
                {
                    var diplomarbeit = new Diplomarbeit(
                        nr: Convert.ToInt32(row["Nr"]),
                        abteilungsKuerzel: row["AbteilungKuerzel"].ToString()!,
                        titel: row["Titel"].ToString()!,
                        autoren: row["Autoren"].ToString()!
                    );

                    var ergebnis = new Ergebnis(
                        ergebnisID: row["ErgebnisID"] == DBNull.Value ? 0 : Convert.ToInt32(row["ErgebnisID"]),
                        diplomarbeitNr: Convert.ToInt32(row["DiplomarbeitNr"]),
                        diplomarbeit: diplomarbeit,
                        punkte: Convert.ToInt32(row["Punkte"])
                    );

                    result.Add(ergebnis);
                }

                return result;
            }
            catch
            {
                return new();
            }
        }


        public List<(int, string)> GetDiplomarbeitForVoting()
        {
            string sql = @"
        SELECT 
            Nr, Titel
        FROM foa_diplomarbeiten
    ";
            try
            {
                DataTable table = DbWrapperMySqlV2.Wrapper.RunQuery(sql);

                var result = new List<(int, string)>();

                foreach (DataRow row in table.Rows)
                {
                    int nr = Convert.ToInt32(row["Nr"]);
                    string titel = row["Titel"].ToString()!;

                    result.Add((nr, titel));
                }

                return result;
            }
            catch
            {
                return new();
            }
        }
    
    
        public bool SendVote(HashSet<int> favoriteIds, int? topFavoriteId)
        {
            string sql = @"
        INSERT INTO foa_ergebnisse (DiplomarbeitNr, Punkte)
        VALUES ({0}, {1})
        ON DUPLICATE KEY UPDATE
            Punkte = Punkte + {1};
    ";

            try
            {
                foreach (int id in favoriteIds)
                {
                    string formattedSql = string.Format(sql, id, 1);
                    DbWrapperMySqlV2.Wrapper.RunNonQuery(formattedSql);
                }

                // TopFavourite immer extra: +2 Punkte
                if (topFavoriteId.HasValue)
                {
                    string formattedSql = string.Format(sql, topFavoriteId.Value, 2);
                    DbWrapperMySqlV2.Wrapper.RunNonQuery(formattedSql);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
