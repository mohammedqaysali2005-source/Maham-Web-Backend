using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Maham.Web.Services;
using Maham.Web.Helpers;

namespace Maham.Web.Controllers;

public class AttachmentsController : Controller
{
    private readonly ApiService _api;
    private readonly IHttpClientFactory _httpFactory;

    public AttachmentsController(ApiService api, IHttpClientFactory httpFactory)
    {
        _api = api;
        _httpFactory = httpFactory;
    }

    [HttpGet("/cards/{cid:guid}/attachments")]
    public async Task<IActionResult> Index(Guid cid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<List<WebAttachmentDto>>($"/api/cards/{cid}/attachments");
        ViewBag.CardId = cid;
        return View(data ?? new List<WebAttachmentDto>());
    }

    [HttpGet("/attachments/{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebAttachmentDto>($"/api/attachments/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpGet("/cards/{cid:guid}/attachments/create")]
    public IActionResult Create(Guid cid)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        ViewBag.CardId = cid;
        return View();
    }

    [HttpPost("/cards/{cid:guid}/attachments/create")]
    public async Task<IActionResult> Create(Guid cid, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "يرجى تحديد ملف صالح لرفعه";
            ViewBag.CardId = cid;
            return View();
        }

        using var form = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        form.Add(new StreamContent(stream), "file", file.FileName);

        var (ok, _, message) = await _api.PostFormAsync<WebAttachmentDto>($"/api/cards/{cid}/attachments", form);
        if (ok)
        {
            TempData["Success"] = "تم رفع الملف بنجاح";
            return RedirectToAction("Details", "Cards", new { id = cid });
        }

        ViewBag.Error = message ?? "فشل رفع الملف";
        ViewBag.CardId = cid;
        return View();
    }

    [HttpGet("/attachments/{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var token = SessionHelper.GetToken(HttpContext.Session);
        var req = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5233/api/attachments/{id}/download");
        if (!string.IsNullOrEmpty(token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var client = _httpFactory.CreateClient();
            var res = await client.SendAsync(req);
            if (res.IsSuccessStatusCode)
            {
                var stream = await res.Content.ReadAsStreamAsync();
                var contentType = res.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                var fileName = res.Content.Headers.ContentDisposition?.FileNameStar 
                               ?? res.Content.Headers.ContentDisposition?.FileName 
                               ?? "downloaded_file";
                
                return File(stream, contentType, fileName);
            }
        }
        catch { }

        TempData["Error"] = "فشل تحميل الملف";
        return RedirectToAction("Index", "Projects");
    }

    [HttpGet("/attachments/{id:guid}/delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Auth");

        var (ok, data, _) = await _api.GetAsync<WebAttachmentDto>($"/api/attachments/{id}");
        if (!ok || data == null) return NotFound();

        return View(data);
    }

    [HttpPost("/attachments/{id:guid}/delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (okGet, data, _) = await _api.GetAsync<WebAttachmentDto>($"/api/attachments/{id}");
        var cid = data?.CardId;

        var ok = await _api.DeleteAsync($"/api/attachments/{id}");
        if (ok)
        {
            TempData["Success"] = "تم حذف المرفق بنجاح";
            if (cid.HasValue) return RedirectToAction("Details", "Cards", new { id = cid.Value });
            return RedirectToAction("Index", "Projects");
        }

        TempData["Error"] = "فشل حذف المرفق";
        return RedirectToAction("Details", "Cards", new { id = cid });
    }
}
