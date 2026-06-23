---
applyTo: "frontend/src/**"
description: "Use when editing navigation/menu visibility. Enforce required group ordering, collapsed defaults, implemented-only visibility, and feature-flag controlled future items."
---

# Menu Order and Visibility Rules

## Default Behavior
- Sidebar collapsed by default
- Menu groups collapsed by default
- Only implemented modules visible by default
- Future modules hidden unless enabled by feature flags

## Required Group Item Order
Use exact order from the remaining modules prompt for:
- Sales
- Service
- Activities
- Documents
- Configuration
- Reporting
- Integrations
- AI & Copilot

## Implementation Notes
- Preserve existing navigation architecture and permission-driven visibility
- Add implemented items in correct position; do not reorder already-correct entries
- If a required item is not implemented, keep it hidden unless feature flag enables it
