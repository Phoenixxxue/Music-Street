using System;
using UnityEngine;
using com.ootii.Messages;
using UnityEngine.UI;
using DG.Tweening;

namespace Dll_Project.BaseUI

{
    /// <summary>
    /// 控制加载出来的GLB模型的位置
    /// </summary>
    public class GLBController : DllGenerateBase
    {
        #region 属性或字段
        private GameObject controllerPanel;
        private GameObject x_Axis_Add;
        private GameObject x_Axis_Minus;
        private GameObject z_Axis_Add;
        private GameObject z_Axis_Minus;
        private GameObject y_Axis_Slider;
        private Vector3 defalutPos;//模型初始位置

        //注：变化使用的是加减法
        private float positionChangeValue = 0.25f;
        public GameObject GlbModel;
        private float maxHeight = 3f;//Y轴最大平移高度
        private Vector3 maxSize;//3*3*3
        #endregion

        #region 方法重写
        public override void Init()
        {
            controllerPanel = BaseMono.ExtralDatas[0].Target.gameObject;

            x_Axis_Add = BaseMono.ExtralDatas[1].Target.gameObject;
            x_Axis_Minus = BaseMono.ExtralDatas[2].Target.gameObject;
            z_Axis_Add = BaseMono.ExtralDatas[3].Target.gameObject;
            z_Axis_Minus = BaseMono.ExtralDatas[4].Target.gameObject;

            y_Axis_Slider = BaseMono.ExtralDatas[5].Target.gameObject;

            maxSize = new Vector3(3, 3, 3);
            base.Init();
        }
        public override void Start()
        {
            x_Axis_Add.GetComponent<Button>().onClick.AddListener(delegate { X_axis_Change(1); });
            x_Axis_Minus.GetComponent<Button>().onClick.AddListener(delegate { X_axis_Change(-1); });

            z_Axis_Add.GetComponent<Button>().onClick.AddListener(delegate { Z_axis_Change(1); });
            z_Axis_Minus.GetComponent<Button>().onClick.AddListener(delegate { Z_axis_Change(-1); });

            y_Axis_Slider.GetComponent<Slider>().onValueChanged.AddListener((float value) => { Y_axis_Change(value); });
            base.Start();
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.AddListener(VrDispMessageType.LoadGlbModelsDone.ToString(), LoadGlbModelsDown);
            base.OnEnable();
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.RemoveListener(VrDispMessageType.LoadGlbModelsDone.ToString(), LoadGlbModelsDown);
            base.OnDisable();
        }

        public override void Update()
        {
            if (mStaticThings.I != null)
            {
                if (Time.frameCount % 30 == 0)
                {
                    if (mStaticThings.I.GlbOjbRoot != null)
                    {
                        if (GlbModel == null)
                        {
                            controllerPanel.SetActive(false);
                        }
                    }
                }
            }

            base.Update();
        }


        public override void OnDestroy()
        {
            OnDisable();
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            base.OnDestroy();
        }
        #endregion

        #region 方法监听
        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (msg == null || msg.Data == null)
            {
                return;
            }
            if (GlbModel != null)
            {

                #region 平移
                if (info.a == "X_axis_Change")//X轴
                {
                    if (info.b == "ADD")
                    {
                        GlbModel.transform.DOLocalMoveX(GlbModel.transform.position.x + positionChangeValue, 0.1f).SetAutoKill(true);
                    }
                    else
                    {
                        GlbModel.transform.DOLocalMoveX(GlbModel.transform.position.x - positionChangeValue, 0.1f).SetAutoKill(true);
                    }
                }
                if (info.a == "Y_axis_Change")//Y轴
                {
                    defalutPos.x = GlbModel.transform.position.x;
                    defalutPos.z = GlbModel.transform.position.z;
                    if (float.TryParse(info.b, out float value))
                    {
                        GlbModel.transform.position = new Vector3(0, value * maxHeight, 0) + defalutPos;
                    }

                }
                if (info.a == "Z_axis_Change")//Z轴
                {
                    if (info.b == "ADD")
                    {
                        GlbModel.transform.DOLocalMoveZ(GlbModel.transform.position.z + positionChangeValue, 0.1f).SetAutoKill(true);
                    }
                    else
                    {
                        GlbModel.transform.DOLocalMoveZ(GlbModel.transform.position.z - positionChangeValue, 0.1f).SetAutoKill(true);

                    }
                }
                #endregion

            }
        }

