dmcs ./src/*.cs -r:./src/bin/Debug/alglibnet2.dll -r:./src/bin/Debug/WordsMatching.dll -r:./src/bin/Debug/Microsoft.Solver.Foundation.dll -r:./src/bin/Debug/WordNetClasses.dll -o ./LanguageGrounding.exe
mkbundle -o LanguageGrounding ./LanguageGrounding.exe ./src/bin/Debug/alglibnet2.dll ./src/bin/Debug/WordsMatching.dll ./src/bin/Debug/Microsoft.Solver.Foundation.dll ./src/bin/Debug/WordNetClasses.dll
