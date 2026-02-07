## Plan: Backend Rebuild with Comprehensive Tests

Modernize the .NET 8 backend with Clean Architecture, comprehensive testing (236+ tests), and security hardening. Fix critical bugs (magic link tokens), enforce role-based authorization, and migrate from manual SQL to EF Core migrations.

**Current State:**
- ❌ Zero backend tests
- ❌ Security gaps (no role enforcement, clients can impersonate parents)
- ❌ Bug: Base64 magic link tokens break in URLs (`+` → space)
- ❌ Manual SQL migrations instead of EF Core
- ❌ DateTime for dates causes timezone issues
- ❌ Mixed naming conventions, unused packages

**Target State:**
- ✅ 250+ unit & integration tests with xUnit + FluentAssertions
- ✅ Clean Architecture with Application layer
- ✅ Role-based authorization policies (Admin, ParentA, ParentB)
- ✅ Base64URL tokens + proper URL encoding
- ✅ DateOnly for calendar dates
- ✅ FluentValidation for requests
- ✅ EF Core migrations as source of truth
- ✅ Result pattern for error handling

**Phases (11 phases)**

1. **Phase 1: Test Infrastructure Setup**
   - **Objective:** Create test project with xUnit, FluentAssertions, and in-memory SQLite for integration tests
   - **Files/Functions to Create:**
     - `backend/CoParenting.Tests/CoParenting.Tests.csproj` - Test project with xUnit, FluentAssertions, Moq, EF InMemory, WebApplicationFactory
     - `backend/CoParenting.Tests/Fixtures/TestWebApplicationFactory.cs` - Custom WebApplicationFactory for integration tests
     - `backend/CoParenting.Tests/Fixtures/DatabaseFixture.cs` - In-memory database fixture
     - `backend/CoParenting.Tests/_Imports.cs` - Global usings for test project
   - **Tests to Write:**
     - `backend/CoParenting.Tests/Fixtures/TestWebApplicationFactory.Tests.cs` - Verify factory creates app successfully
     - `backend/CoParenting.Tests/Fixtures/DatabaseFixture.Tests.cs` - Verify test database spins up
   - **Steps:**
     1. Create `CoParenting.Tests.csproj` with packages: xUnit, xunit.runner.visualstudio, FluentAssertions, Moq, Microsoft.EntityFrameworkCore.InMemory, Microsoft.AspNetCore.Mvc.Testing
     2. Add project reference to CoParenting.API
     3. Create `TestWebApplicationFactory<Program>` with in-memory database configuration
     4. Create `DatabaseFixture` using EF Core InMemory provider for tests
     5. Write tests to verify factory and database fixture work
     6. Run tests to see them pass (green)

2. **Phase 2: Create Application Layer & Move Business Logic**
   - **Objective:** Extract business logic from API into new Application layer following Clean Architecture
   - **Files/Functions to Create:**
     - `backend/CoParenting.Application/CoParenting.Application.csproj` - New Application layer project
     - `backend/CoParenting.Application/Interfaces/IDayAssignmentService.cs` - Interface for service
     - `backend/CoParenting.Application/Services/DayAssignmentService.cs` - Moved from API layer
     - `backend/CoParenting.Application/Services/CommentService.cs` - Moved from API layer
     - `backend/CoParenting.Application/Services/ConfigurationService.cs` - Moved from API layer
     - `backend/CoParenting.Application/Services/AuthService.cs` - Moved from API layer
     - `backend/CoParenting.Application/DTOs/` - Move all DTOs from API to Application
     - Update `backend/CoParenting.API/Program.cs` - Reference Application layer services
   - **Tests to Write:**
     - Tests will be added in later phases; this phase only moves code
   - **Steps:**
     1. Create `CoParenting.Application` project with reference to Core and Infrastructure
     2. Move all services from `CoParenting.API/Services/` to `CoParenting.Application/Services/`
     3. Move all DTOs from `CoParenting.API/DTOs/` to `CoParenting.Application/DTOs/`
     4. Create service interfaces in `CoParenting.Application/Interfaces/`
     5. Update `Program.cs` to register services from Application layer
     6. Update all endpoint files to use Application layer namespaces
     7. Build solution to verify no compilation errors

