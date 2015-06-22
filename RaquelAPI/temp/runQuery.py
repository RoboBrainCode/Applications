from weaverParser import cyParser
from weaverWrapperFns import *
def runQuery(cypherQuery):
	start=cypherQuery.find('MATCH')
	end=cypherQuery.find('RETURN')
	end_2=cypherQuery.find('LIMIT')
	query=cypherQuery[start+5:end].strip()
	args=cypherQuery[end+6:end_2].strip()
	args=args.split(',')
	if len(args)>1:
		print 'Not Supported'
		return False
	
	# print query
	# cyParser(query)
	funcNum, dict_s, propertyList, dict_e = cyParser(query)
	# print funcNum, dict_s,propertyList,dict_e
	if funcNum == 0:
		return returnNodeOneHopForward(dict_s['handle'],{})
	elif funcNum == 1:
		return returnNodeOneHopForward(dict_s['handle'],propertyList)
	elif funcNum == 2:
		return returnNodeOneHopBackward(dict_e['handle'],{})
		# return '2'
	elif funcNum == 3:
		return returnNodeOneHopBackward(dict_e['handle'],propertyList)
		# return '3'
	elif funcNum == 4:
		# print dict_s['handle'],dict_e['handle']
		return returnPathMinMax(src=dict_s['handle'],dest=dict_e['handle'],path_len_min=1,path_len_max=1)	
	elif funcNum == 5:
		return returnPathMinMax(dict_s['handle'],dict_e['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
	elif funcNum ==6:
		return returnNodesForward(src=dict_s['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
	elif funcNum==7:
		return returnNodesBackward(src=dict_e['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
		# return '4'
	else:
		return 'invalid_input'

	return True


if __name__ == "__main__":
	runQuery("MATCH ({handle:'wall'})-[]->(e) RETURN e")
	runQuery("MATCH ({handle:'wall'})-[]->(e) RETURN e")
