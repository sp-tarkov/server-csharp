using SptCommon.Annotations;
using Core.Models.Eft.Ragfair;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairRequiredItemsService
{
    public List<RagfairOffer> GetRequiredItemsById(string searchId)
    {
        Console.WriteLine($"actually implement me plz: owo: GetRequiredItemsById");
        return new List<RagfairOffer>();
    }

    public void BuildRequiredItemTable()
    {
        Console.WriteLine($"actually implement me plz: owo: BuildRequiredItemTable");
    }
}
