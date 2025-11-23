using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 可序列化的波峰数据对
    /// 用于在Unity Inspector中编辑波数据
    /// </summary>
    [Serializable]
    public class SerializablePeakData
    {
        public int position;
        public int value;

        public SerializablePeakData(int position, int value)
        {
            this.position = position;
            this.value = value;
        }
    }
}

