using System.Collections.Generic;
using Nom.Language;

namespace Nom.Bytecode
{
    public interface IConstantSource
    {
        IConstantRef<StringConstant> GetStringConstant(string str);
        
        IConstantRef<ITypeConstant> GetTypeConstant(Language.IType type, bool defaultBottom = false);
        
        IConstantRef<TypeListConstant> GetTypeListConstant(IEnumerable<Language.IType> type);
        IConstantRef<TypeListConstant> GetTypeListConstant(IEnumerable<Language.ITypeArgument> type);
        
        IConstantRef<ClassTypeConstant> GetClassTypeConstant(IParamRef<IInterfaceSpec, Language.ITypeArgument> ct);
        
        IConstantRef<SuperClassConstant> GetSuperClassConstant(IParamRef<IClassSpec, Language.IType> superclass);
        
        IConstantRef<ClassConstant> GetClassConstant(IClassSpec cls);
        IConstantRef<IInterfaceConstant> GetInterfaceConstant(IInterfaceSpec cls);
        IConstantRef<StaticMethodConstant> GetStaticMethodConstant(IParameterizedSpecRef<IStaticMethodSpec> staticMethod/*, IEnumerable<Language.IType> typeArgs*/);
        IConstantRef<MethodConstant> GetMethodConstant(IParameterizedSpecRef<IMethodSpec> method/*, IEnumerable<Language.ITypeArgument> typeArgs*/);
    }
}
