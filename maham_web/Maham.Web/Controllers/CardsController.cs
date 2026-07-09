using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;
using Maham.Application.DTOs.Card;

namespace Maham.Web.Controllers;

public class CardsController : Controller
{
    private readonly ApiService _api;

    public CardsController(ApiService api)
    {
        _api = api;
    }

    [HttpGet("/cards")]
    public async Task<IActionResult> Index(string? search = null, string? priority = null, string? status = null, string? due = null)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebCardDto>>("/api/cards/assigned-to-me");
        var cards = data ?? new List<WebCardDto>();

        // Filter by search text (title or description)
        if (!string.IsNullOrWhiteSpace(search))
        {
            cards = cards.Where(c =>
                c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (c.Description != null && c.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Filter by priority
        if (!string.IsNullOrWhiteSpace(priority))
        {
            cards = cards.Where(c => c.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
        {
            cards = cards.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Filter by due date range
        if (!string.IsNullOrWhiteSpace(due))
        {
            var today = DateTime.Today;
            cards = due switch
            {
                "overdue" => cards.Where(c => c.DueDate.HasValue && c.DueDate.Value.Date < today && c.Status != "Done").ToList(),
                "today" => cards.Where(c => c.DueDate.HasValue && c.DueDate.Value.Date == today).ToList(),
                "week" => cards.Where(c => c.DueDate.HasValue && c.DueDate.Value.Date >= today && c.DueDate.Value.Date <= today.AddDays(7)).ToList(),
                "none" => cards.Where(c => !c.DueDate.HasValue).ToList(),
                _ => cards
            };
        }

        ViewBag.Search = search;
        ViewBag.Priority = priority;
        ViewBag.Status = status;
        ViewBag.Due = due;
        return View(cards);
    }

    [HttpGet("/cards/my")]
    public async Task<IActionResult> My()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebCardDto>>("/api/cards/assigned-to-me");
        return View("Index", data ?? new List<WebCardDto>());
    }

    [HttpGet("/cards/due-today")]
    public async Task<IActionResult> DueToday()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebCardDto>>("/api/cards/due-today");
        return View(data ?? new List<WebCardDto>());
    }

    [HttpGet("/cards/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, card, _) = await _api.GetAsync<WebCardDto>($"/api/cards/{id}");
        if (!ok || card == null) return NotFound();

        var (_, comments, _) = await _api.GetAsync<List<WebCommentDto>>($"/api/cards/{id}/comments");
        var (_, attachments, _) = await _api.GetAsync<List<WebAttachmentDto>>($"/api/cards/{id}/attachments");
        var (_, activity, _) = await _api.GetAsync<List<WebActivityDto>>($"/api/cards/{id}/activity");

        // Fetch column → board → columns list and project members
        List<WebColumnDto> columns = new();
        List<WebMemberDto> members = new();

        var (_, colDto, _) = await _api.GetAsync<WebColumnDto>($"/api/columns/{card.ColumnId}");
        if (colDto != null)
        {
            var (_, boardDto, _) = await _api.GetAsync<WebBoardDto>($"/api/boards/{colDto.BoardId}");
            if (boardDto != null)
            {
                columns = boardDto.Columns;
                var (_, projMembers, _) = await _api.GetAsync<List<WebMemberDto>>($"/api/projects/{boardDto.ProjectId}/members");
                members = projMembers ?? new();
            }
        }

        ViewBag.Comments = comments ?? new List<WebCommentDto>();
        ViewBag.Attachments = attachments ?? new List<WebAttachmentDto>();
        ViewBag.Activity = activity ?? new List<WebActivityDto>();
        ViewBag.Columns = columns;
        ViewBag.Members = members;

        return View(card);
    }

    [HttpGet("/columns/{cid:guid}/cards/create")]
    public IActionResult Create(Guid cid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        ViewBag.ColumnId = cid;
        return View();
    }

    [HttpPost("/columns/{cid:guid}/cards/create")]
    public async Task<IActionResult> Create(Guid cid, CreateCardDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ColumnId = cid;
            return View(model);
        }

        var (ok, data, message) = await _api.PostAsync<WebCardDto>($"/api/columns/{cid}/cards", model);
        if (ok && data != null)
        {
            TempData["Success"] = "تم إضافة البطاقة بنجاح";
            // Retrieve board to redirect back to details
            var (_, col, _) = await _api.GetAsync<WebColumnDto>($"/api/columns/{cid}");
            if (col != null) return RedirectToAction("Details", "Boards", new { id = col.BoardId });
            return RedirectToAction("Index", "Projects");
        }

        ViewBag.Error = message ?? "فشل إضافة البطاقة";
        ViewBag.ColumnId = cid;
        return View(model);
    }

    [HttpGet("/cards/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebCardDto>($"/api/cards/{id}");
        if (!ok || data == null) return NotFound();

        Enum.TryParse<Maham.Domain.Enums.Priority>(data.Priority, out var priority);
        var model = new UpdateCardDto
        {
            Title = data.Title,
            Description = data.Description,
            Priority = priority,
            DueDate = data.DueDate,
            StoryPoints = data.StoryPoints,
            Labels = data.Labels
        };

        ViewBag.CardId = id;
        ViewBag.Status = data.Status;
        return View(model);
    }

    [HttpPost("/cards/{id:guid}/edit")]
    public async Task<IActionResult> Edit(Guid id, UpdateCardDto model, string status)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CardId = id;
            ViewBag.Status = status;
            return View(model);
        }

        var (ok, data, message) = await _api.PutAsync<WebCardDto>($"/api/cards/{id}", model);
        if (ok && data != null)
        {
            // Also update status if changed
            if (data.Status != status)
            {
                await _api.PutAsync<WebCardDto>($"/api/cards/{id}/status", new { status });
            }
            TempData["Success"] = "تم تعديل البطاقة بنجاح";
            return RedirectToAction("Details", new { id });
        }

        ViewBag.Error = message ?? "فشل تعديل البطاقة";
        ViewBag.CardId = id;
        ViewBag.Status = status;
        return View(model);
    }

    [HttpGet("/cards/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebCardDto>($"/api/cards/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpPost("/cards/{id:guid}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (okGet, data, _) = await _api.GetAsync<WebCardDto>($"/api/cards/{id}");
        var cid = data?.ColumnId;

        var ok = await _api.DeleteAsync($"/api/cards/{id}");
        if (ok)
        {
            TempData["Success"] = "تم حذف البطاقة بنجاح";
            if (cid.HasValue)
            {
                var (_, col, _) = await _api.GetAsync<WebColumnDto>($"/api/columns/{cid.Value}");
                if (col != null) return RedirectToAction("Details", "Boards", new { id = col.BoardId });
            }
            return RedirectToAction("Index", "Projects");
        }

        TempData["Error"] = "فشل حذف البطاقة";
        return RedirectToAction("Details", new { id });
    }

    [HttpGet("/cards/{id:guid}/move")]
    public async Task<IActionResult> Move(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, card, _) = await _api.GetAsync<WebCardDto>($"/api/cards/{id}");
        if (!ok || card == null) return NotFound();

        var (_, col, _) = await _api.GetAsync<WebColumnDto>($"/api/columns/{card.ColumnId}");
        List<WebColumnDto> columns = new();
        if (col != null)
        {
            var (_, board, _) = await _api.GetAsync<WebBoardDto>($"/api/boards/{col.BoardId}");
            if (board != null) columns = board.Columns;
        }

        ViewBag.Card = card;
        ViewBag.Columns = columns;
        return View();
    }

    [HttpPost("/cards/{id:guid}/move")]
    public async Task<IActionResult> Move(Guid id, Guid toColumnId, int newOrder = 0)
    {
        var (ok, data, message) = await _api.PutAsync<WebCardDto>($"/api/cards/{id}/move", new { toColumnId, newOrder });
        if (ok)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            TempData["Success"] = "تم نقل البطاقة بنجاح";
            return RedirectToAction("Details", new { id });
        }

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return BadRequest(new { success = false, message });
        }

        TempData["Error"] = message ?? "فشل نقل البطاقة";
        return RedirectToAction("Details", new { id });
    }

    [HttpGet("/cards/{id:guid}/cover")]
    public IActionResult Cover(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        ViewBag.CardId = id;
        return View();
    }

    [HttpPost("/cards/{id:guid}/cover")]
    public async Task<IActionResult> Cover(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "الرجاء اختيار ملف صالح";
            ViewBag.CardId = id;
            return View();
        }

        using var form = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        form.Add(new StreamContent(stream), "file", file.FileName);

        var (ok, _, message) = await _api.PostFormAsync<WebCardDto>($"/api/cards/{id}/cover", form);
        if (ok)
        {
            TempData["Success"] = "تم تحديث غلاف البطاقة بنجاح";
            return RedirectToAction("Details", new { id });
        }

        ViewBag.Error = message ?? "فشل رفع غلاف البطاقة";
        ViewBag.CardId = id;
        return View();
    }

