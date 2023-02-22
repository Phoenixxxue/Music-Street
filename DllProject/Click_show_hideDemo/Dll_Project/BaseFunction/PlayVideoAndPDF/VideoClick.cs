using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Dll_Project.PlayVideoAndPDF
{
    public class VideoClick : DllGenerateBase
    {
        private VideoPlayer videoPlayer;
        private Transform sliderObj;
        private Button openBtn;
        private Button closeBtn;
        private Button TuiBtn;
        private Button JinBtn;

        private GameObject img;//暂停是图标
        private Text nowTime;
        private Text allTime;

        private Button volumeBtn;
        private Slider volumeSlider;

        private bool isMouse = true;
        private bool isVolume = true;

        public override void Init()
        {
            videoPlayer = BaseMono.ExtralDatas[0].Target.GetComponent<VideoPlayer>();
            sliderObj = BaseMono.ExtralDatas[1].Target;
            openBtn = BaseMono.ExtralDatas[2].Target.GetComponent<Button>();
            closeBtn = BaseMono.ExtralDatas[3].Target.GetComponent<Button>();
            TuiBtn = BaseMono.ExtralDatas[4].Target.GetComponent<Button>();
            JinBtn = BaseMono.ExtralDatas[5].Target.GetComponent<Button>();

            img = BaseMono.ExtralDatas[6].Target.gameObject;
            nowTime = BaseMono.ExtralDatas[7].Target.GetComponent<Text>();
            allTime = BaseMono.ExtralDatas[8].Target.GetComponent<Text>();

            volumeBtn = BaseMono.ExtralDatas[9].Target.GetComponent<Button>();
            volumeSlider = BaseMono.ExtralDatas[10].Target.GetComponent<Slider>();
        }
        #region 初始
        public override void Awake()
        {
        }

        public override void Start()
        {
            openBtn.onClick.AddListener(PlayVideoClick);
            closeBtn.onClick.AddListener(PauseVideoClick);
            TuiBtn.onClick.AddListener(TuiClick);
            JinBtn.onClick.AddListener(JinClick);

            volumeBtn.onClick.AddListener(ShowVolumePanelClick);
            volumeSlider.onValueChanged.AddListener(VolumeSlider);

            TriggerClick();
            VolumeTriggerClick();

            if (mStaticThings.I != null)
            {
                if (!mStaticThings.I.isVRApp)
                {
                    TuiBtn.gameObject.SetActive(false);
                    JinBtn.gameObject.SetActive(false);
                }
            }
        }
        public override void OnEnable()
        {
            volumeSlider.value = videoPlayer.GetComponent<AudioSource>().volume;
        }

        public override void OnDisable()
        {
        }
        private bool isOpenVideo = true;
        float time;
        public override void Update()
        {
            if (isOpenVideo && isMouse)
            {
                ChangeJinDuTiao();
            }

            if (volumeSlider.gameObject.activeSelf && isVolume)
            {
                time += Time.deltaTime;
                if (time > 3)
                {
                    volumeSlider.gameObject.SetActive(false);
                    time = 0;
                }
            }
        }
        #endregion


        private void PlayVideoClick()
        {
            openBtn.gameObject.SetActive(false);
            closeBtn.gameObject.SetActive(true);
            videoPlayer.Play();
            img.SetActive(false);
        }
        private void PauseVideoClick()
        {
            openBtn.gameObject.SetActive(true);
            closeBtn.gameObject.SetActive(false);
            videoPlayer.Pause();
            img.SetActive(true);

        }
        private void TuiClick() //快退
        {
            sliderObj.GetComponent<Slider>().value -= 0.1f;
            ChangeVideoPlayTime(sliderObj.GetComponent<Slider>().value);
        }
        private void JinClick()
        {
            sliderObj.GetComponent<Slider>().value += 0.1f;
            ChangeVideoPlayTime(sliderObj.GetComponent<Slider>().value);
        }

        private void ChangeVideoPlayTime(float value)//修改视频播放进度
        {
            videoPlayer.frame = long.Parse((value * videoPlayer.frameCount).ToString("0."));
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }

        private void ChangeJinDuTiao()//修改进度条 
        {
            sliderObj.GetComponent<Slider>().value = (float)(videoPlayer.frame / (float)videoPlayer.frameCount);
            ShowVideoTime();
            ShowVideoLength();
        }


        #region  视频时间显示
        // 当前视频的总时间值和当前播放时间值的参数
        private int currentHour;
        private int currentMinute;
        private int currentSecond;
        private int clipHour;
        private int clipMinute;
        private int clipSecond;

        /// <summary>
        /// 显示当前视频的时间
        /// </summary>
        private void ShowVideoTime()
        {
            // 当前的视频播放时间
            currentHour = (int)videoPlayer.time / 3600;
            currentMinute = (int)(videoPlayer.time - currentHour * 3600) / 60;
            currentSecond = (int)(videoPlayer.time - currentHour * 3600 - currentMinute * 60);
            // 把当前视频播放的时间显示在 Text 上
            nowTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", currentHour, currentMinute, currentSecond);
        }

        /// <summary>
        /// 显示视频的总时长
        /// </summary>
        /// <param name="videos">当前视频</param>
        void ShowVideoLength()
        {
            float at = videoPlayer.frameCount / videoPlayer.frameRate;
            clipHour = (int)at / 60;
            clipMinute = (int)at % 60;
            allTime.text = string.Format("{0}:{1}", clipHour.ToString("00"), clipMinute.ToString("00"));
        }
        #endregion

        /// <summary>
        /// 展示音量调节滑动条
        /// </summary>
        private void ShowVolumePanelClick()
        {
            if (volumeSlider.gameObject.activeSelf)
            {
                volumeSlider.gameObject.SetActive(false);
                time = 0;
            }
            else
            {
                volumeSlider.gameObject.SetActive(true);
            }
        }
        /// <summary>
        /// 音量滑动条事件
        /// </summary>
        private void VolumeSlider(float value)
        {
            videoPlayer.GetComponent<AudioSource>().volume = value;
        }

        #region 视频进度条抬起按下
        private void TriggerClick()
        {
            EventTrigger Uptrigger = sliderObj.GetComponent<EventTrigger>();
            EventTrigger.Entry Upentry1 = new EventTrigger.Entry();
            EventTrigger.Entry Upentry2 = new EventTrigger.Entry();

            Upentry1.eventID = EventTriggerType.PointerDown;
            Upentry2.eventID = EventTriggerType.PointerUp;

            Upentry1.callback = new EventTrigger.TriggerEvent();
            Upentry1.callback.AddListener(ClickUpDownEvent);
            Uptrigger.triggers.Add(Upentry1);

            Upentry2.callback = new EventTrigger.TriggerEvent();
            Upentry2.callback.AddListener(ClickUpUpEvent);
            Uptrigger.triggers.Add(Upentry2);
        }
        void ClickUpDownEvent(BaseEventData pointData)
        {
            isMouse = false;
            videoPlayer.Pause();
        }

        void ClickUpUpEvent(BaseEventData pointData)
        {
            videoPlayer.frame = long.Parse((sliderObj.GetComponent<Slider>().value * videoPlayer.frameCount).ToString("0."));
            BaseMono.StartCoroutine(ShowVideo());
        }

        private IEnumerator ShowVideo()
        {
            yield return new WaitForSeconds(0.2f);
            isMouse = true;
            if (!openBtn.gameObject.activeSelf)
            {
                videoPlayer.Play();
            }
        }
        #endregion

        #region 音量进度条抬起按下
        private void VolumeTriggerClick()
        {
            EventTrigger Uptrigger = volumeSlider.GetComponent<EventTrigger>();
            EventTrigger.Entry Upentry1 = new EventTrigger.Entry();
            EventTrigger.Entry Upentry2 = new EventTrigger.Entry();

            Upentry1.eventID = EventTriggerType.PointerEnter;
            Upentry2.eventID = EventTriggerType.PointerExit;

            Upentry1.callback = new EventTrigger.TriggerEvent();
            Upentry1.callback.AddListener(VolumeClickEnterEvent);
            Uptrigger.triggers.Add(Upentry1);

            Upentry2.callback = new EventTrigger.TriggerEvent();
            Upentry2.callback.AddListener(VolumeClickExitEvent);
            Uptrigger.triggers.Add(Upentry2);
        }
        void VolumeClickEnterEvent(BaseEventData pointData)
        {
            isVolume = false;
            time = 0;
        }

        void VolumeClickExitEvent(BaseEventData pointData)
        {
            isVolume = true;
        }
        #endregion
    }
}
