using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dll_Project.BaseFunction.SpeechMode
{
    /// <summary>
    /// 讲台UI支持人交互按钮
    /// </summary>
    class Share : DllGenerateBase
    {
        /// <summary>
        /// tip
        /// </summary>
        GameObject tipObj=null;

        bool tipDisplay = false;


        /// <summary>
        /// 共享桌面
        /// </summary>
        Button btnGXZM;

        /// <summary>
        /// 共享摄像头
        /// </summary>
        Button btnGXSXT;   


        public override void Init()
        {
            btnGXZM = BaseMono.ExtralDatas[0].Target.GetComponent<Button>();
            btnGXSXT = BaseMono.ExtralDatas[1].Target.GetComponent<Button>();
            tipObj = BaseMono.ExtralDatas[2].Target.gameObject;
        }
        public override void Start()
        {
            btnGXZM.onClick.AddListener(() => { SharedBtnOnClick(btnGXZM.gameObject.name); });
            btnGXSXT.onClick.AddListener(() => { SharedBtnOnClick(btnGXSXT.gameObject.name); });
        }

        /// <summary>
        /// 共享按钮点击事件
        /// </summary>
        /// <param name="btnName"></param>
        void SharedBtnOnClick(string btnName)
        {

            if (btnName == btnGXZM.gameObject.name)
            {
                if (mStaticThings.I.ismobile || mStaticThings.I.isVRApp)
                {
                    BaseMono.StartCoroutine(ShowDontshareScreenTip());
                    return;
                }
                //共享桌面
                string url = "https://s.vscloud.vip/#/screen/share?rid=" + mStaticThings.I.nowRoomID;
                Application.OpenURL(url);
            }
            else if (btnName == btnGXSXT.gameObject.name)
            {
                if (mStaticThings.I.isVRApp)
                {
                    BaseMono.StartCoroutine(ShowDontshareScreenTip());
                    return;
                }
                //共享摄像头
                string url = "https://s.vscloud.vip/#/screen/share?rid=" + mStaticThings.I.nowRoomID;
                Application.OpenURL(url);
            }
        }
        /// <summary>
        /// tip提示
        /// </summary>
        /// <returns></returns>
        IEnumerator ShowDontshareScreenTip()
        {
            if (!tipDisplay)
            {
                tipObj.SetActive(true);
                tipDisplay = true;
                yield return new WaitForSeconds(3);
                tipObj.SetActive(false);
                tipDisplay = false;
            }
            yield return null;
        }
    }
}