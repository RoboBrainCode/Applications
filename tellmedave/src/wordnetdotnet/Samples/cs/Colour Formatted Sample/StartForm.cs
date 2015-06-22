///*
// * This file is a part of the WordNet.Net open source project.
// * 
// * Copyright (C) 2005 Malcolm Crowe, Troy Simpson 
// * 
// * Project Home: http://www.ebswift.com
// *
// * This library is free software; you can redistribute it and/or
// * modify it under the terms of the GNU Lesser General Public
// * License as published by the Free Software Foundation; either
// * version 2.1 of the License, or (at your option) any later version.
// *
// * This library is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// * Lesser General Public License for more details.
// *
// * You should have received a copy of the GNU Lesser General Public
// * License along with this library; if not, write to the Free Software
// * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// * 
// * */

using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using WordNetClasses; //.WN;
using Microsoft.VisualBasic;
using System.Collections;
using System.Drawing;

namespace wnb
{

	public class StartForm : System.Windows.Forms.Form
	{

		internal System.Windows.Forms.MenuItem mnuFile;
		internal System.Windows.Forms.Button btnAdv;
		internal System.Windows.Forms.Button btnNoun;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.MenuItem mnuHelp;
		internal System.Windows.Forms.Button btnAdj;
		internal System.Windows.Forms.Label lblSearchInfo;
		internal System.Windows.Forms.MainMenu MainMenu1;
		internal System.Windows.Forms.MenuItem mnuShowGloss;
		internal System.Windows.Forms.MenuItem mnuExit;
		internal System.Windows.Forms.MenuItem mnuLGPL;
		internal System.Windows.Forms.MenuItem mnuOptions;
		private System.Windows.Forms.Button withEventsField_btnSearch;
		internal System.Windows.Forms.Button btnSearch {
			get { return withEventsField_btnSearch; }
			set {
				if (withEventsField_btnSearch != null) {
					withEventsField_btnSearch.Click -= btnSearch_Click;
				}
				withEventsField_btnSearch = value;
				if (withEventsField_btnSearch != null) {
					withEventsField_btnSearch.Click += btnSearch_Click;
				}
			}
		}
		internal System.Windows.Forms.MenuItem mnuShowHelp;
		internal System.Windows.Forms.MenuItem mnuWordWrap;
		internal System.Windows.Forms.MenuItem mnuAdvancedOptions;
		internal System.Windows.Forms.MenuItem mnuSaveDisplay;
		internal System.Windows.Forms.TextBox txtSenses;
		internal System.Windows.Forms.Label Label1;
		private System.Windows.Forms.TextBox withEventsField_txtSearchWord;
		internal System.Windows.Forms.TextBox txtSearchWord {
			get { return withEventsField_txtSearchWord; }
			set {
				if (withEventsField_txtSearchWord != null) {
					withEventsField_txtSearchWord.KeyDown -= txtSearchWord_KeyDown;
				}
				withEventsField_txtSearchWord = value;
				if (withEventsField_txtSearchWord != null) {
					withEventsField_txtSearchWord.KeyDown += txtSearchWord_KeyDown;
				}
			}
		}
		private System.Windows.Forms.MenuItem withEventsField_mnuWordNetLicense;
		internal System.Windows.Forms.MenuItem mnuWordNetLicense {
			get { return withEventsField_mnuWordNetLicense; }
			set {
				if (withEventsField_mnuWordNetLicense != null) {
					withEventsField_mnuWordNetLicense.Click -= mnuWordNetLicense_Click;
				}
				withEventsField_mnuWordNetLicense = value;
				if (withEventsField_mnuWordNetLicense != null) {
					withEventsField_mnuWordNetLicense.Click += mnuWordNetLicense_Click;
				}
			}
		}
		internal System.Windows.Forms.Button btnOverview;
		internal System.Windows.Forms.Button btnVerb;
		private System.Windows.Forms.MenuItem mnuHistory;
		internal System.Windows.Forms.MenuItem mnuClearDisplay;
		internal System.Windows.Forms.SaveFileDialog SaveFileDialog1;

