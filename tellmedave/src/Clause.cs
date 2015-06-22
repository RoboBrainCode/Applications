using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using WordsMatching;

/* Comments : 
 * Dipendra - clause is a tree which represents segment of the sentence. Can we, 
 *            associate the sentence for which clause is computed. I am sure, you can 
 *            write the code for this
 * Dipendra - Why both lngObj and nounPhase?  I am deprecating nounPhrase 
 * Kejia - nounPhase is an old structure that I am relying on to build up the new lngObj list. It's a list of all the nouns in the sentence.
 * 		   Actually, it would be more appropriate to call it noun instead of nounPhrase*/

namespace ProjectCompton
{
    class Clause
    {
        /*Class Description : Defines data-structure Clause which is eventually mapped to controller programs*/
		public String sentence = null;      //@dipendra - add function to find this sentence
		public SyntacticTree verb = null;                                  //syntatic tree node of the main verb - cook in "Cook me a nice cup of ramne"
		public List<SyntacticTree> verbSpecification = new List<SyntacticTree>(); //provides additional specification for the verb - slowly in "Keep the cup slowly on the table"
		public List<NounPhrase> lngObj = new List<NounPhrase>();     //contains objects of verb like cup, table, water etc.

		public String[,] relation = null;                                //Relationship Matrix :- NounPhrase x NounPhrase -> Relationship  

        public bool isCondition = false;
        public string conditionName = "";    // Name of the condition,listed in Global.condition,also including condition endings like "end if" - Should be an ENUM list in future
        public string condition = "";          /* This represents the condition like "(exists $0 (and (IsCup:t $0) (IsNear $0 table1) ) )".
                                                This possibly deserves a better representation in the future" */
		public Expression conditional = null;

        //private List<SyntacticTree> originalObjects = new List<SyntacticTree>(); //deprecated

        public List<Clause> parent = new List<Clause>();                                      // Parent of this clause
        public List<Clause> children = new List<Clause>();                              // Children of this clause
        public Clause conditionStart = null;											// Used by condition end clause to refer to the condition start clause
        public Clause conditionEnd = null;											// Used by condition start clause to refer to the condition end clause

		//deprecated data-structure
		private List<SyntacticTree> nounPhrase = new List<SyntacticTree>();     //deprecated, contains objects of verb like cup, table, water etc.
		private List<SyntacticTree> originalObjects = new List<SyntacticTree>(); //deprecated

		public Clause alternativeParse = null; //the top-k clause for same sentence, are arrange in a row wise order

        public Clause(SyntacticTree name)
        {
            /* Constructor Description : Stemms the verb and adds to 
             * the */
            //Processing.stemming(name);
            this.verb = name;
            this.isCondition = false;
        }

        public Clause(string conditionName, string sentence="")
        {
            /* Constructor Description : create a condition start clause
			* conditionName is listed in Glocal.conditionIf.*/
			this.conditionName = conditionName;
			this.sentence = sentence;
            this.isCondition = true;
        }

        public Clause()
        {
            /* Constructor Description : Do nothing*/
        }

        public String getClauseDscp()
        {
            //Function Description: Returns a string - verbname [noun,noun....]
            String code = this.verb.getName()+" [ ";
			List<String> names = new List<String> ();
            foreach(NounPhrase np in this.lngObj)
            {
                String nt="";
				foreach (SyntacticTree st in np.getMainNoun())
					nt = nt + " " + st.getName ();
				names.Add (nt);
				code = code + " " + nt+" | ";
            }

			String rel = "Null";
			if (this.relation != null) 
			{
				rel = "(" + this.relation.GetLength (0) + ", " + this.relation.GetLength (1) + ")";
				for (int i=0; i<this.relation.GetLength(0); i++) 
				{
					for (int j=0; j<this.relation.GetLength(1); j++) 
					{
						if (this.relation[i,j]!=null && !this.relation [i, j].Equals ("NONE", StringComparison.OrdinalIgnoreCase) 
						    && i < names.Count () && j < names.Count ())
						    rel = rel + "{" + names [i] + " x " + names [j] + " -> " + this.relation [i, j] + "}";
					}
				}
			}

			code = code + "relation: "+rel + " ]";
            return code;
        }

		public String getSubTreeSentence()
		{
			/* Function Description: Returns the concatenation of the 
			 * sentences of clauses*/
			StringBuilder rootedSen = new StringBuilder();
			Clause iterator = this;
			while(true)//foreach(Clause child in this.children)
			{
				if (iterator.sentence == null && this.conditionName!=null) 
					rootedSen = rootedSen.Append (", "+this.conditionName);
				else if(iterator.sentence == null) 
					rootedSen = rootedSen.Append(", null ");
				else rootedSen = rootedSen.Append(", " + iterator.sentence);
				if (iterator.children.Count () > 0)
					iterator = iterator.children [0];
				else break;
			}
			return rootedSen.ToString();
		}

