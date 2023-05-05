using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dll_Project
{
    public class LoadConfigJson : DllGenerateBase
    {
        public static LoadConfigJson Instance;

        public string JsonPath;
        /// <summary>
        /// 下载下来的主要Json类
        /// </summary>
        public JsonData mainJsonData;
        public JsonData MainJsonData
        {
            get
            {
                return mainJsonData["data"]["dataContent"];
            }
        }
        public JsonData CardMainJsonData
        {
            get
            {
                return mainJsonData["data"];
            }
        }
        /// <summary>
        /// 下载完成回调事件
        /// </summary>
        public Action LoadFinishAction;
        /// <summary>
        /// 加载完成回调事件
        /// </summary>
        public Action LoadConfigFinishAction;
        public override void Init()
        {
            //  Debug.Log("LoadConfigjson");
            JsonPath = "https://s.vswork.space/space/525b789c-390b-4277-a6fb-e8bf57b13158";
        }
        public override void Start()
        {
            Debug.Log("LoadConfigJson  Start");
            if (string.IsNullOrEmpty(mStaticThings.I.nowRoomActionAPI))
            {
                  BaseMono.StartCoroutine(LoadIniConfigFile(JsonPath, 0));
            }
            else
            {
                 BaseMono.StartCoroutine(LoadIniConfigFile(mStaticThings.I.nowRoomActionAPI, 0));

            }
        }
        public override void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 加载总配置文件Json
        /// </summary>
        /// <param name="mPath"></param>
        /// <param name="delayTime"></param>
        /// <returns></returns>
        IEnumerator LoadIniConfigFile(string mPath, float delayTime = 0)
        {
            yield return new WaitForSeconds(delayTime);
            if (!mPath.StartsWith("http"))
            {
                yield break;
            }
            mPath += "/data";
            var uwr = UnityWebRequest.Get(mPath);
            uwr.SetRequestHeader("apikey", mStaticThings.apikey);
            uwr.SetRequestHeader("apitoken", mStaticThings.apitoken);
            uwr.SetRequestHeader("version", "2");
            yield return uwr.SendWebRequest();
            if (!string.IsNullOrEmpty(uwr.error))
            {
                uwr.Dispose();
                BaseMono.StartCoroutine(LoadIniConfigFile(JsonPath, 3));
            }
            else
            {
                mainJsonData = JsonMapper.ToObject(uwr.downloadHandler.text);
                //Debug.LogError(uwr.downloadHandler.text);
                if (LoadFinishAction != null)
                {
                    LoadFinishAction();
                }

                uwr.Dispose();
            }
        }
        private void DataCollectJson(string str)
        {
            JsonData jd = JsonMapper.ToObject(str);
            if (!jd.ToJson().Contains("DataCollection"))
                return;
            JsonData jsonData = jd["DataCollection"];
            //if (!string.IsNullOrEmpty(jsonData.ToJson()))
            //{
            //    InfoCollectController.Instance.isOpen = bool.Parse(jsonData["DataCollection"].ToString());
            //    InfoCollectController.Instance.isSaveTimeZoom = bool.Parse(jsonData["SaveTimeZoom"].ToString());
            //    InfoCollectController.Instance.isSaveViewData = bool.Parse(jsonData["SaveViewData"].ToString());
            //}
        }
        /// <summary>
        /// 把一个Json格式的文本，转成一个对象(Object)
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="filePath">Json文本中的内容</param>
        /// <returns></returns>
        public static List<T> JsonToListObjectByString<T>(string content)
        {
            /*直接解析成对象*/
            //解析Json文本中的内容 -(解析成数组或者List列表都可以)
            //将Json中的int数据转成string
            //JsonMapper.RegisterImporter<int, string>((int value) =>
            //{
            //    return value.ToString();
            //});
            T[] datas = JsonMapper.ToObject<T[]>(content);
            //把数组封装成List列表
            List<T> dataList = new List<T>();
            for (int i = 0; i < datas.Length; i++)
            {
                dataList.Add(datas[i]);
            }
            return dataList;
        }

        public static T JsonToObjectByString<T>(string content)
        {
            T data = JsonMapper.ToObject<T>(content);
            return data;
        }
        class AvatarInfo
        {
            public string id;
            public string name;
            public string mark;
        }
    }
}