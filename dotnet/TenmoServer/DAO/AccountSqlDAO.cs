using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Controllers;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class AccountSqlDAO : IAccountDAO
    {
        private readonly string connectionString;

        public AccountSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }


        public decimal GetBalance(int accountId)
        {
            // Account account = null;
            AccountController userController = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT balance FROM accounts WHERE account_id = @accountId", conn);
                    cmd.Parameters.AddWithValue("@accountId", accountId);
                    //SqlDataReader reader = cmd.ExecuteScalar();

                    return Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return 0;
        }
        public User GetUserById (int userId)
        {
            User user = new User();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM users WHERE user_id = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        user = GetUserFromReader(reader);
                    }
                }

            }
            catch
            {
                throw;
            }
            return user;
        }

        private User GetUserFromReader(SqlDataReader reader)
        {
            User u = new User()
            {
                Username = Convert.ToString(reader["username"]),
                UserId = Convert.ToInt32(reader["user_id"]),

            };
            return u;
        }

        public Account GetAccountById(int userId)
        {
            Account accountFromUserId = new Account();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM accounts WHERE user_id = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        accountFromUserId = GetAccountFromReader(reader);
                    }
                }

            }
            catch
            {
                throw;
            }
            return accountFromUserId;
        }
        public Account GetAccountFromReader(SqlDataReader reader)
        {
            Account a = new Account()
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Balance = Convert.ToDecimal(reader["balance"]),
            };
            return a;
        }
    }
}
