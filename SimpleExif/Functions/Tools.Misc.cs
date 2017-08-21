using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExif.Functions {
    internal partial class Tools {
        /// <summary>
        /// Exif雜類函數
        /// </summary>
        public class Misc {
            /// <summary>
            /// 判斷檔案是否為Jpeg檔(有SOI標頭)
            /// </summary>
            /// <param name="fs"></param>
            /// <param name="fsStartPos"></param>
            /// <returns></returns>
            public static bool IsJpeg(FileStream fs, long fsStartPos) {
                var _byte = new byte[2];
                fs.Seek(fsStartPos, SeekOrigin.Begin);
                fs.Read(_byte, 0, 2);
                if (_byte[0] != Consts.MAKER_PREFIX || _byte[1] != Consts.SOI) {
                    //不是Jpeg檔則游標退回兩位
                    fs.Seek(fs.Position - 2, SeekOrigin.Begin);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// 取得並跳過APP0
            /// </summary>
            /// <param name="fs"></param>
            /// <param name="fsStartPos"></param>
            /// <returns></returns>
            public static void IgnoreAPP0(FileStream fs, long fsStartPos) {
                var _marker = new byte[2];
                fs.Seek(fsStartPos, SeekOrigin.Begin);
                fs.Read(_marker, 0, 2);
                //判斷兩個byte是否等於APP0的Marker(0xFF, 0xE0)
                if (_marker[0] != Consts.MAKER_PREFIX || _marker[1] != Consts.APP0) {
                    fs.Seek(fs.Position - 2, SeekOrigin.Begin); //不是的話就返回原有的位置
                    return;
                }

                //取得2bytes的APP0長度
                fs.Seek(fs.Position, SeekOrigin.Begin);
                var _len = new byte[2];
                fs.Read(_len, 0, 2);
                //APP0段長度，左移8位表示若原本為8C(1000 1100)，左移後變成8C00(1000 1100 0000 0000)，這樣再加上低位數len[1]就是正確數字
                var length = (_len[0] << 8) + _len[1];

                //將fs指標移到APP0後，因長度包含前2bytes的長度區塊，所以要-2
                fs.Seek(fs.Position + (length - 2), SeekOrigin.Begin);
            }

            /// <summary>
            /// 取得APEX格式的光圈值
            /// </summary>
            /// <param name="a">分子</param>
            /// <param name="b">分母</param>
            /// <returns></returns>
            public static string GetAPEXFNumber(int a, int b) {
                return GetAPEXFNumber((double)a, (double)b);
            }
            public static string GetAPEXFNumber(long a, long b) {
                return GetAPEXFNumber((double)a, (double)b);
            }
            public static string GetAPEXFNumber(double a, double b) {
                /*
                 * 如果值是2970854/1000000，算出來是2.970854，
                 * 然後用算出根號2的該次方(根號二用1.4142來算)，
                 * 也就是1.4142^2.970854，等於2.8
                 */
                string ret;
                ret = string.Format("{0:0.0}", Math.Pow(Math.Sqrt(2), (a / b)));
                if (ret.Substring(ret.Length - 2, 2) == ".0") { ret = ret.Split(new char[1] { '.' })[0]; }
                return ret;
            }

            /// <summary>
            /// 取得APEX格式的快門速度
            /// </summary>
            /// <param name="a">分子</param>
            /// <param name="b">分母</param>
            /// <returns></returns>
            public static string GetAPEXExposureTime(int a, int b) {
                return GetAPEXExposureTime((double)a, (double)b);
            }
            public static string GetAPEXExposureTime(long a, long b) {
                return GetAPEXExposureTime((double)a, (double)b);
            }
            public static string GetAPEXExposureTime(double a, double b) {
                /*
                 * 如果值是12287712/1000000，算出來是12.287712，
                 * 然後算出2的該次方，也就是 2^12.287712，
                 * 再倒數，最後就是 1/(2^12.287712)也就是1/5000
                 */
                if (a < 0 || b < 0) {
                    return string.Format("{0:0}", (double)1 / Math.Pow((double)2, (a / b)));
                }
                else {
                    return "1/" + ((int)Math.Pow((double)2, (a / b))).ToString();
                }
            }

        }
    }
}
