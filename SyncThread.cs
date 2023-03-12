using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common;
using Photon.SocketServer;

namespace PhotonServerDemo
{
   public class SyncThread
    {
        //create Thread Object
        private Thread thread;

        public void RunThread()
        {
            thread = new Thread(UpdatePosRotInfo);

            thread.Start();
        }

        public void StopThread()
        {
            thread.Abort();
        }


        //Called by a thread for synchronizing 
        private void UpdatePosRotInfo()
        {

            MyServer.log.Info("Start Sync Thread");
            //Wait for 4 second
            Thread.Sleep(4000);

            while (true)
            {
                Thread.Sleep(10);

                //
                List<PlayerData> playerDatas = new List<PlayerData>();

                for (int i = 0; i < MyServer.roomList.Count; i++)
                {
                    PlayerData playerData = new PlayerData();
                    playerData.Username = MyServer.roomList[i].Username_Current;
                    playerData.Pos = MyServer.roomList[i].Vector3Data_Current;
                    playerData.Rot = MyServer.roomList[i].RotData_Current;



                    playerDatas.Add(playerData);
                }
                using (StringWriter sw = new StringWriter())
                {
                    //create serialization object
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PlayerData>));
                    //use this object to serialize
                    xmlSerializer.Serialize(sw, playerDatas);

                    //transform serialized xml to string
                    string playerDataListString = sw.ToString();
                    Dictionary<byte, object> data = new Dictionary<byte, object>();
                    data.Add((byte)ParameterCode.PlayerData, playerDataListString);

                    EventData eventData = new EventData((byte)EventCode.SyncPosRot);
                    eventData.Parameters = data;

                    for (int i = 0; i < MyServer.roomList.Count; i++)
                    {
                        MyServer.roomList[i].SendEvent(eventData, new SendParameters());
                    }

                }

            }
        }

    }
}
