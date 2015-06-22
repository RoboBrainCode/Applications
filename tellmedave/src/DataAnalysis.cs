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
using System.Xml;
using System.IO;
using System.Threading.Tasks;

namespace ProjectCompton
{
    class DataAnalysis
    {
        /* Class Description : This class describes functions that analyzes the data
         * like finds number of verbs, finds how many sentences have unseen verb for what
         * fraction of data etc.
         */

        private Parser obj = null;
		private List<Environment> envList = null;
		private Logger lg = null;

        public DataAnalysis(Parser obj, List<Environment> envList, Logger lg)
        {
            this.obj = obj;
			this.envList = envList;
			this.lg = lg;
        }

        public List<String> returnAllVerbs()
        {
            /* Function Description : Analyzes the data and reports number of verbs */
            List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> data = obj.returnAllData();
            List<String> listVerbs = new List<string>();

            for (int i = 0; i < data.Count(); i++)
            {
                Clause iter = data[i].Item2;
				if (iter == null)
					throw new ApplicationException ("Null clauses Data Point "+i);
                List<Clause> cls = iter.returnEventClause(); //each clause is one verb 
                if (cls == null)
                    continue;
                foreach (Clause cl in cls)
                {
					if (cl == null)
						throw new ApplicationException ("Child clause is null at Data Point "+i);
					string verb = cl.verb.getName();
                    if (!Global.isPresent(listVerbs, verb))
                        listVerbs.Add(verb);
                }
            }
            return listVerbs;
        }

        public List<int> unseenVerbs()
        {
            /* Function Description : Breaks the data into fraction and analyzes the number of unseen verb in one
             * half based on the other */
            List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> data = obj.returnAllData();
            List<int> unseen = new List<int>();

            double[] fractions = new double[] { 0.25, 0.35, 0.40, 0.50, 0.55, 0.60, 0.65, 0.70, 0.75, 0.80, 0.85, 0.90 };
            foreach (double fraction in fractions)
            {
                List<String> listVerbs = new List<string>();
                int numUnseen = 0;
                for (int i = 0; i < data.Count(); i++)
                {
                    Clause iter = data[i].Item2;
                    List<Clause> cls = iter.returnEventClause(); //each clause is one verb 
                    if (cls == null)
                        continue;
                    foreach (Clause cl in cls)
                    {
                        string verb = cl.verb.getName();
                        if (!Global.isPresent(listVerbs, verb))
                        {
                            if (i < fraction * data.Count())
                                listVerbs.Add(verb);
                            else
                            {
                                numUnseen++;
                            }
                        }
                    }
                }
                unseen.Add(numUnseen);
            }

            return unseen;
        }

        public void analyze()
        {
            /* Function: Does analysis on the dataset and saves the result in an interative form in a file */
			if (!Constants.analyze)
				return;
			//number of verbs in the data
            List<String> numVerbs = this.returnAllVerbs();
			//number of unseen verbs vs fraction of training data
            List<int> unseen = this.unseenVerbs();

			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = obj.returnAllData ();
			//compute average length of action sequence
			double avgLenActSeq = datas.Aggregate (0.0, (acc,x) => acc + x.Item3.Count ())/(double)Math.Max(datas.Count(),1);
			//compute average length of action sequence
			double avgLenSen = datas.Aggregate (0.0, (acc,x) => acc + x.Item2.getSubTreeSentence ().Split(new char[]{' ',','}).Length)/(double)Math.Max(datas.Count(),1);
			//compute average number of objects in the environment
			double avgObjects = this.envList.Aggregate (0.0, (acc,x) => acc + x.objects.Count ()) / (double)Math.Max (this.envList.Count (), 1);

			lg.writeToFile ("Analyis of the data: <br/> <table>"+
			                "<tr><td>Number of Verbs</td><td>"+numVerbs.Count()+"</td></tr>"+
			                "<tr><td>Unseen Verbs</td><td>"+String.Join(", ",unseen)+"</td></tr>"+
			                "<tr><td>Avg Len of Sequences</td><td>"+avgLenActSeq+"</td></tr>"+
			                "<tr><td>Avg Len of Sentence</td><td>"+avgLenSen+"</td></tr>"+
			                "<tr><td>Avg Objects</td><td>"+avgObjects+"</td></tr>"+
			                "</table>");

			int uniqueSentences = 0;
			for (int i=0; i<this.obj.rawData.Count(); i++) 
			{
				bool isUnique = true;
				String sentence = this.obj.rawData [i].Item2;
				for (int j=0; j<i; j++) 
				{
					if (sentence.Equals (this.obj.rawData [j].Item2)) 
					{
						isUnique = false;
						break;
					}
				}
				if (isUnique)
					uniqueSentences++;
			}

			Console.WriteLine ("Unique Sentences " + uniqueSentences + "out of " + this.obj.rawData.Count () + ". Press any key to continue");
			return;
        }
        
    }
}
