/*
 * VTIL.Enterprise - Compliance REST API Controller
 * Copyright (c) 2025 VTIL2 Project
 */

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VTIL.Enterprise.Compliance.Models;
using VTIL.Enterprise.Compliance.Services;

namespace VTIL.Enterprise.Controllers
{
    /// <summary>
    /// REST API controller for compliance management.
    /// </summary>
    [ApiController]
    [Route("api/v1/compliance")]
    [Produces("application/json")]
    public class ComplianceController : ControllerBase
    {
        private readonly IComplianceService _complianceService;

        public ComplianceController(IComplianceService complianceService)
        {
            _complianceService = complianceService ?? throw new ArgumentNullException(nameof(complianceService));
        }

        /// <summary>
        /// Gets all compliance frameworks.
        /// </summary>
        [HttpGet("frameworks")]
        [ProducesResponseType(typeof(IEnumerable<ComplianceFramework>), 200)]
        public async Task<ActionResult<IEnumerable<ComplianceFramework>>> GetFrameworks()
        {
            var frameworks = await _complianceService.GetAllFrameworksAsync();
            return Ok(frameworks);
        }

        /// <summary>
        /// Gets a specific compliance framework by ID.
        /// </summary>
        [HttpGet("frameworks/{id}")]
        [ProducesResponseType(typeof(ComplianceFramework), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComplianceFramework>> GetFramework(Guid id)
        {
            var framework = await _complianceService.GetFrameworkAsync(id);
            if (framework == null)
                return NotFound($"Framework with ID {id} not found");

            return Ok(framework);
        }

        /// <summary>
        /// Gets a compliance framework by type.
        /// </summary>
        [HttpGet("frameworks/type/{type}")]
        [ProducesResponseType(typeof(ComplianceFramework), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComplianceFramework>> GetFrameworkByType(ComplianceFrameworkType type)
        {
            var framework = await _complianceService.GetFrameworkByTypeAsync(type);
            if (framework == null)
                return NotFound($"Framework of type {type} not found");

            return Ok(framework);
        }

        /// <summary>
        /// Creates a new compliance framework.
        /// </summary>
        [HttpPost("frameworks")]
        [ProducesResponseType(typeof(ComplianceFramework), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ComplianceFramework>> CreateFramework([FromBody] ComplianceFramework framework)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _complianceService.CreateFrameworkAsync(framework);
            return CreatedAtAction(nameof(GetFramework), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates an existing compliance framework.
        /// </summary>
        [HttpPut("frameworks/{id}")]
        [ProducesResponseType(typeof(ComplianceFramework), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComplianceFramework>> UpdateFramework(Guid id, [FromBody] ComplianceFramework framework)
        {
            if (id != framework.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _complianceService.UpdateFrameworkAsync(framework);
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a compliance framework.
        /// </summary>
        [HttpDelete("frameworks/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFramework(Guid id)
        {
            var deleted = await _complianceService.DeleteFrameworkAsync(id);
            if (!deleted)
                return NotFound($"Framework with ID {id} not found");

            return NoContent();
        }

        /// <summary>
        /// Gets a specific control within a framework.
        /// </summary>
        [HttpGet("frameworks/{frameworkId}/controls/{controlId}")]
        [ProducesResponseType(typeof(ComplianceControl), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComplianceControl>> GetControl(Guid frameworkId, Guid controlId)
        {
            var control = await _complianceService.GetControlAsync(frameworkId, controlId);
            if (control == null)
                return NotFound($"Control {controlId} not found in framework {frameworkId}");

            return Ok(control);
        }

        /// <summary>
        /// Updates a control within a framework.
        /// </summary>
        [HttpPut("frameworks/{frameworkId}/controls/{controlId}")]
        [ProducesResponseType(typeof(ComplianceControl), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComplianceControl>> UpdateControl(
            Guid frameworkId, 
            Guid controlId, 
            [FromBody] ComplianceControl control)
        {
            if (controlId != control.Id)
                return BadRequest("ID mismatch");

            if (frameworkId != control.FrameworkId)
                return BadRequest("Framework ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _complianceService.UpdateControlAsync(frameworkId, control);
            return Ok(updated);
        }

        /// <summary>
        /// Generates a compliance report for a framework.
        /// </summary>
        [HttpGet("frameworks/{frameworkId}/report")]
        [ProducesResponseType(typeof(ComplianceReport), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ComplianceReport>> GenerateReport(Guid frameworkId)
        {
            try
            {
                var report = await _complianceService.GenerateComplianceReportAsync(frameworkId);
                return Ok(report);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Generates a consolidated compliance report across all frameworks.
        /// </summary>
        [HttpGet("reports/consolidated")]
        [ProducesResponseType(typeof(ComplianceReport), 200)]
        public async Task<ActionResult<ComplianceReport>> GenerateConsolidatedReport()
        {
            var report = await _complianceService.GenerateConsolidatedReportAsync();
            return Ok(report);
        }

        /// <summary>
        /// Performs a compliance assessment for a framework.
        /// </summary>
        [HttpPost("frameworks/{frameworkId}/assess")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PerformAssessment(Guid frameworkId)
        {
            var success = await _complianceService.PerformAssessmentAsync(frameworkId);
            if (!success)
                return NotFound($"Framework with ID {frameworkId} not found");

            return Ok(new { message = "Assessment completed successfully", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Gets all compliance findings, optionally filtered by severity.
        /// </summary>
        [HttpGet("findings")]
        [ProducesResponseType(typeof(IEnumerable<ComplianceFinding>), 200)]
        public async Task<ActionResult<IEnumerable<ComplianceFinding>>> GetFindings(
            [FromQuery] ComplianceSeverity? minSeverity = null)
        {
            var findings = await _complianceService.GetFindingsAsync(minSeverity);
            return Ok(findings);
        }

        /// <summary>
        /// Creates a new compliance finding.
        /// </summary>
        [HttpPost("findings")]
        [ProducesResponseType(typeof(ComplianceFinding), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ComplianceFinding>> CreateFinding([FromBody] ComplianceFinding finding)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _complianceService.CreateFindingAsync(finding);
            return CreatedAtAction(nameof(GetFindings), null, created);
        }

        /// <summary>
        /// Updates a compliance finding.
        /// </summary>
        [HttpPut("findings/{id}")]
        [ProducesResponseType(typeof(ComplianceFinding), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ComplianceFinding>> UpdateFinding(Guid id, [FromBody] ComplianceFinding finding)
        {
            if (id != finding.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _complianceService.UpdateFindingAsync(finding);
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a compliance finding.
        /// </summary>
        [HttpDelete("findings/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFinding(Guid id)
        {
            var deleted = await _complianceService.DeleteFindingAsync(id);
            if (!deleted)
                return NotFound($"Finding with ID {id} not found");

            return NoContent();
        }

        /// <summary>
        /// Gets compliance dashboard metrics.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ComplianceDashboard), 200)]
        public async Task<ActionResult<ComplianceDashboard>> GetDashboard()
        {
            var frameworks = await _complianceService.GetAllFrameworksAsync();
            var findings = await _complianceService.GetFindingsAsync();

            var dashboard = new ComplianceDashboard
            {
                TotalFrameworks = frameworks.Count(),
                ActiveFrameworks = frameworks.Count(f => f.IsActive),
                OverallCompliancePercentage = frameworks.Average(f => f.CompliancePercentage),
                TotalControls = frameworks.Sum(f => f.Controls.Count),
                CompliantControls = frameworks.Sum(f => f.Controls.Count(c => c.Status == ComplianceStatus.Compliant)),
                NonCompliantControls = frameworks.Sum(f => f.Controls.Count(c => c.Status == ComplianceStatus.NonCompliant)),
                OpenFindings = findings.Count(),
                CriticalFindings = findings.Count(f => f.Severity == ComplianceSeverity.Critical),
                HighFindings = findings.Count(f => f.Severity == ComplianceSeverity.High),
                MediumFindings = findings.Count(f => f.Severity == ComplianceSeverity.Medium),
                LowFindings = findings.Count(f => f.Severity == ComplianceSeverity.Low),
                LastUpdated = DateTime.UtcNow
            };

            return Ok(dashboard);
        }
    }

    /// <summary>
    /// Compliance dashboard summary.
    /// </summary>
    public class ComplianceDashboard
    {
        public int TotalFrameworks { get; set; }
        public int ActiveFrameworks { get; set; }
        public double OverallCompliancePercentage { get; set; }
        public int TotalControls { get; set; }
        public int CompliantControls { get; set; }
        public int NonCompliantControls { get; set; }
        public int OpenFindings { get; set; }
        public int CriticalFindings { get; set; }
        public int HighFindings { get; set; }
        public int MediumFindings { get; set; }
        public int LowFindings { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

