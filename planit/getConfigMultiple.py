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
import plotheatmap as pm
import os
import numpy as np
from optparse import OptionParser
from openravepy.misc import OpenRAVEGlobalArguments
import pickle

def getConfig():
	env_colladafile = 'environment/env_1_context_1.dae'
	env = Environment()		
	env.SetViewer('qtcoin')
	env.Load(env_colladafile)
	robot = env.GetRobots()[0]
	configParams=dict()
	configParams['start_configs']=list()
	configParams['end_configs']=list()
	numWayPoints=3
	raw_input('Enter Point 1')
	configParams['start_configs'].append(robot.GetTransform())
	for i in range(1,numWayPoints-1):
		raw_input('Enter Point'+str(i+1))
		userInput=robot.GetTransform()
		configParams['end_configs'].append(userInput)
		configParams['start_configs'].append(userInput)
	raw_input('Enter Point'+str(numWayPoints))
	configParams['end_configs'].append(robot.GetTransform())
	raw_input('Save Camera Angle')
	viewer = env.GetViewer()
	configParams['CameraAngle']=viewer.GetCameraTransform()
	pickle.dump( configParams, open( "params/robotPosM.pk", "wb" ) )
	
	print len(configParams['start_configs'])
	print len(configParams['end_configs'])
if __name__ == "__main__":
	getConfig()
