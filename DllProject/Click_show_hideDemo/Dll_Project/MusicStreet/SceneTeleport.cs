using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.MusicStreet
{
    public class SceneTeleport : DllGenerateBase
    {
        public Transform TeleUI;
        public Transform ShowUI;

        private GameObject[] ButtonList;

        private bool canShowUI = true;
        
        public override void Init()
        {

            TeleUI = BaseMono.ExtralDatas[0].Target;
            ShowUI = BaseMono.ExtralDatas[1].Target;
            ButtonList = new GameObject[TeleUI.GetChild(0).childCount];
            for (int i = 0; i < ButtonList.Length; i++)
            {
               ButtonList[i] = TeleUI.GetChild(0).GetChild(i).gameObject;
               Debug.Log(ButtonList[i].name);
            }
            
        }

        public override void OnEnable()
        {
            for (int i = 0; i < ButtonList.Length; i++)
            {
                int temp = i;
                ButtonList[i].GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
                    {
                        MessageDispatcher.SendMessage(true, VrDispMessageType.AllPlaceTo.ToString(), ButtonList[temp].name, 0);
                    }
                    else
                    {
                        MessageDispatcher.SendMessage(false, VrDispMessageType.AllPlaceTo.ToString(), ButtonList[temp].name, 0);
                    }
                    TeleUI.gameObject.SetActive(false);
                });
            }
            ShowUI.GetComponent<Button>().onClick.AddListener(() =>
            {
                TeleUI.gameObject.SetActive(true);
                Debug.LogError("--");
            });
            TeleUI.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                TeleUI.gameObject.SetActive(false);
            });
        }

        //public override void OnDisable()
        //{
        //    for (int i = 0; i < ButtonList.Length; i++)
        //    {
        //        int temp = i;
        //        ButtonList[i].GetComponent<Button>().onClick.RemoveListener(() =>
        //        {
        //            Debug.Log(temp);
        //            if (mStaticThings.I.isAdmin || mStaticThings.I.sadmin)
        //            {
        //                MessageDispatcher.SendMessage(true, VrDispMessageType.AllPlaceTo.ToString(), ButtonList[temp].name, 0);
        //            }
        //            else
        //            {
        //                MessageDispatcher.SendMessage(false, VrDispMessageType.AllPlaceTo.ToString(), ButtonList[temp].name, 0);
        //            }
        //        });
        //    }
        //}
       
    }
}
