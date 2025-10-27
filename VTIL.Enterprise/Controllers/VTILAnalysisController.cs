/*
 * VTIL.Enterprise - VTIL Analysis REST API Controller
 * Copyright (c) 2025 VTIL2 Project
 */

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VTIL.Architecture;
using VTIL.Compiler.Optimizer;
using VTIL.Compiler.Validation;
using VTIL.SymEx;
using VTIL.Enterprise.Services;

namespace VTIL.Enterprise.Controllers
{
    /// <summary>
    /// REST API controller for VTIL binary analysis and deobfuscation.
    /// </summary>
    [ApiController]
    [Route("api/v1/analysis")]
    [Produces("application/json")]
    public class VTILAnalysisController : ControllerBase
    {
        private readonly IVTILAnalysisService _analysisService;
        private readonly IAuditService _auditService;

        public VTILAnalysisController(
            IVTILAnalysisService analysisService,
            IAuditService auditService)
        {
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Analyzes a binary and returns the deobfuscated routine.
        /// </summary>
        [HttpPost("deobfuscate")]
        [ProducesResponseType(typeof(AnalysisResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<AnalysisResult>> DeobfuscateBinary([FromBody] AnalysisRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Audit the request
                await _auditService.LogAnalysisRequestAsync(request, User.Identity?.Name ?? "Anonymous");

                // Perform analysis
                var result = await _analysisService.DeobfuscateBinaryAsync(request);

                // Audit the result
                await _auditService.LogAnalysisResultAsync(result, User.Identity?.Name ?? "Anonymous");

                return Ok(result);
            }
            catch (Exception ex)
            {
                await _auditService.LogAnalysisErrorAsync(request, ex, User.Identity?.Name ?? "Anonymous");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Simplifies a symbolic expression.
        /// </summary>
        [HttpPost("simplify")]
        [ProducesResponseType(typeof(SimplificationResult), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SimplificationResult>> SimplifyExpression([FromBody] SimplificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _analysisService.SimplifyExpressionAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Validates a routine for correctness.
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidationResult), 200)]
        public async Task<ActionResult<ValidationResult>> ValidateRoutine([FromBody] ValidationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _analysisService.ValidateRoutineAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Gets analysis statistics and metrics.
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(AnalysisStatistics), 200)]
        public async Task<ActionResult<AnalysisStatistics>> GetStatistics()
        {
            var stats = await _analysisService.GetStatisticsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Gets recent analysis jobs.
        /// </summary>
        [HttpGet("jobs")]
        [ProducesResponseType(typeof(IEnumerable<AnalysisJob>), 200)]
        public async Task<ActionResult<IEnumerable<AnalysisJob>>> GetJobs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var jobs = await _analysisService.GetJobsAsync(page, pageSize);
            return Ok(jobs);
        }

        /// <summary>
        /// Gets a specific analysis job by ID.
        /// </summary>
        [HttpGet("jobs/{id}")]
        [ProducesResponseType(typeof(AnalysisJob), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AnalysisJob>> GetJob(Guid id)
        {
            var job = await _analysisService.GetJobAsync(id);
            if (job == null)
                return NotFound($"Job with ID {id} not found");

            return Ok(job);
        }

        /// <summary>
        /// Cancels a running analysis job.
        /// </summary>
        [HttpPost("jobs/{id}/cancel")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CancelJob(Guid id)
        {
            var success = await _analysisService.CancelJobAsync(id);
            if (!success)
                return NotFound($"Job with ID {id} not found or already completed");

            return Ok(new { message = "Job cancelled successfully" });
        }
    }

    /// <summary>
    /// Analysis request model.
    /// </summary>
    public class AnalysisRequest
    {
        /// <summary>
        /// Binary data to analyze (base64 encoded).
        /// </summary>
        public string BinaryData { get; set; } = string.Empty;

        /// <summary>
        /// Target architecture.
        /// </summary>
        public string Architecture { get; set; } = "Amd64";

        /// <summary>
        /// Entry point address.
        /// </summary>
        public ulong EntryPoint { get; set; }

        /// <summary>
        /// Optimization level (0-3).
        /// </summary>
        public int OptimizationLevel { get; set; } = 2;

        /// <summary>
        /// Enable symbolic execution.
        /// </summary>
        public bool EnableSymbolicExecution { get; set; } = true;

        /// <summary>
        /// Maximum analysis time in seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Custom options.
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new();
    }

    /// <summary>
    /// Analysis result model.
    /// </summary>
    public class AnalysisResult
    {
        /// <summary>
        /// Job ID.
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Analysis success status.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of basic blocks.
        /// </summary>
        public int BlockCount { get; set; }

        /// <summary>
        /// Number of instructions.
        /// </summary>
        public int InstructionCount { get; set; }

        /// <summary>
        /// Number of optimizations applied.
        /// </summary>
        public int OptimizationsApplied { get; set; }

        /// <summary>
        /// Analysis duration in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Validation result.
        /// </summary>
        public string ValidationStatus { get; set; } = string.Empty;

        /// <summary>
        /// Deobfuscated routine (serialized).
        /// </summary>
        public string? RoutineData { get; set; }

        /// <summary>
        /// Analysis metrics.
        /// </summary>
        public Dictionary<string, object> Metrics { get; set; } = new();

        /// <summary>
        /// Warnings generated during analysis.
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Error message if analysis failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Simplification request.
    /// </summary>
    public class SimplificationRequest
    {
        /// <summary>
        /// Expression to simplify (string representation).
        /// </summary>
        public string Expression { get; set; } = string.Empty;

        /// <summary>
        /// Enable pretty printing.
        /// </summary>
        public bool Pretty { get; set; } = true;

        /// <summary>
        /// Enable unpacking.
        /// </summary>
        public bool Unpack { get; set; } = true;
    }

    /// <summary>
    /// Simplification result.
    /// </summary>
    public class SimplificationResult
    {
        /// <summary>
        /// Original expression.
        /// </summary>
        public string OriginalExpression { get; set; } = string.Empty;

        /// <summary>
        /// Simplified expression.
        /// </summary>
        public string SimplifiedExpression { get; set; } = string.Empty;

        /// <summary>
        /// Original complexity.
        /// </summary>
        public double OriginalComplexity { get; set; }

        /// <summary>
        /// Simplified complexity.
        /// </summary>
        public double SimplifiedComplexity { get; set; }

        /// <summary>
        /// Complexity reduction percentage.
        /// </summary>
        public double ReductionPercentage { get; set; }

        /// <summary>
        /// Whether simplification was successful.
        /// </summary>
        public bool Success { get; set; }
    }

    /// <summary>
    /// Validation request.
    /// </summary>
    public class ValidationRequest
    {
        /// <summary>
        /// Routine data to validate (serialized).
        /// </summary>
        public string RoutineData { get; set; } = string.Empty;
    }

    /// <summary>
    /// Analysis statistics.
    /// </summary>
    public class AnalysisStatistics
    {
        public int TotalJobs { get; set; }
        public int SuccessfulJobs { get; set; }
        public int FailedJobs { get; set; }
        public int RunningJobs { get; set; }
        public double AverageDurationSeconds { get; set; }
        public long TotalInstructionsProcessed { get; set; }
        public long TotalOptimizationsApplied { get; set; }
        public DateTime LastJobTime { get; set; }
        public Dictionary<string, int> JobsByArchitecture { get; set; } = new();
        public Dictionary<string, double> OptimizationPassEffectiveness { get; set; } = new();
    }

    /// <summary>
    /// Analysis job information.
    /// </summary>
    public class AnalysisJob
    {
        public Guid Id { get; set; }
        public string Architecture { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? BlockCount { get; set; }
        public int? InstructionCount { get; set; }
        public int? OptimizationsApplied { get; set; }
        public string? ErrorMessage { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;
    }
}

