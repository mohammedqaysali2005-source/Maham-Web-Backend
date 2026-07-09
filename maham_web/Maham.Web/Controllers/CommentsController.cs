using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;
using Maham.Application.DTOs.Comment;

namespace Maham.Web.Controllers;

public class CommentsController : Controller
{
    private readonly ApiService _api;

    public CommentsController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/cards/{cid:guid}/comments")]
    public async Task<IActionResult> Index(Guid cid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebCommentDto>>($"/api/cards/{cid}/comments");
        ViewBag.CardId = cid;
        return View(data ?? new List<WebCommentDto>());
    }

    [HttpGet("/comments/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebCommentDto>($"/api/comments/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpGet("/cards/{cid:guid}/comments/create")]
    public IActionResult Create(Guid cid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        ViewBag.CardId = cid;
        return View();
    }

    [HttpPost("/cards/{cid:guid}/comments/create")]
    public async Task<IActionResult> Create(Guid cid, CreateCommentDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CardId = cid;
            return View(model);
        }

        var (ok, data, message) = await _api.PostAsync<WebCommentDto>($"/api/cards/{cid}/comments", model);
        if (ok && data != null)
        {
            TempData["Success"] = "تم إضافة التعليق بنجاح";
            return RedirectToAction("Details", "Cards", new { id = cid });
        }

        ViewBag.Error = message ?? "فشل إضافة التعليق";
        ViewBag.CardId = cid;
        return View(model);
    }

    [HttpGet("/comments/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebCommentDto>($"/api/comments/{id}");
        if (!ok || data == null) return NotFound();

        var model = new UpdateCommentDto { Content = data.Content };
        ViewBag.CommentId = id;
        ViewBag.CardId = data.CardId;
        return View(model);
    }

    [HttpPost("/comments/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdateCommentDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CommentId = id;
            return View(model);
        }

        var (ok, data, message) = await _api.PutAsync<WebCommentDto>($"/api/comments/{id}", model);
        if (ok && data != null)
        {
            TempData["Success"] = "تم تعديل التعليق بنجاح";
            return RedirectToAction("Details", "Cards", new { id = data.CardId });
        }

        ViewBag.Error = message ?? "فشل تعديل التعليق";
        ViewBag.CommentId = id;
        return View(model);
    }

    [HttpGet("/comments/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebCommentDto>($"/api/comments/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpPost("/comments/{id:guid}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (okGet, data, _) = await _api.GetAsync<WebCommentDto>($"/api/comments/{id}");
        var cid = data?.CardId;

        var ok = await _api.DeleteAsync($"/api/comments/{id}");
        if (ok)
        {
            TempData["Success"] = "تم حذف التعليق بنجاح";
            if (cid.HasValue) return RedirectToAction("Details", "Cards", new { id = cid.Value });
            return RedirectToAction("Index", "Projects");
        }

        TempData["Error"] = "فشل حذف التعليق";
        return RedirectToAction("Details", "Cards", new { id = cid });
    }
}
