using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TenmoServer.DAO;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{


    [Route("[controller]")]
    [ApiController]
    [Authorize]

    public class AccountController : ControllerBase
    {

        private int? GetCurrentUserId()
        {
            string userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return null;
            int.TryParse(userId, out int userIdInt);
            return userIdInt;
        }

        private static IAccountDAO AccountDAO;

        public AccountController(IAccountDAO _accountDAO)
        {
            AccountDAO = _accountDAO;
        }

        [HttpGet("{userId}")]
        public User GetUserById(int userId)
        {
            return AccountDAO.GetUserById(userId);
        }

        [HttpGet/*("{accountId}")*/]
        public decimal GetBalance()
           
        {
            int accountId = (int)GetCurrentUserId();

            return AccountDAO.GetBalance(accountId);
        }
    }
}
