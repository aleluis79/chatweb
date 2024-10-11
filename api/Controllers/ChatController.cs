using Microsoft.AspNetCore.Mvc;
using System.Text;
using Ollama;
using System.Text.Json;
using api.Services;
using api.Models;
using System.Net.Http.Headers;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        private readonly IChatService _chatService;

        //private readonly static Uri BASE_URI = new Uri("http://ollama-svc:11434/api");
        private readonly static Uri BASE_URI = new Uri($"http://{Environment.GetEnvironmentVariable("HOST_OLLAMA")}:11434/api");
        
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            using var ollama = new OllamaApiClient(null, BASE_URI);
            var models = await ollama.Models.ListModelsAsync();
            
            // Armar una lista que solo tenga el nombre del modelo
            var modelList = new List<string>();
            if (models.Models != null) {
                foreach (var model in models.Models) {
                    if (model.Model1 != null) {
                        modelList.Add(model.Model1);
                    }
                }
            }
            return Ok(modelList);
        }

        // Upload file
        [HttpPost("upload")]
        public IActionResult UploadFile(string? contextoId, IFormFile? file) {

            Conversation? conversation;

            if (String.IsNullOrEmpty(contextoId)) {
                contextoId = Guid.NewGuid().ToString();
                conversation = _chatService.CreateConversation(Guid.Parse(contextoId));
            } else {
                conversation = _chatService.GetConversation(Guid.Parse(contextoId));
                if (conversation == null) {
                    Console.WriteLine($"Conversation not found: {contextoId}");
                    return Problem("Error al subir el archivo");
                }
            }

            if(file != null && file.Length > 0)
            {
                // El archivo es un pdf?
                if (file.ContentType == "application/pdf") {
                    var texto = "";
                   
                    using (PdfDocument document = PdfDocument.Open(file.OpenReadStream()))
                    {
                        foreach (Page page in document.GetPages())
                        {
                            texto += page.Text;
                        }
                    }

                    conversation.Adjunto = texto;

                    return Ok(contextoId);
                }

            } else {
                conversation.Adjunto = string.Empty;
                conversation.Context = new List<long>();
            }
        
            return Ok();
        }

        [HttpGet("stream-text")]
        public async Task<IActionResult> StreamText(string pregunta, string? contextoId, string model = "llama3.1", string? systemPrompt = null)
        {
            Response.Headers.Append("Content-Type", "application/json");

            //pregunta = "Responde de forma resumida la pregunta: " + pregunta;
            
            var writer = Response.BodyWriter;
            
            Conversation? conversation;

            if (String.IsNullOrEmpty(contextoId)) {
                contextoId = Guid.NewGuid().ToString();
                conversation = _chatService.CreateConversation(Guid.Parse(contextoId));
            } else {
                conversation = _chatService.GetConversation(Guid.Parse(contextoId));
                if (conversation == null) {
                    await writer.CompleteAsync();
                    Console.WriteLine($"Conversation not found: {contextoId}");
                    return new EmptyResult();
                }
            }

            conversation.TokenSource = new CancellationTokenSource();

            if (!String.IsNullOrEmpty(conversation.Adjunto)) {
                pregunta = $"""
                    Usa las siguientes piezas de contexto para responder la pregunta que está al final.
                    Si la pregunta no está en el contexto sólo responde que no lo sabes, no intentes responderla.
                    Responde lo más breve posible.

                    Contexto: {conversation.Adjunto}

                    Pregunta: {pregunta}

                    Respuesta útil:
                    """;
                conversation.Context = new List<long>();
            }

            if (systemPrompt != null) {
                pregunta = systemPrompt + "\n" + pregunta;
            }


            try
            {
                using var ollama = new OllamaApiClient(null, BASE_URI);


                var enumerable = ollama.Completions.GenerateCompletionAsync(model, pregunta, context: conversation.Context, cancellationToken: conversation.TokenSource.Token);
                await foreach (var response in enumerable)
                {
                    //Console.Write($"{response.Response}");
                    var context = response.Context;

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


        [HttpGet("cancel")]
        public IActionResult Cancel(string contextoId) {
            _chatService.CancelToken(Guid.Parse(contextoId));
            return Ok(true);
        }

    }
    
    
}
