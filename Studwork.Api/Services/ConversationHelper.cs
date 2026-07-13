using Studwork.Api.Entities;



namespace Studwork.Api.Services;



public static class ConversationHelper

{

    public static (int user1Id, int user2Id) NormalizePair(int userA, int userB) =>

        userA < userB ? (userA, userB) : (userB, userA);



    public static User GetOtherUser(Conversation conversation, int currentUserId) =>

        conversation.User1Id == currentUserId ? conversation.User2 : conversation.User1;

}


