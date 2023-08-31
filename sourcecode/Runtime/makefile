CPPFLAGS = -D__STDC_CONSTANT_MACROS -D__STDC_FORMAT_MACROS -D__STDC_LIMIT_MACROS -fexceptions -std=c++17
LLFLAGS = /usr/local/Cellar/llvm@12/12.0.1_1/lib/libc++.dylib -L/usr/local/lib -ltbb -lgc $(shell llvm-config --ldflags --libs all)
INCLUDES = -I/opt/intel/oneapi/vtune/latest/include -I/usr/local/opt/onetbb/include -I/usr/local/include

VPATH = .:./instructions:./pugixml

objects := $(patsubst %.cpp,%.o,$(wildcard *.cpp)) $(patsubst instructions/%.cpp,%.o,$(wildcard instructions/*.cpp)) $(patsubst pugixml/%.cpp,%.o,$(wildcard pugixml/*.cpp))
debugobjects := $(patsubst %.cpp,%.debug.o,$(wildcard *.cpp)) $(patsubst instructions/%.cpp,%.debug.o,$(wildcard instructions/*.cpp)) $(patsubst pugixml/%.cpp,%.debug.o,$(wildcard pugixml/*.cpp))


.DEFAULT: nombuild
.PHONY: nombuild
nombuild: headers nom

.PHONY: rebuild
rebuild: clean nombuild

nom: $(objects)
	clang++ -rdynamic -O3 -o nom $(CPPFLAGS) $(LLFLAGS) $^ 

nomdebug: $(debugobjects)
	clang++ -rdynamic -g -O0 -o nom $(CPPFLAGS) $(LLFLAGS) $^ 


.PHONY: clean
clean:
	-rm *.o
	-rm nom
	-rm *.d

%.o : %.cpp
	clang++ $(CPPFLAGS) $(INCLUDES) -O3 -c $< -o $@

%.debug.o : %.cpp
	clang++ $(CPPFLAGS) $(INCLUDES) -g -O0 -c $< -o $@


sources = $(*.cpp)

deps = $(patsubst %.cpp,%.d,$(wildcard *.cpp)) $(patsubst instructions/%.cpp,%.d,$(wildcard instructions/*.cpp)) $(patsubst pugixml/%.cpp,%.d,$(wildcard pugixml/*.cpp))

headers: $(deps)

%.d: %.cpp
	@set -e; rm -f $@; \
	 $(CC) -MM $(CPPFLAGS) $(INCLUDES) $< > $@.$$$$; \
	 sed 's,\($*\)\.o[ :]*,\1.o $@ : ,g' < $@.$$$$ > $@; \
	 sed 's,\($*\)\.debug.o[ :]*,\1.debug.o $@ : ,g' < $@.$$$$ > $@; \
	 rm -f $@.$$$$


-include $(deps)

