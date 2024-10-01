using Microsoft.AspNetCore.Mvc;
using System.Text;
using Ollama;
using System.Text.Json;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        [HttpGet("stream-text")]
        public async Task<IActionResult> StreamText(string pregunta, string? contexto)
        {
            Response.Headers.Append("Content-Type", "application/json");

            var writer = Response.BodyWriter;
            
            IList<long>? context = new List<long>();
            if (contexto != null) {
                context = ConvertStringToLongList(contexto);
            }


            try
            {

                using var ollama = new OllamaApiClient();

                var enumerable = ollama.Completions.GenerateCompletionAsync("llama3.2", pregunta, context: context );
                await foreach (var response in enumerable)
                {
                    //Console.Write($"{response.Response}");
                    context = response.Context;

                    var responseData = new ResponseData
                    {
                        Context = context,
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

        static List<long> ConvertStringToLongList(string input)
        {
            // Separar el string y convertir cada parte a long
            return input.Split(',')
                        .Select(part => long.Parse(part.Trim())) // Usar Trim para eliminar espacios en blanco
                        .ToList();
        }

    }
    
}


public class ResponseData
{
    public IList<long>? Context { get; set; }
    public string Chunk { get; set; } = string.Empty;
}