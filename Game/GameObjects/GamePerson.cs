using System;
using System.Text.Json.Serialization;
using Game.GameObjects;

namespace Game
{
    public abstract class GamePerson : GameObject
    {
        int hp;
        public bool PlayerFriend { get; set; }
        public int HealthPoints
        {
            get
            {
                return hp;
            }
            set
            {
                if (value < 0)
                {
                    hp = 0;
                    Alive = false;
                }
                else
                    hp = value;
            }
        }
        public int Level { get; set; }
        public int Damage { get; set; }
        public bool Alive { get; set; } = true;
        public Map World { get; set; }
        public IWeapon Weapon { get; set; }

        public GamePerson(string name, bool plFriend)
        {
            Name = name;
            HealthPoints = 100;
            Level = 1;
            Damage = 20;
            PlayerFriend = plFriend;
        }

        public void LevelUp()
        {
            Level++;
            HealthPoints += 50;
        }

        public void Hit(GamePerson target)
        {
            if (Alive)
            {
                Random random = new Random();
                target.HealthPoints -= random.Next(Damage - 5, Damage + 6) + (Weapon?.Damage ?? 0);
                if (random.Next(0, 5) == 4)
                    Weapon?.SpecialAttack(target);
                if (target.HealthPoints == 0)
                    LevelUp();
            }
        }

        public void ShowInfo(bool? team = null)
        {
            if (Alive)
                Extensions.ToConsole($"{Name}, hp: {HealthPoints}, dmg/lvl: {Damage}/{Level}, w: {(Weapon == null ? "-" : Weapon.GetType().Name)}"
                    , team != null && !((bool)team ^ PlayerFriend) ? ConsoleColor.Green : ConsoleColor.White);
            else
                Extensions.ToConsole($"{Name} die", ConsoleColor.Red);
        }

        public override void Interaction(GameObject obj)
        {
            base.Interaction(obj);
            if (obj is GamePerson person && person != this && person.PlayerFriend != PlayerFriend)
            {
                Battle newBattle = new Battle(person, this);
                GamePerson winner = newBattle.Fight();
            }
        }

        public abstract Position Move(string direction);
    }
}
