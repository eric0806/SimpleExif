using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleExif.Functions;
using SimpleExif.Models;

namespace SimpleExif {
    public partial class Exif : IDisposable {
        /// <summary>
        /// 檔案完整路徑
        /// </summary>
        private string FilePath { get; set; }

        /// <summary>
        /// 是否為Jpeg檔案
        /// </summary>
        private bool IsJpeg { get; set; }

        /// <summary>
        /// 該檔案的App1區塊資料
        /// </summary>
        private App1 App1 { get; set; }

        /// <summary>
        /// 該檔案的TiffExif區塊資料
        /// </summary>
        private TiffExif TiffExif { get; set; }

        public bool ExifExists { get; internal set; }

        public string ErrorMsg {
            get {
                try { return TiffExif.ErrorMsg; }
                catch { return string.Empty; }
            }
        }

        public long ProcessMilliseconds {
            get {
                try { return sw.ElapsedMilliseconds; }
                catch { return -1; }
            }
        }

        /// <summary>
        /// 可供自行利用的IFD列表，儲存每個獨立IFD
        /// </summary>
        public List<IFD> IFDList {
            get {
                return TiffExif.IFDList;
            }
        }

        /// <summary>
        /// 一般用Exif列表，儲存每個Entry
        /// </summary>
        public List<IFDEntry> ExifList {
            get { return TiffExif.ExifList; }
        }

        public Image Thumbnail {
            get { return TiffExif.Thumbnail; }
        }

        Stopwatch sw;
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="filePath">要讀入Exif的檔案完整路徑</param>
        /// <param name="fetchThumb">是否要取得exif內之縮圖</param>
        public Exif(string filePath, bool fetchThumb = true) {
            ExifExists = true;
            //IFDList = new List<IFD>();
            FilePath = filePath;
            sw = new Stopwatch();

            Parse(fetchThumb);
        }

        private void Parse(bool fetchThumb) {
            sw.Start();
            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read)) {

                if (Tools.Misc.IsJpeg(fs, 0)) {
                    //跳過APP0並取得新位址
                    Tools.Misc.IgnoreAPP0(fs, fs.Position);
                    App1 = App1.Create(fs);
                    TiffExif = App1.TiffExifModel;
                }
                else {
                    TiffExif = new TiffExif(fs);
                }

                if (TiffExif == null) {
                    ExifExists = false;
                    return;
                }

                TiffExif.Parse(fetchThumb);

                ExifExists = TiffExif.Success;
            }
            sw.Stop();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // 處置 Managed 狀態 (Managed 物件)。
                    App1 = null;
                    TiffExif.IFDList.Clear();
                    TiffExif = null;
                }

                // 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        // ~Exif() {
        //   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose() {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            GC.SuppressFinalize(this);
        }
        #endregion



    }
}
