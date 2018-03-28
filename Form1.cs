using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace WindowsFormsApp2
{


    public partial class Form1 : Form
    {
        private Bitmap image;
        private Bitmap imgResult;
        private ImageSegmentor imgSeg;

        public Form1()
        {
            InitializeComponent();
            imgSeg = new ImageSegmentor();
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                image = new Bitmap(openFileDialog1.FileName);
               
                pictureBox2.ImageLocation = openFileDialog1.FileName;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }
        

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private byte GetSegments()
        {
            byte seg;
            try
            {
                seg = Byte.Parse(textBox1.Text);
                textBox1.Text = seg.ToString();
            }
            catch (FormatException exc)
            {
                System.ArgumentException argEx = new System.ArgumentException("Invalid data format. Please enter a number.");
                throw argEx;
            }
            return seg;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte seg = GetSegments();
            imgSeg.SegmentNumber = seg;
            imgResult = imgSeg.Segmentation(image);
            pictureBox1.Image = imgResult;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        void SaveImage(Bitmap img)
        {
            
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream("./1.bmp", FileMode.Create, FileAccess.ReadWrite))
                {
                    img.Save(memory ,ImageFormat.Bmp);
                    byte[] b = memory.ToArray();
                    fs.Write(b, 0, b.Length);
                }
            }
           
        }

    }
}
