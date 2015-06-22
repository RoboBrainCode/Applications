def propertyToDict(label,phase):
	if phase == 0 or phase == 2:
		return {'handle':label}
	else:
		return {'label':label}

def extractPathIndices(path):
	'''takes 'a..b' as input 
		return dic {'start':start, 'end':end}
	'''
	temp_s = path.find('.')
	if temp_s == -1:
		return {'start':int(path), 'end':int(path)}
	else: 
		if temp_s != 0:
			start = int(path[:temp_s])
		else:
			start = 1
		end = int(path[temp_s+2:])
		return {'start':start, 'end':end}



def initParser(NoR, phase):
	'''takes input the string to be parsed 
		returns weaver function index to be called and propertyList
	'''
	# print NoR
	counter = 0
	a = '0'
	b = '0'
	propertyList = {} 
	for c in NoR:
		if c.isalpha():
			# print NoR
			a = '1'
			break
		if c == '{' or c == ':':
			break
		counter = counter + 1	
	propertyCheck = NoR.find('{')
	if propertyCheck != -1:
		b = '1'
	
	if phase == 0 or phase == 2:
		if b == '1':
			labelStart = NoR.rfind(':')
			propertyList = propertyToDict(NoR[labelStart+2:-2],phase)
		return a, b, propertyList

	elif phase == 1:
		propertyStart = NoR.find('`')
		if propertyStart != -1:
			b = '1'
			propertyStop = NoR.rfind('`')
			propertyList = propertyToDict(NoR[propertyStart+1:propertyStop],phase)
		multipleHop = NoR.find('*')
		
		if multipleHop != -1:
			propertyList = extractPathIndices(NoR[multipleHop+1:])	
			return a, b, '1', propertyList
		return a, b, '0', propertyList	


def cyParser(pattern):
	'''for parsing the query into 3 phase
	starting node, edge and ending node
	then finding variables to be returned'''
	
	s1 = pattern.find('(')
	s2 = pattern.find(')')
	#starting node
	node_s = pattern[s1+1:s2]
	# print node_s
	
	r1 = pattern.find('[')
	r2 = pattern.rfind(']')
	#relationship 
	relationShip = pattern[r1+1:r2]
	# print relationShip

	e1 = pattern[s2+1:].find('(')
	e2 = pattern[s2+1:].find(')')
	#ending node
	node_e  = pattern[s2+1+e1+1:s2+1+e2]
	# print node_e

	#dealing with each part separately
	#inialization of the vars to be returned
	indicator = "0000000"
	
	#processing of 1 phase at a time
	
	# indicator[0], indicator[1], propertyList = initParser(node_s,0)
	a, b, dict_s = initParser(node_s,0)
	# print a,b,indicator
	indicator = a + b + indicator[2:7]
	# print a,b,indicator
	
	# indicator[4], indicator[5], propertyList = initParser(node_e,2)
	a, b, dict_e = initParser(node_e,2)
	indicator = indicator[:4] + a + b + indicator[6]
	# print a,b,indicator
	
	# indicator[2], indicator[3], indicator[6], propertyList = initParser(relationShip,1)
	a, b, c, propertyList = initParser(relationShip,1)
	indicator = indicator[:2] + a + b + indicator[4:6] + c
	# print indicator

	if indicator[4] == '1':
		if indicator[3] == '0':
			if 'start' in propertyList:
				return 6, dict_s, propertyList, dict_e
			return 0, dict_s, propertyList, dict_e
		else:
			return 1, dict_s, propertyList, dict_e
	
	elif indicator[0] == '1':
		if indicator[3] == '0':
			if 'start' in propertyList:
				return 7, dict_s, propertyList, dict_e
			return 2, dict_s, propertyList, dict_e
		else:
			return 3, dict_s, propertyList, dict_e
	
	elif indicator[2] == '1':
		if indicator[3] == '1':
			return 4, dict_s, propertyList, dict_e
		elif indicator[6] == '1':
			return 5, dict_s, propertyList, dict_e
		elif indicator[1] == '1' and indicator[5] == '1':
			return 	4, dict_s, propertyList, dict_e
		else:	
			return 'invalid input'
		
### weaver function index
'''
0: forward search node with no label
1: forward search node with label
2: backward search node with no label
3: backward search node with label
4: discover path
5: multiple hop
6: n-hop forward
7: n-hop backward
'''

# def main():
# 	# print initParser("a{handle:'wall'}",0)
# 	# print extractPathIndices('..3')
# 	print cyParser("({handle:'wall'})-[]->(e)")
# 	print cyParser("({handle:'wall'})-[:`HAS_MATERIAL`]->(e)")
# 	print cyParser("(s)-[:`HAS_MATERIAL`]->({handle:'wall'})")
# 	print cyParser("(s)-[]->({handle:'wall'})")
# 	print cyParser("({handle:'wall'})-[e]->({handle:'metal'})")
# 	print cyParser("({handle:'wall'})-[e*..5]->({handle:'metal'})")
# 	print cyParser("({handle:'wall'})-[e*2..5]->({handle:'metal'})")
# 	print cyParser("({handle:'wall'})-[e*5]->({handle:'metal'})")
# 	print cyParser("({handle:'wall'})-[*2..5]->(e)")
# 	print cyParser("(v)-[*5]->({handle:'wall'})")		
		

# main()