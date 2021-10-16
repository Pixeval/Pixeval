using System.Threading.Tasks;

namespace Pixeval.Activation
{
    public interface IAppActivationHandler
    {
        Task Execute(string id);
    }
}