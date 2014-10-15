/***********************************************************************************
 * ICTCLAS简介：计算所汉语词法分析系统ICTCLAS
 *              Institute of Computing Technology, Chinese Lexical Analysis System
 *              功能有：中文分词；词性标注；未登录词识别。
 *              分词正确率高达97.58%(973专家评测结果)，
 *              未登录词识别召回率均高于90%，其中中国人名的识别召回率接近98%;
 *              处理速度为31.5Kbytes/s。
 * 著作权：  Copyright(c)2002-2005中科院计算所 职务著作权人：张华平
 * 遵循协议：自然语言处理开放资源许可证1.0
 * Email: zhanghp@software.ict.ac.cn
 * Homepage:www.i3s.ac.cn
 * 
 *----------------------------------------------------------------------------------
 * 
 * Copyright (c) 2000, 2001
 *     Institute of Computing Tech.
 *     Chinese Academy of Sciences
 *     All rights reserved.
 *
 * This file is the confidential and proprietary property of
 * Institute of Computing Tech. and the posession or use of this file requires
 * a written license from the author.
 * Author:   Kevin Zhang
 *          (zhanghp@software.ict.ac.cn)、
 * 
 *----------------------------------------------------------------------------------
 * 
 * SharpICTCLAS：.net平台下的ICTCLAS
 *               是由河北理工大学经管学院吕震宇根据Free版ICTCLAS改编而成，
 *               并对原有代码做了部分重写与调整
 * 
 * Email: zhenyulu@163.com
 * Blog: http://www.cnblogs.com/zhenyulu
 * 
 ***********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace SharpICTCLAS
{
    public sealed class Utility
    {
        static Utility()
        {
            CC_ID_Init();
            CtoArray();
        }
        private Utility()
        {
        }

        static List<byte[]> C_Array = new List<byte[]>();
        private static void CtoArray()
        {
            for (char i = (char)0; i < 0xffff; i++)
            {
                byte[] b = gb2312.GetBytes(i.ToString());
                C_Array.Add(b);
            }
        }

        /// <summary>
        /// 预先创建gb2312Encoding实例
        /// </summary>
        static Encoding gb2312 = Encoding.GetEncoding("gb2312");

        #region GetPOSValue Method

        public static int GetPOSValue(string sPOS)
        {
            char[] c = sPOS.ToCharArray();

            if (c.Length == 1)
                return Convert.ToInt32(c[0]) * 256;
            else if (c.Length == 2)
                return Convert.ToInt32(c[0]) * 256 + Convert.ToInt32(c[1]);
            else
            {
                string s1 = sPOS.Substring(0, sPOS.IndexOf('+'));
                string s2 = sPOS.Substring(sPOS.IndexOf('+') + 1);
                return 100 * GetPOSValue(s1) + Int32.Parse(s2);
            }
        }

        #endregion

        #region GetPOSString Method

        public static string GetPOSString(int nPOS)
        {
            string sPOSRet;

            if (nPOS > Convert.ToInt32('a') * 25600)
            {
                if ((nPOS / 100) % 256 != 0)
                    sPOSRet = string.Format("{0}{1}+{2}", Convert.ToChar(nPOS / 25600), Convert.ToChar((nPOS / 100) % 256), nPOS % 100);
                else
                    sPOSRet = string.Format("{0}+{1}", Convert.ToChar(nPOS / 25600), nPOS % 100);
            }
            else
            {
                if (nPOS > 256)
                    sPOSRet = string.Format("{0}{1}", Convert.ToChar(nPOS / 256), Convert.ToChar(nPOS % 256));
                else
                    sPOSRet = string.Format("{0}", Convert.ToChar(nPOS % 256));
            }
            return sPOSRet;
        }

        #endregion

        //====================================================================
        // 根据汉字的两个字节返回对应的CC_ID
        //====================================================================
        public static int CC_ID(byte b1, byte b2)
        {
            return (Convert.ToInt32(b1) - 176) * 94 + (Convert.ToInt32(b2) - 161);
        }

        static int[] CC_ID_Dict = new int[0xffff];
        private static void CC_ID_Init()
        {
            for (char i = (char)0; i < 0xffff; i++)
            {
                byte[] b = gb2312.GetBytes(i.ToString());
                if (b.Length != 2)
                {
                    CC_ID_Dict[i] = (-1);
                }
                else
                {
                    CC_ID_Dict[i] = ((Convert.ToInt32(b[0]) - 176) * 94 + (Convert.ToInt32(b[1]) - 161));
                }
            }
        }
        //====================================================================
        // 根据汉字返回对应的CC_ID
        //====================================================================
        public static int CC_ID(char c)
        {
            return CC_ID_Dict[c];
        }

        //====================================================================
        // 根据CC_ID返回对应的汉字
        //====================================================================
        public static char CC_ID2Char(int cc_id)
        {
            if (cc_id < 0 || cc_id > Predefine.CC_NUM)
                return '\0';

            byte[] b = new byte[2];

            b[0] = CC_CHAR1(cc_id);
            b[1] = CC_CHAR2(cc_id);
            return (gb2312.GetChars(b))[0];
        }

        //====================================================================
        // 根据CC_ID返回对应汉字的第一个字节
        //====================================================================
        public static byte CC_CHAR1(int cc_id)
        {
            return Convert.ToByte(cc_id / 94 + 176);
        }

        //====================================================================
        // 根据CC_ID返回对应汉字的第二个字节
        //====================================================================
        public static byte CC_CHAR2(int cc_id)
        {
            return Convert.ToByte(cc_id % 94 + 161);
        }

        //====================================================================
        // 将字符串转换为字节数组（用于将汉字需要拆分成2字节）
        //====================================================================
        public static byte[] String2ByteArray(string s)
        {
            return gb2312.GetBytes(s);
        }

        //====================================================================
        // 将字符串转换为字节数组（用于将汉字需要拆分成2字节）只转换首字符
        //====================================================================
        public static byte[] String2ByteArrayFirst(string s)
        {
            return C_Array[s[0]];
        }

        //====================================================================
        // 将字节数组重新转换为字符串
        //====================================================================
        public static string ByteArray2String(byte[] byteArray)
        {
            return gb2312.GetString(byteArray);
        }

        //====================================================================
        // 获取字符串长度（一个汉字按2个长度算）
        //====================================================================
        public static int GetWordLength(string s)
        {
            return String2ByteArray(s).Length;
        }

        //====================================================================
        // Func Name  : charType
        // Description: Judge the type of sChar or (sChar,sChar+1)
        // Parameters : sFilename: the file name for the output CC List
        // Returns    : int : the type of char
        //====================================================================
        public static int charType(char c)
        {
            if (Convert.ToInt32(c) < 128)
            {
                string delimiters = " *!,.?()[]{}+=";
                //注释：原来程序为"\042!,.?()[]{}+="，"\042"为10进制42好ASC字符，为*
                if (delimiters.IndexOf(c) >= 0)
                    return Predefine.CT_DELIMITER;
                return Predefine.CT_SINGLE;
            }

            byte[] byteArray = C_Array[c];

            if (byteArray.Length != 2)
                return Predefine.CT_OTHER;

            int b1 = Convert.ToInt32(byteArray[0]);
            int b2 = Convert.ToInt32(byteArray[1]);

            return DoubleByteCharType(b1, b2);
        }

        private static int DoubleByteCharType(int b1, int b2)
        {
            //-------------------------------------------------------
            /*
               code  +0 +1 +2 +3 +4 +5 +6 +7 +8 +9 +A +B +C +D +E +F
               A2A0     ⅰ ⅱ ⅲ ⅳ ⅴ ⅵ ⅶ ⅷ ⅸ ⅹ     
               A2B0   ⒈ ⒉ ⒊ ⒋ ⒌ ⒍ ⒎ ⒏ ⒐ ⒑ ⒒ ⒓ ⒔ ⒕ ⒖
               A2C0  ⒗ ⒘ ⒙ ⒚ ⒛ ⑴ ⑵ ⑶ ⑷ ⑸ ⑹ ⑺ ⑻ ⑼ ⑽ ⑾
               A2D0  ⑿ ⒀ ⒁ ⒂ ⒃ ⒄ ⒅ ⒆ ⒇ ① ② ③ ④ ⑤ ⑥ ⑦
               A2E0  ⑧ ⑨ ⑩   ㈠ ㈡ ㈢ ㈣ ㈤ ㈥ ㈦ ㈧ ㈨ ㈩ 
               A2F0   Ⅰ Ⅱ Ⅲ Ⅳ Ⅴ Ⅵ Ⅶ Ⅷ Ⅸ Ⅹ Ⅺ Ⅻ     
             */
            if (b1 == 162)
                return Predefine.CT_INDEX;

            //-------------------------------------------------------
            //０ １ ２ ３ ４ ５ ６ ７ ８ ９
            else if (b1 == 163 && b2 > 175 && b2 < 186)
                return Predefine.CT_NUM;

            //-------------------------------------------------------
            //ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ
            //ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ 
            else if (b1 == 163 && (b2 >= 193 && b2 <= 218 || b2 >= 225 && b2 <= 250))
                return Predefine.CT_LETTER;

            //-------------------------------------------------------
            /*
              code  +0 +1 +2 +3 +4 +5 +6 +7 +8 +9 +A +B +C +D +E +F
              A1A0     　 、 。 · ˉ ˇ ¨ 〃 々 — ～ ‖ … ‘ ’
              A1B0  “ ” 〔 〕 〈 〉 《 》 「 」 『 』 〖 〗 【 】
              A1C0  ± × ÷ ∶ ∧ ∨ ∑ ∏ ∪ ∩ ∈ ∷ √ ⊥ ∥ ∠
              A1D0  ⌒ ⊙ ∫ ∮ ≡ ≌ ≈ ∽ ∝ ≠ ≮ ≯ ≤ ≥ ∞ ∵
              A1E0  ∴ ♂ ♀ ° ′ ″ ℃ ＄ ¤ ￠ ￡ ‰ § № ☆ ★
              A1F0  ○ ● ◎ ◇ ◆ □ ■ △ ▲ ※ → ← ↑ ↓ 〓   
              以下除了字母和数字的部分
              code  +0 +1 +2 +3 +4 +5 +6 +7 +8 +9 +A +B +C +D +E +F
              A3A0     ！ ＂ ＃ ￥ ％ ＆ ＇ （ ） ＊ ＋ ， － ． ／
              A3B0                                ： ； ＜ ＝ ＞ ？
              A3C0  ＠ 
              A3D0                                   ［ ＼ ］ ＾ ＿
              A3E0  ｀ 
              A3F0                                   ｛ ｜ ｝ ￣ 
             */
            else if (b1 == 161 || b1 == 163)
                return Predefine.CT_DELIMITER;


            else if (b1 >= 176 && b1 <= 247)
                return Predefine.CT_CHINESE;


            else
                return Predefine.CT_OTHER;
        }

        //====================================================================
        // Func Name  : IsAllSingleByte
        // Description: Judge the string is all made up of Single Byte Char
        // Parameters : sSentence: the original sentence which includes Chinese or Non-Chinese char
        // Returns    : the end of the sub-sentence
        //====================================================================
        public static bool IsAllChinese(string sString)
        {
            byte[] byteArray = String2ByteArray(sString);

            int nLen = byteArray.Length, i = 0;

            while (i < nLen - 1 && Convert.ToInt32(byteArray[i]) < 248 && Convert.ToInt32(byteArray[i]) > 175)
            {
                i += 2;
            }
            if (i < nLen)
                return false;
            return true;
        }

        static Regex IsAllNumRegex = new Regex(@"^[±+－\-＋]?[０１２３４５６７８９\d]*[∶·．／./]?[０１２３４５６７８９\d]*[百千万亿佰仟％‰%]?$", RegexOptions.Compiled);
        //====================================================================
        //Judge the string is all made up of Num Char
        //====================================================================
        public static bool IsAllNum(string sString)
        {
            return IsAllNumRegex.IsMatch(sString );
        }

        static Regex IsAllChineseNumRegex = new Regex(@"^[几数第上成]?[零○一二两三四五六七八九十廿百千万亿壹贰叁肆伍陆柒捌玖拾佰仟∶·．／点]*[分之]?[零○一二两三四五六七八九十廿百千万亿壹贰叁肆伍陆柒捌玖拾佰仟]*$", RegexOptions.Compiled);
        //====================================================================
        //Decide whether the word is Chinese Num word
        //====================================================================
        public static bool IsAllChineseNum(string sWord)
        {
            //百分之五点六的人早上八点十八分起床
            return IsAllChineseNumRegex.IsMatch(sWord);
        }

        //====================================================================
        //Binary search a value in a table which len is nTableLen
        //====================================================================
        public static int BinarySearch(int nVal, int[] nTable)
        {
            int nStart = 0, nEnd = nTable.Length - 1, nMid = (nStart + nEnd) / 2;

            while (nStart <= nEnd)
            {
                if (nTable[nMid] == nVal)
                    return nMid;
                else if (nTable[nMid] < nVal)
                    nStart = nMid + 1;
                else
                    nEnd = nMid - 1;
                nMid = (nStart + nEnd) / 2;
            }

            return -1;
        }

        //====================================================================
        //Judge the string is all made up of Letter Char
        //====================================================================
        public static bool IsAllLetter(string sString)
        {
            char[] charArray = sString.ToCharArray();
            foreach (char c in charArray)
                if (charType(c) != Predefine.CT_LETTER)
                    return false;

            return true;
        }

        //====================================================================
        //Decide whether the word is all  non-foreign translation
        //====================================================================
        public static int GetForeignCharCount(string sWord)
        {
            int nForeignCount, nCount;
            nForeignCount = GetCharCount(Predefine.TRANS_ENGLISH, sWord); //English char counnts
            nCount = GetCharCount(Predefine.TRANS_JAPANESE, sWord); //Japan char counnts
            if (nForeignCount <= nCount)
                nForeignCount = nCount;
            nCount = GetCharCount(Predefine.TRANS_RUSSIAN, sWord); //Russian char counnts
            if (nForeignCount <= nCount)
                nForeignCount = nCount;
            return nForeignCount;
        }

        //====================================================================
        //Get the count of char which is in sWord and in sCharSet
        //====================================================================
        public static int GetCharCount(string sCharSet, string sWord)
        {
            int nCount = 0;
            char[] charArray = sWord.ToCharArray();
            foreach (char c in charArray)
                if (sCharSet.IndexOf(c) != -1)
                    nCount++;

            return nCount;
        }

        //====================================================================
        // 按照CC_ID的大小比较两个字符串，例如 超－“声 < 生 < 现”
        //====================================================================
        public static int CCStringCompare(string ca1, string ca2)
        {


            int minLength = Math.Min(ca1.Length, ca2.Length);

            for (int i = 0; i < minLength; i++)
            {
                //假设都是全角字符
                int cc1 = CC_ID_Dict[ca1[i]];
                int cc2 = CC_ID_Dict[ca2[i]];
                if (cc1 != -1 && cc2 != -1)
                {
                    if (cc1 < cc2)
                        return -1;
                    if (cc1 > cc2)
                        return 1;
                }
                else
                {

                    int ca1int = Convert.ToInt32(ca1[i]);
                    int ca2int = Convert.ToInt32(ca2[i]);
                    if (ca1int < 128 && ca2int < 128) //如果两个字符都是半角
                    {
                        if (ca1[i] < ca2[i])
                            return -1;
                        else if (ca1[i] > ca2[i])
                            return 1;
                    }
                    else if (ca1int < 128)
                        return -1;
                    else if (ca2int < 128)
                        return 1;
                }
            }

            if (ca1.Length > ca2.Length)
                return 1;
            else if (ca1.Length == ca2.Length)
                return 0;
            else
                return -1;
        }

        //====================================================================
        //Generate the GB2312 List file
        //====================================================================
        public static bool GB2312_Generate(string sFileName)
        {
            bool isSuccess = true;
            byte[] b = new byte[2];
            FileStream outputFile = null;
            StreamWriter writer = null;

            try
            {
                outputFile = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
                if (outputFile == null)
                    return false;

                writer = new StreamWriter(outputFile, Encoding.GetEncoding("gb2312"));

                for (int i = 161; i < 255; i++)
                    for (int j = 161; j < 255; j++)
                    {
                        b[0] = Convert.ToByte(i);
                        b[1] = Convert.ToByte(j);
                        writer.WriteLine(string.Format("{0}, {1}, {2}", ByteArray2String(b), i, j));
                    }
            }
            catch
            {
                isSuccess = false;
            }
            finally
            {
                if (writer != null)
                    writer.Close();

                if (outputFile != null)
                    outputFile.Close();
            }
            return isSuccess;
        }

        //====================================================================
        //Generate the Chinese Char List file
        //====================================================================
        public static bool CC_Generate(string sFileName)
        {
            bool isSuccess = true;
            byte[] b = new byte[2];
            FileStream outputFile = null;
            StreamWriter writer = null;

            try
            {
                outputFile = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
                if (outputFile == null)
                    return false;

                writer = new StreamWriter(outputFile, Encoding.GetEncoding("gb2312"));

                for (int i = 176; i < 255; i++)
                    for (int j = 161; j < 255; j++)
                    {
                        b[0] = Convert.ToByte(i);
                        b[1] = Convert.ToByte(j);
                        writer.WriteLine(string.Format("{0}, {1}, {2}", ByteArray2String(b), i, j));
                    }
            }
            catch
            {
                isSuccess = false;
            }
            finally
            {
                if (writer != null)
                    writer.Close();

                if (outputFile != null)
                    outputFile.Close();
            }
            return isSuccess;
        }

        #region 全角半角转换

        /*==========================================
       * 下面两个方法：
       * Author : 邝伟科(kwklover)
       * Home   : http://www.cnblogs.com/kwklover
       *==========================================*/
        /// <summary>
        /// 把字符转换为全角的(半角转全角)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static char ConvertToQJC(char c)
        {
            if (c == 32)
                return (char)12288;

            if (c < 127)
                return (char)(c + 65248);

            return c;
        }

        /// <summary>
        /// 把字符转换为半角的(全角转半角)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static char ConvertToBJC(char c)
        {
            if (c == 12288)
                return (char)32;

            if (c > 65280 && c < 65375)
                return (char)(c - 65248);

            return c;
        }

        /*=============================================================================
         *下面两个方法from http://hardrock.cnblogs.com/archive/2005/09/27/245255.html
         ==============================================================================*/
        /// <summary>
        /// 转全角的函数(SBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>全角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>        
        public static string ToSBC(string input)
        {
            //半角转全角：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }


        /// <summary>
        /// 转半角的函数(DBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        public static string ToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        #endregion

        public static string Traditional2Simplified(string s)
        {
            StringBuilder sb = new StringBuilder();

            string simp = "锕皑蔼碍爱嗳嫒瑷暧霭谙铵鹌肮袄奥媪骜鳌坝罢钯摆败呗颁办绊钣帮绑镑谤剥饱宝报鲍鸨龅辈贝钡狈备惫鹎贲锛绷笔毕毙币闭荜哔滗铋筚跸边编贬变辩辫苄缏笾标骠飑飙镖镳鳔鳖别瘪濒滨宾摈傧缤槟殡膑镔髌鬓饼禀拨钵铂驳饽钹鹁补钸财参蚕残惭惨灿骖黪苍舱仓沧厕侧册测恻层诧锸侪钗搀掺蝉馋谗缠铲产阐颤冁谄谶蒇忏婵骣觇禅镡场尝长偿肠厂畅伥苌怅阊鲳钞车彻砗尘陈衬伧谌榇碜龀撑称惩诚骋枨柽铖铛痴迟驰耻齿炽饬鸱冲冲虫宠铳畴踌筹绸俦帱雠橱厨锄雏础储触处刍绌蹰传钏疮闯创怆锤缍纯鹑绰辍龊辞词赐鹚聪葱囱从丛苁骢枞凑辏蹿窜撺错锉鹾达哒鞑带贷骀绐担单郸掸胆惮诞弹殚赕瘅箪当挡党荡档谠砀裆捣岛祷导盗焘灯邓镫敌涤递缔籴诋谛绨觌镝颠点垫电巅钿癫钓调铫鲷谍叠鲽钉顶锭订铤丢铥东动栋冻岽鸫窦犊独读赌镀渎椟牍笃黩锻断缎簖兑队对怼镦吨顿钝炖趸夺堕铎鹅额讹恶饿谔垩阏轭锇锷鹗颚颛鳄诶儿尔饵贰迩铒鸸鲕发罚阀珐矾钒烦贩饭访纺钫鲂飞诽废费绯镄鲱纷坟奋愤粪偾丰枫锋风疯冯缝讽凤沣肤辐抚辅赋复负讣妇缚凫驸绂绋赙麸鲋鳆钆该钙盖赅杆赶秆赣尴擀绀冈刚钢纲岗戆镐睾诰缟锆搁鸽阁铬个纥镉颍给亘赓绠鲠龚宫巩贡钩沟苟构购够诟缑觏蛊顾诂毂钴锢鸪鹄鹘剐挂鸹掴关观馆惯贯诖掼鹳鳏广犷规归龟闺轨诡贵刽匦刿妫桧鲑鳜辊滚衮绲鲧锅国过埚呙帼椁蝈铪骇韩汉阚绗颉号灏颢阂鹤贺诃阖蛎横轰鸿红黉讧荭闳鲎壶护沪户浒鹕哗华画划话骅桦铧怀坏欢环还缓换唤痪焕涣奂缳锾鲩黄谎鳇挥辉毁贿秽会烩汇讳诲绘诙荟哕浍缋珲晖荤浑诨馄阍获货祸钬镬击机积饥迹讥鸡绩缉极辑级挤几蓟剂济计记际继纪讦诘荠叽哜骥玑觊齑矶羁虿跻霁鲚鲫夹荚颊贾钾价驾郏浃铗镓蛲歼监坚笺间艰缄茧检碱硷拣捡简俭减荐槛鉴践贱见键舰剑饯渐溅涧谏缣戋戬睑鹣笕鲣鞯将浆蒋桨奖讲酱绛缰胶浇骄娇搅铰矫侥脚饺缴绞轿较挢峤鹪鲛阶节洁结诫届疖颌鲒紧锦仅谨进晋烬尽劲荆茎卺荩馑缙赆觐鲸惊经颈静镜径痉竞净刭泾迳弪胫靓纠厩旧阄鸠鹫驹举据锯惧剧讵屦榉飓钜锔窭龃鹃绢锩镌隽觉决绝谲珏钧军骏皲开凯剀垲忾恺铠锴龛闶钪铐颗壳课骒缂轲钶锞颔垦恳龈铿抠库裤喾块侩郐哙脍宽狯髋矿旷况诓诳邝圹纩贶亏岿窥馈溃匮蒉愦聩篑阃锟鲲扩阔蛴蜡腊莱来赖崃徕涞濑赉睐铼癞籁蓝栏拦篮阑兰澜谰揽览懒缆烂滥岚榄斓镧褴琅阆锒捞劳涝唠崂铑铹痨乐鳓镭垒类泪诔缧篱狸离鲤礼丽厉励砾历沥隶俪郦坜苈莅蓠呖逦骊缡枥栎轹砺锂鹂疠粝跞雳鲡鳢俩联莲连镰怜涟帘敛脸链恋炼练蔹奁潋琏殓裢裣鲢粮凉两辆谅魉疗辽镣缭钌鹩猎临邻鳞凛赁蔺廪檩辚躏龄铃灵岭领绫棂蛏鲮馏刘浏骝绺镏鹨龙聋咙笼垄拢陇茏泷珑栊胧砻楼娄搂篓偻蒌喽嵝镂瘘耧蝼髅芦卢颅庐炉掳卤虏鲁赂禄录陆垆撸噜闾泸渌栌橹轳辂辘氇胪鸬鹭舻鲈峦挛孪滦乱脔娈栾鸾銮抡轮伦仑沦纶论囵萝罗逻锣箩骡骆络荦猡泺椤脶镙驴吕铝侣屡缕虑滤绿榈褛锊呒妈玛码蚂马骂吗唛嬷杩买麦卖迈脉劢瞒馒蛮满谩缦镘颡鳗猫锚铆贸麽没镁门闷们扪焖懑钔锰梦眯谜弥觅幂芈谧猕祢绵缅渑腼黾庙缈缪灭悯闽闵缗鸣铭谬谟蓦馍殁镆谋亩钼呐钠纳难挠脑恼闹铙讷馁内拟腻铌鲵撵辇鲶酿鸟茑袅聂啮镊镍陧蘖嗫颟蹑柠狞宁拧泞苎咛聍钮纽脓浓农侬哝驽钕诺傩疟欧鸥殴呕沤讴怄瓯盘蹒庞抛疱赔辔喷鹏纰罴铍骗谝骈飘缥频贫嫔苹凭评泼颇钋扑铺朴谱镤镨栖脐齐骑岂启气弃讫蕲骐绮桤碛颀颃鳍牵钎铅迁签谦钱钳潜浅谴堑佥荨悭骞缱椠钤枪呛墙蔷强抢嫱樯戗炝锖锵镪羟跄锹桥乔侨翘窍诮谯荞缲硗跷窃惬锲箧钦亲寝锓轻氢倾顷请庆揿鲭琼穷茕蛱巯赇虮鳅趋区躯驱龋诎岖阒觑鸲颧权劝诠绻辁铨却鹊确阕阙悫让饶扰绕荛娆桡热韧认纫饪轫荣绒嵘蝾缛铷颦软锐蚬闰润洒萨飒鳃赛伞毵糁丧骚扫缫涩啬铯穑杀刹纱铩鲨筛晒酾删闪陕赡缮讪姗骟钐鳝墒伤赏垧殇觞烧绍赊摄慑设厍滠畲绅审婶肾渗诜谂渖声绳胜师狮湿诗时蚀实识驶势适释饰视试谥埘莳弑轼贳铈鲥寿兽绶枢输书赎属术树竖数摅纾帅闩双谁税顺说硕烁铄丝饲厮驷缌锶鸶耸怂颂讼诵擞薮馊飕锼苏诉肃谡稣虽随绥岁谇孙损笋荪狲缩琐锁唢睃獭挞闼铊鳎台态钛鲐摊贪瘫滩坛谭谈叹昙钽锬顸汤烫傥饧铴镗涛绦讨韬铽腾誊锑题体屉缇鹈阗条粜龆鲦贴铁厅听烃铜统恸头钭秃图钍团抟颓蜕饨脱鸵驮驼椭箨鼍袜娲腽弯湾顽万纨绾网辋韦违围为潍维苇伟伪纬谓卫诿帏闱沩涠玮韪炜鲔温闻纹稳问阌瓮挝蜗涡窝卧莴龌呜钨乌诬无芜吴坞雾务误邬庑怃妩骛鹉鹜锡牺袭习铣戏细饩阋玺觋虾辖峡侠狭厦吓硖鲜纤贤衔闲显险现献县馅羡宪线苋莶藓岘猃娴鹇痫蚝籼跹厢镶乡详响项芗饷骧缃飨萧嚣销晓啸哓潇骁绡枭箫协挟携胁谐写泻谢亵撷绁缬锌衅兴陉荥凶汹锈绣馐鸺虚嘘须许叙绪续诩顼轩悬选癣绚谖铉镟学谑泶鳕勋询寻驯训讯逊埙浔鲟压鸦鸭哑亚讶垭娅桠氩阉烟盐严岩颜阎艳厌砚彦谚验厣赝俨兖谳恹闫酽魇餍鼹鸯杨扬疡阳痒养样炀瑶摇尧遥窑谣药轺鹞鳐爷页业叶靥谒邺晔烨医铱颐遗仪蚁艺亿忆义诣议谊译异绎诒呓峄饴怿驿缢轶贻钇镒镱瘗舣荫阴银饮隐铟瘾樱婴鹰应缨莹萤营荧蝇赢颖茔莺萦蓥撄嘤滢潆璎鹦瘿颏罂哟拥佣痈踊咏镛优忧邮铀犹诱莸铕鱿舆鱼渔娱与屿语狱誉预驭伛俣谀谕蓣嵛饫阈妪纡觎欤钰鹆鹬龉鸳渊辕园员圆缘远橼鸢鼋约跃钥粤悦阅钺郧匀陨运蕴酝晕韵郓芸恽愠纭韫殒氲杂灾载攒暂赞瓒趱錾赃脏驵凿枣责择则泽赜啧帻箦贼谮赠综缯轧铡闸栅诈斋债毡盏斩辗崭栈战绽谵张涨帐账胀赵诏钊蛰辙锗这谪辄鹧贞针侦诊镇阵浈缜桢轸赈祯鸩挣睁狰争帧症郑证诤峥钲铮筝织职执纸挚掷帜质滞骘栉栀轵轾贽鸷蛳絷踬踯觯钟终种肿众锺诌轴皱昼骤纣绉猪诸诛烛瞩嘱贮铸驻伫槠铢专砖转赚啭馔颞桩庄装妆壮状锥赘坠缀骓缒谆准着浊诼镯兹资渍谘缁辎赀眦锱龇鲻踪总纵偬邹诹驺鲰诅组镞钻缵躜鳟翱并卜沉丑淀迭斗范干皋硅柜后伙秸杰诀夸里凌么霉捻凄扦圣尸抬涂洼喂污锨咸蝎彝涌游吁御愿岳云灶扎札筑于志注凋讠谫郄勐凼坂垅垴埯埝苘荬荮莜莼菰藁揸吒吣咔咝咴噘噼嚯幞岙嵴彷徼犸狍馀馇馓馕愣憷懔丬溆滟溷漤潴澹甯纟绔绱珉枧桊桉槔橥轱轷赍肷胨飚煳煅熘愍淼砜磙眍钚钷铘铞锃锍锎锏锘锝锪锫锿镅镎镢镥镩镲稆鹋鹛鹱疬疴痖癯裥襁耢颥螨麴鲅鲆鲇鲞鲴鲺鲼鳊鳋鳘鳙鞒鞴齄";
            string trad = "錒皚藹礙愛噯嬡璦曖靄諳銨鵪骯襖奧媼驁鰲壩罷鈀擺敗唄頒辦絆鈑幫綁鎊謗剝飽寶報鮑鴇齙輩貝鋇狽備憊鵯賁錛繃筆畢斃幣閉蓽嗶潷鉍篳蹕邊編貶變辯辮芐緶籩標驃颮飆鏢鑣鰾鱉別癟瀕濱賓擯儐繽檳殯臏鑌髕鬢餅稟撥缽鉑駁餑鈸鵓補鈽財參蠶殘慚慘燦驂黲蒼艙倉滄廁側冊測惻層詫鍤儕釵攙摻蟬饞讒纏鏟產闡顫囅諂讖蕆懺嬋驏覘禪鐔場嘗長償腸廠暢倀萇悵閶鯧鈔車徹硨塵陳襯傖諶櫬磣齔撐稱懲誠騁棖檉鋮鐺癡遲馳恥齒熾飭鴟沖衝蟲寵銃疇躊籌綢儔幬讎櫥廚鋤雛礎儲觸處芻絀躕傳釧瘡闖創愴錘綞純鶉綽輟齪辭詞賜鶿聰蔥囪從叢蓯驄樅湊輳躥竄攛錯銼鹺達噠韃帶貸駘紿擔單鄲撣膽憚誕彈殫賧癉簞當擋黨蕩檔讜碭襠搗島禱導盜燾燈鄧鐙敵滌遞締糴詆諦綈覿鏑顛點墊電巔鈿癲釣調銚鯛諜疊鰈釘頂錠訂鋌丟銩東動棟凍崠鶇竇犢獨讀賭鍍瀆櫝牘篤黷鍛斷緞籪兌隊對懟鐓噸頓鈍燉躉奪墮鐸鵝額訛惡餓諤堊閼軛鋨鍔鶚顎顓鱷誒兒爾餌貳邇鉺鴯鮞發罰閥琺礬釩煩販飯訪紡鈁魴飛誹廢費緋鐨鯡紛墳奮憤糞僨豐楓鋒風瘋馮縫諷鳳灃膚輻撫輔賦復負訃婦縛鳧駙紱紼賻麩鮒鰒釓該鈣蓋賅桿趕稈贛尷搟紺岡剛鋼綱崗戇鎬睪誥縞鋯擱鴿閣鉻個紇鎘潁給亙賡綆鯁龔宮鞏貢鉤溝茍構購夠詬緱覯蠱顧詁轂鈷錮鴣鵠鶻剮掛鴰摑關觀館慣貫詿摜鸛鰥廣獷規歸龜閨軌詭貴劊匭劌媯檜鮭鱖輥滾袞緄鯀鍋國過堝咼幗槨蟈鉿駭韓漢闞絎頡號灝顥閡鶴賀訶闔蠣橫轟鴻紅黌訌葒閎鱟壺護滬戶滸鶘嘩華畫劃話驊樺鏵懷壞歡環還緩換喚瘓煥渙奐繯鍰鯇黃謊鰉揮輝毀賄穢會燴匯諱誨繪詼薈噦澮繢琿暉葷渾諢餛閽獲貨禍鈥鑊擊機積饑跡譏雞績緝極輯級擠幾薊劑濟計記際繼紀訐詰薺嘰嚌驥璣覬齏磯羈蠆躋霽鱭鯽夾莢頰賈鉀價駕郟浹鋏鎵蟯殲監堅箋間艱緘繭檢堿鹼揀撿簡儉減薦檻鑒踐賤見鍵艦劍餞漸濺澗諫縑戔戩瞼鶼筧鰹韉將漿蔣槳獎講醬絳韁膠澆驕嬌攪鉸矯僥腳餃繳絞轎較撟嶠鷦鮫階節潔結誡屆癤頜鮚緊錦僅謹進晉燼盡勁荊莖巹藎饉縉贐覲鯨驚經頸靜鏡徑痙競凈剄涇逕弳脛靚糾廄舊鬮鳩鷲駒舉據鋸懼劇詎屨櫸颶鉅鋦窶齟鵑絹錈鐫雋覺決絕譎玨鈞軍駿皸開凱剴塏愾愷鎧鍇龕閌鈧銬顆殼課騍緙軻鈳錁頷墾懇齦鏗摳庫褲嚳塊儈鄶噲膾寬獪髖礦曠況誆誑鄺壙纊貺虧巋窺饋潰匱蕢憒聵簣閫錕鯤擴闊蠐蠟臘萊來賴崍徠淶瀨賚睞錸癩籟藍欄攔籃闌蘭瀾讕攬覽懶纜爛濫嵐欖斕鑭襤瑯閬鋃撈勞澇嘮嶗銠鐒癆樂鰳鐳壘類淚誄縲籬貍離鯉禮麗厲勵礫歷瀝隸儷酈壢藶蒞蘺嚦邐驪縭櫪櫟轢礪鋰鸝癘糲躒靂鱺鱧倆聯蓮連鐮憐漣簾斂臉鏈戀煉練蘞奩瀲璉殮褳襝鰱糧涼兩輛諒魎療遼鐐繚釕鷯獵臨鄰鱗凜賃藺廩檁轔躪齡鈴靈嶺領綾欞蟶鯪餾劉瀏騮綹鎦鷚龍聾嚨籠壟攏隴蘢瀧瓏櫳朧礱樓婁摟簍僂蔞嘍嶁鏤瘺耬螻髏蘆盧顱廬爐擄鹵虜魯賂祿錄陸壚擼嚕閭瀘淥櫨櫓轤輅轆氌臚鸕鷺艫鱸巒攣孿灤亂臠孌欒鸞鑾掄輪倫侖淪綸論圇蘿羅邏鑼籮騾駱絡犖玀濼欏腡鏍驢呂鋁侶屢縷慮濾綠櫚褸鋝嘸媽瑪碼螞馬罵嗎嘜嬤榪買麥賣邁脈勱瞞饅蠻滿謾縵鏝顙鰻貓錨鉚貿麼沒鎂門悶們捫燜懣鍆錳夢瞇謎彌覓冪羋謐獼禰綿緬澠靦黽廟緲繆滅憫閩閔緡鳴銘謬謨驀饃歿鏌謀畝鉬吶鈉納難撓腦惱鬧鐃訥餒內擬膩鈮鯢攆輦鯰釀鳥蔦裊聶嚙鑷鎳隉蘗囁顢躡檸獰寧擰濘苧嚀聹鈕紐膿濃農儂噥駑釹諾儺瘧歐鷗毆嘔漚謳慪甌盤蹣龐拋皰賠轡噴鵬紕羆鈹騙諞駢飄縹頻貧嬪蘋憑評潑頗釙撲鋪樸譜鏷鐠棲臍齊騎豈啟氣棄訖蘄騏綺榿磧頎頏鰭牽釬鉛遷簽謙錢鉗潛淺譴塹僉蕁慳騫繾槧鈐槍嗆墻薔強搶嬙檣戧熗錆鏘鏹羥蹌鍬橋喬僑翹竅誚譙蕎繰磽蹺竊愜鍥篋欽親寢鋟輕氫傾頃請慶撳鯖瓊窮煢蛺巰賕蟣鰍趨區軀驅齲詘嶇闃覷鴝顴權勸詮綣輇銓卻鵲確闋闕愨讓饒擾繞蕘嬈橈熱韌認紉飪軔榮絨嶸蠑縟銣顰軟銳蜆閏潤灑薩颯鰓賽傘毿糝喪騷掃繅澀嗇銫穡殺剎紗鎩鯊篩曬釃刪閃陜贍繕訕姍騸釤鱔墑傷賞坰殤觴燒紹賒攝懾設厙灄畬紳審嬸腎滲詵諗瀋聲繩勝師獅濕詩時蝕實識駛勢適釋飾視試謚塒蒔弒軾貰鈰鰣壽獸綬樞輸書贖屬術樹豎數攄紓帥閂雙誰稅順說碩爍鑠絲飼廝駟緦鍶鷥聳慫頌訟誦擻藪餿颼鎪蘇訴肅謖穌雖隨綏歲誶孫損筍蓀猻縮瑣鎖嗩脧獺撻闥鉈鰨臺態鈦鮐攤貪癱灘壇譚談嘆曇鉭錟頇湯燙儻餳鐋鏜濤絳討韜鋱騰謄銻題體屜緹鵜闐條糶齠鰷貼鐵廳聽烴銅統慟頭鈄禿圖釷團摶頹蛻飩脫鴕馱駝橢籜鼉襪媧膃彎灣頑萬紈綰網輞韋違圍為濰維葦偉偽緯謂衛諉幃闈溈潿瑋韙煒鮪溫聞紋穩問閿甕撾蝸渦窩臥萵齷嗚鎢烏誣無蕪吳塢霧務誤鄔廡憮嫵騖鵡鶩錫犧襲習銑戲細餼鬩璽覡蝦轄峽俠狹廈嚇硤鮮纖賢銜閑顯險現獻縣餡羨憲線莧薟蘚峴獫嫻鷴癇蠔秈躚廂鑲鄉詳響項薌餉驤緗饗蕭囂銷曉嘯嘵瀟驍綃梟簫協挾攜脅諧寫瀉謝褻擷紲纈鋅釁興陘滎兇洶銹繡饈鵂虛噓須許敘緒續詡頊軒懸選癬絢諼鉉鏇學謔澩鱈勛詢尋馴訓訊遜塤潯鱘壓鴉鴨啞亞訝埡婭椏氬閹煙鹽嚴巖顏閻艷厭硯彥諺驗厴贗儼兗讞懨閆釅魘饜鼴鴦楊揚瘍陽癢養樣煬瑤搖堯遙窯謠藥軺鷂鰩爺頁業葉靨謁鄴曄燁醫銥頤遺儀蟻藝億憶義詣議誼譯異繹詒囈嶧飴懌驛縊軼貽釔鎰鐿瘞艤蔭陰銀飲隱銦癮櫻嬰鷹應纓瑩螢營熒蠅贏穎塋鶯縈鎣攖嚶瀅瀠瓔鸚癭頦罌喲擁傭癰踴詠鏞優憂郵鈾猶誘蕕銪魷輿魚漁娛與嶼語獄譽預馭傴俁諛諭蕷崳飫閾嫗紆覦歟鈺鵒鷸齬鴛淵轅園員圓緣遠櫞鳶黿約躍鑰粵悅閱鉞鄖勻隕運蘊醞暈韻鄆蕓惲慍紜韞殞氳雜災載攢暫贊瓚趲鏨贓臟駔鑿棗責擇則澤賾嘖幘簀賊譖贈綜繒軋鍘閘柵詐齋債氈盞斬輾嶄棧戰綻譫張漲帳賬脹趙詔釗蟄轍鍺這謫輒鷓貞針偵診鎮陣湞縝楨軫賑禎鴆掙睜猙爭幀癥鄭證諍崢鉦錚箏織職執紙摯擲幟質滯騭櫛梔軹輊贄鷙螄縶躓躑觶鐘終種腫眾鍾謅軸皺晝驟紂縐豬諸誅燭矚囑貯鑄駐佇櫧銖專磚轉賺囀饌顳樁莊裝妝壯狀錐贅墜綴騅縋諄準著濁諑鐲茲資漬諮緇輜貲眥錙齜鯔蹤總縱傯鄒諏騶鯫詛組鏃鉆纘躦鱒翺並蔔沈醜澱叠鬥範幹臯矽櫃後夥稭傑訣誇裏淩麽黴撚淒扡聖屍擡塗窪餵汙鍁鹹蠍彜湧遊籲禦願嶽雲竈紮劄築於誌註雕訁譾郤猛氹阪壟堖垵墊檾蕒葤蓧蒓菇槁摣咤唚哢噝噅撅劈謔襆嶴脊仿僥獁麅餘餷饊饢楞怵懍爿漵灩混濫瀦淡寧糸絝緔瑉梘棬案橰櫫軲軤賫膁腖飈糊煆溜湣渺碸滾瞘鈈鉕鋣銱鋥鋶鐦鐧鍩鍀鍃錇鎄鎇鎿鐝鑥鑹鑔穭鶓鶥鸌癧屙瘂臒襇繈耮顬蟎麯鮁鮃鮎鯗鯝鯴鱝鯿鰠鰵鱅鞽韝齇";

            char[] sArray = s.ToCharArray();

            foreach (char c in sArray)
            {
                if (trad.IndexOf(c) >= 0)
                    sb.Append(simp.Substring(trad.IndexOf(c), 1));
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /*======= ICTCLAS 中有，但从未用到的方法，目前尚待实现 ============
      
        //Get the max Prefix string made up of Chinese Char
        public static uint GetCCPrefix(string sSentence)
        {
           return 0;
        }

        //Judge the string is all made up of non-Chinese Char
        public static bool IsAllNonChinese(string sString)
        {
           return true;
        }

        //Judge the string is all made up of Single Byte Char
        public static bool IsAllSingleByte(string sString)
        {
           return true;
        }



        //Judge the string is all made up of Index Num Char
        public static bool IsAllIndex(string sString)
        {
           return true;
        }

        //Judge the string is all made up of Delimiter
        public static bool IsAllDelimiter(string sString)
        {
           return true;
        }

        //sWord maybe is a foreign translation
        public static bool IsForeign(string sWord)
        {
           return true;
        }

        //Decide whether the word is all  foreign translation
        public static bool IsAllForeign(string sWord)
        {
           return true;
        }

        //Return the foreign type 
        public static int GetForeignType(string sWord)
        {
           return 0;
        }

        //Get the postfix
        public static bool PostfixSplit(string sWord, string sWordRet, string sPostfix)
        {
           return true;
        }

        //Judge whether it's a num
        public static bool IsSingleByteDelimiter(char cByteChar)
        {
           return true;
        }

        ================================================================*/

        /// <summary>
        /// 判断 s 是否出现在 orgstr的开头 
        /// </summary>
        /// <param name="orgstr"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool StartsWith(string orgstr, string s)
        {
            bool deffret = false;
            int l = s.Length;
            if (orgstr.Length < l)
            {
                return false;
            }
            if (l == 0)
            {
                return true;
            }
            for (int i = 0; i < l; i++)
            {
                if (orgstr[i] != s[i])
                {
                    return false;
                }
                else
                {
                    deffret = true;
                }
            }
            return deffret;
            //bool ss = indexTable[nFirstCharId].WordItems[i].sWord.StartsWith(sWordGet);
            //if (us != ss)
            //{

            //}
        }
    }
}