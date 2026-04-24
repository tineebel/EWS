นี่คือไฟล์ CLAUDE.md ฉบับสมบูรณ์ที่เพิ่มส่วน Folder Structure ตามหลัก Clean Architecture เข้าไปเพื่อให้ Claude Code หรือ AI ตัวอื่น ๆ เข้าใจว่าไฟล์แต่ละประเภทควรถูกวางไว้ที่ไหนในโปรเจกต์ .NET ของคุณครับรายละเอ

CLAUDE.md — EWS (Enterprise Workflow System)
1. Project Overview 🌐
ระบบ Backend สำหรับจัดการ Flow การอนุมัติ (Approval Workflow) ภายในองค์กร

Core Concept: เปลี่ยนจาก User-based เป็น Position-Based (สิทธิ์ผูกกับตำแหน่ง ไม่ใช่บุคคล) เพื่อรองรับการสลับตัวบุคลากรได้ทันที

Business Goal: รองรับการทำ Org Chart Reporting Line, Auto-Escalation เมื่อตำแหน่งว่าง และระบบรักษาการแทน (Delegation)

Optimization: เน้น Data Integrity บน MS SQL และความโปร่งใสของ Audit Trail

2. Tech Stack 🛠️
Runtime: .NET 8/9 Core Web API (C#)

Database: MS SQL Server 2022+ (Schema: dbo, wf)

ORM: Entity Framework Core (Code First + Migrations)

Timezone: UTC+7 (Thailand) เสมอ ทุก DateTime ต้องเช็คเงื่อนไข Effective Date

3. Folder Structure (Clean Architecture) 📂
โครงสร้างโปรเจกต์แบ่งตามความรับผิดชอบ (Separation of Concerns) ดังนี้:

Plaintext
src/
├── Company.Project.Domain/              # 1. Core Business Entities (No dependencies)
│   ├── Entities/                        # Position, Employee, WorkflowInstance
│   ├── Enums/                           # WorkflowStatus, AssignmentType
│   ├── Common/                          # BaseEntity, ValueObjects
│   └── Exceptions/                      # Domain-specific exceptions
│
├── Company.Project.Application/         # 2. Business Logic & Use Cases
│   ├── Common/
│   │   ├── Interfaces/                  # IAppDbContext, IWorkflowEngine
│   │   ├── Behaviors/                   # Validation, Logging, Performance
│   │   └── Models/                      # Result<T>, PaginatedList
│   ├── Features/                        # Vertical Slices (Commands & Queries)
│   │   ├── Positions/
│   │   ├── Employees/
│   │   └── Workflows/                   # Submit, Approve, Reject logic
│   └── Validators/                      # FluentValidation rules
│
├── Company.Project.Infrastructure/      # 3. External Concerns (DB, Services)
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/              # EF Core Fluent API mappings
│   │   └── Migrations/                  # MS SQL Schema changes
│   ├── Services/                        # DateTimeService, EmailService
│   └── Logging/                         # Serilog configuration
│
└── Company.Project.API/                 # 4. Entry Point (Presentation)
    ├── Controllers/                     # REST Endpoints
    ├── Middlewares/                     # Global Error Handling, JSend Response
    ├── Filters/                         # Action filters
    └── Program.cs                       # Dependency Injection & Configuration
4. Coding Conventions 💻
Result Pattern: ทุก Service ต้องคืนค่าเป็น Result<T> ห้ามใช้ Exception คุม Business Flow

Async/Await: ต้องใช้ทุกจุดที่มี I/O (Database, External API)

Naming: PascalCase (C# Standard), Interface เริ่มด้วย I

Validation: ใช้ FluentValidation ตรวจสอบข้อมูลก่อนเข้าสู่ Domain Logic

5. API Design & Schema 📡
Pattern: RESTful API สื่อสารผ่าน JSON

Response Format (JSend):

status: "success", "fail" (4xx), "error" (5xx)

data: Payload ของข้อมูล (ห้ามเป็น Null, ให้ส่ง Empty Object/Array แทน)

message: ข้อความอธิบาย (จำเป็นสำหรับ fail/error)

Pagination: เริ่มต้นที่ page=1, คืนค่า metadata (totalRows, totalPage)

6. Error Messages & Logging ✍️
Status Codes: ใช้ HTTP Status Codes มาตรฐาน

Error Payloads: Error 400 ต้องระบุ field ที่ผิดและเหตุผลชัดเจน

Unique Code: ทุก Business Error ต้องมี Code เฉพาะ (เช่น WF_VACANT_001) เพื่อการทำ Localization

7. Workflow & Org Chart Logic ⚙️
Hierarchy: ทุกตำแหน่งต้องมี ParentPositionId เพื่อสร้างสายบังคับบัญชา
ทุกตำแหน่ง Chef จะมีเลขาทำหน้าที่ Review ก่อนเสมอ ะไม่นับว่าเป็นลำดับการอนุมัติ
ถ้าเลขาทำเอกสารให้ Chef ตัว Chef ต้อง Approve เอกสารก่อรวิ่ง Flow เสมอ และไม่นับว่าเป็นลำดับการอนุมัติ
Auto-Escalation: หากตำแหน่งเป้าหมายคือ Vacant ระบบต้อง Recursive ไปหาตำแหน่งหัวหน้าลำดับถัดไปจนกว่าจะเจอพนักงานจริง

Effective Assignment: เช็คสิทธิ์ด้วยเงื่อนไข StartDate <= Now <= EndDate เสมอ

8. Safety Rules ⚠️
No Hard-Delete: ห้ามลบข้อมูลพนักงานหรือตำแหน่ง ให้ใช้ IsActive หรือ EndDate แทน

Transaction: การอนุมัติที่กระทบหลายตารางต้องอยู่ภายใต้ IDbContextTransaction

Recursive Safety: การทำ Escalation ต้องมี MaxDepth (เช่น 10 ระดับ) ป้องกัน Infinite Loop

9. Commands ⌨️
dotnet run : เริ่มต้นระบบ API

dotnet ef migrations add [Name] : สร้าง Migration ใหม่

dotnet ef database update : อัปเดตโครงสร้าง DB เข้า MS SQL

dotnet test : รันชุดทดสอบทั้งหมด (Unit & Integration)

10. Security & Audit 🛡️
Delegation Audit: บันทึกทั้ง OriginalUserId (คนกด) และ ActingAsPositionId (ตำแหน่งที่ทำแทน)

Audit Trail: ตาราง WorkflowHistory และ AuditLogs ต้องเป็น Insert-Only (Immutable)