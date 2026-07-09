using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;

namespace Maham.Web.Areas.Admin.Controllers;

public class ProjectsController : AdminBaseController
{
    private readonly ApiService _api;

    public ProjectsController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/admin/projects")]
    public async Task<IActionResult> Index()
    {
        var (ok, projects, message) = await _api.GetAsync<List<WebProjectDto>>("/api/projects");
        if (!ok) TempData["Error"] = message ?? "فشل جلب قائمة المشاريع";
        return View(projects ?? new List<WebProjectDto>());
    }

    [HttpPost("/admin/projects/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _api.DeleteAsync($"/api/projects/{id}");
        if (ok) TempData["Success"] = "تم حذف المشروع بواسطة المسؤول";
        else TempData["Error"] = "فشل حذف المشروع";
        return RedirectToAction("Index");
    }
}
