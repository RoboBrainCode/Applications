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
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;
using WordsMatching;

namespace ProjectCompton
{
    class Inference
    {
        /* Class Description: The aim of this class is to provide several inference strategies
         * for the main algorithm. This also includes interface for inference moves */
        public Logger lg = null;
        private List<Environment> envList = null;
		public Features ftr { get; private set;}
        private Parser obj = null;  // Parser object
        private SymbolicPlanner symp = null;
        public Simulator sml = null;
        //private Learning lrn = null;
		public SentenceSimilarity sensim = null;
        public System.IO.StreamWriter cacheQP = null;
        private XmlTextReader readQP = null;

		private Mapping map = null;
		public String oldvalue = null;
		private double[,] oldLECorrMatrix = null;
		private int k = 1;

        public Inference(Logger lg, Simulator sml, SymbolicPlanner symp, List<VerbProgram> veil, List<Environment> envList, Parser obj, Features ftr)
        {
            this.lg = lg;
            this.envList = envList;
            this.ftr = ftr;
            this.obj = obj;
			this.symp = symp;
            this.sml = sml;
			this.map = new Mapping (this);
			this.sensim = new SentenceSimilarity ();
            if (Constants.cacheReadQP)
                this.readQP = new XmlTextReader(Constants.rootPath + "Log/QP.xml");
            if(Constants.cacheWriteQP)
            {
                this.cacheQP = new System.IO.StreamWriter(Constants.rootPath + "Log/QP.xml");
                this.cacheQP.WriteLine("<QP>");
            }
			if (Constants.cacheBootStrapSentenceSim)
				this.sensim.bootStrapCache ();
			Console.WriteLine ("Sentence Simiarity Cache Hit " + this.sensim.cacheHit / (this.sensim.cacheHit + this.sensim.cacheMiss + Constants.epsilon));
			this.lg.writeToFile ("Sentence Simiarity Cache Hit " + this.sensim.cacheHit / (this.sensim.cacheHit + this.sensim.cacheMiss + Constants.epsilon));
        }

		public List<object> initDict(List<double[]> weights)
		{
			//Function Description: Returns the dictionary bootstrapped with weights
			if (weights.Count () != Features.featureNames.Count ())
				throw new ApplicationException ("Number of weight vector parameters are not same as ");
			List<object> parameter = new List<object>();
			for(int i=0; i<weights.Count();i++)
			{
				Dictionary<String,Double> dict = new Dictionary<String,Double> ();
				for (int j=0; j<Features.featureNames[i].Count(); j++)
					dict.Add (Features.featureNames [i] [j], weights [i][j]);
				parameter.Add ((object)dict);
			}
			return parameter;
		}

        public void close()
        {
            //Function Description: Performs action as if this constructor is never going to be called again
            if (Constants.cacheWriteQP)
            {
                this.cacheQP.WriteLine("</QP>");
                this.cacheQP.Flush();
                this.cacheQP.Close();
            }
           if(Constants.cacheReadQP)
				this.readQP.Close();
			this.sensim.storeCache ();
        }

		public String expansion(List<List<String>> samePlurality, String cstr)
		{
			/* Function Description: Takes a constraint such as (On Pillow_1 ArmChair_1); 
			 * and given that there are samePlurality of Pillow_1; the algorithm expands 
			 * it to (On Pillow_1 ArmChair_1); (On Pillow_2 ArmChair_1);..(On Pillow_i ArmChair_1) 
			 * The algorithm : samePlurality(x) and Predicate(x,y) => and_i Predicate(x_i, y) */

			String[] cstrSplit = cstr.Split (new char[] {'^'});
			List<String> newConstraints = cstrSplit.ToList ();	
			foreach (List<String> same_ in samePlurality) 
			{
				if (same_.Count () == 1 || same_.Count() > 5 )
					continue;
				List<String> newlyAdded = new List<String> ();

				foreach(String pred in newConstraints)
				{
					//check if pred contains any object from same_
					int find = same_.FindIndex (obj => pred.Contains (obj));
					if(find!=-1)
					{
						for(int i=0; i< same_.Count();i++)
						{
							if (i == find)
								continue;
							Tuple<bool,string> atom = Global.getAtomic (pred);
							String[] words = atom.Item2.Split (new char[] {' '});
							for(int j=0; j<words.Length; j++)
							{
								if(words[j].Equals(same_[find]))
									words[j]=  same_[i];
							}
							String newpred = "("+string.Join(" ",words)+")";
							if (atom.Item1)
								newpred = "(not " + newpred + ")";
							newlyAdded.Add(newpred);
						}
					}
				}

				newConstraints = newConstraints.Union (newlyAdded).ToList();
			}
			return String.Join ("^", newConstraints);
		}
		
        public List<Instruction> instantiation(LexicalEntry eq, int[] mapping, Environment envTest)
        {
            /* Function Description: Apply the mapping on the template and return
             * the instantiated instruction sequence */

            List<Instruction> insts = eq.inst_.ToList();
            List<Instruction> instant = new List<Instruction>();
            List<String> variables = eq.zVariablesInst;
			List<Object> objList = envTest.objects;

		    foreach (Instruction inst in insts)
            {
                List<String> description = inst.getArguments();
                List<String> newDescription = new List<String>();

                foreach (String dscp in description)
                {
                    bool added = false;
                    for (int i = 0; i < variables.Count(); i++)
                    {
                        if (dscp.Equals(variables[i]))
                        {
                            added = true;
                            newDescription.Add(objList[mapping[i]].uniqueName);
                            break;
                        }
                    }
                    if (!added)
                        newDescription.Add(dscp); //keep it same
                }
                Instruction instNew = new Instruction(inst.getControllerFunction(), newDescription);
                instant.Add(instNew);
            }

            return instant;
        }

		public List<String> instantiatePredicates(LexicalEntry vt, int[] mapping, Environment envTest)
		{
			/* Function Description: Takes a set of predicates [predicate] and mapping of the parameters to envTest
			 * and returns the instantiated predicates */
			List<String> instantiated = new List<String> ();
			List<Object> objList = envTest.objects;
			List<String> variables = vt.zVariablePredicatePost;

			foreach (String predicate in vt.predicatesPost) 
			{
				Tuple<bool,string> predicate_ = Global.getAtomic(predicate);
				String[] words = predicate_.Item2.Split (new char[]{' '});
				for(int i=0; i<words.Length;i++)
				{
					//Console.WriteLine("Word is "+words[i]);
					for (int v = 0; v < variables.Count(); v++)
					{
						if (words [i].Equals (variables [v])) 
						{
							//Console.WriteLine ("Matched and mapped using "+objList [mapping [v]].uniqueName);
							words [i] = objList [mapping [v]].uniqueName;
						}
					}
				}
				String instantiatedPred = "("+string.Join (" ", words)+")";
				if (predicate_.Item1)
					instantiatedPred = "(not " + instantiatedPred + ")";
				instantiated.Add (instantiatedPred);
			}
			return instantiated;
		}

        public Tuple<int[], Double> fetchMapFromCache()
        {
            /* Function Description: Cache has data in the format - 
               <QP>
             *  <pt><map>a1 a2 a3 ... </map><score>...</score></pt>
             *  .....
             * </QP>
             **/
            int[] map = null;
            while (this.readQP.Read())
            {
                switch (this.readQP.NodeType)
                {
                    case XmlNodeType.Text: //Display the text in each element.
                        if (map == null)
                            map = Global.stringToArray(this.readQP.Value);
                        else return new Tuple<int[], double>(map,Double.Parse(this.readQP.Value));
                        break;
                }
            }

            throw new ApplicationException("Asking from Cache where None exists");
        }

        public List<Tuple<Object, String, String, int>> createTableOfStates(Clause cl, Environment env, List<LexicalEntry> programs)
        {
            /* Function Description : Given a clause cl and list of program instances, 
             * corresponding to some verb. This function returns a table of the form 
             *    [Object : Object , State : String, Value : Double] */

            List<Tuple<Object, String, String, int>> stateTable = new List<Tuple<Object, String, String, int>>();
			return stateTable;

            Environment[] endEnvList = new Environment[programs.Count()];

            for (int i = 0; i < programs.Count(); i++)
            {
                //finds last environment for every env in the list
				endEnvList[i] = this.sml.executeList(programs[i].instOld, programs [i].env_);
            }

            for (int i = 0; i < programs.Count(); i++)
            {
				List<Tuple<String, String>> matchedObjects = null;//this.mappingMisra2014(programs[i], cl);  //first item is from program
                foreach (Tuple<String, String> tmp in matchedObjects)
                {
                    //tmp.Item2 has to be from cl to continue
                    if (cl.ifExists(tmp.Item2))
                    {
                        Object obj1 = env.findObject(tmp.Item2);
                        //search for tmp.Item1 in endEnvList[i]
                        Object obj2 = endEnvList[i].findObject(tmp.Item1);
                        /* iterate over states of obj and store it in the table if they are 
                           also states of obj1*/
                        List<Tuple<String, String>> newlyFoundStateList = obj2.getState();
                        foreach (Tuple<String, String> single in newlyFoundStateList)
                        {
                            if (obj1.getStateValue(single.Item1).Count() > 0)
                            {
                                //add to the table
                                bool added = false;
                                for (int iter = 0; iter < stateTable.Count(); iter++)
                                {
                                    if (stateTable[iter].Item1.getName().Equals(obj1.getName(), StringComparison.OrdinalIgnoreCase) &&
                                        stateTable[iter].Item2.Equals(single.Item1, StringComparison.OrdinalIgnoreCase) &&
                                        stateTable[iter].Item3.Equals(single.Item2, StringComparison.OrdinalIgnoreCase))
                                    {
                                        stateTable[iter] = new Tuple<Object, string, string, int>(stateTable[iter].Item1, stateTable[iter].Item2, stateTable[iter].Item3, stateTable[iter].Item4 + 1);
                                    }
                                    added = true;
                                }
                                if (!added)
                                {
                                    stateTable.Add(new Tuple<Object, string, string, int>(obj1, single.Item1, single.Item2, 1));
                                }
                            }
                        }
                    }
                }
            }
            return stateTable;
        }

