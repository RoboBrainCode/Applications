from __future__ import with_statement # for python 2.5
__author__= 'Ashesh Jain'
import os
import time
import openravepy
import numpy.random as rand
if not __openravepy_build_doc__:
    from openravepy import *
    from numpy import *
import xml.dom.minidom
import sys
# import plotheatmap as pm
import os
import numpy as np
from optparse import OptionParser
from openravepy.misc import OpenRAVEGlobalArguments
import pickle


def getPosition(env_colladafile):
	# env_colladafile = 'environment/env_1_context_1.dae'
	print env_colladafile
	paramsFilePrepend=(env_colladafile.split('/')[-1]).split('.')[0]
	env = Environment()		
	env.SetViewer('qtcoin')
	env.Load(env_colladafile)
	robot = env.GetRobots()[0]
	configParams=dict()
	robot = env.GetRobots()[0]
	position=dict()
	positionFile="params/"+paramsFilePrepend+"_objectPosition.pk"
	print positionFile
	setPosition=True
	if setPosition:
		for body in env.GetBodies():
			kinname = body.GetName()
			first_name=kinname.split('-')[0]
			print kinname
			raw_input('Press to Select Position')
			transform=robot.GetTransform()
			position[kinname]=transform
		pickle.dump(position, open(positionFile,"wb" ) )
	else:
		position = pickle.load(open(positionFile,"rb" ))
		raw_input('Press to Select Position')
		robot.SetTransform(position['PR2'])
		raw_input('Press to Select Position')
		robot.SetTransform(position['tv_1'])
		raw_input('Press to Select Position')
		robot.SetTransform(position['bed_1'])
		raw_input('Press to Select Position')

def setPosition(arr,env_colladafile):
	
	env = Environment()		
	# env.SetViewer('qtcoin')
	env.Load(env_colladafile)
	robot = env.GetRobots()[0]
	configParams=dict()
	robot = env.GetRobots()[0]

	print env_colladafile
	paramsFilePrepend=(env_colladafile.split('/')[-1]).split('.')[0]

	positionFile=os.path.dirname(os.path.realpath(__file__))+"/params/"+paramsFilePrepend+"_objectPosition.pk"
	print positionFile
	position=pickle.load(open(positionFile,"rb" ))
	# raw_input('Begin')
	for i in range(len(arr['start_configs'])):
		objName=arr['start_configs'][i]
		arr['start_configs'][i]=position[objName]
		print objName,
		# robot.SetTransform(arr['start_configs'][i])
		# raw_input('Next')
		objName=arr['end_configs'][i]
		arr['end_configs'][i]=position[objName]
		print objName
		print 
		# robot.SetTransform(arr['end_configs'][i])
		# raw_input('Press to Select Position')
	return arr


if __name__ == "__main__":
	env_colladafile = '../environment/env_9_context_1.dae'
	getPosition(env_colladafile)
	# arr=dict()
	# arr['start_configs']=['PR2','tv_1']
	# arr['end_configs']=['tv_1','bed_1']
	# print arr
	# arr=setPosition(arr)
	# print arr
	# pickle.dump( arr, open( "params/robotPosM.pk", "wb" ) )
