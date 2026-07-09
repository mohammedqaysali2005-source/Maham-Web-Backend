using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;

namespace Maham.Web.Areas.Admin.Controllers;

public class UsersController : AdminBaseController
{
    private readonly ApiService _api;

    public UsersController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/admin/users")]
    public async Task<IActionResult> Index()
    {
        var (ok, users, message) = await _api.GetAsync<List<WebUserDto>>("/api/users");
        if (!ok) TempData["Error"] = message ?? "فشل جلب قائمة المستخدمين";
        return View(users ?? new List<WebUserDto>());
    }

    [HttpPost("/admin/users/{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, string role)
    {
        var (ok, _, message) = await _api.PutAsync<WebUserDto>($"/api/users/{id}/role", new { role });
        if (ok)
        {
            TempData["Success"] = "تم تحديث دور المستخدم بنجاح";
        }
        else
        {
            TempData["Error"] = message ?? "فشل تحديث دور المستخدم";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("/admin/users/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _api.DeleteAsync($"/api/users/{id}");
        if (ok)
        {
            TempData["Success"] = "تم حذف المستخدم بنجاح";
        }
        else
        {
            TempData["Error"] = "فشل حذف المستخدم";
        }
        return RedirectToAction("Index");
    }
}
