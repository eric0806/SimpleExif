using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExif.Models {
    /// <summary>
    /// TiffHeader資料結構
    /// </summary>
    internal class TiffHeader {
        public TiffHeader() {
            ByteOrder = new byte[2];
            TiffID = new byte[2];
            NextIFDOffset = new byte[4];
        }
        public byte[] ByteOrder { get; set; }
        public byte[] TiffID { get; set; }
        public byte[] NextIFDOffset { get; set; }
        public long NextIFDOffsetValue { get; set; }
    }

    /// <summary>
    /// IFD區塊資料結構
    /// </summary>
    public class IFD {
        /// <summary>
        /// IFD名稱(如IFD0 ExifIFD GpsIFD IFD11)
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 元素數量
        /// </summary>
        public short EntryCount { get; internal set; }

        /// <summary>
        /// 元素List
        /// </summary>
        public List<IFDEntry> IFDEntries { get; internal set; } //每一個12bytes的IFD資料

        /// <summary>
        /// Exif IFD距離Header的Offset
        /// </summary>
        internal long? ExifPosition { get; set; }

        /// <summary>
        /// GPS IFD距離Header的Offset
        /// </summary>
        internal long? GpsPosition { get; set; }

        /// <summary>
        /// MakerNote IFD距離Header的Offset
        /// </summary>
        internal long? MakerNotePosition { get; set; }

        /// <summary>
        /// 下個IFD的偏移量，如果為0表示已經是最後一個IFD
        /// </summary>
        public byte[] NextIFDOffset { get; internal set; }

        public IFD() {
            IFDEntries = new List<IFDEntry>();
            NextIFDOffset = new byte[4];
            ExifPosition = null;
            GpsPosition = null;
            MakerNotePosition = null;
        }
    }

    /// <summary>
    /// IFD Entry資料結構
    /// </summary>
    public class IFDEntry {
        public IFDEntry() {
            OriginalData = new byte[Consts.IFD_ENTRY_LENGTH];
            ValueOffset = new byte[4];
            RealValInBlock = false;
        }

        /// <summary>
        /// 這個Entry在整個檔案內的位址(絕對位址)
        /// </summary>
        internal long Position { get; set; }

        /// <summary>
        /// 原始二進位資料
        /// </summary>
        public byte[] OriginalData { get; internal set; }

        /// <summary>
        /// Tag數值
        /// </summary>
        public int Tag { get; internal set; }

        /// <summary>
        /// Type數值
        /// </summary>
        public int Type { get; internal set; }

        /// <summary>
        /// Count數值
        /// </summary>
        public int Count { get; internal set; }

        /// <summary>
        /// Value or Offset二進位資料
        /// </summary>
        public byte[] ValueOffset { get; internal set; }

        /// <summary>
        /// 存放實際值的原始二進位byte[]
        /// </summary>
        public byte[] RealValAry { get; internal set; }

        /// <summary>
        /// 指出RealVal是否存放在Value Block內
        /// </summary>
        public bool RealValInBlock { get; internal set; }

        /// <summary>
        /// 處理後的實際值
        /// </summary>
        internal object RealVal { get; set; }

        /// <summary>
        /// 顯示用的值
        /// </summary>
        public string ViewValue { get; internal set; }

        /// <summary>
        /// 顯示用的名稱
        /// </summary>
        public string ViewName { get; internal set; }

        /// <summary>
        /// 產生"名稱: 值"的字串
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return string.Format("{0}: {1}", ViewName, ViewValue);
        }
    }


    internal class StripEntry : IFDEntry {
        public StripEntry()
            : base() {
            //StripOffsetList = new List<Strip>();
        }

        public List<Strip> StripOffsetList { get; set; }
    }

    internal class Strip {
        public int Position { get; set; }
        public int Value { get; set; }
    }
}
