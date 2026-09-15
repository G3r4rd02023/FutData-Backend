using FutData.Domain.Entities;

namespace FutData.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
