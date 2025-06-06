﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LetterHeadShared.DTO
{
    public class UserInfo
    {
        public int Id;
        public int MatchCount;
        public string Username;
        public string AvatarUrl;
        public bool IsPremium;
        public bool HasEmail;
        public string FacebookPictureUrl;
        public string Settings;
        public List<int> PowerupCountList;
    }
}
