using Microsoft.AspNetCore.Mvc;
using MedAssistNG.Api.Mcp;
using System.Text.Json;

namespace MedAssistNG.Api.Controllers
{
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly ClinicalKnowledgeTool _clinicalTool;
        private readonly ILogger<McpController> _logger;

        public McpController(ClinicalKnowledgeTool clinicalTool, ILogger<McpController> logger)
        {
            _clinicalTool = clinicalTool;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult HandleRequest([FromBody] JsonElement request)
        {
            try
            {
                if (!request.TryGetProperty("method", out var methodProp))
                {
                    return BadRequest(new { jsonrpc = "2.0", error = new { code = -32600, message = "Invalid Request" }, id = GetId(request) });
                }

                var method = methodProp.GetString();
                var id = GetId(request);

                return method switch
                {
                    "initialize" => Ok(new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = new
                        {
                            protocolVersion = "2024-11-05",
                            capabilities = new { },
                            serverInfo = new { name = "MedAssistNG-MCP-Server", version = "1.0.0" }
                        }
                    }),
                    "tools/list" => Ok(new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = new
                        {
                            tools = new[]
                            {
                                new
                                {
                                    name = "get_clinical_knowledge",
                                    description = "Get common causes for symptoms in a Nigerian clinical context.",
                                    inputSchema = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            symptoms = new { type = "string", description = "The patients symptoms" }
                                        },
                                        required = new[] { "symptoms" }
                                    }
                                }
                            }
                        }
                    }),
                    "tools/call" => HandleToolCall(request, id),
                    _ => Ok(new { jsonrpc = "2.0", error = new { code = -32601, message = "Method not found" }, id })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MCP request");
                return StatusCode(500, new { jsonrpc = "2.0", error = new { code = -32603, message = "Internal error" }, id = GetId(request) });
            }
        }

        private IActionResult HandleToolCall(JsonElement request, JsonElement id)
        {
            if (!request.TryGetProperty("params", out var paramsProp) ||
                !paramsProp.TryGetProperty("name", out var nameProp))
            {
                return BadRequest(new { jsonrpc = "2.0", error = new { code = -32602, message = "Invalid params" }, id });
            }

            var toolName = nameProp.GetString();
            if (toolName == "get_clinical_knowledge")
            {
                if (paramsProp.TryGetProperty("arguments", out var argsProp) &&
                    argsProp.TryGetProperty("symptoms", out var symptomsProp))
                {
                    var symptoms = symptomsProp.GetString();
                    var result = _clinicalTool.GetCommonCauses(symptoms ?? "");
                    return Ok(new
                    {
                        jsonrpc = "2.0",
                        id,
                        result = new
                        {
                            content = new[]
                            {
                                new { type = "text", text = result }
                            }
                        }
                    });
                }
            }

            return Ok(new { jsonrpc = "2.0", error = new { code = -32601, message = "Tool not found" }, id });
        }

        private JsonElement GetId(JsonElement request)
        {
            return request.TryGetProperty("id", out var id) ? id : default;
        }
    }
}
