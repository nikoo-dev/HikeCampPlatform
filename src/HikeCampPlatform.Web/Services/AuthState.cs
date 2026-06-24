using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace HikeCampPlatform.Web.Services;

public class AuthState
{
    private readonly IJSRuntime _js;

    public string? Token { get; private set; }
    public int? UserId { get; private set; }
    public string? FullName { get; private set; }
    public string? Role { get; private set; }

    public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    public event Action? OnChange;

    public AuthState(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SetUserAsync(string token, int userId, string fullName, string role)
    {
        Token = token;
        UserId = userId;
        FullName = fullName;
        Role = role;

        await _js.InvokeVoidAsync("localStorage.setItem", "auth_token", token);
        await _js.InvokeVoidAsync("localStorage.setItem", "auth_userId", userId.ToString());
        await _js.InvokeVoidAsync("localStorage.setItem", "auth_fullName", fullName);
        await _js.InvokeVoidAsync("localStorage.setItem", "auth_role", role);

        OnChange?.Invoke();
    }

    public async Task LoadFromStorageAsync()
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_token");

        if (!string.IsNullOrEmpty(token))
        {
            var userIdStr = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_userId");
            var fullName = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_fullName");
            var role = await _js.InvokeAsync<string?>("localStorage.getItem", "auth_role");

            if (int.TryParse(userIdStr, out var userId))
            {
                Token = token;
                UserId = userId;
                FullName = fullName;
                Role = role;
                OnChange?.Invoke();
            }
        }
    }
    public void ApplyAuthHeader(HttpClient http)
    {
        if (!string.IsNullOrEmpty(Token))
    {
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
    }
        else
    {
        http.DefaultRequestHeaders.Authorization = null;
    }
}

    public async Task LogoutAsync()
    {
        Token = null;
        UserId = null;
        FullName = null;
        Role = null;

        await _js.InvokeVoidAsync("localStorage.removeItem", "auth_token");
        await _js.InvokeVoidAsync("localStorage.removeItem", "auth_userId");
        await _js.InvokeVoidAsync("localStorage.removeItem", "auth_fullName");
        await _js.InvokeVoidAsync("localStorage.removeItem", "auth_role");

        OnChange?.Invoke();
    }
}