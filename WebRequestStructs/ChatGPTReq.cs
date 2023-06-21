using System;
using System.Collections.Generic;

namespace ChatGPTWrapper {
    public struct ChatGPTReq
    {
        public string model;
        public List<Message> messages;
        public int max_tokens;
        public float temperature;
        public List<Function> functions;
        public string function_call;
    }

    public struct ChatGPTReqNoFunc
    {
        public string model;
        public List<Message> messages;
        public int max_tokens;
        public float temperature;
    }

    [System.Serializable]
    public class Function
    {
        public string name;
        public string description;
        public Parameter parameters;
    }

    [System.Serializable]
    public class Parameter
    {
        public string type;
        public Property properties;
        public string[] required;
    }

    [System.Serializable]
    public class Property
    {
        public Detail property1;
        public Detail property2;
        //public Detail unit;
    }

    [System.Serializable]
    public class Detail
    {
        public string type;
        public string description;
        public string[] @enum;
    }
}
