using Maham.Domain.Entities;

namespace Maham.Application.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
