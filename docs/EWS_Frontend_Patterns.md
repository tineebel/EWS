# EWS Frontend Patterns — API Calls ทุกกรณี

> สรุป pattern การเรียก API สำหรับแต่ละ Role และแต่ละ Action ที่เกิดบน UI

---

## Role ในระบบ

| Role | คือใคร | หน้าหลักที่ใช้ |
|------|--------|---------------|
| **Requester** | พนักงานทั่วไปที่ส่งเอกสาร | Submit, ติดตามสถานะ |
| **Secretary** | เลขาที่สร้างเอกสารแทน Chief | Submit แทน, ดูสถานะ |
| **Chief** | หัวหน้าที่ต้องยืนยันเอกสารของเลขา | Pre-Approve |
| **Approver** | ผู้อนุมัติแต่ละ Step | Inbox, Approve, Reject, Info Request |
| **HR / Admin** | จัดการ Off-boarding, Reassign | Bulk Re-Escalate, Reassign |

---

## 1. Requester — ส่งเอกสาร

### 1.1 โหลดหน้า "สร้างเอกสาร"

```
ไม่ต้องเรียก API ใดก่อน
(ข้อมูล positionCode + employeeId ได้จาก session/auth)
```

### 1.2 Preview approval chain ก่อนส่ง (optional)

```
GET /organization/positions/{positionCode}/approval-chain
    ?docCode=101&amount=45000&isSpecialItem=false&isUrgent=false

→ แสดง: "เอกสารนี้จะผ่านการอนุมัติ 3 ขั้น: Section Manager → Dept Manager → CFO"
→ แสดง badge "Escalated" ถ้า wasEscalated=true (ตำแหน่งว่าง ระบบขึ้นไปแทน)
```

### 1.3 กด Submit

```
POST /workflows/submit
body: { docCode, submitterPositionCode, submitterEmployeeId,
        totalAmount, isSpecialItem, isUrgent, subject, remark,
        isCreatedBySecretary: false }

Response มีทุกอย่างในครั้งเดียว:
→ บันทึก instanceId ลง state
→ แสดง documentNo
→ แสดง approvalChain ทั้งหมด (ไม่ต้อง call ซ้ำ)
→ แสดง status = "Pending"
```

### 1.4 ดูสถานะเอกสารที่ส่งไปแล้ว

```
GET /workflows/{instanceId}

→ steps[].status บอกสถานะแต่ละ Step:
   Approved  = ✅ ผ่านแล้ว (แสดง actorName + actionAt + comment)
   Pending   = ⏳ รออยู่  (แสดง approverPositionName + occupantName)
   Stuck     = ⚠️ ค้าง  (แจ้ง "รอผู้ดูแลระบบกำหนดผู้อนุมัติ")
→ currentStepOrder บอกว่า step ไหน active อยู่ตอนนี้
```

### 1.5 ดูรายการเอกสารที่ส่งทั้งหมด

```
GET /workflows?positionCode={submitterPositionCode}&page=1&pageSize=20

→ กรองเพิ่ม: &status=Pending  (ยังรออยู่)
→ กรองเพิ่ม: &status=Approved (อนุมัติแล้ว)
→ กรองเพิ่ม: &status=Rejected (ถูกปฏิเสธ)
```

### 1.6 ดู Audit Trail (ประวัติเอกสาร)

```
GET /workflows/{instanceId}/audit

→ แสดง timeline ทุก Event: Submit → Approve → ... → Complete
→ ถ้ามี InfoRequest events แสดงว่ามีการขอข้อมูลระหว่างทาง
```

---

## 2. Secretary — สร้างเอกสารแทน Chief

### 2.1 กด Submit (isCreatedBySecretary = true)

```
POST /workflows/submit
body: { ..., isCreatedBySecretary: true }

Response:
→ status = "Draft"  ← ไม่ใช่ Pending!
→ requiresPreApproval = true
→ แสดง: "รอ [chiefName] ยืนยันเอกสาร"
→ approvalChain แสดงได้ แต่ยังไม่เริ่ม
```

### 2.2 ดูสถานะ (รอ Chief ยืนยัน)

```
GET /workflows/{instanceId}

→ status = "Draft" = แสดง banner "รอ Chief ยืนยัน"
→ status = "Pending" = Chief ยืนยันแล้ว Flow เริ่มแล้ว
→ status = "Rejected" = Chief ปฏิเสธ
```

---

## 3. Chief — ยืนยันเอกสารของเลขา

### 3.1 โหลดหน้า "รายการรอยืนยัน" (Pre-Approval Inbox)

```
GET /workflows?status=Draft&positionCode={chiefPositionCode}

→ แสดงเฉพาะเอกสาร status=Draft ที่ Chief คนนี้ต้องยืนยัน
```

### 3.2 ดูรายละเอียดก่อนยืนยัน

```
GET /workflows/{instanceId}

→ แสดง subject, amount, submitterPositionName
→ แสดง approvalChain ที่จะเดินหลังยืนยัน
```

