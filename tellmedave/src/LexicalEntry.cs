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

using System.IO;

namespace ProjectCompton
{
    class LexicalEntry
    {
        /* Class Description: Defines one single VEIL entry
         * and functions for modifying and displaying it.
         * VEIL stands for Verb-Environment-Instruction Sequence Library
         * a single entry in veil consists of - 
         *   1. Clause : an intermediate language representation 
         *               the cls here consists of a single-verb 'eventive' clause
         *   2. Environment : The environment in which the statement represented by clause was made
         *   3. Instruction Sequence : The action committed in response to the command represented by Clause
         *                    and in accordance with the environment
         *   Auxillary Data - 
         *   1. zVariables : Algorithm generalizes the variables of the instruction sequence 
         *                   zVariables contains the new variables
         *   2. xiOrigMapping : contains the mapping of the variables to original objects and relations in environment
         *   3. entryIndex : to be removed
         */

        private Clause cls = null;                           //Single-Verb 'Eventive' Clause
        private List<Instruction> inst = null;               //Instruction Sequence Template
        private Environment env = null;    					 //Environment

        //Auxillary Information
        public List<String> zVariablesInst = null;              //Generalize the template
        public List<int> xiOrigMappingInst = null;              //Original Mapping
        public List<Instruction> instOld {private set; get;} //Ungeneralized Instruction Sequence
        public int entryIndex = -1;                          //Index to where this file is coming from

		public double[,] leCorrMatrix = null;

		//Post Conditions
		public List<String> predicatesPostOld { private set; get;} /* Contains original list of environment predicates that were changed
		                                                            * by this given instruction sequence*/
		public List<String> predicatesPost { private set; get;}    /* Contains generalized list of environment predicates that were changed
		                                                            * by this given instruction sequence*/
		public List<String> zVariablePredicatePost{ private set; get;}         //Generalize the predicate
		public List<int> xiOrigMappingPredicatePost{ private set; get;}        //Original Mapping of the predicate

		//Pre Conditions
		public List<String> predicatesPreOld { private set; get;} /* Contains original list of environment predicates that were changed
		                                                           * by this given instruction sequence*/
		public List<String> predicatesPre { private set; get;}    /* Contains generalized list of environment predicates that were changed
		                                                           * by this given instruction sequence*/
		public List<String> zVariablePredicatePre{ private set; get;}         //Generalize the predicate
		public List<int> xiOrigMappingPredicatePre{ private set; get;}        //Original Mapping of the predicate

        //Properties
        public Clause cls_
        {
            get { return cls; }
            set { cls = value; }
        }

        public List<Instruction> inst_
        {
            get { return inst; }
            set { inst = value; }
        }

        public Environment env_
        {
            get { return env; }
            set { env = value; }
        }

