using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SimpleExif.Functions;
using SimpleExif.Models;

namespace SimpleExif {
    /// <summary>
    /// Exif的類別
    /// </summary>
    internal class TiffExif {
        private FileStream fs;

        public bool Success { get; set; }

        public string ErrorMsg { get; internal set; }

        /// <summary>
        /// 指出是否為Jpeg檔案(跟取得縮圖有關，壓縮與未壓縮圖檔的縮圖tag不一樣)
        /// </summary>
        public bool IsJpeg { get; set; }

        /// <summary>
        /// 是否低位元在前
        /// </summary>
        public bool IsLittleEndian { get; set; }

        /// <summary>
        /// Header距離檔案開頭的距離
        /// </summary>
        public long HeaderPosition { get; set; }

        /// <summary>
        /// TiffHeader
        /// </summary>
        public TiffHeader TiffHeader { get; set; }
        /// <summary>
        /// 所有的IFD列表
        /// </summary>
        public List<IFD> IFDList { get; set; }

        public List<IFDEntry> ExifList { get; set; }

        /// <summary>
        /// MakerNote IFD(獨立出來以便單獨處理)
        /// </summary>
        private IFD MakerNoteIFD { get; set; }

        /// <summary>
        /// 縮圖
        /// </summary>
        public Image Thumbnail { get; set; }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="fileStream"></param>
        public TiffExif(FileStream fileStream) {
            fs = fileStream;
            HeaderPosition = fs.Position;
            IsLittleEndian = false;
            TiffHeader = new TiffHeader();
            IFDList = new List<IFD>();
            MakerNoteIFD = null;
            Success = true;
            IsJpeg = false;
            ExifList = new List<IFDEntry>();
            Thumbnail = null;

            try {
                //讀取Tiff Header資料
                //byte order
                fs.Read(TiffHeader.ByteOrder, 0, 2);
                if (Tools.Bin.BytesToInt(TiffHeader.ByteOrder) == Consts.II) {
                    this.IsLittleEndian = true;
                }

                //所有陣列都存放原始順序資料，要取值得時候才做處理
                fs.Read(TiffHeader.TiffID, 0, 2);
                fs.Read(TiffHeader.NextIFDOffset, 0, 4);

                //如果固定位元不等於0x002A則表示不是TIFF Header，回報錯誤
                if (Tools.Bin.BytesToInt(TiffHeader.TiffID, IsLittleEndian) != Consts.TIFF_ID) {
                    Success = false;
                    return;
                }

                TiffHeader.NextIFDOffsetValue = Tools.Bin.BytesToInt(TiffHeader.NextIFDOffset, IsLittleEndian);
            }
            catch (Exception ex) {
                ErrorMsg = string.Format("Get Tiff Header:{0}", ex.Message);
                Success = false;
            }
        }

        /// <summary>
        /// 解析TiffExif
        /// </summary>
        /// <param name="fetchThumb"></param>
        public void Parse(bool fetchThumb = false) {
            try {
                FindIFD();
                ParseIFD();
                AddExifList();
                if (fetchThumb) {
                    GetThumb();
                }
            }
            catch (Exception ex) {
#if DEBUG
                ErrorMsg = ex.ToString();
#else
                ErrorMsg = ex.Message;
#endif
                Success = false;
            }
        }

        #region FindIFD相關
        /// <summary>
        /// 找出並將將原始IFD資料寫入物件中
        /// </summary>
        private void FindIFD() {
            FindBaseIFD();
            FindSpecialIFD();
        }

        /// <summary>
        /// 找出基本IFD(IFD0 IFD1....)
        /// </summary>
        private void FindBaseIFD() {
            if (!Success) { return; }

            long nextIFDPosition = TiffHeader.NextIFDOffsetValue;

            int ifdIndex = 0;
            //如果下個IFDOffset不等於0就要處理下個IFD
            while (true) {
                var ifdName = string.Format("IFD{0}", ifdIndex++);

                //產生並取得下個IFD的Offset
                nextIFDPosition = MakeIFD(nextIFDPosition, ifdName);

                //如果下個IFD Offset等於0，表示沒有下個IFD，跳出迴圈
                if (nextIFDPosition == 0) {
                    break;
                }
            }
            //處理完後，fs.Position應該是在IFD1的底端(Data Block前)
        }

