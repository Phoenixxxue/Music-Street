using com.ootii.Messages;
using Dll_Project.PlayVideo;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Video;

namespace Dll_Project.MusicStreet
{
    internal class videoplaer : DllGenerateBase
    {
        private VideoPlayer videoPlayer;
        private MVideoPlayer mVideoPlayer;
        private Material videoMaterial;
        private string videoName;
        private FileData videoMes;
        public override void Init()
        {
            videoMaterial = BaseMono.ExtralDataObjs[0].Target as Material;
            videoPlayer = BaseMono.ExtralDatas[0].Target.GetComponent<VideoPlayer>();
        }
        public override void Start()
        {
            mVideoPlayer = new MVideoPlayer();
            videoName = "音乐街区";
            LoadConfigJson.Instance.LoadFinishAction += () =>
            {
                string a = LoadConfigJson.Instance.MainJsonData["RoomList"].ToJson();
                List<JsonStruct> jsonRoomList = LoadConfigJson.JsonToObjectByString<List<JsonStruct>>(a);
                for (int j = 0; j < jsonRoomList.Count; j++)
                {
                    if (videoName == jsonRoomList[j].roomName)
                    {
                        videoMes = jsonRoomList[j].roomVideo;
                        break;
                    }
                }
                SendFile(videoMes.fileUrl, videoMes.fileMd5);
                Debug.LogError("SendFile");
            };
        }

        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        /// <summary>
        /// 发消息给系统，下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="sign"></param>
        private void SendFile(string url, string sign)
        {
            LocalCacheFile sendfile = new LocalCacheFile()
            {
                path = url,
                isURLSign = false,
                sign = sign,
                hasPrefix = false,
                isKOD = false,
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.SendCacheFile.ToString(), sendfile, 0.01f);
        }
        /// <summary>
        /// 视频下载完毕
        /// 存储所有视频本地地址
        /// </summary>
        private void GetCacheFile(IMessage msg)
        {
            Debug.LogError("GetCacheFile1");
            LocalCacheFile sendfile = msg.Data as LocalCacheFile;
            if (sendfile == null)
                return;
            if (sendfile.sign == videoMes.fileMd5)
            {
                mVideoPlayer.PlayNewVideo(videoPlayer, sendfile.path, videoMaterial);
                Debug.LogError("GetCacheFile2");
            }
        }
       
    }
    public class JsonStruct
    {
        public string roomName;
        public FileData roomPDF;           //展馆的PDF
        public FileData roomVideo;         //展馆的视频
    }
    public class FileData
    {
        public string fileId;
        public string fileName;
        public string fileUrl;
        public string fileMd5;
        public string fileType;
        public int fileSize;
    }
}
