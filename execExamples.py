from text_to_traject import PlanPathFromNL
import os 
import RaquelAPI.format_tmd as formaTMD
inputFile='test1.txt'
# inputFile='test1.txt'


with open(inputFile) as f:
	lines=f.readlines()

os.system('rm '+os.path.dirname(os.path.realpath(__file__))+'/results.html')

for i in range(len(lines)/2):
	envName=lines[2*i].split(':')[1].strip()
	text="'"+lines[2*i+1].split(':')[1].strip()+"'"
	with open(os.path.dirname(os.path.realpath(__file__))+'/results.html','a+') as f:
		f.write('<h3> Original Instruction </h3> \n <p>'+text+' </p>\n <h4> Tell Me Dave Output </h4>')
	print envName,text
	videoAppend=(text.replace (" ", "_"))[1:-1]
	os.system('rm '+os.path.dirname(os.path.realpath(__file__))+'/tellmedave/Dataset/VEIL500/Environment/planit/*.xml')
	os.system('rm '+os.path.dirname(os.path.realpath(__file__))+'/tellmedave/dict.json')
	envPath=os.path.dirname(os.path.realpath(__file__))+"/planitDave/env_{0}.dae".format(envName)
	contextGraph=os.path.dirname(os.path.realpath(__file__))+"/environment/{0}_graph_1.xml".format(envName)
	trajectorySaveLocation=os.path.dirname(os.path.realpath(__file__))+"/trajectory/env_{0}_context_1_{1}.tk".format(envName,videoAppend)
	
	formaTMD.format_tmd(envPath)
	formaTMD.format_tmd(contextGraph)


	# videoLocation=os.path.dirname(os.path.realpath(__file__))+"/video/env_{0}_context_1_{1}.mp4".format(envName,videoAppend)
	# print videoLocation
	videoDir='/'.join(os.path.dirname(os.path.realpath(__file__)).split('/')[:-1])+'/Frontend/app/images/planit'
	videoLocation=videoDir+'/env_{0}_context_1_{1}.mp4'.format(envName,videoAppend)
	camera_angle_path =os.path.dirname(os.path.realpath(__file__))+ '/environment/camera_angle_env_{0}_context_1.pik'.format(envName)
	inputStr=text
	os.system('python text_to_traject.py '+inputStr+' '+envPath+' '+contextGraph+' '+trajectorySaveLocation+' '+videoLocation+' '+camera_angle_path)
	# PlanPathFromNL(inputStr,envPath,contextGraph,trajectorySaveLocation,videoLocation,camera_angle_path)
	
