using Nom.Language;

namespace Nom.Bytecode
{
    public interface IMethodRep : IMethodSpec
    {
        IConstantRef<StringConstant> NameConstant
        {
            get;
        }
        IConstantRef<ITypeConstant> ReturnTypeConstant
        {
            get;
        }

        IConstantRef<TypeListConstant> ArgumentTypesConstant
        {
            get;
        }
    }
}
