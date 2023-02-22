using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dll_Project.BaseFunction.SitDown
{
    /// <summary>
    /// Chair类
    /// </summary>
    public class Chair
    {/// <summary>
     /// 椅子名字
     /// </summary>
        public string ChairName;
        /// <summary>
        /// 椅子位置
        /// </summary>
        public Transform ChairTransform;
        /// <summary>
        /// 椅子状态是否有人入座
        /// </summary>
        public bool ChairState;
        /// <summary>
        /// 椅子入座状态 true可入座 false不可入座
        /// </summary>
        public bool ChairSitState;
        /// <summary>
        /// 椅子主任AVATARid
        /// </summary>
        public string ChairMaster;
        /// <summary>
        /// 椅子朝向
        /// </summary>
        public Transform ChairDir;
        /// <summary>
        /// 椅子提示UI
        /// </summary>
        public GameObject ChairTipPanel;
    }
    public class ChairIndex
    {
        /// <summary>
        /// 开始索引
        /// </summary>
        public int StartIndex = 0;
        /// <summary>
        /// 结束索引
        /// </summary>
        public int EndIndex = 0;
        public ChairIndex(int x, int y)
        {
            StartIndex = x;
            EndIndex = y;
        }
    }
    /// <summary>
    /// 用户点击座椅入座：适用于大型场景多椅子的情况，且椅子能够分区域
    /// </summary>
    public class PlayerSitDown : DllGenerateBase
    {
        /// <summary>
        /// 全场椅子分为三个区域
        /// </summary>
        ChairIndex[] chairIndexes = new ChairIndex[4];
        /// <summary>
        /// 当前用户所在位置对应的区域，和场景中设置的区域名称有关
        /// </summary>
        ChairIndex nowindex;
        /// <summary>
        /// 全场所有椅子
        /// </summary>
        List<Chair> chairs = new List<Chair>();
        /// <summary>
        /// 本人坐下的椅子的索引
        /// </summary>
        public int sitChairID = -1;
        private Transform ChairParnet;//BaseMono
        private string Order_ChangeChairState = "ChangeChairState";
        private string chairRegionName = "chairregion";
        private float dis = 2;
        private int minChairIndex = -1;
        private Vector3 oldUserPos_dis;
        private Vector3 oldUserPos_leave;

        public override void Init()
        {
            ChairParnet = BaseMono.ExtralDatas[0].Target;
        }
        public override void Start()
        {
            chairIndexes[0] = new ChairIndex(0, 1);//chairIndexes[0] 其中0要与场景中地面的名字索引相同
            chairIndexes[1] = new ChairIndex(2, 3);
            chairIndexes[2] = new ChairIndex(4, 5);
            chairIndexes[3] = new ChairIndex(6, 9);

            InitChairs();

            MessageDispatcher.SendMessage(WsMessageType.SendGetData.ToString(), 0f);

            chairs[0].ChairSitState = true;
            chairs[0].ChairTipPanel.SetActive(true);
            chairs[0].ChairTransform.GetComponent<BoxCollider>().enabled = true;
        }
        public override void Update()
        {
            CheckDistanceOfChair();
            ChairCheckLeave();
            if (Time.frameCount % 30 == 0)
            {
                CheckChairMaster();
            }
            base.Update();
        }
        public override void OnEnable()
        {
            //连接房间--请求数据--接收数据--点击事件/传送事件/接收数据--离开房间

            MessageDispatcher.AddListener(WsMessageType.RecieveGetData.ToString(), RecieveGetDataEvent);
            MessageDispatcher.AddListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.AddListener(WsMessageType.RecieveCheckAvatar.ToString(), RecieveCheckAvatar);
            MessageDispatcher.AddListener(WsMessageType.RecieveConnected.ToString(), RoomConnectin);
            MessageDispatcher.AddListener(VrDispMessageType.RoomConnected.ToString(), RoomConnectin);
            MessageDispatcher.AddListener(VrDispMessageType.RoomDisConnected.ToString(), DisRoomConnectin);
            MessageDispatcher.AddListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshHandler);
        }
        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveGetData.ToString(), RecieveGetDataEvent);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCheckAvatar.ToString(), RecieveCheckAvatar);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveConnected.ToString(), RoomConnectin);
            MessageDispatcher.RemoveListener(VrDispMessageType.RoomConnected.ToString(), RoomConnectin);
            MessageDispatcher.RemoveListener(VrDispMessageType.RoomDisConnected.ToString(), DisRoomConnectin);
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshHandler);

            base.OnDisable();
        }
        public override void OnDestroy()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveGetData.ToString(), RecieveGetDataEvent);
            MessageDispatcher.RemoveListener(VRPointObjEventType.VRPointClick.ToString(), OnPointClickEvent);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCheckAvatar.ToString(), RecieveCheckAvatar);
            MessageDispatcher.RemoveListener(WsMessageType.RecieveConnected.ToString(), RoomConnectin);
            MessageDispatcher.RemoveListener(VrDispMessageType.RoomConnected.ToString(), RoomConnectin);
            MessageDispatcher.RemoveListener(VrDispMessageType.RoomDisConnected.ToString(), DisRoomConnectin);
            MessageDispatcher.RemoveListener(VrDispMessageType.TelePortToMesh.ToString(), TelePortToMeshHandler);

        }

        private void RecieveGetDataEvent(IMessage rMessage)
        {
            Dictionary<string, string> dic = rMessage.Data as Dictionary<string, string>;
            if (dic == null)
                return;
            ReceiveSystemData(dic);
        }
        private void OnPointClickEvent(IMessage rMessage)
        {
            if (rMessage == null) return;
            GameObject obj = rMessage.Data as GameObject;
            //Debug.LogError("obj   " + obj.name);
            if (obj == null)
                return;
            for (int i = 0; i < chairs.Count; i++)
            {//点击物体名为椅子名并且椅子状态为可入座状态
                if (obj.transform == chairs[i].ChairTransform && chairs[i].ChairSitState)
                {

                    chairs[i].ChairSitState = false;
                    //设置坐下ID
                    sitChairID = i;
                    SitDownControl(chairs[i]);
                    //Debug.Log("sitChairID"+i);
                    break;
                }
            }
        }
        private void RecieveCChangeObj(IMessage rMessage)
        {
            if (rMessage == null)
                return;
            WsCChangeInfo rinfo = rMessage.Data as WsCChangeInfo;
            if (rinfo == null)
                return;
            if (rinfo.a == Order_ChangeChairState)
            {
                int chairId = int.Parse(rinfo.b);

                //接收到的信息为0或空的话设座位状态为无人  否则将座位状态设为有人且座位主人为""
                if (!String.IsNullOrEmpty(rinfo.c))
                {
                    if (rinfo.c == "0")
                    {
                        chairs[chairId].ChairState = false;
                        chairs[chairId].ChairMaster = "";
                        chairs[chairId].ChairTransform.GetComponent<BoxCollider>().enabled = true;
                    }
                    else
                    {
                        chairs[chairId].ChairState = true;
                        chairs[chairId].ChairMaster = rinfo.c;
                        chairs[chairId].ChairTransform.GetComponent<BoxCollider>().enabled = false;
                    }
                }
                MessageDispatcher.SendMessage(WsMessageType.SendGetData.ToString(), 0f);
            }
        }
        private void RecieveCheckAvatar(IMessage rMessage)
        {
            ConnectAvatars connectAvatars = rMessage.Data as ConnectAvatars;
            if (connectAvatars == null)
                return;
            ReceiveSystemData(connectAvatars.chdata);
        }
        private void RoomConnectin(IMessage rMessage)
        {
            MessageDispatcher.SendMessage(WsMessageType.SendGetData.ToString(), 0f);
        }
        private void DisRoomConnectin(IMessage rMessage)
        {
            if (sitChairID < 0)
                return;
            //发送椅子离开事件
            LeaveChair();
        }
        private void TelePortToMeshHandler(IMessage msg)
        {
            string name = msg.Data.ToString();

            if (name.Contains(chairRegionName))
            {
                nowindex = chairIndexes[int.Parse(name.Replace(chairRegionName, ""))];
               // Debug.Log("测试：" + nowindex.StartIndex + " " + nowindex.EndIndex);
            }
            else
            {
                for (int i = 0; i < chairs.Count; i++)
                {
                    chairs[i].ChairTipPanel.SetActive(false);
                    chairs[i].ChairSitState = false;
                    chairs[i].ChairTransform.GetComponent<BoxCollider>().enabled = false;
                }
                nowindex = null;
            }
        }

        private void InitChairs()
        {
            chairs.Clear();
            for (int i = 0; i < ChairParnet.childCount; i++)
            {
                Chair _Chair = new Chair()
                {
                    ChairName = ChairParnet.GetChild(i).name,
                    ChairTransform = ChairParnet.GetChild(i).transform,
                    ChairState = false,
                    ChairMaster = "",
                    ChairDir = ChairParnet.GetChild(i).GetChild(0),
                    ChairTipPanel = ChairParnet.GetChild(i).GetChild(1).gameObject,
                    ChairSitState = true,
                    // UIpanel = UIParnel.GetChild(i).gameObject,
                };

                chairs.Add(_Chair);
            }
        }
        /// <summary>
        /// 接收到场景中存储的座位入座信息
        /// </summary>
        /// <param name="dic"></param>
        private void ReceiveSystemData(Dictionary<string, string> dic)
        {
            if (dic == null)
            {
                return;
            }
            foreach (var item in dic)
            {
                if (item.Key.Contains("Chair_"))
                {
                    int chairId = int.Parse(item.Key.Substring(item.Key.LastIndexOf('_') + 1));

                    //接收到的信息为0或空的话设座位状态为无人  否则将座位状态设为有人且座位主人为""
                    if (!String.IsNullOrEmpty(item.Value))
                    {
                        if (item.Value == "0")
                        {
                            chairs[chairId].ChairState = false;
                            chairs[chairId].ChairMaster = "";
                        }
                        else
                        {
                            chairs[chairId].ChairState = true;
                            chairs[chairId].ChairMaster = item.Value;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 入座
        /// </summary>
        /// <param name="chair"></param>
        public void SitDownControl(Chair chair)
        {
            //VR头显没有入座动画，暂时用移位置代替
            if (mStaticThings.I.isVRApp)
            {
                mStaticThings.I.MainVRROOT.position = chair.ChairDir.position;
                mStaticThings.I.MainVRROOT.localRotation = chair.ChairDir.rotation;
            }
            //发消息给场景：我入座了
            VRSaveRoomData changeInfo = new VRSaveRoomData
            {
                sall = false,
                key = "Chair_" + sitChairID,
                value = mStaticThings.I.mAvatarID
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), changeInfo, 0);
            //获取全场入座状态
            MessageDispatcher.SendMessage(WsMessageType.SendGetData.ToString(), 0.5f);
            //发消息给全场所有人：我入座了
            WsCChangeInfo wsinfo1 = new WsCChangeInfo()
            {
                a = "ChangeChairState",
                b = sitChairID.ToString(),
                c = mStaticThings.I.mAvatarID
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0);
        }
        /// <summary>
        /// 离座
        /// </summary>
        private void LeaveChair()
        {
            chairs[sitChairID].ChairSitState = true;
            VRSaveRoomData changeInfo = new VRSaveRoomData
            {
                sall = false,
                key = "Chair_" + sitChairID,
                value = "0"
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), changeInfo, 0);
            MessageDispatcher.SendMessage(WsMessageType.SendGetData.ToString(), 0f);
            WsCChangeInfo wsinfo1 = new WsCChangeInfo()
            {
                a = "ChangeChairState",
                b = sitChairID.ToString(),
                c = "0"
            };
            MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0);
            MessageDispatcher.SendMessage("SelfStandUpFromChair");
            //设坐下椅子ID为-1表示自己未入座
            sitChairID = -1;
        }
        /// <summary>
        /// 获取当前帧 距离人物最近的椅子
        /// </summary>
        public void CheckDistanceOfChair()
        {
            if (nowindex == null || nowindex.EndIndex == 0)
                return;

            if (sitChairID < 0)
            {
                if (oldUserPos_dis != mStaticThings.I.MainVRROOT.position)
                {
                    dis = 2;
                    minChairIndex = -1;
                    for (int i = nowindex.StartIndex; i <= nowindex.EndIndex; i++)
                    {
                        //当当前椅子的状态为false时
                        if (!chairs[i].ChairState)
                        {
                            float temp = Vector2.Distance(mStaticThings.I != null ? new Vector2(mStaticThings.I.MainVRROOT.position.x, mStaticThings.I.MainVRROOT.position.z) : Vector2.zero, new Vector2(chairs[i].ChairTransform.position.x, chairs[i].ChairTransform.position.z));
                            if (temp < dis)
                            {
                                dis = temp;
                                minChairIndex = i;
                            }
                        }
                    }
                    //Debug.Log("测试：CheckDistanceOfChair" + dis + " " + minChairIndex);
                    oldUserPos_dis = mStaticThings.I.MainVRROOT.position;
                }
              //  Debug.Log("测试：minChairIndex" + "=" + minChairIndex);
                //显示提示UI判断椅子是否可入座
                if (minChairIndex == -1)
                {
                    for (int i = 0; i < chairs.Count; i++)
                    {
                        chairs[i].ChairSitState = false;
                        chairs[i].ChairTipPanel.SetActive(false);
                        chairs[i].ChairTransform.GetComponent<BoxCollider>().enabled = false;
                    }
                }
                else
                {
                    for (int i = nowindex.StartIndex; i <= nowindex.EndIndex; i++)
                    {
                        if (i == minChairIndex)
                        {
                            chairs[i].ChairSitState = true;
                            chairs[i].ChairTipPanel.SetActive(true);
                            chairs[i].ChairTransform.GetComponent<BoxCollider>().enabled = true;
                           // Debug.Log("测试：设置椅子" + i);
                        }
                        else
                        {
                            //恢复椅子状态
                            chairs[i].ChairTipPanel.SetActive(false);
                            chairs[i].ChairSitState = false;
                            chairs[i].ChairTransform.GetComponent<BoxCollider>().enabled = false;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 检测自己离开椅子
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void ChairCheckLeave()
        {
            if (sitChairID < 0)
                return;
            if (oldUserPos_leave != mStaticThings.I.MainVRROOT.position)
            {
                if (Vector2.Distance(mStaticThings.I != null ? new Vector2(mStaticThings.I.MainVRROOT.position.x, mStaticThings.I.MainVRROOT.position.z) : Vector2.zero, new Vector2(chairs[sitChairID].ChairDir.position.x, chairs[sitChairID].ChairDir.position.z)) > 0.5f)
                {
                    LeaveChair();
                }
             //   Debug.Log("测试：ChairCheckLeave");
                oldUserPos_leave = mStaticThings.I.MainVRROOT.position;
            }

        }
        /// <summary>
        /// 检查椅子主人是否存在
        /// </summary>
        private void CheckChairMaster()
        {
            if (mStaticThings.I == null)
                return;
            //当前是否站在某个椅子区域
            if (nowindex == null)
                return;
            if (mStaticThings.I.GetAllStaticAvatarList().Count < 0)
                return;
            if (mStaticThings.I.mAvatarID != mStaticThings.I.GetAllStaticAvatarList()[0])
                return;
            for (int i = nowindex.StartIndex; i <= nowindex.EndIndex; i++)
            {
                if (String.IsNullOrEmpty(chairs[i].ChairMaster))
                {
                    continue;
                }
                else
                {
                    if (mStaticThings.I.GetAllStaticAvatarList().Contains(chairs[i].ChairMaster))
                    {
                        continue;
                    }
                    else
                    {
                        VRSaveRoomData changeInfo = new VRSaveRoomData
                        {
                            sall = false,
                            key = "Chair_" + i,
                            value = "0",
                        };
                        MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), changeInfo, 0);
                        WsCChangeInfo wsinfo1 = new WsCChangeInfo()
                        {
                            a = "ChangeChairState",
                            b = i.ToString(),
                            c = "0"
                        };
                        MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), wsinfo1, 0);
                    }
                }
            }
        }
    }
}
