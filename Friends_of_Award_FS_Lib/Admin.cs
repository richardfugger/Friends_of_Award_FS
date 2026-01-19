using System.Data;

namespace Friends_of_Award_FS_Lib
{
    public class Admin
    {
        public string Email { get; private set; }
        public string Password { get; private set; }

        public Admin(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public bool CheckAdminLoginData()
        {
            bool isAdmin = false;
            DbWrapperMySqlV2 wrappr = DbWrapperMySqlV2.Wrapper;
            DataTable dt;

            try
            {
                string sql = $"SELECT email, password FROM foaadmins";
                dt = wrappr.RunQuery(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if(Email == dr[0].ToString() && Password == dr[1].ToString())
                    {
                        isAdmin = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                isAdmin = false;
            }

            return isAdmin;

        }
    }
}
