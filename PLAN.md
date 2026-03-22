# NorthwindRAG - Implementation Plan

## Context

Build a RAG (Retrieval-Augmented Generation) chat application on top of the existing Northwind SQL Server database (SQL Server 15.0 on `LAPTOPX99100\duvan`). Users will ask natural language questions about products, customers, orders, employees, and suppliers, and get AI-powered answers grounded in real database content.

**Stack**: C# .NET 8 + Semantic Kernel + OpenAI + Angular

## Pre-step: Install RAG Skill

```bash
npx skills add davila7/claude-code-templates@rag-implementation -g -y
```

Skill passed all 3 security audits (Socket, Snyk, Gen Agent Trust Hub). Provides RAG best-practice patterns.

---

## Solution Structure

```
C:\Solution\NorthwindRAG\
├── NorthwindRAG.sln
├── src\
│   ├── NorthwindRAG.Api\              # ASP.NET Core Web API
│   ├── NorthwindRAG.Core\             # Models, interfaces, DTOs
│   ├── NorthwindRAG.Infrastructure\   # EF Core, Northwind DB access
│   ├── NorthwindRAG.SemanticKernel\   # SK orchestration, RAG pipeline
│   └── northwind-chat\               # Angular frontend
└── .gitignore
```

---

## Phase 1: Scaffold Projects

1. `dotnet new sln -n NorthwindRAG`
2. Create 4 .NET 8 projects (webapi for Api, classlib for the rest)
3. Add project references:
   - Api → Core, Infrastructure, SemanticKernel
   - Infrastructure → Core
   - SemanticKernel → Core, Infrastructure
4. Add NuGet packages:
   - **Infrastructure**: `EFCore`, `EFCore.SqlServer`
   - **SemanticKernel**: `Microsoft.SemanticKernel`, `Connectors.OpenAI`, `Plugins.Memory`, `Connectors.InMemory`
   - **Api**: `Swashbuckle.AspNetCore`
5. `ng new northwind-chat --routing --style=scss` for Angular

## Phase 2: Infrastructure Layer

6. Define EF entity classes: `Product`, `Category`, `Customer`, `Order`, `OrderDetail`, `Employee`, `Supplier` — mapped to existing Northwind schema with `[Table]`/`[Column]` attributes
7. Create `NorthwindDbContext` (read-only, no migrations, no-tracking by default)
8. Create `NorthwindRepository` with methods to load all entities with navigation properties
9. **Connection string**: `Server=LAPTOPX99100\\duvan;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True;`

## Phase 3: Core Models & Interfaces

10. `DocumentChunk` model with SK vector store attributes (Id, Text, Source, EntityId, Embedding[1536])
11. `ChatRequest` / `ChatResponse` DTOs
12. Interfaces: `IIngestionService`, `IRagService`, `IVectorStoreService`
13. Options: `OpenAIOptions` (ApiKey, ChatModel, EmbeddingModel)

## Phase 4: Semantic Kernel Layer (Core of RAG)

14. **VectorStoreService**: Wraps `InMemoryVectorStore`, collection `"northwind-chunks"`, methods for upsert and search
15. **IngestionService**: Reads Northwind entities → converts to text chunks → batch-embeds via OpenAI → stores in vector store
    - Chunking strategy: 1 chunk per entity row, natural language format
    - ~1036 total chunks (77 products + 91 customers + 830 orders + 9 employees + 29 suppliers)
16. **NorthwindQueryPlugin** (SK `[KernelFunction]`): LLM generates SELECT-only SQL for aggregate/numerical questions, validated before execution
17. **RagService** — the main pipeline:
    ```
    User question → Embed → Vector search (top-5) → Build augmented prompt → GPT-4 → Response
    ```
    - Optionally invokes NorthwindQueryPlugin via SK function-calling for numerical queries
18. System prompt in `Prompts/SystemPrompt.txt`

## Phase 5: API Layer

19. `Program.cs`: Register EF Core, Semantic Kernel, vector store, CORS (localhost:4200), Swagger
20. **Endpoints**:
    | Method | Route | Purpose |
    |--------|-------|---------|
    | POST | `/api/chat` | Main chat (body: `{message, conversationId}`) |
    | POST | `/api/ingestion/run` | Trigger data ingestion |
    | GET | `/api/ingestion/status` | Check if vectors are loaded |
    | GET | `/api/health` | Health check |
21. `appsettings.Development.json` for OpenAI key (never committed)

## Phase 6: Angular Frontend

22. Generate project with Angular CLI
23. `ChatService` — calls `POST /api/chat`
24. `ChatComponent` — scrollable message list, input bar, loading indicator, source citations
25. Dev proxy: `proxy.conf.json` routing `/api` → `https://localhost:7001`

## Phase 7: Verify End-to-End

26. Start API, call `POST /api/ingestion/run` to ingest Northwind data
27. Test sample queries:
    - "What products are in the Beverages category?"
    - "Who are the customers from Germany?"
    - "What is the most expensive product?"
    - "How many orders were shipped to France in 1997?"
28. Start Angular app, test full chat flow through the UI
29. Tune top-K, prompt template, and chunk format as needed

---

## Key Design Decisions

- **In-Memory vector store** for MVP (~1000 chunks is trivial). `IVectorStoreService` abstraction allows swapping to Qdrant/Azure AI Search later.
- **Hybrid RAG**: Vector search + optional SQL plugin for aggregate questions the LLM can't answer from text chunks alone.
- **text-embedding-3-small** (1536 dims): cheaper and faster than ada-002 with equal quality.
- **Read-only EF Core**: No migrations. Northwind DB already exists.
- **Stateless MVP**: No conversation history. Can be added later by passing message history in requests.

## Critical Files

- `src/NorthwindRAG.SemanticKernel/Services/RagService.cs` — RAG pipeline orchestration
- `src/NorthwindRAG.SemanticKernel/Services/IngestionService.cs` — Data ingestion
- `src/NorthwindRAG.SemanticKernel/Plugins/NorthwindQueryPlugin.cs` — SQL function-calling
- `src/NorthwindRAG.Infrastructure/Data/NorthwindDbContext.cs` — EF Core mapping
- `src/NorthwindRAG.Api/Program.cs` — DI wiring
- `src/northwind-chat/src/app/components/chat/` — Angular chat UI

---

## Agent Team Execution Guide

### Recommended Agent Assignments

**Agent 1 — Backend Scaffold** (Phase 1 steps 1-4):
- Create solution, projects, references, NuGet packages

**Agent 2 — Infrastructure + Core** (Phases 2-3, steps 6-13):
- EF entities, DbContext, repository, models, interfaces
- Depends on: Agent 1 completion

**Agent 3 — Semantic Kernel / RAG** (Phase 4, steps 14-18):
- VectorStoreService, IngestionService, RagService, NorthwindQueryPlugin
- Depends on: Agent 2 completion

**Agent 4 — API Layer** (Phase 5, steps 19-21):
- Program.cs, ChatController, configuration
- Depends on: Agent 3 completion

**Agent 5 — Angular Frontend** (Phase 6, steps 22-25):
- Can run in parallel with Agents 2-4 (independent)
- Angular project, ChatService, ChatComponent, proxy config

**Agent 6 — Integration & Testing** (Phase 7, steps 26-29):
- Depends on: All agents complete
- Build, run ingestion, test queries, verify UI