		public List<SyntacticTree> returnNounList()
		{
			/*Function Description : return noun list*/
			return this.nounPhrase;
		}

		public List<SyntacticTree> returnOriginalCopy()
		{
			/*Function Description : return original list*/
			return this.originalObjects;
		}

		public List<String> getWords()
		{
			/*Function Description: Returns the clause information as a set of words */
			List<String> bag = new List<String> ();
			if (this == null)
				Console.WriteLine ("This itself is null wow");
			if (!this.isCondition) 
			{
				bag.Add (this.verb.getName ());

				if (this.lngObj == null)
					Console.WriteLine ("LngObj is null wow");

				foreach (NounPhrase  np in this.lngObj) 
					bag = bag.Concat(np.getMainNoun ().Select(x=>x.getName())).ToList();

				if (relation != null)
				{
					for (int i=0; i<this.relation.GetLength(0); i++) 
					{
						for (int j=0; j<this.relation.GetLength(1); j++) 
						{
							if (this.relation [i, j] != null && !this.relation [i, j].Equals ("None", StringComparison.OrdinalIgnoreCase))
								bag.Add (relation [i, j]);
						}
					}
				}
			}
			return bag;
		}

		public double returnOriginalNum()
		{
			/* Function Description : returns original number of noun-phrases*/

			int num = 0;
			for (int i = 0; i < this.nounPhrase.Count(); i++)
			{
				if (this.nounPhrase[i].getName().StartsWith("$v"))
					num++;
			}
			return num;
		}

		public void addNoun(SyntacticTree noun)
		{
			/*Function Description : add noun*/
			this.nounPhrase.Add(noun);
		}

        public void addConditionScope(Clause start, Clause end)
        {
            /* Function Description: define start to be the conditionStart for
            * end; define end to be the conditionEnd for start*/
            end.conditionStart = start;
            start.conditionEnd = end;
        }

        public Clause rootClause()
        {
            /* Function Description : Returns the first root Clause else returns null*/
            if (this.isCondition)
            {
                foreach(Clause child in this.children)
                {
                    Clause ret = child.rootClause();
                    if (ret != null)
                        return ret;
                }
            }
            else return this;
            return null;
        }

        public List<Clause> returnEventClause()
        {
            /* Function Description : Many functions want the set of event-clause used
             * by this sub-tree. This set is computed and returned by this function */
            List<Clause> evClause = new List<Clause>();
            if(!this.isCondition)
                evClause.Add(this);
            foreach(Clause child in this.children)
            {
                List<Clause> childEvClause = child.returnEventClause();
				foreach (Clause c in childEvClause) 
					evClause.Add (c);
                childEvClause.Clear();
            }
            return evClause.Distinct().ToList();
        }

		public Clause findLeaf()
		{
			/*Function Description: find the leaf for this Clause*/
			Clause leaf = this;
			while (leaf.children.Count > 0)
			{
				leaf = leaf.children[0];
			}
			return leaf;
		}

		public void addLamdaExpression()
		{
			/*Function Description: add this.sentence to UBL Test File*/
			//File.AppendAllText(Constants.UBLPath + "UBL/experiments/new-1/data/en/run-0/fold-0/test",this.condition + "\n(dummy:t)\n\n");
			//using (System.IO.StreamWriter file = new System.IO.StreamWriter (Constants.UBLPath + "UBL/experiments/new-1/data/en/run-0/fold-0/test")) {//write into test
			//	file.WriteLine (this.sentence + "\n(dummy:t)\n");
			//}
		}
                
