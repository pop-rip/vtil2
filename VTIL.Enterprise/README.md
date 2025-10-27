# 🏢 VTIL.Enterprise - Enterprise Compliance & Analysis Platform

## 📋 Overview

VTIL.Enterprise is a comprehensive enterprise-grade platform built on top of VTIL2, providing:

- ✅ **Regulatory Compliance Management** for HIPAA, NIST, SOC 2, OSHA, and more
- 🔬 **Binary Analysis REST APIs** for large-scale deobfuscation
- 🔒 **Enterprise Security** with encryption, authentication, and audit logging
- 📊 **Governance & Risk Management** frameworks
- 📈 **Monitoring & Observability** with Prometheus metrics and health checks
- 🎯 **High Availability** with horizontal scaling and fault tolerance

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- (Optional) Docker for containerized deployment

### Running Locally

```bash
cd VTIL.Enterprise
dotnet restore
dotnet run
```

The service will start on `https://localhost:5001` with:
- **Swagger UI**: https://localhost:5001 (root path)
- **Health Checks**: https://localhost:5001/health
- **Metrics**: https://localhost:5001/metrics
- **Compliance API**: https://localhost:5001/api/v1/compliance
- **Analysis API**: https://localhost:5001/api/v1/analysis

### Docker Deployment

```dockerfile
# Build
docker build -t vtil-enterprise .

# Run
docker run -p 5001:5001 vtil-enterprise
```

## 📚 API Documentation

### Compliance API Endpoints

#### Get All Frameworks
```bash
GET /api/v1/compliance/frameworks
```

Returns all configured compliance frameworks (HIPAA, NIST, SOC 2, OSHA).

#### Get Compliance Dashboard
```bash
GET /api/v1/compliance/dashboard
```

Returns aggregated compliance metrics:
```json
{
  "totalFrameworks": 5,
  "activeFrameworks": 5,
  "overallCompliancePercentage": 94.5,
  "totalControls": 120,
  "compliantControls": 110,
  "nonCompliantControls": 5,
  "openFindings": 8,
  "criticalFindings": 0,
  "highFindings": 2
}
```

#### Generate Compliance Report
```bash
GET /api/v1/compliance/frameworks/{frameworkId}/report
```

Generates a detailed compliance report for a specific framework.

#### Get Findings
```bash
GET /api/v1/compliance/findings?minSeverity=High
```

Returns compliance findings filtered by severity.

### Analysis API Endpoints

#### Deobfuscate Binary
```bash
POST /api/v1/analysis/deobfuscate
Content-Type: application/json

{
  "binaryData": "base64_encoded_binary",
  "architecture": "Amd64",
  "entryPoint": 4096,
  "optimizationLevel": 2,
  "enableSymbolicExecution": true,
  "timeoutSeconds": 300
}
```

Returns:
```json
{
  "jobId": "123e4567-e89b-12d3-a456-426614174000",
  "success": true,
  "blockCount": 15,
  "instructionCount": 250,
  "optimizationsApplied": 42,
  "durationMs": 1250,
  "validationStatus": "Valid",
  "warnings": []
}
```

#### Simplify Expression
```bash
POST /api/v1/analysis/simplify
Content-Type: application/json

{
  "expression": "(x + 0) * 1",
  "pretty": true,
  "unpack": true
}
```

Returns:
```json
{
  "originalExpression": "(x + 0) * 1",
  "simplifiedExpression": "x",
  "originalComplexity": 5.0,
  "simplifiedComplexity": 1.0,
  "reductionPercentage": 80.0,
  "success": true
}
```

#### Get Analysis Statistics
```bash
GET /api/v1/analysis/statistics
```

Returns comprehensive analysis metrics.

## 🏗️ Architecture

### Layered Architecture

```
┌─────────────────────────────────────────────┐
│       Presentation Layer                    │
│  (REST API Controllers, Swagger UI)         │
├─────────────────────────────────────────────┤
│       Application Layer                     │
│  (Services, Business Logic, CQRS)           │
├─────────────────────────────────────────────┤
│       Domain Layer                          │
│  (Domain Models, Entities, Value Objects)   │
├─────────────────────────────────────────────┤
│       Infrastructure Layer                  │
│  (Data Access, External Services, I/O)      │
├─────────────────────────────────────────────┤
│       Cross-Cutting Concerns               │
│  (Logging, Security, Monitoring, Caching)   │
└─────────────────────────────────────────────┘
```

### Design Patterns Implemented

- ✅ **Repository Pattern** - Data access abstraction
- ✅ **Service Layer Pattern** - Business logic encapsulation
- ✅ **Dependency Injection** - Loose coupling
- ✅ **CQRS** - Separate read/write models
- ✅ **Mediator Pattern** - Decoupled communication
- ✅ **Circuit Breaker** - Fault tolerance
- ✅ **Retry Pattern** - Resilience
- ✅ **Factory Pattern** - Object creation
- ✅ **Strategy Pattern** - Algorithm selection
- ✅ **Observer Pattern** - Event-driven architecture

## 📊 Compliance Frameworks

### HIPAA (Health Insurance Portability and Accountability Act)

**32 Controls** covering:
- Administrative Safeguards (9 controls)
- Physical Safeguards (4 controls)
- Technical Safeguards (13 controls)
- Organizational Requirements (2 controls)
- Policies and Procedures (4 controls)