        /// <summary>
        /// 找出特殊IFD(EXIF、GPS、MakerNote)
        /// </summary>
        private void FindSpecialIFD() {
            if (!Success) { return; }

            IFD[] ifdAry = new IFD[IFDList.Count];
            IFDList.CopyTo(ifdAry);
            foreach (var ifd in ifdAry) {
                if (ifd.ExifPosition.HasValue) {
                    MakeIFD(ifd.ExifPosition.Value, "EXIFIFD");
                }
                if (ifd.GpsPosition.HasValue) {
                    MakeIFD(ifd.GpsPosition.Value, "GPSIFD");
                }
            }

            //因MakerNoteIFD只在ExifIFD中，所以獨立處理
            var exifIfd = IFDList.Where(i => i.Name == "EXIFIFD").FirstOrDefault();
            if (exifIfd != null) {
                if (exifIfd.MakerNotePosition.HasValue) {
                    MakeIFD(exifIfd.MakerNotePosition.Value, "MakerNote", true);
                }
            }
        }

        /// <summary>
        /// 產生IFD及其Entries
        /// </summary>
        /// <returns>下個IFD的Offset</returns>
        private long MakeIFD(long nextIFDPosition, string ifdName, bool isMakerNote = false) {
            IFD ifd = new IFD() { Name = ifdName };
            fs.Seek(nextIFDPosition + HeaderPosition, SeekOrigin.Begin);

            //設定元素數量
            var lenByte = new byte[2];
            fs.Read(lenByte, 0, lenByte.Length);
            ifd.EntryCount = Tools.Bin.BytesToShort(lenByte, IsLittleEndian);

            //處理每一筆元素
            for (int idx = 0; idx < ifd.EntryCount; idx++) {
                IFDEntry entry = new IFDEntry();
                entry.Position = fs.Position;
                fs.Read(entry.OriginalData, 0, Consts.IFD_ENTRY_LENGTH);
                entry.Tag = Tools.Bin.BytesToInt(Tools.Bin.ReadSubBytes(entry.OriginalData, 0, 2), IsLittleEndian);
                entry.Type = Tools.Bin.BytesToInt(Tools.Bin.ReadSubBytes(entry.OriginalData, 2, 2), IsLittleEndian);
                entry.Count = Tools.Bin.BytesToInt(Tools.Bin.ReadSubBytes(entry.OriginalData, 4, 4), IsLittleEndian);
                entry.ValueOffset = Tools.Bin.ReadSubBytes(entry.OriginalData, 8, 4);
                //判斷特殊Tag
                switch (entry.Tag) {
                    case Consts.EXIF_IFD_TAG:
                        ifd.ExifPosition = Tools.Bin.BytesToInt(entry.ValueOffset, IsLittleEndian);
                        break;
                    case Consts.GPS_IFD_TAG:
                        ifd.GpsPosition = Tools.Bin.BytesToInt(entry.ValueOffset, IsLittleEndian);
                        break;
                    case Consts.MAKER_NOTE_TAG:
                        ifd.MakerNotePosition = Tools.Bin.BytesToInt(entry.ValueOffset, IsLittleEndian);
                        break;
                    default:
                        ifd.IFDEntries.Add(entry);
                        break;
                }

            }

            //取得Next Offset
            fs.Read(ifd.NextIFDOffset, 0, 4);

            if (isMakerNote) {
                MakerNoteIFD = ifd;
            }
            else {
                IFDList.Add(ifd);
            }

            return Tools.Bin.BytesToInt(ifd.NextIFDOffset, IsLittleEndian);
        }

        /// <summary>
        /// 取得基本Tag名稱
        /// </summary>
        /// <param name="ifdName"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        private string GetTagName(string ifdName, int tag, string make = "", string model = "") {
            string res = string.Empty;
            var BaseIFDReg = new Regex(@"^IFD\d{1,2}$", RegexOptions.Singleline);
            if (BaseIFDReg.IsMatch(ifdName) || ifdName == "EXIFIFD") {
                res = Tools.TagName.GetIFDTagName(tag);
            }
            if (ifdName == "GPSIFD") {
                res = Tools.TagName.GetGPSTagName(tag);
            }
            return res;
        }
        #endregion

        #region ParseIFD相關
        /// <summary>
        /// 解析每個IFD Entry的值
        /// </summary>
        private void ParseIFD() {
            ParseBasicIFDName();
            ParseBasicIFDValue();
            ParseMakerNote();
        }

