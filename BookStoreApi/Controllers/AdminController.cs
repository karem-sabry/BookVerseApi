using System.Security.Claims;
using BookStoreApi.Constants;
using BookStoreApi.Dtos.User;
using BookStoreApi.Entities;
using BookStoreApi.Enums;
using BookStoreApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles= "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserWithRolesDto>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }
    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(UserWithRolesDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _adminService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }

    [HttpPost("users/{userId:guid}/make-admin")]
    [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MakeUserAdmin(Guid userId)
    {
        var currentAdminEmail = User.FindFirstValue(ClaimTypes.Email) ?? "";
        var response = await _adminService.MakeUserAdminAsync(userId, currentAdminEmail);

        if (response.Succeeded)
            return Ok(response);

        return BadRequest(response);
    }

    [HttpPost("users/{userId:guid}/remove-admin")]
    [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAdminRole(Guid userId)
    {
        var response = await _adminService.RemoveAdminRoleAsync(userId);

        if (response.Succeeded)
            return Ok(response);

        return BadRequest(response);
    }
    [HttpDelete("users/{userId:guid}")]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        var currentAdminEmail = User.FindFirstValue(ClaimTypes.Email) ?? "";
        var response = await _adminService.DeleteUserAsync(userId, currentAdminEmail);

        if (response.Succeeded)
            return Ok(response);

        return BadRequest(response);
    }
}










