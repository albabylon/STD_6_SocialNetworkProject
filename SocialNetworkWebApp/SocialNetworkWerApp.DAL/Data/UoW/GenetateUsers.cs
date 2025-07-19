using SocialNetworkWebApp.Models.Users;
using System;
using System.Collections.Generic;

namespace SocialNetworkWebApp.Data.UoW
{
    public class GenetateUsers
    {
        public readonly string[] maleNames = ["Александро", "Борис", "Василий", "Игорь", "Даниил", "Сергей", "Евгений", "Алексей", "Геогрий", "Валентин"];
        public readonly string[] femaleNames = ["Анна", "Мария", "Станислава", "Елена"];
        public readonly string[] lastNames = ["Тестов", "Титов", "Потапов", "Джабаев", "Иванов"];

        public List<User> Populate(int count)
        {
            var users = new List<User>();
            for (int i = 1; i < count; i++)
            {
                string firstName;
                string image = "https://thispersondoesnotexist.tools/img/male/thispersondoesnotexist-1.jpg";
                var rand = new Random();

                var male = rand.Next(1, 2) == 1;

                var lastName = lastNames[rand.Next(0, lastNames.Length - 1)];
                if (male)
                {
                    firstName = maleNames[rand.Next(0, maleNames.Length - 1)];
                    image = $"https://thispersondoesnotexist.tools/img/male/thispersondoesnotexist-{i}.jpg";
                }
                else
                {
                    lastName = lastName + "a";
                    firstName = femaleNames[rand.Next(0, femaleNames.Length - 1)];
                    image = $"https://thispersondoesnotexist.tools/img/female/thispersondoesnotexist-{i}.jpg";
                }

                var item = new User()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    BirthDate = DateTime.Now.AddDays(-rand.Next(1, (DateTime.Now - DateTime.Now.AddYears(-25)).Days)),
                    Email = "test" + rand.Next(0, 1204) + "@test.com",
                };

                item.UserName = item.Email;
                item.Image = image;

                users.Add(item);
            }

            return users;
        }
    }
}
