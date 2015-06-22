import os,yaml,pickle
import tellmedave.languageGrounding as tellmedave
import planit.PathPlanner as PathPlanner
import RaquelAPI.raquel as raquel
 
def PlanPathFromNL(inputStr,envPath,context_graph,trajectorySaveLocation):

	# Using roboBrain to query tellmedave parameters
	raquelResponse = raquel.fetch("({handle:'tellmedave'})-[:`HAS_PARAMETER`]->(b)")
	tellmedave.preProcess(raquelResponse['1'])
	raquelResponse=raquel.fetch("({handle:'tellmedave'})-[:`HAS_WEIGHTS`]->(b)")
	tellmedave.getFileFromURL(raquelResponse['1'][0])
	
	# calling tellmedave
	robotInstructions=tellmedave.NLtoRobotInstructions(inputStr,envPath)

	# Plan path for environment : envPath, context graph: contextGraph, 
	# robotic instructions returned by tell me dave: robotInstructions, 
	# save the learned trajectories at trajectorySaveLocation
	PathPlanner.MultipleWayPoints(envPath,contextGraph,robotInstructions,trajectorySaveLocation)

	raw_input('Press enter to run trajectory')
	# To replay a saved trajectory for a given environment execute
	PathPlanner.playTrajFromFile(envPath,trajectorySaveLocation)

if __name__ == '__main__':
	envPath=os.path.dirname(os.path.realpath(__file__))+"/environment/env_1_context_1.dae"
	contextGraph=os.path.dirname(os.path.realpath(__file__))+"/environment/1_graph_1.xml"
	trajectorySaveLocation=os.path.dirname(os.path.realpath(__file__))+"/environment/t1.tk"
	inputStr="'move to the blackcouch'"
	PlanPathFromNL(inputStr,envPath,contextGraph,trajectorySaveLocation)
	
