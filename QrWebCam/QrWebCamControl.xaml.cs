using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Threading;
using System.Drawing;
using AForge.Video.DirectShow;
using System.Reflection;
using ZXing;

namespace QrWebCam
{
    /// <summary>
    /// Interaction logic for QrWebCamControl.xaml
    /// </summary>
    public partial class QrWebCamControl : UserControl
    {
       
        List<string> cameraNames;
        Bitmap currentFrame;
        MediaPlayer mp;
        CancellationToken token;
        CancellationTokenSource cts;
        Task decoderTask;
        public event EventHandler<string> QrDecoded;
        public QrWebCamControl()
        {
            InitializeComponent();
            this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            DisposeCamera();
        }

        public List<string> CameraNames
        {
            get { return cameraNames; }
            set { cameraNames = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CameraNames")); }
        }


        private QrWebCam.CameraDevices Cam { get; set; } = new CameraDevices();

        public string DecodedId
        {
            get { return (string)GetValue(DecodedIdProperty); }
            set { SetValue(DecodedIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DecodedId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DecodedIdProperty =
            DependencyProperty.Register("DecodedId", typeof(string), typeof(QrWebCamControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));




        private int cameraIndex;
        public int CameraIndex
        {
            get { return cameraIndex; }
            set { cameraIndex = value; camIndex_SelectionChanged(value); }
        }

        // Using a DependencyProperty as the backing store for CameraIndex.  This enables animation, styling, binding, etc...
       




        public event PropertyChangedEventHandler PropertyChanged;


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            CameraNames = Cam.GetStringCameras();
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeCamera();
        }


        public string BeepSoundPath { get; set; }




        private void Init(int index)
        {
            mp = new MediaPlayer() { Volume = 100 };
            if(!string.IsNullOrEmpty(BeepSoundPath)) mp.Open(new Uri(BeepSoundPath ?? ""));
            Cam.SelectCamera(index);
            Cam.Current.NewFrame += Current_NewFrame;
            Cam.Current.Start();
            cts = new CancellationTokenSource();
            token = cts.Token;
            decoderTask = Task.Factory.StartNew(() => {
                try
                {
                    DecodeQrcode();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }, token);
            
        }

        private void HandleDecoded(string val)
        {
            QrDecoded?.Invoke(this, val);
            DecodedId = val;
        }

        private async void DecodeQrcode()
        {
            BarcodeReader reader = new BarcodeReader();
            while (true)
            {
                if (token.IsCancellationRequested) break;
                if (Cam.Current?.IsRunning == true)
                {
                    if (currentFrame == null) continue;

                    try
                    {

                        var result = reader.Decode(currentFrame);

                        if (result != null)
                        {
                            await Dispatcher.InvokeAsync(() => HandleDecoded(result.Text));
                            await Dispatcher.InvokeAsync(() => Beep());
                            await Task.Delay(400);
                            //MessageBox.Show(result.Text);
                        }
                    }
                    catch (Exception e)
                    {

                        // throw;
                    }

                }
                //await Task.Delay(200);
            }
        }


        private async void Beep()
        {

            mp.Stop();
            mp.Play();
            await Task.Delay(2000);
        }


        private void DisposeCamera()
        {
            cts?.Cancel();
            Cam.Current?.SignalToStop();
            if (Cam.Current != null) Cam.Current.NewFrame -= Current_NewFrame;

            currentFrame?.Dispose();
            currentFrame = null;

            mp?.Close();
            mp = null;
        }

        private void Current_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                Dispatcher.Invoke(() => { camStream.Source = eventArgs.Frame.Convert(); currentFrame = (Bitmap)eventArgs.Frame.Clone(); });

            }
            catch
            {

                //throw;
            }
        }

        private void camIndex_SelectionChanged(int e)
        {
            if (Cam.Devices.Count <= 0) return;
            if (Cam.Current == null)
            {
                Init(e);
            }
            else
            {
                DisposeCamera();
                Init(e);
            }
        }

    }
}
