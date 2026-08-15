using NewKrepysh.WinUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewKrepysh.WinUI.Services
{
    public static class AiAssistanceService
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<string> GenerateBody(
            SitePage sitePage,
            string url,
            string apiKey,
            string model,
            string prompt,
            IList<string>? assets = null)
        {
            if (assets == null)
                assets = new List<string>();

            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(apiKey))
                apiKey = string.Empty;

            if (string.IsNullOrWhiteSpace(model))
                model = string.Empty;

            var requestUri = url.TrimEnd('/') + "/v1/completions";

            var fullPrompt = BuildPrompt(sitePage, prompt, assets);

            var completionRequest = new CompletionRequest
            {
                Model = model,
                Prompt = fullPrompt,
                MaxTokens = 2048,
                Temperature = 0.7,
                TopP = 1.0,
                FrequencyPenalty = 0.5,
                PresencePenalty = 0.5,
                Stream = false
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent.Create(completionRequest);

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var completionResponse = await response.Content.ReadFromJsonAsync<CompletionResponse>();

                if (completionResponse?.Choices is null || completionResponse.Choices.Count == 0)
                {
                    throw new InvalidOperationException("The API returned an empty response.");
                }

                var generatedContent = completionResponse.Choices[0].Text?.Trim() ?? string.Empty;

                return generatedContent;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to call the AI API: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while generating body content: {ex.Message}", ex);
            }
        }

        private static string BuildPrompt(SitePage sitePage, string userPrompt, IList<string> assets)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine("You are generating HTML body content for a website page.");
            promptBuilder.AppendLine($"Page Title: {sitePage.Title}");

            if (!string.IsNullOrWhiteSpace(sitePage.HtmlContent))
            {
                promptBuilder.AppendLine("Current HTML content (for context):");
                promptBuilder.AppendLine(sitePage.HtmlContent);
                promptBuilder.AppendLine();
            }

            if (assets.Count > 0)
            {
                promptBuilder.AppendLine("Available assets stored at wwwroot (for context):");
                promptBuilder.AppendLine(string.Join(", ", assets));
                promptBuilder.AppendLine();
            }

            promptBuilder.AppendLine("Instructions:");
            promptBuilder.AppendLine(userPrompt);
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Return only the HTML content for the <body> element. Do not include <html>, <head>, or <body> tags themselves unless explicitly requested. Do NOT put the HTML content inside markdown code block, and do not commentate it.");

            return promptBuilder.ToString();
        }

        #region API Models

        private class CompletionRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; } = 2048;

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0.7;

            [JsonPropertyName("top_p")]
            public double TopP { get; set; } = 1.0;

            [JsonPropertyName("frequency_penalty")]
            public double FrequencyPenalty { get; set; } = 0.0;

            [JsonPropertyName("presence_penalty")]
            public double PresencePenalty { get; set; } = 0.0;

            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;
        }

        private class CompletionResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("object")]
            public string Object { get; set; } = string.Empty;

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();

            [JsonPropertyName("usage")]
            public Usage? Usage { get; set; }
        }

        private class Choice
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;

            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("logprobs")]
            public object? Logprobs { get; set; }

            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; } = string.Empty;
        }

        private class Usage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }

        #endregion
    }
}