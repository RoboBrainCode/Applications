from weaverParser import cyParser
import json
import yaml
import urllib,requests
import ast
def PostWeaverQuery(fnName,params):
		query=dict(fnName=fnName,params=params)
		myport=3232
		data=json.dumps(query)
		myURL = "http://52.25.65.189:%s/weaverWrapper/execFn/" % (myport)
		headers = {'Content-type': 'application/json', 'Accept': 'text/plain'}
		r = requests.get(myURL, data=data,headers=headers)
		response=yaml.safe_load(r.text)
		return response

def fetchQuery(cypherQuery):
	start=cypherQuery.find('MATCH')
	end=cypherQuery.find('RETURN')
	end_2=cypherQuery.find('LIMIT')
	query=cypherQuery[start+5:end].strip()
	args=cypherQuery[end+6:end_2].strip()
	args=args.split(',')
	if len(args)>1:
		print 'Not Supported'
		return False
	
	funcNum, dict_s, propertyList, dict_e = cyParser(query)
	if funcNum == 0:
		fnName='returnNodeOneHopForward'
		params=dict(src=dict_s['handle'],properties={})
		result=PostWeaverQuery(fnName,params)
		return result
	elif funcNum == 1:
		fnName='returnNodeOneHopForward'
		params=dict(src=dict_s['handle'],properties=propertyList)
		result=PostWeaverQuery(fnName,params)
		return result
	elif funcNum == 2:
		fnName='returnNodeOneHopBackward'
		params=dict(src=dict_e['handle'],properties={})
		result=PostWeaverQuery(fnName,params)
		return result
	elif funcNum == 3:
		fnName='returnNodeOneHopBackward'
		params=dict(src=dict_e['handle'],properties=propertyList)
		result=PostWeaverQuery(fnName,params)
		return result
	elif funcNum == 4:
		print dict_s['handle'],dict_e['handle']
	elif funcNum == 5:
		fnName='returnPathMinMax'
		params=dict(src=dict_s['handle'],dest=dict_e['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
		result=PostWeaverQuery(fnName,params)
		return result
	elif funcNum ==6:
		fnName='returnNodesForward'
		params=dict(src=dict_s['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
		result=PostWeaverQuery(fnName,params)
		return result
	elif funcNum==7:
		fnName='returnNodesBackward'
		params=dict(src=dict_e['handle'],path_len_min=propertyList['start'],path_len_max=propertyList['end'])
		result=PostWeaverQuery(fnName,params)
		return result
	elif funcNum==8:
		fnName='getNode'
		params=dict(node=dict_s['handle'])
		result=PostWeaverQuery(fnName,params)
		return result
	else:
		return 'invalid_input'

	return True

def updateQuery(nodeProps):
	fnName='updateNodeProps'
        params=dict(Id=nodeProps['handle'],nodeProps=nodeProps)
        result=PostWeaverQuery(fnName,params)
        return result

if __name__ == "__main__":
	runQuery("MATCH ({handle:'phone'})")
	# runQuery("MATCH ({handle:'wall'})-[]->(e) RETURN e")

