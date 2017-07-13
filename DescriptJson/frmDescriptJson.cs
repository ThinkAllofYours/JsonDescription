using DescriptJson.CodeWriters_to_Str;
using System;
using System.Text;
using System.Windows.Forms;
using Xamasoft.JsonClassGenerator;
using Xamasoft.JsonClassGenerator.CodeWriters;

namespace DescriptJson
{
    public partial class frmDescriptJson : Form
    {
        public frmDescriptJson()
        {
            InitializeComponent();
        }

        private void frmDescriptJson_Load(object sender, EventArgs e)
        {
            cmbLanguage.Items.AddRange(CodeWriters);
        }

        private void frmDescriptJson_FormClosing(object sender, FormClosingEventArgs e) { }

        #region "Variable declaration"

        private readonly static ICodeWriter[] CodeWriters = new ICodeWriter[] {
            new CSharpCodeWriter(),
            new TypeScriptCodeWriter(),
        };

        #endregion



        #region "Control Event"

        private void btnGenerator_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var gen = Prepare();
            if (gen == null) return;
            try
            {
                gen.GenerateClasses();
                edtJsonDescription.Text = GenerateDescription(gen).ToString();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(this, "Unable to generate the code: " + ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor.Current = Cursors.Default;
        }

        private void messageTimer_Tick(object sender, EventArgs e)
        {
            messageTimer.Stop();
        }

        #endregion



        #region "User-defined functions"
        private StringBuilder GenerateDescription(JsonClassGenerator generator)
        {
            GenerateDescription _jsonDescriptionGenerator = new GenerateDescription();
            cloneGenerator(generator, _jsonDescriptionGenerator);
            string codeWriterName = _jsonDescriptionGenerator.CodeWriter.DisplayName;
            if (codeWriterName == "C#") _jsonDescriptionGenerator.CodeWriter = new CSharpCodeWriterToText();
            else if(codeWriterName == "TypeScript") _jsonDescriptionGenerator.CodeWriter = new TypeScriptCodeWriterToText();

            StringBuilder _sb = new StringBuilder();
            _jsonDescriptionGenerator.WriteDescriptionToStringBuilder(_sb, _jsonDescriptionGenerator.Types);
            return _sb; 
        }

        public void cloneGenerator(JsonClassGenerator original, GenerateDescription clone)
        {
            clone.Namespace = original.Namespace;
            clone.UseProperties = original.UseProperties;
            clone.InternalVisibility = original.InternalVisibility;
            clone.ExplicitDeserialization = original.ExplicitDeserialization;
            clone.NoHelperClass = original.NoHelperClass;
            clone.MainClass = original.MainClass;
            clone.UsePascalCase = original.UsePascalCase;
            clone.UseNestedClasses = original.UseNestedClasses;
            clone.ApplyObfuscationAttributes = original.ApplyObfuscationAttributes;
            clone.SingleFile = original.SingleFile;
            clone.CodeWriter = original.CodeWriter;
            clone.HasSecondaryClasses = original.HasSecondaryClasses;
            clone.AlwaysUseNullableValues = original.AlwaysUseNullableValues;
            clone.UseNamespaces = original.UseNamespaces;
            clone.ExamplesInDocumentation = original.ExamplesInDocumentation;
            clone.Types = original.Types;
            clone.UseNamespaces = original.UseNamespaces;
        }

        private JsonClassGenerator Prepare()
        {
            if (edtJson.Text == string.Empty)
            {
                MessageBox.Show(this, "Please insert json data", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                edtJson.Focus();
                return null;
            }

            if (edtMainClass.Text == string.Empty)
            {
                MessageBox.Show(this, "Please specify a main class name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            var gen = new JsonClassGenerator();
            gen.Example = edtJson.Text;
            gen.InternalVisibility = false;
            gen.CodeWriter = (ICodeWriter)cmbLanguage.SelectedItem;
            gen.ExplicitDeserialization = false;
            gen.Namespace = "Finda";
            gen.NoHelperClass = false;
            gen.SecondaryNamespace = null;
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.StartupPath + @"\Data");
            if (!di.Exists) { di.Create(); }
            gen.TargetFolder = di.ToString();
            gen.UseProperties = true;
            gen.MainClass = edtMainClass.Text;
            gen.UsePascalCase = false;
            gen.UseNestedClasses = false;
            gen.ApplyObfuscationAttributes = false;
            gen.SingleFile = true;
            gen.ExamplesInDocumentation = true;
            return gen;
        }


        #endregion

    }
}
