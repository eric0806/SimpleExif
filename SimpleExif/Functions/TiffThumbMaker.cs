using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleExif.Models;

namespace SimpleExif.Functions {
    /// <summary>
    /// 產生未壓縮格式的縮圖類別，依照輸入的Header與IFD產生二進位資料
    /// </summary>
    internal class TiffThumbMaker {
        private bool isLittleEndian;
        private IFD ifd;
        private List<byte> bytes;
        private TiffHeader tiffHeader;
        private List<StripEntry> specialEntries;

        public List<byte[]> StripDataList { get; set; }

        /// <summary>
        /// 紀錄現在陣列的index
        /// </summary>
        private int index;

        public TiffThumbMaker(TiffHeader header, IFD ifd, bool isLittleEndian = true, List<byte[]> stripDataList = null) {
            tiffHeader = header;
            this.ifd = ifd;
            this.isLittleEndian = isLittleEndian;
            bytes = new List<byte>();
            specialEntries = new List<StripEntry>();
            StripDataList = stripDataList;
            index = 0;

            MakeHeader();
            MakeIFDBinary();
        }

        /// <summary>
        /// 產生TiffHeader(IFD Offset為8)
        /// </summary>
        private void MakeHeader() {
            bytes.AddRange(tiffHeader.ByteOrder.AsEnumerable());
            bytes.AddRange(tiffHeader.TiffID.AsEnumerable());
            bytes.AddRange(Tools.Bin.IntToByte(8, isLittleEndian).AsEnumerable());
            index += 8;
        }

        /// <summary>
        /// 開始處理IFD
        /// </summary>
        private void MakeIFDBinary() {
            MakeIFDLength();
            MakeIFDEntryBinary();
            MakeSpecialBinary();
        }

        /// <summary>
        /// 產生IFD Length資料
        /// </summary>
        private void MakeIFDLength() {
            bytes.AddRange(Tools.Bin.ShortToByte((short)ifd.EntryCount, isLittleEndian).AsEnumerable());
            index += 2;
        }

        /// <summary>
        /// 先初始二進位資料，其中特殊Tag之後要再寫入實際offset值
        /// </summary>
        private void MakeIFDEntryBinary() {
            foreach (var entry in ifd.IFDEntries) {
                //如果值不是offset則直接加入
                if (!entry.RealValInBlock && entry.ViewName != "StripOffsets") {
                    //可直接加入的
                    bytes.AddRange(entry.OriginalData.AsEnumerable());
                }
                //如果是，則新增一個entry，先放不含valueoffset的值(XXXXXXXX0000)
                else {
                    StripEntry newEntry;
                    if (entry.Tag == 0x111) {
                        /*
                         * 需要特殊處理的：
                         * StripOffsets
                         * valueoffset放strip實際開始放的位置，應該就是所有處理完後，index的值
                         */
                        newEntry = MakeSpecialEntry(entry.Tag, entry.Type, entry.Count, null, "StripOffsets");
                    }
                    else {
                        newEntry = MakeSpecialEntry(entry.Tag, entry.Type, entry.Count, entry.RealValAry);
                    }
                    specialEntries.Add(newEntry);
                    bytes.AddRange(newEntry.OriginalData.AsEnumerable());
                }

                index += Consts.IFD_ENTRY_LENGTH;
            }
        }

        /// <summary>
        /// 產生一個特殊的Entry
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="type"></param>
        /// <param name="count"></param>
        /// <param name="realValAry"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        private StripEntry MakeSpecialEntry(int tag, int type, int count, byte[] realValAry = null, string viewName = "") {
            StripEntry entry = new StripEntry() {
                Tag = tag,
                Type = type,
                Count = count,
                Position = index + 8,
                ValueOffset = new byte[4],
                OriginalData = MakeSpecialOriginalData(tag, type, count, new byte[4])
            };
            if (realValAry != null) {
                entry.RealValAry = realValAry;
            }
            if (viewName != "") {
                entry.ViewName = viewName;
            }
            return entry;
        }

