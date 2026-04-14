# In-Clinic Module — Lab Flow & Report Flow Verification

**Date:** 2025-02-28  
**Scope:** D. Lab Flow (In-Clinic), E. Report Flow, STATUS TRANSITIONS

---

## Required flows (from spec)

### D. Lab Flow — In-Clinic
- **Manual status updates:** Ordered → Sample Collected → Processing → Report Uploaded

### E. Report Flow
- Vet reviews report & adds advice
- Pet parent sees report + next steps

### STATUS TRANSITIONS
- **Consult:** Booked → In Progress → Completed → Closed  
- **Diagnostics:** Recommended → Sample Collected → Processing → Report Available → Reviewed

---

## 1. What is implemented

### Consult status (complete)
- **Frontend:** `ConsultStatus` in `ConsultationViewModel.cs`: Booked, In Progress, Completed, Closed.
- **Backend:** Consultation entity has `Status`; API loads/saves via Consultations API.
- **Pet Parent:** Consultation Details shows consult status.

### Diagnostics — In-Clinic (partial)
- **Backend:** 
  - `DiagnosticOrder` entity: `Status` (default `"Ordered"`), `OrderedAt`, `SampleCollectedAt`, `ReportUploadedAt`.
  - API: `POST /api/diagnostics/orders` (create), `GET /api/diagnostics/orders/consultation/{id}` (get). **No API to update status.**
  - Entity comment only lists: "Ordered, SampleCollected, ReportUploaded" (missing Processing, Report Available, Reviewed).
- **Frontend:** 
  - `DiagnosticStatus`: Recommended, Sample Collected, Processing, Report Available, Reviewed. (No `Ordered` constant; API returns `"Ordered"` and it is displayed.)
  - Pet Parent Consultations/Details: "Diagnostics — In-Clinic" section shows order, status badge, tests, price, lab, OrderedAt, SampleCollectedAt, ReportUploadedAt.
  - Vet portal: Can create diagnostic order ("Order diagnostics"); no UI to change status (Sample Collected, Processing, Report Uploaded).

### Report flow — E (UI only, no backend)
- **Frontend:** 
  - `LabReportViewModel`: ReportId, ReportUrl, ReportFileName, VetAdvice, NextSteps, ReviewedAt.
  - Pet Parent Consultations/Details: "Report & advice" section shows report link, Vet advice, Next steps when `Model.Report != null`.
- **Backend:** 
  - **No** DiagnosticReport / LabReport entity. **No** API to upload report file or save vet advice/next steps.
- **Data:** In real API flow, `details.Report` is **never set**; only the demo (`ConsultationStore.GetDetails`) populates Report. So pet parent never sees report/advice from live data.

### Status alignment
- **Diagnostics:** Spec says manual steps: Ordered, Sample Collected, Processing, Report Uploaded; and transitions: Recommended → Sample Collected → Processing → Report Available → Reviewed. So we need **Ordered** (after order is created), then **Report Uploaded** (manual) and **Report Available** (report ready), then **Reviewed** (vet reviewed + advice). Frontend has Recommended, Sample Collected, Processing, Report Available, Reviewed but no `Ordered` in `DiagnosticStatus`. Backend only sets `"Ordered"` on create and has no other status values or update endpoint.

---

## 2. Gaps (to implement for full spec)

| # | Item | Current state | Needed |
|---|------|----------------|--------|
| 1 | **Diagnostic order status update (D)** | No backend API or vet UI to change status | Backend: PATCH/PUT to update status (and timestamps). Vet UI: dropdown or buttons to set Sample Collected, Processing, Report Uploaded (and optionally Report Available / Reviewed). |
| 2 | **Diagnostics status values** | Backend: only Ordered (+ comment SampleCollected, ReportUploaded). Frontend: no "Ordered" in DiagnosticStatus. | Add "Ordered" to frontend DiagnosticStatus. Backend: support full set (Ordered, Sample Collected, Processing, Report Uploaded, Report Available, Reviewed) and set timestamps when status changes. |
| 3 | **Report flow backend (E)** | No entity or API for report + vet advice | New entity (e.g. DiagnosticReport) or fields: DiagnosticOrderId/ConsultationId, ReportFileUrl, VetAdvice, NextSteps, ReviewedAt. API: upload report, save advice/next steps; GET by consultation or order so pet parent can load report. |
| 4 | **Pet Parent sees report from API** | Report only from demo store | ConsultationsController.Details: after loading consultation + diagnostic order, call new API to load report for that consultation/order; map to `details.Report`. |
| 5 | **Vet: upload report & add advice (E)** | No vet UI or API | Vet portal: page/modal to upload report file and enter Vet Advice + Next Steps; call new backend API. Optionally set diagnostic order status to Report Uploaded / Report Available / Reviewed. |

---

## 3. Summary

- **Consult transitions:** Implemented end-to-end.
- **Diagnostics — In-Clinic (D):** Create order and display in Pet Parent view are in place. **Missing:** manual status updates (backend API + vet UI) and full status set (Ordered + Report Uploaded / Report Available / Reviewed).
- **Report flow (E):** Pet Parent UI for report + advice exists. **Missing:** backend storage and API for report file and vet advice/next steps, wiring Pet Parent Details to that API, and vet UI to upload report and add advice.

**Next step:** Confirm this verification; then implementation can proceed for gaps 1–5 (or a subset you specify).

---

## 4. Implementation completed (2025-02-28)

- **Gap 1 & 2:** Backend PATCH `api/diagnostics/orders/{orderId}/status`; service supports statuses: Ordered, Sample Collected, Processing, Report Uploaded, Report Available, Reviewed; timestamps set on transition. Frontend `DiagnosticStatus` includes `Ordered` and `ReportUploaded`.
- **Gap 3:** `DiagnosticReport` entity, repository, service; API: GET report by consultation or order, POST report (multipart: file + vetAdvice + nextSteps), GET report-file/{id} for download.
- **Gap 4:** Pet Parent ConsultationsController.Details loads report from `diagnostics/reports/consultation/{id}` and maps to `details.Report`; report file URL points to API.
- **Gap 5:** Vet portal Consult view: when diagnostic order exists, section “Diagnostics — In-Clinic” with status dropdown and “Update status” button; section “Report & advice” with file upload, Vet advice, Next steps, “Save report & advice” button. POST actions `UpdateDiagnosticStatus` and `UploadReport`.
- **Fix:** VetPortalController typo corrected: `Deserialize<List<VitalEntryDto>>>(` → `Deserialize<List<VitalEntryDto>>(`.
