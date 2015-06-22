using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ProjectCompton
{
	class Mapping
	{
		Inference inf = null;

		public Mapping (Inference inf)
		{
			this.inf = inf;
		}


		public Tuple<int[], Double, String> mappingPredicates(LexicalEntry vtmp, Clause clTest, Environment envTest, List<Instruction> instPrev, Dictionary<String, Double> weights
		                                                      , double[,] oldLECorrMatrix)
		{
			/* Function Description: Finds a 'good' mapping from variables in eq to objects in envTest.
             * This is done by converting it into a queadratic optimization problem with convex hull 
             * domain. Third-party softwares are used to solve this optimization then. 
             * There are 6-type of features responsible for this mapping - 
             * 1. Map objects that they are similar to previously mapped one
             *    \sum_{ij} w_{ee} C^1_{ij} x_{ij}
             *    C^1_{ij} = \sigma_{EE_v} [\xi_v(z_i),j] --- coefficient of env-env correlation
             *    
             * 2. Map objects that have similar spatial relation between objects
             *    \sum_{i_1j_2, i_2j_2} w_{se} C^2_{i_1j_2, i_2j_2} x_{i_1j_1} x_{i_2j_2}
             *    C^2_{i_1j_2, i_2j_2} = 1{\gamma^v_{ij} = \gamma_{kl}} --- coefficient of spatial-relation of environment
             * 
             * 3. Map objects that use maximum from language 𝐿
             *    \sum_{ij} w_{le} C^3_{j} x_{ij} 
             *    C^3_{j} = max_k {\sigma_{LE}[k,j]} --- coefficient of language-environment correlation
             *    
             * 4. Map objects that have strong language correlation 
             *    \sum_{ij} w_{ll} C^4_{ij} x_{ij}
             *    C^4_{ij} = max_{p,q} { \sigma_{L_vE_v}[p,\xi_v(i)]\sigma_{L_vL}[p,q,]\sigma_{LE}[q,j]} --- coefficient of language-language correlation
             * 
             * 5. Map objects that have similar spatial relation between their mapped language
             *   \sum_{i_1j_1,i_2j_2} w_{sl} C^5_{i_1j_1,i_2j_2} x_{i_1j_1} x_{i_2j_2}
             *    C^5_{i_1j_1,i_2j_2} = max_{p_1,q_1,p_2,q_2} 1{\rho_v[p_1,q_1] = \rho[p_2,q_2]}
             *                        \sigma_{L_vE_v}[p_1,\xi_v(i_1)] \sigma_{L_vL}[p_1,q_1] \sigma_{LE}[q_1,j_1]
             *                        \sigma_{L_vE_v}[p_2,\xi_v(i_2)] \sigma_{L_vL}[p_2,q_2] \sigma_{LE}[q_2,j_2]                      
             * 
             * 6. If variable i_1 and i_2 occur together in predicates P1[i_1,i_2],  P2[i_1,i_2], ...
             *    then return the minimum confidence predicate.
             *   \sum_{i_1j_1,i_2j_2} w_{confidence} C^6_{i_1j_1,i_2j_2} x_{i_1j_1} x_{i_2j_2}
             *    C^6_{i_1j_1,i_2j_2} = min_k confidence Pk[i_1 -> j_1, i_2 -> j_2]
             *   
             *  Quadratic Cost function can then be written as - \sum_{ij} a_{ij}x_{ij} + \sum_{i_1j_1,i_2j_2} b_{i_1j_2,i_2j_2}x_{i_1j_1}x_{i_2j_2}
             *       a_{ij} = w_{ee}C^1{ij} + w_{le} C^3_{j} + w_{ll}C^4_{ij}
             *       b_{i_1j_1,i_2j_2} = w_{se} C^2_{i_1j_2, i_2j_2} + w_{sl} C^5_{i_1j_1,i_2j_2}
             */

			if (Constants.cacheReadQP)
				;//return this.inf.fetchMapFromCache();

			int lngTestN = clTest.lngObj.Count(), lngTrainN = vtmp.cls_.lngObj.Count(),
			envTestN = envTest.numObjects(),  envTrainN = vtmp.env_.numObjects(), z = vtmp.numVariables(true);

			if (z == 0)
				return new Tuple<int[], double, string> (new int[0], 0,"No variable");

			double[,] eeCorrMatrix = vtmp.env_.getEnvCorrMatrix(envTest); //E-E correlation Matrix. Its in general asymmteric
			double[,] llCorrMatrix = vtmp.cls_.getLngCorrMatrix(clTest, this.inf.sensim);  //L-L correlation Matrix. Its Symmetric
			double[,] leTestCorrMatrix = envTest.getLECorrMatrix(clTest, instPrev, this.inf.sensim, this.inf.ftr),
			 leTrainCorrMatrix = vtmp.env_.getLECorrMatrix(vtmp.cls_, vtmp.instOld, this.inf.sensim, this.inf.ftr); //L-E correlation Matrix.

			//Naming Convention - x_{ij} \in {0,1} and ij is represented in row-major format
			double[] c1 = new double[z * envTestN];
			double[,] c2 = new double[z * envTestN, z * envTestN];
			double[] c3 = new double[envTestN];
			double[] c4 = new double[z * envTestN];
			double[,] c5 = new double[z * envTestN, z * envTestN];
			double[,] c6 = new double[z * envTestN, z * envTestN];
			double[] a = new double[z * envTestN];
			double[,] b = new double[z * envTestN, z * envTestN];

			#region computing_coefficients
			//Computing Coefficient C1
			for (int i = 0; i < z; i++)
			{
				for (int j = 0; j < envTestN; j++)
					c1[i * envTestN + j] = eeCorrMatrix[vtmp.getOrigObject(i,true), j];
			}

			//Computing Coefficient C2
			for (int i1 = 0; i1 < z; i1++)
            {
                for (int i2 = i1; i2 < z; i2++)
                {
                    for (int j1 = 0; j1 < envTestN; j1++)
                    {
                        for (int j2 = j1; j2 < envTestN; j2++)
                        {
                            if (vtmp.env_.getRelationship(vtmp.getOrigObject(i1,true), vtmp.getOrigObject(i2,true)) == envTest.getRelationship(j1, j2))
                                c2[i2 * envTestN + j2, i1 * envTestN + j1] = c2[i1 * envTestN + j1, i2 * envTestN + j2] = 1;
                            else c2[i2 * envTestN + j2, i1 * envTestN + j1] = c2[i1 * envTestN + j1, i2 * envTestN + j2] = 0;
                        }
                    }
                }
            }

			//Computing Coefficient C3
			for (int j = 0; j < envTestN; j++)
			{
				c3[j] = 0;
				for (int k = 0; k < lngTestN; k++)
					c3[j] = Math.Max(c3[j], leTestCorrMatrix[k, j]);

				//also consider old le matrix
				if(oldLECorrMatrix!=null)
				{
					for (int k = 0; k < oldLECorrMatrix.GetLength(0); k++)
						c3[j] = Math.Max(c3[j], oldLECorrMatrix[k, j]);
				}
			}

			//Computing Coefficient C4
			for (int i = 0; i < z; i++)
			{
				for (int j = 0; j < envTestN; j++)
				{
					c4[i * envTestN + j] = 0;
					for (int p = 0; p < lngTrainN; p++)
					{
						for (int q = 0; q < lngTestN; q++)
						{
							double newSample = leTrainCorrMatrix[p, vtmp.getOrigObject(i,true)] * llCorrMatrix[p, q] * leTestCorrMatrix[q, j];
							c4[i * envTestN + j] = Math.Max(c4[i * envTestN + j], newSample);
						}
					}
				}
			}

			//Computing Coefficient C5
			/*for (int i1 = 0; i1 < z; i1++)
            {
                for (int i2 = i1; i2 < z; i2++)
                {
                    for (int j1 = 0; j1 < envTestN; j1++)
                    {
                        for (int j2 = j1; j2 < envTestN; j2++)
                        {
                            c5[i1 * envTestN + j1, i2 * envTestN + j2] = 0;
                            for (int p1 = 0; p1 < lngTrainN; p1++)
                            {
                                for (int p2 = 0; p2 < lngTrainN; p2++)
                                {
                                    for (int q1 = 0; q1 < lngTestN; q1++)
                                    {
                                        for (int q2 = 0; q2 < lngTestN; q2++)
                                        {
                                            double newSample = 0;
                                            if (eq.cls_.relation[p1, p2] == clTest.relation[q1, q2])
                                            {
                                                newSample = leTrainCorrMatrix[p1, eq.getOrigObject(i1)] * llCorrMatrix[p1, q1] * leTestCorrMatrix[q1, j1];
                                                newSample = newSample * leTrainCorrMatrix[p2, eq.getOrigObject(i2)] * llCorrMatrix[p2, q2] * leTestCorrMatrix[q2, j2];
                                            }
                                            c5[i1 * envTestN + j1, i2 * envTestN + j2] = Math.Max(c5[i1 * envTestN + j1, i2 * envTestN + j2], newSample);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }*/

			//Computing Coefficient C6
			for (int i1 = 0; i1 < z; i1++)
			{
				for (int i2 =i1+1; i2 < z; i2++)
				{
					//find all predicates in which variable i1 and i2 occur together
					List<String> cover = vtmp.predicateCover(i1,i2);
					for (int j1 = 0; j1 < envTestN; j1++)
					{
						for (int j2 = 0; j2 < envTestN; j2++)
						{
							double confidence =0;
							foreach(String pred in cover)
							{
								String pred_ = pred.Replace(vtmp.zVariablePredicatePost[i1],envTest.objects[j1].uniqueName)
									.Replace(vtmp.zVariablePredicatePost[i2],envTest.objects[j2].uniqueName);
								confidence = confidence + this.inf.ftr.getPredicateFreq1(new string[1]{pred_}, false, null);//this.inf.ftr.getPredicateFreqOld(pred_)/(this.inf.ftr.zPredFreqOld+Constants.epsilon);
							}
							if(cover.Count()==0)
								confidence= 1;
							else confidence = confidence/cover.Count();
							c6[i1 * envTestN + j1, i2 * envTestN + j2]=confidence;
							c6[i2 * envTestN + j2, i1 * envTestN + j1]=confidence;
						}
					}
				}
			}

			//Computing a vector
			for (int i = 0; i < z; i++)
			{
				for (int j = 0; j < envTestN; j++)
					a[i * envTestN + j] = weights["w_ee"] * c1[i * envTestN + j] + weights["w_le"] * c3[j] + weights["w_ll"] * c4[i * envTestN + j];
			}

			//Computing b matrix
			for (int i1 = 0; i1 < z; i1++)
            {
                for (int i2 = i1; i2 < z; i2++)
                {
                    for (int j1 = 0; j1 < envTestN; j1++)
                    {
                        for (int j2 = 0; j2 < envTestN; j2++)
                        {
                            b[i1 * envTestN + j1, i2 * envTestN + j2] = weights["w_se"] * c2[i1 * envTestN + j1, i2 * envTestN + j2] +
                                                                        weights["w_sl"] * c5[i1 * envTestN + j1, i2 * envTestN + j2] + 
																		100 * c6[i1 * envTestN + j1, i2 * envTestN + j2];
                        }
                    }
                }
            }
			#endregion


			#region computing_constraints
			/* There are three types of constraints - 
             * 1. x_{ij} \in {0,1} \forall_ij 
             *    after relaxation x_{ij} >= 0 and -x_{ij} >= -1 that is 2*z*envTestN constraints
             * 2. \sum_j x_{ij} = 1 \forall_i that is  constraints
             *    after relaxation \sum_j x_{ij} >= 1 and \sum_j -x_{ij} >= -1 that is 2*z constraint
             * 3. Some variables can e.g. (state $1 state-name) then $1 can only map to objects
             *    such that it has the given state-name
             * 4  Some variable have constraints from relationship e.g. (Grasping $1 $2) then $1
             *    has to be an object that can grasp such as Robot
             */

			List<double[]> constraints_ = new List<double[]>();
			List<double> lower_ = new List<double>();

			//Constraint 1 after relaxation becomes - x_{ij} >=0 and -x_{ij} >= -1
			for (int cstr = 0; cstr < z * envTestN; cstr++)
			{
				double[] l = new double[z * envTestN];
				double[] u = new double[z * envTestN];
				for (int k = 0; k < z * envTestN; k++)
				{
					l[k] = 0;
					u[k] = 0;
				}
				l[cstr] = 1; //x_{ij}
				u[cstr] = -1; //-x_{ij}
				constraints_.Add(l);
				constraints_.Add(u);
				lower_.Add(0); //x_{ij} > =0
				lower_.Add(-1); //-x_{ij} >= -1
			}

			//Constraint 2 \sum_j x_{ij} = 1
			for (int cstr = 0; cstr < z; cstr++)
			{
				double[] l = new double[z * envTestN];
				double[] u = new double[z * envTestN];
				for (int i = 0; i < z; i++)
				{
					for (int j = 0; j < envTestN; j++)
					{
						if (i == cstr)
						{
							l[i * envTestN + j] = 1;
							u[i * envTestN + j] = -1;
						}
						else
						{
							l[i * envTestN + j] = 0;
							u[i * envTestN + j] = 0;
						}
					}
				}

				constraints_.Add(l);
				constraints_.Add(u);
				lower_.Add(1); //\sum_{j} x_{ij} >= 1
				lower_.Add(-1); //\sum_{j} -x_{ij} >= -1
			}

			//Constraint 3 : Semantic Restrictions
			List<Object> objL=envTest.objects;
			for (int i = 0; i < z; i++) //for each generalized variable
			{
				List<String> predicates = vtmp.predicatesPost; //find all predicates containing zth variable
				foreach(String predicate_ in predicates)
				{
					String predicate = Global.getAtomic(predicate_).Item2;
					String[] words=predicate.Split(new char[]{' '});
					if(words[0].Equals("state") && words[1].Equals(vtmp.zVariablePredicatePost[i]))
					{
						bool feasible=false;
						for(int j=0; j<envTestN;j++)
						{
							if(!objL[j].ifStateExist(words[2]))//state-name is words[2]
							{
								//add the constraint - x_{ij} >= 0 and -x_{ij} >= 0
								double[] l = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								double[] u = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								l[i*envTestN+j]=1;
								u[i*envTestN+j]=-1;
								constraints_.Add(l);
								constraints_.Add(u);
								lower_.Add(0); //x_{ij} >= 0
								lower_.Add(0); //-x_{ij} >= 0
							}
							else feasible=true;
						}
						if(!feasible) //the solution cannot be solved since this variable can never be satisfied
							return null;
					}
					else if((words[0].Equals("Grasping")||words[0].Equals("Near")) && words[1].Equals(vtmp.zVariablePredicatePost[i]))
					{
						for(int j=0; j<envTestN;j++)
						{
							if(!objL[j].uniqueName.Equals("Robot"))
							{
								//add the constraint - x_{ij} >= 0 and -x_{ij} >= 0
								double[] l = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								double[] u = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								l[i*envTestN+j]=1;
								u[i*envTestN+j]=-1;
								constraints_.Add(l);
								constraints_.Add(u);
								lower_.Add(0); //x_{ij} >= 0
								lower_.Add(0); //-x_{ij} >= 0
							}
						}
					}
					else if(words[0].Equals("Grasping") && words[2].Equals(vtmp.zVariablePredicatePost[i]))
					{
						for(int j=0; j<envTestN;j++)
						{
							if(!objL[j].affordances_.Contains("IsGraspable"))
							{
								//add the constraint - x_{ij} >= 0 and -x_{ij} >= 0
								double[] l = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								double[] u = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								l[i*envTestN+j]=1;
								u[i*envTestN+j]=-1;
								constraints_.Add(l);
								constraints_.Add(u);
								lower_.Add(0); //x_{ij} >= 0
								lower_.Add(0); //-x_{ij} >= 0
							}
						}
					}
					else if((words[0].Equals("On")||words[0].Equals("In")) && words[1].Equals(vtmp.zVariablePredicatePost[i]))
					{
						for(int j=0; j<envTestN;j++)
						{
							if(!objL[j].affordances_.Contains("IsGraspable"))
							{
								//add the constraint - x_{ij} >= 0 and -x_{ij} >= 0
								double[] l = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								double[] u = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								l[i*envTestN+j]=1;
								u[i*envTestN+j]=-1;
								constraints_.Add(l);
								constraints_.Add(u);
								lower_.Add(0); //x_{ij} >= 0
								lower_.Add(0); //-x_{ij} >= 0
							}
						}
					}
					else if(words[0].Equals("On") && words[2].Equals(vtmp.zVariablePredicatePost[i]))
					{
						for(int j=0; j<envTestN;j++)
						{
							if(!objL[j].affordances_.Contains("IsPlaceableOn"))
							{
								//add the constraint - x_{ij} >= 0 and -x_{ij} >= 0
								double[] l = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								double[] u = Enumerable.Repeat(0.0,z*envTestN).ToArray();
								l[i*envTestN+j]=1;
								u[i*envTestN+j]=-1;
								constraints_.Add(l);
								constraints_.Add(u);
								lower_.Add(0); //x_{ij} >= 0
								lower_.Add(0); //-x_{ij} >= 0
							}
						}
					}
				}
			}

			/* Adding restrictions that different variables cannot take the same value
             * there is no reason why it should always be the case but it looks like it
             * happens in most of the cases 
			 * Constraints 4: \forall_i \ne j,k  - x_ik - x_jk >= -1 */

			for(int i=0; i<z;i++)
			{
				for(int j=i+1; j<z;j++)
				{
					for(int k=0; k<envTestN;k++)
					{
						double[] l = Enumerable.Repeat(0.0,z*envTestN).ToArray();
						l[i*envTestN+k]=-1;
						l[j*envTestN+k]=-1;
						constraints_.Add(l); //- x_ik - x_jk >= -1
						lower_.Add(-1); //- x_ik - x_jk >= -1
					}
				}
			}
			#endregion

			#region solving_QP
			QPSolver qps = new QPSolver(a, b, constraints_, lower_);
			Tuple<double[], double> res = qps.solve();

			double[] map = res.Item1;
			//this.inf.lg.writeToFile("Mapping map "+String.Join(", ",map));
			#endregion

			#region discretize
			int[] map_ = new int[z];
			/* Discretization algorithm - for every i let j_ = argmax_j x_ij 
             * make x_ij\_ = 1 and for all j make x_ij = 0*/
			double discreteScore = 0;
			for (int i = 0; i < z; i++)
			{
				int j_ = 0;
				for (int j = 1; j < envTestN; j++)
				{
					if (map[i * envTestN + j] > map[i * envTestN + j_])
						j_ = j;
				}
				map_[i] = j_;
				discreteScore =  discreteScore + a[i*envTestN+j_];
			}
			discreteScore = discreteScore/(z + Constants.epsilon);
			qps.destroy();

			//if two variables get mapped to same object then write the xij
			for(int i=0; i<z;i++)
			{
				for(int k=i+1; k<z;k++)
				{
					if(map_[i]==map_[k])
					{
						int j = map_[i];
						this.inf.lg.writeToFile("Variable "+i+", "+k+" mapped to "+envTest.objects[j].uniqueName+" with cost "+map[i * envTestN + j]+" and "+map[k * envTestN + j]);
					}
				}
			}

			//DEBUG Code
			for(int i=0; i<vtmp.zVariablePredicatePost.Count(); i++)
			{
				this.inf.lg.writeToFile("variable "+i+" was "+vtmp.env_.objects[vtmp.xiOrigMappingPredicatePost[i]].uniqueName+"\n");
				if(envTest.objects[map_[i]].uniqueName.Equals("Glass_1"))
				{
					this.inf.lg.writeToFile("Okay so its a glass\n Lets try a mug\n");
					int mug = envTest.objects.FindIndex(x=>x.uniqueName.Equals("Mug_1"));
					int tmp = map_[i];
				    if(mug!=-1)
					{
						this.inf.lg.writeToFile("Mug found");
						map_[i]=mug;
						double discreteScore1 = 0;
						for (int l = 0; l < z; l++)
							discreteScore1 =  discreteScore1 + a[i*envTestN+map_[l]];
						discreteScore1 = discreteScore1/(z + Constants.epsilon);
						this.inf.lg.writeToFile("If I used mug, I would have got "+discreteScore1+" compared to "+discreteScore);
					}
					map_[i]=tmp;
				}
			}


			#endregion

			//Store in Cache
			if (Constants.cacheWriteQP) 
			{
				this.inf.cacheQP.WriteLine ("<pt><map>" + Global.arrayToString (map_) + "</map><score>" + res.Item2 + "</score></pt>");
				this.inf.cacheQP.Flush ();
			}

			String log = "";
			for (int i=0; i<z; i++) 
			{//variable z
				log = log + "variable " + i + " maps to " + envTest.objects [map_ [i]].uniqueName + " score EE: "
					+ weights ["w_ee"] * c1 [i * envTestN + map_ [i]] + " LE: " + weights ["w_le"] * c3 [map_ [i]] + " LL: " + weights ["w_ll"] * c4 [i * envTestN + map_[i]]+"<br/>";
			}
			return new Tuple<int[], double, String> (map_, discreteScore/*res.Item2 / (z * envTestN + Constants.epsilon)*/, log);
		}

		public static Dictionary<String,Double> getMappingFeatures(LexicalEntry vtmp, Clause clTest, Environment envTest, 
		                                                           Inference inf, int[] map, List<Instruction> instPrev)
		{
			/* Function Description: Given mapping, compute the mapping features:
			 * W_LE: 
             * W_EE
             * W_LL */

			int lngTestN = clTest.lngObj.Count(), lngTrainN = vtmp.cls_.lngObj.Count(),
			envTestN = envTest.numObjects(),  envTrainN = vtmp.env_.numObjects(), z = vtmp.numVariables(true);

			Dictionary<String,double> dict = new Dictionary<string, double> ();
			dict.Add ("le", 0); dict.Add ("ee", 0); dict.Add ("ll", 0);
			dict.Add ("sl", 0); dict.Add ("se", 0);

			if (z == 0)
				return dict;

			double[,] eeCorrMatrix = vtmp.env_.getEnvCorrMatrix(envTest); //E-E correlation Matrix. Its in general asymmteric
			double[,] llCorrMatrix = vtmp.cls_.getLngCorrMatrix(clTest, inf.sensim);  //L-L correlation Matrix. Its Symmetric
			double[,] leTestCorrMatrix = envTest.getLECorrMatrix(clTest, instPrev, inf.sensim, inf.ftr),
			leTrainCorrMatrix = vtmp.env_.getLECorrMatrix(vtmp.cls_, vtmp.instOld, inf.sensim, inf.ftr); //L-E correlation Matrix.

			//Computing EE
			for (int i = 0; i < z; i++)
				dict["ee"] = dict["ee"] + eeCorrMatrix[vtmp.getOrigObject(i,true), map[i]];

			//Computing LE
			for (int i = 0; i < z; i++) 
			{
				double val = 0;
				for (int k = 0; k < lngTestN; k++)
					val = Math.Max (val, leTestCorrMatrix [k, map[i]]);
				dict ["le"] = dict ["le"] + val;
			}

			//Computing LL
			for (int i = 0; i < z; i++)
			{
				double val = 0;
				for (int p = 0; p < lngTrainN; p++)
				{
					for (int q = 0; q < lngTestN; q++)
					{
						double newSample = leTrainCorrMatrix[p, vtmp.getOrigObject(i,true)] * llCorrMatrix[p, q] * leTestCorrMatrix[q, map[i]];
						val = Math.Max(val, newSample);
					}
				}
				dict ["ll"] = dict ["ll"] + val;
			}

			dict ["ee"] /= (z + Constants.epsilon);
			dict["le"] /= (z + Constants.epsilon);
			dict["ll"] /= (z + Constants.epsilon);

			return dict;
		}

		public int[] mappingMisra2014(LexicalEntry vtmp, Clause clTest, Environment env, double[,] leCorrMatrix, Logger lg)
		{
			/* Function Description: Misra et al. 2014 use a mapping based on three rules ---
             * 1. Relationship: If there exists x1, x2 in eq.cls and y1, y2  in clTest such that rho(x1, x2) = rho(y1,y2)
			 *    then if x1 was mapping to variable z1, x2 was mapping to variable z2; then join z1 and z2 to grounding of y1 and y2
             * 2. If there exists a variable z that originally mapped to obj; and if there exists a y
             *    in clTest that maps to obj then map z to obj
             * 3. For every other variable, map z to object with maximum EE-correlation */

			int[] variables = Enumerable.Repeat(-1,vtmp.zVariablesInst.Count()).ToArray();

			for (int x1=0; x1< vtmp.cls_.lngObj.Count(); x1++) 
			{
				int z1 = vtmp.referringVariable (x1);
				if (z1 == -1 || variables[z1] !=-1)
					continue;

				for(int x2=x1+1; x2<vtmp.cls_.lngObj.Count(); x2++)
				{
					int z2 = vtmp.referringVariable (x2);
					if (z2 == -1 || variables[z2]!=-1 || vtmp.cls_.relation[x1,x2].Equals("NONE"))
						continue;

					for (int y1=0; y1<clTest.lngObj.Count(); y1++) 
					{
						for (int y2=y1+1; y2<clTest.lngObj.Count(); y2++) 
						{
							if (vtmp.cls_.relation [x1, x2].Equals (clTest.relation [y1, y2])) 
							{
								//ground z1 and z2 to grounding of y1 and y2
								variables [z1] = Global.getGrounding (leCorrMatrix, y1);
								variables [z2] = Global.getGrounding (leCorrMatrix, y2);
							}
						}
					}
				}
			}

			for (int i=0; i<variables.Length; i++) 
			{
				if (variables [i] != -1)
					continue;
				for (int lang=0; lang<clTest.lngObj.Count(); lang++) 
				{
					int z = Global.getGrounding (leCorrMatrix, lang);
					if (Global.base_ (vtmp.env_.objects [vtmp.xiOrigMappingInst [i]].uniqueName).Equals(Global.base_ (env.objects [z].uniqueName))) 
					{
						variables [i] = z; 
						break;
					}
				}
			}

			for (int i=0; i<variables.Length; i++) 
			{
				if (variables [i] != -1)
					continue;
				int max = -1;
				double maxScore = Double.NegativeInfinity;
				for(int obj=0; obj<env.objects.Count(); obj++)
				{
					Object original = vtmp.env_.objects[vtmp.xiOrigMappingInst[i]];
					Object current = env.objects[obj];
					double score =  1 - original.findDistance(current.getState()).Item1;
					if (Global.base_ (original.uniqueName).Equals (Global.base_ (current.uniqueName)))
						score = 0.5 * score + 0.5;
					else
						score = 0.5 * score;

					if (score > maxScore) 
					{
						maxScore = score;
						max = obj;
					}
			    }
				variables[i] = max;
			}

			return variables;
		}
	}
}