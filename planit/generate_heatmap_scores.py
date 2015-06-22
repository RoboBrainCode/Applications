import os
import sys
import numpy as np
sys.path.append('/home/siddhantmanocha/Downloads/Robobrain/asheshjain399-robotics-b45da53b8b40/learning_affordances/')
import matplotlib.pyplot as plt 
import plotheatmap as phm
import copy
import Image

global home
home = os.path.expanduser('~')

def main():
	print 'hello'
	path = home + '/Downloads/Robobrain/asheshjain399-robotics-b45da53b8b40/data/tasks_vaibhav/planit_result_videos/environment/'
	env_list = range(1,21)
	use_beta = True
	if len(sys.argv) == 2:
		env_list = [int(sys.argv[1])]
	print env_list
	for e in env_list:
		env_path = path + 'env_{0}_context_{1}.dae'.format(e,1)
		context_graph_path = path + '{0}_graph_{1}.xml'.format(e,1)
		if e <= 8:
			params_file_path = path + '../params/params_task_bedroom_env_1,2,3,4,5,6,7,8,9_context_1,2,3,4_s_False_r_False_n_False_b_True.pik'
		else:
			params_file_path = path + '../params/params_task_entertainment_env_1,2,3,4,5,6,7,8_context_1,2,3,4_s_False_r_False_n_False_b_True.pik'
		
		# print env_path
		# print context_graph_path
		# print params_file_path
		# break
		if os.path.exists(env_path) and os.path.exists(context_graph_path) and os.path.exists(params_file_path): 
			
			heatmap_array = phm.heatmap(env_path,context_graph_path,params_file_path,use_beta)
			
			fig = plt.figure(e)
			imo = plt.imshow(heatmap_array)
			fig.savefig(path+'../figs/env_{0}_context_{1}_beta_{2}.png'.format(e,1,use_beta))
			#plt.show()
			
			imo.write_png(path+'../figs/env_{0}_context_{1}_beta_{2}_noborder.png'.format(e,1,use_beta))
			img = Image.open(path+'../figs/env_{0}_context_{1}_beta_{2}_noborder.png'.format(e,1,use_beta))
			rsize = img.resize((img.size[0]/5,img.size[1]/5))
			rsize.save(path+'../figs/env_{0}_context_{1}_beta_{2}_noborder_small.png'.format(e,1,use_beta))

			# best_traj = ''
			# best_score = float('inf')
			# for trajid in range(10):
			# 	traj_pik_file_path = path + 'traj_env_{0}_context_{1}_id_{2}.pik'.format(e,1,trajid)
			# 	if not os.path.exists(traj_pik_file_path):
			# 		continue
			# 	score = phm.scoretraj(env_path,context_graph_path,params_file_path,traj_pik_file_path,use_beta)
			# 	if score < best_score:
			# 		best_score = score
			# 		best_traj = traj_pik_file_path
			# print "Best traj ",best_traj


if __name__=="__main__":
	main()
