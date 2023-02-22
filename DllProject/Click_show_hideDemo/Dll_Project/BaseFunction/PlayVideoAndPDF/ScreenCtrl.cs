using com.ootii.Messages;
using DG.Tweening;
using Dll_Project.BaseUI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dll_Project.PlayVideoAndPDF
{
    public class ScreenInfo
    {
        public GameObject ScreenGO;
        public Vector3 OldLocalPos;
        public Quaternion OldRotationPos;
        public Vector3 OldScale;
    }
    public class ScreenCtrl : DllGenerateBase
    {
        private string screenPrefix = "rawimage_";
        private ScreenInfo curscreen;
        private ScreenInfo[] screenInfos;   //BaseMono
        private Transform screenParent;     //BaseMono
        private AudioSource clickAudioSource;
        private AudioClip clickAudioClip;

        private Transform CameraTemp;
        private GameObject UICanvas;

        public override void Init()
        {
            CameraTemp = BaseMono.ExtralDatas[3].Target;
            screenParent = BaseMono.ExtralDatas[0].Target;
            clickAudioSource = BaseMono.ExtralDatas[1].Target.Find("Click").GetComponent<AudioSource>();
            clickAudioClip = clickAudioSource.clip;
            UICanvas = BaseMono.ExtralDatas[2].Target.gameObject;
            screenInfos = new ScreenInfo[screenParent.childCount];
            for (int i = 0; i < screenInfos.Length; i++)
            {
                ScreenInfo screenInfo = new ScreenInfo();
                screenInfo.ScreenGO   = screenParent.GetChild(i).gameObject;
                screenInfo.OldLocalPos = screenInfo.ScreenGO.transform.localPosition;
                screenInfo.OldRotationPos = screenInfo.ScreenGO.transform.rotation;
                screenInfo.OldScale = screenInfo.ScreenGO.transform.localScale;
                screenInfos[i] = screenInfo;
            }
        }
        public override void Start()
        {
            for (int i = 0; i < screenParent.childCount; i++)
            {
                screenParent.GetChild(i).Find("PDFGameObject/CloseButton").GetComponent<Button>().onClick.AddListener(ResetCurCard);
                screenParent.GetChild(i).Find("VideoGameObject/ClosePanelButton").GetComponent<Button>().onClick.AddListener(ResetCurCard);
            }
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);

        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
            if (curscreen != null)
            {
                curscreen.ScreenGO.transform.SetParent(screenParent);
            }
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        private void OnPointClickEvent(IMessage msg)
        {
            GameObject go = msg.Data as GameObject;
          //  Debug.LogError("go:" + go.name);
            if (go.name.Contains(screenPrefix) && mStaticData.IsOpenPointClick && mStaticData.IsPointAssetOrCard)
            {
                clickAudioSource.PlayOneShot(clickAudioClip);
                if (curscreen != null && go.name == curscreen.ScreenGO.name)
                {
                   // ResetCurCard();
                }
                else
                {
                    ResetCurCard();
                    SetCurCard(go);
                }
            }
            else if (!go.CompareTag("Player"))
            {
                ResetCurCard();
            }
        }

        private void TelePortToMesh(IMessage msg)
        {
            string name = msg.Data.ToString();
            if (name.Equals("Plane_All"))
            {
                ResetCurCard();
            }
        }
        private void ResetCurCard()
        {
            if (curscreen != null)
            {
                GameObject gameObject = curscreen.ScreenGO;
                Vector3 oldPos = curscreen.OldLocalPos;

                gameObject.transform.SetParent(screenParent);
                gameObject.transform.DOKill(false);
                Tweener tweener = gameObject.transform.DOLocalMove(oldPos, 0.5f).OnComplete(() => { mStaticData.IsPointAssetOrCard = true; });
                tweener.SetAutoKill(true);
                tweener.Play();

                Tweener twScale = gameObject.transform.DOScale(curscreen.OldScale, 0.5f);
                twScale.SetAutoKill(true);
                twScale.Play();

                Tweener twRotation = gameObject.transform.DOLocalRotateQuaternion(curscreen.OldRotationPos, 0.5f);
                twRotation.SetAutoKill(true);
                twRotation.Play();

                gameObject.transform.Find("VideoGameObject").gameObject.SetActive(false);
                gameObject.transform.Find("PDFGameObject").gameObject.SetActive(false);
                gameObject.transform.Find("AssetPanel").GetComponent<Image>().enabled = false;
                gameObject.transform.Find("AssetPanel/MaskImage").GetComponent<Image>().enabled = false;

                curscreen = null;
                gameObject.GetComponent<Canvas>().worldCamera = null;
                ShowOrHideUI(true);
            }
        }
        private void SetCurCard(GameObject go)
        {
            for (int i = 0; i < screenInfos.Length; i++)
            {
                if (screenInfos[i].ScreenGO.name == go.name)
                {
                    curscreen = screenInfos[i];
                    
                    break;
                }
            }
            if (curscreen == null)
                return;

            go.transform.SetParent(CameraTemp);

            go.transform.DOKill(false);
            float dis = 0.35f;
            if (mStaticThings.I.isVRApp)
                dis = 0.7f;
            Tweener tweener = go.transform.DOLocalMove(new Vector3(0, 0, dis), 1).OnComplete(() =>
            {
                mStaticData.IsPointAssetOrCard = false;
                SaveInfo.instance.SaveActionData(go.name, 13);
            });
            tweener.SetAutoKill(true);
            tweener.Play();

            Tweener twScale = go.transform.DOScale(new Vector3(0.0003f, 0.0003f, 0.0003f), 1);//curscreen.OldScale*0.10f
            twScale.SetAutoKill(true);
            twScale.Play();

            Tweener twRotation = go.transform.DOLocalRotate(new Vector3(0, 0, 0), 1);
            twRotation.SetAutoKill(true);
            twRotation.Play();
            go.transform.Find("VideoGameObject").gameObject.SetActive(true);
            go.transform.Find("PDFGameObject").gameObject.SetActive(true);
            go.transform.Find("AssetPanel").GetComponent<Image>().enabled = true;
            go.transform.Find("AssetPanel/MaskImage").GetComponent<Image>().enabled = true;
            go.GetComponent<Canvas>().worldCamera = mStaticThings.I.Maincamera.GetComponent<Camera>();
            ShowOrHideUI(false);

        }

        //控制屏幕UI的显隐
        private void ShowOrHideUI(bool isBool)
        {
            if (!isBool)
            {
                MessageDispatcher.SendMessageData("SwitcPersionViewModeReq", 0);
            }
            var temp = GameObject.Find("LoginUICanvas");
            if (temp != null)
            {
                if (!mStaticThings.I.isVRApp)
                {
                    temp.transform.Find("RoomInPanel").gameObject.SetActive(isBool);
                }
                UICanvas.GetComponent<Canvas>().enabled = isBool;
            }

        }
    }
}
