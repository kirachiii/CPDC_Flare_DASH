using System; 

namespace CPDC_Flare_DASH.Models
{
    class GlobalVariable
    {
        public static class SQL
        {
            public static string IP;
            public static string Port;
            public static string User;
            public static string Password;
            public static bool Status;
            public static string[,] InsertToRawGC = new string[16, 4];
            public static string[,] InsertToRaw = new string[14, 4];

            //20210826 新增
            public static string[,] InsertToAll = new string[30, 4];

            public static string waste_total;//累積流量
        }

        public static class CPDCInfo
        {
            public static string SQLDataCatelog; 
            public static string SQLDataInserString; 
            public static string SQLDataValueString;
            public static string SQLDataInserString_GC;
            public static string SQLDataValueString_GC;

            //20210826 insert T15 T60 ....用
            public static string SQLDataInserString_All;
            public static string SQLDataValueString_All;

            public static string GC_File_VOC_Name;
            public static string GC_File_THC_Name; 
            public static Boolean GC_File_Status = false;
            public static string Regulate_Range;
            public static Boolean Regulate_Status = false;
        }

        public static class Declare
        {
            public static string Password;
            public static string[,] DeclareItem = new string[23,2];
        }

        public static void DeclareLoad()
        {
            string [] DeclareArray;
            for (int i = 1; i <= 23; i++)
            { 
                DeclareArray = InitialFileControl.FileRead("Declare", "DeclareItem" + i, "").Split(',');
                GlobalVariable.Declare.DeclareItem[i - 1,0] = DeclareArray[0]; 
                GlobalVariable.Declare.DeclareItem[i - 1,1] = DeclareArray[1];
            }
        }

        public static string Key = "Jsene";
    }
}
