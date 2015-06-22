def addSpace(readFileName,writeFileName):
    """Function Description: transfer the crude probability table into a standard probability table"""
    readFile = open("./UBL/experiments/new/data/en/run-0/"+readFileName+".actual.ti.final","r")
    writeFile = open("./UBL/experiments/new/data/en/run-0/fold-0/"+writeFileName+".giza_probs","w")
    for line in readFile:
        list = line.split(" ")
        writeFile.write(list[0]+"  ::  "+list[1]+"  ::  "+list[2])
    writeFile.close()
    readFile.close()
    
addSpace("english-lambda","c-w")
addSpace("lambda-english","w-c")