        /// <summary>
        /// 解析基本IFD的Tag名稱
        /// </summary>
        private void ParseBasicIFDName() {
            //設定Tag的ViewName
            var removeEntrys = new List<IFDEntry>();
            foreach (var ifd in IFDList) {
                foreach (var entry in ifd.IFDEntries) {
                    entry.ViewName = GetTagName(ifd.Name, entry.Tag);
                    if (entry.ViewName == string.Empty) {
                        removeEntrys.Add(entry);
                    }
                }
                //移除名稱是空白的Entry(不支援的Tag)
                IFDEntry[] ary = new IFDEntry[removeEntrys.Count];
                removeEntrys.CopyTo(ary);
                foreach (var entry in ary) {
                    ifd.IFDEntries.Remove(entry);
                }
            }
        }

        /// <summary>
        /// 處理基本IFD的值
        /// </summary>
        private void ParseBasicIFDValue() {
            foreach (var ifd in IFDList) {
                foreach (var entry in ifd.IFDEntries) {
                    entry.RealVal = Tools.Bin.ParseEntryRealVal(entry, fs, IsLittleEndian, HeaderPosition);
                    entry.ViewValue = Tools.Bin.GetEntryViewValue(entry);
                }
            }
        }

        /// <summary>
        /// 處理相機的MakerNote
        /// </summary>
        private void ParseMakerNote() {
            if (MakerNoteIFD == null) { return; }

            var removeEntrys = new List<IFDEntry>();
            var ifd0 = IFDList.Where(i => i.Name == "IFD0").FirstOrDefault();
            var model = ifd0.IFDEntries.Where(e => e.ViewName.ToLower() == "model").FirstOrDefault().ViewValue;
            var make = ifd0.IFDEntries.Where(e => e.ViewName.ToLower() == "make").FirstOrDefault().ViewValue;

            //動態取得MakerNote物件，現在只有Canon
            var makerNote = MakerNotes.MakerNote.GetMakerNote(make, model, fs);
            if (makerNote == null) { return; }

            makerNote.TempMakerNoteIFD = MakerNoteIFD;
            makerNote.fs = fs;
            makerNote.IsLittleEndian = IsLittleEndian;
            makerNote.HeaderPosition = HeaderPosition;
            makerNote.Run();

            IFDList.Add(makerNote.MakerNoteIFD);
        }
        #endregion

        #region 設定一般用Exif列表
        /// <summary>
        /// 將主要Exif放入列表內，空值則不放入
        /// </summary>
        private void AddExifList() {
            var ifdNameList = new List<string>() { "IFD0", "EXIFIFD", "MakerNote" };
            var lists = IFDList.Where(i => ifdNameList.Contains(i.Name)).ToList();
            foreach (var ifd in lists) {
                foreach (var entry in ifd.IFDEntries) {
                    if (string.IsNullOrWhiteSpace(entry.ViewValue)) { continue; }
                    if (entry.Tag == 0x111 || entry.Tag == 0x117) { continue; } //忽略StripOffsets & StripByteCounts
                    ExifList.Add(entry);
                }
            }
        }
        #endregion

        #region 設定縮圖
        /// <summary>
        /// 取得並設定Exif內的縮圖資料
        /// </summary>
        private void GetThumb() {
            var ifd = IFDList.Where(i => i.Name == "IFD1").FirstOrDefault();
            if (ifd == null) {
                UsingDrawingSetThumb();
                return;
            }
            if (IsJpegThumb(ifd)) { SetJpegThumb(ifd); }
            if (IsTiffThumb(ifd)) { SetTiffThumb(ifd); }
            if (Thumbnail == null) {
                UsingDrawingSetThumb();
            }

        }

