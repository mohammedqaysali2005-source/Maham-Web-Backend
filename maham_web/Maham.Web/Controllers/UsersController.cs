using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;

namespace Maham.Web.Controllers;

public class UsersController : Controller
{
    private readonly ApiService _api;
    private readonly IHttpClientFactory _httpFactory;

    public UsersController(ApiService api, IHttpClientFactory httpFactory)
    {
        _api = api;
        _httpFactory = httpFactory;
    }

    [HttpGet("/users")]
    public async Task<IActionResult> Index(string? q = null)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        string url = string.IsNullOrWhiteSpace(q)
            ? "/api/users"
            : $"/api/users/search?q={Uri.EscapeDataString(q)}";

        var (ok, data, _) = await _api.GetAsync<List<WebUserDto>>(url);
        ViewBag.SearchQuery = q;
        return View(data ?? new List<WebUserDto>());
    }

    [HttpGet("/users/profile")]
    [HttpGet("/users/{id:guid}")]
    public async Task<IActionResult> Details(Guid? id = null)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var currentUserId = SessionHelper.GetUserId(HttpContext.Session);
        var targetId = id ?? currentUserId;
        if (!targetId.HasValue) return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebUserDto>($"/api/users/{targetId.Value}");
        if (!ok || data == null) return NotFound();

        var (_, cards, _) = await _api.GetAsync<List<WebCardDto>>("/api/cards/assigned-to-me");
        var (_, projects, _) = await _api.GetAsync<List<WebProjectDto>>("/api/projects");
        var (_, activities, _) = await _api.GetAsync<List<WebActivityDto>>($"/api/activity/user/{targetId.Value}");

        ViewBag.Cards = cards ?? new List<WebCardDto>();
        ViewBag.Projects = projects ?? new List<WebProjectDto>();
        ViewBag.Activities = activities ?? new List<WebActivityDto>();
        ViewBag.IsOwnProfile = targetId.Value == currentUserId;

        return View("Details", data);
    }

    [HttpGet("/users/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebUserDto>($"/api/users/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpPost("/users/{id:guid}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var ok = await _api.DeleteAsync($"/api/users/{id}");
        if (ok)
        {
            TempData["Success"] = "تم حذف المستخدم بنجاح";
            return RedirectToAction("Index");
        }

        TempData["Error"] = "فشل حذف المستخدم";
        return RedirectToAction("Details", new { id });
    }

    [HttpGet("/users/{id:guid}/avatar")]
    public IActionResult Avatar(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        ViewBag.UserId = id;
        return View();
    }

    [HttpPost("/users/{id:guid}/avatar")]
    public async Task<IActionResult> Avatar(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "يرجى اختيار صورة صالحة";
            ViewBag.UserId = id;
            return View();
        }

        using var form = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        form.Add(new System.Net.Http.StreamContent(stream), "file", file.FileName);

        var (ok, _, message) = await _api.PostFormAsync<WebUserDto>($"/api/users/{id}/avatar", form);
        if (ok)
        {
            TempData["Success"] = "تم تحديث الصورة الشخصية بنجاح";
            return RedirectToAction("Details", new { id });
        }

        ViewBag.Error = message ?? "فشل رفع الصورة";
        ViewBag.UserId = id;
        return View();
    }

    [HttpGet("/users/create")]
    public IActionResult Create()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        return View();
    }

    [HttpPost("/users/create")]
    public async Task<IActionResult> Create(Maham.Application.DTOs.Auth.RegisterDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, data, message) = await _api.PostAsync<WebAuthResponseDto>("/api/auth/register", model);
        if (ok && data != null)
        {
            TempData["Success"] = "تم إنشاء المستخدم بنجاح";
            return RedirectToAction("Index");
        }

        ViewBag.Error = message ?? "فشل إنشاء المستخدم";
        return View(model);
    }

    [HttpGet("/users/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebUserDto>($"/api/users/{id}");
        if (!ok || data == null) return NotFound();

        var model = new Maham.Application.DTOs.Auth.UpdateProfileDto
        {
            FullName = data.FullName,
            AvatarUrl = data.AvatarUrl
        };
        ViewBag.UserId = id;
        return View(model);
    }

    [HttpPost("/users/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, Maham.Application.DTOs.Auth.UpdateProfileDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.UserId = id;
            return View(model);
        }

        var (ok, _, message) = await _api.PutAsync<WebUserDto>($"/api/auth/me", model);
        if (ok)
        {
            TempData["Success"] = "تم تحديث بيانات المستخدم بنجاح";
            return RedirectToAction("Details", new { id });
        }

        ViewBag.Error = message ?? "فشل تحديث بيانات المستخدم";
        ViewBag.UserId = id;
        return View(model);
    }
}
