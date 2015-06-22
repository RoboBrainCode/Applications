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
using System.Xml;
using System.Threading.Tasks;
using WordsMatching;

namespace ProjectCompton
{
    class Object
    {
        /*Class Description : Describes properties of a 3D object based on Object Version 1.1.
          Each Object has a uniqueName which is derived from baseName ex: Mug1 is the first instance 
          of type Mug. It has centroid, boundingbox, temperature, rotation angles. Also every object
          has its own set of stateName and stateValues. Each object further has its stateAffordance
          like graspable, pourable etc. 
           
         * Future Revision : 1.2
         * - plan to add link to 3D collada file
         * - plan to add object tree hierachy. Where each object can have parent and 
         *   children.
         */

        private double centroidX, centroidY, centroidZ;
        private double boundingX, boundingY, boundingZ;
        private string temperature = "LOW";
        private double alpha, beta, gamma;//pose
		public string uniqueName = "default";

        private List<String> affordances = new List<string>(); //List of all affordance of this object
        private List<Tuple<String, String>> state = new List<Tuple<String, String>>(); //an object can be in many states

        public string temperature_
        {
            set
            {
                temperature = value;
            }
            get
            {
                return temperature;
            }
        }

        public List<String> affordances_
        {
            get { return this.affordances; }
        }

        public List<Tuple<String, String>> state_
        {
            get { return state; }
        }

        public Object(String[] specification)
        {
            /*Constructor Description : Uses string specification to 
             * initialize the data
             * Object      |   Location X    | Location Y  | Location Z |  Height | Width | Depth |  Pose alpha | Pose beta | Pose gamma */
            this.uniqueName = specification[0].Trim();
            this.centroidX = Double.Parse(specification[1]);
            this.centroidY = Double.Parse(specification[2]);
            this.centroidZ = Double.Parse(specification[3]);
            this.boundingX = Double.Parse(specification[4]);
            this.boundingY = Double.Parse(specification[5]);
            this.boundingZ = Double.Parse(specification[6]);
            this.alpha = Double.Parse(specification[7]);
            this.beta = Double.Parse(specification[8]);
            this.gamma = Double.Parse(specification[9]);
        }

        public Object()
        {
            /*Constructor Description : Only when copyObject is immediately called after creation*/
        }
        
        public Object(String error)
        {
            /*Constructor Description : Created when an object could not be found*/
            this.uniqueName = error;
        }

        public void bootStrap(String uniqueName)
        {
            /* Function Description : Given an object name. This function bootstraps the states
               from the XML file */
            this.uniqueName = Global.firstCharToUpper(uniqueName);
            String baseName = Global.base_(uniqueName);

            XmlTextReader reader = new XmlTextReader(Constants.dataFolder+@"/Objects.xml");
            bool objectFound = false;
            String stateName = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("name"))
                        {
                            reader.Read();
                            if(reader.Value.Equals(baseName))
                                objectFound = true;
                        }
                        if (objectFound && reader.Name.Equals("stateName"))
                        {
                            reader.Read();
                            stateName = reader.Value;
                        }
                        if (objectFound && reader.Name.Equals("default"))
                        {
                            reader.Read();
                            if(!this.ifStateExist(stateName))
                                state.Add(new Tuple<string, string>(stateName.ToString(), reader.Value)); //store the (stateName, defaultValue)
                        }
                        if (objectFound && reader.Name.Equals("affordance"))
                        {
                            reader.Read();
                            affordances.Add(reader.Value);
                        }
                        break;

