using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class ImageSegmentor
    {
        private struct MyPixel
        {
            public byte red;
            public byte green;
            public byte blue;
            public byte segment;

            public MyPixel(byte b, byte g, byte r, byte s = 0)
            {
                this.blue = b;
                this.red = r;
                this.green = g;
                this.segment = s;
            }

            public MyPixel(MyPixel other)
            {
                this.blue = other.blue;
                this.red = other.red;
                this.green = other.green;
                this.segment = other.segment;
            }

            public static bool operator ==(MyPixel rhs, MyPixel lhs)
            {
                return ((rhs.blue == lhs.blue) && (rhs.red == lhs.red) && (rhs.green == lhs.green));
            }

            public static bool operator !=(MyPixel rhs, MyPixel lhs)
            {
                return !(rhs == lhs);
            }

            public void setSegment(byte s)
            {
                this.segment = s;
            }

            public void CopyPixelData(MyPixel avrg)
            {
                this.red = avrg.red;
                this.green = avrg.green;
                this.blue = avrg.blue;
            }

            public void CopyPixelData(MyPixelAvrg avrg)
            {
                this.red = (byte)avrg.red;
                this.green = (byte)avrg.green;
                this.blue = (byte)avrg.blue;
            }
        }

        private struct MyPixelAvrg
        {
            public int red;
            public int green;
            public int blue;
            public int segment;
            public int pixelCount;

            public MyPixelAvrg(byte s)
            {
                this.red = 0;
                this.green = 0;
                this.blue = 0;
                this.pixelCount = 0;
                this.segment = s;
            }

            public void AddToColor(MyPixel other)
            {
                this.red += other.red;
                this.green += other.green;
                this.blue += other.blue;
                this.pixelCount++;
            }

            public void AverageOut()
            {
                this.red = this.red / this.pixelCount;
                this.green = this.green / this.pixelCount;
                this.blue = this.blue / this.pixelCount;
                this.pixelCount = 0;
            }
        }

        private byte segmentNumber;
        private List<MyPixel> SegList;
        private List<MyPixel> arrP;
        public ImageSegmentor(byte num = 0)
        {
            this.SegList = new List<MyPixel>();
            this.arrP = new List<MyPixel>();
            this.segmentNumber = num;
        }

        ~ImageSegmentor()
        {
           
        }

        public byte SegmentNumber
        {
            set
            {
                this.segmentNumber = value;
            }
        }

        public Bitmap Segmentation(Bitmap image)
        {
            GetPixelData(image);
            SetSegments();
            CalcSegmentation();
            SegmentImage();            
            return CreateNewBitmap(image);
        }

        private void GetPixelData(Bitmap image)
        {
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            System.Drawing.Imaging.BitmapData imgData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            IntPtr ptr = imgData.Scan0;

            int bytes = imgData.Stride * imgData.Height;
            byte[] rgb = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgb, 0, bytes);
            for (int columns = 0; columns < imgData.Height; columns++)
            {
                for (int rows = 0; rows < imgData.Width; rows++)
                {
                    arrP.Add(new MyPixel(rgb[(columns * imgData.Stride) + (rows * 3)],
                        rgb[(columns * imgData.Stride) + (rows * 3 + 1)],
                        rgb[(columns * imgData.Stride) + (rows * 3 + 2)]));
                }
            }
            image.UnlockBits(imgData);
        }

        private void SetSegments()
        {
            Random rng = new Random();
            for (byte i = 1; i <= this.segmentNumber; i++)
            {
                int num = rng.Next(arrP.Count);
                MyPixel temp = new MyPixel(arrP[num].blue, arrP[num].green, arrP[num].red, i);
                if (IsUnique(temp))
                {
                    SegList.Add(new MyPixel(temp));
                }
                else
                {
                    i--;
                }
            }
        }

        private bool IsUnique(MyPixel temp)
        {
            bool isUnique = true;
            for (int i = 0; i < SegList.Count; i++)
            {
                if (temp == SegList[i])
                {
                    isUnique = false;
                    break;
                }
            }
            return isUnique;
        }

        private void CalcSegmentation()
        {
            bool isReady = false;
            Dictionary<int, MyPixelAvrg> averageP = new Dictionary<int, MyPixelAvrg>();
            for (byte i = 0; i < SegList.Count; i++)
            {
                averageP[i] = new MyPixelAvrg(i);
            }

            while (!isReady)
            {
                isReady = true;
                for (int i = 0; i < arrP.Count; i++)
                {
                    byte SegmentIndex = FindClosestSegmentPoint(arrP[i]);

                    if (SegmentIndex != arrP[i].segment)
                        isReady = false;


                    MyPixel tp = arrP[i];
                    tp.setSegment(SegmentIndex);
                    arrP[i] = tp;
                    MyPixelAvrg temp = averageP[SegmentIndex];
                    temp.AddToColor(arrP[i]);
                    averageP[SegmentIndex] = temp;
                }

                for (int i = 0; i < SegList.Count; i++)
                {
                    MyPixelAvrg temp = averageP[i];
                    temp.AverageOut();
                    averageP[i] = temp;
                    MyPixel tp = SegList[i];
                    tp.CopyPixelData(averageP[i]);
                    SegList[i] = tp;
                }
            }
        }

        private byte FindClosestSegmentPoint(MyPixel pixel)
        {
            int closestIndex = -1;
            int minimumDistance = Int32.MaxValue;

            for (int i = 0; i < SegList.Count; i++)
            {
                int temp = Math.Min(minimumDistance, EquidianDistance(pixel, SegList[i]));
                if (temp < minimumDistance)
                {
                    minimumDistance = temp;
                    closestIndex = i;
                }
            }
            return (byte)closestIndex;
        }

        private int EquidianDistance(MyPixel p1, MyPixel p2)
        {
            return (p1.blue - p2.blue) * (p1.blue - p2.blue) + (p1.red - p2.red) * (p1.red - p2.red) + (p1.green - p2.green) * (p1.green - p2.green);
        }

        private void SegmentImage()
        {
            for (int i = 0; i < arrP.Count; i++)
            {
                int segmentNum = arrP[i].segment;
                arrP[i] = SegList[segmentNum];
            }
        }

        private Bitmap CreateNewBitmap(Bitmap image)
        {
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            System.Drawing.Imaging.BitmapData imgData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int bytes = imgData.Stride * imgData.Height;
            byte[] rgb = new byte[bytes];
            int arrIndex = 0;
            for (int columns = 0; columns < imgData.Height; columns++)
            {
                for (int rows = 0; rows < imgData.Width; rows++)
                {

                    int index = (columns * imgData.Stride) + (rows * 3);
                    rgb[index] = (byte)arrP[arrIndex].blue;
                    rgb[index + 1] = (byte)arrP[arrIndex].green;
                    rgb[index + 2] = (byte)arrP[arrIndex].red;
                    arrIndex++;
                }

            }
            image.UnlockBits(imgData);
            return new Bitmap(image.Width, image.Height, imgData.Stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(rgb, 0));
        }
    }
}
