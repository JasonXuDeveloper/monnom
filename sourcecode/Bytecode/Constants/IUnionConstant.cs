namespace Nom.Bytecode
{
    public interface IUnionConstant : IConstant, ITypeConstant
    {
        TypeListConstant ComponentsConstant
        {
            get;
            set;
        }
    }
}