		public List<String[]> initPredicate(Environment env, List<String> objects)
		{
			/* Function Description: Generate the sample space of predicates
			 * which are possible with the given objects and the environment */

			List<String[]> predicates = new List<String[]> ();
			foreach (Object obj in env.objects) 
			{
				List<Tuple<String,String>> stval = obj.getState ();
				foreach (Tuple<String,String> st in stval)
				{
					//this.lg.writeToFile (obj.uniqueName+" has state of " + st.Item1);
					if (st.Item1.Equals ("Color"))
						continue;
					if (env.isSastified ("state " + obj.uniqueName + " " + st.Item1) == 0) 
					{
						//this.lg.writeToFile ("Also added");
						predicates.Add (new string[3] { "state", obj.uniqueName, st.Item1 });
					}
				}
			}

			foreach (String objName1 in objects) 
			{
				foreach (String objName2 in objects) 
				{
					if (objName1.Equals (objName2) || !env.findObject (objName1).affordances_.Contains ("IsGraspable"))
						continue;
					if ((this.ftr.getBaseFormPredicateFreq("(On "+objName1+" " + objName2 + ")") > 0 )&&
						env.isSastified ("On " + objName1 + " " + objName2) == 0)
						predicates.Add (new string[3] { "On", objName1, objName2 });
					if ((this.ftr.getBaseFormPredicateFreq("(In "+objName1+" " + objName2 + ")") > 0) && 
						env.isSastified ("In " + objName1 + " " + objName2) == 0)
						predicates.Add (new string[3] { "In", objName1, objName2 });
				}
			}

			foreach (String objName1 in objects) 
			{
				if (!env.findObject (objName1).affordances_.Contains ("IsGraspable"))
					continue;
				if ((this.ftr.getBaseFormPredicateFreq ("(Grasping Robot " + objName1 + ")") > 0)&&
					env.isSastified ("Grasping Robot " + objName1) == 0)
					predicates.Add (new string[3] { "Grasping", "Robot", objName1 });
			}

			return predicates;
		}

		public String[] generateRelevantSpace(Clause cls, Environment env, List<Instruction> insts, List<String> grounded, double[,] leCorrMatrix, List<List<String>> plurality)
		{
			/* Function Description: Returns those predicates that contain objects contained referred
			 * to by clause cls or the previous clause. */

			List<double> scores = new List<double> ();
			List<String> objects = grounded; //new List<String> ();
			Console.WriteLine ("String is "+String.Join(",",objects));

			for (int k=insts.Count()-2; k>=0; k--) //the last one is $ marked
			{
				if (insts [k].getControllerFunction ().StartsWith ("$"))
					break;
				objects = objects.Union (insts [k].returnObject ()).ToList ();
			}

			foreach(List<String> l in plurality)
				objects = objects.Union (l).ToList (); //set of relevant objects
			if (objects.Contains ("Stove") || objects.Contains ("Stove_1"))  //though this looks hack, its not ;P -- its just I am not in mood to define children in a separate file
			{
				objects.Add ("StoveFire_1"); objects.Add ("StoveFire_2");
				objects.Add ("StoveFire_3"); objects.Add ("StoveFire_4");
				objects.Add ("StoveKnob_1"); objects.Add ("StoveKnob_2");
				objects.Add ("StoveKnob_3"); objects.Add ("StoveKnob_4");
			}

			Console.WriteLine ("Checkpoint 2 reached");

			List<String> names = cls.lngObj.Select (x => x.getName ()).ToList();
			List<String[]> predicates = initPredicate (env, objects);

			if (predicates.Count () == 0)
				return new String[0];

			for (int i=0; i<predicates.Count(); i++) 
			{
				/* predicate should contain objects in names only
				 * state objName1 stateName 
				 *  score (max_j {objName1,iterator-np_j} + max_j {stateName,iterator-np_j}) + 1{objName has been used}
				 * rel objName1 objName2
				 *  score (max_j {objName2,iterator-np_j} + max_j {objName2,iterator-np_j}) + (1{objName1 has  been used}+1{objName2 has  been used})/2*/
				double score = 0;// score1 = 0, score2 = 0;
				if (predicates [i] [0].Equals ("state")) 
				{
					if (objects.Contains (predicates [i] [1])) 
						score = score + 1;

					//stateName such as IceCream match the object name IceCream_1
					bool stateValObject = objects.Exists(name => Global.base_(name).Equals(predicates[i][2],StringComparison.OrdinalIgnoreCase));
					//stateName such as Vanilla match the stateName of objects 
					bool stateValStateVal = objects.Exists (delegate(String name)
					{
						Object obj = env.findObject(name);
						return obj.getState().Exists(states => states.Item1.Equals(predicates[i][2]) && states.Item2.Equals("True"));
					});

					if (stateValObject || stateValStateVal || names.Exists (x => x.ToLower().Contains(predicates [i] [2].ToLower())
					                                                        || predicates [i] [2].ToLower().Contains(x.ToLower()))) 
						score = score + 1;
				}
				else 
				{
					if (objects.Contains (predicates [i] [1]) || predicates [i] [1].Equals ("Robot")) 
						score = score + 1;

					if (objects.Contains (predicates [i] [2])) 
						score = score + 1;
				}
				scores.Add(score);
			}

			Console.WriteLine ("Checkpoint 3 reached with #predicates "+predicates.Count());
			//sort the predicate based on score
			for (int i=0; i<predicates.Count(); i++) 
			{
				int index = i;
				for (int j=i+1; j<predicates.Count(); j++) 
				{
					if (scores [j] > scores [index])
						index = j;
				}
				string[] swap = predicates [index];
				double swapsc = scores[index];
				predicates [index] = predicates [i];
				scores [index] = scores [i];
				predicates [i] = swap;
				scores [i] = swapsc;
			}

			Console.WriteLine ("Checkpoint 4 reached");
			for (int k=0; k<objects.Count(); k++)
				this.lg.writeToFile (objects[k]+", ");
			this.lg.writeToFile ("<br/><span style='color:orange'>Top Rank Predicates: "); //pick those with maximum score
			double maxScore = scores[0];
			int pred=0;
			for (pred=0; pred<predicates.Count(); pred++) 
			{
				if (scores [pred] != maxScore)
					break;
				this.lg.writeToFile ("(" + Global.arrayToString (predicates [pred], ' ') + ") cost = " + scores [pred] + " and <br/>");
			}
			this.lg.writeToFile ("</span><br/>");
			Console.WriteLine("Using # predicates = "+pred);
			return predicates.GetRange(0,pred).Select(predicate => "(" + predicate[0] +" "+predicate[1]+" "+predicate[2]+ ")").ToArray();
		}

        /* Define Inference Algorithms / Baselines below - 
         * Each inference algorithm/baseline accepts a a test environment and some other parameters
         * and returns the inferred sequence. */

        public List<Instruction> chance(Clause cls, Environment env)
        {
            /* Function Description : Outputs the program based on chance approach
             * Outputs (answer, groundTruth)*/

			Clause clsiterator = cls;
			Environment enviterator = env;
			List<Instruction> insts = new List<Instruction> ();
			Random rnd = new Random ();

			while (clsiterator!=null)
			{
				if (clsiterator.isCondition) 
				{
					; //execute the condition and pick the correct child
					if (clsiterator.children.Count () > 0)
						clsiterator = clsiterator.children [0];
					else
						break;
				}


				List<String[]> atoms = this.initPredicate (env, env.objects.Select (x => x.uniqueName).ToList ());
				if (atoms.Count () == 0)
					continue;
				//use uniform randomization
				List<String> selected = new List<string> ();
				for (int i=0; i<atoms.Count(); i++) 
				{
					double d = rnd.NextDouble ();
					if (d > 0.5)
						selected.Add (String.Join(" ",atoms[i]));
				}

				String postcondition = "(" + String.Join (" ", selected) + ")";

				//Console.WriteLine ("Selecting " + postcondition);
				List<Instruction> insts_ = this.symp.satisfyConstraints (enviterator, postcondition);
				if (insts_ != null) 
				{
					//Console.WriteLine ("Which gave me  " + String.Join ("^", insts_.Select (x => x.getName ())));
					insts = insts.Concat (insts_).ToList ();
					enviterator = this.sml.executeList (insts_, enviterator);
				} 
				//else this.lg.writeToFile ("Instruction was null");
				if (clsiterator.children.Count () == 0)
					break;
				else clsiterator = clsiterator.children [0];
			}

			return insts;
        }

        public List<Instruction> predefinedTemplateBaseline(Clause cls, Environment env)
        {
            /* Function Description: In this base line, we use predefined lexicons
			 * for parsing. This baseline measures whether we need any learning of
			 * lexicon when experts can write down the lexicons. 
			 * Example of lexical entries is:
			 * fill x -> (state x water)
			 * pick x ->  (Grasping Robot x) etc. A single verb can have more than one
			 * different lexicons if they are different in arguments */

			Clause clsiterator = cls;
			Environment enviterator = env;
			List<Instruction> insts = new List<Instruction> ();

			while (clsiterator!=null) 
			{
				if (clsiterator.isCondition) 
				{
					; //execute the condition and pick the correct child
					if (clsiterator.children.Count () > 0)
						clsiterator = clsiterator.children [0];
					else break;
				}

				//for this given clause -- select the matching lexicon
				String verbName = clsiterator.verb.getName ();
				List<Template> matchedTemplate = this.ftr.predefinedTemplates.Where(x=>x.verbName.Equals(verbName, StringComparison.OrdinalIgnoreCase) &&
				                                                                    x.variables.Count()==clsiterator.lngObj.Count()).ToList();

				double[,] leCorrMatrix = enviterator.getLECorrMatrix (clsiterator, insts, this.sensim, this.ftr);
				this.lg.writeToFile ("<p>"+clsiterator.getClauseDscp()+"</p><br/>Matched Template "+matchedTemplate.Count());

				if (matchedTemplate.Count () > 0) 
				{
					int best = -1, bestFitScore = -1;
					for (int tmp = 0; tmp<matchedTemplate.Count(); tmp++) 
					{
						//compute how well this template fits in this given scenario
						int fitscore = matchedTemplate[tmp].fitscore(clsiterator);
						if (fitscore > bestFitScore) 
						{
							bestFitScore = fitscore;
							best = tmp;
						}
					}

					String postcondition = matchedTemplate[best].instantiate(leCorrMatrix, enviterator);// instantiate the logical form 
					this.lg.writeToFile ("Choosing template with fit score " + bestFitScore + "<br/>Giving post-condition as " + postcondition);

					List<Instruction> insts_ = this.symp.satisfyConstraints (enviterator, postcondition);
					if (insts_ != null) 
					{
						this.lg.writeToFile ("Instruction "+String.Join("; ",insts_.Select(x=>x.getName())));
						insts = insts.Concat (insts_).ToList ();
						enviterator = this.sml.executeList (insts_, enviterator);
					}
					else this.lg.writeToFile ("Instruction was null");
				}
			    
				if (clsiterator.children.Count () == 0)
					break;
				else clsiterator = clsiterator.children [0];
			}

			return insts;
        }

        public List<Instruction> treeExploration(Tuple<int, int> test)
        {
            /* Function Description : Uses tree-expansion algorithm to expand the tree of instructions and
             * the pick up the node which has maximum resemblance to the given clause and can be easily
             * executed. */
            Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> utmp = this.obj.getDataInformation(test);
            List<Instruction> output = new List<Instruction>();

            List<Clause> cls = utmp.Item2.returnEventClause();
            Environment iterator = this.envList[test.Item1 - 1].makeCopy();

            foreach (Clause cl in cls) //iterate over each clause
            {
                List<String> clsW = new List<String>();
                clsW.Add(cl.verb.getName());
                List<SyntacticTree> sns = cl.returnNounList();
                foreach (SyntacticTree sn in sns)
                    clsW.Add(sn.getName());

                InstructionTree root = new InstructionTree(iterator);
				double[] weights_opt = null;
                List<Instruction> receiv = InstructionTree.findBestAndExpand(root,weights_opt, clsW);
                foreach (Instruction inst in receiv)
                    output.Add(inst);
                root.free();
            }
			            
            return output;
        }
		
