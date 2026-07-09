using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;
using Maham.Application.DTOs.Board;

namespace Maham.Web.Controllers;

public class BoardsController : Controller
{
    private readonly ApiService _api;

    public BoardsController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/projects/{pid:guid}/boards")]
    public async Task<IActionResult> Index(Guid pid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, message) = await _api.GetAsync<List<WebBoardDto>>($"/api/projects/{pid}/boards");
        ViewBag.ProjectId = pid;
        return View(data ?? new List<WebBoardDto>());
    }

    [HttpGet("/boards/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebBoardDto>($"/api/boards/{id}");
        if (!ok || data == null) return NotFound();

        // Also fetch project members for assigning task responsible
        var (_, members, _) = await _api.GetAsync<List<WebMemberDto>>($"/api/projects/{data.ProjectId}/members");
        ViewBag.Members = members ?? new List<WebMemberDto>();

        return View(data);
    }

    [HttpGet("/boards/create/{pid:guid}")]
    public IActionResult Create(Guid pid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        ViewBag.ProjectId = pid;
        return View();
    }

    [HttpPost("/boards/create/{pid:guid}")]
    public async Task<IActionResult> Create(Guid pid, CreateBoardDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ProjectId = pid;
            return View(model);
        }

        var (ok, data, message) = await _api.PostAsync<WebBoardDto>($"/api/projects/{pid}/boards", model);
        if (ok && data != null)
        {
            TempData["Success"] = "تم إنشاء اللوحة بنجاح";
            return RedirectToAction("Details", "Projects", new { id = pid });
        }

        ViewBag.Error = message ?? "فشل إنشاء اللوحة";
        ViewBag.ProjectId = pid;
        return View(model);
    }

    [HttpGet("/boards/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebBoardDto>($"/api/boards/{id}");
        if (!ok || data == null) return NotFound();

        var model = new UpdateBoardDto
        {
            Name = data.Name,
            Description = data.Description,
            BackgroundColor = data.BackgroundColor
        };
        ViewBag.BoardId = id;
        ViewBag.ProjectId = data.ProjectId;
        return View(model);
    }

    [HttpPost("/boards/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdateBoardDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.BoardId = id;
            return View(model);
        }

        var (ok, data, message) = await _api.PutAsync<WebBoardDto>($"/api/boards/{id}", model);
        if (ok && data != null)
        {
            TempData["Success"] = "تم تعديل اللوحة بنجاح";
            return RedirectToAction("Details", new { id });
        }

        ViewBag.Error = message ?? "فشل تعديل اللوحة";
        ViewBag.BoardId = id;
        return View(model);
    }

    [HttpGet("/boards/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebBoardDto>($"/api/boards/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpPost("/boards/{id:guid}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (okGet, data, _) = await _api.GetAsync<WebBoardDto>($"/api/boards/{id}");
        var pid = data?.ProjectId;

        var ok = await _api.DeleteAsync($"/api/boards/{id}");
        if (ok)
        {
            TempData["Success"] = "تم حذف اللوحة بنجاح";
            if (pid.HasValue) return RedirectToAction("Details", "Projects", new { id = pid.Value });
            return RedirectToAction("Index", "Projects");
        }

        TempData["Error"] = "فشل حذف اللوحة";
        return RedirectToAction("Details", new { id });
    }

    [HttpGet("/boards/{id:guid}/activity")]
    public async Task<IActionResult> Activity(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebActivityDto>>($"/api/boards/{id}/activity");
        ViewBag.BoardId = id;
        return View(data ?? new List<WebActivityDto>());
    }
}
