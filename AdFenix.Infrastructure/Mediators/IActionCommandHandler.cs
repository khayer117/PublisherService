using System.Threading.Tasks;

namespace AdFenix.Infrastructure.Mediators
{
    public interface IActionCommandHandler<in TActionCommand>
    {
        Task Handle(TActionCommand command);
    }
}