        public bool ifExists(String name)
        {
            /*Function Description : Return true if 
             * name is a noun-phrase object */
            foreach (SyntacticTree n in this.nounPhrase)
            {
                if (n.getName().Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public int numNodes()
        {
            /* Function Description : Returns number of nodes in this tree */
            int nodes = 1;
            if (this.isCondition)
                nodes = 0;
            if (this.children == null)
                return nodes;
            foreach (Clause cl in this.children)
                nodes = nodes + cl.numNodes();
            return nodes;
        }

        public Clause first()
        {
            /* Function Description : Returns the first clause of this tree where
               children are access from left to right */
            if (this.isCondition)
            {
                if (this.children == null)
                    return null;
                foreach (Clause cl in this.children)
                {
                    Clause cl_fst = cl.first();
                    if (cl_fst != null)
                        return cl_fst;
                }
            }
            return this;
        }

        public String isExist(String name)
        {
            for (int i = 0; i < this.originalObjects.Count(); i++) //noun contains all $ variables
            {
                SyntacticTree n = this.originalObjects[i];
                if (n.getName().Equals(name, StringComparison.OrdinalIgnoreCase))
                    return this.nounPhrase[i].getName(); //return the variable name
            }
            return null;
        }

        public void addOriginalCopy(SyntacticTree original)
        {
            /*Function Description : take original*/
            originalObjects.Add(original);
        }

        public void findRelation()
        {
            /* Function Description : find relationship between nounphrases */
			if (lngObj.Count > 1)
            {
				relation = new string[lngObj.Count, lngObj.Count];
				for (int i = 0; i < lngObj.Count; i++)
                {
					for (int j = i + 1; j < lngObj.Count; j++)
                    {
						SyntacticTree noun1 = lngObj[i].getOriginalNounPhrase()[0];
						SyntacticTree noun2 = lngObj[j].getOriginalNounPhrase()[0];
                        //find relationship between noun1 and noun2
                        SyntacticTree rln = noun1.findNextRelationNode();
						if (rln == null || rln.getName () == null) 
						{
							relation [i, j] = "NONE";
						} 
						else 
						{
							relation [i, j] = rln.getName ();
						}
                    }
                }
            }

			/*Transivity Property of conjunctions: if there exists 3 noun-phrase x,y,z
			 * and relationship rel1, rel2 such that x rel1 y; y rel2 z; such that 
             * rel1 is a conjunct-type then replace or add x rel2 z. 
             * E.g., mug and cup; cup on table; --> mug on table */
			if (relation == null)
				return;

			for (int i=0; i < lngObj.Count; i++) 
			{
				for (int j=0; j<lngObj.Count; j++) 
				{
					for(int k=0; k<lngObj.Count; k++)
					{
						if (relation [i, j] == null || relation [j, k] == null)
							continue;
			
						if (relation [i, j].Equals ("and", StringComparison.OrdinalIgnoreCase) && !relation [j, k].Equals ("NONE")) 
						{
							relation [i, k] = relation [j, k];
						}
					}
				}
			}

			for (int i=0; i<lngObj.Count; i++) 
			{
				for (int j=0; j<lngObj.Count; j++) 
				{
					if (relation [i, j] == null)
						relation [i, j] = "NONE";
				}
			}
        }

		//Deprecated function
        public void display(Logger lg)
        {
            /*Function Description : Display the clause*/
        }

        public void storeXML(Logger lg)
        {
            /* Function Description : Write the clause description as xml - 
             * <Clause>
             *      <Verb>verb-name</Verb>
             *      
             *      <NounPhrases>
             *          <NounPhrase>...</NounPhrase>
             *          ....
             *      </NounPhrases>
             *      <Relationships>
             *          <Relationship>1,1,string</Relationship>
             *          .....
             *      </Relationships>
             *      <Children>
             *          <Clause>...<Clause>
             *          ....
             *      </Children>
             * </Clause> */

            lg.writeToParserData("<Clause>");
			if (this.sentence != null)
				lg.writeToParserData("<sentence>" + this.sentence + "</sentence>");
            if (this.verb != null)
                lg.writeToParserData("<verb>" + this.verb.getName() + "</verb>");
            lg.writeToParserData("<NounPhrases>");
			foreach (NounPhrase np in this.lngObj) 
				np.storeXML (lg);
            lg.writeToParserData("</NounPhrases>");
            
            if (isCondition && this.conditionName!= null)
			{
				if (this.condition != "") {
					Console.WriteLine (this.condition);
					lg.writeToParserData ("<Condition>" + this.condition + "</Condition>");
				}
				else
					lg.writeToParserData("<Condition>" + this.conditionName + "</Condition>");
			}
            String relationship = "";
            if (this.relation != null)
            {
                int n = this.lngObj.Count();
                for (int i = 0; i < n; i++)
                {
					for (int j = 0; j < n; j++) 
					{
						if (this.relation [i, j] != null || !this.relation [i, j].Equals ("NONE"))
							relationship = "<Relationship> #" + i + " " + this.relation [i, j] + "#" + j + "</Relationship>";
					}
                }
            }
			lg.writeToParserData ("<Relationships>" + relationship + "</Relationships>");//\n<Children>");
            //foreach(Clause child in this.children)
            //   child.storeXML(lg);

            lg.writeToParserData(/*</Children>*/"</Clause>");
        }

		public double[,] getLngCorrMatrix(Clause cls,SentenceSimilarity sensim)
        {
            /* Function Description : Return the Language-Language correlation matrix.
             * The correlation between two words can be defined in multiple ways and different
             * combinations should be tried. For the moment I am simply trying word-net
             * similarity. */

            if (this.isCondition || cls.isCondition) //Conditional Clauses are non-eventive thus correlation matrix is not defined
                throw new System.ApplicationException("Exception: Computing Correlation Matrix of Conditional [Non-Eventive] Part of Sentence.");

			int m = this.lngObj.Count(), n = cls.lngObj.Count();
            double[,] lngCorrMatrix = new double[m, n];

            for (int i = 0; i < m; i++)
            {
				for (int j = i; j < n; j++) 
				{  
					/* Consider keep cup on table AND keep book on desk -> cup maps to book 
					 * cause they have a relation on emerging from them. In short, two nodes
					 * i and j are mapped if r[i,k1] = r[j,k2] for some k1 and k2 */

					lngCorrMatrix [i, j] = 0;//sensim.GetScore (this.lngObj [i].getMainNoun () [0].getName (), cls.lngObj [j].getMainNoun () [0].getName ());
				}
			}
            return lngCorrMatrix;
        }

		public void conjunctionSplit()
		{
			/* Function Description: Given a clause C = (v, [w1,w2, .... ], rel); 
			 * such that the there exist i,j, k; such that following exist:
			 * rel [wi, wj] = CONJ "," type and rel[wi, wk] = rel[wj,wk]
			 * if yes, then split the clause as follows - 
			 * C = C1, C2; C1 = (v, {w1,..}-{wi}, rel), C2 = (v,{w1...}-{wj},rel) */

			if (this.isCondition || this.lngObj.Count() != 3 || this.relation == null)  //currently only doing for 3 noun-phrase
			{
				foreach (Clause child in this.children)
					child.conjunctionSplit ();
				return;
			}

			Clause tail = this;
			for (int i=0; i< this.lngObj.Count(); i++) 
			{
				for (int j=0; j<this.lngObj.Count(); j++) 
				{
					if (!this.relation [i, j].Equals ("and")) //this should be replaced by CONJ type, once we store types with values
						continue;
					for (int k=0; k<this.lngObj.Count(); k++) 
					{
						if (k == i || k == j)
							continue;
						if (this.relation [i, k].Equals (this.relation [j, k])) 
						{
							Clause newC = this.makeCopy ();
							//remove i from this and j from newC
							if (i > j) 
							{
								this.removeithNP (i);
								newC.removeithNP (j);
							}
							else
							{
								this.removeithNP (j);
								newC.removeithNP (i);
							}

							//give children of this to newC, and add edge from C to newC
							foreach(Clause child in this.children)
								newC.children.Add(child);
							this.children.Clear ();
							this.children.Add (newC);
							newC.parent.Add (this);
							tail = newC;
							foreach (Clause child in tail.children)
								child.conjunctionSplit ();
							return;
						}
					}
				}
			}

			foreach (Clause child in tail.children)
				child.conjunctionSplit ();
		}

		public Clause makeCopy()
		{
			/* Function Description : Makes and returns the copy*/			
            Clause cls = new Clause();
            cls.verb = this.verb.makeCopy();
			cls.sentence = this.sentence;

            //foreach (SyntacticTree n in this.nounPhrase)            
            //    cls.nounPhrase.Add(n.makeCopy());
            
			foreach (NounPhrase n in this.lngObj)            
				cls.lngObj.Add(n);

            //foreach (SyntacticTree n in this.originalObjects)
            //    cls.originalObjects.Add(n.makeCopy());

			//copy the relationship matrix
			cls.relation = new string[this.lngObj.Count, this.lngObj.Count]; //relationship matrix are always square

			for (int i = 0; i < cls.lngObj.Count; i++)
			{
				for (int j = 0; j < cls.lngObj.Count; j++)
					cls.relation[i, j] = this.relation[i, j];
			}
            return cls;
		}

		public void removeithNP(int i)
		{
			// Function Description: Remove the ith noun-phrase

			this.lngObj.RemoveAt (i);
			String[,] newRelationship = new string[this.lngObj.Count-1,this.lngObj.Count-1];

			for (int row=0; row<this.lngObj.Count()-1; row++) 
			{
				for (int col=0; col<this.lngObj.Count()-1; col++) 
				{
					if (row < i && col < i)
						newRelationship [row, col] = this.relation [row, col];
					else if (row < i && col > i)
						newRelationship [row, col-1] = this.relation [row, col];
					else if (row > i && col < i)
						newRelationship [row-1, col] = this.relation [row, col];
					else if (row > i && col > i)
						newRelationship [row-1, col-1] = this.relation [row, col];
				}
			}

			this.relation = newRelationship;
		}

    }
}
