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
        public static int M = 0;
        public static int N = 0;
        public const int numberOfSameBits = 24;
        public static UInt32 numbOfElements = 0;
        public static int[] maskArray;
        public static int numberOfFiles = 3;
    }
    class Program
    {
        static void Main(string[] args)
        {
            GlobalVar.M = 3;//int.Parse(args[0]);
            GlobalVar.N = 3;//int.Parse(args[1]);
            int fileNumber = 0;//int.Parse(args[2]);

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


            for(int i =0; i<GlobalVar.numberOfFiles; i++)
            {
                fileNumber = i;
                
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
            }
            Console.WriteLine("number of EBPs: "+GlobalVar.numbOfElements);
            Console.WriteLine("press enter key to finish...");
            Console.ReadKey();
        }

        static void HEBPEnumeration(int VEBPInt, ref Dictionary<int, Dictionary<int, List<List<int>>>> HEBPiMap, BinaryWriter EBPFileWriter)
        {
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
                bool HEBPIsDistinct = true;
                int HEBPInt = HEBPBits.Data;
                //BitVector32 VEBPBits = new BitVector32(VEBPInt);
                //Console.Write("VEBPBits: " + VEBPBits + "  ,  " + "HEBPBits: " + HEBPBits);
                HEBPIsDistinct = checkIfDistinctHEBP(VEBPInt, HEBPInt);
                //Console.WriteLine("symm "+HEBPIsDistinct);
                if((GlobalVar.M == 3 || GlobalVar.M == 5)&& HEBPIsDistinct)
                {
                    HEBPIsDistinct = checkRotationalSymmetryOfEBP(VEBPInt, HEBPInt);
                    //Console.WriteLine("rotate "+HEBPIsDistinct);
                }
                if(HEBPIsDistinct)
                {
                    GlobalVar.numbOfElements++;
                    BitVector32 VEBPBits = new BitVector32(VEBPInt);
                    //Console.Write("VEBPBits: " + VEBPBits + "  ,  " + "HEBPBits: " + HEBPBits);
                    //Console.WriteLine();
                    ////EBPFileWriter.Write(VEBPInt);
                    ////EBPFileWriter.Write(HEBPBits.Data);
                    //BitVector32 tempVEBPBits = new BitVector32(reverseIndexOfEBP(VEBPInt));
                    //BitVector32 tempHEBPBits = new BitVector32(reverseIndexOfEBP(HEBPBits.Data));
                    EBPFileWriter.Write(reverseIndexOfEBP(VEBPInt));
                    EBPFileWriter.Write(reverseIndexOfEBP(HEBPBits.Data));
                    //need to write a file.
                    //Console.Write("newVEBPBits: " + tempVEBPBits + " ," + "newHEBPBits: " + tempHEBPBits);
                    //Console.WriteLine();
                    //Console.WriteLine();
                    //Console.ReadKey();
                }
            }
            else
            {
                int VEInt = VEIntSet[curCol + 1];
                //bool checkZero = false;
                foreach( var element in HEBPiMap[CCInt][VEInt])
                {
                    int HEint = element[0];
                    int nextCCint = element[1];
                    //Console.WriteLine("HEInt: "+HEint+" , CCInt:"+nextCCint);
                    BitVector32 tempHEBits = new BitVector32(HEint);
                    //make HEBPBits..
                    for(int i=0; i<GlobalVar.N; i++)
                    {
                        int originalIndex = GlobalVar.maskArray[i];
                        int newIndex = GlobalVar.maskArray[curCol*GlobalVar.N+i];
                        HEBPBits[newIndex] = tempHEBits[originalIndex];
                    }
                    if(curCol == GlobalVar.M - 2)
                    {
                        if(nextCCint == 0)
                            distinctHEBPEnumeration(VEBPInt, ref HEBPBits, curCol + 1, ref HEBPiMap, ref VEIntSet, nextCCint, EBPFileWriter);
                    }
                    else
                        distinctHEBPEnumeration(VEBPInt, ref HEBPBits, curCol + 1, ref HEBPiMap, ref VEIntSet, nextCCint, EBPFileWriter);
                }
            }

        }

        static bool checkIfDistinctHEBP(int VEBPInt, int HEBPInt)
        {
            bool checkedResult = true;

            int[] symmetricVEBPInts = new int[3];
            int[] symmetricHEBPInts = new int[3];

            symmetricVEBPInts[0] = inverseEBP(VEBPInt);
            symmetricVEBPInts[1] = switchEBP(VEBPInt);
            symmetricVEBPInts[2] = invSwitchEBP(VEBPInt);

            symmetricHEBPInts[0] = switchEBP(HEBPInt);
            symmetricHEBPInts[1] = inverseEBP(HEBPInt);
            symmetricHEBPInts[2] = invSwitchEBP(HEBPInt);

            for(int i = 0; i<3; i++)
            {
                if(VEBPInt == symmetricVEBPInts[i])
                {
                    //Console.WriteLine("i: " + i + "HEBPInt: " + HEBPInt + " symHEBP " + symmetricHEBPInts[i]);
                    if (HEBPInt == maxEBP(HEBPInt, symmetricHEBPInts[i]))//Math.Max(HEBPInt, symmetricHEBPInts[i]))
                        continue;
                    else
                    {
                        checkedResult = false;
                        break;
                    }
                }
            }

            return checkedResult;
        }

        static bool checkRotationalSymmetryOfEBP(int VEBPInt, int HEBPInt)
        {
            bool checkedResult = true;
            int[] symmetricHEBPs = new int[4];
            int[] symmetricVEBPs = new int[4];

            symmetricHEBPs[0] = rotateHEBP(HEBPInt);
            symmetricHEBPs[1] = switchEBP(symmetricHEBPs[0]);
            symmetricHEBPs[2] = inverseEBP(symmetricHEBPs[0]);
            symmetricHEBPs[3] = invSwitchEBP(symmetricHEBPs[0]);

            symmetricVEBPs[0] = rotateVEBP(VEBPInt);
            symmetricVEBPs[1] = inverseEBP(symmetricVEBPs[0]);
            symmetricVEBPs[2] = switchEBP(symmetricVEBPs[0]);
            symmetricVEBPs[3] = invSwitchEBP(symmetricVEBPs[0]);

            for(int i=0; i<4; i++)
            {
                if(VEBPInt == maxEBP(VEBPInt, symmetricHEBPs[i]))
                {
                    if(VEBPInt == symmetricHEBPs[i])
                    {
                        if (HEBPInt == maxEBP(HEBPInt, symmetricVEBPs[i]))
                            continue;
                        else
                        {
                            checkedResult = false;
                            break;
                        }
                    }
                }
                else
                {
                    checkedResult = false;
                    break;
                }
            }
            return checkedResult;
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
                
                VEIntSet.Add( tempVEBits.Data);
            }
            return VEIntSet;
        }

        static int reverseIndexOfEBP(int EBPInt)
        {
            int resultInt = 0;
            BitVector32 EBPBits = new BitVector32(EBPInt);
            BitVector32 tempEBPBits = new BitVector32(0);

            for(int i=0; i<GlobalVar.M; i++)
            {
                for(int j=0; j<GlobalVar.N-1; j++)
                {
                    int originalIndex = GlobalVar.maskArray[j * GlobalVar.M + i];
                    int newIndex = GlobalVar.maskArray[i * (GlobalVar.N - 1) + j];
                    tempEBPBits[newIndex] = EBPBits[originalIndex];
                }
            }
            
            resultInt = tempEBPBits.Data;
            return resultInt;
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

        static int inverseEBP(int EBPInt)
        {
            int resultInt = 0;
            BitVector32 EBPBits = new BitVector32(EBPInt);
            BitVector32 tempEBPBits = new BitVector32(0);

            for (int index = 0; index < (GlobalVar.M - 1) * GlobalVar.N; index++)
            {
                int i = index / GlobalVar.M;
                int j = index % GlobalVar.M;
                int newIndex = GlobalVar.M * (1 + i) - j - 1; //GlobalVar.M - 1 - j + GlobalVar.M * i;
                tempEBPBits[GlobalVar.maskArray[newIndex]] = EBPBits[GlobalVar.maskArray[index]];
            }

            resultInt = tempEBPBits.Data;
            return resultInt;
        }

        static int switchEBP(int EBPInt)
        {
            int resultInt = 0;
            BitVector32 EBPBits = new BitVector32(EBPInt);
            BitVector32 tempEBPBits = new BitVector32(0);

            for (int index = 0; index < (GlobalVar.M - 1) * GlobalVar.N; index++)
            {
                int i = index % GlobalVar.M;
                int j = index / GlobalVar.M;
                int newJ = GlobalVar.N - 1 - j;
                int newIndex = (newJ - 1) * GlobalVar.M + i;
                tempEBPBits[GlobalVar.maskArray[newIndex]] = EBPBits[GlobalVar.maskArray[index]];

            }

            resultInt = tempEBPBits.Data;
            return resultInt;
        }

        static int invSwitchEBP(int EBPInt)
        {
            int resultInt = 0;
            resultInt = inverseEBP(switchEBP(EBPInt));

            return resultInt;
        }

        static int rotateHEBP(int HEBPInt)
        {
            int resultInt = 0;
            resultInt = inverseEBP(HEBPInt);

            return resultInt;
        }

        static int rotateVEBP(int VEBPInt)
        {
            int resultInt = 0;
            resultInt = switchEBP(VEBPInt);

            return resultInt;
        }

        static int maxEBP(int EBP1, int EBP2)
        {
            int MAXEBP = EBP1;
            BitVector32 EBP1Bits = new BitVector32(EBP1);
            BitVector32 EBP2Bits = new BitVector32(EBP2);
            

            for (int i = 0; i < GlobalVar.M * (GlobalVar.N - 1); i++)
            {
                int EBP1Bit = 0;
                int EBP2Bit = 0;
                if (EBP1Bits[GlobalVar.maskArray[i]]) EBP1Bit = 1;
                if (EBP2Bits[GlobalVar.maskArray[i]]) EBP2Bit = 1;
                if (EBP1Bit > EBP2Bit)
                {
                    MAXEBP = EBP1;
                    break;
                }
                else if (EBP1Bit < EBP2Bit)
                {
                    MAXEBP = EBP2;
                    break;
                }

            }

            return MAXEBP;
        }
    }
}
