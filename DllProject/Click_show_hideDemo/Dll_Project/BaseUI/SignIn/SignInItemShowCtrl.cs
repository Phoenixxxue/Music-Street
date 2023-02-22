using UnityEngine;
using UnityEngine.UI;
/***
* Developed by baohan
*/
namespace Dll_Project.BaseUI.SignIn
{
    class SignInItemShowCtrl : DllGenerateBase
    {
        private Text text_Name;
        private Text text_IsSignedIn;
        private Toggle toggle_IsArrive;
        private Image img_SignIn;
        public override void Init()
        {
            text_Name = BaseMono.transform.Find("name").GetComponent<Text>();
            text_IsSignedIn = BaseMono.transform.Find("BG/Text_SignedIn").GetComponent<Text>();
            toggle_IsArrive = BaseMono.transform.Find("Toggle_IsOnline").GetComponent<Toggle>();
            img_SignIn = BaseMono.transform.Find("BG/Img_Bg").GetComponent<Image>();
            toggle_IsArrive.interactable = false;
        }
        public override void Awake()
        {
        }
        public override void Start()
        {

        }
        public override void Update()
        {
        }
        public override void OnEnable()
        {
        }
        public override void OnDisable()
        {
        }
        public override void OnDestroy()
        {
        }
        public void RefreshShowDetail(SignInData data)
        {
            text_Name.text = string.IsNullOrEmpty(data.Name) ? "未填写" : data.Name;
            switch (data.SignInStatu)
            {
                case UserSignInStatu.NotSignedIn:
                    text_IsSignedIn.text = "未签到";
                    img_SignIn.color = new Color((float)255 / 255, (float)85 / 255, 0);
                    break;
                case UserSignInStatu.SignedIn:
                    text_IsSignedIn.text = "已签到";
                    img_SignIn.color = new Color((float)58 / 255, (float)200 / 255, (float)55 / 255);
                    break;
                case UserSignInStatu.AddSignedIn:
                    text_IsSignedIn.text = "已补签";
                    img_SignIn.color = new Color((float)50 / 255, (float)100 / 255, (float)255 / 255);
                    break;
                default:
                    break;
            }
            toggle_IsArrive.isOn = data.IsArrive;
        }
        public void EnableItem(bool isEnable)
        {
            BaseMono.gameObject.SetActive(isEnable);
        }
    }
}