        /// <summary>
        /// 判斷是否為Jpg格式縮圖
        /// </summary>
        /// <param name="ifd"></param>
        /// <returns></returns>
        private bool IsJpegThumb(IFD ifd) {
            if (ifd.IFDEntries.Where(e => e.ViewName == "JPEGInterchangeFormat").Count() != 0 &&
                ifd.IFDEntries.Where(e => e.ViewName == "JPEGInterchangeFormatLength").Count() != 0) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判斷是否為Tiff(無壓縮)格式縮圖
        /// </summary>
        /// <param name="ifd"></param>
        /// <returns></returns>
        private bool IsTiffThumb(IFD ifd) {
            if (ifd.IFDEntries.Where(e => e.ViewName == "StripOffsets").Count() != 0 &&
                ifd.IFDEntries.Where(e => e.ViewName == "StripByteCounts").Count() != 0) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 設定Jpeg格式縮圖實際資料
        /// </summary>
        /// <param name="ifd"></param>
        private void SetJpegThumb(IFD ifd) {
            var jpgOffset = ifd.IFDEntries.Where(e => e.ViewName == "JPEGInterchangeFormat").FirstOrDefault().ViewValue;
            var jpgLength = ifd.IFDEntries.Where(e => e.ViewName == "JPEGInterchangeFormatLength").FirstOrDefault().ViewValue;
            fs.Seek(long.Parse(jpgOffset) + HeaderPosition, SeekOrigin.Begin);
            byte[] thumbByte = new byte[int.Parse(jpgLength)];
            fs.Read(thumbByte, 0, int.Parse(jpgLength));
            try {
                using (MemoryStream ms = new MemoryStream(thumbByte)) {
                    Thumbnail = Image.FromStream(ms, true);
                    RotateThumb();
                }
            }
            catch {
                Thumbnail = null;
            }

        }

        /// <summary>
        /// 設定Tiff格式縮圖實際資料
        /// </summary>
        /// <param name="ifd"></param>
        private void SetTiffThumb(IFD ifd) {
            TiffThumbMaker thumbMaker = new TiffThumbMaker(TiffHeader, ifd, IsLittleEndian, GetRealStripData(ifd));
            try {
                using (MemoryStream ms = new MemoryStream(thumbMaker.ToBinary().Length)) {
                    ms.Write(thumbMaker.ToBinary(), 0, thumbMaker.ToBinary().Length);
                    Thumbnail = Image.FromStream(ms);
                    RotateThumb();
                }

            }
            catch { Thumbnail = null; }
            //BinaryTest = thumbMaker.ToBinary();
        }

        /// <summary>
        /// 取得每段Strip資料
        /// </summary>
        /// <param name="ifd"></param>
        /// <returns></returns>
        private List<byte[]> GetRealStripData(IFD ifd) {
            List<byte[]> byteDataList = new List<byte[]>();
            var byteOffsets = ifd.IFDEntries.Where(e => e.ViewName == "StripOffsets").FirstOrDefault().ViewValue;
            var byteCounts = ifd.IFDEntries.Where(e => e.ViewName == "StripByteCounts").FirstOrDefault().ViewValue;
            var offsetAry = byteOffsets.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var countAry = byteCounts.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            //取得每段strip實際資料
            int cindex = 0;
            //因StripOffsets不知為何是倒過來的，所以要從後往前讀取資料
            for (int index = offsetAry.Length - 1; index >= 0; index--) {
                long offset; int count; byte[] data;
                if (long.TryParse(offsetAry[index], out offset) && int.TryParse(countAry[cindex], out count)) {
                    fs.Seek(offset + HeaderPosition, SeekOrigin.Begin);
                    data = new byte[count];
                    fs.Read(data, 0, count);
                    byteDataList.Add(data);
                }
                cindex++;
            }
            return byteDataList;
        }

        /// <summary>
        /// 旋轉縮圖到正確方向
        /// </summary>
        private void RotateThumb() {
            var ifd0 = IFDList.Where(i => i.Name == "IFD0").FirstOrDefault();
            if (ifd0 == null) { return; }
            var rotate = ifd0.IFDEntries.Where(e => e.Tag == 0x112).FirstOrDefault();
            if (rotate == null) { return; }
            if (Thumbnail == null) { return; }
            try {
                int rvalue = ((int[])rotate.RealVal)[0];
                switch (rvalue) {
                    case 2: //水平翻轉
                        Thumbnail.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3: //旋轉180度
                        Thumbnail.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4: //垂直翻轉
                        Thumbnail.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        break;
                    case 5: //右上向左下翻轉
                        Thumbnail.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6: //向右旋轉90度
                        Thumbnail.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7: //左上向右下翻轉
                        Thumbnail.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8: //向左旋轉90度
                        Thumbnail.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    case 1:
                    default:
                        return;
                }
            }
            catch { }
        }

        private void UsingDrawingSetThumb() {
            Bitmap img = new Bitmap(fs);
            Thumbnail = DrawingGetThumb.GetThumb(img, 300, 200);
        }
        #endregion
    }
}
