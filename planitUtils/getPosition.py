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
from dbFns.main import insertObjectPosition

def getPosition(env_colladafile):
	print env_colladafile
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
			print kinname
			raw_input('Press to Select Position')
			transform=robot.GetTransform()
			position[kinname]=transform
			data={'envName':paramsFilePrepend,'objectName':kinname,'objectPosition':robot.GetTransform()}
			insertObjectPosition(data)
		pickle.dump(position, open(positionFile,"wb" ) )

if __name__ == "__main__":
	import sys
	env_colladafile = '../environment/env_{0}_context_1.dae'.format(sys.argv[1])
	print env_colladafile
	getPosition(env_colladafile)
	