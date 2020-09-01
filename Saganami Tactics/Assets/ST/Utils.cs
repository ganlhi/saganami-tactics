using System;
using System.Collections;
using UnityEngine;

namespace ST
{
    public class Utils
    {
        public static string GenerateId()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "")
                .Replace("+", "-")
                .Replace("/", "_");
        }

        public static IEnumerator DelayedAction(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action.Invoke();
        }
    }
}