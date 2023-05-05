using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Video;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace Dll_Project.PlayVideo
{
    public class MVideoPlayer
    {
        public void PlayNewVideo(VideoPlayer videoPlayer, string url, RawImage rawImage)
        {
            if (videoPlayer == null)
            {
                return;
            } 
            if (string.IsNullOrEmpty(url) || !File.Exists(url))
            {
                return;
            }
              
            DisposeVideoPlayer(videoPlayer);
            if (videoPlayer.targetTexture != null)
            {
                rawImage.texture = videoPlayer.targetTexture;
            }
                
            videoPlayer.url = url;
            videoPlayer.Play();
        }

        public void PlayNewVideo(VideoPlayer videoPlayer, string url, Material material)
        {
            if (videoPlayer == null)
                return;
            if (string.IsNullOrEmpty(url) || !File.Exists(url))
                return;
            DisposeVideoPlayer(videoPlayer);

            if (videoPlayer.targetTexture != null)
                material.SetTexture("_MainTex", videoPlayer.targetTexture);

            videoPlayer.url = url;
            videoPlayer.Play();
        }
        public void PlayVideo(VideoPlayer videoPlayer)
        {
            if (videoPlayer == null || videoPlayer.url == null)
                return;
            videoPlayer.Play();
        }
        public void PauseVideo(VideoPlayer videoPlayer)
        {
            if (videoPlayer == null)
                return;
            videoPlayer.Pause();
        }
        public void StopVideo(VideoPlayer videoPlayer)
        {
            if (videoPlayer == null || videoPlayer.url == null)
                return;
            videoPlayer.Stop();
            DisposeVideoPlayer(videoPlayer);
        }

        public void DisposeVideoPlayer(VideoPlayer videoPlayer)
        {
            if (videoPlayer != null && videoPlayer.targetTexture != null)
            {
                videoPlayer.Stop();
                videoPlayer.targetTexture.Release();
            }
        }
    }
}
