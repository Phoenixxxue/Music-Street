using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Messages;

public class Manager : DllGenerateBase
{
    bool maleActive;
    public List<AvatarEngine> engines;
    public override void Awake()
    {
        maleActive = false;
    }
    public override void OnEnable()
    {
        MessageDispatcher.AddListener(CommonVREventType.VRRaw_Y_ButtonDown.ToString(), ClickY);
    }
    public override void OnDisable()
    {
        MessageDispatcher.RemoveListener(CommonVREventType.VRRaw_Y_ButtonDown.ToString(), ClickY);
    }

    void ClickY(IMessage msg)
    {
        Click();
    }

    void Click()
    {
        if (maleActive)
        {
            engines[0].transform.gameObject.SetActive(false);
            engines[1].transform.gameObject.SetActive(true);
        }
        else
        {
            engines[0].transform.gameObject.SetActive(true);
            engines[1].transform.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        if (Input.GetKeyUp(KeyCode.K))
        {
            Click();
        }
    }
}
