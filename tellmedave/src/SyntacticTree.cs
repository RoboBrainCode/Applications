using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ProjectCompton
{
    class SyntacticTree
    {
        /*Class Description : This class represents the syntactic data-structure*/

        private static List<String> verbSpace = new List<string>() { "Microwav", "Microwave", "Plac", "Place", "Ignit", "Tak", "Add" ,"Take"}; //list of all verbs that it has seen

        private string type = null, name = null;
        private SyntacticTree parent = null;
        private List<SyntacticTree> children = new List<SyntacticTree>();

       	public SyntacticTree makeCopy()
        {
            /* Function Description : Makes and returns the copy*/
            SyntacticTree st = new SyntacticTree();
            st.type = this.type.ToString();
            st.name = this.name.ToString();
            return st;
        }

        public void makeParent(SyntacticTree parent)
        {
            /*Function Description : changes parent*/
            this.parent = parent;
        }

        public SyntacticTree giveParent()
        {
            /*Function Description : Returns parent*/
            return this.parent;
        }

        public void changeVerb(String verb)
        {
            /*Function Description : Replaces the verb*/
            this.name = verb;
        }

        public SyntacticTree createAndAddChild()
        {
            /*Function Description : creates a new node and adds it to present*/
            SyntacticTree newChild = new SyntacticTree();
            this.children.Add(newChild);
            newChild.makeParent(this);
            return newChild;
        }
		
		public void addChild(SyntacticTree child, int index)
		{
			/*Function Description : add a child at index*/
			this.children.Insert(index,child);
		}
		
		public void deleteChild(SyntacticTree child)
		{
			/*Function Description : delete child from this tree*/
			this.getChildren().Remove(child);
		}

        public void addDefinition(string word)
        {
            /*Function Description : takes and word and uses it to define type,name*/
            if (this.type == null)
            {
                this.type = word.ToString();
            }
            else this.name = word.ToString();
        }

        public List<SyntacticTree> getChildren()
        {
            /*Function Description : return children*/
            return this.children;
        }

        public string getType()
        {
            /*Function Description : returns type*/
            return this.type;
        }

        public string getName()
        {
            /*Function Description : returns name*/
            return this.name;
        }

		public List<SyntacticTree> findVerbCondition(List<Clause> vbc)
		{
			/*Function Description : Take a syntactic tree and find verb condition
			* (if,when,after,until,for). Return a string of verb condition*/
            List<SyntacticTree> vbcs = new List<SyntacticTree>();
			if (this != null && this.getType()!=null)
            {
                if (this.getType().Equals("IN") || this.getType().Equals("WRB"))
                {
                    if (String.Compare(this.getName(), "if", true)==0||String.Compare(this.getName(), "until", true)==0||String.Compare(this.getName(), "after", true)==0)
					{
						 this.extractVerbCondition(vbcs,vbc,this.getName(),"PP","SBAR");//Extract the whole conditional phrase and add it to vbcs
					}
					else if (String.Compare(this.getName(), "when", true)==0)
					{
                        this.extractVerbCondition(vbcs, vbc, this.getName(), "ADJP", "SBAR");
					}
					//else if (String.Compare(this.getName(), "for", true)==0)
					//{
					//	this.extractVerbCondition(vbcs,"PP");
					//}
                }
				else if (this.getName()!=null && (this.getName().IndexOf("minute",StringComparison.OrdinalIgnoreCase)==0 || this.getName().IndexOf("hour",StringComparison.OrdinalIgnoreCase)==0||this.getName().IndexOf("second",StringComparison.OrdinalIgnoreCase)==0))
				{
                    this.extractVerbCondition(vbcs, vbc, this.getName(), "PP");
				}
                foreach (SyntacticTree child in this.getChildren())
                {
					if (child!=null)
					{
                    	vbcs=vbcs.Concat(child.findVerbCondition(vbc)).ToList();
					}
                }
            }
			return vbcs;
		}

		public void extractVerbCondition(List<SyntacticTree> verbConditionList, List<Clause> vbc,string conditionName, string type1, string type2="")
		{
			/*Function Description : Takes a syntactic tree and look 
			* for its ancestors with type of type1 or type2. Add such 
			* ancestor node to verbConditionList. */	
			if (this.getType()==type1 || this.getType()==type2)
			{
				verbConditionList.Add(this);

				//Create a conditional clause
				string sentence = this.printSyntacticTree ();
				Clause cl = new Clause(conditionName,sentence);
				//cl.condition = sentence;
				cl.addLamdaExpression();
				vbc.Add (cl);
			}
			else if (this.giveParent()!=null)
			{
				this.giveParent().extractVerbCondition(verbConditionList,vbc,conditionName,type1,type2);
			}
		}

        public void verbify(List<Clause> clauses)
        {
            /*Function Description : Takes a syntactic tree and finds all verbs
             * and adds them to a clause list */
            if (this != null && this.getType()!=null)
            {
				if (this.getType().Equals("VB") || this.getType().Equals("VBD") || this.getType().Equals("VBP") || this.getType().Equals("VBG"))//|| this.getType().Equals("VBN") || this.getType().Equals("VBG") )//|| Global.stringIgnoreCaseExist(SyntacticTree.verbSpace, this.getName()))
                {
                    Clause cls = new Clause(this);
                    clauses.Add(cls);
                    SyntacticTree.verbSpace.Add(this.getName());
                }

                foreach (SyntacticTree child in this.getChildren())
                {
                    child.verbify(clauses);
                }
            }
        }

        public void attachNoun(List<Clause> clauses)
        {
            /*Function Description : Finds noun phrases or adv.s in the syntactic tree and attaches
             * these nouns or adv.s to verbs in the clause list*/
            if (this != null)
            {
                if (this.getType().Equals("NN") || this.getType().Equals("NNP") || this.getType().Equals("NNS"))
                {
                    this.findNounPhrase(clauses);
                }
				else if (this.getName() != null && (this.getName().Equals("it") || this.getName().Equals("It")))//||this.getName().Equals("them")))
                {
                    this.relateNoun(clauses);
                }
				else if (this.getType().Equals("RB")|| this.getType().Equals("RBR") || this.getType().Equals("RBS") || this.getType().Equals("RP"))
				{
					this.relateVerbSpecification(clauses);
				}
				else 
				{
               		foreach (SyntacticTree child in this.getChildren())
                	{
                    	child.attachNoun(clauses);
                	}
				}
            }
        }
		
		public void findNounPhrase(List<Clause> clauses)
		{
			/*Function Description : Find the noun phrase containing this node
			* and relate it to clauses
			* Precondition : this node is a noun.*/
			if (this!=null)
			{
				List<SyntacticTree> ancestors = new List<SyntacticTree>();
				ancestors.Add(this);
				SyntacticTree currentNode = this.giveParent();
				while (currentNode!=null)
				{
					if (currentNode.getType()=="NP")
					{
						foreach(SyntacticTree child in currentNode.getChildren())//if find a conjunction within the NP, break up the NP
						{
							if (child.getType()=="CC"|| child.getType()==",")
							{
								int counter=ancestors.Count-1;
								while(counter>=0)
								{
									if (counter==ancestors.Count-1)
									{
										if (ancestors[counter].getType()=="NP")
										{
											ancestors[counter].relateNoun(clauses);
											return;
										}
										else if (ancestors[counter].getType()=="NN"||ancestors[counter].getType()=="NNP"||ancestors[counter].getType()=="NNS")
										{
											int indexOfChild = currentNode.getChildren().IndexOf(child);
											int indexOfLastNode = currentNode.getChildren().IndexOf(ancestors[counter]);
											NounPhrase parsedNounPhrase=null;
											if (indexOfChild > indexOfLastNode)
											{
												parsedNounPhrase= new NounPhrase(currentNode.getChildren().GetRange(0,indexOfChild));
											}
											else
											{
												parsedNounPhrase= new NounPhrase(currentNode.getChildren().GetRange(indexOfChild+1,currentNode.getChildren().Count-indexOfChild-1));
											}
											ancestors[counter].relateNoun(clauses,parsedNounPhrase);
											return;
										}
									}
									else
									{
										if (ancestors[counter].getType()=="NP"||ancestors[counter].getType()=="NN"||ancestors[counter].getType()=="NNP"||ancestors[counter].getType()=="NNS")
										{
											ancestors[counter].relateNoun(clauses);
											return;
										}
									}
									counter--;
								}
							}
						}
					}
					else if (currentNode.getType()=="PP")//if find a "PP" tag, stop looking upward
					{
						break;
					}
						
					//added by Dipendra 
					/* as the case NP -> NP PP; PP -> NP was failing */

					if (currentNode.giveParent () != null && currentNode.giveParent ().children.Exists (x => x.getType () == "PP")) 
						break;
					ancestors.Add(currentNode);
					currentNode=currentNode.giveParent();
				}
				int count=ancestors.Count-1;
				while(count>=0)//find the top level NP or NN and relate it to clauses
				{
					if (ancestors[count].getType()=="NP"||ancestors[count].getType()=="NN"||ancestors[count].getType()=="NNP"||ancestors[count].getType()=="NNS")
					{
						ancestors[count].relateNoun(clauses);
						return;
					}
					count--;
				}
			}
		}

        public void relateNoun(List<Clause> clauses, NounPhrase nounPhrase=null)
        {
            /*Function Description : Find the verb acting on this noun and 
             * add to the corresponding clause this noun and its parsed noun phrase.*/

            //Extremely Stupid Method - attach noun to the nearest verb in syntactic tree
			
			//check if this noun phrase is a child of some noun phrase in clauses
			List<Tuple<Clause,NounPhrase>> deleteNoun=new List<Tuple<Clause,NounPhrase>>();
			foreach (Clause c in clauses)//Check if this noun phrase has common nodes with noun phrases in clauses
			{
				foreach (NounPhrase np in c.lngObj)
				{
					if (checkInclusion(np,this)==-1)//np includes this
					{
						return;
					}
					else if (checkInclusion(np,this)==1)// this includes np
					{
						deleteNoun.Add(new Tuple<Clause,NounPhrase>(c,np));
					}
				}
			}
			foreach (Tuple<Clause,NounPhrase> deletePair in deleteNoun)
			{
				Clause clause=deletePair.Item1;
				int index=clause.lngObj.IndexOf(deletePair.Item2);
				clause.lngObj.RemoveAt(index);
				//clause.returnNounList().RemoveAt(index);
			}
				
            List<SyntacticTree> doneNodes = new List<SyntacticTree>();
            List<SyntacticTree> activeNodes = new List<SyntacticTree>();
            activeNodes.Add(this);

            while (activeNodes.Count>0)
            {
                SyntacticTree nd=activeNodes[0];
                if (nd.getType().Equals("VB") /*|| nd.getType().Equals("VBN")*/ || Global.stringIgnoreCaseExist(SyntacticTree.verbSpace,nd.getName())) //its a verb
                {
                    //fix it
                    foreach (Clause clause in clauses)
                    {
                        if (clause.verb == nd)
                        {
							if (nounPhrase==null)
							{
								clause.lngObj.Add(new NounPhrase(this));
							}
							else
							{
								clause.lngObj.Add(nounPhrase);
							}
                            return;
                        }
                    }
                }

                foreach(SyntacticTree child in nd.getChildren())
                {
                    if(!doneNodes.Contains(child) && child!=null)
                    {
                        activeNodes.Add(child);
                    }
                }

                if(!doneNodes.Contains(nd.parent) && nd.parent!=null)
                {
                    activeNodes.Add(nd.parent);
                }

                activeNodes.Remove(nd);
                doneNodes.Add(nd);
            }
        }

        public int checkInclusion(SyntacticTree s, SyntacticTree t, List<Tuple<SyntacticTree, SyntacticTree, int>> memory)
        {
            /* Function Description : Given two node s,t this function returns the following code - 
            *       2 if s is the same as t,
            *        1 if s is a descendant of t,
            *       -1 if t is descendant of s, 
            *       or 0 if s and t do not include each other.*/

            if (s == t)
            {
                memory.Add(new Tuple<SyntacticTree, SyntacticTree, int>(s, t, 2));
                return 2;
            }
            else
            {
                //check in memory
                foreach (Tuple<SyntacticTree, SyntacticTree, int> entry in memory)
                {
                    if (entry.Item1 == s && entry.Item2 == t)
                        return entry.Item3;
                }

                foreach (SyntacticTree child in s.getChildren())
                {
                    int childReturns = this.checkInclusion(child, t, memory);
                    if (childReturns == 2 || childReturns == -1) //if t is same as child or is a descendant of child then t is a descendant of s
                    {
                        memory.Add(new Tuple<SyntacticTree, SyntacticTree, int>(s, t, -1));
                        return -1;
                    }
                }
                foreach (SyntacticTree child in t.getChildren())
                {
                    int childReturns = this.checkInclusion(child, s, memory);
                    if (childReturns == 2 || childReturns == -1) //if s is same as child or is a descendant of child then s is a descendant of t
                    {
                        memory.Add(new Tuple<SyntacticTree, SyntacticTree, int>(s, t, 1));
                        return 1;
                    }
                }
                memory.Add(new Tuple<SyntacticTree, SyntacticTree, int>(s, t, 2));
                return 0;
            }
        }

        public int checkInclusion(NounPhrase np, SyntacticTree t)
        {
            /*Function Description : Return 1 if np is a child of t,
            * -1 if t is child of np, 
            * or 0 if s and t do not include each other.*/
            foreach (SyntacticTree s in np.getOriginalNounPhrase())
            {
                int stInclusion = checkInclusion(s, t, new List<Tuple<SyntacticTree,SyntacticTree,int>>());
                if (stInclusion == 2 || stInclusion == -1) //if s is same as t or t is a descendant of s
                    return -1;
                else if (stInclusion == 1)
                    return 1;
            }
            return 0;
        }
			
		public void relateVerbSpecification(List<Clause> clauses)
		{
           /*Function Description : Find the verb acting on this verb specification and 
            * add to the corresponding clause.*/
            List<SyntacticTree> doneNodes = new List<SyntacticTree>();
            List<SyntacticTree> activeNodes = new List<SyntacticTree>();
            activeNodes.Add(this);

            while (activeNodes.Count>0)
            {
                SyntacticTree nd=activeNodes[0];
                if (nd.getType().Equals("VB") /*|| nd.getType().Equals("VBN") */|| Global.stringIgnoreCaseExist(SyntacticTree.verbSpace,nd.getName())) //its a verb
                {
                    //fix it
                    foreach (Clause clause in clauses)
                    {
                        if (clause.verb == nd)
                        {
							clause.verbSpecification.Add(this);
                           		return;
                        }
                    }
                }

                foreach(SyntacticTree child in nd.getChildren())
                {
                    if(!doneNodes.Contains(child) && child!=null)
                    {
                        activeNodes.Add(child);
                    }
                }

                if(!doneNodes.Contains(nd.parent) && nd.parent!=null)
                {
                    activeNodes.Add(nd.parent);
                }

                activeNodes.Remove(nd);
                doneNodes.Add(nd);
            }
		}
		
        public SyntacticTree findNextRelationNode()
        {
            /*Function Description : Find Next Relation Node. Immediate to the right*/

            //we begin by assuming that no relationtype node exists in the sub-tree so we move up
            //go to parent and find where current lies in the children list

            if (this.parent == null)
                return null;
            int i;
            for(i=0;i<this.parent.children.Count;i++)
            {
                if(this.parent.children[i]== this)
                    break;
            }
            //we now travel through all the remaining children
            for (int j = i + 1; j < this.parent.children.Count; j++)
            {
                SyntacticTree result = this.parent.children[j].findRelationNodes();
                if (result != null)
                    return result;
            }

            //we now recursively call on this.parent
            return this.parent.findNextRelationNode();
        }

        public SyntacticTree findRelationNodes()
        {
            /*Function Description : Find relations type node in the sub-tree*/
            List<String> possibleRelation = new List<String>() { "TO", "IN", "CC", "," };
            foreach (String rln in possibleRelation)
            {
                if (this.type.Equals(rln))
                    return this;
            }

            foreach (SyntacticTree child in this.children)
            {
               SyntacticTree result=child.findRelationNodes();
               if (result != null)
                   return result;
            }

            return null;
        }

		public string printSyntacticTree()
		{
			/*Function Description : return the phrase rooted from this node*/
			string phrase="";
			if (this.getName()!=null)
			{
				return this.getName()+" ";
			}
			else
			{
				foreach (SyntacticTree child in this.getChildren())
				{
					phrase=phrase+child.printSyntacticTree();
				}
			}
			return phrase;
		}
    }
}
