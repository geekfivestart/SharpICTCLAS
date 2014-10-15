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

namespace SharpICTCLAS
{
   //==================================================
   // Original predefined in DynamicArray.h file
   //==================================================
   public class WordResult
   {
      //The word 
      public string sWord;

      //the POS of the word
      public int nPOS;

      //The -log(frequency/MAX)
      public double dValue;
   }

   //--------------------------------------------------
   // data structure for word item
   //--------------------------------------------------
   public class WordItem
   {
      public int nWordLen;

      //The word 
      public string sWord;

      //the process or information handle of the word
      public int nPOS;

      //The count which it appear
      public int nFrequency;
   }

   //--------------------------------------------------
   //data structure for dictionary index table item
   //--------------------------------------------------
   public class IndexTableItem
   {
      //The count number of words which initial letter is sInit
      public int nCount;

      //The  head of word items
      public WordItem[] WordItems;

      public Dictionary<string, int> WordItemDict;
   }

   //--------------------------------------------------
   //data structure for word item chain
   //--------------------------------------------------
   public class WordChain
   {
      public WordItem data;
      public WordChain next;
   }

   //--------------------------------------------------
   //data structure for dictionary index table item
   //--------------------------------------------------
   public class ModifyTableItem
   {
      //The count number of words which initial letter is sInit
      public int nCount;

      //The number of deleted items in the index table
      public int nDelete;

      //The head of word items
      public WordChain pWordItemHead;
   }

   //--------------------------------------------------
   // return value of GetWordInfos Method in Dictionary.cs
   //--------------------------------------------------
   public class WordInfo
   {
      public string sWord;
      public int Count = 0;

      public List<int> POSs = new List<int>();
      public List<int> Frequencies = new List<int>();
   }
}
