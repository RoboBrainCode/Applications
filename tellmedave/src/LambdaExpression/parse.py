#find all sentences
test = open("/Users/Ella/Documents/research/Parsing/UBL/experiments/new/data/en/run-0/fold-0/test","r")
sentences = []
num = 0
for sentence in test:
    if num==0:   
        sentences.append(sentence)
    num=(num+1)%3
   
#copy the lambda expressions and parsing score 
lambdaFile = open("/Users/Ella/Documents/research/Parsing/UBL/experiments/new/run.dev.en.0.0","r")
lines = lambdaFile.readlines()
lambdaExpressions = []#save lambda expressions
counter = len(lines)-1
save=False
while (lines[counter].find("0: ==================(0 -- 0)")==-1 and counter>=0):
    line = lines[counter]
    if (save==True):
        indexLeft=line.find("[")
        indexRight=line.find("]")
        lambdaExpressions.append(line[indexLeft+1:indexRight])
        save=False
    elif (line.find("[LexEntries and scores:")>-1):
        save=True
    counter=counter-1
lambdaExpressions[::-1]
test.close()
lambdaFile.close()

#parse lambda expressions
