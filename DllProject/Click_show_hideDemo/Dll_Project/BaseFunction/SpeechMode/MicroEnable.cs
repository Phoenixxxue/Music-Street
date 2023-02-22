using com.ootii.Messages;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Dll_Project.BaseFunction.SpeechMode
{

    class MicroEnable : DllGenerateBase
    {
        /// <summary>
        /// 演讲台移动区域mesh名字
        /// </summary>
        string speechregionname = "speechvrplace";

        /// <summary>
        /// 演讲台mesh名字
        /// </summary>
        string originSpeechregionname = "speechorigin";

        public override void OnEnable()
        {
            base.OnEnable();
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshHandler);
        }
        public override void OnDisable()
        {
            base.OnDisable();
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshHandler);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        /// <summary>
        /// 麦克风开启关闭
        /// </summary>
        /// <param name="rMessage"></param>
        private void TelePortToMeshHandler(IMessage rMessage)
        {
            if (rMessage == null) return;

            string name = rMessage.Data.ToString();

            if (name.Contains(originSpeechregionname) || name.Contains(speechregionname))
            {
                MessageDispatcher.SendMessage(this, VrDispMessageType.CommitOrder.ToString(), VROrderName.micenable.ToString(), 0);
                MessageDispatcher.SendMessage(this, "SetSelfVisibleAlltime", true, 0);

            }
            else
            {
                MessageDispatcher.SendMessage(this, "SetSelfVisibleAlltime", false, 0);
                if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin) { }
                else  MessageDispatcher.SendMessage(this, VrDispMessageType.CommitOrder.ToString(), VROrderName.micdisable.ToString(), 0);
            }
        }

        /// <summary>
        /// 人物放大、表情放大
        /// </summary>
        /// <param name="isEnlarge"></param>
        /// <returns></returns>
        IEnumerator DelaySetvaterScale(bool isEnlarge)
        {
            yield return new WaitForSeconds(2);
            if (isEnlarge)
            {
                mStaticThings.I.MainVRROOT.localScale = new Vector3(2, 2, 2);
                WsCChangeInfo wsinfo = new WsCChangeInfo()
                {
                    a = "SetEmoticonsPos",
                    b = "Up",
                    c = mStaticThings.I.mAvatarID,
                };
                MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
            }
            else
            {
                mStaticThings.I.MainVRROOT.localScale = new Vector3(1, 1, 1);
                WsCChangeInfo wsinfo = new WsCChangeInfo()
                {
                    a = "SetEmoticonsPos",
                    b = "Down",
                    c = mStaticThings.I.mAvatarID,
                };
                MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
            }
        }
    }
}
