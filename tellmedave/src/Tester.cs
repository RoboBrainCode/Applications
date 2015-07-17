/* Tell Me Dave 2013-15, Robot-Language Learning Project
 * Code developed by - Dipendra Misra (dkm@cs.cornell.edu)
 * working in Cornell Personal Robotics Lab.
 * 
 * More details - http://tellmedave.com
 * This is Version 3.0 Beta Released: April, 2015 */

/*  Notes for future Developers - 
 *    <no - note >
 */


using System;
using System.Collections.Generic;
//using System.Core;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
//using System.Data;
//using System.Data.SQLite;

namespace ProjectCompton
{
    class Tester
    {
        /* Class Description : Is the main class which provides functionalities for testing and running the algorithm.
         * At this point the inference is also part of this class but there are plans to move it to a separate class.
         * This class also provides baseline algorithms.*/

		public List<VerbProgram> lexicon{get; private set;}            //lexicon
		public List<Environment> envList{get; private set;}
		public Dictionary<String,Boolean> methods{ get; private set;}

		public List<List<Environment>> listOfAllEnv{get; private set;}

		public Inference inf{ get; private set;}       //Inference object
		public Learning lrn{get; private set;}
        private Features ftr = null;                   //Feature object
        private Random rnd = null;                     //Random Number Generator to be used the entire program
        public Simulator sml = null;                   //Simulator Object
        public Parser prs = null;                      //Parser Object
        public SymbolicPlanner symp = null;            //Symbolic Planner Object
        public DataAnalysis datany = null;             //Data Analysis Object
        public Metrics mtr = null;                     //Metric Object 
        public Logger lg = null;                       //Logger Object
        public System.Diagnostics.Stopwatch ss = null; //stopwatch Object
		public int prevTimeStmp = 0; 				   //stores last stopwatch recording
        public bool jumps = false;

        //Constructors
		public Tester()
        {
            // Constructor Description : Initializes the public objects
            this.ss = new System.Diagnostics.Stopwatch(); //stopwatch
            this.ss.Start(); //starts the stop-watch, Stop-watch should be started as soon as possible
			this.envList =  new List<Environment>();
			this.listOfAllEnv = new List<List<Environment>> ();
            this.sml = new Simulator(); //simulator
            this.prs = new Parser(); //parser
            this.mtr = new Metrics(this); //metric
            this.lg = new Logger(); //logger
            this.symp = new SymbolicPlanner(this.lg); //symbolic planner
            this.datany = new DataAnalysis(this.prs, this.envList, this.lg); //Data Analysis Object
			this.lexicon = new List<VerbProgram>(); //veil library
            this.rnd = new Random();//random number generator
            this.ftr = new Features(this.lg, this.prs, this.sml);
            this.inf = new Inference(this.lg, this.sml, this.symp, this.lexicon, this.envList, this.prs, this.ftr);
			this.lrn = new Learning(this);

			this.methods = new Dictionary<string, bool> ();
			this.methods.Add ("AccScoreLatentTrim",false);
			this.methods.Add ("TemplateBased",false);
			this.methods.Add ("TreeExploration",false);
			this.methods.Add ("Chance", false);
			this.methods.Add ("UBL_Baseline", false);
			this.methods.Add ("RSS_2015_Generation Only", false);
			this.methods.Add ("RSS_2015_Generation+Storage", false);
			this.methods.Add ("RSS_2015_VEIL-Template Only", false);
			this.methods.Add ("RSS_2015_Generation+VEIL-Template", false);
			this.methods.Add ("RSS_2015_Generation+VEIL-Template-Storage",true);
		}

        public void destroyer()
        {
            // Function Description: Destroys the data-structures
            this.lexicon.Clear();
            this.ftr.destroyer();
        }

        public void writeTime(String information="")
        {
            // Function Description: Writes the elapsed time
			int currentTime = this.ss.Elapsed.Seconds;
			this.lg.writeToFile (information + "<span style='margin-left:20px; color:brown;'>Time Since Last Stamp: "
			                     +(currentTime-this.prevTimeStmp)+"; &nbsp;&nbsp;&nbsp;&nbsp;Total Time Elapsed: " + currentTime + " sec</span>");
			this.prevTimeStmp = currentTime;
        }

		public List<List<List<double>>> initEvaluation()
		{
			// Function Description: Returns evaluation datastructure
			List<List<List<double>>> evaluation = new List<List<List<double>>> (); // Method [ Metric [ Score]]
			for (int i = 0; i < this.methods.Count(); i++)
			{
				evaluation.Add(new List<List<double>>());
				for(int mtr=0; mtr<Metrics.numMetrics;mtr++)
					evaluation[i].Add(new List<double>());
			}
			return evaluation;
		}

        public void displayStructure()
        {
            /*Function Description : Displays the verbProgram datastructre*/
            String listOfVerbs = "";
            foreach (VerbProgram v in this.lexicon)
                listOfVerbs = listOfVerbs + ", " + v.getName();
            lg.writeToFile("<h3>The Learned Verb Model</h3><br/>Learned Unique Names : " + listOfVerbs);
            foreach (VerbProgram v in this.lexicon)
                v.display(this.lg);
        }

