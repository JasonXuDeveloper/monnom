using System.IO;
using Nom.Parser;

namespace Nom.Bytecode
{
    public class UnaryOpInstruction : AInstruction
    {
        public int Arg { get; }
        public int Register { get; }
        public UnaryOperator Operator { get; }
        public UnaryOpInstruction(UnaryOperator op, int arg, int reg) : base(OpCode.UnaryOp)
        {
            Operator = op;
            Arg = arg;
            Register = reg;
        }

        protected override void WriteArguments(Stream ws)
        {
            ws.WriteByte((byte)Operator);
            ws.WriteValue(Arg);
            ws.WriteValue(Register);
        }

        public static UnaryOpInstruction Read(Stream s, IReadConstantSource rcs)
        {
            var op = (UnaryOperator)s.ReadActualByte();
            var arg = s.ReadInt();
            var reg = s.ReadInt();
            return new UnaryOpInstruction(op, arg, reg);
        }
    }
}
