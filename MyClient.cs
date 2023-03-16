using Common;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using PhotonServerDemo.Model;
using System;
using System.Collections.Generic;
using System.IO; //file read
using System.Xml.Serialization; //Serialization:序列化

namespace PhotonServerDemo
{
    public class MyClient : ClientPeer
    {
        //perserve current client name
        private string username_Current;

        //perserve current player position info
        private Vector3Data vector3Data_Current;
        private Vector3Data rotData_Current;

        public string Username_Current { get => username_Current; set => username_Current = value; }
        public Vector3Data Vector3Data_Current { get => vector3Data_Current; set => vector3Data_Current = value; }
        public Vector3Data RotData_Current { get => rotData_Current; set => rotData_Current = value; }

        public MyClient(InitRequest initRequest) : base(initRequest) { }
        //call when client disconnect
        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            MyServer.log.Info("One client disconnect from Game Development Class 1");

            //Exception exit
            CheckAndExitRoom(this);
            //remove disconnected client
            MyServer.peerList.Remove(this);

        }

        //operation the request from client
        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            #region client test

            //if (operationRequest.OperationCode.Equals(18))
            //{ 
            //    MyServer.log.Info("Request from client:" + operationRequest.Parameters[5]);

            //   //response for client
            //    Dictionary<byte, object> data = new Dictionary<byte, object>();
            //    data.Add(15, "Response from Server");

            //    //Response from this request of operation
            //    OperationResponse response = new OperationResponse(18);

            //    response.SetParameters(data);

            //    //call only response the request from client
            //    SendOperationResponse(response, sendParameters);

            //}

