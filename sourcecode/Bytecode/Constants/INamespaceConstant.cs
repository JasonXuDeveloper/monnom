using System;

namespace Nom.Bytecode
{
    public interface INamespaceConstant : IConstant
    {
        //IOptional<IConstantRef<INamespaceConstant>> ParentConstant
        //{
        //    get;
        //}
        IConstantRef<StringConstant> NameConstant
        {
            get;
        }
        String QualifiedName
        {
            get;
        }
    }
}
