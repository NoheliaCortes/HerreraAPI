namespace HerreraSystem.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        int? CurrentUserId { get; }
        string? CurrentUsername { get; }
        string? CurrentRole { get; }
        bool IsAuthenticated { get; }
    }
}
