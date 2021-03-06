using AppRTC.RctPoint;
using Capture;
using Client.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowList.ItemsSource = new WindowList(CaptureHelper.GetHandlingWindowList());
            _Capturer = ScreenCapture.Create(HandlingPtr);
        }

        IntPtr HandlingPtr = IntPtr.Zero;
        RtcClient _ClientPeer;
        Bitmap _RemoteView;
        Bitmap _ScreenView;
        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_ClientPeer == null)
            {
                _ClientPeer = new RtcClient(SocketUrl.Text);
                // Set RenderLocal callback

                _ClientPeer.RenderLocal += (rgb, w, h) =>
                  {
                      _ScreenView= new Bitmap((int)w, (int)h, (int)w * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, rgb);
                      //_RemoteView = new Bitmap((int)w, (int)h, (int)w * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, rgb);
                      //RefreshRemoteView();
                      RefreshLocalView();
                  };

                //_ClientPeer.SetRenderLocal((BGR24, w, h) =>
                //{
                //    _ScreenView = new Bitmap((int)w, (int)h, (int)w * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, BGR24);
                //    //_RemoteView = new Bitmap((int)w, (int)h, (int)w * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, rgb);
                //    //RefreshRemoteView();
                //    //RefreshLocalView();
                //    //RefreshLocalView();
                //});
                // Set RenderRemote callback
                //_ClientPeer.SetRenderRemote((BGR24, w, h) =>
                //{
                //    _RemoteView = new Bitmap((int)w, (int)h, (int)w * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, BGR24);
                //    RefreshRemoteView();
                //});
                _ClientPeer.RenderRemote += (BGR24, w, h) =>
                 {
                     _RemoteView = new Bitmap((int)w, (int)h, (int)w * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, BGR24);
                     RefreshRemoteView();
                 };
            }

            try
            {
                _ClientPeer.SocketConnect();

                //StartScrean();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Socket Location:{SocketUrl.Text}");
                _ClientPeer.Dispose();
                _ClientPeer = null;
            }
        }

        Timer _ViewCaptureTrigger;
        ScreenCapture _Capturer;
        private void CaptureBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_ViewCaptureTrigger != null)
                {
                    _Capturer.ResetHandlingPointer(HandlingPtr);
                    //_ViewCaptureTrigger.Enabled = !_ViewCaptureTrigger.Enabled;
                    return;
                }
                _ViewCaptureTrigger = new Timer { Interval = 1000 / CaptureInfo.Fps, };
                _ViewCaptureTrigger.Elapsed += (s, o) =>
                {
                    //_ScreenView = _Capturer.CaptureView();
                    RefreshLocalView();
                };
                _ViewCaptureTrigger.Enabled = true;
            }
            catch (Exception ex)
            { }
        }


        Timer _ViewPushTrigger;
        private void BuildRtc_Click(object sender, RoutedEventArgs e)
        {
            if (_ClientPeer == null) return;

            if (_ViewPushTrigger != null)
            {
                _ViewPushTrigger.Enabled = !_ViewPushTrigger.Enabled;
                //return;
            }
            StartScrean();
            //_ViewPushTrigger = new Timer { Interval = 1000 / CaptureInfo.Fps, };
            _ViewPushTrigger.Elapsed += (s, o) =>
            {
                //_ScreenView = _Capturer.CaptureView();
                _ClientPeer._WebRtc.PushFrame(_Capturer.ImgBufPtr);
                //RefreshRemoteView();
            };
            _ClientPeer.RtcConnect();
            //_ViewPushTrigger.Enabled = true;
        }

        public void StartScrean()
        {
            _ViewPushTrigger = new Timer { Interval = 1000 / CaptureInfo.Fps, };
            _ViewPushTrigger.Elapsed += (s, o) =>
            {
                //_ScreenView = _Capturer.CaptureView();
                _ClientPeer._WebRtc.PushFrame(_Capturer.ImgBufPtr);
                //RefreshRemoteView();
            };
            _ViewPushTrigger.Enabled = true;
        }


        private void RefreshLocalView()
        {
            this.LocalView.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (_ScreenView == null) return;
                    LocalView.Source = CaptureHelper.BitmapToBitmapSource(_ScreenView);
                    //LocalView.Source = CaptureHelper.BitmapToBitmapSource(_ScreenView);
                }
                catch (Exception ex)
                {
                    Debug.Write($"Refresh Error:{ex.Message}");
                }
            }));
        }

        private void RefreshRemoteView()
        {
            this.RemoteView.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (_RemoteView == null) return;
                    RemoteView.Source = CaptureHelper.BitmapToBitmapSource(_RemoteView);
                }
                catch (Exception ex)
                {
                    Debug.Write($"Refresh Error:{ex.Message}");
                }
            }));
        }

        private void WindowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WindowInfo wi = ((ListBox)sender).SelectedItem as WindowInfo;
            HandlingPtr = wi.HandlingPtr;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ClientPeer.Dispose();
        }
    }
}
