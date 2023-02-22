using Paroxe.PdfRenderer;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Dll_Project.PlayVideoAndPDF
{
    public class PDFPlayer
    {
        private PDFDocument pdfDocument;
        private PDFRenderer renderer;
        private Texture2D tempTex;
        private int pageCount;//PPT总页数

        /// <summary>
        /// 播放新的PDF
        /// </summary>
        /// <param name="url"></param>
        /// <param name="material"></param>
        /// <param name="rawImage"></param>
        public int PlayNewPDF(string url, Material material, RawImage rawImage)
        {
            DisposePdf();
            if (material == null && rawImage == null)
                return -1;
            pdfDocument = new PDFDocument(url);
            renderer = new PDFRenderer();
            //如果PDF是有效的
            if (pdfDocument.IsValid)
            {
                int pdfPage = 0;//从第一页开始播放，index为0
                pageCount = pdfDocument.GetPageCount();//获取总页数
                Debug.Log("页数：" + pageCount);

                tempTex = GetPDFPageByIndex(renderer, pdfPage, pageCount);

                if (material != null)
                {
                    if (material.GetTexture("_MainTex") != null)
                        GameObject.Destroy(material.GetTexture("_MainTex"));
                    material.SetTexture("_MainTex", tempTex);
                }
                if (rawImage != null)
                {
                    if (rawImage.texture != null)
                        GameObject.Destroy(rawImage.texture);
                    rawImage.texture = tempTex;
                }
                return pdfPage;
            }
            else//无效PDF就删除
            {
                if (File.Exists(url))
                {
                    File.Delete(url);
                }
                return -1;
            }
        }
        /// <summary>
        /// 播放PDF某一页
        /// </summary>
        /// <param name="pageIndex"></param>
        public int PlayPDFPage(int pageIndex, Material material, RawImage rawImage)
        {
            if (material == null && rawImage == null)
                return -1;
            if (pageIndex < 0)
                pageIndex = pageCount - 1;
            else if (pageIndex > pageCount - 1)
                pageIndex = 0;
            tempTex = GetPDFPageByIndex(renderer, pageIndex, pageCount);
            if (material != null)
            {
                if (material.GetTexture("_MainTex") != null)
                    GameObject.Destroy(material.GetTexture("_MainTex"));
                material.SetTexture("_MainTex", tempTex);
            }
            if (rawImage != null)
            {
                if (rawImage.texture != null)
                    GameObject.Destroy(rawImage.texture);
                rawImage.texture = tempTex;
            }
            return pageIndex;
        }
        /// <summary>
        /// 获取PDF指定一页的贴图
        /// </summary>
        /// <param name="index"></param>
        /// <param name="allPage"></param>
        /// <returns></returns>
        private Texture2D GetPDFPageByIndex(PDFRenderer renderer, int index, int allPage)
        {
            Texture2D texture2D = renderer.RenderPageToTexture(pdfDocument.GetPage(index % allPage));
            texture2D.filterMode = FilterMode.Bilinear;
            texture2D.anisoLevel = 8;
            return texture2D;
        }
        public void DisposePdf()
        {
            if (pdfDocument != null)
            {
                pdfDocument.Dispose();
                pdfDocument = null;
            }
            if (renderer != null)
            {
                renderer.Dispose();
                renderer = null;
            }
            tempTex = null;
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}
