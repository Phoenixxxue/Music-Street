using DG.Tweening;
using static UnityEngine.UI.CanvasScaler;
using System.Collections;
using com.ootii.Messages;
using Dll_Project.BaseUI;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Dll_Project.BaseUI
{
    public class UIControl : DllGenerateBase
    {
        public static UIControl Instanse;
        public static bool commandson = false;
        public bool isShowToggleGroup = false;

        private Transform cameraTemp;
        private GameObject UICanvasObj;
        private Canvas UICanvas;
        private GameObject UITemp;
        private Canvas UITempCanvas;
        private GameObject UnderMenuPanel;
        private Transform toggleGroud;

        private List<Toggle> toggles = new List<Toggle>();

        public override void Init()
        {
            cameraTemp = BaseMono.ExtralDatas[0].Target;
            UICanvasObj = BaseMono.ExtralDatas[1].Target.gameObject;
            toggleGroud = BaseMono.ExtralDatas[3].Target;
        }
        public override void Awake()
        {
            Instanse = this;
        }

        public override void Start()
        {
            UICanvas = UICanvasObj.GetComponent<Canvas>();
            UITemp = GameObject.Find("LoginUICanvas");
            if (UITemp != null)
            {
                UITempCanvas = UITemp.GetComponent<Canvas>();
                UnderMenuPanel = UITemp.transform.Find("UnderMenuPanel").gameObject;
            }
            Transform toggleGround = UICanvasObj.transform.Find("ToggleGroud");
            Debug.Log(toggleGround.name);
            for (int i = 2; i < UICanvasObj.transform.Find("ToggleGroud").childCount; i++)
            {
                toggles.Add(UICanvasObj.transform.Find("ToggleGroud").GetChild(i).GetComponentInChildren<Toggle>());
            }
            toggleGroud.GetChild(0).GetComponent<Button>().onClick.AddListener(ScaleToggleGroud);
            //SetPlayerPlace();
            //IsVR();
        }

        public override void OnEnable()
        {
            //分别展示手机端和PC端UI大小
            if (mStaticThings.I != null)
            {
                if (mStaticThings.I.ismobile)
                {
                    UICanvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1050, 600);
                    UICanvasObj.GetComponent<CanvasScaler>().screenMatchMode = ScreenMatchMode.Expand;
                }
                else
                {
                    UICanvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1400, 800);
                    UICanvasObj.GetComponent<CanvasScaler>().screenMatchMode = ScreenMatchMode.Expand;
                }
            }


            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMesh);
        }

        public override void Update()
        {
            if (mStaticThings.I != null)
            {
                cameraTemp.position = mStaticThings.I.Maincamera.position;
                cameraTemp.rotation = mStaticThings.I.Maincamera.rotation;
            }

            LimitCamera();

            if (Input.GetKeyUp(KeyCode.F12))
            {
                commandson = !commandson;
            }
            if (!commandson)
            {
                ShowOrHideUI();
                return;
            }

            if (Input.GetKeyUp(KeyCode.C))
            {
                UICanvas.enabled = !UICanvas.enabled;
            }

            //if (mStaticThings.I != null)
            //{
            //    if (mStaticThings.I.isVRApp)
            //    {
            //        FollowCamera(-15, 0.65f);
            //    }
            //}
        }

        /// <summary>
        /// 缩放右上角按钮列表
        /// </summary>
        public void ScaleToggleGroud()
        {
            mStaticData.IsOpenPointClick = false;
            if (isShowToggleGroup)
            {
                BaseMono.StartCoroutine(OpenToggleGroup());
            }
            else
            {
                for (int i = 0; i < toggleGroud.childCount - 1; i++)//关闭屏幕弹窗
                {
                    if (i == 0)
                    {
                        toggleGroud.GetChild(i).GetComponent<Image>().enabled = true;
                    }
                    else if (i == 1) { }
                    else
                    {
                        toggleGroud.GetChild(i).GetChild(0).GetComponent<Toggle>().isOn = false;
                    }
                }
                BaseMono.StartCoroutine(CloseToggleGroup());
            }
        }
        private IEnumerator OpenToggleGroup()
        {
            yield return new WaitForSeconds(0.05f);
            toggleGroud.GetChild(0).GetComponent<Image>().enabled = false;
            toggleGroud.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
            isShowToggleGroup = false;
            toggleGroud.GetChild(0).Find("Image").DORotate(new Vector3(0, 0, 180), 0.3f);
            mStaticData.IsOpenPointClick = true;
        }
        private IEnumerator CloseToggleGroup()
        {
            yield return new WaitForSeconds(0.05f);
            toggleGroud.GetComponent<RectTransform>().anchoredPosition = new Vector2(toggleGroud.GetComponent<RectTransform>().rect.width - toggleGroud.GetChild(0).GetComponent<RectTransform>().rect.width, -60);
            isShowToggleGroup = true;
            toggleGroud.GetChild(0).Find("Image").DORotate(new Vector3(0, 0, 0), 0.3f);

            mStaticData.IsOpenPointClick = true;
        }

        public void FollowCamera(float angle, float distocamera)
        {
            if (mStaticThings.I == null) return;
            if (mStaticThings.I.Maincamera == null && !mStaticThings.I.isVRApp) return;
            if (UICanvasObj != null)
            {
                UICanvasObj.transform.forward = mStaticThings.I.Maincamera.forward;
                Quaternion rotate = Quaternion.AngleAxis(angle, mStaticThings.I.Maincamera.right);

                UICanvasObj.transform.position = mStaticThings.I.Maincamera.position + rotate * mStaticThings.I.Maincamera.forward.normalized * distocamera;

                UICanvasObj.transform.position = new Vector3(UICanvasObj.transform.position.x, UICanvasObj.transform.position.y - 0.2f, UICanvasObj.transform.position.z);
            }
        }
        //VR端UI切换成全局UI
        private void IsVR()
        {
            if (mStaticThings.I == null)
                return;
            if (mStaticThings.I.isVRApp)
            {
                UICanvas.renderMode = RenderMode.WorldSpace;
                UICanvas.worldCamera = mStaticThings.I.Maincamera.GetComponent<Camera>();
                UICanvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                UICanvasObj.transform.SetParent(GameObject.Find("CameraTemp").transform);
                UICanvasObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0.7f);
            }
        }
        void TelePortToMesh(IMessage msg)
        {
            string dname = (string)msg.Data;
            if (dname.Equals("ground_jiaban"))
            {
                SetPlayerPlace();
            }
        }

        /// <summary>
        /// 设置物体从玻璃房传送离开，回来时，位置在出现在玻璃房
        /// </summary>
        private void SetPlayerPlace()
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(mStaticThings.I.nowRoomChID + "savePostion")))
            {
                CharacterController control = mStaticThings.I.MainVRROOT.GetComponent<CharacterController>();
                if (control != null)
                    control.enabled = false;

                mStaticThings.I.MainVRROOT.position = new Vector3(UnityEngine.Random.Range(0, 6), 0, UnityEngine.Random.Range(42, 48));
                if (control != null)
                    control.enabled = true;
                PlayerPrefs.SetString(mStaticThings.I.nowRoomChID + "savePostion", null);
            }
        }

        //控制场景包中UI的显隐
        private void ShowOrHideUI()
        {
            if (!mStaticThings.I.isVRApp)
            {
                if (UITemp != null && UITempCanvas.enabled == true)
                {
                    if (UnderMenuPanel.activeSelf == false && UICanvas.enabled == false && cameraTemp.childCount == 0)
                    {
                        UICanvas.enabled = true;
                        MessageDispatcher.SendMessageData("SwitcPersionViewModeReq", 0);
                    }
                    else if (UnderMenuPanel.gameObject.activeSelf == true && UICanvas.enabled == true)
                    {
                        UICanvas.enabled = false;
                    }
                }
            }

            if (UICanvas.enabled == false)
            {
                for (int i = 0; i < toggles.Count; i++)
                {
                    if (toggles[i].isOn == true)
                    {
                        toggles[i].isOn = false;
                    }
                }
            }
        }
        /// <summary>
        /// 限制摄像机翻转视角
        /// </summary>
        private void LimitCamera()
        {
            if (mStaticThings.I != null)
            {
                if (mStaticThings.I.Maincamera.localRotation.x < -0.5373)
                {
                    mStaticThings.I.Maincamera.localEulerAngles = new Vector3(-65, 0, 0);
                }
                else if (mStaticThings.I.Maincamera.localRotation.x > 0.5373)
                {
                    mStaticThings.I.Maincamera.localEulerAngles = new Vector3(65, 0, 0);
                }
            }
        }

    }
}
