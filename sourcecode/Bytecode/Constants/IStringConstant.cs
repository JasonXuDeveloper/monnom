using System;

namespace Nom.Bytecode
{
    public interface IStringConstant : IConstant
    {
        String Value
        {
            get;
        }
    }
}
