from __future__ import with_statement # for python 2.5
from rest_framework import status
from rest_framework.decorators import api_view
from rest_framework.response import Response
from rest_framework.parsers import MultiPartParser, FormParser
from django.http import HttpResponse
import json,yaml,ast
import unicodedata
import sys
import os
import time
import openravepy
import numpy.random as rand
if not __openravepy_build_doc__:
	from openravepy import *
	from numpy import *
import xml.dom.minidom
import sys
from optparse import OptionParser
from openravepy.misc import OpenRAVEGlobalArguments
import pickle
from bs4 import BeautifulSoup
from threading import Thread,Lock
from functools import partial
import requests
from dbFns.main import getBestTrajectory
from dbFns.main import insertPlanitFeedback
from coactiveUpdate.planitUpdate import updatePlanit

app=None
trajectorySaveLocation=None
robot=None
configParams=None
openraveLock=Lock()
stopValLock=Lock()
stopVal=0
envG=None
envName='env_1'
globalEnvPath="/".join(os.path.dirname(os.path.realpath(__file__)).split('/')[:-2])+'/'
planitFeedback1=None
startConfigPlan=None
endConfigPlan=None


def initOpenrave():
	global globalEnvPath,envG,robot,trajectorySaveLocation,envName
	env_colladafile = globalEnvPath+'planitDave/{0}.dae'.format(envName)
	print env_colladafile
	envG=Environment()
	envG.Load(env_colladafile)
	envG.SetViewer('qtcoin')
	robot = envG.GetRobots()[0]
	# trajectorySaveLocation=globalEnvPath+'environment/traj_env_{0}_context_1_id_1.pik'.format(envNumber)

@api_view(['GET'])

def initApp(request):
	if request.method == 'GET':
		global envG
		if envG:
			print 'Openrave Window Already exist'
		else:
			initOpenrave()
		return HttpResponse(json.dumps({'result':{'success':1}}), content_type="application/json")

def playTraj(request):
	if request.method == 'GET':
		data=dict(request.GET)
		json_data=json.dumps(data)
		data=yaml.safe_load(json_data)
		for key,val in data.iteritems():
			data[key]=val[0]
		
		global stopVal,envName,globalEnvPath,trajectorySaveLocation,robot
		with stopValLock:
			stopVal=-1
		print data
		if (envName!=data['env']):
			envName=data['env']
			envG.Reset()
			env_colladafile = globalEnvPath+'planitDave/{0}.dae'.format(envName)
			envG.Load(env_colladafile)
			robot = envG.GetRobots()[0]
			time.sleep(1)
		envName=data['env']
		if (data['startPos']!='PR2'):
			data['startPos']=data['startPos'].lower()
		if (data['endPos']!='PR2'):
			data['endPos']=data['endPos'].lower()
		data={'envName':envName,'objectFrom':data['startPos'],'objectTo':data['endPos']}
		global startConfigPlan
		global endConfigPlan
		startConfigPlan=data['objectFrom']
		endConfigPlan=data['objectTo']
		print data
		trajectorySaveLocation=getBestTrajectory(data)['bestTrajectory']
# print insertBestTrajectory(data)
			
		
		i=0
		nThread = Thread(target=playGivenTraj, args=(i,))
		nThread.start()
		return HttpResponse(json.dumps({'result':'trajectory played'}), content_type="application/json")

def resumeTraj(request):
	if request.method == 'GET':
		global stopVal
		with stopValLock:
			stopVal=0
		return HttpResponse(json.dumps({'result':'trajectory resumed'}), content_type="application/json")

def stopTraj(request):
	if request.method == 'GET':
		global stopVal
		with stopValLock:
			stopVal=1
		return HttpResponse(json.dumps({'result':'trajectory stopped'}), content_type="application/json")

		
def capTraj(request):
	if request.method == 'GET':
		global planitFeedback1
		planitFeedback1=robot.GetTransform()
		return HttpResponse(json.dumps({'result':'captured first way point'}), content_type="application/json")
		
def capNextTraj(request):
	if request.method == 'GET':
		planitFeedback2=robot.GetTransform()
		data=dict(request.GET)
		json_data=json.dumps(data)
		data=yaml.safe_load(json_data)
		global startConfigPlan
		global endConfigPlan
		updatedData={'feedId':data['feedId'][0],'init':planitFeedback1,'final':planitFeedback2,'trajInfo':startConfigPlan+','+endConfigPlan}
		print updatedData
		feedbackData=insertPlanitFeedback(updatedData)
		feedbackData['envName']=envName
		updatePlanit(feedbackData)
		return HttpResponse(json.dumps({'result':'captured second way point'}), content_type="application/json")
		

def saveSeq(request):
	if request.method == 'GET':
		data=ast.literal_eval(dict(request.GET)['query'][0])
		json_data=json.dumps(data)
		data=yaml.safe_load(json_data)
		print data
		return HttpResponse(json.dumps({'result':{'success':1}}), content_type="application/json")

def waitrobot(robot):
	"""busy wait for robot completion"""
	while not robot.GetController().IsDone():
		time.sleep(0.01)

def move_arm(openrave_traj,env,robot):
	trajXML=BeautifulSoup(openrave_traj)
	content=trajXML.data.string.split(' ')
	trajXML.data['count']=2
	global stopVal
	traj = RaveCreateTrajectory(env,'')
	for i in range(0,len(content)-9,8):
		while True:
			if stopVal==0:
				break
			elif stopVal==1:
				continue
			elif stopVal ==-1:
				return
		trajXML.data.string=" ".join(content[i:i+16])+" "
		print 'waypoint',i/8+1,'of',len(content)/8
		trajToParse=str(trajXML.body.contents[0])
		traj.deserialize(trajToParse)
		robot.GetController().SetPath(traj)
		waitrobot(robot)
	return

def Playtraj(index):
	global envG
	global trajectorySaveLocation
	global robot
	# with open(trajectorySaveLocation,'rb') as ff:
	# 	trajs = pickle.load(ff)	
	move_arm(trajectorySaveLocation,envG,robot)

def playGivenTraj(index):
	with openraveLock:
		global stopVal
		stopVal=0
		Playtraj(index)
		print 'Hello World'
