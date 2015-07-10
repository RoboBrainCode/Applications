import requests
import urllib
import yaml
import json

def insertFeedback(data):
	headers = {'Content-type': 'application/json', 'Accept': 'application/json'}
	url = "http://localhost:6363/e2eFeedback/insertFeedback/"
	r = requests.get(url, data=json.dumps(data), headers=headers)
	response=yaml.safe_load(r.text)
	print 'Response:'
	print response
	
def main():
	tellmedaveOutput=list()
	tellmedaveOutput.append('seq')
	planitInput=list()
	planitInput.append('seq')
	data={'actualInput':'test','tellmedaveOutput':tellmedaveOutput,'videoPath':'seq','feedId':'seq1237'}
	print 'function Called'
	insertFeedback(data)
main()
