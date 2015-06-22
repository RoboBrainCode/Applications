import os
import numpy as np
import random as rand
import sys
import sqlite3 as sql
import pickle
from openravepy import *
import openravepy
import pdb
import matplotlib.pyplot as plt 
import getContextGraph_v5 as contextGraph
#from sklearn.cluster import KMeans
from optparse import OptionParser
from scipy.stats import beta as beta_distribution
import probdatapoint_v5 as pdp
import socket 

global home, hostname
home = os.path.expanduser('~')
hostname = socket.gethostname()

def load_traj(filename):
	f = open(filename)
	traj = f.read()
	f.close()
	trajptr = RaveCreateTrajectory(Environment(),'')
	trajptr.deserialize(traj)
	return trajptr

def concatenateTrajs(trajs,delta_t = 0.1):
	# Input: hop_1 and hop_2
	# Output: waypoints at a delta_t = 0.1
	#delta_t = 0.1
	waypoints = []
	count = 0
	for traj in trajs:
		duration = traj.GetDuration()
		while count < duration:
			waypoints.append(traj.Sample(count))
			count = count + delta_t
		count = count - duration
	return waypoints


def returnWaypoints(traj1,traj2):
	# Input: hop_1 and hop_2
	# Output: waypoints at a delta_t = 0.1
	delta_t = 0.1
	waypoints = []
	duration = traj1.GetDuration()
	count = 0
	while count < duration:
		waypoints.append(traj1.Sample(count))
		count = count + delta_t
	count = count - duration
	duration = traj2.GetDuration()
	while count < duration:
		waypoints.append(traj2.Sample(count))
		count = count + delta_t
	return waypoints

def loadData(taskname,env_num,context_num, trajnum):
	global home
	#localdbname = '{0}/project/rubyonrails/pva/public/videos/{1}/{1}_new.sqlite3'.format(home,taskname)
	localdbname = '{0}/project/robotics/data/tasks_vaibhav/{1}/{1}_new.sqlite3'.format(home,taskname)
	localdb = sql.connect(localdbname)
	ptrlocal = localdb.cursor()
	dbname = 'environment_{0}'.format(env_num)
	try:
		rows = ptrlocal.execute('select * from {0} where context = {1} and traj={2}'.format(dbname,int(context_num),int(trajnum)))
		traj = rows.fetchall()
	except:
		traj = []	
	return traj

def weighPoints(taskname,env_num,context_num):
	global home, hostname
	if hostname == 'brane01':
		path = '/localdisk/ashesh_trajectories/planning_via_affordance/{0}/random_start_one_hop_scene{1}'.format(taskname,env_num)
	else:
		path = '{0}/project/robotics/data/tasks_vaibhav/{1}/random_start_one_hop_scene{2}'.format(home,taskname,env_num)
	#path = '{0}/project/robotics/data/tasks_vaibhav/{1}/random_start_one_hop_scene{2}'.format(home,taskname,env_num)
	traj_data = {}
	for trajid in range(1000):
		path_to_traj = '{0}/{1}/hop_1_context_{2}.traj'.format(path,trajid,context_num)	
		if not os.path.exists(path_to_traj):
			continue
		traj1 = load_traj(path_to_traj)
		path_to_traj = '{0}/{1}/hop_2_context_{2}.traj'.format(path,trajid,context_num)	
		traj2 = load_traj(path_to_traj)
		waypoints = returnWaypoints(traj1,traj2)
		traj_data[trajid] = waypoints

	max_dist = -float('inf')
	min_dist = float('inf')

	traj_data_new = {}
	for trajid in traj_data:
		waypoints_new = []
		waypoints = traj_data[trajid]
		for waypoint in waypoints:
			dist = 0.0
			for ids in traj_data:
				waypoints_ = traj_data[ids]
				for point in waypoints_:
					dist += np.linalg.norm(waypoint[:2]-point[:2])
			if dist > max_dist:
				max_dist = dist
			if dist < min_dist:
				min_dist = dist
				
			waypoint = list(waypoint)
			waypoint.extend([dist,dist])
			waypoint = np.array(waypoint)
			waypoints_new.append(waypoint)
		waypoints_new = np.array(waypoints_new)
		traj_data_new[trajid] = waypoints_new
	return traj_data_new, max_dist, min_dist

