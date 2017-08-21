using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExif {
    /// <summary>
    /// APP1的模型
    /// </summary>
    internal class App1 {

        public static App1 Create(FileStream fs) {
            var app1 = new App1 {
                App1Marker = new byte[2],
                App1Length = new byte[2],
                App1IdnCode = new byte[6],
                StartPosition = fs.Position
            };
            fs.Read(app1.App1Marker, 0, 2);
            fs.Read(app1.App1Length, 0, 2);
            fs.Read(app1.App1IdnCode, 0, 6);

            //判斷是否為App1Marker
            if (app1.App1Marker[0] != Consts.MAKER_PREFIX || app1.App1Marker[1] != Consts.APP1) {
                app1.TiffExifModel = null;

            }
            else {
                app1.TiffExifModel = new TiffExif(fs);
                app1.TiffExifModel.IsJpeg = true;
            }
            return app1;
        }
        
        /// <summary>
        /// App1的絕對位址
        /// </summary>
        public long StartPosition { get; private set; }
        public byte[] App1Marker { get; set; }
        public byte[] App1Length { get; set; }
        public byte[] App1IdnCode { get; set; }
        public TiffExif TiffExifModel { get; set; }

    }
}
