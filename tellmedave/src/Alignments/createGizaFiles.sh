#!/bin/bash
giza="./src/Alignments/giza-pp/GIZA++-v2"
$giza/plain2snt.out './src/Alignments/english.txt' './src/Alignments/code.txt'
$giza/snt2cooc.out './src/Alignments/english.vcb' './src/Alignments/code.vcb' './src/Alignments/english_code.snt' > './src/Alignments/english_code_cooc.cooc'
$giza/GIZA++ -S './src/Alignments/english.vcb' -T './src/Alignments/code.vcb' -C './src/Alignments/english_code.snt' -CoocurrenceFile './src/Alignments/english_code_cooc.cooc'
