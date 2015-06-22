#File Description: split train into two files, english.txt and lambda.txt

readFile = open("/Users/Ella/Documents/research/Parsing/UBL/experiments/new/data/en/run-0/fold-0/train","r")
english = open("./giza-pp/GIZA++-v2/english.txt","w")
lambdaExpression = open("./giza-pp/GIZA++-v2/lambda.txt","w")
num=0
for line in readFile:
    if num==0:
        english.write(line)
    elif num==1:
        line=line.replace("(","")
        line=line.replace(")","")
        lambdaExpression.write(line)
    num=(num+1)%3
readFile.close()
english.close()
lambdaExpression.close()
    
    