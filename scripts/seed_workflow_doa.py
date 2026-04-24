# -*- coding: utf-8 -*-
"""
EWS Workflow Config Re-Seeder (DOA-based)
ลบ WorkflowTemplates + Steps เดิมทั้งหมด แล้ว insert ใหม่ตาม DOA จริง

DOA HO Approval Levels:
  SectionManager  (2) = Grade B1/B2 — "ผู้จัดการแผนก"
  DeptManager     (3) = Grade A3    — "Department"
  DivisionDirector(4) = Grade A2    — "Director"
  CLevel          (5) = Grade A1    — "Chief" (COO/CMO/etc.)
  SpecificPosition(7) = HOTOP04     — "CFO" (เฉพาะ Advance)
  Ceo             (6) = Grade A0    — "CEO"

DOA Branch Approval Levels:
  SectionManager  (2) = Grade B1    — "ผู้จัดการสาขา / Theater Manager"
  AreaManager     (8) = Grade B0    — "Area" ← HOHOP09 Area Manager (CB)
  DeptManager     (3) = Grade A3    — "Department"
  DivisionDirector(4) = Grade A2    — "Director"
  CLevel          (5) = Grade A1    — "Chief"
  SpecificPosition(7) = HOTOP04     — "CFO" (เฉพาะ Advance)
  Ceo             (6) = Grade A0    — "CEO"
"""
import sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')

import pyodbc
from datetime import datetime

CONN_STR = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=192.168.0.155;"
    "DATABASE=EWS;"
    "UID=sa;"
    "PWD=SFcinem@;"
    "TrustServerCertificate=yes;"
)
NOW = datetime.now()
BY  = "DOA_SEEDER"
CFO_CODE = "HOTOP04"

# ApproverType enum values
SM   = 2   # SectionManager
AM   = 8   # AreaManager (Branch only)
DM   = 3   # DeptManager
DD   = 4   # DivisionDirector
CL   = 5   # CLevel
CEO  = 6   # Ceo
SP   = 7   # SpecificPosition (CFO)

def s(name, typ, code=None):
    return {"name": name, "type": typ, "code": code}

# ─────────────────────────────────────────────────────────────────
# WORKFLOW CONFIG:
# Each entry = one WorkflowTemplate with its Steps
# Keys: doc_code, flow_code, desc, scope (0=All,1=Branch,2=HO),
#       special (HasSpecialItem), urgent (IsUrgent),
#       cond (Condition1 string), steps (list of step dicts)
# ─────────────────────────────────────────────────────────────────

TEMPLATES = []
flow_code_counters = {}  # doc_code → next flow_code

def fc(doc_code):
    flow_code_counters[doc_code] = flow_code_counters.get(doc_code, 0) + 1
    return flow_code_counters[doc_code]


# ═══════════════════════════════════════════════════════════════════
# MEMO
# ═══════════════════════════════════════════════════════════════════

# 1001 — Memo ทั่วไป (All scope)
TEMPLATES.append(dict(
    doc_code=1001, flow_code=fc(1001), scope=0, special=False, urgent=False, cond=None,
    desc="Memo ทั่วไป",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM)]
))

# 1002 — Memo ผ่าน DOA (All scope)
TEMPLATES.append(dict(
    doc_code=1002, flow_code=fc(1002), scope=0, special=False, urgent=False, cond=None,
    desc="Memo ผ่าน DOA",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL),
           s("CEO",                CEO)]
))


# ═══════════════════════════════════════════════════════════════════
# PCV-HO (2002) — ใบสำคัญจ่าย สำนักงานใหญ่
# DOA: ปกติ ≤1,000 → ผู้จัดการแผนก (SectionManager)
# ═══════════════════════════════════════════════════════════════════

TEMPLATES.append(dict(
    doc_code=2002, flow_code=fc(2002), scope=2, special=False, urgent=False, cond="<= 1000",
    desc="PCV-HO ปกติ ≤1,000",
    steps=[s("Section Manager", SM)]
))