		internal System.Windows.Forms.MenuItem MenuItem17;
		#region "FormVariables"
		private AdvancedOptions f3;
		private static string dictpath = "C:\\Program Files\\WordNet\\3.0\\dict\\";
		private WordNetClasses.WN wnc = new WordNetClasses.WN(dictpath);
		internal WordNetControls.WordNetColourFormat wnColour;

		private object pbobject = new object();
		#endregion

		#region " Windows Form Designer generated code "

		public StartForm() : base()
		{
			Load += Form1_Load;

			//This call is required by the Windows Form Designer.
			InitializeComponent();

			//Add any initialization after the InitializeComponent() call

			wnColour.CanvasNavigating += CanvasNavigating;
			wnColour.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

			//            txtOutput.Anchor = Anchor.Top Or Anchor.Left Or Anchor.Bottom Or Anchor.Right
			f3 = new AdvancedOptions();
			mnuSaveDisplay.Click += mnuSaveDisplay_Click;
			mnuClearDisplay.Click += mnuClearDisplay_Click;
			mnuExit.Click += mnuExit_Click;
			mnuWordWrap.Click += mnuWordWrap_Click;
			mnuShowHelp.Click += mnuShowHelp_Click;
			mnuShowGloss.Click += mnuShowGloss_Click;
			mnuAdvancedOptions.Click += mnuAdvancedOptions_Click;
			mnuWordNetLicense.Click += mnuWordNetLicense_Click;
			mnuLGPL.Click += mnuLGPL_Click;
			btnOverview.Click += btnOverview_Click;
			btnNoun.Click += btnWordType_Click;
			btnVerb.Click += btnWordType_Click;
			btnAdj.Click += btnWordType_Click;
			btnAdv.Click += btnWordType_Click;
		}

		//Form overrides dispose to clean up the component list.
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if ((components != null)) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		//Required by the Windows Form Designer

