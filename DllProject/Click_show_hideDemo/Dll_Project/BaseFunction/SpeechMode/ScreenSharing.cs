using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace Dll_Project.BaseFunction.SpeechMode
{
    /// <summary>
    ///  所有小屏幕共享大屏
    /// </summary>
    public class ScreenSharing : DllGenerateBase
    {
        public List<Transform> allTextureObj;
        private string propertyName = "_MainTex";
        /// <summary>
        /// 是否正在分享
        /// </summary>
        public bool isSharing;
        public Texture initialTexture;  //初始所有屏幕显示tex
        public Texture curTexture;  //当前屏幕显示tex
        public void InitFunction()
        {
            allTextureObj = new List<Transform>();
            for (int i = 0; i < BaseMono.ExtralDatas.Length; i++)
            {
                allTextureObj.Add(BaseMono.ExtralDatas[i].Target);
            }
            initialTexture = allTextureObj[0].GetComponent<RawImage>().texture;
        }
        public override void Update()
        {
            if (WhiteBoard != null && WhiteBoard.gameObject.activeInHierarchy)
            {
                //正在分享
                isSharing = true;
                BigScreenShowShareScreen(WhiteBoard.gameObject);
            }
            else if (isSharing)
            {
               // Debug.Log("停止分享");
                isSharing = false;
                Texture texture = null;
                texture = curTexture == null ? initialTexture : curTexture;
                for (int i = 0; i < allTextureObj.Count; i++)
                {
                    allTextureObj[i].transform.localScale = new Vector3(allTextureObj[i].transform.localScale.x, Math.Abs(allTextureObj[i].transform.localScale.y), allTextureObj[i].transform.localScale.z);
                }
                SetCurTexture(texture);
            }
        }
        Transform WhiteBoard;
        public override void Start()
        {
            if (mStaticThings.I == null) return;
            if (mStaticThings.I.BigscreenRoot != null)
            {
                WhiteBoard = mStaticThings.I.BigscreenRoot.Find("ScreenRoot/Canvas_Picture/Canvas_PIC/Panel/RawImage/whiteboard");

                if (WhiteBoard == null)
                {
                    WhiteBoard = mStaticThings.I.BigscreenRoot.Find("ScreenRoot/Canvas_Picture/Canvas_PIC/Panel/RawImage/WhiteBoard");
                }

            }
            if (mStaticThings.I.nowAvatarFrameList.chdata != null)
            {
                if (mStaticThings.I.nowAvatarFrameList.chdata.ContainsKey("nowroomPDFshowing") && mStaticThings.I.nowAvatarFrameList.chdata["nowroomPDFshowing"] != "")
                {
                    if (mStaticThings.I.BigscreenRoot != null)
                    {
                        GameObject screenRoot = mStaticThings.I.BigscreenRoot.Find("ScreenRoot").gameObject;
                        if (screenRoot != null)
                        {
                            Texture texture = screenRoot.transform.Find("Canvas_Picture/Canvas_PIC/Panel/RawImage").GetComponent<RawImage>().texture;
                            if (texture != null)
                            {
                                for (int i = 0; i < allTextureObj.Count; i++)
                                {
                                    allTextureObj[i].GetComponent<RawImage>().texture = texture;
                                }
                            }
                        }
                    }

                }
            }
        }
        public override void OnEnable()
        {
            InitFunction();
            MessageDispatcher.AddListener(VrDispMessageType.BigScreenShowImage.ToString(), BigScreenShowImage);
            MessageDispatcher.AddListener(VrDispMessageType.BigScreenUpdateImage.ToString(), BigScreenShowImage);
            MessageDispatcher.AddListener(VrDispMessageType.BigScreenShowVideoFrame.ToString(), BigScreenShowImage);
            MessageDispatcher.AddListener(VrDispMessageType.BigScreenShowVideo.ToString(), BigScreenShowVideo);
            MessageDispatcher.AddListener(VrDispMessageType.BigScreenPrepareVideo.ToString(), BigScreenShowVideo);
            MessageDispatcher.AddListener(VrDispMessageType.BigScreenRecieveRTSP.ToString(), BigScreenShowVideo);
        }
        public override void OnDisable()
        {
            texShareScreen = null;
            textureImg = null;
            texVideo = null;
            MessageDispatcher.RemoveListener(VrDispMessageType.BigScreenShowImage.ToString(), BigScreenShowImage);
            MessageDispatcher.RemoveListener(VrDispMessageType.BigScreenUpdateImage.ToString(), BigScreenShowImage);
            MessageDispatcher.RemoveListener(VrDispMessageType.BigScreenShowVideoFrame.ToString(), BigScreenShowImage);
            MessageDispatcher.RemoveListener(VrDispMessageType.BigScreenShowVideo.ToString(), BigScreenShowVideo);
            MessageDispatcher.RemoveListener(VrDispMessageType.BigScreenPrepareVideo.ToString(), BigScreenShowVideo);
            MessageDispatcher.RemoveListener(VrDispMessageType.BigScreenRecieveRTSP.ToString(), BigScreenShowVideo);
            BaseMono.StopAllCoroutines();
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        Texture2D textureImg;
        private void BigScreenShowImage(IMessage msg)
        {
            //Debug.LogError("BigScreenShowImage");
            textureImg = msg.Data as Texture2D;
            if (textureImg == null)
            {
                return;
            }
            for (int i = 0; i < allTextureObj.Count; i++)
            {
                allTextureObj[i].transform.localScale = new Vector3(allTextureObj[i].transform.localScale.x, Math.Abs(allTextureObj[i].transform.localScale.y), allTextureObj[i].transform.localScale.z);
            }
            curTexture = textureImg;
            SetCurTexture(textureImg);
        }
        Texture texVideo;
        private void BigScreenShowVideo(IMessage msg)
        {
            //Debug.LogError("BigScreenShowVideo");
            texVideo = msg.Data as Texture;
            if (texVideo == null)
            {
                return;
            }

            for (int i = 0; i < allTextureObj.Count; i++)
            {
                allTextureObj[i].transform.localScale = new Vector3(allTextureObj[i].transform.localScale.x, Math.Abs(allTextureObj[i].transform.localScale.y), allTextureObj[i].transform.localScale.z);
            }
            curTexture = texVideo;
            SetCurTexture(texVideo);
        }
        Texture texShareScreen;
        private void BigScreenShowShareScreen(GameObject whiteboard)
        {
            //Debug.LogError("BigScreenShowShareScreen");
            texShareScreen = whiteboard.GetComponent<RawImage>().texture;
            if (texShareScreen == null)
            {
                return;
            }
            for (int i = 0; i < allTextureObj.Count; i++)
            {
                allTextureObj[i].transform.localScale = new Vector3(allTextureObj[i].transform.localScale.x, -Math.Abs(allTextureObj[i].transform.localScale.y), allTextureObj[i].transform.localScale.z);
            }
            SetCurTexture(texShareScreen);
        }

        private void SetTextureToMa(Texture texture, Material material)
        {
            material.SetTexture(propertyName, texture);
            allTextureObj[0].GetComponent<MeshRenderer>().material = material;
            for (int i = 0; i < allTextureObj.Count; i++)
            {
                allTextureObj[i].GetComponent<RawImage>().texture = texture;
            }
        }
        private void SetCurTexture(Texture texture)
        {
            for (int i = 0; i < allTextureObj.Count; i++)
            {
                allTextureObj[i].GetComponent<RawImage>().texture = texture;
            }
        }   
    }
}