# ═══════════════════════════════════════════════════════════════════
# PCR-HO (2004) — ใบขอรับเงินสดย่อย สำนักงานใหญ่
# DOA: ปกติ 1-50,000 → Department (DeptManager)
# ═══════════════════════════════════════════════════════════════════

TEMPLATES.append(dict(
    doc_code=2004, flow_code=fc(2004), scope=2, special=False, urgent=False, cond="<= 50000",
    desc="PCR-HO ปกติ ≤50,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM)]
))


# ═══════════════════════════════════════════════════════════════════
# EXP-HO (2010) — ค่าใช้จ่าย/Reimbursement สำนักงานใหญ่
# ═══════════════════════════════════════════════════════════════════

# ปกติ
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=False, urgent=False, cond="<= 1000",
    desc="EXP-HO ปกติ ≤1,000",
    steps=[s("Section Manager", SM)]
))
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=False, urgent=False, cond="> 1000",
    desc="EXP-HO ปกติ 1,001-50,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM)]
))
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=False, urgent=False, cond="> 50000",
    desc="EXP-HO ปกติ 50,001-300,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD)]
))
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=False, urgent=False, cond="> 300000",
    desc="EXP-HO ปกติ 300,001-1,000,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=False, urgent=False, cond="> 1000000",
    desc="EXP-HO ปกติ >1,000,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL),
           s("CEO",                CEO)]
))

# พิเศษ (Special Item)
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=True, urgent=False, cond="<= 1000",
    desc="EXP-HO พิเศษ ≤1,000",
    steps=[s("Section Manager", SM),
           s("C-Level",         CL)]
))
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=True, urgent=False, cond="> 1000",
    desc="EXP-HO พิเศษ 1,001-20,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=True, urgent=False, cond="> 20000",
    desc="EXP-HO พิเศษ 20,001-50,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2010, flow_code=fc(2010), scope=2, special=True, urgent=False, cond="> 50000",
    desc="EXP-HO พิเศษ >50,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL),
           s("CEO",                CEO)]
))


# ═══════════════════════════════════════════════════════════════════
# ADV-HO (2006) — เบิกล่วงหน้า สำนักงานใหญ่
# ═══════════════════════════════════════════════════════════════════

# ปกติ
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=False, urgent=False, cond="<= 1000",
    desc="ADV-HO ปกติ ≤1,000",
    steps=[s("Section Manager", SM)]
))
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=False, urgent=False, cond="> 1000",
    desc="ADV-HO ปกติ 1,001-50,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM)]
))
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=False, urgent=False, cond="> 50000",
    desc="ADV-HO ปกติ 50,001-300,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD)]
))
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=False, urgent=False, cond="> 300000",
    desc="ADV-HO ปกติ 300,001-1,000,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=False, urgent=False, cond="> 1000000",
    desc="ADV-HO ปกติ >1,000,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL),
           s("CFO",                SP, CFO_CODE),
           s("CEO",                CEO)]
))

# พิเศษ
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=True, urgent=False, cond="<= 1000",
    desc="ADV-HO พิเศษ ≤1,000",
    steps=[s("Section Manager", SM),
           s("C-Level",         CL)]
))
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=True, urgent=False, cond="> 1000",
    desc="ADV-HO พิเศษ 1,001-32,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=True, urgent=False, cond="> 32000",
    desc="ADV-HO พิเศษ 32,001-300,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2006, flow_code=fc(2006), scope=2, special=True, urgent=False, cond="> 300000",
    desc="ADV-HO พิเศษ >300,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL),
           s("CFO",                SP, CFO_CODE),
           s("CEO",                CEO)]
))


# ═══════════════════════════════════════════════════════════════════
# ADC-HO (2008) — เบิกเพิ่ม/Clear Advance สำนักงานใหญ่
# ═══════════════════════════════════════════════════════════════════

