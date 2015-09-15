using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectInterface
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //宣言
        #region
        bool IsSave;
        bool IsCalib;
        System.IO.StreamWriter streamWriter;
        KinectSensor kinect;
        byte MaxofJoints;
        byte Dimension;
        //カラーイメージ
        ColorImageFormat colorImageFormat;
        ColorFrameReader colorFrameReader;
        FrameDescription colorFrameDescription;
        WriteableBitmap colorImg;
        WriteableBitmap calibImg;
        Int32Rect bitmapRect;
        int bitmapStride;
        byte[] colors;
        int imageWidth;
        int imageHeigt;
        //骨格情報
        BodyFrameReader bodyFrameReader;
        Body[] bodies;
        int NumberofPlayer;
        //深度情報
        DepthFrameReader depthFrameReader;
        FrameDescription depthFrameDescription;
        WriteableBitmap depthImg;
        Int32Rect depthRect;
        int depthStride;
        ushort[] depthBuffer;
        int depthImageWidth;
        int depthImageHeight;
        //CIPC
        CIPCClientWindow.MainWindow cipcMain;
        Calibration calib;
        Calibration._calibMode calibMode;
        string preIP;
        int preServerPort;
        int preCLientPort;
        int time;

        //UDP
        UDP.UDP_CLIENT_AUT_Window UDPclientWidow;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            this.IsSave = false;
            this.IsCalib = false;
            this.kinect = KinectSensor.GetDefault();
            this.MaxofJoints = (byte)24;
            this.Dimension = (byte)7;
            //colorImage
            #region
            this.colorImageFormat = ColorImageFormat.Bgra;
            this.colorFrameDescription = this.kinect.ColorFrameSource.CreateFrameDescription(this.colorImageFormat);
            this.colorFrameReader = this.kinect.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += ColorFrame_Arrived;
            this.colors = new byte[this.colorFrameDescription.Width
                                           * this.colorFrameDescription.Height
                                           * this.colorFrameDescription.BytesPerPixel];
            this.imageWidth = this.colorFrameDescription.Width;
            this.imageHeigt = this.colorFrameDescription.Height;
            this.colorImg = new WriteableBitmap(this.colorFrameDescription.Width, this.colorFrameDescription.Height, 96, 96, PixelFormats.Bgr32, null);
            this.calibImg = new WriteableBitmap(this.colorFrameDescription.Width, this.colorFrameDescription.Height, 96, 96, PixelFormats.Bgr32, null);                    
            this.bitmapRect = new Int32Rect(0, 0, this.colorFrameDescription.Width, this.colorFrameDescription.Height);
            this.bitmapStride = this.colorFrameDescription.Width * (int)this.colorFrameDescription.BytesPerPixel;
            this.colorImage.Source = this.colorImg;
            this.calibrationImage.Source = this.calibImg;
            #endregion
            //骨格情報
            #region
            this.bodyFrameReader = this.kinect.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += BodyFrame_Arrived;
            #endregion
            //震度情報
            #region
            this.depthFrameReader = this.kinect.DepthFrameSource.OpenReader();
            this.depthFrameReader.FrameArrived += DepthFrame_Arrived;
            this.depthFrameDescription = this.kinect.DepthFrameSource.FrameDescription;
            this.depthBuffer = new ushort[this.depthFrameDescription.LengthInPixels];
            this.depthImageWidth = this.depthFrameDescription.Width;
            this.depthImageHeight = this.depthFrameDescription.Height;
            #endregion

            this.calib = new Calibration();
            this.calibMode = Calibration._calibMode.zero;
            this.preIP = "127.0.0.1";
            this.preCLientPort = 54000;
            this.preServerPort = 50000;
            this.Closed += MainWindow_Closed;
            //System.Threading.Timer timer = new System.Threading.Timer(new System.Threading.TimerCallback(this.Callback));
            //timer.Change(0, 1000);
        }

        //void Callback(object sender) { 
        //Console.Write("a"); }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                this.cipcMain.Close();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }  
        void DataReceived(object sender, byte[] e)
        {
            Console.WriteLine("recieved1");
            
            UDP_PACKETS_CODER.UDP_PACKETS_DECODER dec = new UDP_PACKETS_CODER.UDP_PACKETS_DECODER();
            
            dec.Source = e;
            int i = dec.get_int();
            
            switch (i)
            {
                case 0: 
                    this.calibMode = Calibration._calibMode.zero; this.IsCalib = true; break;
                case 1: 
                    this.calibMode = Calibration._calibMode.x; this.IsCalib = true; break;
                case 2: 
                    this.calibMode = Calibration._calibMode.z; this.IsCalib = true; break;
                case 3: break;
                case 4: break;

            }
            if (i == 0)
            {
                Console.WriteLine("change");
                this.IsCalib = true;
                
            }
            
            this.IsCalib = true;
            Console.WriteLine("recieved2");
        }
       

        //Button
        void ConnectCIPC(object sender, RoutedEventArgs e)
        {
            //Connect
            if (this.cipcMain == null)
            {
                CIPCClientWindow.CIPCSettingWindow settingWindow = new CIPCClientWindow.CIPCSettingWindow(this.preIP, this.preServerPort, this.preCLientPort); 
                if (settingWindow.ShowDialog() == true)
                {
                    this.cipcMain = new CIPCClientWindow.MainWindow("CIPC", settingWindow);
                    this.cipcMain.Show();
                    this.CIPCButton.Content = "Close";
                    this.cipcMain.DataReceived += this.DataReceived;
                    Console.WriteLine("eventhander");
                }
            }
            //Close
            else
            {
                try
                {
                    this.preIP = this.cipcMain.TextBlock_ServerIPAdress.Text;
                    this.preCLientPort = int.Parse(this.cipcMain.TextBlock_ClientPort.Text);
                    this.preServerPort = int.Parse(this.cipcMain.TextBlock_ServerPort.Text);
                    this.cipcMain.Close();
                    this.cipcMain = null;
                    this.CIPCButton.Content = "Connect";
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
            
            
        }
        void ConnectUDP(object sender, RoutedEventArgs e)
        {
            //Connect
            if (this.UDPclientWidow == null)
            {
                this.UDPclientWidow = new UDP.UDP_CLIENT_AUT_Window();
                this.UDPclientWidow.DataReceived += this.DataReceived;
                this.UDPclientWidow.Show();
                this.UDPButton.Content = "Close";
                
            }
            //Close
            else
            {
                try
                {
                    this.UDPclientWidow.Close();
                    this.CIPCButton.Content = "Connect";
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
        }
        void KinectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.KinectButton.Content.ToString() == "Start")
            {
                //kinect Start
                try
                {
                    this.kinect.Open();
                    this.KinectButton.Content = "Stop";
                    //this.time = DateTimeOffset.Now.Millisecond;
                    if (this.kinect == null) this.kinect = KinectSensor.GetDefault();
                }
                catch
                {
                    this.ErrorPoint( System.Reflection.MethodBase.GetCurrentMethod().Name.ToString());
                }
            }
            else if (this.KinectButton.Content.ToString() == "Stop")
            {
                //kinect Stop
                try
                {
                    if (this.kinect != null) this.kinect.Close();
                    this.KinectButton.Content = "Start";
                }
                catch 
                {
                    this.ErrorPoint(System.Reflection.MethodBase.GetCurrentMethod().Name.ToString());
                }
                
            }
        } 
        void SaveCsv(object sender, RoutedEventArgs e)
        {
            if (this.IsSave)
            {
                this.IsSave = false;
                this.SaveFile.Content = "Save";

            }
            else
            {
                try
                {
                    this.IsSave = true;
                    this.streamWriter = new System.IO.StreamWriter(this.FileName.Text.ToString() + ".csv");
                    this.SaveFile.Content = "Stop";
                }
                catch
                {
                    this.ErrorPoint(System.Reflection.MethodBase.GetCurrentMethod().Name.ToString());
                }
                
            }
        }
        void Save(object sender, RoutedEventArgs e)
        {
            this.calib.Save(this.preIP);
        }
        void Load(object sender, RoutedEventArgs e)
        {
            this.calib.Read();
            this.preIP = this.calib.saveData.IpAdress;
            this.CalibInfo_Zero.Content = "X:"+this.calib.saveData.zero.X + " Y:"+ this.calib.saveData.zero.Y + " Z:" + this.calib.saveData.zero.Z ;
            this.CalibInfo_X.Content = "X:" + this.calib.saveData.x.X + " Y:" + this.calib.saveData.x.Y + " Z:" + this.calib.saveData.x.Z;
            this.CalibInfo_Z.Content = "X:" + this.calib.saveData.z.X + " Y:" + this.calib.saveData.z.Y + " Z:" + this.calib.saveData.z.Z;
            this.CalibrationInfo.IsSelected = true;
        }
        
        //骨格情報取得
        void BodyFrame_Arrived(object sender, BodyFrameArrivedEventArgs e)
        {
            try
            {
                BodyFrame bodyFrame = e.FrameReference.AcquireFrame();
                if (bodyFrame == null) return;
                this.bodies = new Body[this.kinect.BodyFrameSource.BodyCount]; //bodycountに骨格情報の数
                bodyFrame.GetAndRefreshBodyData(this.bodies);

                //人数確認
                #region
                this.NumberofPlayer = 0;
                foreach (var body in bodies.Where(b => b.IsTracked))
                {
                    this.NumberofPlayer++;
                }
                bodyFrame.GetAndRefreshBodyData(bodies);
                #endregion
                //データ送信
                if(this.SendData_Body.IsChecked == true) this.SendData();

                //破棄
                bodyFrame.Dispose();
            }
            catch
            {
                this.ErrorPoint(System.Reflection.MethodBase.GetCurrentMethod().ToString());
            }

        }
        void SendBodyData(ref UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc, Body[] bodies, int numberofplayer)
        {
            try
            {
                //UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();
                List<string> str = new List<string>();
            
                str.Add(bodies.Length.ToString());
                byte MoP = (byte)bodies.Length;
                enc += MoP; //MaxofPlayer

                str.Add(numberofplayer.ToString());
                byte NoP = (byte)numberofplayer;
                enc += NoP; //NumberOfPlayer

                int NumberofBody = 0;
                int NumberofJoint = 0;
                foreach (var body in bodies.Where(b => b.IsTracked))
                {
                
                    NumberofBody++;
                    byte NoB = (byte)body.Joints.ToArray().Length;
                    enc += NoB;
                
                    foreach (var joint in body.Joints)
                    {
                        //kinect関節名前書き出し
                        str.Add(joint.Value.JointType.ToString());
                        NumberofJoint++;
                        str.Add(NumberofJoint.ToString());
                        enc += this.Dimension;
                        enc += joint.Value.Position.X; 
                        enc += joint.Value.Position.Y;
                        enc += joint.Value.Position.Z;
                        //追加
                        enc += body.JointOrientations[joint.Value.JointType].Orientation.X;
                        enc += body.JointOrientations[joint.Value.JointType].Orientation.Y;
                        enc += body.JointOrientations[joint.Value.JointType].Orientation.Z;                    
                        enc += body.JointOrientations[joint.Value.JointType].Orientation.W;
                        byte TS = (byte)joint.Value.TrackingState;
                        enc += TS;

                        #region
                        str.Add(this.Dimension.ToString());
                        str.Add(joint.Value.Position.X.ToString());
                        str.Add(joint.Value.Position.Y.ToString());
                        str.Add(joint.Value.Position.Z.ToString());
                        str.Add(body.JointOrientations[joint.Value.JointType].Orientation.X.ToString());
                        str.Add(body.JointOrientations[joint.Value.JointType].Orientation.Y.ToString());
                        str.Add(body.JointOrientations[joint.Value.JointType].Orientation.Z.ToString());
                        str.Add(body.JointOrientations[joint.Value.JointType].Orientation.W.ToString());
                        str.Add(joint.Value.TrackingState.ToString());
                        #endregion
                    }
                }

            

                //byte[] data = enc.data;
                //csvに保存
                #region
                if (this.IsSave)
                {

                    //string s2 = string.Join(",", s1);
                    string strdata = string.Join(",", str.ToArray());
                    this.streamWriter.WriteLine(strdata);
                    //sw.Close();
                }
                #endregion
                //CIPCに送信
                /*
                #region
                if (this.cipcMain != null)
                {    
                    this.cipcMain.send(data);
                }
                #endregion
                */
            }
            catch
            {
                this.ErrorPoint(System.Reflection.MethodBase.GetCurrentMethod().ToString());
            }
        }

        //キャリブレーション装置LEDの中心をとる
        //カラーイメージ取得
        void ColorFrame_Arrived(object sender, ColorFrameArrivedEventArgs e)
        {
            try
            {
                ColorFrame colorFrame = e.FrameReference.AcquireFrame();
                //フレームがなければ終了、あれば格納
                if (colorFrame == null) return;
                colorFrame.CopyConvertedFrameDataToArray(this.colors, this.colorImageFormat);
                //表示
                this.colorImg.WritePixels(this.bitmapRect, this.colors, this.bitmapStride, 0);
                //データ送信
                if (this.IsCalib == true || this.SendData_LEDPt.IsChecked == true)
                {
                    this.SendData_LEDPt.IsChecked = true;
                    this.SendData();
                    this.IsCalib = false;
                }
               
                //破棄
                colorFrame.Dispose();
            }
            catch
            {
                this.ErrorPoint(System.Reflection.MethodBase.GetCurrentMethod().ToString());
            }

            
        }
        //深度情報取得
        void DepthFrame_Arrived(object sender, DepthFrameArrivedEventArgs e)
        {
            try
            {
                DepthFrame depthFrame = e.FrameReference.AcquireFrame();
                //フレームがなければ終了、あれば格納
                if (depthFrame == null) return;
                int[] depthBitdata = new int[depthBuffer.Length];
                depthFrame.CopyFrameDataToArray(this.depthBuffer);

                //破棄
                depthFrame.Dispose();
            }
            catch
            {
                this.ErrorPoint(System.Reflection.MethodBase.GetCurrentMethod().ToString());
            }
        }
        //座標算出
        void SendLEDPtData(ref UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc)
        {           
            CameraSpacePoint LEDPt = this.calib.GetLEDPositinon(this.imageWidth, 
                                                           this.imageHeigt,
                                                           this.colors,
                                                           new OpenCvSharp.CPlusPlus.Scalar(30,93,93),
                                                           this.depthBuffer);
            this.calib.MakeSaveData(this.calibMode, LEDPt);
            enc += LEDPt.X;
            enc += LEDPt.Y;
            enc += LEDPt.Z;
            Console.WriteLine("X:" + LEDPt.X.ToString());
            Console.WriteLine("Y:" + LEDPt.Y.ToString());
            Console.WriteLine("Z:" + LEDPt.Z.ToString());

            //キャリブレーション画像表示
            OpenCvSharp.CPlusPlus.Mat calibMat = calib.GetCalibrationImage().Clone();
            this.calibrationImage.Source = OpenCvSharp.Extensions.WriteableBitmapConverter.ToWriteableBitmap(calibMat);
            this.CalibrationImageTable.IsSelected = true;
            
            //送信データ表示
            this.message.Content = "LEDPosition  " + "X:" + LEDPt.X.ToString() + " Y:" + LEDPt.Y.ToString() + " Z:" + LEDPt.Z.ToString();
            
            //データ送信モード変更
            this.SendData_LEDPt.IsChecked = false;
            this.SendData_Body.IsChecked = true;
        
        }

        //データ送信
        void SendData()
        {
            //CIPCに送信
            #region
            try
            {
                UDP_PACKETS_CODER.UDP_PACKETS_ENCODER enc = new UDP_PACKETS_CODER.UDP_PACKETS_ENCODER();

                if (this.SendData_Body.IsChecked == true) this.SendBodyData(ref enc, this.bodies, this.NumberofPlayer); 
                else if (this.SendData_LEDPt.IsChecked == true) this.SendLEDPtData(ref enc);

                if (this.cipcMain != null && enc.data != null)
                {
                    this.cipcMain.send(enc.data);
                    //Console.WriteLine("sendDat");
                }
                if (this.UDPclientWidow != null && enc.data != null)
                {
                    this.UDPclientWidow.Send(enc.data);
                    //Console.WriteLine("sendDat");
                }
            }
            catch
            {
                this.ErrorPoint(System.Reflection.MethodBase.GetCurrentMethod().Name.ToString());
                
            }
            #endregion

        }

        //エラー検出
        void ErrorPoint(string methodName)
        {
            //Console.WriteLine("Error : " + this.ToString() + ":" + methodName );
            this.message.Content = "Error : " + this.ToString() + ":" + methodName;
        }
      
        //終了時の処理
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            //カラーリーダーの終了      
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }
            //ボディリーダーの終了
            
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }
            
            //ディプスリーダーの終了
            if (this.depthFrameReader != null)
            {
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }
            
            //キネクトの終了
            if (this.kinect != null)
            {
                this.kinect.Close();
            }
            //StreamWriter
            if(this.streamWriter != null) this.streamWriter.Close();
        }
   }
}
