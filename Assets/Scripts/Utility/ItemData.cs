using System;
using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Item Data", menuName = "New Item Data")]
    public class ItemData : ScriptableObject
    {
        public string ItemName;
        public int DefaultPrice;
        [Tooltip("In Hours")]
        public int TimeToDeliver;
    }
}