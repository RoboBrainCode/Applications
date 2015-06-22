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


def CheckCollison(env,roboTrans,robot,report,name):
	x_Val=roboTrans[0][3]
	y_Val=roboTrans[1][3]
	mean=[x_Val,y_Val]
	while True:
		with env:
			robot.SetTransform(roboTrans)
			env.UpdatePublishedBodies()	 
		minDist=1000000
		namedist=10000
		for body in env.GetBodies():
			kinname = body.GetName()
			first_name=kinname.split('-')[0]
			if (kinname=='PR2' or first_name=='Wall' or first_name=='Floor'):
				continue
			env.CheckCollision(body,robot,report=report)
			if (report.minDistance<minDist):
				minDist= report.minDistance
			if (kinname==name):
				namedist=report.minDistance


		if (minDist<=0.00001 or namedist> 0.5):
			pass
		else:
			break
		randVal=np.random.uniform(mean)
		roboTrans[0][3]=randVal[0]
		roboTrans[1][3]=randVal[1]

	return env

	


def getConfig():
	env_colladafile = 'environment/env_1_context_1.dae'
	env = Environment()		
	env.SetViewer('qtcoin')
	env.Load(env_colladafile)
	robot = env.GetRobots()[0]
	collisionChecker = RaveCreateCollisionChecker(env,'pqp')
	collisionChecker.SetCollisionOptions(CollisionOptions.Distance|CollisionOptions.Contacts)
	env.SetCollisionChecker(collisionChecker)
	report = CollisionReport()
	with env:
		env.UpdatePublishedBodies()	 
	# env.CheckCollision(elbow_link,kinbodies[collisionbodies[i]], report=report)
	raw_input('Press enter to start')
	robot=""
	for body in env.GetBodies():
		kinname = body.GetName()
		if (kinname=='PR2'):
			robot=body
			first_name = kinname.split('-')[0]
			oTransform=robot.GetTransform()
			break
		print kinname
		

	for body in env.GetBodies():
		kinname = body.GetName()
		first_name=kinname.split('-')[0]
		if (kinname=='PR2' or first_name=='Floor' or first_name=='Wall'):
			continue
		transform=body.GetTransform()
		roboTrans=oTransform
		roboTrans[0][3]=transform[0][3]
		roboTrans[1][3]=transform[1][3]
		roboTrans=np.asarray([[-1,0,0,0],[0,-1,0,0],[0,0,1,0],[0,0,0,1]])
		roboTrans=np.dot(transform,roboTrans)
		roboTrans=roboTrans.tolist()
		roboTrans[2][3]=0
		env=CheckCollison(env,roboTrans,robot,report,kinname)
		raw_input('Press to move to bext body')
		print kinname

		
		
	raw_input('Press enter to terminate')


if __name__ == "__main__":
	getConfig()
