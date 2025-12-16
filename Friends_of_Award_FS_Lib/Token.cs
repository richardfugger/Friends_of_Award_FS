using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator;

namespace Friends_of_Award_FS_Lib
{
    public class Token
    {
        public static string GenerateToken(int byteLength = 32)
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

        private static bool CheckTokenUniqueness(string hexString)
        {
            bool tokenIsUnique = true;
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;
            DataTable dt;

            try
            {
                string sql = $"SELECT token FROM foa_qr_tokens";
                dt = wrappr.RunQuery(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (hexString == dr[0].ToString())
                    {
                        tokenIsUnique = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tokenIsUnique = false;
            }

            return tokenIsUnique;
        }

        public static bool SaveTokenToDatabase(string token)
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

        public static List<string> LoadTokenFromDb()
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

        public static bool MarkAsUsed(string token)
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
    }
}
