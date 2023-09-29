using System;
using Nom.Language;
using System.IO;

namespace Nom.Bytecode
{
    public class CppMethodDeclRep : MethodDefRep
    {
        public CppMethodDeclRep(IConstantRef<StringConstant> name,
            IConstantRef<StringConstant> cppName, IConstantRef<TypeParametersConstant> typeParameters,
            IConstantRef<ITypeConstant> returnType, IConstantRef<TypeListConstant> argumentTypes, Visibility visibility,
            bool isFinal) : base(name, typeParameters, returnType, argumentTypes, visibility, isFinal,
            0 /*TODO set this*/, null)
        {
            CppNameConstant = cppName;
        }


        public IConstantRef<StringConstant> CppNameConstant { get; }


        protected override IOptional<IParameterizedSpec> ParamParent => throw new NotImplementedException();

        public override ITypeParametersSpec TypeParameters => throw new NotImplementedException();

        public override void WriteByteCode(Stream ws)
        {
            ws.WriteByte((byte)BytecodeInternalElementType.CppMethod);
            ws.WriteValue(NameConstant.ConstantID);
            ws.WriteValue(TypeParametersConstant.ConstantID);
            ws.WriteValue(ReturnTypeConstant.ConstantID);
            ws.WriteValue(ArgumentTypesConstant.ConstantID);
            ws.WriteByte((byte)(IsFinal ? 1 : 0));
            WriteByteCodeBody(ws);
        }

        public override void WriteByteCodeBody(Stream ws)
        {
            ws.WriteValue(CppNameConstant.ConstantID);
        }

        public new static CppMethodDeclRep Read(IInterfaceSpec container, Stream s, IReadConstantSource rcs)
        {
            byte tag = s.ReadActualByte();
            if (tag != (byte)BytecodeInternalElementType.CppMethod)
            {
                throw new NomBytecodeException("Bytecode malformed!");
            }

            var nameconst = rcs.ReferenceStringConstant(s.ReadULong());
            var tpconst = rcs.ReferenceTypeParametersConstant(s.ReadULong());
            var rtconst = rcs.ReferenceTypeConstant(s.ReadULong());
            var argsconst = rcs.ReferenceTypeListConstant(s.ReadULong());
            var isfinal = s.ReadActualByte() == 1;
            var cppnameconst = rcs.ReferenceStringConstant(s.ReadULong());
            if (cppnameconst.ConstantID == 0)
            {
                throw new NomBytecodeException("Bytecode malformed!");
            }

            //TODO: actually put visibility in bytecode here
            return new CppMethodDeclRep(nameconst, cppnameconst, tpconst, rtconst, argsconst, Visibility.Public,
                isfinal);
        }
    }
}