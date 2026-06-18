# IMPORTANT - EXISTING SOLUTION ARCHITECTURE
The Administration and Authentication modules have already been implemented.

Before generating any new code, analyze the existing solution and follow the established architecture.

Do not recreate existing infrastructure.

Reuse:

- Authentication
- Authorization
- Roles
- Permissions
- Teams
- Departments
- Audit Logs
- Lookup Categories
- Lookup Values
- Base Entity hierarchy
- DbContext
- Middleware
- Permission attributes
- Global exception handling
- Validation framework
- Existing UI layout

---

# Mandatory Entity Inheritance
All CRM entities must use the shared inheritance model already implemented in the solution.

Never duplicate audit fields on entities.

Use the existing base classes:

```csharp
public abstract class BaseEntity
{
	public Guid Id { get; set; }

	public DateTime CreatedAt { get; set; }

	public Guid? CreatedById { get; set; }

	public DateTime? UpdatedAt { get; set; }

	public Guid? UpdatedById { get; set; }

	public bool IsDeleted { get; set; }

	public string? TenantId { get; set; }
}
```

```csharp
public abstract class ActivatableEntity : BaseEntity
{
	public bool IsActive { get; set; } = true;
}
```

```csharp
public abstract class OwnedEntity : ActivatableEntity
{
	public Guid? OwnerUserId { get; set; }

	public Guid? OwnerTeamId { get; set; }
}
```

Use these inheritance rules:

```csharp
public class Account : OwnedEntity
{
}
```

```csharp
public class Contact : OwnedEntity
{
}
```

```csharp
public class AccountAddress : ActivatableEntity
{
}
```

```csharp
public class CustomerProfile : BaseEntity
{
}
```

```csharp
public class AccountRelationship : ActivatableEntity
{
}
```

```csharp
public class AccountActivity : BaseEntity
{
}
```

Do NOT add:

- Id
- CreatedAt
- CreatedById
- UpdatedAt
- UpdatedById
- IsDeleted
- TenantId
- IsActive
- OwnerUserId
- OwnerTeamId

to any entity that already inherits them.

Only define fields that are unique to that entity.

---

# Audit Requirements
Audit fields are automatically populated by the existing SaveChanges interceptor.

Controllers and services must never manually set:

- CreatedAt
- CreatedById
- UpdatedAt
- UpdatedById

The existing audit logging framework must automatically track:

- Create
- Update
- Delete
- Ownership changes
- Status changes

for all Account Management entities.

---

# Soft Delete Requirements
All entities inheriting BaseEntity must use soft delete.

Delete endpoints must:

- Set IsDeleted = true
- Never physically remove records

Existing global query filters should automatically exclude deleted records.

Do not implement custom filtering if global query filters already exist.

---

# Existing Lookup Architecture
Do not create new lookup tables.

Use the existing:

- LookupCategory
- LookupValue

entities from the Administration module.

All dropdowns, reference data, statuses, priorities, account types, industries, relationship types, activity types, and contact roles must use LookupValue references.

---

# Existing UI Architecture
Use the existing CRM layout already implemented.

Continue using:

- Dense enterprise layout
- Centered global search
- Compact breadcrumbs
- Compact page headers
- Title + actions on same row
- Fluent UI v9
- Route-based CRUD pages
- No modal forms

Do not introduce a different layout pattern.

All new pages must match the existing Administration pages.

---

# Account Management Entity Definitions
Use the following fields and exclude inherited fields from each entity.

Account Fields (excluding inherited fields)

- AccountNumber
- Name
- LegalName
- TradingName
- AccountTypeId
- IndustryId
- OwnershipTypeId
- CustomerStatusId
- CustomerSegmentId
- Website
- MainPhone
- AlternatePhone
- Email
- Fax
- TaxNumber
- RegistrationNumber
- AnnualRevenue
- NumberOfEmployees
- Description
- ParentAccountId
- PrimaryContactId

Contact Fields (excluding inherited fields)

- AccountId
- ContactRoleId
- SalutationId
- FirstName
- MiddleName
- LastName
- JobTitle
- DepartmentName
- Email
- MobilePhone
- WorkPhone
- Extension
- PreferredCommunicationId
- IsPrimaryContact
- DateOfBirth
- Notes

AccountAddress Fields (excluding inherited fields)

- AccountId
- AddressTypeId
- AttentionTo
- Line1
- Line2
- Landmark
- City
- StateProvince
- PostalCode
- CountryId
- Latitude
- Longitude
- IsPrimary
- IsBilling
- IsShipping

CustomerProfile Fields (excluding inherited fields)

- AccountId
- CreditLimit
- PaymentTermsId
- PreferredCurrencyId
- PreferredLanguageId
- TimeZoneId
- RiskRatingId
- LifecycleStageId
- CustomerSince
- LastReviewDate
- NextReviewDate
- ChurnRiskScore
- SatisfactionScore
- Notes

AccountRelationship Fields (excluding inherited fields)

- SourceAccountId
- TargetAccountId
- RelationshipTypeId
- StartDate
- EndDate
- StrengthId
- Notes

AccountActivity Fields (excluding inherited fields)

- AccountId
- ContactId
- ActivityTypeId
- Subject
- Description
- ActivityDate
- DueDate
- PriorityId
- StatusId
- OutcomeId
- AssignedToUserId
- RelatedEntityType
- RelatedEntityId
- IsPrivate
- FollowUpRequired
- FollowUpDate

Before generating code:

1. Inspect the existing Administration module.
2. Reuse all existing infrastructure.
3. Reuse the BaseEntity hierarchy.
4. Reuse audit logging.
5. Reuse lookup architecture.
6. Reuse permission architecture.
7. Reuse the existing CRM UI layout.

Generate only the new Account Management functionality required for this module.
