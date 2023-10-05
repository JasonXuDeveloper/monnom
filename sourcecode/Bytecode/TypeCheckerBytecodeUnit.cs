using System;
using System.Collections.Generic;
using System.Linq;
using Nom.Language;
using Nom.TypeChecker;

namespace Nom.Bytecode
{
    public class TypeCheckerBytecodeUnit : BytecodeUnit, IConstantSource
    {
        public TypeCheckerBytecodeUnit(string name, AssemblyUnit au): base(name, au)
        {
            superClassConstants = new ConstantDict<IParamRef<IClassSpec, IType>, SuperClassConstant>(sc =>
                RegisterConstant(new SuperClassConstant(GetClassConstant(sc.Element),
                    GetTypeListConstant(sc.Substitutions.OrderBy(kvp => kvp.Key.Index).Select(kvp => kvp.Value)),
                    ++ConstantCounter))); //TODO: get right argument list
            classConstants = new ConstantDict<IClassSpec, ClassConstant>(sc =>
                RegisterConstant(new ClassConstant(++ConstantCounter, GetStringConstant(sc.Library.Name),
                    GetStringConstant(sc.FullQualifiedName))));
            interfaceConstants = new ConstantDict<IInterfaceSpec, InterfaceConstant>(sc =>
                RegisterConstant(new InterfaceConstant(++ConstantCounter, GetStringConstant(sc.Library.Name),
                    GetStringConstant(sc.FullQualifiedName))));
            classTypeConstants = new ConstantDict<IParamRef<IInterfaceSpec, ITypeArgument>, ClassTypeConstant>(sc =>
                RegisterConstant(new ClassTypeConstant(++ConstantCounter,
                    sc.Visit(
                        new ParamRefVisitor<TypeCheckerBytecodeUnit, IConstantRef<INamedConstant>, ITypeArgument>(
                            (_, _) =>
                            {
                                throw new InternalException("Can't use a namespace as class type");
                            }, (t, env) => env.GetInterfaceConstant(t.Element),
                            (t, bcu) => bcu.GetClassConstant(t.Element)), this),
                    GetTypeListConstant(sc.Substitutions.OrderBy(kvp => kvp.Key.Index).Select(kvp => kvp.Value)))));
            maybeTypeConstants = new ConstantDict<IType, MaybeTypeConstant>(tp =>
                RegisterConstant(new MaybeTypeConstant(++ConstantCounter, GetTypeConstant(tp))));
            staticMethodConstants = new ConstantDict<IParameterizedSpecRef<IStaticMethodSpec>, StaticMethodConstant>(
                sm => RegisterConstant(new StaticMethodConstant(++ConstantCounter,
                    (IConstantRef<ClassConstant>)GetNamespaceConstant(sm.Element.Container),
                    GetStringConstant(sm.Element.Name), GetTypeListConstant(sm.Arguments),
                    GetTypeListConstant(sm.Element.Parameters.Entries.Select(ps => ps.Type)))));
            methodConstants = new ConstantDict<IParameterizedSpecRef<IMethodSpec>, MethodConstant>(sm =>
            {
                Func<INamespaceSpec, object, IParamRef<IInterfaceSpec, ITypeArgument>> nsh = (_, _) =>
                {
                    throw new InternalException("Namespace can't contain method!");
                };
                Func<IInterfaceSpec, object, IParamRef<IInterfaceSpec, ITypeArgument>> ifaceh = (iface, _) =>
                    new InterfaceRef<ITypeArgument>(iface, sm.Substitutions.Restrict(sm.Element.Container));
                Func<IClassSpec, object, IParamRef<IInterfaceSpec, ITypeArgument>> clsh = (cls, _) =>
                    new ClassRef<ITypeArgument>(cls, sm.Substitutions.Restrict(sm.Element.Container));
                var nsv = new NamespaceSpecVisitor<object, IParamRef<IInterfaceSpec, ITypeArgument>>(
                    nsh,
                    ifaceh,
                    clsh);
                return RegisterConstant(new MethodConstant(
                    ++ConstantCounter,
                    /*(IConstantRef<INamedConstant>)GetNamespaceConstant(sm.Element.Container)*/
                    GetClassTypeConstant(sm.Element.Container.Visit(nsv)),
                    //sm.ParameterizedParent.Extract(pp => pp.Substitutions)),
                    GetStringConstant(sm.Element.Name),
                    GetTypeListConstant(sm.Arguments),
                    GetTypeListConstant(sm.Element.Parameters.Entries.Select(ps =>
                        ((ISubstitutable<IType>)ps.Type).Substitute(
                            sm.Substitutions.Transform(t => t.AsType))))));
            });
            lambdaConstants =
                new ConstantDict<ITDLambda, LambdaConstant>(_ =>
                    RegisterConstant(new LambdaConstant(++ConstantCounter)));
            structConstants =
                new ConstantDict<ITDStruct, StructConstant>(_ =>
                    RegisterConstant(new StructConstant(++ConstantCounter)));
        }

