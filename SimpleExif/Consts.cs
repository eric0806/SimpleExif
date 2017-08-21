using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExif {
    /// <summary>
    /// 定義常數
    /// </summary>
    internal static class Consts {
        #region 長度相關
        /// <summary>
        /// Tiff Header長度
        /// </summary>
        internal const int TIFF_HEADER_LENGTH = 8;
        /// <summary>
        /// IFD Entry長度
        /// </summary>
        internal const int IFD_ENTRY_LENGTH = 12;
        /// <summary>
        /// Type為Byte的長度
        /// </summary>
        internal const int LEN_BYTE = 1;
        /// <summary>
        /// Type為Short的長度
        /// </summary>
        internal const int LEN_SHORT = 2;
        /// <summary>
        /// Type為Long的長度
        /// </summary>
        internal const int LEN_LONG = 4;
        /// <summary>
        /// Type為Rational的長度
        /// </summary>
        internal const int LEN_RATIONAL = 8;
        #endregion

        #region 固定的特定值
        /// <summary>
        /// Jpeg用，每個區段的起始標記
        /// </summary>
        internal const int MAKER_PREFIX = 0xFF;
        /// <summary>
        /// Jpeg用，標記為Jpeg檔案
        /// </summary>
        internal const int SOI = 0xD8;
        /// <summary>
        /// Jpeg用，標記為APP0區段開始
        /// </summary>
        internal const int APP0 = 0xE0;
        /// <summary>
        /// Jpeg用，標記為APP1區段起始
        /// </summary>
        internal const int APP1 = 0xE1;
        /// <summary>
        /// TIFF ID
        /// </summary>
        internal const int TIFF_ID = 0x002A;
        /// <summary>
        /// ExifIFD的Tag
        /// </summary>
        internal const int EXIF_IFD_TAG = 0x8769;
        /// <summary>
        /// GpsIFD的Tag
        /// </summary>
        internal const int GPS_IFD_TAG = 0x8825;
        /// <summary>
        /// Maker Note IFD的Tag
        /// </summary>
        internal const int MAKER_NOTE_TAG = 0x927C;
        /// <summary>
        /// 順序標籤(反向)
        /// </summary>
        internal const int II = 0x4949;
        #endregion

    }
    #region Type相關
    /// <summary>
    /// IFD Entry Type的型別
    /// </summary>
    internal enum EntryType : int {
        BYTE = 1,
        ASCII = 2,
        SHORT = 3,
        LONG = 4,
        RATIONAL = 5,
        UNDEFINED = 7,
        SLONG = 9,
        SRATIONAL = 10
    };
    #endregion
}
