using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Demo
{
    public partial class Demo : Form
    {
        public Demo() {
            InitializeComponent();
        }

        private void Demo_Load(object sender, EventArgs e) {
            columnHeader1.Width = listExif.Width / 2 - 15;
            columnHeader2.Width = columnHeader1.Width;
            //columnHeader3.Width = listIFDs.Width / 3 - 10;
            //columnHeader6.Width = columnHeader3.Width;
            //columnHeader7.Width = columnHeader3.Width;
        }

        private void button1_Click(object sender, EventArgs e) {
            SimpleExif.Exif exif;
            var digRes = openFileDialog1.ShowDialog();
            if (digRes == System.Windows.Forms.DialogResult.OK) {
                var filePath = openFileDialog1.FileName;
                lblImagePath.Text = filePath;
                using (exif = new SimpleExif.Exif(filePath)) {
                    //MessageBox.Show(exif.DebugStr.ToString());
                    //MessageBox.Show(exif.ErrorMsg);
                    //MessageBox.Show(BinaryToString(exif.BinaryTest));
                    //txtDebug.Text = GetFormatedBinaryString(BinaryToString(exif.BinaryTest));
                    listExif.Items.Clear();
                    listIFDs.Items.Clear();
                    lblSpanTime.Text = string.Format("{0}ms", exif.ProcessMilliseconds);

                    if (!exif.ExifExists) {
                        if (!string.IsNullOrEmpty(exif.ErrorMsg)) {
                            MessageBox.Show(exif.ErrorMsg);
                        }
                        return;
                    }

                    foreach (var entry in exif.ExifList) {
                        ListViewItem lvi = new ListViewItem(entry.ViewName);
                        lvi.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = entry.ViewValue });
                        listExif.Items.Add(lvi);
                    }

                    foreach (var ifd in exif.IFDList) {
                        ListViewGroup gp = new ListViewGroup() { Header = ifd.Name };
                        listIFDs.Groups.Add(gp);
                        foreach (var entry in ifd.IFDEntries) {
                            ListViewItem lvi = new ListViewItem(entry.ViewName, gp);
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = entry.Type.ToString() });
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = entry.Count.ToString() });
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = entry.ViewValue });
                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem() { Text =string.Format("({0}){1}", entry.RealValInBlock, BinaryToString(entry.RealValAry)) });
                            listIFDs.Items.Add(lvi);
                        }
                    }
                    listIFDs.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    try {
                        if (exif.Thumbnail != null) {
                            lblThumbWidth.Text = exif.Thumbnail.Width.ToString();
                            lblThumbHeight.Text = exif.Thumbnail.Height.ToString();
                            this.pictureBox1.Image = exif.Thumbnail;
                        }
                        else {
                            this.pictureBox1.Image = null;
                        }
                    }
                    catch { }
                }
            }
        }

        private void listExif_MouseEnter(object sender, EventArgs e) {
            listExif.Focus();
        }

        private void listIFDs_MouseEnter(object sender, EventArgs e) {
            listIFDs.Focus();
        }

        public string BinaryToString(byte[] data) {
            if (data == null) { return string.Empty; }
            var res = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                res.Append(string.Format("{0:X2} ", data[i]));
            }
            return res.ToString().Trim();
        }

        public string GetFormatedBinaryString(string str) {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var substr in str.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)) {
                sb.Append(substr + " ");
                i++;
                if (i == 15) {
                    //sb.AppendLine();
                    i = 0;
                }
            }
            return sb.ToString();
        }
    }
}
