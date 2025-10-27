/*
 * VTIL.Enterprise - Compliance Service
 * Copyright (c) 2025 VTIL2 Project
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VTIL.Enterprise.Compliance.Models;
using VTIL.Enterprise.Compliance.Frameworks;

namespace VTIL.Enterprise.Compliance.Services
{
    /// <summary>
    /// Service for managing compliance frameworks and assessments.
    /// </summary>
    public interface IComplianceService
    {
        Task<IEnumerable<ComplianceFramework>> GetAllFrameworksAsync();
        Task<ComplianceFramework?> GetFrameworkAsync(Guid id);
        Task<ComplianceFramework?> GetFrameworkByTypeAsync(ComplianceFrameworkType type);
        Task<ComplianceFramework> CreateFrameworkAsync(ComplianceFramework framework);
        Task<ComplianceFramework> UpdateFrameworkAsync(ComplianceFramework framework);
        Task<bool> DeleteFrameworkAsync(Guid id);
        Task<ComplianceControl?> GetControlAsync(Guid frameworkId, Guid controlId);
        Task<ComplianceControl> UpdateControlAsync(Guid frameworkId, ComplianceControl control);
        Task<ComplianceReport> GenerateComplianceReportAsync(Guid frameworkId);
        Task<ComplianceReport> GenerateConsolidatedReportAsync();
        Task<bool> PerformAssessmentAsync(Guid frameworkId);
        Task<IEnumerable<ComplianceFinding>> GetFindingsAsync(ComplianceSeverity? minSeverity = null);
        Task<ComplianceFinding> CreateFindingAsync(ComplianceFinding finding);
        Task<ComplianceFinding> UpdateFindingAsync(ComplianceFinding finding);
        Task<bool> DeleteFindingAsync(Guid findingId);
    }

    /// <summary>
    /// Implementation of compliance service.
    /// </summary>
    public class ComplianceService : IComplianceService
    {
        private readonly List<ComplianceFramework> _frameworks = new();
        private readonly object _lock = new();

        public ComplianceService()
        {
            // Initialize with default frameworks
            InitializeDefaultFrameworks();
        }

        private void InitializeDefaultFrameworks()
        {
            lock (_lock)
            {
                _frameworks.Clear();
                _frameworks.Add(HIPAAFramework.Create());
                _frameworks.Add(NISTCybersecurityFramework.Create());
                _frameworks.Add(SOCFramework.CreateTypeI());
                _frameworks.Add(SOCFramework.CreateTypeII());
                _frameworks.Add(OSHAFramework.Create());
            }
        }

        public Task<IEnumerable<ComplianceFramework>> GetAllFrameworksAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<ComplianceFramework>>(_frameworks.ToList());
            }
        }

        public Task<ComplianceFramework?> GetFrameworkAsync(Guid id)
        {
            lock (_lock)
            {
                return Task.FromResult(_frameworks.FirstOrDefault(f => f.Id == id));
            }
        }

        public Task<ComplianceFramework?> GetFrameworkByTypeAsync(ComplianceFrameworkType type)
        {
            lock (_lock)
            {
                return Task.FromResult(_frameworks.FirstOrDefault(f => f.Type == type));
            }
        }

        public Task<ComplianceFramework> CreateFrameworkAsync(ComplianceFramework framework)
        {
            lock (_lock)
            {
                framework.Id = Guid.NewGuid();
                _frameworks.Add(framework);
                return Task.FromResult(framework);
            }
        }

        public Task<ComplianceFramework> UpdateFrameworkAsync(ComplianceFramework framework)
        {
            lock (_lock)
            {
                var existing = _frameworks.FirstOrDefault(f => f.Id == framework.Id);
                if (existing != null)
                {
                    _frameworks.Remove(existing);
                    _frameworks.Add(framework);
                }
                return Task.FromResult(framework);
            }
        }

        public Task<bool> DeleteFrameworkAsync(Guid id)
        {
            lock (_lock)
            {
                var framework = _frameworks.FirstOrDefault(f => f.Id == id);
                if (framework != null)
                {
                    _frameworks.Remove(framework);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
        }

        public Task<ComplianceControl?> GetControlAsync(Guid frameworkId, Guid controlId)
        {
            lock (_lock)
            {
                var framework = _frameworks.FirstOrDefault(f => f.Id == frameworkId);
                return Task.FromResult(framework?.Controls.FirstOrDefault(c => c.Id == controlId));
            }
        }

        public Task<ComplianceControl> UpdateControlAsync(Guid frameworkId, ComplianceControl control)
        {
            lock (_lock)
            {
                var framework = _frameworks.FirstOrDefault(f => f.Id == frameworkId);
                if (framework != null)
                {
                    var existing = framework.Controls.FirstOrDefault(c => c.Id == control.Id);
                    if (existing != null)
                    {
                        framework.Controls.Remove(existing);
                        framework.Controls.Add(control);
                        framework.CalculateCompliancePercentage();
                    }
                }
                return Task.FromResult(control);
            }
        }

        public Task<ComplianceReport> GenerateComplianceReportAsync(Guid frameworkId)
        {
            lock (_lock)
            {
                var framework = _frameworks.FirstOrDefault(f => f.Id == frameworkId);
                if (framework == null)
                    throw new InvalidOperationException($"Framework {frameworkId} not found");

                var report = new ComplianceReport
                {
                    FrameworkId = frameworkId,
                    FrameworkName = framework.Name,
                    FrameworkType = framework.Type,
                    ReportDate = DateTime.UtcNow,
                    OverallStatus = framework.OverallStatus,
                    CompliancePercentage = framework.CompliancePercentage,
                    TotalControls = framework.Controls.Count,
                    CompliantControls = framework.Controls.Count(c => c.Status == ComplianceStatus.Compliant),
                    PartiallyCompliantControls = framework.Controls.Count(c => c.Status == ComplianceStatus.PartiallyCompliant),
                    NonCompliantControls = framework.Controls.Count(c => c.Status == ComplianceStatus.NonCompliant),
                    NotApplicableControls = framework.Controls.Count(c => c.Status == ComplianceStatus.NotApplicable),
                    Findings = framework.Controls.SelectMany(c => c.Findings).ToList(),
                    Summary = $"Compliance assessment for {framework.Name} completed on {DateTime.UtcNow:yyyy-MM-dd}"
                };

                return Task.FromResult(report);
            }
        }

        public Task<ComplianceReport> GenerateConsolidatedReportAsync()
        {
            lock (_lock)
            {
                var allFindings = _frameworks
                    .SelectMany(f => f.Controls)
                    .SelectMany(c => c.Findings)
                    .ToList();

                var report = new ComplianceReport
                {
                    FrameworkName = "Consolidated Compliance Report",
                    ReportDate = DateTime.UtcNow,
                    TotalControls = _frameworks.Sum(f => f.Controls.Count),
                    CompliantControls = _frameworks.Sum(f => f.Controls.Count(c => c.Status == ComplianceStatus.Compliant)),
                    PartiallyCompliantControls = _frameworks.Sum(f => f.Controls.Count(c => c.Status == ComplianceStatus.PartiallyCompliant)),
                    NonCompliantControls = _frameworks.Sum(f => f.Controls.Count(c => c.Status == ComplianceStatus.NonCompliant)),
                    NotApplicableControls = _frameworks.Sum(f => f.Controls.Count(c => c.Status == ComplianceStatus.NotApplicable)),
                    Findings = allFindings,
                    CompliancePercentage = _frameworks.Average(f => f.CompliancePercentage),
                    Summary = $"Consolidated compliance across {_frameworks.Count} frameworks"
                };

                return Task.FromResult(report);
            }
        }

        public Task<bool> PerformAssessmentAsync(Guid frameworkId)
        {
            lock (_lock)
            {
                var framework = _frameworks.FirstOrDefault(f => f.Id == frameworkId);
                if (framework == null)
                    return Task.FromResult(false);

                framework.LastAssessmentDate = DateTime.UtcNow;
                framework.NextAssessmentDate = DateTime.UtcNow.AddMonths(3); // Quarterly assessments
                framework.CalculateCompliancePercentage();

                // Update control assessment dates
                foreach (var control in framework.Controls)
                {
                    control.LastAssessmentDate = DateTime.UtcNow;
                    control.NextAssessmentDate = DateTime.UtcNow.AddMonths(3);
                }

                return Task.FromResult(true);
            }
        }

        public Task<IEnumerable<ComplianceFinding>> GetFindingsAsync(ComplianceSeverity? minSeverity = null)
        {
            lock (_lock)
            {
                var findings = _frameworks
                    .SelectMany(f => f.Controls)
                    .SelectMany(c => c.Findings)
                    .Where(f => f.Status != FindingStatus.Resolved);

                if (minSeverity.HasValue)
                {
                    findings = findings.Where(f => f.Severity >= minSeverity.Value);
                }

                return Task.FromResult(findings.OrderByDescending(f => f.Severity).AsEnumerable());
            }
        }

        public Task<ComplianceFinding> CreateFindingAsync(ComplianceFinding finding)
        {
            lock (_lock)
            {
                finding.Id = Guid.NewGuid();
                finding.DiscoveredDate = DateTime.UtcNow;

                var control = _frameworks
                    .SelectMany(f => f.Controls)
                    .FirstOrDefault(c => c.Id == finding.ControlId);

                if (control != null)
                {
                    control.Findings.Add(finding);
                    control.Status = ComplianceStatus.NonCompliant;
                }

                return Task.FromResult(finding);
            }
        }

        public Task<ComplianceFinding> UpdateFindingAsync(ComplianceFinding finding)
        {
            lock (_lock)
            {
                var control = _frameworks
                    .SelectMany(f => f.Controls)
                    .FirstOrDefault(c => c.Id == finding.ControlId);

                if (control != null)
                {
                    var existing = control.Findings.FirstOrDefault(f => f.Id == finding.Id);
                    if (existing != null)
                    {
                        control.Findings.Remove(existing);
                        control.Findings.Add(finding);

                        // Update control status if all findings are resolved
                        if (control.Findings.All(f => f.Status == FindingStatus.Resolved))
                        {
                            control.Status = ComplianceStatus.Compliant;
                        }
                    }
                }

                return Task.FromResult(finding);
            }
        }

        public Task<bool> DeleteFindingAsync(Guid findingId)
        {
            lock (_lock)
            {
                foreach (var framework in _frameworks)
                {
                    foreach (var control in framework.Controls)
                    {
                        var finding = control.Findings.FirstOrDefault(f => f.Id == findingId);
                        if (finding != null)
                        {
                            control.Findings.Remove(finding);
                            return Task.FromResult(true);
                        }
                    }
                }
                return Task.FromResult(false);
            }
        }
    }

    /// <summary>
    /// Compliance report.
    /// </summary>
    public class ComplianceReport
    {
        public Guid? FrameworkId { get; set; }
        public string FrameworkName { get; set; } = string.Empty;
        public ComplianceFrameworkType? FrameworkType { get; set; }
        public DateTime ReportDate { get; set; }
        public ComplianceStatus OverallStatus { get; set; }
        public double CompliancePercentage { get; set; }
        public int TotalControls { get; set; }
        public int CompliantControls { get; set; }
        public int PartiallyCompliantControls { get; set; }
        public int NonCompliantControls { get; set; }
        public int NotApplicableControls { get; set; }
        public List<ComplianceFinding> Findings { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public Dictionary<string, object> Metrics { get; set; } = new();
    }
}

