using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using rpnet.teamsalert.function.Models;
using rpnet.teamsalert.function.StaticServices;

namespace rpnet.teamsalert.function.Handlers;

public abstract class RequestHandler : IRequestHandler
{
    private readonly string channelWebhookEndpoint;
    private readonly string geminiApiKey;
    private readonly string geminiApiUrl;
    private readonly HttpClient client;
    private IRequestHandler? _nextHandler;

    public RequestHandler()
    {
        DotNetEnv.Env.Load();
        channelWebhookEndpoint = Environment.GetEnvironmentVariable("WEBHOOK") ?? "";
        geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
        geminiApiUrl = Environment.GetEnvironmentVariable("GEMINI_API_ENDPOINT") ?? "";
        client = new()
        {
            BaseAddress = new Uri(channelWebhookEndpoint)
        };
    }

    public IRequestHandler SetNext(IRequestHandler next)
    {
        _nextHandler = next;
        return _nextHandler;
    }

    public virtual async Task<(string message, HttpStatusCode statusCode)> Handle(HttpContext context)
    {
        if (_nextHandler != null)
            return await _nextHandler.Handle(context);

        return ("No handler found.", HttpStatusCode.InternalServerError);
    }

    private string MountPrompt(string html)
    => @$"
        Analise o HTML de um e-mail que vou te enviar. Procure por erros de transmissão na seção com '------------ OBSERVACAO/ERROS ---------------'.

        Se encontrar erros, crie um JSON com:

        ""hasError"": true,

        ""errors"": um array com os detalhes de cada erro. Cada erro deve ter:

        ""codigo"": o código do erro.

        ""conta"": o valor de 'CONTA/CARTAO', ou null.

        ""data_transacao"": o valor de 'DTA.TRAN', ou null.

        ""valor"": o valor de 'VALOR', ou null.

        Se ""codigo"" (o código do erro) for null, crie um JSON com:

        ""hasError"": false,

        ""errors"": []

        Observações:

        - Se 'CONTA/CARTAO', 'DTA.TRAN' ou 'VALOR' não estiverem na linha do erro, coloque null.
        - Se não encontrar a seção de erros, retorne 'hasError': false e 'errors': [].
        - Se o html_email for vazio ou não for enviado, retorne um erro (ex: 'error': 'html_email obrigatório').

        html = ""{html}""
        ";


    protected bool NeedsAlert(ref ExpectedRequest objectData)
    {
        var prompt = MountPrompt(objectData?.Message ?? "");
        var responseContent = SendRequestToGemini(prompt);

        var extractedJson = ExtractJsonFromResponse(responseContent);
        if (string.IsNullOrEmpty(extractedJson))
            return false;

        var cleanedJson = CleanJsonString(extractedJson);
        bool hasError = Analyze(cleanedJson, out List<object> message);
        if (hasError)
            objectData.Facts = message;

        return hasError;
    }

    private bool Analyze(string cleanedJson, out List<object> message)
    {
        message = null;

        var newObject = JsonSerializer.Deserialize<GeminiDeserializedObject>(cleanedJson);
        if (newObject.HasError)
        {
            message = MountMessage(newObject.Errors);
        }


        return newObject?.HasError ?? false;
    }

    private List<object> MountMessage(GeminiError[]? geminiError)
    {
        var list = new List<object>();
        geminiError?.ToList().ForEach(x => 
                {
                    list.Add(new { name = "", value = "" });
                    list.Add(new { name = "", value = "" });
                    list.Add(new { name = "Codigo do Erro:", value = FindError(x.Codigo) });
                    list.Add(new { name = "Data da Transmissao:", value = x.DataTransacao });
                    list.Add(new { name = "Conta/Cartao:", value = x.Conta });
                    list.Add(new { name = "Valor:", value = x.Valor });
                });


        return list;
    }

