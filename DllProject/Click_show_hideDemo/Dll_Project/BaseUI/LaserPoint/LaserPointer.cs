using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Messages;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LitJson;
using Newtonsoft.Json;
using System.Collections;

namespace Dll_Project.BaseUI.LaserPoint
{
    /// <summary>
    /// 画笔功能
    /// </summary>
    public class LaserPointer : DllGenerateBase
    {
        #region 属性或字段

        Dictionary<string, Dictionary<string, RendererItem>> avatorID_Lines = new Dictionary<string, Dictionary<string, RendererItem>>();
        /// <summary>材质 </summary>
        public Material mat;
        /// <summary>笔画ROOT </summary>
        public GameObject DrawLineRoot;
        /// <summary>是否开启画笔 </summary>
        private bool openLaserPointer = false;
        /// <summary>绘制的最大距离 </summary>
        private float maxDistance = 5f;
        /// <summary>绘制者绘画次数</summary>
        private int drawNum = 0;
        /// <summary>是否绘制结束 </summary>
        private bool drawingEnd = false;
        private float width = .05f;//单位：米
        private Color color = Color.red;
        /// <summary>画笔控制面板</summary>
        private Transform laserPanel;
        private int drawNumRecord;
        private bool isUITouch;
        /// <summary>是否是空间画笔 </summary>
        private bool isSpace = false;



        #region UI界面

        private Transform laserSwitchToggle;
        private Transform cleanBtn;
        private Transform recallBtn;
        private Transform colorBtnsRoot;
        private Transform colorPanelSwitch;
        private Transform styleSwitch;

        #endregion




        #endregion

