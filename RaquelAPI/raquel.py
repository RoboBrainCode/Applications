import sys
from parser import cyParser
from runQuery import fetchQuery,updateQuery
def Belief(p):
	'''input a path(list of edges)
		finds the belief of the path'''
	path = 0
	pathBelief = 1
	for rel in p:
		if 'belief_fake' in rel:
			belief = float(rel['belief_fake'][0])
			pathBelief = min(pathBelief, belief)
		else: 
			return 'no belief property exists'
	return pathBelief

def SortBy(p,property):
	'''function takes a lists of paths containing edges and a property to use as a key
		and return the sorted list'''
	if type(p).__name__== 'dict':
		print 'path doesnot exist'
		return 'path is empty'

	if property == 'Belief':
		if 'belief_fake' in p[0][0]:
			new_results = sorted(p, key=lambda k: Belief(k))
		else:
			return 'no belief property exists'	
	else: 
		return 'currently sorting is not supported for property other then belief'
	print Belief(new_results[0]),Belief(new_results[1])
	return new_results


def fetch(pattern):
	''' Takes a modified cypher pattern e.g.
            (a)-[:`HAS_MATERIAL`]->(b) and returns the list of
            tuples of instantiated values of named variables. The variable
            can be omitted in which case they are not returned. However,
            before making the cypher query, we add these variables.
	'''
	return_string = cyParser(pattern)
	cypherQuery="MATCH "+ pattern +" RETURN " + return_string +" LIMIT 25"
	result=fetchQuery(cypherQuery)
	if result:
		# print result
		return result
	else:
		return {'test':'no results for this query'}
def update(nodeProps):
	result=updateQuery(nodeProps)
	return result
# if __name__ == "__main__":
# 	# fetch("({handle:'wall'})-[:`HAS_MATERIAL`]->(b)")
# 	# fetch("({handle:'phone'})-[]->(e)")
# 	fetch("({handle:'wall'})-[:`HAS_MATERIAL`]->(e)")
# 	# fetch("(s)-[:`HAS_MATERIAL`]->({handle:'wall'})")
# 	# fetch("(s)-[]->({handle:'wall'})")
# 	# fetch("({handle:'standing_human'})-[e]->({handle:'phone'})")
# 	# fetch("({handle:'standing_human'})-[e*..5]->({handle:'volume'})")
# 	# fetch("({handle:'standing_human'})-[e*3..5]->({handle:'volume'})")
# 	# fetch("({handle:'standing_human'})-[e*1]->({handle:'volume'})")
# 	# fetch("({handle:'standing_human'})-[*1..1]->(e)")
# 	# fetch("(v)-[*5]->({handle:'wall'})")

