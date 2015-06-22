using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using WordsMatching;
using System.Diagnostics;

namespace ProjectCompton
{
	public enum OpMode
	{
		Offline,	//offline inference and learning on a dataset using cross-validation
		Online,	    //online inference and learning
		Transfer    //inference on a new dataset with new actions and environment
	}

	public enum Solver
	{
		InteriorPoint,
		Simplex,
		AlgLib
	}

	public enum CrossValidationScheme
	{
		Environment,
		Task
	}

    class Constants
    {
        /*Class Description : Describes various constants */

		//Paths
		internal static string path = Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName;
		internal static string rootPath = path+"/src/";
		internal static string UBLPath = path+"/src/LambdaExpression/";
		internal static string weblink = path+"/Dataset/webfiles/";
		internal static string dataFileName ="data_Feb-18-2015.xml"; 
		internal static string dataFolder = path+"/Dataset/VEIL500";
        internal static string cygwinPath = @"C:/cygwin64/bin/";  //If you are using windows, define cygwin path to run linux style commands
		internal static string ublPath = Constants.rootPath + "Baselines/UBL/UBL/experiments/new/data/en/run-0/fold-0/";
		internal static string gizaPath = Constants.rootPath + "Baselines/UBL/giza-pp/GIZA++-v2/";

		/* Advanced Settings: Do not touch anything below this line if you are not sure what you are doing.
		 * Please contact the developers for more help. */

		//Data Description
		internal static String[] scenarios = new String[2] { "kitchen", "livingRoom" };
        internal static bool analyze = false;
		internal static int numEnvironment = 6;
		internal static int numDataPerEnvironment = 10;
		internal static double wordnetDistThreshold = 0.85; 
		internal static int topKParse = 1;
        internal static int version = 3;                 //Code version
        internal static int loggingLevel = 1;            //[-1 : no log, 0 : only important information, 1: all information]
        internal static bool usingLinux = true;          //Will use different files if OS is Linux if this flag is turned  on. By default the OS is Windows
        internal static double epsilon = 0.001;          //used to prevent dividing by 0 situation

		//Algorithm Flags
		internal static OpMode opmode = OpMode.Online;
		internal static Solver method = Solver.InteriorPoint;
		internal static CrossValidationScheme scheme = CrossValidationScheme.Environment;

        //Cache Flags
        internal static bool cacheReadQP = false;            //reads solution of QP program from xml file
		internal static bool cacheWriteQP = false;           //writes solution of QP program into a file
		internal static bool cacheReadParser = true;         //reads parse output from a parser xml file
		internal static bool cacheReadUBLParser = false;     //reads UBL parse output from a parser file
		internal static bool cacheReadWeights = false;       //reads weights from a learning file
		internal static bool cacheBootStrapPlanOut = true;     //bootstraps the symbolic planner results from cache
		internal static bool cacheBootStrapSentenceSim = false;//true; //bootstrap sentence similarity

		internal static bool allow = false;
		internal static bool disablelog = false;
	}
}
