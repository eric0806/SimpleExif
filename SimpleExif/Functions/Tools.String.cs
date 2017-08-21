using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleExif.Functions {
    internal partial class Tools {
        /// <summary>
        /// Exif字串相關函數
        /// </summary>
        public class String {
            /// <summary>
            /// 輸出byte陣列的原始二進位資料字串
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public static string BinaryToString(byte[] data) {
                var res = new StringBuilder();
                for (int i = 0; i < data.Length; i++) {
                    res.Append(string.Format("{0:X2} ", data[i]));
                }
                return res.ToString().Trim();
            }

            /// <summary>
            /// 處理Exif格式日期，Exif日期格式是yyyy:mm:dd hh:mm:ss，要將日期的:改為-
            /// </summary>
            /// <param name="ExifDate"></param>
            /// <returns></returns>
            public static DateTime ExifDateToDateTime(string ExifDate) {
                DateTime newDate;
                Regex DateReg;
                ExifDate = ExifDate.Trim();
                if (ExifDate.Contains(' ')) {
                    DateReg = new Regex(@"^(?<year>\d{4}).{1}(?<month>\d{1,2}).{1}(?<day>\d{1,2}) (?<hour>\d{1,2}).{1}(?<minute>\d{1,2}).{1}(?<second>\d{1,2})", RegexOptions.Singleline);
                    var match = DateReg.Match(ExifDate);

                    var datestr = string.Format("{0}-{1}-{2} {3}:{4}:{5}",
                        match.Groups["year"].Value,
                        match.Groups["month"].Value,
                        match.Groups["day"].Value,
                        match.Groups["hour"].Value,
                        match.Groups["minute"].Value,
                        match.Groups["second"].Value
                        );
                    newDate = DateTime.Parse(datestr);
                }
                else {
                    DateReg = new Regex(@"^(?<year>\d{4}).{1}(?<month>\d{1,2}).{1}(?<day>\d{1,2})", RegexOptions.Singleline);
                    var match = DateReg.Match(ExifDate);

                    var datestr = string.Format("{0}-{1}-{2}",
                        match.Groups["year"].Value,
                        match.Groups["month"].Value,
                        match.Groups["day"].Value
                        );
                    newDate = DateTime.Parse(datestr);
                }
                return newDate;
            }

            /// <summary>
            /// 將Exif格式的日期時間字串轉成正常的日期時間字串(yyyy-MM-dd HH:mm:ss)
            /// </summary>
            /// <param name="ExifDate"></param>
            /// <returns></returns>
            public static string ExifDateToDateTimeString(string ExifDate) {
                return ExifDateToDateTime(ExifDate).ToString("yyyy-MM-dd HH:mm:ss");
            }

            #region 字串函數
            /// <summary>
            /// 取得系統編碼下，字元真實長度
            /// </summary>
            /// <param name="InChar"></param>
            /// <returns></returns>
            public static int GetCharCount(string InChar) {
                var enc = Encoding.Default;
                return enc.GetByteCount(InChar);
            }

            /// <summary>
            /// 取得字串真實長度，全形字長度為2
            /// </summary>
            /// <param name="InText">原始輸入字串</param>
            /// <returns></returns>
            public static int GetLength(string InText) {
                int Count = 0;
                for (int i = 0; i < InText.Length; i++) {
                    Count += GetCharCount(InText.Substring(i, 1));
                }
                return Count;
            }

            /// <summary>
            /// 將字串左邊填滿指定位數的固定字元
            /// </summary>
            /// <param name="InText">原始輸入字串</param>
            /// <param name="FillCount">要填滿的長度</param>
            /// <param name="FillStr">要填滿的字元</param>
            /// <returns></returns>
            public static string FillChar(string InText, int FillCount, string FillStr) {
                int Len = GetLength(InText);
                if (Len < FillCount) {
                    for (int i = 0; i < FillCount - Len; i++) {
                        InText = FillStr + InText;
                    }
                }
                return InText;
            }
            #endregion

        }
    }
}
