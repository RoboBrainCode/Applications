/* Tell Me Dave 2013-14, Robot-Language Learning Project
 * Code developed by - Dipendra Misra (dkm@cs.cornell.edu)
 * working in Cornell Personal Robotics Lab.
 * 
 * More details - http://tellmedave.cs.cornell.edu
 * This is Version 2.0 - it supports data version 1.1, 1.2, 1.3
 */

/*  Notes for future Developers - 
 *    <no - note >
 */﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Xml;

namespace ProjectCompton
{
    class SymbolicPlanner
    {
        /* Class Description : Symbolic Planner provides utilities
         * for finding set of instructions from EnvA to EnvB */

        static List<Tuple<String, String, List<Instruction>>> cache = new List<Tuple<String, String, List<Instruction>>>();
		static int errorCounter = 1;
        public int cacheHit = 0, cacheMiss = 0, atomicCaseHit = 0, atomicCaseMiss = 0;
		private Process proc = null;

        public SymbolicPlanner(Logger lg)
        {
            /* Function Description : Returns the prelude */
			SymbolicPlanner.errorCounter = System.IO.Directory.GetFiles (Constants.rootPath + "Log/Error/").Count () + 1;

			this.proc = new Process ();
			if (Constants.usingLinux)
				proc.StartInfo.FileName = Constants.rootPath + @"SymbolicPlanner/PlannerLinux/Mp";
			else
			{
				proc.StartInfo.WorkingDirectory = Constants.cygwinPath;//Constants.rootPath + @"SymbolicPlanner/PlannerWindows/";
				proc.StartInfo.FileName = Constants.rootPath + @"SymbolicPlanner/PlannerWindows/Mp.exe";
			}
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;

			if (Constants.cacheBootStrapPlanOut) 
			{
				this.bootstrapCache ();
				Console.WriteLine ("Size of Loaded Cache " + SymbolicPlanner.cache.Count ());
			}
        }

        //Functionalities for using Cache
        private List<Instruction> findInCache(String init, String goal)
        {
            /* Function Description : Search for in cache, return the instructions if found */

            lock ("Cache Update")
            {
                foreach (Tuple<String, String, List<Instruction>> tmp in cache)
                {
					if (tmp.Item2.Equals(goal)&&tmp.Item1.Equals(init))//first condition rarely matches
                    {
                        List<Instruction> copy = new List<Instruction>();
                        foreach (Instruction inst in tmp.Item3)
                        {
                            Instruction inst_ = inst.makeCopy();
                            copy.Add(inst_);
                        }
                        cacheHit++;
                        return copy;
                    }
                }
                cacheMiss++;
            }
            return null;
        }

		private void bootstrapCache()
		{
			//Function Description: Bootstraps cache from file
			if(!System.IO.File.Exists(Constants.dataFolder+@"/cache_planner_output.xml"))
				return;
			/* Reads the file: Plan.xml and bootstraps the cache
				 * <root>
				 * 	<point> <init>...</init> <goal>...</goal> <instructions>
				 *   <instruction>..</instruction
				 *     .....
				 *  </instructions> </point>
				 * </root> */

			XmlTextReader reader = new XmlTextReader (Constants.dataFolder + @"/cache_planner_output.xml");
			String init = null, goal = null;
			List<Instruction> insts = new List<Instruction> ();

			while (reader.Read()) 
			{
				switch (reader.NodeType) 
				{
					case XmlNodeType.Element: // The node is an element.
					if (reader.Name.Equals ("init")) 
					{
						reader.Read ();
						init = reader.Value;
					}
					else if (reader.Name.Equals ("goal")) 
					{
						reader.Read ();
						goal = reader.Value;
					}
					else if (reader.Name.Equals ("instruction")) 
					{
						Instruction inst = new Instruction();
						reader.Read ();
						if (reader.Value.Equals ("Null")) 
							inst.setNameDescription ("Null", new List<string> ());
						else  inst.parse (reader.Value, null);
						insts.Add (inst);
					}
					break;
					case XmlNodeType. EndElement: //Display the end of the element.
					if (reader.Name.Equals ("point")) 
					{
						SymbolicPlanner.cache.Add (new Tuple<String,String,List<Instruction>> (init.ToString (), goal.ToString (), insts.ToList ()));
						insts.Clear ();
					}
					break;
				}
			}
			reader.Close ();
		}

