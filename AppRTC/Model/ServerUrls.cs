using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppRTC.Model
{
    class ServerUrls
    {
        const string stun_1 = "stun:47.254.34.146:3478";
        const string stun_2 = "turn:47.254.34.146:3478?transport=tcp";
        const string stun_3 = "turn:47.254.34.146:3478?transport=udp";

        //public static List<string> GetStunUrls()
        //{
        //    return new List<string> { stun_1, stun_2, stun_3 };
        //}

        public static List<ServerConfig> GetStunUrls()
        {
            return new List<ServerConfig> { new ServerConfig(){url=stun_1,userName=string.Empty,PassWord=string.Empty},
                                            new ServerConfig(){url=stun_2,userName="dds",PassWord="123456"},
                                            new ServerConfig(){url=stun_3,userName="dds",PassWord="123456"} };
        }
    }

    class ServerConfig
    {
        public string url { get; set; }

        public string userName { get; set; }

        public string PassWord { get; set; }
    }
}
