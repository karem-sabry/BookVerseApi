using BookStoreApi.Constants;
using BookStoreApi.Dtos.User;
using BookStoreApi.Entities;
using BookStoreApi.Enums;
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
    private readonly UserManager<User> _userManager;

    public AdminController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserWithRolesDto>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var usersWithRoles = new List<UserWithRolesDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserWithRolesDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            });
        }

        return Ok(usersWithRoles);
    }
    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(UserWithRolesDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return NotFound("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserWithRolesDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        });
    }

    [HttpPost("users/userId:guid/make-admin")]
    [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MakeUserAdmin(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId. ToString());

        if (user==null)
        {
            return NotFound("User not found.");
        }

        if (await _userManager.IsInRoleAsync(user,IdentityRoleConstants.Admin))
        {
            return BadRequest(new BasicResponse
            {
                Succeeded = false,
                Message = "User is already an admin."
            });
        }

        var result = await _userManager.AddToRoleAsync(user, IdentityRoleConstants.Admin);

        if (!result.Succeeded)
        {
            return BadRequest(new BasicResponse
            {
                Succeeded = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            });
        }

        return Ok(new BasicResponse
        {
            Succeeded = true,
            Message = $"User {user.Email} has been granted Admin role."
        });
    }

    [HttpPost("users/{userId:guid}/remove-admin")]
    [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAdminRole(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound("User not found.");
        }
        //Check if user has Admin role
        if (!await _userManager.IsInRoleAsync(user,IdentityRoleConstants.Admin))
        {
            return BadRequest(new BasicResponse
            {
                Succeeded = false,
                Message = "User is not an Admin."
            });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, IdentityRoleConstants.Admin);
        if (!result.Succeeded)
        {
            return BadRequest(new BasicResponse
            {
                Succeeded = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            });
        }

        return Ok(new BasicResponse
        {
            Succeeded = true,
            Message = $"Admin role removed from user {user.Email}."
        });
    }
    [HttpDelete("users/{userId:guid}")]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(new BasicResponse
            {
                Succeeded = false,
                Message = string.Join(" ,", result.Errors.Select(e => e.Description))
            });
        }

        return Ok(new BasicResponse
        {
            Succeeded = true,
            Message = $"User {user.Email} has been deleted."
        });
    }
}