        #region 重写方法
        public override void Init()
        {
            Debug.Log("LaserPointer Init");

            mat = BaseMono.ExtralDataObjs[0].Target as Material;

            laserPanel = BaseMono.ExtralDatas[0].Target;
            laserSwitchToggle = BaseMono.ExtralDatas[1].Target;
            cleanBtn = BaseMono.ExtralDatas[2].Target;
            recallBtn = BaseMono.ExtralDatas[3].Target;
            colorBtnsRoot = BaseMono.ExtralDatas[4].Target;
            colorPanelSwitch = BaseMono.ExtralDatas[5].Target;

            //TODO:需要时再开启 2022年8月18日17点19分
            //styleSwitch = BaseMono.ExtralDatas[6].Target;

            base.Init();
        }
        public override void Start()
        {
            if (mStaticThings.I.isVRApp)
            {
                laserSwitchToggle.gameObject.SetActive(false);
                return;
            }

            DrawLineRoot = new GameObject("LaserPointerRoot");
            laserSwitchToggle.GetComponent<Toggle>().onValueChanged.AddListener((isOn) =>
            {
                LaserControl(isOn);
            });
            colorPanelSwitch.GetComponent<Button>().onClick.AddListener(() =>
            {
                colorBtnsRoot.gameObject.SetActive(!colorBtnsRoot.gameObject.activeInHierarchy);
            });
            cleanBtn.GetComponent<Button>().onClick.AddListener(() =>
            {

                if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                {
                    WsCChangeInfo wsinfo = new WsCChangeInfo
                    {
                        a = "CleanAllLaser",//抬头
                        b = "admin",//绘制者
                    };
                    MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
                }
                else
                {
                    WsCChangeInfo wsinfo = new WsCChangeInfo
                    {
                        a = "CleanAllLaser",//抬头
                        b = mStaticThings.I.mAvatarID,//绘制者
                    };
                    MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);


                }




            });
            recallBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                WsCChangeInfo wsinfo = new WsCChangeInfo
                {
                    a = "RecallLastLaser",//抬头
                    b = mStaticThings.I.mAvatarID,//绘制者
                };
                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
            });
            for (int i = 0; i < colorBtnsRoot.childCount; i++)
            {
                Transform temp = colorBtnsRoot.GetChild(i);
                temp.GetComponent<Button>().onClick.AddListener(() => { ChangeColor(temp.name); });
            }

            //TODO: 需要时再开启 2022年8月18日17点18分
            //styleSwitch.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
            //{
            //    StyleSwitch(isOn);
            //});



            base.Start();
        }

        /// <summary>
        /// 画线模式切换
        /// </summary>
        /// <param name="isOn"></param>
        private void StyleSwitch(bool isOn)
        {
            if (isOn)//空间画笔
            {
                isSpace = true;
            }
            else//激光画笔
            {
                isSpace = false;
            }
        }

        public override void OnEnable()
        {
            MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.AddListener(VrDispMessageType.DestroyWsAvatar.ToString(), DestoryWsAvatorEvent);


            #region 注释
            //监听接收系统发来的场景消息
            //MessageDispatcher.AddListener(WsMessageType.RecieveGetData.ToString(), RecieveGetDataEvent);
            //VRSaveRoomData changeInfo = new VRSaveRoomData
            //{
            //    key = "LaserAdminPointer"
            //};
            //MessageDispatcher.SendMessage(this, WsMessageType.SendGetData.ToString(), changeInfo, 0);

            #endregion

            base.OnEnable();
        }




        public override void OnDisable()
        {
            MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
            MessageDispatcher.RemoveListener(VrDispMessageType.DestroyWsAvatar.ToString(), DestoryWsAvatorEvent);
            #region 注释
            //监听接收系统发来的场景消息
            //MessageDispatcher.RemoveListener(WsMessageType.RecieveGetData.ToString(), RecieveGetDataEvent);

            #endregion


            ClearDrawings();
            CleanCachesAndGC();
            base.OnDisable();
        }
        public override void Update()
        {

            if (mStaticThings.I.isVRApp)
            {
                return;
            }
            if (openLaserPointer)
            {
                if (mStaticThings.I.ismobile)
                {
                    GetRightFingerID();
                    // MDebug($"222 {curFingerID} ");

                    if (curFingerID >= 0)//手指触碰
                    {

                        if (isSpace)
                        {

                            int temp = 0;
                            for (int i = 0; i < Input.touchCount; i++)
                            {
                                if (Input.GetTouch(i).fingerId == curFingerID)
                                {
                                    temp = i;
                                    break;
                                }

                            }

                            //MDebug($"333 {temp} ");
                            Vector3 position = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.GetTouch(temp).position.x, Input.GetTouch(temp).position.y, maxDistance));
                            WsCChangeInfo wsinfo = new WsCChangeInfo()
                            {
                                a = "DrawLaser",//抬头
                                b = mStaticThings.I.mAvatarID,//绘制者
                                c = drawNum.ToString(),//绘制者绘画次数
                                d = position.x.ToString("F3") + "," + position.y.ToString("F3") + "," + position.z.ToString("F3"),//位置
                                e = width.ToString(),//粗细
                                f = color.ToString(),//颜色RGBA
                                g = Vector3.zero.ToString("F3"),//法线
                            };
                            MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
                            drawingEnd = true;

                        }
                        else
                        {
                            int temp = 0;
                            for (int i = 0; i < Input.touchCount; i++)
                            {
                                if (Input.GetTouch(i).fingerId == curFingerID)
                                {
                                    temp = i;
                                    break;
                                }

                            }
                            //MDebug($"333 {temp} ");
                            Vector2 m_screenPos = Input.GetTouch(temp).position;

                            Ray ray = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenPointToRay(m_screenPos);

                            if (Physics.Raycast(ray, out RaycastHit hit))
                            {
                                Vector3 position = hit.point;
                                Vector3 normal = hit.normal;
                                WsCChangeInfo wsinfo = new WsCChangeInfo()
                                {
                                    a = "DrawLaser",//抬头
                                    b = mStaticThings.I.mAvatarID,//绘制者
                                    c = drawNum.ToString(),//绘制者绘画次数
                                    d = position.x.ToString("F3") + "," + position.y.ToString("F3") + "," + position.z.ToString("F3"),//位置
                                    e = width.ToString(),//粗细
                                    f = color.ToString(),//颜色RGBA
                                    g = normal.x.ToString("F3") + "," + normal.y.ToString("F3") + "," + normal.z.ToString("F3"),//法线
                                };
                                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
                            }

                            drawingEnd = true;
                        }


                    }

                    else if (drawingEnd)//Touch绘制结束
                    {
                        drawNum++;
                        drawNumRecord = drawNum;
                        drawingEnd = false;
                        #region 注释
                        //admin或者sadmin绘制结束后上传至临时服务器；
                        //if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                        //{
                        //    string adminID = mStaticThings.I.mAvatarID;
                        //    if (avatorID_Lines.ContainsKey(adminID))
                        //    {

                        //        Dictionary<string, RendererItem> adminLine = avatorID_Lines[adminID];
                        //        List<SaveItem> saveItems = new List<SaveItem>();
                        //        foreach (var item in adminLine)
                        //        {
                        //            SaveItem saveItem = new SaveItem();
                        //            saveItem.color = item.Value.Renderer.material.GetColor("_Color").ToString();
                        //            saveItem.width = item.Value.Renderer.endWidth.ToString();
                        //            saveItem.position.AddRange(SaveItem.Vector3ToVector3Int(item.Value.Position));
                        //            saveItem.normal.AddRange(SaveItem.Vector3ToVector3Int(item.Value.Normal));

                        //            saveItems.Add(saveItem);
                        //        }

                        //        string data = JsonMapper.ToJson(saveItems);

                        //        //Debug.LogError(data);
                        //        VRSaveRoomData changeInfo = new VRSaveRoomData
                        //        {
                        //            sall = false,
                        //            key = $"LaserAdminPointer_{mStaticThings.I.mAvatarID}",
                        //            value = data
                        //        };
                        //        MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), changeInfo, 0);
                        //    }


                        //}
                        #endregion

                    }

                }
                else
                {
                    if (Input.GetMouseButton(0))
                    {
                        IsUITouch();
                        if (!isUITouch)
                        {
                            if (isSpace)
                            {
                                Vector3 position = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, maxDistance));
                                WsCChangeInfo wsinfo = new WsCChangeInfo()
                                {
                                    a = "DrawLaser",//抬头
                                    b = mStaticThings.I.mAvatarID,//绘制者
                                    c = drawNum.ToString(),//绘制者绘画次数
                                    d = position.x.ToString("F3") + "," + position.y.ToString("F3") + "," + position.z.ToString("F3"),//位置
                                    e = width.ToString(),//粗细
                                    f = color.ToString(),//颜色RGBA
                                    g = Vector3.zero.ToString("F3"),//法线
                                };
                                MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
                                drawingEnd = true;
                            }
                            else
                            {
                                Vector2 m_screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                                Ray ray = mStaticThings.I.Maincamera.GetComponent<Camera>().ScreenPointToRay(m_screenPos);

                                if (Physics.Raycast(ray, out RaycastHit hit))
                                {
                                    Vector3 position = hit.point;
                                    Vector3 normal = hit.normal;
                                    WsCChangeInfo wsinfo = new WsCChangeInfo()
                                    {
                                        a = "DrawLaser",//抬头
                                        b = mStaticThings.I.mAvatarID,//绘制者
                                        c = drawNum.ToString(),//绘制者绘画次数
                                        d = position.x.ToString("F3") + "," + position.y.ToString("F3") + "," + position.z.ToString("F3"),//位置
                                        e = width.ToString(),//粗细
                                        f = color.ToString(),//颜色RGBA
                                        g = normal.x.ToString("F3") + "," + normal.y.ToString("F3") + "," + normal.z.ToString("F3"),//法线
                                    };
                                    MessageDispatcher.SendMessage("", WsMessageType.SendCChangeObj.ToString(), wsinfo, 0f);
                                }

                                drawingEnd = true;
                            }
                        }

                    }
                    else if (drawingEnd)//Mouse绘制结束
                    {
                        drawNum++;
                        drawNumRecord = drawNum;
                        drawingEnd = false;
                        #region 注释
                        //admin或者sadmin绘制结束后上传至临时服务器；
                        //if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                        //{
                        //    string adminID = mStaticThings.I.mAvatarID;
                        //    if (avatorID_Lines.ContainsKey(adminID))
                        //    {

                        //        Dictionary<string, RendererItem> adminLine = avatorID_Lines[adminID];
                        //        List<SaveItem> saveItems = new List<SaveItem>();
                        //        foreach (var item in adminLine)
                        //        {
                        //            SaveItem saveItem = new SaveItem();
                        //            saveItem.color = item.Value.Renderer.material.GetColor("_Color").ToString();
                        //            saveItem.width = item.Value.Renderer.endWidth.ToString();
                        //            saveItem.position.AddRange(SaveItem.Vector3ToVector3Int(item.Value.Position));
                        //            saveItem.normal.AddRange(SaveItem.Vector3ToVector3Int(item.Value.Normal));

                        //            saveItems.Add(saveItem);
                        //        }

                        //        string data = JsonMapper.ToJson(saveItems);

                        //        //Debug.LogError(data);
                        //        VRSaveRoomData changeInfo = new VRSaveRoomData
                        //        {
                        //            sall = false,
                        //            key = $"LaserAdminPointer_{mStaticThings.I.mAvatarID}",
                        //            value = data
                        //        };
                        //        MessageDispatcher.SendMessage(this, WsMessageType.SendSaveData.ToString(), changeInfo, 0);
                        //    }


                        //}
                        #endregion
                    }
                }
            }

            base.Update();
        }
        public override void OnDestroy()
        {
            OnDisable();
            ClearDrawings();
            base.OnDestroy();
        }
        #endregion

        #region 监听方法


        #region 注释
        private void RecieveGetDataEvent(IMessage msg)
        {
            if (msg == null || msg.Data == null) { return; }
            Dictionary<string, string> dic = msg.Data as Dictionary<string, string>;
            if (dic == null) { return; }
            if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin) { return; }

            foreach (var item in dic)
            {


                if (item.Key.Contains("LaserAdminPointer"))
                {
                    string adminID = item.Key.Split('_')[1];
                    Debug.LogError("接收到的数据： " + item.Value);

                    List<SaveItem> saveItems = JsonMapper.ToObject<List<SaveItem>>(item.Value);



                    GameObject parent = new GameObject(adminID);
                    if (DrawLineRoot.transform.Find(parent.name) == null)
                    {
                        parent.transform.parent = DrawLineRoot.transform;
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(parent);
                    }

                    //Dictionary<string, RendererItem> newLinesDic = new Dictionary<string, RendererItem>();
                    for (int i = 0; i < saveItems.Count; i++)
                    {
                        GameObject obj = new GameObject(i.ToString());
                        obj.transform.SetParent(parent.transform != null ? parent.transform : DrawLineRoot.transform.Find(parent.name));

                        LineRenderer renderer = obj.AddComponent<LineRenderer>();
                        renderer.material = mat;
                        renderer.startWidth = renderer.endWidth = float.Parse(saveItems[i].width);
                        renderer.material.SetColor("_Color", StringToColor(saveItems[i].color));

                        List<Vector3> vector3s = new List<Vector3>();
                        for (int j = 0; j < saveItems[i].position.Count; j++)
                        {
                            vector3s.Add(SaveItem.Vector3IntToVector3(saveItems[i].position[j]) + 0.02f * SaveItem.Vector3IntToVector3(saveItems[i].normal[j]));
                        }
                        Vector3[] vector3Array = vector3s.ToArray();
                        //renderer.positionCount = vector3Array.Length;
                        //renderer.SetPositions(vector3Array);

                        // for (int k = 0; k < vector3s.Count; k++)
                        // {
                        // renderer.positionCount++;
                        // renderer.SetPosition(k, vector3s[k]);
                        // }

                        //BaseMono.StartCoroutine(DelayTimeToDraw(renderer, vector3s.ToArray()));


                        //RendererItem newItem = new RendererItem();

                        //if (avatorID_Lines.ContainsKey(adminID))
                        //{
                        //    avatorID_Lines[adminID] = newLinesDic;
                        //}
                        //else
                        //{
                        //    avatorID_Lines.Add(adminID, newLinesDic);
                        //}

                        //if(!newLinesDic.ContainsKey(obj.name))
                        //{
                        //    newLinesDic.Add(obj.name, newItem);
                        //}

                    }





                }
            }
        }

        IEnumerator DelayTimeToDraw(LineRenderer renderer, Vector3[] vector3s, float time = 3F)
        {
            yield return new WaitForSeconds(time);
            renderer.positionCount = vector3s.Length;
            renderer.SetPositions(vector3s);

        }

        #endregion


        /// <summary>
        /// 人物角色销毁事件
        /// </summary>
        /// <param name="rMessage"></param>
        private void DestoryWsAvatorEvent(IMessage rMessage)
        {
            string leaverID = rMessage.Data as string;
            if (leaverID == null)
            {
                return;
            }
            if (avatorID_Lines.ContainsKey(leaverID))
            {
                avatorID_Lines.Remove(leaverID);
            }

            if (DrawLineRoot != null)
            {
                Transform avator = DrawLineRoot.transform.Find(leaverID);
                if (avator != null)
                {
                    UnityEngine.Object.Destroy(avator.gameObject);
                }
            }


        }

        /// <summary>
        /// <param name="msg">
        /// a=抬头
        /// b=绘制者
        /// c=绘制者画笔次数
        /// d=位置
        /// e=画笔粗细
        /// f=颜色
        /// g=法线
        /// </param>
        /// </summary>
        private void RecieveCChangeObj(IMessage msg)
        {
            if (msg == null || msg.Data == null) return;
            WsCChangeInfo info = msg.Data as WsCChangeInfo;
            //if (mStaticThings.I != null)
            {

                if (info.a == "DrawLaser")
                {
                    //MDebug(info.c);
                    if (avatorID_Lines.ContainsKey(info.b))//有此人
                    {
                        if (avatorID_Lines[info.b].ContainsKey(info.c))//有此人的这个笔画
                        {
                            avatorID_Lines[info.b][info.c].Position.Add(StringToVector3(info.d));
                            avatorID_Lines[info.b][info.c].Normal.Add(StringToVector3(info.g).normalized);
                            //Draw
                            Draw(avatorID_Lines[info.b][info.c], StringToColor(info.f), float.Parse(info.e));
                        }
                        else//没有此人的这个笔画
                        {

                            GameObject obj = new GameObject(info.c);
                            obj.transform.parent = DrawLineRoot.transform.Find(info.b);
                            RendererItem newItem = new RendererItem();
                            newItem.Renderer = obj.AddComponent<LineRenderer>();
                            newItem.Renderer.positionCount = 0;
                            newItem.Position.Add(StringToVector3(info.d));
                            newItem.Normal.Add(StringToVector3(info.g).normalized);
                            avatorID_Lines[info.b].Add(info.c, newItem);
                            //Draw
                            Draw(avatorID_Lines[info.b][info.c], StringToColor(info.f), float.Parse(info.e));
                        }
                    }
                    else//没有此人=>绘制第一个点
                    {
                        if (DrawLineRoot == null)
                        {
                            return;
                        }

                        GameObject go = new GameObject(info.b);



                        go.transform.parent = DrawLineRoot.transform;
                        GameObject obj = new GameObject(info.c);
                        obj.transform.parent = DrawLineRoot.transform.Find(info.b);
                        Dictionary<string, RendererItem> newLinesDic = new Dictionary<string, RendererItem>();
                        RendererItem newItem = new RendererItem();
                        newItem.Renderer = obj.AddComponent<LineRenderer>();
                        newItem.Renderer.positionCount = 0;
                        newItem.Position.Add(StringToVector3(info.d));
                        newItem.Normal.Add(StringToVector3(info.g).normalized);

                        newLinesDic.Add(info.c, newItem);
                        avatorID_Lines.Add(info.b, newLinesDic);
                        //Draw
                        Draw(avatorID_Lines[info.b][info.c], StringToColor(info.f), float.Parse(info.e));
                    }
                }








                if (info.a == "CleanAllLaser")
                {

                    ClearDrawings(info.b);//清空
                }

                if (info.a == "RecallLastLaser")
                {
                    RecallDrawing(info.b);//撤回
                }

            }
        }
        /// <summary>
        /// 清空画笔
        /// </summary>
        /// <param name="avatorID"></param>
        private void ClearDrawings(string avatorID)
        {
            if (avatorID == null)
            {
                return;
            }
            if (avatorID == "admin")
            {
                if (DrawLineRoot != null)
                {
                    for (int i = 0; i < DrawLineRoot.transform.childCount; i++)
                    {
                        Transform child = DrawLineRoot.transform.GetChild(i);
                        for (int j = 0; j < child.childCount; j++)
                        {
                            UnityEngine.Object.Destroy(child.transform.GetChild(j).gameObject);
                        }

                    }
                }


                if (avatorID_Lines != null)
                {
                    avatorID_Lines.Clear();
                }


            }
            else
            {
                Transform avator = DrawLineRoot.transform.Find(avatorID);
                if (avator != null)
                {
                    UnityEngine.Object.Destroy(avator.gameObject);
                }
                if (avatorID_Lines != null)
                {
                    if (avatorID_Lines.ContainsKey(avatorID))
                    {
                        avatorID_Lines.Remove(avatorID);
                    }
                }
            }


            Resources.UnloadUnusedAssets();
        }



        /// <summary>
        /// 撤回上一笔
        /// </summary>
        /// <param name="avatorID"></param>
        private void RecallDrawing(string avatorID)
        {
            if (DrawLineRoot != null && DrawLineRoot.transform.Find(avatorID) != null && DrawLineRoot.transform.Find(avatorID).childCount > 0)
            {
                Transform newestDraw = DrawLineRoot.transform.Find(avatorID).GetChild(DrawLineRoot.transform.Find(avatorID).childCount - 1);
                UnityEngine.Object.Destroy(newestDraw.gameObject);

                if (avatorID_Lines != null)
                {
                    if (avatorID_Lines.ContainsKey(avatorID))
                    {
                        if (avatorID_Lines[avatorID].ContainsKey(newestDraw.name))
                        {
                            avatorID_Lines[avatorID].Remove(newestDraw.name);
                        }
                    }

                }


            }




        }
        /// <summary>
        /// 颜色改变
        /// </summary>
        /// <param name="btnName"></param>
        private void ChangeColor(string btnName)
        {
            switch (btnName)
            {
                case "黑色":
                    color = Color.black;
                    break;
                case "白色":
                    color = Color.white;
                    break;
                case "橙色":
                    color = new Color(1f, 128f / 255f, 0f, 1f);
                    break;
                case "黄色":
                    color = Color.yellow;
                    break;
                case "绿色":
                    color = Color.green;
                    break;
                case "青色":
                    color = Color.cyan;
                    break;
                case "蓝色":
                    color = Color.blue;
                    break;
                case "紫色":
                    color = new Color(160f / 255f, 32f / 255f, 240f / 255f, 1f);
                    break;
                default:
                case "红色":
                    color = Color.red;
                    break;
            }
        }
        /// <summary>
        /// 画笔开关
        /// </summary>
        /// <param name="isOn"></param>
        private void LaserControl(bool isOn)
        {
            if (laserPanel != null)
            {
                laserPanel.gameObject.SetActive(isOn);
                mStaticData.IsOpenPointClick = !isOn;
                Debug.Log(mStaticData.IsOpenPointClick);
            }
            if (colorBtnsRoot != null)
            {
                colorBtnsRoot.gameObject.SetActive(false);
            }
            openLaserPointer = isOn;
        }

        #endregion

        #region 其他
        private void CleanCachesAndGC()
        {
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        /// <summary>
        /// 清除笔画
        /// </summary>
        public void ClearDrawings()
        {

            if (DrawLineRoot != null)
            {
                for (int i = 0; i < DrawLineRoot.transform.childCount; i++)
                {
                    UnityEngine.Object.Destroy(DrawLineRoot.transform.GetChild(i).gameObject);

                }
            }
            if (avatorID_Lines != null)
            {
                avatorID_Lines.Clear();
            }
            Resources.UnloadUnusedAssets();
        }
        /// <summary>
        /// 判断手指第一次触摸的地方是否有UI
        /// </summary>
        private void IsUITouch()
        {

            if (Input.GetMouseButtonDown(0))
            {
                isUITouch = EventSystem.current.IsPointerOverGameObject();
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    // Debug.Log("EventSystem.current.currentSelectedGameObject" + EventSystem.current.currentSelectedGameObject.name);

                    Canvas canvas = FindCanvas(EventSystem.current.currentSelectedGameObject.transform);
                    if (canvas != null)
                    {
                        // Debug.Log("画布不为空");
                        if (canvas.renderMode == RenderMode.WorldSpace)
                        {
                            isUITouch = false;
                            Debug.Log("world space isUITouch: " + isUITouch);
                        }
                    }
                    else
                    {
                        // Debug.Log("画布为空");
                    }

                }
                else
                {
                    isUITouch = false;
                    //Debug.Log("EventSystem.current.currentSelectedGameObject is NULL");
                }
                //Debug.Log("IsUITouch: " + isUITouch);

            }
        }



        /// <summary>
        /// 当前fingerID
        /// </summary>
        private int curFingerID = -1;
        /// <summary>
        /// 获取争取的FingerID
        /// </summary>
        private void GetRightFingerID()
        {
            if (Input.touchCount == 0)
            {
                curFingerID = -1;
            }
            else
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                        {
                            if (curFingerID < 0)
                            {
                                curFingerID = Input.GetTouch(i).fingerId;
                            }
                        }
                    }

                    if (Input.GetTouch(i).fingerId == curFingerID)
                    {
                        if (Input.GetTouch(i).phase == TouchPhase.Ended)
                        {
                            curFingerID = -1;
                        }
                    }
                }



            }



        }


        /// <summary>
        /// Find Canvas In Parent
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Canvas FindCanvas(Transform obj)
        {
            if (obj.GetComponent<Canvas>() != null)
            {
                return obj.GetComponent<Canvas>();
            }
            else
            {
                if (obj.transform.parent != null)
                {
                    return FindCanvas(obj.transform.parent);
                }
                else
                {
                    return null;
                }

            }
        }

        private Vector3 StringToVector3(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] temp = str.Split(',');

                if (temp.Length == 3)
                {
                    Vector3 v3 = new Vector3(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]));
                    return v3;
                }
                else
                {
                    return Vector3.zero;
                }
            }
            else
            {
                return Vector3.zero;
            }
        }

        private Color StringToColor(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Replace("RGBA(", "").Replace(")", "");
                string[] temp = str.Split(',');
                if (temp.Length == 4)
                {
                    Color color = new Color(float.Parse(temp[0]), float.Parse(temp[1]), float.Parse(temp[2]), float.Parse(temp[3]));
                    return color;
                }
                else
                {
                    return Color.white;
                }
            }
            else
            {
                return Color.white;
            }

        }

        /// <summary>
        /// 绘制
        /// 注：如果有人绘画，有人同时加载场景，不接收别人正在绘画的点位，否则报错
        /// </summary>
        public void Draw(RendererItem item, Color color, float width)
        {


            if (item != null && item.Renderer != null && item.Renderer.material != null)
            {
                item.Renderer.material = mat;
                item.Renderer.startWidth = item.Renderer.endWidth = width;
                item.Renderer.material.SetColor("_Color", color);
                //item.Renderer.numCornerVertices = 10;//转角圆滑程度

                //画点
                if (item.Position != null)
                {
                    item.Renderer.positionCount++;
                    if (item.Renderer.positionCount == item.Position.Count)
                    {
                        item.Renderer.SetPosition(item.Position.Count - 1, item.Position[item.Position.Count - 1] + .02f * item.Normal[item.Normal.Count - 1]);
                    }
                }

            }
        }
        public void MDebug(string msg, int level = 0)
        {
            if (level == 0)
            {
                if (mStaticThings.I == null)
                {
                    Debug.Log(msg);
                    return;
                }
                WsChangeInfo wsinfo = new WsChangeInfo()
                {
                    id = mStaticThings.I.mAvatarID,
                    name = "InfoLog",
                    a = msg,
                    b = InfoColor.black.ToString(),
                    c = "3",
                };
                MessageDispatcher.SendMessage(this, VrDispMessageType.SendInfolog.ToString(), wsinfo, 0);
            }
        }
        #endregion





        public Vector3 Vector3IntToVector3(Vector3Int vector3Int)
        {
            return new Vector3(vector3Int.x, vector3Int.y, vector3Int.z);
        }
        public List<Vector3> Vector3IntToVector3(List<Vector3Int> vector3Ints)
        {
            List<Vector3> vector3s = new List<Vector3>();
            for (int i = 0; i < vector3Ints.Count; i++)
            {
                Vector3 vector3 = Vector3IntToVector3(vector3Ints[i]);
                vector3s.Add(vector3);

            }
            return vector3s;
        }


        public Vector3Int Vector3ToVector3Int(Vector3 vector3)
        {
            return new Vector3Int((int)vector3.x, (int)vector3.y, (int)vector3.z);
        }


        public List<Vector3Int> Vector3ToVector3Int(List<Vector3> vector3s)
        {
            List<Vector3Int> vector3Ints = new List<Vector3Int>();
            for (int i = 0; i < vector3s.Count; i++)
            {
                Vector3Int vector3Int = Vector3ToVector3Int(vector3Ints[i]);
                vector3Ints.Add(vector3Int);

            }
            return vector3Ints;
        }






    }

    /// <summary>
    /// 画笔对象
    /// </summary>
    public class RendererItem
    {
        public LineRenderer Renderer;

        public List<Vector3> Position = new List<Vector3>();

        public List<Vector3> Normal = new List<Vector3>();
    }

    public class SaveItem
    {
        public string color;

        public string width;

        public List<MyVector3Int> position = new List<MyVector3Int>();

        public List<MyVector3Int> normal = new List<MyVector3Int>();


        public struct MyVector3Int
        {
            public int x;
            public int y;
            public int z;

        }

        public static Vector3 Vector3IntToVector3(MyVector3Int vector3Int)
        {
            return new Vector3(vector3Int.x, vector3Int.y, vector3Int.z);
        }
        public static List<Vector3> Vector3IntToVector3(List<MyVector3Int> vector3Ints)
        {
            List<Vector3> vector3s = new List<Vector3>();
            for (int i = 0; i < vector3Ints.Count; i++)
            {
                Vector3 vector3 = Vector3IntToVector3(vector3Ints[i]);
                vector3s.Add(vector3);

            }
            return vector3s;
        }


        public static MyVector3Int Vector3ToVector3Int(Vector3 vector3)
        {
            MyVector3Int vectorInt = new MyVector3Int
            {
                x = (int)vector3.x,
                y = (int)vector3.y,
                z = (int)vector3.z

            };


            return vectorInt;
        }


        public static List<MyVector3Int> Vector3ToVector3Int(List<Vector3> vector3s)
        {
            List<MyVector3Int> vector3Ints = new List<MyVector3Int>();
            for (int i = 0; i < vector3s.Count; i++)
            {
                MyVector3Int vector3Int = Vector3ToVector3Int(vector3s[i]);
                vector3Ints.Add(vector3Int);

            }
            return vector3Ints;
        }



        public class Vector3Double
        {
            public double x;
            public double y;
            public double z;

        }

        public static List<Vector3Double> Vector3ToVector3Double(List<Vector3> vector3s)
        {
            List<Vector3Double> v3Doubles = new List<Vector3Double>();

            for (int i = 0; i < vector3s.Count; i++)
            {
                Vector3Double v3d = new Vector3Double
                {
                    x = vector3s[i].x,
                    y = vector3s[i].y,
                    z = vector3s[i].z,
                };

                v3Doubles.Add(v3d);
            }

            return v3Doubles;

        }

        public static List<Vector3> Vector3DoubleToVector3(List<Vector3Double> vector3Doubles)
        {
            List<Vector3> vector3s = new List<Vector3>();
            for (int i = 0; i < vector3Doubles.Count; i++)
            {
                Vector3 v3 = new Vector3((float)vector3Doubles[i].x, (float)vector3Doubles[i].y, (float)vector3Doubles[i].z);
            }

            return vector3s;



        }
        public static Vector3 Vector3DoubleToVector3(Vector3Double vector3Double)
        {
            return new Vector3((float)vector3Double.x, (float)vector3Double.y, (float)vector3Double.z);

        }

        public static Vector3Double Vector3ToVector3Double(Vector3 vector3)
        {

            Vector3Double v3d = new Vector3Double
            {
                x = vector3.x,
                y = vector3.y,
                z = vector3.z,
            };
            return v3d;


        }
    }
}

