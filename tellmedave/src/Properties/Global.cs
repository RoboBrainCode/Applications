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

        public static List<String> instructions = new List<string>() { "Find", "Walk", "Keep", "Grasp", "Release", "Pour", "Wait", "Turn", "Open", "Close", "Press", "ScoopTo", "ScoopFrom", "Squeeze" };
        public static List<String> relations = new List<string>() { "Above", "On", "Inside", "Below" };
        public static List<String> conditionIf = new List<string>() { "if", "when" };
        public static List<String> conditionFor = new List<string>() { "for" };
        public static List<String> conditionUntil = new List<string>() { "until" };
        public static List<String> conditionElse = new List<string>() { "else", "otherwise" };
        public static List<String> conditionAfter = new List<string>() { "after" };

        internal static string base_(String objName)
        {
            /* Function Description: returns the base name of the objName. Ex: Mug1 returns Mug, Cup12 returns Cup. Ramen_1 returns Ramen */
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

        internal bool caseIgnore(String u, String v)
        {
            /*Function Description : Does a case ignore comparison of two strings*/
            return u.Equals(v, StringComparison.OrdinalIgnoreCase);
        }

        internal String returnMatched(List<Tuple<String, String>> ls, String key)
        {
            /*Function Description : Returns val such that (key,val) is in ls*/
            foreach (Tuple<String, String> tmp in ls)
            {
                if (tmp.Item1.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return tmp.Item2;
            }
            return null;
        }

        internal static bool contains(List<int> c, int x)
        {
            /* Function Description : True if c contains x else False*/
            foreach (int tmp in c)
            {
                if (tmp == x)
                    return true;
            }
            return false;
        }

        internal static int frequency(String ls, char c)
        {
            /* Function Description : Computes how many time the letter c
             * appears in the string ls*/
            int freq = 0;
            for (int i = 0; i < ls.Count(); i++)
            {
                if (ls[i] == c)
                    freq++;
            }
            return freq;
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

        internal static List<String> removeEmpty_(List<String> ar)
        {
            /* Function Description : Remove the empty lines*/
            int len = ar.Count();
            List<int> notEmpty = new List<int>();
            for (int i = 0; i < len; i++)
            {
                if (!Global.isEmpty(ar[i]))
                    notEmpty.Add(i);
            }

            List<String> arNew = new List<String>();
            foreach (int i in notEmpty)
            {
                arNew.Add(ar[i]);
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

        internal static List<String> union(List<String> ls, String newElem)
        {
            /* Function Description : Takes union of ls and newElem. 
               using IgnoreCase comparison*/
            foreach (String elem in ls)
            {
                if (elem.Equals(newElem, StringComparison.OrdinalIgnoreCase))
                {
                    return ls;
                }
            }
            ls.Add(newElem);
            return ls;
        }

        internal static List<String> intersection(List<String> xs, List<String> ys)
        {
            /* Function Description : Intersects the two string. Null means Everything or no restriction*/
            if (ys == null)
                return xs;
            if (xs == null)
                return ys;

            List<String> result = new List<String>();
            foreach (String x in xs)
            {
                bool added = false;
                foreach (String y in ys)
                {
                    if (x.Equals(y, StringComparison.OrdinalIgnoreCase))
                        added = true;
                }
                if (added)
                    result.Add(x);
            }
            return result;
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

        internal static Tuple<double, string> bagOfWord(List<String> xs, List<String> ys)
        {
            /* Function Description : Bag Of Word presence is computed by 
             * N(common-words)/(union of words in xs : ys)
             * that is average number of words present in each other */
            if (xs.Count() == 0 && ys.Count() == 0)
                return new Tuple<double, string>(0, "Both have 0 parameters");//equal
            if (xs.Count() == 0 || ys.Count() == 0)
                return new Tuple<double, string>(1, "Exactly one of them has 0 parameters");//one has non-zero while other has 0

            int common = 0;
            String log = "{  X : ";

            foreach (String x in xs)
            {
                log = log + " " + x + ",";
                foreach (String y in ys)
                {
                    if (x.Equals(y, StringComparison.OrdinalIgnoreCase))
                        common++;
                }
            }

            log = log + "||  Y : ";
            foreach (String y in ys)
                log = log + " " + y + ",";
            log = log + " }";

            double score = common / (double)(xs.Count() + ys.Count() - common);
            score = 1 - score;
            return new Tuple<double, string>(score, log);
        }

        internal static string temporaryHack(string obj)
        {
            /* Function Description : Temporary function to return the root of the obj*/
            if (obj.Equals("Door", StringComparison.OrdinalIgnoreCase))
                return "Microwave";
            if (obj.Equals("StoveKnob", StringComparison.OrdinalIgnoreCase))
                return "Stove";
            if (obj.Equals("Button", StringComparison.OrdinalIgnoreCase))
                return "Microwave";
            if (obj.Equals("fridge_1LeftDoor", StringComparison.OrdinalIgnoreCase) || obj.Equals("fridge_1RightDoor", StringComparison.OrdinalIgnoreCase) || obj.Equals("fridge_1WaterButton", StringComparison.OrdinalIgnoreCase))
                return "fridge_1";
            if (obj.Equals("microwave_1CookButton", StringComparison.OrdinalIgnoreCase))
                return "microwave_1";
            if (obj.Equals("stove_1Knob_1", StringComparison.OrdinalIgnoreCase)
                          || obj.Equals("stove_1Knob_2", StringComparison.OrdinalIgnoreCase)
                          || obj.Equals("stove_1Knob_3", StringComparison.OrdinalIgnoreCase)
                          || obj.Equals("stove_1Knob_4", StringComparison.OrdinalIgnoreCase)
                          || obj.Equals("stove_1Burner_1", StringComparison.OrdinalIgnoreCase)
                          || obj.Equals("stove_1Burner_2", StringComparison.OrdinalIgnoreCase)
                          || obj.Equals("stove_1Burner_3", StringComparison.OrdinalIgnoreCase)
                          || obj.Equals("stove_1Burner_4", StringComparison.OrdinalIgnoreCase))
                return "stove_1";
            if (obj.Equals("sink_1Knob_1", StringComparison.OrdinalIgnoreCase))
                return "sink_1";
            return obj;
        }

        internal static String getString(List<SyntacticTree> syntc)
        {
            /* Function Description: Return the string represented by the syntc */
            String sen = "";
            for (int i = 0; i < syntc.Count(); i++)
                sen = sen + syntc[i].getName() + " ";
            return sen;
        }

        internal static String arrayToString(int[] array, Char seperator = ',', String none = "None")
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

    }
}