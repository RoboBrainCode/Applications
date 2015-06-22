using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompton
{
    class Expression
    {
        /* Class  Description: Represents first-order logic expression and
         * provides functionality for using them.
		 * Used by CCG expression and PDDL*/

        private nodeType type; //Is either of type - and, for, when, not or null (leaf)
        private List<Expression> arguments = new List<Expression>(); /* Syntax - 
                                                      (and arg1 arg2 arg3 ....)
                                                      (for arg1[condition] arg2) 
                                                      (when arg1[condition] arg2) 
                                                      (not arg1) */
        private String exp; //Null except for leaves

        public enum nodeType
        {
            And,
            For,
            When,
            Not,
            Null,
			Exists,
			Object,
			Has, Condition, Time, State, On, In, IsGrasping, IsNear, The
        }

        public Expression(String parse)
        {
            /* Constructor Description: Create the expression out of the string 
             * Expression : (and (Expression1) (Expression2) ..... (ExpressionN)) -- And type
             * Expression : (forall (Expression) (Expression)) -- for
             * Expression : (when (Expression) (Expression)) -- when
             * Expression : (not (Expression)) -- not
             * Expression : (predicate) -- base case 
             * ---- following expressions are added by Kejia for parsing conditionals ----
             * ---- they are pretty non-standard and we need to improve their structure ----
             * Expression : (exists (predicate) (Expression)) -- exists
             * Expression : (object:t (Expression) (Expression)) -- object  (object:t e e t)
			 * Expression : (has:t (Expression) (Expression)) -- object  (has:t e q t)
			 * Expression : (condition:t (Expression) word) -- object  (condition:t c t)
			 * Expression : (time:t (Expression) word (Expression)) -- object  (time:t i u t t)
			 * Expression : (state:t (Expression) word) -- (state:t e s t)
			 * Expression : (On:t (Expression) (Expression)) -- (On:t e e t)
			 * Expression : (In:t (Expression) (Expression)) -- (In:t e e t)
			 * Expression : (IsGrasping:t (Expression) (Expression)) -- (IsGrasping:t p e t)
			 * Expression : (IsNear:t (Expression) (Expression)) -- (IsNear:t p e t)
			 * Expression : (the:t variable (Expression)) -- (the:t $0 t t) */

			if (parse.IndexOf ('(') == -1 && parse.IndexOf (')') == -1)  //it is a word
			{
				this.type = nodeType.Null;
				this.exp = parse;
				throw new ApplicationException ("I feel like dying");
				return;
			}

			parse = parse.Replace('\t', ' ');
            parse = parse.Replace('\n', ' ');
            int paren = parse.IndexOf('(');
            parse = parse.Substring(paren + 1);
			List<String> headers = new List<String> () { "and", "forall", "when", "not", "exists", "object:t", "has:t", "condition:t", 
				                                         "time:t", "state:t", "On:t", "In:t", "IsGrasping:t", "IsNear:t", "the:t"};
			if (headers.Exists(x=>parse.StartsWith(x)))
            {
                if (parse.StartsWith("and"))
                    this.type = nodeType.And;
                else if (parse.StartsWith("forall"))
                    this.type = nodeType.For;
                else if (parse.StartsWith("when"))
                    this.type = nodeType.When;
                else if (parse.StartsWith("not"))
                    this.type = nodeType.Not;
				else if (parse.StartsWith("exists"))
					this.type = nodeType.Exists;
				else if (parse.StartsWith("object:t"))
					this.type = nodeType.Object;
				else if (parse.StartsWith("has:t"))
					this.type = nodeType.Has;
				else if (parse.StartsWith("condition:t"))
					this.type = nodeType.Condition;
				else if (parse.StartsWith("time:t"))
					this.type = nodeType.Time;
				else if (parse.StartsWith("state:t"))
					this.type = nodeType.State;
				else if (parse.StartsWith("On:t"))
					this.type = nodeType.On;
				else if (parse.StartsWith("In:t"))
					this.type = nodeType.In;
				else if (parse.StartsWith("IsGrasping:t"))
					this.type = nodeType.IsGrasping;
				else if (parse.StartsWith("IsNear:t"))
					this.type = nodeType.IsNear;
				else if (parse.StartsWith("the:t"))
					this.type = nodeType.The;

                int j = 0;
                while (j < parse.Count())
                {
					if (parse [j] == '(') 
					{
						//create a new expression
						String exp_ = this.nextBracket (parse, j);
						j = j + exp_.Count () - 1;
						Expression exp1 = new Expression (exp_);
						this.arguments.Add (exp1);
					}

					else if (parse [j] == ' ') 
					{
						//two cases can occur _asdawasdasd_/)  or _(expression)_
						String exp_ = "";
						j++;
						while (parse[j]!='(' && parse[j]!=' ' && parse[j]!=')')
						{
							exp_ = exp_ + parse [j];
							j++;
						}

						if (exp_.Length != 0 && (parse [j] == ' ' || parse [j] == ')'))  //its the first case
						{
							throw new ApplicationException ("Should not have entered "+parse);
							Expression exp1 = new Expression (exp_);
							this.arguments.Add (exp1);
						} 
						else if (parse [j] == '(') //its the 2nd form so we backtrack
							 	j--; //since we do j++ later
					}
                    j++;
                }
            }
            else //leaf
            {
                this.type = nodeType.Null;
                if (parse[parse.Count() - 1] == ')')
                    parse = parse.Substring(0, parse.Count() - 1);
                //effect can have extra closing parethesis so we check for it
                int open_ = 0, close_ = 0, index = -1;
                for (int i = 0; i < parse.Length; i++)
                {
                    if (parse[i] == '(')
                        open_++;
                    else if (parse[i] == ')')
                    {
                        close_++;
                        index = i;
                    }
                }

                if (open_ != close_ && index != -1)
                    parse = parse.Substring(0,index) + parse.Substring(index+1);
                this.exp = parse.Trim();
            }
        }

        private String nextBracket(String exp, int start)
        {
            /* Function Description : Given the start character as (, return 
             * the substring which ends with matching closing bracket */
            
            if (exp[start] != '(')
                throw new ApplicationException("Next Bracket: String must start with (");
            int balance = 1;
            String ret = "(";
            for (int i = start+1; i < exp.Count(); i++)
            {
                ret = ret + exp[i];
                if (exp[i] == '(')
                    balance++;
                if (exp[i] == ')')
                {
                    balance--;
                    if (balance == 0)
                        return ret;
                }
            }
            return null;
        }

        public String instantiate(String expression, List<Tuple<String, String>> map)
        {
            /* Function Description: Instantiate the expression, the variables in 
             * expression are instantiated using the map. The expression is of the form - 
             * state object-name state-name 
             * affordance-type object-name
             * relation object-name object-name
             */

            String exp_ = expression.ToString(), ret = "";
            String[] words = exp_.Split(new char[] {' '}); //split into words
            ret = words[0].Trim();

            for (int i = 1; i < words.Length; i++)
            {
                words[i] = words[i].Trim();
                bool added = false;
                foreach (Tuple<String, String> map_ in map)
                {
                    if (map_.Item1.Equals(words[i]))
                    {
                        added = true;
                        ret = ret + " " + map_.Item2;
                    }
                }
                if (!added)
                    ret = ret + " " + words[i];
            }

            return ret;
        }

        public String evaluate(Environment env, List<Tuple<String,String>> map)
        {
            /* Function Description : Evaluate the expression on the given environment 
             * for the given variable mapping. Returns the string of expression that needs
             * to be satisfied (when satisfiable) else returns null */

            if (type == nodeType.Null) //leaf-type
            {
                //convert this.exp using map
                String instant = this.instantiate(this.exp, map);
				if (instant.Length == 0)//"" represents True
					return ""; 
                int code = env.isSastified(instant);
                if (code == -1) //unsatisfiable
                    return null;
                else if (code == 0) //satisfiable but not satisfied
                    return instant;
                else if (code == 1)//satisfiable and satisfied
                    return "";
            }
            else if (type == nodeType.And) //and-type
            {
                String ans = "";
                foreach (Expression exp in this.arguments)
                {
                    String ret = exp.evaluate(env, map);
					if (ret == null)
						return null;
					else if (ret.Count () > 0) 
					{
						if (ans.Length == 0)
							ans = ret;
						else ans = ans + "^" + ret;
					}
                }
                return ans;
            }
            else if (type == nodeType.For)
            {
                //only for condition that we are handling is =?o ?otherobj
				List<Object> objL = env.objects;
                String resOuter = "";
                foreach (Object obj in objL)
                {
                    map.Add(new Tuple<string, string>("?otherobj", obj.uniqueName));
                    String resInner = this.arguments[1].evaluate(env, map);
                    map.RemoveAt(map.Count() - 1);

                    if (resInner == null)
                        return null;
                    if (!resInner.Equals(""))
                    {
						if (resInner.Count () > 0) 
						{
							if(resOuter.Length==0)
								resOuter = resInner;
							else resOuter = resOuter + "^" + resInner;
						}
                    }
                }
                return resOuter;
            }
            else if (type == nodeType.When)
            {
                String condnEval = this.arguments[0].evaluate(env, map);
                if (condnEval == null)
                    return null;

                if (!condnEval.Equals("")) //if the condition is false we are true
                    return "";

                return this.arguments[1].evaluate(env, map);
            }
            else if (type == nodeType.Not)
            {
                String res = this.arguments[0].evaluate(env, map);
                if (res == null)
                    return null;
                if (res.Equals(""))
					return "not (" + this.instantiate(this.arguments[0].exp, map) + ")";
                else return "";
            }
            else throw new ApplicationException("Unknown PDDL syntax "+this.exp);

            return null;
        }

		public void modify(Environment env, List<Tuple<String,String>> map)
		{
			/* Function Description : Modifies the given environment
			 * according to this expression and map. That is if the
			 * expression is (IsGrasping Robot ?x) and map is ?x->cup
			 * then make the robot grasp the cup */

			if (type == nodeType.Null) //leaf-type
			{
				String instant = this.instantiate(this.exp, map); //convert this.exp using map
				if (!instant.Equals ("")) 
					env.modify (instant, true);
			}
			else if (type == nodeType.And) //and-type
			{
				/* Handling if-else - a common scenario occurs as 
				*  (and (when (C) (not C)) (when (not C) (C)))
				*  (and (when (not C) (C)) (when (C) (not C)))
				*  in this case due to sequential evaluation on C (not C resp.) we end up going back to C (not C resp.)
				*  to solve this we constraint that all if-else occur at same level and must be placed
				*  one after the other. */

				System.IO.StreamWriter sw = new System.IO.StreamWriter (Constants.rootPath + "and.txt");

				List<int> allowed = new List<int> ();
				for (int i=0; i < this.arguments.Count(); i++) 
				{
					if(this.arguments[i].type == nodeType.When)
					{
						//evaluate the when condition on the current environment
						String ans = this.arguments [i].arguments [0].evaluate (env, map);
						if (ans != null && ans.Equals ("")) 
						{
							if(this.arguments[i].arguments[0].type == nodeType.Null)
								sw.WriteLine ("Allowing: "+this.arguments[i].arguments[0].exp);
							allowed.Add (i);
						}
					}
					else allowed.Add(i);
				}

				sw.Flush ();
				sw.Close ();

				for(int i=0;i<allowed.Count();i++) 
				{
					Expression exp = this.arguments[allowed[i]];
					exp.modify (env, map);

					/*if (exp.type == nodeType.When && exp.arguments [0].type == nodeType.Null) //condition C 
					{
						if (i + 1 < this.arguments.Count () && this.arguments [i + 1].type == nodeType.When
						    && this.arguments[i+1].arguments[0].type == nodeType.Not)  //when (not ..) (...)
						{
							//Check if the two conditions are same or not 
							String c1 = exp.arguments [0].exp;
							String c2 = this.arguments [i + 1].arguments [0].arguments [0].exp;
							if (c1 == null || c2 == null)
								continue;
							//don't evalute if the two conditions are of form C and not C and first one has been evaluated to be true
							String ans = exp.arguments [0].evaluate (env, map);
							if (c1.Equals (c2))  
								allowed = false;
						}
					}
					else if(exp.type == nodeType.When && exp.arguments[0].type == nodeType.Not) //condition not C
					{
						if (i + 1 < this.arguments.Count () && this.arguments [i + 1].type == nodeType.When) //when (..) (...)
						{
							//Check if the two conditions are same or not 
							String c1 = exp.arguments [0].arguments[0].exp;
							String c2 = this.arguments [i + 1].arguments [0].exp;
							if (c1 == null || c2 == null)
								continue;
							//don't evalute if the two conditions are of form not C and C and first one has been evaluated to be true
							if (c1.Equals (c2))
								allowed = false;
						}
					}*/
				}
			}
			else if (type == nodeType.For)
			{
				//only for condition that we are handling is =?o ?otherobj
				List<Object> objL = env.objects;
				foreach (Object obj in objL)
				{
					map.Add(new Tuple<string, string>("?otherobj", obj.uniqueName));
					this.arguments[1].modify(env, map);
					map.RemoveAt (map.Count () - 1);
				}
			}
			else if (type == nodeType.When)
			{
				String condnEval = this.arguments[0].evaluate(env, map);
				if (condnEval!=null && condnEval.Equals (""))  //if condition is true then modify the inner stuff
					this.arguments[1].modify(env, map);
			}
			else if (type == nodeType.Not)
			{
				/* if not is used as not (rel1 and rel2) then we
				 * will not know which one to change. Infact in effect,
				 * this condition is undesirable. This is telling us that
				 * we should not have not(p and q) are true. But we can then
				 * replace it simply by not p and q as even then we will not have
				 * p and q as true. */

				Expression innerAtom = this.arguments [0];
				if (innerAtom.type != nodeType.Null) //it has to not p where p is atom
					throw new ApplicationException("Non-deterministic Effect Not (P and Q) ");
				//make it false
				String instant = this.instantiate(innerAtom.exp, map);
				if (instant.Equals ("")) 
					throw new ApplicationException ("");
				env.modify (instant,false);
			}
			else throw new ApplicationException("Unknown PDDL syntax "+this.exp);
		}

		public String getObject(List<Tuple<String,String>> map)
		{
			/* Function Description: Given an expression and map, where the
			 * expression denotes an object. The algorithm returns the object
			 * that the algorithm represents. */
			 

			if (this.type == nodeType.The) {  //the $1 (object $1 category) 
				//-- this is a hack, needs to be relplaced by real implementatio
				if (this.arguments [1].type == nodeType.Object) 
					return this.arguments [1].arguments [1].exp;
				throw new ApplicationException ("Strange use of word the "+this.arguments[1].type.ToString());
			}

			if (this.exp == null)
				throw new ApplicationException ("Conditional Parsing: Expecting non-null object");

			else if (this.exp.StartsWith ("$"))  //its a variable
			{
				foreach (Tuple<String,String> elem in map) 
				{
					if (elem.Item1.Equals (this.exp))
						return elem.Item2;
				}
				throw new ApplicationException ("Variable whose mapping does not exist");
			}

			return this.exp;
		}

		public String groundObject(String word, Environment env)
		{
			/* Function Description: Temporary grounding of objects in the environment.
			 * This function to be merged with LE Matrix. It returns an object that is similar
			 * to the environment */

			return null;
		}

		public bool isConditionSatisfied(Environment env, List<Tuple<String,String>> map)
		{
			/* Function Description: A conditional expression, is a first order expression
             * that could either represent:
             * - a branch condition: (state cup water)   [if the cup has water]
             * - a temporal condition: (wait 10min)
             * This function, takes this condition and if it is a branch condition then evaluates
             * the branch condition and returns True/False accordingly. If its a temporal condition
             * then the algorithm returns True. */

			return true;
			//Semantic rules for different condition

			if (this.type == nodeType.Exists) 
			{
				//first must be a variable
				if (this.arguments [0].type != nodeType.Null)
					throw new ApplicationException ("Exists expression is not in proper format -- expecting Exists $var exp");
				String variable = this.arguments[0].exp;
				foreach(Object obj in env.objects)
				{
					map.Add (new Tuple<String,string>(variable, obj.uniqueName));
					if(this.arguments[1].isConditionSatisfied(env,map))
						return true;
					map.RemoveAt(map.Count()-1);
				}
				return false;
			}

			if (this.type == nodeType.And) 
			{
				foreach (Expression exp in this.arguments) 
				{
					if (!exp.isConditionSatisfied (env, map))
						return false;
				}
				return true;
			}

			if (this.type == nodeType.Condition) //(condition:t c t)
				return true;

			if (this.type == nodeType.State || this.type == nodeType.Has)  //(state:t e s t),  //(has:t e q t)
			{
				if (this.arguments [0].type != nodeType.Null)
					throw new ApplicationException ("what kind of earth is this");

				//for the given object find the state
			}

			if (this.type == nodeType.Object)  //(object:t e e t)
			{
				String objName1 = this.groundObject(this.arguments[0].getObject (map), env);
				String objName2 = this.groundObject(this.arguments[1].getObject (map), env); 
				if (objName1.Equals (objName2))
					return true;
				else return false;
			}

			if (this.type == nodeType.Time)  //(time:t i u t t)
			{
				return true;
			}

			//spatial relation type
			if (this.type == nodeType.On || this.type == nodeType.In || this.type == nodeType.IsGrasping || this.type == nodeType.IsNear) 
			{
				//take the two  object and check for this relation
				String objName1 = this.groundObject(this.arguments[0].getObject (map), env);
				String objName2 = this.groundObject(this.arguments[1].getObject (map), env);
				switch (this.type) 
				{
					case nodeType.On:	return env.checkRelExists (objName1, objName2, SpatialRelation.On);
					case nodeType.In:	return env.checkRelExists (objName1, objName2, SpatialRelation.In);
					case nodeType.IsGrasping:	return env.checkRelExists (objName1, objName2, SpatialRelation.Grasping);
					case nodeType.IsNear:	return env.checkRelExists (objName1, objName2, SpatialRelation.Near);
				}
			}

			if(this.exp!=null)
				throw new ApplicationException ("Executing conditionals: dont know how to excute "+this.exp+".");
			else throw new ApplicationException ("Executing conditionals: dont know how to excute. Exp is null");

			return true;
		}

		public List<String> getExpressionCover(String var)
		{
			/* Function Description: Returns those predicates rooted at this expression
			 * that concerns the given variable */

			List<String> res = new List<String> ();
			if (this.exp != null) 
			{
				String[] wordify = this.exp.Split (new char[]{' '});
				for (int wi=0; wi<wordify.Length; wi++) 
				{
					if (wordify[wi].Equals (var)) 
						return new List<String> () { this.exp };
				}
				return res;
			}

			foreach (Expression arg in this.arguments) 
				res = res.Concat (arg.getExpressionCover(var)).ToList();
			return res;
		}

		public static bool isNotTriviallyInconsistent(String constraint1, String constraint2)
		{
			/*Function Description: We do a simple rule based checking, if 
			 * constraint1^constraint2 makes sense or not. We use the following
			 * rules to make sure that trivial cases are removed. This helps in speeding
			 * the algorithm:
			 * 1. if p and not p occurs together
			 * 2. if (On/In z1 z2) and (On/In z1 z3) z2 \ne z3
			 * The overall consistency will be taken care of later. */

			String[] cstr1 = constraint1.Split (new char[] {'^' });
			String[] cstr2 = constraint2.Split (new char[] {'^' });

			foreach (String cstr1_ in cstr1) 
			{
				Tuple<bool, string> atom1 = Global.getAtomic (cstr1_);
				String[] words1 = atom1.Item2.Split (new char[] { ' ' });
				if (!atom1.Item1 && (words1 [0] == "On" || words1 [0] == "In")) 
				{
					foreach (String cstr2_ in cstr2) 
					{
						Tuple<bool, string> atom2 = Global.getAtomic (cstr2_);
						String[] words2 = atom2.Item2.Split (new char[] { ' ' });
						if(!atom2.Item1 && words2[0].Equals(words1[0]) && words2[1].Equals(words1[1]) && !words2[2].Equals(words1[2]))
							return false; //inconsistent
					}
				}
			}

			return true;
		}
    }
}