		public void storeCache()
		{
			//Function Descriptions: Stores the cache in the xml file
			System.IO.StreamWriter sw = new System.IO.StreamWriter (Constants.dataFolder + @"/cache_planner_output.xml");
			StringBuilder sb = new StringBuilder();
			sb.Append ("<root>\n");
			foreach(Tuple<String,String,List<Instruction>> entry in SymbolicPlanner.cache)
			{
				sb.Append("<point>\n<init>"+entry.Item1+"</init>\n<goal>"+entry.Item2+"</goal>\n<instructions>\n");
				foreach(Instruction inst in entry.Item3)
					sb.Append("<instruction>"+inst.getName()+"</instruction>\n");
				sb.Append("</instructions>\n</point>");
			}
			sb.Append ("</root>\n");
			sw.Write(sb.ToString());
			sw.Flush();
			sw.Close();
		}

        private void add(String init, String goal, List<Instruction> add2)
        {
            /* Function Description : Search for in cache, return the instructions if found */

			String init_ = init.ToString ();
            String goal_ = goal.ToString();
            List<Instruction> copy = new List<Instruction>();
            foreach (Instruction inst in add2)
            {
                Instruction inst_ = inst.makeCopy();
                copy.Add(inst_);
            }

            lock ("Cache Update")
            {
                SymbolicPlanner.cache.Add(new Tuple<String, string, List<Instruction>>(init_, goal_, copy));
            }
        }

		private List<Instruction> ifAtomicCase(Environment env, String[] predicates)
		{
			/* Function Description: Checks if the given case is atomic
			 * and can be solved trivially. 
             * 1. Atomic Case: IsGrasping Robot object-Name
             *    Solution: If obj-Name exist and is graspeable then
             *                 output moveto(objname); grasp(objname) if far else grasp(objname);
             *              else return null
             * 2. Atomic Case: IsNear Robot object=Name */


			if (predicates.Length == 1) 
			{
				Tuple<bool, String> res = Global.getAtomic (predicates[0]);
				String[] words = res.Item2.Trim(new char[]{')','('}).Split (new char[] {' '});

				if(!res.Item1 && words.Length==3 && words[0].Equals("IsGrasping") && words[1].Equals("Robot"))
				{
					this.atomicCaseHit++;
					Object objF = env.findObject (words [2]);
					Instruction instgrasp = new Instruction ();
					instgrasp.setNameDescription ("grasp",new List<String>(){words[2]});
					if (objF == null || !objF.affordances_.Contains("IsGraspable"))
						return new List<Instruction>();
					if (env.checkRelExists ("Robot", words [1], SpatialRelation.Near)) 
						return new List<Instruction> () { instgrasp };
					else
					{
						Instruction instmove = new Instruction ();
						instmove.setNameDescription ("moveto",new List<String>(){words[2]});
						return new List<Instruction>(){instmove, instgrasp};
					}
				}
				else if(!res.Item1 && words.Length==3 && words[0].Equals("Near") && words[1].Equals("Robot"))
				{
					this.atomicCaseHit++;
					Object objF = env.findObject (words [2]);
					if (objF == null)
						return new List<Instruction>();
					Instruction instmove = new Instruction ();
					instmove.setNameDescription ("moveto",new List<String>(){words[2]});
					return new List<Instruction>(){instmove};
				}
			}
			this.atomicCaseMiss++;
			return null;
		}

