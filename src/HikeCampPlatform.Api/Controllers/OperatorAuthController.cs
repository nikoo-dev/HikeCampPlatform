using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HikeCampPlatform.Api.Data;
using HikeCampPlatform.Api.Models;
using HikeCampPlatform.Api.DTOs;

namespace HikeCampPlatform.Api.Controllers;

[ApiController]
[Route("api/operator-auth")]
public class OperatorAuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public OperatorAuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<ActionResult<OperatorAuthResponse>> Register([FromBody] OperatorRegisterRequest request)
    {
        var existing = await _db.Operators.FirstOrDefaultAsync(o => o.Email == request.Email);
        if (existing != null)
        {
            return Conflict(new { message = "Email is already registered." });
        }

        var operatorEntity = new Operator
        {
            CompanyName = request.CompanyName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsVerified = false // always starts unverified — admin approves later
        };

        _db.Operators.Add(operatorEntity);
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(operatorEntity);

        return Ok(new OperatorAuthResponse
        {
            Token = token,
            OperatorId = operatorEntity.Id,
            CompanyName = operatorEntity.CompanyName,
            Email = operatorEntity.Email,
            IsVerified = operatorEntity.IsVerified
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<OperatorAuthResponse>> Login([FromBody] OperatorLoginRequest request)
    {
        var operatorEntity = await _db.Operators.FirstOrDefaultAsync(o => o.Email == request.Email);

        if (operatorEntity == null || !BCrypt.Net.BCrypt.Verify(request.Password, operatorEntity.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = GenerateJwtToken(operatorEntity);

        return Ok(new OperatorAuthResponse
        {
            Token = token,
            OperatorId = operatorEntity.Id,
            CompanyName = operatorEntity.CompanyName,
            Email = operatorEntity.Email,
            IsVerified = operatorEntity.IsVerified
        });
    }

    private string GenerateJwtToken(Operator operatorEntity)
    {
        var jwtKey = _config["Jwt:Key"]!;
        var jwtIssuer = _config["Jwt:Issuer"];
        var jwtAudience = _config["Jwt:Audience"];
        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, operatorEntity.Id.ToString()),
            new Claim(ClaimTypes.Email, operatorEntity.Email),
            new Claim(ClaimTypes.Name, operatorEntity.CompanyName),
            new Claim("role", "Operator"),
            new Claim("isVerified", operatorEntity.IsVerified.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}