# ปกติ
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=False, urgent=False, cond="<= 3000",
    desc="ADC-HO ปกติ ≤3,000",
    steps=[s("Section Manager", SM)]
))
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=False, urgent=False, cond="> 3000",
    desc="ADC-HO ปกติ 3,001-50,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM)]
))
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=False, urgent=False, cond="> 50000",
    desc="ADC-HO ปกติ 50,001-300,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD)]
))
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=False, urgent=False, cond="> 300000",
    desc="ADC-HO ปกติ 300,001-1,000,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=False, urgent=False, cond="> 1000000",
    desc="ADC-HO ปกติ >1,000,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL),
           s("CEO",                CEO)]
))

# พิเศษ
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=True, urgent=False, cond="<= 5000",
    desc="ADC-HO พิเศษ ≤5,000",
    steps=[s("Section Manager", SM),
           s("C-Level",         CL)]
))
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=True, urgent=False, cond="> 5000",
    desc="ADC-HO พิเศษ 5,001-20,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=True, urgent=False, cond="> 20000",
    desc="ADC-HO พิเศษ 20,001-50,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL)]
))
TEMPLATES.append(dict(
    doc_code=2008, flow_code=fc(2008), scope=2, special=True, urgent=False, cond="> 50000",
    desc="ADC-HO พิเศษ >50,000",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL),
           s("CEO",                CEO)]
))


# ═══════════════════════════════════════════════════════════════════
# PCV-BR (2001) — ใบสำคัญจ่าย สาขา
# DOA: ปกติ ≤1,000 → Theater Manager
#       พิเศษ ≤1,000 → Theater+Chief, >1,000 → Theater+Area+Chief
#       ฉุกเฉิน ≤1,000 → Theater+Chief, >1,000 → Theater+Area+Chief
# ═══════════════════════════════════════════════════════════════════

TEMPLATES.append(dict(
    doc_code=2001, flow_code=fc(2001), scope=1, special=False, urgent=False, cond="<= 1000",
    desc="PCV-BR ปกติ ≤1,000",
    steps=[s("Theater Manager", SM)]
))
TEMPLATES.append(dict(
    doc_code=2001, flow_code=fc(2001), scope=1, special=True, urgent=False, cond="<= 1000",
    desc="PCV-BR พิเศษ ≤1,000",
    steps=[s("Theater Manager", SM),
           s("C-Level",         CL)]
))
TEMPLATES.append(dict(
    doc_code=2001, flow_code=fc(2001), scope=1, special=True, urgent=False, cond="> 1000",
    desc="PCV-BR พิเศษ >1,000",
    steps=[s("Theater Manager", SM),
           s("Area Manager",    AM),
           s("C-Level",         CL)]
))
TEMPLATES.append(dict(
    doc_code=2001, flow_code=fc(2001), scope=1, special=False, urgent=True, cond="<= 1000",
    desc="PCV-BR ฉุกเฉิน ≤1,000",
    steps=[s("Theater Manager", SM),
           s("C-Level",         CL)]
))
TEMPLATES.append(dict(
    doc_code=2001, flow_code=fc(2001), scope=1, special=False, urgent=True, cond="> 1000",
    desc="PCV-BR ฉุกเฉิน >1,000",
    steps=[s("Theater Manager", SM),
           s("Area Manager",    AM),
           s("C-Level",         CL)]
))


# ═══════════════════════════════════════════════════════════════════
# PCR-BR (2003) — ใบขอรับเงินสดย่อย สาขา
# DOA: ≤5,000 → Theater+Area, 5,001-20,000 → Theater+Area+Director
# ═══════════════════════════════════════════════════════════════════