        public IConstantRef<IConstant> GetNamespaceConstant(INamespaceSpec ns)
        {
            return ns.Visit(new NamespaceSpecVisitor<object, IConstantRef<IConstant>>(
                (_, _) => throw new NotImplementedException(),
                (iface, _) => GetInterfaceConstant(iface),
                (cls, _) => GetClassConstant(cls)));
        }


        private ConstantDict<IType, MaybeTypeConstant> maybeTypeConstants;

        public IConstantRef<MaybeTypeConstant> GetMaybeTypeConstant(MaybeType tp)
        {
            return maybeTypeConstants.GetConstant(tp.PotentialType);
        }

        private ConstantDict<IClassSpec, ClassConstant> classConstants;

        public IConstantRef<ClassConstant> GetClassConstant(IClassSpec cls)
        {
            return classConstants.GetConstant(cls);
        }

        private ConstantDict<IParamRef<IInterfaceSpec, ITypeArgument>, ClassTypeConstant> classTypeConstants;

        public IConstantRef<ClassTypeConstant> GetClassTypeConstant(
            IParamRef<IInterfaceSpec, ITypeArgument> ct)
        {
            return classTypeConstants.GetConstant(ct);
        }

        private ConstantDict<ITDStruct, StructConstant> structConstants;

        public IConstantRef<StructConstant> GetStructConstant(ITDStruct structure)
        {
            return structConstants.GetConstant(structure);
        }

        private ConstantDict<ITDLambda, LambdaConstant> lambdaConstants;

        public IConstantRef<LambdaConstant> GetLambdaConstant(ITDLambda lambda)
        {
            return lambdaConstants.GetConstant(lambda);
        }

        private ConstantDict<IParamRef<IClassSpec, IType>, SuperClassConstant> superClassConstants;

        public IConstantRef<SuperClassConstant> GetSuperClassConstant(IParamRef<IClassSpec, IType> superclass)
        {
            return superClassConstants.GetConstant(superclass);
        }

        public IConstantRef<SuperInterfacesConstant> GetSuperInterfacesConstant(
            IEnumerable<IParamRef<IInterfaceSpec, IType>> superInterfaces)
        {
            return new ConstantRef<SuperInterfacesConstant>(RegisterConstant(
                new SuperInterfacesConstant(++ConstantCounter,
                    superInterfaces.Select(si => (GetInterfaceConstant(si.Element),
                        GetTypeListConstant(si.PArguments.OrderBy(kvp => kvp.Key.Index).Select(kvp => kvp.Value)))))));
        }

        public IConstantRef<ITypeConstant> GetTypeConstant(IType type, bool defaultBottom = false)
        {
            return type.Visit(TypeVisitor, (defaultBottom, this));
        }
        
        public IConstantRef<TypeListConstant> GetTypeListConstant(IEnumerable<IType> types)
        {
            var enumerable = types?.ToList();
            if (types == null || enumerable.Count == 0)
            {
                return new ConstantRef<TypeListConstant>(EmptyTlc);
            }

            List<IConstantRef<ITypeConstant>> typeConstants = enumerable.Select(t => GetTypeConstant(t)).ToList();
            IOptional<TypeListConstant> ret = TypeListConstants.FirstOrDefault(tlc =>
                tlc.TypeConstants.Count() == typeConstants.Count &&
                tlc.TypeConstants.Zip(typeConstants, (x, y) => x.Equals(y)).All(x => x)).InjectOptional();
            if (!ret.HasElem)
            {
                ret = RegisterConstant(new TypeListConstant(++ConstantCounter, typeConstants)).InjectOptional();
                TypeListConstants.Add(ret.Elem);
            }

            return new ConstantRef<TypeListConstant>(ret.Elem);
        }

        public IConstantRef<TypeListConstant> GetTypeListConstant(IEnumerable<ITypeArgument> types)
        {
            //return GetTypeListConstant(types.SelectMany(t => t.ContravariantPart.Singleton().Snoc(t.CovariantPart)));
            return GetTypeListConstant(types.Select(t => t.AsType));
        }


