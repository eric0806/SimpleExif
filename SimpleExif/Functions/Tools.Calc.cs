using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExif.Functions {
    internal partial class Tools {
        /// <summary>
        /// 各種計算器
        /// </summary>
        public class Calc {
            /// <summary>
            /// int32轉ushort(unsigned)
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static ushort IntToUShort(int value) {
                return ushort.Parse(string.Format("{0:X}", value), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            /// <summary>
            /// int32轉short(signed)
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static short IntToShort(int value) {
                return short.Parse(string.Format("{0:X}", value), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            /// <summary>
            /// int32轉ulong(unsigned)
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static ulong IntToULong(int value) {
                return ulong.Parse(string.Format("{0:X}", value), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            /// <summary>
            /// int32轉long(signed)
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static long IntToLong(int value) {
                return long.Parse(string.Format("{0:X}", value), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            /// <summary>
            /// 浮點數轉x/y形式的分數
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static string FloatToFraction(double a) {
                /*
                 * 思考: xx.xx的分數，就是 xx.xx/1
                 * 然後分子分母依據分子的小數位數乘以正確的10的次方
                 * 最後分子分母做約分即可
                 */
                double top; //分子
                double bottom; //分母
                bottom = 1 * Math.Pow(10, DotNumber(a));
                top = a * bottom;
                long cd = Gcd((long)top, (long)bottom);
                top = top / cd;
                bottom = bottom / cd;
                return top.ToString() + "/" + bottom.ToString();
            }

            /// <summary>
            /// 取得兩數最大公約數
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static long Gcd(long a, long b) {
                if (0 == b) return a;
                return Gcd(b, a % b);
            }


            /// <summary>
            /// 取得小數點位數
            /// </summary>
            /// <param name="a"></param>
            /// <returns></returns>
            private static int DotNumber(double a) {
                int offset = a.ToString().IndexOf(".");
                return a.ToString().Substring(offset + 1, a.ToString().Length - offset - 1).Length;
            }

        }
    }
}
