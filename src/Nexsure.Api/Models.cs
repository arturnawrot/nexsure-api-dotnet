using System.Text.Json;

namespace Nexsure.Api.Models;

/// <summary>
/// Shared response models. Property names match the Nexsure API's PascalCase JSON verbatim.
/// Loosely-typed nested collections are exposed as <see cref="JsonElement"/> for navigation.
/// </summary>
public sealed record Address
{
    public string? AddressType { get; init; }
    public string? StreetAddress1 { get; init; }
    public string? StreetAddress2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public JsonElement? Country { get; init; }
    public string? InternationalAddress { get; init; }
}

public sealed record PersonPhone
{
    public int? PhoneID { get; init; }
    public string? PhoneNumber { get; init; }

    // Note: the API spells this field "Extenstion"; kept verbatim to match the wire format.
    public string? Extenstion { get; init; }
    public string? Description { get; init; }
    public bool? IsPrimaryPhone { get; init; }
    public int? PersonPhoneID { get; init; }
    public string? PhoneType { get; init; }
}

public sealed record Person
{
    public int? PersonID { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleInitial { get; init; }
    public string? LastName { get; init; }
    public string? GoesBy { get; init; }
    public string? Prefix { get; init; }
    public string? Suffix { get; init; }
    public string? Title { get; init; }
    public bool? IsEmployee { get; init; }
    public IReadOnlyList<PersonPhone> Phone { get; init; } = [];
    public string? DateOfBirth { get; init; }
    public string? SSN { get; init; }
    public string? MaritalStatus { get; init; }
    public string? Sex { get; init; }
    public string? DriversLicenseNo { get; init; }
    public string? LicensedState { get; init; }
    public string? LicensedDt { get; init; }
    public IReadOnlyList<string> Email { get; init; } = [];
    public string? CreatedDt { get; init; }
    public string? LastModifiedDt { get; init; }
    public string? EnterpriseApplicationID { get; init; }
    public string? ReplicationID { get; init; }
}

public sealed record Location
{
    public int? LocationID { get; init; }
    public string? LocationTypeCode { get; init; }
    public string? LocationName { get; init; }
    public IReadOnlyList<Address> Address { get; init; } = [];
    public bool? IsPrimaryLocation { get; init; }
    public bool? IsBillingLocation { get; init; }
    public string? LastModifiedDt { get; init; }
    public string? EnterpriseApplicationID { get; init; }
    public string? ReplicationID { get; init; }
}

public sealed record ClientName
{
    public int? ClientNameID { get; init; }
    public string? Name { get; init; }
    public string? LegalEntityCd { get; init; }
    public string? Status { get; init; }
    public bool? IsPrimaryName { get; init; }
    public bool? IsDBAName { get; init; }
    public string? URL { get; init; }
    public int? NoEmployees { get; init; }
    public string? FEIN { get; init; }
    public string? SSN { get; init; }
    public string? GrossReceiptsBasis { get; init; }
    public string? GrossReceipts { get; init; }
    public string? ClientLongName { get; init; }
    public string? Notes { get; init; }
    public string? NatureOfBusiness { get; init; }
    public string? YearStarted { get; init; }
    public string? CreatedDt { get; init; }
    public string? LastModifiedDt { get; init; }
    public string? EnterpriseApplicationID { get; init; }
    public string? ReplicationID { get; init; }
}

public sealed record Client
{
    public int? ClientID { get; init; }
    public string? EnterpriseCode { get; init; }
    public bool? IsActive { get; init; }
    public string? ClientType { get; init; }
    public string? ClientStage { get; init; }
    public string? ClientSince { get; init; }
    public string? CreatedDt { get; init; }
    public string? LastModifiedDt { get; init; }
    public Person? LastModifiedBy { get; init; }
    public IReadOnlyList<ClientName> ClientNames { get; init; } = [];
    public IReadOnlyList<JsonElement> Contacts { get; init; } = [];
    public IReadOnlyList<Location> Locations { get; init; } = [];
    public IReadOnlyList<JsonElement> RelatedAccounts { get; init; } = [];
    public IReadOnlyList<JsonElement> Assignments { get; init; } = [];
    public IReadOnlyList<JsonElement> Classifieds { get; init; } = [];
    public IReadOnlyList<JsonElement> Actions { get; init; } = [];
    public IReadOnlyList<JsonElement> Policies { get; init; } = [];
    public string? EnterpriseApplicationID { get; init; }
    public string? ReplicationID { get; init; }
}

public sealed record Premium
{
    public string? Estimated { get; init; }
    public string? Annualized { get; init; }
    public string? Billed { get; init; }
}

public sealed record Policy
{
    public int? PolicyID { get; init; }
    public int? ClientID { get; init; }
    public string? PolicyNumber { get; init; }
    public string? EffDate { get; init; }
    public string? ExpDate { get; init; }
    public string? CovEffDate { get; init; }
    public string? CovExpDate { get; init; }
    public string? OriginationDate { get; init; }
    public string? PolicyMode { get; init; }
    public string? PolicyStage { get; init; }
    public string? PolicyStatus { get; init; }
    public string? PolicyType { get; init; }
    public string? PolicyDescription { get; init; }
    public bool? IsNonRenewing { get; init; }
    public bool? IsInHistory { get; init; }
    public string? HistoryNotes { get; init; }
    public Premium? Premiums { get; init; }
    public IReadOnlyList<JsonElement> Assignments { get; init; } = [];
    public IReadOnlyList<JsonElement> Actions { get; init; } = [];
    public IReadOnlyList<JsonElement> Classifieds { get; init; } = [];
    public string? CreatedDt { get; init; }
    public string? LastModifiedDt { get; init; }
    public string? EnterpriseApplicationID { get; init; }
    public string? ReplicationID { get; init; }
}

/// <summary>The <c>AssignedTo</c> block on an <see cref="Attachment"/>.</summary>
public sealed record AttachmentAssignment
{
    public string? AssignmentType { get; init; }
    public string? AssignmentReplicationID { get; init; }
    public int? AssignmentTypeID { get; init; }
}

public sealed record Attachment
{
    public int? AttachmentID { get; init; }
    public AttachmentAssignment? AssignedTo { get; init; }
    public string? AttachmentName { get; init; }
    public string? AttachmentDesc { get; init; }
    public string? FileName { get; init; }
    public string? LastModifiedDt { get; init; }
    public int? ActionID { get; init; }
    public string? ReplicationID { get; init; }
}

public sealed record LookupDefinitionValueType
{
    public int? ItemID { get; init; }
    public string? ItemCode { get; init; }
    public string? ItemValue { get; init; }
    public bool? IsSystemType { get; init; }
    public string? CreatedDt { get; init; }
    public string? LastModifiedDt { get; init; }
}

public sealed record LookupDefinitionType
{
    public int? TypeID { get; init; }
    public string? TypeName { get; init; }
    public int? TypeSize { get; init; }
    public IReadOnlyList<LookupDefinitionValueType> DataItem { get; init; } = [];
    public string? CreatedDt { get; init; }
    public string? LastModifiedDt { get; init; }
}

public sealed record LookupCategoryType
{
    public int? CategoryID { get; init; }
    public string? CategoryName { get; init; }
    public bool? IsSystemType { get; init; }
    public IReadOnlyList<LookupDefinitionType> Type { get; init; } = [];
    public string? CreatedDt { get; init; }
}
