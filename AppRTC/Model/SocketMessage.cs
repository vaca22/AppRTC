using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppRTC.Model
{
    abstract class SocketMessage
    {
        public const string OnSuccessAnswer = "OnSuccessAnswer";
        public const string OnIceCandidate = "_ice_candidate";
      
        public const string OfferSpd = "_offer";
        public const string RemovePeer = "_remove_peer";
        public const string NeerPeer = "_new_peer";
        public const string Peer = "_peers";
        public const string Answer = "_answer";

   

        public string eventName { get; set; }

        public static SocketMessage MessageFac(string json)
        {
         
            string cmd = JObject.Parse(json)["eventName"].ToString();

            switch (cmd)
            {
                case Peer:
                    Peer peer = new Peer();
                    peer.eventName = cmd;
                    return JsonConvert.DeserializeObject<Peer>(json);
                case RemovePeer:
                    return JsonConvert.DeserializeObject<RemovePeer>(json);
                case NeerPeer:
                    return JsonConvert.DeserializeObject<NewPeer>(json);
                case OfferSpd:
                    return JsonConvert.DeserializeObject<SuccessAnswerMsg>(json);
                case OnSuccessAnswer:
                    return JsonConvert.DeserializeObject<SuccessAnswerMsg>(json);
                case OnIceCandidate:
                    return JsonConvert.DeserializeObject<IceCandidateMsg>(json);
                case Answer:
                    return JsonConvert.DeserializeObject<SuccessAnswerMsg>(json);
                default:
                    return null;
            }
        }
    }

    class Peer : SocketMessage {
        public ConnectMsg data;
    }
    
    class RemovePeer: SocketMessage
    {
        public SocketID data;
    }

    class MySocketID : SocketMessage
    {
        
    }

    class NewPeer : SocketMessage
    {
        public SocketID data;
    }

    class OfferSpdMsg : SocketMessage {
        public OfferMsg data;
    }

    class SuccessAnswerMsg : SocketMessage
    {
        public SdpID data;
    }

    class IceCandidateMsg : SocketMessage
    {
        public IceCandidateData data;
    }



}
