using AppRTC.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using WebRtc.NET;
using WebSocketSharp;

namespace AppRTC.RctPoint
{
    public class RtcClient : IDisposable
    {
        public readonly WebRtcNative _WebRtc;
        public readonly CancellationTokenSource _Cancel;
   
        public static string socketId;

        private WebSocket _SocketClient;
        CancellationTokenSource _TurnCancel;

    
        public RtcClient(string socketUrl)
        {
            try
            {
                _SocketClient = new WebSocket(socketUrl);

                if (socketUrl.StartsWith("wss")) {
                    _SocketClient.SslConfiguration.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                }       
                //_SocketClient.
                _WebRtc = new WebRtcNative();
                _Cancel = new CancellationTokenSource();
                _TurnCancel = new CancellationTokenSource();
                SocketInitial();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }

        public static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        
        }

        public void Dispose()
        {
            _WebRtc.Dispose();
            _SocketClient.Close();
            _Cancel.Cancel();
        }

        public void SocketConnect()
        {
            try
            {
                _SocketClient.Connect();
                if (_SocketClient.IsAlive)
                {
                    //加入房间服务器 
                    JoinRoom msg = new JoinRoom
                    {
                        eventName = "__join",
                        data = new RoomData
                        {
                            room = "12345678"
                        }
                    };
                    Debug.WriteLine(JsonConvert.SerializeObject(msg));
                    _SocketClient.Send(JsonConvert.SerializeObject(msg));
                    //RtcInitial();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void RtcConnect()
        {

            try
            {
                //if (!_SocketClient.IsAlive) return;
                //RtcInitial();

                Task.Run(() =>
                {
                    var ok = _WebRtc.InitializePeerConnection();
                  
                    if (ok)
                    {
            
                        _WebRtc.CreateOffer();
                   
                        //处理返回的数据
                        while (_WebRtc.ProcessMessages(3)) { }
                        _WebRtc.ProcessMessages(3);
                    }
                    else
                    {
                        Debug.WriteLine("InitializePeerConnection failed");
                    }
                });
            }
            catch (Exception ex)
            {
            }
        }

  
        public void SetRenderRemote(Action<IntPtr, uint, uint> callback)
        {
            _OnRenderRemoteCallback = callback.Invoke;
          
        }
        private WebRtcNative.OnCallbackRender _OnRenderRemoteCallback;

     
        public void SetRenderLocal(Action<IntPtr, uint, uint> callback)
        {
            _OnRenderLocalCallback = callback.Invoke;
   
        }
        private WebRtcNative.OnCallbackRender _OnRenderLocalCallback;

    
        protected void SocketInitial()
        {
            _SocketClient.OnMessage += (s, e) =>
            {
                string data = e.Data;
                Debug.WriteLine("e.Data:" + data);
                SocketMessage message = SocketMessage.MessageFac(data);
              
                switch (message.eventName)
                {
                   

                    case SocketMessage.Peer:
                        Peer message1 = (Peer)message;
                        socketId = message1.data.you;
                        Object obj = message1.data.connections;
                  
                        Object ob=JsonConvert.DeserializeObject(obj.ToString());
                        Newtonsoft.Json.Linq.JArray ls = (Newtonsoft.Json.Linq.JArray)ob;
                        if (ls.Count > 0)
                        {
                            foreach (string sid in ls)
                            {
                                Console.WriteLine(sid);
                                RtcInitial(sid);
                             
                                RtcConnect();
                            }
                         }
                        break;
                    case SocketMessage.OfferSpd:
                       
                        SuccessAnswerMsg sdpmsg = message as SuccessAnswerMsg;
                        using (var go = new ManualResetEvent(false))
                        {
                            var t = Task.Factory.StartNew(() =>
                            {
                                var ok = _WebRtc.InitializePeerConnection();
                                if (ok)
                                {
                                    go.Set();
                                    while (_WebRtc.ProcessMessages(1000)) { }
                                    _WebRtc.ProcessMessages(1000);
                                }
                            }, _Cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                            if (go.WaitOne(9999))
                            {
                                _WebRtc.OnOfferRequest(sdpmsg.data.sdp.sdp);
                            }

                         
                        }
                       
                        break;
                    case SocketMessage.OnSuccessAnswer:
                        SuccessAnswerMsg answerMsg = message as SuccessAnswerMsg;
                    
                        break;
                    case SocketMessage.OnIceCandidate:
                        IceCandidateMsg iceMsg = message as IceCandidateMsg;
                       
                        _WebRtc.AddIceCandidate(iceMsg.data.id==null?"video": iceMsg.data.id, iceMsg.data.label, iceMsg.data.candidate);
                     
                        break;
                    case SocketMessage.Answer:
              
                        SuccessAnswerMsg answerMsg1 = message as SuccessAnswerMsg;
                        _WebRtc.OnOfferReply(answerMsg1.data.sdp.type, answerMsg1.data.sdp.sdp);
                        break;
                    case SocketMessage.NeerPeer:
                        NewPeer newpeer = message as NewPeer;
                        RtcInitial(newpeer.data.socketId);
                       
                        break;
                }
            };
            _SocketClient.OnClose += (s, e) => _Cancel.Cancel();
            _SocketClient.OnError += (s, e) => _Cancel.Cancel();
        }

     
        protected void RtcInitial()
        {
            try
            {
                WebRtcNative.InitializeSSL();
                ServerUrls.GetStunUrls().ForEach(url =>
                {
                    // Set Stun Servers
                    _WebRtc.AddServerConfig(url.url, url.userName, url.PassWord);
                });
                //_WebRtc.AddServerConfig("stun:47.254.34.146:3478", string.Empty, string.Empty);
                //_WebRtc.AddServerConfig("turn:47.254.34.146:3478?transport=tcp", "dds", "123456");
                //_WebRtc.AddServerConfig("turn:47.254.34.146:3478?transport=udp", "dds", "123456");

                // Get audio control
                _WebRtc.SetAudio(true);
                bool bb=_WebRtc.OpenVideoCaptureDevice("");
                // Set Video Capturer
                {
                    //Rectangle cptr = new Screen.PrimaryScreen.Bounds;
                    //_WebRtc.SetVideoCapturer(cptr.Width, cptr.Height, CaptureInfo.Fps);
                }
                // Relative Connection callback
                {
                    // Be triggered On Success to generate Offer
                    _WebRtc.OnSuccessOffer += (sdp) =>
                    {
                        OfferOrAnswerMsg msg = new OfferOrAnswerMsg();
                        msg.eventName = "__offer";
                        msg.data = new SdpID {
                            socketId = "bb72d3fb-92b6-4307-96b2-0dd4aa8ee0b7",
                            sdp = new SdpType {
                                type = "offer",
                                sdp = sdp
                            }
                        };

                  
                        string s = JsonConvert.SerializeObject(msg);
                        _SocketClient.Send(JsonConvert.SerializeObject(msg));
                    };

                    // Be triggered On Success to generate ICE
                    _WebRtc.OnIceCandidate += (sdp_mid, sdp_mline_index, sdp) =>
                    {
             
                        IceCandidate msg = new IceCandidate();
                        msg.eventName = "__ice_candidate";
                        msg.data = new IceCandidateData
                        {
                            id = sdp_mid,
                            label = sdp_mline_index,
                            socketId = "bb72d3fb-92b6-4307-96b2-0dd4aa8ee0b7",
                            candidate = sdp
                        };

                        _SocketClient.Send(JsonConvert.SerializeObject(msg));
                    };

                    // Be triggered On OfferRequest
                    _WebRtc.OnSuccessAnswer += (sdp) =>
                    {
                        OfferOrAnswerMsg msg = new OfferOrAnswerMsg();
                        msg.eventName = "__answer";
                        msg.data = new SdpID
                        {
                            socketId = "bb72d3fb-92b6-4307-96b2-0dd4aa8ee0b7",
                            sdp = new SdpType
                            {
                                type = "answer",
                                sdp = sdp
                            }
                        };
                        //SuccessAnswerMsg msg = new SuccessAnswerMsg
                        //{
                        //    Command = SocketMessage.OnSuccessAnswer,
                        //    SDP = sdp
                        //};
                        _SocketClient.Send(JsonConvert.SerializeObject(msg));
                    };
                }
                // Set Local and Remote callback
                {
                    _WebRtc.OnRenderLocal += _OnRenderLocalCallback;
                    _WebRtc.OnRenderRemote += _OnRenderRemoteCallback;
                }
                // Set Other callback
                {
                    _WebRtc.OnSuccessAnswer += (sdp) => { Debug.WriteLine($"Success Get SDP:{sdp}"); };
                    _WebRtc.OnFailure += (failure) => { Debug.WriteLine($"Failure Msg:{failure}"); };
                    _WebRtc.OnError += (error) => { Debug.WriteLine($"Error Msg:{error}"); };
                    _WebRtc.OnDataMessage += (dmsg) => { Debug.WriteLine($"OnDataMessage: {dmsg}"); };
                    _WebRtc.OnDataBinaryMessage += (dmsg) => { Debug.WriteLine($"OnDataBinaryMessage: {dmsg.Length}"); };
                }
            }
            catch (Exception ex) { }
        }

        protected void RtcInitial(string Sockid)
        {
            try
            {
                WebRtcNative.InitializeSSL();
                ServerUrls.GetStunUrls().ForEach(url =>
                {
                    // Set Stun Servers
                    _WebRtc.AddServerConfig(url.url, url.userName, url.PassWord);
                });
                //_WebRtc.AddServerConfig("stun:47.254.34.146:3478", string.Empty, string.Empty);
                //_WebRtc.AddServerConfig("turn:47.254.34.146:3478?transport=tcp", "dds", "123456");
                //_WebRtc.AddServerConfig("turn:47.254.34.146:3478?transport=udp", "dds", "123456");

                // Get audio control
                _WebRtc.SetAudio(true);
                string[] sarray=WebRtcNative.VideoDevices();
                foreach (string s in sarray)
                {
                    bool b = _WebRtc.OpenVideoCaptureDevice(s);
                }

                // Set Video Capturer
                {
                    //Rectangle cptr = new Screen.PrimaryScreen.Bounds;
                    //_WebRtc.SetVideoCapturer(cptr.Width, cptr.Height, CaptureInfo.Fps);
                }
                // Relative Connection callback
                {
                    // Be triggered On Success to generate Offer
                    _WebRtc.OnSuccessOffer += (sdp) =>
                    {
                        OfferOrAnswerMsg msg = new OfferOrAnswerMsg();
                        msg.eventName = "__offer";
                        msg.data = new SdpID
                        {
                            socketId = Sockid,
                            sdp = new SdpType
                            {
                                type = "offer",
                                sdp = sdp
                            }
                        };

                        //OfferSpdMsg msg = new OfferSpdMsg
                        //{
                        //    Command = SocketMessage.OfferSpd,
                        //    SDP = sdp
                        //};
                        //发送offer
                        string s = JsonConvert.SerializeObject(msg);
                        _SocketClient.Send(JsonConvert.SerializeObject(msg));
                    };
               
                    // Be triggered On Success to generate ICE
                    _WebRtc.OnIceCandidate += (sdp_mid, sdp_mline_index, sdp) =>
                    {
                        //IceCandidateMsg msg = new IceCandidateMsg
                        //{
                        //    Command = SocketMessage.OnIceCandidate,
                        //    SDP = sdp,
                        //    SdpMid = sdp_mid,
                        //    SdpMlineIndex = sdp_mline_index,
                        //};
                        IceCandidate msg = new IceCandidate();
                        msg.eventName = "__ice_candidate";
                        msg.data = new IceCandidateData
                        {
                            id = sdp_mid,
                            label = sdp_mline_index,
                            socketId = Sockid,
                            candidate = sdp
                        };

                        _SocketClient.Send(JsonConvert.SerializeObject(msg));
                    };

                    // Be triggered On OfferRequest
                    _WebRtc.OnSuccessAnswer += (sdp) =>
                    {
                        OfferOrAnswerMsg msg = new OfferOrAnswerMsg();
                        msg.eventName = "__answer";
                        msg.data = new SdpID
                        {
                            socketId = Sockid,
                            sdp = new SdpType
                            {
                                type = "answer",
                                sdp = sdp
                            }
                        };
                        //SuccessAnswerMsg msg = new SuccessAnswerMsg
                        //{
                        //    Command = SocketMessage.OnSuccessAnswer,
                        //    SDP = sdp
                        //};
                        _SocketClient.Send(JsonConvert.SerializeObject(msg));
                    };
                }
                // Set Local and Remote callback
                {
                    _WebRtc.OnRenderLocal += (rgb, w, d) =>
                    {
                        //SetRenderLocal((r, w1, h1) =>
                        //{
                        //    r = rgb;
                        //    w1 = w;
                        //    h1 = d;
                        //    //RefreshLocalView();
                        //});
                        //Action<IntPtr, uint, uint> act = ((r, w1, d1) =>
                        //{
                        //    r = rgb;
                        //    w1 = w;
                        //    d1 = d;
                        //});
                        RenderLocal(rgb, w, d);
                        //SetRenderLocal(act);
                        //Debug.WriteLine("OnRenderLocal:"+rgb);
                        //Thread.Sleep(100);
                    };

                    _WebRtc.OnRenderRemote += (rgb, w, d) =>
                     {
                         RenderRemote(rgb, w, d);
                         //Debug.WriteLine("OnRenderRemote:" + rgb);
                        // Thread.Sleep(100);
                     };

                    // _WebRtc.OnRenderLocal += _OnRenderLocalCallback;
                    //_WebRtc.OnRenderRemote += _OnRenderRemoteCallback;
                }
                // Set Other callback
                {
                    _WebRtc.OnSuccessAnswer += (sdp) => { Debug.WriteLine($"Success Get SDP:{sdp}"); };
                    _WebRtc.OnFailure += (failure) => { Debug.WriteLine($"Failure Msg:{failure}"); };
                    _WebRtc.OnError += (error) => { Debug.WriteLine($"Error Msg:{error}"); };
                    _WebRtc.OnDataMessage += (dmsg) => { Debug.WriteLine($"OnDataMessage: {dmsg}"); };
                    _WebRtc.OnDataBinaryMessage += (dmsg) => { Debug.WriteLine($"OnDataBinaryMessage: {dmsg.Length}"); };
                }
            }
            catch (Exception ex) { }
        }

        public event WebRtcNative.OnCallbackRender RenderLocal;

        public event WebRtcNative.OnCallbackRender RenderRemote;

        private void _WebRtc_OnRenderRemote(IntPtr BGR24, uint w, uint h)
        {
            throw new NotImplementedException();
        }

        private void _WebRtc_OnRenderLocal(IntPtr BGR24, uint w, uint h)
        {
            throw new NotImplementedException();
        }
    }
}
