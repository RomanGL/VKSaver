using OneTeam.SDK.VK.Models.Groups;
using OneTeam.SDK.VK.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Design
{
    public sealed class UserCommViewModelDesign
    {
        public VKUser[] Friends
        {
            get
            {
                return new VKUser[]
                {
                    new VKUser
                    {
                        FirstName = "Павел",
                        LastName = "Дуров",
                        Online = true
                    },
                    new VKUser
                    {
                        FirstName = "Роман",
                        LastName = "Гладких",
                        Online = false
                    }
                };
            }
        }

        public VKUserList[] FriendsLists
        {
            get
            {
                return new VKUserList[]
                {
                    new VKUserList { Name = "Друзья" },
                    new VKUserList { Name = "Коллеги" },
                    new VKUserList { Name = "Родственники" }
                };
            }
        }

        public VKGroup[] Groups
        {
            get
            {
                return new VKGroup[]
                {
                    new VKGroup { Name = "ВКачай" },
                    new VKGroup { Name = "OneVK" },
                    new VKGroup { Name = "Overwatch" },
                    new VKGroup { Name = "Overwatch watch watch over watch overwatch" }
                };
            }
        }
    }
}
