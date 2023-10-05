using System.IO;

namespace Nom.Bytecode
{
    public interface IEmittable
    {
        void Emit(Stream s);
    }
}
