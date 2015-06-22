/*
 Measure semantic similarity between two sentences
 (disregards PartOfSpeech tagging and WordSenseDisambiguation)
 Author: Thanh Ngoc Dao - Thanh.dao@gmx.net
 Copyright (c) 2005 by Thanh Ngoc Dao.
*/

/* Cache Facility Added by Dipendra Misra for the project
 * tell-me-Dave
 */

using System;
using System.IO;
using System.Collections.Generic;

namespace WordsMatching
{
	/// <summary>
	/// Measuring relationship between two given sentences
	/// </summary>
	public class SentenceSimilarity
	{
        //private int[] _senses1, _senses2;
        //float[,] _similarity;
		String[] _source, _target;
		private int m, n;

		Tokeniser tok;
		HeuristicMatcher match;
		List<String> cache1 = new List<String> ();
		List<String> cache2 = new List<String> ();
		List<float> cache3 = new List<float> ();
		public int cacheHit=0, cacheMiss=0;
		String path = "";

		public SentenceSimilarity()
		{
			this.path = "./src/wordnetdotnet/";//@"/home/dipendra/Research/";
			Wnlib.WNCommon.path = path + /*"verbgrounding/wordnetdotnet/dict/";*/"dict/";
			Console.WriteLine 		(Wnlib.WNCommon.path);
			StopWordsHandler stopword=new StopWordsHandler() ;

			tok=new Tokeniser() ;
			tok.UseStemming = false;
			match=new HeuristicMatcher() ;
		}

        private MyWordInfo[] Disambiguate(string[] words)
        {
            if (words.Length == 0) return null;

            MyWordInfo[] wordInfos=new MyWordInfo [words.Length];
            
            for (int i = 0; i < words.Length; i++)
            {
                
                WnLexicon.WordInfo wordInfo = WnLexicon.Lexicon.FindWordInfo(words[i], true);

                if (wordInfo.partOfSpeech != Wnlib.PartsOfSpeech.Unknown)
                {
                    if (wordInfo.text != string.Empty)
                        words[i] = wordInfo.text;

                    Wnlib.PartsOfSpeech[] posEnum = (Wnlib.PartsOfSpeech[])Enum.GetValues(typeof(Wnlib.PartsOfSpeech));

                    for (int j = 0; j < posEnum.Length; j++)
                    {
                        if (wordInfo.senseCounts[j] > 0) // get the first part of speech
                        {
                            wordInfos[i] = new MyWordInfo(words[i], posEnum[j]);                             
                            break;
                        }
                    }
                }
            }

            WordSenseDisambiguator wsd = new WordSenseDisambiguator();
            wordInfos=wsd.Disambiguate(wordInfos);

            return wordInfos;
        }


        //MyWordInfo[] _myWordsInfo_i, _myWordsInfo_j;        
        //private void MyInitOld()
        //{
        //    _myWordsInfo1 = Disambiguate(_source);
        //    _myWordsInfo2 = Disambiguate(_target);

        //    m = _myWordsInfo1.Length; n = _myWordsInfo2.Length;
        //    _similarity =new float[m, n] ;

        //    for (int i=0; i < m; i++)
        //    {
        //        _myWordsInfo1[i].Sense = _myWordsInfo1[i].Sense < 0 ? 0 : _myWordsInfo1[i].Sense;                

        //        string word1 = _source[i];
        //        for (int j=0; j < n; j++)
        //        {
        //            _myWordsInfo2[i].Sense = _myWordsInfo2[i].Sense < 0 ? 0 : _myWordsInfo2[i].Sense;					

        //            string word2=_target[j];
        //            WordDistance distance = new WordDistance();
        //            float weight = distance.GetSimilarity(_myWordsInfo1[i], _myWordsInfo2[j]);					

        //            _similarity[i, j]=weight;					
        //        }
        //    }
        //}



