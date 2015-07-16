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
import os
import numpy as np
from optparse import OptionParser
from openravepy.misc import OpenRAVEGlobalArguments
import pickle
import json
from dbFns.main import insertObjectPosition,insertObjects

def getPosition(env_colladafile):
	paramsFilePrepend=(env_colladafile.split('/')[-1]).split('.')[0]
	print paramsFilePrepend
	env = Environment()		
	env.SetViewer('qtcoin')
	env.Load(env_colladafile)
	robot = env.GetRobots()[0]
	configParams=dict()
	robot = env.GetRobots()[0]
	position=dict()
	positionFile="params/"+paramsFilePrepend+"_objectPosition.pk"
	print positionFile
	objList=list()
	setPosition=True
	if setPosition:
		for body in env.GetBodies():
			kinname = body.GetName()
			if ('-' in kinname):
				firstname=kinname.split('-')[0]
				lastname=kinname.split('-')[1]
				if ('.' in kinname):
					kinname=firstname+'_'+str(int(float(lastname))+1)
				else:
					kinname=firstname+'_'+str(int(lastname))
			if 'Floor' in kinname:
				continue
			elif ('Wall' in kinname):
				continue
			print kinname
			objList.append(kinname)
			transform=body.GetTransform()
			robotTransform=robot.GetTransform()
			robotTransform[0][3]=transform[0][3]
			robotTransform[1][3]=transform[1][3]
			robot.SetTransform(robotTransform)
			raw_input('Press to Select Position')
			transform=robot.GetTransform()
			position[kinname]=transform
			data={'envName':paramsFilePrepend,'objectName':kinname,'objectPosition':robot.GetTransform()}
			insertObjectPosition(data)
		data={'envName':'env_1','objectsList':objList}
		print insertObjects(data)
		# print objList
		raw_input('Terminate')
		# pickle.dump(position, open(positionFile,"wb" ) )

if __name__ == "__main__":
	import sys
	env_colladafile = '../planitDave/env_{0}.dae'.format(sys.argv[1])
	print env_colladafile
	getPosition(env_colladafile)
	