using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Dll_Project.MutiTeleport
{
    public class MulticastTeleport : DllGenerateBase
    {
        private Transform ParentObj;
        private GameObject[] ButtonList;
        private string[] TargetName;

        public override void Init()
        {
            ParentObj = BaseMono.ExtralDatas[0].Target;
            ButtonList = new GameObject[ParentObj.childCount];
            TargetName = new string[ParentObj.childCount];
            for (int i = 0; i < ButtonList.Length; i++)
            {
                ButtonList[i] = ParentObj.GetChild(i).gameObject;
                TargetName[i] = ParentObj.GetChild(i).gameObject.name;

            }
        }

        public override void Start()
        {

        }
        public override void OnEnable()
        {
            for (int i = 0; i < ButtonList.Length; i++)
            {
                int temp = i;
                ButtonList[i].GetComponent<Button>().onClick.AddListener(() =>
                {
                    Debug.Log(temp);
                    if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                    {
                        MessageDispatcher.SendMessage(true, VrDispMessageType.AllPlaceTo.ToString(), TargetName[temp], 0);
                    }
                    else
                    {
                        MessageDispatcher.SendMessage(false, VrDispMessageType.AllPlaceTo.ToString(), TargetName[temp], 0);
                    }
                });
            }
        }

        public override void OnDisable()
        {

            for (int i = 0; i < ButtonList.Length; i++)
            {
                int temp = i;
                ButtonList[i].GetComponent<Button>().onClick.RemoveListener(() =>
                {
                    Debug.Log(temp);
                    if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                    {
                        MessageDispatcher.SendMessage(true, VrDispMessageType.AllPlaceTo.ToString(), TargetName[temp], 0);
                    }
                    else
                    {
                        MessageDispatcher.SendMessage(false, VrDispMessageType.AllPlaceTo.ToString(), TargetName[temp], 0);
                    }
                });
            }
        }
        //private void teleport()
        //{
        //    if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
        //    {
        //        MessageDispatcher.SendMessage(true, VrDispMessageType.AllPlaceTo.ToString(), TargetName[0], 0);
        //    }
        //    else
        //    {
        //        MessageDispatcher.SendMessage(false, VrDispMessageType.AllPlaceTo.ToString(), TargetName[0], 0);
        //    }
        //}
    }


}
