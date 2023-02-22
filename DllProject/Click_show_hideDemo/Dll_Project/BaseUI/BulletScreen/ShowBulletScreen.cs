using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections;
using System.Text.RegularExpressions;

namespace Dll_Project.BaseUI.BulletScreen
{
    public class ShowBulletScreen : DllGenerateBase
    {
        public static ShowBulletScreen Instance;
        private Transform bulletScreenPanel;
        private InputField sendInputField;
        private Button sendButton;
        private Transform recievePanel;

        private Toggle BulletScreenToggle;
        private Transform ToggleGroud;

        private Queue<string> bulletQueue; //弹幕列队
        public float bulletSpeed = 3; //弹幕速度
        private int maxCowCount = 10;  //弹幕最大行数
        public bool[] cowsStatus;  //弹幕是否可以发射
        public float ScreenWidth; //屏幕宽度（目前运行时候窗口拉长会导致弹幕提前消失，每帧获取可以fix）

        private string specialArea = "PaintedEggshell";
        private ParticleSystem specialEffect;
        private List<string> specialWord = new List<string>();
        private Transform specialEffectRoot;
        private ParticleSystem[] partics;
        public override void Init()
        {
            specialWord.Add("来点烟花");
            specialWord.Add("来点彩旗");


            bulletScreenPanel = BaseMono.ExtralDatas[0].Target;
            sendInputField = BaseMono.ExtralDatas[1].Target.GetComponent<InputField>();
            sendButton = BaseMono.ExtralDatas[2].Target.GetComponent<Button>();

            recievePanel = BaseMono.ExtralDatas[3].Target;

            BulletScreenToggle = BaseMono.ExtralDatas[4].Target.GetComponent<Toggle>();
            ToggleGroud = BaseMono.ExtralDatas[5].Target;

            ScreenWidth = BaseMono.ExtralDatas[6].Target.GetComponent<RectTransform>().sizeDelta.x;

            specialEffect = BaseMono.ExtralDatas[7].Target.GetComponent<ParticleSystem>();
            specialEffectRoot = BaseMono.ExtralDatas[8].Target;


            bulletQueue = new Queue<string>();
            //初始化弹幕数据
            cowsStatus = new bool[maxCowCount];
            for (int i = 0; i < maxCowCount; i++)
            {
                cowsStatus[i] = true;
            }
            //手机端速度减慢
            if (mStaticThings.I.ismobile)
            {
                bulletSpeed *= 1.2f;
            }
        }
        #region 初始
        public override void Awake()
        {
            Instance = this;
        }

