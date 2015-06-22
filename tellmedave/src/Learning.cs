/* Tell Me Dave 2013-14, Robot-Language Learning Project
 * Code developed by - Dipendra Misra (dkm@cs.cornell.edu)
 * working in Cornell Personal Robotics Lab.
 * 
 * More details - http://tellmedave.cs.cornell.edu
 * This is Version 2.0 - it supports data version 1.1, 1.2, 1.3
 */

/*  Notes for future Developers - 
 *    <no - note >
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ProjectCompton
{
    class Learning// : Tester
    {
        /* Class Description : Defines tools used for learning in this project
         *  - planning to include : grid search, gradient descent, gradient descent with annealing,
         *    interface for Joachim's SVM_Struct etc.
         */

        private Tester testObj = null;
        private List<int> validation = null;
        private int evalMetric = 0;
        private List<Tuple<double[], double>> costWeights = null;  //contains list of all weights and their score, seen so far
        private int count = 0; //number of weights per iteration
        private double step;
        private int stepFineness; //greater the number, more fine it is
		private List<List<double[]>> cachedWeights = null; //fold [ method [] ]
		private List<double[,]> leTestCorrMatrices = null;
		private int leMatrixCacheIter = -1;

        public Learning(Tester testObj)
        {
            //Constructor Definition : Initializes the tester objected
            this.testObj = testObj;
            costWeights = new List<Tuple<double[], double>>();
            this.step = 10;
            this.stepFineness = 1;
			if (Constants.cacheReadWeights) 
			{
				this.cachedWeights = new List<List<double[]>> ();
				this.bootstrapWeights ();
			}
			this.leTestCorrMatrices = new List<double[,]> ();
        }

        private void gradientDescentLossComputation(object param)
        {
            /*Function Description : Compute the inferred instruction sequence for validation set
             * given the testing object tester*/

            List<List<List<double>>> evaluation = new List<List<List<double>>>(); // Method [ Metric [ Scores ] ]
            for (int i = 0; i < testObj.methods.Count(); i++)
            {
                evaluation.Add(new List<List<double>>());
				for (int mtr = 0; mtr < Metrics.numMetrics; mtr++)
                    evaluation[i].Add(new List<double>());
            }

			List<List<double[]>> score = testObj.inference(testObj.methods.Values.ToList(), validation, (List<object>)param);

            for (int mth = 0; mth < this.testObj.methods.Count(); mth++)
            {
                for (int mtr = 0; mtr < Metrics.numMetrics; mtr++)
                    evaluation[mth][mtr] = evaluation[mth][mtr].Concat(score[mth][mtr].ToList()).ToList();
            }

            double pointScore = 0;
            /* In case there are multiple algorithms are selected,
             * the gradient descent works only on the first
             * selected algorithm. */

            for (int i = 0; i < testObj.methods.Count(); i++)
            {
                if (testObj.methods.Values.ToList()[i])
                {
                    pointScore = evaluation[i][evalMetric].Average();
                    break;
                }
            }

			double[] wgt = ((Dictionary<String,Double>)((List<object>)param)[0]).Values.ToArray();
			String eval_string = Global.arrayToString (evaluation[0][evalMetric].ToArray());
			String wgt_string = Global.arrayToString (wgt);

            lock ("lock")
            {
				this.costWeights.Add(new Tuple<double[], double>(wgt, pointScore));
				//Write result to file
				System.IO.StreamWriter sw=new System.IO.StreamWriter(Constants.rootPath+"data.txt",true);
				sw.WriteLine(wgt_string+" gives \n"+eval_string+" score = "+pointScore);
				sw.Flush ();
				sw.Close ();
            }
        }

		private void bootstrapWeights()
		{
			/* Function Description: Read weight vectors from the file is - 
			 * <root>
			 *   <fold>
			 * 	 <method>double1,double2,double3,....</method>
			 *   .....
			 *   </fold>
			 *   ...
			 * </root> */

			XmlTextReader reader = new XmlTextReader (Constants.rootPath + "weights.xml");
			List<double[]> weights = null;
			while (reader.Read()) 
			{
				switch (reader.NodeType) 
				{
					case XmlNodeType.Element:
						if (reader.Name.Equals ("fold")) 
							weights = new List<double[]> ();
						break;
					case XmlNodeType.Text:
						if(reader.Value.Equals("$$"))
						{
							weights.Add (new double[0]);
							break;
						}
						String[] weight = reader.Value.Split (new char[] { ',' });
						List<double> w = new List<double> ();
						for (int i=0; i<weight.Length; i++)
							w.Add (Double.Parse (weight [i]));
						weights.Add(w.ToArray());
						break;
					case XmlNodeType.EndElement:
						if(reader.Name.Equals("fold"))
						{		
							this.cachedWeights.Add(weights.ToList());
							weights =null;
						}
						break;
					default: break;
				}
			}
			reader.Close();			  
		}

        private bool isSeen(double[] pivot)
        { 
            /* Function Description : Checks if a weight vector has been seen before.
             * Returns true/false accordingly */

            for (int i = 0; i < this.costWeights.Count(); i++)
            {
                bool same = true;
				int numFeature = pivot.Length;
                for (int j = 0; j < numFeature; j++)
                {
                    if (this.costWeights[i].Item1[j] != pivot[j])
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                    return true;
            }
            return false;
        }

        private List<double[]> returnNeighbors(double[] pivot)
        {
            /* Function Description : Takes a pivot and computes its neighborhood.
             * A weight vector x is a neighbor of pivot iff there exists i such that
             *  x[j] = pivot[j] for all j \ne i
             *  x[i] \in {pivot[j]-0.5, pivot[j]+0.5 }
             * */

            List<double[]> ngbr = new List<double[]>();
            double step = this.step;

			int numFeature = pivot.Length;
            /*for (int i = 0; i < 2 * numFeature; i++)
            {
                if (i / 2 == 6 || i / 2 == 7 || i/2 == 8)
                    continue;
                double[] pvt = pivot.ToArray();
                if (i % 2 == 0)
                    pvt[i / 2] = pvt[i / 2] + step;
                else pvt[i / 2] = pvt[i / 2] - step;

                if (!isSeen(pvt))
                {
                    this.count++;
                    ngbr.Add(pvt);
                }
            }*/
			for (int i = 0; i < numFeature; i++)
			{
				if (i == 4)
					continue;
				double[] pvt = pivot.ToArray();
				pvt[i] = pvt[i] + step;
				if (!isSeen(pvt))
				{
					this.count++;
					ngbr.Add(pvt);
				}
			}
            return ngbr;
        }

        private bool updateStep()
        {
            /* Function Description : If we end up finding maxima then we 
             * want to change the grid size to see if we can make finer searches
             * if we do change the grid size then we return true else we return false
             * in which case we call off the search */

            switch (this.stepFineness)
            {
                case 1: this.step = 5.0;
                    this.stepFineness++;
                    return true;
                case 2: this.step = 1.0;
                    this.stepFineness++;
                    return true;
                case 3: this.step = 0.1;
                    this.stepFineness++;
                    return true;
                case 4: this.step = 0.01;
                    this.stepFineness++;
                    return true;
                case 5: this.step = 10; //restore
                    this.stepFineness = 1;
                    return false;
                default: //its error
                    return false;
            }
        }

        public void writeCostToFile(System.IO.StreamWriter tw)
        {
            /* Function Description: Write the costs to file
             * iterate over the costWeights in the table and 
             * write them in the file */

            String data = "";
			int numFeature = this.costWeights [0].Item1.Length;
            for (int i = 0; i < this.costWeights.Count(); i++)
            {
                data = data + "Weight ";
                for (int j = 0; j < numFeature; j++)
                    data = data + costWeights[i].Item1[j];
                data = data + " AverageAccuracy " + costWeights[i].Item2 + "\n";
            }

            tw.WriteLine(data);
        }


		private Tuple<double[],double> groundTruthConstraintScore(String constraint, Clause iterator, Environment env, Tester tester,
		                                                          double[,] leTestCorrMatrix, List<String> grounded, List<List<String>> plurality, 
		                                                          Dictionary<String, Double> weights)
		{
			/* Function Description: Returns the features and cost of the ground-truth constraint*/

			VerbProgram v = tester.lexicon.Find(veil_ => veil_.getName().Equals(iterator.verb.getName(), StringComparison.OrdinalIgnoreCase));
			Dictionary<String, double> features = tester.inf.ftr.getNullDictionary (6);

			/* Compute Feature Vector
                 * - Given envTest, iterator, set of predicates {q} and resultant instruction sequence {i}  
                 *   1. Mapping Cost (which takes into account 5 features - w_ee, w_le, w_ll, w_sl, w_se)
                 *   2. Sentence Similarity 
                 *   3. Language Recall 
                 *   4. Avg. Frequency of predicate skeletal {q}
                 *   5. Total Frequency of predicate {q,e}  
				 *   6. End State Feature 
			     *   7. Bias */

			double le = 0, leRecall = 0;

			List<String> objectCover = Global.getObjects(constraint, env);
			foreach(String objName in objectCover)
			{
				int j = env.objects.FindIndex(x => x.uniqueName.Equals(objName));
				double lePerObject = 0;
				for(int i=0; i<leTestCorrMatrix.GetLength(0);i++) 
					lePerObject = Math.Max(lePerObject,leTestCorrMatrix[i,j]);
				le = le + lePerObject;
			}

			for(int lang=0; lang < leTestCorrMatrix.GetLength(0); lang++)
			{
				double max = Double.NegativeInfinity;
				foreach(String objName in objectCover)
					max = Math.Max(max, leTestCorrMatrix[lang, env.objects.FindIndex(x=>x.uniqueName.Equals(objName))]);
				if (max < 0.85)
					max = 0;
				leRecall = leRecall + max;
			}

			features["lerecall"] = leRecall/(leTestCorrMatrix.GetLength(0)+Constants.epsilon);
			features["le"] = le/ (objectCover.Count() + Constants.epsilon); //the EE cost is 0 as there is no reference
			String[] splitter = constraint.Split(new char[]{'^'});
			features ["dscp"] = 0;//splitter.Length;

			/*if (v != null)
				features ["prior"] = splitter.Aggregate (0.0, (sum,cstr_) => sum + v.fetchFrequency (cstr_)) / (splitter.Length * v.totalFrequency () + Constants.epsilon);
				//features ["prior"] = splitter.Aggregate (0.0, (sum,cstr_) => sum + tester.inf.ftr.getPredicateFreq(cstr_,1,true,null)) / (splitter.Length * v.totalFrequency () + Constants.epsilon);
			//features ["argprior"] = splitter.Aggregate(0.0,(sum,cstr_) => sum + tester.inf.ftr.getPredicateFreqOld(cstr_))/(splitter.Length*tester.inf.ftr.zPredFreqOld + Constants.epsilon);
			features ["argprior"] = splitter.Aggregate(0.0,(sum,cstr_) => sum + tester.inf.ftr.getPredicateFreq(cstr_,1,false,null))/(splitter.Length*tester.inf.ftr.getZPredicateFreq(1,false,null) + Constants.epsilon);
			*/

			features ["1prior"] = tester.inf.ftr.getPredicateFreq1 (splitter, false, null);
			features["2prior"] = tester.inf.ftr.getPredicateFreq2 (splitter, false, null);
			if (v != null) 
			{
				features ["1vprior"] = tester.inf.ftr.getPredicateFreq1 (splitter, false, v.verbName);
				features ["2vprior"] = tester.inf.ftr.getPredicateFreq2 (splitter, false, v.verbName);
			}

			features["1aprior"] = tester.inf.ftr.getPredicateFreq1 (splitter, true, null);
			features["2aprior"] = tester.inf.ftr.getPredicateFreq2 (splitter, true, null);
			if (v != null) 
			{
				features ["1vaprior"] = tester.inf.ftr.getPredicateFreq1 (splitter, true, v.verbName);
				features ["2vaprior"] = tester.inf.ftr.getPredicateFreq2 (splitter, true, v.verbName);
			}


			features ["rel"] = tester.inf.ftr.fetchRelationshipFeature (iterator, plurality, constraint).Item1;

			if (iterator.children == null || iterator.children.Count () == 0)
				features ["end"] = tester.inf.ftr.getEndStateProbability(constraint);		

			features ["bias"] = 0;//1;
			features["trans"] = 0;
			features["argtrans"] = 0;

			double totalScore = Math.Exp(weights["w_le"]*features["le"] + weights["w_lerecall"]*features["lerecall"] +
			                             weights["w_dscp"]*features["dscp"] /*+ weights["w_prior"]*features["prior"] + weights["w_argprior"]*features["argprior"] */
			                             + weights["w_1prior"]*features["1prior"] + weights["w_2prior"]*features["2prior"] + + weights["w_1vprior"]*features["1vprior"] + weights["w_2vprior"]*features["2vprior"] 
			                             + weights["w_1aprior"]*features["1aprior"] + weights["w_2aprior"]*features["2aprior"] + + weights["w_1vaprior"]*features["1vaprior"] + weights["w_2vaprior"]*features["2vaprior"] 
			                             + weights["w_rel"]*features["rel"] + weights["w_end"]*features["end"] + weights["w_bias"]*features["bias"] + 
			                             weights["w_trans"]*features["trans"]+ weights["w_argtrans"]*features["argtrans"]);

			return new Tuple<double[], double>(features.Values.ToArray(),totalScore);
		}

		private int[] findMap(String constraint, LexicalEntry vt, Environment env)
		{
			/* Function Description: Given a veil template such as (Near x y) And (Grasping x z)
			 * and the ground-truth sequence; (Near robot cup) And (Grasping robot mug); it returns
			 * the map x-> robot; y-> cup; z-> mug */

			List<String> parametric = vt.predicatesPost;
			bool exist1 = parametric.Exists (x => x.Length == 0);
			List<String> groundTruth = constraint.Split (new char[] { '^' }).ToList ();
			bool exist2 = groundTruth.Exists (x => x.Length == 0);
			if (exist1 || exist2) 
			{
				;
			}
			List<String> objectCover = Global.getObjects(constraint, env);
			if (groundTruth.Count () != parametric.Count () || objectCover.Count() != vt.zVariablePredicatePost.Count())
				return null;

			/* Apparent Sub-optimal Algorithm that I am going to use: 
             * - each variable takes different object
             *   - space of all  objects that must be consumed
             *   - for each variable z find set of plausible objects S_z
             *   -  in each iteration, if S_z = 0 for some z return null
			 *   -  else for |S_z| with smallest cardinality, pick the first possibility and initialize z to it */

			int[] map = Enumerable.Repeat(-1, vt.zVariablePredicatePost.Count()).ToArray();

			#region find_map
			for (int z=0; z<vt.zVariablePredicatePost.Count(); z++)  //iterations cannot be more the number of variables
			{
				#region define_variable_space
				List<Tuple<int,List<String>>> variableSpace = new List<Tuple<int,List<String>>> ();
				foreach(String predicate in parametric)
				{
					Tuple<bool, string> paramPred = Global.getAtomic(predicate);
					String[] wordsParamPred = paramPred.Item2.Split(new char[]{' '});

					foreach(String gtruth in groundTruth)
					{
						Tuple<bool, string> groundPred = Global.getAtomic(gtruth);
						String[] wordsGroundPred = groundPred.Item2.Split(new char[]{' '});
						if( groundPred.Item1!=paramPred.Item1 || !wordsParamPred[0].Equals(wordsGroundPred[0]) 
						   || wordsParamPred[0].Equals("state") && !wordsParamPred[2].Equals(wordsGroundPred[2]) )
							continue;

						int z1 = vt.zVariablePredicatePost.FindIndex(x=>x.Equals(wordsParamPred[1]));
						if(map[z1] == -1)
						{
							int index = variableSpace.FindIndex(x=>x.Item1 == z1);
							if(index == -1)
								variableSpace.Add(new Tuple<int, List<string>>(z1, new List<String>(){wordsGroundPred[1]}));
							else variableSpace[index].Item2.Add(wordsGroundPred[1]);
						}
						if(!wordsParamPred[0].Equals("state"))
						{
							int z2 = vt.zVariablePredicatePost.FindIndex(x=>x.Equals(wordsParamPred[2]));
							if(map[z2] == -1)
							{
								int index = variableSpace.FindIndex(x=>x.Item1 == z2);
								if(index == -1)
									variableSpace.Add(new Tuple<int, List<string>>(z2, new List<String>(){wordsGroundPred[2]}));
								else variableSpace[index].Item2.Add(wordsGroundPred[2]);
							}
						}
					}
				}
				#endregion

				if(variableSpace.Count()==0)
					return null;

				int min = variableSpace.Min(x=> x.Item2.Count());

				int minIndex = variableSpace.FindIndex(x=>x.Item2.Count()==min);
				map[variableSpace[minIndex].Item1] = env.objects.FindIndex(x=>x.uniqueName.Equals(variableSpace[minIndex].Item2[0]));
				variableSpace[minIndex].Item2.RemoveAt(0);
			}
			#endregion

			return map;
		}


		private double[] gradrss2015Learning(Clause cls, Environment env, List<Instruction> inst, Tester tester,
		                                    List<Instruction> instPrev, Dictionary<String, Double> weights, List<object> param, System.IO.StreamWriter sw)
		{
			/* Function Description: Computes gradient of the pseudo-log likelihood. */
			double alpha = 0; 
			double[] grad= Enumerable.Repeat(0.0, Features.featureNames[6].Count()).ToArray();

			List<String> groundTruthConstraint = tester.sml.executeList(inst,env).difference(env);
			if (groundTruthConstraint.Count () == 0)
				return grad;

			List<Object> objList = env.objects; 
			List<List<String>> plurality = new List<List<String>> ();

			double[,] leTestCorrMatrix = null; 
			if (this.leMatrixCacheIter == -1) 
			{
				leTestCorrMatrix = env.getLECorrMatrix (cls, new List<Instruction> (), tester.inf.sensim, tester.inf.ftr);
				this.leTestCorrMatrices.Add (leTestCorrMatrix);
			}
			else
			{
				leTestCorrMatrix = this.leTestCorrMatrices[this.leMatrixCacheIter];
				this.leMatrixCacheIter++;
			}

			List<String> grounded = new List<String> ();

			for (int i=0; i<leTestCorrMatrix.GetLength(0); i++)
			{
				String mainNoun = cls.lngObj [i].getName ();
				int maxIndex = 0;
				List<String> plurality_ = new List<string> () { objList[0].uniqueName };
				for (int j=1; j<objList.Count(); j++) 
				{
					if (leTestCorrMatrix [i, j] > leTestCorrMatrix [i, maxIndex]) 
					{
						maxIndex = j;
						plurality_.Clear ();
						plurality_.Add (objList[j].uniqueName);
					}
					else if (leTestCorrMatrix [i, j] == leTestCorrMatrix [i, maxIndex]) 
						plurality_.Add (objList[j].uniqueName);
				}
				if (leTestCorrMatrix [i, maxIndex] <= 0.8) //if the max has low confidence then ignore the list
					plurality_.Clear ();
				//if the mainNoun is not in plural form then make a choice
				if (!mainNoun.EndsWith ("s") && plurality_.Count() > 1) //hack for plural
					plurality_.RemoveRange (1, plurality_.Count()-1);
				grounded = grounded.Union (plurality_).ToList();
				plurality.Add (plurality_);
			}

			#region apply_inference_steps
			List<Tuple<double[],double>> derivations = new List<Tuple<double[],double>>();//pair of features and the weights
			VerbProgram v = tester.lexicon.Find(veil_ => veil_.getName().Equals(cls.verb.getName(), StringComparison.OrdinalIgnoreCase));
			List<LexicalEntry> vtList = null;
			if(v == null)
				vtList = new List<LexicalEntry>();
			else vtList = v.getProgram();

			foreach (LexicalEntry vt in vtList)
			{
				if(!(bool)param[0])
					continue;
				//Find the mapping \xi for each one of them 
				int[] map = this.findMap(String.Join("^",groundTruthConstraint), vt, env);
				if(map ==null)
					continue;

				Dictionary<String, double> features = tester.inf.ftr.getNullDictionary(6);
				Dictionary<String, double> mapfeatures = Mapping.getMappingFeatures(vt, cls, env, tester.inf, map, instPrev);

				for (int i = 0; i< mapfeatures.Count(); i++)
					features [mapfeatures.ElementAt (i).Key] = mapfeatures[mapfeatures.ElementAt (i).Key];

				String constraint = String.Join("^",tester.inf.instantiatePredicates(vt, map, env).Distinct().ToList());

				if(constraint.Length==0)
					continue;

				String cstr = constraint;//this.expansion(plurality, constraint);
				String[] cstrSplit = cstr.Split(new char[]{'^'});

				/* Compute Feature Vector
                 * - Given envTest, iterator, set of predicates {q} and resultant instruction sequence {i}  
                 *   1. Mapping Cost (which takes into account 5 features - w_ee, w_le, w_ll, w_sl, w_se)
                 *   2. Sentence Similarity 
                 *   3. Language Recall 
                 *   4. Avg. Frequency of predicate skeletal {q}
                 *   5. Total Frequency of predicate {q,e}  
				 *   6. End State Feature */

				double mapCost = weights["w_ee"]*features["ee"]+weights["w_le"]*features["le"]+weights["w_ll"]*features["ll"]
								+ weights["w_sl"]*features["sl"]+weights["w_se"]*features["se"];

				features["dscp"] = 0;// cstrSplit.Length;

				if(vt.cls_.sentence!=null && cls.sentence!=null)
				{
					List<String> trainWords = vt.cls_.getWords();//sentence.Split(new char[]{' '}).Select(x=>x.Trim()).ToList();
					List<String> testWords = cls.getWords();//sentence.Split(new char[]{' '}).Select(x=>x.Trim()).ToList();
					trainWords.RemoveAll(x=>x.Length==0);
					testWords.RemoveAll(x=>x.Length==0);
					Tuple<double,string> res = Global.jaccardIndex(trainWords,testWords);
					features["sensim"] = res.Item1;
				}

				List<String> objectCover = Global.getObjects(cstr, env);
				double leRecall=0;
				for(int lang=0; lang < leTestCorrMatrix.GetLength(0); lang++)
				{
					double max = Double.NegativeInfinity;
					foreach(String objName in objectCover)
						max = Math.Max(max, leTestCorrMatrix[lang, env.objects.FindIndex(x=>x.uniqueName.Equals(objName))]);
					if(max<0.85)
						max=0;
					leRecall = leRecall + max;
				}

				features["lerecall"] = leRecall/(leTestCorrMatrix.GetLength(0)+Constants.epsilon);
				/*features["prior"] = cstrSplit.Aggregate(0.0,(sum,cstr_) => sum + v.fetchFrequency(cstr_))/(cstrSplit.Length*v.totalFrequency() + Constants.epsilon);
				//features["argprior"] = cstrSplit.Aggregate(0.0,(sum,cstr_) => sum + tester.inf.ftr.getPredicateFreqOld(cstr_))/(cstrSplit.Length*tester.inf.ftr.zPredFreqOld + Constants.epsilon);
				features["argprior"] = cstrSplit.Aggregate(0.0,(sum,cstr_) => sum + tester.inf.ftr.getPredicateFreq(cstr_,1,false, null))/(cstrSplit.Length*tester.inf.ftr.getZPredicateFreq(1,false, null) + Constants.epsilon);*/

				features ["1prior"] =  tester.inf.ftr.getPredicateFreq1 (cstrSplit, false, null);
				features["2prior"] =  tester.inf.ftr.getPredicateFreq2 (cstrSplit, false, null);
				if (v != null) 
				{
					features ["1vprior"] = tester.inf.ftr.getPredicateFreq1 (cstrSplit, false, v.verbName);
					features ["2vprior"] = tester.inf.ftr.getPredicateFreq2 (cstrSplit, false, v.verbName);
				}

				features["1aprior"] =  tester.inf.ftr.getPredicateFreq1 (cstrSplit, true, null);
				features["2aprior"] =  tester.inf.ftr.getPredicateFreq2 (cstrSplit, true, null);
				if (v != null) 
				{
					features ["1vaprior"] = tester.inf.ftr.getPredicateFreq1 (cstrSplit, true, v.verbName);
					features ["2vaprior"] = tester.inf.ftr.getPredicateFreq2 (cstrSplit, true, v.verbName);
				}


				features["rel"] = tester.inf.ftr.fetchRelationshipFeature(cls, plurality, constraint).Item1;
				if (cls.children == null || cls.children.Count() == 0)
					features["end"] = tester.inf.ftr.getEndStateProbability(constraint);
				features["trans"] = 0;
				features["argtrans"] = 0;

				double totalScore = Math.Exp(mapCost + weights["w_lerecall"]*features["lerecall"] + weights["w_dscp"]*features["dscp"] 
				                             + weights["w_sensim"]*features["sensim"]+ /*weights["w_prior"]*features["prior"] + weights["w_argprior"]*features["argprior"]*/ 
				                             + weights["w_1prior"]*features["1prior"] + weights["w_2prior"]*features["2prior"] + + weights["w_1vprior"]*features["1vprior"] + weights["w_2vprior"]*features["2vprior"] 
				                             + weights["w_1aprior"]*features["1aprior"] + weights["w_2aprior"]*features["2aprior"] + + weights["w_1vaprior"]*features["1vaprior"] + weights["w_2vaprior"]*features["2vaprior"] 
				                             + weights["w_rel"]*features["rel"] + weights["w_end"]*features["end"]+ weights["w_trans"]*features["trans"]+ weights["w_argtrans"]*features["argtrans"]);

				derivations.Add(new Tuple<double[],double>(features.Values.ToArray(), totalScore));
				alpha = alpha + totalScore;
				#endregion
			}

			#region GenTemplate
			/* Find derivations that use generated template */
			if((bool)param[1]) //if true then generate templates
			{
				derivations.Add(this.groundTruthConstraintScore(String.Join("^",groundTruthConstraint), cls, env, tester, leTestCorrMatrix, grounded, plurality, weights));
				alpha = alpha + derivations.Last().Item2; //add the generated derivations score
			}
			#endregion

			/* computing the gradient
			 * (\sum_derivations feature e^{w^T\phi})/(\sum_derivations e^{w^T\phi})  */

			grad = derivations.Aggregate(grad,(acc,derivation) => acc.Zip(derivation.Item1.Select(x=>x*derivation.Item2).ToArray(), (x,y)=>x+y).ToArray());
			//sw.WriteLine ("Counter = "+Learning.counter+" Numerator " + String.Join (",", grad) + " denominator " + alpha);
			//foreach (Tuple<double[],double> derivation in derivations) 
			//	sw.WriteLine ("Derivation  Score "+derivation.Item2+" "+string.Join(",",derivation.Item1));
			//sw.WriteLine ("Total Denominator "+alpha);
			return grad.Select (x => x / (alpha+Constants.epsilon)).ToArray();
		}

		static int counter = 0;
		public List<double[]> analyticGradientDescent(List<int> train, int evalMetric, int fold)
		{
			/* Function Description: Finds closed form gradient solutions for the problem */

			if (Constants.cacheReadWeights) 
			{
				Console.WriteLine ("Fold is " + fold);
				List<double[]> c = this.cachedWeights [fold];
				foreach (double[] c_ in c)
					Console.WriteLine ("Weights "+String.Join(",",c_));
				return c;
			}

			List<double[]> weights = new List<double[]>();
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = this.testObj.prs.returnAllData ();
			List<List<Tuple<int, int>>> alignment = this.testObj.prs.returnAllAlignment ();

			for (int mth = 0; mth < this.testObj.methods.Count(); mth++) 
			{
				if (!this.testObj.methods.ElementAt (mth).Value || Features.featureNames[mth].Count() == 0)
				{
					weights.Add(Enumerable.Repeat(0.0, Features.featureNames[mth].Count()).ToArray());
					continue;
				}

				if (mth == 0) 
				{
					weights.Add(this.numericalGradientDescent (train, 0, fold)); //analytic gradient
					continue;
				}

				List<object> param = null;
				if (mth == 5)      //Generated Templates Only
					param = new List<object> () { (object)false, (object)true, (object)false};
				else if (mth == 6) //Generated Templates + Storing 
					param = new List<object> () { (object)true, (object)false, (object)true};
				else if (mth == 7) //VEIL Templates
					param = new List<object> () { (object)true, (object)true, (object)false};
				else if (mth == 8) //VEIL+Generated
					param = new List<object> () { (object)true, (object)true, (object)false};
				else if (mth == 9) //VEIL+Generated+Storing
					param = new List<object> () { (object)true, (object)true, (object)true};	


				#region gradient_descent_algorithm_for_main_model
				int maxinter = 2;
				double learningRate = 0.005, regularizer = 0;//Math.Log(2.8);
				double [] weight = //new double[21]{ 9.09443482245257E-05,6.5600925151066E-06,0.00023913395717792,4.40828544130447E-05,0.00122119514820791,7.42561497298672E-05,0.00144019668363856,0.000108549831716089,0,0,0.00511791519766496,0.000385609743193129,0,0,0.00740168257949265,0.00133940084513705,0.000250469146473068,0.0083253536880869,0,0,0};
					//new double[15]{0.00126810957920883,9.09443427657785E-05,0,0,0.00511791544160663,0.000385611813672254,0,0,0.00740168217600723,0.00133940039043196,0.000250469975184572,0.0083253532592022,0,0,0};
				Enumerable.Repeat(0.0, Features.featureNames[mth].Count()).ToArray();
				//weights.Add(weight);
				//continue;

				if(train.Count()==0) //to handle edge case
				{
					weights.Add(weight);
					continue;
				}

				//Create the validation-set and train-only dataset
				List<int> trainOnly = new List<int>();
				validation = new List<int>();

				int sizeTrain = (int)(9*train.Count()/10);
				for (int point = 0; point < train.Count(); point++)
				{
					if (point < sizeTrain)
						trainOnly.Add(train[point]);
					else validation.Add(train[point]);
				}
			
				this.testObj.constructDataStructure(trainOnly);  //Initialize Features and Generate VEIL templates
			
				Dictionary<String,Double> dict = new Dictionary<String,Double> ();
				for (int j=0; j<Features.featureNames[mth].Count(); j++)
					dict.Add (Features.featureNames [mth] [j], weight[j]);

				System.IO.StreamWriter sw= new System.IO.StreamWriter(Constants.rootPath+"weights_algorithm_"+mth+".txt");
				sw.WriteLine("Iteration, L2 norm of the gradient");

				for(int iter=0; iter<maxinter; iter++) //Perform interations of gradient descent
				{
					Learning.counter = 1;
					if(iter==0)
						this.leMatrixCacheIter = -1; //store leMatrices (costly to compute) in cache for the first iteration
					else this.leMatrixCacheIter = 0; //read leMatices from cache

					double [] gradient = Enumerable.Repeat(0.0, Features.featureNames[mth].Count()).ToArray();
					for (int j=0; j<Features.featureNames[mth].Count(); j++)
						dict[Features.featureNames [mth] [j]] = weight[j];

					foreach(int validationpt in this.validation)
					{
						for(int align = 0; align < alignment[validationpt].Count(); align++)
						{
							Tuple<int,int> align_ = alignment[validationpt][align];

							if(align >= parsedSentence[validationpt].Item4.Count()) //More alignments than clauses :-/
								continue;

							Clause cls = parsedSentence[validationpt].Item4[align];

							if(align_.Item1 > align_.Item2 || cls.isCondition)
								continue;

							Environment env = testObj.listOfAllEnv[validationpt][align_.Item1];
							List<Instruction> inst = parsedSentence[validationpt].Item3.GetRange(align_.Item1,
							                                                              align_.Item2-align_.Item1+1);
							List<Instruction> instPrev = parsedSentence[validationpt].Item3.GetRange(0, align_.Item1);
							double[] newGrad = this.gradrss2015Learning(cls, env, inst, testObj, instPrev, dict, param, sw);
							//sw.WriteLine(Learning.counter + "New Gradient "+String.Join(", ",newGrad)+"\n\n\n");
							for(int wt=0; wt< newGrad.Length;wt++)
								gradient[wt] = gradient[wt] + newGrad[wt];
							//sw.WriteLine(Learning.counter + "Cummulative Gradient "+String.Join(", ",newGrad));
							Learning.counter++;
						}
					}

					#region debug_only_compute_norm_of_gradient
					double norm = 0;
					for(int dim =0; dim<gradient.Length; dim++)
						norm = norm + gradient[dim]*gradient[dim];
					norm = Math.Sqrt(norm)/(double)this.validation.Count();
					//sw.WriteLine("Iteration = "+iter+","+norm);
					#endregion
				
					for(int wt=0; wt<weight.Length; wt++)
						weight[wt] = (1-regularizer)*weight[wt] + learningRate/(validation.Count()+Constants.epsilon)*gradient[wt];

					double norm1 = 0;
					for(int dim =0; dim<weight.Length; dim++)
						norm1 = norm1 + weight[dim]*weight[dim];
					norm1 = Math.Sqrt(norm1);

					sw.WriteLine("Iteration = "+iter+"Norm of weight"+norm1+" Norm of update "+norm+"   Weight Vector "
					             +String.Join(",",weight)+"\n\n\n\n----------------------------------\n\n\n");
				}

				sw.Flush();
				sw.Close();

				Console.WriteLine("Learned Weight for Method: "+mth+" = "+String.Join(", ", weight));
				this.testObj.lg.writeToFile("<span style='color:green'> Method: "+mth+" Learned Weight Vector = "+Global.arrayToString(weight)+"</span>");
				weights.Add(weight);
				#endregion

				this.testObj.destroyer ();
			}
			return weights;
		}


        public double[] numericalGradientDescent(List<int> train, int evalMetric, int fold)
        {
            /* Function Description : Returns weights learned using gradient descent algorithm.
			 *
             * Input : Datas [train,validation]
             *         evalMetric 0 : LV, 1 : uWEED, 2: WEED
             *         
             * Algorithm in detail - 
             *             1.  it creates VEIL library for the training corpus
             *             2.  Maintains a pivot weight  
             *             3.  find the neighbors of the pivot weight
             *             4.     in parallel for each neighbor of the pivot weight     
             *             5.        using the VEIL and inference find the instruction sequence for each point in validation
             *             6.        compute the average loss function over the entire validation
             *             7.  if pivot weight is better then return pivot and stop
             *             8.  else continue with the best neigbhor */

			if (Constants.cacheReadWeights) 
			{
				//this.cacheIter++;
				return this.cachedWeights [fold][0];
			}

			//due to time-shortage, I am using the weights from Misra et al. 2014 paper -- this worked well but in general
			//Misra et al. baseline can go even higher.
			return new double[12] { 40, 2, -0.01, -10, 0, 5, 40, 130, 30, 0, 100, 0 }; 

			Constants.disablelog = true;
			testObj.lg.setLowPriority ();
			List<int> trainOnly = new List<int>();
			validation = new List<int>();
            
            //Create the validation-set and train-only dataset
            int sizeTrain = (int)(9*train.Count()/10);
            for (int i = 0; i < train.Count(); i++)
            {
                if (i < sizeTrain)
                    trainOnly.Add(train[i]);
                else validation.Add(train[i]);
            }

			Console.WriteLine ("Validation Dataset has size = "+validation.Count());
			double[] pivot = Enumerable.Repeat (0.0,Features.featureNames[0].Count()).ToArray();//pivot weight
            double pivotScore = Double.NegativeInfinity; //assuming the defualt pivot is not a local minima, its okay to do so

            int steps = 0, maxStep = 25;
            this.testObj.constructDataStructure(trainOnly); // VEIL Dataset creation phase

            while (steps < maxStep) //we dont want the algorithm to run forever even if it does not find optima
            {
                List<double[]> grid = this.returnNeighbors(pivot);  //Use pivot to define a local neighborhood of unseen points
                List<Thread> threads = new List<Thread>();
				Console.WriteLine ("Step  = " + steps + " of atmost MaxStep = " + maxStep);
                int batchSize = 1;
                int numBatches = grid.Count()/batchSize;
                if (grid.Count() % batchSize != 0)
                    numBatches++;
                
                for (int batch = 0; batch < numBatches; batch++) //execute them in batches
                {
					Console.WriteLine ("Working on batch out of " + batch + " out of " + numBatches);
                    for (int i=0; i < batchSize; i++) // executed in parallel
                    {
                        if(batch*batchSize + i >= grid.Count())
                            break;

						List<double[]> weightParam = new List<double[]> (){grid [batch * batchSize + i]};//only working on the first method
						for(int mth=1; mth < Features.featureNames.Count(); mth++)
							weightParam.Add(Enumerable.Repeat(0.0, Features.featureNames[mth].Count()).ToArray());

						List<object> param = testObj.inf.initDict (weightParam);
						Thread singleWt = new Thread(this.gradientDescentLossComputation);
                        singleWt.Start((object)param);
                        threads.Add(singleWt);
                    }

                    foreach (var thread in threads)
                    {
                        thread.Join();
                    }
                }
               
                //find the new pivot
                int iter = -1;
                for (int i = 0; i < count; i++)
                {
                    if (this.costWeights[this.costWeights.Count() - i - 1].Item2 > pivotScore)
                    {
                        pivotScore = this.costWeights[this.costWeights.Count() - i - 1].Item2;
                        iter = i;
                    }
                }

                count = 0;
                if (iter == -1) //the pivot remains same
                {
                    if (!this.updateStep()) //keep making the step-size(a.k.a. grid-size) finer until its too fine that we give up 
                    {
                        this.testObj.destroyer();
						this.testObj.lg.setHighPriority (); //print the weights
						foreach(Tuple<double[], double> w in this.costWeights)
							this.testObj.lg.writeToFile ("<span style='color:blue;'> Weight = "+Global.arrayToString(w.Item1)+" = "+w.Item2.ToString()+"</span><br/>");
						break;//return pivot;  //pvt is the local optima
                    }
                }
                else
                    pivot = costWeights[this.costWeights.Count() - iter - 1].Item1;
				Console.WriteLine ("Weight "+Global.arrayToString(pivot)+" Step = "+this.step+" Score "+pivotScore);
                steps++;
            }

			//Run the algorithm once on the validation set and see the results
			this.testObj.lg.setHighPriority ();
            this.testObj.destroyer();

			foreach(Tuple<double[], double> w in this.costWeights)//print the weights
				this.testObj.lg.writeToFile ("<span style='color:blue;'> Weight = "+Global.arrayToString(w.Item1)+" = "+w.Item2.ToString()+"</span><br/>");

			/*List<double[]> weights = new List<double[]> ();
			for (int i=0; i<this.testObj.methods.Count(); i++) 
			{
				if (i < 4)
					weights.Add (new double[0]);
				if (4 <= i)
					weights.Add (pivot);
			}*/
			Console.WriteLine ("Weight "+String.Join(", ",pivot));
			Constants.disablelog = false;
			return pivot;
		}
	}
}
