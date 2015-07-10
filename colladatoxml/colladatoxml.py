def quaternionToEuler(string):
	from math import radians, sqrt, pi, atan2, asin
	tmp = string.split(" ")
	x = float(tmp[0])
	y = float(tmp[1])
	z = float(tmp[2])
	unitLength = sqrt(x*x + y*y + z*z)
	a = float(tmp[3])
	a = radians(a)
	abcd = a*x + y*z
	eps = 1e-7    
	if abcd > (0.5-eps)*unitLength:
		yaw = 2 * atan2(y, a)
		pitch = pi
		roll = 0
	elif (abcd < (-0.5+eps)*unitLength):
		yaw = -2 * atan2(y, a)
		pitch = -pi
		roll = 0
	else:
		adbc = a*z - x*y
		acbd = a*y - x*z
		yaw = atan2(2*adbc, 1 - 2*(z*z+x*x))
		pitch = asin(2*abcd/unitLength)
		roll = atan2(2*acbd, 1 - 2*(y*y+x*x))
	euler = "("+ str(roll)+', '+str(pitch)+', ' +str(yaw)+')'
	# print euler
	return euler

def colladatoxml():
	import xml.etree.ElementTree as ET
	create_root = ET.Element("environment")

	# for i in xrange(1,19):
		# print i
	i=100
	tree = ET.parse('environment/env_'+str(i)+'_context_1.dae')
	root = tree.getroot()
	for child in root[1][0]:
		doc = ET.SubElement(create_root, "object")
		ET.SubElement(doc, "name").text=child.attrib['name']
		# print child.attrib['name']
		ET.SubElement(doc, "position").text=(child[0].text).replace(" ", ", ")
		ET.SubElement(doc, "rotation").text=quaternionToEuler(child[1].text) 
		tree = ET.ElementTree(create_root)
	tree.write("tmd_env/livingRoom"+str(i)+".xml")	
	create_root.clear()
	print 'environment generated: livingRoom'+str(i)+".xml"


if __name__ == '__main__':
	colladatoxml()
	# quaternionToEuler("0 0 -1 89.99999999999999")
	# print pi