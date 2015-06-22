def initParser(NoR):
	'''for parsing node(N) or relationship(R) from the given string 
		into string that can be returned via cypher query'''
	#initialization step
	counter = 0
	query = ""
	returnStr = ""
	propertyList = ""
	propertyFound = False
	letterFound = False

	for c in NoR:
		if c.isalpha():
			letterFound = True
			break
		if c == '{':
			propertyFound = True
			break
		if c == '`':
			break
		counter = counter +1
	
	if propertyFound == True:
		propertyEnd_index = NoR.find("}")
		returnStr = ""
		return -1

	#to deal when both letter and/or property are present
	if letterFound:
		#to check for property
		propertyStart_index = NoR.find('{')
		if propertyStart_index != -1:
			propertyFound = True

		last = NoR.find(':')	#common statement to both parts of if
		
		if propertyFound == False:
			query = NoR
			if last != -1:
				returnStr = NoR[:last]
			else:
				returnStr = NoR
			#asterisk check
			asteriskStart = returnStr.find('*')
			if asteriskStart == -1:
				return returnStr
			else:
				return returnStr[:asteriskStart]
		
		# when both letter and property present
		else:	
			if last > propertyStart_index:
				last = propertyStart_index
			returnStr = NoR[:last]
			propertyEnd_index = NoR.find("}")
			return returnStr
	else: 
		return -1
	
def cyParser(pattern):
	'''for parsing the query into 3 phase
	starting node, edge and ending node
	then finding variables to be returned'''
	
	s1 = pattern.find('(')
	s2 = pattern.find(')')
	#starting node
	node_s = pattern[s1+1:s2]
	
	r1 = pattern.find('[')
	r2 = pattern.rfind(']')
	#relationship 
	relationShip = pattern[r1+1:r2]
	
	e1 = pattern[s2+1:].find('(')
	e2 = pattern[s2+1:].find(')')
	#ending node
	node_e  = pattern[s2+1+e1+1:s2+1+e2]
	
	#dealing with each part separately
	#inialization of the vars to be returned
	returnStr = ""
	insertComma = False
	
	init_s = initParser(node_s)
	if init_s != -1:
		returnStr  = returnStr + init_s 
		insertComma = True
	
	init_r = initParser(relationShip)
	if init_r != -1:
		if insertComma:
			returnStr = returnStr + "," + init_r 
		else:
			returnStr = returnStr + init_r 
			insertComma = True

	init_e = initParser(node_e)
	if init_e != -1:
		if insertComma:
			returnStr = returnStr + "," + init_e 
		else:
			returnStr = returnStr + init_e 

	# print returnStr
	return returnStr


# def main():
	
# 	#test cases
# 	# cyParser("({handle:'wall'})-[`HAS_MATERIAL`]->(b)")
# 	# cyParser("(v)-[`HAS_MATERIAL`]->({handle:'wall'})")
# 	# cyParser("(a{handle:'wall'})-[`HAS_MATERIAL`]->(v{src:'HAS_AFFORDANCE'})")
# 	print cyParser("({handle:'wall'})-[r*]->({handle:'cup',type'metal'})")

# main()