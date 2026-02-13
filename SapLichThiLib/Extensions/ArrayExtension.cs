using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiLib.Extensions
{
    public static class ArrayExtension
    {
        public static float GetMaxOfArray(this float[,] array)
        {
            if (array == null) { throw new ArgumentNullException("array");}
            float max = float.MinValue;
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    if (array[i, j] > max) max = array[i, j];
            return max;
        }
        /// <summary>
        /// Tra ve gia tri max va cac index tai vi tri max them dieu kien gia tri toi da
        /// </summary>
        /// <param name="array"> mang cac so can tim max</param>
        /// <param name="condition"> gia tri toi da</param>
        /// <param name="index1">index hang</param>
        /// <param name="index2">index cot</param>
        /// <returns>Gia tri Max co dieu kien va cac gia tri index</returns>
        /// <exception cref="ArgumentNullException">Array = null thi throw</exception>
        public static float GetMaxOfArrayWithCondition(this float[,] array, float condition, out int index1, out int index2)
        {
            if (array == null) { throw new ArgumentNullException("array"); }
            float max = float.MinValue;
            index1 = -1; index2 = -1;
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    if (array[i, j] > max && array[i, j] < condition)
                    {
                        max = array[i, j];
                        index1 = i; index2 = j;
                    }
            return max;
        }

        public static float ChangeArrayWithCondition(this float[,] array,float condition)
        {
            if (array == null) { throw new ArgumentNullException("array"); }
            float max = float.MinValue;
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i,j] > condition)
                        array[i, j] = float.NegativeInfinity;
                }
            return max;
        }

        public static void ChangeArrayAtIndex(this float[,] array, int index1, int index2)
        {
            if (array == null) { throw new ArgumentNullException("array"); }
            
            for (int i = 0; i < array.GetLength(0); i++)
            {
                if (i != index1)
                    array[i, index2] = float.NegativeInfinity;
            }
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if(j != index2)
                    array[index1, j] = float.NegativeInfinity;
            }
            array[index1,index2] = 100f;
        }

        public static bool CheckArrayValid(this float[,] array) 
        {
            int lenght0 = array.GetLength(0);
            int lenght1 = array.GetLength(1);
            int[] rowCount = new int[lenght0];
            int[] colCount = new int[lenght1];
            for (int i = 0; i < lenght0; i++)
            {
                for (int j = 0; j < lenght1; j++)
                {
                    if (array[i, j] > 0)
                    {
                        rowCount[i]++;
                        colCount[j]++;
                    }
                }
            }
            if (rowCount.Max() > 1 || colCount.Max() > 1)
            {
                return false;
            }
            else return true;
        }

        static int count = 0;
        public static void WriteArrayToFile(this float[,] array)
        {
            string path = $"Test/arrayValueTest{count++}.txt";
            using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate)))
            {
                int size1 = array.GetLength(0);
                int size2 = array.GetLength(1);
                sw.Write($"chieu dai canh 1: {size1} \n");
                sw.Write($"chieu dai canh 2: {size2} \n");

                for (int i = 0; i < size1; i++)
                {
                    for (int j = 0; j < size2; j++)
                    {
                        sw.Write(array[i, j].ToString("0.00") + "\t");
                    }
                    sw.WriteLine();
                }
                sw.Close();
            }
            
        }

        static int count2 = 0;
        public static void WriteArrayToFile2(this float[,] array)
        {
            string path = $"Test2/arrayValueTest{count2++}.txt";
            using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate)))
            {
                int size1 = array.GetLength(0);
                int size2 = array.GetLength(1);
                sw.Write($"chieu dai canh 1: {size1} \n");
                sw.Write($"chieu dai canh 2: {size2} \n");
                for (int i = 0; i < size1; i++)
                {
                    for (int j = 0; j < size2; j++)
                    {
                        sw.Write(array[i, j].ToString("0.00") + "\t");
                    }
                    sw.WriteLine();
                }
                sw.Close();
            }

        }

    }
}
