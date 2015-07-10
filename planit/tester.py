#!/usr/bin/python
# -*- coding: iso-8859-1 -*-
from __future__ import with_statement # for python 2.5
__author__= 'Ashesh Jain'
import os
import time
import openravepy
import numpy.random as rand
if not __openravepy_build_doc__:
	from openravepy import *
	from numpy import *
import xml.dom.minidom
import sys
from optparse import OptionParser
from openravepy.misc import OpenRAVEGlobalArguments
import pickle
from bs4 import BeautifulSoup
from threading import Thread,Lock
import Tkinter
from functools import partial
app=None
trajectorySaveLocation=None
robot=None
configParams=None
openraveLock=Lock()
stopValLock=Lock()
stopVal=0

class simpleapp_tk(Tkinter.Tk):
	def __init__(self,parent):
		Tkinter.Tk.__init__(self,parent)
		self.parent = parent
		self.initialize()

	def initialize(self):
		
		self.insbuttons=list()
		self.stopbuttons=list()
		self.grid()
		action_with_arg = partial(self.OnButtonPlay)
		button = Tkinter.Button(self,text=u"Play!",
		command=action_with_arg ,padx=10,pady=10,anchor='w',
		justify='center',background='white',activebackground='white',
		bd=4,font='serif',width=12,state='disabled')
		button.grid(column=0,row=0)
		self.playbutton=button

		action_with_arg = partial(self.OnButtonStop)
		button = Tkinter.Button(self,text=u"Stop!",
		command=action_with_arg ,padx=10,pady=10,anchor='w',
		justify='center',background='white',activebackground='white',
		bd=4,font='serif',width=10,state='disabled')
		button.grid(column=1,row=0)
		self.stopbutton=button

		action_with_arg = partial(self.OnButtonR1)
		button = Tkinter.Button(self,text=u"Actual Way Point",
		command=action_with_arg ,padx=10,pady=10,anchor='w',
		justify='center',background='white',activebackground='white',
		bd=4,font='serif',width=13,state='disabled')
		button.grid(column=2,row=0)
		self.r1button=button


		action_with_arg = partial(self.OnButtonR2)
		button = Tkinter.Button(self,text=u"Updated Way Point",
		command=action_with_arg ,padx=10,pady=10,anchor='w',
		justify='center',background='white',activebackground='white',
		bd=4,font='serif',width=15,state='disabled')
		button.grid(column=3,row=0)
		self.r2button=button


		self.labelVariable = Tkinter.StringVar()
		label = Tkinter.Label(self,textvariable=self.labelVariable,
							  anchor="w",fg="black",font=('serif',20),height=0,padx=10,pady=30)
		label.grid(column=0,row=3,columnspan=4,sticky='EW')
		self.labelVariable.set(u"Instruction Sequence Generated! \n click on insruction to play!")
		global configParams
		for i in range(len(configParams['start_configs'])):
			dispString='Move From'+configParams['start_configs'][i]+'to'+configParams['end_configs'][i]
			action_with_arg = partial(self.OnButtonClick, i)
			button = Tkinter.Button(self,text=dispString,
			command=action_with_arg ,padx=10,pady=10,anchor='w',
			justify='center',background='white',activebackground='white',
			bd=4,font='serif',width=40)
			self.insbuttons.append(button)
			button.grid(column=0,row=i+10,columnspan=4)

		self.geometry('{}x{}'.format(550,700))
		self.grid_columnconfigure(0,weight=1)
		self.resizable(True,True)
		self.update()
		
	def OnButtonClick(self,i):
		print i
		global globThread
		global stopVal

		with stopValLock:
			stopVal=-1
		
		globThread = Thread(target=nope, args=(i,))
		globThread.start()
		
		global configParams
		for j in range(len(configParams['start_configs'])):
			self.insbuttons[j]['state'] = 'disabled'
		self.stopbutton['state'] = 'normal'
		self.playbutton['state'] = 'disabled'
		self.r1button['state'] = 'disabled'
		self.r2button['state'] = 'disabled'
		

	def OnButtonR1(self):
		global configParams
		for j in range(len(configParams['start_configs'])):
			self.insbuttons[j]['state'] = 'disabled'
		global envG
		print robot.GetTransform()
		self.playbutton['state'] = 'disabled'
		self.r1button['state'] = 'disabled'
		self.r2button['state'] = 'normal'

	def OnButtonR2(self):
		global configParams
		for j in range(len(configParams['start_configs'])):
			self.insbuttons[j]['state'] = 'normal'
		print robot.GetTransform()
		self.playbutton['state'] = 'normal'
		self.r1button['state'] = 'normal'
		self.r2button['state'] = 'disabled'

	def OnButtonStop(self):
		global stopVal
		with stopValLock:
			stopVal=1
		global configParams
		for j in range(len(configParams['start_configs'])):
			self.insbuttons[j]['state'] = 'normal'
		self.stopbutton['state'] = 'disabled'
		self.playbutton['state'] = 'normal'
		self.r1button['state'] = 'normal'

	def OnButtonPlay(self):
		global stopVal
		with stopValLock:
			stopVal=0
		global configParams
		for j in range(len(configParams['start_configs'])):
			self.insbuttons[j]['state'] = 'disabled'
		self.stopbutton['state'] = 'normal'
		self.playbutton['state'] = 'disabled'
		self.r1button['state'] = 'disabled'
		self.r2button['state'] = 'disabled'


	def OnPressEnter(self,event):
		self.labelVariable.set( self.entryVariable.get()+" (You pressed ENTER)" )
		self.entry.focus_set()
		self.entry.selection_range(0, Tkinter.END)


