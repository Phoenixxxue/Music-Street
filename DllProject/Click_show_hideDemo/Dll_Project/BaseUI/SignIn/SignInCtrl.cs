using com.ootii.Messages;
using Dll_Project.BaseUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/***
* Developed by baohan
*/
namespace Dll_Project.BaseUI.SignIn
{
    /// <summary>
    /// 发言控制
    /// 需要LoadConfigJson，PoolManager模块
    /// </summary>
    public class SignInCtrl : DllGenerateBase
    {
        #region Variables
        public static SignInCtrl Instance;
        #region 签到页面
        private Transform SignInParent;
        private Button button_SignIn;
        private InputField inputField_SignIn;
        #endregion
        #region 签到管理页面
        private Transform SignInCtrlPanel;
        /// <summary>
        /// 签到状态选择
        /// </summary>
        private Text text_SignInTitle;
        /// <summary>
        /// 下拉框
        /// </summary>
        private Dropdown dropdown_Select;
        /// <summary>
        /// 打开签到控制面板的toggle
        /// </summary>
        private Toggle toggle_EnableSignInCtrlPanel;
        /// <summary>
        /// 打开签到控制面板投屏的button
        /// </summary>
        private Button button_EnableSignInScreen;
        /// <summary>
        /// 关闭签到控制面板投屏的button
        /// </summary>
        private Button button_DisableSignInScreen;
        /// <summary>
        /// 打开签到的button
        /// </summary>
        private Button button_EnableSignIn;
        /// <summary>
        /// 关闭签到的button
        /// </summary>
        private Button button_DisableSignIn;
        /// <summary>
        /// 补签的button
        /// </summary>
        private Button button_StartAddSignIn;
        /// <summary>
        /// 签到数量的text
        /// </summary>
        private Text text_UserCount;
        /// <summary>
        /// admin专属的组件父物体
        /// </summary>
        private Transform adminComponents;
        /// <summary>
        /// SignInList的父类，用于计算高度
        /// </summary>
        private RectTransform rt_PanelBG;
        /// <summary>
        /// 签到用户列表的组件，需要根据用户身份而改变高度
        /// </summary>
        private RectTransform rt_SignInList;
        #endregion
        /// <summary>
        /// 区域检测用到的碰撞体
        /// </summary>
        internal BoxCollider bC_SignInArea;

        /// <summary>
        /// 签到面板的tansform
        /// </summary>
        private Transform SignInPanel;

        /// <summary>
        /// canvas，用于打开签到控制面板显示在最前，防止移动端被摇杆遮盖
        /// </summary>
        private Canvas uiCanvas;

