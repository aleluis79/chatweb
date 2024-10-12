using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using api.Services;
using api.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using OllamaSharp;
using OllamaSharp.Models;

namespace api.Controllers
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
            var ollama = new OllamaApiClient(BASE_URI);
            var models = await ollama.ListLocalModels();
            
            // Armar una lista que solo tenga el nombre del modelo
            var modelList = new List<string>();
            if (models != null) {
                foreach (var model in models) {
                    if (model.Name != null) {
                        modelList.Add(model.Name);
                    }
                }
            }
            return Ok(modelList);
        }

        // Upload file
        [HttpPost("upload")]
        public IActionResult UploadFile(string? contextoId, IFormFile? file) {

            if (String.IsNullOrEmpty(contextoId)) {
                contextoId = Guid.NewGuid().ToString();
            }
            
            var conversation = _chatService.GetConversation(Guid.Parse(contextoId));
            if (conversation == null) {
                Console.WriteLine($"Conversation not found: {contextoId}");
                return Problem("Error al subir el archivo");
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
                conversation.Context = [];
            }
        
            return Ok();
        }

        [HttpGet("stream-text")]
        public async Task<IActionResult> StreamText(string pregunta, string? contextoId, string model = "llama3.1", string? systemPrompt = null)
        {
            Response.Headers.Append("Content-Type", "application/json");

            //pregunta = "Responde de forma resumida la pregunta: " + pregunta;
            
            var writer = Response.BodyWriter;
            
            if (String.IsNullOrEmpty(contextoId)) {
                contextoId = Guid.NewGuid().ToString();
            }

            var conversation = _chatService.GetConversation(Guid.Parse(contextoId));

            if (conversation == null) {
                await writer.CompleteAsync();
                Console.WriteLine($"Conversation not found: {contextoId}");
                return new EmptyResult();
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
                conversation.Context = [];
            }

            if (systemPrompt != null) {
                pregunta = systemPrompt + "\n" + pregunta;
            }


            try
            {
                var ollama = new OllamaApiClient(BASE_URI);

                var request = new GenerateRequest {
                    Options = new RequestOptions {
                        Temperature = 0.5f
                    },
                    Prompt = pregunta,
                    Context = conversation.Context,
                    Model = model
                };
                
                await foreach (var stream in ollama.Generate(request, cancellationToken: conversation.TokenSource.Token)) {

                    var responseData = new ResponseData
                    {
                        ContextId = contextoId,
                        Chunk = stream!.Response
                    };

                    string jsonResponse = JsonSerializer.Serialize(responseData);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);

                    if (stream.Done) {
                        conversation.Context = (stream as GenerateDoneResponseStream)!.Context;
                    }

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
