using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friends_of_Award_FS_Lib.Services
{
    public class DiplomarbeitenExportService
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
    }
}
