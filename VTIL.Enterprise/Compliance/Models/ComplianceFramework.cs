/*
 * VTIL.Enterprise - Enterprise Compliance Framework
 * Copyright (c) 2025 VTIL2 Project
 */

using System;
using System.Collections.Generic;

namespace VTIL.Enterprise.Compliance.Models
{
    /// <summary>
    /// Supported regulatory compliance frameworks.
    /// </summary>
    public enum ComplianceFrameworkType
    {
        /// <summary>
        /// Health Insurance Portability and Accountability Act
        /// </summary>
        HIPAA,

        /// <summary>
        /// NIST Cybersecurity Framework
        /// </summary>
        NISTCybersecurity,

        /// <summary>
        /// Service Organization Control Type I
        /// </summary>
        SOCTypeI,

        /// <summary>
        /// Service Organization Control Type II
        /// </summary>
        SOCTypeII,

        /// <summary>
        /// Occupational Safety and Health Administration
        /// </summary>
        OSHA,

        /// <summary>
        /// General Data Protection Regulation
        /// </summary>
        GDPR,

        /// <summary>
        /// Payment Card Industry Data Security Standard
        /// </summary>
        PCIDSS,

        /// <summary>
        /// ISO 27001 Information Security
        /// </summary>
        ISO27001,

        /// <summary>
        /// Federal Risk and Authorization Management Program
        /// </summary>
        FedRAMP
    }

    /// <summary>
    /// Compliance status for a control or requirement.
    /// </summary>
    public enum ComplianceStatus
    {
        /// <summary>
        /// Fully compliant
        /// </summary>
        Compliant,

        /// <summary>
        /// Partially compliant - remediation in progress
        /// </summary>
        PartiallyCompliant,

        /// <summary>
        /// Non-compliant - requires immediate attention
        /// </summary>
        NonCompliant,

        /// <summary>
        /// Not applicable to this system
        /// </summary>
        NotApplicable,

        /// <summary>
        /// Under review - status being assessed
        /// </summary>
        UnderReview
    }

    /// <summary>
    /// Severity level for compliance violations.
    /// </summary>
    public enum ComplianceSeverity
    {
        /// <summary>
        /// Informational - no immediate action required
        /// </summary>
        Informational,

        /// <summary>
        /// Low severity - should be addressed in normal course
        /// </summary>
        Low,

        /// <summary>
        /// Medium severity - should be addressed soon
        /// </summary>
        Medium,

        /// <summary>
        /// High severity - requires prompt attention
        /// </summary>
        High,

        /// <summary>
        /// Critical severity - requires immediate action
        /// </summary>
        Critical
    }

    /// <summary>
    /// Represents a compliance framework definition.
    /// </summary>
    public class ComplianceFramework
    {
        /// <summary>
        /// Unique identifier for the framework.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Type of compliance framework.
        /// </summary>
        public ComplianceFrameworkType Type { get; set; }

        /// <summary>
        /// Framework name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Framework description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Framework version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Controls defined by this framework.
        /// </summary>
        public List<ComplianceControl> Controls { get; set; } = new();

        /// <summary>
        /// Whether this framework is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Last assessment date.
        /// </summary>
        public DateTime? LastAssessmentDate { get; set; }

        /// <summary>
        /// Next assessment due date.
        /// </summary>
        public DateTime? NextAssessmentDate { get; set; }

        /// <summary>
        /// Overall compliance status.
        /// </summary>
        public ComplianceStatus OverallStatus { get; set; }

        /// <summary>
        /// Compliance percentage (0-100).
        /// </summary>
        public double CompliancePercentage { get; set; }

