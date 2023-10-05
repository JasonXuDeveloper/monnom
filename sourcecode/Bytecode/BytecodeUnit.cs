using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nom.Bytecode
{
    public class BytecodeUnit
    {
        // ReSharper disable InconsistentNaming
        public const uint BYTECODE_VERSION = 2;
        // ReSharper restore InconsistentNaming
        public AssemblyUnit AssemblyUnit { get; }
        protected List<ClassRep> ClassReps = new List<ClassRep>();
        protected List<InterfaceRep> InterfaceReps = new List<InterfaceRep>();
        protected List<IConstant> Constants = new List<IConstant>();

        protected T RegisterConstant<T>(T constant) where T : IConstant
        {
            Constants.Add(constant);
            return constant;
        }

        public IEnumerable<ClassRep> Classes => ClassReps;
        public IEnumerable<InterfaceRep> Interfaces => InterfaceReps;
        public string Name { get; }
        protected ulong ConstantCounter;

        public BytecodeUnit(string name, AssemblyUnit au)
        {
            AssemblyUnit = au;
            Name = name;
            TypeVarConstants = new ConstantDict<int, TypeVariableConstant>(index =>
                RegisterConstant(new TypeVariableConstant(++ConstantCounter, index)));
            StringConstants = new ConstantDict<string, StringConstant>(str =>
                str.Length > 0
                    ? RegisterConstant(new StringConstant(str, ++ConstantCounter))
                    : StringConstant.EmptyStringConstant);
        }

        protected readonly Dictionary<string, IClassConstant> ClassConstants = new();

        public IConstantRef<IClassConstant> GetClassConstant(string fullQualifiedClassName, string libraryName = "Std")
        {
            if (string.IsNullOrEmpty(libraryName))
            {
                libraryName = "Std";
            }

            if (ClassConstants.TryGetValue($"{libraryName}:{fullQualifiedClassName}", out var constant))
            {
                return new ConstantRef<IClassConstant>(constant);
            }

            constant = new ClassConstant(++ConstantCounter, GetStringConstant(libraryName),
                GetStringConstant(fullQualifiedClassName));
            ClassConstants.Add($"{libraryName}:{fullQualifiedClassName}", constant);
            Constants.Add(constant);
            return new ConstantRef<IClassConstant>(constant);
        }

        public IConstantRef<SuperInterfacesConstant> GetSuperInterfacesConstant(
            IEnumerable<(IConstantRef<IInterfaceConstant> iCon, IConstantRef<TypeListConstant> tCon)> superInterfaces)
        {
            return new ConstantRef<SuperInterfacesConstant>(
                RegisterConstant(new SuperInterfacesConstant(++ConstantCounter, superInterfaces)));
        }

        protected readonly Dictionary<string, SuperClassConstant> SuperClassConstants = new();

        public IConstantRef<SuperClassConstant> GetSuperClassConstant(string key,
            IConstantRef<IClassConstant> classConstant,
            IConstantRef<TypeListConstant> typeListConstant)
        {
            if (SuperClassConstants.TryGetValue(key, out var constant))
            {
                return new ConstantRef<SuperClassConstant>(constant);
            }

            constant = new SuperClassConstant(classConstant, typeListConstant, ++ConstantCounter);
            SuperClassConstants.Add(key, constant);
            Constants.Add(constant);
            return new ConstantRef<SuperClassConstant>(constant);
        }

        protected readonly Dictionary<string, InterfaceConstant> InterfaceConstants = new();

        public IConstantRef<InterfaceConstant> GetInterfaceConstant(string fullQualifiedClassName,
            string libraryName = "Std")
        {
            if (string.IsNullOrEmpty(libraryName))
            {
                libraryName = "Std";
            }

            if (InterfaceConstants.TryGetValue($"{libraryName}:{fullQualifiedClassName}", out var constant))
            {
                return new ConstantRef<InterfaceConstant>(constant);
            }

            constant = new InterfaceConstant(++ConstantCounter, GetStringConstant(libraryName),
                GetStringConstant(fullQualifiedClassName));
            InterfaceConstants.Add($"{libraryName}:{fullQualifiedClassName}", constant);
            Constants.Add(constant);
            return new ConstantRef<InterfaceConstant>(constant);
        }

        protected readonly Dictionary<string, ClassTypeConstant> NamedTypeConstants = new();

        public IConstantRef<ClassTypeConstant> GetNamedTypeConstant(string key,
            IConstantRef<INamedConstant> nameConstant,
            IConstantRef<TypeListConstant> typeListConstant)
        {
            if (NamedTypeConstants.TryGetValue(key, out var constant))
            {
                return new ConstantRef<ClassTypeConstant>(constant);
            }

            constant = new ClassTypeConstant(++ConstantCounter, nameConstant, typeListConstant);
            NamedTypeConstants.Add(key, constant);
            Constants.Add(constant);
            return new ConstantRef<ClassTypeConstant>(constant);
        }

        public IConstantRef<TypeParametersConstant> GetTypeParametersConstant(IEnumerable<TypeParameterEntry> entries)
        {
            var typeParameterEntries = entries?.ToList();
            if (typeParameterEntries == null || typeParameterEntries.Count == 0)
            {
                return new ConstantRef<TypeParametersConstant>(EmptyTpc);
            }

            IOptional<TypeParametersConstant> ret = TypeParametersConstants.FirstOrDefault(tpc =>
                    tpc.Entries.Count() == typeParameterEntries.Count() &&
                    tpc.Entries.Zip(typeParameterEntries, (x, y) => x.Equals(y)).All(x => x))
                .InjectOptional();
            if (!ret.HasElem)
            {
                ret = RegisterConstant(new TypeParametersConstant(++ConstantCounter, typeParameterEntries)).InjectOptional();
                TypeParametersConstants.Add(ret.Elem);
            }

            return new ConstantRef<TypeParametersConstant>(ret.Elem);
        }

        public IConstantRef<TypeListConstant> GetTypeListConstant(IEnumerable<IConstantRef<ITypeConstant>> types)
        {
            var constantRefs = types?.ToList();
            if (constantRefs == null || constantRefs.Count == 0)
            {
                return new ConstantRef<TypeListConstant>(EmptyTlc);
            }

            List<IConstantRef<ITypeConstant>> typeConstants = constantRefs.ToList();
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

        protected readonly Dictionary<ITypeConstant, MaybeTypeConstant> MaybeTypeConstants = new();

        public IConstantRef<MaybeTypeConstant> GetMaybeTypeConstant(IConstantRef<ITypeConstant> typeConstant)
        {
            if (MaybeTypeConstants.TryGetValue(typeConstant.Constant, out var constant))
            {
                return new ConstantRef<MaybeTypeConstant>(constant);
            }

            constant = new MaybeTypeConstant(++ConstantCounter, typeConstant);
            MaybeTypeConstants.Add(typeConstant.Constant, constant);
            Constants.Add(constant);
            return new ConstantRef<MaybeTypeConstant>(constant);
        }


        public void Emit(Func<String, Stream> openStream)
        {
            using (Stream fs = openStream(Name))
            {
                fs.WriteValue(BYTECODE_VERSION);
                foreach (IConstant constant in Constants)
                {
                    constant.Emit(fs);
                }

                foreach (ClassRep cr in ClassReps)
                {
                    cr.Emit(fs);
                }

                foreach (InterfaceRep ir in InterfaceReps)
                {
                    ir.Emit(fs);
                }
            }
        }

        protected class ConstantRef<T> : IConstantRef<T> where T : IConstant
        {
            public ConstantRef(T constant)
            {
                Constant = constant;
                ConstantID = constant.ConstantID;
            }

            public T Constant { get; }
            public ulong ConstantID { get; }
        }

        protected class ConstantDict<V, C> where C : IConstant
        {
            private readonly Func<V, C> converter;
            private Dictionary<V, C> entries = new Dictionary<V, C>();

            public ConstantDict(Func<V, C> converter)
            {
                this.converter = converter;
            }

            public IConstantRef<C> GetConstant(V value)
            {
                if (!entries.ContainsKey(value))
                {
                    entries.Add(value, converter(value));
                }

                return new ConstantRef<C>(entries[value]);
            }
        }

        public void AddClass(ClassRep cls)
        {
            ClassReps.Add(cls);
        }

        public void AddInterface(InterfaceRep ifc)
        {
            InterfaceReps.Add(ifc);
        }

        public IConstantRef<IClassConstant> GetEmptyClassConstant() =>
            new ConstantRef<IClassConstant>(EmptyClassConstant.Instance);

        protected ConstantDict<String, StringConstant> StringConstants;

        public IConstantRef<StringConstant> GetStringConstant(string str)
        {
            return StringConstants.GetConstant(str);
        }

        protected static TypeListConstant EmptyTlc = new TypeListConstant(0, new List<IConstantRef<ITypeConstant>>());
        protected List<TypeListConstant> TypeListConstants = new List<TypeListConstant>();

        protected static TypeParametersConstant
            EmptyTpc = new TypeParametersConstant(0, new List<TypeParameterEntry>());

        protected List<TypeParametersConstant> TypeParametersConstants = new List<TypeParametersConstant>();

        protected BottomTypeConstant BottomTypeConstant;

        public IConstantRef<ITypeConstant> GetBottomTypeConstant()
        {
            if (BottomTypeConstant == null)
            {
                BottomTypeConstant = RegisterConstant(new BottomTypeConstant(++ConstantCounter));
            }

            return new ConstantRef<BottomTypeConstant>(BottomTypeConstant);
        }

        protected ConstantDict<int, TypeVariableConstant> TypeVarConstants;

        public IConstantRef<TypeVariableConstant> GetTypeVariableConstant(int index)
        {
            return TypeVarConstants.GetConstant(index);
        }

        private DynamicTypeConstant dynamicTypeConstant;

        public IConstantRef<ITypeConstant> GetDynamicTypeConstant()
        {
            if (dynamicTypeConstant == null)
            {
                dynamicTypeConstant = RegisterConstant(new DynamicTypeConstant(++ConstantCounter));
            }

            return new ConstantRef<ITypeConstant>(dynamicTypeConstant);
        }

        static ITypeConstant _zeroTypeConstant = new EmptyTypeConstant();

        public static IConstantRef<ITypeConstant> GetEmptyTypeConstant() =>
            new ConstantRef<ITypeConstant>(_zeroTypeConstant);
    }
}