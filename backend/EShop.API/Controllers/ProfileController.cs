using System.Security.Claims;
using EShop.API.DTOs;
using EShop.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public ProfileController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.FindByIdAsync(GetUserId());
        if(user == null) return NotFound();

        return Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email
        });
    }

    [HttpPut("name")]
    public async Task<IActionResult> UpdateName(UpdateNameDto dto)
    {
        var user = await _userManager.FindByIdAsync(GetUserId());
        if(user == null) return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var result = await _userManager.UpdateAsync(user);
        if(!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email
        });
    }

    [HttpPut("password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(GetUserId());
        if(user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if(!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(new { message = "Hasło zostało zmienione" });
    }
}