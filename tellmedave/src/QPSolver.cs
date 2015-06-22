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
using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Services;
using Microsoft.SolverFoundation.Solvers;

namespace ProjectCompton
{
    class QPSolver
    {
        /* Class Description: Solve a QP program by converting to a linear
         * program. The algorithm works as follows, given:
         *  maximize a^T x + x^T b x
         *  Ax >= C
         * define auxillary variable z_ij = x_i x_j
         *   maximize a^T x + b^T z = (a+b)^T [x,z] ; z is row-ordered
         *      Ax >= C
         *      x_i -z_ij  >= 0;  x_j - z_ij >= 0;  z_ij - x_i - x_j  >= -1;  */

        double[] a = null;
        double[,] b = null;
        List<double[]> constraintsLower = null;
        List<double> lower = null; //its dimension must be same as constraints row dimension
		InteriorPointSolverParams ipsParams = null;
		SimplexSolverParams ssParams = null;

        public QPSolver(double[] a, double[,] b, List<double[]> constraintsLower, List<double> lower)
        {
            //Constructor Description: Initialize the QP program
			/*if (b != null) 
				this.toLinearProgram (a, b, constraintsLower, lower);
			else*/
			{
				this.a = a;
				this.b = b;
				this.constraintsLower = constraintsLower;
				this.lower = lower;
				if (Constants.method == Solver.InteriorPoint)
					this.ipsParams = new InteriorPointSolverParams ();
				else if (Constants.method == Solver.Simplex)
					this.ssParams = new SimplexSolverParams ();
			}
        }

        public void destroy()
        {
            /* Function Description: Destroy the data-structures */
            Array.Clear(this.a, 0, this.a.Length);
			if(this.b!=null)
            	Array.Clear(this.b, 0, this.b.Length);
            for (int i = 0; i < this.constraintsLower.Count(); i++)
                Array.Clear(this.constraintsLower[i], 0, this.constraintsLower[i].Length);
            this.constraintsLower.Clear();
            this.lower.Clear();
        }

		public Tuple<double[], double> solve()
		{
			//Function Description: Choose a wrapper for QP solver
			switch (Constants.method) 
			{
				case Solver.InteriorPoint:
					return this.interiorPoint();
				case Solver.Simplex:
					return this.simplex ();
				case Solver.AlgLib:
					return this.minQPNonConvex ();
			}

			throw new ApplicationException ("Unknown Mapping Method "+Constants.method.ToString());
		}

		private void toLinearProgram(double[] a, double[,] b, List<double[]> constraintsLower, List<double> lower)
		{
			/* Function Description: Converts quadratic program to a linear program 
			 * using relaxation method described above */

			int numNewVar = a.Length * a.Length;
			this.a = new double[a.Length + numNewVar]; //expand a vector
			for (int i=0; i<a.Length + numNewVar; i++) 
			{
				if (i < a.Length)
					this.a [i] = a [i];
				else 
					this.a[i] = b[(i-a.Length)/a.Length,(i-a.Length)%a.Length];
			}

			this.constraintsLower = new List<double[]> ();
			this.lower = new List<double> ();

			for (int i=0; i<constraintsLower.Count()+numNewVar; i++) 
			{
				if (i < constraintsLower.Count ()) //expand existing constraints to add 0s for new variables
				{
					double[] constraintRow = Enumerable.Repeat(0.0,a.Length+numNewVar).ToArray();
					for (int j=0; j<a.Length+numNewVar; j++) 
					{
						if (j < a.Length)
							constraintRow [j] = constraintsLower [i] [j];
					}
					this.constraintsLower.Add (constraintRow);
					this.lower.Add (lower [i]);
				}
				else //3*numNewVar more constraints are added
				{
					//work with variable numNewVar = z_pq
					int p = (i -constraintsLower.Count()) / a.Length, q = (i-constraintsLower.Count())%a.Length;
					double[] constraintRow1 = Enumerable.Repeat(0.0,a.Length+numNewVar).ToArray();
					double[] constraintRow2 = Enumerable.Repeat(0.0,a.Length+numNewVar).ToArray();
					double[] constraintRow3 = Enumerable.Repeat(0.0,a.Length+numNewVar).ToArray();

					constraintRow1 [p] = 1; constraintRow1 [a.Length + p * q] = -1;
					constraintRow2 [q] = 1; constraintRow1 [a.Length + p * q] = -1;
					constraintRow3 [p] = - 1; constraintRow3 [q] = - 1; constraintRow1 [a.Length + p * q] = 1;

					this.constraintsLower.Add (constraintRow1);
					this.constraintsLower.Add (constraintRow2);
					this.constraintsLower.Add (constraintRow3);
					this.lower.Add(0);//x_p -z_pq  >= 0
					this.lower.Add(0);//x_q - z_pq >= 0
					this.lower.Add(-1);//z_pq - x_p - x_q  >= -1
				}
			}
		}