def waitrobot(robot):
	"""busy wait for robot completion"""
	while not robot.GetController().IsDone():
		time.sleep(0.01)

def move_arm(openrave_traj,env,robot):
	trajXML=BeautifulSoup(openrave_traj)
	content=trajXML.data.string.split(' ')
	trajXML.data['count']=2
	global stopVal
	traj = RaveCreateTrajectory(env,'')
	for i in range(0,len(content)-9,8):
		while True:
			if stopVal==0:
				break
			elif stopVal==1:
				continue
			elif stopVal ==-1:
				return
		trajXML.data.string=" ".join(content[i:i+16])+" "
		print 'waypoint',i/8+1,'of',len(content)/8
		trajToParse=str(trajXML.body.contents[0])
		traj.deserialize(trajToParse)
		robot.GetController().SetPath(traj)
		waitrobot(robot)
	global app
	app.initialize()
	return

def Playtraj(index):
	global envG
	global trajectorySaveLocation
	global robot
	with open(trajectorySaveLocation,'rb') as ff:
		trajs = pickle.load(ff)	
	move_arm(trajs[index],envG,robot)
	# time.sleep(0.5)

def nope(index):
	with openraveLock:
		global stopVal
		stopVal=0
		Playtraj(index)
		print 'Hello World'


class myClass():
	def help(self):
		print 'Enterd fn1'
		global app
		app = simpleapp_tk(None)
		app.title('my application')
		app.mainloop()
	
if __name__ == "__main__":
	env_colladafile = '../environment/env_100_context_1.dae'
	trajectorySaveLocation='environment/t1.pk'
	global envG
	global robot
	envG=Environment()
	envG.Load(env_colladafile)
	envG.SetViewer('qtcoin')
	robot = envG.GetRobots()[0]
	Yep=myClass()
	thread = Thread(target = Yep.help)
	thread.start()
	global trajectorySaveLocation
	global configParams
	trajectorySaveLocation='environment/t1.pk'
	configParams=dict()
	configParams['start_configs']=list()
	configParams['end_configs']=list()
	configParams['start_configs'].append('PR2')
	configParams['start_configs'].append('pillow_2')
	configParams['start_configs'].append('bed_1')
	configParams['start_configs'].append('pillow_1')
	configParams['end_configs'].append('pillow_2')
	configParams['end_configs'].append('bed_1')
	configParams['end_configs'].append('pillow_1')
	configParams['end_configs'].append('bed_1')





