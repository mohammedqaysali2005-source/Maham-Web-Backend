using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;
using Maham.Application.DTOs.Project;

namespace Maham.Web.Controllers;

public class ProjectsController : Controller
{
    private readonly ApiService _api;

    public ProjectsController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/projects")]
    public async Task<IActionResult> Index(string? search = null)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, message) = await _api.GetAsync<List<WebProjectDto>>("/api/projects");
        if (!ok) TempData["Error"] = message ?? "فشل جلب المشاريع";

        var projects = data ?? new List<WebProjectDto>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            projects = projects.Where(p => 
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                (p.Description != null && p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        ViewBag.Search = search;
        return View(projects);
    }

    [HttpGet("/projects/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (okP, proj, _) = await _api.GetAsync<WebProjectDto>($"/api/projects/{id}");
        if (!okP || proj == null) return NotFound();

        var (_, members, _) = await _api.GetAsync<List<WebMemberDto>>($"/api/projects/{id}/members");
        var (_, boards, _) = await _api.GetAsync<List<WebBoardDto>>($"/api/projects/{id}/boards");
        var (_, activity, _) = await _api.GetAsync<List<WebActivityDto>>($"/api/projects/{id}/activity");
        var (_, dashboard, _) = await _api.GetAsync<WebDashboardDto>($"/api/projects/{id}/dashboard");

        ViewBag.Members = members ?? new List<WebMemberDto>();
        ViewBag.Boards = boards ?? new List<WebBoardDto>();
        ViewBag.Activity = activity ?? new List<WebActivityDto>();
        ViewBag.Dashboard = dashboard ?? new WebDashboardDto();

        return View(proj);
    }

    [HttpGet("/projects/create")]
    public IActionResult Create()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");
        return View();
    }

    [HttpPost("/projects/create")]
    public async Task<IActionResult> Create(CreateProjectDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, data, message) = await _api.PostAsync<WebProjectDto>("/api/projects", model);
        if (ok && data != null)
        {
            TempData["Success"] = "تم إنشاء المشروع بنجاح";
            return RedirectToAction("Details", new { id = data.Id });
        }

        ViewBag.Error = message ?? "فشل إنشاء المشروع";
        return View(model);
    }

    [HttpGet("/projects/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, proj, _) = await _api.GetAsync<WebProjectDto>($"/api/projects/{id}");
        if (!ok || proj == null) return NotFound();

        var model = new UpdateProjectDto { Name = proj.Name, Description = proj.Description };
        ViewBag.ProjectId = id;
        return View(model);
    }

    [HttpPost("/projects/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdateProjectDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, _, message) = await _api.PutAsync<WebProjectDto>($"/api/projects/{id}", model);
        if (ok)
        {
            TempData["Success"] = "تم تحديث المشروع بنجاح";
            return RedirectToAction("Details", new { id });
        }

        ViewBag.Error = message ?? "فشل تحديث المشروع";
        return View(model);
    }

    [HttpGet("/projects/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, proj, _) = await _api.GetAsync<WebProjectDto>($"/api/projects/{id}");
        if (!ok || proj == null) return NotFound();

        return View(proj);
    }

    [HttpPost("/projects/{id:guid}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var ok = await _api.DeleteAsync($"/api/projects/{id}");
        if (ok)
        {
            TempData["Success"] = "تم حذف المشروع بنجاح";
            return RedirectToAction("Index");
        }

        TempData["Error"] = "فشل حذف المشروع";
        return RedirectToAction("Details", new { id });
    }

    [HttpGet("/projects/{id:guid}/overdue")]
    public async Task<IActionResult> Overdue(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, cards, _) = await _api.GetAsync<List<WebCardDto>>($"/api/projects/{id}/overdue");
        ViewBag.ProjectId = id;
        return View(cards ?? new List<WebCardDto>());
    }

    [HttpGet("/projects/{id:guid}/cards")]
    public async Task<IActionResult> Cards(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, cards, _) = await _api.GetAsync<List<WebCardDto>>($"/api/projects/{id}/cards");
        ViewBag.ProjectId = id;
        return View(cards ?? new List<WebCardDto>());
    }

    [HttpGet("/projects/invitations")]
    public async Task<IActionResult> Invitations()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, invitations, message) = await _api.GetAsync<List<WebProjectInvitationDto>>("/api/projects/invitations/pending");
        if (!ok) TempData["Error"] = message ?? "فشل جلب الإشعارات والدعوات المعلقة";

        return View(invitations ?? new List<WebProjectInvitationDto>());
    }

    [HttpPost("/projects/invitations/{memberId:guid}/accept")]
    public async Task<IActionResult> AcceptInvitation(Guid memberId)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, _, message) = await _api.PostAsync<object>($"/api/projects/invitations/{memberId}/accept", new { });
        if (ok)
        {
            TempData["Success"] = "تم قبول الدعوة بنجاح والانضمام للمشروع!";
        }
        else
        {
            TempData["Error"] = message ?? "فشل قبول الدعوة";
        }

        return RedirectToAction("Invitations");
    }

    [HttpPost("/projects/invitations/{memberId:guid}/reject")]
    public async Task<IActionResult> RejectInvitation(Guid memberId)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, _, message) = await _api.PostAsync<object>($"/api/projects/invitations/{memberId}/reject", new { });
        if (ok)
        {
            TempData["Success"] = "تم رفض الدعوة";
        }
        else
        {
            TempData["Error"] = message ?? "فشل رفض الدعوة";
        }

        return RedirectToAction("Invitations");
    }
}
