using System.IO;
using System.Threading.Tasks;

namespace Chaos.Document
{
    public interface IStreamQuerier
    {
        Task<Stream> ReadAsStreamAsync(string uri);
    }
}