using com.ootii.Messages;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Dll_Project.BaseFunction.SpeechMode
{

    class PodeiumRegion : DllGenerateBase
    {
        /// <summary>
        /// 演讲台区域空气墙
        /// </summary>
        GameObject podiumAirWall = null;

        /// <summary>
        /// 演讲台移动区域
        /// </summary>
        GameObject speechVrPlace = null;

        /// <summary>
        /// 演讲台移动区域mesh名字
        /// </summary>
        string speechregionname = "speechvrplace";

        /// <summary>
        /// 演讲台mesh名字
        /// </summary>
        string originSpeechregionname = "speechorigin";

        /// <summary>
        /// 讲台上的镜像屏
        /// </summary>
        GameObject mirrorScreen = null;
        Transform camerapos;


        public override void Init()
        {
            base.Init();
            podiumAirWall = BaseMono.ExtralDatas[0].Target.gameObject;
            speechVrPlace = BaseMono.ExtralDatas[1].Target.gameObject;
            mirrorScreen = BaseMono.ExtralDatas[2].Target.gameObject;
            camerapos = BaseMono.ExtralDatas[3].Target;

        }

        public override void Start()
        {
            if (mirrorScreen!=null) mirrorScreen.SetActive(false);
            SetSceneAirWall();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            MessageDispatcher.AddListener(VrDispMessageType.SetAdmin.ToString(), OnAdminChangedevent);
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshHandler);

        }
        public override void OnDisable()
        {
            base.OnDisable();
            MessageDispatcher.RemoveListener(VrDispMessageType.SetAdmin.ToString(), OnAdminChangedevent);
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshHandler);

        }
        public override void OnDestroy()
        {
            OnDisable();
        }

        /// <summary>
        /// 传送到讲台、显示镜像屏
        /// </summary>
        /// <param name="rMessage"></param>
        private void TelePortToMeshHandler(IMessage rMessage)
        {
            if (rMessage == null) return;

            string name = rMessage.Data.ToString();

            if (mirrorScreen == null) return;

            if (name.Contains(speechregionname))
            {
                mirrorScreen.SetActive(true);
                camerapos.gameObject.SetActive(true);
            }
            else
            {
                mirrorScreen.SetActive(false);
                camerapos.gameObject.SetActive(false);
            }
            //被admin拉到讲台区域
            if (name.Contains(originSpeechregionname))
            {
                if (!speechVrPlace.activeInHierarchy)
                {
                    podiumAirWall.SetActive(false);
                    speechVrPlace.SetActive(true);
                }
            }
            else if (!name.Contains(speechregionname))
            {
                if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                {
                }
                else
                {
                    podiumAirWall.SetActive(true);
                    speechVrPlace.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 主持人状态改变
        /// </summary>
        /// <param name="rMessage"></param>
        private void OnAdminChangedevent(IMessage rMessage)
        {
            if (rMessage == null) return;
            SetSceneAirWall();
        }
       
        /// <summary>
        /// 设置场景中的空气墙
        /// </summary>
        void SetSceneAirWall()
        {
            if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
            {
                if (podiumAirWall != null)
                    podiumAirWall.SetActive(false);
                if (speechVrPlace != null)
                    speechVrPlace.SetActive(true);
            }
            else
            {
                if (podiumAirWall != null)
                    podiumAirWall.SetActive(true);
                if (speechVrPlace != null)
                    speechVrPlace.SetActive(false);
            }
        }
    }
}
