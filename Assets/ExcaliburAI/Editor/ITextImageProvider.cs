using System.Threading.Tasks;

namespace ExcaliburAI.Editor
{
    public interface ITextImageProvider
    {
        Task<string> GenerateTextJson(string systemPrompt, string userPrompt);
        Task<byte[]> GeneratePng(string prompt, int size = 512);
        string ProviderName { get; }
    }

    // Temporary no-network provider so the window works immediately.
    public class DummyProvider : ITextImageProvider
    {
        public string ProviderName => "Dummy";

        public Task<string> GenerateTextJson(string systemPrompt, string userPrompt)
        {
            // Minimal, valid example JSON matching our window’s schema.
            var json = @"{
              ""weapons"": [
                { ""id"": ""w_sword_001"", ""name"": ""Bronze Shortsword"", ""tier"": ""Common"", ""type"": ""Sword"", ""minDamage"": 3, ""maxDamage"": 6, ""attackSpeed"": 1.2, ""description"": ""A simple shortsword."", ""iconPrompt"": ""fantasy bronze shortsword, simple"" }
              ],
              ""classes"": [
                { ""id"": ""c_guardian"", ""name"": ""Guardian"", ""role"": ""Tank"", ""primaryStats"": ""STR/VIT"", ""pitch"": ""Shield-bearing protector of the realm."", ""iconPrompt"": ""tower shield sigil"" }
              ]
            }";
            return Task.FromResult(json);
        }

        public Task<byte[]> GeneratePng(string prompt, int size = 512)
        {
            // Return null so the window will create a placeholder icon.
            return Task.FromResult<byte[]>(null);
        }
    }
}