        //Constructor
        public LexicalEntry(Clause cls, List<Instruction> inst, Environment env, int entryIndex, Simulator sml)
        {
            /* Constructor Description : Instantitate the template entries */

            #region Generalization_Algorithm
            /* Generalize the instruction sequence. The entire generalization process was introduced
             * to capture interesting complex structures at training time which is different from what
             * happnes at test time i.e. the instantiation algorithm where they are converted back to original
             * parameters. I introduced the generalization phase mostly to capture structure as follow - 
             * if a person keeps a ball at a point then the instruction 'Keep ball on <point>' can be generalized
             * where ball -> z1 but <point> instead of going to z2 goes to something like \lambda x IsCorner(z3,x) where z3-> table
             * thus the mapping becomes - 
             *
             * ball -> z1
             * table -> z3
             * <point> -> \lambda x IsCorner(z3,x)
             * 
             * Arguably this could also be done at instantiation time but I see them as two different phase. In the generalization
             * you understand what has been done and in instantiation you apply. I believe, code complexity is minimize this way
             * although accuracy might be better for a joint model.
             * 
             * However at this point I am not doing structural generalization. This area is certainly worth exploring given the importance
             * it holds for the field and I see it as a crucial research area. That said, I am presently just replacing
             * all objects by simple parameter like z1, z2 .. and there is no structural relation learning which is sad cause  
             * its beautiful. 
             * 
             * Also I am not generalization non-object parameters such as relations at this time.
             * */

            this.zVariablesInst = new List<String>();
            this.xiOrigMappingInst = new List<int>();

            this.instOld = new List<Instruction>();
            foreach(Instruction insta in inst)
                this.instOld.Add(insta.makeCopy());

            int newVariable = 1;
			List<Object> objL = env.objects;

            foreach (Instruction inst_ in inst)
            {
                List<String> description = inst_.getArguments();
                for (int i = 0; i < description.Count(); i++)
                {
                    String dscp = description[i];

                    //check if it has been already generalized
                    bool alreadyAdded = false;
                    for (int k = 0; k < this.xiOrigMappingInst.Count(); k++)
                    {
                        if (dscp.Equals(objL[this.xiOrigMappingInst[k]].uniqueName,StringComparison.OrdinalIgnoreCase))
                        {
                            alreadyAdded = true;
                            inst_.changeParam(i, this.zVariablesInst[k]);
                            break;
                        }
                    }

                    if (alreadyAdded)
                        continue;
                   
                    for (int j = 0; j < objL.Count(); j++)
                    {
                        if (dscp.Equals(objL[j].uniqueName))
                        {
                            alreadyAdded = true;
                            zVariablesInst.Add("$" + newVariable); //add variable
                            this.xiOrigMappingInst.Add(j); //add original mapping
                            inst_.changeParam(i, "$" + newVariable); //do the actual replacement
                            newVariable++;
                            break;
                        }
                    }

                    if(!alreadyAdded)
                    {
                        //Console.WriteLine(" Why can't I generalize "+dscp);//why?
                        List<String> sp =new List<string>(){"Inside","In","On","Above","Below"};
                        foreach(String s in sp)
                        {
                            if(dscp.Equals(s,StringComparison.OrdinalIgnoreCase))
                                alreadyAdded=true;
                        }
                        if(!alreadyAdded)
                        {
                            bool isThere=false;
                            if (File.Exists(Constants.rootPath + "ungeneralized.txt"))
                            {
                                String[] lines = File.ReadAllLines(Constants.rootPath + "ungeneralized.txt");
                                for (int ik = 0; ik < lines.Length; ik++)
                                {
                                    if (lines[ik].Equals(dscp))
                                    {
                                        isThere = true;
                                        break;
                                    }
                                }
                            }

                            if (!isThere)
                            {
                                StreamWriter sw = new System.IO.StreamWriter(Constants.rootPath + "ungeneralized.txt", true);
                                sw.WriteLine(dscp);
                                sw.Flush();
                                sw.Close();
                            }
                        }
                    }
                }
            }

            #endregion

            this.cls = cls;
            this.inst = inst;
            this.env = env;
            this.entryIndex = entryIndex;

            foreach (Instruction itmp in this.instOld)
            {
                foreach (String dscp in itmp.getArguments())
                {
                    if (dscp[0] == '$')
                        throw new ApplicationException("$");
                }
            }

			#region Pre-Conditions-Predicate-Computation
			/* We store and generalize predicates of all objects
			 * in the cover of the instruction sequence */
			List<String> cover = new List<String>();
			foreach(Instruction itmp in this.instOld)
				cover=cover.Union(itmp.returnObject()).ToList();

			//For this cover, find all the predicates in the original environment
			this.predicatesPreOld=new List<String>();
			foreach(String objName in cover)
				this.predicatesPreOld = this.predicatesPreOld.Concat(this.env.fetchObjPredicates(objName)).ToList();
			this.zVariablePredicatePre=new List<String>();
			this.xiOrigMappingPredicatePre=new List<int>();
			this.predicatesPre = this.generalizePredicateList(this.predicatesPreOld,this.zVariablePredicatePre,this.xiOrigMappingPredicatePre);
			#endregion

			#region Flipped-Post-Conditions-Predicate-Computation
			Environment end = sml.executeList(this.instOld,this.env);
			this.predicatesPostOld = end.difference(this.env);
			this.zVariablePredicatePost=new List<String>();
			this.xiOrigMappingPredicatePost=new List<int>();
			this.predicatesPost = LexicalEntry.takeOnlyPositivePredicates(this.generalizePredicateList(this.predicatesPostOld,this.zVariablePredicatePost,this.xiOrigMappingPredicatePost));
			#endregion
        }

		public LexicalEntry(List<String> constraint, Clause cls, Environment env, int entryIndex, Simulator sml)
		{
			//Constructor Description: Directly adding veil template
			this.cls = cls;
			this.env = env;
			this.inst = new List<Instruction> ();
			this.instOld = new List<Instruction> ();
			this.entryIndex = entryIndex;
			#region Flipped-Post-Conditions-Predicate-Computation
			this.predicatesPostOld = constraint;
			this.zVariablePredicatePost=new List<String>();
			this.xiOrigMappingPredicatePost=new List<int>();
			this.predicatesPost = this.generalizePredicateList(this.predicatesPostOld,this.zVariablePredicatePost,this.xiOrigMappingPredicatePost);
			#endregion
		}

		public LexicalEntry(Clause cls, List<String> constraint, Environment env)
		{
			//Constructor Description: Adds constructor using bootstrap algorithm
			this.cls = cls;
			this.predicatesPost = constraint;
			this.env = env;
			this.xiOrigMappingPredicatePost = new List<int>();
			this.zVariablePredicatePost = new List<String> ();

			for (int i=0; i<env.objects.Count(); i++) 
			{
				this.xiOrigMappingPredicatePost.Add (i);
				this.zVariablePredicatePost.Add ("$var"+(i+1));
			}
			//in the present algorithm, there are unique objects for each variable
		}

