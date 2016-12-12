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
        public static int[] maskArray;
    }
    class Program
    {
        static void Main(string[] args)
        {
            GlobalVar.maskArray = new int[GlobalVar.M * (GlobalVar.N - 1)];
            GlobalVar.maskArray[0] = BitVector32.CreateMask();
            for (int i = 1; i < GlobalVar.M * (GlobalVar.N - 1); i++)
            {
                GlobalVar.maskArray[i] = BitVector32.CreateMask(GlobalVar.maskArray[i - 1]);
            }

            Dictionary<int, Dictionary<int, List<List<int>>>> HEBPiMap = new Dictionary<int, Dictionary<int, List<List<int>>>>();
            using (BinaryReader readFile = new BinaryReader(File.Open("newHEBPiSet_" + GlobalVar.M + ".bin", FileMode.Open)))
            {
                HEBPiMap = hebpMapFromBinaryFile(readFile);
            }

            int fileNumber = 0;
            using (BinaryReader VEBPFileReader = new BinaryReader(File.Open(GlobalVar.M + "x" + GlobalVar.M + "Files\\" + fileNumber + ".bin", FileMode.Open)))
            {
                using (BinaryWriter EBPFileWriter = new BinaryWriter(File.Open(GlobalVar.M + "x" + GlobalVar.M + "Files\\" + fileNumber + "_OUT.bin", FileMode.Create)))
                {
                    long length = VEBPFileReader.BaseStream.Length;
                    while (VEBPFileReader.BaseStream.Position != length)
                    {
                        try
                        {
                            int VEBPInt = Convert.ToInt32(VEBPFileReader.ReadUInt32());
                            HEBPEnumeration(VEBPInt, ref HEBPiMap, EBPFileWriter);
                        }
                        catch (System.OverflowException)
                        {
                            Console.WriteLine("Overflow in uint-to-int conversion.");
                        }
                    }
                }
            }

            Console.WriteLine("press enter key to finish...");
            Console.ReadKey();
        }

        static void HEBPEnumeration(int VEBPInt, ref Dictionary<int, Dictionary<int, List<List<int>>>> HEBPiMap, BinaryWriter EBPFileWriter)
        {
            //implement HEBPEnumeration with distinctHEBPEnumeration()
            int InitCCInt = 0;
            for (int i = 0; i < GlobalVar.N; i++)
                InitCCInt += i * Convert.ToInt32(Math.Pow(Convert.ToDouble(GlobalVar.N), Convert.ToDouble(i)));//test!

            List<int> VEIntSet = VEIntSetFromVEBPInt(VEBPInt);//test!    

            InitCCInt = HEBPiMap[InitCCInt][VEIntSet[0]][0][1];

            BitVector32 HEBPBits = new BitVector32(0);

            distinctHEBPEnumeration(VEBPInt, ref HEBPBits, 0, ref HEBPiMap, ref VEIntSet, InitCCInt, EBPFileWriter);
                    
        }

        static void distinctHEBPEnumeration(int VEBPInt, ref BitVector32 HEBPBits, int curCol, ref Dictionary<int, 
                                            Dictionary<int, List<List<int>>>> HEBPiMap, ref List<int> VEIntSet, int CCInt, BinaryWriter EBPFileWriter)
        {
            if(curCol == GlobalVar.M - 1)
            {
                BitVector32 VEBPBits = new BitVector32(VEBPInt);
                Console.Write("VEBPBits: " + VEBPBits + "  ,  " + "HEBPBits: " + HEBPBits);
            }
            else
            {
                int VEInt = VEIntSet[curCol + 1];
                foreach( var element in HEBPiMap[CCInt][VEInt])
                {
                    int HEint = element[0];
                    int nextCCint = element[1];
                    BitVector32 tempHEBits = new BitVector32(HEint);
                    //make HEBPBits..
                    for(int i=0; i<GlobalVar.N; i++)
                    {
                        int originalIndex = GlobalVar.maskArray[i];
                        int newIndex = GlobalVar.maskArray[curCol*GlobalVar.N+i];
                        HEBPBits[newIndex] = tempHEBits[originalIndex];
                    }
                    distinctHEBPEnumeration(VEBPInt, ref HEBPBits, curCol + 1, ref HEBPiMap, ref VEIntSet, CCInt, EBPFileWriter);
                }
            }

        }

        static List<int> VEIntSetFromVEBPInt(int VEBPInt)
        {
            List<int> VEIntSet = new List<int>(GlobalVar.M);
            BitVector32 VEBPBits = new BitVector32(VEBPInt);
        
            for(int i=0; i<GlobalVar.M; i++)
            {
                BitVector32 tempVEBits = new BitVector32(0);
                for(int j=0; j<GlobalVar.N-1; j++)
                {
                    int originalIndex = GlobalVar.maskArray[j * GlobalVar.M + i];
                    int newIndex = GlobalVar.maskArray[j];
                    tempVEBits[newIndex] = VEBPBits[originalIndex];
                }
                VEIntSet[i] = tempVEBits.Data;
            }

            return VEIntSet;
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
