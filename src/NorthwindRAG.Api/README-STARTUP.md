# NorthwindRAG API - Startup Instructions

## Prerequisites
- .NET 8+ SDK
- SQL Server with Northwind database at LAPTOPX99100\duvan
- OpenAI API key

## Configuration
Add your OpenAI API key to `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "ChatModel": "gpt-4o",
    "EmbeddingModel": "text-embedding-3-small"
  }
}
```

## Running the API
```bash
cd src/NorthwindRAG.Api
dotnet run
```

## Running the Angular Frontend
```bash
cd src/northwind-chat
npm install
npx ng serve
```
Then open http://localhost:4200

## Using the App
1. API starts at https://localhost:7200
2. Call POST /api/ingestion/run to load Northwind data into vector store (~1-2 min)
3. Check GET /api/ingestion/status — wait until `"status": "ready"`
4. Use Swagger UI at /swagger to test, or open the Angular app
5. Ask questions like:
   - "What products are in the Beverages category?"
   - "Who are the customers from Germany?"
   - "What is the most expensive product?"
