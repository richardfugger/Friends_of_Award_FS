using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        public static bool SaveTokenToDatabase(string token)
        {
            bool success = false;
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;

            try
            {
                string sql = $"INSERT INTO foausers (token) VALUES ('{token}')";

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
