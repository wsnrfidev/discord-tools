using System.Collections.Generic;
using System;

namespace DiscordTools
{
    public class MemberInfoData
    {
        public ulong UserId { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public string DisplayName { get; set; }
        public bool IsBot { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? JoinedAt { get; set; }
        public List<string> Roles { get; set; }
        public string AvatarUrl { get; set; }
    }
}
