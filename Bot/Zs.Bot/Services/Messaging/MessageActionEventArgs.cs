﻿using System;
using Zs.Bot.Model.Abstractions;
using Zs.Common.Enums;

namespace Zs.Bot.Services.Messaging
{
    public class MessageActionEventArgs : EventArgs
    {
        public IMessage Message { get; set; }
        public IChat Chat { get; set; }
        public IUser User { get; set; }
        public ChatType ChatType { get; set; }
        public MessageAction Action { get; set; }
    }
}