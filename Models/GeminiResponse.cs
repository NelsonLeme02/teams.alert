using System;
using System.Text.Json.Serialization;

namespace rpnet.teamsalert.function.Models;

public class GeminiResponse
{
    public Candidate[]? Candidates { get; set; }
}
public class Candidate
{
    public Content? Content { get; set; }
}
public class Content
{
    public Part[]? Parts { get; set; }
}
public class Part
{
    public string? Text { get; set; }
}

public class GeminiDeserializedObject
{
    [JsonPropertyName("hasError")]
    public bool HasError { get; set; }
    [JsonPropertyName("errors")]
    public GeminiError[]? Errors { get; set; }
}

public class GeminiError
{
    [JsonPropertyName("codigo")]
    public string? Codigo { get; set; }
    [JsonPropertyName("conta")]
    public string? Conta { get; set; }
    [JsonPropertyName("data_transacao")]
    public string? DataTransacao { get; set; }
    [JsonPropertyName("valor")]
    public string? Valor { get; set; }
}