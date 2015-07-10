from __future__ import with_statement # for python 2.5
__author__= 'Ashesh Jain'

from numpy import *
import Tkinter # For GUI
import os
import time
import openravepy
import numpy.random as rand
if not __openravepy_build_doc__:
	from openravepy import *
	from numpy import *

import xml.dom.minidom
import sys
import os
import numpy as np
import webcolors
from sets import Set
import unicodedata
import copy
env=list()
result=dict()
for j in range(16):
	env.append(Environment())
	k=j+1
	env[j].Load('planitDave/env_{0}.dae'.format(k))
	result[k]=Set()
	count=0
	for body in env[j].GetBodies():
		count=count+1
		kinname = body.GetName()
		first_name=kinname.split('-')[0]
		if 'Floor' in kinname:
			continue
		elif ('Wall' in kinname):
			continue
		elif ('PR2' in kinname):
			continue
		result[k].add(first_name)
	print count
	
	result[k]=list(result[k])
	for i in range(len(result[k])):
		result[k][i]=unicodedata.normalize('NFKD',result[k][i]).encode('ascii','ignore')
	print k,len(result[k])

import json
with open('result.json', 'w') as fp:
    json.dump(result, fp)

			
