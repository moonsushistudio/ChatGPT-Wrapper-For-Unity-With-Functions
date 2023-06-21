using System.Collections.Generic;
using UnityEngine.Events;

namespace ChatGPTWrapper {
    // This is required for older versions of Unity
    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string>{}

    [System.Serializable]
    public class UnityGPTEvent : UnityEvent<string, bool, string> { }
}