		public static bool trivialUnsat(String constraints)
		{
			/* Function Description: To fasten and reduce use of planner, this algorithm checks
			 * if there is an obvious fallacy with with the constraints and return true if yes
			 * else false. Below are the obviously false constraints that cannot be satisfied:
			 * 1. if there exists p and not of p
			 * 2. two near constraints with different cstr
			 * 3. if it contains (rel x y1) and (rel x y2); we cannot have
			 *     x on/in y1 and also on/in y2. Though philosophically, it may be possible.
			 *     we dont have it in our dataset. And this is an optimization function. */

			int isgrasping = 0, isnear = 0;
			String[] predicates = constraints.Split(new char[]{'^'});
			List<Tuple<bool,string>> atoms = predicates.Select (x => Global.getAtomic (x)).ToList();
			List<String[]> words = atoms.Select (x => x.Item2.Split (new char[] {' '})).ToList();

			for (int i=0; i<predicates.Length; i++) 
			{
				for (int j=i+1; j<predicates.Length; j++) 
				{
					Tuple<bool,string> atom1 = atoms[i];
					Tuple<bool,string> atom2 = atoms[j];
					if(atom1.Item2.Equals(atom2.Item2) && (atom1.Item1 && !atom2.Item1 || !atom1.Item1 && atom2.Item1))
						return true;
				}
			}

			for (int i=0; i<predicates.Length; i++) 
			{
				if (words[i].Length != 3)
					continue;
				if (!atoms[i].Item1 && words[i][0].Equals ("Near"))
					isnear++;
				else if (!atoms[i].Item1 && words[i][0].Equals ("Grasping"))
					isgrasping++;
			}

			if (isnear > 1)
				return true;

			for (int i=0; i<predicates.Length; i++) 
			{
				if (words[i].Length != 3)
					continue;

				if (!atoms[i].Item1 && (words[i][0].Equals("In") || words[i][0].Equals("On"))) 
				{
					for (int j=i+1; j<predicates.Length; j++) 
					{
						if ((words[j][0].Equals("In") || words[j][0].Equals("On")) && !atoms[j].Item1 &&
						    words [i] [1].Equals (words [j] [1]) && !words [i] [2].Equals (words [j] [2]))
							return true;
					}
				}
			}

			return false;
		}

		public List<Instruction> satisfyConstraintsInInstruction(Environment env, List<Instruction> inst, Simulator sml, Logger lg)
		{
			//Function Description: Given a sequence, inst -- the algorithm finds constraints and then call the planner

			List<String> constraints = new List<String> ();
			Environment iterator = env.makeCopy ();

			foreach (Instruction inst_ in inst) 
			{
				Tuple<Double, String, String> result = sml.satSyntConstraints(inst_, iterator);
				if (result.Item3 == null) 
					return null;

				if (result.Item3.Length != 0) //unexectuable instructions
				{ 
					List<String> split = result.Item3.Split (new char[] { '^' }).ToList();
					constraints = constraints.Concat (split.ToList()).ToList();
					//change the environment
					foreach (String cstr in split) 
					{
						Tuple<bool,string> atom = Global.getAtomic ("("+cstr+")");
						iterator.modify (atom.Item2/*.Substring(1,atom.Item2.Length-2)*/, !atom.Item1);
					}
				}
				iterator = sml.execute(inst_, iterator, false, false);
			}

			String constraint = String.Join ("^", constraints.Distinct ());
			List<Instruction> ans = this.satisfyConstraints (env, constraint);
			lg.writeToFile("constraint is " + constraint);
			//if (ans == null)
			return ans;
		}

