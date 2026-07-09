using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;
using Maham.Application.DTOs.Project;

namespace Maham.Web.Controllers;

public class MembersController : Controller
{
    private readonly ApiService _api;

    public MembersController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/projects/{pid:guid}/members")]
    public async Task<IActionResult> Index(Guid pid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebMemberDto>>($"/api/projects/{pid}/members");
        ViewBag.ProjectId = pid;
        return View(data ?? new List<WebMemberDto>());
    }

    [HttpGet("/projects/{pid:guid}/members/{uid:guid}")]
    public async Task<IActionResult> Details(Guid pid, Guid uid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebMemberDto>>($"/api/projects/{pid}/members");
        var member = data?.Find(m => m.UserId == uid);
        if (member == null) return NotFound();

        ViewBag.ProjectId = pid;
        return View(member);
    }

    [HttpGet("/projects/{pid:guid}/members/add")]
    public async Task<IActionResult> Add(Guid pid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (_, allUsers, _) = await _api.GetAsync<List<WebUserDto>>("/api/users");
        ViewBag.AllUsers = allUsers ?? new List<WebUserDto>();
        ViewBag.ProjectId = pid;
        return View();
    }

    [HttpPost("/projects/{pid:guid}/members/add")]
    public async Task<IActionResult> Add(Guid pid, AddProjectMemberDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProjectId = pid;
            return View(model);
        }

        var (ok, _, message) = await _api.PostAsync<WebMemberDto>($"/api/projects/{pid}/members", model);
        if (ok)
        {
            TempData["Success"] = "تم إضافة العضو بنجاح للمشروع";
            return RedirectToAction("Index", new { pid });
        }

        ViewBag.Error = message ?? "فشل إضافة العضو للمشروع";
        ViewBag.ProjectId = pid;
        return View(model);
    }

    [HttpGet("/projects/{pid:guid}/members/{uid:guid}/role")]
    public async Task<IActionResult> EditRole(Guid pid, Guid uid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebMemberDto>>($"/api/projects/{pid}/members");
        var member = data?.Find(m => m.UserId == uid);
        if (member == null) return NotFound();

        ViewBag.ProjectId = pid;
        ViewBag.UserId = uid;
        Enum.TryParse<Maham.Domain.Enums.ProjectMemberRole>(member.Role, out var roleEnum);
        var model = new UpdateProjectMemberDto { Role = roleEnum };
        return View(model);
    }

    [HttpPost("/projects/{pid:guid}/members/{uid:guid}/role")]
    public async Task<IActionResult> EditRole(Guid pid, Guid uid, UpdateProjectMemberDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProjectId = pid;
            ViewBag.UserId = uid;
            return View(model);
        }

        var (ok, _, message) = await _api.PutAsync<WebMemberDto>($"/api/projects/{pid}/members/{uid}", model);
        if (ok)
        {
            TempData["Success"] = "تم تحديث دور العضو بنجاح";
            return RedirectToAction("Index", new { pid });
        }

        ViewBag.Error = message ?? "فشل تحديث دور العضو";
        ViewBag.ProjectId = pid;
        ViewBag.UserId = uid;
        return View(model);
    }

    [HttpGet("/projects/{pid:guid}/members/{uid:guid}/remove")]
    public async Task<IActionResult> Remove(Guid pid, Guid uid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebMemberDto>>($"/api/projects/{pid}/members");
        var member = data?.Find(m => m.UserId == uid);
        if (member == null) return NotFound();

        ViewBag.ProjectId = pid;
        return View(member);
    }

    [HttpPost("/projects/{pid:guid}/members/{uid:guid}/remove")]
    public async Task<IActionResult> RemoveConfirmed(Guid pid, Guid uid)
    {
        var ok = await _api.DeleteAsync($"/api/projects/{pid}/members/{uid}");
        if (ok)
        {
            TempData["Success"] = "تم إزالة العضو بنجاح";
            return RedirectToAction("Index", new { pid });
        }

        TempData["Error"] = "فشل إزالة العضو";
        return RedirectToAction("Index", new { pid });
    }
}