        private Tuple<double[], double> interiorPoint()
        {
            /* Function Description: Solves the QP program - 
             * minimize x^Ta + x^Tbx
             * linear constraint over x
             * */

            InteriorPointSolver ips = new InteriorPointSolver();

            //Set the objective function
            int numVar = a.Length;
            int goal;
            ips.AddRow("Goal", out goal);

            int[] variables = new int[numVar];

            for (int i = 0; i < numVar;i++ )
                ips.AddVariable("X" + i, out variables[i]);

            for (int i = 0; i < numVar; i++)
            {
                ips.SetCoefficient(goal, variables[i], a[i]);
                /*for (int j = 0; j < numVar; j++)
                    ips.SetCoefficient(goal, b[i, j], variables[i], variables[j]);*/
            }
            ips.AddGoal(goal, 0, false);

            //Set the constraints
            int numConstraints = this.constraintsLower.Count();
            for (int i = 0; i < numConstraints; i++)
            {
                int constraint;
                ips.AddRow("Constraints" + i, out constraint);
                for (int j = 0; j < numVar; j++)
                    ips.SetCoefficient(constraint, variables[j], (Rational)this.constraintsLower[i][j]);
                ips.SetLowerBound(constraint, (Rational)this.lower[i]);
            }

            /* Solve the program and return the optimum value
             * Return the optimum parameters
             * and also return the solved goal value * */

            ips.Solve(this.ipsParams);
            double[] result = new double[numVar];
            for (int i = 0; i < numVar; i++)
                result[i] = (double)ips.GetValue(variables[i]);

            return new Tuple<double[], double>(result, (double)ips.GetSolutionValue(goal));
        }

        private Tuple<double[], double> simplex()
        {
            /* Function Description: Solves the QP program - 
             * minimize x^Ta
             * linear constraint over x */

            SimplexSolver ss = new SimplexSolver();

            //Set the objective function
            int numVar = a.Length;
            int goal;
            ss.AddRow("Goal", out goal);

            int[] variables = new int[numVar];

            for (int i = 0; i < numVar; i++)
                ss.AddVariable("X" + i, out variables[i]);

            for (int i = 0; i < numVar; i++)
            {
                ss.SetCoefficient(goal, variables[i], a[i]);
                /*for (int j = 0; j < numVar; j++)
                    ips.SetCoefficient(goal, b[i, j], variables[i], variables[j]);*/
            }
            ss.AddGoal(goal, 0, false);

            //Set the constraints
            int numConstraints = this.constraintsLower.Count();
            for (int i = 0; i < numConstraints; i++)
            {
                int constraint;
                ss.AddRow("Constraints" + i, out constraint);
                for (int j = 0; j < numVar; j++)
                    ss.SetCoefficient(constraint, variables[j], (Rational)this.constraintsLower[i][j]);
                ss.SetLowerBound(constraint, (Rational)this.lower[i]);
            }

            /* Solve the program and return the optimum value
             * Return the optimum parameters
             * and also return the solved goal value * */

            ss.Solve(this.ssParams);
            double[] result = new double[numVar];
            for (int i = 0; i < numVar; i++)
                result[i] = (double)ss.GetValue(variables[i]);

			double score = Double.PositiveInfinity;
			//if ((double)ss.GetSolutionValue (goal) != null)
			score = (double)ss.GetSolutionValue (goal);

			return new Tuple<double[], double> (result, score);
        }