		public List<Instruction> misra2014(Tuple<int, int> test, Environment start, Tester tester, int topN, Dictionary<String, Double> weights)
        {
            /* Function Description : This algorithm implements the forward-backward inference algorithm
             * for the assumed linear CRF. The algorithm works as - 
             * 
             * It finds sample space environments at each step of the CRF chain for the next step and uses the
             * the sample space of the previous step to do the forward inference.
             * The main agenda is that the space of environment at level k will be less than O(|T|^k) where
             * |T| is the average sample space of instructions at each level. The argument for this sparsity
             * assumptions is that different ways of doing anything will focus on a cover of objects which will
             * be much smaller than space of all objects.
             * 
             * Complexity :
             * |T|  : average number of instruction sequence per clause
             * |E|  : space of all environments
             * |ER| : average space of all reduced environments
             * k    : length of sequence = number of clauses in the environment
             * 
             * Brute Force Search                       -  O(|T|^k)
             * Forward-Backward on all environments     -  O(k|E||T|)
             * Forward-Backward on reduced environment  -  O(k|ER||T|) */


            Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> testData = tester.prs.getDataInformation(test);
            List<Clause> cls = testData.Item2.returnEventClause();

            if (topN == -1)
                tester.lg.writeToFile("<div> <button onclick='show(this)'>Using Misra et al 2014 </button> <div style='display:none;'><h3>Clausal Decomposition</h3>");
            else tester.lg.writeToFile("<div> <button onclick='show(this)'>Using Method Top" + topN + "Algorithm </button> <div style='display:none;'><h3>Clausal Decomposition</h3>");

            foreach (Clause cl in cls)
                cl.display(tester.lg);

            /* Compute the optimal instruction sequence by forward recursion 
             * 		Alpha(E,k) = min_{E' \in sampleSpace[k]} min_{I \in T, Phi(E',I_l:I) = E } { Psi_{kl} + Psi_{kl}' + Psi_{k} + Psi_{k}' + Alpha(E',k-1) }
             * alpha[e,k] data structure will store both the optimum instruction and the optimum cost
             * where the optimal instruction is the one with minimum cost starting with E0 and ending at level k in chain
             * with the environment e */

            List<Tuple<Environment, List<Instruction>, double>>[] alpha = new List<Tuple<Environment, List<Instruction>, double>>[cls.Count + 1];

            for (int i = 0; i < cls.Count; i++)
            {
                if (i == 0)
                {
                    alpha[0] = new List<Tuple<Environment, List<Instruction>, double>>();
                    alpha[0].Add(new Tuple<Environment, List<Instruction>, double>(start, new List<Instruction>(), 0));  //base case (StartingEnv, {empty-sequence},0 cost)
                }
                alpha[i + 1] = new List<Tuple<Environment, List<Instruction>, double>>(); //after this line, alpha[i], alpha[i+1] are defined with former already computed

                String verbName = cls[i].verb.getName();

				this.lg.writeToFile ("VerbName " + verbName);

                /* we have to define alpha(E,i) for all E in sampleSpace[i] assuming
                 * alpha(E',j) is known for all reachable E' and j < i. We also assume 
                 * we know the sampleSpace at all level j < i */

                foreach (Tuple<Environment, List<Instruction>, double> history in alpha[i]) //now E = env
                {
                    Environment env = history.Item1;
					VerbProgram v = tester.lexicon.Find (x=>x.getName().Equals(verbName, StringComparison.OrdinalIgnoreCase));
                    
	                if (v!=null) //verb exists
	                {
	                    List<LexicalEntry> programs = v.getProgram();
	                    List<Tuple<Object, String, String, int>> tableOfStates = null; //used by accumulated score
						double[,] leCorrMatrix = env.getLECorrMatrix (cls [i], history.Item2, this.sensim, this.ftr);

	                    /* Compute the variability table of the form 
	                        * [ Object : State : Value ]
	                        * where Object : is in Clause, State are its state and Value is the value of the state*/
	                    tableOfStates = this.createTableOfStates(cls[i], env, programs);

	                    for (int t = 0; t < programs.Count(); t++) //Step : Iterate over all program instances
	                    {
	                        /* For each program instance (T) we compute the score given by - 
	                         * interpolation or latent node score (T) + score of the non-latent node (T) + Beta(E',i+1)
	                         * where E' = execute(E, interpolated-instantianted program) */

	                        for (int trim = 0; trim <= 0/*programs[t].inst_.Count()*/; trim++) // Step :  Trimming
	                        {
	                            double score_interpolate = 0, score_core = 0, trim_score = weights["w_trim"] * trim * trim;
								LexicalEntry vtmp = programs [t];//this.trimItDown(programs[t], trim);

	                            //Step: Find the interpolation
	                            int[] mapping = map.mappingMisra2014(vtmp, cls[i], env, leCorrMatrix, this.lg);
								List<Instruction> core = this.instantiation (vtmp, mapping, env);

								List<Instruction> interpolation = tester.symp.satisfyConstraintsInInstruction (env, core, this.sml, this.lg);
								if (interpolation == null) 
									continue;

								Environment iterator = tester.sml.executeList (interpolation, env);

	                            //Step: compute the score of the interpolation
	                            score_interpolate = this.ftr.giveInterpolationScore(interpolation, weights);

	                            //Step: Compute the score of the instruction sequence
								score_core = this.ftr.getAccumulatedScore (vtmp, cls[i], iterator, core, tableOfStates, mapping, weights);
								double cost = score_interpolate + score_core + trim_score  + history.Item3;

								iterator = tester.sml.executeList (core, iterator);

	                            /* we have generated a step where the sequence I_interpol : I_core 
	                             * given the environment env gives the environment iterator. We now
	                             * see if the environment iterator appears in alpha[i+1]. If not then
	                             * we add its entry. If yes then we see if the cost of this one is lower
	                             * and according replace */

								List<Instruction> entireInstructionSeq = history.Item2.ToList().Concat(interpolation.Concat(core.ToList()).ToList()).ToList();

	                            bool replace = false;
	                            int oldEntry = -1;
	                            for (int iter = 0; iter < alpha[i + 1].Count(); iter++)
	                            {
	                                Tuple<Environment, List<Instruction>, double> newEntry = alpha[i + 1][iter];
	                                if (newEntry.Item1.isSame(iterator).Item1)
	                                {
	                                    oldEntry = iter;
	                                    if (cost < newEntry.Item3)
	                                    {
	                                        replace = true;
	                                        break; //there is atmost one entry with same environment
	                                    }
	                                }
	                            }

	                            if (oldEntry == -1) //no entry exist
	                                alpha[i + 1].Add(new Tuple<Environment, List<Instruction>, double>(iterator, entireInstructionSeq, cost));
	                            else if (replace)
	                                alpha[i + 1][oldEntry] = new Tuple<Environment, List<Instruction>, double>(iterator, entireInstructionSeq, cost);
	                        }
	                    }
	                }
                }

                /* If a given verb does not have any templates then alpha[i+1] will be
                 * empty in which case we simply copy alpha[i] to alpha[i+1] */
                if (alpha[i + 1].Count() == 0)
                    alpha[i + 1] = alpha[i];
                else alpha[i].Clear(); /* Whereto the climber upward turns his face. But when he once attains the upmost round,
                                          He then unto the ladder turns his back - Brutus [Julius Caesar] */

                /* Churning
                 * The time complexity of O(kEt) is significant since each of the t templates
                 * takes sufficient amount of time. I propose to use a greedy churning strategy
                 * where we only take the top N choices. This compromises optimality guarantee
                 * at the cost of speed. If N==-1 then the algorithm will use all the entry
                 * else it will use the top choices. */

                if (topN != -1 && alpha[i + 1].Count() > topN)
                {
                    //keep only the topN choices in alpha[i+1]
                    List<Tuple<Environment, List<Instruction>, double>> best = new List<Tuple<Environment, List<Instruction>, double>>();
                    List<int> bestIndex = new List<int>();

                    for (int iter = 0; iter < alpha[i + 1].Count(); iter++)
                    {
                        if (iter < topN)
                        {
                            //insert at the proper position
                            if (iter == 0)
                            {
                                bestIndex.Add(0);
                            }
                            else
                            {
                                bool added = false;
                                for (int j = 0; j < iter; j++)
                                {
                                    if (alpha[i + 1][iter].Item3 < alpha[i + 1][bestIndex[j]].Item3)
                                    {
                                        added = true;
                                        bestIndex.Insert(j, iter);
                                        break;
                                    }
                                }

                                if (!added)
                                    bestIndex.Add(iter);
                            }
                        }
                        else if (alpha[i + 1][iter].Item3 < alpha[i + 1][bestIndex[topN - 1]].Item3)
                        {
                            //insert at the proper position
                            for (int j = 0; j < topN; j++)
                            {
                                if (alpha[i + 1][iter].Item3 < alpha[i + 1][bestIndex[j]].Item3)
                                {
                                    bestIndex.Insert(j, iter);
                                    break;
                                }
                            }
                        }
                    }

                    //use bestIndex to store these choices in best
                    for (int iter = 0; iter < topN; iter++)
                        best.Add(alpha[i + 1][bestIndex[iter]]);
                    alpha[i + 1] = best;
                }
            }

            //find optimal instruction from alpha[cls.count]
            List<Instruction> output = null;
            double optCost = Double.PositiveInfinity;
            if (cls.Count() == 0)
                output = new List<Instruction>();
            else
            {
                foreach (Tuple<Environment, List<Instruction>, double> done in alpha[cls.Count])
                {
                    if (done.Item3 < optCost)
                    {
                        output = done.Item2;
                        optCost = done.Item3;
                    }
                }
            }

			tester.lg.writeToFile("</div></div>");
            return output;
        }

