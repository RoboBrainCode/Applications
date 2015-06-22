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
	public enum MetricList
	{
		LV,
		uWEED,
		WEED,
		END
	}

    class Metrics
    {
        /* Class Description : Defines functions which are used to evaluate the accuracy
         * of the algorithm. These are syntactical metrics like LV or environment based semantic
         * metric like uWEED or WEED.*/

        Tester testObj = null;
		public static int numMetrics = Enum.GetNames(typeof(MetricList)).Length;

        public Metrics(Tester testObj)
        {
            this.testObj = testObj;
        }

        private List<Environment> giveEnvList(Environment start, List<Instruction> inst)
        {
            /* Function Description : Return list of environment starting with start
             * and obtained by apply instruction I. */
            List<Environment> answer = new List<Environment>() { start };
            Environment iterator = start.makeCopy();

            for (int i = 0; i < inst.Count(); i++)
            {
                iterator = this.testObj.sml.execute(inst[i],iterator,true); 
                answer.Add(iterator);
            }
            return answer;
        }

        private List<Environment> trimEnvList(List<Environment> original, List<String> cover)
        {
            /* Function Description : Trim the env-list by collapsing all same environment
             * to a single copy. */

            List<Environment> newList =new List<Environment>();
            int iter = -1;
            for (int i = 0; i < original.Count(); i++)
            {
                if (iter == -1)
                    iter = i;
                else
                {
                    /* check if original[i] is same as original[iter] on the cover
                     * if yes then do not add else add it and update the iter to i*/
                    if(!original[i].envEqualOn(original[iter],cover))
                    {
                        iter=i;
                        newList.Add(original[i]);
                    }
                }
            }
            return newList;
        }

        private List<String> giveCover(List<Instruction> inst)
        {
            /* Function Description : Returns list of relevant objects or cover.
             * A cover for a task is defined as a set of objects whose state
             * values influence the success of the given task
			 * Currently using - find union of all objects in the instruction sequence */

            List<String> cover = new List<String>();
            foreach (Instruction insta in inst)
                cover = cover.Concat(insta.returnObject()).ToList();
            return cover.Distinct().ToList();
        }

        public double levenshtein(List<Instruction> inst1, List<Instruction> inst2)
        {
            /* Function Description : Returns the levenshtein distance between the sequence */
		
            int m = inst1.Count(), n = inst2.Count();
            int[,] lev = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (Math.Min(i, j) == 0)
                    {
                        lev[i, j] = Math.Max(i, j);
                        continue;
                    }

                    int cost = 0;
					if (!inst1 [i - 1].compare (inst2 [j - 1]))
						cost = 1;
                    int a = lev[i - 1, j] + 1;
                    int b = lev[i, j - 1] + 1;
                    int c = lev[i - 1, j - 1] + cost;
                    lev[i, j] = Math.Min(a, Math.Min(b, c));
                }
            }
            return (double)lev[m, n];
        }

        public Tuple<Double,String> unweightedEED(Environment env, List<Instruction> instg, List<Instruction> instout)
        {
            /* Function Description : Returns the unweighted Environment Edit Distance.
             * By definition this distance is asymetric hence is not a metric. We have
             * EED(x,x)=0 though. Also the larger the distance the worse is the algorithmic
             * performance. We have EED(i,j) in {0,1,...,i}
             * Its define by the following recursion : 
             * 
             * L[i,j] = min { (i!=j) + [i+1,j], [i,j+1] }
             * L[i,n] = m-i
             * L[m,j] = 0 */

            List<Environment> e1 = this.giveEnvList(env, instg);
            List<Environment> e2 = this.giveEnvList(env, instout);
			//Compute cover
			List<String> cover = this.giveCover(instg);

            String coverString = "";
            foreach (String cvr in cover)
                coverString = coverString + cvr + " , ";

            //trim the list
            //e1 = this.trimEnvList(e1, cover);
            //e2 = this.trimEnvList(e2, cover);

            int m = e1.Count(), n = e2.Count();
            double[,] table = new double[m + 1, n + 1];
            string[,] log = new string[m + 1, n + 1];

            for (int i = m; i >= 0; i--)
            {
                for (int j = n; j >= 0; j--)
                {
                    if (i == m || j == n)
                    {
                        table[i, j] = m - i;
                        log[i,j] = "";
                    }
                    else
                    {
                        //check if e1[i] and e2[j] are equal
                        bool result = e1[i].envEqualOn(e2[j], cover);
                        if (result)
                        {
                            table[i, j] = Math.Min(table[i + 1, j], table[i, j + 1]);
                            if (table[i, j] == table[i + 1, j])
                                log[i, j] = "E" + i + " -> E" + j + "; " + log[i + 1, j];
                            else log[i, j] = log[i, j + 1];
                        }
                        else
                        {
                            table[i, j] = Math.Min(1 + table[i + 1, j], table[i, j + 1]);
                            if (table[i, j] == 1 + table[i + 1, j])
                                log[i, j] = "Leaving E" + i + "; " + log[i + 1, j];
                            else log[i, j] = log[i, j + 1];
                        }
                    }
                }
            }

            return new Tuple<Double, String>(table[0, 0] / (double)Math.Max(m, 1), " Cover : " + coverString + " = " + log[0, 0]);
        }

        public double weightedEED(Environment env, List<Instruction> inst1, List<Instruction> inst2)
        {
            /* Function Description : Returns the unweighted Environment Edit Distance.
             * By definition this distance is asymetric hence is not a metric. We have
             * EED(x,x)=0 though. Also the larger the distance the worse is the algorithmic
             * performance. We have EED(i,j) in {0,1,...,i}
             * Its define by the following recursion : 
             * 
             * L[i,j] = min { Delta(i,j) + [i+1,j], [i,j+1] }
             * L[i,n] = m-i
             * L[m,j] = 0
             */
            List<Environment> e1 = this.giveEnvList(env, inst1);
            List<Environment> e2 = this.giveEnvList(env, inst2);

            int m = e1.Count(), n = e2.Count();
            double[,] table = new double[m + 1, n + 1];

            //Compute cover
            List<String> cover = this.giveCover(inst1);

            for (int i = m; i >= 0; i--)
            {
                for (int j = n; j >= 0; j--)
                {
                    if (i == m || j == n)
                    {
                        table[i, j] = m - i;
                    }
                    else
                    {
                        //check if e1[i] and e2[j] are equal
                        double score = e1[i].envDistanceOn(e2[j],cover);
                        table[i, j] = Math.Min(score + table[i + 1, j], table[i, j + 1]);
                    }
                }
            }
            return table[0, 0] / (double)Math.Max(m, 1);
        }

		public Tuple<Double,String> endStateMatch(Environment env, List<Instruction> inferred, List<Instruction> groundtruth)
		{
			// Function Description: Returns 1 if the environments in the end state match else return 0
			Environment e1 = this.testObj.sml.executeList (inferred, env, true);
			Environment e2 = this.testObj.sml.executeList (groundtruth, env, true); 
			List<String> fp1 = e1.difference(env); //flipped predicate for ground-truth
			List<String> fp2 = e2.difference(env); //flipped predicate for ground-truth

			//this Metric, computes F1 Score of fp1 and fp2
			double precision = 0, recall = 0; 
			foreach (String fp_ in fp1) 
			{
				if (fp2.Contains (fp_))
					precision++;
			}
			precision = precision/Math.Max(fp1.Count(),1);

			foreach (String fp_ in fp2) 
			{
				if (fp1.Contains (fp_))
					recall++;
			}
			recall = recall/Math.Max(fp2.Count(),1);

			double f1score = precision * recall / Math.Max (precision + recall, Constants.epsilon);

			return Global.jaccardIndex (fp1, fp2);
			//return new Tuple<double, string>(f1score, ""); //Global.jaccardIndex (fp1, fp2);//return the Jaccard Index of these two list of strings
		}

    }
}
