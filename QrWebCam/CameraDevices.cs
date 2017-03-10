using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrWebCam
{
    using AForge.Video.DirectShow;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

   
        public class CameraDevices : INotifyPropertyChanged
        {
            public CameraDevices()
            {
                GatherDevices();
            }

            public void GatherDevices() => devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            private FilterInfoCollection devices;
            public VideoCaptureDevice Current { get; set; }
            public FilterInfoCollection Devices
            {
                get
                {
                    return devices;
                }
                set
                {
                    devices = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Devices"));
                }
            }
            /// <summary>
            /// Gets all camera devices names from this system
            /// </summary>
            /// <returns></returns>
            public List<string> GetStringCameras()
            {
                if (devices.Count <= 0) return new List<string>();
                List<string> dvs = new List<string>();
                for (int i = 0; i < devices.Count; i++)
                {
                    dvs.Add(Devices[i].Name);
                }
                return dvs;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Activates a camera based on returned index from the GetStringCameras
            /// </summary>
            /// <param name="index"></param>
            public void SelectCamera(int index)
            {
                if (index >= Devices.Count) throw new ArgumentOutOfRangeException("Index is illegal");
                Current = new VideoCaptureDevice(Devices[index].MonikerString);
            }
        }

}
