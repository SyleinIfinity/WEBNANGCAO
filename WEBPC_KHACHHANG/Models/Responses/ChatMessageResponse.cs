using System;

namespace WEBPC_KHACHHANG.Models.Responses
{
    public class ChatMessageResponse
    {
        public string Role { get; set; }    // "user" hoặc "model"
        public string Content { get; set; } // Nội dung tin nhắn
        public DateTime Time { get; set; }
    }
}