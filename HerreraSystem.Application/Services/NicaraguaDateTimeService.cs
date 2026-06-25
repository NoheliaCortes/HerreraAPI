using HerreraSystem.Application.Interfaces.Services;

namespace HerreraSystem.Application.Services
{
    public class NicaraguaDateTimeService : INicaraguaDateTimeService
    {
        private static readonly TimeZoneInfo NicaraguaTimeZone = ResolveTimeZone();

        public DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NicaraguaTimeZone);

        private static TimeZoneInfo ResolveTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Managua");
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Managua");
            }
        }
    }
}