        public List<Tuple<List<int>, List<int>>> crossValidator()
        {
            /*Function Description: Creates a cross validation sample. We experiment with two type
			 * of cross validation- test has unseen environments and test has unseen tasks. */

            List<Tuple<List<int>, List<int>>> datas = new List<Tuple<List<int>, List<int>>>();
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> allData = this.prs.returnAllData ();

			/* For this, the algorithm needs to separate the dataset into [train,test] pair such that no
             * environment(task) from one exists in the other. In VEIL-1000 dataset which this version uses, there
             * are 20 environments for 2 scenarios. The algorithm trains on 16 environments(8 task) and tests on the
             * remaining 4(2). These are kept 2(1)from each scenario. The folds are
             * Env: [ {train:(1-8, 11-18), test:(1-2, 11-12)}, {train:(1-2 U 5-10, 11-13 U 15-20), test:(3-4, 13-14)}, .... { train:(1-8, 11-18), test:(9-10, 19-20) }
             * Task: [ {train:(2-5, 7-10), test:(1, 6)}, {train:(1-1 U 3-5, 6-6 U 8-10), test:(2, 7)}, .... { train:(1-4, 6-9), test:(5, 10) }
             * there are always 5 folds. */

			for (int fold = 1; fold <= 5; fold++)
			{
				List<int> train = new List<int> ();
				List<int> test = new List<int> ();
				//first item of first tuple is the environment [1-20] and second is the objective [1-10]
				for (int iter = 0; iter < allData.Count(); iter++) 
				{
					Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> t = allData [iter];
					int environment = t.Item1.Item1, objective = t.Item1.Item2;
					if (Constants.scheme == CrossValidationScheme.Environment) 
					{
						if (2 * (fold - 1) + 1 <= environment && environment <= 2 * (fold - 1) + 2
							|| 2 * (fold - 1) + 11 <= environment && environment <= 2 * (fold - 1) + 12)
							test.Add (iter);
						else
							train.Add (iter);
					}
					else if(Constants.scheme==CrossValidationScheme.Task)
					{
						if (fold == objective || fold +5 == objective )
							test.Add (iter);
						else
							train.Add (iter);
					}
				}

				#region permute_the_test
				/*Random rnd = new Random();
				for(int i=0; i<test.Count(); i++) //Knuth Shuffle Algorithm
				{
					int tmp = test[i];
					int r = rnd.Next(i, test.Count());
					test[i] = test[r];
					test[r] = tmp;
				}*/
				#endregion

				datas.Add (new Tuple<List<int>, List<int>> (train, test));
			}
            return datas;
		}

        public void constructDataStructure(List<int> train)
        {
            /* Function Description: Given training data, it constructs
             * the data structure and features */
            this.induceLexiconFromTraining(train);                      // build VEIL dataset
			this.ftr.constructFeatureDataStructures (train, this.methods.Values.ToList(), this.lexicon,
			                                         this.envList, this.listOfAllEnv, this.inf.sensim);

            if (this.methods.ElementAt(0).Value)
                this.ftr.buildBagOfWordRelation(train);
        }

        public void createAndAddVEILTemplate(List<Instruction> insts, int start, int end, Clause cls, Environment env, int entry)
        {
            /* Function Description : Add the clause entry to the VEIL dataset
             * insts is a sequence of instruction of which insts[start:end] is relevant
             * cls and env are the given clause and environment. entry is the entry in dataset
             * from which this data is coming */

            //Get the correct instruction sequence
            List<Instruction> instructionSequence = new List<Instruction>();
            if (start <= end) //non-empty instruction sequence
            {
                for (int i = start; i <= end; i++)
					instructionSequence.Add(insts[i].makeCopy());  //copy the instruction here
            }

            //Copy the clause
            Clause clsCopy = cls;//.makeCopy(); //makecopy doesnt copy. Be wary.
            Environment envCopy = env.makeCopy();

            //Create the template
            LexicalEntry vtmp = new LexicalEntry(clsCopy, instructionSequence, envCopy, entry, this.sml);
			vtmp.leCorrMatrix = vtmp.env_.getLECorrMatrix (vtmp.cls_,insts.GetRange(0,start),this.inf.sensim, this.ftr);
			this.addVEILTemplate (vtmp);
        }

		public void addVEILTemplate(LexicalEntry vtmp)
		{
			/* Function Description: add the veil template to the list of program */
			String verbName = vtmp.cls_.verb.getName ();
			bool added = false;
			foreach (VerbProgram vprog in this.lexicon)
			{
				if (vprog.getName().Equals(verbName, StringComparison.OrdinalIgnoreCase)) //add to the exisiting condition
				{
					added = true;
					vprog.add(vtmp);
				}
			}

			if (!added)//not added
			{
				//create a new entry
				VerbProgram vprog = new VerbProgram(verbName);
				vprog.add(vtmp);
				this.lexicon.Add(vprog); //add the verb program
			}
		}

        public void loadAllEnv()
        {
            /* Function Description : It parses and stores two environment - 
             * 1. Starting environment which are stored in xml file.
             *    The environment is represented by a graph where node represent
             *    an object and edges represents relationship between objects 
             * 2. Load the intermediate environment for all the points in the dataset. 
             *    Each point has an instruction sequence I{1...n}. This function stores
             *    the environment {E0...En} where E0 is a starting environment already loaded
             *    in step 1. And Ei = simulator(Ei-1, Ii)*/

            //Load the starting environment and store it in envList
            /*foreach (String scn in Constants.scenarios)
            {
                for (int i = 1; i <= Constants.numEnvironment; i++)
                {
                    Environment env = new Environment();
                    env.loadEnvironment(scn + "/" + scn + i.ToString() + ".xml");
                    this.envList.Add(env);
                }
            }*/
            for (int i = 1; i <= Constants.numEnvironment; i++)
            {
            	Environment env = new Environment();
	            env.loadEnvironment("planit/livingRoom"+i+".xml");
	            this.envList.Add(env);
            }
        }

