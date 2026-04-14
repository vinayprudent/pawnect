# PetParent Web – User Flow

This document describes the **PetParent Web** (PawNect.PetParent.Web) user flows, screens, and API usage.

---

## Overview

| Item | Value |
|------|--------|
| **App** | PawNect.PetParent.Web (ASP.NET Core MVC) |
| **Base URL** | `http://localhost:5100` (when run via `Run-PawNect.ps1`) |
| **API** | PawNect.API at `http://localhost:5000/api` (from `appsettings.json`) |
| **Audience** | Pet owners (Pet Parents) |

---

## Entry & Navigation

| Route | Controller.Action | View | Purpose |
|-------|--------------------|------|---------|
| `/` | Home.Index | Home/Index | Landing / dashboard |
| `/Home/Index` | Home.Index | Home/Index | Same as above |
| `/Pets` | Pets.Index | Pets/Index | List all pets (My Pets) |
| `/Pets/Create` | Pets.Create (GET) | Pets/Create | Add new pet form |
| `/Pets/Details/{id}` | Pets.Details | Pets/Details | Single pet details |
| `/Pets/Edit/{id}` | Pets.Edit (GET/POST) | Pets/Edit | Edit pet form |
| `/Pets/Delete/{id}` | Pets.Delete (GET/POST) | Pets/Delete | Confirm & delete pet |

**Layout:** `_Layout.cshtml` – Nav: Home, My Pets, Add Pet; Account dropdown (Profile, Settings, Logout – placeholders).

---

## Flow 1: Home → My Pets

1. User opens **Home** (`/`).
2. Home shows welcome, “Add Your First Pet”, and three feature cards (Health Records, Appointments, Connect with Vets).
3. User clicks **“View My Pets”** or nav **My Pets** → `Pets/Index`.
4. **Pets/Index** calls API `GET /api/pets`, maps response to `PetViewModel`, shows list (cards).
5. From list, user can: **Details**, **Edit**, **Delete**, or go **Add Pet**.

**API:** `GET http://localhost:5000/api/pets`  
**Response:** `ApiResponse<IEnumerable<PetDto>>` → mapped to `List<PetViewModel>`.

---

## Flow 2: Add Pet

1. User goes to **Add Pet** (nav or Home “Add Your First Pet”) → `Pets/Create` (GET).
2. Form shows: Name, Breed, Species, Date of Birth, Weight, Color, Microchip ID, Profile Image URL, Description.
3. User submits (POST) → `PetsController.Create(CreatePetViewModel)`.
4. MVC serializes to JSON (camelCase) and calls `POST /api/pets` with `ownerId: 1` (TODO: from authenticated user).
5. On success: TempData success, redirect to `Pets/Index`. On failure: show error, return same view.

**API:** `POST http://localhost:5000/api/pets`  
**Body (camelCase):** `name`, `breed`, `species`, `dateOfBirth`, `weightKg`, `color`, `microchipId`, `profileImageUrl`, `description`, `ownerId`.

---

## Flow 3: View Pet Details

1. From **My Pets** list, user clicks **Details** on a pet → `Pets/Details/{id}`.
2. MVC calls `GET /api/pets/{id}`, maps to `PetViewModel`, renders **Pets/Details**.
3. Details view shows full pet info; user can go to **Edit** or **Delete** from here (if linked in view).

**API:** `GET http://localhost:5000/api/pets/{id}`  
**Response:** `ApiResponse<PetDto>` → `PetViewModel`.

---

## Flow 4: Edit Pet

1. User goes to **Edit** from list or details → `Pets/Edit/{id}` (GET).
2. MVC loads pet via `GET /api/pets/{id}`, maps to `EditPetViewModel`, shows form.
3. User changes fields and submits (POST) → `PetsController.Edit(id, EditPetViewModel)`.
4. MVC calls `PUT /api/pets/{id}` with update DTO.
5. On success: TempData success, redirect to `Pets/Details/{id}`. On failure: error message, same view.