		public int numVariables(bool usePredicates)
        {
            //Function Description: Returns number of variables
			if (usePredicates)
				return zVariablePredicatePost.Count ();
            else return zVariablesInst.Count();
        }

        public int getOrigObject(int variable, bool usePredicates)
        {
            /* Function Description : Returns the object to which varName was 
             * originally mapped to */
			if (usePredicates && variable < this.xiOrigMappingPredicatePost.Count ())
				return this.xiOrigMappingPredicatePost [variable];
            if (!usePredicates && variable < this.xiOrigMappingInst.Count())
                return this.xiOrigMappingInst[variable];
            return -1;
        }

		public List<Instruction> origInstSeq()
		{
			List<Instruction> inst = new List<Instruction> ();
			List<Object> objL = this.env_.objects;
			foreach (Instruction i in this.inst) 
			{
				List<String> dscpNew = new List<String>();
				foreach (String dscp in i.getArguments()) 
				{
					bool wasGeneralized = false;
					for(int l=0; l<this.numVariables(false);l++)
					{
						if(this.zVariablesInst[l].Equals(dscp))
						{
							wasGeneralized=true;
							dscpNew.Add (objL[this.getOrigObject(l,false)].uniqueName);
							break;
						}
					}

					if(!wasGeneralized)
						dscpNew.Add(dscp);
				}
				inst.Add (new Instruction (i.getControllerFunction(),dscpNew));
			}
			return inst;
		}

		public List<String> predicateCover(int var1, int var2)
		{
			/* Function Description: Return those predicates that use variables var1 and var2 */

			List<String> cover = new List<String> ();
			foreach (String pred in this.predicatesPost) 
			{
				if (pred.Contains (this.zVariablePredicatePost [var1])
					&& pred.Contains (this.zVariablePredicatePost [var2]))
					cover.Add (pred.ToString ());
			}
			return cover;
		}

		private List<String> generalizePredicateList(List<String> predicatesOld, List<String> variables, List<int> ximapping )
		{
			/* Function Description: Given set of predicates {p1,p2,...pn} where 
			 * pi = (state objName stateValue) or (rel objName1 objName2) and their nots. This 
			 * function replaces objName by variables and stores the mapping in 
			 * and variable list in ximapping and variables respectively */

			List<String> predicates = new List<String>();
			List<Object> objL = this.env.objects;
			foreach (String predicate_ in predicatesOld) 
			{	
				String predicate = predicate_.Substring (1, predicate_.Length - 2);
				bool not = false;
				if (predicate.StartsWith ("not")) 
				{
					not = true;
					int startIndex = predicate.IndexOf ('('), endIndex = predicate.IndexOf (')');
					predicate = predicate.Substring (startIndex + 1, endIndex - startIndex - 1);
				}
				String[] words = predicate.Split (new char[] { ' ' });

				if (words [0].Equals ("state")) //state objName stateName
				{
					//generalize words[1]
					int eindex = -1, varindex = -1;
					for (int i=0; i<objL.Count(); i++) 
					{
						if (objL [i].uniqueName.Equals (words [1])) 
						{
							eindex = i;
							break;
						}
					}

					for (int i=0; i<ximapping.Count(); i++) 
					{
						if (ximapping [i] == eindex)
							varindex = i;
					}

					if (varindex == -1)//add a new entry
					{
						variables.Add ("$var" + (variables.Count () + 1));
						ximapping.Add (eindex);
						varindex = variables.Count () - 1;
					}
					if (not)
						predicates.Add ("(not (state " + variables [varindex] + " " + words [2] + "))");
					else
						predicates.Add ("(state " + variables [varindex] + " " + words [2] + ")");
				} 
				else  //relationship objName1 objName2
				{
					//generalize words[1] and words[2]
					int eindex1 = -1, eindex2 = -1, varindex1 = -1, varindex2 = -1;
					for (int i=0; i<objL.Count(); i++) 
					{
						if (objL [i].uniqueName.Equals (words [1]))
							eindex1 = i;
						if (objL [i].uniqueName.Equals (words [2]))
							eindex2 = i;
						if (eindex1 != -1 && eindex2 != -1)
							break;
					}

					for (int i=0; i<ximapping.Count(); i++) 
					{
						if (ximapping [i] == eindex1)
							varindex1 = i;
						if (ximapping [i] == eindex2)
							varindex2 = i;
					}

					if (varindex1 == -1)//add a new entry
					{
						variables.Add ("$var" + (variables.Count () + 1));
						ximapping.Add (eindex1);
						varindex1 = variables.Count () - 1;
					}

					if (varindex2 == -1)//add a new entry
					{
						variables.Add ("$var" + (variables.Count () + 1));
						ximapping.Add (eindex2);
						varindex2 = variables.Count () - 1;
					}

					if (not)
						predicates.Add ("(not (" + words [0] + " " + variables[varindex1] + " " + variables[varindex2] + "))");
					else
						predicates.Add ("(" + words [0] + " " + variables[varindex1] + " " + variables[varindex2] + ")");
				}
			}
			return predicates;	
		}

