using System.Collections.Generic;
using FunPress.Common.Types.Enums;

namespace FunPress.Views.Params
{
    public class MessageDialogParam
    {
        public string Message { get; }
        public IEnumerable<MessageDialogButton> Buttons { get; }

        public MessageDialogParam(string message, IEnumerable<MessageDialogButton> buttons)
        {
            Message = message;
            Buttons = buttons;
        }
    }
}
