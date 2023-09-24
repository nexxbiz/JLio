using TLio.Contracts;

namespace TLio.Models
{
    public class CommandsList<T> : List<ICommand<T>>
    {
    }
}