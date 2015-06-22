import sys
sys.dont_write_bytecode = True
from parser import cyParser
from runQuery import runQuery

def Belief(p):
	'''input a record containing list of relationships
		finds the belief of the path in the record'''
	path = 0
	pathBelief = 1
	for rel in p[0]:
		if 'belief' in rel:
			belief = rel['belief']
			pathBelief = min(pathBelief, belief)
		else: 
			return 'no belief property exists'
	return pathBelief


def parents(n):
	'''returns parents of node n
	to call parents(record_containing_node)'''
	if type(n[0]).__name__ == 'Node':
		return fetch("(v)-[:HAS_PARAMETERS]->({handle:" + handle(n) + "})")


def appearance(n):
	'''returns appearance of object n'''
	if type(n).__name__ == 'str':
		# print 'handle '+n 
		return fetch("({handle:'" + n + "'})-[:`SAME_SYNSET`]->(v)")

def affordance(n):
	'''returns affordances of object n'''
	if type(n).__name__ == 'str':
		return fetch("({handle:" + n + "})-[:HAS_AFFORDANCE]->(v)")

def SortBy(property,p):
	'''function takes a recordList and a property to use as a key
		and return the sorted list'''
	if property == 'Belief':
		if 'belief' in p[0][0]:
			nw_results = sorted(p, key=lambda k: Belief(k))
		else:
			return 'no belief property exists'	
	else: 
		return 'currently not supported'
	return new_results

def trajectory(n):
	'''returns affordances of object n(record)'''
	if type(n[0]).__name__ == 'Node':
		return fetch("({handle:" + handle(n[0]) + "})-[:IS_TRAJECTORY_OF]->(v)")

def handle(n):
	if type(n).__name__ == 'Node':
		return n.properties['handle']


def printingRecords(results):
	for record in results:
		for NoR in record:
			type(NoR)
			if type(NoR).__name__ == 'Node':
				print 'node.handle ',
				print NoR.properties['handle'],
			else:		#py2neo.core.Relationship
				print 'Relationship.keywords ',
				print NoR.properties['keywords'],
		print ""


def fetch(pattern):
	''' Takes a modified cypher pattern e.g.
            (a)-[:`HAS_MATERIAL`]->(b) and returns the list of
            tuples of instantiated values of named variables. The variable
            can be omitted in which case they are not returned. However,
            before making the cypher query, we add these variables.
	'''
	return_string = cyParser(pattern)
	cypherQuery="MATCH "+ pattern +" RETURN " + return_string +" LIMIT 25"
	result=runQuery(cypherQuery)
	# print result
	if result:
		return result
	else:
		return {'test':'Failed'}

if __name__ == "__main__":
	# fetch("({handle:'wall'})-[:`HAS_MATERIAL`]->(b)")
	# fetch("({handle:'phone'})-[]->(e)")
	# fetch("({handle:'wall'})-[:`HAS_MATERIAL`]->(e)")
	# fetch("(s)-[:`HAS_MATERIAL`]->({handle:'wall'})")
	# fetch("(s)-[]->({handle:'wall'})")
	# fetch("({handle:'standing_human'})-[e]->({handle:'phone'})")
	# fetch("({handle:'standing_human'})-[e*..5]->({handle:'volume'})")
	# fetch("({handle:'standing_human'})-[e*3..5]->({handle:'volume'})")
	# fetch("({handle:'standing_human'})-[e*1]->({handle:'volume'})")
	# fetch("({handle:'standing_human'})-[*1..1]->(e)")
	# fetch("(v)-[*5]->({handle:'wall'})")
	print fetch("({handle:'wall'})-[:`HAS_MATERIAL`]->(b)")
