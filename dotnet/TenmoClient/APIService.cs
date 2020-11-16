using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System;
using TenmoClient.Data;


namespace TenmoClient
{
    public class APIService
    {
        private readonly string API_BASE_URL = "";
        private readonly IRestClient client = new RestClient();
        public APIService(string api_url)
        {
            API_BASE_URL = api_url;
        }


        public decimal GetBalance()
        {
            RestRequest request = new RestRequest(API_BASE_URL + "account");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<decimal> response = client.Get<decimal>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }

            return response.Data; 
        }

        // View your past transfers
        public List<Transfer> GetListOfTransfers()
        {
            RestRequest request = new RestRequest(API_BASE_URL + "transfer/all");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                foreach(Transfer t in response.Data)
                {
                    t.AccountFrom = UserService.GetUserId();
                    t.AccountTo = UserService.GetUserId();
                }
            }

            return response.Data;
        }
        public List<API_User> GetUsers()
        {
            RestRequest request = new RestRequest(API_BASE_URL + "transfer");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<List<API_User>> response = client.Get<List<API_User>>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }

            return response.Data;

        }

        public List<Transfer> GetPendingTransfers()
        {
            RestRequest request = new RestRequest(API_BASE_URL + "transfer/pending");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return response.Data;
        }
        public void TransferFunds(int recipientId, decimal amount)
        {
            Transfer t = new Transfer();
            t.AccountFrom = UserService.GetUserId();
            t.AccountTo = recipientId;
            t.Amount = amount;
            t.TransferTypeId = 2;
            RestRequest request = new RestRequest(API_BASE_URL + "transfer");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            request.AddJsonBody(t);
            IRestResponse response = client.Post(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                Console.WriteLine("Funds transferred successfully");
            }
        }
        public void RequestMoney(int accountFromId, decimal amount)
        {
            Transfer t = new Transfer();
            t.TransferStatusId = 1;
            t.TransferTypeId = 1;
            t.AccountFrom = accountFromId;
            t.Amount = amount;
            RestRequest request = new RestRequest(API_BASE_URL + "transfer/pending");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            request.AddJsonBody(t);
            IRestResponse response = client.Post(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
        }
        public void ReceivePendingRequest(int transferId)
        {
            Transfer t = new Transfer();
            t.TransferId = transferId;
            t.TransferStatusId = 2;
            RestRequest request = new RestRequest(API_BASE_URL + "transfer/pending");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            request.AddJsonBody(t);
            IRestResponse response = client.Put(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                Console.WriteLine("Pending funds sent");
            }
        }
        public void RejectTransferRequest(int transferId)
        {
            Transfer t = new Transfer();
            t.TransferId = transferId;
            t.TransferStatusId = 3;
            RestRequest request = new RestRequest(API_BASE_URL + "transfer/pending/rejected");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            request.AddJsonBody(t);
            IRestResponse response = client.Put(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                Console.WriteLine("Pending transfer rejected.");
            }
        }
        public API_User GetUserById (int userId)
        {
            RestRequest request = new RestRequest(API_BASE_URL + "account/" + userId);
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<API_User> response = client.Get<API_User>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }

            return response.Data;

        }
        private void ProcessErrorResponse(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("Error occurred - unable to reach server.");
            }
            else if (!response.IsSuccessful)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Authorization Require. Please Log In");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("You do not have permissions to do that");
                }
                else
                {
                    Console.WriteLine("Error occurred - received non-success response: " + (int)response.StatusCode);
                }
            }
        }





    }
}
