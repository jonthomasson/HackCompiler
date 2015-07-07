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
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            AnalyzeFile();
        }

        /// <summary>
        /// this will call our top level module that will set up and invoke the other modules.
        /// </summary>
        private void AnalyzeFile()
        {
            //1. call Tokenizer
            var stream = new MemoryStream();
            var xmlTokens = XmlWriter.Create(stream);
            var tokenizer = new JackTokenizer(_fileName);

            xmlTokens.WriteStartElement("tokens"); //<tokens>
            while (tokenizer.HasMoreTokens)
            {
                tokenizer.Advance();

                var sbuilder = new StringBuilder(); //used to hold our output

                lblStatus.Text = "Tokenizing Line: " + sbuilder.Length;
                frmStatus.Refresh();

                if (tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
                {
                    //write xml for identifier
                }
                else if (tokenizer.TokenType == Enums.Enumerations.TokenType.INT_CONST)
                {
                    //write xml for int constant
                }
                else if (tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD)
                {
                    //write xml for keyword
                }
                else if (tokenizer.TokenType == Enums.Enumerations.TokenType.STRING_CONST)
                {
                    //write xml for string constant
                }
                else if (tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL)
                {
                    ////write xml for symbol
                }
            }

            xmlTokens.WriteEndElement();//</tokens>
            xmlTokens.Flush();
            stream.Position = 0; //rewind the stream. This will eventually be the input to the CompilationEngine class.
            //xmlTokens.Close();
            //2. call Parser

            //3. call Code Generation
        }
    }
}
