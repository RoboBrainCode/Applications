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
import matplotlib.image as mpimg
import matplotlib.pyplot as plt
import copy
import Image
from getPosition import setPosition

global RecordVal
RecordVal=False


global PlayVal
PlayVal=False



def waitrobot(robot):
	"""busy wait for robot completion"""
	while not robot.GetController().IsDone():
		time.sleep(0.01)

def MultipleWayPoints(env_colladafile,context_graph,configParams,trajectoryName=os.path.dirname(os.path.realpath(__file__))+'/trajectory/t1M.pk',display=True):
	'''
	This filler method is only for testing purpose. You may ignore it. 
	'''

	for i in range(len(configParams['start_configs'])):
		if not configParams['start_configs'][i]=='PR2':
			configParams['start_configs'][i]= configParams['start_configs'][i].lower()
		if not configParams['end_configs'][i]=='PR2':
			configParams['end_configs'][i]= configParams['end_configs'][i].lower()
	
	configParams=setPosition(configParams,env_colladafile)

	params_file = os.path.dirname(os.path.realpath(__file__))+'/params/params_task_bedroom_env_1,2,3,4,5,6,7,8,9_context_1,2,3,4_s_False_r_False_n_False_b_True.pik'
	imgName=os.path.dirname(os.path.realpath(__file__))+'heatmap/env_1_context_1_beta_True_noborder_small.png'
	
	
	use_beta = True
	SetCameraAngle=False
	OverLayHeatMap=False
	PlanPath=True
	runTrajectory=True
	numSampleTraj=2


	finalTraj=list()
	env = Environment()	
	# env.SetViewer('qtcoin')
	env.Load(env_colladafile)
	# configParams = pickle.load( open(configFile,"rb" ) )
	robot = env.GetRobots()[0]
	start_configs=configParams['start_configs']
	end_configs=configParams['end_configs']
	if SetCameraAngle:
		viewer = env.GetViewer()
		viewer.SetCamera(configParams['CameraAngle'])
	# raw_input('Start Learning')
	if PlanPath:
		for i in range(len(start_configs)):
			print 'Point',i,(i+1)
			start_config=start_configs[i]
			end_config=end_configs[i]
			list_traj,env = planmanytraj(env,start_config,end_config,numPoints=numSampleTraj)
			if len(list_traj)>0:
				sorted_list_traj,sorted_scores=FindBestTraj(env_colladafile,context_graph,params_file,list_traj)
				finalTraj.append(sorted_list_traj[0])
			# finalTraj.append(list_traj[0])
		pickle.dump( finalTraj, open( trajectoryName, "wb" ) )
	if OverLayHeatMap:
		plotOverlayHeatMap(env_colladafile,context_graph,params_file,imgName,use_beta)
		env=loadheatmap(env,e,use_beta,imgName)
	# if runTrajectory and display:
	# 	raw_input('runTrajectory')
	# 	with open(trajectoryName,'rb') as ff:
	# 		trajs = pickle.load(ff)	
	# 	gotoStartLocation(trajs[0],robot,env)
	# 	time.sleep(1)
	# 	Playtraj(trajectoryName,env)
	# raw_input('Terminate')
	env.Destroy()


def playTrajFromFile(env_colladafile,trajectoryName,camera_angle_path):
	env = Environment()	
	env.SetViewer('qtcoin')
	viewer=env.GetViewer()
	env.Load(env_colladafile)
	if camera_angle_path:
		with open(camera_angle_path,'rb') as ff:
			camera_angle = pickle.load(ff)
			viewer.SetCamera(camera_angle)
	robot=env.GetRobots()[0]
	print 'playTrajFromFile'
	with open(trajectoryName,'rb') as ff:
		trajs = pickle.load(ff)	
	global RecordVal
	global PlayVal
	gotoStartLocation(trajs[0],robot,env)
	RecordVal=True
	time.sleep(1)
	while not PlayVal:
		pass
	time.sleep(2)
	Playtraj(trajectoryName,env)
	# raw_input('Press any key to terminate')
	env.Destroy()



'''
Mostly this will be the entering function.
'''


