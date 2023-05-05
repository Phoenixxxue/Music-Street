using com.ootii.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.MusicStreet
{
    public class showUI : DllGenerateBase
    {
        public Transform TeleUI;
        public Transform ShowUI;

        public override void Init()
        {
            TeleUI = BaseMono.ExtralDatas[0].Target;
            ShowUI = BaseMono.ExtralDatas[1].Target;
        }

        public override void OnEnable()
        {
            ShowUI.GetComponent<Button>().onClick.AddListener(() =>
            {
                TeleUI.gameObject.SetActive(true);
            });
            TeleUI.Find("CancelButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                TeleUI.gameObject.SetActive(false);
            });
        }
    }
}
