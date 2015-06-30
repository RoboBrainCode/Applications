import numpy as np
import requests
import urllib
import yaml
import RaquelAPI.raquel as raquel
import numpy as np
import random
random.seed(100)
def parseStrToJson(varDict):
		newDict=dict()
		for key,val in varDict.iteritems():
				val=val.split(',')
				try:
						for i in range(len(val)):
								val[i]=float(val[i])
				except: 
						pass
				else:   
						if (len(val)>1):
								val=np.asarray(val)
						else:   
								val=val[0]
						newDict[key]=val
		return newDict


def parseStrToJsonAddNoise(varDict):
		newDict=dict()
		for key,val in varDict.iteritems():
				val=val.split(',')
				try:
						for i in range(len(val)):
								val[i]=float(val[i])+random.uniform(-1,1)
				except: 
						# print 'try failed at',val
						pass
				else:   
						if (len(val)>1):
								val=np.asarray(val)
						else:   
								val=val[0]
						newDict[key]=val
		return newDict





def preProcessList(newDict):
		for key,val in newDict.iteritems():
				newDict[key]=val[0]
		return newDict


def getActivityParams(activity='watching'):

	raquelResponse=raquel.fetch("({handle:'"+activity+"'})-[:`ACTIVITY_PARAMS`{paramtype:'pi'}]->(b)")
	objectName=raquelResponse['1'][0]
	raquelResponse=raquel.fetch("({handle:'"+objectName+"'})")
	pi_info=preProcessList(raquelResponse)
	
	raquelResponse=raquel.fetch("({handle:'"+activity+"'})-[:`ACTIVITY_PARAMS`{paramtype:'human'}]->(b)")
	objectName=raquelResponse['1'][0]
	raquelResponse=raquel.fetch("({handle:'"+objectName+"'})")
	human_info=preProcessList(raquelResponse)


	raquelResponse=raquel.fetch("({handle:'"+activity+"'})-[:`ACTIVITY_PARAMS`{paramtype:'object'}]->(b)")
	objectName=raquelResponse['1'][0]
	raquelResponse=raquel.fetch("({handle:'"+objectName+"'})")
	obj_info=preProcessList(raquelResponse)


	params=dict()
	params['pi']=float(pi_info['pi'])+random.uniform(-1,1)
	params['human']=parseStrToJsonAddNoise(human_info)
	params['object']=parseStrToJsonAddNoise(obj_info)
	
	return params

if __name__ == '__main__':
	print getActivityParams(activity='watching')
	

	
