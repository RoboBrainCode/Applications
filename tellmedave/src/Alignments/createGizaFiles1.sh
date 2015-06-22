#!/bin/bash
giza="./giza-pp/GIZA++-v2"

$giza/plain2snt.out './english.txt' './code.txt'
echo "$------Printing File Code.vcb and English.vcb--$\n\n"
cat './code.vcb'
cat './english.vcb'
echo "$------Printing File english_code.snt and code_english.snt--$\n\n"
cat './english_code.snt'
cat './code_english.snt'

$giza/snt2cooc.out './english.vcb' './code.vcb' './english_code.snt' > './english_code_cooc.cooc'
echo "$------Printing Coocurrence File--$\n\n"
cat './english_code_cooc.cooc'

$giza/GIZA++ -S './english.vcb' -T './code.vcb' -C './english_code.snt' -CoocurrenceFile './english_code_cooc.cooc'
