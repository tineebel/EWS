# EWS API Playbook — การใช้งาน API ตามเหตุการณ์จริง

> Base URL: `http://localhost:5062/api`
> Response format: JSend (`status`, `data`, `message`)
> Version: EWS v1 — Position-Based Workflow

---

## สารบัญ

1. [API Reference ทั้งหมด](#1-api-reference-ทั้งหมด)
2. [Enum / Status Values](#2-enum--status-values)
3. [Scenario: ส่งเอกสารปกติ (Normal Submit)](#3-scenario-ส่งเอกสารปกติ-normal-submit)
4. [Scenario: เลขาสร้างเอกสารให้ Chief](#4-scenario-เลขาสร้างเอกสารให้-chief)
5. [Scenario: อนุมัติเอกสาร (Approve)](#5-scenario-อนุมัติเอกสาร-approve)
6. [Scenario: ปฏิเสธเอกสาร (Reject)](#6-scenario-ปฏิเสธเอกสาร-reject)
7. [Scenario: ขอข้อมูลระหว่างอนุมัติ (Info Request)](#7-scenario-ขอข้อมูลระหว่างอนุมัติ-info-request)
8. [Scenario: ผู้อนุมัติลาออกหลัง Submit (Re-Escalate)](#8-scenario-ผู้อนุมัติลาออกหลัง-submit-re-escalate)
9. [Scenario: พนักงานลาออก (Bulk Re-Escalate)](#9-scenario-พนักงานลาออก-bulk-re-escalate)
10. [Scenario: ไม่มีผู้อนุมัติในสาย (Stuck → Manual Reassign)](#10-scenario-ไม่มีผู้อนุมัติในสาย-stuck--manual-reassign)
11. [Scenario: ตรวจสอบก่อน Submit (Simulate)](#11-scenario-ตรวจสอบก่อน-submit-simulate)
12. [Scenario: ดู Inbox ผู้อนุมัติ](#12-scenario-ดู-inbox-ผู้อนุมัติ)
13. [Scenario: ตรวจสอบสาย Hierarchy](#13-scenario-ตรวจสอบสาย-hierarchy)
14. [Scenario: ดูประวัติเอกสาร (Audit Trail)](#14-scenario-ดูประวัติเอกสาร-audit-trail)

---

## 1. API Reference ทั้งหมด

### Workflows

| Method | Endpoint | ชื่อ | ใช้ทำอะไร |
|--------|----------|------|-----------|
| `GET` | `/workflows` | List Instances | ดูรายการเอกสารทั้งหมด (กรองได้) |
| `GET` | `/workflows/{instanceId}` | Get Instance | ดูรายละเอียด + สถานะทุก Step |
| `GET` | `/workflows/{instanceId}/audit` | Audit Trail | ประวัติทุก Event (immutable) |
| `POST` | `/workflows/submit` | Submit | ส่งเอกสารเข้า Workflow |
| `POST` | `/workflows/{instanceId}/approve` | Approve | อนุมัติ Step ปัจจุบัน |
| `POST` | `/workflows/{instanceId}/reject` | Reject | ปฏิเสธเอกสาร |
| `POST` | `/workflows/{instanceId}/pre-approve` | Pre-Approve | Chief ยืนยัน/ปฏิเสธเอกสารที่เลขาสร้าง |
| `POST` | `/workflows/{instanceId}/re-escalate` | Re-Escalate | Re-route เอกสารเดี่ยว เมื่อผู้อนุมัติว่าง |
| `POST` | `/workflows/{instanceId}/steps/{stepOrder}/reassign` | Reassign Step | Admin กำหนดผู้อนุมัติใหม่ เมื่อ Stuck |
| `POST` | `/workflows/{instanceId}/steps/{stepOrder}/request-info` | Request Info | ขอข้อมูลจาก Step ก่อนหน้า |
| `POST` | `/workflows/info-requests/{infoRequestId}/respond` | Respond Info | ตอบ หรือ Forward ต่อ |
| `GET` | `/workflows/info-requests/pending` | Inbox Info | รายการที่ต้องตอบ (กรองตาม positionCode) |
| `GET` | `/workflows/{instanceId}/info-requests` | Info Thread | ดู thread การถาม-ตอบทั้งหมดของเอกสาร |

### Organization

| Method | Endpoint | ชื่อ | ใช้ทำอะไร |
|--------|----------|------|-----------|
| `GET` | `/organization/positions/{positionCode}/hierarchy` | Get Hierarchy | ดูสายบังคับบัญชาขึ้นถึง CEO |
| `GET` | `/organization/positions/{positionCode}/approval-chain` | Simulate Chain | Preview approval chain โดยไม่สร้าง Record |
| `GET` | `/organization/employees/{employeeCode}/current-position` | Current Position | ดูตำแหน่งปัจจุบันของพนักงาน |
| `POST` | `/organization/employees/{employeeCode}/re-escalate-pending` | Bulk Re-Escalate | Re-route เอกสารทั้งหมดของพนักงานที่ลาออก |

---

## 2. Enum / Status Values

### WorkflowStatus (สถานะ Instance)

| ค่า | ความหมาย |
|-----|-----------|
| `Draft` | เลขาสร้าง รอ Chief Pre-Approve ก่อนเริ่ม Flow |
| `Pending` | กำลังรออนุมัติอยู่ใน Step ใด Step หนึ่ง |
| `Approved` | อนุมัติครบทุก Step แล้ว |
| `Rejected` | ถูกปฏิเสธ (โดย Approver หรือ Chief reject Pre-Approval) |
| `Blocked` | มี Step ที่ Stuck (hierarchy หมด — รอ Admin reassign) |
| `Cancelled` | ถูกยกเลิก |
| `Recalled` | ผู้ส่งดึงเอกสารกลับ |

### ApprovalStatus (สถานะต่อ Step)

| ค่า | ความหมาย |
|-----|-----------|
| `Pending` | รออนุมัติ (อาจมี Info Request ค้างอยู่ก็ได้) |
| `Approved` | อนุมัติแล้ว |
| `Rejected` | ปฏิเสธแล้ว |
| `Delegated` | มอบหมายให้รักษาการ |
| `Escalated` | Escalate ขึ้นไปแล้ว |
| `Skipped` | ข้าม (ตาม Rule) |
| `Stuck` | Hierarchy หมดแล้ว รอ Admin reassign |

### InfoRequestStatus (สถานะ Info Request)

| ค่า | ความหมาย |
|-----|-----------|
| `Open` | รอผู้รับตอบ |
| `Forwarded` | ผู้รับ forward ต่อไปยัง Step ก่อนหน้า รอ child ตอบกลับ |
| `Closed` | ตอบแล้ว |
| `Cancelled` | ถูกยกเลิก (เช่น เจ้าของ Step กด Approve ก่อนตอบ) |

### PreApprovalStatus

| ค่า | ความหมาย |
|-----|-----------|
| `NotRequired` | ไม่ต้องทำ Pre-Approve |
| `Pending` | รอ Chief ยืนยัน |
| `Confirmed` | Chief ยืนยันแล้ว Flow เริ่มได้ |
| `Rejected` | Chief ปฏิเสธ → Instance เป็น Rejected |

---

## 3. Scenario: ส่งเอกสารปกติ (Normal Submit)

**สถานการณ์**: พนักงานส่งใบขออนุมัติซื้อของ

### Step 1 — ดู Preview ก่อนส่ง (optional)

```
GET /organization/positions/{positionCode}/approval-chain
    ?docCode=101
    &amount=45000
    &isSpecialItem=false
    &isUrgent=false
```

**Response** — แสดง Chain ทั้งหมด โดยไม่สร้าง Record:
```json
{
  "flowCode": 101,
  "flowDesc": "PO ≤ 50,000 (HO)",
  "steps": [
    {
      "stepOrder": 1,
      "stepName": "Section Manager",
      "resolvedPositionCode": "HOMGT10",
      "resolvedPositionName": "Section Manager - Finance",
      "wasEscalated": false,
      "occupantName": "สมชาย ใจดี",
      "isVacant": false
    }
  ]
}
```

### Step 2 — Submit เอกสาร

```
POST /workflows/submit
```
```json
{
  "docCode": 101,
  "submitterPositionCode": "HOSTAFF01",
  "submitterEmployeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "actingAsPositionCode": null,
  "totalAmount": 45000,
  "isSpecialItem": false,
  "isUrgent": false,
  "subject": "ขออนุมัติซื้อ Laptop",
  "remark": "สำหรับพนักงานใหม่",
  "isCreatedBySecretary": false
}
```

**Response 201** — ได้รับ `instanceId` + `approvalChain` ทั้งหมดทันที:
```json
{
  "instanceId": "a1b2c3d4-...",
  "documentNo": "PO-2026-00001",
  "flowCode": 101,
  "flowDesc": "PO ≤ 50,000 (HO)",
  "requiresPreApproval": false,
  "status": "Pending",
  "approvalChain": [
    {
      "stepOrder": 1,
      "stepName": "Section Manager",
      "approverPositionCode": "HOMGT10",
      "approverPositionName": "Section Manager - Finance",
      "occupantName": "สมชาย ใจดี",
      "wasEscalated": false,
      "delegatedToPositionCode": null
    }
  ]
}
```

> `approvalChain` แสดงผู้อนุมัติทุกคนตั้งแต่แรก ระบบ resolve ทั้ง chain ณ เวลา Submit

---

## 4. Scenario: เลขาสร้างเอกสารให้ Chief

**สถานการณ์**: เลขา (HOMGT22) สร้างเอกสารแทน Chief (HOCHIEF01)
ระบบตั้ง `status = Draft` รอ Chief กด Pre-Approve ก่อน Flow จึงจะเริ่ม

### Step 1 — เลขา Submit

```
POST /workflows/submit
```
```json
{
  "docCode": 201,
  "submitterPositionCode": "HOMGT22",
  "submitterEmployeeId": "...",
  "totalAmount": 200000,
  "isSpecialItem": false,
  "isUrgent": false,
  "subject": "ขออนุมัติโครงการ A",
  "isCreatedBySecretary": true
}
```

**Response** — `status: "Draft"`, `requiresPreApproval: true`

### Step 2 — Chief กด Pre-Approve

```
POST /workflows/{instanceId}/pre-approve
```
```json
{
  "chiefPositionCode": "HOCHIEF01",
  "chiefEmployeeId": "...",
  "isConfirmed": true,
  "comment": "ยืนยันส่งเอกสาร"
}
```

- `isConfirmed: true` → Instance เปลี่ยนเป็น `Pending`, Flow เริ่มเดิน
- `isConfirmed: false` → Instance เปลี่ยนเป็น `Rejected`, Flow ไม่เริ่ม

---

## 5. Scenario: อนุมัติเอกสาร (Approve)

**สถานการณ์**: ผู้อนุมัติได้รับ Notification มีเอกสารรออนุมัติ

### Step 1 — ดูรายการที่ต้องอนุมัติ (Inbox)

```
GET /workflows?status=Pending&positionCode=HOMGT10&page=1&pageSize=20
```

### Step 2 — ดูรายละเอียดเอกสาร

```
GET /workflows/{instanceId}
```

**Response** — แสดง Steps ทั้งหมดพร้อมสถานะ:
```json
{
  "status": "Pending",
  "currentStepOrder": 1,
  "currentApproverPositionCode": "HOMGT10",
  "steps": [
    { "stepOrder": 1, "status": "Pending", "approverPositionCode": "HOMGT10" },
    { "stepOrder": 2, "status": "Pending", "approverPositionCode": "HOMGT05" }
  ]
}
```

### Step 3 — อนุมัติ

```
POST /workflows/{instanceId}/approve
```
```json
{
  "actorPositionCode": "HOMGT10",
  "actorEmployeeId": "...",
  "comment": "อนุมัติ"
}
```

**Response**:
```json
{
  "instanceId": "...",
  "documentNo": "PO-2026-00001",
  "completedStep": 1,
  "isCompleted": false,
  "status": "Pending",
  "nextApproverPositionCode": "HOMGT05"
}
```

> ถ้า `isCompleted: true` และ `status: "Approved"` = ครบทุก Step แล้ว

> **หมายเหตุ**: ถ้า Step นี้มี Info Request ค้างอยู่ ระบบจะ auto-cancel ทั้งหมดเมื่อกด Approve

---

## 6. Scenario: ปฏิเสธเอกสาร (Reject)

**สถานการณ์**: ผู้อนุมัติไม่เห็นด้วย ปฏิเสธเอกสาร

```
POST /workflows/{instanceId}/reject
```
```json
{
  "actorPositionCode": "HOMGT10",
  "actorEmployeeId": "...",
  "comment": "งบประมาณไม่เพียงพอ กรุณา revise ใหม่"
}
```

- Instance เปลี่ยนเป็น `Rejected` ทันที
- Step อื่นที่ยัง Pending จะถูก Cancel ทั้งหมด
- Info Request ที่ค้างอยู่จะถูก Cancel ทั้งหมด
- `comment` **จำเป็น** (ต้องระบุเหตุผล)

---

## 7. Scenario: ขอข้อมูลระหว่างอนุมัติ (Info Request)

**สถานการณ์**: Step 5 ต้องการข้อมูลเพิ่มเติมจาก Step ก่อนหน้าก่อนตัดสินใจอนุมัติ

### กฎของ Info Request

| กฎ | รายละเอียด |
|----|-----------|
| ขอย้อนหลังเท่านั้น | `toStepOrder` ต้องน้อยกว่า `fromStepOrder` เสมอ |
| ถามหลาย Step พร้อมกันได้ | Step 5 ถาม Step 3 และ Step 1 พร้อมกันได้ |
| ห้ามถาม Step เดิมซ้ำ | ถ้า Step 3 ยังไม่ตอบ จะถาม Step 3 อีกไม่ได้ |
| ถามใหม่ได้หลังปิดแล้ว | เมื่อ request ปิด (Closed) ถาม Step เดิมซ้ำได้ |
| Approve แล้ว = cancel ทั้งหมด | Info requests ที่ค้างจะถูก auto-cancel เมื่อ Approve |

### ตัวอย่าง: Step 5 ถาม Step 3 และ Step 3 ถาม Step 1 ต่อ

```
Step 5 ──ask──► Step 3 ──ask──► Step 1
               (Forwarded)      (Open)
                                  │
                              ตอบกลับ
                                  │
               (Open อีกครั้ง) ◄──┘
                    │
                ตอบกลับ
                    │
Step 5 ◄────────────┘
(กลับมา Pending)
```

### Step 1 — Step 5 ขอข้อมูลจาก Step 3

```
POST /workflows/{instanceId}/steps/5/request-info
```
```json
{
  "toStepOrder": 3,
  "actorPositionCode": "HODIV01",
  "actorEmployeeId": "...",
  "question": "ขอให้ยืนยันว่างบประมาณส่วนนี้ได้รับการอนุมัติจาก Board แล้วหรือยัง"
}
```

**Response**:
```json
{
  "infoRequestId": 1,
  "instanceId": "...",
  "documentNo": "PO-2026-00001",
  "fromStepOrder": 5,
  "fromPositionCode": "HODIV01",
  "toStepOrder": 3,
  "toPositionCode": "HOMGT05",
  "toPositionName": "Finance Manager",
  "toOccupantName": "สมหญิง รักงาน",
  "question": "ขอให้ยืนยัน...",
  "status": "Open"
}
```

> Step 5 ยังคง `Pending` ตลอด — Info Request ไม่ block การ Approve

### Step 2 — ดู Inbox ของ Step 3 (ผู้ถูกถาม)

```
GET /workflows/info-requests/pending?positionCode=HOMGT05
```

**Response**:
```json
[
  {
    "infoRequestId": 1,
    "documentNo": "PO-2026-00001",
    "subject": "ขออนุมัติโครงการ A",
    "fromStepOrder": 5,
    "fromPositionCode": "HODIV01",
    "toStepOrder": 3,
    "question": "ขอให้ยืนยันว่างบประมาณ...",
    "status": "Open",
    "childInfoRequestId": null,
    "createdAt": "2026-03-19T10:00:00"
  }
]
```

### Step 3a — Step 3 ตอบตรง (ไม่ Forward)

```
POST /workflows/info-requests/1/respond
```
```json
{
  "actorPositionCode": "HOMGT05",
  "actorEmployeeId": "...",
  "answer": "ยืนยันแล้ว Board อนุมัติในที่ประชุมเมื่อวันที่ 15/03 เอกสารแนบ #BM2026-003"
}
```

**Response**: `{ "action": "Answered", ... }`
→ Request ปิด (Closed)

### Step 3b — Step 3 Forward ต่อไปยัง Step 1 (ยังไม่รู้คำตอบ)

```
POST /workflows/info-requests/1/respond
```
```json
{
  "actorPositionCode": "HOMGT05",
  "actorEmployeeId": "...",
  "forwardToStepOrder": 1,
  "forwardQuestion": "ขอให้ตรวจสอบว่า Budget Code ที่ใช้ตรงกับที่อนุมัติไว้ไหม"
}
```

**Response**:
```json
{
  "infoRequestId": 1,
  "action": "Forwarded",
  "forwardedToPositionCode": "HOMGT10",
  "forwardedToOccupantName": "สมชาย ใจดี",
  "childInfoRequestId": 2
}
```

→ Request 1 (5→3): `Forwarded`
→ Request 2 (3→1): `Open` — Step 1 ต้องตอบ

### Step 4 — Step 1 ตอบ

```
POST /workflows/info-requests/2/respond
```
```json
{
  "actorPositionCode": "HOMGT10",
  "actorEmployeeId": "...",
  "answer": "Budget Code BC-2026-047 ตรงกับที่อนุมัติไว้ครับ"
}
```

→ Request 2 (3→1): `Closed`
→ Request 1 (5→3): กลับมาเป็น `Open` — แจ้ง Step 3 ให้ตอบ Step 5

### Step 5 — Step 3 ตอบ Step 5 (หลังได้ข้อมูลจาก Step 1)

```
GET /workflows/info-requests/pending?positionCode=HOMGT05
```
→ จะเห็น Request 1 กลับมาเป็น Open พร้อม `childAnswer` จาก Step 1

```
POST /workflows/info-requests/1/respond
```
```json
{
  "actorPositionCode": "HOMGT05",
  "actorEmployeeId": "...",
  "answer": "ยืนยันแล้ว Budget Code ถูกต้อง BC-2026-047 ตรงกับที่ Board อนุมัติ"
}
```

→ Request 1 (5→3): `Closed`
→ Step 5 ยังคง `Pending` — กด Approve ได้เลย

### Step 6 — ดู Thread ทั้งหมด

```
GET /workflows/{instanceId}/info-requests
```

```json
[
  {
    "infoRequestId": 1,
    "fromStepOrder": 5,
    "toStepOrder": 3,
    "question": "ขอให้ยืนยันว่างบประมาณ...",
    "answer": "ยืนยันแล้ว Budget Code ถูกต้อง...",
    "status": "Closed",
    "depth": 0,
    "parentInfoRequestId": null,
    "childInfoRequestId": 2,
    "createdAt": "2026-03-19T10:00:00",
    "answeredAt": "2026-03-19T15:30:00"
  },
  {
    "infoRequestId": 2,
    "fromStepOrder": 3,
    "toStepOrder": 1,
    "question": "ขอให้ตรวจสอบว่า Budget Code...",
    "answer": "Budget Code BC-2026-047 ตรงกับที่อนุมัติไว้ครับ",
    "status": "Closed",
    "depth": 1,
    "parentInfoRequestId": 1,
    "childInfoRequestId": null,
    "createdAt": "2026-03-19T11:00:00",
    "answeredAt": "2026-03-19T14:00:00"
  }
]
```

> `depth: 0` = ต้นสาย, `depth: 1` = forward ต่อ 1 ทอด

### กรณี: Step 5 กด Approve ก่อนที่ Step 3 จะตอบ

ทำได้ — ระบบจะ auto-cancel info requests ที่ค้างทั้งหมดของ Step 5:

```
POST /workflows/{instanceId}/approve
```
→ Request 1 (5→3) และ Request 2 (3→1) ที่ยังค้าง: `Cancelled`
→ Audit Trail บันทึก `AUTO-CANCEL:APPROVED`

---

## 8. Scenario: ผู้อนุมัติลาออกหลัง Submit (Re-Escalate)

**สถานการณ์**: ส่งเอกสารไปแล้ว แต่ผู้อนุมัติ Step 2 ลาออกระหว่างรออนุมัติ

### ขั้นตอน

1. HR ทราบว่าตำแหน่ง `HOMGT05` ว่างแล้ว
2. เรียก Re-Escalate เอกสารนั้น:

```
POST /workflows/{instanceId}/re-escalate
```
```json
{
  "requestedByPositionCode": "HOADMIN01"
}
```

**Response**:
```json
{
  "instanceId": "...",
  "documentNo": "PO-2026-00001",
  "stepsReEscalated": 1,
  "stepsStuck": 0,
  "changes": [
    {
      "stepOrder": 2,
      "oldPositionCode": "HOMGT05",
      "newPositionCode": "HOMGT02",
      "newPositionName": "Division Director",
      "wasEscalated": true,
      "isStuck": false
    }
  ]
}
```

### Logic ที่ระบบทำ

```
สำหรับแต่ละ Pending Step:
  ├─ ตำแหน่งยังมีคน → ไม่เปลี่ยน
  ├─ ตำแหน่งว่าง → Walk up hierarchy หาคนถัดไป
  │   ├─ เจอคน → reassign step → stepsReEscalated++
  │   └─ hierarchy หมด → mark Stuck → stepsStuck++
  └─ Instance มี Stuck → status = Blocked
```

---

## 9. Scenario: พนักงานลาออก (Bulk Re-Escalate)

**สถานการณ์**: HR ทำ Off-boarding — ต้อง Re-route เอกสารค้างทั้งหมดของพนักงานคนนั้น

### ลำดับ Off-boarding ที่ถูกต้อง

```
1. HR deactivate PositionAssignment ของพนักงาน (ใน HR System)
2. เรียก Bulk Re-Escalate → ระบบ re-route เอกสารทั้งหมดอัตโนมัติ
3. ตรวจสอบผลลัพธ์ว่ามี Stuck step ไหม
4. ถ้ามี Stuck → ทำ Manual Reassign (Scenario 10)
```

### Bulk Re-Escalate

```
POST /organization/employees/{employeeCode}/re-escalate-pending
```
```json
{
  "requestedByPositionCode": "HOADMIN01"
}
```

**Response**:
```json
{
  "employeeCode": "EMP001",
  "employeeName": "สมชาย ใจดี",
  "affectedInstances": 3,
  "totalStepsReEscalated": 4,
  "totalStepsStuck": 1,
  "details": [
    {
      "instanceId": "...",
      "documentNo": "PO-2026-00001",
      "stepOrder": 1,
      "oldPositionCode": "HOMGT10",
      "newPositionCode": "HOMGT05",
      "newPositionName": "Finance Manager",
      "isStuck": false
    },
    {
      "instanceId": "...",
      "documentNo": "PR-2026-00042",
      "stepOrder": 3,
      "oldPositionCode": "HOTOP04",
      "newPositionCode": "HOTOP04",
      "isStuck": true
    }
  ]
}
```

> ถ้า `totalStepsStuck > 0` → มีเอกสารที่ต้อง Manual Reassign (ดู Scenario 10)

---

## 10. Scenario: ไม่มีผู้อนุมัติในสาย (Stuck → Manual Reassign)

**สถานการณ์**: Re-Escalate แล้วพบว่า hierarchy หมดชั้น ไม่มีคน
Step นั้นถูก mark `Stuck` และ Instance เป็น `Blocked`

### Step 1 — ดูเอกสารที่ Blocked

```
GET /workflows?status=Blocked&page=1
```

### Step 2 — ดูรายละเอียดว่า Step ไหน Stuck

```
GET /workflows/{instanceId}
```

```json
{
  "status": "Blocked",
  "steps": [
    { "stepOrder": 1, "status": "Approved" },
    { "stepOrder": 2, "status": "Stuck", "approverPositionCode": "HOTOP04" },
    { "stepOrder": 3, "status": "Pending" }
  ]
}
```

### Step 3 — Admin กำหนดผู้อนุมัติใหม่

```
POST /workflows/{instanceId}/steps/{stepOrder}/reassign
```
```json
{
  "targetPositionCode": "HODIRECTOR01",
  "requestedByPositionCode": "HOADMIN01",
  "reason": "CFO ว่าง — CEO มอบหมายให้ Director Finance ทำแทนชั่วคราว"
}
```

- Step กลับเป็น `Pending`
- ถ้าไม่มี Stuck step เหลือ → Instance กลับเป็น `Pending` อัตโนมัติ

---

## 11. Scenario: ตรวจสอบก่อน Submit (Simulate)

**สถานการณ์**: ดูว่าถ้าส่งเอกสาร จะต้องผ่านใครบ้าง โดยไม่สร้างเอกสารจริง

```
GET /organization/positions/{positionCode}/approval-chain
    ?docCode=301
    &amount=500000
    &isSpecialItem=true
    &isUrgent=false
```

**Response**:
```json
{
  "docCode": 301,
  "flowCode": 305,
  "flowDesc": "CapEx > 500,000 Special (HO)",
  "selectedScope": "HO",
  "steps": [
    {
      "stepOrder": 1,
      "approverType": "SectionManager",
      "resolvedPositionCode": "HOMGT10",
      "wasEscalated": false,
      "occupantName": "สมชาย ใจดี",
      "isVacant": false
    },
    {
      "stepOrder": 2,
      "approverType": "CLevel",
      "resolvedPositionCode": "HOCHIEF02",
      "wasEscalated": true,
      "escalationDepth": 2,
      "occupantName": "วิทยา มั่นคง",
      "isVacant": false
    }
  ]
}
```

> `wasEscalated: true` + `escalationDepth: 2` = ระบบต้องขึ้นไป 2 ระดับเพราะตำแหน่งกลางว่าง

---

## 12. Scenario: ดู Inbox ผู้อนุมัติ

**สถานการณ์**: แสดงรายการเอกสารและข้อมูลที่ต้องดำเนินการ

### Inbox อนุมัติ (เอกสารรออนุมัติ)

```
GET /workflows?status=Pending&positionCode=HOMGT10
```

### Inbox Info Request (รายการที่ต้องตอบข้อซักถาม)

```
GET /workflows/info-requests/pending?positionCode=HOMGT10
```

### กรองเฉพาะ Instance ใดอย่างเดียว

```
GET /workflows/info-requests/pending?positionCode=HOMGT10&instanceId={id}
```

### ดูตามประเภทเอกสาร

```
GET /workflows?status=Pending&positionCode=HOMGT10&docCode=101
```

**Response** — Paginated:
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalRows": 47,
  "totalPage": 3
}
```

---

## 13. Scenario: ตรวจสอบสาย Hierarchy

```
GET /organization/positions/{positionCode}/hierarchy
```

**Response** — ระดับ 0 = ตำแหน่งที่ query, ขึ้นไปถึง CEO:
```json
[
  { "level": 0, "positionCode": "HOSTAFF01", "jobGrade": "D1", "isVacant": false, "currentOccupant": "สมศรี" },
  { "level": 1, "positionCode": "HOMGT10",   "jobGrade": "B2", "isVacant": false },
  { "level": 2, "positionCode": "HOMGT05",   "jobGrade": "A3", "isVacant": true  },
  { "level": 3, "positionCode": "HODIV01",   "jobGrade": "A2", "isVacant": false }
]
```

> `isVacant: true` = ตำแหน่งนี้จะถูกข้ามเมื่อ Auto-Escalate

### ดูตำแหน่งปัจจุบันของพนักงาน

```
GET /organization/employees/{employeeCode}/current-position
```

---

## 14. Scenario: ดูประวัติเอกสาร (Audit Trail)

```
GET /workflows/{instanceId}/audit
```

**Response** — เรียงตาม `occurredAt` (Immutable, Insert-Only):
```json
[
  { "eventType": "Submit",                    "actorPositionCode": "HOSTAFF01", "occurredAt": "2026-03-19T09:00:00" },
  { "eventType": "InfoRequest",   "stepOrder": 5, "actorPositionCode": "HODIV01",  "comment": "Step 5 → Step 3: ขอให้ยืนยัน..." },
  { "eventType": "InfoRequest:Forward",       "actorPositionCode": "HOMGT05",   "comment": "Step 3 forwarded to Step 1: ขอให้ตรวจสอบ..." },
  { "eventType": "InfoRequest:Answered",      "actorPositionCode": "HOMGT10",   "comment": "Step 1 answered Step 3: Budget Code ตรงกัน" },
  { "eventType": "InfoRequest:ChainResume",   "actorPositionCode": "HOMGT05",   "comment": "Step 3 received answer. Resume answering Step 5." },
  { "eventType": "InfoRequest:Answered",      "actorPositionCode": "HOMGT05",   "comment": "Step 3 answered Step 5: ยืนยันแล้ว" },
  { "eventType": "Approve",       "stepOrder": 5, "actorPositionCode": "HODIV01",  "comment": "อนุมัติ" }
]
```

### EventType ทั้งหมด

| EventType | เกิดจาก |
|-----------|---------|
| `Submit` | ส่งเอกสาร |
| `PreApprove:Confirmed` | Chief ยืนยัน Pre-Approval |
| `PreApprove:Rejected` | Chief ปฏิเสธ Pre-Approval |
| `Approve` | อนุมัติ Step |
| `Complete` | อนุมัติ Step สุดท้าย — เอกสารสำเร็จ |
| `AutoApproved` | ไม่มี Step เหลือ → Auto-Complete |
| `Reject` | ปฏิเสธเอกสาร |
| `InfoRequest` | ขอข้อมูลจาก Step ก่อนหน้า |
| `InfoRequest:Forward` | Forward ต่อไปยัง Step ก่อนหน้าอีกทอด |
| `InfoRequest:Answered` | ตอบข้อซักถาม |
| `InfoRequest:ChainResume` | child ตอบแล้ว — แจ้ง parent ให้ตอบต่อ |
| `InfoRequest:StepResumed` | root request ปิด — Step กลับมา Pending |
| `ReEscalate` | Re-route Step ไปตำแหน่งใหม่ |
| `ReEscalate:Blocked` | Re-route แล้วพบ Stuck Step |
| `ReEscalate:Stuck` | Hierarchy หมด — Step นี้ Stuck |
| `Reassign` | Admin กำหนดผู้อนุมัติใหม่ด้วยตนเอง |

---

## Quick Reference — เหตุการณ์ → API

| เหตุการณ์ | API ที่ต้องเรียก |
|-----------|-----------------|
| พนักงาน Submit เอกสาร | `POST /workflows/submit` |
| เลขาสร้างเอกสารแทน Chief | `POST /workflows/submit` (`isCreatedBySecretary: true`) |
| Chief ยืนยัน/ปฏิเสธเอกสารของเลขา | `POST /workflows/{id}/pre-approve` |
| ผู้อนุมัติอนุมัติ | `POST /workflows/{id}/approve` |
| ผู้อนุมัติปฏิเสธ | `POST /workflows/{id}/reject` |
| ขอข้อมูลจาก Step ก่อนหน้า | `POST /workflows/{id}/steps/{order}/request-info` |
| ตอบข้อซักถาม / Forward ต่อ | `POST /workflows/info-requests/{id}/respond` |
| ดู Inbox ข้อซักถามที่ต้องตอบ | `GET /workflows/info-requests/pending?positionCode={code}` |
| ดู thread การถาม-ตอบของเอกสาร | `GET /workflows/{id}/info-requests` |
| ตำแหน่งผู้อนุมัติว่างหลัง Submit | `POST /workflows/{id}/re-escalate` |
| **พนักงานลาออก (Off-boarding)** | `POST /organization/employees/{code}/re-escalate-pending` |
| Hierarchy หมด มี Stuck step | `POST /workflows/{id}/steps/{order}/reassign` |
| ดู Inbox รออนุมัติ | `GET /workflows?status=Pending&positionCode={code}` |
| ดูสถานะเอกสาร | `GET /workflows/{id}` |
| ดูประวัติเอกสาร | `GET /workflows/{id}/audit` |
| Preview chain ก่อนส่ง | `GET /organization/positions/{code}/approval-chain?docCode=...` |
| ตรวจสอบสาย Hierarchy | `GET /organization/positions/{code}/hierarchy` |
| ดูตำแหน่งปัจจุบันพนักงาน | `GET /organization/employees/{code}/current-position` |

---

## Flow Diagram — ชีวิตของเอกสาร

```
[Submit]
   │
   ├─ isCreatedBySecretary=false ──────────► status=Pending
   │
   └─ isCreatedBySecretary=true ─────────► status=Draft
                                                  │
                                          Chief Pre-Approve
                                          ├─ Confirmed → status=Pending
                                          └─ Rejected  → status=Rejected ✗

status=Pending  [Step N กำลัง Active]
   │
   ├─ ขอข้อมูล (Info Request)
   │   ├─ ถามหลาย Step พร้อมกันได้
   │   ├─ Step ยังคง Pending (ไม่ block การ Approve)
   │   └─ ตอบ / Forward ต่อ / ถามใหม่ได้หลังปิด
   │
   ├─ Approve ──────────────────────────► Next Step Pending
   │   │  (auto-cancel info requests ค้าง)    │
   │   │                        (ครบทุก Step) └──► status=Approved ✓
   │
   ├─ Reject ────────────────────────────► status=Rejected ✗
   │   (auto-cancel info requests ค้าง)
   │
   └─ Re-Escalate
       ├─ พบคน  → Step ใหม่ → Pending
       └─ หมดสาย → Step=Stuck → status=Blocked ⚠
                                      │
                               Admin Reassign
                                      │
                               status=Pending (ถ้าไม่มี Stuck เหลือ)
```

---

## Info Request — State Diagram

```
[Step N ขอข้อมูล Step M]
         │
    Request: Open
         │
    ┌────┴────────────────────────┐
    │                             │
  ตอบตรง                    Forward ต่อ Step K
    │                             │
Request: Closed            Request: Forwarded
Step N: ยังคง Pending      Child Request: Open (K ต้องตอบ)
                                   │
                              K ตอบ child
                                   │
                           Child: Closed
                           Parent: Open อีกครั้ง (M ต้องตอบ N)
                                   │
                              M ตอบ
                                   │
                           Request: Closed
                           Step N: ยังคง Pending

[ทุกกรณี: ถ้า Step N กด Approve ก่อน]
         │
  Open/Forwarded requests → Cancelled (auto)
```
