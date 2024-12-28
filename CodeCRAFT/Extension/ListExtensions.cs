using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        Random random = new Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1); // 0 ~ i
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
