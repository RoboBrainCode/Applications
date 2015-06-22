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
    class InstructionTree
    {
        Instruction node = null;
        List<InstructionTree> children = null;
        Environment env = null; // result of application of this instruction on the environment of the parent node
        InstructionTree parent = null;
        double score = 0;

        static List<String> clsW = null;

        public InstructionTree(Instruction node)
        {
            this.node = node;
            this.children = new List<InstructionTree>();
            this.computeProbability();
        }

        public InstructionTree(Environment env)
        {
            /* Constructor Description : Used by root node to initialize
             * the starting environment */
            this.children = new List<InstructionTree>();
            this.env = env;
        }

        public double h(Environment goal, List<Tuple<String,String>> matching)
        {
            /*  Function Description : Heuristic Function*/
            double result = goal.envManyManyDistance(this.env,matching);
            return result;
        }

        public List<Instruction> returnPath()
        {
            /* Function Description : Returns instruction set
             * from top node to this node */
            List<Instruction> program = new List<Instruction>();
            InstructionTree iter=this;
            while (iter.node != null)
            {
                program.Insert(0,iter.node);
                iter = iter.parent;
            }

            return program;
        }

        public void add(Instruction inst, List<Tuple<InstructionTree, double>> frontier, Environment goal, List<Tuple<String,String>> matching, double pScore)
        {
            /* Function Description : Given instruction inst, add it to the frontier
             * after computing score */

            Simulator sml = new Simulator();
            InstructionTree newNode = new InstructionTree(inst);
            newNode.env = sml.execute(inst, this.env);
            newNode.parent = this;
            this.children.Add(newNode);

            double score = newNode.h(goal, matching) + pScore;

            frontier.Add(new Tuple<InstructionTree, double>(newNode,score));
        }

        public void safeAdd(Instruction inst)
        {
            /* Function Description :  Adds the instruction if syntactically correct */
            Simulator sml = new Simulator();
            double val = sml.satSyntConstraints(inst, this.env).Item1;
            if (val == 0)
            {
                InstructionTree child = new InstructionTree(inst);
                child.env = sml.execute(inst, this.env);
                child.parent = this;
                this.children.Add(child);
            }
        }
        
		//deprecated
        public void expand(List<Tuple<InstructionTree, double>> frontier, Environment goal, List<Tuple<String,String>> matching, double pScore)
        {
            /* Function Description : Expands around this node and adds
             * to the frontier in sorted order. Also programs which are
             * not syntactical possible are trimmed.*/
			return;
        }

		//deprecated
        public void expand()
        {
            /* Function Description : Expands around this node and adds
             * to the frontier in sorted order. Also programs which are
             * not syntactical possible are trimmed.*/
			return;
        }

        public void computeProbability()
        {
            /* FunctionDescription : Computes its bag of feature cost */

            List<String> instW = new List<string>() { this.node.getControllerFunction() };
            List<String> dscps = this.node.getArguments();
            foreach (String dscp in dscps)
                instW.Add(dscp);

            foreach (String word1 in InstructionTree.clsW)
            {
                foreach (String word2 in instW)
                {
                    foreach (Tuple<String, String, int> tmp in Features.treeExp)
                    {
                        if (tmp.Item1.Equals(word1, StringComparison.OrdinalIgnoreCase) && tmp.Item2.Equals(word2, StringComparison.OrdinalIgnoreCase))
                        {
                            score = score + tmp.Item3;
                            break;
                        }
                    }
                }
            }

            double count = instW.Count() * InstructionTree.clsW.Count();
            score = score / count;
        }

        public Tuple<InstructionTree, double> pickOneWithMinimum(double gScore, double[] weights)
        {
            /* Function Description : Picks one with the minimum with average cost from the root
             *  - Total Score of a leaf-node : [ w1* Sum Score of Each Node + w2* Number of Unique Instruction ] /lengthOfPath - w3* Length of Path 
             */

            int numInst = 0, numUnique = 0;
            double nodeScore = 0, dependencyScore=0;
            if (this.children == null || this.children.Count() == 0)
            {
                /* This is on the frontier- Compute its cost by - 
                 * 
                 * w1* (Sum of this node + gScore * depth )/ (depth+1) + w2* Num_Unique_Instruction / depth - w3* Length of Path 
                 */

                List<Instruction> uniqueInstr = new List<Instruction>();
                List<Instruction> total=new List<Instruction>();
                InstructionTree iterator = this;
                while (iterator.node != null)
                {
                    total.Add(iterator.node);
                    numInst++;
                    nodeScore = nodeScore + iterator.score;
                    bool isUnique = true;
                    foreach (Instruction inst in uniqueInstr)
                    {
                        if (iterator.node.getControllerFunction().Equals(inst.getControllerFunction()))
                        {
                            isUnique = false;
                            break;
                        }
                    }
                    if (isUnique)
                        uniqueInstr.Add(iterator.node);
                    iterator = iterator.parent;
                }

                total.Reverse();
                for (int i = 0; i < total.Count() - 1; i++)
                {
                    foreach (Tuple<Instruction, Instruction, int> tmp in Features.dependent)
                    {
                        if (tmp.Item1.compare(total[i + 1]) && tmp.Item2.compare(total[i]))
                        {
                            dependencyScore = dependencyScore + tmp.Item3;
                        }
                    }
                }

                if (total.Count() > 1)
                    dependencyScore = dependencyScore / (double)(total.Count()-1);

                numUnique = uniqueInstr.Count();
                if (numInst > 0)
                    nodeScore = nodeScore / (double)(numInst);

                double avgNumUnique = 0;
                if (numInst > 0)
                    avgNumUnique = numUnique/(double)(numInst);

                gScore = weights[2] * nodeScore + weights[3] * avgNumUnique + weights[4] * numInst + weights[5]*dependencyScore;

                return new Tuple<InstructionTree, double>(this, gScore);
            }

            double best = Double.NegativeInfinity;
            InstructionTree bestChild = null;
            foreach (InstructionTree child in this.children)
            {
                Tuple<InstructionTree, double> result = child.pickOneWithMinimum(gScore, weights);
                if (result.Item2 > best)
                {
                    best = result.Item2;
                    bestChild = result.Item1;
                }
            }
            return new Tuple<InstructionTree, double>(bestChild, best);
        }

        public static List<Instruction> findBestAndExpand(InstructionTree root, double[] weights, List<String> clsW)
        {
            /* Function Description : Finds the best node and expands it. Keeps doing it 
             * until we stop getting good nodes or we have done it good number of time */

            int maxIter = (int)weights[0];
            double threshold = weights[1];
            InstructionTree.clsW = clsW;
            for (int iter = 0; iter < maxIter; iter++)
            {
                Tuple<InstructionTree, double> best = root.pickOneWithMinimum(0, weights);
                if (best.Item2 > threshold)
                    best.Item1.expand();
                else break;
            }

            //Find the best path
            Tuple<InstructionTree, double> optimal = root.pickOneWithMinimum(0, weights);
            InstructionTree iterator = optimal.Item1;
            List<Instruction> output = new List<Instruction>();
            while (iterator.node != null)
            {
                output.Add(iterator.node);
                iterator = iterator.parent;
            }

            //Reverse output
            output.Reverse();
            return output;
        }

        public void free()
        {
            /* Function Description : Frees space of this node and its sub-tree*/
            if (this.children != null)
            {
                foreach (InstructionTree child in this.children)
                    child.free();
                this.children.Clear();
                this.children = null;
            }
            this.parent = null;
            this.node = null;
            this.env = null;
        }
    }
}