### 3.3 กด ยืนยัน / ปฏิเสธ

```
POST /workflows/{instanceId}/pre-approve
body: { chiefPositionCode, chiefEmployeeId, isConfirmed: true/false, comment }

isConfirmed: true  → status = Pending  (Flow เริ่ม)
isConfirmed: false → status = Rejected (Flow ไม่เริ่ม)
```

---

## 4. Approver — อนุมัติเอกสาร

### 4.1 โหลดหน้า Inbox (เปิดแอปมา)

เรียก **2 calls พร้อมกัน** (parallel):

```
[1] GET /workflows?status=Pending&positionCode={myPositionCode}
    → รายการเอกสารรออนุมัติ

[2] GET /workflows/info-requests/pending?positionCode={myPositionCode}
    → รายการข้อซักถามที่ต้องตอบ

→ แสดง badge แยก:
   🔴 "3 เอกสารรออนุมัติ"
   🟡 "2 รายการรอตอบข้อซักถาม"
```

### 4.2 เปิดเอกสาร

```
GET /workflows/{instanceId}

→ ดู currentStepOrder เทียบกับ myPositionCode
   ถ้า steps[currentStep].approverPositionCode == myPositionCode
   → แสดงปุ่ม "อนุมัติ" และ "ปฏิเสธ"
→ ถ้ามี info requests → แสดง thread การถาม-ตอบ
```

### 4.3 ดู Info Request Thread ของเอกสาร (optional)

```
GET /workflows/{instanceId}/info-requests

→ แสดง thread ประวัติการถาม-ตอบทุก Step
→ depth=0 = ต้นสาย, depth=1 = forward ต่อ 1 ทอด
→ Closed = ตอบแล้ว, Open = รออยู่
```

### 4.4 กด อนุมัติ

```
POST /workflows/{instanceId}/approve
body: { actorPositionCode, actorEmployeeId, comment }

Response:
→ isCompleted=false → แสดง "อนุมัติแล้ว รอ [nextApproverName]"
→ isCompleted=true  → แสดง "เอกสารอนุมัติครบแล้ว ✅"
→ Info Requests ที่ค้างของ Step นี้ถูก auto-cancel (ไม่ต้องทำอะไรเพิ่ม)
```

### 4.5 กด ปฏิเสธ

```
POST /workflows/{instanceId}/reject
body: { actorPositionCode, actorEmployeeId, comment }  ← comment จำเป็น

Response:
→ status = "Rejected"
→ แสดง "เอกสารถูกปฏิเสธ" + เหตุผล
```

---

## 5. Approver — ขอข้อมูล (Info Request)

### 5.1 กด "ขอข้อมูลเพิ่มเติม" จากเอกสาร

```
[ก่อน call] ดู steps จาก GET /workflows/{instanceId}
            → เลือก toStepOrder ที่ต้องการถาม (ต้องน้อยกว่า step ตัวเอง)

POST /workflows/{instanceId}/steps/{myStepOrder}/request-info
body: { toStepOrder, actorPositionCode, actorEmployeeId, question }

→ บันทึก infoRequestId กลับมา
→ แสดง "ส่งคำถามไปยัง [toPositionName] ([toOccupantName]) แล้ว"
→ Step ตัวเองยังคง Pending — ยังกด Approve ได้ตลอด
```

### 5.2 ขอข้อมูลจากหลาย Step พร้อมกัน

```
POST /workflows/{instanceId}/steps/{myStepOrder}/request-info (toStepOrder=3)
POST /workflows/{instanceId}/steps/{myStepOrder}/request-info (toStepOrder=1)

→ ส่งทั้ง 2 calls ได้เลย (parallel)
→ ได้ infoRequestId คนละตัว
```

### 5.3 โหลด Inbox ข้อซักถาม (ฝั่งผู้ถูกถาม)

```
GET /workflows/info-requests/pending?positionCode={myPositionCode}

→ แสดงรายการที่ต้องตอบ
→ status=Open     = ✉️ รอตอบ
→ status=Forwarded = 🔄 ส่งต่อไปแล้ว รอ child ตอบก่อน
  (แสดง childAnswer ถ้า child ตอบกลับมาแล้ว — ต้องตอบ parent ต่อ)
```

### 5.4 กด "ตอบ"

```
POST /workflows/info-requests/{infoRequestId}/respond
body: { actorPositionCode, actorEmployeeId, answer: "..." }

→ Request: Closed
→ ถ้าเป็น child → parent กลับมา Open (แจ้ง parent ให้ตอบต่อ)
```

### 5.5 กด "ส่งต่อ" (ยังไม่รู้คำตอบ)

```
POST /workflows/info-requests/{infoRequestId}/respond
body: { actorPositionCode, actorEmployeeId,
        forwardToStepOrder: 1, forwardQuestion: "..." }

→ Request: Forwarded
→ สร้าง child request ใหม่
→ แสดง "ส่งต่อไปยัง [childToPositionName] แล้ว รอคำตอบ"
```

