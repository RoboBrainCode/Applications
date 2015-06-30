from __future__ import with_statement # for python 2.5
__author__= 'ashesh jain'
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
import matplotlib.image as mpimg
import matplotlib.pyplot as plt
import copy
import Image
from getPosition import setPosition
import itertools

def main(objList,env_colladafile,numSampleTraj=2):
	# list(itertools.combinations('abcd',2))
	env = Environment()	
	env.Load(env_colladafile)
	robot = env.GetRobots()[0]
	paramsFilePrepend=(env_colladafile.split('/')[-1]).split('.')[0]
	positionFile=os.path.dirname(os.path.realpath(__file__))+"/params/"+paramsFilePrepend+"_objectPosition.pk"
	position=pickle.load(open(positionFile,"rb" ))
	permutations=len(objList)*(len(objList)-1)/2
	counter=1
	for r in itertools.product(objList, objList):
		if not r[0]==r[1]:
			print 'Executing Example:',counter,'/',permutations
			start_config=position[r[0]]
			end_config=position[r[1]]
			list_traj,env = planmanytraj(env,start_config,end_config,numPoints=numSampleTraj)
			filename=(((env_colladafile.split('/'))[-1]).split('.'))[0]+'_'+r[0]+'_'+r[1]+'.tk'
			print filename
			pickle.dump( list_traj, open( 'database/'+filename, "wb" ) )
			counter=1
	env.Destroy()

def playDbTraj(objList,env_colladafile):
	env = Environment()	
	env.SetViewer('qtcoin')
	env.Load(env_colladafile)
	robot = env.GetRobots()[0]
	permutations=len(objList)*(len(objList)-1)/2
	counter=1
	for r in itertools.product(objList, objList):
		if not r[0]==r[1]:
			print 'Executing Example:',counter,'/',permutations
			print 'moving from ',r[0],'to',r[1]
			filename=(((env_colladafile.split('/'))[-1]).split('.'))[0]+'_'+r[0]+'_'+r[1]+'.tk'
			Playtraj('database/'+filename,env)
			counter=counter+1
	env.Destroy()


def Playtraj(traj_path,env):
	robot = env.GetRobots()[0]
	with open(traj_path,'rb') as ff:
		trajs = pickle.load(ff)	
	for traj in trajs:
		move_arm(traj,env,robot)

def move_arm(openrave_traj,env,robot):
	traj = RaveCreateTrajectory(env,'')
	traj.deserialize(openrave_traj)
	robot.GetController().SetPath(traj)
	waitrobot(robot)
	return		

def planmanytraj(env,start_config,end_config,numPoints=5):
	'''
	Input: An environment pointer and path to the environment dae file to be loaded
	Output: Many diverse trajectories
	Description: It ask user for robot start and end configuration and then plans 
	many differentverse trajectories between the two configurations. To produce diverse 
	trajectories it plans a trajectory and then updates the environment by blocking 
	the trajectory by introducing an artificial obstacle. This forces next trajectory
	to be different.
	'''
	global obstacle_count
	obstacle_count = 0
	fact=0.5 
	list_traj = []
	with env:
		env.UpdatePublishedBodies()	 
	numUpdates=max(1,numPoints/5)
	for i in range(numPoints):
		if (i+1)%numUpdates==0:
			print 'iteration:',(i+1)
		traj = plantraj(env,start_config,end_config)
		if traj is not None:
			list_traj.append(traj)
			env = addObstacle(env,traj,fact)
		else:
			break

	print 'iteration:',numPoints
	env = deleteAllObstacles(env)
	return list_traj,env

def deleteAllObstacles(env):
	'''
	Description: This function deletes all the obstacles
	'''
	for body in env.GetBodies():
		kinname = body.GetName()
		first_name = kinname.split('-')[0]
		if first_name == 'obstacle':
			with env:
				env.Remove(body)
	with env:
		env.UpdatePublishedBodies()	    	
	return env
		
def plantraj(env,start_config,end_config):
	'''
	Input: An environment pointer, robot start and end confguration matrices.
	Output: A trajectory in the environment.
	'''
	# print "next traj"
	robot = env.GetRobots()[0]
	#ikmodel = databases.inversekinematics.InverseKinematicsModel(robot,iktype=IkParameterization.Type.Transform6D)
	#if not ikmodel.load():
	#	ikmodel.autogenerate()
	plannertype = "BiRRT"
	basemanip = interfaces.BaseManipulation(robot,plannername=plannertype)
        
	Tgoal_ = end_config
        angx = math.acos(Tgoal_[0][0])
        angy = math.acos(Tgoal_[1][0])
	if angy < 0.5*math.pi:
		ang = angx
        else:
	        ang = 2*math.pi - angx
	with env:
		robot.SetTransform(start_config)
	try:
		with env:
			robot.SetActiveDOFs([],DOFAffine.X|DOFAffine.Y|DOFAffine.RotationAxis,[0,0,1])
			traj = basemanip.MoveActiveJoints(goal=[Tgoal_[0][3],Tgoal_[1][3],ang],maxiter=10000,steplength=0.15,maxtries=2,outputtraj=True,execute=False)
		waitrobot(robot)
		planned_successfully = True
	except planning_error,e:
		print e
		traj = None
		planned_successfully = False
	return traj

def waitrobot(robot):
	"""busy wait for robot completion"""
	while not robot.GetController().IsDone():
		time.sleep(0.01)

def addObstacle(env,traj,fact):
	'''
	Input: environment pointer, a trajectory and a factor
	Output: environment
	Description: This function puts a tall obstacle at fact*trajectory_duration
	and returns the new environment. 
	'''
	global obstacle_count
        trajptr = RaveCreateTrajectory(Environment(),'')
        trajptr.deserialize(traj)
	traj_duration = trajptr.GetDuration()
	midwaypoint = trajptr.Sample(fact*traj_duration)

	with env:
		obstacle = RaveCreateKinBody(env,'')
		obstacle.SetName('obstacle-'+str(obstacle_count))
		obstacle.InitFromBoxes(numpy.array([[midwaypoint[0], midwaypoint[1], 0.1 ,0.01,0.01,0.1]]),True)
		env.Add(obstacle,True)
		obstacle_count += 1
		env.UpdatePublishedBodies()	    	
	return env

if __name__ == '__main__':
	objList=['bed_1','tv_1','blackcouch_1']
	# main(objList,'../environment/env_1_context_1.dae',numSampleTraj=2)
	playDbTraj(objList,'../environment/env_1_context_1.dae')