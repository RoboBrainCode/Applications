import numpy as np
import requests
import urllib
import os
import yaml
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

def getResponse(query):
	myport=6363
	data={'query':query}
	myURL = "http://52.25.65.189:%s/raquel/rachQuery/?%s" % (myport, urllib.urlencode(data))
	r = requests.get(myURL)
	response=yaml.safe_load(r.text)
	import ast
	return ast.literal_eval(response['result'][:-4])