TEMPLATES.append(dict(
    doc_code=2003, flow_code=fc(2003), scope=1, special=False, urgent=False, cond="<= 5000",
    desc="PCR-BR ปกติ ≤5,000",
    steps=[s("Theater Manager", SM),
           s("Area Manager",    AM)]
))
TEMPLATES.append(dict(
    doc_code=2003, flow_code=fc(2003), scope=1, special=False, urgent=False, cond="> 5000",
    desc="PCR-BR ปกติ 5,001-20,000",
    steps=[s("Theater Manager", SM),
           s("Area Manager",    AM),
           s("Division Director", DD)]
))


# ═══════════════════════════════════════════════════════════════════
# EXP-BR (2009) — ค่าใช้จ่าย/Reimbursement สาขา
# ═══════════════════════════════════════════════════════════════════

# ปกติ
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=False, urgent=False, cond="<= 1000",
    desc="EXP-BR ปกติ ≤1,000",
    steps=[s("Area Manager", AM)]
))
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=False, urgent=False, cond="> 1000",
    desc="EXP-BR ปกติ 1,001-50,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM)]
))
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=False, urgent=False, cond="> 50000",
    desc="EXP-BR ปกติ 50,001-300,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD)]
))
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=False, urgent=False, cond="> 300000",
    desc="EXP-BR ปกติ 300,001-1,000,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL)]
))
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=False, urgent=False, cond="> 1000000",
    desc="EXP-BR ปกติ >1,000,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL),
           s("CEO",               CEO)]
))

# พิเศษ
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=True, urgent=False, cond="<= 1000",
    desc="EXP-BR พิเศษ ≤1,000",
    steps=[s("Area Manager", AM),
           s("C-Level",      CL)]
))
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=True, urgent=False, cond="> 1000",
    desc="EXP-BR พิเศษ 1,001-20,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("C-Level",           CL)]
))
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=True, urgent=False, cond="> 20000",
    desc="EXP-BR พิเศษ 20,001-50,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL)]
))
TEMPLATES.append(dict(
    doc_code=2009, flow_code=fc(2009), scope=1, special=True, urgent=False, cond="> 50000",
    desc="EXP-BR พิเศษ >50,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL),
           s("CEO",               CEO)]
))


# ═══════════════════════════════════════════════════════════════════
# ADV-BR (2005) — เบิกล่วงหน้า สาขา
# ═══════════════════════════════════════════════════════════════════

# ปกติ
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=False, urgent=False, cond="<= 5000",
    desc="ADV-BR ปกติ ≤5,000",
    steps=[s("Area Manager", AM)]
))
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=False, urgent=False, cond="> 5000",
    desc="ADV-BR ปกติ 5,001-50,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM)]
))
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=False, urgent=False, cond="> 50000",
    desc="ADV-BR ปกติ 50,001-300,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD)]
))
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=False, urgent=False, cond="> 300000",
    desc="ADV-BR ปกติ 300,001-1,000,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL),
           s("CFO",               SP, CFO_CODE)]
))
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=False, urgent=False, cond="> 1000000",
    desc="ADV-BR ปกติ >1,000,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL),
           s("CFO",               SP, CFO_CODE),
           s("CEO",               CEO)]
))

# พิเศษ
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=True, urgent=False, cond="<= 5000",
    desc="ADV-BR พิเศษ ≤5,000",
    steps=[s("Area Manager", AM),
           s("C-Level",      CL)]
))
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=True, urgent=False, cond="> 5000",
    desc="ADV-BR พิเศษ 5,001-20,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("C-Level",           CL)]
))
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=True, urgent=False, cond="> 20000",
    desc="ADV-BR พิเศษ 20,001-300,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL),
           s("CEO",               CEO)]
))
TEMPLATES.append(dict(
    doc_code=2005, flow_code=fc(2005), scope=1, special=True, urgent=False, cond="> 300000",
    desc="ADV-BR พิเศษ >300,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL),
           s("CFO",               SP, CFO_CODE),
           s("CEO",               CEO)]
))


