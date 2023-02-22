
using com.ootii.Messages;
using Dll_Project.BaseUI;

using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***
* Developed by baohan
*/
namespace Dll_Project.BaseUI.SignIn
{
    /// <summary>
    /// 签到的信息模块
    /// 需要LoadConfigJson模块
    /// </summary>
    internal class SignInInfoCtrl : DllGenerateBase
    {
        #region Variables
        internal static SignInInfoCtrl Instance;
        /// <summary>
        /// 读取的签到配置
        /// </summary>
        internal SignInConfig signInConfig;
        /// <summary>
        /// 补签人员列表
        /// </summary>
        List<string> addSignInList = new List<string>();
        /// <summary>
        /// 签到的人ID和对应签到状态
        /// </summary>
        internal Dictionary<string, SignInData> dic_SignInData = new Dictionary<string, SignInData>();

        #region 消息定义
        /// <summary>
        /// 用户签到消息抬头
        /// </summary>
        private const string userSignInMSG = "UserSignInMsg";
        /// <summary>
        /// Admin补签消息抬头
        /// </summary>
        private const string adminSendAddSignInMSG = "AdminAddSignInMsg";
        /// <summary>
        /// 用户到场消息抬头
        /// </summary>
        private const string userArriveMSG = "UserArriveMsg";
        /// <summary>
        /// admin同步开关签到消息抬头
        /// </summary>
        private const string adminSyncEnableSignInMSG = "SyncEnableSignInMSG";
        /// <summary>
        /// admin设置签到投屏消息抬头
        /// </summary>
        private const string adminSyncEnableSignInScreenMSG = "AdminSyncEnableSignInScreenMSG";

        #region 服务器相关
        /// <summary>
        /// 用户签到信息key
        /// </summary>
        private const string userSignInInfoKeyHead = "UserSignInInfo";
        /// <summary>
        /// 同步签到开关key
        /// </summary>
        private const string enableSignInKey = "EnableSignInInfo";
        /// <summary>
        /// 同步签到投屏key
        /// </summary>
        private const string enableSignInScreenKey = "EnableSignInScreenInfo";
        #endregion
        #endregion


        /// <summary>
        /// 用户总数
        /// </summary>
        private int totalUserCount;
        /// <summary>
        /// 已签到用户数量
        /// </summary>
        private int signedInUserCount;

        /// <summary>
        /// 当前用户是否在签到列表中
        /// </summary>
        internal bool isInSignInList = false;
        #endregion

        #region Overrides
        public override void Init()
        {

        }

