using System;
using System.IO;
using SharpICTCLAS;

class Program
{
   static void Main(string[] args)
   {
      string DictPath = Path.Combine(Environment.CurrentDirectory, "Data") + Path.DirectorySeparatorChar;
      Console.WriteLine("正在读入字典，请稍候...");

      WordDictionary dict = new WordDictionary();
      dict.Load(DictPath + "coreDict.dct");
      ShowWordsInfo(dict, '设');

      Console.WriteLine("\r\n向字典库插入“设计模式”一词...");
      dict.AddItem("设计模式", Utility.GetPOSValue("n"), 10);

      Console.WriteLine("\r\n修改完成，将字典写入磁盘文件coreDictNew.dct，请稍候...");
      dict.Save(DictPath + "coreDictNew.dct");

      Console.WriteLine("\r\n打开已写入的字典，请稍候...");
      dict.Load(DictPath + "coreDictNew.dct");
      ShowWordsInfo(dict, '设');

      Console.Write("按下回车键退出......");
      Console.ReadLine();

   }

   public static void ShowWordsInfo(WordDictionary dict, char c)
   {
      int ccid = Utility.CC_ID(c);
      Console.WriteLine("====================================\r\n汉字:{0}, ID ：{1}\r\n", Utility.CC_ID2Char(ccid), ccid);

      Console.WriteLine("  词长  频率  词性   词");
      for (int i = 0; i < dict.indexTable[ccid].nCount; i++)
         Console.WriteLine("{0,5} {1,6} {2,5}  ({3}){4}",
            dict.indexTable[ccid].WordItems[i].nWordLen,
            dict.indexTable[ccid].WordItems[i].nFrequency,
            Utility.GetPOSString(dict.indexTable[ccid].WordItems[i].nPOS),
            Utility.CC_ID2Char(ccid),
            dict.indexTable[ccid].WordItems[i].sWord);
   }
}
