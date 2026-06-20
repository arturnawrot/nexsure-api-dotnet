using System.Text;
using Nexsure.Api.Credentials;
using Nexsure.Api.Services.Attachments;
using Nexsure.Api.Services.Lookup;
using Nexsure.Api.Types;
using Xunit;

namespace Nexsure.Api.Tests.Services;

public class AttachmentAndLookupTests
{
    [Fact]
    public async Task AddAttachment_EncodesBytesAsBase64InQuery_AndParsesAttachment()
    {
        var fileBytes = Encoding.UTF8.GetBytes("hello pdf");
        var expectedB64 = Convert.ToBase64String(fileBytes);

        var client = TestSupport.ClientWithResponse(
            """{ "Attachment": { "AttachmentID": 555, "FileName": "cert.pdf" } }""",
            out var handler,
            new NexsureCredentials("tok"));

        var result = await new AddAttachment(client).ExecuteAsync(new
        {
            policy_id = 777,
            file_name = "cert.pdf",
            file_bytes = fileBytes,
        });

        var query = Uri.UnescapeDataString(handler.LastRequest!.RequestUri!.Query);
        Assert.Contains("attachmentBase64=" + expectedB64, query);
        Assert.Contains("<AssignmentType>Policy</AssignmentType>", query);
        Assert.Contains("<AssignmentTypeID>777</AssignmentTypeID>", query);

        Assert.Equal(555, result.Attachment!.AttachmentID);
        Assert.Equal("cert.pdf", result.Attachment.FileName);
    }

    [Fact]
    public async Task ListLookupManagementValues_MapsSpacedEnumValue_AndParsesNestedTree()
    {
        var client = TestSupport.ClientWithResponse(
            """
            {
              "LookupManagement": {
                "Category": [
                  {
                    "CategoryID": 1,
                    "CategoryName": "Additional Interest",
                    "Type": [
                      { "TypeID": 9, "TypeName": "Kind",
                        "DataItem": [ { "ItemID": 100, "ItemValue": "Bank" } ] }
                    ]
                  }
                ]
              }
            }
            """,
            out var handler,
            new NexsureCredentials("tok"));

        var result = await new ListLookupManagementValues(client)
            .ExecuteAsync(new { category_name = LookupCategory.AdditionalInterest });

        Assert.Contains("categoryName=Additional%20Interest", handler.LastRequest!.RequestUri!.Query);

        var category = Assert.Single(result.Category);
        Assert.Equal("Additional Interest", category.CategoryName);
        var type = Assert.Single(category.Type);
        Assert.Equal("Kind", type.TypeName);
        Assert.Equal("Bank", Assert.Single(type.DataItem).ItemValue);
    }
}
