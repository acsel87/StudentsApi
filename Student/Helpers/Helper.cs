using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Student.Helpers
{
    public static class Helper
    {
        public static T[] RemoveAtIndex<T>(this T[] array, int index)
        {
            T[] newArray = new T[array.Length - 1];

            int j = 0;

            for (int i = 0; i < array.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }
                
                newArray[j] = array[i];
                j++;                
            }

            return newArray;
        }
    }
}