		public static List<String> takeOnlyPositivePredicates(List<String> constraint)
		{
			/* Function Description: Constraints can contain many negative predicates which 
			 * do not concern the context. E.g., if I move a cup from point A to point B. 
			 * I may make it Not Near point C, not near point X, not inside point Z etc.
			 * but usually from data, I see that only positive predicates take part in context
			 * formation i.e. I wanted it to be on point B. Of course, there could be cases
			 * of the reverse and in future we should not need this function. But for now,
			 * its poisoning the veil-template cause negative templates get initializet to 
			 * contradictory things and hence create problem. This could be of course solved by
			 * a better mapping algorithm. */

			List<String> newconstraints = new List<string> ();
			List<String> notconstraints = new List<string> ();
			//remove near predicate
			foreach (String cstr in constraint) 
			{
				Tuple<bool,string> atom = Global.getAtomic (cstr);
				String[] words = atom.Item2.Split (new char[] { ' ' });
				if(!words[0].Equals("Near") && !atom.Item1)
					newconstraints.Add (cstr);
				if (!words [0].Equals ("Near") && words [0].Equals ("state") && atom.Item1)
					notconstraints.Add (cstr);
			}

			//remove not predicate unless constraint has size of 1
			if (newconstraints.Count () == 1) 
				newconstraints = newconstraints.Union (notconstraints).ToList ();
			return newconstraints;
		}

		public int referringVariable(int lang)
		{
			/* Function Descrption: Given a language object in clause indexed by lang. 
			 * Return the variable that maps to the object referred to by the lang object */

			if (this.leCorrMatrix == null)
				throw new ApplicationException ("Why is this error coming");

			int max = 0;
			for (int obj=0; obj<this.leCorrMatrix.GetLength(1); obj++) 
			{
				if (this.leCorrMatrix [lang, obj] > this.leCorrMatrix [lang, max]) 
					max = obj;
			}

			for(int var_ = 0; var_<this.zVariablesInst.Count(); var_++)
			{
				if(this.xiOrigMappingInst[var_] == max)
					return var_;
			}

			return -1;
		}

		public String groundedObject(int lang)
		{
			/* Function Descrption: Given a language object in clause indexed by lang. 
			 * Return the variable that maps to the object referred to by the lang object */

			int max = -1;
			for (int obj=0; obj<this.leCorrMatrix.GetLength(1); obj++) 
			{
				if (this.leCorrMatrix [lang, obj] > this.leCorrMatrix [lang, max]) 
					max = obj;
			}

			if (max == -1)
				return null;
			else return this.env.objects[max].uniqueName;
		}

        public void display(Logger lg)
        {
            /* Function Description : Displays the VeilTemplate in html format */
            lg.writeToFile("<div style='display:none'> Entry = " + this.entryIndex
                               + "<div id='instance'> <b>Displaying Clause</b><br/>"); //clause begin
			lg.writeToFile(cls.getClauseDscp());
			lg.writeToFile("<br/><span><i>"+cls.sentence+"</i></style>");
            lg.writeToFile("</div>"); //clause ending
            lg.writeToFile("<div id='instruct'> <b>Displaying Instruction</b><br/><table>"); //instruction begin
			List<Instruction> insta = this.origInstSeq ();
			for (int i=0; i<this.inst.Count(); i++) 
			{
				lg.writeToFile ("<tr><td>");
				this.inst [i].display (lg);
				lg.writeToFile ("</td><td>");
				insta [i].display (lg);
				lg.writeToFile("</td></tr>");
			}
			lg.writeToFile ("</table>");

			lg.writeToFile ("<b>Displaying Flipped Predicates</b><p style='color:#003333'>");
			for (int j=0; j<this.predicatesPost.Count(); j++) 
			{
				lg.writeToFile (this.predicatesPost[j] + " ");
				if (j %5 == 0)
					lg.writeToFile ("<br/>");
			}
			lg.writeToFile ("</p>");

			//display original variables
			/*String map = "";
			List<Object> objL = this.env.giveObjList ();
			for (int z=0; z < this.zVariables.Count(); z++)
				map = map + this.zVariables[z] + " -> " + objL[this.xiOrigMapping[z]].uniqueName+"<br/>";
			lg.writeToFile(map);*/
			//diplay the environment
            //env.display(lg);  // Display Environment
            lg.writeToFile("</div></div>"); //instruction ends, single verbProgram ends
		}
    }
}
