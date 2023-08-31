# ensure have homebrew
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
# ensure dotnet
brew install dotnet
# prerequisites
brew install llvm@12
brew link llvm@12
brew install bdw-gc
brew link bdw-gc
brew install tbb
brew link tbb