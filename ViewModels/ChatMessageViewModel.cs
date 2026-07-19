using System;

namespace ECommerceWeb.ViewModels
{
    public class ChatMessageViewModel
    {
        public int ChatMessageID { get; set; }
        public string Message { get; set; }
        public string SenderType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}