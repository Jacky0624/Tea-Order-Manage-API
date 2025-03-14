using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeaAPI.Models.Requests.Users;
using TeaAPI.Models.Responses;
using TeaAPI.Services.Account.Interfaces;

namespace TeaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "CanManageAccounts")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                string createUser = GetUser();
                var res = await _userService.CreateUserAsync(request.Username, request.Account, request.Password, request.RoleId, createUser);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseBase()
                {
                    ResultCode = -999,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("ModifyUser")]
        public async Task<IActionResult> ModifyUserAsync(ModifyUserRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string user = userIdClaim ?? "System";
                var res = await _userService.ModifyUserAsync(request.Id, request.Username, request.RoleId, user);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseBase()
                {
                    ResultCode = -999,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUserAsync(DeleteUserRequest request)
        {
            try
            {
                string user = GetUser();
                if(user == request.UserId.ToString())
                {
                    return Ok(new ResponseBase()
                    {
                        ResultCode = -1,
                        Errors = new List<string>() { "can not delete self"}
                    });
                }
                await _userService.DeleteUserAsync(request.UserId, user);
                return Ok(new ResponseBase() { ResultCode = 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
               
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("GetUserById")]  
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByIdAsync([FromBody]int userId)
        {
            try
            {
                var users = await _userService.GetByIdAsync(userId);

                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim ?? "System";
        }
    }
}
