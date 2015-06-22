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
    /* Spatial Relatioship ENUM, describing different possible relationship between objects */
    public enum SpatialRelation
    {
        On,
        In,
        Below,
        Near,
        Far,
        FrontOf,
        LeftOf,
        RighOf,
        BackOf,
        Grasping,
        None
    }

    class Environment
    {
        // Class Description: Describes the environment

		public List<Object> objects{ get; private set;}
		public List<Tuple<int, int, String>> relativePosition{ get; private set;} //Relative Position of the objects i.e. Rel(x,y) holds [note not symmetric]
		public List<Tuple<Object, Object, SpatialRelation>> relationshipMatrix{ get; private set;} //RelationShip Matrix {(x,y,rel)} means x is in relationship rel with respect to y
                                                                                                                    // examples of relationship is - Inside Mug Fridge, Grasping Robot Cup, ....... 
        private String relFileName = null;

		public Environment()
		{
			//Constructor Description: Initializes basic constructors
			this.objects = new List<Object> ();
			this.relativePosition = new List<Tuple<int, int, string>> ();
			this.relationshipMatrix = new List<Tuple<Object, Object, SpatialRelation>> ();
		}

        public void display(Logger lg)
        {
            /*Function Description : display the states of the environment*/
            lg.writeToFile("<div id='environment'> <b>Displaying Environment</b> <br/>");
            foreach (Object ob in this.objects)
            {
                lg.writeToFile("<div> <button onclick='show(this)'>Object : " + ob.uniqueName + " </button> <div style='display:none;'>");
                ob.display(lg);
                lg.writeToFile("</div></div>");
            }

            lg.writeToFile("<div><button onclick='show(this)'>Object-Object Relations </button> <div style='display:none;'>");
            String data = "<table>";
            foreach (Tuple<int, int, String> tmp in this.relativePosition)
            {
                data = data + "<tr><td>" + objects[tmp.Item1].uniqueName + "</td><td><i>" + tmp.Item3 + "</i></td><td>" + objects[tmp.Item2].uniqueName + "</td></tr>";
            }
            data = data +"</table></div></div>";
            lg.writeToFile(data+"</div>");
        }

        public Environment makeCopy()
        {
            /*Function Description : copy the environment*/
			Environment env = new Environment ();
            env.relFileName = this.relFileName.ToString();
            foreach (Object ob in this.objects)
            {
                Object nw = new Object();
                ob.copyObject(nw);
                env.objects.Add(nw);
            }

            foreach (Tuple<int, int, string> tmp in this.relativePosition)
                env.relativePosition.Add(tmp);

            foreach(Tuple<Object,Object,SpatialRelation> sp in this.relationshipMatrix)
            {
                Object first = env.findObject(sp.Item1.uniqueName);
                Object second = env.findObject(sp.Item2.uniqueName);
                env.relationshipMatrix.Add(new Tuple<Object, Object, SpatialRelation>(first, second, sp.Item3));
            }

			return env;
        }

        public Tuple<Boolean,String> isSame(Environment env)
        {
            /* Function Description : Checks if the two environment are same.
			 * Retursn True/False along with log*/

            if (this.objects.Count() != env.objects.Count() || this.relationshipMatrix.Count() != env.relationshipMatrix.Count())
				return new Tuple<Boolean,String>(false,"Numbers not matching");

            for (int i = 0; i < this.objects.Count(); i++)
            {
                Object obj_ = env.findObject(this.objects[i].uniqueName);
                if (obj_.findDistance(this.objects[i].getState()).Item1 != 0)
					return new Tuple<Boolean,String>(false,"Object "+this.objects[i].uniqueName+" does not have same states");
            }

            for (int i = 0; i < this.relationshipMatrix.Count(); i++)
            {
				String name1 = this.relationshipMatrix [i].Item1.uniqueName;
				String name2= this.relationshipMatrix [i].Item2.uniqueName;
				SpatialRelation sr = this.relationshipMatrix [i].Item3;
				bool found = false;
				foreach (Tuple<Object,Object,SpatialRelation> entries in env.relationshipMatrix) 
				{
					if (entries.Item1.uniqueName.Equals (name1) && entries.Item2.uniqueName.Equals (name2) && entries.Item3 == sr) 
					{
						found = true; 
						break;
					}
				}
				if(!found)
					return new Tuple<Boolean,String>(false,"Relationship "+name1+" "+name2+" "+sr.ToString()+" not same");
            }

			return new Tuple<Boolean,String> (true, "");
        }

        public void modifyObjectState(String name, String property, String value)
        {
            /*Function Description : Modify the object with the following name
             * by adding the property, value tuple*/
            foreach (Object oneObject in objects)
            {
                if (oneObject.uniqueName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    oneObject.addState(property, value);
                }
            }
        }

        public int numObjects()
        {
            //Function Description: Returns number of objects in this environment
            return this.objects.Count();
        }

        public double distance(Environment env)
        {
            /* Function Description : Distance of this environment from
             * the given environment.*/

            double dist = 0, numMatching=0;

            foreach(Object u in this.objects)
            {
                List<Tuple<String, String>> uState = u.getState();
                String uName = u.getName();

                //find this object in list of env's object
                foreach (Object v in env.objects)
                {
                    String vName=v.getName();
                    if (vName.Equals(uName)) //same object in both list
                    {
                        numMatching++;
                        List<Tuple<String, String>> vState = v.getState();
                        //find distance between the two states
                        dist = dist + u.findDistance(vState).Item1;
                    }
                }
            }
            double penalty = 0.5 * (this.objects.Count + env.objects.Count - 2*numMatching);
            return dist + penalty;
        }

        public Object findObject(String objectName)
        {
            /* Function Description : Return the object with the given name*/
            foreach(Object iter in this.objects)
            {
                if (iter.uniqueName.Equals(objectName))
                    return iter;
            }
            return null;
        }

        public bool objectExists(String objName)
        {
            /* Function Description : Searches for object with the name objName 
             * and returns true/false accordingly*/
            foreach (Object single in this.objects)
            {
                if (single.getName().Equals(objName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public void changeObjectPosRel(String obj1Name, String obj2Name, String newRelativePos)
        {
            /*Function Description : Change this object's location based on given object.
             This object is kept relative to obj with new relative position given. */
            int i = -1, j = -1;
            for (int iter = 0; iter < objects.Count(); iter++)
            {
                if (this.objects[iter].uniqueName.Equals(obj1Name, StringComparison.OrdinalIgnoreCase))
                    i = iter;
                if (this.objects[iter].uniqueName.Equals(obj2Name, StringComparison.OrdinalIgnoreCase))
                    j = iter;
            }

            if (i != -1 && j != -1)
                relativePosition.Add(new Tuple<int, int, string>(i, j, newRelativePos));
        }

        public void changeObjectPosRel(Object obj1, Object obj2, String newRelativePos)
        {
            /*Function Description : Change this object's location based on given object.
             This object is kept relative to obj with new relative position given. */
            int i = -1, j = -1;
            for (int iter = 0; iter < objects.Count(); iter++)
            {
                if (this.objects[iter].uniqueName.Equals(obj1.uniqueName, StringComparison.OrdinalIgnoreCase))
                    i = iter;
                if (this.objects[iter].uniqueName.Equals(obj2.uniqueName, StringComparison.OrdinalIgnoreCase))
                    j = iter;
            }

            if (i != -1 && j != -1)
                relativePosition.Add(new Tuple<int, int, string>(i, j, newRelativePos));
        }

        public bool objectAndStateValueExists(String objName, String stateName, String value)
        {
            /* Function Description : Searches for object with the name objName 
             * and if yes then looks for the particular state with the particular value.
             * Returns True/False accordingly. */
            foreach (Object single in this.objects)
            {
                if (single.getName().Equals(objName, StringComparison.OrdinalIgnoreCase))
                {
                    if (single.getStateValue(stateName).Equals(value, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        public bool envEqualOn(Environment env, List<String> cover)
        {
            /* Function Description: Finds if environment this and env
             * agree on all the objects in the cover. Return false if 
             * - all the objects are not present in both the environment 
             * - objects are not agreeing on all the states value
             * - relationships are same in both environment for both objects
             *   in the cover
             *  */

            foreach (String objName in cover)
            {
                Object obj1 = this.findObject(objName);
                Object obj2 = env.findObject(objName);
                if (obj1 == null || obj2 == null) 
                    return false;

                //check if these objects are totally identical
                if (obj1.findDistance(obj2.getState()).Item1 > 0)
                    return false;
            }

			foreach (Tuple<Object,Object,SpatialRelation> rel in this.relationshipMatrix) 
			{
				if (cover.Contains (rel.Item1.uniqueName) && cover.Contains (rel.Item2.uniqueName)) 
				{
					if (!env.checkRelExists (rel.Item1.uniqueName, rel.Item2.uniqueName, rel.Item3))
						return false;
				}
			}

			foreach (Tuple<Object,Object,SpatialRelation> rel in env.relationshipMatrix) 
			{
				if (cover.Contains (rel.Item1.uniqueName) && cover.Contains (rel.Item2.uniqueName)) 
				{
					if (!this.checkRelExists (rel.Item1.uniqueName, rel.Item2.uniqueName, rel.Item3))
						return false;
				}
			}

            return true;
        }

		public List<String> difference(Environment env)
		{
			/* Function Description: Computes the set of predicates that are different in the two env.
			 * The output is of the form - p1, p2, p3, p4 ... pk where pi = {Predicate-Atom, Not Predicate-Atom}
			 * It means that these predicates are true in this environment that are not true in env */

			List<String> predicates = new List<String> ();
			//Object-states
			for (int i = 0; i<this.objects.Count(); i++) 
			{
				String name = this.objects [i].uniqueName;
				Object ob_ = env.findObject (name);
				if (ob_ == null)
					throw new ApplicationException ("Difference Function should be called on environments with same object");

				List<Tuple<String,String>> sList = this.objects[i].getState();
				for(int j =0; j<sList.Count();j++)
				{
					//check if this state sList[j].Item1 is also true in ob_
					if (!ob_.checkStateAndVal (sList[j].Item1,sList[j].Item2)) 
					{
						if (sList [j].Item2.Equals ("True"))
							predicates.Add ("(state " + name + " " + sList [j].Item1 + ")");
						else
							predicates.Add ("(not (state " + name + " " + sList [j].Item1 + "))");
					}
				}
			}

			//Object-Object relationship matrix
			foreach (Tuple<Object,Object,SpatialRelation> rel in this.relationshipMatrix)
			{
				if(!env.checkRelExists(rel.Item1.uniqueName,rel.Item2.uniqueName,rel.Item3))
					predicates.Add ("("+rel.Item3.ToString()+" "+rel.Item1.uniqueName+" "+rel.Item2.uniqueName+")");
			}

			foreach (Tuple<Object,Object,SpatialRelation> rel in env.relationshipMatrix)
			{
				if(!this.checkRelExists(rel.Item1.uniqueName,rel.Item2.uniqueName,rel.Item3))
					predicates.Add ("(not ("+rel.Item3.ToString()+" "+rel.Item1.uniqueName+" "+rel.Item2.uniqueName+"))");
			}

			return predicates;
		}
	
        public double envDistanceOn(Environment env, List<String> objList)
        {
            /* Function Description : Finds the distance between the this and the env
             * environment based on all the objects in the objList. Returns the average
             * score. If an object is not found then a penaly of 1 is added. The score
             * is a value in [0,1] with the minimum the better*/
            if(objList.Count()==0)
                return 0;

            double score = 0;
            foreach (String objName in objList)
            {
                Object obj1 = this.findObject(objName);
                Object obj2 = env.findObject(objName);
                if (obj1.isDummy() || obj2.isDummy())
                    score = score + 1;
                //check if these objects are totally identical
                score = score + obj1.findDistance(obj2.getState()).Item1;
            }
            
            return score/(double)objList.Count();
        }

		public List<String> fetchObjPredicates(String objName)
		{
			/* Function Description: Fetch the object predicates 
			 * with the given object name */
			List<String> predicates = new List<String> ();
			Object obj = this.findObject (objName);
			if (obj==null)
				return predicates;
			List<Tuple<String,String>> svalues = obj.getState ();
			foreach (Tuple<String,String> svalue in svalues) 
			{
				if (svalue.Item2.Equals ("True"))
					predicates.Add ("(state "+objName+" "+svalue.Item1+")");
				else predicates.Add ("(not (state "+objName+" "+svalue.Item1+"))");
			}

			foreach (Tuple<Object,Object,SpatialRelation> srs  in this.relationshipMatrix) 
			{
				if(srs.Item1.uniqueName.Equals(objName) || srs.Item2.uniqueName.Equals(objName)) 
					predicates.Add("("+srs.Item3.ToString()+" "+srs.Item1.uniqueName+" "+srs.Item2.uniqueName+")");
			}
			return predicates;
		}

        public void loadEnvironment(String envFileName)
        {
            /* Function Description : Environment in version 3 and up are represented
             * by xml structure. This function loads data from the xml file. The format
             * of the xml file is - 
             *          <environment>
             *             <object>
             *                  <name>-----</name>
             *                  <position>(x,y,z)</position>
             *                  <rotation>(a,b,c)</rotation>
             *                      <state>
             *                          <statename>string</statename>
             *                          <statevalue>string</statevalue>
             *                      </state>
             *                      ......
             *            </object>
             *            ......
             *          </environment>
             *          
             * static attributes about the object are given in the file object.xml
             * Note Mug1 and Mug2 are same class of objects i.e. Mug but can have
             * different static attributes like color. 
             */
             Console.WriteLine(Constants.dataFolder+@"/Environment/"+envFileName);
            XmlTextReader reader = new XmlTextReader(Constants.dataFolder+@"/Environment/"+envFileName);

            Object obj = null;
            String currentStateName = null, currentStateValue = null;
            this.relFileName = envFileName;
            Object rbt = new Object();
            rbt.uniqueName = "Robot";
            this.objects.Add(rbt); //I believe Robot should be a subclass of type object

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("object"))
                        {
                            obj = new Object();
                        }

                        if (reader.Name.Equals("name"))
                        {
                            reader.Read();
                            obj.bootStrap(reader.Value); //bootstraps the object
                            break;
                        }

                        if(reader.Name.Equals("statename"))
                        {
                            reader.Read();
                            currentStateName = reader.Value;
                            break;
                        }

                        if(reader.Name.Equals("statevalue"))
                        {
                            reader.Read();
                            currentStateValue = reader.Value;
							int num = -1;
							if(reader.Value.Equals("Low"))
								currentStateValue="False";
							if (int.TryParse (currentStateValue, out num)) 
							{
								if (num > 0)
									currentStateValue = "True";
							}
						    if (!currentStateValue.Equals ("False") && !currentStateValue.Equals ("True")) 
							    throw new ApplicationException ("State "+currentStateName+" must admit binary values. Received "+currentStateValue);
                            break;
                        }
                        if (reader.Name.Equals("position"))
                        {
                            reader.Read();
                            string[] position = reader.Value.Split(new char[] {','}) ;
                            obj.changePosition(double.Parse(position[0].Replace('(',' ')), double.Parse(position[1]), double.Parse(position[2].Replace(')',' ')));
                        }
                        if (reader.Name.Equals("rotation"))
                        {
                            reader.Read();
                            string[] rotation = reader.Value.Split(new char[]{','});
                            obj.changeRotation(double.Parse(rotation[0].Replace('(',' ')), double.Parse(rotation[1]), double.Parse(rotation[2].Replace(')',' ')));
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name.Equals("state"))
                        {
                            obj.addState(currentStateName, currentStateValue);
                            currentStateName = null; currentStateValue = null;
                        }
                        if (reader.Name.Equals("object"))
                        {
							if (obj.uniqueName.Equals ("Camera1") || obj.uniqueName.Equals ("camera_1"))
							;//this.rbt.moveRobot(obj.centroidX_, obj.centroidY_);
                            else
                            {
                                Object tmp = obj;
                                obj = null;
								/* HACK HACK */
								if (tmp.uniqueName.Equals ("Mug_1"))
									tmp.changeState ("Coffee", "True");
								if (tmp.uniqueName.Equals ("Syrup_1"))
									tmp.changeState ("Vanilla", "True");
								if (tmp.uniqueName.Equals ("Syrup_2"))
									tmp.changeState ("Chocolate", "True");
	                                this.objects.Add(tmp);
                            }
                        }
                        break;
                }
            }

			Object tv = this.objects.Find(x=>x.uniqueName.Equals("Tv_1"));
			if (tv != null) 
			{
				int channelcount = 0;
				foreach (Tuple<String,string> channel in tv.state_) 
				{
					if(channel.Item1.StartsWith("Channel") && channel.Item2.Equals("True"))
						channelcount++;
				}
				if (channelcount > 1)
					throw new ApplicationException ("More than one channel on in "+envFileName);
			}

			this.bootstrapSpatialRelationship ();
			reader.Close ();
        }

        public double envManyManyDistance(Environment env, List<Tuple<String, String>> matching)
        {
            /* Function Description : Computes many-to-many distance between environment
             * using matching relation. An entry (x,y) in matching corresponds to x for 
             * env1 and y for env2. If one exists but not the other then we add 1 else we
             * add 0. If both exist then a distance is added. */

            if (matching.Count() == 0)
                return 0;
            double score = 0;
            foreach (Tuple<String, String> match in matching)
            {
                String obj1 = match.Item1;
                String obj2 = match.Item2;
                if (this.objectExists(obj1) && this.objectExists(obj2))
                {
                    Object obj_1 = this.findObject(obj1);
                    Object obj_2 = env.findObject(obj2);
                    score = score + obj_1.findDistance(obj_2.getState()).Item1;
                }
                else if (!this.objectExists(obj1) && !this.objectExists(obj2))
                {
                    score = score + 0;
                }
                else
                {
                    score = score + 1;
                }
            }
            score = score / (double)matching.Count();
            return score;
        }

        public List<Instruction> interpolation(Environment start, Environment end, int bound, List<Tuple<String,String> > matching)
        {
            /* Function Description : Finds a program of length <= bound 
             * which interpolates the environment start and end. The matching is
             * used to 
             *
             * Search algorithm used is A* with - 
             * 
             * g ( [I, E] ) = sum sum w^T * Phi([I E])
             * h ( [I, E] ) = distance (Env,end)
             *
             */

            bool achieved = true;
            int steps = 0;

            InstructionTree root = new InstructionTree(start);
            List<Tuple<InstructionTree,double>> frontier = new List<Tuple<InstructionTree,double>>(); /* frontier containing
                                                                                                       * nodes along with the
                                                                                                       * cost g(n) + h(n)
                                                                                                       */

            frontier.Add(new Tuple<InstructionTree, double>(root,root.h(end,matching)));

            while (steps < bound && achieved)
            {
                //Pick one node from the frontier and expand
                InstructionTree pickedNode = null;
                int index = -1;

                if (frontier.Count() > 0)
                {
                    double minScore = Double.PositiveInfinity;
                    for (int iter = 0; iter < frontier.Count(); iter++)
                    {
                        if (frontier[iter].Item2 < minScore)
                        {
                            index = iter;
                            minScore = frontier[iter].Item2;
                        }
                    }
                }
                else
                {
                    //not sure
                    return new List<Instruction>(); //empty-set
                }

                pickedNode = frontier[index].Item1;
                double pScore = frontier[index].Item2;

                //Check if pickedNode is good enough
                if (pickedNode.h(end,matching)==0)
                {
                    achieved = true;
                    return pickedNode.returnPath();
                }

                //Remove pickedNode from frontier and add the children to the frontier
                frontier.RemoveAt(index);
                pickedNode.expand(frontier, end, matching, pScore);
                steps++;
            }

            return new List<Instruction>();
        }

        // Objects Manipulation

        public void removeObjects(String name)
        {
            /* Functin Description : Remove all properties with this object */
            Object tmp_ = null;
            foreach (Object obj_ in this.objects)
            {
                if (obj_.uniqueName.Equals(name))
                {
                    tmp_ = obj_;
                    break;
                }
            }

            if (tmp_ == null)
                return;

            this.objects.Remove(tmp_);
            this.relationshipMatrix.RemoveAll(x => x.Item1.uniqueName.Equals(name) || x.Item2.uniqueName.Equals(name));
        }

        // Functions for dealing with relationship

        public static SpatialRelation parseRelationship(String relationship)
        {
            /* Function Description : Given a relationship in string, parse and return 
             * the spatial relationship that is represented */

            if (relationship.Equals("In") || relationship.Equals("Inside"))
                return SpatialRelation.In;
            if (relationship.Equals("On") || relationship.Equals("Above"))
                return SpatialRelation.On;
            if (relationship.Equals("Grasping") || relationship.Equals("IsGrasping"))
                return SpatialRelation.Grasping;
            if (relationship.Equals("Near") || relationship.Equals("IsNear"))
                return SpatialRelation.Near;
            else throw new ApplicationException("Cannot parse the relationship " + relationship);
        }

        public bool checkRelExists(String objName1, String objName2, SpatialRelation relation)
        {
            /* Function Description : Check if relationship exists */
            foreach (Tuple<Object, Object, SpatialRelation> t in this.relationshipMatrix)
            {
                if (t.Item1.getName().Equals(objName1) && t.Item2.getName().Equals(objName2) && t.Item3.Equals(relation))
                    return true;
            }
            return false;
        }

        public void addRelationShip(String objName1, String objName2, SpatialRelation relation)
        {
            /* Function Description : Adds relationship to the matrix. If it already exists then nothing is done. */

            if (this.checkRelExists(objName1, objName2, relation)) //already exists
                return;
            Object obj1 = this.findObject(objName1);
            Object obj2 = this.findObject(objName2);
			if (obj1 == null || obj2 == null) 
			{
				String error = this.relFileName;
				if (obj1 == null)
					error =  error+" "+objName1;
				if (obj2 == null)
					error =  error+" "+objName2;
				throw new ApplicationException ("Error: Null Objects are Referenced "+error);
			}
            this.relationshipMatrix.Add(new Tuple<Object, Object, SpatialRelation>(obj1, obj2, relation));
        }

        public void removeRelationShip(String objName1, String objName2, SpatialRelation relation)
        {
            /* Function Description : Removes relationship from the matrix if it exists */
            int index = -1;
            for (int i = 0; i < this.relationshipMatrix.Count(); i++)
            {
                Tuple<Object, Object, SpatialRelation> t = this.relationshipMatrix[i];
                if (t.Item1.getName().Equals(objName1) && t.Item2.getName().Equals(objName2) && t.Item3.Equals(relation))
                    index = i;
            }

            if (index == -1)
                return;

            this.relationshipMatrix.RemoveAt(index);
        }

        public SpatialRelation getRelationship(int i, int j)
        {
            /* Function Description: Return the relationship between objects indexed by i and j */

            Object objI = this.objects[i], objJ = this.objects[j];
            foreach(Tuple<Object,Object,SpatialRelation> t in this.relationshipMatrix)
            {
                if (t.Item1 == objI && t.Item2 == objJ)
                    return t.Item3;
            }

            return SpatialRelation.None; //no-relationship
        }

        // Function to compute Environment-Environment Relationship

        public double[,] getEnvCorrMatrix(Environment env)
        {
            /* Funtion Description: Computes Correlation between this and env environment.
             * Correlation Matrix e[i,j] has entries corresponding to object obj_i in this 
             * environment and env_j and is an entry between 0-1. Where 0 means that the two
             * objects are not correlated at all while 1 means that they are totally correlated.
             * The precise meaning of correlation can change and different methods should be tried.
             * For the moment, 
             *  M1. trying e[i,j] = state-similarity of object i and j
             *      M1 is bad cause most objects dont have any states are hence treated the same
             *  M2. trying e[i,j] = 1/2 {same object-class} + 1/2 {state-similarity of object i and j}
             */

            int m = this.objects.Count(), n = env.objects.Count();
            double[,] envCorrMatrix = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
				{
					double score =  1 - this.objects[i].findDistance(env.objects[j].getState()).Item1; //try different combinations
					if (Global.base_ (this.objects [i].uniqueName).Equals (Global.base_ (env.objects [j].uniqueName)))
						score = 0.5 * score + 0.5;
					else
						score = 0.5 * score;
					envCorrMatrix [i, j] = score;
				}
            }

            return envCorrMatrix;
        }

        // Functionalities to assist the inference

        public int isSastified(String constraint)
        {
            /* Function Description : Checks if the constraint is satisfied by the
             * given environment. The constraint is of 3 types - 
             *      1. state objName stateName
             *      2. affordance-type objName
             *      3. relationship-type objName1 objName2 (special =)
             * Return is a {-1,0,1} coding scheme. Where -1 means
             * there is an error like (state objName stateName) and 
             * obj either does not exist or does not have the state. 0 means 
             * its syntactical correct and is false and 1 means its syntactically
             * correct and its true.
             * */

            if (constraint.Equals("true") || constraint.Equals(""))
                return 1;

            String[] words = constraint.Split(new char[]{' '});
			if (words [0].Equals ("state")) //constraint of type 1
			{ 
				//objName = words[1], stateName = words[2]
				Object objFound = this.findObject (words [1]);
                
				if (objFound == null)
					return -1;
				/*if (!objFound.ifStateExist(words[2]))
                    return -1;*/
				if (objFound.checkStateAndVal (words [2], "True"))
					return 1;
				else
					return 0;
			}
			else if (words.Count () == 2)
			{
				//affordance-type = words[0], objName = words[1]
				Object objFound = this.findObject (words [1]);
                
				if (objFound == null || !objFound.affordances_.Contains (words [0]))
					return -1;              
				return 1;
			}
            else if (words.Count() == 3)
            {
                //relationship-type = words[0], objName1 = words[1], objName2 = words[2]
				if(words[0].Equals("="))
				{
					if (words [1].Equals (words [2]))
						return 1;
					else return 0;
				}	

                Object obj1 = this.findObject(words[1]);
                Object obj2 = this.findObject(words[2]);
                if (obj1 == null || obj2 == null)
                    return -1;
                if(this.checkRelExists(words[1], words[2], Environment.parseRelationship(words[0])))
                    return 1;
                else return 0;
            }
            else throw new ApplicationException("PDDL-Contraint Parser Error: Cannot Parse Constraint - "+constraint+"."); 

        }

		public void modify(String constraint, bool truth)
		{
			/*Function Description: Constraint represents a relation which is
			 * of one of the two types - 1. (state objName stateName) or 2. (relation objName1 objName2)
			 * If true is true then make these conditions true else make them false */

			String[] words = constraint.Split(new char[]{' '});
			if (words [0].Equals ("state")) //constraint of type 1
			{
				//objName = words[1], stateName = words[2]
				Object objFound = this.findObject (words [1]);
				if (objFound==null)
					return;
				if (truth) 
					objFound.changeState (words [2], "True");
				else
					objFound.changeState (words [2], "False");
			}
			else if (words.Count() == 3)
			{
				//relationship-type = words[0], objName1 = words[1], objName2 = words[2]
				if (truth) 
					this.addRelationShip (words [1], words [2], Environment.parseRelationship(words [0]));
				else this.removeRelationShip(words [1], words [2], Environment.parseRelationship(words [0]));
			}
			else throw new ApplicationException("PDDL-Contraint Parser Error: Cannot Parse Constraint - "+constraint+"."); 
		}


        public double[,] getLECorrMatrix(Clause cls, List<Instruction> instPrev, SentenceSimilarity sensim, Features ftr)
        {
            /* Function Description: Gives the symbol-grounding probability matrix.
             * Ex: the word "red cup" is strongly correlated with an object which 
             * is a red cup than with a red mug
             * M1. if main-noun is same as object unique-name like cup matches cup_1 but not mug_2
             * M2. return sentence similarity of main-noun with object base form unique-name based on Word-Net
             *     [does not work with containment: game matches to bowl etc., too time consuming]
             * M3. check if main-noun is same as object unique name [M1] //save time
             *     check for containment - statevalue should match //containment
             *     check for sentence similarity [does not handle anaphoras and determiner]
             * M4. M3 + Anaphoric Resolution
             *     if the Noun-Phrase is anaphoric in nature such as "it" then:-
             *           LE["it",object] = if |inst|> 0 (last-ref object)/(length of inst + epsilon) else 1/(no. of objects)
             *     if there is a determiner then:-
             *           LE[DT:w, object] = e^{last_index[object reference]-pos_"DT"} LE[w,object]
             * M5: M4 + Giza-PP probabilities
             *     Step 1: handle anaphora; Step 2: check if main-noun is same; Step 3: check for containment
             *     Step 4: check for giza-pp probability; Step 5: check for sentence similarity -- giza-pp not reliable
			 * M6: Modified M5
			 *     Step 1: handle anaphora; Step 2: check if main-noun is same; Step 3: check for containment
			 *     Step 5: check for sentence similarity; Step 6: if no match above 0.85 else return giza-pp
			 *     if still all fails then Step 7: return uniform distribution 
			 * M7: Modified M6 + Part of Category Match
			 *     Step 1: handle anaphora; Step 2: check if part of description matches part of some object name. 
			 *     E.g., egg matches part of Boiled Egg. Then if there is a specification boost these categories.
			 *     Step 3: check for containment; Step 5: check for sentence similarity;
			 *     Step 6: if no match above 0.85 else return giza-pp if still all fails then Step 7: return uniform distribution */

            int lng = cls.lngObj.Count(), obj = this.objects.Count();
            double[,] leCorrMatrix=new double[lng,obj];
            for (int i = 0; i < lng; i++)
            {
				String mainNoun = cls.lngObj [i].getName ();
				#region anaphoric_resolution
				if (mainNoun.Equals ("it"))
				{
					for (int j=0; j<obj; j++) 
					{
						if (instPrev == null || instPrev.Count == 0)
							leCorrMatrix [i, j] = 1.0; // (obj + Constants.epsilon);
						else
						{
							int lastReference = -1;
							for(int iter = instPrev.Count()-1;iter >=0;iter--)
							{
								if (instPrev [iter].getArguments ().Contains (this.objects [j].uniqueName)) 
								{
									lastReference = iter;
									break;
								}
							}

							if(lastReference==-1)
								leCorrMatrix [i, j]=0; //object never occured in the sequence
							else if(instPrev.Count()-lastReference-1<=5)
								leCorrMatrix [i, j]=1; //smoothing out the fraction -- all near behave same
							else leCorrMatrix[i, j]=(lastReference+1)/Math.Max(instPrev.Count(), Constants.epsilon);
							//leCorrMatrix [i, j] = (lastReference + 1) / (instPrev.Count()+Constants.epsilon);
						}
					}
					continue;
				}
				#endregion

				#region check_for_category
				bool found = false;

				for (int j = 0; j < obj; j++)
				{
					String baseForm = Global.fetchObjExpand(this.objects [j].uniqueName);
					if(mainNoun.Equals(baseForm, StringComparison.OrdinalIgnoreCase)
					   || Processing.basePlural(mainNoun).Equals(baseForm,StringComparison.OrdinalIgnoreCase))
					{
						leCorrMatrix[i,j] = 1;
						found=true;
					}
					else leCorrMatrix[i,j] = 0;
				}

				if(found)
					continue;

				List<String> words = mainNoun.Split (new char[] {' '}).ToList();
				List<int> index=new List<int>();

				for (int j = 0; j < obj; j++)
				{
					String baseForm = Global.fetchObjExpand(this.objects [j].uniqueName);
					List<String> objectDescription = baseForm.Split(new char[]{' '}).ToList();
					if(words.Exists(x=> objectDescription.Exists(y=> Processing.basePlural(x).Equals(Processing.basePlural(y),StringComparison.OrdinalIgnoreCase))))
					{
						leCorrMatrix[i,j] = 1;
						index.Add(j);
						found=true;
					}
					else leCorrMatrix[i,j] = 0;
				}

				if(found)
				{
					/* if there are extra words then use them for boosting
					 * out of those index are that are 1. Keep the one with maximum
					 * giza-pp as the maxima */
					if(words.Count() > 1 && index.Count()>1)
					{
						double[] giza = new double[index.Count()];
						for(int iter=0; iter<index.Count(); iter++)
						{
							giza[iter]=0;
							//compute average giza-pp
							int j = index[iter];
							foreach(String word in words)
							{
								Tuple<String,String,Double> result = ftr.gizaProbabilities.Find(x=>x.Item1.Equals(word,StringComparison.OrdinalIgnoreCase)
								                                                                && x.Item2.Equals(this.objects[j].uniqueName,StringComparison.OrdinalIgnoreCase));
								if(result!=null)
									giza[iter] = giza[iter]+ result.Item3;
							}
						}

						double maxGiza = giza.Max();
						for(int iter=0; iter<index.Count(); iter++)
						{
							if(giza[iter] != maxGiza)
								leCorrMatrix[i,index[iter]]=0;
						}
					}
					index.Clear();
					continue;
				}
				#endregion

				#region check_for_containment
				for (int j = 0; j < obj; j++)
				{
					List<Tuple<String,String>> states = this.objects [j].getState ();
					bool stateFound = false;
					for (int state = 0; state<states.Count(); state++) 
					{
						if (mainNoun.Equals (states [state].Item1,StringComparison.OrdinalIgnoreCase)
						    && states[state].Item2.Equals("True",StringComparison.OrdinalIgnoreCase)) 
						{
							stateFound = true;
							break;
						}
					}
					if (stateFound)
					{
						leCorrMatrix [i, j] = 1;
						found = true;
					} 
					else leCorrMatrix [i, j] = 0;
				}

				if(found)
					continue;
				#endregion

				#region WordNet_Similarity
				double max = 0;
                for (int j = 0; j < obj; j++)
                {
					String baseForm = Global.fetchObjExpand(this.objects [j].uniqueName);
					double score_ = sensim.GetScore (mainNoun, baseForm);
					if(score_>max)
						max = score_;
					leCorrMatrix [i, j] = score_;
                }

				if(max>=0.85)//threshold of confidence, to be replaced by learned vector
					continue;
				#endregion

				#region Giza_Probabilities
				String[] words_ = mainNoun.Split(new char[]{' '});
				//check if all the words_ exists in giza file
				found = true; //assume it to be true
				for(int word =0; word<words_.Length;word++)
				{
					if(!ftr.gizaProbabilities.Exists(x=>x.Item1.Equals(words_[word],StringComparison.OrdinalIgnoreCase)))
					{
						found = false;
						break;
					}
				}

				if(found)
				{
					double maxProbability = Double.NegativeInfinity;
					int maxIndex = -1;
					for (int j = 0; j < obj; j++)
					{
						//compute correlation probability for each object
						double probability = 0;
						for(int word=0; word<words_.Length;word++)
						{
							Tuple<String,String,Double> result = ftr.gizaProbabilities.Find(x=>x.Item1.Equals(words_[word],StringComparison.OrdinalIgnoreCase)
							                                                                && x.Item2.Equals(this.objects[j].uniqueName,StringComparison.OrdinalIgnoreCase));
							if(result!=null)
								probability += result.Item3;
						}
						probability = probability/(words_.Length+Constants.epsilon);
						if(probability > maxProbability)
						{
							maxProbability = probability;
							maxIndex = j;
						}
					}

					for(int j=0; j<obj;j++)
						leCorrMatrix[i,j] = 0;
					leCorrMatrix[i,maxIndex] = 1;
					continue;
				}
				#endregion

				//if all fails then return uniform distribution
				#region return_average_probability
				for(int j=0; j<obj;j++)
					leCorrMatrix[i,j] = 1.0/(obj+Constants.epsilon);
				#endregion
            }

            return leCorrMatrix;
        }

		public void bootstrapSpatialRelationship ()
		{
			/* Function Description: Given this environment; bootstrap the spatial relationship between objects.
			 * Right now, we are concerned with On and In relationship */

			for (int i=0; i<this.objects.Count(); i++) 
			{
				//an object can be on top of or inside only one object
				double minDist = Double.PositiveInfinity;
				int index = -1;
				for (int j=0; j<this.objects.Count(); j++) 
				{
					if (!this.objects [j].affordances_.Contains ("IsPlaceableOn") && 
					    !this.objects [j].affordances_.Contains ("IsPlaceableIn") || 
					    !this.objects [i].affordances_.Contains ("IsGraspable")  || 
					     this.objects [j].uniqueName.StartsWith("Microwave") || 
					    this.objects [j].uniqueName.StartsWith("Sink")) //last two are hacks since we dont have bounding boxes yet, hence irrelevant
						//objects are being declared to be on sink or in microwave when in fact they are not. Having bounding boxes will solve this problem
						continue;
					double distance = this.objects [i].getL2PlanarDistance (this.objects [j].centroidX_, this.objects [j].centroidY_);
					if (distance < minDist) 
					{
						index = j;
						minDist = distance;
					}
				}

				if(index!=-1 && minDist < 25)
				{
					if (this.objects [index].affordances_.Contains ("IsPlaceableOn")) 
						this.relationshipMatrix.Add (new Tuple<Object, Object, SpatialRelation> (this.objects [i], this.objects [index], SpatialRelation.On));
					else
						this.relationshipMatrix.Add (new Tuple<Object, Object, SpatialRelation> (this.objects [i], this.objects [index], SpatialRelation.In));
				}
			}
		}
    }
}
