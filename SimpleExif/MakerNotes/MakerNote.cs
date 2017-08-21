using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleExif.MakerNotes.Canon;
using SimpleExif.Models;

namespace SimpleExif.MakerNotes {
    /// <summary>
    /// 一個抽象的類別，指定MakerNote相關
    /// </summary>
    internal abstract class MakerNote {
        /// <summary>
        /// 暫存的MakerNoteIFD，內部處理用
        /// </summary>
        public IFD TempMakerNoteIFD { get; set; }
        /// <summary>
        /// 真正要回傳的MakerNoteIFD
        /// </summary>
        public IFD MakerNoteIFD { get; set; }
        public bool IsLittleEndian { get; set; }
        public long HeaderPosition { get; set; }

        internal FileStream fs = null;

        public static MakerNote GetMakerNote(string make, string model, FileStream fs) {
            switch (make.ToLower()) {
                case "canon":
                    return new MakerNoteCanon(fs);
                default:
                    return null;
            }
        }

        public MakerNote() {
            MakerNoteIFD = new IFD() { Name = "MakerNote" };
        }

        /// <summary>
        /// 每個繼承此類別的子類別都必須實作Run，以處理MakerNote
        /// </summary>
        public abstract void Run();


    }
}
