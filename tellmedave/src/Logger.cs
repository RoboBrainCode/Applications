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
using System.IO;
using System.Threading.Tasks;

namespace ProjectCompton
{
    class Logger
    {
        /*Class Description : Creates log*/

        int pad, bufSize=100, priority = 0; //[ 0 : High Priority, 1 : Low Priority]
		int whichveil = 1;
        StringBuilder buffer = null;

        public Logger()
        {
            this.pad = (System.IO.Directory.GetFiles(Constants.rootPath + "Log").Length/2) + 1;
			this.buffer = new StringBuilder ();
            createLogFiles();
        }

        public void createLogFiles()
        {
            /*Function Description : creates two file - for writing data and for error */
            this.initiate();
        }

        public void initiate()
        {
            /*Function Description : Initializes html file*/
            String style = "<style type='text/css'>"
                + "#environment { display:block;}"
                + "#object { display:block; border:1px solid black; }"
                + "#rbt{display:block;}"
                + "#clause {display:block;}"
                + "</style>";
            String javascript = "<script>"
                                + "function show(e)"
                                + "{"
                                + "var parent = e.parentNode;"
                                + "var children = parent.childNodes;"
                                + "if(children[children.length-1].style.display == 'block')"
                                + "{"
                                + "children[children.length-1].style.display = 'none';"
                                + "}"
                                + "else"
                                + "{"
                                + "children[children.length-1].style.display = 'block';"
                                + "}"
                                + "}"
                                + "</script>";

            this.writeToFile("<html>" + style + javascript + "<body style='padding-left:20%; padding-right:20%'>");
            this.writeToErrFile("<html><body>");
        }

        public void close()
        {
            /*Function Description : Closes html file*/
            this.flush();
            this.writeToFile("</html></body>");
            this.writeToErrFile("</html></body>");
        }

        public void deleteLogFiles()
        {
            /*Function Description : deletes the two file*/
            System.IO.File.Delete(Constants.rootPath + "Log/output" + this.pad + ".html");
            System.IO.File.Delete(Constants.rootPath + "Log/error" + this.pad + ".html");
        }

        public void writeToFile(String data)
        {
            /*Function Description : writes to file*/
            if (this.priority > Constants.loggingLevel || Constants.disablelog)
                return;
            if (buffer.Length + data.Count() > this.bufSize)
            {
                StreamWriter tw = new StreamWriter(Constants.rootPath + "Log/output" + this.pad + ".html", true);
                tw.Write(buffer.ToString()+data);
                tw.Close();
                buffer.Clear();
            }
            else
                buffer = buffer.Append(data);
        }

        public void writeToErrFile(String data)
        {
            /*Function Description : writes to file */
			//return;
            StreamWriter tw = new StreamWriter(Constants.rootPath + "Log/error" + this.pad + ".html", true);
            tw.Write(data);
            tw.Close();
        }

        public void writeToParserData(String data)
        {
            //Function Description : Write data to the parser file
            StreamWriter parserXML = new StreamWriter(Constants.rootPath + "Log/" + "parser_" + this.pad + ".xml", true);
			//StreamWriter parserXML = new StreamWriter(Constants.rootPath + "VEIL/" + "veil_" + this.whichveil + ".xml", true);
            parserXML.WriteLine(data);
            parserXML.Flush();
            parserXML.Close();
        }

		public void incrementParserFile()
		{
			this.whichveil++;
		}

        public void flush()
        {
            /* Function Description : Flushes the buffer */
            if (buffer.Length > 0)
            {
                StreamWriter tw = new StreamWriter(Constants.rootPath + "Log/output" + this.pad + ".html", true);
                tw.Write(buffer);
                tw.Close();
            }
        }

        public void setLowPriority()
        {
            this.priority = 1;
        }

        public void setHighPriority()
        {
            this.priority = 0;
        }
    }
}