        public IConstantRef<TypeParametersConstant> GetTypeParametersConstant(ITypeParametersSpec tps)
        {
            if (!tps.Any())
            {
                return new ConstantRef<TypeParametersConstant>(EmptyTpc);
            }

            var entries = tps.Select(tpsx => new TypeParameterEntry( /*tpsx.Variance,*/
                GetTypeConstant(tpsx.LowerBound), GetTypeConstant(tpsx.UpperBound)));
            IOptional<TypeParametersConstant> ret = TypeParametersConstants.FirstOrDefault(tpc =>
                    tpc.Entries.Count() == tps.Count() && tpc.Entries.Zip(entries, (x, y) => x.Equals(y)).All(x => x))
                .InjectOptional();
            if (!ret.HasElem)
            {
                ret = RegisterConstant(new TypeParametersConstant(++ConstantCounter, entries)).InjectOptional();
                TypeParametersConstants.Add(ret.Elem);
            }

            return new ConstantRef<TypeParametersConstant>(ret.Elem);
        }

        public IConstantRef<IStringConstant> ReferenceStringConstant(ulong id)
        {
            throw new NotImplementedException();
        }

        public IConstantRef<ITypeConstant> ReferenceTypeConstant(ulong id)
        {
            throw new NotImplementedException();
        }

        public IConstantRef<TypeListConstant> ReferenceTypeListConstant(ulong id)
        {
            throw new NotImplementedException();
        }

        ConstantDict<IInterfaceSpec, InterfaceConstant> interfaceConstants;

        public IConstantRef<IInterfaceConstant> GetInterfaceConstant(IInterfaceSpec iface)
        {
            return interfaceConstants.GetConstant(iface);
        }

        ConstantDict<IParameterizedSpecRef<IStaticMethodSpec>, StaticMethodConstant> staticMethodConstants;

        public IConstantRef<StaticMethodConstant> GetStaticMethodConstant(
            IParameterizedSpecRef<IStaticMethodSpec> staticMethod)
        {
            return
                staticMethodConstants
                    .GetConstant(
                        staticMethod); // new ParameterizedSpecRef<IStaticMethodSpec>(staticMethod, new TypeEnvironment<ITypeArgument>(staticMethod.TypeParameters, typeArgs)));
        }

        ConstantDict<IParameterizedSpecRef<IMethodSpec>, MethodConstant> methodConstants;

        public IConstantRef<MethodConstant> GetMethodConstant(IParameterizedSpecRef<IMethodSpec> method)
        {
            return
                methodConstants
                    .GetConstant(
                        method); // new ParameterizedSpecRef<IMethodSpec>(method, new TypeEnvironment<ITypeArgument>(method.TypeParameters, typeArgs)));
        }

        private Dictionary<IEnumerable<IType>, IIntersectionConstant> intersectionConstants =
            new Dictionary<IEnumerable<IType>, IIntersectionConstant>();

        public IConstantRef<IIntersectionConstant> GetIntersectionTypeConstant(IEnumerable<IType> entries)
        {
            var matchKey = intersectionConstants.Keys.FirstOrDefault(k => k.SetEqual(entries));
            if (matchKey != null)
            {
                return new ConstantRef<IIntersectionConstant>(intersectionConstants[matchKey]);
            }

            var enumerable = entries.ToList();
            var constnt =
                RegisterConstant(new IntersectionTypeConstant(++ConstantCounter, GetTypeListConstant(enumerable)));
            intersectionConstants.Add(enumerable, constnt);
            return new ConstantRef<IIntersectionConstant>(constnt);
        }

        private static readonly TypeVisitor<(bool, TypeCheckerBytecodeUnit), IConstantRef<ITypeConstant>> TypeVisitor =
            new()
            {
                VisitClassType = (ct, pair) => pair.Item2.GetClassTypeConstant(ct),
                VisitInterfaceType = (ct, pair) => pair.Item2.GetClassTypeConstant(ct),
                VisitTopType = (_, pair) =>
                    pair.Item1 ? pair.Item2.GetIntersectionTypeConstant(new List<IType>()) : GetEmptyTypeConstant(),
                VisitBotType = (_, pair) => pair.Item1 ? GetEmptyTypeConstant() : pair.Item2.GetBottomTypeConstant(),
                //VisitIntersectionType = (tt, pair) => pair.Item2.GetIntersectionTypeConstant(tt.Components),
                VisitTypeVariable = (tv, pair) => pair.Item2.GetTypeVariableConstant(tv.ParameterSpec.Index),
                //VisitTypeRange = (tr, pair) => tr.PessimisticType.Visit(typeVisitor, pair),
                VisitDynamicType = (_, pair) => pair.Item2.GetDynamicTypeConstant(),
                VisitMaybeType = (mb, pair) => pair.Item2.GetMaybeTypeConstant(mb),
                VisitProbablyType =
                    (mb, pair) =>
                        pair.Item2.GetMaybeTypeConstant(mb) //for runtime purposes, probably types are maybe types
            };
    }
}