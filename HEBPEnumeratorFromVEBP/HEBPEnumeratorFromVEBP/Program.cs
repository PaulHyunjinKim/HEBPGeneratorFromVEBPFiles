using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEBPEnumeratorFromVEBP
{
    public static class GlobalVar
    {
        public const int M = 5;
        public const int N = 5;
        public const int numberOfSameBits = 14;
        public static UInt32 numbOfElements = 0;
    }
    class Program
    {
        static void Main(string[] args)
        {
            int fileNumber = 0;

            Dictionary<int, Dictionary<int, List<List<int>>>> HEBPiMap = new Dictionary<int, Dictionary<int, List<List<int>>>>();
            using (BinaryReader readFile = new BinaryReader(File.Open("newHEBPiSet_" + GlobalVar.M + ".bin", FileMode.Open)))
            {
                HEBPiMap = hebpMapFromBinaryFile(readFile);
            }
            
            using (BinaryReader VEBPFileReader = new BinaryReader(File.Open(GlobalVar.M + "x" + GlobalVar.M + "Files\\" + fileNumber + ".bin", FileMode.Open)))
            {
                long length = VEBPFileReader.BaseStream.Length;
                while(VEBPFileReader.BaseStream.Position != length)
                {
                    UInt32 VEBPInt = VEBPFileReader.ReadUInt32();
                    //HEBPEnumeration
                }
            }

            Console.WriteLine("press enter key to finish...");
            Console.ReadKey();
        }

        static void HEBPEnumeration(UInt32 VEBPInt, ref Dictionary<int, Dictionary<int, List<List<int>>>> HEBPiMap)
        {
            //implement HEBPEnumeration with distinctHEBPEnumeration()
        }



        static Dictionary<int, Dictionary<int, List<List<int>>>> hebpMapFromBinaryFile(BinaryReader readFile)
        {
            Dictionary<int, Dictionary<int, List<List<int>>>> HEBPiMap = new Dictionary<int, Dictionary<int, List<List<int>>>>();
            long length = readFile.BaseStream.Length;

            int CCInt = 0;
            int VEInt = 0;
            int HEInt = 0;
            int nextCCInt = 0;
            
            while(length != readFile.BaseStream.Position)
            {
                CCInt = readFile.ReadInt32();
                VEInt = readFile.ReadInt32();
                HEInt = readFile.ReadInt32();
                nextCCInt = readFile.ReadInt32();

                List<int> tempList = new List<int>();
                tempList.Add(HEInt); tempList.Add(nextCCInt);
                List<List<int>> tempHighVector = new List<List<int>>();
                tempHighVector.Add(tempList);
                Dictionary<int, List<List<int>>> tempMap = new Dictionary<int, List<List<int>>>();
                tempMap.Add(VEInt, tempHighVector);

                if(!HEBPiMap.ContainsKey(CCInt))
                {
                    HEBPiMap.Add(CCInt, tempMap);
                }
                else
                {
                    if(!HEBPiMap[CCInt].ContainsKey(VEInt))
                    {
                        HEBPiMap[CCInt].Add(VEInt, tempHighVector);
                    }
                    else
                    {
                        HEBPiMap[CCInt][VEInt].Add(tempList);
                    }
                }
            }
            return HEBPiMap;
        }
    }
}