    [HttpPost("/cards/{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, Guid? assigneeId)
    {
        var (ok, _, message) = await _api.PutAsync<WebCardDto>($"/api/cards/{id}/assign", new { assigneeId });
        if (ok) TempData["Success"] = "تم تعيين المسؤول بنجاح";
        else TempData["Error"] = message ?? "فشل تعيين المسؤول";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost("/cards/{id:guid}/priority")]
    public async Task<IActionResult> UpdatePriority(Guid id, string priority)
    {
        var (ok, _, message) = await _api.PutAsync<WebCardDto>($"/api/cards/{id}/priority", new { priority });
        if (ok) TempData["Success"] = "تم تحديث الأولوية بنجاح";
        else TempData["Error"] = message ?? "فشل تحديث الأولوية";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost("/cards/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, string status)
    {
        var (ok, _, message) = await _api.PutAsync<WebCardDto>($"/api/cards/{id}/status", new { status });
        if (ok) TempData["Success"] = "تم تحديث الحالة بنجاح";
        else TempData["Error"] = message ?? "فشل تحديث الحالة";
        return RedirectToAction("Details", new { id });
    }

    [HttpPost("/cards/{id:guid}/labels")]
    public async Task<IActionResult> UpdateLabels(Guid id, string labels)
    {
        var (ok, _, message) = await _api.PutAsync<WebCardDto>($"/api/cards/{id}/labels", new { labels });
        if (ok) TempData["Success"] = "تم تحديث التسميات بنجاح";
        else TempData["Error"] = message ?? "فشل تحديث التسميات";
        return RedirectToAction("Details", new { id });
    }
}
