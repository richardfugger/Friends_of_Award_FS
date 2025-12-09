using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Friends_of_Award_FS_Lib
{
    public class TokenGenerator
    {
        public static string GenerateToken(int byteLength = 32)
        {
            var randomBytes = new byte[byteLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            string hexString = Convert.ToHexString(randomBytes);

            bool isUniqueToken = CheckTokenUniqueness(hexString);

            return Convert.ToHexString(randomBytes); // 64 Hex-Chars
        }

        private static bool CheckTokenUniqueness(string hexString)
        {
            bool tokenExists = false;
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;
            DataTable dt;

            try
            {
                string sql = $"SELECT token FROM foausers";
                dt = wrappr.RunQuery(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (hexString == dr[0].ToString())
                    {
                        tokenExists = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tokenExists = false;
            }

            return tokenExists;
        }
    }
}
