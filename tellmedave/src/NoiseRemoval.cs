using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProjectCompton
{
    class NoiseRemoval
    {
        /* Class Description : Provides tool for removing and cleaning the dataset */

        private static void addMissingObjects()
        {
            /* Function Description: Certains objects like buttons on tv_remote are missing in some
             * environment files. This class adds objects to the */

            foreach (String e in Constants.scenarios)
            {
                for (int i = 1; i <= 10; i++)
                {
                    StringBuilder entireFile = new StringBuilder();
                    StringBuilder addNew = new StringBuilder();
                    XmlTextReader reader = new XmlTextReader(Constants.dataFolder+@"/Environment/" + e + @"/" + e + i.ToString() + ".xml");

                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element: // The node is an element.
                                String name = "";
                                if (reader.Name.Equals("object"))
                                {
                                    XmlTextReader xm_ = reader;
                                    String innerXML = reader.ReadInnerXml();
                                    int start = innerXML.IndexOf("<name>");
                                    innerXML = innerXML.Substring(start);
                                    int index = innerXML.IndexOf("</name>");
                                    name = innerXML.Substring(6, index - 6);
                                    entireFile.Append("<object>" + innerXML + "</object>\n");
                                    reader = xm_;
                                }

                                #region describe_added_objects

                                if (name.Equals("--write-the-name-", StringComparison.OrdinalIgnoreCase))
                                {
									;//define a new object
								}
                                #endregion

                                break;
                        }
                    }

                    reader.Close();

                    System.IO.StreamWriter sw = new System.IO.StreamWriter(Constants.rootPath + Constants.dataFolder+@"/Environment/" + e + @"/" + e + i.ToString() + ".xml");
                    sw.WriteLine("<environment>");
                    sw.WriteLine(entireFile.ToString());
                    sw.WriteLine(addNew.ToString());
                    sw.WriteLine("</environment>");
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        private static void deleteInstruction(Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> data, List<List<Tuple<int, int>>> alignment, int i, List<int> removedInstructions)
        {
            /* Fuction Descriptin: Delete the which instruction from data and returns the new alignmen*/
            //Remove Instructions
            removedInstructions.Sort();
            for (int j = 0; j < removedInstructions.Count(); j++)
                data.Item3.RemoveAt(removedInstructions[j] - j); //this assumes removedInstructions is sorted in ascending order

            /* Adjust the allignments
             * [i1 j1] [i2 j2] [i3 j3] [i4 j4] ...... [ik jk]
             *  - to remove {z1, z2, z3, .... zl}
             *  - iterator over zi 
             *     -   for-all [ir jr] while ir, jr < zi replace [ir-pad jr-pad]
             *     -   for ir <= zi <= jr we replace it by [ir jr-1] 
             *            - increment pad by 1 and remve zi.
             *  - add the remaining alignments
             * */

            List<Tuple<int, int>> alignmentNew = new List<Tuple<int, int>>();
            int pad = 0, which = 0;

            for (int k = 0; k < alignment[i].Count(); k++)
            {
                while (which < removedInstructions.Count() && alignment[i][k].Item1 <= removedInstructions[which]
                             && removedInstructions[which] <= alignment[i][k].Item2)
                    which++;
                alignmentNew.Add(new Tuple<int, int>(alignment[i][k].Item1 - pad, alignment[i][k].Item2 - which));
                pad = which;
            }

            alignment[i] = alignmentNew;
        }

		public static void instSeqCleaning(List<Instruction> insts, Environment env, Simulator sml)
		{
			/* Function Description: Cleans the instruction sequence inst using following 
             * rules - 
             *      1. invariant removal  - if E -> inst -> E and inst != wait then remove inst
             *      2. repetition removal - for window in [1 ... max]
             *                                   for pad in [1...|I|]; j=pad;
             *                                          while ( I[pad+1...pad+i] = I[j+1...j+i] & \forall k in [0..i] E[pad+k] = E[j+k]) j += pad;
             *                                          remove I[pad+i+1.....j+i+1] */                                         

			Environment iterator = env.makeCopy();
			List<Environment> envSequence = new List<Environment>() { env };

			#region remove_invariant_insructions
			//Find all indices of removed instructions
			List<int> removedInstructions = new List<int>(); //index of instructions that are removed
			for (int j = 0; j < insts.Count(); j++)
			{
				Instruction inst_ = insts[j];
				if (inst_.getControllerFunction().Equals("wait"))
					continue;
				Environment tmp = sml.execute(inst_, envSequence[envSequence.Count()-1], true,true);
				envSequence.Add(tmp);
				if (iterator.isSame(tmp).Item1)  //redundant
					removedInstructions.Add(j);
				iterator = tmp;
			}
			for(int j=0; j<removedInstructions.Count();j++)
				insts.RemoveAt(removedInstructions[j]-j);
			#endregion

			#region repetition_removal
			removedInstructions.Clear();
			for (int win = 1; win <= 2; win++)
			{
				for (int pad = 0; pad <= insts.Count() - win; pad++)
				{
					int j = pad + win;
					while (true)
					{
						if (j + win > insts.Count())
							break;
						bool condition = true;
						//Check if instruction sequence is syntactically same
						for (int it = 0; it < win; it++)
						{
							if (!insts[pad + it].compare(insts[j + it]))
							{
								condition = false;
								break;
							}
						}

						if (!condition)
							break;

						//Check if environments are also same
						/*for (int it = 0; it <= win; it++)
                        {
                            if (!envSequence[pad + it].isSame(envSequence[j + it]))
                            {
                                condition = false;
                                break;
                            }
                        }

                        if (!condition)
                            break;*/

						j = j + win;
					}

					//remove all elements from I[pad+win ...... j-1], sanity check with j=pad+win
					for (int k = pad + win; k < j; k++)
						removedInstructions.Add(k);
				}
			}

			removedInstructions = removedInstructions.Distinct().ToList();
			for(int j=0; j<removedInstructions.Count();j++)
				insts.RemoveAt(removedInstructions[j]-j);
			#endregion
		}

		private static void instSeqCleaning(Parser prs, Simulator sml, List<Environment> envList)
        {
            /* Function Description: Cleans the instruction sequence inst using following 
             * rules - 
             *      1. invariant removal  - if E -> inst -> E and inst != wait then remove inst
             *      2. repetition removal - for window in [1 ... max]
             *                                   for pad in [1...|I|]; j=pad;
             *                                          while ( I[pad+1...pad+i] = I[j+1...j+i] & \forall k in [0..i] E[pad+k] = E[j+k]) j += pad;
             *                                          remove I[pad+i+1.....j+i+1] */                                         

            List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData();
            List<List<Tuple<int, int>>> alignment = prs.returnAllAlignment();

            if (alignment.Count() != parsedSentence.Count())
                throw new ApplicationException("Error: Not all parsed Sentences are aligned");

			System.IO.StreamWriter swr = new System.IO.StreamWriter (Constants.rootPath+"stream.txt");

            for (int i = 0; i < parsedSentence.Count(); i++)
            {
                List<Instruction> inst = parsedSentence[i].Item3;
                Environment env = envList[parsedSentence[i].Item1.Item1 - 1], iterator = env.makeCopy();
                List<Environment> envSequence = new List<Environment>() { env };

                #region remove_invariant_insructions
                //Find all indices of removed instructions
                List<int> removedInstructions = new List<int>(); //index of instructions that are removed
                for (int j = 0; j < inst.Count(); j++)
                {
                    Instruction inst_ = inst[j];
                    if (inst_.getControllerFunction().Equals("wait"))
                        continue;
					Environment tmp = sml.execute(inst_, envSequence[envSequence.Count()-1], true,true);
					iterator = envSequence[envSequence.Count()-1];
                    envSequence.Add(tmp);
                    if (iterator.isSame(tmp).Item1)  //redundant
					{
                        removedInstructions.Add(j);
					}
                    iterator = tmp;
                }
                NoiseRemoval.deleteInstruction(parsedSentence[i],alignment,i,removedInstructions);
                #endregion

                #region repetition_removal
                removedInstructions.Clear();
                for (int win = 1; win <= 5; win++)
                {
                    for (int pad = 0; pad <= inst.Count() - win; pad++)
                    {
                        int j = pad + win;
                        while (true)
                        {
                            if (j + win > inst.Count())
                                break;
                            bool condition = true;
                            //Check if instruction sequence is syntactically same
                            for (int it = 0; it < win; it++)
                            {
                                if (!inst[pad + it].compare(inst[j + it]))
                                {
                                    condition = false;
                                    break;
                                }
                            }

                            if (!condition)
                                break;

                            //Check if environments are also same
                            /*for (int it = 0; it <= win; it++)
                            {
                                if (!envSequence[pad + it].isSame(envSequence[j + it]))
                                {
                                    condition = false;
                                    break;
                                }
                            }

                            if (!condition)
                                break;*/

                            j = j + win;
                        }

                        //remove all elements from I[pad+win ...... j-1], sanity check with j=pad+win
                        for (int k = pad + win; k < j; k++)
                            removedInstructions.Add(k);
                    }
                }

                removedInstructions = removedInstructions.Distinct().ToList();
                NoiseRemoval.deleteInstruction(parsedSentence[i], alignment, i, removedInstructions);
                #endregion

				#region special_sequence
				//remove release x; moveto x; grasp x; sequence
				removedInstructions.Clear();
				for (int j = 0; j < inst.Count()-2; j = j + 3)
				{
					if(inst[j].getControllerFunction().Equals("release") && inst[j+1].getControllerFunction().Equals("moveto")
					   && inst[j+2].getControllerFunction().Equals("grasp") && inst[j].getArguments()[0].Equals(inst[j+1].getArguments()[0])
					   && inst[j].getArguments()[0].Equals(inst[j+2].getArguments()[0]))
					{
						removedInstructions.Add(j);
						removedInstructions.Add(j+1);
						removedInstructions.Add(j+2);
					}
				}
				NoiseRemoval.deleteInstruction(parsedSentence[i],alignment,i,removedInstructions);
				#endregion
            }
			swr.Flush ();
			swr.Close ();
        }

        private static void removeDeadObjects(List<Environment> envList)
        {
            /* Function Description: Lots of error and complextiy of some function 
             * depends upon number of objects in environments. This noise removal function
             * removes dead objects like - KitchenCeiling, MicrowaveFloor etc.
             * Where are these objects coming from? They are coming from coppercube simulator
             * which needs to create some non-interative objects.
             *  At this point I am simply hardcoding the objects for VEIL-1000. In future,
             * I want to remove those objects which can never occur in argument.
			 */

			List<String> deadObjects = new List<string> () {"FridgeSeparator", "FridgeWater", "FridgeCeiling", "FridgeFloor",
				"FridgeLeft", "FridgeRight", "MicrowaveBack", "MicrowaveCeiling", "MicrowaveWall", "MicrowaveFloor", "Camera1","KitchenCeiling", "StoveTop",
				"flower1", "Kitchen", "skybox", "livingRoom", "livingRoomCeiling", "camera_1"};

            foreach (Environment env in envList)
            {
                foreach (String objname in deadObjects)
					env.removeObjects(objname); //Remove objects from the list
            }
        }

		private static void removeTrivia(Parser prs)
		{
			/* Function Description: Removes trivial errors like datapoints with
			 * no instruction sequence. This follows from the argument that tasks
			 * are never constructed to be trivial.*/

			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData();
			List<List<Tuple<int, int>>> alignment = prs.returnAllAlignment ();
			List<int> toRemove = new List<int> ();

			for(int i=0; i<parsedSentence.Count();i++) 
			{
				if (parsedSentence [i].Item3.Count () == 0)
					toRemove.Add (i);
				else if (parsedSentence [i].Item2 == null) 
				{
					Console.WriteLine ("Null clauses ? Point "+i);
					toRemove.Add (i);
				}
			}

			for (int j=0; j<toRemove.Count(); j++) 
			{
				parsedSentence.RemoveAt (toRemove [j] - j);
				alignment.RemoveAt (toRemove [j] - j);
				prs.rawData.RemoveAt(toRemove [j] - j);
				//env.RemoveAt (toRemove [j] - j);
			}
		}

		public static void checkConsistency(Parser prs, List<Environment> envList,Tester tester)
		{
			/* Function Description: Some Instruction are Not Executeable so the algorithm finds instructions
			 * to make it executable. */

			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData ();
			List<List<Tuple<int,int>>> alignment = prs.returnAllAlignment ();
			Simulator sml = tester.sml;
			SymbolicPlanner symp = tester.symp;

			for (int i=0; i<parsedSentence.Count(); i++) 
			{
				Environment iterator = envList [parsedSentence [i].Item1.Item1 - 1].makeCopy ();
				List<List<Instruction>> missing = new List<List<Instruction>> ();
				List<int> missingIndices = new List<int> ();

				Tuple<int,int> previous = null;
				if (i==110) 
				{
					previous = new Tuple<int,int> (alignment[i][0].Item1, alignment[i][0].Item2);
					Constants.allow = true;
				}

				List<Instruction> newInstruction = new List<Instruction> ();
				bool changed = false;

				for (int j=0; j<parsedSentence[i].Item3.Count(); j++) 
				{
					Instruction inst = parsedSentence [i].Item3[j];
					Tuple<Double, String, String> result = sml.satSyntConstraints(inst, iterator);
					if (result.Item3 == null) 
					{
						throw new ApplicationException ("Syntactically Impossible Statements Occuring "+inst.getName());
					}

					if (result.Item3.Length != 0)  //unexectuable instructions
					{
						String constraint = String.Join("^",result.Item3.Split(new char[]{'^'}).Select(acc=> "("+acc+")"));
						bool edgeCase = false;
						if (inst.getControllerFunction ().Equals ("grasp")) 
						{
							String word = inst.getArguments () [0];
							if (constraint.Equals ("(Near Robot " + word + ")^(not (Grasping Robot " + word + "))")) 
							{
								edgeCase = true;
							}
						}

						if(!edgeCase)
						{
							List<Instruction> missingInstruction = symp.satisfyConstraints (iterator, constraint);
							if (missingInstruction != null) 
							{
								missing.Add (missingInstruction);
								missingIndices.Add (j);
								newInstruction = newInstruction.Concat (missingInstruction).ToList ();
								changed = true;
							}
						}
					} 
					iterator = sml.execute (inst, iterator, true, true); //forcefully execute the instruction anyway
					newInstruction.Add (inst);
				}

				List<String> names = newInstruction.Select (x => x.getName ()).ToList ();


				if (changed) 
					parsedSentence [i] = new Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>> (parsedSentence [i].Item1, parsedSentence [i].Item2
				                                                                                         , newInstruction, parsedSentence [i].Item4);

				//add the missing instructions and adjust the alignment
				int k = 0, pad = 0, missingiter=0;
				while (k<alignment[i].Count()) 
				{
					if (missingiter >= missingIndices.Count ()) //no more alignments left
					{
						alignment [i] [k] = new Tuple<int,int> (alignment [i] [k].Item1 + pad, alignment [i] [k].Item2 + pad);
						k++;
						continue;
					}

					int j = missingIndices [missingiter];//index at which the next set of missing instruction occur

					if (alignment [i] [k].Item2 < j) //missing instructions are in future
					{
						alignment [i] [k] = new Tuple<int,int> (alignment [i] [k].Item1 + pad, alignment [i] [k].Item2 + pad);
						k++;
						continue;
					}

					int sum = 0;
					while (alignment [i] [k].Item1 <= j && j <= alignment [i] [k].Item2) //alignments presently
					{
						sum = sum + missing [missingiter].Count ();
						missingiter++;
						if (missingiter >= missingIndices.Count ())
							break;
						j = missingIndices [missingiter];
					}

					alignment [i] [k] = new Tuple<int,int> (alignment [i] [k].Item1 + pad, alignment [i] [k].Item2 + pad + sum);
					k++;
					pad = sum + pad;
					//pad = pad + missing[missingiter].Count();
				}

				/*for (k=0; k<alignment[i].Count(); k++) 
				{
					if (missingiter >= missingIndices.Count ())
						break;
					int j = missingIndices [missingiter];//index at which the next set of missing instruction occur
					if (alignment [i] [k].Item1 <= j && j <= alignment [i] [k].Item2) 
					{
						alignment [i] [k] = new Tuple<int,int> (alignment [i] [k].Item1 + pad, alignment [i] [k].Item2 + pad + missing[missingiter].Count ());
						pad = pad + missing[missingiter].Count();
						missingiter++;
					}
					else if (alignment [i] [k].Item2 < j)
						alignment [i] [k] = new Tuple<int,int> (alignment [i] [k].Item1 + pad, alignment [i] [k].Item2 + pad);
				}

				while (k<alignment[i].Count()) 
				{
					alignment [i] [k] = new Tuple<int,int> (alignment [i] [k].Item1 + pad, alignment [i] [k].Item2 + pad);
					k++;
				}*/

				if (previous != null) 
				{
					Tuple<int,int> align = alignment[i][0];
				}
			}
		}

		public static void check(Parser prs)
		{
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData ();
			List<List<Tuple<int,int>>> alignment = prs.returnAllAlignment ();

			for (int i=0; i< parsedSentence.Count(); i++) 
			{
				/*Console.WriteLine ("Check "+parsedSentence[i].Item3.Count()+" vs "+env[i].Count());
				if(parsedSentence[i].Item3.Count() + 1!= env[i].Count())
					throw new ApplicationException("WOW");*/
				Console.WriteLine("Scenario "+i);
				foreach (Tuple<int,int> align in alignment[i]) 
				{
					Console.WriteLine (align.Item1+" to "+align.Item2+" out of "+parsedSentence[i].Item3.Count());
					if(align.Item2>=parsedSentence[i].Item3.Count() || align.Item1>=parsedSentence[i].Item3.Count())
						throw new ApplicationException("Why did alignments fail case:"+i);
				}
			}
		}

		private static void fixAlignment(Parser prs)
		{
			/*Function Description: Fix end-alignments */
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData ();
			List<List<Tuple<int,int>>> alignment = prs.returnAllAlignment ();

			for (int i=0; i< alignment.Count(); i++) 
			{
				for(int j=0; j<alignment[i].Count();j++) 
				{
					Tuple<int,int> align = alignment[i][j];
					List<Instruction> instsList = parsedSentence [i].Item3;
					if(align.Item1 >= instsList.Count () && align.Item2 == align.Item1-1) 
						alignment [i][j] = new Tuple<int, int> (instsList.Count()-1, instsList.Count()-2);
				}
			}
		}

		public static void simplifyInstructionSeq(Parser prs, List<Environment> envList, Simulator sml, SymbolicPlanner symp)
		{
			/* Function Description: We do the following, given an instruction
			 * sequence I, we compute the end state S' and then reconstruct I from 
			 * S' using the symbolic planner */

			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData ();
			List<List<Tuple<int,int>>> alignment = prs.returnAllAlignment ();
			System.IO.StreamWriter saveNewInst = new System.IO.StreamWriter (Constants.rootPath+"cleanedseq.xml");
			saveNewInst.WriteLine ("<root>");

			for(int i=0; i<parsedSentence.Count(); i++)
			{
				saveNewInst.WriteLine ("<point>");
				int padding = 0;
				Environment envIter = envList[parsedSentence[i].Item1.Item1-1];
				List<Instruction> instruction = parsedSentence [i].Item3;

				List<Instruction> newInstruction = new List<Instruction> ();
				List<Tuple<int,int>> newAlignment = new List<Tuple<int, int>> ();

				for (int j=0; j<parsedSentence[i].Item4.Count(); j++) 
				{
					Tuple<int,int> align = alignment[i][j];

					if (align.Item1 > align.Item2) //if there was an empty alignment then add empty alignment
					{
						newAlignment.Add (new Tuple<int,int>(padding, padding-1));
						saveNewInst.WriteLine ("<span class=\"instruction\">Change of Segment</span>");
						continue;
					}

					Environment end = sml.executeList (instruction.GetRange (align.Item1, align.Item2 - align.Item1 + 1), envIter);
					List<String> diff = end.difference(envIter);

					if (diff.Count () == 0)  //if difference is 0 then add empty alignment
					{
						newAlignment.Add (new Tuple<int,int>(padding, padding-1));
						saveNewInst.WriteLine ("<span class=\"instruction\">Change of Segment</span>");
						continue;
					}

					List<Instruction> newInstruction_ = symp.satisfyConstraints (envIter, string.Join("^",diff));

					if (newInstruction_.Count () == 0) //if generated instruction sequence is of length 0 then add empty alignment
					{
						newAlignment.Add (new Tuple<int,int>(padding, padding-1));
						saveNewInst.WriteLine ("<span class=\"instruction\">Change of Segment</span>");
						continue;
					}

					newAlignment.Add (new Tuple<int, int> (padding, padding + newInstruction_.Count () - 1));
					padding = padding + newInstruction_.Count ();
					newInstruction = newInstruction.Concat (newInstruction_).ToList ();
					envIter = sml.executeList (newInstruction_, envIter, true);//forcing only cause I suspect bugs in planner else output of planner is always valid
					foreach(Instruction inst in newInstruction_)
						saveNewInst.WriteLine ("<span class=\"instruction\">"+inst.getName()+"</span>");
					saveNewInst.WriteLine ("<span class=\"instruction\">Change of Segment</span>");
				}

				if (newAlignment.Count () != alignment [i].Count ())
					/*throw new ApplicationException*/Console.WriteLine("Alignments of different size"+newAlignment.Count ()+" and "+alignment [i].Count ());

				alignment [i] = newAlignment;
				parsedSentence [i].Item3.Clear ();
				foreach (Instruction inst in newInstruction) //update the new instruction sequence
					parsedSentence [i].Item3.Add (inst);
				saveNewInst.WriteLine ("</point>");
			}
			saveNewInst.WriteLine ("</root>");
			saveNewInst.Flush ();
			saveNewInst.Close ();
		}

		public static void readNoiseFreeDataFromFile(Parser prs, Logger lg)
		{
			/* Function Description: Noise Free Data stored in a file */

			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData ();
			List<List<Tuple<int,int>>> alignment = prs.returnAllAlignment ();

			int index = 0;
			//read the xml file
			XmlTextReader reader = new XmlTextReader(Constants.rootPath+"./noise_free_data.xml");
			int start = 0, end = 0;
			List<Tuple<int,int>> alignment_ = null;
			List<Instruction> insts_ = null;

			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
					if (reader.Name.Equals ("point")) 
					{
						alignment_ = new List<Tuple<int, int>> ();
						insts_ = new List<Instruction> ();
						start = 0;
						end = -1;
					}
					if (reader.Name.Equals ("action") && insts_!=null) 
					{
						reader.Read ();
						if (reader.Value.Equals ("Change-Of-Segment")) 
						{
							//there exist an alignment from [start to end]
							alignment_.Add (new Tuple<int, int> (start,end));
							start = end+1;
						}
						else
						{
							end++;
							Instruction inst_ = new Instruction ();
							inst_.parse (reader.Value, lg);
							insts_.Add (inst_);
						}
					}
					break;

					case XmlNodeType.EndElement:
					if(reader.Name.Equals("point"))
					{
						alignment [index] = alignment_.ToList ();
						parsedSentence [index].Item3.Clear ();
						foreach (Instruction inst_ in insts_)
							parsedSentence [index].Item3.Add (inst_.makeCopy ());
						alignment_ = null;
						insts_ = null;
						index++;
					}
					break;
				}
			}
		}


		public static void readNoiseFreeTestDataFromFile(List<int> test, Parser prs, Logger lg)
		{
			/* Function Description: Due to noise, we manually remove noise from the test 
			 * cases. We store them in a separate file. When we call this function, the
			 * data will be read instead from the file. */

			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData ();
			List<List<Tuple<int,int>>> alignment = prs.returnAllAlignment ();

			int index = 0;
			//read the xml file
			XmlTextReader reader = new XmlTextReader (Constants.dataFolder + @"/test_instruction.xml");
			int start = 0, end = 0;
			List<Tuple<int,int>> alignment_ = null;
			List<Instruction> insts_ = null;

			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					 case XmlNodeType.Element:
						if (reader.Name.Equals ("point")) 
						{
							alignment_ = new List<Tuple<int, int>> ();
							insts_ = new List<Instruction> ();
							start = 0;
							end = -1;
						}
						if (reader.Name.Equals ("instruction") && insts_!=null) 
						{
							reader.Read ();
							if (reader.Value.Equals ("Change-Of-Segment")) 
							{
							   //there exist an alignment from [start to end]
								alignment_.Add (new Tuple<int, int> (start,end));
								start = end+1;
							}
							else
							{
								end++;
								Instruction inst_ = new Instruction ();
								inst_.parse (reader.Value, lg);
								insts_.Add (inst_);
							}
						}
						break;
				
					case XmlNodeType.EndElement:
						if(reader.Name.Equals("point"))
					   	{
							alignment [test[index]] = alignment_.ToList ();
							parsedSentence [test [index]].Item3.Clear ();
							foreach (Instruction inst_ in insts_)
								parsedSentence [test [index]].Item3.Add (inst_.makeCopy ());
							Console.WriteLine ("Changing the test point with sentence " + parsedSentence [test [index]].Item2.getSubTreeSentence ());
							//Console.WriteLine ("\robobrain{} " + String.Join (", ", parsedSentence [test [index]].Item3.Select (x => x.getName ())));
							alignment_ = null;
							insts_ = null;
							index++;
							if (index == 12) 
								return;
						}
					break;
				}
			}
		}

		public static void print(Parser prs)
		{
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData ();

			for (int i=85; i<=150; i++) 
			{
				if (parsedSentence [i].Item2 != null && parsedSentence [i].Item2.getSubTreeSentence () != null && parsedSentence [i].Item2.getSubTreeSentence ().Contains("beer_1"))
				{
					Console.WriteLine ("Sentence " + i + " is " + parsedSentence [i].Item2.getSubTreeSentence ());
					Console.WriteLine ("Instruction " + i + " is " + String.Join (", ", parsedSentence [i].Item3.Select (x => x.getName ())));
				}
			}
		}

		public static void store(Parser prs, bool noiseFree)
		{
			//Function Description: Stores data in a file
			List<Tuple<Tuple<int, int>, Clause, List<Instruction>, List<Clause>>> parsedSentence = prs.returnAllData ();
			List<List<Tuple<int,int>>> alignment = prs.returnAllAlignment ();
			List<Tuple<String,String,int,int>> rawData = prs.rawData;
			String[] objective = new String[10] {" Making Ramen", "Making Affogato", "Boiling Water",
				"Clean the Kitchen", "Make Breakfast", "Prepare Room for Game Night",
				"Clean the Room", "Prepare Room for Movie Night", "Prepare Room for Study Night",
				"Prepare Room for Party"
			};

			if (rawData.Count () != alignment.Count () || alignment.Count () != parsedSentence.Count ())
				throw new ApplicationException ("data are not same length");

			System.IO.StreamWriter sw = null; 
			if(noiseFree)
				sw = new System.IO.StreamWriter (Constants.rootPath+"noise_free_data.xml");
			else sw = new System.IO.StreamWriter (Constants.rootPath+"unprocessed_data.xml");
			sw.WriteLine ("<root>");
			for (int i=0; i<parsedSentence.Count(); i++) 
			{
				sw.WriteLine ("<point>");
				String environment = "";
				int scenarioID = parsedSentence [i].Item1.Item1;
				if (scenarioID <= 10)
					environment = "kitchen_scenario_environment_number_" + scenarioID;
				else environment = "living_room_scenario_environment_number" + (20 - scenarioID);

				String sentence = rawData [i].Item2;
				int oldsen = sentence.IndexOf("<");
				if (oldsen != -1)
					sentence = sentence.Substring(0, oldsen);

				sw.WriteLine ("<text>"+sentence+"</text>");
				sw.WriteLine ("<environment>"+environment+"</environment>");
				sw.WriteLine ("<objective>"+objective[parsedSentence [i].Item1.Item2-1]+"</objective>");
				sw.WriteLine ("<action_sequence>");
				List<Instruction> insts = parsedSentence [i].Item3;
				int j=0;
				for(int k=0; k<alignment[i].Count(); k++)
				{
					Tuple<int,int> align = alignment[i][k];
					for(j=align.Item1; j<=align.Item2;j++)
						sw.WriteLine ("<action>"+insts[j].getName()+"</action>");
					if(k<alignment[i].Count()-1)
						sw.WriteLine ("<change_of_alignment/>");
				}

				Console.WriteLine ("j=  " + j + " vs " + insts.Count ());
				for (int index=j+1; index<insts.Count(); index++)
					sw.WriteLine ("<action>"+insts[index].getName()+"</action>");
				sw.WriteLine ("</action_sequence>");
				sw.WriteLine ("</point>");
			}
			sw.WriteLine ("</root>");
			sw.Flush();
			sw.Close ();
			Console.WriteLine ("Number of unique users " + rawData.Select (x => x.Item1).ToList().Distinct ().Count ());
		}


		public static void cleanData(Tester tester)
		{
			/* Function Description: Calls functions for cleaning the data */
			NoiseRemoval.store (tester.prs, false);
			NoiseRemoval.checkConsistency(tester.prs, tester.envList, tester);
			NoiseRemoval.instSeqCleaning (tester.prs, tester.sml, tester.envList);
			NoiseRemoval.removeDeadObjects (tester.envList);
			NoiseRemoval.removeTrivia(tester.prs);
			NoiseRemoval.fixAlignment (tester.prs);
			//NoiseRemoval.store (tester.obj, true);
			//NoiseRemoval.simplifyInstructionSeq (tester.obj, tester.envList, tester.sml, tester.symp);
			Console.WriteLine ("Done with Noise Removal Total Points :- "+tester.prs.returnAllData().Count());
		}
    }
}