                    case XmlNodeType.EndElement: //The node is an end element
                        if(reader.Name.Equals("object"))
                            objectFound = false;
                        break;
                }
            }
        }

        public double centroidX_
        {
            get
            {
                return centroidX;
            }
        }

        public double centroidY_
        {
            get
            {
                return centroidY;
            }
        }

		public double centroidZ_
		{
			get
			{
				return centroidZ;
			}
		}

		public double getL2Distance(double x, double y, double z)
		{
			/*Function Description: Get L2 Distance of this object from the given point*/
			return Math.Sqrt ((this.centroidX-x)*(this.centroidX-x)+(this.centroidY-y)*(this.centroidY-y)+(this.centroidZ-z)*(this.centroidZ-z));
		}

		public double getL2PlanarDistance(double x, double y)
		{
			/*Function Description: Get L2 Distance of this object from the given point*/
			return Math.Sqrt ((this.centroidX-x)*(this.centroidX-x)+(this.centroidY-y)*(this.centroidY-y));
		}

        public bool isDummy()
        {
            /*Function Description : Returns True if object is dummy*/
            if (this.uniqueName.StartsWith("$"))
                return true;
            else return false;
        }

        public void display(Logger lg)
        {
            /* Function Description : displays the states of the object*/
            if (this.state.Count() != 0)
            {
                lg.writeToFile("<div id='object'><i> " + this.uniqueName + "</i><br/> States<br/>");
                foreach (Tuple<String, String> t in this.state)
                {
                    lg.writeToFile(t.Item1 + " : " + t.Item2 + "<br/>");
                }
                lg.writeToFile("</div>");
            }
        }

        public void changePosition(double x,double y,double z)
        {
            //Function Description: Adds or changes the space position
            this.centroidX = x; this.centroidY = y; this.centroidZ = z;
        }

        public void changeRotation(double alpha, double beta, double gamma)
        {
            //Function Descriptions: Adds or changes the rotation angles
            this.alpha = alpha; this.beta = beta; this.gamma = gamma;
        }

        public void copyObject(Object copy)
        {
            /*Function Description : Create a copy of this object*/
            copy.centroidX = this.centroidX;
            copy.centroidY = this.centroidY;
            copy.centroidZ = this.centroidZ;
            copy.boundingX = this.boundingX;
            copy.boundingY = this.boundingY;
            copy.boundingZ = this.boundingZ;
            copy.alpha = this.alpha;
            copy.beta = this.beta;
            copy.gamma = this.gamma;
            copy.uniqueName = this.uniqueName.ToString();

            foreach (String s in this.affordances)
            {
                copy.affordances.Add(s.ToString());
            }

            foreach (Tuple<String, String> tmp in state)
            {
                copy.state.Add(new Tuple<String, String>(tmp.Item1.ToString(), tmp.Item2.ToString()));
            }
        }

        public Tuple<int, int> returnObjectGroundLoc()
        {
            return new Tuple<int, int>((int)centroidX,(int)centroidY);
        }

        public void addState(String property, String value)
        {
            /*Function Description : Adds a state*/
            //state should already exist during bootstrap phase
            if (this.ifStateExist(property))
            {
                this.deleteState(property);
                this.state.Add(new Tuple<string, string>(property, value));
            }          
        }

        public List<Tuple<String, String>> getState()
        {
            /*Function Description : Returns the state list*/
            return this.state;
        }

        public String getName()
        {
            /*Function Description : Returns the unique name*/
            return this.uniqueName;
        }

        public Tuple<double,string> findDistance(List<Tuple<String, String>> st)
        {
            /*Function Description : Finds distance between this object's
             * state and the st. The distance function used is - 
             * matching-State/ union of type of state
             */

            String log = "{ ";
            //if one of the object has no state then return 0
            if (st.Count == 0 && this.state.Count == 0)
                return new Tuple<double,string>(0," Both Object have 0 states } ");

            if (st.Count == 0 || this.state.Count == 0)
                return new Tuple<double, string>(0.5, " Exactly 1 number of object with 0 states }");

            int commonType = 0, same = 0;
            foreach (Tuple<String, String> utmp in this.state)
            {
                foreach (Tuple<String, String> vtmp in st)
                {
                    if (utmp.Item1.Equals(vtmp.Item1, StringComparison.OrdinalIgnoreCase))
                    {
                        log = log + ", Comparing State - " + utmp.Item1;
                        commonType++;
                        if (utmp.Item2.Equals(vtmp.Item2, StringComparison.OrdinalIgnoreCase))
                        {
                            log = log + ": Matched Value : " + utmp.Item2;
                            same++;
                        }
                        else log = log + " : Mismatched ";
                    }
                }
            }
            log = log + "Number of Not-Common States : " + (this.state.Count() + st.Count() - 2 * commonType) + "} ";
            double d = (double)(same) / ((double)(this.state.Count + st.Count - commonType));
            return new Tuple<double,string>(1 - d,log);
        }

        public void deleteState(String type)
        {
            /*Function Description : Delete the state*/
            Tuple<String, String> toRemove=null;
            foreach (Tuple<String, String> tmp in this.state)
            {
                if (tmp.Item1.Equals(type))
                {
                    toRemove = tmp;
                    break;
                }
            }
            if(toRemove!=null)
                this.state.Remove(toRemove);
        }

        public void changeState(String type, String value)
        {
            //Function Description: Change the state of the object
            if (!this.ifStateExist(type)) //if the state does not exist then return //raise an exception in future
                return; 
            this.deleteState(type);
            this.state.Add(new Tuple<string, string>(type,value));
        }

        public String getStateValue(String type)
        {
            /*Function Description : Return the value of the given type*/
            foreach (Tuple<String, String> t in this.state)
            {
                if (t.Item1.Equals(type))
                    return t.Item2;
            }
            return "";
        }

        public bool ifStateExist(String stateName)
        {
            /* Function Description : Returns true if the 
             * state exists */
            foreach (Tuple<String, String> tmp in this.state)
            {
                if (tmp.Item1.Equals(stateName))
                    return true;
            }
            return false;
        }

        public bool checkStateAndVal(String state, String val)
        {
            /* Function Description : Checks if the given state
             * has the given value */
            foreach (Tuple<String, String> tmp in this.state)
            {
                if (tmp.Item1.Equals(state, StringComparison.OrdinalIgnoreCase) &&
                   tmp.Item2.Equals(val, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

		public void storeXML(Logger lg)
		{
			//Function Description: Stores information in this object, in a xml file
			lg.writeToParserData ("<Object name=\""+this.uniqueName+"\">");
			lg.writeToParserData("<Affordances>");
			foreach(String affordance in this.affordances)
				lg.writeToParserData ("<Affordance>" + affordance + "</Affordance>");
			lg.writeToParserData ("</Affordances><States>");
			foreach (Tuple<String,String> sts in this.state)
				lg.writeToParserData ("<State><statename>" + sts.Item1 + "</statename><statevalue>"
									   + sts.Item2 + "</statevalue></State>");
			lg.writeToParserData ("</States></Object>");
		}

    }
}