def getTimeAndWaypoints(taskname,env_num,context_num,completeRewrite=False):	
	global home
	path_to_environment_context_collada = '{0}/project/robotics/data/tasks_vaibhav/{1}/environment/env_{2}_context_{3}.dae'.format(home,taskname,env_num,context_num)
	if os.path.exists(path_to_environment_context_collada):
		pickle_file = '{0}/project/robotics/data/tasks_vaibhav/{1}/env_{2}_context_{3}_new.pik'.format(home,taskname,env_num,context_num)
		env = Environment()
		env.Load(path_to_environment_context_collada)


		#all_traj_data, max_dist, min_dist = weighPoints(taskname,env_num,context_num)
		if hostname == 'brane01':
			path = '/localdisk/ashesh_trajectories/planning_via_affordance/{0}/random_start_one_hop_scene{1}'.format(taskname,env_num)
		else:
			path = '{0}/project/robotics/data/tasks_vaibhav/{1}/random_start_one_hop_scene{2}'.format(home,taskname,env_num)
		#path = '{0}/project/robotics/data/tasks_vaibhav/{1}/random_start_one_hop_scene{2}'.format(home,taskname,env_num)

		if os.path.exists(pickle_file) and not completeRewrite:
			with open(pickle_file,'rb') as ff:
				data_dict = pickle.load(ff)	
				ff.close()
		else:
			data_dict = {}

		max_val = -float('inf')
		min_val = float('inf')
		for trajid in range(1000):
			path_to_traj = '{0}/{1}/hop_1_context_{2}.traj'.format(path,trajid,context_num)	
			if not os.path.exists(path_to_traj):
				continue
			if (not completeRewrite) and data_dict.has_key(trajid):
				#print "Already in pickle file"
				continue
		
			userdata = loadData(taskname,env_num,context_num,trajid)
			if len(userdata) == 0:
				continue
			#else:
			#	print "NOT in pickle file"

			traj1 = load_traj(path_to_traj)
			path_to_traj = '{0}/{1}/hop_2_context_{2}.traj'.format(path,trajid,context_num)	
			traj2 = load_traj(path_to_traj)
			waypoints = returnWaypoints(traj1,traj2)
		
			numsam = 100
			robot = env.GetRobots()[0]
			trans = robot.GetTransform()
			waypoints_new = []
			for points in waypoints:
				mean = [points[0],points[1]]
				cov = [[0.5,0.0],[0.0,0.5]]
				xx,yy = np.random.multivariate_normal(mean,cov,numsam-1).T
				numcollision = 0.0
				for i in range(numsam-1):
					trans[0,3] = xx[i]
					trans[1,3] = yy[i]
					robot.SetTransform(trans)
					if env.CheckCollision(robot):
						#print "in collision"
						numcollision += 1.0
				points = list(points)
				points.extend([1.0 - numcollision/numsam,1.0 - numcollision/numsam])
				ratio = numsam/(numsam-numcollision)
				if ratio > max_val:
					max_val = ratio
				if ratio < min_val:
					min_val = ratio
				#print ratio
				waypoints_new.append(points)
			
			waypoints = np.array(waypoints_new)
			"""
			waypoints = all_traj_data[trajid]
			waypoints_new = []
			for points in waypoints:
				points[-1] = (1.0/min_dist)*points[-1]
				points[-2] = (1.0/max_dist)*points[-2]
				waypoints_new.append(points)
			waypoints = np.array(waypoints_new)
			"""
			#waypoints = (1.0/min_dist)*waypoints
				
			traj_data = []
			for data in userdata:
				start_time = data[1]
				end_time = data[2]
				score = data[3]
				userid = data[4]
				if start_time > end_time:
					temp = end_time
					end_time = start_time
					start_time = end_time
				startid = int(start_time*10.0)
				endid = int(end_time*10.0)
				start_time = startid/10.0
				end_time = endid/10.0
				
				traj_data.append([waypoints[startid:(endid+1)],start_time,end_time,score,userid,waypoints])

			if len(traj_data) > 0:
				data_dict[trajid]=traj_data
		if len(data_dict) > 0:
			#print "min_val ",min_val
			#print "max_val ",max_val
			with open('{0}/project/robotics/data/tasks_vaibhav/{1}/env_{2}_context_{3}_new.pik'.format(home,taskname,env_num,context_num),'wb') as ff:
				pickle.dump(data_dict,ff,-1)

if __name__ == "__main__": 
	global envstr, contextstr
	usage = "usage: %prog [options] arg"
	parser = OptionParser(usage)
	parser.add_option('-t', '--task', dest='taskname', help="Taskname")
	parser.add_option('-e', '--env', dest='env_num', help="environments to train on [comma separated]")
	parser.add_option('-c', '--context', dest='context_num', help="contexts to train on [comma separated]")
	parser.add_option('-r', '--completeRewrite', action='store_true', dest='completeRewrite', help="to complete re-write the pickle files")
	(options, args) = parser.parse_args()
	if options.taskname is None:	
		parser.error("taskname not entered -t")
	taskname = options.taskname #sys.argv[1]
	if options.env_num is None:
		parser.error("environment not entered -e")
	envstr = options.env_num #sys.argv[2]
	if envstr == 'all':
		env_num = range(15)
	else:
		env_num = [int(x) for x in envstr.split(',')]
	
	if options.context_num is None:
		cstr = ''
		context_num = range(10)
		for cc in context_num:
			cstr = cstr+str(cc)+','
		contextstr = cstr[:-1]
	else:
		contextstr = options.context_num
		context_num = [int(x) for x in contextstr.split(',')]
	
	if options.completeRewrite:
		completeRewrite = True
		#print "Re-writing everything"
	else:
		completeRewrite = False
		#print "Not re-writing"

	for e in env_num:
		for c in context_num:
			if taskname == 'bedroom' and e == 3 and c == 1:
				continue
			getTimeAndWaypoints(taskname,str(e),str(c),completeRewrite)
	print "Done with writing pickle file"