		public List<Tuple<String,List<Instruction>>> ublBaseline(List<int> test)
		{
			/* Function Description: Given the training data, it runs the algorithm and returns
			 * the instruction sequence. */

			#region create_test_file
			System.IO.StreamWriter testFile = new System.IO.StreamWriter(Constants.ublPath+"test");
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = this.obj.returnAllData();

			int testCount = 0;
			foreach(int pt in test)
			{
				for(int i=0; i<datas[pt].Item4.Count(); i++)
				{
					if(datas[pt].Item4[i].sentence.Count()==0)
						continue;
					testCount++;
					testFile.WriteLine(datas[pt].Item4[i].sentence.Replace('\n',' ').ToLower());
					testFile.WriteLine("(Grasping:t Robot:o Robot:o)\n"); //dummy placeholder predicate -- requirement of the UBL algorithm
				}
			}

			testFile.Flush();
			testFile.Close();
			#endregion

			#region run_ubl_algorithm
			Console.WriteLine("running command is ./rundev.pl en 0");
			Process proc = new Process();
			proc.StartInfo.WorkingDirectory = Constants.rootPath + @"Baselines/UBL/UBL/experiments/new/";
			Console.WriteLine(Constants.rootPath + @"Baselines/UBL/UBL/experiments/new/   "+System.IO.File.Exists(Constants.rootPath + @"Baselines/UBL/UBL/experiments/new/rundev.pl"));
			proc.StartInfo.FileName = "/bin/sh";
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.Arguments = string.Format(Constants.rootPath + @"Baselines/UBL/UBL/experiments/new/rundev.sh");
			proc.Start();
			proc.WaitForExit(200000);
			Console.WriteLine("Letting UBL algorithm run for 7minutes. Sleeping in the meantime");
			System.Threading.Thread.Sleep(7*60*1000);//7minutes
			Console.WriteLine("Woken up. Using the output produced by the UBL in the file run.dev.en.0.0");
			#endregion

			#region parse_the_result
			String[] output = System.IO.File.ReadAllLines(Constants.rootPath + @"Baselines/UBL/UBL/experiments/new/run.dev.en.0.0");
			List<String> predicates = new List<String>();

			System.IO.StreamWriter predicateFiles = new System.IO.StreamWriter(Constants.rootPath + @"Baselines/UBL/UBL/experiments/new/predicates");
			bool outputFound = false;
			for(int i = output.Length-1; i>=0;i--)
			{
				if(output[i].StartsWith("-----Testing Now------"))
					break;

				if(output[i].StartsWith("WRONG:"))
				{
					String cstr = output[i].Substring("WRONG:".Length).Trim(), recently=null;
					if(cstr[0]=='[')
					{
						cstr = cstr.Substring(1, cstr.Length -2);
						String[] cstrSplit = cstr.Split(new char[]{','}).Select(x=>x.Trim()).ToArray();
						recently = Global.standardizeStatePredicates(cstrSplit[0]);
					}
					else recently = Global.standardizeStatePredicates(cstr);

					predicates.Add(recently);
					predicateFiles.WriteLine(recently);
					outputFound=true;
				}

				if(output[i].Contains("=================="))
				{
					predicateFiles.WriteLine(output[i+1]);
					if(!outputFound)
						predicates.Add("");
					outputFound=false;
				}
			}
			predicates.Reverse();
			Console.WriteLine("Are they same? "+testCount+" vs "+predicates.Count());
			Console.Read();
			predicateFiles.Flush();
			predicateFiles.Close();

			List<Tuple<String,List<Instruction>>> insts = new List<Tuple<String,List<Instruction>>>();
			int whichPred=0;
			System.IO.StreamWriter instFiles = new System.IO.StreamWriter(Constants.rootPath + @"Baselines/UBL/UBL/experiments/new/instruction");

			foreach(int pt in test)
			{
				List<Instruction> newInst = new List<Instruction>();
				Console.WriteLine("Working on the point "+pt);
				Environment start = this.envList[datas[pt].Item1.Item1-1];
				StringBuilder log = new StringBuilder();
				for(int i=0; i<datas[pt].Item4.Count(); i++) //iterate over clauses
				{
					if(datas[pt].Item4[i].sentence.Count()==0)
						continue;
					Console.WriteLine("Working on the point "+pt+" val i = "+i+" whichPredicate "+whichPred+" out of "+predicates.Count());
					log.Append("Sentence: <i>"+datas[pt].Item4[i].sentence+"</i><br/>");
					log.Append("<span style='color:green'>Logical Form: "+predicates[whichPred]+"</span><br/>");
					List<Instruction> inst_ = this.symp.satisfyConstraints(start,predicates[whichPred++]);
					if(inst_==null) //skip this constraint since it cannot be satisfied
					{
						log.Append("{Cannot be executed}");
						continue; 
					}
					log.Append("<span style='color:red;'>Instruction "+String.Join("; ", inst_.Select(x=>x.getName()))+"</span><br/><br/>");

					newInst = newInst.Concat(inst_).ToList();
					start = this.sml.executeList(inst_,start);
				}

				instFiles.WriteLine(String.Join(" ",newInst.Select(x=>x.getName())));
				insts.Add(new Tuple<String, List<Instruction>>(log.ToString(), newInst));
			}

			instFiles.Flush();
			instFiles.Close();
			#endregion

			return insts;
		}

		public List<Instruction> acl2015(Clause cls, Environment env, Tester tester, Dictionary<String, Double> weights, List<object> param)
        {
            /* Function Description : The main algorithm for grounding the clause-tree cls given the starting environment env. */
			cls.conjunctionSplit ();//split the clause based on conjunction
			this.oldvalue = null;
			this.oldLECorrMatrix = null;
			String name = "Main Model with VEIL template " + ((bool)param[0]).ToString () + " Generative Templates " + 
					      ((bool)param[1]).ToString ()+" Storing Templates "+ ((bool)param[2]).ToString ();
			tester.lg.writeToFile ("<div> <button onclick='show(this)'> " + name + "</button> <div style='display:none;'><h3>Clausal Decomposition</h3>");
            List<Clause> evnCl = cls.returnEventClause();
            foreach (Clause c in evnCl)
                tester.lg.writeToFile(c.getClauseDscp()+"; ");
            
            tester.lg.writeToFile("<br/>");

            List<Tuple<Clause, List<Tuple<Environment, List<Instruction>, double>>>> alpha = new List<Tuple<Clause, List<Tuple<Environment, List<Instruction>, double>>>>();

            List<Clause> visited = new List<Clause>(); //A node is visited when it has been considered for score evaluation
            List<Clause> cover = new List<Clause>();   /* Cover is a set clause nodes which have not been visited 
                                                       * but all their parents have been visited */
            List<Instruction> output = null;
            double maxScoreLeaf = Double.NegativeInfinity;
            cover.Add(cls); //Since parent of cls is null

            List<Tuple<Environment, List<Instruction>, double>> singletonRoot = new List<Tuple<Environment, List<Instruction>, double>>();
            singletonRoot.Add(new Tuple<Environment, List<Instruction>, double>(env, new List<Instruction>(), 1));//this was 0 earlier
            alpha.Add(new Tuple<Clause, List<Tuple<Environment, List<Instruction>, double>>>(null, singletonRoot));

            while (cover.Count() != 0)
            {
                Clause iterator = cover[0];
				/*if(iterator.isCondition)
					Console.WriteLine("Working on a condition "+iterator.conditionName);
				else Console.WriteLine("Working on a fresh clause "+iterator.verb.getName());*/
                cover.RemoveAt(0);

                /* Find reachable entries
                 * - as its in the cover, all its parents have been visited
                 * - find the entries of the parents in the alpha list and trim the non-reachable ones
                 *   ex: if there is an if-condition between the parent and this cls then trim the entries
                 *       according to the condition, the environment in the entry and position of the iterator node
                 *       ex:- if the iterator node is on the true side and if condition is true in the environment then
                 *       keep the entry else trim */

                List<Tuple<Environment, List<Instruction>, double>> entryThisClause = new List<Tuple<Environment, List<Instruction>, double>>();

                foreach (Tuple<Clause, List<Tuple<Environment, List<Instruction>, double>>> entries in alpha)
                {
                    if (iterator.parent.Contains(entries.Item1) || (iterator.parent.Count() == 0 && entries.Item1 == null))
                    {
                        foreach (Tuple<Environment, List<Instruction>, double> history in entries.Item2)
                        {
                            //if the parent is a condition then make sure that this branch is reachable
                            bool reachable = true;
							if (entries.Item1 != null && entries.Item1.isCondition)
								reachable = true;//entries.Item1.conditional.isConditionSatisfied(history.Item1, new List<Tuple<String,String>>());

                            if (reachable) //only if the clause is reachable can you do anything with it
                            {
								Tuple<List<Instruction>, int, Double> res = null; 
								res = this.mainModelPerClause (iterator, history, tester, entryThisClause, weights, param);
                                if (iterator.children == null || iterator.children.Count() == 0)
                                {
                                    if (res.Item3 > maxScoreLeaf)
                                    {
                                        output = res.Item1;
										maxScoreLeaf = res.Item3;
                                    }
                                }
							}
                        }
                    }
                }

                #region keep_top_k_options
                entryThisClause.Sort((a, b) => b.Item3.CompareTo(a.Item3));
                if(entryThisClause.Count() >= this.k)
					entryThisClause.RemoveRange(this.k, entryThisClause.Count() - this.k);
                #endregion

                //Add iterator to the list of visited nodes and update the alpha list
                visited.Add(iterator);
                alpha.Add(new Tuple<Clause, List<Tuple<Environment, List<Instruction>, double>>>(iterator, entryThisClause));

                // Add those children of the iterator to the cover whose all parents have been visited
                foreach (Clause child in iterator.children)
                {
                    bool unvisParentExist = false;
                    foreach (Clause parent in child.parent)
                    {
                        if (!visited.Contains(parent))
                            unvisParentExist = true;
                    }
                    if (!unvisParentExist)
                        cover.Add(child);
                }
            }

            // we output the instruction sequence = argmin_{[i]} { [ (c,e,[i],double) \in alpha, c is a leaf}
            tester.lg.writeToFile("</div></div>");
            if(output == null)
				return new List<Instruction>();
            return output;
        }
		
