using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Nom.Bytecode
{
    public class CppConstructorDefRep : ConstructorDefRep
    {
        public CppConstructorDefRep(IConstantRef<StringConstant> cppName, IConstantRef<TypeListConstant> parameters,
            Visibility visibility, IEnumerable<int> superCallArgsRegs, int regcount) : base(parameters, visibility,
            null, superCallArgsRegs, null, regcount)
        {
            CppNameConstant = cppName;
        }

        public IConstantRef<StringConstant> CppNameConstant { get; }

        public override void WriteByteCode(Stream ws)
        {
            ws.WriteByte((byte)BytecodeInternalElementType.Constructor);
            ws.WriteValue(ParametersConstant.ConstantID);
            ws.WriteValue(RegisterCount);
            ws.WriteValue((UInt64)SuperConstructorArgs.LongCount());
            foreach (int regIndex in SuperConstructorArgs)
            {
                ws.WriteValue(regIndex);
            }

            ws.WriteValue(CppNameConstant.ConstantID);
        }

        public static new CppConstructorDefRep Read(Language.IClassSpec container, Stream s, IReadConstantSource rcs)
        {
            byte tag = s.ReadActualByte();
            if (tag != (byte)BytecodeInternalElementType.Constructor)
            {
                throw new NomBytecodeException("Bytecode malformed!");
            }

            var argsconst = rcs.ReferenceTypeListConstant(s.ReadULong());
            var regcount = s.ReadInt();
            var preinstcount = s.ReadULong();
            var scacount = s.ReadULong();
            var postinstcount = s.ReadULong();
            List<int> superConstructorArgs = new List<int>();
            for (ulong i = 0; i < scacount; i++)
            {
                superConstructorArgs.Add(s.ReadInt());
            }

            var cppnameconst = rcs.ReferenceStringConstant(s.ReadULong());
            if (cppnameconst.ConstantID == 0)
            {
                throw new NomBytecodeException("Bytecode malformed!");
            }

            //TODO: actually put visibility in bytecode here
            return new CppConstructorDefRep(cppnameconst, argsconst, Visibility.Public, superConstructorArgs, regcount);
        }
    }
}