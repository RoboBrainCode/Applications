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
    class ObjectKnowledgeBase
    {
        /* Class Description : Contains knowledge base of objects. 
         * Example : Which object can do what actions etc.
         */

        private List<String> findable = new List<String>() { "Cup", "Mug", "Table", "Desk", "Stove", "StoveKnob", "Microwave", "Door", "Coffee-Powder", "Tap", "Milk-Box", "Sugar-Box", "Plate", "mug_1", "stove_1", "microwave_1", "crockPot_1", "fridge_1", "syrupBottle_1", "ramen_1", "spoon_1", "iceCreamBox_1", "counter_1", "sink_1", "loveseat_1", "armchair_1", "coffeeTable_1", "snackTable_1", "tvTable_1", "tv_1", "tv_1Remote_1", "pillow_1", "pillow_2", "pillow_3", "garbageBag_1", "garbageBin_1", "bowl_1", "bagOfChips_1"};
        private List<String> openable = new List<String>() { "Door", "microwave_1", "fridge_1LeftDoor", "fridge_1RightDoor" };
        private List<String> pressable = new List<String>() { "Button", "fridge_1WaterButton", "microwave_1CookButton", "tv_1Remote_1" };
        private List<String> graspable = new List<String>() { "Mug", "Cup", "Tap", "Coffee-Powder", "Milk-Box", "Sugar-Box", "Plate", "mug_1", "crockPot_1", "syrupBottle_1", "ramen_1", "spoon_1", "iceCreamBox_1", "tv_1Remote_1", "pillow_1", "pillow_2", "pillow_3", "garbageBag_1", "garbageBin_1", "bowl_1", "bagOfChips_1" };
        private List<String> placable = new List<String>() { "Desk", "Table", "Tap", "Stove", "counter_1", "stove_1Burner_1", "stove_1Burner_2", "stove_1Burner_3", "stove_1Burner_4", "sink_1", "crockPot_1", "microwave_1", "tvTable_1" };
        private List<String> closable = new List<String>() { "Door", "microwave_1" ,"fridge_1LeftDoor", "fridge_1RightDoor" };
        private List<String> turnable = new List<String>() { "Tap", "StoveKnob", "stove_1Knob_1", "stove_1Knob_2", "stove_1Knob_3", "stove_1Knob_4", "sink_1Knob_1" };
        private List<String> scoopable = new List<String>() { "spoon_1" };
        private List<String> squeezable = new List<String>() { "syrupBottle_1" };

        public bool doesExist(List<string> ls, String x)
        {
            /* Function Description :  Checks the list ls for element x*/
            foreach (String l in ls)
            {
                if (l.Equals(x, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public bool isFindable(String objName)
        {
            /* Function Description : Checks if object is findable */
            return this.doesExist(findable, objName);
        }

        public bool isGraspable(String objName)
        {
            /* Function Description : Checks if object is graspable */
            return this.doesExist(graspable, objName);
        }

        public bool isOpenable(String objName)
        {
            /* Function Description : Checks if object is openable */
            return this.doesExist(openable,objName);
        }

        public bool isPressable(String objName)
        {
            /* Function Description : Checks if object is pressable */
            return this.doesExist(pressable, objName);
        }

        public bool isPlacable(String objName)
        {
            /* Function Description : Checks if object is placable */
            return this.doesExist(placable, objName);
        }

        public bool isClosable(String objName)
        {
            /* Function Description : Checks if object is closable */
            return this.doesExist(closable, objName);
        }

        public bool isTurnable(String objName)
        {
            /* Function Description : Checks if object is turnable */
            return this.doesExist(turnable, objName);
        }

        public bool isScoopable(String objName)
        {
            /* Function Description : Checks if object is scoopable */
            return this.doesExist(scoopable, objName);
        }

        public bool isSqueezable(String objName)
        {
            /* Function Description : Checks if object is squeezable */
            return this.doesExist(squeezable, objName);
        }

        public List<String> findables()
        {
            /* Function Description : Returns the list of findable objects*/
            return this.findable;
        }

        public List<String> openables()
        {
            /* Function Description : Returns the list of openable objects*/
            return this.openable;
        }

        public List<String> pressables()
        {
            /* Function Description : Returns the list of pressable objects*/
            return this.pressable;
        }

        public List<String> graspables()
        {
            /* Function Description : Returns the list of graspable objects*/
            return this.graspable;
        }

        public List<String> placables()
        {
            /* Function Description : Returns the list of placable objects*/
            return this.placable;
        }

        public List<String> closables()
        {
            /* Function Description : Returns the list of closable objects*/
            return this.closable;
        }

        public List<String> turnables()
        {
            /* Function Description : Returns the list of turnable objects*/
            return this.turnable;
        }

        public List<String> scoopables()
        {
            /* Function Description : Returns the list of scoopable objects*/
            return this.scoopable;
        }

        public List<String> squeezables()
        {
            /* Function Description : Returns the list of squeezable objects*/
            return this.squeezable;
        }

        public List<String> returnObjectParts(String objName)
        {
            /* Function Description : Returns all parts of the given object 
             * including the object itself */

            return new List<string>() { objName};
        }
        
    }
}