		public void loadIntermediateEnv()
		{
			//Function Description: Load intermediate environment
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = this.prs.returnAllData();
			for(int i=0; i<datas.Count(); i++)
			{
				/* We need to find different Environment at different level of instruction
                 * These environments will NOT be modified hence same environment can be inserted at different level */
				Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> data = datas [i];
				Environment begin = this.envList[data.Item1.Item1 - 1];
				List<Environment> listEnv = new List<Environment>() { begin };
				Console.WriteLine ("Data " + i + " out of " + datas.Count ());

				foreach (Instruction inst in data.Item3)
				{
					if(inst == null)
						throw new ApplicationException ("Instruction is null");
					Environment tail = listEnv[listEnv.Count - 1];
					Environment result = this.sml.execute(inst, tail, true);  //execute the instruction inst on the tail environment
					if (result == null) 
						throw new ApplicationException ("Load Intermediate Environment: Error in executing instruction");
					listEnv.Add(result);
				}

				if(listEnv.Count() != data.Item3.Count()+1)
					throw new ApplicationException ("List of Environments not same as instructions");
				this.listOfAllEnv.Add(listEnv);
			}
		}

        public void induceLexiconFromTraining(List<int> train)
        {
            /* Function Description : Creates VEIL datastructure from the training dataset */

            List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> data = this.prs.returnAllData();
            List<List<Tuple<int, int>>> alignments = this.prs.returnAllAlignment();
			//Console.WriteLine ("Data "+data.Count()+" and alignment "+alignments.Count());

            foreach (int index in train)
			{
				//Console.WriteLine ("Working on point " + index);
                Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> info = data[index];
                int numClause = info.Item4.Count();

                for (int i = 0; i < numClause; i++)                  //iterating over each single-verb event clause
                {
                    Clause cls = info.Item4[i];
                    int numSubClauses = cls.numNodes();
                    Environment env = this.listOfAllEnv[index][alignments[index][i].Item1];
                    int start = alignments[index][i].Item1;
                    int end = alignments[index][i].Item2;

					if (end < start) //empty instruction sequence
						continue;

                    if (numSubClauses == 0)
                        this.lg.writeToErrFile("Could Not Find A Single Clause for : { " + info.Item4[i].sentence + " } Source of Error (Entry " + index + ")");//report error
                    else if (numSubClauses == 1)
                        this.createAndAddVEILTemplate(info.Item3, start, end, info.Item4[i].rootClause(), env, index);
                    else //wrong but let's go with the first one for the moment
                    {
                        this.createAndAddVEILTemplate(info.Item3, start, end, info.Item4[i].rootClause(), env, index);
                        this.lg.writeToErrFile("Found Many Clauses for : { " + info.Item4[i].sentence + " } Source of Error (Entry " + index + ")<br/>");
                    }
                }
            }
        }

		public void createNewTemplatesFromUnsupervisedData(List<bool> methods, List<object> param)
		{
			/* Function Description: The algorithm benefits from unsupervised data and we use this data to 
			 * to pump in new templates. */

			List<Tuple<Tuple<int, int>, Clause, List<Clause>>> unlabelledData = this.prs.unlabelledData;

			for (int i = 0; i < unlabelledData.Count(); i++)
			{
				Console.WriteLine ("Pumping Data Using "+i.ToString()+"\n-----------------\n");
				Tuple<Tuple<int, int>, Clause, List<Clause>> testSample = unlabelledData[i];
				if (testSample.Item2 == null)
					continue;

				Tuple<int, int> envAndObjective = testSample.Item1;
				for (int method = 0; method < this.methods.Count(); method++) // Iterating Overall Algorithms
				{
					if (!this.methods.ElementAt(method).Value)
						continue;

					switch (method)
					{
						case 6: //Main Model using only generated templates
								List<object> param4 = new List<object> () { (object)false, (object)true };
								inf.acl2015 (testSample.Item2, this.envList [envAndObjective.Item1 - 1], this, (Dictionary<String, Double>)(param [5]), param4);
								break;
						case 9:	//Main Model using both veil and generated templates
								List<object> param6 = new List<object> () { (object)true, (object)true };
								inf.acl2015 (testSample.Item2, this.envList [envAndObjective.Item1 - 1], this, (Dictionary<String, Double>)(param [8]), param6);
								break;
						default: break;
					}
				}
			}
		}

		public List<Instruction> onlineInference(String text, int envIndex,List<object> param)
		{
			/* Function Description: Given a text and environment index. Algorithm parses and finds
			 * optimal instruction sequence.*/

			this.lg.writeToFile("<div style='border:1px solid red;'><span> Solving Problem Environment: "+envIndex+") <br/> Text : <i>"+ text + "</i></span><br/>");

			Clause cls =  this.prs.shallowParsing(text, lg); //shallow parse the text into clause
			if(cls == null)
				Console.WriteLine("Clause is null");
			else Console.WriteLine("Num nodes "+cls.numNodes());

			Dictionary<String,Double> d = (Dictionary<String,Double>)(param[9]);
			List<object> param9 = new List<object> () { (object)true, (object)true, (object)true };

			this.lg.writeToFile ("Solving problem; text: " + text + " and environment " + envIndex);
			List<Instruction> inferred = inf.acl2015 (cls, this.envList [envIndex - 1], this, (Dictionary<String, Double>)(param [9]), param9);
			inferred = Global.filter (inferred);
			NoiseRemoval.instSeqCleaning (inferred, this.envList [envIndex - 1], this.sml);
			this.lg.writeToFile("</div>");

			return inferred;
		}

