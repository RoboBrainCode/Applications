import numpy as np
from scipy.stats import beta as beta_distribution

two_pi = 1.0/np.sqrt(2*np.pi)

def gauss(point,mu,sigma):
	if sigma < 1e-4:
		return 0
	else:
		return (two_pi/np.sqrt(sigma))*np.exp(-0.5*(pow(point-mu,2.0)/sigma))

def vonMiss(point,kappa,mu):
	return ((1.0/(2*np.pi*np.i0(kappa)))*np.exp(kappa*np.dot(point,mu)))	

def probdata(params,node,data,activity_count,activity_local_prob,use_beta=True):
	activity = node['activity']
	distance_between = node['distance']
	von_pdf = 1.0
	pi = (1.0/(activity_count[activity]))  #*activity_local_prob[activity]['prob']
	norm_dist = {}
	radial_gauss = {}
	beta_dist = {}
	for obj in ['obj1','obj2']:
		objid = node[obj]['id']
		obj_type = node[obj]['name']
		xaxis = node[obj]['xaxis']
		yaxis = node[obj]['yaxis']
		xyz = node[obj]['xyz']
		usename = 'human'
		if obj_type != 'human':
			usename = 'object'
		kappa = params[activity][usename]['kappa']
		cirmu = params[activity][usename]['cirmu']
		sigma = params[activity][usename]['sigma']
		mu = params[activity][usename]['mu']
		alpha = params[activity][usename]['alpha']
		beta = params[activity][usename]['beta']

		dist_vec = data[:3] - xyz
		dist = np.linalg.norm(dist_vec[:2])
		dist_vec = dist_vec[:2]
		dist_align = np.array([np.dot(dist_vec, xaxis), np.dot(dist_vec, yaxis)])
		dist_align_norm = dist_align/np.linalg.norm(dist_align)
		ndist = dist_align_norm
	
		norm_dist[objid] = ndist
		radial_gauss[objid] = dist
		beta_dist[objid] = dist_align[0]/distance_between
		
		prob_beta = 1.0
		if use_beta and usename == 'object' and (activity == 'watching' or activity == 'walking'):
			prob_beta = (beta_distribution.pdf(dist_align[0]/distance_between,alpha,beta))

		if usename == 'object' and (activity == 'reaching' or activity == 'working' or activity == "sitting"):
			continue
		if usename == 'human' and (activity == 'reaching' or activity == 'working' or activity == "sitting"):
			mu = 0.0
			sigma = 0.2
			frac_mult = gauss(dist,mu,sigma)
		else:
			frac_mult = 1.0

		#if prob_beta > 0:
		#	prob_beta = 1.0
		
		von_pdf *= vonMiss(ndist,kappa,cirmu)*frac_mult*prob_beta

	von_pdf *= pi
	return von_pdf, norm_dist, radial_gauss, beta_dist