# ═══════════════════════════════════════════════════════════════════
# ADC-BR (2007) — เบิกเพิ่ม/Clear Advance สาขา
# ═══════════════════════════════════════════════════════════════════

# ปกติ
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=False, urgent=False, cond="<= 5000",
    desc="ADC-BR ปกติ ≤5,000",
    steps=[s("Area Manager", AM)]
))
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=False, urgent=False, cond="> 5000",
    desc="ADC-BR ปกติ 5,001-50,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM)]
))
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=False, urgent=False, cond="> 50000",
    desc="ADC-BR ปกติ 50,001-300,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD)]
))
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=False, urgent=False, cond="> 300000",
    desc="ADC-BR ปกติ 300,001-1,000,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL)]
))
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=False, urgent=False, cond="> 1000000",
    desc="ADC-BR ปกติ >1,000,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL),
           s("CEO",               CEO)]
))

# พิเศษ
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=True, urgent=False, cond="<= 5000",
    desc="ADC-BR พิเศษ ≤5,000",
    steps=[s("Area Manager", AM),
           s("C-Level",      CL)]
))
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=True, urgent=False, cond="> 5000",
    desc="ADC-BR พิเศษ 5,001-20,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("C-Level",           CL)]
))
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=True, urgent=False, cond="> 20000",
    desc="ADC-BR พิเศษ 20,001-50,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL)]
))
TEMPLATES.append(dict(
    doc_code=2007, flow_code=fc(2007), scope=1, special=True, urgent=False, cond="> 50000",
    desc="ADC-BR พิเศษ >50,000",
    steps=[s("Area Manager",      AM),
           s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",           CL),
           s("CEO",               CEO)]
))


# ═══════════════════════════════════════════════════════════════════
# Write Off (2101)
# ═══════════════════════════════════════════════════════════════════

TEMPLATES.append(dict(
    doc_code=2101, flow_code=fc(2101), scope=0, special=False, urgent=False, cond=None,
    desc="Write Off ทรัพย์สิน",
    steps=[s("Department Manager", DM),
           s("Division Director",  DD),
           s("C-Level",            CL),
           s("CEO",                CEO)]
))


# ═══════════════════════════════════════════════════════════════════
# IT Requests (4001-4006)
# ═══════════════════════════════════════════════════════════════════

IT_DOC_CODES = [4001, 4002, 4003, 4004, 4006]
for dc in IT_DOC_CODES:
    TEMPLATES.append(dict(
        doc_code=dc, flow_code=fc(dc), scope=0, special=False, urgent=False, cond=None,
        desc=f"IT Request {dc}",
        steps=[s("Section Manager",    SM),
               s("Department Manager", DM)]
    ))

# 4005 — กรณีพิเศษ ต้องผ่าน Director ด้วย
TEMPLATES.append(dict(
    doc_code=4005, flow_code=fc(4005), scope=0, special=False, urgent=False, cond=None,
    desc="IT Request กรณีพิเศษ",
    steps=[s("Section Manager",    SM),
           s("Department Manager", DM),
           s("Division Director",  DD)]
))


# ═══════════════════════════════════════════════════════════════════
# SIS (5001-5008)
# ═══════════════════════════════════════════════════════════════════

SIS_TIERS = [
    ("<= 5000",   [s("Section Manager", SM)]),
    ("> 5000",    [s("Section Manager", SM), s("Department Manager", DM)]),
    ("> 50000",   [s("Section Manager", SM), s("Department Manager", DM), s("Division Director", DD)]),
    ("> 300000",  [s("Section Manager", SM), s("Department Manager", DM), s("Division Director", DD), s("C-Level", CL)]),
    ("> 1000000", [s("Section Manager", SM), s("Department Manager", DM), s("Division Director", DD), s("C-Level", CL), s("CEO", CEO)]),
]
for dc in range(5001, 5009):
    for cond, steps in SIS_TIERS:
        TEMPLATES.append(dict(
            doc_code=dc, flow_code=fc(dc), scope=0, special=False, urgent=False,
            cond=cond, desc=f"SIS {dc} ({cond})", steps=steps
        ))