3. **Phase 3: Fix Authentication - Part 1 (Base64URL Tokens & Authorization Policies)**
   - **Objective:** Fix critical magic link bug by using Base64URL tokens and add role-based authorization policies
   - **Files/Functions to Modify:**
     - `backend/CoParenting.Application/Services/AuthService.cs`
       - `CreateMagicLinkAsync()` - Use Base64URL encoding
       - `CreateSessionToken()` - Use Base64URL encoding
     - `backend/CoParenting.API/Program.cs`
       - Add authorization policies: `RequireAdminRole`, `RequireParentRole`
   - **Tests to Write:**
     - `backend/CoParenting.Tests/Unit/Services/AuthServiceTests.cs`
       - Test token generation produces Base64URL (no `+`, `/`, `=`)
       - Test magic link tokens are URL-safe
       - Test session token creation/validation
       - Test token expiry logic (24h magic, 30d admin, 90d parent)
       - Test admin password validation (BCrypt)
   - **Steps:**
     1. Write failing tests for Base64URL token generation
     2. Create helper method `GenerateBase64UrlToken()` using `Convert.ToBase64String().Replace('+', '-').Replace('/', '_').TrimEnd('=')`
     3. Update `CreateMagicLinkAsync()` to use Base64URL token
     4. Update `CreateSessionToken()` in admin/magic exchange to use Base64URL
     5. Run tests to verify they pass
     6. Add authorization policies in `Program.cs`: `builder.Services.AddAuthorization(options => { options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin")); })`
     7. Build and verify compilation

4. **Phase 4: Fix Authentication - Part 2 (Claim-Based Auth & Role Enforcement)**
   - **Objective:** Use HttpContext.User claims instead of re-validating tokens; enforce role-based authorization
   - **Files/Functions to Modify:**
     - `backend/CoParenting.API/Endpoints/AuthEndpoints.cs`
       - `GET /api/auth/me` - Use HttpContext.User claims instead of re-validating token
     - `backend/CoParenting.API/Endpoints/AdminEndpoints.cs` (rename from portions of AuthEndpoints)
       - Apply `RequireAuthorization("RequireAdminRole")` to all admin endpoints
     - `backend/CoParenting.API/Endpoints/DayAssignmentEndpoints.cs`
       - Apply `RequireAuthorization("RequireParentRole")` (allows all authenticated users for now)
     - `backend/CoParenting.API/Endpoints/CommentEndpoints.cs`
       - Derive `Parent` from HttpContext.User role (remove from request body)
   - **Tests to Write:**
     - `backend/CoParenting.Tests/Integration/AuthEndpointsTests.cs`
       - Test admin login returns session token
       - Test `/api/auth/me` returns user from claims (not re-query)
       - Test logout invalidates session
       - Test 401 for invalid tokens
     - `backend/CoParenting.Tests/Integration/AdminEndpointsTests.cs`
       - Test magic link creation requires Admin role (403 for parents)
       - Test get users requires Admin role
       - Test get magic links requires Admin role
   - **Steps:**
     1. Write failing integration tests for auth endpoints with role checks
     2. Refactor `/api/auth/me` to read from `HttpContext.User.Claims` instead of calling `ValidateSessionAsync` again
     3. Extract admin endpoints to separate file/group
     4. Apply `.RequireAuthorization("RequireAdminRole")` to admin endpoints
     5. Update comment creation to derive parent from `HttpContext.User.FindFirst(ClaimTypes.Role)?.Value`
     6. Run tests to verify they pass
     7. Test manually that ParentA cannot create magic links

5. **Phase 5: Unit Tests for Day Assignment Logic**
   - **Objective:** Test week-number calculation, default assignment pattern, month initialization, statistics
   - **Files/Functions to Test:**
     - `backend/CoParenting.Application/Services/DayAssignmentService.cs`
       - `GetWeekNumber(date)` - Week calculation logic
       - `InitializeMonthWithDefaultsAsync(year, month)` - Odd/even week pattern
       - `GetYearStatisticsAsync(year)` - Statistics calculations
   - **Tests to Write:**
     - `backend/CoParenting.Tests/Unit/Services/DayAssignmentServiceTests.cs`
       - Test week number calculation for various dates (month starts Mon/Tue/Sun edge cases)
       - Test month initialization creates all days with correct odd/even pattern
       - Test upsert day updates existing or creates new
       - Test statistics count parent A/B days correctly
       - Test statistics count VAB days correctly
       - Test statistics count unassigned days correctly
       - Test statistics count days with comments correctly
       - Test month data includes all days and comments
   - **Steps:**
     1. Write failing tests for `GetWeekNumber()` with edge cases (Feb 2026 starts on Sunday)
     2. Write failing tests for `InitializeMonthWithDefaultsAsync()` verifying odd/even pattern
     3. Create test helper to build mock DbContext with InMemory provider
     4. Mock `CoParentingDbContext` with test data
     5. Write tests to verify statistics calculations are accurate
     6. Run tests and verify they pass (existing logic should pass)
     7. Document any quirks in week calculation behavior

