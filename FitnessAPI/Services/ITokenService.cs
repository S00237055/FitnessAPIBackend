using FitnessAPI.Models;

namespace FitnessAPI.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
