using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillJobAI.Api.Models;
using SkillJobAI.Api.Models.Responses;
using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private const string RefreshTokenCookieName =
        "skilljobai_refresh_token";

    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;

    public UsersController(
        IUserService userService,
        IWebHostEnvironment environment
    )
    {
        _userService = userService;
        _environment = environment;
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        return int.TryParse(
            userIdValue,
            out var userId
        )
            ? userId
            : null;
    }

    // GET: /api/users/profile
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(
        typeof(UserResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var user =
            await _userService.GetProfileAsync(
                userId.Value
            );

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(user);
    }

    // PUT: /api/users/profile
    [Authorize]
    [HttpPut("profile")]
    [Consumes("application/json")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request
    )
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var user =
            await _userService.UpdateProfileAsync(
                userId.Value,
                request
            );

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(new
        {
            message =
                "Profile updated successfully.",

            user
        });
    }

    // POST: /api/users/profile-image
    [Authorize]
    [HttpPost("profile-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> UploadProfileImage(
        IFormFile file
    )
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var result =
            await _userService.UploadProfileImageAsync(
                userId.Value,
                file
            );

        if (!result.Success)
        {
            if (result.ErrorMessage ==
                "User not found.")
            {
                return NotFound(new
                {
                    message =
                        result.ErrorMessage
                });
            }

            return BadRequest(new
            {
                message =
                    result.ErrorMessage
            });
        }

        return Ok(new
        {
            message =
                "Profile image uploaded successfully.",

            profileImageUrl =
                result.ProfileImageUrl
        });
    }

    // DELETE: /api/users/profile-image
    [Authorize]
    [HttpDelete("profile-image")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> DeleteProfileImage()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var deleted =
            await _userService.DeleteProfileImageAsync(
                userId.Value
            );

        if (!deleted)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(new
        {
            message =
                "Profile image deleted successfully."
        });
    }

    // POST: /api/users/cv
    [Authorize]
    [HttpPost("cv")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> UploadCv(
        IFormFile file
    )
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var result =
            await _userService.UploadCvAsync(
                userId.Value,
                file
            );

        if (!result.Success)
        {
            if (result.ErrorMessage ==
                "User not found.")
            {
                return NotFound(new
                {
                    message =
                        result.ErrorMessage
                });
            }

            return BadRequest(new
            {
                message =
                    result.ErrorMessage
            });
        }

        return Ok(new
        {
            message =
                "CV uploaded successfully.",

            cvUrl =
                result.CvUrl
        });
    }

    // GET: /api/users/cv
    [Authorize]
    [HttpGet("cv")]
    [Produces("application/pdf")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> DownloadCv()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var cv =
            await _userService.GetCvDownloadAsync(
                userId.Value
            );

        if (cv is null)
        {
            return NotFound(new
            {
                message = "CV not found."
            });
        }

        return PhysicalFile(
            cv.Value.FilePath,
            "application/pdf",
            cv.Value.DownloadName,
            enableRangeProcessing: true
        );
    }

    // DELETE: /api/users/cv
    [Authorize]
    [HttpDelete("cv")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> DeleteCv()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var deleted =
            await _userService.DeleteCvAsync(
                userId.Value
            );

        if (!deleted)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(new
        {
            message =
                "CV deleted successfully."
        });
    }

    // GET: /api/users/export
    [Authorize]
    [HttpGet("export")]
    [Produces("application/zip")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult>
        ExportPersonalData()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var result =
            await _userService
                .ExportPersonalDataAsync(
                    userId.Value
                );

        if (!result.Success ||
            result.ZipBytes is null ||
            string.IsNullOrWhiteSpace(
                result.DownloadName))
        {
            return NotFound(new
            {
                message =
                    result.ErrorMessage ??
                    "User not found."
            });
        }

        return File(
            result.ZipBytes,
            "application/zip",
            result.DownloadName
        );
    }

    // DELETE: /api/users/account
    [Authorize]
    [HttpDelete("account")]
    [Consumes("application/json")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> DeleteAccount(
        [FromBody] DeleteAccountRequest request
    )
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or missing user identity."
            });
        }

        var result =
            await _userService.DeleteAccountAsync(
                userId.Value,
                request
            );

        if (result.Success)
        {
            DeleteRefreshTokenCookie();

            return Ok(new
            {
                message =
                    "Account deleted successfully."
            });
        }

        if (result.ErrorMessage ==
            "User not found.")
        {
            return NotFound(new
            {
                message =
                    result.ErrorMessage
            });
        }

        if (result.ErrorMessage ==
            "The entered password is incorrect.")
        {
            return Unauthorized(new
            {
                message =
                    result.ErrorMessage
            });
        }

        return BadRequest(new
        {
            message =
                result.ErrorMessage ??
                "Account could not be deleted."
        });
    }

    // PUT: /api/users/{id}/role
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/role")]
    [Consumes("application/json")]
    [ProducesResponseType(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden
    )]
    [ProducesResponseType(
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> UpdateUserRole(
        int id,
        [FromBody] UpdateUserRoleRequest request
    )
    {
        var user =
            await _userService.UpdateUserRoleAsync(
                id,
                request
            );

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(new
        {
            message =
                "User role updated successfully.",

            user
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth"
            }
        );
    }
}