        private void LoadGlbModelsDown(IMessage msg)
        {
            if (mStaticThings.I.isVRApp)
            {
                return;
            }
            if (msg == null || msg.Data == null)
            {
                return;
            }
            GlbSceneObjectFile newglb = msg.Data as GlbSceneObjectFile;
            if (newglb.glbobj != null)
            {
                //显示UI界面
                if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                {
                    controllerPanel.SetActive(true);
                }
                //end

                //将Glb模型赋值
                GlbModel = newglb.glbobj;
                //end

                //初始化Glb模型的尺寸
                if (GlbModel != null)
                {
                    GlbModel.SetActive(false);
                    Vector3 size;
                    if (GlbModel.transform.GetComponent<Renderer>() != null)
                    {
                        size = GlbModel.transform.GetComponent<Renderer>().bounds.size;
                    }
                    else
                    {
                        size = GlbModel.GetComponentInChildren<Renderer>().bounds.size;
                    }

                    if (size.x > maxSize.x || size.y > maxSize.y || size.z > maxSize.z)
                    {
                        float scaleX = size.x / maxSize.x;
                        float scaleY = size.y / maxSize.y;
                        float scaleZ = size.z / maxSize.z;
                        float maxScale = Mathf.Max(new float[] { scaleX, scaleY, scaleZ });
                        GlbModel.transform.localScale /= maxScale;
                    }
                    GlbModel.SetActive(true);
                    defalutPos = GlbModel.transform.localPosition;
                }
                //end
            }
        }
        /// <summary>
        /// X轴变化
        /// </summary>
        /// <param name="direction"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void X_axis_Change(int direction)
        {
            if (GlbModel == null) return;
            if (direction > 0)//正向
            {
                WsCChangeInfo wsinfo1 = new WsCChangeInfo()
                {
                    a = "X_axis_Change",
                    b = "ADD",
                };
                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0f);
            }
            else
            {
                WsCChangeInfo wsinfo1 = new WsCChangeInfo()
                {
                    a = "X_axis_Change",
                    b = "MINUS",
                };
                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0f);
            }

        }

        /// <summary>
        /// Y轴变化
        /// </summary>
        /// <param name="direction"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Y_axis_Change(int direction)
        {
            if (direction > 0)//正向
            {
                WsCChangeInfo wsinfo1 = new WsCChangeInfo()
                {
                    a = "Y_axis_Change",
                    b = "ADD",
                };
                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0f);
            }
            else
            {
                WsCChangeInfo wsinfo1 = new WsCChangeInfo()
                {
                    a = "Y_axis_Change",
                    b = "MINUS",
                };
                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0f);
            }
        }


        private void Y_axis_Change(float sliderValue)
        {
            WsCChangeInfo wsinfo1 = new WsCChangeInfo()
            {
                a = "Y_axis_Change",
                b = sliderValue.ToString(),
            };
            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0f);
        }

        /// <summary>
        /// Z轴变化
        /// </summary>
        /// <param name="direction"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Z_axis_Change(int direction)
        {
            if (direction > 0)//正向
            {
                WsCChangeInfo wsinfo1 = new WsCChangeInfo()
                {
                    a = "Z_axis_Change",
                    b = "ADD",
                };
                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0f);
            }
            else
            {
                WsCChangeInfo wsinfo1 = new WsCChangeInfo()
                {
                    a = "Z_axis_Change",
                    b = "MINUS",
                };
                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0f);
            }
        }
        #endregion

        #region 其他

        #endregion
    }
}