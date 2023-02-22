using com.ootii.Messages;
using Dll_Project.BaseUI.CardCtrl;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

namespace Dll_Project.BaseUI.MyInformation
{
    public class GetBoothInfo : DllGenerateBase
    {
        string urlpath;
        private AudioSource bgAudioSource;//BaseMono
        bool bgStarted;
        int count;

        private Transform MulticastParent;
        public override void Init()
        {
            if (!string.IsNullOrEmpty(mStaticThings.I.nowRoomActionAPI))
            {
                urlpath = mStaticThings.I.nowRoomActionAPI;
            }
            else
            {
                urlpath = BaseMono.ExtralDatas[0].OtherData;
            }

            bgAudioSource = BaseMono.ExtralDatas[1].Target.Find("BG").GetComponent<AudioSource>();

            MulticastParent = BaseMono.ExtralDatas[2].Target;
        }
        #region 初始
        public override void Awake()
        {
            BaseMono.StartCoroutine(LoadIniConfigFile(urlpath, 0));
        }

        public override void Start()
        {

        }
        public override void OnEnable()
        {

            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.AddListener(WsMessageType.RecieveGetData.ToString(), RecieveGetDataEvent);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveGetData.ToString(), RecieveGetDataEvent);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Update()
        {
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

        #region json
        IEnumerator LoadIniConfigFile(string mPath, float delayTime = 0)
        {
            //yield return new WaitForSeconds(delayTime);
            if (!mPath.StartsWith("http"))
            {
                yield break;
            }
            var uwr = UnityWebRequest.Get(mPath);
            count++;
            if (count > 60) yield break;
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || string.IsNullOrEmpty(uwr.downloadHandler.text) || uwr.isNetworkError || uwr.isHttpError)
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(LoadIniConfigFile(mPath, 2f));
            }
            else
            {
                mStaticData.CompanyAsset = ReadCompanyAssetJson(uwr.downloadHandler.text);
                mStaticData.BoothAsset = ReadBoothAssetJson(uwr.downloadHandler.text);
                DataCollectJson(uwr.downloadHandler.text);
                BaseMono.StartCoroutine(ShowBoothPicture.instance.LoadData(0.2f));//展位资源加载

                uwr.Dispose();
            }
        }
        private CompanyAsset ReadCompanyAssetJson(string str)
        {
            CompanyAsset companyAsset = new CompanyAsset();
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("CompanyAsset"))
                return null;
            JsonData jsonData = jd["CompanyAsset"];
            if (!string.IsNullOrEmpty(jsonData.ToJson()))
            {
                if (jsonData.ToJson().Contains("ScreenAsset"))
                {
                    JsonData jsonScreenList = jsonData["ScreenAsset"];
                    if (!jsonScreenList.ToJson().Equals("[]"))
                    {
                        companyAsset.companyFilesList = ReadCompanyFiles(jsonScreenList);
                    }
                }

                if (jsonData.ToJson().Contains("PosTeleport"))
                {
                    JsonData jsonPosTeleportList = jsonData["PosTeleport"];
                    if (!jsonPosTeleportList.ToJson().Equals("[]"))
                    {
                        companyAsset.posTeleports = ReadPosTeleport(jsonPosTeleportList);
                    }
                }

                if (jsonData.ToJson().Contains("FloorTeleport"))
                {
                    JsonData jsonFloorTeleport = jsonData["FloorTeleport"];
                    companyAsset.floorTeleport.HaveTeleport = jsonFloorTeleport["HaveTeleport"].ToString();
                    companyAsset.floorTeleport.FloorNumber = jsonFloorTeleport["FloorNumber"].ToString();
                    if (!jsonFloorTeleport["FloorAsset"].ToJson().Equals("[]"))
                    {
                        companyAsset.floorTeleport.floorTeleports = ReadFloorTeleport(jsonFloorTeleport["FloorAsset"]);
                    }
                }

                if (jsonData.ToJson().Contains("CardInfo"))
                {
                    JsonData jsonCardInfo = jsonData["CardInfo"];
                    if (!jsonCardInfo.ToJson().Equals("[]"))
                    {
                        companyAsset.CardInfo = ReadCardInfo(jsonCardInfo);
                    }
                }

                if (jsonData.ToJson().Contains("CompanyScreen"))
                {
                    JsonData jsonCompanyScreen = jsonData["CompanyScreen"];
                    if (!jsonCompanyScreen.ToJson().Equals("[]"))
                    {
                        companyAsset.companyscreens = ReadCompanyscreen(jsonCompanyScreen);
                    }
                }

                if (jsonData.ToJson().Contains("Identity"))
                {
                    JsonData jsonIdentityInfo = jsonData["Identity"];
                    if (!jsonIdentityInfo.ToJson().Equals("[]"))
                    {
                        companyAsset.IdentityInfo = ReadIdentityInfo(jsonIdentityInfo);
                    }
                }

                if (jsonData.ToJson().Contains("BigScreenLogo"))
                {
                    JsonData jsonBigScreen = jsonData["BigScreenLogo"];
                    if (!string.IsNullOrEmpty(jsonBigScreen.ToString()))
                    {
                        string mediaPath = jsonBigScreen["MediaPath"].ToString();
                        string mediaMD5 = jsonBigScreen["MediaMD5"].ToString();

                        companyAsset.BigScreenLoge.MediaPath = mediaPath;
                        companyAsset.BigScreenLoge.MediaMD5 = mediaMD5;

                        if (!string.IsNullOrEmpty(mediaPath) && !string.IsNullOrEmpty(mediaMD5))
                        {
                            ShowImage(companyAsset.BigScreenLoge.MediaPath, companyAsset.BigScreenLoge.MediaMD5);
                        }
                    }
                }

                if (jsonData.ToJson().Contains("BGVolume"))
                {
                    JsonData jsonBGVolume = jsonData["BGVolume"];
                    if (!string.IsNullOrEmpty(jsonBGVolume.ToString()))
                    {
                        string bgVolume = jsonBGVolume["BGVolume"].ToString();

                        companyAsset.volume.BGVolume = float.Parse(bgVolume);

                        if (!string.IsNullOrEmpty(bgVolume))
                        {
                            bgAudioSource.volume = float.Parse(bgVolume);
                        }
                    }
                }

                if (jsonData.ToJson().Contains("Multicast"))
                {
                    JsonData jsonMulticast = jsonData["Multicast"];
                    if (!jsonMulticast.ToJson().Equals("[]"))
                    {
                        companyAsset.MulticastInfo = ReadMulticastInfo(jsonMulticast);

                        for (int i = 0; i < MulticastParent.childCount; i++)
                        {
                            MulticastParent.GetChild(i).Find("Image/Text").GetComponent<Text>().text = companyAsset.MulticastInfo[i].Name;
                        }
                    }
                }
            }
            return companyAsset;
        }

        private BoothAsset ReadBoothAssetJson(string str)
        {
            BoothAsset boothAsset = new BoothAsset();
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("BoothAsset"))
                return null;
            JsonData jsonData = jd["BoothAsset"];
            if (!string.IsNullOrEmpty(jsonData.ToJson()))
            {
                if (jsonData.ToJson().Contains("MediaPlayer"))
                {
                    JsonData jsonMediaList = jsonData["MediaPlayer"];
                    if (!jsonMediaList.ToJson().Equals("[]"))
                    {
                        boothAsset.mediaFilesList = ReadMediaPlayer(jsonMediaList);
                    }
                }

                if (jsonData.ToJson().Contains("CardInfo"))
                {
                    JsonData jsonCarInfoList = jsonData["CardInfo"];
                    if (!jsonCarInfoList.ToJson().Equals("[]"))
                    {
                        boothAsset.cardInfo = ReadCardInfo(jsonCarInfoList);
                    }
                }

                if (jsonData.ToJson().Contains("PictureInfo"))
                {
                    JsonData jsonPictureInfoList = jsonData["PictureInfo"];
                    if (!jsonPictureInfoList.ToJson().Equals("[]"))
                    {
                        boothAsset.boothAssets = ReadBoothPicInfo(jsonPictureInfoList);
                    }
                }

                if (jsonData.ToJson().Contains("GuideToVisitors"))
                {
                    JsonData jsonGuideToVisitorsList = jsonData["GuideToVisitors"];
                    if (!jsonGuideToVisitorsList.ToJson().Equals("[]"))
                    {
                        boothAsset.guideToVisitors = ReadGuideToVisitors(jsonGuideToVisitorsList);
                    }
                }
            }
            return boothAsset;
        }
        private void DataCollectJson(string str)
        {
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("DataCollection"))
                return;
            JsonData jsonData = jd["DataCollection"];
            if (!string.IsNullOrEmpty(jsonData.ToJson()))
            {
                InfoCollectController.Instance.isOpen = bool.Parse(jsonData["DataCollection"].ToString());
                InfoCollectController.Instance.isSaveTimeZoom = bool.Parse(jsonData["SaveTimeZoom"].ToString());
                InfoCollectController.Instance.isSaveViewData = bool.Parse(jsonData["SaveViewData"].ToString());
            }
        }
        private List<CompanyFiles> ReadCompanyFiles(JsonData jd)
        {
            List<CompanyFiles> companyFilesList = new List<CompanyFiles>();
            for (int i = 0; i < jd.Count; i++)
            {
                string id = jd[i]["ID"].ToString();
                string name = jd[i]["Name"].ToString();
                string mediaPath = jd[i]["MediaPath"].ToString();
                string mediaMD5 = jd[i]["MediaMD5"].ToString();
                string mediaType = jd[i]["MediaType"].ToString();

                CompanyFiles companyFiles = new CompanyFiles();
                companyFiles.ID = id;
                companyFiles.Name = name;
                companyFiles.MediaPath = mediaPath;
                companyFiles.MediaMD5 = mediaMD5;
                companyFiles.MediaType = mediaType;

                companyFilesList.Add(companyFiles);
            }
            return companyFilesList;
        }
        private List<MediaFiles> ReadMediaPlayer(JsonData jd)
        {
            List<MediaFiles> mediaFilesList = new List<MediaFiles>();
            string lastMeshName = string.Empty;
            for (int i = 0; i < jd.Count; i++)
            {
                string meshName = jd[i]["MeshName"].ToString();
                string mediaPath = jd[i]["MediaPath"].ToString();
                string mediaMD5 = jd[i]["MediaMD5"].ToString();
                string mediaType = jd[i]["MediaType"].ToString();
                float videoVolum = float.Parse(jd[i]["VideoVolum"].ToString());
                int type = int.Parse(mediaType);
                string pdfPage = jd[i]["PDFPage"].ToString();
                int page = int.Parse(pdfPage);

                if (meshName != lastMeshName)
                {
                    lastMeshName = meshName;
                    MediaFiles mediaFiles = new MediaFiles();
                    mediaFiles.MeshName = meshName;
                    if (!string.IsNullOrEmpty(mediaPath))
                    {
                        mediaFiles.PDFVideoFiles = new List<MediaFile>();
                        MediaFile media = new MediaFile()
                        {
                            FilePath = mediaPath,
                            MD5 = mediaMD5,
                            PDFPage = page,
                            MediaType = type == 0 ? MediaType.PDF : MediaType.Video,
                            VideoVolume = videoVolum,
                        };
                        mediaFiles.PDFVideoFiles.Add(media);
                    }
                    mediaFilesList.Add(mediaFiles);
                }
                else
                {
                    if (!string.IsNullOrEmpty(mediaPath))
                    {
                        MediaFile media = new MediaFile()
                        {
                            FilePath = mediaPath,
                            MD5 = mediaMD5,
                            PDFPage = page,
                            MediaType = type == 0 ? MediaType.PDF : MediaType.Video,
                            VideoVolume = videoVolum,
                        };
                        mediaFilesList[mediaFilesList.Count - 1].PDFVideoFiles.Add(media);
                    }
                }
            }
            return mediaFilesList;
        }

        private List<TeleportInfo> ReadPosTeleport(JsonData jd)
        {
            List<TeleportInfo> posTeleports = new List<TeleportInfo>();
            for (int i = 0; i < jd.Count; i++)
            {
                string meshName = jd[i]["MeshName"].ToString();
                string rootRoomID = jd[i]["RootRoomID"].ToString();
                string rootVoiceID = jd[i]["RootVoiceID"].ToString();
                string ImgUrl = jd[i]["ImgUrl"].ToString();
                string Name = jd[i]["Name"].ToString();
                string Password = jd[i]["Password"].ToString();
                string HaveTeleport = jd[i]["HaveTeleport"].ToString();


                TeleportInfo teleport = new TeleportInfo();
                teleport.MeshName = meshName;
                teleport.RootRoomID = rootRoomID;
                teleport.RootVoiceID = rootVoiceID;
                teleport.ImgUrl = ImgUrl;
                teleport.Name = Name;
                teleport.Password = Password;
                teleport.HaveTeleport = HaveTeleport;
                posTeleports.Add(teleport);
            }
            return posTeleports;
        }
        private List<TeleportInfo> ReadFloorTeleport(JsonData jd)
        {
            List<TeleportInfo> floorTeleports = new List<TeleportInfo>();
            for (int i = 0; i < jd.Count; i++)
            {
                string id = jd[i]["ID"].ToString();
                string meshName = jd[i]["MeshName"].ToString();
                string rootRoomID = jd[i]["RootRoomID"].ToString();
                string rootVoiceID = jd[i]["RootVoiceID"].ToString();
                string imgUrl = jd[i]["ImgUrl"].ToString();
                string name = jd[i]["Name"].ToString();

                TeleportInfo floorTeleport = new TeleportInfo();
                floorTeleport.ID = id;
                floorTeleport.MeshName = meshName;
                floorTeleport.RootRoomID = rootRoomID;
                floorTeleport.RootVoiceID = rootVoiceID;
                floorTeleport.ImgUrl = imgUrl;
                floorTeleport.Name = name;
                floorTeleports.Add(floorTeleport);
            }
            return floorTeleports;
        }

        private string pdfOrder = "nowroomPDFshowing";
        private bool receiveMyMsg;
        private string Path, Md5;
        private void ShowImage(string path, string md5)
        {
            if (string.IsNullOrEmpty(path))
                return;
            Path = path; Md5 = md5;
            VRSaveRoomData changeInfo = new VRSaveRoomData
            {
                sall = false,
                key = pdfOrder
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendGetData.ToString(), changeInfo, 0);
            receiveMyMsg = true;
        }
        private void RecieveGetDataEvent(IMessage msg)
        {
            //Debug.LogError(msg.Data);
            if (msg == null || msg.Data == null)
                return;
            Dictionary<string, string> dic = msg.Data as Dictionary<string, string>;
            if (!receiveMyMsg)
                return;
            receiveMyMsg = false;
            if (dic.ContainsKey(pdfOrder) && dic[pdfOrder] != "")
            {
                //不播放默认图片
            }
            else
            {
                if (Path == null)
                    return;
                ShowPic(Path, Md5);
            }
        }
        private void ShowPic(string path, string md5)
        {
            WsMediaFile newfile = new WsMediaFile()
            {
                url = path,
                fileMd5 = md5,
                name = path.Substring(path.LastIndexOf('/') + 1)
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.KODGetOneImage.ToString(), newfile, 0.5f);
        }

        private List<Card_Info> ReadCardInfo(JsonData jd)
        {
            List<Card_Info> cardInfo = new List<Card_Info>();
            for (int i = 0; i < jd.Count; i++)
            {
                string exhibition_id = jd[i]["exhibition_id"].ToString();
                string company_name = jd[i]["company_name"].ToString();
                string name = jd[i]["name"].ToString();
                string position = jd[i]["position"].ToString();
                string to_info = jd[i]["to_info"].ToString();
                string weixin = jd[i]["weixin"].ToString();
                string email = jd[i]["email"].ToString();
                string namepinyin = jd[i]["namepinyin"].ToString();

                Card_Info card_Info = new Card_Info();
                card_Info.exhibition_id = exhibition_id;
                card_Info.company_name = company_name;
                card_Info.name = name;
                card_Info.position = position;
                card_Info.to_info = to_info;
                card_Info.weixin = weixin;
                card_Info.email = email;
                card_Info.namepinyin = namepinyin;
                cardInfo.Add(card_Info);
            }
            return cardInfo;
        }
        private List<Companyscreen> ReadCompanyscreen(JsonData jd)
        {
            List<Companyscreen> companyscreen = new List<Companyscreen>();
            for (int i = 0; i < jd.Count; i++)
            {
                string id = jd[i]["ID"].ToString();
                string screenImageUrl = jd[i]["screenImageUrl"].ToString();
                string screenMd5 = jd[i]["screenMd5"].ToString();
                string name = jd[i]["name"].ToString();

                Companyscreen cs = new Companyscreen();
                cs.ID = id;
                cs.screenImageUrl = screenImageUrl;
                cs.screenMd5 = screenMd5;
                cs.name = name;
                companyscreen.Add(cs);
            }
            return companyscreen;
        }
        private List<Identity> ReadIdentityInfo(JsonData jd)
        {
            List<Identity> identityInfo = new List<Identity>();
            for (int i = 0; i < jd.Count; i++)
            {
                string name = jd[i]["Name"].ToString();
                string mAvatorID = jd[i]["mAvatorID"].ToString();
                string sign = jd[i]["Sign"].ToString();

                Identity identity = new Identity();
                identity.Name = name;
                identity.mAvatorID = mAvatorID;
                identity.Sign = sign;
                identityInfo.Add(identity);
            }
            return identityInfo;
        }
        private List<Multicast> ReadMulticastInfo(JsonData jd)
        {
            List<Multicast> multicastInfo = new List<Multicast>();
            for (int i = 0; i < jd.Count; i++)
            {
                string name = jd[i]["Name"].ToString();
                string id = jd[i]["ID"].ToString();

                Multicast multicast = new Multicast();
                multicast.ID = id;
                multicast.Name = name;
                multicastInfo.Add(multicast);
            }
            return multicastInfo;
        }
        private List<BoothPicInfo> ReadBoothPicInfo(JsonData jd)
        {
            List<BoothPicInfo> boothPicInfo = new List<BoothPicInfo>();
            for (int i = 0; i < jd.Count; i++)
            {
                string exhibition_id = jd[i]["exhibition_id"].ToString();
                string logeUrl = jd[i]["LogeUrl"].ToString();
                string logeMD5 = jd[i]["LogeMD5"].ToString();
                string pictureUrl = jd[i]["PictureUrl"].ToString();
                string pictureMD5 = jd[i]["PictureMD5"].ToString();

                BoothPicInfo booth_PicInfo = new BoothPicInfo();
                booth_PicInfo.exhibition_id = exhibition_id;
                booth_PicInfo.LogeUrl = logeUrl;
                booth_PicInfo.LogeMD5 = logeMD5;
                booth_PicInfo.PictureUrl = pictureUrl;
                booth_PicInfo.PictureMD5 = pictureMD5;
                boothPicInfo.Add(booth_PicInfo);
            }
            return boothPicInfo;
        }
        private List<GuideToVisitors> ReadGuideToVisitors(JsonData jd)
        {
            List<GuideToVisitors> guideToVisitors = new List<GuideToVisitors>();
            for (int i = 0; i < jd.Count; i++)
            {
                string id = jd[i]["ID"].ToString();
                string guideUrl = jd[i]["GuideUrl"].ToString();
                string guideMD5 = jd[i]["GuideMD5"].ToString();

                GuideToVisitors guideToVisitor = new GuideToVisitors();
                guideToVisitor.ID = id;
                guideToVisitor.GuideUrl = guideUrl;
                guideToVisitor.GuideMD5 = guideMD5;
                guideToVisitors.Add(guideToVisitor);
            }
            return guideToVisitors;
        }
        #endregion

        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("ground_jiaban"))
            {
                if (bgStarted)
                {
                    bgAudioSource.UnPause();//开始播放背景音乐
                }
                else
                {
                    bgAudioSource.Play();
                    bgStarted = true;
                }
            }
            else
            {
                bgAudioSource.Pause();
            }
        }

    }
}
