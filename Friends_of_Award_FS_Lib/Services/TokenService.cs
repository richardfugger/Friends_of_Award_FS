using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator;

namespace Friends_of_Award_FS_Lib.Services
{
    public class TokenService
    {
        public string GenerateToken(int byteLength = 32)
        {
            byte[] randomBytes;
            bool isUniqueToken = false;
            string hexString = "";

            while (!isUniqueToken)
            {
                randomBytes = new byte[byteLength];

                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);

                hexString = Convert.ToHexString(randomBytes);

                isUniqueToken = CheckTokenUniqueness(hexString);
            }

            return hexString;
        }

        public bool CheckTokenUniqueness(string token)
        {
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;

            try
            {
                token = MySqlHelper.EscapeString(token);

                string sql = $"""
            SELECT COUNT(*) 
            FROM foa_qr_tokens 
            WHERE token = '{token}'
        """;

                var result = wrappr.RunQueryScalar(sql);
                return Convert.ToInt32(result) == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool SaveTokenToDatabase(string token)
        {
            bool success = false;
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;

            try
            {
                string sql = $"INSERT INTO foa_qr_tokens (token) VALUES ('{token}')";

                int numRows = wrappr.RunNonQuery(sql);
                if (numRows != 1) success = false;
                else success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                success = false;
            }

            return success;
        }

        public List<string> LoadTokenFromDb()
        {
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;
            DataTable dt;
            List<string> unusedTokenList = new();

            try
            {
                string sql = $"SELECT token FROM foa_qr_tokens WHERE used = 0";
                dt = wrappr.RunQuery(sql);
                if (dt != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        unusedTokenList.Add(dr[0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                unusedTokenList = new();
            }

            return unusedTokenList;
        }

        public bool MarkAsUsed(string token)
        {
            bool success = false;
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;

            try
            {
                string sql = $"UPDATE foa_qr_tokens SET used = 1 WHERE token = '{token}'";

                int numRows = wrappr.RunNonQuery(sql);
                if (numRows != 1) success = false;
                else success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                success = false;
            }

            return success;
        }

        public bool IsValidUnusedToken(string token)
        {
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;

            string sql = $"""
        SELECT COUNT(*) 
        FROM foa_qr_tokens 
        WHERE token = '{token}'
    """;

            var result = wrappr.RunQueryScalar(sql);

            Console.WriteLine($"[TOKEN CHECK] token={token}, exists={result}");

            return Convert.ToInt32(result) == 1;
        }

        public bool MarkAsVoted(string token)
        {
            bool success = false;
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;

            try
            {
                string sql = $"UPDATE foa_qr_tokens SET voted = 1 WHERE token = '{token}'";

                int numRows = wrappr.RunNonQuery(sql);
                if (numRows != 1) success = false;
                else success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                success = false;
            }

            return success;
        }
    
        public int LoadCountUnusedTokens()
        {
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;

            string sql = "SELECT COUNT(*) FROM foa_qr_tokens WHERE used = 0";

            var result = wrappr.RunQueryScalar(sql);

            if (result == null)
                return 0;

            if (!int.TryParse(result.ToString(), out int unusedToken))
                return 0;

            return unusedToken;
        }
    
        public int LoadCountVotedTokens()
        {
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;

            string sql = "SELECT COUNT(*) FROM foa_qr_tokens WHERE voted = 1";

            var result = wrappr.RunQueryScalar(sql);

            if (result == null)
                return 0;

            if (!int.TryParse(result.ToString(), out int votedTokens))
                return 0;

            return votedTokens;
        }
    }
}