        float[][] GetSimilarityMatrix(string[] string1, string[] string2)
        {
            m = string1.Length; n = string2.Length;            
            float[][] simMatrix = new float[m][];            
            
            Wnlib.PartsOfSpeech[] POSEnum = (Wnlib.PartsOfSpeech[])Enum.GetValues(typeof(Wnlib.PartsOfSpeech));
        	HierarchicalWordData[][] wordData_1 = new HierarchicalWordData[m][];
        	HierarchicalWordData[][] wordData_2 = new HierarchicalWordData[n][];
            for (int i = 0; i < m; i++) 
                simMatrix[i] = new float[n];

            for (int i = 0; i < m; i++)
                wordData_1[i] = new HierarchicalWordData[POSEnum.Length];
            for (int j = 0; j < n; j++)
                wordData_2[j] = new HierarchicalWordData[POSEnum.Length];

            for (int i = 0; i < m; i++)             
            {                                                                                
                for (int j = 0; j < n; j++)
                {
                    float synDist = AcronymChecker.GetEditDistanceSimilarity(string1[i], string2[j]);

                    for (int partOfSpeech = 1; partOfSpeech < POSEnum.Length; partOfSpeech++)
                    {
                         if (wordData_1[i][partOfSpeech] == null)
                         {
                             MyWordInfo myWordsInfo_i = new MyWordInfo(string1[i], POSEnum[partOfSpeech]);
                             wordData_1[i][partOfSpeech] = new HierarchicalWordData(myWordsInfo_i);
                         }
                         if (wordData_2[j][partOfSpeech] == null)
                         {
                             MyWordInfo myWordsInfo_j = new MyWordInfo(string2[j], POSEnum[partOfSpeech]);
                             wordData_2[j][partOfSpeech] = new HierarchicalWordData(myWordsInfo_j);
                         }

                         WordSimilarity wordDistance = new WordSimilarity();
                         float semDist = wordDistance.GetSimilarity(wordData_1[i][partOfSpeech], wordData_2[j][partOfSpeech]);
                         float weight = Math.Max(synDist, semDist);
                         if (simMatrix[i][j] < weight)
                             simMatrix[i][j] = weight;                    
                    }
                }                                    
            }            
         
         return simMatrix;
      }

	  public void bootStrapCache()
	  {
			return; //no longer supported
			System.IO.StreamReader sw = new System.IO.StreamReader (this.path + "ProjectCompton/sentenceSim.txt");
			String s = sw.ReadLine ();
			String[] split = s.Split (new char[] {':'});
			cache1.Add (split [0].Substring (0, split [0].Length - 1));
			cache2.Add (split [1].Substring (1, split [1].Length - 2));
			cache3.Add (float.Parse(split [2].Substring (1, split [2].Length - 1)));
			sw.Close ();
	  }

	  public void storeCache()
	  {
			return;
			System.IO.StreamWriter sw = new System.IO.StreamWriter (this.path + "ProjectCompton/sentenceSim.txt");
			for (int i=0; i<cache1.Count; i++) 
				sw.WriteLine (cache1[i].Replace('\n','_')+" : "+cache2[i].Replace('\n','_')+" : "+cache3[i].ToString());
			Console.WriteLine ("Cache is = "+cache1.Count);
			sw.Flush ();
			sw.Close ();
	  }

	  private float findInCache(String a, String b)
	  {
			for (int i=0; i<cache1.Count; i++) 
			{
				if (cache1 [i].Equals (a) && cache2 [i].Equals (b)) 
				{
					this.cacheHit++;
					return cache3 [i];
				}
			}
			cacheMiss++;
			return -1;
	  }

	  private void addToCache(String a, String b, float c)
	  {
			//Function Description: adds to cache
			if (cache1.Count > 5000) //cache limit
				return;
			cache1.Add (a);
			cache2.Add (b);
			cache3.Add (c);
	  }

	  public float GetScore(string string1, string string2)		
	  {			
			float result = findInCache (string1, string2);
			if (result != -1)
				return result;
			_source=tok.Partition(string1) ;
			_target=tok.Partition(string2) ;
			if (_source.Length == 0 || _target.Length == 0 )
				return 0F;
        	float[][] simMatrix = GetSimilarityMatrix(_source, _target);		
			//float score = HeuristicMatcher.ComputeSetSimilarity(simMatrix, 2, 0.3F);
        	float score = HeuristicMatcher.ComputeSetSimilarity(simMatrix, 1);
			addToCache (string1, string2, score);
			return score;	
	   }
	}
}
