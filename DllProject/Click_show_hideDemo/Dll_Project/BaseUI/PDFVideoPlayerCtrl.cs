using com.ootii.Messages;
using DG.Tweening;
using LitJson;
using Paroxe.PdfRenderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using UMP;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Dll_Project.BaseUI
{
    public class MediaFiles
    {
        public string MeshName;
        public int CurPlayIndex;//PDF-Video,从1开始
        public List<MediaFile> PDFVideoFiles;
        public float BGVolume;
    }
    public class MediaFile
    {
        public string FilePath;
        public string MD5;
        public int PDFPage;
        public MediaType MediaType;
        public float VideoVolume;
    }
    public class Volume
    {
        public float BGVolume;
        public float star上场音乐Volume;
        public float 领导讲话上场Volum;
        public float 退场音乐Volum;
        public float 抽奖环节Volum;
        public float 抽奖环节1Volum;
        public float 抽奖环节2Volum;
        public float 领导讲话完Volum;
        public float 领导讲话完1Volum;
        public float 领导讲话完2Volum;
        public float 备选Volum;
        public float 备选1Volum;
        public float 备选2Volum;
        public float 晚会_交响;
        public float 衍鹿Jump;
    }
    public enum MediaType
    {
        PDF,
        Video
    }
    public class PDFVideoPlayerCtrl : DllGenerateBase
    {
        //private List<MediaFiles> mediaFilesList;
        private GameObject videoPlayer;//BaseMono
        private GameObject[] rawImages;//BaseMono
        private Transform[] sceneVideoGOs;//BaseMono
        private GameObject[] boxBtns;//BaseMono
        private string groungPrefix = "ground_";
        private string boxBtnPrefix = "boxBtn_";
        private string rawimagePrefix = "rawimage_";
        private string curGround="";
        private RawImage curRawImage;
        private MediaFile curMediaFile;
        private RenderTexture pdfVideoRender;
        private AudioSource clickAudioSource;//BaseMono
        private AudioClip clickAudioClip;
        private AudioSource popupAudioSource;//BaseMono
        private AudioClip popupAudioClip;
        private GameObject curDefaultScreen;

        private GameObject[] vftArray;
        public override void Init()
        {
            videoPlayer = BaseMono.ExtralDatas[0].Target.gameObject;
            pdfVideoRender = videoPlayer.GetComponent<VideoPlayer>().targetTexture;
            rawImages = new GameObject[videoPlayer.transform.childCount];
            for (int i = 0; i < videoPlayer.transform.childCount; i++)
            {
                rawImages[i] = videoPlayer.transform.GetChild(i).gameObject;
            }
            Transform boxparent = BaseMono.ExtralDatas[1].Target;
            boxBtns = new GameObject[boxparent.childCount];
            for (int i = 0; i < boxparent.childCount; i++)
            {
                boxBtns[i] = boxparent.GetChild(i).gameObject;
            }
            clickAudioSource = BaseMono.ExtralDatas[3].Target.Find("Click").GetComponent<AudioSource>();
            clickAudioClip = clickAudioSource.clip;
            popupAudioSource = BaseMono.ExtralDatas[3].Target.Find("popup").GetComponent<AudioSource>();
            popupAudioClip = clickAudioSource.clip;

            ExtralData[] extralDatas = BaseMono.ExtralDatas[4].Info;
            sceneVideoGOs = new Transform[BaseMono.ExtralDatas[4].Info.Length];
            for (int i = 0; i < extralDatas.Length; i++)
            {
                sceneVideoGOs[i] = extralDatas[i].Target;
            }

            vftArray = new GameObject[BaseMono.ExtralDatas[2].Target.childCount];
            for (int i = 0; i < BaseMono.ExtralDatas[2].Target.childCount; i++)
            {
                vftArray[i] = BaseMono.ExtralDatas[2].Target.GetChild(i).gameObject;
                vftArray[i].SetActive(false);
            }
        }
        public override void Start()
        {
            for (int i = 0; i < rawImages.Length; i++)
            {
                rawImages[i].transform.Find("PDFGameObject/UpButton").GetComponent<Button>().onClick.AddListener(UpPDFClick);
                rawImages[i].transform.Find("PDFGameObject/NextButton").GetComponent<Button>().onClick.AddListener(NextPDFClick);
            }

            BaseMono.StartCoroutine(LoadPDFAndVideoFile(3));
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);

            MessageDispatcher.AddListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);

        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);

            MessageDispatcher.RemoveListener(VrDispMessageType.GetLocalCacheFile.ToString(), GetCacheFile);

            BaseMono.StopAllCoroutines();
            BaseMono.gameObject.SetActive(false);

            DisposePdfAndGC();
            System.GC.Collect();
        }
        public override void OnDestroy()
        {
            OnDisable();
        }

        private bool isOpen = false;
        float time;
        public override void Update()
        {
            if (isOpen) 
            {
                time += Time.deltaTime;
                if (time>=2) 
                {
                    time = 0;
                    for (int i = 0; i < vftArray.Length; i++)
                    {
                        vftArray[i].SetActive(false);
                    }
                    isOpen = false;
                }
            }
        }
        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();

            if (name.Contains(groungPrefix) && !name.Equals("ground_jiaban") && !name.Equals("ground_boli"))
            {
                if (curGround == name)
                    return;

                if (name.Contains("ground_pos"))
                {
                    vftArray[int.Parse(name.Replace("ground_pos", ""))-1].SetActive(true);
                    popupAudioSource.PlayOneShot(popupAudioClip);
                    isOpen = true;
                }
                
                SaveInfo.instance.SaveActionData(name, 11);
                
                curGround = name;
                StopPlayeMedia();//关闭播放器
                SetRIHide();
                //默认播放PDF
                curMediaFile = GetMediaByGround(name);
                if (curMediaFile == null)
                    return;
                curRawImage = SetRIVisibleByGround(name);
                PlayMedia(curMediaFile);
                //判断打开视频还是PDF控制器
                if (curMediaFile.MediaType == MediaType.PDF)
                {
                    curRawImage.transform.parent.parent.parent.Find("VideoGameObject").localScale = Vector3.zero;
                    curRawImage.transform.parent.parent.parent.Find("PDFGameObject").localScale = Vector3.one;
                }
                else if (curMediaFile.MediaType == MediaType.Video)
                {
                    curRawImage.transform.parent.parent.parent.Find("VideoGameObject").localScale = Vector3.one;
                    curRawImage.transform.parent.parent.parent.Find("PDFGameObject").localScale = Vector3.zero;
                }
            }
            else if (name.Equals("ground_jiaban") || name.Equals("ground_boli") || name.Equals("area_platform") || name.Equals("teleport_5"))
            {
                if (curGround != "")
                {
                    SaveInfo.instance.SaveActionData(curGround, 12);
                }
                curGround = "";
                StopPlayeMedia();//关闭播放器
                SetRIHide();
                DisposePdfWithOutGC();
            }
        }
        GameObject pointGo;
        private void OnPointClickEvent(IMessage msg)
        {
            pointGo = msg.Data as GameObject;
            if (pointGo.name.Contains(boxBtnPrefix))
            {
                if (!curGround.Contains(groungPrefix))
                    return;
                if (curGround.Replace(groungPrefix, "") != pointGo.name.Replace(boxBtnPrefix, ""))
                    return;
                clickAudioSource.PlayOneShot(clickAudioClip);
                //切换播放文件
                curMediaFile = GetMediaByBoxName(pointGo.name);
                if (curMediaFile == null)
                    return;
                AddAnimClick(pointGo);//添加缩放动画
                StopPlayeMedia();
                PlayMedia(curMediaFile);

                //判断打开视频还是PDF控制器
                if (curMediaFile.MediaType == MediaType.PDF)
                {
                    curRawImage.transform.parent.parent.parent.Find("VideoGameObject").localScale = Vector3.zero;
                    curRawImage.transform.parent.parent.parent.Find("PDFGameObject").localScale = Vector3.one;
                }
                else if (curMediaFile.MediaType == MediaType.Video)
                {
                    curRawImage.transform.parent.parent.parent.Find("VideoGameObject").localScale = Vector3.one;
                    curRawImage.transform.parent.parent.parent.Find("PDFGameObject").localScale = Vector3.zero;
                }

                Caching.ClearCache();
                Resources.UnloadUnusedAssets();
            }
        }

        private void AddAnimClick(GameObject go)
        {
            go.GetComponentInChildren<BoxCollider>().enabled = false;
            Tweener tweener = go.transform.parent.DOScale(Vector3.zero, 1).OnComplete(()=> 
            {
                go.transform.parent.DOScale(Vector3.one, 1).OnComplete(() => 
                {
                    go.GetComponentInChildren<BoxCollider>().enabled = true;
                });
            });
            tweener.SetAutoKill(true);
            tweener.Play();
        }
        private void ScreenShowPDF(Texture2D texture)
        {
            if (curRawImage == null)
                return;
            if (texture.width >= 2048 || texture.height >= 2048)
                return;

            RectTransform recttrans = curRawImage.GetComponent<RectTransform>();

            //3DUI PDF rawimage 大小
            float maxWidth = 1920;
            float maxHeight = 1080;
            float scalex = texture.width * 1.0f / maxWidth;
            float scaley = texture.height * 1.0f / maxHeight;
            if (scalex > scaley)
            {
                float d = 1.0f / scalex;
                scaley = scaley * d;
                scalex = 1.0f;
            }
            else
            {
                float d = 1.0f / scaley;
                scalex = scalex * d;
                scaley = 1.0f;
            }
            recttrans.sizeDelta = new Vector2(maxWidth * scalex, maxHeight * scaley);

            if (curRawImage.texture!= null) 
            {
                GameObject.Destroy(curRawImage.texture);
            }
            curRawImage.texture = texture;
            curRawImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            if(curDefaultScreen != null)
            {
                curDefaultScreen.gameObject.SetActive(false);
            }
            Resources.UnloadUnusedAssets();
        }

        public IEnumerator LoadPDFAndVideoFile(float delayTime = 0)
        {
            yield return new WaitForSeconds(delayTime);
            if (mStaticData.BoothAsset.mediaFilesList == null || mStaticData.BoothAsset.mediaFilesList.Count <= 0)
            {
                BaseMono.StartCoroutine(LoadPDFAndVideoFile(3));
            }
            else 
            {
                SetBoxBtnVisible();

                PlayMediaOnStart(curGround);
            }
        }
        private void PlayMediaOnStart(string ground)
        {
            if (string.IsNullOrEmpty(ground))
                return;
            if (ground.Contains(groungPrefix))
            {
                StopPlayeMedia();//关闭播放器
                SetRIHide();

                //默认播放PDF
                curMediaFile = GetMediaByGround(ground);
                if (curMediaFile == null)
                    return;
                curRawImage = SetRIVisibleByGround(ground);
                PlayMedia(curMediaFile);
            }
        }
        private void SetBoxBtnVisible()
        {
            if (mStaticData.BoothAsset.mediaFilesList == null || mStaticData.BoothAsset.mediaFilesList.Count <= 0)
            {
                return;
            }
            if (boxBtns == null || boxBtns.Length <= 0)
            {
                return;
            }
            for (int i = 0; i < mStaticData.BoothAsset.mediaFilesList.Count; i++)
            {
                if (mStaticData.BoothAsset.mediaFilesList[i].PDFVideoFiles != null && mStaticData.BoothAsset.mediaFilesList[i].PDFVideoFiles.Count >= 2)
                {
                    for (int j = 0; j < boxBtns.Length; j++)
                    {
                        if (boxBtns[j].name.Replace(boxBtnPrefix, "") == mStaticData.BoothAsset.mediaFilesList[i].MeshName)
                        {
                            boxBtns[j].SetActive(true);
                            break;
                        }
                    }
                }
            }
        }
        private RawImage SetRIVisibleByGround(string groundName)
        {
            if (string.IsNullOrEmpty(groundName))
            {
                return null;
            }
            string meshName = groundName.Replace(groungPrefix, "");
            RawImage rawImage = null;
            for (int i = 0; i < rawImages.Length; i++)
            {
                if(rawImages[i].gameObject.name.Replace(rawimagePrefix,"") == meshName)
                {
                    rawImages[i].gameObject.SetActive(true);
                    curDefaultScreen = sceneVideoGOs[i].gameObject;
                    rawImage = rawImages[i].GetComponentInChildren<RawImage>();
                }
                else
                {
                    rawImages[i].gameObject.SetActive(false);
                    sceneVideoGOs[i].gameObject.SetActive(true);
                }
            }
            return rawImage;
        }
        private void SetRIHide()
        {
            curRawImage = null;
            curDefaultScreen = null;
            for (int i = 0; i < rawImages.Length; i++)
            {
                rawImages[i].gameObject.SetActive(false);
                sceneVideoGOs[i].gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 默认踩在地面上就播放当前展馆的第一个文件
        /// 这个方法不考虑之前是否播放过
        /// </summary>
        private MediaFile GetMediaByGround(string groundName)
        {
            string meshName = groundName.Replace(groungPrefix, "");
            MediaFiles mediaFiles = null;
            List<MediaFiles> mList = mStaticData.BoothAsset.mediaFilesList;
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].MeshName == meshName)
                {
                    mediaFiles = mList[i];
                    break;
                }
            }
            if (mediaFiles == null)
                return null;
            MediaFile mediaFile = null;
            if (mediaFiles.PDFVideoFiles == null || mediaFiles.PDFVideoFiles.Count <= 0)
            {
                mediaFile = null;
            }
            else
            {
                mediaFile = mediaFiles.PDFVideoFiles[0];
                mediaFiles.CurPlayIndex = 1;
            }
            return mediaFile;
        }
        /// <summary>
        /// 按box切换PDF和视频
        /// </summary>
        private MediaFile GetMediaByBoxName(string boxbtnName)
        {
            string meshName = boxbtnName.Replace(boxBtnPrefix, "");
            MediaFiles mediaFiles = null;
            List<MediaFiles> mList = mStaticData.BoothAsset.mediaFilesList;
            for (int i = 0; i < mList.Count; i++)
            {
                if (mList[i].MeshName == meshName)
                {
                    mediaFiles = mList[i];
                    break;
                }
            }
            if (mediaFiles == null)
                return null;
            MediaFile mediaFile = null;
            if (mediaFiles.PDFVideoFiles == null || mediaFiles.PDFVideoFiles.Count <= 0)
            {
                mediaFile = null;
            }
            else
            {
                mediaFiles.CurPlayIndex++;
                if(mediaFiles.CurPlayIndex > mediaFiles.PDFVideoFiles.Count)
                {
                    mediaFile = mediaFiles.PDFVideoFiles[0];
                    mediaFiles.CurPlayIndex = 1;
                }
                else
                {
                    mediaFile = mediaFiles.PDFVideoFiles[mediaFiles.CurPlayIndex-1];
                }
            }
            return mediaFile;
        }

        Texture2D TempTex;
        private void PlayNewPDF(string url)
        {
            DisposePdfAndGC();
            pdfDocument = new PDFDocument(url);
            if (pdfDocument.IsValid)
            {
                pdfPage = 0;
                pageCount = pdfDocument.GetPageCount();
                renderer = new PDFRenderer();

                TempTex = renderer.RenderPageToTexture(pdfDocument.GetPage(pdfPage % pageCount));
                TempTex.filterMode = FilterMode.Bilinear;
                TempTex.anisoLevel = 8;

                ScreenShowPDF(TempTex);
            }
            else
            {
                if (File.Exists(url))
                {
                    File.Delete(url);
                }
            }
        }
        private void PLayPDF(int page)
        {
            if (curDefaultScreen != null)
            {
                curDefaultScreen.gameObject.SetActive(true);
            }
            if (pdfDocument == null)
                return;
            if (pdfDocument.IsValid)
            {
                pdfPage = page;
                TempTex = renderer.RenderPageToTexture(pdfDocument.GetPage(pdfPage % pageCount));

                TempTex.filterMode = FilterMode.Bilinear;
                TempTex.anisoLevel = 8;

                ScreenShowPDF(TempTex);

                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }
        private void PlayVideo(string path)
        {
            if (curRawImage == null)
                return;
            if (curDefaultScreen != null)
            {
                curDefaultScreen.gameObject.SetActive(false);
            }

            //CustomVideoPlayer cvp = new CustomVideoPlayer
            //{
            //    ContorlObj = videoPlayer,
            //    RenderObj = new GameObject[] { curRawImage.gameObject },
            //    url = path,
            //    vol = 80,
            //    isloop = true,
            //    autostart = true
            //};
            //MessageDispatcher.SendMessage(this, VrDispMessageType.InitVideoPlayer.ToString(), cvp, 1);
            curRawImage.texture = pdfVideoRender;
            VideoPlayer videoPlayer1 = videoPlayer.GetComponent<VideoPlayer>();
            videoPlayer1.url = path;
            videoPlayer1.isLooping = true;
            //videoPlayer.GetComponent<AudioSource>().volume = videoVolunm;
            videoPlayer1.Play();
        }
        //float videoVolunm;
        private void PlayMedia(MediaFile mediaFile)
        {

            if (mediaFile.MediaType == MediaType.PDF)
            {
                //PLayPDF(mediaFile.FilePath, mediaFile.MD5);
                fileMd5 = mediaFile.MD5;
                SendFile(mediaFile.FilePath, mediaFile.MD5);
            }
            else
            {
                curRawImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                curRawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
                //PlayVideo(mediaFile.FilePath);
                videoMd5 = mediaFile.MD5;
                //videoVolunm = mediaFile.VideoVolume;
                SendFile(mediaFile.FilePath, mediaFile.MD5);
            }
        }
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
        string fileMd5;
        string videoMd5;
        void GetCacheFile(IMessage msg)
        {
            LocalCacheFile sendfile = msg.Data as LocalCacheFile;
            if (fileMd5 == sendfile.sign)
            {
                if (curMediaFile.MediaType == MediaType.PDF)
                    PlayNewPDF(sendfile.path);
            }
            else if (videoMd5 == sendfile.sign) 
            {
                if (curMediaFile.MediaType == MediaType.Video)
                    PlayVideo(sendfile.path);
            }
        }
        private void StopPlayeMedia()
        {
            if (curMediaFile == null)
                return;

            if (pdfVideoRender!=null) 
            {
                pdfVideoRender.Release();
            }
            videoPlayer.GetComponent<VideoPlayer>().Stop();

            if (curRawImage != null)
            {
                curRawImage.color = new Color(1.0f, 1.0f, 1.0f, 0f);
                curRawImage.texture = null;
                //GameObject.Destroy(curRawImage.texture);
            }

            //videoPlayer.SendMessage("Stop", SendMessageOptions.DontRequireReceiver);

            Resources.UnloadUnusedAssets();
        }

        #region pdf上一页下一页

        private void UpPDFClick() 
        {
            if (pdfDocument == null)
                return;
            if (pdfPage > 0 && pdfPage <= pageCount)
            {
                pdfPage--;
            }

            PLayPDF(pdfPage);
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
        }
        private void NextPDFClick() 
        {
            if (pdfDocument == null)
                return;
            if (pdfPage >= 0 && pdfPage < pageCount - 1)
            {
                pdfPage++;
            }

            PLayPDF(pdfPage);
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
        }

        
        #endregion

        public void MDebug(string msg, int level = 0)
        {
            if (level == 0)
            {
                if (mStaticThings.I == null)
                {
                    Debug.Log(msg);
                    return;
                }
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = msg,
                    b = InfoColor.black.ToString(),
                    c = "3",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
            }
        }

        #region 播放PDF
        private int pdfPage = 0;
        private PDFDocument pdfDocument;
        PDFRenderer renderer;
        int pageCount;//ppt总页数

        private void DisposePdfAndGC()
        {
            if (pdfDocument != null)
            {
                pdfDocument.Dispose();
                pdfDocument = null;
            }
            if (renderer != null)
            {
                renderer.Dispose();
                renderer = null;
            }
            if (TempTex != null)
            {
                TempTex = null;
            }
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        private void DisposePdfWithOutGC()
        {
            if (pdfDocument != null)
            {
                pdfDocument.Dispose();
                pdfDocument = null;
            }
            if (renderer != null)
            {
                renderer.Dispose();
                renderer = null;
            }
            if (TempTex != null)
            {
                TempTex = null;
            }
        }



        #endregion
    }
}