            #endregion
            switch (operationRequest.OperationCode)
            {
                case (byte)OperationCode.Login:
                    OnHandleLoginRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.Register:
                    OnHandleRegisterRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.LogOut:
                    OnHandleLogOutRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.SyncSpawnPlayer:
                    OnHandleSyncSpawnPlayerRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.SyncPosInfo:
                    OnHandleSyncPosInfoRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.SyncRotInfo:
                    OnHandleSyncRotInfoRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.SyncAttack:
                    OnHandleSyncAttackRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.EntryRoom:
                    OnHandleEntryRoomRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.ExitRoom:
                    OnHandleExitRoomRequest(operationRequest, sendParameters);
                    break;
                case (byte)OperationCode.SyncGameOver:
                    OnHandleGameOverRequest(operationRequest, sendParameters);
                    break;
                default: break;
            }
        }
        private void OnHandleLoginRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Get Uer name and age

            object username, age;
            operationRequest.Parameters.TryGetValue((byte)ParameterCode.Username, out username);
            operationRequest.Parameters.TryGetValue((byte)ParameterCode.Age, out age);

            //response object
            OperationResponse response = new OperationResponse((byte)OperationCode.Login);

            UserController controller = new UserController();
            if (controller.Verify(username.ToString(), int.Parse(age.ToString())))
            {

                //verify successfuly and perserve current username
                username_Current = username.ToString();

                response.ReturnCode = (short)ReturnCode.Success;


                //kick out the same accound
                foreach (var myClient in MyServer.peerList)
                {
                    if (myClient.username_Current == username_Current && myClient != this)
                    {
                        CheckAndExitRoom(myClient);

                        EventData eventData = new EventData((byte)EventCode.KickOut);
                        myClient.SendEvent(eventData, sendParameters);
                        break;
                    }
                }

                MyServer.log.Info($"Login Request: {username} {age}, request pass");
            }
            else
            {
                //verify failed
                response.ReturnCode = (short)ReturnCode.Failed;
                MyServer.log.Info($"Login Request: {username} {age}, request fail");
            }


            SendOperationResponse(response, sendParameters);
        }
        private void OnHandleRegisterRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Get Uer name and age

            object username, age;
            operationRequest.Parameters.TryGetValue((byte)ParameterCode.Username, out username);
            operationRequest.Parameters.TryGetValue((byte)ParameterCode.Age, out age);

            //response object
            OperationResponse response = new OperationResponse((byte)OperationCode.Register);

            UserController controller = new UserController();

            User user = new User() { Name = username.ToString(), Age = Int32.Parse(age.ToString()) };



            if (controller.Add(user))
            {
                //verify successfuly

                response.ReturnCode = (short)ReturnCode.Success;
                MyServer.log.Info($"Register Request: {username} {age}, Register Successfully");
            }
            else
            {
                //verify failed
                response.ReturnCode = (short)ReturnCode.Failed;
                MyServer.log.Info($"Register Request: {username} {age}, Register fail");
            }


            SendOperationResponse(response, sendParameters);
        }
        private void OnHandleLogOutRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Get Uer name and age

            object username;
            operationRequest.Parameters.TryGetValue((byte)ParameterCode.Username, out username);


            //response object
            OperationResponse response = new OperationResponse((byte)OperationCode.LogOut);

            UserController controller = new UserController();


            if (controller.DeleteByUserName(username.ToString()))
            {
                //verify successfuly
                response.ReturnCode = (short)ReturnCode.Success;
                MyServer.log.Info($"LogOut Request: {username}, Register Successfully");
            }
            else
            {
                //verify failed
                response.ReturnCode = (short)ReturnCode.Failed;
                MyServer.log.Info($"LogOut Request: {username}, Register fail");
            }


            SendOperationResponse(response, sendParameters);
        }

        private void OnHandleSyncSpawnPlayerRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {

            //get all onlined username
            List<string> usernameList = new List<string>();
            foreach (var myClient in MyServer.roomList)
            {
                //except self
                if (myClient != this)
                {
                    usernameList.Add(myClient.username_Current);
                    MyServer.log.Info(" Peer Count:" + MyServer.peerList.Count);

                }
            }

            //1、response to current client and send usernameList to it

            using (StringWriter sw = new StringWriter())
            {
                //create serialization object
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<string>));
                //use this object to serialize
                xmlSerializer.Serialize(sw, usernameList);

                //transform serialized xml to string
                string usrnameListString = sw.ToString();

                MyServer.log.Info($"Onlined palyer name list:" + usrnameListString);
                Dictionary<byte, object> data = new Dictionary<byte, object>();
                data.Add((byte)ParameterCode.UsernameList, usrnameListString);
                OperationResponse response = new OperationResponse(operationRequest.OperationCode);
                response.SetParameters(data);
                SendOperationResponse(response, sendParameters);
            }




            //2、broadcast to all onlined player for making them know you log in
            foreach (MyClient myClient in MyServer.roomList)
            {
                if (myClient != this)
                {
                    //Create push event object
                    EventData eventData = new EventData((byte)EventCode.SyncSpawnPlayer);
                    Dictionary<byte, object> data = new Dictionary<byte, object>();
                    data.Add((byte)ParameterCode.Username, username_Current);
                    eventData.Parameters = data;

                    //push event for synchronizing current player to other client
                    myClient.SendEvent(eventData, sendParameters);
                }
            }

        }

        private void OnHandleSyncPosInfoRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //get serialized file sending form client
            object posObj;
            operationRequest.Parameters.TryGetValue((byte)ParameterCode.PositionInfo, out posObj);

            using (StringReader reader = new StringReader(posObj.ToString()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Vector3Data));

                vector3Data_Current = (Vector3Data)xmlSerializer.Deserialize(reader);

                // MyServer.log.Info($"get {username_Current} position :(" + vector3Data_Current.x + "," + vector3Data_Current.y + "," + vector3Data_Current.z + ")");
            }

        }
        private void OnHandleSyncRotInfoRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //get serialized file sending form client
            object posObj;
            operationRequest.Parameters.TryGetValue((byte)ParameterCode.RotationInfo, out posObj);

            using (StringReader reader = new StringReader(posObj.ToString()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Vector3Data));

                rotData_Current = (Vector3Data)xmlSerializer.Deserialize(reader);

                // MyServer.log.Info($"get {username_Current} Rotation :(" + vector3Data_Current.x + "," + vector3Data_Current.y + "," + vector3Data_Current.z + ")");
            }

        }

        private void OnHandleSyncAttackRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //Attack request for this player who are in the room list 
            for (int i = 0; i < MyServer.roomList.Count; i++)
            {
                EventData eventData = new EventData((byte)EventCode.SyncAttack);

                Dictionary<byte, object> data = new Dictionary<byte, object>();

                data.Add((byte)ParameterCode.Username, this.username_Current);

                eventData.SetParameters(data);

                MyServer.roomList[i].SendEvent(eventData, sendParameters);
            }
        }
        private void OnHandleEntryRoomRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            MyServer.roomList.Add(this);

            if (rotData_Current != null || vector3Data_Current != null)
            {
                PlayerData playerData = new PlayerData() { Username = username_Current, Pos = vector3Data_Current, Rot = rotData_Current };
                //Sync pos and rot
                using (StringWriter sw = new StringWriter())
                {
                    //create serialization object
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlayerData));
                    //use this object to serialize
                    xmlSerializer.Serialize(sw, playerData);

                    //transform serialized xml to string
                    string playerDataString = sw.ToString();
                    Dictionary<byte, object> data = new Dictionary<byte, object>();
                    data.Add((byte)ParameterCode.PlayerData, playerDataString);

                    EventData eventData = new EventData((byte)EventCode.EntryRoom);
                    eventData.Parameters = data;
                    this.SendEvent(eventData, new SendParameters());
                }
            }


            MyServer.log.Info(this.username_Current + " Entry Room and Room Player Count:" + MyServer.roomList.Count);
        }
        private void OnHandleExitRoomRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            CheckAndExitRoom(this);
        }

        private void CheckAndExitRoom(MyClient myClient)
        {
            //Exception exit
            if (MyServer.roomList.Contains(myClient))
            {

                MyServer.roomList.Remove(myClient);

                MyServer.log.Info($"{myClient.username_Current} Exit Room:" + MyServer.roomList.Count);


                //notify other roomed player that you exit room
                foreach (var player in MyServer.roomList)
                {
                    MyServer.log.Info($"Exception: Notify {player.username_Current} that {username_Current} leave room");

                    EventData eventData = new EventData((byte)EventCode.ExitRoom);
                    Dictionary<byte, object> data = new Dictionary<byte, object>();
                    data.Add((byte)ParameterCode.Username, username_Current);
                    eventData.Parameters = data;
                    player.SendEvent(eventData, new SendParameters());
                }
            }
        }

        private void OnHandleGameOverRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            //remove this from room
            MyServer.roomList.Remove(this);

            //synchronize the data from client to data base 
            object killCount;
            operationRequest.Parameters.TryGetValue((byte)ParameterCode.KillCount, out killCount);
            int killCountNum = int.Parse(killCount.ToString());
            MyServer.log.Info("this game kill count:" + killCountNum);
        }
    }
}
