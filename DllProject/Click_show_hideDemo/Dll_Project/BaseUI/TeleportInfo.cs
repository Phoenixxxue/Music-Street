using com.ootii.Messages;
using Dll_Project.BaseUI;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dll_Project.BaseUI
{
    public class TeleportInfo
    {
        public string ID;
        public string MeshName;
        public string RootRoomID;
        public string RootVoiceID;
        public string ImgUrl;
        public string Name;
        public string Password;
        public string HaveTeleport;
    }
    public class TeleportCtrl : DllGenerateBase
    {
        private AudioSource clickAudioSource;//BaseMono
        private AudioClip clickAudioClip;

        private Transform FloorTeleportPanel;
        private Transform contentParent;
        private GameObject objPrefab;
        private bool isFloorTeleport = false;

        private Transform floorNumber;
        private GameObject TeleportModel;

        public override void Init()
        {
            clickAudioSource = BaseMono.ExtralDatas[0].Target.Find("Click").GetComponent<AudioSource>();
            clickAudioClip = clickAudioSource.clip;

            FloorTeleportPanel = BaseMono.ExtralDatas[1].Target;
            contentParent = BaseMono.ExtralDatas[1].Info[0].Target;
            objPrefab = BaseMono.ExtralDatas[1].Info[1].Target.gameObject;

            floorNumber = BaseMono.ExtralDatas[2].Target;
            TeleportModel = BaseMono.ExtralDatas[3].Target.gameObject;
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);

        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);

            BaseMono.StopAllCoroutines();
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        public override void Start()
        {
            BaseMono.StartCoroutine(ShowFloorNumber(1.6f));

            FloorTeleportPanel.Find("CancelButton").GetComponent<Button>().onClick.AddListener(FloorCancelClick);
            FloorTeleportPanel.Find("SureButton").GetComponent<Button>().onClick.AddListener(FloorSureClick);

            IsVR();
        }

        private void IsVR()
        {
            if (mStaticThings.I == null)
                return;
            if (mStaticThings.I.isVRApp)
            {
                FloorTeleportPanel.transform.parent.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                FloorTeleportPanel.transform.parent.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
            }
        }
        public override void Update()
        {
            if (mStaticThings.I != null)
            {
                if (mStaticThings.I.isVRApp)
                {
                    FollowCamera(-15, 0.65f);
                }
            }
        }

        /// <summary>
        /// VR传送界面跟随头显移动旋转
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="distocamera"></param>
        public void FollowCamera(float angle, float distocamera)
        {
            if (mStaticThings.I == null) return;
            if (mStaticThings.I.Maincamera == null) return;
            if (FloorTeleportPanel != null)
            {
                FloorTeleportPanel.transform.forward = mStaticThings.I.Maincamera.forward;
                Quaternion rotate = Quaternion.AngleAxis(angle, mStaticThings.I.Maincamera.right);

                FloorTeleportPanel.transform.position = mStaticThings.I.Maincamera.position + rotate * mStaticThings.I.Maincamera.forward.normalized * distocamera;

                FloorTeleportPanel.transform.position = new Vector3(FloorTeleportPanel.transform.position.x, FloorTeleportPanel.transform.position.y - 0.2f, FloorTeleportPanel.transform.position.z);
            }
        }

        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("teleport_5"))
            {
                FloorTeleportPanel.gameObject.SetActive(true);
                isFloorTeleport = true;
            }
            else
            {
                FloorTeleportPanel.gameObject.SetActive(false);
                isFloorTeleport = false;
            }
        }

        #region 楼层跳转
        private void FloorCancelClick()
        {
            FloorTeleportPanel.gameObject.SetActive(false);
        }

        private void FloorSureClick()
        {
            if (mStaticThings.I != null)
            {
                Transform temp = null;
                for (int i = 0; i < contentParent.childCount; i++)
                {
                    if (contentParent.GetChild(i).GetComponent<Toggle>().isOn == true)
                    {
                        temp = contentParent.GetChild(i);
                    }
                }
                if (mStaticThings.I.isAdmin)
                {
                    WsCChangeInfo wsinfo = new WsCChangeInfo()
                    {
                        a = "FloorTeleport",
                        b = temp.name
                    };
                    MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
                }
                else
                {
                    clickAudioSource.PlayOneShot(clickAudioClip);
                    ChangeRoomByFloorName(temp.name);
                    Resources.UnloadUnusedAssets();
                }

            }
        }
        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (isFloorTeleport == true)
            {
                if (info.a.Equals("FloorTeleport"))
                {
                    FloorTeleportPanel.gameObject.SetActive(false);
                    clickAudioSource.PlayOneShot(clickAudioClip);
                    ChangeRoomByFloorName(info.b);
                    Resources.UnloadUnusedAssets();
                }
            }
        }
        private IEnumerator ChangeRoom(string RootRoomID, string RootVoiceID)
        {
            yield return new WaitForSeconds(0.3f);
            //MDebug(RootRoomID);
            if (string.IsNullOrEmpty(RootRoomID) || string.IsNullOrEmpty(RootVoiceID))
                yield break;
            VRRootChanelRoom ch = new VRRootChanelRoom
            {
                roomid = RootRoomID,
                voiceid = RootVoiceID,
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.ConnectToNewChanel.ToString(), ch, 0);
        }
        private void ChangeRoomByFloorName(string meshName)
        {
            if (string.IsNullOrEmpty(meshName))
                return;
            for (int i = 0; i < mStaticData.CompanyAsset.floorTeleport.floorTeleports.Count; i++)
            {
                if (mStaticData.CompanyAsset.floorTeleport.floorTeleports[i].ID == meshName)
                {
                    BaseMono.StartCoroutine(ChangeRoom(mStaticData.CompanyAsset.floorTeleport.floorTeleports[i].RootRoomID, mStaticData.CompanyAsset.floorTeleport.floorTeleports[i].RootVoiceID));
                    return;
                }
            }
        }
        /// <summary>
        /// 楼层传送面板数据展示
        /// </summary>
        private void ShowFlooe()
        {
            count = 0;
            BaseMono.StartCoroutine(GetInfo(mStaticData.CompanyAsset.floorTeleport.floorTeleports[count], count, BackCall));
        }
        int count;
        private IEnumerator GetInfo(TeleportInfo teleportInfo, int index, Action action)
        {
            if (!teleportInfo.ImgUrl.StartsWith("http"))
            {
                yield break;
            }
            var uwr = UnityWebRequestTexture.GetTexture(teleportInfo.ImgUrl);
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error) || uwr.isNetworkError || uwr.isHttpError)
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(GetInfo(teleportInfo, index, action));
            }
            else
            {
                Texture2D mTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;

                var temp = GameObject.Instantiate(objPrefab, contentParent);
                temp.GetComponent<RectTransform>().localScale = Vector3.one;
                temp.name = teleportInfo.ID;
                temp.transform.Find("Background").GetComponent<Image>().sprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), new Vector2(0.5f, 0.5f));
                temp.transform.Find("Image/Label").GetComponent<Text>().text = teleportInfo.Name;
                temp.SetActive(true);
                if (count == int.Parse(mStaticData.CompanyAsset.floorTeleport.FloorNumber) - 1)
                {
                    temp.GetComponent<Toggle>().interactable = false;
                }

                count++;
                if (mStaticData.CompanyAsset.floorTeleport.floorTeleports.Count > count)
                {
                    action();
                }
            }
        }
        private void BackCall()
        {
            BaseMono.StartCoroutine(GetInfo(mStaticData.CompanyAsset.floorTeleport.floorTeleports[count], count, BackCall));
        }
        #endregion

        /// <summary>
        /// 展示楼层数
        /// </summary>
        public IEnumerator ShowFloorNumber(float time)
        {
            yield return new WaitForSeconds(time);
            if (string.IsNullOrEmpty(mStaticData.CompanyAsset.floorTeleport.FloorNumber))
            {
                BaseMono.StartCoroutine(ShowFloorNumber(1.6f));
            }
            else
            {
                for (int i = 0; i < floorNumber.childCount; i++)
                {
                    ShowFloorNumber(floorNumber.GetChild(i));
                }
                if (mStaticData.CompanyAsset.floorTeleport.HaveTeleport == "0")
                {
                    TeleportModel.SetActive(false);
                    BaseMono.ExtralDatas[3].Info[0].Target.gameObject.SetActive(false);
                    BaseMono.ExtralDatas[3].Info[1].Target.gameObject.SetActive(false);
                }
                else
                {
                    ShowFlooe();
                }
            }
        }
        private void ShowFloorNumber(Transform tf)
        {
            if (string.IsNullOrEmpty(mStaticData.CompanyAsset.floorTeleport.FloorNumber))
                return;
            for (int i = 0; i < tf.childCount; i++)
            {
                if (i == int.Parse(mStaticData.CompanyAsset.floorTeleport.FloorNumber) - 1)
                {
                    tf.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    tf.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        public void MDebug(string msg, int level = 0)
        {
            Debug.Log(msg);
            if (level == 0)
            {
                if (mStaticThings.I == null)
                    return;
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
    }
}
