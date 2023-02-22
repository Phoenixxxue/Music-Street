using System.Collections.Generic;
using UnityEngine;

/***
* Developed by nizhuwei，organized by baohan
*/
namespace Dll_Project.BaseUI.SignIn
{ 
    public class PoolManager : DllGenerateBase
    {
        public Dictionary<PoolType, Pool<GameObject>> allPools = new Dictionary<PoolType, Pool<GameObject>>();
        private GameObject SeatItem;
        private GameObject MusicItem;
        private GameObject PdfItem;
        private GameObject SpeakItem;
        private GameObject PeopleItem;
        private GameObject SignInItem;
        private Transform SeatItemParent;
        private Transform MusicItemParent;
        private Transform PdfItemParent;
        private Transform SpeakItemParent;
        private Transform PeopleItemParent;
        private Transform SignInItemParent;

        public static PoolManager Instance;
        public override void Awake()
        {
            Instance = this;
        }
        public override void Start()
        {

            // 初始化对象池
            allPools.Add(PoolType.Seat, new Pool<GameObject>());
            allPools.Add(PoolType.Music, new Pool<GameObject>());
            allPools.Add(PoolType.Pdf, new Pool<GameObject>());
            allPools.Add(PoolType.Speak, new Pool<GameObject>());
            allPools.Add(PoolType.People, new Pool<GameObject>());
            allPools.Add(PoolType.SignIn, new Pool<GameObject>());
            SeatItemParent = BaseMono.ExtralDatas[0].Target;
            MusicItemParent = BaseMono.ExtralDatas[1].Target;
            PdfItemParent = BaseMono.ExtralDatas[2].Target;
            SpeakItemParent = BaseMono.ExtralDatas[3].Target;
            PeopleItemParent = BaseMono.ExtralDatas[4].Target;
            SignInItemParent = BaseMono.ExtralDatas[5].Target;
            SeatItem = BaseMono.ExtralDataObjs[0].Target as GameObject;
            MusicItem = BaseMono.ExtralDataObjs[1].Target as GameObject;
            PdfItem = BaseMono.ExtralDataObjs[2].Target as GameObject;
            SpeakItem = BaseMono.ExtralDataObjs[3].Target as GameObject;
            PeopleItem = BaseMono.ExtralDataObjs[4].Target as GameObject;
            SignInItem = BaseMono.ExtralDataObjs[5].Target as GameObject;

        }
        /// <summary>
        /// 压入对应的池
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="poolType"></param>
        public void PushItem(GameObject bullet, PoolType poolType)
        {
            bullet.SetActive(false); // 失活的
            allPools[poolType].Push(bullet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="poolType"></param>
        /// <returns></returns>
        public GameObject PopItem(PoolType poolType)
        {
            GameObject bullet = allPools[poolType].Pop();
            if (bullet == null)
            {
                switch (poolType)
                {
                    case PoolType.Music:
                        bullet = Object.Instantiate(MusicItem, MusicItemParent);
                        bullet.transform.SetAsFirstSibling();
                        break;
                    case PoolType.Pdf:
                        bullet = Object.Instantiate(PdfItem, PdfItemParent);
                        bullet.transform.SetAsFirstSibling();
                        break;
                    case PoolType.Seat:
                        bullet = Object.Instantiate(SeatItem, SeatItemParent);
                        bullet.transform.SetAsFirstSibling();
                        break;
                    case PoolType.Speak:
                        bullet = Object.Instantiate(SpeakItem, SpeakItemParent);
                        bullet.transform.SetAsFirstSibling();
                        break;
                    case PoolType.People:
                        bullet = Object.Instantiate(PeopleItem, PeopleItemParent);
                        bullet.transform.SetAsFirstSibling();
                        break;
                    case PoolType.SignIn:
                        bullet = Object.Instantiate(SignInItem, SignInItemParent);
                        bullet.transform.SetAsLastSibling();
                        break;
                }
            }
            else
            {
                bullet.SetActive(true);
            }
            return bullet;
        }
    }
    public enum PoolType
    {
        People,
        Seat,
        Speak,
        Music,
        Pdf,
        SignIn
    }
    public class Pool<T>
    {

        Stack<T> objs = new Stack<T>();

        public void Push(T item)
        {
            T node = item;
            objs.Push(node);

        }

        public T Pop()
        {

            if (objs.Count > 0)
            {
                T node = objs.Pop();

                return node;

            }
            else
            {
                return default;
            }


        }
        public bool Contains(T Item)
        {
            return objs.Contains(Item);
        }

    }
}


