using System.Collections.Generic;

namespace Automata.Engine.Collections
{
    public class Registry<T>: Singleton<Registry<T>>
    {
        protected readonly List<(string Name, T Registrant)> Registrants;
        protected readonly Dictionary<string, int> RegistrantNames;

        public T this[int id] => Registrants[id].Registrant;

        public virtual T this[string name] => Registrants[RegistrantNames[name]].Registrant;

        public Registry()
        {
            Registrants = new List<(string, T)>();
            RegistrantNames = new Dictionary<string, int>();
        }

        public bool Exists(int id) => id < Registrants.Count;
        public bool Exists(string name) => RegistrantNames.ContainsKey(name);

        public int GetID(string name) => RegistrantNames[name];
        public string GetName(int id) => Registrants[id].Name;
    }
}