		public void storeLexicon()
		{
			/* Function Description: Stores lexicon in a xml file to be used for
			 * transfer learning */
			System.IO.StreamWriter writer = new System.IO.StreamWriter (Constants.weblink+@"/BootstrapLexicon.xml");
			writer.Write ("<lexicon>");
			foreach(VerbProgram lexicon_ in this.lexicon)
			{
				foreach (LexicalEntry lexicalEntry in lexicon_.program) 
				{
					writer.WriteLine("<lexical_item verb=\""+lexicalEntry.cls_.verb.getName()+"\">");
					/* Standardize the post-condition
					 * due to noise removal; the post-condition often contains more objects than 
					 * there are variables. We standardize the variables e.g.,
					 * (state $var1 $var5) $var1 -> obj1; $var2 -> obj2; ... $var5 -> obj5
					 * gets standardized as (state $var1 $var2) $var1 -> obj1; $var2 -> obj2 */

					List<String> variables = new List<String> (); //e.g. $var1, $var4, $var5
					List<String> objNames = new List<String> (); //Mug_2, Cup_3, ....

					foreach (String term in lexicalEntry.predicatesPost) 
					{
						String[] atom = Global.getAtomic (term).Item2.Split(new char[]{' '}).ToArray();
						Console.WriteLine ("Term : " + term + " with atoms " + atom.Length);
						if (atom [1].StartsWith ("$"))
							variables = variables.Union (new List<String> () { atom[1] }).ToList();
						if(atom[2].StartsWith ("$"))
							variables = variables.Union (new List<String> () { atom[2] }).ToList();
					}

					String newPostCondition = String.Join ("^", lexicalEntry.predicatesPost);

					for (int varindex=1; varindex<=variables.Count(); varindex++) 
						newPostCondition = newPostCondition.Replace (variables [varindex - 1], "$temp" + varindex.ToString ());
					for (int varindex=1; varindex<=variables.Count(); varindex++) 
						newPostCondition = newPostCondition.Replace ("$temp" + varindex.ToString (), "$var" + varindex.ToString ());

					for (int varindex=1; varindex<=variables.Count(); varindex++) 
					{
						int orig = Int32.Parse(variables [varindex-1].Substring ("$var".Length));
						objNames.Add (lexicalEntry.env_.objects[lexicalEntry.xiOrigMappingPredicatePost[orig-1]].uniqueName);
					}

					Console.WriteLine ("Post Condition "+newPostCondition+" and "+String.Join(", ",objNames));

					writer.WriteLine ("<postcondition>" + newPostCondition + "</postcondition>");
					for (int varindex=0; varindex<variables.Count(); varindex++) 
						writer.WriteLine ("<object>" + objNames [varindex] + "</object>");
					writer.WriteLine("</lexical_item>");
				}
			}
			writer.Write ("</lexicon>");
			writer.Flush ();
			writer.Close ();
		}

		/*public List<double[]> readWeights()
		{
			System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(Constants.rootPath + "weightsFile.xml");
			Console.WriteLine("hello i m here");
			Console.WriteLine(reader.Read());
			List<double[]> pivot = new List<double[]>();
			return pivot;
		}*/

