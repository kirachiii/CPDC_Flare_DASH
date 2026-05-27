using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CPDC_Flare_DASH.Models
{
    class ReadFile
    {
        public void ReadFileInitial(string Address, string Type)
        {
            try
            {

                FileTimeInfo file = GetLatestFileTimeInfo(Address, Type);

                if (file.FileName.Length > 0)
                {
                    if (Type == "VOC" && GlobalVariable.CPDCInfo.GC_File_VOC_Name == file.FileName)
                        return;
                    else if (Type == "VOC")
                    {
                        GlobalVariable.CPDCInfo.GC_File_VOC_Name = file.FileName;
                        GlobalVariable.CPDCInfo.GC_File_Status = true;
                    }

                    if (Type == "THC" && GlobalVariable.CPDCInfo.GC_File_THC_Name == file.FileName)
                        return;
                    else if (Type == "THC")
                    {
                        GlobalVariable.CPDCInfo.GC_File_THC_Name = file.FileName;
                        GlobalVariable.CPDCInfo.GC_File_Status = true;
                    }

                    if (InitialFileControl.FileRead("File", Type + "Item", "null") != "")
                    {
                        var tmp = Address + @"\" + file.FileName;
                        var tmp1 = InitialFileControl.FileRead("File", Type + "Item", "null");
                        ReadFileData(Address + @"\" + file.FileName, InitialFileControl.FileRead("File", Type + "Item", "null"));
                    }

                }
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("GC-讀取檔案 失敗", 99);
            }

        }

        public void ReadFileData(string FileAddress, string Item)//傳入1.檔案位置 2.檢查項目名稱
        {
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(FileAddress))
                {
                    string Item_NA = "";
                    int Vocindex = 0;

                    // Read the stream to a string, and write the string to the console.
                    String[] VocArray = sr.ReadToEnd().Replace("\r\n", "").Split('\t');//將多餘字元去除,依空白切割
                    VocArray = VocArray.Where(s => !string.IsNullOrEmpty(s)).ToArray();//將空白陣列去除

                    do
                    {
                        foreach (string item in Item.Split(','))
                        {
                            int index = -1;
                            if(item == "C3H6" || item == "HCN" || item == "ACN" || item == "AN")
                            {
                                index = Array.LastIndexOf(VocArray, item);
                            }
                            else
                            {
                                index = Array.IndexOf(VocArray, item, Vocindex);
                            }
                            //int index = Array.IndexOf(VocArray, item, Vocindex);
                            if (VocArray[index + 2] != "n.a." && index != -1)
                            {
                                DataColletionSystem.SetControlInfo("lbl" + item + "_PPM", Convert.ToDouble(VocArray[index + 2]).ToString("#0.00"));
                            }
                            else
                            {
                                if (Vocindex < Array.IndexOf(VocArray, "Peak", Vocindex))
                                    Item_NA += item + ",";
                                else
                                {
                                    DataColletionSystem.SetControlInfo("lbl" + item + "_PPM", "0.00");
                                }
                            }
                        }
                        Vocindex += Array.IndexOf(VocArray, "Peak", Vocindex);
                        if (Item_NA != "")
                            Item = Item_NA.Substring(0, Item_NA.Length - 1);
                        else
                            Item = Item_NA;
                        Item_NA = "";
                    } while (Item != "");
                }
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("GC-讀取檔案資料 失敗", 99);
            }
        }

        public class FileTimeInfo
        {
            public string FileName;  //檔名
            public DateTime FileCreateTime; //建立時間
        }

        static FileTimeInfo GetLatestFileTimeInfo(string dir, string ext)
        {
            List<FileTimeInfo> list = new List<FileTimeInfo>();
            DirectoryInfo d = new DirectoryInfo(dir);
            foreach (FileInfo file in d.GetFiles())
            {
                if (file.Name.ToUpper().Contains(ext.ToUpper()))
                {
                    list.Add(new FileTimeInfo()
                    {
                        FileName = file.Name,
                        FileCreateTime = file.CreationTime
                    });
                }
            }
            var f = from x in list
                    orderby x.FileCreateTime
                    select x;
            return f.LastOrDefault();
        }
    }
}
