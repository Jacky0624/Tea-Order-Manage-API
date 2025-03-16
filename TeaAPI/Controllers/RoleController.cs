using Microsoft.AspNetCore.Mvc;
using TeaAPI.Services.Account;
using TeaAPI.Services.Account.Interfaces;

namespace TeaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;


        public RoleController(
            IRoleService roleService)
        {
            _roleService = roleService;

        }

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var res = await _roleService.GetAllRolesAsync();
            return Ok(res);
        }

        [HttpPost("GetRoleById")]
        public async Task<IActionResult> GetByIdAsync([FromBody] int id)
        {
            var res = await _roleService.GetByIdAsync(id);
            return Ok(res);
        }
    }
}
