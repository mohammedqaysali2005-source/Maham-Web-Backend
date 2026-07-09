using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;
using Maham.Application.DTOs.Auth;

namespace Maham.Web.Controllers;

public class AuthController : Controller
{
    private readonly ApiService _api;

    public AuthController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/auth/login")]
    public IActionResult Login()
    {
        if (SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, data, message) = await _api.PostAsync<WebAuthResponseDto>("/api/auth/login", model);
        if (ok && data != null)
        {
            SessionHelper.SetToken(HttpContext.Session, data.Token);
            SessionHelper.SetUserId(HttpContext.Session, data.User.Id);
            SessionHelper.SetUserName(HttpContext.Session, data.User.FullName);
            SessionHelper.SetUserRole(HttpContext.Session, data.User.Role);
            TempData["Success"] = "تم تسجيل الدخول بنجاح";
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = message ?? "اسم المستخدم أو كلمة المرور غير صحيحة";
        return View(model);
    }

    [HttpGet("/auth/register")]
    public IActionResult Register()
    {
        if (SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost("/auth/register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, data, message) = await _api.PostAsync<WebAuthResponseDto>("/api/auth/register", model);
        if (ok && data != null)
        {
            SessionHelper.SetToken(HttpContext.Session, data.Token);
            SessionHelper.SetUserId(HttpContext.Session, data.User.Id);
            SessionHelper.SetUserName(HttpContext.Session, data.User.FullName);
            SessionHelper.SetUserRole(HttpContext.Session, data.User.Role);
            TempData["Success"] = "تم إنشاء الحساب وتسجيل الدخول بنجاح";
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = message ?? "فشل إنشاء الحساب، يرجى التحقق من البيانات";
        return View(model);
    }

    [HttpGet("/auth/profile")]
    public async Task<IActionResult> Profile()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login");

        var (ok, user, message) = await _api.GetAsync<WebUserDto>("/api/auth/me");
        if (ok && user != null)
        {
            var updateModel = new UpdateProfileDto
            {
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl
            };
            return View(updateModel);
        }

        TempData["Error"] = message ?? "حدث خطأ أثناء جلب الملف الشخصي";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost("/auth/profile")]
    public async Task<IActionResult> Profile(UpdateProfileDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, user, message) = await _api.PutAsync<WebUserDto>("/api/auth/me", model);
        if (ok && user != null)
        {
            SessionHelper.SetUserName(HttpContext.Session, user.FullName);
            TempData["Success"] = "تم تحديث الملف الشخصي بنجاح";
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = message ?? "فشل تحديث الملف الشخصي";
        return View(model);
    }

    [HttpGet("/auth/logout")]
    public IActionResult Logout()
    {
        SessionHelper.Clear(HttpContext.Session);
        TempData["Success"] = "تم تسجيل الخروج بنجاح";
        return RedirectToAction("Index", "Home");
    }
}