# ═══════════════════════════════════════════════════════════════════
# Contract / Customer (6001-6003)
# ═══════════════════════════════════════════════════════════════════

for dc in [6001, 6002, 6003]:
    TEMPLATES.append(dict(
        doc_code=dc, flow_code=fc(dc), scope=0, special=False, urgent=False, cond=None,
        desc=f"Contract/Customer {dc}",
        steps=[s("Department Manager", DM),
               s("Division Director",  DD),
               s("C-Level",            CL),
               s("CEO",                CEO)]
    ))


# ─────────────────────────────────────────────────────────────────
# DocumentType metadata
# ─────────────────────────────────────────────────────────────────
DOC_META = {
    1001: ("Memo ทั่วไป",                         "General Memo",                  "Memo"),
    1002: ("Memo ผ่าน DOA",                        "Memo via DOA",                  "Memo"),
    2001: ("ใบสำคัญจ่าย-สาขา (PCV-BR)",           "Petty Cash Voucher Branch",     "Finance"),
    2002: ("ใบสำคัญจ่าย-สำนักงาน (PCV-HO)",       "Petty Cash Voucher HO",         "Finance"),
    2003: ("ใบขอเบิก-สาขา (PCR-BR)",              "Petty Cash Request Branch",     "Finance"),
    2004: ("ใบขอเบิก-สำนักงาน (PCR-HO)",          "Petty Cash Request HO",         "Finance"),
    2005: ("เบิกล่วงหน้า-สาขา (ADV-BR)",           "Advance Branch",                "Finance"),
    2006: ("เบิกล่วงหน้า-สำนักงาน (ADV-HO)",       "Advance HO",                    "Finance"),
    2007: ("เบิกเพิ่ม-สาขา (ADC-BR)",              "Additional Advance Branch",     "Finance"),
    2008: ("เบิกเพิ่ม-สำนักงาน (ADC-HO)",          "Additional Advance HO",         "Finance"),
    2009: ("ค่าใช้จ่าย-สาขา (EXP-BR)",             "Expense Branch",                "Finance"),
    2010: ("ค่าใช้จ่าย-สำนักงาน (EXP-HO)",         "Expense HO",                    "Finance"),
    2101: ("Write Off ทรัพย์สิน",                  "Asset Write Off",               "Finance"),
    4001: ("ขอ Email พนักงานใหม่",                  "New Employee Email Request",    "IT"),
    4002: ("ขอสิทธิ์ VPN",                          "VPN Access Request",            "IT"),
    4003: ("ขอสิทธิ์ Application",                  "Application Access Request",   "IT"),
    4004: ("เปลี่ยนแปลงสิทธิ์ระบบ",                "System Permission Change",      "IT"),
    4005: ("ขอสิทธิ์ Application กรณีพิเศษ",       "Special Application Access",   "IT"),
    4006: ("ยกเลิกสิทธิ์",                          "Access Revocation",             "IT"),
    5001: ("SIS: Accrued",                         "SIS Accrued",                   "SIS"),
    5002: ("SIS: Forecast & PR",                   "SIS Forecast & PR",             "SIS"),
    5003: ("SIS: PO (สาขา)",                       "SIS Purchase Order Branch",     "SIS"),
    5004: ("SIS: PO (สำนักงาน)",                   "SIS Purchase Order HO",         "SIS"),
    5005: ("SIS: PO D365",                         "SIS Purchase Order D365",       "SIS"),
    5006: ("SIS: Transfer Order",                  "SIS Transfer Order",            "SIS"),
    5007: ("SIS: ยกเลิก Receipt PO",               "SIS Cancel Receipt PO",         "SIS"),
    5008: ("SIS: รับเงินคืนหลังจ่าย",              "SIS Refund After Payment",      "SIS"),
    6001: ("สัญญาต่างๆ",                            "Contracts",                     "Contract"),
    6002: ("เงินมัดจำ",                              "Deposit",                       "Contract"),
    6003: ("ลูกค้า",                                 "Customer",                      "Contract"),
}


