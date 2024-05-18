using System;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Item Data", menuName = "New Item Data")]
    public class ItemData : ScriptableObject
    {
        public string ItemName;
        public string Description;
        [Space]
        public string ItemCode;
        public int DefaultPrice;
    }
}