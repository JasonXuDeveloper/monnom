#pragma once
#pragma once
#include "PWrapper.h"

namespace Nom
{
	namespace Runtime
	{
		class PWCheckReturnValueFunction: public PWrapper
		{
		public:
			PWCheckReturnValueFunction(llvm::Value* _wrapped) : PWrapper(_wrapped)
			{

			}
		};
	}
}