using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using SharpICTCLAS;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace MutiThreadTest
{
    enum eCodeType
    {
        CODE_TYPE_UNKNOWN,//type unknown
        CODE_TYPE_ASCII,//ASCII
        CODE_TYPE_GB,//GB2312,GBK,GB10380
        CODE_TYPE_UTF8,//UTF-8
        CODE_TYPE_BIG5//BIG5
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct result_t
    {
        [FieldOffset(0)]
        public int start;
        [FieldOffset(4)]
        public int length;
        [FieldOffset(8)]
        public int sPos;
        [FieldOffset(12)]
        public int sPosLow;
        [FieldOffset(16)]
        public int POS_id;
        [FieldOffset(20)]
        public int word_ID;
        [FieldOffset(24)]
        public int word_type;
        [FieldOffset(28)]
        public int weight;
    }
    class Program
    {

        const string path = @"ICTCLAS50.dll";
      
        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_Init")]
        public static extern bool ICTCLAS_Init(String sInitDirPath);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_Exit")]
        public static extern bool ICTCLAS_Exit();

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_ParagraphProcessAW",CallingConvention=CallingConvention.Winapi)]
        public static extern int ICTCLAS_ParagraphProcessAW(String sParagraph,  [Out, MarshalAs(UnmanagedType.LPArray)]result_t[] result,eCodeType eCT, int bPOSTagged);
        
        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "ICTCLAS_FileProcess")]
        public static extern double ICTCLAS_FileProcess(String sSrcFilename, eCodeType eCt, String sDsnFilename, int bPOStagged);

        private static List<string> fileList = new List<string>();
        private static string DictPath = Path.Combine(Environment.CurrentDirectory, "Data1") + Path.DirectorySeparatorChar;
        private static string testFileDir = Path.Combine(Environment.CurrentDirectory, "TESTFILE") + Path.DirectorySeparatorChar;
        private static string outDir = Path.Combine(Environment.CurrentDirectory, "OUTPUT") + Path.DirectorySeparatorChar;
        private static Thread thread0 = null;
        private static Thread thread1 = null;

        private static long totalFileSizeInByte = 0;
        static void Main(string[] args)
        {
            string[] fileStrings = Directory.GetFiles(testFileDir);  //获取文本文件

            
            //计算文本文件总大小
            foreach (string fi in fileStrings)
            {
                FileInfo fInfo = new FileInfo(fi);
                totalFileSizeInByte += fInfo.Length;
            }
            for (int i = 0; i < fileStrings.Length; ++i)
                fileList.Add(fileStrings[i]);

            //两线程使用SharpIctClas时行分词
            thread0 = new Thread(new ParameterizedThreadStart(AnalyFuc));
            thread1 = new Thread(new ParameterizedThreadStart(AnalyFuc));
            Para p0 = new Para();
            Para p1 = new Para();

            p0.Num = 0;
            p1.Num = 1;

            Stopwatch threadSp = new Stopwatch();
            threadSp.Start();
            thread0.Start(p0);
            thread1.Start(p1);

            thread0.Join();
            thread1.Join();

           threadSp.Stop();

           Console.WriteLine("SharpICTCLAS" + ":" + threadSp.ElapsedMilliseconds + "ms"+ " Word Segmentation Speed: "+(float)totalFileSizeInByte/1024/threadSp.ElapsedMilliseconds*1000+" KB/s");

           Thread.Sleep(1000);

            //单线程使用C++版ICTCLAS分词
           CPPictclas();     
    
        }

        
        private static void CPPictclas()
        {
            if (!ICTCLAS_Init(null))
            {
                System.Console.WriteLine("Init ICTCLAS failed!"); 
                System.Console.Read();
            }

            StreamReader sr = null;
            StreamWriter sw = new StreamWriter(outDir+".txt",false,System.Text.Encoding.Default);
            Stopwatch sp = new Stopwatch();
            sp.Start();
            int nWrdcnt;

            //对fileList中的每个文件进行分词，结果写入文本文件
            for (int i = 0; i < fileList.Count; i += 1)
            {
                sr = new StreamReader(fileList[i], System.Text.Encoding.Default);
                string input = "";
                input = sr.ReadLine();
                result_t []result;

                while (input != null)
                {
                    if (input == "")
                    {
                        input = sr.ReadLine();
                        continue;
                    }
                    try
                    {
                        result = new result_t[input.Length];                      
                        nWrdcnt = ICTCLAS_ParagraphProcessAW(input, result, eCodeType.CODE_TYPE_GB, 1);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    byte[] mybyte = System.Text.Encoding.Default.GetBytes(input);
                    byte[] byteWord = new byte[1] ;
                    for (int j = 0; j < nWrdcnt; ++j)
                    {
                        try
                        {
                            byteWord = new byte[result[j].length];
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        Array.Copy(mybyte, result[j].start, byteWord, 0, result[j].length);
                        string watch = System.Text.Encoding.Default.GetString(byteWord);
                        if (watch == " ")
                            continue;
                        sw.Write(System.Text.Encoding.Default.GetString(byteWord)+" ");

                    }
                    sw.WriteLine("");
                    input = sr.ReadLine();
                }
                sr.Close();
            }
            sw.Close();
            sp.Stop();
            Console.WriteLine("ICTCLAS:" + sp.ElapsedMilliseconds + "ms" + " Word Segmentation Speed: " + (float)totalFileSizeInByte / 1024 / sp.ElapsedMilliseconds * 1000 + " KB/s");
            ICTCLAS_Exit();
        }

        //线程的执行函数，for循环中线程根据自身的ID,加2取文本文件，保证两个线程的输入文件没有交集
        private static void AnalyFuc(object order)
        {
            int num = ((Para)order).Num;
            WordSegment wordSegment = new WordSegment();
            wordSegment.InitWordSegment(DictPath);
            StreamReader sr = null;
            StreamWriter sw = new StreamWriter(outDir+num+".txt",false,System.Text.Encoding.Default);

            for (int i = num; i < fileList.Count; i += 2)
            {
                sr = new StreamReader(fileList[i],System.Text.Encoding.Default);
                string input = "";
                input = sr.ReadLine();
                List<WordResult[]> result = null;
                while (input != null)
                {
                    if (input == "")
                    {
                        input = sr.ReadLine();
                        continue;
                    }
                    try
                    {
                        result = wordSegment.Segment(input);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                    for (int j = 1; j < result[0].Length - 1; ++j)
                    {
                        sw.Write(result[0][j].sWord + " ");
                    }
                    sw.WriteLine("");
                    input = sr.ReadLine();
                }
                sr.Close();
            }

           sw.Close();

        }
    }

    //用来向线程中传递参数的类
    class Para
    {
        private int num;
        public int Num
        {
            get { return num; }
            set { num=value;}
        }
    }
}
