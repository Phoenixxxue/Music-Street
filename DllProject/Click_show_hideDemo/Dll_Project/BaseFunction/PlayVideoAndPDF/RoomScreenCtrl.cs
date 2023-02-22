using com.ootii.Messages;
using Dll_Project.BaseUI;
using Dll_Project.PlayVideo;
using HutongGames.PlayMaker;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Dll_Project.PlayVideoAndPDF
{
    public class PDFVideoMessage
    {
        public string name;
        public string url;
        public string md5;
    }
    /// <summary>
    /// 切换视频和PDF按钮
    /// model = 0 默认  1 播放PDF  2 播放视频
    /// </summary>
    public class RoomStruct 
    {
       public GameObject boxBtn;
       public GameObject TriggerPlane;
       public RawImage RawImage_Room;
       public bool StartPlay;
       public int model;
       public bool ModelChanged;
       public PDFVideoMessage pdfMes;
       public PDFVideoMessage videoMes;
       public Texture defaultTexture;
        public GameObject PDFBtn;
        public GameObject VideoBtn;
    }
    public class RoomScreenCtrl : DllGenerateBase
    {
        #region 初始化
        private List<RoomStruct> RoomList;

        private List<GameObject> ScreenList;    //父级canves
        private List<Button> btn_PrePageList;   //上一页按钮
        private List<Button> btn_NextPageList;  //下一页按钮

        private RawImage RawImage_Player;

        private int count;
        private string jsonPath;

        public List<PDFVideoMessage> pdfInfoList;
        public List<PDFVideoMessage> videoInfoList;

        private PDFPlayer PDFPlayer;
        private MVideoPlayer mVideoPlayer;

        private int curPageIndex;
        public VideoPlayer vp;           //视频播放器组件
        /// <summary>
        /// 初始化
        /// </summary>
        public override void Init()
        {
            curPageIndex = -1;

            jsonPath = BaseMono.ExtralDatas[4].OtherData;
            vp = BaseMono.ExtralDatas[5].Target.GetComponent<VideoPlayer>();
            RawImage_Player = BaseMono.ExtralDatas[3].Target.GetComponent<RawImage>();

            PDFPlayer    = new PDFPlayer();
            mVideoPlayer = new MVideoPlayer();  
            RoomList   = new List<RoomStruct>();
            ScreenList = new List<GameObject>();
            btn_PrePageList  = new List<Button>();
            btn_NextPageList = new List<Button>();

            for (int i = 0; i < BaseMono.ExtralDatas[0].Info.Length; i++)
            {
                ScreenList.Add(BaseMono.ExtralDatas[6].Info[i].Target.gameObject);
                RoomStruct playRoom = new RoomStruct();
                playRoom.TriggerPlane = BaseMono.ExtralDatas[0].Info[i].Target.gameObject;                  //房间地板 
                playRoom.boxBtn = BaseMono.ExtralDatas[1].Info[i].Target.gameObject;                        //切换按钮
                playRoom.RawImage_Room = BaseMono.ExtralDatas[2].Info[i].Target.GetComponent<RawImage>();   //房间屏幕
                playRoom.model = 0;                                                                         //播放类型
                playRoom.StartPlay = false;                                                                 //是否进入房间
                playRoom.ModelChanged = false;                                                                 //播放类型是否变化
                playRoom.defaultTexture = playRoom.RawImage_Room.texture;                                   //记录房间默认贴图
                playRoom.PDFBtn = ScreenList[i].transform.Find("PDFGameObject").gameObject;                 //PDF播放控制UI
                playRoom.VideoBtn = ScreenList[i].transform.Find("VideoGameObject").gameObject;             //Video播放控制UI
                RoomList.Add(playRoom);
              
                btn_PrePageList.Add(ScreenList[i].transform.Find("PDFGameObject/NextButton").GetComponent<Button>());
                btn_NextPageList.Add(ScreenList[i].transform.Find("PDFGameObject/UpButton").GetComponent<Button>());
            }
        }
        #endregion
        public override void Start()
        {
            base.Start();
            BaseMono.StartCoroutine(LoadConfigFile(jsonPath, 1f));
            //if (!mStaticThings.I.isVRApp)
            //{
            //    for (int i = 0; i < ScreenList.Count; i++)
            //    {
            //        ScreenList[i].GetComponent<Canvas>().worldCamera = mStaticThings.I.Maincamera.GetComponent<Camera>();
            //    }
            //}
            
        }
        #region 事件监听
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), OnTelePortToMesh);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.AddListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
            for (int i = 0; i < btn_PrePageList.Count; i++)
            {
                btn_PrePageList[i].onClick.AddListener(() =>
                {
                    curPageIndex = PDFPlayer.PlayPDFPage(curPageIndex + 1, null, RawImage_Player);
                });
                btn_NextPageList[i].onClick.AddListener(() =>
                {
                    curPageIndex = PDFPlayer.PlayPDFPage(curPageIndex - 1, null, RawImage_Player);
                });
            }
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), OnTelePortToMesh);
            for (int i = 0; i < btn_PrePageList.Count; i++)
            {
                btn_PrePageList[i].onClick.RemoveListener(() =>
                {
                    curPageIndex = PDFPlayer.PlayPDFPage(curPageIndex + 1, null, RawImage_Player);

                });
                btn_NextPageList[i].onClick.RemoveListener(() =>
                {
                    curPageIndex = PDFPlayer.PlayPDFPage(curPageIndex - 1, null, RawImage_Player);

                });
            }
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            MessageDispatcher.RemoveListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), OnTelePortToMesh);
            for (int i = 0; i < btn_PrePageList.Count; i++)
            {
                btn_PrePageList[i].onClick.RemoveListener(() =>
                {
                    curPageIndex = PDFPlayer.PlayPDFPage(curPageIndex + 1, null, RawImage_Player);

                });
                btn_NextPageList[i].onClick.RemoveListener(() =>
                {
                    curPageIndex = PDFPlayer.PlayPDFPage(curPageIndex - 1, null, RawImage_Player);

                });
            }
        }
        #endregion
        public override void Update()
        {
            base.Update();
            for(int i = 0; i < RoomList.Count; i++)
            {
                if(RoomList[i].StartPlay)
                {
                    RoomList[i].RawImage_Room.texture = RawImage_Player.texture;
                }
                if(RoomList[i].ModelChanged)
                {
                    //正在播放PDF
                    if (RoomList[i].model == 1)
                    {
                        mVideoPlayer.DisposeVideoPlayer(vp);
                        SendFile(RoomList[i].pdfMes.url, RoomList[i].pdfMes.md5);
                    }
                    //正在播放视频
                    else if(RoomList[i].model == 2)
                    {
                        SendFile(RoomList[i].videoMes.url, RoomList[i].videoMes.md5);
                    }
                    //播放默认图片
                    else if (RoomList[i].model == 0)
                    {
                        mVideoPlayer.DisposeVideoPlayer(vp);
                        RoomList[i].RawImage_Room.texture = RoomList[i].defaultTexture;
                    }
                    RoomList[i].ModelChanged = false;
                }
                if(RoomList[i].model != 2 && RoomList[i].VideoBtn)
                {
                    RoomList[i].VideoBtn.SetActive(false);
                }
                if (RoomList[i].model != 1 && RoomList[i].PDFBtn)
                {
                    RoomList[i].PDFBtn.SetActive(false);
                }
            }
        }
        void OnTelePortToMesh(IMessage msg)
        {
            string meshname = (string)msg.Data;
            for (int j = 0; j < RoomList.Count; j++)
            {
                if (RoomList[j].TriggerPlane.name == meshname )
                {
                    //RoomList[j].boxBtn.transform.parent.gameObject.SetActive(true); //切换按钮激活
                    RoomList[j].StartPlay = true;                                     //玩家已进入房间
                    RoomList[j].ModelChanged = true;
                    RoomList[j].model = 1;                                            //默认开始播放PDF
                                                      
                }
                if (RoomList[j].TriggerPlane.name != meshname && RoomList[j].StartPlay)
                {
                   // RoomList[j].boxBtn.transform.parent.gameObject.SetActive(false);//切换按钮隐藏
                    RoomList[j].StartPlay = false;                                    //玩家已离开房间
                    RoomList[j].ModelChanged = true;                                     
                    RoomList[j].model = 0;                                            //切换播放模式为默认图片
                    RawImage_Player.texture = null;
                }
            }
            
        }
        private void OnPointClickEvent(IMessage msg)
        {
            GameObject go = msg.Data as GameObject;
           // Debug.LogError("-------------" + go.name + "-------------");
            for (int i = 0; i < RoomList.Count; i++)
            {
                if (go.name == RoomList[i].boxBtn.name && mStaticData.IsOpenPointClick && mStaticData.IsPointAssetOrCard && RoomList[i].StartPlay)
                {                               
                    if (RoomList[i].model == 2)
                    {
                        RoomList[i].model = 1;
                    }
                    else
                    {
                        RoomList[i].model = RoomList[i].model + 1;
                    }
                    RoomList[i].ModelChanged = true; 
                    break;
                }
            }
            
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
            for (int i = 0; i < RoomList.Count; i++)
            {
                if (RoomList[i].StartPlay)
                {
                    if (RoomList[i].model == 1)//播放PDF                 
                    {                 
                        if (sendfile.sign == RoomList[i].pdfMes.md5)
                        {
                            RawImage_Player.texture = null; 
                            curPageIndex = PDFPlayer.PlayNewPDF(sendfile.path, null, RawImage_Player); 
                        }
                    }
                    else if(RoomList[i].model == 2)//播放视频
                    {       
                        if (sendfile.sign == RoomList[i].videoMes.md5)
                        {
                            RawImage_Player.texture = null;
                            mVideoPlayer.PlayNewVideo(vp, sendfile.path, RawImage_Player);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取JSON配置文件后回调事件
        /// </summary>
        /// <param name="jsonData"></param>
        private void JsonCallback(JsonData jsonData)
        {
            pdfInfoList = JsonMapper.ToObject<List<PDFVideoMessage>>(jsonData["RoonScreen"]["PDFMessage"].ToJson());
            videoInfoList = JsonMapper.ToObject<List<PDFVideoMessage>>(jsonData["RoonScreen"]["VideoMessage"].ToJson());
            for(int i = 0; i < RoomList.Count; i++)
            {
                for(int j = 0; j < pdfInfoList.Count; j++)
                {
                    if (pdfInfoList[j].name == RoomList[i].TriggerPlane.name)
                    {
                        RoomList[i].pdfMes = pdfInfoList[j];
                    }
                    if(videoInfoList[j].name == RoomList[i].TriggerPlane.name)
                    {
                        RoomList[i].videoMes = videoInfoList[j];
                    }
                }
                
            }
        }
        IEnumerator LoadConfigFile(string mPath, float delayTime = 0)
        {
            yield return new WaitForSeconds(delayTime);
            if (!mPath.StartsWith("http"))
            {
                yield break;
            }
            var uwr = UnityWebRequest.Get(mPath);
            count++;
            if (count > 60) yield break;
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error))
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(LoadConfigFile(jsonPath, 3));
            }
            else
            {
                Debug.Log(uwr.downloadHandler.text);
                JsonCallback(JsonMapper.ToObject(uwr.downloadHandler.text));
                uwr.Dispose();
            }
        }
    }
}