        public List<Instruction> satisfyConstraints(Environment env, String constraint)
        {
            /* Function Description : Given a starting environment E and constraint string of the form - 
             * predicate1^predicate2^.... where each predicate is of 4 type - 
             * (state objName stateName) (not (state objName stateName))
             * (relationship-type objName1 objName2) (not (relationship-type objName1 objName2)) */

            if (constraint.Length == 0)
            	return new List<Instruction>();

			if (SymbolicPlanner.trivialUnsat (constraint)) 
				return null;

            StringBuilder code = new StringBuilder();
            StringBuilder init = new StringBuilder();
            StringBuilder goal = new StringBuilder();
			code.Append ("(define (problem data_Nov-14-2014_1)\n");

            #region encode_the_object
            code.Append("(:objects ");
            int tab = 0;
			List<Object> objL = env.objects;
            for (int i = 0; i < objL.Count(); i++)
            {
                code.Append(objL[i].uniqueName + " ");
                if ((tab = (++tab) % 5) == 0)
                    code.Append("\n");
            }
            code.Append("In On)\n");
            #endregion

            #region encode_the_init
            init.Append("(:init \n");
            foreach (Object obj in objL)
            {
                List<Tuple<String, String>> stateList = obj.state_;
                foreach (Tuple<String, String> st in stateList)
                {
                    if (st.Item2.Equals("True")) //Needs improvement
                        init.Append(" (state " + obj.uniqueName + " " + st.Item1 + ")\n");
                    // else code.Append("(not (state " + obj.uniqueName + " " + st.Item1 + "))\n");
                }

                List<String> affordances = obj.affordances_;
                foreach (String affordance in affordances)
                    init.Append("(" + affordance + " " + obj.uniqueName + ")\n");
            }


			List<Tuple<Object, Object, SpatialRelation>> relationshipMatrix = env.relationshipMatrix;
            foreach (Tuple<Object, Object, SpatialRelation> rel in relationshipMatrix)
                init.Append("(" + rel.Item3 + " " + rel.Item1.uniqueName + " " + rel.Item2.uniqueName + ")");
            init.Append(")"); //close init
            #endregion

            #region encode_the_goal
            String[] cstr = constraint.Split(new char[] { '^' });
            goal.Append("(:goal (and ");
            for (int i = 0; i < cstr.Count(); i++)
            {
                if (cstr[i].Length > 0)
                    goal.Append(cstr[i]);
            }
            goal.Append("))"); //close and, goal
            #endregion

            /* Since calling a planner is a very costly hence I have added several 
			 * optimization: chiefly caching and handling atomic cases */
			List<Instruction> atomicCase = this.ifAtomicCase(env, cstr);
			if (atomicCase != null) 
				return atomicCase;
            List<Instruction> cacheResult = this.findInCache(init.ToString(), goal.ToString());
			if (cacheResult != null) 
			{
				if (cacheResult.Count () == 1 && cacheResult [0].getControllerFunction ().Equals ("Null")) 
					return null;
				return cacheResult;
			}

            //Write data in domains.pddl file
            System.IO.StreamWriter tw = null;
            if (Constants.usingLinux)
                tw = new System.IO.StreamWriter(Constants.rootPath + @"Log/domains" + Thread.CurrentThread.ManagedThreadId + ".pddl", false);
            else tw = new System.IO.StreamWriter(Constants.rootPath + @"Log\domains" + Thread.CurrentThread.ManagedThreadId + ".pddl", false);
            tw.WriteLine(code.ToString() + init.ToString() + goal.ToString() + ")");
            tw.Flush();
            tw.Close();

			//Console.WriteLine ("Finding the plan");
            /* Now call the symbolic planner Mp.exe or Mp
             * passing the arguments :  [domainknolwedge.pddl] [problem.pddl] */
            
			if (System.IO.File.Exists (Constants.rootPath + @"SymbolicPlanner/PlannerWindows/Mp.exe")
				|| Constants.usingLinux && System.IO.File.Exists (Constants.rootPath + @"SymbolicPlanner/PlannerLinux/Mp")) 
			{
				#region call_symbolic_planner
				String output = "", error = "";
                
				if (Constants.usingLinux)
					this.proc.StartInfo.Arguments = string.Format ("-r 12 -P 0 -m 4000 " + Constants.dataFolder+@"/Environment/domainKnowledge.pddl " + Constants.rootPath + "Log/domains" + Thread.CurrentThread.ManagedThreadId + ".pddl");
				else
					this.proc.StartInfo.Arguments = string.Format ("-r 10 -P 0 -m 4000 "+ Constants.dataFolder+@"/Environment/domainKnowledge.pddl "+Constants.rootPath+"Log/domains" + Thread.CurrentThread.ManagedThreadId + ".pddl");
				this.proc.Start ();
				this.proc.WaitForExit ();
				for (int i = 0; i < 100; i++)
					output = output + "\n" + this.proc.StandardOutput.ReadLine ();
				error = this.proc.StandardError.ReadLine ();
				#endregion 

				if (!output.Contains ("PLAN FOUND")) 
				{
					this.add (init.ToString (), goal.ToString (), new List<Instruction>(){new Instruction ("Null", new List<string> ())});
					Console.WriteLine("For strange reasons - no termination");
					System.IO.File.Copy(Constants.rootPath + @"Log/domains" + Thread.CurrentThread.ManagedThreadId + ".pddl",Constants.rootPath + @"Log/Error/domains_null_" + SymbolicPlanner.errorCounter+ ".pddl");
					SymbolicPlanner.errorCounter++;
					return null;
				}

				List<Instruction> outInstructions = this.decodeAll (env, output);
				bool validate = this.validate (outInstructions, env);
				if (error != null && error.Length != 0 || !validate) 
				{
					Console.WriteLine ("Planner failed with " + error+" validation "+validate.ToString()+ " error number "+SymbolicPlanner.errorCounter);
					System.IO.File.Copy(Constants.rootPath + @"Log/domains" + Thread.CurrentThread.ManagedThreadId + ".pddl",Constants.rootPath + @"Log/Error/domains" + SymbolicPlanner.errorCounter+ ".pddl");
					SymbolicPlanner.errorCounter++;
					this.add (init.ToString (), goal.ToString (), new List<Instruction>(){new Instruction ("Null", new List<string> ())});
					return null;
				}
				this.add (init.ToString (), goal.ToString (), outInstructions);
				return outInstructions;
			} 
			else throw new ApplicationException ("Calling Planner but Planner file does not exist");
        }

