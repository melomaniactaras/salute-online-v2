﻿using System;
using SaluteOnline.API.DTO.User;
using SaluteOnline.Shared.Common;

namespace SaluteOnline.API.DTO.Club
{
    public class MembershipRequestDto
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public bool SelectedFromExisting { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public MembershipRequestStatus Status { get; set; }
        public UserMainInfoDto UserInfo { get; set; }
    }
}
