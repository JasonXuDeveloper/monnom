using Nom.Language;

namespace Nom.Bytecode
{
    public interface ITypeConstant : IConstant
    {
        IType Value { get; }
    }
}
