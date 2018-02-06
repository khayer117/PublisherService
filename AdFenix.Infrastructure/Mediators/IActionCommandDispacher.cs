using System.Threading.Tasks;

namespace AdFenix.Infrastructure.Mediators
{
    public interface IActionCommandDispacher
    {
        Task Send(object command);
    }
}
