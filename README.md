# SimpleExif

²��Ū�����ɤ�EXIF���u��A�䴩Tiff(�۾�RAW��)�BJpg�榡�A�iŪ�X��Exif�BGPS Exif�A�䴩MakerNote(�۾��t�Ӧۤv��Exif)�C

## MakerNote�䴩�۾��C��
1. Canon

## �Ϊk

    using(var exif = new SimpleExif.exif(@"x:\File\Path.jpg"){
        if (!exif.ExifExists) {
            return;
        }
        
        //Ū����Exif
        exif.ExifList.ForEach(e => Console.WriteLine($"{e.ViewName}-> {e.ViewValue}"));

        //�ھڨC��IFD����Ū���ԲӸ��
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

    