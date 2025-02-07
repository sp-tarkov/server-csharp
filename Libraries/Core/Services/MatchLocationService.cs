using System.Text.Json.Serialization;
using SptCommon.Annotations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class MatchLocationService
{
    protected Dictionary<string, MatchGroup> _locations = new();

    /// <summary>
    ///     DisbandRaidGroup
    /// </summary>
    /// <param name="request"></param>
    public void DeleteGroup(DeleteGroupRequest request)
    {
        // Find group by id by iterating over all locations and looking for it by groupId
        foreach (var locationKvP in _locations)
        {
            var matchingGroup = _locations[locationKvP.Key]
                .Groups.FirstOrDefault(groupKvP => groupKvP == request.GroupId);
            if (matchingGroup != null)
            {
                _locations[locationKvP.Key].Groups.Remove(request.GroupId);
                return;
            }
        }
    }

    public class MatchGroup
    {
        [JsonPropertyName("groups")]
        public List<string> Groups
        {
            get;
            set;
        }
    }

    public class DeleteGroupRequest
    {
        [JsonPropertyName("groupId")]
        public string GroupId
        {
            get;
            set;
        }
    }
}
