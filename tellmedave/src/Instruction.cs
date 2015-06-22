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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ProjectCompton
{
    class Instruction
    {
        /*Class Description : Defines properties of an elementary instruction.*/

        private String controllerInstruction = null;
        private List<String> arguments = new List<string>();

        public Instruction()
        {
            //Constructor Definition: Creates an unitialized object
        }

        public Instruction(String controllerInstruction, List<String> description)
        {
            /* Constructor Description: Given controllerInstruction and description, 
             * intialize the variables */
            this.controllerInstruction = controllerInstruction;
            this.arguments = description;
        }

        public String getName()
        {
            /*Function Description : Returns name*/
            String name = this.controllerInstruction;
            foreach (String dscp in this.arguments)
                name = name + " " + dscp;
            return name;
        }

        public void setNameDescription(String ctrlFunc, List<String> descp)
        {
            this.controllerInstruction = ctrlFunc;
            this.arguments = descp;
        }

        public Instruction makeCopy()
        {
            /*Function Description : Makes a copy*/
            Instruction newCopy = new Instruction();
            newCopy.arguments = this.arguments.ToList();
            newCopy.controllerInstruction = this.controllerInstruction;
            return newCopy;
        }

        public String getControllerFunction()
        {
            /*Function Description : Returns the function name*/
            return this.controllerInstruction;
        }

        public double norm()
        {
            /*Function Description: Returns the norm or description length of this instruction*/ 
			return 1 + this.arguments.Count ();
        }

        public List<String> getArguments()
        {
            /*Function Description : Returns the function description*/
            return this.arguments;
        }

        public void parse(String instruction, Logger lg)
        {
            /*Function Descrption : Parses a string into
             * instruction based on specifications in 
             * ControllerInstructions.xml
             */
            String[] words = instruction.Split(new char[]{' '});
            int len = words.Length;
            if(len == 0)
            {
                lg.writeToErrFile("Null Instruction exist "+instruction);
                return;
            }

            //if (words[0].Equals("Time",StringComparison.OrdinalIgnoreCase) || words[0].Equals("Drop",StringComparison.OrdinalIgnoreCase) || words[0].Equals("Store",StringComparison.OrdinalIgnoreCase) || words[0].Equals("Respawn",StringComparison.OrdinalIgnoreCase))
            //    return; //meta-instructions. Ignoring for the time.
            
			bool matched = false, found = false;
			int numParam = 0;
			XmlTextReader reader = new XmlTextReader (Constants.dataFolder + @"/ControllerInstructions.xml");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("Name"))
                        {
                            reader.Read();
							if (reader.Value.Equals (words [0], StringComparison.OrdinalIgnoreCase))
								matched = true;
							else matched = false;
							numParam = 0;
						}
                        if (reader.Name.Equals("Parameter"))
                        {
                            reader.Read();
							numParam++;
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name.Equals("Instruction"))
                        {
                            if (matched && numParam==len-1) //parsing matched
                            {
                                this.controllerInstruction = words[0].ToLower();
                                for (int i = 1; i < len; i++)
									this.arguments.Add(Global.standardize(Global.firstCharToUpper(words[i]))); //object-name is always capitalized
								found = true;
                            } //else index=0 and next instruction pattern shall be tried
                        }
                        break;
                }
                if (found)
                    break;
            }
			reader.Close ();

			if (!found) 
			{
				throw new ApplicationException ("Don't know how to parse " + instruction);
			}
        }

        public void changeParam(int whichParam, String newParamVal)
        {
            /* Function Description : Replace the whichParam parameter by
             * newParamVal */
            this.arguments[whichParam] = newParamVal;
        }

        public bool compare(Instruction inst)
        {
            /*Function Description : Compares the inst instruction with this instruction.
             * Returns true if equal else false*/

            if (!this.controllerInstruction.Equals(inst.controllerInstruction, StringComparison.OrdinalIgnoreCase))
                return false;

            if (this.arguments.Count() != inst.arguments.Count())
                return false;

            for (int i = 0; i < this.arguments.Count();i++)
            {
                if (!this.arguments[i].Equals(inst.arguments[i], StringComparison.OrdinalIgnoreCase))
                {
                    if (this.arguments[i].StartsWith("stove_1Knob") && inst.arguments[i].StartsWith("stove_1Knob")
                      || this.arguments[i].StartsWith("stove_1Burner") && inst.arguments[i].StartsWith("stove_1Burner"))
                        continue;
                    return false;
                }
            }
            return true;
        }

        public List<String> returnObject()
        {
            // Function Description: Return those parameters which are objects
            
			//hack designed for current set of functions
			switch (this.arguments.Count ()) 
			{
				case 0:  return this.arguments;
				case 1:  return this.arguments;
				case 2:  return this.arguments;
				case 3:  return new List<String>(){this.arguments[0],this.arguments[2]};
				default: throw new ApplicationException ("Instruction with more than 3 arguments.");
			}
        }

        #region deprecated
        public List<String> possibleObject(String var, Environment env)
        {
            /* Function Description : Returns the set of objects from env that 
             * satisfy the constraints of the given object. If it returns null
             * if var is a not in description */

            return null;
        }
        #endregion

        public void display(Logger lg)
        {
            /*Function Description : Display the instruction*/
            String write = "<span style='color:red'>" + this.controllerInstruction + "</span>";
            foreach (String s in this.arguments)
                write = write + " " + s;
            lg.writeToFile(write+"<br/>");
        }

    }
}
