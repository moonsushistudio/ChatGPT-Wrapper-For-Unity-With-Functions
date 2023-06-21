using System;
using System.Collections.Generic;

namespace ChatGPTWrapper {

    [Serializable]
    public class Message
    {
        public string role;
        public string content;

        public Message(string r, string c)
        {
            role = r;
            content = c;
        }
    }

    [Serializable]
    public class MessageRes 
    {
        public string role;
        public string content;
        public FunctionCall function_call;

        public MessageRes(string r, string c) {
            role = r;
            content = c;
        }
    }

    [Serializable]
    public class FunctionCall
    {
        public string name;
        public string arguments;
    }
}