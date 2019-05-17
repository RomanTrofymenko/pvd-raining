using System.Threading.Tasks;

namespace ProvidenceTweeterBot
{
    public interface IWeatherChecker
    {
        Task<bool?> RainCheck();
    }
}