### NIST Cybersecurity Framework

**23 Controls** across five functions:
- **Identify** - Asset management, risk assessment, governance
- **Protect** - Access control, data security, protective technology
- **Detect** - Anomalies, continuous monitoring
- **Respond** - Response planning, communications, analysis, mitigation
- **Recover** - Recovery planning, improvements, communications

### SOC 2 Type I/II

**11 Controls** covering:
- Security (Common Criteria CC6, CC7)
- Availability (A1.1-A1.3)
- Confidentiality (C1.1-C1.2)
- Processing Integrity (PI1.1, PI1.4)
- Privacy (P1.1 - optional)

### OSHA (Occupational Safety and Health Administration)

**12 Controls** covering:
- General Duty & Workplace Safety
- Ergonomics & RSI Prevention
- Electrical Safety
- Fire Safety & Emergency Response
- Indoor Air Quality
- Recordkeeping & Training

## 🔒 Security Features

### Authentication & Authorization
- OAuth 2.0 / OpenID Connect support
- JWT token-based authentication
- Multi-factor authentication (MFA) ready
- Role-Based Access Control (RBAC)

### Encryption
- **Data at Rest**: AES-256 encryption
- **Data in Transit**: TLS 1.3 with perfect forward secrecy
- **Key Management**: Hardware Security Module (HSM) integration ready

### Audit Logging
- Comprehensive audit trail for all operations
- Tamper-proof log storage
- Structured logging with correlation IDs
- 7-year retention for compliance

### Security Assessment
- Automated security scanning
- Vulnerability management
- Threat intelligence integration
- Continuous monitoring

## 📈 Monitoring & Observability

### Prometheus Metrics

- `vtil_analysis_requests_total` - Total analysis requests by architecture and status
- `vtil_optimizations_applied_total` - Optimizations applied by pass name
- `vtil_compliance_violations_total` - Compliance violations by framework and severity
- `vtil_active_analysis_jobs` - Currently running analysis jobs
- `vtil_compliance_percentage` - Compliance percentage by framework
- `vtil_analysis_duration_seconds` - Analysis duration histogram
- `vtil_api_latency_seconds` - API latency summary (p50, p90, p95, p99)

### Health Checks

- **Liveness** (`/health/live`) - Is the application running?
- **Readiness** (`/health/ready`) - Is the application ready to serve requests?
- **Full Health** (`/health`) - Comprehensive health status

Health check components:
- Compliance service health
- Analysis service health
- System resources (CPU, memory, GC)

### Structured Logging

All logs include:
- Timestamp
- Correlation ID
- User/request context
- Severity level
- Structured JSON format

Integration with:
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Splunk
- Azure Application Insights
- AWS CloudWatch

## 🏢 Governance & Risk Management

### Governance Policies

- **DG-001**: Data Classification and Handling Policy
- **DG-002**: Data Retention and Disposal Policy
- **DG-003**: Access Control and Authorization Policy

### Risk Management

- Risk identification and assessment
- Risk mitigation tracking
- Risk scoring and prioritization
- Continuous risk monitoring
- Risk register maintenance

### Governance Metrics

- Policy compliance rate
- Risk exposure by category
- Governance score calculation
- Stakeholder reporting

## 🎯 Enterprise Features

### High Availability
- Stateless design for horizontal scaling
- Auto-scaling based on CPU/memory
- Multi-region deployment support
- Active-active or active-passive configurations

### Disaster Recovery
- **RPO** (Recovery Point Objective): < 1 hour
- **RTO** (Recovery Time Objective): < 4 hours
- Automated backups
- Point-in-time recovery

### Performance SLAs
- API Response Time (p95): < 100ms
- API Response Time (p99): < 500ms
- Analysis Throughput: > 100 routines/second
- Memory Usage: < 2GB per instance
- Uptime: 99.9%

### Scalability
- Horizontal scaling to 100+ instances
- Support for 10,000+ concurrent users
- 10,000+ requests per second
- Multi-region geographic distribution

## 📦 Technology Stack

- **Runtime**: .NET 8.0
- **Language**: C# 12.0
- **Web Framework**: ASP.NET Core 8.0
- **API Documentation**: Swagger/OpenAPI 3.0
- **Logging**: Serilog with structured logging
- **Monitoring**: Prometheus metrics
- **Health Checks**: ASP.NET Core Health Checks
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Mediator**: MediatR
- **Resilience**: Polly

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🐳 Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: vtil-enterprise
spec:
  replicas: 3
  selector:
    matchLabels:
      app: vtil-enterprise
  template:
    metadata:
      labels:
        app: vtil-enterprise
    spec:
      containers:
      - name: vtil-enterprise
        image: vtil-enterprise:latest
        ports:
        - containerPort: 5001
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5001
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5001
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: vtil-enterprise
spec:
  selector:
    app: vtil-enterprise
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5001
  type: LoadBalancer
```

## 📄 License

BSD 3-Clause License - Same as VTIL2 Project

## 🙏 Acknowledgments

Built on top of the VTIL2 project, which is a modern C# port of the original VTIL Project by Can Boluk and contributors.

---

**For more information, visit the [VTIL2 main README](../README.md)**

