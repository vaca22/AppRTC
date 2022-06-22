using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppRTC.Model
{

    public class WebRtcMsg
    {
        public string eventName;
        public string Data;
    }

    public class SocketID
    {
        public string socketId;
    }

    public class ConnectMsg
    {
        public Object connections;

        public string you;
    }

    public class OfferMsg
    {
        public string socketId;

        public string sdp;
    }

    public class JoinRoom
    {
        public string eventName;

        public RoomData data;

    }

    public class RoomData
    {
        public string room;

    }


    public class OfferOrAnswerMsg
    {
        public string eventName;//_answer or _offer

        public SdpID data;
    }

    public class IceCandidate
    {
        public string eventName; //__ice_candidate

        public IceCandidateData data;  

        //public string
    } 

    public class IceCandidateData
    {
        public string id;//sdpMid

        public int label;// iceCandidate.sdpMLineIndex

        public string candidate;

        public string socketId;//
    }
     

    public class SdpID
    {
        public string socketId;

        public SdpType sdp;//SdpType
    }

    public class SdpType
    {
        public string type;

        public string sdp;
    }

}
