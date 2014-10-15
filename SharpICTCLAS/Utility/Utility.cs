/***********************************************************************************
 * ICTCLAS��飺����������ʷ�����ϵͳICTCLAS
 *              Institute of Computing Technology, Chinese Lexical Analysis System
 *              �����У����ķִʣ����Ա�ע��δ��¼��ʶ��
 *              �ִ���ȷ�ʸߴ�97.58%(973ר��������)��
 *              δ��¼��ʶ���ٻ��ʾ�����90%�������й�������ʶ���ٻ��ʽӽ�98%;
 *              �����ٶ�Ϊ31.5Kbytes/s��
 * ����Ȩ��  Copyright(c)2002-2005�п�Ժ������ ְ������Ȩ�ˣ��Ż�ƽ
 * ��ѭЭ�飺��Ȼ���Դ�������Դ���֤1.0
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
 *          (zhanghp@software.ict.ac.cn)��
 * 
 *----------------------------------------------------------------------------------
 * 
 * SharpICTCLAS��.netƽ̨�µ�ICTCLAS
 *               ���ɺӱ�����ѧ����ѧԺ���������Free��ICTCLAS�ı���ɣ�
 *               ����ԭ�д������˲�����д�����
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
        /// Ԥ�ȴ���gb2312Encodingʵ��
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
        // ���ݺ��ֵ������ֽڷ��ض�Ӧ��CC_ID
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
        // ���ݺ��ַ��ض�Ӧ��CC_ID
        //====================================================================
        public static int CC_ID(char c)
        {
            return CC_ID_Dict[c];
        }

        //====================================================================
        // ����CC_ID���ض�Ӧ�ĺ���
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
        // ����CC_ID���ض�Ӧ���ֵĵ�һ���ֽ�
        //====================================================================
        public static byte CC_CHAR1(int cc_id)
        {
            return Convert.ToByte(cc_id / 94 + 176);
        }

        //====================================================================
        // ����CC_ID���ض�Ӧ���ֵĵڶ����ֽ�
        //====================================================================
        public static byte CC_CHAR2(int cc_id)
        {
            return Convert.ToByte(cc_id % 94 + 161);
        }

        //====================================================================
        // ���ַ���ת��Ϊ�ֽ����飨���ڽ�������Ҫ��ֳ�2�ֽڣ�
        //====================================================================
        public static byte[] String2ByteArray(string s)
        {
            return gb2312.GetBytes(s);
        }

        //====================================================================
        // ���ַ���ת��Ϊ�ֽ����飨���ڽ�������Ҫ��ֳ�2�ֽڣ�ֻת�����ַ�
        //====================================================================
        public static byte[] String2ByteArrayFirst(string s)
        {
            return C_Array[s[0]];
        }

        //====================================================================
        // ���ֽ���������ת��Ϊ�ַ���
        //====================================================================
        public static string ByteArray2String(byte[] byteArray)
        {
            return gb2312.GetString(byteArray);
        }

        //====================================================================
        // ��ȡ�ַ������ȣ�һ�����ְ�2�������㣩
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
                //ע�ͣ�ԭ������Ϊ"\042!,.?()[]{}+="��"\042"Ϊ10����42��ASC�ַ���Ϊ*
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
               A2A0     �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
               A2B0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
               A2C0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
               A2D0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
               A2E0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
               A2F0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��   
             */
            if (b1 == 162)
                return Predefine.CT_INDEX;

            //-------------------------------------------------------
            //�� �� �� �� �� �� �� �� �� ��
            else if (b1 == 163 && b2 > 175 && b2 < 186)
                return Predefine.CT_NUM;

            //-------------------------------------------------------
            //���£ãģţƣǣȣɣʣˣ̣ͣΣϣУѣңӣԣգ֣ףأ٣�
            //��������������������������������� 
            else if (b1 == 163 && (b2 >= 193 && b2 <= 218 || b2 >= 225 && b2 <= 250))
                return Predefine.CT_LETTER;

            //-------------------------------------------------------
            /*
              code  +0 +1 +2 +3 +4 +5 +6 +7 +8 +9 +A +B +C +D +E +F
              A1A0     �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
              A1B0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
              A1C0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
              A1D0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
              A1E0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
              A1F0  �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��   
              ���³�����ĸ�����ֵĲ���
              code  +0 +1 +2 +3 +4 +5 +6 +7 +8 +9 +A +B +C +D +E +F
              A3A0     �� �� �� �� �� �� �� �� �� �� �� �� �� �� ��
              A3B0                                �� �� �� �� �� ��
              A3C0  �� 
              A3D0                                   �� �� �� �� ��
              A3E0  �� 
              A3F0                                   �� �� �� �� 
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

        static Regex IsAllNumRegex = new Regex(@"^[��+��\-��]?[��������������������\d]*[�á�����./]?[��������������������\d]*[��ǧ���ڰ�Ǫ����%]?$", RegexOptions.Compiled);
        //====================================================================
        //Judge the string is all made up of Num Char
        //====================================================================
        public static bool IsAllNum(string sString)
        {
            return IsAllNumRegex.IsMatch(sString );
        }

        static Regex IsAllChineseNumRegex = new Regex(@"^[�������ϳ�]?[���һ�������������߰˾�ʮإ��ǧ����Ҽ��������½��ƾ�ʰ��Ǫ�á�������]*[��֮]?[���һ�������������߰˾�ʮإ��ǧ����Ҽ��������½��ƾ�ʰ��Ǫ]*$", RegexOptions.Compiled);
        //====================================================================
        //Decide whether the word is Chinese Num word
        //====================================================================
        public static bool IsAllChineseNum(string sWord)
        {
            //�ٷ�֮������������ϰ˵�ʮ�˷���
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
        // ����CC_ID�Ĵ�С�Ƚ������ַ��������� �������� < �� < �֡�
        //====================================================================
        public static int CCStringCompare(string ca1, string ca2)
        {


            int minLength = Math.Min(ca1.Length, ca2.Length);

            for (int i = 0; i < minLength; i++)
            {
                //���趼��ȫ���ַ�
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
                    if (ca1int < 128 && ca2int < 128) //��������ַ����ǰ��
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

        #region ȫ�ǰ��ת��

        /*==========================================
       * ��������������
       * Author : ��ΰ��(kwklover)
       * Home   : http://www.cnblogs.com/kwklover
       *==========================================*/
        /// <summary>
        /// ���ַ�ת��Ϊȫ�ǵ�(���תȫ��)
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
        /// ���ַ�ת��Ϊ��ǵ�(ȫ��ת���)
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
         *������������from http://hardrock.cnblogs.com/archive/2005/09/27/245255.html
         ==============================================================================*/
        /// <summary>
        /// תȫ�ǵĺ���(SBC case)
        /// </summary>
        /// <param name="input">�����ַ���</param>
        /// <returns>ȫ���ַ���</returns>
        ///<remarks>
        ///ȫ�ǿո�Ϊ12288����ǿո�Ϊ32
        ///�����ַ����(33-126)��ȫ��(65281-65374)�Ķ�Ӧ��ϵ�ǣ������65248
        ///</remarks>        
        public static string ToSBC(string input)
        {
            //���תȫ�ǣ�
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
        /// ת��ǵĺ���(DBC case)
        /// </summary>
        /// <param name="input">�����ַ���</param>
        /// <returns>����ַ���</returns>
        ///<remarks>
        ///ȫ�ǿո�Ϊ12288����ǿո�Ϊ32
        ///�����ַ����(33-126)��ȫ��(65281-65374)�Ķ�Ӧ��ϵ�ǣ������65248
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

            string simp = "ﹰ��������������������ư������������Ӱ��ٰڰ��°����Ӱ����������������������������������Ｑ��ʱϱбұ������������ϱ߱����������ֱ�������������������������������������ޱ���������������𾲹�߲ƲβϲвѲҲ������Բղֲײ޲����������٭�β������������������������������������ⳡ���������������������𳮳������³�����������ųƳͳϳ����������ճٳ۳ܳݳ��������拾����ٱ���ų���������������ۻ��鴫�˴������봸綴��ȴ�����Ǵʴ��˴ϴдѴӴ������ȴ�ꣴڴ�ߥ�����������������窵����������������������쵱������������ɵ�����������Ƶ���еӵݵ���ڮ�������ߵ��������������������������������������������񼶿�����Ķ����������Ͷ϶����ҶӶ����ֶٶ������������������������������������������������ܷ������������������÷����зɷ̷Ϸ������׷طܷ߷��Ƿ���������������������������������������������ŸøƸ���˸ϸѸ���ߦ礸Ըոָٸ���غھ��ﯸ������������ب����Ṩ������������������ڸ����ƹ�ڬ�����������й����ع۹ݹ߹�ڴ�������������������������������������������������������������������Һ׺�ڭ���ú������ڧݦ���׺���������ɻ�������������������������������������ۼ������ƻ����ӻԻٻ߻�������ڶ����������ͻ��ڻ���Ի����������������������������������������üƼǼʼ̼�ڦڵ��ߴ��������������������мԼռּؼۼ�ۣ������ͼ߼�������������������������������������������������������������������������ֽ����������½ýĽŽȽɽʽν�������޽׽ڽ��������ڽ�����������������������ݣ����������������������������������������Ǿ�����վԾپݾ���ڪ������������������������������������俪����������������������ſǿ����������ѿ���﬿ٿ��෿��ۦ���ڿ����ſ����ڲڿ�������ܿ�������������������������������������������������������������������������������������������������������������������������ڳ��������������������������ٳ۪����ݰ��߿����������������������������������������������������������������������������������������������������������������������������������������������������������¢£¤������������¥¦§¨�����������������«¬­®¯°±²³¸»¼½��ߣ���������������������������������������������������������������������������������������¿��������������������߼�����������������������������۽��������á�������èêíó��ûþ���������������������������������������������������������������������������ıĶ��������������������ګ������������������������������������������������šŢ������ťŦŧŨũٯ������ŵ��űŷŸŹŻŽک�������������������������ƭ����Ʈ��Ƶƶ��ƻƾ��������������������������������������ޭ�������������ǣǥǦǨǩǫǮǯǱǳǴǵ��ݡ��������ǹǺǽǾǿ������������������������������ڽ���������������������������������������������������������������ȣڰ������ȧȨȰڹ�����ȴȵȷ�����������������������������������������������������������ɡ���ɥɧɨ��ɬ����ɱɲɴ���ɸɹ��ɾ��������ڨ����������������������������������������������ڷ��������ʤʦʨʪʫʱʴʵʶʻ����������������ݪ߱����������������������������������˧��˫˭˰˳˵˶˸��˿�����������������������޴�������������������������������ݥ������������̡̢������̨̬����̷̸̯̰̱̲̳̾������������������������������������������������������������ͭͳ��ͷ��ͺͼ���������������������������������������������ΤΥΧΪΫάέΰαγν�����������������������������������������ݫ������������������������������������������ϮϰϳϷϸ�������ϺϽϿ����������������������������������������ݲ޺�������������������������ܼ���������������Х�����������ЭЮЯвгдкл��ߢ���п���������������������������������ڼ������ѡѢѤ������ѧ�����ѫѯѰѱѵѶѷ�����ѹѻѼ������������������������������������������ٲ�����������������������������������ҡҢңҤҥҩ������үҳҵҶ����������ҽҿ����������������������������ڱ߽������������������������������������ӣӤӥӦӧӨөӪӫӬӮӱ��ݺ������������������ӴӵӶӸӻӽ��������������ݵ����������������������ԤԦ��ٶ���������������������������ԧԨԯ԰ԱԲԵԶ�����ԼԾԿ������������������������۩ܿ��������������������������������������������������������������աբդթիծձյնշոջս����������������گ������������������������������������������������֢֣֤֡ں������ְִֽֿ֯������������������������������������������������������������������������פ������רשת׬�����׮ׯװױ׳״׶׸׹׺����׻׼���������������������������������������������������������������������������ɸ޹����սܾ�������ôù����Ǥʥʬ̧Ϳ��ι������Ы��ӿ������Ը��������������־ע��ڥ��ۧ����������������ݤݧݯݻ��޻��߸�����������������������������������������������������������������������������������������������������������������������������������������������������������������������������������";
            string trad = "�H�}�@�K�ۇ��ܭa���\�O�@�g�a�\�W��������T�Z�[���h�C�k�O�k�ͽ��^�r������U�d�_݅ؐ�^�N��v�l�S�Q���P�������]ɜ�����G�`ۋ߅���H׃�q�p�S���e����R�j�S�s�B�M�e�T�l�I�e�P���_����Ĝ�\�x�WA�����K�g�G��P�a�ؔ���Q���M�K�N��o�nœ�}����ȃԜy�Ō�Ԍ嚃��O�v���s�׋�p�P�a�U��~׏�r�ԋ��җ�U熈��L�L���c�S���t�O����K�n܇�س��m��r���R�´~�Z�ηQ���\�G���f��K�V�t�Y�u�X����|�_�n�x���|���P�I�I����ׇ���N�z�r�A���|̎�c�I�X���A���J�����N�E�����b�z�p�o�~�n�\�[��ą�ɐ򋘺��ݏ�f�Z�x�e�S�z�_�}�^���J�~�H��������đ���Q�����y�D�������hʎ�nו�X�d�v�u�\���I�c����狔����f���e�g�B���]�C��c�|늎p⚰d��{���ՙ�B�l���Vӆ�b�G�A�|�ӗ��������]�٪��xـ僞^�����V�t呔྄�f��ꠌ���煇��D�g���O�Z���I�Z�~Ӟ���I�@���ܗ�~������{�O�����D�E߃�s���b�l�P�y�m�\�C��؜��L���[���w�u�U�M�p��E�����^���S�f�S���h�L���T�p�S�P���wݗ���o�x��ؓӇ�D�`�D񀼛�Eَ���V�v�ԓ�}�w�W�U�s���M���{�C����䓾V����怲G�a�c䆔R���w�t���v�k�}�o���s�������m�ؕ�^��ƈ��ُ��ԍ���M�M��bݞ��d���]�X���������P�^�^�T؞ԟ���X���V�EҎ�w���|܉Ԏ�F���Q�����u�q�Z݁�LЖ�i��假��^��J�����X�x��n�h�R�W�R̖����u�Q�R�X�HϠ�M�Z���t�Zӏȇ�b�c���o�����G���W�A����Ԓ���f�щĚg�h߀���Q�������o�J�Q�D���S�e�m�]�x���V�x���Z�R�M�d�LԜ�C�����D�q��ȝ��՟�Q钫@؛����Z���C�e���E�I�u�����O݋���D���E����Ӌӛ�H�^�oӓԑ�j�\���K�^�J�W���bϊ�Q�V�q�a�A�v�a�Z⛃r�{�P���e��u���O�Թ{�g�D�}�O�z�A�|���캆���p�]���b�`�vҊ�IŞ���T�u�R���G�V��첀�Y�a���d���{�Y�����v�u�{�\�z���ɔ��q�C�e�_��U�g�I�^�׍����o�A�����Y�]�ðX�M�^�o�\�H֔�M�x�a�M���G�o���|�~�N�B�P�L�@���i�o�R���d����q��ޟ��Ä�n�m���f�b�F���x�e��䏑ք��n�ՙ��Z��|�M�e�N�����h�X�Q�^�H�k�x܊�E���_�P���N�����z�|���`��D�w���n�S�~�V���h�����l�H����ѝ���K�~����Ē�����y�V��r�E�N�����k�L̝�h�Q�����Tʉ�|����K�H�U�ϓϞ�D�R��ه���ƜZ�|�l�A�n�]�[�{�ڔr�@�@�m��׎���[���|���E������|�h����Z�Ƅڝ��Z���焰A�����D��I�C�w�h؂�x���Y������[�v�r�`���B��˞�W�y��ߊ�P�r�����]�Z��Z�O�c�V�Z�~�k�zɏ�B砑z�i����Ę朑ٟ����`�Y���I��ў�c���Z�����vՏ�u���|炿�����C�R���[�C�U�A�[�_�O�k�g��`�X�I�c���|�N�s���g�t�^�y�w���@���\�Ŕn�]�d�{���ɖV�a�Ǌ䓧�t�E�V�D���U���e�N�t�J�R�B�]�t���u̔���T���ꑉ��]��邞o�O�����_�`�A���F�R���A�|�n���\���y�L�D���[莒�݆�����S�]Փ���}�_߉茻j��j�ΫM�T���T��H���X�H�ҿ|�]�V�G���@�s�`�����aΛ�R�R��O�ߘq�I���u�~�}��m�z�U�M֙�z�N���؈�^�T�Q�N�]�V�T�����РF���{�i���[�i��Ғ���d�k�J�[�d�����t�w�R�����瑑�}�h���Q�ևփ��x�{��\���f���c�{�y���X���[�t�G�H�ȔMā��F�f݂�T��B�\�U������Y����b�������Q���r���o�~ē���r�z���w�S�Z�����W�t���I�a֎�Y�T�Pۘ�������r�\���i���`��_՛��h�~�lؚ���O�{�u���Hᕓ�䁘��V�h璗�Ě�R�T�M���◉ә�I�U�_������@�����F�U�w���t�X�Q���\�l�q�L�n�a�q�`���j���܉��N�����ԙ{�����I�j�uۄ�@��̃S�N�[�V�S�w�R���E�`��固D�J�H���u�p��A�Ո�c�������F��͐���g�l�qڅ�^�|��x�x����U�z�E����ԏ�J�b㌅s�o�_��I��׌���_�@ʁ�Ƙ���g�J�x�ܐ�s�q�Vϔ�d��Aܛ�J͘�c�����_�S�wِ��мR���}�߿������C�w���x���|���Y��ᇄh�W�٠��Ә���~��X�����p�s���x���B�d�z���O�������������I�BԖՔ�c�K�َ��{��Ԋ�r�g���R���m��ҕԇ�u�P�P�s�Y�B����۫F�R��ݔ���H���g���Q���d�����V�p�l����f�T�q�p�zP񆾌�J���Z��A�b�\˒�t�`�}�K�V�C�q�d�m�S���q�r�O�p�S�p�s�s���i��Ï�H���Y�B���_�B��T��؝�c�����TՄ�@���g�U활��C���h�|�M���{ӑ�w��v�`�R�}�w�Ͼ��Y�D�l�g�f���N�F�d �N�~�y�Q�^�^�d�D�Q�F���j͑�Ó�r�W�E�X���m�z�e�����B�f�w�U�W�y�f�`����H�SȔ���ξ��^�lՆ��靜����|�t���n���y����键Y��΁�u�C�P�n�}���u���_�oʏ�ǉ]�F���`�w�T�����\�^�F�a���u��㊑��q�]�tҠ�rݠ�{�b�M�B�����r�w�t��e�@�U�F�I�h�W�w�����{�W�\�s�������Bϖ�i�]����lԔ���G�A�J�|��ʒ���N�ԇ[�^�t򔽋�n���f���y�{�C���a�x�C�X���i�\��dꀜ���P�C�}��̓�u��S���w�mԂ�܎���x�_�k�X�C曌W�o���L��ԃ���ZӖӍ�d�_���\���f������Ӡ���I����鎟��}���r���G�������V򞅘�I����ח���Z��|��B����P��ꖰW�B�ӟ����u���b�G�{ˎ�U�_����퓘I�~�v�]���ϟ��t��U�z�xρˇ�|���xԄ�h�x�g���[�r�ҎF��A�O�W�O���O��Ŝ�a��y��[㟰a�ы������t��Ξ�I��ω�A�f�L�L�M�v�t�]�u���W�`�W���ѓ��b�xԁ�O���n�]♪q�T�~�B��ݛ�~�O���c�Z�Z�z�u�A�S���R՘�Iʚ���铋��u�D�e��O���r�x�Y�@�@�T�A���h���S�x�s�S耻�����X�y���E�\�N�j����i�|���C���y�����s���d����ٝ��ڎ�Y�E�K�z菗�؟��t��ّ�K���j�\�Pٛ�C��܈��l���p�S���ֱK��ݚ�䗣��`�d���q���~Û�w�t��U�H�N�@ֆ�m�pؑᘂ��\�ꇜ��b�E�F�c���c�걠�b�����Y���CՊ���`�P�~���̼����S���|���s���d�T�eٗ�v·�{�W�U�z犽K�N�[���R�a�S�����E�q�U�i�T�D�T�����A�T�v�Й�㏌��u�Dٍ�����D���f�b�y�Ѡ��F٘���Y�K�PՁ������Վ�CƝ�Y�n�J�l�w�D�{�O�b��ۙ���v���uՌ�|�O�{�M��@�y�g�V�O�K�N���h�յ��Y�����V������ⷷM���E�F�Y�R���q�ӜD�L�}�ƔE�T�D�j�@�v�yϐ�����[�n�R[노^�����B��I�]��Ӆ�v�S�͚���ň߈��|���{Ȓɉ�O������w�U�z�j�����o�L�S���e�w���N�l��������ݝs����E�z������f�y�z�g�����R���Mܠ�}�d�L�l�������^�L�g�b�O�y�H�{�����u�x��X�[���u��d�����B�I�[��{ğ�M���g��\�D�E�G�R���A�X�a�c�����I�X�x�O";

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

        /*======= ICTCLAS ���У�����δ�õ��ķ�����Ŀǰ�д�ʵ�� ============
      
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
        /// �ж� s �Ƿ������ orgstr�Ŀ�ͷ 
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