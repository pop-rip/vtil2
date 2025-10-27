/*
 * VTIL.Enterprise - SOC Type I/II Framework
 * Copyright (c) 2025 VTIL2 Project
 */

using System;
using System.Collections.Generic;
using VTIL.Enterprise.Compliance.Models;

namespace VTIL.Enterprise.Compliance.Frameworks
{
    /// <summary>
    /// SOC (Service Organization Control) Type I and Type II compliance framework.
    /// </summary>
    public static class SOCFramework
    {
        /// <summary>
        /// Creates a SOC Type I compliance framework.
        /// </summary>
        public static ComplianceFramework CreateTypeI()
        {
            var framework = new ComplianceFramework
            {
                Type = ComplianceFrameworkType.SOCTypeI,
                Name = "SOC 2 Type I",
                Description = "Service Organization Control Type I - Point-in-time assessment of controls",
                Version = "2017 Trust Services Criteria",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };

            framework.Controls = CreateSOCControls(framework.Id, isTypeII: false);
            framework.CalculateCompliancePercentage();
            return framework;
        }

        /// <summary>
        /// Creates a SOC Type II compliance framework.
        /// </summary>
        public static ComplianceFramework CreateTypeII()
        {
            var framework = new ComplianceFramework
            {
                Type = ComplianceFrameworkType.SOCTypeII,
                Name = "SOC 2 Type II",
                Description = "Service Organization Control Type II - Operating effectiveness over time (minimum 6 months)",
                Version = "2017 Trust Services Criteria",
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };

            framework.Controls = CreateSOCControls(framework.Id, isTypeII: true);
            framework.CalculateCompliancePercentage();
            return framework;
        }

        private static List<ComplianceControl> CreateSOCControls(Guid frameworkId, bool isTypeII)
        {
            return new List<ComplianceControl>
            {
                // Security - Common Criteria
                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "CC6.1",
                    Name = "Logical and Physical Access Controls",
                    Description = "The entity implements logical and physical access controls to limit access to authorized users",
                    Category = "Security - Common Criteria",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Multi-factor authentication, role-based access control, physical security controls",
                    Priority = 1,
                    Tags = new List<string> { "Security", "Access Control", "CC6" }
                },

                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "CC6.6",
                    Name = "Encryption of Data at Rest and in Transit",
                    Description = "The entity uses encryption to protect data at rest and in transit",
                    Category = "Security - Common Criteria",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "AES-256 for data at rest, TLS 1.3 for data in transit with perfect forward secrecy",
                    Priority = 1,
                    Tags = new List<string> { "Security", "Encryption", "CC6" }
                },

                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "CC7.2",
                    Name = "System Monitoring",
                    Description = "The entity monitors system components and the operation of those components for anomalies",
                    Category = "Security - Common Criteria",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "24/7 system monitoring with alerting, dashboards, and automated response",
                    Priority = 1,
                    Tags = new List<string> { "Security", "Monitoring", "CC7" }
                },

                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "CC7.3",
                    Name = "Incident Response",
                    Description = "The entity evaluates security events to determine whether they could impact the system",
                    Category = "Security - Common Criteria",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Automated incident detection and response with escalation procedures",
                    Priority = 1,
                    Tags = new List<string> { "Security", "Incident Response", "CC7" }
                },

                // Availability
                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "A1.1",
                    Name = "Availability - System Capacity",
                    Description = "The entity maintains system capacity to meet commitments and system requirements",
                    Category = "Availability",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Auto-scaling infrastructure with capacity monitoring and forecasting",
                    Priority = 2,
                    Tags = new List<string> { "Availability", "Capacity", "Performance" }
                },

                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "A1.2",
                    Name = "Availability - System Monitoring",
                    Description = "The entity monitors system components to ensure availability commitments are met",
                    Category = "Availability",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Real-time availability monitoring with SLA tracking and alerting",
                    Priority = 2,
                    Tags = new List<string> { "Availability", "Monitoring", "SLA" }
                },

                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "A1.3",
                    Name = "Availability - Incident Response",
                    Description = "The entity responds to system availability issues in accordance with commitments",
                    Category = "Availability",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Automated failover, incident escalation, and recovery procedures",
                    Priority = 1,
                    Tags = new List<string> { "Availability", "Incident Response", "Recovery" }
                },

                // Confidentiality
                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "C1.1",
                    Name = "Confidentiality - Information Classification",
                    Description = "The entity identifies and maintains confidential information to meet commitments",
                    Category = "Confidentiality",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Data classification system with automated tagging and handling policies",
                    Priority = 2,
                    Tags = new List<string> { "Confidentiality", "Classification", "Data Protection" }
                },

                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "C1.2",
                    Name = "Confidentiality - Access Restrictions",
                    Description = "The entity disposes of confidential information to meet commitments",
                    Category = "Confidentiality",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Secure data disposal with cryptographic erasure and certificate of destruction",
                    Priority = 2,
                    Tags = new List<string> { "Confidentiality", "Data Disposal", "Lifecycle" }
                },

                // Processing Integrity
                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "PI1.1",
                    Name = "Processing Integrity - Data Processing",
                    Description = "The entity processes data completely, accurately, and in a timely manner",
                    Category = "Processing Integrity",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Data validation, integrity checks, and transaction monitoring in VTIL processing pipeline",
                    Priority = 2,
                    Tags = new List<string> { "Processing Integrity", "Data Quality", "Validation" }
                },

                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "PI1.4",
                    Name = "Processing Integrity - Error Correction",
                    Description = "The entity corrects identified data processing errors in a timely manner",
                    Category = "Processing Integrity",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Medium,
                    Implementation = "Automated error detection and correction in deobfuscation workflows",
                    Priority = 3,
                    Tags = new List<string> { "Processing Integrity", "Error Handling", "Quality" }
                },

                // Privacy (if applicable)
                new ComplianceControl
                {
                    FrameworkId = frameworkId,
                    ControlId = "P1.1",
                    Name = "Privacy - Notice and Communication",
                    Description = "The entity provides notice about privacy practices and communicates choices",
                    Category = "Privacy",
                    Status = ComplianceStatus.NotApplicable,
                    Severity = ComplianceSeverity.Low,
                    Implementation = "Not applicable - VTIL2 does not process personal data directly",
                    Priority = 5,
                    Tags = new List<string> { "Privacy", "Notice", "Optional" }
                }
            };
        }
    }
}

