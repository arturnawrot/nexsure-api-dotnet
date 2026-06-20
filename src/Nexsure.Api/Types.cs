namespace Nexsure.Api.Types;

/// <summary>Match strategy for search endpoints. Serialized as its integer value.</summary>
public enum SearchType
{
    ExactMatch = 0,
    Contains = 1,
    StartsWith = 2,
}

/// <summary>The member name equals the API string value for these enums.</summary>
public enum AddressType
{
    Physical,
    Mailing,
}

public enum ClientType
{
    Personal,
    Commercial,
}

public enum ClientStage
{
    Prospect,
    Client,
    Suspect,
}

public enum LegalEntity
{
    Individual,
    Partnership,
    Corporation,
    JointVenture,
    LLC,
    NPCorp,
    Other,
}

public enum PolicyMode
{
    New,
    Renew,
    RenewCo,
    NewOnExisting,
}

public enum PolicyStage
{
    Marketing,
    Policy,
    Endorsement,
    Cancellation,
    Audit,
    Edit,
    Claim,
    Opportunity,
}

public enum PolicyType
{
    Package,
    Monoline,
}

public enum LOBType
{
    Personal,
    Commercial,
    Benefits,
    Bond,
    FinancialServices,
}

public enum AttachmentAssignmentType
{
    Client,
    Policy,
    RetailAgent,
}

/// <summary>
/// Lookup-data categories. Several values contain spaces, so use
/// <see cref="LookupCategoryExtensions.ToApiValue"/> to get the wire string.
/// </summary>
public enum LookupCategory
{
    AdditionalInterest,
    Carrier,
    Client,
    DocumentIntegration,
    FinancialEntity,
    Miscellaneous,
    Organization,
    People,
    Policy,
    PremiumFinanceCompany,
    RetailAgent,
    TaxAuthority,
    Vendor,
}

/// <summary>Maps <see cref="LookupCategory"/> members to their API string values.</summary>
public static class LookupCategoryExtensions
{
    public static string ToApiValue(this LookupCategory category) => category switch
    {
        LookupCategory.AdditionalInterest => "Additional Interest",
        LookupCategory.DocumentIntegration => "Document Integration",
        LookupCategory.FinancialEntity => "Financial Entity",
        LookupCategory.PremiumFinanceCompany => "Premium Finance Company",
        LookupCategory.RetailAgent => "Retail Agent",
        LookupCategory.TaxAuthority => "Tax Authority",
        _ => category.ToString(),
    };
}
