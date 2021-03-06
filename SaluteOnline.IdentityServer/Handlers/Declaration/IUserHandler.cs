﻿using System.Threading.Tasks;
using SaluteOnline.Shared.Events;

namespace SaluteOnline.IdentityServer.Handlers.Declaration
{
    public interface IUserHandler
    {
        ValueTask<bool> HandleUserCreated(UserCreated data);
        ValueTask<bool> HandleUserRoleChanged(UserRoleChangeEvent data);
    }
}