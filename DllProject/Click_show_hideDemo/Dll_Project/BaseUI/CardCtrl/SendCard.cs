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

namespace Dll_Project.BaseUI.CardCtrl
{
    public class SendCard : DllGenerateBase
    {
        //public string Checkurl = "https://s.vscloud.vip/expo/api/check_user_detail_info";
        public string Checkurl = "http://121.37.129.57/middle/api/get/usercard";

        private Transform sendCardPanel;
        private Text nameText;
        private Text identityText;
        private Text companyText;
        private Button sendButton;
        private Button cancelButton;

        private string toMavtorId;
        public override void Init()
        {
            sendCardPanel = BaseMono.ExtralDatas[0].Target;
            nameText = BaseMono.ExtralDatas[1].Target.GetComponent<Text>();
            identityText = BaseMono.ExtralDatas[2].Target.GetComponent<Text>();
            companyText = BaseMono.ExtralDatas[3].Target.GetComponent<Text>();
            sendButton = BaseMono.ExtralDatas[4].Target.GetComponent<Button>();
            cancelButton = BaseMono.ExtralDatas[5].Target.GetComponent<Button>();
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            sendButton.onClick.AddListener(SendCardClick);
            cancelButton.onClick.AddListener(CancelCardClick);


        }

        void Shottap(Vector2 tappos)
        {

        }
        public override void OnEnable()
        {
            //IT_Gesture.onShortTapE += Shottap;
            //MessageDispatcher.AddListener(VrDispMessageType.SelectAvatarWsid.ToString(), SelectAvatarWsid, true);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
        }

        public override void OnDisable()
        {
            //IT_Gesture.onShortTapE -= Shottap;
            //MessageDispatcher.RemoveListener(VrDispMessageType.SelectAvatarWsid.ToString(), SelectAvatarWsid, true);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
        }

        public override void Update()
        {
            if (mStaticThings.I != null)
            {
                if (mStaticThings.I.ismobile)
                {
                    if (Input.GetMouseButtonDown(0) && sendCardPanel.gameObject.activeSelf == false && mStaticData.IsOpenPointClick == true && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        Ray ray = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out RaycastHit hitInfo, 3))
                        {
                            GameObject go = hitInfo.transform.gameObject;
                            if (go.GetComponentInParent<LookAtNearController>())
                            {
                                if (go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name != mStaticThings.I.mAvatarID)
                                {
                                    if (mStaticThings.AllStaticAvatarsDic.ContainsKey(go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name))
                                    {
                                        toMavtorId = go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name;
                                        BaseMono.StartCoroutine(GetPointCard(toMavtorId));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        void SelectAvatarWsid(IMessage msg)
        {
            //if (mStaticThings.I.ismobile)
            //    return;
            //string selid = (string)msg.Data;
            //if (mStaticThings.AllStaticAvatarsDic.ContainsKey(selid)) 
            //{
            //    toMavtorId = selid;
            //    //WsAvatarFrame wsAvatarFrame = mStaticThings.AllStaticAvatarsDic[selid];
            //    //nameText.text = wsAvatarFrame.name;
            //    //sendCardPanel.gameObject.SetActive(true);
            //    BaseMono.StartCoroutine(GetPointCard(selid));
            //}
        }

        private IEnumerator GetPointCard(string mAvatarID)
        {
            yield return new WaitForSeconds(0.1f);
            if (mStaticData.IsOpenPointClick == true)
            {
                WWWForm wwwForm = new WWWForm();
                wwwForm.AddField("vs_id", mAvatarID);
                wwwForm.AddField("room_id", mStaticThings.I.nowRoomID);
                UnityWebRequest uwr = UnityWebRequest.Post(Checkurl, wwwForm);
                uwr.SetRequestHeader("Authorization", mStaticThings.apitoken);
                yield return uwr.SendWebRequest();
                JsonData jd = JsonMapper.ToObject(uwr.downloadHandler.text);

                if (jd["code"].ToString() == "200")
                {
                    if (jd["data"].ToString() != null)
                    {
                        if (!string.IsNullOrEmpty(jd["data"]["company"].ToString()))
                            companyText.text = jd["data"]["company"].ToString();
                        if (!string.IsNullOrEmpty(jd["data"]["name"].ToString()))
                            nameText.text = jd["data"]["name"].ToString();
                    }
                    for (int i = 0; i < mStaticData.CompanyAsset.IdentityInfo.Count; i++)
                    {
                        if (mStaticData.CompanyAsset.IdentityInfo[i].mAvatorID == mAvatarID)
                        {
                            identityText.text = mStaticData.CompanyAsset.IdentityInfo[i].Sign;
                        }
                        else
                        {
                            identityText.text = null;
                        }
                    }
                    sendCardPanel.gameObject.SetActive(true);
                    sendCardPanel.transform.parent.GetComponent<Canvas>().sortingOrder = 11;
                }
                else
                {
                }
                uwr.Dispose();
            }
        }

        void OnPointClickEvent(IMessage msg)
        {
            GameObject go = (GameObject)msg.Data;
            if (go.GetComponentInParent<LookAtNearController>())
            {
                if (Vector3.Distance(go.transform.position, mStaticThings.I.Maincamera.position) < 3)
                {
                    if (mStaticThings.I.ismobile)
                    {
                        //if (sendCardPanel.gameObject.activeSelf == false && mStaticData.IsOpenPointClick == true /*&& !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)*/)
                        //{
                        //    if (mStaticThings.AllStaticAvatarsDic.ContainsKey(go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name))
                        //    {
                        //        toMavtorId = go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name;
                        //        if (toMavtorId != mStaticThings.I.mAvatarID)
                        //        {
                        //            BaseMono.StartCoroutine(GetPointCard(toMavtorId));
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        if (sendCardPanel.gameObject.activeSelf == false && mStaticData.IsOpenPointClick == true && !EventSystem.current.IsPointerOverGameObject())
                        {
                            if (mStaticThings.AllStaticAvatarsDic.ContainsKey(go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name))
                            {
                                toMavtorId = go.GetComponentInParent<LookAtNearController>().transform.parent.parent.name;
                                if (toMavtorId != mStaticThings.I.mAvatarID)
                                {
                                    BaseMono.StartCoroutine(GetPointCard(toMavtorId));
                                }
                            }
                        }
                    }
                }
            }
        }
        private void SendCardClick()
        {
            sendCardPanel.gameObject.SetActive(false);
            sendCardPanel.transform.parent.GetComponent<Canvas>().sortingOrder = 1;
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = mStaticThings.I.nowRoomStartChID + "GetCardInfo",
                b = toMavtorId,
                c = mStaticThings.I.mAvatarID,
                d = mStaticData.AvatorData.name,
                e = mStaticData.AvatorData.company_name
            };
            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
            //BaseMono.StartCoroutine(SendCardInfo(toMavtorId));
        }
        private void CancelCardClick()
        {
            sendCardPanel.gameObject.SetActive(false);
            sendCardPanel.transform.parent.GetComponent<Canvas>().sortingOrder = 1;
        }

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
    }
}

