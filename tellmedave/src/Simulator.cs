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
using System.IO;
using System.Threading.Tasks;

namespace ProjectCompton
{
    class Simulator
    {
        /* Class Description : Provides functionalities  for simulator */

        public List<Tuple<String, List<String>, Expression>> preConditions = null;
        public List<Tuple<String, List<String>, Expression>> effect = null;

        public Simulator()
        {
            /* Constructor Description: Initializes the preConditions and effect
             * data-structures by reading the domainKnowledge.pddl file */

            this.preConditions = new List<Tuple<String, List<String>, Expression>>();
            this.effect = new List<Tuple<String, List<String>, Expression>>();
			String[] lines = null; 
			if (Constants.usingLinux)
			{
                //Console.WriteLine("dataFolder: "+Constants.path);
                lines = System.IO.File.ReadAllLines (Constants.dataFolder + "/Environment/domainKnowledge.pddl");
            }
			else lines = System.IO.File.ReadAllLines (Constants.dataFolder + @"\Environment\domainKnowledge.pddl");

            String actionName = null;
            List<String> variables = new List<string>();

            for (int i = 0; i < lines.Count(); i++)
            {
                if (lines[i].StartsWith("(:action"))
                {
                    String tmp = lines[i].Substring("(:action".Length);
                    int col = tmp.IndexOf(':');
                    int parenOpen = tmp.IndexOf('(');
                    int parenClose = tmp.IndexOf(')');
                    actionName = tmp.Substring(0, col).Trim();
                    variables = tmp.Substring(parenOpen + 1, parenClose - parenOpen - 1).Split(new char[] { ' ' }).ToList();
                    if (variables[0].Length == 0)
                        variables.RemoveAt(0);
                }

                if (lines[i].StartsWith(":precondition"))
                {
                    String precondition = "";
                    while (!lines[i].StartsWith(":effect"))
                    {
                        precondition = precondition + lines[i];
                        i++;
                    }
                    Expression exp = new Expression(precondition);
                    this.preConditions.Add(new Tuple<String, List<String>, Expression>(actionName, variables, exp));
                }

                if (lines[i].StartsWith(":effect"))
                {
                    String effect = "";
					while (i < lines.Count() && lines[i].Trim().Count() != 0)
                    {
                        effect = effect + lines[i];
                        i++;
                    }
                    Expression exp = new Expression(effect);
                    this.effect.Add(new Tuple<String, List<String>, Expression>(actionName, variables, exp));
                }
            }

            /* Special Cases
             * Some actions may have empty preconditions or effect
             * which are not written in the planner. We handle them here */

            this.preConditions.Add(new Tuple<string, List<string>, Expression>("wait", new List<String>(), new Expression("()")));
            this.effect.Add(new Tuple<string, List<string>, Expression>("wait", new List<string>(), new Expression("()")));
            this.preConditions.Add(new Tuple<string, List<string>, Expression>("moveto", new List<string>() { "?o" }, new Expression("()")));
        }

        public Tuple<String, List<String>> getInstructionAlias(Instruction inst)
        {
            /* Function Description: For coding reasons, instructions are often
             * converted to alias. Example - instruction Keep cup on sink 
             * is converted to Keep_on_sink cup. */

            String cf = inst.getControllerFunction();
            List<String> dscp = inst.getArguments();

            if (cf.Equals("press") || cf.Equals("open") || cf.Equals("close") || cf.Equals("turn"))
                return new Tuple<string, List<string>>(cf + "_" + dscp[0], new List<String>());
            else if (cf.Equals("add") || cf.Equals("place"))
                return new Tuple<string, List<string>>(cf + "_" + dscp[0], new List<String>() { dscp[1] });
            else if (cf.Equals("keep"))
            {
                foreach (Tuple<String, List<String>, Expression> pc in this.preConditions)
                {
                    if (pc.Item1.StartsWith("keep"))
                    {
                        int first_ = pc.Item1.IndexOf('_');
                        if (first_ == -1)
                            break;
                        int second_ = pc.Item1.Substring(first_ + 1).IndexOf('_');
                        if (second_ == -1)
                            break;
                        second_ = second_ + first_;
                        String first = pc.Item1.Substring(first_ + 1, second_ - first_);
                        String second = pc.Item1.Substring(second_ + 2);
                        if (first.Equals(dscp[1]) && second.Equals(dscp[2]))
                            return new Tuple<string, List<string>>(pc.Item1, new List<String>() { dscp[0] });
                    }
                }
            }

            return new Tuple<string, List<string>>(cf, dscp);
        }

