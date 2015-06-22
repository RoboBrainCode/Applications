import os
import yaml
from colladatoxml import colladatoxml
import numpy as np
import requests
import urllib
robobrain_graph = "http://d1rygkc2z32bg1.cloudfront.net/"

def getFileFromURL(handle):
	testfile = urllib.URLopener()
	path = robobrain_graph+(handle.split('__'))[1]
	print path
	index = handle.rfind('/')
	name = handle[index+1:]
	print name
	testfile.retrieve(path, os.path.dirname(os.path.realpath(__file__))+"/Dataset/webfiles/"+name)

def preProcess(raquelResponse):
	for handle in raquelResponse:
		getFileFromURL(handle)	

def NLtoRobotInstructions(inputStr,envPath):
	colladatoxml(envPath)
	os.system(os.path.dirname(os.path.realpath(__file__))+"/LanguageGrounding "+inputStr)
	jsonPath=os.path.dirname(os.path.realpath(__file__))+"/dict.json"
	with open(jsonPath) as json_file:
		#inputToPlanIt is a dict {"start":["xbox","cd"],"stop":["robot","human"]}
		inputToPlanIt = yaml.safe_load(json_file)
	return inputToPlanIt

if __name__ == '__main__':
	envPath=os.path.dirname(os.path.realpath(__file__))+"/../environment/env_1_context_1.dae"
	inputStr="'move to the blackcouch'"
	callTellmedave(inputStr,envPath)

	
