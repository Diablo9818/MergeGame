using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static  class IEnumerableExtensions
{
    public static T RandomElement<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        return list.Count > 0 ? list[UnityEngine.Random.Range(0, list.Count)] : default;
    }
}