---

## 6. HR / Admin — Off-boarding พนักงานลาออก

### ลำดับที่ถูกต้อง

```
1. HR deactivate PositionAssignment ใน HR System
   (ไม่ใช่ EWS API)

2. POST /organization/employees/{employeeCode}/re-escalate-pending
   body: { requestedByPositionCode: "HOADMIN01" }

   Response:
   → totalStepsReEscalated: N  = re-route สำเร็จ N steps
   → totalStepsStuck: M        = หา hierarchy ไม่เจอ M steps
   → details[] = รายการทุก step ที่เปลี่ยนไป

3. ถ้า totalStepsStuck > 0:
   → แสดงรายการ instanceId ที่มี Stuck step
   → ให้ Admin กำหนดผู้อนุมัติใหม่ (ดูข้อ 7)
```

---

## 7. Admin — จัดการเอกสารที่ Stuck

### 7.1 โหลดหน้า "เอกสารที่รอดำเนินการ"

```
GET /workflows?status=Blocked&page=1&pageSize=20

→ แสดงรายการเอกสารที่ hierarchy หมด
```

### 7.2 ดูรายละเอียดว่า Step ไหน Stuck

```
GET /workflows/{instanceId}

→ steps[].status = "Stuck" = ⚠️ ต้อง reassign
→ แสดง approverPositionCode ที่ Stuck อยู่
```

### 7.3 กำหนดผู้อนุมัติใหม่

```
POST /workflows/{instanceId}/steps/{stuckStepOrder}/reassign
body: { targetPositionCode, requestedByPositionCode, reason }

→ Step: Pending อีกครั้ง
→ ถ้าไม่มี Stuck เหลือ → Instance: Pending อัตโนมัติ
→ แสดง "กำหนด [targetPositionName] เป็นผู้อนุมัติแล้ว"
```

---

## 8. หน้า Dashboard / Notification

### โหลด Notification Badge (เปิดแอปมา)

```
เรียก 2 calls พร้อมกัน (parallel):

[1] GET /workflows?status=Pending&positionCode={myPositionCode}&pageSize=1
    → totalRows = จำนวนเอกสารรออนุมัติ

[2] GET /workflows/info-requests/pending?positionCode={myPositionCode}
    → length = จำนวนข้อซักถามที่ต้องตอบ

→ แสดง badge:
   🔴 "5"  (เอกสารรออนุมัติ)
   🟡 "2"  (รอตอบข้อซักถาม)
```

### Dashboard ของ HR (ภาพรวม)

```
GET /workflows?status=Blocked    → เอกสารค้าง รอ Admin
GET /workflows?status=Pending    → เอกสารกำลังเดิน
GET /workflows?status=Approved   → อนุมัติแล้ว (เดือนนี้)
```

---

## สรุป: แต่ละหน้า UI เรียก API อะไรบ้าง

| หน้า UI | API calls |
|---------|-----------|
| หน้าสร้างเอกสาร (ก่อน Submit) | `GET /approval-chain` (preview, optional) |
| กด Submit | `POST /submit` → ได้ chain กลับมาทันที |
| หน้าติดตามเอกสาร | `GET /workflows/{id}` |
| Inbox ผู้อนุมัติ | `GET /workflows?status=Pending&positionCode=X` |
| Inbox ข้อซักถาม | `GET /info-requests/pending?positionCode=X` |
| Notification Badge | `GET /workflows` + `GET /info-requests/pending` (parallel) |
| กด Approve | `POST /{id}/approve` |
| กด Reject | `POST /{id}/reject` |
| กด ขอข้อมูล | `POST /{id}/steps/{order}/request-info` |
| กด ตอบ / ส่งต่อ | `POST /info-requests/{id}/respond` |
| Chief Inbox (Draft) | `GET /workflows?status=Draft&positionCode=X` |
| Chief กด ยืนยัน/ปฏิเสธ | `POST /{id}/pre-approve` |
| Admin Inbox (Blocked) | `GET /workflows?status=Blocked` |
| Admin Reassign | `POST /{id}/steps/{order}/reassign` |
| HR Off-boarding | `POST /employees/{code}/re-escalate-pending` |
| ดูประวัติเอกสาร | `GET /{id}/audit` |
| ดู thread ถาม-ตอบ | `GET /{id}/info-requests` |

---

## ข้อมูลที่ Frontend ต้องมีจาก Auth/Session

```
myPositionCode   — PositionCode ของ user ที่ login อยู่
myEmployeeId     — EmployeeId (GUID)
myRole           — Requester / Approver / Secretary / Chief / HR / Admin
```

> ข้อมูลเหล่านี้ไม่ต้อง call API เพราะได้จาก JWT token หรือ session หลัง login
> ใช้ `GET /organization/employees/{employeeCode}/current-position` ถ้าต้องการ verify หรือแสดงข้อมูลเพิ่มเติม