		private List<Tuple<String,double>> bottomUpGenTemplate(Clause iterator, Tuple<Environment, List<Instruction>, double> history, Tester tester,
		                                                       double[,] leTestCorrMatrix, List<String> grounded, List<List<String>> plurality, 
		                                                       Dictionary<String, Double> weights)
		{
			/* Function Description: Gen-Template Algorithm which generates new samples. The algorithm is as follows:-
			 * 1. Find a small space of predicates that consists of relevant object
			 * 2. Find factor score of each atomic predicates
			 * 3. Pick the top K and consider their combination
			 * 4. If these combinations do not increase the factor score then stop else repeat the 2-4 cycle */

			Environment envTest = history.Item1;
			List<Tuple<String,Double>> predScoreTable = new List<Tuple<string, double>> ();
			VerbProgram v = tester.lexicon.Find(veil_ => veil_.getName().Equals(iterator.verb.getName(), StringComparison.OrdinalIgnoreCase));

			String[] sampleSpace = this.generateRelevantSpace (iterator, history.Item1, history.Item2, grounded, leTestCorrMatrix, plurality);
			List<String> queue = new List<String>();

			for (int i=0; i<sampleSpace.Length; i++)
				queue.Add (sampleSpace[i]);

			bool genPivotNotFound = true;
			double genPivotScore = Double.NegativeInfinity;

			int total = 0;

			while(genPivotNotFound)
			{
				total = total + queue.Count ();
				Console.WriteLine ("total is " + total);
				if(queue.Count() == 0)
				{
					genPivotNotFound=false; 
					break;
				}

				List<Tuple<String,Double>> newlyAdded = new List<Tuple<string, double>> ();
				#region find_factor_scores_of_every_sample
				genPivotNotFound = false;
				foreach(String constraint in queue)
				{
					/* Feature computation stage
	                 * - Given envTest, iterator, set of predicates {q} and resultant instruction sequence {i}  
	                 *   1. Mapping Cost = LE cost + EE cost (at the moment)
	                 *   2. Instruction prior
	                 *   3. Verb-Correlation Score
	                 *   4. Description Length
	                 *   5. Trimming Cost 
	                 *   6. Avg. Frequency of predicate skeletal {q} */

					double mapCost = 0, dscpCost = 0, le = 0, leRecall = 0, /*predSkeletalPrior = 0, predTotalPrior = 0, */
							f_1p = 0, f_2p = 0, f_1vp = 0, f_2vp = 0, f_1ap = 0, f_2ap = 0, f_1vap = 0, f_2vap = 0, 
					        endState=0, bias = 0, trans = 0, argTrans = 0;

					List<String> objectCover = Global.getObjects(constraint, history.Item1);
					String lelog = "";
					foreach(String objName in objectCover)
					{
						int j = envTest.objects.FindIndex(x => x.uniqueName.Equals(objName));
						double lePerObject = 0;

						for(int i=0; i<leTestCorrMatrix.GetLength(0);i++) 
							lePerObject = Math.Max(lePerObject,leTestCorrMatrix[i,j]);

						//In new approach, le also considers neighbors
						if(this.oldLECorrMatrix!=null)
						{
							for(int i=0; i<this.oldLECorrMatrix.GetLength(0);i++) 
								lePerObject = Math.Max(lePerObject,this.oldLECorrMatrix[i,j]); //order of objects dont change (obj indexed by j remains same)
						}

						le = le + lePerObject;
						lelog = lelog + objName + " -> " + lePerObject + "; ";
					}

					int numObjects=0;
					for(int lang=0; lang < leTestCorrMatrix.GetLength(0); lang++) //iterate over each object
					{
						/* if this object is singular than use the maximum 
							 * else use all of the objects that ar greater than with 0.85 correlation */
						String mainNoun = iterator.lngObj [lang].getName ();
						if(Processing.isPlural(mainNoun))
						{
							for(int j=0; j<leTestCorrMatrix.GetLength(1);j++)
							{
								if(leTestCorrMatrix[lang,j]>0.85) //check if this object is present
								{
									if(objectCover.Exists(x=>x.Equals(envTest.objects[j].uniqueName)))
										leRecall = leRecall + leTestCorrMatrix[lang,j];
									numObjects++;
								}
							}
						}
						else
						{
							double max = Double.NegativeInfinity;
							foreach(String objName in objectCover)
								max = Math.Max(max, leTestCorrMatrix[lang, envTest.objects.FindIndex(x=>x.uniqueName.Equals(objName))]);
							if(max < 0.85) //clamping
								max=0;
							numObjects++;
							leRecall = leRecall + max;
						}
					}

					leRecall = weights["w_lerecall"]*leRecall/Math.Max(numObjects,Constants.epsilon);
					lelog = lelog + " divide "+le+" with "+objectCover.Count();
					mapCost= weights["w_le"]*le/ (objectCover.Count() + Constants.epsilon); //the EE cost is 0 as there is no reference
					String[] splitter = constraint.Split(new char[]{'^'});
					dscpCost= weights["w_dscp"] * splitter.Length;//generatedInst.Aggregate(0.0, (sum, n) => sum + n.norm());

					f_1p = weights["w_1prior"]*this.ftr.getPredicateFreq1(splitter, false, null);
					f_2p = weights["w_2prior"]*this.ftr.getPredicateFreq2(splitter, false, null);
					if(v!=null)
					{
						f_1vp = weights["w_1vprior"]*this.ftr.getPredicateFreq1(splitter, false, v.verbName);
						f_2vp = weights["w_2vprior"]*this.ftr.getPredicateFreq2(splitter, false, v.verbName);
					}

					f_1ap = weights["w_1aprior"]*this.ftr.getPredicateFreq1(splitter, true, null);
					f_2ap = weights["w_2aprior"]*this.ftr.getPredicateFreq2(splitter, true, null);
					if(v!=null)
					{
						f_1vap = weights["w_1vaprior"]*this.ftr.getPredicateFreq1(splitter, true, v.verbName);
						f_2vap = weights["w_2vaprior"]*this.ftr.getPredicateFreq2(splitter, true, v.verbName);
					}

					/*if(v!=null)
						predSkeletalPrior = weights["w_prior"]*splitter.Aggregate(0.0,(sum,cstr_) => sum + v.fetchFrequency(cstr_))/(splitter.Length*v.totalFrequency() + Constants.epsilon);
					//predTotalPrior = weights["w_argprior"]*splitter.Aggregate(0.0,(sum,cstr_) => sum + this.ftr.getPredicateFreqOld(cstr_))/(splitter.Length*this.ftr.zPredFreqOld + Constants.epsilon);
					predTotalPrior = weights["w_argprior"]*splitter.Aggregate(0.0,(sum,cstr_) => sum + this.ftr.getPredicateFreq(cstr_,1,false, null))/(splitter.Length*this.ftr.getZPredicateFreq(1,false,null) + Constants.epsilon);*/


					if (iterator.children == null || iterator.children.Count() == 0)
						endState = weights["w_end"]*this.ftr.getEndStateProbability(constraint);

					Tuple<double, String> relres = this.ftr.fetchRelationshipFeature(iterator, plurality, constraint);
					double relfeature = weights["w_rel"]*relres.Item1;
					bias = 0;//weights["w_bias"];
					trans = weights["w_trans"]*this.ftr.fetchTransitionProbability(constraint, this.oldvalue);
					argTrans = weights["w_argtrans"]*this.ftr.fetchArgTransitionProbability(constraint, this.oldvalue);

					double exponent = mapCost + leRecall + dscpCost +  /*predSkeletalPrior + predTotalPrior */ 
								 + f_1p + f_2p + f_1vp + f_1vp + f_1ap + f_2ap + f_1vap + f_2vap +
							     relfeature + endState + bias + trans + argTrans;
					double totalScore = history.Item3*Math.Exp(exponent);

					//tester.lg.setLowPriority();
					tester.lg.writeToFile("Generated Template "+predScoreTable.Count()+"<ul>"+
					                      "<li><b>Constraint</b> "+constraint+"</li></ul>"+
										  "<ul><li><b>Mapping Score</b> "+mapCost+" [Ge LE cost "+ lelog +"]</li>"+
					                      "<li><b>Language Recall</b> "+leRecall+" </li>"+
					                      "<li><b>Description Length</b> "+dscpCost+"</li>"+
					                      "<li><b>Sentence Similarity</b> 0</li>"+
					                      /* "<li><b>Predicate Skeletal Prior</b> "+predSkeletalPrior+"</li>"+
					                      "<li><b>Predicate Total Prior</b> "+predTotalPrior+"</li>"+ */
					                      "<li><b>Environment Priors</b>"+f_1p +"; " + f_2p +"; " +f_1vp +"; " + f_2vp +"; " + f_1ap +"; " 
					                      + f_2ap +"; " + f_1vap +"; " + f_2vap +"</li>"+
					                      "<li><b>Relationship feature</b> "+relfeature+"  ["+relres.Item2+"] </li>"+
					                      "<li><b>End State</b> "+endState+"</li>"+
					                      "<li><b>Bias</b> "+bias+"</li>"+
					                      "<li><b>Transition Prob.</b> "+trans+"</li>"+
					                      "<li><b>Arg Transition Prob.</b> "+argTrans+"</li>"+
					                      "<li><b>Total Score </b> = "+totalScore+" (history: "+history.Item3+" and exponent "+exponent+"</li></ul>");
					//tester.lg.setHighPriority();

					if(totalScore > genPivotScore)
					{
						genPivotScore = totalScore;
						genPivotNotFound = true;
					}

					newlyAdded.Add(new Tuple<string, double>(constraint, totalScore));
					predScoreTable.Add(new Tuple<string, double>(constraint, totalScore));
				}
				#endregion

				if (!genPivotNotFound) //pivot found
					break;

				#region create_next_queue_by_combining_the_top_k
				newlyAdded.Sort((a,b)=> b.Item2.CompareTo(a.Item2));
				//newlyAdded = newlyAdded.OrderBy(a=>a.Item2).ToList(); //-added to make sure the sort is stable

				queue.Clear();
				int k = Math.Min(20,newlyAdded.Count());
				double minScoreAdmitted = newlyAdded[k-1].Item2;

				//take all those top entries with same score as minScoreAdmitted
				for(int j=k; j<newlyAdded.Count();j++)
				{
					if(newlyAdded[j].Item2 == minScoreAdmitted)
						k++;
					else break;
				}
				k = Math.Min(200,k);

				for(int i=0; i<k; i++)
				{
					for(int j=i+1; j<k; j++)
					{
						//combine i and j
						String newConstraint = String.Join("^",newlyAdded[i].Item1.Split(new char[]{'^'}).Union(newlyAdded[j].Item1.Split(new char[]{'^'})).Distinct());
						if(!SymbolicPlanner.trivialUnsat(newConstraint))
						{
							//ensure that its not already in the queue
							String[] newSplit = newConstraint.Split(new char[]{'^'});
							bool IsPresent = false;

							foreach(String present in queue)
							{
								String[] presentSplit = present.Split(new char[]{'^'});
								if(presentSplit.Length==newSplit.Length)
								{
									IsPresent = presentSplit.Aggregate(true, (acc,x)=> acc && newSplit.Contains(x));
									if(IsPresent)
										break;
								}
							}

							if(!IsPresent)
								queue.Add(newConstraint);
						}
					}
				}
				#endregion
			}
			return predScoreTable;
		}


