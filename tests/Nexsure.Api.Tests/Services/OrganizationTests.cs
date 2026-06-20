using Nexsure.Api.Credentials;
using Nexsure.Api.Services.Organization;
using Xunit;

namespace Nexsure.Api.Tests.Services;

public class OrganizationTests
{
    [Fact]
    public async Task SearchBranch_NoArgs_ToleratesStringBooleansAndNumericIds()
    {
        // The live API returns IsActive as the string "true"/"false" and BranchID as a
        // quoted string (e.g. "15") rather than a JSON number (see demo.ipynb). Mix both a
        // string and a numeric id here to prove the int converter tolerates either.
        var client = TestSupport.ClientWithResponse(
            """
            {
              "Branches": {
                "Branch": [
                  { "BranchID": "1", "BranchName": "Insurekore Ltd.", "IsActive": "true" },
                  { "BranchID": 2, "BranchName": "Tech-Insure Consulting Ltd.", "IsActive": "false" }
                ]
              }
            }
            """,
            out var handler,
            new NexsureCredentials("tok"));

        var result = await new SearchBranchByBranchName(client).ExecuteAsync();

        // No branch_name supplied -> only returnContentType in the form body.
        Assert.DoesNotContain("branchName", handler.LastRequestBody);

        Assert.Equal(2, result.Branch.Count);
        Assert.Equal(1, result.Branch[0].BranchID);
        Assert.True(result.Branch[0].IsActive);
        Assert.False(result.Branch[1].IsActive);
    }

    [Fact]
    public async Task SearchDepartment_DefaultsWildcard_AndParsesStringIsActive()
    {
        var client = TestSupport.ClientWithResponse(
            """
            { "Departments": { "Department": [
              { "DepartmentID": "1", "DepartmentName": "CA$", "IsActive": "true" }
            ] } }
            """,
            out var handler,
            new NexsureCredentials("tok"));

        var result = await new SearchDepartmentByName(client).ExecuteAsync();

        Assert.Contains("departmentName=%25%25", handler.LastRequestBody); // "%%" url-encoded
        var dept = Assert.Single(result.Department);
        Assert.Equal("1", dept.DepartmentID);
        Assert.Equal("true", dept.IsActive);
    }
}
