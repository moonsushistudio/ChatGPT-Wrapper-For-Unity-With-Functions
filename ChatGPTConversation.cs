using UnityEngine;
using System.Collections.Generic;
using Reqs;
using System.IO;

namespace ChatGPTWrapper
{

    public class ChatGPTConversation : MonoBehaviour
    {
        [SerializeField]
        private bool _useProxy = false;
        [SerializeField]
        private string _proxyUri = null;

        [SerializeField]
        private string _apiKey = null;

        public enum Model
        {
            ChatGPT4,
            ChatGPT,
            Davinci,
            Curie
        }
        [SerializeField]
        public Model _model = Model.ChatGPT;
        private string _selectedModel = null;
        [SerializeField]
        private int _maxTokens = 500;
        [SerializeField]
        private float _temperature = 0.5f;

        private string _uri;
        private List<(string, string)> _reqHeaders;


        private Requests requests = new Requests();
        private Prompt _prompt;
        private Chat _chat;
        private string _lastUserMsg;
        private string _lastChatGPTMsg;

        [SerializeField]
        private string _chatbotName = "ChatGPT";

        [TextArea(4, 6)]
        [SerializeField]
        private string _initialPrompt = "You are ChatGPT, a large language model trained by OpenAI.";

        public UnityGPTEvent chatGPTResponse = new UnityGPTEvent();
        private string args;

        private void OnEnable()
        {

            TextAsset textAsset = Resources.Load<TextAsset>("APIKEY");
            if (textAsset != null)
            {
                _apiKey = textAsset.text;
            }


            _reqHeaders = new List<(string, string)>
            {
                ("Authorization", $"Bearer {_apiKey}"),
                ("Content-Type", "application/json")
            };
            switch (_model)
            {
                case Model.ChatGPT:
                    _chat = new Chat(_initialPrompt);
                    _uri = "https://api.openai.com/v1/chat/completions";
                    _selectedModel = "gpt-3.5-turbo-0613";
                    break;
                case Model.ChatGPT4:
                    _chat = new Chat(_initialPrompt);
                    _uri = "https://api.openai.com/v1/chat/completions";
                    _selectedModel = "gpt-4";
                    break;
                case Model.Davinci:
                    _prompt = new Prompt(_chatbotName, _initialPrompt);
                    _uri = "https://api.openai.com/v1/completions";
                    _selectedModel = "text-davinci-003";
                    break;
                case Model.Curie:
                    _prompt = new Prompt(_chatbotName, _initialPrompt);
                    _uri = "https://api.openai.com/v1/completions";
                    _selectedModel = "text-curie-001";
                    break;
            }
        }

        public void ResetChat(string initialPrompt)
        {
            switch (_model)
            {
                case Model.ChatGPT:
                    _chat = new Chat(initialPrompt);
                    break;
                case Model.ChatGPT4:
                    _chat = new Chat(initialPrompt);
                    break;
                default:
                    _prompt = new Prompt(_chatbotName, initialPrompt);
                    break;
            }
        }

        public void AddUserMessageHistory(string userHistoryChat)
        {
            _chat.AppendMessage(Chat.Speaker.User, userHistoryChat);
        }

        public void AddChatGPTMessageHistory(string gptHistoryChat)
        {
            _chat.AppendMessage(Chat.Speaker.ChatGPT, gptHistoryChat);
        }

        public void SendToChatGPT(string message, List<Function> functions,string requiredFunctionMode)
        {
            _lastUserMsg = message;
           
            if (_model == Model.ChatGPT || _model == Model.ChatGPT4)
            {
                if (_useProxy)
                {
                    ProxyReq proxyReq = new ProxyReq();
                    proxyReq.max_tokens = _maxTokens;
                    proxyReq.temperature = _temperature;
                    proxyReq.messages = new List<Message>(_chat.CurrentChat);
                    proxyReq.messages.Add(new Message("user", message));

                    string proxyJson = JsonUtility.ToJson(proxyReq);

                    StartCoroutine(requests.PostReq<ChatGPTRes>(_proxyUri, proxyJson, ResolveChatGPT, _reqHeaders));
                }
                else
                {
                    if (functions != null && functions.Count > 0)
                    {
                        ChatGPTReq chatGPTReq = new ChatGPTReq();
                        chatGPTReq.model = _selectedModel;
                        chatGPTReq.max_tokens = _maxTokens;
                        chatGPTReq.temperature = _temperature;
                        chatGPTReq.messages = _chat.CurrentChat;
                        chatGPTReq.functions = functions;
                        chatGPTReq.function_call = requiredFunctionMode;
                        chatGPTReq.messages.Add(new Message("user", message));
                        string chatGPTJson = JsonUtility.ToJson(chatGPTReq);
                        Log(chatGPTJson);
                        StartCoroutine(requests.PostReq<ChatGPTRes>(_uri, chatGPTJson, ResolveChatGPT, _reqHeaders));
                    }
                    else 
                    {
                        ChatGPTReqNoFunc chatGPTReq = new ChatGPTReqNoFunc();
                        chatGPTReq.model = _selectedModel;
                        chatGPTReq.max_tokens = _maxTokens;
                        chatGPTReq.temperature = _temperature;
                        chatGPTReq.messages = _chat.CurrentChat;
                        chatGPTReq.messages.Add(new Message("user", message));
                        string chatGPTJson = JsonUtility.ToJson(chatGPTReq);
                        Log(chatGPTJson);
                        StartCoroutine(requests.PostReq<ChatGPTRes>(_uri, chatGPTJson, ResolveChatGPT, _reqHeaders));
                    }
                }
            }
            else 
            {
                _prompt.AppendText(Prompt.Speaker.User, message);

                GPTReq reqObj = new GPTReq();
                reqObj.model = _selectedModel;
                reqObj.prompt = _prompt.CurrentPrompt;
                reqObj.max_tokens = _maxTokens;
                reqObj.temperature = _temperature;
                string json = JsonUtility.ToJson(reqObj);

                StartCoroutine(requests.PostReq<GPTRes>(_uri, json, ResolveGPT, _reqHeaders));
            }
        }

        private void ResolveChatGPT(ChatGPTRes res)
        {
            bool functioncall = false;
            if (res.choices[0].finish_reason == "function_call")
            {
                functioncall = true;
                _lastChatGPTMsg = res.choices[0].message.function_call.name;
                args = res.choices[0].message.function_call.arguments;
            }
            else 
            {
                _lastChatGPTMsg = res.choices[0].message.content;
                _chat.AppendMessage(Chat.Speaker.User, _lastUserMsg);
                _chat.AppendMessage(Chat.Speaker.ChatGPT, _lastChatGPTMsg);
            }
            chatGPTResponse.Invoke(_lastChatGPTMsg, functioncall,args);
        }

        private void ResolveGPT(GPTRes res)
        {
            _lastChatGPTMsg = res.choices[0].text
                .TrimStart('\n')
                .Replace("<|im_end|>", "");

            _prompt.AppendText(Prompt.Speaker.Bot, _lastChatGPTMsg);
            chatGPTResponse.Invoke(_lastChatGPTMsg,false, args);
        }

        private void Log(string message)
        {
            using (StreamWriter writer = new StreamWriter("chatgptlogfile.txt", true)) // true to append data to the file
            {
                writer.WriteLine(message);
            }
        }
    }
}