        //Functionalities for reading and decoding the output of the Symbolic Planner
        private List<Instruction> decodeAll(Environment env, String result)
        {
            /* Function Description : Given output of the symbolic planner
             * we convert it back into the sequence of instruction */
            List<Instruction> instructions = new List<Instruction>();
            String[] lines = result.Split(new char[] { '\n' });
            for (int i = 0; i < lines.Count(); i++)
            {
                if (lines[i].StartsWith("STEP"))
                {
                    int iter = lines[i].IndexOf(':');
                    String[] indivPredicate = Global.removeEmpty(lines[i].Substring(iter + 1).Split(new char[] { ' ' }));
                    for (int j = 0; j < indivPredicate.Count(); j++)
                    {
                        Instruction insta = decode(env, indivPredicate[j]);
                        if (insta != null)
                            instructions.Add(insta);
                    }
                }
            }

            return instructions;
        }

        private Instruction decode(Environment env, String oneString)
        {
            /* Function Description : Decodes a single instruction
             * This function takes the data in STRIPS format and 
             * convert them into C# Instruction Class object. 
             * For this, we do extensive one-one mapping for every STRIPS
             * predicate */

            //Each predicate has the following form PredicateName(Arg1,Arg2,.....,Argn)
            String[] token = oneString.Split(new char[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);

            String actionName = token[0];
            List<String> arguments = new List<String>();

			if (token [0].StartsWith ("press") || token [0].StartsWith ("turn") || token [0].StartsWith ("open") || token [0].StartsWith ("close") || token [0].StartsWith ("add") || token [0].StartsWith ("place")) 
			{
				int first_ = token [0].IndexOf ('_');
				actionName = token [0].Substring (0, first_);
				arguments.Add (Global.firstCharToUpper (token [0].Substring (first_ + 1)));
				for (int i = 1; i < token.Count(); i++)
					arguments.Add (Global.firstCharToUpper (token [i]));
			}
			else if (token [0].StartsWith ("keep") && !token [0].Equals ("keep")) 
			{
				int first_ = token [0].IndexOf ('_');
				actionName = token [0].Substring (0, first_);
				int second_ = token [0].Substring (first_ + 1).IndexOf ('_');
				arguments.Add (Global.firstCharToUpper (token [1]));
				arguments.Add (Global.firstCharToUpper (token [0].Substring (first_ + 1, second_)));
				arguments.Add (Global.firstCharToUpper (token [0].Substring (first_ + second_ + 2)));
			}
			else
			{
				for (int i = 1; i < token.Count(); i++)
					arguments.Add(Global.firstCharToUpper(token[i]));
			}
		

            for (int i = 0; i < arguments.Count(); i++)
            {
                foreach (Object obj in env.objects)
                {
                    if (obj.uniqueName.Equals(arguments[i], StringComparison.OrdinalIgnoreCase))
                        arguments[i] = obj.uniqueName;
                }
            }

            return new Instruction(actionName, arguments);
        }

		private bool validate(List<Instruction> inst, Environment env)
		{
			/* Function Description: Instruction sequnece returned by 
			 * the planner may contain objects that were hard-coded in
			 * the planner but dont really exist in the given environment.
			 * In future, I plan to resolve this by adding (Exist ObjName)
			 * condition but for now I am just adding this function to return
			 * false if this is the case */

			foreach (Instruction inst_ in inst) 
			{
				List<String> objects = inst_.returnObject ();
				foreach (String objName in objects)
				{
					if (env.findObject (objName) == null)
						return false;
				}
			}
			return true;
		}
    }
}
