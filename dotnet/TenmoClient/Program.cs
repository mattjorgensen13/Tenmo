using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using TenmoClient.Data;

namespace TenmoClient
{
    class Program
    {
        private static readonly ConsoleService consoleService = new ConsoleService();
        private static readonly AuthService authService = new AuthService();
        private readonly static APIService API_BASE_URL = new APIService("https://localhost:44315/");
        private static readonly LoginUser loggedInUser = new LoginUser();
        
        static void Main(string[] args)
        {
            Run();
        }
        private static void Run()
        {
            int loginRegister = -1;
            while (loginRegister != 1 && loginRegister != 2)
            {
                Console.WriteLine("Welcome to TEnmo!");
                Console.WriteLine("1: Login");
                Console.WriteLine("2: Register");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out loginRegister))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (loginRegister == 1)
                {
                    while (!UserService.IsLoggedIn()) //will keep looping until user is logged in
                    {
                        LoginUser loginUser = consoleService.PromptForLogin();
                        API_User user = authService.Login(loginUser);
                        if (user != null)
                        {
                            UserService.SetLogin(user);
                        }
                    }
                }
                else if (loginRegister == 2)
                {
                    bool isRegistered = false;
                    while (!isRegistered) //will keep looping until user is registered
                    {
                        LoginUser registerUser = consoleService.PromptForLogin();
                        isRegistered = authService.Register(registerUser);
                        if (isRegistered)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Registration successful. You can now log in.");
                            loginRegister = -1; //reset outer loop to allow choice for login
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }

            MenuSelection();
        }

        private static void MenuSelection()
        {
            int menuSelection = -1;
            while (menuSelection != 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Welcome to TEnmo! Please make a selection: ");
                Console.WriteLine("1: View your current balance");
                Console.WriteLine("2: View your past transfers");
                Console.WriteLine("3: View your pending requests");
                Console.WriteLine("4: Send TE bucks");
                Console.WriteLine("5: Request TE bucks");
                Console.WriteLine("6: Log in as different user");
                Console.WriteLine("0: Exit");
                Console.WriteLine("---------");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out menuSelection))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (menuSelection == 1)
                {
                    Console.WriteLine("Your current balance is: $" + API_BASE_URL.GetBalance());
                }
                else if (menuSelection == 2)
                {
                    PrintTransferDetails(API_BASE_URL.GetListOfTransfers());//list user's names
                }
                else if (menuSelection == 3)
                {

                    PrintPendingRequests(API_BASE_URL.GetPendingTransfers());
                    List<int> pendingTransferIds = new List<int>();
                    foreach (Transfer t in API_BASE_URL.GetPendingTransfers())
                    {
                        pendingTransferIds.Add(t.TransferId);
                    }
                    int transferId = PromptForTransferId(pendingTransferIds);
                    if (transferId != 0)
                    {
                        int acceptOrReject = AcceptReject();
                        if (acceptOrReject == 1)
                        {
                            API_BASE_URL.ReceivePendingRequest(transferId);
                        }
                        else if (acceptOrReject == 2)
                        {
                            API_BASE_URL.RejectTransferRequest(transferId);
                        }
                    }

                }
                else if (menuSelection == 4)
                {
                    PrintTEBucks(API_BASE_URL.GetUsers());
                    int id = PromptForUserIDSelection();
                    decimal amount = 0;
                    if (id != 0)
                    {
                        amount = PromptForAmountSelection();
                    }
                    API_BASE_URL.TransferFunds(id, amount);
                }
                else if (menuSelection == 5)
                {
                    PrintTEBucks(API_BASE_URL.GetUsers());
                    int id = PromptForUserIDSelection();
                    decimal amount = 0;
                    if (id != 0)
                    {
                        amount = PromptForAmountSelection();
                        API_BASE_URL.RequestMoney(id, amount);
                    }
                }
                else if (menuSelection == 6)
                {
                    Console.WriteLine("");
                    UserService.SetLogin(new API_User()); //wipe out previous login info
                    Run(); //return to entry point
                }
                else
                {
                    Console.WriteLine("Goodbye!");
                    Environment.Exit(0);
                }
            }
        }
        private static int PromptForTransferId(List<int> pendingTransferIds)
        {
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid input.");
                Console.WriteLine("Please enter transfer ID to approve/reject (0 to cancel): ");
                Console.ReadLine();
            }
            return id;
        }

        public static int AcceptReject()
        {
            Console.WriteLine("1: Approve");
            Console.WriteLine("2: Reject");
            Console.WriteLine("0: Don't approve or reject");
            Console.WriteLine("---------");
            Console.WriteLine("Please choose an option:");
            int[] options = { 0, 1, 2 };
            int menuId;
            while (!int.TryParse(Console.ReadLine(), out menuId) || !options.Contains(menuId))
            {
                Console.WriteLine("Invalid input.");
                Console.WriteLine("Enter ID of user you are sending to (0 to cancel): ");
                Console.ReadLine();
            }

            return menuId;
        }

        private static int PromptForUserIDSelection()
        {
            Console.WriteLine("Enter ID of user you are sending to (0 to cancel): ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid input.");
                Console.WriteLine("Enter ID of user you are sending to (0 to cancel): ");
                Console.ReadLine();
            }

            return id;
        }

        private static decimal PromptForAmountSelection()
        {
            Console.WriteLine("Enter amount: ");
            decimal amount;
            while (!decimal.TryParse(Console.ReadLine(), out amount))
            {
                Console.WriteLine("Invalid input.");
                Console.WriteLine("Enter amount: ");
                Console.ReadLine();
            }
            return amount;
        }

        public static void PrintTransferDetails(List<Transfer> allTransfers)
        {
            foreach (Transfer t in allTransfers)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("Transfer Details"); //change to TransfersID, add From/To, add Amount
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("Id: " + t.TransferId);
                Console.WriteLine("From: " + t.AccountFrom);
                Console.WriteLine("To: " + t.AccountTo);
                Console.WriteLine("Type: " + t.TransferTypeId); //don't need this
                Console.WriteLine("Status: " + t.TransferStatusId); //don't need this
                Console.WriteLine("Amount: " + t.Amount);
                Console.WriteLine("-----------------------------------------");
            }
            Console.WriteLine("please enter transfer ID to view details (0 to cancel): ");
        }
        public static void PrintPendingRequests(List<Transfer> pendingTransfers)
        {
            List<API_User> users = new List<API_User>();
            foreach(Transfer t in pendingTransfers)
            {
                users.Add(API_BASE_URL.GetUserById(t.AccountTo));
            }
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Pending Transfers");
            Console.WriteLine($"{"ID", -10}{"To", -20}{"Amount", -10}");
            Console.WriteLine("--------------------------------------------");
            for (int i = 0; i < users.Count; i++)
            {
                Console.WriteLine($"{pendingTransfers[i].TransferId, -10} {users[i].Username, -20}{pendingTransfers[i].Amount, -10}");
            }
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("Please enter transfer ID to approve/reject (0 to cancel): ");
        }
        public static void PrintTEBucks(List<API_User> allUsers)
        {
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"{"UsersID",-10}{"Name", -10}");
            Console.WriteLine("--------------------------------------------");
            foreach (API_User user in allUsers)
            {
                Console.WriteLine($"{user.UserId, -10}{user.Username, -10}");
            }
            Console.WriteLine("----------------------------------------------");
        }
    }
}
