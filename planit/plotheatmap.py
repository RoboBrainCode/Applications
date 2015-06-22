"""
For evaluating and visualizing the distribution learned using learning_v5.py
"""

import os
import numpy as np
import random as rand
import copy
import sys
import sqlite3 as sql
import pickle
from openravepy import *
import openravepy
import pdb
import matplotlib.pyplot as plt 
import getContextGraph_v5 as contextGraph
from matplotlib.backends.backend_pdf import PdfPages
from scipy.stats import beta as beta_distribution
import probdatapoint_v5 as pdp
import loadFromdb
import socket
import loadActivityParams as lap

"""
Input: path to environment colladafile, path to context graph, path to params pickle file
Output: Loads all relevant data structures
"""

def loadPrereq(env_colladafile,context_graph,params_file):
	env = Environment()
	env.Load(env_colladafile)
	# with open(params_file,'rb') as ff:
	# 	params = pickle.load(ff)
	
	nodes, activity_happening, activity_instances, activity_count = contextGraph.LoadGraph(context_graph,env)
	params=dict()
	for activity in activity_happening:
		params[activity]=lap.getActivityParams(activity=activity)


	activity_local_prob = {}
	total_prob=0.0
	for activity in activity_happening:
		activity_local_prob[activity] = {}
		activity_local_prob[activity]['prob'] = params[activity]['pi']
		total_prob = total_prob + activity_local_prob[activity]['prob']
	for activity in activity_happening:
		activity_local_prob[activity]['prob'] /=total_prob

	return env, params, nodes, activity_happening, activity_instances, activity_count, activity_local_prob


def move_arm(openrave_traj,env):
	traj = RaveCreateTrajectory(env,'')
	traj.deserialize(openrave_traj)
	return traj

"""
Input: path to environment colladafile, path to context graph, path to params pickle file
Output: 2D array zz, containing values of heatmap
"""
def heatmap(env_colladafile,context_graph,params_file,use_beta=True):
	env, params, nodes, activity_happening, activity_instances, activity_count, activity_local_prob = loadPrereq(env_colladafile,context_graph,params_file)
	w1 = env.GetKinBody('Wall-1')
	w3 = env.GetKinBody('Wall-3')
	numsamp = 80
	xdim = w3.GetLinks()[0].GetGeometries()[0].GetBoxExtents()[0]
	ydim = w1.GetLinks()[0].GetGeometries()[0].GetBoxExtents()[1]
	x = np.linspace(-xdim,xdim,numsamp)
	y = np.linspace(ydim,-ydim,numsamp)
	xx, yy = np.meshgrid(x,y)
	zz = np.zeros((numsamp,numsamp))
	for i in range(numsamp):
		for j in range(numsamp):
			data = np.array([xx[i,j],yy[i,j],0])
			running_sum = 0.0
			for node in nodes:
				von_pdf, t_1, t_2, t_3 = pdp.probdata(params,nodes[node],data,activity_count,activity_local_prob,use_beta)
				running_sum += von_pdf
			zz[i,j] = running_sum
	with open('temp.pik','wb') as ff:
		pickle.dump(zz,ff,-1)
	return zz

"""
Input: path to environment colladafile, path to context graph, path to params pickle file, path to trajectory pickle file
Output: Score of the trajectory
"""
def scoretraj(env_colladafile,context_graph,params_file,traj_pik_file,use_beta=True):
	env, params, nodes, activity_happening, activity_instances, activity_count, activity_local_prob = loadPrereq(env_colladafile,context_graph,params_file)
	with open(traj_pik_file,'rb') as ff:
		trajs = pickle.load(ff)
	alltraj = []
	for traj in trajs:
		alltraj.append(move_arm(traj,env))
	waypoints = loadFromdb.concatenateTrajs(alltraj)
	maxscore = -float('inf')
	for waypoint in waypoints:
		data = np.array([waypoint[0],waypoint[1],0])
		running_sum = 0.0
		for node in nodes:
			von_pdf, t_1, t_2, t_3 = pdp.probdata(params,nodes[node],data,activity_count,activity_local_prob,use_beta)
			running_sum += von_pdf
		if running_sum > maxscore:
			maxscore = running_sum	
	return maxscore

"""
Input: path to environment colladafile, path to context graph, path to params pickle file, trajectory pointer
Output: Score of the trajectory
"""
def scoresingletraj(env_colladafile,context_graph,params_file,traj,use_beta=True):
	env, params, nodes, activity_happening, activity_instances, activity_count, activity_local_prob = loadPrereq(env_colladafile,context_graph,params_file)
	alltraj = []
	alltraj.append(move_arm(traj,env))
	waypoints = loadFromdb.concatenateTrajs(alltraj)
	maxscore = -float('inf')
	for waypoint in waypoints:
		data = np.array([waypoint[0],waypoint[1],0])
		running_sum = 0.0
		for node in nodes:
			von_pdf, t_1, t_2, t_3 = pdp.probdata(params,nodes[node],data,activity_count,activity_local_prob,use_beta)
			running_sum += von_pdf
		if running_sum > maxscore:
			maxscore = running_sum	
	return maxscore

