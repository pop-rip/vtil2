/*
 * VTIL.Enterprise - NIST Cybersecurity Framework
 * Copyright (c) 2025 VTIL2 Project
 */

using System;
using System.Collections.Generic;
using VTIL.Enterprise.Compliance.Models;

namespace VTIL.Enterprise.Compliance.Frameworks
{
    /// <summary>
    /// NIST Cybersecurity Framework compliance.
    /// </summary>
    public static class NISTCybersecurityFramework
    {
        /// <summary>
        /// Creates a NIST CSF compliance framework with all required controls.
        /// </summary>
        public static ComplianceFramework Create()
        {
            var framework = new ComplianceFramework
            {
                Type = ComplianceFrameworkType.NISTCybersecurity,
                Name = "NIST Cybersecurity Framework",
                Description = "Framework for improving critical infrastructure cybersecurity based on five core functions",
                Version = "1.1",
                EffectiveDate = new DateTime(2018, 4, 16),
                IsActive = true
            };

            framework.Controls = new List<ComplianceControl>
            {
                // IDENTIFY Function
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "ID.AM-1",
                    Name = "Asset Management - Physical Devices",
                    Description = "Physical devices and systems within the organization are inventoried",
                    Category = "Identify - Asset Management",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Automated asset discovery and inventory management system",
                    Priority = 2,
                    Tags = new List<string> { "Identify", "Asset Management", "Inventory" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "ID.AM-2",
                    Name = "Asset Management - Software Platforms",
                    Description = "Software platforms and applications within the organization are inventoried",
                    Category = "Identify - Asset Management",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Software bill of materials (SBOM) tracking with version control",
                    Priority = 2,
                    Tags = new List<string> { "Identify", "Asset Management", "Software" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "ID.RA-1",
                    Name = "Risk Assessment - Vulnerabilities",
                    Description = "Asset vulnerabilities are identified and documented",
                    Category = "Identify - Risk Assessment",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Continuous vulnerability scanning with automated remediation workflows",
                    Priority = 1,
                    Tags = new List<string> { "Identify", "Risk Assessment", "Vulnerabilities" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "ID.RA-2",
                    Name = "Risk Assessment - Threat Intelligence",
                    Description = "Cyber threat intelligence is received from information sharing forums and sources",
                    Category = "Identify - Risk Assessment",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Medium,
                    Implementation = "Integration with threat intelligence feeds and security advisories",
                    Priority = 3,
                    Tags = new List<string> { "Identify", "Risk Assessment", "Threat Intelligence" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "ID.GV-1",
                    Name = "Governance - Cybersecurity Policy",
                    Description = "Organizational cybersecurity policy is established and communicated",
                    Category = "Identify - Governance",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Documented cybersecurity policies with employee acknowledgment tracking",
                    Priority = 2,
                    Tags = new List<string> { "Identify", "Governance", "Policy" }
                },

                // PROTECT Function
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "PR.AC-1",
                    Name = "Access Control - Identity Management",
                    Description = "Identities and credentials are issued, managed, verified, revoked, and audited",
                    Category = "Protect - Access Control",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Centralized identity management with automated provisioning/deprovisioning",
                    Priority = 1,
                    Tags = new List<string> { "Protect", "Access Control", "Identity" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "PR.AC-3",
                    Name = "Access Control - Remote Access",
                    Description = "Remote access is managed",
                    Category = "Protect - Access Control",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "VPN with MFA for all remote access, session monitoring and recording",
                    Priority = 2,
                    Tags = new List<string> { "Protect", "Access Control", "Remote Access" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "PR.AC-4",
                    Name = "Access Control - Permissions",
                    Description = "Access permissions and authorizations are managed, incorporating principles of least privilege",
                    Category = "Protect - Access Control",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Fine-grained RBAC with automated access reviews and recertification",
                    Priority = 1,
                    Tags = new List<string> { "Protect", "Access Control", "Least Privilege" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "PR.AC-5",
                    Name = "Access Control - Network Integrity",
                    Description = "Network integrity is protected (e.g., network segregation, network segmentation)",
                    Category = "Protect - Access Control",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Network segmentation with micro-segmentation for critical workloads",
                    Priority = 2,
                    Tags = new List<string> { "Protect", "Access Control", "Network" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "PR.DS-1",
                    Name = "Data Security - At Rest",
                    Description = "Data-at-rest is protected",
                    Category = "Protect - Data Security",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "AES-256 encryption for all data at rest with hardware security module (HSM) key management",
                    Priority = 1,
                    Tags = new List<string> { "Protect", "Data Security", "Encryption" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "PR.DS-2",
                    Name = "Data Security - In Transit",
                    Description = "Data-in-transit is protected",
                    Category = "Protect - Data Security",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "TLS 1.3 with perfect forward secrecy for all data in transit",
                    Priority = 1,
                    Tags = new List<string> { "Protect", "Data Security", "Encryption" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "PR.PT-1",
                    Name = "Protective Technology - Audit Logs",
                    Description = "Audit/log records are determined, documented, implemented, and reviewed",
                    Category = "Protect - Protective Technology",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Comprehensive audit logging with SIEM integration and real-time analysis",
                    Priority = 1,
                    Tags = new List<string> { "Protect", "Protective Technology", "Logging" }
                },

                // DETECT Function
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "DE.AE-1",
                    Name = "Anomalies and Events - Baseline",
                    Description = "A baseline of network operations and expected data flows is established and managed",
                    Category = "Detect - Anomalies and Events",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Machine learning-based behavioral analysis with anomaly detection",
                    Priority = 2,
                    Tags = new List<string> { "Detect", "Anomalies", "Baseline" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "DE.AE-3",
                    Name = "Anomalies and Events - Aggregation",
                    Description = "Event data are collected and correlated from multiple sources and sensors",
                    Category = "Detect - Anomalies and Events",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Centralized log aggregation with correlation rules and alerting",
                    Priority = 2,
                    Tags = new List<string> { "Detect", "Anomalies", "Correlation" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "DE.CM-1",
                    Name = "Security Continuous Monitoring - Network",
                    Description = "The network is monitored to detect potential cybersecurity events",
                    Category = "Detect - Continuous Monitoring",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "24/7 network monitoring with IDS/IPS and automated threat response",
                    Priority = 1,
                    Tags = new List<string> { "Detect", "Monitoring", "Network" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "DE.CM-3",
                    Name = "Security Continuous Monitoring - Personnel Activity",
                    Description = "Personnel activity is monitored to detect potential cybersecurity events",
                    Category = "Detect - Continuous Monitoring",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "User behavior analytics (UBA) with privileged access monitoring",
                    Priority = 2,
                    Tags = new List<string> { "Detect", "Monitoring", "Personnel" }
                },

                // RESPOND Function
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "RS.RP-1",
                    Name = "Response Planning",
                    Description = "Response plan is executed during or after an incident",
                    Category = "Respond - Response Planning",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Documented incident response playbooks with automated orchestration",
                    Priority = 1,
                    Tags = new List<string> { "Respond", "Planning", "Incident Response" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "RS.CO-2",
                    Name = "Communications - Incident Reporting",
                    Description = "Incidents are reported consistent with established criteria",
                    Category = "Respond - Communications",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Automated incident reporting with stakeholder notification workflows",
                    Priority = 2,
                    Tags = new List<string> { "Respond", "Communications", "Reporting" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "RS.AN-1",
                    Name = "Analysis - Notification Investigation",
                    Description = "Notifications from detection systems are investigated",
                    Category = "Respond - Analysis",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Automated triage and investigation workflows with case management",
                    Priority = 1,
                    Tags = new List<string> { "Respond", "Analysis", "Investigation" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "RS.MI-1",
                    Name = "Mitigation - Incident Containment",
                    Description = "Incidents are contained",
                    Category = "Respond - Mitigation",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Automated containment actions with manual override capabilities",
                    Priority = 1,
                    Tags = new List<string> { "Respond", "Mitigation", "Containment" }
                },

                // RECOVER Function
                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "RC.RP-1",
                    Name = "Recovery Planning",
                    Description = "Recovery plan is executed during or after a cybersecurity incident",
                    Category = "Recover - Recovery Planning",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Critical,
                    Implementation = "Automated recovery procedures with failover and rollback capabilities",
                    Priority = 1,
                    Tags = new List<string> { "Recover", "Planning", "Recovery" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "RC.IM-1",
                    Name = "Improvements - Lessons Learned",
                    Description = "Recovery plans incorporate lessons learned",
                    Category = "Recover - Improvements",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.Medium,
                    Implementation = "Post-incident review process with continuous improvement tracking",
                    Priority = 3,
                    Tags = new List<string> { "Recover", "Improvements", "Lessons Learned" }
                },

                new ComplianceControl
                {
                    FrameworkId = framework.Id,
                    ControlId = "RC.CO-3",
                    Name = "Communications - Recovery Activities",
                    Description = "Recovery activities are communicated to internal and external stakeholders",
                    Category = "Recover - Communications",
                    Status = ComplianceStatus.Compliant,
                    Severity = ComplianceSeverity.High,
                    Implementation = "Stakeholder communication templates with automated status updates",
                    Priority = 2,
                    Tags = new List<string> { "Recover", "Communications", "Stakeholders" }
                }
            };

            framework.CalculateCompliancePercentage();
            return framework;
        }
    }
}

