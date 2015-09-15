using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Blob;
using OpenCvSharp;

namespace KinectInterface
{
    class Calibration
    {
        Mat CalibrationImage;
        public enum _calibMode
        {
            zero,x,z,
        }
        public class SaveData
        {
            public Microsoft.Kinect.CameraSpacePoint zero ;
            public Microsoft.Kinect.CameraSpacePoint x ;
            public Microsoft.Kinect.CameraSpacePoint z ;
            public string IpAdress;

        }
        public SaveData saveData;

        public Calibration() 
        {
            this.CalibrationImage = new Mat();
            saveData = new SaveData();
        }

        //LEDの位置検出
        public Microsoft.Kinect.CameraSpacePoint GetLEDPositinon(int imgW, int imgH, byte[] colors, Scalar color, ushort[] depthBuffer)
        {
            //二値化　→　輪郭抽出・中心位置取得　→　depth位置に変換　→　座標取得
            //Mat ColorImage = this.CreatMat(colors, imgW, imgH); 
            //Mat GrayScaleImage = this.Converter(ColorImage);
            Mat GrayScaleImage = this.Converter(colors, imgW, imgH, 4);
            Point pt = this.GetCenterPointofLED(GrayScaleImage); 
            return this.GetCenterPosition(pt, depthBuffer, imgW, imgH); 
        }

        public Mat GetCalibrationImage()
        {
            return this.CalibrationImage;
        }
        public void MakeSaveData(_calibMode mode, Microsoft.Kinect.CameraSpacePoint data)
        {
            try
            {
                switch (mode)
                {
                    case _calibMode.zero:
                        this.saveData.zero = data;
                        break;
                    case _calibMode.x:
                        this.saveData.x = data;
                        break;
                    case _calibMode.z:
                        this.saveData.z = data;
                        break;
                }
            }
            catch
            {
                Console.WriteLine("savedata");
            }
            
        }
        public void Save()
        {
            try
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
                string fileName = "";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fileName = sfd.FileName;
                }
                else { }

