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
        
        public static void MoveToLayer(Transform root, int layer) {
            root.gameObject.layer = layer;
            foreach(Transform child in root)
                MoveToLayer(child, layer);
        }
    }
}