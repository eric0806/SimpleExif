# SimpleExif

簡易讀取圖檔內EXIF的工具，支援Tiff(相機RAW檔)、Jpg格式，可讀出基本Exif、GPS Exif，支援MakerNote(相機廠商自己的Exif)。

## MakerNote支援相機列表
1. Canon

## 用法

    using(var exif = new SimpleExif.exif(@"x:\File\Path.jpg"){
        if (!exif.ExifExists) {
            return;
        }
        
        //讀取基本Exif
        exif.ExifList.ForEach(e => Console.WriteLine($"{e.ViewName}-> {e.ViewValue}"));

        //根據每個IFD分組讀取詳細資料
        foreach(var ifd in exif.IFDList){
            Console.WriteLine($"IFD: {ifd.Name}");
            ifd.IFDEntries.ForEach(e => Console.WriteLine($"{e.ViewName}\t{e.Type}\t{e.Count}\t{e.ViewValue}\t{BinaryToString(e.RealValAry)}"))
            Console.WriteLine();
        }
    }

    public string BinaryToString(byte[] data) {
        if (data == null) { return string.Empty; }
        var res = new StringBuilder();
        for (int i = 0; i < data.Length; i++) {
            res.Append(string.Format("{0:X2} ", data[i]));
        }
        return res.ToString().Trim();
    }

    