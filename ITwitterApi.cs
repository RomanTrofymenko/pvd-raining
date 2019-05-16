using System.Threading.Tasks;

namespace ProvidenceTweeterBot
{
    public interface ITwitterApi
    {
        Task<string> Tweet(string text);
    }
}