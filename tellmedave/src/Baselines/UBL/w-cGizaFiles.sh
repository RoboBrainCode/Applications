giza="./giza-pp/GIZA++-v2"
$giza/plain2snt.out $giza'/english.txt' $giza'/lambda.txt'
$giza/snt2cooc.out $giza'/english.vcb' $giza'/lambda.vcb' $giza'/english_lambda.snt' > $giza/'english_lambda_cooc.cooc'
$giza/GIZA++ -S $giza'/english.vcb' -T $giza'/lambda.vcb' -C $giza'/english_lambda.snt' -CoocurrenceFile $giza/'english_lambda_cooc.cooc'
