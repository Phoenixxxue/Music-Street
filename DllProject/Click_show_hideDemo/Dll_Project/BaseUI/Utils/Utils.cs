using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dll_Project.BaseUI.Utils
{
    public class Utils
    {
        public static bool IsInTime(List<DayOfWeek> days, TimeSpan ts_Start, TimeSpan ts_End)
        {
            if (!days.Contains(DateTime.Now.DayOfWeek))
            {
                return false;
            }
            if (DateTime.Now.TimeOfDay < ts_Start || DateTime.Now.TimeOfDay > ts_End)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 用射线检测判断是否在某个collider上方
        /// </summary>
        /// <returns>是否在签到区域</returns>
        public static bool IsOnCollider(Vector3 origin, Collider collider)
        {
            bool isOnCollider = false;
            Ray ray = new Ray(origin + Vector3.up, Vector3.down);
            RaycastHit[] hitInfos;
            //射线检测到的所有信息
            hitInfos = Physics.RaycastAll(ray);
            for (int i = 0; i < hitInfos.Length; i++)
            {
                if (hitInfos[i].collider == collider)
                {
                    isOnCollider = true;
                }
            }
            return isOnCollider;
        }
    }
}