		public void bootstrapLexicon()
		{
			/* Function Description: Bootstrap lexicon from xml file 
			 * <lexicon>
			 * 		<lexical_item verb="name">
			 * 			<postcondition>constraints</postcondition>
			 *			<object>name</object>
			 * 			<object>name</object>
			 * 			....
			 * 		</lexical_item>
			 * 		....
			 * </lexicon>
			 * Ideally, use RoboBrain for fetching the lexicon */

			System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(Constants.weblink + "BootstrapLexicon.xml");
			List<String> postcondition = null;
			Environment env = null;
			Clause cls = null;

			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case System.Xml.XmlNodeType.Element:
						if (reader.Name.Equals ("lexical_item")) 
						{
							String verb = reader.GetAttribute (0);
							SyntacticTree st = new SyntacticTree ();
							st.changeVerb (verb);
							cls = new Clause (st);
							env = new Environment ();
						}
						if (reader.Name.Equals ("postcondition")) 
						{
							reader.Read ();
							if(reader.Value.Length>0)
								postcondition = reader.Value.Split (new char[] { '^' }).ToList ();
						}
						if (reader.Name.Equals ("object")) 
						{
							reader.Read ();
							Object obj = new Object ();
							obj.uniqueName = reader.Value;
							env.objects.Add (obj);
						}
						break;

					case System.Xml.XmlNodeType.EndElement:
						if (reader.Name.Equals ("lexical_item")) 
						{
							if (postcondition == null || postcondition.Count () == 0) 
								continue;

							Clause cls_ = cls;
							List<String> postcondition_ = postcondition;
							Environment env_ = env;

							LexicalEntry vtmp = new LexicalEntry (cls_, postcondition_, env_);
							this.addVEILTemplate (vtmp);

							cls = null;
							postcondition = null;
							env = null;
						}
						break;
				}
			}
		}

		//Deprecated
		/*public void printWeigths(List<double[]> pivot)
		{
			//to write the weight file	
			System.IO.StreamWriter weightsFile = new System.IO.StreamWriter (Constants.rootPath + "weightsFile.xml");
			weightsFile.Write("<WEIGHTS>");
			foreach(double[] item in pivot){
				Console.WriteLine(item.Count());
				weightsFile.Write("<weights>");
				foreach(double val in item)
					weightsFile.Write("<int>"+val.ToString()+"</int>");
				weightsFile.WriteLine("</weights>");
			}
			weightsFile.Write("</WEIGHTS>");
			weightsFile.Flush ();
			weightsFile.Close ();
		}*/

		public List<double[]> bootstrapWeights() //marked for deprecation in next release
		{
			/* Function Description: Bootstraps weight from xml file.
			 * Ideally, use RoboBrain for fetching weights. */
			
			//System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader("http://10.132.4.205/arpit/weightsFile.xml");
			//System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader("http://localhost/weightsFile.xml");
			System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(Constants.weblink+"weightsFile.xml");
			List<double[]> weights = new List<double[]> ();
			int mthCounter = 0;
			int counter = 0;
			double [] weight = Enumerable.Repeat (0.0,Features.featureNames[counter].Count()).ToArray();
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case System.Xml.XmlNodeType.Element:
						if(reader.Name.Equals("weights"))
						{
							weight  = Enumerable.Repeat (0.0,Features.featureNames[mthCounter].Count()).ToArray();
							mthCounter++;
							counter = 0;
						}
						else if(reader.Name.Equals("int"))
						{
							reader.Read();
							weight[counter] = Convert.ToDouble(reader.Value);
							counter++;
							
						}		
						break;
					case System.Xml.XmlNodeType.EndElement:
						if(reader.Name.Equals("weights"))
							weights.Add(weight);
						break;
				}
			}

			/*Console.WriteLine("constructed weights");
			foreach (double[] item in weights)
				{
					Console.Write("printing weights: ");
					foreach(int var in item)	
						Console.Write(var+" ");
					Console.WriteLine("");
				}
			*/
			return weights;
		}

		public List<List<double[]>> inference(List<bool> methods, List<int> test, List<object> param)
        {
            /* Function Description : Takes input from Main function of which
             * type of testing method to use and gives the output */

			//test = test.GetRange(20,3).ToList();
            List<List<double[]>> scores = new List<List<double[]>>(); //Method [ Metric [ Score ]  ]
            for (int i = 0; i < methods.Count(); i++)
            {
                scores.Add(new List<double[]>());
				for (int mtr=0; mtr< Metrics.numMetrics; mtr++)
					scores [i].Add (new double[test.Count ()]);
            }

			List<Tuple<String,List<Instruction>>> ublOutput = null;
			if (this.methods.ElementAt (4).Value) //UBL baseline works best on entire test not per point
				ublOutput = this.inf.ublBaseline (test);

			List<double> cumulativeIED = new List<double> ();
			List<double> cumulativeEED = new List<double> ();
			double cumulIED = 0, cumulEED = 0;

			//this.obj.getParserAccuracy (test);

            List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> datas = this.prs.returnAllData();
            for (int i = 0; i < test.Count(); i++)
            {
				Console.WriteLine ("Working on Test Case "+i.ToString()+"\n-----------------\n");
                Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> testSample = datas[test[i]];
                if (testSample.Item2 == null)
                    continue;

				List<Instruction> groundtruth = testSample.Item3;
                Tuple<int, int> envAndObjective = testSample.Item1;
                String sentence = "";
				if (testSample.Item2.getSubTreeSentence () != null)
					sentence = testSample.Item2.getSubTreeSentence ();
                this.lg.writeToFile("<div style='border:1px solid red;'><span> Solving Problem ( Entry " + test[i] +
				                    " Environment: "+envAndObjective.Item1+") <br/> Sentence : <i>"+ sentence + "</i></span><br/>");
                for (int method = 0; method < this.methods.Count(); method++) // Iterating Overall Algorithms
                {
                    if (!this.methods.ElementAt(method).Value)
                        continue;
					this.writeTime ("Test Case "+i.ToString()+" "+this.methods.ElementAt(method).Key);
					List<Instruction> inferred = null;
                    // Running the Particular Algorithm on this test data point
                    this.lg.writeToFile("<span> Using Method Name " + this.methods.ElementAt(method).Key + "</span><br/>");
                    switch (method)
                    {
						case 0: inferred = inf.misra2014(envAndObjective, this.envList[envAndObjective.Item1 - 1], this, 5, (Dictionary<String, Double>)(param[0]));// this.distanceMethodWithLatentNodesAndTrim(sample, this.methodName[method]);
        	                    break;
						case 1: inferred = inf.predefinedTemplateBaseline(testSample.Item2, this.envList [envAndObjective.Item1 - 1]); //Manually Defined Templates
    	                        break;
                        case 2: inferred = inf.treeExploration(envAndObjective); //Search Method
	                            break;
						case 3: inferred = inf.chance (testSample.Item2, this.envList[envAndObjective.Item1 - 1]); // change
								break;
						case 4: this.lg.writeToFile (ublOutput[i].Item1); //log of the UBL algorithm
								inferred = ublOutput [i].Item2; //UBL [Kwiatowski's algorithm]
								break;
						case 5: //Main Model: Generated Templates Only
								List<object> param5 = new List<object> () { (object)false, (object)true, (object)false};	
								inferred = inf.acl2015 (testSample.Item2, this.envList [envAndObjective.Item1 - 1], this, (Dictionary<String, Double>)(param [5]), param5);
								inferred = Global.filter (inferred);
								NoiseRemoval.instSeqCleaning(inferred,this.envList [envAndObjective.Item1 - 1],this.sml);
                            	break;
						case 6: //Main Model: Generated Templates And Storing Them
								List<object> param6 = new List<object> () { (object)false, (object)true, (object)true};
								inferred = inf.acl2015 (testSample.Item2, this.envList [envAndObjective.Item1 - 1], this, (Dictionary<String, Double>)(param [6]), param6);
								inferred = Global.filter (inferred);
								NoiseRemoval.instSeqCleaning(inferred,this.envList [envAndObjective.Item1 - 1],this.sml);
								break;
						case 7: //Main Model: Only VEIL Templates
								List<object> param7 = new List<object> () { (object)true, (object)false, (object)false};
								inferred = inf.acl2015 (testSample.Item2, this.envList [envAndObjective.Item1 - 1], this, (Dictionary<String, Double>)(param [7]), param7);
								inferred = Global.filter (inferred);
								NoiseRemoval.instSeqCleaning(inferred,this.envList [envAndObjective.Item1 - 1],this.sml);
								break;
						case 8:	//Main Model: Only VEIL Templates and Generated Templates
								List<object> param8 = new List<object> () { (object)true, (object)true, (object)false};
								inferred = inf.acl2015 (testSample.Item2, this.envList [envAndObjective.Item1 - 1], this, (Dictionary<String, Double>)(param [8]), param8);
								inferred = Global.filter (inferred);
								NoiseRemoval.instSeqCleaning (inferred, this.envList [envAndObjective.Item1 - 1], this.sml);
								break;
						case 9:	//Main Model: Only VEIL Templates, Generated Templates and Storing them
								List<object> param9 = new List<object> () { (object)true, (object)true, (object)true};
								inferred = inf.acl2015 (testSample.Item2, this.envList [envAndObjective.Item1 - 1], this, (Dictionary<String, Double>)(param [9]), param9);
								inferred = Global.filter (inferred);
								NoiseRemoval.instSeqCleaning (inferred, this.envList [envAndObjective.Item1 - 1], this.sml);
								break;
						default: break;
                    }

					//inferred = Inference.makeValidInstruction (this.envList [envAndObjective.Item1 - 1], inferred); //convert pseudo-instruction into valid instruction

                    #region fancyOutput
                    this.lg.writeToFile("<div> Final Output Cost <br/><br/><table><tr><th>Ground Truth</th><th>Inferred Sequence</th></tr>");
					int count = Math.Max(inferred.Count(),groundtruth.Count());

                    for (int k = 0; k < count; k++)
                    {
                        this.lg.writeToFile("<tr><td>");
                        if (k < groundtruth.Count())
                            groundtruth[k].display(this.lg);
                        this.lg.writeToFile("</td><td>");
                        if (k < inferred.Count())
                            inferred[k].display(this.lg);
                        this.lg.writeToFile("</td></tr>");
                    }
                    this.lg.writeToFile("</table></div><br/><br/>");
                    #endregion

					//Compute Metric
					List<Instruction> instRes = Global.filter (inferred); //inferred;
					double scoreLV = (double)this.mtr.levenshtein (instRes, groundtruth) / ((double)Math.Max (groundtruth.Count, instRes.Count) + Constants.epsilon);
					Tuple<Double, String> uWEED = this.mtr.unweightedEED (this.envList [envAndObjective.Item1 - 1], groundtruth, instRes); //Item2 is ground truth and has to be first
					double scoreWEED = this.mtr.weightedEED (this.envList [envAndObjective.Item1 - 1], groundtruth, instRes); //Item2 is ground truth and has to be first
					Tuple<Double,String> end = this.mtr.endStateMatch (this.envList [envAndObjective.Item1 - 1], instRes, groundtruth);

                    //Normalize the metric to 100 such that larger scores are better
                    scores[method][0][i] = (1 - scoreLV) * 100;
                    scores[method][1][i] = (1 - uWEED.Item1) * 100;
                    scores[method][2][i] = (1 - scoreWEED) * 100;
					scores[method][3][i] = end.Item1 * 100;

					cumulIED = cumulIED + scores [method] [0] [i];
					cumulEED = cumulEED + scores [method] [1] [i];
					cumulativeIED.Add (cumulIED);
					cumulativeEED.Add (cumulEED);

					this.lg.writeToFile ("<span style='color:green'> Using Method = " + this.methods.ElementAt (method).Key + "<br/>" +
										 ", LV Score : = " + scores [method] [0] [i] + "<br/>" +
										 " uWEED Score := " + scores [method] [1] [i] + /*" Log ( " + uWEED.Item2 + */"<br/>" +
									     " WEED Score := " + scores [method] [2] [i] + "<br/>" +
										 " END-ENV Score := " + scores [method] [3] [i] + "(" + end.Item2 + ")</span><br/><br/>");
					//double cacheHitPercent = (this.symp.cacheHit * 100) / (double)(this.symp.cacheHit + this.symp.cacheMiss + Constants.epsilon); //cache hit of interpolation
                }
                this.lg.writeToFile("</div>");
            }

			#region fancy_presentation
            String data = "<table><tr><td> Method Name </td> <td> Average </td> <td>Variance </td> </tr>";
            for (int i = 0; i < methods.Count(); i++)
            {
				if (!this.methods.ElementAt(i).Value)
					continue;
				for(int mtr=0; mtr < Metrics.numMetrics;mtr++)
				{
					if (mtr == 0)
						data = data + "<tr><td>" + this.methods.ElementAt(i).Key + "</td><td>" + scores [i] [0].Average () + "</td><td>" + Global.variance (scores [i] [0]) + "</td></tr>";
				    else data = data + "<tr><td> </td><td>" + scores [i] [mtr].Average () + "</td><td>" + Global.variance (scores [i] [mtr]) + "</td></tr>" ;
				}
            }
            data = data + "</table>";
            this.lg.writeToFile(data);
			#endregion 

            return scores;
        }

        static void planitInput(List<Instruction> output)
        {
        	/*input: instruction set
        	useful vars: "input" is a dict with start and stop strings from instructions
			  output: json file containing dict for planit input
        	*/
        	Dictionary<string,List<string>> input = new Dictionary<string,List<string>>();
        	List<string> start = new List<string>();
        	List<string> stop = new List<string>();
        	string past=null;
        	start.Add("PR2");
        	foreach (Instruction instr in output)
        	{
        		if (instr.getControllerFunction()== "moveto")
        		{
        			List<string> arg = instr.getArguments();
        			//Console.WriteLine(arg[0]);
        			if (arg.Count()==1)
        			{
        				stop.Add(arg[0]);
    					if (past!=null)
    						start.Add(past);
    					past = arg[0];
        			}
        		}
        	}

        	if (output.Count()==0)
        		stop.Add("PR2");

        	input.Add("start_configs",start);
        	input.Add("end_configs",stop);
        	
        	//to print the input to planit in json format
			System.IO.StreamWriter inputFile = new System.IO.StreamWriter (Constants.rootPath + "../dict.json");
			bool tmp_comma=false, tmp_loop_comma=false;
			inputFile.WriteLine("{");
			
			inputFile.Write("\"originalInstructions\": [");
			foreach (Instruction inst in output)
			{
				if (!tmp_comma)
				{
					inputFile.Write("\""+inst.getName()+"\"");
					tmp_comma = true;
				}	
				else
					inputFile.Write(",\""+inst.getName()+"\"");
			}
			inputFile.WriteLine("],");

			tmp_comma = false;
			foreach (KeyValuePair<string,List<string>>  kvp in input)
			{
				if (!tmp_comma)
					inputFile.Write("\""+kvp.Key+"\": [");
				else
					inputFile.Write(",\""+kvp.Key+"\": [");
				tmp_loop_comma = false;
				foreach (string ch in kvp.Value)
				{
					if (!tmp_loop_comma)
						inputFile.Write("\""+ch+"\"");
					else
						inputFile.Write(",\""+ch+"\"");
					tmp_loop_comma = true;
				}
				inputFile.WriteLine("]");
				tmp_comma = true;
			}
			inputFile.WriteLine("}");
			inputFile.Flush();
			inputFile.Close();

			System.IO.StreamWriter inputFile_feedback = new System.IO.StreamWriter (Constants.rootPath + "../tmd_feedback.txt");
			foreach (Instruction inst in output)
			{
				inputFile_feedback.Write(inst.getControllerFunction()+',');
				int k=1;
				foreach (String s in inst.getArguments())
                {
                	inputFile_feedback.Write(s+',');
                	k++;
                }
                while(k<3)
                {
                	inputFile_feedback.Write(',');
                	k++;
                }
                inputFile_feedback.Write('\n');
			}
			inputFile_feedback.Flush();
			inputFile_feedback.Close();
		}

        static void Main(string[] args)
        {
            /* Function Description: Main function which does the duty of running the algorithm on a given dataset.
             * It reads and parses the dataset as well as organizes it into train, validation and test dataset.
             * It then performs learning and runs the inference on the test dataset. The output is displayed in
             * an interactive html manner. There are three modes in which this code can be run - 
             * 
             * 0. Online Learning: Given an online sequence of datapoints, perform inference 
             * 1. Transfer Learning: Given a dataset and bootstrap data, perform inference on this dataset with different actions
             * 2. Offline Standard: Given a dataset, perform cross-validation and learning and inference */

            var sw = Stopwatch.StartNew();

            Tester testObj = new Tester();
            //Step 1: Create datastructure needed by Inference and Learning
            #region pre_processing_noise_removal_analysis
			testObj.loadAllEnv();                   //load the starting environments
			Console.WriteLine("path: "+ Constants.path);
			if(Constants.opmode == OpMode.Online)
			{
				Constants.cacheReadParser=false;  
				List<double[]> pivot = testObj.bootstrapWeights(); //bootstrap weight
				List<object> param = testObj.inf.initDict (pivot); 

				testObj.bootstrapLexicon();      //bootstrap lexicon
				testObj.ftr.bootstrapFeatures(); //bootstrap features

				/*Object vase = testObj.envList[5].findObject ("Vase_1");
				foreach(String affordance in vase.affordances_)
					Console.WriteLine("affordance: "+affordance);*/

				//Read text and envIndex from the sqlite database
				//while(true)
				{
					//String text = "Turn on xbox. Take Far Cry Game CD and put in xbox by pressig eject to open drive. Throw out beer, coke, and sketchy stuff in bowl. Take pillows from shelf and distribute among couches."; //Populate it somehow, by user input
					String text = "Move to PR2";
					int envIndex = 1; //Populate it somehow, by user input
					if (args.Length==1)
						text = args[0];
					else if(args.Length==2)
					{
						text = args[0];
						envIndex = Convert.ToInt16(args[1]);
					}
					//Console.WriteLine("env: "+envIndex+"\ntext:"+text);	
					List<Instruction> output = testObj.onlineInference (text, envIndex, param);

					Console.WriteLine("---------\nenv: "+envIndex+"\ntext: "+text+"\nInstruction ");
					foreach (Instruction inst in output)
						Console.WriteLine (inst.getName()+"\n");
					Console.WriteLine("----------------");
					planitInput(output);
				}
				testObj.destroyer(); //destroy data-structure
				return;
			}

            testObj.prs.parseLabelledData(testObj.lg, testObj.envList);         //Parses all the data
			//testObj.obj.parseUnsupervisedData(testObj.lg, testObj.envList);   //Gets unsupervised data

			NoiseRemoval.cleanData(testObj);        //Clean the data
			//NoiseRemoval.readNoiseFreeDataFromFile(testObj.prs, testObj.lg);

			List<Tuple<List<int>, List<int>>> datas = testObj.crossValidator(); // [[train, test], [train,test]... ]
			NoiseRemoval.readNoiseFreeTestDataFromFile(datas.Last().Item2, testObj.prs, testObj.lg);
			//NoiseRemoval.store(testObj.prs, true);

			testObj.loadIntermediateEnv();          //Load intermediate environments
			testObj.prs.storeAll(testObj.lg);       //Store the parsed data
            testObj.datany.analyze();               //Analyze the dataset and output the analysis
			List<List<List<double>>> evaluation = testObj.initEvaluation(); // Method [ Metric [ Scores ] ]
			testObj.writeTime();
            #endregion
            
			/* Step 2: Learning and Inference */
			if (Constants.opmode == OpMode.Transfer) 
			{
				/* In our Transfer Learning setting, we have the following scenario
				 * a dataset with possibly new actions is given as input, the algorithm
				 * uses the lexicon, weights, feature tables (environment based) using
				 * another dataset for bootstraping a learned model and then performs inference using it */

				//Since learning is not happening on the new dataset, it makes no sense to perform cross-validation
				List<int> test = Enumerable.Range(Math.Max(0,testObj.prs.returnAllData().Count()-10), Math.Min(10,testObj.prs.returnAllData().Count())).ToList();

				Console.WriteLine ("Going to transfer weights and lexicon");
				//load parameters for transfer learning
				testObj.bootstrapLexicon (); 							//Loads Lexicon from file
				List<double[]> pivot = testObj.bootstrapWeights();    	//Load weights from file
																		//Load features from file
				List<object> param = testObj.inf.initDict (pivot);

				Console.WriteLine ("Performing Inference");
				List<List<double[]>> score = testObj.inference(testObj.methods.Values.ToList(), test, param); //Do inference on the test data and get results [Method [Metric [numbers] ] ]

				//Analyze the results
				for (int mth = 0; mth < testObj.methods.Count(); mth++)
				{
					for (int mtr = 0; mtr < Metrics.numMetrics; mtr++)
						evaluation[mth][mtr] = evaluation[mth][mtr].Concat(score[mth][mtr].ToList()).ToList();
				}

				testObj.destroyer(); //destroy data-structure
			}
			else if(Constants.opmode == OpMode.Offline)
			{
	            /* Step 2: Cross-Validation Folds for offline computation */
				for (int fold= datas.Count()-1; fold < datas.Count(); fold++)   //iterating over datas: [ [train, test], [train, test] ..... ]
	            {
					Console.WriteLine ("Beginning with Experiment "+fold);
	                List<int> train = datas[fold].Item1, test = datas[fold].Item2;

					//Global.store (testObj.obj, test);
					testObj.lg.writeToFile ("<h2>Begining With Experiment Number " + fold + "</h2> Size of Training Data " + train.Count () + " and Test Data " + test.Count () + "<br/>");

	                /* Sub-Step 2.1: Learning */
					List<double[]> pivot = testObj.lrn.analyticGradientDescent(train, 0,fold);    //Apply learning algorithms to train weights
					//List<double[]> pivot = testObj.bootstrapWeights();
					List<object> param = testObj.inf.initDict (pivot);

					/* Sub-Step 2.2: Build Structures Required For Inference */
					Console.WriteLine ("Building Structure: Training Data "+train.Count());
					testObj.induceLexiconFromTraining(train);                                      //build the VEIL datastructure
					testObj.displayStructure();                                    			   //display the data
					testObj.ftr.constructFeatureDataStructures(train, testObj.methods.Values.ToList(), testObj.lexicon,
					                                           testObj.envList, testObj.listOfAllEnv, testObj.inf.sensim);  //construct data structures required for computing features
					//testObj.obj.storeVEILTemplates(testObj.veil, testObj.lg);

					/* Sub-Step 2.4: Pumping New Templates */
					/*Console.WriteLine ("Pumping New Templates");
					testObj.createNewTemplatesFromUnsupervisedData (testObj.methods.Values.ToList (), param);
					testObj.displayStructure();                                    			   //display the data
					testObj.ftr.destroyer ();
					testObj.ftr.constructFeatureDataStructures(train, testObj.methods.Values.ToList(),
					                                           testObj.veil, testObj.envList);  //construct data structures required for computing features
				    */

	                /* Sub-Step 2.5: Inference */
					Console.WriteLine ("Performing Inference");
	                List<List<double[]>> score = testObj.inference(testObj.methods.Values.ToList(), test, param); //Do inference on the test data and get results [Method [Metric [numbers] ] ]

	                /* Sub-Step 2.6: Analyze the results */
	                for (int mth = 0; mth < testObj.methods.Count(); mth++)
	                {
	                    for (int mtr = 0; mtr < Metrics.numMetrics; mtr++)
	                        evaluation[mth][mtr] = evaluation[mth][mtr].Concat(score[mth][mtr].ToList()).ToList();
	                }

					testObj.storeLexicon ();
	                testObj.destroyer(); //destroy data-structure
					testObj.symp.storeCache ();
	            }
			}

			//Step 3: Analyze the results
            testObj.inf.close();
			double cacheHitPercent = (testObj.symp.cacheHit * 100) / (double)(testObj.symp.cacheHit + testObj.symp.cacheMiss + Constants.epsilon); //cache hit of interpolation
			double atomicHitPercent = (testObj.symp.atomicCaseHit * 100) / (double)(testObj.symp.atomicCaseHit + testObj.symp.atomicCaseMiss + Constants.epsilon); //cache hit of interpolation
            StringBuilder toWrite = new StringBuilder("End of Overall Experiment : <br/><table><tr><td>Method Name</td><td>Average</td><td>Variance</td></tr>");
            for (int mth = 0; mth < testObj.methods.Count(); mth++)
            {
				if (testObj.methods.ElementAt (mth).Value) 
				{
					for (int mtr=0; mtr<Metrics.numMetrics; mtr++) 
					{
						Console.WriteLine ("Metric "+mtr+" "+evaluation [mth] [mtr].Average ());
						toWrite = toWrite.Append("<tr><td>" + testObj.methods.ElementAt (mth).Key + "</td><td>" + evaluation [mth] [mtr].Average () +
							"</td><td>" + Global.variance (evaluation [mth] [mtr].ToArray ()) + "</td></tr>");
					}
				}
            }
            toWrite = toWrite.Append("</table>");
            testObj.lg.setHighPriority();
            testObj.lg.writeToFile(toWrite.ToString());
            testObj.lg.setLowPriority();
		        
            //Step 4: Delete the datastructures and close the streams
            evaluation.ForEach(tmpMethod=>tmpMethod.ForEach (tmpMetric=>tmpMetric.Clear()));
            testObj.ss.Stop();
            testObj.writeTime();
			//testObj.symp.storeCache ();
            testObj.lg.close(); //close the log stream

			Console.WriteLine ("Cache Hit Percent  "+cacheHitPercent);
			Console.WriteLine ("Atomic Hit Percent "+atomicHitPercent);
			sw.Stop();
			Console.WriteLine("Time elapsed: {0} milliseconds", sw.ElapsedMilliseconds);		
			Console.WriteLine ("GoodBye");
        }
    }
}
