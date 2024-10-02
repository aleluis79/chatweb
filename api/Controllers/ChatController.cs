using Microsoft.AspNetCore.Mvc;
using System.Text;
using Ollama;
using System.Text.Json;
using api.Services;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("stream-text")]
        public async Task<IActionResult> StreamText(string pregunta, string? contextoId)
        {
            Response.Headers.Append("Content-Type", "application/json");

            pregunta = "Responde de forma resumida la pregunta: " + pregunta;

            var writer = Response.BodyWriter;
            
            IList<long>? context = new List<long>();
            if (!String.IsNullOrEmpty(contextoId)) {
                context = this._chatService.GetContext(Guid.Parse(contextoId));
            } else {
                contextoId = Guid.NewGuid().ToString();
            }

            try
            {

                using var ollama = new OllamaApiClient();

                var enumerable = ollama.Completions.GenerateCompletionAsync("llama3.1", pregunta, context: context );
                await foreach (var response in enumerable)
                {
                    //Console.Write($"{response.Response}");
                    context = response.Context;

                    if (context != null)
                    {
                        _chatService.AddContext(Guid.Parse(contextoId), context);
                    }

                    var responseData = new ResponseData
                    {
                        ContextId = contextoId,
                        Chunk = response.Response!
                    };

                    string jsonResponse = JsonSerializer.Serialize(responseData);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);

                    await writer.WriteAsync(responseBytes);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar chunks: {ex.Message}");
            }
            finally
            {
                await writer.CompleteAsync();
            }

            return new EmptyResult();
        }

    }
    
}


public class ResponseData
{
    public string ContextId { get; set; } = string.Empty;
    public string Chunk { get; set; } = string.Empty;
}