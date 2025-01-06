using Core.Models.Eft.Common;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Profile;

namespace Core.Callbacks;

public class DialogCallbacks
{
    public GetBodyResponseData<GetFriendListDataResponse> GetFriendList(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<ChatServer>> GetChatServerList(string url, GetChatServerListRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<DialogueInfo>> GetMailDialogList(string url, GetMailDialogListRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetMailDialogViewResponseData> GetMailDialogView(string url, GetMailDialogViewRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> GetMailDialogInfo(string url, GetMailDialogInfoRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> RemoveDialog(string url, RemoveDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> PinDialog(string url, PinDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<List<object>> UnpinDialog(string url, PinDialogRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public GetBodyResponseData<List<object>> SetRead(string url, SetDialogReadRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetAllAttachmentsResponse> GetAllAttachments(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> ListOutbox(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> ListInbox(string url, EmptyRequestData info, string sessionID)
    {
        
    }

    public NullResponseData SendFriendRequest(string url, FriendRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<int> SendMessage(string url, SendMessageRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public bool Update()
    {
        throw new NotImplementedException();
    }
}