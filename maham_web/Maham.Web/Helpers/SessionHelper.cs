using System;
using Microsoft.AspNetCore.Http;

namespace Maham.Web.Helpers;

public static class SessionHelper
{
    public static void SetToken(ISession s, string token) => s.SetString("Token", token);
    public static string? GetToken(ISession s) => s.GetString("Token");
    public static void SetUserId(ISession s, Guid id) => s.SetString("UserId", id.ToString());
    public static Guid? GetUserId(ISession s)
    {
        var v = s.GetString("UserId");
        return v == null ? null : Guid.Parse(v);
    }
    public static void SetUserName(ISession s, string name) => s.SetString("UserName", name);
    public static string? GetUserName(ISession s) => s.GetString("UserName");
    public static void SetUserRole(ISession s, string role) => s.SetString("UserRole", role);
    public static string? GetUserRole(ISession s) => s.GetString("UserRole");
    public static void Clear(ISession s) => s.Clear();
    public static bool IsLoggedIn(ISession s) => !string.IsNullOrEmpty(GetToken(s));
    public static bool IsAdmin(ISession s) => GetUserRole(s)?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
}
