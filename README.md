# Engineering Request Tracking Module Pack

Portable Phase 1 module for a Vue + ASP.NET Core EF Core CAE dashboard.

## What Is Included

- EF Core entities for `Requests`, `Systems`, and `RequestNotes`.
- DTOs, services, and API controllers.
- Vue pages for request list, request details, systems register, and dashboard summary.
- SQL schema reference.
- Integration and development-order guide.

## Start Here

Read [docs/architecture.md](docs/architecture.md), then copy the backend and frontend folders into the matching areas of your work project.

The only expected adaptation is replacing placeholder namespaces and `TDbContext` with your existing dashboard namespace and EF Core DbContext name.