		private Tuple<double[], double> minQPNonConvex()
		{
			/* Function Description: Solves using algib QP program which can also handle non-convex programs
			 * minimizes a^Tx + 1/2 x^T B x: since our original problem is max a^Tx + x^T B x 
			 * hence -a and -2B to be used */

			double[] x;
			alglib.minqpstate state;
			alglib.minqpreport rep;

			// create solver, set quadratic/linear terms, constraints
			for (int i=0; i<this.a.Length; i++) 
			{
				this.a [i] = -this.a [i];
				for (int j=0; j<this.a.Length; j++)
					this.b [i, j] = -2*this.b [i, j]; //Changing the matrices -- this is dangerous and should be avoided in future
			}

			alglib.minqpcreate (this.a.Length, out state);
			alglib.minqpsetlinearterm (state, this.a); //-a
			state.innerobj.a.a = this.b; //out of desperation
			//alglib.minqpsetquadraticterm (state, this.b); //-2B

			alglib.minqpsetstartingpoint(state, Enumerable.Repeat(0.0,this.a.Length).ToArray()); //[x_ij] -- for each i; set it to some j

			double[,] c = new double[this.lower.Count (), this.a.Length + 1];
			int[] ct = new int[this.lower.Count()];
			for (int k=0; k<this.constraintsLower.Count(); k++) 
			{
				for(int i=0; i<this.a.Length;i++)
					c[k,i] = this.constraintsLower[k][i];
				c [k, this.a.Length] = this.lower [k];
				ct [k] = 1;
			}

			alglib.minqpsetlc (state, c, ct);

			// Set scale of the parameters.
			alglib.minqpsetscale(state, Enumerable.Repeat(0.1,this.a.Length).ToArray());

			// solve problem with BLEIC-QP solver.
			// default stopping criteria are used.
			alglib.minqpsetalgobleic(state, 0.0, 0.0, 0.0, 0);
			alglib.minqpoptimize(state);
			alglib.minqpresults(state, out x, out rep);

			double obj = 0; //we also want the objective function value for the given map
			for (int i=0; i<this.a.Length; i++) 
			{
				obj = obj - this.a [i] * x [i];
				for (int j=0; j<this.a.Length; j++)
					obj = obj - 0.5* this.b [i, j] * x [i] * x [j];
			}
			//System.Console.WriteLine("{0}", alglib.ap.format(x,2)); // EXPECTED: [2,2]
			//System.Console.WriteLine("{0}", rep.terminationtype); // EXPECTED: -4

			return new Tuple<double[], double> (x, obj);
		}

		private Tuple<double[], double> L_BLGS2()
		{
			/* Function Description: Solves using LBLGS algorithm using Langrangian
			 * to handle the constraints. Use the alglib library */

			double[] x = Enumerable.Repeat (0.0, this.a.Length + this.constraintsLower.Count ()).ToArray ();
			double epsg = 0.0000000001;
			double epsf = 0;
			double epsx = 0;
			int maxits = 100;
			alglib.minlbfgsstate state;
			alglib.minlbfgsreport rep;


			alglib.minlbfgscreate(1, x, out state);
			alglib.minlbfgssetcond(state, epsg, epsf, epsx, maxits);
			alglib.minlbfgsoptimize(state, func_gradient, null, null);
			alglib.minlbfgsresults(state, out x, out rep);

			//x has the value --- we only take the first a.Length values since remaining are Langrange multipliers
			double[] map = x.Take(this.a.Length).ToArray();
			double obj = 0; //we also want the objective function value for the given map
			for (int i=0; i<this.a.Length; i++) 
			{
				obj = obj + this.a [i] * map [i];
				for (int j=0; j<this.a.Length; j++)
					obj = obj + this.b [i, j] * map [i] * map [j];
			}

			//System.Console.WriteLine("{0}", rep.terminationtype); // EXPECTED: 4
			//System.Console.WriteLine("{0}", alglib.ap.format(x,2)); // EXPECTED: [-3,3]
			//System.Console.ReadLine();

			return new Tuple<double[], double> (map, obj);
		}

		private void func_gradient(double[] y, ref double func, double[] grad, object obj)
		{
			/* Function Description: Function with its lagrangian is y=[x,\lambda]
			 *   a_i y_i + \sum_ij b_ij y_i y_j + \sum_k \lambda_k (\sum_j c_kj y_j -d_k) 
			 * gradient is: g_i = a_i + \sum_j (b_ij + b_ji) y_j  + \sum_k \lambda_k c_ki  
			 * 		        g_k = \sum_j c_kj y_j -d_k
			 * this callback calculates function and its gradient */
		
			for (int i=0; i<this.a.Length; i++) 
			{
				func = func + this.a [i] * y [i]; 
				for (int j=0; j<this.a.Length; j++)
					func = func + this.b [i, j] * y [i] * y [j];

				double grad_i = this.a [i];
				for (int j=0; j<this.a.Length; j++)
					grad_i = grad_i + (this.b [i, j] + this.b [j, i]) * y [j];

				for (int k=0; k<this.constraintsLower.Count(); k++)
					grad_i = grad_i + y [this.a.Length + k] * this.constraintsLower [k] [i];

				grad [i] = grad_i;
			}

			//add Langrange multiplier terms
			for (int k=0; k<this.constraintsLower.Count(); k++) 
			{
				double cy = 0;
				for (int j=0; j<this.a.Length; j++) 
						cy = cy + this.constraintsLower[k][j]*y[j];

				grad [this.a.Length + k] = cy - this.lower [k];
				func = func + y [this.a.Length + k] * (cy - this.lower [k]);
			}

		}
    }
}