		private Tuple<List<Instruction>, int, Double> mainModelPerClause(Clause iterator, Tuple<Environment, List<Instruction>, double> history,
		                                                                    Tester tester, List<Tuple<Environment, List<Instruction>, double>> entryThisClause,
		                                                                    Dictionary<String, Double> weights, List<object> param)
		{
			/* Function Description: For a given node, the history (given environment, instruction so far, score so far) 
             * this algorithm updates the entryThisClause vector by performing inference steps on this clause node.
             * The algorithm also return the best score and corresponding instruction sequence. */

			if (iterator.isCondition) //if clause represents a condition
			{
				Instruction marker = new Instruction ();
				marker.setNameDescription ("$Conditional",new List<String>());
				history.Item2.Add(marker);
				entryThisClause.Add (history);
				return new Tuple<List<Instruction>, int, double>(history.Item2, -1, history.Item3);
			}

			Environment envTest = history.Item1;
			List<Instruction> output = null;
			double maxScoreLeaf = Double.NegativeInfinity;
			int init = entryThisClause.Count ();

			List<List<String>> referencedObjects = new List<List<String>> (); //List_i refers to set of objects referred to by object_i

			double[,] leTestCorrMatrix = envTest.getLECorrMatrix (iterator, history.Item2, this.sensim, this.ftr);

			List<String> grounded = new List<String> ();
			String sdt = "";

			#region compute_referenced_objects
			for (int i=0; i<leTestCorrMatrix.GetLength(0); i++)
			{
				String mainNoun = iterator.lngObj [i].getName ();
				int maxIndex = 0;
				List<String> plurality_ = new List<string> ();
				if (Processing.isPlural (mainNoun)) 
				{
					for (int j=0; j<envTest.objects.Count(); j++) 
					{
						if (leTestCorrMatrix [i, j] > 0.85) 
							plurality_.Add (envTest.objects [j].uniqueName);
					}
				}
				else
				{
					plurality_.Add(envTest.objects[0].uniqueName);
					for (int j=1; j<envTest.objects.Count(); j++) 
					{
						if (leTestCorrMatrix [i, j] > leTestCorrMatrix [i, maxIndex]) 
						{
							maxIndex = j;
							plurality_.Clear ();
							plurality_.Add (envTest.objects[j].uniqueName);
						} else if (leTestCorrMatrix [i, j] == leTestCorrMatrix [i, maxIndex]) 
							plurality_.Add (envTest.objects [j].uniqueName);
					}

					if (leTestCorrMatrix [i, maxIndex] <= 0.8) //if the max has low confidence then ignore the list
						plurality_.Clear ();

					//if the mainNoun is not in plural form then make a choice
					if (plurality_.Count() > 1) //since set of objects is not plural therefore make a choice
					{
						this.lg.writeToFile("Made a choice for   "+String.Join("^",plurality_)+" max is "+leTestCorrMatrix[i,maxIndex]);
						//need a smarted way to break the tie
						plurality_.Sort();
						plurality_.RemoveRange (1, plurality_.Count()-1);
					}
				}

				grounded = grounded.Union (plurality_).ToList();
				sdt = sdt + i.ToString () + ". " + mainNoun + " maps to " + String.Join(", ",plurality_) + " Cost: "
					      + leTestCorrMatrix [i, maxIndex] + " MaxFreq: " + plurality_.Count() + "<br/>";
				referencedObjects.Add (plurality_);
			}
			tester.lg.writeToFile (sdt);
			#endregion

			tester.lg.writeToFile("<div> <button onclick='show(this)'>Clause - "+iterator.verb.getName()+"</button> <div style='display:none;'>");

			#region apply_inference_steps
			/* - iterate over each dataset D_iterator = { {(c,i,e,z,\xi) \in D such that v(C)=v(iterator) }
			 * - fetch the flipped-predicates [post-conditions] of each environment
			 * - call the symbolic planner to fulfill these post conditions
			 * - infer I
			 * - update alpha tables */

			List<Tuple<String,double>> predScoreTable = new List<Tuple<String,double>>();

			VerbProgram v = tester.lexicon.Find(veil_ => veil_.getName().Equals(iterator.verb.getName(), StringComparison.OrdinalIgnoreCase));
			List<LexicalEntry> vtList = null;
			if(v == null)
				vtList = new List<LexicalEntry>();
			else vtList = v.getProgram();

			int count = 0;
			foreach (LexicalEntry vt in vtList)
			{
				if(!(bool)param[0]) //if the model is only test-based lexicon then dont use these lexical entries
					continue;

				tester.lg.writeToFile("<br/>Lexical Entry " + String.Join("^",vt.predicatesPost) + "Count = "+count+" of "+vtList.Count());
				Tuple<int[], double, String> mappingResult = this.map.mappingPredicates(vt, iterator, envTest, history.Item2, weights, this.oldLECorrMatrix);
				if(mappingResult ==null)
				{
					tester.lg.writeToFile("No appropriate mapping was found<br/>");
					continue;
				}

				//Console.WriteLine("Mapping "+String.Join(",",mappingResult.Item1));
				String stas = "";
				for(int j=0; j<mappingResult.Item1.Count();j++)
					stas = stas + "; " +envTest.objects[mappingResult.Item1[j]].uniqueName;
				//Console.WriteLine(stas);

				String iterConstraints = String.Join("^",this.instantiatePredicates(vt, mappingResult.Item1, envTest).Distinct().ToList());
				tester.lg.writeToFile("Constraints "+iterConstraints+"<br/>");

				if(iterConstraints.Length==0)
				{
					Console.WriteLine ("empty ");
					count++;
					continue;
				}

				String constraint_ = iterConstraints;
				/* For each constraint, find the instruction sequence that satisfies it
                 * Each constraint is of type - predicate^predicate^.... where these
                 * predicates need to be satisfied */

				String cstrlog = "";
				String preNoiseConstraint = constraint_.ToString();
				String constraint = constraint_;

				if(Constants.opmode == OpMode.Offline)
					constraint = this.removeNoise(constraint_, referencedObjects, history.Item2, history.Item1);

				if(constraint.Length == 0)
					continue;

				if(!preNoiseConstraint.Equals(constraint))
					cstrlog= "Pre Noise Constraint was "+preNoiseConstraint;

				String cstr = constraint;
				cstr = this.expansion(referencedObjects, constraint);

				String[] cstrSplit = cstr.Split(new char[]{'^'});

				/* Feature computation stage
                 * - Given envTest, iterator, set of predicates {q} and resultant instruction sequence {i}  
                 *   1. Mapping Cost (which takes into account 5 features)
                 *   2. Sentence Similarity
                 *   3. Language Recall
                 *   4. Avg. Frequency of predicate skeletal {q}
                 *   5. Total Frequency of predicate {q,e} 
				 *   */
				 
				double mapCost = 0,dscpCost = 0, sentenceSim = 0, /*predSkeletalPrior = 0, predTotalPrior = 0, */
				       f_1p = 0, f_2p = 0, f_1vp = 0, f_2vp = 0, f_1ap = 0, f_2ap = 0, f_1vap = 0, f_2vap = 0, 
					   leRecall= 0, endState = 0, bias=0, trans=0, argTrans= 0;

				//Logical Form and Environment Score
				mapCost= mappingResult.Item2; //Mapping-Cost
				dscpCost= weights["w_dscp"] * cstrSplit.Length;
				String sensSimLog = "";

				/*Console.WriteLine("\robobrain{} 1 "+iterConstraints+" original postc "+
				                  String.Join("^",vt.predicatesPost) + " variables "+vt.zVariablePredicatePost.Count());*/

				if(vt.cls_.sentence!=null && iterator.sentence!=null)
				{
					List<String> trainWords = vt.cls_.getWords();//sentence.Split(new char[]{' '}).Select(x=>x.Trim()).ToList();
					List<String> testWords = iterator.getWords();//sentence.Split(new char[]{' '}).Select(x=>x.Trim()).ToList();
					trainWords.RemoveAll(x=>x.Length==0);
					testWords.RemoveAll(x=>x.Length==0);
					Tuple<double,string> res = Global.jaccardIndex(trainWords, testWords);
					sentenceSim = weights["w_sensim"]*res.Item1;
					sensSimLog = res.Item2;
				}
				else sentenceSim = 0;

				List<String> objectCover = Global.getObjects(cstr, history.Item1);
				int numObjects = 0;
				for(int lang=0; lang < leTestCorrMatrix.GetLength(0); lang++)//if each object that was referred is being used
				{
					/* if this object is singular then use the maximum 
					 * else use all of the objects that ar greater than with 0.85 correlation */
					String mainNoun = iterator.lngObj [lang].getName ();
					if(Processing.isPlural(mainNoun))
					{
						for(int j=0; j<leTestCorrMatrix.GetLength(1);j++)
						{
							if(leTestCorrMatrix[lang,j]>0.85) //check if this object is present
							{
								if(objectCover.Exists(x=>x.Equals(envTest.objects[j].uniqueName)))
									leRecall = leRecall + leTestCorrMatrix[lang,j];
								numObjects++;
							}
						}
					}
					else
					{
						double max = Double.NegativeInfinity;
						foreach(String objName in objectCover)
							max = Math.Max(max, leTestCorrMatrix[lang, envTest.objects.FindIndex(x=>x.uniqueName.Equals(objName))]);
						if(max < 0.85) //clamping
							max=0;
						numObjects++;
						leRecall = leRecall + max;
					}
				}
				leRecall = weights["w_lerecall"]*leRecall/Math.Max(numObjects, Constants.epsilon);

				//Logical Forms -- Environment Prior
				//predSkeletalPrior = weights["w_prior"]*cstrSplit.Aggregate(0.0,(sum,cstr_) => sum + v.fetchFrequency(cstr_))/(cstrSplit.Length*v.totalFrequency() + Constants.epsilon);
				//predSkeletalPrior = weights["w_prior"]*cstrSplit.Aggregate(0.0,(sum,cstr_) => sum + this.ftr.getPredicateFreq(cstr_,1,true,null))/(cstrSplit.Length*this.ftr.getZPredicateFreq(1, true, null) + Constants.epsilon);
				//predTotalPrior = weights["w_argprior"]*cstrSplit.Aggregate(0.0,(sum,cstr_) => sum + this.ftr.getPredicateFreqOld(cstr_))/(cstrSplit.Length*this.ftr.zPredFreqOld + Constants.epsilon);

				f_1p = weights["w_1prior"]*this.ftr.getPredicateFreq1(cstrSplit, false, null);
				f_2p = weights["w_2prior"]*this.ftr.getPredicateFreq2(cstrSplit, false, null);
				if(v!=null)
				{
					f_1vp = weights["w_1vprior"]*this.ftr.getPredicateFreq1(cstrSplit, false, v.verbName);
					f_2vp = weights["w_2vprior"]*this.ftr.getPredicateFreq2(cstrSplit, false, v.verbName);
				}

				f_1ap = weights["w_1aprior"]*this.ftr.getPredicateFreq1(cstrSplit, true, null);
				f_2ap = weights["w_2aprior"]*this.ftr.getPredicateFreq2(cstrSplit, true, null);
				if(v!=null)
				{
					f_1vap = weights["w_1vaprior"]*this.ftr.getPredicateFreq1(cstrSplit, true, v.verbName);
					f_2vap = weights["w_2vaprior"]*this.ftr.getPredicateFreq2(cstrSplit, true, v.verbName);
				}

				//Relationship Feature
				Tuple<double,string> relres = this.ftr.fetchRelationshipFeature(iterator, referencedObjects, constraint);
				double relfeature = weights["w_rel"]*relres.Item1;

				//End State Feature
				if (iterator.children == null || iterator.children.Count() == 0)
					endState = weights["w_end"]*this.ftr.getEndStateProbability(constraint);

				//Transition Probabilities
				trans = weights["w_trans"]*this.ftr.fetchTransitionProbability(cstr, this.oldvalue);
				argTrans = weights["w_argtrans"]*this.ftr.fetchArgTransitionProbability(cstr, this.oldvalue);
			
				//All Features have been computed
				double exponent = mapCost + leRecall + dscpCost + sentenceSim +  /*predSkeletalPrior 
								  + predTotalPrior */ + f_1p + f_2p + f_1vp + f_1vp + f_1ap + f_2ap + f_1vap + f_2vap 
								  + relfeature + endState + bias + trans + argTrans;

				double totalScore = history.Item3*Math.Exp(exponent);//Math.Round(history.Item3*Math.Exp(exponent),2); --handle overflow in future

				//tester.lg.setLowPriority();
				tester.lg.writeToFile("Template Count ["+count+"]<ul>"+
				                      "<li><b>Constraint:</b>"+cstr+" <br/>["+ cstrlog +"] </li>"+
				                      "<li><b>Mapping Score</b> "+mappingResult.Item2+" [Log : "+mappingResult.Item3+"]</li>"+
				                      "<li><b>Language Recall</b> "+leRecall+"</li>"+
				                      "<li><b>Description Length</b> "+dscpCost+"</li>"+
				                      "<li><b>Sentence Similarity</b> "+sentenceSim+"  ["+sensSimLog+"] </li>"+
				                      /*"<li><b>Predicate Skeletal Prior</b> "+predSkeletalPrior+"</li>"+
				                      "<li><b>Predicate Total Prior</b> "+predTotalPrior+"</li>"+*/
				                      "<li><b>Environment Priors</b>"+f_1p +"; " + f_2p +"; " +f_1vp +"; " + f_2vp +"; " + f_1ap +"; " 
				                      								 + f_2ap +"; " + f_1vap +"; " + f_2vap +"</li>"+
				                      "<li><b>Relationship feature</b> "+relfeature+" ["+relres.Item2+"] </li>"+
				                      "<li><b>End State</b> "+endState+"</li>"+
				                      "<li><b>Transition Prob.</b> "+trans+"</li>"+
				                      "<li><b>Arg Transition Prob.</b> "+argTrans+"</li>"+
				                      "<li><b>Bias</b> "+bias+"</li>"+
				                      "<li><b>Total Score </b> := "+totalScore+" [ history: "+history.Item3+", exponent"+exponent+" ]</li></ul>");
				//tester.lg.setHighPriority();
				tester.lg.writeToFile("Total Score "+totalScore+" [ history: "+history.Item3+", exponent"+exponent+" ]</li></ul>");

				predScoreTable.Add(new Tuple<String,double>(cstr, totalScore));
				count++;
				#endregion
			}

			int numVEILTemplates = predScoreTable.Count(); //number of VEIL templates

			#region GenTemplate
			/* GEN-Template Stage: Maybe the given templates are all wrong, so we
			 * create additional template as possible samples. */
			if((bool)param[1])
			{
				predScoreTable = predScoreTable.Concat(this.bottomUpGenTemplate(iterator, history, tester, leTestCorrMatrix,
				                                                                grounded, referencedObjects, weights)).ToList();
			}
			tester.lg.writeToFile("</div></div>");
			#endregion

			List<int> indices = new List<int> (); //indices
			for (int i=0; i<predScoreTable.Count(); i++)
				indices.Add(i);

			//predScoreTable.Sort ((a,b) => b.Item2.CompareTo(a.Item2));

			for (int i=0; i<predScoreTable.Count(); i++) 
			{
				int placement = i; //placement of element at i
				for (int j=i+1; j<predScoreTable.Count(); j++) 
				{
					if(predScoreTable[j].Item2 > predScoreTable[placement].Item2)
						placement = j;
				}

				if (placement != i) //if condition is true then swap elements as position i and placement
				{
					Tuple<String, double> tmpswap = predScoreTable[i];
					int indicestmp = indices[i];
					predScoreTable [i] = new Tuple<String, double> (predScoreTable [placement].Item1, predScoreTable [placement].Item2);
					predScoreTable [placement] = new Tuple<string, double> (tmpswap.Item1, tmpswap.Item2);
					indices [i] = indices [placement];
					indices [placement] = indicestmp;
				}
			}

			#region find_top_k_valid_instruction_assignment_with_maximum_factor_score
			int which = 0, added = 0;

			/*tester.lg.writeToFile("<br/>------------------<br/>");
			foreach (Tuple<String, double> discoveredPred in predScoreTable) 
				tester.lg.writeToFile(discoveredPred.Item1+" at score "+discoveredPred.Item2);
			tester.lg.writeToFile("<br/>------------------<br/>");*/

			foreach (Tuple<String, double> discoveredPred in predScoreTable) 
			{
				which++;
				String orderedConstraint = this.ordered(discoveredPred.Item1, referencedObjects, envTest);
				tester.lg.writeToFile("<br/>Picking "+orderedConstraint+" at score = "+discoveredPred.Item2+"<br/>");
				List<Instruction> instruction = null;
				if(which>50)
					instruction = new List<Instruction>(); //---optimization choice: Note that this an optimization choice
				else instruction = tester.symp.satisfyConstraints (envTest, orderedConstraint);

				if (instruction == null) 
				{
					tester.lg.writeToFile("Which gave me Null results :-(<br/>");
					continue;
				}

				this.oldvalue = discoveredPred.Item1;
				tester.lg.writeToFile("<br/>Picking "+orderedConstraint+" at score = "+discoveredPred.Item2+"<br/>");
				String logs = "Found at (" + which + "/" + predScoreTable.Count () + ") Instruction is " + instruction.Aggregate ("", (acc,inst) => acc + "; " + inst.getName ());

				//if the used template is a generatedTemplate then create a new VEIL-template from it for future use
				if((bool)param[2] && indices[which-1] >= numVEILTemplates)
				{
					#region generate_new_veil_template_with_this_predicates
					Environment copiedEnv = history.Item1.makeCopy();
					LexicalEntry generatedTemplate = new LexicalEntry(discoveredPred.Item1.Split(new char[]{'^'}).ToList(), iterator, copiedEnv, -1, this.sml);//new VeilTemplate(iterator, copiedInst, copiedEnv, -1, this.sml);
					this.lg.writeToFile("Generating template for the verb " +iterator.verb.getName()+" with predicates " +discoveredPred.Item1 );
					tester.addVEILTemplate(generatedTemplate);
					tester.inf.ftr.singletonUpdate(generatedTemplate);
					#endregion
				}

				tester.lg.writeToFile (logs+"<br/>");
				tester.lg.writeToFile ("<b>Choosing "+discoveredPred.Item1+"</b><br/>");
				List<Instruction> entire = history.Item2.ToList ().Concat (instruction).ToList ();

				if (iterator.children == null || iterator.children.Count () == 0) //iterator is a leaf
				{
					if (discoveredPred.Item2 > maxScoreLeaf) 
					{
						output = entire;
						maxScoreLeaf = discoveredPred.Item2;
					}
				}

				Console.WriteLine("Constraint "+discoveredPred.Item1+" gives instruction = " +instruction.Aggregate("",(acc,x)=>acc+"; "+x.getName()));
				Environment iterEnv_ = tester.sml.executeList (instruction, envTest);
				Instruction marker = new Instruction ();
				marker.setNameDescription ("$Verb " + iterator.verb.getName () + " Count = " + count, new List<String> ());
				entire.Add (marker);
				entryThisClause.Add (new Tuple<Environment, List<Instruction>, double> (iterEnv_, entire, discoveredPred.Item2));
				added++;
				if(added==this.k) //optimization choice
					break;
			}
			#endregion

			if(init == entryThisClause.Count())
			{
				Instruction marker = new Instruction ();
				marker.setNameDescription ("$Stupid Grounding",new List<String>());
				history.Item2.Add(marker);
				entryThisClause.Add (history);
			}

			if (output == null)
			{
				output = history.Item2;
				maxScoreLeaf = history.Item3;
			}

			this.oldLECorrMatrix = leTestCorrMatrix;
			return new Tuple<List<Instruction>, int, double>(output, -1, maxScoreLeaf);
		}

