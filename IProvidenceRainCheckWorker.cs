using System.Threading.Tasks;

namespace ProvidenceTweeterBot
{
    public interface IProvidenceRainCheckWorker
    {
        Task Execute();
    }
}