#include "PWInterface.h"
#include "RTInterface.h"
#include "CompileHelpers.h"
#include "PWSuperInstance.h"
#include "PWSignature.h"
#include "PWCastFunction.h"
#include "PWCheckReturnValueFunction.h"

using namespace llvm;
using namespace std;
namespace Nom
{
	namespace Runtime
	{
		llvm::Type* PWInterface::GetLLVMType()
		{
			return RTInterface::GetLLVMType();
		}
		llvm::Type* PWInterface::GetWrappedLLVMType()
		{
			return NLLVMPointer(GetLLVMType());
		}
		PWInt64 PWInterface::ReadSuperInstanceCount(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::SuperTypesCount), "SuperTypesCount", AtomicOrdering::NotAtomic);
		}
		PWInt64 PWInterface::ReadSuperClassCount(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::SuperClassCount), "SuperClassCount", AtomicOrdering::NotAtomic);
		}
		PWInt64 PWInterface::ReadSuperInterfaceCount(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::SuperInterfaceCount), "SuperInterfaceCount", AtomicOrdering::NotAtomic);
		}
		PWFlatArr<PWSuperInstance> PWInterface::ReadSuperInstances(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::SuperTypes), "SuperTypes", AtomicOrdering::NotAtomic);
		}
		PWFlatArr<PWSuperInstance> PWInterface::ReadSuperInterfaces(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::SuperInterfaces), "SuperInterfaces", AtomicOrdering::NotAtomic);
		}
		PWInt32 PWInterface::ReadTypeArgCount(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::TypeArgCount), "TypeArgumentCount", AtomicOrdering::NotAtomic);
		}
		PWSignature PWInterface::ReadSignature(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::Signature), "Signature", AtomicOrdering::NotAtomic);
		}
		PWCheckReturnValueFunction PWInterface::ReadReturnValueCheckFunction(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::ReturnValueCheckFunction), "ReturnValueCheckFunction", AtomicOrdering::NotAtomic);
		}
		llvm::Value* PWInterface::ReadFlags(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::Flags), "InterfaceFlags", AtomicOrdering::NotAtomic);
		}
		llvm::Value* PWInterface::ReadIRPtr(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::IRPtr), "InterfaceFlags", AtomicOrdering::NotAtomic);
		}
		PWCastFunction PWInterface::ReadCastFunction(NomBuilder& builder)
		{
			return MakeInvariantLoad(builder, GetLLVMType(), wrapped, MakeInt32(RTInterfaceFields::CastFunction), "CastFunction", AtomicOrdering::NotAtomic);
		}
	}
}