		public List<String> getAffordance(Instruction inst, int which)
		{
			/* Function Description: Given instruction inst, find the 
			 * set of affordances that are satisfied by the description dscp eg: $1,
			 * in the preconditions and effect of inst
			 *  */

			List<String> affordances = new List<String>();
			List<String> dscpCover = new List<String>();
			List<String> dscpL = inst.getArguments();

			foreach(Tuple<String, List<String>, Expression> pc in this.preConditions)
			{
				if(pc.Item1.Equals(inst.getControllerFunction()) && pc.Item2.Count == dscpL.Count())
				{
					dscpCover = dscpCover.Concat (pc.Item3.getExpressionCover(pc.Item2[which])).ToList();
					break;
				}
			}

			foreach(Tuple<String, List<String>, Expression> ef in this.effect)
			{
				if(ef.Item1.Equals(inst.getControllerFunction()) && ef.Item2.Count == dscpL.Count())
				{
					dscpCover = dscpCover.Concat (ef.Item3.getExpressionCover(ef.Item2[which])).ToList();
					break;
				}
			}

			//Find affordances from the cover list
			foreach (String elem in dscpCover) 
			{
				String[] words = elem.Split (new char[]{' '});
				if (words.Length == 2)
					affordances.Add (elem);
			}

			return affordances; //variable has to satisfy these affordances
		}

        public Tuple<double, string, string> satSyntConstraints(Instruction inst, Environment present)
        {
            /* Function Description: Given an instruction and the environment. Returns whether
             * the instruction can be executed on the given environment. Presently returning a : 
             * 
             * Param 1 : 0-1 score with 1 meaning the given instruction is not valid and 0 means it is valid.
             * The modified score should take into account various other features like distance from the object.
             * Param 2 : Return the log string
             * Param 3 : Return the STRIPS predicate as to conditions that need to be satisfied for it to make sense
             * */

            Tuple<String,List<String>> alias = this.getInstructionAlias(inst);
            String res = null;
            foreach (Tuple<String, List<String>, Expression> pc in this.preConditions)
            {
                if (pc.Item1.Equals(alias.Item1, StringComparison.OrdinalIgnoreCase))
                {
                    List<Tuple<String, String>> map = new List<Tuple<String, String>>();//define the map
                    if (alias.Item2.Count() != pc.Item2.Count()) //overloading is allowed in pddl rules
                        continue;
                    for (int i = 0; i < alias.Item2.Count(); i++)
                        map.Add(new Tuple<string, string>(pc.Item2[i], alias.Item2[i]));
                    res = pc.Item3.evaluate(present, map);
                    break;
                }
            }

            if(res == null) //irrepairable failure
               return new Tuple<double, string, string>(0,null,null);
            else if(res.Equals("")) //satisfied
                return new Tuple<double, string, string>(0, "", "");
            else return new Tuple<double, string, string>(1, "", res);
        }

		public Environment execute(Instruction inst, Environment env, bool force = false, bool copy=true)
		{
			/* Function Description: Execute an instruction on the environment and
             * returns the final environment. It first checks for preconditions unless force is true.
			 * Otherwise if preconditions fail then null is returned. The output is given by 
			 * evaluating simulator. */

			if (!force) //if the execution is forced then it won't check for precondition satisfaction
			{ 
				if (this.satSyntConstraints (inst, env).Item3 == null || this.satSyntConstraints (inst, env).Item1 == 1) 
				{
					if(copy)
						return env.makeCopy ();
					return env;
				}
			}

			Environment present =  null;
			if (copy) //If copy is true then copy the environment
				present = env.makeCopy ();
			else present = env;

			//Console.WriteLine ("Instruction is " + inst.getControllerFunction () + " and " + String.Join (",", inst.getArguments ()));
			Tuple<String,List<String>> alias = this.getInstructionAlias(inst);
			//Console.WriteLine ("Alias " + alias.Item1 + " and " + String.Join(" ",alias.Item2));
			bool found = false;
			foreach (Tuple<String, List<String>, Expression> eff in this.effect)
			{
				if (eff.Item1.Equals(alias.Item1, StringComparison.OrdinalIgnoreCase))
				{
					//Console.WriteLine ("Matched with var-" + String.Join (",", eff.Item2));
					List<Tuple<String, String>> map = new List<Tuple<String, String>>();//define the map
					if (alias.Item2.Count() != eff.Item2.Count()) //overloading is allowed in pddl rules
						continue;
					found = true;
					for (int i = 0; i < alias.Item2.Count(); i++)
						map.Add(new Tuple<string, string>(eff.Item2[i], alias.Item2[i]));
					eff.Item3.modify(present, map);
					break;
				}
			}

			if (!found)
				throw new ApplicationException ("Cannot parse "+inst.getName());

			return present;
		}

		public Environment executeList(List<Instruction> inst, Environment env, bool force=true)
		{
			/* Function Descriptions: Executes these instructions on env environment
			 * and returns the final environment */
			Environment copy = env.makeCopy ();

			for (int i=0; i<inst.Count(); i++)
				copy = this.execute(inst[i],copy,force);
			return copy;
		}

    }
}
