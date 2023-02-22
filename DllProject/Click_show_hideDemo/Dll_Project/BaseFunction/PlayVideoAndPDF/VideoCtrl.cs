using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Video;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.PlayVideo
{
    internal class VideoCtrl : DllGenerateBase
    {
        private List<string> videoURL;          //视频URL
        private List<string> videoMD5;          //视频MD5 
        private List<string> videoPath;         //视频下载后的本地地址
        private RawImage RawImage_Player;
        public Transform MainVideo;      //视频播放器载体
        public VideoPlayer vp;           //视频播放器组件
        //private Material material;     //播放材质
        MVideoPlayer mVideoPlayer;

        //private List<>
        public override void Init()
        {
            base.Init();
            videoURL = new List<string>();         
            videoMD5 = new List<string>();
            videoPath = new List<string>();
            
            MainVideo = BaseMono.ExtralDatas[4].Target;
            vp = MainVideo.GetComponent<VideoPlayer>();

            for (int i = 0; i < BaseMono.ExtralDatas[0].Info.Length; i++)
            {
                videoURL.Add(BaseMono.ExtralDatas[0].Info[i].OtherData);
                videoMD5.Add(BaseMono.ExtralDatas[1].Info[i].OtherData);

                videoPath.Add("");
            }
            RawImage_Player = BaseMono.ExtralDatas[3].Target.gameObject.GetComponent<RawImage>();
            // material = BaseMono.ExtralDataObjs[0].Target as Material;
        }
        public override void Start()
        {
            base.Start();
            mVideoPlayer = new MVideoPlayer();
            SendFile(videoURL[0], videoMD5[0]);
            //mVideoPlayer.PlayNewVideo(vp, videoURL[0], RawImageList[0]);
        }
        public override void OnEnable()
        {
            //监听文件下载成功事件
            MessageDispatcher.AddListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
        }
        public override void OnDisable()
        {
            //监听文件下载成功事件
            MessageDispatcher.RemoveListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
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
            LocalCacheFile sendfile = msg.Data as LocalCacheFile;
            if (sendfile == null)
                return;
            for(int i=0;  i< videoMD5.Count; i++)
            {
                if (sendfile.sign == videoMD5[i])
                {
                    videoPath[i] = sendfile.path;
                    if(i+1<videoURL.Count)
                    {
                        SendFile(videoURL[i+1], videoMD5[i+1]);
                    }
                    return;
                }
            }
        }
        public void PlayRoomVideo(int index)
        {
            mVideoPlayer.PlayNewVideo(vp, videoPath[index], RawImage_Player);
        }           
    }
}
