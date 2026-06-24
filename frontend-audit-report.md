# Frontend Audit Report

## Scope
- Sidebar initial/default behavior
- Sidebar group toggle behavior
- Route/navigation integrity for visible menu items
- List filter popup behavior across audited list pages
- Frontend lint, unit test, and build quality gates
- Playwright screenshot capture for required states

## Sidebar Issues Found
- Sidebar default state was collapsed on first load.
- Group expansion state previously auto-expanded active groups and did not enforce collapsed-by-default groups.

## Sidebar Fixes Applied
- Added versioned menu state infrastructure with reset behavior:
  - `crm-menu-state-v3`
  - default sidebar = expanded
  - default expanded groups = []
  - stale all-groups-expanded localStorage reset
- Persisted sidebar collapsed preference and expanded groups via shared helpers.
- Updated sidebar expansion logic to toggle only targeted group and never auto-expand all groups on load.

## Menu Group Issues Found
- Group ordering did not match required canonical order (Documents/Activities and Administration/Security/Configuration ordering mismatch).

## Menu Group Fixes Applied
- Updated group order to:
  1. Dashboard
  2. Customers
  3. Sales
  4. Marketing
  5. Service
  6. Projects
  7. Documents
  8. Activities
  9. Finance
  10. Reporting
  11. Administration
  12. Security
  13. Configuration
  14. Data Management
  15. Audit
  16. Integrations
  17. AI & Copilot
  18. Personal

## Navigation Issues Found
- None found in audited visible menu navigation path.

## Navigation Fixes Applied
- Added stronger unit coverage for canonical ordering and implemented-default visibility.
- Added Playwright navigation sweep for visible child items with URL change, refresh, back/forward validation.

## Filter Issues Found
- Shared grid filter actions were missing when no page-specific filter panel existed.
- A shared query-state synchronization loop caused route instability on heavy list pages (`/leads` and `/opportunities`) during filter interactions.

## Filter Fixes Applied
- Updated shared grid so Apply/Clear/Cancel actions are always present in filter surface.
- Fixed shared query-state synchronization behavior in `useListQueryState` to prevent redundant state churn and render loops.
- Added Playwright filter coverage and screenshot capture for audited routes.

## Routes Fixed
- No route dead-link fixes were required during this pass.
- Route inventory generated at `tests/reports/route-inventory.md`.

## Components Fixed
- `frontend/src/layout/components/AppShell.tsx`
- `frontend/src/layout/components/Sidebar.tsx`
- `frontend/src/layout/navigation.tsx`
- `frontend/src/layout/menuState.ts` (new)
- `frontend/src/components/grid/DenseDataGrid.tsx`
- `frontend/src/test/navigation.spec.ts`
- `tests/e2e/navigation-and-filters.spec.js`
- `tests/scripts/generate-route-inventory.mjs` (new)

## Screenshots Captured
- `tests/reports/screenshots/sidebar-initial-expanded-groups-collapsed.png`
- `tests/reports/screenshots/customers-group-expanded.png`
- `tests/reports/screenshots/sales-group-expanded.png`
- `tests/reports/screenshots/accounts-filter-open.png`
- `tests/reports/screenshots/products-filter-open.png`
- `tests/reports/screenshots/quotes-filter-open.png`
- `tests/reports/screenshots/orders-filter-open.png`
- `tests/reports/screenshots/invoices-filter-open.png`
- `tests/reports/screenshots/cases-filter-open.png`

Missing required screenshots in current run:
- None.

## Tests Added/Updated
- Unit:
  - `frontend/src/test/navigation.spec.ts`
- E2E:
  - `tests/e2e/navigation-and-filters.spec.js`

## Final Test Status
- `npm run lint`: PASS
- `npm run test`: PASS
- `npm run build`: PASS
- `npm run test:e2e`: PASS (10/10)

## Final Status
PASS

---

## Global Subgrid Modal Refactor Pass

### Scope
- Remove inline child create/edit forms from parent tabs and child record pages.
- Enforce shared subgrid pattern with list-only surface and Add New action.
- Enforce modal add/edit and confirm-delete workflow for child records.
- Refresh only affected subgrid after create/update/delete.

