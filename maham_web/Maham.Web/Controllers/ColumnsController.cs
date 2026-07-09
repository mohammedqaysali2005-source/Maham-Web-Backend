using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;
using Maham.Application.DTOs.Column;

namespace Maham.Web.Controllers;

public class ColumnsController : Controller
{
    private readonly ApiService _api;

    public ColumnsController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/boards/{bid:guid}/columns")]
    public async Task<IActionResult> Index(Guid bid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebBoardDto>($"/api/boards/{bid}");
        ViewBag.BoardId = bid;
        ViewBag.BoardName = data?.Name;
        return View(data?.Columns ?? new List<WebColumnDto>());
    }

    [HttpGet("/columns/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebColumnDto>($"/api/columns/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpGet("/boards/{bid:guid}/columns/create")]
    public IActionResult Create(Guid bid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        ViewBag.BoardId = bid;
        return View();
    }

    [HttpPost("/boards/{bid:guid}/columns/create")]
    public async Task<IActionResult> Create(Guid bid, CreateColumnDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.BoardId = bid;
            return View(model);
        }

        var (ok, _, message) = await _api.PostAsync<WebColumnDto>($"/api/boards/{bid}/columns", model);
        if (ok)
        {
            TempData["Success"] = "تم إضافة العمود بنجاح";
            return RedirectToAction("Details", "Boards", new { id = bid });
        }

        ViewBag.Error = message ?? "فشل إضافة العمود";
        ViewBag.BoardId = bid;
        return View(model);
    }

    [HttpGet("/columns/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebColumnDto>($"/api/columns/{id}");
        if (!ok || data == null) return NotFound();

        var model = new UpdateColumnDto { Name = data.Name, WipLimit = data.WipLimit };
        ViewBag.ColumnId = id;
        ViewBag.BoardId = data.BoardId;
        return View(model);
    }

    [HttpPost("/columns/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdateColumnDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ColumnId = id;
            return View(model);
        }

        var (ok, data, message) = await _api.PutAsync<WebColumnDto>($"/api/columns/{id}", model);
        if (ok && data != null)
        {
            TempData["Success"] = "تم تعديل العمود بنجاح";
            return RedirectToAction("Details", "Boards", new { id = data.BoardId });
        }

        ViewBag.Error = message ?? "فشل تعديل العمود";
        ViewBag.ColumnId = id;
        return View(model);
    }

    [HttpGet("/columns/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebColumnDto>($"/api/columns/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpPost("/columns/{id:guid}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (okGet, data, _) = await _api.GetAsync<WebColumnDto>($"/api/columns/{id}");
        var bid = data?.BoardId;

        var ok = await _api.DeleteAsync($"/api/columns/{id}");
        if (ok)
        {
            TempData["Success"] = "تم حذف العمود بنجاح";
            if (bid.HasValue) return RedirectToAction("Details", "Boards", new { id = bid.Value });
            return RedirectToAction("Index", "Projects");
        }

        TempData["Error"] = "فشل حذف العمود";
        return RedirectToAction("Details", "Boards", new { id = bid });
    }

    [HttpGet("/columns/{id:guid}/cards")]
    public async Task<IActionResult> Cards(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebCardDto>>($"/api/columns/{id}/cards");
        ViewBag.ColumnId = id;
        return View(data ?? new List<WebCardDto>());
    }
}