        public override void Awake()
        {
            Instance = this;
        }
        public override void Start()
        {
            LoadConfigJson.Instance.LoadFinishAction += () =>
            {
                //将FloorTeleport数据转成类
                string signInDATA;
                try
                {
                    signInDATA = LoadConfigJson.Instance.mainJsonData["SigninConfig"].ToJson();
                    if (signInDATA != null)
                    {
                        signInConfig = LoadConfigJson.JsonToObjectByString<SignInConfig>(signInDATA);
                        if (signInConfig != null)
                        {
                            SignInCtrl.Instance.isEnableFunction = signInConfig.EnableSignIn;
                            if (!SignInCtrl.Instance.isEnableFunction)
                            {
                                SignInCtrl.Instance.DisableFunction();
                                return;
                            }
                            if (signInConfig.EnableTimeSwitch)
                            {
                                foreach (string str in signInConfig.DaysOfWeek)
                                {
                                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), str);
                                    SignInCtrl.Instance.daysOfWeek.Add(day);
                                }
                                SignInCtrl.Instance.ts_Start = DateTime.Parse(signInConfig.StartTime).TimeOfDay;
                                SignInCtrl.Instance.ts_End = DateTime.Parse(signInConfig.EndTime).TimeOfDay;
                            }

                            signedInUserCount = 0;
                            totalUserCount = signInConfig.UsersList.Count;
                            //初始化签到数据
                            for (int i = 0; i < signInConfig.UsersList.Count; i++)
                            {
                                //初始化姓名
                                if (!isInSignInList)
                                {
                                    if (string.Equals(signInConfig.UsersList[i].ID, mStaticThings.I.mAvatarID))
                                    {
                                        SignInCtrl.Instance.MName = signInConfig.UsersList[i].Name;
                                        isInSignInList = true;
                                    }
                                }
                                if (!dic_SignInData.ContainsKey(signInConfig.UsersList[i].ID))
                                {
                                    SignInData data = new SignInData
                                    {
                                        Name = signInConfig.UsersList[i].Name,
                                        SignInStatu = UserSignInStatu.NotSignedIn,
                                        IsArrive = false
                                    };
                                    dic_SignInData.Add(signInConfig.UsersList[i].ID, data);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("signInDATA error");
                        }
                    }
                    else
                    {
                        Debug.Log("no config");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            };
            MessageDispatcher.SendMessage(WsMessageType.SendGetData.ToString(), 0.2f);
        }
        public override void Update()
        {

        }
        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), OnRecieveCChangeObj);
            MessageDispatcher.AddListener(WsMessageType.RecieveGetData.ToString(), RecieveGetData);
        }

        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), OnRecieveCChangeObj);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveGetData.ToString(), RecieveGetData);
        }
        public override void OnDestroy()
        {
        }
        #endregion

        #region Litseners
        string tempID = string.Empty;
        private void OnRecieveCChangeObj(IMessage rMessage)
        {
            if (rMessage == null || rMessage.Data == null || dic_SignInData == null)
            {
                return;
            }
            WsCChangeInfo rinfo = rMessage.Data as WsCChangeInfo;
            switch (rinfo.a)
            {
                //签到消息处理
                case userSignInMSG:
                    if (!SignInCtrl.Instance.IsEnableSignIn) return;
                    tempID = rinfo.b;
                    if (!dic_SignInData.ContainsKey(tempID))
                    {
                        SignInData data = new SignInData
                        {
                            Name = rinfo.e,
                            SignInStatu = UserSignInStatu.NotSignedIn,
                            IsArrive = false
                        };
                        addSignInList.Add(tempID);
                        dic_SignInData.Add(tempID, data);
                    }
                    UserSignInStatu tempStatu = (UserSignInStatu)Enum.Parse(typeof(UserSignInStatu), rinfo.c);
                    dic_SignInData[tempID].SignInStatu = tempStatu;
                    dic_SignInData[tempID].Name = rinfo.e;
                    if (SignInCtrl.Instance.IsAdmin && tempStatu != UserSignInStatu.NotSignedIn)
                    {
                        if (bool.TryParse(rinfo.d, out bool isShowMessage))
                        {
                            dic_SignInData[tempID].IsArrive = true;
                            if (isShowMessage) ShowTip(dic_SignInData[tempID].Name + "已签到");
                        }
                        SaveUserSignInInfo(tempID);
                    }
                    SignInCtrl.Instance.RefreshSignInText();
                    SignInCtrl.Instance.UpdateSignInItem(tempID);
                    break;
                //范围消息处理
                case userArriveMSG:
                    tempID = rinfo.b;
                    if (dic_SignInData.ContainsKey(tempID))
                    {
                        if (bool.TryParse(rinfo.c, out bool isUserInSignArea))
                        {
                            if (dic_SignInData[tempID].IsArrive != isUserInSignArea)
                            {
                                dic_SignInData[tempID].IsArrive = isUserInSignArea;
                                if (SignInCtrl.Instance.IsAdmin)
                                {
                                    if (bool.TryParse(rinfo.d, out bool isShowMessage))
                                    {
                                        if (isShowMessage) ShowTip(dic_SignInData[tempID].Name + (isUserInSignArea ? "已进入签到范围" : "已离开签到范围"));
                                    }
                                    SaveUserSignInInfo(tempID);
                                }
                                SignInCtrl.Instance.UpdateSignInItem(tempID);
                            }
                        }
                    }
                    break;
                case adminSendAddSignInMSG:
                    //当收到补签命令时
                    if (SignInCtrl.Instance.signInStatu == UserSignInStatu.NotSignedIn && Utils.Utils.IsOnCollider(mStaticThings.I.MainVRROOT.position, SignInCtrl.Instance.bC_SignInArea))
                    {
                        SignInCtrl.Instance.EnableSignInPanel(true);
                    }
                    break;
                case adminSyncEnableSignInMSG:
                    //当收到同步签到是否开启命令时向服务器收取消息
                    VRSaveRoomData enableSignInInfo = new VRSaveRoomData
                    {
                        sall = false,
                        key = enableSignInKey
                    };
                    MessageDispatcher.SendMessage(this, WsMessageType.SendGetData.ToString(), enableSignInInfo, 0.2f);
                    break;
                case adminSyncEnableSignInScreenMSG:
                    //当收到同步签到投屏是否开启命令时向服务器收取消息
                    VRSaveRoomData enableSignInScreenInfo = new VRSaveRoomData
                    {
                        sall = false,
                        key = enableSignInScreenKey
                    };
                    MessageDispatcher.SendMessage(this, WsMessageType.SendGetData.ToString(), enableSignInScreenInfo, 0);
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Methods


        /// <summary>
        /// 刷新签到人数
        /// </summary>
        internal string GetSignInCountContent()
        {
            signedInUserCount = 0;
            foreach (var item in dic_SignInData.Values)
            {
                if (item.SignInStatu == UserSignInStatu.SignedIn) signedInUserCount++;
            }
            return signedInUserCount.ToString() + "/" + totalUserCount.ToString();
        }


        internal void RemoveUser(string id)
        {
            dic_SignInData.Remove(id);
            SignInCtrl.Instance.RemoveItem(id);
            if (SignInCtrl.Instance.IsAdmin)
            {
                DeleteUserSignInInfo(id);
            }
        }
        /// <summary>
        /// 保存签到开关信息
        /// </summary>
        internal void SaveEnableSignInInfo()
        {
            if (!SignInCtrl.Instance.IsAdmin) return;
            VRSaveRoomData enableSignInData = new VRSaveRoomData
            {
                key = enableSignInKey,
                value = SignInCtrl.Instance.IsEnableSignIn.ToString()
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), enableSignInData, 0);
        }
        /// <summary>
        /// 保存当前投屏开关状态到服务器
        /// </summary>
        internal void SaveEnableSignInScreenInfo()
        {
            if (!SignInCtrl.Instance.IsAdmin) return;
            VRSaveRoomData enableSignInScreenData = new VRSaveRoomData
            {
                key = enableSignInScreenKey,
                value = SignInCtrl.Instance.IsEnableSignInScreen.ToString()
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), enableSignInScreenData, 0);
        }
        /// <summary>
        /// 保存某人状态到服务器
        /// </summary>
        /// <param name="id"></param>
        internal void SaveUserSignInInfo(string id)
        {
            if (!SignInCtrl.Instance.IsAdmin) return;
            if (dic_SignInData.ContainsKey(id))
            {
                VRSaveRoomData signInData = new VRSaveRoomData
                {
                    key = userSignInInfoKeyHead + id,
                    value = JsonMapper.ToJson(dic_SignInData[id])
                };
                MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), signInData, 0);
            }
            else
            {
                Debug.LogError(id + " is not in dic");
            }
        }
        /// <summary>
        /// 清除某人数据
        /// </summary>
        internal void DeleteUserSignInInfo(string id)
        {
            VRSaveRoomData userData = new VRSaveRoomData
            {
                key = userSignInInfoKeyHead + id,
                isclear = true
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), userData, 0);
        }

        /// <summary>
        /// 获取当前房间临时服务器上的数据
        /// </summary>
        /// <param name="msg"></param>
        internal void RecieveGetData(IMessage msg)
        {
            Dictionary<string, string> rankDic = msg.Data as Dictionary<string, string>;
            foreach (var item in rankDic)
            {
                //同步签到功能是否开启
                if (item.Key.Equals(enableSignInKey))
                {
                    if (bool.TryParse(item.Value, out bool isEnable))
                    {
                        SignInCtrl.Instance.IsEnableSignIn = isEnable;
                    }
                }
                //同步签到投屏
                else if (item.Key.Equals(enableSignInScreenKey))
                {
                    if (bool.TryParse(item.Value, out bool isEnable))
                    {
                        SignInCtrl.Instance.IsEnableSignInScreen = isEnable;
                    }
                }
                //同步某个用户的签到状态
                else if (item.Key.Contains(userSignInInfoKeyHead))
                {
                    string userId = item.Key.Replace(userSignInInfoKeyHead, string.Empty);
                    var data = JsonMapper.ToObject<SignInData>(item.Value);
                    if (dic_SignInData != null)
                    {
                        if (SignInCtrl.Instance.isSignedIn && !SignInCtrl.Instance.isSignedInSuccess)
                        {
                            if (userId == mStaticThings.I.mAvatarID && data.SignInStatu != UserSignInStatu.NotSignedIn)
                            {
                                SignInCtrl.Instance.isSignedInSuccess = true;
                                ShowTip("签到成功");
                                BaseMono.StopCoroutine(CheckSignInStatu(0));
                            }
                        }
                        if (dic_SignInData.ContainsKey(userId))
                        {
                            dic_SignInData[userId] = data;
                        }
                        else
                        {
                            dic_SignInData.Add(userId, data);
                        }
                        SignInCtrl.Instance.UpdateSignInItem(userId);
                        SignInCtrl.Instance.RefreshSignInText();
                    }
                }
            }
        }
        /// <summary>
        /// 发送请求签到消息
        /// </summary>
        /// <param name="statu">是否签到</param>
        /// <param name="isDisplayPrompts">是否显示签到提示</param>
        internal void SendSignInMessage(UserSignInStatu statu, string name, bool isDisplayPrompts = false)
        {
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = userSignInMSG,
                b = mStaticThings.I.mAvatarID,
                c = statu.ToString(),
                d = isDisplayPrompts.ToString(),
                e = name
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
        }
        /// <summary>
        /// 发送是否在范围内消息
        /// </summary>
        /// <param name="isInArea">是否在范围内</param>
        /// <param name="isDisplayPrompts">是否显示上线提示</param>
        internal void SendArriveMessage(bool isInArea, bool isDisplayPrompts = false)
        {
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = userArriveMSG,
                b = mStaticThings.I.mAvatarID,
                c = isInArea.ToString(),
                d = isDisplayPrompts.ToString()
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
        }

        /// <summary>
        /// 重置签到信息
        /// </summary>
        internal void ResetSignInData()
        {
            if (signInConfig == null) return;
            //删除补签人员（不在表里的）
            if (addSignInList != null && addSignInList.Count > 0)
            {
                for (int i = 0; i < addSignInList.Count; i++)
                {
                    RemoveUser(addSignInList[i]);
                }
            }
            dic_SignInData.Clear();
            for (int i = 0; i < signInConfig.UsersList.Count; i++)
            {
                SignInAvatar avatar = signInConfig.UsersList[i];
                SignInData data = new SignInData
                {
                    Name = avatar.Name,
                    SignInStatu = UserSignInStatu.NotSignedIn,
                    IsArrive = false
                };
                dic_SignInData.Add(avatar.ID, data);
                if (SignInCtrl.Instance.IsAdmin)
                {
                    SaveUserSignInInfo(avatar.ID);
                }
                //延迟一点接收签到信息，否则可能会出现同步不一致的情况
                VRSaveRoomData signInInfo = new VRSaveRoomData
                {
                    sall = false,
                    key = userSignInInfoKeyHead + avatar.ID,
                };
                MessageDispatcher.SendMessage(this, WsMessageType.SendGetData.ToString(), signInInfo, 0.2f);
            }
            SignInCtrl.Instance.RefreshSignInText();
        }
        /// <summary>
        /// Admin发送补签消息
        /// </summary>
        internal void SendAddSignInMessage()
        {
            if (!SignInCtrl.Instance.IsAdmin) return;
            if (!SignInCtrl.Instance.IsEnableSignIn)
            {
                ShowTip("签到已结束，无法补签");
                return;
            }
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = adminSendAddSignInMSG
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo, 0);
        }
        /// <summary>
        /// 发送同步签到开关命令
        /// </summary>
        internal void SendEnableSignInMessage()
        {
            if (!SignInCtrl.Instance.IsAdmin) return;
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = adminSyncEnableSignInMSG
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo, 0.2f);
        }
        /// <summary>
        /// 发送同步签到投屏开关命令
        /// </summary>
        internal void SendEnableSignInScreenMessage()
        {
            if (!SignInCtrl.Instance.IsAdmin) return;
            WsCChangeInfo wsinfo = new WsCChangeInfo()
            {
                a = adminSyncEnableSignInScreenMSG
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo, 0.2f);
        }
        /// <summary>
        /// 判断是否签到成功
        /// </summary>
        /// <returns></returns>
        internal IEnumerator CheckSignInStatu(int checkTime)
        {
            yield return new WaitForSeconds(2f);
            //已点击签到但未签到成功时
            if (SignInCtrl.Instance.isSignedIn && !SignInCtrl.Instance.isSignedInSuccess)
            {
                checkTime++;
                if (checkTime < 3)
                {
                    ShowTip("因为网络波动或当前无主持人，签到失败，正在第" + checkTime + "次尝试重新签到");

                }
                else if (checkTime == 3)
                {
                    ShowTip("当前无主持人在线,待主持人上线将自动为您签到");
                }
                SendSignInMessage(SignInCtrl.Instance.signInStatu, SignInCtrl.Instance.MName, true);
                VRSaveRoomData signInInfo = new VRSaveRoomData
                {
                    sall = false,
                    key = userSignInInfoKeyHead + mStaticThings.I.mAvatarID,
                };
                MessageDispatcher.SendMessage(this, WsMessageType.SendGetData.ToString(), signInInfo, 0);
                BaseMono.StartCoroutine(CheckSignInStatu(checkTime));
            }
        }
        internal void ShowTip(string infoStr)
        {
            WsChangeInfo wsinfo = new WsChangeInfo()
            {
                id = mStaticThings.I.mAvatarID,
                name = "InfoLog",
                a = infoStr,//要显示的Log字符串
                b = InfoColor.black.ToString(),//显示Log文字的颜色
                c = "3"//显示Log的时间
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
        }
        internal void ShowTip(string infoStr, string color)
        {
            WsChangeInfo wsinfo = new WsChangeInfo()
            {
                id = mStaticThings.I.mAvatarID,
                name = "InfoLog",
                a = infoStr,//要显示的Log字符串
                b = color,//显示Log文字的颜色
                c = "3"//显示Log的时间
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
        }

        #endregion

    }

    internal class SignInData
    {
        public string Name;
        public UserSignInStatu SignInStatu;
        public bool IsArrive;
    }
    internal class SignInConfig
    {
        //是否开启签到功能
        public bool EnableSignIn;
        public bool EnableTimeSwitch;
        public List<string> DaysOfWeek;
        public string StartTime;
        public string EndTime;
        /// <summary>
        /// 所有用户的名字
        /// </summary>
        public List<SignInAvatar> UsersList;
    }
    internal class SignInAvatar
    {
        public string ID;
        public string Name;
    }
}