        /// <summary>
        /// Metadata for additional framework information.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Calculates the overall compliance percentage.
        /// </summary>
        public void CalculateCompliancePercentage()
        {
            if (Controls.Count == 0)
            {
                CompliancePercentage = 0;
                return;
            }

            var compliantControls = Controls.Count(c => c.Status == ComplianceStatus.Compliant);
            var partialControls = Controls.Count(c => c.Status == ComplianceStatus.PartiallyCompliant);
            var applicableControls = Controls.Count(c => c.Status != ComplianceStatus.NotApplicable);

            if (applicableControls == 0)
            {
                CompliancePercentage = 100;
                return;
            }

            CompliancePercentage = ((compliantControls + (partialControls * 0.5)) / applicableControls) * 100;

            // Update overall status
            if (CompliancePercentage >= 100)
                OverallStatus = ComplianceStatus.Compliant;
            else if (CompliancePercentage >= 75)
                OverallStatus = ComplianceStatus.PartiallyCompliant;
            else
                OverallStatus = ComplianceStatus.NonCompliant;
        }
    }

    /// <summary>
    /// Represents a specific compliance control or requirement.
    /// </summary>
    public class ComplianceControl
    {
        /// <summary>
        /// Unique identifier for the control.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Framework this control belongs to.
        /// </summary>
        public Guid FrameworkId { get; set; }

        /// <summary>
        /// Control identifier (e.g., "AC-1", "164.308(a)(1)(i)").
        /// </summary>
        public string ControlId { get; set; } = string.Empty;

        /// <summary>
        /// Control name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Control description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Control category.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Current compliance status.
        /// </summary>
        public ComplianceStatus Status { get; set; }

        /// <summary>
        /// Severity if non-compliant.
        /// </summary>
        public ComplianceSeverity Severity { get; set; }

        /// <summary>
        /// Implementation details.
        /// </summary>
        public string Implementation { get; set; } = string.Empty;

        /// <summary>
        /// Evidence of compliance.
        /// </summary>
        public List<ComplianceEvidence> Evidence { get; set; } = new();

        /// <summary>
        /// Findings or issues.
        /// </summary>
        public List<ComplianceFinding> Findings { get; set; } = new();

        /// <summary>
        /// Last assessment date.
        /// </summary>
        public DateTime? LastAssessmentDate { get; set; }

        /// <summary>
        /// Next assessment due date.
        /// </summary>
        public DateTime? NextAssessmentDate { get; set; }

        /// <summary>
        /// Assigned to (person or team).
        /// </summary>
        public string? AssignedTo { get; set; }

        /// <summary>
        /// Priority level.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Tags for categorization.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Related controls.
        /// </summary>
        public List<Guid> RelatedControls { get; set; } = new();

        /// <summary>
        /// Checks if this control is due for assessment.
        /// </summary>
        public bool IsDueForAssessment()
        {
            if (!NextAssessmentDate.HasValue)
                return true;

            return DateTime.UtcNow >= NextAssessmentDate.Value;
        }
    }

    /// <summary>
    /// Evidence supporting compliance.
    /// </summary>
    public class ComplianceEvidence
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Control this evidence supports.
        /// </summary>
        public Guid ControlId { get; set; }

        /// <summary>
        /// Evidence type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Evidence description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// File path or URL to evidence.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Collected date.
        /// </summary>
        public DateTime CollectedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collected by.
        /// </summary>
        public string CollectedBy { get; set; } = string.Empty;

        /// <summary>
        /// Evidence metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Compliance finding or issue.
    /// </summary>
    public class ComplianceFinding
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Control this finding relates to.
        /// </summary>
        public Guid ControlId { get; set; }

        /// <summary>
        /// Finding title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Finding description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Severity level.
        /// </summary>
        public ComplianceSeverity Severity { get; set; }

        /// <summary>
        /// Finding status.
        /// </summary>
        public FindingStatus Status { get; set; }

        /// <summary>
        /// Discovered date.
        /// </summary>
        public DateTime DiscoveredDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Resolved date.
        /// </summary>
        public DateTime? ResolvedDate { get; set; }

        /// <summary>
        /// Remediation plan.
        /// </summary>
        public string? RemediationPlan { get; set; }

        /// <summary>
        /// Assigned to.
        /// </summary>
        public string? AssignedTo { get; set; }

        /// <summary>
        /// Due date for remediation.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Resolution notes.
        /// </summary>
        public string? ResolutionNotes { get; set; }
    }

    /// <summary>
    /// Finding status.
    /// </summary>
    public enum FindingStatus
    {
        Open,
        InProgress,
        Resolved,
        Accepted,
        Deferred
    }
}

