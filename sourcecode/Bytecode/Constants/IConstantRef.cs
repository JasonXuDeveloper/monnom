using System;

namespace Nom.Bytecode
{
    public interface IConstantRef<out T> where T : IConstant
    {
        T Constant { get; }
        UInt64 ConstantID { get; } 
    }
}
