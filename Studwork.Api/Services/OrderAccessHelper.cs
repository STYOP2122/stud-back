using Studwork.Api.Entities;



namespace Studwork.Api.Services;



public static class OrderAccessHelper

{

    public static bool CanView(Order order, int? userId, UserRole? role)

    {

        if (!order.IsPrivate) return true;

        if (userId == null) return false;

        if (role == UserRole.Admin) return true;

        if (order.CustomerId == userId) return true;

        if (order.InvitedExecutorId == userId) return true;

        if (order.ExecutorId == userId) return true;

        return false;

    }



    public static bool CanBid(Order order, int userId, UserRole role)

    {

        if (role != UserRole.Executor) return false;

        if (order.Status != OrderStatus.Open) return false;

        if (order.CustomerId == userId) return false;

        if (order.IsPrivate && order.InvitedExecutorId != userId) return false;

        return true;

    }



    public static bool CanAccessOrderChat(Order order, int userId, UserRole role)

    {

        if (role == UserRole.Admin) return true;

        return order.CustomerId == userId || order.ExecutorId == userId;

    }



    public static bool CanUploadOrderFiles(Order order, int userId, UserRole role)

    {

        if (role == UserRole.Admin) return true;

        if (order.CustomerId == userId) return true;

        if (order.ExecutorId == userId) return true;

        if (order.IsPrivate && order.InvitedExecutorId == userId && order.Status == OrderStatus.Open)

            return true;

        return false;

    }

}


