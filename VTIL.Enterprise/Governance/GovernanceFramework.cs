/*
 * VTIL.Enterprise - Governance Framework
 * Copyright (c) 2025 VTIL2 Project
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace VTIL.Enterprise.Governance
{
    /// <summary>
    /// Enterprise governance framework for VTIL2.
    /// </summary>
    public class GovernanceFramework
    {
        /// <summary>
        /// Governance policies.
        /// </summary>
        public List<GovernancePolicy> Policies { get; set; } = new();

        /// <summary>
        /// Risk register.
        /// </summary>
        public List<RiskItem> Risks { get; set; } = new();

        /// <summary>
        /// Governance metrics.
        /// </summary>
        public GovernanceMetrics Metrics { get; set; } = new();

        /// <summary>
        /// Calculates governance score.
        /// </summary>
        public double CalculateGovernanceScore()
        {
            if (Policies.Count == 0)
                return 0;

            var approvedPolicies = Policies.Count(p => p.Status == PolicyStatus.Approved);
            var policyScore = (double)approvedPolicies / Policies.Count * 100;

            var lowRiskCount = Risks.Count(r => r.CurrentRiskLevel == RiskLevel.Low || r.CurrentRiskLevel == RiskLevel.VeryLow);
            var riskScore = Risks.Count > 0 ? (double)lowRiskCount / Risks.Count * 100 : 100;

            return (policyScore * 0.6) + (riskScore * 0.4);
        }
    }

    /// <summary>
    /// Governance policy.
    /// </summary>
    public class GovernancePolicy
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PolicyNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PolicyStatus Status { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public string Owner { get; set; } = string.Empty;
        public List<string> Stakeholders { get; set; } = new();
        public string PolicyDocument { get; set; } = string.Empty;
        public List<string> RelatedPolicies { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Policy status.
    /// </summary>
    public enum PolicyStatus
    {
        Draft,
        UnderReview,
        Approved,
        Active,
        Deprecated,
        Archived
    }

    /// <summary>
    /// Risk item in the risk register.
    /// </summary>
    public class RiskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RiskId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RiskCategory Category { get; set; }
        public RiskLevel InherentRiskLevel { get; set; }
        public RiskLevel CurrentRiskLevel { get; set; }
        public RiskLevel TargetRiskLevel { get; set; }
        public RiskStatus Status { get; set; }
        public string Owner { get; set; } = string.Empty;
        public DateTime IdentifiedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public List<RiskMitigation> Mitigations { get; set; } = new();
        public double LikelihoodScore { get; set; } // 1-5
        public double ImpactScore { get; set; } // 1-5
        public double RiskScore => LikelihoodScore * ImpactScore;
    }

    /// <summary>
    /// Risk category.
    /// </summary>
    public enum RiskCategory
    {
        Operational,
        Technical,
        Security,
        Compliance,
        Financial,
        Reputational,
        Strategic
    }

    /// <summary>
    /// Risk level.
    /// </summary>
    public enum RiskLevel
    {
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Critical
    }

    /// <summary>
    /// Risk status.
    /// </summary>
    public enum RiskStatus
    {
        Identified,
        Assessing,
        Mitigating,
        Monitoring,
        Accepted,
        Closed
    }

    /// <summary>
    /// Risk mitigation action.
    /// </summary>
    public class RiskMitigation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MitigationType Type { get; set; }
        public MitigationStatus Status { get; set; }
        public string Owner { get; set; } = string.Empty;
        public DateTime PlannedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public double EffectivenessScore { get; set; } // 0-1
    }

    /// <summary>
    /// Mitigation type.
    /// </summary>
    public enum MitigationType
    {
        Avoid,
        Reduce,
        Transfer,
        Accept
    }

    /// <summary>
    /// Mitigation status.
    /// </summary>
    public enum MitigationStatus
    {
        Planned,
        InProgress,
        Implemented,
        Verified,
        Ineffective
    }

    /// <summary>
    /// Governance metrics.
    /// </summary>
    public class GovernanceMetrics
    {
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public int TotalPolicies { get; set; }
        public int ApprovedPolicies { get; set; }
        public int TotalRisks { get; set; }
        public int HighRisks { get; set; }
        public int CriticalRisks { get; set; }
        public double AverageRiskScore { get; set; }
        public double GovernanceScore { get; set; }
        public Dictionary<string, int> RisksByCategory { get; set; } = new();
        public Dictionary<string, int> PolicyComplianceRate { get; set; } = new();
    }

    /// <summary>
    /// Data governance policies specific to VTIL operations.
    /// </summary>
    public static class DataGovernancePolicies
    {
        public static readonly GovernancePolicy DataClassificationPolicy = new()
        {
            PolicyNumber = "DG-001",
            Title = "Data Classification and Handling Policy",
            Description = "Defines classification levels for analyzed binaries and routines, and appropriate handling procedures for each level",
            Status = PolicyStatus.Approved,
            EffectiveDate = new DateTime(2025, 1, 1),
            Owner = "Chief Information Security Officer",
            PolicyDocument = @"
                Classification Levels:
                - Public: Non-sensitive deobfuscated code
                - Internal: Proprietary analysis results
                - Confidential: Customer-specific binaries
                - Restricted: Security-sensitive findings
                
                Handling Requirements:
                - Public: No restrictions
                - Internal: Encrypted at rest, access logging
                - Confidential: Encryption, access control, audit trail
                - Restricted: Maximum security, need-to-know basis
            "
        };

        public static readonly GovernancePolicy DataRetentionPolicy = new()
        {
            PolicyNumber = "DG-002",
            Title = "Data Retention and Disposal Policy",
            Description = "Defines retention periods for analysis data and secure disposal procedures",
            Status = PolicyStatus.Approved,
            EffectiveDate = new DateTime(2025, 1, 1),
            Owner = "Chief Information Security Officer",
            PolicyDocument = @"
                Retention Periods:
                - Analysis Jobs: 90 days
                - Audit Logs: 7 years (compliance requirement)
                - Deobfuscated Routines: 1 year
                - Compliance Reports: 6 years (HIPAA requirement)
                
                Disposal:
                - Cryptographic erasure for all sensitive data
                - Certificate of destruction for regulated data
                - Automated purge processes with verification
            "
        };

        public static readonly GovernancePolicy AccessControlPolicy = new()
        {
            PolicyNumber = "DG-003",
            Title = "Access Control and Authorization Policy",
            Description = "Defines access control requirements and authorization procedures",
            Status = PolicyStatus.Approved,
            EffectiveDate = new DateTime(2025, 1, 1),
            Owner = "Chief Information Security Officer",
            PolicyDocument = @"
                Access Control Model: Role-Based Access Control (RBAC)
                
                Roles:
                - Admin: Full system access
                - Analyst: Analysis and deobfuscation operations
                - Auditor: Read-only access to compliance and audit data
                - Viewer: Read-only access to non-sensitive data
                
                Authorization Requirements:
                - Multi-factor authentication for all users
                - Principle of least privilege
                - Regular access reviews (quarterly)
                - Immediate revocation upon termination
            "
        };
    }
}

