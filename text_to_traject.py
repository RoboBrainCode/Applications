import os,yaml,pickle
import tellmedave.languageGrounding as tellmedave
import planit.PathPlanner as PathPlanner
import RaquelAPI.raquel as raquel
import copy
import requests
import urllib
import json
from dbFns.main import inserte2eFeedback
	
def generateFeed(robotInstructionsC,videoPath,inputStr,envPath):
	envName=(envPath.split('/')[-1]).split('.')[0]
	tellmedaveOutput=list()
	planitInput=list()
	for j in range(len(robotInstructionsC['originalInstructions'])):
		tellmedaveOutput.append(robotInstructionsC['originalInstructions'][j])
	

	for i in range(len(robotInstructionsC['start_configs'])):		
		string='MoveFrom: '+robotInstructionsC['start_configs'][i]+' to '+robotInstructionsC['end_configs'][i]
		feedId=".".join((videoPath.split('/')[-1]).split('.')[:-1])
		videoPath='images/planit/'+videoPath.split('/')[-1]
	data={'envName':envName,'actualInput':inputStr[1:-1],'tellmedaveOutput':[tellmedaveOutput],'videoPath':videoPath,'feedId':feedId}
	return data

def PlanPathFromNL(inputStr,envPath,context_graph,trajectorySaveLocation,videoLocation=None,camera_angle_path=None):

	# Using roboBrain to query tellmedave parameters
	raquelResponse = raquel.fetch("({handle:'tellmedave'})-[:`HAS_PARAMETER`]->(b)")
	tellmedave.preProcess(raquelResponse['1'])
	raquelResponse=raquel.fetch("({handle:'tellmedave'})-[:`HAS_WEIGHTS`]->(b)")
	raquelResponse=raquel.fetch("({handle:'"+raquelResponse['1'][0]+"'})")
	tellmedave.getWeights(raquelResponse)
	
	# calling tellmedave

	robotInstructions=tellmedave.NLtoRobotInstructions(inputStr,envPath)
	robotInstructionsC=copy.deepcopy(robotInstructions)

	# Plan path for environment : envPath, context graph: contextGraph, 
	# robotic instructions returned by tell me dave: robotInstructions, 
	# save the learned trajectories at trajectorySaveLocation




	if videoLocation:
		for j in range(len(robotInstructionsC['originalInstructions'])):
			print os.path.dirname(os.path.realpath(__file__))+'/results.html'
			with open(os.path.dirname(os.path.realpath(__file__))+'/results.html','a') as f:
				print robotInstructionsC['originalInstructions'][j]
				f.write('<li> '+robotInstructionsC['originalInstructions'][j]+' </li> \n ')

		with open(os.path.dirname(os.path.realpath(__file__))+'/results.html','a') as f:
			f.write('<h4> PlanIt Input </h4> \n <ul>')
		for i in range(len(robotInstructionsC['start_configs'])):
			with open(os.path.dirname(os.path.realpath(__file__))+'/results.html','a') as f:
				f.write('<li> MoveFrom: '+robotInstructionsC['start_configs'][i]+' to '+robotInstructionsC['end_configs'][i]+'</li> \n')
		with open(os.path.dirname(os.path.realpath(__file__))+'/results.html','a') as f:
			f.write('</ul> \n <video width="640" height="360" preload controls> \n <source src="'+videoLocation+'" />\n </video>\n <br><br><br>')
	


	retTraj=PathPlanner.MultipleWayPoints(envPath,context_graph,robotInstructions,trajectorySaveLocation)

	# raw_input('Press enter to run trajectory')
	# To replay a saved trajectory for a given environment execute
	returnVal=generateFeed(robotInstructionsC,videoLocation,inputStr,envPath)
	if videoLocation:
		print inserte2eFeedback(returnVal)
		PathPlanner.playTrajFromFileandSave(envPath,retTraj,videoLocation,camera_angle_path)
	else:
		PathPlanner.playTrajFromFile(envPath,trajectorySaveLocation,camera_angle_path)
	
	return returnVal


if __name__ == '__main__':
	import sys
	# print sys.argv[1:]
	# envName=1
	# envPath=os.path.dirname(os.path.realpath(__file__))+"/environment/env_{0}_context_1.dae".format(envName)
	# contextGraph=os.path.dirname(os.path.realpath(__file__))+"/environment/{0}_graph_1.xml".format(envName)
	# trajectorySaveLocation=os.path.dirname(os.path.realpath(__file__))+"/trajectory/env_{0}_context_1.tk".format(envName)
	# videoLocation=os.path.dirname(os.path.realpath(__file__))+"/video/env_{0}_context_1.mp4".format(envName)
	# camera_angle_path =os.path.dirname(os.path.realpath(__file__))+ '/environment/camera_angle_env_{0}_context_1.pik'.format(envName)
	# inputStr="'move to the blackcouch'"
	print sys.argv[1],sys.argv[2],sys.argv[3],sys.argv[4],sys.argv[5],sys.argv[6]
	returnVal=PlanPathFromNL("'"+sys.argv[1]+"'",sys.argv[2],sys.argv[3],sys.argv[4],sys.argv[5],sys.argv[6])
	
	
