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

namespace ProjectCompton
{
    class VerbProgram
    {
        /* Class Description : Each verb is mapped to set of controller programs.
         * This class handles this structure and objects of this class are learned.*/

        public String verbName {private set; get;} //name of the verb example : boil, ignite, place etc.
        public List<LexicalEntry> program {private set; get;}
		public List<Tuple<String,int>> predicateFreq = new List<Tuple<string, int>> ();//number of times a predicate is fired

        public VerbProgram(String verbName)
        {
            /*Constructor Description : initialize verb name*/
            this.verbName = verbName;
            this.program = new List<LexicalEntry>();
        }

        public string getName()
        {
            /*Function Description : Returns name of the verb*/
            return this.verbName;
        }

        public string fileNameString(int i)
        {
            /*Function Description : Return file Name String at i^th pos*/
            return "( Entry "+ this.program[i].entryIndex + " )";
        }

        public List<LexicalEntry> getProgram()
        {
            /*Function Description : Returns the program*/
            return this.program;
        }

		public String neutralize(String predicate)
		{
			/*Function Description: Predicate */
			Tuple<bool,string> pred = Global.getAtomic (predicate);
			//pred.Item1 is of 2 type
			String[] words = pred.Item2.Split (new char[] { ' ' });
			String ret = "";
			if (words [0].Equals ("state")) 
				ret = words [0] + " x " + words [2];
			else ret = words [0] + " x y";
			if (pred.Item1)
				return "(not (" + ret + "))";
			else
				return "(" + ret + ")";
		}

        public void add(LexicalEntry vtmp)
        {
            /* Function Description: Does the following - 
             * 1. Add the new template to the list
             * 2. Changes the frequency table */
            this.program.Add(vtmp);
			foreach (String pred_ in vtmp.predicatesPost) 
			{
				String pred = this.neutralize(pred_);
				int index = this.predicateFreq.FindIndex (x => x.Item1.Equals (pred));
				if (index != -1) 
					this.predicateFreq [index] = new Tuple<String,int> (this.predicateFreq [index].Item1, this.predicateFreq [index].Item2 + 1);
				else
					this.predicateFreq.Add (new Tuple<String, int> (pred, 1)); 
			}
        }

		public int fetchFrequency(String predicate)
		{
			/* FunctionDescription: Given a predicate, this function
			 * fetches the frequency of the predicate from the predicate list */
			String neutral = this.neutralize (predicate);
			foreach (Tuple<String,int> elem in this.predicateFreq) 
			{
				if (elem.Item1.Equals (neutral)) 
					return elem.Item2;
			}
			return 0;//no evidence
		}

		public int totalFrequency()
		{
			/* Function Description: Returns the total frequency of all predicates,
			 * mostly used for normalizaton. */
			int freqZ = 0;
			foreach (Tuple<String,int> elem in this.predicateFreq) 
				freqZ = freqZ + elem.Item2;
			return freqZ;
		}

        public void display(Logger lg)
        {
            // Function Description : Display the learned verb program
            lg.writeToFile("<div><button onclick='show(this)'>" + this.verbName + " (Total Count " + this.program.Count() + ")</button><div style='display:none'><br/>");
            for (int i = 0; i < this.program.Count();i++)
            {
                lg.writeToFile("<div><button onclick='show(this)'> Program Instance " + i.ToString() + " </button> ");
                this.program[i].display(lg);
                lg.writeToFile("</div>");
            }
			lg.writeToFile ("<b>Predicate Frequency Table</b><table>");
			foreach (Tuple<String,int> predF in this.predicateFreq) 
				lg.writeToFile ("<tr><td>"+predF.Item1+"</td><td>"+predF.Item2+"</td></tr>");
			lg.writeToFile("</table></div></div><br/><br/>"); //verbProgram ends
        }
    }
}
