#include "Path.h"
#include <locale>

namespace Nom
{
	namespace Runtime
	{
		using namespace std;
		Path::Path(const std::string &path) : path(FSNamespace::path(path
#ifndef __APPLE__
 ,std::locale("UTF-8")
#endif
))
		{
		}

		Path::~Path()
		{
		}
	}
}
