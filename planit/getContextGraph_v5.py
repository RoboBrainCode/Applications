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

global home, objects, activities
home = os.path.expanduser('~')

objects = ['tv','mirror','human','sofa','chair','bed','almirah','bookshelf','lamp','table','desk']
activities = ['dancing','sitting','working','reaching','walking','watching','interacting','relaxing','watching2']

def findMatch(word):
	length = 0
	idx = 0
	for obj in objects:
		isMatch = word.find(obj)
		if not isMatch == -1: #-1 when no match found
			new_length = len(obj)
			if new_length > length:
				length = new_length
				idx = objects.index(obj)
	return objects[idx]

def LoadGraph(path_to_environment_context_graph,env):
	#print "Context file ",path_to_environment_context_graph
	activity_happening = []
	f = open(path_to_environment_context_graph,'r')
	lines = f.readlines()
	context_graph = {}
	for line in lines:
		if line[0] == '#': # is a commented
			continue
		#print line
		line = line.strip().split(',')
		word1 = findMatch(line[0])
		word2 = findMatch(line[1])
		activity = line[2]
		if activity == "relaxing": # or activity =='sitting':
			continue
		if not activity in activity_happening:
			activity_happening.append(activity)
		
		if activity == 'interacting':
			t0 = env.GetKinBody(line[0]).GetTransform()
			t1 = env.GetKinBody(line[1]).GetTransform()
			twrt = t1-t0
			distance = np.linalg.norm(twrt[:2,3])
			xaxis = twrt[:2,3]/distance
			yaxis = np.array([-xaxis[1],xaxis[0]])
			idname = '{0}:{1}:{2}'.format(line[0],line[1],activity)
			object1 = {}
			object1['name'] = word1
			object1['id'] = line[0]
			transform = env.GetKinBody(line[0]).GetTransform()
			object1['rot'] = (transform[:3,:3]).T
			object1['xyz'] = transform[:3,3]
			object1['xaxis'] = xaxis
			object1['yaxis'] = yaxis
			object2 = {}
			object2['name'] = word2
			object2['id'] = line[1]
			transform = env.GetKinBody(line[1]).GetTransform()
			object2['rot'] = (transform[:3,:3]).T
			object2['xyz'] = transform[:3,3]
			object2['xaxis'] = -xaxis
			object2['yaxis'] = -yaxis
			context_graph[idname] = {}
			context_graph[idname]['obj1'] = object1
			context_graph[idname]['obj2'] = object2
			context_graph[idname]['activity'] = activity
			context_graph[idname]['distance'] = distance
		else:
			if word1 == 'human':
				human_body = line[0]
				obj_body = line[1]
			else:
				human_body = line[1]
				obj_body = line[0]
				word2 = word1
			t0 = env.GetKinBody(human_body).GetTransform()
			#print env.GetKinBody(obj_body).GetName()
			t_ = env.GetKinBody(obj_body).GetTransform()
		        collisionChecker = RaveCreateCollisionChecker(env,'pqp')
 		        collisionChecker.SetCollisionOptions(CollisionOptions.Distance|CollisionOptions.Contacts)
		        env.SetCollisionChecker(collisionChecker)
		        report = CollisionReport()
			env.CheckCollision(env.GetKinBody(human_body),env.GetKinBody(obj_body),report=report)
			dist_between = np.array([np.linalg.norm((t0 - t_)[:2,3]),0,0])
			#dist_between = np.array([report.minDistance,0,0])
			xyz_obj = t0[:3,3] + np.dot(t0[:3,:3],dist_between)
			t1 = np.eye(4)
			t1[:3,3] = xyz_obj
			#t1 = env.GetKinBody(line[1]).GetTransform()
			twrt = t1-t0
			distance = np.linalg.norm(twrt[:2,3])
			xaxis = twrt[:2,3]/distance
			yaxis = np.array([-xaxis[1],xaxis[0]])
			idname = '{0}:{1}:{2}'.format(human_body,obj_body,activity)
			object1 = {}
			object1['name'] = 'human'
			object1['id'] = human_body
			transform = env.GetKinBody(human_body).GetTransform()
			object1['rot'] = (transform[:3,:3]).T
			object1['xyz'] = transform[:3,3]
			object1['xaxis'] = xaxis
			object1['yaxis'] = yaxis
			object2 = {}
			object2['name'] = word2
			object2['id'] = obj_body
			transform = env.GetKinBody(obj_body).GetTransform()
			object2['rot'] = (transform[:3,:3]).T
			object2['xyz'] = xyz_obj
			object2['xaxis'] = -xaxis
			object2['yaxis'] = -yaxis
			context_graph[idname] = {}
			context_graph[idname]['obj1'] = object1
			context_graph[idname]['obj2'] = object2
			context_graph[idname]['activity'] = activity
			context_graph[idname]['distance'] = distance


	
	activity_instances = {}
	activity_count = {}
	for activity in activities:
		activity_instances[activity] = {}
		activity_instances[activity]['human'] = {}
		activity_instances[activity]['object'] = {}
		activity_count[activity] = 0
	num_cluster = 0
	for nodes in context_graph:
		node = context_graph[nodes]
		activity = node['activity']
		activity_count[activity] += 1
		obj1 = node['obj1']['id']
		obj1_type = node['obj1']['name']
		obj2 = node['obj2']['id']
		obj2_type = node['obj2']['name']
		
		usename = 'human'
		if obj1_type != 'human':
			usename = 'object'
		if not activity_instances[activity][usename].has_key(obj1):
			activity_instances[activity][usename][obj1] = ['{0}::{1}::{2}'.format(activity,usename,obj2)]
		else:
			activity_instances[activity][usename][obj1].append('{0}::{1}::{2}'.format(activity,usename,obj2))
	
		usename = 'human'
		if obj2_type != 'human':
			usename = 'object'
		if not activity_instances[activity][usename].has_key(obj2):
			activity_instances[activity][usename][obj2] = ['{0}::{1}::{2}'.format(activity,usename,obj1)]
		else:
			activity_instances[activity][usename][obj2].append('{0}::{1}::{2}'.format(activity,usename,obj1))

		num_cluster = num_cluster + 1 
	#print context_graph
	return context_graph, activity_happening, activity_instances, activity_count

