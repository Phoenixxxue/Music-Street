using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neuron;
using com.ootii.Messages;
using LitJson;
/// <summary>
/// 存储每一帧所有骨骼信息，以及角色id+角色序号的类
/// </summary>
public class TransInfoOfBones
{
    public string HandleID;
    public List<double> locX = new List<double>();
    public List<double> locY = new List<double>();
    public List<double> locZ = new List<double>();
    public List<double> rotX = new List<double>();
    public List<double> rotY = new List<double>();
    public List<double> rotZ = new List<double>();
    public List<double> rotA = new List<double>();
}
/// <summary>
/// 消息发送类，每次发送信息至vsvr时调用此类
/// </summary>
public class Sender : MonoBehaviour
{
    [Header("Sender的ID，对应AvatarEngine的ID")]
    public int id;
    public NeuronTransformsInstance NI;
    [Header("每几帧发送一次消息：")]
    public float sendTime;

    private void Awake()
    {
        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
            Destroy(this.gameObject);
    }

    private void Start()
    {
        InvokeRepeating("Send", 0, sendTime);
    }
    void BuildAndSendDic(TransInfoOfBones tb)
    {
        WsCChangeInfo newMsg = new WsCChangeInfo
        {
            a = "AvatarMotion",
            b = JsonMapper.ToJson(tb),
            c = id.ToString()
        };

        MessageDispatcher.SendMessage(this, WsMessageType.SendCChangeObj.ToString(), newMsg, 0);
    }

    private void Send()
    {
        if (NI.temp.HandleID != null)
            BuildAndSendDic(NI.temp);
    }
}
