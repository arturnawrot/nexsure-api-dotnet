using Nexsure.Api.Types;

namespace Nexsure.Api.Inputs;

/// <summary>An address supplied when creating a client.</summary>
public sealed record AddressInput(
    string Street,
    string City,
    string State,
    string ZipCode,
    AddressType AddressType = AddressType.Physical);

/// <summary>A contact supplied when creating a client.</summary>
public sealed record ContactInput(
    string FirstName,
    string LastName,
    bool IsPrimary = true);

/// <summary>A branch/department assignment supplied when creating a client or policy.</summary>
public sealed record AssignmentInput(
    string BranchId,
    string DepartmentId,
    bool IsPrimary = true);