### Shared Subgrid Components Added
- `frontend/src/components/subgrid/RelatedRecordsSubgrid.tsx`
- `frontend/src/components/subgrid/SubgridCommandBar.tsx`
- `frontend/src/components/subgrid/SubgridDataGrid.tsx`
- `frontend/src/components/subgrid/SubgridModalForm.tsx`
- `frontend/src/components/subgrid/SubgridRowActions.tsx`
- `frontend/src/components/subgrid/SubgridEmptyState.tsx`
- `frontend/src/components/subgrid/SubgridDeleteConfirmDialog.tsx`
- `frontend/src/components/subgrid/Subgrid.module.css`

### Subgrids Refactored To Modal Pattern
- `frontend/src/opportunities/OpportunityRelatedPanels.tsx`
  - Products, Competitors, Activities
- `frontend/src/contacts/ContactRelatedPanels.tsx`
  - Communications, Interaction History
- `frontend/src/leads/LeadRelatedPanels.tsx`
  - Activities
- `frontend/src/sales/QuoteLinesPage.tsx`
  - Quote Lines
- `frontend/src/sales/OrderLinesPage.tsx`
  - Order Lines
- `frontend/src/sales/InvoiceLinesPage.tsx`
  - Invoice Lines
- `frontend/src/sales/ProductPricingPages.tsx`
  - Price List Items, Bundle Items
- `frontend/src/service/CaseCommentsPage.tsx`
  - Case Comments
- `frontend/src/documents/DocumentVersionsPage.tsx`
  - Document Versions

### Behavior Changes Confirmed
- No inline subgrid add/edit forms remain in the refactored modules.
- Add and Edit open modal forms.
- Delete uses confirmation dialog.
- Parent context remains automatic and non-editable (route-bound parent id usage).
- Success notifications shown after create/update/delete.
- Subgrid refresh occurs after mutation without full parent reload.

### Opportunity Products Specific Outcome
- Opportunity Products no longer render inline add form.
- Products tab now presents list-only subgrid with Add Product action.
- Add/Edit opens modal with product lookup, product name fallback, quantity, price, discount percent, discount amount, tax amount, description, and sort order.

### Verification Status
- `npm run lint`: PASS
- `npm run test`: PASS
- `npm run build`: PASS
- `npm run test:e2e`: PASS (10/10)

### Final Status
PASS

---

## Global List Filter and Column Chooser Stabilization Pass

### Scope
- Shared list column chooser persistence and reset behavior
- Centralized entity-level column and filter registry
- Generic EntityListPage dynamic filter rendering
- Runtime query param hygiene (non-empty params only)
- Date range filter serialization to ISO format

### Issues Found
- Shared column chooser behavior was not centrally configured per entity.
- Generic EntityListPage did not consume centralized filter metadata.
- New shared grid/list scaffolding existed but was not wired into live runtime paths.

### Fixes Applied
- Added centralized entity column and filter registry used by major list entities.
- Added shared column visibility menu and chooser popover components.
- Upgraded shared grid to support per-entity persistent visible columns, required columns, and reset-to-default.
- Wired generic EntityListPage to dynamic filter panel generation (lookup, boolean, text, date-range).
- Ensured API requests send only non-empty filter params.
- Ensured date range fields are converted to ISO datetime values before API calls.

### Additional Components Updated
- `frontend/src/components/grid/EntityColumnRegistry.ts` (new)
- `frontend/src/components/grid/ColumnVisibilityMenu.tsx` (new)
- `frontend/src/components/grid/ColumnChooserPopover.tsx` (new)
- `frontend/src/components/grid/EntityDataGrid.tsx` (new)
- `frontend/src/components/grid/entityListSchema.ts` (new)
- `frontend/src/components/grid/DenseDataGrid.tsx`
- `frontend/src/components/crud/EntityListPage.tsx`
- `frontend/src/components/crud/adminConfig.tsx`
- `frontend/src/types/models.ts`

### Verification Status
- `npm run lint`: PASS
- `npm run test`: PASS
- `npm run build`: PASS
- `npm run test:e2e`: PASS (10/10)

### Final Status
PASS
