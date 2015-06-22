#! /usr/bin/env python
import hashlib
def _get_unique_id(node_handle):
    m = hashlib.md5(node_handle.encode())
    return int(str(int(m.hexdigest(), 16))[0:16])

import weaver.client as client

graphClient = client.Client('172.31.33.213', 2002)
print 'created client: forward'

def returnPathMinMax(src='standing_human',dest='volume',path_len_min=0,path_len_max=5,nodeListDisplay=False,path_len_exact=False,displayPath=False):
	
	# path lies from path_length_min to path_length_max, we return all path in this range if path_length_exact=False
	# return path=path_length_max if path_length_exact
	
	nodename1=src
	nodename2=dest
	paths=graphClient.discover_paths(start_node=nodename1,end_node=nodename2, path_len=path_len_max,edge_preds=[client.PropPredicate('edgeDirection','F',1)])
	edges=graphClient.enumerate_paths(paths, nodename1, nodename2, 5)
	flag=0
	counter_ED=0
	all_path=list()
	for edge in edges:
		possible_path=list()
		if nodeListDisplay:
			counter_ED=1
			length=len(edge)
			possible_path=list()
			possible_path.append(edge[0].start_node)
			for i in range(0,length):
				possible_path.append(edge[i].end_node)
		else:
			for ed in edge:
				v1=dict(ed.properties)
				v1['source']=ed.start_node
				v1['target']=ed.end_node
				possible_path.append(v1)

		if len(possible_path)-counter_ED>=path_len_min:
				if path_len_exact:
					if len(possible_path)-1==path_len_max:
						flag=1
						all_path.append(possible_path)
						if displayPath:
							for node in possible_path:
								print node,
							print
				else:
					flag=1
					all_path.append(possible_path)
					if displayPath:
						for node in possible_path:
							print node,
						print 

	return all_path


def CollectNodesRecursive(graphDir,visited,hopLengthNodes,hopPoint,path_len_max):
	hopLengthNodes[hopPoint]=list()
	for node in hopLengthNodes[hopPoint-1]:
		if node in visited:
			pass
		else:
			visited[node]=1
			oneHop=graphClient.traverse(node).out_edge({'edgeDirection':graphDir}).node().execute()
			for n1 in oneHop:
				hopLengthNodes[hopPoint].append(n1)

	if hopPoint==path_len_max:
		return hopLengthNodes
	else:
		return CollectNodesRecursive(graphDir,visited,hopLengthNodes,hopPoint+1,path_len_max)

def CollectNodes(graphDir,src='standing_human',path_len_max=5,properties={}):
	visited=dict()
	hopPoint=1
	hopLengthNodes=dict()
	hopLengthNodes[hopPoint-1]=list()
	start=src
	hopLengthNodes[hopPoint-1].append(start)
	retValFinal=dict()
	edge_props=dict()
	if not path_len_max==1:
		retValFinal=CollectNodesRecursive(graphDir,visited,hopLengthNodes,hopPoint,path_len_max)
	else:
		hopLengthNodes[hopPoint]=list()
		edge_props['edgeDirection']=graphDir
		oneHop=graphClient.traverse(start).out_edge(edge_props=properties).node().execute()
		for node in oneHop:
			hopLengthNodes[hopPoint].append(node)
		retValFinal=hopLengthNodes
	
	return retValFinal


def CollectNodesModified(graphDir,src='standing_human',path_len_min=1,path_len_max=5,properties={}):
	start=src
	retValFinal=dict()
	print 'Collect Nodes Modified src:,path_len:,path_len_max:',src,path_len_min,path_len_max
	for num_hops in range(path_len_min,path_len_max+1):
		query = graphClient.traverse(start)
		for i in range(num_hops):
		    query = query.out_edge({'edgeDirection':graphDir}).node()
		newList=list(query.execute())
		retValFinal[num_hops]=newList

	return retValFinal


def returnNodesForward(src='standing_human',path_len_min=1,path_len_max=5):
	return CollectNodesModified(graphDir='F',src=src,path_len_min=path_len_min,path_len_max=path_len_max,properties={})

def returnNodesBackward(src='laptop',path_len_min=1,path_len_max=5):
	return CollectNodesModified(graphDir='B',src=src,path_len_min=path_len_min,path_len_max=path_len_max,properties={})

def returnNodeOneHopBackward(src='standing_human',properties={}):	
	return CollectNodes(graphDir='B',src=src,path_len_max=1,properties=properties)

def returnNodeOneHopForward(src='standing_human',properties={}):	
	return CollectNodes(graphDir='F',src=src,path_len_max=1,properties=properties)

if __name__ == "__main__":
	print '------------------------------------------------------------------------------'
	print returnPathMinMax(src='standing_human',dest='volume',path_len_min=1,path_len_max=5,path_len_exact=False,nodeListDisplay=False,displayPath=False)	
	print 'Path One Hop properties'
	print returnNodeOneHopForward(src='standing_human',properties={'label':'CAN_USE'})
	print '-------------------------------------------------------------------------------'
	print returnNodeOneHopBackward(src='laptop',properties={'label':'CAN_USE'})
	print '-------------------------------------------------------------------------------'
	print 'Path n-hop Properties'
	print returnNodesForward(src='standing_human',path_len_max=2)
	print '--------------------------------------------------------------------------------'
	print returnNodesBackward(src='volume',path_len_max=2)
	print '--------------------------------------------------------------------------------'

	
