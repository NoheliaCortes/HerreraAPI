using System.Security.Claims;
using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public int? CurrentUserId
        {
            get
            {
                var userId = User?.FindFirst("UserId")?.Value
                    ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User?.FindFirst("sub")?.Value;

                return int.TryParse(userId, out var parsedUserId)
                    ? parsedUserId
                    : null;
            }
        }

        public string? CurrentUsername =>
            User?.FindFirst(ClaimTypes.Name)?.Value
            ?? User?.FindFirst("username")?.Value;

        public string? CurrentRole =>
            User?.FindFirst(ClaimTypes.Role)?.Value;

        public bool IsAuthenticated =>
            User?.Identity?.IsAuthenticated == true;
    }
}
