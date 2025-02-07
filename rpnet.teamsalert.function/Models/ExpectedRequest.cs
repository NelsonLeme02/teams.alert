using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace rpnet.teamsalert.function.Models;

public class ExpectedRequest
{
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }
    [JsonPropertyName("sender")]
    public string? Sender { get; set; }
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    public List<object>? Facts { get; set; }
}
