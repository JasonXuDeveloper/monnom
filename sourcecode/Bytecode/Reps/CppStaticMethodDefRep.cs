using System.IO;
using Nom.Language;

namespace Nom.Bytecode
{
    public class CppStaticMethodDefRep : StaticMethodDefRep
    {
        public CppStaticMethodDefRep(IConstantRef<IStringConstant> name, IConstantRef<StringConstant> cppName,
            IConstantRef<ITypeConstant> returnType, IConstantRef<TypeParametersConstant> typeParamConstraints,
            IConstantRef<TypeListConstant> parameters, Visibility visibility) : base(name, returnType,
            typeParamConstraints, parameters, visibility, null, 0)
        {
            CppNameConstant = cppName;
        }

        public IConstantRef<StringConstant> CppNameConstant { get; }

        public override void WriteByteCode(Stream ws)
        {
            ws.WriteByte((byte)BytecodeInternalElementType.CppStaticMethod);
            ws.WriteValue(NameConstant.ConstantID);
            ws.WriteValue(CppNameConstant.ConstantID);
            ws.WriteValue(TypeParametersConstant.ConstantID);
            ws.WriteValue(ReturnTypeConstant.ConstantID);
            ws.WriteValue(ParametersConstant.ConstantID);
        }

        public new static CppStaticMethodDefRep Read(IClassSpec container, Stream s, IReadConstantSource rcs)
        {
            byte tag = s.ReadActualByte();
            if (tag != (byte)BytecodeInternalElementType.CppStaticMethod)
            {
                throw new NomBytecodeException("Bytecode malformed!");
            }

            var nameconst = rcs.ReferenceStringConstant(s.ReadULong());
            var cppnameconst = rcs.ReferenceStringConstant(s.ReadULong());
            var tpconst = rcs.ReferenceTypeParametersConstant(s.ReadULong());
            var rtconst = rcs.ReferenceTypeConstant(s.ReadULong());
            var argsconst = rcs.ReferenceTypeListConstant(s.ReadULong());

            if (cppnameconst.ConstantID == 0)
            {
                throw new NomBytecodeException("Bytecode malformed!");
            }
            //TODO: actually put visibility in bytecode here
            return new CppStaticMethodDefRep(nameconst, cppnameconst, rtconst, tpconst, argsconst, Visibility.Public);
        }
    }
}