		public String removeNoise(String constraint, List<List<String>> plurality, List<Instruction> instPrev, Environment env)
		{
			/* Function Description: Remove all the noisy predicates in constraint, that
			 * does not contain an object/state from plurality */

			List<String> predicates = constraint.Split (new char[] { '^' }).ToList();
			List<String> noiseFree = new List<string> ();
			List<String> relevantObjects = plurality.Aggregate (new List<String> (), (acc,x) => acc.Union (x).ToList());

			for (int k=instPrev.Count()-2; k>=0; k--) //the last one is $ marked
			{
				if (instPrev [k].getControllerFunction ().StartsWith ("$"))
					break;
				relevantObjects = relevantObjects.Union (instPrev [k].returnObject ()).ToList ();
			}

			foreach (String predicate in predicates) 
			{
				String[] words = Global.getAtomic (predicate).Item2.Split (new char[] { ' ' });
				Tuple<bool,string> res = Global.getAtomic (predicate);
				int satisfy = env.isSastified (res.Item2);

				if (res.Item1 && satisfy == 1)
					satisfy = 0;
				else if (res.Item1 && satisfy == 0)
					satisfy = 1;

				if ((relevantObjects.Contains (words [1]) || relevantObjects.Contains (words [2]) ) && satisfy==0 && this.ftr.getBaseFormPredicateFreq(predicate)>0)
				{
					noiseFree.Add (predicate);
					continue;
				}
				else if (words [0].Equals ("state")) 
				{
					/* if the statename was the name of a category of relevant objects or 
					 * is the name of a state that is present in a relevant object and is true */

					if (relevantObjects.FindIndex (x => Global.base_ (x).Equals (words [2], StringComparison.OrdinalIgnoreCase) || 
					                               env.findObject(x).checkStateAndVal(words[2],"True")) != -1 ) 
					{
						noiseFree.Add (predicate);
						continue;
					}
				}
			}

			return String.Join ("^", noiseFree);
		}

