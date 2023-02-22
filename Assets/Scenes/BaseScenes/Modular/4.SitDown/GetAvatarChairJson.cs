using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using Newtonsoft.Json;
public class GetAvatarChairJson : MonoBehaviour
{
    /// <summary>
    /// 项目名
    /// </summary>
    public string programName = "programname";
    /// <summary>
    /// 椅子父节点
    /// </summary>
    public Transform AvatarChairRoot;
    /// <summary>
    /// 椅子数组
    /// </summary>
    List<AvatarTransformK> avatarTransformsnK = new List<AvatarTransformK>();

    // Start is called before the first frame update
    void Start()
    {
        //文件保存地址
        string savepath = Application.dataPath + "/Scenes/Modular/4.SitDown/" + programName + ".json";

        #region 数据收集
        //初始化赋值椅子列表
        for (int i = 0; i < AvatarChairRoot.childCount; i++)
        {
            avatarTransformsnK.Add(new AvatarTransformK(AvatarChairRoot.GetChild(i).transform.position, AvatarChairRoot.GetChild(i).transform.rotation));
        }
        //转json
        string c = JsonConvert.SerializeObject(avatarTransformsnK);
        print(c);//打印查看数据
        #endregion


        #region 文件处理
        //删除旧椅子数据
        if (File.Exists(savepath))
        {
            File.Delete(savepath);
        }
        //文件流写入文件
        FileStream fileStream = new FileStream(savepath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
        sw.Write(c);

        //释放资源
        sw.Close();
        fileStream.Close();

        #endregion
    }

}
/// <summary>
/// 椅子对象
/// </summary>
public class AvatarTransformK
{

    public Vektor3 wp;
    public Qukternion wr;
    public AvatarTransformK(Vector3 pos, Quaternion rot)
    {
        wp = new Vektor3(pos);
        wr = new Qukternion(rot);
    }
}
/// <summary>
/// position
/// </summary>
public class Vektor3
{
    public float x;
    public float y;
    public float z;
    public Vektor3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}
/// <summary>
/// rotation
/// </summary>
public class Qukternion
{
    public float x;
    public float y;
    public float w;
    public float z;
    public Qukternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        w = quaternion.w;
        z = quaternion.z;

    }
}