		private System.ComponentModel.IContainer components;
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.  
		//Do not modify it using the code editor.
		internal System.Windows.Forms.Panel Panel1;
		internal System.Windows.Forms.StatusBar StatusBar1;
		internal System.Windows.Forms.Panel Panel3;
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MenuItem17 = new System.Windows.Forms.MenuItem();
			this.SaveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.mnuClearDisplay = new System.Windows.Forms.MenuItem();
			this.mnuHistory = new System.Windows.Forms.MenuItem();
			this.btnVerb = new System.Windows.Forms.Button();
			this.btnOverview = new System.Windows.Forms.Button();
			this.mnuWordNetLicense = new System.Windows.Forms.MenuItem();
			this.txtSearchWord = new System.Windows.Forms.TextBox();
			this.Label1 = new System.Windows.Forms.Label();
			this.txtSenses = new System.Windows.Forms.TextBox();
			this.mnuSaveDisplay = new System.Windows.Forms.MenuItem();
			this.mnuAdvancedOptions = new System.Windows.Forms.MenuItem();
			this.mnuWordWrap = new System.Windows.Forms.MenuItem();
			this.mnuShowHelp = new System.Windows.Forms.MenuItem();
			this.btnSearch = new System.Windows.Forms.Button();
			this.mnuOptions = new System.Windows.Forms.MenuItem();
			this.mnuShowGloss = new System.Windows.Forms.MenuItem();
			this.mnuLGPL = new System.Windows.Forms.MenuItem();
			this.mnuExit = new System.Windows.Forms.MenuItem();
			this.MainMenu1 = new System.Windows.Forms.MainMenu(this.components);
			this.mnuFile = new System.Windows.Forms.MenuItem();
			this.mnuHelp = new System.Windows.Forms.MenuItem();
			this.lblSearchInfo = new System.Windows.Forms.Label();
			this.btnAdj = new System.Windows.Forms.Button();
			this.Label3 = new System.Windows.Forms.Label();
			this.btnNoun = new System.Windows.Forms.Button();
			this.btnAdv = new System.Windows.Forms.Button();
			this.Panel1 = new System.Windows.Forms.Panel();
			this.Panel3 = new System.Windows.Forms.Panel();
			this.wnColour = new WordNetControls.WordNetColourFormat();
			this.StatusBar1 = new System.Windows.Forms.StatusBar();
			this.Panel1.SuspendLayout();
			this.Panel3.SuspendLayout();
			this.SuspendLayout();
			//
			//MenuItem17
			//
			this.MenuItem17.Index = 1;
			this.MenuItem17.Text = "-";
			//
			//SaveFileDialog1
			//
			this.SaveFileDialog1.Filter = "Text files (*.txt)|*.txt";
			//
			//mnuClearDisplay
			//
			this.mnuClearDisplay.Index = 1;
			this.mnuClearDisplay.Text = "Clear Current Display";
			//
			//mnuHistory
			//
			this.mnuHistory.Index = -1;
			this.mnuHistory.Text = "";
			//
			//btnVerb
			//
			this.btnVerb.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnVerb.Location = new System.Drawing.Point(350, 36);
			this.btnVerb.Name = "btnVerb";
			this.btnVerb.Size = new System.Drawing.Size(40, 18);
			this.btnVerb.TabIndex = 4;
			this.btnVerb.Text = "Verb";
			this.btnVerb.Visible = false;
			//
			//btnOverview
			//
			this.btnOverview.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOverview.Location = new System.Drawing.Point(223, 36);
			this.btnOverview.Name = "btnOverview";
			this.btnOverview.Size = new System.Drawing.Size(75, 18);
			this.btnOverview.TabIndex = 15;
			this.btnOverview.Text = "Overview";
			this.btnOverview.Visible = false;
			//
			//mnuWordNetLicense
			//
			this.mnuWordNetLicense.Index = 0;
			this.mnuWordNetLicense.Text = "WordNet License (Princeton)";
			//
			//txtSearchWord
			//
			this.txtSearchWord.AcceptsReturn = true;
			this.txtSearchWord.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.txtSearchWord.Location = new System.Drawing.Point(80, 6);
			this.txtSearchWord.Name = "txtSearchWord";
			this.txtSearchWord.Size = new System.Drawing.Size(216, 21);
			this.txtSearchWord.TabIndex = 1;
			//
			//Label1
			//
			this.Label1.Location = new System.Drawing.Point(0, 6);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(80, 19);
			this.Label1.TabIndex = 0;
			this.Label1.Text = "Search Word:";
			//
			//txtSenses
			//
			this.txtSenses.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.txtSenses.Location = new System.Drawing.Point(544, 6);
			this.txtSenses.Name = "txtSenses";
			this.txtSenses.Size = new System.Drawing.Size(40, 21);
			this.txtSenses.TabIndex = 8;
			this.txtSenses.Text = "0";
			//
			//mnuSaveDisplay
			//
			this.mnuSaveDisplay.Index = 0;
			this.mnuSaveDisplay.Text = "Save Current Display";
			//
			//mnuAdvancedOptions
			//
			this.mnuAdvancedOptions.Index = 3;
			this.mnuAdvancedOptions.Text = "Advanced search options";
			//
			//mnuWordWrap
			//
			this.mnuWordWrap.Checked = true;
			this.mnuWordWrap.Index = 0;
			this.mnuWordWrap.Text = "Word Wrap";
			//
			//mnuShowHelp
			//
			this.mnuShowHelp.Index = 1;
			this.mnuShowHelp.Text = "Show help with each search";
			//
			//btnSearch
			//
			this.btnSearch.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnSearch.Location = new System.Drawing.Point(304, 6);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(56, 19);
			this.btnSearch.TabIndex = 13;
			this.btnSearch.Text = "Search";
			//
			//mnuOptions
			//
			this.mnuOptions.Index = 1;
			this.mnuOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				this.mnuWordWrap,
				this.mnuShowHelp,
				this.mnuShowGloss,
				this.mnuAdvancedOptions
			});
			this.mnuOptions.Text = "Options";
			//
			//mnuShowGloss
			//
			this.mnuShowGloss.Index = 2;
			this.mnuShowGloss.Text = "Show descriptive gloss";
			//
			//mnuLGPL
			//
			this.mnuLGPL.Index = 2;
			this.mnuLGPL.Text = "License";
			//
			//mnuExit
			//
			this.mnuExit.Index = 2;
			this.mnuExit.Text = "Exit";
			//
			//MainMenu1
			//
			this.MainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				this.mnuFile,
				this.mnuOptions,
				this.mnuHelp
			});
			//
			//mnuFile
			//
			this.mnuFile.Index = 0;
			this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				this.mnuSaveDisplay,
				this.mnuClearDisplay,
				this.mnuExit
			});
			this.mnuFile.Text = "File";
			//
			//mnuHelp
			//
			this.mnuHelp.Index = 2;
			this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
				this.mnuWordNetLicense,
				this.MenuItem17,
				this.mnuLGPL
			});
			this.mnuHelp.Text = "Help";
			//
			//lblSearchInfo
			//
			this.lblSearchInfo.Location = new System.Drawing.Point(2, 40);
			this.lblSearchInfo.Name = "lblSearchInfo";
			this.lblSearchInfo.Size = new System.Drawing.Size(296, 14);
			this.lblSearchInfo.TabIndex = 2;
			//
			//btnAdj
			//
			this.btnAdj.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnAdj.Location = new System.Drawing.Point(396, 36);
			this.btnAdj.Name = "btnAdj";
			this.btnAdj.Size = new System.Drawing.Size(64, 18);
			this.btnAdj.TabIndex = 5;
			this.btnAdj.Text = "Adjective";
			this.btnAdj.Visible = false;
			//
			//Label3
			//
			this.Label3.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.Label3.Location = new System.Drawing.Point(480, 6);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(100, 19);
			this.Label3.TabIndex = 7;
			this.Label3.Text = "Senses:";
			//
			//btnNoun
			//
			this.btnNoun.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnNoun.Location = new System.Drawing.Point(304, 36);
			this.btnNoun.Name = "btnNoun";
			this.btnNoun.Size = new System.Drawing.Size(40, 18);
			this.btnNoun.TabIndex = 3;
			this.btnNoun.Text = "Noun";
			this.btnNoun.Visible = false;
			//
			//btnAdv
			//
			this.btnAdv.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnAdv.Location = new System.Drawing.Point(466, 36);
			this.btnAdv.Name = "btnAdv";
			this.btnAdv.Size = new System.Drawing.Size(48, 18);
			this.btnAdv.TabIndex = 6;
			this.btnAdv.Text = "Adverb";
			this.btnAdv.Visible = false;
			//
			//Panel1
			//
			this.Panel1.Controls.Add(this.btnAdj);
			this.Panel1.Controls.Add(this.btnAdv);
			this.Panel1.Controls.Add(this.txtSenses);
			this.Panel1.Controls.Add(this.btnNoun);
			this.Panel1.Controls.Add(this.Label1);
			this.Panel1.Controls.Add(this.btnSearch);
			this.Panel1.Controls.Add(this.btnVerb);
			this.Panel1.Controls.Add(this.btnOverview);
			this.Panel1.Controls.Add(this.lblSearchInfo);
			this.Panel1.Controls.Add(this.txtSearchWord);
			this.Panel1.Controls.Add(this.Label3);
			this.Panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.Panel1.Location = new System.Drawing.Point(0, 0);
			this.Panel1.Name = "Panel1";
			this.Panel1.Size = new System.Drawing.Size(592, 60);
			this.Panel1.TabIndex = 19;
			//
			//Panel3
			//
			this.Panel3.Controls.Add(this.wnColour);
			this.Panel3.Controls.Add(this.StatusBar1);
			this.Panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Panel3.Location = new System.Drawing.Point(0, 60);
			this.Panel3.Name = "Panel3";
			this.Panel3.Size = new System.Drawing.Size(592, 339);
			this.Panel3.TabIndex = 21;
			//
			//wnColour
			//
			this.wnColour.CSSBody = "font-family:'Verdana';font-size:10pt;";
			this.wnColour.CSSDefinition = "color: #000000; text-decoration: none";
			this.wnColour.CSSHover = "background-color: yellow";
			this.wnColour.CSSLexeme = "color: #0000FF; font-weight:bold; text-decoration: none";
			this.wnColour.CSSPOS = "font-size: 12pt; color: #FFFFFF; font-weight: bold; background-color: #808080";
			this.wnColour.CSSQuote = "color: #800000; text-decoration: none";
			this.wnColour.Location = new System.Drawing.Point(0, 3);
			this.wnColour.Name = "wnColour";
			this.wnColour.Size = new System.Drawing.Size(589, 315);
			this.wnColour.TabIndex = 12;
			//
			//StatusBar1
			//
			this.StatusBar1.Location = new System.Drawing.Point(0, 320);
			this.StatusBar1.Name = "StatusBar1";
			this.StatusBar1.Size = new System.Drawing.Size(592, 19);
			this.StatusBar1.TabIndex = 11;
			this.StatusBar1.Text = "WordNet.Net Colour Formatting Sample";
			//
			//StartForm
			//
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(592, 399);
			this.Controls.Add(this.Panel3);
			this.Controls.Add(this.Panel1);
			this.Menu = this.MainMenu1;
			this.Name = "StartForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "WordNet.Net Colour Formatting Sample";
			this.Panel1.ResumeLayout(false);
			this.Panel1.PerformLayout();
			this.Panel3.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		//ByVal e As System.Windows.Forms.WebBrowserNavigatingEventArgs)
		public void CanvasNavigating(object sender, string url)
		{
			//            Dim tmpstr As String
			//            tmpstr = e.Url.ToString()
			//            tmpstr = Replace(tmpstr, "about:blank", "")
			//            tmpstr = Replace(tmpstr, "about:", "")
			//            If tmpstr = "" Then
			//                Exit Sub
			//            End If

			//            e.Cancel = True

			//            Dim myWriter As New StringWriter
			// Decode the encoded string.

			//            txtSearchWord.Text = Replace(tmpstr, "%20", " ")
			txtSearchWord.Text = url;
			btnSearch_Click(null, null);
		}


		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.DoEvents();
			Application.Run(new StartForm());
		}

		private void Form1_Load(System.Object sender, System.EventArgs e)
		{
			LoadAbout();
		}

		/// <summary>
		/// Loads the 'about' summary text.
		/// </summary>
		private void LoadAbout()
		{
			System.IO.StreamReader myFile = new System.IO.StreamReader(MyPath() + "\\license.txt");
			string mystring = myFile.ReadToEnd();

			myFile.Close();

			mystring = Strings.Replace(mystring, Constants.vbCrLf, "<br>");
			wnColour.Canvas.DocumentText = mystring;

			showFeedback(mystring);
		}

		/// <summary>
		/// Gets the path of the application.
		/// </summary>
		/// <returns>String representing the path of the application.</returns>
		private string MyPath()
		{
			//get the app path
			string fullAppName = Assembly.GetExecutingAssembly().GetName().CodeBase;
			//This strips off the exe name
			string FullAppPath = Path.GetDirectoryName(fullAppName);

			FullAppPath = Strings.Mid(FullAppPath, Strings.Len("file:\\\\"));

			// following is only during testing
			#if DEBUG
			FullAppPath = Strings.Mid(FullAppPath, 1, Strings.InStrRev(FullAppPath, "\\"));
			#endif

			return FullAppPath;
		}

		/// <summary>
		/// This is an overview search - the basis for any advanced search.
		/// </summary>
		private void Overview()
		{
			//overview for 'search'
			string t = null;
			WordNetClasses.WN wnc = new WordNetClasses.WN(dictpath);

			se = null;
			// prevent the output from being overwritten by old results in showresults
			t = txtSearchWord.Text;
			lblSearchInfo.Text = "Searches for " + t + ":";
			lblSearchInfo.Visible = true;
			btnOverview.Visible = false;

			//            txtOutput.Text = ""
			//            txtOutput.Visible = False
			StatusBar1.Text = "Overview of " + t;
			Refresh();

			try {
				bool b = false;
				// sets the visibility of noun, verb, adj, adv when showing buttons for a word

				list = new ArrayList();
				wnc.OverviewFor(t, "noun", ref b, ref bobj2, list);
				btnNoun.Visible = b;

				wnc.OverviewFor(t, "verb", ref b, ref bobj3, list);
				btnVerb.Visible = b;

				wnc.OverviewFor(t, "adj", ref b, ref bobj4, list);
				btnAdj.Visible = b;

				wnc.OverviewFor(t, "adv", ref b, ref bobj5, list);
				btnAdv.Visible = b;

				txtSearchWord.Text = t;
				txtSenses.Text = "0";

				string outstr = "";

				outstr = wnColour.buildContents(list);

				wnColour.Canvas.DocumentText = outstr;
			} catch (Exception ex) {
				MessageBox.Show(ex.Message + Constants.vbCrLf + Constants.vbCrLf + "Princeton's WordNet not pre-installed to default location?");
			}

			FixDisplay(null);
		}


		Wnlib.Search se;
		private void DoSearch(Wnlib.Opt opt)
		{
			if (opt.sch.ptp.mnemonic == "OVERVIEW") {
				Overview();
				return;
			}

			//            txtOutput.Text = ""
			Refresh();

			list = new ArrayList();
			se = new Wnlib.Search(txtSearchWord.Text, true, opt.pos, opt.sch, Int16.Parse(txtSenses.Text));
			int a = se.buf.IndexOf("\\n");
			if ((a >= 0)) {
				if ((a == 0)) {
					se.buf = se.buf.Substring(a + 1);
					a = se.buf.IndexOf("\\n");
				}
				StatusBar1.Text = se.buf.Substring(0, a);
				se.buf = se.buf.Substring(a + 1);
			}
			list.Add(se);
			if ((Wnlib.WNOpt.opt("-h").flag)) {
				help = new Wnlib.WNHelp(opt.sch, opt.pos).help;
			}
			FixDisplay(opt);
		}

		ArrayList list = new ArrayList();

		string help = "";
		/// <summary>
		/// Helper for displaying output and associated housekeeping.
		/// </summary>
		/// <param name="opt"></param>
		public void FixDisplay(Wnlib.Opt opt)
		{
			pbobject = "";
			ShowResults(opt);

			txtSearchWord.Focus();
		}

		/// <summary>
		/// Displays the results of the search.
		/// </summary>
		/// <param name="opt">The opt object holds the user-defined search options</param>
		private void ShowResults(Wnlib.Opt opt)
		{
			string tmpstr = "";

			if (list.Count == 0) {
				showFeedback("Search for " + txtSearchWord.Text + " returned 0 results.");
				return;
			}

			Overview tmptbox = new Overview();

			if ((!object.ReferenceEquals(pbobject.GetType(), tmptbox.GetType()))) {
				Overview tb = new Overview();
				tb.useList(list, help, ref tmpstr);
				if ((help != null) & !string.IsNullOrEmpty(help)) {
					tmpstr = help + Constants.vbCrLf + Constants.vbCrLf + tmpstr;
				}
				tmpstr = Strings.Replace(tmpstr, Constants.vbLf, Constants.vbCrLf);
				tmpstr = Strings.Replace(tmpstr, Constants.vbCrLf, "", 1, 1);
				tmpstr = Strings.Replace(tmpstr, "_", " ");
				showFeedback(tmpstr);

				if (string.IsNullOrEmpty(tmpstr) | tmpstr == "<font color='green'><br />" + Constants.vbCr + " " + txtSearchWord.Text + " has no senses </font>") {
					showFeedback("Search for " + txtSearchWord.Text + " returned 0 results.");
				}
				pbobject = tb;
			}

			txtSearchWord.Focus();

			if ((se != null)) {
				string outstr = "";

				if (se.morphs.Count > 0) {
					// use morphs instead of se
					string wrd = null;

					foreach (string wrd_loopVariable in se.morphs.Keys) {
						wrd = wrd_loopVariable;
						// Build the output with the search results.
						// Node hierarchy is automatically constructed
						list = null;
						list[0] = se.morphs[wrd];
						outstr = wnColour.buildContents(list);
					}
				} else {
					// there are no morphs - all senses exist in se
					// Build the output with the search results.
					// Node hierarchy is automatically constructed
					list[0] = se;
					outstr = wnColour.buildContents(list);
				}

				wnColour.Canvas.DocumentText = outstr;
			}
		}

		public Wnlib.SearchSet bobj2;
		public Wnlib.SearchSet bobj3;
		public Wnlib.SearchSet bobj4;

		public Wnlib.SearchSet bobj5;
		/// <summary>
		/// Perform the overview search.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSearch_Click(System.Object sender, System.EventArgs e)
		{
			Overview();
			txtSearchWord.Focus();
		}

		/// <summary>
		/// When the enter key is pressed in the search text field, perform an overview search.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void txtSearchWord_KeyDown(System.Object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) {
				e.Handled = true;
				btnSearch_Click(null, null);
			}
		}


		ArrayList opts = null;
		/// <summary>
		/// Handles the sense buttons to build and display the appropriate dropdown menu for 
		/// searches on word relationships.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnWordType_Click(System.Object sender, System.EventArgs e)
		{
			Button b = (System.Windows.Forms.Button)sender;
			Wnlib.SearchSet ss = null;
			string btext = b.Text;

			if (btext == "Adjective") {
				btext = "Adj";
			}

			switch (btext) {
				case "Noun":
					ss = (Wnlib.SearchSet)bobj2;

					break;
				case "Verb":
					ss = (Wnlib.SearchSet)bobj3;

					break;
				case "Adj":
					ss = (Wnlib.SearchSet)bobj4;

					break;
				case "Adverb":
					ss = (Wnlib.SearchSet)bobj5;
					break;
			}

			Wnlib.PartOfSpeech pos = Wnlib.PartOfSpeech.of(btext.ToLower());
			int i = 0;
			opts = new ArrayList();
			ContextMenu cm = new ContextMenu();
			ArrayList tmplst = new ArrayList();

			for (i = 0; i <= Wnlib.Opt.Count - 1; i++) {
				Wnlib.Opt opt = Wnlib.Opt.at(i);

				if (ss[opt.sch.ptp.ident] & object.ReferenceEquals(opt.pos, pos)) {
					if (tmplst.IndexOf(opt.label) == -1 & opt.label != "Grep") {
						MenuItem mi = new MenuItem();
						mi.Text = opt.label;
						mi.Click += searchMenu_Click;
						opts.Add(opt);
						cm.MenuItems.Add(mi);

						tmplst.Add(opt.label);
					}
				}
			}
			cm.Show(b.Parent, new Point(b.Left, b.Bottom));
		}

		/// <summary>
		/// Handles all word relationship menu selections.  Performs a relationship search.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void searchMenu_Click(object sender, System.EventArgs e)
		{
			// one of the options for button2_click was selected
			MenuItem mi = (System.Windows.Forms.MenuItem)sender;
			Wnlib.Opt opt = null;
			int i = 0;
			string tmpstr = null;

			//            txtOutput.Text = ""
			tmpstr = mi.Text;
			tmpstr = Strings.Replace(tmpstr, "Syns", "Synonyms");
			tmpstr = Strings.Replace(tmpstr, " x ", " by ");
			tmpstr = Strings.Replace(tmpstr, "Freq:", "Frequency:");
			StatusBar1.Text = tmpstr;
			Refresh();

			for (i = 0; i <= mi.Parent.MenuItems.Count - 1; i++) {
				if (mi.Text == mi.Parent.MenuItems[i].Text) {
					opt = (Wnlib.Opt)opts[i];
				}
			}
			DoSearch(opt);
			btnOverview.Visible = true;

			Refresh();
		}

		/// <summary>
		/// Toggles the wordwrap menu option and sets wrapping on the output text field accordingly.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuWordWrap_Click(System.Object sender, System.EventArgs e)
		{
			MenuItem mi = (System.Windows.Forms.MenuItem)sender;

			mi.Checked = !mi.Checked;
			// TODO: Fix wordwrap for the webbrowser control
//			sender.@checked = !sender.@checked;

			//            txtOutput.WordWrap = sender.checked
			//            showFeedback(txtOutput.Text)
		}

		/// <summary>
		/// Option for controlling whether descriptive help is displayed alongside relationship searches.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuShowHelp_Click(System.Object sender, System.EventArgs e)
		{
			MenuItem mi = (System.Windows.Forms.MenuItem)sender;
			mi.Checked = (!mi.Checked);
			Wnlib.WNOpt.opt("-h").flag = mi.Checked;
		}

		/// <summary>
		/// Toggles whether a glossary is displayed for relationship searches.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuShowGloss_Click(System.Object sender, System.EventArgs e)
		{
			MenuItem mi = (System.Windows.Forms.MenuItem)sender;
			mi.Checked = !mi.Checked;
			Wnlib.WNOpt.opt("-g").flag = !mi.Checked;
		}

		/// <summary>
		/// Re-displays the overview search.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnOverview_Click(System.Object sender, System.EventArgs e)
		{
			Overview();
		}

		/// <summary>
		/// Exits the application.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuExit_Click(System.Object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		/// <summary>
		/// Clears and resets the entire application interface.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuClearDisplay_Click(System.Object sender, System.EventArgs e)
		{
			wnColour.Canvas.DocumentText = "";
			txtSearchWord.Text = "";
			lblSearchInfo.Text = "";
			StatusBar1.Text = "WordNetDT";
			btnNoun.Visible = false;
			btnVerb.Visible = false;
			btnAdj.Visible = false;
			btnAdv.Visible = false;
			btnOverview.Visible = false;
			btnSearch.Visible = true;
		}

		/// <summary>
		/// Displays Princeton's WordNet license.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuWordNetLicense_Click(System.Object sender, System.EventArgs e)
		{
			System.IO.StreamReader myFile = new System.IO.StreamReader(MyPath() + "\\wordnetlicense.txt");
			string mystring = myFile.ReadToEnd();

			myFile.Close();

			showFeedback(mystring);
		}

		/// <summary>
		/// Displays the 'about' text.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuLGPL_Click(System.Object sender, System.EventArgs e)
		{
			LoadAbout();
		}

		/// <summary>
		/// Saves the text in the output field.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuSaveDisplay_Click(System.Object sender, System.EventArgs e)
		{
			SaveFileDialog1.FileName = txtSearchWord.Text;

			if (SaveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				StreamWriter f = new StreamWriter(SaveFileDialog1.FileName, false);

				f.Write(wnColour.Canvas.DocumentText);
				f.Close();
			}
		}

		/// <summary>
		/// Display the passed text in the output textbox and set the focus to the search input field.
		/// </summary>
		/// <param name="mystring">The text to display in the output field</param>
		private void showFeedback(string mystring)
		{
			//            txtOutput.Text = mystring
			txtSearchWord.Focus();
		}

		/// <summary>
		/// Displays the advanced options dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mnuAdvancedOptions_Click(System.Object sender, System.EventArgs e)
		{
			f3.ShowDialog();
		}
	}

	/// <summary>
	/// Displays the basic overview text which is the 'buf' result returned from the WordNet.Net library.
	/// </summary>
	public class Overview
	{
		private ArrayList cont;
		private int totLines;
		private string sw;

		private int helpLines;
		public void usePassage(string passage, ref string tmpstr)
		{
			tmpstr += passage;
		}

		public void useList(ArrayList w, string help, ref string tmpstr)
		{
			cont = new ArrayList();
			totLines = 0;
			sw = null;

			if ((help != null) && !(string.IsNullOrEmpty(help))) {
				usePassage(help, ref tmpstr);
			}
			helpLines = totLines;
			int j = 0;
			while (j < w.Count) {
				Wnlib.Search se = (Wnlib.Search)w[j];
				sw = se.word;
				usePassage(se.buf, ref tmpstr);
				System.Math.Min(System.Threading.Interlocked.Increment(ref j), j - 1);
			}
		}
	}
}
