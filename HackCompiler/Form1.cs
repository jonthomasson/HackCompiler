using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HackCompiler.Modules;
using System.Xml;
using System.IO;

namespace HackCompiler
{
    public partial class Form1 : Form
    {
        private string _fileName = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            

            dlg.Filter = "Jack Program (*.jack)|*.jack";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _fileName = dlg.FileName;
                lblStatus.Text = "File Loaded Successfully: " + _fileName;
                
                frmStatus.Refresh();

                ReadSourceFile();
            }
        }

        private void ReadSourceFile()
        {
            rtbErrors.Text = "";
            var sourceFile = System.IO.File.ReadAllText(_fileName);

            rtbSource.Text = sourceFile;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Compile();
        }

        private void Compile()
        {
            SaveSourceFileChanges();

            ReadSourceFile();

            AnalyzeFile();
        }

        //private async void AnalyzeFileAsync()
        //{
        //    await Task.Delay(1000);

        //    AnalyzeFile();
        //}

        private void SaveSourceFileChanges()
        {
            rtbSource.SaveFile(_fileName, RichTextBoxStreamType.PlainText);
        }

        /// <summary>
        /// this will call our top level module that will set up and invoke the other modules.
        /// </summary>
        private void AnalyzeFile()
        {
            //creating a compilationEngine class for this file.
            //the CompilationEngine will then take care of creating the JackTokenizer and compiling the given class

            var compileEngine = new CompilationEngine(_fileName, _fileName.Replace(".jack", ".xml"));

            rtbDestination.Text = compileEngine.Xml ;

            //check for errors
            if (compileEngine.HasErrors)
            {
                var errorTokens = from t in compileEngine.Tokens
                                  where !string.IsNullOrEmpty(t.Error)
                                  select t;

                foreach (var error in errorTokens)
                {
                    //Select the line from it's number
                    var errorLine = error.LineNo - 1;

                    var startIndex = rtbSource.GetFirstCharIndexFromLine(errorLine) + error.CharNo;

                    var currentChar = rtbSource.GetCharFromPosition(rtbSource.GetPositionFromCharIndex(startIndex)).ToString();
                    var length = 0;
                    var currentIndex = startIndex;
                    while(currentChar != "\n"){
                        currentIndex++;
                        length++;
                        currentChar = rtbSource.GetCharFromPosition(rtbSource.GetPositionFromCharIndex(currentIndex)).ToString();
                    }

                    //rtbSource.Select(rtbSource.GetFirstCharIndexFromLine(errorLine), currentIndex - rtbSource.GetFirstCharIndexFromLine(errorLine));
                    //rtbSource.SelectedText += " error: " + error.Error;

                    rtbSource.Select(startIndex, 1);

                    //Set the selected text fore and background color
                    rtbSource.SelectionColor = System.Drawing.Color.Black;
                    rtbSource.SelectionBackColor = System.Drawing.Color.Red;

                    //rtbSource.Select(currentIndex, error.Error.Length + 8);

                    ////Set the selected text fore and background color
                    //rtbSource.SelectionColor = System.Drawing.Color.Black;
                    //rtbSource.SelectionBackColor = System.Drawing.Color.Orange;

                    rtbSource.DeselectAll();
                }

                lblStatus.Text = "Compilation failed! Error count = " + errorTokens.Count() + ";  line: " + errorTokens.First().LineNo + "; char: " + errorTokens.First().CharNo + "; Error Message: " + errorTokens.First().Error;
                rtbErrors.Text = "Compilation failed! Error count = " + errorTokens.Count() + ";  line: " + errorTokens.First().LineNo + "; char: " + errorTokens.First().CharNo + "; Error Message: " + errorTokens.First().Error;
                rtbErrors.Text += Environment.NewLine + "Stack Trace: " +  errorTokens.First().StackTrace;
                frmStatus.Refresh();
            }

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //open save file dialog
            var savefile = new SaveFileDialog();
            // set a default file name
            var fileName = System.IO.Path.GetFileName(_fileName).Replace("jack", "xml");
            savefile.FileName = fileName;

            // set filters - this can be done in properties as well
            savefile.Filter = "xml files (*.xml)|*.xml*";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                rtbDestination.SaveFile(savefile.FileName, RichTextBoxStreamType.PlainText);

                lblStatus.Text = "File " + savefile.FileName + " Saved Successfully.";
                frmStatus.Refresh();
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            Compile();
        }
    }
}
