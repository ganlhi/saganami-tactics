using System;
using UnityEngine;

namespace ST.Common
{
    public static class LanguageExtensions
    {

        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
            }

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        public static float DistanceTo(this Vector3 a, Vector3 b)
        {
            return (b - a).magnitude;
        }

        public static Vector3 DirectionTo(this Vector3 a, Vector3 b)
        {
            return (b - a).normalized;
        }
    }
}