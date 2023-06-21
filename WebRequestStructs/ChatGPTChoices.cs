using System;

namespace ChatGPTWrapper {
    [Serializable]
    public struct ChatGPTChoices
    {
        public MessageRes message;
        public string finish_reason; 
    }
}