**API:**  
- Load: `GET /api/pets/{id}`  
- Save: `PUT http://localhost:5000/api/pets/{id}`  
**Body (camelCase):** `id`, `name`, `breed`, `species`, `dateOfBirth`, `weightKg`, `color`, `microchipId`, `profileImageUrl`, `description`, `status`.

---

## Flow 5: Delete Pet

1. User goes to **Delete** from list or details → `Pets/Delete/{id}` (GET).
2. MVC loads pet via `GET /api/pets/{id}`, shows confirmation view.
3. User confirms (POST) → `PetsController.DeleteConfirmed(id)`.
4. MVC calls `DELETE /api/pets/{id}`.
5. On success/failure: TempData message, redirect to `Pets/Index`.

**API:** `DELETE http://localhost:5000/api/pets/{id}`.

---

## Data Flow Summary

```
Browser                    PetParent.Web (MVC)              PawNect.API
   |                              |                                |
   |  GET /                        |  (no API call)                 |
   |  -------------------------->  |  Home/Index                    |
   |  <--------------------------  |  View: Home/Index               |
   |                              |                                |
   |  GET /Pets                    |  GET /api/pets                 |
   |  -------------------------->  |  ----------------------------> |
   |  <--------------------------  |  <---------------------------- |
   |  View: Pets/Index (list)       |  ApiResponse → PetViewModel     |
   |                              |                                |
   |  GET /Pets/Create             |  (no API call)                 |
   |  -------------------------->  |  View: Create form              |
   |  POST /Pets/Create            |  POST /api/pets                 |
   |  -------------------------->  |  ----------------------------> |
   |  <--------------------------  |  <---------------------------- |
   |  Redirect to /Pets            |                                |
   |                              |                                |
   |  GET /Pets/Details/5          |  GET /api/pets/5                |
   |  -------------------------->  |  ----------------------------> |
   |  <--------------------------  |  <---------------------------- |
   |  View: Details                |                                |
   |                              |                                |
   |  GET /Pets/Edit/5             |  GET /api/pets/5                |
   |  POST /Pets/Edit/5            |  PUT /api/pets/5                |
   |  GET /Pets/Delete/5           |  GET /api/pets/5                |
   |  POST /Pets/Delete/5          |  DELETE /api/pets/5             |
```

---

## Configuration

- **API base URL:** `appsettings.json` → `ApiSettings:BaseUrl` = `http://localhost:5000/api`.
- **PetParent Web URL:** Set at run time (e.g. `dotnet run --urls http://localhost:5100` or via `Run-PawNect.ps1`).

---

## Current Gaps / TODOs

| Item | Location | Note |
|------|----------|------|
| **Owner ID** | PetsController.Create | `ownerId = 1` hardcoded; should come from authenticated user. |
| **Auth** | App-wide | No login/register flow; Account menu (Profile, Settings, Logout) are placeholders. |
| **My Pets filter** | Pets.Index | Uses `GET /api/pets` (all pets). For “my pets” only, use `GET /api/pets/owner/{ownerId}` when user is known. |
| **Health / Appointments** | Home & nav | Home mentions Health/Appointments/Connect with Vets; no controllers/views yet. |
| **Privacy** | HomeController | `Privacy()` action exists but no `Views/Home/Privacy.cshtml`; remove action or add view. |

---

## Quick Test Checklist

- [ ] Open `http://localhost:5100` → Home.
- [ ] Nav to My Pets → list (empty or existing pets).
- [ ] Add Pet → fill form → submit → redirect to list with success.
- [ ] Details → open a pet → see full info.
- [ ] Edit → change fields → save → redirect to Details.
- [ ] Delete → confirm → redirect to list.
- [ ] Ensure API is running at `http://localhost:5000` and CORS allows the PetParent origin if needed.

---

*Document for PawNect PetParent Web flow. Update when adding auth, owner-scoped pets, or Health/Appointments.*
