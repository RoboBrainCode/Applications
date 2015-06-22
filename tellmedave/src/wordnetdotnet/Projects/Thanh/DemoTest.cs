using System;
using System.Diagnostics;
using System.Text.RegularExpressions ;

namespace WordsMatching
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class DemoTest
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
			// TDMS 21 Sept 2005 - added dictionary path
			Wnlib.WNCommon.path = @"/home/robo328b/dipendra_tellmedave/dict/";//@"/home/dipendra/Research/ProjectCompton/wordnetdotnet/dict/";//"C:\\Users\\AirRobot\\Desktop\\wordnetdotnet\\dict\\";
		}

        public DemoTest() //NUnit missing! 
		{
            
		}

	}
}
