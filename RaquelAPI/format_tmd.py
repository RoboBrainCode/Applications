#!/usr/bin/python
# -*- coding: ascii -*-
import re
# toadd=[]

def repl(matchobj):
    global toadd
    if matchobj.group(0)!="UTF-8":
    	ch=matchobj.group(0)
    	ch=ch.replace('-','_')
    	index=ch.find('.')
    	if index!=-1:
    		ch=ch.replace(ch[index-1],str(int(ch[index-1])+1))
    		ch = ch[:index]
    		# print ch
    		
    	# print 'reached: '+ch
    	# if ch not in toadd:
    	# 	toadd.append(ch)
    	return ch
    else:
    	return matchobj.group(0)

def format_tmd(filename):
	# with open("objects.txt","w") as obj:
	# filename = 'environment/env_'+str(i)+'_context_1.dae'
	with open(filename, "r+") as f:
		data = f.read()
		format_check = re.compile(r'(?<![0-9])[A-Za-z]+-[0-9][.]?[0-9]?')
		data = re.sub(format_check,repl,data)
		f.seek(0)
		f.write(data)
		f.truncate()
		# for item in toadd:
  # 			obj.write("%s\n" % item)
  # 		print toadd
		

if __name__ == '__main__':
	format_tmd()