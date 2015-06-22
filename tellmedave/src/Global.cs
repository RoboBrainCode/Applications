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
    class Global
    {
        /*Class Description : Contains a list of static functions*/

        public static List<String> conditionIf = new List<string>() { "if", "when" };
        public static List<String> conditionFor = new List<string>() { "for" };
        public static List<String> conditionUntil = new List<string>() { "until" };
        public static List<String> conditionElse = new List<string>() { "else", "otherwise" };
        public static List<String> conditionAfter = new List<string>() { "after" };

        internal static string base_(String objName)
        {
            /* Function Description: returns the base name of the objName.
             * Ex: Mug1 returns Mug, Cup12 returns Cup. Ramen_1 returns Ramen */
            int i = -1;
            for (i = objName.Length - 1; i >= 0; i--)
            {
                char c = objName[i];
                if (48 <= c && c <= 57 || c == '_') //if its a number or _
                    continue;
                break;
            }
            return Global.firstCharToUpper(objName.Substring(0, i + 1));
        }

        internal static string firstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return "";
                throw new ArgumentException("ARGH!");
            }
            return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
        }

        internal static string[] removeEmpty(String[] ar)
        {
            /* Function Description : Remove the empty lines*/
            int len = ar.Count(), count = 0;
            List<int> notEmpty = new List<int>();
            for (int i = 0; i < len; i++)
            {
                if (!Global.isEmpty(ar[i]))
                    notEmpty.Add(i);
            }

            String[] arNew = new String[notEmpty.Count()];
            foreach (int i in notEmpty)
            {
                arNew[count] = ar[i];
                count++;
            }
            notEmpty.Clear();
            return arNew;
        }

        internal static bool isPresent(List<string> dscps, string dscp)
        {
            /*Function Description : checks if dscp exists in dscps*/
            foreach (String tmp in dscps)
            {
                if (dscp.Equals(tmp, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        internal String intersperse(List<String> words, String word)
        {
            /* Function Description : Intersperse's a list of string with word*/
            String cummulative = "";
            for (int i = 0; i < words.Count(); i++)
            {
                if (i < words.Count() - 1)
                    cummulative = cummulative + words[i] + word;
                else cummulative = cummulative + words[i];
            }
            return cummulative;
        }

        internal static double variance(Double[] d)
        {
            /*Function Description : find varinace of d*/
            double avg = d.Average();
            double sum = 0;
            for (int i = 0; i < d.Count(); i++)
            {
                sum = sum + d[i] * d[i];
            }
            return Math.Sqrt(sum / d.Count() - avg * avg);
        }

        internal static bool isEmpty(String str)
        {
            /*Function Description : Checks if ines is empty i.e. composed
             * of entirely ' '. Outputs true if it is else false*/
            int length = str.Count();
            for (int i = 0; i < length; i++)
            {
                if (str[i] != ' ')
                {
                    return false;
                }
            }
            return true;
        }

        internal static String trimFirstWord(String str)
        {
            /*Function Description : Takes a string and outputs
             * the string after removing the first word*/
            int index = str.IndexOf(' ');
            return str.Substring(index + 1);
        }

        internal static Tuple<int, int> find(String[,] array, int size, String data)
        {
            /*Function Description : Searches array for data and returns the index
             as a tuple*/

            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    if (array[i, j].Equals(data))
                        return new Tuple<int, int>(i, j);
                }
            }
            return null;
        }

        internal static void increment(List<Tuple<String, int, int>> lst, String key, int padding)
        {
            /* Function Description : Given lst = [ (s,x,y) ] increment the singe entry of 
             * (key,padding,y) to (key,padding,y+1). If not present add [ (s,x,1) ]*/
            bool incremented = false;
            for (int i = 0; i < lst.Count(); i++)
            {
                Tuple<String, int, int> tmp = lst[i];
                if (tmp.Item1.Equals(key, StringComparison.OrdinalIgnoreCase) && tmp.Item2 == padding)
                {
                    incremented = true;
                    lst[i] = new Tuple<string, int, int>(tmp.Item1, tmp.Item2, tmp.Item3 + 1);
                }
            }
            if (!incremented)
            {
                lst.Add(new Tuple<string, int, int>(key, padding, 1));
            }
        }

        internal static List<Instruction> extractSubSequence(List<Instruction> seq, int start, int end)
        {
            /* Function Description : Extract and return sub-sequence*/
            List<Instruction> subsequence = new List<Instruction>();
            for (int iter = start; iter <= end; iter++)
                subsequence.Add(seq[iter]);
            return subsequence;
        }

        internal static int getCount(List<Tuple<String, int, int>> lst, String key, int padding)
        {
            /* Function Description : Given lst = [ (s,x,y)] return y corresponding to the entry
             * (key,padding,y) in lst*/
            for (int i = 0; i < lst.Count(); i++)
            {
                Tuple<String, int, int> tmp = lst[i];
                if (tmp.Item1.Equals(key, StringComparison.OrdinalIgnoreCase) && tmp.Item2 == padding)
                {
                    return tmp.Item3;
                }
            }
            return 0;
        }

        internal static void kBeliefInsertion(List<Tuple<List<Instruction>, Environment, double>> list, Tuple<List<Instruction>, Environment, double> newEntry)
        {
            /* Function Description : Insert the newEntry at sorted position*/
            for (int i = 0; i < list.Count(); i++)
            {
                if (list[i].Item3 > newEntry.Item3)
                {
                    list.Insert(i, newEntry);
                    return;
                }
            }
            list.Add(newEntry);
        }

        internal static bool stringIgnoreCaseExist(List<String> ls, String elem)
        {
            /* Function Description : Returns true if elem matches an element of ls
             * ignoring the case*/
            foreach (String l in ls)
            {
                if (l.Equals(elem, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        internal static Tuple<double, string> jaccardIndex(List<String> xs, List<String> ys)
        {
            /* Function Description: Jaccard Index(xs,ys) is computed by 
             * N(common-words)/(union of words in xs : ys) */
            if (xs.Count() == 0 && ys.Count() == 0)
                return new Tuple<double, string>(1, "Both have 0 parameters");//this was 0 earlier
            if (xs.Count() == 0 || ys.Count() == 0)
                return new Tuple<double, string>(0, "Exactly one of them has 0 parameters");//this was 1 earlier for some reason

			xs = xs.Distinct().ToList ();
			ys = ys.Distinct().ToList ();
            int common = 0;
            foreach (String x in xs)
            {
                foreach (String y in ys)
                {
					if (x.Equals (y, StringComparison.OrdinalIgnoreCase)) 
					{
						common++;
						break;
					}
                }
            }

			String log = "X: " + String.Join (",", xs.ToArray ()) + " against Y: " + String.Join (",", ys.ToArray ());
			double score = common / (double)(xs.Count () + ys.Count () - common + Constants.epsilon);
			if (score > 1) 
				throw new ApplicationException ("Error in computing Jaccard index");
            return new Tuple<double, string>(score, log);
        }

        internal static string temporaryHack(string obj)
        {
            /* Function Description : Temporary function to return the root of the obj*/
			return null;
        }

        internal static String getString(List<SyntacticTree> syntc)
        {
            /* Function Description: Return the string represented by the syntc */
            String sen = "";
            for (int i = 0; i < syntc.Count(); i++)
                sen = sen + syntc[i].getName() + " ";
            return sen;
        }

        internal static String arrayToString<T>(T[] array, Char seperator = ',', String none = "None")
        {
            /* Function Description: Converts array to a string as elem1 seperator elem2 seperator ...*/
            String res = "";
            for (int i = 0; i < array.Length - 1; i++)
                res = res + array[i] + seperator;
            if (array.Length != 0)
                res = res + array[array.Length - 1];
            if (res.Equals(""))
                res = none;
            return res;
        }

        internal static int[] stringToArray(String code, char seperator = ',', String none = "None")
        {
            /* Function Description: Converts string to array splitting by seperator*/
            if (code.Equals(none))
                return new int[0];
            String[] split_ = code.Split(new char[] { seperator });
            int[] res = new int[split_.Length];
            for (int i = 0; i < split_.Length; i++)
                res[i] = (int)Double.Parse(split_[i]);
            return res;
        }

		internal static String standardize(String name)
		{
			/* Function Description: Standardize names like objnameNumber to objectname_number */

			for (int i= name.Length-1; i>=0; i--) 
			{
				if (name [i] < '0' || name [i] > '9') 
				{
					if (i == name.Length - 1 || name[i]=='_')
						return name;
					else return name.Substring (0, i + 1) + '_' + name.Substring (i + 1);
				}
			}
			return name;
		}

		internal static List<Instruction> filter(List<Instruction> inst)
		{
			//Function Description: Remove $ symbols
			List<int> indices = new List<int> ();
			for (int i=0; i<inst.Count(); i++) 
			{
				if (inst [i].getControllerFunction ().StartsWith ("$"))
					indices.Add (i);
			}
			for (int j=0; j<indices.Count(); j++)
				inst.RemoveAt (indices [j] - j);
			return inst;
		}

		internal static Tuple<bool,String> getAtomic(String predicate)
		{
			/* Function Description: Given predicate (p) (not (p)) it returns
			 * p and a flag which tell if its not */
			String base_ = predicate.Substring (1, predicate.Length - 2);
			if (base_.StartsWith ("not"))
			{
				int iter = base_.IndexOf ('(');
				return new Tuple<bool,string> (true, base_.Substring (iter + 1, base_.Length - iter - 2));
			}
			else return new Tuple<bool, string>(false, base_);
		}

		internal static string fetchObjExpand(String objName)
		{
			/*Function Description: Given an object name such as Tv_1PowerButton,
			 *XboxController_1, MicrowaveButton etc. it returns Tv Power Button, 
			 *Xbox Contorller, Microwave Button etc. */

			String expansion = "", words="";
			for (int i=0; i<objName.Length; i++) 
			{
				if (objName [i] >= 'A' && objName [i] <= 'Z') 
				{
					expansion = expansion + Global.base_(words)+" ";
					words = "";
				}
				words = words + objName [i];
			}
			if(words.Length>0)
				expansion = expansion  + Global.base_(words)+" ";

			return expansion.Trim();
		}

		internal static List<String> getObjects(String constraint, Environment env)
		{
			//Function Description: later
			List<String> objectL = new List<String> ();
			String[] cstrSplit = constraint.Split (new char[] { '^' });

			for (int i=0; i<cstrSplit.Length; i++) 
			{
				String[] objects = Global.getAtomic (cstrSplit [i]).Item2.Split (new char[] { ' ' });
				objectL.Add (objects [1]);
				if (!objects [0].Equals ("state"))
					objectL.Add (objects [2]);
				else
				{
					//state values may point to an object
					foreach (Object obj in env.objects) 
					{
						if(Global.base_(obj.uniqueName).Equals(objects[2],StringComparison.OrdinalIgnoreCase) || 
						   obj.checkStateAndVal(objects[2],"True"))
							objectL.Add(obj.uniqueName);
					}
				}
			}
			return objectL.Distinct().ToList();
		}

		internal static List<String> parametrize(List<String> predicates)
		{
			/*Function Description: Given a sequence of predicates,  it returns 
			 * the constraint after replacing each object with a unique variable 
			 * variables are named as z1, z2, ... */

			int zID = 0;
			List<String> seenObjects = new List<String> (); //object at index i has zID = i+1
			List<String> argPredicates = new List<string> ();

			foreach (String predicate in predicates) 
			{
				Tuple<bool,string> atom = Global.getAtomic(predicate);
				String[] words = atom.Item2.Split(new char[]{' '});
				String newPredicate = words[0];
				int ind1 = seenObjects.FindIndex (x => x.Equals (words [1]));
				if (ind1 == -1) 
				{ 
					zID++;
					seenObjects.Add (words[1]);
					newPredicate = newPredicate + " z"+zID;
				} 
				else newPredicate = newPredicate + " z"+(ind1+1);

				if (!words [0].Equals ("state")) 
				{
					int ind2 = seenObjects.FindIndex (x => x.Equals (words [2]));
					if (ind2 == -1) 
					{ 
						zID++;
						seenObjects.Add (words[2]);
						newPredicate = newPredicate + " z" + zID;
					}
					else newPredicate = newPredicate + " z"+(ind2+1);
				} 
				else newPredicate = newPredicate + " " + words [2];
				newPredicate = "("+newPredicate+")";
				if(atom.Item1)
					newPredicate = "(not "+newPredicate+")";
				argPredicates.Add (newPredicate);
			}

			return argPredicates;
		}

		internal static List<String> reformatStatePredicatesForGiza(List<String> predicates)
		{
			/*Function Description: Given a conjunction (and p1 p2 ..) where
			 pi = (state a b) it replaces all such predicates by (state-b a) */

			List<String> collapse = new List<String> ();
			foreach (String predicate in predicates) 
			{
				Tuple<bool,string> atom = Global.getAtomic (predicate);
				String[] words = atom.Item2.Split(new char[]{' '});
				if (words [0].Equals ("state")) 
				{
					if (atom.Item1)
						collapse.Add ("(not (" + words [0] + "_" + words [2] + ":t " + words [1] + ":o))");
					else
						collapse.Add ("(" + words [0] + "_" + words [2] + ":t " + words [1] + ":o)");
				}
				else 
				{
					if (atom.Item1)
						collapse.Add ("(not (" + words [0] + ":t " + words [1] + ":o " + words [2] + ":o))");
					else
						collapse.Add ("(" + words [0] + ":t " + words [1] + ":o " + words [2] + ":o)");
				}
			}

			return collapse;
		}

		internal static String standardizeStatePredicates(String predicates)
		{
			/*Function Description: Given a predicate string of the form:
			 * p or(and p1 p2 p3 ...); output f(p); f(p1)^f(p2)^...f(pk) 
			 * where f(p) takes (state_state-name:t objName:o) and converts to (state objName state-name)
			 * and converts (rel:t objName1:o objName2:o) to (rel objName1 objName2) */

			predicates = predicates.Trim ();
			List<String> typedPredicates = new List<String>();
			Console.WriteLine ("Predicates "+predicates);
			if (predicates.StartsWith ("(and")) 
			{
				predicates = predicates.Substring ("(and ".Length, predicates.Length-1-"(and ".Length);
				String[] predicatesSplit = predicates.Split (new string[]{") ("}, StringSplitOptions.None);
				for (int i=0; i<predicatesSplit.Length; i++) 
				{
					if (i != 0) //all non first entries have a missing ( at the front
						predicatesSplit [i] = "(" + predicatesSplit [i];
					if (i != predicatesSplit.Length - 1) //all non last entries have a missing ) at the end
						predicatesSplit [i] = predicatesSplit [i] + ")";
					typedPredicates.Add (predicatesSplit [i]);
				}
			} 
			else typedPredicates.Add (predicates);
			Console.WriteLine (" typed predicates " + string.Join (", ", typedPredicates));
			List<String> stripsPredicates = new List<String> ();

			Func<string, string> drop = delegate(string s)
			{
				return String.Join("",s.TakeWhile (x=> x != ':'));
			};

			foreach(String typedPredicate in typedPredicates)
			{
				if (typedPredicate.Length == 0) 
				{
					Console.WriteLine ("Found empty predicate");
					continue;
				}
				Tuple<bool,string> atomic = Global.getAtomic (typedPredicate);
				String[] words = atomic.Item2.Split (new char[] { ' ' });
				if (words [0].StartsWith ("state")) 
				{
					String[] w = words [0].Split (new char[] { '_' });
					if (atomic.Item1)
						stripsPredicates.Add ("(not (" + drop(w [0]) + " " + drop(words [1]) + " " + drop(w [1]) + "))");
					else stripsPredicates.Add ("(" + drop(w [0]) + " " + drop(words [1]) + " " + drop(w [1]) + ")");
				}
				else
				{
					if (atomic.Item1)
						stripsPredicates.Add ("(not (" + drop(words [0]) + " " + drop(words [1]) + " " + drop(words [2]) + "))");
					else stripsPredicates.Add ("(" + drop(words [0]) + " " + drop(words [1]) + " " + drop(words [2]) + ")");
				}
				Console.WriteLine ("Words "+words[0]+"   "+drop(words[0]));
			}

			Console.WriteLine (" strips predicates " + string.Join (", ", stripsPredicates));
			return string.Join("^",stripsPredicates);
		}

		internal static String[] renumerateGizaFiles(String[] uncodified, Dictionary<int,String> vocabA, Dictionary<int, String> vocabB)
		{
			/* Function Description: replace numbers in probabilities by vocabulary words using lookup
			  uncodified consists of tokenA tokenB C; we output vocab[tokenA]  ::  vocab[tokenB]  :: C */
			String[] codified = new String[uncodified.Length];

			for (int i=0; i<uncodified.Length; i++) 
			{
				String[] words = uncodified [i].Split (new char[] { ' ' });
				codified[i] = vocabA[Int32.Parse(words[0])]+"  ::  "+vocabB[Int32.Parse(words[1])]+"  ::  "+words[2];
			}
			return codified;
		}

		internal static Dictionary<int,string> vocabToDictionary(String[] vocab)
		{
			//Function Description: Given a=["x y z"]; return dictionary d with d[x] = y
			Dictionary<int,String> dict = new Dictionary<int, string> ();
			dict.Add (0, "NULL");
			foreach (String vocab_ in vocab) 
			{
				String[] words = vocab_.Split (new char[] { ' ' });
				dict.Add (Int32.Parse(words[0]), words[1]);
			}
			return dict;
		}

		internal static int getGrounding(double[,] leCorrMatrix, int lang)
		{
			/*Function Description: Given a matrix and an object description; 
             * the function returns its grounding */
			int max = 0;
			for (int j=1; j<leCorrMatrix.GetLength(1); j++) 
			{
				if (leCorrMatrix [lang, j] > leCorrMatrix [lang, max])
					max = j;
			}
			return max;
		}

		internal static void store(Parser obj, List<int> test)
		{
			//Function Description: Stores parser data in xml file so as to be cleaned later.

			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = obj.returnAllData ();
			List<List<Tuple<int, int>>> alignment = obj.returnAllAlignment ();
			System.IO.StreamWriter save = new System.IO.StreamWriter(Constants.rootPath+"test_instruction.xml");
			save.WriteLine("<root>");
			foreach (int point in test) 
			{
				save.WriteLine("<point>");
				List<Instruction> insts = datas [point].Item3;
				List<Tuple<int,int>> alignment_ = alignment [point];
				for(int j=0; j< alignment_.Count(); j++) 
				{
					Tuple<int,int> align = alignment_ [j];
					for (int i=align.Item1; i<=align.Item2; i++)
						save.WriteLine ("<instruction>"+insts[i].getName ()+"</instruction>");

					if(j<alignment_.Count()-1)
						save.WriteLine ("<instruction>Change-Of-Segment</instruction>");
				}
				save.WriteLine("</point>");
			}
			save.WriteLine("</root>");
			save.Flush ();
			save.Close ();
		}
    }
}