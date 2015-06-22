import re
# m = re.match(r"[M][A][T][C][H] [(]([a-zA-Z0-9]*)[{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}][)][-]", "MATCH (a{wall:'hellasf123',wall1:'hello'})-")
# m1 = re.match(r"[\[][\ ]*(([a-zA-Z0-9_]*)[:]([`][a-zA-Z0-9_]+[`])*([{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}])*)*[\]]", "[asd:`A`{a:'sg',s:'cn'}]")
# m2=re.match(r"[-][>][(]([a-zA-Z0-9]*)[{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}][)][\ ]*[R][E][T][U][R][N][A-Za-z0-9\ _]", "->(asd{a:'sg',s:'cn'}) RETURN fvv")

# mF = re.match(r"[M][A][T][C][H][\ ]*[(]([a-zA-Z0-9]*)[{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}][)][-][\[][\ ]*(([a-zA-Z0-9_]*)[:]([`][a-zA-Z0-9_]+[`])*([{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}])*)*[\]][-][>][(]([a-zA-Z0-9]*)[{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}][)][\ ]*[R][E][T][U][R][N][A-Za-z0-9\ _]",query)

# print mF.group(1)
# print '{'+mF.group(2)+'}'
# print 
# print mF.group(6)
# print mF.group(7)
# print mF.group(8)
# print 
# print mF.group(12)
# print '{'+mF.group(13)+'}'

matchQuery="[M][A][T][C][H][\ ]*"
leftNode="[(]([a-zA-Z0-9]*)([{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}]){0,1}[)]"
connector1="[-]"

variable="([a-zA-Z0-9_]+)"
props="([{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}])"

edge="[\[][\ ]*(([a-zA-Z0-9_]*)(([:]))([`][a-zA-Z0-9_]+[`])**)*([\*]([0-9]*)(([.][.])([0-9]+)){0,1}){0,1}[\]]"

# edge="[\[][\ ]*([a-zA-Z0-9_]*)([:]{0,1})([{](([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+['][,])*([a-zA-Z0-9]+[:]['][a-zA-Z0-9]+[']){0,1})[}])"

connector2="[-][>]"
returnSt="[\ ]*[R][E][T][U][R][N][A-Za-z0-9\ _]"
patternStr=matchQuery+leftNode+connector1+edge+connector2+leftNode+returnSt

# query="MATCH (a{wall:'hellasf123',wall1:'hello'})-[asd:`A`{a:'sg',s:'cn'}]->(node{a:'node',s:'node'}) RETURN fvv"
# mF=re.match(patternStr,query)


num="([\*]([0-9]*)(([.][.])([0-9]+)){0,1}){0,1}"

testquery="a12:"
mF=re.match(variable,testquery)

# if (a=="") ,eand this id empty

print mF.group(1)
print mF.group(2)
print mF.group(3)
print mF.group(4)
print mF.group(5)
print mF.group(6)
print mF.group(7)
print mF.group(8)
print mF.group(9)
print mF.group(10)
print mF.group(11)
print mF.group(12)
print mF.group(13)
print mF.group(14)
print mF.group(15)
print mF.group(16)
print mF.group(17)
print mF.group(18)
print mF.group(19)
print mF.group(20)
print mF.group(21)
print mF.group(22)

# print mF.group(6)


# nodeVarL=mF.group(1)
# nodePropsL='{'+mF.group(2)+'}'
# edgeVar=mF.group(3)
# edgeLabel=mF.group(4)
# edgeProps=mF.group(5)
# leftNodeVar=mF.group(1)
# leftNodeProps='{'+mF.group(2)+'}'
# edgeVar=mF.group(6)
# edgeLabel=mF.group(7)
# edgeProps=mF.group(8)
# nodeVarR=mF.group(12)
# nodePropsR='{'+mF.group(13)+'}'

# print nodeVarL
# print nodePropsL
# print edgeVar
# print edgeLabel
# print edgeProps
# print nodeVarR
# print nodePropsR


