using ModernDev.InTouch;

namespace VKSaver.Design
{
    public sealed class UserCommViewModelDesign
    {
        public User[] Friends
        {
            get
            {
                return new User[]
                {
                    new User
                    {
                        FirstName = "Павел",
                        LastName = "Дуров",
                        Online = true
                    },
                    new User
                    {
                        FirstName = "Роман",
                        LastName = "Гладких",
                        Online = false
                    }
                };
            }
        }

        public FriendsList[] FriendsLists
        {
            get
            {
                return new FriendsList[]
                {
                    new FriendsList { Name = "Друзья" },
                    new FriendsList { Name = "Коллеги" },
                    new FriendsList { Name = "Родственники" }
                };
            }
        }

        public Group[] Groups
        {
            get
            {
                return new Group[]
                {
                    new Group { Name = "ВКачай" },
                    new Group { Name = "OneVK" },
                    new Group { Name = "Overwatch" },
                    new Group { Name = "Overwatch watch watch over watch overwatch" }
                };
            }
        }
    }
}
