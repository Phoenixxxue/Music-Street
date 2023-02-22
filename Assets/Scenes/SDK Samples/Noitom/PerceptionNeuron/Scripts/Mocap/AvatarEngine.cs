using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Messages;
using LitJson;

public class AvatarEngine : MonoBehaviour
{
    [Header("每几帧更新一次骨骼信息")]
    public int Frame;
    [Header("是否使用默认的骨骼排序")]
    public bool flag;
    [Header("是否捕捉了手指")]
    public bool mocapingFingers;
    [Header("AvatarEngine的ID，对应Sender的ID")]
    public string id;
    private Queue<TransInfoOfBones> q; // 此队列用于存储接收到的每一组骨骼信息，依次更新
    public Transform[] transforms; 
    private void Awake()
    {
        q = new Queue<TransInfoOfBones>();
        if (flag)
        {
            transforms = new Transform[59];
            transforms[0] = transform.GetChild(2);
            transforms[1] = transforms[0].GetChild(1);
            transforms[2] = transforms[1].GetChild(0);
            transforms[3] = transforms[2].GetChild(0);
            transforms[4] = transforms[0].GetChild(0);
            transforms[5] = transforms[4].GetChild(0);
            transforms[6] = transforms[5].GetChild(0);
            transforms[7] = transforms[0].GetChild(2);
            transforms[8] = transforms[7].GetChild(0);
            transforms[9] = transforms[8].GetChild(0);
            transforms[10] = transforms[9].GetChild(1);
            transforms[12] = transforms[10].GetChild(0);
            transforms[13] = transforms[9].GetChild(2);
            transforms[14] = transforms[13].GetChild(0);
            transforms[15] = transforms[14].GetChild(0);
            transforms[16] = transforms[15].GetChild(0);
            transforms[17] = transforms[16].GetChild(4).GetChild(0);
            transforms[18] = transforms[17].GetChild(0);
            transforms[19] = transforms[18].GetChild(0);
            transforms[20] = transforms[16].GetChild(0);
            transforms[21] = transforms[20].GetChild(0);
            transforms[22] = transforms[21].GetChild(0);
            transforms[23] = transforms[22].GetChild(0);
            transforms[24] = transforms[16].GetChild(1);
            transforms[25] = transforms[24].GetChild(0);
            transforms[26] = transforms[25].GetChild(0);
            transforms[27] = transforms[26].GetChild(0);
            transforms[28] = transforms[16].GetChild(3);
            transforms[29] = transforms[28].GetChild(0);
            transforms[30] = transforms[29].GetChild(0);
            transforms[31] = transforms[30].GetChild(0);
            transforms[32] = transforms[16].GetChild(2);
            transforms[33] = transforms[32].GetChild(0);
            transforms[34] = transforms[33].GetChild(0);
            transforms[35] = transforms[34].GetChild(0);
            transforms[36] = transforms[9].GetChild(0);
            transforms[37] = transforms[36].GetChild(0);
            transforms[38] = transforms[37].GetChild(0);
            transforms[39] = transforms[38].GetChild(0);
            transforms[40] = transforms[39].GetChild(4).GetChild(0);
            transforms[41] = transforms[40].GetChild(0);
            transforms[42] = transforms[41].GetChild(0);
            transforms[43] = transforms[39].GetChild(0);
            transforms[44] = transforms[43].GetChild(0);
            transforms[45] = transforms[44].GetChild(0);
            transforms[46] = transforms[45].GetChild(0);
            transforms[47] = transforms[39].GetChild(1);
            transforms[48] = transforms[47].GetChild(0);
            transforms[49] = transforms[48].GetChild(0);
            transforms[50] = transforms[49].GetChild(0);
            transforms[51] = transforms[39].GetChild(3);
            transforms[52] = transforms[51].GetChild(0);
            transforms[53] = transforms[52].GetChild(0);
            transforms[54] = transforms[53].GetChild(0);
            transforms[55] = transforms[39].GetChild(2);
            transforms[56] = transforms[55].GetChild(0);
            transforms[57] = transforms[56].GetChild(0);
            transforms[58] = transforms[57].GetChild(0);
        }
    }
    private void OnEnable()
    {
        MessageDispatcher.AddListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        //MessageDispatcher.AddListener(WsMessageType.SendCChangeObj.ToString(), RecieveCChangeObj);
    }
    private void OnDisable()
    {
        MessageDispatcher.RemoveListener(WsMessageType.RecieveCChangeObj.ToString(), RecieveCChangeObj);
        //MessageDispatcher.RemoveListener(WsMessageType.SendCChangeObj.ToString(), RecieveCChangeObj);
    }

    void RecieveCChangeObj(IMessage msg)
    {
        WsCChangeInfo tempMsg = msg.Data as WsCChangeInfo;
        TransInfoOfBones t = JsonMapper.ToObject<TransInfoOfBones>(tempMsg.b);
        if (tempMsg.c == id)
            Drive(t);
    }

    /// <summary>
    /// 用于更新模型骨骼信息的方法
    /// </summary>
    /// <param name="t">存储每一帧所有骨骼信息的类的实例化变量</param> 
    void Drive(TransInfoOfBones t)
    {
        for (int i = 0; i < t.locX.Count; i++)
        {
            if (i != 11)
            {
                if (!mocapingFingers && ((i >= 17 && i <= 35) || (i >= 40)))
                    continue;
                Vector3 v = new Vector3((float)t.locX[i], (float)t.locY[i], (float)t.locZ[i]);
                Quaternion q = new Quaternion((float)t.rotX[i], (float)t.rotY[i], (float)t.rotZ[i], (float)t.rotA[i]);
                if (transforms[i] != null)
                {
                    transforms[i].localPosition = v;
                    transforms[i].localRotation = q;
                }
            }
        }
    }
}
