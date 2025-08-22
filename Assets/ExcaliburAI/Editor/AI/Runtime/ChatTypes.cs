namespace ExcaliburAI.AI
{
    public class ChatMessage
    {
        public string role;
        public string content;
        public ChatMessage() {}
        public ChatMessage(string role, string content) { this.role = role; this.content = content; }
    }

    public interface IChatProvider
    {
        System.Threading.Tasks.Task<string> ChatAsync(
            System.Collections.Generic.IList<ChatMessage> messages,
            bool stream,
            System.Action<string> onDelta,
            int maxTokens,
            float temperature,
            System.Threading.CancellationToken ct
        );
    }

    public interface IImageGenerator
    {
        System.Threading.Tasks.Task<string> Txt2ImgAsync(
            string prompt,
            string negativePrompt,
            int width,
            int height,
            int steps,
            float cfgScale,
            System.Threading.CancellationToken ct
        );
    }
}