        public override void Start()
        {
            sendButton.onClick.AddListener(SendBulletScreen);
            BulletScreenToggle.onValueChanged.AddListener(ToggleClick);
            sendInputField.onEndEdit.AddListener(sendInput);

            partics = specialEffectRoot.GetComponentsInChildren<ParticleSystem>();

            sendTrigger();
        }

        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshEvent);
            //MessageDispatcher.AddListener(VrDispMessageType.SetAdmin.ToString(), OnAdminChangedevent);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshEvent);
            //MessageDispatcher.RemoveListener(VrDispMessageType.SetAdmin.ToString(), OnAdminChangedevent);
        }
        public override void OnDestroy()
        {
            OnDisable();
        }
        bool IsShooting = false;
        public override void Update()
        {
            if (bulletQueue.Count > 0 && !IsShooting)
            {
                BaseMono.StartCoroutine(CreateBullet(bulletQueue.Dequeue()));
                IsShooting = true;
            }

            if ((mStaticThings.I.isAdmin || mStaticThings.I.sadmin))
            {
                if (!BulletScreenToggle.transform.parent.gameObject.activeSelf)
                {
                    BulletScreenToggle.transform.parent.gameObject.SetActive(true);
                    UIControl.Instanse.isShowToggleGroup = true;
                    UIControl.Instanse.ScaleToggleGroud();
                }
            }
            else
            {
                if (BulletScreenToggle.transform.parent.gameObject.activeSelf)
                {
                    BulletScreenToggle.transform.parent.gameObject.SetActive(false);
                    BulletScreenToggle.isOn = false;
                    UIControl.Instanse.isShowToggleGroup = true;
                    UIControl.Instanse.ScaleToggleGroud();
                }
            }
        }
        #endregion

        private void SendBulletScreen()
        {
            if (mStaticThings.I != null)
            {
                if (!string.IsNullOrEmpty(sendInputField.text))
                {
                    WsCChangeInfo wsinfo = new WsCChangeInfo()
                    {
                        a = mStaticThings.I.nowRoomStartChID + "SendBulletScreen",
                        b = mStaticData.AvatorData.name + ":" + sendInputField.text
                    };
                    MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
                }
                if (curMeshName == specialArea)
                {
                    for (int i = 0; i < specialWord.Count; i++)
                    {
                        if (sendInputField.text.Contains(specialWord[i]))
                        {

                            WsCChangeInfo wsinfo1 = new WsCChangeInfo()
                            {
                                a = "PaintedEggShell",
                            };
                            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0);
                            break;
                        }
                    }
                }

                sendInputField.text = null;
            }
        }
        //当前地面
        private string curMeshName;
        private void TelePortToMeshEvent(IMessage rMessage)
        {
            if (rMessage == null || rMessage.Data == null)
            {
                return;
            }
            curMeshName = rMessage.Data as string;
        }

        private void RecieveCChangeObj(IMessage msg)
        {
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            if (info.a == mStaticThings.I.nowRoomStartChID + "SendBulletScreen")
            {
                bulletQueue.Enqueue(info.b);
            }

            if (info.a == "PaintedEggShell")
            {
                if (!specialEffect.isPlaying)
                {
                    Debug.Log("specialEffect Play");
                    specialEffectRoot.gameObject.SetActive(!specialEffect.isPlaying);
                    specialEffect.Play();

                    for (int i = 0; i < partics.Length; i++)
                    {
                        partics[i].Play();
                    }
                }
            }
        }
        /// <summary>
        /// 发射弹幕
        /// </summary>
        /// <param name="bulletStr">弹幕内容</param>
        /// <param name="bulletDelayTime">弹幕延迟发射时间</param>
        /// <returns></returns>
        IEnumerator CreateBullet(string bulletStr, float bulletDelayTime = 0)
        {
            yield return new WaitForSeconds(bulletDelayTime);

            float height = -120; //弹幕高度
            int index = 0; //弹幕序号
            for (int i = 0; i < maxCowCount; i++)
            {
                //弹幕冷却中
                if (!cowsStatus[i])
                {
                    height -= 40;
                    //最后一个也不能发射弹幕则延迟一段时间再判断
                    if (i == maxCowCount - 1)
                    {
                        BaseMono.StartCoroutine(CreateBullet(bulletStr, 0.5f));
                        yield break;
                    }
                }
                else
                {
                    index = i;
                    break;
                }
            }
            var tempClone = GameObject.Instantiate(recievePanel, recievePanel.parent);
            tempClone.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, height);
            tempClone.GetComponent<GeneralDllBehavior>().OtherData = bulletStr;
            tempClone.GetComponent<GeneralDllBehavior>().ExtralDatas[1].Target = BaseMono.transform;
            tempClone.GetComponent<GeneralDllBehavior>().ExtralDatas[1].OtherData = index.ToString();
            tempClone.gameObject.SetActive(true);
            IsShooting = false;
        }
        private void OnPointClickEvent(IMessage msg)
        {
            GameObject go = msg.Data as GameObject;
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                {
                    if (bulletScreenPanel.gameObject.activeSelf)
                    {
                        if (BulletScreenToggle.isOn == true)
                            BulletScreenToggle.isOn = false;
                    }
                }
            }
        }
        private void ToggleClick(bool isOn)
        {
            if (isOn)
            {
                BaseMono.StartCoroutine(OpenBullet(isOn));
            }
            else
            {
                BaseMono.StartCoroutine(CloseBullet(isOn));
            }
            sendInputField.text = null;
        }
        private IEnumerator OpenBullet(bool isOn)
        {
            yield return new WaitForSeconds(0.03f);
            bulletScreenPanel.gameObject.SetActive(isOn);
            mStaticData.IsOpenPointClick = false;
            var x = ToggleGroud.GetComponent<RectTransform>().rect.width + 10;
            bulletScreenPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-x, -60);
            SaveInfo.instance.SaveActionData("BulletScreen", 10);
        }
        private IEnumerator CloseBullet(bool isOn)
        {
            yield return new WaitForSeconds(0.03f);
            bulletScreenPanel.gameObject.SetActive(isOn);
            mStaticData.IsOpenPointClick = true;
        }
        //控制弹幕文字字数
        private void sendInput(string info)
        {
            info = FilterEmoji(info);
            if (info.Length > 30)
            {
                sendInputField.text = info.Substring(0, 30);
                return;
            }
        }
        List<string> patten = new List<string>() { @"\p{Cs}", @"\p{Co}", @"\p{Cn}", @"^[\u2702-\u27B0]+$", @"^[\uD83D][\uDE01-\uDE4F]+$" };
        private string FilterEmoji(string str)
        {
            for (int i = 0; i < patten.Count; i++)
            {
                str = Regex.Replace(str, patten[i], "");//屏蔽emoji   
            }
            return str;
        }


        private void sendTrigger()
        {
            EventTrigger eventTrigger = sendInputField.GetComponent<EventTrigger>();
            eventTrigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry enterpriseEntry = new EventTrigger.Entry
            {
                //事件类型
                eventID = EventTriggerType.PointerClick,
                callback = new EventTrigger.TriggerEvent()
                //创建回调函数
            };
            enterpriseEntry.callback.AddListener((data) =>
            {

                InputFieldClick(sendInputField);
            });
            eventTrigger.triggers.Add(enterpriseEntry);
        }

        private void InputFieldClick(InputField fd)
        {
            MessageDispatcher.SendMessage(fd, VrDispMessageType.InputFildClicked.ToString(), fd.text, 0);
        }
    }
}
