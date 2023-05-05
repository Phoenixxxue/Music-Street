using com.ootii.Messages;
using Dll_Project.BaseUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Dll_Project.MusicStreet
{
    public class ChannelTeleport : DllGenerateBase
    {
        private Button TeleportBtn;
        private TeleportInfo outRoom;
        private TeleportInfo inRoom;

        private TeleportInfo nowTarget;
        public override void Init()
        {
            TeleportBtn = BaseMono.ExtralDatas[0].Target.GetComponent<Button>();
        }
        public override void Start()
        {
            LoadConfigJson.Instance.LoadFinishAction += () =>
            {
                string outRoomJson = LoadConfigJson.Instance.MainJsonData["SceneConfig"]["MusicStreet"]["TeleportInfo"]["outRoom"].ToJson();
                string inRoomJson  = LoadConfigJson.Instance.MainJsonData["SceneConfig"]["MusicStreet"]["TeleportInfo"]["inRoom"].ToJson();

                outRoom = LoadConfigJson.JsonToObjectByString<TeleportInfo>(outRoomJson);
                inRoom  = LoadConfigJson.JsonToObjectByString<TeleportInfo>(inRoomJson);
                TeleportBtn.onClick.AddListener(() =>
                {
                    if (TeleportBtn.gameObject.name.Equals("室内"))
                    {
                        nowTarget = inRoom;
                    }
                    else if (TeleportBtn.gameObject.name.Equals("室外"))
                    {
                        nowTarget = outRoom;
                    }
                    BaseMono.StartCoroutine(ChangeRoom(nowTarget.RoomID, nowTarget.VoiceID));
                });
            };
        }

        /// <summary>
        /// 传送的核心方法
        /// </summary>
        /// <param name="RootRoomID"></param>
        /// <param name="RootVoiceID"></param>
        /// <returns></returns>
        private IEnumerator ChangeRoom(string RootRoomID, string RootVoiceID)
        {

            yield return new WaitForSeconds(0.3f);
            if (string.IsNullOrEmpty(RootRoomID) || string.IsNullOrEmpty(RootVoiceID))
            {
                yield return null;
            }

            VRRootChanelRoom ch = new VRRootChanelRoom
            {
                roomid = RootRoomID,
                voiceid = RootVoiceID,
            };
            MessageDispatcher.SendMessage(this, VrDispMessageType.ConnectToNewChanel.ToString(), ch, 0);
        }
    }
    public class TeleportInfo
    {
        public string Name;
        public string RoomID;
        public string VoiceID;
    }
}
