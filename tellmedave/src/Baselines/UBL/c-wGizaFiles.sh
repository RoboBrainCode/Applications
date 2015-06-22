giza="./giza-pp/GIZA++-v2"
$giza/plain2snt.out $giza'/lambda.txt' $giza'/english.txt'
$giza/snt2cooc.out $giza'/lambda.vcb' $giza'/english.vcb' $giza'/lambda_english.snt' > 	$giza/'lambda_english_cooc.cooc'
$giza/GIZA++ -S $giza'/lambda.vcb' -T $giza'/english.vcb' -C $giza'/lambda_english.snt' -CoocurrenceFile $giza/'lambda_english_cooc.cooc'
