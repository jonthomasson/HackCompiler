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
                    xmlTokens.WriteStartElement("identifier"); //<identifier>
                    xmlTokens.WriteString(tokenizer.Identifier());
                    xmlTokens.WriteEndElement();
                }
                else if (tokenizer.TokenType == Enums.Enumerations.TokenType.INT_CONST)
                {
                    //write xml for int constant
                    xmlTokens.WriteStartElement("integerConstant"); //<integerConstant>
                    xmlTokens.WriteString(tokenizer.IntVal().ToString());
                    xmlTokens.WriteEndElement();
                }
                else if (tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD)
                {
                    //write xml for keyword
                    xmlTokens.WriteStartElement("keyword"); //<keyword>
                    xmlTokens.WriteString(tokenizer.KeyWord());
                    xmlTokens.WriteEndElement();
                }
                else if (tokenizer.TokenType == Enums.Enumerations.TokenType.STRING_CONST)
                {
                    //write xml for string constant
                    xmlTokens.WriteStartElement("stringConstant"); //<stringConstant>
                    xmlTokens.WriteString(tokenizer.StringVal());
                    xmlTokens.WriteEndElement();
                }
                else if (tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL)
                {
                    ////write xml for symbol
                    xmlTokens.WriteStartElement("symbol"); //<stringConstant>
                    xmlTokens.WriteString(tokenizer.Symbol());
                    xmlTokens.WriteEndElement();
                }
            }

            xmlTokens.WriteEndElement();//</tokens>
            xmlTokens.Flush();
            stream.Position = 0; //rewind the stream. This will eventually be the input to the CompilationEngine class.
            string xml = new StreamReader(stream).ReadToEnd();
            //xmlTokens.Close();
            //2. call Parser
            rtbDestination.Text = xml;
            //xmlTokens.Close();
            //3. call Code Generation
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
    }
}