        /// <summary>
        /// 處理特殊Entry
        /// </summary>
        private void MakeSpecialBinary() {
            foreach (var entry in specialEntries) {
                PutValueToBlockAndSetOffset(entry);
            }

            //跑完後index位址就是StripOffset的值
            StripEntry stripOffsetEntry = specialEntries.Where(e => e.ViewName == "StripOffsets").FirstOrDefault();
            if (stripOffsetEntry == null) { return; }
            PutStripOffsetsValueToBlockAndSetOffset(stripOffsetEntry);


            //插入最後StripData
            InsertStripData(stripOffsetEntry);

            //修改實際stripOffsets的值
            ModStripDataOffset(stripOffsetEntry);
        }

        /// <summary>
        /// 將value是存放在獨立區塊的entry做處理，存放block並設定offset
        /// </summary>
        private void PutValueToBlockAndSetOffset(StripEntry entry) {
            if (entry.ViewName == "StripOffsets") { return; } //StripOffsets先跳過，等最後再處理
            //value實際存放位置就是目前的index
            int offset = index;
            //放入data
            bytes.AddRange(entry.RealValAry.AsEnumerable());
            //結尾的空白位址所在
            index += entry.RealValAry.Length;
            //修改entry
            //將位址轉換為byte[]
            byte[] newvalue = Tools.Bin.IntToByte(offset, isLittleEndian);
            //移除原本byte內該位址所在的值(4bytes)
            bytes.RemoveRange((int)entry.Position, 4);
            //插入新的值
            bytes.InsertRange((int)entry.Position, newvalue.AsEnumerable());
        }

        /// <summary>
        /// 將StripOffsets內的值放到Value區塊內，並設定offset
        /// </summary>
        /// <param name="entry"></param>
        private void PutStripOffsetsValueToBlockAndSetOffset(StripEntry entry) {
            //取得Offset的二進位陣列(就是目前的index值)
            byte[] value = Tools.Bin.IntToByte(index, isLittleEndian);
            //移除原本valueoffset的值(4bytes 00)
            bytes.RemoveRange((int)entry.Position, 4);
            //插入新的值(目前位址)
            bytes.InsertRange((int)entry.Position, value.AsEnumerable());

            //這邊再來先放都是0的offsets陣列
            entry.StripOffsetList = new List<Strip>();
            for (int i = 0; i < entry.Count; i++) {
                entry.StripOffsetList.Add(new Strip() {
                    Position = index,
                    Value = 0
                });
                bytes.AddRange(Tools.Bin.IntToByte(0, isLittleEndian));
                index += 4;
            }
        }

        /// <summary>
        /// 將StripData放入Block內，並將修改StripOffsets的ValueBlock位址值
        /// </summary>
        /// <param name="entry"></param>
        private void InsertStripData(StripEntry entry) {
            int sindex = 0;
            foreach (var stripData in StripDataList) {
                //加入Data
                bytes.AddRange(stripData.AsEnumerable());
                //修改stripOffsets[sindex]的值
                entry.StripOffsetList[sindex].Value = index;
                sindex++;
                index += stripData.Length;
            }
        }

        /// <summary>
        /// 修改StripOffsets的ValueBlock內的值
        /// </summary>
        /// <param name="entry"></param>
        private void ModStripDataOffset(StripEntry entry) {
            foreach (var strip in entry.StripOffsetList) {
                bytes.RemoveRange(strip.Position, 4);
                bytes.InsertRange(strip.Position, Tools.Bin.IntToByte(strip.Value, isLittleEndian));
            }

        }


        /// <summary>
        /// 產生空白值的IFDEntry二進位陣列
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="type"></param>
        /// <param name="count"></param>
        /// <param name="valueoffset"></param>
        /// <returns></returns>
        private byte[] MakeSpecialOriginalData(int tag, int type, int count, byte[] valueoffset) {
            List<byte> tmp = new List<byte>();
            tmp.AddRange(Tools.Bin.ShortToByte((short)tag, isLittleEndian).AsEnumerable());
            tmp.AddRange(Tools.Bin.ShortToByte((short)type, isLittleEndian).AsEnumerable());
            tmp.AddRange(Tools.Bin.IntToByte(count, isLittleEndian).AsEnumerable());
            tmp.AddRange(valueoffset.AsEnumerable());
            return tmp.ToArray();
        }

        /// <summary>
        /// 回傳處理過後的縮圖資料
        /// </summary>
        /// <returns></returns>
        public byte[] ToBinary() {
            return bytes.ToArray();
        }
    }
}
