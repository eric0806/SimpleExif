using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleExif.MakerNotes.Canon
{
    internal partial class MakerNoteCanon : MakerNote
    {
        /// <summary>
        /// 取得白平衡名稱
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetWhiteBalanceName(short value) {
            string ret = string.Empty;
            switch (value) {
                case 0: ret = "Auto"; break;
                case 1: ret = "Daylight"; break;
                case 2: ret = "Cloudy"; break;
                case 3: ret = "Tungsten"; break;
                case 4: ret = "Fluorescent"; break;
                case 5: ret = "Flash"; break;
                case 6: ret = "Custom"; break;
                case 7: ret = "Black & White"; break;
                case 8: ret = "Shade"; break;
                case 9: ret = "Manual Temperature (Kelvin)"; break;
                case 10: ret = "PC SET1"; break;
                case 11: ret = "PC SET2"; break;
                case 12: ret = "PC SET3"; break;
                case 14: ret = "Daylight Fluorescent"; break;
                case 15: ret = "Custom 1"; break;
                case 16: ret = "Custom 2"; break;
                case 17: ret = "Underwater"; break;
                case 18: ret = "Custom 3"; break;
                case 19: ret = "Custom 4"; break;
                case 20: ret = "PC SET4"; break;
                case 21: ret = "PC SET5"; break;
            }
            return ret;
        }

        /// <summary>
        /// 取得白平衡名稱
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetWhiteBalanceName(ushort value) {
            string ret = GetWhiteBalanceName((short)value);
            if (ret == string.Empty) {
                switch (value) {
                    case 0x8000: ret = "Custom, Auto"; break;
                    case 0x8001: ret = "Custom, Daylight"; break;
                    case 0x8002: ret = "Custom, Cloudy"; break;
                    case 0x8003: ret = "Custom, Tungsten"; break;
                    case 0x8004: ret = "Custom, Fluorescent"; break;
                    case 0x8005: ret = "Custom, Flash"; break;
                    case 0x8006: ret = "Custom, Custom"; break;
                    case 0x8007: ret = "Custom, Black & White"; break;
                    case 0x8008: ret = "Custom, Shade"; break;
                    case 0x8009: ret = "Custom, Manual temperature"; break;
                    case 0x800a: ret = "Custom, PC Set 1"; break;
                    case 0x800b: ret = "Custom, PC Set 2"; break;
                    case 0x800c: ret = "Custom, PC Set 3"; break;
                    case 0x800e: ret = "Custom, Daylight Fluorescent"; break;
                    case 0x800f: ret = "Custom, Custom 1"; break;
                    case 0x8010: ret = "Custom, Custom 2"; break;
                    case 0x8011: ret = "Custom, Underwater"; break;
                    case 0x8012: ret = "Custom, Custom 3"; break;
                    case 0x8013: ret = "Custom, Custom 4"; break;
                    case 0x8014: ret = "Custom, PC Set 4"; break;
                    case 0x8015: ret = "Custom, PC Set 5"; break;
                    case 0xffff: ret = "n/a"; break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 取得PictureStyle名稱
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetPictureStyleName(short value) {
            string ret = string.Empty;
            switch (value) {
                case 0x0: ret = "None"; break;
                case 0x1: ret = "Standard"; break;
                case 0x2: ret = "Portrait"; break;
                case 0x3: ret = "High Saturation"; break;
                case 0x4: ret = "Adobe RGB"; break;
                case 0x5: ret = "Low Saturation"; break;
                case 0x6: ret = "CM Set 1"; break;
                case 0x7: ret = "CM Set 2"; break;
                case 0x21: ret = "User Def. 1"; break;
                case 0x22: ret = "User Def. 2"; break;
                case 0x23: ret = "User Def. 3"; break;
                case 0x41: ret = "PC 1"; break;
                case 0x42: ret = "PC 2"; break;
                case 0x43: ret = "PC 3"; break;
                case 0x81: ret = "Standard"; break;
                case 0x82: ret = "Portrait"; break;
                case 0x83: ret = "Landscape"; break;
                case 0x84: ret = "Neutral"; break;
                case 0x85: ret = "Faithful"; break;
                case 0x86: ret = "Monochrome"; break;
                case 0x87: ret = "Auto"; break;
            }
            return ret;
        }
    }
}
