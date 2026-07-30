# Dmytro Vietrov

**.NET Software Engineer | Infrastructure Automation**

📍 Paterna, Valencia, Spain (Permanent Resident) · 📞 +34 603 963 377 · ✉️ dmitriy.vetrov@gmail.com · 🔗 [linkedin.com/in/dmvietrov](https://www.linkedin.com/in/dmvietrov)

---

## Professional Summary

.NET Software Engineer with 8+ years building backend and full-stack systems in C# / .NET — REST APIs, services, web applications, and data layers. Strong with ASP.NET Core, Entity Framework, and SQL, deploying to the cloud and operating workloads in Kubernetes. My background combines .NET engineering with hands-on DevOps (Terraform, CI/CD) and enterprise system modernization.

---

## AI Engineering Experience

Hands-on experience across several AI directions:

- **Agentic RAG pipeline (LangGraph)** — query decomposition, parallel retrieval, LLM-based semantic validation of results, and answer aggregation.

- **AI-Assisted Software Development (Claude Code)** — used as an AI-first development environment for the full software lifecycle: from requirements decomposition and architecture design to incremental implementation, testing, and refactoring. Delivered:
  - A **Telegram bot** for processing receipts and invoices with Azure Document Intelligence, converting documents into structured JSON with subsequent LLM-based categorization of goods and services — [github.com/DmitriyVetrov/payper](https://github.com/DmitriyVetrov/payper).
  - A **production-grade companion application** demonstrating the full cycle of AI-assisted software development — [github.com/DmitriyVetrov/powerservice-challenge](https://github.com/DmitriyVetrov/powerservice-challenge).

- **Multi-Agent AI System (Claude Code)** — built a multi-agent system for automated job searching across 10+ job platforms (LinkedIn and others), comprising:
  - **Search Agent** — finds relevant vacancies on 10+ job platforms based on résumé requirements.
  - **Ranking Agent** — scores and classifies vacancies by match level (high / medium / low).
  - **Reporting Agent** — aggregates results and produces structured reports.
  - Integrated **Playwright** for browser automation, **MCP connectors**, **Gmail** integration, and automatic web-form filling driven by a knowledge base (salary expectations, language proficiency, work experience, etc.).

- **Microsoft Agent Framework** — currently exploring it for a chatbot and a lightweight RAG, specifically as a native, enterprise-grade approach to agent development in C# / .NET from Microsoft.

---

## Technical Skills

| Area | Technologies |
|---|---|
| **Languages & Frameworks** | C#, .NET / .NET Core, ASP.NET Core, Web API, Entity Framework, LINQ, Minimal API |
| **Testing** | xUnit, Selenium, mocking |
| **Cloud (Azure)** | App Services, Blob Storage, Azure AI services; Azure deployment |
| **AI / Agents** | LangGraph, RAG, Agentic & Multi-Agent systems, MCP, Claude Code, Microsoft Agent Framework, Azure OpenAI, Azure AI Document Intelligence, Playwright |
| **Architecture** | REST APIs, Microservices, Domain-Driven Design, Clean Architecture |
| **Front-End** | React, TypeScript, JavaScript, SignalR, HTML, CSS |
| **Data** | SQL (MS SQL Server, Oracle, PostgreSQL), Redis |
| **Infrastructure** | Kubernetes, Helm, Docker, Terraform, IaC |
| **DevOps** | CI/CD, GitHub Actions, Git |

---

## Experience

### Career Transition — .NET / Cloud Focus
**Independent** · Valencia, Spain · *September 2025 – Present*

- Relocating to Spain and adapting professionally, including actively learning Spanish to integrate into the local market.
- Refreshing modern .NET (C# / ASP.NET Core) and cloud development through hands-on practice, targeting .NET / Cloud engineering roles.
- Building a hands-on project (**payper**) to practice modern .NET with Azure AI: a C# backend with REST API and SQL persistence, using Azure AI Document Intelligence to extract structured data from receipt images — [github.com/dmitriyvetrov/payper](https://github.com/dmitriyvetrov/payper).
- Exploring Azure OpenAI and RAG patterns for data categorization, evaluating practical trade-offs of applying AI to real tasks.
- Coding challenge: [github.com/DmitriyVetrov/powerservice-challenge](https://github.com/DmitriyVetrov/powerservice-challenge).

**Stack:** C#, .NET / ASP.NET Core · Azure (AI Document Intelligence, OpenAI) · SQL · RAG

---

### Cloud DevOps Engineer
**SAP** · Bratislava, Slovakia (On-Site) · *February 2023 – September 2025*

- Developed a GitHub-based self-service workflow for on-demand provisioning of Kubernetes clusters; managed workloads via Helm (resource limits, rollout strategies).
- Built a Terraform framework for managing HashiCorp Vault policies (secrets management), with plan/apply pipelines triggered on approved PRs; wrote Python automation for security-token rotation.
- Built and operated CI/CD pipelines (GitHub Actions) — embedding security tests, building Docker images, and publishing to the corporate artifact registry (JFrog Artifactory).
- Automated security-agent rollout across all VMs (test/dev/prod, ~50 VMs total) using SaltStack, centralizing software-inventory reporting to a monitoring server; infrastructure provisioned via Terraform on an OpenStack-based private cloud (SAP Converged Cloud).
- Authored a Go-based Selenium integration test validating Kubernetes cluster health, embedded as a Job in the CI pipeline.

**Stack:** Kubernetes, Helm, Docker · Terraform, OpenStack (SAP Converged Cloud) · SaltStack · CI/CD, GitHub Actions, JFrog Artifactory · HashiCorp Vault · Python, Go

---

### .NET Full-Stack Engineer
**Cayzu Inc** · Windsor, Canada (Remote) · *October 2021 – December 2022*

- Designed and built a real-time chat feature (React + TypeScript + SignalR) within a multi-tenant SaaS customer-support platform.
- Developed backend features in C# / ASP.NET — including IP-based request filtering middleware with a cached, database-backed blacklist to improve platform stability.
- Initiated adoption of UI/UX best practices to improve usability and consistency.
- Wrote unit tests (xUnit) for backend features, improving coverage and regression safety.

**Stack:** C#, .NET Framework, ASP.NET, SignalR, xUnit · ReactJS, TypeScript, JavaScript, jQuery, HTML, CSS · Amazon SQS, Redis

---

### .NET Full-Stack Engineer
**Crif S.p.A** · Bratislava, Slovakia (On-Site) · *June 2017 – August 2021*

- Led migration of a legacy enterprise frontend to a modern ASP.NET-based web application, ensuring continuity of core business processes.
- Designed and built a modular .NET Core Corporate Admin Panel with a plugin-oriented architecture, enabling extensibility for other developers.
- Migrated and enhanced the Corporate Credit History Portal with dynamically generated, configurable reporting components.
- Owned report customization across 5 markets, implementing country-specific rules, layouts, and data structures.
- Covered core business logic with unit tests (xUnit) to safeguard country-specific reporting rules across markets.

**Stack:** C#, .NET Core, .NET Framework, ASP.NET, SOAP, xUnit · Bootstrap, jQuery, JavaScript, HTML, CSS · Oracle

---

### .NET Full-Stack Engineer
**StartMobile.co** · Montreal, Canada (Remote) · *October 2014 – December 2016*

- Built and deployed backend APIs (ASP.NET Web API, C#) to Azure App Services, managing environments, deployment configuration, and MS SQL Server integration.
- Built MVP mobile applications on .NET across the full lifecycle — web backend/frontend and mobile client (Xamarin.iOS).
- Implemented scalable media storage with Azure Blob Storage for user-generated content.
- Enabled real-time engagement via Azure Notification Hubs (push notifications).

**Stack:** ASP.NET Web API, .NET Framework, C#, Entity Framework · Azure (App Services, Blob Storage, Notification Hubs) · Xamarin.iOS · MS SQL Server · Razor, jQuery, Bootstrap · Stripe

---

### ERP Systems Consultant & Developer (1C:Enterprise)
**Ernst & Young, Nova Linia, etc.** · Ukraine (On-Site) · *January 2005 – January 2013*

- Customized ERP solutions on the 1C platform (CRM, trade, budgeting, HR) and automated business workflows.
- Integrated 1C:Enterprise with MS Dynamics AX via SOAP web services, ensuring consistent business data across platforms.
- Led ERP-to-ERP data migration between MS SQL systems, designing schema mapping and transformation logic.

---

## Education

**Master's Degree — Computer Science** · June 2004
IPAI (Institute of Artificial Intelligence Problems) · Kyiv, Ukraine · [ipai.net.ua/en](https://www.ipai.net.ua/en)
*Thesis:* web-based distance-learning / assessment system (PHP, MySQL, Apache)

---

## Languages

- **Ukrainian & Russian** — Native
- **English** — Professional Working Proficiency
- **Slovak** — Advanced
- **Spanish** — Basic Conversational
