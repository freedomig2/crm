using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Middleware;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserContext _currentUser;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        AppDbContext dbContext,
        ITokenService tokenService,
        IAuditService auditService,
        ICurrentUserContext currentUser)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register(RegisterRequestDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
        {
            return Conflict(new { message = "User already exists." });
        }

        var user = new AppUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _auditService.LogAsync("Users", user.Id.ToString(), "Register", newValues: $"{{\"Email\":\"{user.Email}\"}}", userId: user.Id);

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEnabled = user.IsEnabled,
            IsLocked = await _userManager.IsLockedOutAsync(user),
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == dto.Email && !x.IsDeleted);
        if (user is null)
        {
            await _auditService.LogAsync("Auth", dto.Email, "LoginFailed", newValues: "{\"Reason\":\"InvalidEmail\"}");
            return Unauthorized(new { message = "Invalid credentials." });
        }

        if (!user.IsEnabled)
        {
            return Forbid();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            await _auditService.LogAsync("Auth", user.Id.ToString(), "LoginFailed", newValues: "{\"Reason\":\"InvalidPasswordOrLocked\"}", userId: user.Id);
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var token = await _tokenService.CreateAccessTokenAsync(user);
        var refresh = _tokenService.CreateRefreshToken();

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refresh.HashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedFromIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("Auth", user.Id.ToString(), "LoginSuccess", userId: user.Id);

        return Ok(await BuildAuthResponseAsync(user, token.AccessToken, refresh.RawToken, token.ExpiresAt));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(RefreshTokenRequestDto dto)
    {
        var hashed = _tokenService.HashToken(dto.RefreshToken);
        var existing = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hashed && x.RevokedAt == null);

        if (existing is null || !existing.IsActive)
        {
            return Unauthorized(new { message = "Invalid refresh token." });
        }

        var newRefresh = _tokenService.CreateRefreshToken();
        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newRefresh.HashedToken;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = newRefresh.HashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedFromIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        var accessToken = await _tokenService.CreateAccessTokenAsync(existing.User);
        await _dbContext.SaveChangesAsync();

        return Ok(await BuildAuthResponseAsync(existing.User, accessToken.AccessToken, newRefresh.RawToken, accessToken.ExpiresAt));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(RefreshTokenRequestDto dto)
    {
        var hashed = _tokenService.HashToken(dto.RefreshToken);
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hashed && x.RevokedAt == null);
        if (token is not null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        if (_currentUser.UserId.HasValue)
        {
            await _auditService.LogAsync("Auth", _currentUser.UserId.Value.ToString(), "Logout", userId: _currentUser.UserId.Value);
        }

        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return Ok(new { message = "If the account exists, a reset link has been generated." });
        }

        var rawToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var hash = _tokenService.HashToken(rawToken);

        _dbContext.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });

        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync("Auth", user.Id.ToString(), "ForgotPassword", userId: user.Id);

        return Ok(new { message = "If the account exists, a reset link has been generated.", resetToken = rawToken });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return BadRequest(new { message = "Invalid request." });
        }

        var hash = _tokenService.HashToken(dto.Token);
        var token = await _dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.TokenHash == hash && x.UsedAt == null && x.ExpiresAt >= DateTime.UtcNow);

        if (token is null)
        {
            return BadRequest(new { message = "Invalid or expired token." });
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        token.UsedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        await _auditService.LogAsync("Users", user.Id.ToString(), "ResetPassword", userId: user.Id);
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _auditService.LogAsync("Users", user.Id.ToString(), "ChangePassword", userId: user.Id);
        return NoContent();
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(AppUser user, string accessToken, string refreshToken, DateTime expiresAt)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _dbContext.RolePermissions
            .Where(x => roles.Contains(x.Role.Name!))
            .Select(x => x.Permission.Name)
            .Distinct()
            .ToListAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEnabled = user.IsEnabled,
                IsLocked = await _userManager.IsLockedOutAsync(user),
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };
    }
}
