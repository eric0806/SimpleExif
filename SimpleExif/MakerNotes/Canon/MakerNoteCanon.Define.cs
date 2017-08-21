using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleExif.MakerNotes.Canon
{
    internal partial class MakerNoteCanon : MakerNote
    {
        List<int> RemoveTagList = new List<int>(){
            0x0019,
            0x00AA,
            0x4009,
            0x4011,
            0x4012,
            0x4015,
            0x4016,
            0x4020
            /*
            0x0003,
            0x0094,
            0x00a1,
            0x00a2,
            0x00a3,
            0x00a4,
            0x4002,
            0x4005
            */
        };
    }
}
