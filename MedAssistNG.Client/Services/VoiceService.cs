using Microsoft.JSInterop;

namespace MedAssistNG.Client.Services
{
    public class VoiceService
    {
        private readonly IJSRuntime _js;

        public VoiceService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task StartListening(object dotnetRef)
        {
            await _js.InvokeVoidAsync("voiceInput.start", dotnetRef);
        }
    }
}