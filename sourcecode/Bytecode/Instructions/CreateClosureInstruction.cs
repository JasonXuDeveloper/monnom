using System.Collections.Generic;
using System.IO;

namespace Nom.Bytecode
{
    public class CreateClosureInstruction : ACallInstruction
    {
        public IConstantRef<LambdaConstant> LambdaConstant { get; }
        public IConstantRef<TypeListConstant> TypeArgumentsConstant { get; }
        public int ResultRegister { get; }
        public CreateClosureInstruction(IConstantRef<LambdaConstant> lambda, IConstantRef<TypeListConstant> typeArguments, IEnumerable<int> argumentRegisters, int resultRegister) : base(typeArguments, argumentRegisters, OpCode.CreateClosure)
        {
            LambdaConstant = lambda;
            TypeArgumentsConstant = typeArguments;
            ResultRegister = resultRegister;
        }

        protected override void WriteArguments(Stream ws)
        {
            ws.WriteValue(LambdaConstant.ConstantID);
            ws.WriteValue(TypeArgumentsConstant.ConstantID);
            ws.WriteValue(ResultRegister);
        }
        public static CreateClosureInstruction Read(Stream s, IReadConstantSource rcs, IEnumerable<int> argRegs)
        {
            var lambdaconst = rcs.ReferenceLambdaConstant(s.ReadULong());
            var targsconst = rcs.ReferenceTypeListConstant(s.ReadULong());
            var resultReg = s.ReadInt();
            return new CreateClosureInstruction(lambdaconst, targsconst, argRegs, resultReg);
        }
    }
}