        /// <summary>
        /// 用户显示的姓名
        /// </summary>
        private string mName;
        internal string MName
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
                inputField_SignIn.text = mName;
            }
        }
        /// <summary>
        /// 当前用户的签到状态
        /// </summary>
        internal UserSignInStatu signInStatu = 0;
        /// <summary>
        /// 当前用户是否已经点击了签到按钮
        /// </summary>
        internal bool isSignedIn = false;
        /// <summary>
        /// 当前用户是否已签到成功
        /// </summary>
        internal bool isSignedInSuccess = false;
        /// <summary>
        /// 是否主持人开启签到
        /// </summary>

        bool isEnableSignIn = false;
        /// <summary>
        /// 是否开启了签到
        /// </summary>
        internal bool IsEnableSignIn
        {
            get
            {
                return isEnableSignIn;
            }
            set
            {
                if (isEnableSignIn != value)
                {
                    isEnableSignIn = value;
                    ///打开签到的时候初始化签到信息
                    if (isEnableSignIn)
                    {
                        signInStatu = UserSignInStatu.NotSignedIn;
                        SignInInfoCtrl.Instance.ResetSignInData();
                        isSignedIn = false;
                        isSignedInSuccess = false;
                        isInSignArea = false;
                    }
                    button_EnableSignIn.transform.Find("Image").gameObject.SetActive(isEnableSignIn);
                    button_DisableSignIn.transform.Find("Image").gameObject.SetActive(!isEnableSignIn);
                    button_StartAddSignIn.gameObject.SetActive(isEnableSignIn);
                    SignInInfoCtrl.Instance.ShowTip(isEnableSignIn ? "签到已开始" : "签到已结束");
                    //如果是主持人保存状态到服务器并且发送同步指令
                    if (IsAdmin)
                    {
                        SignInInfoCtrl.Instance.SaveEnableSignInInfo();
                        SignInInfoCtrl.Instance.SendEnableSignInMessage();
                    }
                }
            }
        }
        /// <summary>
        /// 是否开启签到屏幕投放
        /// </summary>
        bool isEnableSignInScreen = false;
        internal bool IsEnableSignInScreen
        {
            get
            {
                return isEnableSignInScreen;
            }
            set
            {
                if (isEnableSignInScreen != value)
                {
                    isEnableSignInScreen = value;
                    button_EnableSignInScreen.transform.Find("Image").gameObject.SetActive(isEnableSignInScreen);
                    button_DisableSignInScreen.transform.Find("Image").gameObject.SetActive(!isEnableSignInScreen);
                    //如果是主持人发送信息
                    if (IsAdmin)
                    {
                        SignInInfoCtrl.Instance.SaveEnableSignInScreenInfo();
                        SignInInfoCtrl.Instance.SendEnableSignInScreenMessage();
                    }
                    //非主持人根据是否在签到区域打开签到面板
                    else
                    {
                        EnableSignInCtrlPanel(isInSignArea && value);
                    }
                }
            }
        }
        /// <summary>
        /// 是否开启签到功能
        /// </summary>
        internal bool isEnableFunction = false;

        /// <summary>
        /// 需要显示的用户的列表
        /// </summary>
        internal List<string> userShowList = new List<string>();
        /// <summary>
        /// 用户签到字典
        /// id-SignInItemShowCtrl
        /// </summary>
        internal Dictionary<string, SignInItemShowCtrl> dic_SignInItem = new Dictionary<string, SignInItemShowCtrl>();

        private bool isAdmin = false;
        /// <summary>
        /// 根据身份判断是否有打开管理页面的权限，根据需要改变
        /// </summary>
        internal bool IsAdmin
        {
            get
            {
                return isAdmin;
            }
            set
            {
                isAdmin = value;
            }
        }
        /// <summary>
        /// 是否在签到区域
        /// </summary>
        internal bool isInSignArea = false;
        internal enum DropDownStatu
        {
            all = 0,
            SignedIn = 1,
            NotSignedIn = 2,
            AddSignedIn = 3,
        }
        internal DropDownStatu currentDropDownStatu = DropDownStatu.all;



        /// <summary>
        /// 是否在时间内本地变量
        /// </summary>
        internal bool isInTime = false;
        internal List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();
        internal TimeSpan ts_Start;
        internal TimeSpan ts_End;
        #endregion

        #region Overrides
        public override void Init()
        {
            Debug.Log("SignIn Init");
            toggle_EnableSignInCtrlPanel = BaseMono.ExtralDatas[0].Target.GetComponent<Toggle>();
            SignInCtrlPanel = BaseMono.ExtralDatas[1].Target;
            text_UserCount = BaseMono.ExtralDatas[2].Target.GetComponent<Text>();
            dropdown_Select = BaseMono.ExtralDatas[3].Target.GetComponent<Dropdown>();
            text_SignInTitle = BaseMono.ExtralDatas[4].Target.GetComponent<Text>();
            rt_SignInList = BaseMono.ExtralDatas[5].Target.GetComponent<RectTransform>();
            rt_PanelBG = BaseMono.ExtralDatas[6].Target.GetComponent<RectTransform>();
            SignInParent = BaseMono.ExtralDatas[7].Target;
            button_StartAddSignIn = BaseMono.ExtralDatas[8].Target.GetComponent<Button>();
            button_EnableSignIn = BaseMono.ExtralDatas[9].Target.GetComponent<Button>();
            button_DisableSignIn = BaseMono.ExtralDatas[10].Target.GetComponent<Button>();
            button_EnableSignInScreen = BaseMono.ExtralDatas[11].Target.GetComponent<Button>();
            button_DisableSignInScreen = BaseMono.ExtralDatas[12].Target.GetComponent<Button>();
            bC_SignInArea = BaseMono.ExtralDatas[13].Target.GetComponent<BoxCollider>();
            SignInPanel = BaseMono.ExtralDatas[14].Target;
            button_SignIn = BaseMono.ExtralDatas[15].Target.GetComponent<Button>();
            inputField_SignIn = BaseMono.ExtralDatas[16].Target.GetComponent<InputField>();
            adminComponents = BaseMono.ExtralDatas[17].Target;
            uiCanvas = BaseMono.ExtralDatas[18].Target.GetComponent<Canvas>();
        }

        public override void Awake()
        {
            Instance = this;
        }
        public override void Start()
        {
            toggle_EnableSignInCtrlPanel.transform.parent.gameObject.SetActive(false);
            button_StartAddSignIn.gameObject.SetActive(false);
            toggle_EnableSignInCtrlPanel.onValueChanged.AddListener((bool ison) =>
            {
                EnableSignInCtrlPanel(ison);
            });
            button_EnableSignIn.onClick.AddListener(() => { IsEnableSignIn = true; });
            button_DisableSignIn.onClick.AddListener(() => { IsEnableSignIn = false; });
            button_EnableSignInScreen.onClick.AddListener(() => { IsEnableSignInScreen = true; });
            button_DisableSignInScreen.onClick.AddListener(() => { IsEnableSignInScreen = false; });
            //默认为mStaticThings.I.mNickName
            MName = mStaticThings.I.mNickName.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)[0];
            //默认为nickname
            //MName = inputField_SignIn.text = !String.IsNullOrEmpty(mStaticData.AvatorData.name) ? mStaticData.AvatorData.name : mStaticThings.I.mNickName.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)[0];
            //选择显示身份
            dropdown_Select.onValueChanged.AddListener((int index) =>
            {
                currentDropDownStatu = (DropDownStatu)index;
                RefreshSignInItemList();
            });
            button_StartAddSignIn.onClick.AddListener(SignInInfoCtrl.Instance.SendAddSignInMessage);
            button_SignIn.onClick.AddListener(() =>
            {
                isSignedIn = true;
                signInStatu = SignInInfoCtrl.Instance.isInSignInList ? UserSignInStatu.SignedIn : UserSignInStatu.AddSignedIn;
                EnableSignInPanel(false);
                SignInInfoCtrl.Instance.ShowTip("已发送签到请求");
                SignInInfoCtrl.Instance.SendSignInMessage(signInStatu, mName, true);
                BaseMono.StartCoroutine(SignInInfoCtrl.Instance.CheckSignInStatu(0));
            });
            inputField_SignIn.onValueChanged.AddListener((value) =>
            {
                mName = value;
            });
            MessageDispatcher.SendMessage(WsMessageType.SendGetData.ToString(), 0.2f);
        }
        private int checkFrameCount = 60;
        private int tempFrameCount = 0;
        public override void Update()
        {
            if (SignInInfoCtrl.Instance.signInConfig == null || !isEnableFunction) return;
            tempFrameCount++;
            if (tempFrameCount >= checkFrameCount)
            {
                tempFrameCount = 0;
                CheckUserIsInArea();
                if (isInTime != Utils.Utils.IsInTime(daysOfWeek, ts_Start, ts_End))
                {
                    IsEnableSignIn = isInTime = Utils.Utils.IsInTime(daysOfWeek, ts_Start, ts_End);
                }
            }
        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(VrDispMessageType.DestroyWsAvatar.ToString(), DestoryWsAvatorEvent);
            MessageDispatcher.AddListener("VRGetOneOrder", OnGetOneOrder);
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(VrDispMessageType.DestroyWsAvatar.ToString(), DestoryWsAvatorEvent);
            MessageDispatcher.RemoveListener("VRGetOneOrder", OnGetOneOrder);
        }
        public override void OnDestroy()
        {
        }
        #endregion

        #region Litseners
        private void OnGetOneOrder(IMessage rMessage)
        {
            string str = (string)rMessage.Data;
            if (str.Equals("admin") || str.Equals("qiandao"))
            {
                SignInInfoCtrl.Instance.ShowTip("您已获得签到管理权限");
                IsAdmin = true;
                if (!toggle_EnableSignInCtrlPanel.transform.parent.gameObject.activeInHierarchy)
                {
                    toggle_EnableSignInCtrlPanel.transform.parent.gameObject.SetActive(true);
                    toggle_EnableSignInCtrlPanel.isOn = false;
                }
            }
            else if (str.Equals("cadmin"))
            {
                IsAdmin = false;
                if (toggle_EnableSignInCtrlPanel.transform.parent.gameObject.activeSelf)
                {
                    toggle_EnableSignInCtrlPanel.transform.parent.gameObject.SetActive(false);
                    toggle_EnableSignInCtrlPanel.isOn = false;
                }
            }
        }
        private void DestoryWsAvatorEvent(IMessage msg)
        {
            if (msg == null || msg.Data == null) return;
            string leaverID = msg.Data as string;
            if (string.IsNullOrEmpty(leaverID)) return;
            if (SignInInfoCtrl.Instance.dic_SignInData.ContainsKey(leaverID))
            {
                SignInInfoCtrl.Instance.ShowTip(SignInInfoCtrl.Instance.dic_SignInData[leaverID].Name + "已离线");
                SignInInfoCtrl.Instance.dic_SignInData[leaverID].IsArrive = false;
                SignInInfoCtrl.Instance.dic_SignInData[leaverID].SignInStatu = UserSignInStatu.NotSignedIn;
                UpdateSignInItem(leaverID);
                if (IsAdmin) SignInInfoCtrl.Instance.SaveUserSignInInfo(leaverID);
                RefreshSignInText();
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// 刷新签到表
        /// </summary>
        internal void RefreshSignInItemList()
        {
            userShowList.Clear();
            RefreshSignInText();
            switch (currentDropDownStatu)
            {
                case DropDownStatu.all:
                    foreach (var item in SignInInfoCtrl.Instance.dic_SignInData)
                    {
                        userShowList.Add(item.Key);
                    }
                    break;
                case DropDownStatu.SignedIn:
                    foreach (var item in SignInInfoCtrl.Instance.dic_SignInData)
                    {
                        if (item.Value.SignInStatu == UserSignInStatu.SignedIn || item.Value.SignInStatu == UserSignInStatu.AddSignedIn) userShowList.Add(item.Key);
                    }
                    break;
                case DropDownStatu.NotSignedIn:
                    foreach (var item in SignInInfoCtrl.Instance.dic_SignInData)
                    {
                        if (item.Value.SignInStatu == UserSignInStatu.NotSignedIn) userShowList.Add(item.Key);
                    }
                    break;
                case DropDownStatu.AddSignedIn:
                    foreach (var item in SignInInfoCtrl.Instance.dic_SignInData)
                    {
                        if (item.Value.SignInStatu == UserSignInStatu.AddSignedIn) userShowList.Add(item.Key);
                    }
                    break;
                default:
                    foreach (var item in SignInInfoCtrl.Instance.dic_SignInData)
                    {
                        userShowList.Add(item.Key);
                    }
                    break;
            }
            for (int i = 0; i < SignInParent.childCount; i++)
            {
                //对象池
                PoolManager.Instance.PushItem(SignInParent.GetChild(i).gameObject, PoolType.SignIn);
            }
            dic_SignInItem.Clear();
            for (int i = 0; i < userShowList.Count; i++)
            {
                SignInItemShowCtrl signInItem = GetOrCreateSignInItem(userShowList[i]);
                signInItem.RefreshShowDetail(SignInInfoCtrl.Instance.dic_SignInData[userShowList[i]]);
            }
        }
        /// <summary>
        /// 刷新签到人数
        /// </summary>
        internal void RefreshSignInText()
        {
            text_UserCount.text = SignInInfoCtrl.Instance.GetSignInCountContent();
        }

        internal void UpdateSignInItem(string id)
        {
            switch (currentDropDownStatu)
            {
                case DropDownStatu.all:
                    break;
                case DropDownStatu.SignedIn:
                    if (SignInInfoCtrl.Instance.dic_SignInData[id].SignInStatu == UserSignInStatu.NotSignedIn)
                    {
                        RemoveItem(id);
                        return;
                    }
                    break;
                case DropDownStatu.NotSignedIn:
                    if (SignInInfoCtrl.Instance.dic_SignInData[id].SignInStatu != UserSignInStatu.NotSignedIn)
                    {
                        RemoveItem(id);
                        return;
                    }
                    break;
                case DropDownStatu.AddSignedIn:
                    if (SignInInfoCtrl.Instance.dic_SignInData[id].SignInStatu != UserSignInStatu.AddSignedIn)
                    {
                        RemoveItem(id);
                        return;
                    }
                    break;
                default:
                    break;
            }
            GetOrCreateSignInItem(id).RefreshShowDetail(SignInInfoCtrl.Instance.dic_SignInData[id]);
        }
        /// <summary>
        /// 获得或者创建一个签到预制体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private SignInItemShowCtrl GetOrCreateSignInItem(string id)
        {
            if (!dic_SignInItem.ContainsKey(id))
            {
                GameObject itemTemp = PoolManager.Instance.PopItem(PoolType.SignIn);
                itemTemp.name = id;
                SignInItemShowCtrl signInItem = itemTemp.GetDllComponent<SignInItemShowCtrl>();
                dic_SignInItem.Add(id, signInItem);
            }
            return dic_SignInItem[id];
        }
        /// <summary>
        /// 删除一个SignInItem
        /// </summary>
        /// <param name="id"></param>
        internal void RemoveItem(string id)
        {
            if (dic_SignInItem.ContainsKey(id))
            {
                PoolManager.Instance.PushItem(dic_SignInItem[id].BaseMono.gameObject, PoolType.SignIn);
                dic_SignInItem.Remove(id);
            }
        }
        /// <summary>
        /// 检测用户是否在签到区域内
        /// </summary>
        private void CheckUserIsInArea()
        {
            if (mStaticThings.I == null) return;
            //未打开签到
            if (!IsEnableSignIn) return;
            //不在签到列表里并且没有签到
            if (!SignInInfoCtrl.Instance.isInSignInList && !isSignedIn) return;
            bool isUserInSignInArea = Utils.Utils.IsOnCollider(mStaticThings.I.MainVRROOT.position, bC_SignInArea);
            //当是否在签到区域信息改变的时候
            if (isInSignArea != isUserInSignInArea)
            {
                isInSignArea = isUserInSignInArea;
                EnableSignInPanel(isInSignArea && !isSignedIn);
                SignInInfoCtrl.Instance.SendArriveMessage(isInSignArea, true);
            }
            if (signInStatu == UserSignInStatu.SignedIn) EnableSignInPanel(false);
        }

        /// <summary>
        /// 是否打开签到面板
        /// </summary>
        /// <param name="active"></param>
        internal void EnableSignInPanel(bool active)
        {
            if (SignInPanel.gameObject.activeInHierarchy != active) SignInPanel.gameObject.SetActive(active);
        }
        /// <summary>
        /// 是否打开签到管理(详情)面板
        /// </summary>
        /// <param name="active"></param>
        void EnableSignInCtrlPanel(bool active)
        {
            if (SignInCtrlPanel.gameObject.activeInHierarchy != active)
            {
                SignInCtrlPanel.gameObject.SetActive(active);
                uiCanvas.sortingOrder = active ? 11 : 1;
                if (active)
                {
                    text_SignInTitle.text = IsAdmin ? "签到管理" : "签到详情";
                    float listGeigt = rt_PanelBG.rect.height - 100 - (IsAdmin ? 107.5f : 7.5f);
                    rt_SignInList.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 100f, listGeigt);
                    adminComponents.gameObject.SetActive(IsAdmin);
                    RefreshSignInItemList();
                }
            }
            toggle_EnableSignInCtrlPanel.isOn = active;
        }
        /// <summary>
        /// 关闭签到功能
        /// </summary>
        internal void DisableFunction()
        {
            isEnableFunction = false;
            toggle_EnableSignInCtrlPanel.transform.parent.gameObject.SetActive(false);
            bC_SignInArea.gameObject.SetActive(false);
            OnDisable();
        }
        #endregion
    }
    /// <summary>
    /// 用户签到状态
    /// </summary>
    internal enum UserSignInStatu
    {
        NotSignedIn = 0,
        SignedIn = 1,
        AddSignedIn = 2
    }
}
