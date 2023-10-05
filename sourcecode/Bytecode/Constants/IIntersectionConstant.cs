namespace Nom.Bytecode
{
    public interface IIntersectionConstant : IConstant, ITypeConstant
    {
        IConstantRef<TypeListConstant> ComponentsConstant
        {
            get;
        }
    }
}
