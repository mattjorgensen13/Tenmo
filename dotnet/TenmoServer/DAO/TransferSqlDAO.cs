using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class TransferSqlDAO : ITransferDAO
    {
        private readonly string connectionString;
        static TransferStatuses status = new TransferStatuses();
        public TransferSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public List<User> GetUsers()
        {
            List<User> allUsers = new List<User>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT * FROM users", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        User u = ConvertReaderToUser(reader);
                        allUsers.Add(u);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return allUsers;
        }

        private User ConvertReaderToUser(SqlDataReader reader)
        {
            User u = new User();
            u.UserId = Convert.ToInt32(reader["user_id"]);
            u.Username = Convert.ToString(reader["username"]);
            return u;
        }
        //"UPDATE accounts SET transfer_status_id =2 WHERE transfer_id = @transferid", conn);
        public decimal TransferFunds(decimal amountToTransfer, Account sender, Account receiver, int userId)//sending money as a sender successfully
        {

            if (sender.Balance >= amountToTransfer && userId == sender.AccountId && amountToTransfer > 0)
            {

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand("UPDATE accounts SET balance = (balance-@amount) WHERE user_id = @senderId; " + // =balance-amounttoTransfer
                                                        "UPDATE accounts SET balance = (balance + @amount) WHERE user_id = @receiverId; " +
                                                        "INSERT INTO transfers (transfer_type_id, transfer_status_id, account_from, account_to, amount) VALUES (2, 2, @senderId, @receiverId, @amount)", conn);//hard coded type id

                        cmd.Parameters.AddWithValue("@senderId", sender.AccountId);
                        cmd.Parameters.AddWithValue("@receiverId", receiver.AccountId);
                        cmd.Parameters.AddWithValue("@amount", amountToTransfer);
                        SqlDataReader reader = cmd.ExecuteReader();
                    }
                    sender.Balance -= amountToTransfer;
                    receiver.Balance += amountToTransfer;
                }
                catch (SqlException)
                {
                    status.TransferStatusId = 3;
                    throw;
                }
            }
            return sender.Balance;
        }

        public void ReceivePendingRequest(decimal amountToTransfer, Account sender, Account receiver, int userId, int transferId)
        {
            
            if (sender.Balance >= amountToTransfer && amountToTransfer > 0)
            {

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand("UPDATE accounts SET balance = (balance-@amount) WHERE user_id = @senderId; " + // =balance-amounttoTransfer
                                                        "UPDATE accounts SET balance = (balance + @amount) WHERE user_id = @receiverId; " +
                                                        "UPDATE transfers SET transfer_status_id = 2 WHERE transfer_id = @transferid;", conn) ;//hard coded type id

                        cmd.Parameters.AddWithValue("@senderId", sender.AccountId);
                        cmd.Parameters.AddWithValue("@receiverId", receiver.AccountId);
                        cmd.Parameters.AddWithValue("@amount", amountToTransfer);
                        cmd.Parameters.AddWithValue("@transferid", transferId);
                        int rows= cmd.ExecuteNonQuery();
                    }
                    sender.Balance -= amountToTransfer;
                    receiver.Balance += amountToTransfer;
                }
                catch (SqlException)
                {
                    status.TransferStatusId = 3;
                    throw;
                }
            }
            //return sender.Balance;
        }

        public void RequestMoney(decimal amountToTransfer, Account sender, Account receiver, int userId)
        {

             status.TransferStatusId = 1;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO transfers (transfer_type_id, transfer_status_id, account_from, account_to, amount) VALUES (1, 1, @senderId, @receiverId, @amount)", conn);//hard coded type id
                    cmd.Parameters.AddWithValue("@newBalance", sender.Balance);
                    cmd.Parameters.AddWithValue("@newBalance2", receiver.Balance);
                    cmd.Parameters.AddWithValue("@senderId", sender.AccountId);
                    cmd.Parameters.AddWithValue("@receiverId", receiver.AccountId);
                    cmd.Parameters.AddWithValue("@amount", amountToTransfer);
                    SqlDataReader reader = cmd.ExecuteReader();
                }
            }
            catch (SqlException)
            {
                status.TransferStatusId = 3;
                throw;
            }

        }

        public List<Transfer> GetPendingTransfers(int userId)
        {
            List<Transfer> pendingTransfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM transfers WHERE transfer_status_id = 1 AND account_from = @userId; ", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Transfer t = ConvertReaderToTransfer(reader);
                        pendingTransfers.Add(t);
                    }
                }
            }
            catch
            {
                throw;
            }
            return pendingTransfers;
        }
        public void RejectTransferRequest(Transfer t)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE transfers SET transfer_status_id = 3 WHERE transfer_id = @transferId;", conn);
                    cmd.Parameters.AddWithValue("@transferId", t.TransferId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
        }
        public List<Transfer> GetListOfTransfers(int userId)
        {
            List<Transfer> allTransfer = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM transfers WHERE account_from= @userId OR account_to= @userId ", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Transfer t = ConvertReaderToTransfer(reader);
                        allTransfer.Add(t);
                    }
                }
            }
            catch
            {
                throw;
            }
            return allTransfer;
        }

        public Transfer GetDetailsOfTransfer(int transferId) //fix formatting in printtransferdetail()
        {
            Transfer transfer = new Transfer();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM transfers WHERE transfer_id = @transferId", conn);
                    cmd.Parameters.AddWithValue("@transferId", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        transfer = ConvertReaderToTransfer(reader);
                        return transfer;
                    }
                }
            }
            catch
            {
                throw;
            }
            return transfer;
        }

            private Transfer ConvertReaderToTransfer(SqlDataReader reader)
            {
            Transfer t = new Transfer();
            t.TransferId = Convert.ToInt32(reader["transfer_id"]);
            t.TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]);
            t.TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]);
            t.AccountFrom = Convert.ToInt32(reader["account_from"]);
            t.AccountTo = Convert.ToInt32(reader["account_to"]);
            t.Amount = Convert.ToDecimal(reader["amount"]);
            return t;
             }

        public IEnumerable<TransferUserDetails> GetListOfTransfers()
        {
            throw new NotImplementedException();
        }
    }
}