6. **Phase 6: Unit Tests for Comments & Configuration**
   - **Objective:** Test comment CRUD, configuration get/set, parent name management
   - **Files/Functions to Test:**
     - `backend/CoParenting.Application/Services/CommentService.cs` - All methods
     - `backend/CoParenting.Application/Services/ConfigurationService.cs` - All methods
   - **Tests to Write:**
     - `backend/CoParenting.Tests/Unit/Services/CommentServiceTests.cs`
       - Test add comment creates with correct parent and timestamp
       - Test get comments for day returns ordered by CreatedAt
       - Test update comment changes text and ModifiedAt
       - Test delete comment removes from database
     - `backend/CoParenting.Tests/Unit/Services/ConfigurationServiceTests.cs`
       - Test get value by key
       - Test set value creates or updates
       - Test get parent names returns both names
       - Test set parent names updates both configurations
   - **Steps:**
     1. Write failing tests for comment CRUD operations
     2. Mock `CoParentingDbContext.Comments` DbSet
     3. Verify comment service methods interact correctly with DbContext
     4. Write failing tests for configuration service
     5. Mock `CoParentingDbContext.Configurations` DbSet
     6. Verify configuration service methods work correctly
     7. Run tests to verify they pass

7. **Phase 7: Integration Tests for API Endpoints**
   - **Objective:** Test full HTTP request/response cycles with real database
   - **Files/Functions to Test:**
     - All endpoints in `CoParenting.API/Endpoints/`
   - **Tests to Write:**
     - `backend/CoParenting.Tests/Integration/DayAssignmentEndpointsTests.cs`
       - Test GET month returns days with correct JSON shape
       - Test GET specific day returns 200 or 404
       - Test PUT day creates/updates and returns updated day
       - Test POST initialize creates days with default pattern
       - Test all day endpoints require authentication (401 without token)
     - `backend/CoParenting.Tests/Integration/CommentEndpointsTests.cs`
       - Test POST comment creates with derived parent from role
       - Test GET comments returns list for day
       - Test PUT comment updates text
       - Test DELETE comment removes
       - Test all require authentication
     - `backend/CoParenting.Tests/Integration/ConfigurationEndpointsTests.cs`
       - Test GET parent names returns names
       - Test PUT parent names updates configuration
       - Test requires authentication
     - `backend/CoParenting.Tests/Integration/StatisticsEndpointsTests.cs`
       - Test GET year statistics returns correct counts
       - Test requires authentication
   - **Steps:**
     1. Write failing integration tests using TestWebApplicationFactory
     2. Use EF Core InMemory database for test isolation
     3. Create test helper to generate auth tokens for Admin, ParentA, ParentB
     4. Test each endpoint with valid/invalid tokens
     5. Verify JSON serialization matches frontend expectations
     6. Run tests to verify they pass
     7. Ensure at least 250+ total tests across unit + integration

8. **Phase 8: Data Model Improvements & EF Core Migrations**
   - **Objective:** Switch to DateOnly for dates, add FluentValidation, migrate to EF Core migrations
   - **Files/Functions to Modify:**
     - `backend/CoParenting.Core/Entities/DayAssignment.cs` - Change `Date` from `DateTime` to `DateOnly`
     - `backend/CoParenting.Infrastructure/Data/CoParentingDbContext.cs` - Update configurations, remove HasData seeds
     - `backend/CoParenting.Application/DTOs/` - Update all DTOs to use `DateOnly` or string dates
     - `backend/CoParenting.Application/Validators/` - Create FluentValidation validators
     - Remove `backend/init-db.sql` and `backend/migrations/002-add-authentication.sql`
     - Create EF Core migration: `dotnet ef migrations add InitialCreate`
   - **Tests to Write:**
     - `backend/CoParenting.Tests/Unit/Validators/UpdateDayAssignmentDtoValidatorTests.cs`
       - Test parent must be "A", "B", or null
       - Test specialStatus has max length
     - `backend/CoParenting.Tests/Unit/Validators/CreateCommentDtoValidatorTests.cs`
       - Test commentText is required and max 1000 chars
     - Update all existing tests to use DateOnly
   - **Steps:**
     1. Install FluentValidation.AspNetCore package in Application project
     2. Create validators for all DTOs
     3. Write failing tests for validators
     4. Update entity from DateTime to DateOnly
     5. Update all DTOs and services to use DateOnly.FromDateTime where needed
     6. Update DbContext configuration for DateOnly column type
     7. Update all unit tests to use DateOnly
     8. Delete manual SQL scripts
     9. Create initial EF Core migration: `dotnet ef migrations add InitialCreate -p CoParenting.Infrastructure -s CoParenting.API`
     10. Run tests to verify all pass
     11. Build solution and verify API still works