    private string FindError(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return "Código inválido.";

        var errorDictionary = new Dictionary<string, string>
    {
        { "01", "HEADER INEXISTENTE" },
        { "02", "MAIS DE 1 HEADER" },
        { "03", "TRAILER INEXISTENTE" },
        { "04", "MAIS DE 1 TRAILER" },
        { "05", "SEQUENCIA DE REGISTROS INVALIDA" },
        { "06", "LOTE DE ARQUIVO INVALIDO" },
        { "07", "LOTE DE ARQUIVO FORA DE SEQUENCIA" },
        { "08", "DATA DO LOTE DE ARQUIVO INVALIDO" },
        { "09", "DATA DO LOTE DE ARQUIVO IGUAL OU MENOR QUE O ULTIMO" },
        { "10", "TOTAIS DO TRAILER INVALIDO" },
        { "11", "TOTAIS DO TRAILER NAO BATE" },
        { "12", "TIPO DE REGISTRO INVALIDO" },
        { "13", "CORP INVALIDA" },
        { "14", "VALOR INVALIDO" },
        { "15", "DATA INVALIDA" },
        { "16", "PGTOS MISTURADO COM OUTRAS TRANSACOES" },
        { "17", "AUTORIZACAO REVERTIDA" },
        { "18", "DATA DO LOTE DE ARQUIVO MENOR QUE O ULTIMO" },
        { "19", "LOTE DE SERVICO INVALIDO" },
        { "20", "TOTAIS DO TRAILER DE ARQUIVO INVALIDO" },
        { "21", "TOTAIS DO TRAILER DE ARQUIVO NAO BATE" },
        { "22", "TOTAIS DO TRAILER DE SERVICO INVALIDO" },
        { "23", "TOTAIS DO TRAILER DE SERVICO NAO BATE" },
        { "24", "ORDEM DOS TIPOS DE REGISTROS INVALIDA" },
        { "25", "LOTE DE SERVICO NAO BATE" },
        { "26", "ARQUIVO DUPLICADO" },
        { "27", "TIPO DE REGISTRO INVALIDO" },
        { "28", "NOSSO NUMERO INVALIDO/NAO ENCONTRADO" },
        { "29", "VALOR REAL DE PAGAMENTO INVALIDO" },
        { "30", "DATA REAL DE PAGAMENTO INVALIDO" }
    };

        return errorDictionary.TryGetValue(codigo, out var errorMessage)
            ? errorMessage
            : "Código de erro não encontrado.";
    }

    private string SendRequestToGemini(string prompt)
    {
        using var geminiClient = new HttpClient { BaseAddress = new Uri(geminiApiUrl + geminiApiKey) };

        var requestBody = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var response = geminiClient.PostAsync("", CreateHttpContent(requestBody)).GetAwaiter().GetResult();
        var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        var logEvent = new Event("Sent Request to Gemini", response);
        StaticRepository.Event(logEvent).GetAwaiter().GetResult();

        return responseContent;
    }
    private string? ExtractJsonFromResponse(string responseContent)
    {
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
    }

    private string CleanJsonString(string rawJson)
    {
        return Regex.Replace(rawJson, @"^```json\n|\n```$", "").Trim();
    }

    protected async Task<bool> Alert(ExpectedRequest content)
    {
        var bodyStructure = CreateStructure(content);
        var body = CreateHttpContent(bodyStructure);
        var response = await client.PostAsync("", body);
        var logEvent = new Event("Sent Alert to Teams", response);
        await StaticRepository.Event(logEvent);
        return response.IsSuccessStatusCode;
    }

    private object CreateStructure(ExpectedRequest content)
    {
        return new
        {
            type = "MessageCard",
            context = "http://schema.org/extensions",
            themeColor = "0076D7",
            summary = "Ocorreu um erro na transmissão dos seguintes pagamentos:",
            sections = new[]
            {
            new
            {
                activityTitle = "Ocorreu um erro na transmissão dos seguintes pagamentos:",
                activitySubtitle = "By Alert Function",
                facts = content.Facts
            }
        },
            markdown = true
        };
    }


    protected static T Deserialize<T>(string responseContent) where T : new()
        => JsonSerializer.Deserialize<T>(responseContent) ?? new T();

    protected static HttpContent CreateHttpContent(object? content)
    {
        var json = JsonSerializer.Serialize(content);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
