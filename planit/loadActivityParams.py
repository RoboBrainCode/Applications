import numpy as np
import requests
import urllib
import yaml
import RaquelAPI.raquel as raquel
import numpy as np
import random
random.seed(100)

def parse(input1):
    for key,val in input1.iteritems():
        x_str=""
        # print type(val)
        if type(val)==np.ndarray:
            x_arrstr = np.char.mod('%f', val)
            x_str = ",".join(x_arrstr)
        elif type(val)==list:
            for i in range(len(val)):
                val[i]=str(val[i])
            x_str=",".join(val)
        else:
            x_str=str(val)
        input1[key]=x_str
    return input1

def parseStrToJson(varDict,noise):
		newDict=dict()
		nonParamDict=dict()
		for key,val in varDict.iteritems():
				val=val.split(',')
				try:
						for i in range(len(val)):
								val[i]=float(val[i])
								if noise:
									print 'Noise Added'
									val[i]=val[i]+random.uniform(-1,1)
				except: 
						nonParamDict[key]=val[0]
						pass
				else:   
						if (len(val)>1):
								val=np.asarray(val)
						else:   
								val=val[0]
						newDict[key]=val

				newDict.update(nonParamDict)

		return newDict




def preProcessList(newDict):
		for key,val in newDict.iteritems():
				newDict[key]=val[0]
		return newDict


def getActivityParams(activity='watching',noise=False):

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
	params['pi']=parseStrToJson(pi_info,noise)
	params['human']=parseStrToJson(human_info,noise)
	params['object']=parseStrToJson(obj_info,noise)


	# updateActivityParams(params)

	return params

def updateActivityParams(params):
	print raquel.update(parse(params['pi']))
	print raquel.update(parse(params['human']))
	print raquel.update(parse(params['object']))
	

if __name__ == '__main__':
	params=getActivityParams(activity='watching')
	print params
	# activities=['dancing','interacting','reaching','relaxing','sitting','walking','watching2','working']
	# for activity in activities:
	# 	params=getActivityParams(activity=activity,noise=True)
	# 	updateActivityParams(params)
	


	

	
