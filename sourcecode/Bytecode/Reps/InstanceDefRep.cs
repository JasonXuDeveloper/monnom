using System;
using Nom.Language;

namespace Nom.Bytecode
{
    public class InstanceDefRep : IInstanceSpec
    {
        public Visibility Visibility => throw new NotImplementedException();
        INamespaceSpec IMember.Container => Container;
        public IClassSpec Container { get; }
    }
}
