using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions;
using Microsoft.Extensions.Options;

namespace Shop.Infrastructure.Services
{
    public class CommentModerationService : ICommentModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaSettings _settings;

        public CommentModerationService(
            IHttpClientFactory httpClientFactory,
            IOptions<OllamaSettings> settings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _settings = settings.Value;
        }

        public async Task<CommentModerationResultDto>
            AnalyzeCommentAsync(string comment)
        {
            try
            {
                var prompt = $@"
                You are a Persian comment moderation system.
                
                Analyze the following comment:
                
                {comment}
                
                Rules:
                
                - Offensive language => reject
                - Advertisement => reject
                - Phone number => reject
                - Telegram ID => reject
                - Website link => reject
                - Spam => reject
                - Otherwise approve
                
                Return ONLY valid JSON:
                
                {{
                  ""approved"": true,
                  ""reason"": ""clean""
                }}
                ";


                var response =
                    await _httpClient.PostAsJsonAsync(
                        $"{_settings.BaseUrl}/api/generate",
                        new
                        {
                            model = _settings.Model,
                            prompt,
                            stream = false
                        });

                response.EnsureSuccessStatusCode();

                var content =
                    await response.Content.ReadAsStringAsync();

                using var doc =
                    JsonDocument.Parse(content);

                var aiText =
                    doc.RootElement
                       .GetProperty("response")
                       .GetString();

                if (string.IsNullOrWhiteSpace(aiText))
                {
                    return new CommentModerationResultDto
                    {
                        IsApproved = false,
                        Reason = "Empty AI response"
                    };
                }

                aiText = aiText
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                using var aiJson =
                    JsonDocument.Parse(aiText);

                // throw new Exception(aiText);

                return new CommentModerationResultDto
                {
                    IsApproved =
                        aiJson.RootElement
                              .GetProperty("approved")
                              .GetBoolean(),

                    Reason =
                        aiJson.RootElement
                              .GetProperty("reason")
                              .GetString() ?? ""
                };
            }

            catch (Exception ex)
            {
                return new CommentModerationResultDto
                {
                    IsApproved = false,
                    Reason = ex.ToString()
                };
            }
        }
    }
}

