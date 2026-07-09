using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Maham.Application.Services.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    void DeleteFile(string filePath);
}