def FindBestTraj(env_colladafile,context_graph,params_file,list_traj):
	'''
	Input: Environment colladafile, context graph file and learned params pickle file
	Output: Executes top k trajectories
	Description: It samples many trajectories using planmanytraj() function and ranks
	them using the learned params.
	'''
	scores = []
	list_traj = np.array(list_traj)
	for traj in list_traj:
		score = pm.scoresingletraj(env_colladafile,context_graph,params_file,traj,use_beta=True)
		scores.append(score)
	scores = np.array(scores)
	sorted_args = np.argsort(scores)
	sorted_list_traj = list_traj[sorted_args]
	sorted_scores = scores[sorted_args]
	return sorted_list_traj,sorted_scores
	

def executeTopKtraj(env,env_colladafile,list_traj,k=3):
	'''
	Input: Environment colladafile and a list of trajectories
	Output: It executes top k trajectories from the list
	Description: It first loads the environment from colladafile and then execute the trajectories.
	'''
	k = min([k,len(list_traj)])
	executeAllTraj(env,list_traj[:k])

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

	for i in range(numPoints):
		traj = plantraj(env,start_config,end_config)
		if traj is not None:
			list_traj.append(traj)
			env = addObstacle(env,traj,fact)
		else:
			env = deleteAllObstacles(env)
			break
		
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
		
def executeAllTraj(env,list_traj):
	'''
	Input: An environment pointer and a list of trajectories
	Description: This functions plays the trajectories in the list
	'''
	with env:
		env.UpdatePublishedBodies()	    	
	robot = env.GetRobots()[0]
	for openrave_traj in list_traj:
		traj = RaveCreateTrajectory(env,'')
		traj.deserialize(openrave_traj)
		robot.GetController().SetPath(traj)
		waitrobot(robot) 

def plantraj(env,start_config,end_config):
	'''
	Input: An environment pointer, robot start and end confguration matrices.
	Output: A trajectory in the environment.
	'''
	print "next traj"
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


def loadheatmap(env,env_num,use_beta,imgName):
	w1 = env.GetKinBody('Wall-1')
	w3 = env.GetKinBody('Wall-3')
	xdim = w3.GetLinks()[0].GetGeometries()[0].GetBoxExtents()[0]
	ydim = w1.GetLinks()[0].GetGeometries()[0].GetBoxExtents()[1]
	img=mpimg.imread(imgName)
	M = len(img)
	N = len(img[0])
	X = 2.0*xdim
	Y = 2.0*ydim
	xlen = X*0.5/N
	ylen = Y*0.5/M
	kinbodies = []
	for i in range(M):
		for j in range(N):
			with env:
				xcenter = (j+1)*2.0*xlen - xlen - (0.5*X)
				ycenter = (0.5*Y) - ( (i+1)*2.0*ylen - ylen)
				kinbodies.append(RaveCreateKinBody(env,''))
				kinbodies[-1].InitFromBoxes(numpy.array([[xcenter,ycenter,0,xlen,ylen,0.01]]),True)
				kiname = str(i)+"-"+str(j)
				kinbodies[-1].SetName(kiname)
				env.AddKinBody(kinbodies[-1],True)
				kinbodies[-1].GetLinks()[0].GetGeometries()[0].SetDiffuseColor(list(img[i,j])[:-1])
	return env

def colorhumans(env,bodies):
	colors = [(1.0,0.0,0.0), (0.0,1.0,0.0), (1.0,0.5,0.1), (1.0,1.0,0.0), (1.0,10.2,0.8), (0.0,1.0,1.0), (0.5,0.2,0.5)]
	count = 0
	robot = env.GetRobots()[0]
	env.RemoveKinBody(robot)
	for body in bodies:	
		if body.GetName().startswith('human'):
			body.GetLinks()[0].GetGeometries()[0].SetDiffuseColor(colors[count])
			count += 1

def plotOverlayHeatMap(env_path,context_graph_path,params_file_path,imageFilePath,use_beta):
	if not os.path.exists(imageFilePath): 
		heatmap_array = pm.heatmap(env_path,context_graph_path,params_file_path,use_beta)
		fig = plt.figure(e)
		imo = plt.imshow(heatmap_array)
		imo.write_png('heatmap/temp.png')
		img = Image.open('heatmap/temp.png')
		rsize = img.resize((img.size[0]/5,img.size[1]/5))
		rsize.save(imageFilePath)

