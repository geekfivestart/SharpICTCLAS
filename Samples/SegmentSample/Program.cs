using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharpICTCLAS;

class Program
{
   static void Main(string[] args)
   {
      string DictPath = Path.Combine(Environment.CurrentDirectory, "Data") + Path.DirectorySeparatorChar;
      Console.WriteLine("正在初始化字典库，请稍候...");
      WordSegmentSample sample = new WordSegmentSample(DictPath, 4);

      List<WordResult[]> result = sample.Segment(@"国防科大的王永恒提出了一种面向大规模数据库的基于频繁词集的短文本聚类算法");//(@"王晓平在1月份滦南大会上说的确实在理");
      PrintResult(result);

      Console.Write("\n按下回车键退出......");
      Console.ReadLine();
   }

   static void PrintResult(List<WordResult[]> result)
   {
      Console.WriteLine();
      for (int i = 0; i < result.Count; i++)
      {
          for (int j = 1; j < result[i].Length - 1; j++)
              // Console.Write("{0} /{1} ", result[i][j].sWord, Utility.GetPOSString(result[i][j].nPOS));
              Console.Write("{0} ", result[i][j].sWord);

         Console.WriteLine();
      }
   }
}
