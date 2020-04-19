using System;

namespace Game.GameObjects
{
    //public abstract class GameObject
    public class GameObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        protected GameObject()
        {
             Id = Guid.NewGuid();
        }

        public virtual void Interaction(GameObject obj)
        {
            Console.WriteLine("Interaction: {0} => {1}", Name, obj.Name);
        }
    }
}