                //XmlSerializerオブジェクトを作成
                //オブジェクトの型を指定する
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(SaveData));
                //書き込むファイルを開く（UTF-8 BOM無し）
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                    fileName, false, new System.Text.UTF8Encoding(false));
                //シリアル化し、XMLファイルに保存する
                serializer.Serialize(sw, this.saveData);
                //ファイルを閉じる
                sw.Close();
            }
            catch(Exception ex) 
            { 
            }
            

        }
        public void Save(string IpAdress)
        {
            this.saveData.IpAdress = IpAdress;
            try
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
                string fileName = "";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fileName = sfd.FileName;
                }
                else { }

                //XmlSerializerオブジェクトを作成
                //オブジェクトの型を指定する
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(SaveData));
                //書き込むファイルを開く（UTF-8 BOM無し）
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                    fileName, false, new System.Text.UTF8Encoding(false));
                //シリアル化し、XMLファイルに保存する
                serializer.Serialize(sw, this.saveData);
                //ファイルを閉じる
                sw.Close();
            }
            catch (Exception ex)
            {
            }


        }
        public void Read()
        {
            try
            {
                System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
                string fileName = "";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                }
                else { }

                //XmlSerializerオブジェクトを作成
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(SaveData));
                //読み込むファイルを開く
                System.IO.StreamReader sr = new System.IO.StreamReader(
                    fileName, new System.Text.UTF8Encoding(false));
                //XMLファイルから読み込み、逆シリアル化する
                this.saveData = (SaveData)serializer.Deserialize(sr);
                //ファイルを閉じる
                sr.Close();
            }
            catch (Exception ex)
            {
            }
        }
        
       

        //Mat作成
        Mat CreatMat(byte[] colors, int imgW, int imgH)
        {
            Mat colorImage = new Mat(imgH, imgW, MatType.CV_8UC4);
            int channenl = colorImage.Depth();
            unsafe
            {
                byte* matPtr = colorImage.DataPointer;
                for (int i = 0; i < colors.Length; i++)
                {
                    *(matPtr + i) = colors[i];
                }
            }

            
            return colorImage;
        }

        //画像二値化
        Mat Converter(Mat colorImage, Scalar colorHsv, int range)
        {
            
            int channel = colorImage.Channels();
            int imageW = colorImage.Width;
            int imageH = colorImage.Height;
            colorImage.CvtColor(OpenCvSharp.ColorConversion.BgrToHsv);
            Mat grayImage = new Mat(imageH, imageW, MatType.CV_8UC1);

            unsafe
            {
                byte* matPtr = grayImage.DataPointer;
                byte* colorPtr = colorImage.DataPointer;

                for (int i = 0; i < imageW * imageH; i++)
                {
                    //color Comperer
                    if (*(colorPtr + i * channel) < (colorHsv.Val0 + range) && *(colorPtr + i * channel) > (colorHsv.Val0 - range) &&
                       (*(colorPtr + i * channel + 1) < (colorHsv.Val1 + range) && *(colorPtr + i * channel + 1) > (colorHsv.Val1 - range)) &&
                       (*(colorPtr + i * channel + 2) < (colorHsv.Val2 + range) && *(colorPtr + i * channel + 2) > (colorHsv.Val2 - range)))
                    {
                        *(matPtr + i) = 255;
                    }
                    else
                    {
                        *(matPtr + i) = 0;
                    }
                }
            }     

            return grayImage;
        }
        Mat Converter(Mat colorImage)
        {

            int channel = colorImage.Channels();
            int imageW = colorImage.Width;
            int imageH = colorImage.Height;
            //colorImage.CvtColor(OpenCvSharp.ColorConversion.BgrToHsv);
            Mat grayImage = new Mat(imageH, imageW, MatType.CV_8UC1);
            unsafe
            {
                byte* matPtr   = grayImage.DataPointer;
                byte* colorPtr = colorImage.DataPointer;

                for (int i = 0; i < imageW * imageH; i++)
                {
                    int red = (*(colorPtr + i * channel) + *(colorPtr + i * channel + 1))/2;

                    //color Comperer
                    if (0 < *(colorPtr + i * channel))
                    {
                        *(matPtr + i) = 0;
                    }
                    else
                    {
                        *(matPtr + i) = 255;
                    }
                }
            }

            return grayImage;
        }
        Mat Converter(byte[] colors, int imgW, int imgH, int channel)
        {
            Mat grayImage = new Mat(imgH, imgW, MatType.CV_8UC1);
            
            unsafe
            {
                byte* matPtr = grayImage.DataPointer;
             
                for (int i = 0; i < imgW * imgH; i++)
                {
                    //color Comperer
                    int red = (colors[i * channel] + colors[i * channel + 1])   + 50;

                    if (red < colors[i * channel + 2])
                    {
                        *(matPtr + i) = 255;
                    }
                    else
                    {
                        *(matPtr + i) = 0;
                    }

                }
            }


            return grayImage;
        }
        //輪郭抽出して中心座標取得
        Point GetCenterPointofLED(Mat grayImage)
        {       
            OpenCvSharp.CPlusPlus.Point centerPoint = new OpenCvSharp.CPlusPlus.Point();
            IplImage grayIpl = grayImage.ToIplImage().Clone();
            IplImage calibIpl = new IplImage(grayIpl.Size, BitDepth.U8, 3);
            //中心の検出    
            CvBlobs blobs = new CvBlobs();
            blobs.Label(grayIpl);
            //blobs.FilterByArea(20, 1500);
            CvBlob blob = blobs.LargestBlob();
            
            try
            {
                if (blob != null)
                {
                    centerPoint = new Point(blob.Centroid.X, blob.Centroid.Y);
                    
                    blobs.RenderBlobs(grayIpl, calibIpl);
                }
            }catch{
                Console.WriteLine("eroor:counter");
            }

            this.CalibrationImage = new Mat(calibIpl);
            Console.WriteLine(centerPoint);
            return centerPoint;
        }

        //3次元位置取得
        Microsoft.Kinect.CameraSpacePoint GetCenterPosition(OpenCvSharp.CPlusPlus.Point colorImagePoint, ushort[] depthBuffer, int imgW,int imgH)
        {
           
            Microsoft.Kinect.KinectSensor kinect = Microsoft.Kinect.KinectSensor.GetDefault();
            
            Microsoft.Kinect.CameraSpacePoint[] bodyPosition = new Microsoft.Kinect.CameraSpacePoint[imgW * imgH];
            kinect.CoordinateMapper.MapColorFrameToCameraSpace(depthBuffer, bodyPosition);
            
            Microsoft.Kinect.CameraSpacePoint centerPoint = bodyPosition[colorImagePoint.X + colorImagePoint.Y * imgW];
            //Console.WriteLine(centerPoint.X.ToString());
            
            return centerPoint;
        }
        
        

    }
}
