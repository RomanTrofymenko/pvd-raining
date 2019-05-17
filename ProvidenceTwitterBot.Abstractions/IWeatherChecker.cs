using System.Threading.Tasks;

namespace ProvidenceTwitterBot
{
    public interface IWeatherChecker
    {
        Task<bool?> RainCheck();
    }
}