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
    class Template
    {
        /* Class Description : Defines templates which are used by the baselines
         * relying on manually constructed templates */

        public String verbName = null;
		public List<String> variables{ get; private set; }
		private String logicalform = null;
		private List<Tuple<String, String, String>> relationship;

        public Template()
        {
            variables = new List<string>();
            relationship = new List<Tuple<string, string, string>>();
        }

        public String instantiate(double[,] leCorrMatrix, Environment env)
        {
        	/* Function Description: Given a nxm matrix where n is same as variables
			 * in this template and m is the set of objects to map to. This algorithm
			 * finds good initialization for the n variables. Currently, we pick the
			 * best match for each m.*/

			List<String> predicateStrings = this.logicalform.Split (new char[] { '^' }).ToList();
			List<bool> booleans = new List<bool> ();
			List<string[]> predicates = new List<string[]> ();

			foreach (String predicateString in predicateStrings) 
			{
				if (predicateString.Length == 0) 
				{
					Console.WriteLine ("Logical Form Empty " + this.logicalform);
					continue;
				}
				Tuple<bool,string> atom = Global.getAtomic (predicateString);
				booleans.Add (atom.Item1);
				predicates .Add(atom.Item2.Split (new char[] { ' ' }).ToArray ());
			}

			Console.WriteLine ("Variables "+ this.variables.Count()+" Instantiate " + leCorrMatrix.GetLength (0) + " and " + leCorrMatrix.GetLength (1));
			for (int i=0; i<this.variables.Count(); i++) 
			{
				int maxindex = 0;
				for (int obj=1; obj<env.objects.Count(); obj++) 
				{
					if (leCorrMatrix [i, obj] > leCorrMatrix [i, maxindex])
						maxindex = obj;
				}

				foreach (String[] predicate in predicates) 
				{
					Console.WriteLine ("Predicate Length " + predicate.Length);
					for (int j=1; j<predicate.Length; j++) 
					{
						if(predicate[j].Equals(this.variables[i]))
							predicate [j] = env.objects [maxindex].uniqueName;
					}
				}

				//perform plurality expansion
			}

			List<String> instantiated = predicates.Select (x => "(" + String.Join (" ", x) + ")").ToList ();
			Console.WriteLine ("instantiated " + instantiated.Count () + " vs " + booleans.Count ());

			for (int j=0; j<booleans.Count(); j++) 
			{
				if (booleans [j])
					instantiated [j] = "(not " + instantiated [j] + ")";
			}

			return String.Join ("^", instantiated);
        }

		public int fitscore(Clause cls)
		{
			/* Function Description: Given a test-time sentence context captured in Clause cls.
			 * We assign a score to this template, as to how good does this template captures
			 * the context. This score is given by the number of relationship in this template
			 * that the clause satisfies. Better approaches for doing the same needs to be explored.*/

			int match = 0;
			foreach (Tuple<String,String,String> rel in this.relationship) 
			{
				int i = this.variables.IndexOf (rel.Item1);
				int j = this.variables.IndexOf(rel.Item2);
				if (i == -1 || j == -1)
					continue;
				if (cls.relation [i, j].Equals (rel.Item3))
					match++;
			}
			return match;
		}

        public void parse(String lexicon, String relinfo)
        {
            /* Function Description: Parses the templates from a file. The parsing format is given by -
             * verb-name1 variables  ->  logical-form
             * r1 x1 y1; r2 x2 y2; ... */

			int index = lexicon.IndexOf ("->");
			if (index == -1)
				throw new ApplicationException ("Templates are not in word -> lexicon; format \n"+lexicon);

			String[] verbinfo = lexicon.Substring (0, index).Split(new char[]{' '}).Select(x=>x.Trim()).ToArray();
			this.verbName = verbinfo [0];
			for (int i=1; i<verbinfo.Length; i++)
				this.variables.Add (verbinfo[i]);

			this.logicalform = lexicon.Substring (index + 2).Trim();
			Console.WriteLine ("New Template verb: " + this.verbName + "\n Variables " + String.Join (",", this.variables)+"\n Logical Form "+this.logicalform);
			relinfo = relinfo.Trim ();
			if (relinfo.Length == 0)
				return;

			List<String> relations = relinfo.Split(new char[]{';'}).ToList();
			foreach (String relation in relations) 
			{
				String[] words = relation.Split (new char[] { ' ' }).ToArray ();
				if(words.Length!=3)
					throw new ApplicationException ("Templates are not in required format "+relation+" length "+relation.Length+" overall relinfo "+relinfo+" len "+relinfo.Length);
				//if (!this.variables.Exists (x=>x.Equals(words [1])) || !this.variables.Exists (x=>x.Equals(words [2])))
				//	throw new ApplicationException ("Template relationship using variables that dont exist - \n" + lexicon + "\n" + relinfo);

				this.relationship.Add (new Tuple<string, string, string> (words[0], words[1], words[2]));
			}
        }
    }
}
