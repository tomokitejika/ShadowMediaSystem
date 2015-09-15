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
using System.IO;

namespace StreamReaderForKI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IDataObject dataObj;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Main_Click(object sender, RoutedEventArgs e)
        {
            if (this.TextBlock_Main.Text == "") return;
            try
            {
                if (!this.TextBlock_Main.Text.Contains("scd"))
                {
                    throw new Exception("\"scd\"ファイルのみ対応しています。");
                }
                var reader = new BinaryReader(File.OpenRead(this.TextBlock_Main.Text));
                var size = reader.BaseStream.Length;
                var writer = new StreamWriter(this.TextBlock_Main.Text.Replace("scd", "csv"));
                

                if (size < 8)
                {
                    throw new Exception("ファイルに正常な書き込みが行われていません。");
                }
                //First frame number
                try
                {
                    while (true)
                    {
                        //header
                        var CurrentFrame = reader.ReadUInt32();
                        writer.Write(CurrentFrame);
                        writer.Write(",");

                        var time = reader.ReadInt64();
                        writer.Write(time);
                        writer.Write(",");
                        var data = reader.ReadBytes(reader.ReadInt32());
                        
                        //Decorder
                        var dec = new UDP_PACKETS_CODER.UDP_PACKETS_DECODER();
                        dec.Source = data;
                        Console.WriteLine(dec.Length);
                        
                        var MoP = (byte)dec.get_byte();//max of person

                        Console.WriteLine(MoP.ToString());
                        writer.Write(MoP);
                        writer.Write(",");
                        var NoP = (byte)dec.get_byte();//number of person]
                        writer.Write(NoP);
                        writer.Write(",");

                        for (int i = 0; i < NoP; i++)
                        {
                            var NoB = (byte)dec.get_byte();//number of person]
                            writer.Write(NoB);
                            writer.Write(",");
                            for (int j = 0; j < NoB; j++ )
                            {
                                var Dimension = dec.get_byte();
                                writer.Write(Dimension);
                                writer.Write(",");
                                var Px = dec.get_float();
                                writer.Write(Px);
                                writer.Write(",");
                                var Py = dec.get_float();
                                writer.Write(Py);
                                writer.Write(",");
                                var Pz = dec.get_float();
                                writer.Write(Pz);
                                writer.Write(",");

                                var Rx = dec.get_float();
                                writer.Write(Rx);
                                writer.Write(",");
                                var Ry = dec.get_float();
                                writer.Write(Ry);
                                writer.Write(",");
                                var Rz = dec.get_float();
                                writer.Write(Rz);
                                writer.Write(",");
                                var Rw = dec.get_float();
                                writer.Write(Rw);
                                writer.Write(",");
                                var ts = dec.get_byte();
                                writer.Write(ts);
                                writer.Write(",");
                            }
                            
                        }
                        writer.Write("\n");

                    }
                }
                catch (Exception ex)
                {
                    writer.Write(ex.Message);
                }
                finally
                {
                    reader.Close();
                    writer.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Border_Main_DragEnter(object sender, DragEventArgs e)
        {
            dataObj = e.Data as IDataObject;
            String[] FileName = dataObj.GetData(DataFormats.FileDrop) as String[];
            this.TextBlock_Main.Text = FileName[0];

        }
    }
}