def waitrobot(robot):
	"""busy wait for robot completion"""
	while not robot.GetController().IsDone():
		time.sleep(0.01)

def move_arm(openrave_traj,env,robot):
	traj = RaveCreateTrajectory(env,'')
	traj.deserialize(openrave_traj)
	robot.GetController().SetPath(traj)
	waitrobot(robot)
	return

def gotoStartLocation(openrave_traj,robot,env):
	traj = RaveCreateTrajectory(env,'')
	traj.deserialize(openrave_traj)
	traj_temp = RaveCreateTrajectory(env,'')
	traj_temp.Init(traj.GetConfigurationSpecification())
	traj_temp.Insert(0,traj.GetWaypoint(0),traj.GetConfigurationSpecification(),True)
	robot.GetController().SetPath(traj_temp)
	waitrobot(robot) 
	return

def Playtraj(traj_path,env):
	robot = env.GetRobots()[0]
	with open(traj_path,'rb') as ff:
		trajs = pickle.load(ff)	
	for traj in trajs:
		move_arm(traj,env,robot)

@openravepy.with_destroy
def run(args=None):
	global env
	parser = OptionParser(description='Explicitly specify goals to get a simple navigation and manipulation demo.', usage='openrave.py --example hanoi [options]')
	OpenRAVEGlobalArguments.addOptions(parser)
	parser.add_option('--planner',action="store",type='string',dest='planner',default=None,help='the planner to use')
	(options, leftargs) = parser.parse_args(args=args)
	env = OpenRAVEGlobalArguments.parseAndCreate(options,defaultviewer=True)
	main(env,options)
	time.sleep(4)



from threading import Thread
class myClass():
    def help(self):
    	print 'Enterd fn1'
        os.system(os.path.dirname(os.path.realpath(__file__))+'/./record.sh '+videoLocationF)

    def nope(self):
		print 'Enterd fn2'
		global trajectoryNameF
		global envF
		time.sleep(2)
		Playtraj(trajectoryNameF,envF)
		envF.Destroy()
		os.system(os.path.dirname(os.path.realpath(__file__))+'/./kill.sh')
		

		





		# playTrajFromFile(env_colladafile,trajectoryName,camera_angle_path)
		


# if __name__ == "__main__":
global envF
global trajectoryNameF
global videoLocationF

def playTrajFromFileandSave(env_colladafile,trajectoryName,videoLocation,camera_angle_path):
	Yep = myClass()
	
	global trajectoryNameF
	global videoLocationF
	global envF

	trajectoryNameF=trajectoryName
	videoLocationF=videoLocation

	envF = Environment()	
	envF.SetViewer('qtcoin')
	viewer=envF.GetViewer()
	envF.Load(env_colladafile)
	if camera_angle_path:
		with open(camera_angle_path,'rb') as ff:
			camera_angle = pickle.load(ff)
			viewer.SetCamera(camera_angle)
	robot=envF.GetRobots()[0]
	print 'playTrajFromFile'
	with open(trajectoryName,'rb') as ff:
		trajs = pickle.load(ff)	

	gotoStartLocation(trajs[0],robot,envF)
	os.system('rm '+videoLocationF)
	time.sleep(1)

	






	thread = Thread(target = Yep.help)
	thread2 = Thread(target = Yep.nope)
	thread.start()
	thread2.start()
	thread.join()


		



	print 'Finished'






if __name__ == "__main__":
	env_colladafile = 'environment/env_1_context_1.dae'
	context_graph = 'environment/1_graph_1.xml'
	trajectorySaveLocation='environment/t1.pk'
	configParams=dict()
	configParams['start_configs']=list()
	configParams['end_configs']=list()
	configParams['start_configs'].append('PR2')
	configParams['end_configs'].append('blackcouch_1')
	MultipleWayPoints(env_colladafile,context_graph,configParams,trajectorySaveLocation)
	raw_input('Next')
	playTrajFromFile(env_colladafile,trajectorySaveLocation)
