using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*Comments: Dipendra 
* 1. why is main noun a list of syntactic tree and not just one syntactic tree 
* 2. would you rather prefer a datastructure of the form [specifier-type, specifier]
* which encapsulates adjectives, determiner and state conditions. Example - 
* [determiner, "the"], [adjective, "red], [state-condition,"(state cup water)"] ...
* rather than having multiple list? Thoughts.
**/

namespace ProjectCompton
{
    class NounPhrase
    {
        /*Class Description : Defines data-structure NounPhrase which is used in Clause*/
		private List<SyntacticTree> originalNounPhrase =new List<SyntacticTree>(); //the unprocessed original noun phrase
		private List<SyntacticTree> mainNoun=new List<SyntacticTree>(); //the main noun in a noun phrase
		private List<SyntacticTree> objectSpecification=new List<SyntacticTree>(); //contains specification like "that contains water"
        private List<SyntacticTree> adjs = new List<SyntacticTree>(); //contains adjs for the main noun like "white"
		private List<SyntacticTree> determiner = new List<SyntacticTree>(); // contains determiner like "a", "the", "all","every"
		private List<Tuple<String,NounPhrase>> SpatialRelationTree = new List<Tuple<String,NounPhrase>> (); // contains children to this NounPhrase. The string represents a spatial relation, like "of", "on", "in front of" etc.

        public NounPhrase(SyntacticTree nounPhrase)
        {
            /* Constructor Description : parse the syntactic tree 
			* in to main noun, obj-specification, adjs, and other 
			* words */
			this.originalNounPhrase.Add(nounPhrase);
			
			//Step 1 : find the SBAR, PP TAGS
			this.findObejectSpecification(nounPhrase);
			
			//Step 2 : find the main noun, adj.s, and determiners
			this.findParts(nounPhrase);
			//Console.WriteLine(this.getNPhraseSen(this.originalNounPhrase));

			//Console.WriteLine (this.output ());
			foreach (Tuple<String,NounPhrase> i in this.SpatialRelationTree){
				//Console.WriteLine("Child"+i.Item1+i.Item2.output());
			}

		}

        public NounPhrase(List<SyntacticTree> nounPhrase)
        {
            /* Constructor Description : parse the list of syntactic trees
			* in to main noun, obj-specification, adjs, and other 
			* words */
            foreach (SyntacticTree np in nounPhrase)
			{
				this.originalNounPhrase.Add(np);
				
				//Step 1 : find the SBAR, PP TAGS
				this.findObejectSpecification(np);
				
				//Step 2 : find the main noun, adj.s, and determiners
				this.findParts(np);
			}
        }
		
		public List<SyntacticTree> getMainNoun()
		{
			/*Function Description : return main noun*/
			return this.mainNoun;
		}
		
		public List<SyntacticTree> getOriginalNounPhrase()
		{
			/*Function Description : return original noun phrase*/
			return this.originalNounPhrase;
		}
			
		public string output()
		{
			/* Function Description : Ouput the comtents of this NounPhrase
			* in the format:{main-noun:{}, adj:{}, object-specification:{}, determiner:{}, other{}}*/
			string output="{";
			foreach (SyntacticTree t in this.originalNounPhrase)
			{
				output=output+t.printSyntacticTree()+ " ";
			}
			output=output+", mn:{";
			foreach (SyntacticTree t in this.mainNoun)
			{
				output=output+t.printSyntacticTree()+ ",";
			}
			if (this.mainNoun.Count!=0)
				output=output.Substring(0,output.Length-1);
			output=output+"},adj:{";
			foreach (SyntacticTree t in this.adjs)
			{
				output=output+t.printSyntacticTree()+ ",";
			}
			if (this.adjs.Count!=0)
				output=output.Substring(0,output.Length-1);
			output=output+"},os:{";
			foreach (SyntacticTree t in this.objectSpecification)
			{
				output=output+t.printSyntacticTree()+ ",";
			}
			if (this.objectSpecification.Count!=0)
				output=output.Substring(0,output.Length-1);
			output=output+"},dt:{";
			foreach (SyntacticTree t in this.determiner)
			{
				output=output+t.printSyntacticTree()+ ",";
			}
			if (this.determiner.Count!=0)
				output=output.Substring(0,output.Length-1);
			output=output+"}}";
			return output;
		}
		
		public void findObejectSpecification(SyntacticTree phrase)
		{
			/*Function Description: find object specifications in phrase and
			* add to objectSpecification*/
			if (phrase.getType () == "SBAR") 
			{
				this.objectSpecification.Add (phrase);
			} 
			else if (phrase.getType () == "PP"&&phrase.getChildren().Count>=2) 
			{
				SyntacticTree p = phrase.getChildren () [0];
				SyntacticTree np = phrase.getChildren () [1];
				SyntacticTree f = np.getChildren () [0];
				NounPhrase node = null;
				String relation = "";
				if (p.getName () == "in" && f.getChildren ().Count != 0 && f.getChildren () [0].getName () == "front" && np.getChildren ().Count > 1) 
				{
					relation = "in front of";
					node = new NounPhrase (np.getChildren () [1].getChildren () [1]);
				} 
				else 
				{
					relation = p.getName ();
					node = new NounPhrase (phrase.getChildren()[1]);
				}
				this.SpatialRelationTree.Add (new Tuple<String,NounPhrase>(relation,node));
			}
			else
			{
				foreach(SyntacticTree child in phrase.getChildren())
				{
					this.findObejectSpecification(child);
				}
			}
    	}
		
		public void findParts(SyntacticTree phrase)
		{
			/*Function Description: find main nouns, adj.s, and determiners in phrase and
			* add to mainNoun*/
			if (phrase.getType().Equals("NN") || phrase.getType().Equals("NNP") || phrase.getType().Equals("NNS")|| phrase.getType().Equals("PRP"))
			{
				this.mainNoun.Add(phrase);
			}
			else if (phrase.getType().Equals("JJ"))
			{
				this.adjs.Add(phrase);
			}
			else if (phrase.getType().Equals("DT"))
			{
				this.determiner.Add(phrase);
			}
			else if (!phrase.getType().Equals("PP")&&!phrase.getType().Equals("SBAR"))
			{
				foreach(SyntacticTree child in phrase.getChildren())
				{
					if (child!=null)
					{
						this.findParts(child);
					}
				}
			}
		}

		public string getNPhraseSen(List<SyntacticTree> trees)
        {
            /* Function Description: Returns the string given by the
             * original noun-phrase */
            String sentence = "";
            foreach(SyntacticTree s in trees)            
				sentence = sentence + s.printSyntacticTree() + " ";
            return sentence;
        }

		public String getName()
		{
			String name = "";
			foreach (SyntacticTree st in this.getMainNoun())
				name = name + " " + st.getName ();
			return name.Trim();
		}

        public void storeXML(Logger lg)
        {
            /* Function Description : Store the noun-phrase in an xml format */
			lg.writeToParserData("<NounPhrase><Original>" + this.getNPhraseSen(this.originalNounPhrase) + "</Original>");
            lg.writeToParserData("<MainNoun>" + Global.getString(this.mainNoun) + "</MainNoun>");
			lg.writeToParserData("<ObjectSpecification>" + this.getNPhraseSen(this.objectSpecification) + "</ObjectSpecification>");
            lg.writeToParserData("<Adjectives>" + Global.getString(this.adjs) + "</Adjectives>");
            lg.writeToParserData("<Determiner>" + Global.getString(this.determiner) + "</Determiner></NounPhrase>");
        }
	}
}
