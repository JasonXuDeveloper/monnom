#include "RTRawFloat.h"
#include "RTRawInt.h"
#include "RTRawBool.h"
#include "RTVMPtr.h"
#include "RTObject.h"
#include "RTRefValue.h"
#include "PWRefValue.h"

using namespace llvm;
namespace Nom
{
	namespace Runtime
	{
		const RTRawFloat* RTRawFloat::Get(NomBuilder& builder, const PWFloat _value, bool _isfc)
		{
			return new(builder.Malloc(sizeof(RTRawFloat))) RTRawFloat(_value, GetFloatClassType(), _isfc);
		}
		NomTypeRef RTRawFloat::GetNomType() const
		{
			return GetFloatClassType();
		}
		const RTPWValuePtr<PWInt64> RTRawFloat::AsRawInt(NomBuilder& builder, RTValuePtr orig = nullptr, bool check) const
		{
			return RTConvValue < PWInt64, NomClassTypeRef, RTRawInt > ::Get(builder, builder->CreateFPToSI(value.wrapped, INTTYPE, "floatToInt"), static_cast<NomClassTypeRef>(GetIntClassType()), orig.Coalesce(this));
		}
		const RTPWValuePtr<PWFloat> RTRawFloat::AsRawFloat(NomBuilder& builder, RTValuePtr orig = nullptr, bool check) const
		{
			return this;
		}
		const RTPWValuePtr<PWBool> RTRawFloat::AsRawBool(NomBuilder& builder, RTValuePtr orig = nullptr, bool check) const
		{
			return RTConvValue<PWBool, NomClassTypeRef, RTRawBool>::Get(builder, builder->CreateFCmpUNE(value.wrapped, ConstantFP::get(FLOATTYPE, 0.0), "floatIsNotNull"), static_cast<NomClassTypeRef>(GetBoolClassType()), orig.Coalesce(this));
		}
		const RTPWValuePtr<PWRefValue> RTRawFloat::AsRefValue(NomBuilder& builder, RTValuePtr orig = nullptr) const
		{
			return RTConvValue<PWRefValue, NomClassTypeRef, RTRefValue>::Get(builder, PackFloat(builder, value.wrapped), static_cast<NomClassTypeRef>(GetFloatClassType()), orig.Coalesce(this));
		}
		const RTPWValuePtr<PWVMPtr> RTRawFloat::AsVMPtr(NomBuilder& builder, RTValuePtr orig = nullptr) const
		{
			return RTConvValue<PWVMPtr, NomClassTypeRef, RTVMPtr>::Get(builder, PWVMPtr(builder->CreateIntToPtr(builder->CreateBitCast(value.wrapped, INTTYPE), POINTERTYPE)), static_cast<NomClassTypeRef>(GetFloatClassType()), orig.Coalesce(this));
		}
		const RTPWValuePtr<PWObject> RTRawFloat::AsObject(NomBuilder& builder, RTValuePtr orig = nullptr, bool check) const
		{
			throw new std::exception(); //technically possible, but stupid; should not happen
		}
		const RTPWValuePtr<PWLambda> RTRawFloat::AsLambda(NomBuilder& builder, RTValuePtr orig = nullptr, bool check) const
		{
			throw new std::exception();
		}
		const RTPWValuePtr<PWPartialApp> RTRawFloat::AsPartialApp(NomBuilder& builder, RTValuePtr orig = nullptr, bool check) const
		{
			throw new std::exception();
		}
		const RTPWValuePtr<PWRecord> RTRawFloat::AsRecord(NomBuilder& builder, RTValuePtr orig = nullptr, bool check) const
		{
			throw new std::exception();
		}
		const RTPWValuePtr<PWStructVal> RTRawFloat::AsStructVal(NomBuilder& builder, RTValuePtr orig = nullptr, bool check) const
		{
			throw new std::exception();
		}
		const RTPWValuePtr<PWPacked> RTRawFloat::AsPackedValue(NomBuilder& builder, RTValuePtr orig) const
		{
			return nullptr;
		}
		int RTRawFloat::GenerateRefOrPrimitiveValueSwitchUnpackPrimitives(NomBuilder& builder, std::function<void(NomBuilder&, RTPWValuePtr<PWRefValue>)> onRefValue, std::function<void(NomBuilder&, RTPWValuePtr<PWInt64>)> onPrimitiveInt, std::function<void(NomBuilder&, RTPWValuePtr<PWFloat>)> onPrimitiveFloat, std::function<void(NomBuilder&, RTPWValuePtr<PWBool>)> onPrimitiveBool, bool unboxObjects, uint64_t refWeight, uint64_t intWeight, uint64_t floatWeight, uint64_t boolWeight) const
		{
			BasicBlock* newBB = BasicBlock::Create(builder->getContext(), "isPrimitiveFloat", builder->GetInsertBlock()->getParent());
			builder->CreateBr(newBB);
			builder->SetInsertPoint(newBB);
			onPrimitiveFloat(builder, this);
			return 1;
		}
	}
}