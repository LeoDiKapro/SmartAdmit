sequenceDiagram
    participant Browser
    participant Account as "AccountController (Auth)"
    participant StudentUI as "Student (Application flows)"
    participant RepUI as "UniversityRep (Rep flows)"
    participant AdminUI as "Admin (Admin flows)"
    participant AppCtrl as "ApplicationController"
    participant RepCtrl as "RepController"
    participant AdminCtrl as "AdminController"
    participant Claude as "ClaudeService"
    participant Scoring as "ScoringService"
    participant Db as "AppDbContext"
    participant FS as "File System (wwwroot/uploads)"
    participant Identity as "Identity / UserManager"

    % Anonymous / login
    Browser->>Account: GET /Account/Login
    Account-->>Browser: Login page
    Browser->>Account: POST /Account/Login (credentials)
    Account->>Identity: Authenticate user
    Identity-->>Account: SignIn result
    Account-->>Browser: Redirect (role-specific dashboard)

    %% STUDENT role flows
    Browser->>AppCtrl: GET /Application/Apply
    AppCtrl->>Db: Query Universities & Programs
    Db-->>AppCtrl: Lists
    AppCtrl-->>Browser: Render Apply view

    Browser->>AppCtrl: POST /Application/Apply (form + files)
    AppCtrl->>Identity: Get current Student
    AppCtrl->>Db: Check duplicate application
    Db-->>AppCtrl: existingApplication / null
    AppCtrl->>Db: Add Application entity
    Db-->>AppCtrl: Saved (Id)
    AppCtrl->>FS: Save uploaded files (/uploads/{appId})
    AppCtrl->>Db: Add Document & Language records
    Db-->>AppCtrl: Persisted
    AppCtrl-->>Browser: Redirect to UploadDocuments

    Browser->>AppCtrl: POST /Application/FinalSubmit (applicationId)
    AppCtrl->>Db: Load Application + MasterProgram
    Db-->>AppCtrl: Data
    AppCtrl->>AppCtrl: Auto-screen (GPA/Years)
    AppCtrl->>Db: Update Status, SubmittedAt
    Db-->>AppCtrl: Saved
    AppCtrl-->>Browser: Redirect Dashboard (message)

    Browser->>AppCtrl: POST /Application/Chat (messages)
    AppCtrl->>Db: Load apps, languages, programs
    Db-->>AppCtrl: Context data
    AppCtrl->>Claude: Chat(history, systemPrompt)
    Claude->>Claude: HTTP POST to Anthropic API
    Claude-->>AppCtrl: Reply
    AppCtrl-->>Browser: JSON { reply }

    %% UNIVERSITY REP role flows
    Browser->>RepCtrl: GET /Rep/Dashboard
    RepCtrl->>Identity: Get current Rep (UniversityId)
    RepCtrl->>Db: Query Applications for University
    Db-->>RepCtrl: Applications list
    RepCtrl-->>Browser: Render Rep dashboard

    Browser->>RepCtrl: GET /Rep/Rankings?programId=#
    RepCtrl->>Db: Load MasterProgram, ScoringWeights, Applications
    Db-->>RepCtrl: Data
    RepCtrl->>Scoring: RankApplications(apps, weights)
    Scoring-->>RepCtrl: Ranked results
    RepCtrl-->>Browser: Render Rankings

    Browser->>RepCtrl: POST /Rep/GenerateSummary (appId)
    RepCtrl->>Db: Load Application + Documents + Languages
    Db-->>RepCtrl: Data
    RepCtrl->>Claude: GenerateApplicationSummary(...)
    Claude-->>RepCtrl: Summary text
    RepCtrl-->>Browser: JSON { summary }

    Browser->>RepCtrl: POST /Rep/Accept || /Rep/Reject || /Rep/SaveNotes
    RepCtrl->>Db: Update Application.Status / Notes / ReviewedAt
    Db-->>RepCtrl: Saved
    RepCtrl-->>Browser: Redirect (success)

    %% ADMIN role flows
    Browser->>AdminCtrl: GET /Admin/Universities
    AdminCtrl->>Db: Query Universities (+Programs)
    Db-->>AdminCtrl: Data
    AdminCtrl-->>Browser: Render Universities view

    Browser->>AdminCtrl: POST /Admin/CreateUniversity
    AdminCtrl->>Db: Add University; SaveChanges
    Db-->>AdminCtrl: Persisted
    AdminCtrl-->>Browser: Redirect (success)

    Browser->>AdminCtrl: GET /Admin/CreateStaff
    AdminCtrl-->>Browser: Render CreateStaff view
    Browser->>AdminCtrl: POST /Admin/CreateStaff (email, pw, uniId)
    AdminCtrl->>Identity: Create AppUser and AddToRole("UniversityRep")
    Identity-->>AdminCtrl: Result
    AdminCtrl->>Db: (optional) set UniversityId on user
    AdminCtrl-->>Browser: Redirect Staff (success)

    %% Shared startup wiring
    Note over Identity,Db: Services wired in `Program.cs`
    Program->>Db: Configure AppDbContext (SQLite)
    Program->>Identity: Add Identity stores
    Program->>Scoring: Add ScoringService
    Program->>Claude: AddHttpClient for ClaudeService