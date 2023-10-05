using System.IO;

namespace Nom.Bytecode
{
    public class LoadBoolConstantInstruction : AInstruction
    {
        public bool Value { get; }
        public int Register { get; }
        public LoadBoolConstantInstruction(bool value, int register) : base(OpCode.LoadBoolConstant)
        {
            Value = value;
            Register = register;
        }

        protected override void WriteArguments(Stream ws)
        {
            ws.WriteByte(Value ? (byte)1 : (byte)0);
            ws.WriteValue(Register);
        }

        public static LoadBoolConstantInstruction Read(Stream s, IReadConstantSource rcs)
        {
            var value = s.ReadByte() == 1;
            var reg = s.ReadInt();
            return new LoadBoolConstantInstruction(value, reg);
        }
    }
}