# ─────────────────────────────────────────────────────────────────
# MAIN
# ─────────────────────────────────────────────────────────────────
def main():
    print("=" * 60)
    print("EWS Workflow DOA Re-Seeder")
    print(f"Templates to insert: {len(TEMPLATES)}")
    print("=" * 60)

    conn = pyodbc.connect(CONN_STR, autocommit=False)
    cur  = conn.cursor()

    try:
        # ── 1. ลบ WorkflowSteps + WorkflowTemplates เดิมทั้งหมด ──
        print("\n── Clearing old workflow config ──")
        cur.execute("DELETE FROM wf.WorkflowSteps")
        cur.execute("DELETE FROM wf.WorkflowTemplates")
        cur.execute("DELETE FROM wf.DocumentTypes")
        conn.commit()
        print("  Cleared WorkflowSteps, WorkflowTemplates, DocumentTypes")

        # ── 2. Insert DocumentTypes ───────────────────────────────
        doc_type_ids = {}
        doc_codes = sorted({t["doc_code"] for t in TEMPLATES})
        for dc in doc_codes:
            meta = DOC_META.get(dc, (f"Document {dc}", f"Document {dc}", "Other"))
            cur.execute("""
                INSERT INTO wf.DocumentTypes (DocCode, DocName, DocNameEn, Category, IsActive, CreatedAt, CreatedBy)
                VALUES (?, ?, ?, ?, 1, ?, ?)
            """, dc, meta[0], meta[1], meta[2], NOW, BY)
            cur.execute("SELECT @@IDENTITY")
            doc_type_ids[dc] = int(cur.fetchone()[0])

        conn.commit()
        print(f"  DocumentTypes: {len(doc_type_ids)}")

        # ── 3. Insert WorkflowTemplates + Steps ──────────────────
        tmpl_count = 0
        step_count = 0

        for t in TEMPLATES:
            dc   = t["doc_code"]
            dtid = doc_type_ids[dc]
            cur.execute("""
                INSERT INTO wf.WorkflowTemplates
                (DocumentTypeId, FlowCode, FlowDesc, WfScopeType, HasSpecialItem, IsUrgent,
                 Condition1, IsActive, CreatedAt, CreatedBy)
                VALUES (?, ?, ?, ?, ?, ?, ?, 1, ?, ?)
            """, dtid, t["flow_code"], t["desc"], t["scope"],
                 1 if t["special"] else 0,
                 1 if t["urgent"]  else 0,
                 t.get("cond"),
                 NOW, BY)
            cur.execute("SELECT @@IDENTITY")
            tmpl_id = int(cur.fetchone()[0])
            tmpl_count += 1

            for i, step in enumerate(t["steps"], 1):
                cur.execute("""
                    INSERT INTO wf.WorkflowSteps
                    (TemplateId, StepOrder, StepName, ApproverType, SpecificPositionCode,
                     EscalationDays, IsRequired, IsActive, CreatedAt, CreatedBy)
                    VALUES (?, ?, ?, ?, ?, 3, 1, 1, ?, ?)
                """, tmpl_id, i, step["name"], step["type"], step.get("code"), NOW, BY)
                step_count += 1

        conn.commit()
        print(f"  WorkflowTemplates: {tmpl_count}")
        print(f"  WorkflowSteps:     {step_count}")
        print("\n✓ Done!")

    except Exception as e:
        conn.rollback()
        print(f"\n✗ Error: {e}")
        raise
    finally:
        conn.close()


if __name__ == "__main__":
    main()
