using System.Threading.Tasks;

namespace ProvidenceTwitterBot
{
    public interface ITwitterApi
    {
        Task<string> Tweet(string text);
    }
}