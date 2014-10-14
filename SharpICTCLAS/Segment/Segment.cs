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

/************************************************************************************
This file was modified by Zhangjinchao , April 2012
��Segment������������private���͵�NShortPath ����ʵ��rawNShortPath ,optNShortPath 

��88��89 �д�ԭNShortPath��̬�����ĵ��ø�Ϊ��ʵ��rawNShortPath��Ӧ�����ĵ���
��122��123�д�ԭNShortPath��̬�����ĵ��ø�Ϊ��ʵ��optNShortPath ��Ӧ�����ĵ���
 
�Ž�  �й���ѧԺ���㼼���о���
Blog: http://www.hackerforward.com
 **************************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpICTCLAS
{
    public class Segment
    {
        private WordDictionary biDict, coreDict;

        public List<AtomNode> atomSegment;
        public RowFirstDynamicArray<ChainContent> segGraph;
        public ColumnFirstDynamicArray<ChainContent> biGraphResult;
        public RowFirstDynamicArray<ChainContent> m_graphOptimum;
        public List<WordResult[]> m_pWordSeg;  //��Ŷ���ִʽ��

        public event SegmentEventHandler OnSegmentEvent;

        public bool IsSendSegmentEvent { get; set; }

        private NShortPath rawNShortPath;
        private NShortPath optNShortPath;
        #region ���캯��

        public Segment(WordDictionary biDict, WordDictionary coreDict)
        {
            this.biDict = biDict;
            this.coreDict = coreDict;
            rawNShortPath = new NShortPath();
            optNShortPath = new NShortPath();
            IsSendSegmentEvent = false;
        }

        #endregion

        #region BiSegment Method

        public int BiSegment(string sSentence, double smoothPara, int nKind)
        {
            WordResult[] tmpResult;
            WordLinkedArray linkedArray;

            if (biDict == null || coreDict == null)
                throw new Exception("biDict �� coreDict ��δ��ʼ����");

            //---ԭ�ӷִ�
            atomSegment = AtomSegment(sSentence);
            OnAtomSegment(atomSegment);

            //---�����ʿ⣬�������п��ִܷʷ�������������ṹ
            segGraph = GenerateWordNet(atomSegment, coreDict);
            OnGenSegGraph(segGraph);

            //---�������п��ܵ��������
            biGraphResult = BiGraphGenerate(segGraph, smoothPara, biDict, coreDict);
            OnGenBiSegGraph(biGraphResult);

            //---N ���·�����������ִʷ���
            rawNShortPath.Calculate(biGraphResult, nKind);
            List<int[]> spResult = rawNShortPath.GetNPaths(Predefine.MAX_SEGMENT_NUM);
            OnNShortPath(spResult, segGraph);

            m_pWordSeg = new List<WordResult[]>();
            m_graphOptimum = new RowFirstDynamicArray<ChainContent>();

            for (int i = 0; i < spResult.Count; i++)
            {
                linkedArray = BiPath2LinkedArray(spResult[i], segGraph, atomSegment);
                tmpResult = GenerateWord(spResult[i], linkedArray, m_graphOptimum);

                if (tmpResult != null)
                    m_pWordSeg.Add(tmpResult);
            }

            OnBeforeOptimize(m_pWordSeg);

            return m_pWordSeg.Count;
        }

        #endregion

        #region BiOptimumSegment Method

        public int BiOptimumSegment(int nResultCount, double dSmoothingPara)
        {
            WordResult[] tmpResult;
            WordLinkedArray linkedArray;

            //Generate the biword link net
            ColumnFirstDynamicArray<ChainContent> aBiwordsNet = BiGraphGenerate(m_graphOptimum, dSmoothingPara, biDict, coreDict);
            OnGenBiOptimumSegGraph(aBiwordsNet);

            optNShortPath.Calculate(aBiwordsNet, nResultCount);
            List<int[]> spResult = optNShortPath.GetNPaths(Predefine.MAX_SEGMENT_NUM);

            m_pWordSeg = new List<WordResult[]>();
            segGraph = m_graphOptimum;
            m_graphOptimum = new RowFirstDynamicArray<ChainContent>();

            for (int i = 0; i < spResult.Count; i++)
            {
                linkedArray = BiPath2LinkedArray(spResult[i], segGraph, atomSegment);
                tmpResult = GenerateWord(spResult[i], linkedArray, m_graphOptimum);

                if (tmpResult != null)
                    m_pWordSeg.Add(tmpResult);
            }

            return m_pWordSeg.Count;
        }

        #endregion

        #region AtomSegment Method

        //====================================================================
        // ��sSentence���е������ֵ��и�
        //====================================================================
        public static List<AtomNode> AtomSegment(string sSentence)
        {
            List<AtomNode> atomSegment = new List<AtomNode>();
            AtomNode tmpEnd = null;
            int startIndex = 0, length = sSentence.Length, pCur = 0, nCurType, nNextType;
            StringBuilder sb = new StringBuilder();
            char c;

            // ����ǿ�ʼ����
            if (sSentence.StartsWith(Predefine.SENTENCE_BEGIN))
            {
                atomSegment.Add(new AtomNode(Predefine.SENTENCE_BEGIN, Predefine.CT_SENTENCE_BEGIN));
                startIndex = Predefine.SENTENCE_BEGIN.Length;
                length -= startIndex;
            }

            // ����ǽ�������
            if (sSentence.EndsWith(Predefine.SENTENCE_END))
            {
                tmpEnd = new AtomNode(Predefine.SENTENCE_END, Predefine.CT_SENTENCE_END);
                length -= Predefine.SENTENCE_END.Length;
            }

            //==============================================================================================
            // by zhenyulu:
            //
            // TODO: ʹ��һϵ��������ʽ�������е������ɷ֣��ٷֱȡ����ڡ������ʼ���URL�ȣ�Ԥ����ȡ����
            //==============================================================================================

            char[] charArray = sSentence.ToCharArray(startIndex, length);
            int[] charTypeArray = new int[charArray.Length];

            // ���ɶ�Ӧ�������ֵ��ַ���������
            for (int i = 0; i < charArray.Length; i++)
            {
                c = charArray[i];
                charTypeArray[i] = Utility.charType(c);

                if (c == '.' && i < (charArray.Length - 1) && Utility.charType(charArray[i + 1]) == Predefine.CT_NUM)
                    charTypeArray[i] = Predefine.CT_NUM;
                else if (c == '.' && i < (charArray.Length - 1) && charArray[i + 1] >= '0' && charArray[i + 1] <= '9')
                    charTypeArray[i] = Predefine.CT_SINGLE;
                else if (charTypeArray[i] == Predefine.CT_LETTER)
                    charTypeArray[i] = Predefine.CT_SINGLE;
            }

            // �����ַ����������е��������ԭ���и�
            while (pCur < charArray.Length)
            {
                nCurType = charTypeArray[pCur];

                if (nCurType == Predefine.CT_CHINESE || nCurType == Predefine.CT_INDEX ||
                   nCurType == Predefine.CT_DELIMITER || nCurType == Predefine.CT_OTHER)
                {
                    if (charArray[pCur].ToString().Trim().Length != 0)
                        atomSegment.Add(new AtomNode(charArray[pCur].ToString(), nCurType));
                    pCur++;
                }
                //������ַ������ֻ��ߺ�����������ֵ�С���㡰.����һֱȡ��ȥ��
                else if (pCur < charArray.Length - 1 && (nCurType == Predefine.CT_SINGLE || nCurType == Predefine.CT_NUM))
                {
                    sb.Remove(0, sb.Length);
                    sb.Append(charArray[pCur]);

                    bool reachEnd = true;
                    while (pCur < charArray.Length - 1)
                    {
                        nNextType = charTypeArray[++pCur];

                        if (nNextType == nCurType)
                            sb.Append(charArray[pCur]);
                        else
                        {
                            reachEnd = false;
                            break;
                        }
                    }
                    atomSegment.Add(new AtomNode(sb.ToString(), nCurType));
                    if (reachEnd)
                        pCur++;
                }
                // ���������������
                else
                {
                    atomSegment.Add(new AtomNode(charArray[pCur].ToString(), nCurType));
                    pCur++;
                }
            }

            // ���ӽ�����־
            if (tmpEnd != null)
                atomSegment.Add(tmpEnd);

            return atomSegment;
        }

        #endregion

        #region GenerateWordNet Method

        //====================================================================
        // Func Name  : GenerateWordNet
        // Description: Generate the segmentation word net according 
        //              the original sentence
        // Parameters : sSentence: the sentence
        //              dictCore : core dictionary
        //              bOriginalFreq=false: output original frequency
        // Returns    : bool
        //====================================================================
        public static RowFirstDynamicArray<ChainContent> GenerateWordNet(List<AtomNode> atomSegment, WordDictionary coreDict)
        {
            string sWord = "", sMaxMatchWord;
            int nPOSRet, nPOS, nTotalFreq;
            double dValue = 0;

            RowFirstDynamicArray<ChainContent> m_segGraph = new RowFirstDynamicArray<ChainContent>();
            m_segGraph.SetEmpty();

            // ��ԭ�Ӳ��ִ���m_segGraph
            for (int i = 0; i < atomSegment.Count; i++)//Init the cost array
            {
                if (atomSegment[i].nPOS == Predefine.CT_CHINESE)
                    m_segGraph.SetElement(i, i + 1, new ChainContent(0, 0, atomSegment[i].sWord));
                else
                {
                    sWord = atomSegment[i].sWord;//init the word 
                    dValue = Predefine.MAX_FREQUENCE;
                    switch (atomSegment[i].nPOS)
                    {
                        case Predefine.CT_INDEX:
                        case Predefine.CT_NUM:
                            nPOS = -27904;//'m'*256
                            sWord = "δ##��";
                            dValue = 0;
                            break;
                        case Predefine.CT_DELIMITER:
                            nPOS = 30464;//'w'*256;
                            break;
                        case Predefine.CT_LETTER:
                            nPOS = -28280; // -'n' * 256 - 'x';
                            dValue = 0;
                            sWord = "δ##��";
                            break;
                        case Predefine.CT_SINGLE://12021-2129-3121
                            if (Regex.IsMatch(atomSegment[i].sWord, @"^(-?\d+)(\.\d+)?$"))����//ƥ�両����
                            {
                                nPOS = -27904;//'m'*256
                                sWord = "δ##��";
                            }
                            else
                            {
                                nPOS = -28280; // -'n' * 256 - 'x'
                                sWord = "δ##��";
                            }
                            dValue = 0;
                            break;
                        default:
                            nPOS = atomSegment[i].nPOS;//'?'*256;
                            break;
                    }
                    m_segGraph.SetElement(i, i + 1, new ChainContent(dValue, nPOS, sWord));//init the link with minimum
                }
            }

            // �����п��ܵ���ʴ���m_segGraph
            for (int i = 0; i < atomSegment.Count; i++)//All the word
            {
                sWord = atomSegment[i].sWord;//Get the current atom
                int j = i + 1;

                while (j < atomSegment.Count && coreDict.GetMaxMatch(sWord, out sMaxMatchWord, out nPOSRet))
                {
                    if (sMaxMatchWord == sWord)  // ��������Ҫ�ҵĴ�
                    {
                        WordInfo info = coreDict.GetWordInfo(sWord); // �ôʿ��ܾ��ж��ִ���

                        // ����ôʵ����д�Ƶ֮��
                        nTotalFreq = 0;
                        for (int k = 0; k < info.Count; k++)
                            nTotalFreq += info.Frequencies[k];

                        // ���Ƴ���ĳЩ�����
                        if (sWord.Length == 2 && (sWord.StartsWith("��") || sWord.StartsWith("��")) && i >= 1 &&
                           (Utility.IsAllNum(atomSegment[i - 1].sWord) ||
                           Utility.IsAllChineseNum(atomSegment[i - 1].sWord)))
                        {
                            //1���ڡ�1999��ĩ
                            if ("ĩ���е�ǰ���".IndexOf(sWord.Substring(1)) >= 0)
                                break;
                        }

                        // ����ô�ֻ��һ�����ԣ���洢��������Լ�¼Ϊ 0
                        if (info.Count == 1)
                            m_segGraph.SetElement(i, j, new ChainContent(nTotalFreq, info.POSs[0], sWord));
                        else
                            m_segGraph.SetElement(i, j, new ChainContent(nTotalFreq, 0, sWord));
                    }

                    sWord += atomSegment[j++].sWord;
                }
            }
            return m_segGraph;
        }

        #endregion

        #region BiGraphGenerate Method

        //====================================================================
        // ����������֮��Ķ���ͼ��
        //====================================================================
        public static ColumnFirstDynamicArray<ChainContent> BiGraphGenerate(
           RowFirstDynamicArray<ChainContent> aWord, double smoothPara, WordDictionary biDict, WordDictionary coreDict)
        {
            ColumnFirstDynamicArray<ChainContent> aBiWordNet = new ColumnFirstDynamicArray<ChainContent>();

            ChainItem<ChainContent> pCur, pNextWords;
            int nTwoWordsFreq = 0, nCurWordIndex, nNextWordIndex;
            double dCurFreqency, dValue, dTemp;
            string sTwoWords;
            StringBuilder sb = new StringBuilder();

            //Record the position map of possible words
            int[] m_npWordPosMapTable = PreparePositionMap(aWord);

            pCur = aWord.GetHead();
            while (pCur != null)
            {
                if (pCur.Content.nPOS >= 0)
                    //It's not an unknown words
                    dCurFreqency = pCur.Content.eWeight;
                else
                    //Unknown words
                    dCurFreqency = coreDict.GetFrequency(pCur.Content.sWord, 2);

                //Get next words which begin with pCur.col��ע��������Ķ�Ӧ��ϵ��
                pNextWords = aWord.GetFirstElementOfRow(pCur.col);

                while (pNextWords != null && pNextWords.row == pCur.col)
                {
                    sb.Remove(0, sb.Length);
                    sb.Append(pCur.Content.sWord);
                    sb.Append(Predefine.WORD_SEGMENTER);
                    sb.Append(pNextWords.Content.sWord);

                    sTwoWords = sb.ToString();

                    //Two linked Words frequency
                    nTwoWordsFreq = biDict.GetFrequency(sTwoWords, 3);

                    //Smoothing
                    dTemp = 1.0 / Predefine.MAX_FREQUENCE;

                    //-log{a*P(Ci-1)+(1-a)P(Ci|Ci-1)} Note 0<a<1
                    dValue = -Math.Log(smoothPara * (1.0 + dCurFreqency) / (Predefine.MAX_FREQUENCE + 80000.0)
                      + (1.0 - smoothPara) * ((1.0 - dTemp) * nTwoWordsFreq / (1.0 + dCurFreqency) +
                      dTemp));

                    //Unknown words: P(Wi|Ci);while known words:1
                    if (pCur.Content.nPOS < 0)
                        dValue += pCur.Content.nPOS;

                    //Get the position index of current word in the position map table
                    nCurWordIndex = Utility.BinarySearch(pCur.row * Predefine.MAX_SENTENCE_LEN + pCur.col, m_npWordPosMapTable);
                    nNextWordIndex = Utility.BinarySearch(pNextWords.row * Predefine.MAX_SENTENCE_LEN + pNextWords.col, m_npWordPosMapTable);

                    aBiWordNet.SetElement(nCurWordIndex, nNextWordIndex, new ChainContent(dValue, pCur.Content.nPOS, sTwoWords));

                    pNextWords = pNextWords.next; //Get next word
                }
                pCur = pCur.next;
            }

            return aBiWordNet;
        }

        //====================================================================
        // ׼��PositionMap�����ڼ�¼�ʵ�λ��
        //====================================================================
        private static int[] PreparePositionMap(RowFirstDynamicArray<ChainContent> aWord)
        {
            int[] m_npWordPosMapTable;
            ChainItem<ChainContent> pTail, pCur;
            int nWordIndex = 0, m_nWordCount;

            //Get tail element and return the words count
            m_nWordCount = aWord.GetTail(out pTail);

            if (m_nWordCount > 0)
                m_npWordPosMapTable = new int[m_nWordCount];
            else
                m_npWordPosMapTable = null;

            //Record the  position of possible words
            pCur = aWord.GetHead();
            while (pCur != null)
            {
                m_npWordPosMapTable[nWordIndex++] = pCur.row * Predefine.MAX_SENTENCE_LEN + pCur.col;
                pCur = pCur.next;
            }

            return m_npWordPosMapTable;
        }

        #endregion

        #region Private Static Functions

        #region BiPath2LinkedArray Method

        //====================================================================
        // ��BiPathת��ΪLinkedArray
        // ���硰��˵��ȷʵ����
        // BiPath����0, 1, 2, 3, 6, 9, 11, 12��
        //    0    1   2   3   4     5   6     7   8     9   10    11  12
        // ʼ##ʼ  ��  ˵  ��  ��ȷ  ȷ  ȷʵ  ʵ  ʵ��  ��  ����  ��  ĩ##ĩ
        //====================================================================
        private static WordLinkedArray BiPath2LinkedArray(int[] biPath, RowFirstDynamicArray<ChainContent> segGraph, List<AtomNode> atomSegment)
        {
            List<ChainItem<ChainContent>> list = segGraph.ToListItems();
            StringBuilder sb = new StringBuilder();

            WordLinkedArray result = new WordLinkedArray();

            for (int i = 0; i < biPath.Length; i++)
            {
                WordNode node = new WordNode();

                node.row = list[biPath[i]].row;
                node.col = list[biPath[i]].col;
                node.sWordInSegGraph = list[biPath[i]].Content.sWord;

                node.theWord = new WordResult();
                if (node.sWordInSegGraph == "δ##��" || node.sWordInSegGraph == "δ##��" ||
                   node.sWordInSegGraph == "δ##��" || node.sWordInSegGraph == "δ##ʱ" || node.sWordInSegGraph == "δ##��")
                {
                    sb.Remove(0, sb.Length);
                    for (int j = node.row; j < node.col; j++)
                        sb.Append(atomSegment[j].sWord);

                    node.theWord.sWord = sb.ToString();
                }
                else
                    node.theWord.sWord = list[biPath[i]].Content.sWord;

                node.theWord.nPOS = list[biPath[i]].Content.nPOS;
                node.theWord.dValue = list[biPath[i]].Content.eWeight;

                result.AppendNode(node);
            }

            return result;
        }

        #endregion

        #region GenerateWord Method

        //====================================================================
        // Generate Word according the segmentation route
        //====================================================================
        private static WordResult[] GenerateWord(int[] uniPath, WordLinkedArray linkedArray, RowFirstDynamicArray<ChainContent> m_graphOptimum)
        {
            if (linkedArray.Count == 0)
                return null;

            //--------------------------------------------------------------------
            //Merge all seperate continue num into one number
            MergeContinueNumIntoOne(ref linkedArray);

            //--------------------------------------------------------------------
            //The delimiter "����"
            ChangeDelimiterPOS(ref linkedArray);

            //--------------------------------------------------------------------
            //���ǰһ���������֣���ǰ���ԡ�������-����ʼ�����Ҳ�ֹ��һ���ַ���
            //��ô���ˡ��������Ŵӵ�ǰ���з��������
            //���� ��3 / -4 / �¡���Ҫ��ֳɡ�3 / - / 4 / �¡�
            SplitMiddleSlashFromDigitalWords(ref linkedArray);

            //--------------------------------------------------------------------
            //1�������ǰ�������֣���һ�����ǡ��¡��ա�ʱ���֡��롢�·ݡ��е�һ������ϲ�,�ҵ�ǰ�ʴ�����ʱ��
            //2�������ǰ���ǿ�����Ϊ��ݵ����֣���һ�����ǡ��ꡱ����ϲ�������Ϊʱ�䣬����Ϊ���֡�
            //3��������һ��������"��" ������Ϊ��ǰ������ʱ��
            //4�������ǰ�����һ�����ֲ���"�á�����"�Ͱ�ǵ�'.''/'����ô����
            //5����ǰ�����һ��������"�á�����"�Ͱ�ǵ�'.''/'���ҳ��ȴ���1����ôȥ�����һ���ַ�������"1."
            CheckDateElements(ref linkedArray);

            //--------------------------------------------------------------------
            //������
            WordResult[] result = new WordResult[linkedArray.Count];

            WordNode pCur = linkedArray.first;
            int i = 0;
            while (pCur != null)
            {
                WordResult item = new WordResult();
                item.sWord = pCur.theWord.sWord;
                item.nPOS = pCur.theWord.nPOS;
                item.dValue = pCur.theWord.dValue;
                result[i] = item;

                m_graphOptimum.SetElement(pCur.row, pCur.col, new ChainContent(item.dValue, item.nPOS, pCur.sWordInSegGraph));

                pCur = pCur.next;
                i++;
            }

            return result;
        }

        #endregion

        #region MergeContinueNumIntoOne Method

        private static void MergeContinueNumIntoOne(ref WordLinkedArray linkedArray)
        {
            if (linkedArray.Count < 2)
                return;

            string tmp;
            WordNode pCur = linkedArray.first;
            WordNode pNext = pCur.next;

            while (pNext != null)
            {
                if ((Utility.IsAllNum(pCur.theWord.sWord) || Utility.IsAllChineseNum(pCur.theWord.sWord)) &&
                   (Utility.IsAllNum(pNext.theWord.sWord) || Utility.IsAllChineseNum(pNext.theWord.sWord)))
                {
                    tmp = pCur.theWord.sWord + pNext.theWord.sWord;
                    if (Utility.IsAllNum(tmp) || Utility.IsAllChineseNum(tmp))
                    {
                        pCur.theWord.sWord += pNext.theWord.sWord;
                        pCur.col = pNext.col;
                        pCur.next = pNext.next;
                        linkedArray.Count--;
                        pNext = pCur.next;
                        continue;
                    }
                }

                pCur = pCur.next;
                pNext = pNext.next;
            }
        }

        #endregion

        #region ChangeDelimiterPOS Method

        private static void ChangeDelimiterPOS(ref WordLinkedArray linkedArray)
        {
            WordNode pCur = linkedArray.first;
            while (pCur != null)
            {
                if (pCur.theWord.sWord == "����" || pCur.theWord.sWord == "��" || pCur.theWord.sWord == "-")
                {
                    pCur.theWord.nPOS = 30464; //'w'*256;Set the POS with 'w'
                    pCur.theWord.dValue = 0;
                }

                pCur = pCur.next;
            }
        }

        #endregion

        #region SplitMiddleSlashFromDigitalWords Method

        //====================================================================
        //���ǰһ���������֣���ǰ���ԡ�������-����ʼ�����Ҳ�ֹ��һ���ַ���
        //��ô���ˡ��������Ŵӵ�ǰ���з��������
        //���� ��3 / -4 / �¡���Ҫ��ֳɡ�3 / - / 4 / �¡�
        //====================================================================
        private static void SplitMiddleSlashFromDigitalWords(ref WordLinkedArray linkedArray)
        {
            if (linkedArray.Count < 2)
                return;

            WordNode pCur = linkedArray.first.next;
            WordNode pPre = linkedArray.first;

            while (pCur != null)
            {
                //27904='m'*256
                if ((Math.Abs(pPre.theWord.nPOS) == 27904 || Math.Abs(pPre.theWord.nPOS) == 29696) &&
                   (Utility.IsAllNum(pCur.theWord.sWord) || Utility.IsAllChineseNum(pCur.theWord.sWord)) &&
                   ("-��".IndexOf(pCur.theWord.sWord.ToCharArray()[0]) >= 0) && pCur.theWord.sWord.Length > 1)
                {
                    // ����������ֳ�����
                    WordNode newNode = new WordNode();
                    newNode.row = pCur.row + 1;
                    newNode.col = pCur.col;
                    newNode.sWordInSegGraph = pCur.theWord.sWord.Substring(1);
                    WordResult theWord = new WordResult();
                    theWord.sWord = newNode.sWordInSegGraph;
                    theWord.nPOS = 27904;
                    theWord.dValue = pCur.theWord.dValue;
                    newNode.theWord = theWord;

                    pCur.col = pCur.row + 1;
                    pCur.theWord.sWord = pCur.theWord.sWord.Substring(0, 1);
                    pCur.theWord.nPOS = 30464; //'w'*256;
                    pCur.theWord.dValue = 0;

                    newNode.next = pCur.next;
                    pCur.next = newNode;

                    linkedArray.Count++;
                }
                pCur = pCur.next;
                pPre = pPre.next;
            }
        }

        #endregion

        #region CheckDateElements Method

        //====================================================================
        //1�������ǰ�������֣���һ�����ǡ��¡��ա�ʱ���֡��롢�·ݡ��е�һ������ϲ��ҵ�ǰ�ʴ�����ʱ��
        //2�������ǰ���ǿ�����Ϊ��ݵ����֣���һ�����ǡ��ꡱ����ϲ�������Ϊʱ�䣬����Ϊ���֡�
        //3��������һ��������"��" ������Ϊ��ǰ������ʱ��
        //4�������ǰ�����һ�����ֲ���"�á�����"�Ͱ�ǵ�'.''/'����ô����
        //5����ǰ�����һ��������"�á�����"�Ͱ�ǵ�'.''/'���ҳ��ȴ���1����ôȥ�����һ���ַ�������"1."
        //====================================================================
        private static void CheckDateElements(ref WordLinkedArray linkedArray)
        {
            if (linkedArray.Count < 2)
                return;

            string nextWord;
            WordNode pCur = linkedArray.first;
            WordNode pNext = pCur.next;

            while (pNext != null)
            {
                if (Utility.IsAllNum(pCur.theWord.sWord) || Utility.IsAllChineseNum(pCur.theWord.sWord))
                {
                    //===== 1�������ǰ�������֣���һ�����ǡ��¡��ա�ʱ���֡��롢�·ݡ��е�һ������ϲ��ҵ�ǰ�ʴ�����ʱ��
                    nextWord = pNext.theWord.sWord;
                    if ((nextWord.Length == 1 && "����ʱ����".IndexOf(nextWord) != -1) || (nextWord.Length == 2 && nextWord == "�·�"))
                    {
                        //2001��
                        pCur.theWord.sWord += nextWord;
                        pCur.col = pNext.col;
                        pCur.sWordInSegGraph = "δ##ʱ";
                        pCur.theWord.nPOS = -29696; //'t'*256;//Set the POS with 'm'
                        pCur.next = pNext.next;
                        pNext = pCur.next;
                        linkedArray.Count--;
                    }
                    //===== 2�������ǰ���ǿ�����Ϊ��ݵ����֣���һ�����ǡ��ꡱ����ϲ�������Ϊʱ�䣬����Ϊ���֡�
                    else if (nextWord == "��")
                    {
                        if (IsYearTime(pCur.theWord.sWord))
                        {
                            pCur.theWord.sWord += nextWord;
                            pCur.col = pNext.col;
                            pCur.sWordInSegGraph = "δ##ʱ";
                            pCur.theWord.nPOS = -29696; //'t'*256;//Set the POS with 'm'
                            pCur.next = pNext.next;
                            pNext = pCur.next;
                            linkedArray.Count--;
                        }
                        //===== ����ǰ�ʾ��������� =====
                        else
                        {
                            pCur.sWordInSegGraph = "δ##��";
                            pCur.theWord.nPOS = -27904; //Set the POS with 'm'
                        }
                    }
                    else
                    {
                        //===== 3��������һ��������"��" ������Ϊ��ǰ������ʱ��
                        if (pCur.theWord.sWord.EndsWith("��"))
                        {
                            pCur.sWordInSegGraph = "δ##ʱ";
                            pCur.theWord.nPOS = -29696; //Set the POS with 't'
                        }
                        else
                        {
                            char[] tmpcharArray = pCur.theWord.sWord.ToCharArray();
                            string lastChar = tmpcharArray[tmpcharArray.Length - 1].ToString();
                            //===== 4�������ǰ�����һ�����ֲ���"�á�����"�Ͱ�ǵ�'.''/'����ô����
                            if ("�á�����./".IndexOf(lastChar) == -1)
                            {
                                pCur.sWordInSegGraph = "δ##��";
                                pCur.theWord.nPOS = -27904; //'m'*256;Set the POS with 'm'
                            }
                            //===== 5����ǰ�����һ��������"�á�����"�Ͱ�ǵ�'.''/'���ҳ��ȴ���1����ôȥ�����һ���ַ�������"1."
                            else if (pCur.theWord.sWord.Length > 1)
                            {
                                pCur.theWord.sWord = pCur.theWord.sWord.Substring(0, pCur.theWord.sWord.Length - 1);

                                pCur.sWordInSegGraph = "δ##��";
                                pCur.theWord.nPOS = -27904; //'m'*256;Set the POS with 'm'
                            }
                        }
                    }
                }

                pCur = pCur.next;
                pNext = pNext.next;
            }
        }

        #endregion

        #region IsYearTime Method

        private static bool IsYearTime(string sNum)
        {
            //Judge whether the sNum is a num genearating year
            int nLen = sNum.Length;
            char[] charArray = sNum.ToCharArray();

            //1992��, 90��
            if (Utility.IsAllNum(sNum) && (nLen == 4 || nLen == 2 && "����������56789".IndexOf(charArray[0]) != -1))
                return true;

            if (Utility.GetCharCount("���һ�����������߰˾�Ҽ��������½��ƾ�", sNum) == nLen && nLen >= 2)
                return true;

            //��Ǫ�����
            if (nLen == 4 && Utility.GetCharCount("ǧǪ���", sNum) == 2)
                return true;

            if (nLen == 1 && Utility.GetCharCount("ǧǪ", sNum) == 1)
                return true;

            if (nLen == 2 && Regex.IsMatch(sNum, "^[���ұ����켺�����ɹ�][�ӳ���î������δ�����纥]$"))
                return true;

            return false;
        }

        #endregion

        #endregion

        #region Events

        private void SendEvents(SegmentEventArgs e)
        {
            if (OnSegmentEvent != null)
                OnSegmentEvent(this, e);
        }

        private void OnAtomSegment(List<AtomNode> nodes)
        {
            if (IsSendSegmentEvent == false)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nodes.Count; i++)
                sb.Append(string.Format("{0}, ", nodes[i].sWord));

            sb.Append("\r\n");

            SendEvents(new SegmentEventArgs(SegmentStage.AtomSegment, sb.ToString()));
        }

        private void OnGenSegGraph(RowFirstDynamicArray<ChainContent> segGraph)
        {
            if (IsSendSegmentEvent == false)
            {
                return;
            }
            SendEvents(new SegmentEventArgs(SegmentStage.GenSegGraph, segGraph.ToString()));
        }

        private void OnGenBiSegGraph(ColumnFirstDynamicArray<ChainContent> biGraph)
        {
            if (IsSendSegmentEvent == false)
            {
                return;
            }
            SendEvents(new SegmentEventArgs(SegmentStage.GenBiSegGraph, biGraph.ToString()));
        }

        private void OnNShortPath(List<int[]> paths, RowFirstDynamicArray<ChainContent> segGraph)
        {
            if (IsSendSegmentEvent == false)
            {
                return;
            }
            List<ChainItem<ChainContent>> list = segGraph.ToListItems();
            string theWord;

            int[] aPath;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < paths.Count; i++)
            {
                aPath = paths[i];
                for (int j = 0; j < aPath.Length; j++)
                {
                    theWord = list[aPath[j]].Content.sWord;
                    if (theWord == "δ##��" || theWord == "δ##��" || theWord == "δ##��" || theWord == "δ##ʱ" || theWord == "δ##��")
                    {
                        for (int k = list[aPath[j]].row; k < list[aPath[j]].col; k++)
                            sb.Append(atomSegment[k].sWord);
                        sb.Append(", ");
                    }
                    else
                        sb.Append(string.Format("{0}, ", list[aPath[j]].Content.sWord));
                }

                sb.Append("\r\n");
            }

            SendEvents(new SegmentEventArgs(SegmentStage.NShortPath, sb.ToString()));
        }

        private void OnBeforeOptimize(List<WordResult[]> m_pWordSeg)
        {
            if (IsSendSegmentEvent == false)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < m_pWordSeg.Count; k++)
            {
                for (int j = 0; j < m_pWordSeg[k].Length; j++)
                    sb.Append(string.Format("{0}, ", m_pWordSeg[k][j].sWord));
                sb.Append("\r\n");
            }

            SendEvents(new SegmentEventArgs(SegmentStage.BeforeOptimize, sb.ToString()));
        }

        private void OnOptimumSegment(RowFirstDynamicArray<ChainContent> m_graphOptimum)
        {
            if (IsSendSegmentEvent == false)
            {
                return;
            }
            SendEvents(new SegmentEventArgs(SegmentStage.OptimumSegment, m_graphOptimum.ToString()));
        }

        private void OnGenBiOptimumSegGraph(ColumnFirstDynamicArray<ChainContent> biOptGraph)
        {
            if (IsSendSegmentEvent==false)
            {
                return;
            }
            SendEvents(new SegmentEventArgs(SegmentStage.GenBiSegGraph, biOptGraph.ToString()));
        }

        #endregion
    }
}
