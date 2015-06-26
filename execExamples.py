from text_to_traject import PlanPathFromNL
import os 

inputFile='test1.txt'

with open(inputFile) as f:
	lines=f.readlines()

for i in range(len(lines)/2):
	envName=lines[2*i].split(':')[1].strip()
	text="'"+lines[2*i+1].split(':')[1].strip()+"'"
	print envName,text
	videoAppend=(text.replace (" ", "_"))[1:-1]
	os.system('rm /home/siddhantmanocha/intern/roboBrainProduction/Applications/tellmedave/Dataset/VEIL500/Environment/planit/*.xml')
	os.system('rm /home/siddhantmanocha/intern/roboBrainProduction/Applications/tellmedave/dict.json')
	envPath=os.path.dirname(os.path.realpath(__file__))+"/environment/env_{0}_context_1.dae".format(envName)
	contextGraph=os.path.dirname(os.path.realpath(__file__))+"/environment/{0}_graph_1.xml".format(envName)
	trajectorySaveLocation=os.path.dirname(os.path.realpath(__file__))+"/trajectory/env_{0}_context_1_{1}.tk".format(envName,videoAppend)
	videoLocation=os.path.dirname(os.path.realpath(__file__))+"/video/env_{0}_context_1_{1}.mp4".format(envName,videoAppend)
	print videoLocation
	camera_angle_path =os.path.dirname(os.path.realpath(__file__))+ '/environment/camera_angle_env_{0}_context_1.pik'.format(envName)
	inputStr=text
	
	os.system('python text_to_traject.py '+inputStr+' '+envPath+' '+contextGraph+' '+trajectorySaveLocation+' '+videoLocation+' '+camera_angle_path)
	# PlanPathFromNL(inputStr,envPath,contextGraph,trajectorySaveLocation,videoLocation,camera_angle_path)
	
