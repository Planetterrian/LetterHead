using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WeightedList<T> : List<T> where T: IWeightedListItem
{
    public T GetRandomItem()
    {
        var total = 0f;
        for (int i = 0; i < Count; i++)
        {
            total += this[i].Weight();
        }

        var arrow = UnityEngine.Random.Range(0f, total);
        var cur = 0f;
        for (int i = 0; i < Count; i++)
        {
            cur += this[i].Weight();
            if (arrow <= cur)
            {
                return this[i];
            }
        }

        return default(T);
    }

    public WeightedList(IEnumerable<T> fromList) : base(fromList)
    {
    }

    public WeightedList() : base()
    {
    }

}

public interface IWeightedListItem
{
    float Weight();
}

public static class WeightedListExtentions
{
    public static WeightedList<T> ToWeightedList<T>(this IEnumerable<T> enumerable) where T : IWeightedListItem
    {
        return new WeightedList<T>(enumerable);
    }
}