9. **Phase 9: ChangeRequest Negotiation Feature (Backend + Frontend)**
   - **Objective:** Implement day-swap negotiation between parents with full workflow
   - **Files/Functions to Create (Backend):**
     - `backend/CoParenting.Application/Services/ChangeRequestService.cs` - Business logic for change requests
     - `backend/CoParenting.Application/DTOs/ChangeRequestDtos.cs` - Request/response DTOs
     - `backend/CoParenting.API/Endpoints/ChangeRequestEndpoints.cs` - API endpoints
   - **Files/Functions to Create (Frontend):**
     - `frontend/src/api/changeRequests.ts` - API client
     - `frontend/src/components/ChangeRequestButton.tsx` - Button in DayModal to propose swap
     - `frontend/src/components/ChangeRequestsList.tsx` - List pending/responded requests
     - `frontend/src/pages/ChangeRequestsPage.tsx` - Dedicated page for managing requests
   - **Tests to Write (Backend):**
     - `backend/CoParenting.Tests/Unit/Services/ChangeRequestServiceTests.cs`
       - Test create request by ParentA for ParentB's day
       - Test cannot create request for own day
       - Test review request (accept/reject) only by target parent
       - Test accept updates day assignment
       - Test reject marks as rejected
       - Test get pending requests for user
     - `backend/CoParenting.Tests/Integration/ChangeRequestEndpointsTests.cs`
       - Test POST /api/change-requests creates request (ParentA → ParentB)
       - Test GET /api/change-requests returns user's requests (requested by OR for them)
       - Test PUT /api/change-requests/{id}/review accepts/rejects
       - Test 403 when wrong parent tries to review
       - Test 400 when reviewing already-reviewed request
   - **Tests to Write (Frontend):**
     - `frontend/src/components/ChangeRequestButton.test.tsx` - Component renders, opens dialog
     - `frontend/src/components/ChangeRequestsList.test.tsx` - Lists requests with accept/reject buttons
     - `frontend/src/pages/ChangeRequestsPage.test.tsx` - Full page with filtering
     - `frontend/src/api/changeRequests.test.ts` - API client methods
   - **Steps:**
     1. Write failing backend tests for ChangeRequestService
     2. Implement ChangeRequestService with workflow:
        - CreateRequestAsync(date, requestedByUserId, reason) - validates day is assigned to other parent
        - GetRequestsForUserAsync(userId) - returns pending requests where user is requester OR target
        - ReviewRequestAsync(requestId, userId, isApproved, responseReason) - validates user is target parent
        - On approve: updates DayAssignment.Parent to requester's parent value
     3. Create endpoint: POST /api/change-requests, GET /api/change-requests, PUT /api/change-requests/{id}/review
     4. Write failing integration tests and verify endpoints work
     5. Create frontend API client with create, list, review methods
     6. Create ChangeRequestButton in DayModal (only shows if day assigned to other parent)
     7. Create ChangeRequestsList component showing pending incoming/outgoing requests
     8. Create ChangeRequestsPage with tabs: "Incoming" (for me to review), "Outgoing" (I requested)
     9. Add route /change-requests to App.tsx
     10. Write frontend component tests
     11. Run all 280+ tests (backend + frontend) to verify they pass

**Open Questions (1 question)**
1. Week calculation quirk: Current `GetWeekNumber()` behaves oddly for months starting on Sunday (e.g. Feb 2026). Should we preserve exact behavior or fix to use ISO week numbers? **→ Will preserve exact behavior for backwards compatibility**
