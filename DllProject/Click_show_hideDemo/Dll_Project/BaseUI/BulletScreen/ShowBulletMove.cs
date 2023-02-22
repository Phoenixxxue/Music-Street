using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.BaseUI.BulletScreen
{
    public class ShowBulletMove : DllGenerateBase
    {
        private Text showText;
        //弹幕发射的index
        private int index;
        //弹幕速度
        private float speed;

        ShowBulletScreen showBulletScreen;
        RectTransform m_rectTransform;
        //弹幕走出屏幕时x的值
        private float bulletMinX;
        //弹幕长度
        private float bulletLength;
        public override void Init()
        {
            showText = BaseMono.ExtralDatas[0].Target.GetComponent<Text>();
            index = int.Parse(BaseMono.ExtralDatas[1].OtherData);
            showBulletScreen = ShowBulletScreen.Instance;
            m_rectTransform = BaseMono.transform.GetComponent<RectTransform>();
        }
        public override void Awake()
        {
        }

        public override void Start()
        {
            showText.text = BaseMono.OtherData;
            LayoutRebuilder.ForceRebuildLayoutImmediate(showText.gameObject.GetComponent<RectTransform>());
            bulletLength = showText.gameObject.GetComponent<RectTransform>().sizeDelta.x + 30;
            m_rectTransform.sizeDelta = new Vector2(bulletLength, 40);
            bulletMinX = -showBulletScreen.ScreenWidth - bulletLength;
            speed = showBulletScreen.bulletSpeed;
            if (speed == 0)
            {
                Debug.LogError("speed can't be 0");
            }
            float bulletCDTime = bulletLength / speed * Time.fixedDeltaTime;
            showBulletScreen.cowsStatus[index] = false;
        }
        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }
        //是否离开发射区域
        private bool isShootComplete = false;
        public override void Update()
        {
            //发射成功时
            if (m_rectTransform.anchoredPosition.x < -bulletLength && !isShootComplete)
            {
                isShootComplete = true;
                showBulletScreen.cowsStatus[index] = true;
            }
            //正在发射
            if (!isShootComplete && showBulletScreen.cowsStatus[index])
            {
                showBulletScreen.cowsStatus[index] = false;
            }
            //弹幕到了屏幕尽头时
            if (m_rectTransform.anchoredPosition.x < bulletMinX)
            {
                //防止屏幕太小弹幕太长
                showBulletScreen.cowsStatus[index] = true;
                GameObject.Destroy(BaseMono.gameObject);
            }
        }

        public override void FixedUpdate()
        {
            m_rectTransform.position -= new Vector3(speed, 0, 0);
        }
    }
}
