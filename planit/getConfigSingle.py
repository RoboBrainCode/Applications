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
	raw_input('Enter start config')
	configParams['start_config'] = robot.GetTransform()
	raw_input('Enter end config')
	configParams['end_config'] = robot.GetTransform()
	raw_input('Save Camera Angle')
	viewer = env.GetViewer()
	configParams['CameraAngle']=viewer.GetCameraTransform()
	pickle.dump( configParams, open( "params/robotPos.pk", "wb" ) )
	
	print configParams
if __name__ == "__main__":
	getConfig()
