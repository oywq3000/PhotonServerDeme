using System;
using System.Collections.Generic;
using System.IO;

using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net.Config;
using Photon.SocketServer;

namespace PhotonServerDemo
{
    /// <summary>
    /// Main Server Class
    /// </summary>
    public class MyServer : ApplicationBase
    {

        //set static for using outside
        public static ILogger log = LogManager.GetCurrentClassLogger();

        //store all online client
        public static List<MyClient> peerList = new List<MyClient>();
        public static List<MyClient> roomList = new List<MyClient>();

        /// <summary>
        /// peer to peer :端对端
        /// call this method when users connect this server
        /// </summary>
        /// <param name="initRequest"></param>
        /// <returns></returns>
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {

            log.Info("One client connect from Game Development Class 1");
            var myClient = new MyClient(initRequest);
            peerList.Add(myClient);
            return myClient;
        }

        /// <summary>
        /// call when server start
        /// </summary>
        protected override void Setup()
        {

            //init log
            InitLog();

            log.Info("Start MyServer");
        }

        /// <summary>
        /// call when server close 
        /// </summary>
        protected override void TearDown()
        {
            log.Info("Close MyServer");
        }

        private void InitLog()
        {
            //steps one:set log generative path :Photon:ApplicationLogPath
            log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] =
             Path.Combine(Path.Combine(this.ApplicationRootPath, "bin_Win64"), "log");

            //steps two, enable PhotonServer knows our using plugin :Log4net,and read it
            FileInfo fileInfo = new FileInfo(Path.Combine(this.BinaryPath, "log4net.config"));

            if (fileInfo.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(fileInfo);
            }
        }
    }
}
