using System.Collections.Generic;
using System.IO;

namespace Nom.Bytecode
{
    public class ConstructStructInstruction : ACallInstruction
    {
        public IConstantRef<StructConstant> StructConstant { get; }
        public IConstantRef<TypeListConstant> TypeArgumentsConstant { get; }
        public int ResultRegister { get; }
        public ConstructStructInstruction(IConstantRef<StructConstant> structure, IConstantRef<TypeListConstant> typeArguments, IEnumerable<int> argumentRegisters, int resultRegister) : base(typeArguments, argumentRegisters, OpCode.ConstructStruct)
        {
            StructConstant = structure;
            TypeArgumentsConstant = typeArguments;
            ResultRegister = resultRegister;
        }

        protected override void WriteArguments(Stream ws)
        {
            ws.WriteValue(StructConstant.ConstantID);
            ws.WriteValue(TypeArgumentsConstant.ConstantID);
            ws.WriteValue(ResultRegister);
        }

        public static ConstructStructInstruction Read(Stream s, IReadConstantSource rcs, IEnumerable<int> argRegs)
        {
            var structconst = rcs.ReferenceStructConstant(s.ReadULong());
            var targsconst = rcs.ReferenceTypeListConstant(s.ReadULong());
            var resultReg = s.ReadInt();
            return new ConstructStructInstruction(structconst, targsconst, argRegs, resultReg);
        }
    }
}
