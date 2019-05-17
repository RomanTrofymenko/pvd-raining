using System.Threading.Tasks;

namespace ProvidenceTwitterBot
{
    public interface IProvidenceRainCheckWorker
    {
        Task Execute();
    }
}