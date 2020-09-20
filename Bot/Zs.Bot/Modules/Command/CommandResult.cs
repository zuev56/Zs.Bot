namespace Zs.Bot.Modules.Command
{
    /// <summary> 
    /// Contains command execution result
    /// </summary>
    public class CommandResult
    {
        public int ChatIdForAnswer { get; private set; }

        /// <summary> Text result </summary>
        public string Text { get; private set; }

        public CommandResult(int chatIdForAnswer, string result)
        {
            ChatIdForAnswer = chatIdForAnswer;
            Text = result;
        }
    }
}
