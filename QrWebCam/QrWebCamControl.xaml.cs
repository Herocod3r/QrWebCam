using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using AForge.Imaging;
using AForge.Video;
using ZXing;

namespace QrWebCam
{
    /// <summary>
    ///     Interaction logic for QrWebCamControl.xaml
    /// </summary>
    public partial class QrWebCamControl : INotifyPropertyChanged
    {
        #region Public properties

        public MediaPlayer MediaPlayer { get; set; }

        public string BeepSoundPath { get; set; }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public int CameraIndex
        {
            get { return _cameraIndex; }
            set
            {
                _cameraIndex = value;
                CameraIndexSelectionChanged(value);
            }
        }

        public List<string> CameraNames
        {
            get { return _cameraNames; }
            set
            {
                _cameraNames = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Private vars

        private CameraDevices Cam { get; } = new CameraDevices();
        private CancellationTokenSource Cts { get; set; }

        private List<string> _cameraNames;
        private int _cameraIndex;
        private string _statusText = "No Signal";

        #endregion

        #region Events

        public event EventHandler<string> QrDecoded;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public QrWebCamControl()
        {
            InitializeComponent();
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            DataContext = this;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            DisposeCamera();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CameraNames = Cam.GetStringCameras();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DisposeCamera();
        }

        private void Init(int index)
        {
            MediaPlayer = new MediaPlayer {Volume = 100};
            if (!string.IsNullOrEmpty(BeepSoundPath)) MediaPlayer.Open(new Uri(BeepSoundPath ?? ""));
            Cam.SelectCamera(index);
            Cam.Current.NewFrame += CameraNewFrame;
            Cam.Current.Start();
            Cts = new CancellationTokenSource();
        }

        private void DisposeCamera()
        {
            Cts?.Cancel();
            Cam.Current?.SignalToStop();
            if (Cam.Current != null) Cam.Current.NewFrame -= CameraNewFrame;

            MediaPlayer?.Close();
            MediaPlayer = null;
        }

        private void CameraNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            using (var newFrame = Image.Clone(eventArgs.Frame))
            {
                var reader = new BarcodeReader().Decode(newFrame);

                Dispatcher.Invoke(() =>
                {
                    if (reader != null)
                        QrDecoded?.Invoke(this, reader.Text);

                    CamStream.Source = newFrame.Convert();
                });
            }
        }

        private void CameraIndexSelectionChanged(int e)
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}