using com.ootii.Messages;
using DG.Tweening;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.BaseFunction.SpeechMode
{
    public class ScreenInfo
    {
        public GameObject ScreenGO;
        public Vector3 OldLocalPos;
        public Quaternion OldRotationPos;
        public Vector3 OldScale;
        public ScreenInfo(GameObject go) {
            ScreenGO = go;
            OldLocalPos = go.transform.localPosition;
            OldRotationPos = go.transform.localRotation;
            OldScale = go.transform.localScale;
        }
    }
    class MirrorScreen : DllGenerateBase
    {
        /// <summary>
        /// admin面板
        /// </summary>
        GameObject adminScreen=null;

        /// <summary>
        /// 非admin面板
        /// </summary>
        GameObject noAdminScreen = null;

        Button adminRawimageScreen;
        Button adminclose;

        Button noAdminRawimageScreen;
        Button noAdminclose;

        GameObject LoginUICanvas;
        private ScreenInfo curscreen;
        private ScreenInfo adminScreenInfo;
        private ScreenInfo noAdminScreenInfo;

        GameObject cameraPos;
        Transform screenroot;

        public override void Init()
        {
            base.Init();
            adminScreen = BaseMono.ExtralDatas[0].Target.gameObject;
            noAdminScreen = BaseMono.ExtralDatas[1].Target.gameObject;
            adminRawimageScreen = BaseMono.ExtralDatas[2].Target.GetComponent<Button>();
            adminclose = BaseMono.ExtralDatas[3].Target.GetComponent<Button>();
            noAdminRawimageScreen = BaseMono.ExtralDatas[4].Target.GetComponent<Button>();
            noAdminclose = BaseMono.ExtralDatas[5].Target.GetComponent<Button>();
            screenroot = BaseMono.ExtralDatas[6].Target;

            adminScreenInfo = new ScreenInfo(adminScreen);
            noAdminScreenInfo = new ScreenInfo(noAdminScreen);
            cameraPos= BaseMono.ExtralDatas[7].Target.gameObject;
        }

        public override void Start()
        {
            if (adminScreen != null) adminScreen.SetActive(false);
            if (noAdminScreen != null) noAdminScreen.SetActive(false);
            if (adminclose != null) adminclose.gameObject.SetActive(false);
            if (noAdminclose != null) noAdminclose.gameObject.SetActive(false);

            if (LoginUICanvas == null)
            {
                LoginUICanvas = GameObject.Find("LoginUICanvas");
            }
            adminScreen.GetComponent<Canvas>().worldCamera = mStaticThings.I.Maincamera.GetComponent<Camera>();
            noAdminScreen.GetComponent<Canvas>().worldCamera = mStaticThings.I.Maincamera.GetComponent<Camera>();

            adminRawimageScreen.onClick.AddListener(SetCurCard);
            noAdminRawimageScreen.onClick.AddListener(SetCurCard);
            adminclose.onClick.AddListener(ResetCurCard);
            noAdminclose.onClick.AddListener(ResetCurCard);

            SetScreen();
        }

        public override void LateUpdate()
        {
            cameraPos.transform.position = mStaticThings.I.Maincamera.position;
            cameraPos.transform.rotation = mStaticThings.I.Maincamera.rotation;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            MessageDispatcher.AddListener(VrDispMessageType.SetAdmin.ToString(), OnAdminChangedevent);
        }
        public override void OnDisable()
        {
            base.OnDisable();
            MessageDispatcher.RemoveListener(VrDispMessageType.SetAdmin.ToString(), OnAdminChangedevent);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }



        /// <summary>
        /// 主持人状态改变
        /// </summary>
        /// <param name="rMessage"></param>
        private void OnAdminChangedevent(IMessage rMessage)
        {
            if (rMessage == null) return;
            SetScreen();
        }
       
        /// <summary>
        /// 设置场景中的空气墙
        /// </summary>
        void SetScreen()
        {
            if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
            {
                if (adminScreen != null)
                    adminScreen.SetActive(true);
                if (noAdminScreen != null)
                    noAdminScreen.SetActive(false);
            }
            else
            {
                if (adminScreen != null)
                    adminScreen.SetActive(false);
                if (noAdminScreen != null)
                    noAdminScreen.SetActive(true);
            }
        }

        private void SetCurCard()
        {
            GameObject curObj = null ;
            if (adminScreen.activeInHierarchy)
            {
                curscreen = adminScreenInfo;
                curObj = adminScreen;
            }
            else if (noAdminScreen.activeInHierarchy)
            {
                curscreen = noAdminScreenInfo;
                curObj = noAdminScreen;
            }
            noAdminclose.gameObject.SetActive(true);
            adminclose.gameObject.SetActive(true);

            curObj.transform.SetParent(cameraPos.transform);
            curObj.transform.DOKill(false);
            float dis = 0.35f;
            if (mStaticThings.I.isVRApp)
                dis = 0.7f;
            curObj.transform.DOLocalMove(new Vector3(0, 0, dis), 1).SetAutoKill(true);
            curObj.transform.DOScale(new Vector3(0.0004756f, 0.0004756f, 0.0004756f), 1).SetAutoKill(true);
            curObj.transform.DOLocalRotate(new Vector3(0, 0, 0), 1).SetAutoKill(true);
            BaseMono.StartCoroutine(ShowOrHideUI(false, 0));

        }

        /// <summary>
        /// 恢复UI缩小到场景
        /// </summary>
        private void ResetCurCard()
        {
            if (curscreen != null)
            {
                noAdminclose.gameObject.SetActive(false);
                adminclose.gameObject.SetActive(false);
                GameObject screenObj = curscreen.ScreenGO;

                screenObj.transform.SetParent(screenroot);
                screenObj.transform.DOKill(false);
                screenObj.transform.DOLocalMove(curscreen.OldLocalPos, 0.5f).SetAutoKill(true);
                screenObj.transform.DOScale(curscreen.OldScale, 0.5f).SetAutoKill(true);
                screenObj.transform.DOLocalRotateQuaternion(curscreen.OldRotationPos, 0.5f).SetAutoKill(true);
                BaseMono.StartCoroutine(ShowOrHideUI(true, 0.5f));

                curscreen = null;
            }
        }

        /// <summary>
        /// 设置屏幕UI的显示隐藏
        /// </summary>
        /// <param name="isBool"></param>
        private IEnumerator ShowOrHideUI(bool isBool, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            if (LoginUICanvas == null)
            {
                LoginUICanvas = GameObject.Find("LoginUICanvas");
            }
            if (LoginUICanvas != null)
            {
                LoginUICanvas.GetComponent<Canvas>().enabled = isBool;
            }
        }
    }
}
