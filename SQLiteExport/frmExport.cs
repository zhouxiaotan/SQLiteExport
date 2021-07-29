using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace SQLiteExport
{
    public partial class frmExport : Form
    {
        string cs = @"URI=file:C:\Workspace\Fukui\SoureCode\_MstMaintUpgrade\MstMaint\MstMaint.sdb";

        public frmExport()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(txtSQLite.Text)) {
                MessageBox.Show("Please select SQLite File!!!");
                return ;
            }
            if(string.IsNullOrEmpty(txtFolder.Text)) {
                MessageBox.Show("Please select output folder!!!");
                return ;
            }
            cs = @"URI=file:" + txtSQLite.Text;
            var list = textBox2.Lines;
            foreach (var item in list)
            {
                try
                {
                    export_Data(item);
                }
                catch (Exception)
                {
                    MessageBox.Show(item + " export Fail!!!");
                }
            }
            MessageBox.Show("Export Completed!!!!!");
        }

        private void export_Data(string tableName)
        {
            var con = new System.Data.SQLite.SQLiteConnection(cs);
            con.Open();
            string sql = " select * from " + tableName;
            var dta = new System.Data.SQLite.SQLiteDataAdapter(sql, con);
            var ds = new DataSet();
            dta.Fill(ds);
            con.Close();

            List<string> output = new List<string>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string outrow = string.Empty;
                foreach (DataColumn item in ds.Tables[0].Columns)
                {
                    var data = string.Empty;
                    if (row[item] != null)
                    {
                        data = row[item].ToString();
                        if (data.Length > 100) data = "";
                    }
                    if (item.DataType.ToString() == "System.String")
                    {
                        if (data.Contains("\r") || data.Contains("\n"))
                        {
                            data = data.Replace("\r", "\\r").Replace("\n", "\\n");
                        }
                        if (data.Contains("\""))
                        {
                            data = data.Replace("\"", "\"\"");
                        }
                        if (item.ColumnName == "LAST_OP_ID")
                        {
                            if (data == string.Empty) data = "CICDL";
                        }
                        if (data == string.Empty) data = " ";
                        data = "\"" + data + "\"";
                        outrow += data + ",";
                    }
                    else if (item.DataType.ToString() == "System.Int32")
                    {
                        if (data == string.Empty) data = "0";
                        outrow += data + ",";
                    }
                    else if (item.DataType.ToString() == "System.DateTime")
                    {
                        if (item.ColumnName == "LAST_DATE")
                        {
                            if (data == string.Empty)
                            {
                                data = "20210714";
                            }
                            else
                            {
                                data = DateTime.Parse(row[item].ToString()).ToString("yyyyMMdd");
                            }
                        }
                        else if (item.ColumnName == "LAST_TIME")
                        {
                            if (data == string.Empty)
                            {
                                data = "00.00.01";
                            }
                            else
                            {
                                data = DateTime.Parse(row[item].ToString()).ToString("HH.mm.ss");
                            }

                        }
                        outrow += data + ",";
                    }
                }
                if (outrow[outrow.Length - 1] == ',')
                {
                    outrow = outrow.Remove(outrow.Length - 1);
                }
                output.Add(outrow);
                //outrow += Environment.NewLine;

            }
            if (output.Count > 0)
            {
                var filename = txtFolder.Text + "\\" + tableName + ".del";
                File.WriteAllLines(filename, output, Encoding.GetEncoding("Shift-JIS"));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dia = new OpenFileDialog();
            dia.DefaultExt = "*.*|*.*";
            dia.Multiselect = false;
            if (txtSQLite.Text != string.Empty)
            {
                dia.FileName = txtSQLite.Text;
            }
            if (dia.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtSQLite.Text = dia.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var dlg = new FileFolderDialog();
            dlg.IsDialog = true;
            if (txtFolder.Text != string.Empty)
            {
                dlg.SelectedPath = txtFolder.Text;
            }
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = dlg.SelectedPath;
            }
        }
    }
}