		public static List<Instruction> makeValidInstruction(Environment env, List<Instruction> instruction)
		{
			/* Function Description: There are cases where the instruction may not be valid, e.g.;
			 * consider the command "take the pillows"; which translates to command that must 
			 * grasp 4 pillows while robot cannot grasp more than 2 pillows. However, "take the pillows"
			 * command might be followed by the command "keep them on the couch". In which case; we
			 * convert the pseudo-instruction into valid instruction. Except, for this grasp glitch, its
			 * not clear where else this functionality might be needed. Thus this is a task specific function.
			 * I forsee something little more than glitches and maybe its worth paying attention to where else does it occur */

			/* algorithm:
             * if a command is moveto(z1); grasp(z1); such that there are more than 2 objects grasped simulatenously;
             * then wait until you find the place where z1 is used and shift moveto z1; grasp z1; just before it.
             * If z1 is the one that is immediately being used then keep one of the object on a temporary place and 
             * add moveto z1; grasp z1; towards the next nearest reference or the end (if never referred). 
             * One choice of picking which object to place would be, the one which has the nearest reference. Right, not the last.
             * E.g., moveto cup1; grasp cup1; moveto cup2; grasp cup2; moveto cup3; grasp cup3; moveto table3; keep cup3 on table3; keep cup3 on table3
             * then we change it to 
             * moveto cup1; grasp cup1; moveto cup3; grasp cup3; moveto table3; keep cup3 on table3; moveto cup2; grasp cup2; keep cup2 on table3 
			 * why? cause stand-alone grasp and moveto maybe primary goal while intermediate grasp such as grasping cup2, cup3 can be moved around
			 * since they are usually not primary goal. A sticky situation could be where, we end up with three grasping. Where, we can simply leave them.
			 */

			List<String> graspedObject = new List<String> (); //list of grasped object
			List<String> reserveGraspCommand = new List<String> ();
			List<Instruction> valid = new List<Instruction> ();
			String graspedThis= null;

			for (int i=0; i< instruction.Count(); i++) 
			{
				int index = -1;
				while (instruction [i].getArguments ().FindIndex(x => reserveGraspCommand.Contains (x)) != -1) 
				{
					index = instruction [i].getArguments ().FindIndex (x => reserveGraspCommand.Contains (x));
					String objName_ = instruction [i].getArguments () [index];
					Instruction moveto = new Instruction ("moveto", new List<String>(){objName_});
					Instruction grasp = new Instruction ("grasp", new List<String>(){objName_});
					valid.Add (moveto);
					valid.Add (grasp);
					reserveGraspCommand.RemoveAll (x => x.Equals (objName_));
				}

				valid.Add (instruction[i]);

				if (instruction [i].getControllerFunction ().Equals ("grasp")) 
				{
					graspedThis = instruction [i].getArguments () [0];
					graspedObject.Add (graspedThis);
				}
				else if (instruction [i].getControllerFunction ().Equals ("release") ||
					(instruction [i].getControllerFunction ().Equals ("keep")) ||
					(instruction [i].getControllerFunction ().Equals ("insert")))
					graspedObject.RemoveAll(x => x.Equals (instruction [i].getArguments () [0]));

				if (graspedObject.Count () > 2) //an object was grasped at this step i; when already 2 objects were being grasped
				{
					if (i == instruction.Count () - 1)
						break;
					/* else if the recently grasped object; appears somewhere else in the 
					 * instruction sequence, then move the grasp and moveto command there 
					 * else leave it as it is. */
				
					bool appearsLater = false;
					for (int j=i+1; j<instruction.Count(); j++) 
					{
						if (instruction [j].getArguments ().Contains (graspedThis)) 
						{
							appearsLater = true;
							reserveGraspCommand.Add (graspedThis);
							break;
						}
					}

					if (appearsLater) 
					{
						valid.RemoveAt (valid.Count () - 1);
						if (valid.Last ().getControllerFunction ().Equals ("moveto") && valid.Last ().getArguments () [0].Equals (graspedThis))
							valid.RemoveAt (valid.Count () - 1);
						graspedObject.Remove (graspedThis);
					}
				}
			}

			return valid;
		}


		public static List<Instruction> makeValidInstruction1(Environment env, List<Instruction> instruction)
		{
			/* Function Description: There are cases where the instruction may not be valid, e.g.;
			 * consider the command "take the pillows"; which translates to command that must 
			 * grasp 4 pillows while robot cannot grasp more than 2 pillows. However, "take the pillows"
			 * command might be followed by the command "keep them on the couch". In which case; we
			 * convert the pseudo-instruction into valid instruction. Except, for this grasp glitch, its
			 * not clear where else this functionality might be needed. Thus this is a task specific function.
			 * I forsee something little more than glitches and maybe its worth paying attention to where else does it occur */

			/* algorithm:
             * if a command is moveto(z1); grasp(z1); such that there are more than 2 objects grasped simulatenously;
             * then wait until you find the place where z1 is used and shift moveto z1; grasp z1; just before it.
             * If z1 is the one that is immediately being used then keep one of the object on a temporary place and 
             * add moveto z1; grasp z1; towards the next nearest reference or the end (if never referred). 
             * One choice of picking which object to place would be, the one which has the nearest reference. Right, not the last.
             * E.g., moveto cup1; grasp cup1; moveto cup2; grasp cup2; moveto cup3; grasp cup3; moveto table3; keep cup3 on table3; keep cup3 on table3
             * then we change it to 
             * moveto cup1; grasp cup1; moveto cup3; grasp cup3; moveto table3; keep cup3 on table3; moveto cup2; grasp cup2; keep cup2 on table3 
			 * why? cause stand-alone grasp and moveto maybe primary goal while intermediate grasp such as grasping cup2, cup3 can be moved around
			 * since they are usually not primary goal. A sticky situation could be where, we end up with three grasping. Where, we can simply leave them.
			 */

			List<String> graspedObject = new List<String> (); //list of grasped object
			List<String> reserveGraspCommand = new List<String> (); 
			List<Instruction> valid = new List<Instruction> ();
			String graspedThis= null;

			for (int i=0; i< instruction.Count(); i++) 
			{
				//check if this instruction has anything to do with any reserve command. If yes then grasp the object.
				int index = -1;
				while (instruction [i].getArguments ().FindIndex(x => reserveGraspCommand.Contains (x)) != -1) 
				{
					index = instruction [i].getArguments ().FindIndex (x => reserveGraspCommand.Contains (x));
					Console.WriteLine ("index "+index);
					String objName_ = instruction [i].getArguments () [index];
					Instruction moveto = new Instruction ("moveto", new List<String>(){objName_});
					Instruction grasp = new Instruction ("grasp", new List<String>(){objName_});
					valid.Add (moveto);
					valid.Add (grasp);
					reserveGraspCommand.RemoveAll (x => x.Equals (objName_));
				}

				valid.Add (instruction[i]);

				if (instruction [i].getControllerFunction ().Equals ("grasp")) 
				{
					graspedThis = instruction [i].getArguments () [0];
					graspedObject.Add (graspedThis);
				}
				else if (instruction [i].getControllerFunction ().Equals ("release") ||
				         (instruction [i].getControllerFunction ().Equals ("keep")) ||
				         (instruction [i].getControllerFunction ().Equals ("insert")))
					graspedObject.RemoveAll(x => x.Equals (instruction [i].getArguments () [0]));

				if (graspedObject.Count () > 2) //an object was grasped at this step i; when already 2 objects were being grasped
				{
					if (i == instruction.Count () - 1)
						break;
					//check if grasped object is being immediately referred
					if (instruction [i + 1].getArguments ().Exists(x => x.Equals (graspedThis))) 
					{
						//keep down a grasped object; and put it in reserve
						//picking the first object for now, but should use the strategy mentioned in comment
						reserveGraspCommand.Add (graspedObject[0]);
						String releasedObject = graspedObject [0];
						graspedObject.RemoveAt (0);
						//if last command involving this object was a grasp then remove it and any daggling moveto else keep this object somewhere
						int wasgrasp = -1;
						bool dagglingmoveto = false;

						for (int j=valid.Count()-1; j>=0; j--) //remove the last grasped command (and daggling moveto) that grasped the released object
						{
							if (valid [j].getArguments ().Contains (releasedObject) && valid [j].getControllerFunction ().Equals ("grasp")) 
							{
								wasgrasp = j;
								if (j - 1 >= 0 && valid [j - 1].getControllerFunction ().Equals ("moveto") && valid [j - 1].getArguments () [0].Equals (releasedObject))
									dagglingmoveto = true;
								break;
							}
						}

						if (wasgrasp == -1) 
						{
							//keep this object somewhere
							String chosenObj = null;
							if(env.objects.FindIndex(x=>x.uniqueName.Equals("Counter_1"))!=-1)
								chosenObj = "Counter_1";
							else
							{
								foreach (Object obj in env.objects) 
								{
									if (obj.affordances_.Contains ("IsPlaceableOn")) 
									{
										chosenObj = obj.uniqueName;
										break;
									}
								}
							}

							Instruction moveto = new Instruction ("moveto", new List<String>(){chosenObj});
							Instruction grasp = new Instruction ("keep", new List<String>(){releasedObject, "On", chosenObj});
							valid.Add (moveto);
							valid.Add (grasp);
						}
						else
						{
							//remove the grasp instruction
							valid.RemoveAt (wasgrasp);
							if (dagglingmoveto)
								valid.RemoveAt (wasgrasp-1);
						}

					}
					else 
					{
						//keep this object in reserve and remove instructions
						reserveGraspCommand.Add (graspedThis);
						valid.RemoveAt (valid.Count()-1); //remove grasp command
						if (valid.Last ().getControllerFunction ().Equals ("moveto") && valid.Last ().getArguments ()[0].Equals(graspedThis))
							valid.RemoveAt (valid.Count()-1); //removing any daggling moveto command
						graspedObject.Remove (graspedThis);
					}
				}
			}

			foreach (String obj in reserveGraspCommand) 
			{
				Instruction moveto = new Instruction ("moveto", new List<String>(){obj});
				Instruction grasp = new Instruction ("grasp", new List<String>(){obj});
				valid.Add (moveto);
				valid.Add (grasp);
			}

			return valid;
		}

		public String ordered(String constraint, List<List<String>> plurality, Environment env)
		{
			/* Function Description: Given a constraint, this function reorders the constraint
			 * according to the sentence. This takes advantage of Rintanen's planner tie-breaking design
			 * such that sequence tries to fulfill first predicate first. Thus sequence will look more aligned
			 * to the way objects occur in the sentence. E.g., we get different sequence for (On pillow_1 armchair_1)^(On chips_1 armchair_1)
			 * vs (On chips_1 armchair_1)^(On pillow_1 armchair_1). But, if chips are mentioned first, we prefer later as this gives
			 * a squence which first keeps the chip. This is closer to the ordering in the sentence.*/

			if (constraint.Length == 0)
				return constraint;

			constraint = constraint.Replace ("(In Cd_1 Xbox_1)", "(state Xbox_1 CD)"); //to be removed in future

			String[] split = constraint.Split (new char[] {'^'}).ToArray();

			int head = 0;
			for (int i=0; i<plurality.Count(); i++) 
			{
				for (int j=0; j<split.Count(); j++) 
				{
					//if split contains the base form of any plurality[i] then move it to position head
					int index = plurality[i].FindIndex(x=>split[j].Contains(Global.base_(x)));
					if (index != -1 && head < j) 
					{
						//swap j and head
						String tmp = split [head].ToString();
						split [head] = split [j];
						split [j] = tmp;
						head++;
						break;
					}
				}
			}

			return string.Join ("^", split);
		}

    }
}
